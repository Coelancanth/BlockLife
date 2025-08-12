using Godot;
using Serilog.Events;
using System;

namespace BlockLife.Godot.Resources;

/// <summary>
/// A Godot Resource to hold configuration for the logging system.
/// This allows developers to adjust log settings directly from the Godot Editor.
/// </summary>
[GlobalClass]
public partial class LogSettings : Resource
{
    [Export(PropertyHint.Enum, "Verbose,Debug,Information,Warning,Error,Fatal")]
    public int DefaultLogLevel { get; set; } = (int)LogEventLevel.Information;

    [Export]
    public bool EnableRichTextInGodot { get; set; } = true;

    [Export]
    public string[] LogCategories { get; set; } = Array.Empty<string>();
    
    [Export(PropertyHint.ArrayType, "int,enum,Verbose,Debug,Information,Warning,Error,Fatal")]
    public int[] LogLevels { get; set; } = Array.Empty<int>();
}
