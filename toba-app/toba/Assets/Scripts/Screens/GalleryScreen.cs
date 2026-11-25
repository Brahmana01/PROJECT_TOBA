using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GalleryScreen : BaseScreen
{
    private GalleryResponse galleryData;
    private GameObject loadingObj;
    private GameObject emptyStateObj;
    private int totalCards;

    public GalleryScreen(Transform parent) : base(parent, "GalleryScreen") { }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        HeaderBar.Create(screenObj.transform, "GALERI BUDAYA", () =>
        {
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
        });

        ShowLoading();
        FetchGallery();
    }

    private void ShowLoading()
    {
        ClearEmptyState();
        ClearLoading();

        loadingObj = UIBuilder.CreateTextSized(
            screenObj.transform,
            "Memuat koleksi...",
            42,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(700f, 80f),
            Vector2.zero
        );
    }

    private void ClearLoading()
    {
        if (loadingObj != null)
        {
            Object.Destroy(loadingObj);
            loadingObj = null;
        }
    }

    private void ClearEmptyState()
    {
        if (emptyStateObj != null)
        {
            Object.Destroy(emptyStateObj);
            emptyStateObj = null;
        }
    }

    private void FetchGallery()
    {
        if (screenObj == null)
            return;

        ClearEmptyState();

        int playerID = PlayerPrefs.GetInt("PlayerID");
        if (playerID <= 0 || APIManager.Instance == null)
        {
            ClearLoading();
            emptyStateObj = EmptyState.Show(
                screenObj.transform,
                "<color=#F2E4C7><sprite name=\"gallery\" tint=1></color>",
                "Galeri tidak tersedia",
                "Silakan login dan periksa koneksi.",
                RetryFetchGallery
            );
            return;
        }

        APIManager.Instance.GetRequest(
            "/player/" + playerID + "/gallery",
            response =>
            {
                if (screenObj == null)
                    return;

                ClearLoading();

                try
                {
                    if (string.IsNullOrWhiteSpace(response))
                        throw new System.FormatException("Response galeri kosong.");

                    GalleryResponse parsed = JsonUtility.FromJson<GalleryResponse>(response);
                    if (parsed == null)
                        throw new System.FormatException("Struktur response galeri tidak valid.");

                    if (parsed.cards == null)
                        parsed.cards = new List<PlayerGalleryCardData>();

                    parsed.cards.RemoveAll(card => card == null);
                    parsed.total = Mathf.Max(0, parsed.total);
                    galleryData = parsed;
                    totalCards = galleryData.cards.Count;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[Gallery] Gagal membaca response: " + ex.Message);
                    galleryData = null;
                    totalCards = 0;
                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "<color=#F2E4C7><sprite name=\"gallery\" tint=1></color>",
                        "Data galeri bermasalah",
                        "Silakan muat ulang katalog.",
                        RetryFetchGallery
                    );
                    return;
                }

                // Katalog kosong berbeda dari pemain yang belum memiliki kartu.
                // Jika katalog ada, kartu yang belum dimiliki tetap dibuat sebagai slot terkunci.
                if (totalCards == 0)
                {
                    emptyStateObj = EmptyState.Show(
                        screenObj.transform,
                        "<color=#F2E4C7><sprite name=\"gallery\" tint=1></color>",
                        "Katalog belum tersedia",
                        "Kartu budaya akan muncul setelah katalog diisi oleh Admin.",
                        RetryFetchGallery
                    );
                    return;
                }

                BuildGallery();
            },
            error =>
            {
                if (screenObj == null)
                    return;

                Debug.LogWarning("[Gallery] Gagal memuat galeri: " + error);
                ClearLoading();
                emptyStateObj = EmptyState.Show(
                    screenObj.transform,
                    "<color=#F2E4C7><sprite name=\"gallery\" tint=1></color>",
                    "Gagal memuat galeri",
                    "Periksa koneksi ke server, lalu coba lagi.",
                    RetryFetchGallery
                );
            }
        );
    }

    private void RetryFetchGallery()
    {
        ClearEmptyState();
        ShowLoading();
        FetchGallery();
    }

    private void BuildGallery()
    {
        if (screenObj == null || galleryData == null)
            return;

        List<PlayerGalleryCardData> cards = galleryData.cards ?? new List<PlayerGalleryCardData>();
        totalCards = cards.Count;

        int owned = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].is_owned)
                owned++;
        }

        GameObject infoBar = UIBuilder.CreatePanel(
            screenObj.transform,
            "InfoBar",
            TOBATheme.WoodDark,
            new Vector2(0f, 0.82f),
            new Vector2(1f, 0.88f)
        );

        UIBuilder.CreateTextSized(
            infoBar.transform,
            "Koleksi Kamu : " + owned + " / " + totalCards,
            38,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(800f, 60f),
            Vector2.zero
        );

        GameObject progressBackground = UIBuilder.CreatePanel(
            screenObj.transform,
            "ProgressBg",
            new Color(0f, 0f, 0f, 0.3f),
            new Vector2(0f, 0.78f),
            new Vector2(1f, 0.82f)
        );

        float fillRatio = totalCards > 0 ? (float)owned / totalCards : 0f;
        GameObject progressFill = UIBuilder.CreatePanel(
            progressBackground.transform,
            "Fill",
            TOBATheme.BatakGold,
            Vector2.zero,
            new Vector2(fillRatio, 1f)
        );
        progressFill.GetComponent<Image>().raycastTarget = false;

        ScrollRect scroll = UIBuilder.CreateScrollView(
            screenObj.transform,
            "GalleryScroll",
            new Vector2(0f, 0.1f),
            new Vector2(1f, 0.78f)
        );

        Transform content = scroll.content;
        const int columns = 2;
        const float cardWidth = 460f;
        const float cardHeight = 340f;
        const float horizontalPadding = 30f;
        const float verticalPadding = 30f;

        int rows = Mathf.CeilToInt((float)totalCards / columns);
        float totalHeight = rows * (cardHeight + verticalPadding) + 80f;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, totalHeight);

        for (int i = 0; i < cards.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;

            float x = column == 0
                ? -(cardWidth / 2f + horizontalPadding / 2f)
                : (cardWidth / 2f + horizontalPadding / 2f);
            float y = -(row * (cardHeight + verticalPadding)) - cardHeight / 2f - 40f;

            CreateGalleryCard(content, cards[i], x, y, cardWidth, cardHeight, i + 1);
        }
    }

    private void CreateGalleryCard(
        Transform parent,
        PlayerGalleryCardData cardData,
        float x,
        float y,
        float width,
        float height,
        int slotNumber)
    {
        bool isOwned = cardData != null && cardData.is_owned && cardData.card != null;
        int cardNumber = cardData != null && cardData.gallery_card_id > 0
            ? cardData.gallery_card_id
            : slotNumber;

        GameObject border = UIBuilder.CreatePanelSized(
            parent,
            "CardBorder_" + slotNumber,
            isOwned ? TOBATheme.BatakGold : TOBATheme.Locked,
            new Vector2(width + 6f, height + 6f),
            new Vector2(x, y)
        );

        GameObject card = UIBuilder.CreatePanelSized(
            border.transform,
            "Card",
            isOwned ? TOBATheme.CardBG : new Color(0.2f, 0.2f, 0.2f, 0.9f),
            new Vector2(width, height),
            Vector2.zero
        );

        if (isOwned)
        {
            UIBuilder.CreatePanelSized(
                card.transform,
                "ImgArea",
                TOBATheme.LakeBlue,
                new Vector2(width - 20f, 180f),
                new Vector2(0f, 65f)
            );

            TextMeshProUGUI titleText = UIBuilder.CreateTextSized(
                card.transform,
                cardData.card.title,
                34,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center,
                new Vector2(width - 30f, 55f),
                new Vector2(0f, -80f)
            ).GetComponent<TextMeshProUGUI>();
            titleText.fontStyle = FontStyles.Normal;
            UIBuilder.SetWordWrap(titleText, true);

            TextMeshProUGUI descriptionText = UIBuilder.CreateTextSized(
                card.transform,
                cardData.card.description,
                24,
                TOBATheme.TextGray,
                TextAlignmentOptions.Center,
                new Vector2(width - 30f, 60f),
                new Vector2(0f, -135f)
            ).GetComponent<TextMeshProUGUI>();
            descriptionText.fontStyle = FontStyles.Normal;
            UIBuilder.SetWordWrap(descriptionText, true);

            GameObject badge = UIBuilder.CreatePanelSized(
                card.transform,
                "Badge",
                TOBATheme.BtnPrimary,
                new Vector2(160f, 45f),
                new Vector2(110f, 140f)
            );

            UIBuilder.CreateTextSized(
                badge.transform,
                "✓ DIMILIKI",
                22,
                Color.white,
                TextAlignmentOptions.Center,
                new Vector2(155f, 42f),
                Vector2.zero
            );
        }
        else
        {
            UIBuilder.CreateTextSized(
                card.transform,
                "🔒",
                80,
                TOBATheme.Locked,
                TextAlignmentOptions.Center,
                new Vector2(width, 120f),
                new Vector2(0f, 50f)
            );

            UIBuilder.CreateTextSized(
                card.transform,
                "Kartu #" + cardNumber,
                30,
                TOBATheme.Locked,
                TextAlignmentOptions.Center,
                new Vector2(width - 20f, 50f),
                new Vector2(0f, -80f)
            );

            UIBuilder.CreateTextSized(
                card.transform,
                "Selesaikan lokasi\nuntuk mendapatkan",
                24,
                new Color(0.5f, 0.5f, 0.5f),
                TextAlignmentOptions.Center,
                new Vector2(width - 30f, 70f),
                new Vector2(0f, -140f)
            ).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
    }
}
