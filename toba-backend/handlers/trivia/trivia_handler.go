package trivia

import (
	"toba-backend/services/trivia"

	"github.com/gofiber/fiber/v2"
)

func GetTriviaCategories(c *fiber.Ctx) error {
	categories, err := trivia.GetTriviaCategories()
	if err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil kategori trivia",
		})
	}

	return c.JSON(categories)
}
