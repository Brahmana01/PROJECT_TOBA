using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class LevelUpPopup
{
    public static void Show(Transform parent, int newLevel, string newTitle)
    {
        if (ScreenManager.Instance != null)
            ScreenManager.Instance.StartCoroutine(ShowCoroutine(parent, newLevel, newTitle));
    }

    static IEnumerator ShowCoroutine(Transform parent, int newLevel, string newTitle)
    {
        // Overlay
        GameObject overlay = new GameObject("LevelUpOverlay");
        overlay.transform.SetParent(parent, false);
        RectTransform rtO = overlay.AddComponent<RectTransform>();
        rtO.anchorMin = Vector2.zero;
        rtO.anchorMax = Vector2.one;
        rtO.offsetMin = rtO.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        // Card
        GameObject card = UIBuilder.CreatePanelSized(overlay.transform,
            "LevelCard", TOBATheme.WoodDark, new Vector2(800, 700), Vector2.zero);
        card.transform.localScale = Vector3.zero;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(5, 5);

        // Banner
        GameObject banner = UIBuilder.CreatePanelSized(card.transform,
            "Banner", TOBATheme.BtnPrimary, new Vector2(800, 100), new Vector2(0, 280f));

        UIBuilder.CreateTextSized(banner.transform,
            "🎉 LEVEL NAIK!", 42, Color.white, TextAlignmentOptions.Center,
            new Vector2(760, 80), Vector2.zero);

        // Level besar
        UIBuilder.CreateTextSized(card.transform,
            newLevel.ToString(), 160, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(300, 200), new Vector2(0, 90f));

        // Gelar baru
        if (!string.IsNullOrEmpty(newTitle))
        {
            UIBuilder.CreateTextSized(card.transform,
                "Gelar baru :", 30, TOBATheme.TextGray, TextAlignmentOptions.Center,
                new Vector2(700, 45), new Vector2(0, -80f))
                .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

            UIBuilder.CreateTextSized(card.transform,
                newTitle, 48, TOBATheme.BatakGold, TextAlignmentOptions.Center,
                new Vector2(720, 65), new Vector2(0, -140f));
        }

        // Reward info
        UIBuilder.CreateTextSized(card.transform,
            "✨ +50 Bonus XP", 36, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 55), new Vector2(0, -215f));

        // Tombol OK
        GameObject btnOk = UIBuilder.CreateButton(card.transform,
            "BtnOk", "LANJUTKAN!",
            new Vector2(580, 120), TOBATheme.BatakGold, () => {
                Object.Destroy(overlay);
            });
        btnOk.GetComponent<RectTransform>().anchorMin =
            btnOk.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnOk.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -310f);

        // Label OK color override (teks gelap di atas emas)
        TextMeshProUGUI btnLabel = btnOk.GetComponentInChildren<TextMeshProUGUI>();
        if (btnLabel != null) btnLabel.color = TOBATheme.WoodDark;

        // Animasi
        yield return UIBuilder.AnimateScaleIn(card.transform, 0.3f);

        // Confetti (teks unicode sederhana)
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            SpawnConfetti(overlay.transform);
        }
    }

    static void SpawnConfetti(Transform parent)
    {
        string[] confetti = { "✨", "⭐", "🎊", "🎉", "💛" };
        System.Random rng = new System.Random();

        GameObject c = new GameObject("Confetti");
        c.transform.SetParent(parent, false);
        RectTransform rt = c.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(60, 60);
        rt.anchoredPosition = new Vector2(
            rng.Next(-450, 450),
            rng.Next(200, 900));

        TextMeshProUGUI tmp = c.AddComponent<TextMeshProUGUI>();
        tmp.text = confetti[rng.Next(confetti.Length)];
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        Object.Destroy(c, 2f);
    }
}