package controllers

import (
	"fmt"
	"math/rand"
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// POST /api/battle/create
// Host membuat room baru
// ============================================================
func CreateRoom(c *fiber.Ctx) error {
	type CreateInput struct {
		PlayerID uint `json:"player_id"`
	}

	input := new(CreateInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	// Generate kode room unik (6 karakter)
	roomCode := generateRoomCode()

	// Waktu expired: 5 menit dari sekarang
	expiredAt := time.Now().Add(5 * time.Minute)

	room := models.BattleRoom{
		RoomCode:     roomCode,
		HostPlayerID: input.PlayerID,
		Status:       "WAITING",
		ExpiredAt:    &expiredAt,
	}

	if err := config.DB.Create(&room).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal membuat room"})
	}

	return c.JSON(fiber.Map{
		"room_id":    room.ID,
		"room_code":  room.RoomCode,
		"status":     room.Status,
		"expired_at": room.ExpiredAt,
	})
}

// ============================================================
// POST /api/battle/join
// Guest bergabung ke room
// ============================================================
func JoinRoom(c *fiber.Ctx) error {
	type JoinInput struct {
		PlayerID uint   `json:"player_id"`
		RoomCode string `json:"room_code"`
	}

	input := new(JoinInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	// Cari room
	var room models.BattleRoom
	if err := config.DB.Where("room_code = ?", input.RoomCode).First(&room).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Room tidak ditemukan"})
	}

	// Validasi status room
	if room.Status != "WAITING" {
		return c.Status(400).JSON(fiber.Map{"message": "Room tidak tersedia"})
	}

	// Cek expired
	if room.ExpiredAt != nil && time.Now().After(*room.ExpiredAt) {
		config.DB.Model(&room).Update("status", "EXPIRED")
		return c.Status(400).JSON(fiber.Map{"message": "Room sudah kadaluarsa"})
	}

	// Cek host tidak bisa join room sendiri
	if room.HostPlayerID == input.PlayerID {
		return c.Status(400).JSON(fiber.Map{"message": "Kamu adalah host room ini"})
	}

	// Set guest & ubah status PLAYING
	now := time.Now()
	room.GuestPlayerID = &input.PlayerID
	room.Status = "PLAYING"
	room.StartedAt = &now
	config.DB.Save(&room)

	// Pilih 5 soal battle acak & simpan ke battle_room_questions
	var battleQuestions []models.BattleQuestion
	config.DB.
		Preload("Options").
		Where("is_active = ?", true).
		Order("RANDOM()").
		Limit(5).
		Find(&battleQuestions)

	for i, q := range battleQuestions {
		roomQuestion := models.BattleRoomQuestion{
			RoomID:        room.ID,
			QuestionID:    q.ID,
			QuestionOrder: i + 1,
		}
		config.DB.Create(&roomQuestion)
	}

	return c.JSON(fiber.Map{
		"room_id":   room.ID,
		"room_code": room.RoomCode,
		"status":    room.Status,
		"questions": battleQuestions,
	})
}

// ============================================================
// GET /api/battle/status/:code
// Cek status room (polling dari Unity)
// ============================================================
func GetRoomStatus(c *fiber.Ctx) error {
	roomCode := c.Params("code")

	var room models.BattleRoom
	if err := config.DB.Where("room_code = ?", roomCode).First(&room).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Room tidak ditemukan"})
	}

	// Auto-expire jika sudah lewat waktu
	if room.Status == "WAITING" && room.ExpiredAt != nil && time.Now().After(*room.ExpiredAt) {
		config.DB.Model(&room).Update("status", "EXPIRED")
		room.Status = "EXPIRED"
	}

	// Ambil soal jika status PLAYING
	var questions []models.BattleQuestion
	if room.Status == "PLAYING" {
		var roomQuestions []models.BattleRoomQuestion
		config.DB.
			Where("room_id = ?", room.ID).
			Order("question_order ASC").
			Find(&roomQuestions)

		for _, rq := range roomQuestions {
			var q models.BattleQuestion
			config.DB.Preload("Options").First(&q, rq.QuestionID)
			questions = append(questions, q)
		}
	}

	// Ambil nama host & guest
	var host models.Player
	config.DB.First(&host, room.HostPlayerID)

	hostName := host.Name
	guestName := ""
	if room.GuestPlayerID != nil {
		var guest models.Player
		config.DB.First(&guest, *room.GuestPlayerID)
		guestName = guest.Name
	}

	return c.JSON(fiber.Map{
		"room_id":         room.ID,
		"room_code":       room.RoomCode,
		"status":          room.Status,
		"host_player_id":  room.HostPlayerID,
		"guest_player_id": room.GuestPlayerID,
		"winner_id":       room.WinnerPlayerID,
		"host_name":       hostName,
		"guest_name":      guestName,
		"questions":       questions,
	})
}

