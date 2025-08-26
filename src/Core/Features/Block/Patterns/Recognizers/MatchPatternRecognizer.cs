using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlockLife.Core.Features.Block.Patterns.Recognizers
{
    /// <summary>
    /// Recognizer for match-3+ patterns using flood-fill algorithm.
    /// Finds connected components of 3+ blocks of the same type.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public class MatchPatternRecognizer : IPatternRecognizer
    {
        private readonly Dictionary<string, double> _performanceMetrics;
        private long _totalRecognitions;
        private long _totalMatchesFound;

        public MatchPatternRecognizer()
        {
            _performanceMetrics = new Dictionary<string, double>();
            _totalRecognitions = 0;
            _totalMatchesFound = 0;
        }

        /// <summary>
        /// The type of pattern this recognizer can find.
        /// </summary>
        public PatternType SupportedType => PatternType.Match;

        /// <summary>
        /// Whether this recognizer is currently enabled.
        /// Match patterns are always enabled as they're core game mechanics.
        /// </summary>
        public bool IsEnabled => PatternType.Match.IsEnabled();

        /// <summary>
        /// Unique identifier for this recognizer implementation.
        /// </summary>
        public string RecognizerId => "MatchPatternRecognizer_v1.0";

        /// <summary>
        /// Finds all match patterns involving or adjacent to the trigger position.
        /// Uses flood-fill algorithm to identify connected components of same-type blocks.
        /// </summary>
        public Fin<Seq<IPattern>> Recognize(IGridStateService gridService, Vector2Int triggerPosition, PatternContext context)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _totalRecognitions++;

                // Early exit if disabled or invalid context
                if (!IsEnabled)
                    return Fin<Seq<IPattern>>.Succ(Seq<IPattern>());

                if (context.TargetPatternTypes.Any() && !context.TargetPatternTypes.Contains(PatternType.Match))
                    return Fin<Seq<IPattern>>.Succ(Seq<IPattern>());

                // Get positions to search from (trigger position + its neighborhood)
                var searchPositions = GetSearchPositions(gridService, triggerPosition);

                // Find all match patterns using flood-fill for each starting position
                var allPatterns = FindAllMatchPatterns(gridService, searchPositions, context.MaxPatternsPerType);

                stopwatch.Stop();
                UpdatePerformanceMetrics(stopwatch.ElapsedMilliseconds, allPatterns.Count);

                _totalMatchesFound += allPatterns.Count;

                return Fin<Seq<IPattern>>.Succ(allPatterns.Cast<IPattern>().ToSeq());
            }
            catch (Exception ex)
            {
                return Fin<Seq<IPattern>>.Fail(Error.New("MatchPatternRecognizer failed", ex));
            }
        }

        /// <summary>
        /// Performs a quick check to see if match patterns could exist at the given position.
        /// Optimizes performance by avoiding expensive flood-fill when impossible.
        /// </summary>
        public bool CanRecognizeAt(IGridStateService gridService, Vector2Int position)
        {
            // Must have a block at the trigger position
            if (gridService.IsPositionEmpty(position))
                return false;

            // Must have at least 1 adjacent block for potential matches
            // (Pattern validation happens in full recognition algorithm)
            var adjacentOccupiedCount = position.GetOrthogonallyAdjacentPositions()
                .Count(pos => gridService.IsValidPosition(pos) && gridService.IsPositionOccupied(pos));

            return adjacentOccupiedCount >= 1;
        }

        /// <summary>
        /// Gets performance statistics for this recognizer.
        /// </summary>
        public Seq<(string Metric, double Value)> GetPerformanceMetrics()
        {
            var baseMetrics = _performanceMetrics.Select(kvp => (kvp.Key, kvp.Value)).ToSeq();
            var additionalMetrics = Seq<(string, double)>(
                ("TotalRecognitions", (double)_totalRecognitions),
                ("TotalMatchesFound", (double)_totalMatchesFound),
                ("AverageMatchesPerRecognition", _totalRecognitions > 0 ? (double)_totalMatchesFound / _totalRecognitions : 0)
            );
            return baseMetrics.Concat(additionalMetrics);
        }

        /// <summary>
        /// Validates that this recognizer's configuration is correct.
        /// </summary>
        public Fin<Unit> ValidateConfiguration()
        {
            if (!IsEnabled)
                return Fin<Unit>.Fail(Error.New("MatchPatternRecognizer is disabled"));

            if (SupportedType != PatternType.Match)
                return Fin<Unit>.Fail(Error.New("MatchPatternRecognizer misconfigured - wrong pattern type"));

            return Fin<Unit>.Succ(Unit.Default);
        }

        /// <summary>
        /// Gets all positions to search from, including the trigger and nearby positions
        /// that could be part of matches involving the trigger position.
        /// Uses expanded search radius to find patterns adjacent to the trigger area.
        /// </summary>
        private Seq<Vector2Int> GetSearchPositions(IGridStateService gridService, Vector2Int triggerPosition)
        {
            var searchPositions = new System.Collections.Generic.HashSet<Vector2Int> { triggerPosition };
            
            // Add immediate neighbors (distance 1 from trigger)
            foreach (var adjacent in triggerPosition.GetOrthogonallyAdjacentPositions())
            {
                if (gridService.IsValidPosition(adjacent))
                {
                    searchPositions.Add(adjacent);
                    
                    // Add neighbors of neighbors if they contain blocks (distance 2 from trigger)
                    foreach (var neighbor in adjacent.GetOrthogonallyAdjacentPositions())
                    {
                        if (gridService.IsValidPosition(neighbor) && gridService.IsPositionOccupied(neighbor))
                        {
                            searchPositions.Add(neighbor);
                        }
                    }
                }
            }
            
            // Only return positions that contain blocks for actual pattern searching
            return searchPositions
                .Where(pos => gridService.IsPositionOccupied(pos))
                .ToSeq();
        }

        /// <summary>
        /// Finds all match patterns using flood-fill algorithm from the given search positions.
        /// Returns unique patterns without duplicates.
        /// </summary>
        private Seq<MatchPattern> FindAllMatchPatterns(IGridStateService gridService, Seq<Vector2Int> searchPositions, int maxPatterns)
        {
            var foundPatterns = new List<MatchPattern>();
            var processedPositions = new System.Collections.Generic.HashSet<Vector2Int>();

            foreach (var startPosition in searchPositions)
            {
                if (processedPositions.Contains(startPosition))
                    continue;

                // Get block at starting position
                var blockOption = gridService.GetBlockAt(startPosition);
                if (blockOption.IsNone)
                    continue;

                var block = blockOption.IfNone(() => throw new InvalidOperationException("Block should exist"));

                // Run flood-fill to find connected component
                var connectedPositions = FloodFillFindMatches(gridService, startPosition, block.Type);

                // Mark all positions in this component as processed
                foreach (var pos in connectedPositions)
                {
                    processedPositions.Add(pos);
                }

                // Create pattern if we have enough blocks
                if (connectedPositions.Count >= 3)
                {
                    var pattern = MatchPattern.Create(connectedPositions, block.Type);
                    pattern.IfSome(p => foundPatterns.Add(p));

                    // Stop if we've reached the maximum
                    if (foundPatterns.Count >= maxPatterns)
                        break;
                }
            }

            return foundPatterns.ToSeq();
        }

        /// <summary>
        /// Flood-fill algorithm to find all connected blocks of the same type.
        /// Uses breadth-first search to identify connected components.
        /// Leverages LanguageExt Seq for efficient immutable collections.
        /// </summary>
        private Seq<Vector2Int> FloodFillFindMatches(IGridStateService gridService, Vector2Int startPosition, BlockType targetType)
        {
            var visited = new System.Collections.Generic.HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            var matchedPositions = new List<Vector2Int>();

            queue.Enqueue(startPosition);
            visited.Add(startPosition);

            while (queue.Count > 0)
            {
                var currentPosition = queue.Dequeue();

                // Verify current position has the correct block type
                var blockAtPosition = gridService.GetBlockAt(currentPosition);
                if (blockAtPosition.IsNone || blockAtPosition.IfNone(() => new BlockLife.Core.Domain.Block.Block 
                    { 
                        Id = Guid.Empty, 
                        Type = BlockType.Basic, 
                        Position = Vector2Int.Zero, 
                        Tier = 1, 
                        CreatedAt = DateTime.MinValue, 
                        LastModifiedAt = DateTime.MinValue 
                    }).Type != targetType)
                    continue;

                matchedPositions.Add(currentPosition);

                // Check all orthogonally adjacent positions
                foreach (var adjacentPosition in currentPosition.GetOrthogonallyAdjacentPositions())
                {
                    // Skip if already visited or out of bounds
                    if (visited.Contains(adjacentPosition) || !gridService.IsValidPosition(adjacentPosition))
                        continue;

                    // Skip if no block at position
                    if (gridService.IsPositionEmpty(adjacentPosition))
                        continue;

                    // Check if adjacent block matches our target type
                    var adjacentBlock = gridService.GetBlockAt(adjacentPosition);
                    if (adjacentBlock.IsSome && adjacentBlock.IfNone(() => new BlockLife.Core.Domain.Block.Block 
                        { 
                            Id = Guid.Empty, 
                            Type = BlockType.Basic, 
                            Position = Vector2Int.Zero, 
                            Tier = 1, 
                            CreatedAt = DateTime.MinValue, 
                            LastModifiedAt = DateTime.MinValue 
                        }).Type == targetType)
                    {
                        visited.Add(adjacentPosition);
                        queue.Enqueue(adjacentPosition);
                    }
                }
            }

            return matchedPositions.ToSeq();
        }

        /// <summary>
        /// Updates internal performance metrics for monitoring and optimization.
        /// </summary>
        private void UpdatePerformanceMetrics(long elapsedMs, int patternsFound)
        {
            _performanceMetrics["LastRecognitionTimeMs"] = elapsedMs;
            _performanceMetrics["AverageRecognitionTimeMs"] = 
                (_performanceMetrics.GetValueOrDefault("AverageRecognitionTimeMs") * (_totalRecognitions - 1) + elapsedMs) / _totalRecognitions;
        }
    }
}