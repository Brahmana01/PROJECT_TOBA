package battle

import (
	"toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func CreateRoom(c *fiber.Ctx) error {
	var input struct {
		PlayerID uint `json:"player_id"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, err := battle.CreateBattleRoom(input.PlayerID)

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	return c.JSON(result)
}

func JoinRoom(c *fiber.Ctx) error {
	var input struct {
		PlayerID uint   `json:"player_id"`
		RoomCode string `json:"room_code"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := battle.JoinBattleRoom(
		input.PlayerID,
		input.RoomCode,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}

func GetRoomStatus(c *fiber.Ctx) error {
	result, message, status := battle.GetBattleRoomStatus(
		c.Params("code"),
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}

func GetRoomQuestions(c *fiber.Ctx) error {
	questions := battle.GetRoomBattleQuestions(
		c.Params("code"),
	)

	return c.JSON(fiber.Map{
		"questions": questions,
	})
}
