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

public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    private readonly IPositionIsValidRule _positionValidRule;
    private readonly IPositionIsEmptyRule _positionEmptyRule;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly ILogger<PlaceBlockCommandHandler> _logger;
    
    public PlaceBlockCommandHandler(
        IPositionIsValidRule positionValidRule,
        IPositionIsEmptyRule positionEmptyRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        ILogger<PlaceBlockCommandHandler> logger)
    {
        _positionValidRule = positionValidRule;
        _positionEmptyRule = positionEmptyRule;
        _gridState = gridState;
        _simulation = simulation;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling PlaceBlockCommand for position {Position}", request.Position);
        
        return await (
            from validPosition in _positionValidRule.Validate(request.Position)
            from emptyPosition in _positionEmptyRule.Validate(request.Position)
            from block in CreateBlock(request)
            from placed in PlaceBlockInGrid(block)
            from effectQueued in QueueEffect(block)
            select Unit.Default
        ).AsTask();
    }
    
    private Fin<Domain.Block.Block> CreateBlock(PlaceBlockCommand request) =>
        FinSucc(new Domain.Block.Block
        {
            Id = request.BlockId,
            Position = request.Position,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        });
    
    private Fin<Unit> PlaceBlockInGrid(Domain.Block.Block block) =>
        _gridState.PlaceBlock(block);
    
    private Fin<Unit> QueueEffect(Domain.Block.Block block) =>
        _simulation.QueueEffect(new BlockPlacedEffect(
            block.Id,
            block.Position,
            block.Type,
            block.CreatedAt
        ));
}