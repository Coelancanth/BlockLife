using Godot;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Text;

namespace BlockLife.Godot.Infrastructure.Logging;

/// <summary>
/// A custom Serilog sink that writes log events to a Godot RichTextLabel.
/// It supports rich text formatting (BBCode) and is optimized for performance
/// by managing its own line buffer and deferring UI updates to the main thread.
/// </summary>
public class RichTextLabelSink : ILogEventSink
{
    private readonly RichTextLabel _richTextLabel;
    private readonly bool _enableRichText;
    private readonly int _maxLines;
    private readonly Queue<string> _logQueue = new();

    public RichTextLabelSink(RichTextLabel richTextLabel, bool enableRichText, int maxLines = 1000)
    {
        _richTextLabel = richTextLabel;
        _enableRichText = enableRichText;
        _maxLines = maxLines;
        _richTextLabel.BbcodeEnabled = true; // Ensure BBCode is enabled
    }

    public void Emit(LogEvent logEvent)
    {
        var message = _enableRichText 
            ? FormatRichText(logEvent) 
            : logEvent.RenderMessage();

        // UI operations must be deferred to the main thread to be safe.
        _richTextLabel.CallDeferred(nameof(AppendLog), message);
    }

    // This method will be called by Godot on the main thread via CallDeferred.
    private void AppendLog(string message)
    {
        if (!GodotObject.IsInstanceValid(_richTextLabel)) return;

        _logQueue.Enqueue(message);
        while (_logQueue.Count > _maxLines)
        {
            _logQueue.Dequeue();
        }

        var sb = new StringBuilder();
        foreach (var line in _logQueue)
        {
            sb.AppendLine(line);
        }
        _richTextLabel.Text = sb.ToString();
        _richTextLabel.ScrollToLine(_richTextLabel.GetLineCount() - 1);
    }

    private string FormatRichText(LogEvent logEvent)
    {
        var color = logEvent.Level switch
        {
            LogEventLevel.Verbose => "gray",
            LogEventLevel.Debug => "lightblue",
            LogEventLevel.Information => "white",
            LogEventLevel.Warning => "yellow",
            LogEventLevel.Error => "orangered",
            LogEventLevel.Fatal => "red",
            _ => "white"
        };
        
        logEvent.Properties.TryGetValue("SourceContext", out var sourceContext);
        var context = sourceContext?.ToString().Trim('"') ?? "Default";
        
        return $"[color={color}][{logEvent.Level.ToString().ToUpper()}] [{context}] {logEvent.RenderMessage()}[/color]";
    }
}
