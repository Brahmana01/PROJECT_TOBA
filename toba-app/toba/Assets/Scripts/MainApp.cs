using UnityEngine;
using System.Collections;

public class MainApp : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
        SetupCanvas();
        SetupManagers();
    }

    void Start()
    {

        // Cek antrian offline saat online kembali
        StartCoroutine(ProcessOfflineQueue());

        IEnumerator ProcessOfflineQueue()
        {
            yield return new WaitForSecondsRealtime(5f);

            string pendingData     = PlayerPrefs.GetString("OfflineSubmitQueue", "");
            string pendingEndpoint = PlayerPrefs.GetString("OfflineSubmitEndpoint", "");

            if (!string.IsNullOrEmpty(pendingData) && NetworkManager.Instance.IsOnline)
            {
                APIManager.Instance.PostRequest(pendingEndpoint, pendingData,
                    (r) => {
                        PlayerPrefs.DeleteKey("OfflineSubmitQueue");
                        PlayerPrefs.DeleteKey("OfflineSubmitEndpoint");
                        NetworkManager.Instance?.ShowToast(
                            "Hasil offline berhasil disinkronkan!", false);
                    },
                    (e) => { /* coba lagi nanti */ });
            }
        }

        ScreenManager.Instance.canvasTransform = canvas.transform;
        SoundManager.Instance.PlayBGM("bgm_main");

        NetworkManager.Instance.Init(
            canvas.transform,
            APIManager.Instance.GetBaseUrl());

        FirebaseManager.Instance?.Initialize();

        #if FIREBASE_ENABLED
        Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsCrashes = true;
        #endif

        bool hasSession  = PlayerPrefs.HasKey("UserToken")
                        && PlayerPrefs.HasKey("SavedUsername");
        bool doneOnboard = PlayerPrefs.GetInt("OnboardingDone", 0) == 1;

        if (hasSession)
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(canvas.transform));
        else if (!doneOnboard)
            ScreenManager.Instance.ChangeScreen(new OnboardingScreen(canvas.transform));
        else
            ScreenManager.Instance.ChangeScreen(new SplashScreen(canvas.transform));
    }

    void SetupManagers()
    {
        GameObject core = GameObject.Find("TOBA_CORE");
        if (core == null)
        {
            core = new GameObject("TOBA_CORE");
            DontDestroyOnLoad(core);
        }

        if (!core.GetComponent<ScreenManager>())   core.AddComponent<ScreenManager>();
        if (!core.GetComponent<APIManager>())      core.AddComponent<APIManager>();
        if (!core.GetComponent<SoundManager>())    core.AddComponent<SoundManager>();
        if (!core.GetComponent<HapticManager>())   core.AddComponent<HapticManager>();
        if (!core.GetComponent<NetworkManager>())  core.AddComponent<NetworkManager>();
        if (!core.GetComponent<FirebaseManager>()) core.AddComponent<FirebaseManager>();
        if (!core.GetComponent<BackHandler>())     core.AddComponent<BackHandler>();
    }

    void SetupCanvas()
    {
        GameObject canvasObj = new GameObject("MainCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

}