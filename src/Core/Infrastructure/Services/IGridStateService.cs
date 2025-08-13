using BlockLife.Core.Domain.Common;
using LanguageExt;
using LanguageExt.Common;
using System;
using System.Collections.Generic;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for managing the state of the game grid.
    /// Provides operations for block placement, removal, and querying.
    /// </summary>
    public interface IGridStateService
    {
        /// <summary>
        /// Places a block at the specified position.
        /// </summary>
        /// <param name="block">The block to place</param>
        /// <returns>Success or failure with error details</returns>
        Fin<Unit> PlaceBlock(Domain.Block.Block block);

        /// <summary>
        /// Removes a block from the specified position.
        /// </summary>
        /// <param name="position">The position to remove the block from</param>
        /// <returns>The removed block, or failure if no block exists at that position</returns>
        Fin<Unit> RemoveBlock(Vector2Int position);

        /// <summary>
        /// Removes a block by its ID.
        /// </summary>
        /// <param name="blockId">The ID of the block to remove</param>
        /// <returns>The removed block, or failure if the block doesn't exist</returns>
        Fin<Unit> RemoveBlock(Guid blockId);

        /// <summary>
        /// Moves a block from one position to another.
        /// </summary>
        /// <param name="fromPosition">Current position of the block</param>
        /// <param name="toPosition">New position for the block</param>
        /// <returns>The updated block, or failure if the move is invalid</returns>
        Fin<Domain.Block.Block> MoveBlock(Vector2Int fromPosition, Vector2Int toPosition);

        /// <summary>
        /// Moves a block by ID to a new position.
        /// </summary>
        /// <param name="blockId">ID of the block to move</param>
        /// <param name="toPosition">New position for the block</param>
        /// <returns>The updated block, or failure if the move is invalid</returns>
        Fin<Domain.Block.Block> MoveBlock(Guid blockId, Vector2Int toPosition);

        /// <summary>
        /// Gets the block at the specified position.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>The block at that position, or None if empty</returns>
        Option<Domain.Block.Block> GetBlockAt(Vector2Int position);

        /// <summary>
        /// Gets a block by its ID.
        /// </summary>
        /// <param name="blockId">The ID of the block</param>
        /// <returns>The block with that ID, or None if not found</returns>
        Option<Domain.Block.Block> GetBlockById(Guid blockId);

        /// <summary>
        /// Checks if a position is empty (no block present).
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if empty, false if occupied</returns>
        bool IsPositionEmpty(Vector2Int position);
        
        /// <summary>
        /// Checks if a position is occupied (has a block present).
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if occupied, false if empty</returns>
        bool IsPositionOccupied(Vector2Int position);

        /// <summary>
        /// Checks if a position is within the valid grid bounds.
        /// </summary>
        /// <param name="position">The position to validate</param>
        /// <returns>True if valid, false if out of bounds</returns>
        bool IsValidPosition(Vector2Int position);

        /// <summary>
        /// Gets all blocks currently on the grid.
        /// </summary>
        /// <returns>Read-only collection of all blocks</returns>
        IReadOnlyCollection<Domain.Block.Block> GetAllBlocks();

        /// <summary>
        /// Gets all blocks adjacent to the specified position.
        /// </summary>
        /// <param name="position">The center position</param>
        /// <param name="includeOccupied">Whether to include positions with blocks</param>
        /// <returns>Collection of adjacent blocks</returns>
        IReadOnlyCollection<Domain.Block.Block> GetAdjacentBlocks(Vector2Int position, bool includeOccupied = true);

        /// <summary>
        /// Gets all blocks of a specific type.
        /// </summary>
        /// <param name="blockType">The type to filter by</param>
        /// <returns>Collection of blocks of the specified type</returns>
        IReadOnlyCollection<Domain.Block.Block> GetBlocksByType(Domain.Block.BlockType blockType);

        /// <summary>
        /// Clears all blocks from the grid.
        /// </summary>
        void ClearGrid();

        /// <summary>
        /// Gets the current dimensions of the grid.
        /// </summary>
        /// <returns>Width and height of the grid</returns>
        Vector2Int GetGridDimensions();
    }
}