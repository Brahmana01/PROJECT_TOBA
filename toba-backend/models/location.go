package models

import "time"

type Location struct {
	ID             uint       `gorm:"primaryKey" json:"id"`
	Name           string     `json:"name"`
	Description    string     `json:"description"`
	Image          string     `json:"image"`
	RewardXP       int        `json:"reward_xp"`
	RewardStars    int        `json:"reward_stars"`
	QuestionCount  int        `json:"question_count"`
	MinimumCorrect int        `json:"minimum_correct"`
	LevelOrder     int        `gorm:"unique" json:"level_order"`
	IsActive       bool       `json:"is_active"`
	DeletedAt      *time.Time `json:"deleted_at,omitempty"`
	CreatedAt      time.Time  `json:"created_at"`
	UpdatedAt      time.Time  `json:"updated_at"`
}

type LocationFact struct {
	ID         uint      `gorm:"primaryKey" json:"id"`
	LocationID uint      `json:"location_id"`
	Fact       string    `json:"fact"`
	CreatedAt  time.Time `json:"created_at"`
}

type LocationQuestion struct {
	ID          uint                     `gorm:"primaryKey" json:"id"`
	LocationID  uint                     `json:"location_id"`
	Question    string                   `json:"question"`
	Explanation string                   `json:"explanation"`
	IsActive    bool                     `json:"is_active"`
	Options     []LocationQuestionOption `gorm:"foreignKey:QuestionID" json:"options"`
}

type LocationQuestionOption struct {
	ID         uint   `gorm:"primaryKey" json:"id"`
	QuestionID uint   `json:"question_id"`
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

type PlayerLocationProgress struct {
	ID            uint       `gorm:"primaryKey" json:"id"`
	PlayerID      uint       `json:"player_id"`
	LocationID    uint       `json:"location_id"`
	AttemptCount  int        `json:"attempt_count"`
	Score         int        `json:"score"`
	CorrectAnswer int        `json:"correct_answer"`
	WrongAnswer   int        `json:"wrong_answer"`
	XPEarned      int        `json:"xp_earned"`
	StarsEarned   int        `json:"stars_earned"`
	IsUnlocked    bool       `json:"is_unlocked"`
	IsCompleted   bool       `json:"is_completed"`
	CompletedAt   *time.Time `json:"completed_at,omitempty"`
	CreatedAt     time.Time  `json:"created_at"`
	UpdatedAt     time.Time  `json:"updated_at"`
}
