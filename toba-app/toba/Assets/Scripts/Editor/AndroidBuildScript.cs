#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;


public class AndroidBuildScript
{
    // ── Konfigurasi Build ────────────────────────────────
    private const string APP_NAME        = "TOBA";
    private const string BUNDLE_ID       = "com.toba.budayabatak";
    private const string VERSION         = "1.0.0";
    private const int    VERSION_CODE    = 1;
    private const string OUTPUT_FOLDER   = "Builds/Android";
    private const string OUTPUT_FILENAME = "TOBA_v1.0.0.apk";

    // ── Build APK Debug ──────────────────────────────────
    [MenuItem("TOBA/Build/Android APK (Debug)")]
    public static void BuildDebugAPK()
    {
        ConfigurePlayerSettings();
        string outputPath = GetOutputPath("TOBA_DEBUG.apk");
        BuildAPK(outputPath, BuildOptions.Development | BuildOptions.AllowDebugging);
    }

    // ── Build APK Release ─────────────────────────────────
    [MenuItem("TOBA/Build/Android APK (Release)")]
    public static void BuildReleaseAPK()
    {
        ConfigurePlayerSettings();
        string outputPath = GetOutputPath(OUTPUT_FILENAME);
        BuildAPK(outputPath, BuildOptions.None);
    }

    // ── Build APK + Run di Device ────────────────────────
    [MenuItem("TOBA/Build/Android APK + Install ke Device")]
    public static void BuildAndRunAPK()
    {
        ConfigurePlayerSettings();
        string outputPath = GetOutputPath("TOBA_INSTALL.apk");
        BuildAPK(outputPath, BuildOptions.AutoRunPlayer);
    }

    // ── Helper: Konfigurasi PlayerSettings ───────────────
    static void ConfigurePlayerSettings()
    {
        // Identitas app
        PlayerSettings.productName                          = APP_NAME;
        PlayerSettings.applicationIdentifier               = BUNDLE_ID;
        PlayerSettings.bundleVersion                       = VERSION;
        PlayerSettings.Android.bundleVersionCode           = VERSION_CODE;

        // Resolusi & orientasi
        PlayerSettings.defaultInterfaceOrientation         = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait         = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft    = false;
        PlayerSettings.allowedAutorotateToLandscapeRight   = false;

        // Android specific
        PlayerSettings.Android.minSdkVersion               = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion            = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.Android.preferredInstallLocation    = AndroidPreferredInstallLocation.Auto;

        // Grafis
        PlayerSettings.colorSpace                          = ColorSpace.Linear;
        PlayerSettings.gpuSkinning                        = true;

        // Scripting
        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Internet permission (wajib untuk API)
        PlayerSettings.Android.forceInternetPermission     = true;

        // Splash screen
        PlayerSettings.SplashScreen.show                   = false;

        Debug.Log("[TOBA Build] Player settings dikonfigurasi.");
    }

    // ── Helper: Build APK ─────────────────────────────────
    static void BuildAPK(string outputPath, BuildOptions options)
    {
        // Pastikan folder output ada
        string dir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // Scene yang akan di-build (hanya 1 scene)
        string[] scenes = GetBuildScenes();

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes           = scenes,
            locationPathName = outputPath,
            target           = BuildTarget.Android,
            options          = options
        };

        Debug.Log("[TOBA Build] Memulai build APK...");
        Debug.Log("[TOBA Build] Output: " + outputPath);

        BuildReport  report  = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            long sizeInMB = (long)(summary.totalSize / 1024 / 1024);
            Debug.Log($"[TOBA Build] ✅ Build BERHASIL!");
            Debug.Log($"[TOBA Build] Ukuran: {sizeInMB} MB");
            Debug.Log($"[TOBA Build] Waktu: {summary.totalTime.TotalSeconds:F1} detik");
            Debug.Log($"[TOBA Build] Output: {outputPath}");

            // Buka folder output
            EditorUtility.RevealInFinder(outputPath);
        }
        else
        {
            Debug.LogError($"[TOBA Build] ❌ Build GAGAL: {summary.result}");
        }
    }

    // ── Helper: Ambil semua scene ────────────────────────
    static string[] GetBuildScenes()
    {
        // Ambil scenes dari Build Settings
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        if (scenes.Count == 0)
        {
            // Fallback: cari scene MainScene
            string[] allScenes = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in allScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scenes.Add(path);
                break; // Ambil scene pertama
            }
        }

        Debug.Log("[TOBA Build] Scenes: " + string.Join(", ", scenes));
        return scenes.ToArray();
    }

    // ── Helper: Path output ──────────────────────────────
    static string GetOutputPath(string filename)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(projectRoot, OUTPUT_FOLDER, filename);
    }
}
#endif