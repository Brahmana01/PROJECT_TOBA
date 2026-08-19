using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterSelectScreen : BaseScreen
{
    private static Sprite generatedCircleSprite;
    private static Sprite generatedPatternSprite;
    private static Sprite generatedSweepSprite;

    public CharacterSelectScreen(Transform parent)
        : base(parent, "CharSelect")
    {
    }


    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // ============================================================
        // BACKGROUND
        // ============================================================

        CreatePatternBackground();


        // ============================================================
        // HEADER TOBA
        // ============================================================

        Sprite headerSprite =
            LoadSprite(
                "CharacterSelectScreen/character_select_header"
            );

        if (headerSprite != null)
        {
            GameObject header =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "CharacterSelectHeader",
                    headerSprite,
                    new Vector2(
                        1040f,
                        345f
                    ),
                    new Vector2(
                        0f,
                        605f
                    )
                );

            Image headerImg =
                header.GetComponent<Image>();

            headerImg.preserveAspect =
                true;

            headerImg.raycastTarget =
                false;

            // Light Sweep / Shimmer
            ScreenManager.Instance.StartCoroutine(
                HeaderShimmer(
                    header.transform
                )
            );
        }


        // ============================================================
        // TITLE ORNAMENT
        // ============================================================

        Sprite ornamentSprite =
            LoadSprite(
                "CharacterSelectScreen/character_select_title_ornament"
            );

        if (ornamentSprite != null)
        {
            // LEFT
            GameObject leftOrnament =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "TitleOrnamentLeft",
                    ornamentSprite,
                    new Vector2(
                        135f,
                        85f
                    ),
                    new Vector2(
                        -390f,
                        345f
                    )
                );

            leftOrnament
                .GetComponent<Image>()
                .raycastTarget = false;


            // RIGHT
            GameObject rightOrnament =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "TitleOrnamentRight",
                    ornamentSprite,
                    new Vector2(
                        135f,
                        85f
                    ),
                    new Vector2(
                        390f,
                        345f
                    )
                );

            RectTransform rtRight =
                rightOrnament.GetComponent<RectTransform>();

            rtRight.localScale =
                new Vector3(
                    -1f,
                    1f,
                    1f
                );

            rightOrnament
                .GetComponent<Image>()
                .raycastTarget = false;
        }


        // ============================================================
        // TITLE
        // ============================================================

        GameObject titleObj =
            UIBuilder.CreateText(
                screenObj.transform,
                "PILIH KARAKTER",
                76,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center
            );

        RectTransform rtTitle =
            titleObj.GetComponent<RectTransform>();

        rtTitle.anchorMin =
            rtTitle.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtTitle.sizeDelta =
            new Vector2(
                760f,
                100f
            );

        rtTitle.anchoredPosition =
            new Vector2(
                0f,
                350f
            );

        TextMeshProUGUI titleTmp =
            titleObj.GetComponent<TextMeshProUGUI>();

        titleTmp.fontStyle =
            FontStyles.Bold;

        titleTmp.characterSpacing =
            2f;

        titleTmp.raycastTarget =
            false;


        // ============================================================
        // CHARACTER CARDS
        // ============================================================

        CreateCard(
            "UCOK",
            1,
            "char_ucok",
            -245f
        );

        CreateCard(
            "BUTET",
            2,
            "char_butet",
            245f
        );


        // ============================================================
        // BOTTOM LEAVES
        // ============================================================

        CreateBottomLeaves();


        // ============================================================
        // SCREEN ENTRANCE
        // ============================================================

        ScreenManager.Instance.StartCoroutine(
            AnimateScreenEntrance()
        );
    }


    // ============================================================
    // SPRITE LOADER
    // ============================================================

    static Sprite LoadSprite(string path)
    {
        Sprite sprite =
            Resources.Load<Sprite>(path);

        if (sprite != null)
            return sprite;

        Texture2D texture =
            Resources.Load<Texture2D>(path);

        if (texture != null)
        {
            return Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    texture.width,
                    texture.height
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );
        }

        Debug.LogWarning(
            "[CharacterSelect] Sprite tidak ditemukan: " +
            path
        );

        return null;
    }


    // ============================================================
    // BACKGROUND CORAK
    // ============================================================

    void CreatePatternBackground()
    {
        Sprite pattern =
            GetGeneratedPatternSprite();

        if (pattern == null)
            return;

        GameObject patternObj =
            UIBuilder.CreateImage(
                screenObj.transform,
                "BackgroundPattern",
                pattern,
                new Vector2(
                    1080f,
                    1920f
                ),
                Vector2.zero
            );

        Image img =
            patternObj.GetComponent<Image>();

        img.color =
            new Color(
                0.15f,
                0.34f,
                0.17f,
                0.16f
            );

        img.raycastTarget =
            false;

        img.type =
            Image.Type.Tiled;
    }


    // ============================================================
    // GENERATED GREEN PATTERN
    // ============================================================

    static Sprite GetGeneratedPatternSprite()
    {
        if (generatedPatternSprite != null)
            return generatedPatternSprite;

        const int width = 256;
        const int height = 256;

        Texture2D texture =
            new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );

        texture.wrapMode =
            TextureWrapMode.Repeat;

        Color clear =
            new Color(
                1f,
                1f,
                1f,
                0f
            );

        Color[] pixels =
            new Color[
                width * height
            ];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;


        // ------------------------------------------------------------
        // Motif kecil berbentuk daun / ornamen
        // ------------------------------------------------------------

        DrawPatternLeaf(
            pixels,
            width,
            height,
            64,
            64
        );

        DrawPatternLeaf(
            pixels,
            width,
            height,
            192,
            192
        );

        DrawPatternLeaf(
            pixels,
            width,
            height,
            192,
            64
        );

        DrawPatternLeaf(
            pixels,
            width,
            height,
            64,
            192
        );


        // Garis diagonal sangat halus
        Color lineColor =
            new Color(
                0.8f,
                0.9f,
                0.5f,
                0.025f
            );

        for (
            int y = 0;
            y < height;
            y++
        )
        {
            int x =
                (y * 3) % width;

            if (
                x >= 0 &&
                x < width)
            {
                pixels[
                    y * width + x
                ] =
                    lineColor;
            }
        }


        texture.SetPixels(
            pixels
        );

        texture.Apply();

        generatedPatternSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    width,
                    height
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        return generatedPatternSprite;
    }


    static void DrawPatternLeaf(
        Color[] pixels,
        int width,
        int height,
        int cx,
        int cy)
    {
        Color motifColor =
            new Color(
                0.8f,
                0.9f,
                0.4f,
                0.035f
            );


        for (
            int y = -35;
            y <= 35;
            y++)
        {
            for (
                int x = -35;
                x <= 35;
                x++)
            {
                float nx =
                    x / 35f;

                float ny =
                    y / 35f;

                float ellipse =
                    nx * nx +
                    ny * ny;

                if (ellipse <= 1f)
                {
                    float alpha =
                        (1f - ellipse) *
                        motifColor.a;

                    Color c =
                        new Color(
                            motifColor.r,
                            motifColor.g,
                            motifColor.b,
                            alpha
                        );

                    int px =
                        cx + x;

                    int py =
                        cy + y;

                    if (
                        px >= 0 &&
                        px < width &&
                        py >= 0 &&
                        py < height)
                    {
                        pixels[
                            py * width + px
                        ] = c;
                    }
                }
            }
        }
    }


    // ============================================================
    // CHARACTER CARD
    // ============================================================

    void CreateCard(
        string characterName,
        int id,
        string characterPath,
        float x)
    {
        // ------------------------------------------------------------
        // ROOT
        // ------------------------------------------------------------

        GameObject root =
            new GameObject(
                "CharacterCard_" +
                characterName
            );

        root.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rtRoot =
            root.AddComponent<RectTransform>();

        rtRoot.anchorMin =
            rtRoot.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtRoot.sizeDelta =
            new Vector2(
                455f,
                690f
            );

        rtRoot.anchoredPosition =
            new Vector2(
                x,
                -40f
            );


        // ------------------------------------------------------------
        // CHARACTER
        // ------------------------------------------------------------

        Sprite characterSprite =
            LoadSprite(
                characterPath
            );

        if (characterSprite != null)
        {
            GameObject character =
                UIBuilder.CreateImage(
                    root.transform,
                    "Character",
                    characterSprite,
                    new Vector2(
                        250f,
                        375f
                    ),
                    new Vector2(
                        0f,
                        25f
                    )
                );

            Image characterImg =
                character.GetComponent<Image>();

            characterImg.preserveAspect =
                true;

            characterImg.raycastTarget =
                false;
        }


        // ------------------------------------------------------------
        // FRAME
        // ------------------------------------------------------------

        Sprite frameSprite =
            LoadSprite(
                "CharacterSelectScreen/character_select_frame"
            );

        if (frameSprite != null)
        {
            GameObject frame =
                UIBuilder.CreateImage(
                    root.transform,
                    "Frame",
                    frameSprite,
                    new Vector2(
                        455f,
                        690f
                    ),
                    Vector2.zero
                );

            Image frameImg =
                frame.GetComponent<Image>();

            frameImg.preserveAspect =
                true;

            frameImg.raycastTarget =
                false;
        }


        // ------------------------------------------------------------
        // CHARACTER NAME
        // ------------------------------------------------------------

        GameObject nameObj =
            UIBuilder.CreateText(
                screenObj.transform,
                characterName,
                44,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center
            );

        RectTransform rtName =
            nameObj.GetComponent<RectTransform>();

        rtName.anchorMin =
            rtName.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtName.sizeDelta =
            new Vector2(
                300f,
                70f
            );

        rtName.anchoredPosition =
            new Vector2(
                x,
                -390f
            );

        TextMeshProUGUI nameTmp =
            nameObj.GetComponent<TextMeshProUGUI>();

        nameTmp.fontStyle =
            FontStyles.Bold;

        nameTmp.characterSpacing =
            1.5f;

        nameTmp.raycastTarget =
            false;


        // ------------------------------------------------------------
        // BUTTON
        // ------------------------------------------------------------

        CreateSelectButton(
            screenObj.transform,
            id,
            x
        );


        // ------------------------------------------------------------
        // FLOATING CHARACTER CARD
        // ------------------------------------------------------------

        ScreenManager.Instance.StartCoroutine(
            FloatCharacterCard(
                root.transform,
                x > 0f
                    ? 0.8f
                    : 0f
            )
        );
    }


    // ============================================================
    // SELECT BUTTON
    // ============================================================

    void CreateSelectButton(
        Transform parent,
        int id,
        float x)
    {
        Sprite btnSprite =
            LoadSprite(
                "btn_gold"
            );

        GameObject buttonObj;

        if (btnSprite != null)
        {
            buttonObj =
                UIBuilder.CreateImage(
                    parent,
                    "Btn_" + id,
                    btnSprite,
                    new Vector2(
                        350f,
                        125f
                    ),
                    new Vector2(
                        x,
                        -500f
                    )
                );

            buttonObj
                .GetComponent<Image>()
                .raycastTarget = true;
        }
        else
        {
            buttonObj =
                UIBuilder.CreateButton(
                    parent,
                    "Btn_" + id,
                    "PILIH",
                    new Vector2(
                        300f,
                        100f
                    ),
                    TOBATheme.BatakGold,
                    null
                );

            buttonObj
                .GetComponent<RectTransform>()
                .anchoredPosition =
                new Vector2(
                    x,
                    -500f
                );
        }


        // ============================================================
        // BUTTON TEXT
        // ============================================================

        GameObject labelObj =
            UIBuilder.CreateText(
                buttonObj.transform,
                "PILIH",
                34,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center
            );

        RectTransform rtLabel =
            labelObj.GetComponent<RectTransform>();

        rtLabel.anchorMin =
            Vector2.zero;

        rtLabel.anchorMax =
            Vector2.one;

        rtLabel.offsetMin =
            Vector2.zero;

        rtLabel.offsetMax =
            Vector2.zero;

        TextMeshProUGUI labelTmp =
            labelObj.GetComponent<TextMeshProUGUI>();

        labelTmp.fontStyle =
            FontStyles.Bold;

        labelTmp.characterSpacing =
            1f;

        labelTmp.raycastTarget =
            false;


        // ============================================================
        // BUTTON
        // ============================================================

        Image buttonImage =
            buttonObj.GetComponent<Image>();

        Button button =
            buttonObj.GetComponent<Button>();

        if (button == null)
        {
            button =
                buttonObj.AddComponent<Button>();
        }

        button.targetGraphic =
            buttonImage;

        button.transition =
            Selectable.Transition.None;


        // ============================================================
        // LOGIC ASLI - TIDAK DIUBAH
        // ============================================================

        button.onClick.AddListener(
            () =>
            {
                SoundManager.Instance.PlaySFX(
                    "sfx_click"
                );

                PlayerPrefs.SetInt(
                    "Temp_CharID",
                    id
                );

                ScreenManager.Instance.ChangeScreen(
                    new RegisterNameScreen(
                        container
                    )
                );
            }
        );


        // ============================================================
        // BUTTON IDLE
        // ============================================================

        ScreenManager.Instance.StartCoroutine(
            AnimateButtonIdle(
                buttonObj.transform
            )
        );
    }


    // ============================================================
    // BOTTOM LEAVES
    // ============================================================

    void CreateBottomLeaves()
    {
        // ------------------------------------------------------------
        // LEFT
        // ------------------------------------------------------------

        Sprite leftLeaves =
            LoadSprite(
                "CharacterSelectScreen/character_select_left_leaves"
            );

        if (leftLeaves != null)
        {
            GameObject left =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "BottomLeavesLeft",
                    leftLeaves,
                    new Vector2(
                        500f,
                        430f
                    ),
                    new Vector2(
                        -360f,
                        -700f
                    )
                );

            Image img =
                left.GetComponent<Image>();

            img.preserveAspect =
                true;

            img.raycastTarget =
                false;

            ScreenManager.Instance.StartCoroutine(
                SwayLeaves(
                    left.transform,
                    0f
                )
            );
        }


        // ------------------------------------------------------------
        // RIGHT
        // ------------------------------------------------------------

        Sprite rightLeaves =
            LoadSprite(
                "CharacterSelectScreen/character_select_right_leaves"
            );

        if (rightLeaves != null)
        {
            GameObject right =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "BottomLeavesRight",
                    rightLeaves,
                    new Vector2(
                        500f,
                        430f
                    ),
                    new Vector2(
                        360f,
                        -700f
                    )
                );

            Image img =
                right.GetComponent<Image>();

            img.preserveAspect =
                true;

            img.raycastTarget =
                false;

            ScreenManager.Instance.StartCoroutine(
                SwayLeaves(
                    right.transform,
                    1.2f
                )
            );
        }
    }


    // ============================================================
    // LEAF SWAY
    // ============================================================

    IEnumerator SwayLeaves(
        Transform target,
        float phaseOffset)
    {
        if (target == null)
            yield break;

        Vector3 originalScale =
            target.localScale;

        Vector3 originalPosition =
            target.localPosition;

        float t =
            phaseOffset;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                0.8f;

            float sway =
                Mathf.Sin(t) *
                2.2f;

            float vertical =
                Mathf.Sin(
                    t * 0.65f
                ) *
                1.2f;

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    sway
                );

            target.localPosition =
                new Vector3(
                    originalPosition.x,
                    originalPosition.y +
                    vertical,
                    originalPosition.z
                );

            target.localScale =
                originalScale *
                (
                    1f +
                    Mathf.Sin(
                        t * 0.55f
                    ) *
                    0.015f
                );

            yield return null;
        }
    }


    // ============================================================
    // CHARACTER FLOAT
    // ============================================================

    IEnumerator FloatCharacterCard(
        Transform target,
        float phaseOffset)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float t =
            phaseOffset;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                0.75f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) *
                    3f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(
                        t * 0.8f
                    ) *
                    0.4f
                );

            yield return null;
        }
    }


    // ============================================================
    // BUTTON IDLE
    // ============================================================

    IEnumerator AnimateButtonIdle(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localScale;

        float t =
            0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                1.2f;

            float scale =
                1f +
                Mathf.Sin(t) *
                0.025f;

            target.localScale =
                origin *
                scale;

            yield return null;
        }
    }


    // ============================================================
    // HEADER SHIMMER / LIGHT SWEEP
    // ============================================================

    IEnumerator HeaderShimmer(
        Transform header)
    {
        if (header == null)
            yield break;

        // Mask supaya sweep hanya terlihat di area logo header
        Image headerImage =
            header.GetComponent<Image>();

        if (headerImage == null)
            yield break;

        Mask mask =
            header.gameObject
                .GetComponent<Mask>();

        if (mask == null)
        {
            mask =
                header.gameObject
                    .AddComponent<Mask>();

            mask.showMaskGraphic =
                true;
        }


        Sprite sweepSprite =
            GetSweepSprite();

        if (sweepSprite == null)
            yield break;


        GameObject sweep =
            UIBuilder.CreateImage(
                header,
                "TOBAShimmer",
                sweepSprite,
                new Vector2(
                    180f,
                    420f
                ),
                new Vector2(
                    -520f,
                    0f
                )
            );

        Image sweepImg =
            sweep.GetComponent<Image>();

        sweepImg.color =
            new Color(
                1f,
                0.95f,
                0.55f,
                0.18f
            );

        sweepImg.raycastTarget =
            false;


        RectTransform rtSweep =
            sweep.GetComponent<RectTransform>();

        rtSweep.localEulerAngles =
            new Vector3(
                0f,
                0f,
                -18f
            );


        // ============================================================
        // LOOP SHIMMER
        // ============================================================

        while (
            header != null &&
            sweep != null)
        {
            // Tunggu
            yield return new WaitForSecondsRealtime(
                2.0f
            );


            float duration =
                1.15f;

            float elapsed =
                0f;

            float startX =
                -560f;

            float endX =
                560f;


            while (
                elapsed < duration &&
                sweep != null)
            {
                elapsed +=
                    Time.deltaTime;

                float t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        elapsed /
                        duration
                    );

                rtSweep.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(
                            startX,
                            endX,
                            t
                        ),
                        0f
                    );

                yield return null;
            }


            if (sweep != null)
            {
                rtSweep.anchoredPosition =
                    new Vector2(
                        startX,
                        0f
                    );
            }
        }
    }


    // ============================================================
    // GENERATED SWEEP SPRITE
    // ============================================================

    static Sprite GetSweepSprite()
    {
        if (generatedSweepSprite != null)
            return generatedSweepSprite;

        const int width = 64;
        const int height = 256;

        Texture2D texture =
            new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );

        Color[] pixels =
            new Color[
                width * height
            ];


        for (
            int y = 0;
            y < height;
            y++)
        {
            float vertical =
                y /
                (float)(height - 1);

            for (
                int x = 0;
                x < width;
                x++)
            {
                float horizontal =
                    x /
                    (float)(width - 1);

                float center =
                    Mathf.Abs(
                        horizontal -
                        0.5f
                    ) *
                    2f;

                float alpha =
                    Mathf.Clamp01(
                        1f -
                        center
                    );

                alpha *=
                    0.75f;

                pixels[
                    y * width + x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }


        texture.SetPixels(
            pixels
        );

        texture.Apply();

        generatedSweepSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    width,
                    height
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        return generatedSweepSprite;
    }


    // ============================================================
    // SCREEN ENTRANCE
    // ============================================================

    IEnumerator AnimateScreenEntrance()
    {
        Transform header =
            screenObj.transform.Find(
                "CharacterSelectHeader"
            );

        if (header != null)
        {
            header.localScale =
                Vector3.one *
                0.92f;

            float t =
                0f;

            while (
                t < 0.35f &&
                header != null)
            {
                t +=
                    Time.deltaTime;

                header.localScale =
                    Vector3.Lerp(
                        Vector3.one *
                        0.92f,
                        Vector3.one,
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            t /
                            0.35f
                        )
                    );

                yield return null;
            }
        }


        Transform ucok =
            screenObj.transform.Find(
                "CharacterCard_UCOK"
            );

        Transform butet =
            screenObj.transform.Find(
                "CharacterCard_BUTET"
            );


        if (ucok != null)
        {
            ucok.localScale =
                Vector3.one *
                0.95f;
        }

        if (butet != null)
        {
            butet.localScale =
                Vector3.one *
                0.95f;
        }


        float elapsed =
            0f;

        while (
            elapsed < 0.3f)
        {
            elapsed +=
                Time.deltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    elapsed /
                    0.3f
                );

            if (ucok != null)
            {
                ucok.localScale =
                    Vector3.Lerp(
                        Vector3.one *
                        0.95f,
                        Vector3.one,
                        p
                    );
            }

            if (butet != null)
            {
                butet.localScale =
                    Vector3.Lerp(
                        Vector3.one *
                        0.95f,
                        Vector3.one,
                        p
                    );
            }

            yield return null;
        }
    }
}