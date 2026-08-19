using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuizResultScreen : BaseScreen
{
    private LocationData locationData;
    private int correctCount;
    private int totalQuestions;
    private bool isSubmitted = false;

    // UI
    private TextMeshProUGUI statusTxt;
    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI starsTxt;
    private TextMeshProUGUI scoreTxt;
    private GameObject loadingObj;

    public QuizResultScreen(Transform parent, LocationData loc, int correct, int total)
        : base(parent, "QuizResult")
    {
        locationData = loc;
        correctCount = correct;
        totalQuestions = total;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        Sprite bgSprite = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bgSprite);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0, 0, 0.5f), Vector2.zero, Vector2.one);

        HeaderBar.Create(screenObj.transform, "HASIL TANTANGAN", null);

        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");
        Sprite btnSprite   = TOBATheme.LoadSprite("btn_gold");

        // Card hasil utama
        GameObject resultCard = new GameObject("ResultCard");
        resultCard.transform.SetParent(screenObj.transform, false);
        RectTransform rtRC = resultCard.AddComponent<RectTransform>();
        rtRC.anchorMin = rtRC.anchorMax = new Vector2(0.5f, 0.5f);
        rtRC.sizeDelta = new Vector2(960, 940);
        rtRC.anchoredPosition = new Vector2(0, 80f);

        Image rcImg = resultCard.AddComponent<Image>();
        rcImg.sprite = panelSprite ?? UIBuilder.GetDefaultSprite();
        rcImg.type   = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        rcImg.color  = panelSprite != null
            ? new Color(0.92f, 0.82f, 0.65f, 0.98f)
            : TOBATheme.CardBG;
        rcImg.raycastTarget = false;

        Outline rcOl = resultCard.AddComponent<Outline>();
        rcOl.effectColor    = TOBATheme.BatakGold;
        rcOl.effectDistance = new Vector2(6, 6);

        Shadow rcSh = resultCard.AddComponent<Shadow>();
        rcSh.effectColor    = new Color(0, 0, 0, 0.6f);
        rcSh.effectDistance = new Vector2(8, -8);

        // Status teks
        statusTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "Menilai...", 70, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(900, 110), new Vector2(0, 360f))
            .GetComponent<TextMeshProUGUI>();
        statusTxt.raycastTarget = false;

        // Bintang animasi
        int starEarned = CalculateStars();
        ScreenManager.Instance.StartCoroutine(
            StarRating.CreateAnimated(resultCard.transform, starEarned, 3,
                new Vector2(0, 215f), 150f));

        // Garis pembatas
        UIBuilder.CreatePanelSized(resultCard.transform, "Divider",
            TOBATheme.BatakGold, new Vector2(820, 4), new Vector2(0, 80f))
            .GetComponent<Image>().raycastTarget = false;

        // Skor
        scoreTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "Benar : " + correctCount + " / " + totalQuestions,
            42, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(700, 65), new Vector2(0, 20f))
            .GetComponent<TextMeshProUGUI>();
        scoreTxt.fontStyle    = FontStyles.Normal;
        scoreTxt.raycastTarget = false;

        // XP reward
        xpTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "✨ XP Diperoleh : +0",
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(820, 65), new Vector2(0, -60f))
            .GetComponent<TextMeshProUGUI>();
        xpTxt.fontStyle    = FontStyles.Normal;
        xpTxt.raycastTarget = false;

        // Bintang reward
        starsTxt = UIBuilder.CreateTextSized(resultCard.transform,
            "⭐ Bintang Diperoleh : +0",
            40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(820, 65), new Vector2(0, -135f))
            .GetComponent<TextMeshProUGUI>();
        starsTxt.fontStyle    = FontStyles.Normal;
        starsTxt.raycastTarget = false;

        // Loading submit
        loadingObj = UIBuilder.CreateTextSized(resultCard.transform,
            "Menyimpan hasil...", 30, TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(800, 50), new Vector2(0, -205f));
        loadingObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        loadingObj.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Karakter ekspresi
        bool isPass = correctCount >= locationData.minimum_correct;
        int charID  = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string baseName = charID == 1 ? "ucok" : "butet";
        string exprPath = "gambar/" + baseName + (isPass ? "_senang" : "_salah");
        Sprite exprSprite = TOBATheme.LoadSprite(exprPath)
                        ?? TOBATheme.LoadSprite(charID == 1 ? "char_ucok" : "char_butet");
        if (exprSprite != null)
        {
            GameObject charGO = UIBuilder.CreateImage(screenObj.transform,
                "Character", exprSprite,
                new Vector2(260, 360), new Vector2(460f, -480f));
            charGO.GetComponent<Image>().preserveAspect = true;
        }

        // Tombol aksi
        CreateActionButtons();

        // Submit
        SubmitResult();
    }

    int CalculateStars()
    {
        float ratio = (float)correctCount / totalQuestions;
        if (ratio >= 1f) return 3;
        if (ratio >= 0.6f) return 2;
        if (ratio >= 0.4f) return 1;
        return 0;
    }

    void CreateActionButtons()
    {
        bool isPass = correctCount >= locationData.minimum_correct;

        if (isPass)
        {
            // Tombol Lanjut ke lokasi berikutnya
            GameObject btnLanjut = UIBuilder.CreateButton(screenObj.transform,
                "BtnLanjut", "LANJUT ▶",
                new Vector2(800, 130), TOBATheme.BtnPrimary, () => {
                    SoundManager.Instance.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(new MapScreen(container));
                });
            btnLanjut.GetComponent<RectTransform>().anchorMin =
                btnLanjut.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnLanjut.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -560f);

            // Tombol Kembali ke Peta
            GameObject btnPeta = UIBuilder.CreateButton(screenObj.transform,
                "BtnPeta", "KEMBALI KE PETA",
                new Vector2(800, 100), TOBATheme.BtnSecondary, () => {
                    SoundManager.Instance.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(new MapScreen(container));
                });
            btnPeta.GetComponent<RectTransform>().anchorMin =
                btnPeta.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnPeta.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -690f);
        }
        else
        {
            // Tombol Ulangi
            GameObject btnUlangi = UIBuilder.CreateButton(screenObj.transform,
                "BtnUlangi", "🔄 ULANGI",
                new Vector2(800, 130), TOBATheme.Warning, () => {
                    SoundManager.Instance.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(
                        new QuizScreen(container, locationData));
                });
            btnUlangi.GetComponent<RectTransform>().anchorMin =
                btnUlangi.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnUlangi.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -560f);

            // Tombol Kembali ke Peta
            GameObject btnPeta = UIBuilder.CreateButton(screenObj.transform,
                "BtnPeta", "KEMBALI KE PETA",
                new Vector2(800, 100), TOBATheme.BtnSecondary, () => {
                    SoundManager.Instance.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(new MapScreen(container));
                });
            btnPeta.GetComponent<RectTransform>().anchorMin =
                btnPeta.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnPeta.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -690f);
        }
    }

    void SubmitResult()
    {
        if (isSubmitted) return;
        isSubmitted = true;

        int playerID = PlayerPrefs.GetInt("PlayerID");
        bool isPass = correctCount >= locationData.minimum_correct;

        SubmitQuizRequest req = new SubmitQuizRequest
        {
            player_id = playerID,
            location_id = locationData.id,
            correct_count = correctCount
        };

        string json = JsonUtility.ToJson(req);

        APIManager.Instance.PostRequest("/locations/submit", json, (response) => {
            if (screenObj == null) return;

            SubmitQuizResponse res = JsonUtility.FromJson<SubmitQuizResponse>(response);

            // Update status
            if (statusTxt != null)
            {
                statusTxt.text = isPass ? "🎉 LUAR BIASA!" : "😔 BELUM LULUS";
                statusTxt.color = isPass ? TOBATheme.BtnPrimary : TOBATheme.Error;
            }

            // Update XP & bintang
            if (xpTxt != null)
                xpTxt.text = "✨ XP Diperoleh : +" + res.xp;

            if (starsTxt != null)
                starsTxt.text = "⭐ Bintang Diperoleh : +" + res.stars;

            // Simpan data terbaru ke PlayerPrefs
            if (res.level > 0)
                PlayerPrefs.SetInt("PlayerLevel", res.level);

            // Hapus loading
            if (loadingObj != null) Object.Destroy(loadingObj);

            Debug.Log("[QuizResult] Submit berhasil. Status: " + res.status);

        }, (err) => {
            if (screenObj == null) return;

            if (statusTxt != null)
            {
                statusTxt.text = isPass ? "🎉 LUAR BIASA!" : "😔 BELUM LULUS";
                statusTxt.color = isPass ? TOBATheme.BtnPrimary : TOBATheme.Error;
            }

            if (xpTxt != null) xpTxt.text = "✨ XP Diperoleh : (offline)";
            if (starsTxt != null) starsTxt.text = "⭐ Bintang : (offline)";
            if (loadingObj != null) Object.Destroy(loadingObj);

            Debug.LogWarning("[QuizResult] Submit gagal: " + err);
        });
    }
}