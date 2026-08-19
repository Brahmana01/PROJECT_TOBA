using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class UIBuilder
{
    // ── DEFAULT SPRITE ────────────────────────────────────
    private static Sprite _defaultSprite;
    public static Sprite GetDefaultSprite()
    {
        if (_defaultSprite != null) return _defaultSprite;
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        _defaultSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        return _defaultSprite;
    }

    // ── IMAGE DENGAN SPRITE ───────────────────────────────
    public static GameObject CreateImage(Transform parent, string name,
        Sprite sprite, Vector2 size, Vector2 pos,
        Color? tint = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = sprite ?? GetDefaultSprite();
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = tint ?? Color.white;
        img.raycastTarget = false;
        return go;
    }

    // ── PANEL ─────────────────────────────────────────────
    public static GameObject CreatePanel(Transform parent, string name,
        Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.sprite = GetDefaultSprite();
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        return panel;
    }

    public static GameObject CreatePanelSized(Transform parent, string name,
        Color color, Vector2 size, Vector2 pos)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        Image img = panel.AddComponent<Image>();
        img.sprite = GetDefaultSprite();
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        Shadow sh = panel.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.4f);
        sh.effectDistance = new Vector2(4, -4);
        return panel;
    }

    // ── PANEL DENGAN SPRITE (panel_wood, dll) ─────────────
    public static GameObject CreateSpritePanel(Transform parent, string name,
        Sprite sprite, Vector2 size, Vector2 pos, Image.Type imgType = Image.Type.Sliced)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = sprite ?? GetDefaultSprite();
        img.type = sprite != null ? imgType : Image.Type.Simple;
        img.color = Color.white;
        img.raycastTarget = false;
        return go;
    }

    // ── BACKGROUND FULL SCREEN ────────────────────────────
    public static GameObject CreateBackground(Transform parent, Sprite bgSprite)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        bg.transform.SetAsFirstSibling(); // selalu di belakang
        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = bg.AddComponent<Image>();
        img.sprite = bgSprite ?? GetDefaultSprite();
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = Color.white;
        img.raycastTarget = false;
        return bg;
    }

    // ── TEKS ──────────────────────────────────────────────
    public static GameObject CreateText(Transform parent, string content,
        int fontSize, Color color, TextAlignmentOptions align)
    {
        GameObject textObj = new GameObject("Text_UI");
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(900, 200);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return textObj;
    }

    public static GameObject CreateTextSized(Transform parent, string content,
        int fontSize, Color color, TextAlignmentOptions align,
        Vector2 size, Vector2 pos)
    {
        GameObject obj = CreateText(parent, content, fontSize, color, align);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        return obj;
    }

    // ── INPUT FIELD ───────────────────────────────────────
    public static TMP_InputField CreateInputField(Transform parent, string name,
        string placeholderText, bool isPassword)
    {
        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");

        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);
        RectTransform rtInput = inputObj.AddComponent<RectTransform>();
        rtInput.sizeDelta = new Vector2(850, 130);
        rtInput.anchorMin = rtInput.anchorMax = new Vector2(0.5f, 0.5f);

        Image img = inputObj.AddComponent<Image>();
        img.sprite = panelSprite ?? GetDefaultSprite();
        img.type = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color = panelSprite != null ? Color.white : TOBATheme.CardBG;
        img.raycastTarget = true;

        Outline outline = inputObj.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform rtArea = textArea.AddComponent<RectTransform>();
        rtArea.anchorMin = Vector2.zero;
        rtArea.anchorMax = Vector2.one;
        rtArea.offsetMin = new Vector2(30, 8);
        rtArea.offsetMax = new Vector2(-30, -8);
        textArea.AddComponent<RectMask2D>();

        GameObject textMainObj = CreateText(textArea.transform, "", 45,
            TOBATheme.TextDark, TextAlignmentOptions.Left);
        TextMeshProUGUI textMain = textMainObj.GetComponent<TextMeshProUGUI>();
        textMain.fontStyle = FontStyles.Normal;
        textMain.raycastTarget = false;

        GameObject placeholderObj = CreateText(textArea.transform, placeholderText, 40,
            TOBATheme.TextGray, TextAlignmentOptions.Left);
        TextMeshProUGUI placeholder = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.raycastTarget = false;

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textComponent = textMain;
        inputField.placeholder = placeholder;
        inputField.textViewport = rtArea;
        if (isPassword) inputField.contentType = TMP_InputField.ContentType.Pin;

        return inputField;
    }

    // ── TOMBOL EMAS (pakai btn_gold sprite) ───────────────
    public static GameObject CreateButton(Transform parent, string name,
        string label, Vector2 size, Color btnColor, System.Action onClick)
    {
        Sprite btnSprite = TOBATheme.LoadSprite("btn_gold");

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        Image img = btnObj.AddComponent<Image>();
        if (btnSprite != null)
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = GetDefaultSprite();
            img.type = Image.Type.Simple;
            img.color = btnColor;
        }
        img.raycastTarget = true;

        Shadow sh = btnObj.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.5f);
        sh.effectDistance = new Vector2(0, -5);

        // Label
        Color labelColor = btnSprite != null ? TOBATheme.TextDark : TOBATheme.TextLight;
        GameObject txtObj = CreateText(btnObj.transform, label, 44,
            labelColor, TextAlignmentOptions.Center);
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.sizeDelta = size;
        rtTxt.anchoredPosition = Vector2.zero;
        rtTxt.anchorMin = rtTxt.anchorMax = new Vector2(0.5f, 0.5f);
        txtObj.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor    = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 0.8f, 1f);
        cb.pressedColor   = new Color(0.8f, 0.8f, 0.6f, 1f);
        btn.colors = cb;
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => {
            if (ScreenManager.Instance != null)
                ScreenManager.Instance.StartCoroutine(AnimateBounce(btnObj.transform));
            onClick?.Invoke();
        });

        return btnObj;
    }

    // ── TOMBOL KECIL ──────────────────────────────────────
    public static GameObject CreateSmallButton(Transform parent, string name,
        string label, Vector2 size, Color btnColor, Color textColor,
        System.Action onClick)
    {
        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        Image img = btnObj.AddComponent<Image>();
        if (panelSprite != null && btnColor != Color.clear)
        {
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = GetDefaultSprite();
            img.type = Image.Type.Simple;
            img.color = btnColor;
        }
        img.raycastTarget = true;

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2, 2);

        GameObject txtObj = CreateText(btnObj.transform, label, 34,
            textColor, TextAlignmentOptions.Center);
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.sizeDelta = size;
        rtTxt.anchoredPosition = Vector2.zero;
        rtTxt.anchorMin = rtTxt.anchorMax = new Vector2(0.5f, 0.5f);
        txtObj.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => {
            if (ScreenManager.Instance != null)
                ScreenManager.Instance.StartCoroutine(AnimateBounce(btnObj.transform));
            onClick?.Invoke();
        });

        return btnObj;
    }

    // ── SCROLL VIEW ───────────────────────────────────────
    public static ScrollRect CreateScrollView(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject scrollGO = new GameObject(name);
        scrollGO.transform.SetParent(parent, false);
        RectTransform rtScroll = scrollGO.AddComponent<RectTransform>();
        rtScroll.anchorMin = anchorMin;
        rtScroll.anchorMax = anchorMax;
        rtScroll.offsetMin = rtScroll.offsetMax = Vector2.zero;
        Image scrollImg = scrollGO.AddComponent<Image>();
        scrollImg.color = new Color(0, 0, 0, 0.005f);
        scrollImg.sprite = GetDefaultSprite();
        scrollImg.raycastTarget = true;

        GameObject vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        RectTransform rtVP = vpGO.AddComponent<RectTransform>();
        rtVP.anchorMin = Vector2.zero;
        rtVP.anchorMax = Vector2.one;
        rtVP.offsetMin = rtVP.offsetMax = Vector2.zero;
        Image vpImg = vpGO.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0.005f);
        vpImg.sprite = GetDefaultSprite();
        Mask mask = vpGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        RectTransform rtContent = contentGO.AddComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0f, 1f);
        rtContent.anchorMax = new Vector2(1f, 1f);
        rtContent.pivot = new Vector2(0.5f, 1f);
        rtContent.anchoredPosition = Vector2.zero;
        rtContent.offsetMin = rtContent.offsetMax = Vector2.zero;
        rtContent.sizeDelta = new Vector2(0, 100f);

        ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
        sr.viewport = rtVP;
        sr.content = rtContent;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.inertia = true;
        sr.decelerationRate = 0.135f;
        sr.scrollSensitivity = 40f;

        return sr;
    }

    // ── ANIMASI ───────────────────────────────────────────
    public static IEnumerator AnimateBounce(Transform t)
    {
        if (t == null) yield break;
        Vector3 orig = t.localScale;
        t.localScale = orig * 0.90f;
        yield return new WaitForSecondsRealtime(0.07f);
        if (t != null) t.localScale = orig * 1.08f;
        yield return new WaitForSecondsRealtime(0.06f);
        if (t != null) t.localScale = orig;
    }

    public static IEnumerator AnimateFadeIn(CanvasGroup cg, float duration = 0.35f)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public static IEnumerator AnimateScaleIn(Transform t, float duration = 0.25f)
    {
        if (t == null) yield break;
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            if (t != null) t.localScale = Vector3.one * scale;
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }

    public static void SetWordWrap(TextMeshProUGUI tmp, bool enabled)
    {
        tmp.textWrappingMode = enabled
            ? TextWrappingModes.Normal
            : TextWrappingModes.NoWrap;
    }
}