package battle

import (
	"fmt"

	"toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func Matchmake(c *fiber.Ctx) error {
	type Input struct {
		PlayerID uint `json:"player_id"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := battle.MatchmakePlayer(input.PlayerID)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}

func CheckMatchmake(c *fiber.Ctx) error {
	var playerID uint

	if _, err := fmt.Sscanf(
		c.Params("player_id"),
		"%d",
		&playerID,
	); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	return c.JSON(
		battle.CheckMatchmaking(playerID),
	)
}

func CancelMatchmake(c *fiber.Ctx) error {
	var playerID uint

	if _, err := fmt.Sscanf(
		c.Params("player_id"),
		"%d",
		&playerID,
	); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	battle.CancelMatchmaking(playerID)

	return c.JSON(fiber.Map{
		"message": "Pencarian dibatalkan",
	})
}
