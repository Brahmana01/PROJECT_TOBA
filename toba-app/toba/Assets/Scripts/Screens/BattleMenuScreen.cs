using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Battle lobby TOBA - Modern Batak Arena.
/// UI dibuat secara runtime tanpa bergantung pada sprite panel kayu lama.
/// Menggunakan TextMeshPro untuk tipografi dan DOTween untuk animasi.
/// Alur API dan navigasi dipertahankan dari BattleMenuScreen sebelumnya.
/// </summary>
public class BattleMenuScreen : BaseScreen
{
    // ---------------------------------------------------------------------
    // PALET WARNA
    // ---------------------------------------------------------------------

    // Palet visual terinspirasi Ulos dan Gorga Batak Toba.
    // Marun, merah bata, hitam, krem, dan emas menjadi warna utama.
    private static readonly Color C_BG_TOP       = Hex("#4A2428");
    private static readonly Color C_BG_BOTTOM    = Hex("#120F13");
    private static readonly Color C_PANEL_TOP    = Hex("#54272A");
    private static readonly Color C_PANEL_BOTTOM = Hex("#28171B");
    private static readonly Color C_GOLD         = Hex("#D7A84D");
    private static readonly Color C_GOLD_LIGHT   = Hex("#F5DDA5");
    private static readonly Color C_CREAM        = Hex("#F7EAD2");
    private static readonly Color C_MUTED        = Hex("#D7C5AA");
    private static readonly Color C_GREEN_TOP    = Hex("#47705B");
    private static readonly Color C_GREEN_BOTTOM = Hex("#263F35");
    private static readonly Color C_RED          = Hex("#D95A48");
    private static readonly Color C_INPUT        = Hex("#1B1115");
    private static readonly Color C_GORGA_RED    = Hex("#A93232");
    private static readonly Color C_GORGA_BLACK  = Hex("#211619");

    // Seluruh sprite dibuat runtime. Tidak membutuhkan aset kayu lama.
    private static Sprite roundedSprite;
    private static Sprite circleSprite;
    private static Sprite backgroundGradientSprite;
    private static Sprite glowSprite;
    private static Sprite goldButtonSprite;
    private static Sprite greenButtonSprite;
    private static Sprite redButtonSprite;
    private static Sprite panelGradientSprite;
    private static Sprite ulosPatternSprite;

    // ---------------------------------------------------------------------
    // STATE
    // ---------------------------------------------------------------------

    private TMP_InputField roomCodeInput;
    private TextMeshProUGUI statusTxt;

    private Button createButton;
    private Button joinButton;
    private Button autoMatchButton;

    private bool isBusy;
    private readonly List<Tween> activeTweens = new List<Tween>();

    private GameObject createCard;
    private GameObject joinCard;
    private GameObject autoCard;
    private GameObject heroCard;
    private Transform vsMedallion;

    public BattleMenuScreen(Transform parent) : base(parent, "BattleMenu") { }

    // ---------------------------------------------------------------------
    // SHOW
    // ---------------------------------------------------------------------

    public override void Show()
    {
        SetBackground(C_BG_BOTTOM);
        EnsureRuntimeSprites();

        CreateBackground();
        CreateHeader();
        CreateHero();
        CreateCreateRoomCard();
        CreateJoinRoomCard();
        CreateQuickMatchCard();
        CreateFooter();

        // Entrance berurutan agar layar terasa hidup, tetapi tidak berlebihan.
        AnimateEntrance(heroCard, 0.04f, new Vector2(0f, -22f));
        AnimateEntrance(createCard, 0.12f, new Vector2(0f, -22f));
        AnimateEntrance(joinCard, 0.20f, new Vector2(0f, -22f));
        AnimateEntrance(autoCard, 0.28f, new Vector2(0f, -22f));

        if (vsMedallion != null)
        {
            Tween pulse = vsMedallion
                .DOScale(1.055f, 1.25f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);

            activeTweens.Add(pulse);
        }
    }

    // ---------------------------------------------------------------------
    // BACKGROUND
    // ---------------------------------------------------------------------

    private void CreateBackground()
    {
        GameObject bg = new GameObject(
            "TobaLakeAtDuskBackground",
            typeof(RectTransform),
            typeof(Image)
        );

        bg.transform.SetParent(screenObj.transform, false);
        bg.transform.SetAsFirstSibling();

        RectTransform bgRT = bg.GetComponent<RectTransform>();
        Stretch(bgRT);

        Image bgImage = bg.GetComponent<Image>();
        bgImage.sprite = backgroundGradientSprite;
        bgImage.type = Image.Type.Simple;
        bgImage.color = Color.white;
        bgImage.raycastTarget = false;

        // Lapisan gelap transparan agar teks tetap terbaca di atas lanskap.
        GameObject shade = new GameObject(
            "BackgroundReadabilityShade",
            typeof(RectTransform),
            typeof(Image)
        );
        shade.transform.SetParent(screenObj.transform, false);
        // Background dibuat terlebih dahulu; shade berada tepat di atasnya.

        RectTransform shadeRT = shade.GetComponent<RectTransform>();
        Stretch(shadeRT);

        Image shadeImage = shade.GetComponent<Image>();
        shadeImage.color = new Color(0.10f, 0.035f, 0.045f, 0.26f);
        shadeImage.raycastTarget = false;

        // Cahaya senja lembut pada area Danau Toba.
        CreateGlow(
            screenObj.transform,
            "SunsetGlow",
            new Vector2(700f, 700f),
            new Vector2(330f, 390f),
            new Color(1f, 0.55f, 0.22f, 0.18f)
        );

        // Garis motif Ulos di bagian atas dan bawah layar.
        CreateUlosBand(
            screenObj.transform,
            "HeaderUlosBand",
            new Vector2(900f, 34f),
            new Vector2(0f, 765f),
            1f
        );

        CreateUlosBand(
            screenObj.transform,
            "FooterUlosBand",
            new Vector2(900f, 34f),
            new Vector2(0f, -865f),
            0.95f
        );

        // Garis bingkai emas tipis sebagai pengikat komposisi.
        CreateAccentLine(
            screenObj.transform,
            "HeaderGoldLine",
            new Vector2(900f, 3f),
            new Vector2(0f, 790f),
            new Color(C_GOLD.r, C_GOLD.g, C_GOLD.b, 0.82f)
        );
    }

