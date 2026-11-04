using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TriviaMenuScreen : BaseScreen
{
    private List<TriviaCategoryData> categories = new List<TriviaCategoryData>();
    private GameObject loadingObj;
    private GameObject emptyStateObj;
    private Coroutine introCoroutine;

    // Posisi gambar menu. Tombol kategori mengikuti posisi ini.
    private const float frameImageWidth = 980f;
    private const float frameBorder = 12f;
    private readonly Vector2 visualPos = new Vector2(0f, -36f);

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

    public TriviaMenuScreen(Transform parent) : base(parent, "TriviaMenu") { }

    public override void Show()
    {
        CreateBackground();
        CreateHeader();
        BuildTriviaVisual();
        CreateRibbon();
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
        shadeImg.color = new Color(0.02f, 0.08f, 0.08f, 0.35f);
        shadeImg.raycastTarget = false;
    }

    // ───────────────── HEADER + TOMBOL KEMBALI ─────────────────
    private void CreateHeader()
    {
        // Container header memenuhi area layar
        GameObject headerContainer = new GameObject("TriviaHeaderContainer");
        headerContainer.transform.SetParent(screenObj.transform, false);

        RectTransform containerRT = headerContainer.AddComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // =====================================
        // 1. PAPAN JUDUL - SELALU DI TENGAH
        // =====================================

        Sprite headerSprite = Resources.Load<Sprite>("gambar/trivia_header");

        if (headerSprite == null)
        {
            Debug.LogError("[Trivia] Gambar trivia_header tidak ditemukan!");
            return;
        }

        GameObject header = new GameObject("TriviaWoodHeader");
        header.transform.SetParent(headerContainer.transform, false);

        RectTransform headerRT = header.AddComponent<RectTransform>();

        // Lebar papan simetris terhadap layar
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

        // =====================================
        // 2. TOMBOL KEMBALI
        // Titik tengahnya = titik tengah KAYU papan (HeaderCenterY),
        // ukuran dan jarak kiri sama dengan layar kuis & hasil.
        // =====================================

        Sprite back = TOBATheme.LoadSprite(
            "RegisterNameScreen/register_name_back_button_v2"
        );

        if (back == null)
        {
            Debug.LogError("[Trivia] Back button sprite tidak ditemukan!");
            return;
        }

        GameObject buttonObject = new GameObject("BackButton");
        buttonObject.transform.SetParent(headerContainer.transform, false);

        RectTransform buttonRT = buttonObject.AddComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0f, HeaderCenterY);
        buttonRT.anchorMax = new Vector2(0f, HeaderCenterY);
        buttonRT.pivot = new Vector2(0f, 0.5f);
        buttonRT.sizeDelta = new Vector2(BackButtonSize, BackButtonSize);
        buttonRT.anchoredPosition = new Vector2(BackButtonLeft, 0f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = back;
        buttonImage.preserveAspect = true;
        buttonImage.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;

        button.onClick.AddListener(() =>
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.StartCoroutine(
                    BackButtonPress(buttonObject.transform)
                );
            }
        });

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.StartCoroutine(
                BackButtonFloat(buttonObject.transform)
            );
        }
    }

    private IEnumerator BackButtonFloat(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin = target.localPosition;
        float time = 0f;

        while (target != null)
        {
            time += Time.deltaTime;

            target.localPosition = new Vector3(
                origin.x,
                origin.y + Mathf.Sin(time * 1.1f) * 2f,
                origin.z
            );

            yield return null;
        }
    }

    private IEnumerator BackButtonPress(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original = target.localScale;

        target.localScale = original * 0.88f;
        yield return new WaitForSeconds(0.07f);

        if (target == null)
            yield break;

        target.localScale = original * 1.06f;
        yield return new WaitForSeconds(0.07f);

        if (target == null)
            yield break;

        target.localScale = original;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX("sfx_click");

        ScreenManager.Instance.ChangeScreen(
            new MainMenuScreen(container)
        );
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
            "Ketuk kartu kategori untuk mulai bermain",
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

    // ───────────────── DATA ─────────────────
    private void FetchCategories()
    {
        ClearEmptyState();

        if (APIManager.Instance == null)
        {
            Debug.LogError("TriviaMenuScreen: APIManager tidak ditemukan.");
            return;
        }

        APIManager.Instance.GetRequest(
            "/trivia/categories",
            (response) => {
                if (screenObj == null) return;

                string wrapped = "{\"categories\":" + response + "}";
                TriviaCategoryListResponse res =
                    JsonUtility.FromJson<TriviaCategoryListResponse>(wrapped);

                categories = res != null && res.categories != null
                    ? res.categories
                    : new List<TriviaCategoryData>();

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                if (categories.Count == 0)
                {
                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "?",
                        "Belum ada kategori trivia",
                        "Hubungi admin untuk menambahkan kategori",
                        () => FetchCategories()
                    );
                    return;
                }

                BuildCategoryButtons();
            },
            (err) => {
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

    // ───────────────── TOMBOL KATEGORI (di atas gambar) ─────────────────
    private void BuildCategoryButtons()
    {
        if (screenObj == null) return;

        Transform parent = screenObj.transform;

        // Tombol mengikuti ukuran gambar di dalam bingkai
        float k = frameImageWidth / 1080f;

        float cardWidth = 480f * k;
        float cardHeight = 430f * k;

        float leftX = visualPos.x - 260f * k;
        float rightX = visualPos.x + 260f * k;
        float topY = visualPos.y + 290f * k;
        float bottomY = visualPos.y - 210f * k;

        for (int i = 0; i < categories.Count && i < 4; i++)
        {
            TriviaCategoryData cat = categories[i];

            float x = (i % 2 == 0) ? leftX : rightX;
            float y = (i < 2) ? topY : bottomY;

            CreateInvisibleCategoryButton(
                parent,
                cat,
                new Vector2(cardWidth, cardHeight),
                new Vector2(x, y)
            );
        }
    }

    private void CreateInvisibleCategoryButton(
        Transform parent,
        TriviaCategoryData cat,
        Vector2 size,
        Vector2 position)
    {
        GameObject buttonObj = new GameObject("CategoryButton_" + cat.id);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        button.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("sfx_click");

            if (ScreenManager.Instance != null)
                ScreenManager.Instance.ChangeScreen(
                    new TriviaQuizScreen(container, cat));
        });
    }

    // ───────────────── GAMBAR MENU (dalam bingkai) ─────────────────
    private void BuildTriviaVisual()
    {
        if (screenObj == null)
        {
            Debug.LogError("[Trivia] screenObj NULL!");
            return;
        }

        Sprite sprite = Resources.Load<Sprite>("Trivia/trivia_menu");

        if (sprite == null)
        {
            Debug.LogError(
                "[Trivia] GAGAL LOAD GAMBAR! " +
                "Pastikan file ada di " +
                "Assets/Resources/Trivia/trivia_menu.png"
            );
            return;
        }

        float imgW = frameImageWidth;
        float imgH = imgW * sprite.rect.height / sprite.rect.width;

        // Lapis 1: bingkai emas
        GameObject frame = new GameObject("TriviaVisualFrame");
        frame.transform.SetParent(screenObj.transform, false);

        RectTransform rect = frame.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(
            imgW + frameBorder * 2f,
            imgH + frameBorder * 2f);
        rect.anchoredPosition = visualPos;

        Image frameImg = frame.AddComponent<Image>();
        frameImg.sprite = GetRoundedSprite();
        frameImg.type = Image.Type.Sliced;
        frameImg.color = TOBATheme.BatakGold;
        frameImg.raycastTarget = false;

        Shadow shadow = frame.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -8f);

        // Lapis 2: cokelat
        CreateInsetImage(
            frame.transform, "FrameBorder", TOBATheme.CardBorder, 4f);

        // Lapis 3: gambar menu (tepinya tertutup bingkai)
        // Masker membulat agar sudut gambar halus
        Image maskImg = CreateInsetImage(
            frame.transform, "VisualMask", Color.white, frameBorder);
        Mask mask = maskImg.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject visual = new GameObject("TriviaVisual");
        visual.transform.SetParent(maskImg.transform, false);

        RectTransform vr = visual.AddComponent<RectTransform>();
        vr.anchorMin = Vector2.zero;
        vr.anchorMax = Vector2.one;
        vr.offsetMin = Vector2.zero;
        vr.offsetMax = Vector2.zero;

        Image image = visual.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;

        // Muncul perlahan dengan efek membesar
        CanvasGroup group = frame.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        rect.localScale = Vector3.one * 0.92f;

        if (ScreenManager.Instance != null)
            introCoroutine = ScreenManager.Instance.StartCoroutine(
                PlayIntro(group, rect));
        else
        {
            group.alpha = 1f;
            rect.localScale = Vector3.one;
        }
    }

    private IEnumerator PlayIntro(CanvasGroup group, RectTransform rect)
    {
        float t = 0f;
        const float dur = 0.5f;

        while (t < dur)
        {
            if (screenObj == null || group == null || rect == null) yield break;

            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float ease = 1f - Mathf.Pow(1f - k, 3f);

            group.alpha = ease;
            rect.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, ease);
            yield return null;
        }

        if (group != null) group.alpha = 1f;
        if (rect != null) rect.localScale = Vector3.one;

        introCoroutine = null;
    }

    public override void Hide()
    {
        if (introCoroutine != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(introCoroutine);

        introCoroutine = null;

        base.Hide();
    }
}