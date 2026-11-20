package battle

import (
	"strings"

	"toba-backend/middleware"
	battleService "toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func GetBattleQuestions(c *fiber.Ctx) error {
	playerID, ok := middleware.AuthenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"message": "Sesi pemain tidak valid"})
	}

	roomCode := strings.TrimSpace(c.Query("room_code"))
	if roomCode == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "room_code diperlukan"})
	}

	room, message, status := battleService.GetBattleRoomStatus(roomCode, playerID)
	if room == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.JSON(fiber.Map{"questions": room.Questions})
}

func SubmitBattleAnswer(c *fiber.Ctx) error {
	var input struct {
		RoomID           uint  `json:"room_id"`
		PlayerID         uint  `json:"player_id"`
		QuestionID       uint  `json:"question_id"`
		SelectedOptionID uint  `json:"selected_option_id"`
		AnswerTimeMs     int64 `json:"answer_time_ms"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}
	if input.RoomID == 0 || input.QuestionID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Data jawaban tidak lengkap"})
	}

	result, message, status := battleService.SubmitBattleAnswer(
		input.RoomID, playerID, input.QuestionID, input.SelectedOptionID, input.AnswerTimeMs,
	)
	if result == nil {
		return c.Status(status).JSON(fiber.Map{"message": message})
	}
	return c.Status(status).JSON(result)
}
