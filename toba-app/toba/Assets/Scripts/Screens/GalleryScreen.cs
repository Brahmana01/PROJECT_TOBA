using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GalleryScreen : BaseScreen
{
    private GalleryResponse galleryData;
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
        if (screenObj != null)
            LoadingOverlay.Show(screenObj.transform, "Memuat koleksi budaya...");
    }

    private void ClearLoading()
    {
        if (screenObj != null)
            LoadingOverlay.Hide(screenObj.transform);
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

    private Sprite LoadGallerySprite(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
            return null;

        string resourceName = imageName.Trim().Replace("\\", "/");
        int slashIndex = resourceName.LastIndexOf('/');
        if (slashIndex >= 0)
            resourceName = resourceName.Substring(slashIndex + 1);

        int extensionIndex = resourceName.LastIndexOf('.');
        if (extensionIndex > 0)
            resourceName = resourceName.Substring(0, extensionIndex);

        Sprite sprite = Resources.Load<Sprite>("gambar/" + resourceName);
        if (sprite != null)
            return sprite;

        Sprite[] sprites = Resources.LoadAll<Sprite>("gambar/" + resourceName);
        if (sprites != null && sprites.Length > 0)
            return sprites[0];

        Texture2D texture = Resources.Load<Texture2D>("gambar/" + resourceName);
        if (texture == null)
            texture = Resources.Load<Texture2D>(resourceName);

        return texture != null ? TOBATheme.TextureToSprite(texture) : null;
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
        bool hasCardData = cardData != null && cardData.card != null;
        bool isOwned = hasCardData && cardData.is_owned;
        int cardNumber = cardData != null && cardData.gallery_card_id > 0
            ? cardData.gallery_card_id
            : slotNumber;

        string title = hasCardData && !string.IsNullOrWhiteSpace(cardData.card.title)
            ? cardData.card.title
            : "Kartu #" + cardNumber;
        string description = hasCardData && !string.IsNullOrWhiteSpace(cardData.card.description)
            ? cardData.card.description
            : "Informasi kartu budaya belum tersedia.";

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
            isOwned ? TOBATheme.CardBG : new Color(0.16f, 0.18f, 0.16f, 0.96f),
            new Vector2(width, height),
            Vector2.zero
        );

        GameObject imageArea = UIBuilder.CreatePanelSized(
            card.transform,
            "ImgArea",
            TOBATheme.LakeBlue,
            new Vector2(width - 20f, 180f),
            new Vector2(0f, 65f)
        );

        Image galleryImage = imageArea.GetComponent<Image>();
        Sprite gallerySprite = hasCardData
            ? LoadGallerySprite(cardData.card.image)
            : null;

        if (galleryImage != null)
        {
            galleryImage.raycastTarget = false;
            if (gallerySprite != null)
            {
                galleryImage.sprite = gallerySprite;
                galleryImage.type = Image.Type.Simple;
                galleryImage.preserveAspect = true;
                galleryImage.color = isOwned
                    ? Color.white
                    : new Color(0.38f, 0.42f, 0.40f, 0.78f);
            }
            else
            {
                galleryImage.color = new Color(0.12f, 0.25f, 0.30f, 1f);
            }
        }

        if (gallerySprite == null)
        {
            UIBuilder.CreateTextSized(
                imageArea.transform,
                "GAMBAR BELUM TERSEDIA",
                20,
                TOBATheme.TextLight,
                TextAlignmentOptions.Center,
                new Vector2(width - 40f, 55f),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>().raycastTarget = false;
        }

        if (!isOwned)
        {
            GameObject lockShade = UIBuilder.CreatePanelSized(
                imageArea.transform,
                "LockedShade",
                new Color(0f, 0f, 0f, 0.34f),
                new Vector2(width - 20f, 180f),
                Vector2.zero
            );
            Image shadeImage = lockShade.GetComponent<Image>();
            if (shadeImage != null)
                shadeImage.raycastTarget = false;

            UIBuilder.CreateTextSized(
                imageArea.transform,
                "🔒",
                58,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(100f, 100f),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>().raycastTarget = false;
        }

        TextMeshProUGUI titleText = UIBuilder.CreateTextSized(
            card.transform,
            title,
            31,
            isOwned ? TOBATheme.WoodDark : TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(width - 30f, 55f),
            new Vector2(0f, -80f)
        ).GetComponent<TextMeshProUGUI>();
        titleText.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(titleText, true);

        TextMeshProUGUI descriptionText = UIBuilder.CreateTextSized(
            card.transform,
            description,
            21,
            isOwned ? TOBATheme.TextGray : new Color(0.82f, 0.82f, 0.78f),
            TextAlignmentOptions.Center,
            new Vector2(width - 30f, 62f),
            new Vector2(0f, -137f)
        ).GetComponent<TextMeshProUGUI>();
        descriptionText.fontStyle = FontStyles.Normal;
        UIBuilder.SetWordWrap(descriptionText, true);

        GameObject badge = UIBuilder.CreatePanelSized(
            card.transform,
            "Badge",
            isOwned ? TOBATheme.BtnPrimary : TOBATheme.Locked,
            new Vector2(190f, 42f),
            new Vector2(0f, 145f)
        );

        UIBuilder.CreateTextSized(
            badge.transform,
            isOwned ? "✓ DIMILIKI" : "🔒 TERKUNCI",
            21,
            Color.white,
            TextAlignmentOptions.Center,
            new Vector2(185f, 40f),
            Vector2.zero
        ).GetComponent<TextMeshProUGUI>().raycastTarget = false;
    }

}
