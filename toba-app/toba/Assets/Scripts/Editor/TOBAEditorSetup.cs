#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class TOBAEditorSetup
{
    // ── Setup Lengkap Project ─────────────────────────────
    [MenuItem("TOBA/Setup/Setup Project Lengkap")]
    public static void SetupProject()
    {
        Debug.Log("[TOBA Setup] Memulai setup project...");

        SetupScene();
        SetupBuildSettings();
        SetupQualitySettings();
        SetupAndroidSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TOBA Setup] ✅ Setup selesai! Project siap digunakan.");
        EditorUtility.DisplayDialog("TOBA Setup",
            "Setup project selesai!\n\n" +
            "✅ Scene dikonfigurasi\n" +
            "✅ Build Settings diatur\n" +
            "✅ Quality Settings dioptimalkan\n" +
            "✅ Android Settings dikonfigurasi\n\n" +
            "Kamu bisa langsung Play atau Build APK.",
            "OK");
    }

    // ── Setup Scene ──────────────────────────────────────
    [MenuItem("TOBA/Setup/Setup Scene")]
    public static void SetupScene()
    {
        // Cari atau buat scene
        string scenePath = "Assets/Scenes/MainScene.unity";

        UnityEngine.SceneManagement.Scene scene;

        // Cek apakah scene sudah ada
        if (!System.IO.File.Exists(scenePath))
        {
            // Buat folder Scenes jika belum ada
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[TOBA Setup] Scene baru dibuat: " + scenePath);
        }
        else
        {
            scene = EditorSceneManager.OpenScene(scenePath);
            Debug.Log("[TOBA Setup] Scene dibuka: " + scenePath);
        }

        // Cari atau buat GameObject MainApp
        GameObject mainApp = GameObject.Find("MainApp");
        if (mainApp == null)
        {
            mainApp = new GameObject("MainApp");
            mainApp.AddComponent<MainApp>();
            Debug.Log("[TOBA Setup] ✅ MainApp GameObject dibuat");
        }
        else
        {
            if (mainApp.GetComponent<MainApp>() == null)
                mainApp.AddComponent<MainApp>();
            Debug.Log("[TOBA Setup] ✅ MainApp sudah ada");
        }

        EditorSceneManager.SaveScene(scene);
    }

    // ── Setup Build Settings ──────────────────────────────
    static void SetupBuildSettings()
    {
        string scenePath = "Assets/Scenes/MainScene.unity";

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[TOBA Setup] ✅ Build Settings dikonfigurasi");
    }

    // ── Setup Quality Settings ────────────────────────────
    static void SetupQualitySettings()
    {
        // Target 60 FPS
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount  = 0;

        // Shadow off untuk performa mobile
        QualitySettings.shadows       = ShadowQuality.Disable;
        QualitySettings.antiAliasing  = 2;

        Debug.Log("[TOBA Setup] ✅ Quality Settings dioptimalkan untuk mobile");
    }

    // ── Setup Android Settings ────────────────────────────
    static void SetupAndroidSettings()
    {
        // App identifier
        PlayerSettings.applicationIdentifier =
            "com.toba.budayabatak";
        PlayerSettings.productName = "TOBA";

        // Portrait only
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // Min SDK
        PlayerSettings.Android.minSdkVersion =
            AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion =
            AndroidSdkVersions.AndroidApiLevel34;

        // IL2CPP + ARM64
        PlayerSettings.SetScriptingBackend(
            BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Internet permission
        PlayerSettings.Android.forceInternetPermission = true;

        // Warna
        PlayerSettings.colorSpace = ColorSpace.Linear;

        Debug.Log("[TOBA Setup] ✅ Android Settings dikonfigurasi");
    }

    // ── Validasi IP untuk Android Device ─────────────────
    [MenuItem("TOBA/Setup/Update IP Backend untuk Android")]
    public static void UpdateBackendIP()
    {
        string currentIP = EditorPrefs.GetString("TOBA_BackendIP", "192.168.1.x");

        string newIP = EditorInputDialog.Show(
            "Update IP Backend",
            "Masukkan IP komputer kamu (yang menjalankan backend):\n" +
            "Contoh: 192.168.1.100\n\n" +
            "IP saat ini: " + currentIP,
            currentIP);

        if (!string.IsNullOrEmpty(newIP))
        {
            EditorPrefs.SetString("TOBA_BackendIP", newIP);

            // Update APIManager.cs
            string apiManagerPath =
                "Assets/Scripts/Core/APIManager.cs";

            if (System.IO.File.Exists(apiManagerPath))
            {
                string content = System.IO.File.ReadAllText(apiManagerPath);

                // Replace URL
                System.Text.RegularExpressions.Regex rgxHttp =
                    new System.Text.RegularExpressions.Regex(
                        @"private string BASE_URL = ""http://[^""]+""");
                System.Text.RegularExpressions.Regex rgxWS =
                    new System.Text.RegularExpressions.Regex(
                        @"private string WS_URL\s*=\s*""ws://[^""]+""");

                content = rgxHttp.Replace(content,
                    $"private string BASE_URL = \"http://{newIP}:8080/api\"");
                content = rgxWS.Replace(content,
                    $"private string WS_URL   = \"ws://{newIP}:8080/ws\"");

                System.IO.File.WriteAllText(apiManagerPath, content);
                AssetDatabase.Refresh();

                Debug.Log($"[TOBA Setup] ✅ IP Backend diupdate ke: {newIP}");
                EditorUtility.DisplayDialog("Berhasil",
                    $"IP Backend diupdate ke:\n{newIP}\n\n" +
                    "APIManager.cs sudah diperbarui.", "OK");
            }
            else
            {
                Debug.LogWarning("[TOBA Setup] APIManager.cs tidak ditemukan!");
            }
        }
    }

    // ── Bersihkan PlayerPrefs ────────────────────────────
    [MenuItem("TOBA/Debug/Clear PlayerPrefs (Logout)")]
    public static void ClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Clear PlayerPrefs",
            "Ini akan menghapus semua data login tersimpan.\nLanjutkan?",
            "Ya, Hapus", "Batal"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[TOBA Debug] ✅ PlayerPrefs dihapus");
        }
    }

    // ── Play Mode langsung dari MainScene ────────────────
    [MenuItem("TOBA/Debug/Play dari MainScene")]
    public static void PlayFromMainScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        string scenePath = "Assets/Scenes/MainScene.unity";
        if (System.IO.File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }
        else
        {
            EditorUtility.DisplayDialog("Scene tidak ditemukan",
                "Jalankan TOBA → Setup → Setup Project Lengkap terlebih dahulu.",
                "OK");
        }
    }
}

// ── Helper: Input Dialog sederhana ───────────────────────
public static class EditorInputDialog
{
    public static string Show(string title, string message, string defaultValue)
    {
        // Gunakan EditorPrefs sebagai temporary storage
        string result = EditorPrefs.GetString("TOBA_InputDialog_Result", defaultValue);

        bool confirmed = EditorUtility.DisplayDialog(title,
            message + "\n\n(Nilai saat ini: " + result + ")\n" +
            "Untuk mengubah, edit langsung di APIManager.cs atau\n" +
            "gunakan menu ini setelah mengubah nilai di bawah.",
            "Gunakan Nilai Saat Ini", "Batal");

        return confirmed ? result : "";
    }
}
#endif