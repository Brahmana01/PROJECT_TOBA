package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/trivia"
)

func RegisterTriviaRoutes(authGroup fiber.Router) {
	authGroup.Get("/trivia/:id/quiz", trivia.GetTriviaQuiz)
	authGroup.Post("/trivia/answer", trivia.SubmitTriviaAnswer)
	authGroup.Post("/trivia/submit", trivia.SubmitTrivia)
}
