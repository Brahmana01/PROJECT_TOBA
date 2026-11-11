package player

import (
	"errors"

	"toba-backend/config"
	"toba-backend/models"

	"gorm.io/gorm"
)

// XP yang dibutuhkan untuk naik satu level.
// Konsisten dengan perhitungan level pada sistem game TOBA.
const profileXPPerLevel = 500

func GetProfile(playerID uint) (map[string]interface{}, error) {

	// ==========================================
	// 1. AMBIL DATA UTAMA PEMAIN
	// ==========================================

	var currentPlayer models.Player

	if err := config.DB.
		First(&currentPlayer, playerID).
		Error; err != nil {
		return nil, err
	}

	// ==========================================
	// 2. AMBIL KARAKTER PEMAIN
	// ==========================================

	var character *models.Character

	if currentPlayer.CharacterID > 0 {

		var selectedCharacter models.Character

		err := config.DB.
			First(&selectedCharacter, currentPlayer.CharacterID).
			Error

		if err == nil {
			character = &selectedCharacter
		} else if !errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, err
		}
	}

	// ==========================================
	// 3. AMBIL GELAR PEMAIN
	// ==========================================

	var titleName string

	if currentPlayer.TitleID > 0 {

		var playerTitle models.Title

		err := config.DB.
			First(&playerTitle, currentPlayer.TitleID).
			Error

		if err == nil {
			titleName = playerTitle.TitleName
		} else if !errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, err
		}
	}

	// ==========================================
	// 4. HITUNG LOKASI YANG SELESAI
	// ==========================================

	var completedLocations int64

	if err := config.DB.
		Model(&models.PlayerLocationProgress{}).
		Where(
			"player_id = ? AND is_completed = ?",
			playerID,
			true,
		).
		Count(&completedLocations).
		Error; err != nil {

		return nil, err
	}

	// ==========================================
	// 5. HITUNG KOLEKSI GALERI
	// ==========================================

	var galleryCount int64

	if err := config.DB.
		Model(&models.PlayerGalleryCard{}).
		Where("player_id = ?", playerID).
		Count(&galleryCount).
		Error; err != nil {

		return nil, err
	}

	// ==========================================
	// 6. AMBIL ACHIEVEMENT PEMAIN
	// ==========================================

	achievements := make([]models.PlayerAchievement, 0)

	if err := config.DB.
		Preload("Achievement").
		Where("player_id = ?", playerID).
		Find(&achievements).
		Error; err != nil {

		return nil, err
	}

	achievementCount := len(achievements)

	// ==========================================
	// 7. HITUNG PROGRESS XP
	// ==========================================

	xpForNext := profileXPPerLevel

	xpProgress := currentPlayer.XP % profileXPPerLevel

	if xpProgress < 0 {
		xpProgress = 0
	}

	// ==========================================
	// 8. SUSUN RESPONSE PROFILE
	// ==========================================

	return map[string]interface{}{

		// Identitas pemain
		"id":        currentPlayer.ID,
		"user_code": currentPlayer.UserCode,
		"name":      currentPlayer.Name,
		"username":  currentPlayer.Username,

		// Level dan reward
		"level": currentPlayer.Level,
		"xp":    currentPlayer.XP,
		"stars": currentPlayer.Stars,

		// Data karakter
		"character_id":       currentPlayer.CharacterID,
		"selected_character": character,

		// Data gelar
		"title_name": titleName,
		"title":      titleName,

		// Statistik lokasi
		"location_done":       completedLocations,
		"completed_locations": completedLocations,

		// Statistik achievement
		"achievement_count": achievementCount,
		"achievements":      achievements,

		// Statistik galeri
		"gallery_count": galleryCount,

		// Progress XP
		"xp_for_next": xpForNext,
		"xp_progress": xpProgress,
	}, nil
}
