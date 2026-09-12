package player

import (
	"strconv"

	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// PROFILE
// ============================================================

func GetProfile(c *fiber.Ctx) error {
	playerID := c.Params("id")

	id, err := strconv.ParseUint(playerID, 10, 64)
	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	result, err := player.GetProfile(uint(id))
	if err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Player tidak ditemukan",
		})
	}

	return c.JSON(result)
}

// ============================================================
// FCM TOKEN
// ============================================================

func UpdateFCMToken(c *fiber.Ctx) error {
	type Input struct {
		PlayerID uint   `json:"player_id"`
		FCMToken string `json:"fcm_token"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Invalid",
		})
	}

	err := player.UpdateFCMToken(input.PlayerID, input.FCMToken)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal memperbarui FCM token",
		})
	}

	return c.JSON(fiber.Map{
		"message": "FCM token updated",
	})
}

// ============================================================
// LEADERBOARD
// ============================================================

func GetLeaderboard(c *fiber.Ctx) error {
	result, err := player.GetLeaderboard()
	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil leaderboard",
		})
	}

	return c.JSON(result)
}
