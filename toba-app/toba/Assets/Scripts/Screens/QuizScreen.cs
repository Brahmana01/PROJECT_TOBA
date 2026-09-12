using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuizScreen : BaseScreen
{
    private LocationData locationData;
    private List<QuizQuestionData> questions = new List<QuizQuestionData>();
    private int currentIndex  = 0;
    private int correctCount  = 0;
    private int totalQuestions = 0;

    // UI refs
    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI questionTxt;
    private TextMeshProUGUI timerTxt;
    private GameObject      optionsContainer;
    private GameObject      feedbackPanel;
    private GameObject      characterGO;
    private Image           timerBg;

    // Timer
    private float timeLimit    = 15f;
    private float timeRemaining;
    private bool  isAnswered   = false;
    private Coroutine timerCo;

    public QuizScreen(Transform parent, LocationData loc)
        : base(parent, "QuizScreen")
    {
        locationData = loc;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0f, 0.04f, 0f, 0.5f), Vector2.zero, Vector2.one);

        // Header (tanpa tombol back — tidak bisa keluar saat quiz)
        HeaderBar.Create(screenObj.transform, locationData.name.ToUpper(), null);

        CreateProgressArea();
        CreateQuestionArea();
        CreateOptionsArea();
        CreateCharacter();
        FetchQuiz();
    }

    // ── PROGRESS ──────────────────────────────────────────
    void CreateProgressArea()
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        GameObject bg = new GameObject("ProgressBg");
        bg.transform.SetParent(screenObj.transform, false);
        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.82f);
        rt.anchorMax = new Vector2(1f, 0.88f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        Image img = bg.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color  = new Color(0.22f, 0.10f, 0.03f, 0.97f);
        img.raycastTarget = false;

        progressTxt = UIBuilder.CreateTextSized(bg.transform,
            "Soal 1/" + totalQuestions, 40,
            TOBATheme.TextLight, TextAlignmentOptions.Left,
            new Vector2(420, 65), new Vector2(-235f, 0))
            .GetComponent<TextMeshProUGUI>();
        progressTxt.fontStyle    = FontStyles.Normal;
        progressTxt.raycastTarget = false;

        // Timer background
        timerBg = UIBuilder.CreatePanelSized(bg.transform, "TimerBg",
            new Color(0, 0, 0, 0.35f), new Vector2(180, 65), new Vector2(355f, 0))
            .GetComponent<Image>();
        timerBg.raycastTarget = false;
        timerBg.sprite = UIBuilder.GetDefaultSprite();

        timerTxt = UIBuilder.CreateTextSized(timerBg.gameObject.transform,
            "⏱ 15", 38, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(175, 60), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        timerTxt.fontStyle    = FontStyles.Bold;
        timerTxt.raycastTarget = false;
    }

    // ── PERTANYAAN ────────────────────────────────────────
    void CreateQuestionArea()
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        GameObject card = new GameObject("QuestionCard");
        card.transform.SetParent(screenObj.transform, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(980, 310);
        rt.anchoredPosition = new Vector2(0, 418f);
        Image img = card.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color  = wp != null
            ? new Color(0.95f, 0.87f, 0.70f, 1f)
            : TOBATheme.CardBG;
        img.raycastTarget = false;
        card.AddComponent<Outline>().effectColor    = TOBATheme.BatakGold;
        card.AddComponent<Shadow>().effectDistance  = new Vector2(5, -5);
        ((Shadow)card.GetComponents<Component>()[^1]).effectColor =
            new Color(0, 0, 0, 0.5f);

        questionTxt = UIBuilder.CreateTextSized(card.transform,
            "Memuat soal...", 40, TOBATheme.TextDark,
            TextAlignmentOptions.Center, new Vector2(920, 270), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        questionTxt.fontStyle        = FontStyles.Normal;
        questionTxt.textWrappingMode = TextWrappingModes.Normal;
        questionTxt.raycastTarget    = false;
    }

    // ── OPSI ──────────────────────────────────────────────
    void CreateOptionsArea()
    {
        optionsContainer = new GameObject("Options");
        optionsContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rt = optionsContainer.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.18f);
        rt.anchorMax = new Vector2(0.98f, 0.64f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── KARAKTER ──────────────────────────────────────────
    void CreateCharacter()
    {
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string path = charID == 1
            ? "gambar/ucok_berpikir"
            : "gambar/butet_berpikir";
        Sprite sp = TOBATheme.LoadSprite(path)
                 ?? TOBATheme.LoadSprite(charID == 1 ? "char_ucok" : "char_butet");
        if (sp == null) return;

        characterGO = UIBuilder.CreateImage(screenObj.transform,
            "Character", sp, new Vector2(200, 280), new Vector2(440f, -680f));
        characterGO.GetComponent<Image>().preserveAspect = true;
    }

    void SetCharacterExpression(bool correct)
    {
        if (characterGO == null) return;
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string b = charID == 1 ? "ucok" : "butet";
        Sprite sp = TOBATheme.LoadSprite("gambar/" + b + (correct ? "_senang" : "_salah"))
                 ?? TOBATheme.LoadSprite(charID == 1 ? "char_ucok" : "char_butet");
        if (sp != null) characterGO.GetComponent<Image>().sprite = sp;
    }

    // ── FETCH ─────────────────────────────────────────────
    void FetchQuiz()
    {
        // Tampilkan loading saat fetch soal
        LoadingOverlay.Show(screenObj.transform, "Memuat soal...");

        string cached = CacheManager.GetCachedLocationQuiz(locationData.id);

        APIManager.Instance.GetRequestWithCache(
            "/locations/" + locationData.id + "/quiz",
            cached,
            (response) => {
                LoadingOverlay.Hide();
                if (screenObj == null) return;

                CacheManager.CacheLocationQuiz(locationData.id, response);
                QuizResponse res = JsonUtility.FromJson<QuizResponse>(response);

                if (res?.questions != null && res.questions.Count > 0)
                {
                    questions      = res.questions;
                    totalQuestions = questions.Count;
                    if (progressTxt)
                        progressTxt.text = "Soal 1/" + totalQuestions;
                    ShowQuestion(0);
                }
                else
                {
                    if (questionTxt)
                        questionTxt.text = "Soal belum tersedia untuk lokasi ini.";
                    NetworkManager.Instance?.ShowToast(
                        "Soal belum tersedia", true);
                }
            },
            (err) => {
                LoadingOverlay.Hide();
                if (screenObj == null) return;
                if (questionTxt)
                    questionTxt.text = "Gagal memuat soal.\n" + err;
                NetworkManager.Instance?.ShowToast(err, true);
            });
    }
    
    // ── TAMPILKAN SOAL ────────────────────────────────────
    void ShowQuestion(int index)
    {
        if (screenObj == null || index >= questions.Count) return;
        isAnswered    = false;
        currentIndex  = index;
        var q = questions[index];

        if (progressTxt) progressTxt.text = "Soal " + (index + 1) + "/" + totalQuestions;
        if (questionTxt)
        {
            questionTxt.text = q.question;
            ScreenManager.Instance.StartCoroutine(
                UIBuilder.AnimateScaleIn(questionTxt.transform, 0.2f));
        }

        // Bersihkan opsi lama
        if (optionsContainer)
            foreach (Transform c in optionsContainer.transform)
                Object.Destroy(c.gameObject);

        // Karakter ekspresi kembali ke berpikir
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string b = charID == 1 ? "ucok" : "butet";
        Sprite thinkSprite = TOBATheme.LoadSprite("gambar/" + b + "_berpikir");
        if (characterGO != null && thinkSprite != null)
            characterGO.GetComponent<Image>().sprite = thinkSprite;

        string[] labels = { "A", "B", "C", "D" };
        float[] yPos    = { 120f, -20f, -160f, -300f };

        for (int i = 0; i < q.options.Count && i < 4; i++)
        {
            var opt = q.options[i];
            int ci  = i;
            CreateOptionButton(opt, labels[i], yPos[i], () => {
                if (!isAnswered) OnAnswerSelected(opt, ci, q);
            });
        }

        // Timer
        timeRemaining = timeLimit;
        if (timerCo != null) ScreenManager.Instance.StopCoroutine(timerCo);
        timerCo = ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    // ── OPSI BUTTON ───────────────────────────────────────
    void CreateOptionButton(QuizOptionData opt, string label,
        float yPos, System.Action onClick)
    {
        Sprite wp  = TOBATheme.LoadSprite("panel_wood");
        Sprite btn = TOBATheme.LoadSprite("btn_gold");

        GameObject optGO = new GameObject("Opt_" + label);
        optGO.transform.SetParent(optionsContainer.transform, false);
        RectTransform rt = optGO.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960, 125);
        rt.anchoredPosition = new Vector2(0, yPos);

        Image img = optGO.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color  = wp != null
            ? new Color(0.96f, 0.88f, 0.72f, 1f)
            : TOBATheme.OptionNormal;
        img.raycastTarget = true;

        Outline ol = optGO.AddComponent<Outline>();
        ol.effectColor    = new Color(TOBATheme.BatakGold.r,
                                      TOBATheme.BatakGold.g,
                                      TOBATheme.BatakGold.b, 0.7f);
        ol.effectDistance = new Vector2(3, 3);
        optGO.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);
        ((Shadow)optGO.GetComponents<Component>()[^1]).effectColor =
            new Color(0, 0, 0, 0.3f);

        // Badge A/B/C/D
        GameObject badge = new GameObject("Badge");
        badge.transform.SetParent(optGO.transform, false);
        RectTransform rtB = badge.AddComponent<RectTransform>();
        rtB.anchorMin = rtB.anchorMax = new Vector2(0.5f, 0.5f);
        rtB.sizeDelta = new Vector2(90, 90);
        rtB.anchoredPosition = new Vector2(-415f, 0);
        Image bImg = badge.AddComponent<Image>();
        bImg.sprite = btn ?? UIBuilder.GetDefaultSprite();
        bImg.type   = btn != null ? Image.Type.Sliced : Image.Type.Simple;
        bImg.color  = btn != null ? Color.white : TOBATheme.WoodDark;
        bImg.raycastTarget = false;
        TextMeshProUGUI bTmp = UIBuilder.CreateTextSized(badge.transform,
            label, 50, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(90, 90), Vector2.zero).GetComponent<TextMeshProUGUI>();
        bTmp.fontStyle = FontStyles.Bold; bTmp.raycastTarget = false;

        // Teks jawaban
        TextMeshProUGUI optTxt = UIBuilder.CreateTextSized(optGO.transform,
            opt.option_text, 36, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(800, 100), new Vector2(60f, 0))
            .GetComponent<TextMeshProUGUI>();
        optTxt.fontStyle        = FontStyles.Normal;
        optTxt.textWrappingMode = TextWrappingModes.Normal;
        optTxt.raycastTarget    = false;

        Button b2 = optGO.AddComponent<Button>();
        b2.targetGraphic = img;
        ColorBlock cb = b2.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 0.82f, 1f);
        cb.pressedColor     = new Color(0.85f, 0.75f, 0.4f, 1f);
        b2.colors = cb;
        b2.onClick.AddListener(() => {
            SoundManager.Instance?.PlaySFX("sfx_click");
            onClick?.Invoke();
        });

        // Slide-in animation
        ScreenManager.Instance.StartCoroutine(SlideIn(optGO, yPos));
    }

    IEnumerator SlideIn(GameObject go, float targetY)
    {
        if (go == null) yield break;
        var rt = go.GetComponent<RectTransform>();
        var cg = go.AddComponent<CanvasGroup>();
        float start = targetY - 55f, t = 0f;
        while (t < 0.25f && go != null)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.25f);
            cg.alpha = p;
            rt.anchoredPosition = new Vector2(0, Mathf.Lerp(start, targetY, p));
            yield return null;
        }
        if (cg != null) Object.Destroy(cg);
        if (rt != null) rt.anchoredPosition = new Vector2(0, targetY);
    }

    // ── JAWABAN DIPILIH ───────────────────────────────────
    void OnAnswerSelected(QuizOptionData selected, int selIdx,
        QuizQuestionData question)
    {
        if (isAnswered) return;
        isAnswered = true;
        if (timerCo != null) ScreenManager.Instance.StopCoroutine(timerCo);

        bool correct = selected.is_correct;
        if (correct) correctCount++;

        // Haptic
        HapticManager.Instance?.Vibrate(
            correct ? HapticType.Success : HapticType.Error);

        // SFX
        SoundManager.Instance?.PlaySFX(correct ? "sfx_correct" : "sfx_wrong");

        // Ekspresi karakter
        SetCharacterExpression(correct);

        HighlightOptions(question, selIdx);
        ScreenManager.Instance.StartCoroutine(
            ShowFeedbackThenNext(correct, question.explanation));
    }

    // ── HIGHLIGHT OPSI ────────────────────────────────────
    void HighlightOptions(QuizQuestionData question, int selIdx)
    {
        if (optionsContainer == null) return;
        int i = 0;
        foreach (Transform child in optionsContainer.transform)
        {
            if (i < question.options.Count)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    if (question.options[i].is_correct)
                        img.color = TOBATheme.OptionCorrect;
                    else if (i == selIdx)
                        img.color = TOBATheme.OptionWrong;
                    else
                        img.color = new Color(TOBATheme.CardBG.r,
                            TOBATheme.CardBG.g, TOBATheme.CardBG.b, 0.45f);

                    Button btn = child.GetComponent<Button>();
                    if (btn) btn.interactable = false;
                }
            }
            i++;
        }
    }

    // ── FEEDBACK ──────────────────────────────────────────
    IEnumerator ShowFeedbackThenNext(bool correct, string explanation)
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        feedbackPanel = new GameObject("Feedback");
        feedbackPanel.transform.SetParent(screenObj.transform, false);
        RectTransform rtFB = feedbackPanel.AddComponent<RectTransform>();
        rtFB.anchorMin = rtFB.anchorMax = new Vector2(0.5f, 0.5f);
        rtFB.sizeDelta = new Vector2(980, 175);
        rtFB.anchoredPosition = new Vector2(0, -350f);
        feedbackPanel.transform.localScale = Vector3.zero;

        Image fbImg = feedbackPanel.AddComponent<Image>();
        fbImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        fbImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        fbImg.color  = correct
            ? new Color(0.1f, 0.55f, 0.1f, 0.97f)
            : new Color(0.65f, 0.1f, 0.1f, 0.97f);
        fbImg.raycastTarget = false;

        UIBuilder.CreateTextSized(feedbackPanel.transform,
            correct ? "✅ BENAR!" : "❌ SALAH!",
            48, Color.white, TextAlignmentOptions.Left,
            new Vector2(420, 85), new Vector2(-235f, 22f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;

        TextMeshProUGUI expTmp = UIBuilder.CreateTextSized(feedbackPanel.transform,
            explanation, 28, Color.white, TextAlignmentOptions.Left,
            new Vector2(860, 70), new Vector2(0, -36f))
            .GetComponent<TextMeshProUGUI>();
        expTmp.fontStyle        = FontStyles.Normal;
        expTmp.textWrappingMode = TextWrappingModes.Normal;
        expTmp.raycastTarget    = false;

        // Animasi masuk
        yield return UIBuilder.AnimateScaleIn(feedbackPanel.transform, 0.2f);
        yield return new WaitForSecondsRealtime(2.3f);

        if (feedbackPanel) Object.Destroy(feedbackPanel);

        int next = currentIndex + 1;
        if (next < questions.Count) ShowQuestion(next);
        else GoToResult();
    }

    // ── TIMER ─────────────────────────────────────────────
    IEnumerator RunTimer()
    {
        while (timeRemaining > 0 && !isAnswered)
        {
            if (timerTxt)
            {
                timerTxt.text  = "⏱ " + Mathf.CeilToInt(timeRemaining);
                bool warn = timeRemaining <= 5f;
                timerTxt.color = warn ? TOBATheme.Error : TOBATheme.BatakGold;
                if (timerBg) timerBg.color = warn
                    ? new Color(0.5f, 0.05f, 0.05f, 0.6f)
                    : new Color(0, 0, 0, 0.35f);
                if (warn) SoundManager.Instance?.PlaySFX("sfx_timer_warning");
            }
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (!isAnswered && screenObj != null)
        {
            isAnswered = true;
            HapticManager.Instance?.Vibrate(HapticType.Error);
            var q = questions[currentIndex];
            HighlightOptions(q, -1);
            SetCharacterExpression(false);
            ScreenManager.Instance.StartCoroutine(
                ShowFeedbackThenNext(false, "Waktu habis! " + q.explanation));
        }
    }

    void GoToResult()
    {
        ScreenManager.Instance.ChangeScreen(
            new QuizResultScreen(container, locationData, correctCount, totalQuestions));
    }

    public override void Hide()
    {
        if (timerCo != null) ScreenManager.Instance.StopCoroutine(timerCo);
        base.Hide();
    }
}