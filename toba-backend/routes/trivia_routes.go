package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/trivia"
)

func RegisterTriviaRoutes(authGroup fiber.Router) {
	authGroup.Post("/trivia/submit", trivia.SubmitTrivia)
}
