using BlockLife.Core.Domain.Player;
using LanguageExt;
using LanguageExt.Common;
using System;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for managing player state operations.
    /// Provides functions for querying and mutating player resources and attributes.
    /// All state changes are immutable and validated.
    /// </summary>
    public interface IPlayerStateService
    {
        /// <summary>
        /// Creates a new player with initial state.
        /// </summary>
        /// <param name="name">Display name for the player</param>
        /// <returns>Success with new player state, or failure with error details</returns>
        Fin<PlayerState> CreatePlayer(string name);

        /// <summary>
        /// Gets the current player state.
        /// Assumes single-player game with one active player.
        /// </summary>
        /// <returns>Current player state, or None if no player exists</returns>
        Option<PlayerState> GetCurrentPlayer();

        /// <summary>
        /// Gets a player by their unique identifier.
        /// </summary>
        /// <param name="playerId">The player's unique ID</param>
        /// <returns>Player state, or None if not found</returns>
        Option<PlayerState> GetPlayer(Guid playerId);

        /// <summary>
        /// Updates the player state with a new version.
        /// Includes optimistic concurrency control via version checking.
        /// </summary>
        /// <param name="playerState">Updated player state</param>
        /// <returns>Success, or failure if version conflict or validation fails</returns>
        Fin<Unit> UpdatePlayer(PlayerState playerState);

        /// <summary>
        /// Adds resources to the current player's balance.
        /// </summary>
        /// <param name="resourceType">Type of resource to add</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>Updated player state, or failure if validation fails</returns>
        Fin<PlayerState> AddResource(ResourceType resourceType, int amount);

        /// <summary>
        /// Spends resources from the current player's balance.
        /// </summary>
        /// <param name="resourceType">Type of resource to spend</param>
        /// <param name="amount">Amount to spend (must be positive)</param>
        /// <returns>Updated player state, or failure if insufficient resources</returns>
        Fin<PlayerState> SpendResource(ResourceType resourceType, int amount);

        /// <summary>
        /// Increases an attribute level for the current player.
        /// Attributes can only increase, never decrease.
        /// </summary>
        /// <param name="attributeType">Type of attribute to increase</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>Updated player state, or failure if validation fails</returns>
        Fin<PlayerState> AddAttribute(AttributeType attributeType, int amount);

        /// <summary>
        /// Applies multiple resource and attribute changes atomically.
        /// If any change fails validation, the entire operation fails.
        /// </summary>
        /// <param name="resourceChanges">Resources to add/spend (negative for spending)</param>
        /// <param name="attributeChanges">Attributes to increase</param>
        /// <returns>Updated player state, or failure if any change fails</returns>
        Fin<PlayerState> ApplyRewards(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges);

        /// <summary>
        /// Gets the current amount of a specific resource.
        /// </summary>
        /// <param name="resourceType">The resource type to query</param>
        /// <returns>Current amount, or 0 if player doesn't exist</returns>
        int GetResourceAmount(ResourceType resourceType);

        /// <summary>
        /// Gets the current level of a specific attribute.
        /// </summary>
        /// <param name="attributeType">The attribute type to query</param>
        /// <returns>Current level, or 0 if player doesn't exist</returns>
        int GetAttributeLevel(AttributeType attributeType);

        /// <summary>
        /// Checks if the current player can afford a set of resource costs.
        /// </summary>
        /// <param name="costs">Required resources and amounts</param>
        /// <returns>True if all costs can be afforded, false otherwise</returns>
        bool CanAfford(Map<ResourceType, int> costs);

        /// <summary>
        /// Gets the current player's total score (sum of all resources and attributes).
        /// Useful for achievements and progression tracking.
        /// </summary>
        /// <returns>Total score, or 0 if no player exists</returns>
        int GetTotalScore();

        /// <summary>
        /// Resets the current player to initial state (development/testing).
        /// </summary>
        /// <returns>Reset player state, or failure if no player exists</returns>
        Fin<PlayerState> ResetPlayer();

        /// <summary>
        /// Persists the current player state to storage.
        /// </summary>
        /// <returns>Success, or failure with error details</returns>
        Fin<Unit> SavePlayerState();

        /// <summary>
        /// Loads player state from storage.
        /// </summary>
        /// <returns>Success, or failure with error details</returns>
        Fin<Unit> LoadPlayerState();
    }
}
