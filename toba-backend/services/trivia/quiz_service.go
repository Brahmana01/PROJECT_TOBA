package trivia

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

type TriviaAnswerResult struct {
	QuestionID       uint   `json:"question_id"`
	SelectedOptionID uint   `json:"selected_option_id"`
	CorrectOptionID  uint   `json:"correct_option_id"`
	IsCorrect        bool   `json:"is_correct"`
	Explanation      string `json:"explanation"`
}

type TriviaCategoryResponse struct {
	ID                uint                 `json:"id"`
	Name              string               `json:"name"`
	Description       string               `json:"description"`
	QuestionCount     int                  `json:"question_count"`
	MinimumQuestion   int                  `json:"minimum_question"`
	DisplayOrder      int                  `json:"display_order"`
	SessionID         string               `json:"-"`
	AnsweredQuestions []TriviaAnswerResult `json:"-"`
}

type TriviaQuestionResponse struct {
	ID       uint                   `json:"id"`
	Question string                 `json:"question"`
	Options  []TriviaOptionResponse `json:"options"`
}

type TriviaOptionResponse struct {
	ID         uint   `json:"id"`
	OptionText string `json:"option_text"`
}

func buildTriviaSnapshot(tx *gorm.DB, categoryID uint) ([]models.QuizSessionQuestionSnapshot, error) {
	var questions []models.TriviaQuestion
	if err := tx.
		Preload("Options").
		Where("category_id = ? AND is_active = ?", categoryID, true).
		Order("RANDOM()").
		Limit(models.TriviaQuestionCount).
		Find(&questions).Error; err != nil {
		return nil, err
	}
	if len(questions) < models.TriviaQuestionCount {
		return nil, fmt.Errorf("%w: ditemukan %d soal aktif", quizsession.ErrInsufficientQuestions, len(questions))
	}

	snapshot := make([]models.QuizSessionQuestionSnapshot, 0, models.TriviaQuestionCount)
	for _, question := range questions {
		if len(question.Options) != 4 {
			return nil, fmt.Errorf("soal Trivia %d harus memiliki tepat 4 opsi", question.ID)
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
			return nil, fmt.Errorf("soal Trivia %d harus memiliki tepat 1 jawaban benar", question.ID)
		}
		snapshot = append(snapshot, item)
	}
	return snapshot, nil
}

func toTriviaQuestionResponses(snapshot []models.QuizSessionQuestionSnapshot) []TriviaQuestionResponse {
	responses := make([]TriviaQuestionResponse, 0, len(snapshot))
	for _, question := range snapshot {
		item := TriviaQuestionResponse{
			ID:       question.QuestionID,
			Question: question.Question,
			Options:  make([]TriviaOptionResponse, 0, len(question.Options)),
		}
		for _, option := range question.Options {
			item.Options = append(item.Options, TriviaOptionResponse{
				ID:         option.OptionID,
				OptionText: option.OptionText,
			})
		}
		responses = append(responses, item)
	}
	return responses
}

func GetTriviaQuiz(playerID uint, categoryID string) (*TriviaCategoryResponse, []TriviaQuestionResponse, string, int) {
	if playerID == 0 {
		return nil, nil, "Sesi pemain tidak valid", fiber.StatusUnauthorized
	}

	var category models.TriviaCategory
	if err := config.DB.
		Where("id = ? AND is_active = ? AND deleted_at IS NULL", categoryID, true).
		First(&category).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, "Kategori tidak ditemukan", fiber.StatusNotFound
		}
		return nil, nil, "Gagal mengambil kategori", fiber.StatusInternalServerError
	}

	var session *models.QuizSession
	var snapshot []models.QuizSessionQuestionSnapshot
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var startErr error
		session, snapshot, startErr = quizsession.StartOrResume(
			tx,
			playerID,
			quizsession.ModeTrivia,
			category.ID,
			models.TriviaQuestionCount,
			func(tx *gorm.DB) ([]models.QuizSessionQuestionSnapshot, error) {
				return buildTriviaSnapshot(tx, category.ID)
			},
		)
		return startErr
	})
	if err != nil {
		if errors.Is(err, quizsession.ErrInsufficientQuestions) {
			return nil, nil, "Bank soal kategori harus memiliki minimal 10 soal aktif dan valid", fiber.StatusConflict
		}
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, "Pemain tidak ditemukan", fiber.StatusNotFound
		}
		return nil, nil, "Gagal membuat quiz session Trivia", fiber.StatusInternalServerError
	}

	storedAnswers, err := quizsession.DecodeAnswers(session.AnswersJSON)
	if err != nil {
		return nil, nil, "Data jawaban session Trivia tidak valid", fiber.StatusInternalServerError
	}
	answeredResponses := make([]TriviaAnswerResult, 0, len(storedAnswers))
	for _, answer := range storedAnswers {
		answeredResponses = append(answeredResponses, TriviaAnswerResult{
			QuestionID:       answer.QuestionID,
			SelectedOptionID: answer.SelectedOptionID,
			CorrectOptionID:  answer.CorrectOptionID,
			IsCorrect:        answer.IsCorrect,
			Explanation:      answer.Explanation,
		})
	}

	return &TriviaCategoryResponse{
		ID:                category.ID,
		Name:              category.Name,
		Description:       category.Description,
		QuestionCount:     models.TriviaQuestionCount,
		MinimumQuestion:   models.TriviaQuestionCount,
		DisplayOrder:      category.DisplayOrder,
		SessionID:         session.SessionID,
		AnsweredQuestions: answeredResponses,
	}, toTriviaQuestionResponses(snapshot), "", fiber.StatusOK
}

