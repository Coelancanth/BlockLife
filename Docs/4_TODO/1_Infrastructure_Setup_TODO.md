# TODO List: Foundational Infrastructure (Logger & DI)

This document outlines the concrete tasks and milestones required to implement the `Advanced_Logger_And_GameStrapper_Implementation_Plan`.

## Milestone 1: Core Dependencies and Configuration

**Objective:** Lay the groundwork by setting up project dependencies and creating the data resource for configuration.

-   [ ] **Task 1.1:** Add `Serilog.Sinks.File` and `Serilog.Sinks.Console` package references to `src/BlockLife.Core.csproj`.
-   [ ] **Task 1.2:** Add `Serilog.Sinks.Godot` package reference to `godot_project/BlockLife.Godot.csproj`.
-   [ ] **Task 1.3:** Create `src/Core/Infrastructure/Logging/LogCategory.cs` and populate it with initial, well-defined log categories (`Core`, `DI`, `Input`, etc.).
-   [ ] **Task 1.4:** Create the `godot_project/resources/LogSettings.cs` script, inheriting from `Resource` and exporting its properties (`DefaultLogLevel`, `EnableRichTextInGodot`, `CategoryLogLevels`).
-   [ ] **Task 1.5:** In the Godot editor, create a `LogSettings` resource file (e.g., `res://log_settings.tres`) from the new script.

## Milestone 2: Implement Logging and DI Core Logic

**Objective:** Build the C# classes responsible for logging, DI container setup, and MediatR pipeline behavior.

-   [ ] **Task 2.1:** Create the `godot_project/infrastructure/logging/RichTextLabelSink.cs` class, implementing the high-performance version that uses `RemoveLine(0)` for buffer management.
-   [ ] **Task 2.2:** Create the `src/Core/Application/Behaviors/` directory.
-   [ ] **Task 2.3:** Implement the `src/Core/Application/Behaviors/LoggingBehavior.cs` class to automatically log all MediatR requests, including their execution time and results.
-   [ ] **Task 2.4:** Implement the static `src/Core/GameStrapper.cs` class.
    -   [ ] **Sub-task 2.4.1:** Implement the `Initialize` method that accepts `LogSettings` and an optional `ILogEventSink`.
    -   [ ] **Sub-task 2.4.2:** Implement the private `ConfigureAndCreateLogger` method within `GameStrapper`.
    -   [ ] **Sub-task 2.4.3:** Ensure `GameStrapper` registers the `ILogger`, the `LoggingBehavior`, MediatR, and the `IPresenterFactory`.
    -   [ ] **Sub-task 2.4.4:** Ensure `GameStrapper` builds the service provider with `ValidateOnBuild = true` and `ValidateScopes = true`.

## Milestone 3: Integrate with Godot `SceneRoot`

**Objective:** Connect the Godot world to the C# core by having `SceneRoot` drive the initialization process.

-   [ ] **Task 3.1:** Create the `godot_project/scenes/Main/SceneRoot.cs` script.
-   [ ] **Task 3.2:** Add the `[Export]` properties for `LogSettings` and the `RichTextLabel` path to `SceneRoot.cs`.
-   [ ] **Task 3.3:** Implement the `_EnterTree` logic in `SceneRoot.cs`:
    -   [ ] **Sub-task 3.3.1:** Implement the singleton guard pattern to prevent multiple instances.
    -   [ ] **Sub-task 3.3.2:** Validate that the `_logSettings` resource is assigned.
    -   [ ] **Sub-task 3.3.3:** Instantiate the `RichTextLabelSink` if the path is provided.
    -   [ ] **Sub-task 3.3.4:** Call `GameStrapper.Initialize()` with the configuration.
    -   [ ] **Sub-task 3.3.5:** Retrieve the `IPresenterFactory` and `ILogger` from the service provider.
    -   [ ] **Sub-task 3.3.6:** Log a final "initialization complete" message.
-   [ ] **Task 3.4:** In the Godot editor, create the main scene, attach the `SceneRoot.cs` script, and assign the `log_settings.tres` resource to its exported property.

## Milestone 4: DI Container Validation Testing

**Objective:** Create a suite of unit tests to guarantee the integrity and correctness of the DI container setup.

-   [ ] **Task 4.1:** Create the `tests/BlockLife.Core.Tests/Infrastructure/DependencyInjection/` directory.
-   [ ] **Task 4.2:** Implement `DependencyResolutionTests.cs`.
    -   [ ] **Sub-task 4.2.1:** Write the test that uses reflection to discover all services based on established patterns (e.g., `PresenterBase<>`, `IRequestHandler<>`).
    -   [ ] **Sub-task 4.2.2:** Ensure the test attempts to resolve every discovered service from the container.
-   [ ] **Task 4.3:** Implement `DependencyLifetimeTests.cs`.
    -   [ ] **Sub-task 4.3.1:** Write a test to verify that singleton services (`ILogger`, `IPresenterFactory`) always resolve to the same instance.
    -   [ ] **Sub-task 4.3.2:** Write a test to verify that transient services (Presenters) resolve to new instances each time.
-   [ ] **Task 4.4:** Implement `ConfigurationValidationTests.cs`.
    -   [ ] **Sub-task 4.4.1:** Write a test to confirm that `ValidateScopes = true` catches invalid singleton-to-transient dependencies.
    -   [ ] **Sub-task 4.4.2:** Write a test to confirm that `ValidateOnBuild = true` catches unregistered dependencies.

## Milestone 5: Final Review and Documentation

**Objective:** Ensure the implementation is clean, documented, and ready for team use.

-   [ ] **Task 5.1:** Review all new code for adherence to the style guide.
-   [ ] **Task 5.2:** Add code comments to any complex or non-obvious sections of the implementation.
-   [ ] **Task 5.3:** Update the main `README.md` or a relevant architecture document to explain how to use the new logging system and how to register new services in `GameStrapper`.
-   [ ] **Task 5.4:** Mark this TODO list as complete.