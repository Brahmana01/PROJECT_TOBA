package battle

import (
	"errors"
	"log"
	"time"

	"toba-backend/config"
	"toba-backend/models"
	playerprogression "toba-backend/services/player"

	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

const (
	BattleWinnerRewardXP    = 50
	BattleWinnerRewardStars = 2
)

type FinishBattleResult struct {
	Status        string `json:"status"`
	HostScore     int64  `json:"host_score"`
	GuestScore    int64  `json:"guest_score"`
	WinnerID      *uint  `json:"winner_id"`
	RewardXP      int    `json:"reward_xp"`
	RewardStars   int    `json:"reward_stars"`
	PreviousLevel int    `json:"previous_level"`
	Level         int    `json:"level"`
	LeveledUp     bool   `json:"leveled_up"`
	TotalXP       int    `json:"total_xp"`
	TotalStars    int    `json:"total_stars"`
	XPForNext     int    `json:"xp_for_next"`
	XPProgress    int    `json:"xp_progress"`
}

func countCorrectAnswers(tx *gorm.DB, roomID uint, playerID uint) (int64, error) {
	var count int64
	err := tx.Model(&models.BattleAnswer{}).
		Where("room_id = ? AND player_id = ? AND is_correct = ?", roomID, playerID, true).
		Count(&count).Error
	return count, err
}

func countSubmittedAnswers(tx *gorm.DB, roomID uint, playerID uint) (int64, error) {
	var count int64
	err := tx.Model(&models.BattleAnswer{}).
		Where("room_id = ? AND player_id = ?", roomID, playerID).
		Count(&count).Error
	return count, err
}

func getBattleScores(tx *gorm.DB, room models.BattleRoom) (int64, int64, error) {
	hostScore, err := countCorrectAnswers(tx, room.ID, room.HostPlayerID)
	if err != nil {
		return 0, 0, err
	}
	var guestScore int64
	if room.GuestPlayerID != nil {
		guestScore, err = countCorrectAnswers(tx, room.ID, *room.GuestPlayerID)
		if err != nil {
			return 0, 0, err
		}
	}
	return hostScore, guestScore, nil
}

func awardBattleWinner(tx *gorm.DB, winnerID uint) (*playerprogression.ProgressionResult, error) {
	var winner models.Player
	if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&winner, winnerID).Error; err != nil {
		return nil, err
	}
	return playerprogression.ApplyRewardToLockedPlayer(
		tx, &winner, BattleWinnerRewardXP, BattleWinnerRewardStars,
	)
}

