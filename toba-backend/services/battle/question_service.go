package battle

import (
	"toba-backend/config"
	"toba-backend/models"
)

func GetRoomBattleQuestions(roomCode string) []models.BattleQuestion {
	var room models.BattleRoom

	if err := config.DB.
		Where("room_code = ?", roomCode).
		First(&room).Error; err != nil {
		return []models.BattleQuestion{}
	}

	return GetRoomBattleQuestionsFromDB(room.ID)
}

func GetRoomBattleQuestionsFromDB(roomID uint) []models.BattleQuestion {
	var roomQuestions []models.BattleRoomQuestion

	if err := config.DB.
		Where("room_id = ?", roomID).
		Order("question_order ASC").
		Find(&roomQuestions).Error; err != nil {
		return []models.BattleQuestion{}
	}

	questions := make([]models.BattleQuestion, 0, len(roomQuestions))

	for _, roomQuestion := range roomQuestions {
		var question models.BattleQuestion

		if err := config.DB.
			Preload("Options").
			First(&question, roomQuestion.QuestionID).
			Error; err != nil {
			continue
		}

		questions = append(questions, question)
	}

	return questions
}

func AssignBattleQuestions(roomID uint) {
	var questions []models.BattleQuestion

	if err := config.DB.
		Preload("Options").
		Where("is_active = ?", true).
		Order("RANDOM()").
		Limit(5).
		Find(&questions).Error; err != nil {
		return
	}

	if len(questions) == 0 {
		return
	}

	config.DB.
		Where("room_id = ?", roomID).
		Delete(&models.BattleRoomQuestion{})

	for index, question := range questions {
		roomQuestion := models.BattleRoomQuestion{
			RoomID:        roomID,
			QuestionID:    question.ID,
			QuestionOrder: index + 1,
		}

		config.DB.Create(&roomQuestion)
	}
}