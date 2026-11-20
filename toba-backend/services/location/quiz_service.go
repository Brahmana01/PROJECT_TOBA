package location

import (
	"errors"
	"fmt"
	"log"
	"time"

	"toba-backend/config"
	"toba-backend/models"
	playerprogression "toba-backend/services/player"
	"toba-backend/services/quizsession"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

type QuizOptionResponse struct {
	ID         uint   `json:"id"`
	OptionText string `json:"option_text"`
}

type QuizQuestionResponse struct {
	ID       uint                 `json:"id"`
	Question string               `json:"question"`
	Options  []QuizOptionResponse `json:"options"`
}

type LocationQuizResponse struct {
	ID                uint               `json:"id"`
	Name              string             `json:"name"`
	Description       string             `json:"description"`
	Image             string             `json:"image"`
	RewardXP          int                `json:"reward_xp"`
	RewardStars       int                `json:"reward_stars"`
	QuestionCount     int                `json:"question_count"`
	MinimumCorrect    int                `json:"minimum_correct"`
	LevelOrder        int                `json:"level_order"`
	SessionID         string             `json:"-"`
	AnsweredQuestions []QuizAnswerResult `json:"-"`
}

type QuizAnswerResult struct {
	QuestionID       uint   `json:"question_id"`
	SelectedOptionID uint   `json:"selected_option_id"`
	CorrectOptionID  uint   `json:"correct_option_id"`
	IsCorrect        bool   `json:"is_correct"`
	Explanation      string `json:"explanation"`
}

func buildLocationSnapshot(tx *gorm.DB, locationID uint) ([]models.QuizSessionQuestionSnapshot, error) {
	var questions []models.LocationQuestion
	if err := tx.
		Preload("Options").
		Where("location_id = ? AND is_active = ?", locationID, true).
		Order("RANDOM()").
		Limit(models.LocationQuestionCount).
		Find(&questions).Error; err != nil {
		return nil, err
	}
	if len(questions) < models.LocationQuestionCount {
		return nil, fmt.Errorf("%w: ditemukan %d soal aktif", quizsession.ErrInsufficientQuestions, len(questions))
	}

	snapshot := make([]models.QuizSessionQuestionSnapshot, 0, models.LocationQuestionCount)
	for _, question := range questions {
		if len(question.Options) != 4 {
			return nil, fmt.Errorf("soal %d harus memiliki tepat 4 opsi", question.ID)
		}
		item := models.QuizSessionQuestionSnapshot{
			QuestionID:  question.ID,
			Question:    question.Question,
			Explanation: question.Explanation,
			Options:     make([]models.QuizSessionOptionSnapshot, 0, 4),
		}
		correctCount := 0
		for _, option := range question.Options {
			if option.IsCorrect {
				correctCount++
			}
			item.Options = append(item.Options, models.QuizSessionOptionSnapshot{
				OptionID:   option.ID,
				OptionText: option.OptionText,
				IsCorrect:  option.IsCorrect,
			})
		}
		if correctCount != 1 {
			return nil, fmt.Errorf("soal %d harus memiliki tepat 1 jawaban benar", question.ID)
		}
		snapshot = append(snapshot, item)
	}
	return snapshot, nil
}

func toLocationQuestionResponses(snapshot []models.QuizSessionQuestionSnapshot) []QuizQuestionResponse {
	responses := make([]QuizQuestionResponse, 0, len(snapshot))
	for _, question := range snapshot {
		item := QuizQuestionResponse{
			ID:       question.QuestionID,
			Question: question.Question,
			Options:  make([]QuizOptionResponse, 0, len(question.Options)),
		}
		for _, option := range question.Options {
			item.Options = append(item.Options, QuizOptionResponse{
				ID:         option.OptionID,
				OptionText: option.OptionText,
			})
		}
		responses = append(responses, item)
	}
	return responses
}

func GetLocationQuiz(playerID uint, locationID string) (*LocationQuizResponse, []QuizQuestionResponse, string, int) {
	if playerID == 0 {
		return nil, nil, "Sesi pemain tidak valid", fiber.StatusUnauthorized
	}
	id, err := GetLocationID(locationID)
	if err != nil {
		return nil, nil, "ID lokasi tidak valid", fiber.StatusBadRequest
	}

	var location models.Location
	if err := config.DB.
		Where("id = ? AND is_active = ? AND deleted_at IS NULL", id, true).
		First(&location).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, "Lokasi tidak ditemukan", fiber.StatusNotFound
		}
		return nil, nil, "Gagal mengambil lokasi", fiber.StatusInternalServerError
	}

	// Lokasi pertama selalu tersedia; lokasi berikutnya harus sudah di-unlock.
	if location.LevelOrder > 1 {
		var progress models.PlayerLocationProgress
		err := config.DB.
			Where("player_id = ? AND location_id = ?", playerID, location.ID).
			First(&progress).Error
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, "Lokasi belum terbuka", fiber.StatusForbidden
		}
		if err != nil {
			return nil, nil, "Gagal memeriksa akses lokasi", fiber.StatusInternalServerError
		}
		if !progress.IsUnlocked {
			return nil, nil, "Lokasi belum terbuka", fiber.StatusForbidden
		}
	}

	// First clear harus dapat membuka tepat dua kartu Gallery.
	// Jangan biarkan pemain memulai sesi yang tidak bisa diselesaikan
	// karena konten Gallery lokasi belum lengkap.
	var existingProgress models.PlayerLocationProgress
	progressErr := config.DB.
		Where("player_id = ? AND location_id = ?", playerID, location.ID).
		First(&existingProgress).Error
	if progressErr != nil && !errors.Is(progressErr, gorm.ErrRecordNotFound) {
		return nil, nil, "Gagal memeriksa progres lokasi", fiber.StatusInternalServerError
	}
	if errors.Is(progressErr, gorm.ErrRecordNotFound) || !existingProgress.IsCompleted {
		var galleryCount int64
		if err := config.DB.Model(&models.GalleryCard{}).
			Where("location_id = ?", location.ID).
			Count(&galleryCount).Error; err != nil {
			return nil, nil, "Gagal memeriksa kartu Gallery lokasi", fiber.StatusInternalServerError
		}
		if galleryCount != int64(models.GalleryCardsPerLocation) {
			return nil, nil,
				"Lokasi belum siap: wajib memiliki tepat 2 kartu Gallery",
				fiber.StatusConflict
		}
	}

	var session *models.QuizSession
	var snapshot []models.QuizSessionQuestionSnapshot
	err = config.DB.Transaction(func(tx *gorm.DB) error {
		var startErr error
		session, snapshot, startErr = quizsession.StartOrResume(
			tx,
			playerID,
			quizsession.ModeLocation,
			location.ID,
			models.LocationQuestionCount,
			func(tx *gorm.DB) ([]models.QuizSessionQuestionSnapshot, error) {
				return buildLocationSnapshot(tx, location.ID)
			},
		)
		return startErr
	})
	if err != nil {
		if errors.Is(err, quizsession.ErrInsufficientQuestions) {
			return nil, nil, "Bank soal lokasi harus memiliki minimal 10 soal aktif dan valid", fiber.StatusConflict
		}
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, "Pemain tidak ditemukan", fiber.StatusNotFound
		}
		return nil, nil, "Gagal membuat quiz session lokasi", fiber.StatusInternalServerError
	}

	storedAnswers, err := quizsession.DecodeAnswers(session.AnswersJSON)
	if err != nil {
		return nil, nil, "Data jawaban session tidak valid", fiber.StatusInternalServerError
	}
	answeredResponses := make([]QuizAnswerResult, 0, len(storedAnswers))
	for _, answer := range storedAnswers {
		answeredResponses = append(answeredResponses, QuizAnswerResult{
			QuestionID:       answer.QuestionID,
			SelectedOptionID: answer.SelectedOptionID,
			CorrectOptionID:  answer.CorrectOptionID,
			IsCorrect:        answer.IsCorrect,
			Explanation:      answer.Explanation,
		})
	}

	return &LocationQuizResponse{
		ID:                location.ID,
		Name:              location.Name,
		Description:       location.Description,
		Image:             location.Image,
		RewardXP:          location.RewardXP,
		RewardStars:       location.RewardStars,
		QuestionCount:     models.LocationQuestionCount,
		MinimumCorrect:    models.LocationMinimumCorrect,
		LevelOrder:        location.LevelOrder,
		SessionID:         session.SessionID,
		AnsweredQuestions: answeredResponses,
	}, toLocationQuestionResponses(snapshot), "", fiber.StatusOK
}

