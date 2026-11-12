using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleWaitingScreen : BaseScreen
{
    private string roomCode;
    private int roomID;
    private bool isHost;

    private TextMeshProUGUI statusTxt;
    private TextMeshProUGUI countdownTxt;
    private Coroutine countdownCoroutine;
    private bool battleStarted = false;

    public BattleWaitingScreen(Transform parent, string code, int id, bool host)
        : base(parent, "BattleWaiting")
    {
        roomCode = code;
        roomID   = id;
        isHost   = host;
    }

    public override void Show()
    {
        if (screenObj == null)
            return;

        if (string.IsNullOrWhiteSpace(roomCode) ||
            roomID <= 0 ||
            APIManager.Instance == null ||
            ScreenManager.Instance == null)
        {
            Debug.LogWarning("[BattleWaiting] Data room atau manager tidak valid.");

            ScreenManager.Instance?.ChangeScreen(
                new BattleMenuScreen(container)
            );

            return;
        }

        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "MENUNGGU LAWAN", null);

        // Icon
        UIBuilder.CreateTextSized(screenObj.transform,
            "⚔", 180, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(300, 220), new Vector2(0, 480f));

        // Label kode
        UIBuilder.CreateTextSized(screenObj.transform,
            "Kode Room", 38, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 60), new Vector2(0, 240f));

        // Box kode
        GameObject codeBox = UIBuilder.CreatePanelSized(screenObj.transform,
            "CodeBox", TOBATheme.WoodDark, new Vector2(700, 170), new Vector2(0, 90f));
        Outline outline = codeBox.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(5, 5);

        UIBuilder.CreateTextSized(codeBox.transform,
            roomCode, 90, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(680, 145), Vector2.zero);

        // Info share
        TextMeshProUGUI shareTmp = UIBuilder.CreateTextSized(screenObj.transform,
            "Bagikan kode ini ke temanmu!", 32, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(800, 55), new Vector2(0, -30f))
            .GetComponent<TextMeshProUGUI>();
        shareTmp.fontStyle = FontStyles.Normal;

        // Status koneksi
        statusTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "🔄 Menunggu lawan bergabung...", 34, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(900, 65), new Vector2(0, -130f))
            .GetComponent<TextMeshProUGUI>();
        statusTxt.fontStyle = FontStyles.Normal;

        // Countdown
        countdownTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "Room kadaluarsa dalam: 5:00", 28, TOBATheme.TextGray,
            TextAlignmentOptions.Center, new Vector2(800, 50), new Vector2(0, -200f))
            .GetComponent<TextMeshProUGUI>();
        countdownTxt.fontStyle = FontStyles.Normal;

        // Animasi dots loading
        ScreenManager.Instance.StartCoroutine(AnimateWaiting());

        // Tombol batalkan
        GameObject btnCancel = UIBuilder.CreateButton(screenObj.transform,
            "BtnCancel", "❌ BATALKAN",
            new Vector2(600, 120), TOBATheme.Error, () => {
                SoundManager.Instance?.PlaySFX("sfx_click");
                Cleanup();
                ScreenManager.Instance.ChangeScreen(new BattleMenuScreen(container));
            });
        btnCancel.GetComponent<RectTransform>().anchorMin =
            btnCancel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnCancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -430f);

        // Hubungkan WebSocket
        ConnectWebSocket();

        // Countdown timer 5 menit
        countdownCoroutine = ScreenManager.Instance.StartCoroutine(CountdownTimer(300f));
    }

    void ConnectWebSocket()
    {
        if (screenObj == null || APIManager.Instance == null)
            return;

        int playerID = PlayerPrefs.GetInt("PlayerID");
        string playerName = PlayerPrefs.GetString("SavedUsername", "Player");

        if (playerID <= 0)
        {
            if (statusTxt != null)
                statusTxt.text = "Data pemain tidak valid.";

            return;
        }

        APIManager.Instance.ConnectBattleWS(
            roomCode,
            playerID,
            playerName,
            isHost,

            onMessage: msg =>
            {
                if (screenObj != null)
                    OnWSMessage(msg);
            },

            onConnected: () =>
            {
                if (screenObj != null && statusTxt != null)
                    statusTxt.text = "✅ Terhubung! Menunggu lawan...";
            },

            onDisconnected: () =>
            {
                if (!battleStarted &&
                    screenObj != null &&
                    statusTxt != null)
                {
                    statusTxt.text = "⚠ Koneksi terputus. Mencoba ulang...";
                }
            }
        );
    }
    void OnWSMessage(string rawMsg)
    {
        if (screenObj == null || string.IsNullOrWhiteSpace(rawMsg))
            return;

        WSMessageData msg = null;

        try
        {
            msg = JsonUtility.FromJson<WSMessageData>(rawMsg);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "[BattleWaiting] WebSocket message tidak valid: " + ex.Message
            );

            return;
        }

        if (msg == null || string.IsNullOrWhiteSpace(msg.type))
            return;

        switch (msg.type)
        {
            case "START":
                if (!battleStarted)
                {
                    battleStarted = true;

                    if (statusTxt != null)
                        statusTxt.text = "🎮 Lawan ditemukan! Memuat soal...";

                    FetchQuestionsAndStart();
                }
                break;

            case "DISCONNECT":
                if (statusTxt != null)
                    statusTxt.text = "Lawan keluar dari room.";
                break;
        }
    }

    void FetchQuestionsAndStart()
    {
        if (screenObj == null || APIManager.Instance == null)
            return;

        APIManager.Instance.GetRequest(
            "/battle/status/" + roomCode,

            response =>
            {
                if (screenObj == null)
                    return;

                try
                {
                    if (string.IsNullOrWhiteSpace(response))
                        throw new System.FormatException("Response room kosong.");

                    BattleRoomStatusResponse res =
                        JsonUtility.FromJson<BattleRoomStatusResponse>(response);

                    if (res == null || res.questions == null)
                        throw new System.FormatException("Data soal room tidak valid.");

                    res.questions.RemoveAll(q =>
                        q == null ||
                        string.IsNullOrWhiteSpace(q.question) ||
                        q.options == null
                    );

                    foreach (QuizQuestionData q in res.questions)
                    {
                        q.options.RemoveAll(option =>
                            option == null ||
                            string.IsNullOrWhiteSpace(option.option_text)
                        );
                    }

                    res.questions.RemoveAll(q =>
                        q.options.Count == 0 ||
                        !q.options.Exists(option => option.is_correct)
                    );

                    if (res.questions.Count == 0)
                        throw new System.FormatException("Soal Battle kosong.");

                    // Hentikan countdown, tetapi pertahankan WebSocket
                    // selama pertandingan berlangsung.
                    StopCountdown();

                    ScreenManager.Instance?.ChangeScreen(
                        new BattleQuizScreen(container, res, isHost)
                    );
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(
                        "[BattleWaiting] Gagal membaca soal: " + ex.Message
                    );

                    battleStarted = false;

                    if (statusTxt != null)
                        statusTxt.text = "Soal belum tersedia. Silakan coba lagi.";
                }
            },

            err =>
            {
                if (screenObj == null)
                    return;

                Debug.LogWarning("[BattleWaiting] Gagal mengambil soal: " + err);

                if (statusTxt != null)
                    statusTxt.text = "Koneksi gagal: " + err;
            }
        );
    }

    IEnumerator AnimateWaiting()
    {
        string[] dots = { ".", "..", "..." };
        int i = 0;
        while (!battleStarted && screenObj != null)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            i = (i + 1) % dots.Length;
        }
    }

    IEnumerator CountdownTimer(float total)
    {
        float remaining = total;
        while (remaining > 0 && !battleStarted && screenObj != null)
        {
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);
            if (countdownTxt != null)
                countdownTxt.text =
                    string.Format("Room kadaluarsa dalam: {0}:{1:D2}", mins, secs);
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!battleStarted && screenObj != null)
        {
            Cleanup();
            ScreenManager.Instance.ChangeScreen(new BattleMenuScreen(container));
        }
    }

    void StopCountdown()
    {
        if (countdownCoroutine != null &&
            ScreenManager.Instance != null)
        {
            ScreenManager.Instance.StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = null;
    }

    void Cleanup()
    {
        StopCountdown();

        if (APIManager.Instance != null)
            APIManager.Instance.DisconnectBattleWS();
    }

    public override void Hide()
    {
        battleStarted = true;

        Cleanup();

        base.Hide();
    }
}

// ── Helper model untuk parse WS message ──────────────────
// ⚠️ BUKAN [Serializable] model API — ini khusus WS parsing
// Taruh di file ini saja karena hanya dipakai di sini
[System.Serializable]
internal class WSMessageData
{
    public string type;
    public string payload;
}