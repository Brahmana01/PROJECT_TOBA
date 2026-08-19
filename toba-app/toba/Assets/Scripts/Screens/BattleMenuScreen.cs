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
        GameObject createCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "CreateCard", TOBATheme.CardBG, new Vector2(960, 200), new Vector2(0, 100f));

        Outline outlineCreate = createCard.AddComponent<Outline>();
        outlineCreate.effectColor = TOBATheme.BatakGold;
        outlineCreate.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(createCard.transform,
            "🏠  Buat Room Baru", 42, TOBATheme.WoodDark, TextAlignmentOptions.Left,
            new Vector2(680, 60), new Vector2(-90f, 40f));

        UIBuilder.CreateTextSized(createCard.transform,
            "Undang temanmu dengan kode room", 30, TOBATheme.TextGray,
            TextAlignmentOptions.Left, new Vector2(680, 50), new Vector2(-90f, -20f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        GameObject btnBuat = UIBuilder.CreateSmallButton(createCard.transform,
            "BtnBuat", "BUAT",
            new Vector2(160, 70), TOBATheme.BtnPrimary, TOBATheme.TextLight, OnCreateRoom);
        btnBuat.GetComponent<RectTransform>().anchoredPosition = new Vector2(370f, 0);

        // Card GABUNG ROOM
        GameObject joinCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "JoinCard", TOBATheme.CardBG, new Vector2(960, 240), new Vector2(0, -160f));

        Outline outlineJoin = joinCard.AddComponent<Outline>();
        outlineJoin.effectColor = TOBATheme.BatakGold;
        outlineJoin.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(joinCard.transform,
            "🚪  Gabung Room", 42, TOBATheme.WoodDark, TextAlignmentOptions.Left,
            new Vector2(680, 60), new Vector2(-90f, 70f));

        // Input kode room
        roomCodeInput = UIBuilder.CreateInputField(
            joinCard.transform, "InputRoomCode", "Masukkan Kode Room", false);
        roomCodeInput.characterLimit = 6;
        roomCodeInput.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50f, -20f);
        roomCodeInput.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

        // Tombol join
        GameObject btnJoin = UIBuilder.CreateSmallButton(joinCard.transform,
            "BtnJoin", "JOIN",
            new Vector2(160, 100), TOBATheme.BtnPrimary, TOBATheme.TextLight, OnJoinRoom);
        btnJoin.GetComponent<RectTransform>().anchoredPosition = new Vector2(390f, -20f);

        // Status teks
        statusTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "", 36, TOBATheme.Error, TextAlignmentOptions.Center,
            new Vector2(900, 60), new Vector2(0, -330f))
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

        CreateRoomRequest req = new CreateRoomRequest { player_id = playerID };

        APIManager.Instance.PostRequest("/battle/create", JsonUtility.ToJson(req), (response) => {
            if (screenObj == null) return;

            CreateRoomResponse res = JsonUtility.FromJson<CreateRoomResponse>(response);
            if (res != null && !string.IsNullOrEmpty(res.room_code))
            {
                ScreenManager.Instance.ChangeScreen(
                    new BattleWaitingScreen(container, res.room_code, res.room_id, true));
            }
            else
            {
                ShowStatus("Gagal membuat room. Coba lagi.", true);
            }
        }, (err) => {
            ShowStatus("Koneksi gagal. Cek internet.", true);
        });
    }

    void OnJoinRoom()
    {
        SoundManager.Instance.PlaySFX("sfx_click");
        string code = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpper() : "";

        if (string.IsNullOrEmpty(code) || code.Length < 4)
        {
            ShowStatus("Masukkan kode room yang valid.", true);
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID");
        ShowStatus("Bergabung ke room...", false);

        JoinRoomRequest req = new JoinRoomRequest
        {
            player_id = playerID,
            room_code = code
        };

        APIManager.Instance.PostRequest("/battle/join", JsonUtility.ToJson(req), (response) => {
            if (screenObj == null) return;

            // Join berhasil → langsung ke quiz sebagai guest
            BattleRoomStatusResponse res =
                JsonUtility.FromJson<BattleRoomStatusResponse>(response);

            if (res != null && res.room_id > 0)
            {
                ScreenManager.Instance.ChangeScreen(
                    new BattleQuizScreen(container, res, false));
            }
            else
            {
                ShowStatus("Room tidak ditemukan atau penuh.", true);
            }
        }, (err) => {
            ShowStatus("Gagal bergabung. Kode salah?", true);
        });
    }

    void ShowStatus(string msg, bool isError)
    {
        if (statusTxt != null)
        {
            statusTxt.text = msg;
            statusTxt.color = isError ? TOBATheme.Error : TOBATheme.BatakGold;
        }
    }
}