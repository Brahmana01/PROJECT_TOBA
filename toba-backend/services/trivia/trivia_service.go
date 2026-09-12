package trivia

import (
	"toba-backend/config"
	"toba-backend/models"
)

func GetTriviaCategories() ([]models.TriviaCategory, error) {
	var categories []models.TriviaCategory

	result := config.DB.
		Where("is_active = ? AND deleted_at IS NULL", true).
		Order("display_order ASC").
		Find(&categories)

	if result.Error != nil {
		return nil, result.Error
	}

	return categories, nil
}
