package challenge

import (
	"errors"
	"log"
	"time"

	"toba-backend/config"
	"toba-backend/models"
	playerprogression "toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

const ChallengeSessionDuration = 2 * time.Hour

func GetActiveChallenge(playerID uint) (fiber.Map, string, int) {
	if playerID == 0 {
		return nil, "Sesi pemain tidak valid", fiber.StatusUnauthorized
	}

	now := time.Now()
	var response fiber.Map
	var businessMessage string
	var businessStatus int

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		// Lock player row so concurrent requests cannot create parallel sessions.
		var player models.Player
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&player, playerID).Error; err != nil {
			return err
		}

		// Resume an unexpired session first, even if Admin has since published
		// another weekly challenge. The player owns this snapshot for two hours.
		var session models.WeeklyChallengeSession
		sessionErr := tx.
			Where("player_id = ? AND completed_at IS NULL AND expires_at > ?", playerID, now).
			Order("created_at DESC").
			First(&session).Error
		if sessionErr == nil {
			var sessionChallenge models.WeeklyChallenge
			if err := tx.First(&sessionChallenge, session.ChallengeID).Error; err != nil {
				return err
			}
			response = fiber.Map{
				"has_challenge": true,
				"is_completed":  false,
				"challenge":     challengeSessionMetadata(sessionChallenge, session),
				"session": fiber.Map{
					"session_id": session.SessionID,
					"started_at": session.StartedAt,
					"expires_at": session.ExpiresAt,
				},
			}
			return nil
		}
		if sessionErr != nil && !errors.Is(sessionErr, gorm.ErrRecordNotFound) {
			return sessionErr
		}

		var active models.WeeklyChallenge
		activeErr := tx.
			Where("is_active = ? AND start_date <= ? AND end_date >= ?", true, now, now).
			Order("created_at DESC").
			First(&active).Error
		if errors.Is(activeErr, gorm.ErrRecordNotFound) {
			response = fiber.Map{"has_challenge": false, "is_completed": false}
			return nil
		}
		if activeErr != nil {
			return activeErr
		}

		var completedProgress models.PlayerChallengeProgress
		completedErr := tx.
			Where("player_id = ? AND challenge_id = ? AND is_completed = ?", playerID, active.ID, true).
			First(&completedProgress).Error
		if completedErr == nil {
			response = fiber.Map{"has_challenge": true, "is_completed": true, "challenge": challengeMetadata(active, "", "")}
			return nil
		}
		if completedErr != nil && !errors.Is(completedErr, gorm.ErrRecordNotFound) {
			return completedErr
		}

		publicJSON, answerKeyJSON, buildErr := BuildChallengeSnapshot(active.GameType, active.DataJSON)
		if buildErr != nil {
			businessStatus = fiber.StatusUnprocessableEntity
			businessMessage = "Konten tantangan belum valid: " + buildErr.Error()
			return nil
		}

		session = models.WeeklyChallengeSession{
			SessionID:      uuid.NewString(),
			PlayerID:       playerID,
			ChallengeID:    active.ID,
			Title:          active.Title,
			Description:    active.Description,
			GameType:       NormalizeGameType(active.GameType),
			RewardXP:       active.RewardXP,
			RewardStars:    active.RewardStars,
			PublicDataJSON: publicJSON,
			AnswerKeyJSON:  answerKeyJSON,
			StartedAt:      now,
			ExpiresAt:      now.Add(ChallengeSessionDuration),
		}
		if err := tx.Create(&session).Error; err != nil {
			return err
		}

		response = fiber.Map{
			"has_challenge": true,
			"is_completed":  false,
			"challenge":     challengeSessionMetadata(active, session),
			"session": fiber.Map{
				"session_id": session.SessionID,
				"started_at": session.StartedAt,
				"expires_at": session.ExpiresAt,
			},
		}
		return nil
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Pemain atau tantangan tidak ditemukan", fiber.StatusNotFound
		}
		return nil, "Gagal menyiapkan sesi tantangan", fiber.StatusInternalServerError
	}
	if businessStatus != 0 {
		return nil, businessMessage, businessStatus
	}
	return response, "", fiber.StatusOK
}

func challengeMetadata(challenge models.WeeklyChallenge, sessionID, publicDataJSON string) fiber.Map {
	return fiber.Map{
		"id":           challenge.ID,
		"title":        challenge.Title,
		"description":  challenge.Description,
		"game_type":    NormalizeGameType(challenge.GameType),
		"reward_xp":    challenge.RewardXP,
		"reward_stars": challenge.RewardStars,
		"session_id":   sessionID,
		"data_json":    publicDataJSON,
		"start_date":   challenge.StartDate,
		"end_date":     challenge.EndDate,
	}
}

func challengeSessionMetadata(challenge models.WeeklyChallenge, session models.WeeklyChallengeSession) fiber.Map {
	return fiber.Map{
		"id":           challenge.ID,
		"title":        session.Title,
		"description":  session.Description,
		"game_type":    session.GameType,
		"reward_xp":    session.RewardXP,
		"reward_stars": session.RewardStars,
		"session_id":   session.SessionID,
		"data_json":    session.PublicDataJSON,
		"start_date":   challenge.StartDate,
		"end_date":     challenge.EndDate,
	}
}

