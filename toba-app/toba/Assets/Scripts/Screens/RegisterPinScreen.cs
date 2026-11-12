using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RegisterPinScreen : BaseScreen
{
    private TMP_InputField pinInput;
    private TMP_InputField pinConfirmInput;
    private TextMeshProUGUI statusTxt;
    private GameObject btnDaftar;

    private static Sprite patternSprite;

    private const float VIEW_W = 420f;
    private const float VIEW_H = 55f;
    private const float PAD = 10f;

    public RegisterPinScreen(Transform parent)
        : base(parent, "RegisterPin") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        CreateBackground();
        CreateHeader();
        CreateSteps();

        Transform form =
            CreateForm().transform;

        CreatePen(form);

        CreateText(
            form,
            "PIN digunakan untuk masuk ke akunmu.\nJangan beritahu PIN ke siapapun!",
            29,
            new Vector2(700, 90),
            new Vector2(0, 155),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreateLabel(
            form,
            "PIN (6 digit)",
            15
        );

        pinInput =
            CreateInput(
                form,
                "InputPIN",
                -75
            );

        CreateLabel(
            form,
            "Konfirmasi PIN",
            -155
        );

        pinConfirmInput =
            CreateInput(
                form,
                "InputPINConfirm",
                -245
            );

        statusTxt =
            CreateText(
                form,
                "",
                28,
                new Vector2(760, 50),
                new Vector2(0, -325),
                FontStyles.Bold,
                TextAlignmentOptions.Center
            );

        CreateButton();

        ScreenManager.Instance.StartCoroutine(
            AnimateEntrance()
        );
    }

    // ============================================================
    // SPRITE
    // ============================================================

    Sprite Load(string path)
    {
        return Resources.Load<Sprite>(path);
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
                new Color(.012f, .075f, .028f, 1f),
                Vector2.zero,
                Vector2.one
            );

        Image bgImage =
            bg.GetComponent<Image>();

        if (bgImage != null)
            bgImage.raycastTarget = false;

        if (patternSprite == null)
        {
            const int size = 256;

            Texture2D tex =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false
                );

            tex.wrapMode =
                TextureWrapMode.Repeat;

            Color[] pixels =
                new Color[size * size];

            Color motif =
                new Color(
                    .95f,
                    .78f,
                    .28f,
                    .06f
                );

            DrawPattern(
                pixels,
                size,
                64,
                64,
                motif
            );

            DrawPattern(
                pixels,
                size,
                192,
                64,
                motif
            );

            DrawPattern(
                pixels,
                size,
                64,
                192,
                motif
            );

            DrawPattern(
                pixels,
                size,
                192,
                192,
                motif
            );

            tex.SetPixels(pixels);
            tex.Apply();

            patternSprite =
                Sprite.Create(
                    tex,
                    new Rect(
                        0,
                        0,
                        size,
                        size
                    ),
                    new Vector2(
                        .5f,
                        .5f
                    ),
                    100f
                );
        }

        GameObject pattern =
            UIBuilder.CreateImage(
                screenObj.transform,
                "BackgroundPattern",
                patternSprite,
                new Vector2(
                    1080,
                    1920
                ),
                Vector2.zero
            );

        Image patternImage =
            pattern.GetComponent<Image>();

        if (patternImage != null)
        {
            patternImage.type =
                Image.Type.Tiled;

            patternImage.color =
                new Color(
                    .78f,
                    .92f,
                    .45f,
                    .07f
                );

            patternImage.raycastTarget =
                false;
        }
    }

    static void DrawPattern(
        Color[] pixels,
        int size,
        int cx,
        int cy,
        Color color)
    {
        for (int y = -48; y <= 48; y++)
        {
            for (int x = -48; x <= 48; x++)
            {
                float nx = x / 48f;
                float ny = y / 48f;

                float r =
                    Mathf.Sqrt(
                        nx * nx +
                        ny * ny
                    );

                bool outer =
                    r > .72f &&
                    r < .78f;

                bool inner =
                    r > .43f &&
                    r < .48f;

                float d =
                    Mathf.Abs(nx) +
                    Mathf.Abs(ny);

                bool diamond =
                    d > .90f &&
                    d < .98f;

                if (
                    !outer &&
                    !inner &&
                    !diamond
                )
                    continue;

                int px =
                    cx + x;

                int py =
                    cy + y;

                if (
                    px >= 0 &&
                    px < size &&
                    py >= 0 &&
                    py < size
                )
                {
                    pixels[
                        py * size + px
                    ] = color;
                }
            }
        }
    }

    // ============================================================
    // HEADER + BACK BUTTON
    // ============================================================

    void CreateHeader()
    {
        Sprite header =
            Load(
                "RegisterPinScreen/register_pin_header"
            );

        if (header != null)
        {
            GameObject go =
                UIBuilder.CreateImage(
                    screenObj.transform,
                    "RegisterPinHeader",
                    header,
                    new Vector2(
                        1060,
                        320
                    ),
                    new Vector2(
                        0,
                        650
                    )
                );

            Image image =
                go.GetComponent<Image>();

            if (image != null)
                image.preserveAspect = true;
        }

        Sprite back =
            Load(
                "RegisterNameScreen/register_name_back_button_v2"
            );

        if (back == null)
            return;

        GameObject button =
            UIBuilder.CreateImage(
                screenObj.transform,
                "BackButton",
                back,
                new Vector2(
                    145,
                    145
                ),
                new Vector2(
                    -470,
                    755
                )
            );

        Image imageButton =
            button.GetComponent<Image>();

        if (imageButton != null)
        {
            imageButton.preserveAspect =
                true;

            imageButton.raycastTarget =
                true;
        }

        Button btn =
            button.GetComponent<Button>();

        if (btn == null)
            btn =
                button.AddComponent<Button>();

        btn.targetGraphic =
            imageButton;

        btn.transition =
            Selectable.Transition.None;

        btn.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressBack(
                        button.transform
                    )
                );
            }
        );

        ScreenManager.Instance.StartCoroutine(
            FloatBack(
                button.transform
            )
        );
    }

    IEnumerator FloatBack(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                1.1f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) * 2.5f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0,
                    0,
                    Mathf.Sin(
                        t * .8f
                    ) * .7f
                );

            yield return null;
        }
    }

    IEnumerator PressBack(
        Transform target)
    {
        if (target != null)
        {
            Vector3 original =
                target.localScale;

            target.localScale =
                original * .87f;

            yield return new WaitForSeconds(
                .08f
            );

            target.localScale =
                original * 1.05f;

            yield return new WaitForSeconds(
                .10f
            );

            target.localScale =
                original;
        }

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        ScreenManager.Instance.ChangeScreen(
            new RegisterNameScreen(
                container
            )
        );
    }

    // ============================================================
    // FORM
    // ============================================================

    GameObject CreateForm()
    {
        Sprite sprite =
            Load(
                "RegisterNameScreen/register_name_panel"
            );

        GameObject form =
            new GameObject(
                "FormCard"
            );

        form.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rt =
            form.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                .5f,
                .5f
            );

        rt.sizeDelta =
            new Vector2(
                980,
                850
            );

        rt.anchoredPosition =
            new Vector2(
                0,
                -45
            );

        Image image =
            form.AddComponent<Image>();

        image.sprite =
            sprite ??
            UIBuilder.GetDefaultSprite();

        image.type =
            sprite != null
                ? Image.Type.Sliced
                : Image.Type.Simple;

        image.raycastTarget =
            false;

        Shadow shadow =
            form.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(
                0,
                0,
                0,
                .45f
            );

        shadow.effectDistance =
            new Vector2(
                0,
                -8
            );

        return form;
    }

    // ============================================================
    // PEN
    // ============================================================

    void CreatePen(
        Transform parent)
    {
        Sprite sprite =
            Load(
                "RegisterNameScreen/register_name_pen"
            );

        if (sprite == null)
            return;

        GameObject pen =
            UIBuilder.CreateImage(
                parent,
                "Pen",
                sprite,
                new Vector2(
                    65,
                    78
                ),
                new Vector2(
                    0,
                    240
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

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                .65f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) * 1.5f,
                    origin.z
                );

            target.localEulerAngles =
                new Vector3(
                    0,
                    0,
                    -12f +
                    Mathf.Sin(
                        t * .75f
                    ) * .6f
                );

            yield return null;
        }
    }

    // ============================================================
    // TEXT
    // ============================================================

    TextMeshProUGUI CreateText(
        Transform parent,
        string value,
        int size,
        Vector2 area,
        Vector2 pos,
        FontStyles style,
        TextAlignmentOptions align)
    {
        GameObject go =
            UIBuilder.CreateTextSized(
                parent,
                value,
                size,
                TOBATheme.TextDark,
                align,
                area,
                pos
            );

        TextMeshProUGUI text =
            go.GetComponent<
                TextMeshProUGUI
            >();

        text.fontStyle =
            style;

        text.raycastTarget =
            false;

        return text;
    }

    void CreateLabel(
        Transform parent,
        string value,
        float y)
    {
        TextMeshProUGUI label =
            CreateText(
                parent,
                value,
                35,
                new Vector2(
                    700,
                    55
                ),
                new Vector2(
                    0,
                    y
                ),
                FontStyles.Bold,
                TextAlignmentOptions.Left
            );

        label.characterSpacing =
            .5f;
    }

    // ============================================================
    // INPUT
    // ============================================================

    TMP_InputField CreateInput(
        Transform parent,
        string objectName,
        float y)
    {
        Vector2 pos =
            new Vector2(
                0,
                y
            );

        Sprite frame =
            Load(
                "RegisterNameScreen/register_name_input_frame_v2"
            );

        if (frame != null)
        {
            GameObject frameGo =
                UIBuilder.CreateImage(
                    parent,
                    objectName + "_Frame",
                    frame,
                    new Vector2(
                        840,
                        145
                    ),
                    pos
                );

            Image frameImage =
                frameGo.GetComponent<Image>();

            if (frameImage != null)
            {
                frameImage.preserveAspect =
                    true;

                frameImage.raycastTarget =
                    false;
            }
        }

        GameObject viewportGo =
            new GameObject(
                objectName +
                "_Viewport"
            );

        viewportGo.transform.SetParent(
            parent,
            false
        );

        RectTransform viewport =
            viewportGo.AddComponent<
                RectTransform
            >();

        viewport.anchorMin =
            viewport.anchorMax =
            new Vector2(
                .5f,
                .5f
            );

        viewport.sizeDelta =
            new Vector2(
                VIEW_W,
                VIEW_H
            );

        viewport.anchoredPosition =
            pos;

        viewportGo.AddComponent<
            RectMask2D
        >();

        GameObject inputGo =
            new GameObject(
                objectName
            );

        inputGo.transform.SetParent(
            viewportGo.transform,
            false
        );

        RectTransform inputRect =
            inputGo.AddComponent<
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

        Image raycast =
            inputGo.AddComponent<Image>();

        raycast.sprite =
            UIBuilder.GetDefaultSprite();

        raycast.color =
            Color.clear;

        raycast.raycastTarget =
            true;

        TMP_InputField input =
            inputGo.AddComponent<
                TMP_InputField
            >();

        input.characterLimit =
            6;

        input.lineType =
            TMP_InputField.LineType.SingleLine;

        input.keyboardType =
            TouchScreenKeyboardType.NumberPad;

        input.shouldHideMobileInput =
            false;

        input.contentType =
            TMP_InputField.ContentType.Pin;

        input.caretBlinkRate =
            .85f;

        input.caretWidth =
            3;

        TMP_Text text =
            CreateInputText(
                inputGo.transform,
                "Text",
                31
            );

        TMP_Text placeholder =
            CreateInputText(
                inputGo.transform,
                "Placeholder",
                25
            );

        placeholder.text =
            "••••••";

        placeholder.fontStyle =
            FontStyles.Normal;

        placeholder.color =
            new Color(
                .35f,
                .30f,
                .23f,
                .70f
            );

        input.textComponent =
            text;

        input.placeholder =
            placeholder;

        input.textViewport =
            viewport;

        input.targetGraphic =
            raycast;

        RectTransform textRect =
            text.GetComponent<
                RectTransform
            >();

        input.onValueChanged.AddListener(
            _ =>
            {
                UpdateScroll(
                    input,
                    textRect
                );
            }
        );

        input.onSelect.AddListener(
            _ =>
            {
                UpdateScroll(
                    input,
                    textRect
                );
            }
        );

        input.onEndEdit.AddListener(
            _ =>
            {
                UpdateScroll(
                    input,
                    textRect
                );
            }
        );

        ScreenManager.Instance.StartCoroutine(
            MonitorScroll(
                input,
                textRect
            )
        );

        return input;
    }

    TMP_Text CreateInputText(
        Transform parent,
        string objectName,
        float fontSize)
    {
        GameObject go =
            new GameObject(
                objectName
            );

        go.transform.SetParent(
            parent,
            false
        );

        RectTransform rt =
            go.AddComponent<
                RectTransform
            >();

        rt.anchorMin =
            new Vector2(
                0,
                .5f
            );

        rt.anchorMax =
            new Vector2(
                0,
                .5f
            );

        rt.pivot =
            new Vector2(
                0,
                .5f
            );

        rt.sizeDelta =
            new Vector2(
                VIEW_W,
                VIEW_H
            );

        TextMeshProUGUI text =
            go.AddComponent<
                TextMeshProUGUI
            >();

        text.fontSize =
            fontSize;

        text.fontStyle =
            FontStyles.Bold;

        text.color =
            TOBATheme.TextDark;

        text.alignment =
            TextAlignmentOptions.MidlineLeft;

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        text.overflowMode =
            TextOverflowModes.Overflow;

        text.margin =
            new Vector4(
                PAD,
                0,
                PAD,
                0
            );

        text.raycastTarget =
            false;

        return text;
    }

    void UpdateScroll(
        TMP_InputField input,
        RectTransform textRect)
    {
        if (
            input == null ||
            textRect == null ||
            input.textComponent == null
        )
            return;

        TMP_Text text =
            input.textComponent;

        float contentWidth =
            string.IsNullOrEmpty(
                input.text
            )
                ? VIEW_W
                : Mathf.Max(
                    VIEW_W,
                    text.GetPreferredValues(
                        input.text,
                        2000,
                        VIEW_H
                    ).x +
                    PAD * 2f
                );

        textRect.sizeDelta =
            new Vector2(
                contentWidth,
                VIEW_H
            );

        text.ForceMeshUpdate();

        int count =
            text.textInfo.characterCount;

        int caret =
            Mathf.Clamp(
                input.caretPosition,
                0,
                input.text.Length
            );

        float caretX =
            PAD;

        if (
            count > 0 &&
            caret > 0
        )
        {
            int index =
                Mathf.Clamp(
                    caret - 1,
                    0,
                    count - 1
                );

            caretX =
                text.textInfo
                    .characterInfo[index]
                    .xAdvance +
                PAD;
        }

        float x =
            textRect.anchoredPosition.x;

        float visible =
            caretX + x;

        if (
            visible >
            VIEW_W - PAD
        )
        {
            x =
                VIEW_W -
                PAD -
                caretX;
        }

        if (
            visible <
            PAD
        )
        {
            x =
                PAD -
                caretX;
        }

        x =
            Mathf.Clamp(
                x,
                Mathf.Min(
                    0,
                    VIEW_W -
                    contentWidth
                ),
                0
            );

        textRect.anchoredPosition =
            new Vector2(
                x,
                0
            );
    }

    IEnumerator MonitorScroll(
        TMP_InputField input,
        RectTransform textRect)
    {
        while (
            screenObj != null &&
            input != null
        )
        {
            if (input.isFocused)
            {
                UpdateScroll(
                    input,
                    textRect
                );
            }

            yield return null;
        }
    }

    // ============================================================
    // BUTTON
    // PENTING:
    // pakai UIBuilder.CreateButton seperti kode asli
    // ============================================================

    void CreateButton()
    {
        btnDaftar =
            UIBuilder.CreateButton(
                screenObj.transform,
                "BtnDaftar",
                "BUAT AKUN",
                new Vector2(
                    800,
                    130
                ),
                TOBATheme.BtnPrimary,
                OnDaftarPressed
            );

        btnDaftar
            .GetComponent<RectTransform>()
            .anchoredPosition =
            new Vector2(
                0,
                -585
            );

        TextMeshProUGUI label =
            btnDaftar.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (label != null)
        {
            label.fontSize =
                40;

            label.fontStyle =
                FontStyles.Bold;

            label.characterSpacing =
                1.2f;

            label.raycastTarget =
                false;
        }

        Button button =
            btnDaftar.GetComponentInChildren<
                Button
            >();

        if (button != null)
        {
            button.onClick.AddListener(
                () =>
                {
                    ScreenManager.Instance.StartCoroutine(
                        AnimateButtonPress(
                            btnDaftar.transform
                        )
                    );
                }
            );
        }

        CreateLeaves(
            btnDaftar.transform
        );

        ScreenManager.Instance.StartCoroutine(
            FloatButton(
                btnDaftar.transform
            )
        );
    }

    // ============================================================
    // BUTTON LEAVES
    // ============================================================

    void CreateLeaves(
        Transform parent)
    {
        Sprite sprite =
            Load(
                "RegisterNameScreen/register_name_button_leaves"
            );

        if (sprite == null)
            return;

        GameObject left =
            UIBuilder.CreateImage(
                parent,
                "LeavesLeft",
                sprite,
                new Vector2(
                    190,
                    125
                ),
                new Vector2(
                    -380,
                    0
                )
            );

        GameObject right =
            UIBuilder.CreateImage(
                parent,
                "LeavesRight",
                sprite,
                new Vector2(
                    190,
                    125
                ),
                new Vector2(
                    380,
                    0
                )
            );

        Image leftImage =
            left.GetComponent<Image>();

        Image rightImage =
            right.GetComponent<Image>();

        if (leftImage != null)
        {
            leftImage.preserveAspect =
                true;

            leftImage.raycastTarget =
                false;
        }

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
                -1,
                1,
                1
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

        float t =
            phase;

        while (leaf != null)
        {
            t +=
                Time.deltaTime *
                .9f;

            float rotation =
                Mathf.Sin(t) *
                4f;

            float y =
                Mathf.Sin(
                    t * .7f
                ) *
                2.5f;

            float scale =
                1f +
                Mathf.Sin(
                    t * .55f
                ) *
                .025f;

            leaf.localEulerAngles =
                new Vector3(
                    0,
                    0,
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
                    1
                );

            yield return null;
        }
    }

    IEnumerator FloatButton(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                .8f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) * 2f,
                    origin.z
                );

            yield return null;
        }
    }


    IEnumerator AnimateButtonPress(Transform target)
    {
        if (target == null || screenObj == null)
            yield break;

        Vector3 original = target.localScale;

        target.localScale = original * 0.91f;

        yield return new WaitForSeconds(0.09f);

        // Pastikan objek masih tersedia setelah menunggu.
        if (target == null || screenObj == null)
            yield break;

        target.localScale = original * 1.04f;

        yield return new WaitForSeconds(0.07f);

        if (target == null || screenObj == null)
            yield break;

        target.localScale = original;
    }


    // ============================================================
    // STEP INDICATOR
    // ============================================================

    void CreateSteps()
    {
        GameObject root =
            UIBuilder.CreatePanel(
                screenObj.transform,
                "StepBg",
                Color.clear,
                new Vector2(.5f, .5f),
                new Vector2(.5f, .5f)
            );

        RectTransform rt =
            root.GetComponent<RectTransform>();

        rt.sizeDelta =
            new Vector2(
                300,
                70
            );

        rt.anchoredPosition =
            new Vector2(
                0,
                390
            );

        const float spacing =
            88f;

        for (
            int i = 0;
            i < 3;
            i++)
        {
            float x =
                (i - 1) *
                spacing;

            CreateText(
                root.transform,
                "◆",
                54,
                new Vector2(
                    62,
                    62
                ),
                new Vector2(
                    x,
                    0
                ),
                FontStyles.Bold,
                TextAlignmentOptions.Center
            );

            if (i < 2)
            {
                GameObject line =
                    UIBuilder.CreatePanelSized(
                        root.transform,
                        "Line_" + i,
                        TOBATheme.BatakGold,
                        new Vector2(
                            spacing - 30,
                            6
                        ),
                        new Vector2(
                            x +
                            spacing / 2,
                            0
                        )
                    );

                Image image =
                    line.GetComponent<Image>();

                if (image != null)
                    image.raycastTarget =
                        false;
            }
        }
    }

    // ============================================================
    // ENTRANCE
    // ============================================================

    IEnumerator AnimateEntrance()
    {
        Transform header =
            screenObj.transform.Find(
                "RegisterPinHeader"
            );

        Transform panel =
            screenObj.transform.Find(
                "FormCard"
            );

        Transform button =
            screenObj.transform.Find(
                "BtnDaftar"
            );

        if (header != null)
            header.localScale =
                Vector3.one * .92f;

        if (panel != null)
            panel.localScale =
                Vector3.one * .94f;

        if (button != null)
            button.localScale =
                Vector3.one * .92f;

        float t = 0f;

        while (t < .45f)
        {
            t +=
                Time.deltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t / .45f
                );

            if (header != null)
            {
                header.localScale =
                    Vector3.Lerp(
                        Vector3.one * .92f,
                        Vector3.one,
                        p
                    );
            }

            if (panel != null)
            {
                panel.localScale =
                    Vector3.Lerp(
                        Vector3.one * .94f,
                        Vector3.one,
                        p
                    );
            }

            if (button != null)
            {
                button.localScale =
                    Vector3.Lerp(
                        Vector3.one * .92f,
                        Vector3.one,
                        p
                    );
            }

            yield return null;
        }
    }

    // ============================================================
    // LOGIC REGISTRASI ASLI
    // ============================================================

    void OnDaftarPressed()
    {
        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        string pin =
            pinInput != null
                ? pinInput.text.Trim()
                : "";

        string pinConfirm =
            pinConfirmInput != null
                ? pinConfirmInput.text.Trim()
                : "";

        // ========================================================
        // VALIDASI PIN
        // ========================================================

        var pinCheck =
            ValidationHelper.ValidatePinMatch(
                pin,
                pinConfirm
            );

        if (!pinCheck.ok)
        {
            ShowStatus(
                pinCheck.error
            );

            return;
        }

        ShowStatus(
            "Membuat akun...",
            false
        );

        string name =
            PlayerPrefs.GetString(
                "RegisterName",
                ""
            );

        int characterID =
            PlayerPrefs.GetInt(
                "Temp_CharID",
                1
            );

        string deviceID =
            SystemInfo.deviceUniqueIdentifier;

        RegisterRequest req =
            new RegisterRequest
            {
                name = name,
                pin = pin,
                character_id = characterID,
                device_id = deviceID
            };

        APIManager.Instance.PostRequest(
            "/register",
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
                        1
                    );

                    PlayerPrefs.Save();

                    SoundManager.Instance.PlaySFX(
                        "sfx_win"
                    );

                    ScreenManager.Instance.ChangeScreen(
                        new RegisterSummaryScreen(
                            container,
                            res
                        )
                    );
                }
                else
                {
                    ShowStatus(
                        "Gagal membuat akun. Coba lagi."
                    );
                }
            },
            (err) =>
            {
                ShowStatus(
                    "Koneksi gagal. Cek internet."
                );
            }
        );
    }
    void ShowStatus(
        string message,
        bool isError = true)
    {
        if (statusTxt == null)
            return;

        statusTxt.text =
            message;

        statusTxt.color =
            isError
                ? TOBATheme.Error
                : TOBATheme.BatakGold;
    }
}