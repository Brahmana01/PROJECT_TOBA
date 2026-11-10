using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class AchievementPopup
{
    public static void Show(Transform parent, AchievementData achievement)
    {
        if (ScreenManager.Instance != null)
            ScreenManager.Instance.StartCoroutine(ShowCoroutine(parent, achievement));
    }

    static IEnumerator ShowCoroutine(Transform parent, AchievementData achievement)
    {
        // Overlay gelap
        GameObject overlay = new GameObject("AchievementOverlay");
        overlay.transform.SetParent(parent, false);
        RectTransform rtO = overlay.AddComponent<RectTransform>();
        rtO.anchorMin = Vector2.zero;
        rtO.anchorMax = Vector2.one;
        rtO.offsetMin = rtO.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        // Card achievement
        GameObject card = UIBuilder.CreatePanelSized(overlay.transform,
            "AchCard", TOBATheme.WoodDark, new Vector2(860, 600), Vector2.zero);
        card.transform.localScale = Vector3.zero;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(5, 5);

        // Header banner
        GameObject banner = UIBuilder.CreatePanelSized(card.transform,
            "Banner", TOBATheme.BatakGold, new Vector2(860, 100), new Vector2(0, 240f));

        UIBuilder.CreateTextSized(banner.transform,
            "<b><color=#D6A84B>♛</color></b> ACHIEVEMENT DIBUKA!", 38, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(820, 80), Vector2.zero);

        // Icon achievement
        UIBuilder.CreateTextSized(card.transform,
            string.IsNullOrEmpty(achievement.icon) ? "<b><color=#D6A84B>★</color></b>" : achievement.icon,
            100, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(200, 140), new Vector2(0, 100f));

        // Nama achievement
        UIBuilder.CreateTextSized(card.transform,
            achievement.name, 48, Color.white, TextAlignmentOptions.Center,
            new Vector2(800, 70), new Vector2(0, -20f));

        // Deskripsi
        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(card.transform,
            achievement.description, 32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(780, 70), new Vector2(0, -90f))
            .GetComponent<TextMeshProUGUI>();
        descTmp.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(descTmp, true);

        // Reward
        string rewardStr = "";
        if (achievement.reward_xp > 0)   rewardStr += "✨ +" + achievement.reward_xp + " XP  ";
        if (achievement.reward_stars > 0) rewardStr += "<b><color=#D6A84B>★</color></b> +" + achievement.reward_stars;

        if (!string.IsNullOrEmpty(rewardStr))
        {
            UIBuilder.CreateTextSized(card.transform,
                rewardStr.Trim(), 36, TOBATheme.BatakGold, TextAlignmentOptions.Center,
                new Vector2(800, 55), new Vector2(0, -165f));
        }

        // Tombol klaim
        GameObject btnKlaim = UIBuilder.CreateButton(card.transform,
            "BtnKlaim", "KLAIM REWARD",
            new Vector2(600, 120), TOBATheme.BtnPrimary, () => {
                ClaimAndClose(overlay, achievement);
            });
        btnKlaim.GetComponent<RectTransform>().anchorMin =
            btnKlaim.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnKlaim.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250f);

        // Animasi masuk
        yield return UIBuilder.AnimateScaleIn(card.transform, 0.3f);
    }

    static void ClaimAndClose(GameObject overlay, AchievementData achievement)
    {
        int playerID = PlayerPrefs.GetInt("PlayerID");

        ClaimAchievementRequest req = new ClaimAchievementRequest
        {
            player_id      = playerID,
            achievement_id = achievement.id
        };

        APIManager.Instance.PostRequest("/player/achievement/claim",
            JsonUtility.ToJson(req), (response) => {
                if (overlay != null) Object.Destroy(overlay);
            }, (err) => {
                if (overlay != null) Object.Destroy(overlay);
            });
    }
}