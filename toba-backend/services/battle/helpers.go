package battle

import (
	"math/rand"
	"strings"
	"time"
)

func GenerateRoomCode() string {
	const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

	r := rand.New(rand.NewSource(time.Now().UnixNano()))

	var builder strings.Builder

	for i := 0; i < 6; i++ {
		builder.WriteByte(chars[r.Intn(len(chars))])
	}

	return builder.String()
}

func absInt(value int) int {
	if value < 0 {
		return -value
	}

	return value
}
