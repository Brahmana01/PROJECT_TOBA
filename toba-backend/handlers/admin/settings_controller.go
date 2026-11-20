package admin

import (
	"strconv"
	"strings"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

var lockedGameSettingKeys = map[string]bool{
	"quiz_question_count":   true,
	"min_correct_to_pass":   true,
	"battle_question_count": true,
	"xp_per_level":          true,
}

func AdminGetSettings(c *fiber.Ctx) error {
	var settings []models.GameSetting
	if err := config.DB.Order("id ASC").Find(&settings).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal mengambil setting"})
	}

	// Nilai setting inti ditampilkan dari konstanta gameplay agar data lama
	// (misalnya masih 5 soal) tidak menyesatkan Admin sebelum seed final.
	for i := range settings {
		switch settings[i].SettingKey {
		case "quiz_question_count":
			settings[i].SettingValue = strconv.Itoa(models.LocationQuestionCount)
		case "min_correct_to_pass":
			settings[i].SettingValue = strconv.Itoa(models.LocationMinimumCorrect)
		case "battle_question_count":
			settings[i].SettingValue = strconv.Itoa(models.BattleQuestionCount)
		case "xp_per_level":
			settings[i].SettingValue = strconv.Itoa(models.DefaultXPPerLevel)
		}
	}
	return c.JSON(settings)
}

func AdminUpdateSetting(c *fiber.Ctx) error {
	key := strings.TrimSpace(c.Params("key"))

	var input struct {
		Value string `json:"value"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(400).JSON(fiber.Map{"message": "Input tidak valid"})
	}

	if lockedGameSettingKeys[key] {
		return c.Status(fiber.StatusConflict).JSON(fiber.Map{
			"message":     "Setting ini dikunci karena merupakan aturan inti gameplay TOBA",
			"setting_key": key,
		})
	}

	if strings.TrimSpace(input.Value) == "" {
		return c.Status(400).JSON(fiber.Map{"message": "Nilai setting tidak boleh kosong"})
	}

	// Validasi numeric settings yang memang masih boleh diedit.
	switch key {
	case "quiz_time_limit", "battle_time_per_question", "matchmaking_timeout":
		value, err := strconv.Atoi(strings.TrimSpace(input.Value))
		if err != nil || value <= 0 {
			return c.Status(400).JSON(fiber.Map{
				"message": "Nilai harus berupa bilangan bulat lebih dari 0",
			})
		}
		input.Value = strconv.Itoa(value)
	}

	result := config.DB.
		Model(&models.GameSetting{}).
		Where("setting_key = ?", key).
		Update("setting_value", input.Value)

	if result.Error != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Gagal memperbarui setting"})
	}
	if result.RowsAffected == 0 {
		return c.Status(404).JSON(fiber.Map{"message": "Setting tidak ditemukan"})
	}

	var setting models.GameSetting
	if err := config.DB.Where("setting_key = ?", key).First(&setting).Error; err != nil {
		return c.Status(500).JSON(fiber.Map{"message": "Setting berhasil diubah tetapi gagal mengambil hasil"})
	}

	return c.JSON(setting)
}
