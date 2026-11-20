package routes

import (
	"toba-backend/handlers/ws"
	"toba-backend/middleware"

	"github.com/gofiber/fiber/v2"
	fiberWS "github.com/gofiber/websocket/v2"
)

func RegisterWSRoutes(app *fiber.App) {
	app.Use("/ws", func(c *fiber.Ctx) error {
		if fiberWS.IsWebSocketUpgrade(c) {
			return c.Next()
		}

		return fiber.ErrUpgradeRequired
	})

	app.Get(
		"/ws/battle/:roomCode",
		middleware.AuthMiddleware,
		fiberWS.New(ws.BattleWebSocket),
	)
}
