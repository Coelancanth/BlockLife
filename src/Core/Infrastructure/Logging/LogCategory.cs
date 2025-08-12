namespace BlockLife.Core.Infrastructure.Logging;

/// <summary>
/// Provides well-defined, constant strings for log source contexts.
/// This prevents typos and allows for easy discovery of available categories.
/// </summary>
public static class LogCategory
{
    public const string Core = "Core";
    public const string DI = "DependencyInjection";
    public const string Input = "Input";
    public const string GameState = "GameState";
    public const string AI = "AI";
    public const string Physics = "Physics";
    public const string Rendering = "Rendering";
}
