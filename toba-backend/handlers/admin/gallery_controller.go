package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetGalleryCards(c *fiber.Ctx) error {
	var cards []models.GalleryCard

	config.DB.Find(&cards)

	return c.JSON(cards)
}

func AdminCreateGalleryCard(c *fiber.Ctx) error {
	type Input struct {
		LocationID  uint   `json:"location_id"`
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	card := models.GalleryCard{
		LocationID:  input.LocationID,
		Title:       input.Title,
		Image:       input.Image,
		Description: input.Description,
	}

	config.DB.Create(&card)

	return c.Status(201).JSON(card)
}

func AdminUpdateGalleryCard(c *fiber.Ctx) error {
	id := c.Params("id")

	var card models.GalleryCard

	if err := config.DB.First(&card, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Kartu tidak ditemukan",
		})
	}

	type Input struct {
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	card.Title = input.Title
	card.Image = input.Image
	card.Description = input.Description

	config.DB.Save(&card)

	return c.JSON(card)
}

func AdminDeleteGalleryCard(c *fiber.Ctx) error {
	id := c.Params("id")

	config.DB.Delete(&models.GalleryCard{}, id)

	return c.JSON(fiber.Map{
		"message": "Kartu berhasil dihapus",
	})
}