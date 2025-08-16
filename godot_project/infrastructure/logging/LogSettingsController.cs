#if TOOLS
using BlockLife.Core.Infrastructure.Logging;
using Godot;
using Godot.Collections;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlockLife.Godot.Infrastructure.Logging;

[Tool]
[GlobalClass]
public partial class LogSettingsController : Node
{
    // This dictionary is no longer exported but is still public so SceneRoot can read its initial values.
    public global::Godot.Collections.Dictionary<string, int> CategoryLogLevelOverrides { get; set; } = new();
    
    public int DefaultLogLevel { get; set; } = (int)LogEventLevel.Information;
    
    private ILogger? _logger;
    private LoggingLevelSwitch? _masterSwitch;
    private IDictionary<string, LoggingLevelSwitch>? _categorySwitches;

    // Fields to store the last known state for polling
    private int _lastKnownDefaultLevel;
    private readonly System.Collections.Generic.Dictionary<string, int> _lastKnownCategoryLevels = new();

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        // Property for the default log level
        properties.Add(new Dictionary
        {
            { "name", nameof(DefaultLogLevel) },
            { "type", (int)Variant.Type.Int },
            { "usage", (int)PropertyUsageFlags.Default },
            { "hint", (int)PropertyHint.Enum },
            { "hint_string", "Verbose,Debug,Information,Warning,Error,Fatal" }
        });

        // Group for the category overrides
        properties.Add(new Dictionary
        {
            { "name", "Category Overrides" },
            { "type", (int)Variant.Type.Nil },
            { "usage", (int)PropertyUsageFlags.Category }
        });

        var logCategories = typeof(LogCategory).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetRawConstantValue()?.ToString())
            .ToList();

        foreach (var category in logCategories)
        {
            if (string.IsNullOrEmpty(category)) continue;
            
            properties.Add(new Dictionary
            {
                { "name", $"category_{category}" },
                { "type", (int)Variant.Type.Int },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", "Verbose,Debug,Information,Warning,Error,Fatal" }
            });
        }

        return properties;
    }

    public override Variant _Get(StringName property)
    {
        if (property == nameof(DefaultLogLevel))
        {
            return DefaultLogLevel;
        }

        if (property.ToString().StartsWith("category_"))
        {
            var category = property.ToString().Remove(0, "category_".Length);
            if (CategoryLogLevelOverrides.TryGetValue(category, out var level))
            {
                return level;
            }
            // Default to the main level if no override is set
            return DefaultLogLevel;
        }

        return base._Get(property);
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (property == nameof(DefaultLogLevel))
        {
            DefaultLogLevel = value.As<int>();
            NotifyPropertyListChanged();
            return true;
        }

        if (property.ToString().StartsWith("category_"))
        {
            var category = property.ToString().Remove(0, "category_".Length);
            CategoryLogLevelOverrides[category] = value.As<int>();
            NotifyPropertyListChanged();
            return true;
        }

        return base._Set(property, value);
    }

    public void Initialize(ILogger logger, LoggingLevelSwitch masterSwitch, IDictionary<string, LoggingLevelSwitch> categorySwitches)
    {
        _logger = logger;
        _masterSwitch = masterSwitch;
        _categorySwitches = categorySwitches;

        _lastKnownDefaultLevel = -1; 
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || _masterSwitch is null || _categorySwitches is null) return;

        if (_lastKnownDefaultLevel != DefaultLogLevel)
        {
            _masterSwitch.MinimumLevel = (LogEventLevel)DefaultLogLevel;
            _lastKnownDefaultLevel = DefaultLogLevel;
        }

        foreach (var (category, level) in CategoryLogLevelOverrides)
        {
            if (!_lastKnownCategoryLevels.TryGetValue(category, out var lastLevel) || lastLevel != level)
            {
                if (_categorySwitches.TryGetValue(category, out var levelSwitch))
                {
                    levelSwitch.MinimumLevel = (LogEventLevel)level;
                }
                _lastKnownCategoryLevels[category] = level;
            }
        }
    }
}
#endif