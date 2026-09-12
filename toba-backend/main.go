package main

import (
	"fmt"
	"time"

	"toba-backend/config"
	"toba-backend/database"
	"toba-backend/routes"
	"toba-backend/websocket"

	"github.com/gofiber/fiber/v2"
	"github.com/gofiber/fiber/v2/middleware/cors"
	"github.com/gofiber/fiber/v2/middleware/limiter"
	"github.com/gofiber/fiber/v2/middleware/logger"
)

func main() {
	// ============================================================
	// DATABASE
	// ============================================================
	config.ConnectDatabase()
	database.SeedAll()

	// ============================================================
	// WEBSOCKET HUB
	// ============================================================
	go websocket.Hub.Run()

	// ============================================================
	// FIBER APP
	// ============================================================
	app := fiber.New(fiber.Config{
		AppName:        "TOBA API v1.0",
		ReadBufferSize: 8192,
	})

	// ============================================================
	// LOGGER
	// ============================================================
	app.Use(logger.New())

	// ============================================================
	// CORS
	// ============================================================
	app.Use(cors.New(cors.Config{
		AllowOrigins: "http://localhost:5173, http://127.0.0.1:5173",
		AllowHeaders: "Origin, Content-Type, Accept, Authorization",
		AllowMethods: "GET, POST, PUT, DELETE, OPTIONS",
	}))

	// ============================================================
	// REST API
	// ============================================================
	api := app.Group("/api")

	// ============================================================
	// GLOBAL API RATE LIMITER
	// Maksimal 60 request per menit per IP.
	// ============================================================
	api.Use(limiter.New(limiter.Config{
		Max:        60,
		Expiration: 1 * time.Minute,
		LimitReached: func(c *fiber.Ctx) error {
			return c.Status(fiber.StatusTooManyRequests).JSON(
				fiber.Map{
					"message": "Terlalu banyak permintaan. Coba lagi sebentar.",
				},
			)
		},
	}))

	// ============================================================
	// AUTH RATE LIMITER
	// Login dan register lebih ketat.
	// ============================================================
	authLimiter := limiter.New(limiter.Config{
		Max:        5,
		Expiration: 1 * time.Minute,
		LimitReached: func(c *fiber.Ctx) error {
			return c.Status(fiber.StatusTooManyRequests).JSON(
				fiber.Map{
					"message": "Terlalu banyak percobaan login. Tunggu 1 menit.",
				},
			)
		},
	})

	// ============================================================
	// REGISTER SEMUA ROUTES
	// ============================================================
	routes.RegisterRoutes(
		app,
		api,
		authLimiter,
	)

	// ============================================================
	// SERVER INFO
	// ============================================================
	fmt.Println()
	fmt.Println("========================================")
	fmt.Println("   TOBA SERVER AKTIF DI PORT 8080")
	fmt.Println("   WebSocket: ws://localhost:8080/ws/")
	fmt.Println("   API Rate Limit: 60 request/menit/IP")
	fmt.Println("   Auth Rate Limit: 5 request/menit/IP")
	fmt.Println("   Player Protected Routes: AKTIF")
	fmt.Println("   Admin Protected Routes: AKTIF")
	fmt.Println("========================================")
	fmt.Println()

	// ============================================================
	// START SERVER
	// ============================================================
	if err := app.Listen("0.0.0.0:8080"); err != nil {
		fmt.Println("Server gagal dijalankan:", err)
	}
}
