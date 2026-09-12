package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func GetAdminStats(c *fiber.Ctx) error {
	var totalPlayers int64
	config.DB.Model(&models.Player{}).Where("is_active = ?", true).Count(&totalPlayers)

	var totalBattle int64
	config.DB.Model(&models.BattleRoom{}).Where("status = ?", "FINISHED").Count(&totalBattle)

	var totalTrivia int64
	config.DB.Model(&models.PlayerTriviaHistory{}).Count(&totalTrivia)

	var totalLocation int64
	config.DB.Model(&models.Location{}).Where("deleted_at IS NULL").Count(&totalLocation)

	return c.JSON(fiber.Map{
		"total_players":  totalPlayers,
		"total_battle":   totalBattle,
		"total_trivia":   totalTrivia,
		"total_location": totalLocation,
	})
}
