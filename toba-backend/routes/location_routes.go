package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/location"
)

func RegisterLocationRoutes(authGroup fiber.Router) {
	authGroup.Post("/locations/submit", location.SubmitQuiz)
}
