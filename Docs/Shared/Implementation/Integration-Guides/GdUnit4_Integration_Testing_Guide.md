# GdUnit4 Integration Testing Guide

**Purpose**: Official guide for writing reliable integration tests using GdUnit4 in BlockLife  
**Status**: ✅ **OFFICIAL PATTERN** - Mandatory for all integration tests  
**Last Updated**: 2025-08-14  
**Based On**: Successful resolution of parallel service container architecture issues

## 🎯 Executive Summary

This guide establishes the official architecture pattern for GdUnit4 integration tests in BlockLife, based on lessons learned from debugging complex service container isolation issues. The `SimpleSceneTest` pattern is now the mandatory approach for all integration testing.

**Key Principle**: Integration tests MUST use the same service instances as production to ensure reliable results.

## 🚨 CRITICAL: The Parallel Service Container Problem

### What Went Wrong
The original integration test architecture created **parallel service containers** that were separate from the production SceneRoot autoload. This caused:

- **Commands** executed against test-created services  
- **Notifications** came from production SceneRoot services
- **Visual updates** used yet different service instances
- **State mismatches** between what tests expected vs. what actually existed

### Symptoms of the Problem
- ✅ Individual commands succeed
- ❌ Tests report unexpected state ("blocks found when none expected")
- ❌ "Block carryover between tests" - state isolation issues
- ❌ Phantom blocks that exist in one service but not another
- ❌ Notifications fire but don't correspond to test state

## ✅ SOLUTION: SimpleSceneTest Architecture

### The Mandatory Pattern
**ALL integration tests MUST use this exact pattern:**

```csharp
using Godot;
using GdUnit4;
using System;
using System.Threading.Tasks;
using BlockLife.Godot.Scenes;
using BlockLife.Core.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

[TestSuite]
public partial class YourIntegrationTest : Node  // ✅ MUST inherit from Node directly
{
    private Node? _testScene;
    private IServiceProvider? _serviceProvider;  // ✅ THE REAL service provider
    private SceneTree? _sceneTree;
    
    // Your controller references
    private GridInteractionController? _gridController;
    private BlockVisualizationController? _visualController;

    [Before]
    public async Task Setup()
    {
        // STEP 1: Get SceneTree from the test node itself
        _sceneTree = GetTree();
        _sceneTree.Should().NotBeNull("scene tree must be available");
        
        // STEP 2: Get SceneRoot autoload - THE REAL ONE, not a test copy
        var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot == null)
        {
            GD.PrintErr("SceneRoot not found - test must be run from Godot editor");
            return;
        }
        
        // STEP 3: Access the REAL service provider via reflection
        var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _serviceProvider = serviceProviderField?.GetValue(sceneRoot) as IServiceProvider;
        _serviceProvider.Should().NotBeNull("service provider must be initialized");
        
        // STEP 4: Create test scene as child of this node
        _testScene = await CreateTestScene();
        _testScene.Should().NotBeNull("test scene must be created");
        
        // STEP 5: Get controllers from the scene
        _gridController = _testScene!.GetNode<GridInteractionController>("GridView/GridInteractionController");
        _visualController = _testScene.GetNode<BlockVisualizationController>("GridView/BlockVisualizationController");
        
        _gridController.Should().NotBeNull("grid controller must exist in scene");
        _visualController.Should().NotBeNull("visualization controller must exist in scene");
    }

    [After]
    public async Task Cleanup()
    {
        // Clean up test scene
        if (_testScene != null && IsInstanceValid(_testScene))
        {
            _testScene.QueueFree();
            await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
        }
    }

    [TestCase]
    public async Task YourTest()
    {
        // CRITICAL: Graceful handling if not in Godot context
        if (_serviceProvider == null)
        {
            GD.Print("Skipping test - not in proper Godot context");
            return;
        }
        
        // Now you have the REAL services!
        var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
        var blockRepository = _serviceProvider.GetRequiredService<IBlockRepository>();
        
        // Your test logic here - uses same services as production
        // Commands will affect the same state that queries read from
    }

    /// <summary>
    /// Creates test scene with proper structure
    /// </summary>
    private async Task<Node> CreateTestScene()
    {
        // Create scene programmatically or load from .tscn file
        var root = new Node2D { Name = "TestRoot" };
        AddChild(root);
        
        // Create GridView structure exactly as in production
        var gridView = new GridView { Name = "GridView" };
        root.AddChild(gridView);
        
        var gridController = new GridInteractionController { Name = "GridInteractionController" };
        gridView.AddChild(gridController);
        
        var visualController = new BlockVisualizationController { Name = "BlockVisualizationController" };
        gridView.AddChild(visualController);
        
        var blockContainer = new Node2D { Name = "BlockContainer" };
        visualController.AddChild(blockContainer);
        
        // Let everything initialize properly
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        return root;
    }
}
```

## 🚫 FORBIDDEN PATTERNS

### ❌ Never Use Custom Test Base Classes
```csharp
// ❌ FORBIDDEN - Creates parallel service containers!
public partial class BadIntegrationTest : GodotIntegrationTestBase
{
    // This creates its own DI container separate from SceneRoot
    // Commands go to one container, notifications come from another
    // Results in impossible-to-debug state isolation issues
}
```

