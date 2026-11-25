using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingScreen : BaseScreen
{
    public SettingScreen(Transform parent) : base(parent, "SettingScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "PENGATURAN", () =>
        {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        ScrollRect scroll = UIBuilder.CreateScrollView(
            screenObj.transform,
            "SettingScroll",
            new Vector2(0f, 0.08f),
            new Vector2(1f, 0.88f)
        );

        Transform content = scroll.content;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 1420f);

        CreateSectionHeader(content, "🔊 SUARA", 0f);

        float bgm = SoundManager.Instance != null ? SoundManager.Instance.GetBGMVolume() : 0.55f;
        float sfx = SoundManager.Instance != null ? SoundManager.Instance.GetSFXVolume() : 0.85f;
        bool muted = SoundManager.Instance != null && SoundManager.Instance.IsMuted();
        bool hapticEnabled = HapticManager.Instance == null || HapticManager.Instance.IsEnabled();

        CreateSliderRow(content, "Volume BGM", bgm, -130f,
            value => SoundManager.Instance?.SetBGMVolume(value));
        CreateSliderRow(content, "Volume SFX", sfx, -260f,
            value => SoundManager.Instance?.SetSFXVolume(value));

        CreateToggleRow(content, "Matikan Suara", muted, -390f,
            value => SoundManager.Instance?.SetMuted(value));
        CreateToggleRow(content, "Getaran", hapticEnabled, -520f,
            value => HapticManager.Instance?.SetEnabled(value));

        CreateSectionHeader(content, "ℹ INFORMASI", -650f);
        CreateInfoRow(content, "Versi Aplikasi", "v" + Application.version, -780f);
        CreateInfoRow(content, "Platform", Application.platform.ToString(), -870f);
        CreateInfoRow(content, "Developer", "TOBA Team", -960f);

        CreateSectionHeader(content, "📖 TENTANG", -1080f);

        GameObject aboutCard = UIBuilder.CreatePanelSized(
            content,
            "AboutCard",
            TOBATheme.CardBG,
            new Vector2(960f, 180f),
            new Vector2(0f, -1220f)
        );

        Outline outline = aboutCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3f, 3f);

        TextMeshProUGUI aboutText = UIBuilder.CreateTextSized(
            aboutCard.transform,
            "TOBA adalah game edukasi interaktif yang memperkenalkan kekayaan budaya Batak Toba kepada masyarakat luas melalui pendekatan gamifikasi.",
            28,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(900f, 160f),
            Vector2.zero
        ).GetComponent<TextMeshProUGUI>();

        aboutText.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(aboutText, true);
    }

    private void CreateSectionHeader(Transform parent, string title, float yPos)
    {
        GameObject header = UIBuilder.CreatePanelSized(
            parent,
            "SectionHeader_" + title,
            TOBATheme.WoodDark,
            new Vector2(1000f, 75f),
            new Vector2(0f, yPos)
        );

        UIBuilder.CreateTextSized(
            header.transform,
            title,
            36,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Left,
            new Vector2(920f, 60f),
            new Vector2(20f, 0f)
        );
    }

    private void CreateSliderRow(
        Transform parent,
        string label,
        float currentValue,
        float yPos,
        System.Action<float> onChange)
    {
        currentValue = Mathf.Clamp01(currentValue);

        GameObject row = UIBuilder.CreatePanelSized(
            parent,
            "SliderRow_" + label,
            TOBATheme.CardBG,
            new Vector2(1000f, 110f),
            new Vector2(0f, yPos)
        );

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = new Color(
            TOBATheme.BatakGold.r,
            TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b,
            0.3f
        );
        outline.effectDistance = new Vector2(2f, 2f);

        UIBuilder.CreateTextSized(
            row.transform,
            label,
            34,
            TOBATheme.TextDark,
            TextAlignmentOptions.Left,
            new Vector2(380f, 60f),
            new Vector2(-270f, 0f)
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        TextMeshProUGUI valueText = UIBuilder.CreateTextSized(
            row.transform,
            Mathf.RoundToInt(currentValue * 100f) + "%",
            30,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Right,
            new Vector2(100f, 55f),
            new Vector2(430f, 0f)
        ).GetComponent<TextMeshProUGUI>();

        // Root Slider menjadi satu-satunya target raycast.
        GameObject sliderObject = new GameObject("Slider_" + label);
        sliderObject.transform.SetParent(row.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(380f, 42f);
        sliderRect.anchoredPosition = new Vector2(80f, 0f);

        Image hitArea = sliderObject.AddComponent<Image>();
        hitArea.sprite = UIBuilder.GetDefaultSprite();
        hitArea.color = new Color(1f, 1f, 1f, 0.001f);
        hitArea.raycastTarget = true;

        // Track background.
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.35f);
        backgroundRect.anchorMax = new Vector2(1f, 0.65f);
        backgroundRect.offsetMin = new Vector2(14f, 0f);
        backgroundRect.offsetMax = new Vector2(-14f, 0f);

        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = UIBuilder.GetDefaultSprite();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.22f);
        backgroundImage.raycastTarget = false;

        // Fill area dan fill harus menjadi anak langsung dari root hierarchy Slider.
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
        fillAreaRect.offsetMin = new Vector2(14f, 0f);
        fillAreaRect.offsetMax = new Vector2(-14f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(currentValue, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = UIBuilder.GetDefaultSprite();
        fillImage.color = TOBATheme.BatakGold;
        fillImage.raycastTarget = false;

        // Handle area.
        GameObject handleArea = new GameObject("HandleArea");
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(14f, 0f);
        handleAreaRect.offsetMax = new Vector2(-14f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(currentValue, 0.5f);
        handleRect.sizeDelta = new Vector2(32f, 32f);
        handleRect.anchoredPosition = Vector2.zero;

        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = UIBuilder.GetDefaultSprite();
        handleImage.color = TOBATheme.BatakGold;
        handleImage.raycastTarget = false;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = true;
        slider.SetValueWithoutNotify(currentValue);

        slider.onValueChanged.AddListener(value =>
        {
            onChange?.Invoke(value);
            if (valueText != null)
                valueText.text = Mathf.RoundToInt(value * 100f) + "%";
        });
    }

    private void CreateToggleRow(
        Transform parent,
        string label,
        bool currentValue,
        float yPos,
        System.Action<bool> onChange)
    {
        GameObject row = UIBuilder.CreatePanelSized(
            parent,
            "ToggleRow_" + label,
            TOBATheme.CardBG,
            new Vector2(1000f, 110f),
            new Vector2(0f, yPos)
        );

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = new Color(
            TOBATheme.BatakGold.r,
            TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b,
            0.3f
        );
        outline.effectDistance = new Vector2(2f, 2f);

        UIBuilder.CreateTextSized(
            row.transform,
            label,
            34,
            TOBATheme.TextDark,
            TextAlignmentOptions.Left,
            new Vector2(700f, 60f),
            new Vector2(-80f, 0f)
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        bool state = currentValue;
        Color onColor = TOBATheme.BtnPrimary;
        Color offColor = TOBATheme.Locked;

        GameObject toggleButton = UIBuilder.CreateSmallButton(
            row.transform,
            "Toggle",
            state ? "ON" : "OFF",
            new Vector2(120f, 65f),
            state ? onColor : offColor,
            Color.white,
            () =>
            {
                state = !state;
                onChange?.Invoke(state);

                Image image = toggleButtonImage(row.transform);
                if (image != null)
                    image.color = state ? onColor : offColor;

                TextMeshProUGUI text = row.transform.Find("Toggle/Text_UI")
                    ?.GetComponent<TextMeshProUGUI>();
                if (text != null)
                    text.text = state ? "ON" : "OFF";
            }
        );

        toggleButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(400f, 0f);

        Image initialImage = toggleButton.GetComponent<Image>();
        if (initialImage != null)
            initialImage.color = state ? onColor : offColor;
    }

    private Image toggleButtonImage(Transform parent)
    {
        return parent.Find("Toggle")?.GetComponent<Image>();
    }

    private void CreateInfoRow(Transform parent, string label, string value, float yPos)
    {
        GameObject row = UIBuilder.CreatePanelSized(
            parent,
            "InfoRow_" + label,
            TOBATheme.CardBG,
            new Vector2(1000f, 80f),
            new Vector2(0f, yPos)
        );

        UIBuilder.CreateTextSized(
            row.transform,
            label,
            30,
            TOBATheme.TextGray,
            TextAlignmentOptions.Left,
            new Vector2(450f, 60f),
            new Vector2(-220f, 0f)
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        UIBuilder.CreateTextSized(
            row.transform,
            value,
            32,
            TOBATheme.TextDark,
            TextAlignmentOptions.Right,
            new Vector2(380f, 60f),
            new Vector2(260f, 0f)
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
    }
}
