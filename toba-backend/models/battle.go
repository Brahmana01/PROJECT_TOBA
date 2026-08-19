package models

import "time"

type BattleQuestion struct {
	ID          uint                   `gorm:"primaryKey" json:"id"`
	Question    string                 `json:"question"`
	Explanation string                 `json:"explanation"`
	IsActive    bool                   `json:"is_active"`
	Options     []BattleQuestionOption `gorm:"foreignKey:QuestionID" json:"options"`
	CreatedAt   time.Time              `json:"created_at"`
	UpdatedAt   time.Time              `json:"updated_at"`
}

type BattleQuestionOption struct {
	ID         uint   `gorm:"primaryKey" json:"id"`
	QuestionID uint   `json:"question_id"`
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

type BattleRoom struct {
	ID             uint       `gorm:"primaryKey" json:"id"`
	RoomCode       string     `json:"room_code"`
	HostPlayerID   uint       `json:"host_player_id"`
	GuestPlayerID  *uint      `json:"guest_player_id,omitempty"`
	WinnerPlayerID *uint      `json:"winner_player_id,omitempty"`
	Status         string     `json:"status"`
	CreatedAt      time.Time  `json:"created_at"`
	StartedAt      *time.Time `json:"started_at,omitempty"`
	FinishedAt     *time.Time `json:"finished_at,omitempty"`
	ExpiredAt      *time.Time `json:"expired_at,omitempty"`
}

type BattleRoomQuestion struct {
	ID            uint `gorm:"primaryKey" json:"id"`
	RoomID        uint `json:"room_id"`
	QuestionID    uint `json:"question_id"`
	QuestionOrder int  `json:"question_order"`
}

type BattleAnswer struct {
	ID               uint      `gorm:"primaryKey" json:"id"`
	RoomID           uint      `json:"room_id"`
	PlayerID         uint      `json:"player_id"`
	QuestionID       uint      `json:"question_id"`
	SelectedOptionID uint      `json:"selected_option_id"`
	IsCorrect        bool      `json:"is_correct"`
	AnsweredAt       time.Time `json:"answered_at"`
}
