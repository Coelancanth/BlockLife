using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Notifications;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Application.Simulation;

public class SimulationManager : ISimulationManager
{
    private readonly IMediator _mediator;
    private readonly ILogger<SimulationManager> _logger;
    private readonly Queue<object> _effectQueue = new();

    public SimulationManager(IMediator mediator, ILogger<SimulationManager> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Fin<Unit> QueueEffect<TEffect>(TEffect effect) where TEffect : class
    {
        try
        {
            _effectQueue.Enqueue(effect);
            _logger.LogDebug("Queued effect of type {EffectType}", typeof(TEffect).Name);
            return FinSucc(Unit.Default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue effect");
            return FinFail<Unit>(LanguageExt.Common.Error.New("QUEUE_EFFECT_FAILED", ex.Message));
        }
    }

    public async Task ProcessQueuedEffectsAsync()
    {
        while (_effectQueue.Count > 0)
        {
            var effect = _effectQueue.Dequeue();

            try
            {
                switch (effect)
                {
                    case BlockPlacedEffect blockPlaced:
                        await ProcessBlockPlacedEffect(blockPlaced);
                        break;
                    case BlockRemovedEffect blockRemoved:
                        await ProcessBlockRemovedEffect(blockRemoved);
                        break;
                    default:
                        _logger.LogWarning("Unknown effect type: {EffectType}", effect.GetType().Name);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process effect of type {EffectType}", effect.GetType().Name);
            }
        }
    }

    public async Task PublishNotificationAsync<TNotification>(TNotification notification)
        where TNotification : INotification
    {
        await _mediator.Publish(notification);
        _logger.LogDebug("Published notification of type {NotificationType}", typeof(TNotification).Name);
    }

    public bool HasPendingEffects => _effectQueue.Count > 0;
    public int PendingEffectCount => _effectQueue.Count;

    private async Task ProcessBlockPlacedEffect(BlockPlacedEffect effect)
    {
        // Trace: ProcessBlockPlacedEffect called for block {BlockId} at {Position}

        var notification = new BlockPlacedNotification(
            effect.BlockId,
            effect.Position,
            effect.Type,
            effect.PlacedAt
        );

        await PublishNotificationAsync(notification);
        // Trace: BlockPlacedNotification published for block {BlockId}
    }

    private async Task ProcessBlockRemovedEffect(BlockRemovedEffect effect)
    {
        var notification = new BlockRemovedNotification(
            effect.BlockId,
            effect.Position,
            effect.Type,
            effect.RemovedAt
        );

        await PublishNotificationAsync(notification);
    }
}
