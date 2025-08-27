using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System;

namespace BlockLife.Core.Domain.Turn
{
    /// <summary>
    /// Immutable value object representing a single turn in the game.
    /// Each turn represents one complete action cycle where the player can perform actions.
    /// Following established domain patterns with LanguageExt functional types.
    /// </summary>
    public sealed record Turn
    {
        /// <summary>
        /// The sequential number of this turn, starting from 1.
        /// Turn numbers are always positive and increment by 1.
        /// </summary>
        public required int Number { get; init; }

        /// <summary>
        /// Timestamp when this turn was created.
        /// Used for tracking game session timing and analytics.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Default constructor required for record initialization with required properties.
        /// </summary>
        public Turn()
        {
        }

        /// <summary>
        /// Creates a new Turn with validation.
        /// Ensures turn number is valid and sets creation timestamp.
        /// </summary>
        /// <param name="number">The turn number (must be >= 1)</param>
        /// <returns>Fin&lt;Turn&gt; with validated turn or error</returns>
        public static Fin<Turn> Create(int number)
        {
            if (number < 1)
                return FinFail<Turn>(Error.New("Turn number must be 1 or greater"));

            return new Turn
            {
                Number = number,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates the initial turn (turn 1) for a new game.
        /// </summary>
        /// <returns>The first turn with number 1</returns>
        public static Turn CreateInitial() => new()
        {
            Number = 1,
            CreatedAt = DateTime.UtcNow
        };

        /// <summary>
        /// Creates the next turn in sequence.
        /// Increments the turn number by 1 and sets new timestamp.
        /// </summary>
        /// <returns>Fin&lt;Turn&gt; with next turn or overflow error</returns>
        public Fin<Turn> Next()
        {
            // Prevent integer overflow
            if (Number == int.MaxValue)
                return FinFail<Turn>(Error.New("Turn number overflow - maximum turns reached"));

            return new Turn
            {
                Number = Number + 1,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Checks if this turn comes before another turn.
        /// </summary>
        /// <param name="other">The turn to compare against</param>
        /// <returns>True if this turn number is less than the other</returns>
        public bool IsBefore(Turn other) => Number < other.Number;

        /// <summary>
        /// Checks if this turn comes after another turn.
        /// </summary>
        /// <param name="other">The turn to compare against</param>
        /// <returns>True if this turn number is greater than the other</returns>
        public bool IsAfter(Turn other) => Number > other.Number;

        /// <summary>
        /// Checks if this is the first turn of the game.
        /// </summary>
        public bool IsFirst => Number == 1;

        /// <summary>
        /// Calculates the number of turns between this and another turn.
        /// </summary>
        /// <param name="other">The turn to calculate distance to</param>
        /// <returns>Absolute difference in turn numbers</returns>
        public int DistanceTo(Turn other) => Math.Abs(Number - other.Number);

        public override string ToString() => $"Turn {Number} ({CreatedAt:yyyy-MM-dd HH:mm:ss})";
    }
}