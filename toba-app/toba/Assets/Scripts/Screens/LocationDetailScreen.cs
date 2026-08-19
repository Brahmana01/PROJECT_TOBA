using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LocationDetailScreen : BaseScreen
{
    private LocationData locationData;
    private TextMeshProUGUI factTxt;
    private GameObject loadingFactObj;

    public LocationDetailScreen(Transform parent, LocationData loc)
        : base(parent, "LocationDetail")
    {
        locationData = loc;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        Sprite bgSprite = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bgSprite);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0.04f, 0, 0.45f), Vector2.zero, Vector2.one);

        HeaderBar.Create(screenObj.transform,
            locationData.name.ToUpper(), () => {
                ScreenManager.Instance.ChangeScreen(new MapScreen(container));
            });

        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");
        Sprite btnSprite   = TOBATheme.LoadSprite("btn_gold");

        ScrollRect scroll = UIBuilder.CreateScrollView(screenObj.transform,
            "DetailScroll", new Vector2(0, 0.08f), new Vector2(1, 0.88f));
        Transform content = scroll.content;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1500f);

        // Gambar lokasi placeholder
        GameObject imgArea = new GameObject("ImageArea");
        imgArea.transform.SetParent(content, false);
        RectTransform rtImg = imgArea.AddComponent<RectTransform>();
        rtImg.anchorMin = rtImg.anchorMax = new Vector2(0.5f, 1f);
        rtImg.sizeDelta = new Vector2(1000, 520);
        rtImg.anchoredPosition = new Vector2(0, -280f);
        Image imgAreaImg = imgArea.AddComponent<Image>();
        imgAreaImg.sprite = UIBuilder.GetDefaultSprite();
        imgAreaImg.color  = new Color(0.15f, 0.35f, 0.5f, 0.8f);
        imgAreaImg.raycastTarget = false;
        Outline imgOl = imgArea.AddComponent<Outline>();
        imgOl.effectColor    = TOBATheme.BatakGold;
        imgOl.effectDistance = new Vector2(4, 4);

        UIBuilder.CreateTextSized(imgArea.transform,
            "[ " + locationData.name + " ]",
            38, new Color(1f, 1f, 1f, 0.6f), TextAlignmentOptions.Center,
            new Vector2(940, 100), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Card deskripsi
        GameObject descCard = new GameObject("DescCard");
        descCard.transform.SetParent(content, false);
        RectTransform rtDesc = descCard.AddComponent<RectTransform>();
        rtDesc.anchorMin = rtDesc.anchorMax = new Vector2(0.5f, 1f);
        rtDesc.sizeDelta = new Vector2(960, 340);
        rtDesc.anchoredPosition = new Vector2(0, -650f);
        Image descImg = descCard.AddComponent<Image>();
        descImg.sprite = panelSprite ?? UIBuilder.GetDefaultSprite();
        descImg.type   = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        descImg.color  = panelSprite != null
            ? new Color(0.92f, 0.82f, 0.65f, 0.97f)
            : TOBATheme.CardBG;
        descImg.raycastTarget = false;
        Outline descOl = descCard.AddComponent<Outline>();
        descOl.effectColor    = TOBATheme.BatakGold;
        descOl.effectDistance = new Vector2(4, 4);

        UIBuilder.CreateTextSized(descCard.transform,
            "📍 TENTANG LOKASI", 36,
            TOBATheme.WoodDark, TextAlignmentOptions.Left,
            new Vector2(900, 60), new Vector2(0, 125f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(descCard.transform,
            locationData.description, 30,
            TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(900, 240), new Vector2(0, -30f))
            .GetComponent<TextMeshProUGUI>();
        descTmp.fontStyle         = FontStyles.Normal;
        descTmp.textWrappingMode  = TextWrappingModes.Normal;
        descTmp.raycastTarget     = false;

        // Card fakta
        GameObject factCard = new GameObject("FactCard");
        factCard.transform.SetParent(content, false);
        RectTransform rtFact = factCard.AddComponent<RectTransform>();
        rtFact.anchorMin = rtFact.anchorMax = new Vector2(0.5f, 1f);
        rtFact.sizeDelta = new Vector2(960, 320);
        rtFact.anchoredPosition = new Vector2(0, -1030f);
        Image factImg = factCard.AddComponent<Image>();
        factImg.sprite = panelSprite ?? UIBuilder.GetDefaultSprite();
        factImg.type   = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        factImg.color  = panelSprite != null
            ? new Color(0.35f, 0.18f, 0.05f, 0.97f)
            : TOBATheme.WoodDark;
        factImg.raycastTarget = false;
        Outline factOl = factCard.AddComponent<Outline>();
        factOl.effectColor    = TOBATheme.BatakGold;
        factOl.effectDistance = new Vector2(4, 4);

        UIBuilder.CreateTextSized(factCard.transform,
            "💡 FAKTA MENARIK", 36,
            TOBATheme.BatakGold, TextAlignmentOptions.Left,
            new Vector2(900, 60), new Vector2(0, 118f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        loadingFactObj = UIBuilder.CreateTextSized(factCard.transform,
            "Memuat fakta...", 30,
            TOBATheme.TextLight, TextAlignmentOptions.Left,
            new Vector2(900, 220), new Vector2(0, -28f));
        factTxt = loadingFactObj.GetComponent<TextMeshProUGUI>();
        factTxt.fontStyle         = FontStyles.Normal;
        factTxt.textWrappingMode  = TextWrappingModes.Normal;
        factTxt.raycastTarget     = false;

        // Info reward
        CreateRewardInfo(content);

        // Tombol mulai
        GameObject startBtn = UIBuilder.CreateButton(content, "BtnMulai",
            "⚔ MULAI TANTANGAN", new Vector2(820, 150),
            TOBATheme.BtnPrimary, () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(
                    new QuizScreen(container, locationData));
            });
        startBtn.GetComponent<RectTransform>().anchorMin =
            startBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        startBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1410f);

        FetchRandomFact();
    }

    void CreateRewardInfo(Transform parent)
    {
        GameObject rewardCard = UIBuilder.CreatePanelSized(parent, "RewardCard",
            new Color(0.1f, 0.1f, 0.1f, 0.5f), new Vector2(950, 120), new Vector2(0, -1170f));

        // XP reward
        UIBuilder.CreateTextSized(rewardCard.transform,
            "✨ +" + locationData.reward_xp + " XP",
            36, TOBATheme.BatakGold, TextAlignmentOptions.Left,
            new Vector2(400, 80), new Vector2(-220f, 0));

        // Bintang reward
        UIBuilder.CreateTextSized(rewardCard.transform,
            "⭐ +" + locationData.reward_stars + " Bintang",
            36, TOBATheme.StarActive, TextAlignmentOptions.Right,
            new Vector2(400, 80), new Vector2(220f, 0));
    }

    void FetchRandomFact()
    {
        APIManager.Instance.GetRequest("/locations/" + locationData.id + "/fact", (response) => {
            if (factTxt == null) return;

            LocationFactResponse res = JsonUtility.FromJson<LocationFactResponse>(response);
            if (res != null && res.fact != null)
            {
                factTxt.text = res.fact.fact;
            }
            else
            {
                factTxt.text = "Lokasi ini menyimpan kekayaan budaya Batak Toba yang luar biasa.";
            }
        }, (err) => {
            if (factTxt != null)
                factTxt.text = "Tidak dapat memuat fakta saat ini.";
        });
    }
}