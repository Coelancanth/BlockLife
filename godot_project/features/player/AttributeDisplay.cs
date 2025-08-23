using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Player.Presenters;
using BlockLife.Core.Features.Player.Views;
using BlockLife.godot_project.scenes.Main;
using Godot;
using LanguageExt;
using System;
using System.Threading.Tasks;

namespace BlockLife.godot_project.features.player;

/// <summary>
/// Godot UI node for displaying player attributes and resources.
/// Implements IAttributeView to work with AttributePresenter in MVP pattern.
/// Shows current player state and provides feedback for attribute changes.
/// </summary>
public partial class AttributeDisplay : Control, IAttributeView
{
    private AttributePresenter? _presenter;
    private Label? _playerNameLabel;
    private Label? _resourcesLabel;
    private Label? _attributesLabel;
    private Label? _feedbackLabel;
    private PlayerState? _pendingPlayerState;

    public override void _Ready()
    {
        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("AttributeDisplay _Ready called");

        // Get UI components
        _playerNameLabel = GetNode<Label>("VBoxContainer/PlayerName");
        _resourcesLabel = GetNode<Label>("VBoxContainer/Resources");
        _attributesLabel = GetNode<Label>("VBoxContainer/Attributes");
        _feedbackLabel = GetNode<Label>("VBoxContainer/Feedback");

        if (_playerNameLabel == null || _resourcesLabel == null || _attributesLabel == null || _feedbackLabel == null)
        {
            logger?.Error("AttributeDisplay: Required child nodes not found. Expected VBoxContainer with PlayerName, Resources, Attributes, and Feedback labels");
            return;
        }

        // Initialize presenter using the factory pattern
        var sceneRoot = GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot != null && sceneRoot.PresenterFactory != null)
        {
            _presenter = sceneRoot.PresenterFactory.Create<AttributePresenter, IAttributeView>(this);
            _presenter?.Initialize();
            logger?.Debug("AttributeDisplay presenter created and initialized successfully");
        }
        else
        {
            logger?.Warning("SceneRoot not found at /root/SceneRoot. Presenter creation skipped. This is expected in test scenarios.");
        }
    }

    public override void _ExitTree()
    {
        _presenter?.Dispose();
    }

    /// <summary>
    /// Updates the display with the current player state.
    /// Shows all resources and attributes with their current values.
    /// </summary>
    public async Task UpdateAttributeDisplayAsync(PlayerState playerState)
    {
        if (!IsInsideTree())
        {
            return;
        }

        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Updating attribute display for player {PlayerName}", playerState.Name);

        // Store the player state and update on main thread
        _pendingPlayerState = playerState;
        CallDeferred(nameof(UpdateDisplayOnMainThread));
        await Task.CompletedTask;
    }

    /// <summary>
    /// Shows feedback when attributes change due to match rewards.
    /// </summary>
    public async Task ShowAttributeChangesAsync(
        Map<ResourceType, int> resourceChanges,
        Map<AttributeType, int> attributeChanges,
        string? matchDescription = null)
    {
        if (!IsInsideTree())
        {
            return;
        }

        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Showing attribute changes feedback");

        var feedbackText = FormatChangesText(resourceChanges, attributeChanges, matchDescription);
        CallDeferred(nameof(ShowFeedbackOnMainThread), feedbackText);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Displays an error if player state could not be retrieved.
    /// </summary>
    public async Task ShowErrorAsync(string errorMessage)
    {
        if (!IsInsideTree())
        {
            return;
        }

        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Warning("Showing error in attribute display: {Error}", errorMessage);

        CallDeferred(nameof(ShowErrorOnMainThread), errorMessage);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Clears the attribute display when no player exists.
    /// </summary>
    public async Task ClearDisplayAsync()
    {
        if (!IsInsideTree())
        {
            return;
        }

        var logger = GetNodeOrNull<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Clearing attribute display");

        CallDeferred(nameof(ClearDisplayOnMainThread));
        await Task.CompletedTask;
    }

    private void UpdateDisplayOnMainThread()
    {
        if (_playerNameLabel == null || _resourcesLabel == null || _attributesLabel == null || _feedbackLabel == null || _pendingPlayerState == null)
        {
            return;
        }

        var playerState = _pendingPlayerState;

        // Update player name
        _playerNameLabel.Text = $"Player: {playerState.Name}";

        // Update resources
        var resourceText = "Resources:\n";
        foreach (var (resourceType, amount) in playerState.Resources)
        {
            resourceText += $"  {resourceType}: {amount}\n";
        }
        _resourcesLabel.Text = resourceText;

        // Update attributes
        var attributeText = "Attributes:\n";
        foreach (var (attributeType, level) in playerState.Attributes)
        {
            attributeText += $"  {attributeType}: {level}\n";
        }
        _attributesLabel.Text = attributeText;

        // Clear feedback when updating normal display
        _feedbackLabel.Text = "";

        // Clear the pending state
        _pendingPlayerState = null;
    }

    private void ShowFeedbackOnMainThread(string feedbackText)
    {
        if (_feedbackLabel != null)
        {
            _feedbackLabel.Text = feedbackText;
            _feedbackLabel.Modulate = Colors.Green;

            // Clear feedback after 3 seconds
            GetTree().CreateTimer(3.0).Connect("timeout", Callable.From(ClearFeedback));
        }
    }

    private void ShowErrorOnMainThread(string errorMessage)
    {
        if (_playerNameLabel == null || _resourcesLabel == null || _attributesLabel == null || _feedbackLabel == null)
        {
            return;
        }

        _playerNameLabel.Text = "Player: Error";
        _resourcesLabel.Text = "Resources: Unable to load";
        _attributesLabel.Text = "Attributes: Unable to load";
        _feedbackLabel.Text = $"Error: {errorMessage}";
        _feedbackLabel.Modulate = Colors.Red;
    }

    private void ClearDisplayOnMainThread()
    {
        if (_playerNameLabel == null || _resourcesLabel == null || _attributesLabel == null || _feedbackLabel == null)
        {
            return;
        }

        _playerNameLabel.Text = "Player: None";
        _resourcesLabel.Text = "Resources: No player";
        _attributesLabel.Text = "Attributes: No player";
        _feedbackLabel.Text = "";
    }

    private void ClearFeedback()
    {
        if (_feedbackLabel != null)
        {
            _feedbackLabel.Text = "";
        }
    }

    private string FormatChangesText(
        Map<ResourceType, int> resourceChanges,
        Map<AttributeType, int> attributeChanges,
        string? matchDescription)
    {
        var text = matchDescription ?? "Match rewards:";
        text += "\n";

        foreach (var (resourceType, change) in resourceChanges)
        {
            text += $"  {resourceType}: +{change}\n";
        }

        foreach (var (attributeType, change) in attributeChanges)
        {
            text += $"  {attributeType}: +{change}\n";
        }

        return text;
    }
}