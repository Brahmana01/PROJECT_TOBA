using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TriviaMenuScreen : BaseScreen
{
    private List<TriviaCategoryData> categories = new List<TriviaCategoryData>();
    private GameObject loadingObj;

    public TriviaMenuScreen(Transform parent) : base(parent, "TriviaMenu") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // Header
        HeaderBar.Create(screenObj.transform, "TRIVIA BUDAYA", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });


        // Judul sub
        UIBuilder.CreateTextSized(screenObj.transform,
            "Pilih Kategori", 42, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(900, 70), new Vector2(0, 680f));

        // Loading
        loadingObj = UIBuilder.CreateTextSized(screenObj.transform,
            "Memuat kategori...", 40, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 80), new Vector2(0, 0));

        FetchCategories();
    }

    void FetchCategories()
    {
        APIManager.Instance.GetRequest("/trivia/categories", (response) => {
            if (screenObj == null) return;

            // Parse array langsung
            string wrapped = "{\"categories\":" + response + "}";
            TriviaCategoryListResponse res =
                JsonUtility.FromJson<TriviaCategoryListResponse>(wrapped);

            if (res != null && res.categories != null)
                categories = res.categories;

            if (loadingObj != null) Object.Destroy(loadingObj);
            BuildCategoryList();

        }, (err) => {
            if (loadingObj != null) Object.Destroy(loadingObj);
            UIBuilder.CreateTextSized(screenObj.transform,
                "Gagal memuat. Cek koneksi.", 38, TOBATheme.Error,
                TextAlignmentOptions.Center, new Vector2(800, 80), Vector2.zero);
        });
    }

    void BuildCategoryList()
    {
        if (screenObj == null) return;

        // Scroll area
        ScrollRect scroll = UIBuilder.CreateScrollView(
            screenObj.transform, "CategoryScroll",
            new Vector2(0, 0.1f), new Vector2(1, 0.82f));

        Transform content = scroll.content;
        float cardHeight = 220f;
        float totalHeight = categories.Count * cardHeight + 60f;
        scroll.content.sizeDelta = new Vector2(0, totalHeight);

        for (int i = 0; i < categories.Count; i++)
        {
            TriviaCategoryData cat = categories[i];
            float yPos = -(i * cardHeight) - 110f;
            CreateCategoryCard(content, cat, yPos);
        }
    }

    void CreateCategoryCard(Transform parent, TriviaCategoryData cat, float yPos)
    {
        // Card container
        GameObject card = UIBuilder.CreatePanelSized(parent, "Card_" + cat.id,
            TOBATheme.CardBG, new Vector2(960, 190), new Vector2(0, yPos));

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        // Strip warna kiri
        GameObject strip = UIBuilder.CreatePanelSized(card.transform, "Strip",
            TOBATheme.BtnPrimary, new Vector2(14, 170), new Vector2(-460f, 0));

        // Nama kategori
        UIBuilder.CreateTextSized(card.transform, cat.name.ToUpper(),
            44, TOBATheme.WoodDark, TextAlignmentOptions.Left,
            new Vector2(650, 70), new Vector2(-90f, 35f));

        // Deskripsi
        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(card.transform,
            cat.description, 30, TOBATheme.TextGray, TextAlignmentOptions.Left,
            new Vector2(650, 55), new Vector2(-90f, -20f))
            .GetComponent<TextMeshProUGUI>();
        descTmp.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(descTmp, true);

        // Info soal
        UIBuilder.CreateTextSized(card.transform,
            cat.question_count + " Soal",
            30, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(200, 50), new Vector2(350f, 35f));

        // Tombol mulai
        GameObject btnMulai = UIBuilder.CreateSmallButton(card.transform,
            "BtnMulai", "MULAI ▶",
            new Vector2(180, 70), TOBATheme.BtnPrimary, TOBATheme.TextLight, () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(
                    new TriviaQuizScreen(container, cat));
            });
        btnMulai.GetComponent<RectTransform>().anchoredPosition = new Vector2(360f, -40f);

        // Seluruh card bisa diklik
        Button cardBtn = card.AddComponent<Button>();
        cardBtn.onClick.AddListener(() => {
            SoundManager.Instance.PlaySFX("sfx_click");
            ScreenManager.Instance.ChangeScreen(new TriviaQuizScreen(container, cat));
        });
    }
}