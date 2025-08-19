using BlockLife.Core.Presentation;
using BlockLife.godot_project.scenes.Main;
using Godot;

namespace BlockLife.godot_project.infrastructure;

/// <summary>
/// Extension methods for SceneRoot to simplify presenter creation
/// </summary>
public static class SceneRootExtensions
{
    /// <summary>
    /// Creates a presenter for the given view using the SceneRoot's PresenterFactory
    /// </summary>
    public static TPresenter CreatePresenterFor<TPresenter, TView>(this SceneRoot sceneRoot, TView view)
        where TPresenter : PresenterBase<TView>
        where TView : class
    {
        if (sceneRoot?.PresenterFactory == null)
        {
            GD.PrintErr($"Cannot create presenter {typeof(TPresenter).Name}: SceneRoot or PresenterFactory is null");
            throw new System.InvalidOperationException("SceneRoot is not properly initialized");
        }
        
        return sceneRoot.PresenterFactory.Create<TPresenter, TView>(view);
    }
}