func sessionHTTPStatus(err error) int {
	switch {
	case errors.Is(err, quizsession.ErrSessionNotFound):
		return fiber.StatusNotFound
	case errors.Is(err, quizsession.ErrSessionMismatch):
		return fiber.StatusForbidden
	case errors.Is(err, quizsession.ErrSessionExpired):
		return fiber.StatusGone
	case errors.Is(err, quizsession.ErrSessionNotActive),
		errors.Is(err, quizsession.ErrQuestionAlreadyDone),
		errors.Is(err, quizsession.ErrQuestionOutOfOrder):
		return fiber.StatusConflict
	case errors.Is(err, quizsession.ErrQuestionNotInSession),
		errors.Is(err, quizsession.ErrOptionNotInQuestion):
		return fiber.StatusBadRequest
	default:
		return fiber.StatusInternalServerError
	}
}

func SubmitLocationQuizAnswer(
	playerID uint,
	locationID uint,
	sessionID string,
	questionID uint,
	selectedOptionID uint,
) (fiber.Map, string, int) {
	if playerID == 0 || locationID == 0 || sessionID == "" || questionID == 0 {
		return nil, "Data jawaban atau session tidak lengkap", fiber.StatusBadRequest
	}

	var answer models.QuizSessionAnswerSnapshot
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var recordErr error
		answer, recordErr = quizsession.RecordAnswer(
			tx,
			sessionID,
			playerID,
			quizsession.ModeLocation,
			locationID,
			models.LocationQuestionCount,
			questionID,
			selectedOptionID,
		)
		return recordErr
	})
	if err != nil {
		return nil, err.Error(), sessionHTTPStatus(err)
	}

	return fiber.Map{
		"question_id":        answer.QuestionID,
		"selected_option_id": answer.SelectedOptionID,
		"correct_option_id":  answer.CorrectOptionID,
		"is_correct":         answer.IsCorrect,
		"explanation":        answer.Explanation,
	}, "", fiber.StatusOK
}

