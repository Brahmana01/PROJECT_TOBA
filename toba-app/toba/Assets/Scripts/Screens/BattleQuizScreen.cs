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
    private List<BattleQuestionData> questions = new List<BattleQuestionData>();
    private int currentIndex = 0;
    private int myScore = 0;
    private int opponentScore = 0;
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
    private float _questionStartTime = 0f;
    private bool isBotMatch = false;
    private bool battleFinished = false;
    private bool isFinishing = false;
    private string botName = "Bot Toba";
    private int botLevel = 1;
    private Coroutine botCo;
    private Coroutine statusPollCo;
    private bool isSubmittingAnswer = false;
    private int pendingSelectedOptionID = -1;
    private int pendingSelectedIndex = -1;
    private BattleQuestionData pendingQuestion;
    private BattleOptionData pendingSelectedOption;
    private long pendingAnswerTimeMs;
    private bool pendingTimeout;
    private int pendingRetryCount = 0;
    public BattleQuizScreen(Transform parent, BattleRoomStatusResponse room, bool host)
            : base(parent, "BattleQuiz")
    {
        roomData = room;
        isHost = host;
        playerID = PlayerPrefs.GetInt("PlayerID");
    }
    public override void Show()
    {
        if (screenObj == null)
            return;

        if (roomData == null || roomData.room_id <= 0)
        {
            NetworkManager.Instance?.ShowToast("Data room Battle tidak valid.", true);
            ScreenManager.Instance?.ChangeScreen(new BattleMenuScreen(container));
            return;
        }

        SetBackground(TOBATheme.ForestGreen);
        isBotMatch = PlayerPrefs.GetInt("BattleIsBot", 0) == 1;
        botName = PlayerPrefs.GetString("BattleBotName", "Bot Toba");
        botLevel = PlayerPrefs.GetInt("BattleBotLevel", 1);

        HeaderBar.Create(screenObj.transform, "BATTLE QUIZ", null);
        CreateScoreBoard();
        CreateProgressArea();
        CreateQuestionArea();
        CreateOptionsArea();

        questions = roomData.questions ?? new List<BattleQuestionData>();
        questions.RemoveAll(q => q == null ||
            string.IsNullOrWhiteSpace(q.question) || q.options == null || q.options.Count != 4);
        foreach (BattleQuestionData q in questions)
            q.options.RemoveAll(option => option == null || string.IsNullOrWhiteSpace(option.option_text));
        questions.RemoveAll(q => q.options.Count != 4);

        int initialHostScore = (int)roomData.host_score;
        int initialGuestScore = (int)roomData.guest_score;
        myScore = isHost ? initialHostScore : initialGuestScore;
        opponentScore = isHost ? initialGuestScore : initialHostScore;
        if (myScoreTxt != null) myScoreTxt.text = myScore.ToString();
        if (oppScoreTxt != null) oppScoreTxt.text = opponentScore.ToString();

        if (questions.Count != 10)
        {
            if (questionTxt != null)
                questionTxt.text = "Battle membutuhkan tepat 10 soal valid.";
            NetworkManager.Instance?.ShowToast("Data soal Battle tidak lengkap.", true);
            return;
        }

        ShowQuestion(0);
        if (!isBotMatch && APIManager.Instance != null && !APIManager.Instance.IsWSConnected())
        {
            APIManager.Instance.ConnectBattleWS(
                roomData.room_code,
                onMessage: _ => { },
                onConnected: () => Debug.Log("[BattleQuiz] WebSocket Battle terhubung."),
                onDisconnected: () => Debug.LogWarning("[BattleQuiz] WebSocket Battle terputus."));
        }
        if (!isBotMatch && statusPollCo == null && ScreenManager.Instance != null)
            statusPollCo = ScreenManager.Instance.StartCoroutine(PollBattleStatus());
    }
    // SCOREBOARD
    void CreateScoreBoard()
    {
        GameObject scorePanel = UIBuilder.CreatePanel(screenObj.transform, "ScorePanel", TOBATheme.WoodDark, new Vector2(0, 0.78f), new Vector2(1, 0.88f));
        string myName =
            isHost
                ? roomData.host_name
                : roomData.guest_name;
        string oppName =
            isHost
                ? roomData.guest_name
                : roomData.host_name;
        if (isBotMatch)
            oppName = botName + " 🤖";
        myNameTxt = UIBuilder.CreateTextSized(scorePanel.transform, myName, 34, TOBATheme.TextLight, TextAlignmentOptions.Left, new Vector2(380, 55), new Vector2(-230f, 20f)).GetComponent<TextMeshProUGUI>();
        myNameTxt.fontStyle = FontStyles.Normal;
        myScoreTxt = UIBuilder.CreateTextSized(scorePanel.transform, "0", 55, TOBATheme.BatakGold, TextAlignmentOptions.Left, new Vector2(120, 70), new Vector2(-230f, -25f)).GetComponent<TextMeshProUGUI>();
        UIBuilder.CreateTextSized(scorePanel.transform, "VS", 50, TOBATheme.Error, TextAlignmentOptions.Center, new Vector2(120, 70), new Vector2(0, 0));
        oppNameTxt = UIBuilder.CreateTextSized(scorePanel.transform, oppName, 34, TOBATheme.TextLight, TextAlignmentOptions.Right, new Vector2(380, 55), new Vector2(230f, 20f)).GetComponent<TextMeshProUGUI>();
        oppNameTxt.fontStyle = FontStyles.Normal;
        oppScoreTxt = UIBuilder.CreateTextSized(scorePanel.transform, "0", 55, TOBATheme.TextLight, TextAlignmentOptions.Right, new Vector2(120, 70), new Vector2(230f, -25f)).GetComponent<TextMeshProUGUI>();
    }
    // PROGRESS
    void CreateProgressArea()
    {
        GameObject progressBg = UIBuilder.CreatePanel(screenObj.transform, "ProgressBg", new Color(0, 0, 0, 0.3f), new Vector2(0, 0.72f), new Vector2(1, 0.78f));
        progressTxt = UIBuilder.CreateTextSized(progressBg.transform, "Soal 1/10", 36, TOBATheme.TextLight, TextAlignmentOptions.Left, new Vector2(400, 55), new Vector2(-250f, 0)).GetComponent<TextMeshProUGUI>();
        timerTxt = UIBuilder.CreateTextSized(progressBg.transform, "⏱ 15", 36, TOBATheme.BatakGold, TextAlignmentOptions.Right, new Vector2(200, 55), new Vector2(350f, 0)).GetComponent<TextMeshProUGUI>();
    }
    // QUESTION
    void CreateQuestionArea()
    {
        GameObject questionCard = UIBuilder.CreatePanelSized(screenObj.transform, "QuestionCard", TOBATheme.CardBG, new Vector2(980, 260), new Vector2(0, 300f));
        Outline outline =
            questionCard.AddComponent<Outline>();
        outline.effectColor =
            TOBATheme.BatakGold;
        outline.effectDistance =
            new Vector2(4, 4);
        questionTxt = UIBuilder.CreateTextSized(questionCard.transform, "Memuat soal...", 38, TOBATheme.TextDark, TextAlignmentOptions.Center, new Vector2(920, 230), Vector2.zero).GetComponent<TextMeshProUGUI>();
        questionTxt.fontStyle =
            FontStyles.Normal;
        questionTxt.enableWordWrapping =
            true;
    }
    // OPTIONS
    void CreateOptionsArea()
    {
        optionsContainer =
            new GameObject("OptionsContainer");
        optionsContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rt =
            optionsContainer.AddComponent<RectTransform>();
        rt.anchorMin =
            new Vector2(0.02f, 0.07f);
        rt.anchorMax =
            new Vector2(0.98f, 0.54f);
        rt.offsetMin =
            rt.offsetMax =
                Vector2.zero;
    }
    // SHOW QUESTION
    void ShowQuestion(int index)
    {
        if (screenObj == null ||
            questions == null ||
            optionsContainer == null ||
            ScreenManager.Instance == null ||
            index < 0 ||
            index >= questions.Count)
            return;

        BattleQuestionData q = questions[index];

        if (q == null || q.options == null || q.options.Count == 0)
        {
            if (questionTxt != null)
                questionTxt.text = "Soal tidak tersedia.";

            return;
        }

        _questionStartTime = Time.realtimeSinceStartup;
        isAnswered = false;
        currentIndex = index;
        if (progressTxt != null)
        {
            progressTxt.text =
                "Soal " +
                (index + 1) +
                "/" +
                questions.Count;
        }
        if (questionTxt != null)
            questionTxt.text =
                q.question;
        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer.transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
        string[] labels =
        {
            "A",
            "B",
            "C",
            "D"
        };
        float[] yPos =
        {
            110f,
            -10f,
            -130f,
            -250f
        };
        for (int i = 0; i < q.options.Count && i < 4; i++)
        {
            BattleOptionData opt =
                q.options[i];
            int capturedI = i;
            CreateOptionButton(opt, labels[i], yPos[i], () =>
                            {
                                if (!isAnswered)
                                {
                                    OnAnswerSelected(opt, capturedI, q);
                                }
                            }
                        );
        }
        timeRemaining =
            timeLimit;
        if (timerCoroutine != null)
        {
            ScreenManager.Instance.StopCoroutine(timerCoroutine);
        }
        timerCoroutine =
ScreenManager.Instance.StartCoroutine(RunTimer());
        if (isBotMatch)
        {
            if (botCo != null)
            {
                ScreenManager.Instance.StopCoroutine(botCo);
            }
            botCo =
ScreenManager.Instance.StartCoroutine(BotAnswerCoroutine(index));
        }
    }
    // OPTION BUTTON
    void CreateOptionButton(BattleOptionData opt, string label, float yPos, System.Action onClick)
    {
        if (opt == null || optionsContainer == null)
            return;
        GameObject optObj =
    new GameObject("Opt_" + label);
        optObj.transform.SetParent(optionsContainer.transform, false);
        RectTransform rt =
            optObj.AddComponent<RectTransform>();
        rt.anchorMin =
            rt.anchorMax =
                new Vector2(0.5f, 0.5f);
        rt.sizeDelta =
            new Vector2(960, 110);
        rt.anchoredPosition =
            new Vector2(0, yPos);
        Image img =
            optObj.AddComponent<Image>();
        img.color =
            TOBATheme.CardBG;
        img.raycastTarget =
            true;
        Outline outline =
            optObj.AddComponent<Outline>();
        outline.effectColor =
            TOBATheme.BatakGold;
        outline.effectDistance =
            new Vector2(3, 3);
        GameObject labelCircle =
UIBuilder.CreatePanelSized(optObj.transform, "LabelCircle", TOBATheme.WoodDark, new Vector2(85, 85), new Vector2(-410f, 0));
        UIBuilder.CreateTextSized(labelCircle.transform, label, 40, TOBATheme.BatakGold, TextAlignmentOptions.Center, new Vector2(85, 85), Vector2.zero);
        TextMeshProUGUI optTxt =
UIBuilder.CreateTextSized(optObj.transform, opt.option_text, 34, TOBATheme.TextDark, TextAlignmentOptions.Left, new Vector2(800, 95), new Vector2(60f, 0)).GetComponent<TextMeshProUGUI>();
        optTxt.fontStyle =
            FontStyles.Normal;
        optTxt.enableWordWrapping =
            true;
        Button btn =
            optObj.AddComponent<Button>();
        BattleOptionButtonTag optionTag = optObj.AddComponent<BattleOptionButtonTag>();
        optionTag.OptionID = opt.id;
        btn.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.PlaySFX("sfx_click");
                    onClick?.Invoke();
                });
    }
    void OnAnswerSelected(BattleOptionData selected, int selectedIndex, BattleQuestionData question)
    {
        if (screenObj == null || selected == null || question == null || isSubmittingAnswer || pendingTimeout)
            return;

        // Jika request sebelumnya gagal diterima client, hanya pilihan yang sama
        // boleh dikirim ulang agar tidak bertentangan dengan jawaban yang mungkin
        // sudah tersimpan di server.
        if (pendingSelectedOptionID > 0 && selected.id != pendingSelectedOptionID)
            return;

        if (pendingSelectedOptionID > 0)
        {
            SubmitPendingAnswer();
            return;
        }

        long elapsed = (long)((Time.realtimeSinceStartup - _questionStartTime) * 1000f);
        pendingSelectedOptionID = selected.id;
        pendingSelectedIndex = selectedIndex;
        pendingQuestion = question;
        pendingSelectedOption = selected;
        pendingAnswerTimeMs = System.Math.Max(200L, System.Math.Min(15000L, elapsed));
        pendingTimeout = false;
        pendingRetryCount = 0;
        SubmitPendingAnswer();
    }

    void SubmitPendingAnswer()
    {
        if (isSubmittingAnswer || pendingQuestion == null || APIManager.Instance == null)
            return;

        isSubmittingAnswer = true;
        isAnswered = true;
        if (timerCoroutine != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(timerCoroutine);

        foreach (Transform child in optionsContainer.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) button.interactable = false;
        }

        SubmitBattleAnswerRequest request = new SubmitBattleAnswerRequest
        {
            room_id = roomData.room_id,
            player_id = playerID,
            question_id = pendingQuestion.id,
            selected_option_id = pendingSelectedOptionID < 0 ? 0 : pendingSelectedOptionID,
            answer_time_ms = pendingAnswerTimeMs
        };

        APIManager.Instance.PostRequest(
            "/battle/answer",
            JsonUtility.ToJson(request),
            response =>
            {
                isSubmittingAnswer = false;
                if (screenObj == null) return;

                BattleAnswerSubmitResponse result = null;
                try
                {
                    result = JsonUtility.FromJson<BattleAnswerSubmitResponse>(response);
                    if (result == null || result.correct_option_id <= 0)
                        throw new System.FormatException("Hasil jawaban Battle tidak valid.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[BattleQuiz] Response jawaban tidak valid: " + ex.Message);
                    HandleAnswerSubmitFailure("Respons jawaban tidak valid.");
                    return;
                }

                myScore = isHost ? (int)result.host_score : (int)result.guest_score;
                opponentScore = isHost ? (int)result.guest_score : (int)result.host_score;
                if (myScoreTxt != null) myScoreTxt.text = myScore.ToString();
                if (oppScoreTxt != null) oppScoreTxt.text = opponentScore.ToString();

                bool correct = result.is_correct;
                BattleQuestionData answeredQuestion = pendingQuestion;
                int selectedIndex = pendingSelectedIndex;
                int correctOptionID = result.correct_option_id;
                pendingSelectedOptionID = -1;
                pendingSelectedIndex = -1;
                pendingQuestion = null;
                pendingSelectedOption = null;
                pendingTimeout = false;
                pendingRetryCount = 0;

                HighlightOptions(answeredQuestion, selectedIndex, correctOptionID);
                ScreenManager.Instance?.StartCoroutine(
                    ShowFeedbackThenNext(correct, result.explanation ?? ""));
            },
            error =>
            {
                isSubmittingAnswer = false;
                if (screenObj == null) return;
                HandleAnswerSubmitFailure(error);
            }
        );
    }

    void HandleAnswerSubmitFailure(string message)
    {
        isAnswered = false;
        NetworkManager.Instance?.ShowToast(
            string.IsNullOrWhiteSpace(message) ? "Jawaban belum tersimpan. Ketuk pilihan yang sama untuk mencoba lagi." : message,
            true);

        if (pendingTimeout)
        {
            if (ScreenManager.Instance != null && pendingRetryCount < 3)
                ScreenManager.Instance.StartCoroutine(RetryTimeoutAnswer());
            return;
        }

        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer.transform)
            {
                Button button = child.GetComponent<Button>();
                BattleOptionButtonTag tag = child.GetComponent<BattleOptionButtonTag>();
                if (button != null)
                    button.interactable = tag != null && tag.OptionID == pendingSelectedOptionID;
            }
        }
        if (timerTxt != null) timerTxt.text = "Pilih jawaban yang sama untuk kirim ulang";
    }

    IEnumerator RetryTimeoutAnswer()
    {
        pendingRetryCount++;
        yield return new WaitForSecondsRealtime(2f);
        if (screenObj != null && pendingTimeout && !isSubmittingAnswer)
            SubmitPendingAnswer();
        if (pendingRetryCount >= 3 && pendingTimeout && screenObj != null)
            NetworkManager.Instance?.ShowToast("Koneksi gagal. Jawaban waktu habis belum tersimpan; periksa koneksi lalu coba kembali.", true);
    }


    IEnumerator BotAnswerCoroutine(int questionIndex)
    {
        // Bot berpikir sekitar 0.7 - 1.6 detik.
        // Dibuat di bawah durasi feedback 2 detik
        // agar bot sempat menyelesaikan jawaban
        // sebelum soal berganti.
        float thinkTime =
UnityEngine.Random.Range(0.7f, 1.6f);
        yield return new WaitForSecondsRealtime(thinkTime);
        if (screenObj == null)
            yield break;
        if (currentIndex != questionIndex)
            yield break;
        // Akurasi berdasarkan level bot.
        float accuracy =
            botLevel <= 3
                ? 0.35f
                : botLevel <= 7
                    ? 0.55f
                    : 0.75f;
        bool botCorrect =
            UnityEngine.Random.value <
            accuracy;
        if (botCorrect)
        {
            opponentScore++;
            if (oppScoreTxt != null)
            {
                oppScoreTxt.text =
                    opponentScore.ToString();
            }
        }
        else
        {
            if (oppScoreTxt != null)
            {
                oppScoreTxt.text =
                    opponentScore.ToString();
            }
        }
        Debug.Log(
                    $"[Bot] {botName} soal #{questionIndex + 1}: " +
        (botCorrect ? "✅ BENAR" : "❌ SALAH") +
                    $" | Level: {botLevel}" +
                    $" | Skor bot: {opponentScore}"
                );
    }
    void HighlightOptions(BattleQuestionData question, int selectedIndex, int correctOptionID)
    {
        if (optionsContainer == null || question == null || question.options == null)
            return;

        for (int i = 0; i < optionsContainer.transform.childCount; i++)
        {
            Transform child = optionsContainer.transform.GetChild(i);
            Image image = child.GetComponent<Image>();
            Button button = child.GetComponent<Button>();
            BattleOptionButtonTag tag = child.GetComponent<BattleOptionButtonTag>();
            if (image != null && tag != null)
            {
                if (tag.OptionID == correctOptionID)
                    image.color = TOBATheme.OptionCorrect;
                else if (i == selectedIndex)
                    image.color = TOBATheme.OptionWrong;
                else
                    image.color = new Color(TOBATheme.CardBG.r, TOBATheme.CardBG.g, TOBATheme.CardBG.b, 0.5f);
            }
            if (button != null) button.interactable = false;
        }
    }
    IEnumerator ShowFeedbackThenNext(bool isCorrect, string explanation)
    {
        if (screenObj == null)
            yield break;
        feedbackPanel =
UIBuilder.CreatePanelSized(screenObj.transform, "FeedbackPanel", isCorrect ? TOBATheme.Success : TOBATheme.Error, new Vector2(980, 150), new Vector2(0, -420f));
        UIBuilder.CreateTextSized(feedbackPanel.transform, isCorrect ? "✅ BENAR!" : "❌ SALAH!", 42, Color.white, TextAlignmentOptions.Left, new Vector2(400, 75), new Vector2(-230f, 18f));
        TextMeshProUGUI expTxt =
UIBuilder.CreateTextSized(feedbackPanel.transform, explanation, 27, Color.white, TextAlignmentOptions.Left, new Vector2(860, 65), new Vector2(0, -32f)).GetComponent<TextMeshProUGUI>();
        expTxt.fontStyle =
            FontStyles.Normal;
        expTxt.enableWordWrapping =
            true;
        yield return new WaitForSecondsRealtime(2f);
        if (screenObj == null)
            yield break;
        if (feedbackPanel != null)
            Object.Destroy(feedbackPanel);
        int next =
            currentIndex + 1;
        if (next < questions.Count)
        {
            ShowQuestion(next);
        }
        else
        {
            FinishBattle();
        }
    }
    IEnumerator RunTimer()
    {
        while (timeRemaining > 0f && !isAnswered && screenObj != null)
        {
            if (timerTxt != null)
            {
                timerTxt.text = "⏱ " + Mathf.CeilToInt(timeRemaining);
                timerTxt.color = timeRemaining <= 5f ? TOBATheme.Error : TOBATheme.BatakGold;
            }
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (isAnswered || screenObj == null) yield break;
        if (questions == null || currentIndex < 0 || currentIndex >= questions.Count)
        {
            FinishBattle();
            yield break;
        }

        BattleQuestionData question = questions[currentIndex];
        if (question == null)
        {
            FinishBattle();
            yield break;
        }

        pendingSelectedOptionID = 0;
        pendingSelectedIndex = -1;
        pendingQuestion = question;
        pendingSelectedOption = null;
        pendingAnswerTimeMs = 15000;
        pendingTimeout = true;
        pendingRetryCount = 0;
        SubmitPendingAnswer();
    }
    void FinishBattle()
    {
        if (screenObj == null || isFinishing || battleFinished) return;
        RequestFinishBattle();
    }

    void RequestFinishBattle()
    {
        if (screenObj == null || isFinishing || battleFinished || APIManager.Instance == null)
            return;
        isFinishing = true;
        FinishBattleRequest request = new FinishBattleRequest
        {
            room_id = roomData.room_id,
            player_id = playerID
        };

        APIManager.Instance.PostRequest(
            "/battle/finish",
            JsonUtility.ToJson(request),
            response =>
            {
                if (screenObj == null) return;
                isFinishing = false;
                FinishBattleResponse result = null;
                try
                {
                    result = JsonUtility.FromJson<FinishBattleResponse>(response);
                    if (result == null || string.IsNullOrWhiteSpace(result.status))
                        throw new System.FormatException("Data hasil Battle tidak valid.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[BattleQuiz] Hasil Battle tidak valid: " + ex.Message);
                    NetworkManager.Instance?.ShowToast("Belum dapat membaca hasil Battle.", true);
                    return;
                }

                if (result.status == "WAITING_OPPONENT")
                {
                    if (timerTxt != null) timerTxt.text = "Menunggu lawan menyelesaikan 10 soal...";
                    if (progressTxt != null) progressTxt.text = "Menunggu lawan";
                    return;
                }
                ShowFinalBattleResult(result);
            },
            error =>
            {
                if (screenObj == null) return;
                isFinishing = false;
                NetworkManager.Instance?.ShowToast("Gagal menyelesaikan Battle: " + error, true);
            }
        );
    }

    void ShowFinalBattleResult(FinishBattleResponse result)
    {
        if (result == null || screenObj == null || battleFinished) return;
        battleFinished = true;
        if (!isBotMatch)
            APIManager.Instance?.DisconnectBattleWS();
        if (statusPollCo != null && ScreenManager.Instance != null)
            ScreenManager.Instance.StopCoroutine(statusPollCo);
        statusPollCo = null;

        if (isBotMatch)
        {
            bool botWin = myScore > opponentScore;
            bool botDraw = myScore == opponentScore;
            ShowBattleResult(botWin, botDraw, myScore, opponentScore, 0, 0);
            return;
        }

        bool humanWin = result.winner_id == playerID;
        bool humanDraw = result.winner_id == 0;
        ShowBattleResult(
            humanWin, humanDraw,
            (int)result.host_score, (int)result.guest_score,
            result.reward_xp, result.reward_stars);
    }

    IEnumerator PollBattleStatus()
    {
        while (screenObj != null && !battleFinished && !isBotMatch)
        {
            yield return new WaitForSecondsRealtime(2f);
            if (screenObj == null || battleFinished || APIManager.Instance == null) yield break;

            APIManager.Instance.GetRequest(
                "/battle/status/" + roomData.room_code,
                response =>
                {
                    if (screenObj == null || battleFinished) return;
                    try
                    {
                        BattleRoomStatusResponse status = JsonUtility.FromJson<BattleRoomStatusResponse>(response);
                        if (status == null) return;
                        myScore = isHost ? (int)status.host_score : (int)status.guest_score;
                        opponentScore = isHost ? (int)status.guest_score : (int)status.host_score;
                        if (myScoreTxt != null) myScoreTxt.text = myScore.ToString();
                        if (oppScoreTxt != null) oppScoreTxt.text = opponentScore.ToString();

                        if (status.status == "FINISHED" && !isFinishing)
                            RequestFinishBattle();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("[BattleQuiz] Gagal membaca status room: " + ex.Message);
                    }
                },
                error => Debug.LogWarning("[BattleQuiz] Poll status gagal: " + error)
            );
        }
    }
    void ShowBattleResult(bool iWin, bool isDraw, int hostScore, int guestScore, int rewardXP = 0, int rewardStars = 0)
    {
        if (screenObj == null || roomData == null)
            return;
        GameObject overlay =
UIBuilder.CreatePanel(screenObj.transform, "ResultOverlay", new Color(0, 0, 0, 0.85f), Vector2.zero, Vector2.one);
        string resultText =
            isDraw
                ? "🤝 SERI!"
: (iWin ? "<color=#D6A84B><sprite name=\"leaderboard\" tint=1></color> KAMU MENANG!" : "😔 KAMU KALAH");
        Color resultColor =
            isDraw
                ? TOBATheme.Warning
: (iWin ? TOBATheme.BatakGold : TOBATheme.Error);
        UIBuilder.CreateTextSized(overlay.transform, resultText, 80, resultColor, TextAlignmentOptions.Center, new Vector2(900, 140), new Vector2(0, 300f));
        string myName =
            isHost
                ? roomData.host_name
                : roomData.guest_name;
        string oppName =
            isHost
                ? roomData.guest_name
                : roomData.host_name;
        // Bot
        if (isBotMatch)
            oppName = botName + " 🤖";
        int myFinalScore =
            isBotMatch
                ? myScore
: (isHost ? hostScore : guestScore);
        int oppFinalScore =
            isBotMatch
                ? opponentScore
: (isHost ? guestScore : hostScore);
        UIBuilder.CreateTextSized(overlay.transform, myName + " : " + myFinalScore, 46, TOBATheme.TextLight, TextAlignmentOptions.Center, new Vector2(800, 70), new Vector2(0, 100f));
        UIBuilder.CreateTextSized(overlay.transform, oppName + " : " + oppFinalScore, 46, TOBATheme.TextLight, TextAlignmentOptions.Center, new Vector2(800, 70), new Vector2(0, 20f));
        if (iWin && (rewardXP > 0 || rewardStars > 0))
        {
            UIBuilder.CreateTextSized(
                overlay.transform,
                "✨ +" + rewardXP + " XP  <color=#D6A84B><sprite name=\"star\" tint=1></color> +" + rewardStars + " Bintang",
                40, TOBATheme.BatakGold, TextAlignmentOptions.Center,
                new Vector2(800, 60), new Vector2(0, -80f));
        }
        GameObject btnBack =
UIBuilder.CreateButton(overlay.transform, "BtnBack", "KEMBALI KE MENU", new Vector2(700, 130), TOBATheme.BtnPrimary, () =>
                {
                    SoundManager.Instance?.PlaySFX("sfx_click");
                    ScreenManager.Instance?.ChangeScreen(
                        new BattleMenuScreen(container)
                    );
                }
            );
        btnBack.GetComponent<RectTransform>().anchorMin =
            btnBack.GetComponent<RectTransform>().anchorMax =
                new Vector2(0.5f, 0.5f);
        btnBack.GetComponent<RectTransform>()
            .anchoredPosition =
                new Vector2(0, -300f);
    }
    public override void Hide()
    {
        if (ScreenManager.Instance != null)
        {
            if (timerCoroutine != null)
                ScreenManager.Instance.StopCoroutine(timerCoroutine);

            if (botCo != null)
                ScreenManager.Instance.StopCoroutine(botCo);
            if (statusPollCo != null)
                ScreenManager.Instance.StopCoroutine(statusPollCo);
        }

        timerCoroutine = null;
        botCo = null;
        statusPollCo = null;

        if (feedbackPanel != null)
        {
            Object.Destroy(feedbackPanel);
            feedbackPanel = null;
        }

        if (!battleFinished && !isBotMatch &&
            APIManager.Instance != null)
        {
            APIManager.Instance.DisconnectBattleWS();
        }
        base.Hide();
    }
}


[System.Serializable]
public class BattleOptionButtonTag : MonoBehaviour
{
    public int OptionID;
}
