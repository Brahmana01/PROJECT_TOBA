using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WelcomeScreen : BaseScreen
{
    private static Texture2D waterTexture;
    private static Sprite generatedSweepSprite;
    private static readonly List<Sprite> leafSprites = new List<Sprite>();

    private const int WIND_LEAF_COUNT = 18;
    private readonly List<Transform> windLeaves = new List<Transform>();

    private float windGust = 0f;
    private float targetGust = 0f;
    private float gustTimer = 3f;

    public WelcomeScreen(Transform parent) : base(parent, "Welcome") { }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);

        Sprite bgSprite = TOBATheme.LoadSprite("bg_toba");
        CreateAnimatedBackground(bgSprite);
        CreateWaterMotion();

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0f, 0.035f, 0f, 0.24f),
            Vector2.zero,
            Vector2.one
        );

        CanvasGroup cg = screenObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        Sprite logoSprite = Resources.Load<Sprite>(
            "CharacterSelectScreen/character_select_header"
        );

        if (logoSprite != null) CreateLogo(logoSprite);
        else CreateFallbackLogo();

        CreateGoldDivider();

        TextMeshProUGUI subtitle = UIBuilder.CreateTextSized(
            screenObj.transform,
            "Game Edukasi Budaya Batak Toba",
            36,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(820f, 62f),
            new Vector2(0f, 300f)
        ).GetComponent<TextMeshProUGUI>();
        subtitle.fontStyle = FontStyles.Bold;
        subtitle.characterSpacing = 0.8f;
        subtitle.raycastTarget = false;
        AddTextShadow(subtitle, 0.7f);

        TextMeshProUGUI slogan = UIBuilder.CreateTextSized(
            screenObj.transform,
            "Ikuti Petualangan Interaktif!",
            34,
            new Color(1f, 1f, 1f, 0.90f),
            TextAlignmentOptions.Center,
            new Vector2(760f, 60f),
            new Vector2(0f, -355f)
        ).GetComponent<TextMeshProUGUI>();
        slogan.fontStyle = FontStyles.Bold;
        slogan.characterSpacing = 0.7f;
        slogan.raycastTarget = false;
        AddTextShadow(slogan, 0.65f);

        CreateWindLeaves();
        ScreenManager.Instance.StartCoroutine(WindGustController());

        GameObject btnExplore = UIBuilder.CreateButton(
            screenObj.transform,
            "BtnJelajah",
            "MULAI PETUALANGAN",
            new Vector2(950f, 180f),
            TOBATheme.BtnPrimary,
            null
        );
        btnExplore.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0f, -505f);
        StyleWelcomeButton(btnExplore);

        Button exploreButton = btnExplore.GetComponent<Button>();
        if (exploreButton != null)
        {
            exploreButton.onClick.AddListener(() =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressWelcomeButton(btnExplore.transform, () =>
                    {
                        ScreenManager.Instance.ChangeScreen(
                            new CharacterSelectScreen(container)
                        );
                    })
                );
            });
        }

        GameObject btnLogin = UIBuilder.CreateButton(
            screenObj.transform,
            "BtnLogin",
            "SUDAH PUNYA AKUN",
            new Vector2(950f, 160f),
            TOBATheme.BtnSecondary,
            null
        );
        btnLogin.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(0f, -690f);
        StyleWelcomeButton(btnLogin);

        Button loginButton = btnLogin.GetComponent<Button>();
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(() =>
            {
                ScreenManager.Instance.StartCoroutine(
                    PressWelcomeButton(btnLogin.transform, () =>
                    {
                        ScreenManager.Instance.ChangeScreen(
                            new LoginScreen(container)
                        );
                    })
                );
            });
        }

        ScreenManager.Instance.StartCoroutine(
            FloatUI(btnExplore.transform, 0f, 2.5f)
        );
        ScreenManager.Instance.StartCoroutine(
            FloatUI(btnLogin.transform, 0.8f, 2.8f)
        );
        ScreenManager.Instance.StartCoroutine(
            UIBuilder.AnimateFadeIn(cg, 0.7f)
        );
    }

    private void AddTextShadow(TextMeshProUGUI text, float alpha)
    {
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, alpha);
        shadow.effectDistance = new Vector2(2f, -2f);
    }

    // ============================================================
    // BACKGROUND
    // ============================================================

    private void CreateAnimatedBackground(Sprite bgSprite)
    {
        if (bgSprite == null)
        {
            Debug.LogWarning("[Welcome] bg_toba tidak ditemukan.");
            return;
        }

        GameObject bg = new GameObject("WelcomeBackground");
        bg.transform.SetParent(screenObj.transform, false);

        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = bg.AddComponent<Image>();
        img.sprite = bgSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        bg.transform.localScale = new Vector3(1.045f, 1.045f, 1f);

        ScreenManager.Instance.StartCoroutine(FloatBackground(bg.transform));
    }

    private IEnumerator FloatBackground(Transform target)
    {
        if (target == null) yield break;

        Vector3 basePos = target.localPosition;
        Vector3 baseScale = target.localScale;
        float t = 0f;

        while (target != null)
        {
            t += Time.deltaTime * 0.12f;
            target.localPosition = new Vector3(
                basePos.x + Mathf.Sin(t) * 6f,
                basePos.y + Mathf.Cos(t * 0.75f) * 5f,
                basePos.z
            );
            target.localScale = baseScale * (
                1f + Mathf.Sin(t * 0.45f) * 0.010f
            );
            yield return null;
        }
    }

    // ============================================================
    // WATER: UV SCROLL + SHIMMER
    // ============================================================

    private void CreateWaterMotion()
    {
        Texture2D texture = GetWaterTexture();
        if (texture == null) return;

        GameObject baseWater = new GameObject("LakeWaterUV");
        baseWater.transform.SetParent(screenObj.transform, false);

        RectTransform baseRT = baseWater.AddComponent<RectTransform>();
        baseRT.anchorMin = new Vector2(0f, 0.30f);
        baseRT.anchorMax = new Vector2(1f, 0.68f);
        baseRT.offsetMin = baseRT.offsetMax = Vector2.zero;

        RawImage baseImage = baseWater.AddComponent<RawImage>();
        baseImage.texture = texture;
        baseImage.color = new Color(1f, 1f, 1f, 0.032f);
        baseImage.raycastTarget = false;
        baseImage.uvRect = new Rect(0f, 0f, 2.7f, 1f);

        GameObject shimmer = new GameObject("LakeWaterShimmer");
        shimmer.transform.SetParent(screenObj.transform, false);

        RectTransform shimmerRT = shimmer.AddComponent<RectTransform>();
        shimmerRT.anchorMin = new Vector2(0f, 0.31f);
        shimmerRT.anchorMax = new Vector2(1f, 0.67f);
        shimmerRT.offsetMin = shimmerRT.offsetMax = Vector2.zero;

        RawImage shimmerImage = shimmer.AddComponent<RawImage>();
        shimmerImage.texture = texture;
        shimmerImage.color = new Color(1f, 1f, 1f, 0.014f);
        shimmerImage.raycastTarget = false;
        shimmerImage.uvRect = new Rect(0f, 0f, 3.2f, 1f);

        ScreenManager.Instance.StartCoroutine(AnimateWaterUV(baseImage));
        ScreenManager.Instance.StartCoroutine(AnimateWaterShimmer(shimmerImage));
    }

    private IEnumerator AnimateWaterUV(RawImage image)
    {
        float offsetX = 0f;
        float offsetY = 0f;

        while (image != null)
        {
            offsetX = Mathf.Repeat(offsetX + Time.deltaTime * 0.018f, 1f);
            offsetY = Mathf.Repeat(offsetY + Time.deltaTime * 0.0025f, 1f);

            Rect uv = image.uvRect;
            uv.x = offsetX;
            uv.y = offsetY;
            image.uvRect = uv;

            yield return null;
        }
    }

    private IEnumerator AnimateWaterShimmer(RawImage image)
    {
        float offsetX = 0f;
        float time = 0f;

        while (image != null)
        {
            time += Time.deltaTime;
            offsetX = Mathf.Repeat(offsetX + Time.deltaTime * 0.0085f, 1f);

            Rect uv = image.uvRect;
            uv.x = offsetX;
            image.uvRect = uv;

            float pulse = Mathf.Sin(time * 0.42f) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0.008f, 0.022f, pulse);
            image.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }
    }

    private Texture2D GetWaterTexture()
    {
        if (waterTexture != null) return waterTexture;

        const int width = 256;
        const int height = 128;

        waterTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );
        waterTexture.wrapMode = TextureWrapMode.Repeat;
        waterTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float fy = y / (float)(height - 1);

            for (int x = 0; x < width; x++)
            {
                float fx = x / (float)(width - 1);

                float broad = Mathf.Sin(fx * 15f + fy * 2.0f);
                float medium = Mathf.Sin(fx * 29f - fy * 3.5f);
                float fine = Mathf.Sin(fx * 57f + fy * 6.5f);
                float sparkle = Mathf.Sin(fx * 91f - fy * 9f);

                float combined =
                    broad * 0.50f +
                    medium * 0.28f +
                    fine * 0.15f +
                    sparkle * 0.07f;

                combined = combined * 0.5f + 0.5f;
                float highlight = Mathf.SmoothStep(0.56f, 0.86f, combined);
                float centerFade = 1f - Mathf.Abs(fy - 0.5f) * 0.75f;
                float alpha = highlight * centerFade * 0.38f;

                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        waterTexture.SetPixels(pixels);
        waterTexture.Apply();
        return waterTexture;
    }

    // ============================================================
    // WIND LEAVES
    // ============================================================

    private void CreateWindLeaves()
    {
        CreateLeafVariants();
        if (leafSprites.Count == 0) return;

        for (int i = 0; i < WIND_LEAF_COUNT; i++)
        {
            Sprite sprite = leafSprites[Random.Range(0, leafSprites.Count)];
            float group = Random.value;
            float scale = group < 0.33f
                ? Random.Range(0.50f, 0.78f)
                : group < 0.88f
                    ? Random.Range(0.82f, 1.18f)
                    : Random.Range(1.30f, 1.75f);

            GameObject leaf = UIBuilder.CreateImage(
                screenObj.transform,
                "WindLeaf_" + i,
                sprite,
                new Vector2(58f, 38f),
                new Vector2(
                    Random.Range(-660f, 520f),
                    Random.Range(-760f, 700f)
                )
            );

            Image image = leaf.GetComponent<Image>();
            if (image != null)
            {
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = RandomLeafTint();
            }

            leaf.transform.localScale = new Vector3(scale, scale, 1f);
            leaf.transform.localEulerAngles = new Vector3(
                0f, 0f, Random.Range(0f, 360f)
            );

            windLeaves.Add(leaf.transform);

            ScreenManager.Instance.StartCoroutine(
                AnimateWindLeaf(
                    leaf.transform,
                    scale,
                    Random.Range(35f, 62f),
                    Random.Range(18f, 45f),
                    Random.Range(0.40f, 0.90f),
                    Random.Range(45f, 120f),
                    Random.Range(70f, 180f),
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f)
                )
            );
        }
    }

    private Color RandomLeafTint()
    {
        switch (Random.Range(0, 5))
        {
            case 0:
                return new Color(0.84f, 0.94f, 0.45f, Random.Range(0.55f, 0.88f));
            case 1:
                return new Color(0.40f, 0.66f, 0.18f, Random.Range(0.60f, 0.90f));
            case 2:
                return new Color(0.72f, 0.82f, 0.25f, Random.Range(0.55f, 0.84f));
            case 3:
                return new Color(0.78f, 0.62f, 0.22f, Random.Range(0.48f, 0.78f));
            default:
                return new Color(0.52f, 0.34f, 0.15f, Random.Range(0.45f, 0.70f));
        }
    }

    private void CreateLeafVariants()
    {
        if (leafSprites.Count > 0) return;

        for (int i = 0; i < 5; i++)
            leafSprites.Add(CreateLeafSprite(i));
    }

    private Sprite CreateLeafSprite(int variant)
    {
        const int width = 96;
        const int height = 62;

        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        Color darkGreen = new Color(0.18f, 0.38f, 0.08f, 1f);
        Color lightGreen = new Color(0.67f, 0.83f, 0.23f, 1f);
        Color yellowGreen = new Color(0.76f, 0.76f, 0.20f, 1f);
        Color brown = new Color(0.48f, 0.30f, 0.12f, 1f);
        Color darkBrown = new Color(0.26f, 0.15f, 0.07f, 1f);

        float asymmetry = variant % 2 == 0 ? 0.12f : -0.10f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x - width * 0.50f) / (width * 0.50f);
                float ny = (y - height * 0.50f) / (height * 0.50f);
                float curvedX = nx - Mathf.Sin(ny * 2.7f) * 0.08f + asymmetry * ny;
                float body =
                    (curvedX * curvedX) / 0.82f +
                    (ny * ny) / (0.78f + (variant % 2) * 0.10f);
                float tipFactor = Mathf.Clamp01((nx + 1f) * 0.5f);
                float threshold = 0.82f - tipFactor * 0.10f;

                if (body < threshold)
                {
                    float gradient = Mathf.Clamp01((ny + 0.95f) / 1.9f);
                    Color color;

                    if (variant == 3)
                        color = Color.Lerp(brown, yellowGreen, gradient);
                    else if (variant == 4)
                        color = Color.Lerp(darkBrown, brown, gradient);
                    else if (variant == 2)
                        color = Color.Lerp(yellowGreen, lightGreen, gradient);
                    else
                        color = Color.Lerp(darkGreen, lightGreen, gradient);

                    pixels[y * width + x] = color;
                }
            }
        }

        for (int x = 10; x < width - 8; x++)
        {
            float nx = x / (float)(width - 1);
            int y = Mathf.RoundToInt(
                height * 0.5f + Mathf.Sin(nx * 5.2f) * 2.4f
            );

            if (y >= 0 && y < height)
            {
                pixels[y * width + x] = new Color(0.85f, 0.79f, 0.36f, 0.85f);
                if (y + 1 < height)
                    pixels[(y + 1) * width + x] = new Color(0.62f, 0.55f, 0.20f, 0.50f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private IEnumerator AnimateWindLeaf(
        Transform leaf,
        float baseScale,
        float baseSpeed,
        float turbulence,
        float turbulenceSpeed,
        float rotationSpeed,
        float tumbleSpeed,
        float phaseX,
        float phaseY,
        float phaseR
    )
    {
        if (leaf == null) yield break;

        Vector3 position = leaf.localPosition;
        float time = Random.Range(0f, 3f);
        float waveFrequency = Random.Range(0.55f, 1.05f);

        while (leaf != null)
        {
            time += Time.deltaTime;

            float noiseX = Mathf.PerlinNoise(phaseX, time * turbulenceSpeed) - 0.5f;
            float noiseY = Mathf.PerlinNoise(phaseY, time * turbulenceSpeed) - 0.5f;

            float horizontalSpeed =
                (baseSpeed + windGust * Random.Range(20f, 55f)) *
                (1f + windGust * Random.Range(0.15f, 0.45f));

            position.x += horizontalSpeed * Time.deltaTime;

            float wave = Mathf.Sin(time * waveFrequency + phaseY) * 16f;
            position.y += (wave + noiseY * turbulence) * Time.deltaTime;
            position.x += noiseX * turbulence * 0.25f * Time.deltaTime;

            leaf.localPosition = position;

            float roll = Mathf.Sin(time * 2.0f + phaseR) * 26f;
            float tilt = Mathf.Sin(time * 1.25f + phaseR) * 16f;

            leaf.localEulerAngles += new Vector3(
                roll * Time.deltaTime * 2f,
                tilt * Time.deltaTime,
                tumbleSpeed * Time.deltaTime
            );

            float pulse = 1f + Mathf.Sin(time * 1.1f + phaseX) * 0.035f;
            leaf.localScale = new Vector3(
                baseScale * pulse,
                baseScale * pulse,
                1f
            );

            if (windGust > 0.2f)
            {
                leaf.localEulerAngles += new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(-2.5f, 2.5f)
                );
            }

            if (position.x > 650f)
            {
                position.x = Random.Range(-700f, -610f);
                position.y = Random.Range(-760f, 700f);
                leaf.localPosition = position;
                leaf.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
                time = Random.Range(0f, 2.5f);
            }

            yield return null;
        }
    }

    private IEnumerator WindGustController()
    {
        while (screenObj != null)
        {
            gustTimer -= Time.deltaTime;

            if (gustTimer <= 0f)
            {
                targetGust = Random.Range(0.0f, 1.25f);
                gustTimer = targetGust > 0.25f
                    ? Random.Range(1.0f, 2.0f)
                    : Random.Range(2.5f, 5.0f);

                if (targetGust > 0.25f)
                {
                    // Additional impulse: several existing leaves get an immediate push.
                    for (int i = 0; i < windLeaves.Count; i++)
                    {
                        if (windLeaves[i] != null && Random.value < 0.55f)
                        {
                            RectTransform rt = windLeaves[i].GetComponent<RectTransform>();
                            if (rt != null)
                                rt.anchoredPosition += new Vector2(Random.Range(8f, 24f), Random.Range(-5f, 8f));
                        }
                    }
                }
            }

            if (targetGust <= 0.01f)
                targetGust = 0f;

            windGust = Mathf.Lerp(
                windGust,
                targetGust,
                Time.deltaTime * 1.6f
            );

            // Gust decays naturally when the timer reaches zero.
            if (gustTimer < 0.6f && targetGust > 0f)
                targetGust = 0f;

            yield return null;
        }
    }

    // ============================================================
    // LOGO
    // ============================================================

    private void CreateLogo(Sprite logoSprite)
    {
        GameObject logo = new GameObject("WelcomeLogo");
        logo.transform.SetParent(screenObj.transform, false);

        RectTransform rt = logo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(930f, 360f);
        rt.anchoredPosition = new Vector2(0f, 475f);

        Image image = logo.AddComponent<Image>();
        image.sprite = logoSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        Mask mask = logo.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        Sprite sweepSprite = GetSweepSprite();
        if (sweepSprite != null)
        {
            GameObject sweep = new GameObject("LogoShimmer");
            sweep.transform.SetParent(logo.transform, false);

            RectTransform sweepRT = sweep.AddComponent<RectTransform>();
            sweepRT.anchorMin = sweepRT.anchorMax = new Vector2(0.5f, 0.5f);
            sweepRT.sizeDelta = new Vector2(155f, 500f);
            sweepRT.anchoredPosition = new Vector2(-700f, 0f);
            sweepRT.localEulerAngles = new Vector3(0f, 0f, -18f);

            Image sweepImage = sweep.AddComponent<Image>();
            sweepImage.sprite = sweepSprite;
            sweepImage.color = new Color(1f, 0.96f, 0.60f, 0.24f);
            sweepImage.raycastTarget = false;

            ScreenManager.Instance.StartCoroutine(
                AnimateLogoShimmer(sweepRT)
            );
        }
    }

    private void CreateFallbackLogo()
    {
        GameObject logoTxt = UIBuilder.CreateTextSized(
            screenObj.transform,
            "TOBA",
            195,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(850f, 260f),
            new Vector2(0f, 520f)
        );

        TextMeshProUGUI text = logoTxt.GetComponent<TextMeshProUGUI>();
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 2f;
        text.raycastTarget = false;

        Shadow shadow = logoTxt.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.3f, 0.15f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(5f, -5f);
    }

    private IEnumerator AnimateLogoShimmer(RectTransform sweep)
    {
        while (sweep != null)
        {
            yield return new WaitForSecondsRealtime(1.8f);

            float elapsed = 0f;
            const float duration = 1.15f;

            while (elapsed < duration && sweep != null)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                sweep.anchoredPosition = new Vector2(
                    Mathf.Lerp(-700f, 700f, p),
                    0f
                );
                yield return null;
            }

            if (sweep != null)
                sweep.anchoredPosition = new Vector2(-700f, 0f);
        }
    }

    private Sprite GetSweepSprite()
    {
        if (generatedSweepSprite != null)
            return generatedSweepSprite;

        const int width = 64;
        const int height = 256;

        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x / (float)(width - 1);
                float distance = Mathf.Abs(nx - 0.5f) * 2f;
                float alpha = Mathf.Clamp01(1f - distance) * 0.80f;
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        generatedSweepSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return generatedSweepSprite;
    }

    // ============================================================
    // DIVIDER
    // ============================================================

    private void CreateGoldDivider()
    {
        GameObject line = UIBuilder.CreatePanelSized(
            screenObj.transform,
            "LogoLine",
            TOBATheme.BatakGold,
            new Vector2(520f, 5f),
            new Vector2(0f, 315f)
        );

        Image image = line.GetComponent<Image>();
        if (image != null) image.raycastTarget = false;
    }

    // ============================================================
    // BUTTON STYLE
    // ============================================================

    private void StyleWelcomeButton(GameObject buttonObj)
    {
        if (buttonObj == null) return;

        TextMeshProUGUI[] texts =
            buttonObj.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in texts)
        {
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 0.9f;
            text.raycastTarget = false;
        }

        Image image = buttonObj.GetComponent<Image>();
        if (image != null) image.raycastTarget = true;

        Outline outline = buttonObj.GetComponent<Outline>();
        if (outline == null) outline = buttonObj.AddComponent<Outline>();

        outline.effectColor = new Color(
            TOBATheme.BatakGold.r,
            TOBATheme.BatakGold.g,
            TOBATheme.BatakGold.b,
            0.55f
        );
        outline.effectDistance = new Vector2(2f, 2f);

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.86f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.65f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
        }
    }

    // ============================================================
    // BUTTON PRESS
    // ============================================================

    private IEnumerator PressWelcomeButton(
        Transform target,
        System.Action onFinished
    )
    {
        if (target != null)
        {
            Vector3 original = target.localScale;
            float time = 0f;

            while (time < 0.09f && target != null)
            {
                time += Time.deltaTime;
                target.localScale = Vector3.Lerp(
                    original,
                    original * 0.91f,
                    time / 0.09f
                );
                yield return null;
            }

            time = 0f;
            while (time < 0.07f && target != null)
            {
                time += Time.deltaTime;
                target.localScale = Vector3.Lerp(
                    original * 0.91f,
                    original * 1.04f,
                    time / 0.07f
                );
                yield return null;
            }

            time = 0f;
            while (time < 0.06f && target != null)
            {
                time += Time.deltaTime;
                target.localScale = Vector3.Lerp(
                    original * 1.04f,
                    original,
                    time / 0.06f
                );
                yield return null;
            }

            if (target != null)
                target.localScale = original;
        }

        SoundManager.Instance.PlaySFX("sfx_click");
        onFinished?.Invoke();
    }

    // ============================================================
    // FLOAT BUTTONS
    // ============================================================

    private IEnumerator FloatUI(
        Transform target,
        float phase,
        float speed
    )
    {
        if (target == null) yield break;

        Vector3 origin = target.localPosition;
        float time = phase;

        while (target != null)
        {
            time += Time.deltaTime * speed;
            target.localPosition = new Vector3(
                origin.x,
                origin.y + Mathf.Sin(time) * 2.5f,
                origin.z
            );
            yield return null;
        }
    }
}