package main

import (
	"fmt"
	"toba-backend/config"
	"toba-backend/controllers"
	"toba-backend/database"
	"toba-backend/websocket"

	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cors"
	"github.com/gofiber/fiber/v2/middleware/logger"
	fiberWS "github.com/gofiber/websocket/v2"
)

func main() {
	// ── Setup ──────────────────────────────────────────────
	config.ConnectDatabase()
	database.SeedAll()

	// ── Jalankan WebSocket Hub ─────────────────────────────
	go websocket.Hub.Run()

	// ── Fiber App ──────────────────────────────────────────
	app := fiber.New(fiber.Config{
		AppName:        "TOBA API v1.0",
		ReadBufferSize: 8192,
	})

	app.Use(logger.New())
	app.Use(cors.New(cors.Config{
		AllowOrigins: "*",
		AllowHeaders: "Origin, Content-Type, Accept",
	}))

	// ── WebSocket Middleware ───────────────────────────────
	app.Use("/ws", func(c *fiber.Ctx) error {
		if fiberWS.IsWebSocketUpgrade(c) {
			return c.Next()
		}
		return fiber.ErrUpgradeRequired
	})

	// ── WebSocket Routes ──────────────────────────────────
	app.Get("/ws/battle/:roomCode/:playerID/:playerName/:isHost",
		fiberWS.New(controllers.BattleWebSocket))

	// ── REST API ──────────────────────────────────────────
	api := app.Group("/api")

	// Health
	api.Get("/", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "success", "message": "API TOBA v1 ready!"})
	})

	// Auth
	api.Post("/register", controllers.Register)
	api.Post("/login", controllers.Login)

	// Profile
	api.Get("/profile/:id", controllers.GetProfile)

	// Locations
	api.Get("/locations", controllers.GetAllLocations)
	api.Get("/locations/:id/detail", controllers.GetLocationDetail)
	api.Get("/locations/:id/fact", controllers.GetLocationFact)
	api.Get("/locations/:id/quiz", controllers.GetLocationQuiz)
	api.Post("/locations/submit", controllers.SubmitQuiz)

	// Player
	api.Get("/player/:id/progress", controllers.GetPlayerProgress)
	api.Get("/player/:id/gallery", controllers.GetPlayerGallery)
	api.Get("/player/:id/achievements", controllers.GetPlayerAchievements)
	api.Post("/player/achievement/claim", controllers.ClaimAchievement)
	api.Get("/check-achievements/:id", controllers.CheckAndUnlockAchievements)

	// Trivia
	api.Get("/trivia/categories", controllers.GetTriviaCategories)
	api.Get("/trivia/:id/quiz", controllers.GetTriviaQuiz)
	api.Post("/trivia/submit", controllers.SubmitTrivia)

	// Battle (REST — untuk create & join room)
	api.Post("/battle/create", controllers.CreateRoom)
	api.Post("/battle/join", controllers.JoinRoom)
	api.Get("/battle/status/:code", controllers.GetRoomStatus)
	api.Post("/battle/finish", controllers.FinishBattle)
	api.Post("/battle/disconnect", controllers.DisconnectBattle)
	api.Post("/battle/answer", controllers.SubmitBattleAnswer)

	// Leaderboard
	api.Get("/leaderboard", controllers.GetLeaderboard)

	// ── Admin — Stats & Players ───────────────────────────────
	api.Get("/admin/stats", controllers.GetAdminStats)
	api.Get("/admin/players", controllers.GetAllPlayers)

	// ── Admin — Lokasi ────────────────────────────────────────
	api.Get("/admin/locations", controllers.AdminGetLocations)
	api.Post("/admin/locations", controllers.AdminCreateLocation)
	api.Put("/admin/locations/:id", controllers.AdminUpdateLocation)
	api.Delete("/admin/locations/:id", controllers.AdminDeleteLocation)
	api.Post("/admin/locations/upload-image", controllers.AdminUploadLocationImage)

	// ── Admin — Fakta Budaya ──────────────────────────────────
	api.Get("/admin/locations/:id/facts", controllers.AdminGetLocationFacts)
	api.Post("/admin/locations/:id/facts", controllers.AdminCreateLocationFact)
	api.Delete("/admin/facts/:id", controllers.AdminDeleteLocationFact)

	// ── Admin — Soal Jelajah ──────────────────────────────────
	api.Get("/admin/locations/:id/questions", controllers.AdminGetLocationQuestions)
	api.Post("/admin/locations/:id/questions", controllers.AdminCreateLocationQuestion)
	api.Put("/admin/questions/location/:id", controllers.AdminUpdateLocationQuestion)
	api.Delete("/admin/questions/location/:id", controllers.AdminDeleteLocationQuestion)

	// ── Admin — Kategori Trivia ───────────────────────────────
	api.Get("/admin/trivia/categories", controllers.AdminGetTriviaCategories)
	api.Post("/admin/trivia/categories", controllers.AdminCreateTriviaCategory)
	api.Put("/admin/trivia/categories/:id", controllers.AdminUpdateTriviaCategory)
	api.Delete("/admin/trivia/categories/:id", controllers.AdminDeleteTriviaCategory)

	// ── Admin — Soal Trivia ───────────────────────────────────
	api.Get("/admin/trivia/:id/questions", controllers.AdminGetTriviaQuestions)
	api.Post("/admin/trivia/:id/questions", controllers.AdminCreateTriviaQuestion)
	api.Put("/admin/trivia/questions/:id", controllers.AdminUpdateTriviaQuestion)
	api.Delete("/admin/trivia/questions/:id", controllers.AdminDeleteTriviaQuestion)

	// ── Admin — Soal Battle ───────────────────────────────────
	api.Get("/admin/battle/questions", controllers.AdminGetBattleQuestions)
	api.Post("/admin/battle/questions", controllers.AdminCreateBattleQuestion)
	api.Put("/admin/battle/questions/:id", controllers.AdminUpdateBattleQuestion)
	api.Delete("/admin/battle/questions/:id", controllers.AdminDeleteBattleQuestion)

	// ── Admin — Achievement ───────────────────────────────────
	api.Get("/admin/achievements", controllers.AdminGetAchievements)
	api.Post("/admin/achievements", controllers.AdminCreateAchievement)
	api.Put("/admin/achievements/:id", controllers.AdminUpdateAchievement)
	api.Delete("/admin/achievements/:id", controllers.AdminDeleteAchievement)

	// ── Admin — Game Settings ─────────────────────────────────
	api.Get("/admin/settings", controllers.AdminGetSettings)
	api.Put("/admin/settings/:key", controllers.AdminUpdateSetting)

	// ── Admin — Musik ─────────────────────────────────────────
	api.Get("/admin/music", controllers.AdminGetMusicList)
	api.Post("/admin/music/upload", controllers.AdminUploadMusic)
	api.Delete("/admin/music/:filename", controllers.AdminDeleteMusic)

	// ── Admin — Galeri Kartu ──────────────────────────────────
	api.Get("/admin/gallery", controllers.AdminGetGalleryCards)
	api.Post("/admin/gallery", controllers.AdminCreateGalleryCard)
	api.Put("/admin/gallery/:id", controllers.AdminUpdateGalleryCard)
	api.Delete("/admin/gallery/:id", controllers.AdminDeleteGalleryCard)

	// ── Static Files (gambar & musik) ────────────────────────
	app.Static("/uploads", "./uploads")

	// Root
	app.Get("/", func(c *fiber.Ctx) error {
		return c.SendString("TOBA Backend is Running!")
	})

	fmt.Println("\n========================================")
	fmt.Println("   TOBA SERVER AKTIF DI PORT 8080")
	fmt.Println("   WebSocket: ws://localhost:8080/ws/")
	fmt.Println("========================================\n")

	app.Listen("0.0.0.0:8080")
}
