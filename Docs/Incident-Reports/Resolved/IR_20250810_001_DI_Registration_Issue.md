# Bug Post-Mortem: DI Container Presenter Registration Error

## Bug ID: BPM-003
**Date**: 2025-08-13  
**Severity**: Critical (Application fails to start)  
**Component**: Dependency Injection / Presenter Architecture  
**Fixed In**: feature/move_block_vertical_slice

## Summary
The application failed to start due to a DI container validation error when `BlockManagementPresenter` implemented `INotificationHandler` interfaces but required `IBlockManagementView` in its constructor, which wasn't registered in the DI container.

## Timeline
- **Discovery**: Application crashed on startup with DI validation error
- **Root Cause Identified**: Presenter implementing MediatR notification handler interfaces
- **Fix Applied**: Removed notification handler interfaces from presenter
- **Resolution**: Created separate empty notification handlers for MediatR

## Error Message
```
FATAL: Failed to initialize DI container. Error: System.AggregateException: Some services are not able to be constructed 
Error while validating the service descriptor 'ServiceType: MediatR.INotificationHandler`1[BlockPlacedNotification]': 
Unable to resolve service for type 'IBlockManagementView' while attempting to activate 'BlockManagementPresenter'
```

## Root Cause Analysis

### The Problem
1. **MediatR Auto-Registration**: When a class implements `INotificationHandler<T>`, MediatR automatically tries to register it as a service during assembly scanning
2. **View Dependency**: `BlockManagementPresenter` required `IBlockManagementView` in its constructor
3. **Godot-Managed Views**: Views are created by Godot's scene system, not the DI container
4. **Validation Failure**: DI container validation (`ValidateOnBuild = true`) caught the missing dependency

### Architecture Conflict
The issue revealed a fundamental architectural conflict:
- **Presenters** need view dependencies (created by Godot)
- **MediatR handlers** need to be DI-constructible (all dependencies in container)
- **Result**: Presenters cannot be notification handlers directly

## The Fix

### Solution Approach
1. **Removed INotificationHandler interfaces** from `BlockManagementPresenter`
2. **Created EmptyBlockPlacementHandlers** to satisfy MediatR's requirement for at least one handler
3. **Maintained presenter functionality** through direct command handling

### Code Changes

#### Before (Incorrect)
```csharp
public class BlockManagementPresenter : PresenterBase<IBlockManagementView>,
    INotificationHandler<BlockPlacedNotification>,  // ❌ Causes DI registration
    INotificationHandler<BlockRemovedNotification>   // ❌ Causes DI registration
{
    public BlockManagementPresenter(
        IBlockManagementView view,  // ❌ Not in DI container
        IMediator mediator,
        ILogger<BlockManagementPresenter> logger) : base(view)
    { }
    
    public async Task Handle(BlockPlacedNotification notification, CancellationToken cancellationToken)
    { }
}
```

#### After (Correct)
```csharp
// Presenter without notification handler interfaces
public class BlockManagementPresenter : PresenterBase<IBlockManagementView>
{
    public BlockManagementPresenter(
        IBlockManagementView view,  // ✅ Provided by PresenterFactory
        IMediator mediator,
        ILogger<BlockManagementPresenter> logger) : base(view)
    { }
    
    // Regular methods, not INotificationHandler implementations
    public async Task HandleBlockPlaced(BlockPlacedNotification notification)
    { }
}

// Separate handler for MediatR
public class EmptyBlockPlacementHandlers : 
    INotificationHandler<BlockPlacedNotification>,
    INotificationHandler<BlockRemovedNotification>
{
    // ✅ All dependencies are in DI container
    public EmptyBlockPlacementHandlers(ILogger<EmptyBlockPlacementHandlers> logger)
    { }
}
```

## Lessons Learned

### 1. Presenter Pattern Constraints
- **Rule**: Presenters MUST NOT implement `INotificationHandler` or other auto-registered interfaces
- **Reason**: Presenters require view dependencies not available in DI container
- **Solution**: Use separate infrastructure handlers or event aggregation patterns

### 2. MediatR Integration
- **Discovery**: MediatR automatically registers all `INotificationHandler` implementations found during assembly scanning
- **Implication**: Any class implementing these interfaces must be fully DI-constructible
- **Best Practice**: Keep notification handlers separate from view-dependent components

### 3. DI Container Validation
- **Value**: `ValidateOnBuild = true` caught the issue immediately
- **Benefit**: Prevented runtime failures in production
- **Recommendation**: Always enable validation in development

## Architectural Guidelines Updated

### New Rules for Presenters
1. ✅ **DO** create presenters via `PresenterFactory`
2. ✅ **DO** inject view dependencies through constructor
3. ❌ **DON'T** implement `INotificationHandler` or `IRequestHandler` interfaces
4. ❌ **DON'T** register presenters directly in DI container

### Notification Handling Patterns
For presenter notification handling, use one of these patterns:

#### Pattern 1: Empty Handlers (Current Solution)
- Create minimal handlers that satisfy MediatR
- Presenters handle updates through command results

#### Pattern 2: Event Aggregator (Future Enhancement)
- Implement a separate event aggregator for presenter notifications
- Presenters subscribe/unsubscribe during their lifecycle

#### Pattern 3: Notification Service (Alternative)
- Create a notification service that presenters can subscribe to
- Service receives MediatR notifications and forwards to subscribers

## Testing Improvements

### New Architecture Test Added
```csharp
[Fact]
public void Presenters_Should_Not_Implement_NotificationHandler_Interfaces()
{
    var presenterTypes = Types.InAssembly(CoreAssembly)
        .That().Inherit(typeof(PresenterBase<>))
        .GetTypes();

    var violatingPresenters = presenterTypes
        .Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && 
                      i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)))
        .ToList();

    violatingPresenters.Should().BeEmpty(
        "Presenters must not implement INotificationHandler as they require " +
        "view dependencies not available in the DI container");
}
```

## Impact on Existing Code

### Files Modified
1. `src/Features/Block/Placement/BlockManagementPresenter.cs` - Removed notification handler interfaces
2. `src/Features/Block/Placement/Effects/EmptyBlockPlacementHandlers.cs` - Created empty handlers
3. `src/Core/GameStrapper.cs` - Registered empty handlers

### Backward Compatibility
- No breaking changes to public APIs
- Existing functionality preserved
- Future presenters must follow new pattern

## Prevention Measures

### 1. Documentation Updates
- Updated CLAUDE.md with presenter constraints
- Added to architecture guide
- Included in development workflow

### 2. Code Review Checklist
- [ ] Presenters don't implement auto-registered interfaces
- [ ] All notification handlers are DI-constructible
- [ ] Views are created via PresenterFactory pattern

### 3. Automated Checks
- Architecture test prevents future violations
- DI validation catches registration issues
- Build pipeline includes validation step

## Related Issues
- Links to: Architecture Guide, MVP Pattern Documentation
- Affects: All future presenter implementations
- Similar to: BPM-001 (SceneRoot initialization order)

## Conclusion
This bug revealed an important architectural constraint that wasn't initially documented. The fix maintains clean separation between Godot-managed views and DI-managed services while preserving all functionality. The solution follows Clean Architecture principles by keeping infrastructure concerns separate from presentation logic.