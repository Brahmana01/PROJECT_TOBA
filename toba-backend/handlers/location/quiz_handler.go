package location

import (
	"log"

	"toba-backend/services/location"
	playerprogression "toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func currentPlayerID(c *fiber.Ctx) (uint, bool) {
	playerID, ok := c.Locals("player_id").(uint)
	return playerID, ok && playerID > 0
}

func GetLocationQuiz(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	locationData, questions, message, status :=
		location.GetLocationQuiz(playerID, c.Params("id"))
	if locationData == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}

	return c.JSON(fiber.Map{
		"session_id":         locationData.SessionID,
		"location":           locationData,
		"questions":          questions,
		"answered_questions": locationData.AnsweredQuestions,
	})
}

func SubmitLocationAnswer(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	var input struct {
		SessionID        string `json:"session_id"`
		LocationID       uint   `json:"location_id"`
		QuestionID       uint   `json:"question_id"`
		SelectedOptionID uint   `json:"selected_option_id"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input jawaban tidak valid",
		})
	}

	result, message, status := location.SubmitLocationQuizAnswer(
		playerID,
		input.LocationID,
		input.SessionID,
		input.QuestionID,
		input.SelectedOptionID,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.Status(status).JSON(result)
}

func SubmitQuiz(c *fiber.Ctx) error {
	playerID, ok := currentPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	var input struct {
		LocationID uint   `json:"location_id"`
		SessionID  string `json:"session_id"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input submit tidak valid",
		})
	}

	result, message, status := location.SubmitLocationQuiz(
		playerID,
		input.LocationID,
		input.SessionID,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	newlyUnlocked, achievementErr := playerprogression.CheckAndUnlockAchievements(playerID)
	if achievementErr != nil {
		log.Printf("[Achievement] Gagal memeriksa setelah Jelajah: %v", achievementErr)
	} else {
		result["newly_unlocked_achievements"] = newlyUnlocked
	}
	return c.Status(status).JSON(result)
}
