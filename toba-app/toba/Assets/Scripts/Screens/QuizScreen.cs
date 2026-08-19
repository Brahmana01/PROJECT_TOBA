using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class QuizScreen : BaseScreen
{
    private LocationData locationData;
    private List<QuizQuestionData> questions = new List<QuizQuestionData>();
    private int currentIndex = 0;
    private int correctCount = 0;
    private int totalQuestions = 0;

    // UI References
    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI questionTxt;
    private TextMeshProUGUI timerTxt;
    private GameObject optionsContainer;
    private GameObject feedbackPanel;

    // Timer
    private float timeLimit = 15f;
    private float timeRemaining;
    private bool isAnswered = false;
    private Coroutine timerCoroutine;

    private GameObject characterGO;

    public QuizScreen(Transform parent, LocationData loc)
        : base(parent, "QuizScreen")
    {
        locationData = loc;
    }

    void CreateCharacterDisplay()
    {
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string baseName = charID == 1 ? "ucok" : "butet";
        string idlePath = charID == 1 ? "char_ucok" : "char_butet";

        Sprite idleSprite = TOBATheme.LoadSprite("gambar/" + baseName + "_berpikir")
                        ?? TOBATheme.LoadSprite(idlePath);

        if (idleSprite != null)
        {
            characterGO = UIBuilder.CreateImage(screenObj.transform, "Character",
                idleSprite, new Vector2(200, 280), new Vector2(440f, -680f));
            characterGO.GetComponent<Image>().preserveAspect = true;
        }
    }

    void UpdateCharacterExpression(bool isCorrect)
    {
        if (characterGO == null) return;
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 1);
        string baseName = charID == 1 ? "ucok" : "butet";
        string exprPath = "gambar/" + baseName + (isCorrect ? "_senang" : "_salah");
        Sprite exprSprite = TOBATheme.LoadSprite(exprPath);
        if (exprSprite != null)
            characterGO.GetComponent<Image>().sprite = exprSprite;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // Header tanpa tombol back (tidak boleh keluar saat quiz)
        HeaderBar.Create(screenObj.transform, locationData.name.ToUpper(), null);

        // Progress bar area
        CreateProgressArea();

        // Area soal
        CreateQuestionArea();

        // Area opsi jawaban
        CreateOptionsArea();

        // Fetch soal
        FetchQuiz();

        CreateCharacterDisplay();
    }

    void CreateProgressArea()
    {
        Sprite woodSprite = TOBATheme.LoadSprite("panel_wood");

        GameObject progressBg = new GameObject("ProgressBg");
        progressBg.transform.SetParent(screenObj.transform, false);
        RectTransform rtPBg = progressBg.AddComponent<RectTransform>();
        rtPBg.anchorMin = new Vector2(0f, 0.82f);
        rtPBg.anchorMax = new Vector2(1f, 0.88f);
        rtPBg.offsetMin = rtPBg.offsetMax = Vector2.zero;
        Image pBgImg = progressBg.AddComponent<Image>();
        pBgImg.sprite = woodSprite ?? UIBuilder.GetDefaultSprite();
        pBgImg.type   = woodSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        pBgImg.color  = new Color(0.28f, 0.14f, 0.04f, 0.95f);
        pBgImg.raycastTarget = false;

        progressTxt = UIBuilder.CreateTextSized(progressBg.transform,
            "Soal 1/" + totalQuestions, 40,
            TOBATheme.TextLight, TextAlignmentOptions.Left,
            new Vector2(420, 65), new Vector2(-230f, 0))
            .GetComponent<TextMeshProUGUI>();
        progressTxt.fontStyle    = FontStyles.Normal;
        progressTxt.raycastTarget = false;

        timerTxt = UIBuilder.CreateTextSized(progressBg.transform,
            "⏱ 15", 40, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(210, 65), new Vector2(355f, 0))
            .GetComponent<TextMeshProUGUI>();
        timerTxt.raycastTarget = false;
    }

    void CreateQuestionArea()
    {
        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");

        // Card pertanyaan
        GameObject qCard = new GameObject("QuestionCard");
        qCard.transform.SetParent(screenObj.transform, false);
        RectTransform rtQC = qCard.AddComponent<RectTransform>();
        rtQC.anchorMin = rtQC.anchorMax = new Vector2(0.5f, 0.5f);
        rtQC.sizeDelta = new Vector2(980, 310);
        rtQC.anchoredPosition = new Vector2(0, 420f);

        Image qImg = qCard.AddComponent<Image>();
        qImg.sprite = panelSprite ?? UIBuilder.GetDefaultSprite();
        qImg.type   = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        qImg.color  = panelSprite != null
            ? new Color(0.95f, 0.88f, 0.72f, 1f)
            : TOBATheme.CardBG;
        qImg.raycastTarget = false;

        Outline qOl = qCard.AddComponent<Outline>();
        qOl.effectColor    = TOBATheme.BatakGold;
        qOl.effectDistance = new Vector2(5, 5);

        Shadow qSh = qCard.AddComponent<Shadow>();
        qSh.effectColor    = new Color(0, 0, 0, 0.5f);
        qSh.effectDistance = new Vector2(5, -5);

        questionTxt = UIBuilder.CreateTextSized(qCard.transform,
            "Memuat soal...", 40, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(920, 270), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        questionTxt.fontStyle         = FontStyles.Normal;
        questionTxt.textWrappingMode  = TextWrappingModes.Normal;
        questionTxt.raycastTarget     = false;
    }
    void CreateOptionsArea()
    {
        optionsContainer = new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rt = optionsContainer.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.2f);
        rt.anchorMax = new Vector2(0.98f, 0.65f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void FetchQuiz()
    {
        APIManager.Instance.GetRequest("/locations/" + locationData.id + "/quiz", (response) => {
            if (screenObj == null) return;

            QuizResponse res = JsonUtility.FromJson<QuizResponse>(response);
            if (res != null && res.questions != null && res.questions.Count > 0)
            {
                questions = res.questions;
                totalQuestions = questions.Count;
                ShowQuestion(0);
            }
            else
            {
                ShowError("Gagal memuat soal.");
            }
        }, (err) => {
            if (screenObj == null) return;
            ShowError("Koneksi gagal.");
        });
    }

    void ShowQuestion(int index)
    {
        if (screenObj == null || index >= questions.Count) return;

        isAnswered = false;
        currentIndex = index;
        QuizQuestionData q = questions[index];

        // Update progress
        if (progressTxt != null)
            progressTxt.text = "Soal " + (index + 1) + "/" + totalQuestions;

        // Update pertanyaan
        if (questionTxt != null)
            questionTxt.text = q.question;

        // Bersihkan opsi lama
        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer.transform)
                Object.Destroy(child.gameObject);
        }

        // Buat opsi jawaban
        string[] optionLabels = { "A", "B", "C", "D" };
        float[] yPositions = { 120f, -20f, -160f, -300f };

        for (int i = 0; i < q.options.Count && i < 4; i++)
        {
            QuizOptionData opt = q.options[i];
            string label = optionLabels[i];
            float yPos = yPositions[i];
            int capturedI = i;

            CreateOptionButton(opt, label, yPos, () => {
                if (!isAnswered) OnAnswerSelected(opt, capturedI, q);
            });
        }

        // Mulai timer
        timeRemaining = timeLimit;
        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);
        timerCoroutine = ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    void CreateOptionButton(QuizOptionData opt, string label, float yPos,
        System.Action onClick)
    {
        Sprite panelSprite = TOBATheme.LoadSprite("panel_wood");

        GameObject optObj = new GameObject("Opt_" + label);
        optObj.transform.SetParent(optionsContainer.transform, false);
        RectTransform rt = optObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960, 128);
        rt.anchoredPosition = new Vector2(0, yPos);

        Image optImg = optObj.AddComponent<Image>();
        optImg.sprite = panelSprite ?? UIBuilder.GetDefaultSprite();
        optImg.type   = panelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        optImg.color  = panelSprite != null
            ? new Color(0.95f, 0.88f, 0.72f, 1f)
            : TOBATheme.OptionNormal;
        optImg.raycastTarget = true;

        Outline optOl = optObj.AddComponent<Outline>();
        optOl.effectColor    = new Color(TOBATheme.BatakGold.r,
                                        TOBATheme.BatakGold.g,
                                        TOBATheme.BatakGold.b, 0.7f);
        optOl.effectDistance = new Vector2(3, 3);

        Shadow optSh = optObj.AddComponent<Shadow>();
        optSh.effectColor    = new Color(0, 0, 0, 0.35f);
        optSh.effectDistance = new Vector2(3, -3);

        // Badge huruf A/B/C/D
        Sprite btnSprite = TOBATheme.LoadSprite("btn_gold");
        GameObject badge = new GameObject("Badge");
        badge.transform.SetParent(optObj.transform, false);
        RectTransform rtBadge = badge.AddComponent<RectTransform>();
        rtBadge.anchorMin = rtBadge.anchorMax = new Vector2(0.5f, 0.5f);
        rtBadge.sizeDelta = new Vector2(90, 90);
        rtBadge.anchoredPosition = new Vector2(-415f, 0);
        Image badgeImg = badge.AddComponent<Image>();
        badgeImg.sprite = btnSprite ?? UIBuilder.GetDefaultSprite();
        badgeImg.type   = btnSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        badgeImg.color  = btnSprite != null
            ? Color.white
            : TOBATheme.WoodDark;
        badgeImg.raycastTarget = false;

        TextMeshProUGUI badgeTmp = UIBuilder.CreateTextSized(badge.transform,
            label, 48, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(90, 90), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        badgeTmp.fontStyle    = FontStyles.Bold;
        badgeTmp.raycastTarget = false;

        // Teks jawaban
        TextMeshProUGUI optTxt = UIBuilder.CreateTextSized(optObj.transform,
            opt.option_text, 36, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(800, 100), new Vector2(60f, 0))
            .GetComponent<TextMeshProUGUI>();
        optTxt.fontStyle         = FontStyles.Normal;
        optTxt.textWrappingMode  = TextWrappingModes.Normal;
        optTxt.raycastTarget     = false;

        // Button
        Button btn = optObj.AddComponent<Button>();
        btn.targetGraphic = optImg;
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
        cb.pressedColor     = new Color(0.85f, 0.75f, 0.4f, 1f);
        btn.colors = cb;

        btn.onClick.AddListener(() => {
            SoundManager.Instance.PlaySFX("sfx_click");
            onClick?.Invoke();
        });

        // Animasi masuk
        ScreenManager.Instance.StartCoroutine(SlideInOption(optObj, yPos));
    }

    IEnumerator SlideInOption(GameObject obj, float targetY)
    {
        if (obj == null) yield break;
        RectTransform rt = obj.GetComponent<RectTransform>();
        CanvasGroup cg = obj.AddComponent<CanvasGroup>();
        float startY = targetY - 60f;
        float elapsed = 0f, dur = 0.28f;
        while (elapsed < dur && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
            cg.alpha = t;
            rt.anchoredPosition = new Vector2(0, Mathf.Lerp(startY, targetY, t));
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.anchoredPosition = new Vector2(0, targetY);
        if (obj != null)
        {
            CanvasGroup cg2 = obj.GetComponent<CanvasGroup>();
            if (cg2 != null) Object.Destroy(cg2);
        }
    }

    void OnAnswerSelected(QuizOptionData selected, int selectedIndex, QuizQuestionData question)
    {
        if (isAnswered) return;
        isAnswered = true;

        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        bool isCorrect = selected.is_correct;
        if (isCorrect)
        {
            correctCount++;
            SoundManager.Instance?.PlaySFX("sfx_correct");
        }
        else
        {
            SoundManager.Instance?.PlaySFX("sfx_wrong");
        }

        // Update ekspresi karakter
        UpdateCharacterExpression(isCorrect);

        HighlightOptions(question, selectedIndex);
        ScreenManager.Instance.StartCoroutine(
            ShowFeedbackThenNext(isCorrect, question.explanation));
    }

    void HighlightOptions(QuizQuestionData question, int selectedIndex)
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
                    else if (i == selectedIndex && !question.options[i].is_correct)
                        img.color = TOBATheme.OptionWrong;
                    else
                        img.color = new Color(TOBATheme.CardBG.r, TOBATheme.CardBG.g,
                            TOBATheme.CardBG.b, 0.5f);

                    // Disable button
                    Button btn = child.GetComponent<Button>();
                    if (btn != null) btn.interactable = false;
                }
            }
            i++;
        }
    }

    IEnumerator ShowFeedbackThenNext(bool isCorrect, string explanation)
    {
        // Panel feedback
        feedbackPanel = UIBuilder.CreatePanelSized(screenObj.transform, "FeedbackPanel",
            isCorrect ? TOBATheme.Success : TOBATheme.Error,
            new Vector2(980, 160), new Vector2(0, -350f));

        string feedbackMsg = isCorrect ? "✅ BENAR!" : "❌ SALAH!";
        UIBuilder.CreateTextSized(feedbackPanel.transform, feedbackMsg,
            44, Color.white, TextAlignmentOptions.Left,
            new Vector2(400, 80), new Vector2(-230f, 20f));

        // Penjelasan
        TextMeshProUGUI expTxt = UIBuilder.CreateTextSized(feedbackPanel.transform,
            explanation, 28, Color.white, TextAlignmentOptions.Left,
            new Vector2(860, 70), new Vector2(0, -35f)).GetComponent<TextMeshProUGUI>();
        expTxt.fontStyle = FontStyles.Normal;
        expTxt.enableWordWrapping = true;

        yield return new WaitForSecondsRealtime(2.5f);

        if (feedbackPanel != null) Object.Destroy(feedbackPanel);

        // Soal berikutnya atau selesai
        int next = currentIndex + 1;
        if (next < questions.Count)
        {
            ShowQuestion(next);
        }
        else
        {
            GoToResult();
        }
    }

    IEnumerator RunTimer()
    {
        timeRemaining = timeLimit;
        while (timeRemaining > 0 && !isAnswered)
        {
            if (timerTxt != null)
            {
                timerTxt.text = "⏱ " + Mathf.CeilToInt(timeRemaining);
                timerTxt.color = timeRemaining <= 5f ? TOBATheme.Error : TOBATheme.BatakGold;
            }
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Waktu habis
        if (!isAnswered && screenObj != null)
        {
            isAnswered = true;
            QuizQuestionData q = questions[currentIndex];
            HighlightOptions(q, -1);
            ScreenManager.Instance.StartCoroutine(
                ShowFeedbackThenNext(false, "Waktu habis! " + q.explanation));
        }
    }

    void GoToResult()
    {
        ScreenManager.Instance.ChangeScreen(
            new QuizResultScreen(container, locationData, correctCount, totalQuestions));
    }

    void ShowError(string msg)
    {
        if (questionTxt != null) questionTxt.text = msg;
    }
}