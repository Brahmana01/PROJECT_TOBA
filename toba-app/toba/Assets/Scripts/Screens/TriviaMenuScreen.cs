using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class TriviaMenuScreen : BaseScreen
{
    private List<TriviaCategoryData> categories = new List<TriviaCategoryData>();

    private GameObject loadingObj;
    private GameObject emptyStateObj;

    // Carousel kategori: empat kartu per halaman (2 kolom x 2 baris).
    private const int CategoriesPerPage = 4;
    private int currentPage = 0;
    private int totalPages = 0;

    private GameObject carouselFrameObj;
    private RectTransform carouselViewport;
    private GameObject currentPageObj;

    private Button previousPageButton;
    private Button nextPageButton;
    private TextMeshProUGUI pageCounterText;

    private Tween pageTransitionTween;
    private bool isPageTransitioning = false;
    private bool isOpeningCategory = false;

    // ───────── HEADER: papan judul + tombol kembali ─────────
    // Area papan judul (anchor 0-1 terhadap layar)
    private const float HeaderMinY = 0.875f;
    private const float HeaderMaxY = 0.995f;

    // Titik tengah bagian KAYU pada gambar papan, dihitung dari bawah gambar
    // (kayu berpusat di y=392 dari 724 px -> 1 - 392/724 = 0.459)
    private const float WoodCenterFromBottom = 0.459f;

    // Titik tengah vertikal kayu di layar (dipakai tombol kembali)
    private static float HeaderCenterY =>
        HeaderMinY + (HeaderMaxY - HeaderMinY) * WoodCenterFromBottom;

    // Ukuran dan jarak tombol kembali (sama dengan layar kuis & hasil)
    private const float BackButtonSize = 120f;
    private const float BackButtonLeft = 30f;

    private static Sprite roundedSprite;
    private static Sprite chevronSprite;
    private static Sprite circleSprite;

    public TriviaMenuScreen(Transform parent) : base(parent, "TriviaMenu") { }

    public override void Show()
    {
        CreateBackground();
        CreateHeader();
        CreateRibbon();
        CreateCarouselFrame();
        CreateHintPill();

        ShowLoading();
        FetchCategories();
    }

    // ───────────────── SPRITE HELPER ─────────────────
    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite == null)
            roundedSprite = BuildRoundedSprite(128, 44, 48);
        return roundedSprite;
    }

    private static Sprite BuildRoundedSprite(int size, int radius, int border)
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

    // Sprite lingkaran untuk tombol carousel dan indikator halaman.
    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;

        const int size = 96;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 0.75f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        circleSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return circleSprite;
    }

    // Ikon panah kiri digambar lewat kode (tidak bergantung pada font)
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
                float alpha = Mathf.Clamp01(half - d + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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

    // Pil berbingkai emas dengan isi berwarna. Mengembalikan objek bingkai.
    private GameObject CreateFramedPill(
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color faceColor,
        out Image face)
    {
        GameObject frame = CreateAnchoredPanel(
            name, anchorMin, anchorMax, TOBATheme.BatakGold);

        Shadow shadow = frame.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -6f);

        face = CreateInsetImage(frame.transform, "Face", faceColor, 5f);

        // Animasi pop-in ringan agar panel terasa hidup, tetapi tetap selaras
        // dengan papan kayu dan ornamen emas pada Main Menu.
        CanvasGroup group = frame.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        frame.transform.localScale = Vector3.one * 0.94f;

        if (screenObj != null)
        {
            Sequence intro = DOTween.Sequence();
            intro.SetUpdate(true);
            intro.SetTarget(screenObj.transform);
            intro.Append(frame.transform.DOScale(1f, 0.32f).SetEase(Ease.OutBack));
            intro.Join(group.DOFade(1f, 0.24f));
        }

        return frame;
    }

    private TextMeshProUGUI MakeText(
        Transform parent,
        string text,
        int size,
        Color color,
        Vector2 box,
        Vector2 pos,
        FontStyles style)
    {
        TextMeshProUGUI t = UIBuilder.CreateTextSized(
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
        return t;
    }

    // ───────────────── BACKGROUND ─────────────────
    private void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(screenObj.transform, false);

        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = bg.AddComponent<Image>();

        Sprite sprite = Resources.Load<Sprite>("Backgrounds/trivia_bg");
        if (sprite == null) sprite = Resources.Load<Sprite>("bg_toba");

        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }
        else
        {
            Debug.LogWarning("Background trivia tidak ditemukan!");
            image.color = TOBATheme.ForestGreen;
        }

        image.raycastTarget = false;
        bg.transform.SetAsFirstSibling();

        GameObject shade = new GameObject("BackgroundShade");
        shade.transform.SetParent(screenObj.transform, false);

        RectTransform shadeRt = shade.AddComponent<RectTransform>();
        shadeRt.anchorMin = Vector2.zero;
        shadeRt.anchorMax = Vector2.one;
        shadeRt.offsetMin = Vector2.zero;
        shadeRt.offsetMax = Vector2.zero;

        Image shadeImg = shade.AddComponent<Image>();
        shadeImg.color = new Color(0.02f, 0.08f, 0.08f, 0.24f);
        shadeImg.raycastTarget = false;
    }

    // ───────────────── HEADER + TOMBOL KEMBALI ─────────────────

    // ───────────────── HEADER + TOMBOL KEMBALI ─────────────────
    private void CreateHeader()
    {
        GameObject headerContainer = new GameObject("TriviaHeaderContainer");
        headerContainer.transform.SetParent(screenObj.transform, false);

        RectTransform containerRT = headerContainer.AddComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // Papan judul bergambar yang sama dengan identitas visual TOBA.
        Sprite headerSprite = Resources.Load<Sprite>("gambar/trivia_header");

        if (headerSprite != null)
        {
            GameObject header = new GameObject("TriviaWoodHeader");
            header.transform.SetParent(headerContainer.transform, false);

            RectTransform headerRT = header.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.02f, HeaderMinY);
            headerRT.anchorMax = new Vector2(0.98f, HeaderMaxY);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            Image headerImg = header.AddComponent<Image>();
            headerImg.sprite = headerSprite;
            headerImg.type = Image.Type.Simple;
            headerImg.preserveAspect = true;
            headerImg.color = Color.white;
            headerImg.raycastTarget = false;

            headerRT.localScale = Vector3.one * 0.94f;

            Sequence headerIntro = DOTween.Sequence();
            headerIntro.SetUpdate(true);
            headerIntro.SetTarget(screenObj.transform);
            headerIntro.Append(headerRT.DOScale(1f, 0.42f).SetEase(Ease.OutBack));
        }
        else
        {
            Debug.LogWarning(
                "[Trivia] gambar/trivia_header tidak ditemukan. " +
                "Menggunakan header kayu cadangan."
            );

            Image fallbackFace;
            CreateFramedPill(
                "TriviaHeaderFallback",
                new Vector2(0.12f, HeaderMinY),
                new Vector2(0.88f, HeaderMaxY),
                TOBATheme.WoodDark,
                out fallbackFace
            );

            MakeText(
                fallbackFace.transform,
                "TRIVIA BUDAYA",
                42,
                TOBATheme.BatakGold,
                new Vector2(650f, 90f),
                Vector2.zero,
                FontStyles.Bold
            );
        }

        // Tombol kembali: gunakan sprite proyek jika tersedia.
        Sprite backSprite = TOBATheme.LoadSprite(
            "RegisterNameScreen/register_name_back_button_v2"
        );

        GameObject backObject = new GameObject(
            "BackButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        backObject.transform.SetParent(headerContainer.transform, false);

        RectTransform backRT = backObject.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0f, HeaderCenterY);
        backRT.anchorMax = new Vector2(0f, HeaderCenterY);
        backRT.pivot = new Vector2(0f, 0.5f);
        backRT.sizeDelta = new Vector2(BackButtonSize, BackButtonSize);
        backRT.anchoredPosition = new Vector2(BackButtonLeft, 0f);

        Image backImage = backObject.GetComponent<Image>();
        backImage.raycastTarget = true;
        backImage.preserveAspect = true;

        if (backSprite != null)
        {
            backImage.sprite = backSprite;
            backImage.color = Color.white;
        }
        else
        {
            backImage.sprite = GetCircleSprite();
            backImage.color = TOBATheme.BatakGold;

            GameObject inner = new GameObject(
                "BackButtonInner",
                typeof(RectTransform),
                typeof(Image)
            );
            inner.transform.SetParent(backObject.transform, false);

            RectTransform innerRT = inner.GetComponent<RectTransform>();
            innerRT.anchorMin = Vector2.zero;
            innerRT.anchorMax = Vector2.one;
            innerRT.offsetMin = new Vector2(7f, 7f);
            innerRT.offsetMax = new Vector2(-7f, -7f);

            Image innerImage = inner.GetComponent<Image>();
            innerImage.sprite = GetCircleSprite();
            innerImage.color = TOBATheme.WoodDark;
            innerImage.raycastTarget = false;

            GameObject arrow = new GameObject(
                "BackChevron",
                typeof(RectTransform),
                typeof(Image)
            );
            arrow.transform.SetParent(inner.transform, false);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(0.22f, 0.22f);
            arrowRT.anchorMax = new Vector2(0.78f, 0.78f);
            arrowRT.offsetMin = Vector2.zero;
            arrowRT.offsetMax = Vector2.zero;

            Image arrowImage = arrow.GetComponent<Image>();
            arrowImage.sprite = GetChevronSprite();
            arrowImage.color = TOBATheme.BatakGold;
            arrowImage.preserveAspect = true;
            arrowImage.raycastTarget = false;
        }

        Button backButton = backObject.GetComponent<Button>();
        backButton.targetGraphic = backImage;
        backButton.transition = Selectable.Transition.None;
        backButton.onClick.AddListener(() => GoBackToMainMenu(backObject.transform));

        // Gerakan mengambang sangat halus, tidak mengganggu tombol.
        backRT.DOAnchorPosY(3f, 1.15f)
            .SetRelative()
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetTarget(screenObj.transform);
    }

    private void GoBackToMainMenu(Transform target)
    {
        if (target == null || isOpeningCategory || isPageTransitioning)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("sfx_click");

        Sequence press = DOTween.Sequence();
        press.SetUpdate(true);
        press.SetTarget(screenObj.transform);
        press.Append(target.DOScale(0.86f, 0.07f).SetEase(Ease.OutQuad));
        press.Append(target.DOScale(1.06f, 0.10f).SetEase(Ease.OutBack));
        press.Append(target.DOScale(1f, 0.08f).SetEase(Ease.OutQuad));
        press.OnComplete(() =>
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.ChangeScreen(
                    new MainMenuScreen(container)
                );
            }
        });
    }

        // ───────────────── PITA JUDUL + PETUNJUK ─────────────────
    private void CreateRibbon()
    {
        Image face;
        CreateFramedPill(
            "Ribbon",
            new Vector2(0.14f, 0.775f),
            new Vector2(0.86f, 0.835f),
            TOBATheme.WoodDark,
            out face
        );

        TextMeshProUGUI txt = MakeText(
            face.transform,
            "PILIH KATEGORI KUIS",
            44,
            TOBATheme.BatakGold,
            new Vector2(720f, 100f),
            Vector2.zero,
            FontStyles.Bold
        );

        txt.enableAutoSizing = true;
        txt.fontSizeMin = 26;
        txt.fontSizeMax = 44;
    }

    private void CreateHintPill()
    {
        Image face;
        CreateFramedPill(
            "HintPill",
            new Vector2(0.1f, 0.127f),
            new Vector2(0.9f, 0.187f),
            TOBATheme.WoodDark,
            out face
        );

        TextMeshProUGUI txt = MakeText(
            face.transform,
            "Pilih kategori dan mulai petualangan budaya!",
            34,
            TOBATheme.TextLight,
            new Vector2(800f, 100f),
            Vector2.zero,
            FontStyles.Bold
        );

        txt.textWrappingMode = TextWrappingModes.Normal;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 22;
        txt.fontSizeMax = 34;
    }

    // ───────────────── CAROUSEL FRAME ─────────────────
    private void CreateCarouselFrame()
    {
        if (screenObj == null) return;

        // Bingkai utama area kategori.
        carouselFrameObj = new GameObject("TriviaCarouselFrame");
        carouselFrameObj.transform.SetParent(screenObj.transform, false);

        RectTransform frameRT = carouselFrameObj.AddComponent<RectTransform>();
        frameRT.anchorMin = new Vector2(0.105f, 0.265f);
        frameRT.anchorMax = new Vector2(0.895f, 0.735f);
        frameRT.offsetMin = Vector2.zero;
        frameRT.offsetMax = Vector2.zero;

        Image frameImage = carouselFrameObj.AddComponent<Image>();
        frameImage.sprite = GetRoundedSprite();
        frameImage.type = Image.Type.Sliced;
        frameImage.color = TOBATheme.BatakGold;
        frameImage.raycastTarget = false;

        Shadow frameShadow = carouselFrameObj.AddComponent<Shadow>();
        frameShadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
        frameShadow.effectDistance = new Vector2(0f, -7f);

        CanvasGroup frameGroup = carouselFrameObj.AddComponent<CanvasGroup>();
        frameGroup.alpha = 0f;
        frameRT.localScale = Vector3.one * 0.95f;

        Sequence frameIntro = DOTween.Sequence();
        frameIntro.SetUpdate(true);
        frameIntro.SetTarget(screenObj.transform);
        frameIntro.Append(frameRT.DOScale(1f, 0.38f).SetEase(Ease.OutBack));
        frameIntro.Join(frameGroup.DOFade(1f, 0.28f));

        // Lapisan dalam berwarna kayu/cokelat.
        Image innerFrame = CreateInsetImage(
            carouselFrameObj.transform,
            "CarouselInnerFrame",
            TOBATheme.CardBorder,
            7f
        );

        // Viewport menjadi area pemotong halaman. Kartu tidak keluar dari bingkai.
        GameObject viewportObj = new GameObject(
            "CategoryViewport",
            typeof(RectTransform),
            typeof(RectMask2D)
        );
        viewportObj.transform.SetParent(innerFrame.transform, false);

        carouselViewport = viewportObj.GetComponent<RectTransform>();
        carouselViewport.anchorMin = Vector2.zero;
        carouselViewport.anchorMax = Vector2.one;
        carouselViewport.offsetMin = new Vector2(17f, 17f);
        carouselViewport.offsetMax = new Vector2(-17f, -17f);

        // Tombol navigasi berada di sisi kiri dan kanan bingkai.
        previousPageButton = CreateCarouselArrow(
            "PreviousPageButton",
            false,
            new Vector2(0.052f, 0.50f)
        );

        nextPageButton = CreateCarouselArrow(
            "NextPageButton",
            true,
            new Vector2(0.948f, 0.50f)
        );

        // Indikator menggunakan format angka agar tetap rapi meskipun halaman banyak.
        GameObject counterObj = new GameObject("PageCounter");
        counterObj.transform.SetParent(screenObj.transform, false);

        RectTransform counterRT = counterObj.AddComponent<RectTransform>();
        counterRT.anchorMin = new Vector2(0.5f, 0.215f);
        counterRT.anchorMax = new Vector2(0.5f, 0.215f);
        counterRT.pivot = new Vector2(0.5f, 0.5f);
        counterRT.sizeDelta = new Vector2(260f, 42f);
        counterRT.anchoredPosition = Vector2.zero;

        pageCounterText = MakeText(
            counterObj.transform,
            "1 / 1",
            25,
            TOBATheme.BatakGold,
            new Vector2(260f, 42f),
            Vector2.zero,
            FontStyles.Bold
        );
    }

    private Button CreateCarouselArrow(
        string objectName,
        bool pointsRight,
        Vector2 screenAnchor)
    {
        GameObject arrowObj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        arrowObj.transform.SetParent(screenObj.transform, false);

        RectTransform arrowRT = arrowObj.GetComponent<RectTransform>();
        arrowRT.anchorMin = screenAnchor;
        arrowRT.anchorMax = screenAnchor;
        arrowRT.pivot = new Vector2(0.5f, 0.5f);
        arrowRT.sizeDelta = new Vector2(94f, 94f);
        arrowRT.anchoredPosition = Vector2.zero;

        // Lingkaran emas sebagai bingkai tombol.
        Image outerImage = arrowObj.GetComponent<Image>();
        outerImage.sprite = GetCircleSprite();
        outerImage.color = TOBATheme.BatakGold;
        outerImage.raycastTarget = true;

        // Lingkaran kayu di bagian dalam.
        GameObject innerObj = new GameObject(
            "InnerCircle",
            typeof(RectTransform),
            typeof(Image)
        );
        innerObj.transform.SetParent(arrowObj.transform, false);

        RectTransform innerRT = innerObj.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(6f, 6f);
        innerRT.offsetMax = new Vector2(-6f, -6f);

        Image innerImage = innerObj.GetComponent<Image>();
        innerImage.sprite = GetCircleSprite();
        innerImage.color = TOBATheme.WoodDark;
        innerImage.raycastTarget = false;

        // Sprite chevron bawaan menghadap kiri; tombol kanan diputar 180 derajat.
        GameObject iconObj = new GameObject(
            "Chevron",
            typeof(RectTransform),
            typeof(Image)
        );
        iconObj.transform.SetParent(innerObj.transform, false);

        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.22f, 0.22f);
        iconRT.anchorMax = new Vector2(0.78f, 0.78f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.sprite = GetChevronSprite();
        iconImage.preserveAspect = true;
        iconImage.color = TOBATheme.BatakGold;
        iconImage.raycastTarget = false;

        if (pointsRight)
            iconRT.localRotation = Quaternion.Euler(0f, 0f, 180f);

        Button button = arrowObj.GetComponent<Button>();
        button.targetGraphic = outerImage;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.92f, 0.68f, 1f);
        colors.pressedColor = new Color(0.82f, 0.68f, 0.38f, 1f);
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        button.colors = colors;

        button.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            ChangePage(
                currentPage + (pointsRight ? 1 : -1),
                pointsRight ? 1 : -1
            );
        });

        return button;
    }

    // ───────────────── LOADING ─────────────────
    private void ShowLoading()
    {
        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }

        ClearEmptyState();

        Image face;
        loadingObj = CreateFramedPill(
            "LoadingPill",
            new Vector2(0.18f, 0.455f),
            new Vector2(0.82f, 0.545f),
            TOBATheme.WoodDark,
            out face
        );

        MakeText(
            face.transform,
            "Memuat kategori...",
            40,
            TOBATheme.BatakGold,
            new Vector2(640f, 90f),
            Vector2.zero,
            FontStyles.Bold
        );
    }

    private void ClearEmptyState()
    {
        if (emptyStateObj != null)
        {
            Object.Destroy(emptyStateObj);
            emptyStateObj = null;
        }
    }

    // ───────────────── DATA KATEGORI DARI API ─────────────────
    private void FetchCategories()
    {
        ClearEmptyState();

        if (APIManager.Instance == null)
        {
            Debug.LogError("TriviaMenuScreen: APIManager tidak ditemukan.");
            if (loadingObj != null)
            {
                Object.Destroy(loadingObj);
                loadingObj = null;
            }

            emptyStateObj = EmptyState.Show(
                screenObj.transform,
                "!",
                "Koneksi game belum siap",
                "Coba buka halaman ini kembali",
                () => RetryFetchCategories()
            );
            return;
        }

        APIManager.Instance.GetRequest(
            "/trivia/categories",
            (response) =>
            {
                if (screenObj == null) return;

                TriviaCategoryListResponse res = null;

                try
                {
                    string json = string.IsNullOrWhiteSpace(response)
                        ? "[]"
                        : response.Trim();

                    string wrapped = json.StartsWith("{")
                        ? json
                        : "{\"categories\":" + json + "}";

                    res = JsonUtility.FromJson<TriviaCategoryListResponse>(wrapped);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[Trivia] Gagal membaca JSON kategori: " + ex.Message);
                }

                categories = res != null && res.categories != null
                    ? res.categories.FindAll(c => c != null)
                    : new List<TriviaCategoryData>();

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                if (categories.Count == 0)
                {
                    if (currentPageObj != null)
                    {
                        Object.Destroy(currentPageObj);
                        currentPageObj = null;
                    }

                    totalPages = 0;
                    currentPage = 0;
                    UpdateCarouselControls();

                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "?",
                        "Belum ada kategori trivia",
                        "Hubungi admin untuk menambahkan kategori",
                        () => FetchCategories()
                    );
                    return;
                }

                BuildCategoryPages();
            },
            (err) =>
            {
                if (screenObj == null) return;

                Debug.LogWarning("[Trivia] Gagal memuat kategori: " + err);

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                emptyStateObj = EmptyState.Show(
                    screenObj.transform,
                    "!",
                    "Gagal memuat kategori",
                    "Periksa koneksi internet",
                    () => RetryFetchCategories()
                );
            }
        );
    }

    private void RetryFetchCategories()
    {
        ClearEmptyState();
        ShowLoading();
        FetchCategories();
    }

    // ───────────────── MEMBANGUN HALAMAN KATEGORI ─────────────────
    private void BuildCategoryPages()
    {
        if (screenObj == null || carouselViewport == null) return;

        if (currentPageObj != null)
        {
            Object.Destroy(currentPageObj);
            currentPageObj = null;
        }

        totalPages = Mathf.CeilToInt(
            categories.Count / (float)CategoriesPerPage
        );

        currentPage = 0;
        isPageTransitioning = false;

        DisplayPage(currentPage, 0, false);
        UpdateCarouselControls();

        Debug.Log(
            $"[Trivia] {categories.Count} kategori dimuat dalam {totalPages} halaman."
        );
    }

    private void ChangePage(int targetPage, int direction)
    {
        if (isPageTransitioning) return;
        if (targetPage < 0 || targetPage >= totalPages) return;
        if (targetPage == currentPage) return;

        currentPage = targetPage;
        DisplayPage(currentPage, direction, true);
        UpdateCarouselControls();
    }


    private void DisplayPage(int pageIndex, int direction, bool animate)
    {
        if (carouselViewport == null || screenObj == null)
            return;

        GameObject oldPage = currentPageObj;

        GameObject newPage = new GameObject(
            "TriviaCategoryPage_" + (pageIndex + 1),
            typeof(RectTransform),
            typeof(CanvasGroup)
        );
        newPage.transform.SetParent(carouselViewport, false);

        RectTransform newPageRT = newPage.GetComponent<RectTransform>();
        newPageRT.anchorMin = Vector2.zero;
        newPageRT.anchorMax = Vector2.one;
        newPageRT.offsetMin = Vector2.zero;
        newPageRT.offsetMax = Vector2.zero;
        newPageRT.anchoredPosition = Vector2.zero;

        CanvasGroup newPageGroup = newPage.GetComponent<CanvasGroup>();
        newPageGroup.alpha = 1f;
        newPageGroup.interactable = true;
        newPageGroup.blocksRaycasts = true;

        int startIndex = pageIndex * CategoriesPerPage;
        int endIndex = Mathf.Min(
            startIndex + CategoriesPerPage,
            categories.Count
        );

        int slotIndex = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            TriviaCategoryData category = categories[i];
            if (category == null)
                continue;

            CreateCategoryCard(
                newPage.transform,
                category,
                i + 1,
                slotIndex
            );

            slotIndex++;
        }

        currentPageObj = newPage;

        if (oldPage == null || !animate)
        {
            if (oldPage != null)
                Object.Destroy(oldPage);

            newPageRT.anchoredPosition = Vector2.zero;
            newPageGroup.alpha = 1f;
            newPageGroup.interactable = true;
            newPageGroup.blocksRaycasts = true;
            isPageTransitioning = false;
            return;
        }

        isPageTransitioning = true;

        CanvasGroup oldPageGroup = oldPage.GetComponent<CanvasGroup>();
        RectTransform oldPageRT = oldPage.GetComponent<RectTransform>();

        if (oldPageGroup != null)
        {
            oldPageGroup.interactable = false;
            oldPageGroup.blocksRaycasts = false;
        }

        float width = carouselViewport.rect.width;
        if (width < 100f)
            width = 800f;

        float distance = width * 0.24f;
        newPageRT.anchoredPosition = new Vector2(direction * distance, 0f);
        newPageGroup.alpha = 0f;
        newPageGroup.interactable = false;
        newPageGroup.blocksRaycasts = false;

        Sequence transition = DOTween.Sequence();
        transition.SetUpdate(true);
        transition.SetTarget(screenObj.transform);

        transition.Join(
            newPageRT.DOAnchorPos(Vector2.zero, 0.34f)
                .SetEase(Ease.OutCubic)
        );
        transition.Join(
            newPageGroup.DOFade(1f, 0.28f)
                .SetEase(Ease.OutQuad)
        );

        if (oldPageRT != null)
        {
            transition.Join(
                oldPageRT.DOAnchorPos(
                    new Vector2(-direction * distance, 0f),
                    0.34f
                ).SetEase(Ease.InCubic)
            );
        }

        if (oldPageGroup != null)
        {
            transition.Join(
                oldPageGroup.DOFade(0f, 0.25f)
                    .SetEase(Ease.InQuad)
            );
        }

        pageTransitionTween = transition;
        transition.OnComplete(() =>
        {
            if (oldPage != null)
                Object.Destroy(oldPage);

            if (newPage != null)
            {
                RectTransform finalRT = newPage.GetComponent<RectTransform>();
                CanvasGroup finalGroup = newPage.GetComponent<CanvasGroup>();

                if (finalRT != null)
                    finalRT.anchoredPosition = Vector2.zero;

                if (finalGroup != null)
                {
                    finalGroup.alpha = 1f;
                    finalGroup.interactable = true;
                    finalGroup.blocksRaycasts = true;
                }
            }

            pageTransitionTween = null;
            isPageTransitioning = false;
            UpdateCarouselControls();
        });
    }

        // ───────────────── KARTU KATEGORI DINAMIS ─────────────────

    private void CreateCategoryCard(
        Transform parent,
        TriviaCategoryData category,
        int displayNumber,
        int slotIndex)
    {
        int column = slotIndex % 2;
        int row = slotIndex / 2;

        float xMin = column == 0 ? 0.035f : 0.515f;
        float xMax = column == 0 ? 0.485f : 0.965f;

        // Baris pertama di atas, baris kedua di bawah.
        float yMin = row == 0 ? 0.515f : 0.035f;
        float yMax = row == 0 ? 0.965f : 0.485f;

        GameObject card = new GameObject(
            "CategoryCard_" + category.id,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(Shadow),
            typeof(CanvasGroup)
        );
        card.transform.SetParent(parent, false);

        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(xMin, yMin);
        cardRT.anchorMax = new Vector2(xMax, yMax);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;

        // Tiga lapisan: emas, cokelat ukiran, lalu permukaan kayu.
        Image outerFrame = card.GetComponent<Image>();
        outerFrame.sprite = GetRoundedSprite();
        outerFrame.type = Image.Type.Sliced;
        outerFrame.color = TOBATheme.BatakGold;
        outerFrame.raycastTarget = true;

        Shadow shadow = card.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
        shadow.effectDistance = new Vector2(0f, -6f);

        CreateInsetImage(
            card.transform,
            "CarvedBorder",
            TOBATheme.CardBorder,
            5f
        );

        Image woodFace = CreateInsetImage(
            card.transform,
            "WoodFace",
            TOBATheme.WoodDark,
            10f
        );
        woodFace.raycastTarget = false;

        // Aksen sudut dan garis emas memberi kesan papan kayu berornamen.
        CreateCardCorner(woodFace.transform, new Vector2(0f, 1f), new Vector2(17f, -17f));
        CreateCardCorner(woodFace.transform, new Vector2(1f, 1f), new Vector2(-17f, -17f));
        CreateCardCorner(woodFace.transform, new Vector2(0f, 0f), new Vector2(17f, 17f));
        CreateCardCorner(woodFace.transform, new Vector2(1f, 0f), new Vector2(-17f, 17f));

        CreateCardDivider(
            woodFace.transform,
            new Vector2(0.18f, 0.25f),
            new Vector2(0.82f, 0.25f)
        );

        MakeText(
            woodFace.transform,
            "TRIVIA BUDAYA",
            16,
            TOBATheme.BatakGold,
            new Vector2(320f, 36f),
            new Vector2(0f, 158f),
            FontStyles.Bold
        );

        // Medali nomor kategori.
        GameObject medal = new GameObject(
            "CategoryMedal",
            typeof(RectTransform),
            typeof(Image)
        );
        medal.transform.SetParent(woodFace.transform, false);

        RectTransform medalRT = medal.GetComponent<RectTransform>();
        medalRT.anchorMin = new Vector2(0.5f, 0.68f);
        medalRT.anchorMax = new Vector2(0.5f, 0.68f);
        medalRT.pivot = new Vector2(0.5f, 0.5f);
        medalRT.sizeDelta = new Vector2(56f, 56f);
        medalRT.anchoredPosition = Vector2.zero;

        Image medalOuter = medal.GetComponent<Image>();
        medalOuter.sprite = GetCircleSprite();
        medalOuter.color = TOBATheme.BatakGold;
        medalOuter.raycastTarget = false;

        GameObject medalInner = new GameObject(
            "MedalInner",
            typeof(RectTransform),
            typeof(Image)
        );
        medalInner.transform.SetParent(medal.transform, false);

        RectTransform medalInnerRT = medalInner.GetComponent<RectTransform>();
        medalInnerRT.anchorMin = Vector2.zero;
        medalInnerRT.anchorMax = Vector2.one;
        medalInnerRT.offsetMin = new Vector2(5f, 5f);
        medalInnerRT.offsetMax = new Vector2(-5f, -5f);

        Image medalInnerImage = medalInner.GetComponent<Image>();
        medalInnerImage.sprite = GetCircleSprite();
        medalInnerImage.color = TOBATheme.WoodDark;
        medalInnerImage.raycastTarget = false;

        MakeText(
            medalInner.transform,
            displayNumber.ToString("00"),
            22,
            TOBATheme.BatakGold,
            new Vector2(48f, 48f),
            Vector2.zero,
            FontStyles.Bold
        );

        // Nama kategori selalu berasal dari API, bukan ditanam pada gambar.
        string categoryName = string.IsNullOrWhiteSpace(category.name)
            ? "Kategori Trivia"
            : category.name.Trim();

        TextMeshProUGUI title = MakeText(
            woodFace.transform,
            categoryName,
            34,
            TOBATheme.TextLight,
            new Vector2(370f, 132f),
            new Vector2(0f, -28f),
            FontStyles.Bold
        );

        title.textWrappingMode = TextWrappingModes.Normal;
        title.alignment = TextAlignmentOptions.Center;
        title.enableAutoSizing = true;
        title.fontSizeMin = 19;
        title.fontSizeMax = 34;
        title.raycastTarget = false;

        TextMeshProUGUI actionText = MakeText(
            woodFace.transform,
            "KETUK UNTUK MEMULAI",
            15,
            TOBATheme.BatakGold,
            new Vector2(330f, 34f),
            new Vector2(0f, -142f),
            FontStyles.Bold
        );
        actionText.enableAutoSizing = true;
        actionText.fontSizeMin = 12;
        actionText.fontSizeMax = 15;

        // Animasi kartu masuk secara berurutan.
        CanvasGroup cardGroup = card.GetComponent<CanvasGroup>();
        cardGroup.alpha = 0f;
        cardGroup.interactable = true;
        cardGroup.blocksRaycasts = true;
        cardRT.localScale = Vector3.one * 0.86f;

        Sequence cardIntro = DOTween.Sequence();
        cardIntro.SetUpdate(true);
        cardIntro.SetTarget(screenObj.transform);
        cardIntro.AppendInterval(slotIndex * 0.055f);
        cardIntro.Append(
            cardRT.DOScale(1f, 0.34f)
                .SetEase(Ease.OutBack)
        );
        cardIntro.Join(
            cardGroup.DOFade(1f, 0.24f)
                .SetEase(Ease.OutQuad)
        );

        Button button = card.GetComponent<Button>();
        button.targetGraphic = outerFrame;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock buttonColors = button.colors;
        buttonColors.normalColor = Color.white;
        buttonColors.highlightedColor = new Color(1f, 0.94f, 0.76f, 1f);
        buttonColors.pressedColor = new Color(0.82f, 0.69f, 0.43f, 1f);
        buttonColors.selectedColor = Color.white;
        button.colors = buttonColors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        TriviaCategoryData selectedCategory = category;
        button.onClick.AddListener(() =>
        {
            if (isPageTransitioning || isOpeningCategory)
                return;

            isOpeningCategory = true;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            Sequence press = DOTween.Sequence();
            press.SetUpdate(true);
            press.SetTarget(screenObj.transform);
            press.Append(cardRT.DOScale(0.94f, 0.07f).SetEase(Ease.OutQuad));
            press.Append(cardRT.DOScale(1.045f, 0.11f).SetEase(Ease.OutBack));
            press.Append(cardRT.DOScale(1f, 0.08f).SetEase(Ease.OutQuad));
            press.OnComplete(() =>
            {
                if (ScreenManager.Instance != null)
                {
                    ScreenManager.Instance.ChangeScreen(
                        new TriviaQuizScreen(container, selectedCategory)
                    );
                }
                else
                {
                    isOpeningCategory = false;
                }
            });
        });
    }

    private void CreateCardCorner(
        Transform parent,
        Vector2 anchor,
        Vector2 offset)
    {
        GameObject dot = new GameObject(
            "GoldCornerOrnament",
            typeof(RectTransform),
            typeof(Image)
        );
        dot.transform.SetParent(parent, false);

        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(13f, 13f);
        rt.anchoredPosition = offset;

        Image img = dot.GetComponent<Image>();
        img.sprite = GetCircleSprite();
        img.color = TOBATheme.BatakGold;
        img.raycastTarget = false;
    }

    private void CreateCardDivider(
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject line = new GameObject(
            "GoldDivider",
            typeof(RectTransform),
            typeof(Image)
        );
        line.transform.SetParent(parent, false);

        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(0f, -1.5f);
        rt.offsetMax = new Vector2(0f, 1.5f);

        Image img = line.GetComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = new Color(
            TOBATheme.BatakGold.r,
            TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b,
            0.72f
        );
        img.raycastTarget = false;
    }

    private void UpdateCarouselControls()
    {
        bool hasMultiplePages = totalPages > 1;

        if (previousPageButton != null)
        {
            previousPageButton.gameObject.SetActive(hasMultiplePages);
            previousPageButton.interactable = hasMultiplePages && !isPageTransitioning && currentPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(hasMultiplePages);
            nextPageButton.interactable =
                hasMultiplePages && !isPageTransitioning && currentPage < totalPages - 1;
        }

        if (pageCounterText != null)
        {
            int displayPage = totalPages > 0 ? currentPage + 1 : 0;
            pageCounterText.text = $"HALAMAN {displayPage:00} / {totalPages:00}";
            pageCounterText.gameObject.SetActive(totalPages > 1);
        }
    }


    public override void Hide()
    {
        // Hentikan seluruh tween yang ditargetkan ke root layar sebelum layar ditutup.
        if (screenObj != null)
            DOTween.Kill(screenObj.transform, false);

        if (pageTransitionTween != null)
        {
            pageTransitionTween.Kill(false);
            pageTransitionTween = null;
        }

        isPageTransitioning = false;
        isOpeningCategory = false;

        base.Hide();
    }

}