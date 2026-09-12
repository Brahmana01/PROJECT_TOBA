package player

import (
	"toba-backend/config"
	"toba-backend/models"
)

func GetLeaderboard() ([]map[string]interface{}, error) {
	var players []models.Player

	if err := config.DB.
		Order("stars DESC").
		Order("xp DESC").
		Order("created_at ASC").
		Find(&players).Error; err != nil {
		return nil, err
	}

	result := make([]map[string]interface{}, 0, len(players))

	for index, currentPlayer := range players {
		titleName := ""

		if currentPlayer.TitleID > 0 {
			var title models.Title

			if err := config.DB.
				First(&title, currentPlayer.TitleID).
				Error; err == nil {

				titleName = title.TitleName
			}
		}

		result = append(result, map[string]interface{}{
			"rank":         index + 1,
			"id":           currentPlayer.ID,
			"name":         currentPlayer.Name,
			"username":     currentPlayer.Username,
			"level":        currentPlayer.Level,
			"stars":        currentPlayer.Stars,
			"xp":           currentPlayer.XP,
			"title_name":   titleName,
			"character_id": currentPlayer.CharacterID,
		})
	}

	return result, nil
}
