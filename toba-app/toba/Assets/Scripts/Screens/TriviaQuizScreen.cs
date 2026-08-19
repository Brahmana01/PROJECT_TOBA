using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TriviaQuizScreen : BaseScreen
{
    private TriviaCategoryData categoryData;
    private List<QuizQuestionData> questions = new List<QuizQuestionData>();
    private int currentIndex = 0;
    private int correctCount = 0;

    // UI
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

    public TriviaQuizScreen(Transform parent, TriviaCategoryData cat)
        : base(parent, "TriviaQuiz")
    {
        categoryData = cat;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // Header tanpa tombol back (tidak bisa keluar saat kuis)
        HeaderBar.Create(screenObj.transform, categoryData.name.ToUpper(), null);

        // Progress area
        CreateProgressArea();

        // Area soal
        CreateQuestionArea();

        // Area opsi
        CreateOptionsArea();

        // Fetch soal
        FetchQuiz();
    }

    void CreateProgressArea()
    {
        GameObject progressBg = UIBuilder.CreatePanel(screenObj.transform, "ProgressBg",
            TOBATheme.WoodDark, new Vector2(0, 0.82f), new Vector2(1, 0.88f));

        progressTxt = UIBuilder.CreateTextSized(progressBg.transform,
            "Soal 1/" + categoryData.question_count,
            38, TOBATheme.TextLight, TextAlignmentOptions.Left,
            new Vector2(400, 60), new Vector2(-250f, 0))
            .GetComponent<TextMeshProUGUI>();

        timerTxt = UIBuilder.CreateTextSized(progressBg.transform,
            "⏱ 15", 38, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(200, 60), new Vector2(350f, 0))
            .GetComponent<TextMeshProUGUI>();
    }

    void CreateQuestionArea()
    {
        GameObject questionCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "QuestionCard", TOBATheme.CardBG, new Vector2(980, 280), new Vector2(0, 430f));

        Outline outline = questionCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4, 4);

        questionTxt = UIBuilder.CreateTextSized(questionCard.transform,
            "Memuat soal...", 38, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(920, 240), Vector2.zero)
            .GetComponent<TextMeshProUGUI>();
        questionTxt.fontStyle = FontStyles.Normal;
        questionTxt.enableWordWrapping = true;
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
        APIManager.Instance.GetRequest("/trivia/" + categoryData.id + "/quiz", (response) => {
            if (screenObj == null) return;

            TriviaQuizResponse res = JsonUtility.FromJson<TriviaQuizResponse>(response);
            if (res != null && res.questions != null && res.questions.Count > 0)
            {
                questions = res.questions;
                ShowQuestion(0);
            }
            else
            {
                if (questionTxt != null) questionTxt.text = "Soal belum tersedia.";
            }
        }, (err) => {
            if (questionTxt != null) questionTxt.text = "Gagal memuat soal.";
        });
    }

    void ShowQuestion(int index)
    {
        if (screenObj == null || index >= questions.Count) return;

        isAnswered = false;
        currentIndex = index;
        QuizQuestionData q = questions[index];

        if (progressTxt != null)
            progressTxt.text = "Soal " + (index + 1) + "/" + questions.Count;

        if (questionTxt != null)
            questionTxt.text = q.question;

        // Bersihkan opsi lama
        if (optionsContainer != null)
            foreach (Transform child in optionsContainer.transform)
                Object.Destroy(child.gameObject);

        // Buat opsi
        string[] labels = { "A", "B", "C", "D" };
        float[] yPos = { 120f, -20f, -160f, -300f };

        for (int i = 0; i < q.options.Count && i < 4; i++)
        {
            QuizOptionData opt = q.options[i];
            int capturedI = i;
            CreateOptionButton(opt, labels[i], yPos[i], () => {
                if (!isAnswered) OnAnswerSelected(opt, capturedI, q);
            });
        }

        // Timer
        timeRemaining = timeLimit;
        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);
        timerCoroutine = ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    void CreateOptionButton(QuizOptionData opt, string label, float yPos, System.Action onClick)
    {
        GameObject optObj = new GameObject("Opt_" + label);
        optObj.transform.SetParent(optionsContainer.transform, false);
        RectTransform rt = optObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960, 120);
        rt.anchoredPosition = new Vector2(0, yPos);

        Image img = optObj.AddComponent<Image>();
        img.color = TOBATheme.CardBG;
        img.raycastTarget = true;

        Outline outline = optObj.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        // Label A/B/C/D
        GameObject labelCircle = UIBuilder.CreatePanelSized(optObj.transform,
            "LabelCircle", TOBATheme.WoodDark, new Vector2(90, 90), new Vector2(-410f, 0));
        UIBuilder.CreateTextSized(labelCircle.transform, label, 44,
            TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(90, 90), Vector2.zero);

        // Teks jawaban
        TextMeshProUGUI optTxt = UIBuilder.CreateTextSized(optObj.transform,
            opt.option_text, 36, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(800, 100), new Vector2(60f, 0))
            .GetComponent<TextMeshProUGUI>();
        optTxt.fontStyle = FontStyles.Normal;
        optTxt.enableWordWrapping = true;

        Button btn = optObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            SoundManager.Instance.PlaySFX("sfx_click");
            onClick?.Invoke();
        });
    }

    void OnAnswerSelected(QuizOptionData selected, int selectedIndex, QuizQuestionData question)
    {
        if (isAnswered) return;
        isAnswered = true;

        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        if (selected.is_correct) correctCount++;

        HighlightOptions(question, selectedIndex);
        ScreenManager.Instance.StartCoroutine(
            ShowFeedbackThenNext(selected.is_correct, question.explanation));
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
                    else if (i == selectedIndex)
                        img.color = TOBATheme.OptionWrong;
                    else
                        img.color = new Color(TOBATheme.CardBG.r, TOBATheme.CardBG.g,
                            TOBATheme.CardBG.b, 0.5f);

                    Button btn = child.GetComponent<Button>();
                    if (btn != null) btn.interactable = false;
                }
            }
            i++;
        }
    }

    IEnumerator ShowFeedbackThenNext(bool isCorrect, string explanation)
    {
        feedbackPanel = UIBuilder.CreatePanelSized(screenObj.transform, "FeedbackPanel",
            isCorrect ? TOBATheme.Success : TOBATheme.Error,
            new Vector2(980, 160), new Vector2(0, -350f));

        UIBuilder.CreateTextSized(feedbackPanel.transform,
            isCorrect ? "✅ BENAR!" : "❌ SALAH!",
            44, Color.white, TextAlignmentOptions.Left,
            new Vector2(400, 80), new Vector2(-230f, 20f));

        TextMeshProUGUI expTxt = UIBuilder.CreateTextSized(feedbackPanel.transform,
            explanation, 28, Color.white, TextAlignmentOptions.Left,
            new Vector2(860, 70), new Vector2(0, -35f))
            .GetComponent<TextMeshProUGUI>();
        expTxt.fontStyle = FontStyles.Normal;
        expTxt.enableWordWrapping = true;

        yield return new WaitForSecondsRealtime(2.5f);

        if (feedbackPanel != null) Object.Destroy(feedbackPanel);

        int next = currentIndex + 1;
        if (next < questions.Count)
            ShowQuestion(next);
        else
            GoToResult();
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
            new TriviaResultScreen(container, categoryData, correctCount, questions.Count));
    }
}