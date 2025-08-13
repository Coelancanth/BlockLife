# Implementation Plan: "Move Block" Feature

**Status: 🔄 IN PROGRESS** | **Last Updated: 2025-08-13** | **Follows: Comprehensive Development Workflow**

This document outlines the comprehensive, end-to-end implementation plan for the "Move Block" feature. As the first core gameplay mechanic, this vertical slice will serve as the reference implementation for the project's architecture, as defined in the **Project Architecture Guide (v3.1)**. It will establish the full CQRS flow, from user input to state mutation to visual feedback.

## ✅ Prerequisites Completed
- ✅ Core infrastructure (GameStrapper, DI, Logging)
- ✅ Grid system foundation (GridStateService, IGridStateService) 
- ✅ Related features (PlaceBlock, RemoveBlock commands/handlers)
- ✅ Base presenter/view architecture (GridPresenter, IGridView)
- ✅ Testing infrastructure (4-pillar strategy)
- ✅ Comprehensive Development Workflow established

## 📋 Implementation Status Overview
| Phase | Status | Items | Completed |
|-------|--------|-------|-----------|
| Phase 1: Core Logic | ✅ Completed | 5 | 5/5 |
| Phase 2: Presentation Contracts | ✅ Completed | 3 | 3/3 |
| Phase 3: Godot View Implementation | ❌ Not Started | 2 | 0/2 |
| Phase 4: Input Handling | ❌ Not Started | 2 | 0/2 |
| Phase 5: Closing the Loop | ❌ Not Started | 1 | 0/1 |
| Phase 6: Testing | ✅ Completed | 2 | 2/2 |
| **TOTAL** | **🔄 IN PROGRESS** | **15** | **10/15** |

## Phase 1: Core Logic & State (The Model)
**Status: ✅ Completed** | **TDD Required: Yes** | **Architecture Tests First: Yes**

**Goal:** Establish the pure, Godot-agnostic business logic for moving a block. All files in this phase will be created within the `src/BlockLife.Core.csproj` project.

**⚠️ WORKFLOW NOTE:** Must follow TDD+VSA approach from Comprehensive Development Workflow:
1. Write architecture fitness tests FIRST
2. Write failing unit tests (RED)
3. Implement minimal code (GREEN) 
4. Refactor while keeping tests green

### 1. ✅ **Feature Slice Scaffolding:**
- [x] Create directory: `src/Features/Block/Move/`

### 2. ✅ **Command Definition (TDD):**
- [ ] **RED**: Write architecture test ensuring Commands are immutable records
- [ ] **RED**: Write unit test for MoveBlockCommand validation
- [ ] **GREEN**: Create `MoveBlockCommand.cs` record implementing `ICommand<Fin<Unit>>`
- [ ] **VERIFY**: Contains `Guid BlockId`, `Vector2 ToPosition` with init setters
- [ ] **NOTE**: `FromPosition` omitted - IGridStateService is source of truth

### 3. ✅ **Effect & Notification Definition (TDD):**
- [ ] **RED**: Write tests for effect/notification immutability
- [ ] **GREEN**: Create `BlockMovedEffect.cs` in `src/Features/Block/Effects/`
- [ ] **GREEN**: Create `BlockMovedNotification.cs` in `src/Features/Block/Effects/`
- [ ] **VERIFY**: Both contain `BlockId` and `ToPosition`

### 4. ✅ **Handler Implementation (TDD + Functional):**
- [ ] **RED**: Write failing tests for all scenarios in `MoveBlockCommandHandlerTests.cs`
- [ ] **GREEN**: Create `MoveBlockCommandHandler.cs` with dependencies:
  - [ ] `IGridStateService` (for current state)
  - [ ] `ISimulationManager` (for effect queuing)
  - [ ] Validation rules (when implemented)
- [ ] **IMPLEMENT**: Functional pipeline with `Fin<T>` monad:
  1. Fetch current block state
  2. Validate block not locked 
  3. Validate target position valid/unoccupied
  4. Update state in `IGridStateService`
  5. Enqueue `BlockMovedEffect`
- [ ] **VERIFY**: Returns `Fin<Unit>` or structured `Error`

### 5. ✅ **Unit Testing Foundation Available:**
- ✅ Test directory structure exists: `tests/Features/Block/`
- [x] **CREATED**: `tests/Features/Block/Move/MoveBlockCommandHandlerTests.cs`
- [ ] **COVER**: Success scenario (state change + effect queuing)
- [ ] **COVER**: All failure scenarios with correct Error codes

## Phase 2: Presentation Layer Contracts
**Status: ✅ Completed**

