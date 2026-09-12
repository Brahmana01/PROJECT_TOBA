using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TriviaMenuScreen : BaseScreen
{
    private List<TriviaCategoryData> categories = new List<TriviaCategoryData>();
    private GameObject loadingObj;
    private GameObject emptyStateObj;

    public TriviaMenuScreen(Transform parent) : base(parent, "TriviaMenu") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "TRIVIA BUDAYA", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        UIBuilder.CreateTextSized(
            screenObj.transform, "Pilih Kategori", 42, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(900, 70), new Vector2(0, 680f)
        );

        ShowLoading();
        FetchCategories();
    }

    private void ShowLoading()
    {
        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }

        ClearEmptyState();

        loadingObj = UIBuilder.CreateTextSized(
            screenObj.transform, "Memuat kategori...", 40, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(700, 80), new Vector2(0, 0)
        );
    }

    private void ClearEmptyState()
    {
        if (emptyStateObj != null)
        {
            Object.Destroy(emptyStateObj);
            emptyStateObj = null;
        }
    }

    private void FetchCategories()
    {
        ClearEmptyState();

        APIManager.Instance.GetRequest(
            "/trivia/categories",
            (response) => {
                if (screenObj == null) return;

                string wrapped = "{\"categories\":" + response + "}";
                TriviaCategoryListResponse res = JsonUtility.FromJson<TriviaCategoryListResponse>(wrapped);

                categories = res != null && res.categories != null
                    ? res.categories
                    : new List<TriviaCategoryData>();

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                if (categories.Count == 0)
                {
                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "📚",
                        "Belum ada kategori trivia",
                        "Hubungi admin untuk menambahkan kategori",
                        () => FetchCategories()
                    );
                    return;
                }

                BuildCategoryList();
            },
            (err) => {
                if (screenObj == null) return;

                Debug.LogWarning("[Trivia] Gagal memuat kategori: " + err);

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                emptyStateObj = EmptyState.Show(
                    screenObj.transform,
                    "📚",
                    "Gagal memuat kategori",
                    "Periksa koneksi internet",
                    () => RetryFetchCategories()
                );
            }
        );
    }

    private void RetryFetchCategories()
    {
        ClearEmptyState();
        ShowLoading();
        FetchCategories();
    }

    private void BuildCategoryList()
    {
        if (screenObj == null) return;

        ScrollRect scroll = UIBuilder.CreateScrollView(
            screenObj.transform, "CategoryScroll",
            new Vector2(0, 0.1f), new Vector2(1, 0.82f)
        );

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

    private void CreateCategoryCard(Transform parent, TriviaCategoryData cat, float yPos)
    {
        GameObject card = UIBuilder.CreatePanelSized(
            parent, "Card_" + cat.id, TOBATheme.CardBG,
            new Vector2(960, 190), new Vector2(0, yPos)
        );

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        UIBuilder.CreatePanelSized(
            card.transform, "Strip", TOBATheme.BtnPrimary,
            new Vector2(14, 170), new Vector2(-460f, 0)
        );

        UIBuilder.CreateTextSized(
            card.transform, cat.name.ToUpper(), 44, TOBATheme.WoodDark,
            TextAlignmentOptions.Left, new Vector2(650, 70), new Vector2(-90f, 35f)
        );

        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(
            card.transform, cat.description, 30, TOBATheme.TextGray,
            TextAlignmentOptions.Left, new Vector2(650, 55), new Vector2(-90f, -20f)
        ).GetComponent<TextMeshProUGUI>();

        descTmp.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(descTmp, true);

        UIBuilder.CreateTextSized(
            card.transform, cat.question_count + " Soal", 30, TOBATheme.BatakGold,
            TextAlignmentOptions.Right, new Vector2(200, 50), new Vector2(350f, 35f)
        );

        GameObject btnMulai = UIBuilder.CreateSmallButton(
            card.transform, "BtnMulai", "MULAI ▶",
            new Vector2(180, 70), TOBATheme.BtnPrimary, TOBATheme.TextLight,
            () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(new TriviaQuizScreen(container, cat));
            }
        );

        btnMulai.GetComponent<RectTransform>().anchoredPosition = new Vector2(360f, -40f);

        Button cardBtn = card.AddComponent<Button>();
        cardBtn.onClick.AddListener(() => {
            SoundManager.Instance.PlaySFX("sfx_click");
            ScreenManager.Instance.ChangeScreen(new TriviaQuizScreen(container, cat));
        });
    }
}