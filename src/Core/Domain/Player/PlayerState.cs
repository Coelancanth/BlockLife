using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BlockLife.Core.Domain.Player
{
    /// <summary>
    /// Immutable aggregate representing the complete state of a player.
    /// Tracks resources (finite, spendable) and attributes (permanent improvements).
    /// Following established domain patterns with LanguageExt functional types.
    /// </summary>
    public sealed record PlayerState
    {
        /// <summary>
        /// Unique identifier for this player.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Display name for the player.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Current resource balances (Money, Social Capital, etc.).
        /// Resources can be earned and spent.
        /// </summary>
        public required Map<ResourceType, int> Resources { get; init; }

        /// <summary>
        /// Current attribute levels (Knowledge, Health, etc.).
        /// Attributes represent permanent character improvements.
        /// </summary>
        public required Map<AttributeType, int> Attributes { get; init; }

        /// <summary>
        /// Timestamp when this player state was created.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Timestamp when this player state was last modified.
        /// </summary>
        public required DateTime LastModifiedAt { get; init; }

        /// <summary>
        /// Version number for optimistic concurrency control.
        /// Incremented on each state change.
        /// </summary>
        public required int Version { get; init; }

        /// <summary>
        /// Maximum tier the player has unlocked for merge patterns.
        /// Tier 1 = basic match-3 only, Tier 2+ = merge patterns enabled.
        /// Part of VS_003B-3: Unlock Purchase System.
        /// </summary>
        public required int MaxUnlockedTier { get; init; } = 2;  // Enable merge by default for testing

        /// <summary>
        /// Default constructor required for record initialization with required properties.
        /// </summary>
        public PlayerState()
        {
        }

        /// <summary>
        /// JsonConstructor for Newtonsoft.Json deserialization on Linux.
        /// Ensures cross-platform compatibility with required properties.
        /// </summary>
        [JsonConstructor]
        public PlayerState(
            Guid id,
            string name,
            Map<ResourceType, int> resources,
            Map<AttributeType, int> attributes,
            DateTime createdAt,
            DateTime lastModifiedAt,
            int version,
            int maxUnlockedTier = 2)  // Enable merge by default for testing
        {
            Id = id;
            Name = name;
            Resources = resources;
            Attributes = attributes;
            CreatedAt = createdAt;
            LastModifiedAt = lastModifiedAt;
            Version = version;
            MaxUnlockedTier = maxUnlockedTier;
        }

        /// <summary>
        /// Creates a new player with initial state.
        /// All resources and attributes start at zero.
        /// </summary>
        /// <param name="name">Display name for the player</param>
        /// <returns>New player state with initialized values</returns>
        public static PlayerState CreateNew(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be null or empty", nameof(name));

            var now = DateTime.UtcNow;
            return new PlayerState
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Resources = InitializeResources(),
                Attributes = InitializeAttributes(),
                CreatedAt = now,
                LastModifiedAt = now,
                Version = 1,
                MaxUnlockedTier = 2  // Enable merge by default for testing
            };
        }

        /// <summary>
        /// Gets the current amount of a specific resource.
        /// </summary>
        /// <param name="resourceType">The resource type to query</param>
        /// <returns>Current amount, or 0 if not found</returns>
        public int GetResource(ResourceType resourceType) =>
            Resources.Find(resourceType).IfNone(0);

        /// <summary>
        /// Gets the current level of a specific attribute.
        /// </summary>
        /// <param name="attributeType">The attribute type to query</param>
        /// <returns>Current level, or 0 if not found</returns>
        public int GetAttribute(AttributeType attributeType) =>
            Attributes.Find(attributeType).IfNone(0);

        /// <summary>
        /// Adds resources to the player's current balance.
        /// Returns a new state with updated values and incremented version.
        /// </summary>
        /// <param name="resourceType">Type of resource to add</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>New player state with added resources</returns>
        public Option<PlayerState> AddResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0)
                return Option<PlayerState>.None;

            var currentAmount = GetResource(resourceType);
            var newAmount = currentAmount + amount;

            // Prevent integer overflow
            if (newAmount < currentAmount)
                return Option<PlayerState>.None;

            return Option<PlayerState>.Some(this with
            {
                Resources = Resources.AddOrUpdate(resourceType, newAmount),
                LastModifiedAt = DateTime.UtcNow,
                Version = Version + 1
            });
        }

        /// <summary>
        /// Spends resources from the player's current balance.
        /// Returns None if insufficient resources available.
        /// </summary>
        /// <param name="resourceType">Type of resource to spend</param>
        /// <param name="amount">Amount to spend (must be positive)</param>
        /// <returns>New player state with spent resources, or None if insufficient</returns>
        public Option<PlayerState> SpendResource(ResourceType resourceType, int amount)
        {
            if (amount <= 0)
                return Option<PlayerState>.None;

            var currentAmount = GetResource(resourceType);
            if (currentAmount < amount)
                return Option<PlayerState>.None;

            return Option<PlayerState>.Some(this with
            {
                Resources = Resources.AddOrUpdate(resourceType, currentAmount - amount),
                LastModifiedAt = DateTime.UtcNow,
                Version = Version + 1
            });
        }

        /// <summary>
        /// Adds to an attribute level.
        /// Attributes can only increase, never decrease.
        /// </summary>
        /// <param name="attributeType">Type of attribute to increase</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>New player state with increased attribute</returns>
        public Option<PlayerState> AddAttribute(AttributeType attributeType, int amount)
        {
            if (amount <= 0)
                return Option<PlayerState>.None;

            var currentLevel = GetAttribute(attributeType);
            var newLevel = currentLevel + amount;

            // Prevent integer overflow
            if (newLevel < currentLevel)
                return Option<PlayerState>.None;

            return Option<PlayerState>.Some(this with
            {
                Attributes = Attributes.AddOrUpdate(attributeType, newLevel),
                LastModifiedAt = DateTime.UtcNow,
                Version = Version + 1
            });
        }

        /// <summary>
        /// Applies multiple resource and attribute changes atomically.
        /// If any change fails validation, the entire operation fails.
        /// </summary>
        /// <param name="resourceChanges">Resources to add/spend (negative for spending)</param>
        /// <param name="attributeChanges">Attributes to increase</param>
        /// <returns>New player state with all changes applied, or None if any change fails</returns>
        public Option<PlayerState> ApplyChanges(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges)
        {
            // Validate all resource changes first
            foreach (var (resourceType, change) in resourceChanges)
            {
                if (change < 0) // Spending
                {
                    var required = Math.Abs(change);
                    if (GetResource(resourceType) < required)
                        return Option<PlayerState>.None;
                }
            }

            // Validate all attribute changes (must be positive)
            if (attributeChanges.Values.Any(change => change <= 0))
                return Option<PlayerState>.None;

            // Apply all changes
            var newResources = resourceChanges.Fold(Resources, (current, kvp) =>
                current.AddOrUpdate(kvp.Key, GetResource(kvp.Key) + kvp.Value));

            var newAttributes = attributeChanges.Fold(Attributes, (current, kvp) =>
                current.AddOrUpdate(kvp.Key, GetAttribute(kvp.Key) + kvp.Value));

            return Option<PlayerState>.Some(this with
            {
                Resources = newResources,
                Attributes = newAttributes,
                LastModifiedAt = DateTime.UtcNow,
                Version = Version + 1
            });
        }

        /// <summary>
        /// Checks if the player has sufficient resources for a purchase.
        /// </summary>
        /// <param name="costs">Required resources and amounts</param>
        /// <returns>True if all costs can be afforded</returns>
        public bool CanAfford(Map<ResourceType, int> costs) =>
            costs.ForAll((KeyValuePair<ResourceType, int> kvp) => GetResource(kvp.Key) >= kvp.Value);

        /// <summary>
        /// Gets the total "wealth" score combining all resources and attributes.
        /// Useful for achievements and progression tracking.
        /// </summary>
        public int GetTotalScore() =>
            Resources.Values.Sum() + Attributes.Values.Sum();

        /// <summary>
        /// Checks if the merge-to-next-tier unlock is available for the player.
        /// Part of VS_003B-3: Uses actual unlock data from purchases.
        /// </summary>
        /// <returns>True if player has unlocked merge patterns (MaxUnlockedTier >= 2), false for match-only mode</returns>
        public bool IsMergeUnlocked()
        {
            // Player has merge unlocked if they've purchased tier 2 or higher
            return MaxUnlockedTier >= 2;
        }

        /// <summary>
        /// Creates a copy with updated last modified timestamp (touch operation).
        /// </summary>
        public PlayerState Touch() => this with
        {
            LastModifiedAt = DateTime.UtcNow,
            Version = Version + 1
        };

        /// <summary>
        /// Initializes all resource types to zero.
        /// </summary>
        private static Map<ResourceType, int> InitializeResources() =>
            Map<ResourceType, int>(
                (ResourceType.Money, 0),
                (ResourceType.SocialCapital, 0)
            );

        /// <summary>
        /// Initializes all attribute types to zero.
        /// </summary>
        private static Map<AttributeType, int> InitializeAttributes() =>
            Map<AttributeType, int>(
                (AttributeType.Knowledge, 0),
                (AttributeType.Health, 0),
                (AttributeType.Happiness, 0),
                (AttributeType.Energy, 0),
                (AttributeType.Nutrition, 0),
                (AttributeType.Fitness, 0),
                (AttributeType.Mindfulness, 0),
                (AttributeType.Creativity, 0)
            );

        public override string ToString() =>
            $"{Name} (Score: {GetTotalScore()}, Version: {Version})";
    }
}
