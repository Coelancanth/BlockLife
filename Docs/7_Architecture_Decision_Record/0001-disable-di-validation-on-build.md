# ADR 0001: Disable DI `ValidateOnBuild` to Support the Presenter Lifecycle

-   **Status**: Accepted
-   **Date**: 2025-08-11
-   **Deciders**: Gemini, Coel

## Context and Problem Statement

The project's architecture uses a Model-View-Presenter (MVP) pattern where the `View` (a Godot `Node`) is instantiated by the game engine, and the `Presenter` (a pure C# class) is created by a `PresenterFactory` that injects dependencies from a `Microsoft.Extensions.DependencyInjection` container.

The `Presenter`'s constructor requires an `IGridView` dependency. This dependency is fulfilled at runtime by the `PresenterFactory`, which is given the `View` instance that Godot created. The `IGridView` itself is never registered as a service in the DI container.

We enabled the `ValidateOnBuild = true` option in the DI container for maximum safety. This feature checks at startup if all registered services can be successfully constructed. However, this created a conflict:
1.  `MediatR` automatically scans assemblies and registers the `GridPresenter` as a transient service because it implements `INotificationHandler`.
2.  When `ValidateOnBuild` runs, the container tries to create a `GridPresenter` to validate it.
3.  It fails because it cannot resolve the `IGridView` dependency, which is not a registered service.

This results in a fatal crash at startup, preventing the application from running.

## Decision Drivers

-   We need to resolve the startup crash.
-   We want to maintain the core architectural pattern of the `PresenterFactory` providing the `View` instance at runtime.
-   We want to keep as much of the DI container's safety features as possible.
-   The solution should be pragmatic and not introduce excessive complexity for the current stage of the project.

## Considered Options

### 1. Disable `ValidateOnBuild`

-   **Description**: Simply turn off the `ValidateOnBuild` option in the `ServiceProviderOptions`. Keep `ValidateScopes = true`.
-   **Pros**:
    -   Immediately and simply solves the crash.
    -   Requires minimal code changes.
    -   Keeps the rest of the architecture clean and easy to understand.
-   **Cons**:
    -   We lose the safety net of the container verifying its entire dependency graph at startup. A missing dependency for a non-presenter service would only be discovered at runtime.

### 2. Implement a "Notification Bridge" (Advanced Pattern)

-   **Description**: An advanced architectural pattern to fully decouple presenters from DI registration.
    -   Presenters would no longer implement `INotificationHandler`.
    -   A central, singleton `PresenterNotifierService` would maintain a list of active presenters.
    -   A generic `NotificationBridge<T>` class that *does* implement `INotificationHandler` would be registered in the container.
    -   When the bridge receives a notification, it forwards it to the appropriate, currently active presenters in the `PresenterNotifierService`.
-   **Pros**:
    -   Allows `ValidateOnBuild = true` to be re-enabled, providing maximum DI safety.
    -   Architecturally pure; presenters are completely managed outside the container.
-   **Cons**:
    -   Adds significant complexity (multiple new classes, a new layer of indirection).
    -   Increases cognitive overhead for developers.
    -   Potentially over-engineering for the project's current scale.

### 3. Exclude Presenters from MediatR Scanning

-   **Description**: Keep `ValidateOnBuild = true` but prevent MediatR from automatically registering presenters. This was attempted by creating a custom `IPipelineBehavior` to filter them out.
-   **Pros**:
    -   Aims to get the best of both worlds.
-   **Cons**:
    -   Proved to be complex and brittle. The filtering logic is not straightforward and can be easily broken. It did not work as intended and still resulted in the same crash.

## Decision Outcome

**Chosen Option**: Option 1, "Disable `ValidateOnBuild`".

**Rationale**: This is the most pragmatic and simplest solution. It resolves the immediate problem with a single, well-documented line of code. The benefit of keeping the architecture simple and easy to understand for the development team currently outweighs the benefit of the full `ValidateOnBuild` safety check.

The `ValidateScopes = true` option is kept enabled, which still provides significant protection against the most common and dangerous DI errors (scoped dependencies in singletons).

This decision can be revisited in the future if the project grows to a scale where DI configuration errors become a frequent problem. The "Notification Bridge" pattern (Option 2) is a viable future path if necessary.
