using BlockLife.Core.Application.Commands;
using LanguageExt;

namespace BlockLife.Core.Features.Turn.Commands
{
    /// <summary>
    /// Command to advance the game to the next turn.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public sealed record AdvanceTurnCommand : ICommand
    {
        /// <summary>
        /// Creates a new AdvanceTurnCommand.
        /// </summary>
        public static AdvanceTurnCommand Create() =>
            new();
    }
}