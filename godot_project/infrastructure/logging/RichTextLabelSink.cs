using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Godot;
using Serilog.Core;
using Serilog.Events;

namespace BlockLife.godot_project.infrastructure.logging;

/// <summary>
/// **PERFORMANCE-CRITICAL** Serilog sink for Godot RichTextLabel.
/// 
/// This implementation addresses critical performance issues:
/// - Batched updates prevent UI freezing under high-frequency logging
/// - Weak references prevent memory leaks when nodes are destroyed  
/// - Rate limiting prevents runaway logging scenarios
/// - Thread-safe with proper disposal pattern
/// 
/// ARCHITECTURE NOTE: This is a presentation-layer component that's allowed
/// to have Godot dependencies for debugging infrastructure.
/// </summary>
public class RichTextLabelSink : ILogEventSink, IDisposable
{
    private readonly WeakReference<RichTextLabel> _richTextLabelRef;
    private readonly bool _enableRichText;
    private readonly int _maxLines;
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly System.Threading.Timer _flushTimer;
    private readonly StringBuilder _batchBuffer = new(4096);
    private volatile bool _disposed;

    // Rate limiting to prevent UI freezing
    private const int MaxBatchSize = 50;
    private const int FlushIntervalMs = 100;

    public RichTextLabelSink(RichTextLabel richTextLabel, bool enableRichText, int maxLines = 1000)
    {
        _richTextLabelRef = new WeakReference<RichTextLabel>(richTextLabel);
        _enableRichText = enableRichText;
        _maxLines = maxLines;

        if (GodotObject.IsInstanceValid(richTextLabel))
        {
            richTextLabel.BbcodeEnabled = true;
        }

        // Batched processing to prevent performance issues
        _flushTimer = new System.Threading.Timer(FlushBatch, null, FlushIntervalMs, FlushIntervalMs);
    }

    public void Emit(LogEvent logEvent)
    {
        if (_disposed) return;

        try
        {
            var message = _enableRichText
                ? FormatRichText(logEvent)
                : logEvent.RenderMessage();

            _logQueue.Enqueue(message);

            // Prevent unbounded growth
            while (_logQueue.Count > _maxLines * 2)
            {
                _logQueue.TryDequeue(out _);
            }
        }
        catch
        {
            // CRITICAL: Never crash application due to logging failure
        }
    }

    private void FlushBatch(object? state)
    {
        if (_disposed || !_richTextLabelRef.TryGetTarget(out var richTextLabel) || !GodotObject.IsInstanceValid(richTextLabel))
            return;

        try
        {
            _batchBuffer.Clear();
            int count = 0;

            while (_logQueue.TryDequeue(out var message) && count < MaxBatchSize)
            {
                if (_batchBuffer.Length > 0) _batchBuffer.AppendLine();
                _batchBuffer.Append(message);
                count++;
            }

            if (_batchBuffer.Length > 0)
            {
                richTextLabel.CallDeferred(nameof(AppendBatchSafe), _batchBuffer.ToString());
            }
        }
        catch
        {
            // CRITICAL: Silent failure - logging should never crash the application
        }
    }

    private void AppendBatchSafe(string batchText)
    {
        if (!_richTextLabelRef.TryGetTarget(out var richTextLabel) || !GodotObject.IsInstanceValid(richTextLabel))
            return;

        try
        {
            // Simple but effective line management approach
            var currentLines = richTextLabel.GetLineCount();
            if (currentLines >= _maxLines)
            {
                // Clear and rebuild when we hit the limit (performance trade-off for simplicity)
                var lines = richTextLabel.Text.Split('\n');
                var keepLines = lines.Skip(Math.Max(0, lines.Length - _maxLines + MaxBatchSize)).ToArray();
                richTextLabel.Text = string.Join('\n', keepLines);
            }

            if (richTextLabel.Text.Length > 0)
            {
                richTextLabel.AppendText("\n" + batchText);
            }
            else
            {
                richTextLabel.AppendText(batchText);
            }

            // Auto-scroll to bottom
            richTextLabel.ScrollToLine(richTextLabel.GetLineCount() - 1);
        }
        catch
        {
            // CRITICAL: Silent failure - never crash due to logging
        }
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _flushTimer?.Dispose();

        // Final flush
        FlushBatch(null);
    }
}
