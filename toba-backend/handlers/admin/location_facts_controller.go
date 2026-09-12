package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetLocationFacts(c *fiber.Ctx) error {
	locationID := c.Params("id")
	var facts []models.LocationFact
	config.DB.Where("location_id = ?", locationID).Find(&facts)
	return c.JSON(facts)
}

func AdminCreateLocationFact(c *fiber.Ctx) error {
	locationID := c.Params("id")

	type Input struct {
		Fact string `json:"fact"`
	}

	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	locID := parseUint(locationID)
	fact := models.LocationFact{
		LocationID: locID,
		Fact:       input.Fact,
	}

	config.DB.Create(&fact)

	return c.Status(201).JSON(fact)
}

func AdminDeleteLocationFact(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Delete(&models.LocationFact{}, id)
	return c.JSON(fiber.Map{"message": "Fakta berhasil dihapus"})
}
