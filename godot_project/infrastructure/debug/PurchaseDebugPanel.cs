using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Player.Commands;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.godot_project.scenes.Main;
using Godot;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.godot_project.infrastructure.debug;

/// <summary>
/// F8 Debug Purchase Panel for VS_003B-4 Phase 4.
/// Provides developer access to test merge unlock purchases.
/// Integrates with PurchaseMergeUnlockCommand through MediatR.
/// </summary>
[GlobalClass]
public partial class PurchaseDebugPanel : Control
{
    // UI Components
    private Panel? _panel;
    private VBoxContainer? _content;
    private Label? _statusLabel;
    private Label? _playerInfoLabel;
    private Button? _buyT2Button;
    private Button? _buyT3Button;
    private Button? _buyT4Button;
    private Label? _feedbackLabel;

    // Services
    private IMediator? _mediator;
    private IPlayerStateService? _playerStateService;

    public override void _Ready()
    {
        // Manual positioning for debug panel
        CreateUI();
        
        // Get services from SceneRoot
        if (SceneRoot.Instance?.ServiceProvider != null)
        {
            _mediator = SceneRoot.Instance.ServiceProvider.GetRequiredService<IMediator>();
            _playerStateService = SceneRoot.Instance.ServiceProvider.GetRequiredService<IPlayerStateService>();
        }

        // Start hidden
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.F8)
            {
                TogglePanel();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void CreateUI()
    {
        // Main panel
        _panel = new Panel
        {
            Size = new Vector2I(350, 300),
            Position = new Vector2I(50, 50)
        };
        AddChild(_panel);

        // Content container
        _content = new VBoxContainer
        {
            Position = new Vector2I(10, 10),
            Size = new Vector2I(330, 280)
        };
        _panel.AddChild(_content);

        // Title
        var title = new Label
        {
            Text = "🔧 F8 DEBUG: Merge Unlock Purchases",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _content.AddChild(title);

        // Status info
        _statusLabel = new Label
        {
            Text = "VS_003B-4 Phase 4: Purchase System Testing",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Modulate = Colors.Gray
        };
        _content.AddChild(_statusLabel);

        // Player info
        _playerInfoLabel = new Label
        {
            Text = "Loading player info...",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _content.AddChild(_playerInfoLabel);

        // Purchase buttons
        _buyT2Button = new Button { Text = "Buy Tier 2 Unlock (100 💰)" };
        _buyT2Button.Pressed += () => PurchaseTierAsync(2);
        _content.AddChild(_buyT2Button);

        _buyT3Button = new Button { Text = "Buy Tier 3 Unlock (500 💰)" };
        _buyT3Button.Pressed += () => PurchaseTierAsync(3);
        _content.AddChild(_buyT3Button);

        _buyT4Button = new Button { Text = "Buy Tier 4 Unlock (2500 💰)" };
        _buyT4Button.Pressed += () => PurchaseTierAsync(4);
        _content.AddChild(_buyT4Button);

        // Feedback area
        _feedbackLabel = new Label
        {
            Text = "",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Modulate = Colors.White
        };
        _content.AddChild(_feedbackLabel);

        // Instructions
        var instructions = new Label
        {
            Text = "Instructions:\n• F8: Toggle this panel\n• Purchase unlocks to test merge system\n• Check grid to see merge behavior change",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Modulate = Colors.LightGray
        };
        _content.AddChild(instructions);
    }

    private void TogglePanel()
    {
        // Safety check - ensure services are available
        if (_mediator == null || _playerStateService == null)
        {
            GD.PrintErr("[PurchaseDebugPanel] ERROR: Services not available! Cannot open panel.");
            GD.PrintErr("_mediator: " + (_mediator != null ? "OK" : "NULL"));
            GD.PrintErr("_playerStateService: " + (_playerStateService != null ? "OK" : "NULL"));
            
            // Try to get services again from SceneRoot
            if (SceneRoot.Instance?.ServiceProvider != null)
            {
                try
                {
                    _mediator = SceneRoot.Instance.ServiceProvider.GetRequiredService<IMediator>();
                    _playerStateService = SceneRoot.Instance.ServiceProvider.GetRequiredService<IPlayerStateService>();
                    GD.Print("[PurchaseDebugPanel] Services recovered from SceneRoot");
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[PurchaseDebugPanel] Failed to get services: {ex.Message}");
                    return;
                }
            }
            else
            {
                GD.PrintErr("[PurchaseDebugPanel] SceneRoot or ServiceProvider is null");
                return;
            }
        }
        
        Visible = !Visible;
        if (Visible)
        {
            UpdatePlayerInfo();
        }
    }

    private async void PurchaseTierAsync(int tier)
    {
        if (_mediator == null)
        {
            ShowFeedback("❌ ERROR: MediatR not available", Colors.Red);
            return;
        }

        try
        {
            ShowFeedback($"⏳ Purchasing Tier {tier} unlock...", Colors.Yellow);

            var command = PurchaseMergeUnlockCommand.Create(tier);
            var result = await _mediator.Send(command);

            await result.Match(
                Succ: player =>
                {
                    ShowFeedback($"✅ SUCCESS: Tier {tier} unlock purchased! MaxUnlockedTier: {player.MaxUnlockedTier}", Colors.Green);
                    UpdatePlayerInfo(); // Refresh display
                    return Task.CompletedTask;
                },
                Fail: error =>
                {
                    ShowFeedback($"❌ FAILED: {error.Message}", Colors.Red);
                    return Task.CompletedTask;
                }
            );
        }
        catch (Exception ex)
        {
            ShowFeedback($"❌ EXCEPTION: {ex.Message}", Colors.Red);
        }
    }

    private void UpdatePlayerInfo()
    {
        if (_playerInfoLabel == null)
        {
            GD.PrintErr("[PurchaseDebugPanel] _playerInfoLabel is null - UI not created properly");
            return;
        }
        
        if (_playerStateService == null)
        {
            _playerInfoLabel.Text = "❌ PlayerStateService not available";
            return;
        }

        var currentPlayer = _playerStateService.GetCurrentPlayer();
        currentPlayer.Match(
            Some: player =>
            {
                var money = player.GetResource(ResourceType.Money);
                var tier = player.MaxUnlockedTier;
                var mergeUnlocked = player.IsMergeUnlocked();
                
                _playerInfoLabel.Text = $"Player: {player.Name}\n💰 Money: {money}\n🔓 Max Unlocked Tier: {tier}\n🔄 Merge Enabled: {mergeUnlocked}";
                
                // Update button states
                UpdateButtonStates(money, tier);
            },
            None: () =>
            {
                _playerInfoLabel.Text = "❌ No current player found";
            }
        );
    }

    private void UpdateButtonStates(int money, int maxUnlockedTier)
    {
        // T2 Button
        _buyT2Button!.Disabled = maxUnlockedTier >= 2;
        _buyT2Button.Text = maxUnlockedTier >= 2 ? "✅ Tier 2 UNLOCKED" : 
                           money >= 100 ? "Buy Tier 2 Unlock (100 💰)" : 
                           "Buy Tier 2 Unlock (100 💰) - INSUFFICIENT FUNDS";

        // T3 Button  
        _buyT3Button!.Disabled = maxUnlockedTier >= 3;
        _buyT3Button.Text = maxUnlockedTier >= 3 ? "✅ Tier 3 UNLOCKED" :
                           money >= 500 ? "Buy Tier 3 Unlock (500 💰)" :
                           "Buy Tier 3 Unlock (500 💰) - INSUFFICIENT FUNDS";

        // T4 Button
        _buyT4Button!.Disabled = maxUnlockedTier >= 4;
        _buyT4Button.Text = maxUnlockedTier >= 4 ? "✅ Tier 4 UNLOCKED" :
                           money >= 2500 ? "Buy Tier 4 Unlock (2500 💰)" :
                           "Buy Tier 4 Unlock (2500 💰) - INSUFFICIENT FUNDS";
    }

    private void ShowFeedback(string message, Color color)
    {
        if (_feedbackLabel != null)
        {
            _feedbackLabel.Text = message;
            _feedbackLabel.Modulate = color;
        }
        
        GD.Print($"[PurchaseDebugPanel] {message}");
    }
}