// ============================================================
// GET /api/battle/questions
// Ambil soal battle untuk room tertentu
// ============================================================
func GetBattleQuestions(c *fiber.Ctx) error {
	roomID := c.Query("room_id")
	if roomID == "" {
		return c.Status(400).JSON(fiber.Map{"message": "room_id diperlukan"})
	}

	var roomQuestions []models.BattleRoomQuestion
	config.DB.
		Where("room_id = ?", roomID).
		Order("question_order ASC").
		Find(&roomQuestions)

	var questions []models.BattleQuestion
	for _, rq := range roomQuestions {
		var q models.BattleQuestion
		config.DB.Preload("Options").First(&q, rq.QuestionID)
		questions = append(questions, q)
	}

	return c.JSON(fiber.Map{"questions": questions})
}

// ============================================================
// POST /api/battle/answer
// Submit jawaban battle satu per satu
// ============================================================
func SubmitBattleAnswer(c *fiber.Ctx) error {
	type AnswerInput struct {
		RoomID           uint `json:"room_id"`
		PlayerID         uint `json:"player_id"`
		QuestionID       uint `json:"question_id"`
		SelectedOptionID uint `json:"selected_option_id"`
	}

	input := new(AnswerInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	// Cek apakah jawaban benar
	var option models.BattleQuestionOption
	config.DB.First(&option, input.SelectedOptionID)

	answer := models.BattleAnswer{
		RoomID:           input.RoomID,
		PlayerID:         input.PlayerID,
		QuestionID:       input.QuestionID,
		SelectedOptionID: input.SelectedOptionID,
		IsCorrect:        option.IsCorrect,
		AnsweredAt:       time.Now(),
	}
	config.DB.Create(&answer)

	return c.JSON(fiber.Map{
		"is_correct": option.IsCorrect,
	})
}

// ============================================================
// POST /api/battle/finish
// Selesaikan battle & tentukan pemenang
// ============================================================
func FinishBattle(c *fiber.Ctx) error {
	type FinishInput struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"` // player yang request finish
	}

	input := new(FinishInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var room models.BattleRoom
	if err := config.DB.First(&room, input.RoomID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Room tidak ditemukan"})
	}

	// Hitung skor host
	var hostScore int64
	config.DB.Model(&models.BattleAnswer{}).
		Where("room_id = ? AND player_id = ? AND is_correct = ?",
			input.RoomID, room.HostPlayerID, true).
		Count(&hostScore)

	// Hitung skor guest
	var guestScore int64
	if room.GuestPlayerID != nil {
		config.DB.Model(&models.BattleAnswer{}).
			Where("room_id = ? AND player_id = ? AND is_correct = ?",
				input.RoomID, *room.GuestPlayerID, true).
			Count(&guestScore)
	}

	// Tentukan pemenang
	var winnerID *uint
	if hostScore > guestScore {
		winnerID = &room.HostPlayerID
	} else if guestScore > hostScore && room.GuestPlayerID != nil {
		winnerID = room.GuestPlayerID
	}
	// Jika sama: winnerID = nil (seri)

	// Update room
	now := time.Now()
	room.Status = "FINISHED"
	room.FinishedAt = &now
	room.WinnerPlayerID = winnerID
	config.DB.Save(&room)

	// Beri reward XP ke pemenang
	if winnerID != nil {
		var winner models.Player
		config.DB.First(&winner, *winnerID)
		winner.XP += 50 // 50 XP untuk pemenang battle
		winner.Stars += 2

		newLevel := (winner.XP / 500) + 1
		if newLevel > winner.Level {
			winner.Level = newLevel
			updatePlayerTitle(&winner)
		}
		config.DB.Save(&winner)
	}

	return c.JSON(fiber.Map{
		"status":      "FINISHED",
		"host_score":  hostScore,
		"guest_score": guestScore,
		"winner_id":   winnerID,
	})
}

// ============================================================
// POST /api/battle/disconnect
// Handle disconnect salah satu player
// ============================================================
func DisconnectBattle(c *fiber.Ctx) error {
	type DisconnectInput struct {
		RoomID   uint `json:"room_id"`
		PlayerID uint `json:"player_id"`
	}

	input := new(DisconnectInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var room models.BattleRoom
	if err := config.DB.First(&room, input.RoomID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Room tidak ditemukan"})
	}

	if room.Status != "PLAYING" {
		return c.Status(400).JSON(fiber.Map{"message": "Battle tidak sedang berlangsung"})
	}

	// Tentukan pemenang (yang tidak disconnect)
	var winnerID uint
	if input.PlayerID == room.HostPlayerID {
		if room.GuestPlayerID != nil {
			winnerID = *room.GuestPlayerID
		}
	} else {
		winnerID = room.HostPlayerID
	}

	now := time.Now()
	room.Status = "FINISHED"
	room.FinishedAt = &now
	room.WinnerPlayerID = &winnerID
	config.DB.Save(&room)

	return c.JSON(fiber.Map{
		"status":    "FINISHED",
		"winner_id": winnerID,
		"message":   "Lawan disconnect. Kamu menang!",
	})
}

// ============================================================
// HELPER: Generate kode room 6 karakter
// ============================================================
func generateRoomCode() string {
	rand.Seed(time.Now().UnixNano())
	chars := "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
	code := make([]byte, 6)
	for i := range code {
		code[i] = chars[rand.Intn(len(chars))]
	}
	return fmt.Sprintf("%s", code)
}
