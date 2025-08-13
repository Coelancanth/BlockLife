# Integration Testing Guide for BlockLife

## Executive Summary

This guide establishes the integration testing strategy for BlockLife, focusing on end-to-end validation of the complete MVP architecture, notification pipeline, and user interaction flows. Integration tests verify that all components work together correctly in the actual Godot runtime environment.

## Integration Testing Philosophy

### Purpose
Integration tests validate that:
- **Components integrate correctly**: Presenters, Views, Commands, and Notifications work together
- **User flows work end-to-end**: From UI interaction to visual feedback
- **Runtime behavior is correct**: Actual Godot scenes behave as expected
- **Performance is acceptable**: System handles realistic loads

### Key Principles
1. **Test Real Scenarios**: Use actual Godot scenes and components
2. **Verify Observable Behavior**: Focus on what users would see/experience
3. **Test Complete Flows**: From input to final state change
4. **Isolate External Dependencies**: But use real internal components

## Testing Infrastructure

### GdUnit4 Framework
BlockLife uses GdUnit4 for integration testing, which provides:
- Native Godot testing environment
- Scene loading and manipulation
- Input simulation capabilities
- Async operation support
- Performance measurement tools

### Test Organization
```
test/
├── integration/
│   ├── features/
│   │   ├── block_placement/
│   │   │   ├── BlockPlacementIntegrationTest.cs
│   │   │   └── PresenterViewIntegrationTest.cs
│   │   ├── block_movement/
│   │   └── rule_engine/
│   └── performance/
│       └── StressTests.cs
└── run_integration_tests.cmd
```

## Writing Integration Tests

### Test Structure
```csharp
[TestSuite]
public partial class FeatureIntegrationTest
{
    private SceneTree _sceneTree;
    private SceneRoot _sceneRoot;
    private Node _mainScene;
    
    [Before]
    public async Task Setup()
    {
        // 1. Get scene tree and SceneRoot
        _sceneTree = Engine.GetMainLoop() as SceneTree;
        _sceneRoot = _sceneTree.Root.GetNode<SceneRoot>("/root/SceneRoot");
        
        // 2. Load actual scene
        var scene = GD.Load<PackedScene>("res://path/to/scene.tscn");
        _mainScene = scene.Instantiate();
        _sceneTree.Root.AddChild(_mainScene);
        
        // 3. Wait for initialization
        await _sceneTree.ProcessFrame();
    }
    
    [After]
    public async Task Cleanup()
    {
        // Clean up scene
        _mainScene?.QueueFree();
        await _sceneTree.ProcessFrame();
    }
    
    [TestCase]
    public async Task TestScenario()
    {
        // Test implementation
    }
}
```

### Common Test Patterns

#### 1. User Interaction Testing
```csharp
[TestCase]
public async Task UserClick_TriggersExpectedBehavior()
{
    // Arrange
    var clickPosition = new Vector2(100, 100);
    var clickEvent = new InputEventMouseButton
    {
        Position = clickPosition,
        ButtonIndex = MouseButton.Left,
        Pressed = true
    };
    
    // Act
    var controller = _mainScene.GetNode<Control>("ControlNode");
    controller._GuiInput(clickEvent);
    await _sceneTree.ProcessFrame();
    
    // Assert
    // Verify expected state changes
}
```

#### 2. Notification Pipeline Testing
```csharp
[TestCase]
public async Task Notification_UpdatesAllSubscribers()
{
    // Arrange
    var notification = new TestNotification();
    var mediator = _serviceProvider.GetRequiredService<IMediator>();
    
    // Act
    await mediator.Publish(notification);
    await Task.Delay(100); // Allow async processing
    await _sceneTree.ProcessFrame();
    
    // Assert
    // Verify all views updated
}
```

#### 3. State Verification
```csharp
[TestCase]
public async Task CompleteFlow_UpdatesBothCoreAndVisualState()
{
    // Act - Perform action
    
    // Assert Core State
    var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
    var coreBlock = gridState.GetBlockAt(position);
    Assert.That(coreBlock.IsSome).IsTrue();
    
    // Assert Visual State
    var visualContainer = _view.GetNode("BlockContainer");
    Assert.That(visualContainer.GetChildCount()).IsEqual(1);
}
```

## Test Categories

### 1. Feature Integration Tests
Test complete feature workflows from user action to final result.

**Example**: Block Placement Flow
- User clicks grid → Command sent → State updated → Notification published → View updated

### 2. Presenter-View Communication Tests
Verify MVP pattern implementation and event handling.

**Example**: Event Subscription Lifecycle
- Presenter subscribes on init → Handles events → Unsubscribes on disposal

### 3. Performance Integration Tests
Test system behavior under load.

