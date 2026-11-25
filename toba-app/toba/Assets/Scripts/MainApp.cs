using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainApp : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform safeAreaRoot;

    private const float OFFLINE_QUEUE_INITIAL_DELAY = 3f;
    private const float OFFLINE_QUEUE_WAIT_INTERVAL = 3f;
    private const float OFFLINE_QUEUE_RETRY_DELAY = 5f;
    private const float OFFLINE_QUEUE_AUTH_RETRY_DELAY = 30f;
    private const string OFFLINE_QUEUE_KEY = "OfflineSubmitQueueV2";
    private const int OFFLINE_QUEUE_MAX_ITEMS = 5;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;

        SetupCanvas();
        SetupManagers();
    }

    private void Start()
    {
        if (!ValidateStartup())
            return;

        // ScreenManager menggunakan SafeArea sebagai parent screen.
        ScreenManager.Instance.canvasTransform = safeAreaRoot;

        // Musik latar tetap dijalankan seperti alur sebelumnya.
        SoundManager.Instance?.PlayBGM("bgm_main");

        // Mulai pemeriksaan koneksi sebelum memproses antrean offline.
        NetworkManager.Instance.Init(
            canvas.transform,
            APIManager.Instance.GetBaseUrl()
        );

        StartCoroutine(ProcessOfflineQueue());

        // Firebase berjalan secara asynchronous dan tidak boleh
        // menghambat navigasi awal aplikasi.
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.Initialize();

#if FIREBASE_CRASHLYTICS
            // Aktifkan define FIREBASE_CRASHLYTICS hanya jika
            // Firebase Crashlytics Unity package memang terpasang.
            StartCoroutine(InitializeCrashlyticsWhenReady());
#endif
        }
        else
        {
            Debug.LogWarning(
                "[MainApp] FirebaseManager tidak tersedia. " +
                "Push notification dan Crashlytics tidak diinisialisasi."
            );
        }

        OpenInitialScreen();
    }

    // ============================================================
    // STARTUP VALIDATION
    // ============================================================

    private bool ValidateStartup()
    {
        if (canvas == null)
        {
            Debug.LogError(
                "[MainApp] MainCanvas gagal dibuat."
            );
            return false;
        }

        if (safeAreaRoot == null)
        {
            Debug.LogError(
                "[MainApp] SafeArea gagal dibuat."
            );
            return false;
        }

        if (ScreenManager.Instance == null)
        {
            Debug.LogError(
                "[MainApp] ScreenManager tidak tersedia."
            );
            return false;
        }

        if (APIManager.Instance == null)
        {
            Debug.LogError(
                "[MainApp] APIManager tidak tersedia."
            );
            return false;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogError(
                "[MainApp] NetworkManager tidak tersedia."
            );
            return false;
        }

        return true;
    }

    // ============================================================
    // INITIAL SCREEN
    // ============================================================

    private void OpenInitialScreen()
    {
        if (ScreenManager.Instance == null || safeAreaRoot == null)
            return;


        // RESET SESI LAMA SEKALI SAJA
        if (PlayerPrefs.GetInt("TOBA_InitialSessionReset_v1", 0) == 0)
        {
            PlayerPrefs.DeleteKey("UserToken");
            PlayerPrefs.DeleteKey("SavedUsername");
            PlayerPrefs.DeleteKey("PlayerID");

            PlayerPrefs.SetInt("TOBA_InitialSessionReset_v1", 1);
            PlayerPrefs.Save();
        }

        bool hasSession =
            PlayerPrefs.HasKey("UserToken") &&
            PlayerPrefs.HasKey("SavedUsername");

        bool doneOnboard =
            PlayerPrefs.GetInt("OnboardingDone", 0) == 1;

        if (hasSession)
        {
            ScreenManager.Instance.ChangeScreen(
                new MainMenuScreen(safeAreaRoot)
            );
        }
        else if (!doneOnboard)
        {
            ScreenManager.Instance.ChangeScreen(
                new OnboardingScreen(safeAreaRoot)
            );
        }
        else
        {
            ScreenManager.Instance.ChangeScreen(
                new SplashScreen(safeAreaRoot)
            );
        }
    }

    // ============================================================
    // OFFLINE SUBMISSION QUEUE
    // Queue hanya menyimpan payload final yang sudah memiliki session_id
    // dari backend. Endpoint finalisasi Jelajah, Trivia, dan Challenge
    // menyimpan hasil idempotent sehingga retry tidak menggandakan reward.
    // ============================================================

    public static bool QueueOfflineSubmission(string endpoint, string payload)
    {
        int playerID = PlayerPrefs.GetInt("PlayerID", 0);
        if (playerID <= 0 || string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(payload) || payload.Length > 256 * 1024)
            return false;

        OfflineSubmissionQueueData queue = LoadOfflineSubmissionQueue();
        if (queue.items == null)
            queue.items = new List<OfflineSubmissionItem>();

        for (int i = 0; i < queue.items.Count; i++)
        {
            OfflineSubmissionItem existing = queue.items[i];
            if (existing != null &&
                existing.endpoint == endpoint &&
                existing.payload == payload)
            {
                return true;
            }
        }

        if (queue.items.Count >= OFFLINE_QUEUE_MAX_ITEMS)
        {
            Debug.LogWarning("[MainApp] Antrean offline penuh; hasil baru tidak ditambahkan.");
            return false;
        }

        queue.items.Add(new OfflineSubmissionItem
        {
            player_id = playerID,
            endpoint = endpoint,
            payload = payload,
            queued_at = System.DateTime.UtcNow.ToString("o")
        });

        SaveOfflineSubmissionQueue(queue);
        Debug.Log("[MainApp] Hasil ditambahkan ke antrean offline: " + endpoint);
        return true;
    }

    public static void RemoveOfflineSubmission(string endpoint, string payload)
    {
        OfflineSubmissionQueueData queue = LoadOfflineSubmissionQueue();
        if (queue.items == null)
            queue.items = new List<OfflineSubmissionItem>();

        queue.items.RemoveAll(item => item != null &&
            item.endpoint == endpoint && item.payload == payload);

        SaveOfflineSubmissionQueue(queue);

        // Hapus format antrean tunggal versi lama jika payload-nya sama.
        if (PlayerPrefs.GetString("OfflineSubmitQueue", "") == payload &&
            PlayerPrefs.GetString("OfflineSubmitEndpoint", "") == endpoint)
        {
            PlayerPrefs.DeleteKey("OfflineSubmitQueue");
            PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
            PlayerPrefs.Save();
        }
    }

    public static bool IsRetryableSubmissionError(string error)
    {
        if (NetworkManager.Instance != null && !NetworkManager.Instance.IsOnline)
            return true;

        string value = (error ?? string.Empty).ToLowerInvariant();
        return value.Contains("koneksi") ||
               value.Contains("terhubung") ||
               value.Contains("timeout") ||
               value.Contains("timed out") ||
               value.Contains("server sedang mengalami gangguan") ||
               value.Contains("terlalu banyak permintaan") ||
               value.Contains("gagal dimulai");
    }

    private static OfflineSubmissionQueueData LoadOfflineSubmissionQueue()
    {
        OfflineSubmissionQueueData queue = null;
        string json = PlayerPrefs.GetString(OFFLINE_QUEUE_KEY, string.Empty);

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                queue = JsonUtility.FromJson<OfflineSubmissionQueueData>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[MainApp] Antrean offline tidak dapat dibaca: " + ex.Message);
            }
        }

        if (queue == null || queue.items == null)
            queue = new OfflineSubmissionQueueData();

        // Migrasi satu hasil dari versi lama ke antrean multi-item.
        string legacyPayload = PlayerPrefs.GetString("OfflineSubmitQueue", string.Empty);
        string legacyEndpoint = PlayerPrefs.GetString("OfflineSubmitEndpoint", string.Empty);
        if (!string.IsNullOrWhiteSpace(legacyPayload) &&
            !string.IsNullOrWhiteSpace(legacyEndpoint))
        {
            bool exists = false;
            for (int i = 0; i < queue.items.Count; i++)
            {
                OfflineSubmissionItem item = queue.items[i];
                if (item != null && item.endpoint == legacyEndpoint && item.payload == legacyPayload)
                {
                    exists = true;
                    break;
                }
            }

            bool migrated = exists;
            if (!exists && queue.items.Count < OFFLINE_QUEUE_MAX_ITEMS)
            {
                queue.items.Add(new OfflineSubmissionItem
                {
                    player_id = PlayerPrefs.GetInt("PlayerID", 0),
                    endpoint = legacyEndpoint,
                    payload = legacyPayload,
                    queued_at = System.DateTime.UtcNow.ToString("o")
                });
                migrated = true;
            }

            // Jika antrean baru penuh, jangan hapus payload lama yang belum termigrasi.
            if (migrated)
            {
                PlayerPrefs.DeleteKey("OfflineSubmitQueue");
                PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
                SaveOfflineSubmissionQueue(queue);
            }
        }

        return queue;
    }

    private static void SaveOfflineSubmissionQueue(OfflineSubmissionQueueData queue)
    {
        if (queue == null || queue.items == null || queue.items.Count == 0)
        {
            PlayerPrefs.DeleteKey(OFFLINE_QUEUE_KEY);
        }
        else
        {
            PlayerPrefs.SetString(OFFLINE_QUEUE_KEY, JsonUtility.ToJson(queue));
        }
        PlayerPrefs.Save();
    }

    private static bool IsAuthenticationSubmissionError(string error)
    {
        string value = (error ?? string.Empty).ToLowerInvariant();
        return value.Contains("sesi pemain tidak valid") ||
               value.Contains("token tidak valid") ||
               value.Contains("sesi login") ||
               value.Contains("login kembali");
    }

    private static bool IsPermanentSubmissionError(string error)
    {
        string value = (error ?? string.Empty).ToLowerInvariant();
        return value.Contains("kedaluwarsa") ||
               value.Contains("tidak ditemukan") ||
               value.Contains("bukan milik") ||
               value.Contains("tidak valid") ||
               value.Contains("tidak aktif") ||
               value.Contains("tidak lengkap") ||
               value.Contains("sudah pernah") ||
               value.Contains("wajib diisi") ||
               value.Contains("format jawaban") ||
               value.Contains("harus dijawab") ||
               value.Contains("harus dikirim") ||
               value.Contains("tidak dikenal") ||
               value.Contains("duplikat") ||
               value.Contains("batas ukuran") ||
               value.Contains("tidak memiliki izin") ||
               value.Contains("input tidak valid") ||
               value.Contains("tidak sesuai") ||
               value.Contains("tidak diperbolehkan") ||
               value.Contains("format tidak valid");
    }

    private static void ApplyOfflineSubmissionResponse(string endpoint, string response)
    {
        int level = 0;

        try
        {
            if (endpoint == "/locations/submit")
            {
                SubmitQuizResponse result = JsonUtility.FromJson<SubmitQuizResponse>(response);
                if (result != null)
                    level = result.level;
            }
            else if (endpoint == "/trivia/submit")
            {
                SubmitTriviaResponse result = JsonUtility.FromJson<SubmitTriviaResponse>(response);
                if (result != null)
                    level = result.level;
            }
            else if (endpoint == "/challenge/submit")
            {
                ChallengeSubmitResponse result = JsonUtility.FromJson<ChallengeSubmitResponse>(response);
                if (result != null)
                    level = result.level;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[MainApp] Respons sinkronisasi tidak dapat dibaca: " + ex.Message);
        }

        if (level > 0)
        {
            PlayerPrefs.SetInt("PlayerLevel", level);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator ProcessOfflineQueue()
    {
        yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_INITIAL_DELAY);

        // Loop tetap aktif agar hasil yang baru masuk saat aplikasi berjalan
        // juga terkirim otomatis ketika koneksi kembali pulih.
        while (true)
        {
            if (NetworkManager.Instance == null || APIManager.Instance == null ||
                !NetworkManager.Instance.IsOnline)
            {
                yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_WAIT_INTERVAL);
                continue;
            }

            OfflineSubmissionQueueData queue = LoadOfflineSubmissionQueue();
            if (queue.items == null || queue.items.Count == 0)
            {
                yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_WAIT_INTERVAL);
                continue;
            }

            int currentPlayerID = PlayerPrefs.GetInt("PlayerID", 0);
            string currentToken = PlayerPrefs.GetString("UserToken", string.Empty);
            if (currentPlayerID <= 0 || string.IsNullOrWhiteSpace(currentToken))
            {
                yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_WAIT_INTERVAL);
                continue;
            }

            // Pilih antrean milik akun yang sedang login. Item milik akun lain
            // tetap tersimpan dan tidak menghalangi sinkronisasi akun ini.
            int pendingIndex = queue.items.FindIndex(item =>
                item != null && item.player_id == currentPlayerID);
            if (pendingIndex < 0)
            {
                yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_WAIT_INTERVAL);
                continue;
            }

            OfflineSubmissionItem pending = queue.items[pendingIndex];
            if (pending == null || string.IsNullOrWhiteSpace(pending.endpoint) ||
                string.IsNullOrWhiteSpace(pending.payload))
            {
                queue.items.RemoveAt(pendingIndex);
                SaveOfflineSubmissionQueue(queue);
                continue;
            }

            bool requestFinished = false;
            bool requestSucceeded = false;
            string responseBody = string.Empty;
            string requestError = string.Empty;

            APIManager.Instance.PostRequest(
                pending.endpoint,
                pending.payload,
                response =>
                {
                    responseBody = response;
                    requestSucceeded = true;
                    requestFinished = true;
                },
                error =>
                {
                    requestError = error;
                    requestFinished = true;
                }
            );

            while (!requestFinished)
                yield return null;

            if (requestSucceeded)
            {
                ApplyOfflineSubmissionResponse(pending.endpoint, responseBody);
                RemoveOfflineSubmission(pending.endpoint, pending.payload);
                NetworkManager.Instance?.ShowToast(
                    "Hasil permainan berhasil disinkronkan!",
                    false
                );
                Debug.Log("[MainApp] Antrean berhasil disinkronkan: " + pending.endpoint);
                continue;
            }

            Debug.LogWarning("[MainApp] Sinkronisasi gagal: " + requestError);

            if (IsAuthenticationSubmissionError(requestError))
            {
                NetworkManager.Instance?.ShowToast(
                    "Sesi login perlu diperbarui. Login kembali dengan akun yang sama untuk menyinkronkan hasil.",
                    true
                );
                yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_AUTH_RETRY_DELAY);
                continue;
            }

            if (IsPermanentSubmissionError(requestError))
            {
                RemoveOfflineSubmission(pending.endpoint, pending.payload);
                NetworkManager.Instance?.ShowToast(
                    "Hasil offline tidak dapat diproses: " + requestError,
                    true
                );
                continue;
            }

            yield return new WaitForSecondsRealtime(OFFLINE_QUEUE_RETRY_DELAY);
        }
    }

    // ============================================================
    // FIREBASE CRASHLYTICS
    // Opsional: aktifkan dengan scripting define
    // FIREBASE_CRASHLYTICS setelah package Crashlytics dipastikan ada.
    // ============================================================

