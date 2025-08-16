# Project Architecture Guide (v3.1)

This document is the definitive architectural guide for the project. It has undergone rigorous review and hardening to ensure long-term stability, maintainability, and scalability. Adherence is mandatory and forms the basis of our development contract. This version incorporates critical feedback to further strengthen the architecture against common pitfalls and long-term maintenance challenges, specifically by adopting a **Pragmatic Model-View-Presenter (MVP)** approach and introducing **the "Humble Presenter Principle" to strictly prevent "God Presenters," now further enforced by the "Composite View Pattern."**

## 1. Architectural Goals & Trade-offs

This section clarifies the "why" behind our strict architectural choices.

*   **The Pain Point We Solve**: This architecture is designed to combat "spaghetti code," a common problem in game development where game logic (the Model) becomes tightly coupled with rendering and input handling (the View). The previous "Pure MVP" approach attempted to solve this by making Presenters completely Godot-agnostic, but this introduced significant "mapping overhead" and "interface bloat," forcing developers to write excessive boilerplate code and making it difficult to directly leverage Godot's powerful built-in features (like `Tween`, `AnimationPlayer`). This reduced development speed and increased friction with the engine. Fundamentally, this architecture aims to combat **uncontrolled entropy increase**. Without strict architectural constraints, system complexity grows exponentially with time and features, eventually leading to cascading bugs, near-zero iteration speed, and project stagnation. The adoption of Pragmatic MVP, while solving some issues, introduces a new risk: **Presenters becoming "God Objects"** that accumulate too many responsibilities, leading to similar problems of tight coupling and untestability within the presentation layer itself. The "Humble Presenter Principle" was introduced to address this, but it relies on developer discipline. Without a structured pattern to enforce it, Presenters can still inadvertently become bloated by directly managing numerous "Controller Nodes" and leaking View implementation details through `Get...()` methods.

*   **Our Chosen Approach**: We employ a combination of battle-tested patterns, now refined into a **Pragmatic MVP** (or "Smart View Pattern") further strengthened by the **Composite View Pattern**:
    1.  **Clean Architecture (Model Layer)**: By forbidding `using Godot;` in our core logic (Handlers, Services, Repositories), we create a compiler-enforced firewall, ensuring the game's core is portable and independently testable. This remains the absolute foundation.
    2.  **CQRS (Command Query Responsibility Segregation)**: By separating write operations (`Commands`) from read operations (`Queries`), we create a predictable, unidirectional data flow. State changes originate from a single, traceable source (Command Handlers), eliminating a massive class of state desynchronization bugs.
    3.  **Functional Core**: We use `LanguageExt.Core` to handle operation outcomes (`Fin<T>`) and optionality (`Option<T>`). This allows us to write declarative, composable business logic that is robust by design, eliminating entire classes of runtime errors like `NullReferenceException` and unhandled exceptions.
    4.  **Dependency Injection (DI) & IoC**: Through constructor injection, we eliminate "Temporal Coupling," ensuring objects are fully valid upon creation.
    5.  **Pragmatic Model-View-Presenter (MVP) with Composite View Pattern**: The Presenter now acts as a **smart bridge**, directly interacting with Godot Nodes to drive presentation. The architectural firewall has shifted:
        *   **Model Layer (Handlers, Services, Repositories)** remains absolutely pure and Godot-agnostic.
        *   **Presenter Layer** is now **Godot-aware** (`using Godot;` is allowed and encouraged). It handles all logic related to gameplay presentation, user interaction, and scene coordination. Crucially, it interacts with the View through a **composite interface** that exposes capabilities (e.g., `View.Animation.Play()`) rather than concrete controller nodes.
        *   **View Layer** becomes extremely minimal, primarily exposing node references and forwarding raw input events to the Presenter. With the Composite View Pattern, the main View node also acts as a **composition root** for its child "Controller Nodes," exposing their capabilities through a unified interface to the Presenter.
        This approach maximizes development speed and leverages Godot's strengths while preserving a pure, testable core and enforcing a truly "Humble Presenter."

*   **The Consequences of Not Following This Guide**: Deviating from this architecture will inevitably lead to:
    1.  **Mixed Logic**: Game logic scattered across various `_Process`, `_PhysicsProcess`, and `_Input` methods.
    2.  **Tight Coupling**: A fragile web of `GetNode` calls and signals, where any scene tree refactor risks runtime crashes.
    3.  **State Chaos**: Game state spread across multiple nodes, singletons (Autoloads), and static variables, with no single source of truth, leading to bugs that are nearly impossible to reproduce and debug.
    4.  **Untestability**: Logic deeply bound to Godot APIs cannot be unit-tested without a full engine instance, dramatically slowing down development and debugging cycles.
    5.  **God Presenters**: Without strict internal constraints on Presenter responsibilities, and specifically without the **Composite View Pattern** to enforce delegation, Presenters will become "God Objects" that are difficult to maintain, understand, and test, negating the benefits of the Pragmatic MVP. The `IView` interfaces will leak implementation details (e.g., `GetAnimationController()`), coupling Presenters to the internal structure of the View.
    If we do not formally adopt this "Pragmatic MVP" model and its accompanying Presenter constraints, the team's development efficiency will continue to suffer, especially when implementing complex animations and interactions. This could lead to developers bypassing the architecture for convenience, incorrectly placing logic in Views that should be in Presenters, ultimately leading to architectural decay and a chaotic mix of styles.
    In short, the consequences are **"unknowability"** and **"untrustworthiness."** Developers will be unable to confirm if their changes will break other parts (unknowability), nor can they trust that the system's current state is correct (untrustworthiness). Debugging will become a nightmare of guesswork and luck, rather than rigorous logical deduction.

*   **The Pragmatic Trade-offs (The Price of Discipline)**: This architecture is not free. We consciously accept the following costs for long-term stability:
    1.  **Boilerplate & Cognitive Overhead**: A simple feature requires creating multiple files (Command, Handler, View Interface, Presenter). This feels "slower" initially and has a steeper learning curve for new developers, especially with the introduction of functional programming concepts like `Fin` and `Option` which require a shift in thinking away from traditional imperative code. Additionally, each View now requires a few lines of standardized boilerplate code to manage its Presenter's lifecycle via composition. The **Composite View Pattern** further introduces more interfaces and an additional layer of abstraction, which can feel like "over-engineering" for very simple UI elements.
    2.  **Inconvenience of Strictness**: The physical separation of `src/` and `godot_project/` is architecturally pure but introduces friction in daily navigation. The need to map between Godot types and Model types at the Presenter boundary is a necessary, albeit tedious, task.
    3.  **Potential Performance Considerations**: The centralized Command Bus (MediatR) and monadic operations from `LanguageExt.Core` introduce complexities and small object allocations that must be managed. This guide provides patterns (Read Models, Fast-Path Results) to address these. In performance-critical hot paths, these patterns must be used with care.
    4.  **Not for Rapid Prototyping**: This architecture is optimized for the long-term health and scalability of a large project, not for the initial, chaotic phase of rapid prototyping.
    5.  **Risk of Architectural Dogma**: When a team adheres too rigidly to rules without understanding their underlying "why," there's a risk of over-engineering simple problems or sacrificing necessary performance in hot paths. Architecture is a map, not a straitjacket. Teams must understand the rationale behind each rule to make informed, documented exceptions when truly necessary.
    6.  **Team Capability Bottleneck**: This architecture demands a high level of discipline and learning capability from team members. It can amplify the impact of the weakest link in the team. A developer who doesn't deeply understand functional programming or CQRS principles might write code that is formally compliant but fundamentally distorts the pattern's intent, which can be harder to detect and fix than pure "spaghetti code."
    7.  **Tooling Tax**: While Section 6.2 mentions templates, they are not merely a suggestion but a **necessity**. Without robust IDE/CLI tools to automate boilerplate generation, the "friction cost" for developers will be extremely high, potentially leading them to seek shortcuts around architectural rules. This "tooling tax" must be paid by investing resources in tooling development early in the project.
    8.  **Bureaucracy of Exceptions**: Rule #15 (Performance Hot-Path Exception) is pragmatic. However, in real-world team collaboration, the process of requesting an "exception" itself can become bureaucratic, requiring reviews, discussions, and documentation, which can slow down the resolution of urgent performance issues. A clear, lightweight exception approval process must be established.
    9.  **Loss of Presenter Unit Testability & Portability**: This is the most significant trade-off of the Pragmatic MVP. Since Presenters now depend on Godot APIs, we cannot perform isolated, fast unit tests on them. Testing Presenters will **degrade to integration tests**, which must be run within the Godot environment (e.g., using GdUnit), making them slower and more complex than pure unit tests. Consequently, Presenter logic is now bound to the Godot engine and cannot be directly ported to other platforms (e.g., a dedicated server).
    10. **Increased Structural Complexity**: To prevent Presenters from becoming "God Objects," we introduce further constraints that often require creating more specialized "Controller Nodes" within the Godot scene tree. This increases the number of files and scene tree depth for a given feature, potentially increasing initial learning and navigation costs for developers unfamiliar with this pattern. We consciously trade "lower single-file cognitive complexity" for "higher project file count." The **Composite View Pattern** adds further interfaces and composition logic, increasing the initial setup complexity for views with multiple presentation concerns.

### 1.5 Assumptions & Limitations

