package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetBattleQuestions(c *fiber.Ctx) error {
	var questions []models.BattleQuestion
	config.DB.Preload("Options").Find(&questions)
	return c.JSON(questions)
}

func AdminCreateBattleQuestion(c *fiber.Ctx) error {
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
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	question := models.BattleQuestion{
		Question:    input.Question,
		Explanation: input.Explanation,
		IsActive:    true,
	}

	config.DB.Create(&question)

	for _, opt := range input.Options {
		config.DB.Create(&models.BattleQuestionOption{
			QuestionID: question.ID,
			OptionText: opt.OptionText,
			IsCorrect:  opt.IsCorrect,
		})
	}

	config.DB.Preload("Options").First(&question, question.ID)

	return c.Status(201).JSON(question)
}

func AdminUpdateBattleQuestion(c *fiber.Ctx) error {
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
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var question models.BattleQuestion

	if err := config.DB.First(&question, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Soal tidak ditemukan"})
	}

	question.Question = input.Question
	question.Explanation = input.Explanation
	question.IsActive = input.IsActive

	config.DB.Save(&question)

	if len(input.Options) > 0 {
		config.DB.Where("question_id = ?", question.ID).Delete(&models.BattleQuestionOption{})

		for _, opt := range input.Options {
			config.DB.Create(&models.BattleQuestionOption{
				QuestionID: question.ID,
				OptionText: opt.OptionText,
				IsCorrect:  opt.IsCorrect,
			})
		}
	}

	config.DB.Preload("Options").First(&question, question.ID)

	return c.JSON(question)
}

func AdminDeleteBattleQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Where("question_id = ?", id).Delete(&models.BattleQuestionOption{})
	config.DB.Delete(&models.BattleQuestion{}, id)
	return c.JSON(fiber.Map{"message": "Soal battle berhasil dihapus"})
}