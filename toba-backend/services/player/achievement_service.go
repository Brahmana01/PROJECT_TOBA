package player

import (
	"errors"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

// ClaimAchievementResult berisi hasil klaim reward.
type ClaimAchievementResult struct {
	RewardXP       int    `json:"reward_xp"`
	RewardStars    int    `json:"reward_stars"`
	PreviousLevel  int    `json:"previous_level"`
	Level          int    `json:"level"`
	LeveledUp      bool   `json:"leveled_up"`
	TotalXP        int    `json:"total_xp"`
	TotalStars     int    `json:"total_stars"`
	XPForNext      int    `json:"xp_for_next"`
	XPProgress     int    `json:"xp_progress"`
	TitleName      string `json:"title_name"`
	AlreadyClaimed bool   `json:"-"`
}

// ==========================================
// GET PLAYER ACHIEVEMENTS
// ==========================================

func GetPlayerAchievements(playerID uint) ([]models.PlayerAchievement, error) {

	achievements := make([]models.PlayerAchievement, 0)

	if err := config.DB.
		Preload("Achievement").
		Where("player_id = ?", playerID).
		Order("obtained_at ASC").
		Find(&achievements).Error; err != nil {

		return nil, err
	}

	return achievements, nil
}

// ==========================================
// CLAIM ACHIEVEMENT
// ==========================================

func ClaimAchievement(
	playerID uint,
	achievementID uint,
) (*ClaimAchievementResult, error) {

	result := &ClaimAchievementResult{}

	err := config.DB.Transaction(func(tx *gorm.DB) error {

		// Lock player untuk mencegah klaim bersamaan.
		var currentPlayer models.Player

		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&currentPlayer, playerID).Error; err != nil {

			return err
		}

		// Ambil achievement yang sudah terbuka.
		var playerAchievement models.PlayerAchievement

		if err := tx.
			Preload("Achievement").
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where(
				"player_id = ? AND achievement_id = ?",
				playerID,
				achievementID,
			).
			First(&playerAchievement).Error; err != nil {

			return err
		}

		if playerAchievement.Achievement.ID == 0 {
			return gorm.ErrRecordNotFound
		}

		// Cegah reward diberikan dua kali.
		if playerAchievement.Claimed {

			result.AlreadyClaimed = true
			result.PreviousLevel = currentPlayer.Level
			result.Level = currentPlayer.Level
			result.TotalXP = currentPlayer.XP
			result.TotalStars = currentPlayer.Stars

			xpPerLevel, settingErr := GetXPPerLevel(tx)
			if settingErr != nil {
				return settingErr
			}
			result.XPForNext = xpPerLevel
			result.XPProgress = currentPlayer.XP % xpPerLevel

			if currentPlayer.TitleID > 0 {
				var currentTitle models.Title
				titleErr := tx.First(&currentTitle, currentPlayer.TitleID).Error
				if titleErr == nil {
					result.TitleName = currentTitle.TitleName
				} else if !errors.Is(titleErr, gorm.ErrRecordNotFound) {
					return titleErr
				}
			}

			return nil
		}

		// Terapkan reward melalui progression service pusat.
		rewardXP := playerAchievement.Achievement.RewardXP
		rewardStars := playerAchievement.Achievement.RewardStars

		progression, err := ApplyRewardToLockedPlayer(
			tx,
			&currentPlayer,
			rewardXP,
			rewardStars,
		)
		if err != nil {
			return err
		}

		// Tandai achievement sudah diklaim.
		now := time.Now()

		playerAchievement.Claimed = true
		playerAchievement.ClaimedAt = &now

		if err := tx.Save(&playerAchievement).Error; err != nil {
			return err
		}

		// Susun hasil.
		result.RewardXP = progression.RewardXP
		result.RewardStars = progression.RewardStars
		result.PreviousLevel = progression.PreviousLevel
		result.Level = progression.Level
		result.LeveledUp = progression.LeveledUp
		result.TotalXP = progression.TotalXP
		result.TotalStars = progression.TotalStars
		result.XPForNext = progression.XPForNext
		result.XPProgress = progression.XPProgress
		result.TitleName = progression.TitleName

		return nil
	})

	if err != nil {
		return nil, err
	}

	return result, nil
}

// ==========================================
// CHECK AND UNLOCK ACHIEVEMENTS
// ==========================================

