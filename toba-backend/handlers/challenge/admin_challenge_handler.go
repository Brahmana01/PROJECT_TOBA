package challenge

import (
	"toba-backend/services/challenge"

	"github.com/gofiber/fiber/v2"
)

func AdminGetChallenges(c *fiber.Ctx) error {
	challenges, err := challenge.AdminGetChallenges()

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil data tantangan",
		})
	}

	return c.JSON(challenges)
}

func AdminCreateChallenge(c *fiber.Ctx) error {
	type Input struct {
		Title        string `json:"title"`
		Description  string `json:"description"`
		GameType     string `json:"game_type"`
		RewardXP     int    `json:"reward_xp"`
		RewardStars  int    `json:"reward_stars"`
		DataJSON     string `json:"data_json"`
		DurationDays int    `json:"duration_days"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	challenge, err := challenge.AdminCreateChallenge(
		input.Title,
		input.Description,
		input.GameType,
		input.RewardXP,
		input.RewardStars,
		input.DataJSON,
		input.DurationDays,
	)

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal membuat tantangan",
		})
	}

	return c.Status(201).JSON(challenge)
}

func AdminUpdateChallenge(c *fiber.Ctx) error {
	id := c.Params("id")

	type Input struct {
		Title       string `json:"title"`
		Description string `json:"description"`
		RewardXP    int    `json:"reward_xp"`
		RewardStars int    `json:"reward_stars"`
		DataJSON    string `json:"data_json"`
		IsActive    bool   `json:"is_active"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	challenge, message, status := challenge.AdminUpdateChallenge(
		id,
		input.Title,
		input.Description,
		input.RewardXP,
		input.RewardStars,
		input.DataJSON,
		input.IsActive,
	)

	if challenge == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(challenge)
}

func AdminDeleteChallenge(c *fiber.Ctx) error {
	id := c.Params("id")

	message, status := challenge.AdminDeleteChallenge(id)

	return c.Status(status).JSON(fiber.Map{
		"message": message,
	})
}
