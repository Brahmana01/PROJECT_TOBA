using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RegisterNameScreen : BaseScreen
{
    private TMP_InputField nameInput;
    private TMP_Text namePlaceholder;
    private TextMeshProUGUI statusTxt;

    // Input viewport & text
    private RectTransform nameInputViewport;
    private RectTransform nameInputTextRect;
    private TMP_Text nameInputText;

    private static Sprite generatedPatternSprite;

    // ============================================================
    // INPUT SCROLL SETTINGS
    // ============================================================

    private const float INPUT_VIEW_WIDTH = 420f;
    private const float INPUT_VIEW_HEIGHT = 55f;

    private const float INPUT_TEXT_LEFT_PADDING = 10f;
    private const float INPUT_TEXT_RIGHT_PADDING = 10f;


    public RegisterNameScreen(Transform parent)
        : base(parent, "RegisterName")
    {
    }


    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        CreateBackground();
        CreateHeader();
        CreateStepIndicator(2, 3);

        GameObject formCard =
            CreateFormPanel();

        CreatePen(formCard.transform);
        CreateDescription(formCard.transform);
        CreateNameLabel(formCard.transform);
        CreateNameInput(formCard.transform);
        CreateUsernameInfo(formCard.transform);

        statusTxt =
            UIBuilder.CreateTextSized(
                formCard.transform,
                "",
                28,
                TOBATheme.Error,
                TextAlignmentOptions.Center,
                new Vector2(760f, 50f),
                new Vector2(0f, -260f)
            ).GetComponent<TextMeshProUGUI>();

        statusTxt.fontStyle =
            FontStyles.Bold;

        statusTxt.raycastTarget =
            false;

        CreateContinueButton();

        ScreenManager.Instance.StartCoroutine(
            AnimateEntrance()
        );
    }


    // ============================================================
    // LOAD SPRITE
    // ============================================================

    Sprite LoadSprite(string path)
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
            "[RegisterName] Sprite tidak ditemukan: " +
            path
        );

        return null;
    }


    // ============================================================
    // BACKGROUND
    // ============================================================

    void CreateBackground()
    {
        GameObject bg =
            UIBuilder.CreatePanel(
                screenObj.transform,
                "Background",
                new Color(
                    0.012f,
                    0.075f,
                    0.028f,
                    1f
                ),
                Vector2.zero,
                Vector2.one
            );

        Image bgImage =
            bg.GetComponent<Image>();

        if (bgImage != null)
            bgImage.raycastTarget = false;

        Sprite pattern =
            GetPatternSprite();

        if (pattern == null)
            return;

        GameObject patternObject =
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

        Image patternImage =
            patternObject.GetComponent<Image>();

        if (patternImage != null)
        {
            patternImage.type =
                Image.Type.Tiled;

            patternImage.color =
                new Color(
                    0.78f,
                    0.92f,
                    0.45f,
                    0.07f
                );

            patternImage.raycastTarget =
                false;
        }
    }


    Sprite GetPatternSprite()
    {
        if (generatedPatternSprite != null)
            return generatedPatternSprite;

        const int size = 256;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.wrapMode =
            TextureWrapMode.Repeat;

        Color[] pixels =
            new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color motif =
            new Color(
                0.95f,
                0.78f,
                0.28f,
                0.06f
            );

        DrawPattern(pixels, size, 64, 64, motif);
        DrawPattern(pixels, size, 192, 64, motif);
        DrawPattern(pixels, size, 64, 192, motif);
        DrawPattern(pixels, size, 192, 192, motif);

        texture.SetPixels(pixels);
        texture.Apply();

        generatedPatternSprite =
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

        return generatedPatternSprite;
    }


    static void DrawPattern(
        Color[] pixels,
        int size,
        int centerX,
        int centerY,
        Color color)
    {
        for (int y = -48; y <= 48; y++)
        {
            for (int x = -48; x <= 48; x++)
            {
                float nx = x / 48f;
                float ny = y / 48f;

                float radius =
                    Mathf.Sqrt(
                        nx * nx +
                        ny * ny
                    );

                bool ringOuter =
                    radius > 0.72f &&
                    radius < 0.78f;

                bool ringInner =
                    radius > 0.43f &&
                    radius < 0.48f;

                float diamond =
                    Mathf.Abs(nx) +
                    Mathf.Abs(ny);

                bool diamondLine =
                    diamond > 0.90f &&
                    diamond < 0.98f;

                if (
                    ringOuter ||
                    ringInner ||
                    diamondLine)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (
                        px >= 0 &&
                        px < size &&
                        py >= 0 &&
                        py < size)
                    {
                        pixels[
                            py * size + px
                        ] = color;
                    }
                }
            }
        }
    }


    // ============================================================
    // HEADER
    // ============================================================

    void CreateHeader()
    {
        Sprite headerSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_header_clean"
            );

        if (headerSprite != null)
        {
            GameObject header =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "RegisterNameHeader",
                    headerSprite,
                    new Vector2(
                        1060f,
                        320f
                    ),
                    new Vector2(
                        0f,
                        650f
                    )
                );

            Image headerImage =
                header.GetComponent<Image>();

            if (headerImage != null)
            {
                headerImage.preserveAspect =
                    true;

                headerImage.raycastTarget =
                    false;
            }
        }

        Sprite backSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_back_button_v2"
            );

        if (backSprite == null)
            return;

        GameObject back =
            UIBuilder.CreateImage(
                screenObj.transform,
                "BackButton",
                backSprite,
                new Vector2(
                    145f,
                    145f
                ),
                new Vector2(
                    -470f,
                    755f
                )
            );

        Image backImage =
            back.GetComponent<Image>();

        if (backImage != null)
        {
            backImage.preserveAspect =
                true;

            backImage.raycastTarget =
                true;
        }

        Button backButton =
            back.GetComponent<Button>();

        if (backButton == null)
            backButton =
                back.AddComponent<Button>();

        backButton.targetGraphic =
            backImage;

        backButton.transition =
            Selectable.Transition.None;

        backButton.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressBackButton(
                        back.transform
                    )
                );
            }
        );

        ScreenManager.Instance.StartCoroutine(
            FloatBackButton(
                back.transform
            )
        );
    }


    IEnumerator FloatBackButton(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float time = 0f;

        while (target != null)
        {
            time +=
                Time.deltaTime * 1.1f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(time) * 2.5f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(
                        time * 0.8f
                    ) * 0.7f
                );

            yield return null;
        }
    }


    IEnumerator PressBackButton(
        Transform target)
    {
        if (target != null)
        {
            Vector3 original =
                target.localScale;

            float time = 0f;

            while (
                time < 0.08f &&
                target != null)
            {
                time +=
                    Time.deltaTime;

                target.localScale =
                    Vector3.Lerp(
                        original,
                        original * 0.87f,
                        time / 0.08f
                    );

                yield return null;
            }

            time = 0f;

            while (
                time < 0.10f &&
                target != null)
            {
                time +=
                    Time.deltaTime;

                target.localScale =
                    Vector3.Lerp(
                        original * 0.87f,
                        original * 1.05f,
                        time / 0.10f
                    );

                yield return null;
            }

            target.localScale =
                original;
        }

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        ScreenManager.Instance.ChangeScreen(
            new CharacterSelectScreen(
                container
            )
        );
    }


    // ============================================================
    // FORM PANEL
    // ============================================================

    GameObject CreateFormPanel()
    {
        Sprite panelSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_panel"
            );

        GameObject panel =
            new GameObject(
                "FormCard"
            );

        panel.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rect =
            panel.AddComponent<RectTransform>();

        rect.anchorMin =
            rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.sizeDelta =
            new Vector2(
                980f,
                850f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -45f
            );

        Image image =
            panel.AddComponent<Image>();

        if (panelSprite != null)
        {
            image.sprite =
                panelSprite;

            image.type =
                Image.Type.Sliced;

            image.color =
                Color.white;
        }
        else
        {
            image.sprite =
                UIBuilder.GetDefaultSprite();

            image.color =
                TOBATheme.CardBG;
        }

        image.raycastTarget =
            false;

        Shadow shadow =
            panel.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.45f
            );

        shadow.effectDistance =
            new Vector2(
                0f,
                -8f
            );

        return panel;
    }


    // ============================================================
    // PEN
    // ============================================================

    void CreatePen(
        Transform parent)
    {
        Sprite penSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_pen"
            );

        if (penSprite == null)
            return;

        GameObject pen =
            UIBuilder.CreateImage(
                parent,
                "Pen",
                penSprite,
                new Vector2(
                    65f,
                    78f
                ),
                new Vector2(
                    0f,
                    185f
                )
            );

        Image image =
            pen.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }

        ScreenManager.Instance.StartCoroutine(
            FloatPen(
                pen.transform
            )
        );
    }


    IEnumerator FloatPen(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float time = 0f;

        while (target != null)
        {
            time +=
                Time.deltaTime * 0.65f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(time) * 1.5f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    -12f +
                    Mathf.Sin(
                        time * 0.75f
                    ) * 0.6f
                );

            yield return null;
        }
    }


    // ============================================================
    // DESCRIPTION
    // ============================================================

    void CreateDescription(
        Transform parent)
    {
        TextMeshProUGUI description =
            UIBuilder.CreateTextSized(
                parent,
                "Masukkan namamu untuk memulai petualangan!\nNama akan ditampilkan di profil dan leaderboard.",
                29,
                TOBATheme.TextDark,
                TextAlignmentOptions.Center,
                new Vector2(
                    700f,
                    100f
                ),
                new Vector2(
                    0f,
                    70f
                )
            ).GetComponent<TextMeshProUGUI>();

        description.fontStyle =
            FontStyles.Bold;

        description.characterSpacing =
            0.35f;

        description.raycastTarget =
            false;

        UIBuilder.SetWordWrap(
            description,
            true
        );
    }


    // ============================================================
    // NAME LABEL
    // ============================================================

    void CreateNameLabel(
        Transform parent)
    {
        TextMeshProUGUI label =
            UIBuilder.CreateTextSized(
                parent,
                "Nama Lengkap",
                35,
                TOBATheme.TextDark,
                TextAlignmentOptions.Left,
                new Vector2(
                    700f,
                    60f
                ),
                new Vector2(
                    0f,
                    -30f
                )
            ).GetComponent<TextMeshProUGUI>();

        label.fontStyle =
            FontStyles.Bold;

        label.characterSpacing =
            0.5f;

        label.raycastTarget =
            false;
    }


    // ============================================================
    // NAME INPUT
    // ============================================================

    void CreateNameInput(
        Transform parent)
    {
        // --------------------------------------------------------
        // FRAME VISUAL
        // --------------------------------------------------------

        Sprite frameSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_input_frame_v2"
            );

        if (frameSprite != null)
        {
            GameObject frame =
                UIBuilder.CreateImage(
                    parent,
                    "InputFrame",
                    frameSprite,
                    new Vector2(
                        840f,
                        145f
                    ),
                    new Vector2(
                        0f,
                        -125f
                    )
                );

            Image frameImage =
                frame.GetComponent<Image>();

            if (frameImage != null)
            {
                frameImage.preserveAspect =
                    true;

                frameImage.raycastTarget =
                    false;
            }
        }

        // --------------------------------------------------------
        // VIEWPORT
        // --------------------------------------------------------

        GameObject viewport =
            new GameObject(
                "NameInputViewport"
            );

        viewport.transform.SetParent(
            parent,
            false
        );

        nameInputViewport =
            viewport.AddComponent<RectTransform>();

        nameInputViewport.anchorMin =
            nameInputViewport.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        nameInputViewport.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        // ========================================================
        // INI BATAS AREA PUTIH
        // ========================================================

        nameInputViewport.sizeDelta =
            new Vector2(
                INPUT_VIEW_WIDTH,
                INPUT_VIEW_HEIGHT
            );

        nameInputViewport.anchoredPosition =
            new Vector2(
                0f,
                -125f
            );

        RectMask2D mask =
            viewport.AddComponent<RectMask2D>();

        mask.enabled =
            true;


        // --------------------------------------------------------
        // INPUT ROOT
        // --------------------------------------------------------

        GameObject inputObject =
            new GameObject(
                "InputName"
            );

        inputObject.transform.SetParent(
            viewport.transform,
            false
        );

        RectTransform inputRect =
            inputObject.AddComponent<RectTransform>();

        inputRect.anchorMin =
            Vector2.zero;

        inputRect.anchorMax =
            Vector2.one;

        inputRect.offsetMin =
            Vector2.zero;

        inputRect.offsetMax =
            Vector2.zero;


        // --------------------------------------------------------
        // RAYCAST IMAGE
        // --------------------------------------------------------

        Image inputImage =
            inputObject.AddComponent<Image>();

        inputImage.sprite =
            UIBuilder.GetDefaultSprite();

        inputImage.color =
            Color.clear;

        inputImage.raycastTarget =
            true;


        // --------------------------------------------------------
        // TMP INPUT FIELD
        // --------------------------------------------------------

        nameInput =
            inputObject.AddComponent<TMP_InputField>();

        nameInput.characterLimit =
            50;

        nameInput.lineType =
            TMP_InputField.LineType.SingleLine;

        nameInput.keyboardType =
            TouchScreenKeyboardType.Default;

        nameInput.shouldHideMobileInput =
            false;

        nameInput.caretBlinkRate =
            0.85f;

        nameInput.caretWidth =
            3;


        // --------------------------------------------------------
        // TEXT OBJECT
        // --------------------------------------------------------

        GameObject textObject =
            new GameObject(
                "Text"
            );

        textObject.transform.SetParent(
            inputObject.transform,
            false
        );

        nameInputTextRect =
            textObject.AddComponent<RectTransform>();

        nameInputTextRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        nameInputTextRect.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        nameInputTextRect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        nameInputTextRect.sizeDelta =
            new Vector2(
                INPUT_VIEW_WIDTH,
                INPUT_VIEW_HEIGHT
            );

        nameInputTextRect.anchoredPosition =
            Vector2.zero;


        nameInputText =
            textObject.AddComponent<TextMeshProUGUI>();

        nameInputText.text =
            "";

        nameInputText.fontSize =
            29f;

        nameInputText.fontStyle =
            FontStyles.Bold;

        nameInputText.color =
            TOBATheme.TextDark;

        nameInputText.alignment =
            TextAlignmentOptions.MidlineLeft;

        nameInputText.enableWordWrapping =
            false;

        nameInputText.overflowMode =
            TextOverflowModes.Overflow;

        nameInputText.margin =
            new Vector4(
                INPUT_TEXT_LEFT_PADDING,
                0f,
                INPUT_TEXT_RIGHT_PADDING,
                0f
            );

        nameInputText.characterSpacing =
            0.25f;

        nameInputText.raycastTarget =
            false;


        // --------------------------------------------------------
        // PLACEHOLDER
        // --------------------------------------------------------

        GameObject placeholderObject =
            new GameObject(
                "Placeholder"
            );

        placeholderObject.transform.SetParent(
            inputObject.transform,
            false
        );

        RectTransform placeholderRect =
            placeholderObject.AddComponent<RectTransform>();

        placeholderRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        placeholderRect.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        placeholderRect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        placeholderRect.sizeDelta =
            new Vector2(
                INPUT_VIEW_WIDTH,
                INPUT_VIEW_HEIGHT
            );

        placeholderRect.anchoredPosition =
            Vector2.zero;

        namePlaceholder =
            placeholderObject.AddComponent<TextMeshProUGUI>();

        namePlaceholder.text =
            "Contoh: Budi Santoso";

        namePlaceholder.fontSize =
            25f;

        namePlaceholder.fontStyle =
            FontStyles.Normal;

        namePlaceholder.color =
            new Color(
                0.35f,
                0.30f,
                0.23f,
                0.70f
            );

        namePlaceholder.alignment =
            TextAlignmentOptions.MidlineLeft;

        namePlaceholder.enableWordWrapping =
            false;

        namePlaceholder.overflowMode =
            TextOverflowModes.Overflow;

        namePlaceholder.margin =
            new Vector4(
                INPUT_TEXT_LEFT_PADDING,
                0f,
                INPUT_TEXT_RIGHT_PADDING,
                0f
            );

        namePlaceholder.raycastTarget =
            false;


        // --------------------------------------------------------
        // CONNECT TMP
        // --------------------------------------------------------

        nameInput.textComponent =
            nameInputText;

        nameInput.placeholder =
            namePlaceholder;

        nameInput.targetGraphic =
            inputImage;

        // Sangat penting:
        // viewport kita sendiri yang mengatur batas.
        nameInput.textViewport =
            nameInputViewport;


        // --------------------------------------------------------
        // LISTENER
        // --------------------------------------------------------

        nameInput.onValueChanged.AddListener(
            (_) =>
            {
                ResizeAndScrollNameInput();
            }
        );

        nameInput.onSelect.AddListener(
            (_) =>
            {
                ResizeAndScrollNameInput();
            }
        );

        nameInput.onEndEdit.AddListener(
            (_) =>
            {
                ResizeAndScrollNameInput();
            }
        );


        // --------------------------------------------------------
        // INITIAL
        // --------------------------------------------------------

        ResizeAndScrollNameInput();

        // Monitor caret terus-menerus.
        ScreenManager.Instance.StartCoroutine(
            MonitorNameInputScroll()
        );
    }


    // ============================================================
    // AUTO RESIZE + HORIZONTAL SCROLL
    // ============================================================

    void ResizeAndScrollNameInput()
    {
        if (
            nameInput == null ||
            nameInputText == null ||
            nameInputTextRect == null ||
            nameInputViewport == null)
        {
            return;
        }

        string currentText =
            nameInput.text;

        float preferredWidth;

        if (string.IsNullOrEmpty(currentText))
        {
            preferredWidth =
                INPUT_VIEW_WIDTH;
        }
        else
        {
            preferredWidth =
                nameInputText.GetPreferredValues(
                    currentText,
                    2000f,
                    INPUT_VIEW_HEIGHT
                ).x;

            preferredWidth +=
                INPUT_TEXT_LEFT_PADDING +
                INPUT_TEXT_RIGHT_PADDING;
        }

        float contentWidth =
            Mathf.Max(
                INPUT_VIEW_WIDTH,
                preferredWidth
            );

        nameInputTextRect.sizeDelta =
            new Vector2(
                contentWidth,
                INPUT_VIEW_HEIGHT
            );

        UpdateNameInputScroll();
    }


    // ============================================================
    // REAL-TIME CARET SCROLL
    // ============================================================

    void UpdateNameInputScroll()
    {
        if (
            nameInput == null ||
            nameInputText == null ||
            nameInputTextRect == null ||
            nameInputViewport == null)
        {
            return;
        }

        nameInputText.ForceMeshUpdate();

        int characterCount =
            nameInputText.textInfo.characterCount;

        int caretPosition =
            Mathf.Clamp(
                nameInput.caretPosition,
                0,
                nameInput.text.Length
            );

        float caretX = 0f;

        if (
            characterCount > 0 &&
            caretPosition > 0)
        {
            int charIndex =
                Mathf.Clamp(
                    caretPosition - 1,
                    0,
                    characterCount - 1
                );

            TMP_CharacterInfo charInfo =
                nameInputText
                    .textInfo
                    .characterInfo[
                        charIndex
                    ];

            caretX =
                charInfo.xAdvance +
                INPUT_TEXT_LEFT_PADDING;
        }

        else if (characterCount > 0)
        {
            caretX =
                INPUT_TEXT_LEFT_PADDING;
        }


        float currentX =
            nameInputTextRect.anchoredPosition.x;


        float leftLimit =
            0f;

        float rightLimit =
            INPUT_VIEW_WIDTH;


        // ========================================================
        // CURSOR KELUAR KANAN
        // ========================================================

        float visibleCaretX =
            caretX +
            currentX;

        if (
            visibleCaretX >
            rightLimit -
            INPUT_TEXT_RIGHT_PADDING)
        {
            float desiredX =
                rightLimit -
                INPUT_TEXT_RIGHT_PADDING -
                caretX;

            currentX =
                Mathf.Min(
                    0f,
                    desiredX
                );
        }


        // ========================================================
        // CURSOR KELUAR KIRI
        // ========================================================

        if (
            visibleCaretX <
            leftLimit +
            INPUT_TEXT_LEFT_PADDING)
        {
            float desiredX =
                leftLimit +
                INPUT_TEXT_LEFT_PADDING -
                caretX;

            currentX =
                Mathf.Min(
                    0f,
                    desiredX
                );
        }


        // ========================================================
        // BATAS SCROLL
        // ========================================================

        float contentWidth =
            nameInputTextRect.rect.width;

        float minimumX =
            Mathf.Min(
                0f,
                INPUT_VIEW_WIDTH -
                contentWidth
            );

        currentX =
            Mathf.Clamp(
                currentX,
                minimumX,
                0f
            );


        nameInputTextRect.anchoredPosition =
            new Vector2(
                currentX,
                0f
            );
    }


    // ============================================================
    // MONITOR CARET
    // ============================================================

    IEnumerator MonitorNameInputScroll()
    {
        while (
            screenObj != null &&
            nameInput != null)
        {
            if (
                nameInput.isFocused)
            {
                ResizeAndScrollNameInput();
            }

            yield return null;
        }
    }


    // ============================================================
    // USERNAME INFO
    // ============================================================

    void CreateUsernameInfo(
        Transform parent)
    {
        GameObject info =
            UIBuilder.CreatePanelSized(
                parent,
                "InfoBg",
                new Color(
                    TOBATheme.BtnPrimary.r,
                    TOBATheme.BtnPrimary.g,
                    TOBATheme.BtnPrimary.b,
                    0.12f
                ),
                new Vector2(
                    770f,
                    82f
                ),
                new Vector2(
                    0f,
                    -250f
                )
            );

        Outline outline =
            info.AddComponent<Outline>();

        outline.effectColor =
            new Color(
                TOBATheme.BtnPrimary.r,
                TOBATheme.BtnPrimary.g,
                TOBATheme.BtnPrimary.b,
                0.18f
            );

        outline.effectDistance =
            new Vector2(
                1f,
                1f
            );

        TextMeshProUGUI infoText =
            UIBuilder.CreateTextSized(
                info.transform,
                "Username akan dibuat otomatis oleh sistem",
                25,
                TOBATheme.BtnPrimary,
                TextAlignmentOptions.Center,
                new Vector2(
                    720f,
                    65f
                ),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>();

        infoText.fontStyle =
            FontStyles.Bold;

        infoText.characterSpacing =
            0.25f;

        infoText.raycastTarget =
            false;
    }


    // ============================================================
    // CONTINUE BUTTON
    // ============================================================

    void CreateContinueButton()
    {
        Sprite buttonSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_button"
            );

        if (buttonSprite == null)
            return;

        GameObject buttonObject =
            UIBuilder.CreateImage(
                screenObj.transform,
                "BtnLanjut",
                buttonSprite,
                new Vector2(
                    850f,
                    155f
                ),
                new Vector2(
                    0f,
                    -585f
                )
            );

        Image buttonImage =
            buttonObject.GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.preserveAspect =
                true;

            buttonImage.raycastTarget =
                true;
        }

        GameObject labelObject =
            UIBuilder.CreateText(
                buttonObject.transform,
                "LANJUT",
                40,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center
            );

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin =
            Vector2.zero;

        labelRect.anchorMax =
            Vector2.one;

        labelRect.offsetMin =
            Vector2.zero;

        labelRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.fontStyle =
            FontStyles.Bold;

        label.characterSpacing =
            1.2f;

        label.raycastTarget =
            false;

        Button button =
            buttonObject.GetComponent<Button>();

        if (button == null)
            button =
                buttonObject.AddComponent<Button>();

        button.targetGraphic =
            buttonImage;

        button.transition =
            Selectable.Transition.None;

        button.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressContinueButton(
                        buttonObject.transform
                    )
                );
            }
        );

        CreateButtonLeaves(
            buttonObject.transform
        );

        ScreenManager.Instance.StartCoroutine(
            FloatButton(
                buttonObject.transform
            )
        );
    }


    // ============================================================
    // BUTTON PRESS
    // ============================================================

    IEnumerator PressContinueButton(
        Transform target)
    {
        if (target != null)
        {
            Vector3 original =
                target.localScale;

            float time = 0f;

            while (
                time < 0.09f &&
                target != null)
            {
                time +=
                    Time.deltaTime;

                target.localScale =
                    Vector3.Lerp(
                        original,
                        original * 0.91f,
                        time / 0.09f
                    );

                yield return null;
            }

            time = 0f;

            while (
                time < 0.07f &&
                target != null)
            {
                time +=
                    Time.deltaTime;

                target.localScale =
                    Vector3.Lerp(
                        original * 0.91f,
                        original * 1.04f,
                        time / 0.07f
                    );

                yield return null;
            }

            time = 0f;

            while (
                time < 0.06f &&
                target != null)
            {
                time +=
                    Time.deltaTime;

                target.localScale =
                    Vector3.Lerp(
                        original * 1.04f,
                        original,
                        time / 0.06f
                    );

                yield return null;
            }
        }

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        OnLanjutPressed();
    }


    // ============================================================
    // BUTTON LEAVES
    // ============================================================

    void CreateButtonLeaves(
        Transform parent)
    {
        Sprite leaves =
            LoadSprite(
                "RegisterNameScreen/register_name_button_leaves"
            );

        if (leaves == null)
            return;

        GameObject left =
            UIBuilder.CreateImage(
                parent,
                "LeavesLeft",
                leaves,
                new Vector2(
                    190f,
                    125f
                ),
                new Vector2(
                    -380f,
                    0f
                )
            );

        Image leftImage =
            left.GetComponent<Image>();

        if (leftImage != null)
        {
            leftImage.preserveAspect =
                true;

            leftImage.raycastTarget =
                false;
        }

        GameObject right =
            UIBuilder.CreateImage(
                parent,
                "LeavesRight",
                leaves,
                new Vector2(
                    190f,
                    125f
                ),
                new Vector2(
                    380f,
                    0f
                )
            );

        Image rightImage =
            right.GetComponent<Image>();

        if (rightImage != null)
        {
            rightImage.preserveAspect =
                true;

            rightImage.raycastTarget =
                false;
        }

        right
            .GetComponent<RectTransform>()
            .localScale =
            new Vector3(
                -1f,
                1f,
                1f
            );

        ScreenManager.Instance.StartCoroutine(
            SwayLeaves(
                left.transform,
                0f,
                false
            )
        );

        ScreenManager.Instance.StartCoroutine(
            SwayLeaves(
                right.transform,
                1.1f,
                true
            )
        );
    }


    IEnumerator SwayLeaves(
        Transform leaf,
        float phase,
        bool mirrored)
    {
        if (leaf == null)
            yield break;

        Vector3 origin =
            leaf.localPosition;

        float time =
            phase;

        while (leaf != null)
        {
            time +=
                Time.deltaTime *
                0.9f;

            float rotation =
                Mathf.Sin(time) *
                4f;

            float y =
                Mathf.Sin(
                    time * 0.7f
                ) * 2.5f;

            float scale =
                1f +
                Mathf.Sin(
                    time * 0.55f
                ) * 0.025f;

            leaf.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    mirrored
                        ? -rotation
                        : rotation
                );

            leaf.localPosition =
                new Vector3(
                    origin.x,
                    origin.y + y,
                    origin.z
                );

            leaf.localScale =
                new Vector3(
                    mirrored
                        ? -scale
                        : scale,
                    scale,
                    1f
                );

            yield return null;
        }
    }


    // ============================================================
    // BUTTON FLOAT
    // ============================================================

    IEnumerator FloatButton(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float time = 0f;

        while (target != null)
        {
            time +=
                Time.deltaTime *
                0.8f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(time) * 2f,
                    origin.z
                );

            yield return null;
        }
    }


    // ============================================================
    // ENTRANCE
    // ============================================================

    IEnumerator AnimateEntrance()
    {
        Transform header =
            screenObj.transform.Find(
                "RegisterNameHeader"
            );

        Transform panel =
            screenObj.transform.Find(
                "FormCard"
            );

        Transform button =
            screenObj.transform.Find(
                "BtnLanjut"
            );

        if (header != null)
            header.localScale =
                Vector3.one * 0.92f;

        if (panel != null)
            panel.localScale =
                Vector3.one * 0.94f;

        if (button != null)
            button.localScale =
                Vector3.one * 0.92f;

        float elapsed = 0f;

        while (
            elapsed < 0.45f)
        {
            elapsed +=
                Time.deltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    elapsed / 0.45f
                );

            if (header != null)
            {
                header.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.92f,
                        Vector3.one,
                        p
                    );
            }

            if (panel != null)
            {
                panel.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.94f,
                        Vector3.one,
                        p
                    );
            }

            if (button != null)
            {
                button.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.92f,
                        Vector3.one,
                        p
                    );
            }

            yield return null;
        }
    }


    // ============================================================
    // ORIGINAL REGISTRATION LOGIC
    // ============================================================

    void OnLanjutPressed()
    {
        string name =
            nameInput != null
                ? nameInput.text.Trim()
                : "";

        if (string.IsNullOrEmpty(name))
        {
            ShowStatus(
                "Nama tidak boleh kosong!"
            );

            return;
        }

        if (name.Length < 2)
        {
            ShowStatus(
                "Nama minimal 2 karakter."
            );

            return;
        }

        if (name.Length > 50)
        {
            ShowStatus(
                "Nama maksimal 50 karakter."
            );

            return;
        }

        PlayerPrefs.SetString(
            "RegisterName",
            name
        );

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        ScreenManager.Instance.ChangeScreen(
            new RegisterPinScreen(
                container
            )
        );
    }


    // ============================================================
    // STEP INDICATOR
    // ============================================================

    void CreateStepIndicator(
        int current,
        int total)
    {
        GameObject stepBg =
            UIBuilder.CreatePanel(
                screenObj.transform,
                "StepBg",
                Color.clear,
                new Vector2(
                    0.5f,
                    0.5f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

        RectTransform rect =
            stepBg.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(
                300f,
                70f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                390f
            );

        float spacing = 88f;

        float totalWidth =
            (total - 1) *
            spacing;

        for (
            int i = 1;
            i <= total;
            i++)
        {
            bool active =
                i <= current;

            float x =
                -(totalWidth / 2f) +
                ((i - 1) * spacing);

            GameObject dot =
                UIBuilder.CreateTextSized(
                    stepBg.transform,
                    "◆",
                    54,
                    active
                        ? TOBATheme.BatakGold
                        : new Color(
                            0.25f,
                            0.25f,
                            0.25f,
                            1f
                        ),
                    TextAlignmentOptions.Center,
                    new Vector2(
                        62f,
                        62f
                    ),
                    new Vector2(
                        x,
                        0f
                    )
                );

            TextMeshProUGUI dotText =
                dot.GetComponent<TextMeshProUGUI>();

            dotText.fontStyle =
                FontStyles.Bold;

            dotText.raycastTarget =
                false;

            if (i < total)
            {
                GameObject line =
                    UIBuilder.CreatePanelSized(
                        stepBg.transform,
                        "Line_" + i,
                        active
                            ? TOBATheme.BatakGold
                            : new Color(
                                0.25f,
                                0.25f,
                                0.25f,
                                1f
                            ),
                        new Vector2(
                            spacing - 30f,
                            6f
                        ),
                        new Vector2(
                            x +
                            spacing / 2f,
                            0f
                        )
                    );

                line.GetComponent<Image>()
                    .raycastTarget =
                    false;
            }
        }
    }


    // ============================================================
    // STATUS
    // ============================================================

    void ShowStatus(
        string message)
    {
        if (statusTxt != null)
        {
            statusTxt.text =
                message;

            statusTxt.fontStyle =
                FontStyles.Bold;
        }
    }
}