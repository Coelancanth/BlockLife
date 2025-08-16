using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Serilog;

namespace BlockLife.Godot.Features.Block.Performance;

/// <summary>
/// Performance profiler for tracking view layer operations.
/// Helps identify bottlenecks in the visual pipeline.
/// </summary>
public static class PerformanceProfiler
{
    private static readonly Dictionary<string, List<long>> _timings = new();
    private static readonly Dictionary<string, Stopwatch> _activeTimers = new();
    private static ILogger? _logger;
    
    public static void Initialize(ILogger logger)
    {
        _logger = logger?.ForContext("SourceContext", "Performance");
    }
    
    /// <summary>
    /// Start timing an operation.
    /// </summary>
    public static void StartTimer(string operationName)
    {
        if (_activeTimers.ContainsKey(operationName))
        {
            _logger?.Warning("Timer {Operation} already running, restarting", operationName);
        }
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _activeTimers[operationName] = stopwatch;
    }
    
    /// <summary>
    /// Stop timing an operation and record the result.
    /// </summary>
    public static long StopTimer(string operationName, bool logImmediately = false)
    {
        if (!_activeTimers.TryGetValue(operationName, out var stopwatch))
        {
            _logger?.Warning("Timer {Operation} not found", operationName);
            return -1;
        }
        
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        
        // Store timing
        if (!_timings.ContainsKey(operationName))
        {
            _timings[operationName] = new List<long>();
        }
        _timings[operationName].Add(elapsedMs);
        
        // Clean up
        _activeTimers.Remove(operationName);
        
        // Log if requested or if timing exceeds threshold
        if (logImmediately || elapsedMs > 16) // 16ms = 60fps frame budget
        {
            var warningLevel = elapsedMs > 33 ? "🔴" : elapsedMs > 16 ? "🟡" : "🟢";
            _logger?.Information("{Level} {Operation} took {ElapsedMs}ms", 
                warningLevel, operationName, elapsedMs);
        }
        
        return elapsedMs;
    }
    
    /// <summary>
    /// Measure a synchronous operation.
    /// </summary>
    public static T Measure<T>(string operationName, Func<T> operation)
    {
        StartTimer(operationName);
        try
        {
            return operation();
        }
        finally
        {
            StopTimer(operationName, true);
        }
    }
    
    /// <summary>
    /// Measure a void operation.
    /// </summary>
    public static void Measure(string operationName, Action operation)
    {
        StartTimer(operationName);
        try
        {
            operation();
        }
        finally
        {
            StopTimer(operationName, true);
        }
    }
    
    /// <summary>
    /// Print performance report.
    /// </summary>
    public static void PrintReport()
    {
        _logger?.Information("===== PERFORMANCE REPORT =====");
        
        foreach (var kvp in _timings.OrderByDescending(x => x.Value.Average()))
        {
            var operation = kvp.Key;
            var times = kvp.Value;
            
            if (times.Count == 0) continue;
            
            var avg = times.Average();
            var min = times.Min();
            var max = times.Max();
            var count = times.Count;
            
            var status = avg > 33 ? "❌ CRITICAL" : avg > 16 ? "⚠️ WARNING" : "✅ OK";
            
            _logger?.Information("{Status} {Operation}: Avg={Avg:F1}ms, Min={Min}ms, Max={Max}ms, Count={Count}",
                status, operation, avg, min, max, count);
        }
        
        _logger?.Information("==============================");
    }
    
    /// <summary>
    /// Clear all recorded timings.
    /// </summary>
    public static void Reset()
    {
        _timings.Clear();
        _activeTimers.Clear();
        _logger?.Information("Performance profiler reset");
    }
    
    /// <summary>
    /// Get average timing for an operation.
    /// </summary>
    public static double GetAverageMs(string operationName)
    {
        if (!_timings.TryGetValue(operationName, out var times) || times.Count == 0)
            return 0;
        
        return times.Average();
    }
}