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
    private GameObject emptyStateObj;
    private GameObject loadingObj;
    GameObject contentArea;

    public ProfileScreen(Transform parent) : base(parent, "ProfileScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        // Latar gambar. Selalu di belakang karena memakai SetAsFirstSibling.
        UIBuilder.CreateBackgroundCover(screenObj.transform, LoadBackgroundSprite());

        HeaderBar.Create(screenObj.transform, "PROFIL", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        ShowLoading();
        FetchProfile();
    }

// Memuat bg_toba dari Resources. LoadSpriteFlexible milik MainMenu bersifat private,
    // jadi Profil memuat sendiri dengan pola yang sama.
    Sprite LoadBackgroundSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("bg_toba");
        if (sprite != null)
            return sprite;

        Texture2D tex = Resources.Load<Texture2D>("bg_toba");
        if (tex != null)
        {
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        Debug.LogWarning("[Profile] Sprite bg_toba tidak ditemukan di Resources.");
        return null;
    }
    
    void ShowLoading()
    {
        ClearEmptyState();

        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }

        loadingObj = UIBuilder.CreateTextSized(
            screenObj.transform, "Memuat profil...", 42, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(700, 80), Vector2.zero
        );
    }

    void ClearEmptyState()
    {
        if (emptyStateObj != null)
        {
            Object.Destroy(emptyStateObj);
            emptyStateObj = null;
        }
    }

    void BuildProfileUI()
    {
        // Pengaman: kalau terpanggil dua kali, buang wadah lama.
        if (contentArea != null)
        {
            Object.Destroy(contentArea);
            contentArea = null;
        }

        // ── CONTENT AREA: mengisi panel di bawah header ───────
        contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(screenObj.transform, false);
        RectTransform rtArea = contentArea.AddComponent<RectTransform>();
        rtArea.anchorMin = Vector2.zero;
        rtArea.anchorMax = Vector2.one;
        rtArea.offsetMin = Vector2.zero;
        rtArea.offsetMax = new Vector2(0f, -HeaderBar.Height);

        // ── FAKTOR PERKECIL SERAGAM ───────────────────────────
        // Paksa Canvas menghitung ukuran dulu supaya rect tidak masih 0.
        Canvas.ForceUpdateCanvases();
        Rect areaRect = rtArea.rect;
        float areaW = areaRect.width > 1f ? areaRect.width : 1080f;   // cadangan
        float areaH = areaRect.height > 1f ? areaRect.height : 1690f; // cadangan

        float widthFactor = areaW / 1000f;
        float heightFactor = (0.33f * areaH - 30f) / 420f;
        float s = Mathf.Clamp(Mathf.Min(widthFactor, heightFactor), 0.6f, 1f);

        // ── CARD AVATAR & INFO UTAMA ──────────────────────────
        GameObject topCard = UIBuilder.CreatePanelSized(
            contentArea.transform, "TopCard", TOBATheme.WoodDark,
            new Vector2(1000, 420), Vector2.zero
        );
        PlaceFromTop(topCard.GetComponent<RectTransform>(), 0.08f, new Vector2(1000, 420), s);

        Outline topOutline = topCard.AddComponent<Outline>();
        topOutline.effectColor = TOBATheme.BatakGold;
        topOutline.effectDistance = new Vector2(4, 4);

        // Avatar circle
        GameObject avatar = UIBuilder.CreatePanelSized(
            topCard.transform, "Avatar", TOBATheme.BatakGold,
            new Vector2(160, 160), new Vector2(-340f, 60f)
        );

        UIBuilder.CreateTextSized(
            avatar.transform, "👤", 80, TOBATheme.WoodDark,
            TextAlignmentOptions.Center, new Vector2(160, 160), Vector2.zero
        );

        // Nama
        nameTxt = UIBuilder.CreateTextSized(
            topCard.transform, "MEMUAT...", 52, Color.white,
            TextAlignmentOptions.Left, new Vector2(560, 70), new Vector2(100f, 100f)
        ).GetComponent<TextMeshProUGUI>();

        // Gelar
        titleTxt = UIBuilder.CreateTextSized(
            topCard.transform, "...", 32, TOBATheme.BatakGold,
            TextAlignmentOptions.Left, new Vector2(560, 50), new Vector2(100f, 40f)
        ).GetComponent<TextMeshProUGUI>();

        titleTxt.fontStyle = FontStyles.Normal;

        // Username
        usernameTxt = UIBuilder.CreateTextSized(
            topCard.transform, "@...", 28, TOBATheme.TextGray,
            TextAlignmentOptions.Left, new Vector2(560, 45), new Vector2(100f, -5f)
        ).GetComponent<TextMeshProUGUI>();

        usernameTxt.fontStyle = FontStyles.Normal;

        // Level
        levelTxt = UIBuilder.CreateTextSized(
            topCard.transform, "Level 1", 38, Color.white,
            TextAlignmentOptions.Left, new Vector2(260, 55), new Vector2(-50f, -60f)
        ).GetComponent<TextMeshProUGUI>();

        // Bintang
        starTxt = UIBuilder.CreateTextSized(
            topCard.transform, "⭐ 0", 38, TOBATheme.BatakGold,
            TextAlignmentOptions.Right, new Vector2(260, 55), new Vector2(320f, -60f)
        ).GetComponent<TextMeshProUGUI>();

        // XP Bar background
        GameObject xpBg = UIBuilder.CreatePanelSized(
            topCard.transform, "XPBg", new Color(0, 0, 0, 0.4f),
            new Vector2(900, 30), new Vector2(0, -120f)
        );

        // XP Bar fill
        xpBarFill = UIBuilder.CreatePanel(
            xpBg.transform, "XPFill", TOBATheme.BtnPrimary,
            Vector2.zero, new Vector2(0f, 1f)
        );

        xpBarFill.GetComponent<Image>().raycastTarget = false;

        // XP teks
        xpTxt = UIBuilder.CreateTextSized(
            topCard.transform, "0 / 500 XP", 28, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(900, 40), new Vector2(0, -155f)
        ).GetComponent<TextMeshProUGUI>();

        xpTxt.fontStyle = FontStyles.Normal;

        // ── CARD STATISTIK (260 tinggi) ───────────────────────
        GameObject statsCard = UIBuilder.CreatePanelSized(
            contentArea.transform, "StatsCard", TOBATheme.CardBG,
            new Vector2(1000, 260), Vector2.zero
        );
        PlaceFromTop(statsCard.GetComponent<RectTransform>(), 0.41f, new Vector2(1000, 260), s);

        Outline statsOutline = statsCard.AddComponent<Outline>();
        statsOutline.effectColor = TOBATheme.BatakGold;
        statsOutline.effectDistance = new Vector2(3, 3);

        UIBuilder.CreateTextSized(
            statsCard.transform, "📊 STATISTIK", 36, TOBATheme.WoodDark,
            TextAlignmentOptions.Left, new Vector2(900, 55), new Vector2(-10f, 92f)
        );

        // 3 stat boxes
        CreateStatBox(statsCard.transform, "🗺", "Lokasi", "0", -280f);
        CreateStatBox(statsCard.transform, "🏆", "Achievement", "0", 0f);
        CreateStatBox(statsCard.transform, "🖼", "Kartu", "0", 280f);

        // ── GRUP TOMBOL (270 tinggi: 120 + jarak 30 + 120) ────
        GameObject buttonGroup = new GameObject("ButtonGroup");
        buttonGroup.transform.SetParent(contentArea.transform, false);
        RectTransform rtGroup = buttonGroup.AddComponent<RectTransform>();
        PlaceFromTop(rtGroup, 0.75f, new Vector2(1000, 270), s);

        // Tombol Ganti Karakter (atas)
        GameObject btnGanti = UIBuilder.CreateButton(
            buttonGroup.transform, "BtnGantiKarakter", "🎭 GANTI KARAKTER",
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
            }
        );

        RectTransform rtGanti = btnGanti.GetComponent<RectTransform>();
        rtGanti.anchorMin = rtGanti.anchorMax = new Vector2(0.5f, 0.5f);
        rtGanti.anchoredPosition = new Vector2(0, 75f);

        // Tombol Logout (bawah)
        GameObject btnLogout = UIBuilder.CreateButton(
            buttonGroup.transform, "BtnLogout", "🚪 KELUAR",
            new Vector2(900, 120), TOBATheme.Error, () => {
                SoundManager.Instance?.PlaySFX("sfx_click");

                ConfirmDialog.Show(
                    screenObj.transform,
                    "Keluar Akun?",
                    "Progress kamu sudah tersimpan.\nYakin mau keluar?",
                    "Ya, Keluar",
                    () => {
                        SoundManager.Instance?.PlaySFX("sfx_click");

                        APIManager.Instance.PostRequest("/logout", "{}", (r) => { }, (e) => { });

                        PlayerPrefs.DeleteKey("UserToken");
                        PlayerPrefs.DeleteKey("SavedUsername");
                        PlayerPrefs.DeleteKey("PlayerID");
                        PlayerPrefs.DeleteKey("PlayerLevel");
                        PlayerPrefs.DeleteKey("SelectedCharacterID");
                        PlayerPrefs.Save();

                        ScreenManager.Instance.ChangeScreen(new WelcomeScreen(container));
                    },
                    "Batal"
                );
            }
        );

        RectTransform rtLogout = btnLogout.GetComponent<RectTransform>();
        rtLogout.anchorMin = rtLogout.anchorMax = new Vector2(0.5f, 0.5f);
        rtLogout.anchoredPosition = new Vector2(0, -75f);
    }

    void CreateStatBox(Transform parent, string icon, string label, string value, float xPos)
    {
        // Kotak 150 tinggi, tengahnya di y = -35 dari tengah StatsCard (260).
        GameObject box = UIBuilder.CreatePanelSized(
            parent, "Stat_" + label, new Color(0, 0, 0, 0.08f),
            new Vector2(270, 150), new Vector2(xPos, -35f)
        );

        UIBuilder.CreateTextSized(
            box.transform, icon, 42, TOBATheme.WoodDark,
            TextAlignmentOptions.Center, new Vector2(270, 55), new Vector2(0, 42f)
        );

        TextMeshProUGUI valTxt = UIBuilder.CreateTextSized(
            box.transform, value, 36, TOBATheme.WoodDark,
            TextAlignmentOptions.Center, new Vector2(270, 44), new Vector2(0, -8f)
        ).GetComponent<TextMeshProUGUI>();

        // Simpan referensi berdasarkan label
        if (label == "Lokasi") locationTxt = valTxt;
        if (label == "Achievement") achievementTxt = valTxt;
        if (label == "Kartu") galleryTxt = valTxt;

        TextMeshProUGUI lblTxt = UIBuilder.CreateTextSized(
            box.transform, label, 28, TOBATheme.TextGray,
            TextAlignmentOptions.Center, new Vector2(270, 40), new Vector2(0, -50f)
        ).GetComponent<TextMeshProUGUI>();

        lblTxt.fontStyle = FontStyles.Normal;
    }

    
    void FetchProfile()
    {
        ClearEmptyState();

        int playerID = PlayerPrefs.GetInt("PlayerID");

        if (playerID == 0)
        {
            if (loadingObj != null)
            {
                Object.Destroy(loadingObj);
                loadingObj = null;
            }

            emptyStateObj = EmptyState.Show(
                screenObj.transform, "👤", "Profil tidak tersedia",
                "Silakan login terlebih dahulu", () => {
                    ScreenManager.Instance.ChangeScreen(new WelcomeScreen(container));
                }
            );

            return;
        }

        APIManager.Instance.GetRequest(
            "/profile/" + playerID,
            (response) => {
                if (screenObj == null) return;

                profileData = JsonUtility.FromJson<ProfileResponse>(response);

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                if (profileData == null)
                {
                    Debug.LogWarning("[Profile] Data profile kosong / gagal di-parse.");

                    emptyStateObj = EmptyState.Show(
                        screenObj.transform, "👤", "Profil tidak tersedia",
                        "Coba muat ulang data profil", () => RetryFetchProfile()
                    );

                    return;
                }

                BuildProfileUI();
                UpdateUI();
            },
            (err) => {
                if (screenObj == null) return;

                Debug.LogWarning("[Profile] Gagal memuat profil: " + err);

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                emptyStateObj = EmptyState.Show(
                    screenObj.transform, "👤", "Gagal memuat profil",
                    "Periksa koneksi internetmu", () => RetryFetchProfile()
                );
            }
        );
    }

    void RetryFetchProfile()
    {
        // Bersihkan dulu, baru tampilkan loading (urutan lama terbalik).
        ClearEmptyState();

        if (contentArea != null)
        {
            Object.Destroy(contentArea);
            contentArea = null;
        }

        ShowLoading();
        FetchProfile();
    }

    // Menaruh elemen: menempel di persentase tinggi ContentArea (dari atas),
    // menggantung ke bawah, lalu dikecilkan seragam dari tepi atasnya.
    void PlaceFromTop(RectTransform rt, float topFraction, Vector2 size, float scale)
    {
        // Urutan: anchor, pivot, sizeDelta, anchoredPosition PALING AKHIR.
        rt.anchorMin = new Vector2(0.5f, 1f - topFraction);
        rt.anchorMax = new Vector2(0.5f, 1f - topFraction);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = new Vector3(scale, scale, 1f);
    }

    void UpdateUI()
    {
        if (nameTxt != null) nameTxt.text = profileData.name.ToUpper();
        if (usernameTxt != null) usernameTxt.text = "@" + profileData.username;
        if (titleTxt != null) titleTxt.text = profileData.title_name;
        if (levelTxt != null) levelTxt.text = "Level " + profileData.level;
        if (starTxt != null) starTxt.text = "⭐ " + profileData.stars;
        if (locationTxt != null) locationTxt.text = profileData.location_done.ToString();
        if (achievementTxt != null) achievementTxt.text = profileData.achievement_count.ToString();
        if (galleryTxt != null) galleryTxt.text = profileData.gallery_count.ToString();

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

        APIManager.Instance.GetRequest(
            "/check-achievements/" + playerID,
            (response) => {
                if (screenObj == null) return;

                NewAchievementResponse res = JsonUtility.FromJson<NewAchievementResponse>(response);

                if (res != null && res.count > 0)
                {
                    AchievementPopup.Show(screenObj.transform, res.newly_unlocked[0]);
                }
            },
            (err) => { }
        );
    }

    void ShowLogoutConfirm()
    {
        // Overlay konfirmasi
        GameObject overlay = UIBuilder.CreatePanel(
            screenObj.transform, "LogoutOverlay",
            new Color(0, 0, 0, 0.8f), Vector2.zero, Vector2.one
        );

        GameObject confirmCard = UIBuilder.CreatePanelSized(
            overlay.transform, "ConfirmCard", TOBATheme.CardBG,
            new Vector2(800, 400), Vector2.zero
        );

        Outline outline = confirmCard.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(4, 4);

        UIBuilder.CreateTextSized(
            confirmCard.transform, "Yakin ingin keluar?", 46, TOBATheme.WoodDark,
            TextAlignmentOptions.Center, new Vector2(750, 70), new Vector2(0, 120f)
        );

        UIBuilder.CreateTextSized(
            confirmCard.transform, "Kamu bisa login kembali kapan saja.", 32,
            TOBATheme.TextGray, TextAlignmentOptions.Center,
            new Vector2(720, 55), new Vector2(0, 50f)
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Tombol Ya
        GameObject btnYa = UIBuilder.CreateButton(
            confirmCard.transform, "BtnYa", "YA, KELUAR",
            new Vector2(320, 110), TOBATheme.Error, () => {
                PlayerPrefs.DeleteKey("UserToken");
                PlayerPrefs.DeleteKey("SavedUsername");
                PlayerPrefs.DeleteKey("PlayerID");
                PlayerPrefs.DeleteKey("PlayerLevel");
                PlayerPrefs.Save();

                ScreenManager.Instance.ChangeScreen(new WelcomeScreen(container));
            }
        );

        btnYa.GetComponent<RectTransform>().anchorMin =
            btnYa.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnYa.GetComponent<RectTransform>().anchoredPosition = new Vector2(180f, -100f);

        // Tombol Batal
        GameObject btnBatal = UIBuilder.CreateButton(
            confirmCard.transform, "BtnBatal", "BATAL",
            new Vector2(320, 110), TOBATheme.BtnSecondary, () => {
                Object.Destroy(overlay);
            }
        );

        btnBatal.GetComponent<RectTransform>().anchorMin =
            btnBatal.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnBatal.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180f, -100f);
    }

    void ShowToast(string message)
    {
        GameObject toast = UIBuilder.CreatePanelSized(
            screenObj.transform, "Toast", new Color(0.1f, 0.1f, 0.1f, 0.9f),
            new Vector2(800, 90), new Vector2(0, -700f)
        );

        Outline outline = toast.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(2, 2);

        UIBuilder.CreateTextSized(
            toast.transform, message, 32, Color.white,
            TextAlignmentOptions.Center, new Vector2(770, 80), Vector2.zero
        ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        ScreenManager.Instance.StartCoroutine(DestroyAfter(toast, 2.5f));
    }

    System.Collections.IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (obj != null)
            Object.Destroy(obj);
    }
}