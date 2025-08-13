using BlockLife.Core.Domain.Common;
using LanguageExt;
using LanguageExt.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe implementation of IGridStateService using concurrent collections.
    /// Manages the authoritative state of the game grid.
    /// </summary>
    public sealed class GridStateService : IGridStateService
    {
        private readonly ConcurrentDictionary<Vector2Int, Domain.Block.Block> _blocksByPosition = new();
        private readonly ConcurrentDictionary<Guid, Domain.Block.Block> _blocksById = new();
        private readonly Vector2Int _gridDimensions;

        /// <summary>
        /// Initializes a new GridStateService with the specified dimensions.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        public GridStateService(int width = 10, int height = 10)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
            
            _gridDimensions = new Vector2Int(width, height);
        }

        public Fin<Unit> PlaceBlock(Domain.Block.Block block)
        {
            // Validate position
            if (!IsValidPosition(block.Position))
                return Error.New($"Position {block.Position} is outside grid bounds");

            // Check if position is empty
            if (!IsPositionEmpty(block.Position))
                return Error.New($"Position {block.Position} is already occupied");

            // Check if block ID is already in use
            if (_blocksById.ContainsKey(block.Id))
                return Error.New($"Block with ID {block.Id} already exists");

            // Place the block
            if (!_blocksByPosition.TryAdd(block.Position, block))
                return Error.New("Failed to place block due to concurrent modification");

            if (!_blocksById.TryAdd(block.Id, block))
            {
                // Rollback position placement
                _blocksByPosition.TryRemove(block.Position, out _);
                return Error.New("Failed to place block due to concurrent modification");
            }

            return Unit.Default;
        }

        public Fin<Unit> RemoveBlock(Vector2Int position)
        {
            if (!_blocksByPosition.TryRemove(position, out var block))
                return Error.New($"No block found at position {position}");

            // Also remove from ID index
            _blocksById.TryRemove(block.Id, out _);

            return Unit.Default;
        }

        public Fin<Unit> RemoveBlock(Guid blockId)
        {
            if (!_blocksById.TryRemove(blockId, out var block))
                return Error.New($"No block found with ID {blockId}");

            // Also remove from position index
            _blocksByPosition.TryRemove(block.Position, out _);

            return Unit.Default;
        }

        public Fin<Domain.Block.Block> MoveBlock(Vector2Int fromPosition, Vector2Int toPosition)
        {
            // Validate positions
            if (!IsValidPosition(fromPosition))
                return Error.New($"Source position {fromPosition} is outside grid bounds");

            if (!IsValidPosition(toPosition))
                return Error.New($"Target position {toPosition} is outside grid bounds");

            // Get the block to move
            if (!_blocksByPosition.TryGetValue(fromPosition, out var block))
                return Error.New($"No block found at position {fromPosition}");

            // Check if target position is empty
            if (!IsPositionEmpty(toPosition))
                return Error.New($"Target position {toPosition} is already occupied");

            // Create updated block
            var updatedBlock = block.MoveTo(toPosition);

            // Perform atomic update
            if (!_blocksByPosition.TryRemove(fromPosition, out _))
                return Error.New("Failed to remove block from source position");

            if (!_blocksByPosition.TryAdd(toPosition, updatedBlock))
            {
                // Rollback - put block back at original position
                _blocksByPosition.TryAdd(fromPosition, block);
                return Error.New("Failed to place block at target position");
            }

            // Update ID index
            _blocksById.TryUpdate(block.Id, updatedBlock, block);

            return updatedBlock;
        }

        public Fin<Domain.Block.Block> MoveBlock(Guid blockId, Vector2Int toPosition)
        {
            if (!_blocksById.TryGetValue(blockId, out var block))
                return Error.New($"No block found with ID {blockId}");

            return MoveBlock(block.Position, toPosition);
        }

        public Option<Domain.Block.Block> GetBlockAt(Vector2Int position) =>
            _blocksByPosition.TryGetValue(position, out var block) ? Option<Domain.Block.Block>.Some(block) : Option<Domain.Block.Block>.None;

        public Option<Domain.Block.Block> GetBlockById(Guid blockId) =>
            _blocksById.TryGetValue(blockId, out var block) ? Option<Domain.Block.Block>.Some(block) : Option<Domain.Block.Block>.None;

        public bool IsPositionEmpty(Vector2Int position) => !_blocksByPosition.ContainsKey(position);
        
        public bool IsPositionOccupied(Vector2Int position) => _blocksByPosition.ContainsKey(position);

        public bool IsValidPosition(Vector2Int position) =>
            position.X >= 0 && position.X < _gridDimensions.X &&
            position.Y >= 0 && position.Y < _gridDimensions.Y;

        public IReadOnlyCollection<Domain.Block.Block> GetAllBlocks() => _blocksById.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<Domain.Block.Block> GetAdjacentBlocks(Vector2Int position, bool includeOccupied = true)
        {
            var adjacentPositions = position.GetOrthogonallyAdjacentPositions();
            var result = new List<Domain.Block.Block>();

            foreach (var adjPosition in adjacentPositions)
            {
                if (!IsValidPosition(adjPosition)) continue;

                var blockOption = GetBlockAt(adjPosition);
                if (includeOccupied && blockOption.IsSome)
                {
                    result.Add(blockOption.IfNone(() => throw new InvalidOperationException()));
                }
            }

            return result.AsReadOnly();
        }

        public IReadOnlyCollection<Domain.Block.Block> GetBlocksByType(Domain.Block.BlockType blockType) =>
            _blocksById.Values.Where(b => b.Type == blockType).ToList().AsReadOnly();

        public void ClearGrid()
        {
            _blocksByPosition.Clear();
            _blocksById.Clear();
        }

        public Vector2Int GetGridDimensions() => _gridDimensions;
    }
}