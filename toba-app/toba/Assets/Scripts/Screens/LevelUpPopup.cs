
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class LevelUpPopup
{
    // ==========================================
    // MAIN
    // ==========================================

    public static void Show(Transform parent, int newLevel, string newTitle)
    {
        if (ScreenManager.Instance != null && parent != null)
        {
            ScreenManager.Instance.StartCoroutine(
                ShowCoroutine(parent, newLevel, newTitle)
            );
        }
    }

    static IEnumerator ShowCoroutine(
        Transform parent, int newLevel, string newTitle)
    {
        // OVERLAY
        GameObject overlay = new GameObject("LevelUpOverlay");
        overlay.transform.SetParent(parent, false);

        RectTransform overlayRect =
            overlay.AddComponent<RectTransform>();

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0.025f, 0.035f, 0.025f, 0.82f);
        overlayImage.raycastTarget = true;

        // MAIN CARD
        GameObject card = UIBuilder.CreatePanelSized(
            overlay.transform,
            "LevelCard",
            TOBATheme.WoodDark,
            new Vector2(760, 700),
            Vector2.zero
        );

        card.transform.localScale = Vector3.zero;

        AddOutline(card, TOBATheme.BatakGold, 5f);

        // CORNER GORGA ORNAMENTS
        CreateDiamond(card.transform, new Vector2(-350, 320), 20, TOBATheme.BatakGold);
        CreateDiamond(card.transform, new Vector2(350, 320), 20, TOBATheme.BatakGold);
        CreateDiamond(card.transform, new Vector2(-350, -320), 20, TOBATheme.BatakGold);
        CreateDiamond(card.transform, new Vector2(350, -320), 20, TOBATheme.BatakGold);

        // HEADER BANNER
        GameObject banner = CreatePanel(
            card.transform,
            "LevelBanner",
            TOBATheme.WoodMedium,
            new Vector2(710, 100),
            new Vector2(0, 275)
        );

        AddOutline(banner, TOBATheme.BatakGold, 3f);

        CreateLabel(
            banner.transform,
            "LevelUpTitle",
            "LEVEL NAIK!",
            44,
            TOBATheme.TextGold,
            new Vector2(680, 75),
            Vector2.zero
        );

        CreateDiamond(
            banner.transform,
            new Vector2(-330, 0),
            17,
            TOBATheme.Error
        );

        CreateDiamond(
            banner.transform,
            new Vector2(330, 0),
            17,
            TOBATheme.Error
        );

        // SUBTITLE
        CreateLabel(
            card.transform,
            "AchievementSubtitle",
            "PENCAPAIAN BARU",
            24,
            TOBATheme.TextGold,
            new Vector2(650, 40),
            new Vector2(0, 190)
        );

        // LEVEL MEDALLION
        GameObject levelPlate = CreatePanel(
            card.transform,
            "LevelMedallion",
            TOBATheme.WoodMedium,
            new Vector2(250, 225),
            new Vector2(0, 48)
        );

        AddOutline(levelPlate, TOBATheme.BatakGold, 4f);

        CreateDiamond(
            levelPlate.transform,
            new Vector2(-112, 98),
            12,
            TOBATheme.BatakGold
        );

        CreateDiamond(
            levelPlate.transform,
            new Vector2(112, 98),
            12,
            TOBATheme.BatakGold
        );

        CreateLabel(
            levelPlate.transform,
            "LevelLabel",
            "LEVEL",
            24,
            TOBATheme.CardBG,
            new Vector2(210, 38),
            new Vector2(0, 65)
        );

        CreateLabel(
            levelPlate.transform,
            "LevelNumber",
            newLevel.ToString(),
            145,
            TOBATheme.BatakGold,
            new Vector2(220, 155),
            new Vector2(0, -22)
        );

        // NEW TITLE
        if (!string.IsNullOrEmpty(newTitle))
        {
            CreateLabel(
                card.transform,
                "NewTitleLabel",
                "GELAR BARU",
                23,
                TOBATheme.TextGold,
                new Vector2(650, 38),
                new Vector2(0, -110)
            );

            CreateLabel(
                card.transform,
                "NewTitle",
                newTitle,
                36,
                TOBATheme.TextLight,
                new Vector2(670, 65),
                new Vector2(0, -150)
            );
        }

        // DIVIDER
        CreatePanel(
            card.transform,
            "RewardDivider",
            TOBATheme.BatakGold,
            new Vector2(550, 3),
            new Vector2(0, -205)
        );

        // XP REWARD
        CreateLabel(
            card.transform,
            "BonusXP",
            "+50 BONUS XP",
            38,
            TOBATheme.TextLight,
            new Vector2(650, 65),
            new Vector2(0, -240)
        );

        // CONTINUE BUTTON
        GameObject btnOk = UIBuilder.CreateButton(
            card.transform,
            "BtnContinue",
            "LANJUTKAN",
            new Vector2(550, 100),
            TOBATheme.BatakGold,
            () =>
            {
                Object.Destroy(overlay);
            }
        );

        RectTransform btnRect =
            btnOk.GetComponent<RectTransform>();

        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -310);

        Image btnImage = btnOk.GetComponent<Image>();

        if (btnImage != null)
            btnImage.color = TOBATheme.BatakGold;

        AddOutline(btnOk, TOBATheme.WoodLight, 3f);

        TextMeshProUGUI btnLabel =
            btnOk.GetComponentInChildren<TextMeshProUGUI>();

        if (btnLabel != null)
        {
            btnLabel.text = "LANJUTKAN";
            btnLabel.color = TOBATheme.WoodDark;
            btnLabel.fontStyle = FontStyles.Bold;
            btnLabel.fontSize = 34;
            btnLabel.alignment = TextAlignmentOptions.Center;
        }

        // ANIMATION
        yield return UIBuilder.AnimateScaleIn(card.transform, 0.35f);

        // GORGA CONFETTI
        for (int i = 0; i < 12; i++)
        {
            SpawnConfetti(overlay.transform);

            yield return new WaitForSecondsRealtime(0.07f);
        }
    }

    // ==========================================
    // UI HELPERS
    // ==========================================

    static GameObject CreatePanel(
        Transform parent,
        string name,
        Color color,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = UIBuilder.CreatePanelSized(
            parent, name, color, size, position
        );

        Image image = obj.GetComponent<Image>();

        if (image != null)
            image.raycastTarget = false;

        return obj;
    }

    static GameObject CreateLabel(
        Transform parent,
        string name,
        string value,
        int fontSize,
        Color color,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj = UIBuilder.CreateTextSized(
            parent,
            value,
            fontSize,
            color,
            TextAlignmentOptions.Center,
            size,
            position
        );

        obj.name = name;

        TextMeshProUGUI tmp =
            obj.GetComponent<TextMeshProUGUI>();

        if (tmp != null)
        {
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = fontSize * 0.75f;
            tmp.fontSizeMax = fontSize;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            tmp.margin = Vector4.zero;
        }

        return obj;
    }

    static void AddOutline(
        GameObject obj,
        Color color,
        float thickness)
    {
        Outline outline = obj.GetComponent<Outline>();

        if (outline == null)
            outline = obj.AddComponent<Outline>();

        outline.effectColor = color;
        outline.effectDistance =
            new Vector2(thickness, thickness);

        outline.useGraphicAlpha = true;
    }

    // ==========================================
    // GORGA GEOMETRIC DECORATION
    // ==========================================

    static void CreateDiamond(
        Transform parent,
        Vector2 position,
        float size,
        Color color)
    {
        GameObject diamond = CreatePanel(
            parent,
            "GorgaDiamond",
            color,
            new Vector2(size, size),
            position
        );

        RectTransform rect =
            diamond.GetComponent<RectTransform>();

        rect.localRotation =
            Quaternion.Euler(0, 0, 45f);
    }

    // ==========================================
    // PROCEDURAL CONFETTI
    // ==========================================

    static void SpawnConfetti(Transform parent)
    {
        Color[] palette =
        {
            TOBATheme.BatakGold,
            TOBATheme.Error,
            TOBATheme.TextLight,
            TOBATheme.ForestGreen,
            TOBATheme.WoodLight
        };

        Color color = palette[
            Random.Range(0, palette.Length)
        ];

        float size = Random.Range(10f, 19f);

        GameObject particle = CreatePanel(
            parent,
            "GorgaConfetti",
            color,
            new Vector2(size, size),
            new Vector2(
                Random.Range(-340f, 340f),
                Random.Range(-120f, 380f)
            )
        );

        RectTransform rect =
            particle.GetComponent<RectTransform>();

        rect.localRotation =
            Quaternion.Euler(0, 0, Random.Range(0f, 90f));

        Object.Destroy(particle, 1.8f);
    }
}
