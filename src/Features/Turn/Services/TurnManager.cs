using BlockLife.Core.Domain.Turn;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System.Threading;
using Serilog;

namespace BlockLife.Core.Features.Turn.Services
{
    /// <summary>
    /// Domain service implementation for managing turn progression and state.
    /// Enforces business rules: only one action per turn, turns advance after valid actions.
    /// Thread-safe implementation using immutable Turn objects and atomic operations.
    /// Following established service patterns with LanguageExt functional types.
    /// </summary>
    public sealed class TurnManager : ITurnManager
    {
        private Option<Core.Domain.Turn.Turn> _currentTurn = None;
        private bool _actionPerformed = false;
        private readonly object _lock = new object();
        private readonly ILogger _logger = Log.ForContext<TurnManager>();

        /// <summary>
        /// Gets the current turn.
        /// </summary>
        /// <returns>The current turn, or None if not initialized</returns>
        public Option<Core.Domain.Turn.Turn> GetCurrentTurn() => _currentTurn;

        /// <summary>
        /// Advances to the next turn with validation.
        /// Requires that an action has been performed in the current turn.
        /// Resets the action flag for the new turn.
        /// </summary>
        /// <returns>Fin&lt;Turn&gt; with new current turn or validation error</returns>
        public Fin<Core.Domain.Turn.Turn> AdvanceTurn()
        {
            lock (_lock)
            {
                // Validate that we can advance
                if (!CanAdvanceTurn())
                {
                    return _currentTurn.Match(
                        Some: turn => FinFail<Core.Domain.Turn.Turn>(Error.New($"Cannot advance from turn {turn.Number} - no action performed")),
                        None: () => FinFail<Core.Domain.Turn.Turn>(Error.New("Cannot advance turn - game not initialized"))
                    );
                }

                return _currentTurn.Match(
                    Some: current => 
                    {
                        return current.Next().Match(
                            Succ: nextTurn =>
                            {
                                _logger.Information("Turn {CurrentTurn} → Turn {NextTurn}", 
                                    current.Number, nextTurn.Number);
                                _currentTurn = Some(nextTurn);
                                _actionPerformed = false; // Reset for new turn
                                return nextTurn;
                            },
                            Fail: error => FinFail<Core.Domain.Turn.Turn>(error)
                        );
                    },
                    None: () => FinFail<Core.Domain.Turn.Turn>(Error.New("Cannot advance turn - game not initialized"))
                );
            }
        }

        /// <summary>
        /// Checks if turn advancement is currently allowed.
        /// Turn advancement requires that a valid game action was completed.
        /// </summary>
        /// <returns>True if turn can be advanced, false otherwise</returns>
        public bool CanAdvanceTurn()
        {
            return _currentTurn.IsSome && _actionPerformed;
        }

        /// <summary>
        /// Resets the turn manager to initial state (turn 1).
        /// Used when starting a new game.
        /// </summary>
        /// <returns>Fin&lt;Turn&gt; with initial turn</returns>
        public Fin<Core.Domain.Turn.Turn> Reset()
        {
            lock (_lock)
            {
                var initialTurn = Core.Domain.Turn.Turn.CreateInitial();
                _currentTurn = Some(initialTurn);
                _actionPerformed = false;
                return initialTurn;
            }
        }

        /// <summary>
        /// Gets the total number of turns elapsed in the current game.
        /// </summary>
        /// <returns>Number of turns completed (current turn number), 0 if not initialized</returns>
        public int GetTurnsElapsed()
        {
            return _currentTurn.Match(
                Some: turn => turn.Number,
                None: () => 0
            );
        }

        /// <summary>
        /// Marks that a valid action has been performed, enabling turn advancement.
        /// This is called after successful moves, pattern execution, etc.
        /// Can only be called once per turn to enforce one-action-per-turn rule.
        /// </summary>
        public void MarkActionPerformed()
        {
            lock (_lock)
            {
                // Only allow one action per turn
                if (!_actionPerformed)
                {
                    _actionPerformed = true;
                    // Debug logging removed - action marking is internal implementation detail
                }
                // Silently ignore if already performed - this is expected behavior
                // Note: Silently ignore if already performed to avoid exceptions
                // in complex command chains
            }
        }

        /// <summary>
        /// Checks if an action has been performed in the current turn.
        /// Used to enforce one-action-per-turn limitation.
        /// </summary>
        /// <returns>True if an action was performed this turn</returns>
        public bool HasActionBeenPerformed() => _actionPerformed;

        /// <summary>
        /// Loads turn state from external source (save data, persistence layer).
        /// Validates the turn and action state before setting internal state.
        /// </summary>
        /// <param name="turn">The turn to restore</param>
        /// <param name="actionPerformed">Whether an action was performed in this turn</param>
        /// <returns>Fin&lt;Turn&gt; with loaded turn or validation error</returns>
        public Fin<Core.Domain.Turn.Turn> LoadState(Core.Domain.Turn.Turn turn, bool actionPerformed)
        {
            if (turn == null)
                return FinFail<Core.Domain.Turn.Turn>(Error.New("Cannot load null turn"));

            if (turn.Number < 1)
                return FinFail<Core.Domain.Turn.Turn>(Error.New($"Invalid turn number: {turn.Number}"));

            lock (_lock)
            {
                _currentTurn = Some(turn);
                _actionPerformed = actionPerformed;
                return turn;
            }
        }

        /// <summary>
        /// Gets a summary of the current turn state for debugging and logging.
        /// </summary>
        /// <returns>Human-readable turn state summary</returns>
        public string GetTurnStateSummary()
        {
            return _currentTurn.Match(
                Some: turn => $"Turn {turn.Number}, Action: {(_actionPerformed ? "Performed" : "Pending")}",
                None: () => "Turn system not initialized"
            );
        }
    }
}