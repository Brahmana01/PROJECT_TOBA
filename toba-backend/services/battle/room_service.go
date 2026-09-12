package battle

import (
	"fmt"
	"time"

	"toba-backend/config"
	"toba-backend/models"
)

type CreateRoomResult struct {
	RoomID    uint       `json:"room_id"`
	RoomCode  string     `json:"room_code"`
	Status    string     `json:"status"`
	ExpiredAt *time.Time `json:"expired_at"`
}

type JoinRoomResult struct {
	RoomID    uint                    `json:"room_id"`
	RoomCode  string                  `json:"room_code"`
	Status    string                  `json:"status"`
	Questions []models.BattleQuestion `json:"questions"`
}

type RoomStatusResult struct {
	RoomID        uint                    `json:"room_id"`
	RoomCode      string                  `json:"room_code"`
	Status        string                  `json:"status"`
	HostPlayerID  uint                    `json:"host_player_id"`
	GuestPlayerID *uint                   `json:"guest_player_id"`
	WinnerID      *uint                   `json:"winner_id"`
	HostName      string                  `json:"host_name"`
	GuestName     string                  `json:"guest_name"`
	Questions     []models.BattleQuestion `json:"questions"`
}

func CreateBattleRoom(playerID uint) (*CreateRoomResult, error) {
	var existingRoom models.BattleRoom

	err := config.DB.
		Where(
			"host_player_id = ? AND status IN ?",
			playerID,
			[]string{"WAITING", "PLAYING"},
		).
		First(&existingRoom).Error

	if err == nil {
		return &CreateRoomResult{
			RoomID:    existingRoom.ID,
			RoomCode:  existingRoom.RoomCode,
			Status:    existingRoom.Status,
			ExpiredAt: existingRoom.ExpiredAt,
		}, nil
	}

	roomCode := GenerateRoomCode()
	expiredAt := time.Now().Add(5 * time.Minute)

	room := models.BattleRoom{
		RoomCode:     roomCode,
		HostPlayerID: playerID,
		Status:       "WAITING",
		ExpiredAt:    &expiredAt,
	}

	if err := config.DB.Create(&room).Error; err != nil {
		return nil, err
	}

	return &CreateRoomResult{
		RoomID:    room.ID,
		RoomCode:  room.RoomCode,
		Status:    room.Status,
		ExpiredAt: room.ExpiredAt,
	}, nil
}

func JoinBattleRoom(
	playerID uint,
	roomCode string,
) (*JoinRoomResult, string, int) {

	var room models.BattleRoom

	if err := config.DB.
		Where("room_code = ?", roomCode).
		First(&room).Error; err != nil {
		return nil, "Room tidak ditemukan", 404
	}

	if room.Status != "WAITING" {
		return nil, "Room tidak tersedia", 400
	}

	if room.ExpiredAt != nil &&
		time.Now().After(*room.ExpiredAt) {

		room.Status = "EXPIRED"
		config.DB.Save(&room)

		return nil, "Room sudah expired", 400
	}

	if room.HostPlayerID == playerID {
		return nil, "Tidak dapat join room sendiri", 400
	}

	room.GuestPlayerID = &playerID
	room.Status = "PLAYING"

	startedAt := time.Now()
	room.StartedAt = &startedAt
	room.ExpiredAt = nil

	if err := config.DB.Save(&room).Error; err != nil {
		return nil, "Gagal memperbarui room", 500
	}

	questions := GetRoomBattleQuestionsFromDB(room.ID)

	if len(questions) == 0 {
		AssignBattleQuestions(room.ID)
		questions = GetRoomBattleQuestionsFromDB(room.ID)
	}

	return &JoinRoomResult{
		RoomID:    room.ID,
		RoomCode:  room.RoomCode,
		Status:    room.Status,
		Questions: questions,
	}, "", 200
}

func GetBattleRoomStatus(
	roomCode string,
) (*RoomStatusResult, string, int) {

	var room models.BattleRoom

	if err := config.DB.
		Where("room_code = ?", roomCode).
		First(&room).Error; err != nil {
		return nil, "Room tidak ditemukan", 404
	}

	if room.Status == "WAITING" &&
		room.ExpiredAt != nil &&
		time.Now().After(*room.ExpiredAt) {

		room.Status = "EXPIRED"
		config.DB.Save(&room)
	}

	var host models.Player

	config.DB.First(&host, room.HostPlayerID)

	var guestName string

	if room.GuestPlayerID != nil {
		var guest models.Player

		if err := config.DB.
			First(&guest, *room.GuestPlayerID).
			Error; err == nil {

			guestName = guest.Name
		}
	}

	var questions []models.BattleQuestion

	if room.Status == "PLAYING" {
		questions = GetRoomBattleQuestionsFromDB(room.ID)
	}

	return &RoomStatusResult{
		RoomID:        room.ID,
		RoomCode:      room.RoomCode,
		Status:        room.Status,
		HostPlayerID:  room.HostPlayerID,
		GuestPlayerID: room.GuestPlayerID,
		WinnerID:      room.WinnerPlayerID,
		HostName:      host.Name,
		GuestName:     guestName,
		Questions:     questions,
	}, "", 200
}

func ValidateBattleRoom(
	roomID uint,
) (*models.BattleRoom, error) {

	var room models.BattleRoom

	if err := config.DB.
		First(&room, roomID).
		Error; err != nil {

		return nil, fmt.Errorf("room tidak ditemukan")
	}

	return &room, nil
}
