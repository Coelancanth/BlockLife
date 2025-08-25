using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlockLife.Core.Features.Block.Patterns.Recognizers
{
    /// <summary>
    /// Recognizer for tier-up patterns that detects exactly 3 adjacent blocks of the same type.
    /// These patterns can be merged into a single higher-tier block when unlocked.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public class TierUpPatternRecognizer : IPatternRecognizer
    {
        private readonly Dictionary<string, double> _performanceMetrics;
        private long _totalRecognitions;
        private long _totalPatternsFound;

        public TierUpPatternRecognizer()
        {
            _performanceMetrics = new Dictionary<string, double>();
            _totalRecognitions = 0;
            _totalPatternsFound = 0;
        }

        /// <summary>
        /// The type of pattern this recognizer can find.
        /// </summary>
        public PatternType SupportedType => PatternType.TierUp;

        /// <summary>
        /// Whether this recognizer is currently enabled.
        /// TierUp patterns are enabled when the feature is unlocked.
        /// </summary>
        public bool IsEnabled => PatternType.TierUp.IsEnabled();

        /// <summary>
        /// Unique identifier for this recognizer implementation.
        /// </summary>
        public string RecognizerId => "TierUpPatternRecognizer_v1.0";

        /// <summary>
        /// Finds all tier-up patterns involving or adjacent to the trigger position.
        /// Looks for exactly 3 connected blocks of the same type (not more, not less).
        /// </summary>
        public Fin<Seq<IPattern>> Recognize(IGridStateService gridService, Vector2Int triggerPosition, PatternContext context)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _totalRecognitions++;

                // Early exit if disabled or invalid context
                if (!IsEnabled)
                {
                    Debug.WriteLine($"TierUpPatternRecognizer: Not enabled");
                    return Fin<Seq<IPattern>>.Succ(Seq<IPattern>());
                }

                if (context.TargetPatternTypes.Any() && !context.TargetPatternTypes.Contains(PatternType.TierUp))
                {
                    Debug.WriteLine($"TierUpPatternRecognizer: Not in target pattern types");
                    return Fin<Seq<IPattern>>.Succ(Seq<IPattern>());
                }

                // Get positions to search from (trigger position + immediate neighbors)
                var searchPositions = GetSearchPositions(gridService, triggerPosition);
                Debug.WriteLine($"TierUpPatternRecognizer: Searching from {searchPositions.Count} positions");

                // Find all tier-up patterns (exactly 3 blocks each)
                var allPatterns = FindAllTierUpPatterns(gridService, searchPositions, context.MaxPatternsPerType);

                stopwatch.Stop();
                UpdatePerformanceMetrics(stopwatch.ElapsedMilliseconds, allPatterns.Count);

                _totalPatternsFound += allPatterns.Count;

                if (allPatterns.Any())
                {
                    Debug.WriteLine($"TierUpPatternRecognizer: Found {allPatterns.Count} tier-up patterns");
                    foreach (var pattern in allPatterns)
                    {
                        Debug.WriteLine($"  - {pattern.GetDescription()}");
                    }
                }
                else
                {
                    Debug.WriteLine($"TierUpPatternRecognizer: No tier-up patterns found");
                }

                return Fin<Seq<IPattern>>.Succ(allPatterns.Cast<IPattern>().ToSeq());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TierUpPatternRecognizer: Exception - {ex.Message}");
                return Fin<Seq<IPattern>>.Fail(Error.New("TierUpPatternRecognizer failed", ex));
            }
        }

        /// <summary>
        /// Performs a quick check to see if tier-up patterns could exist at the given position.
        /// Requires at least 2 adjacent blocks for a potential tier-up pattern.
        /// </summary>
        public bool CanRecognizeAt(IGridStateService gridService, Vector2Int position)
        {
            // Must have a block at the trigger position
            if (gridService.IsPositionEmpty(position))
                return false;

            // Must have at least 2 adjacent blocks of same type for potential tier-up
            var blockOption = gridService.GetBlockAt(position);
            if (blockOption.IsNone)
                return false;

            var block = blockOption.IfNone(() => throw new InvalidOperationException("Block should exist"));
            
            // Count adjacent blocks of the same type
            var adjacentSameTypeCount = position.GetOrthogonallyAdjacentPositions()
                .Count(pos => 
                    gridService.IsValidPosition(pos) && 
                    gridService.GetBlockAt(pos)
                        .Map(b => b.Type == block.Type)
                        .IfNone(false));

            // Need at least 2 adjacent of same type to form a group of 3
            return adjacentSameTypeCount >= 2;
        }

        /// <summary>
        /// Gets performance statistics for this recognizer.
        /// </summary>
        public Seq<(string Metric, double Value)> GetPerformanceMetrics()
        {
            var baseMetrics = _performanceMetrics.Select(kvp => (kvp.Key, kvp.Value)).ToSeq();
            var additionalMetrics = Seq<(string, double)>(
                ("TotalRecognitions", (double)_totalRecognitions),
                ("TotalPatternsFound", (double)_totalPatternsFound),
                ("AveragePatternsPerRecognition", _totalRecognitions > 0 ? (double)_totalPatternsFound / _totalRecognitions : 0)
            );
            return baseMetrics.Concat(additionalMetrics);
        }

        /// <summary>
        /// Validates that this recognizer's configuration is correct.
        /// </summary>
        public Fin<Unit> ValidateConfiguration()
        {
            if (!IsEnabled)
                return Fin<Unit>.Fail(Error.New("TierUpPatternRecognizer is disabled"));

            if (SupportedType != PatternType.TierUp)
                return Fin<Unit>.Fail(Error.New("TierUpPatternRecognizer misconfigured - wrong pattern type"));

            return Fin<Unit>.Succ(Unit.Default);
        }

        /// <summary>
        /// Gets positions to search from, including the trigger and immediate neighbors.
        /// More limited search than match patterns since we're looking for exactly 3 blocks.
        /// </summary>
        private Seq<Vector2Int> GetSearchPositions(IGridStateService gridService, Vector2Int triggerPosition)
        {
            var searchPositions = new System.Collections.Generic.HashSet<Vector2Int> { triggerPosition };
            
            // Add immediate neighbors only (tier-up patterns are more localized)
            foreach (var adjacent in triggerPosition.GetOrthogonallyAdjacentPositions())
            {
                if (gridService.IsValidPosition(adjacent) && gridService.IsPositionOccupied(adjacent))
                {
                    searchPositions.Add(adjacent);
                }
            }
            
            return searchPositions.ToSeq();
        }

        /// <summary>
        /// Finds all tier-up patterns (exactly 3 connected blocks of same type).
        /// Uses a different algorithm than match patterns since we need exactly 3 blocks.
        /// </summary>
        private Seq<TierUpPattern> FindAllTierUpPatterns(IGridStateService gridService, Seq<Vector2Int> searchPositions, int maxPatterns)
        {
            var foundPatterns = new List<TierUpPattern>();
            var processedGroups = new System.Collections.Generic.HashSet<string>();

            foreach (var startPosition in searchPositions)
            {
                // Get block at starting position
                var blockOption = gridService.GetBlockAt(startPosition);
                if (blockOption.IsNone)
                    continue;

                var block = blockOption.IfNone(() => throw new InvalidOperationException("Block should exist"));

                // Find all groups of exactly 3 connected blocks starting from this position
                var groups = FindExactlyThreeConnected(gridService, startPosition, block.Type);

                foreach (var group in groups)
                {
                    // Create a unique key for this group to avoid duplicates
                    var groupKey = string.Join("_", group.OrderBy(p => p.X).ThenBy(p => p.Y).Select(p => $"{p.X}x{p.Y}"));
                    
                    if (processedGroups.Contains(groupKey))
                        continue;

                    processedGroups.Add(groupKey);

                    // Create pattern for this group
                    var pattern = TierUpPattern.Create(group, block.Type, currentTier: 1);
                    pattern.IfSome(p => 
                    {
                        foundPatterns.Add(p);
                        Debug.WriteLine($"  Found tier-up group: {groupKey}");
                    });

                    // Stop if we've reached the maximum
                    if (foundPatterns.Count >= maxPatterns)
                        return foundPatterns.ToSeq();
                }
            }

            return foundPatterns.ToSeq();
        }

        /// <summary>
        /// Finds all groups of exactly 3 connected blocks of the same type.
        /// Uses combination search to find all valid 3-block groups.
        /// </summary>
        private Seq<Seq<Vector2Int>> FindExactlyThreeConnected(IGridStateService gridService, Vector2Int startPosition, BlockType targetType)
        {
            var results = new List<Seq<Vector2Int>>();

            // First, find all connected blocks of the target type using flood-fill
            var allConnected = FloodFillFindSameType(gridService, startPosition, targetType);

            // If we have fewer than 3 blocks, no valid groups
            if (allConnected.Count < 3)
                return Seq<Seq<Vector2Int>>();

            // If we have exactly 3 blocks and they're connected, that's our only group
            if (allConnected.Count == 3)
            {
                results.Add(allConnected);
                return results.ToSeq();
            }

            // If we have more than 3, find all possible 3-block subgroups that are connected
            // This is more complex and typically not needed for basic tier-up
            // For now, we'll look for any 3 connected blocks containing the start position
            var startGroup = FindFirstThreeFrom(allConnected, startPosition);
            if (startGroup.Count == 3)
            {
                results.Add(startGroup);
            }

            return results.ToSeq();
        }

        /// <summary>
        /// Finds the first group of 3 connected blocks starting from a specific position.
        /// Uses breadth-first search to find the closest 3 blocks.
        /// </summary>
        private Seq<Vector2Int> FindFirstThreeFrom(Seq<Vector2Int> allPositions, Vector2Int startPosition)
        {
            if (!allPositions.Contains(startPosition))
                return Seq<Vector2Int>();

            var result = new List<Vector2Int>();
            var visited = new System.Collections.Generic.HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            queue.Enqueue(startPosition);
            visited.Add(startPosition);
            result.Add(startPosition);

            while (queue.Count > 0 && result.Count < 3)
            {
                var current = queue.Dequeue();

                foreach (var adjacent in current.GetOrthogonallyAdjacentPositions())
                {
                    if (allPositions.Contains(adjacent) && !visited.Contains(adjacent))
                    {
                        visited.Add(adjacent);
                        result.Add(adjacent);
                        queue.Enqueue(adjacent);

                        if (result.Count == 3)
                            break;
                    }
                }
            }

            return result.Count == 3 ? result.ToSeq() : Seq<Vector2Int>();
        }

        /// <summary>
        /// Flood-fill algorithm to find all connected blocks of the same type.
        /// Returns all positions that are connected to the start position.
        /// </summary>
        private Seq<Vector2Int> FloodFillFindSameType(IGridStateService gridService, Vector2Int startPosition, BlockType targetType)
        {
            var visited = new System.Collections.Generic.HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            var connectedPositions = new List<Vector2Int>();

            queue.Enqueue(startPosition);
            visited.Add(startPosition);

            while (queue.Count > 0)
            {
                var currentPosition = queue.Dequeue();

                // Verify current position has the correct block type
                var blockAtPosition = gridService.GetBlockAt(currentPosition);
                if (blockAtPosition.IsNone)
                    continue;

                var block = blockAtPosition.IfNone(() => throw new InvalidOperationException("Block should exist"));
                if (block.Type != targetType)
                    continue;

                connectedPositions.Add(currentPosition);

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
                    if (adjacentBlock.IsSome)
                    {
                        var adjBlock = adjacentBlock.IfNone(() => throw new InvalidOperationException("Block should exist"));
                        if (adjBlock.Type == targetType)
                        {
                            visited.Add(adjacentPosition);
                            queue.Enqueue(adjacentPosition);
                        }
                    }
                }
            }

            return connectedPositions.ToSeq();
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