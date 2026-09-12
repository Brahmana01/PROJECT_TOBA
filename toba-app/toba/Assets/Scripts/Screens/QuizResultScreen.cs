using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuizResultScreen : BaseScreen
{
    private LocationData locationData;
    private int  correctCount;
    private int  totalQuestions;
    private bool isSubmitted = false;

    private TextMeshProUGUI statusTxt;
    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI starsTxt;
    private TextMeshProUGUI scoreTxt;
    private GameObject      loadingObj;

    // Untuk share
    private int _starsEarned = 0;

    public QuizResultScreen(Transform parent, LocationData loc,
        int correct, int total)
        : base(parent, "QuizResult")
    {
        locationData   = loc;
        correctCount   = correct;
        totalQuestions = total;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0, 0, 0.5f), Vector2.zero, Vector2.one);

        HeaderBar.Create(screenObj.transform, "HASIL TANTANGAN", null);

        bool isPass = correctCount >= locationData.minimum_correct;
        _starsEarned = CalculateStars();

        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        // ── CARD HASIL ────────────────────────────────────
        GameObject card = new GameObject("ResultCard");
        card.transform.SetParent(screenObj.transform, false);
        RectTransform rtC = card.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0.5f, 0.5f);
        rtC.sizeDelta = new Vector2(960, 950);
        rtC.anchoredPosition = new Vector2(0, 75f);

        Image cardImg = card.AddComponent<Image>();
        cardImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        cardImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        cardImg.color  = wp != null
            ? new Color(0.92f, 0.82f, 0.65f, 0.98f)
            : TOBATheme.CardBG;
        cardImg.raycastTarget = false;

        Outline ol = card.AddComponent<Outline>();
        ol.effectColor    = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(6, 6);
        card.AddComponent<Shadow>().effectDistance = new Vector2(8, -8);
        ((Shadow)card.GetComponents<Component>()[^1]).effectColor =
            new Color(0, 0, 0, 0.6f);
        ScreenManager.Instance.StartCoroutine(
            UIBuilder.AnimateScaleIn(card.transform, 0.3f));

        // Status
        statusTxt = UIBuilder.CreateTextSized(card.transform,
            "Menilai...", 70, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(900, 110), new Vector2(0, 365f))
            .GetComponent<TextMeshProUGUI>();
        statusTxt.raycastTarget = false;

        // Bintang animasi
        ScreenManager.Instance.StartCoroutine(
            StarRating.CreateAnimated(card.transform, _starsEarned, 3,
                new Vector2(0, 215f), 155f));

        // Garis
        UIBuilder.CreatePanelSized(card.transform, "Line",
            TOBATheme.BatakGold, new Vector2(820, 4), new Vector2(0, 82f))
            .GetComponent<Image>().raycastTarget = false;

        // Skor
        scoreTxt = UIBuilder.CreateTextSized(card.transform,
            "Benar : " + correctCount + " / " + totalQuestions,
            42, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(700, 65), new Vector2(0, 22f))
            .GetComponent<TextMeshProUGUI>();
        scoreTxt.fontStyle = FontStyles.Normal; scoreTxt.raycastTarget = false;

        xpTxt = UIBuilder.CreateTextSized(card.transform,
            "✨ XP Diperoleh : +0",
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(820, 65), new Vector2(0, -62f))
            .GetComponent<TextMeshProUGUI>();
        xpTxt.fontStyle = FontStyles.Normal; xpTxt.raycastTarget = false;

        starsTxt = UIBuilder.CreateTextSized(card.transform,
            "⭐ Bintang Diperoleh : +0",
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(820, 65), new Vector2(0, -138f))
            .GetComponent<TextMeshProUGUI>();
        starsTxt.fontStyle = FontStyles.Normal; starsTxt.raycastTarget = false;

        loadingObj = UIBuilder.CreateTextSized(card.transform,
            "Menyimpan hasil...", 30, TOBATheme.TextGray,
            TextAlignmentOptions.Center, new Vector2(800, 50),
            new Vector2(0, -210f));
        loadingObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        loadingObj.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Karakter ekspresi
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string b   = charID == 1 ? "ucok" : "butet";
        Sprite exprSp = TOBATheme.LoadSprite("gambar/" + b + (isPass ? "_senang" : "_salah"))
                      ?? TOBATheme.LoadSprite(charID == 1 ? "char_ucok" : "char_butet");
        if (exprSp != null)
        {
            GameObject ch = UIBuilder.CreateImage(screenObj.transform,
                "Character", exprSp, new Vector2(255, 355), new Vector2(458f, -475f));
            ch.GetComponent<Image>().preserveAspect = true;
        }

        // Haptic sesuai hasil
        HapticManager.Instance?.Vibrate(isPass ? HapticType.Success : HapticType.Medium);
        SoundManager.Instance?.PlaySFX(isPass ? "sfx_win" : "sfx_lose");

        CreateActionButtons(isPass);
        SubmitResult();
    }

    int CalculateStars()
    {
        float r = totalQuestions > 0 ? (float)correctCount / totalQuestions : 0f;
        if (r >= 1f)   return 3;
        if (r >= 0.6f) return 2;
        if (r >= 0.4f) return 1;
        return 0;
    }

    void CreateActionButtons(bool isPass)
    {
        float yLanjut  = -568f;
        float ykembali = -700f;

        if (isPass)
        {
            // Lanjut
            UIBuilder.CreateButton(screenObj.transform,
                "BtnLanjut", "LANJUT ▶",
                new Vector2(800, 132), TOBATheme.BtnPrimary, () => {
                    SoundManager.Instance?.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(new MapScreen(container));
                }).GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, yLanjut);
        }
        else
        {
            // Ulangi
            UIBuilder.CreateButton(screenObj.transform,
                "BtnUlangi", "🔄 ULANGI",
                new Vector2(800, 132), TOBATheme.Warning, () => {
                    SoundManager.Instance?.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(
                        new QuizScreen(container, locationData));
                }).GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, yLanjut);
        }

        // Kembali ke peta
        UIBuilder.CreateButton(screenObj.transform,
            "BtnPeta", "KEMBALI KE PETA",
            new Vector2(800, 105), TOBATheme.BtnSecondary, () => {
                SoundManager.Instance?.PlaySFX("sfx_click");
                ScreenManager.Instance.ChangeScreen(new MapScreen(container));
            }).GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0, ykembali);

        // Tombol Share
        GameObject btnShare = UIBuilder.CreateSmallButton(screenObj.transform,
            "BtnShare", "📤 BAGIKAN",
            new Vector2(380, 90), new Color(0.1f, 0.3f, 0.5f, 0.9f),
            Color.white, OnShare);
        btnShare.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0, -810f);
    }

    void OnShare()
    {
        SoundManager.Instance?.PlaySFX("sfx_click");
        HapticManager.Instance?.Vibrate(HapticType.Light);
        ShareManager.ShareQuizResult(
            locationData.name, correctCount, totalQuestions, _starsEarned);
    }

    void SubmitResult()
    {
        if (isSubmitted) return;
        isSubmitted = true;

        int playerID = PlayerPrefs.GetInt("PlayerID");

        var req = new SubmitQuizRequest
        {
            player_id     = playerID,
            location_id   = locationData.id,
            correct_count = correctCount
        };

        string jsonPayload = JsonUtility.ToJson(req);

        // ── Coba submit online dulu ──────────────────────────
        APIManager.Instance.PostRequest(
            "/locations/submit",
            jsonPayload,

            // SUCCESS
            (response) =>
            {
                if (screenObj == null) return;

                // Berhasil → hapus antrian offline
                PlayerPrefs.DeleteKey("OfflineSubmitQueue");
                PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
                PlayerPrefs.Save();

                HandleSubmitResponse(response);
            },

            // ERROR / OFFLINE
            (err) =>
            {
                if (screenObj == null) return;

                // Gagal → simpan hasil ke antrian offline
                PlayerPrefs.SetString("OfflineSubmitQueue", jsonPayload);
                PlayerPrefs.SetString(
                    "OfflineSubmitEndpoint",
                    "/locations/submit"
                );
                PlayerPrefs.Save();

                NetworkManager.Instance?.ShowToast(
                    "Offline: hasil disimpan, akan dikirim saat online.",
                    false
                );

                // Tetap tampilkan hasil ke pemain
                HandleOfflineResult();
            }
        );
    }


    void HandleSubmitResponse(string response)
    {
        if (screenObj == null) return;

        bool isPass = correctCount >= locationData.minimum_correct;
        int playerID = PlayerPrefs.GetInt("PlayerID");

        SubmitQuizResponse res =
            JsonUtility.FromJson<SubmitQuizResponse>(response);

        if (statusTxt)
        {
            statusTxt.text = isPass
                ? "🎉 LUAR BIASA!"
                : "😔 BELUM LULUS";

            statusTxt.color = isPass
                ? TOBATheme.BtnPrimary
                : TOBATheme.Error;
        }

        if (xpTxt)
            xpTxt.text = "✨ XP Diperoleh : +" + res.xp;

        if (starsTxt)
            starsTxt.text = "⭐ Bintang Diperoleh : +" + res.stars;

        if (loadingObj)
            Object.Destroy(loadingObj);

        if (res.level > PlayerPrefs.GetInt("PlayerLevel", 1))
        {
            PlayerPrefs.SetInt("PlayerLevel", res.level);
            SoundManager.Instance?.PlaySFX("sfx_levelup");
            HapticManager.Instance?.Vibrate(HapticType.Success);

            LevelUpPopup.Show(
                screenObj.transform,
                res.level,
                ""
            );
        }

        APIManager.Instance.GetRequest(
            "/check-achievements/" + playerID,
            (achRes) =>
            {
                if (screenObj == null) return;

                NewAchievementResponse ach =
                    JsonUtility.FromJson<NewAchievementResponse>(achRes);

                if (ach?.count > 0)
                {
                    SoundManager.Instance?.PlaySFX("sfx_achievement");

                    AchievementPopup.Show(
                        screenObj.transform,
                        ach.newly_unlocked[0]
                    );
                }
            },
            (_) => { }
        );
    }

    void HandleOfflineResult()
    {
        bool isPass = correctCount >= locationData.minimum_correct;

        if (statusTxt)
        {
            statusTxt.text = isPass
                ? "🎉 LUAR BIASA!"
                : "😔 BELUM LULUS";

            statusTxt.color = isPass
                ? TOBATheme.BtnPrimary
                : TOBATheme.Error;
        }

        if (xpTxt)
            xpTxt.text = "✨ XP : (offline)";

        if (starsTxt)
            starsTxt.text = "⭐ Bintang : (offline)";

        if (loadingObj)
            Object.Destroy(loadingObj);
    }

}