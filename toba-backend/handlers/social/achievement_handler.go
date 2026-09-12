package social

import (
	"strconv"

	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerAchievements(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)

	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	result, err := player.GetPlayerAchievements(uint(id))

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil achievement",
		})
	}

	return c.JSON(result)
}

func ClaimAchievement(c *fiber.Ctx) error {
	type Input struct {
		PlayerID      uint `json:"player_id"`
		AchievementID uint `json:"achievement_id"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	err := player.ClaimAchievement(
		input.PlayerID,
		input.AchievementID,
	)

	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Achievement tidak dapat diklaim",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Achievement berhasil diklaim",
	})
}

func CheckAndUnlockAchievements(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(
		c.Params("id"),
		10,
		64,
	)

	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	if err := player.CheckAndUnlockAchievements(uint(id)); err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengecek achievement",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Achievement berhasil diperiksa",
	})
}