func SubmitLocationQuiz(
	playerID uint,
	locationID uint,
	sessionID string,
) (fiber.Map, string, int) {
	if playerID == 0 || locationID == 0 || sessionID == "" {
		return nil, "Data pemain, lokasi, atau session tidak valid", fiber.StatusBadRequest
	}

	var response fiber.Map
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		finalize, err := quizsession.LockForFinalize(
			tx,
			sessionID,
			playerID,
			quizsession.ModeLocation,
			locationID,
			models.LocationQuestionCount,
		)
		if err != nil {
			return err
		}
		if finalize.AlreadyCompleted {
			response = fiber.Map(finalize.CachedResult)
			return nil
		}

		var location models.Location
		if err := tx.First(&location, locationID).Error; err != nil {
			return err
		}

		if location.LevelOrder > 1 {
			var access models.PlayerLocationProgress
			accessErr := tx.
				Where("player_id = ? AND location_id = ?", playerID, location.ID).
				First(&access).Error
			if errors.Is(accessErr, gorm.ErrRecordNotFound) {
				return fiber.ErrForbidden
			}
			if accessErr != nil {
				return accessErr
			}
			if !access.IsUnlocked {
				return fiber.ErrForbidden
			}
		}

		answersByQuestion := make(map[uint]models.QuizSessionAnswerSnapshot, len(finalize.Answers))
		for _, answer := range finalize.Answers {
			answersByQuestion[answer.QuestionID] = answer
		}

		results := make([]QuizAnswerResult, 0, models.LocationQuestionCount)
		correctCount := 0
		for _, question := range finalize.Snapshot {
			answer, ok := answersByQuestion[question.QuestionID]
			if !ok {
				return quizsession.ErrIncompleteSession
			}
			if answer.IsCorrect {
				correctCount++
			}
			results = append(results, QuizAnswerResult{
				QuestionID:       answer.QuestionID,
				SelectedOptionID: answer.SelectedOptionID,
				CorrectOptionID:  answer.CorrectOptionID,
				IsCorrect:        answer.IsCorrect,
				Explanation:      answer.Explanation,
			})
		}

		wrongCount := models.LocationQuestionCount - correctCount
		passed := correctCount >= models.LocationMinimumCorrect

		var currentPlayer models.Player
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&currentPlayer, playerID).Error; err != nil {
			return err
		}

		var progress models.PlayerLocationProgress
		progressErr := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("player_id = ? AND location_id = ?", playerID, location.ID).
			First(&progress).Error
		if progressErr != nil && !errors.Is(progressErr, gorm.ErrRecordNotFound) {
			return progressErr
		}
		if errors.Is(progressErr, gorm.ErrRecordNotFound) {
			progress = models.PlayerLocationProgress{
				PlayerID:   playerID,
				LocationID: location.ID,
				IsUnlocked: true,
			}
		}

		progress.AttemptCount++
		if correctCount > progress.Score {
			progress.Score = correctCount
		}
		progress.CorrectAnswer = correctCount
		progress.WrongAnswer = wrongCount

		isNewPass := passed && !progress.IsCompleted
		rewardXP := 0
		rewardStars := 0

		if isNewPass {
			progress.IsCompleted = true
			progress.IsUnlocked = true
			now := time.Now()
			progress.CompletedAt = &now
			rewardXP = location.RewardXP
			rewardStars = location.RewardStars
			progress.XPEarned = rewardXP
			progress.StarsEarned = rewardStars

			if _, err := playerprogression.ApplyRewardToLockedPlayer(
				tx,
				&currentPlayer,
				rewardXP,
				rewardStars,
			); err != nil {
				return err
			}

			// Unlock lokasi aktif berikutnya berdasarkan level_order.
			var nextLocation models.Location
			if err := tx.
				Where("level_order > ? AND is_active = ? AND deleted_at IS NULL", location.LevelOrder, true).
				Order("level_order ASC").
				First(&nextLocation).Error; err == nil {
				var nextProgress models.PlayerLocationProgress
				nextErr := tx.
					Clauses(clause.Locking{Strength: "UPDATE"}).
					Where("player_id = ? AND location_id = ?", playerID, nextLocation.ID).
					First(&nextProgress).Error
				if errors.Is(nextErr, gorm.ErrRecordNotFound) {
					nextProgress = models.PlayerLocationProgress{
						PlayerID:   playerID,
						LocationID: nextLocation.ID,
						IsUnlocked: true,
					}
					if err := tx.Create(&nextProgress).Error; err != nil {
						return err
					}
				} else if nextErr != nil {
					return nextErr
				} else if !nextProgress.IsUnlocked {
					nextProgress.IsUnlocked = true
					if err := tx.Save(&nextProgress).Error; err != nil {
						return err
					}
				}
			} else if !errors.Is(err, gorm.ErrRecordNotFound) {
				return err
			}

			// First clear membuka tepat dua kartu Gallery lokasi ini.
			var galleryCount int64
			if err := tx.Model(&models.GalleryCard{}).
				Where("location_id = ?", location.ID).
				Count(&galleryCount).Error; err != nil {
				return err
			}
			if galleryCount != int64(models.GalleryCardsPerLocation) {
				return fmt.Errorf(
					"gallery lokasi %d harus memiliki tepat %d kartu",
					location.ID, models.GalleryCardsPerLocation,
				)
			}

			var cards []models.GalleryCard
			if err := tx.
				Where("location_id = ?", location.ID).
				Order("id ASC").
				Find(&cards).Error; err != nil {
				return err
			}
			for _, card := range cards {
				var existing models.PlayerGalleryCard
				cardErr := tx.
					Where("player_id = ? AND gallery_card_id = ?", playerID, card.ID).
					First(&existing).Error
				if errors.Is(cardErr, gorm.ErrRecordNotFound) {
					if err := tx.Create(&models.PlayerGalleryCard{
						PlayerID:      playerID,
						GalleryCardID: card.ID,
						ObtainedAt:    time.Now(),
					}).Error; err != nil {
						return err
					}
				} else if cardErr != nil {
					return cardErr
				}
			}
		}

		if progress.ID == 0 {
			if err := tx.Create(&progress).Error; err != nil {
				return err
			}
		} else if err := tx.Save(&progress).Error; err != nil {
			return err
		}

		response = fiber.Map{
			"location_id":     location.ID,
			"correct_count":   correctCount,
			"wrong_count":     wrongCount,
			"total_questions": models.LocationQuestionCount,
			"minimum_correct": models.LocationMinimumCorrect,
			"passed":          passed,
			"is_new_pass":     isNewPass,
			"reward_xp":       rewardXP,
			"reward_stars":    rewardStars,
			"answers":         results,
			"attempt_count":   progress.AttemptCount,
			"best_score":      progress.Score,
			"level":           currentPlayer.Level,
			"total_xp":        currentPlayer.XP,
			"total_stars":     currentPlayer.Stars,
		}

		return quizsession.MarkCompleted(tx, finalize.Session, response)
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Lokasi atau pemain tidak ditemukan", fiber.StatusNotFound
		}
		if errors.Is(err, fiber.ErrForbidden) {
			return nil, "Lokasi belum terbuka", fiber.StatusForbidden
		}
		if errors.Is(err, quizsession.ErrIncompleteSession) {
			return nil, err.Error(), fiber.StatusConflict
		}
		if errors.Is(err, quizsession.ErrSessionNotFound) ||
			errors.Is(err, quizsession.ErrSessionMismatch) ||
			errors.Is(err, quizsession.ErrSessionExpired) ||
			errors.Is(err, quizsession.ErrSessionNotActive) {
			return nil, err.Error(), sessionHTTPStatus(err)
		}
		return nil, err.Error(), fiber.StatusInternalServerError
	}

	// Achievement diperiksa setelah hasil gameplay berhasil tersimpan.
	// Fungsi ini idempotent; kegagalan pemeriksaan tidak membatalkan hasil kuis.
	if _, achievementErr := playerprogression.CheckAndUnlockAchievements(playerID); achievementErr != nil {
		log.Printf("[Achievement] Gagal memeriksa setelah Jelajah player=%d: %v", playerID, achievementErr)
	}

	return response, "", fiber.StatusOK
}
