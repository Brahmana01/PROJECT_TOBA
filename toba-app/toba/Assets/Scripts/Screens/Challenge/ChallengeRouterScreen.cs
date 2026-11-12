
using UnityEngine;

public class ChallengeRouterScreen : BaseScreen
{
    private readonly ChallengeData challenge;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public ChallengeRouterScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeRouter")
    {
        challenge = ch;
    }

    // ============================================================
    // SHOW / ROUTING
    // ============================================================

    public override void Show()
    {
        // Pastikan screen masih valid.
        if (!IsReady)
        {
            Debug.LogWarning(
                "[ChallengeRouter] Screen tidak tersedia."
            );
            return;
        }

        ScreenManager manager = ScreenManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "[ChallengeRouter] ScreenManager tidak tersedia."
            );
            return;
        }

        // Validasi data challenge.
        if (challenge == null || challenge.id <= 0)
        {
            Debug.LogWarning(
                "[ChallengeRouter] Data challenge tidak valid."
            );

            ReturnToMainMenu(manager);
            return;
        }

        // Normalisasi game type.
        string gameType = string.IsNullOrWhiteSpace(
            challenge.game_type
        )
            ? string.Empty
            : challenge.game_type.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(gameType))
        {
            Debug.LogWarning(
                "[ChallengeRouter] Game type kosong."
            );

            ReturnToMainMenu(manager);
            return;
        }

        BaseScreen nextScreen = null;

        // ========================================================
        // ROUTING GAME
        // ========================================================

        switch (gameType)
        {
            case "MATCHING":

                nextScreen = new ChallengeMatchingScreen(
                    container,
                    challenge
                );
                break;

            case "GUESS_IMAGE":

                nextScreen = new ChallengeGuessImageScreen(
                    container,
                    challenge
                );
                break;

            case "WORD_ARRANGE":

                nextScreen = new ChallengeWordArrangeScreen(
                    container,
                    challenge
                );
                break;

            case "MEMORY_CARD":

                nextScreen = new ChallengeMemoryCardScreen(
                    container,
                    challenge
                );
                break;

            case "ORDER":

                nextScreen = new ChallengeOrderScreen(
                    container,
                    challenge
                );
                break;

            case "CATEGORIZE":

                nextScreen = new ChallengeCategorizeScreen(
                    container,
                    challenge
                );
                break;

            case "FIND_OBJECT":
            case "PUZZLE":
            case "GUESS_SOUND":

                nextScreen = new ChallengeComingSoonScreen(
                    container,
                    challenge,
                    gameType
                );
                break;

            default:

                Debug.LogWarning(
                    "[ChallengeRouter] Tipe game tidak dikenal: "
                    + gameType
                );

                nextScreen = new MainMenuScreen(container);
                break;
        }

        // ========================================================
        // VALIDASI SCREEN TUJUAN
        // ========================================================

        if (nextScreen == null)
        {
            Debug.LogWarning(
                "[ChallengeRouter] Screen tujuan tidak tersedia."
            );

            ReturnToMainMenu(manager);
            return;
        }

        if (!nextScreen.IsReady)
        {
            Debug.LogWarning(
                "[ChallengeRouter] Screen tujuan tidak valid: "
                + nextScreen.GetType().Name
            );

            nextScreen.Hide();

            ReturnToMainMenu(manager);
            return;
        }

        // ========================================================
        // PINDAH SCREEN
        // ========================================================

        manager.ChangeScreen(nextScreen);
    }

    // ============================================================
    // FALLBACK
    // ============================================================

    private void ReturnToMainMenu(ScreenManager manager)
    {
        if (manager == null || container == null)
            return;

        manager.ChangeScreen(
            new MainMenuScreen(container)
        );
    }
}
