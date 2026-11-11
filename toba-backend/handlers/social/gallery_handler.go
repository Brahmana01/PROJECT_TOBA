package social

import (
	"strconv"

	"toba-backend/services/player"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerGallery(c *fiber.Ctx) error {
	playerID := c.Params("id")

	id, err := strconv.ParseUint(playerID, 10, 64)
	if err != nil {
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

	return c.JSON(fiber.Map{
		"total": len(result),
		"cards": result,
	})
}
