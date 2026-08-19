package models

import "time"

type TriviaCategory struct {
	ID              uint       `gorm:"primaryKey" json:"id"`
	Name            string     `json:"name"`
	Description     string     `json:"description"`
	QuestionCount   int        `json:"question_count"`
	MinimumQuestion int        `json:"minimum_question"`
	DisplayOrder    int        `json:"display_order"`
	IsActive        bool       `json:"is_active"`
	DeletedAt       *time.Time `json:"deleted_at,omitempty"`
	CreatedAt       time.Time  `json:"created_at"`
	UpdatedAt       time.Time  `json:"updated_at"`
}

type TriviaQuestion struct {
	ID          uint                   `gorm:"primaryKey" json:"id"`
	CategoryID  uint                   `json:"category_id"`
	Question    string                 `json:"question"`
	Explanation string                 `json:"explanation"`
	IsActive    bool                   `json:"is_active"`
	Options     []TriviaQuestionOption `gorm:"foreignKey:QuestionID" json:"options"`
	CreatedAt   time.Time              `json:"created_at"`
	UpdatedAt   time.Time              `json:"updated_at"`
}

type TriviaQuestionOption struct {
	ID         uint   `gorm:"primaryKey" json:"id"`
	QuestionID uint   `json:"question_id"`
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

type PlayerTriviaHistory struct {
	ID            uint      `gorm:"primaryKey" json:"id"`
	PlayerID      uint      `json:"player_id"`
	CategoryID    uint      `json:"category_id"`
	Score         int       `json:"score"`
	CorrectAnswer int       `json:"correct_answer"`
	WrongAnswer   int       `json:"wrong_answer"`
	XPEarned      int       `json:"xp_earned"`
	StarsEarned   int       `json:"stars_earned"`
	PlayedAt      time.Time `json:"played_at"`
}

type PlayerTriviaAnswer struct {
	ID               uint      `gorm:"primaryKey" json:"id"`
	HistoryID        uint      `json:"history_id"`
	QuestionID       uint      `json:"question_id"`
	SelectedOptionID uint      `json:"selected_option_id"`
	IsCorrect        bool      `json:"is_correct"`
	AnsweredAt       time.Time `json:"answered_at"`
}

type GameSetting struct {
	ID           uint      `gorm:"primaryKey" json:"id"`
	SettingKey   string    `gorm:"unique" json:"setting_key"`
	SettingValue string    `json:"setting_value"`
	Description  string    `json:"description"`
	UpdatedAt    time.Time `json:"updated_at"`
}
