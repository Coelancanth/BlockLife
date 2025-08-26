using BlockLife.Core.Features.Block.Placement.Notifications;
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
    /// Handles BlockPlacedNotification by triggering pattern recognition and execution.
    /// This ensures match-3 patterns are detected when new blocks are placed on the grid.
    /// </summary>
    public class ProcessPatternsAfterPlacementHandler : INotificationHandler<BlockPlacedNotification>
    {
        private readonly IGridStateService _gridService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessPatternsAfterPlacementHandler>? _logger;

        public ProcessPatternsAfterPlacementHandler(
            IGridStateService gridService,
            IServiceProvider serviceProvider,
            ILogger<ProcessPatternsAfterPlacementHandler>? logger = null)
        {
            _gridService = gridService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger?.LogDebug("Processing patterns after block placement at {Position}",
                    notification.Position);

                // Process patterns at the placement position
                await ProcessPatternsAtPosition(notification.Position, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing patterns after block placement. Position: {Position}, Error: {ErrorMessage}", 
                    notification.Position, ex.Message);
                // Don't throw - pattern processing failure shouldn't break the placement
            }
        }

        private async Task ProcessPatternsAtPosition(Domain.Common.Vector2Int position, CancellationToken cancellationToken)
        {
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
                
                // Process match patterns as before
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

                _logger?.LogInformation("Found {Count} patterns at {Position} after placement", patterns.Count, position);

                // Execute each pattern
                foreach (var pattern in patterns)
                {
                    if (pattern is MatchPattern matchPattern)
                    {
                        _logger?.LogInformation("Executing match pattern with {Count} blocks after placement", 
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
                            _logger?.LogInformation("Successfully executed pattern {PatternId} after placement", 
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
        }
    }
}