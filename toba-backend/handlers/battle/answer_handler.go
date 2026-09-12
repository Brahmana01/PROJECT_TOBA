package battle

import (
	"toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func GetBattleQuestions(c *fiber.Ctx) error {
	roomID := c.Query("room_id")

	if roomID == "" {
		return c.Status(400).JSON(fiber.Map{
			"message": "room_id diperlukan",
		})
	}

	return c.JSON(fiber.Map{
		"questions": battle.GetRoomBattleQuestions(roomID),
	})
}

func SubmitBattleAnswer(c *fiber.Ctx) error {
	type Input struct {
		RoomID           uint  `json:"room_id"`
		PlayerID         uint  `json:"player_id"`
		QuestionID       uint  `json:"question_id"`
		SelectedOptionID uint  `json:"selected_option_id"`
		AnswerTimeMs     int64 `json:"answer_time_ms"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	isCorrect, message, status := battle.SubmitBattleAnswer(
		input.RoomID,
		input.PlayerID,
		input.QuestionID,
		input.SelectedOptionID,
		input.AnswerTimeMs,
	)

	if message != "" {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(fiber.Map{
		"is_correct": isCorrect,
	})
}
