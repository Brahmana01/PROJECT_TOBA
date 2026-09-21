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
        SetPhotoBackground();

        HeaderBar.Create(screenObj.transform, "TRIVIA BUDAYA", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        BuildTriviaVisual();

        ShowLoading();
        FetchCategories();
    }

    private void SetPhotoBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(screenObj.transform, false);

        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = bg.AddComponent<Image>();

        Sprite sprite = Resources.Load<Sprite>("Backgrounds/trivia_bg");

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }
        else
        {
            Debug.LogWarning("Background trivia_bg tidak ditemukan!");
            image.color = TOBATheme.ForestGreen;
        }

        bg.transform.SetAsFirstSibling();
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

                BuildCategoryButtons();
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

    private void BuildCategoryButtons()
{
    if (screenObj == null) return;

    Transform parent = screenObj.transform;

    float cardWidth = 480f;
    float cardHeight = 430f;

    float leftX = -260f;
    float rightX = 260f;

    float topY = 250f;
    float bottomY = -250f;

    for (int i = 0; i < categories.Count && i < 4; i++)
    {
        TriviaCategoryData cat = categories[i];

        float x;
        float y;

        if (i == 0)
        {
            x = leftX;
            y = topY;
        }
        else if (i == 1)
        {
            x = rightX;
            y = topY;
        }
        else if (i == 2)
        {
            x = leftX;
            y = bottomY;
        }
        else
        {
            x = rightX;
            y = bottomY;
        }

        CreateInvisibleCategoryButton(
            parent,
            cat,
            new Vector2(cardWidth, cardHeight),
            new Vector2(x, y)
        );
    }
}

    private void CreateInvisibleCategoryButton(
    Transform parent,
    TriviaCategoryData cat,
    Vector2 size,
    Vector2 position)
{
    GameObject buttonObj = new GameObject("CategoryButton_" + cat.id);

    buttonObj.transform.SetParent(parent, false);

    RectTransform rect = buttonObj.AddComponent<RectTransform>();

    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);

    rect.pivot = new Vector2(0.5f, 0.5f);

    rect.sizeDelta = size;
    rect.anchoredPosition = position;

    Image image = buttonObj.AddComponent<Image>();

    // Transparan
    image.color = new Color(1f, 1f, 1f, 0f);

    Button button = buttonObj.AddComponent<Button>();

    button.targetGraphic = image;

    button.onClick.AddListener(() =>
    {
        SoundManager.Instance.PlaySFX("sfx_click");

        ScreenManager.Instance.ChangeScreen(
            new TriviaQuizScreen(container, cat)
        );
    });
}

    private void RetryFetchCategories()
    {
        ClearEmptyState();
        ShowLoading();
        FetchCategories();
    }

    private void BuildTriviaVisual()
{
    if (screenObj == null)
    {
        Debug.LogError("[Trivia] screenObj NULL!");
        return;
    }

    GameObject bg = new GameObject("TriviaVisual");
    bg.transform.SetParent(screenObj.transform, false);

    RectTransform rect = bg.AddComponent<RectTransform>();

    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    rect.sizeDelta = new Vector2(1080f, 1080f);
    rect.anchoredPosition = new Vector2(0f, -40f);

    Image image = bg.AddComponent<Image>();

    Sprite sprite = Resources.Load<Sprite>("Trivia/trivia_menu");

    if (sprite == null)
    {
        Debug.LogError(
            "[Trivia] GAGAL LOAD GAMBAR! " +
            "Pastikan file ada di " +
            "Assets/Resources/Trivia/trivia_menu.png"
        );

        return;
    }

    image.sprite = sprite;
    image.type = Image.Type.Simple;
    image.preserveAspect = true;

    // Background dibuat terlebih dahulu.
    // TriviaVisual harus berada DI ATAS Background.
    bg.transform.SetSiblingIndex(1);

    Debug.Log("[Trivia] Gambar trivia berhasil dimuat!");
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