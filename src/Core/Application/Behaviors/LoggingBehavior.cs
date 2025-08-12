using MediatR;
using Serilog;
using LanguageExt;
using LanguageExt.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using BlockLife.Core.Application.Commands;

namespace BlockLife.Core.Application.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that logs information about each request.
/// It logs the request name, execution time, and the success or failure result.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ISuccessFail
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

        response.Match(
            Succ: _ => 
            {
                _logger.Information("{RequestName} handled successfully in {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);
            },
            Fail: error =>
            {
                error.Exception.Match(
                    Some: ex => _logger.Error(ex, "Failed to handle {RequestName} in {ElapsedMilliseconds}ms. Code: {ErrorCode}, Message: {ErrorMessage}",
                        requestName, stopwatch.ElapsedMilliseconds, error.Code, error.Message),
                    None: () => _logger.Warning("Failed to handle {RequestName} in {ElapsedMilliseconds}ms. Code: {ErrorCode}, Message: {ErrorMessage}",
                        requestName, stopwatch.ElapsedMilliseconds, error.Code, error.Message)
                );
            }
        );

        return response;
    }
}
