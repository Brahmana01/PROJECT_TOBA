package challenge

import (
	"toba-backend/services/challenge"

	"github.com/gofiber/fiber/v2"
)

func authenticatedPlayerID(c *fiber.Ctx) (uint, bool) {
	playerID, ok := c.Locals("player_id").(uint)
	return playerID, ok && playerID > 0
}

func GetActiveChallenge(c *fiber.Ctx) error {
	playerID, ok := authenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"message": "Sesi pemain tidak valid"})
	}

	response, message, status := challenge.GetActiveChallenge(playerID)
	if response == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.Status(status).JSON(response)
}

func SubmitChallenge(c *fiber.Ctx) error {
	playerID, ok := authenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"message": "Sesi pemain tidak valid"})
	}

	var input struct {
		SessionID   string `json:"session_id"`
		AnswersJSON string `json:"answers_json"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if input.SessionID == "" || input.AnswersJSON == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "session_id dan answers_json wajib diisi"})
	}

	result, message, status := challenge.SubmitChallenge(playerID, input.SessionID, input.AnswersJSON)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.Status(status).JSON(result)
}
