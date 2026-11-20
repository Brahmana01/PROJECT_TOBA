package challenge

import (
	"errors"
	"time"

	"toba-backend/config"
	"toba-backend/models"
	playerprogression "toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

func GetActiveChallenge(playerID uint) fiber.Map {
	var challenge models.WeeklyChallenge

	now := time.Now()
	err := config.DB.
		Where("is_active = ? AND start_date <= ? AND end_date >= ?", true, now, now).
		Order("created_at DESC").
		First(&challenge).Error

	if err != nil {
		return fiber.Map{
			"has_challenge": false,
		}
	}

	isCompleted := false

	if playerID > 0 {
		var progress models.PlayerChallengeProgress

		err := config.DB.
			Where(
				"player_id = ? AND challenge_id = ?",
				playerID,
				challenge.ID,
			).
			First(&progress).Error

		if err == nil {
			isCompleted = progress.IsCompleted
		}
	}

	return fiber.Map{
		"has_challenge": true,
		"is_completed":  isCompleted,
		"challenge": fiber.Map{
			"id":           challenge.ID,
			"title":        challenge.Title,
			"description":  challenge.Description,
			"game_type":    challenge.GameType,
			"reward_xp":    challenge.RewardXP,
			"reward_stars": challenge.RewardStars,
			"data_json":    challenge.DataJSON,
		},
	}
}

func SubmitChallenge(
	playerID uint,
	challengeID uint,
	score int,
) (fiber.Map, string, int) {
	if playerID == 0 || challengeID == 0 {
		return nil, "Data pemain atau challenge tidak valid", fiber.StatusBadRequest
	}
	if score < 0 {
		return nil, "Skor tidak boleh negatif", fiber.StatusBadRequest
	}

	var response fiber.Map
	alreadyCompleted := false
	businessStatus := 0
	businessMessage := ""

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var challenge models.WeeklyChallenge
		if err := tx.First(&challenge, challengeID).Error; err != nil {
			return err
		}

		now := time.Now()
		if !challenge.IsActive || now.Before(challenge.StartDate) || now.After(challenge.EndDate) {
			businessStatus = fiber.StatusConflict
			businessMessage = "Weekly Challenge belum aktif atau sudah berakhir"
			return nil
		}

		// Lock the player so concurrent reward requests cannot overwrite XP/stars.
		var currentPlayer models.Player
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&currentPlayer, playerID).Error; err != nil {
			return err
		}

		var progress models.PlayerChallengeProgress
		progressErr := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("player_id = ? AND challenge_id = ?", playerID, challengeID).
			First(&progress).Error

		if progressErr == nil && progress.IsCompleted {
			alreadyCompleted = true
			return nil
		}
		if progressErr != nil && !errors.Is(progressErr, gorm.ErrRecordNotFound) {
			return progressErr
		}

		now := time.Now()
		if errors.Is(progressErr, gorm.ErrRecordNotFound) {
			progress = models.PlayerChallengeProgress{
				PlayerID:    playerID,
				ChallengeID: challengeID,
			}
		}

		progress.IsCompleted = true
		progress.Score = score
		progress.CompletedAt = &now

		if progress.ID == 0 {
			if err := tx.Create(&progress).Error; err != nil {
				return err
			}
		} else if err := tx.Save(&progress).Error; err != nil {
			return err
		}

		progression, err := playerprogression.ApplyRewardToLockedPlayer(
			tx,
			&currentPlayer,
			challenge.RewardXP,
			challenge.RewardStars,
		)
		if err != nil {
			return err
		}

		response = fiber.Map{
			"message":        "Tantangan selesai!",
			"reward_xp":      progression.RewardXP,
			"reward_stars":   progression.RewardStars,
			"previous_level": progression.PreviousLevel,
			"level":          progression.Level,
			"leveled_up":     progression.LeveledUp,
			"total_xp":       progression.TotalXP,
			"total_stars":    progression.TotalStars,
			"xp_for_next":    progression.XPForNext,
			"xp_progress":    progression.XPProgress,
			"title_name":     progression.TitleName,
		}

		return nil
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Tantangan atau pemain tidak ditemukan", fiber.StatusNotFound
		}
		return nil, "Gagal menyimpan hasil tantangan", fiber.StatusInternalServerError
	}

	if businessStatus != 0 {
		return nil, businessMessage, businessStatus
	}

	if alreadyCompleted {
		return nil, "Tantangan sudah pernah diselesaikan", fiber.StatusBadRequest
	}

	return response, "", fiber.StatusOK
}