**Goal:** Define the interfaces that decouple the Presenter from the concrete View implementation. These interfaces will also reside in `src/BlockLife.Core.csproj`.

1.  ✅ **View Sub-Interface (Capability):**
    *   In `src/Features/Block/Move/`, created `IBlockAnimationView.cs`.
    *   This interface defines the animation capability: `Task AnimateMoveAsync(Guid blockId, Vector2Int fromPosition, Vector2Int toPosition)`. The `Task` return type is crucial for the animation queuing system.

2.  ✅ **Composite View Interface:**
    *   Augmented existing `IGridView.cs` in `src/Features/Block/Presenters/`.
    *   This interface follows the **Composite View Pattern**. It exposes the animation capability through a property: `IBlockAnimationView? BlockAnimator { get; }`.

3.  ✅ **Presenter Update:**
    *   Updated `GridPresenter.cs` in `src/Features/Block/Presenters/`.
    *   Added `HandleBlockMovedAsync` method to handle block move notifications.
    *   Depends on the composite `IGridView` interface.
    *   Contains animation logic with fallback to instant update if animator not available.

## Phase 3: Godot View Implementation

**Goal:** Create the Godot nodes and scripts that realize the presentation contracts. All files in this phase are created within the `godot_project/BlockLife.Godot.csproj`.

1.  **Animation Controller Node:**
    *   Create a script `BlockAnimationController.cs` that implements `IBlockAnimationView`.
    *   This script will be attached to a `Node` in the main grid scene.
    *   The `AnimateMoveAsync` method will find the correct block `Node` by its ID (managed by the `GridView`), create a `Tween`, and `await` its completion signal.

2.  **Main Grid View:**
    *   Create/modify `GridView.cs`, which will be attached to the root node of the grid scene.
    *   It will implement the composite `IGridView` and `IPresenterContainer<GridPresenter>`.
    *   It will have an `[Export]` property for the `BlockAnimationController` node.
    *   The `IGridView.BlockAnimator` property will simply return this exported node.
    *   It will use the mandatory `PresenterLifecycleManager` pattern in `_Ready` and `_ExitTree` to manage its `GridPresenter`.
    *   It will be responsible for spawning/managing the visual block nodes.

## Phase 4: Input Handling & Command Dispatch

**Goal:** Capture user input and translate it into a `MoveBlockCommand`.

1.  **Input Logic in `GridPresenter`:**
    *   The `GridPresenter`'s `Initialize()` method will subscribe to input events from the `GridView` (or directly from Godot's input singleton).
    *   It will handle mouse-down, drag, and mouse-up events to determine the start and end positions of a move.
    *   It will inject and use the `ISystemStateService` to check if the system is locked (`Animating` or `LogicRunning`) *before* processing input, but it will **not** lock the system itself at this stage.

2.  **Command Dispatch:**
    *   Upon detecting a valid drag-and-drop action, the `GridPresenter` will populate and send a `MoveBlockCommand` using the injected `IMediator`.
    *   **Revised:** The system will **not** be locked immediately upon command dispatch. The locking will occur later, just before the animation begins, to ensure responsiveness for transient validation failures.

## Phase 5: Closing the Loop

**Goal:** Ensure the full, orchestrated sequence works as designed, incorporating the animation and system state plans.

1.  **Command Handling:** The `MoveBlockCommandHandler` executes, updates the model, and enqueues the `BlockMovedEffect`. This is a synchronous operation. If validation fails, the handler returns an `Error`, and the system remains unlocked.
2.  **Simulation Step:** The `SimulationManager` processes the effect and publishes the `BlockMovedNotification`.
3.  **Notification Handling:** The `GridPresenter` receives the notification.
4.  **Animation Queuing & Execution:**
    *   The notification is added to the Presenter's internal animation `Queue`.
    *   The `TryProcessQueue` method is called.
    *   **Revised:** It transitions the system state: `_systemStateService.Lock(SystemState.Animating)`. This lock is acquired *just before* the long-running asynchronous animation begins.
    *   It dequeues the notification and calls `View.BlockAnimator.AnimateMoveAsync(...)`.
    *   **Revised:** The `AnimateMoveAsync` call is `await`ed within a `try/finally` block to ensure the system is always unlocked.
5.  **System Unlock:**
    *   **Revised:** Once the animation `Task` completes (or if an exception occurs during animation), the `finally` block in the Presenter's `TryProcessQueue` method will execute.
    *   If the animation queue is now empty, the `_systemStateService.Unlock()` method is called directly by the `GridPresenter`. This adheres to the "Acquirer Releases" principle, ensuring the component that locks the system is also responsible for unlocking it.

