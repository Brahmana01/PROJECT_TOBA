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
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	return c.JSON(challenge.GetActiveChallenge(playerID))
}

func SubmitChallenge(c *fiber.Ctx) error {
	playerID, ok := authenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	var input struct {
		ChallengeID uint `json:"challenge_id"`
		Score       int  `json:"score"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}
	if input.ChallengeID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Challenge ID wajib diisi",
		})
	}

	result, message, status := challenge.SubmitChallenge(
		playerID,
		input.ChallengeID,
		input.Score,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}
	return c.Status(status).JSON(result)
}
