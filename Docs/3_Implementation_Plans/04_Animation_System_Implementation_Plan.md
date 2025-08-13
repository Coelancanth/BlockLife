# Animation System Implementation Plan

This document outlines the implementation plan for a robust, queue-based animation system, as conceptualized in `AnimationSystem.md`. The goal is to visually represent complex, multi-step logical chains (like cascading merges) as smooth, sequential animations, while ensuring the system remains responsive and free of race conditions.

## Phase 1: System-Wide State Management (The "Input Lock")

**Goal:** Establish a global state service to make gameplay sequences atomic and prevent conflicting user inputs during animations.

1.  **State Definition (`src/Core/System/`):**
    *   Create a `SystemState` enum: `Idle` (accepting input), `LogicRunning` (processing commands), `Animating` (playing visual feedback).
2.  **Service Contract (`src/Core/System/`):**
    *   Define an `ISystemStateService` interface.
    *   It must include methods to control the state: `void Lock(SystemState state)`, `void Unlock()`.
    *   It must include a property to query the state: `bool IsLocked { get; }`.
3.  **Service Implementation (`src/Core/System/`):**
    *   Create the `SystemStateService` class implementing the interface.
    *   Register it as a singleton service in the `GameStrapper`.
4.  **Input Guard (`src/Features/Input/`):**
    *   Modify the `InputPresenter` to inject `ISystemStateService`.
    *   Before processing any user input that could trigger a command, the presenter must first check if `_systemStateService.IsLocked` is `false`. If it is locked, the input is ignored.

## Phase 2: Presenter Enhancement for Animation Queuing

**Goal:** Modify the Presenter layer to handle a stream of notifications by playing their corresponding animations sequentially, never in parallel.

1.  **Presenter Internals (e.g., `src/Features/Grid/GridPresenter.cs`):**
    *   Add a `private readonly Queue<TNotification> _notificationQueue` to hold incoming game event notifications.
    *   Add a `private bool _isAnimationPlaying` flag, initialized to `false`.
2.  **Notification Handling Logic:**
    *   The `Handle(TNotification notification, ...)` method (from `INotificationHandler`) will no longer directly call the View.
    *   Its only responsibilities are to enqueue the notification into `_notificationQueue` and then call a new private, non-blocking method, `TryProcessQueue()`.
3.  **Asynchronous Queue Processor:**
    *   Create a new `private async void TryProcessQueue()` method within the presenter.
    *   **Guard Clause:** The method will immediately exit if `_isAnimationPlaying` is `true` or if the queue is empty.
    *   **Processing Logic:**
        *   Set `_isAnimationPlaying = true`.
        *   Dequeue the next notification.
        *   Call the appropriate animation method on the View (e.g., `_view.AnimateMergeAsync(...)`), which must now return a `Task`.
        *   `await` the `Task` returned by the view method.
        *   Once the `await` completes, set `_isAnimationPlaying = false`.
        *   Recursively call `TryProcessQueue()` again to process the next item in the queue, if any.
    *   **System Idle Signal:** When `TryProcessQueue` is called and finds the queue is empty, it will publish a new `SystemIdleNotification` via MediatR.

## Phase 3: View Layer Enhancement for Asynchronous Animation

**Goal:** Enable the Godot-specific View layer to signal back to the C# Presenter when an animation has completed.

1.  **Update View Interfaces (`src/Features/Grid/IGridView.cs`):**
    *   All methods that trigger animations (e.g., `AnimateBlockMove`, `AnimateMerge`) must be renamed to reflect their async nature (e.g., `AnimateMergeAsync`) and their return type must be changed from `void` to `Task`.
2.  **Implement Asynchronous Views (`godot_project/features/grid/GridView.cs`):**
    *   In the concrete View script, implement the updated interface.
    *   Inside the async methods, use Godot's `Tween` or `AnimationPlayer` to perform the animation.
    *   Crucially, use the `await ToSignal(tween_or_animation_player, "finished_signal_name")` pattern. This will pause the C# method execution until the Godot signal is emitted, correctly fulfilling the `Task` contract.

## Phase 4: Orchestrating the Atomic Loop

**Goal:** Connect the locking and unlocking mechanisms to the full `Command -> ... -> Animation` lifecycle.

1.  **Locking the System:**
    *   The `CommandHandler` that initiates a potentially complex sequence (e.g., `MoveBlockCommandHandler`) will be given the responsibility to inject `ISystemStateService`.
    *   As its first action, it will call `_systemStateService.Lock(SystemState.LogicRunning)`.
2.  **Transitioning the Lock:**
    *   When the `Presenter`'s `TryProcessQueue` method begins playing its *first* animation, it will call `_systemStateService.Lock(SystemState.Animating)` to transition the lock state.
3.  **Unlocking the System:**
    *   Create a new, simple `SystemStateController` class. It will be an `INotificationHandler<SystemIdleNotification>`.
    *   This controller will inject `ISystemStateService`.
    *   When it handles the `SystemIdleNotification` (published by the Presenter when its animation queue is empty), it will call `_systemStateService.Unlock()`.

## Phase 5: Validation and Testing

**Goal:** Ensure the new asynchronous and concurrent logic is robust and correct.

1.  **Unit Tests (`src/`):**
    *   Test the `SystemStateService` to ensure its locking logic is correct.
    *   Heavily test the `Presenter`'s queuing mechanism.
        *   Mock the `IView` dependency.
        *   Setup the async animation methods (e.g., `AnimateMergeAsync`) to return a `Task.CompletedTask`.
        *   In the test, send multiple notifications to the presenter in a tight loop.
        *   Use `Moq.Verify` to assert that the view's animation methods were called sequentially and in the correct order, not in parallel.
2.  **Integration Tests (`godot_project/`):**
    *   Use GdUnit4Net to create a test that triggers a real animation.
    *   The test should assert that the `SystemStateService` is locked immediately after a command is sent.
    *   The test should then `await` a signal that indicates the animation has finished, and finally assert that the `SystemStateService` has been unlocked. This validates the entire end-to-end flow.