func triviaStarsForScore(correct int) int {
	switch {
	case correct >= 10:
		return 3
	case correct >= 8:
		return 2
	case correct >= 6:
		return 1
	default:
		return 0
	}
}

func triviaSessionHTTPStatus(err error) int {
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

func SubmitTriviaAnswer(
	playerID uint,
	categoryID uint,
	sessionID string,
	questionID uint,
	selectedOptionID uint,
) (fiber.Map, string, int) {
	if playerID == 0 || categoryID == 0 || sessionID == "" || questionID == 0 {
		return nil, "Data jawaban atau session tidak lengkap", fiber.StatusBadRequest
	}

	var answer models.QuizSessionAnswerSnapshot
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var recordErr error
		answer, recordErr = quizsession.RecordAnswer(
			tx,
			sessionID,
			playerID,
			quizsession.ModeTrivia,
			categoryID,
			models.TriviaQuestionCount,
			questionID,
			selectedOptionID,
		)
		return recordErr
	})
	if err != nil {
		return nil, err.Error(), triviaSessionHTTPStatus(err)
	}

	return fiber.Map{
		"question_id":        answer.QuestionID,
		"selected_option_id": answer.SelectedOptionID,
		"correct_option_id":  answer.CorrectOptionID,
		"is_correct":         answer.IsCorrect,
		"explanation":        answer.Explanation,
	}, "", fiber.StatusOK
}

