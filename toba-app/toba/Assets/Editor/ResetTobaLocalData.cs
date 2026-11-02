using UnityEngine;
using UnityEditor;

public static class ResetTobaLocalData
{
    [MenuItem("TOBA/Reset Local Data")]
    public static void ResetLocalData()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode sebelum melakukan reset."
            );
            return;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log(
            "[TOBA] Data lokal berhasil direset. " +
            "Jalankan game kembali."
        );
    }
}