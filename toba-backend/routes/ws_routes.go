package routes

import (
	"github.com/gofiber/fiber/v2"
	fiberWS "github.com/gofiber/websocket/v2"

	"toba-backend/handlers/ws"
)

func RegisterWSRoutes(app *fiber.App) {
	app.Use("/ws", func(c *fiber.Ctx) error {
		if fiberWS.IsWebSocketUpgrade(c) {
			return c.Next()
		}

		return fiber.ErrUpgradeRequired
	})

	app.Get(
		"/ws/battle/:roomCode/:playerID/:playerName/:isHost",
		fiberWS.New(ws.BattleWebSocket),
	)
}
