package admin

import (
	"errors"
	"strconv"
	"strings"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

func AdminGetGalleryCards(c *fiber.Ctx) error {
	var cards []models.GalleryCard
	if err := config.DB.
		Order("location_id ASC").
		Order("id ASC").
		Find(&cards).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil kartu Gallery",
		})
	}
	if cards == nil {
		cards = []models.GalleryCard{}
	}
	return c.JSON(cards)
}

func AdminCreateGalleryCard(c *fiber.Ctx) error {
	var input struct {
		LocationID  uint   `json:"location_id"`
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input kartu Gallery tidak valid",
		})
	}

	input.Title = strings.TrimSpace(input.Title)
	input.Image = strings.TrimSpace(input.Image)
	input.Description = strings.TrimSpace(input.Description)
	if input.LocationID == 0 || input.Title == "" || input.Image == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Lokasi, judul, dan gambar wajib diisi",
		})
	}

	var card models.GalleryCard
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		var location models.Location
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("id = ? AND deleted_at IS NULL", input.LocationID).
			First(&location).Error; err != nil {
			return err
		}

		var count int64
		if err := tx.Model(&models.GalleryCard{}).
			Where("location_id = ?", input.LocationID).
			Count(&count).Error; err != nil {
			return err
		}
		if count >= int64(models.GalleryCardsPerLocation) {
			return errors.New("GALLERY_CARD_LIMIT_REACHED")
		}

		card = models.GalleryCard{
			LocationID:  input.LocationID,
			Title:       input.Title,
			Image:       input.Image,
			Description: input.Description,
		}
		return tx.Create(&card).Error
	})
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Lokasi tidak ditemukan",
			})
		}
		if strings.Contains(err.Error(), "GALLERY_CARD_LIMIT_REACHED") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": "Setiap lokasi hanya boleh memiliki tepat 2 kartu Gallery",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal membuat kartu Gallery",
		})
	}

	return c.Status(fiber.StatusCreated).JSON(card)
}

func AdminUpdateGalleryCard(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID kartu Gallery tidak valid",
		})
	}

	var input struct {
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input kartu Gallery tidak valid",
		})
	}

	input.Title = strings.TrimSpace(input.Title)
	input.Image = strings.TrimSpace(input.Image)
	input.Description = strings.TrimSpace(input.Description)
	if input.Title == "" || input.Image == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Judul dan gambar wajib diisi",
		})
	}

	var card models.GalleryCard
	if err := config.DB.First(&card, uint(id)).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Kartu Gallery tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil kartu Gallery",
		})
	}

	card.Title = input.Title
	card.Image = input.Image
	card.Description = input.Description
	if err := config.DB.Save(&card).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui kartu Gallery",
		})
	}
	return c.JSON(card)
}

func AdminDeleteGalleryCard(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID kartu Gallery tidak valid",
		})
	}

	err = config.DB.Transaction(func(tx *gorm.DB) error {
		var card models.GalleryCard
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&card, uint(id)).Error; err != nil {
			return err
		}

		var location models.Location
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&location, card.LocationID).Error; err != nil {
			return err
		}

		var count int64
		if err := tx.Model(&models.GalleryCard{}).
			Where("location_id = ?", card.LocationID).
			Count(&count).Error; err != nil {
			return err
		}
		if count <= int64(models.GalleryCardsPerLocation) {
			return errors.New("GALLERY_CARD_MINIMUM_REQUIRED")
		}

		var obtainedCount int64
		if err := tx.Model(&models.PlayerGalleryCard{}).
			Where("gallery_card_id = ?", card.ID).
			Count(&obtainedCount).Error; err != nil {
			return err
		}
		if obtainedCount > 0 {
			return errors.New("GALLERY_CARD_ALREADY_CLAIMED")
		}

		return tx.Delete(&card).Error
	})
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Kartu Gallery tidak ditemukan",
			})
		}
		if strings.Contains(err.Error(), "GALLERY_CARD_MINIMUM_REQUIRED") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": "Setiap lokasi wajib mempertahankan 2 kartu Gallery",
			})
		}
		if strings.Contains(err.Error(), "GALLERY_CARD_ALREADY_CLAIMED") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": "Kartu sudah dimiliki pemain dan tidak dapat dihapus",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal menghapus kartu Gallery",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Kartu Gallery berhasil dihapus",
	})
}
