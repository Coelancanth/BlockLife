using System;
using System.Collections.Generic;
using BlockLife.Core;
using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Player.Commands;
using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Infrastructure.Logging;
using BlockLife.Core.Presentation;
// using BlockLife.godot_project.infrastructure.debug; // Removed - merge enabled by default
using BlockLife.godot_project.infrastructure.logging;
using BlockLife.godot_project.resources.settings;
using Godot;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using static LanguageExt.Prelude;

namespace BlockLife.godot_project.scenes.Main;

/// <summary>
/// **ENHANCED** SceneRoot with comprehensive error handling and cleanup.
/// 
/// Acts as the composition root bridging Godot with the C# DI world.
/// Features:
/// - Fallback-safe initialization that never crashes the application
/// - Proper resource cleanup to prevent memory leaks  
/// - LogSettings resource integration for easy debugging
/// - Comprehensive error handling with detailed diagnostics
/// - Singleton pattern enforcement with proper lifecycle management
/// 
/// CRITICAL SAFETY: This node ensures the application can always start,
/// even with configuration errors, providing diagnostics for debugging.
/// </summary>
public partial class SceneRoot : Node
{
    private static readonly object _initLock = new();
    public static SceneRoot? Instance { get; private set; }
    public IPresenterFactory? PresenterFactory { get; private set; }
    public ILogger? Logger { get; private set; } // Expose logger for Views
    public IServiceProvider? ServiceProvider => _serviceProvider; // Expose for dependency injection
    private IServiceProvider? _serviceProvider;
    private RichTextLabelSink? _richTextSink; // Store reference for proper disposal

    [Export]
    private LogSettings? _logSettings;
    [Export]
    private NodePath? _richTextLogLabelPath;

    public override void _EnterTree()
    {
        // --- 1. Enforce Singleton Pattern with thread-safe mutex protection ---
        lock (_initLock)
        {
            if (Instance != null)
            {
                GD.PrintErr("FATAL: A second SceneRoot was instantiated. Destroying self to prevent multiple DI containers.");
                QueueFree();
                return;
            }
            Instance = this;
        }
    }

    public override void _Ready()
    {
        // Use CallDeferred to handle async initialization properly in Godot
        CallDeferred(nameof(InitializeApplicationSafelyAsync));
    }

    private async void InitializeApplicationSafelyAsync()
    {
        try
        {
            await InitializeApplicationSafely();
        }
        catch (Exception ex)
        {
            // CRITICAL: Never crash on initialization - provide diagnostics and continue
            GD.PrintErr($"FATAL: SceneRoot initialization failed: {ex.Message}");
            GD.PrintErr($"Stack Trace: {ex.StackTrace}");

            // Create minimal fallback setup so the game can still run
            CreateFallbackSetup();
        }
    }

    private async System.Threading.Tasks.Task InitializeApplicationSafely()
    {
        // --- 2. Validate LogSettings Configuration ---
        if (_logSettings == null)
        {
            GD.Print("INFO: LogSettings resource not assigned. Creating default debug configuration.");
            _logSettings = new LogSettings()
            {
                DefaultLogLevel = LogEventLevel.Information,
                EnableRichTextInGodot = true,
                EnableFileLogging = false, // Disable file logging for debug
                MaxConsoleLines = 1000
            };
        }

        // --- 3. Prepare Godot-specific Inputs with Error Handling ---
        _richTextSink = null;
        if (_richTextLogLabelPath != null && !_richTextLogLabelPath.IsEmpty)
        {
            try
            {
                if (GetNode(_richTextLogLabelPath) is RichTextLabel label)
                {
                    _richTextSink = new RichTextLabelSink(label, _logSettings.EnableRichTextInGodot, _logSettings.MaxConsoleLines);
                    GD.Print($"In-game console initialized at: {_richTextLogLabelPath}");
                }
                else
                {
                    GD.PrintErr($"SceneRoot: Node at path '{_richTextLogLabelPath}' is not a RichTextLabel. In-game console disabled.");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SceneRoot: Failed to initialize RichTextLabel sink: {ex.Message}. In-game console disabled.");
            }
        }

        // --- 4. Initialize DI Container with Enhanced GameStrapper ---
        try
        {
            // Create GodotConsoleSink for console output (always)
            var godotConsoleSink = new GodotConsoleSink(
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                null);

            // Use legacy API with GodotConsoleSink for console output
            var masterSwitch = new LoggingLevelSwitch(_logSettings.DefaultLogLevel);
            var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();

            _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, godotConsoleSink, _richTextSink);
            GD.Print("DI Container initialized successfully with GodotConsoleSink for console output");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"CRITICAL: Failed to initialize DI container with new API: {ex.Message}");
            throw; // Will be caught by outer try-catch for fallback
        }

