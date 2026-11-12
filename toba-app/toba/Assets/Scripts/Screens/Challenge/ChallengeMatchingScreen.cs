using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChallengeMatchingScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<MatchPair> pairs = new List<MatchPair>();

    private int correctCount = 0;
    private int selectedBatakIndex = -1;

    private GameObject batakContainer;
    private GameObject indoContainer;

    private Image[] batakCardImages;
    private Image[] indoCardImages;
    private Button[] batakCardButtons;
    private Button[] indoCardButtons;
    private bool[] matchedPairs;

    private bool isResolving = false;
    private bool isFinished = false;
    private bool isSubmitting = false;

    private readonly Color defaultCardColor =
        new Color(0.9f, 0.8f, 0.6f, 1f);

    public ChallengeMatchingScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeMatching")
    {
        challenge = ch;
    }

    // ============================================================
    // SHOW
    // ============================================================

    public override void Show()
    {
        if (!IsReady)
        {
            Debug.LogWarning(
                "[ChallengeMatching] Screen atau parent tidak tersedia."
            );
            return;
        }

        if (challenge == null || challenge.id <= 0)
        {
            Debug.LogWarning(
                "[ChallengeMatching] Data challenge tidak valid."
            );
            ReturnToMainMenu();
            return;
        }

        ResetGameState();

        SetBackground(TOBATheme.BGDark);

        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0, 0.04f, 0, 0.5f),
            Vector2.zero,
            Vector2.one
        );

        string challengeTitle = string.IsNullOrWhiteSpace(challenge.title)
            ? "TANTANGAN"
            : challenge.title.ToUpperInvariant();

        HeaderBar.Create(
            screenObj.transform,
            challengeTitle,
            ReturnToMainMenu
        );

        if (!TryLoadPairs())
        {
            Debug.LogWarning(
                "[ChallengeMatching] Data pasangan kata tidak valid atau kosong."
            );

            NetworkManager.Instance?.ShowToast(
                "Data tantangan tidak tersedia.",
                true
            );

            ReturnToMainMenu();
            return;
        }

        // Posisi, ukuran, warna, dan teks instruksi dipertahankan.
        GameObject instruction = UIBuilder.CreateTextSized(
            screenObj.transform,
            "Cocokkan kata Batak dengan artinya!",
            34,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(800, 60),
            new Vector2(0, 700f)
        );

        if (instruction != null)
        {
            TextMeshProUGUI instructionText =
                instruction.GetComponent<TextMeshProUGUI>();

            if (instructionText != null)
                instructionText.fontStyle = FontStyles.Normal;
        }

        // Kolom Bahasa Batak: anchor dan ukuran tetap.
        batakContainer = new GameObject("BatakCol");
        batakContainer.transform.SetParent(screenObj.transform, false);

        RectTransform rtB =
            batakContainer.AddComponent<RectTransform>();

        rtB.anchorMin = new Vector2(0.05f, 0.15f);
        rtB.anchorMax = new Vector2(0.48f, 0.75f);
        rtB.offsetMin = rtB.offsetMax = Vector2.zero;

        // Kolom Bahasa Indonesia: anchor dan ukuran tetap.
        indoContainer = new GameObject("IndoCol");
        indoContainer.transform.SetParent(screenObj.transform, false);

        RectTransform rtI =
            indoContainer.AddComponent<RectTransform>();

        rtI.anchorMin = new Vector2(0.52f, 0.15f);
        rtI.anchorMax = new Vector2(0.95f, 0.75f);
        rtI.offsetMin = rtI.offsetMax = Vector2.zero;

        InitializeCardStates();

        // Acak urutan tampilan kartu Indonesia, bukan urutan data pasangan.
        List<int> shuffledPairIndices = new List<int>();

        for (int i = 0; i < pairs.Count; i++)
            shuffledPairIndices.Add(i);

        Shuffle(shuffledPairIndices);

        for (int i = 0; i < pairs.Count; i++)
        {
            CreateWordCard(
                batakContainer.transform,
                pairs[i].batak,
                i,
                i,
                true
            );
        }

        for (int i = 0; i < shuffledPairIndices.Count; i++)
        {
            int pairIndex = shuffledPairIndices[i];

            CreateWordCard(
                indoContainer.transform,
                pairs[pairIndex].indo,
                i,
                pairIndex,
                false
            );
        }
    }

    // ============================================================
    // DATA VALIDATION
    // ============================================================

    private bool TryLoadPairs()
    {
        pairs.Clear();

        if (string.IsNullOrWhiteSpace(challenge.data_json))
        {
            Debug.LogWarning(
                "[ChallengeMatching] JSON challenge kosong."
            );
            return false;
        }

        MatchingData data;

        try
        {
            data = JsonUtility.FromJson<MatchingData>(
                challenge.data_json
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "[ChallengeMatching] JSON tidak valid: " + ex.Message
            );
            return false;
        }

        if (data == null || data.pairs == null)
        {
            Debug.LogWarning(
                "[ChallengeMatching] Properti pairs tidak ditemukan."
            );
            return false;
        }

        HashSet<string> usedBatakWords =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        HashSet<string> usedIndonesianWords =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        HashSet<string> exactPairs =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < data.pairs.Count; i++)
        {
            MatchPair pair = data.pairs[i];

            if (pair == null ||
                string.IsNullOrWhiteSpace(pair.batak) ||
                string.IsNullOrWhiteSpace(pair.indo))
            {
                Debug.LogWarning(
                    "[ChallengeMatching] Pasangan kosong dilewati pada indeks " + i
                );
                continue;
            }

            string batak = pair.batak.Trim();
            string indo = pair.indo.Trim();

            // Pasangan yang sama persis cukup digunakan satu kali.
            string pairKey = batak + "\u001F" + indo;

            if (exactPairs.Contains(pairKey))
            {
                Debug.LogWarning(
                    "[ChallengeMatching] Pasangan duplikat dilewati: " +
                    batak + " - " + indo
                );
                continue;
            }

            // Setiap kata harus memiliki pasangan yang unik.
            if (usedBatakWords.Contains(batak) ||
                usedIndonesianWords.Contains(indo))
            {
                Debug.LogWarning(
                    "[ChallengeMatching] Kata memiliki pasangan ambigu: " +
                    batak + " - " + indo
                );

                pairs.Clear();
                return false;
            }

            exactPairs.Add(pairKey);
            usedBatakWords.Add(batak);
            usedIndonesianWords.Add(indo);

            pairs.Add(new MatchPair
            {
                batak = batak,
                indo = indo
            });
        }

        return pairs.Count > 0;
    }

    // ============================================================
    // GAME STATE
    // ============================================================

    private void ResetGameState()
    {
        correctCount = 0;
        selectedBatakIndex = -1;

        isResolving = false;
        isFinished = false;
        isSubmitting = false;

        batakContainer = null;
        indoContainer = null;

        batakCardImages = null;
        indoCardImages = null;
        batakCardButtons = null;
        indoCardButtons = null;
        matchedPairs = null;
    }

    private void InitializeCardStates()
    {
        int count = pairs.Count;

        batakCardImages = new Image[count];
        indoCardImages = new Image[count];

        batakCardButtons = new Button[count];
        indoCardButtons = new Button[count];

        matchedPairs = new bool[count];
    }

    private bool IsValidPairIndex(int index)
    {
        return pairs != null &&
               index >= 0 &&
               index < pairs.Count &&
               matchedPairs != null &&
               index < matchedPairs.Length;
    }

    // ============================================================
    // WORD CARD
    // ============================================================

    private void CreateWordCard(
        Transform parent,
        string text,
        int displayIndex,
        int pairIndex,
        bool isBatak)
    {
        if (!IsAlive ||
            parent == null ||
            !IsValidPairIndex(pairIndex))
        {
            return;
        }

        Sprite woodPanel = TOBATheme.LoadSprite("panel_wood");

        // Ukuran, jarak, anchor, dan posisi kartu dipertahankan.
        float h = 110f;
        float gap = 15f;
        float y = -(displayIndex * (h + gap)) - h / 2f;

        GameObject card = new GameObject("Card_" + text);
        card.transform.SetParent(parent, false);

        RectTransform rt = card.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0, h);
        rt.anchoredPosition = new Vector2(0, y);

        Image image = card.AddComponent<Image>();

        image.sprite = woodPanel ?? UIBuilder.GetDefaultSprite();
        image.type = woodPanel != null
            ? Image.Type.Sliced
            : Image.Type.Simple;

        image.color = defaultCardColor;
        image.raycastTarget = true;

        GameObject textObject = UIBuilder.CreateTextSized(
            card.transform,
            text,
            30,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(0, h),
            Vector2.zero
        );

        if (textObject != null)
        {
            TextMeshProUGUI cardText =
                textObject.GetComponent<TextMeshProUGUI>();

            if (cardText != null)
                cardText.raycastTarget = false;
        }

        Button button = card.AddComponent<Button>();
        button.targetGraphic = image;

        int capturedIndex = pairIndex;
        bool capturedIsBatak = isBatak;

        button.onClick.AddListener(
            () => OnWordClicked(capturedIndex, capturedIsBatak)
        );

        if (isBatak)
        {
            batakCardImages[pairIndex] = image;
            batakCardButtons[pairIndex] = button;
        }
        else
        {
            indoCardImages[pairIndex] = image;
            indoCardButtons[pairIndex] = button;
        }
    }

    // ============================================================
    // WORD SELECTION
    // ============================================================

    private void OnWordClicked(int pairIndex, bool isBatak)
    {
        if (!IsAlive ||
            isResolving ||
            isFinished ||
            isSubmitting ||
            !IsValidPairIndex(pairIndex) ||
            matchedPairs[pairIndex])
        {
            return;
        }

        SoundManager.Instance?.PlaySFX("sfx_click");

        if (isBatak)
        {
            SelectBatakWord(pairIndex);
            return;
        }

        if (selectedBatakIndex < 0 ||
            !IsValidPairIndex(selectedBatakIndex))
        {
            return;
        }

        int selectedIndex = selectedBatakIndex;

        if (string.Equals(
            pairs[selectedIndex].indo,
            pairs[pairIndex].indo,
            System.StringComparison.OrdinalIgnoreCase))
        {
            HandleCorrectMatch(selectedIndex, pairIndex);
        }
        else
        {
            HandleWrongMatch(selectedIndex, pairIndex);
        }
    }

    private void SelectBatakWord(int pairIndex)
    {
        if (!IsValidPairIndex(pairIndex) || matchedPairs[pairIndex])
            return;

        // Menekan kartu Batak yang sama membatalkan pilihan.
        if (selectedBatakIndex == pairIndex)
        {
            SetCardColor(
                batakCardImages[pairIndex],
                defaultCardColor
            );

            selectedBatakIndex = -1;
            return;
        }

        // Kembalikan warna pilihan sebelumnya.
        if (IsValidPairIndex(selectedBatakIndex) &&
            !matchedPairs[selectedBatakIndex])
        {
            SetCardColor(
                batakCardImages[selectedBatakIndex],
                defaultCardColor
            );
        }

        selectedBatakIndex = pairIndex;

        SetCardColor(
            batakCardImages[pairIndex],
            TOBATheme.BatakGold
        );
    }

    // ============================================================
    // MATCH RESULT
    // ============================================================

    private void HandleCorrectMatch(int batakIndex, int indoIndex)
    {
        if (!IsValidPairIndex(batakIndex) ||
            !IsValidPairIndex(indoIndex) ||
            matchedPairs[batakIndex] ||
            matchedPairs[indoIndex])
        {
            return;
        }

        // Tandai pasangan agar tidak dapat dihitung dua kali.
        matchedPairs[batakIndex] = true;
        matchedPairs[indoIndex] = true;

        correctCount++;

        SetCardColor(
            batakCardImages[batakIndex],
            TOBATheme.Success
        );

        SetCardColor(
            indoCardImages[indoIndex],
            TOBATheme.Success
        );

        SetButtonInteractable(
            batakCardButtons[batakIndex],
            false
        );

        SetButtonInteractable(
            indoCardButtons[indoIndex],
            false
        );

        selectedBatakIndex = -1;

        SoundManager.Instance?.PlaySFX("sfx_correct");
        HapticManager.Instance?.Vibrate(HapticType.Success);

        if (correctCount >= pairs.Count)
        {
            isFinished = true;

            if (!StartScreenCoroutine(FinishAfterDelay()))
                SubmitResult();
        }
    }

    private void HandleWrongMatch(int batakIndex, int indoIndex)
    {
        if (!IsValidPairIndex(batakIndex) ||
            !IsValidPairIndex(indoIndex))
        {
            selectedBatakIndex = -1;
            return;
        }

        isResolving = true;

        SetCardColor(
            batakCardImages[batakIndex],
            TOBATheme.Error
        );

        SetCardColor(
            indoCardImages[indoIndex],
            TOBATheme.Error
        );

        SoundManager.Instance?.PlaySFX("sfx_wrong");
        HapticManager.Instance?.Vibrate(HapticType.Error);

        if (!StartScreenCoroutine(
            ResetWrongCardsAfterDelay(batakIndex, indoIndex)))
        {
            ResetWrongCards(batakIndex, indoIndex);
        }
    }

    private IEnumerator ResetWrongCardsAfterDelay(
        int batakIndex,
        int indoIndex)
    {
        yield return new WaitForSecondsRealtime(0.65f);

        if (!IsAlive)
            yield break;

        ResetWrongCards(batakIndex, indoIndex);
    }

    private void ResetWrongCards(int batakIndex, int indoIndex)
    {
        if (!IsAlive)
            return;

        if (IsValidPairIndex(batakIndex) &&
            !matchedPairs[batakIndex])
        {
            SetCardColor(
                batakCardImages[batakIndex],
                defaultCardColor
            );
        }

        if (IsValidPairIndex(indoIndex) &&
            !matchedPairs[indoIndex])
        {
            SetCardColor(
                indoCardImages[indoIndex],
                defaultCardColor
            );
        }

        selectedBatakIndex = -1;
        isResolving = false;
    }

    // ============================================================
    // SUBMISSION
    // ============================================================

    private IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.75f);

        if (!IsAlive || !isFinished || isSubmitting)
            yield break;

        SubmitResult();
    }

    private void SubmitResult()
    {
        if (!IsAlive ||
            isSubmitting ||
            challenge == null ||
            correctCount <= 0)
        {
            return;
        }

        if (ScreenManager.Instance == null)
        {
            Debug.LogWarning(
                "[ChallengeMatching] ScreenManager tidak tersedia saat submit."
            );
            return;
        }

        isSubmitting = true;

        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            correctCount
        );
    }

    // ============================================================
    // SAFE HELPERS
    // ============================================================

    private void SetCardColor(Image image, Color color)
    {
        if (image != null)
            image.color = color;
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    private bool StartScreenCoroutine(IEnumerator routine)
    {
        if (routine == null ||
            !IsAlive ||
            ScreenManager.Instance == null)
        {
            return false;
        }

        ScreenManager.Instance.StartCoroutine(routine);
        return true;
    }

    private void ReturnToMainMenu()
    {
        if (ScreenManager.Instance == null || container == null)
            return;

        ScreenManager.Instance.ChangeScreen(
            new MainMenuScreen(container)
        );
    }

    // ============================================================
    // SHUFFLE
    // ============================================================

    private void Shuffle(List<int> list)
    {
        if (list == null)
            return;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
