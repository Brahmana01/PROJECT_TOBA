package battle

import (
	"errors"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

type BattleAnswerResult struct {
	IsCorrect       bool   `json:"is_correct"`
	CorrectOptionID uint   `json:"correct_option_id"`
	Explanation     string `json:"explanation"`
	HostScore       int64  `json:"host_score"`
	GuestScore      int64  `json:"guest_score"`
	AnsweredCount   int64  `json:"answered_count"`
	TotalQuestions  int    `json:"total_questions"`
}

func battleScores(tx *gorm.DB, room models.BattleRoom) (int64, int64, error) {
	var hostScore int64
	if err := tx.Model(&models.BattleAnswer{}).
		Where("room_id = ? AND player_id = ? AND is_correct = ?", room.ID, room.HostPlayerID, true).
		Count(&hostScore).Error; err != nil {
		return 0, 0, err
	}

	var guestScore int64
	if room.GuestPlayerID != nil {
		if err := tx.Model(&models.BattleAnswer{}).
			Where("room_id = ? AND player_id = ? AND is_correct = ?", room.ID, *room.GuestPlayerID, true).
			Count(&guestScore).Error; err != nil {
			return 0, 0, err
		}
	}

	return hostScore, guestScore, nil
}

// SubmitBattleAnswer menerima ID opsi, bukan nilai benar/salah dari client.
// selectedOptionID=0 berarti waktu habis dan dicatat sebagai jawaban salah.
func SubmitBattleAnswer(
	roomID uint,
	playerID uint,
	questionID uint,
	selectedOptionID uint,
	answerTimeMs int64,
) (*BattleAnswerResult, string, int) {
	if roomID == 0 || playerID == 0 || questionID == 0 {
		return nil, "Data jawaban tidak lengkap", 400
	}
	if answerTimeMs < 200 || answerTimeMs > 16000 {
		return nil, "Waktu jawaban tidak valid", 400
	}

	var response BattleAnswerResult
	var businessStatus int
	var businessMessage string

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&room, roomID).Error; err != nil {
			return err
		}

		if room.Status != "PLAYING" {
			businessStatus, businessMessage = 409, "Battle tidak sedang berlangsung"
			return errors.New(businessMessage)
		}

		isHost := room.HostPlayerID == playerID
		isGuest := room.GuestPlayerID != nil && *room.GuestPlayerID == playerID
		if !isHost && !isGuest {
			businessStatus, businessMessage = 403, "Player bukan peserta battle"
			return errors.New(businessMessage)
		}

		var assigned models.BattleRoomQuestion
		if err := tx.Where("room_id = ? AND question_id = ?", roomID, questionID).First(&assigned).Error; err != nil {
			if errors.Is(err, gorm.ErrRecordNotFound) {
				businessStatus, businessMessage = 400, "Pertanyaan bukan bagian dari battle"
				return errors.New(businessMessage)
			}
			return err
		}

		// Retry dengan opsi yang sama bersifat idempotent.
		var existing models.BattleAnswer
		existingErr := tx.Where("room_id = ? AND player_id = ? AND question_id = ?", roomID, playerID, questionID).First(&existing).Error
		if existingErr == nil {
			if existing.SelectedOptionID != selectedOptionID {
				businessStatus, businessMessage = 409, "Pertanyaan sudah pernah dijawab dengan pilihan berbeda"
				return errors.New(businessMessage)
			}
			response.IsCorrect = existing.IsCorrect
		} else if !errors.Is(existingErr, gorm.ErrRecordNotFound) {
			return existingErr
		} else {
			var answeredCount int64
			if err := tx.Model(&models.BattleAnswer{}).
				Where("room_id = ? AND player_id = ?", roomID, playerID).
				Count(&answeredCount).Error; err != nil {
				return err
			}
			if int64(assigned.QuestionOrder) != answeredCount+1 {
				businessStatus, businessMessage = 409, "Jawab soal Battle sesuai urutan"
				return errors.New(businessMessage)
			}

			var options []models.BattleQuestionOption
			if err := tx.Where("question_id = ?", questionID).Find(&options).Error; err != nil {
				return err
			}
			if len(options) != 4 {
				businessStatus, businessMessage = 409, "Soal Battle tidak memiliki tepat 4 opsi"
				return errors.New(businessMessage)
			}

			correctCount := 0
			for _, option := range options {
				if option.IsCorrect {
					correctCount++
					response.CorrectOptionID = option.ID
				}
			}
			if correctCount != 1 {
				businessStatus, businessMessage = 409, "Soal Battle harus memiliki tepat 1 jawaban benar"
				return errors.New(businessMessage)
			}

			isCorrect := false
			if selectedOptionID != 0 {
				var selected models.BattleQuestionOption
				if err := tx.Where("id = ? AND question_id = ?", selectedOptionID, questionID).First(&selected).Error; err != nil {
					if errors.Is(err, gorm.ErrRecordNotFound) {
						businessStatus, businessMessage = 400, "Pilihan jawaban bukan milik pertanyaan"
						return errors.New(businessMessage)
					}
					return err
				}
				isCorrect = selected.IsCorrect
			}

			answer := models.BattleAnswer{
				RoomID: roomID, PlayerID: playerID, QuestionID: questionID,
				SelectedOptionID: selectedOptionID, IsCorrect: isCorrect, AnsweredAt: time.Now(),
			}
			if err := tx.Create(&answer).Error; err != nil {
				return err
			}
			response.IsCorrect = isCorrect
		}

		// Penjelasan baru dikirim setelah jawaban diterima server.
		var answeredQuestion models.BattleQuestion
		if err := tx.First(&answeredQuestion, questionID).Error; err != nil {
			return err
		}
		response.Explanation = answeredQuestion.Explanation

		// Pastikan correct option tetap dikembalikan pada retry.
		if response.CorrectOptionID == 0 {
			var correct models.BattleQuestionOption
			if err := tx.Where("question_id = ? AND is_correct = ?", questionID, true).First(&correct).Error; err != nil {
				return err
			}
			response.CorrectOptionID = correct.ID
		}

		var err error
		response.HostScore, response.GuestScore, err = battleScores(tx, room)
		if err != nil {
			return err
		}
		if err := tx.Model(&models.BattleAnswer{}).
			Where("room_id = ? AND player_id = ?", roomID, playerID).
			Count(&response.AnsweredCount).Error; err != nil {
			return err
		}
		response.TotalQuestions = models.BattleQuestionCount
		return nil
	})

	if err != nil {
		if businessStatus != 0 {
			return nil, businessMessage, businessStatus
		}
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Room atau data soal tidak ditemukan", 404
		}
		return nil, "Gagal menyimpan jawaban Battle", 500
	}
	return &response, "", 200
}
