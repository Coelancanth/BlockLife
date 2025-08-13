# Implementation Plan: Live Inspector Logging Controls

This document outlines a simplified, robust implementation for a dynamic logging system. It replaces the complex in-game UI plan with a more direct approach: allowing developers to change log levels in real-time by modifying exported properties in the Godot Editor's inspector during a debug session.

## 1. High-Level Explanation

This plan solves the need for dynamic logging by creating a single `LogSettingsController` node. This node will be placed in the main scene, and its exported properties (log levels) will be visible in the inspector. The script on this node will continuously check (poll) for any changes made to these properties while the game is running. When a change is detected, it will instantly update the live Serilog logging configuration, providing immediate feedback without requiring a complex UI.

## Phase 1: `LogSettingsController` Node

**Goal:** Create the central node that will hold the log settings and detect live changes from the inspector.

1.  **Create the Controller Script (`LogSettingsController.cs`):**
    *   **Location:** `godot_project/infrastructure/logging/`
    *   **Inherits:** `Godot.Node`.
    *   **Exported Properties:** It will export the same properties as the old `LogSettings` resource, using primitive arrays that Godot can reliably serialize:
        *   `DefaultLogLevel` (`int`, with `PropertyHint.Enum`).
        *   `LogCategories` (`string[]`).
        *   `LogLevels` (`int[]`, with `PropertyHint.ArrayType` to get an enum dropdown).
    *   **Internal State:** It will have private fields to store the "last known" values of the exported properties.
    *   **Public `Initialize` Method:** A method `Initialize(LoggingLevelSwitch master, Dictionary<string, LoggingLevelSwitch> categorySwitches)` will be created to receive the live `LoggingLevelSwitch` objects from `SceneRoot`.
    *   **Polling Logic in `_Process()`:** The `_Process` method will be implemented to:
        1.  Compare the current value of the exported properties against the "last known" values.
        2.  If a value has changed, update the corresponding `LoggingLevelSwitch`'s `MinimumLevel` property.
        3.  Update the "last known" value to match the new state.

## Phase 2: Core Logging Infrastructure (No Changes Needed)

**Goal:** Ensure the core logging system is configured to be externally controllable.

The changes made previously to `GameStrapper.cs` to accept `LoggingLevelSwitch` instances are already correct and perfectly suited for this new plan. No further modifications are needed in the `BlockLife.Core` project.

## Phase 3: `SceneRoot` Integration

**Goal:** Update `SceneRoot` to orchestrate the creation of the switches and connect them to the `LogSettingsController`.

1.  **Update `SceneRoot.cs`:**
    *   Remove the `[Export]` for the `LogSettings` resource.
    *   Add an `[Export]` for a `NodePath` to the `LogSettingsController` node in the scene.
    *   In `_EnterTree()`:
        1.  Get the `LogSettingsController` node from the exported path.
        2.  Read the *initial* values from the controller's exported properties.
        3.  Create the master `LoggingLevelSwitch` and the dictionary of category switches based on these initial values.
        4.  Call `GameStrapper.Initialize`, passing the newly created switches to it.
        5.  Call the `Initialize` method on the `LogSettingsController` instance, passing the same switches to it so it can control them at runtime.

## Phase 4: Manual Setup and Workflow

**Goal:** Define the simple, manual steps to use the new system.

1.  **Scene Setup:**
    *   A `LogSettingsController` node is added to the main scene.
    *   The `SceneRoot` node is updated to point to this new controller via its exported `NodePath`.
2.  **Live Debugging Workflow:**
    *   The developer runs the game from the editor.
    *   While the game is running, they select the `LogSettingsController` node in the "Remote" scene tree view.
    *   They can now change any of the log levels in the `Inspector`. The changes will take effect on the very next frame.