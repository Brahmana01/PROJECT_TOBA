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
            SubmitChallengeRequest pendingRequest = new SubmitChallengeRequest
            {
                session_id = challenge.session_id,
                answers_json = answersJson
            };
            string pendingPayload = JsonUtility.ToJson(pendingRequest);
            bool queued = MainApp.QueueOfflineSubmission("/challenge/submit", pendingPayload);
            NetworkManager.Instance?.ShowToast(
                queued
                    ? "Layanan belum tersedia. Hasil tantangan disimpan untuk sinkronisasi."
                    : "Hasil tantangan tidak dapat disimpan ke antrean sinkronisasi.",
                !queued);
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container));
            return;
        }

        SubmitChallengeRequest request = new SubmitChallengeRequest
        {
            session_id = challenge.session_id,
            answers_json = answersJson
        };

        string payload = JsonUtility.ToJson(request);

        APIManager.Instance.PostRequest(
            "/challenge/submit",
            payload,
            response =>
            {
                MainApp.RemoveOfflineSubmission("/challenge/submit", payload);
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

                        if (result.level > 0)
                        {
                            PlayerPrefs.SetInt("PlayerLevel", result.level);
                            PlayerPrefs.Save();
                        }
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
                bool retryable = MainApp.IsRetryableSubmissionError(error);
                bool queued = !retryable || MainApp.QueueOfflineSubmission("/challenge/submit", payload);

                if (sourceScreen == null)
                    return;

                Debug.LogWarning("[Challenge] Gagal menyimpan hasil: " + error);
                NetworkManager.Instance?.ShowToast(
                    retryable && queued
                        ? "Koneksi terputus. Hasil tantangan disimpan untuk sinkronisasi."
                        : retryable
                            ? "Hasil tantangan tidak dapat disimpan ke antrean sinkronisasi."
                            : "Gagal menyimpan hasil tantangan. " + error,
                    !retryable || !queued);
                ScreenManager.Instance?.ChangeScreen(
                    new MainMenuScreen(container));
            }
        );
    }
}
