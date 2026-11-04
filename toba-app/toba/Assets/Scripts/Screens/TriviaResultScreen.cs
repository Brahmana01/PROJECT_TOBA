using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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
    private int starsEarned;
    private Coroutine introCoroutine;

    // Sprite dibuat lewat kode (tanpa aset tambahan)
    private static Sprite roundedSprite;
    private static Sprite starSprite;

    // ───────── PAPAN HEADER (aset AI di Resources/gambar/) ─────────
    private const string BoardSpritePath = "gambar/papan_hasil_trivia_bintang";
    private const string BoardPlainPath = "gambar/papan_polos";

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

        CreateBackground();
        CreateHeader();
        CreateBackButton();
        CreateResultCard();
        CreateButtons();

        if (ScreenManager.Instance != null)
            introCoroutine = ScreenManager.Instance.StartCoroutine(PlayIntro());

        SubmitResult();
    }

    // ───────────────── SPRITE HELPER ─────────────────
    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite == null)
            roundedSprite = BuildRoundedSprite(64, 20, 22);
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

    // Bintang digambar lewat kode, jadi tidak bergantung pada font
    private static Sprite GetStarSprite()
    {
        if (starSprite != null) return starSprite;

        const int size = 128;
        Vector2[] pts = new Vector2[10];

        for (int i = 0; i < 10; i++)
        {
            float r = (i % 2 == 0) ? 58f : 25f;
            float ang = Mathf.PI / 2f + i * Mathf.PI / 5f;
            pts[i] = new Vector2(
                64f + Mathf.Cos(ang) * r,
                62f + Mathf.Sin(ang) * r
            );
        }

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
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
                        if (PointInPolygon(pts, p)) hit++;
                    }
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, hit / 9f));
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

    private static bool PointInPolygon(Vector2[] poly, Vector2 p)
    {
        bool inside = false;

        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) /
                       (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    // ───────────────── UI HELPER ─────────────────
    private GameObject CreateAnchoredPanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
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

    // Pil/chip berukuran tetap di tengah parent
    private Image CreateChip(
        Transform parent,
        string name,
        Color color,
        Vector2 size,
        Vector2 pos)
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
        outline.effectDistance = new Vector2(2f, 2f);

        return img;
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
        GameObject bg = new GameObject("ResultBackground");
        bg.transform.SetParent(screenObj.transform, false);

        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = bg.AddComponent<Image>();

        Sprite sprite = Resources.Load<Sprite>("Backgrounds/trivia_bg");
        if (sprite == null) sprite = Resources.Load<Sprite>("bg_toba");

        img.sprite = sprite;
        img.color = sprite != null ? Color.white : TOBATheme.ForestGreen;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = false;
        bg.transform.SetAsFirstSibling();

        GameObject shade = new GameObject("ResultShade");
        shade.transform.SetParent(screenObj.transform, false);

        RectTransform shadeRt = shade.AddComponent<RectTransform>();
        shadeRt.anchorMin = Vector2.zero;
        shadeRt.anchorMax = Vector2.one;
        shadeRt.offsetMin = Vector2.zero;
        shadeRt.offsetMax = Vector2.zero;

        Image shadeImg = shade.AddComponent<Image>();
        shadeImg.color = new Color(0.02f, 0.08f, 0.08f, 0.45f);
        shadeImg.raycastTarget = false;
    }

    // ───────────────── HEADER (PAPAN AI HASIL TRIVIA) ─────────────────
    private void CreateHeader()
    {
        // Papan khusus (tulisan HASIL TRIVIA sudah ada di gambar)
        Sprite board = Resources.Load<Sprite>(BoardSpritePath);
        bool baked = board != null;

        // Cadangan: papan polos + judul dari kode
        if (board == null)
        {
            Debug.LogWarning("[TriviaResult] Sprite '" + BoardSpritePath +
                "' tidak ditemukan, pakai papan polos.");
            board = Resources.Load<Sprite>(BoardPlainPath);
        }

        GameObject header = new GameObject("ResultBoardHeader");
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

        if (!baked)
        {
            TextMeshProUGUI titleTxt = MakeText(
                header.transform,
                "HASIL TRIVIA",
                44,
                TOBATheme.BatakGold,
                new Vector2(480f, 90f),
                new Vector2(0f, -9f),    // kayu sedikit di bawah tengah rect
                FontStyles.Bold
            );

            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = 22;
            titleTxt.fontSizeMax = 44;
        }
    }

    // ───────────────── TOMBOL KEMBALI (SPRITE SAMA DENGAN LAYAR LAIN) ─────────────────
    private void CreateBackButton()
    {
        Sprite backSprite = Resources.Load<Sprite>(BackSpritePath);

        if (backSprite == null)
        {
            Debug.LogWarning("[TriviaResult] Sprite tombol kembali '" +
                BackSpritePath + "' tidak ditemukan, tombol kembali dilewati.");
            return;
        }

        GameObject root = new GameObject("BtnBackHeader");
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

            if (ScreenManager.Instance != null)
                ScreenManager.Instance.ChangeScreen(
                    new TriviaMenuScreen(container));
        });
    }

    // ───────────────── KARTU HASIL ─────────────────
    private void CreateResultCard()
    {
        // Lapis 1: emas
        GameObject card = CreateAnchoredPanel(
            screenObj.transform,
            "ResultCard",
            new Vector2(0.06f, 0.295f),
            new Vector2(0.94f, 0.855f),     // atas diturunkan karena papan lebih tinggi
            TOBATheme.BatakGold
        );

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -8f);

        // Lapis 2: cokelat
        Image brown = CreateInsetImage(
            card.transform, "CardBorder", TOBATheme.CardBorder, 6f);

        // Lapis 3: perkamen
        Image parchmentImg = CreateInsetImage(
            brown.transform, "CardParchment", TOBATheme.CardBG, 5f);

        Sprite parchment = TOBATheme.LoadSprite("panel_parchment");
        if (parchment != null)
        {
            parchmentImg.sprite = parchment;
            parchmentImg.type = parchment.border != Vector4.zero
                ? Image.Type.Sliced
                : Image.Type.Simple;
            parchmentImg.color = Color.white;
        }

        Transform p = parchmentImg.transform;

        // Nama kategori
        string catName = categoryData != null &&
                         !string.IsNullOrEmpty(categoryData.name)
            ? categoryData.name.ToUpper()
            : "TRIVIA BUDAYA";

        TextMeshProUGUI catTxt = MakeText(
            p, catName, 50, TOBATheme.WoodDark,
            new Vector2(860, 80), new Vector2(0f, 485f), FontStyles.Bold);
        catTxt.enableAutoSizing = true;
        catTxt.fontSizeMin = 28;
        catTxt.fontSizeMax = 50;

        // Garis emas pemisah
        CreateChip(p, "TopDivider", TOBATheme.BatakGold,
            new Vector2(700f, 6f), new Vector2(0f, 425f));

        // Bintang
        CreateStars(p);

        // Skor besar
        scoreTxt = MakeText(
            p, "0 / " + totalQuestions, 130, TOBATheme.WoodDark,
            new Vector2(860, 160), new Vector2(0f, 115f), FontStyles.Bold);

        MakeText(
            p, "JAWABAN BENAR", 36, TOBATheme.TextGray,
            new Vector2(860, 60), new Vector2(0f, 10f), FontStyles.Bold);

        // Pesan semangat
        TextMeshProUGUI msgTxt = MakeText(
            p, GetMessage(), 42, TOBATheme.WoodMedium,
            new Vector2(860, 110), new Vector2(0f, -70f), FontStyles.Bold);
        msgTxt.textWrappingMode = TextWrappingModes.Normal;
        msgTxt.enableAutoSizing = true;
        msgTxt.fontSizeMin = 30;
        msgTxt.fontSizeMax = 42;

        // Chip benar dan salah
        CreateChip(p, "ChipBenar", TOBATheme.Success,
            new Vector2(420f, 115f), new Vector2(-225f, -195f));
        MakeText(
            p, "BENAR  " + correctCount, 48, Color.white,
            new Vector2(400f, 105f), new Vector2(-225f, -195f), FontStyles.Bold);

        CreateChip(p, "ChipSalah", TOBATheme.Error,
            new Vector2(420f, 115f), new Vector2(225f, -195f));
        MakeText(
            p, "SALAH  " + (totalQuestions - correctCount), 48, Color.white,
            new Vector2(400f, 105f), new Vector2(225f, -195f), FontStyles.Bold);

        // Garis pemisah bawah
        CreateChip(p, "BottomDivider", TOBATheme.BatakGold,
            new Vector2(700f, 6f), new Vector2(0f, -280f));

        // Baris XP dan level
        CreateChip(p, "XPRow", TOBATheme.WoodDark,
            new Vector2(880f, 140f), new Vector2(0f, -385f));

        xpTxt = MakeText(
            p, "XP  +0", 48, TOBATheme.BatakGold,
            new Vector2(420f, 110f), new Vector2(-215f, -385f), FontStyles.Bold);

        levelTxt = MakeText(
            p, "LEVEL  -", 48, TOBATheme.TextLight,
            new Vector2(420f, 110f), new Vector2(215f, -385f), FontStyles.Bold);

        // Teks menyimpan
        loadingObj = MakeText(
            p, "Menyimpan hasil...", 30, TOBATheme.TextGray,
            new Vector2(800f, 50f), new Vector2(0f, -490f), FontStyles.Italic)
            .gameObject;
    }

    private void CreateStars(Transform parent)
    {
        float[] xs = { -200f, 0f, 200f };
        float[] ys = { 295f, 320f, 295f };
        float[] sizes = { 160f, 190f, 160f };

        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject("Star_" + (i + 1));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(sizes[i], sizes[i]);
            rt.anchoredPosition = new Vector2(xs[i], ys[i]);

            Image img = go.AddComponent<Image>();
            img.sprite = GetStarSprite();
            img.type = Image.Type.Simple;
            img.raycastTarget = false;

            bool active = i < starsEarned;
            img.color = active ? TOBATheme.StarActive : TOBATheme.StarInactive;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = TOBATheme.WoodDark;
            outline.effectDistance = new Vector2(4f, -4f);

            // Bintang aktif muncul lewat animasi
            if (active) rt.localScale = Vector3.zero;

            starImages[i] = img;
        }
    }

    private string GetMessage()
    {
        if (starsEarned >= 3) return "Luar biasa! Kamu jago budaya Batak!";
        if (starsEarned == 2) return "Bagus sekali! Sedikit lagi sempurna.";
        if (starsEarned == 1) return "Lumayan! Ayo coba lagi.";
        return "Jangan menyerah, ayo belajar lagi!";
    }

    // ───────────────── TOMBOL ─────────────────
    private void CreateButtons()
    {
        CreateActionButton(
            "BtnLagi",
            "MAIN LAGI",
            new Vector2(0.1f, 0.165f),
            new Vector2(0.9f, 0.255f),
            TOBATheme.BtnPrimary,
            Color.white,
            () =>
            {
                if (ScreenManager.Instance != null)
                    ScreenManager.Instance.ChangeScreen(
                        new TriviaQuizScreen(container, categoryData));
            }
        );

        CreateActionButton(
            "BtnBack",
            "PILIH KATEGORI LAIN",
            new Vector2(0.1f, 0.055f),
            new Vector2(0.9f, 0.145f),
            TOBATheme.BtnSecondary,
            TOBATheme.BatakGold,
            () =>
            {
                if (ScreenManager.Instance != null)
                    ScreenManager.Instance.ChangeScreen(
                        new TriviaMenuScreen(container));
            }
        );
    }

    private void CreateActionButton(
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color faceColor,
        Color textColor,
        Action onClick)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image frame = root.AddComponent<Image>();
        frame.sprite = GetRoundedSprite();
        frame.type = Image.Type.Sliced;
        frame.color = TOBATheme.BatakGold;
        frame.raycastTarget = true;

        Shadow shadow = root.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -7f);

        Image face = CreateInsetImage(root.transform, "Face", faceColor, 5f);

        TextMeshProUGUI txt = MakeText(
            face.transform,
            label,
            50,
            textColor,
            new Vector2(780f, 100f),
            Vector2.zero,
            FontStyles.Bold
        );
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 28;
        txt.fontSizeMax = 50;

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

    // ───────────────── ANIMASI ─────────────────
    private IEnumerator PlayIntro()
    {
        // Skor naik dari 0
        float t = 0f;
        const float dur = 0.7f;

        while (t < dur)
        {
            if (screenObj == null || scoreTxt == null) yield break;

            t += Time.unscaledDeltaTime;
            int v = Mathf.RoundToInt(
                Mathf.Lerp(0f, correctCount, Mathf.Clamp01(t / dur)));
            scoreTxt.text = v + " / " + totalQuestions;
            yield return null;
        }

        if (scoreTxt != null)
            scoreTxt.text = correctCount + " / " + totalQuestions;

        // Bintang muncul satu per satu
        for (int i = 0; i < starsEarned && i < 3; i++)
        {
            if (screenObj == null || starImages[i] == null) yield break;

            Transform tr = starImages[i].transform;
            float s = 0f;
            const float popDur = 0.4f;

            while (s < popDur)
            {
                if (screenObj == null || tr == null) yield break;

                s += Time.unscaledDeltaTime;
                float x = Mathf.Clamp01(s / popDur) - 1f;
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                float scale = 1f + c3 * x * x * x + c1 * x * x;
                tr.localScale = Vector3.one * Mathf.Max(0f, scale);
                yield return null;
            }

            if (tr != null) tr.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(0.12f);
        }

        introCoroutine = null;
    }

    // ───────────────── LOGIKA ─────────────────
    int CalculateStars()
    {
        if (totalQuestions <= 0) return 0;

        float ratio = (float)correctCount / totalQuestions;
        if (ratio >= 1f) return 3;
        if (ratio >= 0.6f) return 2;
        if (ratio >= 0.4f) return 1;
        return 0;
    }

    void SubmitResult()
    {
        if (APIManager.Instance == null || categoryData == null) return;

        int playerID = PlayerPrefs.GetInt("PlayerID");

        SubmitTriviaRequest req = new SubmitTriviaRequest
        {
            player_id = playerID,
            category_id = categoryData.id,
            correct_count = correctCount,
            wrong_count = totalQuestions - correctCount
        };

        APIManager.Instance.PostRequest(
            "/trivia/submit",
            JsonUtility.ToJson(req),
            (response) =>
            {
                if (screenObj == null) return;

                SubmitTriviaResponse res =
                    JsonUtility.FromJson<SubmitTriviaResponse>(response);

                if (res != null)
                {
                    if (xpTxt != null) xpTxt.text = "XP  +" + res.xp;
                    if (levelTxt != null) levelTxt.text = "LEVEL  " + res.level;
                    PlayerPrefs.SetInt("PlayerLevel", res.level);
                }

                if (loadingObj != null)
                    UnityEngine.Object.Destroy(loadingObj);
            },
            (err) =>
            {
                if (screenObj == null) return;

                if (xpTxt != null) xpTxt.text = "XP  offline";
                if (loadingObj != null)
                    UnityEngine.Object.Destroy(loadingObj);
            }
        );
    }

    public override void Hide()
    {
        if (introCoroutine != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(introCoroutine);

        introCoroutine = null;

        base.Hide();
    }
}