package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/admin"
	"toba-backend/handlers/challenge"
)

func RegisterAdminRoutes(adminAuth fiber.Router) {
	adminAuth.Get("/challenges", challenge.AdminGetChallenges)
	adminAuth.Post("/challenges", challenge.AdminCreateChallenge)
	adminAuth.Put("/challenges/:id", challenge.AdminUpdateChallenge)
	adminAuth.Delete("/challenges/:id", challenge.AdminDeleteChallenge)

	adminAuth.Get("/stats", admin.GetAdminStats)
	adminAuth.Get("/players", admin.GetAllPlayers)

	adminAuth.Get("/locations", admin.AdminGetLocations)
	adminAuth.Post("/locations", admin.AdminCreateLocation)
	adminAuth.Put("/locations/:id", admin.AdminUpdateLocation)
	adminAuth.Delete("/locations/:id", admin.AdminDeleteLocation)
	adminAuth.Post("/locations/upload-image", admin.AdminUploadLocationImage)

	adminAuth.Get("/locations/:id/facts", admin.AdminGetLocationFacts)
	adminAuth.Post("/locations/:id/facts", admin.AdminCreateLocationFact)
	adminAuth.Delete("/facts/:id", admin.AdminDeleteLocationFact)

	adminAuth.Get("/locations/:id/questions", admin.AdminGetLocationQuestions)
	adminAuth.Post("/locations/:id/questions", admin.AdminCreateLocationQuestion)
	adminAuth.Put("/questions/location/:id", admin.AdminUpdateLocationQuestion)
	adminAuth.Delete("/questions/location/:id", admin.AdminDeleteLocationQuestion)

	adminAuth.Get("/trivia/categories", admin.AdminGetTriviaCategories)
	adminAuth.Post("/trivia/categories", admin.AdminCreateTriviaCategory)
	adminAuth.Put("/trivia/categories/:id", admin.AdminUpdateTriviaCategory)
	adminAuth.Delete("/trivia/categories/:id", admin.AdminDeleteTriviaCategory)

	adminAuth.Get("/trivia/:id/questions", admin.AdminGetTriviaQuestions)
	adminAuth.Post("/trivia/:id/questions", admin.AdminCreateTriviaQuestion)
	adminAuth.Put("/trivia/questions/:id", admin.AdminUpdateTriviaQuestion)
	adminAuth.Delete("/trivia/questions/:id", admin.AdminDeleteTriviaQuestion)

	adminAuth.Get("/battle/questions", admin.AdminGetBattleQuestions)
	adminAuth.Post("/battle/questions", admin.AdminCreateBattleQuestion)
	adminAuth.Put("/battle/questions/:id", admin.AdminUpdateBattleQuestion)
	adminAuth.Delete("/battle/questions/:id", admin.AdminDeleteBattleQuestion)

	adminAuth.Get("/achievements", admin.AdminGetAchievements)
	adminAuth.Post("/achievements", admin.AdminCreateAchievement)
	adminAuth.Put("/achievements/:id", admin.AdminUpdateAchievement)
	adminAuth.Delete("/achievements/:id", admin.AdminDeleteAchievement)

	adminAuth.Get("/settings", admin.AdminGetSettings)
	adminAuth.Put("/settings/:key", admin.AdminUpdateSetting)

	adminAuth.Get("/music", admin.AdminGetMusicList)
	adminAuth.Post("/music/upload", admin.AdminUploadMusic)
	adminAuth.Delete("/music/:filename", admin.AdminDeleteMusic)

	adminAuth.Get("/gallery", admin.AdminGetGalleryCards)
	adminAuth.Post("/gallery", admin.AdminCreateGalleryCard)
	adminAuth.Put("/gallery/:id", admin.AdminUpdateGalleryCard)
	adminAuth.Delete("/gallery/:id", admin.AdminDeleteGalleryCard)
}