func SubmitChallenge(playerID uint, sessionID, answersJSON string) (fiber.Map, string, int) {
	if playerID == 0 || sessionID == "" {
		return nil, "Sesi pemain atau challenge tidak valid", fiber.StatusBadRequest
	}
	if len(answersJSON) == 0 || len(answersJSON) > 256*1024 {
		return nil, "Jawaban kosong atau melebihi batas ukuran", fiber.StatusBadRequest
	}

	var response fiber.Map
	var shouldCheckAchievements bool
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var session models.WeeklyChallengeSession
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("session_id = ?", sessionID).
			First(&session).Error; err != nil {
			return err
		}

		if session.PlayerID != playerID {
			return gorm.ErrRecordNotFound
		}
		if session.CompletedAt != nil {
			return errChallengeAlreadyCompleted
		}
		if time.Now().After(session.ExpiresAt) {
			return errChallengeSessionExpired
		}

		var challenge models.WeeklyChallenge
		if err := tx.First(&challenge, session.ChallengeID).Error; err != nil {
			return err
		}

		score, total, scoreErr := ScoreChallengeAnswers(session.GameType, session.AnswerKeyJSON, answersJSON)
		if scoreErr != nil {
			businessErr := &challengeValidationError{message: scoreErr.Error()}
			return businessErr
		}

		var player models.Player
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&player, playerID).Error; err != nil {
			return err
		}

		var completedProgress models.PlayerChallengeProgress
		completedErr := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("player_id = ? AND challenge_id = ? AND is_completed = ?", playerID, session.ChallengeID, true).
			First(&completedProgress).Error
		if completedErr == nil {
			return errChallengeAlreadyCompleted
		}
		if completedErr != nil && !errors.Is(completedErr, gorm.ErrRecordNotFound) {
			return completedErr
		}

		var progress models.PlayerChallengeProgress
		progressErr := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("player_id = ? AND challenge_id = ?", playerID, session.ChallengeID).
			Order("id DESC").
			First(&progress).Error
		if progressErr != nil && !errors.Is(progressErr, gorm.ErrRecordNotFound) {
			return progressErr
		}

		now := time.Now()
		if errors.Is(progressErr, gorm.ErrRecordNotFound) {
			progress = models.PlayerChallengeProgress{PlayerID: playerID, ChallengeID: session.ChallengeID}
		}
		progress.IsCompleted = true
		progress.Score = score
		progress.ScoreValidated = true
		progress.CompletedAt = &now
		if progress.ID == 0 {
			if err := tx.Create(&progress).Error; err != nil {
				return err
			}
		} else if err := tx.Save(&progress).Error; err != nil {
			return err
		}

		progression, err := playerprogression.ApplyRewardToLockedPlayer(tx, &player, session.RewardXP, session.RewardStars)
		if err != nil {
			return err
		}

		session.CompletedAt = &now
		if err := tx.Save(&session).Error; err != nil {
			return err
		}

		shouldCheckAchievements = true
		percentage := 0
		if total > 0 {
			percentage = score * 100 / total
		}
		response = fiber.Map{
			"message":         "Tantangan selesai!",
			"score":           score,
			"total_questions": total,
			"score_percent":   percentage,
			"reward_xp":       progression.RewardXP,
			"reward_stars":    progression.RewardStars,
			"previous_level":  progression.PreviousLevel,
			"level":           progression.Level,
			"leveled_up":      progression.LeveledUp,
			"total_xp":        progression.TotalXP,
			"total_stars":     progression.TotalStars,
			"xp_for_next":     progression.XPForNext,
			"xp_progress":     progression.XPProgress,
			"title_name":      progression.TitleName,
		}
		return nil
	})

	if err != nil {
		switch {
		case errors.Is(err, gorm.ErrRecordNotFound):
			return nil, "Sesi tantangan tidak ditemukan", fiber.StatusNotFound
		case errors.Is(err, errChallengeAlreadyCompleted):
			return nil, "Tantangan sudah pernah diselesaikan", fiber.StatusConflict
		case errors.Is(err, errChallengeSessionExpired):
			return nil, "Sesi tantangan sudah kedaluwarsa. Silakan buka tantangan kembali.", fiber.StatusGone
		default:
			var validationErr *challengeValidationError
			if errors.As(err, &validationErr) {
				return nil, validationErr.message, fiber.StatusBadRequest
			}
			return nil, "Gagal menyimpan hasil tantangan", fiber.StatusInternalServerError
		}
	}

	if shouldCheckAchievements {
		unlocked, unlockErr := playerprogression.CheckAndUnlockAchievements(playerID)
		if unlockErr != nil {
			log.Printf("[Challenge] hasil tersimpan, tetapi pemeriksaan achievement pemain %d gagal: %v", playerID, unlockErr)
		} else {
			response["newly_unlocked_achievements"] = unlocked
		}
	}
	return response, "", fiber.StatusOK
}

var (
	errChallengeAlreadyCompleted = errors.New("challenge already completed")
	errChallengeSessionExpired   = errors.New("challenge session expired")
)

type challengeValidationError struct{ message string }

func (e *challengeValidationError) Error() string { return e.message }
