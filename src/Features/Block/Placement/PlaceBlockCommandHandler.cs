using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Block.Placement.Rules;
using BlockLife.Core.Infrastructure.Extensions;
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
    private readonly IMediator _mediator;
    private readonly ILogger<PlaceBlockCommandHandler> _logger;
    
    public PlaceBlockCommandHandler(
        IPositionIsValidRule positionValidRule,
        IPositionIsEmptyRule positionEmptyRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        IMediator mediator,
        ILogger<PlaceBlockCommandHandler> logger)
    {
        _positionValidRule = positionValidRule;
        _positionEmptyRule = positionEmptyRule;
        _gridState = gridState;
        _simulation = simulation;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling PlaceBlockCommand for position {Position}", request.Position);
        
        var result = await (
            from validPosition in _positionValidRule.Validate(request.Position)
            from emptyPosition in _positionEmptyRule.Validate(request.Position)
            from block in CreateBlock(request)
            from placed in PlaceBlockInGrid(block)
            from effectQueued in QueueEffect(block)
            select block
        ).AsTask();
        
        return await result.Match(
            Succ: async block =>
            {
                // Create and publish notification directly (following MoveBlock pattern)
                var notification = new BlockPlacedNotification(
                    block.Id,
                    block.Position,
                    block.Type,
                    block.CreatedAt
                );
                
                var publishResult = await _mediator.Publish(notification, cancellationToken).ToFin("NOTIFICATION_PUBLISH_FAILED", "Failed to publish block placed notification");
                
                return publishResult.Match(
                    Succ: _ =>
                    {
                        _logger.LogDebug("PlaceBlockCommand completed successfully for position {Position}", request.Position);
                        return FinSucc(Unit.Default);
                    },
                    Fail: error =>
                    {
                        _logger.LogWarning("PlaceBlockCommand notification failed for position {Position}: {Error}", request.Position, error.Message);
                        return FinFail<Unit>(error);
                    }
                );
            },
            Fail: error =>
            {
                _logger.LogWarning("PlaceBlockCommand failed for position {Position}: {Error}", request.Position, error.Message);
                return Task.FromResult(FinFail<Unit>(error));
            }
        );
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