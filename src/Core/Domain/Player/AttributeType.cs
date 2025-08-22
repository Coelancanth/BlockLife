namespace BlockLife.Core.Domain.Player
{
    /// <summary>
    /// Defines the different types of attributes in the game.
    /// Attributes are permanent character improvements that grow over time.
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// Knowledge gained from study activities.
        /// Represents accumulated learning and expertise.
        /// </summary>
        Knowledge = 1,

        /// <summary>
        /// Health gained from health-focused activities.
        /// Represents physical wellbeing and vitality.
        /// </summary>
        Health = 2,

        /// <summary>
        /// Happiness gained from fun activities.
        /// Represents emotional wellbeing and life satisfaction.
        /// </summary>
        Happiness = 3,

        /// <summary>
        /// Energy gained from sleep activities.
        /// Represents stamina and capacity for action.
        /// </summary>
        Energy = 4,

        /// <summary>
        /// Nutrition gained from food activities.
        /// Represents nutritional health and dietary balance.
        /// </summary>
        Nutrition = 5,

        /// <summary>
        /// Fitness gained from exercise activities.
        /// Represents physical strength and conditioning.
        /// </summary>
        Fitness = 6,

        /// <summary>
        /// Mindfulness gained from meditation activities.
        /// Represents mental clarity and emotional regulation.
        /// </summary>
        Mindfulness = 7,

        /// <summary>
        /// Creativity gained from creativity activities.
        /// Represents artistic ability and innovative thinking.
        /// </summary>
        Creativity = 8
    }

    /// <summary>
    /// Extension methods for AttributeType to provide additional functionality.
    /// </summary>
    public static class AttributeTypeExtensions
    {
        /// <summary>
        /// Gets the human-readable name for the attribute type.
        /// </summary>
        public static string GetDisplayName(this AttributeType attributeType) => attributeType switch
        {
            AttributeType.Knowledge => "Knowledge",
            AttributeType.Health => "Health",
            AttributeType.Happiness => "Happiness",
            AttributeType.Energy => "Energy",
            AttributeType.Nutrition => "Nutrition",
            AttributeType.Fitness => "Fitness",
            AttributeType.Mindfulness => "Mindfulness",
            AttributeType.Creativity => "Creativity",
            _ => attributeType.ToString()
        };

        /// <summary>
        /// Gets the icon symbol for display in UI.
        /// </summary>
        public static string GetIcon(this AttributeType attributeType) => attributeType switch
        {
            AttributeType.Knowledge => "📚", // Book
            AttributeType.Health => "❤️", // Heart
            AttributeType.Happiness => "😊", // Smile
            AttributeType.Energy => "⚡", // Lightning
            AttributeType.Nutrition => "🍎", // Apple
            AttributeType.Fitness => "💪", // Muscle
            AttributeType.Mindfulness => "🧘", // Meditation
            AttributeType.Creativity => "🎨", // Palette
            _ => "❓" // Question
        };

        /// <summary>
        /// Gets the color hex code for visual representation.
        /// </summary>
        public static string GetColorHex(this AttributeType attributeType) => attributeType switch
        {
            AttributeType.Knowledge => "#32CD32", // Lime Green
            AttributeType.Health => "#FF6347", // Tomato Red
            AttributeType.Happiness => "#FFD700", // Gold
            AttributeType.Energy => "#FFFF00", // Yellow
            AttributeType.Nutrition => "#FF8C00", // Dark Orange
            AttributeType.Fitness => "#FF4500", // Orange Red
            AttributeType.Mindfulness => "#9370DB", // Medium Purple
            AttributeType.Creativity => "#FF69B4", // Hot Pink
            _ => "#808080" // Gray
        };
    }
}
