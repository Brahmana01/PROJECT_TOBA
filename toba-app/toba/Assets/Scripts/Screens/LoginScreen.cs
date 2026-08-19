using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoginScreen : BaseScreen
{
    private TMP_InputField usernameInput;
    private TMP_InputField pinInput;
    private TextMeshProUGUI statusTxt;
    private GameObject btnLogin;

    // ============================================================
    // INPUT REFERENCES
    // ============================================================

    private RectTransform usernameViewport;
    private RectTransform usernameTextRect;
    private TMP_Text usernameText;

    private RectTransform pinViewport;
    private RectTransform pinTextRect;
    private TMP_Text pinText;

    // ============================================================
    // INPUT AREA
    // Disesuaikan dengan frame login
    // ============================================================

    private const float INPUT_VIEW_WIDTH = 355f;
    private const float INPUT_VIEW_HEIGHT = 55f;

    private const float INPUT_LEFT_PADDING = 14f;
    private const float INPUT_RIGHT_PADDING = 14f;

    private const float INPUT_OFFSET_X = 0f;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public LoginScreen(Transform parent)
        : base(parent, "LoginScreen")
    {
    }

    // ============================================================
    // SHOW
    // ============================================================

    public override void Show()
    {
        SetBackground(
            TOBATheme.ForestGreen
        );

        CreateBackground();
        CreateHeader();

        GameObject panel =
            CreatePanel();

        CreateProfileBadge(
            panel.transform
        );

        CreateTitle(
            panel.transform
        );

        CreateDivider(
            panel.transform
        );

        // ========================================================
        // USERNAME
        // ========================================================

        CreateLabel(
            panel.transform,
            "Username",
            new Vector2(
                -120f,
                18f
            )
        );

        usernameInput =
            CreateLoginInput(
                panel.transform,
                "UsernameInput",
                "Contoh: bagus1234",
                new Vector2(
                    0f,
                    -65f
                ),
                false,
                out usernameViewport,
                out usernameTextRect,
                out usernameText
            );

        // ========================================================
        // PIN
        // ========================================================

        CreateLabel(
            panel.transform,
            "PIN (6 digit)",
            new Vector2(
                -120f,
                -150f
            )
        );

        pinInput =
            CreateLoginInput(
                panel.transform,
                "PinInput",
                "••••••",
                new Vector2(
                    0f,
                    -235f
                ),
                true,
                out pinViewport,
                out pinTextRect,
                out pinText
            );

        pinInput.characterLimit = 6;

        // ========================================================
        // STATUS
        // ========================================================

        CreateStatus(
            panel.transform
        );

        // ========================================================
        // LOGIN BUTTON
        // ========================================================

        CreateLoginButton(
            panel.transform
        );

        // ========================================================
        // REGISTER LINK
        // ========================================================

        CreateRegisterLink();

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

        if (texture == null)
        {
            Debug.LogWarning(
                "[Login] Sprite tidak ditemukan: " + path
            );

            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
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

    // ============================================================
    // BACKGROUND
    // ============================================================

    void CreateBackground()
    {
        GameObject bg =
            UIBuilder.CreatePanel(
                screenObj.transform,
                "Background",
                TOBATheme.ForestGreen,
                Vector2.zero,
                Vector2.one
            );

        Image image =
            bg.GetComponent<Image>();

        if (image != null)
            image.raycastTarget = false;
    }

    // ============================================================
    // HEADER + BACK BUTTON
    // ============================================================

    void CreateHeader()
    {
        Sprite headerSprite =
            LoadSprite(
                "LoginScreen/login_header"
            );

        if (headerSprite != null)
        {
            GameObject header =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "LoginHeader",
                    headerSprite,
                    new Vector2(
                        1050f,
                        300f
                    ),
                    new Vector2(
                        0f,
                        700f
                    )
                );

            Image image =
                header.GetComponent<Image>();

            if (image != null)
            {
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
        }

        // ========================================================
        // BACK BUTTON
        // Tetap gunakan asset konsisten
        // ========================================================

        Sprite backSprite =
            LoadSprite(
                "RegisterNameScreen/register_name_back_button_v2"
            );

        if (backSprite == null)
        {
            Debug.LogWarning(
                "[Login] Tombol kembali tidak ditemukan."
            );

            return;
        }

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
                    790f
                )
            );

        Image backImage =
            back.GetComponent<Image>();

        if (backImage != null)
        {
            backImage.preserveAspect = true;
            backImage.raycastTarget = true;
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

    // ============================================================
    // PANEL
    // ============================================================

    GameObject CreatePanel()
    {
        Sprite panelSprite =
            LoadSprite(
                "LoginScreen/login_panel"
            );

        GameObject panel =
            new GameObject(
                "LoginPanel"
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
                990f
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

        return panel;
    }

    // ============================================================
    // PROFILE
    // ============================================================

    void CreateProfileBadge(
        Transform parent
    )
    {
        Sprite badgeSprite =
            LoadSprite(
                "LoginScreen/login_profile_badge"
            );

        if (badgeSprite == null)
            return;

        GameObject badge =
            UIBuilder.CreateImage(
                parent,
                "ProfileBadge",
                badgeSprite,
                new Vector2(
                    180f,
                    180f
                ),
                new Vector2(
                    0f,
                    295f
                )
            );

        Image image =
            badge.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    // ============================================================
    // TITLE
    // ============================================================

    void CreateTitle(
        Transform parent
    )
    {
        TextMeshProUGUI title =
            UIBuilder.CreateTextSized(
                parent,
                "Selamat Datang Kembali!",
                43,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center,
                new Vector2(
                    700f,
                    60f
                ),
                new Vector2(
                    0f,
                    180f
                )
            ).GetComponent<TextMeshProUGUI>();

        title.fontStyle =
            FontStyles.Bold;

        title.characterSpacing =
            0.25f;

        title.raycastTarget = false;

        TextMeshProUGUI subtitle =
            UIBuilder.CreateTextSized(
                parent,
                "Masuk dengan username dan PIN kamu",
                27,
                TOBATheme.TextGray,
                TextAlignmentOptions.Center,
                new Vector2(
                    700f,
                    45f
                ),
                new Vector2(
                    0f,
                    130f
                )
            ).GetComponent<TextMeshProUGUI>();

        subtitle.fontStyle =
            FontStyles.Normal;

        subtitle.raycastTarget = false;
    }

    // ============================================================
    // DIVIDER
    // ============================================================

    void CreateDivider(
        Transform parent
    )
    {
        Sprite dividerSprite =
            LoadSprite(
                "LoginScreen/login_divider"
            );

        if (dividerSprite == null)
            return;

        GameObject divider =
            UIBuilder.CreateImage(
                parent,
                "Divider",
                dividerSprite,
                new Vector2(
                    700f,
                    34f
                ),
                new Vector2(
                    0f,
                    80f
                )
            );

        Image image =
            divider.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    // ============================================================
    // LABEL
    // ============================================================

    void CreateLabel(
        Transform parent,
        string value,
        Vector2 position
    )
    {
        TextMeshProUGUI label =
            UIBuilder.CreateTextSized(
                parent,
                value,
                30,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Left,
                new Vector2(
                    320f,
                    48f
                ),
                position
            ).GetComponent<TextMeshProUGUI>();

        label.fontStyle =
            FontStyles.Bold;

        label.characterSpacing =
            0.3f;

        label.raycastTarget =
            false;
    }

    // ============================================================
    // LOGIN INPUT
    // ============================================================

    TMP_InputField CreateLoginInput(
        Transform parent,
        string objectName,
        string placeholderText,
        Vector2 position,
        bool isPassword,
        out RectTransform viewport,
        out RectTransform textRect,
        out TMP_Text inputText
    )
    {
        // --------------------------------------------------------
        // FRAME VISUAL
        // --------------------------------------------------------

        Sprite frameSprite =
            LoadSprite(
                "LoginScreen/login_input_frame"
            );

        if (frameSprite != null)
        {
            GameObject frame =
                UIBuilder.CreateImage(
                    parent,
                    objectName + "_Frame",
                    frameSprite,
                    new Vector2(
                        830f,
                        140f
                    ),
                    position
                );

            Image frameImage =
                frame.GetComponent<Image>();

            if (frameImage != null)
            {
                frameImage.preserveAspect = true;
                frameImage.raycastTarget = false;
            }
        }

        // --------------------------------------------------------
        // VIEWPORT
        // --------------------------------------------------------

        GameObject viewportObject =
            new GameObject(
                objectName + "_Viewport"
            );

        viewportObject.transform.SetParent(
            parent,
            false
        );

        viewport =
            viewportObject.AddComponent<
                RectTransform
            >();

        viewport.anchorMin =
            viewport.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        viewport.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        viewport.sizeDelta =
            new Vector2(
                INPUT_VIEW_WIDTH,
                INPUT_VIEW_HEIGHT
            );

        viewport.anchoredPosition =
            new Vector2(
                position.x +
                INPUT_OFFSET_X,
                position.y
            );

        viewportObject.AddComponent<
            RectMask2D
        >();

        // --------------------------------------------------------
        // INPUT ROOT
        // --------------------------------------------------------

        GameObject inputObject =
            new GameObject(
                objectName
            );

        inputObject.transform.SetParent(
            viewportObject.transform,
            false
        );

        RectTransform inputRect =
            inputObject.AddComponent<
                RectTransform
            >();

        inputRect.anchorMin =
            Vector2.zero;

        inputRect.anchorMax =
            Vector2.one;

        inputRect.offsetMin =
            Vector2.zero;

        inputRect.offsetMax =
            Vector2.zero;

        Image inputImage =
            inputObject.AddComponent<Image>();

        inputImage.sprite =
            UIBuilder.GetDefaultSprite();

        inputImage.color =
            Color.clear;

        inputImage.raycastTarget =
            true;

        // --------------------------------------------------------
        // TMP INPUT
        // --------------------------------------------------------

        TMP_InputField input =
            inputObject.AddComponent<
                TMP_InputField
            >();

        input.characterLimit =
            isPassword
                ? 6
                : 50;

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.keyboardType =
            isPassword
                ? TouchScreenKeyboardType.NumberPad
                : TouchScreenKeyboardType.Default;

        input.shouldHideMobileInput =
            false;

        input.caretBlinkRate =
            0.85f;

        input.caretWidth =
            3;

        if (isPassword)
        {
            input.contentType =
                TMP_InputField.ContentType.Pin;
        }

        // --------------------------------------------------------
        // TEXT
        // --------------------------------------------------------

        GameObject textObject =
            new GameObject(
                "Text"
            );

        textObject.transform.SetParent(
            inputObject.transform,
            false
        );

        textRect =
            textObject.AddComponent<
                RectTransform
            >();

        textRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        textRect.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        textRect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        textRect.sizeDelta =
            new Vector2(
                INPUT_VIEW_WIDTH,
                INPUT_VIEW_HEIGHT
            );

        textRect.anchoredPosition =
            Vector2.zero;

        TextMeshProUGUI tmp =
            textObject.AddComponent<
                TextMeshProUGUI
            >();

        inputText =
            tmp;

        tmp.text =
            "";

        tmp.fontSize =
            isPassword
                ? 31f
                : 29f;

        tmp.fontStyle =
            FontStyles.Bold;

        tmp.color =
            TOBATheme.WoodDark;

        tmp.alignment =
            TextAlignmentOptions.MidlineLeft;

        tmp.enableWordWrapping =
            false;

        tmp.overflowMode =
            TextOverflowModes.Overflow;

        tmp.margin =
            new Vector4(
                INPUT_LEFT_PADDING,
                0f,
                INPUT_RIGHT_PADDING,
                0f
            );

        tmp.characterSpacing =
            0.25f;

        tmp.raycastTarget =
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
            placeholderObject.AddComponent<
                RectTransform
            >();

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

        TextMeshProUGUI placeholder =
            placeholderObject.AddComponent<
                TextMeshProUGUI
            >();

        placeholder.text =
            placeholderText;

        placeholder.fontSize =
            24f;

        placeholder.fontStyle =
            FontStyles.Normal;

        placeholder.color =
            new Color(
                0.35f,
                0.30f,
                0.23f,
                0.70f
            );

        placeholder.alignment =
            TextAlignmentOptions.MidlineLeft;

        placeholder.enableWordWrapping =
            false;

        placeholder.overflowMode =
            TextOverflowModes.Overflow;

        placeholder.margin =
            new Vector4(
                INPUT_LEFT_PADDING,
                0f,
                INPUT_RIGHT_PADDING,
                0f
            );

        placeholder.raycastTarget =
            false;

        // --------------------------------------------------------
        // CONNECT
        // --------------------------------------------------------

        input.textComponent =
            tmp;

        input.placeholder =
            placeholder;

        input.textViewport =
            viewport;

        input.targetGraphic =
            inputImage;

        // Simpan ke local variable supaya aman dipakai lambda.
        RectTransform localTextRect =
            textRect;

        TMP_Text localText =
            tmp;

        input.onValueChanged.AddListener(
            _ =>
            {
                UpdateInputScroll(
                    input,
                    localTextRect,
                    localText
                );
            }
        );

        input.onSelect.AddListener(
            _ =>
            {
                UpdateInputScroll(
                    input,
                    localTextRect,
                    localText
                );
            }
        );

        input.onEndEdit.AddListener(
            _ =>
            {
                UpdateInputScroll(
                    input,
                    localTextRect,
                    localText
                );
            }
        );

        UpdateInputScroll(
            input,
            localTextRect,
            localText
        );

        return input;
    }

    // ============================================================
    // AUTO SCROLL
    // ============================================================

    void UpdateInputScroll(
        TMP_InputField input,
        RectTransform textRect,
        TMP_Text text
    )
    {
        if (
            input == null ||
            textRect == null ||
            text == null
        )
            return;

        string currentText =
            input.text;

        float preferredWidth;

        if (
            string.IsNullOrEmpty(
                currentText
            )
        )
        {
            preferredWidth =
                INPUT_VIEW_WIDTH;
        }
        else
        {
            preferredWidth =
                text.GetPreferredValues(
                    currentText,
                    2000f,
                    INPUT_VIEW_HEIGHT
                ).x +
                INPUT_LEFT_PADDING +
                INPUT_RIGHT_PADDING;
        }

        float contentWidth =
            Mathf.Max(
                INPUT_VIEW_WIDTH,
                preferredWidth
            );

        textRect.sizeDelta =
            new Vector2(
                contentWidth,
                INPUT_VIEW_HEIGHT
            );

        text.ForceMeshUpdate();

        int characterCount =
            text.textInfo.characterCount;

        int caretPosition =
            Mathf.Clamp(
                input.caretPosition,
                0,
                input.text.Length
            );

        float caretX =
            INPUT_LEFT_PADDING;

        if (
            characterCount > 0 &&
            caretPosition > 0
        )
        {
            int charIndex =
                Mathf.Clamp(
                    caretPosition - 1,
                    0,
                    characterCount - 1
                );

            TMP_CharacterInfo info =
                text.textInfo.characterInfo[
                    charIndex
                ];

            caretX =
                info.xAdvance +
                INPUT_LEFT_PADDING;
        }

        float currentX =
            textRect.anchoredPosition.x;

        float visibleCaretX =
            caretX +
            currentX;

        // Geser ke kiri ketika caret mencapai sisi kanan.
        float rightLimit =
            INPUT_VIEW_WIDTH -
            INPUT_RIGHT_PADDING;

        if (
            visibleCaretX >
            rightLimit
        )
        {
            currentX =
                rightLimit -
                caretX;
        }

        // Kembalikan scroll ketika caret bergerak ke kiri.
        float leftLimit =
            INPUT_LEFT_PADDING;

        if (
            visibleCaretX <
            leftLimit
        )
        {
            currentX =
                leftLimit -
                caretX;
        }

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

        textRect.anchoredPosition =
            new Vector2(
                currentX,
                0f
            );
    }

    // ============================================================
    // REAL-TIME MONITOR
    // ============================================================

    IEnumerator MonitorInputScroll(
        TMP_InputField input,
        RectTransform textRect,
        TMP_Text text
    )
    {
        while (
            screenObj != null &&
            input != null
        )
        {
            if (input.isFocused)
            {
                UpdateInputScroll(
                    input,
                    textRect,
                    text
                );
            }

            yield return null;
        }
    }

    void StartInputMonitors()
    {
        if (
            usernameInput != null &&
            usernameTextRect != null &&
            usernameText != null
        )
        {
            ScreenManager.Instance.StartCoroutine(
                MonitorInputScroll(
                    usernameInput,
                    usernameTextRect,
                    usernameText
                )
            );
        }

        if (
            pinInput != null &&
            pinTextRect != null &&
            pinText != null
        )
        {
            ScreenManager.Instance.StartCoroutine(
                MonitorInputScroll(
                    pinInput,
                    pinTextRect,
                    pinText
                )
            );
        }
    }

    // ============================================================
    // STATUS
    // Posisi tetap mengikuti tampilan sekarang.
    // ============================================================

    void CreateStatus(
        Transform parent
    )
    {
        statusTxt =
            UIBuilder.CreateTextSized(
                parent,
                "",
                24,
                TOBATheme.Error,
                TextAlignmentOptions.Center,
                new Vector2(
                    700f,
                    55f
                ),
                new Vector2(
                    0f,
                    -330f
                )
            ).GetComponent<TextMeshProUGUI>();

        statusTxt.fontStyle =
            FontStyles.Bold;

        statusTxt.enableWordWrapping =
            true;

        statusTxt.raycastTarget =
            false;
    }

    // ============================================================
    // LOGIN BUTTON
    // Tampilan tetap sama.
    // Logic asli tetap dipanggil.
    // ============================================================

    void CreateLoginButton(
        Transform parent
    )
    {
        btnLogin =
            UIBuilder.CreateButton(
                parent,
                "BtnLogin",
                "MASUK",
                new Vector2(
                    650f,
                    145f
                ),
                TOBATheme.BtnPrimary,
                null
            );

        btnLogin
            .GetComponent<RectTransform>()
            .anchoredPosition =
            new Vector2(
                0f,
                -380f
            );

        TextMeshProUGUI label =
            btnLogin.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (label != null)
        {
            label.fontSize =
                39f;

            label.fontStyle =
                FontStyles.Bold;

            label.characterSpacing =
                1f;

            label.raycastTarget =
                false;
        }

        Button button =
            btnLogin.GetComponentInChildren<Button>();

        if (button == null)
        {
            Debug.LogWarning(
                "[Login] Button MASUK tidak ditemukan."
            );

            return;
        }

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color(
                1f,
                1f,
                0.86f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.82f,
                0.75f,
                0.48f,
                1f
            );

        button.colors =
            colors;

        button.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressLoginButton(
                        btnLogin.transform
                    )
                );
            }
        );
    }

    IEnumerator PressLoginButton(
        Transform target
    )
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        float time = 0f;

        while (
            time < 0.09f &&
            target != null
        )
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
            target != null
        )
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

        target.localScale =
            original;

        // ========================================================
        // LOGIC ASLI
        // ========================================================

        OnLoginPressed();
    }

    // ============================================================
    // REGISTER LINK
    // ============================================================

    void CreateRegisterLink()
    {
        GameObject root =
            new GameObject(
                "RegisterLink"
            );

        root.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rect =
            root.AddComponent<RectTransform>();

        rect.anchorMin =
            rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.sizeDelta =
            new Vector2(
                680f,
                65f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -600f
            );

        TextMeshProUGUI text =
            UIBuilder.CreateTextSized(
                root.transform,
                "Belum punya akun? Daftar di sini",
                26,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(
                    600f,
                    55f
                ),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>();

        text.fontStyle =
            FontStyles.Bold;

        text.characterSpacing =
            0.15f;

        text.raycastTarget =
            false;

        Button button =
            root.AddComponent<Button>();

        button.transition =
            Selectable.Transition.None;

        button.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressRegisterLink(
                        root.transform
                    )
                );
            }
        );
    }

    IEnumerator PressRegisterLink(
        Transform target
    )
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        float time = 0f;

        while (
            time < 0.08f &&
            target != null
        )
        {
            time +=
                Time.deltaTime;

            target.localScale =
                Vector3.Lerp(
                    original,
                    original * 0.94f,
                    time / 0.08f
                );

            yield return null;
        }

        target.localScale =
            original;

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        // Logic asli
        ScreenManager.Instance.ChangeScreen(
            new CharacterSelectScreen(
                container
            )
        );
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    IEnumerator FloatBackButton(
        Transform target
    )
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float time = 0f;

        while (
            target != null
        )
        {
            time +=
                Time.deltaTime * 1.1f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(time) *
                    2.5f,
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
        Transform target
    )
    {
        if (target != null)
        {
            Vector3 original =
                target.localScale;

            float time = 0f;

            while (
                time < 0.08f &&
                target != null
            )
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
                target != null
            )
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

        // Logic asli
        ScreenManager.Instance.ChangeScreen(
            new WelcomeScreen(
                container
            )
        );
    }

    // ============================================================
    // LOGIN LOGIC ASLI
    // JANGAN DIUBAH
    // ============================================================

    void OnLoginPressed()
    {
        SoundManager.Instance.PlaySFX("sfx_click");

        string username =
            usernameInput != null
                ? usernameInput.text.Trim()
                : "";

        string pin =
            pinInput != null
                ? pinInput.text.Trim()
                : "";

        if (string.IsNullOrEmpty(username))
        {
            ShowStatus(
                "Username tidak boleh kosong."
            );

            return;
        }

        if (pin.Length != 6)
        {
            ShowStatus(
                "PIN harus 6 digit."
            );

            return;
        }

        ShowStatus(
            "Sedang masuk...",
            false
        );

        SetButtonInteractable(false);

        string deviceID =
            SystemInfo.deviceUniqueIdentifier;

        LoginRequest req =
            new LoginRequest
            {
                username = username,
                pin = pin,
                device_id = deviceID
            };

        APIManager.Instance.PostRequest(
            "/login",
            JsonUtility.ToJson(req),
            (response) =>
            {
                if (screenObj == null)
                    return;

                AuthResponse res =
                    JsonUtility.FromJson<AuthResponse>(
                        response
                    );

                if (
                    res != null &&
                    !string.IsNullOrEmpty(
                        res.token
                    )
                )
                {
                    PlayerPrefs.SetString(
                        "UserToken",
                        res.token
                    );

                    PlayerPrefs.SetString(
                        "SavedUsername",
                        res.GetValidUsername()
                    );

                    PlayerPrefs.SetInt(
                        "PlayerID",
                        res.GetValidID()
                    );

                    PlayerPrefs.SetInt(
                        "PlayerLevel",
                        res.level
                    );

                    PlayerPrefs.Save();

                    SoundManager.Instance.PlaySFX(
                        "sfx_win"
                    );

                    ScreenManager.Instance.ChangeScreen(
                        new MainMenuScreen(
                            container
                        )
                    );
                }
                else
                {
                    ShowStatus(
                        "Username atau PIN salah."
                    );

                    SetButtonInteractable(
                        true
                    );
                }
            },
(err) =>
{
    // ========================================================
    // SERVER BENAR-BENAR TIDAK BISA DIHUBUNGI
    // ========================================================

    if (
        string.IsNullOrEmpty(err) ||
        err == "NETWORK_ERROR"
    )
    {
        ShowStatus(
            "Koneksi gagal. Cek internet."
        );

        SetButtonInteractable(true);
        return;
    }

    // ========================================================
    // RESPONSE ERROR DARI BACKEND
    // ========================================================

    try
    {
        LoginErrorResponse error =
            JsonUtility.FromJson<LoginErrorResponse>(
                err
            );

        if (
            error != null &&
            !string.IsNullOrEmpty(
                error.message
            )
        )
        {
            // Kalau backend mengirim pesan tertentu,
            // tampilkan pesan tersebut.
            ShowStatus(
                error.message
            );
        }
        else
        {
            // Kalau backend mengirim plain text.
            ShowStatus(
                err
            );
        }
    }
    catch
    {
        // Response bukan JSON.
        ShowStatus(
            err
        );
    }

    SetButtonInteractable(true);
}
        );
    }

    // ============================================================
    // STATUS
    // ============================================================

    void ShowStatus(
        string msg,
        bool isError = true
    )
    {
        if (statusTxt != null)
        {
            statusTxt.text =
                msg;

            statusTxt.color =
                isError
                    ? TOBATheme.Error
                    : TOBATheme.BatakGold;
        }
    }

    // ============================================================
    // BUTTON STATE
    // KEMBALI KE POLA KODE ASLI
    // ============================================================

    void SetButtonInteractable(
        bool interactable
    )
    {
        if (btnLogin != null)
        {
            Button btn =
                btnLogin.GetComponentInChildren<Button>();

            if (btn != null)
                btn.interactable =
                    interactable;
        }
    }

    // ============================================================
    // ENTRANCE
    // ============================================================

    IEnumerator AnimateEntrance()
    {
        Transform header =
            screenObj.transform.Find(
                "LoginHeader"
            );

        Transform panel =
            screenObj.transform.Find(
                "LoginPanel"
            );

        Transform button =
            panel != null
                ? panel.Find(
                    "BtnLogin"
                )
                : null;

        if (header != null)
        {
            header.localScale =
                Vector3.one * 0.94f;
        }

        if (panel != null)
        {
            panel.localScale =
                Vector3.one * 0.95f;
        }

        if (button != null)
        {
            button.localScale =
                Vector3.one * 0.94f;
        }

        float elapsed = 0f;

        while (
            elapsed < 0.4f
        )
        {
            elapsed +=
                Time.deltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    elapsed / 0.4f
                );

            if (header != null)
            {
                header.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.94f,
                        Vector3.one,
                        p
                    );
            }

            if (panel != null)
            {
                panel.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.95f,
                        Vector3.one,
                        p
                    );
            }

            if (button != null)
            {
                button.localScale =
                    Vector3.Lerp(
                        Vector3.one * 0.94f,
                        Vector3.one,
                        p
                    );
            }

            yield return null;
        }

        StartInputMonitors();
    }
}