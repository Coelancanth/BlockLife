using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Player;
using System;

namespace BlockLife.Core.Features.Player.Queries
{
    /// <summary>
    /// Query to retrieve the current player state.
    /// Used for UI presentation and game state validation.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record GetCurrentPlayerQuery : IQuery<PlayerState>
    {
        /// <summary>
        /// Creates a new GetCurrentPlayerQuery instance.
        /// </summary>
        public static GetCurrentPlayerQuery Create() => new();
    }
}