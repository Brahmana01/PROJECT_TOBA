package location

import (
	"toba-backend/services/location"

	"github.com/gofiber/fiber/v2"
)

func GetLocationQuiz(c *fiber.Ctx) error {
	locationData, questions, message, status :=
		location.GetLocationQuiz(c.Params("id"))

	if locationData == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(fiber.Map{
		"location":  locationData,
		"questions": questions,
	})
}

func SubmitQuiz(c *fiber.Ctx) error {
	type SubmitInput struct {
		PlayerID     uint `json:"player_id"`
		LocationID   uint `json:"location_id"`
		CorrectCount int  `json:"correct_count"`
	}

	var input SubmitInput

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := location.SubmitLocationQuiz(
		input.PlayerID,
		input.LocationID,
		input.CorrectCount,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.Status(status).JSON(result)
}
