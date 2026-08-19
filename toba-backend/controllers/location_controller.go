package controllers

import (
	"math/rand"
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// GET /api/locations
// Ambil semua lokasi aktif, urut berdasarkan level_order
// ============================================================
func GetAllLocations(c *fiber.Ctx) error {
	var locations []models.Location
	result := config.DB.
		Where("is_active = ? AND deleted_at IS NULL", true).
		Order("level_order ASC").
		Find(&locations)

	if result.Error != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal mengambil data lokasi",
		})
	}

	return c.JSON(locations)
}

// ============================================================
// GET /api/locations/:id/detail
// Ambil detail satu lokasi
// ============================================================
func GetLocationDetail(c *fiber.Ctx) error {
	locationID := c.Params("id")

	var location models.Location
	if err := config.DB.
		Where("id = ? AND is_active = ? AND deleted_at IS NULL", locationID, true).
		First(&location).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Lokasi tidak ditemukan"})
	}

	return c.JSON(location)
}

// ============================================================
// GET /api/locations/:id/fact
// Ambil satu fakta budaya secara acak dari lokasi
// ============================================================
func GetLocationFact(c *fiber.Ctx) error {
	locationID := c.Params("id")

	var facts []models.LocationFact
	config.DB.Where("location_id = ?", locationID).Find(&facts)

	if len(facts) == 0 {
		return c.Status(404).JSON(fiber.Map{
			"message": "Fakta tidak ditemukan untuk lokasi ini",
		})
	}

	// Pilih fakta secara acak
	rand.Seed(time.Now().UnixNano())
	randomFact := facts[rand.Intn(len(facts))]

	return c.JSON(fiber.Map{
		"fact": randomFact,
	})
}

// ============================================================
// GET /api/locations/:id/quiz
// Ambil soal kuis acak untuk lokasi tertentu
// ============================================================
func GetLocationQuiz(c *fiber.Ctx) error {
	locationID := c.Params("id")

	var location models.Location
	if err := config.DB.First(&location, locationID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Lokasi tidak ditemukan"})
	}

	var questions []models.LocationQuestion
	config.DB.
		Preload("Options").
		Where("location_id = ? AND is_active = ?", locationID, true).
		Order("RANDOM()").
		Limit(location.QuestionCount).
		Find(&questions)

	if len(questions) == 0 {
		return c.Status(404).JSON(fiber.Map{"message": "Soal belum tersedia untuk lokasi ini"})
	}

	return c.JSON(fiber.Map{
		"location":  location,
		"questions": questions,
	})
}

// ============================================================
// POST /api/locations/submit
// Submit hasil kuis & update XP/Level/Stars/Progress
// ============================================================
func SubmitQuiz(c *fiber.Ctx) error {
	type SubmitInput struct {
		PlayerID     uint `json:"player_id"`
		LocationID   uint `json:"location_id"`
		CorrectCount int  `json:"correct_count"`
	}

	input := new(SubmitInput)
	if err := c.BodyParser(input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	// Ambil data lokasi
	var loc models.Location
	if err := config.DB.First(&loc, input.LocationID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Lokasi tidak ditemukan"})
	}

	// Ambil data player
	var player models.Player
	if err := config.DB.First(&player, input.PlayerID).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Player tidak ditemukan"})
	}

	isPass := input.CorrectCount >= loc.MinimumCorrect
	xpGained := 0
	starsGained := 0
	status := "GAGAL"

	// Ambil atau buat progress
	var progress models.PlayerLocationProgress
	isNew := false
	err := config.DB.
		Where("player_id = ? AND location_id = ?", input.PlayerID, input.LocationID).
		First(&progress).Error

	if err != nil {
		// Belum ada progress, buat baru
		progress = models.PlayerLocationProgress{
			PlayerID:   input.PlayerID,
			LocationID: input.LocationID,
			IsUnlocked: true,
		}
		isNew = true
	}

	// Update attempt
	progress.AttemptCount++
	progress.CorrectAnswer = input.CorrectCount
	progress.WrongAnswer = loc.QuestionCount - input.CorrectCount

	if isPass {
		status = "LULUS"
		xpGained = loc.RewardXP
		progress.XPEarned += xpGained

		// Bintang hanya diberikan sekali (pertama kali lulus)
		if !progress.IsCompleted {
			starsGained = loc.RewardStars
			progress.StarsEarned = starsGained
			now := time.Now()
			progress.CompletedAt = &now
		}

		progress.IsCompleted = true

		// Update XP & Stars player
		player.XP += xpGained
		if !progress.IsCompleted || isNew {
			player.Stars += starsGained
		}

		// Level Up Logic (500 XP per level)
		newLevel := (player.XP / 500) + 1
		if newLevel > player.Level {
			player.Level = newLevel
			updatePlayerTitle(&player)
		}

		// Unlock lokasi berikutnya
		unlockNextLocation(input.PlayerID, loc.LevelOrder)
	}

	// Simpan progress
	if isNew {
		config.DB.Create(&progress)
	} else {
		config.DB.Save(&progress)
	}

	// Simpan player
	config.DB.Save(&player)

	return c.JSON(fiber.Map{
		"status":      status,
		"xp":          xpGained,
		"stars":       starsGained,
		"level":       player.Level,
		"total_xp":    player.XP,
		"total_stars": player.Stars,
	})
}

// ============================================================
// GET /api/player/:id/progress
// Ambil semua progress lokasi milik player
// ============================================================
func GetPlayerProgress(c *fiber.Ctx) error {
	playerID := c.Params("id")

	var progress []models.PlayerLocationProgress
	config.DB.
		Where("player_id = ?", playerID).
		Find(&progress)

	return c.JSON(progress)
}

// ============================================================
// HELPER: Update gelar otomatis berdasarkan level
// ============================================================
func updatePlayerTitle(player *models.Player) {
	switch {
	case player.Level >= 20:
		player.TitleID = 5
	case player.Level >= 15:
		player.TitleID = 4
	case player.Level >= 10:
		player.TitleID = 3
	case player.Level >= 5:
		player.TitleID = 2
	default:
		player.TitleID = 1
	}
}

// ============================================================
// HELPER: Unlock lokasi berikutnya setelah lulus
// ============================================================
func unlockNextLocation(playerID uint, currentLevelOrder int) {
	// Cari lokasi berikutnya
	var nextLocation models.Location
	err := config.DB.
		Where("level_order = ? AND is_active = ?", currentLevelOrder+1, true).
		First(&nextLocation).Error

	if err != nil {
		// Tidak ada lokasi berikutnya (sudah selesai semua)
		return
	}

	// Cek apakah sudah ada progress untuk lokasi berikutnya
	var existingProgress models.PlayerLocationProgress
	err = config.DB.
		Where("player_id = ? AND location_id = ?", playerID, nextLocation.ID).
		First(&existingProgress).Error

	if err != nil {
		// Belum ada, buat progress baru dengan is_unlocked = true
		newProgress := models.PlayerLocationProgress{
			PlayerID:   playerID,
			LocationID: nextLocation.ID,
			IsUnlocked: true,
		}
		config.DB.Create(&newProgress)
	} else {
		// Sudah ada, update is_unlocked
		existingProgress.IsUnlocked = true
		config.DB.Save(&existingProgress)
	}
}
