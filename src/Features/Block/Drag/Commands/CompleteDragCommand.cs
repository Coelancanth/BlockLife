using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Drag.Commands
{
    /// <summary>
    /// Command to complete a drag operation by dropping the block at the target position.
    /// This finalizes the drag and moves the block to its new location.
    /// </summary>
    public sealed record CompleteDragCommand : ICommand, IRequest<Fin<LanguageExt.Unit>>
    {
        /// <summary>
        /// The unique identifier of the block being dragged.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The target position where the block should be dropped.
        /// </summary>
        public required Vector2Int DropPosition { get; init; }

        /// <summary>
        /// Creates a new CompleteDragCommand with the specified parameters.
        /// </summary>
        public static CompleteDragCommand Create(Guid blockId, Vector2Int dropPosition) =>
            new()
            {
                BlockId = blockId,
                DropPosition = dropPosition
            };
    }
}