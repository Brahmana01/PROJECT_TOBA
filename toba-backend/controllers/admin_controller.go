package controllers

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

// ============================================================
// STATS & PLAYERS (sudah ada, pertahankan)
// ============================================================

func GetAdminStats(c *fiber.Ctx) error {
	var totalPlayers int64
	config.DB.Model(&models.Player{}).Where("is_active = ?", true).Count(&totalPlayers)
	var totalBattle int64
	config.DB.Model(&models.BattleRoom{}).Where("status = ?", "FINISHED").Count(&totalBattle)
	var totalTrivia int64
	config.DB.Model(&models.PlayerTriviaHistory{}).Count(&totalTrivia)
	var totalLocation int64
	config.DB.Model(&models.PlayerLocationProgress{}).Where("is_completed = ?", true).Count(&totalLocation)

	return c.JSON(fiber.Map{
		"total_players":  totalPlayers,
		"total_battle":   totalBattle,
		"total_trivia":   totalTrivia,
		"total_location": totalLocation,
	})
}

func GetAllPlayers(c *fiber.Ctx) error {
	var players []models.Player
	config.DB.Where("is_active = ?", true).Order("created_at DESC").Find(&players)
	return c.JSON(fiber.Map{"total": len(players), "players": players})
}

// ============================================================
// LOKASI — CRUD
// ============================================================

func AdminGetLocations(c *fiber.Ctx) error {
	var locations []models.Location
	config.DB.Where("deleted_at IS NULL").Order("level_order ASC").Find(&locations)
	return c.JSON(locations)
}

func AdminCreateLocation(c *fiber.Ctx) error {
	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Image          string `json:"image"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		QuestionCount  int    `json:"question_count"`
		MinimumCorrect int    `json:"minimum_correct"`
		LevelOrder     int    `json:"level_order"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	loc := models.Location{
		Name:           input.Name,
		Description:    input.Description,
		Image:          input.Image,
		RewardXP:       input.RewardXP,
		RewardStars:    input.RewardStars,
		QuestionCount:  input.QuestionCount,
		MinimumCorrect: input.MinimumCorrect,
		LevelOrder:     input.LevelOrder,
		IsActive:       true,
	}
	if err := config.DB.Create(&loc).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal membuat lokasi"})
	}
	return c.Status(201).JSON(loc)
}

func AdminUpdateLocation(c *fiber.Ctx) error {
	id := c.Params("id")
	var loc models.Location
	if err := config.DB.Where("id = ? AND deleted_at IS NULL", id).First(&loc).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Lokasi tidak ditemukan"})
	}
	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Image          string `json:"image"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		QuestionCount  int    `json:"question_count"`
		MinimumCorrect int    `json:"minimum_correct"`
		LevelOrder     int    `json:"level_order"`
		IsActive       bool   `json:"is_active"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	loc.Name = input.Name
	loc.Description = input.Description
	loc.Image = input.Image
	loc.RewardXP = input.RewardXP
	loc.RewardStars = input.RewardStars
	loc.QuestionCount = input.QuestionCount
	loc.MinimumCorrect = input.MinimumCorrect
	loc.LevelOrder = input.LevelOrder
	loc.IsActive = input.IsActive
	config.DB.Save(&loc)
	return c.JSON(loc)
}

func AdminDeleteLocation(c *fiber.Ctx) error {
	id := c.Params("id")
	var loc models.Location
	if err := config.DB.Where("id = ? AND deleted_at IS NULL", id).First(&loc).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Lokasi tidak ditemukan"})
	}
	// Soft delete
	now := time.Now()
	loc.DeletedAt = &now
	config.DB.Save(&loc)
	return c.JSON(fiber.Map{"message": "Lokasi berhasil dihapus"})
}

