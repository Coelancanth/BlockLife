using BlockLife.Core.Features.Turn;
using BlockLife.godot_project.scenes.Main;
using Godot;
using System;

namespace BlockLife.godot_project.features.turn;

/// <summary>
/// Godot UI node for displaying turn information.
/// Implements ITurnDisplayView to work with TurnDisplayPresenter in MVP pattern.
/// Shows current turn number and status updates.
/// </summary>
public partial class TurnDisplay : Control, ITurnDisplayView
{
    private TurnDisplayPresenter? _presenter;
    private Label? _turnCounterLabel;
    private Label? _turnStatusLabel;

    public override void _Ready()
    {
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("TurnDisplay _Ready called");

        // Get UI components
        _turnCounterLabel = GetNode<Label>("VBoxContainer/TurnCounter");
        _turnStatusLabel = GetNode<Label>("VBoxContainer/TurnStatus");

        if (_turnCounterLabel == null || _turnStatusLabel == null)
        {
            logger?.Error("TurnDisplay: Required child nodes not found. Expected VBoxContainer with TurnCounter and TurnStatus labels");
            return;
        }

        // Initialize presenter using the factory pattern - defer to avoid race condition
        CallDeferred(nameof(InitializePresenterWhenSceneRootReady));
    }

    /// <summary>
    /// Deferred presenter initialization to avoid race condition with SceneRoot async setup
    /// </summary>
    private void InitializePresenterWhenSceneRootReady()
    {
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        var logger = sceneRoot?.Logger?.ForContext("SourceContext", "UI");

        if (sceneRoot?.PresenterFactory == null)
        {
            logger?.Warning("TurnDisplay: SceneRoot or PresenterFactory not ready yet, retrying in 100ms");
            GetTree().CreateTimer(0.1).Timeout += InitializePresenterWhenSceneRootReady;
            return;
        }

        try
        {
            // Create presenter using the generic factory pattern
            _presenter = sceneRoot.PresenterFactory.Create<TurnDisplayPresenter, ITurnDisplayView>(this);
            _presenter?.Initialize();
            logger?.Information("TurnDisplay presenter initialized successfully");
        }
        catch (Exception ex)
        {
            logger?.Error(ex, "TurnDisplay: Failed to initialize presenter");
        }
    }

    public override void _ExitTree()
    {
        // Clean up presenter to prevent memory leaks
        _presenter?.Dispose();
        _presenter = null;
    }

    // Implementation of ITurnDisplayView interface

    /// <summary>
    /// Updates the displayed turn information when a new turn starts.
    /// </summary>
    public void DisplayTurnStart(BlockLife.Core.Domain.Turn.Turn turn)
    {
        // Must be called from main thread
        CallDeferred(nameof(UpdateTurnCounterDeferred), turn.Number);
        CallDeferred(nameof(UpdateTurnStatusDeferred), $"Turn {turn.Number} started");
        
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("TurnDisplay: Turn {TurnNumber} started", turn.Number);
    }

    /// <summary>
    /// Updates the displayed turn information when a turn ends.
    /// </summary>
    public void DisplayTurnEnd(BlockLife.Core.Domain.Turn.Turn turn)
    {
        // Must be called from main thread
        CallDeferred(nameof(UpdateTurnStatusDeferred), $"Turn {turn.Number} ended");
        
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("TurnDisplay: Turn {TurnNumber} ended", turn.Number);
    }

    /// <summary>
    /// Updates the current turn display without transition effects.
    /// </summary>
    public void UpdateCurrentTurn(BlockLife.Core.Domain.Turn.Turn turn)
    {
        // Must be called from main thread
        CallDeferred(nameof(UpdateTurnCounterDeferred), turn.Number);
        CallDeferred(nameof(UpdateTurnStatusDeferred), "");
        
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("TurnDisplay: Current turn updated to {TurnNumber}", turn.Number);
    }

    /// <summary>
    /// Shows an error related to turn operations.
    /// </summary>
    public void ShowTurnError(string errorMessage)
    {
        // Must be called from main thread
        CallDeferred(nameof(UpdateTurnStatusDeferred), $"Error: {errorMessage}");
        
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Warning("TurnDisplay: Turn error - {ErrorMessage}", errorMessage);
    }

    // Thread-safe deferred methods for UI updates

    private void UpdateTurnCounterDeferred(int turnNumber)
    {
        if (_turnCounterLabel != null)
        {
            _turnCounterLabel.Text = $"Turn: {turnNumber}";
        }
    }

    private void UpdateTurnStatusDeferred(string status)
    {
        if (_turnStatusLabel != null)
        {
            _turnStatusLabel.Text = status;
        }
    }
}