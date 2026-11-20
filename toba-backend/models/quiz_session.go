package models

import "time"

// QuizSession menyimpan satu sesi Jelajah atau Trivia di server.
// SnapshotJSON menyimpan soal, opsi, dan kunci jawaban saat sesi dibuat.
// Data ini membuat sesi tetap konsisten walaupun Admin mengubah bank soal.
type QuizSession struct {
	ID           uint       `gorm:"primaryKey" json:"id"`
	SessionID    string     `gorm:"size:36;uniqueIndex;not null" json:"session_id"`
	PlayerID     uint       `gorm:"index:idx_quiz_session_owner_target,priority:1;not null" json:"player_id"`
	GameMode     string     `gorm:"size:20;index:idx_quiz_session_owner_target,priority:2;not null" json:"game_mode"`
	TargetID     uint       `gorm:"index:idx_quiz_session_owner_target,priority:3;not null" json:"target_id"`
	SnapshotJSON string     `gorm:"type:text;not null" json:"-"`
	AnswersJSON  string     `gorm:"type:text;not null;default:'[]'" json:"-"`
	ResultJSON   string     `gorm:"type:text" json:"-"`
	Status       string     `gorm:"size:20;index;not null" json:"status"`
	ExpiresAt    time.Time  `gorm:"index;not null" json:"expires_at"`
	SubmittedAt  *time.Time `json:"submitted_at,omitempty"`
	CreatedAt    time.Time  `json:"created_at"`
	UpdatedAt    time.Time  `json:"updated_at"`
}

// QuizSessionQuestionSnapshot adalah salinan permanen satu soal dalam sesi.
// IsCorrect hanya tersimpan di database dan tidak pernah dikirim pada GET quiz.
type QuizSessionQuestionSnapshot struct {
	QuestionID  uint                        `json:"question_id"`
	Question    string                      `json:"question"`
	Explanation string                      `json:"explanation"`
	Options     []QuizSessionOptionSnapshot `json:"options"`
}

type QuizSessionOptionSnapshot struct {
	OptionID   uint   `json:"option_id"`
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

// QuizSessionAnswerSnapshot menyimpan satu jawaban yang sudah diverifikasi server.
type QuizSessionAnswerSnapshot struct {
	QuestionID       uint      `json:"question_id"`
	SelectedOptionID uint      `json:"selected_option_id"`
	CorrectOptionID  uint      `json:"correct_option_id"`
	IsCorrect        bool      `json:"is_correct"`
	Explanation      string    `json:"explanation"`
	AnsweredAt       time.Time `json:"answered_at"`
}
