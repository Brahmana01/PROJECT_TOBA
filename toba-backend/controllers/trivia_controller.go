package controllers

import (
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// GET /api/trivia/categories
// Ambil semua kategori trivia aktif
// ============================================================
func GetTriviaCategories(c *fiber.Ctx) error {
	var categories []models.TriviaCategory
	config.DB.
		Where("is_active = ? AND deleted_at IS NULL", true).
		Order("display_order ASC").
		Find(&categories)

	return c.JSON(categories)
}

// ============================================================
// GET /api/trivia/:id/quiz
// Ambil soal trivia acak berdasarkan kategori
// ============================================================
func GetTriviaQuiz(c *fiber.Ctx) error {
	categoryID := c.Params("id")

	var category models.TriviaCategory
	if err := config.DB.First(&category, categoryID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "Kategori tidak ditemukan",
		})
	}

	var questions []models.TriviaQuestion
	config.DB.
		Preload("Options").
		Where("category_id = ? AND is_active = ?", categoryID, true).
		Order("RANDOM()").
		Limit(category.QuestionCount).
		Find(&questions)

	if len(questions) == 0 {
		return c.Status(404).JSON(fiber.Map{
			"message": "Soal belum tersedia untuk kategori ini",
		})
	}

	return c.JSON(fiber.Map{
		"category":  category,
		"questions": questions,
	})
}

// ============================================================
// POST /api/trivia/submit
// Submit hasil trivia & update XP player
// ============================================================
func SubmitTrivia(c *fiber.Ctx) error {
	type SubmitInput struct {
		PlayerID   uint `json:"player_id"`
		CategoryID uint `json:"category_id"`
		Correct    int  `json:"correct_count"`
		Wrong      int  `json:"wrong_count"`
	}

	input := new(SubmitInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	// Ambil player
	var player models.Player
	if err := config.DB.First(&player, input.PlayerID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Player tidak ditemukan"})
	}

	// Hitung XP (10 XP per jawaban benar)
	xpGained := input.Correct * 10
	player.XP += xpGained

	// Level up check
	newLevel := (player.XP / 500) + 1
	if newLevel > player.Level {
		player.Level = newLevel
		updatePlayerTitle(&player)
	}
	config.DB.Save(&player)

	// Simpan riwayat trivia
	history := models.PlayerTriviaHistory{
		PlayerID:      input.PlayerID,
		CategoryID:    input.CategoryID,
		CorrectAnswer: input.Correct,
		WrongAnswer:   input.Wrong,
		XPEarned:      xpGained,
		PlayedAt:      time.Now(),
	}
	config.DB.Create(&history)

	return c.JSON(fiber.Map{
		"xp":          xpGained,
		"level":       player.Level,
		"total_xp":    player.XP,
		"total_stars": player.Stars,
	})
}
