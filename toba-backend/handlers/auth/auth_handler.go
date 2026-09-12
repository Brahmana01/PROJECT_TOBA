package auth

import (
	"crypto/rand"
	"encoding/hex"
	"fmt"
	mathrand "math/rand"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"golang.org/x/crypto/bcrypt"
)

// ============================================================
// POST /api/register
// ============================================================
func Register(c *fiber.Ctx) error {
	type RegisterInput struct {
		Name        string `json:"name"`
		Pin         string `json:"pin"`
		CharacterID int    `json:"character_id"`
		DeviceID    string `json:"device_id"`
	}

	input := new(RegisterInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if len(input.Pin) != 6 {
		return c.Status(400).JSON(fiber.Map{"message": "PIN harus 6 digit"})
	}
	if strings.TrimSpace(input.Name) == "" {
		return c.Status(400).JSON(fiber.Map{"message": "Nama tidak boleh kosong"})
	}

	pinHash, err := bcrypt.GenerateFromPassword([]byte(input.Pin), bcrypt.DefaultCost)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal memproses PIN"})
	}

	var lastPlayer models.Player
	config.DB.Order("id DESC").First(&lastPlayer)
	newID := lastPlayer.ID + 1
	userCode := fmt.Sprintf("USR%06d", newID)

	cleanName := strings.ToLower(strings.ReplaceAll(strings.TrimSpace(input.Name), " ", ""))
	mathrand.Seed(time.Now().UnixNano())
	suffix := mathrand.Intn(9000) + 1000
	username := fmt.Sprintf("%s%d", cleanName, suffix)

	for {
		var existing models.Player
		if config.DB.Where("username = ?", username).First(&existing).Error != nil {
			break
		}
		suffix = mathrand.Intn(9000) + 1000
		username = fmt.Sprintf("%s%d", cleanName, suffix)
	}

	player := models.Player{
		UserCode: userCode, Username: username, Name: strings.TrimSpace(input.Name),
		PinHash: string(pinHash), CharacterID: input.CharacterID, TitleID: 1,
		Level: 1, XP: 0, Stars: 0, IsActive: true,
	}

	if err := config.DB.Create(&player).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal membuat akun"})
	}

	var firstLocation models.Location
	if config.DB.Where("level_order = 1 AND is_active = ?", true).First(&firstLocation).Error == nil {
		firstProgress := models.PlayerLocationProgress{PlayerID: player.ID, LocationID: firstLocation.ID, IsUnlocked: true}
		config.DB.Create(&firstProgress)
	}

	token := generateToken(player.ID)
	session := models.PlayerSession{PlayerID: player.ID, Token: token, DeviceID: input.DeviceID, IsActive: true}
	config.DB.Create(&session)

	return c.Status(201).JSON(fiber.Map{
		"message": "Akun berhasil dibuat!", "id": player.ID, "user_code": player.UserCode,
		"username": player.Username, "name": player.Name, "level": player.Level,
		"stars": player.Stars, "token": token,
	})
}

// ============================================================
// POST /api/login
// ============================================================
func Login(c *fiber.Ctx) error {
	type LoginInput struct {
		Username string `json:"username"`
		Pin      string `json:"pin"`
		DeviceID string `json:"device_id"`
	}

	input := new(LoginInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var player models.Player
	if err := config.DB.Where("username = ?", input.Username).First(&player).Error; err != nil {
		return c.Status(401).JSON(fiber.Map{"message": "Username atau PIN salah"})
	}
	if err := bcrypt.CompareHashAndPassword([]byte(player.PinHash), []byte(input.Pin)); err != nil {
		return c.Status(401).JSON(fiber.Map{"message": "Username atau PIN salah"})
	}
	if !player.IsActive {
		return c.Status(403).JSON(fiber.Map{"message": "Akun tidak aktif"})
	}

	now := time.Now()
	player.LastLogin = &now
	config.DB.Save(&player)

	config.DB.Model(&models.PlayerSession{}).Where("player_id = ? AND device_id = ?", player.ID, input.DeviceID).Update("is_active", false)

	token := generateToken(player.ID)
	session := models.PlayerSession{PlayerID: player.ID, Token: token, DeviceID: input.DeviceID, IsActive: true, LastLogin: now}
	config.DB.Create(&session)

	var title models.Title
	config.DB.First(&title, player.TitleID)

	return c.JSON(fiber.Map{
		"message": "Login berhasil!", "id": player.ID, "user_code": player.UserCode,
		"username": player.Username, "name": player.Name, "level": player.Level,
		"xp": player.XP, "stars": player.Stars, "title_name": title.TitleName, "token": token,
	})
}

// ============================================================
// POST /api/admin/login
// ============================================================
func AdminLogin(c *fiber.Ctx) error {
	var req struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}

	if err := c.BodyParser(&req); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Request tidak valid"})
	}

	var admin models.Admin
	if err := config.DB.Where("username = ?", req.Username).First(&admin).Error; err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"message": "Username atau password salah"})
	}

	if err := bcrypt.CompareHashAndPassword([]byte(admin.PasswordHash), []byte(req.Password)); err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"message": "Username atau password salah"})
	}

	tokenBytes := make([]byte, 32)
	if _, err := rand.Read(tokenBytes); err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal membuat session"})
	}

	token := "adm_" + hex.EncodeToString(tokenBytes)
	config.DB.Model(&models.AdminSession{}).Where("admin_id = ?", admin.ID).Update("is_active", false)

	session := models.AdminSession{AdminID: admin.ID, Token: token, IsActive: true, LastLogin: time.Now()}
	if err := config.DB.Create(&session).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal membuat session admin"})
	}

	return c.JSON(fiber.Map{
		"message": "Login admin berhasil", "token": token,
		"admin": fiber.Map{"id": admin.ID, "fullname": admin.Fullname, "username": admin.Username, "role": admin.Role},
	})
}

// ============================================================
// POST /api/logout
// ============================================================
func Logout(c *fiber.Ctx) error {
	token := strings.TrimPrefix(c.Get("Authorization"), "Bearer ")
	if token != "" {
		config.DB.Model(&models.PlayerSession{}).Where("token = ?", token).Update("is_active", false)
	}
	return c.JSON(fiber.Map{"message": "Berhasil keluar"})
}

// ============================================================
// HELPER: Generate token aman (crypto/rand)
// ============================================================
func generateToken(playerID uint) string {
	bytes := make([]byte, 24)
	rand.Read(bytes)
	return fmt.Sprintf("tok_%d_%s", playerID, hex.EncodeToString(bytes))
}
