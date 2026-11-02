using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeaderBar
{
    // Tinggi header dalam satuan Canvas. Satu angka ini dipakai bersama
    // oleh layar lain agar tidak ada angka 230 yang tersebar.
    public const float Height = 230f;

    // Tebal garis emas, dan seberapa jauh garis masuk ke dalam header.
    const float GoldLineThickness = 10f;
    const float GoldLineInset = 6f;

    public static GameObject Create(Transform parent, string title,
        System.Action onBack = null)
    {
        Sprite woodSprite = TOBATheme.LoadSprite("panel_wood");

        // Panel header
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent, false);
        RectTransform rtH = header.AddComponent<RectTransform>();

        // Urutan: anchor, pivot, lalu sizeDelta dan anchoredPosition PALING AKHIR.
        rtH.anchorMin = new Vector2(0f, 1f);
        rtH.anchorMax = new Vector2(1f, 1f);
        rtH.pivot = new Vector2(0.5f, 1f);
        rtH.sizeDelta = new Vector2(0f, Height);
        rtH.anchoredPosition = Vector2.zero;

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

        // Garis emas bawah (saudara header, bukan anak header)
        GameObject goldLine = new GameObject("GoldLine");
        goldLine.transform.SetParent(parent, false);
        RectTransform rtLine = goldLine.AddComponent<RectTransform>();
        rtLine.anchorMin = new Vector2(0f, 1f);
        rtLine.anchorMax = new Vector2(1f, 1f);
        rtLine.pivot = new Vector2(0.5f, 1f);
        rtLine.sizeDelta = new Vector2(0f, GoldLineThickness);
        rtLine.anchoredPosition = new Vector2(0f, -(Height - GoldLineInset));
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

        TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        titleTmp.raycastTarget = false;
        titleTmp.fontStyle = FontStyles.Bold;

        return header;
    }
}