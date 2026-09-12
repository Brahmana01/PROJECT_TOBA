package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/auth"
	"toba-backend/handlers/location"
	"toba-backend/handlers/player"
	"toba-backend/handlers/social"
)

func RegisterPlayerRoutes(authGroup fiber.Router) {
	authGroup.Post("/logout", auth.Logout)

	authGroup.Get("/profile/:id", player.GetProfile)

	authGroup.Get(
		"/player/:id/progress",
		location.GetPlayerProgress,
	)

	authGroup.Get(
		"/player/:id/gallery",
		social.GetPlayerGallery,
	)

	authGroup.Get(
		"/player/:id/achievements",
		social.GetPlayerAchievements,
	)

	authGroup.Post(
		"/player/fcm-token",
		player.UpdateFCMToken,
	)

	authGroup.Post(
		"/player/achievement/claim",
		social.ClaimAchievement,
	)

	authGroup.Get(
		"/check-achievements/:id",
		social.CheckAndUnlockAchievements,
	)
}
