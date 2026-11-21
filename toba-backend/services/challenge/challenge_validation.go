package challenge

import (
	"encoding/json"
	"errors"
	"fmt"
	"math/rand"
	"strings"
	"time"
	"unicode"

	"github.com/google/uuid"
)

const (
	GameMatching    = "MATCHING"
	GameGuessImage  = "GUESS_IMAGE"
	GameWordArrange = "WORD_ARRANGE"
	GameMemoryCard  = "MEMORY_CARD"
	GameOrder       = "ORDER"
	GameCategorize  = "CATEGORIZE"
)

var playableGameTypes = map[string]bool{
	GameMatching: true, GameGuessImage: true, GameWordArrange: true,
	GameMemoryCard: true, GameOrder: true, GameCategorize: true,
}

func NormalizeGameType(value string) string {
	return strings.ToUpper(strings.TrimSpace(value))
}

func IsPlayableGameType(value string) bool {
	return playableGameTypes[NormalizeGameType(value)]
}

type matchingPair struct {
	Batak string `json:"batak"`
	Indo  string `json:"indo"`
}
type matchingContent struct {
	Pairs []matchingPair `json:"pairs"`
}
type matchingPublic struct {
	BatakWords []string `json:"batak_words"`
	IndoWords  []string `json:"indo_words"`
}
type matchingAnswerKey struct {
	Pairs []matchingPair `json:"pairs"`
}
type matchingSubmission struct {
	Pairs []matchingPair `json:"pairs"`
}

type guessImageQuestion struct {
	Image   string   `json:"image"`
	Answer  string   `json:"answer"`
	Options []string `json:"options"`
}
type guessImageContent struct {
	Questions []guessImageQuestion `json:"questions"`
}
type guessImagePublicQuestion struct {
	Image   string   `json:"image"`
	Options []string `json:"options"`
}
type guessImagePublic struct {
	Questions []guessImagePublicQuestion `json:"questions"`
}
type guessImageAnswerKeyQuestion struct {
	Answer  string   `json:"answer"`
	Options []string `json:"options"`
}
type indexedStringAnswer struct {
	QuestionIndex  int    `json:"question_index"`
	Answer         string `json:"answer"`
	SelectedOption string `json:"selected_option"`
}
type indexedAnswersSubmission struct {
	Answers []indexedStringAnswer `json:"answers"`
}

type wordArrangeItem struct {
	Scrambled string `json:"scrambled"`
	Answer    string `json:"answer"`
	Hint      string `json:"hint"`
}
type wordArrangeContent struct {
	Words []wordArrangeItem `json:"words"`
}
type wordArrangePublicItem struct {
	Scrambled string `json:"scrambled"`
	Hint      string `json:"hint"`
}
type wordArrangePublic struct {
	Words []wordArrangePublicItem `json:"words"`
}

type memoryCardItem struct {
	ID    string `json:"id"`
	Label string `json:"label"`
	Emoji string `json:"emoji"`
}
type memoryCardContent struct {
	Cards []memoryCardItem `json:"cards"`
}
type memoryCardPublic struct {
	Cards []memoryCardItem `json:"cards"`
}
type memoryPairKey struct {
	FirstCardID  string `json:"first_card_id"`
	SecondCardID string `json:"second_card_id"`
}
type memoryCardAnswerKey struct {
	Pairs []memoryPairKey `json:"pairs"`
}
type memoryPairSubmission struct {
	FirstCardID  string `json:"first_card_id"`
	SecondCardID string `json:"second_card_id"`
}
type memorySubmission struct {
	Matches []memoryPairSubmission `json:"matches"`
}

type orderContent struct {
	Question     string   `json:"question"`
	CorrectOrder []string `json:"correct_order"`
}
type orderPublic struct {
	Question string   `json:"question"`
	Items    []string `json:"items"`
}
type orderSubmission struct {
	OrderedItems []string `json:"ordered_items"`
}

type categorizeCategory struct {
	Name  string   `json:"name"`
	Items []string `json:"items"`
}
type categorizeContent struct {
	Categories []categorizeCategory `json:"categories"`
}
type categorizePublicCategory struct {
	Name string `json:"name"`
}
type categorizePublic struct {
	Categories []categorizePublicCategory `json:"categories"`
	Items      []string                   `json:"items"`
}
type categorizeAssignment struct {
	Item     string `json:"item"`
	Category string `json:"category"`
}
type categorizeSubmission struct {
	Assignments []categorizeAssignment `json:"assignments"`
}