    private void CreateGlow(
        Transform parent,
        string objectName,
        Vector2 size,
        Vector2 position,
        Color tint)
    {
        GameObject glow = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        glow.transform.SetParent(parent, false);

        RectTransform rt = glow.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = glow.GetComponent<Image>();
        image.sprite = glowSprite;
        image.color = tint;
        image.raycastTarget = false;
    }

    private void CreateAccentLine(
        Transform parent,
        string objectName,
        Vector2 size,
        Vector2 position,
        Color color)
    {
        GameObject line = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        line.transform.SetParent(parent, false);

        RectTransform rt = line.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = line.GetComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
    }

    // ---------------------------------------------------------------------
    // HEADER
    // ---------------------------------------------------------------------

    private void CreateHeader()
    {
        CreateButton(
            screenObj.transform,
            "BtnBack",
            "‹  KEMBALI",
            new Vector2(176f, 68f),
            new Vector2(-385f, 858f),
            ButtonVisual.Dark,
            () =>
            {
                PlayClick();

                if (ScreenManager.Instance != null)
                {
                    ScreenManager.Instance.ChangeScreen(
                        new MainMenuScreen(container)
                    );
                }
            }
        );

        AddText(
            screenObj.transform,
            "BATTLE QUIZ",
            58,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(650f, 76f),
            new Vector2(30f, 858f),
            FontStyles.Bold,
            false
        );

        AddText(
            screenObj.transform,
            "ARENA PERSAUDARAAN BATAK TOBA",
            23,
            C_CREAM,
            TextAlignmentOptions.Center,
            new Vector2(700f, 36f),
            new Vector2(30f, 808f),
            FontStyles.Bold,
            false
        );

        AddText(
            screenObj.transform,
            "HORAS!",
            25,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(200f, 34f),
            new Vector2(0f, 735f),
            FontStyles.Bold,
            false
        );
    }

    // ---------------------------------------------------------------------
    // HERO / VS
    // ---------------------------------------------------------------------

    private void CreateHero()
    {
        heroCard = CreateSurface(
            "BatakArenaHero",
            new Vector2(940f, 255f),
            new Vector2(0f, 560f),
            C_PANEL_TOP
        );

        CreatePill(
            heroCard.transform,
            "ArenaBadge",
            "HORAS • ARENA BATAK TOBA",
            new Vector2(370f, 42f),
            new Vector2(0f, 75f),
            C_GORGA_BLACK,
            C_GOLD_LIGHT,
            21
        );

        AddText(
            heroCard.transform,
            "SIAP MENGUJI PENGETAHUANMU?",
            43,
            C_CREAM,
            TextAlignmentOptions.Center,
            new Vector2(790f, 58f),
            new Vector2(0f, 34f),
            FontStyles.Bold,
            false
        );

        CreatePill(
            heroCard.transform,
            "ExplorerSide",
            "PENJELAJAH",
            new Vector2(235f, 58f),
            new Vector2(-285f, -43f),
            Hex("#2A171B"),
            C_GOLD_LIGHT,
            25
        );

        CreatePill(
            heroCard.transform,
            "OpponentSide",
            "PENANTANG",
            new Vector2(235f, 58f),
            new Vector2(285f, -43f),
            Hex("#2A171B"),
            C_GOLD_LIGHT,
            25
        );

        GameObject medallion = CreateCircle(
            heroCard.transform,
            "GorgaVSOuter",
            new Vector2(132f, 132f),
            new Vector2(0f, -43f),
            C_GORGA_RED
        );

        GameObject goldRing = CreateCircle(
            medallion.transform,
            "GorgaVSGoldRing",
            new Vector2(116f, 116f),
            Vector2.zero,
            C_GOLD
        );

        GameObject inner = CreateCircle(
            goldRing.transform,
            "GorgaVSInner",
            new Vector2(98f, 98f),
            Vector2.zero,
            C_GORGA_BLACK
        );

        AddText(
            inner.transform,
            "VS",
            57,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(92f, 72f),
            Vector2.zero,
            FontStyles.Bold,
            false
        );

        vsMedallion = medallion.transform;

        AddText(
            heroCard.transform,
            "BERTANDING • BELAJAR • MENJAGA SEMANGAT DALIHAN NA TOLU",
            21,
            C_CREAM,
            TextAlignmentOptions.Center,
            new Vector2(850f, 30f),
            new Vector2(0f, -111f),
            FontStyles.Bold,
            false
        );
    }

    // ---------------------------------------------------------------------
    // CREATE ROOM
    // ---------------------------------------------------------------------

