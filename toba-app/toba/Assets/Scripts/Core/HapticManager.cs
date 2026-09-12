using UnityEngine;

public enum HapticType { Light, Medium, Heavy, Success, Error }

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;
    private bool _enabled = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _enabled = PlayerPrefs.GetInt("HapticEnabled", 1) == 1;
        }
        else Destroy(gameObject);
    }

    public void SetEnabled(bool val)
    {
        _enabled = val;
        PlayerPrefs.SetInt("HapticEnabled", val ? 1 : 0);
    }

    public bool IsEnabled() => _enabled;

    public void Vibrate(HapticType type = HapticType.Light)
    {
        if (!_enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            switch (type)
            {
                case HapticType.Light:
                    Handheld.Vibrate();
                    break;
                case HapticType.Medium:
                    VibrateMs(80L);
                    break;
                case HapticType.Heavy:
                    VibrateMs(200L);
                    break;
                case HapticType.Success:
                    VibratePattern(new long[] { 0, 50, 50, 100 });
                    break;
                case HapticType.Error:
                    VibratePattern(new long[] { 0, 100, 50, 100, 50, 100 });
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Haptic] " + e.Message);
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject GetVibrator()
    {
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        return activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    }

    void VibrateMs(long ms)
    {
        using var v = GetVibrator();
        v.Call("vibrate", ms);
    }

    void VibratePattern(long[] pattern)
    {
        using var v = GetVibrator();
        v.Call("vibrate", pattern, -1);
    }
#endif
}   