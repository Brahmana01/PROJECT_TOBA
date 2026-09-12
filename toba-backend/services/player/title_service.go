package player

import (
	"toba-backend/config"
	"toba-backend/models"
)

func UpdatePlayerTitle(currentPlayer *models.Player) error {
	var title models.Title

	if err := config.DB.
		Where("minimum_level <= ?", currentPlayer.Level).
		Order("minimum_level DESC").
		First(&title).Error; err != nil {
		return err
	}

	currentPlayer.TitleID = int(title.ID)

	return config.DB.Save(currentPlayer).Error
}
