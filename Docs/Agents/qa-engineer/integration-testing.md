# Integration Testing Guide

*This content was delegated from CLAUDE.md to the QA Engineer agent.*

## 🚨 CRITICAL: GdUnit4 Integration Test Pattern
**WARNING**: GdUnit4 integration tests are extremely sensitive to architectural patterns!

## ✅ MANDATORY PATTERN: SimpleSceneTest Architecture

**ALL integration tests MUST follow this exact pattern - no exceptions:**

```csharp
[TestSuite]
public partial class YourIntegrationTest : Node  // MUST inherit from Node directly
{
    private Node? _testScene;
    private IServiceProvider? _serviceProvider;
    private SceneTree? _sceneTree;

    [Before]
    public async Task Setup()
    {
        // 1. Get SceneTree from test node itself
        _sceneTree = GetTree();
        _sceneTree.Should().NotBeNull("scene tree must be available");
        
        // 2. Get SceneRoot autoload - THE REAL ONE, not a test copy
        var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot == null)
        {
            GD.PrintErr("SceneRoot not found - test must be run from Godot editor");
            return;
        }
        
        // 3. Access the REAL service provider via reflection
        var field = typeof(SceneRoot).GetField("_serviceProvider",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _serviceProvider = field?.GetValue(sceneRoot) as IServiceProvider;
        
        // 4. Create test scene as child of this node
        _testScene = await CreateTestScene();
        
        // 5. Access your controllers normally
        _gridController = _testScene!.GetNode<GridInteractionController>("GridView/GridInteractionController");
    }
    
    [TestCase]
    public async Task YourTest()
    {
        if (_serviceProvider == null) 
        {
            GD.Print("Skipping test - not in proper Godot context");
            return;
        }
        // Your test logic using the REAL service provider
        var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
        // Tests now use the same services as production!
    }
}
```

## 🚫 FORBIDDEN PATTERN: Custom Test Base Classes

**NEVER use this pattern - it creates parallel service containers:**

```csharp
// ❌ FORBIDDEN - Creates parallel service containers!
public partial class BadIntegrationTest : GodotIntegrationTestBase
{
    // This pattern creates its own DI container separate from SceneRoot
    // Commands go to one container, notifications from another
    // Results in impossible-to-debug failures with "phantom blocks"
}
```

## 🔑 Critical Architecture Principle

**Single Service Container Rule**: Integration tests MUST use the exact same service instances as production SceneRoot. Any test infrastructure creating parallel containers will cause:
- Commands succeed but don't affect visible state
- Notifications fire but don't match actual state  
- "Blocks carryover between tests" symptoms
- Complete state isolation leading to phantom failures

## 📚 Reference Files

**Working Examples**:
- ✅ `SimpleSceneTest.cs` - Gold standard implementation
- ✅ `BlockPlacementIntegrationTest.cs` - Refactored to use correct pattern

**Investigation Documentation**:  
- 📚 `Docs/Shared/Post-Mortems/Integration_Test_Architecture_Deep_Dive.md` - Complete debugging journey and architectural lessons

For complete testing strategy, see [Docs/Agent-References/qa-references.md](../../Agent-References/qa-references.md).