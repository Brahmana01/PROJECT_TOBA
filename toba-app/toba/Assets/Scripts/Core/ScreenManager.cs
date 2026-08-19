using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;
    private BaseScreen currentScreen;
    public Transform canvasTransform;

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

    public void ChangeScreen(BaseScreen newScreen)
    {
        if (currentScreen != null)
        {
            currentScreen.Hide();
        }
        
        currentScreen = newScreen;
        currentScreen.Show();
        Debug.Log("[ScreenManager] Switched to: " + newScreen.GetType().Name);
    }
}