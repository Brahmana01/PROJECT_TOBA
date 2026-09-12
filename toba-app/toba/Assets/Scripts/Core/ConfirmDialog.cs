using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class ConfirmDialog
{
    public static void Show(Transform canvas, string title, string message,
        string confirmText, System.Action onConfirm,
        string cancelText = "Batal")
    {
        if (ScreenManager.Instance == null) return;
        ScreenManager.Instance.StartCoroutine(
            ShowCoroutine(canvas, title, message,
                confirmText, onConfirm, cancelText));
    }

    static IEnumerator ShowCoroutine(Transform canvas, string title,
        string message, string confirmText,
        System.Action onConfirm, string cancelText)
    {
        // Overlay
        GameObject overlay = new GameObject("ConfirmOverlay");
        overlay.transform.SetParent(canvas, false);
        overlay.transform.SetAsLastSibling();
        RectTransform rtO = overlay.AddComponent<RectTransform>();
        rtO.anchorMin = Vector2.zero;
        rtO.anchorMax = Vector2.one;
        rtO.offsetMin = rtO.offsetMax = Vector2.zero;
        Image bgImg = overlay.AddComponent<Image>();
        bgImg.sprite = UIBuilder.GetDefaultSprite();
        bgImg.color  = new Color(0, 0, 0, 0.72f);
        bgImg.raycastTarget = true;

        // Panel
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        panel.transform.localScale = Vector3.zero;
        RectTransform rtP = panel.AddComponent<RectTransform>();
        rtP.anchorMin = rtP.anchorMax = new Vector2(0.5f, 0.5f);
        rtP.sizeDelta = new Vector2(860, 460);
        rtP.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        pImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        pImg.color  = new Color(0.22f, 0.10f, 0.03f, 0.99f);
        pImg.raycastTarget = false;
        Outline ol = panel.AddComponent<Outline>();
        ol.effectColor    = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(5, 5);

        // Scale in
        yield return UIBuilder.AnimateScaleIn(panel.transform, 0.2f);

        // Judul
        TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(panel.transform,
            title, 48, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(800, 70), new Vector2(0, 145f))
            .GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.raycastTarget = false;

        // Pesan
        TextMeshProUGUI msgTmp = UIBuilder.CreateTextSized(panel.transform,
            message, 32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(780, 100), new Vector2(0, 30f))
            .GetComponent<TextMeshProUGUI>();
        msgTmp.fontStyle        = FontStyles.Normal;
        msgTmp.textWrappingMode = TextWrappingModes.Normal;
        msgTmp.raycastTarget    = false;

        // Tombol Konfirmasi
        UIBuilder.CreateButton(panel.transform,
            "BtnConfirm", confirmText,
            new Vector2(340, 110), TOBATheme.Error, () => {
                Object.Destroy(overlay);
                onConfirm?.Invoke();
            }).GetComponent<RectTransform>().anchoredPosition =
                new Vector2(-195f, -140f);

        // Tombol Batal
        UIBuilder.CreateButton(panel.transform,
            "BtnCancel", cancelText,
            new Vector2(340, 110), TOBATheme.BtnPrimary, () => {
                Object.Destroy(overlay);
            }).GetComponent<RectTransform>().anchoredPosition =
                new Vector2(195f, -140f);
    }
}