using BlockLife.Core.Features.Block.Effects;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Executors;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Notifications
{
    /// <summary>
    /// Handles BlockMovedNotification by triggering pattern recognition and execution.
    /// This is the bridge between block movements and the match-3 game mechanics.
    /// </summary>
    public class ProcessPatternsAfterMoveHandler : INotificationHandler<BlockMovedNotification>
    {
        private readonly IGridStateService _gridService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessPatternsAfterMoveHandler>? _logger;
        private readonly IPatternProcessingTracker _processingTracker;

        public ProcessPatternsAfterMoveHandler(
            IGridStateService gridService,
            IServiceProvider serviceProvider,
            IPatternProcessingTracker processingTracker,
            ILogger<ProcessPatternsAfterMoveHandler>? logger = null)
        {
            _gridService = gridService;
            _serviceProvider = serviceProvider;
            _processingTracker = processingTracker;
            _logger = logger;
        }

        public async Task Handle(BlockMovedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger?.LogDebug("Processing patterns after block move from {From} to {To}",
                    notification.FromPosition, notification.ToPosition);

                // Process patterns at the destination position (where block landed)
                await ProcessPatternsAtPosition(notification.ToPosition, cancellationToken);

                // Also check the source position if blocks fell into it
                // This handles cascades when blocks fall after matches
                await ProcessPatternsAtPosition(notification.FromPosition, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing patterns after block move. From: {From}, To: {To}, Error: {ErrorMessage}", 
                    notification.FromPosition, notification.ToPosition, ex.Message);
                // Don't throw - pattern processing failure shouldn't break the move
            }
        }

        private async Task ProcessPatternsAtPosition(Domain.Common.Vector2Int position, CancellationToken cancellationToken)
        {
            // Track that we're starting pattern processing
            _processingTracker.BeginProcessing();
            
            try
            {
                _logger?.LogDebug("Getting pattern services from DI for position {Position}", position);
                
                // Get match pattern recognizer from DI
                var matchRecognizer = _serviceProvider.GetService<MatchPatternRecognizer>();
                if (matchRecognizer == null)
                {
                    _logger?.LogWarning("MatchPatternRecognizer not found in DI, creating new instance");
                    matchRecognizer = new MatchPatternRecognizer();
                }
                
                // Get pattern execution resolver from DI
                var resolver = _serviceProvider.GetService<PatternExecutionResolver>();
                if (resolver == null)
                {
                    _logger?.LogWarning("PatternExecutionResolver not found in DI");
                    return;
                }

                // Create pattern context with no specific target types (find all patterns)
                var context = new PatternContext
                {
                    TriggerPosition = position,
                    TargetPatternTypes = Seq<PatternType>(),
                    MaxPatternsPerType = 10,
                    RecognitionStartedAt = DateTime.UtcNow
                };

                // Find match patterns at this position
                _logger?.LogDebug("Recognizing match patterns at position {Position}", position);
                var matchPatternsResult = matchRecognizer.Recognize(_gridService, position, context);
                
                // Process match patterns
                var patternsResult = matchPatternsResult;
                
                if (patternsResult.IsFail)
                {
                    _logger?.LogWarning("Pattern recognition failed at {Position}: {Error}", 
                        position, patternsResult.Match(Succ: _ => "", Fail: e => e.Message));
                    return;
                }

                var patterns = patternsResult.Match(
                    Succ: p => p,
                    Fail: _ => Empty
                );

                if (!patterns.Any())
                {
                    _logger?.LogDebug("No patterns found at {Position}", position);
                    return;
                }

                _logger?.LogInformation("Found {Count} patterns at {Position}", patterns.Count, position);

                // Execute each pattern
                foreach (var pattern in patterns)
                {
                    if (pattern is MatchPattern matchPattern)
                    {
                        _logger?.LogInformation("Executing match pattern with {Count} blocks", 
                            matchPattern.Positions.Count);

                        var executionContext = BlockLife.Core.Features.Block.Patterns.ExecutionContext.Create(_gridService, position);
                        // Use resolver to determine which executor to use
                        var executor = resolver.ResolveExecutor(pattern);
                        var executeResult = await executor.Execute(pattern, executionContext);

                        if (executeResult.IsFail)
                        {
                            _logger?.LogWarning("Failed to execute pattern {PatternId}: {Error}",
                                pattern.PatternId,
                                executeResult.Match(Succ: _ => "", Fail: e => e.Message));
                        }
                        else
                        {
                            _logger?.LogInformation("Successfully executed pattern {PatternId}", 
                                pattern.PatternId);
                            
                            // After successful execution, check for cascades
                            // Get positions adjacent to removed blocks for cascade checking
                            var cascadePositions = matchPattern.Positions
                                .SelectMany(p => p.GetOrthogonallyAdjacentPositions())
                                .Where(p => _gridService.IsValidPosition(p))
                                .Distinct()
                                .ToList();

                            foreach (var cascadePos in cascadePositions)
                            {
                                // Recursively check for new patterns formed by the removal
                                await ProcessPatternsAtPosition(cascadePos, cancellationToken);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ProcessPatternsAtPosition for position {Position}", position);
                throw;
            }
            finally
            {
                // Always mark processing as complete, even if there was an error
                _processingTracker.EndProcessing();
            }
        }
    }
}