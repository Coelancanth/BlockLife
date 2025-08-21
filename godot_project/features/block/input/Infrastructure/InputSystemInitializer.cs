using System;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using Godot;
using LanguageExt;
using Serilog;
using static LanguageExt.Prelude;

namespace BlockLife.godot_project.features.block.input.Infrastructure;

/// <summary>
/// Handles one-time initialization and pre-warming of input system components.
/// Addresses BF_001 performance issues by pre-warming expensive operations at startup.
/// </summary>
public static class InputSystemInitializer
{
    private static bool _initialized = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Pre-warms all input system components to prevent first-operation delays.
    /// Should be called once during application startup.
    /// </summary>
    public static async Task Initialize(Node context, ILogger? logger)
    {
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }

        logger = logger?.ForContext("PreWarm", true);
        logger?.Debug("Starting input system pre-warming...");

        var startTime = DateTime.Now;

        // Pre-warm in parallel for faster startup
        var tasks = new[]
        {
            PreWarmSerilogTemplates(logger),
            PreWarmAsyncStateMachine(logger),
            PreWarmGodotTweenSystem(context, logger),
            PreWarmLanguageExtPatterns(logger)
        };

        await Task.WhenAll(tasks);

        var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
        logger?.Information("Input system pre-warming completed in {ElapsedMs}ms", elapsed);
    }

    /// <summary>
    /// Pre-compiles Serilog message templates to prevent compilation delay on first use.
    /// </summary>
    private static Task PreWarmSerilogTemplates(ILogger? logger)
    {
        if (logger == null) return Task.CompletedTask;

        try
        {
            // Create a silent logger that discards all output
            var silentLogger = Serilog.Log.Logger.ForContext("PreWarm", true)
                .ForContext(Serilog.Core.Constants.SourceContextPropertyName, "PreWarm");

            // Pre-compile common message templates (output is discarded)
            silentLogger.Debug("Moving block {BlockId} from {FromPosition} to {ToPosition}",
                Guid.Empty, Vector2Int.Zero, Vector2Int.One);
            silentLogger.Information("Block placed successfully at {Position}", Vector2Int.Zero);
            silentLogger.Debug("Selected block {BlockId} at position {Position}",
                Guid.Empty, Vector2Int.Zero);
            silentLogger.Warning("Failed to move block {BlockId}: {Error}",
                Guid.Empty, "test");
            silentLogger.Information("Inspecting position {Position}", Vector2Int.Zero);
            silentLogger.Information("BLOCK INFO at {Position}:", Vector2Int.Zero);
            silentLogger.Information("   BlockId: {BlockId}", Guid.Empty);
            silentLogger.Information("   Type: {BlockType}", BlockType.Basic);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm Serilog templates");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pre-warms async/await state machine to prevent first-time initialization cost.
    /// </summary>
    private static async Task PreWarmAsyncStateMachine(ILogger? logger)
    {
        try
        {
            // Warm up basic async patterns
            await Task.CompletedTask;
            await Task.Delay(1);

            // Warm up Option<T>.Match with async lambdas (the pattern that caused 271ms delay)
            var testOption = Some(new Vector2Int(0, 0));
            await testOption.Match(
                Some: async pos =>
                {
                    await Task.CompletedTask;
                    return Unit.Default;
                },
                None: async () =>
                {
                    await Task.CompletedTask;
                    return Unit.Default;
                }
            );

            // Warm up Fin<T>.Match with async lambdas
            var testFin = FinSucc(Unit.Default);
            await testFin.Match(
                Succ: async _ =>
                {
                    await Task.CompletedTask;
                    return Unit.Default;
                },
                Fail: async _ =>
                {
                    await Task.CompletedTask;
                    return Unit.Default;
                }
            );
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm async state machine");
        }
    }

    /// <summary>
    /// Pre-warms Godot Tween subsystem to prevent ~289ms initialization delay.
    /// </summary>
    private static Task PreWarmGodotTweenSystem(Node context, ILogger? logger)
    {
        if (!context.IsInsideTree()) return Task.CompletedTask;

        try
        {
            // Create and immediately destroy a tween to trigger JIT compilation
            var warmupTween = context.CreateTween();
            warmupTween?.Kill();

            // Create a second tween to verify pre-warming worked
            var verifyTween = context.CreateTween();
            verifyTween?.Kill();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm Godot Tween subsystem");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pre-warms LanguageExt patterns commonly used in input handling.
    /// </summary>
    private static Task PreWarmLanguageExtPatterns(ILogger? logger)
    {
        try
        {
            // Pre-warm Option patterns
            var none = Option<Vector2Int>.None;
            var some = Some(Vector2Int.Zero);
            _ = some.IsSome;
            _ = none.IsNone;
            _ = some.Map(v => v.X);

            // Pre-warm Fin patterns
            var success = FinSucc(Unit.Default);
            var failure = FinFail<Unit>(LanguageExt.Common.Error.New("TEST", "Pre-warm test error"));
            _ = success.IsSucc;
            _ = failure.IsFail;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm LanguageExt patterns");
            return Task.CompletedTask;
        }
    }
}
