using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardScreen : BaseScreen
{
    private List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>();
    private int myPlayerID;
    private GameObject loadingObj;
    private GameObject emptyStateObj;

    // Header papan (sama dengan layar Profil)
    const float HeaderMinY = 0.875f;
    const float HeaderMaxY = 0.995f;
    const float WoodCenterFromBottom = 0.459f;
    static float HeaderCenterY =>
        HeaderMinY + (HeaderMaxY - HeaderMinY) * WoodCenterFromBottom;
    const float BackButtonSize = 120f;
    const float BackButtonLeft = 30f;

    // Tata letak (pecahan tinggi layar)
    const float MyBarMinY = 0.805f;
    const float MyBarMaxY = 0.865f;
    const float PodiumMinY = 0.55f;
    const float PodiumMaxY = 0.80f;
    const float ListMinY = 0.02f;
    const float ListMaxY = 0.54f;

    const float RowHeight = 150f;
    const float RowGap = 14f;

    // Warna medali
    static readonly Color Gold = new Color(0.95f, 0.78f, 0.25f);
    static readonly Color Silver = new Color(0.78f, 0.80f, 0.84f);
    static readonly Color Bronze = new Color(0.80f, 0.50f, 0.22f);

    static Sprite circleSprite;

    public LeaderboardScreen(Transform parent) : base(parent, "LeaderboardScreen") { }

    // ───────────────────────── SHOW ─────────────────────────
    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);
        myPlayerID = PlayerPrefs.GetInt("PlayerID");

        // Latar gambar, selalu di belakang.
        UIBuilder.CreateBackgroundCover(screenObj.transform, LoadBackgroundSprite());

        CreateLeaderboardHeader();

        ShowLoading();
        FetchLeaderboard();
    }

    // ───────────────────────── SPRITE ─────────────────────────
    // Aman untuk mode Single maupun Multiple.
    Sprite LoadSpriteAny(string path)
    {
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;

        Sprite[] all = Resources.LoadAll<Sprite>(path);
        if (all != null && all.Length > 0) return all[0];

        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex != null)
        {
            return Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f
            );
        }
        return null;
    }

    // Coba beberapa lokasi umum sampai ketemu.
    Sprite LoadFirst(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            Sprite s = LoadSpriteAny(paths[i]);
            if (s != null) return s;
        }
        return null;
    }

    Sprite LoadBackgroundSprite()
    {
        Sprite sprite = LoadSpriteAny("bg_toba");
        if (sprite == null)
            Debug.LogWarning("[Leaderboard] Sprite bg_toba tidak ditemukan di Resources.");
        return sprite;
    }

    Sprite LoadPanelDeskripsi()
    {
        return LoadFirst("gambar/panel_deskripsi", "panel_deskripsi",
            "Trivia/panel_deskripsi", "Backgrounds/panel_deskripsi");
    }

    Sprite LoadPanelFakta()
    {
        return LoadFirst("gambar/panel_fakta", "panel_fakta",
            "Trivia/panel_fakta", "Backgrounds/panel_fakta");
    }

    static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
    }

    // ───────────────────────── HELPER UI ─────────────────────────
    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static void SetRect(RectTransform r, Vector2 aMin, Vector2 aMax, Vector2 pivot,
        Vector2 size, Vector2 pos)
    {
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.pivot = pivot;
        r.sizeDelta = size;
        r.anchoredPosition = pos;
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    // Panel: pakai sprite 9-slice kalau border-nya sudah diatur, kalau tidak pakai warna + outline.
    Image MakePanel(string name, Transform parent, Sprite sprite, Color fallback,
        Color outlineColor, float ppuMultiplier)
    {
        RectTransform rt = NewRect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.raycastTarget = false;

        bool sliced = sprite != null && sprite.border.sqrMagnitude > 0.01f;
        if (sliced)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = ppuMultiplier;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = fallback;
            Outline o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = outlineColor;
            o.effectDistance = new Vector2(3, 3);
        }
        return img;
    }

    TextMeshProUGUI MakeText(Transform parent, string text, int size, Color color,
        TextAlignmentOptions align, bool bold, bool autoSize)
    {
        GameObject g = UIBuilder.CreateTextSized(
            parent, text, size, color, align, new Vector2(100, 50), Vector2.zero
        );
        TextMeshProUGUI t = g.GetComponent<TextMeshProUGUI>();
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        t.raycastTarget = false;

        if (autoSize)
        {
            t.enableAutoSizing = true;
            t.fontSizeMax = size;
            t.fontSizeMin = Mathf.Max(12, size * 0.55f);
        }
        return t;
    }

    Image MakeCircle(string name, Transform parent, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = GetCircleSprite();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static string SafeName(LeaderboardEntryData e)
    {
        return string.IsNullOrEmpty(e.name) ? "?" : e.name;
    }

    static string SafeInitial(LeaderboardEntryData e)
    {
        return string.IsNullOrEmpty(e.name) ? "?" : e.name.Substring(0, 1).ToUpper();
    }

    // ───────────────────────── HEADER ─────────────────────────
    void CreateLeaderboardHeader()
    {
        GameObject headerRoot = new GameObject("LeaderboardHeader");
        headerRoot.transform.SetParent(screenObj.transform, false);
        RectTransform rtRoot = headerRoot.AddComponent<RectTransform>();
        Stretch(rtRoot);

        // Papan judul
        GameObject board = new GameObject("LeaderboardBoard");
        board.transform.SetParent(headerRoot.transform, false);
        RectTransform rtBoard = board.AddComponent<RectTransform>();
        rtBoard.anchorMin = new Vector2(0.02f, HeaderMinY);
        rtBoard.anchorMax = new Vector2(0.98f, HeaderMaxY);
        rtBoard.offsetMin = rtBoard.offsetMax = Vector2.zero;

        Sprite boardSprite = LoadSpriteAny("gambar/toba_header_panel");
        Image boardImg = board.AddComponent<Image>();
        if (boardSprite != null)
        {
            boardImg.sprite = boardSprite;
            boardImg.color = Color.white;
        }
        else
        {
            Debug.LogWarning("[Leaderboard] Sprite toba_header_panel tidak ditemukan.");
            boardImg.sprite = UIBuilder.GetDefaultSprite();
            boardImg.color = TOBATheme.WoodMedium;
        }
        boardImg.type = Image.Type.Simple;
        boardImg.preserveAspect = true;
        boardImg.raycastTarget = false;

        // Judul
        GameObject titleObj = UIBuilder.CreateTextSized(
            board.transform, "LEADERBOARD", 52, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(480f, 90f), new Vector2(0f, -9f)
        );
        TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.enableAutoSizing = true;
        titleTmp.fontSizeMin = 24;
        titleTmp.fontSizeMax = 52;
        titleTmp.raycastTarget = false;

        // Tombol kembali: lingkaran emas, sejajar tengah kayu
        Sprite backSprite = TOBATheme.LoadSprite("RegisterNameScreen/register_name_back_button_v2");

        GameObject btnObj = new GameObject("BackButton");
        btnObj.transform.SetParent(headerRoot.transform, false);
        RectTransform rtBtn = btnObj.AddComponent<RectTransform>();
        rtBtn.anchorMin = new Vector2(0f, HeaderCenterY);
        rtBtn.anchorMax = new Vector2(0f, HeaderCenterY);
        rtBtn.pivot = new Vector2(0f, 0.5f);
        rtBtn.sizeDelta = new Vector2(BackButtonSize, BackButtonSize);
        rtBtn.anchoredPosition = new Vector2(BackButtonLeft, 0f);

        Image btnImg = btnObj.AddComponent<Image>();
        if (backSprite != null)
        {
            btnImg.sprite = backSprite;
            btnImg.color = Color.white;
        }
        else
        {
            Debug.LogWarning("[Leaderboard] Sprite tombol kembali tidak ditemukan.");
            btnImg.sprite = UIBuilder.GetDefaultSprite();
            btnImg.color = TOBATheme.BatakGold;
        }
        btnImg.preserveAspect = true;
        btnImg.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => {
            SoundManager.Instance?.PlaySFX("sfx_click");
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });
    }

    // ───────────────────────── LOADING / EMPTY ─────────────────────────
    private void ShowLoading()
    {
        ClearEmptyState();

        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }

        loadingObj = UIBuilder.CreateTextSized(
            screenObj.transform, "Memuat leaderboard...", 42, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(700, 80), Vector2.zero
        );
    }

    private void ClearEmptyState()
    {
        if (emptyStateObj != null)
        {
            Object.Destroy(emptyStateObj);
            emptyStateObj = null;
        }
    }

    // ───────────────────────── DATA ─────────────────────────
    void FetchLeaderboard()
    {
        ClearEmptyState();

        APIManager.Instance.GetRequest(
            "/leaderboard",
            (response) => {
                if (screenObj == null) return;

                string wrapped = "{\"entries\":" + response + "}";
                LeaderboardListResponse res = JsonUtility.FromJson<LeaderboardListResponse>(wrapped);

                entries = res != null && res.entries != null
                    ? res.entries
                    : new List<LeaderboardEntryData>();

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                if (entries.Count == 0)
                {
                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "⭐",
                        "Leaderboard masih kosong",
                        "Belum ada pemain di papan peringkat",
                        () => RetryFetchLeaderboard()
                    );
                    return;
                }

                BuildLeaderboard();
            },
            (err) => {
                if (screenObj == null) return;

                Debug.LogWarning("[Leaderboard] Gagal memuat leaderboard: " + err);

                if (loadingObj != null)
                {
                    Object.Destroy(loadingObj);
                    loadingObj = null;
                }

                emptyStateObj = EmptyState.Show(
                    screenObj.transform,
                    "⭐",
                    "Gagal memuat leaderboard",
                    "Periksa koneksi internet",
                    () => RetryFetchLeaderboard()
                );
            }
        );
    }

    private void RetryFetchLeaderboard()
    {
        ClearEmptyState();
        ShowLoading();
        FetchLeaderboard();
    }

    // ───────────────────────── BUILD ─────────────────────────
    void BuildLeaderboard()
    {
        if (screenObj == null) return;

        int myRank = -1;
        int myStars = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].id == myPlayerID)
            {
                myRank = entries[i].rank;
                myStars = entries[i].stars;
                break;
            }
        }

        BuildMyBar(myRank, myStars);
        BuildPodium();
        BuildList();
    }

    // Bar "Posisimu"
    void BuildMyBar(int myRank, int myStars)
    {
        RectTransform barRt = NewRect("MyBar", screenObj.transform);
        barRt.anchorMin = new Vector2(0.04f, MyBarMinY);
        barRt.anchorMax = new Vector2(0.96f, MyBarMaxY);
        barRt.offsetMin = barRt.offsetMax = Vector2.zero;

        Image bg = MakePanel("Bg", barRt, LoadPanelFakta(), TOBATheme.WoodDark,
            TOBATheme.BatakGold, 1.5f);
        Stretch(bg.rectTransform);

        string txt = myRank > 0
            ? "Posisimu : #" + myRank + "  ⭐ " + myStars
            : "Kamu belum di papan";

        TextMeshProUGUI t = MakeText(barRt, txt, 38, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, true, true);
        t.rectTransform.anchorMin = new Vector2(0.05f, 0f);
        t.rectTransform.anchorMax = new Vector2(0.95f, 1f);
        t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
    }

    // Podium juara 1-3
    void BuildPodium()
    {
        RectTransform area = NewRect("PodiumArea", screenObj.transform);
        area.anchorMin = new Vector2(0f, PodiumMinY);
        area.anchorMax = new Vector2(1f, PodiumMaxY);
        area.offsetMin = area.offsetMax = Vector2.zero;

        Canvas.ForceUpdateCanvases();
        float k = area.rect.height > 1f ? Mathf.Clamp(area.rect.height / 480f, 0.6f, 1.2f) : 1f;

        float[] colMin = { 0.03f, 0.35f, 0.67f };
        float[] colMax = { 0.33f, 0.65f, 0.97f };
        int[] ranks = { 2, 1, 3 };
        float[] hFrac = { 0.24f, 0.34f, 0.18f };
        Color[] medalColors = { Silver, Gold, Bronze };

        for (int p = 0; p < 3; p++)
        {
            int rank = ranks[p];
            if (entries.Count < rank) continue;

            LeaderboardEntryData entry = entries[rank - 1];
            Color mc = medalColors[p];

            // Kolom
            RectTransform col = NewRect("Podium_" + rank, area);
            col.anchorMin = new Vector2(colMin[p], 0f);
            col.anchorMax = new Vector2(colMax[p], 1f);
            col.offsetMin = col.offsetMax = Vector2.zero;

            // Alas podium
            Image ped = MakePanel("PodBox", col, null, mc, TOBATheme.BatakGold, 1f);
            RectTransform pedRt = ped.rectTransform;
            pedRt.anchorMin = new Vector2(0.06f, 0f);
            pedRt.anchorMax = new Vector2(0.94f, hFrac[p]);
            pedRt.offsetMin = pedRt.offsetMax = Vector2.zero;

            // Garis terang di atas alas
            Image strip = MakeCircle("Strip", pedRt, new Color(1f, 1f, 1f, 0f));
            strip.sprite = null;
            strip.color = new Color(1f, 1f, 1f, 0.25f);
            SetRect(strip.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, 10f * k), Vector2.zero);

            // Bintang
            TextMeshProUGUI stars = MakeText(col, "⭐ " + entry.stars, Mathf.RoundToInt(32 * k),
                TOBATheme.BatakGold, TextAlignmentOptions.Center, true, true);
            SetRect(stars.rectTransform, new Vector2(0.5f, hFrac[p]), new Vector2(0.5f, hFrac[p]),
                new Vector2(0.5f, 0f), new Vector2(280f, 44f * k), new Vector2(0f, 8f * k));

            // Nama
            TextMeshProUGUI nm = MakeText(col, SafeName(entry), Mathf.RoundToInt(30 * k),
                TOBATheme.TextLight, TextAlignmentOptions.Center, false, true);
            SetRect(nm.rectTransform, new Vector2(0.5f, hFrac[p]), new Vector2(0.5f, hFrac[p]),
                new Vector2(0.5f, 0f), new Vector2(280f, 40f * k), new Vector2(0f, 54f * k));

            // Avatar: cincin emas + lingkaran kayu + inisial
            Image ring = MakeCircle("AvatarRing", col, TOBATheme.BatakGold);
            SetRect(ring.rectTransform, new Vector2(0.5f, hFrac[p]), new Vector2(0.5f, hFrac[p]),
                new Vector2(0.5f, 0f), new Vector2(112f * k, 112f * k), new Vector2(0f, 98f * k));

            Image av = MakeCircle("Avatar", ring.transform, TOBATheme.WoodMedium);
            SetRect(av.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(98f * k, 98f * k), Vector2.zero);

            TextMeshProUGUI ini = MakeText(av.transform, SafeInitial(entry), Mathf.RoundToInt(52 * k),
                Color.white, TextAlignmentOptions.Center, true, false);
            Stretch(ini.rectTransform);

            // Lencana medali (pengganti emoji): cincin emas + warna medali + angka
            Image badgeOuter = MakeCircle("MedalOuter", col, TOBATheme.BatakGold);
            SetRect(badgeOuter.rectTransform, new Vector2(0.5f, hFrac[p]), new Vector2(0.5f, hFrac[p]),
                new Vector2(0.5f, 0.5f), new Vector2(66f * k, 66f * k), new Vector2(54f * k, 196f * k));

            Image badgeInner = MakeCircle("MedalInner", badgeOuter.transform, mc);
            SetRect(badgeInner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(54f * k, 54f * k), Vector2.zero);

            TextMeshProUGUI num = MakeText(badgeInner.transform, rank.ToString(), Mathf.RoundToInt(34 * k),
                TOBATheme.WoodDark, TextAlignmentOptions.Center, true, false);
            Stretch(num.rectTransform);
        }
    }

    // Daftar peringkat 4 dst
    void BuildList()
    {
        ScrollRect scroll = UIBuilder.CreateScrollView(
            screenObj.transform, "LeaderScroll",
            new Vector2(0.03f, ListMinY), new Vector2(0.97f, ListMaxY)
        );

        RectTransform content = scroll.content;
        int listCount = Mathf.Max(0, entries.Count - 3);

        // Isi menempel di atas dan selebar area scroll
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, listCount * (RowHeight + RowGap) + 30f);

        Sprite rowSprite = LoadPanelDeskripsi();
        Sprite meSprite = LoadPanelFakta();

        for (int i = 3; i < entries.Count; i++)
        {
            float y = 10f + (i - 3) * (RowHeight + RowGap);
            CreateLeaderboardRow(content, entries[i], y, entries[i].id == myPlayerID,
                rowSprite, meSprite);
        }
    }

    void CreateLeaderboardRow(Transform parent, LeaderboardEntryData entry, float y,
        bool isMe, Sprite rowSprite, Sprite meSprite)
    {
        RectTransform row = NewRect("Row_" + entry.rank, parent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.sizeDelta = new Vector2(-24f, RowHeight);
        row.anchoredPosition = new Vector2(0f, -y);

        // Latar baris
        Image bg = isMe
            ? MakePanel("Bg", row, meSprite, TOBATheme.WoodDark, TOBATheme.BatakGold, 2f)
            : MakePanel("Bg", row, rowSprite, TOBATheme.CardBG, TOBATheme.BatakGold, 2f);
        Stretch(bg.rectTransform);

        Color mainText = isMe ? Color.white : TOBATheme.TextDark;
        Color subText = isMe ? new Color(1f, 0.92f, 0.7f) : TOBATheme.WoodMedium;
        Color accent = isMe ? TOBATheme.BatakGold : TOBATheme.WoodDark;

        // Peringkat
        TextMeshProUGUI rankTxt = MakeText(row, "#" + entry.rank, 42, accent,
            TextAlignmentOptions.Center, true, true);
        rankTxt.rectTransform.anchorMin = new Vector2(0.03f, 0.2f);
        rankTxt.rectTransform.anchorMax = new Vector2(0.15f, 0.8f);
        rankTxt.rectTransform.offsetMin = rankTxt.rectTransform.offsetMax = Vector2.zero;

        // Avatar bulat
        Image ring = MakeCircle("AvRing", row, TOBATheme.BatakGold);
        SetRect(ring.rectTransform, new Vector2(0.16f, 0.5f), new Vector2(0.16f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(90f, 90f), Vector2.zero);

        Image av = MakeCircle("Av", ring.transform, TOBATheme.WoodMedium);
        SetRect(av.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(78f, 78f), Vector2.zero);

        TextMeshProUGUI ini = MakeText(av.transform, SafeInitial(entry), 40, Color.white,
            TextAlignmentOptions.Center, true, false);
        Stretch(ini.rectTransform);

        // Nama + gelar
        float nameMax = isMe ? 0.60f : 0.73f;

        TextMeshProUGUI nm = MakeText(row, SafeName(entry), 38, mainText,
            TextAlignmentOptions.Left, true, true);
        nm.rectTransform.anchorMin = new Vector2(0.31f, 0.5f);
        nm.rectTransform.anchorMax = new Vector2(nameMax, 0.95f);
        nm.rectTransform.offsetMin = nm.rectTransform.offsetMax = Vector2.zero;

        string sub = (string.IsNullOrEmpty(entry.title_name) ? "" : entry.title_name + "  •  ")
            + "Lv." + entry.level;
        TextMeshProUGUI st = MakeText(row, sub, 28, subText,
            TextAlignmentOptions.Left, false, true);
        st.rectTransform.anchorMin = new Vector2(0.31f, 0.08f);
        st.rectTransform.anchorMax = new Vector2(0.73f, 0.5f);
        st.rectTransform.offsetMin = st.rectTransform.offsetMax = Vector2.zero;

        // Bintang
        TextMeshProUGUI stars = MakeText(row, "⭐ " + entry.stars, 38, TOBATheme.BatakGold,
            TextAlignmentOptions.Right, true, true);
        stars.rectTransform.anchorMin = new Vector2(0.74f, 0.2f);
        stars.rectTransform.anchorMax = new Vector2(0.96f, 0.8f);
        stars.rectTransform.offsetMin = stars.rectTransform.offsetMax = Vector2.zero;

        // Lencana KAMU (di samping nama, tidak menutupi bintang)
        if (isMe)
        {
            RectTransform pill = NewRect("MeBadge", row);
            pill.anchorMin = new Vector2(0.61f, 0.58f);
            pill.anchorMax = new Vector2(0.73f, 0.92f);
            pill.offsetMin = pill.offsetMax = Vector2.zero;

            Image pillImg = pill.gameObject.AddComponent<Image>();
            pillImg.sprite = GetCircleSprite();
            pillImg.type = Image.Type.Sliced;
            pillImg.color = TOBATheme.BatakGold;
            pillImg.raycastTarget = false;

            TextMeshProUGUI kamu = MakeText(pill, "KAMU", 24, TOBATheme.WoodDark,
                TextAlignmentOptions.Center, true, true);
            Stretch(kamu.rectTransform);
        }
    }
}