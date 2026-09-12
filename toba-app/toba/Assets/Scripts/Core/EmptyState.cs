using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class EmptyState
{
    public static GameObject Show(Transform parent, string emoji,
        string title, string subtitle = "",
        System.Action onRetry = null)
    {
        GameObject go = new GameObject("EmptyState");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.3f);
        rt.anchorMax = new Vector2(0.9f, 0.7f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Emoji
        UIBuilder.CreateTextSized(go.transform,
            emoji, 90, Color.white, TextAlignmentOptions.Center,
            new Vector2(600, 120), new Vector2(0, 80f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Judul
        TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(go.transform,
            title, 40, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 65), new Vector2(0, -10f))
            .GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle    = FontStyles.Bold;
        titleTmp.raycastTarget = false;

        // Subtitle
        if (!string.IsNullOrEmpty(subtitle))
        {
            TextMeshProUGUI subTmp = UIBuilder.CreateTextSized(go.transform,
                subtitle, 28, TOBATheme.TextGray, TextAlignmentOptions.Center,
                new Vector2(700, 55), new Vector2(0, -68f))
                .GetComponent<TextMeshProUGUI>();
            subTmp.fontStyle        = FontStyles.Normal;
            subTmp.textWrappingMode = TextWrappingModes.Normal;
            subTmp.raycastTarget    = false;
        }

        // Tombol retry
        if (onRetry != null)
        {
            UIBuilder.CreateButton(go.transform,
                "BtnRetry", "🔄 Coba Lagi",
                new Vector2(380, 100), TOBATheme.BtnPrimary, onRetry)
                .GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, -150f);
        }

        return go;
    }
}