using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeaderBar
{
    public static GameObject Create(Transform parent, string title,
        System.Action onBack = null)
    {
        Sprite woodSprite = TOBATheme.LoadSprite("panel_wood");

        // Panel header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent, false);
        RectTransform rtH = header.AddComponent<RectTransform>();
        rtH.anchorMin = new Vector2(0, 0.88f);
        rtH.anchorMax = new Vector2(1, 1f);
        rtH.offsetMin = rtH.offsetMax = Vector2.zero;

        Image imgH = header.AddComponent<Image>();
        if (woodSprite != null)
        {
            imgH.sprite = woodSprite;
            imgH.type = Image.Type.Sliced;
            imgH.color = new Color(0.6f, 0.35f, 0.1f, 0.95f);
        }
        else
        {
            imgH.sprite = UIBuilder.GetDefaultSprite();
            imgH.color = TOBATheme.WoodDark;
        }
        imgH.raycastTarget = false;

        // Shadow bawah header
        Shadow headerShadow = header.AddComponent<Shadow>();
        headerShadow.effectColor = new Color(0, 0, 0, 0.6f);
        headerShadow.effectDistance = new Vector2(0, -4f);

        // Garis emas bawah
        GameObject goldLine = new GameObject("GoldLine");
        goldLine.transform.SetParent(parent, false);
        RectTransform rtLine = goldLine.AddComponent<RectTransform>();
        rtLine.anchorMin = new Vector2(0, 0.878f);
        rtLine.anchorMax = new Vector2(1, 0.883f);
        rtLine.offsetMin = rtLine.offsetMax = Vector2.zero;
        Image lineImg = goldLine.AddComponent<Image>();
        lineImg.sprite = UIBuilder.GetDefaultSprite();
        lineImg.color = TOBATheme.BatakGold;
        lineImg.raycastTarget = false;

        // Tombol Back
        if (onBack != null)
        {
            GameObject backBtn = UIBuilder.CreateSmallButton(
                header.transform, "BtnBack", "< KEMBALI",
                new Vector2(220, 72), TOBATheme.WoodMedium, TOBATheme.TextLight, onBack);
            RectTransform rtBack = backBtn.GetComponent<RectTransform>();
            rtBack.anchorMin = new Vector2(0, 0.5f);
            rtBack.anchorMax = new Vector2(0, 0.5f);
            rtBack.anchoredPosition = new Vector2(135f, 0);
        }

        // Judul
        GameObject titleObj = UIBuilder.CreateText(header.transform, title,
            50, TOBATheme.BatakGold, TextAlignmentOptions.Center);
        RectTransform rtTitle = titleObj.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(onBack != null ? 0.2f : 0f, 0f);
        rtTitle.anchorMax = Vector2.one;
        rtTitle.offsetMin = rtTitle.offsetMax = Vector2.zero;

        // Drop shadow teks judul
        TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        titleTmp.raycastTarget = false;
        titleTmp.fontStyle = FontStyles.Bold;

        return header;
    }
}