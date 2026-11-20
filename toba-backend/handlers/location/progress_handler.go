package location

import (
	"strconv"

	"toba-backend/middleware"
	locationService "toba-backend/services/location"

	"github.com/gofiber/fiber/v2"
)

func GetPlayerProgress(c *fiber.Ctx) error {
	playerID, err := middleware.RequirePathPlayerID(
		c,
		c.Params("id"),
	)

	if err != nil {
		return err
	}

	progress, err := locationService.GetPlayerLocationProgress(
		strconv.FormatUint(uint64(playerID), 10),
	)

	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil progress player",
		})
	}

	return c.JSON(progress)
}
