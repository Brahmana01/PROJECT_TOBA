using UnityEngine;

public static class TOBATheme
{
    // ── WARNA UTAMA ───────────────────────────────────────
    public static Color ForestGreen    = HexToColor("#1A3A1A");
    public static Color LakeBlue       = HexToColor("#2E6B8A");
    public static Color BatakGold      = HexToColor("#D4AF37");
    public static Color WoodDark       = HexToColor("#3D1F0A");
    public static Color WoodMedium     = HexToColor("#6B3A1F");
    public static Color WoodLight      = HexToColor("#A0622A");
    public static Color CardBG         = HexToColor("#F5E6C8");
    public static Color CardBorder     = HexToColor("#A0522D");

    // ── WARNA TEKS ────────────────────────────────────────
    public static Color TextDark       = HexToColor("#2C1810");
    public static Color TextLight      = HexToColor("#FFF8E7");
    public static Color TextGold       = HexToColor("#D4AF37");
    public static Color TextGray       = HexToColor("#8B7355");

    // ── WARNA STATUS ──────────────────────────────────────
    public static Color Success        = HexToColor("#27AE60");
    public static Color Error          = HexToColor("#C0392B");
    public static Color Warning        = HexToColor("#F39C12");
    public static Color Locked         = HexToColor("#5A5A5A");

    // ── WARNA TOMBOL ──────────────────────────────────────
    public static Color BtnPrimary     = HexToColor("#3A7D1E");
    public static Color BtnSecondary   = HexToColor("#5C3317");
    public static Color BtnDanger      = HexToColor("#C0392B");

    // ── WARNA OVERLAY ─────────────────────────────────────
    public static Color OverlayDark    = new Color(0, 0, 0, 0.65f);
    public static Color OverlayLight   = new Color(1, 1, 1, 0.08f);

    // ── WARNA QUIZ ────────────────────────────────────────
    public static Color OptionNormal   = HexToColor("#F5E6C8");
    public static Color OptionCorrect  = HexToColor("#27AE60");
    public static Color OptionWrong    = HexToColor("#C0392B");

    // ── WARNA BINTANG ─────────────────────────────────────
    public static Color StarActive     = HexToColor("#F1C40F");
    public static Color StarInactive   = HexToColor("#6B6B6B");

    // ── BACKGROUND ────────────────────────────────────────
    public static Color BGDark         = HexToColor("#0D1A0D");

    // ── HELPER ────────────────────────────────────────────
    private static Color HexToColor(string hex)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    // ── LOAD SPRITE dari Resources ────────────────────────
    public static Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning("[TOBATheme] Sprite tidak ditemukan: " + path);
        return sprite;
    }

    // ── LOAD TEXTURE sebagai Sprite ───────────────────────
    public static Sprite TextureToSprite(Texture2D tex)
    {
        if (tex == null) return null;
        return Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), 100f);
    }
}