// ValidateChallengeContent validates the full administrator-authored payload.
// The full payload contains answer keys and must never be returned to players.
func ValidateChallengeContent(gameType, dataJSON string) error {
	gameType = NormalizeGameType(gameType)
	if !IsPlayableGameType(gameType) {
		return fmt.Errorf("tipe permainan %q belum tersedia; hanya enam tipe playable yang diizinkan", gameType)
	}
	if strings.TrimSpace(dataJSON) == "" {
		return errors.New("data_json wajib diisi")
	}

	switch gameType {
	case GameMatching:
		var data matchingContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON MATCHING tidak valid: %w", err)
		}
		if len(data.Pairs) < 2 || len(data.Pairs) > 8 {
			return errors.New("MATCHING harus memiliki 2 sampai 8 pasangan")
		}
		left, right := map[string]bool{}, map[string]bool{}
		for _, pair := range data.Pairs {
			l, r := normalizeText(pair.Batak), normalizeText(pair.Indo)
			if l == "" || r == "" {
				return errors.New("setiap pasangan MATCHING harus memiliki kata Batak dan arti")
			}
			if left[l] || right[r] {
				return errors.New("kata pada MATCHING tidak boleh memiliki pasangan duplikat/ambigu")
			}
			left[l], right[r] = true, true
		}
	case GameGuessImage:
		var data guessImageContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON GUESS_IMAGE tidak valid: %w", err)
		}
		if len(data.Questions) < 1 || len(data.Questions) > 30 {
			return errors.New("GUESS_IMAGE harus memiliki 1 sampai 30 pertanyaan")
		}
		for i, q := range data.Questions {
			if strings.TrimSpace(q.Image) == "" || strings.TrimSpace(q.Answer) == "" {
				return fmt.Errorf("GUESS_IMAGE pertanyaan %d membutuhkan image dan answer", i+1)
			}
			if len(q.Options) != 4 {
				return fmt.Errorf("GUESS_IMAGE pertanyaan %d harus memiliki tepat 4 opsi", i+1)
			}
			correct := 0
			seen := map[string]bool{}
			for _, option := range q.Options {
				n := normalizeText(option)
				if n == "" || seen[n] {
					return fmt.Errorf("GUESS_IMAGE pertanyaan %d memiliki opsi kosong/duplikat", i+1)
				}
				seen[n] = true
				if n == normalizeText(q.Answer) {
					correct++
				}
			}
			if correct != 1 {
				return fmt.Errorf("jawaban GUESS_IMAGE pertanyaan %d harus muncul tepat satu kali dalam opsi", i+1)
			}
		}
	case GameWordArrange:
		var data wordArrangeContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON WORD_ARRANGE tidak valid: %w", err)
		}
		if len(data.Words) < 1 || len(data.Words) > 30 {
			return errors.New("WORD_ARRANGE harus memiliki 1 sampai 30 kata")
		}
		for i, word := range data.Words {
			if strings.TrimSpace(word.Scrambled) == "" || strings.TrimSpace(word.Answer) == "" {
				return fmt.Errorf("WORD_ARRANGE item %d membutuhkan scrambled dan answer", i+1)
			}
			if !sameLetters(word.Scrambled, word.Answer) {
				return fmt.Errorf("huruf scrambled dan answer WORD_ARRANGE item %d tidak sama", i+1)
			}
		}
	case GameMemoryCard:
		var data memoryCardContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON MEMORY_CARD tidak valid: %w", err)
		}
		if len(data.Cards) < 2 || len(data.Cards) > 8 {
			return errors.New("MEMORY_CARD harus memiliki 2 sampai 8 kartu pasangan")
		}
		seenIDs, seenLabels := map[string]bool{}, map[string]bool{}
		for _, card := range data.Cards {
			id := normalizeText(card.ID)
			label := normalizeText(card.Label)
			if id == "" || label == "" {
				return errors.New("setiap kartu MEMORY_CARD membutuhkan id dan label")
			}
			if seenIDs[id] {
				return errors.New("ID kartu MEMORY_CARD harus unik")
			}
			if seenLabels[label] {
				return errors.New("label pasangan MEMORY_CARD harus unik")
			}
			seenIDs[id] = true
			seenLabels[label] = true
		}
	case GameOrder:
		var data orderContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON ORDER tidak valid: %w", err)
		}
		if strings.TrimSpace(data.Question) == "" || len(data.CorrectOrder) < 2 || len(data.CorrectOrder) > 6 {
			return errors.New("ORDER membutuhkan question dan 2 sampai 6 item correct_order")
		}
		seen := map[string]bool{}
		for _, item := range data.CorrectOrder {
			n := normalizeText(item)
			if n == "" || seen[n] {
				return errors.New("item ORDER tidak boleh kosong atau duplikat")
			}
			seen[n] = true
		}
	case GameCategorize:
		var data categorizeContent
		if err := json.Unmarshal([]byte(dataJSON), &data); err != nil {
			return fmt.Errorf("JSON CATEGORIZE tidak valid: %w", err)
		}
		if len(data.Categories) < 2 || len(data.Categories) > 4 {
			return errors.New("CATEGORIZE harus memiliki 2 sampai 4 kategori")
		}
		categorySeen, itemSeen := map[string]bool{}, map[string]bool{}
		itemCount := 0
		for _, category := range data.Categories {
			name := normalizeText(category.Name)
			if name == "" || categorySeen[name] {
				return errors.New("nama kategori CATEGORIZE kosong atau duplikat")
			}
			categorySeen[name] = true
			if len(category.Items) == 0 {
				return fmt.Errorf("kategori %q belum memiliki item", category.Name)
			}
			for _, item := range category.Items {
				n := normalizeText(item)
				if n == "" || itemSeen[n] {
					return errors.New("item CATEGORIZE kosong atau muncul di lebih dari satu kategori")
				}
				itemSeen[n] = true
				itemCount++
			}
		}
		if itemCount < 4 || itemCount > 6 {
			return errors.New("CATEGORIZE harus memiliki 4 sampai 6 item agar sesuai layout permainan")
		}
	}
	return nil
}

