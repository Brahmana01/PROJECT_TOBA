package admin

import (
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetTriviaCategories(c *fiber.Ctx) error {
	var cats []models.TriviaCategory

	config.DB.
		Where("deleted_at IS NULL").
		Order("display_order ASC").
		Find(&cats)

	return c.JSON(cats)
}

func AdminCreateTriviaCategory(c *fiber.Ctx) error {
	type Input struct {
		Name          string `json:"name"`
		Description   string `json:"description"`
		QuestionCount int    `json:"question_count"`
		DisplayOrder  int    `json:"display_order"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	cat := models.TriviaCategory{
		Name:            input.Name,
		Description:     input.Description,
		QuestionCount:   input.QuestionCount,
		MinimumQuestion: input.QuestionCount,
		DisplayOrder:    input.DisplayOrder,
		IsActive:        true,
	}

	config.DB.Create(&cat)

	return c.Status(201).JSON(cat)
}

func AdminUpdateTriviaCategory(c *fiber.Ctx) error {
	id := c.Params("id")

	var cat models.TriviaCategory

	if err := config.DB.First(&cat, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Kategori tidak ditemukan",
		})
	}

	type Input struct {
		Name          string `json:"name"`
		Description   string `json:"description"`
		QuestionCount int    `json:"question_count"`
		DisplayOrder  int    `json:"display_order"`
		IsActive      bool   `json:"is_active"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	cat.Name = input.Name
	cat.Description = input.Description
	cat.QuestionCount = input.QuestionCount
	cat.DisplayOrder = input.DisplayOrder
	cat.IsActive = input.IsActive

	config.DB.Save(&cat)

	return c.JSON(cat)
}

func AdminDeleteTriviaCategory(c *fiber.Ctx) error {
	id := c.Params("id")

	now := time.Now()

	config.DB.
		Model(&models.TriviaCategory{}).
		Where("id = ?", id).
		Update("deleted_at", now)

	return c.JSON(fiber.Map{
		"message": "Kategori berhasil dihapus",
	})
}

func AdminGetTriviaQuestions(c *fiber.Ctx) error {
	categoryID := c.Params("id")

	var questions []models.TriviaQuestion

	config.DB.
		Preload("Options").
		Where("category_id = ?", categoryID).
		Find(&questions)

	return c.JSON(questions)
}

func AdminCreateTriviaQuestion(c *fiber.Ctx) error {
	categoryID := c.Params("id")

	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}

	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		Options     []OptionInput `json:"options"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	catID := parseUint(categoryID)

	question := models.TriviaQuestion{
		CategoryID:  catID,
		Question:    input.Question,
		Explanation: input.Explanation,
		IsActive:    true,
	}

	config.DB.Create(&question)

	for _, opt := range input.Options {
		config.DB.Create(&models.TriviaQuestionOption{
			QuestionID: question.ID,
			OptionText: opt.OptionText,
			IsCorrect:  opt.IsCorrect,
		})
	}

	config.DB.
		Preload("Options").
		First(&question, question.ID)

	return c.Status(201).JSON(question)
}

func AdminUpdateTriviaQuestion(c *fiber.Ctx) error {
	id := c.Params("id")

	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}

	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		IsActive    bool          `json:"is_active"`
		Options     []OptionInput `json:"options"`
	}

	var input Input

	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	var question models.TriviaQuestion

	if err := config.DB.First(&question, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Soal tidak ditemukan",
		})
	}

	question.Question = input.Question
	question.Explanation = input.Explanation
	question.IsActive = input.IsActive

	config.DB.Save(&question)

	if len(input.Options) > 0 {
		config.DB.
			Where("question_id = ?", question.ID).
			Delete(&models.TriviaQuestionOption{})

		for _, opt := range input.Options {
			config.DB.Create(&models.TriviaQuestionOption{
				QuestionID: question.ID,
				OptionText: opt.OptionText,
				IsCorrect:  opt.IsCorrect,
			})
		}
	}

	config.DB.
		Preload("Options").
		First(&question, question.ID)

	return c.JSON(question)
}

func AdminDeleteTriviaQuestion(c *fiber.Ctx) error {
	id := c.Params("id")

	config.DB.
		Where("question_id = ?", id).
		Delete(&models.TriviaQuestionOption{})

	config.DB.Delete(&models.TriviaQuestion{}, id)

	return c.JSON(fiber.Map{
		"message": "Soal trivia berhasil dihapus",
	})
}