// ── Upload gambar lokasi ──────────────────────────────────
func AdminUploadLocationImage(c *fiber.Ctx) error {
	file, err := c.FormFile("image")
	if err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "File tidak ditemukan"})
	}

	// Validasi ekstensi
	ext := strings.ToLower(filepath.Ext(file.Filename))
	allowed := map[string]bool{".jpg": true, ".jpeg": true, ".png": true, ".webp": true}
	if !allowed[ext] {
		return c.Status(400).JSON(fiber.Map{"message": "Format file tidak didukung. Gunakan jpg/png/webp"})
	}

	// Buat folder uploads jika belum ada
	uploadDir := "./uploads/locations"
	os.MkdirAll(uploadDir, 0755)

	// Nama file unik
	filename := fmt.Sprintf("loc_%d%s", time.Now().UnixNano(), ext)
	savePath := filepath.Join(uploadDir, filename)

	if err := c.SaveFile(file, savePath); err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal menyimpan file"})
	}

	return c.JSON(fiber.Map{
		"filename": filename,
		"url":      "/uploads/locations/" + filename,
	})
}

// ============================================================
// FAKTA BUDAYA — CRUD
// ============================================================

func AdminGetLocationFacts(c *fiber.Ctx) error {
	locationID := c.Params("id")
	var facts []models.LocationFact
	config.DB.Where("location_id = ?", locationID).Find(&facts)
	return c.JSON(facts)
}

