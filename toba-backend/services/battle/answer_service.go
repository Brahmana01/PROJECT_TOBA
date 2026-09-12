package battle

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
)

func SubmitBattleAnswer(
	roomID uint,
	playerID uint,
	questionID uint,
	selectedOptionID uint,
	answerTimeMs int64,
) (bool, string, int) {

	if answerTimeMs < 200 || answerTimeMs > 16000 {
		return false, "Waktu jawaban tidak valid", 400
	}

	var room models.BattleRoom

	if err := config.DB.First(&room, roomID).Error; err != nil {
		return false, "Room tidak ditemukan", 404
	}

	if room.Status != "PLAYING" {
		return false, "Battle tidak sedang berlangsung", 400
	}

	if room.HostPlayerID != playerID &&
		(room.GuestPlayerID == nil || *room.GuestPlayerID != playerID) {
		return false, "Player bukan peserta battle", 403
	}

	var roomQuestion models.BattleRoomQuestion

	if err := config.DB.
		Where("room_id = ? AND question_id = ?", roomID, questionID).
		First(&roomQuestion).Error; err != nil {
		return false, "Pertanyaan bukan bagian dari battle", 400
	}

	var option models.BattleQuestionOption

	if err := config.DB.First(&option, selectedOptionID).Error; err != nil {
		return false, "Pilihan jawaban tidak ditemukan", 400
	}

	answer := models.BattleAnswer{
		RoomID:           roomID,
		PlayerID:         playerID,
		QuestionID:       questionID,
		SelectedOptionID: selectedOptionID,
		IsCorrect:        option.IsCorrect,
		AnsweredAt:       time.Now(),
	}

	if err := config.DB.Create(&answer).Error; err != nil {
		return false, "Gagal menyimpan jawaban", 500
	}

	return option.IsCorrect, "", 200
}
