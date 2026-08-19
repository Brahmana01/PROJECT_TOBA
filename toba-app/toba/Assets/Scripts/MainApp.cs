using UnityEngine;

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
        ScreenManager.Instance.canvasTransform = canvas.transform;
        SoundManager.Instance.PlayBGM("bgm_main");

        // CEK SESSION (UNLIMITED LOGIN)
        if (PlayerPrefs.HasKey("UserToken") && PlayerPrefs.HasKey("SavedUsername"))
        {
            // Jika token ada, langsung ke Main Menu
            Debug.Log("Session ditemukan. Selamat datang kembali!");
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(canvas.transform));
        }
        else
        {
            // Jika tidak ada, mulai dari Splash
            ScreenManager.Instance.ChangeScreen(new SplashScreen(canvas.transform));
        }
    }

    void SetupManagers()
    {
        GameObject core = GameObject.Find("TOBA_CORE");
        if (core == null)
        {
            core = new GameObject("TOBA_CORE");
            core.AddComponent<ScreenManager>();
            core.AddComponent<APIManager>();
            core.AddComponent<SoundManager>();
            DontDestroyOnLoad(core);
        }
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