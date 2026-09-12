package routes

import (
	"toba-backend/middleware"

	"github.com/gofiber/fiber/v2"
)

func RegisterRoutes(
	app *fiber.App,
	api fiber.Router,
	authLimiter fiber.Handler,
) {
	RegisterPublicRoutes(api, authLimiter)

	auth := api.Group("/", middleware.AuthMiddleware)
	adminAuth := api.Group("/admin", middleware.AdminAuthMiddleware)

	RegisterPlayerRoutes(auth)
	RegisterLocationRoutes(auth)
	RegisterTriviaRoutes(auth)
	RegisterBattleRoutes(auth)
	RegisterChallengeRoutes(auth)

	RegisterAdminRoutes(adminAuth)
	RegisterWSRoutes(app)
	RegisterStaticRoutes(app)
}
