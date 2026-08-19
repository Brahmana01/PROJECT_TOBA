package controllers

import (
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// GET /api/player/:id/gallery
// Ambil semua kartu galeri milik player
// ============================================================
func GetPlayerGallery(c *fiber.Ctx) error {
	playerID := c.Params("id")

	var cards []models.PlayerGalleryCard
	config.DB.
		Preload("Card").
		Where("player_id = ?", playerID).
		Order("obtained_at DESC").
		Find(&cards)

	return c.JSON(fiber.Map{
		"total": len(cards),
		"cards": cards,
	})
}

// ============================================================
// GET /api/player/:id/achievements
// Ambil semua achievement milik player
// ============================================================
func GetPlayerAchievements(c *fiber.Ctx) error {
	playerID := c.Params("id")

	var achievements []models.PlayerAchievement
	config.DB.
		Preload("Achievement").
		Where("player_id = ?", playerID).
		Order("obtained_at DESC").
		Find(&achievements)

	return c.JSON(fiber.Map{
		"total":        len(achievements),
		"achievements": achievements,
	})
}

// ============================================================
// POST /api/player/:id/achievement/claim
// Klaim reward achievement
// ============================================================
func ClaimAchievement(c *fiber.Ctx) error {
	type ClaimInput struct {
		PlayerID      uint `json:"player_id"`
		AchievementID uint `json:"achievement_id"`
	}

	input := new(ClaimInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var pa models.PlayerAchievement
	if err := config.DB.
		Preload("Achievement").
		Where("player_id = ? AND achievement_id = ?", input.PlayerID, input.AchievementID).
		First(&pa).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Achievement tidak ditemukan"})
	}

	if pa.Claimed {
		return c.Status(400).JSON(fiber.Map{"message": "Achievement sudah diklaim"})
	}

	// Beri reward ke player
	var player models.Player
	config.DB.First(&player, input.PlayerID)
	player.XP += pa.Achievement.RewardXP
	player.Stars += pa.Achievement.RewardStars

	// Level up check
	newLevel := (player.XP / 500) + 1
	if newLevel > player.Level {
		player.Level = newLevel
		updatePlayerTitle(&player)
	}
	config.DB.Save(&player)

	// Tandai sudah diklaim
	now := time.Now()
	pa.Claimed = true
	pa.ClaimedAt = &now
	config.DB.Save(&pa)

	return c.JSON(fiber.Map{
		"message":      "Achievement berhasil diklaim!",
		"reward_xp":    pa.Achievement.RewardXP,
		"reward_stars": pa.Achievement.RewardStars,
		"level":        player.Level,
		"total_xp":     player.XP,
		"total_stars":  player.Stars,
	})
}

// ============================================================
// GET /api/leaderboard
// Ambil leaderboard global
// ============================================================
// func GetLeaderboard(c *fiber.Ctx) error {
// 	type LeaderboardRaw struct {
// 		ID          uint   `json:"id"`
// 		Name        string `json:"name"`
// 		Username    string `json:"username"`
// 		Level       int    `json:"level"`
// 		Stars       int    `json:"stars"`
// 		XP          int    `json:"xp"`
// 		TitleName   string `json:"title_name"`
// 		CharacterID int    `json:"character_id"`
// 	}

// 	var entries []LeaderboardRaw
// 	config.DB.Raw(`
// 		SELECT 
// 			p.id,
// 			p.name,
// 			p.username,
// 			p.level,
// 			p.stars,
// 			p.xp,
// 			p.character_id,
// 			t.title_name
// 		FROM players p
// 		LEFT JOIN titles t ON t.id = p.title_id
// 		WHERE p.is_active = true
// 		ORDER BY p.stars DESC, p.xp DESC, p.created_at ASC
// 		LIMIT 100
// 	`).Scan(&entries)

// 	// Tambah rank
// 	result := make([]fiber.Map, len(entries))
// 	for i, e := range entries {
// 		result[i] = fiber.Map{
// 			"rank":         i + 1,
// 			"id":           e.ID,
// 			"name":         e.Name,
// 			"username":     e.Username,
// 			"level":        e.Level,
// 			"stars":        e.Stars,
// 			"xp":           e.XP,
// 			"title_name":   e.TitleName,
// 			"character_id": e.CharacterID,
// 		}
// 	}

// 	return c.JSON(result)
// }

// ============================================================
// GET /api/profile/:id
// Ambil profil lengkap player
// ============================================================
// func GetProfile(c *fiber.Ctx) error {
// 	playerID := c.Params("id")

// 	type ProfileResult struct {
// 		ID          uint   `json:"id"`
// 		UserCode    string `json:"user_code"`
// 		Username    string `json:"username"`
// 		Name        string `json:"name"`
// 		Level       int    `json:"level"`
// 		XP          int    `json:"xp"`
// 		Stars       int    `json:"stars"`
// 		CharacterID int    `json:"character_id"`
// 		TitleName   string `json:"title_name"`
// 	}

// 	var result ProfileResult
// 	err := config.DB.Raw(`
// 		SELECT 
// 			p.id,
// 			p.user_code,
// 			p.username,
// 			p.name,
// 			p.level,
// 			p.xp,
// 			p.stars,
// 			p.character_id,
// 			t.title_name
// 		FROM players p
// 		LEFT JOIN titles t ON t.id = p.title_id
// 		WHERE p.id = ?
// 	`, playerID).Scan(&result).Error

// 	if err != nil || result.ID == 0 {
// 		return c.Status(404).JSON(fiber.Map{"message": "Player tidak ditemukan"})
// 	}

// 	// Hitung statistik tambahan
// 	var locationDone int64
// 	config.DB.Model(&models.PlayerLocationProgress{}).
// 		Where("player_id = ? AND is_completed = ?", playerID, true).
// 		Count(&locationDone)

// 	var achievementCount int64
// 	config.DB.Model(&models.PlayerAchievement{}).
// 		Where("player_id = ?", playerID).
// 		Count(&achievementCount)

// 	var galleryCount int64
// 	config.DB.Model(&models.PlayerGalleryCard{}).
// 		Where("player_id = ?", playerID).
// 		Count(&galleryCount)

// 	// XP untuk naik level berikutnya
// 	xpForNextLevel := result.Level * 500
// 	xpProgress := result.XP - ((result.Level - 1) * 500)

// 	return c.JSON(fiber.Map{
// 		"id":                result.ID,
// 		"user_code":         result.UserCode,
// 		"username":          result.Username,
// 		"name":              result.Name,
// 		"level":             result.Level,
// 		"xp":                result.XP,
// 		"stars":             result.Stars,
// 		"character_id":      result.CharacterID,
// 		"title_name":        result.TitleName,
// 		"location_done":     locationDone,
// 		"achievement_count": achievementCount,
// 		"gallery_count":     galleryCount,
// 		"xp_for_next":       xpForNextLevel,
// 		"xp_progress":       xpProgress,
// 	})
// }

// ============================================================
// GET /api/check-achievements/:id
// Cek & unlock achievement baru untuk player
// ============================================================
func CheckAndUnlockAchievements(c *fiber.Ctx) error {
	playerID := c.Params("id")

	var player models.Player
	if err := config.DB.First(&player, playerID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Player tidak ditemukan"})
	}

	// Ambil semua achievement aktif
	var achievements []models.Achievement
	config.DB.Where("is_active = ?", true).Find(&achievements)

	var newlyUnlocked []models.Achievement

	for _, ach := range achievements {
		// Cek apakah sudah punya
		var existing models.PlayerAchievement
		err := config.DB.Where("player_id = ? AND achievement_id = ?",
			playerID, ach.ID).First(&existing).Error
		if err == nil {
			continue // Sudah punya
		}

		// Cek kondisi
		unlocked := false
		switch ach.ConditionType {
		case "LEVEL":
			unlocked = player.Level >= ach.ConditionValue
		case "STAR":
			unlocked = player.Stars >= ach.ConditionValue
		case "XP":
			unlocked = player.XP >= ach.ConditionValue
		case "LOCATION":
			var count int64
			config.DB.Model(&models.PlayerLocationProgress{}).
				Where("player_id = ? AND is_completed = ?", playerID, true).
				Count(&count)
			unlocked = int(count) >= ach.ConditionValue
		case "TRIVIA":
			var count int64
			config.DB.Model(&models.PlayerTriviaHistory{}).
				Where("player_id = ?", playerID).
				Count(&count)
			unlocked = int(count) >= ach.ConditionValue
		case "BATTLE":
			var count int64
			config.DB.Model(&models.BattleRoom{}).
				Where("winner_player_id = ?", playerID).
				Count(&count)
			unlocked = int(count) >= ach.ConditionValue
		}

		if unlocked {
			pa := models.PlayerAchievement{
				PlayerID:      player.ID,
				AchievementID: ach.ID,
				Claimed:       false,
				ObtainedAt:    time.Now(),
			}
			config.DB.Create(&pa)
			newlyUnlocked = append(newlyUnlocked, ach)
		}
	}

	return c.JSON(fiber.Map{
		"newly_unlocked": newlyUnlocked,
		"count":          len(newlyUnlocked),
	})
}
