using Godot;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace BlockLife.Godot.Resources;

/// <summary>
/// A Godot Resource to hold configuration for the logging system.
/// This allows developers to adjust log settings directly from the Godot Editor.
/// Essential for debugging - enables live log level adjustments without recompilation.
/// 
/// NOTE: Trace level (LogEventLevel.Verbose) is not displayed in Godot editor dropdown
/// due to Godot's enum export limitations. To enable Trace level, set it programmatically
/// or use Debug level which includes most diagnostic information.
/// </summary>
[GlobalClass]
[Tool]
public partial class LogSettings : Resource
{
    #region Log Level Documentation
    
    /// <summary>
    /// Log Level Guidelines:
    /// 
    /// VERBOSE/TRACE (0): Everything - method entry/exit, variable values, loop iterations
    ///   - Performance Impact: HIGH
    ///   - Use: Deep troubleshooting only
    ///   
    /// DEBUG (1): Diagnostic details - state changes, decisions, command completions  
    ///   - Performance Impact: Moderate
    ///   - Use: Development and debugging
    ///   
    /// INFORMATION (2): Key events - app startup, major operations completed
    ///   - Performance Impact: Minimal
    ///   - Use: Production default
    ///   
    /// WARNING (3): Recoverable issues - retries, fallbacks, degraded operation
    ///   - Performance Impact: None
    ///   - Use: Always enabled
    ///   
    /// ERROR (4): Failures requiring attention - exceptions, failed operations
    ///   - Performance Impact: None
    ///   - Use: Always enabled
    ///   
    /// FATAL (5): Application crash imminent
    ///   - Performance Impact: None
    ///   - Use: Always enabled
    /// </summary>
    private const string LogLevelDocumentation = "See tooltips for each level";
    
    #endregion
    [Export(PropertyHint.Enum, "Verbose:0,Debug:1,Information:2,Warning:3,Error:4,Fatal:5")]
    [ExportGroup("Log Levels")]
    [ExportSubgroup("Default Level")]
    public LogEventLevel DefaultLogLevel { get; set; } = LogEventLevel.Information;

    [Export]
    [ExportGroup("Display Settings")]
    public bool EnableRichTextInGodot { get; set; } = true;

    /// <summary>
    /// Enable debug logging for Commands (useful for debugging business logic)
    /// </summary>
    [Export]
    [ExportGroup("Category Overrides")]
    public bool VerboseCommands { get; set; } = false;
    
    /// <summary>
    /// Enable debug logging for Queries (useful for debugging read operations)
    /// </summary>
    [Export]
    public bool VerboseQueries { get; set; } = false;

    /// <summary>
    /// Maximum lines to keep in the in-game RichTextLabel console to prevent memory issues.
    /// </summary>
    [Export(PropertyHint.Range, "100,10000,100")]
    [ExportGroup("Performance")]
    public int MaxConsoleLines { get; set; } = 1000;

    /// <summary>
    /// Enable file logging for production diagnostics (always enabled for warnings/errors).
    /// </summary>
    [Export]
    [ExportGroup("Output")]
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
