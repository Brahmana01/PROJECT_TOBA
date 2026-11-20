package websocket

import (
	"encoding/json"
	"log"
	"sync"
	"time"

	battleService "toba-backend/services/battle"
)

const (
	MsgTypeJoin        = "JOIN"
	MsgTypeReady       = "READY"
	MsgTypeStart       = "START"
	MsgTypeAnswer      = "ANSWER"
	MsgTypeScoreUpdate = "SCORE_UPDATE"
	MsgTypeFinish      = "FINISH"
	MsgTypeDisconnect  = "DISCONNECT"
	MsgTypePing        = "PING"
	MsgTypePong        = "PONG"
	MsgTypeError       = "ERROR"
)

type WSMessage struct {
	Type    string          `json:"type"`
	Payload json.RawMessage `json:"payload"`
}

type JoinPayload struct {
	PlayerID   int    `json:"player_id"`
	PlayerName string `json:"player_name"`
	RoomCode   string `json:"room_code"`
	IsHost     bool   `json:"is_host"`
}

type Client struct {
	PlayerID   int
	PlayerName string
	IsHost     bool
	RoomID     int
	RoomCode   string
	Send       chan []byte
	Done       chan struct{}
	Hub        *BattleHub
	mu         sync.Mutex
}

func (c *Client) SafeSend(msg []byte) {
	c.mu.Lock()
	defer c.mu.Unlock()
	select {
	case <-c.Done:
		return
	default:
	}
	select {
	case <-c.Done:
		return
	case c.Send <- msg:
	default:
		log.Printf("[WS] Send queue penuh player=%d room=%s", c.PlayerID, c.RoomCode)
	}
}

type BattleRoomWS struct {
	RoomCode  string
	Host      *Client
	Guest     *Client
	StartedAt time.Time
	mu        sync.Mutex
}

func (r *BattleRoomWS) Broadcast(msg []byte) {
	r.mu.Lock()
	host, guest := r.Host, r.Guest
	r.mu.Unlock()
	if host != nil {
		host.SafeSend(msg)
	}
	if guest != nil && guest != host {
		guest.SafeSend(msg)
	}
}

func (r *BattleRoomWS) IsFull() bool {
	r.mu.Lock()
	defer r.mu.Unlock()
	return r.Host != nil && r.Guest != nil
}

type BattleHub struct {
	Rooms      map[string]*BattleRoomWS
	Register   chan *ClientRegister
	Unregister chan *Client
	Message    chan *ClientMessage
	mu         sync.RWMutex
}

type ClientRegister struct {
	Client   *Client
	RoomCode string
	IsHost   bool
}

type ClientMessage struct {
	Client  *Client
	Message []byte
}

var Hub = &BattleHub{
	Rooms:      make(map[string]*BattleRoomWS),
	Register:   make(chan *ClientRegister, 100),
	Unregister: make(chan *Client, 100),
	Message:    make(chan *ClientMessage, 500),
}

func (h *BattleHub) Run() {
	log.Println("[WS] BattleHub berjalan")
	for {
		select {
		case reg := <-h.Register:
			h.mu.Lock()
			room := h.Rooms[reg.RoomCode]
			if room == nil {
				room = &BattleRoomWS{RoomCode: reg.RoomCode}
				h.Rooms[reg.RoomCode] = room
			}
			room.mu.Lock()
			if reg.IsHost {
				room.Host = reg.Client
			} else {
				room.Guest = reg.Client
			}
			isFull := room.Host != nil && room.Guest != nil
			if isFull {
				room.StartedAt = time.Now()
			}
			room.mu.Unlock()
			h.mu.Unlock()

			if isFull {
				room.Broadcast(buildMsg(MsgTypeStart, map[string]interface{}{"message": "Battle dimulai!"}))
			}

		case unreg := <-h.Unregister:
			var room *BattleRoomWS
			var opponent *Client
			wasActiveConnection := false

			h.mu.Lock()
			room = h.Rooms[unreg.RoomCode]
			if room != nil {
				room.mu.Lock()
				if room.Host == unreg {
					room.Host = nil
					opponent = room.Guest
					wasActiveConnection = true
				} else if room.Guest == unreg {
					room.Guest = nil
					opponent = room.Host
					wasActiveConnection = true
				}
				empty := room.Host == nil && room.Guest == nil
				room.mu.Unlock()
				if empty {
					delete(h.Rooms, unreg.RoomCode)
				}
			}
			h.mu.Unlock()

			// Hanya koneksi aktif yang boleh memicu forfeit. Koneksi lama
			// yang tertutup setelah reconnect tidak boleh mengalahkan pemain.
			if wasActiveConnection && unreg.RoomID > 0 {
				winnerID, _, status := battleService.DisconnectBattle(uint(unreg.RoomID), uint(unreg.PlayerID))
				if opponent != nil && status == 200 && winnerID > 0 {
					payload := map[string]interface{}{
						"player_id": unreg.PlayerID,
						"winner_id": winnerID,
						"message":   "Lawan terputus dari Battle. Kamu menang!",
					}
					opponent.SafeSend(buildMsg(MsgTypeDisconnect, payload))
				} else if opponent != nil && status >= 500 {
					opponent.SafeSend(buildMsg(MsgTypeError, map[string]string{
						"message": "Koneksi lawan terputus, tetapi finalisasi Battle perlu diperiksa.",
					}))
				}
			}

		case clientMsg := <-h.Message:
			h.handleMessage(clientMsg)
		}
	}
}

func (h *BattleHub) handleMessage(cm *ClientMessage) {
	if cm == nil || cm.Client == nil {
		return
	}
	var msg WSMessage
	if err := json.Unmarshal(cm.Message, &msg); err != nil {
		cm.Client.SafeSend(buildMsg(MsgTypeError, map[string]string{"message": "Format pesan tidak valid"}))
		return
	}

	h.mu.RLock()
	room := h.Rooms[cm.Client.RoomCode]
	active := room != nil && (room.Host == cm.Client || room.Guest == cm.Client)
	h.mu.RUnlock()
	if !active {
		cm.Client.SafeSend(buildMsg(MsgTypeError, map[string]string{"message": "Koneksi bukan anggota room aktif"}))
		return
	}

	switch msg.Type {
	case MsgTypePing:
		cm.Client.SafeSend(buildMsg(MsgTypePong, nil))
	case MsgTypeAnswer:
		// Nilai is_correct dari client tidak pernah dipercaya. Jawaban harus
		// dikirim melalui POST /battle/answer agar diverifikasi dari database.
		cm.Client.SafeSend(buildMsg(MsgTypeError, map[string]string{
			"message": "Kirim jawaban melalui endpoint REST /battle/answer",
		}))
	case MsgTypeFinish:
		// Finalisasi dan reward hanya boleh dilakukan oleh service database
		// melalui POST /battle/finish.
		cm.Client.SafeSend(buildMsg(MsgTypeError, map[string]string{
			"message": "Finalisasi Battle dilakukan melalui endpoint REST /battle/finish",
		}))
	default:
		cm.Client.SafeSend(buildMsg(MsgTypeError, map[string]string{"message": "Jenis pesan tidak dikenal"}))
	}
}

func buildMsg(msgType string, payload interface{}) []byte {
	p, _ := json.Marshal(payload)
	data, _ := json.Marshal(WSMessage{Type: msgType, Payload: p})
	return data
}
