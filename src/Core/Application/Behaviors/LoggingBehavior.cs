using MediatR;
using Serilog;
using Serilog.Events;
using LanguageExt;
using LanguageExt.Common;
using BlockLife.Core.Infrastructure.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

namespace BlockLife.Core.Application.Behaviors;

/// <summary>
/// **ENHANCED** MediatR pipeline behavior that provides comprehensive logging
/// for all commands and queries with full functional error handling support.
/// 
/// This behavior integrates seamlessly with our functional architecture (ADR-006)
/// and provides detailed logging for debugging and monitoring.
/// 
/// Features:
/// - Automatic timing of all operations
/// - Detailed error logging with codes and messages
/// - Exception handling and logging  
/// - Performance monitoring integration
/// - Categorized logging for Commands/Queries
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger logger)
    {
        var category = typeof(TRequest).Name.Contains("Command") 
            ? LogCategory.Commands 
            : LogCategory.Queries;
            
        _logger = logger.ForContext("SourceContext", category);
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Log start with request details for debugging
        _logger.Debug("Starting {RequestName} with data: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        TResponse response;
        
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "EXCEPTION handling {RequestName} after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw; // Re-throw to preserve exception behavior
        }
        
        stopwatch.Stop();

        // Enhanced Fin<T> support with detailed error logging
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Fin<>))
        {
            LogFinResult(requestName, response, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            // Non-functional response types
            _logger.Information("{RequestName} completed in {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

    private void LogFinResult(string requestName, TResponse response, long elapsedMs)
    {
        try
        {
            var responseType = typeof(TResponse);
            
            // Use pattern matching for cleaner Fin<T> handling
            if (response is Fin<object> finResult)
            {
                finResult.Match(
                    Succ: _ => {
                        _logger.Information("{RequestName} SUCCESS in {ElapsedMilliseconds}ms", 
                            requestName, elapsedMs);
                    },
                    Fail: error => {
                        // Detailed error logging based on error characteristics  
                        var hasException = error.Exception.IsSome;
                        var logLevel = hasException ? LogEventLevel.Error : LogEventLevel.Warning;
                        
                        if (hasException)
                        {
                            error.Exception.IfSome(ex =>
                                _logger.Write(logLevel, ex, 
                                    "{RequestName} FAILED in {ElapsedMilliseconds}ms - Code: {ErrorCode}, Message: {ErrorMessage}", 
                                    requestName, elapsedMs, error.Code, error.Message));
                        }
                        else
                        {
                            _logger.Write(logLevel, 
                                "{RequestName} FAILED in {ElapsedMilliseconds}ms - Code: {ErrorCode}, Message: {ErrorMessage}", 
                                requestName, elapsedMs, error.Code, error.Message);
                        }
                    }
                );
            }
            else
            {
                // Fallback reflection-based approach for type safety
                var isFailProperty = responseType.GetProperty("IsFail");
                if (isFailProperty?.GetValue(response) is bool isFail)
                {
                    if (!isFail)
                    {
                        _logger.Information("{RequestName} SUCCESS in {ElapsedMilliseconds}ms", 
                            requestName, elapsedMs);
                    }
                    else
                    {
                        _logger.Warning("{RequestName} FAILED in {ElapsedMilliseconds}ms", 
                            requestName, elapsedMs);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // CRITICAL: Logging should never crash the application
            _logger.Warning(ex, "Failed to log result for {RequestName}", requestName);
        }
    }
}
