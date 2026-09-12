package routes

import (
	"github.com/gofiber/fiber/v2"

	"toba-backend/handlers/battle"
)

func RegisterBattleRoutes(authGroup fiber.Router) {
	authGroup.Post("/battle/create", battle.CreateRoom)
	authGroup.Post("/battle/join", battle.JoinRoom)
	authGroup.Get("/battle/status/:code", battle.GetRoomStatus)
	authGroup.Post("/battle/finish", battle.FinishBattle)
	authGroup.Post("/battle/disconnect", battle.DisconnectBattle)
	authGroup.Post("/battle/answer", battle.SubmitBattleAnswer)

	authGroup.Post("/battle/matchmake", battle.Matchmake)
	authGroup.Get("/battle/matchmake/check/:player_id", battle.CheckMatchmake)
	authGroup.Delete("/battle/matchmake/cancel/:player_id", battle.CancelMatchmake)
}
