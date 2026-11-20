package location

import (
	"errors"
	"strconv"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
)

func GetPlayerLocationProgress(playerID string) ([]models.PlayerLocationProgress, error) {
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

// SaveLocationProgress is retained for compatibility with older callers.
// New gameplay submission must use SubmitLocationQuiz so scoring and reward
// are calculated atomically in one transaction.
func SaveLocationProgress(playerID uint, locationID uint, rewardXP int, rewardStars int) (bool, error) {
	return false, errors.New("SaveLocationProgress tidak digunakan; gunakan SubmitLocationQuiz")
}

// UnlockNextLocation is retained for compatibility with older callers.
// The active gameplay flow unlocks the next location inside SubmitLocationQuiz.
func UnlockNextLocation(playerID uint, currentLevelOrder int) error {
	return errors.New("UnlockNextLocation tidak digunakan; unlock dilakukan di SubmitLocationQuiz")
}

func IsLocationUnlocked(db *gorm.DB, playerID uint, locationID uint) (bool, error) {
	if db == nil {
		return false, errors.New("database belum tersedia")
	}

	var location models.Location
	if err := db.First(&location, locationID).Error; err != nil {
		return false, err
	}

	if location.LevelOrder == 1 {
		return true, nil
	}

	var progress models.PlayerLocationProgress
	err := db.Where(
		"player_id = ? AND location_id = ?",
		playerID, locationID,
	).First(&progress).Error

	if errors.Is(err, gorm.ErrRecordNotFound) {
		return false, nil
	}
	if err != nil {
		return false, err
	}

	return progress.IsUnlocked, nil
}
