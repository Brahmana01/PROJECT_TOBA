using UnityEngine;

public class ChallengeRouterScreen : BaseScreen
{
    private ChallengeData challenge;

    public ChallengeRouterScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeRouter") { challenge = ch; }

    public override void Show()
    {
        switch (challenge.game_type)
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
            // 3 tipe berikut: tampilkan halaman "Segera Hadir"
            case "FIND_OBJECT":
            case "PUZZLE":
            case "GUESS_SOUND":
                ScreenManager.Instance.ChangeScreen(
                    new ChallengeComingSoonScreen(
                        container, challenge, challenge.game_type));
                break;
            default:
                ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
                break;
        }
    }
}