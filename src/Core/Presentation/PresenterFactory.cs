using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockLife.Core.Presentation;

/// <summary>
/// Defines the contract for the factory responsible for creating Presenter instances.
/// This is the bridge between Godot-managed Views and DI-managed Presenters.
/// </summary>
public interface IPresenterFactory
{
    /// <summary>
    /// Creates an instance of a Presenter, injecting its View interface dependency and any other services from the DI container.
    /// </summary>
    TPresenter Create<TPresenter, TViewInterface>(TViewInterface view)
        where TPresenter : PresenterBase<TViewInterface>
        where TViewInterface : class;
}

/// <summary>
/// A robust implementation of IPresenterFactory using Microsoft's DI extensions.
/// This class is the critical bridge between Godot's instantiation lifecycle (for Views)
/// and our DI container's lifecycle (for Presenters and Services).
/// It must be registered as a singleton in the DI container.
/// </summary>
public class PresenterFactory : IPresenterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PresenterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public TPresenter Create<TPresenter, TViewInterface>(TViewInterface view)
        where TPresenter : PresenterBase<TViewInterface>
        where TViewInterface : class
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view), "A valid view instance is required to create a presenter.");
        }

        // ActivatorUtilities allows us to create an instance of TPresenter
        // by resolving its dependencies from the IServiceProvider, while also
        // manually providing the 'view' parameter, which is not in the DI container.
        try
        {
            return ActivatorUtilities.CreateInstance<TPresenter>(_serviceProvider, view);
        }
        catch (InvalidOperationException ex)
        {
            // This catch block is crucial for debugging DI issues.
            throw new InvalidOperationException(
                $"Failed to create presenter '{typeof(TPresenter).Name}'. " +
                $"This is likely due to a missing service registration for one of its dependencies in the DI container. " +
                $"Check the DI setup in your GameStrapper. Inner exception: {ex.Message}", ex);
        }
    }
}
