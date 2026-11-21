package main

import (
	"fmt"
	"os"
	"strings"
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

	// Seed tidak dijalankan otomatis setiap server start. Seed terarah dapat
	// dipanggil satu kali dengan: ./toba-backend seed gameplay
	// Mode: gameplay (aturan + Trivia + Battle), battle (Battle + settings), all.
	if len(os.Args) > 1 && strings.EqualFold(strings.TrimSpace(os.Args[1]), "seed") {
		mode := "gameplay"
		if len(os.Args) > 2 && strings.TrimSpace(os.Args[2]) != "" {
			mode = strings.ToLower(strings.TrimSpace(os.Args[2]))
		}
		switch mode {
		case "gameplay":
			if err := database.SeedGameplayRules(); err != nil {
				fmt.Println("Seed gameplay gagal:", err)
				os.Exit(1)
			}
		case "battle":
			if err := database.SeedBattleRules(); err != nil {
				fmt.Println("Seed Battle gagal:", err)
				os.Exit(1)
			}
		case "all":
			database.SeedAll()
		default:
			fmt.Println("Mode seed tidak dikenal. Gunakan gameplay, battle, atau all.")
			os.Exit(2)
		}
		fmt.Println("Seed selesai. Proses berhenti tanpa menjalankan HTTP server.")
		return
	}

	// Kompatibilitas untuk deployment lama. Nilai true berarti all; nilai
	// gameplay/battle menjalankan seed terarah. Env harus dilepas setelah satu kali.
	seedMode := strings.ToLower(strings.TrimSpace(os.Getenv("TOBA_RUN_SEED")))
	switch seedMode {
	case "true", "all":
		database.SeedAll()
	case "gameplay":
		if err := database.SeedGameplayRules(); err != nil {
			fmt.Println("Seed gameplay gagal:", err)
			os.Exit(1)
		}
	case "battle":
		if err := database.SeedBattleRules(); err != nil {
			fmt.Println("Seed Battle gagal:", err)
			os.Exit(1)
		}
	default:
		fmt.Println("ℹ️ Seed otomatis dilewati. Gunakan perintah 'seed gameplay' untuk menjalankan seed terarah.")
	}

	// ============================================================
	// WEBSOCKET HUB
	// ============================================================
	go websocket.Hub.Run()

	app := fiber.New(fiber.Config{
		AppName:        "TOBA API v1.0",
		ReadBufferSize: 8192,
	})

	app.Use(logger.New())

	corsOrigins := strings.TrimSpace(os.Getenv("CORS_ALLOW_ORIGINS"))
	if corsOrigins == "" {
		corsOrigins = "http://localhost:5173,http://127.0.0.1:5173"
	}
	app.Use(cors.New(cors.Config{
		AllowOrigins: corsOrigins,
		AllowHeaders: "Origin, Content-Type, Accept, Authorization",
		AllowMethods: "GET, POST, PUT, DELETE, OPTIONS",
	}))

	api := app.Group("/api")

	api.Use(limiter.New(limiter.Config{
		Max:        60,
		Expiration: 1 * time.Minute,
		LimitReached: func(c *fiber.Ctx) error {
			return c.Status(fiber.StatusTooManyRequests).JSON(
				fiber.Map{"message": "Terlalu banyak permintaan. Coba lagi sebentar."},
			)
		},
	}))

	authLimiter := limiter.New(limiter.Config{
		Max:        5,
		Expiration: 1 * time.Minute,
		LimitReached: func(c *fiber.Ctx) error {
			return c.Status(fiber.StatusTooManyRequests).JSON(
				fiber.Map{"message": "Terlalu banyak percobaan login. Tunggu 1 menit."},
			)
		},
	})

	routes.RegisterRoutes(app, api, authLimiter)

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

	if err := app.Listen("0.0.0.0:8080"); err != nil {
		fmt.Println("Server gagal dijalankan:", err)
	}
}
