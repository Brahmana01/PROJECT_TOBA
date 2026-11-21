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
	RoomCode         string `json:"room_code"`
	Status           string `json:"status"`
	HostScore        int64  `json:"host_score"`
	GuestScore       int64  `json:"guest_score"`
	WinnerID         *uint  `json:"winner_id"`
	WinnerType       string `json:"winner_type"`
	BotAnsweredCount int    `json:"bot_answered_count"`
	RewardXP         int    `json:"reward_xp"`
	RewardStars      int    `json:"reward_stars"`
	PreviousLevel    int    `json:"previous_level"`
	Level            int    `json:"level"`
	LeveledUp        bool   `json:"leveled_up"`
	TotalXP          int    `json:"total_xp"`
	TotalStars       int    `json:"total_stars"`
	XPForNext        int    `json:"xp_for_next"`
	XPProgress       int    `json:"xp_progress"`
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
	if isBotRoom(room) {
		guestScore = int64(room.BotScore)
	} else if room.GuestPlayerID != nil {
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

func winnerTypeForRoom(room models.BattleRoom, hostScore, guestScore int64) string {
	if room.FinishReason == "DISCONNECT" {
		if isBotRoom(room) {
			return "BOT"
		}
		if room.WinnerPlayerID == nil {
			return "DRAW"
		}
		if *room.WinnerPlayerID == room.HostPlayerID {
			return "HOST"
		}
		return "GUEST"
	}
	if hostScore == guestScore {
		return "DRAW"
	}
	if isBotRoom(room) {
		if hostScore > guestScore {
			return "PLAYER"
		}
		return "BOT"
	}
	if hostScore > guestScore {
		return "HOST"
	}
	return "GUEST"
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

		isBot := isBotRoom(room)
		if isBot {
			normalizeBotRoom(&room)
		}

		if room.Status == "FINISHED" {
			hostScore, guestScore, err := getBattleScores(tx, room)
			if err != nil {
				return err
			}
			response = FinishBattleResult{
				RoomCode: room.RoomCode, Status: room.Status, HostScore: hostScore,
				GuestScore: guestScore, WinnerID: room.WinnerPlayerID,
				WinnerType:       winnerTypeForRoom(room, hostScore, guestScore),
				BotAnsweredCount: room.BotAnsweredCount,
			}
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

		if isBot {
			if _, err := advanceBotProgressTx(tx, &room, time.Now()); err != nil {
				return err
			}
			if room.BotAnsweredCount < models.BattleQuestionCount {
				hostScore, guestScore, err := getBattleScores(tx, room)
				if err != nil {
					return err
				}
				response = FinishBattleResult{
					RoomCode: room.RoomCode, Status: "WAITING_OPPONENT", HostScore: hostScore,
					GuestScore: guestScore, BotAnsweredCount: room.BotAnsweredCount,
				}
				responseStatus = 202
				return nil
			}
		} else {
			if room.GuestPlayerID == nil {
				businessStatus, businessMessage = 409, "Peserta kedua belum bergabung"
				return errors.New(businessMessage)
			}
			hostAnswers, err := countSubmittedAnswers(tx, room.ID, room.HostPlayerID)
			if err != nil {
				return err
			}
			guestAnswers, err := countSubmittedAnswers(tx, room.ID, *room.GuestPlayerID)
			if err != nil {
				return err
			}
			if hostAnswers != int64(models.BattleQuestionCount) || guestAnswers != int64(models.BattleQuestionCount) {
				hostScore, guestScore, err := getBattleScores(tx, room)
				if err != nil {
					return err
				}
				response = FinishBattleResult{
					RoomCode: room.RoomCode, Status: "WAITING_OPPONENT", HostScore: hostScore,
					GuestScore: guestScore,
				}
				responseStatus = 202
				return nil
			}
		}

		hostScore, guestScore, err := getBattleScores(tx, room)
		if err != nil {
			return err
		}

		var winnerID *uint
		winnerType := winnerTypeForRoom(room, hostScore, guestScore)
		if isBot {
			if winnerType == "PLAYER" {
				winnerID = &room.HostPlayerID
			}
		} else {
			switch winnerType {
			case "HOST":
				winnerID = &room.HostPlayerID
			case "GUEST":
				winnerID = room.GuestPlayerID
			}
		}

		finishedAt := time.Now()
		room.Status = "FINISHED"
		room.WinnerPlayerID = winnerID
		room.FinishReason = "COMPLETED"
		room.FinishedAt = &finishedAt
		if err := tx.Save(&room).Error; err != nil {
			return err
		}

		response = FinishBattleResult{
			RoomCode: room.RoomCode, Status: room.Status, HostScore: hostScore,
			GuestScore: guestScore, WinnerID: winnerID,
			WinnerType: winnerType, BotAnsweredCount: room.BotAnsweredCount,
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

		isBot := isBotRoom(room)
		if isBot {
			normalizeBotRoom(&room)
		}
		if room.HostPlayerID == playerID {
			if isBot || room.GuestPlayerID == nil {
				room.Status = "FINISHED"
				room.FinishReason = "DISCONNECT"
				finishedAt := time.Now()
				room.FinishedAt = &finishedAt
				room.WinnerPlayerID = nil
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
		room.FinishReason = "DISCONNECT"
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
			log.Printf("[Achievement] Gagal memeriksa setelah disconnect Battle player=%d: %v", winnerID, unlockErr)
		}
	}
	return winnerID, "", 200
}
