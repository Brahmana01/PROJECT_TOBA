package trivia

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetTriviaQuiz(categoryID string) (*models.TriviaCategory, []models.TriviaQuestion, string, int) {
	var category models.TriviaCategory

	if err := config.DB.First(&category, categoryID).Error; err != nil {
		return nil, nil, "Kategori tidak ditemukan", fiber.StatusNotFound
	}

	var questions []models.TriviaQuestion

	config.DB.
		Preload("Options").
		Where("category_id = ? AND is_active = ?", categoryID, true).
		Order("RANDOM()").
		Limit(category.QuestionCount).
		Find(&questions)

	if len(questions) == 0 {
		return nil, nil, "Soal belum tersedia untuk kategori ini", fiber.StatusNotFound
	}

	return &category, questions, "", fiber.StatusOK
}

func SubmitTrivia(
	playerID uint,
	categoryID uint,
	correct int,
	wrong int,
) (fiber.Map, string, int) {

	var currentPlayer models.Player

	if err := config.DB.First(&currentPlayer, playerID).Error; err != nil {
		return nil, "Player tidak ditemukan", fiber.StatusNotFound
	}

	xpGained := correct * 10
	currentPlayer.XP += xpGained

	newLevel := (currentPlayer.XP / 500) + 1

	if newLevel > currentPlayer.Level {
		currentPlayer.Level = newLevel
		player.UpdatePlayerTitle(&currentPlayer)
	}

	if err := config.DB.Save(&currentPlayer).Error; err != nil {
		return nil, "Gagal menyimpan data player", fiber.StatusInternalServerError
	}

	history := models.PlayerTriviaHistory{
		PlayerID:      playerID,
		CategoryID:    categoryID,
		CorrectAnswer: correct,
		WrongAnswer:   wrong,
		XPEarned:      xpGained,
		PlayedAt:      time.Now(),
	}

	if err := config.DB.Create(&history).Error; err != nil {
		return nil, "Gagal menyimpan riwayat trivia", fiber.StatusInternalServerError
	}

	return fiber.Map{
		"xp":          xpGained,
		"level":       currentPlayer.Level,
		"total_xp":    currentPlayer.XP,
		"total_stars": currentPlayer.Stars,
	}, "", fiber.StatusOK
}
