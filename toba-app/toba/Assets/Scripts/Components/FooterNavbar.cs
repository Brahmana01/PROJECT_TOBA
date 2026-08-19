using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class FooterNavbar
{
    public static GameObject Create(
        Transform parent,
        string activeLabel,
        System.Action onHome,
        System.Action onGallery,
        System.Action onBattle,
        System.Action onLeaderboard,
        System.Action onProfile)
    {
        // ============================================================
        // FOOTER BOARD
        // ============================================================

        Sprite footerSprite =
            Resources.Load<Sprite>(
                "gambar/toba_footer_panel"
            );

        if (footerSprite == null)
        {
            Debug.LogWarning(
                "[FooterNavbar] toba_footer_panel tidak ditemukan."
            );
        }

        GameObject footer =
            new GameObject(
                "FooterNavbar"
            );

        footer.transform.SetParent(
            parent,
            false
        );

        RectTransform rtFooter =
            footer.AddComponent<RectTransform>();

        rtFooter.anchorMin =
            new Vector2(
                0f,
                0f
            );

        rtFooter.anchorMax =
            new Vector2(
                1f,
                0.17f
            );

        rtFooter.offsetMin =
            rtFooter.offsetMax =
            Vector2.zero;

        Image footerImage =
            footer.AddComponent<Image>();

        footerImage.sprite =
            footerSprite
            ?? TOBATheme.LoadSprite("panel_wood")
            ?? UIBuilder.GetDefaultSprite();

        footerImage.type =
            Image.Type.Sliced;

        footerImage.color =
            Color.white;

        footerImage.raycastTarget =
            false;

        Shadow footerShadow =
            footer.AddComponent<Shadow>();

        footerShadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.55f
            );

        footerShadow.effectDistance =
            new Vector2(
                0f,
                -5f
            );


        // ============================================================
        // NAV DATA
        // ============================================================

        string[] labels =
        {
            "Beranda",
            "Galeri",
            "Battle",
            "Papan",
            "Profil"
        };

        string[] iconPaths =
        {
            "gambar/icon_home",
            "gambar/icon_gallery",
            "gambar/icon_battle",
            "gambar/icon_leaderboard",
            "gambar/icon_profil"
        };

        System.Action[] actions =
        {
            onHome,
            onGallery,
            onBattle,
            onLeaderboard,
            onProfile
        };

        float[] normalizedX =
        {
            0.10f,
            0.30f,
            0.50f,
            0.70f,
            0.90f
        };


        // ============================================================
        // NAV ITEMS
        // ============================================================

        for (int i = 0;
             i < labels.Length;
             i++)
        {
            bool isActive =
                labels[i] == activeLabel;


            GameObject item =
                new GameObject(
                    "Nav_" +
                    labels[i]
                );

            item.transform.SetParent(
                footer.transform,
                false
            );

            RectTransform rt =
                item.AddComponent<RectTransform>();

            rt.anchorMin =
                rt.anchorMax =
                new Vector2(
                    normalizedX[i],
                    0.50f
                );

            rt.sizeDelta =
                new Vector2(
                    190f,
                    150f
                );

            rt.anchoredPosition =
                Vector2.zero;


            // ========================================================
            // HITBOX
            // ========================================================

            Image hitImage =
                item.AddComponent<Image>();

            hitImage.sprite =
                UIBuilder.GetDefaultSprite();

            hitImage.color =
                Color.clear;

            hitImage.raycastTarget =
                true;


            Button button =
                item.AddComponent<Button>();

            button.targetGraphic =
                hitImage;

            button.transition =
                Selectable.Transition.None;


            // ========================================================
            // ICON
            // ========================================================

            Sprite iconSprite =
                LoadSpriteFlexible(
                    iconPaths[i]
                );

            if (iconSprite != null)
            {
                // Glow
                GameObject glow =
                    UIBuilder.CreateImage(
                        item.transform,
                        "HoverGlow",
                        iconSprite,
                        new Vector2(
                            112f,
                            112f
                        ),
                        new Vector2(
                            0f,
                            20f
                        ),
                        new Color(
                            TOBATheme.BatakGold.r,
                            TOBATheme.BatakGold.g,
                            TOBATheme.BatakGold.b,
                            isActive
                                ? 0.16f
                                : 0f
                        )
                    );

                Image glowImg =
                    glow.GetComponent<Image>();

                glowImg.preserveAspect =
                    true;

                glowImg.raycastTarget =
                    false;


                // Shadow
                GameObject shadow =
                    UIBuilder.CreateImage(
                        item.transform,
                        "IconShadow",
                        iconSprite,
                        new Vector2(
                            100f,
                            100f
                        ),
                        new Vector2(
                            4f,
                            15f
                        ),
                        new Color(
                            0f,
                            0f,
                            0f,
                            0.40f
                        )
                    );

                Image shadowImg =
                    shadow.GetComponent<Image>();

                shadowImg.preserveAspect =
                    true;

                shadowImg.raycastTarget =
                    false;


                // Main icon
                GameObject icon =
                    UIBuilder.CreateImage(
                        item.transform,
                        "Icon",
                        iconSprite,
                        new Vector2(
                            96f,
                            96f
                        ),
                        new Vector2(
                            0f,
                            22f
                        )
                    );

                Image iconImage =
                    icon.GetComponent<Image>();

                iconImage.preserveAspect =
                    true;

                iconImage.color =
                    isActive
                        ? Color.white
                        : new Color(
                            1f,
                            1f,
                            1f,
                            0.90f
                        );

                iconImage.raycastTarget =
                    false;


                // Outline
                Outline iconOutline =
                    icon.AddComponent<Outline>();

                iconOutline.effectColor =
                    new Color(
                        TOBATheme.BatakGold.r,
                        TOBATheme.BatakGold.g,
                        TOBATheme.BatakGold.b,
                        isActive
                            ? 0.72f
                            : 0.12f
                    );

                iconOutline.effectDistance =
                    new Vector2(
                        isActive ? 2f : 1f,
                        isActive ? 2f : 1f
                    );


                // ====================================================
                // LABEL
                // ====================================================

                GameObject textObject =
                    UIBuilder.CreateText(
                        item.transform,
                        labels[i],
                        20,
                        isActive
                            ? TOBATheme.BatakGold
                            : TOBATheme.TextLight,
                        TextAlignmentOptions.Center
                    );

                RectTransform rtText =
                    textObject.GetComponent<RectTransform>();

                rtText.anchorMin =
                    new Vector2(
                        0f,
                        0.035f
                    );

                rtText.anchorMax =
                    new Vector2(
                        1f,
                        0.245f
                    );

                rtText.offsetMin =
                    rtText.offsetMax =
                    Vector2.zero;


                TextMeshProUGUI label =
                    textObject.GetComponent<TextMeshProUGUI>();

                label.fontStyle =
                    FontStyles.Bold;

                label.characterSpacing =
                    0.6f;

                label.raycastTarget =
                    false;


                // ====================================================
                // ACTIVE BAR
                // ====================================================

                GameObject activeBar =
                    new GameObject(
                        "ActiveBar"
                    );

                activeBar.transform.SetParent(
                    item.transform,
                    false
                );

                RectTransform rtBar =
                    activeBar.AddComponent<RectTransform>();

                rtBar.anchorMin =
                    rtBar.anchorMax =
                    new Vector2(
                        0.5f,
                        0f
                    );

                rtBar.sizeDelta =
                    new Vector2(
                        84f,
                        isActive
                            ? 6f
                            : 0f
                    );

                rtBar.anchoredPosition =
                    new Vector2(
                        0f,
                        -50f
                    );

                Image barImage =
                    activeBar.AddComponent<Image>();

                barImage.sprite =
                    UIBuilder.GetDefaultSprite();

                barImage.color =
                    isActive
                        ? TOBATheme.BatakGold
                        : Color.clear;

                barImage.raycastTarget =
                    false;


                // ====================================================
                // ANIMATION
                // ====================================================

                if (isActive)
                {
                    ScreenManager.Instance.StartCoroutine(
                        PulseActiveIcon(
                            icon.transform,
                            glow.transform
                        )
                    );
                }
                else
                {
                    ScreenManager.Instance.StartCoroutine(
                        FloatFooterIcon(
                            icon.transform,
                            glow.transform,
                            i
                        )
                    );
                }


                // ====================================================
                // BUTTON
                // ====================================================

                Transform capturedItem =
                    item.transform;

                Transform capturedIcon =
                    icon.transform;

                Transform capturedGlow =
                    glow.transform;

                System.Action capturedAction =
                    actions[i];

                button.onClick.AddListener(() =>
                {
                    ScreenManager.Instance.StartCoroutine(
                        PressFooter(
                            capturedItem,
                            capturedIcon,
                            capturedGlow,
                            capturedAction
                        )
                    );
                });


                // ====================================================
                // HOVER
                // ====================================================

                FooterHoverHandler hover =
                    item.AddComponent<FooterHoverHandler>();

                hover.Setup(
                    capturedItem,
                    capturedIcon,
                    capturedGlow,
                    isActive
                );
            }
            else
            {
                System.Action capturedAction =
                    actions[i];

                button.onClick.AddListener(() =>
                {
                    capturedAction?.Invoke();
                });
            }
        }


        return footer;
    }


    // ============================================================
    // LOAD SPRITE
    // ============================================================

    static Sprite LoadSpriteFlexible(
        string path)
    {
        Sprite sprite =
            Resources.Load<Sprite>(path);

        if (sprite != null)
            return sprite;

        Texture2D texture =
            Resources.Load<Texture2D>(path);

        if (texture != null)
        {
            return Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    texture.width,
                    texture.height
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );
        }

        Debug.LogWarning(
            "[FooterNavbar] Icon tidak ditemukan: " +
            path
        );

        return null;
    }


    // ============================================================
    // SET PROFILE CHARACTER
    // ============================================================

    public static void SetProfileCharacter(
        GameObject footer,
        int characterID)
    {
        if (footer == null)
            return;

        string path =
            characterID == 2
                ? "char_butet"
                : "char_ucok";

        Sprite character =
            LoadSpriteFlexible(
                path
            );

        if (character == null)
            return;


        Transform profile =
            footer.transform.Find(
                "Nav_Profil"
            );

        if (profile == null)
            return;


        Transform icon =
            profile.Find(
                "Icon"
            );

        Transform shadow =
            profile.Find(
                "IconShadow"
            );

        Transform glow =
            profile.Find(
                "HoverGlow"
            );


        if (icon != null)
        {
            Image img =
                icon.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    character;

                img.preserveAspect =
                    true;
            }
        }


        if (shadow != null)
        {
            Image img =
                shadow.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    character;

                img.preserveAspect =
                    true;
            }
        }


        if (glow != null)
        {
            Image img =
                glow.GetComponent<Image>();

            if (img != null)
            {
                img.sprite =
                    character;

                img.preserveAspect =
                    true;
            }
        }
    }


    // ============================================================
    // ACTIVE PULSE
    // ============================================================

    static IEnumerator PulseActiveIcon(
        Transform icon,
        Transform glow)
    {
        if (icon == null)
            yield break;

        float phase =
            0f;

        while (
            icon != null &&
            glow != null)
        {
            phase +=
                Time.deltaTime *
                1.15f;

            icon.localScale =
                Vector3.one *
                (
                    1f +
                    Mathf.Sin(phase) *
                    0.045f
                );

            glow.localScale =
                Vector3.one *
                (
                    1f +
                    Mathf.Sin(phase) *
                    0.08f
                );

            yield return null;
        }
    }


    // ============================================================
    // FLOAT NON-ACTIVE
    // ============================================================

    static IEnumerator FloatFooterIcon(
        Transform icon,
        Transform glow,
        int index)
    {
        if (icon == null)
            yield break;

        float phase =
            index * 0.8f;

        Vector3 baseIconPos =
            icon.localPosition;

        Vector3 baseGlowPos =
            glow != null
                ? glow.localPosition
                : Vector3.zero;

        while (
            icon != null)
        {
            phase +=
                Time.deltaTime *
                0.72f;

            float y =
                Mathf.Sin(phase) *
                2.4f;

            float rot =
                Mathf.Sin(
                    phase * 0.8f
                ) *
                0.7f;

            icon.localPosition =
                new Vector3(
                    baseIconPos.x,
                    baseIconPos.y + y,
                    baseIconPos.z
                );

            icon.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    rot
                );

            if (glow != null)
            {
                glow.localPosition =
                    new Vector3(
                        baseGlowPos.x,
                        baseGlowPos.y + y,
                        baseGlowPos.z
                    );
            }

            yield return null;
        }
    }


    // ============================================================
    // PRESS
    // ============================================================

    static IEnumerator PressFooter(
        Transform item,
        Transform icon,
        Transform glow,
        System.Action action)
    {
        if (item == null)
        {
            action?.Invoke();
            yield break;
        }


        float t = 0f;

        while (t < 0.08f)
        {
            t +=
                Time.deltaTime;

            float p =
                t / 0.08f;

            item.localScale =
                Vector3.one *
                Mathf.Lerp(
                    1f,
                    0.94f,
                    p
                );

            if (icon != null)
            {
                icon.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        1f,
                        0.88f,
                        p
                    );
            }

            yield return null;
        }


        t = 0f;

        while (t < 0.10f)
        {
            t +=
                Time.deltaTime;

            float p =
                t / 0.10f;

            item.localScale =
                Vector3.one *
                Mathf.Lerp(
                    0.94f,
                    1.06f,
                    p
                );

            if (icon != null)
            {
                icon.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        0.88f,
                        1.10f,
                        p
                    );
            }

            if (glow != null)
            {
                glow.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        1f,
                        1.18f,
                        p
                    );
            }

            yield return null;
        }


        t = 0f;

        while (t < 0.08f)
        {
            t +=
                Time.deltaTime;

            float p =
                t / 0.08f;

            item.localScale =
                Vector3.one *
                Mathf.Lerp(
                    1.06f,
                    1f,
                    p
                );

            if (icon != null)
            {
                icon.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        1.10f,
                        1f,
                        p
                    );
            }

            yield return null;
        }


        item.localScale =
            Vector3.one;

        if (icon != null)
            icon.localScale =
                Vector3.one;

        action?.Invoke();
    }
}