**Example**: Rapid Click Handling
- 100 rapid clicks → All processed → No dropped events → Acceptable frame rate

### 4. Error Scenario Tests
Verify graceful error handling across components.

**Example**: Invalid Input Handling
- Invalid position → Command fails → Error logged → User feedback shown

## Running Integration Tests

### Command Line
```bash
# Set Godot path (one time)
set GODOT_BIN=C:\Path\To\Godot.exe

# Run all integration tests
test\run_integration_tests.cmd

# Run specific test suite
addons\gdUnit4\runtest.cmd --path test/integration/features/block_placement
```

### In Godot Editor
1. Open Godot project
2. Go to Project → Tools → GdUnit4
3. Navigate to test folder
4. Run individual tests or suites

### Continuous Integration
```yaml
# GitHub Actions example
- name: Run Integration Tests
  run: |
    $env:GODOT_BIN = "${{ steps.godot.outputs.godot-executable }}"
    test\run_integration_tests.cmd
```

## Best Practices

### 1. Test Independence
- Each test should set up its own state
- Clean up after test completion
- Don't rely on test execution order

### 2. Async Operations
- Always await async operations
- Use ProcessFrame() to advance Godot's main loop
- Add small delays for complex async flows

### 3. Scene Management
- Load fresh scenes for each test
- Properly dispose of scenes in cleanup
- Use IsInstanceValid() to check node validity

### 4. Assertion Strategy
- Verify both core state and visual state
- Use specific assertions (IsEqual vs IsTrue)
- Include meaningful assertion messages

### 5. Performance Considerations
- Integration tests are slower than unit tests
- Group related assertions in single test
- Use [TestCase] parameters for similar scenarios

## Common Pitfalls and Solutions

### Pitfall 1: Flaky Tests
**Problem**: Tests pass/fail inconsistently
**Solution**: Add proper delays for async operations, ensure proper cleanup

### Pitfall 2: Test Pollution
**Problem**: Tests affect each other's state
**Solution**: Use Before/After methods, create fresh scenes per test

### Pitfall 3: Slow Test Suite
**Problem**: Tests take too long to run
**Solution**: Parallelize where possible, optimize setup/teardown

### Pitfall 4: Missing Notifications
**Problem**: Notifications not received by presenters
**Solution**: Verify presenter initialization, check event subscriptions

## Integration Test Checklist

Before committing integration tests:
- [ ] Test runs in isolation successfully
- [ ] Test cleans up all created resources
- [ ] Async operations properly awaited
- [ ] Both core and visual state verified
- [ ] Performance impact acceptable
- [ ] Clear test name and documentation
- [ ] No hardcoded paths or values
- [ ] Handles edge cases gracefully

## Debugging Integration Tests

### Enable Verbose Logging
```csharp
[Before]
public void EnableDebugLogging()
{
    var logger = _sceneRoot.Logger;
    logger.Information("Starting test: {TestName}", TestContext.CurrentTest.Name);
}
```

### Visual Debugging
```csharp
// Pause to see visual state
await Task.Delay(1000); // See what's happening
GD.Print($"Current state: {visualContainer.GetChildCount()} blocks");
```

### State Inspection
```csharp
// Dump state for debugging
var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
foreach (var block in gridState.GetAllBlocks())
{
    GD.Print($"Block at {block.Position}: {block.Type}");
}
```

## Metrics and Goals

### Target Metrics
- **Test Execution Time**: < 30 seconds for full suite
- **Test Coverage**: 80% of user-facing features
- **Reliability**: 100% pass rate in CI/CD
- **Maintainability**: < 50 lines per test method

### Success Indicators
- No production bugs in tested flows
- Fast feedback on breaking changes
- Confidence in refactoring
- Clear documentation of expected behavior

## Future Enhancements

### Planned Improvements
1. **Visual Regression Testing**: Screenshot comparison
2. **Performance Profiling**: Automated performance regression detection
3. **Mutation Testing**: Verify test effectiveness
4. **Contract Testing**: Validate presenter-view contracts
5. **Load Testing**: Simulate hundreds of concurrent users

### Research Areas
- Property-based integration testing
- Fuzzing user inputs
- Chaos engineering for resilience testing
- AI-assisted test generation

## Conclusion

Integration testing is crucial for validating BlockLife's architecture works correctly in practice. By following this guide, developers can create robust integration tests that ensure the game functions correctly from the user's perspective while maintaining the architectural integrity of the Clean Architecture and MVP patterns.

Remember: **Integration tests are living documentation of how the system should behave end-to-end.**

---

**Document Version**: 1.0  
**Last Updated**: 2025-08-14  
**Primary Audience**: BlockLife Development Team  
**Review Schedule**: After each major feature addition