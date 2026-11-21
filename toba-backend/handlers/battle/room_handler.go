package battle

import (
	"strings"

	"toba-backend/middleware"
	battleService "toba-backend/services/battle"
	battleWS "toba-backend/websocket"

	"github.com/gofiber/fiber/v2"
)

func CreateRoom(c *fiber.Ctx) error {
	var input struct {
		PlayerID uint `json:"player_id"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}

	result, err := battleService.CreateBattleRoom(playerID)
	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal membuat battle room",
		})
	}

	return c.JSON(result)
}

func JoinRoom(c *fiber.Ctx) error {
	var input struct {
		PlayerID uint   `json:"player_id"`
		RoomCode string `json:"room_code"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}

	roomCode := strings.ToUpper(strings.TrimSpace(input.RoomCode))
	if roomCode == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Kode room wajib diisi",
		})
	}

	result, message, status := battleService.JoinBattleRoom(
		playerID,
		roomCode,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.Status(status).JSON(result)
}

func GetRoomStatus(c *fiber.Ctx) error {
	playerID, ok := middleware.AuthenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	roomCode := strings.TrimSpace(c.Params("code"))
	if roomCode == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Kode room tidak valid",
		})
	}

	result, message, status := battleService.GetBattleRoomStatus(
		roomCode,
		playerID,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	if result.ScoreChanged {
		battleWS.Hub.BroadcastScoreUpdate(result.RoomCode, fiber.Map{
			"room_code":            result.RoomCode,
			"host_score":           result.HostScore,
			"guest_score":          result.GuestScore,
			"host_answered_count":  result.HostAnsweredCount,
			"guest_answered_count": result.GuestAnsweredCount,
			"total_questions":      10,
			"is_bot":               result.IsBot,
			"bot_name":             result.BotName,
			"bot_level":            result.BotLevel,
			"bot_answered_count":   result.BotAnsweredCount,
		})
	}
	return c.Status(status).JSON(result)
}

// Tetap gunakan pemeriksaan kepemilikan jika nantinya digunakan.
func GetRoomQuestions(c *fiber.Ctx) error {
	playerID, ok := middleware.AuthenticatedPlayerID(c)
	if !ok {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Sesi pemain tidak valid",
		})
	}

	roomCode := strings.TrimSpace(c.Query("room_code"))
	if roomCode == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "room_code diperlukan",
		})
	}

	result, message, status := battleService.GetBattleRoomStatus(
		roomCode,
		playerID,
	)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.JSON(fiber.Map{
		"questions": result.Questions,
	})
}
