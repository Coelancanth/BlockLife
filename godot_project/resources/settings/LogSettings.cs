using Godot;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace BlockLife.Godot.Resources;

/// <summary>
/// A Godot Resource to hold configuration for the logging system.
/// This allows developers to adjust log settings directly from the Godot Editor.
/// Essential for debugging - enables live log level adjustments without recompilation.
/// </summary>
[GlobalClass]
public partial class LogSettings : Resource
{
    [Export]
    public LogEventLevel DefaultLogLevel { get; set; } = LogEventLevel.Information;

    [Export]
    public bool EnableRichTextInGodot { get; set; } = true;

    /// <summary>
    /// Enable debug logging for Commands (useful for debugging business logic)
    /// </summary>
    [Export]
    public bool VerboseCommands { get; set; } = false;
    
    /// <summary>
    /// Enable debug logging for Queries (useful for debugging read operations)
    /// </summary>
    [Export]
    public bool VerboseQueries { get; set; } = false;

    /// <summary>
    /// Maximum lines to keep in the in-game RichTextLabel console to prevent memory issues.
    /// </summary>
    [Export]
    public int MaxConsoleLines { get; set; } = 1000;

    /// <summary>
    /// Enable file logging for production diagnostics (always enabled for warnings/errors).
    /// </summary>
    [Export]
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Gets category-specific log levels as a dictionary for easier consumption in code.
    /// </summary>
    public Dictionary<string, LogEventLevel> GetCategoryLogLevels()
    {
        var result = new Dictionary<string, LogEventLevel>();
        
        // Simple verbose flags instead of complex arrays
        if (VerboseCommands)
            result["Commands"] = LogEventLevel.Debug;
        if (VerboseQueries)
            result["Queries"] = LogEventLevel.Debug;
            
        return result;
    }
}
