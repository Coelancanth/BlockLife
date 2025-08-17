using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Drag.Commands
{
    /// <summary>
    /// Command to initiate dragging a block from its current position.
    /// This marks the beginning of a drag operation.
    /// </summary>
    public sealed record StartDragCommand : ICommand, IRequest<Fin<LanguageExt.Unit>>
    {
        /// <summary>
        /// The unique identifier of the block to start dragging.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// The initial mouse/pointer position when drag starts (in grid coordinates).
        /// </summary>
        public required Vector2Int StartPosition { get; init; }

        /// <summary>
        /// Creates a new StartDragCommand with the specified parameters.
        /// </summary>
        public static StartDragCommand Create(Guid blockId, Vector2Int startPosition) =>
            new()
            {
                BlockId = blockId,
                StartPosition = startPosition
            };
    }
}