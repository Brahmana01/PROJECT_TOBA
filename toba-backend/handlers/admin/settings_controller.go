package admin

import (
	"strconv"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

var lockedGameSettingKeys = map[string]bool{
	// Canonical gameplay keys.
	"quiz_question_count":        true,
	"trivia_question_count":      true,
	"min_correct_to_pass":        true,
	"battle_question_count":      true,
	"battle_bank_question_count": true,
	"xp_per_level":               true,
	// Legacy alias retained for old Admin builds.
	"location_question_count": true,
}

func AdminGetSettings(c *fiber.Ctx) error {
	var settings []models.GameSetting
	if err := config.DB.Order("id ASC").Find(&settings).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal mengambil setting"})
	}

	lockedValues := []models.GameSetting{
		{SettingKey: "quiz_question_count", SettingValue: strconv.Itoa(models.LocationQuestionCount), Description: "Jumlah soal Jelajah Budaya per sesi (dikunci)"},
		{SettingKey: "trivia_question_count", SettingValue: strconv.Itoa(models.TriviaQuestionCount), Description: "Jumlah soal Trivia Budaya per sesi (dikunci)"},
		{SettingKey: "min_correct_to_pass", SettingValue: strconv.Itoa(models.LocationMinimumCorrect), Description: "Jumlah minimum jawaban benar Jelajah (dikunci)"},
		{SettingKey: "battle_question_count", SettingValue: strconv.Itoa(models.BattleQuestionCount), Description: "Jumlah soal Battle per pertandingan (dikunci)"},
		{SettingKey: "battle_bank_question_count", SettingValue: strconv.Itoa(models.BattleBankQuestionCount), Description: "Target bank soal Battle (acuan konten)"},
		{SettingKey: "xp_per_level", SettingValue: strconv.Itoa(models.DefaultXPPerLevel), Description: "XP yang dibutuhkan untuk naik satu level (dikunci)"},
	}

	indexByKey := make(map[string]int, len(settings))
	for i := range settings {
		indexByKey[settings[i].SettingKey] = i
	}
	for _, locked := range lockedValues {
		if index, exists := indexByKey[locked.SettingKey]; exists {
			settings[index].SettingValue = locked.SettingValue
			settings[index].Description = locked.Description
		} else {
			locked.UpdatedAt = time.Time{}
			settings = append(settings, locked)
		}
	}

	// Legacy alias must also report the current value, never the stale seeded value.
	if index, exists := indexByKey["location_question_count"]; exists {
		settings[index].SettingValue = strconv.Itoa(models.LocationQuestionCount)
		settings[index].Description = "Alias lama jumlah soal Jelajah per sesi (dikunci)"
	}

	return c.JSON(settings)
}

func AdminUpdateSetting(c *fiber.Ctx) error {
	key := strings.TrimSpace(c.Params("key"))
	var input struct {
		Value string `json:"value"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if lockedGameSettingKeys[key] {
		return c.Status(fiber.StatusConflict).JSON(fiber.Map{
			"message":     "Setting ini dikunci karena merupakan aturan inti gameplay TOBA",
			"setting_key": key,
		})
	}
	if strings.TrimSpace(input.Value) == "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Nilai setting tidak boleh kosong"})
	}

	switch key {
	case "quiz_time_limit", "battle_time_per_question", "matchmaking_timeout":
		value, err := strconv.Atoi(strings.TrimSpace(input.Value))
		if err != nil || value <= 0 {
			return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Nilai harus berupa bilangan bulat lebih dari 0"})
		}
		input.Value = strconv.Itoa(value)
	}

	result := config.DB.Model(&models.GameSetting{}).
		Where("setting_key = ?", key).
		Update("setting_value", input.Value)
	if result.Error != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal memperbarui setting"})
	}
	if result.RowsAffected == 0 {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{"message": "Setting tidak ditemukan"})
	}

	var setting models.GameSetting
	if err := config.DB.Where("setting_key = ?", key).First(&setting).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Setting berhasil diubah tetapi gagal mengambil hasil"})
	}
	return c.JSON(setting)
}
