package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/auth"
	"toba-backend/handlers/location"
	"toba-backend/handlers/player"
	"toba-backend/handlers/trivia"
)

func RegisterPublicRoutes(
	api fiber.Router,
	authLimiter fiber.Handler,
) {
	api.Get("/", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"status":  "success",
			"message": "API TOBA v1 ready!",
		})
	})

	api.Post("/register", authLimiter, auth.Register)
	api.Post("/login", authLimiter, auth.Login)
	api.Post("/admin/login", authLimiter, auth.AdminLogin)

	api.Get("/version", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"latest_version": "1.0.0",
			"min_version":    "1.0.0",
			"force_update":   false,
			"update_message": "Update tersedia!",
			"store_url":      "https://play.google.com/store/apps/details?id=com.toba.budayabatak",
		})
	})

	api.Get(
		"/locations",
		location.GetAllLocations,
	)

	api.Get(
		"/locations/:id/detail",
		location.GetLocationDetail,
	)

	api.Get(
		"/locations/:id/fact",
		location.GetLocationFact,
	)

	api.Get(
		"/locations/:id/quiz",
		location.GetLocationQuiz,
	)

	api.Get(
		"/trivia/categories",
		trivia.GetTriviaCategories,
	)

	api.Get(
		"/trivia/:id/quiz",
		trivia.GetTriviaQuiz,
	)

	api.Get(
		"/leaderboard",
		player.GetLeaderboard,
	)
}
