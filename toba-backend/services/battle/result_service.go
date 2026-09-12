package battle

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"
	"toba-backend/services/player"
)

type FinishBattleResult struct {
	Status     string `json:"status"`
	HostScore  int64  `json:"host_score"`
	GuestScore int64  `json:"guest_score"`
	WinnerID   *uint  `json:"winner_id"`
}

func FinishBattle(roomID uint) (*FinishBattleResult, string, int) {
	var room models.BattleRoom

	if err := config.DB.First(&room, roomID).Error; err != nil {
		return nil, "Room tidak ditemukan", 404
	}

	if room.Status == "FINISHED" {
		var hostScore int64
		var guestScore int64

		config.DB.
			Model(&models.BattleAnswer{}).
			Where(
				"room_id = ? AND player_id = ? AND is_correct = ?",
				roomID,
				room.HostPlayerID,
				true,
			).
			Count(&hostScore)

		if room.GuestPlayerID != nil {
			config.DB.
				Model(&models.BattleAnswer{}).
				Where(
					"room_id = ? AND player_id = ? AND is_correct = ?",
					roomID,
					*room.GuestPlayerID,
					true,
				).
				Count(&guestScore)
		}

		return &FinishBattleResult{
			Status:     room.Status,
			HostScore:  hostScore,
			GuestScore: guestScore,
			WinnerID:   room.WinnerPlayerID,
		}, "", 200
	}

	var hostScore int64
	var guestScore int64

	config.DB.
		Model(&models.BattleAnswer{}).
		Where(
			"room_id = ? AND player_id = ? AND is_correct = ?",
			roomID,
			room.HostPlayerID,
			true,
		).
		Count(&hostScore)

	if room.GuestPlayerID != nil {
		config.DB.
			Model(&models.BattleAnswer{}).
			Where(
				"room_id = ? AND player_id = ? AND is_correct = ?",
				roomID,
				*room.GuestPlayerID,
				true,
			).
			Count(&guestScore)
	}

	var winnerID *uint

	if hostScore > guestScore {
		winnerID = &room.HostPlayerID
	} else if guestScore > hostScore && room.GuestPlayerID != nil {
		winnerID = room.GuestPlayerID
	}

	room.Status = "FINISHED"
	room.WinnerPlayerID = winnerID

	finishedAt := time.Now()
	room.FinishedAt = &finishedAt

	if err := config.DB.Save(&room).Error; err != nil {
		return nil, "Gagal menyelesaikan battle", 500
	}

	if winnerID != nil {
		var winner models.Player

		if err := config.DB.First(&winner, *winnerID).Error; err == nil {
			winner.XP += 50
			winner.Stars += 2

			newLevel := (winner.XP / 500) + 1

			if newLevel > winner.Level {
				winner.Level = newLevel
				player.UpdatePlayerTitle(&winner)
			}

			if err := config.DB.Save(&winner).Error; err != nil {
				return nil, "Gagal menyimpan reward winner", 500
			}
		}
	}

	return &FinishBattleResult{
		Status:     room.Status,
		HostScore:  hostScore,
		GuestScore: guestScore,
		WinnerID:   winnerID,
	}, "", 200
}

func DisconnectBattle(roomID uint, playerID uint) (uint, string, int) {
	var room models.BattleRoom

	if err := config.DB.First(&room, roomID).Error; err != nil {
		return 0, "Room tidak ditemukan", 404
	}

	if room.Status != "PLAYING" {
		return 0, "Battle tidak sedang berlangsung", 400
	}

	if room.HostPlayerID == playerID {
		if room.GuestPlayerID == nil {
			return 0, "Tidak ada lawan", 400
		}

		room.WinnerPlayerID = room.GuestPlayerID

	} else if room.GuestPlayerID != nil &&
		*room.GuestPlayerID == playerID {

		room.WinnerPlayerID = &room.HostPlayerID

	} else {
		return 0, "Player bukan peserta battle", 403
	}

	room.Status = "FINISHED"

	finishedAt := time.Now()
	room.FinishedAt = &finishedAt

	if err := config.DB.Save(&room).Error; err != nil {
		return 0, "Gagal menyelesaikan battle", 500
	}

	if room.WinnerPlayerID == nil {
		return 0, "Winner tidak ditemukan", 500
	}

	return *room.WinnerPlayerID, "", 200
}
