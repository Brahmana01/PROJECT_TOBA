package player

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
)

func GetPlayerAchievements(playerID uint) ([]models.PlayerAchievement, error) {
	var achievements []models.PlayerAchievement

	if err := config.DB.
		Preload("Achievement").
		Where("player_id = ?", playerID).
		Find(&achievements).Error; err != nil {
		return nil, err
	}

	return achievements, nil
}

func ClaimAchievement(playerID uint, achievementID uint) error {
	var achievement models.PlayerAchievement

	if err := config.DB.
		Where(
			"player_id = ? AND achievement_id = ?",
			playerID,
			achievementID,
		).
		First(&achievement).Error; err != nil {
		return err
	}

	if achievement.Claimed {
		return nil
	}

	now := time.Now()

	achievement.Claimed = true
	achievement.ClaimedAt = &now

	return config.DB.Save(&achievement).Error
}

func CheckAndUnlockAchievements(playerID uint) error {
	var currentPlayer models.Player

	if err := config.DB.First(&currentPlayer, playerID).Error; err != nil {
		return err
	}

	var achievements []models.Achievement

	if err := config.DB.
		Where("is_active = ?", true).
		Find(&achievements).Error; err != nil {
		return err
	}

	for _, achievement := range achievements {
		var existing models.PlayerAchievement

		err := config.DB.
			Where(
				"player_id = ? AND achievement_id = ?",
				playerID,
				achievement.ID,
			).
			First(&existing).Error

		if err == nil {
			continue
		}

		unlocked := false

		switch achievement.ConditionType {
		case "LEVEL":
			unlocked = currentPlayer.Level >= achievement.ConditionValue

		case "XP":
			unlocked = currentPlayer.XP >= achievement.ConditionValue

		case "STAR":
			unlocked = currentPlayer.Stars >= achievement.ConditionValue

		case "LOCATION":
			var count int64

			config.DB.
				Model(&models.PlayerLocationProgress{}).
				Where(
					"player_id = ? AND is_completed = ?",
					playerID,
					true,
				).
				Count(&count)

			unlocked = count >= int64(achievement.ConditionValue)

		case "TRIVIA":
			var count int64

			config.DB.
				Model(&models.PlayerTriviaHistory{}).
				Where(
					"player_id = ?",
					playerID,
				).
				Count(&count)

			unlocked = count >= int64(achievement.ConditionValue)

		case "BATTLE":
			var count int64

			config.DB.
				Model(&models.BattleRoom{}).
				Where(
					"winner_player_id = ?",
					playerID,
				).
				Count(&count)

			unlocked = count >= int64(achievement.ConditionValue)
		}

		if !unlocked {
			continue
		}

		playerAchievement := models.PlayerAchievement{
			PlayerID:      playerID,
			AchievementID: achievement.ID,
			Claimed:       false,
		}

		if err := config.DB.Create(&playerAchievement).Error; err != nil {
			return err
		}
	}

	return nil
}
