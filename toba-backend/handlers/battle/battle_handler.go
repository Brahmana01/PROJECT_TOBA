package battle

import (
	"toba-backend/middleware"
	battleService "toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func FinishBattle(c *fiber.Ctx) error {
	var input struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	if input.RoomID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Room ID wajib diisi",
		})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}

	result, message, status := battleService.FinishBattle(
		input.RoomID,
		playerID,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.Status(status).JSON(result)
}

func DisconnectBattle(c *fiber.Ctx) error {
	var input struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	if input.RoomID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Room ID wajib diisi",
		})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}

	winnerID, message, status := battleService.DisconnectBattle(
		input.RoomID,
		playerID,
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
