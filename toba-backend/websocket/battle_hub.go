package websocket

import (
	"encoding/json"
	"log"
	"sync"
	"time"
)

// ============================================================
// MESSAGE TYPES
// ============================================================
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
)

// ============================================================
// STRUCTS
// ============================================================
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

type AnswerPayload struct {
	PlayerID         int  `json:"player_id"`
	QuestionID       int  `json:"question_id"`
	SelectedOptionID int  `json:"selected_option_id"`
	IsCorrect        bool `json:"is_correct"`
}

type ScorePayload struct {
	HostScore  int `json:"host_score"`
	GuestScore int `json:"guest_score"`
}

type FinishPayload struct {
	HostScore  int    `json:"host_score"`
	GuestScore int    `json:"guest_score"`
	WinnerID   int    `json:"winner_id"`
	WinnerName string `json:"winner_name"`
}

// ============================================================
// CLIENT
// ============================================================
type Client struct {
	PlayerID   int
	PlayerName string
	IsHost     bool
	RoomCode   string
	Send       chan []byte
	Hub        *BattleHub
	mu         sync.Mutex
}

func (c *Client) SafeSend(msg []byte) {
	c.mu.Lock()
	defer c.mu.Unlock()
	select {
	case c.Send <- msg:
	default:
		// Channel penuh, skip
	}
}

// ============================================================
// ROOM
// ============================================================
type BattleRoomWS struct {
	RoomCode   string
	Host       *Client
	Guest      *Client
	HostScore  int
	GuestScore int
	mu         sync.Mutex
	StartedAt  time.Time
}

func (r *BattleRoomWS) Broadcast(msg []byte) {
	r.mu.Lock()
	defer r.mu.Unlock()
	if r.Host != nil {
		r.Host.SafeSend(msg)
	}
	if r.Guest != nil {
		r.Guest.SafeSend(msg)
	}
}

func (r *BattleRoomWS) SendToOther(senderID int, msg []byte) {
	r.mu.Lock()
	defer r.mu.Unlock()
	if r.Host != nil && r.Host.PlayerID != senderID {
		r.Host.SafeSend(msg)
	}
	if r.Guest != nil && r.Guest.PlayerID != senderID {
		r.Guest.SafeSend(msg)
	}
}

func (r *BattleRoomWS) IsFull() bool {
	return r.Host != nil && r.Guest != nil
}

// ============================================================
// HUB
// ============================================================
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
	log.Println("🔌 WebSocket BattleHub berjalan...")
	for {
		select {

		case reg := <-h.Register:
			h.mu.Lock()
			room, exists := h.Rooms[reg.RoomCode]
			if !exists {
				room = &BattleRoomWS{RoomCode: reg.RoomCode}
				h.Rooms[reg.RoomCode] = room
			}
			if reg.IsHost {
				room.Host = reg.Client
			} else {
				room.Guest = reg.Client
				// Kedua player sudah join → broadcast START
				if room.IsFull() {
					h.mu.Unlock()
					room.StartedAt = time.Now()
					startMsg := buildMsg(MsgTypeStart, map[string]interface{}{
						"message": "Battle dimulai!",
					})
					room.Broadcast(startMsg)
					continue
				}
			}
			h.mu.Unlock()

		case unreg := <-h.Unregister:
			h.mu.Lock()
			room, exists := h.Rooms[unreg.RoomCode]
			if exists {
				var winnerID int
				var winnerName string

				if room.Host != nil && room.Host.PlayerID == unreg.PlayerID {
					// Host disconnect → guest menang
					room.Host = nil
					if room.Guest != nil {
						winnerID = room.Guest.PlayerID
						winnerName = room.Guest.PlayerName
					}
				} else if room.Guest != nil && room.Guest.PlayerID == unreg.PlayerID {
					// Guest disconnect → host menang
					room.Guest = nil
					if room.Host != nil {
						winnerID = room.Host.PlayerID
						winnerName = room.Host.PlayerName
					}
				}

				if winnerID != 0 {
					finishMsg := buildMsg(MsgTypeDisconnect, map[string]interface{}{
						"winner_id":   winnerID,
						"winner_name": winnerName,
						"message":     "Lawan disconnect. Kamu menang!",
					})
					room.Broadcast(finishMsg)
				}

				// Cleanup room jika sudah kosong
				if room.Host == nil && room.Guest == nil {
					delete(h.Rooms, unreg.RoomCode)
				}
			}
			h.mu.Unlock()

		case clientMsg := <-h.Message:
			h.handleMessage(clientMsg)
		}
	}
}

func (h *BattleHub) handleMessage(cm *ClientMessage) {
	var msg WSMessage
	if err := json.Unmarshal(cm.Message, &msg); err != nil {
		log.Printf("WS parse error: %v", err)
		return
	}

	h.mu.RLock()
	room, exists := h.Rooms[cm.Client.RoomCode]
	h.mu.RUnlock()

	if !exists {
		return
	}

	switch msg.Type {

	case MsgTypePing:
		pong := buildMsg(MsgTypePong, nil)
		cm.Client.SafeSend(pong)

	case MsgTypeAnswer:
		var payload AnswerPayload
		if err := json.Unmarshal(msg.Payload, &payload); err != nil {
			return
		}

		room.mu.Lock()
		if payload.IsCorrect {
			if cm.Client.IsHost {
				room.HostScore++
			} else {
				room.GuestScore++
			}
		}
		hostScore := room.HostScore
		guestScore := room.GuestScore
		room.mu.Unlock()

		// Broadcast update skor ke semua
		scoreMsg := buildMsg(MsgTypeScoreUpdate, ScorePayload{
			HostScore:  hostScore,
			GuestScore: guestScore,
		})
		room.Broadcast(scoreMsg)

	case MsgTypeFinish:
		room.mu.Lock()
		hostScore := room.HostScore
		guestScore := room.GuestScore
		room.mu.Unlock()

		var winnerID int
		var winnerName string

		if hostScore > guestScore && room.Host != nil {
			winnerID = room.Host.PlayerID
			winnerName = room.Host.PlayerName
		} else if guestScore > hostScore && room.Guest != nil {
			winnerID = room.Guest.PlayerID
			winnerName = room.Guest.PlayerName
		}

		finishMsg := buildMsg(MsgTypeFinish, FinishPayload{
			HostScore:  hostScore,
			GuestScore: guestScore,
			WinnerID:   winnerID,
			WinnerName: winnerName,
		})
		room.Broadcast(finishMsg)

		// Hapus room
		h.mu.Lock()
		delete(h.Rooms, cm.Client.RoomCode)
		h.mu.Unlock()
	}
}

func buildMsg(msgType string, payload interface{}) []byte {
	p, _ := json.Marshal(payload)
	msg := WSMessage{Type: msgType, Payload: p}
	data, _ := json.Marshal(msg)
	return data
}
