using LanguageExt;

namespace BlockLife.Core.Domain.Turn
{
    /// <summary>
    /// Domain service interface for managing turn progression and state.
    /// Encapsulates the business rules for when and how turns advance.
    /// Following established domain service patterns.
    /// </summary>
    public interface ITurnManager
    {
        /// <summary>
        /// Gets the current turn.
        /// </summary>
        /// <returns>The current turn, or None if not initialized</returns>
        Option<Turn> GetCurrentTurn();

        /// <summary>
        /// Advances to the next turn with validation.
        /// Only advances if the current game state allows it.
        /// </summary>
        /// <returns>Fin&lt;Turn&gt; with new current turn or validation error</returns>
        Fin<Turn> AdvanceTurn();

        /// <summary>
        /// Checks if turn advancement is currently allowed.
        /// Turn advancement requires that a valid game action was completed.
        /// </summary>
        /// <returns>True if turn can be advanced, false otherwise</returns>
        bool CanAdvanceTurn();

        /// <summary>
        /// Resets the turn manager to initial state (turn 1).
        /// Used when starting a new game.
        /// </summary>
        /// <returns>Fin&lt;Turn&gt; with initial turn or error</returns>
        Fin<Turn> Reset();

        /// <summary>
        /// Gets the total number of turns elapsed in the current game.
        /// Useful for game statistics and progression tracking.
        /// </summary>
        /// <returns>Number of completed turns, 0 if not initialized</returns>
        int GetTurnsElapsed();

        /// <summary>
        /// Marks that a valid action has been performed, enabling turn advancement.
        /// This is called after successful moves, pattern matches, etc.
        /// </summary>
        void MarkActionPerformed();

        /// <summary>
        /// Checks if an action has been performed in the current turn.
        /// Used to enforce one-action-per-turn limitation.
        /// </summary>
        /// <returns>True if an action was performed this turn</returns>
        bool HasActionBeenPerformed();

        /// <summary>
        /// Loads turn state from external source (save data, etc.).
        /// </summary>
        /// <param name="turn">The turn to restore</param>
        /// <param name="actionPerformed">Whether an action was performed in this turn</param>
        /// <returns>Fin&lt;Turn&gt; with loaded turn or validation error</returns>
        Fin<Turn> LoadState(Turn turn, bool actionPerformed);
    }
}