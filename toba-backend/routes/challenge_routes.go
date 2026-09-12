package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/challenge"
)

func RegisterChallengeRoutes(authGroup fiber.Router) {
	authGroup.Get("/challenge/active", challenge.GetActiveChallenge)
	authGroup.Post("/challenge/submit", challenge.SubmitChallenge)
}
