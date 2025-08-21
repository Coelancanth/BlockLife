using Godot;

namespace BlockLife.test.integration
{
    /// <summary>
    /// DEPRECATED: This base class is deprecated and should not be used.
    /// 
    /// PROBLEM: This class created parallel service containers that conflicted
    /// with the real SceneRoot autoload, causing state isolation issues and
    /// test failures due to commands going to different service instances
    /// than notifications.
    /// 
    /// The main issues were:
    /// 1. InitializeSceneRoot() created a NEW SceneRoot with NEW services
    /// 2. Commands went to the test-created service instance
    /// 3. Notifications came from the real SceneRoot autoload instance
    /// 4. Visual updates used yet another service instance
    /// 5. State accumulated in wrong places, causing test carryover
    /// 
    /// SOLUTION: Use the SimpleSceneTest pattern instead:
    /// 1. Inherit directly from Node
    /// 2. Access SceneRoot via GetTree().Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot")
    /// 3. Use reflection to access private _serviceProvider field
    /// 4. Keep test setup simple and direct
    /// 5. Use the SAME service instances as production code
    /// 
    /// REFERENCE FILES:
    /// - SimpleSceneTest.cs - Working reference implementation
    /// - Integration_Test_Architecture_Deep_Dive.md - Full investigation
    /// 
    /// All integration tests have been refactored to use the SimpleSceneTest pattern.
    /// </summary>
    // NOTE: Cannot use Obsolete attribute because Godot source generator references this class
    // WARNING: DO NOT USE THIS CLASS - See comments below
    public partial class GodotIntegrationTestBase : Node
    {
        // This class is intentionally left empty and marked obsolete with error=true
        // to prevent its use in new tests and force compilation errors if used.
        // 
        // The Obsolete attribute with error=true will cause a compilation error
        // if anyone tries to use this class, ensuring they use the correct pattern.
        // 
        // All integration tests should follow the pattern in SimpleSceneTest.cs
        // which properly uses the real SceneRoot autoload and its services.
    }
}
