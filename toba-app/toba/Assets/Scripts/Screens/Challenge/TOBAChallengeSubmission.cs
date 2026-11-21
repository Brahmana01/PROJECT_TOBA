using UnityEngine;

public static class TOBAChallengeSubmission
{
    public static void Submit(
        GameObject sourceScreen,
        Transform container,
        ChallengeData challenge,
        string answersJson)
    {
        if (sourceScreen == null)
            return;

        if (challenge == null ||
            challenge.id <= 0 ||
            string.IsNullOrWhiteSpace(challenge.session_id) ||
            string.IsNullOrWhiteSpace(answersJson))
        {
            NetworkManager.Instance?.ShowToast(
                "Sesi atau jawaban tantangan tidak valid.", true);
            return;
        }

        if (APIManager.Instance == null)
        {
            NetworkManager.Instance?.ShowToast(
                "Layanan tantangan tidak tersedia.", true);
            return;
        }

        SubmitChallengeRequest request = new SubmitChallengeRequest
        {
            session_id = challenge.session_id,
            answers_json = answersJson
        };

        APIManager.Instance.PostRequest(
            "/challenge/submit",
            JsonUtility.ToJson(request),
            response =>
            {
                if (sourceScreen == null)
                    return;

                SoundManager.Instance?.PlaySFX("sfx_win");
                string resultMessage = "Tantangan selesai! Hasil telah divalidasi server.";
                try
                {
                    ChallengeSubmitResponse result = JsonUtility.FromJson<ChallengeSubmitResponse>(response);
                    if (result != null)
                    {
                        resultMessage = "Tantangan selesai: " + result.score + "/" +
                            result.total_questions + " • +" + result.reward_xp + " XP • +" +
                            result.reward_stars + " bintang";
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[Challenge] Respons hasil tidak dapat dibaca: " + ex.Message);
                }
                NetworkManager.Instance?.ShowToast(resultMessage, false);
                ScreenManager.Instance?.ChangeScreen(
                    new MainMenuScreen(container));
            },
            error =>
            {
                if (sourceScreen == null)
                    return;

                Debug.LogWarning("[Challenge] Gagal menyimpan hasil: " + error);
                NetworkManager.Instance?.ShowToast(
                    "Gagal menyimpan hasil tantangan. " + error, true);
                ScreenManager.Instance?.ChangeScreen(
                    new MainMenuScreen(container));
            }
        );
    }
}
