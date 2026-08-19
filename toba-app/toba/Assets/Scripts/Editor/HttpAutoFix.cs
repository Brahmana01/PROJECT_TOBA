using UnityEditor;

[InitializeOnLoad]
public class HttpAutoFix
{
    static HttpAutoFix()
    {
        // Mengatur Player Settings agar mengizinkan HTTP (Insecure connection) secara otomatis
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        UnityEngine.Debug.Log("Sistem TOBA: Akses HTTP telah diizinkan secara otomatis.");
    }
}