package admin

import (
	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func AdminGetAchievements(c *fiber.Ctx) error {
	var achievements []models.Achievement

	config.DB.
		Where("is_active = ?", true).
		Find(&achievements)

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
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
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
		return c.Status(404).JSON(fiber.Map{
			"message": "Achievement tidak ditemukan",
		})
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
		return c.Status(400).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
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

	config.DB.
		Model(&models.Achievement{}).
		Where("id = ?", id).
		Update("is_active", false)

	return c.JSON(fiber.Map{
		"message": "Achievement berhasil dihapus",
	})
}
