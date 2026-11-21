package battle

import (
	"time"

	"toba-backend/models"

	"gorm.io/gorm"
)

const (
	defaultBattleBotName = "Bot Toba"
	battleBotMinDelayMS  = 700
	battleBotDelayRange  = 900
)

// A room tanpa guest manusia yang sudah PLAYING adalah pertandingan Bot.
// IsBot tetap disimpan eksplisit untuk room baru; inferensi ini menjaga room
// Bot lama tetap dapat dibaca setelah kolom baru ditambahkan oleh AutoMigrate.
func isBotRoom(room models.BattleRoom) bool {
	return room.IsBot || (room.GuestPlayerID == nil && room.Status == "PLAYING")
}

func normalizeBotRoom(room *models.BattleRoom) {
	if room == nil || !isBotRoom(*room) {
		return
	}
	room.IsBot = true
	if room.BotName == "" {
		room.BotName = defaultBattleBotName
	}
	if room.BotLevel < 1 {
		room.BotLevel = 1
	}
}

// botRoll menghasilkan angka deterministik per room dan nomor soal. Dengan
// demikian, retry request atau restart backend tidak mengubah jawaban Bot.
func botRoll(roomID uint, questionIndex int) uint64 {
	x := uint64(roomID)*0x9E3779B185EBCA87 + uint64(questionIndex)*0xC2B2AE3D27D4EB4F
	x ^= x >> 30
	x *= 0xBF58476D1CE4E5B9
	x ^= x >> 27
	x *= 0x94D049BB133111EB
	x ^= x >> 31
	return x
}

func botThinkDelay(roomID uint, questionIndex int) time.Duration {
	return time.Duration(battleBotMinDelayMS+int(botRoll(roomID, questionIndex)%battleBotDelayRange)) * time.Millisecond
}

func botAccuracyPercent(level int) uint64 {
	switch {
	case level <= 3:
		return 35
	case level <= 7:
		return 55
	default:
		return 75
	}
}

func botAnswersCorrectly(roomID uint, questionIndex, level int) bool {
	return botRoll(roomID+uint(level)*7919, questionIndex+97)%100 < botAccuracyPercent(level)
}

// advanceBotProgressTx menghitung progres Bot dari waktu server sejak room
// dimulai. Skor tidak pernah diterima dari Unity/client. Hasilnya persisten
// pada battle_rooms sehingga konsisten setelah reconnect atau restart server.
func advanceBotProgressTx(tx *gorm.DB, room *models.BattleRoom, now time.Time) (bool, error) {
	if room == nil || !isBotRoom(*room) || room.Status != "PLAYING" {
		return false, nil
	}

	wasStarted := room.StartedAt != nil
	wasBot := room.IsBot
	wasName := room.BotName
	wasLevel := room.BotLevel
	normalizeBotRoom(room)
	if room.StartedAt == nil {
		started := now
		room.StartedAt = &started
	}

	elapsed := now.Sub(*room.StartedAt)
	var cumulative time.Duration
	newAnsweredCount := 0
	for index := 1; index <= models.BattleQuestionCount; index++ {
		cumulative += botThinkDelay(room.ID, index)
		if elapsed >= cumulative {
			newAnsweredCount = index
		} else {
			break
		}
	}

	if newAnsweredCount < room.BotAnsweredCount {
		newAnsweredCount = room.BotAnsweredCount
	}
	if newAnsweredCount > models.BattleQuestionCount {
		newAnsweredCount = models.BattleQuestionCount
	}

	newScore := 0
	for index := 1; index <= newAnsweredCount; index++ {
		if botAnswersCorrectly(room.ID, index, room.BotLevel) {
			newScore++
		}
	}

	changed := !wasBot || wasName == "" || wasLevel < 1 ||
		room.BotAnsweredCount != newAnsweredCount || room.BotScore != newScore || !wasStarted
	room.IsBot = true
	room.BotName = defaultBattleBotName
	room.BotAnsweredCount = newAnsweredCount
	room.BotScore = newScore

	if !changed {
		return false, nil
	}

	updates := map[string]interface{}{
		"is_bot":             true,
		"bot_name":           room.BotName,
		"bot_level":          room.BotLevel,
		"bot_answered_count": room.BotAnsweredCount,
		"bot_score":          room.BotScore,
		"started_at":         room.StartedAt,
	}
	if err := tx.Model(&models.BattleRoom{}).Where("id = ?", room.ID).Updates(updates).Error; err != nil {
		return false, err
	}
	return true, nil
}

func botLevelForPlayer(playerLevel int) int {
	if playerLevel < 1 {
		return 1
	}
	if playerLevel > 10 {
		return 10
	}
	return playerLevel
}
