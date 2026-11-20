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

type adminTriviaOptionInput struct {
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

func validateTriviaOptions(options []adminTriviaOptionInput) error {
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

func AdminGetTriviaCategories(c *fiber.Ctx) error {
	var categories []models.TriviaCategory
	if err := config.DB.
		Where("deleted_at IS NULL").
		Order("display_order ASC").
		Find(&categories).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil kategori Trivia",
		})
	}

	for i := range categories {
		// Sepuluh adalah jumlah soal yang dimainkan per sesi,
		// bukan batas jumlah soal di bank kategori.
		categories[i].QuestionCount = models.TriviaQuestionCount
		categories[i].MinimumQuestion = models.TriviaQuestionCount
	}

	if categories == nil {
		categories = []models.TriviaCategory{}
	}
	return c.JSON(categories)
}

func AdminCreateTriviaCategory(c *fiber.Ctx) error {
	return c.Status(fiber.StatusConflict).JSON(fiber.Map{
		"message": "Lima kategori Trivia merupakan struktur tetap dan tidak dapat ditambah",
	})
}

func AdminUpdateTriviaCategory(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID kategori tidak valid",
		})
	}

	var input struct {
		Name         string `json:"name"`
		Description  string `json:"description"`
		DisplayOrder int    `json:"display_order"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input kategori tidak valid",
		})
	}

	input.Name = strings.TrimSpace(input.Name)
	input.Description = strings.TrimSpace(input.Description)
	if input.Name == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Nama kategori wajib diisi",
		})
	}

	var category models.TriviaCategory
	if err := config.DB.
		Where("id = ? AND deleted_at IS NULL", uint(id)).
		First(&category).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Kategori Trivia tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mencari kategori Trivia",
		})
	}

	category.Name = input.Name
	category.Description = input.Description
	category.DisplayOrder = input.DisplayOrder
	category.QuestionCount = models.TriviaQuestionCount
	category.MinimumQuestion = models.TriviaQuestionCount
	// IsActive dipertahankan; form edit kategori tidak mengubah statusnya.

	if err := config.DB.Save(&category).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui kategori Trivia",
		})
	}

	return c.JSON(category)
}

func AdminDeleteTriviaCategory(c *fiber.Ctx) error {
	return c.Status(fiber.StatusConflict).JSON(fiber.Map{
		"message": "Kategori Trivia merupakan struktur tetap dan tidak dapat dihapus",
	})
}

func AdminGetTriviaQuestions(c *fiber.Ctx) error {
	categoryID, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || categoryID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID kategori tidak valid",
		})
	}

	var category models.TriviaCategory
	if err := config.DB.
		Where("id = ? AND deleted_at IS NULL", uint(categoryID)).
		First(&category).Error; err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
				"message": "Kategori Trivia tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memeriksa kategori Trivia",
		})
	}

	var questions []models.TriviaQuestion
	if err := config.DB.
		Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).
		Where("category_id = ?", uint(categoryID)).
		Order("id ASC").
		Find(&questions).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil bank soal Trivia",
		})
	}

	if questions == nil {
		questions = []models.TriviaQuestion{}
	}
	return c.JSON(questions)
}

func AdminCreateTriviaQuestion(c *fiber.Ctx) error {
	categoryID, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || categoryID == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID kategori tidak valid",
		})
	}

	var input struct {
		Question    string                   `json:"question"`
		Explanation string                   `json:"explanation"`
		Options     []adminTriviaOptionInput `json:"options"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input soal Trivia tidak valid",
		})
	}

	input.Question = strings.TrimSpace(input.Question)
	input.Explanation = strings.TrimSpace(input.Explanation)
	if input.Question == "" || input.Explanation == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Pertanyaan dan penjelasan wajib diisi",
		})
	}
	if err := validateTriviaOptions(input.Options); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	var question models.TriviaQuestion
	err = config.DB.Transaction(func(tx *gorm.DB) error {
		var category models.TriviaCategory
		if err := tx.
			Clauses(clause.Locking{Strength: "UPDATE"}).
			Where("id = ? AND deleted_at IS NULL", uint(categoryID)).
			First(&category).Error; err != nil {
			return err
		}

		question = models.TriviaQuestion{
			CategoryID:  uint(categoryID),
			Question:    input.Question,
			Explanation: input.Explanation,
			IsActive:    true,
		}
		if err := tx.Create(&question).Error; err != nil {
			return err
		}

		for _, option := range input.Options {
			newOption := models.TriviaQuestionOption{
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
				"message": "Kategori Trivia tidak ditemukan",
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal membuat soal Trivia",
		})
	}

	if err := config.DB.Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Soal tersimpan tetapi gagal mengambil detailnya",
		})
	}

	return c.Status(fiber.StatusCreated).JSON(question)
}

func AdminUpdateTriviaQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID soal tidak valid",
		})
	}

	var input struct {
		Question    string                   `json:"question"`
		Explanation string                   `json:"explanation"`
		IsActive    bool                     `json:"is_active"`
		Options     []adminTriviaOptionInput `json:"options"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input soal Trivia tidak valid",
		})
	}

	input.Question = strings.TrimSpace(input.Question)
	input.Explanation = strings.TrimSpace(input.Explanation)
	if input.Question == "" || input.Explanation == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Pertanyaan dan penjelasan wajib diisi",
		})
	}
	if err := validateTriviaOptions(input.Options); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": err.Error(),
		})
	}

	var question models.TriviaQuestion
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

		var existingOptions []models.TriviaQuestionOption
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
				"message": "Soal Trivia tidak ditemukan",
			})
		}
		if strings.Contains(err.Error(), "data opsi lama tidak valid") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{
				"message": err.Error(),
			})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal memperbarui soal Trivia",
		})
	}

	if err := config.DB.Preload("Options", func(db *gorm.DB) *gorm.DB { return db.Order("id ASC") }).First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Soal diperbarui tetapi gagal mengambil detailnya",
		})
	}

	return c.JSON(question)
}

func AdminDeleteTriviaQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "ID soal tidak valid",
		})
	}

	result := config.DB.Model(&models.TriviaQuestion{}).
		Where("id = ?", uint(id)).
		Update("is_active", false)
	if result.Error != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal menonaktifkan soal Trivia",
		})
	}
	if result.RowsAffected == 0 {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{
			"message": "Soal Trivia tidak ditemukan",
		})
	}

	return c.JSON(fiber.Map{
		"message": "Soal Trivia berhasil dinonaktifkan",
	})
}
