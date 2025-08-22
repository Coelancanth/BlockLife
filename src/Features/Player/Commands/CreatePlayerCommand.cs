using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Player;
using System;

namespace BlockLife.Features.Player.Commands
{
    /// <summary>
    /// Command to create a new player with initial state.
    /// Replaces any existing player in single-player mode.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record CreatePlayerCommand : ICommand<PlayerState>
    {
        /// <summary>
        /// Display name for the new player.
        /// </summary>
        public required string PlayerName { get; init; }

        /// <summary>
        /// Creates a new CreatePlayerCommand with the specified player name.
        /// </summary>
        public static CreatePlayerCommand Create(string playerName) =>
            new()
            {
                PlayerName = playerName
            };
    }
}