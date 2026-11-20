package battle

import (
	"errors"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
)

func MatchmakePlayer(playerID uint) (map[string]interface{}, string, int) {
	if playerID == 0 {
		return nil, "Player tidak valid", 400
	}

	var player models.Player
	if err := config.DB.First(&player, playerID).Error; err != nil {
		return nil, "Player tidak ditemukan", 404
	}

	CleanMatchmakingQueue()

	// Pemain yang sudah dipasangkan mungkin sudah dikeluarkan dari antrean.
	// Kembalikan room aktifnya sebelum mencoba memasukkan ulang ke antrean.
	var activeRoom models.BattleRoom
	activeErr := config.DB.
		Where("(host_player_id = ? OR guest_player_id = ?) AND status = ?", playerID, playerID, "PLAYING").
		Order("id DESC").First(&activeRoom).Error
	if activeErr == nil {
		var opponentID *uint
		var opponentName string
		isHost := activeRoom.HostPlayerID == playerID
		if isHost {
			opponentID = activeRoom.GuestPlayerID
		} else {
			opponentID = &activeRoom.HostPlayerID
		}
		if opponentID != nil {
			var opponent models.Player
			if err := config.DB.First(&opponent, *opponentID).Error; err == nil {
				opponentName = opponent.Name
			}
		} else {
			opponentName = "Bot Toba"
		}
		return map[string]interface{}{
			"status": "MATCHED", "room_id": activeRoom.ID, "room_code": activeRoom.RoomCode,
			"opponent_id": opponentID, "opponent_name": opponentName,
			"is_host": isHost, "is_bot": activeRoom.GuestPlayerID == nil,
		}, "", 200
	}
	if activeErr != nil && !errors.Is(activeErr, gorm.ErrRecordNotFound) {
		return nil, "Gagal memeriksa pertandingan aktif", 500
	}

	existing := FindMatchmakingEntry(playerID)
	if existing != nil {
		if time.Since(existing.EnteredAt) >= 30*time.Second {
			RemoveMatchmakingEntry(playerID)
			return createBotMatch(&player)
		}
		return map[string]interface{}{"status": "SEARCHING"}, "", 200
	}

	opponent := PopMatchmakingOpponent(playerID, player.Level)
	if opponent == nil {
		AddMatchmakingEntry(MatchEntry{
			PlayerID: player.ID, PlayerName: player.Name,
			Level: player.Level, EnteredAt: time.Now(),
		})
		return map[string]interface{}{"status": "SEARCHING"}, "", 200
	}

	room, err := createAssignedRoom(player.ID, &opponent.PlayerID)
	if err != nil {
		// Kembalikan kedua pemain ke antrean agar kegagalan bank soal
		// tidak membuat pemain hilang dari matchmaking.
		AddMatchmakingEntry(*opponent)
		AddMatchmakingEntry(MatchEntry{
			PlayerID: player.ID, PlayerName: player.Name,
			Level: player.Level, EnteredAt: time.Now(),
		})
		return nil, "Gagal menyiapkan soal Battle: " + err.Error(), 409
	}

	return map[string]interface{}{
		"status": "MATCHED", "room_id": room.ID, "room_code": room.RoomCode,
		"opponent_id": opponent.PlayerID, "opponent_name": opponent.PlayerName,
		"is_host": true, "is_bot": false,
	}, "", 200
}

func createAssignedRoom(hostID uint, guestID *uint) (*models.BattleRoom, error) {
	var room models.BattleRoom
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		room = models.BattleRoom{
			RoomCode: GenerateRoomCode(), HostPlayerID: hostID,
			GuestPlayerID: guestID, Status: "PLAYING",
		}
		now := time.Now()
		room.StartedAt = &now
		if err := tx.Create(&room).Error; err != nil {
			return err
		}
		if _, err := assignBattleQuestionsTx(tx, room.ID); err != nil {
			return err
		}
		return nil
	})
	if err != nil {
		return nil, err
	}
	return &room, nil
}

func CheckMatchmaking(playerID uint) map[string]interface{} {
	entry := FindMatchmakingEntry(playerID)
	if entry != nil {
		return map[string]interface{}{"status": "SEARCHING"}
	}

	var room models.BattleRoom
	err := config.DB.
		Where("(host_player_id = ? OR guest_player_id = ?) AND status IN ?", playerID, playerID, []string{"PLAYING", "FINISHED"}).
		Order("id DESC").First(&room).Error
	if err != nil {
		return map[string]interface{}{"status": "NOT_FOUND"}
	}

	var opponentID *uint
	var opponentName string
	if room.HostPlayerID == playerID {
		opponentID = room.GuestPlayerID
	} else {
		opponentID = &room.HostPlayerID
	}
	if opponentID != nil {
		var opponent models.Player
		if err := config.DB.First(&opponent, *opponentID).Error; err == nil {
			opponentName = opponent.Name
		}
	}

	return map[string]interface{}{
		"status": "MATCHED", "room_id": room.ID, "room_code": room.RoomCode,
		"opponent_id": opponentID, "opponent_name": opponentName,
		"is_host": room.HostPlayerID == playerID, "is_bot": room.GuestPlayerID == nil,
	}
}

func CancelMatchmaking(playerID uint) map[string]interface{} {
	RemoveMatchmakingEntry(playerID)
	return map[string]interface{}{"status": "CANCELLED"}
}

func createBotMatch(player *models.Player) (map[string]interface{}, string, int) {
	room, err := createAssignedRoom(player.ID, nil)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Data pemain tidak ditemukan", 404
		}
		return nil, "Gagal membuat pertandingan Bot: " + err.Error(), 409
	}
	return map[string]interface{}{
		"status": "MATCHED", "room_id": room.ID, "room_code": room.RoomCode,
		"opponent_id": nil, "opponent_name": "Bot Toba",
		"is_host": true, "is_bot": true,
	}, "", 200
}
