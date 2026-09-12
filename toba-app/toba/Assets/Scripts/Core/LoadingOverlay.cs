using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class LoadingOverlay
{
    private static GameObject _overlay;

    public static void Show(Transform canvas, string msg = "Memuat...")
    {
        if (_overlay != null) return;
        if (canvas == null) return;

        _overlay = new GameObject("LoadingOverlay");
        _overlay.transform.SetParent(canvas, false);
        _overlay.transform.SetAsLastSibling();

        // Background gelap — block semua input
        RectTransform rt = _overlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image bg = _overlay.AddComponent<Image>();
        bg.sprite = UIBuilder.GetDefaultSprite();
        bg.color  = new Color(0f, 0f, 0f, 0.68f);
        bg.raycastTarget = true;

        // Panel kayu
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(_overlay.transform, false);
        RectTransform rtP = panel.AddComponent<RectTransform>();
        rtP.anchorMin = rtP.anchorMax = new Vector2(0.5f, 0.5f);
        rtP.sizeDelta = new Vector2(560, 220);
        rtP.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        pImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        pImg.color  = new Color(0.22f, 0.10f, 0.03f, 0.98f);
        pImg.raycastTarget = false;
        Outline ol = panel.AddComponent<Outline>();
        ol.effectColor    = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(4, 4);

        // Spinner
        GameObject spinGO = new GameObject("Spinner");
        spinGO.transform.SetParent(panel.transform, false);
        RectTransform rtSp = spinGO.AddComponent<RectTransform>();
        rtSp.anchorMin = rtSp.anchorMax = new Vector2(0.5f, 0.5f);
        rtSp.sizeDelta = new Vector2(65, 65);
        rtSp.anchoredPosition = new Vector2(0, 42f);
        TextMeshProUGUI spinTxt = spinGO.AddComponent<TextMeshProUGUI>();
        spinTxt.text      = "◉";
        spinTxt.fontSize  = 55;
        spinTxt.color     = TOBATheme.BatakGold;
        spinTxt.alignment = TextAlignmentOptions.Center;
        spinTxt.raycastTarget = false;

        // Teks pesan
        TextMeshProUGUI msgTxt = UIBuilder.CreateTextSized(panel.transform,
            msg, 32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(500, 58), new Vector2(0, -38f))
            .GetComponent<TextMeshProUGUI>();
        msgTxt.fontStyle    = FontStyles.Normal;
        msgTxt.raycastTarget = false;

        // Animasi spinner
        if (ScreenManager.Instance != null)
            ScreenManager.Instance.StartCoroutine(
                AnimateSpin(spinGO.transform));
    }

    public static void Hide()
    {
        if (_overlay == null) return;
        Object.Destroy(_overlay);
        _overlay = null;
    }

    static IEnumerator AnimateSpin(Transform t)
    {
        while (t != null && _overlay != null)
        {
            if (t != null)
                t.localEulerAngles += new Vector3(0, 0, -200f * Time.deltaTime);
            yield return null;
        }
    }
}