using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChallengeComingSoonScreen : BaseScreen
{
    private string gameType;
    private ChallengeData challenge;

    private static readonly Dictionary<string, (string emoji, string name, string desc)>
        INFO = new()
    {
        ["FIND_OBJECT"] = ("<color=#F2E4C7><sprite name=\"search\" tint=1></color>", "Cari Benda Budaya",
            "Temukan benda budaya Batak yang tersembunyi\ndalam gambar!"),
        ["PUZZLE"]      = ("🧩", "Puzzle Gambar Budaya",
            "Susun kepingan puzzle menjadi\ngambar budaya Batak yang utuh!"),
        ["GUESS_SOUND"] = ("🔊", "Tebak Suara Budaya",
            "Dengarkan suara alat musik Batak\nlalu tebak namanya!"),
    };

    public ChallengeComingSoonScreen(Transform parent, ChallengeData ch, string type)
        : base(parent, "ChallengeComingSoon")
    {
        challenge = ch; gameType = type;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0,0.04f,0,0.6f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform, "TANTANGAN MINGGUAN", () =>
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));

        (string emoji, string name, string desc) info = INFO.ContainsKey(gameType)
            ? INFO[gameType]
            : ("🎮", gameType, "Mini-game ini segera hadir!");

        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = UIBuilder.CreateSpritePanel(screenObj.transform,
            "Panel", wp, new Vector2(900, 600), Vector2.zero);
        if (wp != null) panel.GetComponent<Image>().color =
            new Color(0.22f, 0.1f, 0.03f, 0.97f);
        panel.GetComponent<Outline>().effectColor = TOBATheme.BatakGold;

        UIBuilder.CreateTextSized(panel.transform,
            info.emoji, 120, Color.white, TextAlignmentOptions.Center,
            new Vector2(600, 150), new Vector2(0, 160f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;

        TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(panel.transform,
            info.name, 52, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(820, 75), new Vector2(0, 40f)).GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Bold; titleTmp.raycastTarget = false;

        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(panel.transform,
            info.desc, 32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(820, 100), new Vector2(0, -60f)).GetComponent<TextMeshProUGUI>();
        descTmp.fontStyle        = FontStyles.Normal;
        descTmp.textWrappingMode = TextWrappingModes.Normal;
        descTmp.raycastTarget    = false;

        UIBuilder.CreatePanelSized(panel.transform, "Line",
            TOBATheme.BatakGold, new Vector2(600, 3), new Vector2(0, -140f))
            .GetComponent<Image>().raycastTarget = false;

        TextMeshProUGUI soonTmp = UIBuilder.CreateTextSized(panel.transform,
            "⏳ Segera Hadir di Update Berikutnya", 28,
            new Color(1f,1f,1f,0.55f), TextAlignmentOptions.Center,
            new Vector2(820, 50), new Vector2(0, -195f)).GetComponent<TextMeshProUGUI>();
        soonTmp.fontStyle = FontStyles.Normal; soonTmp.raycastTarget = false;

        UIBuilder.CreateButton(screenObj.transform,
            "BtnBack", "KEMBALI KE MENU",
            new Vector2(700, 120), TOBATheme.BtnSecondary, () =>
                ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -600f);
    }
}