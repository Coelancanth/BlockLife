using BlockLife.Core.Domain.Player;
using LanguageExt;
using System.Threading.Tasks;

namespace BlockLife.Core.Features.Player.Views
{
    /// <summary>
    /// Interface for the attribute display view that shows player resources and attributes.
    /// This interface exposes capabilities that the presenter needs to update the attribute display.
    /// Following the established MVP pattern from BlockLife architecture.
    /// </summary>
    public interface IAttributeView
    {
        /// <summary>
        /// Updates the display with the current player state.
        /// Shows all resources and attributes with their current values.
        /// </summary>
        /// <param name="playerState">Current player state with resources and attributes</param>
        Task UpdateAttributeDisplayAsync(PlayerState playerState);

        /// <summary>
        /// Shows feedback when attributes change due to match rewards.
        /// </summary>
        /// <param name="resourceChanges">Resources that changed (can be empty)</param>
        /// <param name="attributeChanges">Attributes that changed (can be empty)</param>
        /// <param name="matchDescription">Optional description of what caused the changes</param>
        Task ShowAttributeChangesAsync(
            Map<ResourceType, int> resourceChanges,
            Map<AttributeType, int> attributeChanges,
            string? matchDescription = null);

        /// <summary>
        /// Displays an error if player state could not be retrieved.
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        Task ShowErrorAsync(string errorMessage);

        /// <summary>
        /// Clears the attribute display when no player exists.
        /// </summary>
        Task ClearDisplayAsync();
    }
}