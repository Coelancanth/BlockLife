namespace BlockLife.Core.Domain.Player
{
    /// <summary>
    /// Defines the different types of resources in the game.
    /// Resources are finite quantities that can be earned and spent.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Money earned from work activities.
        /// Primary currency for purchasing upgrades and items.
        /// </summary>
        Money = 1,

        /// <summary>
        /// Social capital earned from relationship activities.
        /// Used for unlocking social features and collaborations.
        /// </summary>
        SocialCapital = 2
    }

    /// <summary>
    /// Extension methods for ResourceType to provide additional functionality.
    /// </summary>
    public static class ResourceTypeExtensions
    {
        /// <summary>
        /// Gets the human-readable name for the resource type.
        /// </summary>
        public static string GetDisplayName(this ResourceType resourceType) => resourceType switch
        {
            ResourceType.Money => "Money",
            ResourceType.SocialCapital => "Social Capital",
            _ => resourceType.ToString()
        };

        /// <summary>
        /// Gets the symbol for display in UI.
        /// </summary>
        public static string GetSymbol(this ResourceType resourceType) => resourceType switch
        {
            ResourceType.Money => "$",
            ResourceType.SocialCapital => "♥",
            _ => "?"
        };

        /// <summary>
        /// Gets the color hex code for visual representation.
        /// </summary>
        public static string GetColorHex(this ResourceType resourceType) => resourceType switch
        {
            ResourceType.Money => "#32CD32", // Lime Green
            ResourceType.SocialCapital => "#FF69B4", // Hot Pink
            _ => "#808080" // Gray
        };
    }
}
