using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GalleryScreen : BaseScreen
{
    private GalleryResponse galleryData;
    private GameObject loadingObj;
    private int totalCards = 20; // total kartu dari 10 lokasi (bisa dikonfigurasi)

    public GalleryScreen(Transform parent) : base(parent, "GalleryScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "GALERI BUDAYA", () => {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });


        loadingObj = UIBuilder.CreateTextSized(screenObj.transform,
            "Memuat koleksi...", 42, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(700, 80), Vector2.zero);

        FetchGallery();
    }

    void FetchGallery()
    {
        int playerID = PlayerPrefs.GetInt("PlayerID");
        APIManager.Instance.GetRequest("/player/" + playerID + "/gallery", (response) => {
            if (screenObj == null) return;

            galleryData = JsonUtility.FromJson<GalleryResponse>(response);
            if (loadingObj != null) Object.Destroy(loadingObj);
            BuildGallery();

        }, (err) => {
            if (loadingObj != null) Object.Destroy(loadingObj);
            UIBuilder.CreateTextSized(screenObj.transform,
                "Gagal memuat galeri.", 38, TOBATheme.Error,
                TextAlignmentOptions.Center, new Vector2(800, 80), Vector2.zero);
        });
    }

    void BuildGallery()
    {
        if (screenObj == null) return;

        int owned = galleryData != null ? galleryData.total : 0;

        // Header info koleksi
        GameObject infoBar = UIBuilder.CreatePanel(screenObj.transform, "InfoBar",
            TOBATheme.WoodDark, new Vector2(0, 0.82f), new Vector2(1, 0.88f));

        UIBuilder.CreateTextSized(infoBar.transform,
            "Koleksi Kamu : " + owned + " / " + totalCards,
            38, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(800, 60), Vector2.zero);

        // Progress bar koleksi
        GameObject progressBg = UIBuilder.CreatePanel(screenObj.transform, "ProgressBg",
            new Color(0, 0, 0, 0.3f), new Vector2(0, 0.78f), new Vector2(1, 0.82f));

        float fillRatio = totalCards > 0 ? (float)owned / totalCards : 0f;
        GameObject progressFill = UIBuilder.CreatePanel(progressBg.transform, "Fill",
            TOBATheme.BatakGold, Vector2.zero, new Vector2(fillRatio, 1f));
        progressFill.GetComponent<Image>().raycastTarget = false;

        // Grid kartu scroll
        ScrollRect scroll = UIBuilder.CreateScrollView(screenObj.transform,
            "GalleryScroll", new Vector2(0, 0.1f), new Vector2(1, 0.78f));

        Transform content = scroll.content;

        // Buat map kartu yang dimiliki
        HashSet<int> ownedIDs = new HashSet<int>();
        List<PlayerGalleryCardData> cards = new List<PlayerGalleryCardData>();

        if (galleryData != null && galleryData.cards != null)
        {
            cards = galleryData.cards;
            foreach (var c in cards) ownedIDs.Add(c.gallery_card_id);
        }

        // Layout grid 2 kolom
        int cols = 2;
        float cardW = 460f;
        float cardH = 340f;
        float padX = 30f;
        float padY = 30f;

        // Total slot = totalCards (10 lokasi)
        int rows = Mathf.CeilToInt((float)totalCards / cols);
        float totalHeight = rows * (cardH + padY) + 80f;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalHeight);

        for (int i = 0; i < totalCards; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float xPos = col == 0 ? -(cardW / 2 + padX / 2) : (cardW / 2 + padX / 2);
            float yPos = -(row * (cardH + padY)) - cardH / 2 - 40f;

            // Cari kartu yang cocok (berdasarkan index, kartu ke-i+1)
            PlayerGalleryCardData cardData = null;
            if (i < cards.Count) cardData = cards[i];

            CreateGalleryCard(content, cardData, xPos, yPos, cardW, cardH, i + 1);
        }
    }

    void CreateGalleryCard(Transform parent, PlayerGalleryCardData cardData,
        float x, float y, float w, float h, int slotNumber)
    {
        bool isOwned = cardData != null;

        // Border emas
        GameObject border = UIBuilder.CreatePanelSized(parent, "CardBorder_" + slotNumber,
            isOwned ? TOBATheme.BatakGold : TOBATheme.Locked,
            new Vector2(w + 6, h + 6), new Vector2(x, y));

        // Card inner
        GameObject card = UIBuilder.CreatePanelSized(border.transform, "Card",
            isOwned ? TOBATheme.CardBG : new Color(0.2f, 0.2f, 0.2f, 0.9f),
            new Vector2(w, h), Vector2.zero);

        if (isOwned)
        {
            // Gambar placeholder (warna biru)
            GameObject imgArea = UIBuilder.CreatePanelSized(card.transform, "ImgArea",
                TOBATheme.LakeBlue, new Vector2(w - 20, 180), new Vector2(0, 65f));

            // Judul kartu
            TextMeshProUGUI titleTmp = UIBuilder.CreateTextSized(card.transform,
                cardData.card.title, 34, TOBATheme.WoodDark, TextAlignmentOptions.Center,
                new Vector2(w - 30, 55), new Vector2(0, -80f))
                .GetComponent<TextMeshProUGUI>();
            titleTmp.fontStyle = FontStyles.Normal;
            UIBuilder.SetWordWrap(titleTmp, true);
            // Deskripsi singkat
            TextMeshProUGUI descTmp = UIBuilder.CreateTextSized(card.transform,
                cardData.card.description, 24, TOBATheme.TextGray, TextAlignmentOptions.Center,
                new Vector2(w - 30, 60), new Vector2(0, -135f))
                .GetComponent<TextMeshProUGUI>();
            descTmp.fontStyle = FontStyles.Normal;
            UIBuilder.SetWordWrap(descTmp, true);

            // Badge "DIMILIKI"
            GameObject badge = UIBuilder.CreatePanelSized(card.transform, "Badge",
                TOBATheme.BtnPrimary, new Vector2(160, 45), new Vector2(110f, 140f));
            UIBuilder.CreateTextSized(badge.transform, "✓ DIMILIKI", 22,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(155, 42), Vector2.zero);
        }
        else
        {
            // Slot kosong — tampilkan nomor & kunci
            UIBuilder.CreateTextSized(card.transform,
                "🔒", 80, TOBATheme.Locked, TextAlignmentOptions.Center,
                new Vector2(w, 120), new Vector2(0, 50f));

            UIBuilder.CreateTextSized(card.transform,
                "Kartu #" + slotNumber, 30, TOBATheme.Locked, TextAlignmentOptions.Center,
                new Vector2(w - 20, 50), new Vector2(0, -80f));

            UIBuilder.CreateTextSized(card.transform,
                "Selesaikan lokasi\nuntuk mendapatkan", 24,
                new Color(0.5f, 0.5f, 0.5f), TextAlignmentOptions.Center,
                new Vector2(w - 30, 70), new Vector2(0, -140f))
                .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
    }
}