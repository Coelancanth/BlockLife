using System;

namespace BlockLife.Core.Presentation;

/// <summary>
/// Base class for all Presenters. It provides a strongly-typed reference to the View interface
/// and standard lifecycle hooks.
/// </summary>
/// <typeparam name="TViewInterface">The interface of the view this presenter manages.</typeparam>
public abstract class PresenterBase<TViewInterface> : IDisposable where TViewInterface : class
{
    protected TViewInterface View { get; }

    protected PresenterBase(TViewInterface view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <summary>
    /// Called by the PresenterLifecycleManager after the presenter is created and assigned to the view.
    /// Use this for subscriptions, getting Godot node references from the View, and initial setup.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called by the PresenterLifecycleManager when the view is exiting the tree.
    /// Use this for unsubscribing from events and releasing resources.
    /// </summary>
    public virtual void Dispose() { }
}