        // --- 5. Retrieve Services from DI Container ---
        try
        {
            PresenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
            Logger = _serviceProvider.GetRequiredService<ILogger>();

            // Confirm successful initialization
            Logger.ForContext("SourceContext", LogCategory.Core)
                  .Information("🚀 SceneRoot initialized successfully - Advanced Logger & GameStrapper active");

            Logger.ForContext("SourceContext", LogCategory.DI)
                  .Debug("DI Container validated and ready with {ServiceCount} registered services",
                         _serviceProvider.GetType().GetProperty("Services")?.GetValue(_serviceProvider) ?? "unknown");

            // --- 6. Initialize Default Player for Match-3 Game ---
            await InitializeDefaultPlayerAsync();
            
            // --- 7. Initialize Turn System ---
            await InitializeTurnSystemAsync();

            // Debug UI removed - merge now enabled by default
        }
        catch (Exception ex)
        {
            GD.PrintErr($"CRITICAL: Failed to retrieve services from DI container: {ex.Message}");
            throw;
        }
    }

    private void CreateFallbackSetup()
    {
        GD.PrintErr("Creating minimal fallback setup to prevent application crash...");

        try
        {
            // Create absolute minimal setup using legacy API as emergency fallback
            var masterSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
            var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
            var godotConsoleSink = new GodotConsoleSink(GodotSinkExtensions.DefaultGodotSinkOutputTemplate, null);

            _serviceProvider = GameStrapper.Initialize(masterSwitch, categorySwitches, godotConsoleSink, null);
            PresenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();

            GD.PrintErr("Fallback setup complete - Application can continue with limited functionality");
        }
        catch (Exception fallbackEx)
        {
            GD.PrintErr($"FATAL: Even fallback setup failed: {fallbackEx.Message}");
            GD.PrintErr("Application may not function correctly");
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            // --- CRITICAL: Comprehensive Cleanup to Prevent Memory Leaks ---
            try
            {
                // 1. Dispose of RichTextLabelSink to stop timers and prevent memory leaks
                if (_richTextSink is IDisposable disposableSink)
                {
                    disposableSink.Dispose();
                    _richTextSink = null;
                }

                // 2. Dispose of DI container and all registered services
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                    _serviceProvider = null;
                }

                // 3. Clear presenter factory reference
                PresenterFactory = null;

                GD.Print("SceneRoot cleanup completed successfully");
            }
            catch (Exception ex)
            {
                // CRITICAL: Cleanup should never crash the application
                GD.PrintErr($"WARNING: SceneRoot cleanup encountered error: {ex.Message}");
            }
            finally
            {
                // Always clear the instance to allow new SceneRoot creation
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Creates a default player for the match-3 game if no player exists.
    /// This ensures the attribute system has a player to display.
    /// </summary>
    private async System.Threading.Tasks.Task InitializeDefaultPlayerAsync()
    {
        try
        {
            var mediator = _serviceProvider?.GetRequiredService<IMediator>();
            if (mediator == null)
            {
                Logger?.Warning("MediatR not available - skipping player initialization");
                return;
            }

            Logger?.Information("Initializing default player for match-3 game");

            // Create default player with starting resources
            var command = CreatePlayerCommand.Create("Player");
            var result = await mediator.Send(command);

            // Give the new player starting resources and attributes for gameplay, then report success
            await result.Match(
                Succ: async player => 
                {
                    await GivePlayerStartingResourcesAsync(mediator, player);
                    Logger?.Information("Default player '{PlayerName}' created successfully with ID {PlayerId}", 
                        player.Name, player.Id);
                },
                Fail: error =>
                {
                    Logger?.Warning("Failed to create default player: {Error}", error.Message);
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            );
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Error during player initialization");
            // Don't throw - game can continue without player, it just won't show attributes
        }
    }

    /// <summary>
    /// Initializes the turn system by calling Reset() to create the initial turn.
    /// This must be called after the DI container is set up but before gameplay begins.
    /// </summary>
    private System.Threading.Tasks.Task InitializeTurnSystemAsync()
    {
        try
        {
            var turnManager = _serviceProvider?.GetRequiredService<ITurnManager>();
            if (turnManager == null)
            {
                Logger?.Warning("ITurnManager not available - skipping turn system initialization");
                return System.Threading.Tasks.Task.CompletedTask;
            }

            Logger?.Information("Initializing turn system");

            // Initialize the turn system by resetting to turn 1
            var result = turnManager.Reset();
            
            result.Match(
                Succ: turn =>
                {
                    Logger?.Information("Turn system initialized successfully - starting at Turn {TurnNumber}", turn.Number);
                },
                Fail: error =>
                {
                    Logger?.Error("Failed to initialize turn system: {Error}", error.Message);
                }
            );
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Exception during turn system initialization: {ErrorMessage}", ex.Message);
        }
        
        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Gives a newly created player starting resources and attributes for gameplay.
    /// Uses ApplyMatchRewardsCommand to add resources without breaking domain integrity.
    /// </summary>
    private async System.Threading.Tasks.Task GivePlayerStartingResourcesAsync(IMediator mediator, PlayerState player)
    {
        try
        {
            // Give starting resources
            var resourceCommand = ApplyMatchRewardsCommand.Create(
                Map(
                    (ResourceType.Money, 100),
                    (ResourceType.SocialCapital, 50)
                ),
                Map(
                    (AttributeType.Knowledge, 10),
                    (AttributeType.Health, 15),
                    (AttributeType.Happiness, 12),
                    (AttributeType.Energy, 20),
                    (AttributeType.Nutrition, 8),
                    (AttributeType.Fitness, 10),
                    (AttributeType.Mindfulness, 5),
                    (AttributeType.Creativity, 7)
                ),
                "Welcome bonus - starting resources for new player"
            );

            var result = await mediator.Send(resourceCommand);
            await result.Match(
                Succ: _ => 
                {
                    Logger?.Information("Starting resources and attributes granted to new player");
                    return System.Threading.Tasks.Task.CompletedTask;
                },
                Fail: error =>
                {
                    Logger?.Warning("Failed to grant starting resources: {Error}", error.Message);
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            );
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Error granting starting resources to player");
        }
    }

    // InitializeDebugUI method removed - merge now enabled by default, no F8 purchase panel needed
}
