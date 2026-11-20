package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/location"
)

func RegisterLocationRoutes(authGroup fiber.Router) {
	authGroup.Get("/locations/:id/quiz", location.GetLocationQuiz)
	authGroup.Post("/locations/answer", location.SubmitLocationAnswer)
	authGroup.Post("/locations/submit", location.SubmitQuiz)
}
