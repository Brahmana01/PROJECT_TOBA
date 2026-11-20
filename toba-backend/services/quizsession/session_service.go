package quizsession

import (
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"strings"
	"time"

	"toba-backend/models"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

const (
	ModeLocation = "LOCATION"
	ModeTrivia   = "TRIVIA"

	StatusActive    = "ACTIVE"
	StatusCompleted = "COMPLETED"
	StatusExpired   = "EXPIRED"

	SessionDuration = 2 * time.Hour
)

var (
	ErrInsufficientQuestions = errors.New("bank soal aktif belum memiliki minimal 10 soal valid")
	ErrSessionNotFound       = errors.New("quiz session tidak ditemukan")
	ErrSessionMismatch       = errors.New("quiz session bukan milik pemain atau mode ini")
	ErrSessionExpired        = errors.New("quiz session sudah kedaluwarsa")
	ErrSessionNotActive      = errors.New("quiz session tidak aktif")
	ErrQuestionNotInSession  = errors.New("pertanyaan bukan bagian dari quiz session")
	ErrOptionNotInQuestion   = errors.New("pilihan jawaban bukan milik pertanyaan")
	ErrQuestionAlreadyDone   = errors.New("pertanyaan sudah dijawab dengan pilihan berbeda")
	ErrQuestionOutOfOrder    = errors.New("jawaban harus dikirim sesuai urutan soal sesi")
	ErrIncompleteSession     = errors.New("seluruh 10 pertanyaan harus dijawab sebelum submit")
	ErrInvalidSnapshot       = errors.New("snapshot quiz session tidak valid")
)

func newSessionID() (string, error) {
	var b [16]byte
	if _, err := rand.Read(b[:]); err != nil {
		return "", err
	}
	b[6] = (b[6] & 0x0f) | 0x40
	b[8] = (b[8] & 0x3f) | 0x80
	encoded := hex.EncodeToString(b[:])
	return fmt.Sprintf("%s-%s-%s-%s-%s", encoded[0:8], encoded[8:12], encoded[12:16], encoded[16:20], encoded[20:32]), nil
}

func validateSnapshot(snapshot []models.QuizSessionQuestionSnapshot, expected int) error {
	if len(snapshot) != expected {
		return ErrInvalidSnapshot
	}
	seen := make(map[uint]bool, len(snapshot))
	for _, question := range snapshot {
		if question.QuestionID == 0 || strings.TrimSpace(question.Question) == "" || seen[question.QuestionID] || len(question.Options) != 4 {
			return ErrInvalidSnapshot
		}
		seen[question.QuestionID] = true
		correct := 0
		optionIDs := make(map[uint]bool, len(question.Options))
		for _, option := range question.Options {
			if option.OptionID == 0 || optionIDs[option.OptionID] || strings.TrimSpace(option.OptionText) == "" {
				return ErrInvalidSnapshot
			}
			optionIDs[option.OptionID] = true
			if option.IsCorrect {
				correct++
			}
		}
		if correct != 1 {
			return ErrInvalidSnapshot
		}
	}
	return nil
}

func decodeSnapshot(raw string, expected int) ([]models.QuizSessionQuestionSnapshot, error) {
	var snapshot []models.QuizSessionQuestionSnapshot
	if err := json.Unmarshal([]byte(raw), &snapshot); err != nil {
		return nil, ErrInvalidSnapshot
	}
	if err := validateSnapshot(snapshot, expected); err != nil {
		return nil, err
	}
	return snapshot, nil
}

func decodeAnswers(raw string) ([]models.QuizSessionAnswerSnapshot, error) {
	if raw == "" {
		return []models.QuizSessionAnswerSnapshot{}, nil
	}
	var answers []models.QuizSessionAnswerSnapshot
	if err := json.Unmarshal([]byte(raw), &answers); err != nil {
		return nil, err
	}
	return answers, nil
}

// StartOrResume mengunci row Player sehingga dua GET bersamaan tidak membuat
// dua sesi aktif untuk pemain dan target yang sama.
func StartOrResume(
	tx *gorm.DB,
	playerID uint,
	mode string,
	targetID uint,
	questionCount int,
	buildSnapshot func(*gorm.DB) ([]models.QuizSessionQuestionSnapshot, error),
) (*models.QuizSession, []models.QuizSessionQuestionSnapshot, error) {
	if playerID == 0 || targetID == 0 || questionCount <= 0 || buildSnapshot == nil {
		return nil, nil, errors.New("parameter quiz session tidak valid")
	}

	var player models.Player
	if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&player, playerID).Error; err != nil {
		return nil, nil, err
	}

	now := time.Now()
	if err := tx.Model(&models.QuizSession{}).
		Where("player_id = ? AND game_mode = ? AND target_id = ? AND status = ? AND expires_at <= ?",
			playerID, mode, targetID, StatusActive, now).
		Update("status", StatusExpired).Error; err != nil {
		return nil, nil, err
	}

	var existing models.QuizSession
	err := tx.
		Where("player_id = ? AND game_mode = ? AND target_id = ? AND status = ? AND expires_at > ?",
			playerID, mode, targetID, StatusActive, now).
		Order("id DESC").First(&existing).Error
	if err == nil {
		snapshot, decodeErr := decodeSnapshot(existing.SnapshotJSON, questionCount)
		_, answersErr := decodeAnswers(existing.AnswersJSON)
		if decodeErr == nil && answersErr == nil {
			return &existing, snapshot, nil
		}
		if err := tx.Model(&existing).Update("status", StatusExpired).Error; err != nil {
			return nil, nil, err
		}
	} else if !errors.Is(err, gorm.ErrRecordNotFound) {
		return nil, nil, err
	}

	snapshot, err := buildSnapshot(tx)
	if err != nil {
		return nil, nil, err
	}
	if err := validateSnapshot(snapshot, questionCount); err != nil {
		return nil, nil, err
	}

	snapshotJSON, err := json.Marshal(snapshot)
	if err != nil {
		return nil, nil, err
	}
	sessionID, err := newSessionID()
	if err != nil {
		return nil, nil, err
	}

	session := models.QuizSession{
		SessionID:    sessionID,
		PlayerID:     playerID,
		GameMode:     mode,
		TargetID:     targetID,
		SnapshotJSON: string(snapshotJSON),
		AnswersJSON:  "[]",
		Status:       StatusActive,
		ExpiresAt:    now.Add(SessionDuration),
	}
	if err := tx.Create(&session).Error; err != nil {
		return nil, nil, err
	}

	return &session, snapshot, nil
}

