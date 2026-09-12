package trivia

import (
	"toba-backend/services/trivia"

	"github.com/gofiber/fiber/v2"
)

func GetTriviaQuiz(c *fiber.Ctx) error {
	category, questions, message, status := trivia.GetTriviaQuiz(c.Params("id"))

	if category == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(fiber.Map{
		"category":  category,
		"questions": questions,
	})
}

func SubmitTrivia(c *fiber.Ctx) error {
	type SubmitInput struct {
		PlayerID   uint `json:"player_id"`
		CategoryID uint `json:"category_id"`
		Correct    int  `json:"correct_count"`
		Wrong      int  `json:"wrong_count"`
	}

	input := new(SubmitInput)

	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result, message, status := trivia.SubmitTrivia(
		input.PlayerID,
		input.CategoryID,
		input.Correct,
		input.Wrong,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(result)
}
