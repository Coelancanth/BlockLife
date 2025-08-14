using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Infrastructure.Block;

public class InMemoryBlockRepository : IBlockRepository
{
    private readonly Dictionary<Guid, Domain.Block.Block> _blocks = new();
    private readonly Dictionary<Vector2Int, Guid> _positionIndex = new();
    private readonly ILogger<InMemoryBlockRepository> _logger;

    public InMemoryBlockRepository(ILogger<InMemoryBlockRepository> logger)
    {
        _logger = logger;
    }

    public Task<Option<Domain.Block.Block>> GetByIdAsync(Guid id) =>
        Task.FromResult(_blocks.TryGetValue(id, out var block) ? Some(block) : None);

    public Task<Option<Domain.Block.Block>> GetAtPositionAsync(Vector2Int position) =>
        Task.FromResult(
            _positionIndex.TryGetValue(position, out var id) && _blocks.TryGetValue(id, out var block)
                ? Some(block) : None
        );

    public Task<IReadOnlyList<Domain.Block.Block>> GetAllAsync() =>
        Task.FromResult(_blocks.Values.ToList() as IReadOnlyList<Domain.Block.Block>);

    public Task<IReadOnlyList<Domain.Block.Block>> GetInRegionAsync(Vector2Int topLeft, Vector2Int bottomRight)
    {
        var blocksInRegion = _blocks.Values
            .Where(block =>
                block.Position.X >= topLeft.X && block.Position.X <= bottomRight.X &&
                block.Position.Y >= topLeft.Y && block.Position.Y <= bottomRight.Y)
            .ToList();

        return Task.FromResult(blocksInRegion as IReadOnlyList<Domain.Block.Block>);
    }

    public Task<Fin<Unit>> AddAsync(Domain.Block.Block block)
    {
        if (_blocks.ContainsKey(block.Id))
            return Task.FromResult(FinFail<Unit>(Error.New("BLOCK_EXISTS", "Block already exists")));

        if (_positionIndex.ContainsKey(block.Position))
            return Task.FromResult(FinFail<Unit>(Error.New("POSITION_OCCUPIED", "Position is occupied")));

        _blocks[block.Id] = block;
        _positionIndex[block.Position] = block.Id;

        _logger.LogDebug("Added block {BlockId} to repository", block.Id);
        return Task.FromResult(FinSucc(Unit.Default));
    }

    public Task<Fin<Unit>> UpdateAsync(Domain.Block.Block block)
    {
        if (!_blocks.TryGetValue(block.Id, out var existingBlock))
            return Task.FromResult(FinFail<Unit>(Error.New("BLOCK_NOT_FOUND", "Block not found")));

        // Update position index if position changed
        if (existingBlock.Position != block.Position)
        {
            _positionIndex.Remove(existingBlock.Position);

            if (_positionIndex.ContainsKey(block.Position))
                return Task.FromResult(FinFail<Unit>(Error.New("POSITION_OCCUPIED", "New position is occupied")));

            _positionIndex[block.Position] = block.Id;
        }

        _blocks[block.Id] = block;

        _logger.LogDebug("Updated block {BlockId}", block.Id);
        return Task.FromResult(FinSucc(Unit.Default));
    }

    public Task<Fin<Unit>> RemoveAsync(Guid id)
    {
        if (!_blocks.TryGetValue(id, out var block))
            return Task.FromResult(FinFail<Unit>(Error.New("BLOCK_NOT_FOUND", "Block not found")));

        _blocks.Remove(id);
        _positionIndex.Remove(block.Position);

        _logger.LogDebug("Removed block {BlockId} from repository", id);
        return Task.FromResult(FinSucc(Unit.Default));
    }

    public Task<Fin<Unit>> RemoveAtPositionAsync(Vector2Int position)
    {
        if (!_positionIndex.TryGetValue(position, out var id))
            return Task.FromResult(FinFail<Unit>(Error.New("NO_BLOCK_AT_POSITION", "No block at position")));

        return RemoveAsync(id);
    }

    public Task<bool> ExistsAtPositionAsync(Vector2Int position) =>
        Task.FromResult(_positionIndex.ContainsKey(position));

    public Task<bool> ExistsAsync(Guid id) =>
        Task.FromResult(_blocks.ContainsKey(id));
}
