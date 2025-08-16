# Move Block Implementation Plan ✅ COMPLETED
*Created by Tech Lead - Reference Implementation*

**Status**: ✅ **COMPLETED & PRODUCTION-READY** - 18 passing tests  
**Location**: `src/Features/Block/Move/`  
**Pattern**: Copy of Block Placement with movement-specific logic  

## 🏗️ Technical Implementation Plan

### Technical Approach
- **Pattern**: Command/Handler with CQRS (copy from Block Placement)
- **State**: GridStateService for block state management
- **UI**: MVP with GridPresenter handling movement feedback
- **Movement Logic**: Source position lookup + destination validation
- **Testing**: Full TDD coverage with movement-specific edge cases

### Phase 1: Domain Logic ✅ COMPLETED (Est: 4-6 hours)
- [x] Write failing test for MoveBlockCommand
- [x] Create MoveBlockCommand with BlockId + ToPosition (no FromPosition needed)
- [x] Write failing test for MoveBlockCommandHandler
- [x] Implement handler with block existence + destination validation
- [x] Add Fin<Unit> error handling for movement failures
- [x] Register in DI container

### Phase 2: Infrastructure ✅ COMPLETED (Est: 2-4 hours)  
- [x] Extend GridStateService with MoveBlock operation
- [x] Create movement validation rules (BlockExists, DestinationValid)
- [x] Implement BlockMovedNotification for UI updates
- [x] Test atomic move operations (remove from source, add to destination)

### Phase 3: Presentation ✅ COMPLETED (Est: 6-8 hours)
- [x] Define IBlockAnimationView interface for movement feedback
- [x] Create BlockMovementNotificationBridge for presenter communication
- [x] Extend GridPresenter to handle move notifications
- [x] Wire up movement commands from UI interactions

### Phase 4: Testing & Polish ✅ COMPLETED (Est: 2-4 hours)
- [x] Integration tests for complete move operations
- [x] Edge cases (invalid source, blocked destination, out-of-bounds)
- [x] Performance validation for move operations
- [x] Architecture compliance tests

### Technical Decisions Made
- **BlockId Lookup**: Handler finds source position via GridStateService
- **Atomic Operations**: Move = Remove + Place in single transaction
- **Validation Chain**: Block exists → Destination empty → Position valid
- **Error Handling**: Specific error messages for each failure type
- **Animation Ready**: Interface prepared for future animation integration

### Key Implementation Details
```csharp
// Move Command (simplified from placement)
public sealed record MoveBlockCommand : ICommand
{
    public required Guid BlockId { get; init; }
    public required Vector2Int ToPosition { get; init; }
    // FromPosition omitted - GridStateService is source of truth
}

// Handler Logic (extends placement patterns)
public async Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
{
    return await FindBlockPosition(request.BlockId)
        .BindAsync(fromPos => ValidateDestination(request.ToPosition))
        .BindAsync(_ => ExecuteMove(request.BlockId, request.ToPosition))
        .BindAsync(_ => PublishMoveNotification(request));
}
```

### Folder Structure Implemented
```
src/Features/Block/Move/
├── (Commands in ../Commands/)
│   ├── MoveBlockCommand.cs           # Move command definition
│   └── MoveBlockCommandHandler.cs    # Move business logic
├── Effects/
│   └── BlockMovementNotificationBridge.cs  # Presenter bridge
└── IBlockAnimationView.cs           # Animation interface (future)

tests/BlockLife.Core.Tests/Features/Block/Move/
├── MoveBlockCommandHandlerTests.cs  # 18 passing tests
└── Effects/
    └── BlockMovementNotificationBridgeTests.cs
```

### Movement Validation Rules
- ✅ **Block Exists**: Source block must exist in grid
- ✅ **Destination Empty**: Target position must be available  
- ✅ **Position Valid**: Target must be within grid boundaries
- ✅ **Different Position**: Cannot move to current position
- ✅ **Atomic Operation**: Move succeeds completely or fails completely

### Performance Characteristics
- **Move Operations**: O(1) for position-based block lookup and updates
- **Validation**: O(1) for all movement validation checks
- **Memory**: Efficient in-place updates without additional allocations
- **Concurrency**: Thread-safe via GridStateService locking

### Future Extensions Ready
- **Animation System**: IBlockAnimationView interface prepared
- **Drag & Drop**: Command pattern supports UI input mapping
- **Multi-Block**: Pattern extensible to group operations
- **Undo/Redo**: Commands are immutable and replay-able

## Reference Implementation Usage

**This demonstrates the "copy and adapt" pattern:**
1. **Started with Block Placement** as foundation
2. **Modified command properties** (added BlockId, removed explicit position)
3. **Adapted business logic** for movement vs. creation
4. **Extended validation rules** for movement-specific constraints
5. **Reused infrastructure** (GridStateService, notification patterns)

**Key learning: 70% code reuse from Block Placement pattern**

**Total Implementation Time**: 14-20 hours (completed 2025-08-13)

---

*This implementation demonstrates successful pattern replication and adaptation from the foundational Block Placement reference.*