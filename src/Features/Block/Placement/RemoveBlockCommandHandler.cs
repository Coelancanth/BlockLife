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

public class RemoveBlockCommandHandler : IRequestHandler<RemoveBlockCommand, Fin<Unit>>
{
    private readonly IBlockExistsRule _blockExistsRule;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly IMediator _mediator;
    private readonly ILogger<RemoveBlockCommandHandler> _logger;
    
    public RemoveBlockCommandHandler(
        IBlockExistsRule blockExistsRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        IMediator mediator,
        ILogger<RemoveBlockCommandHandler> logger)
    {
        _blockExistsRule = blockExistsRule;
        _gridState = gridState;
        _simulation = simulation;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(RemoveBlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling RemoveBlockCommand for position {Position}", request.Position);
        
        var result = await (
            from block in _blockExistsRule.Validate(request.Position)
            from removed in RemoveBlockFromGrid(request.Position)
            from effectQueued in QueueEffect(block)
            select block
        ).AsTask();
        
        return await result.Match(
            Succ: async block =>
            {
                var processResult = await ProcessQueuedEffects();
                
                return await processResult.Match(
                    Succ: async _ =>
                    {
                        // Create and publish notification after successful effect processing  
                        var notification = new BlockRemovedNotification(
                            block.Id,
                            block.Position,
                            block.Type,
                            DateTime.UtcNow
                        );
                        
                        var publishResult = await _mediator.Publish(notification, cancellationToken).ToFin("NOTIFICATION_PUBLISH_FAILED", "Failed to publish block removed notification");
                        
                        return publishResult.Match(
                            Succ: _ =>
                            {
                                _logger.LogDebug("RemoveBlockCommand completed successfully for position {Position}", request.Position);
                                return FinSucc(Unit.Default);
                            },
                            Fail: error =>
                            {
                                _logger.LogWarning("RemoveBlockCommand notification failed for position {Position}: {Error}", request.Position, error.Message);
                                return FinFail<Unit>(error);
                            }
                        );
                    },
                    Fail: error =>
                    {
                        _logger.LogWarning("RemoveBlockCommand effect processing failed for position {Position}: {Error}", request.Position, error.Message);
                        return Task.FromResult(FinFail<Unit>(error));
                    }
                );
            },
            Fail: error => Task.FromResult(FinFail<Unit>(error))
        );
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
    
    private async Task<Fin<Unit>> ProcessQueuedEffects()
    {
        var result = await _simulation.ProcessQueuedEffectsAsync().ToFin("PROCESS_EFFECTS_FAILED", "Failed to process queued effects");
        
        return result.Match(
            Succ: _ => result,
            Fail: error =>
            {
                _logger.LogError("Failed to process queued effects: {Error}", error.Message);
                return result;
            }
        );
    }
}

public class RemoveBlockByIdCommandHandler : IRequestHandler<RemoveBlockByIdCommand, Fin<Unit>>
{
    private readonly IBlockExistsRule _blockExistsRule;
    private readonly IGridStateService _gridState;
    private readonly ISimulationManager _simulation;
    private readonly IMediator _mediator;
    private readonly ILogger<RemoveBlockByIdCommandHandler> _logger;
    
    public RemoveBlockByIdCommandHandler(
        IBlockExistsRule blockExistsRule,
        IGridStateService gridState,
        ISimulationManager simulation,
        IMediator mediator,
        ILogger<RemoveBlockByIdCommandHandler> logger)
    {
        _blockExistsRule = blockExistsRule;
        _gridState = gridState;
        _simulation = simulation;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<Fin<Unit>> Handle(RemoveBlockByIdCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling RemoveBlockByIdCommand for block {BlockId}", request.BlockId);
        
        var result = await (
            from block in _blockExistsRule.Validate(request.BlockId)
            from removed in RemoveBlockFromGrid(request.BlockId)
            from effectQueued in QueueEffect(block)
            select block
        ).AsTask();
        
        return await result.Match(
            Succ: async block =>
            {
                var processResult = await ProcessQueuedEffectsForId();
                
                return await processResult.Match(
                    Succ: async _ =>
                    {
                        // Create and publish notification after successful effect processing  
                        var notification = new BlockRemovedNotification(
                            block.Id,
                            block.Position,
                            block.Type,
                            DateTime.UtcNow
                        );
                        
                        var publishResult = await _mediator.Publish(notification, cancellationToken).ToFin("NOTIFICATION_PUBLISH_FAILED", "Failed to publish block removed notification");
                        
                        return publishResult.Match(
                            Succ: _ =>
                            {
                                _logger.LogDebug("RemoveBlockByIdCommand completed successfully for block {BlockId}", request.BlockId);
                                return FinSucc(Unit.Default);
                            },
                            Fail: error =>
                            {
                                _logger.LogWarning("RemoveBlockByIdCommand notification failed for block {BlockId}: {Error}", request.BlockId, error.Message);
                                return FinFail<Unit>(error);
                            }
                        );
                    },
                    Fail: error =>
                    {
                        _logger.LogWarning("RemoveBlockByIdCommand effect processing failed for block {BlockId}: {Error}", request.BlockId, error.Message);
                        return Task.FromResult(FinFail<Unit>(error));
                    }
                );
            },
            Fail: error => Task.FromResult(FinFail<Unit>(error))
        );
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
    
    private async Task<Fin<Unit>> ProcessQueuedEffectsForId()
    {
        var result = await _simulation.ProcessQueuedEffectsAsync().ToFin("PROCESS_EFFECTS_FAILED", "Failed to process queued effects");
        
        return result.Match(
            Succ: _ => result,
            Fail: error =>
            {
                _logger.LogError("Failed to process queued effects: {Error}", error.Message);
                return result;
            }
        );
    }
}