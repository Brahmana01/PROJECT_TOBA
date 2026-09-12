using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;

    private BaseScreen currentScreen;
    public Transform canvasTransform;

    // ============================================================
    // SCREEN STATE
    // ============================================================

    private string _currentScreenName = "";

    private System.Collections.Generic.Stack<System.Action> _history =
        new System.Collections.Generic.Stack<System.Action>();

    // ============================================================
    // AWAKE
    // ============================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================================
    // CHANGE SCREEN
    // ============================================================

    public void ChangeScreen(BaseScreen newScreen)
    {
        if (currentScreen != null)
        {
            currentScreen.Hide();
        }

        currentScreen = newScreen;

        _currentScreenName =
            newScreen.GetType()
                .Name
                .Replace("Screen", "");

        currentScreen.Show();

        Debug.Log(
            "[ScreenManager] Switched to: " +
            newScreen.GetType().Name
        );
    }

    // ============================================================
    // GET CURRENT SCREEN
    // ============================================================

    public string GetCurrentScreenName() =>
        _currentScreenName;

    // ============================================================
    // GO BACK
    // ============================================================

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            _history.Pop()?.Invoke();
        }
        else
        {
            // Kembali ke MainMenu sebagai default
            ChangeScreen(new MainMenuScreen(canvasTransform));
        }
    }
}