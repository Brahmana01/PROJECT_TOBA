using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuScreen : BaseScreen
{
    private TextMeshProUGUI nameTxt;
    private TextMeshProUGUI titleTxt;
    private TextMeshProUGUI starTxt;
    private TextMeshProUGUI levelTxt;
    private GameObject xpBarFill;

    private static Sprite _circleSprite;

    // Karakter player dari backend
    private int selectedCharacterID = 1;

    // Referensi footer
    private GameObject footerGO;

    public MainMenuScreen(Transform parent)
        : base(parent, "MainMenu")
    {
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);

        // ============================================================
        // BACKGROUND
        // ============================================================

        Sprite bgSprite =
            LoadSpriteFlexible("gambar/bg_mainmenu")
            ?? LoadSpriteFlexible("bg_toba");

        UIBuilder.CreateBackground(
            screenObj.transform,
            bgSprite
        );

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0f, 0f, 0.08f),
            Vector2.zero,
            Vector2.one
        );

        // ============================================================
        // HEADER
        // ============================================================

        BuildHeader();

        // ============================================================
        // FOOTER
        // ============================================================

        footerGO =
            FooterNavbar.Create(
                screenObj.transform,
                "Beranda",

                // Home
                () =>
                {
                    // Tetap kosong
                },

                // Gallery
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new GalleryScreen(container)
                    );
                },

                // Battle
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new BattleMenuScreen(container)
                    );
                },

                // Leaderboard
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new LeaderboardScreen(container)
                    );
                },

                // Profile
                () =>
                {
                    ScreenManager.Instance.ChangeScreen(
                        new ProfileScreen(container)
                    );
                }
            );

        // ============================================================
        // MAIN MENU
        // ============================================================

        BuildMenuGrid();

        // ============================================================
        // CHARACTER DECORATION
        // ============================================================

        CreateCharacterDecoration();

        // ============================================================
        // PROFILE
        // ============================================================

        FetchProfile();
    }


    // ============================================================
    // SPRITE LOADER
    // ============================================================

    Sprite LoadSpriteFlexible(string path)
    {
        Sprite sprite =
            Resources.Load<Sprite>(path);

        if (sprite != null)
            return sprite;

        Texture2D tex =
            Resources.Load<Texture2D>(path);

        if (tex != null)
        {
            return Sprite.Create(
                tex,
                new Rect(
                    0,
                    0,
                    tex.width,
                    tex.height
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );
        }

        Debug.LogWarning(
            "[MainMenu] Sprite tidak ditemukan: " + path
        );

        return null;
    }


    // ============================================================
    // HEADER
    // ============================================================

    void BuildHeader()
    {
        Sprite headerSprite =
            LoadSpriteFlexible(
                "gambar/toba_header_panel"
            );

        GameObject header =
            new GameObject("Header");

        header.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rtHeader =
            header.AddComponent<RectTransform>();

        rtHeader.anchorMin =
            new Vector2(
                0f,
                0.84f
            );

        rtHeader.anchorMax =
            Vector2.one;

        rtHeader.offsetMin =
            rtHeader.offsetMax =
            Vector2.zero;

        Image headerImg =
            header.AddComponent<Image>();

        headerImg.sprite =
            headerSprite
            ?? LoadSpriteFlexible("panel_wood")
            ?? UIBuilder.GetDefaultSprite();

        headerImg.type =
            Image.Type.Sliced;

        headerImg.color =
            Color.white;

        headerImg.raycastTarget =
            false;

        Shadow headerShadow =
            header.AddComponent<Shadow>();

        headerShadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.65f
            );

        headerShadow.effectDistance =
            new Vector2(
                0f,
                -5f
            );


        // ============================================================
        // GOLD LINE
        // ============================================================

        GameObject goldLine =
            new GameObject("HeaderGoldLine");

        goldLine.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rtLine =
            goldLine.AddComponent<RectTransform>();

        rtLine.anchorMin =
            new Vector2(
                0f,
                0.837f
            );

        rtLine.anchorMax =
            new Vector2(
                1f,
                0.842f
            );

        rtLine.offsetMin =
            rtLine.offsetMax =
            Vector2.zero;

        Image goldImg =
            goldLine.AddComponent<Image>();

        goldImg.sprite =
            UIBuilder.GetDefaultSprite();

        goldImg.color =
            TOBATheme.BatakGold;

        goldImg.raycastTarget =
            false;


        // ============================================================
        // HEADER AVATAR
        // ============================================================

        selectedCharacterID =
            PlayerPrefs.GetInt(
                "SelectedCharacterID",
                1
            );

        string characterPath =
            selectedCharacterID == 2
                ? "char_butet"
                : "char_ucok";

        Sprite characterSprite =
            LoadSpriteFlexible(
                characterPath
            );

        Sprite borderSprite =
            LoadSpriteFlexible(
                "gambar/border"
            );

        CreateProfileAvatar(
            header.transform,
            characterSprite,
            borderSprite
        );


        // ============================================================
        // PLAYER INFO
        // ============================================================

        GameObject info =
            new GameObject("PlayerInfo");

        info.transform.SetParent(
            header.transform,
            false
        );

        RectTransform rtInfo =
            info.AddComponent<RectTransform>();

        rtInfo.anchorMin =
            rtInfo.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtInfo.sizeDelta =
            new Vector2(
                500f,
                170f
            );

        rtInfo.anchoredPosition =
            new Vector2(
                -50f,
                0f
            );


        // ============================================================
        // NAME
        // ============================================================

        nameTxt =
            UIBuilder.CreateTextSized(
                info.transform,
                "MEMUAT...",
                52,
                Color.white,
                TextAlignmentOptions.Left,
                new Vector2(
                    480f,
                    64f
                ),
                new Vector2(
                    0f,
                    42f
                )
            ).GetComponent<TextMeshProUGUI>();

        nameTxt.fontStyle =
            FontStyles.Bold;

        nameTxt.characterSpacing =
            1.2f;

        nameTxt.raycastTarget =
            false;


        // ============================================================
        // TITLE
        // ============================================================

        titleTxt =
            UIBuilder.CreateTextSized(
                info.transform,
                "Wisatawan",
                29,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Left,
                new Vector2(
                    480f,
                    40f
                ),
                new Vector2(
                    0f,
                    3f
                )
            ).GetComponent<TextMeshProUGUI>();

        titleTxt.fontStyle =
            FontStyles.Bold;

        titleTxt.characterSpacing =
            0.8f;

        titleTxt.raycastTarget =
            false;


        // ============================================================
        // LEVEL
        // ============================================================

        levelTxt =
            UIBuilder.CreateTextSized(
                info.transform,
                "Level 1",
                22,
                TOBATheme.TextLight,
                TextAlignmentOptions.Left,
                new Vector2(
                    100f,
                    30f
                ),
                new Vector2(
                    0f,
                    -50f
                )
            ).GetComponent<TextMeshProUGUI>();

        levelTxt.fontStyle =
            FontStyles.Bold;

        levelTxt.characterSpacing =
            0.6f;

        levelTxt.raycastTarget =
            false;


        // ============================================================
        // XP BAR
        // ============================================================

        GameObject xpBackground =
            UIBuilder.CreatePanelSized(
                info.transform,
                "XPBackground",
                new Color(
                    0f,
                    0f,
                    0f,
                    0.55f
                ),
                new Vector2(
                    310f,
                    18f
                ),
                new Vector2(
                    85f,
                    -50f
                )
            );

        xpBackground
            .GetComponent<Image>()
            .raycastTarget = false;

        Outline xpOutline =
            xpBackground.AddComponent<Outline>();

        xpOutline.effectColor =
            new Color(
                TOBATheme.BatakGold.r,
                TOBATheme.BatakGold.g,
                TOBATheme.BatakGold.b,
                0.5f
            );

        xpOutline.effectDistance =
            new Vector2(
                1f,
                1f
            );

        xpBarFill =
            UIBuilder.CreatePanel(
                xpBackground.transform,
                "Fill",
                TOBATheme.BatakGold,
                Vector2.zero,
                new Vector2(
                    0.05f,
                    1f
                )
            );

        xpBarFill
            .GetComponent<Image>()
            .raycastTarget = false;


        // ============================================================
        // STAR BOX
        // ============================================================

        GameObject starBox =
            UIBuilder.CreateSpritePanel(
                header.transform,
                "StarBox",
                headerSprite
                ?? LoadSpriteFlexible("panel_wood"),
                new Vector2(
                    170f,
                    92f
                ),
                new Vector2(
                    330f,
                    0f
                )
            );

        Image starBoxImg =
            starBox.GetComponent<Image>();

        starBoxImg.color =
            Color.white;

        starBoxImg.raycastTarget =
            false;

        Outline starOutline =
            starBox.AddComponent<Outline>();

        starOutline.effectColor =
            TOBATheme.BatakGold;

        starOutline.effectDistance =
            new Vector2(
                2f,
                2f
            );


        UIBuilder.CreateTextSized(
            starBox.transform,
            "★",
            42,
            TOBATheme.StarActive,
            TextAlignmentOptions.Center,
            new Vector2(
                55f,
                55f
            ),
            new Vector2(
                -45f,
                0f
            )
        );


        starTxt =
            UIBuilder.CreateTextSized(
                starBox.transform,
                "0",
                38,
                Color.white,
                TextAlignmentOptions.Center,
                new Vector2(
                    65f,
                    55f
                ),
                new Vector2(
                    32f,
                    0f
                )
            ).GetComponent<TextMeshProUGUI>();

        starTxt.fontStyle =
            FontStyles.Bold;

        starTxt.characterSpacing =
            0.5f;

        starTxt.raycastTarget =
            false;


        // ============================================================
        // SETTINGS
        // ============================================================

        CreateSettingsButton(
            header.transform
        );
    }


    // ============================================================
    // PROFILE AVATAR + BORDER
    // ============================================================

    void CreateProfileAvatar(
        Transform parent,
        Sprite characterSprite,
        Sprite borderSprite)
    {
        GameObject root =
            new GameObject(
                "ProfileAvatar"
            );

        root.transform.SetParent(
            parent,
            false
        );

        RectTransform rtRoot =
            root.AddComponent<RectTransform>();

        rtRoot.anchorMin =
            rtRoot.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        rtRoot.sizeDelta =
            new Vector2(
                148f,
                148f
            );

        rtRoot.anchoredPosition =
            new Vector2(
                92f,
                0f
            );


        // ============================================================
        // BORDER
        // ============================================================

        if (borderSprite != null)
        {
            GameObject border =
                UIBuilder.CreateImage(
                    root.transform,
                    "Border",
                    borderSprite,
                    new Vector2(
                        148f,
                        148f
                    ),
                    Vector2.zero
                );

            Image borderImg =
                border.GetComponent<Image>();

            borderImg.preserveAspect =
                true;

            borderImg.raycastTarget =
                false;
        }
        else
        {
            Image ring =
                root.AddComponent<Image>();

            ring.sprite =
                GetCircleSprite();

            ring.color =
                TOBATheme.BatakGold;

            ring.raycastTarget =
                false;
        }


        // ============================================================
        // MASK
        // ============================================================

        GameObject maskObject =
            new GameObject(
                "AvatarMask"
            );

        maskObject.transform.SetParent(
            root.transform,
            false
        );

        RectTransform rtMask =
            maskObject.AddComponent<RectTransform>();

        rtMask.anchorMin =
            rtMask.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtMask.sizeDelta =
            new Vector2(
                108f,
                108f
            );

        rtMask.anchoredPosition =
            Vector2.zero;


        Image maskImage =
            maskObject.AddComponent<Image>();

        maskImage.sprite =
            GetCircleSprite();

        maskImage.color =
            Color.white;

        maskImage.raycastTarget =
            false;


        Mask mask =
            maskObject.AddComponent<Mask>();

        mask.showMaskGraphic =
            false;


        // ============================================================
        // CHARACTER
        // ============================================================

        if (characterSprite != null)
        {
            GameObject avatar =
                new GameObject(
                    "Character"
                );

            avatar.transform.SetParent(
                maskObject.transform,
                false
            );

            RectTransform rtAvatar =
                avatar.AddComponent<RectTransform>();

            rtAvatar.anchorMin =
                Vector2.zero;

            rtAvatar.anchorMax =
                Vector2.one;

            rtAvatar.offsetMin =
                Vector2.zero;

            rtAvatar.offsetMax =
                Vector2.zero;


            Image avatarImage =
                avatar.AddComponent<Image>();

            avatarImage.sprite =
                characterSprite;

            avatarImage.preserveAspect =
                true;

            avatarImage.color =
                Color.white;

            avatarImage.raycastTarget =
                false;
        }
    }


    // ============================================================
    // CIRCLE SPRITE
    // ============================================================

    static Sprite GetCircleSprite()
    {
        if (_circleSprite != null)
            return _circleSprite;

        const int size = 128;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        Color[] pixels =
            new Color[
                size * size
            ];

        float center =
            (size - 1) / 2f;

        float radius =
            size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx =
                    x - center;

                float dy =
                    y - center;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                pixels[
                    y * size + x
                ] =
                    distance <= radius
                        ? Color.white
                        : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        texture.wrapMode =
            TextureWrapMode.Clamp;

        _circleSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        return _circleSprite;
    }


    // ============================================================
    // SETTINGS
    // ============================================================

    void CreateSettingsButton(
        Transform parent)
    {
        GameObject button =
            new GameObject(
                "BtnSetting"
            );

        button.transform.SetParent(
            parent,
            false
        );

        RectTransform rt =
            button.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                1f,
                1f
            );

        rt.sizeDelta =
            new Vector2(
                124f,
                124f
            );

        rt.anchoredPosition =
            new Vector2(
                -58f,
                -58f
            );


        Image hit =
            button.AddComponent<Image>();

        hit.sprite =
            UIBuilder.GetDefaultSprite();

        hit.color =
            Color.clear;

        hit.raycastTarget =
            true;


        Sprite settings =
            LoadSpriteFlexible(
                "gambar/icon_settings"
            );

        if (settings != null)
        {
            GameObject icon =
                UIBuilder.CreateImage(
                    button.transform,
                    "Icon",
                    settings,
                    new Vector2(
                        112f,
                        112f
                    ),
                    Vector2.zero
                );

            Image iconImg =
                icon.GetComponent<Image>();

            iconImg.preserveAspect =
                true;

            iconImg.raycastTarget =
                false;

            Outline outline =
                icon.AddComponent<Outline>();

            outline.effectColor =
                new Color(
                    TOBATheme.BatakGold.r,
                    TOBATheme.BatakGold.g,
                    TOBATheme.BatakGold.b,
                    0.35f
                );

            outline.effectDistance =
                new Vector2(
                    2f,
                    2f
                );
        }


        Button btn =
            button.AddComponent<Button>();

        btn.targetGraphic =
            hit;


        ColorBlock colors =
            btn.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color(
                1f,
                1f,
                0.82f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.8f,
                0.8f,
                0.6f,
                1f
            );

        btn.colors =
            colors;


        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX(
                "sfx_click"
            );

            ScreenManager.Instance.ChangeScreen(
                new SettingScreen(container)
            );
        });
    }


    // ============================================================
    // MAIN MENU GRID
    // ============================================================

    void BuildMenuGrid()
    {
        var menus =
            new (
                string name,
                string iconPath
            )[]
            {
                (
                    "Jelajah Budaya",
                    "gambar/icon_jelajah"
                ),
                (
                    "Trivia Budaya",
                    "gambar/icon_trivia"
                ),
                (
                    "Battle Quiz",
                    "gambar/icon_battle"
                ),
                (
                    "Galeri Budaya",
                    "gambar/icon_gallery"
                ),
                (
                    "Leaderboard",
                    "gambar/icon_leaderboard"
                ),
                (
                    "Profil",
                    "gambar/icon_profil"
                )
            };

        float[] xPositions =
        {
            -215f,
             215f
        };

        float[] yPositions =
        {
             355f,
               5f,
            -345f
        };

        for (int i = 0; i < menus.Length; i++)
        {
            CreateMenuIcon(
                menus[i].name,
                menus[i].iconPath,
                xPositions[i % 2],
                yPositions[i / 2],
                i
            );
        }
    }


    // ============================================================
    // MENU ICON
    // ============================================================

    void CreateMenuIcon(
        string menuName,
        string iconPath,
        float x,
        float y,
        int index)
    {
        Sprite sprite =
            LoadSpriteFlexible(
                iconPath
            );

        if (sprite == null)
        {
            Debug.LogWarning(
                "[MainMenu] Icon tidak ditemukan: " +
                iconPath
            );

            return;
        }


        GameObject root =
            new GameObject(
                "MenuIcon_" +
                menuName
            );

        root.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rt =
            root.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rt.sizeDelta =
            new Vector2(
                360f,
                360f
            );

        rt.anchoredPosition =
            new Vector2(
                x,
                y
            );

        CanvasGroup cg =
            root.AddComponent<CanvasGroup>();

        cg.alpha =
            0f;


        // Shadow
        GameObject shadow =
            UIBuilder.CreateImage(
                root.transform,
                "Shadow",
                sprite,
                new Vector2(
                    338f,
                    338f
                ),
                new Vector2(
                    9f,
                    -11f
                ),
                new Color(
                    0f,
                    0f,
                    0f,
                    0.38f
                )
            );

        shadow.GetComponent<Image>()
            .raycastTarget = false;


        // Icon
        GameObject icon =
            UIBuilder.CreateImage(
                root.transform,
                "Icon",
                sprite,
                new Vector2(
                    325f,
                    325f
                ),
                Vector2.zero
            );

        Image iconImage =
            icon.GetComponent<Image>();

        iconImage.preserveAspect =
            true;

        iconImage.raycastTarget =
            false;


        // Hitbox
        GameObject hit =
            new GameObject(
                "Hitbox"
            );

        hit.transform.SetParent(
            root.transform,
            false
        );

        RectTransform rtHit =
            hit.AddComponent<RectTransform>();

        rtHit.anchorMin =
            Vector2.zero;

        rtHit.anchorMax =
            Vector2.one;

        rtHit.offsetMin =
            Vector2.zero;

        rtHit.offsetMax =
            Vector2.zero;


        Image hitImage =
            hit.AddComponent<Image>();

        hitImage.sprite =
            UIBuilder.GetDefaultSprite();

        hitImage.color =
            Color.clear;

        hitImage.raycastTarget =
            true;


        Button btn =
            hit.AddComponent<Button>();

        btn.targetGraphic =
            hitImage;


        ColorBlock colors =
            btn.colors;

        colors.normalColor =
            Color.clear;

        colors.highlightedColor =
            new Color(
                1f,
                1f,
                1f,
                0.05f
            );

        colors.pressedColor =
            new Color(
                0f,
                0f,
                0f,
                0.12f
            );

        btn.colors =
            colors;


        string capturedMenu =
            menuName;

        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX(
                "sfx_click"
            );

            ScreenManager.Instance.StartCoroutine(
                PressAndNavigate(
                    root.transform,
                    capturedMenu
                )
            );
        });


        ScreenManager.Instance.StartCoroutine(
            AnimateMenuIn(
                root,
                index
            )
        );
    }


    // ============================================================
    // CHARACTER DECORATION
    // ============================================================

    void CreateCharacterDecoration()
    {
        bool isButet =
            selectedCharacterID == 2;

        string path =
            isButet
                ? "gambar/butet_berpikir"
                : "gambar/ucok_berpikir";

        Sprite character =
            LoadSpriteFlexible(path);

        if (character == null)
            return;


        GameObject obj =
            UIBuilder.CreateImage(
                screenObj.transform,
                "CharacterDecoration",
                character,
                new Vector2(
                    180f,
                    275f
                ),
                new Vector2(
                    -430f,
                    -575f
                )
            );


        Image image =
            obj.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }


        obj.transform.SetAsLastSibling();


        ScreenManager.Instance.StartCoroutine(
            FloatCharacter(
                obj.transform
            )
        );
    }


    // ============================================================
    // UPDATE CHARACTER VISUALS
    // ============================================================

    void UpdateCharacterVisuals()
    {
        bool isButet =
            selectedCharacterID == 2;

        string characterSpritePath =
            isButet
                ? "char_butet"
                : "char_ucok";

        string decorationSpritePath =
            isButet
                ? "gambar/butet_berpikir"
                : "gambar/ucok_berpikir";

        Sprite characterSprite =
            LoadSpriteFlexible(
                characterSpritePath
            );

        Sprite decorationSprite =
            LoadSpriteFlexible(
                decorationSpritePath
            );


        // ============================================================
        // 1. HEADER AVATAR
        // ============================================================

        Transform avatar =
            screenObj.transform.Find(
                "Header/ProfileAvatar/AvatarMask/Character"
            );

        if (avatar != null &&
            characterSprite != null)
        {
            Image img =
                avatar.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    characterSprite;

                img.preserveAspect =
                    true;
            }
        }


        // ============================================================
        // 2. CHARACTER DECORATION
        // ============================================================

        Transform decoration =
            screenObj.transform.Find(
                "CharacterDecoration"
            );

        if (decoration != null &&
            decorationSprite != null)
        {
            Image img =
                decoration.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    decorationSprite;

                img.preserveAspect =
                    true;
            }
        }


        // ============================================================
        // 3. PROFILE TAB FOOTER
        // ============================================================

        if (footerGO != null)
        {
            FooterNavbar.SetProfileCharacter(
                footerGO,
                selectedCharacterID
            );
        }


        // ============================================================
        // NOTE:
        // MenuIcon_Profil sengaja TIDAK diubah.
        // Tetap menggunakan gambar/icon_profil.png
        // ============================================================
    }


    // ============================================================
    // MENU ANIMATION
    // ============================================================

    IEnumerator AnimateMenuIn(
        GameObject root,
        int index)
    {
        if (root == null)
            yield break;

        yield return
            new WaitForSecondsRealtime(
                index * 0.05f
            );

        CanvasGroup cg =
            root.GetComponent<CanvasGroup>();

        RectTransform rt =
            root.GetComponent<RectTransform>();

        if (rt == null)
            yield break;

        Vector2 target =
            rt.anchoredPosition;

        Vector2 start =
            target +
            new Vector2(
                0f,
                -75f
            );

        float elapsed =
            0f;

        float duration =
            0.42f;

        root.transform.localScale =
            Vector3.one * 0.78f;

        while (
            elapsed < duration &&
            root != null)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    elapsed / duration
                );

            if (cg != null)
                cg.alpha =
                    t;

            if (rt != null)
            {
                rt.anchoredPosition =
                    Vector2.Lerp(
                        start,
                        target,
                        t
                    );
            }

            root.transform.localScale =
                Vector3.one *
                Mathf.Lerp(
                    0.78f,
                    1f,
                    t
                );

            yield return null;
        }

        if (cg != null)
            cg.alpha = 1f;

        if (rt != null)
            rt.anchoredPosition =
                target;

        root.transform.localScale =
            Vector3.one;


        ScreenManager.Instance.StartCoroutine(
            FloatMenu(
                root,
                index
            )
        );
    }


    // ============================================================
    // FLOAT MENU
    // ============================================================

    IEnumerator FloatMenu(
        GameObject root,
        int index)
    {
        if (root == null)
            yield break;

        RectTransform rt =
            root.GetComponent<RectTransform>();

        if (rt == null)
            yield break;

        Vector2 origin =
            rt.anchoredPosition;

        float phase =
            index * 0.75f;

        while (
            root != null &&
            rt != null)
        {
            phase +=
                Time.deltaTime *
                (
                    0.70f +
                    index * 0.04f
                );

            rt.anchoredPosition =
                new Vector2(
                    origin.x,
                    origin.y +
                    Mathf.Sin(phase) *
                    7f
                );

            rt.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(
                        phase * 0.85f
                    ) * 1.1f
                );

            yield return null;
        }
    }


    // ============================================================
    // FLOAT CHARACTER
    // ============================================================

    IEnumerator FloatCharacter(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float phase =
            0f;

        while (target != null)
        {
            phase +=
                Time.deltaTime *
                0.65f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(phase) *
                    4f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(
                        phase * 0.8f
                    ) * 0.8f
                );

            yield return null;
        }
    }


    // ============================================================
    // PRESS MENU
    // ============================================================

    IEnumerator PressAndNavigate(
        Transform menuTransform,
        string menu)
    {
        if (menuTransform != null)
        {
            float t = 0f;

            while (t < 0.08f)
            {
                t +=
                    Time.deltaTime;

                menuTransform.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        1f,
                        0.90f,
                        t / 0.08f
                    );

                yield return null;
            }

            t = 0f;

            while (t < 0.06f)
            {
                t +=
                    Time.deltaTime;

                menuTransform.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        0.90f,
                        1.04f,
                        t / 0.06f
                    );

                yield return null;
            }

            menuTransform.localScale =
                Vector3.one;
        }

        NavigateTo(menu);
    }


    // ============================================================
    // NAVIGATION
    // ============================================================

    void NavigateTo(
        string menu)
    {
        switch (menu)
        {
            case "Jelajah Budaya":
                ScreenManager.Instance.ChangeScreen(
                    new MapScreen(container)
                );
                break;

            case "Trivia Budaya":
                ScreenManager.Instance.ChangeScreen(
                    new TriviaMenuScreen(container)
                );
                break;

            case "Battle Quiz":
                ScreenManager.Instance.ChangeScreen(
                    new BattleMenuScreen(container)
                );
                break;

            case "Galeri Budaya":
                ScreenManager.Instance.ChangeScreen(
                    new GalleryScreen(container)
                );
                break;

            case "Leaderboard":
                ScreenManager.Instance.ChangeScreen(
                    new LeaderboardScreen(container)
                );
                break;

            case "Profil":
                ScreenManager.Instance.ChangeScreen(
                    new ProfileScreen(container)
                );
                break;
        }
    }


    // ============================================================
    // FETCH PROFILE
    // ============================================================

    void FetchProfile()
    {
        int playerID =
            PlayerPrefs.GetInt(
                "PlayerID"
            );

        if (playerID == 0)
            return;


        APIManager.Instance.GetRequest(
            "/profile/" + playerID,

            (response) =>
            {
                if (
                    screenObj == null ||
                    nameTxt == null
                )
                    return;


                ProfileResponse res =
                    JsonUtility.FromJson<ProfileResponse>(
                        response
                    );

                if (res == null)
                    return;


                // ====================================================
                // CHARACTER DARI BACKEND
                // ====================================================

                selectedCharacterID =
                    res.character_id == 2
                        ? 2
                        : 1;

                PlayerPrefs.SetInt(
                    "SelectedCharacterID",
                    selectedCharacterID
                );

                PlayerPrefs.Save();


                // Update visual karakter:
                // Header + dekorasi + tab Profil
                // TIDAK mengubah menu Profil tengah.
                UpdateCharacterVisuals();


                // ====================================================
                // PROFILE DATA
                // ====================================================

                nameTxt.text =
                    res.name.ToUpper();

                titleTxt.text =
                    res.title_name;

                levelTxt.text =
                    "Level " +
                    res.level;

                starTxt.text =
                    res.stars.ToString();


                float xpRatio =
                    res.xp_for_next > 0
                        ? Mathf.Clamp01(
                            (float)res.xp_progress /
                            res.xp_for_next
                        )
                        : 0f;


                if (xpBarFill != null)
                {
                    RectTransform fill =
                        xpBarFill.GetComponent<RectTransform>();

                    if (fill != null)
                    {
                        fill.anchorMin =
                            Vector2.zero;

                        fill.anchorMax =
                            new Vector2(
                                xpRatio,
                                1f
                            );

                        fill.offsetMin =
                            fill.offsetMax =
                            Vector2.zero;
                    }
                }


                PlayerPrefs.SetInt(
                    "PlayerLevel",
                    res.level
                );
            },

            (error) =>
            {
                if (nameTxt != null)
                    nameTxt.text =
                        "OFFLINE";
            }
        );
    }
}