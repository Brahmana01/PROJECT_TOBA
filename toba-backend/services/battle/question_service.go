package battle

import (
	"errors"
	"fmt"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
)

type BattleOptionResponse struct {
	ID         uint   `json:"id"`
	OptionText string `json:"option_text"`
}

type BattleQuestionResponse struct {
	ID       uint                   `json:"id"`
	Question string                 `json:"question"`
	Options  []BattleOptionResponse `json:"options"`
}

func GetRoomBattleQuestions(roomCode string) []BattleQuestionResponse {
	var room models.BattleRoom

	if err := config.DB.
		Where("room_code = ?", roomCode).
		First(&room).Error; err != nil {
		return nil
	}

	return GetRoomBattleQuestionsFromDB(room.ID)
}

func GetRoomBattleQuestionsFromDB(
	roomID uint,
) []BattleQuestionResponse {

	if roomID == 0 {
		return nil
	}

	var roomQuestions []models.BattleRoomQuestion

	if err := config.DB.
		Where("room_id = ?", roomID).
		Order("question_order ASC").
		Find(&roomQuestions).Error; err != nil {
		return nil
	}

	if len(roomQuestions) != models.BattleQuestionCount {
		return nil
	}

	responses := make([]BattleQuestionResponse, 0, len(roomQuestions))

	for _, roomQuestion := range roomQuestions {
		var question models.BattleQuestion

		if err := config.DB.
			Preload("Options").
			First(&question, roomQuestion.QuestionID).Error; err != nil {
			return nil
		}

		if len(question.Options) != 4 {
			return nil
		}

		correctCount := 0
		options := make([]BattleOptionResponse, 0, 4)

		for _, option := range question.Options {
			if option.IsCorrect {
				correctCount++
			}

			options = append(options, BattleOptionResponse{
				ID:         option.ID,
				OptionText: option.OptionText,
			})
		}

		if correctCount != 1 {
			return nil
		}

		responses = append(responses, BattleQuestionResponse{
			ID:       question.ID,
			Question: question.Question,
			Options:  options,
		})
	}

	return responses
}

func assignBattleQuestionsTx(
	tx *gorm.DB,
	roomID uint,
) ([]BattleQuestionResponse, error) {

	// Bersihkan penugasan lama hanya untuk room yang sedang
	// disiapkan sebelum pertandingan dimulai.
	if err := tx.
		Where("room_id = ?", roomID).
		Delete(&models.BattleRoomQuestion{}).Error; err != nil {
		return nil, err
	}

	var activeCount int64

	if err := tx.Model(&models.BattleQuestion{}).
		Where("is_active = ?", true).
		Count(&activeCount).Error; err != nil {
		return nil, err
	}

	if activeCount < int64(models.BattleQuestionCount) {
		return nil, fmt.Errorf(
			"bank Battle harus memiliki minimal %d soal aktif; ditemukan %d",
			models.BattleQuestionCount,
			activeCount,
		)
	}

	var questions []models.BattleQuestion

	if err := tx.
		Preload("Options").
		Where("is_active = ?", true).
		Order("RANDOM()").
		Limit(models.BattleQuestionCount).
		Find(&questions).Error; err != nil {
		return nil, err
	}

	if len(questions) != models.BattleQuestionCount {
		return nil, errors.New("gagal mengambil sepuluh soal Battle")
	}

	responses := make([]BattleQuestionResponse, 0, len(questions))

	for index, question := range questions {
		if len(question.Options) != 4 {
			return nil, fmt.Errorf(
				"soal Battle %d harus memiliki tepat 4 opsi",
				question.ID,
			)
		}

		correctCount := 0
		options := make([]BattleOptionResponse, 0, 4)

		for _, option := range question.Options {
			if option.IsCorrect {
				correctCount++
			}

			options = append(options, BattleOptionResponse{
				ID:         option.ID,
				OptionText: option.OptionText,
			})
		}

		if correctCount != 1 {
			return nil, fmt.Errorf(
				"soal Battle %d harus memiliki tepat 1 jawaban benar",
				question.ID,
			)
		}

		responses = append(responses, BattleQuestionResponse{
			ID:       question.ID,
			Question: question.Question,
			Options:  options,
		})

		roomQuestion := models.BattleRoomQuestion{
			RoomID:        roomID,
			QuestionID:    question.ID,
			QuestionOrder: index + 1,
		}

		if err := tx.Create(&roomQuestion).Error; err != nil {
			return nil, err
		}
	}

	return responses, nil
}

func AssignBattleQuestions(roomID uint) error {
	if roomID == 0 {
		return errors.New("room ID tidak valid")
	}

	return config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom

		if err := tx.
			First(&room, roomID).Error; err != nil {
			return err
		}

		if room.Status != "WAITING" {
			return errors.New("soal hanya dapat ditetapkan sebelum Battle dimulai")
		}

		var existingCount int64

		if err := tx.Model(&models.BattleRoomQuestion{}).
			Where("room_id = ?", roomID).
			Count(&existingCount).Error; err != nil {
			return err
		}

		if existingCount > 0 {
			return errors.New("soal Battle sudah pernah ditetapkan")
		}

		_, err := assignBattleQuestionsTx(tx, roomID)
		return err
	})
}
