using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SplashScreen : BaseScreen
{
    private GameObject backgroundGO;
    private GameObject atmosphereGO;

    public SplashScreen(Transform parent)
        : base(parent, "SplashScreen")
    {
    }


    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);

        // ============================================================
        // BACKGROUND
        // ============================================================

        Sprite bgSprite =
            TOBATheme.LoadSprite("bg_toba");

        CreateAnimatedBackground(
            bgSprite
        );


        // ============================================================
        // ATMOSPHERE / FATAMORGANA
        // ============================================================

        CreateAtmosphereEffect();


        // ============================================================
        // MAIN CANVAS GROUP
        // ============================================================

        CanvasGroup cg =
            screenObj.AddComponent<CanvasGroup>();

        cg.alpha =
            0f;


        // ============================================================
        // LOGO TOBA
        // ============================================================

        Sprite logoSprite =
            Resources.Load<Sprite>(
                "CharacterSelectScreen/character_select_header"
            );

        if (logoSprite != null)
        {
            CreateLogo(
                logoSprite
            );
        }
        else
        {
            // Fallback kalau asset belum ditemukan
            CreateFallbackLogo();
        }


        // ============================================================
        // GARIS ORNAMEN
        // ============================================================

        CreateGoldDivider();


        // ============================================================
        // SUBTITLE
        // ============================================================

        TextMeshProUGUI subTmp =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Game Edukasi Budaya Batak Toba",
                37,
                TOBATheme.TextLight,
                TextAlignmentOptions.Center,
                new Vector2(
                    800f,
                    62f
                ),
                new Vector2(
                    0f,
                    55f
                )
            ).GetComponent<TextMeshProUGUI>();

        subTmp.fontStyle =
            FontStyles.Bold;

        subTmp.characterSpacing =
            0.6f;

        subTmp.raycastTarget =
            false;

        Shadow subtitleShadow =
            subTmp.gameObject.AddComponent<Shadow>();

        subtitleShadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.7f
            );

        subtitleShadow.effectDistance =
            new Vector2(
                2f,
                -2f
            );


        // ============================================================
        // LOADING BAR
        // ============================================================

        GameObject loadContainer =
            CreateLoadingBar();


        // ============================================================
        // LOADING TEXT
        // ============================================================

        TextMeshProUGUI loadTxt =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "Memuat...",
                30,
                TOBATheme.TextLight,
                TextAlignmentOptions.Center,
                new Vector2(
                    700f,
                    60f
                ),
                new Vector2(
                    0f,
                    -455f
                )
            ).GetComponent<TextMeshProUGUI>();

        loadTxt.fontStyle =
            FontStyles.Bold;

        loadTxt.characterSpacing =
            0.5f;

        loadTxt.raycastTarget =
            false;

        Shadow loadingTextShadow =
            loadTxt.gameObject.AddComponent<Shadow>();

        loadingTextShadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.75f
            );

        loadingTextShadow.effectDistance =
            new Vector2(
                2f,
                -2f
            );


        // ============================================================
        // AMBIL FILL
        // ============================================================

        GameObject loadFill =
            loadContainer.transform.Find(
                "Fill"
            )?.gameObject;


        // ============================================================
        // ANIMATION
        // ============================================================

        ScreenManager.Instance.StartCoroutine(
            AnimateSplash(
                cg,
                loadFill,
                loadTxt
            )
        );
    }


    // ============================================================
    // ANIMATED BACKGROUND
    // ============================================================

    void CreateAnimatedBackground(
        Sprite bgSprite)
    {
        if (bgSprite == null)
        {
            Debug.LogWarning(
                "[Splash] bg_toba tidak ditemukan."
            );
            return;
        }


        backgroundGO =
            new GameObject(
                "AnimatedBackground"
            );

        backgroundGO.transform.SetParent(
            screenObj.transform,
            false
        );


        RectTransform rt =
            backgroundGO.AddComponent<RectTransform>();

        rt.anchorMin =
            new Vector2(
                0f,
                0f
            );

        rt.anchorMax =
            new Vector2(
                1f,
                1f
            );

        rt.offsetMin =
            rt.offsetMax =
            Vector2.zero;


        Image img =
            backgroundGO.AddComponent<Image>();

        img.sprite =
            bgSprite;

        img.preserveAspect =
            true;

        img.raycastTarget =
            false;


        // Background dibuat sedikit lebih besar
        // supaya gerakan tidak memperlihatkan tepi kosong.
        backgroundGO.transform.localScale =
            new Vector3(
                1.08f,
                1.08f,
                1f
            );


        ScreenManager.Instance.StartCoroutine(
            AnimateBackground(
                backgroundGO.transform
            )
        );
    }


    // ============================================================
    // BACKGROUND MOTION
    // ============================================================

    IEnumerator AnimateBackground(
        Transform target)
    {
        if (target == null)
            yield break;


        Vector3 basePosition =
            target.localPosition;

        Vector3 baseScale =
            target.localScale;

        float time =
            0f;


        while (
            target != null)
        {
            time +=
                Time.deltaTime *
                0.16f;


            float driftX =
                Mathf.Sin(
                    time
                ) *
                9f;

            float driftY =
                Mathf.Cos(
                    time * 0.72f
                ) *
                7f;

            float zoom =
                1f +
                Mathf.Sin(
                    time * 0.48f
                ) *
                0.018f;


            target.localPosition =
                new Vector3(
                    basePosition.x +
                    driftX,
                    basePosition.y +
                    driftY,
                    basePosition.z
                );


            target.localScale =
                baseScale *
                zoom;


            yield return null;
        }
    }


    // ============================================================
    // ATMOSPHERE / FATAMORGANA
    // ============================================================

    void CreateAtmosphereEffect()
    {
        Sprite atmosphereSprite =
            GetAtmosphereSprite();

        if (atmosphereSprite == null)
            return;


        atmosphereGO =
            new GameObject(
                "Atmosphere"
            );

        atmosphereGO.transform.SetParent(
            screenObj.transform,
            false
        );


        RectTransform rt =
            atmosphereGO.AddComponent<RectTransform>();

        rt.anchorMin =
            Vector2.zero;

        rt.anchorMax =
            Vector2.one;

        rt.offsetMin =
            rt.offsetMax =
            Vector2.zero;


        Image img =
            atmosphereGO.AddComponent<Image>();

        img.sprite =
            atmosphereSprite;

        img.color =
            new Color(
                1f,
                1f,
                1f,
                0.065f
            );

        img.raycastTarget =
            false;

        img.type =
            Image.Type.Tiled;


        ScreenManager.Instance.StartCoroutine(
            AnimateAtmosphere(
                atmosphereGO.transform
            )
        );
    }


    // ============================================================
    // ATMOSPHERE MOTION
    // ============================================================

    IEnumerator AnimateAtmosphere(
        Transform target)
    {
        if (target == null)
            yield break;


        Vector3 origin =
            target.localPosition;

        float time =
            0f;


        while (
            target != null)
        {
            time +=
                Time.deltaTime *
                0.22f;


            float x =
                Mathf.Sin(
                    time
                ) *
                18f;


            float y =
                Mathf.Cos(
                    time * 0.7f
                ) *
                10f;


            float rotation =
                Mathf.Sin(
                    time * 0.45f
                ) *
                0.35f;


            target.localPosition =
                new Vector3(
                    origin.x + x,
                    origin.y + y,
                    origin.z
                );


            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    rotation
                );


            yield return null;
        }
    }


    // ============================================================
    // GENERATED ATMOSPHERE TEXTURE
    // ============================================================

    Sprite GetAtmosphereSprite()
    {
        const int width =
            256;

        const int height =
            256;


        Texture2D texture =
            new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );


        texture.wrapMode =
            TextureWrapMode.Repeat;


        Color[] pixels =
            new Color[
                width *
                height
            ];


        for (
            int y = 0;
            y < height;
            y++)
        {
            float vertical =
                y /
                (float)(
                    height - 1
                );


            for (
                int x = 0;
                x < width;
                x++)
            {
                float horizontal =
                    x /
                    (float)(
                        width - 1
                    );


                float wave =
                    Mathf.Sin(
                        horizontal *
                        Mathf.PI *
                        4f +
                        vertical *
                        Mathf.PI *
                        2f
                    );


                float alpha =
                    (wave * 0.5f +
                     0.5f) *
                    0.22f;


                // Lebih lembut di tengah,
                // lebih terlihat di area tertentu.
                float fade =
                    Mathf.Sin(
                        vertical *
                        Mathf.PI
                    );


                alpha *=
                    fade;


                pixels[
                    y * width +
                    x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }


        texture.SetPixels(
            pixels
        );

        texture.Apply();


        return Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                width,
                height
            ),
            new Vector2(
                0.5f,
                0.5f
            ),
            100f
        );
    }


    // ============================================================
    // LOGO TOBA
    // ============================================================

    void CreateLogo(
        Sprite logoSprite)
    {
        GameObject logoRoot =
            new GameObject(
                "TOBALogo"
            );

        logoRoot.transform.SetParent(
            screenObj.transform,
            false
        );


        RectTransform rtRoot =
            logoRoot.AddComponent<RectTransform>();

        rtRoot.anchorMin =
            rtRoot.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtRoot.sizeDelta =
            new Vector2(
                920f,
                410f
            );

        rtRoot.anchoredPosition =
            new Vector2(
                0f,
                320f
            );


        // ========================================================
        // MASK ROOT
        // ========================================================

        Image logoImage =
            logoRoot.AddComponent<Image>();

        logoImage.sprite =
            logoSprite;

        logoImage.preserveAspect =
            true;

        logoImage.color =
            Color.white;

        logoImage.raycastTarget =
            false;


        Mask mask =
            logoRoot.AddComponent<Mask>();

        mask.showMaskGraphic =
            true;


        // ========================================================
        // SHIMMER
        // ========================================================

        Sprite sweepSprite =
            GetSweepSprite();


        if (sweepSprite != null)
        {
            GameObject sweep =
                new GameObject(
                    "LightSweep"
                );

            sweep.transform.SetParent(
                logoRoot.transform,
                false
            );


            RectTransform rtSweep =
                sweep.AddComponent<RectTransform>();

            rtSweep.anchorMin =
                rtSweep.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rtSweep.sizeDelta =
                new Vector2(
                    150f,
                    600f
                );

            rtSweep.anchoredPosition =
                new Vector2(
                    -700f,
                    0f
                );


            Image sweepImg =
                sweep.AddComponent<Image>();

            sweepImg.sprite =
                sweepSprite;

            sweepImg.color =
                new Color(
                    1f,
                    0.95f,
                    0.55f,
                    0.25f
                );

            sweepImg.raycastTarget =
                false;


            rtSweep.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    -18f
                );


            ScreenManager.Instance.StartCoroutine(
                AnimateLogoShimmer(
                    rtSweep
                )
            );
        }
    }


    // ============================================================
    // LOGO FALLBACK
    // ============================================================

    void CreateFallbackLogo()
    {
        GameObject logoGO =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "TOBA",
                210,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(
                    900f,
                    290f
                ),
                new Vector2(
                    0f,
                    290f
                )
            );


        TextMeshProUGUI logoTmp =
            logoGO.GetComponent<TextMeshProUGUI>();

        logoTmp.fontStyle =
            FontStyles.Bold;

        logoTmp.characterSpacing =
            2f;

        logoTmp.raycastTarget =
            false;


        Shadow logoSh =
            logoGO.AddComponent<Shadow>();

        logoSh.effectColor =
            new Color(
                0.3f,
                0.15f,
                0f,
                0.85f
            );

        logoSh.effectDistance =
            new Vector2(
                6f,
                -6f
            );
    }


    // ============================================================
    // LOGO SHIMMER ANIMATION
    // ============================================================

    IEnumerator AnimateLogoShimmer(
        RectTransform sweep)
    {
        if (sweep == null)
            yield break;


        while (
            sweep != null)
        {
            // Jeda antar sweep
            yield return
                new WaitForSecondsRealtime(
                    1.8f
                );


            float elapsed =
                0f;

            float duration =
                1.2f;


            float startX =
                -700f;

            float endX =
                700f;


            while (
                elapsed <
                duration &&
                sweep != null)
            {
                elapsed +=
                    Time.deltaTime;


                float t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        elapsed /
                        duration
                    );


                sweep.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(
                            startX,
                            endX,
                            t
                        ),
                        0f
                    );


                yield return null;
            }


            if (sweep != null)
            {
                sweep.anchoredPosition =
                    new Vector2(
                        startX,
                        0f
                    );
            }
        }
    }


    // ============================================================
    // GENERATED SWEEP
    // ============================================================

    Sprite GetSweepSprite()
    {
        const int width =
            64;

        const int height =
            256;


        Texture2D texture =
            new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );


        Color[] pixels =
            new Color[
                width *
                height
            ];


        for (
            int y = 0;
            y < height;
            y++)
        {
            for (
                int x = 0;
                x < width;
                x++)
            {
                float nx =
                    x /
                    (float)(
                        width - 1
                    );


                float distance =
                    Mathf.Abs(
                        nx -
                        0.5f
                    ) *
                    2f;


                float alpha =
                    Mathf.Clamp01(
                        1f -
                        distance
                    );


                alpha *=
                    0.8f;


                pixels[
                    y * width +
                    x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }


        texture.SetPixels(
            pixels
        );

        texture.Apply();


        return Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                width,
                height
            ),
            new Vector2(
                0.5f,
                0.5f
            ),
            100f
        );
    }


    // ============================================================
    // GOLD DIVIDER
    // ============================================================

    void CreateGoldDivider()
    {
        GameObject line =
            UIBuilder.CreatePanelSized(
                screenObj.transform,
                "Line",
                TOBATheme.BatakGold,
                new Vector2(
                    520f,
                    5f
                ),
                new Vector2(
                    0f,
                    120f
                )
            );


        line.GetComponent<Image>()
            .raycastTarget = false;


        // Ornament kecil tengah
        GameObject center =
            UIBuilder.CreateTextSized(
                screenObj.transform,
                "◆",
                28,
                TOBATheme.BatakGold,
                TextAlignmentOptions.Center,
                new Vector2(
                    50f,
                    50f
                ),
                new Vector2(
                    0f,
                    120f
                )
            );


        center.GetComponent<TextMeshProUGUI>()
            .fontStyle =
            FontStyles.Bold;


        center.GetComponent<TextMeshProUGUI>()
            .raycastTarget =
            false;
    }


    // ============================================================
    // LOADING BAR
    // ============================================================

    GameObject CreateLoadingBar()
    {
        Sprite frameSprite =
            Resources.Load<Sprite>(
                "SplashScreen/splash_loading_bar"
            );


        GameObject root =
            new GameObject(
                "LoadContainer"
            );

        root.transform.SetParent(
            screenObj.transform,
            false
        );


        RectTransform rtRoot =
            root.AddComponent<RectTransform>();

        rtRoot.anchorMin =
            rtRoot.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rtRoot.sizeDelta =
            new Vector2(
                720f,
                110f
            );

        rtRoot.anchoredPosition =
            new Vector2(
                0f,
                -370f
            );


        // ============================================================
        // FILL BACKGROUND
        // ============================================================

        GameObject fillBg =
            UIBuilder.CreatePanel(
                root.transform,
                "FillBackground",
                new Color(
                    0.08f,
                    0.04f,
                    0.015f,
                    0.95f
                ),
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    0.88f,
                    0.40f
                )
            );


        RectTransform rtFillBg =
            fillBg.GetComponent<RectTransform>();

        rtFillBg.anchorMin =
            new Vector2(
                0.12f,
                0.30f
            );

        rtFillBg.anchorMax =
            new Vector2(
                0.88f,
                0.70f
            );

        rtFillBg.offsetMin =
            rtFillBg.offsetMax =
            Vector2.zero;


        fillBg.GetComponent<Image>()
            .raycastTarget = false;


        // ============================================================
        // PROGRESS FILL
        // ============================================================

        GameObject fill =
            new GameObject(
                "Fill"
            );

        fill.transform.SetParent(
            root.transform,
            false
        );


        RectTransform rtFill =
            fill.AddComponent<RectTransform>();

        rtFill.anchorMin =
            new Vector2(
                0.12f,
                0.30f
            );

        rtFill.anchorMax =
            new Vector2(
                0.12f,
                0.70f
            );

        rtFill.offsetMin =
            rtFill.offsetMax =
            Vector2.zero;


        Image fillImg =
            fill.AddComponent<Image>();

        fillImg.sprite =
            UIBuilder.GetDefaultSprite();

        fillImg.color =
            TOBATheme.BatakGold;

        fillImg.raycastTarget =
            false;


        // ============================================================
        // GOLD HIGHLIGHT DI PROGRESS
        // ============================================================

        GameObject fillGlow =
            new GameObject(
                "FillGlow"
            );

        fillGlow.transform.SetParent(
            fill.transform,
            false
        );


        RectTransform rtGlow =
            fillGlow.AddComponent<RectTransform>();

        rtGlow.anchorMin =
            Vector2.zero;

        rtGlow.anchorMax =
            Vector2.one;

        rtGlow.offsetMin =
            rtGlow.offsetMax =
            Vector2.zero;


        Image glowImg =
            fillGlow.AddComponent<Image>();

        glowImg.sprite =
            UIBuilder.GetDefaultSprite();

        glowImg.color =
            new Color(
                1f,
                1f,
                0.75f,
                0.16f
            );

        glowImg.raycastTarget =
            false;


        // ============================================================
        // FRAME
        // ============================================================

        if (frameSprite != null)
        {
            GameObject frame =
                UIBuilder.CreateImage(
                    root.transform,
                    "Frame",
                    frameSprite,
                    new Vector2(
                        720f,
                        110f
                    ),
                    Vector2.zero
                );


            Image frameImg =
                frame.GetComponent<Image>();

            frameImg.preserveAspect =
                true;

            frameImg.raycastTarget =
                false;
        }
        else
        {
            Image fallback =
                root.AddComponent<Image>();

            fallback.sprite =
                TOBATheme.LoadSprite(
                    "panel_wood"
                );

            fallback.type =
                Image.Type.Sliced;

            fallback.color =
                new Color(
                    0.2f,
                    0.1f,
                    0.03f,
                    0.9f
                );

            fallback.raycastTarget =
                false;
        }


        return root;
    }


    // ============================================================
    // SPLASH ANIMATION
    // ============================================================

    IEnumerator AnimateSplash(
        CanvasGroup cg,
        GameObject fill,
        TextMeshProUGUI loadTxt)
    {
        // ============================================================
        // FADE IN
        // ============================================================

        yield return
            UIBuilder.AnimateFadeIn(
                cg,
                0.75f
            );


        // ============================================================
        // EXISTING LOADING FLOW
        // ============================================================

        string[] msgs =
        {
            "Memuat data budaya...",
            "Menyiapkan peta Toba...",
            "Menghubungkan ke server...",
            "Siap berpetualang! 🎉"
        };


        float[] milestones =
        {
            0.25f,
            0.5f,
            0.82f,
            1f
        };


        RectTransform rtFill =
            fill != null
                ? fill.GetComponent<RectTransform>()
                : null;


        for (
            int i = 0;
            i < milestones.Length;
            i++)
        {
            if (loadTxt != null)
            {
                loadTxt.text =
                    msgs[i];
            }


            float from =
                i == 0
                    ? 0f
                    : milestones[i - 1];


            float to =
                milestones[i];


            float t =
                0f;


            while (
                t < 0.45f)
            {
                t +=
                    Time.deltaTime;


                float r =
                    Mathf.Lerp(
                        from,
                        to,
                        t /
                        0.45f
                    );


                if (
                    rtFill != null)
                {
                    rtFill.anchorMin =
                        new Vector2(
                            0.12f,
                            0.30f
                        );


                    rtFill.anchorMax =
                        new Vector2(
                            Mathf.Lerp(
                                0.12f,
                                0.88f,
                                r
                            ),
                            0.70f
                        );


                    rtFill.offsetMin =
                        rtFill.offsetMax =
                        Vector2.zero;
                }


                yield return null;
            }


            yield return
                new WaitForSecondsRealtime(
                    0.22f
                );
        }


        // ============================================================
        // FINAL PAUSE
        // ============================================================

        yield return
            new WaitForSecondsRealtime(
                0.3f
            );


        // ============================================================
        // FADE OUT
        // ============================================================

        float elapsed =
            0f;


        CanvasGroup cg2 =
            screenObj != null
                ? screenObj.GetComponent<CanvasGroup>()
                : null;


        while (
            elapsed < 0.4f &&
            cg2 != null)
        {
            elapsed +=
                Time.deltaTime;


            cg2.alpha =
                1f -
                (
                    elapsed /
                    0.4f
                );


            yield return null;
        }


        // ============================================================
        // NEXT SCREEN
        // ============================================================

        if (
            ScreenManager.Instance != null &&
            screenObj != null)
        {
            ScreenManager.Instance.ChangeScreen(
                new WelcomeScreen(
                    container
                )
            );
        }
    }
}