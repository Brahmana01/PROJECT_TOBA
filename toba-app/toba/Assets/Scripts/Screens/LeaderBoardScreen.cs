using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardScreen : BaseScreen
{
    private List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>();
    private int myPlayerID;
    private GameObject loadingObj;

    public LeaderboardScreen(Transform parent) : base(parent, "LeaderboardScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        myPlayerID = PlayerPrefs.GetInt("PlayerID");

        HeaderBar.Create(screenObj.transform, "LEADERBOARD", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });


        loadingObj = UIBuilder.CreateTextSized(screenObj.transform,
            "Memuat leaderboard...", 42, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 80), Vector2.zero);

        FetchLeaderboard();
    }

    void FetchLeaderboard()
    {
        APIManager.Instance.GetRequest("/leaderboard", (response) => {
            if (screenObj == null) return;

            string wrapped = "{\"entries\":" + response + "}";
            LeaderboardListResponse res =
                JsonUtility.FromJson<LeaderboardListResponse>(wrapped);

            if (res != null && res.entries != null)
                entries = res.entries;

            if (loadingObj != null) Object.Destroy(loadingObj);
            BuildLeaderboard();

        }, (err) => {
            if (loadingObj != null) Object.Destroy(loadingObj);
            UIBuilder.CreateTextSized(screenObj.transform,
                "Gagal memuat leaderboard.", 38, TOBATheme.Error,
                TextAlignmentOptions.Center, new Vector2(800, 80), Vector2.zero);
        });
    }

    void BuildLeaderboard()
    {
        if (screenObj == null) return;

        // Cari rank player sendiri
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

        // Bar posisi saya
        GameObject myBar = UIBuilder.CreatePanel(screenObj.transform, "MyBar",
            TOBATheme.WoodDark, new Vector2(0, 0.82f), new Vector2(1, 0.88f));

        string myRankText = myRank > 0 ? "Posisimu : #" + myRank + "  ⭐ " + myStars : "Kamu belum di papan";
        UIBuilder.CreateTextSized(myBar.transform,
            myRankText, 36, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(900, 60), Vector2.zero);

        // Podium top 3
        BuildPodium();

        // Scroll list (rank 4+)
        ScrollRect scroll = UIBuilder.CreateScrollView(screenObj.transform,
            "LeaderScroll", new Vector2(0, 0.1f), new Vector2(1, 0.55f));

        Transform content = scroll.content;
        int listCount = Mathf.Max(0, entries.Count - 3);
        float rowH = 140f;
        content.GetComponent<RectTransform>().sizeDelta =
            new Vector2(0, listCount * rowH + 40f);

        for (int i = 3; i < entries.Count; i++)
        {
            float yPos = -((i - 3) * rowH) - 70f;
            CreateLeaderboardRow(content, entries[i], yPos, entries[i].id == myPlayerID);
        }
    }

    void BuildPodium()
    {
        // Area podium
        GameObject podiumArea = UIBuilder.CreatePanel(screenObj.transform,
            "PodiumArea", Color.clear, new Vector2(0, 0.55f), new Vector2(1, 0.82f));

        // Posisi podium: 2nd kiri, 1st tengah, 3rd kanan
        float[] xPos   = { -300f, 0f, 300f };
        float[] yPos   = { -30f, 30f, -60f };
        float[] heights = { 180f, 230f, 150f };
        int[]   ranks  = { 2, 1, 3 };
        Color[] podiumColors =
        {
            new Color(0.75f, 0.75f, 0.75f), // Perak
            TOBATheme.BatakGold,              // Emas
            new Color(0.8f, 0.5f, 0.2f)      // Perunggu
        };
        string[] medals = { "🥈", "🥇", "🥉" };

        for (int p = 0; p < 3; p++)
        {
            int rank = ranks[p];
            if (entries.Count < rank) continue;

            LeaderboardEntryData entry = entries[rank - 1];

            // Container
            GameObject podium = new GameObject("Podium_" + rank);
            podium.transform.SetParent(podiumArea.transform, false);
            RectTransform rtP = podium.AddComponent<RectTransform>();
            rtP.anchorMin = rtP.anchorMax = new Vector2(0.5f, 0.5f);
            rtP.sizeDelta = new Vector2(280, 300);
            rtP.anchoredPosition = new Vector2(xPos[p], yPos[p]);

            // Medal
            UIBuilder.CreateTextSized(podium.transform,
                medals[p], 60, Color.white, TextAlignmentOptions.Center,
                new Vector2(280, 80), new Vector2(0, 100f));

            // Avatar placeholder
            GameObject avatar = UIBuilder.CreatePanelSized(podium.transform,
                "Avatar", podiumColors[p], new Vector2(110, 110), new Vector2(0, 20f));

            UIBuilder.CreateTextSized(avatar.transform,
                entry.name.Substring(0, 1).ToUpper(), 55, Color.white,
                TextAlignmentOptions.Center, new Vector2(110, 110), Vector2.zero);

            // Nama
            TextMeshProUGUI nameTmp = UIBuilder.CreateTextSized(podium.transform,
                entry.name, 30, TOBATheme.TextLight, TextAlignmentOptions.Center,
                new Vector2(270, 50), new Vector2(0, -50f))
                .GetComponent<TextMeshProUGUI>();
            nameTmp.fontStyle = FontStyles.Normal;
            nameTmp.enableWordWrapping = false;

            // Bintang
            UIBuilder.CreateTextSized(podium.transform,
                "⭐ " + entry.stars, 32, TOBATheme.BatakGold, TextAlignmentOptions.Center,
                new Vector2(270, 45), new Vector2(0, -95f));

            // Kotak podium bawah
            GameObject podiumBox = UIBuilder.CreatePanelSized(podium.transform,
                "PodBox", podiumColors[p], new Vector2(260, heights[p]), new Vector2(0, -180f));
        }
    }

    void CreateLeaderboardRow(Transform parent, LeaderboardEntryData entry,
        float yPos, bool isMe)
    {
        Color rowColor = isMe
            ? new Color(TOBATheme.BatakGold.r, TOBATheme.BatakGold.g,
                TOBATheme.BatakGold.b, 0.2f)
            : TOBATheme.CardBG;

        GameObject row = UIBuilder.CreatePanelSized(parent, "Row_" + entry.rank,
            rowColor, new Vector2(1000, 120), new Vector2(0, yPos));

        Outline outline = row.AddComponent<Outline>();
        outline.effectColor = isMe ? TOBATheme.BatakGold : TOBATheme.CardBorder;
        outline.effectDistance = new Vector2(2, 2);

        // Rank number
        UIBuilder.CreateTextSized(row.transform,
            "#" + entry.rank, 40,
            isMe ? TOBATheme.BatakGold : TOBATheme.TextGray,
            TextAlignmentOptions.Center, new Vector2(90, 90), new Vector2(-430f, 0));

        // Avatar circle
        GameObject av = UIBuilder.CreatePanelSized(row.transform,
            "Av", TOBATheme.WoodMedium, new Vector2(80, 80), new Vector2(-340f, 0));
        UIBuilder.CreateTextSized(av.transform,
            entry.name.Substring(0, 1).ToUpper(), 40, Color.white,
            TextAlignmentOptions.Center, new Vector2(80, 80), Vector2.zero);

        // Nama & gelar
        TextMeshProUGUI nameTmp = UIBuilder.CreateTextSized(row.transform,
            entry.name, 36, TOBATheme.TextDark, TextAlignmentOptions.Left,
            new Vector2(420, 55), new Vector2(-60f, 20f))
            .GetComponent<TextMeshProUGUI>();
        nameTmp.fontStyle = FontStyles.Normal;

        TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(row.transform,
            entry.title_name + "  •  Lv." + entry.level,
            26, TOBATheme.TextGray, TextAlignmentOptions.Left,
            new Vector2(420, 40), new Vector2(-60f, -22f))
            .GetComponent<TextMeshProUGUI>();
        titleTmp.fontStyle = FontStyles.Normal;

        // Bintang
        UIBuilder.CreateTextSized(row.transform,
            "⭐ " + entry.stars, 36, TOBATheme.BatakGold, TextAlignmentOptions.Right,
            new Vector2(200, 55), new Vector2(370f, 0));

        // Label "KAMU"
        if (isMe)
        {
            GameObject meBadge = UIBuilder.CreatePanelSized(row.transform,
                "MeBadge", TOBATheme.BatakGold, new Vector2(110, 38), new Vector2(370f, -30f));
            UIBuilder.CreateTextSized(meBadge.transform,
                "KAMU", 22, TOBATheme.WoodDark, TextAlignmentOptions.Center,
                new Vector2(108, 36), Vector2.zero);
        }
    }
}