func CheckAndUnlockAchievements(
	playerID uint,
) ([]models.Achievement, error) {

	newlyUnlocked := make([]models.Achievement, 0)

	err := config.DB.Transaction(func(tx *gorm.DB) error {

		// Lock player agar tidak terjadi unlock duplikat
		// ketika beberapa request berjalan bersamaan.
		var currentPlayer models.Player

		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&currentPlayer, playerID).Error; err != nil {

			return err
		}

		// Ambil seluruh achievement aktif.
		var achievements []models.Achievement

		if err := tx.
			Where("is_active = ?", true).
			Order("id ASC").
			Find(&achievements).Error; err != nil {

			return err
		}

		// ======================================
		// HITUNG STATISTIK PEMAIN
		// ======================================

		var locationCount int64

		if err := tx.
			Model(&models.PlayerLocationProgress{}).
			Where(
				"player_id = ? AND is_completed = ?",
				playerID,
				true,
			).
			Count(&locationCount).Error; err != nil {

			return err
		}

		var triviaPlayCount int64

		if err := tx.
			Model(&models.PlayerTriviaHistory{}).
			Where("player_id = ?", playerID).
			Count(&triviaPlayCount).Error; err != nil {

			return err
		}

		var triviaCorrectCount int64

		if err := tx.
			Model(&models.PlayerTriviaHistory{}).
			Select("COALESCE(SUM(correct_answer), 0)").
			Where("player_id = ?", playerID).
			Scan(&triviaCorrectCount).Error; err != nil {

			return err
		}

		var battleWinCount int64

		if err := tx.
			Model(&models.BattleRoom{}).
			Where("winner_player_id = ?", playerID).
			Count(&battleWinCount).Error; err != nil {

			return err
		}

		var activeLocationCount int64
		if err := tx.Model(&models.Location{}).
			Where("is_active = ? AND deleted_at IS NULL", true).
			Count(&activeLocationCount).Error; err != nil {
			return err
		}

		var completedActiveLocationCount int64
		if err := tx.Model(&models.PlayerLocationProgress{}).
			Joins("JOIN locations ON locations.id = player_location_progresses.location_id").
			Where(
				"player_location_progresses.player_id = ? AND player_location_progresses.is_completed = ? AND locations.is_active = ? AND locations.deleted_at IS NULL",
				playerID,
				true,
				true,
			).
			Count(&completedActiveLocationCount).Error; err != nil {
			return err
		}

		// ======================================
		// EVALUASI SETIAP ACHIEVEMENT
		// ======================================

		for _, achievement := range achievements {

			// Periksa apakah sudah pernah terbuka.
			var existing models.PlayerAchievement

			existingErr := tx.
				Where(
					"player_id = ? AND achievement_id = ?",
					playerID,
					achievement.ID,
				).
				First(&existing).Error

			if existingErr == nil {
				continue
			}

			if !errors.Is(existingErr, gorm.ErrRecordNotFound) {
				return existingErr
			}

			unlocked := false

			conditionType := strings.ToUpper(
				strings.TrimSpace(achievement.ConditionType),
			)

			required := int64(achievement.ConditionValue)

			switch conditionType {

			case "LEVEL":

				unlocked = int64(currentPlayer.Level) >= required

			case "XP":

				unlocked = int64(currentPlayer.XP) >= required

			case "STAR", "STARS":

				unlocked = int64(currentPlayer.Stars) >= required

			case "LOCATION":

				unlocked = locationCount >= required

			case "ALL_LOCATION":

				unlocked = activeLocationCount > 0 &&
					completedActiveLocationCount >= activeLocationCount

			case "TRIVIA":

				unlocked = triviaPlayCount >= required

			case "TRIVIA_CORRECT":

				unlocked = triviaCorrectCount >= required

			case "BATTLE", "BATTLE_WIN":

				unlocked = battleWinCount >= required

			case "ALL_ACHIEVEMENT":

				var unlockedCount int64

				if err := tx.
					Model(&models.PlayerAchievement{}).
					Where(
						"player_id = ? AND achievement_id <> ?",
						playerID,
						achievement.ID,
					).
					Count(&unlockedCount).Error; err != nil {

					return err
				}

				unlocked = unlockedCount >= required
			}

			if !unlocked {
				continue
			}

			// Simpan achievement baru.
			playerAchievement := models.PlayerAchievement{
				PlayerID:      playerID,
				AchievementID: achievement.ID,
				Claimed:       false,
			}

			if err := tx.Create(&playerAchievement).Error; err != nil {
				return err
			}

			// Masukkan ke daftar achievement yang baru terbuka.
			newlyUnlocked = append(
				newlyUnlocked,
				achievement,
			)
		}

		return nil
	})

	if err != nil {
		return nil, err
	}

	return newlyUnlocked, nil
}