*   **Assumptions**: Team members are willing and capable of learning and adopting these advanced patterns. This architecture demands a high level of discipline from the team. We will invest resources in establishing and maintaining a reliable **integration testing** solution (e.g., GdUnit) to ensure the quality of the Presenter layer code.
*   **Limitations**: This architecture is designed for medium to large-scale, long-lifecycle projects. For small games, game jams, or rapid prototyping phases, its overhead cost may outweigh the benefits. While Presenters can operate Godot nodes, they are **still forbidden from holding authoritative game state**. All core state must reside in the Model. Any state held within a Presenter should be considered temporary, derived, and purely for presentation purposes. The **Composite View Pattern** is most beneficial for views with multiple, distinct presentation concerns; for very simple views, the overhead might be disproportionate, and Rule #16.2 (Complexity Threshold) should guide the decision.

## 2. Core Philosophy

This project strictly separates **Logic (the 'Model')** from **Presentation (the 'View')**, with the **Presenter** acting as a smart coordinator.
-   **The Model**: Pure C# logic. It knows nothing about Godot Nodes, scenes, or rendering. It is independently testable. **Absolutely no `using Godot;` allowed.**
-   **The Presenter**: The smart, **humble** bridge between Model and View. It translates Model notifications into View actions and View events into Model commands. **It is Godot-aware and can `using Godot;` to directly interact with Godot Nodes, but its role is strictly that of a coordinator and delegator, not an executor of complex presentation logic. It interacts with the View through a composite interface, delegating presentation tasks to specialized Controller Nodes via this interface.**
-   **The View**: Godot scenes and nodes. They are "dumb" and primarily responsible for exposing node references to the Presenter and capturing raw input. With the **Composite View Pattern**, the main View node also acts as a composition root, exposing the capabilities of its child Controller Nodes through a unified interface.

-   **Single Source of Truth**:
    -   The Model is the only authoritative source of gameplay state. Treat everything else as a projection.
    -   Presenters and Views must not own or mutate authoritative state. Any local caches must be considered ephemeral and invalidated on notifications.
    -   All state changes flow through Commands and their Handlers. Views/Presenters never mutate Model state directly.

## 3. The Golden Rules (AI MUST FOLLOW)

### 1. THE FIRST PRINCIPLE: THE SINGLE AUTHORITATIVE WRITE MODEL

*   **Definition**: The entire application's core state is encapsulated in one or a set of clearly defined, DI-container-managed singleton services. We call this the **"Authoritative Write Model"**. This is the game's single source of truth. For example, `IPlayerStateService`, `IGridStateService`, etc., collectively form this authoritative model.
*   **The Iron Law**: **Modification of the Authoritative Write Model is only permitted inside a Command Handler.** Any other class type (Presenter, View, Service, etc.) that injects an authoritative model service may only call its read-only methods. This rule aims to leverage C#'s access modifiers and interface segregation to guarantee SSOT at the compiler level as much as possible.
*   **Corollary**: Any state within a Presenter or View must be considered a **temporary, derived, and disposable cache**. They must and can only synchronize data from the authoritative model by executing a `Query` or reacting to a `Notification`. Holding independent, mutable game state in Presenters or Views is strictly forbidden.
*   **Fundamental Rationale**: This rule is designed to architecturally eradicate the possibility of **State Desynchronization**. It enforces a predictable, unidirectional data flow, making system behavior trivial to reason about, debug, and test.

### 2. ATOMIC CONSTRUCTION VIA CONSTRUCTOR INJECTION (No Temporal Coupling)

An object must be fully valid, operational, and its dependencies resolved at the moment its constructor completes. We exclusively use Constructor Injection for all application services, handlers, and presenters.
*   **Rationale**: This rule eliminates an entire class of `NullReferenceException` bugs and makes the system predictable. It enforces that an object cannot exist in a "partially initialized" state. Method injection (e.g., `Initialize()` methods) and property injection for mandatory dependencies are forbidden as they create **Temporal Coupling**—the requirement that methods must be called in a specific order after construction.
*   **Litmus Test**: If you have a variable that is set *after* the constructor and is required for the object's core functionality, you are violating this rule. All mandatory dependencies MUST be supplied via the constructor.
*   ***Note***: This rule applies to all C# classes managed by our DI container. For Godot-instantiated `Node`s, we use patterns like the `PresenterFactory` to bridge the gap and ensure the Presenters they own still adhere to this principle.
*   ***Architect's Note on `Presenter.Initialize()`***:
    You will notice that our `PresenterLifecycleManager` calls a `presenter.Initialize()` method after construction. This appears to violate the "No Temporal Coupling" rule. This is a **deliberate and controlled exception** to the rule, confined strictly to the Presenter layer.
    *   **The Rationale**: A Presenter's initialization logic often requires access to Godot nodes provided by its View. These nodes are only guaranteed to be ready and accessible within Godot's `_Ready()` lifecycle method, which occurs long after the C# constructor has run.
    *   **The Contract**: The `Initialize()` method is the designated place for logic that depends on the View's nodes being ready. It bridges the gap between the C# object construction and the Godot node lifecycle. This pattern is **only permissible for Presenters** because they live at the boundary of our pure C# core and the Godot engine. It is **strictly forbidden** to use this `Initialize()` pattern for any class within the pure Model layer (Services, Handlers, Repositories).

### 2.1 (Sub-rule) ELIMINATE NULLS WITH `Option<T>`

To fundamentally eliminate `NullReferenceException` in core business logic, **you must** use `LanguageExt.Option<T>` to represent a value that may be missing.
*   In the **Model layer** (Services, Handlers, Repositories), any method that returns a reference type that could be missing, its return type **must** be `Option<T>`.
*   For non-essential dependencies in a constructor, the parameter type **must** be `Option<TDependency>`.
*   **In the Presenter and View layers**, when interacting directly with Godot API methods (e.g., `GetNodeOrNull<T>()`) that return `null` for missing nodes, it is **permissible to perform a `null` check**. However, this `null` check should be immediately followed by a conversion to `Option<T>` (e.g., `node.ToOption()`) before passing the value to any other method or logic. Direct `null` checks are considered a "code smell" and should be strictly confined to the minimal boundary where Godot's API forces their use.

### 3. NO `using Godot;` IN THE MODEL LAYER (Handlers, Services, Repositories)

MediatR Handlers (`IReuestHandler`), Services, and Repositories reside in the Model layer. They must not reference any Godot-specific types (`Node`, `Vector2`, `Color`, etc.).
*   **Rationale**: This creates a **compiler-enforced architectural boundary** for the Model layer. This independence is critical for **testability** (running logic tests without the Godot engine) and **portability**. This rule is the primary guardian of our Clean Architecture.
*   **Crucial Exception for Presenters**: Unlike the Model layer, **Presenters ARE permitted and encouraged to `using Godot;`**. They are the smart coordinators that bridge the pure Model with the Godot engine's presentation capabilities.
*   **Sub-rule 3.1: Primitives and Data Structures**: All data structures used within the Model, including vectors and geometric types, MUST be from the `System.Numerics` namespace or pure C# equivalents. A dedicated mapping layer or extension method is responsible for converting between Godot types and Model types at the Presenter boundary. This is a non-negotiable trade-off for testability.

### 4. LEVERAGE GODOT'S STRENGTHS (Don't Reinvent the Wheel)

Before writing custom C# code for a presentation-layer problem, always prefer Godot's built-in, highly optimized tools.
*   **Rationale**: The goal is to maximize **development efficiency and performance**. Godot's built-in systems (Animation, Signals, Resources) are written in C++, highly optimized, and deeply integrated into the editor's workflow.
*   **For Animation**: Use `AnimationPlayer` and `Tween`. The Presenter directs *what* to play, the View handles *how*.
*   **For Intra-View Communication**: Use Godot Signals. They are visible and manageable in the editor, providing crucial discoverability.
*   **For Data**: Use Godot's `Resource` system. It promotes a data-driven design easily accessible to non-programmers.

### 5. MIND THE MAIN THREAD (`async/await` Usage) - **CRITICAL GODOT CONSTRAINT**

The Godot engine runs on a single main thread. Blocking this thread will freeze the game. Therefore, the use of `async/await` is strictly controlled. **Using `async/await` on Godot's main thread for anything other than Godot's own async operations (like `ToSignal`) will cause game freezes and frame drops.**

*   **Rationale**: This rule exists to **guarantee application responsiveness** and prevent production game freezes.
*   **FORBIDDEN PATTERNS**:
    ```csharp
    // ❌ FORBIDDEN - Will freeze the game
    private async Task HandleBlockMovedAsync(Guid blockId, Vector2Int fromPos, Vector2Int toPos)
    {
        await View.AnimateMoveAsync(blockId, fromPos, toPos); // Blocks main thread!
        await View.ShowFeedbackAsync("Success"); // Compound blocking!
    }
    
    // ❌ FORBIDDEN - Any await that blocks main thread
    await SomeWebApiCall();
    await Task.Delay(1000);
    await File.WriteAllTextAsync();
    ```
*   **ALLOWED PATTERNS**:
    ```csharp
    // ✅ ALLOWED - Godot's own async operations
    await ToSignal(tween, "finished");
    await ToSignal(animationPlayer, "animation_finished");
    
    // ✅ ALLOWED - Fire-and-forget operations
    _ = SomeBackgroundTask(); // No await on main thread
    
    // ✅ PREFERRED - Signal-based coordination
    View.StartMoveAnimation(blockId, fromPos, toPos);
    // Animation completion handled via signals, not awaiting
    ```
