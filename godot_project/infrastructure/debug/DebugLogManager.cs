using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlockLife.Core.Infrastructure.Logging;
using Godot;
using Serilog.Core;
using Serilog.Events;

namespace BlockLife.godot_project.infrastructure.debug;

[GlobalClass]
public partial class DebugLogManager : Control
{
    [Export]
    private CheckButton? _toggleButton;
    [Export]
    private VBoxContainer? _logSettingsContainer;

    private LoggingLevelSwitch? _masterSwitch;
    private IDictionary<string, LoggingLevelSwitch>? _categorySwitches;

    public override void _Ready()
    {
        if (_toggleButton is not null && _logSettingsContainer is not null)
        {
            _toggleButton.Toggled += (toggled) => _logSettingsContainer.Visible = toggled;
        }
    }

    public void Initialize(LoggingLevelSwitch masterSwitch, IDictionary<string, LoggingLevelSwitch> categorySwitches)
    {
        _masterSwitch = masterSwitch;
        _categorySwitches = categorySwitches;
        PopulateControls();
    }

    private void PopulateControls()
    {
        if (_logSettingsContainer is null) return;

        // Clear any existing controls
        foreach (var child in _logSettingsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (_masterSwitch is null || _categorySwitches is null) return;

        // Add master switch control
        AddLogLevelControl("Default", _masterSwitch);

        // Add category switch controls
        var logCategories = typeof(LogCategory).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => (string?)f.GetRawConstantValue())
            .ToList();

        foreach (var category in logCategories)
        {
            if (category is not null && _categorySwitches.TryGetValue(category, out var levelSwitch))
            {
                AddLogLevelControl(category, levelSwitch);
            }
        }
    }

    private void AddLogLevelControl(string name, LoggingLevelSwitch levelSwitch)
    {
        if (_logSettingsContainer is null) return;

        var hbox = new HBoxContainer();
        var label = new Label { Text = name };
        var optionButton = new OptionButton();

        foreach (LogEventLevel level in Enum.GetValues(typeof(LogEventLevel)))
        {
            optionButton.AddItem(level.ToString());
        }

        optionButton.Selected = (int)levelSwitch.MinimumLevel;
        optionButton.ItemSelected += (index) =>
        {
            levelSwitch.MinimumLevel = (LogEventLevel)index;
        };

        hbox.AddChild(label);
        hbox.AddChild(optionButton);
        _logSettingsContainer.AddChild(hbox);
    }
}