## Phase 6: Testing
**Status: ✅ Completed**

**Goal:** Verify the correctness of the implementation through both isolated unit tests and full-flow integration tests.

1.  ✅ **Presenter Integration Tests (`tests/`):**
    *   Created `GridPresenterTests.cs` in `tests/BlockLife.Core.Tests/Features/Block/Presenters/`. These are **integration tests** for the Presenter, involving mocking of the `IGridView` and `IBlockAnimationView` to verify the Presenter's interaction logic.
    *   Tests that receiving a `BlockMovedNotification` correctly calls the `IBlockAnimationView` method.
    *   Tests fallback behavior when animator is not available.
    *   All tests passing (65 total tests in solution).
    *   **Clarification:** Due to the Presenter's interaction with Godot's input and view interfaces, these tests cannot be purely isolated unit tests but will require a test harness that simulates the Godot environment.

3.  **Integration Tests Extension:**
    *   Extend the existing integration test suite from F1.
    *   Add `MoveBlockIntegrationTest.cs` following established GdUnit4 patterns.
    *   The test will build on F1 patterns:
        1.  Use existing grid scene instantiation patterns.
        2.  Place a block (using F1 functionality), then move it.
        3.  Assert animation completion and correct final position.
        4.  Verify the move completed successfully using F1-established verification patterns.

---

## 🚀 NEXT WORK ITEMS (Priority Order)

Based on the current state analysis, here are the immediate next steps following the **Comprehensive Development Workflow**:

### 🎯 **IMMEDIATE NEXT STEPS** (Start Here)

#### 1. **Architecture Foundation (Week 1)**
```bash
# STEP 1: Run architecture tests to establish baseline
dotnet test --filter "FullyQualifiedName~Architecture"

# STEP 2: Create Move feature slice directory
mkdir src/Features/Block/Move
```

#### 2. **TDD Cycle 1: Command Definition** 
- **RED**: Write architecture test for MoveBlockCommand immutability
- **RED**: Write unit tests for command validation
- **GREEN**: Implement minimal MoveBlockCommand record
- **REFACTOR**: Optimize while keeping tests green

#### 3. **TDD Cycle 2: Effect/Notification**
- **RED**: Write tests for BlockMovedEffect/Notification
- **GREEN**: Implement both classes
- **VERIFY**: Architecture tests pass

### 📅 **SPRINT BREAKDOWN**

**Sprint 1 (Days 1-3): Core Command Infrastructure**
- [ ] Architecture fitness tests for Move feature
- [ ] MoveBlockCommand with TDD
- [ ] BlockMovedEffect/Notification with TDD
- [ ] Unit test foundation

**Sprint 2 (Days 4-6): Handler Logic**
- [ ] MoveBlockCommandHandler with full TDD cycle
- [ ] Integration with IGridStateService
- [ ] Validation rule integration
- [ ] Property-based tests for movement invariants

**Sprint 3 (Days 7-9): Presentation Layer**
- [ ] IBlockAnimationView interface
- [ ] GridPresenter notification handling
- [ ] Animation queuing logic

**Sprint 4 (Days 10-12): Godot Integration**
- [ ] BlockAnimationController implementation
- [ ] GridView updates for animation
- [ ] Input handling integration

**Sprint 5 (Days 13-15): Full Integration**
- [ ] End-to-end testing
- [ ] GdUnit4 integration tests
- [ ] Performance validation
- [ ] Documentation updates

### ⚠️ **BLOCKERS TO RESOLVE**
- **Decision needed**: Animation system approach (Tween vs custom)
- **Decision needed**: Input handling pattern (presenter vs view)
- **Verify**: ISimulationManager interface completeness

### 📊 **SUCCESS CRITERIA**
- [ ] All 55+ tests pass (maintaining 1,030 validations)
- [ ] Architecture fitness tests validate Move feature compliance
- [ ] Property tests cover movement mathematical invariants  
- [ ] Integration tests verify end-to-end Move functionality
- [ ] No Godot dependencies in Core project
- [ ] Full TDD documentation trail maintained

### 🔄 **WORKFLOW COMPLIANCE CHECKLIST**
- [ ] Follow [Comprehensive_Development_Workflow.md](../6_Guides/Comprehensive_Development_Workflow.md) exactly
- [ ] Use `Quick_Reference_Development_Checklist.md` for daily validation
- [ ] Run architecture tests before any implementation
- [ ] Maintain TDD Red-Green-Refactor discipline
- [ ] Update TodoWrite tool for progress tracking
- [ ] Validate against 4-pillar testing strategy throughout