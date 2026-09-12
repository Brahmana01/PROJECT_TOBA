package models

import "time"

type Player struct {
	ID          uint       `gorm:"primaryKey" json:"id"`
	UserCode    string     `json:"user_code"`
	Username    string     `json:"username"`
	Name        string     `json:"name"`
	PinHash     string     `json:"-"`
	CharacterID int        `json:"character_id"`
	TitleID     int        `json:"title_id"`
	Level       int        `json:"level"`
	XP          int        `json:"xp"`
	Stars       int        `json:"stars"`
	IsActive    bool       `json:"is_active"`
	LastLogin   *time.Time `json:"last_login,omitempty"`
	CreatedAt   time.Time  `json:"created_at"`
	UpdatedAt   time.Time  `json:"updated_at"`
	FCMToken string `gorm:"column:fcm_token" json:"fcm_token,omitempty"`
}

type PlayerSession struct {
	ID         uint       `gorm:"primaryKey" json:"id"`
	PlayerID   uint       `json:"player_id"`
	Token      string     `json:"token"`
	DeviceName string     `json:"device_name"`
	DeviceID   string     `json:"device_id"`
	IsActive   bool       `json:"is_active"`
	LastLogin  time.Time  `json:"last_login"`
	ExpiredAt  *time.Time `json:"expired_at,omitempty"`
	CreatedAt  time.Time  `json:"created_at"`
}
