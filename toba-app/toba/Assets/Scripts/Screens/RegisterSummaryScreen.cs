using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RegisterSummaryScreen : BaseScreen
{
    private AuthResponse authData;
    private bool isStarting = false;

    public RegisterSummaryScreen(
        Transform parent,
        AuthResponse data
    ) : base(parent, "RegisterSummary")
    {
        authData = data;
    }

    public override void Show()
    {
        SetBackground(TOBATheme.ForestGreen);

        CreateBackground();
        CreateHeader();
        CreateBoard();
        CreateAvatar();
        CreateWelcome();
        CreateAccountInfo();
        CreateUsernameNote();
        CreateStartButton();

        ScreenManager.Instance.StartCoroutine(
            Entrance()
        );
    }

    // ============================================================
    // SPRITE
    // ============================================================

    Sprite Load(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    // ============================================================
    // BACKGROUND
    // ============================================================

    void CreateBackground()
    {
        Sprite sprite =
            Load(
                "RegisterSummaryScreen/register_summary_background"
            );

        GameObject bg =
            UIBuilder.CreatePanel(
                screenObj.transform,
                "Background",
                TOBATheme.ForestGreen,
                Vector2.zero,
                Vector2.one
            );

        Image bgImage =
            bg.GetComponent<Image>();

        if (bgImage != null)
            bgImage.raycastTarget = false;

        if (sprite == null)
            return;

        GameObject background =
            UIBuilder.CreateImage(
                screenObj.transform,
                "SummaryBackground",
                sprite,
                new Vector2(
                    1080f,
                    1920f
                ),
                Vector2.zero
            );

        Image image =
            background.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    // ============================================================
    // HEADER
    // ============================================================

    void CreateHeader()
    {
        Sprite sprite =
            Load(
                "RegisterSummaryScreen/register_summary_header"
            );

        if (sprite == null)
            return;

        GameObject header =
            UIBuilder.CreateImage(
                screenObj.transform,
                "SummaryHeader",
                sprite,
                new Vector2(
                    1020f,
                    280f
                ),
                new Vector2(
                    0f,
                    700f
                )
            );

        Image image =
            header.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        ScreenManager.Instance.StartCoroutine(
            FloatElement(
                header.transform,
                0.45f,
                2f
            )
        );
    }

    // ============================================================
    // MAIN BOARD
    // ============================================================

    void CreateBoard()
    {
        Sprite sprite =
            Load(
                "RegisterSummaryScreen/register_summary_main_board_v2"
            );

        if (sprite == null)
            return;

        GameObject board =
            UIBuilder.CreateImage(
                screenObj.transform,
                "SummaryBoard",
                sprite,
                new Vector2(
                    930f,
                    1130f
                ),
                new Vector2(
                    0f,
                    15f
                )
            );

        Image image =
            board.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    // ============================================================
    // AVATAR
    // ============================================================

    void CreateAvatar()
    {
        Sprite sprite =
            Load(
                "RegisterSummaryScreen/register_summary_avatar_badge"
            );

        if (sprite == null)
            return;

        GameObject avatar =
            UIBuilder.CreateImage(
                screenObj.transform,
                "AvatarBadge",
                sprite,
                new Vector2(
                    135f,
                    135f
                ),
                new Vector2(
                    0f,
                    430f
                )
            );

        Image image =
            avatar.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        ScreenManager.Instance.StartCoroutine(
            FloatElement(
                avatar.transform,
                0.7f,
                2.5f
            )
        );
    }

    // ============================================================
    // WELCOME
    // ============================================================

    void CreateWelcome()
    {
        TextMeshProUGUI title =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Selamat Datang!",
                40,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center,
                new Vector2(
                    520f,
                    55f
                ),
                new Vector2(
                    0f,
                    235f
                )
            ).GetComponent<TextMeshProUGUI>();

        title.fontStyle =
            FontStyles.Bold;

        title.characterSpacing =
            0.3f;

        title.raycastTarget =
            false;

        AddTextShadow(title);
    }

    // ============================================================
    // ACCOUNT DATA
    // ============================================================

    void CreateAccountInfo()
    {
        string name =
            authData.GetValidName();

        string username =
            authData.GetValidUsername();

        string character =
            PlayerPrefs.GetInt(
                "SelectedCharacterID",
                1
            ) == 1
                ? "Ucok"
                : "Butet";

        CreateDataRow(
            "Nama",
            name,
            130f
        );

        CreateDataRow(
            "Username",
            "@" + username,
            45f
        );

        CreateDataRow(
            "Karakter",
            character,
            -40f
        );

        CreateDataRow(
            "Level",
            "1 - Wisatawan",
            -125f
        );
    }

    void CreateDataRow(
        string label,
        string value,
        float y)
    {
        // LABEL KIRI
        TextMeshProUGUI left =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                label,
                25,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Left,
                new Vector2(
                    180f,
                    45f
                ),
                new Vector2(
                    -155f,
                    y
                )
            ).GetComponent<TextMeshProUGUI>();

        left.fontStyle =
            FontStyles.Bold;

        left.enableWordWrapping =
            false;

        left.raycastTarget =
            false;

        AddTextShadow(left);

        // VALUE KANAN
        // Digeser 20 px ke kiri dari versi sebelumnya
        TextMeshProUGUI right =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                value,
                25,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Right,
                new Vector2(
                    210f,
                    45f
                ),
                new Vector2(
                    135f,
                    y
                )
            ).GetComponent<TextMeshProUGUI>();

        right.fontStyle =
            FontStyles.Bold;

        right.enableWordWrapping =
            false;

        right.overflowMode =
            TextOverflowModes.Ellipsis;

        right.raycastTarget =
            false;

        AddTextShadow(right);

        ScreenManager.Instance.StartCoroutine(
            FloatElement(
                left.transform,
                0.22f,
                0.4f
            )
        );

        ScreenManager.Instance.StartCoroutine(
            FloatElement(
                right.transform,
                0.22f,
                0.4f
            )
        );
    }

    // ============================================================
    // USERNAME NOTE
    // ============================================================

    void CreateUsernameNote()
    {
        string username =
            authData.GetValidUsername();

        TextMeshProUGUI note =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "⚠  Catat username-mu: @" +
                username +
                "\nUsername digunakan untuk login.",
                20,
                TOBATheme.WoodDark,
                TextAlignmentOptions.Center,
                new Vector2(
                    500f,
                    60f
                ),
                new Vector2(
                    0f,
                    -210f
                )
            ).GetComponent<TextMeshProUGUI>();

        note.fontStyle =
            FontStyles.Bold;

        note.enableWordWrapping =
            true;

        note.raycastTarget =
            false;

        AddTextShadow(note);

        ScreenManager.Instance.StartCoroutine(
            FloatElement(
                note.transform,
                0.28f,
                0.6f
            )
        );
    }

    // ============================================================
    // TEXT SHADOW
    // ============================================================

    void AddTextShadow(
        TextMeshProUGUI text)
    {
        Shadow shadow =
            text.gameObject.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(
                1f,
                0.90f,
                0.55f,
                0.35f
            );

        shadow.effectDistance =
            new Vector2(
                1f,
                -1f
            );
    }

    // ============================================================
    // BUTTON
    // LOGIC ASLI TETAP
    // ============================================================

    void CreateStartButton()
    {
        GameObject button =
            UIBuilder.CreateButton(
                screenObj.transform,
                "BtnMulai",
                "MULAI PETUALANGAN!",
                new Vector2(
                    850f,
                    145f
                ),
                TOBATheme.BatakGold,
                () => { }
            );

        button.GetComponent<RectTransform>()
            .anchoredPosition =
            new Vector2(
                0f,
                -700f
            );

        TextMeshProUGUI label =
            button.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (label != null)
        {
            label.fontSize = 38;
            label.fontStyle = FontStyles.Bold;
            label.color = TOBATheme.WoodDark;
            label.characterSpacing = 1f;
            label.raycastTarget = false;
        }

        CreateRocket(
            button.transform
        );

        ScreenManager.Instance.StartCoroutine(
            ButtonIdle(
                button.transform
            )
        );

        Button btn =
            button.GetComponentInChildren<
                Button
            >();

        if (btn != null)
        {
            btn.onClick.AddListener(
                () =>
                {
                    if (isStarting)
                        return;

                    isStarting = true;

                    btn.interactable = false;

                    SoundManager.Instance.PlaySFX(
                        "sfx_win"
                    );

                    ScreenManager.Instance.StartCoroutine(
                        ButtonPressAndNavigate(
                            button.transform,
                            btn
                        )
                    );
                }
            );
        }
    }

    // ============================================================
    // ROCKET
    // ============================================================

    void CreateRocket(
        Transform parent)
    {
        Sprite rocket =
            Load(
                "RegisterSummaryScreen/register_summary_rocket"
            );

        if (rocket == null)
            return;

        GameObject rocketObject =
            UIBuilder.CreateImage(
                parent,
                "Rocket",
                rocket,
                new Vector2(
                    115f,
                    115f
                ),
                new Vector2(
                    380f,
                    5f
                )
            );

        Image image =
            rocketObject.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }

        ScreenManager.Instance.StartCoroutine(
            RocketIdle(
                rocketObject.transform
            )
        );

        CreateFlame(
            rocketObject.transform
        );
    }

    IEnumerator RocketIdle(
        Transform target)
    {
        if (target == null)
            yield break;

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                4f;

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(t) *
                    0.4f
                );

            yield return null;
        }
    }

    // ============================================================
    // FLAME
    // ============================================================

    void CreateFlame(
        Transform rocket)
    {
        Sprite flame =
            Load(
                "RegisterSummaryScreen/register_summary_flame"
            );

        if (flame == null)
            return;

        for (int i = 0; i < 5; i++)
        {
            GameObject flameObject =
                UIBuilder.CreateImage(
                    rocket,
                    "Flame_" + i,
                    flame,
                    new Vector2(
                        45f,
                        80f
                    ),
                    new Vector2(
                        -45f,
                        -48f
                    )
                );

            Image image =
                flameObject.GetComponent<Image>();

            if (image == null)
                continue;

            image.preserveAspect =
                true;

            image.raycastTarget =
                false;

            image.color =
                i < 2
                    ? new Color(
                        1f,
                        .98f,
                        .78f,
                        .95f
                    )
                    : i < 4
                        ? new Color(
                            1f,
                            .58f,
                            .10f,
                            .82f
                        )
                        : new Color(
                            .92f,
                            .18f,
                            .04f,
                            .68f
                        );

            ScreenManager.Instance.StartCoroutine(
                FlameAnimation(
                    flameObject.transform,
                    image,
                    i
                )
            );
        }
    }

    IEnumerator FlameAnimation(
        Transform target,
        Image image,
        int index)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float phase =
            Random.Range(
                0f,
                6.28f
            );

        while (target != null)
        {
            float t =
                Time.time *
                7f +
                phase;

            target.localPosition =
                origin +
                new Vector3(
                    Mathf.Sin(t) *
                    (2.5f + index),
                    Mathf.Sin(t * .8f) *
                    4f,
                    0f
                );

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(t) *
                    9f
                );

            target.localScale =
                new Vector3(
                    .7f,
                    .75f +
                    Mathf.Abs(
                        Mathf.Sin(
                            t * 1.2f
                        )
                    ) * .45f,
                    1f
                );

            Color color =
                image.color;

            color.a =
                .70f +
                Mathf.Abs(
                    Mathf.Sin(
                        t * 1.6f
                    )
                ) * .25f;

            image.color =
                color;

            yield return null;
        }
    }

    // ============================================================
    // FLOATING
    // ============================================================

    IEnumerator FloatElement(
        Transform target,
        float speed,
        float amount)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float phase =
            Random.Range(
                0f,
                6.28f
            );

        while (target != null)
        {
            float t =
                Time.time *
                speed +
                phase;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) *
                    amount,
                    origin.z
                );

            yield return null;
        }
    }

    // ============================================================
    // BUTTON IDLE
    // ============================================================

    IEnumerator ButtonIdle(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime *
                .7f;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(t) *
                    1.5f,
                    origin.z
                );

            yield return null;
        }
    }

    // ============================================================
    // BUTTON PRESS
    // ============================================================

    IEnumerator ButtonPressAndNavigate(
        Transform target,
        Button btn)
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        target.localScale =
            original * .92f;

        yield return new WaitForSeconds(
            .08f
        );

        if (target == null)
            yield break;

        target.localScale =
            original * 1.03f;

        yield return new WaitForSeconds(
            .07f
        );

        if (target == null)
            yield break;

        target.localScale =
            original;

        // Simpan/bersihkan data registrasi
        PlayerPrefs.DeleteKey(
            "RegisterName"
        );

        PlayerPrefs.DeleteKey(
            "SelectedCharacterID"
        );

        PlayerPrefs.Save();

        // Setelah animasi selesai,
        // baru pindah ke Main Menu
        if (ScreenManager.Instance == null)
            yield break;

        ScreenManager.Instance.ChangeScreen(
            new MainMenuScreen(
                container
            )
        );
    }

    // ============================================================
    // ENTRANCE
    // ============================================================

    IEnumerator Entrance()
    {
        Transform header =
            screenObj.transform.Find(
                "SummaryHeader"
            );

        Transform board =
            screenObj.transform.Find(
                "SummaryBoard"
            );

        Transform button =
            screenObj.transform.Find(
                "BtnMulai"
            );

        if (header != null)
            header.localScale =
                Vector3.one * .95f;

        if (board != null)
            board.localScale =
                Vector3.one * .96f;

        if (button != null)
            button.localScale =
                Vector3.one * .94f;

        float t = 0f;

        while (t < .4f)
        {
            t +=
                Time.deltaTime;

            float progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t / .4f
                );

            if (header != null)
            {
                header.localScale =
                    Vector3.Lerp(
                        Vector3.one * .95f,
                        Vector3.one,
                        progress
                    );
            }

            if (board != null)
            {
                board.localScale =
                    Vector3.Lerp(
                        Vector3.one * .96f,
                        Vector3.one,
                        progress
                    );
            }

            if (button != null)
            {
                button.localScale =
                    Vector3.Lerp(
                        Vector3.one * .94f,
                        Vector3.one,
                        progress
                    );
            }

            yield return null;
        }
    }
}