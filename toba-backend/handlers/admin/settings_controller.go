package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetSettings(c *fiber.Ctx) error {
	var settings []models.GameSetting

	config.DB.Find(&settings)

	return c.JSON(settings)
}

func AdminUpdateSetting(c *fiber.Ctx) error {
	key := c.Params("key")

	type Input struct {
		Value string `json:"value"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	result := config.DB.
		Model(&models.GameSetting{}).
		Where("setting_key = ?", key).
		Update("setting_value", input.Value)

	if result.RowsAffected == 0 {
		return c.Status(404).JSON(fiber.Map{
			"message": "Setting tidak ditemukan",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Setting berhasil diupdate",
		"key":     key,
		"value":   input.Value,
	})
}
