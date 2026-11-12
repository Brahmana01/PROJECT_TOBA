using UnityEngine;

public class ChallengeRouterScreen : BaseScreen
{
    private ChallengeData challenge;

    public ChallengeRouterScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeRouter")
    {
        challenge = ch;
    }

    public override void Show()
    {
        if (ScreenManager.Instance == null)
        {
            Debug.LogError("[ChallengeRouter] ScreenManager tidak tersedia.");
            return;
        }

        if (challenge == null)
        {
            Debug.LogWarning("[ChallengeRouter] Data challenge kosong.");
            ScreenManager.Instance.ChangeScreen(
                new MainMenuScreen(container)
            );
            return;
        }

        string gameType = string.IsNullOrWhiteSpace(challenge.game_type)
            ? string.Empty
            : challenge.game_type.Trim().ToUpperInvariant();

        switch (gameType)
        {
            case "MATCHING":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeMatchingScreen(container, challenge));
                break;

            case "GUESS_IMAGE":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeGuessImageScreen(container, challenge));
                break;

            case "WORD_ARRANGE":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeWordArrangeScreen(container, challenge));
                break;

            case "MEMORY_CARD":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeMemoryCardScreen(container, challenge));
                break;

            case "ORDER":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeOrderScreen(container, challenge));
                break;

            case "CATEGORIZE":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeCategorizeScreen(container, challenge));
                break;

            case "FIND_OBJECT":
            case "PUZZLE":
            case "GUESS_SOUND":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeComingSoonScreen(
                        container, challenge, gameType));
                break;

            default:
                Debug.LogWarning(
                    "[ChallengeRouter] Tipe game tidak dikenal: " + gameType
                );

                ScreenManager.Instance.ChangeScreen(
                    new MainMenuScreen(container)
                );
                break;
        }
    }

    
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
                Debug.LogWarning("[Challenge] APIManager tidak tersedia.");

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


}
