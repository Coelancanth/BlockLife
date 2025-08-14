using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using System;

namespace BlockLife.Core.Features.Block.Rules
{
    /// <summary>
    /// Validation rule for block placement operations.
    /// Ensures that block placement requests are valid according to game rules.
    /// </summary>
    public sealed class PlacementValidationRule
    {
        private readonly IGridStateService _gridStateService;

        public PlacementValidationRule(IGridStateService gridStateService)
        {
            _gridStateService = gridStateService ?? throw new ArgumentNullException(nameof(gridStateService));
        }

        /// <summary>
        /// Validates whether a block can be placed at the specified position.
        /// </summary>
        /// <param name="blockType">The type of block to place</param>
        /// <param name="position">The target position</param>
        /// <param name="blockId">Optional pre-specified block ID</param>
        /// <returns>Success or validation error</returns>
        public Fin<Unit> ValidatePlacement(BlockType blockType, Vector2Int position, Guid? blockId = null)
        {
            // Rule 1: Position must be within grid bounds
            if (!_gridStateService.IsValidPosition(position))
                return Error.New($"Position {position} is outside grid bounds");

            // Rule 2: Position must be empty
            if (!_gridStateService.IsPositionEmpty(position))
                return Error.New($"Position {position} is already occupied");

            // Rule 3: Block type must be valid
            if (!Enum.IsDefined<BlockType>(blockType))
                return Error.New($"Block type {blockType} is not a valid block type");

            // Rule 4: If block ID is specified, it must not already exist
            if (blockId.HasValue)
            {
                var existingBlock = _gridStateService.GetBlockById(blockId.Value);
                if (existingBlock.IsSome)
                    return Error.New($"Block with ID {blockId.Value} already exists");
            }

            // Rule 5: Validate block type constraints (for future expansion)
            var typeValidation = ValidateBlockTypeConstraints(blockType, position);
            if (typeValidation.IsFail)
                return typeValidation;

            return Unit.Default;
        }

        /// <summary>
        /// Validates block-type-specific constraints.
        /// This can be extended for future game rules that restrict certain types to certain areas.
        /// </summary>
        private Fin<Unit> ValidateBlockTypeConstraints(BlockType blockType, Vector2Int position)
        {
            // For F1 implementation, all primary block types can be placed anywhere
            if (blockType.IsPrimaryType())
                return Unit.Default;

            // Special/combination types should not be directly placeable in F1
            if (blockType.IsSpecialType())
                return Error.New($"Special block type {blockType.GetDisplayName()} cannot be directly placed. It must be created through block combinations.");

            return Unit.Default;
        }

        /// <summary>
        /// Validates the overall grid state for placement operations.
        /// This can include rules like maximum blocks per type, grid density limits, etc.
        /// </summary>
        public Fin<Unit> ValidateGridState()
        {
            var allBlocks = _gridStateService.GetAllBlocks();
            var gridDimensions = _gridStateService.GetGridDimensions();
            var totalCells = gridDimensions.X * gridDimensions.Y;

            // Rule: Grid cannot exceed 80% capacity (leaving room for combinations)
            if (allBlocks.Count >= totalCells * 0.8)
                return Error.New($"Grid is at maximum capacity ({allBlocks.Count}/{totalCells * 0.8:F0} blocks). Remove some blocks before placing new ones.");

            return Unit.Default;
        }
    }
}
