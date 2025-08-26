using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Player;
using BlockLife.Core.Features.Block.Patterns.Models;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Player.Commands;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Patterns.Executors
{
    /// <summary>
    /// Executor for match-3+ patterns.
    /// Removes matched blocks from the grid and awards resources/attributes to the player.
    /// Following ADR-001: Pattern Recognition Framework.
    /// </summary>
    public class MatchPatternExecutor : IPatternExecutor
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MatchPatternExecutor>? _logger;
        private readonly Dictionary<string, double> _performanceMetrics;
        private long _totalExecutions;
        private long _totalBlocksRemoved;

        public MatchPatternExecutor(IMediator mediator, ILogger<MatchPatternExecutor>? logger = null)
        {
            _mediator = mediator;
            _logger = logger;
            _performanceMetrics = new Dictionary<string, double>();
            _totalExecutions = 0;
            _totalBlocksRemoved = 0;
        }

        public PatternType SupportedType => PatternType.Match;

        public string ExecutorId => "MatchPatternExecutor_v1.0";

        public bool IsEnabled => true;

        public async Task<Fin<ExecutionResult>> Execute(IPattern pattern, ExecutionContext context)
        {
            try
            {
                _totalExecutions++;

                if (pattern is not MatchPattern matchPattern)
                    return Error.New($"Pattern is not a MatchPattern: {pattern.GetType().Name}");

                // Step 1: Collect blocks before removal
                var blocksToRemove = new List<BlockLife.Core.Domain.Block.Block>();
                foreach (var position in matchPattern.Positions)
                {
                    var blockResult = context.GridService.GetBlockAt(position);
                    blockResult.IfSome(block => blocksToRemove.Add(block));
                }

                if (!blocksToRemove.Any())
                {
                    _logger?.LogWarning("No blocks found at match positions");
                    return Error.New("No blocks found to remove");
                }

                // Step 2: Remove matched blocks from the grid and notify view
                var removedBlocks = new List<(BlockLife.Core.Domain.Block.Block block, Vector2Int position)>();
                foreach (var position in matchPattern.Positions)
                {
                    // Get the block before removing it (for notification)
                    var blockToRemove = blocksToRemove.FirstOrDefault(b => b.Position == position);
                    
                    var removeResult = context.GridService.RemoveBlock(position);
                    if (removeResult.IsFail)
                    {
                        _logger?.LogWarning("Failed to remove block at {Position}", position);
                    }
                    else if (blockToRemove != null)
                    {
                        removedBlocks.Add((blockToRemove, position));
                    }
                }

                // Step 2b: Publish notifications for removed blocks
                foreach (var (block, position) in removedBlocks)
                {
                    var notification = new BlockRemovedNotification(
                        block.Id,
                        position,
                        block.Type,
                        block.Tier,
                        DateTime.UtcNow
                    );
                    
                    // Fire and forget - don't wait for notification handlers
                    _ = _mediator.Publish(notification);
                    _logger?.LogDebug("Published BlockRemovedNotification for block {BlockId} at {Position}", 
                        block.Id, position);
                }

                _totalBlocksRemoved += removedBlocks.Count;

                // Step 3: Calculate rewards based on removed blocks
                var resourceChanges = Map<ResourceType, int>();
                var attributeChanges = Map<AttributeType, int>();

                foreach (var block in blocksToRemove)
                {
                    var (rewardType, baseAmount) = GetRewardForBlockType(block.Type);
                    
                    // Apply size bonus
                    var sizeBonusMultiplier = GetSizeBonus(matchPattern.Positions.Count);
                    var finalAmount = (int)(baseAmount * sizeBonusMultiplier);

                    switch (rewardType)
                    {
                        case ResourceReward resource:
                            var currentResourceAmount = resourceChanges.Find(resource.Type).Match(
                                Some: v => v,
                                None: () => 0
                            );
                            resourceChanges = resourceChanges.AddOrUpdate(
                                resource.Type, 
                                currentResourceAmount + finalAmount);
                            break;
                        case AttributeReward attribute:
                            var currentAttributeAmount = attributeChanges.Find(attribute.Type).Match(
                                Some: v => v,
                                None: () => 0
                            );
                            attributeChanges = attributeChanges.AddOrUpdate(
                                attribute.Type,
                                currentAttributeAmount + finalAmount);
                            break;
                    }
                }

                // Step 4: Apply rewards to player
                if (resourceChanges.Count > 0 || attributeChanges.Count > 0)
                {
                    var rewardCommand = ApplyMatchRewardsCommand.Create(
                        resourceChanges,
                        attributeChanges,
                        $"Match-{matchPattern.Positions.Count} of {blocksToRemove.FirstOrDefault()?.Type}"
                    );

                    var rewardResult = await _mediator.Send(rewardCommand);
                    if (rewardResult.IsFail)
                    {
                        _logger?.LogWarning("Failed to apply match rewards: {Error}", 
                            rewardResult.Match(Succ: _ => "", Fail: e => e.Message));
                    }
                }

                // Step 5: Create execution result with proper PatternOutcome
                var bonusMultiplier = GetSizeBonus(matchPattern.Positions.Count);
                
                // Convert rewards to the format expected by PatternOutcome
                var resourceRewardSeq = resourceChanges
                    .Select(kv => (kv.Key.ToString(), kv.Value))
                    .ToSeq();
                
                var attributeRewardSeq = attributeChanges
                    .Select(kv => (kv.Key.ToString(), kv.Value))
                    .ToSeq();

                var outcome = new PatternOutcome
                {
                    RemovedPositions = matchPattern.Positions.ToSeq(),
                    CreatedBlocks = Seq<(Vector2Int, BlockType)>(),
                    ModifiedBlocks = Seq<(Vector2Int, BlockType)>(),
                    ResourceRewards = resourceRewardSeq,
                    AttributeRewards = attributeRewardSeq,
                    ScoreReward = matchPattern.Positions.Count * 10,
                    BonusMultiplier = bonusMultiplier,
                    CanTriggerChains = true
                };

                // Create chain trigger positions (adjacent to removed blocks)
                var chainTriggers = matchPattern.Positions
                    .SelectMany(p => p.GetOrthogonallyAdjacentPositions())
                    .Where(p => context.GridService.IsValidPosition(p))
                    .Distinct()
                    .ToSeq();

                var result = ExecutionResult.SuccessWithChains(
                    outcome, 
                    outcome, 
                    chainTriggers, 
                    context.ElapsedMs);

                _logger?.LogInformation("Executed match pattern: {Blocks} blocks removed, {Resources} resources, {Attributes} attributes",
                    removedBlocks.Count, resourceChanges.Count, attributeChanges.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing match pattern");
                return Error.New($"Failed to execute match pattern: {ex.Message}");
            }
        }

        public Task<Fin<bool>> CanExecute(IPattern pattern, ExecutionContext context)
        {
            if (pattern is not MatchPattern matchPattern)
                return Task.FromResult(Fin<bool>.Succ(false));

            // Verify all positions still have blocks
            var allPositionsValid = matchPattern.Positions.All(pos =>
                context.GridService.IsValidPosition(pos) && 
                context.GridService.IsPositionOccupied(pos));

            return Task.FromResult(Fin<bool>.Succ(allPositionsValid));
        }

        public double EstimateExecutionTime(IPattern pattern)
        {
            if (pattern is not MatchPattern matchPattern)
                return 0.0;

            // Estimate ~1ms per block removed + overhead
            return 5.0 + (matchPattern.Positions.Count * 1.0);
        }

        public Seq<(string Metric, double Value)> GetPerformanceMetrics()
        {
            return Seq(
                ("TotalExecutions", (double)_totalExecutions),
                ("TotalBlocksRemoved", (double)_totalBlocksRemoved),
                ("AverageBlocksPerExecution", _totalExecutions > 0 ? (double)_totalBlocksRemoved / _totalExecutions : 0.0)
            );
        }

        public Fin<LanguageExt.Unit> ValidateConfiguration()
        {
            // Verify we can access required services
            if (_mediator == null)
                return Error.New("Mediator is not configured");

            return LanguageExt.Unit.Default;
        }

        /// <summary>
        /// Maps block types to their reward types and amounts.
        /// Based on actual BlockType enum values in the codebase.
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

        /// <summary>
        /// Calculates bonus multiplier based on match size.
        /// </summary>
        private static double GetSizeBonus(int matchSize)
        {
            return matchSize switch
            {
                3 => 1.0,   // Base rewards
                4 => 1.5,   // 50% bonus
                5 => 2.0,   // 100% bonus
                _ when matchSize >= 6 => 3.0, // 200% bonus
                _ => 1.0
            };
        }

        // Helper discriminated union for reward types
        private abstract record RewardType;
        private sealed record ResourceReward(ResourceType Type) : RewardType;
        private sealed record AttributeReward(AttributeType Type) : RewardType;
    }
}