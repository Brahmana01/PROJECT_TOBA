package player

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
)

type PlayerGalleryCardResponse struct {
	ID            uint               `json:"id"`
	GalleryCardID uint               `json:"gallery_card_id"`
	ObtainedAt    *time.Time         `json:"obtained_at,omitempty"`
	IsOwned       bool               `json:"is_owned"`
	Card          models.GalleryCard `json:"card"`
}

// GetPlayerGallery mengembalikan seluruh katalog kartu dari lokasi aktif.
// Status kepemilikan ditambahkan agar client dapat menggambar kartu terkunci.
func GetPlayerGallery(playerID uint) ([]PlayerGalleryCardResponse, error) {
	var catalogue []models.GalleryCard
	if err := config.DB.
		Model(&models.GalleryCard{}).
		Joins("JOIN locations ON locations.id = gallery_cards.location_id").
		Where("locations.is_active = ? AND locations.deleted_at IS NULL", true).
		Order("gallery_cards.location_id ASC").
		Order("gallery_cards.id ASC").
		Find(&catalogue).Error; err != nil {
		return nil, err
	}

	var ownedCards []models.PlayerGalleryCard
	if err := config.DB.
		Where("player_id = ?", playerID).
		Find(&ownedCards).Error; err != nil {
		return nil, err
	}

	ownedByCardID := make(map[uint]models.PlayerGalleryCard, len(ownedCards))
	for _, owned := range ownedCards {
		ownedByCardID[owned.GalleryCardID] = owned
	}

	result := make([]PlayerGalleryCardResponse, 0, len(catalogue))
	for _, card := range catalogue {
		item := PlayerGalleryCardResponse{
			GalleryCardID: card.ID,
			IsOwned:       false,
			Card:          card,
		}

		if owned, ok := ownedByCardID[card.ID]; ok {
			obtainedAt := owned.ObtainedAt
			item.ID = owned.ID
			item.ObtainedAt = &obtainedAt
			item.IsOwned = true
		}

		result = append(result, item)
	}

	return result, nil
}
