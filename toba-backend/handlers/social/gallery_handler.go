package social

import (
	"strconv"

	"toba-backend/models"
	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerGallery(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Player ID tidak valid",
		})
	}

	result, err := player.GetPlayerGallery(uint(id))
	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil gallery",
		})
	}

	if result == nil {
		result = make([]models.PlayerGalleryCard, 0)
	}

	return c.JSON(fiber.Map{
		"total": len(result),
		"cards": result,
	})
}
