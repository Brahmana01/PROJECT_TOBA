
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class QuizResultScreen : BaseScreen
{
    private LocationData locationData;
    private int correctCount;
    private int totalQuestions;
    private bool isSubmitted = false;
    private bool isPass;

    private TextMeshProUGUI statusTxt;
    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI starsTxt;
    private TextMeshProUGUI scoreTxt;
    private GameObject loadingObj;

    private int starsEarned;
    private Image[] starImages = new Image[3];
    private Coroutine introCoroutine;

    private static Sprite roundedSprite;
    private static Sprite starSprite;

    public QuizResultScreen(
        Transform parent,
        LocationData loc,
        int correct,
        int total
    ) : base(parent, "QuizResult")
    {
        locationData = loc;
        correctCount = correct;
        totalQuestions = total;
    }

    // =========================================================
    // SHOW
    // =========================================================

    public override void Show()
    {
        isPass = locationData != null &&
                 correctCount >= locationData.minimum_correct;

        starsEarned = CalculateStars();

        CreateBackground();

        HeaderBar.Create(
            screenObj.transform,
            "HASIL TANTANGAN",
            null
        );

        CreateResultCard();
        CreateCharacter();
        CreateActionButtons(isPass);

        if (ScreenManager.Instance != null)
        {
            introCoroutine =
                ScreenManager.Instance.StartCoroutine(PlayIntro());
        }

        SubmitResult();
    }

    // =========================================================
    // BACKGROUND
    // =========================================================

    private void CreateBackground()
    {
        SetBackground(TOBATheme.BGDark);

        Sprite bg = TOBATheme.LoadSprite("bg_toba");

        UIBuilder.CreateBackground(
            screenObj.transform,
            bg
        );

        GameObject shade = new GameObject("ResultOverlay");
        shade.transform.SetParent(screenObj.transform, false);

        RectTransform rt = shade.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = shade.AddComponent<Image>();
        img.color = new Color(0.015f, 0.07f, 0.035f, 0.38f);
        img.raycastTarget = false;
    }

    // =========================================================
    // ROUNDED SPRITE
    // =========================================================

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite == null)
            roundedSprite = BuildRoundedSprite(64, 20, 22);

        return roundedSprite;
    }

    private static Sprite BuildRoundedSprite(
        int size,
        int radius,
        int border
    )
    {
        Texture2D tex = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, radius, size - 1 - radius);
                float cy = Mathf.Clamp(y, radius, size - 1 - radius);

                float d = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(cx, cy)
                );

                float a = Mathf.Clamp01(radius - d + 0.5f);

                tex.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, a)
                );
            }
        }

        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border)
        );
    }

    // =========================================================
    // STAR SPRITE
    // =========================================================

    private static Sprite GetStarSprite()
    {
        if (starSprite != null)
            return starSprite;

        const int size = 128;

        Vector2[] pts = new Vector2[10];

        for (int i = 0; i < 10; i++)
        {
            float r = (i % 2 == 0) ? 58f : 25f;

            float ang =
                Mathf.PI / 2f + i * Mathf.PI / 5f;

            pts[i] = new Vector2(
                64f + Mathf.Cos(ang) * r,
                62f + Mathf.Sin(ang) * r
            );
        }

        Texture2D tex = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int hit = 0;

                for (int sy = 0; sy < 3; sy++)
                {
                    for (int sx = 0; sx < 3; sx++)
                    {
                        Vector2 p = new Vector2(
                            x + (sx + 0.5f) / 3f,
                            y + (sy + 0.5f) / 3f
                        );

                        if (PointInPolygon(pts, p))
                            hit++;
                    }
                }

                tex.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, hit / 9f)
                );
            }
        }

        tex.Apply();

        starSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return starSprite;
    }

    private static bool PointInPolygon(
        Vector2[] poly,
        Vector2 p
    )
    {
        bool inside = false;

        for (
            int i = 0, j = poly.Length - 1;
            i < poly.Length;
            j = i++
        )
        {
            if (
                ((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x <
                    (poly[j].x - poly[i].x) *
                    (p.y - poly[i].y) /
                    (poly[j].y - poly[i].y) +
                    poly[i].x)
            )
            {
                inside = !inside;
            }
        }

        return inside;
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreateAnchoredPanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color
    )
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();

        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;

        return go;
    }

    private Image CreateInsetImage(
        Transform parent,
        string name,
        Color color,
        float inset
    )
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);

        Image img = go.AddComponent<Image>();

        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;

        return img;
    }

    private Image CreateChip(
        Transform parent,
        string name,
        Color color,
        Vector2 size,
        Vector2 pos
    )
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image img = go.AddComponent<Image>();

        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;

        Outline outline = go.AddComponent<Outline>();

        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2f, -2f);

        return img;
    }

    private TextMeshProUGUI MakeText(
        Transform parent,
        string text,
        int size,
        Color color,
        Vector2 box,
        Vector2 pos,
        FontStyles style
    )
    {
        TextMeshProUGUI t =
            UIBuilder.CreateTextSized(
                parent,
                text,
                size,
                color,
                TextAlignmentOptions.Center,
                box,
                pos
            ).GetComponent<TextMeshProUGUI>();

        t.fontStyle = style;
        t.raycastTarget = false;

        t.enableAutoSizing = true;
        t.fontSizeMin = Mathf.Max(18, size * 0.6f);
        t.fontSizeMax = size;

        return t;
    }

    // =========================================================
    // RESULT CARD
    // =========================================================

    private void CreateResultCard()
    {
        GameObject card = CreateAnchoredPanel(
            screenObj.transform,
            "ResultCard",
            new Vector2(0.055f, 0.29f),
            new Vector2(0.945f, 0.84f),
            TOBATheme.BatakGold
        );

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(0f, -9f);

        // Lapisan kayu
        Image wood = CreateInsetImage(
            card.transform,
            "WoodFrame",
            TOBATheme.CardBorder,
            7f
        );

        // Lapisan perkamen
        Image parchment = CreateInsetImage(
            wood.transform,
            "Parchment",
            TOBATheme.CardBG,
            7f
        );

        Sprite parchmentSprite =
            TOBATheme.LoadSprite("panel_parchment");

        if (parchmentSprite != null)
        {
            parchment.sprite = parchmentSprite;

            parchment.type =
                parchmentSprite.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;

            parchment.color = Color.white;
        }

        Transform p = parchment.transform;

        // Nama lokasi
        string locationName =
            locationData != null &&
            !string.IsNullOrEmpty(locationData.name)
            ? locationData.name.ToUpper()
            : "JELAJAH BUDAYA";

        TextMeshProUGUI locationTxt = MakeText(
            p,
            locationName,
            38,
            TOBATheme.WoodDark,
            new Vector2(830, 80),
            new Vector2(0f, 445f),
            FontStyles.Bold
        );

        locationTxt.enableAutoSizing = true;
        locationTxt.fontSizeMin = 24;

        // Status kelulusan
        statusTxt = MakeText(
            p,
            isPass ? "LUAR BIASA!" : "BELUM LULUS",
            64,
            isPass ? TOBATheme.Success : TOBATheme.Error,
            new Vector2(850, 100),
            new Vector2(0f, 350f),
            FontStyles.Bold
        );

        // Garis dekorasi
        CreateChip(
            p,
            "TopDivider",
            TOBATheme.BatakGold,
            new Vector2(650, 5),
            new Vector2(0f, 290f)
        );

        // Bintang
        CreateStars(p);

        // Skor
        scoreTxt = MakeText(
            p,
            "0 / " + totalQuestions,
            110,
            TOBATheme.WoodDark,
            new Vector2(800, 130),
            new Vector2(0f, 55f),
            FontStyles.Bold
        );

        MakeText(
            p,
            "JAWABAN BENAR",
            30,
            TOBATheme.TextGray,
            new Vector2(800, 55),
            new Vector2(0f, -15f),
            FontStyles.Bold
        );

        // Progress bar
        CreateProgressBar(p);

        // Informasi XP
        CreateChip(
            p,
            "XPRow",
            TOBATheme.WoodDark,
            new Vector2(760, 82),
            new Vector2(0f, -220f)
        );

        MakeText(
            p,
            "XP DIPEROLEH",
            32,
            TOBATheme.TextLight,
            new Vector2(380, 65),
            new Vector2(-145f, -220f),
            FontStyles.Bold
        );

        xpTxt = MakeText(
            p,
            "+0",
            42,
            TOBATheme.BatakGold,
            new Vector2(200, 65),
            new Vector2(260f, -220f),
            FontStyles.Bold
        );

        // Informasi bintang
        CreateChip(
            p,
            "StarsRow",
            TOBATheme.WoodDark,
            new Vector2(760, 82),
            new Vector2(0f, -320f)
        );

        MakeText(
            p,
            "BINTANG DIPEROLEH",
            30,
            TOBATheme.TextLight,
            new Vector2(430, 65),
            new Vector2(-125f, -320f),
            FontStyles.Bold
        );

        starsTxt = MakeText(
            p,
            "+0",
            42,
            TOBATheme.BatakGold,
            new Vector2(180, 65),
            new Vector2(275f, -320f),
            FontStyles.Bold
        );

        // Loading
        loadingObj = MakeText(
            p,
            "MENYIMPAN HASIL...",
            25,
            TOBATheme.TextGray,
            new Vector2(800, 55),
            new Vector2(0f, -430f),
            FontStyles.Italic
        ).gameObject;
    }

    // =========================================================
    // PROGRESS BAR
    // =========================================================

    private void CreateProgressBar(Transform parent)
    {
        CreateChip(
            parent,
            "ProgressBackground",
            TOBATheme.WoodDark,
            new Vector2(760, 22),
            new Vector2(0f, -105f)
        );

        float progress = totalQuestions > 0
            ? Mathf.Clamp01((float)correctCount / totalQuestions)
            : 0f;

        GameObject fill = new GameObject("ProgressFill");
        fill.transform.SetParent(parent, false);

        RectTransform rt = fill.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.sizeDelta = new Vector2(
            750f * progress,
            13f
        );

        rt.anchoredPosition = new Vector2(
            -375f + (750f * progress / 2f),
            -105f
        );

        Image img = fill.AddComponent<Image>();

        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = TOBATheme.BatakGold;
        img.raycastTarget = false;

        float percentage = progress * 100f;

        MakeText(
            parent,
            "AKURASI " + Mathf.RoundToInt(percentage) + "%",
            25,
            TOBATheme.WoodMedium,
            new Vector2(500, 50),
            new Vector2(0f, -150f),
            FontStyles.Bold
        );
    }

    // =========================================================
    // STARS
    // =========================================================

    private void CreateStars(Transform parent)
    {
        float[] xs = { -190f, 0f, 190f };
        float[] ys = { 205f, 230f, 205f };
        float[] sizes = { 135f, 165f, 135f };

        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject("Star_" + (i + 1));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax =
                new Vector2(0.5f, 0.5f);

            rt.sizeDelta = new Vector2(
                sizes[i],
                sizes[i]
            );

            rt.anchoredPosition = new Vector2(
                xs[i],
                ys[i]
            );

            Image img = go.AddComponent<Image>();

            img.sprite = GetStarSprite();
            img.type = Image.Type.Simple;
            img.raycastTarget = false;

            bool active = i < starsEarned;

            img.color = active
                ? TOBATheme.StarActive
                : TOBATheme.StarInactive;

            Outline outline = go.AddComponent<Outline>();

            outline.effectColor = TOBATheme.WoodDark;
            outline.effectDistance = new Vector2(3f, -3f);

            if (active)
                rt.localScale = Vector3.zero;

            starImages[i] = img;
        }
    }

    // =========================================================
    // CHARACTER
    // =========================================================

    private void CreateCharacter()
    {
        int charID = PlayerPrefs.GetInt(
            "SelectedCharacterID",
            1
        );

        string character = charID == 1 ? "ucok" : "butet";

        Sprite expression = TOBATheme.LoadSprite(
            "gambar/" + character +
            (isPass ? "_senang" : "_salah")
        );

        if (expression == null)
        {
            expression = TOBATheme.LoadSprite(
                charID == 1 ? "char_ucok" : "char_butet"
            );
        }

        if (expression == null)
            return;

        GameObject ch = UIBuilder.CreateImage(
            screenObj.transform,
            "ResultCharacter",
            expression,
            new Vector2(235f, 330f),
            new Vector2(445f, -475f)
        );

        Image img = ch.GetComponent<Image>();

        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    // =========================================================
    // ACTION BUTTONS
    // =========================================================

    private void CreateActionButtons(bool passed)
    {
        if (passed)
        {
            CreateActionButton(
                "BtnLanjut",
                "LANJUTKAN JELAJAH",
                new Vector2(820, 120),
                new Vector2(0f, -575f),
                TOBATheme.BtnPrimary,
                Color.white,
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new MapScreen(container)
                    );
                }
            );
        }
        else
        {
            CreateActionButton(
                "BtnUlangi",
                "ULANGI TANTANGAN",
                new Vector2(820, 120),
                new Vector2(0f, -575f),
                TOBATheme.Warning,
                TOBATheme.WoodDark,
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new QuizScreen(container, locationData)
                    );
                }
            );
        }

        CreateActionButton(
            "BtnPeta",
            "KEMBALI KE PETA",
            new Vector2(820, 110),
            new Vector2(0f, -710f),
            TOBATheme.BtnSecondary,
            TOBATheme.BatakGold,
            () =>
            {
                ScreenManager.Instance.ChangeScreen(
                    new MapScreen(container)
                );
            }
        );

        CreateActionButton(
            "BtnShare",
            "BAGIKAN HASIL",
            new Vector2(400, 85),
            new Vector2(0f, -830f),
            TOBATheme.WoodDark,
            TOBATheme.BatakGold,
            OnShare
        );
    }

    private void CreateActionButton(
        string name,
        string label,
        Vector2 size,
        Vector2 pos,
        Color faceColor,
        Color textColor,
        Action onClick
    )
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax =
            new Vector2(0.5f, 0.5f);

        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image frame = root.AddComponent<Image>();

        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -6f);

        CreateInsetImage(
            root.transform,
            "ButtonFace",
            faceColor,
            5f
        );

        MakeText(
            root.transform,
            label,
            42,
            textColor,
            size - new Vector2(35f, 20f),
            Vector2.zero,
            FontStyles.Bold
        );

        Button btn = root.AddComponent<Button>();

        btn.targetGraphic = frame;

        ColorBlock colors = btn.colors;

        colors.normalColor = Color.white;
        colors.highlightedColor =
            new Color(1f, 0.95f, 0.75f, 1f);

        colors.pressedColor =
            new Color(0.8f, 0.7f, 0.5f, 1f);

        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance?.PlaySFX("sfx_click");
            HapticManager.Instance?.Vibrate(HapticType.Light);

            onClick?.Invoke();
        });
    }

    // =========================================================
    // SHARE
    // =========================================================

    private void OnShare()
    {
        SoundManager.Instance?.PlaySFX("sfx_click");

        HapticManager.Instance?.Vibrate(HapticType.Light);

        ShareManager.ShareQuizResult(
            locationData.name,
            correctCount,
            totalQuestions,
            starsEarned
        );
    }

    // =========================================================
    // INTRO ANIMATION
    // =========================================================

    private IEnumerator PlayIntro()
    {
        // Skor menghitung naik
        float t = 0f;
        const float duration = 0.8f;

        while (t < duration)
        {
            if (screenObj == null || scoreTxt == null)
                yield break;

            t += Time.unscaledDeltaTime;

            int value = Mathf.RoundToInt(
                Mathf.Lerp(
                    0f,
                    correctCount,
                    Mathf.Clamp01(t / duration)
                )
            );

            scoreTxt.text = value + " / " + totalQuestions;

            yield return null;
        }

        if (scoreTxt != null)
        {
            scoreTxt.text =
                correctCount + " / " + totalQuestions;
        }

        // Bintang muncul satu per satu
        for (int i = 0; i < starsEarned; i++)
        {
            if (starImages[i] == null)
                continue;

            Transform tr = starImages[i].transform;

            float elapsed = 0f;
            const float popDuration = 0.4f;

            while (elapsed < popDuration)
            {
                if (screenObj == null || tr == null)
                    yield break;

                elapsed += Time.unscaledDeltaTime;

                float x = Mathf.Clamp01(
                    elapsed / popDuration
                ) - 1f;

                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;

                float scale =
                    1f + c3 * x * x * x + c1 * x * x;

                tr.localScale =
                    Vector3.one * Mathf.Max(0f, scale);

                yield return null;
            }

            if (tr != null)
                tr.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(0.15f);
        }

        introCoroutine = null;
    }

    // =========================================================
    // STAR CALCULATION
    // =========================================================

    private int CalculateStars()
    {
        if (totalQuestions <= 0)
            return 0;

        float ratio =
            (float)correctCount / totalQuestions;

        if (ratio >= 1f)
            return 3;

        if (ratio >= 0.6f)
            return 2;

        if (ratio >= 0.4f)
            return 1;

        return 0;
    }

    // =========================================================
    // OFFLINE QUEUE
    // =========================================================

    private void SaveOfflineSubmission(string payload)
    {
        PlayerPrefs.SetString(
            "OfflineSubmitQueue",
            payload
        );

        PlayerPrefs.SetString(
            "OfflineSubmitEndpoint",
            "/locations/submit"
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // SUBMIT RESULT
    // =========================================================

    private void SubmitResult()
    {
        if (isSubmitted)
            return;

        isSubmitted = true;

        if (locationData == null)
        {
            HandleOfflineResult();
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID");

        SubmitQuizRequest req = new SubmitQuizRequest
        {
            player_id = playerID,
            location_id = locationData.id,
            correct_count = correctCount
        };

        string payload = JsonUtility.ToJson(req);

        // API tidak tersedia
        if (APIManager.Instance == null)
        {
            SaveOfflineSubmission(payload);
            HandleOfflineResult();
            return;
        }

        APIManager.Instance.PostRequest(
            "/locations/submit",
            payload,

            // SUCCESS
            (response) =>
            {
                if (screenObj == null)
                    return;

                PlayerPrefs.DeleteKey("OfflineSubmitQueue");
                PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
                PlayerPrefs.Save();

                HandleSubmitResponse(response);
            },

            // ERROR / OFFLINE
            (err) =>
            {
                if (screenObj == null)
                    return;

                SaveOfflineSubmission(payload);

                NetworkManager.Instance?.ShowToast(
                    "Offline: hasil disimpan, akan dikirim saat online.",
                    false
                );

                HandleOfflineResult();
            }
        );
    }

    // =========================================================
    // HANDLE API RESPONSE
    // =========================================================

    private void HandleSubmitResponse(string response)
    {
        if (screenObj == null)
            return;

        SubmitQuizResponse res = null;

        try
        {
            res = JsonUtility.FromJson<SubmitQuizResponse>(
                response
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[QuizResult] Gagal membaca response: " + e.Message
            );
        }

        if (res == null)
        {
            HandleOfflineResult();
            return;
        }

        if (statusTxt != null)
        {
            statusTxt.text = isPass
                ? "LUAR BIASA!"
                : "BELUM LULUS";

            statusTxt.color = isPass
                ? TOBATheme.Success
                : TOBATheme.Error;
        }

        if (xpTxt != null)
            xpTxt.text = "+" + res.xp;

        if (starsTxt != null)
            starsTxt.text = "+" + res.stars;

        RemoveLoading();

        // LEVEL UP
        int previousLevel = PlayerPrefs.GetInt(
            "PlayerLevel",
            1
        );

        if (res.level > previousLevel)
        {
            PlayerPrefs.SetInt(
                "PlayerLevel",
                res.level
            );

            PlayerPrefs.Save();

            SoundManager.Instance?.PlaySFX("sfx_levelup");

            HapticManager.Instance?.Vibrate(
                HapticType.Success
            );

            LevelUpPopup.Show(
                screenObj.transform,
                res.level,
                ""
            );
        }

        // CHECK ACHIEVEMENTS
        CheckAchievements();
    }

    // =========================================================
    // CHECK ACHIEVEMENTS
    // =========================================================

    private void CheckAchievements()
    {
        if (APIManager.Instance == null)
            return;

        int playerID = PlayerPrefs.GetInt("PlayerID");

        APIManager.Instance.GetRequest(
            "/check-achievements/" + playerID,

            (achRes) =>
            {
                if (screenObj == null)
                    return;

                NewAchievementResponse ach = null;

                try
                {
                    ach = JsonUtility.FromJson<NewAchievementResponse>(
                        achRes
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        "[QuizResult] Achievement response error: " +
                        e.Message
                    );
                }

                if (
                    ach != null &&
                    ach.count > 0 &&
                    ach.newly_unlocked != null &&
                    ach.newly_unlocked.Count > 0
                )
                {
                    SoundManager.Instance?.PlaySFX(
                        "sfx_achievement"
                    );

                    AchievementPopup.Show(
                        screenObj.transform,
                        ach.newly_unlocked[0]
                    );
                }
            },

            (err) =>
            {
                Debug.LogWarning(
                    "[QuizResult] Gagal mengecek achievement: " + err
                );
            }
        );
    }

    // =========================================================
    // OFFLINE RESULT
    // =========================================================

    private void HandleOfflineResult()
    {
        if (screenObj == null)
            return;

        if (statusTxt != null)
        {
            statusTxt.text = isPass
                ? "LUAR BIASA!"
                : "BELUM LULUS";

            statusTxt.color = isPass
                ? TOBATheme.Success
                : TOBATheme.Error;
        }

        if (xpTxt != null)
            xpTxt.text = "OFFLINE";

        if (starsTxt != null)
            starsTxt.text = "OFFLINE";

        RemoveLoading();
    }

    private void RemoveLoading()
    {
        if (loadingObj != null)
        {
            UnityEngine.Object.Destroy(loadingObj);
            loadingObj = null;
        }
    }

    // =========================================================
    // HIDE
    // =========================================================

    public override void Hide()
    {
        if (
            introCoroutine != null &&
            ScreenManager.Instance != null
        )
        {
            ScreenManager.Instance.StopCoroutine(
                introCoroutine
            );
        }

        introCoroutine = null;

        base.Hide();
    }
}
