using BlockLife.Core.Domain.Player;
using LanguageExt;
using MediatR;

namespace BlockLife.Core.Features.Player.Notifications
{
    /// <summary>
    /// Notification published when player state changes due to match rewards or other actions.
    /// Used to trigger UI updates and maintain view synchronization.
    /// Following the established notification pattern from BlockLife architecture.
    /// </summary>
    public sealed record PlayerStateChangedNotification : INotification
    {
        /// <summary>
        /// The updated player state after changes were applied.
        /// </summary>
        public required PlayerState UpdatedPlayerState { get; init; }

        /// <summary>
        /// Resources that changed in this update (can be empty).
        /// Positive values indicate gains, negative values indicate spending.
        /// </summary>
        public required Map<ResourceType, int> ResourceChanges { get; init; }

        /// <summary>
        /// Attributes that changed in this update (can be empty).
        /// All values should be positive (attributes only increase).
        /// </summary>
        public required Map<AttributeType, int> AttributeChanges { get; init; }

        /// <summary>
        /// Optional description of what caused the changes.
        /// </summary>
        public string? ChangeDescription { get; init; }

        /// <summary>
        /// Creates a notification for player state changes.
        /// </summary>
        public static PlayerStateChangedNotification Create(
            PlayerState updatedPlayerState,
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges,
            string? changeDescription = null) =>
            new()
            {
                UpdatedPlayerState = updatedPlayerState,
                ResourceChanges = resourceChanges,
                AttributeChanges = attributeChanges,
                ChangeDescription = changeDescription
            };

        /// <summary>
        /// Creates a notification when only the player state changed (no specific changes tracked).
        /// </summary>
        public static PlayerStateChangedNotification CreateSimple(
            PlayerState updatedPlayerState,
            string? changeDescription = null) =>
            new()
            {
                UpdatedPlayerState = updatedPlayerState,
                ResourceChanges = Map<ResourceType, int>.Empty,
                AttributeChanges = Map<AttributeType, int>.Empty,
                ChangeDescription = changeDescription
            };
    }
}