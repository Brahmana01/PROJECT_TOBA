package location

import (
	"toba-backend/config"
	"toba-backend/models"
	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetLocationQuiz(
	locationID string,
) (*models.Location, []models.LocationQuestion, string, int) {

	id, err := GetLocationID(locationID)

	if err != nil {
		return nil, nil, "ID lokasi tidak valid", fiber.StatusBadRequest
	}

	var location models.Location

	if err := config.DB.
		Where("id = ? AND is_active = ?", id, true).
		First(&location).Error; err != nil {

		return nil, nil, "Lokasi tidak ditemukan", fiber.StatusNotFound
	}

	var questions []models.LocationQuestion

	result := config.DB.
		Preload("Options").
		Where(
			"location_id = ? AND is_active = ?",
			id,
			true,
		).
		Order("RANDOM()").
		Limit(location.QuestionCount).
		Find(&questions)

	if result.Error != nil {
		return nil, nil, "Gagal mengambil soal", fiber.StatusInternalServerError
	}

	if len(questions) == 0 {
		return nil, nil, "Soal belum tersedia untuk lokasi ini", fiber.StatusNotFound
	}

	return &location, questions, "", fiber.StatusOK
}

func SubmitLocationQuiz(
	playerID uint,
	locationID uint,
	correctCount int,
) (fiber.Map, string, int) {

	var location models.Location

	if err := config.DB.
		Where("id = ? AND is_active = ?", locationID, true).
		First(&location).Error; err != nil {

		return nil, "Lokasi tidak ditemukan", fiber.StatusNotFound
	}

	minimumCorrect := location.MinimumCorrect

	if minimumCorrect <= 0 {
		minimumCorrect = 1
	}

	passed := correctCount >= minimumCorrect

	if !passed {
		return fiber.Map{
			"location_id":     location.ID,
			"correct_count":   correctCount,
			"total_questions": location.QuestionCount,
			"minimum_correct": minimumCorrect,
			"passed":          false,
			"reward_xp":       0,
			"reward_stars":    0,
			"is_new_pass":     false,
		}, "", fiber.StatusOK
	}

	isNewPass, err := SaveLocationProgress(
		playerID,
		location.ID,
		location.RewardXP,
		location.RewardStars,
	)

	if err != nil {
		return nil, "Gagal menyimpan progress lokasi", fiber.StatusInternalServerError
	}

	if err := UnlockNextLocation(
		playerID,
		location.LevelOrder,
	); err != nil {
		return nil, "Gagal membuka lokasi berikutnya", fiber.StatusInternalServerError
	}

	var currentPlayer models.Player

	if err := config.DB.First(&currentPlayer, playerID).Error; err != nil {
		return nil, "Player tidak ditemukan", fiber.StatusNotFound
	}

	newLevel := (currentPlayer.XP / 500) + 1

	if newLevel > currentPlayer.Level {
		currentPlayer.Level = newLevel
		player.UpdatePlayerTitle(&currentPlayer)
		config.DB.Save(&currentPlayer)
	}

	return fiber.Map{
		"location_id":     location.ID,
		"correct_count":   correctCount,
		"total_questions": location.QuestionCount,
		"minimum_correct": minimumCorrect,
		"passed":          true,
		"reward_xp":       location.RewardXP,
		"reward_stars":    location.RewardStars,
		"is_new_pass":     isNewPass,
		"level":           currentPlayer.Level,
		"total_xp":        currentPlayer.XP,
		"total_stars":     currentPlayer.Stars,
	}, "", fiber.StatusOK
}