// DecodeAnswers membaca jawaban yang sudah tersimpan untuk kebutuhan resume UI.
func DecodeAnswers(raw string) ([]models.QuizSessionAnswerSnapshot, error) {
	return decodeAnswers(raw)
}

func loadActiveSession(
	tx *gorm.DB,
	sessionID string,
	playerID uint,
	mode string,
	targetID uint,
	questionCount int,
) (*models.QuizSession, []models.QuizSessionQuestionSnapshot, []models.QuizSessionAnswerSnapshot, error) {
	if sessionID == "" {
		return nil, nil, nil, ErrSessionNotFound
	}

	var session models.QuizSession
	if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).
		Where("session_id = ?", sessionID).First(&session).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, nil, nil, ErrSessionNotFound
		}
		return nil, nil, nil, err
	}

	if session.PlayerID != playerID || session.GameMode != mode || session.TargetID != targetID {
		return nil, nil, nil, ErrSessionMismatch
	}
	if session.Status != StatusActive {
		return nil, nil, nil, ErrSessionNotActive
	}
	if !session.ExpiresAt.After(time.Now()) {
		_ = tx.Model(&session).Update("status", StatusExpired).Error
		return nil, nil, nil, ErrSessionExpired
	}

	snapshot, err := decodeSnapshot(session.SnapshotJSON, questionCount)
	if err != nil {
		return nil, nil, nil, err
	}
	answers, err := decodeAnswers(session.AnswersJSON)
	if err != nil {
		return nil, nil, nil, err
	}
	return &session, snapshot, answers, nil
}

