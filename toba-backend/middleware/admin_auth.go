package middleware

import (
	"strings"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminAuthMiddleware(c *fiber.Ctx) error {
	authHeader := c.Get("Authorization")
	if authHeader == "" {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token admin tidak ditemukan",
		})
	}

	token := strings.TrimPrefix(authHeader, "Bearer ")
	if token == authHeader {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Format token admin tidak valid",
		})
	}

	var session models.AdminSession

	if err := config.DB.
		Where("token = ? AND is_active = ?", token, true).
		First(&session).Error; err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
			"message": "Token admin tidak valid atau sudah tidak aktif",
		})
	}

	c.Locals("admin_id", session.AdminID)

	return c.Next()
}