func AdminCreateLocationFact(c *fiber.Ctx) error {
	locationID := c.Params("id")
	type Input struct {
		Fact string `json:"fact"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	locID := parseUint(locationID)
	fact := models.LocationFact{LocationID: locID, Fact: input.Fact}
	config.DB.Create(&fact)
	return c.Status(201).JSON(fact)
}

func AdminDeleteLocationFact(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Delete(&models.LocationFact{}, id)
	return c.JSON(fiber.Map{"message": "Fakta berhasil dihapus"})
}

// ============================================================
// SOAL JELAJAH — CRUD
// ============================================================

func AdminGetLocationQuestions(c *fiber.Ctx) error {
	locationID := c.Params("id")
	var questions []models.LocationQuestion
	config.DB.Preload("Options").Where("location_id = ?", locationID).Find(&questions)
	return c.JSON(questions)
}

func AdminCreateLocationQuestion(c *fiber.Ctx) error {
	locationID := c.Params("id")
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if len(input.Options) < 2 {
		return c.Status(400).JSON(fiber.Map{"message": "Minimal 2 pilihan jawaban"})
	}

	locID := parseUint(locationID)
	question := models.LocationQuestion{
		LocationID:  locID,
		Question:    input.Question,
		Explanation: input.Explanation,
		IsActive:    true,
	}
	config.DB.Create(&question)

	for _, opt := range input.Options {
		config.DB.Create(&models.LocationQuestionOption{
			QuestionID: question.ID,
			OptionText: opt.OptionText,
			IsCorrect:  opt.IsCorrect,
		})
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.Status(201).JSON(question)
}

func AdminUpdateLocationQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		IsActive    bool          `json:"is_active"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var question models.LocationQuestion
	if err := config.DB.First(&question, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Soal tidak ditemukan"})
	}

	question.Question = input.Question
	question.Explanation = input.Explanation
	question.IsActive = input.IsActive
	config.DB.Save(&question)

	// Update opsi — hapus lama, buat baru
	if len(input.Options) > 0 {
		config.DB.Where("question_id = ?", question.ID).Delete(&models.LocationQuestionOption{})
		for _, opt := range input.Options {
			config.DB.Create(&models.LocationQuestionOption{
				QuestionID: question.ID,
				OptionText: opt.OptionText,
				IsCorrect:  opt.IsCorrect,
			})
		}
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.JSON(question)
}

func AdminDeleteLocationQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Where("question_id = ?", id).Delete(&models.LocationQuestionOption{})
	config.DB.Delete(&models.LocationQuestion{}, id)
	return c.JSON(fiber.Map{"message": "Soal berhasil dihapus"})
}

// ============================================================
// KATEGORI TRIVIA — CRUD
// ============================================================

func AdminGetTriviaCategories(c *fiber.Ctx) error {
	var cats []models.TriviaCategory
	config.DB.Where("deleted_at IS NULL").Order("display_order ASC").Find(&cats)
	return c.JSON(cats)
}

func AdminCreateTriviaCategory(c *fiber.Ctx) error {
	type Input struct {
		Name          string `json:"name"`
		Description   string `json:"description"`
		QuestionCount int    `json:"question_count"`
		DisplayOrder  int    `json:"display_order"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	cat := models.TriviaCategory{
		Name:            input.Name,
		Description:     input.Description,
		QuestionCount:   input.QuestionCount,
		MinimumQuestion: input.QuestionCount,
		DisplayOrder:    input.DisplayOrder,
		IsActive:        true,
	}
	config.DB.Create(&cat)
	return c.Status(201).JSON(cat)
}

func AdminUpdateTriviaCategory(c *fiber.Ctx) error {
	id := c.Params("id")
	var cat models.TriviaCategory
	if err := config.DB.First(&cat, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Kategori tidak ditemukan"})
	}
	type Input struct {
		Name          string `json:"name"`
		Description   string `json:"description"`
		QuestionCount int    `json:"question_count"`
		DisplayOrder  int    `json:"display_order"`
		IsActive      bool   `json:"is_active"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	cat.Name = input.Name
	cat.Description = input.Description
	cat.QuestionCount = input.QuestionCount
	cat.DisplayOrder = input.DisplayOrder
	cat.IsActive = input.IsActive
	config.DB.Save(&cat)
	return c.JSON(cat)
}

func AdminDeleteTriviaCategory(c *fiber.Ctx) error {
	id := c.Params("id")
	now := time.Now()
	config.DB.Model(&models.TriviaCategory{}).Where("id = ?", id).Update("deleted_at", now)
	return c.JSON(fiber.Map{"message": "Kategori berhasil dihapus"})
}

// ============================================================
// SOAL TRIVIA — CRUD
// ============================================================

func AdminGetTriviaQuestions(c *fiber.Ctx) error {
	categoryID := c.Params("id")
	var questions []models.TriviaQuestion
	config.DB.Preload("Options").Where("category_id = ?", categoryID).Find(&questions)
	return c.JSON(questions)
}

func AdminCreateTriviaQuestion(c *fiber.Ctx) error {
	categoryID := c.Params("id")
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	catID := parseUint(categoryID)
	question := models.TriviaQuestion{
		CategoryID:  catID,
		Question:    input.Question,
		Explanation: input.Explanation,
		IsActive:    true,
	}
	config.DB.Create(&question)

	for _, opt := range input.Options {
		config.DB.Create(&models.TriviaQuestionOption{
			QuestionID: question.ID,
			OptionText: opt.OptionText,
			IsCorrect:  opt.IsCorrect,
		})
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.Status(201).JSON(question)
}

func AdminUpdateTriviaQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		IsActive    bool          `json:"is_active"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var question models.TriviaQuestion
	if err := config.DB.First(&question, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Soal tidak ditemukan"})
	}

	question.Question = input.Question
	question.Explanation = input.Explanation
	question.IsActive = input.IsActive
	config.DB.Save(&question)

	if len(input.Options) > 0 {
		config.DB.Where("question_id = ?", question.ID).Delete(&models.TriviaQuestionOption{})
		for _, opt := range input.Options {
			config.DB.Create(&models.TriviaQuestionOption{
				QuestionID: question.ID,
				OptionText: opt.OptionText,
				IsCorrect:  opt.IsCorrect,
			})
		}
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.JSON(question)
}

func AdminDeleteTriviaQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Where("question_id = ?", id).Delete(&models.TriviaQuestionOption{})
	config.DB.Delete(&models.TriviaQuestion{}, id)
	return c.JSON(fiber.Map{"message": "Soal trivia berhasil dihapus"})
}

// ============================================================
// SOAL BATTLE — CRUD
// ============================================================

func AdminGetBattleQuestions(c *fiber.Ctx) error {
	var questions []models.BattleQuestion
	config.DB.Preload("Options").Find(&questions)
	return c.JSON(questions)
}

func AdminCreateBattleQuestion(c *fiber.Ctx) error {
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	question := models.BattleQuestion{
		Question:    input.Question,
		Explanation: input.Explanation,
		IsActive:    true,
	}
	config.DB.Create(&question)

	for _, opt := range input.Options {
		config.DB.Create(&models.BattleQuestionOption{
			QuestionID: question.ID,
			OptionText: opt.OptionText,
			IsCorrect:  opt.IsCorrect,
		})
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.Status(201).JSON(question)
}

func AdminUpdateBattleQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	type OptionInput struct {
		OptionText string `json:"option_text"`
		IsCorrect  bool   `json:"is_correct"`
	}
	type Input struct {
		Question    string        `json:"question"`
		Explanation string        `json:"explanation"`
		IsActive    bool          `json:"is_active"`
		Options     []OptionInput `json:"options"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	var question models.BattleQuestion
	if err := config.DB.First(&question, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Soal tidak ditemukan"})
	}

	question.Question = input.Question
	question.Explanation = input.Explanation
	question.IsActive = input.IsActive
	config.DB.Save(&question)

	if len(input.Options) > 0 {
		config.DB.Where("question_id = ?", question.ID).Delete(&models.BattleQuestionOption{})
		for _, opt := range input.Options {
			config.DB.Create(&models.BattleQuestionOption{
				QuestionID: question.ID,
				OptionText: opt.OptionText,
				IsCorrect:  opt.IsCorrect,
			})
		}
	}

	config.DB.Preload("Options").First(&question, question.ID)
	return c.JSON(question)
}

func AdminDeleteBattleQuestion(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Where("question_id = ?", id).Delete(&models.BattleQuestionOption{})
	config.DB.Delete(&models.BattleQuestion{}, id)
	return c.JSON(fiber.Map{"message": "Soal battle berhasil dihapus"})
}

// ============================================================
// ACHIEVEMENT — CRUD
// ============================================================

func AdminGetAchievements(c *fiber.Ctx) error {
	var achievements []models.Achievement
	config.DB.Where("is_active = ?", true).Find(&achievements)
	return c.JSON(achievements)
}

func AdminCreateAchievement(c *fiber.Ctx) error {
	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Icon           string `json:"icon"`
		ConditionType  string `json:"condition_type"`
		ConditionValue int    `json:"condition_value"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	ach := models.Achievement{
		Name:           input.Name,
		Description:    input.Description,
		Icon:           input.Icon,
		ConditionType:  input.ConditionType,
		ConditionValue: input.ConditionValue,
		RewardXP:       input.RewardXP,
		RewardStars:    input.RewardStars,
		IsActive:       true,
	}
	config.DB.Create(&ach)
	return c.Status(201).JSON(ach)
}

func AdminUpdateAchievement(c *fiber.Ctx) error {
	id := c.Params("id")
	var ach models.Achievement
	if err := config.DB.First(&ach, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Achievement tidak ditemukan"})
	}
	type Input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Icon           string `json:"icon"`
		ConditionType  string `json:"condition_type"`
		ConditionValue int    `json:"condition_value"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		IsActive       bool   `json:"is_active"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	ach.Name = input.Name
	ach.Description = input.Description
	ach.Icon = input.Icon
	ach.ConditionType = input.ConditionType
	ach.ConditionValue = input.ConditionValue
	ach.RewardXP = input.RewardXP
	ach.RewardStars = input.RewardStars
	ach.IsActive = input.IsActive
	config.DB.Save(&ach)
	return c.JSON(ach)
}

func AdminDeleteAchievement(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Model(&models.Achievement{}).Where("id = ?", id).Update("is_active", false)
	return c.JSON(fiber.Map{"message": "Achievement berhasil dihapus"})
}

// ============================================================
// GAME SETTINGS — GET & UPDATE
// ============================================================

func AdminGetSettings(c *fiber.Ctx) error {
	var settings []models.GameSetting
	config.DB.Find(&settings)
	return c.JSON(settings)
}

func AdminUpdateSetting(c *fiber.Ctx) error {
	key := c.Params("key")
	type Input struct {
		Value string `json:"value"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	result := config.DB.Model(&models.GameSetting{}).
		Where("setting_key = ?", key).
		Update("setting_value", input.Value)
	if result.RowsAffected == 0 {
		return c.Status(404).JSON(fiber.Map{"message": "Setting tidak ditemukan"})
	}
	return c.JSON(fiber.Map{"message": "Setting berhasil diupdate", "key": key, "value": input.Value})
}

// ============================================================
// MUSIK / AUDIO — CRUD
// ============================================================

func AdminGetMusicList(c *fiber.Ctx) error {
	uploadDir := "./uploads/music"
	os.MkdirAll(uploadDir, 0755)

	entries, err := os.ReadDir(uploadDir)
	if err != nil {
		return c.JSON([]fiber.Map{})
	}

	var files []fiber.Map
	for _, entry := range entries {
		if !entry.IsDir() {
			info, _ := entry.Info()
			files = append(files, fiber.Map{
				"filename": entry.Name(),
				"url":      "/uploads/music/" + entry.Name(),
				"size":     info.Size(),
				"type":     strings.ToLower(filepath.Ext(entry.Name())),
			})
		}
	}
	if files == nil {
		files = []fiber.Map{}
	}
	return c.JSON(files)
}

func AdminUploadMusic(c *fiber.Ctx) error {
	file, err := c.FormFile("music")
	if err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "File tidak ditemukan"})
	}

	ext := strings.ToLower(filepath.Ext(file.Filename))
	allowed := map[string]bool{".mp3": true, ".ogg": true, ".wav": true}
	if !allowed[ext] {
		return c.Status(400).JSON(fiber.Map{"message": "Format file tidak didukung. Gunakan mp3/ogg/wav"})
	}

	uploadDir := "./uploads/music"
	os.MkdirAll(uploadDir, 0755)

	filename := fmt.Sprintf("music_%d%s", time.Now().UnixNano(), ext)
	savePath := filepath.Join(uploadDir, filename)

	if err := c.SaveFile(file, savePath); err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal menyimpan file"})
	}

	return c.JSON(fiber.Map{
		"filename": filename,
		"url":      "/uploads/music/" + filename,
	})
}

