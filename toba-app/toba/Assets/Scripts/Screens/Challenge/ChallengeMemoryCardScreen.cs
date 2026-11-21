using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MemoryCardItem
{
    public string id;
    public string label;
    public string emoji;
}

[System.Serializable]
public class MemoryCardData
{
    public List<MemoryCardItem> cards;
}

public class ChallengeMemoryCardScreen : BaseScreen
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    private const int GRID_COLUMNS = 4;
    private const int MAX_PAIRS = 8;

    private const float CARD_WIDTH = 195f;
    private const float CARD_HEIGHT = 220f;
    private const float GRID_GAP_X = 15f;
    private const float GRID_GAP_Y = 15f;
    private const float GRID_START_Y = 350f;

    private const float MATCH_CHECK_DELAY = 0.8f;

    // ============================================================
    // DATA
    // ============================================================

    private ChallengeData challenge;

    // cardIds berisi token unik per kartu untuk validasi server.
    // cardValues berisi label pasangan untuk umpan balik visual lokal.
    private readonly List<string> cardValues = new List<string>();
    private readonly List<string> cardEmojis = new List<string>();
    private readonly List<string> cardIds = new List<string>();
    private readonly List<ChallengeMemoryCardMatch> submittedMatches =
        new List<ChallengeMemoryCardMatch>();

    private readonly List<GameObject> cards = new List<GameObject>();
    private readonly List<bool> revealed = new List<bool>();
    private readonly List<bool> matched = new List<bool>();

    // ============================================================
    // GAME STATE
    // ============================================================

    private int firstIdx = -1;
    private int secondIdx = -1;

    private bool isChecking = false;
    private bool hasSubmitted = false;

    private int matchedPairs = 0;
    private int totalPairs = 0;

    // ============================================================
    // UI REFERENCES
    // ============================================================

    private TextMeshProUGUI pairsTxt;
    private Sprite cardBackSprite;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public ChallengeMemoryCardScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeMemoryCard")
    {
        challenge = ch;
    }

    // ============================================================
    // SHOW
    // ============================================================

    public override void Show()
    {
        if (screenObj == null)
            return;

        ResetGameState();

        SetBackground(TOBATheme.BGDark);

        UIBuilder.CreateBackground(
            screenObj.transform,
            TOBATheme.LoadSprite("bg_toba")
        );

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0.04f, 0f, 0.5f),
            Vector2.zero,
            Vector2.one
        );

        HeaderBar.Create(
            screenObj.transform,
            "MEMORY CARD BUDAYA",
            () =>
            {
                if (ScreenManager.Instance != null)
                {
                    ScreenManager.Instance.ChangeScreen(
                        new MainMenuScreen(container)
                    );
                }
            }
        );

        if (!LoadAndPrepareCards())
            return;

        CreateGameHeader();
        CreateCardGrid();
    }

    // ============================================================
    // RESET STATE
    // ============================================================

    private void ResetGameState()
    {
        cardValues.Clear();
        cardEmojis.Clear();
        cardIds.Clear();
        submittedMatches.Clear();

        cards.Clear();
        revealed.Clear();
        matched.Clear();

        firstIdx = -1;
        secondIdx = -1;

        isChecking = false;
        hasSubmitted = false;

        matchedPairs = 0;
        totalPairs = 0;

        pairsTxt = null;
        cardBackSprite = null;
    }

    // ============================================================
    // LOAD AND VALIDATE JSON
    // ============================================================

    private bool LoadAndPrepareCards()
    {
        MemoryCardData data = null;

        try
        {
            if (
                challenge == null ||
                string.IsNullOrWhiteSpace(challenge.data_json)
            )
            {
                throw new System.FormatException(
                    "Data challenge kosong."
                );
            }

            data = JsonUtility.FromJson<MemoryCardData>(
                challenge.data_json
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "[MemoryCard] JSON tidak valid: " + ex.Message
            );

            ShowDataError(
                "Data tidak tersedia",
                "Data permainan tidak dapat dibaca. Hubungi admin."
            );

            return false;
        }

        if (data == null || data.cards == null)
        {
            ShowDataError(
                "Data tidak tersedia",
                "Data kartu belum tersedia. Hubungi admin."
            );

            return false;
        }

        // Server mengirim kartu individual dengan ID unik. Relasi pasangan
        // tetap tersimpan privat di server; client hanya menerima wajah kartu.
        HashSet<string> usedIds = new HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase);
        List<MemoryCardItem> validCards = new List<MemoryCardItem>();

        foreach (MemoryCardItem card in data.cards)
        {
            if (card == null ||
                string.IsNullOrWhiteSpace(card.id) ||
                string.IsNullOrWhiteSpace(card.label))
                continue;

            card.id = card.id.Trim();
            card.label = card.label.Trim();
            if (!usedIds.Add(card.id))
            {
                ShowDataError("ID kartu duplikat", "Hubungi admin.");
                return false;
            }
            if (!string.IsNullOrWhiteSpace(card.emoji))
                card.emoji = card.emoji.Trim();
            validCards.Add(card);
        }

        if (validCards.Count < 4 ||
            validCards.Count > MAX_PAIRS * 2 ||
            validCards.Count % 2 != 0)
        {
            ShowDataError(
                "Jumlah kartu tidak valid",
                "Permainan membutuhkan 2 sampai 8 pasangan kartu."
            );
            return false;
        }

        totalPairs = validCards.Count / 2;
        foreach (MemoryCardItem card in validCards)
        {
            cardIds.Add(card.id);
            cardValues.Add(card.label);
            cardEmojis.Add(string.IsNullOrWhiteSpace(card.emoji)
                ? card.label
                : card.emoji);
        }

        ShuffleCards();

        return true;
    }

    // ============================================================
    // DATA ERROR
    // ============================================================

    private void ShowDataError(
        string title,
        string message)
    {
        if (screenObj == null)
            return;

        EmptyState.Show(
            screenObj.transform,
            "🃏",
            title,
            message
        );
    }

    // ============================================================
    // SHUFFLE
    // ============================================================

    private void ShuffleCards()
    {
        for (int i = cardValues.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            string tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;

            string tempEmoji = cardEmojis[i];
            cardEmojis[i] = cardEmojis[j];
            cardEmojis[j] = tempEmoji;

            string tempId = cardIds[i];
            cardIds[i] = cardIds[j];
            cardIds[j] = tempId;
        }
    }

    // ============================================================
    // GAME HEADER
    // ============================================================

    private void CreateGameHeader()
    {
        if (screenObj == null)
            return;

        GameObject pairsObject =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Pasangan: 0/" + totalPairs,
                34,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(500f, 50f),
                new Vector2(0f, 760f)
            );

        if (pairsObject != null)
        {
            pairsTxt =
                pairsObject.GetComponent<TextMeshProUGUI>();

            if (pairsTxt != null)
            {
                pairsTxt.fontStyle = FontStyles.Normal;
                pairsTxt.raycastTarget = false;
            }
        }

        GameObject instructionObject =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Temukan semua pasangan gambar budaya!",
                28,
                new Color(1f, 1f, 1f, 0.7f),
                TextAlignmentOptions.Center,
                new Vector2(800f, 45f),
                new Vector2(0f, 700f)
            );

        if (instructionObject != null)
        {
            TextMeshProUGUI instructionText =
                instructionObject.GetComponent<TextMeshProUGUI>();

            if (instructionText != null)
            {
                instructionText.fontStyle = FontStyles.Normal;
                instructionText.raycastTarget = false;
            }
        }
    }

    // ============================================================
    // CARD GRID
    // ============================================================

    private void CreateCardGrid()
    {
        if (
            screenObj == null ||
            cardValues.Count == 0 ||
            cardValues.Count != cardEmojis.Count
        )
        {
            return;
        }

        int totalCards = cardValues.Count;

        int cols = Mathf.Min(
            GRID_COLUMNS,
            totalCards
        );

        cardBackSprite =
            TOBATheme.LoadSprite("panel_wood");

        for (int i = 0; i < totalCards; i++)
        {
            revealed.Add(false);
            matched.Add(false);

            int row = i / cols;
            int col = i % cols;

            // Pusatkan setiap baris, termasuk baris terakhir
            // yang jumlah kartunya kurang dari empat.
            int cardsInThisRow = Mathf.Min(
                cols,
                totalCards - row * cols
            );

            float rowWidth =
                cardsInThisRow * CARD_WIDTH +
                (cardsInThisRow - 1) * GRID_GAP_X;

            float startX = -rowWidth / 2f;

            float x =
                startX +
                col * (CARD_WIDTH + GRID_GAP_X) +
                CARD_WIDTH / 2f;

            float y =
                GRID_START_Y -
                row * (CARD_HEIGHT + GRID_GAP_Y);

            GameObject card = CreateCard(
                i,
                cardEmojis[i],
                x,
                y,
                CARD_WIDTH,
                CARD_HEIGHT
            );

            if (card != null)
                cards.Add(card);
        }

        if (cards.Count != totalCards)
        {
            Debug.LogError(
                "[MemoryCard] Jumlah kartu yang berhasil dibuat tidak sesuai."
            );
        }
    }

    // ============================================================
    // CREATE CARD
    // ============================================================

    private GameObject CreateCard(
        int idx,
        string label,
        float x,
        float y,
        float w,
        float h)
    {
        if (screenObj == null)
            return null;

        GameObject card = new GameObject(
            "Card_" + idx
        );

        card.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rt =
            card.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(0.5f, 0.5f);

        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);

        // SISI DEPAN KARTU YANG TERTUTUP
        Image backImg = card.AddComponent<Image>();

        backImg.sprite =
            cardBackSprite != null
                ? cardBackSprite
                : UIBuilder.GetDefaultSprite();

        backImg.type =
            cardBackSprite != null
                ? Image.Type.Sliced
                : Image.Type.Simple;

        backImg.color =
            new Color(0.3f, 0.15f, 0.05f, 1f);

        backImg.raycastTarget = true;

        GameObject questionObject =
            UIBuilder.CreateTextSized(
                card.transform,
                "?",
                72,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(w, h),
                Vector2.zero
            );

        if (questionObject != null)
        {
            TextMeshProUGUI questionMark =
                questionObject.GetComponent<TextMeshProUGUI>();

            if (questionMark != null)
            {
                questionMark.fontStyle = FontStyles.Bold;
                questionMark.name = "QuestionMark";
                questionMark.raycastTarget = false;
            }
        }

        // SISI BELAKANG KARTU YANG MENAMPILKAN KONTEN
        GameObject frontGO = new GameObject("Front");

        frontGO.transform.SetParent(
            card.transform,
            false
        );

        RectTransform rtF =
            frontGO.AddComponent<RectTransform>();

        rtF.anchorMin = Vector2.zero;
        rtF.anchorMax = Vector2.one;
        rtF.offsetMin = Vector2.zero;
        rtF.offsetMax = Vector2.zero;

        Image frontImg =
            frontGO.AddComponent<Image>();

        frontImg.sprite = UIBuilder.GetDefaultSprite();

        frontImg.color =
            new Color(0.92f, 0.82f, 0.65f, 1f);

        frontImg.raycastTarget = false;

        GameObject frontTextObject =
            UIBuilder.CreateTextSized(
                frontGO.transform,
                label,
                36,
                TOBATheme.TextDark,
                TextAlignmentOptions.Center,
                new Vector2(w - 20f, h - 20f),
                Vector2.zero
            );

        if (frontTextObject != null)
        {
            TextMeshProUGUI frontTxt =
                frontTextObject.GetComponent<TextMeshProUGUI>();

            if (frontTxt != null)
            {
                frontTxt.fontStyle = FontStyles.Bold;
                frontTxt.textWrappingMode = TextWrappingModes.Normal;
                frontTxt.enableAutoSizing = true;
                frontTxt.fontSizeMin = 22f;
                frontTxt.fontSizeMax = 36f;
                frontTxt.raycastTarget = false;
            }
        }

        // Kartu dimulai dalam keadaan tertutup.
        frontGO.SetActive(false);

        Button btn = card.AddComponent<Button>();

        btn.targetGraphic = backImg;
        btn.transition = Selectable.Transition.None;

        int capturedIdx = idx;

        btn.onClick.AddListener(
            () => OnCardClick(capturedIdx)
        );

        return card;
    }

    // ============================================================
    // CARD CLICK
    // ============================================================

    private void OnCardClick(int idx)
    {
        if (
            screenObj == null ||
            isChecking ||
            hasSubmitted ||
            idx < 0 ||
            idx >= revealed.Count ||
            idx >= matched.Count ||
            idx >= cards.Count ||
            idx >= cardValues.Count ||
            idx >= cardIds.Count
        )
        {
            return;
        }

        if (
            revealed[idx] ||
            matched[idx] ||
            cards[idx] == null
        )
        {
            return;
        }

        SoundManager.Instance?.PlaySFX("sfx_pop");

        if (!RevealCard(idx))
            return;

        if (firstIdx == -1)
        {
            firstIdx = idx;
            return;
        }

        secondIdx = idx;
        isChecking = true;

        // Coroutine dijalankan melalui ScreenManager.
        if (!StartMemoryCoroutine(CheckMatch()))
        {
            HideCard(firstIdx);
            HideCard(secondIdx);

            firstIdx = -1;
            secondIdx = -1;
            isChecking = false;
        }
    }

    // ============================================================
    // SAFE COROUTINE
    // ============================================================

    private bool StartMemoryCoroutine(IEnumerator routine)
    {
        if (
            routine == null ||
            screenObj == null ||
            ScreenManager.Instance == null
        )
        {
            return false;
        }

        ScreenManager.Instance.StartCoroutine(routine);

        return true;
    }

    // ============================================================
    // REVEAL CARD
    // ============================================================

    private bool RevealCard(int idx)
    {
        if (
            screenObj == null ||
            idx < 0 ||
            idx >= revealed.Count ||
            idx >= cards.Count ||
            cards[idx] == null
        )
        {
            return false;
        }

        Transform card = cards[idx].transform;

        Transform questionMark =
            card.Find("QuestionMark");

        Transform front =
            card.Find("Front");

        if (questionMark == null || front == null)
        {
            Debug.LogWarning(
                "[MemoryCard] Komponen tampilan kartu tidak lengkap: " + idx
            );

            return false;
        }

        revealed[idx] = true;

        questionMark.gameObject.SetActive(false);
        front.gameObject.SetActive(true);

        return true;
    }

    // ============================================================
    // HIDE CARD
    // ============================================================

    private void HideCard(int idx)
    {
        if (
            idx < 0 ||
            idx >= revealed.Count ||
            idx >= cards.Count ||
            cards[idx] == null
        )
        {
            return;
        }

        Transform card = cards[idx].transform;

        Transform questionMark =
            card.Find("QuestionMark");

        Transform front =
            card.Find("Front");

        revealed[idx] = false;

        if (questionMark != null)
            questionMark.gameObject.SetActive(true);

        if (front != null)
            front.gameObject.SetActive(false);
    }

    // ============================================================
    // CHECK MATCH
    // ============================================================

    private IEnumerator CheckMatch()
    {
        int checkingFirst = firstIdx;
        int checkingSecond = secondIdx;

        yield return new WaitForSecondsRealtime(
            MATCH_CHECK_DELAY
        );

        if (screenObj == null)
            yield break;

        if (
            !IsValidCardIndex(checkingFirst) ||
            !IsValidCardIndex(checkingSecond) ||
            checkingFirst == checkingSecond
        )
        {
            firstIdx = -1;
            secondIdx = -1;
            isChecking = false;
            yield break;
        }

        // Kedua kartu menampilkan label pasangan yang sama.
        bool isMatch = string.Equals(
            cardValues[checkingFirst],
            cardValues[checkingSecond],
            System.StringComparison.Ordinal
        );

        if (isMatch)
        {
            submittedMatches.Add(new ChallengeMemoryCardMatch
            {
                first_card_id = cardIds[checkingFirst],
                second_card_id = cardIds[checkingSecond]
            });

            matched[checkingFirst] = true;
            matched[checkingSecond] = true;

            SoundManager.Instance?.PlaySFX("sfx_correct");
            HapticManager.Instance?.Vibrate(HapticType.Success);

            // Ubah warna kartu yang berhasil dicocokkan.
            Image firstImage =
                cards[checkingFirst].GetComponent<Image>();

            Image secondImage =
                cards[checkingSecond].GetComponent<Image>();

            if (firstImage != null)
                firstImage.color = TOBATheme.OptionCorrect;

            if (secondImage != null)
                secondImage.color = TOBATheme.OptionCorrect;

            matchedPairs++;

            UpdatePairsText();

            if (matchedPairs >= totalPairs)
            {
                firstIdx = -1;
                secondIdx = -1;
                isChecking = false;

                if (!hasSubmitted)
                {
                    hasSubmitted = true;
                    SubmitResult();
                }

                yield break;
            }
        }
        else
        {
            SoundManager.Instance?.PlaySFX("sfx_wrong");
            HapticManager.Instance?.Vibrate(HapticType.Error);

            HideCard(checkingFirst);
            HideCard(checkingSecond);
        }

        firstIdx = -1;
        secondIdx = -1;
        isChecking = false;
    }

    // ============================================================
    // VALIDATE CARD INDEX
    // ============================================================

    private bool IsValidCardIndex(int idx)
    {
        return
            idx >= 0 &&
            idx < cardValues.Count &&
            idx < cards.Count &&
            idx < revealed.Count &&
            idx < matched.Count &&
            idx < cardIds.Count &&
            cards[idx] != null;
    }

    // ============================================================
    // UPDATE PAIRS TEXT
    // ============================================================

    private void UpdatePairsText()
    {
        if (pairsTxt == null)
            return;

        pairsTxt.text =
            "Pasangan: " +
            matchedPairs +
            "/" +
            totalPairs;
    }

    // ============================================================
    // SUBMIT RESULT
    // ============================================================

    private void SubmitResult()
    {
        if (screenObj == null || challenge == null)
        {
            Debug.LogWarning(
                "[MemoryCard] Hasil tidak dapat dikirim karena data challenge tidak tersedia."
            );

            return;
        }

        ChallengeMemoryCardSubmission submission = new ChallengeMemoryCardSubmission
        {
            matches = submittedMatches
        };
        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            JsonUtility.ToJson(submission)
        );
    }
}
