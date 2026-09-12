package player

import (
	"toba-backend/config"
	"toba-backend/models"
)

func GetProfile(playerID uint) (map[string]interface{}, error) {
	var currentPlayer models.Player

	if err := config.DB.First(&currentPlayer, playerID).Error; err != nil {
		return nil, err
	}

	var character *models.Character

	if currentPlayer.CharacterID > 0 {
		var selectedCharacter models.Character

		if err := config.DB.
			First(&selectedCharacter, currentPlayer.CharacterID).
			Error; err == nil {

			character = &selectedCharacter
		}
	}

	var title string

	if currentPlayer.TitleID > 0 {
		var playerTitle models.Title

		if err := config.DB.
			First(&playerTitle, currentPlayer.TitleID).
			Error; err == nil {

			title = playerTitle.TitleName
		}
	}

	var completedLocations int64

	config.DB.
		Model(&models.PlayerLocationProgress{}).
		Where(
			"player_id = ? AND is_completed = ?",
			playerID,
			true,
		).
		Count(&completedLocations)

	var galleryCount int64

	config.DB.
		Model(&models.PlayerGalleryCard{}).
		Where("player_id = ?", playerID).
		Count(&galleryCount)

	var achievements []models.PlayerAchievement

	config.DB.
		Preload("Achievement").
		Where("player_id = ?", playerID).
		Find(&achievements)

	return map[string]interface{}{
		"id":                  currentPlayer.ID,
		"user_code":           currentPlayer.UserCode,
		"name":                currentPlayer.Name,
		"username":            currentPlayer.Username,
		"level":               currentPlayer.Level,
		"xp":                  currentPlayer.XP,
		"stars":               currentPlayer.Stars,
		"title":               title,
		"selected_character":  character,
		"completed_locations": completedLocations,
		"achievements":        achievements,
		"gallery_count":       galleryCount,
	}, nil
}
