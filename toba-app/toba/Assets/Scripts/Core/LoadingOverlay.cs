using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class LoadingOverlay
{
    private static GameObject _overlay;
    private static TextMeshProUGUI _messageText;

    public static void Show(Transform canvas, string msg = "Memuat...")
    {
        if (canvas == null)
            return;

        // Jika overlay masih berada pada canvas yang sama, cukup perbarui pesannya.
        if (_overlay != null && _overlay.transform.parent == canvas)
        {
            if (_messageText != null)
                _messageText.text = msg;
            return;
        }

        // Bersihkan overlay lama jika parent screen sudah berganti.
        if (_overlay != null)
            Object.Destroy(_overlay);

        _overlay = new GameObject("LoadingOverlay");
        _overlay.transform.SetParent(canvas, false);
        _overlay.transform.SetAsLastSibling();

        RectTransform rt = _overlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image bg = _overlay.AddComponent<Image>();
        bg.sprite = UIBuilder.GetDefaultSprite();
        bg.color = new Color(0f, 0f, 0f, 0.68f);
        bg.raycastTarget = true;

        Sprite woodPanel = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(_overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 220f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = woodPanel ?? UIBuilder.GetDefaultSprite();
        panelImage.type = woodPanel != null ? Image.Type.Sliced : Image.Type.Simple;
        panelImage.color = new Color(0.22f, 0.10f, 0.03f, 0.98f);
        panelImage.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4f, 4f);

        GameObject spinner = new GameObject("Spinner");
        spinner.transform.SetParent(panel.transform, false);

        RectTransform spinnerRect = spinner.AddComponent<RectTransform>();
        spinnerRect.anchorMin = spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.sizeDelta = new Vector2(65f, 65f);
        spinnerRect.anchoredPosition = new Vector2(0f, 42f);

        TextMeshProUGUI spinnerText = spinner.AddComponent<TextMeshProUGUI>();
        spinnerText.text = "◉";
        spinnerText.fontSize = 55f;
        spinnerText.color = TOBATheme.BatakGold;
        spinnerText.alignment = TextAlignmentOptions.Center;
        spinnerText.raycastTarget = false;

        GameObject messageObject = UIBuilder.CreateTextSized(
            panel.transform,
            msg,
            32,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(500f, 58f),
            new Vector2(0f, -38f)
        );
        _messageText = messageObject.GetComponent<TextMeshProUGUI>();
        if (_messageText != null)
        {
            _messageText.fontStyle = FontStyles.Normal;
            _messageText.raycastTarget = false;
        }

        if (ScreenManager.Instance != null)
            ScreenManager.Instance.StartCoroutine(AnimateSpin(spinner.transform));
    }

    public static void Hide()
    {
        if (_overlay != null)
            Object.Destroy(_overlay);

        _overlay = null;
        _messageText = null;
    }

    private static IEnumerator AnimateSpin(Transform spinner)
    {
        while (spinner != null && _overlay != null)
        {
            spinner.localEulerAngles += new Vector3(0f, 0f, -200f * Time.unscaledDeltaTime);
            yield return null;
        }
    }
}
