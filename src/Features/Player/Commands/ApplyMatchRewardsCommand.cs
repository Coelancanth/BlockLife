using BlockLife.Core.Application.Commands;
using BlockLife.Core.Domain.Player;
using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Player.Commands
{
    /// <summary>
    /// Command to apply resource and attribute rewards from match-3 patterns.
    /// This is the primary command for VS_003A match-3 integration with player state.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record ApplyMatchRewardsCommand : ICommand<PlayerState>
    {
        /// <summary>
        /// Resources to add/spend (negative values for spending).
        /// </summary>
        public required Map<ResourceType, int> ResourceChanges { get; init; }

        /// <summary>
        /// Attributes to increase (must be positive values).
        /// </summary>
        public required Map<AttributeType, int> AttributeChanges { get; init; }

        /// <summary>
        /// Optional description of the match that triggered these rewards.
        /// </summary>
        public string? MatchDescription { get; init; }

        /// <summary>
        /// Creates a new ApplyMatchRewardsCommand with the specified rewards.
        /// </summary>
        public static ApplyMatchRewardsCommand Create(
            Map<ResourceType, int> resourceChanges, 
            Map<AttributeType, int> attributeChanges,
            string? matchDescription = null) =>
            new()
            {
                ResourceChanges = resourceChanges,
                AttributeChanges = attributeChanges,
                MatchDescription = matchDescription
            };

        /// <summary>
        /// Creates a command for simple resource rewards (common case).
        /// </summary>
        public static ApplyMatchRewardsCommand CreateResourceReward(
            ResourceType resourceType, 
            int amount,
            string? matchDescription = null) =>
            Create(
                Map((resourceType, amount)),
                Map<AttributeType, int>(),
                matchDescription
            );

        /// <summary>
        /// Creates a command for simple attribute rewards (common case).
        /// </summary>
        public static ApplyMatchRewardsCommand CreateAttributeReward(
            AttributeType attributeType,
            int amount,
            string? matchDescription = null) =>
            Create(
                Map<ResourceType, int>(),
                Map((attributeType, amount)),
                matchDescription
            );
    }
}