    private void CreateCreateRoomCard()
    {
        createCard = CreateSurface(
            "CreateRoomCard",
            new Vector2(940f, 230f),
            new Vector2(0f, 245f),
            C_PANEL_TOP
        );

        CreateNumberBadge(
            createCard.transform,
            "HostNumber",
            "01",
            new Vector2(-395f, 7f)
        );

        CreatePill(
            createCard.transform,
            "HostTag",
            "PEMBUAT ROOM",
            new Vector2(255f, 38f),
            new Vector2(-155f, 68f),
            C_GORGA_BLACK,
            C_GOLD_LIGHT,
            18
        );

        AddText(
            createCard.transform,
            "BUAT ROOM BARU",
            49,
            C_CREAM,
            TextAlignmentOptions.Left,
            new Vector2(570f, 64f),
            new Vector2(-65f, 28f),
            FontStyles.Bold,
            false
        );

        AddText(
            createCard.transform,
            "Buka arena sendiri dan undang temanmu\nbertanding menggunakan kode room.",
            25,
            C_MUTED,
            TextAlignmentOptions.Left,
            new Vector2(590f, 68f),
            new Vector2(-55f, -49f),
            FontStyles.Normal,
            true
        );

        createButton = CreateButton(
            createCard.transform,
            "BtnCreateRoom",
            "BUAT ROOM",
            new Vector2(220f, 82f),
            new Vector2(337f, -22f),
            ButtonVisual.Gold,
            OnCreateRoom
        );
    }

    // ---------------------------------------------------------------------
    // JOIN ROOM
    // ---------------------------------------------------------------------

    private void CreateJoinRoomCard()
    {
        joinCard = CreateSurface(
            "JoinRoomCard",
            new Vector2(940f, 278f),
            new Vector2(0f, -45f),
            C_PANEL_TOP
        );

        CreateNumberBadge(
            joinCard.transform,
            "JoinNumber",
            "02",
            new Vector2(-395f, 88f)
        );

        CreatePill(
            joinCard.transform,
            "JoinTag",
            "PESERTA ROOM",
            new Vector2(225f, 38f),
            new Vector2(-160f, 91f),
            C_GORGA_BLACK,
            C_GOLD_LIGHT,
            18
        );

        AddText(
            joinCard.transform,
            "GABUNG ROOM",
            49,
            C_CREAM,
            TextAlignmentOptions.Left,
            new Vector2(570f, 60f),
            new Vector2(-65f, 47f),
            FontStyles.Bold,
            false
        );

        AddText(
            joinCard.transform,
            "Masukkan kode dari teman untuk masuk ke arena.",
            24,
            C_MUTED,
            TextAlignmentOptions.Left,
            new Vector2(700f, 38f),
            new Vector2(-20f, 3f),
            FontStyles.Normal,
            false
        );

        roomCodeInput = CreateRoomCodeInput(
            joinCard.transform,
            new Vector2(565f, 82f),
            new Vector2(-120f, -68f)
        );

        joinButton = CreateButton(
            joinCard.transform,
            "BtnJoinRoom",
            "GABUNG",
            new Vector2(205f, 82f),
            new Vector2(333f, -68f),
            ButtonVisual.Gold,
            OnJoinRoom
        );

        AddText(
            joinCard.transform,
            "KODE ROOM TERDIRI DARI 4–6 KARAKTER",
            19,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(600f, 28f),
            new Vector2(-105f, -124f),
            FontStyles.Bold,
            false
        );
    }

    private TMP_InputField CreateRoomCodeInput(
        Transform parent,
        Vector2 size,
        Vector2 position)
    {
        GameObject inputRoot = new GameObject(
            "RoomCodeInput",
            typeof(RectTransform),
            typeof(Image)
        );

        inputRoot.transform.SetParent(parent, false);

        RectTransform inputRT = inputRoot.GetComponent<RectTransform>();
        SetCentered(inputRT, size, position);

        Image inputImage = inputRoot.GetComponent<Image>();
        inputImage.sprite = roundedSprite;
        inputImage.type = Image.Type.Sliced;
        inputImage.color = C_INPUT;
        inputImage.raycastTarget = true;

        Outline outline = inputRoot.AddComponent<Outline>();
        outline.effectColor = new Color(C_GOLD.r, C_GOLD.g, C_GOLD.b, 0.75f);
        outline.effectDistance = new Vector2(2f, 2f);
        outline.useGraphicAlpha = true;

        GameObject viewportObject = new GameObject(
            "TextViewport",
            typeof(RectTransform),
            typeof(RectMask2D)
        );

        viewportObject.transform.SetParent(inputRoot.transform, false);

        RectTransform viewportRT = viewportObject.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(20f, 8f);
        viewportRT.offsetMax = new Vector2(-20f, -8f);

        TextMeshProUGUI textComponent = CreateInputText(
            viewportObject.transform,
            "",
            C_CREAM,
            FontStyles.Bold
        );

        TextMeshProUGUI placeholder = CreateInputText(
            viewportObject.transform,
            "MASUKKAN KODE ROOM",
            new Color(C_MUTED.r, C_MUTED.g, C_MUTED.b, 0.75f),
            FontStyles.Normal
        );

        TMP_InputField input = inputRoot.AddComponent<TMP_InputField>();
        input.textViewport = viewportRT;
        input.textComponent = textComponent;
        input.placeholder = placeholder;
        input.targetGraphic = inputImage;
        input.characterLimit = 6;
        input.contentType = TMP_InputField.ContentType.Alphanumeric;
        input.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.selectionColor = new Color(C_GOLD.r, C_GOLD.g, C_GOLD.b, 0.35f);
        input.caretWidth = 3;

        input.onValueChanged.AddListener(value =>
        {
            string normalized = value.Trim().ToUpperInvariant().Replace(" ", "");

            if (normalized != value)
                input.SetTextWithoutNotify(normalized);
        });

        return input;
    }

