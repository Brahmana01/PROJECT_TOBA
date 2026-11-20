package battle

import (
	"errors"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

type CreateRoomResult struct {
	RoomID    uint       `json:"room_id"`
	RoomCode  string     `json:"room_code"`
	Status    string     `json:"status"`
	ExpiredAt *time.Time `json:"expired_at"`
}

type JoinRoomResult struct {
	RoomID    uint                     `json:"room_id"`
	RoomCode  string                   `json:"room_code"`
	Status    string                   `json:"status"`
	Questions []BattleQuestionResponse `json:"questions"`
}

type RoomStatusResult struct {
	RoomID        uint                     `json:"room_id"`
	RoomCode      string                   `json:"room_code"`
	Status        string                   `json:"status"`
	HostPlayerID  uint                     `json:"host_player_id"`
	GuestPlayerID *uint                    `json:"guest_player_id"`
	WinnerID      *uint                    `json:"winner_id"`
	HostName      string                   `json:"host_name"`
	GuestName     string                   `json:"guest_name"`
	HostScore     int64                    `json:"host_score"`
	GuestScore    int64                    `json:"guest_score"`
	Questions     []BattleQuestionResponse `json:"questions"`
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

	if playerID == 0 || roomCode == "" {
		return nil, "Data pemain atau kode room tidak valid", 400
	}

	var response *JoinRoomResult
	var businessStatus int
	var businessMessage string

	err := config.DB.Transaction(func(tx *gorm.DB) error {

		var room models.BattleRoom

		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("room_code = ?", roomCode).
			First(&room).Error; err != nil {
			return err
		}

		if room.Status != "WAITING" {
			businessStatus = 400
			businessMessage = "Room tidak tersedia"
			return errors.New(businessMessage)
		}

		if room.ExpiredAt != nil &&
			time.Now().After(*room.ExpiredAt) {

			room.Status = "EXPIRED"

			if err := tx.Save(&room).Error; err != nil {
				return err
			}

			businessStatus = 400
			businessMessage = "Room sudah expired"
			return nil
		}

		if room.HostPlayerID == playerID {
			businessStatus = 400
			businessMessage = "Tidak dapat join room sendiri"
			return errors.New(businessMessage)
		}

		if room.GuestPlayerID != nil {
			businessStatus = 400
			businessMessage = "Room sudah memiliki peserta kedua"
			return errors.New(businessMessage)
		}

		// Ambil sepuluh soal dan simpan penugasannya
		// dalam transaksi yang sama dengan perubahan room.
		questions, err := assignBattleQuestionsTx(tx, room.ID)
		if err != nil {
			businessStatus = 409
			businessMessage = err.Error()
			return err
		}

		now := time.Now()

		room.GuestPlayerID = &playerID
		room.Status = "PLAYING"
		room.StartedAt = &now
		room.ExpiredAt = nil

		if err := tx.Save(&room).Error; err != nil {
			return err
		}

		response = &JoinRoomResult{
			RoomID:    room.ID,
			RoomCode:  room.RoomCode,
			Status:    room.Status,
			Questions: questions,
		}

		return nil
	})

	if err != nil {
		if businessStatus != 0 {
			return nil, businessMessage, businessStatus
		}

		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Room tidak ditemukan", 404
		}

		return nil, "Gagal bergabung ke Battle", 500
	}

	if businessStatus != 0 {
		return nil, businessMessage, businessStatus
	}

	return response, "", 200
}

func GetBattleRoomStatus(
	roomCode string,
	playerID uint,
) (*RoomStatusResult, string, int) {

	if playerID == 0 {
		return nil, "Identitas pemain tidak valid", 401
	}

	var room models.BattleRoom

	if err := config.DB.
		Where("room_code = ?", roomCode).
		First(&room).Error; err != nil {
		return nil, "Room tidak ditemukan", 404
	}

	isHost := room.HostPlayerID == playerID
	isGuest := room.GuestPlayerID != nil &&
		*room.GuestPlayerID == playerID

	if !isHost && !isGuest {
		return nil, "Player bukan peserta room", 403
	}

	if room.Status == "WAITING" &&
		room.ExpiredAt != nil &&
		time.Now().After(*room.ExpiredAt) {

		room.Status = "EXPIRED"

		if err := config.DB.Save(&room).Error; err != nil {
			return nil, "Gagal memperbarui status room", 500
		}
	}

	var host models.Player

	if err := config.DB.
		First(&host, room.HostPlayerID).Error; err != nil {
		return nil, "Data host tidak ditemukan", 500
	}

	var guestName string

	if room.GuestPlayerID != nil {
		var guest models.Player

		if err := config.DB.
			First(&guest, *room.GuestPlayerID).Error; err != nil {
			return nil, "Data guest tidak ditemukan", 500
		}

		guestName = guest.Name
	}

	var questions []BattleQuestionResponse

	if room.Status == "PLAYING" {
		questions = GetRoomBattleQuestionsFromDB(room.ID)

		if len(questions) != models.BattleQuestionCount {
			return nil, "Data soal Battle tidak lengkap", 409
		}
	}

	hostScore, guestScore, scoreErr := getBattleScores(config.DB, room)
	if scoreErr != nil {
		return nil, "Gagal mengambil skor Battle", 500
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
		HostScore:     hostScore,
		GuestScore:    guestScore,
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

		return nil, errors.New("room tidak ditemukan")
	}

	return &room, nil
}
