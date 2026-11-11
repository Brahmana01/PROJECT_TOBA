package social

import (
	"errors"
	"log"
	"strconv"

	"toba-backend/models"
	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
)

// ==========================================
// AUTHENTICATED PLAYER
// ==========================================

func authenticatedPlayerID(c *fiber.Ctx) (uint, bool) {

	id, ok := c.Locals("player_id").(uint)

	if !ok || id == 0 {
		return 0, false
	}

	return id, true
}

// ==========================================
// GET PLAYER ACHIEVEMENTS
// ==========================================

func GetPlayerAchievements(c *fiber.Ctx) error {

	requestedID, err := strconv.ParseUint(
		c.Params("id"),
		10,
		64,
	)

	if err != nil || requestedID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	authPlayerID, ok := authenticatedPlayerID(c)

	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	if uint(requestedID) != authPlayerID {
		return c.Status(fiber.StatusForbidden).JSON(fiber.Map{
			"message": "Tidak diizinkan mengakses achievement pemain lain",
		})
	}

	result, err := player.GetPlayerAchievements(authPlayerID)

	if err != nil {

		log.Printf(
			"[Achievement] Gagal mengambil achievement player %d: %v",
			authPlayerID,
			err,
		)

		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil achievement",
		})
	}

	if result == nil {
		result = make([]models.PlayerAchievement, 0)
	}

	return c.JSON(fiber.Map{
		"total":        len(result),
		"achievements": result,
	})
}

// ==========================================
// CLAIM ACHIEVEMENT
// ==========================================

func ClaimAchievement(c *fiber.Ctx) error {

	type Input struct {
		PlayerID      uint `json:"player_id"`
		AchievementID uint `json:"achievement_id"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {

		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	if input.AchievementID == 0 {

		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Achievement ID wajib diisi",
		})
	}

	authPlayerID, ok := authenticatedPlayerID(c)

	if !ok {

		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	// Pastikan request tidak mengklaim achievement
	// milik pemain lain.
	if input.PlayerID != 0 && input.PlayerID != authPlayerID {

		return c.Status(fiber.StatusForbidden).JSON(fiber.Map{
			"message": "Tidak diizinkan mengklaim achievement pemain lain",
		})
	}

	result, err := player.ClaimAchievement(
		authPlayerID,
		input.AchievementID,
	)

	if err != nil {

		if errors.Is(err, gorm.ErrRecordNotFound) {

			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Achievement belum terbuka atau data pemain tidak ditemukan",
			})
		}

		log.Printf(
			"[Achievement] Gagal klaim achievement %d untuk player %d: %v",
			input.AchievementID,
			authPlayerID,
			err,
		)

		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Achievement tidak dapat diklaim",
		})
	}

	message := "Achievement berhasil diklaim"

	if result.AlreadyClaimed {
		message = "Achievement sudah pernah diklaim"
	}

	return c.JSON(fiber.Map{
		"message":      message,
		"reward_xp":    result.RewardXP,
		"reward_stars": result.RewardStars,
		"level":        result.Level,
		"total_xp":     result.TotalXP,
		"total_stars":  result.TotalStars,
	})
}

// ==========================================
// CHECK AND UNLOCK ACHIEVEMENTS
// ==========================================

func CheckAndUnlockAchievements(c *fiber.Ctx) error {

	requestedID, err := strconv.ParseUint(
		c.Params("id"),
		10,
		64,
	)

	if err != nil || requestedID == 0 {

		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	authPlayerID, ok := authenticatedPlayerID(c)

	if !ok {

		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	if uint(requestedID) != authPlayerID {

		return c.Status(fiber.StatusForbidden).JSON(fiber.Map{
			"message": "Tidak diizinkan memeriksa achievement pemain lain",
		})
	}

	newlyUnlocked, err := player.CheckAndUnlockAchievements(
		authPlayerID,
	)

	if err != nil {

		if errors.Is(err, gorm.ErrRecordNotFound) {

			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Player tidak ditemukan",
			})
		}

		log.Printf(
			"[Achievement] Gagal mengecek achievement player %d: %v",
			authPlayerID,
			err,
		)

		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengecek achievement",
		})
	}

	if newlyUnlocked == nil {
		newlyUnlocked = make([]models.Achievement, 0)
	}

	return c.JSON(fiber.Map{
		"message":        "Achievement berhasil diperiksa",
		"count":          len(newlyUnlocked),
		"newly_unlocked": newlyUnlocked,
	})
}
