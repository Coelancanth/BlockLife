using BlockLife.Core.Domain.Turn;

namespace BlockLife.Core.Features.Turn
{
    /// <summary>
    /// Interface for turn display capabilities in the view layer.
    /// This interface defines the display capability that views must implement
    /// to support turn information updates following the MVP pattern.
    /// </summary>
    public interface ITurnDisplayView
    {
        /// <summary>
        /// Updates the displayed turn information when a new turn starts.
        /// </summary>
        /// <param name="turn">The turn that has started</param>
        void DisplayTurnStart(Domain.Turn.Turn turn);

        /// <summary>
        /// Updates the displayed turn information when a turn ends.
        /// Called before the turn advances to provide transition feedback.
        /// </summary>
        /// <param name="turn">The turn that has ended</param>
        void DisplayTurnEnd(Domain.Turn.Turn turn);

        /// <summary>
        /// Updates the current turn display without transition effects.
        /// Used for immediate updates or initialization scenarios.
        /// </summary>
        /// <param name="turn">The current turn to display</param>
        void UpdateCurrentTurn(Domain.Turn.Turn turn);

        /// <summary>
        /// Shows an error related to turn operations.
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        void ShowTurnError(string errorMessage);
    }
}