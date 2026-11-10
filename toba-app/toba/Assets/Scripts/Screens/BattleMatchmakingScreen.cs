using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleMatchmakingScreen : BaseScreen
{
    private int playerID;
    private int playerLevel;
    private string playerName;
    private bool isSearching   = true;
    private bool isCancelling  = false;
    private Coroutine pollCo;

    private TextMeshProUGUI statusTxt;
    private TextMeshProUGUI timeTxt;
    private float searchTime  = 0f;

    public BattleMatchmakingScreen(Transform parent)
        : base(parent, "BattleMatchmaking") { }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0f, 0.04f, 0f, 0.55f), Vector2.zero, Vector2.one);

        HeaderBar.Create(screenObj.transform, "CARI LAWAN", null);

        playerID    = PlayerPrefs.GetInt("PlayerID");
        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        playerName  = PlayerPrefs.GetString("SavedUsername", "Player");

        BuildUI();
        StartSearch();
    }

    void BuildUI()
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        // Panel utama
        GameObject panel = new GameObject("MainPanel");
        panel.transform.SetParent(screenObj.transform, false);
        RectTransform rtP = panel.AddComponent<RectTransform>();
        rtP.anchorMin = rtP.anchorMax = new Vector2(0.5f, 0.5f);
        rtP.sizeDelta = new Vector2(900, 820);
        rtP.anchoredPosition = new Vector2(0, 80f);
        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        pImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        pImg.color  = new Color(0.22f, 0.10f, 0.03f, 0.97f);
        pImg.raycastTarget = false;
        Outline ol = panel.AddComponent<Outline>();
        ol.effectColor    = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(5, 5);

        // Animasi radar (lingkaran berdenyut)
        ScreenManager.Instance.StartCoroutine(BuildRadar(panel.transform));

        // Label mencari
        UIBuilder.CreateTextSized(panel.transform,
            "<b><color=#A83232>⚔</color></b> MENCARI LAWAN", 52, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(820, 75),
            new Vector2(0, 220f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Status
        statusTxt = UIBuilder.CreateTextSized(panel.transform,
            "Mencari lawan level " + playerLevel + "...",
            32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(800, 55), new Vector2(0, 140f))
            .GetComponent<TextMeshProUGUI>();
        statusTxt.fontStyle    = FontStyles.Normal;
        statusTxt.raycastTarget = false;

        // Info level
        UIBuilder.CreateTextSized(panel.transform,
            "Level kamu: " + playerLevel + "  •  Mencari lawan level " +
            Mathf.Max(1, playerLevel - 3) + "-" + (playerLevel + 3),
            26, TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(800, 45), new Vector2(0, 70f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Timer
        timeTxt = UIBuilder.CreateTextSized(panel.transform,
            "00:00", 48, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(240, 70), new Vector2(0, -30f))
            .GetComponent<TextMeshProUGUI>();
        timeTxt.fontStyle    = FontStyles.Bold;
        timeTxt.raycastTarget = false;

        // Tips
        TextMeshProUGUI tipsTmp = UIBuilder.CreateTextSized(panel.transform,
            "<b><color=#D6A84B>✧</color></b> Jika tidak ada lawan dalam 3 menit,\ncari ulang atau buat room manual",
            26, new Color(1f, 1f, 1f, 0.55f), TextAlignmentOptions.Center,
            new Vector2(800, 80), new Vector2(0, -150f))
            .GetComponent<TextMeshProUGUI>();
        tipsTmp.fontStyle        = FontStyles.Normal;
        tipsTmp.textWrappingMode = TextWrappingModes.Normal;
        tipsTmp.raycastTarget    = false;

        // Tombol Batalkan
        UIBuilder.CreateButton(screenObj.transform,
            "BtnBatal", "✕ BATALKAN PENCARIAN",
            new Vector2(780, 120), TOBATheme.Error, OnCancel)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -680f);
    }

    // Animasi radar lingkaran berdenyut
    IEnumerator BuildRadar(Transform parent)
    {
        // Lingkaran dalam
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            GameObject ring = new GameObject("Ring_" + i);
            ring.transform.SetParent(parent, false);
            RectTransform rt = ring.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            float size = 160f + i * 90f;
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(0, -280f);
            Image img = ring.AddComponent<Image>();
            img.sprite = UIBuilder.GetDefaultSprite();
            img.color  = new Color(TOBATheme.BatakGold.r, TOBATheme.BatakGold.g,
                                   TOBATheme.BatakGold.b, 0.25f - idx * 0.07f);
            img.raycastTarget = false;

            ScreenManager.Instance.StartCoroutine(PulseRing(img, idx));
        }

        // Ikon center (VS)
        UIBuilder.CreateTextSized(parent, "VS",
            72, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(200, 100), new Vector2(0, -280f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;
        yield break;
    }

    IEnumerator PulseRing(Image img, int offset)
    {
        float t = offset * 0.6f;
        while (img != null && screenObj != null)
        {
            t += Time.deltaTime * 1.2f;
            float alpha = (Mathf.Sin(t * Mathf.PI) * 0.5f + 0.5f) *
                         (0.3f - offset * 0.08f);
            if (img != null)
                img.color = new Color(TOBATheme.BatakGold.r,
                    TOBATheme.BatakGold.g, TOBATheme.BatakGold.b, alpha);
            yield return null;
        }
    }

    void StartSearch()
    {
        isSearching = true;
        ScreenManager.Instance.StartCoroutine(TimerUpdate());
        DoMatchmake();
    }

    void DoMatchmake()
    {
        if (!isSearching || screenObj == null) return;

        var req = new MatchmakeRequest {
            player_id   = playerID,
            level       = playerLevel,
            player_name = playerName
        };

        APIManager.Instance.PostRequest("/battle/matchmake",
            JsonUtility.ToJson(req),
            (response) => {
                if (!isSearching || screenObj == null) return;

                MatchmakeResponse res =
                    JsonUtility.FromJson<MatchmakeResponse>(response);

                if (res.status == "MATCHED")
                {
                    OnMatched(res);
                }
                else
                {
                    if (statusTxt)
                    {
                        string botInfo = res.bot_in > 0
                            ? $"\n<size=24><color=#aaaaaa>vs Bot dalam {res.bot_in} detik</color></size>"
                            : "";
                        statusTxt.text = res.message + botInfo;
                    }
                    if (screenObj != null)
                        pollCo = ScreenManager.Instance.StartCoroutine(PollDelay());
                }
            },
            (err) => {
                if (statusTxt) statusTxt.text = "Koneksi error. Mencoba ulang...";
                if (screenObj != null)
                    pollCo = ScreenManager.Instance.StartCoroutine(PollDelay());
            });
    }

    IEnumerator PollDelay()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        if (isSearching && screenObj != null)
            DoMatchmake();
    }

    IEnumerator TimerUpdate()
    {
        while (isSearching && screenObj != null)
        {
            searchTime += Time.deltaTime;
            int minutes = (int)searchTime / 60;
            int seconds = (int)searchTime % 60;
            if (timeTxt) timeTxt.text = $"{minutes:00}:{seconds:00}";

            // Timeout 3 menit → suggest manual
            if (searchTime > 180f && statusTxt != null)
                statusTxt.text = "Sulit menemukan lawan. Coba buat room manual.";

            yield return null;
        }
    }

    void OnMatched(MatchmakeResponse res)
    {
        isSearching = false;
        if (pollCo != null) ScreenManager.Instance.StopCoroutine(pollCo);

        if (res.is_bot)
        {
            SoundManager.Instance?.PlaySFX("sfx_unlock");
            HapticManager.Instance?.Vibrate(HapticType.Medium);
            if (statusTxt) statusTxt.text =
                $"🤖 Tidak ada lawan\nBermain vs {res.opponent_name}!";
        }
        else
        {
            SoundManager.Instance?.PlaySFX("sfx_win");
            HapticManager.Instance?.Vibrate(HapticType.Success);
            if (statusTxt) statusTxt.text =
                $"✅ Lawan ditemukan!\n{res.opponent_name}";
        }

        // Simpan info bot ke PlayerPrefs
        PlayerPrefs.SetInt("BattleIsBot", res.is_bot ? 1 : 0);
        PlayerPrefs.SetString("BattleBotName", res.opponent_name ?? "Bot Toba");
        PlayerPrefs.SetInt("BattleBotLevel", res.bot_level);
        PlayerPrefs.Save();

        ScreenManager.Instance.StartCoroutine(GoToBattle(res));
    }

    IEnumerator GoToBattle(MatchmakeResponse res)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (screenObj == null) yield break;

        // Buat roomData dari response matchmaking
        BattleRoomStatusResponse roomData = new BattleRoomStatusResponse {
            room_id   = res.room_id,
            room_code = res.room_code,
        };

        // Fetch soal dari server dulu
        APIManager.Instance.GetRequest(
            "/battle/status/" + res.room_code,
            (statusResponse) => {
                if (screenObj == null) return;
                BattleRoomStatusResponse fullRoom =
                    JsonUtility.FromJson<BattleRoomStatusResponse>(statusResponse);

                ScreenManager.Instance.ChangeScreen(
                    new BattleQuizScreen(container, fullRoom, res.is_host));
            },
            (err) => {
                // Kalau gagal fetch, pakai roomData minimal
                if (screenObj == null) return;
                ScreenManager.Instance.ChangeScreen(
                    new BattleQuizScreen(container, roomData, res.is_host));
            });
    }

    void OnCancel()
    {
        if (isCancelling) return;
        isCancelling = true;
        isSearching  = false;

        SoundManager.Instance?.PlaySFX("sfx_click");

        APIManager.Instance.PostRequest(
            "/battle/matchmake/cancel/" + playerID, "{}",
            (r) => {
                if (screenObj != null)
                    ScreenManager.Instance.ChangeScreen(
                        new BattleMenuScreen(container));
            },
            (e) => {
                if (screenObj != null)
                    ScreenManager.Instance.ChangeScreen(
                        new BattleMenuScreen(container));
            });
    }

    public override void Hide()
    {
        isSearching = false;
        if (pollCo != null) ScreenManager.Instance.StopCoroutine(pollCo);
        base.Hide();
    }
}