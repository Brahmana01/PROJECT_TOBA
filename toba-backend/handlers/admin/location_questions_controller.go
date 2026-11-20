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

type adminLocationOptionInput struct {
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

func validateFourOptions(options []adminLocationOptionInput) error {
	if len(options) != 4 {
		return errors.New("setiap soal wajib memiliki tepat 4 pilihan jawaban")
	}

	correctCount := 0
	for _, option := range options {
		if strings.TrimSpace(option.OptionText) == "" {
			return errors.New("teks pilihan jawaban tidak boleh kosong")
		}
		if option.IsCorrect {
			correctCount++
		}
	}

	if correctCount != 1 {
		return errors.New("setiap soal wajib memiliki tepat 1 jawaban benar")
	}

	return nil
}

func AdminGetLocationQuestions(c *fiber.Ctx) error {
	locationID, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || locationID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID lokasi tidak valid",
		})
	}

	var location models.Location
	if err := config.DB.
		Where("id = ? AND deleted_at IS NULL", uint(locationID)).
		First(&location).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Lokasi tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memeriksa lokasi",
		})
	}

	var questions []models.LocationQuestion
	if err := config.DB.
		Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).
		Where("location_id = ?", uint(locationID)).
		Order("id ASC").
		Find(&questions).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil bank soal lokasi",
		})
	}

	if questions == nil {
		questions = []models.LocationQuestion{}
	}

	return c.JSON(questions)
}

func AdminCreateLocationQuestion(c *fiber.Ctx) error {
	locationID, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || locationID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID lokasi tidak valid",
		})
	}

	var input struct {
		Question    string                     `json:"question"`
		Explanation string                     `json:"explanation"`
		Options     []adminLocationOptionInput `json:"options"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input soal tidak valid",
		})
	}

	input.Question = strings.TrimSpace(input.Question)
	input.Explanation = strings.TrimSpace(input.Explanation)
	if input.Question == "" || input.Explanation == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Pertanyaan dan penjelasan wajib diisi",
		})
	}
	if err := validateFourOptions(input.Options); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	var question models.LocationQuestion
	err = config.DB.Transaction(func(tx *gorm.DB) error {
		// Kunci parent agar lokasi tidak terhapus/berubah selama soal dibuat.
		var location models.Location
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("id = ? AND deleted_at IS NULL", uint(locationID)).
			First(&location).Error; err != nil {
			return err
		}

		question = models.LocationQuestion{
			LocationID:  uint(locationID),
			Question:    input.Question,
			Explanation: input.Explanation,
			IsActive:    true,
		}
		if err := tx.Create(&question).Error; err != nil {
			return err
		}

		for _, option := range input.Options {
			newOption := models.LocationQuestionOption{
				QuestionID: question.ID,
				OptionText: strings.TrimSpace(option.OptionText),
				IsCorrect:  option.IsCorrect,
			}
			if err := tx.Create(&newOption).Error; err != nil {
				return err
			}
		}

		return nil
	})
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Lokasi tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal membuat soal Jelajah",
		})
	}

	if err := config.DB.Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Soal tersimpan tetapi gagal mengambil detailnya",
		})
	}

	return c.Status(fiber.StatusCreated).JSON(question)
}

func AdminUpdateLocationQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID soal tidak valid",
		})
	}

	var input struct {
		Question    string                     `json:"question"`
		Explanation string                     `json:"explanation"`
		IsActive    bool                       `json:"is_active"`
		Options     []adminLocationOptionInput `json:"options"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input soal tidak valid",
		})
	}

	input.Question = strings.TrimSpace(input.Question)
	input.Explanation = strings.TrimSpace(input.Explanation)
	if input.Question == "" || input.Explanation == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Pertanyaan dan penjelasan wajib diisi",
		})
	}
	if err := validateFourOptions(input.Options); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	var question models.LocationQuestion
	err = config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			First(&question, uint(id)).Error; err != nil {
			return err
		}

		question.Question = input.Question
		question.Explanation = input.Explanation
		question.IsActive = input.IsActive
		if err := tx.Save(&question).Error; err != nil {
			return err
		}

		// Perbarui empat opsi yang sudah ada tanpa mengganti ID opsi.
		var existingOptions []models.LocationQuestionOption
		if err := tx.
			Where("question_id = ?", question.ID).
			Order("id ASC").
			Find(&existingOptions).Error; err != nil {
			return err
		}
		if len(existingOptions) != 4 {
			return errors.New("data opsi lama tidak valid; soal harus memiliki tepat 4 opsi")
		}

		for i := range existingOptions {
			existingOptions[i].OptionText = strings.TrimSpace(input.Options[i].OptionText)
			existingOptions[i].IsCorrect = input.Options[i].IsCorrect
			if err := tx.Save(&existingOptions[i]).Error; err != nil {
				return err
			}
		}

		return nil
	})
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Soal Jelajah tidak ditemukan",
			})
		}
		if strings.Contains(err.Error(), "data opsi lama tidak valid") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": err.Error(),
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui soal Jelajah",
		})
	}

	if err := config.DB.Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Soal diperbarui tetapi gagal mengambil detailnya",
		})
	}

	return c.JSON(question)
}

func AdminDeleteLocationQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID soal tidak valid",
		})
	}

	result := config.DB.Model(&models.LocationQuestion{}).
		Where("id = ?", uint(id)).
		Update("is_active", false)
	if result.Error != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal menonaktifkan soal Jelajah",
		})
	}
	if result.RowsAffected == 0 {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
			"message": "Soal Jelajah tidak ditemukan",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Soal Jelajah berhasil dinonaktifkan",
	})
}
