package middleware

import (
	"errors"
	"strconv"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
)

func AuthMiddleware(c *fiber.Ctx) error {
	authHeader := strings.TrimSpace(c.Get("Authorization"))
	parts := strings.Fields(authHeader)

	if len(parts) != 2 ||
		!strings.EqualFold(parts[0], "Bearer") ||
		strings.TrimSpace(parts[1]) == "" {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token tidak ditemukan atau format tidak valid",
		})
	}

	token := strings.TrimSpace(parts[1])

	var session models.PlayerSession

	if err := config.DB.
		Where("token = ? AND is_active = ?", token, true).
		First(&session).Error; err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token tidak valid atau sudah tidak aktif",
		})
	}

	if session.PlayerID == 0 {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Identitas session tidak valid",
		})
	}

	// Session dengan ExpiredAt yang kosong tetap kompatibel
	// dengan session lama. Jika terisi, wajib belum kedaluwarsa.
	if session.ExpiredAt != nil &&
		!session.ExpiredAt.After(time.Now()) {

		config.DB.Model(&models.PlayerSession{}).
			Where("id = ?", session.ID).
			Update("is_active", false)

		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Session sudah kedaluwarsa",
		})
	}

	// Pastikan pemilik session masih terdaftar dan aktif.
	var currentPlayer models.Player

	err := config.DB.
		Select("id", "is_active").
		First(&currentPlayer, session.PlayerID).Error

	if errors.Is(err, gorm.ErrRecordNotFound) {
		config.DB.Model(&models.PlayerSession{}).
			Where("id = ?", session.ID).
			Update("is_active", false)

		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Akun pemain tidak ditemukan",
		})
	}

	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memvalidasi akun pemain",
		})
	}

	if !currentPlayer.IsActive {
		config.DB.Model(&models.PlayerSession{}).
			Where("id = ?", session.ID).
			Update("is_active", false)

		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Akun pemain tidak aktif",
		})
	}

	// Identitas tepercaya berasal dari session database.
	c.Locals("player_id", session.PlayerID)

	return c.Next()
}

// Mengambil identitas pemain yang sudah diverifikasi middleware.
func AuthenticatedPlayerID(c *fiber.Ctx) (uint, bool) {
	playerID, ok := c.Locals("player_id").(uint)

	if !ok || playerID == 0 {
		return 0, false
	}

	return playerID, true
}

// Memastikan identitas dari request tidak berbeda dengan token.
// Nilai 0 berarti client tidak mengirim player_id.
func RequirePlayerID(c *fiber.Ctx, requestedID uint) (uint, error) {
	authenticatedID, ok := AuthenticatedPlayerID(c)

	if !ok {
		return 0, c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	if requestedID != 0 && requestedID != authenticatedID {
		return 0, c.Status(fiber.StatusForbidden).JSON(fiber.Map{
			"message": "Tidak diizinkan mengakses akun pemain lain",
		})
	}

	return authenticatedID, nil
}

// Memeriksa parameter URL yang berisi ID pemain.
func RequirePathPlayerID(c *fiber.Ctx, rawID string) (uint, error) {
	requestedID, err := strconv.ParseUint(
		strings.TrimSpace(rawID),
		10,
		64,
	)

	if err != nil || requestedID == 0 {
		return 0, c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	authenticatedID, ok := AuthenticatedPlayerID(c)

	if !ok {
		return 0, c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	if requestedID != uint64(authenticatedID) {
		return 0, c.Status(fiber.StatusForbidden).JSON(fiber.Map{
			"message": "Tidak diizinkan mengakses data pemain lain",
		})
	}

	return authenticatedID, nil
}
