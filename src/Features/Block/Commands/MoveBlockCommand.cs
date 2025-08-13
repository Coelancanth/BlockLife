using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;

namespace BlockLife.Core.Features.Block.Commands
{
    /// <summary>
    /// Command to move an existing block from its current position to a new position on the grid.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record MoveBlockCommand : ICommand
    {
        /// <summary>
        /// The unique identifier of the block to move.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The target position where the block should be moved.
        /// Note: FromPosition is intentionally omitted as IGridStateService is the source of truth.
        /// </summary>
        public required Vector2Int ToPosition { get; init; }

        /// <summary>
        /// Creates a new MoveBlockCommand with the specified parameters.
        /// </summary>
        public static MoveBlockCommand Create(Guid blockId, Vector2Int toPosition) =>
            new()
            {
                BlockId = blockId,
                ToPosition = toPosition
            };
    }
}