    private TextMeshProUGUI CreateInputText(
        Transform parent,
        string value,
        Color color,
        FontStyles style)
    {
        GameObject textObject = new GameObject(
            string.IsNullOrEmpty(value) ? "InputText" : "InputPlaceholder",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(parent, false);

        RectTransform rt = textObject.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4f, 0f);
        rt.offsetMax = new Vector2(-4f, 0f);

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = 34f;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        return tmp;
    }

    // ---------------------------------------------------------------------
    // QUICK MATCH
    // ---------------------------------------------------------------------

    private void CreateQuickMatchCard()
    {
        autoCard = CreateSurface(
            "AutoMatchCard",
            new Vector2(940f, 220f),
            new Vector2(0f, -385f),
            C_PANEL_TOP
        );

        CreateNumberBadge(
            autoCard.transform,
            "QuickMatchNumber",
            "03",
            new Vector2(-395f, 60f)
        );

        CreatePill(
            autoCard.transform,
            "QuickMatchTag",
            "TANDINGAN CEPAT",
            new Vector2(300f, 38f),
            new Vector2(-115f, 66f),
            C_GORGA_BLACK,
            C_GOLD_LIGHT,
            18
        );

        AddText(
            autoCard.transform,
            "CARI LAWAN OTOMATIS",
            45,
            C_CREAM,
            TextAlignmentOptions.Center,
            new Vector2(760f, 58f),
            new Vector2(0f, 24f),
            FontStyles.Bold,
            false
        );

        AddText(
            autoCard.transform,
            "Temukan penjelajah lain dan mulai pertandingan.",
            25,
            C_MUTED,
            TextAlignmentOptions.Center,
            new Vector2(780f, 38f),
            new Vector2(0f, -24f),
            FontStyles.Normal,
            false
        );

        autoMatchButton = CreateButton(
            autoCard.transform,
            "BtnAutoMatch",
            "CARI LAWAN",
            new Vector2(460f, 76f),
            new Vector2(0f, -77f),
            ButtonVisual.Red,
            OnAutoMatchmaking
        );
    }

    // ---------------------------------------------------------------------
    // FOOTER
    // ---------------------------------------------------------------------

    private void CreateFooter()
    {
        AddText(
            screenObj.transform,
            "PILIH CARA BERTANDING DAN AJAK SAHABATMU!",
            22,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(920f, 38f),
            new Vector2(0f, -585f),
            FontStyles.Bold,
            false
        );

        AddText(
            screenObj.transform,
            "DALIHAN NA TOLU • SALING MENGHORMATI • BERMAIN SPORTIF",
            22,
            C_CREAM,
            TextAlignmentOptions.Center,
            new Vector2(920f, 38f),
            new Vector2(0f, -690f),
            FontStyles.Bold,
            false
        );

        statusTxt = AddText(
            screenObj.transform,
            "",
            23,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(900f, 58f),
            new Vector2(0f, -755f),
            FontStyles.Bold,
            true
        );
    }

    // ---------------------------------------------------------------------
    // REUSABLE VISUAL HELPERS
    // ---------------------------------------------------------------------

