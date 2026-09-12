package battle

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
)

func MatchmakePlayer(playerID uint) (map[string]interface{}, string, int) {
	var player models.Player

	if err := config.DB.First(&player, playerID).Error; err != nil {
		return nil, "Player tidak ditemukan", 404
	}

	CleanMatchmakingQueue()

	existing := FindMatchmakingEntry(playerID)

	if existing != nil {
		if time.Since(existing.EnteredAt) >= 30*time.Second {
			RemoveMatchmakingEntry(playerID)
			return createBotMatch(&player), "", 200
		}

		return map[string]interface{}{
			"status": "SEARCHING",
		}, "", 200
	}

	opponent := PopMatchmakingOpponent(playerID, player.Level)

	if opponent == nil {
		AddMatchmakingEntry(MatchEntry{
			PlayerID:   player.ID,
			PlayerName: player.Name,
			Level:      player.Level,
			EnteredAt:  time.Now(),
		})

		return map[string]interface{}{
			"status": "SEARCHING",
		}, "", 200
	}

	roomCode := GenerateRoomCode()

	room := models.BattleRoom{
		RoomCode:      roomCode,
		HostPlayerID:  player.ID,
		GuestPlayerID: &opponent.PlayerID,
		Status:        "PLAYING",
	}

	startedAt := time.Now()
	room.StartedAt = &startedAt

	if err := config.DB.Create(&room).Error; err != nil {
		return nil, "Gagal membuat battle room", 500
	}

	AssignBattleQuestions(room.ID)

	return map[string]interface{}{
		"status":        "MATCHED",
		"room_id":       room.ID,
		"room_code":     room.RoomCode,
		"opponent_id":   opponent.PlayerID,
		"opponent_name": opponent.PlayerName,
	}, "", 200
}

func CheckMatchmaking(playerID uint) map[string]interface{} {
	entry := FindMatchmakingEntry(playerID)

	if entry != nil {
		return map[string]interface{}{
			"status": "SEARCHING",
		}
	}

	var room models.BattleRoom

	err := config.DB.
		Where(
			"(host_player_id = ? OR guest_player_id = ?) AND status = ?",
			playerID,
			playerID,
			"PLAYING",
		).
		Order("id DESC").
		First(&room).Error

	if err != nil {
		return map[string]interface{}{
			"status": "NOT_FOUND",
		}
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
		"status":        "MATCHED",
		"room_id":       room.ID,
		"room_code":     room.RoomCode,
		"opponent_id":   opponentID,
		"opponent_name": opponentName,
	}
}

func CancelMatchmaking(playerID uint) map[string]interface{} {
	RemoveMatchmakingEntry(playerID)

	return map[string]interface{}{
		"status": "CANCELLED",
	}
}

func createBotMatch(player *models.Player) map[string]interface{} {
	roomCode := GenerateRoomCode()

	room := models.BattleRoom{
		RoomCode:     roomCode,
		HostPlayerID: player.ID,
		Status:       "PLAYING",
	}

	startedAt := time.Now()
	room.StartedAt = &startedAt

	if err := config.DB.Create(&room).Error; err != nil {
		return map[string]interface{}{
			"status":  "ERROR",
			"message": "Gagal membuat bot match",
		}
	}

	AssignBattleQuestions(room.ID)

	return map[string]interface{}{
		"status":        "MATCHED",
		"room_id":       room.ID,
		"room_code":     room.RoomCode,
		"opponent_id":   nil,
		"opponent_name": "Bot Toba",
	}
}
