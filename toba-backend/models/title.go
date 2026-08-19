package models

import "time"

type Title struct {
	ID           uint      `gorm:"primaryKey" json:"id"`
	TitleName    string    `json:"title_name"`
	MinimumLevel int       `json:"minimum_level"`
	CreatedAt    time.Time `json:"created_at"`
	UpdatedAt    time.Time `json:"updated_at"`
}

type Character struct {
	ID          uint      `gorm:"primaryKey" json:"id"`
	Name        string    `json:"name"`
	Image       string    `json:"image"`
	Description string    `json:"description"`
	CreatedAt   time.Time `json:"created_at"`
}
