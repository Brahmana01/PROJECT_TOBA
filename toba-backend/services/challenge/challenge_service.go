package challenge

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetActiveChallenge(playerID string) fiber.Map {
	var challenge models.WeeklyChallenge

	err := config.DB.
		Where("is_active = ? AND end_date > ?", true, time.Now()).
		Order("created_at DESC").
		First(&challenge).Error

	if err != nil {
		return fiber.Map{
			"has_challenge": false,
		}
	}

	isCompleted := false

	if playerID != "" {
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

	var challenge models.WeeklyChallenge

	if err := config.DB.First(&challenge, challengeID).Error; err != nil {
		return nil, "Tantangan tidak ditemukan", fiber.StatusNotFound
	}

	var progress models.PlayerChallengeProgress

	err := config.DB.
		Where(
			"player_id = ? AND challenge_id = ?",
			playerID,
			challengeID,
		).
		First(&progress).Error

	if err == nil && progress.IsCompleted {
		return nil, "Tantangan sudah pernah diselesaikan", fiber.StatusBadRequest
	}

	now := time.Now()

	if err != nil {
		progress = models.PlayerChallengeProgress{
			PlayerID:    playerID,
			ChallengeID: challengeID,
		}
	}

	progress.IsCompleted = true
	progress.Score = score
	progress.CompletedAt = &now

	if err != nil {
		if createErr := config.DB.Create(&progress).Error; createErr != nil {
			return nil, "Gagal menyimpan progress tantangan", fiber.StatusInternalServerError
		}
	} else {
		if saveErr := config.DB.Save(&progress).Error; saveErr != nil {
			return nil, "Gagal menyimpan progress tantangan", fiber.StatusInternalServerError
		}
	}

	var currentPlayer models.Player

	if err := config.DB.First(&currentPlayer, playerID).Error; err != nil {
		return nil, "Player tidak ditemukan", fiber.StatusNotFound
	}

	currentPlayer.XP += challenge.RewardXP
	currentPlayer.Stars += challenge.RewardStars

	newLevel := (currentPlayer.XP / 500) + 1

	if newLevel > currentPlayer.Level {
		currentPlayer.Level = newLevel
		player.UpdatePlayerTitle(&currentPlayer)
	}

	if err := config.DB.Save(&currentPlayer).Error; err != nil {
		return nil, "Gagal menyimpan data player", fiber.StatusInternalServerError
	}

	return fiber.Map{
		"message":      "Tantangan selesai!",
		"reward_xp":    challenge.RewardXP,
		"reward_stars": challenge.RewardStars,
		"level":        currentPlayer.Level,
	}, "", fiber.StatusOK
}
