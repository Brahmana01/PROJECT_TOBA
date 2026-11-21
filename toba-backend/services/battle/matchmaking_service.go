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

		isBot := isBotRoom(activeRoom)
		if opponentID != nil {
			var opponent models.Player
			if err := config.DB.First(&opponent, *opponentID).Error; err == nil {
				opponentName = opponent.Name
			}
		} else if isBot {
			opponentName = activeRoom.BotName
			if opponentName == "" {
				opponentName = defaultBattleBotName
			}
		}

		return map[string]interface{}{
			"status": "MATCHED", "room_id": activeRoom.ID, "room_code": activeRoom.RoomCode,
			"opponent_id": opponentID, "opponent_name": opponentName,
			"is_host": isHost, "is_bot": isBot,
			"bot_name": activeRoom.BotName, "bot_level": activeRoom.BotLevel,
			"bot_in": 0,
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
		waited := int(time.Since(existing.EnteredAt).Seconds())
		botIn := 30 - waited
		if botIn < 0 {
			botIn = 0
		}
		return map[string]interface{}{
			"status": "SEARCHING", "waited": waited, "bot_in": botIn,
			"message": "Mencari lawan...",
		}, "", 200
	}

	opponent := PopMatchmakingOpponent(playerID, player.Level)
	if opponent == nil {
		AddMatchmakingEntry(MatchEntry{
			PlayerID: player.ID, PlayerName: player.Name,
			Level: player.Level, EnteredAt: time.Now(),
		})
		return map[string]interface{}{
			"status": "SEARCHING", "waited": 0, "bot_in": 30,
			"message": "Mencari lawan...",
		}, "", 200
	}

	room, err := createAssignedRoom(player.ID, &opponent.PlayerID, 0)
	if err != nil {
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
		"is_host": true, "is_bot": false, "bot_in": 0,
	}, "", 200
}

func createAssignedRoom(hostID uint, guestID *uint, botLevel int) (*models.BattleRoom, error) {
	var room models.BattleRoom
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		room = models.BattleRoom{
			RoomCode: GenerateRoomCode(), HostPlayerID: hostID,
			GuestPlayerID: guestID, Status: "PLAYING",
			IsBot: guestID == nil,
		}
		if guestID == nil {
			room.BotName = defaultBattleBotName
			room.BotLevel = botLevelForPlayer(botLevel)
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
		waited := int(time.Since(entry.EnteredAt).Seconds())
		botIn := 30 - waited
		if botIn < 0 {
			botIn = 0
		}
		return map[string]interface{}{"status": "SEARCHING", "waited": waited, "bot_in": botIn}
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
	isBot := isBotRoom(room)
	if opponentID != nil {
		var opponent models.Player
		if err := config.DB.First(&opponent, *opponentID).Error; err == nil {
			opponentName = opponent.Name
		}
	} else if isBot {
		opponentName = room.BotName
		if opponentName == "" {
			opponentName = defaultBattleBotName
		}
	}

	return map[string]interface{}{
		"status": "MATCHED", "room_id": room.ID, "room_code": room.RoomCode,
		"opponent_id": opponentID, "opponent_name": opponentName,
		"is_host": room.HostPlayerID == playerID, "is_bot": isBot,
		"bot_name": room.BotName, "bot_level": room.BotLevel,
	}
}

func CancelMatchmaking(playerID uint) map[string]interface{} {
	RemoveMatchmakingEntry(playerID)
	return map[string]interface{}{"status": "CANCELLED"}
}

func createBotMatch(player *models.Player) (map[string]interface{}, string, int) {
	botLevel := botLevelForPlayer(player.Level)
	room, err := createAssignedRoom(player.ID, nil, botLevel)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Data pemain tidak ditemukan", 404
		}
		return nil, "Gagal membuat pertandingan Bot: " + err.Error(), 409
	}
	return map[string]interface{}{
		"status": "MATCHED", "room_id": room.ID, "room_code": room.RoomCode,
		"opponent_id": nil, "opponent_name": room.BotName,
		"is_host": true, "is_bot": true,
		"bot_name": room.BotName, "bot_level": room.BotLevel, "bot_in": 0,
	}, "", 200
}
