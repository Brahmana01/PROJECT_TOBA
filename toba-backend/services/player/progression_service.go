package player

import (
	"errors"

	"toba-backend/models"

	"gorm.io/gorm"
)

const DefaultXPPerLevel = 500

// ProgressionResult is the authoritative progression state after a reward is applied.
type ProgressionResult struct {
	RewardXP      int    `json:"reward_xp"`
	RewardStars   int    `json:"reward_stars"`
	PreviousLevel int    `json:"previous_level"`
	Level         int    `json:"level"`
	LeveledUp     bool   `json:"leveled_up"`
	TotalXP       int    `json:"total_xp"`
	TotalStars    int    `json:"total_stars"`
	XPForNext     int    `json:"xp_for_next"`
	XPProgress    int    `json:"xp_progress"`
	TitleName     string `json:"title_name"`
}

// GetXPPerLevel returns the locked gameplay threshold. GameSettings cannot override it.
func GetXPPerLevel(db *gorm.DB) (int, error) {
	// XP per level is a locked gameplay rule. A stale database setting must
	// never override the authoritative 500 XP threshold.
	return models.DefaultXPPerLevel, nil
}

// ApplyRewardToLockedPlayer applies XP and stars using the caller's transaction.
// The caller MUST lock the Player row with SELECT ... FOR UPDATE before calling this function.
// This keeps reward processing atomic with the activity that grants the reward.
func ApplyRewardToLockedPlayer(
	tx *gorm.DB,
	currentPlayer *models.Player,
	rewardXP int,
	rewardStars int,
) (*ProgressionResult, error) {
	if tx == nil {
		return nil, errors.New("transaksi database tidak tersedia")
	}
	if currentPlayer == nil || currentPlayer.ID == 0 {
		return nil, errors.New("data pemain tidak valid")
	}
	if rewardXP < 0 || rewardStars < 0 {
		return nil, errors.New("reward XP dan bintang tidak boleh negatif")
	}

	xpPerLevel, err := GetXPPerLevel(tx)
	if err != nil {
		return nil, err
	}

	previousLevel := currentPlayer.Level
	currentPlayer.XP += rewardXP
	currentPlayer.Stars += rewardStars
	currentPlayer.Level = (currentPlayer.XP / xpPerLevel) + 1

	var title models.Title
	titleErr := tx.
		Where("minimum_level <= ?", currentPlayer.Level).
		Order("minimum_level DESC").
		First(&title).Error

	titleName := ""
	if titleErr == nil {
		currentPlayer.TitleID = int(title.ID)
		titleName = title.TitleName
	} else if !errors.Is(titleErr, gorm.ErrRecordNotFound) {
		return nil, titleErr
	} else if currentPlayer.TitleID > 0 {
		// Keep the player's existing title if title seed data is temporarily unavailable.
		var existingTitle models.Title
		if err := tx.First(&existingTitle, currentPlayer.TitleID).Error; err == nil {
			titleName = existingTitle.TitleName
		} else if !errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, err
		}
	}

	if err := tx.Model(&models.Player{}).
		Where("id = ?", currentPlayer.ID).
		Updates(map[string]interface{}{
			"xp":       currentPlayer.XP,
			"stars":    currentPlayer.Stars,
			"level":    currentPlayer.Level,
			"title_id": currentPlayer.TitleID,
		}).Error; err != nil {
		return nil, err
	}

	xpProgress := currentPlayer.XP % xpPerLevel
	if xpProgress < 0 {
		xpProgress = 0
	}

	return &ProgressionResult{
		RewardXP:      rewardXP,
		RewardStars:   rewardStars,
		PreviousLevel: previousLevel,
		Level:         currentPlayer.Level,
		LeveledUp:     currentPlayer.Level > previousLevel,
		TotalXP:       currentPlayer.XP,
		TotalStars:    currentPlayer.Stars,
		XPForNext:     xpPerLevel,
		XPProgress:    xpProgress,
		TitleName:     titleName,
	}, nil
}
