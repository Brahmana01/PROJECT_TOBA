using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapScreen : BaseScreen
{
    // ============================================================
    // DATA - TETAP MENGIKUTI SISTEM LAMA
    // ============================================================

    private List<LocationData> locationList =
        new List<LocationData>();

    private List<PlayerProgressData> progressList =
        new List<PlayerProgressData>();

    // ============================================================
    // WORLD CONFIG
    // ============================================================

    private const float WORLD_WIDTH = 1080f;
    private const float SEGMENT_HEIGHT = 2200f;

    // Ruang aman agar Level 1 tidak tertutup header.
    private const float WORLD_TOP_PADDING = 180f;

    private const float NODE_SIZE = 150f;

    // ============================================================
    // MASTER 6-SLOT PATTERN
    //
    // Ini BUKAN posisi Level 1, 2, 3 secara permanen.
    //
    // Ini adalah pattern yang berulang:
    //
    // Slot 0 = kiri
    // Slot 1 = kanan
    // Slot 2 = kiri
    // Slot 3 = kanan
    // Slot 4 = kiri
    // Slot 5 = kanan
    //
    // Jarak vertikal mengikuti pattern yang kita tanam
    // ke 7 background:
    //
    // 400 -> 760  = 360
    // 760 -> 1120 = 360
    // 1120 -> 1480 = 360
    // 1480 -> 1840 = 360
    // 1840 -> 2040 = 200
    // ============================================================

    private static readonly Vector2[] MASTER_PATTERN =
    {
        new Vector2(350f, 400f),   // L1
        new Vector2(680f, 675f),   // L2
        new Vector2(350f, 950f),   // L3
        new Vector2(680f, 1225f),  // L4
        new Vector2(350f, 1500f),  // L5
        new Vector2(680f, 1775f)   // L6
    };

    // ============================================================
    // 7 BACKGROUND ASSETS
    // ============================================================

    private static readonly string[] SEGMENT_ASSETS =
    {
        "MapScreen/segment_01_morning_valley",
        "MapScreen/segment_02_river_canyon",
        "MapScreen/segment_03_root_ruins_forest",
        "MapScreen/segment_04_lake_wetland_rainbow",
        "MapScreen/segment_05_highland_bamboo_valley",
        "MapScreen/segment_06_windy_misty_highland",
        "MapScreen/segment_07_ancient_batak_world"
    };

    // ============================================================
    // REFERENCES
    // ============================================================

    private RectTransform worldContent;
    private ScrollRect scrollRect;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public MapScreen(Transform parent)
        : base(parent, "MapScreen")
    {
    }

    // ============================================================
    // SHOW
    // ============================================================

    public override void Show()
    {
        SetBackground(
            TOBATheme.ForestGreen
        );

        ShowLoading();
        FetchData();
    }

    // ============================================================
    // LOADING
    // ============================================================

    private void ShowLoading()
    {
        GameObject loading =
            new GameObject("Loading");

        loading.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform rt =
            loading.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(0.5f, 0.5f);

        rt.sizeDelta =
            new Vector2(
                700f,
                80f
            );

        rt.anchoredPosition =
            Vector2.zero;

        TextMeshProUGUI text =
            loading.AddComponent<TextMeshProUGUI>();

        text.text =
            "Memuat peta...";

        text.fontSize = 38f;
        text.fontStyle = FontStyles.Bold;
        text.color = TOBATheme.BatakGold;
        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget = false;
    }

    // ============================================================
    // API
    // TETAP MENGIKUTI KODE LAMA
    // ============================================================

    private void FetchData()
    {
        int playerID =
            PlayerPrefs.GetInt("PlayerID");

        APIManager.Instance.GetRequest(
            "/locations",
            (locationResponse) =>
            {
                if (screenObj == null)
                    return;

                LocationListResponse locations =
                    JsonUtility.FromJson<LocationListResponse>(
                        "{\"locations\":" +
                        locationResponse +
                        "}"
                    );

                if (
                    locations != null &&
                    locations.locations != null
                )
                {
                    locationList =
                        locations.locations;
                }

                APIManager.Instance.GetRequest(
                    "/player/" +
                    playerID +
                    "/progress",
                    (progressResponse) =>
                    {
                        if (screenObj == null)
                            return;

                        PlayerProgressResponse progress =
                            JsonUtility.FromJson<PlayerProgressResponse>(
                                "{\"progress\":" +
                                progressResponse +
                                "}"
                            );

                        if (
                            progress != null &&
                            progress.progress != null
                        )
                        {
                            progressList =
                                progress.progress;
                        }

                        BuildMap();
                    },
                    (_) =>
                    {
                        BuildMap();
                    }
                );
            },
            (_) =>
            {
                Debug.LogWarning(
                    "[Map] Gagal mengambil locations."
                );

                BuildMap();
            }
        );
    }

    // ============================================================
    // BUILD
    // ============================================================

    private void BuildMap()
    {
        if (screenObj == null)
            return;

        Transform loading =
            screenObj.transform.Find("Loading");

        if (loading != null)
        {
            UnityEngine.Object.Destroy(
                loading.gameObject
            );
        }

        CreateWorld();
        CreateFixedHeader();
        CreateNodes();
    }

    // ============================================================
    // WORLD / SCROLL
    // ============================================================

    private void CreateWorld()
    {
        scrollRect =
            UIBuilder.CreateScrollView(
                screenObj.transform,
                "MapScroll",
                Vector2.zero,
                Vector2.one
            );

        if (scrollRect == null)
        {
            Debug.LogError(
                "[Map] ScrollView gagal dibuat."
            );

            return;
        }

        scrollRect.horizontal =
            false;

        scrollRect.vertical =
            true;

        scrollRect.movementType =
            ScrollRect.MovementType.Clamped;

        scrollRect.inertia =
            true;

        scrollRect.decelerationRate =
            0.12f;

        scrollRect.scrollSensitivity =
            38f;

        worldContent =
            scrollRect.content;

        if (worldContent == null)
        {
            Debug.LogError(
                "[Map] Scroll content null."
            );

            return;
        }

        worldContent.anchorMin =
            new Vector2(
                0.5f,
                1f
            );

        worldContent.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        worldContent.pivot =
            new Vector2(
                0.5f,
                1f
            );

        /*
         * Ini penting:
         *
         * World digeser sedikit ke bawah agar
         * area Segment 01 tidak langsung bertabrakan
         * dengan header fixed.
         */
        worldContent.anchoredPosition =
            new Vector2(
                0f,
                -WORLD_TOP_PADDING
            );

        CreateSegmentBackgrounds();

        CreateEnvironmentEffects();

        SetWorldHeight();


    }

    // ============================================================
    // BACKGROUND SEGMENTS
    // ============================================================

    private void CreateSegmentBackgrounds()
    {
        if (
            worldContent == null ||
            locationList == null ||
            locationList.Count == 0
        )
        {
            return;
        }

        int segmentCount =
            Mathf.CeilToInt(
                locationList.Count / 6f
            );

        for (
            int segmentIndex = 0;
            segmentIndex < segmentCount;
            segmentIndex++
        )
        {
            CreateSingleSegment(
                segmentIndex
            );
        }
    }

    private void CreateSingleSegment(
        int segmentIndex)
    {
        string assetPath =
            SEGMENT_ASSETS[
                segmentIndex %
                SEGMENT_ASSETS.Length
            ];

        Sprite background =
            TOBATheme.LoadSprite(
                assetPath
            );

        if (background == null)
        {
            Debug.LogError(
                "[Map] Background tidak ditemukan: " +
                assetPath
            );

            return;
        }

        GameObject bg =
            new GameObject(
                "MapSegment_" +
                (segmentIndex + 1)
            );

        bg.transform.SetParent(
            worldContent,
            false
        );

        RectTransform rt =
            bg.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        rt.pivot =
            new Vector2(
                0.5f,
                1f
            );

        rt.sizeDelta =
            new Vector2(
                WORLD_WIDTH,
                SEGMENT_HEIGHT
            );

        /*
         * Karena worldContent sudah punya TOP PADDING,
         * setiap background hanya perlu ditempatkan
         * berdasarkan segment.
         */
        rt.anchoredPosition =
            new Vector2(
                0f,
                -(
                    segmentIndex *
                    SEGMENT_HEIGHT
                )
            );

        Image image =
            bg.AddComponent<Image>();

        image.sprite =
            background;

        image.type =
            Image.Type.Simple;

        image.preserveAspect =
            false;

        image.raycastTarget =
            false;
    }

    // ============================================================
    // WORLD HEIGHT
    // ============================================================

    private void SetWorldHeight()
    {
        if (worldContent == null)
            return;

        if (
            locationList == null ||
            locationList.Count == 0
        )
        {
            worldContent.sizeDelta =
                new Vector2(
                    0f,
                    SEGMENT_HEIGHT
                );

            return;
        }

        int lastIndex =
            locationList.Count - 1;

        int lastSegment =
            lastIndex / 6;

        int lastSlot =
            lastIndex % 6;

        float lastSlotY =
            MASTER_PATTERN[
                lastSlot
            ].y;

        /*
         * Tinggi sampai lokasi terakhir.
         */
        float gameplayEnd =
            lastSegment *
            SEGMENT_HEIGHT
            +
            lastSlotY
            +
            350f;

        /*
         * Minimum untuk satu segment.
         */
        float minimum =
            (
                lastSegment + 1
            ) *
            SEGMENT_HEIGHT;

        float finalHeight =
            Mathf.Max(
                gameplayEnd,
                minimum
            );

        /*
         * Tambahkan top padding karena worldContent
         * kita sudah digeser ke bawah.
         */
        finalHeight +=
            WORLD_TOP_PADDING;

        worldContent.sizeDelta =
            new Vector2(
                0f,
                finalHeight
            );
    }

    // ============================================================
    // FIXED HEADER
    // ============================================================

    private void CreateFixedHeader()
    {
        GameObject layer =
            new GameObject(
                "HeaderLayer"
            );

        layer.transform.SetParent(
            screenObj.transform,
            false
        );

        RectTransform layerRT =
            layer.AddComponent<RectTransform>();

        layerRT.anchorMin =
            Vector2.zero;

        layerRT.anchorMax =
            Vector2.one;

        layerRT.offsetMin =
            Vector2.zero;

        layerRT.offsetMax =
            Vector2.zero;

        layer.transform.SetAsLastSibling();

        CreateHeaderBoard(
            layer.transform
        );

        CreateBackButton(
            layer.transform
        );
    }

    // ============================================================
    // HEADER BOARD
    // ============================================================

    private void CreateHeaderBoard(
        Transform parent)
    {
        Sprite header =
            TOBATheme.LoadSprite(
                "MapScreen/map_header_board"
            );

        if (header == null)
        {
            Debug.LogError(
                "[Map] map_header_board tidak ditemukan."
            );

            return;
        }

        /*
         * Header sedikit lebih tinggi agar benar-benar
         * menjadi elemen tersendiri di atas world.
         */
        GameObject board =
            UIBuilder.CreateImage(
                parent,
                "MapHeader",
                header,
                new Vector2(
                    980f,
                    225f
                ),
                new Vector2(
                    0f,
                    770f
                )
            );

        Image image =
            board.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }

        CreateHeaderShine(
            board.transform
        );
    }

    // ============================================================
    // HEADER SHINE
    // ============================================================

    private void CreateHeaderShine(
        Transform parent)
    {
        Sprite shine =
            TOBATheme.LoadSprite(
                "MapScreen/map_header_shine"
            );

        if (shine == null)
            return;

        GameObject shineObject =
            UIBuilder.CreateImage(
                parent,
                "HeaderShine",
                shine,
                new Vector2(
                    170f,
                    100f
                ),
                new Vector2(
                    -500f,
                    0f
                ),
                new Color(
                    1f,
                    1f,
                    1f,
                    0.55f
                )
            );

        Image image =
            shineObject.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                false;
        }

        ScreenManager.Instance.StartCoroutine(
            HeaderShineLoop(
                shineObject.transform
            )
        );
    }

    private IEnumerator HeaderShineLoop(
        Transform target)
    {
        if (target == null)
            yield break;

        while (target != null)
        {
            float p =
                Mathf.Repeat(
                    Time.time * 0.17f,
                    1.35f
                );

            target.localPosition =
                new Vector3(
                    -500f +
                    p * 1000f,
                    0f,
                    0f
                );

            yield return null;
        }
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    private void CreateBackButton(
        Transform parent)
    {
        Sprite back =
            TOBATheme.LoadSprite(
                "RegisterNameScreen/register_name_back_button_v2"
            );

        if (back == null)
            return;

        GameObject buttonObject =
            UIBuilder.CreateImage(
                parent,
                "BackButton",
                back,
                new Vector2(
                    145f,
                    145f
                ),
                new Vector2(
                    -450f,
                    765f
                )
            );

        Image image =
            buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                true;

            image.raycastTarget =
                true;
        }

        Button button =
            buttonObject.AddComponent<Button>();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.None;

        button.onClick.AddListener(
            () =>
            {
                ScreenManager.Instance.StartCoroutine(
                    BackButtonPress(
                        buttonObject.transform
                    )
                );
            }
        );

        ScreenManager.Instance.StartCoroutine(
            BackButtonFloat(
                buttonObject.transform
            )
        );
    }

    private IEnumerator BackButtonFloat(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        float time = 0f;

        while (target != null)
        {
            time +=
                Time.deltaTime;

            target.localPosition =
                new Vector3(
                    origin.x,
                    origin.y +
                    Mathf.Sin(
                        time * 1.1f
                    ) * 2f,
                    origin.z
                );

            yield return null;
        }
    }

    private IEnumerator BackButtonPress(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        target.localScale =
            original * 0.88f;

        yield return new WaitForSeconds(
            0.07f
        );

        if (target == null)
            yield break;

        target.localScale =
            original * 1.06f;

        yield return new WaitForSeconds(
            0.07f
        );

        if (target == null)
            yield break;

        target.localScale =
            original;

        SoundManager.Instance.PlaySFX(
            "sfx_click"
        );

        ScreenManager.Instance.ChangeScreen(
            new MainMenuScreen(
                container
            )
        );
    }

    // ============================================================
    // NODE CREATION
    // ============================================================

    private void CreateNodes()
    {
        if (
            worldContent == null ||
            locationList == null
        )
        {
            return;
        }

        for (
            int i = 0;
            i < locationList.Count;
            i++
        )
        {
            LocationData location =
                locationList[i];

            Vector2 position =
                GetPatternPosition(i);

            bool unlocked =
                IsUnlocked(
                    location.id,
                    i
                );

            bool completed =
                IsCompleted(
                    location.id
                );

            CreateNode(
                location,
                i,
                position,
                unlocked,
                completed
            );
        }
    }

    // ============================================================
    // PATTERN POSITION
    //
    // Ini inti sistem:
    //
    // index 0..5  = pattern 1 segment
    // index 6..11 = pattern berikutnya
    // index 12..17 = pattern berikutnya
    //
    // dst.
    // ============================================================

    private Vector2 GetPatternPosition(
        int locationIndex)
    {
        int segmentIndex =
            locationIndex / 6;

        int slotIndex =
            locationIndex % 6;

        Vector2 pattern =
            MASTER_PATTERN[
                slotIndex
            ];

        float x =
            pattern.x -
            WORLD_WIDTH / 2f;

        float y =
            -(
                segmentIndex *
                SEGMENT_HEIGHT
                +
                pattern.y
            );

        return new Vector2(
            x,
            y
        );
    }

    // ============================================================
    // NODE
    // ============================================================

    private void CreateNode(
        LocationData location,
        int index,
        Vector2 position,
        bool unlocked,
        bool completed)
    {
        string asset =
            completed
                ? "MapScreen/map_node_completed"
                : unlocked
                    ? "MapScreen/map_node_active"
                    : "MapScreen/map_node_locked";

        Sprite sprite =
            TOBATheme.LoadSprite(
                asset
            );

        if (sprite == null)
        {
            Debug.LogWarning(
                "[Map] Node sprite missing: " +
                asset
            );

            return;
        }

        GameObject node =
            CreateWorldImage(
                "Node_" +
                (index + 1),
                sprite,
                new Vector2(
                    NODE_SIZE,
                    NODE_SIZE
                ),
                position
            );

        Image image =
            node.GetComponent<Image>();

        if (image == null)
            return;

        image.raycastTarget =
            true;

        /*
         * Nomor tetap ditampilkan.
         */
        string number =
            completed
                ? "✓"
                : (index + 1).ToString();

        CreateNodeNumber(
            node.transform,
            number,
            completed ? 58 : 54,
            unlocked
        );

        Button button =
            node.AddComponent<Button>();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.None;

        if (unlocked)
        {
            LocationData captured =
                location;

            button.onClick.AddListener(
                () =>
                {
                    SoundManager.Instance.PlaySFX(
                        "sfx_click"
                    );

                    ScreenManager.Instance.StartCoroutine(
                        NodeTouch(
                            node.transform
                        )
                    );

                    ScreenManager.Instance.ChangeScreen(
                        new LocationDetailScreen(
                            container,
                            captured
                        )
                    );
                }
            );

            if (completed)
            {
                ScreenManager.Instance.StartCoroutine(
                    CompletedNodeLoop(
                        node.transform
                    )
                );
            }
            else
            {
                CreateActiveNodeEffect(
                    node.transform,
                    index
                );
            }
        }
        else
        {
            button.onClick.AddListener(
                () =>
                {
                    SoundManager.Instance.PlaySFX(
                        "sfx_click"
                    );

                    ScreenManager.Instance.StartCoroutine(
                        LockedTouch(
                            node.transform
                        )
                    );
                }
            );
        }
    }

    // ============================================================
    // LEVEL NUMBER
    // ============================================================

    private void CreateNodeNumber(
        Transform parent,
        string value,
        int fontSize,
        bool unlocked)
    {
        GameObject obj =
            new GameObject(
                "LevelNumber"
            );

        obj.transform.SetParent(
            parent,
            false
        );

        RectTransform rt =
            obj.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rt.sizeDelta =
            new Vector2(
                120f,
                90f
            );

        rt.anchoredPosition =
            new Vector2(
                0f,
                5f
            );

        TextMeshProUGUI text =
            obj.AddComponent<TextMeshProUGUI>();

        text.text =
            value;

        text.fontSize =
            fontSize;

        text.fontStyle =
            FontStyles.Bold;

        text.alignment =
            TextAlignmentOptions.Center;

        text.enableWordWrapping =
            false;

        text.raycastTarget =
            false;

        text.color =
            unlocked
                ? new Color(
                    1f,
                    0.84f,
                    0.34f,
                    1f
                )
                : new Color(
                    0.76f,
                    0.64f,
                    0.35f,
                    0.75f
                );

        Outline outline =
            obj.AddComponent<Outline>();

        outline.effectColor =
            new Color(
                0.20f,
                0.06f,
                0.015f,
                0.95f
            );

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );

        Shadow shadow =
            obj.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.35f
            );

        shadow.effectDistance =
            new Vector2(
                1f,
                -2f
            );
    }

    // ============================================================
    // ACTIVE NODE
    // ============================================================

    private void CreateActiveNodeEffect(
        Transform node,
        int index)
    {
        CreateNodeGlow(
            node,
            index
        );

        CreateNodeSparkles(
            node,
            index
        );

        ScreenManager.Instance.StartCoroutine(
            ActiveNodeLoop(
                node,
                index * 0.35f
            )
        );
    }

    private IEnumerator ActiveNodeLoop(
        Transform target,
        float phase)
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        float t =
            phase;

        while (target != null)
        {
            t +=
                Time.deltaTime * 1.25f;

            target.localScale =
                original *
                (
                    1f +
                    Mathf.Abs(
                        Mathf.Sin(t)
                    ) * 0.035f
                );

            yield return null;
        }
    }

    // ============================================================
    // NODE GLOW
    // ============================================================

    private void CreateNodeGlow(
        Transform node,
        int index)
    {
        RectTransform nodeRT =
            node.GetComponent<RectTransform>();

        if (nodeRT == null)
            return;

        GameObject glow =
            CreateWorldPanel(
                "NodeGlow_" + index,
                new Color(
                    TOBATheme.BatakGold.r,
                    TOBATheme.BatakGold.g,
                    TOBATheme.BatakGold.b,
                    0.11f
                ),
                new Vector2(
                    NODE_SIZE + 36f,
                    NODE_SIZE + 36f
                ),
                nodeRT.anchoredPosition
            );

        Image glowImage =
            glow.GetComponent<Image>();

        if (glowImage != null)
        {
            glowImage.raycastTarget =
                false;
        }

        /*
         * Glow dibuat tepat sebelum node.
         */
        int nodeIndex =
            node.GetSiblingIndex();

        glow.transform.SetSiblingIndex(
            Mathf.Max(
                0,
                nodeIndex
            )
        );

        ScreenManager.Instance.StartCoroutine(
            GlowLoop(
                glow.transform,
                glowImage
            )
        );
    }

    private IEnumerator GlowLoop(
        Transform target,
        Image image)
    {
        if (
            target == null ||
            image == null
        )
        {
            yield break;
        }

        float time = 0f;

        while (target != null)
        {
            time +=
                Time.deltaTime * 1.35f;

            image.color =
                new Color(
                    TOBATheme.BatakGold.r,
                    TOBATheme.BatakGold.g,
                    TOBATheme.BatakGold.b,
                    0.05f +
                    Mathf.Abs(
                        Mathf.Sin(time)
                    ) * 0.18f
                );

            target.localScale =
                Vector3.one *
                (
                    1f +
                    Mathf.Abs(
                        Mathf.Sin(time)
                    ) * 0.045f
                );

            yield return null;
        }
    }

    // ============================================================
    // NODE SPARKLES
    // ============================================================

    private void CreateNodeSparkles(
        Transform node,
        int nodeIndex)
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_path_glint"
            );

        if (sprite == null)
            return;

        RectTransform nodeRT =
            node.GetComponent<RectTransform>();

        if (nodeRT == null)
            return;

        for (
            int i = 0;
            i < 3;
            i++
        )
        {
            float angle =
                i *
                Mathf.PI *
                0.666f;

            GameObject sparkle =
                CreateWorldImage(
                    "NodeSpark_" +
                    nodeIndex +
                    "_" +
                    i,
                    sprite,
                    new Vector2(
                        20f,
                        20f
                    ),
                    nodeRT.anchoredPosition +
                    new Vector2(
                        Mathf.Cos(angle) * 65f,
                        Mathf.Sin(angle) * 65f
                    ),
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.80f
                    )
                );

            ScreenManager.Instance.StartCoroutine(
                NodeSparkLoop(
                    sparkle.transform,
                    nodeRT.anchoredPosition,
                    angle
                )
            );
        }
    }

    private IEnumerator NodeSparkLoop(
        Transform target,
        Vector2 center,
        float phase)
    {
        if (target == null)
            yield break;

        RectTransform rt =
            target.GetComponent<
                RectTransform
            >();

        if (rt == null)
            yield break;

        while (target != null)
        {
            float t =
                Time.time * 0.82f +
                phase;

            rt.anchoredPosition =
                center +
                new Vector2(
                    Mathf.Cos(t) * 65f,
                    Mathf.Sin(t * 1.2f) * 65f
                );

            target.localScale =
                Vector3.one *
                (
                    0.65f +
                    Mathf.Abs(
                        Mathf.Sin(t)
                    ) * 0.30f
                );

            Image image =
                target.GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.10f +
                        Mathf.Abs(
                            Mathf.Sin(t)
                        ) * 0.80f
                    );
            }

            yield return null;
        }
    }

    // ============================================================
    // COMPLETED NODE
    // ============================================================

    private IEnumerator CompletedNodeLoop(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        float t = 0f;

        while (target != null)
        {
            t +=
                Time.deltaTime * 0.75f;

            target.localScale =
                original *
                (
                    1f +
                    Mathf.Abs(
                        Mathf.Sin(t)
                    ) * 0.02f
                );

            yield return null;
        }
    }

    // ============================================================
    // TOUCH ACTIVE
    // ============================================================

    private IEnumerator NodeTouch(
        Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original =
            target.localScale;

        target.localScale =
            original * 0.88f;

        yield return new WaitForSeconds(
            0.06f
        );

        if (target == null)
            yield break;

        target.localScale =
            original * 1.07f;

        yield return new WaitForSeconds(
            0.08f
        );

        if (target == null)
            yield break;

        target.localScale =
            original;
    }

    // ============================================================
    // TOUCH LOCKED
    // ============================================================

    private IEnumerator LockedTouch(
        Transform target)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;

        while (
            elapsed < 0.20f &&
            target != null
        )
        {
            elapsed +=
                Time.deltaTime;

            target.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(
                        elapsed * 80f
                    ) * 6f
                );

            yield return null;
        }

        if (target != null)
        {
            target.localRotation =
                Quaternion.identity;
        }
    }

    // ============================================================
    // ENVIRONMENT EFFECTS
    // ============================================================

    private void CreateEnvironmentEffects()
    {
        if (worldContent == null)
            return;

        CreateFireflies();
        CreateWaterShimmer();
        CreateWaterfallSpray();
        CreateForegroundLeaves();
        CreateHouseSmoke();

        /*
         * CLOUD CUMA DI SEAM SEGMENT.
         *
         * Bukan cloud kecil satu-satu.
         * 1 boundary = 1 curtain.
         */
        CreateCloudTransitions();
    }

    // ============================================================
    // FIREFLIES
    // ============================================================

    private void CreateFireflies()
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_firefly"
            );

        if (sprite == null)
            return;

        int segmentCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    locationList.Count / 6f
                )
            );

        int total =
            segmentCount * 6;

        for (
            int i = 0;
            i < total;
            i++
        )
        {
            int segment =
                i / 6;

            float x =
                Random.Range(
                    -440f,
                    440f
                );

            if (
                x > -120f &&
                x < 120f
            )
            {
                x =
                    x < 0f
                        ? -170f
                        : 170f;
            }

            float y =
                Random.Range(
                    300f,
                    1880f
                );

            GameObject firefly =
                CreateWorldImage(
                    "Firefly_" + i,
                    sprite,
                    new Vector2(
                        34f,
                        34f
                    ),
                    new Vector2(
                        x,
                        -(
                            segment *
                            SEGMENT_HEIGHT +
                            y
                        )
                    ),
                    new Color(
                        1f,
                        0.98f,
                        0.42f,
                        0.78f
                    )
                );

            ScreenManager.Instance.StartCoroutine(
                FireflyLoop(
                    firefly.transform,
                    firefly.GetComponent<Image>(),
                    Random.Range(0f, 10f)
                )
            );
        }
    }

    private IEnumerator FireflyLoop(
        Transform target,
        Image image,
        float phase)
    {
        if (
            target == null ||
            image == null
        )
        {
            yield break;
        }

        RectTransform rt =
            target.GetComponent<RectTransform>();

        if (rt == null)
            yield break;

        Vector2 origin =
            rt.anchoredPosition;

        while (target != null)
        {
            float t =
                Time.time * 0.72f +
                phase;

            rt.anchoredPosition =
                origin +
                new Vector2(
                    Mathf.Sin(t * 0.65f) * 20f,
                    Mathf.Cos(t * 0.48f) * 15f
                );

            image.color =
                new Color(
                    1f,
                    0.98f,
                    0.42f,
                    0.15f +
                    Mathf.Abs(
                        Mathf.Sin(t * 1.45f)
                    ) * 0.80f
                );

            yield return null;
        }
    }

    // ============================================================
    // WATER SHIMMER
    // ============================================================

    private void CreateWaterShimmer()
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_water_shimmer"
            );

        if (sprite == null)
            return;

        int segmentCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    locationList.Count / 6f
                )
            );

        for (
            int segment = 0;
            segment < segmentCount;
            segment++
        )
        {
            for (
                int i = 0;
                i < 3;
                i++
            )
            {
                float y =
                    320f +
                    i * 560f;

                GameObject shimmer =
                    CreateWorldImage(
                        "WaterShimmer_" +
                        segment +
                        "_" +
                        i,
                        sprite,
                        new Vector2(
                            190f,
                            62f
                        ),
                        new Vector2(
                            405f,
                            -(
                                segment *
                                SEGMENT_HEIGHT +
                                y
                            )
                        ),
                        new Color(
                            1f,
                            1f,
                            1f,
                            0.20f
                        )
                    );

                ScreenManager.Instance.StartCoroutine(
                    WaterShimmerLoop(
                        shimmer.transform,
                        i
                    )
                );
            }
        }
    }

    private IEnumerator WaterShimmerLoop(
        Transform target,
        int index)
    {
        if (target == null)
            yield break;

        RectTransform rt =
            target.GetComponent<
                RectTransform
            >();

        if (rt == null)
            yield break;

        Vector2 origin =
            rt.anchoredPosition;

        while (target != null)
        {
            float t =
                Time.time * 0.50f +
                index;

            rt.anchoredPosition =
                origin +
                new Vector2(
                    Mathf.Sin(t) * 28f,
                    Mathf.Sin(t * 0.75f) * 5f
                );

            Image image =
                target.GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.08f +
                        Mathf.Abs(
                            Mathf.Sin(t * 1.30f)
                        ) * 0.30f
                    );
            }

            yield return null;
        }
    }

    // ============================================================
    // WATERFALL SPRAY
    // ============================================================

    private void CreateWaterfallSpray()
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_waterfall_spray"
            );

        if (sprite == null)
            return;

        int segmentCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    locationList.Count / 6f
                )
            );

        for (
            int segment = 0;
            segment < segmentCount;
            segment++
        )
        {
            GameObject spray =
                CreateWorldImage(
                    "WaterfallSpray_" +
                    segment,
                    sprite,
                    new Vector2(
                        90f,
                        90f
                    ),
                    new Vector2(
                        410f,
                        -(
                            segment *
                            SEGMENT_HEIGHT +
                            480f
                        )
                    ),
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.24f
                    )
                );

            ScreenManager.Instance.StartCoroutine(
                WaterfallSprayLoop(
                    spray.transform,
                    segment
                )
            );
        }
    }

    private IEnumerator WaterfallSprayLoop(
        Transform target,
        int index)
    {
        if (target == null)
            yield break;

        Vector3 origin =
            target.localPosition;

        while (target != null)
        {
            float t =
                Time.time * 1.10f +
                index * 0.8f;

            target.localPosition =
                origin +
                new Vector3(
                    Mathf.Sin(t) * 6f,
                    Mathf.Cos(t * 1.3f) * 4f,
                    0f
                );

            yield return null;
        }
    }

    // ============================================================
    // FOREGROUND LEAVES
    // ============================================================

    private void CreateForegroundLeaves()
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_leaf_sway"
            );

        if (sprite == null)
            return;

        int segmentCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    locationList.Count / 6f
                )
            );

        for (
            int segment = 0;
            segment < segmentCount;
            segment++
        )
        {
            for (
                int i = 0;
                i < 3;
                i++
            )
            {
                float x =
                    i % 2 == 0
                        ? -465f
                        : 455f;

                float y =
                    600f +
                    i * 560f;

                GameObject leaves =
                    CreateWorldImage(
                        "LeafSway_" +
                        segment +
                        "_" +
                        i,
                        sprite,
                        new Vector2(
                            125f,
                            80f
                        ),
                        new Vector2(
                            x,
                            -(
                                segment *
                                SEGMENT_HEIGHT +
                                y
                            )
                        )
                    );

                ScreenManager.Instance.StartCoroutine(
                    LeafSwayLoop(
                        leaves.transform,
                        i
                    )
                );
            }
        }
    }

    private IEnumerator LeafSwayLoop(
        Transform target,
        int index)
    {
        if (target == null)
            yield break;

        while (target != null)
        {
            float t =
                Time.time * 0.72f +
                index * 0.8f;

            target.localEulerAngles =
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(t) * 4f
                );

            yield return null;
        }
    }

    // ============================================================
    // HOUSE SMOKE
    // ============================================================

    private void CreateHouseSmoke()
    {
        Sprite sprite =
            TOBATheme.LoadSprite(
                "MapScreen/map_smoke"
            );

        if (sprite == null)
            return;

        int segmentCount =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    locationList.Count / 6f
                )
            );

        for (
            int segment = 0;
            segment < segmentCount;
            segment++
        )
        {
            GameObject smoke =
                CreateWorldImage(
                    "HouseSmoke_" +
                    segment,
                    sprite,
                    new Vector2(
                        44f,
                        66f
                    ),
                    new Vector2(
                        -360f,
                        -(
                            segment *
                            SEGMENT_HEIGHT +
                            300f
                        )
                    ),
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.05f
                    )
                );

            ScreenManager.Instance.StartCoroutine(
                SmokeLoop(
                    smoke.transform,
                    segment
                )
            );
        }
    }

    private IEnumerator SmokeLoop(
        Transform target,
        int index)
    {
        if (target == null)
            yield break;

        RectTransform rt =
            target.GetComponent<
                RectTransform
            >();

        if (rt == null)
            yield break;

        Vector2 origin =
            rt.anchoredPosition;

        while (target != null)
        {
            float t =
                Time.time * 0.32f +
                index;

            rt.anchoredPosition =
                origin +
                new Vector2(
                    Mathf.Sin(t) * 12f,
                    Mathf.Abs(
                        Mathf.Sin(t * 0.65f)
                    ) * 28f
                );

            yield return null;
        }
    }

    // ============================================================
    // CLOUD TRANSITION
    //
    // SATU OBJECT PER BOUNDARY.
    //
    // TIDAK ADA:
    // - 7 cloud kecil
    // - smoke kecil berjejer
    // - cloud di antara level
    //
    // HANYA:
    // L6 -> CLOUD -> segment berikutnya
    // ============================================================

    private void CreateCloudTransitions()
    {
        /*
         * Kita sengaja pakai SATU sprite map_smoke
         * yang diregangkan sebagai curtain.
         *
         * Tidak membuat kumpulan child cloud.
         */
        Sprite cloud =
            TOBATheme.LoadSprite(
                "MapScreen/map_smoke"
            );

        if (cloud == null)
        {
            Debug.LogWarning(
                "[Map] map_smoke tidak ditemukan."
            );

            return;
        }

        int segmentCount =
            Mathf.CeilToInt(
                locationList.Count / 6f
            );

        if (segmentCount <= 1)
            return;

        for (
            int boundary = 1;
            boundary < segmentCount;
            boundary++
        )
        {
            CreateSingleCloudCurtain(
                cloud,
                boundary
            );
        }
    }

    private void CreateSingleCloudCurtain(
        Sprite cloud,
        int boundary)
    {
        float boundaryY =
            -(
                boundary *
                SEGMENT_HEIGHT
            );

        /*
         * Satu GameObject.
         * Satu Image.
         * Satu texture.
         */
        GameObject curtain =
            CreateWorldImage(
                "CloudCurtain_" +
                boundary,
                cloud,
                new Vector2(
                    WORLD_WIDTH + 100f,
                    300f
                ),
                new Vector2(
                    0f,
                    boundaryY
                ),
                new Color(
                    0.95f,
                    0.98f,
                    1f,
                    0.80f
                )
            );

        Image image =
            curtain.GetComponent<Image>();

        if (image != null)
        {
            image.preserveAspect =
                false;

            image.type =
                Image.Type.Simple;
        }

        /*
         * Curtain harus di atas background,
         * tapi tetap sebagai bagian dari world.
         */
        curtain.transform.SetAsLastSibling();

        ScreenManager.Instance.StartCoroutine(
            CloudCurtainLoop(
                curtain.transform,
                image
            )
        );
    }

    private IEnumerator CloudCurtainLoop(
        Transform target,
        Image image)
    {
        if (
            target == null ||
            image == null
        )
        {
            yield break;
        }

        Vector3 origin =
            target.localPosition;

        while (target != null)
        {
            float t =
                Time.time * 0.16f;

            /*
             * Drift horizontal sangat kecil.
             */
            target.localPosition =
                origin +
                new Vector3(
                    Mathf.Sin(t) * 35f,
                    Mathf.Sin(t * 0.65f) * 5f,
                    0f
                );

            /*
             * Brightness / opacity bernapas perlahan.
             */
            image.color =
                new Color(
                    0.95f,
                    0.98f,
                    1f,
                    0.62f +
                    Mathf.Abs(
                        Mathf.Sin(
                            t * 0.45f
                        )
                    ) * 0.18f
                );

            /*
             * Sangat subtle breathing.
             */
            float scale =
                0.985f +
                Mathf.Sin(
                    t * 0.35f
                ) * 0.015f;

            target.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );

            yield return null;
        }
    }

    // ============================================================
    // WORLD IMAGE HELPER
    // ============================================================

    private GameObject CreateWorldImage(
        string name,
        Sprite sprite,
        Vector2 size,
        Vector2 position,
        Color? tint = null)
    {
        GameObject obj =
            new GameObject(name);

        obj.transform.SetParent(
            worldContent,
            false
        );

        RectTransform rt =
            obj.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        rt.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        rt.sizeDelta =
            size;

        rt.anchoredPosition =
            position;

        Image image =
            obj.AddComponent<Image>();

        image.sprite =
            sprite ??
            UIBuilder.GetDefaultSprite();

        image.type =
            Image.Type.Simple;

        image.preserveAspect =
            true;

        image.color =
            tint ??
            Color.white;

        image.raycastTarget =
            false;

        return obj;
    }

    // ============================================================
    // WORLD PANEL
    // ============================================================

    private GameObject CreateWorldPanel(
        string name,
        Color color,
        Vector2 size,
        Vector2 position)
    {
        GameObject obj =
            new GameObject(name);

        obj.transform.SetParent(
            worldContent,
            false
        );

        RectTransform rt =
            obj.AddComponent<RectTransform>();

        rt.anchorMin =
            rt.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        rt.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        rt.sizeDelta =
            size;

        rt.anchoredPosition =
            position;

        Image image =
            obj.AddComponent<Image>();

        image.sprite =
            UIBuilder.GetDefaultSprite();

        image.color =
            color;

        image.raycastTarget =
            false;

        return obj;
    }

    // ============================================================
    // STATE
    // ============================================================

    private bool IsUnlocked(
        int id,
        int index)
    {
        if (index == 0)
            return true;

        foreach (
            PlayerProgressData progress
            in progressList
        )
        {
            if (
                progress.location_id == id &&
                progress.is_unlocked
            )
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCompleted(
        int id)
    {
        foreach (
            PlayerProgressData progress
            in progressList
        )
        {
            if (
                progress.location_id == id &&
                progress.is_completed
            )
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // DEBUG
    // ============================================================

    /*
     * Hapus komentar ini bila sudah tidak diperlukan.
     *
     * Fungsi ini membantu mengecek bahwa pattern yang dipakai
     * benar-benar berulang.
     */

    private void LogPatternForDebug()
    {
        for (
            int i = 0;
            i < MASTER_PATTERN.Length;
            i++
        )
        {
            Debug.Log(
                "[Map Pattern] Slot " +
                (i + 1) +
                " = " +
                MASTER_PATTERN[i]
            );
        }
    }
}