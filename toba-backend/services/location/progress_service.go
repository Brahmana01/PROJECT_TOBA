package location

import (
	"strconv"

	"toba-backend/config"
	"toba-backend/models"
)

func GetPlayerLocationProgress(
	playerID string,
) ([]models.PlayerLocationProgress, error) {

	id, err := strconv.ParseUint(playerID, 10, 64)
	if err != nil {
		return nil, err
	}

	var progress []models.PlayerLocationProgress

	if err := config.DB.
		Where("player_id = ?", uint(id)).
		Order("location_id ASC").
		Find(&progress).Error; err != nil {
		return nil, err
	}

	return progress, nil
}

func SaveLocationProgress(
	playerID uint,
	locationID uint,
	rewardXP int,
	rewardStars int,
) (bool, error) {

	var progress models.PlayerLocationProgress

	err := config.DB.
		Where(
			"player_id = ? AND location_id = ?",
			playerID,
			locationID,
		).
		First(&progress).Error

	isNewPass := false

	if err != nil {
		progress = models.PlayerLocationProgress{
			PlayerID:   playerID,
			LocationID: locationID,
		}

		isNewPass = true
	}

	wasCompleted := progress.IsCompleted

	progress.IsCompleted = true

	if err := config.DB.Save(&progress).Error; err != nil {
		return false, err
	}

	if !wasCompleted {
		var currentPlayer models.Player

		if err := config.DB.
			First(&currentPlayer, playerID).
			Error; err != nil {
			return false, err
		}

		currentPlayer.XP += rewardXP
		currentPlayer.Stars += rewardStars

		if err := config.DB.Save(&currentPlayer).Error; err != nil {
			return false, err
		}

		isNewPass = true
	}

	return isNewPass, nil
}

func UnlockNextLocation(
	playerID uint,
	currentLevelOrder int,
) error {

	var nextLocation models.Location

	if err := config.DB.
		Where(
			"level_order > ? AND is_active = ?",
			currentLevelOrder,
			true,
		).
		Order("level_order ASC").
		First(&nextLocation).Error; err != nil {
		return nil
	}

	var existing models.PlayerLocationProgress

	err := config.DB.
		Where(
			"player_id = ? AND location_id = ?",
			playerID,
			nextLocation.ID,
		).
		First(&existing).Error

	if err == nil {
		return nil
	}

	progress := models.PlayerLocationProgress{
		PlayerID:    playerID,
		LocationID:  nextLocation.ID,
		IsUnlocked:  true,
		IsCompleted: false,
	}

	return config.DB.Create(&progress).Error
}