#if FIREBASE_CRASHLYTICS
    private IEnumerator InitializeCrashlyticsWhenReady()
    {
        const float readyTimeout = 35f;
        float elapsed = 0f;

        while (!FirebaseManager.IsReady && elapsed < readyTimeout)
        {
            if (this == null || FirebaseManager.Instance == null)
            {
                Debug.LogWarning(
                    "[MainApp] FirebaseManager berhenti sebelum siap. " +
                    "Crashlytics dilewati."
                );
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!FirebaseManager.IsReady)
        {
            Debug.LogWarning(
                "[MainApp] Firebase belum siap setelah batas waktu. " +
                "Crashlytics dilewati."
            );
            yield break;
        }

        bool failed = false;
        string errorMessage = null;

        try
        {
            Firebase.FirebaseApp app =
                Firebase.FirebaseApp.DefaultInstance;

            if (app == null)
            {
                failed = true;
                errorMessage =
                    "FirebaseApp.DefaultInstance bernilai null.";
            }
            else
            {
                Firebase.Crashlytics.Crashlytics
                    .ReportUncaughtExceptionsAsFatal = true;
            }
        }
        catch (System.Exception ex)
        {
            failed = true;
            errorMessage = ex.Message;
        }

        if (failed)
        {
            Debug.LogWarning(
                "[MainApp] Crashlytics gagal diinisialisasi: " +
                errorMessage
            );
            yield break;
        }

        Debug.Log(
            "[MainApp] Firebase Crashlytics berhasil diinisialisasi."
        );
    }
#endif

    // ============================================================
    // MANAGERS
    // ============================================================

    private void SetupManagers()
    {
        GameObject core = GameObject.Find("TOBA_CORE");

        if (core == null)
        {
            core = new GameObject("TOBA_CORE");
        }

        // Pastikan juga berlaku jika TOBA_CORE sudah ada di scene.
        DontDestroyOnLoad(core);

        EnsureManager<ScreenManager>(core);
        EnsureManager<APIManager>(core);
        EnsureManager<SoundManager>(core);
        EnsureManager<HapticManager>(core);
        EnsureManager<NetworkManager>(core);
        EnsureManager<FirebaseManager>(core);
        EnsureManager<BackHandler>(core);
    }

    private T EnsureManager<T>(GameObject parent)
        where T : Component
    {
        if (parent == null)
            return null;

        T component = parent.GetComponent<T>();

        if (component == null)
            component = parent.AddComponent<T>();

        return component;
    }

    // ============================================================
    // CANVAS
    // Tampilan, ukuran, warna, dan struktur visual dipertahankan.
    // ============================================================

    private void SetupCanvas()
    {
        GameObject canvasObject = new GameObject(
            "MainCanvas",
            typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster)
        );

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        UnityEngine.UI.CanvasScaler scaler =
            canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();

        scaler.uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        // --------------------------------------------------------
        // LAYER 1: FULL SCREEN BACKGROUND
        // --------------------------------------------------------

        GameObject background = new GameObject(
            "FullScreenBackground",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image)
        );

        background.transform.SetParent(
            canvasObject.transform,
            false
        );

        RectTransform backgroundRect =
            background.GetComponent<RectTransform>();

        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image backgroundImage =
            background.GetComponent<UnityEngine.UI.Image>();

        // Default sprite mencegah Image dibuat tanpa sprite.
        backgroundImage.sprite = UIBuilder.GetDefaultSprite();
        backgroundImage.color = TOBATheme.ForestGreen;
        backgroundImage.raycastTarget = false;

        // --------------------------------------------------------
        // LAYER 2: SAFE AREA
        // --------------------------------------------------------

        GameObject safeAreaObject = new GameObject(
            "SafeArea",
            typeof(RectTransform)
        );

        safeAreaObject.transform.SetParent(
            canvasObject.transform,
            false
        );

        safeAreaRoot =
            safeAreaObject.GetComponent<RectTransform>();

        safeAreaObject.AddComponent<SafeArea>();

        // --------------------------------------------------------
        // EVENT SYSTEM
        // --------------------------------------------------------

        if (UnityEngine.Object.FindFirstObjectByType<
            UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule)
            );

            DontDestroyOnLoad(eventSystemObject);
        }
    }
}
