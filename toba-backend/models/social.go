package models

import "time"

type GalleryCard struct {
	ID          uint      `gorm:"primaryKey" json:"id"`
	LocationID  uint      `json:"location_id"`
	Title       string    `json:"title"`
	Image       string    `json:"image"`
	Description string    `json:"description"`
	CreatedAt   time.Time `json:"created_at"`
}

type PlayerGalleryCard struct {
	ID            uint        `gorm:"primaryKey" json:"id"`
	PlayerID      uint        `json:"player_id"`
	GalleryCardID uint        `json:"gallery_card_id"`
	ObtainedAt    time.Time   `json:"obtained_at"`
	Card          GalleryCard `gorm:"foreignKey:GalleryCardID" json:"card"`
}

type Achievement struct {
	ID             uint      `gorm:"primaryKey" json:"id"`
	Name           string    `json:"name"`
	Description    string    `json:"description"`
	Icon           string    `json:"icon"`
	ConditionType  string    `json:"condition_type"`
	ConditionValue int       `json:"condition_value"`
	RewardXP       int       `json:"reward_xp"`
	RewardStars    int       `json:"reward_stars"`
	IsActive       bool      `json:"is_active"`
	CreatedAt      time.Time `json:"created_at"`
}

type PlayerAchievement struct {
	ID            uint        `gorm:"primaryKey" json:"id"`
	PlayerID      uint        `json:"player_id"`
	AchievementID uint        `json:"achievement_id"`
	Claimed       bool        `json:"claimed"`
	ObtainedAt    time.Time   `json:"obtained_at"`
	ClaimedAt     *time.Time  `json:"claimed_at,omitempty"`
	Achievement   Achievement `gorm:"foreignKey:AchievementID" json:"achievement"`
}

type LeaderboardEntry struct {
	ID          uint   `json:"id"`
	Name        string `json:"name"`
	Username    string `json:"username"`
	Level       int    `json:"level"`
	Stars       int    `json:"stars"`
	XP          int    `json:"xp"`
	TitleName   string `json:"title_name"`
	CharacterID int    `json:"character_id"`
	Rank        int    `json:"rank"`
}

type Admin struct {
	ID           uint      `gorm:"primaryKey" json:"id"`
	Fullname     string    `json:"fullname"`
	Username     string    `gorm:"unique" json:"username"`
	PasswordHash string    `json:"-"`
	Role         string    `json:"role"`
	CreatedAt    time.Time `json:"created_at"`
	UpdatedAt    time.Time `json:"updated_at"`
}