func SubmitTrivia(
	playerID uint,
	categoryID uint,
	sessionID string,
) (fiber.Map, string, int) {
	if playerID == 0 || categoryID == 0 || sessionID == "" {
		return nil, "Data pemain, kategori, atau session tidak valid", fiber.StatusBadRequest
	}

	var response fiber.Map
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		finalize, err := quizsession.LockForFinalize(
			tx,
			sessionID,
			playerID,
			quizsession.ModeTrivia,
			categoryID,
			models.TriviaQuestionCount,
		)
		if err != nil {
			return err
		}
		if finalize.AlreadyCompleted {
			response = fiber.Map(finalize.CachedResult)
			return nil
		}

		var category models.TriviaCategory
		if err := tx.First(&category, categoryID).Error; err != nil {
			return err
		}

		answersByQuestion := make(map[uint]models.QuizSessionAnswerSnapshot, len(finalize.Answers))
		for _, answer := range finalize.Answers {
			answersByQuestion[answer.QuestionID] = answer
		}

		results := make([]TriviaAnswerResult, 0, models.TriviaQuestionCount)
		correct := 0
		for _, question := range finalize.Snapshot {
			answer, ok := answersByQuestion[question.QuestionID]
			if !ok {
				return quizsession.ErrIncompleteSession
			}
			if answer.IsCorrect {
				correct++
			}
			results = append(results, TriviaAnswerResult{
				QuestionID:       answer.QuestionID,
				SelectedOptionID: answer.SelectedOptionID,
				CorrectOptionID:  answer.CorrectOptionID,
				IsCorrect:        answer.IsCorrect,
				Explanation:      answer.Explanation,
			})
		}
		wrong := models.TriviaQuestionCount - correct

		var currentPlayer models.Player
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&currentPlayer, playerID).Error; err != nil {
			return err
		}

		var previousAttempts int64
		if err := tx.Model(&models.PlayerTriviaHistory{}).
			Where("player_id = ? AND category_id = ?", playerID, categoryID).
			Count(&previousAttempts).Error; err != nil {
			return err
		}

		var previousBest int
		if err := tx.Model(&models.PlayerTriviaHistory{}).
			Select("COALESCE(MAX(correct_answer), 0)").
			Where("player_id = ? AND category_id = ?", playerID, categoryID).
			Scan(&previousBest).Error; err != nil {
			return err
		}

		isFirstAttempt := previousAttempts == 0
		isNewBest := isFirstAttempt || correct > previousBest

		xpGained := 0
		if isFirstAttempt {
			xpGained = correct * models.TriviaXPPerCorrect
		}

		currentBest := previousBest
		if correct > currentBest {
			currentBest = correct
		}
		currentBestStars := triviaStarsForScore(currentBest)

		var previouslyAwardedStars int
		if err := tx.Model(&models.PlayerTriviaHistory{}).
			Select("COALESCE(SUM(stars_earned), 0)").
			Where("player_id = ? AND category_id = ?", playerID, categoryID).
			Scan(&previouslyAwardedStars).Error; err != nil {
			return err
		}

		starsGained := currentBestStars - previouslyAwardedStars
		if starsGained < 0 {
			starsGained = 0
		}

		progression, err := playerprogression.ApplyRewardToLockedPlayer(
			tx, &currentPlayer, xpGained, starsGained,
		)
		if err != nil {
			return err
		}

		history := models.PlayerTriviaHistory{
			PlayerID:      playerID,
			CategoryID:    categoryID,
			Score:         correct,
			CorrectAnswer: correct,
			WrongAnswer:   wrong,
			XPEarned:      xpGained,
			StarsEarned:   starsGained,
			PlayedAt:      time.Now(),
		}
		if err := tx.Create(&history).Error; err != nil {
			return err
		}

		for _, answer := range results {
			if err := tx.Create(&models.PlayerTriviaAnswer{
				HistoryID:        history.ID,
				QuestionID:       answer.QuestionID,
				SelectedOptionID: answer.SelectedOptionID,
				IsCorrect:        answer.IsCorrect,
				AnsweredAt:       time.Now(),
			}).Error; err != nil {
				return err
			}
		}

		response = fiber.Map{
			"category_id":      categoryID,
			"correct_count":    correct,
			"wrong_count":      wrong,
			"total_questions":  models.TriviaQuestionCount,
			"xp":               progression.RewardXP,
			"reward_xp":        progression.RewardXP,
			"reward_stars":     progression.RewardStars,
			"previous_level":   progression.PreviousLevel,
			"level":            progression.Level,
			"leveled_up":       progression.LeveledUp,
			"total_xp":         progression.TotalXP,
			"total_stars":      progression.TotalStars,
			"xp_for_next":      progression.XPForNext,
			"xp_progress":      progression.XPProgress,
			"title_name":       progression.TitleName,
			"history_id":       history.ID,
			"is_first_attempt": isFirstAttempt,
			"is_new_best":      isNewBest,
			"best_score":       currentBest,
			"best_stars":       currentBestStars,
			"answers":          results,
		}

		return quizsession.MarkCompleted(tx, finalize.Session, response)
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Player atau kategori tidak ditemukan", fiber.StatusNotFound
		}
		if errors.Is(err, quizsession.ErrIncompleteSession) {
			return nil, err.Error(), fiber.StatusConflict
		}
		if errors.Is(err, quizsession.ErrSessionNotFound) ||
			errors.Is(err, quizsession.ErrSessionMismatch) ||
			errors.Is(err, quizsession.ErrSessionExpired) ||
			errors.Is(err, quizsession.ErrSessionNotActive) {
			return nil, err.Error(), triviaSessionHTTPStatus(err)
		}
		return nil, err.Error(), fiber.StatusInternalServerError
	}

	// Achievement diperiksa setelah histori dan reward Trivia tersimpan.
	// Kegagalan pemeriksaan tidak membatalkan hasil permainan.
	if _, achievementErr := playerprogression.CheckAndUnlockAchievements(playerID); achievementErr != nil {
		log.Printf("[Achievement] Gagal memeriksa setelah Trivia player=%d: %v", playerID, achievementErr)
	}

	return response, "", fiber.StatusOK
}
