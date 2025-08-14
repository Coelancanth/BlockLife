using BlockLife.Core;
using BlockLife.Core.Infrastructure.Logging;
using BlockLife.Core.Presentation;
using BlockLife.Godot.Infrastructure.Logging;
using BlockLife.Godot.Resources;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace BlockLife.Godot.Scenes;

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
        try
        {
            InitializeApplicationSafely();
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

    private void InitializeApplicationSafely()
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
}
