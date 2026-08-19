package controllers

import (
	"fmt"
	"math/rand"
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

	// Hash PIN
	pinHash, err := bcrypt.GenerateFromPassword([]byte(input.Pin), bcrypt.DefaultCost)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal memproses PIN"})
	}

	// Generate user_code & username unik
	var lastPlayer models.Player
	config.DB.Order("id DESC").First(&lastPlayer)
	newID := lastPlayer.ID + 1
	userCode := fmt.Sprintf("USR%06d", newID)

	// Username: nama bersih + 4 angka random
	cleanName := strings.ToLower(strings.ReplaceAll(strings.TrimSpace(input.Name), " ", ""))
	rand.Seed(time.Now().UnixNano())
	suffix := rand.Intn(9000) + 1000
	username := fmt.Sprintf("%s%d", cleanName, suffix)

	// Pastikan username unik
	for {
		var existing models.Player
		if config.DB.Where("username = ?", username).First(&existing).Error != nil {
			break
		}
		suffix = rand.Intn(9000) + 1000
		username = fmt.Sprintf("%s%d", cleanName, suffix)
	}

	// Buat player
	player := models.Player{
		UserCode:    userCode,
		Username:    username,
		Name:        strings.TrimSpace(input.Name),
		PinHash:     string(pinHash),
		CharacterID: input.CharacterID,
		TitleID:     1,
		Level:       1,
		XP:          0,
		Stars:       0,
		IsActive:    true,
	}

	if err := config.DB.Create(&player).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal membuat akun"})
	}

	// Unlock lokasi pertama otomatis
	var firstLocation models.Location
	if config.DB.Where("level_order = 1 AND is_active = ?", true).
		First(&firstLocation).Error == nil {
		firstProgress := models.PlayerLocationProgress{
			PlayerID:   player.ID,
			LocationID: firstLocation.ID,
			IsUnlocked: true,
		}
		config.DB.Create(&firstProgress)
	}

	// Buat session
	token := generateToken(player.ID)
	session := models.PlayerSession{
		PlayerID: player.ID,
		Token:    token,
		DeviceID: input.DeviceID,
		IsActive: true,
	}
	config.DB.Create(&session)

	return c.Status(201).JSON(fiber.Map{
		"message":   "Akun berhasil dibuat!",
		"id":        player.ID,
		"user_code": player.UserCode,
		"username":  player.Username,
		"name":      player.Name,
		"level":     player.Level,
		"stars":     player.Stars,
		"token":     token,
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
	if err := config.DB.Where("username = ?", input.Username).
		First(&player).Error; err != nil {
		return c.Status(401).JSON(fiber.Map{"message": "Username atau PIN salah"})
	}

	if err := bcrypt.CompareHashAndPassword([]byte(player.PinHash), []byte(input.Pin)); err != nil {
		return c.Status(401).JSON(fiber.Map{"message": "Username atau PIN salah"})
	}

	if !player.IsActive {
		return c.Status(403).JSON(fiber.Map{"message": "Akun tidak aktif"})
	}

	// Update last login
	now := time.Now()
	player.LastLogin = &now
	config.DB.Save(&player)

	// Nonaktifkan session lama di device yang sama
	config.DB.Model(&models.PlayerSession{}).
		Where("player_id = ? AND device_id = ?", player.ID, input.DeviceID).
		Update("is_active", false)

	// Buat session baru
	token := generateToken(player.ID)
	session := models.PlayerSession{
		PlayerID:  player.ID,
		Token:     token,
		DeviceID:  input.DeviceID,
		IsActive:  true,
		LastLogin: now,
	}
	config.DB.Create(&session)

	// Ambil title
	var title models.Title
	config.DB.First(&title, player.TitleID)

	return c.JSON(fiber.Map{
		"message":    "Login berhasil!",
		"id":         player.ID,
		"user_code":  player.UserCode,
		"username":   player.Username,
		"name":       player.Name,
		"level":      player.Level,
		"xp":         player.XP,
		"stars":      player.Stars,
		"title_name": title.TitleName,
		"token":      token,
	})
}

// ============================================================
// HELPER: Generate token sederhana
// ============================================================
func generateToken(playerID uint) string {
	rand.Seed(time.Now().UnixNano())
	return fmt.Sprintf("tok_%d_%d_%d", playerID, time.Now().Unix(), rand.Intn(99999))
}
