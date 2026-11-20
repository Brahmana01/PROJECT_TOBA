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

type battleOptionInput struct {
	OptionText string `json:"option_text"`
	IsCorrect  bool   `json:"is_correct"`
}

type battleQuestionInput struct {
	Question    string              `json:"question"`
	Explanation string              `json:"explanation"`
	IsActive    *bool               `json:"is_active"`
	Options     []battleOptionInput `json:"options"`
}

func validateBattleQuestionInput(input *battleQuestionInput) error {
	input.Question = strings.TrimSpace(input.Question)
	input.Explanation = strings.TrimSpace(input.Explanation)
	if input.Question == "" || input.Explanation == "" {
		return errors.New("pertanyaan dan penjelasan wajib diisi")
	}
	if len(input.Options) != 4 {
		return errors.New("setiap soal Battle wajib memiliki tepat 4 opsi")
	}
	correctCount := 0
	for i := range input.Options {
		input.Options[i].OptionText = strings.TrimSpace(input.Options[i].OptionText)
		if input.Options[i].OptionText == "" {
			return errors.New("seluruh teks opsi wajib diisi")
		}
		if input.Options[i].IsCorrect {
			correctCount++
		}
	}
	if correctCount != 1 {
		return errors.New("setiap soal Battle wajib memiliki tepat 1 jawaban benar")
	}
	return nil
}

func AdminGetBattleQuestions(c *fiber.Ctx) error {
	var questions []models.BattleQuestion
	if err := config.DB.Preload("Options").Order("id ASC").Find(&questions).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{
			"message": "Gagal mengambil bank soal Battle",
		})
	}
	if questions == nil {
		questions = []models.BattleQuestion{}
	}
	return c.JSON(questions)
}

func AdminCreateBattleQuestion(c *fiber.Ctx) error {
	var input battleQuestionInput
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if err := validateBattleQuestionInput(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": err.Error()})
	}

	question := models.BattleQuestion{
		Question: input.Question, Explanation: input.Explanation, IsActive: true,
	}
	if err := config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.Create(&question).Error; err != nil {
			return err
		}
		for _, option := range input.Options {
			row := models.BattleQuestionOption{
				QuestionID: question.ID, OptionText: option.OptionText, IsCorrect: option.IsCorrect,
			}
			if err := tx.Create(&row).Error; err != nil {
				return err
			}
		}
		return nil
	}); err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal membuat soal Battle"})
	}

	if err := config.DB.Preload("Options").First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Soal dibuat tetapi gagal mengambil hasil"})
	}
	return c.Status(fiber.StatusCreated).JSON(question)
}

func AdminUpdateBattleQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "ID soal tidak valid"})
	}

	var input battleQuestionInput
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if err := validateBattleQuestionInput(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": err.Error()})
	}

	var question models.BattleQuestion
	if err := config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&question, uint(id)).Error; err != nil {
			return err
		}
		question.Question = input.Question
		question.Explanation = input.Explanation
		if input.IsActive != nil {
			question.IsActive = *input.IsActive
		}
		if err := tx.Save(&question).Error; err != nil {
			return err
		}

		var existing []models.BattleQuestionOption
		if err := tx.Where("question_id = ?", question.ID).Order("id ASC").Find(&existing).Error; err != nil {
			return err
		}
		if len(existing) != 4 {
			return errors.New("data opsi lama tidak valid; soal harus memiliki tepat 4 opsi")
		}
		for i := range existing {
			existing[i].OptionText = input.Options[i].OptionText
			existing[i].IsCorrect = input.Options[i].IsCorrect
			if err := tx.Save(&existing[i]).Error; err != nil {
				return err
			}
		}
		return nil
	}); err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{"message": "Soal Battle tidak ditemukan"})
		}
		if strings.Contains(err.Error(), "data opsi lama tidak valid") {
			return c.Status(fiber.StatusConflict).JSON(fiber.Map{"message": err.Error()})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal memperbarui soal Battle"})
	}

	if err := config.DB.Preload("Options").First(&question, question.ID).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Soal diperbarui tetapi gagal mengambil hasil"})
	}
	return c.JSON(question)
}

// Delete hanya menonaktifkan soal agar histori Battle dan jawaban lama tetap utuh.
func AdminDeleteBattleQuestion(c *fiber.Ctx) error {
	id, err := strconv.ParseUint(c.Params("id"), 10, 64)
	if err != nil || id == 0 {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "ID soal tidak valid"})
	}
	result := config.DB.Model(&models.BattleQuestion{}).
		Where("id = ?", uint(id)).Update("is_active", false)
	if result.Error != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal menonaktifkan soal Battle"})
	}
	if result.RowsAffected == 0 {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{"message": "Soal Battle tidak ditemukan"})
	}
	return c.JSON(fiber.Map{"message": "Soal Battle berhasil dinonaktifkan"})
}
