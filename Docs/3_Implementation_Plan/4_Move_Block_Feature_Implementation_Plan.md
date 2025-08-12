# Implementation Plan: "Move Block" Feature

This document outlines the comprehensive, end-to-end implementation plan for the "Move Block" feature. As the first core gameplay mechanic, this vertical slice will serve as the reference implementation for the project's architecture, as defined in the **Project Architecture Guide (v3.1)**. It will establish the full CQRS flow, from user input to state mutation to visual feedback.

## Phase 1: Core Logic & State (The Model)

**Goal:** Establish the pure, Godot-agnostic business logic for moving a block. All files in this phase will be created within the `src/BlockLife.Core.csproj` project.

1.  **Feature Slice Scaffolding:**
    *   Create a new feature directory: `src/Features/Block/Move/`.

2.  **Command Definition:**
    *   In the new directory, create `MoveBlockCommand.cs`.
    *   This command will be a `record` implementing `ICommand` (returning `Fin<Unit>`).
    *   It will contain the necessary data: `Guid BlockId`, `Vector2 FromPosition`, `Vector2 ToPosition`.

3.  **Effect & Notification Definition:**
    *   In `src/Core/Application/Simulation/Effects/`, define `BlockMovedEffect.cs`. This is a simple data record containing the `BlockId` and the final `ToPosition`.
    *   In `src/Core/Application/Simulation/Notifications/`, define `BlockMovedNotification.cs`. This will mirror the effect's data.

4.  **Handler Implementation (`MoveBlockCommandHandler.cs`):**
    *   Create the handler in the `src/Features/Block/Move/` directory.
    *   **Dependencies:** It will inject `IBlockRepository`, `IGridStateService`, `ISimulationManager`, and relevant validation `IRule` services.
    *   **Logic (Functional Style):** The `Handle` method will use the `Fin<T>` monad and a LINQ expression to create a declarative validation and execution pipeline:
        1.  Fetch the `Block` entity from the repository.
        2.  Validate that the block is not locked (`BlockIsNotLockedRule`).
        3.  Validate that the target position is valid and unoccupied (`TargetPositionIsValidRule`).
        4.  If all validations pass, update the authoritative state in `IGridStateService`.
        5.  Enqueue the `BlockMovedEffect` into the `ISimulationManager`.
    *   The handler will return `Fin<Unit>` on success or a structured `Error` on failure.

5.  **Unit Testing:**
    *   In `tests/Features/Block/Move/`, create `MoveBlockCommandHandlerTests.cs`.
    *   Tests will cover:
        *   A successful move, verifying state change and effect queuing.
        *   Failure scenarios for each validation rule (e.g., moving to an occupied space, moving a locked block), ensuring the correct `Error` is returned and no state is changed.

## Phase 2: Presentation Layer Contracts

**Goal:** Define the interfaces that decouple the Presenter from the concrete View implementation. These interfaces will also reside in `src/BlockLife.Core.csproj`.

1.  **View Sub-Interface (Capability):**
    *   In `src/Features/Block/Move/`, define `IBlockAnimationView.cs`.
    *   This interface will define the animation capability: `Task AnimateMoveAsync(Guid blockId, Vector2 toPosition)`. The `Task` return type is crucial for the animation queuing system.

2.  **Composite View Interface:**
    *   In `src/Features/Block/Move/`, define `IGridView.cs` (or augment it if it already exists).
    *   This interface will follow the **Composite View Pattern**. It will expose the animation capability through a property: `IBlockAnimationView BlockAnimator { get; }`.

3.  **Presenter Definition:**
    *   In `src/Features/Block/Move/`, create `GridPresenter.cs`.
    *   It will implement `INotificationHandler<BlockMovedNotification>`.
    *   It will depend on the composite `IGridView` interface.
    *   It will contain the animation queuing logic as specified in the **Animation System Implementation Plan**.

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
    *   It will inject and use the `ISystemStateService` to ensure no input is processed while the system is locked (`Animating` or `LogicRunning`).

2.  **Command Dispatch:**
    *   Upon detecting a valid drag-and-drop action, the `GridPresenter` will populate and send a `MoveBlockCommand` using the injected `IMediator`.
    *   It will immediately call `_systemStateService.Lock(SystemState.LogicRunning)` to prevent further input until the action is resolved.

## Phase 5: Closing the Loop

**Goal:** Ensure the full, orchestrated sequence works as designed, incorporating the animation and system state plans.

1.  **Command Handling:** The `MoveBlockCommandHandler` executes, updates the model, and enqueues the `BlockMovedEffect`.
2.  **Simulation Step:** The `SimulationManager` processes the effect and publishes the `BlockMovedNotification`.
3.  **Notification Handling:** The `GridPresenter` receives the notification.
4.  **Animation Queuing:**
    *   The notification is added to the Presenter's internal `Queue`.
    *   The `TryProcessQueue` method is called.
    *   It transitions the system state: `_systemStateService.Lock(SystemState.Animating)`.
    *   It dequeues the notification and calls `View.BlockAnimator.AnimateMoveAsync(...)`.
5.  **Animation Execution:** The `BlockAnimationController` plays the `Tween` animation. The `await` in the Presenter pauses its execution.
6.  **System Unlock:** Once the animation `Task` completes, the Presenter's `TryProcessQueue` method continues. If the queue is now empty, it publishes a `SystemIdleNotification`. A dedicated `SystemStateController` handles this notification and calls `_systemStateService.Unlock()`, allowing user input once again.

## Phase 6: Testing

**Goal:** Verify the correctness of the implementation through both isolated unit tests and full-flow integration tests.

1.  **Presenter Unit Tests (`tests/`):**
    *   Create `GridPresenterTests.cs`.
    *   Test that input events correctly create and send a `MoveBlockCommand`.
    *   Test that receiving a `BlockMovedNotification` correctly calls the `IBlockAnimationView` method via the animation queue.

2.  **Integration Tests (`godot_project/tests/`):**
    *   Using a framework like GdUnit4Net, create `MoveBlockIntegrationTest.cs`.
    *   The test will programmatically:
        1.  Instantiate the grid scene.
        2.  Send a `MoveBlockCommand`.
        3.  Assert that the `ISystemStateService` becomes locked.
        4.  `await` the completion of the animation `Tween`.
        5.  Query the `GridView` to find the block's `Node2D` and assert that its final `GlobalPosition` is correct.
        6.  Assert that the `ISystemStateService` is unlocked.
