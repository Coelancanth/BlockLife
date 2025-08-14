using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using System;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Block.Placement.Rules;

public class BlockExistsRule : IBlockExistsRule
{
    private readonly IGridStateService _gridState;
    private readonly IBlockRepository _repository;

    public BlockExistsRule(IGridStateService gridState, IBlockRepository repository)
    {
        _gridState = gridState;
        _repository = repository;
    }

    public Fin<Domain.Block.Block> Validate(Vector2Int position) =>
        _gridState.GetBlockAt(position)
            .ToFin(Error.New("NO_BLOCK_AT_POSITION", $"No block exists at position {position}"));

    public Fin<Domain.Block.Block> Validate(Guid blockId)
    {
        // Use synchronous GridStateService method to avoid deadlock risk
        // Both IGridStateService and IBlockRepository now point to the same consolidated GridStateService
        return _gridState.GetBlockById(blockId)
            .ToFin(Error.New("BLOCK_NOT_FOUND", $"Block with ID {blockId} not found"));
    }
}
