using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Infrastructure.Extensions;

/// <summary>
/// Extension methods to bridge the gap between Task-based and Fin-based async operations.
/// Part of ADR-006: Fin&lt;T&gt; vs Task&lt;T&gt; Consistency - Phase 1 Implementation.
/// 
/// These extensions enable functional composition throughout command handlers
/// without requiring breaking changes to infrastructure services.
/// </summary>
public static class TaskFinExtensions
{
    /// <summary>
    /// Converts a Task&lt;T&gt; to Task&lt;Fin&lt;T&gt;&gt; with proper error handling.
    /// Enables functional composition in command handlers.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="task">The task to convert</param>
    /// <returns>A task that resolves to either Success or Failure</returns>
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return FinSucc(result);
        }
        catch (Exception ex)
        {
            return FinFail<T>(Error.New("TASK_EXECUTION_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Converts a Task to Task&lt;Fin&lt;Unit&gt;&gt; with proper error handling.
    /// Enables functional composition for void-returning async operations.
    /// </summary>
    /// <param name="task">The task to convert</param>
    /// <returns>A task that resolves to either Success(Unit) or Failure</returns>
    public static async Task<Fin<Unit>> ToFin(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return FinSucc(Unit.Default);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New("TASK_EXECUTION_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Converts a Task&lt;T&gt; to Task&lt;Fin&lt;T&gt;&gt; with custom error code.
    /// Provides more specific error context for different operation types.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="task">The task to convert</param>
    /// <param name="errorCode">Custom error code for failures</param>
    /// <returns>A task that resolves to either Success or Failure with custom error code</returns>
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task, string errorCode)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return FinSucc(result);
        }
        catch (Exception ex)
        {
            return FinFail<T>(Error.New(errorCode, ex.Message));
        }
    }

    /// <summary>
    /// Converts a Task to Task&lt;Fin&lt;Unit&gt;&gt; with custom error code.
    /// Provides more specific error context for different operation types.
    /// </summary>
    /// <param name="task">The task to convert</param>
    /// <param name="errorCode">Custom error code for failures</param>
    /// <returns>A task that resolves to either Success(Unit) or Failure with custom error code</returns>
    public static async Task<Fin<Unit>> ToFin(this Task task, string errorCode)
    {
        try
        {
            await task.ConfigureAwait(false);
            return FinSucc(Unit.Default);
        }
        catch (Exception ex)
        {
            return FinFail<Unit>(Error.New(errorCode, ex.Message));
        }
    }

    /// <summary>
    /// Converts a Task&lt;T&gt; to Task&lt;Fin&lt;T&gt;&gt; with custom error code and message.
    /// Provides complete control over error representation.
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="task">The task to convert</param>
    /// <param name="errorCode">Custom error code for failures</param>
    /// <param name="errorMessage">Custom error message for failures</param>
    /// <returns>A task that resolves to either Success or Failure with custom error details</returns>
    public static async Task<Fin<T>> ToFin<T>(this Task<T> task, string errorCode, string errorMessage)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return FinSucc(result);
        }
        catch (Exception)
        {
            return FinFail<T>(Error.New(errorCode, errorMessage));
        }
    }

    /// <summary>
    /// Converts a Task to Task&lt;Fin&lt;Unit&gt;&gt; with custom error code and message.
    /// Provides complete control over error representation.
    /// </summary>
    /// <param name="task">The task to convert</param>
    /// <param name="errorCode">Custom error code for failures</param>
    /// <param name="errorMessage">Custom error message for failures</param>
    /// <returns>A task that resolves to either Success(Unit) or Failure with custom error details</returns>
    public static async Task<Fin<Unit>> ToFin(this Task task, string errorCode, string errorMessage)
    {
        try
        {
            await task.ConfigureAwait(false);
            return FinSucc(Unit.Default);
        }
        catch (Exception)
        {
            return FinFail<Unit>(Error.New(errorCode, errorMessage));
        }
    }
}