package challenge

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetChallenges() ([]models.WeeklyChallenge, error) {
	var challenges []models.WeeklyChallenge

	result := config.DB.
		Order("created_at DESC").
		Find(&challenges)

	if result.Error != nil {
		return nil, result.Error
	}

	return challenges, nil
}

func AdminCreateChallenge(
	title string,
	description string,
	gameType string,
	rewardXP int,
	rewardStars int,
	dataJSON string,
	durationDays int,
) (*models.WeeklyChallenge, error) {

	config.DB.
		Model(&models.WeeklyChallenge{}).
		Where("is_active = ?", true).
		Update("is_active", false)

	duration := durationDays

	if duration <= 0 {
		duration = 7
	}

	now := time.Now()

	challenge := models.WeeklyChallenge{
		Title:       title,
		Description: description,
		GameType:    gameType,
		RewardXP:    rewardXP,
		RewardStars: rewardStars,
		DataJSON:    dataJSON,
		IsActive:    true,
		StartDate:   now,
		EndDate:     now.AddDate(0, 0, duration),
	}

	if err := config.DB.Create(&challenge).Error; err != nil {
		return nil, err
	}

	return &challenge, nil
}

func AdminUpdateChallenge(
	id string,
	title string,
	description string,
	rewardXP int,
	rewardStars int,
	dataJSON string,
	isActive bool,
) (*models.WeeklyChallenge, string, int) {

	var challenge models.WeeklyChallenge

	if err := config.DB.First(&challenge, id).Error; err != nil {
		return nil, "Tantangan tidak ditemukan", fiber.StatusNotFound
	}

	challenge.Title = title
	challenge.Description = description
	challenge.RewardXP = rewardXP
	challenge.RewardStars = rewardStars
	challenge.DataJSON = dataJSON
	challenge.IsActive = isActive

	if err := config.DB.Save(&challenge).Error; err != nil {
		return nil, "Gagal memperbarui tantangan", fiber.StatusInternalServerError
	}

	return &challenge, "", fiber.StatusOK
}

func AdminDeleteChallenge(id string) (string, int) {
	result := config.DB.Delete(&models.WeeklyChallenge{}, id)

	if result.Error != nil {
		return "Gagal menghapus tantangan", fiber.StatusInternalServerError
	}

	return "Tantangan berhasil dihapus", fiber.StatusOK
}