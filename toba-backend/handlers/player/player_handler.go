package player

import (
	"strings"

	"toba-backend/middleware"
	playerService "toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// PROFILE
// GET /api/profile/:id
// ============================================================

func GetProfile(c *fiber.Ctx) error {
	playerID, err := middleware.RequirePathPlayerID(
		c,
		c.Params("id"),
	)

	if err != nil {
		return err
	}

	result, err := playerService.GetProfile(playerID)

	if err != nil {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
			"message": "Player tidak ditemukan",
		})
	}

	return c.JSON(result)
}

// ============================================================
// FCM TOKEN
// POST /api/player/fcm-token
// ============================================================

func UpdateFCMToken(c *fiber.Ctx) error {
	type Input struct {
		PlayerID uint   `json:"player_id"`
		FCMToken string `json:"fcm_token"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	playerID, err := middleware.RequirePlayerID(
		c,
		input.PlayerID,
	)

	if err != nil {
		return err
	}

	token := strings.TrimSpace(input.FCMToken)

	if token == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "FCM token tidak boleh kosong",
		})
	}

	if err := playerService.UpdateFCMToken(playerID, token); err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui FCM token",
		})
	}

	return c.JSON(fiber.Map{
		"message": "FCM token berhasil diperbarui",
	})
}

// ============================================================
// LEADERBOARD
// GET /api/leaderboard
// ============================================================

func GetLeaderboard(c *fiber.Ctx) error {
	result, err := playerService.GetLeaderboard()

	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil leaderboard",
		})
	}

	return c.JSON(result)
}
