using MediatR;
using Serilog;
using LanguageExt;
using LanguageExt.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

namespace BlockLife.Core.Application.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that logs information about each request.
/// It logs the request name, execution time, and the success or failure result.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger logger)
    {
        _logger = logger.ForContext("SourceContext", "MediatR");
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.Information("Handling {RequestName}...", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        // Check if response is a Fin<T> type and log accordingly
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Fin<>))
        {
            // Use reflection to check if it's success or failure
            var isFailProperty = responseType.GetProperty("IsFail");
            if (isFailProperty != null && isFailProperty.GetValue(response) is bool isFail)
            {
                if (!isFail)
                {
                    _logger.Information("{RequestName} handled successfully in {ElapsedMilliseconds}ms.", 
                        requestName, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.Warning("Failed to handle {RequestName} in {ElapsedMilliseconds}ms.", 
                        requestName, stopwatch.ElapsedMilliseconds);
                }
            }
        }
        else
        {
            _logger.Information("{RequestName} handled in {ElapsedMilliseconds}ms.", 
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
