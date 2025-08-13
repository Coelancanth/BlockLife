# Implementation Plan: Automated In-Game Debug Console

This document outlines a revised, more intuitive implementation for the in-game rich text logging console. It replaces the confusing manual `NodePath` connection with an automated system where `SceneRoot` manages a dedicated, reusable debug UI scene.

## 1. High-Level Explanation

The core problem with the previous approach was the manual and error-prone process of connecting a `RichTextLabel` to the logging system. This new plan solves that by creating a self-contained "Debug Console" scene. The `SceneRoot` will only need a reference to this scene file (`PackedScene`). When the game starts, `SceneRoot` will automatically create an instance of the console, add it to the game, and wire it up to the logging system. This is a cleaner, more modular, and less error-prone workflow.

## Phase 1: Create the Self-Contained Debug Console Scene

**Goal:** Build a reusable UI scene that contains the `RichTextLabel` and the logic to control its visibility.

1.  **Create the Controller Script (`DebugLogConsole.cs`):**
    *   **Location:** `godot_project/infrastructure/debug/`
    *   **Inherits:** `Godot.CanvasLayer`. (Using a `CanvasLayer` ensures the console always draws on top of the game world).
    *   **Functionality:**
        *   It will have a public property `public RichTextLabel LogLabel { get; private set; }` to expose its `RichTextLabel` node to `SceneRoot`.
        *   It will handle input (e.g., pressing the `F1` key) in `_Input()` to toggle its own visibility, making it easy to show or hide the console.
        *   It will contain a `CheckButton` to allow toggling with the mouse.

2.  **Create the Scene (`DebugLogConsole.tscn`):**
    *   The root node will be a `CanvasLayer` with the `DebugLogConsole.cs` script attached.
    *   The scene will contain a `PanelContainer` (for background), a `MarginContainer`, and a `VBoxContainer`.
    *   Inside the `VBoxContainer`, there will be a `HBoxContainer` with a `Label` ("In-Game Console") and a `CheckButton` ("Visible").
    *   Below the `HBoxContainer`, the `RichTextLabel` node will be added. This is the node that will display the logs.

## Phase 2: Refactor `SceneRoot` for Automation

**Goal:** Modify `SceneRoot` to automatically instance and manage the `DebugLogConsole` scene.

1.  **Update `SceneRoot.cs`:**
    *   Remove the `[Export]` for the `_richTextLogLabelPath`.
    *   Add a new `[Export]` property: `private PackedScene _debugLogConsoleScene;`. This allows a `.tscn` file to be assigned in the inspector.
    *   In `_EnterTree()`:
        1.  The logic for preparing sinks will be updated. It will first check if `_debugLogConsoleScene` is not null.
        2.  If it's assigned, `SceneRoot` will call `_debugLogConsoleScene.Instantiate<DebugLogConsole>()`.
        3.  It will then add the new console instance as a child of `SceneRoot` using `AddChild()`.
        4.  It will get the `RichTextLabel` from the instance's public property (`consoleInstance.LogLabel`).
        5.  Finally, it will create the `RichTextLabelSink`, passing this label to it. If the scene is not assigned, the sink is simply not created.

## Phase 3: Manual Setup and Workflow

**Goal:** Define the simplified workflow for using the in-game console.

1.  **Scene Creation:** The `DebugLogConsole.tscn` is created once as a reusable tool.
2.  **Scene Setup:**
    *   The developer drags the `DebugLogConsole.tscn` file from the `FileSystem` dock onto the `Debug Log Console Scene` property of the `SceneRoot` node in the main scene.
3.  **Workflow:**
    *   That's it. When the game runs, the console will be automatically created and will start receiving logs. The developer can show or hide it by pressing the designated key (e.g., `F1`) or clicking its visibility checkbox.

This revised plan makes the in-game console a modular, plug-and-play feature, which is a significant improvement in usability and clarity.