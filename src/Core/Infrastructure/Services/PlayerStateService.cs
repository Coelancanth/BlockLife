using BlockLife.Core.Domain.Player;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using System;
using System.Threading;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Infrastructure.Services
{
    /// <summary>
    /// In-memory implementation of IPlayerStateService.
    /// Manages single-player state with thread-safe operations.
    /// Suitable for game development and testing scenarios.
    /// </summary>
    public class PlayerStateService : IPlayerStateService
    {
        private readonly object _lock = new object();
        private PlayerState? _currentPlayer;

        /// <summary>
        /// Creates a new player with initial state.
        /// Replaces any existing player (single-player mode).
        /// </summary>
        /// <param name="name">Display name for the player</param>
        /// <returns>Success with new player state, or failure with error details</returns>
        public Fin<PlayerState> CreatePlayer(string name)
        {
            try
            {
                var newPlayer = PlayerState.CreateNew(name);
                
                lock (_lock)
                {
                    _currentPlayer = newPlayer;
                }
                
                return FinSucc(newPlayer);
            }
            catch (ArgumentException ex)
            {
                return FinFail<PlayerState>(Error.New(ex));
            }
            catch (Exception ex)
            {
                return FinFail<PlayerState>(Error.New(ex));
            }
        }

        /// <summary>
        /// Gets the current player state.
        /// Thread-safe access to player data.
        /// </summary>
        /// <returns>Current player state, or None if no player exists</returns>
        public Option<PlayerState> GetCurrentPlayer()
        {
            lock (_lock)
            {
                return _currentPlayer ?? Option<PlayerState>.None;
            }
        }

        /// <summary>
        /// Gets a player by their unique identifier.
        /// In single-player mode, only returns current player if ID matches.
        /// </summary>
        /// <param name="playerId">The player's unique ID</param>
        /// <returns>Player state, or None if not found</returns>
        public Option<PlayerState> GetPlayer(Guid playerId)
        {
            lock (_lock)
            {
                return _currentPlayer?.Id == playerId 
                    ? Option<PlayerState>.Some(_currentPlayer) 
                    : Option<PlayerState>.None;
            }
        }

        /// <summary>
        /// Updates the player state with a new version.
        /// Includes optimistic concurrency control via version checking.
        /// </summary>
        /// <param name="playerState">Updated player state</param>
        /// <returns>Success, or failure if version conflict or validation fails</returns>
        public Fin<Unit> UpdatePlayer(PlayerState playerState)
        {
            if (playerState == null)
                return FinFail<Unit>(Error.New("Player state cannot be null"));

            lock (_lock)
            {
                if (_currentPlayer == null)
                    return FinFail<Unit>(Error.New("No current player to update"));

                if (_currentPlayer.Id != playerState.Id)
                    return FinFail<Unit>(Error.New("Player ID mismatch"));

                if (_currentPlayer.Version >= playerState.Version)
                    return FinFail<Unit>(Error.New("Optimistic concurrency conflict: stale version"));

                _currentPlayer = playerState;
                return FinSucc(unit);
            }
        }

        /// <summary>
        /// Adds resources to the current player's balance.
        /// </summary>
        /// <param name="resourceType">Type of resource to add</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>Updated player state, or failure if validation fails</returns>
        public Fin<PlayerState> AddResource(ResourceType resourceType, int amount)
        {
            return GetCurrentPlayer().ToFin(Error.New("No current player"))
                .Bind(player => player.AddResource(resourceType, amount)
                    .ToFin(Error.New($"Failed to add {amount} {resourceType}")))
                .Bind(UpdatePlayerAndReturn);
        }

        /// <summary>
        /// Spends resources from the current player's balance.
        /// </summary>
        /// <param name="resourceType">Type of resource to spend</param>
        /// <param name="amount">Amount to spend (must be positive)</param>
        /// <returns>Updated player state, or failure if insufficient resources</returns>
        public Fin<PlayerState> SpendResource(ResourceType resourceType, int amount)
        {
            return GetCurrentPlayer().ToFin(Error.New("No current player"))
                .Bind(player => player.SpendResource(resourceType, amount)
                    .ToFin(Error.New($"Insufficient {resourceType} (need {amount}, have {player.GetResource(resourceType)})")))
                .Bind(UpdatePlayerAndReturn);
        }

        /// <summary>
        /// Increases an attribute level for the current player.
        /// </summary>
        /// <param name="attributeType">Type of attribute to increase</param>
        /// <param name="amount">Amount to add (must be positive)</param>
        /// <returns>Updated player state, or failure if validation fails</returns>
        public Fin<PlayerState> AddAttribute(AttributeType attributeType, int amount)
        {
            return GetCurrentPlayer().ToFin(Error.New("No current player"))
                .Bind(player => player.AddAttribute(attributeType, amount)
                    .ToFin(Error.New($"Failed to add {amount} {attributeType}")))
                .Bind(UpdatePlayerAndReturn);
        }

        /// <summary>
        /// Applies multiple resource and attribute changes atomically.
        /// </summary>
        /// <param name="resourceChanges">Resources to add/spend (negative for spending)</param>
        /// <param name="attributeChanges">Attributes to increase</param>
        /// <returns>Updated player state, or failure if any change fails</returns>
        public Fin<PlayerState> ApplyRewards(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges)
        {
            return GetCurrentPlayer().ToFin(Error.New("No current player"))
                .Bind(player => player.ApplyChanges(resourceChanges, attributeChanges)
                    .ToFin(Error.New("Failed to apply reward changes")))
                .Bind(UpdatePlayerAndReturn);
        }

        /// <summary>
        /// Gets the current amount of a specific resource.
        /// </summary>
        /// <param name="resourceType">The resource type to query</param>
        /// <returns>Current amount, or 0 if player doesn't exist</returns>
        public int GetResourceAmount(ResourceType resourceType) =>
            GetCurrentPlayer().Map(player => player.GetResource(resourceType)).IfNone(0);

        /// <summary>
        /// Gets the current level of a specific attribute.
        /// </summary>
        /// <param name="attributeType">The attribute type to query</param>
        /// <returns>Current level, or 0 if player doesn't exist</returns>
        public int GetAttributeLevel(AttributeType attributeType) =>
            GetCurrentPlayer().Map(player => player.GetAttribute(attributeType)).IfNone(0);

        /// <summary>
        /// Checks if the current player can afford a set of resource costs.
        /// </summary>
        /// <param name="costs">Required resources and amounts</param>
        /// <returns>True if all costs can be afforded, false otherwise</returns>
        public bool CanAfford(Map<ResourceType, int> costs) =>
            GetCurrentPlayer().Map(player => player.CanAfford(costs)).IfNone(false);

        /// <summary>
        /// Gets the current player's total score.
        /// </summary>
        /// <returns>Total score, or 0 if no player exists</returns>
        public int GetTotalScore() =>
            GetCurrentPlayer().Map(player => player.GetTotalScore()).IfNone(0);

        /// <summary>
        /// Resets the current player to initial state.
        /// </summary>
        /// <returns>Reset player state, or failure if no player exists</returns>
        public Fin<PlayerState> ResetPlayer()
        {
            return GetCurrentPlayer().ToFin(Error.New("No current player to reset"))
                .Map(player => {
                    var resetPlayer = PlayerState.CreateNew(player.Name);
                    lock (_lock)
                    {
                        _currentPlayer = resetPlayer;
                    }
                    return resetPlayer;
                });
        }

        /// <summary>
        /// Persists the current player state to storage.
        /// In this implementation, state is already in memory, so this is a no-op.
        /// </summary>
        /// <returns>Success (always succeeds for in-memory storage)</returns>
        public Fin<Unit> SavePlayerState() => FinSucc(unit);

        /// <summary>
        /// Loads player state from storage.
        /// In this implementation, state is already in memory, so this is a no-op.
        /// </summary>
        /// <returns>Success (always succeeds for in-memory storage)</returns>
        public Fin<Unit> LoadPlayerState() => FinSucc(unit);

        /// <summary>
        /// Helper method to update player state and return the new state.
        /// Ensures atomic update with consistent locking.
        /// </summary>
        private Fin<PlayerState> UpdatePlayerAndReturn(PlayerState newState)
        {
            return UpdatePlayer(newState).Map(_ => newState);
        }
    }
}
