using LanguageExt;
using MediatR;
using System.Threading.Tasks;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Application.Simulation;

public interface ISimulationManager
{
    // Effect Management
    Fin<Unit> QueueEffect<TEffect>(TEffect effect) where TEffect : class;
    Task ProcessQueuedEffectsAsync();

    // Notification Publishing
    Task PublishNotificationAsync<TNotification>(TNotification notification)
        where TNotification : INotification;

    // State Queries
    bool HasPendingEffects { get; }
    int PendingEffectCount { get; }
}
