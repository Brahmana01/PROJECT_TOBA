package trivia

import (
	"log"

	playerprogression "toba-backend/services/player"
	"toba-backend/services/trivia"

	"github.com/gofiber/fiber/v2"
)

func currentPlayerID(c *fiber.Ctx) (uint, bool) {
	playerID, ok := c.Locals("player_id").(uint)
	return playerID, ok && playerID > 0
}

func GetTriviaQuiz(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	category, questions, message, status :=
		trivia.GetTriviaQuiz(playerID, c.Params("id"))
	if category == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}

	return c.JSON(fiber.Map{
		"session_id":         category.SessionID,
		"category":           category,
		"questions":          questions,
		"answered_questions": category.AnsweredQuestions,
	})
}

func SubmitTriviaAnswer(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	var input struct {
		SessionID        string `json:"session_id"`
		CategoryID       uint   `json:"category_id"`
		QuestionID       uint   `json:"question_id"`
		SelectedOptionID uint   `json:"selected_option_id"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input jawaban tidak valid",
		})
	}

	result, message, status := trivia.SubmitTriviaAnswer(
		playerID,
		input.CategoryID,
		input.SessionID,
		input.QuestionID,
		input.SelectedOptionID,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.Status(status).JSON(result)
}

func SubmitTrivia(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	var input struct {
		CategoryID uint   `json:"category_id"`
		SessionID  string `json:"session_id"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input submit tidak valid",
		})
	}

	result, message, status := trivia.SubmitTrivia(
		playerID,
		input.CategoryID,
		input.SessionID,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	newlyUnlocked, achievementErr := playerprogression.CheckAndUnlockAchievements(playerID)
	if achievementErr != nil {
		log.Printf("[Achievement] Gagal memeriksa setelah Trivia: %v", achievementErr)
	} else {
		result["newly_unlocked_achievements"] = newlyUnlocked
	}
	return c.Status(status).JSON(result)
}