// ====================================================================
// HOVER HANDLER
// ====================================================================

public class FooterHoverHandler :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private Transform item;
    private Transform icon;
    private Transform glow;

    private bool isActive;
    private bool hovering;


    public void Setup(
        Transform itemTransform,
        Transform iconTransform,
        Transform glowTransform,
        bool active)
    {
        item =
            itemTransform;

        icon =
            iconTransform;

        glow =
            glowTransform;

        isActive =
            active;
    }


    public void OnPointerEnter(
        PointerEventData eventData)
    {
        hovering =
            true;

        if (item != null)
        {
            StopAllCoroutines();

            StartCoroutine(
                HoverIn()
            );
        }
    }


    public void OnPointerExit(
        PointerEventData eventData)
    {
        hovering =
            false;

        if (item != null)
        {
            StopAllCoroutines();

            StartCoroutine(
                HoverOut()
            );
        }
    }


    IEnumerator HoverIn()
    {
        Vector3 itemTarget =
            Vector3.one *
            1.06f;

        Vector3 iconTarget =
            Vector3.one *
            1.10f;

        Vector3 glowTarget =
            Vector3.one *
            1.18f;

        float t = 0f;

        while (
            t < 0.14f &&
            hovering)
        {
            t +=
                Time.unscaledDeltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t / 0.14f
                );

            if (item != null)
            {
                item.localScale =
                    Vector3.Lerp(
                        item.localScale,
                        itemTarget,
                        p
                    );
            }

            if (icon != null)
            {
                icon.localScale =
                    Vector3.Lerp(
                        icon.localScale,
                        iconTarget,
                        p
                    );
            }

            if (glow != null)
            {
                glow.localScale =
                    Vector3.Lerp(
                        glow.localScale,
                        glowTarget,
                        p
                    );
            }

            yield return null;
        }
    }


    IEnumerator HoverOut()
    {
        Vector3 itemTarget =
            Vector3.one;

        Vector3 iconTarget =
            Vector3.one;

        Vector3 glowTarget =
            Vector3.one *
            (isActive
                ? 1.04f
                : 1f);

        float t = 0f;

        while (
            t < 0.16f &&
            !hovering)
        {
            t +=
                Time.unscaledDeltaTime;

            float p =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t / 0.16f
                );

            if (item != null)
            {
                item.localScale =
                    Vector3.Lerp(
                        item.localScale,
                        itemTarget,
                        p
                    );
            }

            if (icon != null)
            {
                icon.localScale =
                    Vector3.Lerp(
                        icon.localScale,
                        iconTarget,
                        p
                    );
            }

            if (glow != null)
            {
                glow.localScale =
                    Vector3.Lerp(
                        glow.localScale,
                        glowTarget,
                        p
                    );
            }

            yield return null;
        }
    }
}