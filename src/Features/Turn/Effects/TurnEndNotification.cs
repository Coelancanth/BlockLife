using MediatR;

namespace BlockLife.Core.Features.Turn.Effects
{
    /// <summary>
    /// Notification published when a turn has ended.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record TurnEndNotification : INotification
    {
        /// <summary>
        /// The turn that has just ended.
        /// </summary>
        public required Domain.Turn.Turn Turn { get; init; }

        /// <summary>
        /// Creates a new TurnEndNotification with the specified turn.
        /// </summary>
        public static TurnEndNotification Create(Domain.Turn.Turn turn) =>
            new()
            {
                Turn = turn
            };
    }
}