package social

import (
	"toba-backend/middleware"
	"toba-backend/models"
	playerService "toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerGallery(c *fiber.Ctx) error {
	playerID, err := middleware.RequirePathPlayerID(
		c,
		c.Params("id"),
	)

	if err != nil {
		return err
	}

	result, err := playerService.GetPlayerGallery(playerID)

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
