using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleMenuScreen : BaseScreen
{
    private TMP_InputField roomCodeInput;
    private TextMeshProUGUI statusTxt;

    public BattleMenuScreen(Transform parent) : base(parent, "BattleMenu") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "BATTLE QUIZ", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        // Ilustrasi VS
        UIBuilder.CreateTextSized(screenObj.transform,
            "⚔", 160, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(300, 200), new Vector2(0, 480f));

        UIBuilder.CreateTextSized(screenObj.transform,
            "VS", 80, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(300, 120), new Vector2(0, 330f));

        // Card BUAT ROOM
        GameObject createCard = UIBuilder.CreatePanelSized(
            screenObj.transform,
            "CreateCard",
            TOBATheme.CardBG,
            new Vector2(960, 200),
            new Vector2(0, 100f)
        );

        Outline outlineCreate = createCard.AddComponent<Outline>();
        outlineCreate.effectColor = TOBATheme.BatakGold;
        outlineCreate.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(
            createCard.transform,
            "🏠  Buat Room Baru",
            42,
            TOBATheme.WoodDark,
            TextAlignmentOptions.Left,
            new Vector2(680, 60),
            new Vector2(-90f, 40f)
        );

        UIBuilder.CreateTextSized(
            createCard.transform,
            "Undang temanmu dengan kode room",
            30,
            TOBATheme.TextGray,
            TextAlignmentOptions.Left,
            new Vector2(680, 50),
            new Vector2(-90f, -20f)
        )
        .GetComponent<TextMeshProUGUI>()
        .fontStyle = FontStyles.Normal;

        GameObject btnBuat = UIBuilder.CreateSmallButton(
            createCard.transform,
            "BtnBuat",
            "BUAT",
            new Vector2(160, 70),
            TOBATheme.BtnPrimary,
            TOBATheme.TextLight,
            OnCreateRoom
        );

        btnBuat.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(370f, 0);

        // Card GABUNG ROOM
        GameObject joinCard = UIBuilder.CreatePanelSized(
            screenObj.transform,
            "JoinCard",
            TOBATheme.CardBG,
            new Vector2(960, 240),
            new Vector2(0, -160f)
        );

        Outline outlineJoin = joinCard.AddComponent<Outline>();
        outlineJoin.effectColor = TOBATheme.BatakGold;
        outlineJoin.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(
            joinCard.transform,
            "🚪  Gabung Room",
            42,
            TOBATheme.WoodDark,
            TextAlignmentOptions.Left,
            new Vector2(680, 60),
            new Vector2(-90f, 70f)
        );

        // Input kode room
        roomCodeInput = UIBuilder.CreateInputField(
            joinCard.transform,
            "InputRoomCode",
            "Masukkan Kode Room",
            false
        );

        roomCodeInput.characterLimit = 6;

        roomCodeInput.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(-50f, -20f);

        roomCodeInput.GetComponent<RectTransform>().sizeDelta =
            new Vector2(600, 100);

        // Tombol join
        GameObject btnJoin = UIBuilder.CreateSmallButton(
            joinCard.transform,
            "BtnJoin",
            "JOIN",
            new Vector2(160, 100),
            TOBATheme.BtnPrimary,
            TOBATheme.TextLight,
            OnJoinRoom
        );

        btnJoin.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(390f, -20f);

        // ============================================================
        // AUTO MATCHMAKING
        // ============================================================

        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        GameObject autoCard = new GameObject("AutoMatchCard");
        autoCard.transform.SetParent(screenObj.transform, false);

        RectTransform rtAC = autoCard.AddComponent<RectTransform>();
        rtAC.anchorMin = rtAC.anchorMax = new Vector2(0.5f, 0.5f);
        rtAC.sizeDelta = new Vector2(900, 160);
        rtAC.anchoredPosition = new Vector2(0, -320f);

        Image acImg = autoCard.AddComponent<Image>();
        acImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        acImg.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        acImg.color = new Color(0.12f, 0.22f, 0.45f, 0.95f);
        acImg.raycastTarget = true;

        Outline acOl = autoCard.AddComponent<Outline>();
        acOl.effectColor = new Color(0.4f, 0.7f, 1f, 0.8f);
        acOl.effectDistance = new Vector2(4, 4);

        // Icon
        UIBuilder.CreateTextSized(
            autoCard.transform,
            "🔍",
            55,
            Color.white,
            TextAlignmentOptions.Left,
            new Vector2(80, 80),
            new Vector2(-360f, 0)
        )
        .GetComponent<TextMeshProUGUI>()
        .raycastTarget = false;

        // Judul
        TextMeshProUGUI acTitle = UIBuilder.CreateTextSized(
            autoCard.transform,
            "CARI LAWAN OTOMATIS",
            38,
            Color.white,
            TextAlignmentOptions.Left,
            new Vector2(620, 55),
            new Vector2(50f, 22f)
        )
        .GetComponent<TextMeshProUGUI>();

        acTitle.fontStyle = FontStyles.Bold;
        acTitle.raycastTarget = false;

        // Sub
        TextMeshProUGUI acSub = UIBuilder.CreateTextSized(
            autoCard.transform,
            "Sistem carikan lawan selevel • vs Bot jika tidak ada lawan",
            24,
            new Color(1f, 1f, 1f, 0.65f),
            TextAlignmentOptions.Left,
            new Vector2(620, 38),
            new Vector2(50f, -22f)
        )
        .GetComponent<TextMeshProUGUI>();

        acSub.fontStyle = FontStyles.Normal;
        acSub.raycastTarget = false;

        // Button Auto Matchmaking
        Button acBtn = autoCard.AddComponent<Button>();
        acBtn.targetGraphic = acImg;

        acBtn.onClick.AddListener(() => {
            SoundManager.Instance?.PlaySFX("sfx_click");
            HapticManager.Instance?.Vibrate(HapticType.Light);

            ScreenManager.Instance.ChangeScreen(
                new BattleMatchmakingScreen(container)
            );
        });

        // ============================================================
        // MATCHMAKING DESCRIPTION
        // ============================================================

        TextMeshProUGUI mmDesc =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Sistem akan mencarikan lawan selevel secara otomatis",
                28,
                new Color(
                    1f,
                    1f,
                    1f,
                    0.55f
                ),
                TextAlignmentOptions.Center,
                new Vector2(
                    820,
                    50
                ),
                new Vector2(
                    0f,
                    -475f
                )
            ).GetComponent<TextMeshProUGUI>();

        mmDesc.fontStyle = FontStyles.Normal;
        mmDesc.raycastTarget = false;

        // Status teks
        statusTxt = UIBuilder.CreateTextSized(
            screenObj.transform,
            "",
            36,
            TOBATheme.Error,
            TextAlignmentOptions.Center,
            new Vector2(900, 60),
            new Vector2(0, -330f)
        )
        .GetComponent<TextMeshProUGUI>();

        statusTxt.fontStyle = FontStyles.Normal;
    }

    void OnCreateRoom()
    {
        SoundManager.Instance.PlaySFX("sfx_click");

        int playerID = PlayerPrefs.GetInt("PlayerID");

        if (playerID == 0)
        {
            ShowStatus("Kamu harus login terlebih dahulu.", true);
            return;
        }

        ShowStatus("Membuat room...", false);

        CreateRoomRequest req = new CreateRoomRequest
        {
            player_id = playerID
        };

        APIManager.Instance.PostRequest(
            "/battle/create",
            JsonUtility.ToJson(req),
            (response) => {
                if (screenObj == null) return;

                CreateRoomResponse res =
                    JsonUtility.FromJson<CreateRoomResponse>(response);

                if (res != null && !string.IsNullOrEmpty(res.room_code))
                {
                    ScreenManager.Instance.ChangeScreen(
                        new BattleWaitingScreen(
                            container,
                            res.room_code,
                            res.room_id,
                            true
                        )
                    );
                }
                else
                {
                    ShowStatus(
                        "Gagal membuat room. Coba lagi.",
                        true
                    );
                }
            },
            (err) => {
                ShowStatus(
                    "Koneksi gagal. Cek internet.",
                    true
                );
            }
        );
    }

    void OnJoinRoom()
    {
        SoundManager.Instance.PlaySFX("sfx_click");

        string code = roomCodeInput != null
            ? roomCodeInput.text.Trim().ToUpper()
            : "";

        if (string.IsNullOrEmpty(code) || code.Length < 4)
        {
            ShowStatus(
                "Masukkan kode room yang valid.",
                true
            );
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID");

        ShowStatus(
            "Bergabung ke room...",
            false
        );

        JoinRoomRequest req = new JoinRoomRequest
        {
            player_id = playerID,
            room_code = code
        };

        APIManager.Instance.PostRequest(
            "/battle/join",
            JsonUtility.ToJson(req),
            (response) => {
                if (screenObj == null) return;

                // Join berhasil → langsung ke quiz sebagai guest
                BattleRoomStatusResponse res =
                    JsonUtility.FromJson<BattleRoomStatusResponse>(response);

                if (res != null && res.room_id > 0)
                {
                    ScreenManager.Instance.ChangeScreen(
                        new BattleQuizScreen(
                            container,
                            res,
                            false
                        )
                    );
                }
                else
                {
                    ShowStatus(
                        "Room tidak ditemukan atau penuh.",
                        true
                    );
                }
            },
            (err) => {
                ShowStatus(
                    "Gagal bergabung. Kode salah?",
                    true
                );
            }
        );
    }

    void ShowStatus(string msg, bool isError)
    {
        if (statusTxt != null)
        {
            statusTxt.text = msg;
            statusTxt.color =
                isError
                    ? TOBATheme.Error
                    : TOBATheme.BatakGold;
        }
    }
}