*   **Handling Asynchronous Operations**: When an operation is inherently asynchronous (e.g., calling a web API, loading large assets), it MUST NOT be executed directly within a standard Command Handler or directly by the Presenter using `await` for the entire duration. The correct pattern is to introduce a dedicated **Async Orchestrator Service**:
    1.  The Presenter (or a dedicated View) initiates the async operation by calling a simple, immediately-returning method on an injected `Async Orchestrator Service`. The Presenter may show a loading indicator.
    2.  The `Async Orchestrator Service` (e.g., `ILoginOrchestrator`) is a DI-managed service responsible for handling all complex asynchronous logic, including `async/await` calls, retries, error handling, and background processing. This service is ignorant of the game loop.
    3.  Upon completion of the asynchronous work, the `Async Orchestrator Service` uses the `IMediator` to send a **new Command** with the final result (e.g., `ProcessLoginResultCommand`).
    4.  This new command is then processed by a standard, synchronous Handler on the main thread, which updates the Authoritative Model state.
    This pattern completely strips complex asynchronous logic from the Presenter, maintaining its single responsibility of "translating" between View and Model.

*   **CRITICAL WARNING**: Any `async/await` usage that can block the main thread for longer than a single frame (16ms at 60fps) will cause:
    - **Game Freezes**: Entire game becomes unresponsive
    - **Frame Rate Drops**: Visible stuttering and poor user experience  
    - **Input Lag**: User input becomes unresponsive during blocking operations
    - **Platform Issues**: Mobile platforms may kill the app for being unresponsive
    - **Editor Crashes**: Godot editor may crash during development

### 6. AVOID RACE CONDITIONS; ENSURE DETERMINISM

Mutate Model state only on the main thread, inside Handlers, and in a deterministic order.
*   **Rationale**: The core purpose is to achieve a **deterministic and predictable simulation**. By processing all state changes serially on a single thread, we ensure that for a given sequence of inputs, the game state will *always* be the same.

### 7. HANDLERS RETURN A `LanguageExt.Fin`

To handle predictable business logic failures, all Command Handlers **must** return `LanguageExt.Fin<T>`.
*   For standard Commands that do not return business data, the return type **must** be `Fin<Unit>`. The `Unit` type explicitly signals "the operation was successful, but there is no data to return," which enforces the one-way data flow principle.
*   For special Commands that require a "Fast-Path Result," the return type can be `Fin<TResult>`, where `TResult` is a DTO. This pattern must be used sparingly, only for latency-sensitive UI interactions that need to bypass the one-frame delay of the `Command->Query` sequence.
*   All Query Handlers also **must** return `Fin<TResult>` to gracefully handle cases where the query itself might fail (e.g., data not found).
*   Using exceptions for control flow is forbidden. The `Fin` type forces the caller (the Presenter) to explicitly handle both success and failure cases via methods like `Match`, `Map`, and `IfFail`, making the system more robust and self-documenting. The `Error` type within `Fin` allows for structured, detailed failure information, enabling more robust error handling and logging.

### 7.1 (Sub-rule) Fast-Path Result Contract

`ICommand<TResult>` is a compromise to pure CQRS, useful for immediate UI feedback. However, it is extremely dangerous if misused.
*   **The Contract**: The `TResult` returned by `ICommand<TResult>` **MUST** be a simple, non-authoritative value type or a minimal DTO (e.g., containing only a `bool IsSuccess`, a temporary `Guid`, or a simple error code).
*   **Forbidden**: It **MUST NOT** return a complex, authoritative snapshot DTO derived directly from the Model.
*   **Purpose**: The Fast-Path Result is solely for the **initiating party's** immediate UI feedback (e.g., disabling a button, showing a loading animation, displaying a quick success/failure message). Authoritative data synchronization **MUST** still occur through the standard `Effect -> Notification -> Projector -> Read Model` flow. Misusing this pattern can reintroduce tight coupling between Presenters and Handler implementation details, which we explicitly aim to avoid.
*   **Fast-Path Result Litmus Test**: Ask yourself: "**Can another completely unrelated system make a correct, authoritative business decision based *solely* on the data in this `TResult`?**" If the answer is "yes," then this `TResult` contains too much authoritative state and violates the contract. Its data should be obtained through the standard `Notification -> Query` flow. The `TResult` should only contain transient feedback information required by the initiating UI (e.g., `IsSuccess`, a temporary operation ID, or a simple error code).

### 7.2 (Sub-rule) ERRORS MUST BE STRUCTURED

When creating a `Fail` state, you **must** use the `Error.New(int code, string message, Option<Exception> exception = default)` overload.
*   **The Contract**: The `code` parameter **must** correspond to a well-defined value in a shared `ErrorCode` enum. The `message` should be a developer-facing description of the error.
*   **Rationale**: Using simple string-based errors (`Error.New("some error")`) is strictly forbidden. Structured errors are critical for:
    1.  **Robust Error Handling**: Allows consumers (like Presenters) to programmatically react to specific error types, rather than relying on brittle string matching.
    2.  **Telemetry & Analytics**: Enables easy aggregation and analysis of failures in your logging backend.
    3.  **Localization**: The error code can be used as a key to look up user-facing, localized error messages.

### 8. COMMANDS TRIGGER EFFECTS

