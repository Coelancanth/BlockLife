namespace BlockLife.Core.Domain.Block
{
    /// <summary>
    /// Defines the different types of blocks in the game.
    /// Each type represents a different life concept with unique properties and behaviors.
    /// </summary>
    public enum BlockType
    {
        /// <summary>
        /// Basic block type used for testing and initial placement.
        /// </summary>
        Basic = 0,

        /// <summary>
        /// Represents work, career, and professional development.
        /// Core building block for productivity and achievement.
        /// </summary>
        Work = 1,

        /// <summary>
        /// Represents education, learning, and skill development.
        /// Foundation for knowledge and personal growth.
        /// </summary>
        Study = 2,

        /// <summary>
        /// Represents relationships, social connections, and emotional bonds.
        /// Essential for social fulfillment and support networks.
        /// </summary>
        Relationship = 3,

        /// <summary>
        /// Represents physical wellness, exercise, and health maintenance.
        /// Critical for long-term wellbeing and vitality.
        /// </summary>
        Health = 4,

        /// <summary>
        /// Represents creative expression, hobbies, and artistic pursuits.
        /// Important for mental health and personal fulfillment.
        /// </summary>
        Creativity = 5,

        /// <summary>
        /// Represents relaxation, entertainment, and leisure activities.
        /// Necessary for stress relief and mental balance.
        /// </summary>
        Fun = 6,

        /// <summary>
        /// Special block type created from Work + Study combination.
        /// Represents career advancement and professional opportunities.
        /// </summary>
        CareerOpportunity = 7,

        /// <summary>
        /// Special block type created from Relationship + Health combination.
        /// Represents deep connections and supportive partnerships.
        /// </summary>
        Partnership = 8,

        /// <summary>
        /// Special block type created from Creativity + Fun combination.
        /// Represents passion projects and fulfilling pursuits.
        /// </summary>
        Passion = 9
    }

    /// <summary>
    /// Extension methods for BlockType to provide additional functionality.
    /// </summary>
    public static class BlockTypeExtensions
    {
        /// <summary>
        /// Checks if this block type is a basic/primary type (not created from combinations).
        /// </summary>
        public static bool IsPrimaryType(this BlockType blockType) => blockType switch
        {
            BlockType.Basic => true,
            BlockType.Work => true,
            BlockType.Study => true,
            BlockType.Relationship => true,
            BlockType.Health => true,
            BlockType.Creativity => true,
            BlockType.Fun => true,
            _ => false
        };

        /// <summary>
        /// Checks if this block type is a special/combination type.
        /// </summary>
        public static bool IsSpecialType(this BlockType blockType) => !blockType.IsPrimaryType();

        /// <summary>
        /// Gets the human-readable name for the block type.
        /// </summary>
        public static string GetDisplayName(this BlockType blockType) => blockType switch
        {
            BlockType.Basic => "Basic",
            BlockType.Work => "Work",
            BlockType.Study => "Study",
            BlockType.Relationship => "Relationship",
            BlockType.Health => "Health",
            BlockType.Creativity => "Creativity",
            BlockType.Fun => "Fun",
            BlockType.CareerOpportunity => "Career Opportunity",
            BlockType.Partnership => "Partnership",
            BlockType.Passion => "Passion",
            _ => blockType.ToString()
        };

        /// <summary>
        /// Gets the base value/score for this block type.
        /// </summary>
        public static int GetBaseValue(this BlockType blockType) => blockType switch
        {
            BlockType.Basic => 5,
            BlockType.Work => 10,
            BlockType.Study => 8,
            BlockType.Relationship => 12,
            BlockType.Health => 15,
            BlockType.Creativity => 9,
            BlockType.Fun => 6,
            BlockType.CareerOpportunity => 25,
            BlockType.Partnership => 30,
            BlockType.Passion => 20,
            _ => 1
        };

        /// <summary>
        /// Gets the color hex code for visual representation of this block type.
        /// Colors chosen for clarity and visual distinction.
        /// </summary>
        public static string GetColorHex(this BlockType blockType) => blockType switch
        {
            BlockType.Basic => "#808080",           // Gray
            BlockType.Work => "#4169E1",            // Royal Blue
            BlockType.Study => "#32CD32",           // Lime Green
            BlockType.Relationship => "#FF69B4",     // Hot Pink
            BlockType.Health => "#FF6347",          // Tomato Red
            BlockType.Creativity => "#9370DB",       // Medium Purple
            BlockType.Fun => "#FFD700",             // Gold
            BlockType.CareerOpportunity => "#00CED1", // Dark Turquoise
            BlockType.Partnership => "#FF1493",      // Deep Pink
            BlockType.Passion => "#FF8C00",         // Dark Orange
            _ => "#FFFFFF"                          // White (fallback)
        };

        /// <summary>
        /// Gets RGB color values for this block type (0-255 range).
        /// Useful for Godot Color construction.
        /// </summary>
        public static (byte r, byte g, byte b) GetColorRGB(this BlockType blockType) => blockType switch
        {
            BlockType.Basic => (128, 128, 128),           // Gray
            BlockType.Work => (65, 105, 225),             // Royal Blue
            BlockType.Study => (50, 205, 50),             // Lime Green
            BlockType.Relationship => (255, 105, 180),     // Hot Pink
            BlockType.Health => (255, 99, 71),            // Tomato Red
            BlockType.Creativity => (147, 112, 219),       // Medium Purple
            BlockType.Fun => (255, 215, 0),               // Gold
            BlockType.CareerOpportunity => (0, 206, 209),  // Dark Turquoise
            BlockType.Partnership => (255, 20, 147),       // Deep Pink
            BlockType.Passion => (255, 140, 0),           // Dark Orange
            _ => (255, 255, 255)                          // White (fallback)
        };
    }
}
