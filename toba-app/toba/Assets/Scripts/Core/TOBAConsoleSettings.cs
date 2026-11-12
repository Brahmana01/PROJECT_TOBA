using UnityEngine;

public static class TOBAConsoleSettings
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void DisableDeveloperConsole()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        // Menonaktifkan Unity Developer Console.
        Debug.developerConsoleEnabled = false;

        // Memastikan jendela console tidak terlihat.
        Debug.developerConsoleVisible = false;

#endif
    }
}
