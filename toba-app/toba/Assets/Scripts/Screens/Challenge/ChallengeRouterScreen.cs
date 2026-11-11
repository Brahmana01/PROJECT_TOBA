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
}
