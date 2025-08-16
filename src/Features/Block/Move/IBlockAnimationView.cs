using BlockLife.Core.Domain.Common;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Move
{
    /// <summary>
    /// Interface for block animation capabilities in the view layer.
    /// This interface defines the animation capability that views must implement
    /// to support animated block movement following the MVP pattern.
    /// </summary>
    public interface IBlockAnimationView
    {
        /// <summary>
        /// Animates the movement of a block from its current position to a new position.
        /// The Task return type is crucial for the animation queuing system.
        /// </summary>
        /// <param name="blockId">The unique identifier of the block to animate</param>
        /// <param name="fromPosition">The starting position of the animation</param>
        /// <param name="toPosition">The target position for the animation</param>
        /// <returns>A task that completes when the animation finishes</returns>
        Task AnimateMoveAsync(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition);

        /// <summary>
        /// Immediately updates a block's position without animation.
        /// Used for instant updates or error recovery scenarios.
        /// </summary>
        /// <param name="blockId">The unique identifier of the block</param>
        /// <param name="position">The new position for the block</param>
        /// <returns>A task that completes when the position is updated</returns>
        Task SetBlockPositionAsync(Guid blockId, Vector2Int position);

        /// <summary>
        /// Checks if a specific block is currently animating.
        /// </summary>
        /// <param name="blockId">The unique identifier of the block</param>
        /// <returns>True if the block is currently animating, false otherwise</returns>
        bool IsBlockAnimating(Guid blockId);

        /// <summary>
        /// Cancels any ongoing animation for a specific block.
        /// </summary>
        /// <param name="blockId">The unique identifier of the block</param>
        /// <returns>A task that completes when the animation is cancelled</returns>
        Task CancelAnimationAsync(Guid blockId);
    }
}