Handlers should not directly cause visual changes. They modify the pure data state and enqueue `IEffect` objects into the `SimulationManager`.
*   **Rationale**: This enforces **one-way data flow** and promotes **extreme decoupling**. The Model announces "what happened" (an Effect), allowing any number of systems (VFX, SFX, UI) to react without requiring any change to the core logic.
*   **Note on Latency**: The standard `Effect -> Notification` pipeline has an inherent one-frame delay. This is desirable for decoupling most systems. However, for the original caller that needs immediate feedback, the "Fast-Path Result" pattern (see Rule #7) should be used. All other systems MUST rely on the delayed notification.

### 9. PRESENTERS ARE THE ONLY BRIDGE (VIA INTERFACES)

A Presenter listens to View events, creates Commands, and listens for Model notifications to update the View. To ensure decoupling, **a Presenter MUST depend on a View Interface (e.g., `IMyView`), not a concrete View class.**
*   **Rationale**: This establishes the Presenter as a **specialized, reusable, and testable mediator**. By depending on an interface, the Presenter is completely ignorant of Godot, enhancing its testability and allowing different Views to be swapped in without changing the Presenter's code.

### 10. ALL SERVICES VIA DI (NO STATIC LOCATORS)

Access services exclusively via Dependency Injection (constructor injection). **The use of static service locators (e.g., `GameStrapper.Provider`) is forbidden in application code (Views, Presenters, Handlers).**
*   **Rationale**: This enforces the **Inversion of Control (IoC)** principle. Static locators hide dependencies, make unit testing difficult, and violate the DI pattern. We use DI-managed entry points (like a Presenter Factory) to bridge the gap for Godot-instantiated objects.

*   ***Architect's Note on `SceneRoot.Instance`***:
    The static `SceneRoot.Instance` is a deliberate and controlled exception to this rule, serving a specific architectural purpose as the **Composition Root**. It is the **single, sanctioned bridge** between the Godot world (which instantiates nodes) and the C# DI world. Its usage is strictly limited to the `_Ready` method of Views for the sole purpose of accessing the `IPresenterFactory` via the `PresenterLifecycleManager`. Any other use of `SceneRoot.Instance` as a generic service locator in application logic is strictly forbidden and considered a violation of this architecture. This controlled exception prevents a "leaky" DI container while solving the fundamental bootstrapping problem.

### 11. DEFINE DATA CONTRACTS (DTOs) FOR BOUNDARIES

Any data crossing an architectural boundary (Model to Presenter, etc.) MUST be encapsulated in a pure, immutable Data Transfer Object (DTO). **By convention, all DTOs must be suffixed with `Dto` or `Snapshot`** (e.g., `GridSnapshotDto`, `PlayerStateDto`).
*   **Rationale**: This provides a stable, explicit contract and enforces true decoupling. It prevents "leaky abstractions" where knowledge of the Model's internal data structures spreads across the codebase, creating brittle connections.
*   ***Note***: To enforce immutability at the code level, it is highly recommended to define DTOs using C#'s `record` type or a class with `init-only` properties.
    ```csharp
    // Preferred way to define a DTO
    public record GridSnapshotDto(int Width, int Height, IReadOnlyList<BlockDto> Blocks);
    ```

### 12. MANAGE LIFECYCLES EXPLICITLY

An object's lifetime in the C# world MUST be explicitly tied to its corresponding object's lifetime in the Godot world. We do not rely on the Garbage Collector to manage objects with cross-world responsibilities. **The mandatory `SceneRoot` and "Lifecycle Manager Composition Pattern" (detailed in Section 4) is the prescribed method for achieving this, ensuring Presenters are correctly created and disposed alongside their Views.**

### 13. FORMALIZE QUERIES

All reads from the Model by a Presenter MUST go through a formal Query pattern, using our custom `IQuery<TResult>` interface.
*   **Rationale**: This ensures that read-side logic is also centralized, reusable, and testable. It prevents Presenters from containing complex data-fetching logic and maintains architectural consistency between read and write operations. Query Handlers should be optimized for performance, potentially reading from pre-calculated projections or caches to avoid blocking the main thread.

### 14. OPTIMIZE READS WITH DEDICATED READ MODELS (PROJECTIONS)

While the Authoritative Write Model is the single source of truth, it is optimized for transactional consistency, not necessarily for read performance. For complex or frequent queries, Query Handlers should not read directly from the Write Model.
*   **Definition**: A **Read Model** (or Projection) is a denormalized, pre-calculated cache of data tailored specifically for a query's needs. It is not authoritative.
*   **The Workflow**:
    1.  A `CommandHandler` modifies the authoritative **Write Model**.
    2.  The resulting `Notification` is published.
    3.  A dedicated `NotificationHandler`, called a **Projector**, listens for this notification.
    4.  The Projector's sole responsibility is to update the relevant **Read Model** based on the notification's data.
    5.  A `QueryHandler` can then read from this highly optimized Read Model almost instantly, without performing complex joins or calculations.
*   **Rationale**: This pattern architecturally separates write-side concerns from read-side concerns, leading to maximum performance for data retrieval operations. It prevents slow queries from blocking the main thread. The cost is the eventual consistency and the extra code for the Projectors.
*   **Crucial Note on Consistency**: **Projector-updated Read Model data is eventually consistent.** Within the same logical frame, a caller sending a command **cannot** assume that a subsequent query will immediately reflect the changes caused by that command. If immediate confirmation is required, the "Fast-Path Result" pattern (Rule #7.1) must be used, but only for obtaining the operation's success status, not for full data synchronization.

### 15. THE PERFORMANCE HOT-PATH EXCEPTION (Controlled Bypass)

For logic that must run every frame in `_Process` or `_PhysicsProcess` (e.g., player character movement, continuous physics adjustments), the full `Command -> Handler -> Effect -> Notification` pipeline introduces unacceptable latency and GC pressure. For these **strictly defined performance-critical hot-paths**, a controlled bypass is permitted under the following rigid conditions:

1.  **Bypass is Read-Dominant**: The logic inside `_Process` can directly call injected services to **read** data for immediate calculations (e.g., `_playerQueryService.GetPlayerPosition()`).
2.  **Writes are Non-Authoritative or Fire-and-Forget**:
    *   If the write operation updates **non-authoritative, view-specific state** (e.g., visual effects, temporary UI state), the Presenter may directly call a method on the View or a dedicated View-Model service.
    *   If the write operation **must** eventually update the authoritative model, it should be treated as a "fire-and-forget" command. The Presenter can call a highly-optimized service method that performs the critical state update directly and immediately returns. This service is then responsible for queuing a lightweight `Effect` if other systems need to be notified later.

   ***Example and Counter-Example***:
   - **CORRECT USAGE (Non-Authoritative Write)**: Inside `_Process`, a `PlayerPresenter` directly sets the rotation of a visual-only weapon node to follow the mouse cursor. This rotation is purely cosmetic and does not affect gameplay logic.
     ```csharp
     // In PlayerPresenter._Process()
     View.SetWeaponVisualRotation(mouseDirection); // This is acceptable.
     ```
   - **CORRECT USAGE (Fire-and-Forget Authoritative Write)**: Inside `_PhysicsProcess`, the `PlayerPresenter` calls a highly optimized service to update the player's core position in the authoritative model.
     ```csharp
     // In PlayerPresenter._PhysicsProcess()
     _playerMovementService.UpdatePosition(newPosition); // Acceptable. The service handles the direct state mutation.
     ```
   - **INCORRECT USAGE (Violation)**: Inside `_Process`, the `PlayerPresenter` directly calls a service to change the player's `Health` property. Player health is a critical authoritative state with complex business rules (e.g., cannot go below 0, might trigger 'OnDamage' effects). This kind of change **MUST** go through a standard `TakeDamageCommand` to ensure all business rules are correctly applied.
     ```csharp
     // In PlayerPresenter._Process()
     // _playerStateService.Health -= 1; // FORBIDDEN! This bypasses all validation and effects.
     ```
    
3.  **No Business Logic**: The logic in the bypass path must be mechanically simple (e.g., `position += velocity * delta`). Complex business rule validations **must** still go through the standard Command Handler pipeline.
4.  **Documentation is Mandatory**: Any use of this pattern must be explicitly documented in the code with a comment explaining why the standard CQRS pattern is not suitable for that specific case.

*   **Rationale**: This rule provides a pragmatic, architecturally-sanctioned "escape hatch" to prevent uncontrolled, ad-hoc solutions for performance-critical code. It acknowledges that one pattern does not fit all use cases, trading off architectural purity for real-world performance, but within a controlled and documented framework.

### 16. THE HUMBLE PRESENTER PRINCIPLE (Preventing "God Presenters")

This is the cornerstone principle for our presentation layer. While Presenters are Godot-aware, their power is strictly constrained. A "Humble Presenter" understands that its role is one of **coordination**, not execution. It adheres to the following four pillars, now further enforced by the **Composite View Pattern**:

#### CRITICAL CONSTRAINT: Presenters and MediatR Interfaces
**Presenters MUST NOT implement `INotificationHandler<T>` or `IRequestHandler<T,R>` interfaces.** These interfaces cause MediatR to automatically register the implementing class during assembly scanning, requiring all constructor dependencies to be available in the DI container. Since Presenters require View dependencies (which are Godot-managed, not DI-managed), this creates an unresolvable dependency conflict.

**Standard Solution**: Use the **Static Event Bridge Pattern** from [Standard Patterns](Standard_Patterns.md):
- Infrastructure handlers (DI-managed) implement `INotificationHandler<T>`
- Bridge handlers expose static events that presenters subscribe to
- Presenters are created via `PresenterFactory` with their View dependency injected
- **Reference Implementation**: `BlockPlacementNotificationBridge` and `BlockManagementPresenter`

**Example Violation** (FORBIDDEN):
```csharp
public class MyPresenter : PresenterBase<IMyView>, 
    INotificationHandler<SomeNotification>  // ❌ FORBIDDEN
{
    // This will cause DI container validation to fail
}
```

**Example Correct Implementation**:
```csharp
public class MyPresenter : PresenterBase<IMyView>  // ✅ No handler interfaces
{
    // Handle notifications through other patterns
}
```

*   **A. The Coordination Principle (Coordinate, Don't Execute)**
    A Presenter's code must read like a high-level script or a checklist. It defines **"what"** happens and **"when,"** but delegates the **"how"** to other, more specialized components. Its methods should be short and primarily consist of calls to other objects.

*   **B. The Delegation Principle (Delegate via Composite Interface, Don't Micromanage)**
    For any non-trivial presentation logic (animations, VFX, SFX, complex UI interactions), the Presenter **MUST** delegate to specialized **Controller Nodes** within the View hierarchy. Crucially, this delegation occurs **through a composite View interface** (see Composite View Pattern below) that exposes capabilities as properties (e.g., `View.Animation.Play()`) rather than concrete types or `Get...()` methods. This ensures the Presenter is completely decoupled from the internal structure of the View. For any complex, reusable, Godot-agnostic algorithms (pathfinding, damage formulas), the logic **MUST** be extracted into a pure C# `Service` and injected via DI.

*   **C. The Scope Principle (Focus, Don't Disperse)**
    A Presenter must be responsible for only **one independent, user-perceptible feature** or a cohesive set of related UI elements. Monolithic Presenters managing disparate functionalities are strictly forbidden. When in doubt, create a new, smaller, more focused Presenter.

*   **D. The State-Impotence Principle (Reflect, Don't Own)**
    A Presenter is **forbidden** from holding any **authoritative game state** (e.g., `currentHealth`, `ammoCount`). It may only hold temporary, non-authoritative presentation state (e.g., `private bool _isAnimating;`). To read authoritative state, it sends a `Query`; to change it, it sends a `Command`.

#### 16.1 (Sub-rule) Pragmatic `async` in Presenters - **UPDATED FOR GODOT MAIN THREAD SAFETY**

**⚠️ CRITICAL UPDATE**: Based on Move Block implementation stress testing, the previous guidance on `async/await` in Presenters has been **significantly revised** to prevent main thread blocking.

*   **STRICTLY FORBIDDEN**: Using `async/await` for any operation that blocks the main thread:
    ```csharp
    // ❌ FORBIDDEN - Blocks main thread, causes game freezes
    private async Task HandleBlockMovedAsync(...)
    {
        await View.AnimateMoveAsync(...); // Will freeze the game!
    }
    ```

*   **ALLOWED (With Extreme Caution)**: Using `await` **only** for Godot's own async primitives that don't block the main thread:
    ```csharp
    // ✅ ALLOWED - Godot's internal async operations
    await ToSignal(tween, "finished");
    await ToSignal(animationPlayer, "animation_finished");
    ```

*   **STRONGLY PREFERRED**: Use signal-based coordination instead of `async/await`:
    ```csharp
    // ✅ PREFERRED - Non-blocking signal pattern
    View.StartMoveAnimation(blockId, fromPos, toPos);
    // Connect to animation completion signal instead of awaiting
    ```

*   **FORBIDDEN**: Using `async/await` to perform long-running, non-presentation tasks (e.g., I/O, web requests). Such operations **MUST** follow **Rule #5** and be handled by a dedicated **Async Orchestrator Service**.

*   **ARCHITECTURE IMPACT**: This change means most Presenter methods should be **synchronous** to prevent accidental main thread blocking. Any complex async coordination should be delegated to dedicated services or handled via Godot's signal system.

#### 16.2 (Sub-rule) The Complexity Threshold
For **exceptionally simple** presentation logic (e.g., a method under 10 lines that just sets a `Label.Text` or toggles `Visible`), a Presenter may directly manipulate a Godot node without a dedicated Controller. When logic grows beyond this, or when multiple distinct presentation concerns arise within a single view, it is a **mandatory refactoring signal** to apply the **Composite View Pattern** and the Delegation Principle.

#### 16.3 (Sub-rule) The Simple View Exception Principle
To address the potential for over-engineering with the Composite View Pattern in very simple scenarios, the **Simple View Pattern** is introduced as a formal, lightweight alternative.

*   **Definition:** A view **MUST** use the "Simple View Pattern" instead of the "Composite View Pattern" when it meets **all** of the following conditions:
    1.  The view contains only a small number (e.g., fewer than 5) of UI nodes that need to be directly accessed by the Presenter.
    2.  The view does not involve complex, asynchronously coordinated presentation flows (e.g., no `async/await` coordinating multiple animations or Tweens).
    3.  All presentation logic within the view is exceptionally straightforward (e.g., merely setting `Text`, `Visible`, `Modulate` properties, or simple event subscriptions).

*   **Pattern Implementation:**
    1.  **View Interface:** The view interface **MAY directly expose** concrete Godot node types as read-only properties, rather than exposing sub-interfaces.
    2.  **View Implementation:** The view node itself implements this simple interface, providing the property implementations via `[Export]` attributes or `GetNode` calls. Dedicated child "Controller Nodes" are not required.
    3.  **Presenter Implementation:** The Presenter can directly access these node properties through the simple view interface and perform basic operations. The Presenter remains "humble" by ensuring its logic is still high-level coordination, delegating the "how" of Godot node manipulation to the view's exposed properties.

    ```csharp
    // src/Features/HUD/Health/IHealthBarView.cs
    using Godot; // Allowed to expose Godot types in View interfaces

    /// <summary>
    /// A simple view interface for a health bar.
    /// It directly exposes the required Godot nodes, following the "Simple View Pattern".
    /// This avoids the overhead of the Composite View Pattern for simple UI elements.
    /// </summary>
    public interface IHealthBarView
    {
        // Directly exposes the ProgressBar node.
        // The Presenter can now access it without going through a sub-interface.
        ProgressBar HealthProgressBar { get; }

        // Directly exposes the Label for numeric display.
        Label HealthLabel { get; }
    }
    ```

    ```csharp
    // godot_project/features/hud/health/HealthBarView.cs
    using Godot;
    using System;

    // Implements the simple interface and the standard IPresenterContainer.
    public partial class HealthBarView : Control, IHealthBarView, IPresenterContainer<HealthBarPresenter>
    {
        // --- IPresenterContainer Implementation (Standard) ---
        public HealthBarPresenter Presenter { get; set; }
        private IDisposable _lifecycleManager;

        // --- IHealthBarView Implementation ---
        // Use [Export] for easy wiring in the Godot editor.
        [Export]
        public ProgressBar HealthProgressBar { get; private set; }

        [Export]
        public Label HealthLabel { get; private set; }

        // --- Standard Lifecycle Management ---
        public override void _Ready()
        {
            // The lifecycle management boilerplate remains the same. It's robust and works perfectly here.
            _lifecycleManager = SceneRoot.Instance?.CreatePresenterFor(this);
            if (_lifecycleManager == null) return;
        }

        public override void _ExitTree()
        {
            _lifecycleManager?.Dispose();
        }
    }
    ```

    ```csharp
    // src/Features/HUD/Health/HealthBarPresenter.cs
    using MediatR; // Assuming it listens to notifications

    /// <summary>
    /// A presenter for the HealthBar. It follows the "Simple View Pattern".
    /// It's still "humble" because its logic is simple coordination,
    /// but it interacts more directly with the view's nodes via the ISimpleHealthBarView interface.
    /// </summary>
    public class HealthBarPresenter : PresenterBase<IHealthBarView>
    {
        private readonly IMediator _mediator;

        public HealthBarPresenter(IHealthBarView view, IMediator mediator) : base(view)
        {
            _mediator = mediator;
        }

        public override void Initialize()
        {
            // Subscribe to notifications, e.g., PlayerHealthChangedNotification
            // _mediator.Subscribe<PlayerHealthChangedNotification>(OnHealthChanged);

            // Example of initial setup
            UpdateHealthDisplay(100, 100);
        }

        // This would be the handler for a notification
        private void OnHealthChanged(/* PlayerHealthChangedNotification notification */)
        {
            // UpdateHealthDisplay(notification.CurrentHealth, notification.MaxHealth);
        }

        private void UpdateHealthDisplay(float currentHealth, float maxHealth)
        {
            // The presenter's logic is still high-level coordination.
            // It directly manipulates the nodes exposed by the simple view interface.
            // This is much cleaner than the full Composite View Pattern for this simple case.
            if (View.HealthProgressBar != null)
            {
                View.HealthProgressBar.MaxValue = maxHealth;
                View.HealthProgressBar.Value = currentHealth;
            }

            if (View.HealthLabel != null)
            {
                View.HealthLabel.Text = $"{currentHealth} / {maxHealth}";
            }
        }

        public override void Dispose()
        {
            // Unsubscribe from notifications
            base.Dispose();
        }
    }
    ```

## 4. Object Lifetime Management

This section defines the mandatory, automated pattern for managing object lifecycles to prevent memory leaks and eliminate manual configuration errors.

### The Core Problem: Bridging Godot and C# DI

Godot instantiates Nodes (`Views`) without knowledge of our C# Dependency Injection container. Our C# objects (`Presenters`, `Services`) require dependencies from this container. The challenge is to connect them reliably and automatically.

### The Solution: The `SceneRoot` & Lifecycle Manager Composition Pattern

We solve this with a robust, two-part pattern that is mandatory for all Views that require a Presenter.

**Part 1: `SceneRoot.cs` (The Singleton DI Entry Point)**

Create a single script, `SceneRoot.cs`, and attach it to the absolute root node of your main scene(s). This node acts as the sole entry point to the C# DI world.

```csharp
// godot_project/scenes/SceneRoot.cs
public partial class SceneRoot : Node
{
    // A controlled, well-documented static instance.
    // Its ONLY purpose is to bridge the Godot world to the C# DI world.
    public static SceneRoot Instance { get; private set; }

    public IPresenterFactory PresenterFactory { get; private set; }
    private IServiceProvider _serviceProvider;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            // This ensures only one SceneRoot exists, preventing state duplication.
            QueueFree();
            return;
        }
        Instance = this;
        
        // Initialize DI container here (e.g., using your GameStrapper)
        // _serviceProvider = GameStrapper.Initialize();
        // PresenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
        GD.Print("SceneRoot initialized and DI container is ready.");
    }

    /// <summary>
    /// A centralized, simplified helper method for Views to create their presenters.
    /// This encapsulates the boilerplate logic and reduces chances of error in the View.
    /// </summary>
    /// <returns>A configured lifecycle manager that the view must hold and dispose.</returns>
    public PresenterLifecycleManager<TViewNode, TViewInterface, TPresenter> 
        CreatePresenterFor<TViewNode, TViewInterface, TPresenter>(TViewNode view)
        where TViewNode : Node, TViewInterface, IPresenterContainer<TPresenter>
        where TViewInterface : class
        where TPresenter : PresenterBase<TViewInterface>, IDisposable
    {
        if (PresenterFactory == null)
        {
            GD.PrintErr($"FATAL: PresenterFactory is not ready. Cannot create presenter for {view.Name}.");
            GetTree().Quit();
            return null; // Or throw an exception if preferred for immediate crash.
        }
        
        return new PresenterLifecycleManager<TViewNode, TViewInterface, TPresenter>(view, PresenterFactory);
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            // Clean up DI container if it's disposable
            (_serviceProvider as IDisposable)?.Dispose();
            Instance = null;
        }
    }
}
```

**Part 2: `IPresenterContainer.cs` (The Contract for Views)**

This interface defines a contract that any View needing a Presenter must implement.

```csharp
// src/Core/Presentation/IPresenterContainer.cs
public interface IPresenterContainer<TPresenter> where TPresenter : class
{
    /// <summary>
    /// Gets or sets the presenter instance associated with this container.
    /// The property MUST have a public setter for the lifecycle manager to assign it.
    /// </summary>
    TPresenter Presenter { get; set; }
}
```

**Part 3: `PresenterLifecycleManager.cs` (The Presenter Lifecycle Handler)**

This is a plain C# class that encapsulates the logic for creating and disposing a Presenter. Views will *contain* an instance of this class.

```csharp
// src/Core/Presentation/PresenterLifecycleManager.cs
using System;
using Godot;

// TViewNode: The concrete Godot Node type (e.g., Node2D)
// TViewInterface: The interface the Presenter depends on (e.g., IMyView)
// TPresenter: The concrete Presenter type (e.g., MyPresenter)
public class PresenterLifecycleManager<TViewNode, TViewInterface, TPresenter> : IDisposable
    where TViewNode : Node, TViewInterface, IPresenterContainer<TPresenter> // TViewNode is a Godot Node, implements its interface and the container interface
    where TViewInterface : class                                       // TViewInterface is a class/interface
    where TPresenter : PresenterBase<TViewInterface>, IDisposable      // TPresenter inherits from PresenterBase<TViewInterface> and is disposable
{
    private readonly TViewNode _view;
    private IDisposable _presenterDisposable;

    /// <summary>
    /// Creates the presenter, injects it into the view, and calls its Initialize method immediately upon construction.
    /// This eliminates temporal coupling and ensures the presenter is ready as soon as the manager is created.
    /// </summary>
    public PresenterLifecycleManager(TViewNode view, IPresenterFactory presenterFactory)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        
        if (view.Presenter != null)
        {
            GD.PrintErr($"Presenter already exists for view '{_view.GetType().Name}'. Aborting new initialization.");
            return;
        }

        // The factory creates the presenter, injecting the view instance (which implements TViewInterface)
        var presenter = (presenterFactory ?? throw new ArgumentNullException(nameof(presenterFactory)))
            .Create<TPresenter, TViewInterface>(_view);
        
        _presenterDisposable = presenter;
        _view.Presenter = presenter;
        
        // Call presenter's own initialization logic
        presenter.Initialize(); 
    }

    /// <summary>
    /// Disposes the presenter. This should be called from the View's _ExitTree() method.
    /// </summary>
    public void DisposePresenter()
    {
        Dispose(); // Alias for Dispose()
    }

    public void Dispose()
    {
        _presenterDisposable?.Dispose();
        _presenterDisposable = null;
        if (_view != null) // Check if view is still valid before trying to set property
        {
            _view.Presenter = null;
        }
    }
}
```

**How to Use It (The New Standard Workflow)**

Your `MyView.cs` now becomes incredibly simple and robust, using composition instead of problematic inheritance.

```csharp
// godot_project/features/my_feature/MyView.cs
using Godot; // Required for Node2D
using System; // For IDisposable

// Inherits from a specific Godot Node type (e.g., Node2D), NOT a custom ViewBase.
// Implements its own view interface AND the new IPresenterContainer interface.
public partial class MyView : Node2D, IMyView, IPresenterContainer<MyPresenter>
{
    // The Presenter is now a public property to satisfy the IPresenterContainer interface.
    public MyPresenter Presenter { get; set; }

    // The view holds its own lifecycle manager instance.
    private IDisposable _lifecycleManager;

    public override void _Ready()
    {
        // This is the new standard boilerplate for ALL views that need a Presenter.
        // It's explicit, clear, and robust.
        // The boilerplate is now a single, clean, and intention-revealing line.
        // It's much harder to make a mistake here.
        _lifecycleManager = SceneRoot.Instance?.CreatePresenterFor(this);

        if (_lifecycleManager == null) return; // SceneRoot will log the error and quit if fatal.
        
        // Now you can safely use the presenter
        // Presenter.DoSomething();
    }

    public override void _ExitTree()
    {
        // This is also standard boilerplate for ALL views that need a Presenter.
        // Dispose is idempotent and safe to call.
        _lifecycleManager?.Dispose();
    }

    // --- IMyView interface implementations ---
    // Example:
    // The IMyView interface is now minimal, primarily exposing Godot node references.
    // With the Composite View Pattern, this would typically be properties returning sub-interfaces.
    // public ProgressBar GetMyProgressBar() => GetNode<ProgressBar>("MyProgressBar"); // Old way
    // public IProgressBarView ProgressBar { get; } // New way with Composite View Pattern
}
```

### 4.2 Async Orchestration Service Pattern

This section provides concrete code examples for implementing the Async Orchestrator Service pattern, as described in Rule #5, to handle complex asynchronous operations gracefully and maintain Presenter's single responsibility.

**1. Define the Async Operation Handle (`IAsyncOperation`):**
This interface allows the caller to monitor and potentially cancel the asynchronous work.

```csharp
// src/Core/Async/IAsyncOperation.cs
using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a handle to a long-running operation that can be
/// monitored and potentially cancelled.
/// </summary>
public interface IAsyncOperation
{
    /// <summary>
    /// The underlying Task representing the asynchronous operation.
    /// </summary>
    Task Task { get; }

    /// <summary>
    /// Requests cancellation of the asynchronous operation.
    /// </summary>
    void RequestCancellation();
}
```

**2. Define the Interface (The Contract):**
```csharp
// src/Features/Authentication/IAuthOrchestrator.cs
using System.Threading.Tasks;

public interface IAuthOrchestrator
{
    /// <summary>
    /// Initiates the login process. This method returns immediately
    /// and the process runs in the background. It returns a handle
    /// to the operation for monitoring and cancellation.
    /// </summary>
    IAsyncOperation BeginLoginProcess(string username, string password);
}
```

**3. Implement the Service (The Implementation):**
```csharp
// src/Features/Authentication/AuthOrchestrator.cs
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Godot; // For GD.PrintErr

public class AuthOrchestrator : IAuthOrchestrator
{
    private readonly IMediator _mediator;
    private readonly IAuthApiClient _apiClient; // A service that actually calls the web API

    public AuthOrchestrator(IMediator mediator, IAuthApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    public IAsyncOperation BeginLoginProcess(string username, string password)
    {
        var cts = new CancellationTokenSource();
        var task = LoginAsync(username, password, cts.Token);
        return new AsyncOperation(task, cts);
    }

    private async Task LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        try
        {
            // The API client call should accept a CancellationToken.
            var result = await _apiClient.LoginAsync(username, password, cancellationToken);
            
            // Check for cancellation after the async call completes but before dispatching.
            cancellationToken.ThrowIfCancellationRequested();

            // Once the async work is done, dispatch a standard, synchronous command
            // with the result. This command will be handled on the main thread
            // by a regular command handler to update the authoritative state.
            var command = new ProcessLoginResultCommand(result);
            await _mediator.Send(command);
        }
        catch (OperationCanceledException)
        {
            // Log that the operation was cancelled, this is not an error.
            GD.Print("Login operation was cancelled by the user.");
        }
        catch (Exception ex)
        {
            // CRITICAL: Catch all other exceptions to prevent them from being swallowed.
            // This ensures that unhandled exceptions in async operations are logged.
            GD.PrintErr($"An unhandled exception occurred during login: {ex.Message}");
            // Optionally, dispatch a command to show a generic error UI or update state.
            // Example: await _mediator.Send(new ProcessLoginResultCommand(LoginResult.Failure("Unknown error")));
        }
    }
    
    // A private implementation of the handle.
    private class AsyncOperation : IAsyncOperation
    {
        private readonly CancellationTokenSource _cts;
        public Task Task { get; }

        public AsyncOperation(Task task, CancellationTokenSource cts)
        {
            Task = task;
            _cts = cts;
        }

        void IAsyncOperation.RequestCancellation()
        {
            _cts.Cancel();
        }
    }
}
```

**4. Presenter Usage (The Clean Delegation):**
```csharp
// src/Features/Authentication/LoginPresenter.cs
using System; // For IDisposable
using Godot; // Allowed for Presenters

public class LoginPresenter : PresenterBase<ILoginView>
{
    private readonly IAuthOrchestrator _authOrchestrator;
    private IAsyncOperation _currentLoginOperation; // Store the handle
    private Button _loginButton; // Presenter can hold direct references to Godot nodes

    public LoginPresenter(ILoginView view, IAuthOrchestrator authOrchestrator) : base(view)
    {
        _authOrchestrator = authOrchestrator;
    }

    public override void Initialize()
    {
        // With Composite View Pattern, this might be View.UI.GetLoginButton() or View.LoginButton.
        _loginButton = View.GetLoginButton(); // Get node reference from the minimal View interface
        _loginButton.Pressed += HandleLoginButtonPressed; // Connect directly
        // ... other initializations
    }

    private void HandleLoginButtonPressed()
    {
        // The presenter's job is simple: delegate to the orchestrator.
        // It is not concerned with async/await or complex business logic.
        // View.ShowLoadingIndicator(); // This could be a direct call to a View method
        // Or, Presenter directly manipulates a node:
        _loginButton.Disabled = true; 
        
        // Store the handle to the operation.
        _currentLoginOperation = _authOrchestrator.BeginLoginProcess("username", "password");
    }
    
    public override void Dispose()
    {
        // When the presenter is disposed (e.g., view exits tree),
        // request cancellation of any ongoing asynchronous operation.
        _currentLoginOperation?.RequestCancellation();
        if (_loginButton != null)
        {
            _loginButton.Pressed -= HandleLoginButtonPressed; // Unsubscribe
        }
        // ... other cleanups
        base.Dispose();
    }
}
```

---

## 5. Ubiquitous Language & Core Pattern Definitions

```csharp
// ... IQuery<TResult> definition ...

// src/Core/Presentation/PresenterBase.cs
using System; // For IDisposable
using Godot; // Presenters are now Godot-aware

/// <summary>
/// Base class for all Presenters. It provides a strongly-typed reference to the View interface
/// and standard lifecycle hooks. Presenters are now Godot-aware and can directly interact with Godot Nodes.
/// </summary>
/// <typeparam name="TViewInterface">The interface of the view this presenter manages.</typeparam>
public abstract class PresenterBase<TViewInterface> : IDisposable where TViewInterface : class
{
    protected TViewInterface View { get; }

    protected PresenterBase(TViewInterface view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <summary>
    /// Called by the PresenterLifecycleManager after the presenter is created and assigned to the view.
    /// Use this for subscriptions, getting Godot node references from the View, and initial setup.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called by the PresenterLifecycleManager when the view is exiting the tree.
    /// Use this for unsubscribing from events and releasing resources.
    /// </summary>
    public virtual void Dispose() { }
}


/// <summary>
/// Defines the contract for the factory responsible for creating Presenter instances.
/// This is the bridge between Godot-managed Views and DI-managed Presenters.
/// </summary>
public interface IPresenterFactory
{
    /// <summary>
    /// Creates an instance of a Presenter, injecting its View interface dependency and any other services from the DI container.
    /// </summary>
    /// <typeparam name="TPresenter">The concrete type of the presenter to create.</typeparam>
    /// <typeparam name="TViewInterface">The interface of the view the presenter depends on.</typeparam>
    /// <param name="view">The view instance (implementing TViewInterface) that the presenter will manage.</param>
    /// <returns>A fully constructed and operational presenter instance.</returns>
    TPresenter Create<TPresenter, TViewInterface>(TViewInterface view)
        where TPresenter : PresenterBase<TViewInterface>
        where TViewInterface : class;
}

/// <summary>
/// A robust implementation of IPresenterFactory using Microsoft's DI extensions.
/// This class is the critical bridge between Godot's instantiation lifecycle (for Views)
/// and our DI container's lifecycle (for Presenters and Services).
/// It must be registered as a singleton in the DI container.
/// </summary>
public class PresenterFactory : IPresenterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PresenterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public TPresenter Create<TPresenter, TViewInterface>(TViewInterface view)
        where TPresenter : PresenterBase<TViewInterface>
        where TViewInterface : class
    {
        if (view == null)
        {
            // Fail fast if the view instance is not provided.
            throw new ArgumentNullException(nameof(view), "A valid view instance is required to create a presenter.");
        }

        // ActivatorUtilities is the key here. It allows us to create an instance of TPresenter
        // by resolving all its constructor dependencies from the IServiceProvider (_serviceProvider),
        // while simultaneously allowing us to manually provide the 'view' parameter, which is not in the DI container.
        // This elegantly solves the problem of injecting DI-managed services into a class that also depends on a
        // non-DI object (the Godot-instantiated View).
        try
        {
            return ActivatorUtilities.CreateInstance<TPresenter>(_serviceProvider, view);
        }
        catch (InvalidOperationException ex)
        {
            // This catch block is crucial for debugging DI issues.
            // If a dependency of the TPresenter is not registered in the DI container,
            // ActivatorUtilities will throw an an InvalidOperationException.
            // We wrap it in a more specific exception to provide better context.
            throw new InvalidOperationException(
                $"Failed to create presenter '{typeof(TPresenter).Name}'. " +
                $"This is likely due to a missing service registration for one of its dependencies in the DI container. " +
                $"Check the DI setup in your GameStrapper. Inner exception: {ex.Message}", ex);
        }
    }
}
```

To reinforce CQRS semantics and ensure clarity, we use custom marker interfaces that inherit from MediatR's `IRequest`.

**Marker Interface Definitions:**
```csharp
using MediatR;
using LanguageExt;
// `using static LanguageExt.Prelude;` is highly recommended for fluency in handler implementations.

// A command that changes state. Its handler MUST return Fin<Unit>.
// Unit signals a successful operation without a data payload.
public interface ICommand : IRequest<Fin<Unit>> { }

// A command that changes state and returns a "Fast-Path" result.
// Its handler MUST return Fin<TResult>.
// This pattern should be used sparingly for latency-sensitive UI feedback.
public interface ICommand<TResult> : IRequest<Fin<TResult>> { }

// A query that retrieves data. It should not modify state.
// Its handler MUST return Fin<TResult> to handle potential query failures.
public interface IQuery<TResult> : IRequest<Fin<TResult>> { }
```

| Term / Pattern | Role in Our Architecture | Purpose & Rationale |
| :--- | :--- | :--- |
| **CQRS Command**<br>*(e.g., `MoveBlockCommand`)* | **Intent Message / DTO** | A plain, immutable data object representing the **intent** to perform an action. **Must implement `ICommand` (handler returns `Fin<Unit>`)** or `ICommand<TResult>` (handler returns `Fin<TResult>`). |
| **Handler**<br>*(e.g., `MoveBlockCommandHandler`)* | **The Authoritative Model's Sole Gatekeeper** | Implements `IRequestHandler`. Its only responsibility is to receive a Command, validate business rules, **and serve as the exclusive entry point for modifying the Authoritative Write Model's state**, then enqueue resulting `IEffect`s. **Must return `Fin<Unit>` or `Fin<TResult>`** to provide a robust, explicit contract for operation outcomes. Uses function composition (`Map`, `Bind`, LINQ) to create clean, declarative business logic pipelines. |
| **Fin&lt;T&gt;** | **Operation Outcome Contract** | A `LanguageExt` type representing a final state, either success (`Succ<T>`) or failure (`Fail<Error>`). The `Error` type provides structured details about the failure. **Replaces exceptions for control flow.** Forces explicit handling of both outcomes. `Fin<Unit>` is the standard for commands that do not return data. |
| **Option&lt;T&gt;** | **Null-Safety Contract** | A `LanguageExt` type representing a value that may or may not be present (`Some<T>` or `None`). **Used to eliminate `NullReferenceException`s.** All potentially null values must be wrapped in an `Option`. |
| **Query**<br>*(e.g., `GetGridStateQuery`)* | **Data Request Message** | An object representing a request for information from the Model. **Must implement `IQuery<TResult>`** to signal it is a safe, read-only operation. |
| **Query Handler**<br>*(e.g., `GetGridStateQueryHandler`)* | **Data Tailor** | Implements `IRequestHandler`. It safely reads data from the **Authoritative Write Model** or **optimized Read Models**, and tailors it into the DTO required by the caller. **It must never cause any state change or side effect.** |
| **Service**<br>*(e.g., `IMergeService`)* | **Reusable Business Logic** | A stateless class that encapsulates a cohesive, complex business algorithm. Used by Handlers to keep them clean and focused on coordination. |
| **Effect**<br>*(e.g., `BlocksMergedEffect`)* | **Record of Fact** | A plain, immutable data object that describes a single, atomic state change that **has already occurred**. Effects are the output of a Handler, destined for the SimulationManager. |
| **DTO (Data Transfer Object)**<br>*(e.g., `GridSnapshotDto`)* | **Data Contract for Boundaries** | A pure data object used to transfer information across architectural boundaries. **Must be suffixed with `Dto` or `Snapshot`.** |
| **Presenter**<br>*(e.g., `PlayerSkillPresenter`)* | **Humble Coordinator** | A business-stateless mediator that is Godot-aware. It translates user input and system notifications into actions. Its primary role is to **coordinate** by delegating tasks to Controller Nodes (for presentation) **via a composite View interface** and sending Commands/Queries to the Model (for logic), as defined by the **Humble Presenter Principle (Rule #16)**. |
| **Controller Node**<br>*(e.g., `AnimationController`)* | **Specialized Presentation Executor** | A Godot `Node` (with a script) that encapsulates the "how" of a specific presentation task (e.g., managing `AnimationPlayer`, handling VFX, playing sounds). It implements a specific sub-interface (e.g., `IAnimationView`) and receives high-level commands from a Presenter **indirectly through the main View's composite interface**. |
| **Composite View Pattern** | **Structured View Composition** | A pattern where a main View interface (e.g., `IPlayerSkillView`) is composed of properties that return other, more specialized sub-interfaces (e.g., `IAnimationView`). The main View implementation then exposes its child Controller Nodes via these properties. This pattern enforces a clean separation of concerns within the presentation layer and prevents Presenters from knowing the internal structure of the View. |
| **Presenter Factory**<br>*(e.g., `IPresenterFactory`)* | **DI-to-Godot Bridge** | A DI-managed service responsible for creating Presenter instances. This allows Godot-managed objects (Views) to obtain DI-managed objects (Presenters) without using a service locator, correctly applying constructor injection. |
| **Read Model / Projection**<br>*(e.g., `InventoryReadModel`)* | **Optimized Data Cache** | A denormalized data structure optimized for a specific query. It is not authoritative and is updated by a Projector in response to notifications. Query Handlers read from these for high performance. |
| **Projector**<br>*(e.g., `InventoryProjector`)* | **Read Model Maintainer** | A specialized `INotificationHandler`. Its only job is to listen for notifications and update one or more Read Models accordingly. It bridges the gap between the Write and Read sides of the application. |
| **Async Orchestrator Service**<br>*(e.g., `IAuthOrchestrator`)* | **Complex Async Flow Manager** | A dedicated DI-managed service responsible for encapsulating and managing complex asynchronous operations (e.g., API calls, long-running tasks). It offloads `async/await` logic from Presenters and dispatches new Commands upon completion to update the Model. |
| **Temporal Coupling**<br>*(Anti-Pattern)* | **A FORBIDDEN PRACTICE** | An anti-pattern where correctness depends on the order methods are called over time. For example, requiring an `Initialize()` method to be called after `new()`. **We eliminate this by enforcing Atomic Construction via Constructor Injection.** |

## 6. Feature Development

For comprehensive guidance on implementing new features, including step-by-step instructions, functional programming patterns, rule engines, and the Humble Presenter principle with practical examples, see:


Key topics covered:
- Step-by-step feature implementation process
- From imperative to functional command handlers
- Advanced validation patterns and rule engines
- Humble Presenter principle with composite view pattern
- Practical tooling and development strategies
- Complete "Meteor Strike" implementation example

## 7. Infrastructure Setup (DI, Logging, Simulation Pipeline)

-   **DI Container**: `Microsoft.Extensions.DependencyInjection`. The entrypoint `GameStrapper.Initialize()` builds the container. A scene-level root node will hold a reference to a scoped provider or factory.
-   **Logging**: `Serilog`. Use constructor injection with `Serilog.ILogger`.
-   **Simulation Pipeline**:
    -   Handlers enqueue pure `IEffect` objects into `ISimulationManager`.
    -   The presentation layer calls `GameStrapper.ProcessFrame()` once per frame.
    -   `SimulationManager` converts effects into `INotification`s and publishes them via MediatR. This pipeline has an inherent one-frame delay.
    -   Presenters subscribe to these notifications and drive View updates. For immediate feedback, they use the "Fast-Path Result" from the command handler directly.
-   **Testing**:
    *   **Model Layer**: Pure C# unit tests using xUnit, Moq, Shouldly.
    *   **Presenter Layer**: Integration tests using GdUnit4Net. This is a crucial investment to compensate for the loss of unit testability in Presenters.

## 8. Complex Rule Engine

For comprehensive implementation guidance on the Complex Rule Engine Architecture, including anchor-based pattern matching, conflict resolution, and designer-friendly rule definitions, see:

**👉 [Complex Rule Engine Implementation Guide](../5_Architecture_Decision_Records/Complex_Rule_Engine_Implementation_Guide.md)**

Key topics covered:
- Anchor-based pattern matching system for optimal performance
- Priority-based conflict resolution algorithms
- Declarative pattern builders and visual pattern definitions
- Rule evaluation engine interfaces and implementation
- Integration with CQRS command processing
- Performance characteristics and benchmarks
- Rule validation and correctness verification
- Project structure for rule engine components

For the architectural decision rationale behind this approach, see the [Complex Rule Engine Architecture ADR](../5_Architecture_Decision_Records/Complex_Rule_Engine_Architecture.md).

## 9. Lessons Learned from F1 Implementation (2025-08-13)

The F1 Block Placement vertical slice implementation served as a critical validation of our architectural principles. The following lessons emerged from both the successful implementation and the bugs encountered during development.

### 9.1 Architectural Validations - What Worked

**✅ Clean Architecture Enforcement**
- The compiler-enforced boundary between `src/` (pure C#) and Godot presentation worked perfectly
- Zero instances of Godot types leaking into the Model layer
- Business logic remained completely testable without Godot engine

**✅ CQRS with Functional Programming**
- The `Fin<T>` monad eliminated runtime exceptions in business logic entirely
- Command/Handler pattern with MediatR provided clear, traceable data flow
- Functional composition in handlers (using LINQ expressions) created readable, maintainable code

**✅ MVP Pattern with Humble Presenters**
- GridPresenter successfully coordinated between pure Model and Godot Views
- View interfaces (IGridView) effectively decoupled presentation concerns
- Presenter lifecycle management worked seamlessly with Godot's Node lifecycle

**✅ Dependency Injection Integration**
- DI container integration with Godot worked without issues once properly configured
- Constructor injection eliminated temporal coupling throughout the system
- Service registration patterns proved robust and scalable

### 9.2 Critical Issues Encountered and Resolutions

**🚨 Type System Violations (LanguageExt.Fin<T> Ambiguity)**
- **Issue**: Custom wrapper around LanguageExt.Fin<T> created compiler ambiguity
- **Root Cause**: Attempted to "improve" third-party types instead of using them directly
- **Resolution**: Removed custom wrapper, used LanguageExt.Fin<T> directly
- **Architecture Principle**: Don't wrap third-party types unless providing clear value

**🚨 DI Registration Anti-Pattern (Presenter Double-Registration)**
- **Issue**: Presenters registered both manually in DI and auto-registered by MediatR
- **Root Cause**: Misunderstanding of which types should be in DI container
- **Resolution**: Use factory pattern for types with runtime dependencies
- **Architecture Principle**: Not everything belongs in the DI container

**🚨 Error Handling Inconsistency**
- **Issue**: Mixed use of Error.New() with different parameter patterns
- **Root Cause**: Lack of standardized error creation patterns
- **Resolution**: Standardized on single-parameter Error.New() format
- **Architecture Principle**: Establish and document error handling patterns early

**🚨 Validation Bypass Anti-Pattern**
- **Issue**: Handler bypassed validation rules and went directly to services
- **Root Cause**: Convenience over correctness in implementation
- **Resolution**: All handlers must use validation rules before state changes
- **Architecture Principle**: Always validate commands before execution

### 9.3 Updated Golden Rules Based on F1 Experience

**Rule 17: Use Third-Party Types Directly**
- Never create wrapper types that conflict with library types
- Use LanguageExt.Fin<T>, Option<T> directly without custom wrappers
- Favor composition over inheritance for external library integration

**Rule 18: Factory Pattern for Runtime Dependencies**
- Don't register types in DI that have unresolvable dependencies
- Use factory pattern for Presenters and other types needing runtime context
- Keep clear separation between compile-time and runtime dependency resolution

**Rule 19: Standardize Error Handling Early**
- Establish error creation patterns before implementation begins
- Use consistent Error.New() format throughout codebase
- Consider error factory methods for commonly used error types

**Rule 20: Functional Error Handling Consistency (ADR-006)**
- Use TaskFinExtensions for Task<T> → Fin<T> conversion in async operations
- Eliminate try-catch blocks in command handlers in favor of functional composition
- Maintain railway-oriented programming throughout async pipelines
- Architecture tests must enforce functional consistency to prevent regression

**Rule 21: Validation-First Command Handling**
- All command handlers MUST validate before executing state changes
- Use validation rules, not direct service calls for business rule enforcement
- Consider pipeline behaviors for automatic validation application

### 9.4 Test Strategy Validations

**✅ Test-Driven Development Success**
- 30 tests provided comprehensive coverage and caught integration issues
- Unit tests for pure business logic worked perfectly
- Integration tests with mocked dependencies validated handler logic

**✅ Functional Programming Testability**
- `Fin<T>` monadic operations were easily testable
- Functional composition in handlers made test scenarios clear
- Option<T> eliminated null reference testing complexity

**🔄 Integration Testing Needs**
- Need more integration tests for full command flow
- Consider adding architecture fitness tests to prevent violations
- ✅ **Property-based testing implemented** - Provides mathematical proofs of architectural invariants

### 9.5 Performance and Scalability Insights

**✅ CQRS Performance**
- Command/Handler pattern added minimal overhead
- MediatR pipeline behaviors (logging) performed well
- No noticeable impact from functional programming constructs

**🔄 Grid State Management**
- In-memory grid state worked well for F1 scale
- Consider spatial indexing for larger grids in future
- State synchronization between Model and View was efficient

### 9.6 Developer Experience Lessons

**✅ Architecture Pays Off**
- Initial overhead of multiple files/patterns was quickly justified
- Bug isolation was dramatically easier with clear boundaries
- Code review and understanding was enhanced by consistent patterns

**🔄 Tooling Needs Identified**
- Need templates for command/handler/test generation
- IDE support for navigating between related files would help
- Consider architecture validation tools

### 9.7 Actionable Improvements for Future Implementation

1. **~~Error Handling~~**: ✅ **COMPLETED** - Functional error handling via ADR-006 (see [Standard Patterns](Standard_Patterns.md#2-functional-error-handling-pattern-))
2. **Validation Pipeline**: Implement MediatR pipeline behavior for automatic validation  
3. **~~Architecture Tests~~**: ✅ **COMPLETED** - Functional consistency enforcement
4. **Developer Tooling**: Create templates based on proven patterns
5. **~~Documentation~~**: ✅ **COMPLETED** - Patterns documented in Standard Patterns guide

### 9.8 Success Metrics and Validation

**Quantitative Success**:
- 30 tests passing (100% business logic coverage)
- Zero runtime exceptions in business logic
- Zero architecture boundary violations
- Full feature implementation in ~5 hours (including bug fixes)

**Qualitative Success**:
- Code is readable and maintainable
- Business logic is completely decoupled from presentation
- Error handling is predictable and functional
- Architecture principles are validated through working implementation

**Confidence Level**: The F1 implementation proves our architectural decisions are sound and ready for scaling to more complex features like F2 Move Block functionality.

## 10. Testing Strategy

For comprehensive guidance on the Four-Pillar Testing Strategy, including property-based testing with FsCheck, architectural fitness tests, and mathematical validation of domain invariants, see:

**👉 [Test Guide](Test_Guide.md)**

Key topics covered:
- Four-pillar testing strategy (Unit, Command, Query, Property-Based)
- Property-based testing with FsCheck for mathematical validation
- Custom generators for domain-specific test data
- Architectural fitness tests and compliance validation
- Current test statistics and coverage metrics
- Integration with Clean Architecture principles
- Property testing roadmap and anti-patterns

The Test Guide provides detailed implementation guidance for maintaining high-quality, architecturally-compliant test coverage across all layers of the application.