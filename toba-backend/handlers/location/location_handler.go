package location

import (
	"toba-backend/services/location"

	"github.com/gofiber/fiber/v2"
)

func GetAllLocations(c *fiber.Ctx) error {
	locations, err := location.GetAllLocations()

	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil data lokasi",
		})
	}

	return c.JSON(locations)
}

func GetLocationDetail(c *fiber.Ctx) error {
	location, err := location.GetLocationDetail(c.Params("id"))

	if err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Lokasi tidak ditemukan",
		})
	}

	return c.JSON(location)
}

func GetLocationFact(c *fiber.Ctx) error {
	fact, err := location.GetLocationFact(c.Params("id"))

	if err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	return c.JSON(fiber.Map{
		"fact": fact,
	})
}
