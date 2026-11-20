package battle

import (
	"toba-backend/middleware"
	battleService "toba-backend/services/battle"

	"github.com/gofiber/fiber/v2"
)

func Matchmake(c *fiber.Ctx) error {
	var input struct {
		PlayerID uint `json:"player_id"`
	}

	if err := c.BodyParser(&input); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{
			"message": "Input tidak valid",
		})
	}

	playerID, err := middleware.RequirePlayerID(c, input.PlayerID)
	if err != nil {
		return err
	}

	result, message, status := battleService.MatchmakePlayer(playerID)

	if result == nil {
		return c.Status(status).JSON(fiber.Map{
			"message": message,
		})
	}

	return c.Status(status).JSON(result)
}

func CheckMatchmake(c *fiber.Ctx) error {
	playerID, err := middleware.RequirePathPlayerID(
		c,
		c.Params("player_id"),
	)

	if err != nil {
		return err
	}

	return c.JSON(battleService.CheckMatchmaking(playerID))
}

func CancelMatchmake(c *fiber.Ctx) error {
	playerID, err := middleware.RequirePathPlayerID(
		c,
		c.Params("player_id"),
	)

	if err != nil {
		return err
	}

	result := battleService.CancelMatchmaking(playerID)

	return c.JSON(result)
}
