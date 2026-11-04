
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuizScreen : BaseScreen
{
    private LocationData locationData;
    private List<QuizQuestionData> questions = new List<QuizQuestionData>();
    private int currentIndex = 0;
    private int correctCount = 0;
    private int totalQuestions = 0;

    // UI refs
    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI questionTxt;
    private TextMeshProUGUI timerTxt;
    private RectTransform progressFill;
    private GameObject optionsContainer;
    private GameObject feedbackPanel;
    private GameObject characterGO;
    private GameObject exitDialog;
    private Image timerBg;

    // Timer
    private float timeLimit = 15f;
    private float timeRemaining;
    private bool isAnswered = false;
    private bool isPaused = false;
    private int lastWarnSecond = -1;
    private Coroutine timerCo;
    private Coroutine feedbackCo;

    // Sprite dibuat lewat kode
    private static Sprite roundedSprite;
    private static Sprite circleSprite;
    private static Sprite chevronSprite;

    public QuizScreen(Transform parent, LocationData loc)
        : base(parent, "QuizScreen")
    {
        locationData = loc;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);

        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0.04f, 0f, 0.4f),
            Vector2.zero,
            Vector2.one
        );

        CreateHeader();
        CreateBackButton();
        CreateProgressArea();
        CreateQuestionArea();
        CreateOptionsArea();
        CreateCharacter();
        FetchQuiz();
    }

    // ───────────────── SPRITE HELPER ─────────────────

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite == null)
            roundedSprite = BuildSprite(64, 20, 22);

        return roundedSprite;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite == null)
            circleSprite = BuildSprite(64, 32, 0);

        return circleSprite;
    }

    private static Sprite BuildSprite(int size, int radius, int border)
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

    private static Sprite GetChevronSprite()
    {
        if (chevronSprite != null)
            return chevronSprite;

        const int size = 128;

        Vector2 a = new Vector2(86f, 24f);
        Vector2 b = new Vector2(42f, 64f);
        Vector2 c = new Vector2(86f, 104f);

        const float half = 11f;

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
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                float d = Mathf.Min(
                    DistToSegment(p, a, b),
                    DistToSegment(p, b, c)
                );

                tex.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        Mathf.Clamp01(half - d + 0.5f)
                    )
                );
            }
        }

        tex.Apply();

        chevronSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return chevronSprite;
    }

    private static float DistToSegment(
        Vector2 p,
        Vector2 a,
        Vector2 b)
    {
        Vector2 ab = b - a;

        float t = Mathf.Clamp01(
            Vector2.Dot(p - a, ab) / ab.sqrMagnitude
        );

        return Vector2.Distance(p, a + ab * t);
    }

    // ───────────────── UI HELPER ─────────────────

    private GameObject CreateAnchoredPanel(
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(screenObj.transform, false);

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
        float inset)
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

    private TextMeshProUGUI MakeText(
        Transform parent,
        string text,
        int size,
        Color color,
        TextAlignmentOptions align,
        Vector2 box,
        Vector2 pos,
        FontStyles style)
    {
        TextMeshProUGUI t = UIBuilder.CreateTextSized(
            parent,
            text,
            size,
            color,
            align,
            box,
            pos
        ).GetComponent<TextMeshProUGUI>();

        t.fontStyle = style;
        t.raycastTarget = false;

        return t;
    }

    private void PlayClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("sfx_click");
    }

    // ───────────────── HEADER PAPAN IT DEL ─────────────────

    private void CreateHeader()
    {
        GameObject header = CreateAnchoredPanel(
            "QuizWoodHeader",
            new Vector2(0.08f, 0.89f),
            new Vector2(0.92f, 0.985f),
            Color.white
        );

        Image headerImg = header.GetComponent<Image>();

        // Gambar papan AI milik IT Del
        Sprite papan = TOBATheme.LoadSprite("gambar/papan_it_del");

        if (papan != null)
        {
            headerImg.sprite = papan;
            headerImg.type = Image.Type.Simple;
            headerImg.color = Color.white;
            headerImg.preserveAspect = true;
        }
        else
        {
            Debug.LogWarning("Gambar papan_it_del tidak ditemukan!");
        }
    }

    // ───────────────── TOMBOL KEMBALI ─────────────────

    private void CreateBackButton()
    {
        GameObject root = new GameObject("BtnBack");
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, 0.94f);
        rt.anchorMax = new Vector2(0f, 0.94f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(104f, 104f);
        rt.anchoredPosition = new Vector2(86f, 0f);

        Image frame = root.AddComponent<Image>();

        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Image face = CreateInsetImage(
            root.transform,
            "Face",
            TOBATheme.WoodDark,
            4f
        );

        GameObject icon = new GameObject("Chevron");
        icon.transform.SetParent(face.transform, false);

        RectTransform iconRt = icon.AddComponent<RectTransform>();

        iconRt.anchorMin = iconRt.anchorMax =
            new Vector2(0.5f, 0.5f);

        iconRt.sizeDelta = new Vector2(60f, 60f);
        iconRt.anchoredPosition = new Vector2(-2f, 0f);

        Image iconImg = icon.AddComponent<Image>();

        iconImg.sprite = GetChevronSprite();
        iconImg.type = Image.Type.Simple;
        iconImg.color = TOBATheme.BatakGold;
        iconImg.raycastTarget = false;

        Button btn = root.AddComponent<Button>();

        btn.targetGraphic = face;

        ColorBlock colors = btn.colors;

        colors.normalColor = Color.white;
        colors.highlightedColor =
            new Color(1f, 0.95f, 0.8f, 1f);
        colors.pressedColor =
            new Color(0.8f, 0.7f, 0.5f, 1f);

        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            PlayClick();
            ShowExitDialog();
        });
    }

    // ───────────────── PROGRESS + TIMER ─────────────────

    private void CreateProgressArea()
    {
        GameObject bar = CreateAnchoredPanel(
            "ProgressBar",
            new Vector2(0.055f, 0.825f),
            new Vector2(0.945f, 0.88f),
            TOBATheme.WoodDark
        );

        Outline outline = bar.AddComponent<Outline>();

        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2f, 2f);

        GameObject track = new GameObject("ProgressTrack");
        track.transform.SetParent(bar.transform, false);

        RectTransform trackRt = track.AddComponent<RectTransform>();

        trackRt.anchorMin = new Vector2(0.04f, 0.10f);
        trackRt.anchorMax = new Vector2(0.96f, 0.24f);
        trackRt.offsetMin = Vector2.zero;
        trackRt.offsetMax = Vector2.zero;

        Image trackImg = track.AddComponent<Image>();

        trackImg.sprite = GetRoundedSprite();
        trackImg.type = Image.Type.Sliced;
        trackImg.color = new Color(0f, 0f, 0f, 0.5f);
        trackImg.raycastTarget = false;

        GameObject fill = new GameObject("ProgressFill");
        fill.transform.SetParent(track.transform, false);

        progressFill = fill.AddComponent<RectTransform>();

        progressFill.anchorMin = new Vector2(0f, 0f);
        progressFill.anchorMax = new Vector2(0f, 1f);
        progressFill.offsetMin = Vector2.zero;
        progressFill.offsetMax = Vector2.zero;

        Image fillImg = fill.AddComponent<Image>();

        fillImg.sprite = GetRoundedSprite();
        fillImg.type = Image.Type.Sliced;
        fillImg.color = TOBATheme.BatakGold;
        fillImg.raycastTarget = false;

        progressTxt = MakeText(
            bar.transform,
            "SOAL - / -",
            38,
            TOBATheme.TextLight,
            TextAlignmentOptions.Left,
            new Vector2(520, 90),
            new Vector2(-180f, 8f),
            FontStyles.Bold
        );

        GameObject chip = new GameObject("TimerBg");
        chip.transform.SetParent(bar.transform, false);

        RectTransform chipRt = chip.AddComponent<RectTransform>();

        chipRt.anchorMin = chipRt.anchorMax =
            new Vector2(0.5f, 0.5f);

        chipRt.sizeDelta = new Vector2(140f, 54f);
        chipRt.anchoredPosition = new Vector2(385f, 10f);

        timerBg = chip.AddComponent<Image>();

        timerBg.sprite = GetRoundedSprite();
        timerBg.type = Image.Type.Sliced;
        timerBg.color = new Color(0f, 0f, 0f, 0.35f);
        timerBg.raycastTarget = false;

        timerTxt = MakeText(
            chip.transform,
            "15",
            42,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(140f, 54f),
            Vector2.zero,
            FontStyles.Bold
        );
    }

    private void SetProgressFill(int current, int total)
    {
        if (progressFill == null)
            return;

        float p = Mathf.Clamp01(
            (float)current / Mathf.Max(1, total)
        );

        progressFill.anchorMax =
            new Vector2(Mathf.Max(0.04f, p), 1f);
    }

    // ───────────────── KARTU PERTANYAAN ─────────────────

    private void CreateQuestionArea()
    {
        GameObject card = CreateAnchoredPanel(
            "QuestionCard",
            new Vector2(0.065f, 0.615f),
            new Vector2(0.935f, 0.795f),
            TOBATheme.BatakGold
        );

        Shadow shadow = card.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -7f);

        Image brown = CreateInsetImage(
            card.transform,
            "CardBorder",
            TOBATheme.CardBorder,
            5f
        );

        Image parchmentImg = CreateInsetImage(
            brown.transform,
            "CardParchment",
            TOBATheme.CardBG,
            4f
        );

        Sprite parchment = TOBATheme.LoadSprite("panel_parchment");

        if (parchment != null)
        {
            parchmentImg.sprite = parchment;

            parchmentImg.type =
                parchment.border != Vector4.zero
                    ? Image.Type.Sliced
                    : Image.Type.Simple;

            parchmentImg.color = Color.white;
        }

        questionTxt = MakeText(
            parchmentImg.transform,
            "Memuat soal...",
            42,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(900, 290),
            Vector2.zero,
            FontStyles.Bold
        );

        questionTxt.textWrappingMode = TextWrappingModes.Normal;
        questionTxt.overflowMode = TextOverflowModes.Ellipsis;
        questionTxt.enableAutoSizing = true;
        questionTxt.fontSizeMin = 28;
        questionTxt.fontSizeMax = 44;
        questionTxt.margin = new Vector4(20, 15, 20, 15);
    }

    // ───────────────── AREA OPSI ─────────────────

    private void CreateOptionsArea()
    {
        optionsContainer = new GameObject("Options");
        optionsContainer.transform.SetParent(screenObj.transform, false);

        RectTransform rt =
            optionsContainer.AddComponent<RectTransform>();

        // Area opsi diperluas ke bawah kartu pertanyaan
        rt.anchorMin = new Vector2(0.055f, 0.14f);
        rt.anchorMax = new Vector2(0.945f, 0.595f);

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ───────────────── KARAKTER ─────────────────

    private void CreateCharacter()
    {
        int charID = PlayerPrefs.GetInt(
            "SelectedCharacterID",
            1
        );

        string path = charID == 1
            ? "gambar/ucok_berpikir"
            : "gambar/butet_berpikir";

        Sprite sp =
            TOBATheme.LoadSprite(path) ??
            TOBATheme.LoadSprite(
                charID == 1 ? "char_ucok" : "char_butet"
            );

        if (sp == null)
            return;

        characterGO = UIBuilder.CreateImage(
            screenObj.transform,
            "Character",
            sp,
            new Vector2(200, 280),
            new Vector2(356f, -797f)
        );

        characterGO.GetComponent<Image>().preserveAspect = true;
    }

    private void SetCharacterExpression(bool correct)
    {
        if (characterGO == null)
            return;

        int charID = PlayerPrefs.GetInt(
            "SelectedCharacterID",
            1
        );

        string b = charID == 1 ? "ucok" : "butet";

        Sprite sp =
            TOBATheme.LoadSprite(
                "gambar/" + b +
                (correct ? "_senang" : "_salah")
            ) ??
            TOBATheme.LoadSprite(
                charID == 1 ? "char_ucok" : "char_butet"
            );

        if (sp != null)
            characterGO.GetComponent<Image>().sprite = sp;
    }

    // ───────────────── FETCH QUIZ ─────────────────

    private void FetchQuiz()
    {
        LoadingOverlay.Show(
            screenObj.transform,
            "Memuat soal..."
        );

        string cached =
            CacheManager.GetCachedLocationQuiz(locationData.id);

        APIManager.Instance.GetRequestWithCache(
            "/locations/" + locationData.id + "/quiz",
            cached,

            (response) =>
            {
                LoadingOverlay.Hide();

                if (screenObj == null)
                    return;

                CacheManager.CacheLocationQuiz(
                    locationData.id,
                    response
                );

                QuizResponse res =
                    JsonUtility.FromJson<QuizResponse>(response);

                if (res?.questions != null &&
                    res.questions.Count > 0)
                {
                    questions = res.questions;
                    totalQuestions = questions.Count;

                    ShowQuestion(0);
                }
                else
                {
                    if (questionTxt)
                        questionTxt.text =
                            "Soal belum tersedia untuk lokasi ini.";

                    NetworkManager.Instance?.ShowToast(
                        "Soal belum tersedia",
                        true
                    );
                }
            },

            (err) =>
            {
                LoadingOverlay.Hide();

                if (screenObj == null)
                    return;

                if (questionTxt)
                    questionTxt.text =
                        "Gagal memuat soal.\n" + err;

                NetworkManager.Instance?.ShowToast(
                    err,
                    true
                );
            }
        );
    }

    // ───────────────── TAMPILKAN SOAL ─────────────────

    private void ShowQuestion(int index)
    {
        if (screenObj == null ||
            index >= questions.Count)
            return;

        isAnswered = false;
        currentIndex = index;

        var q = questions[index];

        if (progressTxt)
        {
            progressTxt.text =
                "SOAL " + (index + 1) +
                " / " + totalQuestions;
        }

        SetProgressFill(index + 1, totalQuestions);

        if (questionTxt)
        {
            questionTxt.text = q.question;

            ScreenManager.Instance.StartCoroutine(
                UIBuilder.AnimateScaleIn(
                    questionTxt.transform,
                    0.2f
                )
            );
        }

        // Bersihkan opsi lama
        if (optionsContainer)
        {
            foreach (Transform c in optionsContainer.transform)
            {
                UnityEngine.Object.Destroy(c.gameObject);
            }
        }

        // Karakter kembali ke ekspresi berpikir
        int charID = PlayerPrefs.GetInt(
            "SelectedCharacterID",
            1
        );

        string b = charID == 1 ? "ucok" : "butet";

        Sprite thinkSprite =
            TOBATheme.LoadSprite(
                "gambar/" + b + "_berpikir"
            );

        if (characterGO != null && thinkSprite != null)
        {
            characterGO.GetComponent<Image>().sprite =
                thinkSprite;
        }

        if (q.options == null || q.options.Count == 0)
        {
            if (questionTxt)
                questionTxt.text = "Pilihan jawaban belum tersedia.";

            return;
        }

        string[] labels = { "A", "B", "C", "D" };

        int count = Mathf.Min(q.options.Count, 4);

        for (int i = 0; i < count; i++)
        {
            var opt = q.options[i];

            if (opt == null)
                continue;

            int ci = i;

            CreateOptionButton(
                opt,
                labels[i],
                i,
                count,
                () =>
                {
                    if (!isAnswered)
                        OnAnswerSelected(opt, ci, q);
                }
            );
        }

        // Timer
        timeRemaining = timeLimit;
        lastWarnSecond = -1;

        if (timerCo != null)
            ScreenManager.Instance.StopCoroutine(timerCo);

        timerCo =
            ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    // ───────────────── TOMBOL JAWABAN ─────────────────

    private void CreateOptionButton(
        QuizOptionData opt,
        string label,
        int index,
        int count,
        System.Action onClick)
    {
        // Ukuran dan jarak tombol disesuaikan
        // agar empat pilihan dapat terlihat sekaligus.
        const float pitch = 0.23f;
        const float height = 0.21f;

        float totalHeight =
            height + (count - 1) * pitch;

        float shift =
            (1f - totalHeight) * 0.5f;

        float top =
            1f - shift - index * pitch;

        float bottom =
            top - height;

        GameObject optGO =
            new GameObject("Opt_" + label);

        optGO.transform.SetParent(
            optionsContainer.transform,
            false
        );

        RectTransform rt =
            optGO.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, bottom);
        rt.anchorMax = new Vector2(1f, top);

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image frame = optGO.AddComponent<Image>();

        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = optGO.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(0f, 0f, 0f, 0.4f);

        shadow.effectDistance =
            new Vector2(0f, -6f);

        // Permukaan tombol
        Image face = CreateInsetImage(
            optGO.transform,
            "Face",
            TOBATheme.OptionNormal,
            4f
        );

        // Lencana huruf
        GameObject badge = new GameObject("LetterBadge");

        badge.transform.SetParent(
            face.transform,
            false
        );

        RectTransform badgeRt =
            badge.AddComponent<RectTransform>();

        badgeRt.anchorMin =
            badgeRt.anchorMax =
                new Vector2(0f, 0.5f);

        badgeRt.pivot = new Vector2(0f, 0.5f);
        badgeRt.sizeDelta = new Vector2(86f, 86f);
        badgeRt.anchoredPosition = new Vector2(16f, 0f);

        Image badgeImg = badge.AddComponent<Image>();

        badgeImg.sprite = GetCircleSprite();
        badgeImg.type = Image.Type.Simple;
        badgeImg.color = TOBATheme.WoodMedium;
        badgeImg.raycastTarget = false;

        Outline badgeOutline =
            badge.AddComponent<Outline>();

        badgeOutline.effectColor =
            TOBATheme.BatakGold;

        badgeOutline.effectDistance =
            new Vector2(3f, 3f);

        MakeText(
            badge.transform,
            label,
            42,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(86f, 86f),
            Vector2.zero,
            FontStyles.Bold
        );

        // Teks jawaban
        GameObject txtObj = UIBuilder.CreateTextSized(
            face.transform,
            opt.option_text ?? "",
            36,
            TOBATheme.TextDark,
            TextAlignmentOptions.Left,
            new Vector2(740f, 100f),
            new Vector2(65f, 0f)
        );

        txtObj.name = "OptionText";

        TextMeshProUGUI optTxt =
            txtObj.GetComponent<TextMeshProUGUI>();

        optTxt.fontStyle = FontStyles.Bold;
        optTxt.textWrappingMode = TextWrappingModes.Normal;
        optTxt.overflowMode = TextOverflowModes.Ellipsis;
        optTxt.enableAutoSizing = true;
        optTxt.fontSizeMin = 26;
        optTxt.fontSizeMax = 38;
        optTxt.margin = new Vector4(8, 5, 8, 5);
        optTxt.raycastTarget = false;

        // Button
        Button b2 = optGO.AddComponent<Button>();

        b2.targetGraphic = face;

        ColorBlock cb = b2.colors;

        cb.normalColor = Color.white;

        cb.highlightedColor =
            new Color(1f, 0.94f, 0.72f, 1f);

        cb.pressedColor =
            new Color(0.88f, 0.76f, 0.52f, 1f);

        cb.disabledColor = Color.white;

        b2.colors = cb;

        b2.onClick.AddListener(() =>
        {
            PlayClick();
            onClick?.Invoke();
        });

        ScreenManager.Instance.StartCoroutine(
            SlideIn(optGO, index * 0.06f)
        );
    }

    private IEnumerator SlideIn(
        GameObject go,
        float delay)
    {
        if (go == null)
            yield break;

        RectTransform rt =
            go.GetComponent<RectTransform>();

        CanvasGroup cg =
            go.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        float t = 0f;

        while (t < delay)
        {
            if (go == null)
                yield break;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        t = 0f;

        const float dur = 0.25f;

        while (t < dur)
        {
            if (go == null)
                yield break;

            t += Time.unscaledDeltaTime;

            float p = Mathf.SmoothStep(
                0f,
                1f,
                t / dur
            );

            cg.alpha = p;

            rt.anchoredPosition = new Vector2(
                0f,
                Mathf.Lerp(-55f, 0f, p)
            );

            yield return null;
        }

        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        if (cg != null)
            UnityEngine.Object.Destroy(cg);
    }

    // ───────────────── JAWABAN DIPILIH ─────────────────

    private void OnAnswerSelected(
        QuizOptionData selected,
        int selIdx,
        QuizQuestionData question)
    {
        if (isAnswered)
            return;

        isAnswered = true;

        if (timerCo != null)
            ScreenManager.Instance.StopCoroutine(timerCo);

        timerCo = null;

        bool correct = selected.is_correct;

        if (correct)
            correctCount++;

        HapticManager.Instance?.Vibrate(
            correct ? HapticType.Success : HapticType.Error
        );

        SoundManager.Instance?.PlaySFX(
            correct ? "sfx_correct" : "sfx_wrong"
        );

        SetCharacterExpression(correct);

        HighlightOptions(question, selIdx);

        feedbackCo =
            ScreenManager.Instance.StartCoroutine(
                ShowFeedbackThenNext(
                    correct,
                    question.explanation
                )
            );
    }

    private void HighlightOptions(
        QuizQuestionData question,
        int selIdx)
    {
        if (optionsContainer == null ||
            question == null ||
            question.options == null)
            return;

        for (int i = 0;
             i < optionsContainer.transform.childCount;
             i++)
        {
            if (i >= question.options.Count)
                continue;

            Transform child =
                optionsContainer.transform.GetChild(i);

            Transform faceTr = child.Find("Face");

            Image face =
                faceTr != null
                    ? faceTr.GetComponent<Image>()
                    : null;

            Transform textTr =
                faceTr != null
                    ? faceTr.Find("OptionText")
                    : null;

            TextMeshProUGUI txt =
                textTr != null
                    ? textTr.GetComponent<TextMeshProUGUI>()
                    : null;

            if (face != null)
            {
                if (question.options[i].is_correct)
                {
                    face.color = TOBATheme.OptionCorrect;

                    if (txt != null)
                        txt.color = Color.white;
                }
                else if (i == selIdx)
                {
                    face.color = TOBATheme.OptionWrong;

                    if (txt != null)
                        txt.color = Color.white;
                }
                else
                {
                    face.color = new Color(
                        TOBATheme.CardBG.r,
                        TOBATheme.CardBG.g,
                        TOBATheme.CardBG.b,
                        0.6f
                    );
                }
            }

            Button btn = child.GetComponent<Button>();

            if (btn != null)
                btn.interactable = false;
        }
    }

    // ───────────────── FEEDBACK ─────────────────

    private IEnumerator ShowFeedbackThenNext(
        bool correct,
        string explanation)
    {
        if (screenObj == null)
            yield break;

        feedbackPanel = CreateAnchoredPanel(
            "Feedback",
            new Vector2(0.05f, 0.16f),
            new Vector2(0.95f, 0.325f),
            correct ? TOBATheme.Success : TOBATheme.Error
        );

        feedbackPanel.transform.localScale =
            Vector3.zero;

        Outline outline =
            feedbackPanel.AddComponent<Outline>();

        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3f, 3f);

        Shadow shadow =
            feedbackPanel.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(0f, 0f, 0f, 0.4f);

        shadow.effectDistance =
            new Vector2(0f, -5f);

        MakeText(
            feedbackPanel.transform,
            correct ? "BENAR!" : "SALAH!",
            46,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(880f, 60f),
            new Vector2(0f, 95f),
            FontStyles.Bold
        );

        TextMeshProUGUI expTmp = MakeText(
            feedbackPanel.transform,

            string.IsNullOrEmpty(explanation)
                ? "Lanjut ke soal berikutnya."
                : explanation,

            30,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(880f, 180f),
            new Vector2(0f, -40f),
            FontStyles.Normal
        );

        expTmp.textWrappingMode =
            TextWrappingModes.Normal;

        expTmp.overflowMode =
            TextOverflowModes.Ellipsis;

        expTmp.enableAutoSizing = true;
        expTmp.fontSizeMin = 20;
        expTmp.fontSizeMax = 30;

        expTmp.margin =
            new Vector4(10, 5, 10, 5);

        yield return UIBuilder.AnimateScaleIn(
            feedbackPanel.transform,
            0.2f
        );

        float waited = 0f;

        while (waited < 2.3f)
        {
            if (screenObj == null)
                yield break;

            if (!isPaused)
                waited += Time.unscaledDeltaTime;

            yield return null;
        }

        if (screenObj == null)
            yield break;

        if (feedbackPanel)
            UnityEngine.Object.Destroy(feedbackPanel);

        feedbackPanel = null;

        int next = currentIndex + 1;

        if (next < questions.Count)
            ShowQuestion(next);
        else
            GoToResult();
    }

    // ───────────────── TIMER ─────────────────

    private IEnumerator RunTimer()
    {
        while (timeRemaining > 0 && !isAnswered)
        {
            if (timerTxt)
            {
                int sec =
                    Mathf.CeilToInt(timeRemaining);

                timerTxt.text = sec.ToString();

                bool warn = timeRemaining <= 5f;

                timerTxt.color = warn
                    ? TOBATheme.Error
                    : TOBATheme.BatakGold;

                if (timerBg)
                {
                    timerBg.color = warn
                        ? new Color(0.5f, 0.05f, 0.05f, 0.6f)
                        : new Color(0f, 0f, 0f, 0.35f);
                }

                if (warn &&
                    !isPaused &&
                    sec != lastWarnSecond)
                {
                    lastWarnSecond = sec;

                    SoundManager.Instance?.PlaySFX(
                        "sfx_timer_warning"
                    );
                }
            }

            if (!isPaused)
                timeRemaining -= Time.deltaTime;

            yield return null;
        }

        timerCo = null;

        if (!isAnswered && screenObj != null)
        {
            isAnswered = true;

            HapticManager.Instance?.Vibrate(
                HapticType.Error
            );

            var q = questions[currentIndex];

            HighlightOptions(q, -1);

            SetCharacterExpression(false);

            feedbackCo =
                ScreenManager.Instance.StartCoroutine(
                    ShowFeedbackThenNext(
                        false,
                        "Waktu habis! " + q.explanation
                    )
                );
        }
    }

    // ───────────────── KONFIRMASI KELUAR ─────────────────

    private void ShowExitDialog()
    {
        if (screenObj == null || exitDialog != null)
            return;

        isPaused = true;

        exitDialog = new GameObject("ExitDialog");

        exitDialog.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform dimRt =
            exitDialog.AddComponent<RectTransform>();

        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        Image dim = exitDialog.AddComponent<Image>();

        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;

        GameObject card = new GameObject("DialogCard");

        card.transform.SetParent(
            exitDialog.transform,
            false
        );

        RectTransform cardRt =
            card.AddComponent<RectTransform>();

        cardRt.anchorMin =
            new Vector2(0.08f, 0.355f);

        cardRt.anchorMax =
            new Vector2(0.92f, 0.645f);

        cardRt.offsetMin = Vector2.zero;
        cardRt.offsetMax = Vector2.zero;

        Image cardImg = card.AddComponent<Image>();

        cardImg.sprite = GetRoundedSprite();
        cardImg.type = Image.Type.Sliced;
        cardImg.color = TOBATheme.BatakGold;
        cardImg.raycastTarget = false;

        Shadow cardShadow =
            card.AddComponent<Shadow>();

        cardShadow.effectColor =
            new Color(0f, 0f, 0f, 0.5f);

        cardShadow.effectDistance =
            new Vector2(0f, -8f);

        Image brown = CreateInsetImage(
            card.transform,
            "DialogBorder",
            TOBATheme.CardBorder,
            5f
        );

        Image parchment = CreateInsetImage(
            brown.transform,
            "DialogParchment",
            TOBATheme.CardBG,
            4f
        );

        Transform p = parchment.transform;

        TextMeshProUGUI title = MakeText(
            p,
            "KELUAR DARI KUIS?",
            54,
            TOBATheme.WoodDark,
            TextAlignmentOptions.Center,
            new Vector2(860f, 90f),
            new Vector2(0f, 170f),
            FontStyles.Bold
        );

        title.enableAutoSizing = true;
        title.fontSizeMin = 30;
        title.fontSizeMax = 54;

        TextMeshProUGUI msg = MakeText(
            p,
            "Progres kuis ini akan hilang dan hadiahnya tidak akan didapat.",
            38,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(820f, 170f),
            new Vector2(0f, 15f),
            FontStyles.Normal
        );

        msg.textWrappingMode =
            TextWrappingModes.Normal;

        msg.enableAutoSizing = true;
        msg.fontSizeMin = 28;
        msg.fontSizeMax = 38;

        CreateDialogButton(
            p,
            "LANJUT MAIN",
            new Vector2(-215f, -175f),
            TOBATheme.BtnPrimary,
            Color.white,
            CloseExitDialog
        );

        CreateDialogButton(
            p,
            "KELUAR",
            new Vector2(215f, -175f),
            TOBATheme.BtnDanger,
            Color.white,
            ExitQuiz
        );
    }

    private void CreateDialogButton(
        Transform parent,
        string label,
        Vector2 pos,
        Color faceColor,
        Color textColor,
        System.Action onClick)
    {
        GameObject root =
            new GameObject("Btn_" + label);

        root.transform.SetParent(parent, false);

        RectTransform rt =
            root.AddComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax =
            new Vector2(0.5f, 0.5f);

        rt.sizeDelta = new Vector2(410f, 115f);
        rt.anchoredPosition = pos;

        Image frame = root.AddComponent<Image>();

        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(0f, 0f, 0f, 0.4f);

        shadow.effectDistance =
            new Vector2(0f, -5f);

        Image face = CreateInsetImage(
            root.transform,
            "Face",
            faceColor,
            5f
        );

        TextMeshProUGUI txt = MakeText(
            face.transform,
            label,
            42,
            textColor,
            TextAlignmentOptions.Center,
            new Vector2(380f, 100f),
            Vector2.zero,
            FontStyles.Bold
        );

        txt.enableAutoSizing = true;
        txt.fontSizeMin = 24;
        txt.fontSizeMax = 42;

        Button btn = root.AddComponent<Button>();

        btn.targetGraphic = face;

        ColorBlock colors = btn.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color(1f, 0.95f, 0.8f, 1f);

        colors.pressedColor =
            new Color(0.8f, 0.7f, 0.5f, 1f);

        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            PlayClick();
            onClick?.Invoke();
        });
    }

    private void CloseExitDialog()
    {
        if (exitDialog != null)
            UnityEngine.Object.Destroy(exitDialog);

        exitDialog = null;
        isPaused = false;
    }

    private void ExitQuiz()
    {
        isPaused = false;

        ScreenManager.Instance.ChangeScreen(
            new LocationDetailScreen(
                container,
                locationData
            )
        );
    }

    // ───────────────── HASIL ─────────────────

    private void GoToResult()
    {
        if (screenObj == null)
            return;

        ScreenManager.Instance.ChangeScreen(
            new QuizResultScreen(
                container,
                locationData,
                correctCount,
                totalQuestions
            )
        );
    }

    // ───────────────── HIDE ─────────────────

    public override void Hide()
    {
        if (ScreenManager.Instance != null)
        {
            if (timerCo != null)
                ScreenManager.Instance.StopCoroutine(timerCo);

            if (feedbackCo != null)
                ScreenManager.Instance.StopCoroutine(feedbackCo);
        }

        timerCo = null;
        feedbackCo = null;

        base.Hide();
    }
}
