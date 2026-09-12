package admin

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetLocations(c *fiber.Ctx) error {
	var locations []models.Location

	config.DB.
		Where("deleted_at IS NULL").
		Order("level_order ASC").
		Find(&locations)

	return c.JSON(locations)
}

func AdminCreateLocation(c *fiber.Ctx) error {
	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Image          string `json:"image"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		QuestionCount  int    `json:"question_count"`
		MinimumCorrect int    `json:"minimum_correct"`
		LevelOrder     int    `json:"level_order"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	loc := models.Location{
		Name:           input.Name,
		Description:    input.Description,
		Image:          input.Image,
		RewardXP:       input.RewardXP,
		RewardStars:    input.RewardStars,
		QuestionCount:  input.QuestionCount,
		MinimumCorrect: input.MinimumCorrect,
		LevelOrder:     input.LevelOrder,
		IsActive:       true,
	}

	if err := config.DB.Create(&loc).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal membuat lokasi",
		})
	}

	return c.Status(201).JSON(loc)
}

func AdminUpdateLocation(c *fiber.Ctx) error {
	id := c.Params("id")

	var loc models.Location

	if err := config.DB.
		Where("id = ? AND deleted_at IS NULL", id).
		First(&loc).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Lokasi tidak ditemukan",
		})
	}

	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Image          string `json:"image"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		QuestionCount  int    `json:"question_count"`
		MinimumCorrect int    `json:"minimum_correct"`
		LevelOrder     int    `json:"level_order"`
		IsActive       bool   `json:"is_active"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	loc.Name = input.Name
	loc.Description = input.Description
	loc.Image = input.Image
	loc.RewardXP = input.RewardXP
	loc.RewardStars = input.RewardStars
	loc.QuestionCount = input.QuestionCount
	loc.MinimumCorrect = input.MinimumCorrect
	loc.LevelOrder = input.LevelOrder
	loc.IsActive = input.IsActive

	config.DB.Save(&loc)

	return c.JSON(loc)
}

func AdminDeleteLocation(c *fiber.Ctx) error {
	id := c.Params("id")

	var loc models.Location

	if err := config.DB.
		Where("id = ? AND deleted_at IS NULL", id).
		First(&loc).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Lokasi tidak ditemukan",
		})
	}

	now := time.Now()
	loc.DeletedAt = &now

	config.DB.Save(&loc)

	return c.JSON(fiber.Map{
		"message": "Lokasi berhasil dihapus",
	})
}

func AdminUploadLocationImage(c *fiber.Ctx) error {
	file, err := c.FormFile("image")
	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "File tidak ditemukan",
		})
	}

	const maxImageSize = 5 * 1024 * 1024

	if file.Size > maxImageSize {
		return c.Status(400).JSON(fiber.Map{
			"message": "Ukuran gambar maksimal 5MB",
		})
	}

	ext := strings.ToLower(filepath.Ext(file.Filename))

	allowed := map[string]bool{
		".jpg":  true,
		".jpeg": true,
		".png":  true,
		".webp": true,
	}

	if !allowed[ext] {
		return c.Status(400).JSON(fiber.Map{
			"message": "Format file tidak didukung. Gunakan jpg/png/webp",
		})
	}

	os.MkdirAll("./uploads/locations", 0755)

	filename := fmt.Sprintf(
		"loc_%d%s",
		time.Now().UnixNano(),
		ext,
	)

	savePath := filepath.Join(
		"./uploads/locations",
		filename,
	)

	if err := c.SaveFile(file, savePath); err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal menyimpan file",
		})
	}

	return c.JSON(fiber.Map{
		"filename": filename,
		"url":      "/uploads/locations/" + filename,
	})
}