func FinishBattle(roomID uint, playerID uint) (*FinishBattleResult, string, int) {
	if roomID == 0 || playerID == 0 {
		return nil, "Data room atau pemain tidak valid", 400
	}

	var response FinishBattleResult
	responseStatus := 200
	businessStatus := 0
	businessMessage := ""

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&room, roomID).Error; err != nil {
			return err
		}

		isHost := room.HostPlayerID == playerID
		isGuest := room.GuestPlayerID != nil && *room.GuestPlayerID == playerID
		if !isHost && !isGuest {
			businessStatus, businessMessage = 403, "Player bukan peserta battle"
			return errors.New(businessMessage)
		}

		if room.Status == "FINISHED" {
			hostScore, guestScore, err := getBattleScores(tx, room)
			if err != nil {
				return err
			}
			response = FinishBattleResult{
				Status: room.Status, HostScore: hostScore,
				GuestScore: guestScore, WinnerID: room.WinnerPlayerID,
			}
			// Nilai reward hanya dilaporkan ulang kepada pemenang; tidak diberikan lagi.
			if room.WinnerPlayerID != nil && *room.WinnerPlayerID == playerID {
				response.RewardXP = BattleWinnerRewardXP
				response.RewardStars = BattleWinnerRewardStars
			}
			return nil
		}

		if room.Status != "PLAYING" {
			businessStatus, businessMessage = 409, "Battle belum berlangsung atau sudah tidak aktif"
			return errors.New(businessMessage)
		}

		var assignedCount int64
		if err := tx.Model(&models.BattleRoomQuestion{}).Where("room_id = ?", room.ID).Count(&assignedCount).Error; err != nil {
			return err
		}
		if assignedCount != int64(models.BattleQuestionCount) {
			businessStatus, businessMessage = 409, "Data soal Battle tidak lengkap"
			return errors.New(businessMessage)
		}

		ownAnswers, err := countSubmittedAnswers(tx, room.ID, playerID)
		if err != nil {
			return err
		}
		if ownAnswers != int64(models.BattleQuestionCount) {
			businessStatus, businessMessage = 409, "Selesaikan seluruh 10 soal sebelum mengakhiri Battle"
			return errors.New(businessMessage)
		}

		hostScore, guestScore, err := getBattleScores(tx, room)
		if err != nil {
			return err
		}

		// Room tanpa guest adalah pertandingan Bot. Skor Bot tetap lokal,
		// sehingga server menutup room tanpa memberikan reward berbasis skor client.
		if room.GuestPlayerID != nil {
			guestAnswers, err := countSubmittedAnswers(tx, room.ID, *room.GuestPlayerID)
			if err != nil {
				return err
			}
			if guestAnswers != int64(models.BattleQuestionCount) {
				response = FinishBattleResult{
					Status: "WAITING_OPPONENT", HostScore: hostScore, GuestScore: guestScore,
				}
				responseStatus = 202
				return nil
			}
		}

		var winnerID *uint
		if room.GuestPlayerID != nil {
			if hostScore > guestScore {
				winnerID = &room.HostPlayerID
			} else if guestScore > hostScore {
				winnerID = room.GuestPlayerID
			}
		}

		finishedAt := time.Now()
		room.Status = "FINISHED"
		room.WinnerPlayerID = winnerID
		room.FinishedAt = &finishedAt
		if err := tx.Save(&room).Error; err != nil {
			return err
		}

		response = FinishBattleResult{
			Status: room.Status, HostScore: hostScore,
			GuestScore: guestScore, WinnerID: winnerID,
		}

		if winnerID != nil {
			progression, err := awardBattleWinner(tx, *winnerID)
			if err != nil {
				return err
			}
			response.RewardXP = progression.RewardXP
			response.RewardStars = progression.RewardStars
			response.PreviousLevel = progression.PreviousLevel
			response.Level = progression.Level
			response.LeveledUp = progression.LeveledUp
			response.TotalXP = progression.TotalXP
			response.TotalStars = progression.TotalStars
			response.XPForNext = progression.XPForNext
			response.XPProgress = progression.XPProgress
		}
		return nil
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Room tidak ditemukan", 404
		}
		if businessStatus != 0 {
			return nil, businessMessage, businessStatus
		}
		return nil, "Gagal menyelesaikan Battle atau memberikan reward", 500
	}
	if response.Status == "FINISHED" {
		var finishedRoom models.BattleRoom
		if err := config.DB.First(&finishedRoom, roomID).Error; err == nil {
			participants := []uint{finishedRoom.HostPlayerID}
			if finishedRoom.GuestPlayerID != nil {
				participants = append(participants, *finishedRoom.GuestPlayerID)
			}
			for _, participantID := range participants {
				if _, unlockErr := playerprogression.CheckAndUnlockAchievements(participantID); unlockErr != nil {
					log.Printf("[Achievement] Gagal memeriksa setelah Battle player=%d: %v", participantID, unlockErr)
				}
			}
		}
	}
	return &response, "", responseStatus
}

func DisconnectBattle(roomID uint, playerID uint) (uint, string, int) {
	if roomID == 0 || playerID == 0 {
		return 0, "Data room atau pemain tidak valid", 400
	}

	var winnerID uint
	businessMessage := ""
	businessStatus := 0

	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var room models.BattleRoom
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&room, roomID).Error; err != nil {
			return err
		}

		if room.Status == "FINISHED" {
			businessMessage, businessStatus = "Battle sudah selesai", 409
			return errors.New(businessMessage)
		}
		if room.Status != "PLAYING" {
			businessMessage, businessStatus = "Battle tidak sedang berlangsung", 409
			return errors.New(businessMessage)
		}

		if room.HostPlayerID == playerID {
			if room.GuestPlayerID == nil {
				// Bot room: tidak ada lawan manusia yang dapat menerima kemenangan.
				room.Status = "FINISHED"
				finishedAt := time.Now()
				room.FinishedAt = &finishedAt
				return tx.Save(&room).Error
			}
			winnerID = *room.GuestPlayerID
		} else if room.GuestPlayerID != nil && *room.GuestPlayerID == playerID {
			winnerID = room.HostPlayerID
		} else {
			businessMessage, businessStatus = "Player bukan peserta battle", 403
			return errors.New(businessMessage)
		}

		finishedAt := time.Now()
		room.Status = "FINISHED"
		room.WinnerPlayerID = &winnerID
		room.FinishedAt = &finishedAt
		if err := tx.Save(&room).Error; err != nil {
			return err
		}

		if _, err := awardBattleWinner(tx, winnerID); err != nil {
			return err
		}
		return nil
	})

	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return 0, "Room tidak ditemukan", 404
		}
		if businessStatus != 0 {
			return 0, businessMessage, businessStatus
		}
		return 0, "Gagal memproses disconnect Battle", 500
	}
	if winnerID > 0 {
		if _, unlockErr := playerprogression.CheckAndUnlockAchievements(winnerID); unlockErr != nil {
			log.Printf("[Achievement] Gagal memeriksa setelah forfeit Battle player=%d: %v", winnerID, unlockErr)
		}
	}
	return winnerID, "", 200
}
