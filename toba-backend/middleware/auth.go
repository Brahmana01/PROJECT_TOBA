package middleware

import (
	"strings"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AuthMiddleware(c *fiber.Ctx) error {
	if strings.HasPrefix(c.Path(), "/api/admin") {
		return c.Next()
	}

	authHeader := c.Get("Authorization")
	if authHeader == "" {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token tidak ditemukan",
		})
	}

	token := strings.TrimPrefix(authHeader, "Bearer ")
	if token == authHeader {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Format token tidak valid",
		})
	}

	var session models.PlayerSession

	if err := config.DB.
		Where("token = ? AND is_active = ?", token, true).
		First(&session).Error; err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token tidak valid atau sudah tidak aktif",
		})
	}

	c.Locals("player_id", session.PlayerID)

	return c.Next()
}
