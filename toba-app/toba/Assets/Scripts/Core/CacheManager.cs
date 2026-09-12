using UnityEngine;
using System;

public static class CacheManager
{
    private const float CACHE_HOURS = 24f;

    // ── LOCATIONS ─────────────────────────────────────────
    public static void CacheLocations(string json)
        => Save("cache_locations", json);

    public static string GetCachedLocations()
        => Load("cache_locations");

    // ── LOCATION QUIZ ─────────────────────────────────────
    public static void CacheLocationQuiz(int locId, string json)
        => Save("cache_loc_" + locId, json);

    public static string GetCachedLocationQuiz(int locId)
        => Load("cache_loc_" + locId);

    // ── TRIVIA CATEGORIES ─────────────────────────────────
    public static void CacheTriviaCategories(string json)
        => Save("cache_tri_cats", json);

    public static string GetCachedTriviaCategories()
        => Load("cache_tri_cats");

    // ── TRIVIA QUIZ ───────────────────────────────────────
    public static void CacheTriviaQuiz(int catId, string json)
        => Save("cache_tri_" + catId, json);

    public static string GetCachedTriviaQuiz(int catId)
        => Load("cache_tri_" + catId);

    // ── HELPER ────────────────────────────────────────────
    static void Save(string key, string json)
    {
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.SetString(key + "_t", DateTime.UtcNow.ToString("O"));
        PlayerPrefs.Save();
    }

    static string Load(string key)
    {
        if (!PlayerPrefs.HasKey(key)) return null;
        if (IsExpired(key + "_t")) return null;
        return PlayerPrefs.GetString(key);
    }

    static bool IsExpired(string timeKey)
    {
        if (!PlayerPrefs.HasKey(timeKey)) return true;
        try
        {
            var saved = DateTime.Parse(
                PlayerPrefs.GetString(timeKey), null,
                System.Globalization.DateTimeStyles.RoundtripKind);
            return (DateTime.UtcNow - saved).TotalHours > CACHE_HOURS;
        }
        catch { return true; }
    }

    public static void ClearAll()
    {
        string[] keys = {
            "cache_locations", "cache_tri_cats"
        };
        foreach (var k in keys)
        {
            PlayerPrefs.DeleteKey(k);
            PlayerPrefs.DeleteKey(k + "_t");
        }
        for (int i = 1; i <= 20; i++)
        {
            PlayerPrefs.DeleteKey("cache_loc_" + i);
            PlayerPrefs.DeleteKey("cache_loc_" + i + "_t");
            PlayerPrefs.DeleteKey("cache_tri_" + i);
            PlayerPrefs.DeleteKey("cache_tri_" + i + "_t");
        }
        PlayerPrefs.Save();
        Debug.Log("[Cache] Semua cache dibersihkan.");
    }
}