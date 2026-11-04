using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class TriviaQuizScreen : BaseScreen
{
    private TriviaCategoryData categoryData;
    private List<QuizQuestionData> questions = new List<QuizQuestionData>();

    private int currentIndex;
    private int correctCount;

    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI questionTxt;
    private TextMeshProUGUI timerTxt;
    private RectTransform progressFill;
    private GameObject optionsContainer;
    private GameObject feedbackPanel;
    private GameObject exitDialog;
    private bool isPaused;

    private readonly float timeLimit = 15f;
    private float timeRemaining;
    private bool isAnswered;
    private Coroutine timerCoroutine;
    private Coroutine feedbackCoroutine;

    // Sprite membulat dibuat lewat kode (tanpa aset tambahan)
    private static Sprite roundedSprite;
    private static Sprite circleSprite;
    private static Sprite chevronSprite;

    // ───────── PAPAN HEADER (aset AI di Resources/gambar/) ─────────
    // Area papan header (anchor 0-1 terhadap layar)
    private const float HeaderMinX = 0.15f;
    private const float HeaderMaxX = 0.85f;
    private const float HeaderMinY = 0.865f;
    private const float HeaderMaxY = 0.985f;

    // Titik tengah bagian KAYU pada gambar papan, dihitung dari bawah gambar
    // (kayu berpusat di y=392 dari 724 px -> 1 - 392/724 = 0.459)
    private const float WoodCenterFromBottom = 0.459f;

    // Titik tengah vertikal kayu di layar (dipakai tombol kembali)
    private static float HeaderCenterY =>
        HeaderMinY + (HeaderMaxY - HeaderMinY) * WoodCenterFromBottom;

    // Sprite tombol kembali (sama dengan layar lain), path di dalam folder Resources
    private const string BackSpritePath = "RegisterNameScreen/register_name_back_button_v2";
    private const float BackButtonSize = 120f;
    private const float BackButtonLeft = 30f;

    public TriviaQuizScreen(Transform parent, TriviaCategoryData cat)
        : base(parent, "TriviaQuiz")
    {
        categoryData = cat;
    }

    public override void Show()
    {
        CreateTriviaBackground();
        CreateHeader();
        CreateBackButton();
        CreateProgressArea();
        CreateQuestionArea();
        CreateOptionsArea();
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
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, radius, size - 1 - radius);
                float cy = Mathf.Clamp(y, radius, size - 1 - radius);
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float a = Mathf.Clamp01(radius - d + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
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

    // Membuat anak Image yang mengisi parent dengan inset (px)
    private Image CreateInsetImage(Transform parent, string name, Color color, float inset)
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

    // ───────────────── BACKGROUND ─────────────────
    private void CreateTriviaBackground()
    {
        GameObject bg = new GameObject("TriviaBackground");
        bg.transform.SetParent(screenObj.transform, false);

        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = bg.AddComponent<Image>();

        // Coba trivia_bg dulu, kalau tidak ada pakai bg_toba
        Sprite sprite = Resources.Load<Sprite>("Backgrounds/trivia_bg");
        if (sprite == null) sprite = Resources.Load<Sprite>("bg_toba");

        img.sprite = sprite;
        img.color = sprite != null ? Color.white : TOBATheme.ForestGreen;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = false;
        bg.transform.SetAsFirstSibling();

        // Lapisan gelap tipis agar teks tetap terbaca
        GameObject shade = new GameObject("BackgroundShade");
        shade.transform.SetParent(screenObj.transform, false);

        RectTransform shadeRt = shade.AddComponent<RectTransform>();
        shadeRt.anchorMin = Vector2.zero;
        shadeRt.anchorMax = Vector2.one;
        shadeRt.offsetMin = Vector2.zero;
        shadeRt.offsetMax = Vector2.zero;

        Image shadeImg = shade.AddComponent<Image>();
        shadeImg.color = new Color(0.02f, 0.08f, 0.08f, 0.35f);
        shadeImg.raycastTarget = false;
    }

    // ───────────────── HEADER (PAPAN AI PER KATEGORI) ─────────────────
    private static Sprite LoadBoardSprite(string categoryName, out bool hasBakedText)
    {
        string key = (categoryName ?? "").ToLowerInvariant();
        string file = null;

        if (key.Contains("makanan")) file = "papan_makanan_tradisional";
        else if (key.Contains("pakaian")) file = "papan_pakaian_adat";
        else if (key.Contains("musik")) file = "papan_musik_tradisional";
        else if (key.Contains("wisata")) file = "papan_tempat_wisata";

        if (file != null)
        {
            Sprite s = Resources.Load<Sprite>("gambar/" + file);
            if (s != null)
            {
                hasBakedText = true;   // tulisan sudah ada di gambar
                return s;
            }
        }

        hasBakedText = false;          // papan polos + judul dari kode
        return Resources.Load<Sprite>("gambar/papan_polos");
    }

    private void CreateHeader()
    {
        string title = categoryData != null &&
                       !string.IsNullOrEmpty(categoryData.name)
            ? categoryData.name.ToUpper()
            : "TRIVIA BUDAYA";

        bool baked;
        Sprite board = LoadBoardSprite(
            categoryData != null ? categoryData.name : "", out baked);

        GameObject header = new GameObject("TriviaBoardHeader");
        header.transform.SetParent(screenObj.transform, false);

        RectTransform rt = header.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(HeaderMinX, HeaderMinY);
        rt.anchorMax = new Vector2(HeaderMaxX, HeaderMaxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = header.AddComponent<Image>();
        img.sprite = board;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;      // tidak gepeng / melar
        img.color = board != null ? Color.white : TOBATheme.WoodMedium;
        img.raycastTarget = false;

        // Papan polos dipakai (kategori tanpa papan khusus) -> tulis judul dari kode
        if (!baked)
        {
            GameObject titleObj = UIBuilder.CreateTextSized(
                header.transform,
                title,
                44,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(480f, 90f),
                new Vector2(0f, -9f)    // kayu sedikit di bawah tengah rect
            );

            TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = 22;
            titleTxt.fontSizeMax = 44;
            titleTxt.raycastTarget = false;
        }
    }

    // ───────────────── PROGRESS + TIMER ─────────────────
    private void CreateProgressArea()
    {
        GameObject bar = CreateAnchoredPanel(
            "ProgressBar",
            new Vector2(0.055f, 0.800f),
            new Vector2(0.945f, 0.855f),
            TOBATheme.WoodDark
        );

        Outline outline = bar.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2f, 2f);

        // Jalur progres emas di bagian bawah bar
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

        progressTxt = UIBuilder.CreateTextSized(
            bar.transform,
            "SOAL 0 / 0",
            38,
            TOBATheme.TextLight,
            TextAlignmentOptions.Left,
            new Vector2(520, 90),
            new Vector2(-180f, 8f)
        ).GetComponent<TextMeshProUGUI>();

        progressTxt.fontStyle = FontStyles.Bold;
        progressTxt.raycastTarget = false;

        timerTxt = UIBuilder.CreateTextSized(
            bar.transform,
            "15",
            46,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Right,
            new Vector2(180, 90),
            new Vector2(330f, 8f)
        ).GetComponent<TextMeshProUGUI>();

        timerTxt.fontStyle = FontStyles.Bold;
        timerTxt.raycastTarget = false;
    }

    private void SetProgressFill(int current, int total)
    {
        if (progressFill == null) return;

        float p = Mathf.Clamp01((float)current / Mathf.Max(1, total));
        progressFill.anchorMax = new Vector2(Mathf.Max(0.04f, p), 1f);
    }

    // ───────────────── KARTU PERTANYAAN ─────────────────
    private void CreateQuestionArea()
    {
        // Lapis 1: emas (luar)
        GameObject card = CreateAnchoredPanel(
            "QuestionCard",
            new Vector2(0.065f, 0.605f),
            new Vector2(0.935f, 0.785f),
            TOBATheme.BatakGold
        );

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -7f);

        // Lapis 2: cokelat
        Image brown = CreateInsetImage(
            card.transform, "CardBorder", TOBATheme.CardBorder, 5f);

        // Lapis 3: perkamen krem (atau panel_parchment kalau ada)
        Image parchmentImg = CreateInsetImage(
            brown.transform, "CardParchment", TOBATheme.CardBG, 4f);

        Sprite parchment = TOBATheme.LoadSprite("panel_parchment");
        if (parchment != null)
        {
            parchmentImg.sprite = parchment;
            parchmentImg.type = parchment.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;
            parchmentImg.color = Color.white;
        }

        questionTxt = UIBuilder.CreateTextSized(
            parchmentImg.transform,
            "Memuat soal...",
            39,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(900, 300),
            Vector2.zero
        ).GetComponent<TextMeshProUGUI>();

        questionTxt.fontStyle = FontStyles.Bold;
        questionTxt.textWrappingMode = TextWrappingModes.Normal;
        questionTxt.overflowMode = TextOverflowModes.Ellipsis;
        questionTxt.enableAutoSizing = true;
        questionTxt.fontSizeMin = 32;
        questionTxt.fontSizeMax = 48;
        questionTxt.margin = new Vector4(20, 15, 20, 15);
        questionTxt.raycastTarget = false;
    }

    private void CreateOptionsArea()
    {
        optionsContainer = new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(screenObj.transform, false);

        RectTransform rt = optionsContainer.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.055f, 0.19f);
        rt.anchorMax = new Vector2(0.945f, 0.585f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

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

    // ───────────────── TOMBOL KEMBALI + KONFIRMASI KELUAR ─────────────────
    private static Sprite GetChevronSprite()
    {
        if (chevronSprite != null) return chevronSprite;

        const int size = 128;
        Vector2 a = new Vector2(86f, 24f);
        Vector2 b = new Vector2(42f, 64f);
        Vector2 c = new Vector2(86f, 104f);
        const float half = 11f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(
                    DistToSegment(p, a, b),
                    DistToSegment(p, b, c));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f,
                    Mathf.Clamp01(half - d + 0.5f)));
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

    private static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }

    // Tombol kembali memakai sprite yang sama dengan layar lain.
    // Titik tengahnya sejajar dengan titik tengah kayu papan.
    private void CreateBackButton()
    {
        Sprite backSprite = Resources.Load<Sprite>(BackSpritePath);

        if (backSprite == null)
        {
            Debug.LogWarning("[TriviaQuiz] Sprite tombol kembali '" +
                BackSpritePath + "' tidak ditemukan, pakai tombol bawaan.");
            CreateBackButtonFallback();
            return;
        }

        GameObject root = new GameObject("BtnBack");
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, HeaderCenterY);
        rt.anchorMax = new Vector2(0f, HeaderCenterY);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(BackButtonSize, BackButtonSize);
        rt.anchoredPosition = new Vector2(BackButtonLeft, 0f);

        Image img = root.AddComponent<Image>();
        img.sprite = backSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = Color.white;
        img.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.5f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            ShowExitDialog();
        });
    }

    // Cadangan: tombol digambar lewat kode kalau sprite tidak ditemukan
    private void CreateBackButtonFallback()
    {
        GameObject root = new GameObject("BtnBack");
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, HeaderCenterY);
        rt.anchorMax = new Vector2(0f, HeaderCenterY);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(104f, 104f);
        rt.anchoredPosition = new Vector2(40f, 0f);   // jarak dari tepi kiri layar

        Image frame = root.AddComponent<Image>();
        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -4f);

        Image face = CreateInsetImage(
            root.transform, "Face", TOBATheme.WoodDark, 4f);

        GameObject icon = new GameObject("Chevron");
        icon.transform.SetParent(face.transform, false);

        RectTransform iconRt = icon.AddComponent<RectTransform>();
        iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
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
        colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.5f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            ShowExitDialog();
        });
    }

    private void ShowExitDialog()
    {
        if (screenObj == null || exitDialog != null) return;

        isPaused = true;

        // Lapisan gelap yang menahan semua sentuhan di belakangnya
        exitDialog = new GameObject("ExitDialog");
        exitDialog.transform.SetParent(screenObj.transform, false);

        RectTransform dimRt = exitDialog.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        Image dim = exitDialog.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;

        // Kartu: emas, cokelat, perkamen
        GameObject card = new GameObject("DialogCard");
        card.transform.SetParent(exitDialog.transform, false);

        RectTransform cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.08f, 0.355f);
        cardRt.anchorMax = new Vector2(0.92f, 0.645f);
        cardRt.offsetMin = Vector2.zero;
        cardRt.offsetMax = Vector2.zero;

        Image cardImg = card.AddComponent<Image>();
        cardImg.sprite = GetRoundedSprite();
        cardImg.type = Image.Type.Sliced;
        cardImg.color = TOBATheme.BatakGold;
        cardImg.raycastTarget = false;

        Shadow cardShadow = card.AddComponent<Shadow>();
        cardShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        cardShadow.effectDistance = new Vector2(0f, -8f);

        Image brown = CreateInsetImage(
            card.transform, "DialogBorder", TOBATheme.CardBorder, 5f);
        Image parchment = CreateInsetImage(
            brown.transform, "DialogParchment", TOBATheme.CardBG, 4f);

        Transform p = parchment.transform;

        TextMeshProUGUI title = UIBuilder.CreateTextSized(
            p,
            "KELUAR DARI KUIS?",
            54,
            TOBATheme.WoodDark,
            TextAlignmentOptions.Center,
            new Vector2(860f, 90f),
            new Vector2(0f, 170f)
        ).GetComponent<TextMeshProUGUI>();
        title.fontStyle = FontStyles.Bold;
        title.enableAutoSizing = true;
        title.fontSizeMin = 30;
        title.fontSizeMax = 54;
        title.raycastTarget = false;

        TextMeshProUGUI msg = UIBuilder.CreateTextSized(
            p,
            "Progres kuis ini akan hilang dan XP tidak akan didapat.",
            38,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(820f, 170f),
            new Vector2(0f, 15f)
        ).GetComponent<TextMeshProUGUI>();
        msg.fontStyle = FontStyles.Normal;
        msg.textWrappingMode = TextWrappingModes.Normal;
        msg.enableAutoSizing = true;
        msg.fontSizeMin = 28;
        msg.fontSizeMax = 38;
        msg.raycastTarget = false;

        CreateDialogButton(
            p, "LANJUT MAIN", new Vector2(-215f, -175f),
            TOBATheme.BtnPrimary, Color.white,
            CloseExitDialog);

        CreateDialogButton(
            p, "KELUAR", new Vector2(215f, -175f),
            TOBATheme.BtnDanger, Color.white,
            ExitQuiz);
    }

    private void CreateDialogButton(
        Transform parent,
        string label,
        Vector2 pos,
        Color faceColor,
        Color textColor,
        Action onClick)
    {
        GameObject root = new GameObject("Btn_" + label);
        root.transform.SetParent(parent, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(410f, 115f);
        rt.anchoredPosition = pos;

        Image frame = root.AddComponent<Image>();
        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(0f, -5f);

        Image face = CreateInsetImage(root.transform, "Face", faceColor, 5f);

        TextMeshProUGUI txt = UIBuilder.CreateTextSized(
            face.transform,
            label,
            42,
            textColor,
            TextAlignmentOptions.Center,
            new Vector2(380f, 100f),
            Vector2.zero
        ).GetComponent<TextMeshProUGUI>();
        txt.fontStyle = FontStyles.Bold;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 24;
        txt.fontSizeMax = 42;
        txt.raycastTarget = false;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = face;

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.5f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            if (onClick != null)
                onClick();
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

        if (ScreenManager.Instance != null)
            ScreenManager.Instance.ChangeScreen(new TriviaMenuScreen(container));
    }

    // ───────────────── FETCH SOAL ─────────────────
    private void FetchQuiz()
    {
        if (categoryData == null)
        {
            SetQuestionMessage("Kategori kuis tidak ditemukan.");
            Debug.LogError("TriviaQuizScreen: categoryData kosong.");
            return;
        }

        if (APIManager.Instance == null)
        {
            SetQuestionMessage("Koneksi belum siap. Silakan coba lagi.");
            Debug.LogError("TriviaQuizScreen: APIManager tidak ditemukan.");
            return;
        }

        SetQuestionMessage("Memuat soal...");

        APIManager.Instance.GetRequest(
            "/trivia/" + categoryData.id + "/quiz",
            response =>
            {
                if (screenObj == null) return;

                TriviaQuizResponse res =
                    JsonUtility.FromJson<TriviaQuizResponse>(response);

                if (res != null &&
                    res.questions != null &&
                    res.questions.Count > 0)
                {
                    questions = res.questions;
                    currentIndex = 0;
                    correctCount = 0;
                    ShowQuestion(0);
                }
                else
                {
                    SetQuestionMessage("Soal belum tersedia.");
                }
            },
            err =>
            {
                Debug.LogError("Gagal memuat soal trivia: " + err);

                if (screenObj != null)
                    SetQuestionMessage(
                        "Gagal memuat soal.\nPeriksa koneksi lalu coba lagi."
                    );
            }
        );
    }

    private void SetQuestionMessage(string message)
    {
        if (questionTxt != null)
            questionTxt.text = message;
    }

    // ───────────────── TAMPILKAN SOAL ─────────────────
    private void ShowQuestion(int index)
    {
        if (screenObj == null ||
            questions == null ||
            index < 0 ||
            index >= questions.Count)
            return;

        QuizQuestionData q = questions[index];

        if (q == null || q.options == null || q.options.Count == 0)
        {
            SetQuestionMessage("Pilihan jawaban belum tersedia.");
            return;
        }

        isAnswered = false;
        currentIndex = index;

        if (feedbackPanel != null)
        {
            UnityEngine.Object.Destroy(feedbackPanel);
            feedbackPanel = null;
        }

        if (progressTxt != null)
            progressTxt.text =
                "SOAL " + (index + 1) + " / " + questions.Count;

        SetProgressFill(index + 1, questions.Count);

        if (questionTxt != null)
            questionTxt.text = q.question ?? "Pertanyaan tidak tersedia.";

        // FIX: hapus tombol soal sebelumnya (bukan feedbackPanel)
        foreach (Transform child in optionsContainer.transform)
            UnityEngine.Object.Destroy(child.gameObject);

        int count = Mathf.Min(q.options.Count, 4);
        string[] labels = { "A", "B", "C", "D" };

        RectTransform containerRt =
            optionsContainer.GetComponent<RectTransform>();

        float containerHeight = containerRt.rect.height;
        float spacing = Mathf.Min(190f, containerHeight / Mathf.Max(1, count));
        float startY = ((count - 1) * spacing) / 2f;

        for (int i = 0; i < count; i++)
        {
            QuizOptionData opt = q.options[i];
            if (opt == null) continue;

            int captured = i;

            CreateOptionButton(
                opt,
                labels[i],
                startY - i * spacing,
                () =>
                {
                    if (!isAnswered)
                        OnAnswerSelected(opt, captured, q);
                }
            );
        }

        if (timerCoroutine != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        timeRemaining = timeLimit;

        if (ScreenManager.Instance != null)
            timerCoroutine =
                ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    // ───────────────── TOMBOL JAWABAN ─────────────────
    private void CreateOptionButton(
        QuizOptionData opt,
        string label,
        float yPos,
        Action onClick)
    {
        // Root = bingkai emas
        GameObject option = new GameObject("Option_" + label);
        option.transform.SetParent(optionsContainer.transform, false);

        RectTransform rt = option.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(980f, 150f);
        rt.anchoredPosition = new Vector2(0f, yPos);

        Image frame = option.AddComponent<Image>();
        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = option.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(0f, -6f);

        // Face = permukaan krem (warna berubah saat benar/salah)
        Image face = CreateInsetImage(
            option.transform, "Face", TOBATheme.OptionNormal, 4f);

        // Lencana huruf (lingkaran cokelat bingkai emas)
        GameObject badge = new GameObject("LetterBadge");
        badge.transform.SetParent(face.transform, false);

        RectTransform badgeRt = badge.AddComponent<RectTransform>();
        badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0f, 0.5f);
        badgeRt.pivot = new Vector2(0f, 0.5f);
        badgeRt.sizeDelta = new Vector2(92f, 92f);
        badgeRt.anchoredPosition = new Vector2(16f, 0f);

        Image badgeImg = badge.AddComponent<Image>();
        badgeImg.sprite = GetCircleSprite();
        badgeImg.type = Image.Type.Simple;
        badgeImg.color = TOBATheme.WoodMedium;
        badgeImg.raycastTarget = false;

        Outline badgeOutline = badge.AddComponent<Outline>();
        badgeOutline.effectColor = TOBATheme.BatakGold;
        badgeOutline.effectDistance = new Vector2(3f, 3f);

        GameObject letterObj = UIBuilder.CreateTextSized(
            badge.transform,
            label,
            42,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(92f, 92f),
            Vector2.zero
        );

        TextMeshProUGUI letter = letterObj.GetComponent<TextMeshProUGUI>();
        letter.fontStyle = FontStyles.Bold;
        letter.raycastTarget = false;

        // Teks jawaban
        GameObject optionTextObj = UIBuilder.CreateTextSized(
            face.transform,
            opt.option_text ?? "",
            34,
            TOBATheme.TextDark,
            TextAlignmentOptions.Left,
            new Vector2(760f, 126f),
            new Vector2(70f, 0f)
        );
        optionTextObj.name = "OptionText";

        TextMeshProUGUI optionText =
            optionTextObj.GetComponent<TextMeshProUGUI>();
        optionText.fontStyle = FontStyles.Bold;
        optionText.textWrappingMode = TextWrappingModes.Normal;
        optionText.overflowMode = TextOverflowModes.Ellipsis;
        optionText.enableAutoSizing = true;
        optionText.fontSizeMin = 30;
        optionText.fontSizeMax = 42;
        optionText.margin = new Vector4(8, 5, 8, 5);
        optionText.raycastTarget = false;

        Button button = option.AddComponent<Button>();
        button.targetGraphic = face;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
        colors.pressedColor = new Color(0.88f, 0.76f, 0.52f, 1f);
        colors.disabledColor = Color.white;
        button.colors = colors;

        button.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            if (onClick != null)
                onClick();
        });
    }

    // ───────────────── JAWAB ─────────────────
    private void OnAnswerSelected(
        QuizOptionData selected,
        int selectedIndex,
        QuizQuestionData question)
    {
        if (isAnswered) return;

        isAnswered = true;

        if (timerCoroutine != null && ScreenManager.Instance != null)
        {
            ScreenManager.Instance.StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (selected.is_correct)
            correctCount++;

        HighlightOptions(question, selectedIndex);

        if (ScreenManager.Instance != null)
        {
            feedbackCoroutine = ScreenManager.Instance.StartCoroutine(
                ShowFeedbackThenNext(
                    selected.is_correct,
                    question.explanation
                )
            );
        }
    }

    private void HighlightOptions(
        QuizQuestionData question,
        int selectedIndex)
    {
        if (optionsContainer == null ||
            question == null ||
            question.options == null)
            return;

        for (int i = 0; i < optionsContainer.transform.childCount; i++)
        {
            Transform child = optionsContainer.transform.GetChild(i);

            if (i >= question.options.Count) continue;

            Transform faceTr = child.Find("Face");
            Image face = faceTr != null ? faceTr.GetComponent<Image>() : null;
            Button btn = child.GetComponent<Button>();

            Transform textTr = faceTr != null ? faceTr.Find("OptionText") : null;
            TextMeshProUGUI txt = textTr != null
                ? textTr.GetComponent<TextMeshProUGUI>()
                : null;

            if (face != null)
            {
                if (question.options[i].is_correct)
                {
                    face.color = TOBATheme.OptionCorrect;
                    if (txt != null) txt.color = Color.white;
                }
                else if (i == selectedIndex)
                {
                    face.color = TOBATheme.OptionWrong;
                    if (txt != null) txt.color = Color.white;
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

            if (btn != null)
                btn.interactable = false;
        }
    }

    // ───────────────── FEEDBACK ─────────────────
    private IEnumerator ShowFeedbackThenNext(
        bool isCorrect,
        string explanation)
    {
        if (screenObj == null) yield break;

        feedbackPanel = CreateAnchoredPanel(
            "FeedbackPanel",
            new Vector2(0.05f, 0.025f),
            new Vector2(0.95f, 0.18f),
            isCorrect ? TOBATheme.Success : TOBATheme.Error
        );

        Outline outline = feedbackPanel.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3f, 3f);

        Shadow shadow = feedbackPanel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(0f, -5f);

        TextMeshProUGUI resultTxt = UIBuilder.CreateTextSized(
            feedbackPanel.transform,
            isCorrect ? "BENAR!" : "SALAH!",
            46,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(880f, 60f),
            new Vector2(0f, 100f)
        ).GetComponent<TextMeshProUGUI>();

        resultTxt.fontStyle = FontStyles.Bold;
        resultTxt.raycastTarget = false;

        TextMeshProUGUI expTxt = UIBuilder.CreateTextSized(
            feedbackPanel.transform,
            string.IsNullOrEmpty(explanation)
                ? "Lanjut ke soal berikutnya."
                : explanation,
            30,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(880f, 190f),
            new Vector2(0f, -35f)
        ).GetComponent<TextMeshProUGUI>();

        expTxt.fontStyle = FontStyles.Normal;
        expTxt.textWrappingMode = TextWrappingModes.Normal;
        expTxt.overflowMode = TextOverflowModes.Ellipsis;
        expTxt.enableAutoSizing = true;
        expTxt.fontSizeMin = 20;
        expTxt.fontSizeMax = 30;
        expTxt.margin = new Vector4(10, 5, 10, 5);
        expTxt.raycastTarget = false;

        float waited = 0f;
        while (waited < 2.5f)
        {
            if (screenObj == null) yield break;
            if (!isPaused) waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (screenObj == null) yield break;

        if (feedbackPanel != null)
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
        timeRemaining = timeLimit;

        while (timeRemaining > 0f && !isAnswered)
        {
            if (timerTxt != null)
            {
                timerTxt.text = Mathf.CeilToInt(timeRemaining).ToString();
                timerTxt.color = timeRemaining <= 5f
                    ? TOBATheme.Error
                    : TOBATheme.BatakGold;
            }

            if (!isPaused) timeRemaining -= Time.deltaTime;
            yield return null;
        }

        timerCoroutine = null;

        if (!isAnswered &&
            screenObj != null &&
            questions.Count > currentIndex)
        {
            isAnswered = true;

            QuizQuestionData q = questions[currentIndex];
            HighlightOptions(q, -1);

            if (ScreenManager.Instance != null)
            {
                feedbackCoroutine = ScreenManager.Instance.StartCoroutine(
                    ShowFeedbackThenNext(
                        false,
                        "Waktu habis! " + (q.explanation ?? "")
                    )
                );
            }
        }
    }

    private void GoToResult()
    {
        if (screenObj == null || ScreenManager.Instance == null)
            return;

        ScreenManager.Instance.ChangeScreen(
            new TriviaResultScreen(
                container,
                categoryData,
                correctCount,
                questions.Count
            )
        );
    }

    public override void Hide()
    {
        if (ScreenManager.Instance != null)
        {
            if (timerCoroutine != null)
                ScreenManager.Instance.StopCoroutine(timerCoroutine);

            if (feedbackCoroutine != null)
                ScreenManager.Instance.StopCoroutine(feedbackCoroutine);
        }

        timerCoroutine = null;
        feedbackCoroutine = null;

        base.Hide();
    }
}