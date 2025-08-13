using BlockLife.Core;
using BlockLife.Core.Infrastructure.Logging;
using BlockLife.Core.Presentation;
using BlockLife.Godot.Infrastructure.Logging;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace BlockLife.Godot.Scenes;

/// <summary>
/// The root node of the main scene. It acts as the composition root for the entire
/// application, bridging the Godot world with the C# DI world.
/// </summary>
public partial class SceneRoot : Node
{
    public static SceneRoot? Instance { get; private set; }
    public IPresenterFactory? PresenterFactory { get; private set; }
    private IServiceProvider? _serviceProvider;

    [Export]
    private NodePath? _logSettingsControllerPath;
    [Export]
    private NodePath? _richTextLogLabelPath;

    public override void _EnterTree()
    {
        // Enforce Singleton: This must be in _EnterTree to prevent multiple nodes from even entering the scene.
        if (Instance != null) 
        {
            GD.PrintErr("FATAL: A second SceneRoot was instantiated. Destroying self to prevent multiple DI containers.");
            QueueFree(); 
            return; 
        }
        Instance = this;
    }

    public override void _Ready()
    {
        // --- 2. Validate Configuration ---
        if (_logSettingsControllerPath is null)
        {
            GD.PrintErr($"FATAL: LogSettingsControllerPath is not set in the editor. Application cannot start.");
            GetTree().Quit();
            return;
        }
        var logSettingsController = GetNodeOrNull<LogSettingsController>(_logSettingsControllerPath);
        if (logSettingsController == null)
        {
            GD.PrintErr($"FATAL: LogSettingsController node not found at path '{_logSettingsControllerPath}'. Application cannot start.");
            GetTree().Quit();
            return;
        }

        // --- 3. Prepare Godot-specific Inputs & Create Log Switches ---
        var godotConsoleSink = new GodotConsoleSink(GodotSinkExtensions.DefaultGodotSinkOutputTemplate, null);
        
        var masterSwitch = new LoggingLevelSwitch((LogEventLevel)logSettingsController.DefaultLogLevel);
        var categorySwitches = new Dictionary<string, LoggingLevelSwitch>();
        
        if (logSettingsController.CategoryLogLevelOverrides != null)
        {
            foreach (var (category, level) in logSettingsController.CategoryLogLevelOverrides)
            {
                if (!string.IsNullOrEmpty(category))
                {
                    categorySwitches[category] = new LoggingLevelSwitch((LogEventLevel)level);
                }
            }
        }

        RichTextLabelSink? richTextSink = null;
        if (_richTextLogLabelPath != null && !_richTextLogLabelPath.IsEmpty)
        {
            var label = GetNodeOrNull<RichTextLabel>(_richTextLogLabelPath);
            if (label is not null)
            {
                // For simplicity, the rich text sink's rich text toggle is not currently dynamic.
                richTextSink = new RichTextLabelSink(label, true); 
            }
            else
            {
                GD.PrintErr($"SceneRoot: Node at path '{_richTextLogLabelPath}' is not a RichTextLabel. In-game console disabled.");
            }
        }

        // --- 4. Initialize DI Container ---
        try
        {
            _serviceProvider = GameStrapper.Initialize(
                masterSwitch,
                categorySwitches,
                godotConsoleSink,
                richTextSink);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"FATAL: Failed to initialize DI container. Error: {ex}");
            GetTree().Quit();
            return;
        }
        
        // --- 5. Retrieve Services & Initialize Controller ---
        PresenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
        var logger = _serviceProvider.GetRequiredService<ILogger>();
        
        logSettingsController.Initialize(logger, masterSwitch, categorySwitches);
        
        logger.ForContext("SourceContext", LogCategory.Core).Information("SceneRoot initialized and DI container is ready.");
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            // Clean up DI container if it's disposable
            (_serviceProvider as IDisposable)?.Dispose();
            Instance = null;
        }
    }
}
