package ws

import (
	"log"
	"toba-backend/websocket"

	fiberWS "github.com/gofiber/websocket/v2"
)

func BattleWebSocket(c *fiberWS.Conn) {
	roomCode := c.Params("roomCode")
	playerID := c.Params("playerID")
	playerName := c.Params("playerName")
	isHost := c.Params("isHost") == "true"

	var pID int
	for _, ch := range playerID {
		if ch >= '0' && ch <= '9' {
			pID = pID*10 + int(ch-'0')
		}
	}

	client := &websocket.Client{
		PlayerID:   pID,
		PlayerName: playerName,
		IsHost:     isHost,
		RoomCode:   roomCode,
		Send:       make(chan []byte, 64),
		Hub:        websocket.Hub,
	}

	websocket.Hub.Register <- &websocket.ClientRegister{
		Client:   client,
		RoomCode: roomCode,
		IsHost:   isHost,
	}

	log.Printf("WS Connected: player=%s room=%s host=%v", playerName, roomCode, isHost)

	go func() {
		for msg := range client.Send {
			if err := c.WriteMessage(1, msg); err != nil {
				log.Printf("WS write error: %v", err)
				return
			}
		}
	}()

	defer func() {
		websocket.Hub.Unregister <- client
		close(client.Send)
		log.Printf("WS Disconnected: player=%s room=%s", playerName, roomCode)
	}()

	for {
		_, msg, err := c.ReadMessage()
		if err != nil {
			break
		}

		websocket.Hub.Message <- &websocket.ClientMessage{
			Client:  client,
			Message: msg,
		}
	}
}
