using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BattleQuizScreen : BaseScreen
{
    private BattleRoomStatusResponse roomData;
    private bool isHost;
    private int playerID;

    private List<QuizQuestionData> questions = new List<QuizQuestionData>();
    private int currentIndex = 0;
    private int myScore = 0;
    private int opponentScore = 0;

    // UI
    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI questionTxt;
    private TextMeshProUGUI timerTxt;
    private TextMeshProUGUI myScoreTxt;
    private TextMeshProUGUI oppScoreTxt;
    private TextMeshProUGUI myNameTxt;
    private TextMeshProUGUI oppNameTxt;
    private GameObject optionsContainer;
    private GameObject feedbackPanel;

    private float timeLimit = 15f;
    private float timeRemaining;
    private bool isAnswered = false;
    private Coroutine timerCoroutine;

    public BattleQuizScreen(Transform parent, BattleRoomStatusResponse room, bool host)
        : base(parent, "BattleQuiz")
    {
        roomData = room;
        isHost = host;
        playerID = PlayerPrefs.GetInt("PlayerID");
    }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // Header tanpa back
        HeaderBar.Create(screenObj.transform, "BATTLE QUIZ", null);

        // Scoreboard area
        CreateScoreBoard();

        // Progress
        CreateProgressArea();

        // Soal
        CreateQuestionArea();

        // Opsi
        CreateOptionsArea();

        // Load soal dari roomData
        if (roomData.questions != null && roomData.questions.Count > 0)
        {
            questions = roomData.questions;
            ShowQuestion(0);
        }
        else
        {
            if (questionTxt != null)
                questionTxt.text = "Gagal memuat soal battle.";
        }
    }

    void CreateScoreBoard()
    {
        // Panel scoreboard
        GameObject scorePanel = UIBuilder.CreatePanel(
            screenObj.transform,
            "ScorePanel",
            TOBATheme.WoodDark,
            new Vector2(0, 0.78f),
            new Vector2(1, 0.88f)
        );

        string myName = isHost ? roomData.host_name : roomData.guest_name;
        string oppName = isHost ? roomData.guest_name : roomData.host_name;

        // Nama & skor pemain sendiri (kiri)
        myNameTxt = UIBuilder.CreateTextSized(
            scorePanel.transform,
            myName,
            34,
            TOBATheme.TextLight,
            TextAlignmentOptions.Left,
            new Vector2(380, 55),
            new Vector2(-230f, 20f)
        ).GetComponent<TextMeshProUGUI>();

        myNameTxt.fontStyle = FontStyles.Normal;

        myScoreTxt = UIBuilder.CreateTextSized(
            scorePanel.transform,
            "0",
            55,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Left,
            new Vector2(120, 70),
            new Vector2(-230f, -25f)
        ).GetComponent<TextMeshProUGUI>();

        // VS di tengah
        UIBuilder.CreateTextSized(
            scorePanel.transform,
            "VS",
            50,
            TOBATheme.Error,
            TextAlignmentOptions.Center,
            new Vector2(120, 70),
            new Vector2(0, 0)
        );

        // Nama & skor lawan (kanan)
        oppNameTxt = UIBuilder.CreateTextSized(
            scorePanel.transform,
            oppName,
            34,
            TOBATheme.TextLight,
            TextAlignmentOptions.Right,
            new Vector2(380, 55),
            new Vector2(230f, 20f)
        ).GetComponent<TextMeshProUGUI>();

        oppNameTxt.fontStyle = FontStyles.Normal;

        oppScoreTxt = UIBuilder.CreateTextSized(
            scorePanel.transform,
            "0",
            55,
            TOBATheme.TextLight,
            TextAlignmentOptions.Right,
            new Vector2(120, 70),
            new Vector2(230f, -25f)
        ).GetComponent<TextMeshProUGUI>();
    }

    void CreateProgressArea()
    {
        GameObject progressBg = UIBuilder.CreatePanel(
            screenObj.transform,
            "ProgressBg",
            new Color(0, 0, 0, 0.3f),
            new Vector2(0, 0.72f),
            new Vector2(1, 0.78f)
        );

        progressTxt = UIBuilder.CreateTextSized(
            progressBg.transform,
            "Soal 1/5",
            36,
            TOBATheme.TextLight,
            TextAlignmentOptions.Left,
            new Vector2(400, 55),
            new Vector2(-250f, 0)
        ).GetComponent<TextMeshProUGUI>();

        timerTxt = UIBuilder.CreateTextSized(
            progressBg.transform,
            "⏱ 15",
            36,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Right,
            new Vector2(200, 55),
            new Vector2(350f, 0)
        ).GetComponent<TextMeshProUGUI>();
    }

    void CreateQuestionArea()
    {
        GameObject questionCard = UIBuilder.CreatePanelSized(
            screenObj.transform,
            "QuestionCard",
            TOBATheme.CardBG,
            new Vector2(980, 260),
            new Vector2(0, 300f)
        );

        Outline outline = questionCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4, 4);

        questionTxt = UIBuilder.CreateTextSized(
            questionCard.transform,
            "Memuat soal...",
            38,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(920, 230),
            Vector2.zero
        ).GetComponent<TextMeshProUGUI>();

        questionTxt.fontStyle = FontStyles.Normal;
        questionTxt.enableWordWrapping = true;
    }

    void CreateOptionsArea()
    {
        optionsContainer = new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(screenObj.transform, false);

        RectTransform rt = optionsContainer.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.07f);
        rt.anchorMax = new Vector2(0.98f, 0.54f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void ShowQuestion(int index)
    {
        if (screenObj == null || index >= questions.Count)
            return;

        isAnswered = false;
        currentIndex = index;

        QuizQuestionData q = questions[index];

        if (progressTxt != null)
            progressTxt.text = "Soal " + (index + 1) + "/" + questions.Count;

        if (questionTxt != null)
            questionTxt.text = q.question;

        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer.transform)
                Object.Destroy(child.gameObject);
        }

        string[] labels = { "A", "B", "C", "D" };
        float[] yPos = { 110f, -10f, -130f, -250f };

        for (int i = 0; i < q.options.Count && i < 4; i++)
        {
            QuizOptionData opt = q.options[i];
            int capturedI = i;

            CreateOptionButton(
                opt,
                labels[i],
                yPos[i],
                () =>
                {
                    if (!isAnswered)
                        OnAnswerSelected(opt, capturedI, q);
                }
            );
        }

        timeRemaining = timeLimit;

        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        timerCoroutine = ScreenManager.Instance.StartCoroutine(RunTimer());
    }

    void CreateOptionButton(
        QuizOptionData opt,
        string label,
        float yPos,
        System.Action onClick)
    {
        GameObject optObj = new GameObject("Opt_" + label);
        optObj.transform.SetParent(optionsContainer.transform, false);

        RectTransform rt = optObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960, 110);
        rt.anchoredPosition = new Vector2(0, yPos);

        Image img = optObj.AddComponent<Image>();
        img.color = TOBATheme.CardBG;
        img.raycastTarget = true;

        Outline outline = optObj.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(3, 3);

        GameObject labelCircle = UIBuilder.CreatePanelSized(
            optObj.transform,
            "LabelCircle",
            TOBATheme.WoodDark,
            new Vector2(85, 85),
            new Vector2(-410f, 0)
        );

        UIBuilder.CreateTextSized(
            labelCircle.transform,
            label,
            40,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(85, 85),
            Vector2.zero
        );

        TextMeshProUGUI optTxt = UIBuilder.CreateTextSized(
            optObj.transform,
            opt.option_text,
            34,
            TOBATheme.TextDark,
            TextAlignmentOptions.Left,
            new Vector2(800, 95),
            new Vector2(60f, 0)
        ).GetComponent<TextMeshProUGUI>();

        optTxt.fontStyle = FontStyles.Normal;
        optTxt.enableWordWrapping = true;

        Button btn = optObj.AddComponent<Button>();

        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX("sfx_click");
            onClick?.Invoke();
        });
    }

    void OnAnswerSelected(
        QuizOptionData selected,
        int selectedIndex,
        QuizQuestionData question)
    {
        if (isAnswered)
            return;

        isAnswered = true;

        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        bool isCorrect = selected.is_correct;

        if (isCorrect)
        {
            myScore++;

            if (myScoreTxt != null)
                myScoreTxt.text = myScore.ToString();
        }

        // =====================================================
        // KIRIM JAWABAN MELALUI WEBSOCKET
        // =====================================================
        SendAnswerViaWS(question, selected);

        HighlightOptions(question, selectedIndex);

        ScreenManager.Instance.StartCoroutine(
            ShowFeedbackThenNext(
                isCorrect,
                question.explanation
            )
        );
    }

    // =========================================================
    // SEND ANSWER VIA WEBSOCKET
    // =========================================================
    void SendAnswerViaWS(
        QuizQuestionData question,
        QuizOptionData selected)
    {
        var payload = new
        {
            type = "ANSWER",
            payload = new
            {
                player_id = playerID,
                question_id = question.id,
                selected_option_id = selected.id,
                is_correct = selected.is_correct
            }
        };

        string json = JsonUtility.ToJson(payload);

        APIManager.Instance.SendBattleWSMessage(json);
    }

    void HighlightOptions(
        QuizQuestionData question,
        int selectedIndex)
    {
        if (optionsContainer == null)
            return;

        int i = 0;

        foreach (Transform child in optionsContainer.transform)
        {
            if (i < question.options.Count)
            {
                Image img = child.GetComponent<Image>();

                if (img != null)
                {
                    if (question.options[i].is_correct)
                    {
                        img.color = TOBATheme.OptionCorrect;
                    }
                    else if (i == selectedIndex)
                    {
                        img.color = TOBATheme.OptionWrong;
                    }
                    else
                    {
                        img.color = new Color(
                            TOBATheme.CardBG.r,
                            TOBATheme.CardBG.g,
                            TOBATheme.CardBG.b,
                            0.5f
                        );
                    }

                    Button btn = child.GetComponent<Button>();

                    if (btn != null)
                        btn.interactable = false;
                }
            }

            i++;
        }
    }

    IEnumerator ShowFeedbackThenNext(
        bool isCorrect,
        string explanation)
    {
        feedbackPanel = UIBuilder.CreatePanelSized(
            screenObj.transform,
            "FeedbackPanel",
            isCorrect ? TOBATheme.Success : TOBATheme.Error,
            new Vector2(980, 150),
            new Vector2(0, -420f)
        );

        UIBuilder.CreateTextSized(
            feedbackPanel.transform,
            isCorrect ? "✅ BENAR!" : "❌ SALAH!",
            42,
            Color.white,
            TextAlignmentOptions.Left,
            new Vector2(400, 75),
            new Vector2(-230f, 18f)
        );

        TextMeshProUGUI expTxt = UIBuilder.CreateTextSized(
            feedbackPanel.transform,
            explanation,
            27,
            Color.white,
            TextAlignmentOptions.Left,
            new Vector2(860, 65),
            new Vector2(0, -32f)
        ).GetComponent<TextMeshProUGUI>();

        expTxt.fontStyle = FontStyles.Normal;
        expTxt.enableWordWrapping = true;

        yield return new WaitForSecondsRealtime(2f);

        if (feedbackPanel != null)
            Object.Destroy(feedbackPanel);

        int next = currentIndex + 1;

        if (next < questions.Count)
            ShowQuestion(next);
        else
            FinishBattle();
    }

    IEnumerator RunTimer()
    {
        while (timeRemaining > 0 && !isAnswered)
        {
            if (timerTxt != null)
            {
                timerTxt.text = "⏱ " + Mathf.CeilToInt(timeRemaining);

                timerTxt.color =
                    timeRemaining <= 5f
                        ? TOBATheme.Error
                        : TOBATheme.BatakGold;
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
                ShowFeedbackThenNext(
                    false,
                    "Waktu habis! " + q.explanation
                )
            );
        }
    }

    void FinishBattle()
    {
        // =====================================================
        // KIRIM PESAN FINISH MELALUI WEBSOCKET
        // =====================================================
        string finishMsg =
            "{\"type\":\"FINISH\",\"payload\":{}}";

        APIManager.Instance.SendBattleWSMessage(finishMsg);

        // Disconnect WebSocket setelah pesan FINISH dikirim
        APIManager.Instance.DisconnectBattleWS();

        // =====================================================
        // TETAP PANGGIL REST UNTUK FINALISASI / SIMPAN KE DB
        // =====================================================
        FinishBattleRequest req = new FinishBattleRequest
        {
            room_id = roomData.room_id,
            player_id = playerID
        };

        APIManager.Instance.PostRequest(
            "/battle/finish",
            JsonUtility.ToJson(req),
            (response) =>
            {
                if (screenObj == null)
                    return;

                FinishBattleResponse res =
                    JsonUtility.FromJson<FinishBattleResponse>(response);

                bool iWin = res.winner_id == playerID;
                bool isDraw = res.winner_id == 0;

                ShowBattleResult(
                    iWin,
                    isDraw,
                    res.host_score,
                    res.guest_score
                );
            },
            (err) =>
            {
                ShowBattleResult(
                    false,
                    true,
                    myScore,
                    opponentScore
                );
            }
        );
    }

    void ShowBattleResult(
        bool iWin,
        bool isDraw,
        int hostScore,
        int guestScore)
    {
        // Overlay hasil
        GameObject overlay = UIBuilder.CreatePanel(
            screenObj.transform,
            "ResultOverlay",
            new Color(0, 0, 0, 0.85f),
            Vector2.zero,
            Vector2.one
        );

        string resultText =
            isDraw
                ? "🤝 SERI!"
                : (iWin ? "🏆 KAMU MENANG!" : "😔 KAMU KALAH");

        Color resultColor =
            isDraw
                ? TOBATheme.Warning
                : (iWin ? TOBATheme.BatakGold : TOBATheme.Error);

        UIBuilder.CreateTextSized(
            overlay.transform,
            resultText,
            80,
            resultColor,
            TextAlignmentOptions.Center,
            new Vector2(900, 140),
            new Vector2(0, 300f)
        );

        // Skor akhir
        string myName =
            isHost ? roomData.host_name : roomData.guest_name;

        string oppName =
            isHost ? roomData.guest_name : roomData.host_name;

        int myFinalScore =
            isHost ? hostScore : guestScore;

        int oppFinalScore =
            isHost ? guestScore : hostScore;

        UIBuilder.CreateTextSized(
            overlay.transform,
            myName + " : " + myFinalScore,
            46,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(800, 70),
            new Vector2(0, 100f)
        );

        UIBuilder.CreateTextSized(
            overlay.transform,
            oppName + " : " + oppFinalScore,
            46,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(800, 70),
            new Vector2(0, 20f)
        );

        if (iWin)
        {
            UIBuilder.CreateTextSized(
                overlay.transform,
                "✨ +50 XP  ⭐ +2 Bintang",
                40,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(800, 60),
                new Vector2(0, -80f)
            );
        }

        // Tombol kembali
        GameObject btnBack = UIBuilder.CreateButton(
            overlay.transform,
            "BtnBack",
            "KEMBALI KE MENU",
            new Vector2(700, 130),
            TOBATheme.BtnPrimary,
            () =>
            {
                SoundManager.Instance.PlaySFX("sfx_click");

                ScreenManager.Instance.ChangeScreen(
                    new BattleMenuScreen(container)
                );
            }
        );

        btnBack.GetComponent<RectTransform>().anchorMin =
            btnBack.GetComponent<RectTransform>().anchorMax =
            new Vector2(0.5f, 0.5f);

        btnBack.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0, -300f);
    }

    public override void Hide()
    {
        if (timerCoroutine != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        base.Hide();
    }
}