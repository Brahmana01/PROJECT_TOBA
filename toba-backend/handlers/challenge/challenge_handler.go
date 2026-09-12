package challenge

import (
	"toba-backend/services/challenge"

	"github.com/gofiber/fiber/v2"
)

func GetActiveChallenge(c *fiber.Ctx) error {
	playerID := c.Query("player_id")

	result := challenge.GetActiveChallenge(playerID)

	return c.JSON(result)
}

func SubmitChallenge(c *fiber.Ctx) error {
	type Input struct {
		PlayerID    uint `json:"player_id"`
		ChallengeID uint `json:"challenge_id"`
		Score       int  `json:"score"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := challenge.SubmitChallenge(
		input.PlayerID,
		input.ChallengeID,
		input.Score,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}
