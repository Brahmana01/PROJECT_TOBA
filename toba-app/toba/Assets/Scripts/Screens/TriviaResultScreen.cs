using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TriviaResultScreen : BaseScreen
{
    private TriviaCategoryData categoryData;
    private int correctCount;
    private int totalQuestions;

    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI levelTxt;
    private GameObject loadingObj;

    public TriviaResultScreen(Transform parent, TriviaCategoryData cat, int correct, int total)
        : base(parent, "TriviaResult")
    {
        categoryData = cat;
        correctCount = correct;
        totalQuestions = total;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "HASIL TRIVIA", null);

        // Card hasil
        GameObject resultCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "ResultCard", TOBATheme.CardBG, new Vector2(960, 860), new Vector2(0, 130f));

        Outline outline = resultCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(5, 5);

        // Nama kategori
        UIBuilder.CreateTextSized(resultCard.transform,
            categoryData.name.ToUpper(), 48, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(900, 80), new Vector2(0, 360f));

        // Bintang animasi
        int stars = CalculateStars();
        ScreenManager.Instance.StartCoroutine(
            StarRating.CreateAnimated(resultCard.transform, stars, 3, new Vector2(0, 220f)));

        // Skor
        UIBuilder.CreateTextSized(resultCard.transform,
            "Benar  : " + correctCount + " / " + totalQuestions,
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(700, 60), new Vector2(0, 80f));

        UIBuilder.CreateTextSized(resultCard.transform,
            "Salah  : " + (totalQuestions - correctCount) + " / " + totalQuestions,
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(700, 60), new Vector2(0, 10f));

        // Divider
        UIBuilder.CreatePanelSized(resultCard.transform, "Divider",
            TOBATheme.BatakGold, new Vector2(800, 4), new Vector2(0, -50f));

        // XP
        xpTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "✨ XP Diperoleh : +0",
            38, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(800, 60), new Vector2(0, -120f))
            .GetComponent<TextMeshProUGUI>();
        xpTxt.fontStyle = FontStyles.Normal;

        // Level
        levelTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "Level saat ini : -",
            36, TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(800, 55), new Vector2(0, -190f))
            .GetComponent<TextMeshProUGUI>();
        levelTxt.fontStyle = FontStyles.Normal;

        // Loading
        loadingObj = UIBuilder.CreateTextSized(resultCard.transform,
            "Menyimpan hasil...", 30, TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(800, 50), new Vector2(0, -260f));

        // Tombol aksi
        // Mainkan lagi
        GameObject btnLagi = UIBuilder.CreateButton(screenObj.transform,
            "BtnLagi", "🔄 MAIN LAGI",
            new Vector2(800, 130), TOBATheme.BtnPrimary, () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(
                    new TriviaQuizScreen(container, categoryData));
            });
        btnLagi.GetComponent<RectTransform>().anchorMin =
            btnLagi.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnLagi.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -560f);

        // Kembali ke menu trivia
        GameObject btnBack = UIBuilder.CreateButton(screenObj.transform,
            "BtnBack", "PILIH KATEGORI LAIN",
            new Vector2(800, 100), TOBATheme.BtnSecondary, () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(new TriviaMenuScreen(container));
            });
        btnBack.GetComponent<RectTransform>().anchorMin =
            btnBack.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnBack.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -690f);

        SubmitResult();
    }

    int CalculateStars()
    {
        float ratio = (float)correctCount / totalQuestions;
        if (ratio >= 1f)   return 3;
        if (ratio >= 0.6f) return 2;
        if (ratio >= 0.4f) return 1;
        return 0;
    }

    void SubmitResult()
    {
        int playerID = PlayerPrefs.GetInt("PlayerID");

        SubmitTriviaRequest req = new SubmitTriviaRequest
        {
            player_id   = playerID,
            category_id = categoryData.id,
            correct_count = correctCount,
            wrong_count   = totalQuestions - correctCount
        };

        APIManager.Instance.PostRequest("/trivia/submit", JsonUtility.ToJson(req), (response) => {
            if (screenObj == null) return;

            SubmitTriviaResponse res = JsonUtility.FromJson<SubmitTriviaResponse>(response);

            if (xpTxt != null)    xpTxt.text    = "✨ XP Diperoleh : +" + res.xp;
            if (levelTxt != null) levelTxt.text  = "Level saat ini : " + res.level;
            if (loadingObj != null) Object.Destroy(loadingObj);

            PlayerPrefs.SetInt("PlayerLevel", res.level);

        }, (err) => {
            if (xpTxt != null)    xpTxt.text    = "✨ XP : (offline)";
            if (loadingObj != null) Object.Destroy(loadingObj);
        });
    }
}