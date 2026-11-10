using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationDetailScreen : BaseScreen
{
    private LocationData locationData;
    private TextMeshProUGUI factTxt;

    private static Sprite roundedSprite;

    // Papan judul (sama dengan layar kuis)
    private const float HeaderMinX = 0.15f, HeaderMaxX = 0.85f;
    private const float HeaderMinY = 0.865f, HeaderMaxY = 0.985f;
    private const float WoodCenterFromBottom = 0.459f;
    private static float HeaderCenterY =>
        HeaderMinY + (HeaderMaxY - HeaderMinY) * WoodCenterFromBottom;

    private const string BackSpritePath = "RegisterNameScreen/register_name_back_button_v2";
    private const float BackButtonSize = 120f;
    private const float BackButtonLeft = 30f;

    // Warna kartu fakta (kayu gelap, teks terang)
    private static readonly Color FactFace = new Color(0.27f, 0.14f, 0.05f, 1f);

    public LocationDetailScreen(Transform parent, LocationData loc)
        : base(parent, "LocationDetail")
    {
        locationData = loc;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0f, 0.04f, 0f, 0.35f), Vector2.zero, Vector2.one);

        CreateHeader();
        CreateBackButton();
        CreatePhotoCard();
        CreateRewardChips();
        CreateDescCard();
        CreateFactCard();
        CreateStartButton();

        FetchRandomFact();
    }

    // ───────────── SPRITE & UI HELPER ─────────────
    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;

        const int size = 64, radius = 20, border = 22;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, radius, size - 1 - radius);
                float cy = Mathf.Clamp(y, radius, size - 1 - radius);
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d + 0.5f)));
            }

        tex.Apply();
        roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        return roundedSprite;
    }

    private Image MakeImage(Transform parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = offMin; rt.offsetMax = offMax;

        Image img = go.AddComponent<Image>();
        img.sprite = GetRoundedSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    // Teks yang meregang mengikuti parent (tidak pakai posisi pixel)
    private TextMeshProUGUI MakeText(Transform parent, string text, int size,
        Color color, TextAlignmentOptions align,
        Vector2 aMin, Vector2 aMax, Vector4 pad, FontStyles style)
    {
        TextMeshProUGUI t = UIBuilder.CreateTextSized(parent, text, size, color, align,
            new Vector2(100f, 100f), Vector2.zero).GetComponent<TextMeshProUGUI>();

        RectTransform rt = t.rectTransform;
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(pad.x, pad.y);      // kiri, bawah
        rt.offsetMax = new Vector2(-pad.z, -pad.w);    // kanan, atas

        t.fontStyle = style;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.overflowMode = TextOverflowModes.Ellipsis;
        t.raycastTarget = false;
        return t;
    }

    // Kartu: pakai sprite dari Resources/gambar/<aiSpriteName> kalau ada,
    // kalau tidak pakai bingkai emas -> cokelat -> isi buatan kode.
    // Mengembalikan area isi (tempat menaruh teks).
    private Transform MakeCard(string name, Vector2 aMin, Vector2 aMax,
        string aiSpriteName, Color faceColor, float aiPadding)
    {
        Sprite ai = Resources.Load<Sprite>("gambar/" + aiSpriteName);

        if (ai != null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(screenObj.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.sprite = ai;
            img.type = ai.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            img.raycastTarget = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(go.transform, false);
            RectTransform crt = content.AddComponent<RectTransform>();
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(aiPadding, aiPadding);
            crt.offsetMax = new Vector2(-aiPadding, -aiPadding);
            return content.transform;
        }

        Debug.LogWarning("[LocationDetail] Sprite 'gambar/" + aiSpriteName +
            "' tidak ditemukan, pakai panel bawaan dari kode.");

        Image gold = MakeImage(screenObj.transform, name, aMin, aMax,
            Vector2.zero, Vector2.zero, TOBATheme.BatakGold);
        Shadow sh = gold.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
        sh.effectDistance = new Vector2(0f, -6f);

        Image brown = MakeImage(gold.transform, "Border", Vector2.zero, Vector2.one,
            new Vector2(5f, 5f), new Vector2(-5f, -5f), TOBATheme.CardBorder);
        Image face = MakeImage(brown.transform, "Face", Vector2.zero, Vector2.one,
            new Vector2(4f, 4f), new Vector2(-4f, -4f), faceColor);
        return face.transform;
    }

    // ───────────── HEADER + TOMBOL KEMBALI ─────────────
    private void CreateHeader()
    {
        GameObject header = new GameObject("LocationBoardHeader");
        header.transform.SetParent(screenObj.transform, false);

        RectTransform rt = header.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(HeaderMinX, HeaderMinY);
        rt.anchorMax = new Vector2(HeaderMaxX, HeaderMaxY);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Papan khusus lokasi: Resources/gambar/papan_<nama_lokasi>.png
        // contoh "IT Del" -> papan_it_del. Kalau tidak ada -> papan polos + judul dari kode.
        Sprite board = Resources.Load<Sprite>("gambar/papan_" + Slug(locationData.name));
        bool baked = board != null;
        if (!baked) board = Resources.Load<Sprite>("gambar/papan_polos");

        Image img = header.AddComponent<Image>();
        img.sprite = board;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = board != null ? Color.white : TOBATheme.WoodMedium;
        img.raycastTarget = false;

        // Tulisan sudah ada di gambar papan khusus; kalau tidak, tulis dari kode
        if (!baked)
        {
            TextMeshProUGUI title = UIBuilder.CreateTextSized(header.transform,
                locationData.name.ToUpper(), 44, TOBATheme.BatakGold,
                TextAlignmentOptions.Center, new Vector2(480f, 90f), new Vector2(0f, -9f))
                .GetComponent<TextMeshProUGUI>();
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 22;
            title.fontSizeMax = 44;
            title.raycastTarget = false;
        }
    }

    // "IT Del" -> "it_del"
    private static string Slug(string name)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char ch in (name ?? "").ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (ch == ' ' || ch == '-' || ch == '_') sb.Append('_');
        }
        return sb.ToString();
    }

    private void CreateBackButton()
    {
        Sprite back = Resources.Load<Sprite>(BackSpritePath);

        GameObject root = new GameObject("BtnBack");
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, HeaderCenterY);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(BackButtonSize, BackButtonSize);
        rt.anchoredPosition = new Vector2(BackButtonLeft, 0f);

        Image img = root.AddComponent<Image>();
        if (back != null)
        {
            img.sprite = back;
            img.preserveAspect = true;
        }
        else
        {
            // Cadangan kalau sprite tidak ditemukan
            img.sprite = GetRoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = TOBATheme.BatakGold;
            MakeText(root.transform, "<", 60, TOBATheme.WoodDark,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
                Vector4.zero, FontStyles.Bold);
        }
        img.raycastTarget = true;

        Shadow sh = root.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
        sh.effectDistance = new Vector2(0f, -4f);

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("sfx_click");
            ScreenManager.Instance.ChangeScreen(new MapScreen(container));
        });
    }

    // ───────────── FOTO LOKASI ─────────────
    private void CreatePhotoCard()
    {
        Transform face = MakeCard("PhotoCard",
            new Vector2(0.06f, 0.600f), new Vector2(0.94f, 0.840f),
            "panel_foto", TOBATheme.ForestGreen, 60f);

        // Foto lokasi: Resources/Locations/<id>.png (kalau belum ada -> placeholder rapi)
        Sprite photo = Resources.Load<Sprite>("Locations/" + locationData.id);

        GameObject maskObj = new GameObject("PhotoMask");
        maskObj.transform.SetParent(face, false);
        RectTransform mrt = maskObj.AddComponent<RectTransform>();
        mrt.anchorMin = Vector2.zero; mrt.anchorMax = Vector2.one;
        mrt.offsetMin = mrt.offsetMax = Vector2.zero;

        Image maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetRoundedSprite();
        maskImg.type = Image.Type.Sliced;
        maskImg.color = Color.white;
        maskObj.AddComponent<Mask>().showMaskGraphic = false;

        GameObject pic = new GameObject("Photo");
        pic.transform.SetParent(maskObj.transform, false);
        RectTransform prt = pic.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;

        Image picImg = pic.AddComponent<Image>();
        picImg.raycastTarget = false;

        if (photo != null)
        {
            picImg.sprite = photo;
            picImg.preserveAspect = false;
            picImg.color = Color.white;
        }
        else
        {
            picImg.color = new Color(0.12f, 0.30f, 0.26f, 1f);
            MakeText(pic.transform, locationData.name, 42,
                new Color(1f, 1f, 1f, 0.55f), TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector4(30, 0, 30, 0), FontStyles.Bold);
        }
    }

    // ───────────── CHIP XP & BINTANG ─────────────
    private void CreateRewardChips()
    {
        MakeChip("ChipXP", new Vector2(0.06f, 0.545f), new Vector2(0.48f, 0.592f),
            "✨ +" + locationData.reward_xp + " XP", TOBATheme.BatakGold);

        MakeChip("ChipStar", new Vector2(0.52f, 0.545f), new Vector2(0.94f, 0.592f),
            "<b><color=#D6A84B>★</color></b> +" + locationData.reward_stars + " Bintang", TOBATheme.StarActive);
    }

    private void MakeChip(string name, Vector2 aMin, Vector2 aMax, string label, Color textColor)
    {
        Image gold = MakeImage(screenObj.transform, name, aMin, aMax,
            Vector2.zero, Vector2.zero, TOBATheme.BatakGold);
        Image face = MakeImage(gold.transform, "Face", Vector2.zero, Vector2.one,
            new Vector2(4f, 4f), new Vector2(-4f, -4f), TOBATheme.WoodDark);

        TextMeshProUGUI t = MakeText(face.transform, label, 34, textColor,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
            new Vector4(10, 0, 10, 0), FontStyles.Bold);
        t.enableAutoSizing = true;
        t.fontSizeMin = 22;
        t.fontSizeMax = 34;
    }

    // ───────────── DESKRIPSI ─────────────
    private void CreateDescCard()
    {
        Transform face = MakeCard("DescCard",
            new Vector2(0.06f, 0.335f), new Vector2(0.94f, 0.535f),
            "panel_deskripsi", TOBATheme.CardBG, 70f);

        MakeText(face, "<b><color=#A83232>⌖</color></b> TENTANG LOKASI", 36, TOBATheme.WoodDark,
            TextAlignmentOptions.Left,
            new Vector2(0f, 0.76f), Vector2.one,
            new Vector4(10, 0, 10, 4), FontStyles.Bold);

        // Garis pemisah emas tipis
        MakeImage(face, "Divider", new Vector2(0.02f, 0.755f), new Vector2(0.98f, 0.765f),
            Vector2.zero, Vector2.zero, TOBATheme.BatakGold);

        TextMeshProUGUI body = MakeText(face, locationData.description, 30,
            TOBATheme.TextDark, TextAlignmentOptions.TopLeft,
            Vector2.zero, new Vector2(1f, 0.74f),
            new Vector4(10, 4, 10, 6), FontStyles.Normal);
        body.enableAutoSizing = true;
        body.fontSizeMin = 22;
        body.fontSizeMax = 32;
    }

    // ───────────── FAKTA MENARIK ─────────────
    private void CreateFactCard()
    {
        Transform face = MakeCard("FactCard",
            new Vector2(0.06f, 0.185f), new Vector2(0.94f, 0.325f),
            "panel_fakta", FactFace, 70f);

        MakeText(face, "<b><color=#D6A84B>✧</color></b> FAKTA MENARIK", 34, TOBATheme.BatakGold,
            TextAlignmentOptions.Left,
            new Vector2(0f, 0.66f), Vector2.one,
            new Vector4(10, 0, 10, 2), FontStyles.Bold);

        factTxt = MakeText(face, "Memuat fakta...", 30, TOBATheme.TextLight,
            TextAlignmentOptions.TopLeft,
            Vector2.zero, new Vector2(1f, 0.66f),
            new Vector4(10, 2, 10, 4), FontStyles.Normal);
        factTxt.enableAutoSizing = true;
        factTxt.fontSizeMin = 22;
        factTxt.fontSizeMax = 30;
    }

    // ───────────── TOMBOL MULAI ─────────────
    private void CreateStartButton()
    {
        Sprite ai = Resources.Load<Sprite>("gambar/tombol_mulai");

        GameObject root = new GameObject("BtnMulai");
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.12f, 0.055f);
        rt.anchorMax = new Vector2(0.88f, 0.165f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image frame = root.AddComponent<Image>();
        Graphic target = frame;

        if (ai != null)
        {
            frame.sprite = ai;
            frame.type = Image.Type.Simple;
            frame.preserveAspect = true;
            frame.raycastTarget = true;
        }
        else
        {
            frame.sprite = GetRoundedSprite();
            frame.type = Image.Type.Sliced;
            frame.color = TOBATheme.BatakGold;
            frame.raycastTarget = true;

            Shadow sh = root.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.45f);
            sh.effectDistance = new Vector2(0f, -6f);

            Image face = MakeImage(root.transform, "Face", Vector2.zero, Vector2.one,
                new Vector2(5f, 5f), new Vector2(-5f, -5f), TOBATheme.BtnPrimary);
            target = face;
        }

        TextMeshProUGUI t = MakeText(root.transform, "⚔ MULAI TANTANGAN", 46,
            ai != null ? TOBATheme.WoodDark : Color.white,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
            new Vector4(40, 0, 40, 0), FontStyles.Bold);
        t.enableAutoSizing = true;
        t.fontSizeMin = 28;
        t.fontSizeMax = 46;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = target;

        ColorBlock c = btn.colors;
        c.normalColor = Color.white;
        c.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
        c.pressedColor = new Color(0.8f, 0.7f, 0.5f, 1f);
        btn.colors = c;

        btn.onClick.AddListener(() =>
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX("sfx_click");
            ScreenManager.Instance.ChangeScreen(new QuizScreen(container, locationData));
        });
    }

    // ───────────── DATA ─────────────
    private void FetchRandomFact()
    {
        APIManager.Instance.GetRequest("/locations/" + locationData.id + "/fact",
            response =>
            {
                if (factTxt == null) return;

                LocationFactResponse res = JsonUtility.FromJson<LocationFactResponse>(response);
                factTxt.text = (res != null && res.fact != null)
                    ? res.fact.fact
                    : "Lokasi ini menyimpan kekayaan budaya Batak Toba yang luar biasa.";
            },
            err =>
            {
                if (factTxt != null)
                    factTxt.text = "Tidak dapat memuat fakta saat ini.";
            });
    }
}