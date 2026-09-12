package location

import (
	"toba-backend/services/location"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerProgress(c *fiber.Ctx) error {
	progress, err := location.GetPlayerLocationProgress(
		c.Params("id"),
	)

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil progress player",
		})
	}

	return c.JSON(progress)
}
