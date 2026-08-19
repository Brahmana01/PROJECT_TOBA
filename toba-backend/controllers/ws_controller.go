package controllers

import (
	"log"
	"toba-backend/websocket"

	fiberWS "github.com/gofiber/websocket/v2"
)

// ============================================================
// GET /ws/battle/:roomCode/:playerID/:playerName/:isHost
// WebSocket endpoint untuk battle real-time
// ============================================================
func BattleWebSocket(c *fiberWS.Conn) {
	roomCode := c.Params("roomCode")
	playerID := c.Params("playerID")
	playerName := c.Params("playerName")
	isHostStr := c.Params("isHost")

	isHost := isHostStr == "true"

	// Parse playerID
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

	// Register ke hub
	websocket.Hub.Register <- &websocket.ClientRegister{
		Client:   client,
		RoomCode: roomCode,
		IsHost:   isHost,
	}

	log.Printf("WS Connected: player=%s room=%s host=%v", playerName, roomCode, isHost)

	// Goroutine: kirim pesan ke client
	go func() {
		for msg := range client.Send {
			if err := c.WriteMessage(1, msg); err != nil {
				log.Printf("WS write error: %v", err)
				return
			}
		}
	}()

	// Goroutine: terima pesan dari client
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
