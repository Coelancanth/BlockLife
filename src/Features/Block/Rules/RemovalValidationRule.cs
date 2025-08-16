using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using System;

namespace BlockLife.Core.Features.Block.Rules
{
    /// <summary>
    /// Validation rule for block removal operations.
    /// Ensures that block removal requests are valid according to game rules.
    /// </summary>
    public sealed class RemovalValidationRule
    {
        private readonly IGridStateService _gridStateService;

        public RemovalValidationRule(IGridStateService gridStateService)
        {
            _gridStateService = gridStateService ?? throw new ArgumentNullException(nameof(gridStateService));
        }

        /// <summary>
        /// Validates whether a block can be removed from the specified position.
        /// </summary>
        /// <param name="position">The position to remove from</param>
        /// <returns>Success or validation error</returns>
        public Fin<Unit> ValidateRemovalByPosition(Vector2Int position)
        {
            // Rule 1: Position must be within grid bounds
            if (!_gridStateService.IsValidPosition(position))
                return Error.New($"Position {position} is outside grid bounds");

            // Rule 2: Position must contain a block
            var block = _gridStateService.GetBlockAt(position);
            if (block.IsNone)
                return Error.New($"No block found at position {position}");

            // Rule 3: Validate block-specific removal constraints
            return ValidateBlockRemovalConstraints(block.IfNone(() => throw new InvalidOperationException()));
        }

        /// <summary>
        /// Validates whether a block can be removed by its ID.
        /// </summary>
        /// <param name="blockId">The ID of the block to remove</param>
        /// <returns>Success or validation error</returns>
        public Fin<Unit> ValidateRemovalById(Guid blockId)
        {
            // Rule 1: Block must exist
            var block = _gridStateService.GetBlockById(blockId);
            if (block.IsNone)
                return Error.New($"No block found with ID {blockId}");

            // Rule 2: Validate block-specific removal constraints
            return ValidateBlockRemovalConstraints(block.IfNone(() => throw new InvalidOperationException()));
        }

        /// <summary>
        /// Validates block-specific constraints for removal.
        /// This can be extended for future game rules that prevent certain blocks from being removed.
        /// </summary>
        private Fin<Unit> ValidateBlockRemovalConstraints(Domain.Block.Block block)
        {
            // For F1 implementation, all blocks can be freely removed
            // Future rules might include:
            // - Locked blocks that cannot be removed
            // - Blocks that require certain conditions to be met
            // - Blocks that are part of active patterns/combinations

            // Rule: Check if block is currently involved in any active effects
            // (This would require integration with a future effects system)

            // Rule: Validate minimum block requirements
            // (For example, ensuring at least one block remains on the grid)
            var allBlocks = _gridStateService.GetAllBlocks();
            if (allBlocks.Count <= 1)
                return Error.New("Cannot remove the last block from the grid. At least one block must remain.");

            return Unit.Default;
        }

        /// <summary>
        /// Validates the overall grid state after a potential removal.
        /// This can include rules about maintaining grid connectivity, balance, etc.
        /// </summary>
        public Fin<Unit> ValidatePostRemovalState(Vector2Int removalPosition)
        {
            // For F1 implementation, we don't have complex state requirements
            // Future rules might include:
            // - Ensuring no blocks become isolated/disconnected
            // - Maintaining minimum blocks of each type
            // - Preserving certain patterns or structures

            return Unit.Default;
        }
    }
}
