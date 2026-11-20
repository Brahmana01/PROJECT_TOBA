package admin

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
)

func AdminGetLocations(c *fiber.Ctx) error {
	var locations []models.Location
	if err := config.DB.
		Where("deleted_at IS NULL").
		Order("level_order ASC").
		Find(&locations).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil daftar lokasi",
		})
	}

	for i := range locations {
		// question_count berarti jumlah soal per sesi, bukan jumlah bank soal.
		locations[i].QuestionCount = models.LocationQuestionCount
		locations[i].MinimumCorrect = models.LocationMinimumCorrect
	}

	if locations == nil {
		locations = []models.Location{}
	}
	return c.JSON(locations)
}

func AdminCreateLocation(c *fiber.Ctx) error {
	var input struct {
		Name        string `json:"name"`
		Description string `json:"description"`
		Image       string `json:"image"`
		RewardXP    int    `json:"reward_xp"`
		RewardStars int    `json:"reward_stars"`
		LevelOrder  int    `json:"level_order"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input lokasi tidak valid",
		})
	}

	input.Name = strings.TrimSpace(input.Name)
	input.Description = strings.TrimSpace(input.Description)
	input.Image = strings.TrimSpace(input.Image)
	if input.Name == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Nama lokasi wajib diisi",
		})
	}
	if input.RewardXP < 0 || input.RewardStars < 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Reward XP dan bintang tidak boleh negatif",
		})
	}

	var location models.Location
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		// Project menggunakan PostgreSQL. Lock tabel hanya selama penentuan
		// level_order dan insert agar dua request create tidak memperoleh
		// urutan otomatis yang sama.
		if err := tx.Exec("LOCK TABLE locations IN EXCLUSIVE MODE").Error; err != nil {
			return err
		}

		levelOrder := input.LevelOrder
		if levelOrder <= 0 {
			if err := tx.Model(&models.Location{}).
				Select("COALESCE(MAX(level_order), 0)").
				Scan(&levelOrder).Error; err != nil {
				return err
			}
			levelOrder++
		}

		var duplicateCount int64
		if err := tx.Model(&models.Location{}).
			Where("level_order = ?", levelOrder).
			Count(&duplicateCount).Error; err != nil {
			return err
		}
		if duplicateCount > 0 {
			return errors.New("LEVEL_ORDER_ALREADY_USED")
		}

		location = models.Location{
			Name:           input.Name,
			Description:    input.Description,
			Image:          input.Image,
			RewardXP:       input.RewardXP,
			RewardStars:    input.RewardStars,
			QuestionCount:  models.LocationQuestionCount,
			MinimumCorrect: models.LocationMinimumCorrect,
			LevelOrder:     levelOrder,
			IsActive:       true,
		}

		return tx.Create(&location).Error
	})
	if err != nil {
		if strings.Contains(err.Error(), "LEVEL_ORDER_ALREADY_USED") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": "Urutan level sudah digunakan lokasi lain",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal membuat lokasi baru",
		})
	}

	return c.Status(fiber.StatusCreated).JSON(location)
}

func AdminUpdateLocation(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID lokasi tidak valid",
		})
	}

	var input struct {
		Name        string `json:"name"`
		Description string `json:"description"`
		Image       string `json:"image"`
		RewardXP    int    `json:"reward_xp"`
		RewardStars int    `json:"reward_stars"`
		LevelOrder  int    `json:"level_order"`
		IsActive    bool   `json:"is_active"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input lokasi tidak valid",
		})
	}

	input.Name = strings.TrimSpace(input.Name)
	input.Description = strings.TrimSpace(input.Description)
	input.Image = strings.TrimSpace(input.Image)
	if input.Name == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Nama lokasi wajib diisi",
		})
	}
	if input.RewardXP < 0 || input.RewardStars < 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Reward XP dan bintang tidak boleh negatif",
		})
	}

	var location models.Location
	err = config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.First(&location, uint(id)).Error; err != nil {
			return err
		}
		if location.DeletedAt != nil {
			return gorm.ErrRecordNotFound
		}

		levelOrder := input.LevelOrder
		if levelOrder <= 0 {
			levelOrder = location.LevelOrder
		}

		var duplicateCount int64
		if err := tx.Model(&models.Location{}).
			Where("level_order = ? AND id <> ?", levelOrder, uint(id)).
			Count(&duplicateCount).Error; err != nil {
			return err
		}
		if duplicateCount > 0 {
			return errors.New("LEVEL_ORDER_ALREADY_USED")
		}

		location.Name = input.Name
		location.Description = input.Description
		location.Image = input.Image
		location.RewardXP = input.RewardXP
		location.RewardStars = input.RewardStars
		location.LevelOrder = levelOrder
		location.QuestionCount = models.LocationQuestionCount
		location.MinimumCorrect = models.LocationMinimumCorrect
		location.IsActive = input.IsActive

		return tx.Save(&location).Error
	})
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Lokasi tidak ditemukan",
			})
		}
		if strings.Contains(err.Error(), "LEVEL_ORDER_ALREADY_USED") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": "Urutan level sudah digunakan lokasi lain",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui lokasi",
		})
	}

	return c.JSON(location)
}

func AdminDeleteLocation(c *fiber.Ctx) error {
	return c.Status(fiber.StatusConflict).JSON(fiber.Map{
		"message": "Lokasi tidak dihapus permanen agar progres pemain tetap aman. Nonaktifkan lokasi melalui Edit Lokasi.",
	})
}

func AdminUploadLocationImage(c *fiber.Ctx) error {
	file, err := c.FormFile("image")
	if err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "File gambar tidak ditemukan",
		})
	}

	const maxImageSize = 5 * 1024 * 1024
	if file.Size > maxImageSize {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
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
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Format gambar tidak didukung. Gunakan JPG, PNG, atau WEBP",
		})
	}

	if err := os.MkdirAll("./uploads/locations", 0755); err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal menyiapkan folder upload",
		})
	}

	filename := fmt.Sprintf("loc_%d%s", time.Now().UnixNano(), ext)
	savePath := filepath.Join("./uploads/locations", filename)
	if err := c.SaveFile(file, savePath); err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal menyimpan gambar",
		})
	}

	return c.JSON(fiber.Map{
		"filename": filename,
		"url":      "/uploads/locations/" + filename,
	})
}
