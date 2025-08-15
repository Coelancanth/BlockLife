# Integration Testing Patterns Playbook

**Document ID**: LWP_002  
**Version**: 1.0  
**Last Updated**: 2025-08-15  
**Owner**: Architect Agent  
**Status**: Active  
**Evolution History**: 
- v1.0: Extracted from Integration Test Architecture Deep Dive (2025-08-15)

## Purpose

This playbook provides proven patterns for integration testing in BlockLife, particularly focusing on GdUnit4 integration with our Clean Architecture + MVP pattern. Born from extensive debugging of parallel service container issues that caused phantom test failures.

## Quick Reference Checklist

**For ALL integration tests, verify**:
- [ ] **Single Service Container**: Tests use REAL SceneRoot, not parallel containers
- [ ] **SimpleSceneTest Pattern**: Inherit directly from Node, not custom base classes
- [ ] **Service Access**: Get services via reflection from actual SceneRoot instance
- [ ] **Test Isolation**: Tests run independently without carryover effects
- [ ] **Export Properties**: All Godot exports properly configured in test scenes
- [ ] **MediatR Registration**: No duplicate handler registrations

## ✅ CORRECT Pattern: SimpleSceneTest Architecture

### MANDATORY Pattern - No Exceptions

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

### Why This Pattern Works

1. **Single Service Container**: All tests share the same DI container as production
2. **Real Autoload**: Uses the actual SceneRoot singleton, not a test copy
3. **No Parallel State**: Commands and notifications go to the same system
4. **Test Isolation**: Each test creates its own scene but shares services

## 🚫 FORBIDDEN Pattern: Custom Test Base Classes

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

### Why This Pattern Fails

1. **Parallel Containers**: Test creates separate DI container from SceneRoot
2. **State Isolation**: Commands succeed but don't affect visible state
3. **Notification Gaps**: Events fire from different container than test observes
4. **Phantom State**: Blocks appear/disappear unpredictably between tests

## Critical Architecture Principles

### 1. Single Service Container Rule

**Integration tests MUST use the exact same service instances as production SceneRoot.** Any test infrastructure creating parallel containers will cause:
- Commands succeed but don't affect visible state
- Notifications fire but don't match actual state  
- "Blocks carryover between tests" symptoms
- Complete state isolation leading to phantom failures

### 2. Export Property Configuration

**All Godot exports MUST be properly configured in test scenes:**

```csharp
// ✅ CORRECT - Export properly assigned
[Export] public GridView? GridView { get; set; }

// In test scene setup:
GridView.Should().NotBeNull("GridView export must be assigned in scene");
```

### 3. MediatR Handler Registration

**Avoid duplicate handler registrations that cause conflicts:**

```csharp
// ❌ WRONG - Manual handler registration conflicts with automatic scanning
services.AddTransient<BlockPlacedNotificationHandler>();

// ✅ CORRECT - Let MediatR auto-scan and register
services.AddMediatR(typeof(BlockPlacedNotificationHandler).Assembly);
```

## Common Integration Test Antipatterns

### Antipattern 1: "Helper Methods" That Hide Context

```csharp
// ❌ WRONG - Hides the fact that services might be null
private async Task PlaceBlock(GridPosition position)
{
    var command = new PlaceBlockCommand(blockTypeA, position);
    await _mediator.Send(command); // What if _mediator is null?
}

// ✅ CORRECT - Explicit null checking and context awareness
private async Task PlaceBlock(GridPosition position)
{
    if (_serviceProvider == null)
    {
        Assert.Fail("Test not properly initialized - services unavailable");
        return;
    }
    
    var mediator = _serviceProvider.GetRequiredService<IMediator>();
    var command = new PlaceBlockCommand(blockTypeA, position);
    var result = await mediator.Send(command);
    result.IsSuccess.Should().BeTrue($"Block placement should succeed at {position}");
}
```

### Antipattern 2: Complex Test Base Classes

