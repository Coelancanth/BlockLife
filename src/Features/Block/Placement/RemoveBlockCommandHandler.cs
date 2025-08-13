using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Rules;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Features.Block.Placement;

public class RemoveBlockCommandHandler : IRequestHandler<RemoveBlockCommand, Fin<Unit>>
{
    private readonly IBlockExistsRule _blockExistsRule;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly ILogger<RemoveBlockCommandHandler> _logger;
    
    public RemoveBlockCommandHandler(
        IBlockExistsRule blockExistsRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        ILogger<RemoveBlockCommandHandler> logger)
    {
        _blockExistsRule = blockExistsRule;
        _gridState = gridState;
        _simulation = simulation;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(RemoveBlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling RemoveBlockCommand for position {Position}", request.Position);
        
        return await (
            from block in _blockExistsRule.Validate(request.Position)
            from removed in RemoveBlockFromGrid(request.Position)
            from effectQueued in QueueEffect(block)
            select Unit.Default
        ).AsTask();
    }
    
    private Fin<Unit> RemoveBlockFromGrid(Domain.Common.Vector2Int position) =>
        _gridState.RemoveBlock(position);
    
    private Fin<Unit> QueueEffect(Domain.Block.Block block) =>
        _simulation.QueueEffect(new BlockRemovedEffect(
            block.Id,
            block.Position,
            block.Type,
            DateTime.UtcNow
        ));
}

public class RemoveBlockByIdCommandHandler : IRequestHandler<RemoveBlockByIdCommand, Fin<Unit>>
{
    private readonly IBlockExistsRule _blockExistsRule;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly ILogger<RemoveBlockByIdCommandHandler> _logger;
    
    public RemoveBlockByIdCommandHandler(
        IBlockExistsRule blockExistsRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        ILogger<RemoveBlockByIdCommandHandler> logger)
    {
        _blockExistsRule = blockExistsRule;
        _gridState = gridState;
        _simulation = simulation;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(RemoveBlockByIdCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling RemoveBlockByIdCommand for block {BlockId}", request.BlockId);
        
        return await (
            from block in _blockExistsRule.Validate(request.BlockId)
            from removed in RemoveBlockFromGrid(request.BlockId)
            from effectQueued in QueueEffect(block)
            select Unit.Default
        ).AsTask();
    }
    
    private Fin<Unit> RemoveBlockFromGrid(Guid blockId) =>
        _gridState.RemoveBlock(blockId);
    
    private Fin<Unit> QueueEffect(Domain.Block.Block block) =>
        _simulation.QueueEffect(new BlockRemovedEffect(
            block.Id,
            block.Position,
            block.Type,
            DateTime.UtcNow
        ));
}