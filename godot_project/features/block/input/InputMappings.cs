using Godot;

namespace BlockLife.godot_project.features.block.input;

/// <summary>
/// Centralized configuration for all input mappings.
/// Extracted from various handlers to provide single source of truth.
/// </summary>
[GlobalClass]
public partial class InputMappings : Resource
{
    // Block operations
    [Export] public Key PlaceBlockKey { get; set; } = Key.Space;
    [Export] public Key InspectBlockKey { get; set; } = Key.I;
    [Export] public Key CycleBlockTypeKey { get; set; } = Key.Tab;

    // Movement operations
    [Export] public MouseButton SelectButton { get; set; } = MouseButton.Left;
    [Export] public MouseButton CancelButton { get; set; } = MouseButton.Right;

    // Testing operations
    [Export] public Key UnlockMergeKey { get; set; } = Key.M;
    
    // Swap operations (for future use)
    [Export] public Key SwapBlockKey { get; set; } = Key.S;
    [Export] public float SwapMaxDistance { get; set; } = 3.0f;

    // Camera controls (for future use)
    [Export] public Key CameraUpKey { get; set; } = Key.W;
    [Export] public Key CameraDownKey { get; set; } = Key.S;
    [Export] public Key CameraLeftKey { get; set; } = Key.A;
    [Export] public Key CameraRightKey { get; set; } = Key.D;

    // Modifier keys
    [Export] public Key QuickPlaceModifier { get; set; } = Key.Shift;
    [Export] public Key MultiSelectModifier { get; set; } = Key.Ctrl;

    // Debug keys
    [Export] public Key DebugToggleKey { get; set; } = Key.F3;
    [Export] public Key ReloadSceneKey { get; set; } = Key.F5;

    /// <summary>
    /// Creates default input mappings.
    /// </summary>
    public static InputMappings CreateDefault()
    {
        return new InputMappings();
    }

    /// <summary>
    /// Validates that all required keys are set.
    /// </summary>
    public bool IsValid()
    {
        // All keys have defaults, so always valid for now
        // Can add validation logic if needed
        return true;
    }

    /// <summary>
    /// Gets a human-readable description of the key mappings.
    /// </summary>
    public string GetMappingDescription()
    {
        return $@"
Input Mappings:
  Place Block: {PlaceBlockKey}
  Inspect Block: {InspectBlockKey}
  Cycle Block Type: {CycleBlockTypeKey}
  Select/Move: {SelectButton}
  Cancel: {CancelButton}
  Unlock Merge (Testing): {UnlockMergeKey}
  Swap Blocks: {SwapBlockKey} (max distance: {SwapMaxDistance})
";
    }
}
