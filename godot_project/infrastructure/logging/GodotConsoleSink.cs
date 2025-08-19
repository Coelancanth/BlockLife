using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Godot;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;

namespace BlockLife.godot_project.infrastructure.logging;

public class GodotConsoleSink : ILogEventSink
{
    private readonly ITextFormatter _formatter;

    public GodotConsoleSink(string outputTemplate, IFormatProvider? formatProvider)
    {
        _formatter = new TemplateRenderer(outputTemplate, formatProvider);
    }

    public void Emit(LogEvent logEvent)
    {
        using TextWriter writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        writer.Flush();

        string color = logEvent.Level switch
        {
            LogEventLevel.Debug => Colors.SpringGreen.ToHtml(false),
            LogEventLevel.Information => Colors.Cyan.ToHtml(false),
            LogEventLevel.Warning => Colors.Yellow.ToHtml(false),
            LogEventLevel.Error => Colors.Red.ToHtml(false),
            LogEventLevel.Fatal => Colors.Purple.ToHtml(false),
            _ => Colors.LightGray.ToHtml(false),
        };

        // Use a null-coalescing operator for safety, although StringWriter should not produce null.
        foreach (string line in writer.ToString()?.Split('\n') ?? Array.Empty<string>())
        {
            if (!string.IsNullOrEmpty(line)) // Avoid printing empty lines
            {
                // Enhance line with special formatting for coordinates and values
                var enhancedLine = EnhanceLineFormatting(line);
                GD.PrintRich($"[color=#{color}]{enhancedLine}[/color]");
            }
        }

        if (logEvent.Exception is null) return;

        if (logEvent.Level >= LogEventLevel.Error)
            GD.PushError(logEvent.Exception);
        else
            GD.PushWarning(logEvent.Exception);
    }

    private static string EnhanceLineFormatting(string line)
    {
        // Highlight coordinate patterns like (X, Y) or X, Y with bright colors
        line = System.Text.RegularExpressions.Regex.Replace(line, 
            @"\((\d+),\s*(\d+)\)", 
            "[/color][color=orange]($1, $2)[/color][color=inherit]");
        
        // Highlight standalone numbers that might be coordinates or important values
        line = System.Text.RegularExpressions.Regex.Replace(line,
            @"\b(\d+)x(\d+)\b",
            "[/color][color=lime]$1x$2[/color][color=inherit]");
            
        // Highlight timing values (e.g., "7ms", "150ms")
        line = System.Text.RegularExpressions.Regex.Replace(line,
            @"\b(\d+)ms\b",
            "[/color][color=yellow]$1ms[/color][color=inherit]");
            
        // Highlight SUCCESS/FAILED patterns
        line = System.Text.RegularExpressions.Regex.Replace(line,
            @"\b(SUCCESS|COMPLETED)\b",
            "[/color][color=lightgreen]$1[/color][color=inherit]");
            
        line = System.Text.RegularExpressions.Regex.Replace(line,
            @"\b(FAILED|ERROR|CRITICAL)\b",
            "[/color][color=red]$1[/color][color=inherit]");
            
        return line;
    }

    private class TemplateRenderer : ITextFormatter
    {
        private delegate void Renderer(LogEvent logEvent, TextWriter output);

        private readonly Renderer[] _renderers;
        private readonly IFormatProvider? _formatProvider;

        public TemplateRenderer(string outputTemplate, IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;

            MessageTemplate template = new MessageTemplateParser().Parse(outputTemplate);
            _renderers = template.Tokens.Select(
            token => token switch
            {
                TextToken textToken => (Renderer)((_, output) => output.Write(textToken.Text)),
                PropertyToken propertyToken => propertyToken.PropertyName switch
                {
                    OutputProperties.LevelPropertyName
                        => (logEvent, output) => output.Write(logEvent.Level),
                    OutputProperties.MessagePropertyName
                        => (logEvent, output) => logEvent.RenderMessage(output, formatProvider),
                    OutputProperties.NewLinePropertyName
                        => (_, output) => output.Write('\n'),
                    OutputProperties.TimestampPropertyName
                        => RenderTimestamp(propertyToken.Format),
                    _ 
                        => RenderProperty(propertyToken.PropertyName, propertyToken.Format),
                },
                _ => null,
            }
            ).OfType<Renderer>().ToArray();
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            foreach (var renderer in _renderers)
                renderer.Invoke(logEvent, output);
        }

        private Renderer RenderTimestamp(string? format)
        {
            Func<LogEvent, string> f = _formatProvider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter formatter
                ? (logEvent) => formatter.Format(format, logEvent.Timestamp, _formatProvider)
                : (logEvent) => logEvent.Timestamp.ToString(format, _formatProvider ?? CultureInfo.InvariantCulture);

            return (logEvent, output) => output.Write(f(logEvent));
        }

        private Renderer RenderProperty(string propertyName, string? format)
        {
            return delegate (LogEvent logEvent, TextWriter output)
            {
                if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue))
                    propertyValue.Render(output, format, _formatProvider);
            };
        }
}
}

public static class GodotSinkExtensions
{
    public const string DefaultGodotSinkOutputTemplate = "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration Godot(this LoggerSinkConfiguration configuration,
                                            string outputTemplate = DefaultGodotSinkOutputTemplate,
                                            IFormatProvider? formatProvider = null)
    {
        return configuration.Sink(new GodotConsoleSink(outputTemplate, formatProvider));
    }
}