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
	RoomID             uint                     `json:"room_id"`
	RoomCode           string                   `json:"room_code"`
	Status             string                   `json:"status"`
	HostPlayerID       uint                     `json:"host_player_id"`
	GuestPlayerID      *uint                    `json:"guest_player_id"`
	WinnerID           *uint                    `json:"winner_id"`
	WinnerType         string                   `json:"winner_type"`
	HostName           string                   `json:"host_name"`
	GuestName          string                   `json:"guest_name"`
	HostScore          int64                    `json:"host_score"`
	GuestScore         int64                    `json:"guest_score"`
	HostAnsweredCount  int64                    `json:"host_answered_count"`
	GuestAnsweredCount int64                    `json:"guest_answered_count"`
	IsBot              bool                     `json:"is_bot"`
	BotName            string                   `json:"bot_name,omitempty"`
	BotLevel           int                      `json:"bot_level,omitempty"`
	BotAnsweredCount   int                      `json:"bot_answered_count"`
	Questions          []BattleQuestionResponse `json:"questions"`
	ScoreChanged       bool                     `json:"-"`
}

func CreateBattleRoom(playerID uint) (*CreateRoomResult, error) {
	var existingRoom models.BattleRoom

	err := config.DB.
		Where("host_player_id = ? AND status IN ?", playerID, []string{"WAITING", "PLAYING"}).
		First(&existingRoom).Error

	if err == nil {
		return &CreateRoomResult{
			RoomID: existingRoom.ID, RoomCode: existingRoom.RoomCode,
			Status: existingRoom.Status, ExpiredAt: existingRoom.ExpiredAt,
		}, nil
	}
	if !errors.Is(err, gorm.ErrRecordNotFound) {
		return nil, err
	}

	expiredAt := time.Now().Add(5 * time.Minute)
	room := models.BattleRoom{
		RoomCode: GenerateRoomCode(), HostPlayerID: playerID,
		Status: "WAITING", ExpiredAt: &expiredAt, IsBot: false,
	}
	if err := config.DB.Create(&room).Error; err != nil {
		return nil, err
	}

	return &CreateRoomResult{
		RoomID: room.ID, RoomCode: room.RoomCode,
		Status: room.Status, ExpiredAt: room.ExpiredAt,
	}, nil
}

