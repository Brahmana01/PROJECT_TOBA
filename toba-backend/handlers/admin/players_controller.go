package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func GetAllPlayers(c *fiber.Ctx) error {
	var players []models.Player

	config.DB.
		Where("is_active = ?", true).
		Order("created_at DESC").
		Find(&players)

	return c.JSON(fiber.Map{
		"total":   len(players),
		"players": players,
	})
}
