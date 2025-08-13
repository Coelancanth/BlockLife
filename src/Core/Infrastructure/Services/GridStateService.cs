using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unit = LanguageExt.Unit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe implementation of IGridStateService and IBlockRepository using concurrent collections.
    /// Serves as the single source of truth for block state management.
    /// Consolidates dual state management to eliminate race conditions.
    /// </summary>
    public sealed class GridStateService : IGridStateService, IBlockRepository
    {
        private readonly ConcurrentDictionary<Vector2Int, Domain.Block.Block> _blocksByPosition = new();
        private readonly ConcurrentDictionary<Guid, Domain.Block.Block> _blocksById = new();
        private readonly Vector2Int _gridDimensions;
        private readonly ILogger<GridStateService>? _logger;

        // Grid dimension constraints to prevent unbounded memory usage
        private const int MaxGridWidth = 1000;
        private const int MaxGridHeight = 1000;

        /// <summary>
        /// Initializes a new GridStateService with the specified dimensions.
        /// </summary>
        /// <param name="width">Grid width (max 1000)</param>
        /// <param name="height">Grid height (max 1000)</param>
        /// <param name="logger">Logger for operations tracking</param>
        public GridStateService(int width = 10, int height = 10, ILogger<GridStateService>? logger = null)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive");
            if (width > MaxGridWidth) throw new ArgumentOutOfRangeException(nameof(width), $"Width cannot exceed {MaxGridWidth}");
            if (height > MaxGridHeight) throw new ArgumentOutOfRangeException(nameof(height), $"Height cannot exceed {MaxGridHeight}");
            
            _gridDimensions = new Vector2Int(width, height);
            _logger = logger;
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
            var adjacentPositions = position.GetOrthogonallyAdjacentPositions()
                .Where(IsValidPosition)
                .ToList();

            if (!includeOccupied)
                return new List<Domain.Block.Block>().AsReadOnly();

            // Optimized: Single LINQ query instead of N+1 dictionary lookups
            // Uses the concurrent dictionary's thread-safe enumeration
            var adjacentBlocks = _blocksByPosition
                .Where(kvp => adjacentPositions.Contains(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();

            return adjacentBlocks.AsReadOnly();
        }

        public IReadOnlyCollection<Domain.Block.Block> GetBlocksByType(Domain.Block.BlockType blockType) =>
            _blocksById.Values.Where(b => b.Type == blockType).ToList().AsReadOnly();

        public void ClearGrid()
        {
            _blocksByPosition.Clear();
            _blocksById.Clear();
        }

        public Vector2Int GetGridDimensions() => _gridDimensions;

        // IBlockRepository async methods implementation
        // Consolidates state management to eliminate dual-repository race conditions
        
        public Task<Option<Domain.Block.Block>> GetByIdAsync(Guid id) =>
            Task.FromResult(GetBlockById(id));
        
        public Task<Option<Domain.Block.Block>> GetAtPositionAsync(Vector2Int position) =>
            Task.FromResult(GetBlockAt(position));
        
        public Task<IReadOnlyList<Domain.Block.Block>> GetAllAsync() =>
            Task.FromResult(GetAllBlocks().ToList() as IReadOnlyList<Domain.Block.Block>);
        
        public Task<IReadOnlyList<Domain.Block.Block>> GetInRegionAsync(Vector2Int topLeft, Vector2Int bottomRight)
        {
            var blocksInRegion = _blocksById.Values
                .Where(block => 
                    block.Position.X >= topLeft.X && block.Position.X <= bottomRight.X &&
                    block.Position.Y >= topLeft.Y && block.Position.Y <= bottomRight.Y)
                .ToList();
            
            return Task.FromResult(blocksInRegion as IReadOnlyList<Domain.Block.Block>);
        }
        
        public Task<Fin<Unit>> AddAsync(Domain.Block.Block block)
        {
            _logger?.LogDebug("Adding block {BlockId} at position {Position}", block.Id, block.Position);
            var result = PlaceBlock(block);
            if (result.IsSucc)
                _logger?.LogDebug("Successfully added block {BlockId}", block.Id);
            return Task.FromResult(result);
        }
        
        public Task<Fin<Unit>> UpdateAsync(Domain.Block.Block block)
        {
            _logger?.LogDebug("Updating block {BlockId}", block.Id);
            
            if (!_blocksById.TryGetValue(block.Id, out var existingBlock))
                return Task.FromResult(FinFail<Unit>(Error.New("BLOCK_NOT_FOUND", "Block not found")));
            
            // If position changed, validate new position and update atomically
            if (existingBlock.Position != block.Position)
            {
                // Check new position is valid and empty
                if (!IsValidPosition(block.Position))
                    return Task.FromResult(FinFail<Unit>(Error.New("INVALID_POSITION", $"Position {block.Position} is outside grid bounds")));
                    
                if (!IsPositionEmpty(block.Position))
                    return Task.FromResult(FinFail<Unit>(Error.New("POSITION_OCCUPIED", "New position is occupied")));
                
                // Atomic update: remove from old position, add to new
                if (!_blocksByPosition.TryRemove(existingBlock.Position, out _))
                    return Task.FromResult(FinFail<Unit>(Error.New("CONCURRENT_MODIFICATION", "Failed to remove from old position")));
                
                if (!_blocksByPosition.TryAdd(block.Position, block))
                {
                    // Rollback: restore old position
                    _blocksByPosition.TryAdd(existingBlock.Position, existingBlock);
                    return Task.FromResult(FinFail<Unit>(Error.New("CONCURRENT_MODIFICATION", "Failed to add to new position")));
                }
            }
            
            // Update in ID index
            _blocksById.TryUpdate(block.Id, block, existingBlock);
            
            _logger?.LogDebug("Successfully updated block {BlockId}", block.Id);
            return Task.FromResult(FinSucc(Unit.Default));
        }
        
        public Task<Fin<Unit>> RemoveAsync(Guid id)
        {
            _logger?.LogDebug("Removing block {BlockId}", id);
            var result = RemoveBlock(id);
            if (result.IsSucc)
                _logger?.LogDebug("Successfully removed block {BlockId}", id);
            return Task.FromResult(result);
        }
        
        public Task<Fin<Unit>> RemoveAtPositionAsync(Vector2Int position)
        {
            _logger?.LogDebug("Removing block at position {Position}", position);
            var result = RemoveBlock(position);
            if (result.IsSucc)
                _logger?.LogDebug("Successfully removed block at position {Position}", position);
            return Task.FromResult(result);
        }
        
        public Task<bool> ExistsAtPositionAsync(Vector2Int position) =>
            Task.FromResult(IsPositionOccupied(position));
        
        public Task<bool> ExistsAsync(Guid id) =>
            Task.FromResult(_blocksById.ContainsKey(id));
    }
}