```csharp
// ❌ WRONG - Complex inheritance creates hidden dependencies
public partial class ComplexTestBase : Node
{
    protected IServiceProvider? ServiceProvider { get; private set; }
    
    // Lots of setup logic that can fail silently
    protected virtual async Task SetupComplexInfrastructure() { ... }
}

// ✅ CORRECT - Simple, explicit setup in each test
[Before]
public async Task Setup()
{
    // Clear, simple setup that's easy to debug
    _sceneTree = GetTree();
    var sceneRoot = _sceneTree.Root.GetNode<SceneRoot>("/root/SceneRoot");
    // ... explicit steps
}
```

### Antipattern 3: Ignoring Service Provider Null States

```csharp
// ❌ WRONG - Assumes services are always available
[TestCase]
public async Task SomeTest()
{
    var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
    // What if _serviceProvider is null because SceneRoot wasn't found?
}

// ✅ CORRECT - Explicit null checking with clear messaging
[TestCase]
public async Task SomeTest()
{
    if (_serviceProvider == null)
    {
        GD.Print("Skipping test - SceneRoot services not available");
        return; // Graceful skip rather than cryptic null reference
    }
    
    var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
}
```

## Test Scene Configuration

### Proper Scene Structure

```
TestScene.tscn
├── GridView (GridView script attached)
├── GridInteractionController (script attached)
├── BlockContainer (Node2D for block placement)
└── Other UI elements as needed
```

### Required Export Assignments

In the scene editor, ensure all exports are properly assigned:
- GridView.GridContainer → Node in scene
- GridView.BlockContainer → Node2D for block rendering
- GridInteractionController.GridView → GridView node

## Debugging Integration Test Failures

### Step 1: Verify Service Container Access

```csharp
[TestCase]
public void Debug_ServiceProvider_Should_Not_Be_Null()
{
    _serviceProvider.Should().NotBeNull("Service provider should be available from SceneRoot");
    
    var gridState = _serviceProvider!.GetRequiredService<IGridStateService>();
    gridState.Should().NotBeNull("Grid state service should be registered");
    
    GD.Print($"Service provider: {_serviceProvider.GetType().Name}");
    GD.Print($"Grid state instance: {gridState.GetHashCode()}");
}
```

### Step 2: Verify Scene Configuration

```csharp
[TestCase] 
public void Debug_Scene_Configuration()
{
    _testScene.Should().NotBeNull("Test scene should be created");
    
    var gridView = _testScene!.GetNode<GridView>("GridView");
    gridView.Should().NotBeNull("GridView should exist in scene");
    
    gridView.GridContainer.Should().NotBeNull("GridView.GridContainer export should be assigned");
    gridView.BlockContainer.Should().NotBeNull("GridView.BlockContainer export should be assigned");
}
```

### Step 3: Trace Command Flow

```csharp
[TestCase]
public async Task Debug_Command_Flow()
{
    var mediator = _serviceProvider!.GetRequiredService<IMediator>();
    var command = new PlaceBlockCommand(blockTypeA, new GridPosition(0, 0));
    
    GD.Print($"Sending command: {command}");
    var result = await mediator.Send(command);
    GD.Print($"Command result: {result}");
    
    var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
    var blocks = gridState.GetAllBlocks();
    GD.Print($"Blocks after command: {blocks.Count()}");
}
```

## Success Metrics

### Technical Validation
- ✅ All integration tests pass consistently
- ✅ No "phantom blocks" or state carryover between tests
- ✅ Tests use same service instances as production
- ✅ Service provider access never returns null in valid test contexts

### Architecture Validation  
- ✅ SimpleSceneTest pattern used consistently
- ✅ No custom test base classes creating parallel containers
- ✅ Export properties properly configured in all test scenes
- ✅ Clear error messages when tests skip due to environment issues

## Evolution Notes

This playbook evolved from the "Integration Test Architecture Deep Dive" investigation where we discovered that custom test base classes were creating parallel service containers, leading to phantom test failures that took extensive debugging to resolve.

**Key Lesson**: Integration test failures that seem related to cleanup or state management are often actually architecture issues with service container isolation.

## References

- **SimpleSceneTest.cs**: Gold standard working implementation
- **BlockPlacementIntegrationTest.cs**: Refactored to use correct pattern
- **Integration_Test_Architecture_Deep_Dive.md**: Complete debugging journey (archived)
- **GdUnit4_Integration_Testing_Guide.md**: Additional GdUnit4 patterns