// RecordAnswer memvalidasi jawaban dari snapshot server, bukan dari data client.
// selectedOptionID=0 dipakai untuk jawaban yang habis waktu/tidak dijawab.
func RecordAnswer(
	tx *gorm.DB,
	sessionID string,
	playerID uint,
	mode string,
	targetID uint,
	questionCount int,
	questionID uint,
	selectedOptionID uint,
) (models.QuizSessionAnswerSnapshot, error) {
	if questionID == 0 {
		return models.QuizSessionAnswerSnapshot{}, errors.New("question_id wajib diisi")
	}

	session, snapshot, answers, err := loadActiveSession(
		tx, sessionID, playerID, mode, targetID, questionCount,
	)
	if err != nil {
		return models.QuizSessionAnswerSnapshot{}, err
	}

	var question *models.QuizSessionQuestionSnapshot
	for i := range snapshot {
		if snapshot[i].QuestionID == questionID {
			question = &snapshot[i]
			break
		}
	}
	if question == nil {
		return models.QuizSessionAnswerSnapshot{}, ErrQuestionNotInSession
	}

	for _, existing := range answers {
		if existing.QuestionID == questionID {
			if existing.SelectedOptionID == selectedOptionID {
				return existing, nil
			}
			return models.QuizSessionAnswerSnapshot{}, ErrQuestionAlreadyDone
		}
	}

	// Pemain hanya boleh menjawab pertanyaan berikutnya yang belum dijawab.
	if len(answers) >= len(snapshot) || snapshot[len(answers)].QuestionID != questionID {
		return models.QuizSessionAnswerSnapshot{}, ErrQuestionOutOfOrder
	}

	var correctOptionID uint
	correctCount := 0
	selectedFound := selectedOptionID == 0
	selectedCorrect := false
	for _, option := range question.Options {
		if option.IsCorrect {
			correctOptionID = option.OptionID
			correctCount++
		}
		if option.OptionID == selectedOptionID && selectedOptionID != 0 {
			selectedFound = true
			selectedCorrect = option.IsCorrect
		}
	}
	if correctCount != 1 {
		return models.QuizSessionAnswerSnapshot{}, ErrInvalidSnapshot
	}
	if !selectedFound {
		return models.QuizSessionAnswerSnapshot{}, ErrOptionNotInQuestion
	}

	answer := models.QuizSessionAnswerSnapshot{
		QuestionID:       questionID,
		SelectedOptionID: selectedOptionID,
		CorrectOptionID:  correctOptionID,
		IsCorrect:        selectedOptionID != 0 && selectedCorrect,
		Explanation:      question.Explanation,
		AnsweredAt:       time.Now(),
	}
	answers = append(answers, answer)
	answersJSON, err := json.Marshal(answers)
	if err != nil {
		return models.QuizSessionAnswerSnapshot{}, err
	}
	if err := tx.Model(session).Update("answers_json", string(answersJSON)).Error; err != nil {
		return models.QuizSessionAnswerSnapshot{}, err
	}
	return answer, nil
}

type FinalizeData struct {
	Session          *models.QuizSession
	Snapshot         []models.QuizSessionQuestionSnapshot
	Answers          []models.QuizSessionAnswerSnapshot
	CachedResult     map[string]interface{}
	AlreadyCompleted bool
}

func LockForFinalize(
	tx *gorm.DB,
	sessionID string,
	playerID uint,
	mode string,
	targetID uint,
	questionCount int,
) (*FinalizeData, error) {
	if sessionID == "" {
		return nil, ErrSessionNotFound
	}
	var session models.QuizSession
	if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).
		Where("session_id = ?", sessionID).First(&session).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, ErrSessionNotFound
		}
		return nil, err
	}
	if session.PlayerID != playerID || session.GameMode != mode || session.TargetID != targetID {
		return nil, ErrSessionMismatch
	}

	if session.Status == StatusCompleted {
		var cached map[string]interface{}
		if err := json.Unmarshal([]byte(session.ResultJSON), &cached); err != nil || cached == nil {
			return nil, errors.New("hasil quiz session tersimpan tidak valid")
		}
		return &FinalizeData{
			Session:          &session,
			CachedResult:     cached,
			AlreadyCompleted: true,
		}, nil
	}
	if session.Status != StatusActive {
		return nil, ErrSessionNotActive
	}
	if !session.ExpiresAt.After(time.Now()) {
		_ = tx.Model(&session).Update("status", StatusExpired).Error
		return nil, ErrSessionExpired
	}

	snapshot, err := decodeSnapshot(session.SnapshotJSON, questionCount)
	if err != nil {
		return nil, err
	}
	answers, err := decodeAnswers(session.AnswersJSON)
	if err != nil {
		return nil, err
	}
	if len(answers) != questionCount {
		return nil, ErrIncompleteSession
	}

	seen := make(map[uint]bool, len(answers))
	for _, answer := range answers {
		if seen[answer.QuestionID] {
			return nil, ErrInvalidSnapshot
		}
		seen[answer.QuestionID] = true
	}
	if len(seen) != questionCount {
		return nil, ErrIncompleteSession
	}

	return &FinalizeData{
		Session:  &session,
		Snapshot: snapshot,
		Answers:  answers,
	}, nil
}

func MarkCompleted(tx *gorm.DB, session *models.QuizSession, result interface{}) error {
	if session == nil {
		return errors.New("quiz session tidak tersedia")
	}
	resultJSON, err := json.Marshal(result)
	if err != nil {
		return err
	}
	now := time.Now()
	session.Status = StatusCompleted
	session.ResultJSON = string(resultJSON)
	session.SubmittedAt = &now
	return tx.Save(session).Error
}
