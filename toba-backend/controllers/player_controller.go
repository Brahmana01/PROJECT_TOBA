package controllers

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// Ambil Detail Profil (Poin #15 Master Plan)
func GetProfile(c *fiber.Ctx) error {
	playerID := c.Params("id")

	type ProfileResult struct {
		ID          uint   `json:"id"`
		UserCode    string `json:"user_code"`
		Username    string `json:"username"`
		Name        string `json:"name"`
		Level       int    `json:"level"`
		XP          int    `json:"xp"`
		Stars       int    `json:"stars"`
		CharacterID int    `json:"character_id"`
		TitleName   string `json:"title_name"`
	}

	var result ProfileResult
	err := config.DB.Raw(`
		SELECT 
			p.id,
			p.user_code,
			p.username,
			p.name,
			p.level,
			p.xp,
			p.stars,
			p.character_id,
			t.title_name
		FROM players p
		LEFT JOIN titles t ON t.id = p.title_id
		WHERE p.id = ?
	`, playerID).Scan(&result).Error

	if err != nil || result.ID == 0 {
		return c.Status(404).JSON(fiber.Map{"message": "Player tidak ditemukan"})
	}

	// Hitung statistik tambahan
	var locationDone int64
	config.DB.Model(&models.PlayerLocationProgress{}).
		Where("player_id = ? AND is_completed = ?", playerID, true).
		Count(&locationDone)

	var achievementCount int64
	config.DB.Model(&models.PlayerAchievement{}).
		Where("player_id = ?", playerID).
		Count(&achievementCount)

	var galleryCount int64
	config.DB.Model(&models.PlayerGalleryCard{}).
		Where("player_id = ?", playerID).
		Count(&galleryCount)

	// XP untuk naik level berikutnya
	xpForNextLevel := result.Level * 500
	xpProgress := result.XP - ((result.Level - 1) * 500)

	return c.JSON(fiber.Map{
		"id":                result.ID,
		"user_code":         result.UserCode,
		"username":          result.Username,
		"name":              result.Name,
		"level":             result.Level,
		"xp":                result.XP,
		"stars":             result.Stars,
		"character_id":      result.CharacterID,
		"title_name":        result.TitleName,
		"location_done":     locationDone,
		"achievement_count": achievementCount,
		"gallery_count":     galleryCount,
		"xp_for_next":       xpForNextLevel,
		"xp_progress":       xpProgress,
	})
}

// Ambil Top 10 Pemain (Poin #13 Master Plan)
func GetLeaderboard(c *fiber.Ctx) error {
	type LeaderboardRaw struct {
		ID          uint   `json:"id"`
		Name        string `json:"name"`
		Username    string `json:"username"`
		Level       int    `json:"level"`
		Stars       int    `json:"stars"`
		XP          int    `json:"xp"`
		TitleName   string `json:"title_name"`
		CharacterID int    `json:"character_id"`
	}

	var entries []LeaderboardRaw
	config.DB.Raw(`
		SELECT 
			p.id,
			p.name,
			p.username,
			p.level,
			p.stars,
			p.xp,
			p.character_id,
			t.title_name
		FROM players p
		LEFT JOIN titles t ON t.id = p.title_id
		WHERE p.is_active = true
		ORDER BY p.stars DESC, p.xp DESC, p.created_at ASC
		LIMIT 100
	`).Scan(&entries)

	// Tambah rank
	result := make([]fiber.Map, len(entries))
	for i, e := range entries {
		result[i] = fiber.Map{
			"rank":         i + 1,
			"id":           e.ID,
			"name":         e.Name,
			"username":     e.Username,
			"level":        e.Level,
			"stars":        e.Stars,
			"xp":           e.XP,
			"title_name":   e.TitleName,
			"character_id": e.CharacterID,
		}
	}

	return c.JSON(result)
}