// BuildChallengeSnapshot separates public playable content from the private answer key.
func BuildChallengeSnapshot(gameType, dataJSON string) (string, string, error) {
	gameType = NormalizeGameType(gameType)
	if err := ValidateChallengeContent(gameType, dataJSON); err != nil {
		return "", "", err
	}

	rng := rand.New(rand.NewSource(time.Now().UnixNano()))
	marshal := func(v interface{}) (string, error) { b, err := json.Marshal(v); return string(b), err }

	switch gameType {
	case GameMatching:
		var data matchingContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		public := matchingPublic{BatakWords: make([]string, 0, len(data.Pairs)), IndoWords: make([]string, 0, len(data.Pairs))}
		for _, p := range data.Pairs {
			public.BatakWords = append(public.BatakWords, p.Batak)
			public.IndoWords = append(public.IndoWords, p.Indo)
		}
		rng.Shuffle(len(public.BatakWords), func(i, j int) {
			public.BatakWords[i], public.BatakWords[j] = public.BatakWords[j], public.BatakWords[i]
		})
		rng.Shuffle(len(public.IndoWords), func(i, j int) { public.IndoWords[i], public.IndoWords[j] = public.IndoWords[j], public.IndoWords[i] })
		key := matchingAnswerKey{Pairs: data.Pairs}
		pubJSON, err := marshal(public)
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(key)
		return pubJSON, keyJSON, err
	case GameGuessImage:
		var data guessImageContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		public := guessImagePublic{Questions: make([]guessImagePublicQuestion, 0, len(data.Questions))}
		key := make([]guessImageAnswerKeyQuestion, 0, len(data.Questions))
		for _, q := range data.Questions {
			options := append([]string(nil), q.Options...)
			rng.Shuffle(len(options), func(i, j int) { options[i], options[j] = options[j], options[i] })
			public.Questions = append(public.Questions, guessImagePublicQuestion{Image: q.Image, Options: options})
			key = append(key, guessImageAnswerKeyQuestion{Answer: q.Answer, Options: append([]string(nil), q.Options...)})
		}
		pubJSON, err := marshal(public)
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(key)
		return pubJSON, keyJSON, err
	case GameWordArrange:
		var data wordArrangeContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		public := wordArrangePublic{Words: make([]wordArrangePublicItem, 0, len(data.Words))}
		key := make([]string, 0, len(data.Words))
		for _, word := range data.Words {
			public.Words = append(public.Words, wordArrangePublicItem{Scrambled: word.Scrambled, Hint: word.Hint})
			key = append(key, word.Answer)
		}
		pubJSON, err := marshal(public)
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(key)
		return pubJSON, keyJSON, err
	case GameMemoryCard:
		var data memoryCardContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		public := memoryCardPublic{Cards: make([]memoryCardItem, 0, len(data.Cards)*2)}
		key := memoryCardAnswerKey{Pairs: make([]memoryPairKey, 0, len(data.Cards))}
		for _, card := range data.Cards {
			firstID, secondID := uuid.NewString(), uuid.NewString()
			// The public deck contains individual cards only. Pair identifiers stay private.
			public.Cards = append(public.Cards, memoryCardItem{ID: firstID, Label: card.Label, Emoji: card.Emoji}, memoryCardItem{ID: secondID, Label: card.Label, Emoji: card.Emoji})
			key.Pairs = append(key.Pairs, memoryPairKey{FirstCardID: firstID, SecondCardID: secondID})
		}
		rng.Shuffle(len(public.Cards), func(i, j int) { public.Cards[i], public.Cards[j] = public.Cards[j], public.Cards[i] })
		pubJSON, err := marshal(public)
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(key)
		return pubJSON, keyJSON, err
	case GameOrder:
		var data orderContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		items := append([]string(nil), data.CorrectOrder...)
		rng.Shuffle(len(items), func(i, j int) { items[i], items[j] = items[j], items[i] })
		pubJSON, err := marshal(orderPublic{Question: data.Question, Items: items})
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(data.CorrectOrder)
		return pubJSON, keyJSON, err
	case GameCategorize:
		var data categorizeContent
		_ = json.Unmarshal([]byte(dataJSON), &data)
		public := categorizePublic{Categories: make([]categorizePublicCategory, 0, len(data.Categories)), Items: make([]string, 0)}
		key := make(map[string]string)
		for _, category := range data.Categories {
			public.Categories = append(public.Categories, categorizePublicCategory{Name: category.Name})
			for _, item := range category.Items {
				public.Items = append(public.Items, item)
				key[item] = category.Name
			}
		}
		rng.Shuffle(len(public.Items), func(i, j int) { public.Items[i], public.Items[j] = public.Items[j], public.Items[i] })
		pubJSON, err := marshal(public)
		if err != nil {
			return "", "", err
		}
		keyJSON, err := marshal(key)
		return pubJSON, keyJSON, err
	default:
		return "", "", fmt.Errorf("game type %q belum didukung", gameType)
	}
}

