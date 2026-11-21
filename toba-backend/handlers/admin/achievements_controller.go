package admin

import (
	"strings"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
)

var supportedAchievementConditions = map[string]bool{
	"LEVEL": true, "STAR": true, "STARS": true, "XP": true,
	"LOCATION": true, "ALL_LOCATION": true,
	"TRIVIA": true, "TRIVIA_CORRECT": true,
	"BATTLE": true, "BATTLE_WIN": true,
	"CHALLENGE": true, "CHALLENGE_SCORE": true,
	"ALL_ACHIEVEMENT": true,
}

func AdminGetAchievements(c *fiber.Ctx) error {
	var achievements []models.Achievement
	if err := config.DB.Where("is_active = ?", true).Order("id ASC").Find(&achievements).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal mengambil achievement"})
	}
	return c.JSON(achievements)
}

func validateAchievementInput(name, description, conditionType string, conditionValue, rewardXP, rewardStars int) string {
	conditionType = strings.ToUpper(strings.TrimSpace(conditionType))
	if strings.TrimSpace(name) == "" || strings.TrimSpace(description) == "" {
		return "Nama dan deskripsi achievement wajib diisi"
	}
	if !supportedAchievementConditions[conditionType] {
		return "Tipe kondisi achievement tidak didukung"
	}
	if conditionValue <= 0 || rewardXP < 0 || rewardStars < 0 {
		return "Nilai kondisi harus lebih dari 0 dan reward tidak boleh negatif"
	}
	return ""
}

func AdminCreateAchievement(c *fiber.Ctx) error {
	var input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Icon           string `json:"icon"`
		ConditionType  string `json:"condition_type"`
		ConditionValue int    `json:"condition_value"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if message := validateAchievementInput(input.Name, input.Description, input.ConditionType, input.ConditionValue, input.RewardXP, input.RewardStars); message != "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": message})
	}

	achievement := models.Achievement{
		Name: strings.TrimSpace(input.Name), Description: strings.TrimSpace(input.Description),
		Icon: strings.TrimSpace(input.Icon), ConditionType: strings.ToUpper(strings.TrimSpace(input.ConditionType)),
		ConditionValue: input.ConditionValue, RewardXP: input.RewardXP,
		RewardStars: input.RewardStars, IsActive: true,
	}
	if err := config.DB.Create(&achievement).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal membuat achievement"})
	}
	return c.Status(fiber.StatusCreated).JSON(achievement)
}

func AdminUpdateAchievement(c *fiber.Ctx) error {
	id := c.Params("id")
	var achievement models.Achievement
	if err := config.DB.First(&achievement, id).Error; err != nil {
		if err == gorm.ErrRecordNotFound {
			return c.Status(fiber.StatusNotFound).JSON(fiber.Map{"message": "Achievement tidak ditemukan"})
		}
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal mengambil achievement"})
	}

	var input struct {
		Name           string `json:"name"`
		Description    string `json:"description"`
		Icon           string `json:"icon"`
		ConditionType  string `json:"condition_type"`
		ConditionValue int    `json:"condition_value"`
		RewardXP       int    `json:"reward_xp"`
		RewardStars    int    `json:"reward_stars"`
		IsActive       bool   `json:"is_active"`
	}
	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": "Input tidak valid"})
	}
	if message := validateAchievementInput(input.Name, input.Description, input.ConditionType, input.ConditionValue, input.RewardXP, input.RewardStars); message != "" {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"message": message})
	}

	achievement.Name = strings.TrimSpace(input.Name)
	achievement.Description = strings.TrimSpace(input.Description)
	achievement.Icon = strings.TrimSpace(input.Icon)
	achievement.ConditionType = strings.ToUpper(strings.TrimSpace(input.ConditionType))
	achievement.ConditionValue = input.ConditionValue
	achievement.RewardXP = input.RewardXP
	achievement.RewardStars = input.RewardStars
	achievement.IsActive = input.IsActive
	if err := config.DB.Save(&achievement).Error; err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal memperbarui achievement"})
	}
	return c.JSON(achievement)
}

func AdminDeleteAchievement(c *fiber.Ctx) error {
	result := config.DB.Model(&models.Achievement{}).Where("id = ?", c.Params("id")).Update("is_active", false)
	if result.Error != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"message": "Gagal menonaktifkan achievement"})
	}
	if result.RowsAffected == 0 {
		return c.Status(fiber.StatusNotFound).JSON(fiber.Map{"message": "Achievement tidak ditemukan"})
	}
	return c.JSON(fiber.Map{"message": "Achievement berhasil dinonaktifkan"})
}