    private GameObject CreateSurface(
        string objectName,
        Vector2 size,
        Vector2 position,
        Color fillTint)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform));
        root.transform.SetParent(screenObj.transform, false);

        RectTransform rootRT = root.GetComponent<RectTransform>();
        SetCentered(rootRT, size, position);

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        GameObject borderObject = new GameObject(
            "GorgaGoldOuterFrame",
            typeof(RectTransform),
            typeof(Image)
        );

        borderObject.transform.SetParent(root.transform, false);

        RectTransform borderRT = borderObject.GetComponent<RectTransform>();
        Stretch(borderRT);

        Image borderImage = borderObject.GetComponent<Image>();
        borderImage.sprite = roundedSprite;
        borderImage.type = Image.Type.Sliced;
        borderImage.color = C_GOLD;
        borderImage.raycastTarget = false;

        Shadow shadow = borderObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.effectDistance = new Vector2(0f, -10f);
        shadow.useGraphicAlpha = true;

        GameObject fillObject = new GameObject(
            "MaroonPanelFill",
            typeof(RectTransform),
            typeof(Image)
        );

        fillObject.transform.SetParent(root.transform, false);

        RectTransform fillRT = fillObject.GetComponent<RectTransform>();
        Stretch(fillRT);
        fillRT.offsetMin = new Vector2(5f, 5f);
        fillRT.offsetMax = new Vector2(-5f, -5f);

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.sprite = panelGradientSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = Color.Lerp(Color.white, fillTint, 0.12f);
        fillImage.raycastTarget = false;

        // Tenunan Ulos sebagai detail tepi panel.
        CreateUlosBand(
            fillObject.transform,
            "UlosTopTrim",
            new Vector2(size.x - 24f, 18f),
            new Vector2(0f, size.y * 0.5f - 18f),
            0.96f
        );

        // Garis merah tipis di bawah panel untuk aksen Gorga.
        GameObject lowerAccent = new GameObject(
            "GorgaLowerAccent",
            typeof(RectTransform),
            typeof(Image)
        );
        lowerAccent.transform.SetParent(fillObject.transform, false);

        RectTransform lowerRT = lowerAccent.GetComponent<RectTransform>();
        lowerRT.anchorMin = new Vector2(0f, 0f);
        lowerRT.anchorMax = new Vector2(1f, 0f);
        lowerRT.pivot = new Vector2(0.5f, 0f);
        lowerRT.sizeDelta = new Vector2(-34f, 4f);
        lowerRT.anchoredPosition = new Vector2(0f, 8f);

        Image lowerImage = lowerAccent.GetComponent<Image>();
        lowerImage.sprite = roundedSprite;
        lowerImage.type = Image.Type.Sliced;
        lowerImage.color = C_GORGA_RED;
        lowerImage.raycastTarget = false;

        // Ornamen sudut kecil agar panel tidak terlihat seperti kartu dashboard generik.
        CreateGorgaCorner(fillObject.transform, "GorgaCornerLeft", new Vector2(-size.x * 0.5f + 25f, -size.y * 0.5f + 24f));
        CreateGorgaCorner(fillObject.transform, "GorgaCornerRight", new Vector2(size.x * 0.5f - 25f, -size.y * 0.5f + 24f));

        return root;
    }

    private void CreateUlosBand(
        Transform parent,
        string objectName,
        Vector2 size,
        Vector2 position,
        float alpha)
    {
        if (ulosPatternSprite == null)
            return;

        GameObject band = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );
        band.transform.SetParent(parent, false);

        RectTransform rt = band.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = band.GetComponent<Image>();
        image.sprite = ulosPatternSprite;
        image.type = Image.Type.Simple;
        image.color = new Color(1f, 1f, 1f, alpha);
        image.raycastTarget = false;
    }

    private void CreateGorgaCorner(
        Transform parent,
        string objectName,
        Vector2 position)
    {
        GameObject corner = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );
        corner.transform.SetParent(parent, false);

        RectTransform rt = corner.GetComponent<RectTransform>();
        SetCentered(rt, new Vector2(34f, 34f), position);
        rt.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image image = corner.GetComponent<Image>();
        image.sprite = circleSprite;
        image.color = C_GORGA_RED;
        image.raycastTarget = false;

        CreateCircle(
            corner.transform,
            "GorgaGoldCenter",
            new Vector2(13f, 13f),
            Vector2.zero,
            C_GOLD_LIGHT
        );
    }

    private GameObject CreateCircle(
        Transform parent,
        string objectName,
        Vector2 size,
        Vector2 position,
        Color color)
    {
        GameObject circle = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        circle.transform.SetParent(parent, false);

        RectTransform rt = circle.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = circle.GetComponent<Image>();
        image.sprite = circleSprite;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;

        return circle;
    }

    private void CreateNumberBadge(
        Transform parent,
        string objectName,
        string number,
        Vector2 position)
    {
        GameObject outer = CreateCircle(
            parent,
            objectName,
            new Vector2(74f, 74f),
            position,
            C_GOLD
        );

        CreateCircle(
            outer.transform,
            "BadgeInner",
            new Vector2(64f, 64f),
            Vector2.zero,
            C_GORGA_BLACK
        );

        AddText(
            outer.transform,
            number,
            29,
            C_GOLD_LIGHT,
            TextAlignmentOptions.Center,
            new Vector2(60f, 42f),
            Vector2.zero,
            FontStyles.Bold,
            false
        );
    }

    private void CreatePill(
        Transform parent,
        string objectName,
        string label,
        Vector2 size,
        Vector2 position,
        Color background,
        Color textColor,
        int fontSize)
    {
        GameObject pill = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image)
        );

        pill.transform.SetParent(parent, false);

        RectTransform rt = pill.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = pill.GetComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = background;
        image.raycastTarget = false;

        Outline outline = pill.AddComponent<Outline>();
        outline.effectColor = new Color(C_GOLD.r, C_GOLD.g, C_GOLD.b, 0.65f);
        outline.effectDistance = new Vector2(1.5f, 1.5f);
        outline.useGraphicAlpha = true;

        AddText(
            pill.transform,
            label,
            fontSize,
            textColor,
            TextAlignmentOptions.Center,
            size - new Vector2(12f, 4f),
            Vector2.zero,
            FontStyles.Bold,
            false
        );
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 size,
        Vector2 position,
        ButtonVisual visual,
        Action onClick)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(parent, false);

        RectTransform rt = buttonObject.GetComponent<RectTransform>();
        SetCentered(rt, size, position);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = visual == ButtonVisual.Gold
            ? goldButtonSprite
            : visual == ButtonVisual.Green
                ? greenButtonSprite
                : visual == ButtonVisual.Red
                    ? redButtonSprite
                    : roundedSprite;

        image.type = Image.Type.Sliced;
        image.color = visual == ButtonVisual.Dark
            ? C_GORGA_BLACK
            : Color.white;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.55f, 0.58f, 0.55f, 0.55f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        if (visual == ButtonVisual.Dark)
        {
            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = C_GOLD;
            outline.effectDistance = new Vector2(2f, 2f);
        }

        Color labelColor = visual == ButtonVisual.Gold
            ? Hex("#382016")
            : C_CREAM;

        AddText(
            buttonObject.transform,
            label,
            visual == ButtonVisual.Dark ? 25 : 34,
            labelColor,
            TextAlignmentOptions.Center,
            size - new Vector2(18f, 12f),
            Vector2.zero,
            FontStyles.Bold,
            false
        );

        button.onClick.AddListener(() =>
        {
            AnimateTap(buttonObject.transform);
            onClick?.Invoke();
        });

        return button;
    }

    private TextMeshProUGUI AddText(
        Transform parent,
        string value,
        int fontSize,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 size,
        Vector2 position,
        FontStyles style,
        bool wrap)
    {
        GameObject textObject = UIBuilder.CreateTextSized(
            parent,
            value,
            fontSize,
            color,
            alignment,
            size,
            position
        );

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = wrap
            ? TextWrappingModes.Normal
            : TextWrappingModes.NoWrap;

        tmp.margin = new Vector4(8f, 4f, 8f, 4f);
        return tmp;
    }

    // ---------------------------------------------------------------------
    // ANIMATIONS - DOTWEEN
    // ---------------------------------------------------------------------

    private void AnimateEntrance(GameObject target, float delay, Vector2 offset)
    {
        if (target == null)
            return;

        RectTransform rect = target.GetComponent<RectTransform>();
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (rect == null)
            return;

        if (group == null)
            group = target.AddComponent<CanvasGroup>();

        Vector2 finalPosition = rect.anchoredPosition;

        group.alpha = 0f;
        rect.anchoredPosition = finalPosition + offset;
        rect.localScale = Vector3.one * 0.96f;

        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);
        sequence.AppendInterval(delay);
        sequence.Append(group.DOFade(1f, 0.32f));
        sequence.Join(
            rect.DOAnchorPos(finalPosition, 0.48f)
                .SetEase(Ease.OutCubic)
        );
        sequence.Join(
            rect.DOScale(1f, 0.48f)
                .SetEase(Ease.OutBack)
        );

        activeTweens.Add(sequence);
    }

    private void AnimateTap(Transform target)
    {
        if (target == null)
            return;

        target.DOKill();
        target.localScale = Vector3.one;

        Tween tween = target
            .DOPunchScale(new Vector3(0.045f, 0.045f, 0f), 0.22f, 5, 0.65f)
            .SetUpdate(true);

        activeTweens.Add(tween);
    }

    private void PlayClick()
    {
        SoundManager.Instance?.PlaySFX("sfx_click");
        HapticManager.Instance?.Vibrate(HapticType.Light);
    }

    // ---------------------------------------------------------------------
    // ACTIONS - API FLOW DIPERTAHANKAN
    // ---------------------------------------------------------------------

    private void OnCreateRoom()
    {
        if (isBusy)
            return;

        PlayClick();

        int playerID = PlayerPrefs.GetInt("PlayerID", 0);

        if (playerID == 0)
        {
            ShowStatus("Kamu harus login terlebih dahulu.", true);
            return;
        }

        if (APIManager.Instance == null)
        {
            ShowStatus("Layanan server belum siap. Coba lagi.", true);
            return;
        }

        SetBusy(true);
        ShowStatus("Menyiapkan arena dan membuat room...", false);

        CreateRoomRequest req = new CreateRoomRequest
        {
            player_id = playerID
        };

        APIManager.Instance.PostRequest(
            "/battle/create",
            JsonUtility.ToJson(req),
            response =>
            {
                if (screenObj == null)
                    return;

                SetBusy(false);

                CreateRoomResponse res = null;

                try
                {
                    res = JsonUtility.FromJson<CreateRoomResponse>(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        "[Battle] Gagal membaca response create room: " + ex.Message
                    );
                }

                if (res != null && !string.IsNullOrEmpty(res.room_code))
                {
                    ShowStatus("Room berhasil dibuat. Menyiapkan lobby...", false);

                    if (ScreenManager.Instance != null)
                    {
                        ScreenManager.Instance.ChangeScreen(
                            new BattleWaitingScreen(
                                container,
                                res.room_code,
                                res.room_id,
                                true
                            )
                        );
                    }
                }
                else
                {
                    ShowStatus("Gagal membuat room. Silakan coba lagi.", true);
                }
            },
            err =>
            {
                if (screenObj == null)
                    return;

                SetBusy(false);

                Debug.LogWarning("[Battle] Create room gagal: " + err);
                ShowStatus("Koneksi gagal. Periksa internet dan coba lagi.", true);
            }
        );
    }

    private void OnJoinRoom()
    {
        if (isBusy)
            return;

        PlayClick();

        string code = roomCodeInput != null
            ? roomCodeInput.text.Trim().ToUpperInvariant()
            : "";

        if (string.IsNullOrEmpty(code) || code.Length < 4)
        {
            ShowStatus("Masukkan kode room yang valid.", true);
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID", 0);

        if (playerID == 0)
        {
            ShowStatus("Kamu harus login terlebih dahulu.", true);
            return;
        }

        if (APIManager.Instance == null)
        {
            ShowStatus("Layanan server belum siap. Coba lagi.", true);
            return;
        }

        SetBusy(true);
        ShowStatus("Menghubungkan kamu ke room...", false);

        JoinRoomRequest req = new JoinRoomRequest
        {
            player_id = playerID,
            room_code = code
        };

        APIManager.Instance.PostRequest(
            "/battle/join",
            JsonUtility.ToJson(req),
            response =>
            {
                if (screenObj == null)
                    return;

                SetBusy(false);

                BattleRoomStatusResponse res = null;

                try
                {
                    res = JsonUtility.FromJson<BattleRoomStatusResponse>(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        "[Battle] Gagal membaca response join room: " + ex.Message
                    );
                }

                if (res != null && res.room_id > 0)
                {
                    if (ScreenManager.Instance != null)
                    {
                        ScreenManager.Instance.ChangeScreen(
                            new BattleQuizScreen(
                                container,
                                res,
                                false
                            )
                        );
                    }
                }
                else
                {
                    ShowStatus("Room tidak ditemukan atau sudah penuh.", true);
                }
            },
            err =>
            {
                if (screenObj == null)
                    return;

                SetBusy(false);

                Debug.LogWarning("[Battle] Join room gagal: " + err);
                ShowStatus("Gagal bergabung. Periksa kode room dan koneksi.", true);
            }
        );
    }

    private void OnAutoMatchmaking()
    {
        if (isBusy)
            return;

        PlayClick();

        if (ScreenManager.Instance == null)
        {
            ShowStatus("Screen manager belum siap. Coba lagi.", true);
            return;
        }

        ScreenManager.Instance.ChangeScreen(
            new BattleMatchmakingScreen(container)
        );
    }

    // ---------------------------------------------------------------------
    // STATUS / BUSY
    // ---------------------------------------------------------------------

    private void SetBusy(bool busy)
    {
        isBusy = busy;

        if (createButton != null)
            createButton.interactable = !busy;

        if (joinButton != null)
            joinButton.interactable = !busy;

        if (autoMatchButton != null)
            autoMatchButton.interactable = !busy;

        if (roomCodeInput != null)
            roomCodeInput.interactable = !busy;
    }

    private void ShowStatus(string message, bool isError)
    {
        if (statusTxt == null)
            return;

        statusTxt.text = message;
        statusTxt.color = isError ? C_RED : C_GOLD_LIGHT;

        statusTxt.transform.DOKill();
        statusTxt.transform.localScale = Vector3.one * 0.96f;

        Tween tween = statusTxt.transform
            .DOScale(1f, 0.22f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        activeTweens.Add(tween);
    }

    // ---------------------------------------------------------------------
    // RUNTIME SPRITE GENERATION
    // ---------------------------------------------------------------------

    private static void EnsureRuntimeSprites()
    {
        if (roundedSprite != null &&
            circleSprite != null &&
            backgroundGradientSprite != null &&
            glowSprite != null &&
            goldButtonSprite != null &&
            greenButtonSprite != null &&
            redButtonSprite != null &&
            panelGradientSprite != null &&
            ulosPatternSprite != null)
        {
            return;
        }

        roundedSprite = CreateRoundedMaskSprite(
            "TOBA_Gorga_Rounded",
            256,
            64,
            15f,
            new Vector4(16f, 16f, 16f, 16f)
        );

        circleSprite = CreateRoundedMaskSprite(
            "TOBA_Gorga_Circle",
            64,
            64,
            32f,
            new Vector4(24f, 24f, 24f, 24f)
        );

        backgroundGradientSprite = CreateTobaLandscapeSprite(
            "TOBA_DanauToba_Dusk",
            512,
            1024
        );

        glowSprite = CreateRadialGlowSprite(
            "TOBA_SunsetGlow",
            128
        );

        goldButtonSprite = CreateRoundedGradientSprite(
            "TOBA_GoldButton",
            256,
            64,
            15f,
            Hex("#F4D88F"),
            Hex("#B97A2E"),
            new Vector4(16f, 16f, 16f, 16f)
        );

        greenButtonSprite = CreateRoundedGradientSprite(
            "TOBA_GreenButton",
            256,
            64,
            15f,
            C_GREEN_TOP,
            C_GREEN_BOTTOM,
            new Vector4(16f, 16f, 16f, 16f)
        );

        redButtonSprite = CreateRoundedGradientSprite(
            "TOBA_GorgaRedButton",
            256,
            64,
            15f,
            Hex("#D85B48"),
            Hex("#8E292B"),
            new Vector4(16f, 16f, 16f, 16f)
        );

        panelGradientSprite = CreateRoundedGradientSprite(
            "TOBA_MaroonPanel",
            256,
            64,
            15f,
            Hex("#653035"),
            Hex("#2A171B"),
            new Vector4(16f, 16f, 16f, 16f)
        );

        ulosPatternSprite = CreateUlosPatternSprite(
            "TOBA_UlosWeave",
            512,
            48
        );
    }

    private static Sprite CreateTobaLandscapeSprite(
        string spriteName,
        int width,
        int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        Color skyTop = Hex("#32191F");
        Color skyMid = Hex("#8A4540");
        Color sunset = Hex("#D18B52");
        Color lake = Hex("#172B31");
        Color farMountain = Hex("#493039");
        Color nearMountain = Hex("#28191F");

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            float ny = (float)y / (height - 1);

            Color rowColor;
            if (ny > 0.66f)
            {
                float t = Mathf.InverseLerp(0.66f, 1f, ny);
                rowColor = Color.Lerp(skyMid, skyTop, t);
            }
            else if (ny > 0.57f)
            {
                float t = Mathf.InverseLerp(0.57f, 0.66f, ny);
                rowColor = Color.Lerp(sunset, skyMid, t);
            }
            else
            {
                float t = Mathf.InverseLerp(0.12f, 0.57f, ny);
                rowColor = Color.Lerp(lake, Hex("#344044"), t);
            }

            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / (width - 1);
                float ridgeFar =
                    0.535f +
                    0.035f * Mathf.Sin(nx * 12.5f) +
                    0.018f * Mathf.Sin(nx * 29f + 1.2f);

                float ridgeNear =
                    0.485f +
                    0.045f * Mathf.Sin(nx * 9.5f + 1.7f) +
                    0.022f * Mathf.Sin(nx * 24f);

                Color pixel = rowColor;

                // Dua lapis siluet perbukitan di atas permukaan danau.
                // Area di bawah garis pantai tetap berupa air, bukan tertutup gunung.
                if (ny >= 0.455f && ny <= ridgeFar)
                    pixel = farMountain;

                if (ny >= 0.435f && ny <= ridgeNear)
                    pixel = nearMountain;

                // Permukaan Danau Toba: garis pantulan cahaya horizontal.
                if (ny < 0.43f && ny > 0.16f)
                {
                    int waveBand = (int)(ny * 520f);
                    float wave = Mathf.Abs(
                        Mathf.Sin(nx * 82f + waveBand * 0.12f)
                    );

                    if (wave > 0.965f && ((x + waveBand * 3) % 19) < 13)
                        pixel = Color.Lerp(pixel, Hex("#A66E52"), 0.30f);
                }

                // Kilau senja di tengah danau.
                float glowX = Mathf.Abs(nx - 0.52f);
                float glowY = Mathf.Abs(ny - 0.43f);
                float glow = Mathf.Clamp01(1f - glowX * 3.5f - glowY * 5f);
                if (ny < 0.46f && ny > 0.28f)
                    pixel = Color.Lerp(pixel, Hex("#A96A4C"), glow * 0.20f);

                pixels[y * width + x] = pixel;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private static Sprite CreateUlosPatternSprite(
        string spriteName,
        int width,
        int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.hideFlags = HideFlags.DontSave;

        Color maroon = Hex("#3A171D");
        Color red = Hex("#B43B35");
        Color cream = Hex("#E9D6B1");
        Color gold = Hex("#D5A64C");
        Color black = Hex("#211317");

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = maroon;

                // Garis tenun horizontal.
                if (y < 3 || y >= height - 3)
                    color = gold;
                else if (y >= 5 && y < 8)
                    color = cream;
                else if (y >= height - 9 && y < height - 6)
                    color = red;
                else if (y >= 21 && y <= 25)
                    color = black;

                // Motif belah ketupat berulang, terinspirasi susunan tenun Ulos.
                int localX = x % 40;
                int localY = y - 24;
                int dx = Mathf.Abs(localX - 20);
                int dy = Mathf.Abs(localY);

                if (dx + dy <= 15)
                    color = gold;
                if (dx + dy <= 10)
                    color = red;
                if (dx + dy <= 5)
                    color = cream;

                // Sisik/zigzag kecil di sela motif.
                if ((localX < 4 || localX > 36) && y > 10 && y < height - 10)
                {
                    if (((x + y) % 8) < 3)
                        color = cream;
                }

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private static Sprite CreateRoundedMaskSprite(
        string spriteName,
        int width,
        int height,
        float radius,
        Vector4 border)
    {
        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = RoundedBoxDistance(
                    x + 0.5f,
                    y + 0.5f,
                    width,
                    height,
                    radius
                );

                byte alpha = (byte)(
                    Mathf.Clamp01(0.5f - distance) * 255f
                );

                pixels[y * width + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border
        );

        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;

        return sprite;
    }

    private static Sprite CreateRoundedGradientSprite(
        string spriteName,
        int width,
        int height,
        float radius,
        Color topColor,
        Color bottomColor,
        Vector4 border)
    {
        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            float t = height > 1 ? (float)y / (height - 1) : 1f;
            Color gradient = Color.Lerp(bottomColor, topColor, t);

            for (int x = 0; x < width; x++)
            {
                float distance = RoundedBoxDistance(
                    x + 0.5f,
                    y + 0.5f,
                    width,
                    height,
                    radius
                );

                float alpha = Mathf.Clamp01(0.5f - distance);
                Color pixel = gradient;
                pixel.a *= alpha;

                pixels[y * width + x] = pixel;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border
        );

        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;

        return sprite;
    }

    private static Sprite CreateVerticalGradientSprite(
        string spriteName,
        int width,
        int height,
        Color bottomColor,
        Color topColor)
    {
        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            float t = height > 1 ? (float)y / (height - 1) : 1f;
            Color color = Color.Lerp(bottomColor, topColor, t);

            for (int x = 0; x < width; x++)
                pixels[y * width + x] = color;
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;

        return sprite;
    }

    private static Sprite CreateRadialGlowSprite(string spriteName, int size)
    {
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        texture.name = spriteName + "_Texture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;

        Color32[] pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxDistance = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x + 0.5f, y + 0.5f),
                    center
                );

                float t = Mathf.Clamp01(distance / maxDistance);
                float alpha = Mathf.Pow(1f - t, 2.6f);

                pixels[y * size + x] = new Color32(
                    255,
                    255,
                    255,
                    (byte)(alpha * 255f)
                );
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;

        return sprite;
    }

    private static float RoundedBoxDistance(
        float px,
        float py,
        float width,
        float height,
        float radius)
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        float qx = Mathf.Abs(px - halfW) - (halfW - radius);
        float qy = Mathf.Abs(py - halfH) - (halfH - radius);

        float outside = Mathf.Sqrt(
            Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
            Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f)
        );

        float inside = Mathf.Min(
            Mathf.Max(qx, qy),
            0f
        );

        return outside + inside - radius;
    }

    // ---------------------------------------------------------------------
    // RECT HELPERS
    // ---------------------------------------------------------------------

    private static void SetCentered(
        RectTransform rect,
        Vector2 size,
        Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Color Hex(string value)
    {
        if (ColorUtility.TryParseHtmlString(value, out Color color))
            return color;

        return Color.white;
    }

    private enum ButtonVisual
    {
        Gold,
        Green,
        Dark,
        Red
    }

    // ---------------------------------------------------------------------
    // LIFECYCLE
    // ---------------------------------------------------------------------

    public override void Hide()
    {
        for (int i = 0; i < activeTweens.Count; i++)
        {
            if (activeTweens[i] != null && activeTweens[i].IsActive())
                activeTweens[i].Kill();
        }

        activeTweens.Clear();

        if (screenObj != null)
            screenObj.transform.DOKill(true);

        base.Hide();
    }
}
