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
    }
}