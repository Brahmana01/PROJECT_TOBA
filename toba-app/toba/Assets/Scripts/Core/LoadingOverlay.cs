using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class LoadingOverlay
{
    private static GameObject _overlay;
    private static TextMeshProUGUI _messageText;
    private static Transform _owner;

    public static void Show(Transform canvas, string msg = "Memuat...")
    {
        if (canvas == null)
            return;

        if (_overlay != null && _overlay.transform.parent == canvas)
        {
            _owner = canvas;
            if (_messageText != null)
                _messageText.text = msg;
            return;
        }

        Hide();

        _owner = canvas;
        _overlay = new GameObject("LoadingOverlay");
        _overlay.transform.SetParent(canvas, false);
        _overlay.transform.SetAsLastSibling();

        RectTransform rt = _overlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = _overlay.AddComponent<Image>();
        bg.sprite = UIBuilder.GetDefaultSprite();
        bg.color = new Color(0f, 0f, 0f, 0.68f);
        bg.raycastTarget = true;

        Sprite woodPanel = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(_overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(650f, 250f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = woodPanel ?? UIBuilder.GetDefaultSprite();
        panelImage.type = woodPanel != null ? Image.Type.Sliced : Image.Type.Simple;
        panelImage.color = new Color(0.22f, 0.10f, 0.03f, 0.98f);
        panelImage.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4f, 4f);

        GameObject spinner = UIBuilder.CreateTextSized(
            panel.transform,
            "◌",
            72,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(78f, 78f),
            new Vector2(0f, 48f)
        );
        spinner.name = "Spinner";
        TextMeshProUGUI spinnerText = spinner.GetComponent<TextMeshProUGUI>();
        if (spinnerText != null)
            spinnerText.raycastTarget = false;

        GameObject messageObject = UIBuilder.CreateTextSized(
            panel.transform,
            msg,
            30,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(590f, 72f),
            new Vector2(0f, -48f)
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

    public static void Hide(Transform owner)
    {
        if (owner == null || _owner != owner)
            return;

        Hide();
    }

    public static void Hide()
    {
        if (_overlay != null)
            Object.Destroy(_overlay);

        _overlay = null;
        _messageText = null;
        _owner = null;
    }

    private static IEnumerator AnimateSpin(Transform spinner)
    {
        while (spinner != null && _overlay != null)
        {
            spinner.localEulerAngles += new Vector3(
                0f, 0f, -240f * Time.unscaledDeltaTime
            );
            yield return null;
        }
    }
}
