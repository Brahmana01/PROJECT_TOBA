using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileScreen : BaseScreen
{
    private ProfileResponse profileData;
    private TextMeshProUGUI nameTxt;
    private TextMeshProUGUI usernameTxt;
    private TextMeshProUGUI levelTxt;
    private TextMeshProUGUI titleTxt;
    private TextMeshProUGUI starTxt;
    private TextMeshProUGUI xpTxt;
    private TextMeshProUGUI locationTxt;
    private TextMeshProUGUI achievementTxt;
    private TextMeshProUGUI galleryTxt;
    private GameObject xpBarFill;

    public ProfileScreen(Transform parent) : base(parent, "ProfileScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "PROFIL", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        BuildProfileUI();
        FetchProfile();
    }

    void BuildProfileUI()
    {
        // ── CARD AVATAR & INFO UTAMA ──────────────────────────
        GameObject topCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "TopCard", TOBATheme.WoodDark, new Vector2(1000, 420), new Vector2(0, 520f));

        Outline topOutline = topCard.AddComponent<Outline>();
        topOutline.effectColor = TOBATheme.BatakGold;
        topOutline.effectDistance = new Vector2(4, 4);

        // Avatar circle
        GameObject avatar = UIBuilder.CreatePanelSized(topCard.transform,
            "Avatar", TOBATheme.BatakGold, new Vector2(160, 160), new Vector2(-340f, 60f));

        UIBuilder.CreateTextSized(avatar.transform,
            "👤", 80, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(160, 160), Vector2.zero);

        // Nama
        nameTxt = UIBuilder.CreateTextSized(topCard.transform,
            "MEMUAT...", 52, Color.white, TextAlignmentOptions.Left,
            new Vector2(560, 70), new Vector2(100f, 100f))
            .GetComponent<TextMeshProUGUI>();

        // Gelar
        titleTxt = UIBuilder.CreateTextSized(topCard.transform,
            "...", 32, TOBATheme.BatakGold, TextAlignmentOptions.Left,
            new Vector2(560, 50), new Vector2(100f, 40f))
            .GetComponent<TextMeshProUGUI>();
        titleTxt.fontStyle = FontStyles.Normal;

        // Username
        usernameTxt = UIBuilder.CreateTextSized(topCard.transform,
            "@...", 28, TOBATheme.TextGray, TextAlignmentOptions.Left,
            new Vector2(560, 45), new Vector2(100f, -5f))
            .GetComponent<TextMeshProUGUI>();
        usernameTxt.fontStyle = FontStyles.Normal;

        // Level
        levelTxt = UIBuilder.CreateTextSized(topCard.transform,
            "Level 1", 38, Color.white, TextAlignmentOptions.Left,
            new Vector2(260, 55), new Vector2(-50f, -60f))
            .GetComponent<TextMeshProUGUI>();

        // Bintang
        starTxt = UIBuilder.CreateTextSized(topCard.transform,
            "⭐ 0", 38, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(260, 55), new Vector2(320f, -60f))
            .GetComponent<TextMeshProUGUI>();

        // XP Bar background
        GameObject xpBg = UIBuilder.CreatePanelSized(topCard.transform,
            "XPBg", new Color(0, 0, 0, 0.4f), new Vector2(900, 30), new Vector2(0, -120f));

        // XP Bar fill
        xpBarFill = UIBuilder.CreatePanel(xpBg.transform, "XPFill",
            TOBATheme.BtnPrimary, Vector2.zero, new Vector2(0f, 1f));
        xpBarFill.GetComponent<Image>().raycastTarget = false;

        // XP teks
        xpTxt = UIBuilder.CreateTextSized(topCard.transform,
            "0 / 500 XP", 28, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(900, 40), new Vector2(0, -155f))
            .GetComponent<TextMeshProUGUI>();
        xpTxt.fontStyle = FontStyles.Normal;

        // ── CARD STATISTIK ────────────────────────────────────
        GameObject statsCard = UIBuilder.CreatePanelSized(screenObj.transform,
            "StatsCard", TOBATheme.CardBG, new Vector2(1000, 220), new Vector2(0, 190f));

        Outline statsOutline = statsCard.AddComponent<Outline>();
        statsOutline.effectColor = TOBATheme.BatakGold;
        statsOutline.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(statsCard.transform,
            "📊 STATISTIK", 36, TOBATheme.WoodDark, TextAlignmentOptions.Left,
            new Vector2(900, 55), new Vector2(-10f, 70f));

        // 3 stat boxes
        CreateStatBox(statsCard.transform, "🗺", "Lokasi", "0", -280f);
        CreateStatBox(statsCard.transform, "🏆", "Achievement", "0", 0f);
        CreateStatBox(statsCard.transform, "🖼", "Kartu", "0", 280f);

        // ── CARD AKSI ─────────────────────────────────────────
        // Tombol Ganti Karakter
        GameObject btnGanti = UIBuilder.CreateButton(screenObj.transform,
            "BtnGantiKarakter", "🎭 GANTI KARAKTER",
            new Vector2(900, 120), TOBATheme.WoodMedium, () => {
                int level = PlayerPrefs.GetInt("PlayerLevel", 1);
                if (level >= 5)
                {
                    SoundManager.Instance.PlaySFX("sfx_click");
                    ScreenManager.Instance.ChangeScreen(new CharacterSelectScreen(container));
                }
                else
                {
                    ShowToast("Ganti karakter tersedia di Level 5!");
                }
            });
        btnGanti.GetComponent<RectTransform>().anchorMin =
            btnGanti.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnGanti.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -210f);

        // Tombol Logout
        GameObject btnLogout = UIBuilder.CreateButton(screenObj.transform,
            "BtnLogout", "🚪 KELUAR",
            new Vector2(900, 120), TOBATheme.Error, () => {
                SoundManager.Instance.PlaySFX("sfx_click");
                ShowLogoutConfirm();
            });
        btnLogout.GetComponent<RectTransform>().anchorMin =
            btnLogout.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnLogout.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -360f);
    }

    void CreateStatBox(Transform parent, string icon, string label,
        string value, float xPos)
    {
        GameObject box = UIBuilder.CreatePanelSized(parent, "Stat_" + label,
            new Color(0, 0, 0, 0.08f), new Vector2(270, 110), new Vector2(xPos, -45f));

        UIBuilder.CreateTextSized(box.transform,
            icon, 42, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(270, 55), new Vector2(0, 20f));

        TextMeshProUGUI valTxt = UIBuilder.CreateTextSized(box.transform,
            value, 36, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(270, 44), new Vector2(0, -20f))
            .GetComponent<TextMeshProUGUI>();

        // Simpan referensi berdasarkan label
        if (label == "Lokasi")       locationTxt    = valTxt;
        if (label == "Achievement")  achievementTxt = valTxt;
        if (label == "Kartu")        galleryTxt     = valTxt;

        TextMeshProUGUI lblTxt = UIBuilder.CreateTextSized(box.transform,
            label, 22, TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(270, 35), new Vector2(0, -52f))
            .GetComponent<TextMeshProUGUI>();
        lblTxt.fontStyle = FontStyles.Normal;
    }

    void FetchProfile()
    {
        int playerID = PlayerPrefs.GetInt("PlayerID");
        if (playerID == 0) return;

        APIManager.Instance.GetRequest("/profile/" + playerID, (response) => {
            if (screenObj == null) return;

            profileData = JsonUtility.FromJson<ProfileResponse>(response);
            if (profileData == null) return;

            UpdateUI();

        }, (err) => {
            if (nameTxt != null) nameTxt.text = "OFFLINE";
        });
    }

    void UpdateUI()
    {
        if (nameTxt != null)       nameTxt.text       = profileData.name.ToUpper();
        if (usernameTxt != null)   usernameTxt.text   = "@" + profileData.username;
        if (titleTxt != null)      titleTxt.text      = profileData.title_name;
        if (levelTxt != null)      levelTxt.text      = "Level " + profileData.level;
        if (starTxt != null)       starTxt.text       = "⭐ " + profileData.stars;
        if (locationTxt != null)   locationTxt.text   = profileData.location_done.ToString();
        if (achievementTxt != null) achievementTxt.text = profileData.achievement_count.ToString();
        if (galleryTxt != null)    galleryTxt.text    = profileData.gallery_count.ToString();

        // XP progress bar
        float xpRatio = profileData.xp_for_next > 0
            ? (float)profileData.xp_progress / profileData.xp_for_next
            : 0f;
        xpRatio = Mathf.Clamp01(xpRatio);

        if (xpBarFill != null)
        {
            RectTransform rt = xpBarFill.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(xpRatio, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        if (xpTxt != null)
            xpTxt.text = profileData.xp_progress + " / " + profileData.xp_for_next + " XP";

        // Simpan level ke prefs
        PlayerPrefs.SetInt("PlayerLevel", profileData.level);

        // Cek achievement baru
        CheckNewAchievements();
    }

    void CheckNewAchievements()
    {
        int playerID = PlayerPrefs.GetInt("PlayerID");
        APIManager.Instance.GetRequest("/check-achievements/" + playerID, (response) => {
            if (screenObj == null) return;

            NewAchievementResponse res =
                JsonUtility.FromJson<NewAchievementResponse>(response);

            if (res != null && res.count > 0)
            {
                // Tampilkan popup achievement pertama yang baru
                AchievementPopup.Show(screenObj.transform, res.newly_unlocked[0]);
            }
        }, (err) => { });
    }

    void ShowLogoutConfirm()
    {
        // Overlay konfirmasi
        GameObject overlay = UIBuilder.CreatePanel(screenObj.transform,
            "LogoutOverlay", new Color(0, 0, 0, 0.8f), Vector2.zero, Vector2.one);

        GameObject confirmCard = UIBuilder.CreatePanelSized(overlay.transform,
            "ConfirmCard", TOBATheme.CardBG, new Vector2(800, 400), Vector2.zero);

        Outline outline = confirmCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4, 4);

        UIBuilder.CreateTextSized(confirmCard.transform,
            "Yakin ingin keluar?", 46, TOBATheme.WoodDark, TextAlignmentOptions.Center,
            new Vector2(750, 70), new Vector2(0, 120f));

        UIBuilder.CreateTextSized(confirmCard.transform,
            "Kamu bisa login kembali kapan saja.", 32, TOBATheme.TextGray,
            TextAlignmentOptions.Center, new Vector2(720, 55), new Vector2(0, 50f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Tombol Ya
        GameObject btnYa = UIBuilder.CreateButton(confirmCard.transform,
            "BtnYa", "YA, KELUAR",
            new Vector2(320, 110), TOBATheme.Error, () => {
                PlayerPrefs.DeleteKey("UserToken");
                PlayerPrefs.DeleteKey("SavedUsername");
                PlayerPrefs.DeleteKey("PlayerID");
                PlayerPrefs.DeleteKey("PlayerLevel");
                PlayerPrefs.Save();
                ScreenManager.Instance.ChangeScreen(new WelcomeScreen(container));
            });
        btnYa.GetComponent<RectTransform>().anchorMin =
            btnYa.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnYa.GetComponent<RectTransform>().anchoredPosition = new Vector2(180f, -100f);

        // Tombol Batal
        GameObject btnBatal = UIBuilder.CreateButton(confirmCard.transform,
            "BtnBatal", "BATAL",
            new Vector2(320, 110), TOBATheme.BtnSecondary, () => {
                Object.Destroy(overlay);
            });
        btnBatal.GetComponent<RectTransform>().anchorMin =
            btnBatal.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnBatal.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180f, -100f);
    }

    void ShowToast(string message)
    {
        GameObject toast = UIBuilder.CreatePanelSized(screenObj.transform,
            "Toast", new Color(0.1f, 0.1f, 0.1f, 0.9f),
            new Vector2(800, 90), new Vector2(0, -700f));

        Outline outline = toast.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2, 2);

        UIBuilder.CreateTextSized(toast.transform,
            message, 32, Color.white, TextAlignmentOptions.Center,
            new Vector2(770, 80), Vector2.zero)
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        ScreenManager.Instance.StartCoroutine(DestroyAfter(toast, 2.5f));
    }

    System.Collections.IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (obj != null) Object.Destroy(obj);
    }
}