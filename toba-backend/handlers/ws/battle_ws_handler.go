package ws

import (
	"log"
	"strings"

	"toba-backend/services/battle"
	"toba-backend/websocket"

	fiberWS "github.com/gofiber/websocket/v2"
)

func BattleWebSocket(c *fiberWS.Conn) {
	playerID, ok := c.Locals("player_id").(uint)

	if !ok || playerID == 0 {
		_ = c.WriteMessage(
			1,
			[]byte(`{"type":"ERROR","payload":{"message":"Sesi tidak valid"}}`),
		)
		_ = c.Close()
		return
	}

	roomCode := strings.ToUpper(
		strings.TrimSpace(c.Params("roomCode")),
	)

	if roomCode == "" {
		_ = c.Close()
		return
	}

	// Ambil data room berdasarkan identitas session.
	room, message, status := battle.GetBattleRoomStatus(
		roomCode,
		playerID,
	)

	if room == nil {
		log.Printf(
			"[WS] Akses ditolak player=%d room=%s status=%d reason=%s",
			playerID,
			roomCode,
			status,
			message,
		)

		_ = c.WriteMessage(
			1,
			[]byte(`{"type":"ERROR","payload":{"message":"Tidak dapat mengakses room"}}`),
		)
		_ = c.Close()
		return
	}

	if room.Status != "WAITING" && room.Status != "PLAYING" {
		_ = c.Close()
		return
	}

	isHost := room.HostPlayerID == playerID

	playerName := room.GuestName
	if isHost {
		playerName = room.HostName
	}

	client := &websocket.Client{
		PlayerID:   int(playerID),
		RoomID:     int(room.RoomID),
		PlayerName: playerName,
		IsHost:     isHost,
		RoomCode:   roomCode,
		Send:       make(chan []byte, 64),
		Done:       make(chan struct{}),
		Hub:        websocket.Hub,
	}

	websocket.Hub.Register <- &websocket.ClientRegister{
		Client:   client,
		RoomCode: roomCode,
		IsHost:   isHost,
	}

	log.Printf(
		"[WS] Connected player=%d room=%s host=%v",
		playerID,
		roomCode,
		isHost,
	)

	// Writer berhenti melalui Done, bukan dengan menutup
	// channel Send dari goroutine handler.
	go func() {
		for {
			select {
			case <-client.Done:
				return

			case msg := <-client.Send:
				if err := c.WriteMessage(1, msg); err != nil {
					log.Printf("[WS] Write error: %v", err)
					return
				}
			}
		}
	}()

	defer func() {
		websocket.Hub.Unregister <- client
		close(client.Done)

		log.Printf(
			"[WS] Disconnected player=%d room=%s",
			playerID,
			roomCode,
		)
	}()

	for {
		_, message, err := c.ReadMessage()
		if err != nil {
			break
		}

		websocket.Hub.Message <- &websocket.ClientMessage{
			Client:  client,
			Message: message,
		}
	}
}
