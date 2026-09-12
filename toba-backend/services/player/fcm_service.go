package player

import (
	"toba-backend/config"
	"toba-backend/models"
)

func UpdateFCMToken(playerID uint, token string) error {
	var player models.Player

	if err := config.DB.First(&player, playerID).Error; err != nil {
		return err
	}

	player.FCMToken = token

	return config.DB.Save(&player).Error
}