func AdminDeleteMusic(c *fiber.Ctx) error {
	filename := c.Params("filename")
	// Validasi keamanan — hanya boleh hapus file di folder music
	if strings.Contains(filename, "..") || strings.Contains(filename, "/") {
		return c.Status(400).JSON(fiber.Map{"message": "Nama file tidak valid"})
	}
	path := filepath.Join("./uploads/music", filename)
	if err := os.Remove(path); err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "File tidak ditemukan"})
	}
	return c.JSON(fiber.Map{"message": "File berhasil dihapus"})
}

// ============================================================
// GALERI KARTU BUDAYA — CRUD
// ============================================================

func AdminGetGalleryCards(c *fiber.Ctx) error {
	var cards []models.GalleryCard
	config.DB.Find(&cards)
	return c.JSON(cards)
}

func AdminCreateGalleryCard(c *fiber.Ctx) error {
	type Input struct {
		LocationID  uint   `json:"location_id"`
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	card := models.GalleryCard{
		LocationID:  input.LocationID,
		Title:       input.Title,
		Image:       input.Image,
		Description: input.Description,
	}
	config.DB.Create(&card)
	return c.Status(201).JSON(card)
}

func AdminUpdateGalleryCard(c *fiber.Ctx) error {
	id := c.Params("id")
	var card models.GalleryCard
	if err := config.DB.First(&card, id).Error; err != nil {
		return c.Status(404).JSON(fiber.Map{"message": "Kartu tidak ditemukan"})
	}
	type Input struct {
		Title       string `json:"title"`
		Image       string `json:"image"`
		Description string `json:"description"`
	}
	var input Input
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	card.Title = input.Title
	card.Image = input.Image
	card.Description = input.Description
	config.DB.Save(&card)
	return c.JSON(card)
}

func AdminDeleteGalleryCard(c *fiber.Ctx) error {
	id := c.Params("id")
	config.DB.Delete(&models.GalleryCard{}, id)
	return c.JSON(fiber.Map{"message": "Kartu berhasil dihapus"})
}

// ============================================================
// HELPER
// ============================================================

func parseUint(s string) uint {
	var n uint
	fmt.Sscanf(s, "%d", &n)
	return n
}
