package player

import (
	"toba-backend/config"
	"toba-backend/models"
)

func GetPlayerGallery(playerID uint) ([]models.PlayerGalleryCard, error) {
	gallery := make([]models.PlayerGalleryCard, 0)

	if err := config.DB.
		Preload("Card").
		Where("player_id = ?", playerID).
		Find(&gallery).Error; err != nil {
		return nil, err
	}

	return gallery, nil
}
