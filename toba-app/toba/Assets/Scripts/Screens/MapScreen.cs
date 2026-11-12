using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapScreen : BaseScreen
{
    // ============================================================
    // DATA
    // ============================================================

    private List<LocationData> locationList = new List<LocationData>();
    private List<PlayerProgressData> progressList = new List<PlayerProgressData>();

    // ============================================================
    // WORLD CONFIGURATION
    // ============================================================

    private const float WORLD_WIDTH = 1080f;
    private const float SEGMENT_HEIGHT = 2200f;
    private const float WORLD_TOP_PADDING = 180f;
    private const float NODE_SIZE = 150f;

    private static readonly Vector2[] MASTER_PATTERN =
    {
        new Vector2(350f, 400f),
        new Vector2(680f, 675f),
        new Vector2(350f, 950f),
        new Vector2(680f, 1225f),
        new Vector2(350f, 1500f),
        new Vector2(680f, 1775f)
    };

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
    // REFERENCES / STATE
    // ============================================================

    private RectTransform worldContent;
    private ScrollRect scrollRect;
    private bool isFetching;
    private bool isNavigating;

    public MapScreen(Transform parent)
        : base(parent, "MapScreen")
    {
    }

    // ============================================================
    // SHOW / LOADING
    // ============================================================

    public override void Show()
    {
        if (screenObj == null)
            return;

        isNavigating = false;
        SetBackground(TOBATheme.ForestGreen);
        FetchData();
    }

    private void ShowLoading()
    {
        if (screenObj == null)
            return;

        RemoveMapLoading();

        GameObject loading = new GameObject("Loading");
        loading.transform.SetParent(screenObj.transform, false);

        RectTransform rt = loading.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700f, 80f);
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = loading.AddComponent<TextMeshProUGUI>();
        text.text = "Memuat peta...";
        text.fontSize = 38f;
        text.fontStyle = FontStyles.Bold;
        text.color = TOBATheme.BatakGold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private void RemoveMapLoading()
    {
        if (screenObj == null)
            return;

        Transform loading = screenObj.transform.Find("Loading");
        if (loading != null)
            UnityEngine.Object.Destroy(loading.gameObject);
    }

    private void RemoveEmptyState()
    {
        if (screenObj == null)
            return;

        Transform oldEmptyState = screenObj.transform.Find("EmptyState");
        if (oldEmptyState != null)
            UnityEngine.Object.Destroy(oldEmptyState.gameObject);
    }

    // ============================================================
    // API
    // ============================================================

    private void FetchData()
    {
        if (screenObj == null || isFetching)
            return;

        if (APIManager.Instance == null)
        {
            ShowMapLoadError(
                "Layanan tidak tersedia",
                "Silakan coba kembali."
            );
            return;
        }

        isFetching = true;
        isNavigating = false;

        RemoveEmptyState();
        ShowLoading();

        int playerID = PlayerPrefs.GetInt("PlayerID", 0);

        APIManager.Instance.GetRequest(
            "/locations",
            locationResponse =>
            {
                if (screenObj == null)
                {
                    isFetching = false;
                    return;
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(locationResponse))
                        throw new System.FormatException("Response lokasi kosong.");

                    string json = locationResponse.Trim();

                    if (json.StartsWith("["))
                        json = "{\"locations\":" + json + "}";

                    LocationListResponse response =
                        JsonUtility.FromJson<LocationListResponse>(json);

                    if (response == null)
                        throw new System.FormatException("Struktur data lokasi tidak valid.");

                    locationList = response.locations ?? new List<LocationData>();

                    locationList.RemoveAll(location =>
                        location == null || location.id <= 0
                    );
                }
                catch (System.Exception ex)
                {
                    isFetching = false;

                    Debug.LogWarning(
                        "[Map] Gagal membaca data lokasi: " + ex.Message
                    );

                    ShowMapLoadError(
                        "Data lokasi bermasalah",
                        "Silakan muat ulang peta."
                    );
                    return;
                }

                if (locationList.Count == 0)
                {
                    isFetching = false;
                    RemoveMapLoading();
                    ClearMapVisuals();

                    EmptyState.Show(
                        screenObj.transform,
                        "◆",
                        "Belum ada lokasi tersedia",
                        "Hubungi admin untuk menambahkan lokasi"
                    );
                    return;
                }

                progressList = new List<PlayerProgressData>();

                if (playerID <= 0 || APIManager.Instance == null)
                {
                    isFetching = false;
                    BuildMap();
                    return;
                }

                APIManager.Instance.GetRequest(
                    "/player/" + playerID + "/progress",
                    progressResponse =>
                    {
                        if (screenObj == null)
                        {
                            isFetching = false;
                            return;
                        }

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(progressResponse))
                            {
                                string json = progressResponse.Trim();

                                if (json.StartsWith("["))
                                    json = "{\"progress\":" + json + "}";

                                PlayerProgressResponse response =
                                    JsonUtility.FromJson<PlayerProgressResponse>(json);

                                if (response == null)
                                    throw new System.FormatException("Struktur data progress tidak valid.");

                                progressList = response.progress ??
                                    new List<PlayerProgressData>();

                                progressList.RemoveAll(item => item == null);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning(
                                "[Map] Gagal membaca progress: " + ex.Message
                            );

                            progressList.Clear();
                        }

                        isFetching = false;
                        BuildMap();
                    },
                    error =>
                    {
                        if (screenObj == null)
                        {
                            isFetching = false;
                            return;
                        }

                        Debug.LogWarning(
                            "[Map] Gagal mengambil progress: " + error
                        );

                        progressList.Clear();
                        isFetching = false;
                        BuildMap();
                    }
                );
            },
            error =>
            {
                if (screenObj == null)
                {
                    isFetching = false;
                    return;
                }

                isFetching = false;

                Debug.LogWarning(
                    "[Map] Gagal mengambil lokasi: " + error
                );

                ShowMapLoadError(
                    "Gagal memuat peta",
                    "Periksa koneksi internet."
                );
            }
        );
    }

    private void ShowMapLoadError(string title, string message)
    {
        if (screenObj == null)
            return;

        isFetching = false;
        RemoveMapLoading();
        ClearMapVisuals();
        RemoveEmptyState();

        EmptyState.Show(
            screenObj.transform,
            "◆",
            title,
            message,
            () => FetchData()
        );
    }

    private void ClearMapVisuals()
    {
        if (screenObj == null)
            return;

        Transform oldMap = screenObj.transform.Find("MapScroll");
        if (oldMap != null)
            UnityEngine.Object.Destroy(oldMap.gameObject);

        Transform oldHeader = screenObj.transform.Find("HeaderLayer");
        if (oldHeader != null)
            UnityEngine.Object.Destroy(oldHeader.gameObject);

        worldContent = null;
        scrollRect = null;
    }

    // ============================================================
    // COROUTINE HELPER
    // ============================================================

    private void StartMapCoroutine(IEnumerator routine)
    {
        if (routine == null || screenObj == null || isNavigating)
            return;

        if (ScreenManager.Instance == null)
        {
            Debug.LogWarning("[Map] ScreenManager tidak tersedia; animasi dilewati.");
            return;
        }

        // Coroutine harus dijalankan oleh MonoBehaviour ScreenManager.
        // Jangan memanggil StartMapCoroutine(routine) dari dalam fungsi ini.
        ScreenManager.Instance.StartCoroutine(routine);
    }

    private bool IsMapRunning(Transform target)
    {
        return target != null &&
               screenObj != null &&
               screenObj.activeInHierarchy &&
               !isNavigating;
    }

    // ============================================================
    // BUILD MAP
    // ============================================================

    private void BuildMap()
    {
        if (screenObj == null)
            return;

        isFetching = false;
        RemoveMapLoading();
        RemoveEmptyState();
        ClearMapVisuals();

        CreateWorld();

        if (scrollRect == null || worldContent == null)
        {
            ShowMapLoadError(
                "Peta gagal dibuat",
                "Silakan coba muat ulang peta."
            );
            return;
        }

        CreateFixedHeader();
        CreateNodes();
    }

    // ============================================================
    // WORLD / SCROLL
    // ============================================================

    private void CreateWorld()
    {
        if (screenObj == null)
            return;

        scrollRect = UIBuilder.CreateScrollView(
            screenObj.transform,
            "MapScroll",
            Vector2.zero,
            Vector2.one
        );

        if (scrollRect == null)
        {
            Debug.LogError("[Map] ScrollView gagal dibuat.");
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 38f;

        worldContent = scrollRect.content;

        if (worldContent == null)
        {
            Debug.LogError("[Map] Scroll content null.");
            return;
        }

        // Anchor horizontal dibuat stretch mengikuti lebar viewport.
        // Karena itu sizeDelta.x = 0 berarti mengikuti lebar parent,
        // bukan membuat lebar content menjadi nol.
        worldContent.anchorMin = new Vector2(0f, 1f);
        worldContent.anchorMax = new Vector2(1f, 1f);
        worldContent.pivot = new Vector2(0.5f, 1f);
        worldContent.anchoredPosition = new Vector2(0f, -WORLD_TOP_PADDING);

        CreateSegmentBackgrounds();
        CreateEnvironmentEffects();
        SetWorldHeight();
    }

    // ============================================================
    // BACKGROUND SEGMENTS
    // ============================================================

    private void CreateSegmentBackgrounds()
    {
        if (worldContent == null || locationList == null || locationList.Count == 0)
            return;

        int segmentCount = Mathf.CeilToInt(locationList.Count / 6f);

        for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            CreateSingleSegment(segmentIndex);
    }

    private void CreateSingleSegment(int segmentIndex)
    {
        if (worldContent == null || SEGMENT_ASSETS.Length == 0)
            return;

        string assetPath = SEGMENT_ASSETS[segmentIndex % SEGMENT_ASSETS.Length];
        Sprite background = TOBATheme.LoadSprite(assetPath);

        if (background == null)
        {
            Debug.LogWarning("[Map] Background tidak ditemukan: " + assetPath);
            return;
        }

        GameObject bg = new GameObject("MapSegment_" + (segmentIndex + 1));
        bg.transform.SetParent(worldContent, false);

        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(WORLD_WIDTH, SEGMENT_HEIGHT);
        rt.anchoredPosition = new Vector2(
            0f,
            -(segmentIndex * SEGMENT_HEIGHT)
        );

        Image image = bg.AddComponent<Image>();
        image.sprite = background;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
    }

    // ============================================================
    // WORLD HEIGHT
    // ============================================================

    private void SetWorldHeight()
    {
        if (worldContent == null)
            return;

        if (locationList == null || locationList.Count == 0)
        {
            worldContent.sizeDelta = new Vector2(0f, SEGMENT_HEIGHT);
            return;
        }

        int lastIndex = locationList.Count - 1;
        int lastSegment = lastIndex / 6;
        int lastSlot = lastIndex % 6;

        float lastSlotY = MASTER_PATTERN[lastSlot].y;

        float gameplayEnd =
            lastSegment * SEGMENT_HEIGHT +
            lastSlotY +
            350f;

        float minimum = (lastSegment + 1) * SEGMENT_HEIGHT;
        float finalHeight = Mathf.Max(gameplayEnd, minimum);
        finalHeight += WORLD_TOP_PADDING;

        // Lebar tetap mengikuti viewport karena anchor horizontal stretch.
        worldContent.sizeDelta = new Vector2(0f, finalHeight);
    }

    // ============================================================
    // FIXED HEADER
    // ============================================================

    private void CreateFixedHeader()
    {
        if (screenObj == null)
            return;

        GameObject layer = new GameObject("HeaderLayer");
        layer.transform.SetParent(screenObj.transform, false);

        RectTransform layerRT = layer.AddComponent<RectTransform>();
        layerRT.anchorMin = Vector2.zero;
        layerRT.anchorMax = Vector2.one;
        layerRT.offsetMin = Vector2.zero;
        layerRT.offsetMax = Vector2.zero;

        layer.transform.SetAsLastSibling();

        CreateHeaderBoard(layer.transform);
        CreateBackButton(layer.transform);
    }

    private void CreateHeaderBoard(Transform parent)
    {
        if (parent == null)
            return;

        Sprite header = TOBATheme.LoadSprite("MapScreen/map_header_board");

        if (header == null)
        {
            Debug.LogWarning("[Map] map_header_board tidak ditemukan.");
            return;
        }

        GameObject board = UIBuilder.CreateImage(
            parent,
            "MapHeader",
            header,
            new Vector2(980f, 225f),
            new Vector2(0f, 770f)
        );

        if (board == null)
            return;

        Image image = board.GetComponent<Image>();
        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        CreateHeaderShine(board.transform);
    }

    private void CreateHeaderShine(Transform parent)
    {
        if (parent == null)
            return;

        // Shine merupakan aset dekoratif opsional. Resources.Load tidak
        // menghasilkan log error ketika aset ini memang tidak tersedia.
        Sprite shine = Resources.Load<Sprite>("MapScreen/map_header_shine");

        if (shine == null)
            return;

        GameObject shineObject = UIBuilder.CreateImage(
            parent,
            "HeaderShine",
            shine,
            new Vector2(170f, 100f),
            new Vector2(-500f, 0f),
            new Color(1f, 1f, 1f, 0.55f)
        );

        if (shineObject == null)
            return;

        Image image = shineObject.GetComponent<Image>();
        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        StartMapCoroutine(HeaderShineLoop(shineObject.transform));
    }

    private IEnumerator HeaderShineLoop(Transform target)
    {
        if (target == null)
            yield break;

        while (IsMapRunning(target))
        {
            float p = Mathf.Repeat(Time.unscaledTime * 0.17f, 1.35f);
            target.localPosition = new Vector3(-500f + p * 1000f, 0f, 0f);
            yield return null;
        }
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    private void CreateBackButton(Transform parent)
    {
        if (parent == null)
            return;

        Sprite back = TOBATheme.LoadSprite(
            "RegisterNameScreen/register_name_back_button_v2"
        );

        if (back == null)
        {
            Debug.LogWarning("[Map] Back button sprite tidak ditemukan.");
            return;
        }

        GameObject buttonObject = UIBuilder.CreateImage(
            parent,
            "BackButton",
            back,
            new Vector2(145f, 145f),
            new Vector2(-450f, 765f)
        );

        if (buttonObject == null)
            return;

        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
        {
            image.preserveAspect = true;
            image.raycastTarget = true;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
            button = buttonObject.AddComponent<Button>();

        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (isNavigating)
                return;

            StartMapCoroutine(BackButtonPress(buttonObject.transform));
        });

        StartMapCoroutine(BackButtonFloat(buttonObject.transform));
    }

    private IEnumerator BackButtonFloat(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 origin = target.localPosition;
        float time = 0f;

        while (IsMapRunning(target))
        {
            time += Time.unscaledDeltaTime;
            target.localPosition = new Vector3(
                origin.x,
                origin.y + Mathf.Sin(time * 1.1f) * 2f,
                origin.z
            );
            yield return null;
        }
    }

    private IEnumerator BackButtonPress(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original = target.localScale;
        target.localScale = original * 0.88f;
        yield return new WaitForSecondsRealtime(0.07f);

        if (target == null)
            yield break;

        target.localScale = original * 1.06f;
        yield return new WaitForSecondsRealtime(0.07f);

        if (target == null || screenObj == null || ScreenManager.Instance == null)
            yield break;

        target.localScale = original;
        isNavigating = true;

        SoundManager.Instance?.PlaySFX("sfx_click");
        ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
    }

    // ============================================================
    // NODE CREATION / POSITION
    // ============================================================

    private void CreateNodes()
    {
        if (worldContent == null || locationList == null)
            return;

        for (int i = 0; i < locationList.Count; i++)
        {
            LocationData location = locationList[i];

            if (location == null || location.id <= 0)
                continue;

            Vector2 position = GetPatternPosition(i);
            bool unlocked = IsUnlocked(location.id, i);
            bool completed = IsCompleted(location.id);

            CreateNode(location, i, position, unlocked, completed);
        }
    }

    private Vector2 GetPatternPosition(int locationIndex)
    {
        int safeIndex = Mathf.Max(0, locationIndex);
        int segmentIndex = safeIndex / MASTER_PATTERN.Length;
        int slotIndex = safeIndex % MASTER_PATTERN.Length;

        Vector2 pattern = MASTER_PATTERN[slotIndex];

        float x = pattern.x - WORLD_WIDTH / 2f;
        float y = -(segmentIndex * SEGMENT_HEIGHT + pattern.y);

        return new Vector2(x, y);
    }

    private void CreateNode(
        LocationData location,
        int index,
        Vector2 position,
        bool unlocked,
        bool completed)
    {
        if (location == null || screenObj == null || worldContent == null)
            return;

        string asset = completed
            ? "MapScreen/map_node_completed"
            : unlocked
                ? "MapScreen/map_node_active"
                : "MapScreen/map_node_locked";

        Sprite sprite = TOBATheme.LoadSprite(asset);

        if (sprite == null)
        {
            Debug.LogWarning("[Map] Node sprite tidak ditemukan: " + asset);
            return;
        }

        GameObject node = CreateWorldImage(
            "Node_" + (index + 1),
            sprite,
            new Vector2(NODE_SIZE, NODE_SIZE),
            position
        );

        if (node == null)
            return;

        Image image = node.GetComponent<Image>();
        if (image == null)
            return;

        image.raycastTarget = true;

        string number = completed ? "✓" : (index + 1).ToString();
        CreateNodeNumber(
            node.transform,
            number,
            completed ? 58 : 54,
            unlocked
        );

        Button button = node.GetComponent<Button>();
        if (button == null)
            button = node.AddComponent<Button>();

        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        if (unlocked)
        {
            LocationData capturedLocation = location;
            Transform capturedNode = node.transform;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (isNavigating)
                    return;

                SoundManager.Instance?.PlaySFX("sfx_click");
                StartMapCoroutine(
                    OpenLocationAfterTouch(capturedNode, capturedLocation)
                );
            });

            if (completed)
            {
                StartMapCoroutine(CompletedNodeLoop(node.transform));
            }
            else
            {
                CreateActiveNodeEffect(node.transform, index);
            }
        }
        else
        {
            Transform capturedNode = node.transform;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (isNavigating)
                    return;

                SoundManager.Instance?.PlaySFX("sfx_click");
                StartMapCoroutine(LockedTouch(capturedNode));
            });
        }
    }

    private void CreateNodeNumber(
        Transform parent,
        string value,
        int fontSize,
        bool unlocked)
    {
        if (parent == null)
            return;

        GameObject obj = new GameObject("LevelNumber");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(120f, 90f);
        rt.anchoredPosition = new Vector2(0f, 5f);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        text.color = unlocked
            ? new Color(1f, 0.84f, 0.34f, 1f)
            : new Color(0.76f, 0.64f, 0.35f, 0.75f);

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0.20f, 0.06f, 0.015f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        shadow.effectDistance = new Vector2(1f, -2f);
    }

    // ============================================================
    // ACTIVE / COMPLETED NODE EFFECTS
    // ============================================================

    private void CreateActiveNodeEffect(Transform node, int index)
    {
        if (node == null)
            return;

        CreateNodeGlow(node, index);
        CreateNodeSparkles(node, index);
        StartMapCoroutine(ActiveNodeLoop(node, index * 0.35f));
    }

    private IEnumerator ActiveNodeLoop(Transform target, float phase)
    {
        if (target == null)
            yield break;

        Vector3 original = target.localScale;
        float time = phase;

        while (IsMapRunning(target))
        {
            time += Time.unscaledDeltaTime * 1.25f;
            target.localScale = original *
                (1f + Mathf.Abs(Mathf.Sin(time)) * 0.035f);
            yield return null;
        }
    }

    private void CreateNodeGlow(Transform node, int index)
    {
        if (node == null || worldContent == null)
            return;

        RectTransform nodeRT = node.GetComponent<RectTransform>();
        if (nodeRT == null)
            return;

        GameObject glow = CreateWorldPanel(
            "NodeGlow_" + index,
            new Color(
                TOBATheme.BatakGold.r,
                TOBATheme.BatakGold.g,
                TOBATheme.BatakGold.b,
                0.11f
            ),
            new Vector2(NODE_SIZE + 36f, NODE_SIZE + 36f),
            nodeRT.anchoredPosition
        );

        if (glow == null)
            return;

        Image glowImage = glow.GetComponent<Image>();
        if (glowImage != null)
            glowImage.raycastTarget = false;

        glow.transform.SetSiblingIndex(
            Mathf.Max(0, node.GetSiblingIndex())
        );

        StartMapCoroutine(GlowLoop(glow.transform, glowImage));
    }

    private IEnumerator GlowLoop(Transform target, Image image)
    {
        if (target == null || image == null)
            yield break;

        float time = 0f;

        while (IsMapRunning(target) && image != null)
        {
            time += Time.unscaledDeltaTime * 1.35f;

            image.color = new Color(
                TOBATheme.BatakGold.r,
                TOBATheme.BatakGold.g,
                TOBATheme.BatakGold.b,
                0.05f + Mathf.Abs(Mathf.Sin(time)) * 0.18f
            );

            target.localScale = Vector3.one *
                (1f + Mathf.Abs(Mathf.Sin(time)) * 0.045f);

            yield return null;
        }
    }

    private void CreateNodeSparkles(Transform node, int nodeIndex)
    {
        if (node == null || worldContent == null)
            return;

        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_path_glint");
        if (sprite == null)
            return;

        RectTransform nodeRT = node.GetComponent<RectTransform>();
        if (nodeRT == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            float angle = i * Mathf.PI * 0.666f;

            GameObject sparkle = CreateWorldImage(
                "NodeSpark_" + nodeIndex + "_" + i,
                sprite,
                new Vector2(20f, 20f),
                nodeRT.anchoredPosition + new Vector2(
                    Mathf.Cos(angle) * 65f,
                    Mathf.Sin(angle) * 65f
                ),
                new Color(1f, 1f, 1f, 0.80f)
            );

            if (sparkle == null)
                continue;

            StartMapCoroutine(
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

        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt == null)
            yield break;

        while (IsMapRunning(target))
        {
            float time = Time.unscaledTime * 0.82f + phase;

            rt.anchoredPosition = center + new Vector2(
                Mathf.Cos(time) * 65f,
                Mathf.Sin(time * 1.2f) * 65f
            );

            target.localScale = Vector3.one *
                (0.65f + Mathf.Abs(Mathf.Sin(time)) * 0.30f);

            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(
                    1f,
                    1f,
                    1f,
                    0.10f + Mathf.Abs(Mathf.Sin(time)) * 0.80f
                );
            }

            yield return null;
        }
    }

    private IEnumerator CompletedNodeLoop(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original = target.localScale;
        float time = 0f;

        while (IsMapRunning(target))
        {
            time += Time.unscaledDeltaTime * 0.75f;
            target.localScale = original *
                (1f + Mathf.Abs(Mathf.Sin(time)) * 0.02f);
            yield return null;
        }
    }

    // ============================================================
    // NODE TOUCH / NAVIGATION
    // ============================================================

    private IEnumerator OpenLocationAfterTouch(
        Transform target,
        LocationData location)
    {
        yield return NodeTouch(target);

        if (screenObj == null ||
            location == null ||
            ScreenManager.Instance == null ||
            isNavigating)
        {
            yield break;
        }

        isNavigating = true;
        ScreenManager.Instance.ChangeScreen(
            new LocationDetailScreen(container, location)
        );
    }

    private IEnumerator NodeTouch(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 original = target.localScale;
        target.localScale = original * 0.88f;

        yield return new WaitForSecondsRealtime(0.06f);

        if (target == null)
            yield break;

        target.localScale = original * 1.07f;

        yield return new WaitForSecondsRealtime(0.08f);

        if (target != null)
            target.localScale = original;
    }

    private IEnumerator LockedTouch(Transform target)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < 0.20f && IsMapRunning(target))
        {
            elapsed += Time.unscaledDeltaTime;
            target.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Sin(elapsed * 80f) * 6f
            );

            yield return null;
        }

        if (target != null)
            target.localRotation = Quaternion.identity;
    }

    // ============================================================
    // ENVIRONMENT EFFECTS
    // ============================================================

    private int GetSegmentCount()
    {
        if (locationList == null || locationList.Count == 0)
            return 0;

        return Mathf.CeilToInt(locationList.Count / 6f);
    }

    private void CreateEnvironmentEffects()
    {
        if (worldContent == null)
            return;

        CreateFireflies();
        CreateWaterShimmer();
        CreateWaterfallSpray();
        CreateForegroundLeaves();
        CreateHouseSmoke();
        CreateCloudTransitions();
    }

    // ============================================================
    // FIREFLIES
    // ============================================================

    private void CreateFireflies()
    {
        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_firefly");
        if (sprite == null)
            return;

        int segmentCount = GetSegmentCount();
        int total = segmentCount * 6;

        for (int i = 0; i < total; i++)
        {
            int segment = i / 6;
            float x = UnityEngine.Random.Range(-440f, 440f);

            if (x > -120f && x < 120f)
                x = x < 0f ? -170f : 170f;

            float y = UnityEngine.Random.Range(300f, 1880f);

            GameObject firefly = CreateWorldImage(
                "Firefly_" + i,
                sprite,
                new Vector2(34f, 34f),
                new Vector2(
                    x,
                    -(segment * SEGMENT_HEIGHT + y)
                ),
                new Color(1f, 0.98f, 0.42f, 0.78f)
            );

            if (firefly == null)
                continue;

            Image image = firefly.GetComponent<Image>();

            StartMapCoroutine(
                FireflyLoop(
                    firefly.transform,
                    image,
                    UnityEngine.Random.Range(0f, 10f)
                )
            );
        }
    }

    private IEnumerator FireflyLoop(
        Transform target,
        Image image,
        float phase)
    {
        if (target == null || image == null)
            yield break;

        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt == null)
            yield break;

        Vector2 origin = rt.anchoredPosition;

        while (IsMapRunning(target) && image != null)
        {
            float time = Time.unscaledTime * 0.72f + phase;

            rt.anchoredPosition = origin + new Vector2(
                Mathf.Sin(time * 0.65f) * 20f,
                Mathf.Cos(time * 0.48f) * 15f
            );

            image.color = new Color(
                1f,
                0.98f,
                0.42f,
                0.15f + Mathf.Abs(Mathf.Sin(time * 1.45f)) * 0.80f
            );

            yield return null;
        }
    }

    // ============================================================
    // WATER SHIMMER
    // ============================================================

    private void CreateWaterShimmer()
    {
        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_water_shimmer");
        if (sprite == null)
            return;

        int segmentCount = GetSegmentCount();

        for (int segment = 0; segment < segmentCount; segment++)
        {
            for (int i = 0; i < 3; i++)
            {
                float y = 320f + i * 560f;

                GameObject shimmer = CreateWorldImage(
                    "WaterShimmer_" + segment + "_" + i,
                    sprite,
                    new Vector2(190f, 62f),
                    new Vector2(
                        405f,
                        -(segment * SEGMENT_HEIGHT + y)
                    ),
                    new Color(1f, 1f, 1f, 0.20f)
                );

                if (shimmer == null)
                    continue;

                StartMapCoroutine(
                    WaterShimmerLoop(shimmer.transform, i)
                );
            }
        }
    }

    private IEnumerator WaterShimmerLoop(Transform target, int index)
    {
        if (target == null)
            yield break;

        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt == null)
            yield break;

        Vector2 origin = rt.anchoredPosition;

        while (IsMapRunning(target))
        {
            float time = Time.unscaledTime * 0.50f + index;

            rt.anchoredPosition = origin + new Vector2(
                Mathf.Sin(time) * 28f,
                Mathf.Sin(time * 0.75f) * 5f
            );

            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(
                    1f,
                    1f,
                    1f,
                    0.08f + Mathf.Abs(Mathf.Sin(time * 1.30f)) * 0.30f
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
        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_waterfall_spray");
        if (sprite == null)
            return;

        int segmentCount = GetSegmentCount();

        for (int segment = 0; segment < segmentCount; segment++)
        {
            GameObject spray = CreateWorldImage(
                "WaterfallSpray_" + segment,
                sprite,
                new Vector2(90f, 90f),
                new Vector2(
                    410f,
                    -(segment * SEGMENT_HEIGHT + 480f)
                ),
                new Color(1f, 1f, 1f, 0.24f)
            );

            if (spray == null)
                continue;

            StartMapCoroutine(
                WaterfallSprayLoop(spray.transform, segment)
            );
        }
    }

    private IEnumerator WaterfallSprayLoop(Transform target, int index)
    {
        if (target == null)
            yield break;

        Vector3 origin = target.localPosition;

        while (IsMapRunning(target))
        {
            float time = Time.unscaledTime * 1.10f + index * 0.8f;

            target.localPosition = origin + new Vector3(
                Mathf.Sin(time) * 6f,
                Mathf.Cos(time * 1.3f) * 4f,
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
        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_leaf_sway");
        if (sprite == null)
            return;

        int segmentCount = GetSegmentCount();

        for (int segment = 0; segment < segmentCount; segment++)
        {
            for (int i = 0; i < 3; i++)
            {
                float x = i % 2 == 0 ? -465f : 455f;
                float y = 600f + i * 560f;

                GameObject leaves = CreateWorldImage(
                    "LeafSway_" + segment + "_" + i,
                    sprite,
                    new Vector2(125f, 80f),
                    new Vector2(
                        x,
                        -(segment * SEGMENT_HEIGHT + y)
                    )
                );

                if (leaves == null)
                    continue;

                StartMapCoroutine(
                    LeafSwayLoop(leaves.transform, i)
                );
            }
        }
    }

    private IEnumerator LeafSwayLoop(Transform target, int index)
    {
        if (target == null)
            yield break;

        while (IsMapRunning(target))
        {
            float time = Time.unscaledTime * 0.72f + index * 0.8f;

            target.localEulerAngles = new Vector3(
                0f,
                0f,
                Mathf.Sin(time) * 4f
            );

            yield return null;
        }
    }

    // ============================================================
    // HOUSE SMOKE
    // ============================================================

    private void CreateHouseSmoke()
    {
        Sprite sprite = TOBATheme.LoadSprite("MapScreen/map_smoke");
        if (sprite == null)
            return;

        int segmentCount = GetSegmentCount();

        for (int segment = 0; segment < segmentCount; segment++)
        {
            GameObject smoke = CreateWorldImage(
                "HouseSmoke_" + segment,
                sprite,
                new Vector2(44f, 66f),
                new Vector2(
                    -360f,
                    -(segment * SEGMENT_HEIGHT + 300f)
                ),
                new Color(1f, 1f, 1f, 0.05f)
            );

            if (smoke == null)
                continue;

            StartMapCoroutine(
                SmokeLoop(smoke.transform, segment)
            );
        }
    }

    private IEnumerator SmokeLoop(Transform target, int index)
    {
        if (target == null)
            yield break;

        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt == null)
            yield break;

        Vector2 origin = rt.anchoredPosition;

        while (IsMapRunning(target))
        {
            float time = Time.unscaledTime * 0.32f + index;

            rt.anchoredPosition = origin + new Vector2(
                Mathf.Sin(time) * 12f,
                Mathf.Abs(Mathf.Sin(time * 0.65f)) * 28f
            );

            yield return null;
        }
    }

    // ============================================================
    // CLOUD TRANSITIONS
    // ============================================================

    private void CreateCloudTransitions()
    {
        Sprite cloud = TOBATheme.LoadSprite("MapScreen/map_smoke");
        if (cloud == null)
            return;

        int segmentCount = GetSegmentCount();
        if (segmentCount <= 1)
            return;

        for (int boundary = 1; boundary < segmentCount; boundary++)
            CreateSingleCloudCurtain(cloud, boundary);
    }

    private void CreateSingleCloudCurtain(Sprite cloud, int boundary)
    {
        if (cloud == null || worldContent == null)
            return;

        float boundaryY = -(boundary * SEGMENT_HEIGHT);

        GameObject curtain = CreateWorldImage(
            "CloudCurtain_" + boundary,
            cloud,
            new Vector2(WORLD_WIDTH + 100f, 300f),
            new Vector2(0f, boundaryY),
            new Color(0.95f, 0.98f, 1f, 0.80f)
        );

        if (curtain == null)
            return;

        Image image = curtain.GetComponent<Image>();
        if (image != null)
        {
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }

        curtain.transform.SetAsLastSibling();
        StartMapCoroutine(
            CloudCurtainLoop(curtain.transform, image)
        );
    }

    private IEnumerator CloudCurtainLoop(Transform target, Image image)
    {
        if (target == null || image == null)
            yield break;

        Vector3 origin = target.localPosition;

        while (IsMapRunning(target) && image != null)
        {
            float time = Time.unscaledTime * 0.16f;

            target.localPosition = origin + new Vector3(
                Mathf.Sin(time) * 35f,
                Mathf.Sin(time * 0.65f) * 5f,
                0f
            );

            image.color = new Color(
                0.95f,
                0.98f,
                1f,
                0.62f + Mathf.Abs(Mathf.Sin(time * 0.45f)) * 0.18f
            );

            float scale = 0.985f + Mathf.Sin(time * 0.35f) * 0.015f;
            target.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }
    }

    // ============================================================
    // WORLD IMAGE / PANEL HELPERS
    // ============================================================

    private GameObject CreateWorldImage(
        string objectName,
        Sprite sprite,
        Vector2 size,
        Vector2 position,
        Color? tint = null)
    {
        if (worldContent == null)
            return null;

        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(worldContent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite != null ? sprite : UIBuilder.GetDefaultSprite();
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = tint ?? Color.white;
        image.raycastTarget = false;

        return obj;
    }

    private GameObject CreateWorldPanel(
        string objectName,
        Color color,
        Vector2 size,
        Vector2 position)
    {
        if (worldContent == null)
            return null;

        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(worldContent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        Image image = obj.AddComponent<Image>();
        image.sprite = UIBuilder.GetDefaultSprite();
        image.color = color;
        image.raycastTarget = false;

        return obj;
    }

    // ============================================================
    // LOCATION STATE
    // ============================================================

    private bool IsUnlocked(int id, int index)
    {
        if (index == 0)
            return true;

        if (progressList == null)
            return false;

        foreach (PlayerProgressData progress in progressList)
        {
            if (progress == null)
                continue;

            if (progress.location_id == id && progress.is_unlocked)
                return true;
        }

        return false;
    }

    private bool IsCompleted(int id)
    {
        if (progressList == null)
            return false;

        foreach (PlayerProgressData progress in progressList)
        {
            if (progress == null)
                continue;

            if (progress.location_id == id && progress.is_completed)
                return true;
        }

        return false;
    }
}
