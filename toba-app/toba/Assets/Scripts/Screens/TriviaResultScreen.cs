using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Layar hasil Trivia.
/// Tampilan mengikuti QuizResultScreen (panel kayu, bingkai emas,
/// animasi bintang, dan tombol bergaya TOBA), tetapi seluruh data,
/// endpoint, serta navigasi tetap khusus Trivia.
/// </summary>
public class TriviaResultScreen : BaseScreen
{
    private TriviaCategoryData categoryData;
    private int correctCount;
    private int totalQuestions;

    private TextMeshProUGUI scoreTxt;
    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI levelTxt;
    private GameObject loadingObj;

    private Image[] starImages = new Image[3];
    private Image progressFill;
    private int starsEarned;
    private Coroutine introCoroutine;
    private bool isSubmitted;

    private static Sprite starSprite;

    public TriviaResultScreen(Transform parent, TriviaCategoryData cat, int correct, int total)
        : base(parent, "TriviaResult")
    {
        categoryData = cat;
        correctCount = correct;
        totalQuestions = total;
    }

    public override void Show()
    {
        starsEarned = CalculateStars();
        isSubmitted = false;

        // Background dan header mengikuti QuizResultScreen.
        SetBackground(TOBATheme.BGDark);

        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);
        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0f, 0f, 0.5f),
            Vector2.zero,
            Vector2.one
        );

        HeaderBar.Create(
            screenObj.transform,
            "HASIL TRIVIA",
            () =>
            {
                SoundManager.Instance?.PlaySFX("sfx_click");
                ScreenManager.Instance?.ChangeScreen(new TriviaMenuScreen(container));
            }
        );

        CreateResultCard();
        CreateCharacter();
        CreateActionButtons();

        if (ScreenManager.Instance != null)
        {
            introCoroutine = ScreenManager.Instance.StartCoroutine(PlayIntro());
        }
        else
        {
            // Fallback jika ScreenManager tidak tersedia.
            if (scoreTxt != null)
                scoreTxt.text = correctCount + " / " + totalQuestions + " JAWABAN BENAR";

            if (progressFill != null)
                progressFill.fillAmount = GetAccuracyRatio();

            for (int i = 0; i < starsEarned && i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                    starImages[i].transform.localScale = Vector3.one;
            }
        }

        SubmitResult();
    }

    // ---------------------------------------------------------------------
    // TEXT HELPER
    // ---------------------------------------------------------------------

    private TextMeshProUGUI MakeText(
        Transform parent,
        string text,
        int size,
        Color color,
        Vector2 box,
        Vector2 position,
        FontStyles style = FontStyles.Normal)
    {
        GameObject textObject = UIBuilder.CreateTextSized(
            parent,
            text,
            size,
            color,
            TextAlignmentOptions.Center,
            box,
            position
        );

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ---------------------------------------------------------------------
    // RESULT CARD
    // ---------------------------------------------------------------------

    private void CreateResultCard()
    {
        Sprite woodPanel = TOBATheme.LoadSprite("panel_wood");

        GameObject card = new GameObject("ResultCard");
        card.transform.SetParent(screenObj.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(960f, 950f);
        cardRect.anchoredPosition = new Vector2(0f, 75f);

        Image cardImage = card.AddComponent<Image>();
        cardImage.sprite = woodPanel != null ? woodPanel : UIBuilder.GetDefaultSprite();
        cardImage.type = woodPanel != null ? Image.Type.Sliced : Image.Type.Simple;
        cardImage.color = woodPanel != null
            ? new Color(0.92f, 0.82f, 0.65f, 0.98f)
            : TOBATheme.CardBG;
        cardImage.raycastTarget = false;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(6f, 6f);

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(8f, -8f);

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.StartCoroutine(
                UIBuilder.AnimateScaleIn(card.transform, 0.3f)
            );
        }

        // Nama kategori Trivia.
        string categoryName = categoryData != null && !string.IsNullOrEmpty(categoryData.name)
            ? categoryData.name.ToUpper()
            : "TRIVIA BUDAYA";

        TextMeshProUGUI categoryTxt = MakeText(
            card.transform,
            categoryName,
            38,
            TOBATheme.WoodDark,
            new Vector2(850f, 65f),
            new Vector2(0f, 425f),
            FontStyles.Bold
        );
        categoryTxt.enableAutoSizing = true;
        categoryTxt.fontSizeMin = 24f;
        categoryTxt.fontSizeMax = 38f;

        // Status hasil berdasarkan kualitas jawaban, bukan reward database.
        TextMeshProUGUI statusTxt = MakeText(
            card.transform,
            GetResultMessage(),
            66,
            starsEarned >= 2 ? TOBATheme.BtnPrimary : TOBATheme.Warning,
            new Vector2(880f, 95f),
            new Vector2(0f, 345f),
            FontStyles.Bold
        );
        statusTxt.enableAutoSizing = true;
        statusTxt.fontSizeMin = 38f;
        statusTxt.fontSizeMax = 66f;

        // Tiga bintang visual, dengan animasi kemunculan satu per satu.
        CreateStars(card.transform);

        // Garis emas pemisah.
        UIBuilder.CreatePanelSized(
            card.transform,
            "ResultDivider",
            TOBATheme.BatakGold,
            new Vector2(820f, 4f),
            new Vector2(0f, 105f)
        ).GetComponent<Image>().raycastTarget = false;

        // Skor utama.
        scoreTxt = MakeText(
            card.transform,
            "0 / " + totalQuestions + " JAWABAN BENAR",
            43,
            TOBATheme.TextDark,
            new Vector2(850f, 68f),
            new Vector2(0f, 48f),
            FontStyles.Bold
        );

        // Ringkasan jawaban benar dan salah.
        MakeText(
            card.transform,
            "BENAR : " + correctCount,
            35,
            TOBATheme.Success,
            new Vector2(360f, 58f),
            new Vector2(-205f, -25f),
            FontStyles.Bold
        );

        MakeText(
            card.transform,
            "SALAH : " + Mathf.Max(0, totalQuestions - correctCount),
            35,
            TOBATheme.Error,
            new Vector2(360f, 58f),
            new Vector2(205f, -25f),
            FontStyles.Bold
        );

        // Progress bar akurasi jawaban.
        MakeText(
            card.transform,
            "AKURASI JAWABAN",
            27,
            TOBATheme.WoodDark,
            new Vector2(700f, 42f),
            new Vector2(0f, -83f),
            FontStyles.Bold
        );
        CreateProgressBar(card.transform);

        // XP dan level tetap berasal dari response API Trivia.
        xpTxt = MakeText(
            card.transform,
            "✨ XP Diperoleh : +0",
            38,
            TOBATheme.TextDark,
            new Vector2(820f, 58f),
            new Vector2(0f, -205f),
            FontStyles.Normal
        );

        levelTxt = MakeText(
            card.transform,
            "LEVEL : " + PlayerPrefs.GetInt("PlayerLevel", 1),
            36,
            TOBATheme.TextDark,
            new Vector2(820f, 55f),
            new Vector2(0f, -270f),
            FontStyles.Bold
        );

        loadingObj = MakeText(
            card.transform,
            "Menyimpan hasil...",
            28,
            TOBATheme.TextGray,
            new Vector2(800f, 48f),
            new Vector2(0f, -345f),
            FontStyles.Italic
        ).gameObject;
    }

    // ---------------------------------------------------------------------
    // STARS
    // ---------------------------------------------------------------------

    private void CreateStars(Transform parent)
    {
        float[] xPositions = { -190f, 0f, 190f };
        float[] sizes = { 145f, 165f, 145f };

        for (int i = 0; i < 3; i++)
        {
            GameObject starObject = new GameObject("Star_" + (i + 1));
            starObject.transform.SetParent(parent, false);

            RectTransform rect = starObject.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(sizes[i], sizes[i]);
            rect.anchoredPosition = new Vector2(xPositions[i], 220f);

            Image starImage = starObject.AddComponent<Image>();
            starImage.sprite = GetStarSprite();
            starImage.type = Image.Type.Simple;
            starImage.preserveAspect = true;
            starImage.raycastTarget = false;
            starImage.color = i < starsEarned
                ? TOBATheme.StarActive
                : TOBATheme.StarInactive;

            Outline starOutline = starObject.AddComponent<Outline>();
            starOutline.effectColor = TOBATheme.WoodDark;
            starOutline.effectDistance = new Vector2(3f, -3f);

            if (i < starsEarned)
                rect.localScale = Vector3.zero;

            starImages[i] = starImage;
        }
    }

    private static Sprite GetStarSprite()
    {
        if (starSprite != null)
            return starSprite;

        const int textureSize = 128;
        Vector2[] points = new Vector2[10];

        for (int i = 0; i < 10; i++)
        {
            float radius = (i % 2 == 0) ? 58f : 25f;
            float angle = Mathf.PI / 2f + i * Mathf.PI / 5f;

            points[i] = new Vector2(
                64f + Mathf.Cos(angle) * radius,
                62f + Mathf.Sin(angle) * radius
            );
        }

        Texture2D texture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int hit = 0;

                for (int sampleY = 0; sampleY < 3; sampleY++)
                {
                    for (int sampleX = 0; sampleX < 3; sampleX++)
                    {
                        Vector2 sample = new Vector2(
                            x + (sampleX + 0.5f) / 3f,
                            y + (sampleY + 0.5f) / 3f
                        );

                        if (PointInPolygon(points, sample))
                            hit++;
                    }
                }

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, hit / 9f));
            }
        }

        texture.Apply();

        starSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return starSprite;
    }

    private static bool PointInPolygon(Vector2[] polygon, Vector2 point)
    {
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) *
                    (point.y - polygon[i].y) /
                    (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    // ---------------------------------------------------------------------
    // PROGRESS BAR
    // ---------------------------------------------------------------------

    private void CreateProgressBar(Transform parent)
    {
        GameObject barBackground = UIBuilder.CreatePanelSized(
            parent,
            "AccuracyBarBackground",
            new Color(0.18f, 0.12f, 0.07f, 0.75f),
            new Vector2(700f, 24f),
            new Vector2(0f, -125f)
        );

        Image backgroundImage = barBackground.GetComponent<Image>();
        backgroundImage.raycastTarget = false;

        GameObject fillObject = new GameObject("AccuracyBarFill");
        fillObject.transform.SetParent(barBackground.transform, false);

        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        progressFill = fillObject.AddComponent<Image>();
        progressFill.sprite = UIBuilder.GetDefaultSprite();
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFill.fillAmount = 0f;
        progressFill.color = TOBATheme.BtnPrimary;
        progressFill.raycastTarget = false;
    }

    private float GetAccuracyRatio()
    {
        if (totalQuestions <= 0)
            return 0f;

        return Mathf.Clamp01((float)correctCount / totalQuestions);
    }

    // ---------------------------------------------------------------------
    // CHARACTER
    // ---------------------------------------------------------------------

    private void CreateCharacter()
    {
        int characterID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string characterName = characterID == 1 ? "ucok" : "butet";
        bool goodResult = starsEarned >= 2;

        Sprite expressionSprite = TOBATheme.LoadSprite(
            "gambar/" + characterName + (goodResult ? "_senang" : "_salah")
        );

        if (expressionSprite == null)
        {
            expressionSprite = TOBATheme.LoadSprite(
                characterID == 1 ? "char_ucok" : "char_butet"
            );
        }

        if (expressionSprite == null)
            return;

        GameObject character = UIBuilder.CreateImage(
            screenObj.transform,
            "Character",
            expressionSprite,
            new Vector2(230f, 320f),
            new Vector2(458f, -475f)
        );

        Image characterImage = character.GetComponent<Image>();
        if (characterImage != null)
            characterImage.preserveAspect = true;
    }

    // ---------------------------------------------------------------------
    // BUTTONS - NAVIGASI KHUSUS TRIVIA
    // ---------------------------------------------------------------------

    private void CreateActionButtons()
    {
        // Main lagi pada kategori yang sama.
        GameObject playAgainButton = UIBuilder.CreateButton(
            screenObj.transform,
            "BtnLagi",
            "MAIN LAGI",
            new Vector2(800f, 132f),
            TOBATheme.BtnPrimary,
            () =>
            {
                SoundManager.Instance?.PlaySFX("sfx_click");
                ScreenManager.Instance?.ChangeScreen(
                    new TriviaQuizScreen(container, categoryData)
                );
            }
        );

        playAgainButton.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0f, -575f);

        // Kembali ke menu kategori Trivia.
        GameObject categoryButton = UIBuilder.CreateButton(
            screenObj.transform,
            "BtnKategori",
            "PILIH KATEGORI LAIN",
            new Vector2(800f, 112f),
            TOBATheme.BtnSecondary,
            () =>
            {
                SoundManager.Instance?.PlaySFX("sfx_click");
                ScreenManager.Instance?.ChangeScreen(
                    new TriviaMenuScreen(container)
                );
            }
        );

        categoryButton.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0f, -710f);
    }

    // ---------------------------------------------------------------------
    // INTRO ANIMATION
    // ---------------------------------------------------------------------

    private IEnumerator PlayIntro()
    {
        // Angka skor dan progress bar naik bersamaan.
        float elapsed = 0f;
        const float duration = 0.7f;

        while (elapsed < duration)
        {
            if (screenObj == null || scoreTxt == null)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int animatedCorrect = Mathf.RoundToInt(
                Mathf.Lerp(0f, correctCount, progress)
            );

            scoreTxt.text = animatedCorrect + " / " + totalQuestions + " JAWABAN BENAR";

            if (progressFill != null)
            {
                progressFill.fillAmount = totalQuestions > 0
                    ? (float)animatedCorrect / totalQuestions
                    : 0f;
            }

            yield return null;
        }

        if (scoreTxt != null)
            scoreTxt.text = correctCount + " / " + totalQuestions + " JAWABAN BENAR";

        if (progressFill != null)
            progressFill.fillAmount = GetAccuracyRatio();

        // Bintang aktif muncul satu per satu.
        for (int i = 0; i < starsEarned && i < starImages.Length; i++)
        {
            if (screenObj == null || starImages[i] == null)
                yield break;

            Transform starTransform = starImages[i].transform;
            float elapsedPop = 0f;
            const float popDuration = 0.4f;

            while (elapsedPop < popDuration)
            {
                if (screenObj == null || starTransform == null)
                    yield break;

                elapsedPop += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedPop / popDuration) - 1f;
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                float scale = 1f + c3 * t * t * t + c1 * t * t;

                starTransform.localScale = Vector3.one * Mathf.Max(0f, scale);
                yield return null;
            }

            if (starTransform != null)
                starTransform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(0.12f);
        }

        introCoroutine = null;
    }

    // ---------------------------------------------------------------------
    // RESULT MESSAGE AND STAR CALCULATION
    // ---------------------------------------------------------------------

    private string GetResultMessage()
    {
        if (starsEarned >= 3)
            return "LUAR BIASA!";

        if (starsEarned == 2)
            return "BAGUS SEKALI!";

        if (starsEarned == 1)
            return "TERUS TINGKATKAN!";

        return "JANGAN MENYERAH!";
    }

    private int CalculateStars()
    {
        if (totalQuestions <= 0)
            return 0;

        float ratio = (float)correctCount / totalQuestions;

        if (ratio >= 1f)
            return 3;

        if (ratio >= 0.6f)
            return 2;

        if (ratio >= 0.4f)
            return 1;

        return 0;
    }

    // ---------------------------------------------------------------------
    // SUBMIT TRIVIA - ENDPOINT DAN PAYLOAD TETAP
    // ---------------------------------------------------------------------

    private void SubmitResult()
    {
        if (isSubmitted)
            return;

        isSubmitted = true;

        if (APIManager.Instance == null || categoryData == null)
        {
            ShowOfflineResult();
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID");

        SubmitTriviaRequest request = new SubmitTriviaRequest
        {
            player_id = playerID,
            category_id = categoryData.id,
            correct_count = correctCount,
            wrong_count = totalQuestions - correctCount
        };

        string jsonPayload = JsonUtility.ToJson(request);

        APIManager.Instance.PostRequest(
            "/trivia/submit",
            jsonPayload,
            (response) =>
            {
                if (screenObj == null)
                    return;

                SubmitTriviaResponse result =
                    JsonUtility.FromJson<SubmitTriviaResponse>(response);

                if (result != null)
                {
                    if (xpTxt != null)
                        xpTxt.text = "✨ XP Diperoleh : +" + result.xp;

                    if (levelTxt != null)
                        levelTxt.text = "LEVEL : " + result.level;

                    PlayerPrefs.SetInt("PlayerLevel", result.level);
                }
                else
                {
                    if (xpTxt != null)
                        xpTxt.text = "✨ XP Diperoleh : (data tidak terbaca)";
                }

                DestroyLoadingText();
            },
            (error) =>
            {
                if (screenObj == null)
                    return;

                ShowOfflineResult();
            }
        );
    }

    private void ShowOfflineResult()
    {
        if (xpTxt != null)
            xpTxt.text = "✨ XP Diperoleh : OFFLINE";

        if (levelTxt != null)
            levelTxt.text = "LEVEL : " + PlayerPrefs.GetInt("PlayerLevel", 1);

        DestroyLoadingText();
    }

    private void DestroyLoadingText()
    {
        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }
    }

    // ---------------------------------------------------------------------
    // CLEANUP
    // ---------------------------------------------------------------------

    public override void Hide()
    {
        if (introCoroutine != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(introCoroutine);

        introCoroutine = null;
        base.Hide();
    }
}
