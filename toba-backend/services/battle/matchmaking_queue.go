package battle

import (
	"sync"
	"time"
)

type MatchEntry struct {
	PlayerID   uint
	PlayerName string
	Level      int
	EnteredAt  time.Time
}

var (
	mmQueue []MatchEntry
	mmMu    sync.Mutex
)

func AddMatchmakingEntry(entry MatchEntry) {
	mmMu.Lock()
	defer mmMu.Unlock()

	mmQueue = append(mmQueue, entry)
}

func RemoveMatchmakingEntry(playerID uint) {
	mmMu.Lock()
	defer mmMu.Unlock()

	for i, entry := range mmQueue {
		if entry.PlayerID == playerID {
			mmQueue = append(mmQueue[:i], mmQueue[i+1:]...)
			return
		}
	}
}

func FindMatchmakingEntry(playerID uint) *MatchEntry {
	mmMu.Lock()
	defer mmMu.Unlock()

	for i := range mmQueue {
		if mmQueue[i].PlayerID == playerID {
			entry := mmQueue[i]
			return &entry
		}
	}

	return nil
}

func PopMatchmakingOpponent(playerID uint, playerLevel int) *MatchEntry {
	mmMu.Lock()
	defer mmMu.Unlock()

	bestIndex := -1
	bestDifference := int(^uint(0) >> 1)

	for i, entry := range mmQueue {
		if entry.PlayerID == playerID {
			continue
		}

		difference := absInt(entry.Level - playerLevel)

		if difference <= 3 && difference < bestDifference {
			bestIndex = i
			bestDifference = difference
		}
	}

	if bestIndex == -1 {
		return nil
	}

	opponent := mmQueue[bestIndex]
	mmQueue = append(mmQueue[:bestIndex], mmQueue[bestIndex+1:]...)

	return &opponent
}

func CleanMatchmakingQueue() {
	mmMu.Lock()
	defer mmMu.Unlock()

	now := time.Now()

	filtered := make([]MatchEntry, 0, len(mmQueue))

	for _, entry := range mmQueue {
		if now.Sub(entry.EnteredAt) < 5*time.Minute {
			filtered = append(filtered, entry)
		}
	}

	mmQueue = filtered
}
