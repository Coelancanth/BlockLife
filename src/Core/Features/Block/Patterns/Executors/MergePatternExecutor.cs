using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Player;
using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Block.Patterns.Models;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Player.Commands;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Patterns.Executors
{
    /// <summary>
    /// Executor for merge patterns - combines 3+ blocks into a single higher-tier block.
    /// Per Glossary: Merge replaces Match behavior when merge-to-next-tier is unlocked.
    /// </summary>
    public class MergePatternExecutor : IPatternExecutor
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MergePatternExecutor>? _logger;

        public MergePatternExecutor(IMediator mediator, ILogger<MergePatternExecutor>? logger = null)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public PatternType SupportedType => PatternType.Match; // Merges are executed on Match patterns

        public string ExecutorId => "MergePatternExecutor_v1.0";

        public bool IsEnabled => true; // Enabled when merge is unlocked (checked by resolver)

        public async Task<Fin<ExecutionResult>> Execute(IPattern pattern, ExecutionContext context)
        {
            try
            {
                // Validate context and services
                if (context == null)
                    return Error.New("ExecutionContext cannot be null");
                    
                if (context.GridService == null)
                    return Error.New("GridService is required for merge pattern execution");
                    
                // Validate pattern type
                if (pattern is not MatchPattern matchPattern)
                    return Error.New($"Pattern is not a MatchPattern: {pattern.GetType().Name}");

                // Validate pattern has positions (Seq is a value type, check Count)
                if (matchPattern.Positions.Count == 0)
                    return Error.New("Pattern has no positions to merge");

                // Validate trigger position is available (required for merge)
                if (!context.TriggerPosition.HasValue)
                    return Error.New("Merge patterns require a trigger position for result placement");

                var triggerPosition = context.TriggerPosition.Value;

                // Step 1: Collect blocks before removal
                var blocksToRemove = new List<BlockLife.Core.Domain.Block.Block>();
                foreach (var position in matchPattern.Positions)
                {
                    var blockResult = context.GridService.GetBlockAt(position);
                    blockResult.IfSome(block => blocksToRemove.Add(block));
                }

                if (!blocksToRemove.Any())
                {
                    _logger?.LogWarning("No blocks found at merge positions");
                    return Error.New("No blocks found to merge");
                }

                // Step 2: Validate all blocks are same tier and type, then determine merge result
                var firstBlock = blocksToRemove.FirstOrDefault();
                if (firstBlock == null)
                {
                    _logger?.LogWarning("No valid blocks found to merge");
                    return Error.New("No valid blocks found at merge positions");
                }
                
                var expectedTier = firstBlock.Tier;
                var expectedType = firstBlock.Type;
                
                // CRITICAL: Verify all blocks are exactly the same tier and type
                foreach (var block in blocksToRemove)
                {
                    if (block.Tier != expectedTier || block.Type != expectedType)
                    {
                        _logger?.LogError("Mixed tier/type merge attempt: Expected {ExpectedType}-T{ExpectedTier}, found {ActualType}-T{ActualTier}", 
                            expectedType, expectedTier, block.Type, block.Tier);
                        return Error.New($"Cannot merge blocks with different tiers or types. Expected: {expectedType}-T{expectedTier}, Found: {block.Type}-T{block.Tier}");
                    }
                }
                
                var newTier = expectedTier + 1;
                
                // Validate tier bounds (max tier is T4)
                const int MAX_TIER = 4;
                if (newTier > MAX_TIER)
                {
                    _logger?.LogWarning("Cannot merge beyond T{MaxTier}. Current tier: {CurrentTier}", MAX_TIER, expectedTier);
                    return Error.New($"Cannot merge blocks beyond T{MAX_TIER}. Current blocks are T{expectedTier}");
                }
                
                var blockType = expectedType;

                _logger?.LogDebug("Merging {Count} {BlockType}-T{Tier} blocks into 1 {BlockType}-T{NewTier} at {TriggerPosition}", 
                    blocksToRemove.Count, blockType, expectedTier, blockType, newTier, triggerPosition);

                // Step 3: Remove matched blocks from the grid and notify view
                var removedBlocks = new List<(BlockLife.Core.Domain.Block.Block block, Vector2Int position)>();
                
                // Create a copy of positions to avoid concurrent modification issues
                var positionsToProcess = matchPattern.Positions.ToList();
                
                foreach (var position in positionsToProcess)
                {
                    var blockToRemove = blocksToRemove.FirstOrDefault(b => b != null && b.Position == position);
                    
                    var removeResult = context.GridService.RemoveBlock(position);
                    if (removeResult.IsFail)
                    {
                        _logger?.LogWarning("Failed to remove block at {Position} during merge", position);
                    }
                    else if (blockToRemove != null)
                    {
                        removedBlocks.Add((blockToRemove, position));
                    }
                }

                // Step 3b: Publish notifications for removed blocks
                foreach (var (block, position) in removedBlocks)
                {
                    var removeNotification = new BlockRemovedNotification(
                        block.Id,
                        position,
                        block.Type,
                        block.Tier,
                        DateTime.UtcNow
                    );
                    
                    _ = _mediator.Publish(removeNotification);
                    _logger?.LogDebug("Published BlockRemovedNotification for merged block {BlockId} at {Position}", 
                        block.Id, position);
                }

                // Step 4: Create the merged block at trigger position
                var mergedBlock = BlockLife.Core.Domain.Block.Block.CreateNew(blockType, triggerPosition, newTier);
                
                var placeResult = context.GridService.PlaceBlock(mergedBlock);
                if (placeResult.IsFail)
                {
                    _logger?.LogError("Failed to place merged block at trigger position {TriggerPosition}", triggerPosition);
                    return Error.New($"Failed to place merged block at {triggerPosition}");
                }

                // Step 4b: Publish notification for created block
                var placeNotification = new BlockPlacedNotification(
                    mergedBlock.Id,
                    triggerPosition,
                    blockType,
                    newTier,
                    DateTime.UtcNow
                );
                _ = _mediator.Publish(placeNotification);
                _logger?.LogDebug("Published BlockPlacedNotification for merged block {BlockId} at {Position}", 
                    mergedBlock.Id, triggerPosition);

                // Step 5: Calculate tier-scaled rewards (T2 = 3x base, T3 = 9x base)
                var tierMultiplier = Math.Pow(3, newTier - 1); // T1=1, T2=3, T3=9, T4=27
                var resourceChanges = Map<ResourceType, int>();
                var attributeChanges = Map<AttributeType, int>();

                // Calculate base rewards for the merged block type
                var (rewardType, baseAmount) = GetRewardForBlockType(blockType);
                var tierScaledAmount = (int)(baseAmount * tierMultiplier);

                switch (rewardType)
                {
                    case ResourceReward resource:
                        resourceChanges = resourceChanges.AddOrUpdate(resource.Type, tierScaledAmount);
                        break;
                    case AttributeReward attribute:
                        attributeChanges = attributeChanges.AddOrUpdate(attribute.Type, tierScaledAmount);
                        break;
                }

                // Step 6: Apply rewards to player
                if (resourceChanges.Count > 0 || attributeChanges.Count > 0)
                {
                    var rewardCommand = ApplyMatchRewardsCommand.Create(
                        resourceChanges,
                        attributeChanges,
                        $"Merge-{removedBlocks.Count}→T{newTier} {blockType}"
                    );

                    var rewardResult = await _mediator.Send(rewardCommand);
                    if (rewardResult.IsFail)
                    {
                        _logger?.LogWarning("Failed to apply merge rewards: {Error}", 
                            rewardResult.Match(Succ: _ => "", Fail: e => e.Message));
                    }
                    else
                    {
                        // DEBUG: Show resource changes
                        var resourceList = resourceChanges.Select(kv => $"{kv.Key}:+{kv.Value}").ToList();
                        var attributeList = attributeChanges.Select(kv => $"{kv.Key}:+{kv.Value}").ToList();
                        
                        var allRewards = new List<string>();
                        allRewards.AddRange(resourceList);
                        allRewards.AddRange(attributeList);

                        if (allRewards.Any())
                        {
                            _logger?.LogInformation("🚀 MERGE REWARDS: {Rewards} (from {Count} T{Tier} {BlockType} → 1 T{NewTier} {BlockType})", 
                                string.Join(", ", allRewards), 
                                removedBlocks.Count,
                                expectedTier,
                                blockType,
                                newTier,
                                blockType);
                        }
                    }
                }

                // Step 7: Create execution result
                var resourceRewardSeq = resourceChanges
                    .Select(kv => (kv.Key.ToString(), kv.Value))
                    .ToSeq();
                
                var attributeRewardSeq = attributeChanges
                    .Select(kv => (kv.Key.ToString(), kv.Value))
                    .ToSeq();

                var outcome = new PatternOutcome
                {
                    RemovedPositions = matchPattern.Positions.ToSeq(),
                    CreatedBlocks = Seq<(Vector2Int, BlockType)>(new[] { (triggerPosition, blockType) }),
                    ModifiedBlocks = Seq<(Vector2Int, BlockType)>(),
                    ResourceRewards = resourceRewardSeq,
                    AttributeRewards = attributeRewardSeq,
                    ScoreReward = (int)(removedBlocks.Count * tierMultiplier * 10),
                    BonusMultiplier = tierMultiplier,
                    CanTriggerChains = true
                };

                var result = ExecutionResult.Success(outcome, outcome, context.ElapsedMs);

                _logger?.LogInformation("Executed merge pattern: {Count} T{OldTier} blocks → 1 T{NewTier} {BlockType} at {Position}",
                    removedBlocks.Count, firstBlock.Tier, newTier, blockType, triggerPosition);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing merge pattern");
                return Error.New($"Failed to execute merge pattern: {ex.Message}");
            }
        }

        public async Task<Fin<bool>> CanExecute(IPattern pattern, ExecutionContext context)
        {
            // Validate prerequisites
            if (pattern == null)
                return Error.New("Pattern cannot be null");
                
            if (context?.GridService == null)
                return Error.New("Valid context with GridService is required");
            
            // Can execute any Match pattern when merge is unlocked
            await Task.CompletedTask;
            return Fin<bool>.Succ(pattern.Type == PatternType.Match && pattern.IsValidFor(context.GridService));
        }

        public double EstimateExecutionTime(IPattern pattern)
        {
            // Defensive check for null pattern
            if (pattern == null)
                return 0.0;
                
            // Merge is relatively fast - estimate based on block count
            return pattern.Positions.Count * 10.0; // 10ms per block
        }

        public Seq<(string Metric, double Value)> GetPerformanceMetrics()
        {
            return Seq<(string Metric, double Value)>();
        }

        public Fin<LanguageExt.Unit> ValidateConfiguration()
        {
            // Verify we can access required services
            if (_mediator == null)
                return Error.New("Mediator is not configured");

            return LanguageExt.Unit.Default;
        }

        /// <summary>
        /// Maps block types to their reward types and amounts for merge system.
        /// Same base rewards as Match system, but will be tier-multiplied.
        /// </summary>
        private static (RewardType Type, int Amount) GetRewardForBlockType(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Work => (new ResourceReward(ResourceType.Money), 10),
                BlockType.Study => (new AttributeReward(AttributeType.Knowledge), 10),
                BlockType.Health => (new AttributeReward(AttributeType.Health), 10),
                BlockType.Relationship => (new ResourceReward(ResourceType.SocialCapital), 10),
                BlockType.Fun => (new AttributeReward(AttributeType.Happiness), 10),
                BlockType.Creativity => (new AttributeReward(AttributeType.Creativity), 10),
                // Special types give more rewards
                BlockType.CareerOpportunity => (new ResourceReward(ResourceType.Money), 25),
                BlockType.Partnership => (new ResourceReward(ResourceType.SocialCapital), 30),
                BlockType.Passion => (new AttributeReward(AttributeType.Happiness), 20),
                // Default for Basic or unknown
                _ => (new ResourceReward(ResourceType.Money), 5)
            };
        }

        // Helper discriminated union for reward types
        private abstract record RewardType;
        private sealed record ResourceReward(ResourceType Type) : RewardType;
        private sealed record AttributeReward(AttributeType Type) : RewardType;
    }
}