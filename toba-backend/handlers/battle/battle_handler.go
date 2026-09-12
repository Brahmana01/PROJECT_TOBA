package battle

import (
	"toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func FinishBattle(c *fiber.Ctx) error {
	type Input struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := battle.FinishBattle(input.RoomID)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}

func DisconnectBattle(c *fiber.Ctx) error {
	type Input struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	winnerID, message, status := battle.DisconnectBattle(
		input.RoomID,
		input.PlayerID,
	)

	if message != "" {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(fiber.Map{
		"status":    "FINISHED",
		"winner_id": winnerID,
		"message":   "Lawan disconnect. Kamu menang!",
	})
}
