using BlockLife.Core.Application.Commands;
using LanguageExt;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Drag.Commands
{
    /// <summary>
    /// Command to cancel an ongoing drag operation.
    /// Triggered by ESC key or right-click during drag.
    /// </summary>
    public sealed record CancelDragCommand : ICommand, IRequest<Fin<LanguageExt.Unit>>
    {
        /// <summary>
        /// The unique identifier of the block being dragged.
        /// </summary>
        public required Guid BlockId { get; init; }

        /// <summary>
        /// Creates a new CancelDragCommand with the specified parameters.
        /// </summary>
        public static CancelDragCommand Create(Guid blockId) =>
            new()
            {
                BlockId = blockId
            };
    }
}