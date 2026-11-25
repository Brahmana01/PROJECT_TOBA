package models

import "time"

type WeeklyChallenge struct {
	ID          uint      `gorm:"primaryKey" json:"id"`
	Title       string    `json:"title"`
	Description string    `json:"description"`
	GameType    string    `json:"game_type"`
	RewardXP    int       `json:"reward_xp"`
	RewardStars int       `json:"reward_stars"`
	IsActive    bool      `json:"is_active"`
	StartDate   time.Time `json:"start_date"`
	EndDate     time.Time `json:"end_date"`
	DataJSON    string    `json:"data_json" gorm:"type:text"`
	CreatedAt   time.Time `json:"created_at"`
}

// WeeklyChallengeSession stores a per-player snapshot of the public game data
// and the private answer key. AnswerKeyJSON is never serialized to Unity.
type WeeklyChallengeSession struct {
	ID             uint       `gorm:"primaryKey" json:"id"`
	SessionID      string     `gorm:"uniqueIndex;size:64;not null" json:"session_id"`
	PlayerID       uint       `gorm:"index;not null" json:"player_id"`
	ChallengeID    uint       `gorm:"index;not null" json:"challenge_id"`
	Title          string     `json:"title"`
	Description    string     `json:"description"`
	GameType       string     `json:"game_type"`
	RewardXP       int        `json:"reward_xp"`
	RewardStars    int        `json:"reward_stars"`
	PublicDataJSON string     `gorm:"type:text" json:"-"`
	AnswerKeyJSON  string     `gorm:"type:text" json:"-"`
	ResultJSON     string     `gorm:"type:text" json:"-"`
	StartedAt      time.Time  `json:"started_at"`
	ExpiresAt      time.Time  `gorm:"index" json:"expires_at"`
	CompletedAt    *time.Time `json:"completed_at,omitempty"`
	CreatedAt      time.Time  `json:"created_at"`
	UpdatedAt      time.Time  `json:"updated_at"`
}

type PlayerChallengeProgress struct {
	ID             uint       `gorm:"primaryKey" json:"id"`
	PlayerID       uint       `json:"player_id"`
	ChallengeID    uint       `json:"challenge_id"`
	IsCompleted    bool       `json:"is_completed"`
	Score          int        `json:"score"`
	ScoreValidated bool       `gorm:"not null;default:false" json:"-"`
	CompletedAt    *time.Time `json:"completed_at"`
	CreatedAt      time.Time  `json:"created_at"`
}
