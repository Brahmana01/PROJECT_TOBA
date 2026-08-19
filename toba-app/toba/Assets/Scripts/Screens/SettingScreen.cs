using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingScreen : BaseScreen
{
    public SettingScreen(Transform parent) : base(parent, "SettingScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "PENGATURAN", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        ScrollRect scroll = UIBuilder.CreateScrollView(screenObj.transform,
            "SettingScroll", new Vector2(0, 0.08f), new Vector2(1, 0.88f));

        Transform content = scroll.content;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1200f);

        // ── SUARA ─────────────────────────────────────────
        CreateSectionHeader(content, "🔊 SUARA", 0f);

        // BGM Volume
        CreateSliderRow(content, "Volume BGM",
            SoundManager.Instance.GetBGMVolume(), -130f,
            (val) => SoundManager.Instance.SetBGMVolume(val));

        // SFX Volume
        CreateSliderRow(content, "Volume SFX",
            SoundManager.Instance.GetSFXVolume(), -260f,
            (val) => SoundManager.Instance.SetSFXVolume(val));

        // Mute toggle
        CreateToggleRow(content, "Matikan Suara",
            SoundManager.Instance.IsMuted(), -390f,
            (val) => SoundManager.Instance.ToggleMute());

        // ── INFORMASI ─────────────────────────────────────
        CreateSectionHeader(content, "ℹ INFORMASI", -520f);

        CreateInfoRow(content, "Versi Aplikasi", "v1.0.0", -650f);
        CreateInfoRow(content, "Platform",        "Android", -740f);
        CreateInfoRow(content, "Developer",       "TOBA Team", -830f);

        // ── TENTANG ───────────────────────────────────────
        CreateSectionHeader(content, "📖 TENTANG", -950f);

        GameObject aboutCard = UIBuilder.CreatePanelSized(content, "AboutCard",
            TOBATheme.CardBG, new Vector2(960, 180), new Vector2(0, -1090f));

        Outline outline = aboutCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        TextMeshProUGUI aboutTmp = UIBuilder.CreateTextSized(aboutCard.transform,
            "TOBA adalah game edukasi interaktif yang memperkenalkan kekayaan budaya Batak Toba kepada masyarakat luas melalui pendekatan gamifikasi.",
            28, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(900, 160), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        aboutTmp.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(aboutTmp, true);
    }

    void CreateSectionHeader(Transform parent, string title, float yPos)
    {
        GameObject header = UIBuilder.CreatePanelSized(parent, "SectionHeader_" + title,
            TOBATheme.WoodDark, new Vector2(1000, 75), new Vector2(0, yPos));

        UIBuilder.CreateTextSized(header.transform,
            title, 36, TOBATheme.BatakGold, TextAlignmentOptions.Left,
            new Vector2(920, 60), new Vector2(20f, 0));
    }

    void CreateSliderRow(Transform parent, string label, float currentValue,
        float yPos, System.Action<float> onChange)
    {
        GameObject row = UIBuilder.CreatePanelSized(parent, "SliderRow_" + label,
            TOBATheme.CardBG, new Vector2(1000, 110), new Vector2(0, yPos));

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = new Color(TOBATheme.BatakGold.r, TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b, 0.3f);
        outline.effectDistance = new Vector2(2, 2);

        // Label
        UIBuilder.CreateTextSized(row.transform,
            label, 34, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(380, 60), new Vector2(-270f, 0))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Value text
        TextMeshProUGUI valTxt = UIBuilder.CreateTextSized(row.transform,
            Mathf.RoundToInt(currentValue * 100) + "%",
            30, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(100, 55), new Vector2(430f, 0))
            .GetComponent<TextMeshProUGUI>();

        // Slider
        GameObject sliderObj = new GameObject("Slider_" + label);
        sliderObj.transform.SetParent(row.transform, false);
        RectTransform rtSlider = sliderObj.AddComponent<RectTransform>();
        rtSlider.anchorMin = rtSlider.anchorMax = new Vector2(0.5f, 0.5f);
        rtSlider.sizeDelta = new Vector2(380, 30);
        rtSlider.anchoredPosition = new Vector2(80f, 0);

        // Slider background
        GameObject sliderBg = UIBuilder.CreatePanelSized(sliderObj.transform,
            "BG", new Color(0, 0, 0, 0.2f), new Vector2(380, 12), Vector2.zero);

        // Slider fill
        GameObject sliderFill = UIBuilder.CreatePanel(sliderBg.transform, "Fill",
            TOBATheme.BatakGold, Vector2.zero, new Vector2(currentValue, 1f));

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = currentValue;

        slider.onValueChanged.AddListener((val) => {
            onChange?.Invoke(val);
            if (valTxt != null)
                valTxt.text = Mathf.RoundToInt(val * 100) + "%";

            RectTransform rtFill = sliderFill.GetComponent<RectTransform>();
            if (rtFill != null)
            {
                rtFill.anchorMax = new Vector2(val, 1f);
                rtFill.offsetMin = rtFill.offsetMax = Vector2.zero;
            }
        });
    }

    void CreateToggleRow(Transform parent, string label, bool currentValue,
        float yPos, System.Action<bool> onChange)
    {
        GameObject row = UIBuilder.CreatePanelSized(parent, "ToggleRow_" + label,
            TOBATheme.CardBG, new Vector2(1000, 110), new Vector2(0, yPos));

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = new Color(TOBATheme.BatakGold.r, TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b, 0.3f);
        outline.effectDistance = new Vector2(2, 2);

        UIBuilder.CreateTextSized(row.transform,
            label, 34, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(700, 60), new Vector2(-80f, 0))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Toggle button
        bool[] state = { currentValue };
        Color onColor  = TOBATheme.BtnPrimary;
        Color offColor = TOBATheme.Locked;

        GameObject toggleBtn = UIBuilder.CreateSmallButton(row.transform,
            "Toggle", currentValue ? "ON" : "OFF",
            new Vector2(120, 65),
            currentValue ? onColor : offColor,
            Color.white, () => {
                state[0] = !state[0];
                onChange?.Invoke(state[0]);
                // Update visual
                Image img = row.transform.Find("Toggle")?.GetComponent<Image>();
                if (img != null) img.color = state[0] ? onColor : offColor;
                TextMeshProUGUI txt = row.transform.Find("Toggle/Text_UI")
                    ?.GetComponent<TextMeshProUGUI>();
                if (txt != null) txt.text = state[0] ? "ON" : "OFF";
            });
        toggleBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(400f, 0);
    }

    void CreateInfoRow(Transform parent, string label, string value, float yPos)
    {
        GameObject row = UIBuilder.CreatePanelSized(parent, "InfoRow_" + label,
            TOBATheme.CardBG, new Vector2(1000, 80), new Vector2(0, yPos));

        UIBuilder.CreateTextSized(row.transform,
            label, 30, TOBATheme.TextGray, TextAlignmentOptions.Left,
            new Vector2(450, 60), new Vector2(-220f, 0))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        UIBuilder.CreateTextSized(row.transform,
            value, 32, TOBATheme.TextDark, TextAlignmentOptions.Right,
            new Vector2(380, 60), new Vector2(260f, 0))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
    }
}