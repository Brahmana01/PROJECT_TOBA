
using UnityEngine;

public static class TOBAChallengeSubmission
{
    public static void Submit(
        GameObject sourceScreen,
        Transform container,
        ChallengeData challenge,
        int score)
    {
        if (sourceScreen == null)
            return;

        int playerID = PlayerPrefs.GetInt("PlayerID");

        if (challenge == null ||
            challenge.id <= 0 ||
            playerID <= 0)
        {
            Debug.LogWarning(
                "[Challenge] Data pemain atau challenge tidak valid."
            );

            NetworkManager.Instance?.ShowToast(
                "Data tantangan tidak valid.",
                true
            );

            ScreenManager.Instance?.ChangeScreen(
                new MainMenuScreen(container)
            );

            return;
        }

        if (APIManager.Instance == null)
        {
            Debug.LogWarning(
                "[Challenge] APIManager tidak tersedia."
            );

            NetworkManager.Instance?.ShowToast(
                "Layanan tantangan tidak tersedia.",
                true
            );

            ScreenManager.Instance?.ChangeScreen(
                new MainMenuScreen(container)
            );

            return;
        }

        SubmitChallengeRequest req = new SubmitChallengeRequest
        {
            player_id = playerID,
            challenge_id = challenge.id,
            score = Mathf.Max(0, score)
        };

        APIManager.Instance.PostRequest(
            "/challenge/submit",
            JsonUtility.ToJson(req),

            response =>
            {
                if (sourceScreen == null)
                    return;

                SoundManager.Instance?.PlaySFX("sfx_win");

                ScreenManager.Instance?.ChangeScreen(
                    new MainMenuScreen(container)
                );
            },

            error =>
            {
                if (sourceScreen == null)
                    return;

                Debug.LogWarning(
                    "[Challenge] Gagal menyimpan hasil: " + error
                );

                NetworkManager.Instance?.ShowToast(
                    "Gagal menyimpan hasil tantangan.",
                    true
                );

                ScreenManager.Instance?.ChangeScreen(
                    new MainMenuScreen(container)
                );
            }
        );
    }
}
