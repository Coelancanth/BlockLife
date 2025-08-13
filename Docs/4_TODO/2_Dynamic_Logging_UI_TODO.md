# TODO List: Live Inspector Logging Controls

This document outlines the tasks and milestones required to implement the simplified `Dynamic_Logging_UI_Implementation_Plan` using a polling-based inspector approach.

## Milestone 1: Create the Log Settings Controller

**Objective:** Build the core `Node` that will manage and detect changes to the log settings at runtime.

-   [ ] **Task 1.1:** Create the `godot_project/infrastructure/logging/` directory if it doesn't exist.
-   [ ] **Task 1.2:** Create the `LogSettingsController.cs` script inheriting from `Godot.Node`.
-   [ ] **Task 1.3:** In `LogSettingsController.cs`, add the `[Export]` properties for `DefaultLogLevel` (int), `LogCategories` (string[]), and `LogLevels` (int[]).
-   [ ] **Task 1.4:** Add private fields to `LogSettingsController.cs` to store the last known state of the exported properties.
-   [ ] **Task 1.5:** Implement the public `Initialize` method that accepts and stores the `LoggingLevelSwitch` instances.
-   [ ] **Task 1.6:** Implement the `_Process` method to poll for changes between the exported properties and the last known state, and update the `LoggingLevelSwitch` objects when a change is detected.

## Milestone 2: Refactor `SceneRoot` for Integration

**Objective:** Update the application's entry point to use the new controller-based system.

-   [ ] **Task 2.1:** Modify `SceneRoot.cs`.
    -   [ ] **Sub-task 2.1.1:** Remove the `[Export]` property for the old `LogSettings` resource.
    -   [ ] **Sub-task 2.1.2:** Add a new `[Export]` `NodePath` property for the `LogSettingsController`.
-   [ ] **Task 2.2:** Update the `_EnterTree` method in `SceneRoot.cs`.
    -   [ ] **Sub-task 2.2.1:** Get the `LogSettingsController` node from the new path.
    -   [ ] **Sub-task 2.2.2:** Read the initial log level values directly from the controller's properties.
    -   [ ] **Sub-task 2.2.3:** Create the master and category `LoggingLevelSwitch` instances.
    -   [ ] **Sub-task 2.2.4:** Call `GameStrapper.Initialize` with the new switches.
    -   [ ] **Sub-task 2.2.5:** Call the `Initialize` method on the `LogSettingsController` instance, passing the switches to it.

## Milestone 3: Final Scene Setup and Testing

**Objective:** Configure the main scene in the Godot editor and perform a manual test to verify the system works as expected.

-   [ ] **Task 3.1:** In the Godot editor, create a new `Node` in the main scene and name it `LogSettingsController`.
-   [ ] **Task 3.2:** Attach the `LogSettingsController.cs` script to this new node.
-   [ ] **Task 3.3:** Configure the initial log levels in the Inspector for the `LogSettingsController` node.
-   [ ] **Task 3.4:** Select the `SceneRoot` node and assign the `LogSettingsController` to its exported `NodePath` property.
-   [ ] **Task 3.5:** Run the game from the editor.
    -   [ ] **Sub-task 3.5.1:** Verify that the initial logs respect the settings from the controller.
    -   [ ] **Sub-task 3.5.2:** While the game is running, select the `LogSettingsController` in the **Remote** scene tree.
    -   [ ] **Sub-task 3.5.3:** Change the `Default Log Level` in the Inspector and confirm that the verbosity of the log output changes immediately.
    -   [ ] **Sub-task 3.5.4:** Change a category-specific log level and confirm that only the relevant logs are affected.
-   [ ] **Task 3.6:** Mark this TODO list as complete.