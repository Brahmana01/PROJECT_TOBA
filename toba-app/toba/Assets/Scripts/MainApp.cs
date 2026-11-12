using UnityEngine;
using System.Collections;

public class MainApp : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform safeAreaRoot;

    private const float OFFLINE_QUEUE_INITIAL_DELAY = 6f;
    private const float OFFLINE_QUEUE_WAIT_INTERVAL = 3f;

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
    // ============================================================

    private IEnumerator ProcessOfflineQueue()
    {
        // Beri waktu NetworkManager menyelesaikan pemeriksaan awal.
        yield return new WaitForSecondsRealtime(
            OFFLINE_QUEUE_INITIAL_DELAY
        );

        string pendingData =
            PlayerPrefs.GetString("OfflineSubmitQueue", "");

        string pendingEndpoint =
            PlayerPrefs.GetString("OfflineSubmitEndpoint", "");

        if (string.IsNullOrWhiteSpace(pendingData) ||
            string.IsNullOrWhiteSpace(pendingEndpoint))
        {
            yield break;
        }

        // Jika koneksi sedang offline, tunggu sampai NetworkManager
        // menyatakan backend dapat dijangkau.
        while (NetworkManager.Instance != null &&
               !NetworkManager.Instance.IsOnline)
        {
            yield return new WaitForSecondsRealtime(
                OFFLINE_QUEUE_WAIT_INTERVAL
            );
        }

        if (NetworkManager.Instance == null ||
            APIManager.Instance == null)
        {
            Debug.LogWarning(
                "[MainApp] Antrean offline belum dapat dikirim. " +
                "Data tetap disimpan di perangkat."
            );
            yield break;
        }

        APIManager.Instance.PostRequest(
            pendingEndpoint,
            pendingData,
            response =>
            {
                // Hapus antrean hanya setelah backend mengonfirmasi sukses.
                PlayerPrefs.DeleteKey("OfflineSubmitQueue");
                PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
                PlayerPrefs.Save();

                NetworkManager.Instance?.ShowToast(
                    "Hasil offline berhasil disinkronkan!",
                    false
                );

                Debug.Log(
                    "[MainApp] Antrean offline berhasil disinkronkan."
                );
            },
            error =>
            {
                // Jangan hapus data jika request gagal.
                // Antrean dapat dicoba lagi saat aplikasi dibuka berikutnya.
                Debug.LogWarning(
                    "[MainApp] Sinkronisasi antrean offline gagal. " +
                    "Data tetap disimpan."
                );
            }
        );
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
