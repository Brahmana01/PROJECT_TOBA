package models

import "time"

type WeeklyChallenge struct {
	ID          uint      `gorm:"primaryKey" json:"id"`
	Title       string    `json:"title"`
	Description string    `json:"description"`
	GameType    string    `json:"game_type"` // MATCHING, GUESS_IMAGE, WORD_ARRANGE, dst
	RewardXP    int       `json:"reward_xp"`
	RewardStars int       `json:"reward_stars"`
	IsActive    bool      `json:"is_active"`
	StartDate   time.Time `json:"start_date"`
	EndDate     time.Time `json:"end_date"`
	DataJSON    string    `json:"data_json" gorm:"type:text"` // konten game (fleksibel per tipe)
	CreatedAt   time.Time `json:"created_at"`
}

type PlayerChallengeProgress struct {
	ID          uint       `gorm:"primaryKey" json:"id"`
	PlayerID    uint       `json:"player_id"`
	ChallengeID uint       `json:"challenge_id"`
	IsCompleted bool       `json:"is_completed"`
	Score       int        `json:"score"`
	CompletedAt *time.Time `json:"completed_at"`
	CreatedAt   time.Time  `json:"created_at"`
}
