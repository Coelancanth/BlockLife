using MediatR;

namespace BlockLife.Core.Features.Turn.Effects
{
    /// <summary>
    /// Notification published when a new turn has started.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record TurnStartNotification : INotification
    {
        /// <summary>
        /// The turn that has just started.
        /// </summary>
        public required Domain.Turn.Turn Turn { get; init; }

        /// <summary>
        /// Creates a new TurnStartNotification with the specified turn.
        /// </summary>
        public static TurnStartNotification Create(Domain.Turn.Turn turn) =>
            new()
            {
                Turn = turn
            };
    }
}