func JoinBattleRoom(playerID uint, roomCode string) (*JoinRoomResult, string, int) {
	if playerID == 0 || roomCode == "" {
		return nil, "Data pemain atau kode room tidak valid", 400
	}

	var response *JoinRoomResult
	var businessStatus int
	var businessMessage string

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("room_code = ?", roomCode).First(&room).Error; err != nil {
			return err
		}

		if room.Status != "WAITING" {
			businessStatus, businessMessage = 400, "Room tidak tersedia"
			return errors.New(businessMessage)
		}
		if room.ExpiredAt != nil && time.Now().After(*room.ExpiredAt) {
			room.Status = "EXPIRED"
			if err := tx.Save(&room).Error; err != nil {
				return err
			}
			businessStatus, businessMessage = 400, "Room sudah expired"
			return errors.New(businessMessage)
		}
		if room.HostPlayerID == playerID {
			businessStatus, businessMessage = 400, "Tidak dapat join room sendiri"
			return errors.New(businessMessage)
		}
		if room.GuestPlayerID != nil {
			businessStatus, businessMessage = 400, "Room sudah memiliki peserta kedua"
			return errors.New(businessMessage)
		}

		questions, err := assignBattleQuestionsTx(tx, room.ID)
		if err != nil {
			businessStatus, businessMessage = 409, err.Error()
			return err
		}

		now := time.Now()
		room.GuestPlayerID = &playerID
		room.Status = "PLAYING"
		room.StartedAt = &now
		room.ExpiredAt = nil
		room.IsBot = false
		room.BotName = ""
		room.BotLevel = 0
		room.BotScore = 0
		room.BotAnsweredCount = 0
		if err := tx.Save(&room).Error; err != nil {
			return err
		}

		response = &JoinRoomResult{
			RoomID: room.ID, RoomCode: room.RoomCode,
			Status: room.Status, Questions: questions,
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

func GetBattleRoomStatus(roomCode string, playerID uint) (*RoomStatusResult, string, int) {
	if playerID == 0 || roomCode == "" {
		return nil, "Identitas pemain atau kode room tidak valid", 401
	}

	var response RoomStatusResult
	var businessStatus int
	var businessMessage string

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("room_code = ?", roomCode).First(&room).Error; err != nil {
			return err
		}

		isHost := room.HostPlayerID == playerID
		isGuest := room.GuestPlayerID != nil && *room.GuestPlayerID == playerID
		if !isHost && !isGuest {
			businessStatus, businessMessage = 403, "Player bukan peserta room"
			return errors.New(businessMessage)
		}

		if room.Status == "WAITING" && room.ExpiredAt != nil && time.Now().After(*room.ExpiredAt) {
			room.Status = "EXPIRED"
			if err := tx.Save(&room).Error; err != nil {
				return err
			}
		}

		isBot := isBotRoom(room)
		scoreChanged := false
		if isBot && room.Status == "PLAYING" {
			var advanceErr error
			scoreChanged, advanceErr = advanceBotProgressTx(tx, &room, time.Now())
			if advanceErr != nil {
				return advanceErr
			}
		}
		if isBot {
			normalizeBotRoom(&room)
		}

		var host models.Player
		if err := tx.First(&host, room.HostPlayerID).Error; err != nil {
			return err
		}

		guestName := ""
		if room.GuestPlayerID != nil {
			var guest models.Player
			if err := tx.First(&guest, *room.GuestPlayerID).Error; err != nil {
				return err
			}
			guestName = guest.Name
		} else if isBot {
			guestName = room.BotName
		}

		hostScore, guestScore, scoreErr := getBattleScores(tx, room)
		if scoreErr != nil {
			return scoreErr
		}
		hostAnswers, err := countSubmittedAnswers(tx, room.ID, room.HostPlayerID)
		if err != nil {
			return err
		}
		var guestAnswers int64
		if isBot {
			guestAnswers = int64(room.BotAnsweredCount)
		} else if room.GuestPlayerID != nil {
			guestAnswers, err = countSubmittedAnswers(tx, room.ID, *room.GuestPlayerID)
			if err != nil {
				return err
			}
		}

		var questions []BattleQuestionResponse
		if room.Status == "PLAYING" {
			questions = GetRoomBattleQuestionsFromDB(room.ID)
			if len(questions) != models.BattleQuestionCount {
				businessStatus, businessMessage = 409, "Data soal Battle tidak lengkap"
				return errors.New(businessMessage)
			}
		}

		winnerType := ""
		if room.Status == "FINISHED" {
			winnerType = winnerTypeForRoom(room, hostScore, guestScore)
		}

		response = RoomStatusResult{
			RoomID: room.ID, RoomCode: room.RoomCode, Status: room.Status,
			HostPlayerID: room.HostPlayerID, GuestPlayerID: room.GuestPlayerID,
			WinnerID: room.WinnerPlayerID, WinnerType: winnerType,
			HostName: host.Name, GuestName: guestName,
			HostScore: hostScore, GuestScore: guestScore,
			HostAnsweredCount: hostAnswers, GuestAnsweredCount: guestAnswers,
			IsBot: isBot, BotName: room.BotName, BotLevel: room.BotLevel,
			BotAnsweredCount: room.BotAnsweredCount,
			Questions:        questions, ScoreChanged: scoreChanged,
		}
		return nil
	})

	if err != nil {
		if businessStatus != 0 {
			return nil, businessMessage, businessStatus
		}
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Room atau pemain tidak ditemukan", 404
		}
		return nil, "Gagal mengambil status Battle", 500
	}
	return &response, "", 200
}

func ValidateBattleRoom(roomID uint) (*models.BattleRoom, error) {
	var room models.BattleRoom
	if err := config.DB.First(&room, roomID).Error; err != nil {
		return nil, errors.New("room tidak ditemukan")
	}
	return &room, nil
}
