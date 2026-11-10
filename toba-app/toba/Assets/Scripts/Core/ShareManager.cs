using UnityEngine;

public static class ShareManager
{
    public static void ShareQuizResult(string locationName, int correct,
        int total, int stars)
    {
        string s = "";
        for (int i = 0; i < stars; i++) s += "<b><color=#D6A84B>★</color></b>";
        string msg =
            "🎮 TOBA — Game Edukasi Budaya Batak Toba\n\n" +
            $"Aku baru selesai tantangan di *{locationName}*!\n" +
            $"Hasil: {correct}/{total} jawaban benar {s}\n\n" +
            "Yuk pelajari budaya Batak Toba bareng! 🏔️\n" +
            "#TOBA #BudayaBatak #GameEdukasi";
        Share(msg);
    }

    public static void ShareTriviaResult(string catName, int correct, int total)
    {
        float pct = total > 0 ? (float)correct / total * 100f : 0f;
        string msg =
            "🎮 TOBA — Trivia Budaya Batak Toba\n\n" +
            $"Trivia *{catName}*: {correct}/{total} benar ({pct:F0}%)\n\n" +
            "Kamu bisa nyamain skorku? 📚\n" +
            "#TOBA #TriviaBudaya #BatakToba";
        Share(msg);
    }

    public static void ShareBattleResult(bool won, bool isDraw, string oppName)
    {
        string emoji  = isDraw ? "🤝" : won ? "<b><color=#D6A84B>♛</color></b>" : "💪";
        string result = isDraw ? "Seri" : won ? "Menang" : "Kalah (tapi tetap semangat!)";
        string msg =
            $"🎮 TOBA — Battle Quiz Budaya Batak Toba\n\n" +
            $"{emoji} {result} melawan {oppName}!\n\n" +
            "Ayo battle quiz budaya Batak bersamaku! <b><color=#A83232>⚔</color></b>\n" +
            "#TOBA #BattleQuiz #BudayaBatak";
        Share(msg);
    }

    public static void Share(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var intentCls = new AndroidJavaClass("android.content.Intent");
            using var intent    = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
            intent.Call<AndroidJavaObject>("setType",   "text/plain");
            intent.Call<AndroidJavaObject>("putExtra",
                "android.intent.extra.TEXT", text);
            using var chooser = intentCls.CallStatic<AndroidJavaObject>(
                "createChooser", intent, "Bagikan via...");
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("startActivity", chooser);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Share] " + e.Message);
        }
#elif UNITY_EDITOR
        Debug.Log("[Share Preview]\n" + text);
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}