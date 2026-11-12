
using UnityEngine;
using System.Collections.Generic;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;

    private BaseScreen currentScreen;

    public Transform canvasTransform;

    // ============================================================
    // SCREEN STATE
    // ============================================================

    private string _currentScreenName = "";

    private readonly Stack<System.Action> _history =
        new Stack<System.Action>();

    // ============================================================
    // AWAKE
    // ============================================================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ============================================================
    // CHANGE SCREEN
    // ============================================================

    public void ChangeScreen(BaseScreen newScreen)
    {
        // Pastikan screen baru tersedia.
        if (newScreen == null)
        {
            Debug.LogWarning(
                "[ScreenManager] Screen baru null. Perpindahan dibatalkan."
            );
            return;
        }

        // Pastikan screen memiliki parent dan GameObject valid.
        if (!newScreen.IsReady)
        {
            Debug.LogWarning(
                "[ScreenManager] Screen tidak valid: " +
                newScreen.GetType().Name
            );

            newScreen.Hide();
            return;
        }

        // Hindari membuka instance screen yang sama dua kali.
        if (currentScreen == newScreen)
            return;

        BaseScreen previousScreen = currentScreen;

        // Tetapkan screen baru sebelum Show().
        // Ini penting untuk screen yang melakukan perpindahan
        // lanjutan dari dalam fungsi Show().
        currentScreen = newScreen;

        _currentScreenName = newScreen.GetType()
            .Name
            .Replace("Screen", "");

        // Tutup screen sebelumnya.
        if (previousScreen != null &&
            previousScreen != newScreen)
        {
            previousScreen.Hide();
        }

        // Tampilkan screen baru.
        newScreen.Show();

        // Hindari log yang salah pada perpindahan bertingkat.
        if (currentScreen == newScreen)
        {
            Debug.Log(
                "[ScreenManager] Switched to: " +
                newScreen.GetType().Name
            );
        }
    }

    // ============================================================
    // CURRENT SCREEN VALIDATION
    // ============================================================

    public bool IsCurrentScreen(BaseScreen screen)
    {
        return screen != null &&
               currentScreen == screen &&
               screen.IsReady;
    }

    public string GetCurrentScreenName()
    {
        return _currentScreenName;
    }

    // ============================================================
    // GO BACK
    // ============================================================

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            System.Action action = _history.Pop();
            action?.Invoke();
            return;
        }

        if (canvasTransform == null)
        {
            Debug.LogWarning(
                "[ScreenManager] Canvas belum tersedia."
            );
            return;
        }

        ChangeScreen(
            new MainMenuScreen(canvasTransform)
        );
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (currentScreen != null)
        {
            currentScreen.Hide();
            currentScreen = null;
        }

        _currentScreenName = "";

        Instance = null;
    }
}
