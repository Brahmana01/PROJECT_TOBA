using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OnboardingScreen : BaseScreen
{
    private int currentSlide = 0;
    private GameObject[] slides;
    private GameObject[] dots;

    private static readonly (string emoji, string title, string desc)[] SLIDES =
    {
        ("	◆",
         "Jelajah Budaya",
         "Kunjungi 10 lokasi bersejarah Batak Toba.\nJawab kuis dan kumpulkan bintang!"),

        ("📚",
         "Trivia Budaya",
         "Uji pengetahuanmu tentang budaya Batak!\n5 kategori, 50+ soal seru menanti."),

        ("<color=#A83232><sprite name=\"battle\" tint=1></color>",
         "Battle Quiz",
         "Tantang temanmu dalam duel soal real-time!\nSiapa yang tercepat dan terpintar?"),

        ("<color=#D6A84B><sprite name=\"leaderboard\" tint=1></color>",
         "Kumpulkan Reward",
         "Naik level, raih gelar, dan buka galeri kartu budaya Batak Toba!"),
    };

    public OnboardingScreen(Transform parent) : base(parent, "Onboarding") { }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);

        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0.05f, 0f, 0.5f),
            Vector2.zero,
            Vector2.one
        );

        // Skip button
        GameObject btnSkip = UIBuilder.CreateSmallButton(
            screenObj.transform,
            "BtnSkip",
            "LEWATI →",
            new Vector2(200, 65),
            new Color(0, 0, 0, 0.4f),
            TOBATheme.TextGray,
            FinishOnboarding
        );

        RectTransform rtSkip = btnSkip.GetComponent<RectTransform>();
        rtSkip.anchorMin = rtSkip.anchorMax = new Vector2(1f, 1f);
        rtSkip.anchoredPosition = new Vector2(-130f, -80f);

        // Slides
        slides = new GameObject[SLIDES.Length];

        for (int i = 0; i < SLIDES.Length; i++)
        {
            slides[i] = BuildSlide(i);
            slides[i].SetActive(i == 0);

            CanvasGroup slideCg = slides[i].GetComponent<CanvasGroup>();

            if (slideCg != null)
                slideCg.alpha = i == 0 ? 1f : 0f;
        }

        // Dots
        BuildDots();

        // Navigation buttons
        BuildNavButtons();

        // Fade in seluruh screen
        CanvasGroup cg = screenObj.GetComponent<CanvasGroup>();

        if (cg == null)
            cg = screenObj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        ScreenManager.Instance.StartCoroutine(
            UIBuilder.AnimateFadeIn(cg, 0.5f)
        );
    }

    GameObject BuildSlide(int i)
    {
        var (emoji, title, desc) = SLIDES[i];

        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        GameObject slide = new GameObject("Slide_" + i);
        slide.transform.SetParent(screenObj.transform, false);

        RectTransform rt = slide.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.2f);
        rt.anchorMax = new Vector2(0.95f, 0.82f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = slide.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color = new Color(0.25f, 0.12f, 0.04f, 0.97f);
        img.raycastTarget = false;

        Outline ol = slide.AddComponent<Outline>();
        ol.effectColor = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(5, 5);

        Shadow sh = slide.AddComponent<Shadow>();
        sh.effectColor = new Color(0, 0, 0, 0.6f);
        sh.effectDistance = new Vector2(6, -6);

        // CanvasGroup wajib ada di setiap slide
        CanvasGroup canvasGroup = slide.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = false;

        // Emoji besar
        UIBuilder.CreateTextSized(
            slide.transform,
            emoji,
            140,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(400, 180),
            new Vector2(0, 140f)
        ).GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Nomor slide
        TextMeshProUGUI numTmp = UIBuilder.CreateTextSized(
            slide.transform,
            $"{i + 1} / {SLIDES.Length}",
            26,
            TOBATheme.TextGray,
            TextAlignmentOptions.Center,
            new Vector2(200, 40),
            new Vector2(0, 30f)
        ).GetComponent<TextMeshProUGUI>();

        numTmp.fontStyle = FontStyles.Normal;
        numTmp.raycastTarget = false;

        // Garis emas
        UIBuilder.CreatePanelSized(
            slide.transform,
            "Line",
            TOBATheme.BatakGold,
            new Vector2(340, 3),
            new Vector2(0, -10f)
        ).GetComponent<Image>().raycastTarget = false;

        // Judul
        TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(
            slide.transform,
            title,
            58,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(850, 80),
            new Vector2(0, -75f)
        ).GetComponent<TextMeshProUGUI>();

        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.raycastTarget = false;

        // Deskripsi
        TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(
            slide.transform,
            desc,
            34,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(820, 150),
            new Vector2(0, -190f)
        ).GetComponent<TextMeshProUGUI>();

        descTmp.fontStyle = FontStyles.Normal;
        descTmp.textWrappingMode = TextWrappingModes.Normal;
        descTmp.raycastTarget = false;
        descTmp.lineSpacing = 10f;

        return slide;
    }

    void BuildDots()
    {
        dots = new GameObject[SLIDES.Length];

        float spacing = 55f;
        float totalW = (SLIDES.Length - 1) * spacing;

        for (int i = 0; i < SLIDES.Length; i++)
        {
            float xPos = -(totalW / 2f) + i * spacing;

            GameObject dot = new GameObject("Dot_" + i);
            dot.transform.SetParent(screenObj.transform, false);

            RectTransform rt = dot.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(i == 0 ? 34 : 22, 22);
            rt.anchoredPosition = new Vector2(xPos, -820f);

            Image img = dot.AddComponent<Image>();
            img.sprite = UIBuilder.GetDefaultSprite();
            img.color = i == 0
                ? TOBATheme.BatakGold
                : new Color(1f, 1f, 1f, 0.3f);

            img.raycastTarget = false;

            dots[i] = dot;
        }
    }

    void BuildNavButtons()
    {
        UIBuilder.CreateButton(
            screenObj.transform,
            "BtnNext",
            "LANJUT →",
            new Vector2(700, 135),
            TOBATheme.BtnPrimary,
            OnNext
        ).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -720f);
    }

    void OnNext()
    {
        SoundManager.Instance?.PlaySFX("sfx_click");

        if (currentSlide < SLIDES.Length - 1)
        {
            ScreenManager.Instance.StartCoroutine(
                TransitionSlide(currentSlide + 1)
            );
        }
        else
        {
            FinishOnboarding();
        }
    }

    IEnumerator TransitionSlide(int nextIdx)
    {
        // ==========================
        // FADE OUT SLIDE LAMA
        // ==========================

        CanvasGroup cgOld = slides[currentSlide].GetComponent<CanvasGroup>();

        if (cgOld == null)
            cgOld = slides[currentSlide].AddComponent<CanvasGroup>();

        float t = 0f;

        while (t < 0.18f)
        {
            t += Time.deltaTime;
            cgOld.alpha = 1f - t / 0.18f;
            yield return null;
        }

        cgOld.alpha = 0f;
        slides[currentSlide].SetActive(false);

        // ==========================
        // PINDAH SLIDE
        // ==========================

        currentSlide = nextIdx;

        slides[currentSlide].SetActive(true);

        // ==========================
        // UPDATE DOTS
        // ==========================

        for (int i = 0; i < dots.Length; i++)
        {
            Image dImg = dots[i].GetComponent<Image>();
            RectTransform dRt = dots[i].GetComponent<RectTransform>();

            dImg.color = i == currentSlide
                ? TOBATheme.BatakGold
                : new Color(1f, 1f, 1f, 0.3f);

            dRt.sizeDelta = new Vector2(
                i == currentSlide ? 34 : 22,
                22
            );
        }

        // ==========================
        // UPDATE TEKS TOMBOL
        // ==========================

        Transform btnNext = screenObj.transform.Find("BtnNext");

        if (btnNext != null)
        {
            TextMeshProUGUI btnTmp =
                btnNext.GetComponentInChildren<TextMeshProUGUI>();

            if (btnTmp != null)
            {
                btnTmp.text =
                    currentSlide == SLIDES.Length - 1
                        ? "MULAI! 🎮"
                        : "LANJUT →";
            }
        }

        // ==========================
        // FADE IN SLIDE BARU
        // ==========================

        CanvasGroup cgNew =
            slides[currentSlide].GetComponent<CanvasGroup>();

        if (cgNew == null)
            cgNew = slides[currentSlide].AddComponent<CanvasGroup>();

        cgNew.alpha = 0f;

        t = 0f;

        while (t < 0.22f)
        {
            t += Time.deltaTime;
            cgNew.alpha = t / 0.22f;
            yield return null;
        }

        cgNew.alpha = 1f;
    }

    void FinishOnboarding()
    {
        PlayerPrefs.SetInt("OnboardingDone", 1);
        PlayerPrefs.Save();

        SoundManager.Instance?.PlaySFX("sfx_win");

        ScreenManager.Instance.ChangeScreen(
            new WelcomeScreen(container)
        );
    }
}