### ❌ Never Create Your Own Service Provider
```csharp
// ❌ FORBIDDEN - Parallel container creation
var services = new ServiceCollection();
// ... register services
var testServiceProvider = services.BuildServiceProvider();
// This creates a separate universe from production!
```

### ❌ Never Use Static Service Locators in Tests
```csharp
// ❌ FORBIDDEN - Bypasses dependency injection
var gridState = ServiceLocator.Get<IGridStateService>();
// This might get cached instances from wrong container
```

## ✅ RECOMMENDED PATTERNS

### Input Simulation
```csharp
private async Task SimulateClick(Vector2 position)
{
    var clickEvent = new InputEventMouseButton
    {
        Position = position,
        GlobalPosition = position,
        ButtonIndex = MouseButton.Left,
        Pressed = true
    };
    
    _gridController!.GetViewport().PushInput(clickEvent);
    
    // Also send release event
    var releaseEvent = new InputEventMouseButton
    {
        Position = position,
        GlobalPosition = position,
        ButtonIndex = MouseButton.Left,
        Pressed = false
    };
    
    _gridController.GetViewport().PushInput(releaseEvent);
    
    // Wait for processing
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
}
```

### Service Access
```csharp
[TestCase]
public async Task TestWithServices()
{
    if (_serviceProvider == null) return;
    
    // ✅ CORRECT - Get services from the REAL provider
    var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
    var commandBus = _serviceProvider.GetRequiredService<IMediator>();
    
    // These are the SAME instances production uses!
}
```

### Assertions
```csharp
// Use FluentAssertions for readable tests
var block = gridState.GetBlockAt(position);
block.ShouldBeSome("block should be placed");

var blockContainer = _visualController!.GetNode("BlockContainer");
blockContainer.GetChildCount().Should().Be(1, "exactly one visual block should exist");
```

## 🔧 Troubleshooting

### Common Issues and Solutions

#### "SceneRoot not found"
**Problem**: Test running outside proper Godot context  
**Solution**: Ensure test is run from Godot editor with SceneRoot autoload configured

#### "Service provider is null"
**Problem**: SceneRoot not properly initialized or reflection failed  
**Solution**: Check that SceneRoot has `_serviceProvider` field and it's properly initialized

#### "Tests pass individually but fail when run together"
**Problem**: State contamination between tests  
**Solution**: Ensure proper cleanup in `[After]` method and verify service isolation

#### "Commands succeed but visual state doesn't match"
**Problem**: Using wrong service provider (probably a test-created one)  
**Solution**: Verify you're using reflection to get the REAL SceneRoot service provider

#### "Blocks carry over between test methods"  
**Problem**: Classic parallel service container issue  
**Solution**: Follow this guide's pattern exactly - no custom test base classes

### Debug Validation Steps

1. **Verify Service Provider Source**:
   ```csharp
   GD.Print($"Service provider hash: {_serviceProvider?.GetHashCode()}");
   // Should be same across all tests
   ```

2. **Verify Service Instance Identity**:
   ```csharp
   var gridState1 = _serviceProvider.GetRequiredService<IGridStateService>();
   var gridState2 = _serviceProvider.GetRequiredService<IGridStateService>();
   GD.Print($"Same instance: {ReferenceEquals(gridState1, gridState2)}");
   // Should be true (singleton)
   ```

3. **Verify Scene Tree Structure**:
   ```csharp
   GD.Print($"SceneTree root: {_sceneTree?.Root}");
   GD.Print($"SceneRoot path: {_sceneTree?.Root.GetNodeOrNull("/root/SceneRoot")}");
   ```

## 📊 Test Metrics and Quality

### Test Quality Indicators
- ✅ All tests pass consistently when run individually  
- ✅ All tests pass consistently when run as a suite
- ✅ No "phantom state" issues between test methods
- ✅ Visual state matches service state in all scenarios  
- ✅ Console output shows expected test execution flow

### Performance Guidelines
- **Setup Time**: Should complete in < 100ms  
- **Test Execution**: Individual tests should run in < 500ms
- **Cleanup**: Should be nearly instantaneous
- **Memory**: No memory leaks between test runs

## 📚 Examples and References

### Working Examples
- **`SimpleSceneTest.cs`** - Gold standard implementation with 4 test cases
- **`BlockPlacementIntegrationTest.cs`** - Refactored using this pattern

### Related Documentation  
- **`Integration_Test_Architecture_Deep_Dive.md`** - Complete investigation and resolution
- **`CLAUDE.md`** - Project-wide integration test requirements
- **`Essential_Development_Workflow.md`** - TDD workflow including integration tests

## 🎯 Conclusion

The `SimpleSceneTest` pattern is now the official, battle-tested approach for GdUnit4 integration testing in BlockLife. By using the real SceneRoot service provider via reflection, we ensure that integration tests operate in exactly the same service context as production code.

**Remember**: The goal of integration tests is to verify that different components work together correctly. This requires them to use the same service instances that production code uses.

---

> **Maintenance Note**: This guide was created after resolving critical parallel service container issues. Any deviation from this pattern risks reintroducing the complex debugging scenarios we fought to resolve.