// ScoreChallengeAnswers validates every submitted interaction against the private session key.
func ScoreChallengeAnswers(gameType, answerKeyJSON, answersJSON string) (int, int, error) {
	if strings.TrimSpace(answersJSON) == "" {
		return 0, 0, errors.New("jawaban belum dikirim")
	}
	gameType = NormalizeGameType(gameType)
	switch gameType {
	case GameMatching:
		var key matchingAnswerKey
		var submitted matchingSubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban MATCHING tidak valid")
		}
		if len(submitted.Pairs) != len(key.Pairs) {
			return 0, 0, errors.New("semua pasangan MATCHING harus dijawab")
		}
		correct := map[string]string{}
		allowedRight := map[string]bool{}
		for _, p := range key.Pairs {
			correct[normalizeText(p.Batak)] = normalizeText(p.Indo)
			allowedRight[normalizeText(p.Indo)] = true
		}
		seenLeft, seenRight := map[string]bool{}, map[string]bool{}
		score := 0
		for _, p := range submitted.Pairs {
			l, r := normalizeText(p.Batak), normalizeText(p.Indo)
			if l == "" || r == "" || correct[l] == "" || !allowedRight[r] || seenLeft[l] || seenRight[r] {
				return 0, 0, errors.New("pasangan MATCHING tidak valid atau duplikat")
			}
			seenLeft[l], seenRight[r] = true, true
			if correct[l] == r {
				score++
			}
		}
		return score, len(key.Pairs), nil
	case GameGuessImage:
		var key []guessImageAnswerKeyQuestion
		var submitted indexedAnswersSubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban GUESS_IMAGE tidak valid")
		}
		if len(submitted.Answers) != len(key) {
			return 0, 0, errors.New("semua pertanyaan GUESS_IMAGE harus dijawab")
		}
		seen := map[int]bool{}
		score := 0
		for _, a := range submitted.Answers {
			if a.QuestionIndex < 0 || a.QuestionIndex >= len(key) || seen[a.QuestionIndex] || strings.TrimSpace(a.SelectedOption) == "" {
				return 0, 0, errors.New("jawaban GUESS_IMAGE tidak valid atau duplikat")
			}
			seen[a.QuestionIndex] = true
			allowed := false
			for _, option := range key[a.QuestionIndex].Options {
				if normalizeText(option) == normalizeText(a.SelectedOption) {
					allowed = true
					break
				}
			}
			if !allowed {
				return 0, 0, errors.New("opsi GUESS_IMAGE tidak dikenal")
			}
			if normalizeText(a.SelectedOption) == normalizeText(key[a.QuestionIndex].Answer) {
				score++
			}
		}
		return score, len(key), nil
	case GameWordArrange:
		var key []string
		var submitted indexedAnswersSubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban WORD_ARRANGE tidak valid")
		}
		if len(submitted.Answers) != len(key) {
			return 0, 0, errors.New("semua kata WORD_ARRANGE harus dijawab")
		}
		seen := map[int]bool{}
		score := 0
		for _, a := range submitted.Answers {
			if a.QuestionIndex < 0 || a.QuestionIndex >= len(key) || seen[a.QuestionIndex] || strings.TrimSpace(a.Answer) == "" {
				return 0, 0, errors.New("jawaban WORD_ARRANGE tidak valid atau duplikat")
			}
			seen[a.QuestionIndex] = true
			if normalizeWord(a.Answer) == normalizeWord(key[a.QuestionIndex]) {
				score++
			}
		}
		return score, len(key), nil
	case GameMemoryCard:
		var key memoryCardAnswerKey
		var submitted memorySubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban MEMORY_CARD tidak valid")
		}
		if len(submitted.Matches) != len(key.Pairs) {
			return 0, 0, errors.New("semua pasangan MEMORY_CARD harus diselesaikan")
		}
		validPairs := map[string]bool{}
		validCards := map[string]bool{}
		for _, p := range key.Pairs {
			validPairs[pairKey(p.FirstCardID, p.SecondCardID)] = true
			validCards[p.FirstCardID] = true
			validCards[p.SecondCardID] = true
		}
		used := map[string]bool{}
		score := 0
		for _, p := range submitted.Matches {
			a, b := p.FirstCardID, p.SecondCardID
			if a == "" || b == "" || a == b || !validCards[a] || !validCards[b] || used[a] || used[b] {
				return 0, 0, errors.New("pasangan MEMORY_CARD tidak valid atau kartu digunakan lebih dari sekali")
			}
			used[a], used[b] = true, true
			if validPairs[pairKey(a, b)] {
				score++
			}
		}
		return score, len(key.Pairs), nil
	case GameOrder:
		var key []string
		var submitted orderSubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban ORDER tidak valid")
		}
		if len(submitted.OrderedItems) != len(key) {
			return 0, 0, errors.New("semua item ORDER harus dikirim")
		}
		allowed, seen := map[string]bool{}, map[string]bool{}
		for _, item := range key {
			allowed[normalizeText(item)] = true
		}
		for _, item := range submitted.OrderedItems {
			n := normalizeText(item)
			if !allowed[n] || seen[n] {
				return 0, 0, errors.New("item ORDER tidak valid atau duplikat")
			}
			seen[n] = true
		}
		for i := range key {
			if normalizeText(key[i]) != normalizeText(submitted.OrderedItems[i]) {
				return 0, 1, nil
			}
		}
		return 1, 1, nil
	case GameCategorize:
		var key map[string]string
		var submitted categorizeSubmission
		if err := json.Unmarshal([]byte(answerKeyJSON), &key); err != nil {
			return 0, 0, err
		}
		if err := json.Unmarshal([]byte(answersJSON), &submitted); err != nil {
			return 0, 0, errors.New("format jawaban CATEGORIZE tidak valid")
		}
		if len(submitted.Assignments) != len(key) {
			return 0, 0, errors.New("semua item CATEGORIZE harus ditempatkan")
		}
		correct := map[string]string{}
		allowedCategories := map[string]bool{}
		for item, category := range key {
			correct[normalizeText(item)] = normalizeText(category)
			allowedCategories[normalizeText(category)] = true
		}
		seen := map[string]bool{}
		score := 0
		for _, a := range submitted.Assignments {
			item, category := normalizeText(a.Item), normalizeText(a.Category)
			if item == "" || category == "" || correct[item] == "" || !allowedCategories[category] || seen[item] {
				return 0, 0, errors.New("assignment CATEGORIZE tidak valid atau duplikat")
			}
			seen[item] = true
			if correct[item] == category {
				score++
			}
		}
		return score, len(key), nil
	default:
		return 0, 0, fmt.Errorf("game type %q belum didukung", gameType)
	}
}

func normalizeText(value string) string {
	return strings.ToUpper(strings.Join(strings.Fields(strings.TrimSpace(value)), " "))
}

func normalizeWord(value string) string {
	var b strings.Builder
	for _, r := range strings.ToUpper(strings.TrimSpace(value)) {
		if !unicode.IsSpace(r) {
			b.WriteRune(r)
		}
	}
	return b.String()
}

func sameLetters(a, b string) bool {
	counts := map[rune]int{}
	for _, r := range normalizeWord(a) {
		counts[r]++
	}
	for _, r := range normalizeWord(b) {
		counts[r]--
	}
	for _, count := range counts {
		if count != 0 {
			return false
		}
	}
	return true
}

func pairKey(a, b string) string {
	if a > b {
		a, b = b, a
	}
	return a + "\x1f" + b
}
