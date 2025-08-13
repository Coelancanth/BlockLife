# Implementation Plan: Advanced Logger & GameStrapper

This document outlines the implementation plan for the foundational `GameStrapper` and a new, advanced `LoggerManager`. This system will provide a robust, dynamically configurable logging service that supports rich text formatting in the Godot console and allows for live log level/category toggling directly from the Godot Editor's Inspector, as requested.

## 1. High-Level Explanation (The 'Why')

#### What problem does this plan aim to solve? (The Pain Point)
The core objective of this implementation plan is to address two fundamental yet critical issues:
1.  **Unreliable Bootstrapping:** Without a centralized, verifiable bootstrapper, the configuration of the Dependency Injection (DI) container, the initialization of the logging system, and the startup of other core services can become fragmented, chaotic, and difficult to test. This leads to unexpected runtime crashes due to configuration errors and makes troubleshooting difficult when issues arise.
2.  **Opaque Operation Flow:** As system complexity increases, it becomes challenging to know which commands and queries are being executed, whether they succeed or fail, and how they perform. Relying on developers to manually add logging to each handler is unreliable and boilerplate-heavy, ultimately leading to inconsistent or forgotten logging.

#### Why are we using this specific approach? (The Rationale)
Our chosen solution is based on several key software engineering principles:
1.  **Separation of Concerns:** `SceneRoot` (the Godot world) is strictly responsible for collecting *configuration*, while `GameStrapper` (the pure C# world) is responsible for *building* services based on this configuration. This creates a clear boundary that aligns perfectly with our architectural rules (Godot code should not pollute core logic).
2.  **Aspect-Oriented Programming (AOP):** MediatR's `IPipelineBehavior` is a perfect application of AOP. We use it to decouple logging, a "cross-cutting concern," from the business logic. This keeps our handlers clean, focusing solely on their core responsibilities, while ensuring mandatory and consistent logging for all operations.
3.  **Configuration as Data:** Using Godot's `Resource` (`LogSettings.cs`) to drive log configuration allows non-programmers (e.g., designers, QA) to adjust log levels directly in the editor without modifying code or recompiling. This embodies the data-driven design philosophy.
4.  **Defensive Programming:** Enabling `ValidateOnBuild` and `ValidateScopes` in `GameStrapper`, along with adding dedicated DI tests in Phase 7, are practices of defensive programming. We don't just "hope" the DI container is configured correctly; we *forcefully verify* its correctness at startup and during testing through automated means.

#### What would happen if we didn't do this? (The Consequences / The "Bad" Way)
If we adopted a simpler, more primitive approach:
*   **Logging System:** Log configuration would be hardcoded. Every time a log level needed adjustment (e.g., enabling Verbose for debugging a specific module), it would require code modification and recompilation, which is inefficient. Log formats would be inconsistent, and critical failure information might be lost.
*   **Dependency Injection:** DI container setup would be scattered, or objects would be manually `new`ed. This would lead to tight coupling, making unit testing impossible or extremely difficult. When "service cannot be resolved" errors occur, you would only discover them at runtime, not during startup or testing.
*   **Operation Tracking:** You would have to rely on `GD.Print` or manually inject and use `ILogger` in every handler. This would generate a lot of duplicate code, and if someone forgets to add a log, you lose a part of the system's observability.

#### What are the drawbacks or trade-offs of this approach? (The Pragmatic View)
We must acknowledge that this approach introduces higher initial complexity:
*   **Learning Curve:** Team members need to understand Serilog, advanced options of `Microsoft.Extensions.DependencyInjection`, and the concept of MediatR Pipelines.
*   **Startup Overhead:** `ValidateOnBuild` will slightly increase the application's initial startup time as it performs a comprehensive check when building the service provider. This is a conscious trade-off we accept, exchanging a minor startup performance cost for significant runtime stability and development confidence.
*   **Abstraction Layers:** We introduce abstraction layers like `GameStrapper` and `PresenterFactory`. While crucial for decoupling, for someone new to the project, understanding the full flow from a `SceneRoot` signal to the eventual execution of a `CommandHandler` will take more time.

## 2. Solution Overview (The 'What' and 'How')

#### Clear Overview
This plan aims to build a robust, Godot editor-integrated infrastructure layer. It consists of two main parts:
1.  **Dynamic Logging System:** Uses Serilog as the backend, with configuration driven by a Godot `Resource` file. It supports categorized, level-based logging and can output formatted rich text logs to both the Godot console and an in-game `RichTextLabel`.
2.  **Validated Bootstrapper:** A static class named `GameStrapper` will serve as the "heart" of the application, responsible for initializing and configuring a strictly validated DI container. The `SceneRoot` node, upon starting in the Godot scene tree, will call `GameStrapper`, passing editor configurations to it, thereby connecting the Godot world with our pure C# core logic world.

Additionally, this plan automatically logs all Command and Query processing via the MediatR Pipeline and defensively protects the DI container's configuration through dedicated unit tests.

#### Development Steps
The plan's seven phases are logically clear, covering the entire process from dependency configuration to final test validation:
1.  **Phase 1: Dependencies & Configuration Resource.** Establishes the foundation.
2.  **Phase 2: Custom Rich Text Sink.** Implements the in-game log viewer.
3.  **Phase 3 & 5 (Merged): `GameStrapper` & `SceneRoot` Integration.** This is the core architectural connection point, combining configuration with implementation.
4.  **Phase 4: MediatR Logging Behavior.** Implements transparency for operation flow.
5.  **Phase 6: Workflow Documentation.** Ensures team usability.
6.  **Phase 7: DI Container Validation Testing.** This is the defense for the architecture, perfectly aligning with the testing guide's philosophy.

#### Assumptions and Limitations
*   **Tech Stack:** The solution heavily relies on `Serilog`, `Microsoft.Extensions.DependencyInjection`, and `MediatR`.
*   **Architectural Boundaries:** The solution strictly adheres to architectural boundaries. Only `SceneRoot`, `LogSettings`, and `RichTextLabelSink` are aware of the Godot API. `GameStrapper` and `LoggingBehavior` reside in the pure C# core layer.
*   **Performance:** The performance of `RichTextLabelSink` directly impacts the performance of the in-game debugging tool. Its implementation must be very careful.

---

## Phase 1: Dependencies & Configuration Resource

**Goal:** Establish the necessary packages and create a data-driven `Resource` to control logger behavior from the Godot editor.

1.  **Add NuGet Packages:**
    *   In `src/BlockLife.Core.csproj`, ensure `Serilog` is referenced.
    *   In `godot_project/BlockLife.Godot.csproj`, add `Serilog.Sinks.Godot` which provides a basic Godot console sink.

2.  **Define Log Categories (`src/Core/Infrastructure/Logging/`):**
    *   Create a static class `LogCategory.cs` to provide well-defined, constant strings for log source contexts. This prevents typos and allows for easy discovery of available categories.
    ```csharp
    // src/Core/Infrastructure/Logging/LogCategory.cs
    public static class LogCategory
    {
        public const string Core = "Core";
        public const string DI = "DependencyInjection";
        public const string Input = "Input";
        public const string GameState = "GameState";
        public const string AI = "AI";
        public const string Physics = "Physics";
        public const string Rendering = "Rendering";
    }
    ```

3.  **Create `LogSettings` Resource (`godot_project/resources/`):**
    *   Create a new C# script `LogSettings.cs` that inherits from `Godot.Resource`. This will be the data object for our configuration.
    *   Expose properties to the Godot Inspector using the `[Export]` attribute.
    ```csharp
    // godot_project/resources/LogSettings.cs
    using Godot;
    using System.Collections.Generic;
    using Serilog.Events;

    public partial class LogSettings : Resource
    {
        [Export]
        public LogEventLevel DefaultLogLevel { get; set; } = LogEventLevel.Information;

        [Export]
        public bool EnableRichTextInGodot { get; set; } = true;

        [Export]
        public Godot.Collections.Dictionary<string, LogEventLevel> CategoryLogLevels { get; set; } = new();
    }
    ```

## Phase 2: Custom Rich Text Godot Sink

**Goal:** Create a custom Serilog sink capable of writing formatted BBCode to a Godot `RichTextLabel` node for live, in-game log viewing, with performance considerations.

1.  **`RichTextLabelSink` Implementation (`godot_project/infrastructure/logging/`):**
    *   Create `RichTextLabelSink.cs`. It will implement Serilog's `ILogEventSink`.
    *   The constructor will take a `RichTextLabel` node and configuration for rich text and max lines.
    *   The `Emit` method will format the `LogEvent` into a BBCode string and append it to the `RichTextLabel` using `CallDeferred` to ensure thread safety and UI updates on the main thread.
    *   It will manage a queue of log lines to enforce a `MaxLines` limit, removing the oldest lines when the limit is exceeded to prevent performance degradation.

    ```csharp
    // godot_project/infrastructure/logging/RichTextLabelSink.cs
    using Godot;
    using Serilog.Core;
    using Serilog.Events;
    using System.Collections.Generic; // This using is no longer strictly needed for _logQueue, but kept for consistency if other list types are used.
    using System.Text;

    public class RichTextLabelSink : ILogEventSink
    {
        private readonly RichTextLabel _richTextLabel;
        private readonly bool _enableRichText;
        private readonly int _maxLines;
        // private readonly Queue<string> _logQueue = new(); // Removed as RichTextLabel manages its own lines

        public RichTextLabelSink(RichTextLabel richTextLabel, bool enableRichText, int maxLines = 1000)
        {
            _richTextLabel = richTextLabel;
            _enableRichText = enableRichText;
            _maxLines = maxLines;
            _richTextLabel.BbcodeEnabled = true; // Ensure BBCode is enabled for rich text formatting
        }

        public void Emit(LogEvent logEvent)
        {
            var message = _enableRichText 
                ? FormatRichText(logEvent) 
                : logEvent.RenderMessage();

            // This must be deferred to the main thread to be safe.
            _richTextLabel.CallDeferred(nameof(AppendLog), message);
        }

        // This method will be called by Godot on the main thread.
        private void AppendLog(string message)
        {
            if (!GodotObject.IsInstanceValid(_richTextLabel)) return;

            // ARCHITECT'S NOTE: High-performance implementation.
            // Avoids rebuilding the entire text on each log by using Godot's built-in line management.
            if (_richTextLabel.GetLineCount() >= _maxLines)
            {
                // This removes the first line, effectively acting as a circular buffer.
                _richTextLabel.RemoveLine(0);
            }

            // AppendText handles adding a newline automatically if the label's text isn't empty.
            if (_richTextLabel.Text.Length > 0)
            {
                _richTextLabel.AppendText("\n" + message);
            }
            else
            {
                _richTextLabel.AppendText(message);
            }
            
            // Scroll to the end after the UI has had a chance to update.
            // CallDeferred is used here to ensure the scroll happens after the text update is processed.
            _richTextLabel.CallDeferred("scroll_to_line", _richTextLabel.GetLineCount() - 1);
        }

        private string FormatRichText(LogEvent logEvent)
        {
            var color = logEvent.Level switch
            {
                LogEventLevel.Verbose => "gray",
                LogEventLevel.Debug => "lightblue",
                LogEventLevel.Information => "white",
                LogEventLevel.Warning => "yellow",
                LogEventLevel.Error => "orangered",
                LogEventLevel.Fatal => "red",
                _ => "white"
            };
            
            logEvent.Properties.TryGetValue("SourceContext", out var sourceContext);
            var context = sourceContext?.ToString().Trim('"') ?? "Default";
            
            return $"[color={color}][{logEvent.Level.ToString().ToUpper()}] [{context}] {logEvent.RenderMessage()}[/color]";
        }
    }
    ```

## Phase 3: Dynamic Logger Configuration (Integrated into GameStrapper)

**Goal:** Redesign the logger configuration to be dynamically built based on the `LogSettings` resource directly within the `GameStrapper`, removing the need for a separate `LoggerManager` class. This phase's content is now integrated into Phase 5.

## Phase 4: MediatR Logging Behavior

**Goal:** Automatically log every command and query that flows through MediatR, including their results and execution time, without adding boilerplate to individual handlers.

1.  **`LoggingBehavior` Implementation (`src/Core/Application/Behaviors/`):**
    *   Create a new directory `Behaviors` for the pipeline component.
    *   Create `LoggingBehavior.cs`. It will be a generic class `LoggingBehavior<TRequest, TResponse>` that implements `IPipelineBehavior<TRequest, TResponse>`.
    *   It will inject `ILogger`.
    *   The `Handle` method will:
        1.  Log an informational message before processing.
        2.  Measure the execution time of the request.
        3.  Call `await next()` to pass the request to the next behavior or the handler.
        4.  Inspect the `TResponse` (which will be a `Fin<T>`).
        5.  Use `response.Match` to log success or failure appropriately, including execution time and detailed error information (code, message, and exception if present) on failure.

    ```csharp
    // src/Core/Application/Behaviors/LoggingBehavior.cs
    using MediatR;
    using Serilog;
    using LanguageExt;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TResponse : ISuccessFail // Assuming ISuccessFail is the base interface for Fin<T>
    {
        private readonly ILogger _logger;

        public LoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.Information("Handling {RequestName}...", requestName);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            response.Match(
                Succ: _ => {
                    _logger.Information("{RequestName} handled successfully in {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);
                },
                Fail: error => {
                    if (error.Exception.IsSome)
                    {
                        _logger.Error(error.Exception.Value, "Failed to handle {RequestName} in {ElapsedMilliseconds}ms. Code: {ErrorCode}, Message: {ErrorMessage}", 
                            requestName, stopwatch.ElapsedMilliseconds, error.Code, error.Message);
                    }
                    else
                    {
                        _logger.Warning("Failed to handle {RequestName} in {ElapsedMilliseconds}ms. Code: {ErrorCode}, Message: {ErrorMessage}", 
                            requestName, stopwatch.ElapsedMilliseconds, error.Code, error.Message);
                    }
                }
            );

            return response;
        }
    }
    ```

## Phase 5: `GameStrapper` & `SceneRoot` Integration

**Goal:** Connect the Godot world (where the configuration lives) to the pure C# world (where the logger is built and used), and register the MediatR logging pipeline, ensuring clear separation of concerns.

1.  **`GameStrapper` Update (`src/Core/`):**
    *   The `Initialize` method signature will change to accept the raw configuration materials (`LogSettings` and optional `ILogEventSink`).
    *   It will contain a private method `ConfigureAndCreateLogger` responsible for building the `ILogger` instance based on the provided settings and sink.
    *   Inside `Initialize`, the created `ILogger` instance will be registered as a singleton.
    *   The MediatR logging behavior will be registered as a transient pipeline behavior *before* MediatR itself.

    ```csharp
    // src/Core/GameStrapper.cs
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Core; // For ILogEventSink
    using System;
    using MediatR;

    public static class GameStrapper
    {
        public static IServiceProvider Initialize(LogSettings settings, ILogEventSink richTextSink = null)
        {
            var services = new ServiceCollection();
            
            // --- Logger Configuration ---
            var logger = ConfigureAndCreateLogger(settings, richTextSink);
            services.AddSingleton<ILogger>(logger);

            // --- MediatR Pipeline Behaviors ---
            // The logging behavior is registered first, so it wraps everything else.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // --- MediatR Registration ---
            // Assuming your commands/handlers are in the same assembly as GameStrapper
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GameStrapper).Assembly));
            
            // --- Register Other Services ---
            // services.AddSingleton<IPlayerStateService, PlayerStateService>();
            // ...
            
            // --- Presenter Factory ---
            services.AddSingleton<IPresenterFactory, PresenterFactory>();

            // --- Build Provider with Validation ---
            // This is critical for catching DI errors early.
            // ValidateOnBuild ensures all dependencies can be resolved at startup.
            // ValidateScopes prevents incorrect service lifetimes (e.g., singleton depending on transient).
            return services.BuildServiceProvider(new ServiceProviderOptions 
            { 
                ValidateOnBuild = true, 
                ValidateScopes = true 
            });
        }

        private static ILogger ConfigureAndCreateLogger(LogSettings settings, ILogEventSink richTextSink)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(settings.DefaultLogLevel)
                .WriteTo.Godot(applyTheme: settings.EnableRichTextInGodot); 

            foreach (var (category, level) in settings.CategoryLogLevels)
            {
                loggerConfig.MinimumLevel.Override(category, level);
            }

            if (richTextSink != null)
            {
                loggerConfig.WriteTo.Sink(richTextSink);
            }

            // Add other sinks like File for production builds
            // #if !DEBUG
            // loggerConfig.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);
            // #endif

            return loggerConfig.CreateLogger();
        }
    }
    ```

2.  **`SceneRoot` Integration (`godot_project/scenes/Main/`):**
    *   The `SceneRoot.cs` script becomes the central point of configuration.
    *   **Exported Properties:**
        *   `[Export] private LogSettings _logSettings;`
        *   `[Export] private NodePath _richTextLogLabelPath;` (Optional path to a `RichTextLabel` node in the scene).
    *   **`_EnterTree()` Logic:**
        1.  **Enforce Singleton Pattern:** The first action is to check if another `SceneRoot` already exists. This prevents multiple DI containers and ensures runtime stability.
        2.  Validate that `_logSettings` has been assigned. If not, log a fatal error and quit.
        3.  Check if the `_richTextLogLabelPath` is set. If it is, retrieve the `RichTextLabel` node and create an instance of the `RichTextLabelSink`.
        4.  Call `GameStrapper.Initialize(_logSettings, richTextSink)` to build the DI container with the fully configured logger.
        5.  Proceed with retrieving the `IPresenterFactory` and other services from the DI container.
        6.  Log a confirmation message that initialization is complete.

    ```csharp
    // godot_project/scenes/SceneRoot.cs
    using Godot;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;

    public partial class SceneRoot : Node
    {
        public static SceneRoot Instance { get; private set; }
        public IPresenterFactory PresenterFactory { get; private set; }
        private IServiceProvider _serviceProvider;

        [Export]
        private LogSettings _logSettings;
        [Export]
        private NodePath _richTextLogLabelPath;

        public override void _EnterTree()
        {
            // --- 1. Enforce Singleton & Prevent Multiple DI Containers ---
            if (Instance != null) 
            {
                GD.PrintErr("FATAL: A second SceneRoot was instantiated. Destroying self to prevent multiple DI containers.");
                QueueFree(); 
                return; 
            }
            Instance = this;

            // --- 2. Validate Configuration ---
            if (_logSettings == null)
            {
                GD.PrintErr("FATAL: LogSettings resource not assigned to SceneRoot. Application cannot start.");
                GetTree().Quit();
                return;
            }

            // --- 3. Prepare Godot-specific Inputs ---
            RichTextLabelSink richTextSink = null;
            if (_richTextLogLabelPath != null && !_richTextLogLabelPath.IsEmpty)
            {
                if (GetNode(_richTextLogLabelPath) is RichTextLabel label)
                {
                    richTextSink = new RichTextLabelSink(label, _logSettings.EnableRichTextInGodot);
                }
                else
                {
                    GD.PrintErr($"SceneRoot: Node at path '{_richTextLogLabelPath}' is not a RichTextLabel. In-game console disabled.");
                }
            }
            
            // --- 4. Initialize DI Container ---
            _serviceProvider = GameStrapper.Initialize(_logSettings, richTextSink);
            
            // --- 5. Retrieve Services from DI ---
            PresenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
            
            var logger = _serviceProvider.GetRequiredService<ILogger>();
            logger.ForContext("SourceContext", LogCategory.Core).Information("SceneRoot initialized and DI container is ready.");
        }

        // ... other methods from the main guide ...
    }
    ```

## Phase 6: Usage and Workflow

**Goal:** Document the end-to-end workflow for developers.

1.  **In-Editor Configuration:**
    *   A developer creates a `LogSettings` resource in the Godot FileSystem dock (e.g., `res://log_settings.tres`).
    *   They assign this resource to the `Log Settings` property of the `SceneRoot` node in the main scene.
    *   They can now edit the `log_settings.tres` resource directly in the Inspector, changing the default log level and adding category-specific overrides. These changes take effect on the next game run.

2.  **In-Code Usage:**
    *   Any service, handler, or presenter that needs to log will inject `Serilog.ILogger` via its constructor.
    *   To write a categorized log, the developer uses the `ForContext` method:
    ```csharp
    public class MyAIService
    {
        private readonly ILogger _logger;
        public MyAIService(ILogger logger)
        {
            // Create a context-specific logger instance
            _logger = logger.ForContext("SourceContext", LogCategory.AI);
        }

        public void MakeDecision()
        {
            // This log will be filtered based on the "AI" category's level
            _logger.Information("AI making a decision...");
        }
    }
    ```
This plan creates a powerful, flexible, and developer-friendly logging infrastructure that is fully integrated with the Godot editor, fulfilling all the specified requirements.

## Phase 7: DI Container Validation Testing

**Goal:** To programmatically verify the correctness of the Dependency Injection container setup, ensuring all services can be resolved, lifetimes are correctly configured, and the configuration is valid. These tests will be located in the `tests/BlockLife.Core.Tests` project.

1.  **Resolution Test (`DependencyResolutionTests.cs`):**
    *   **Objective:** Ensure that all registered services in the DI container can be successfully resolved.
    *   **Implementation:**
        *   A test will call `GameStrapper.Initialize()` with a mock `LogSettings` object.
        *   It will then iterate through a list of all registered service types (e.g., `IPlayerStateService`, `IPresenterFactory`, `IMediator`, all `IRequestHandler`s, all `Presenter`s).
        *   For each service type, it will call `serviceProvider.GetRequiredService<TService>()`.
        *   The test will pass if no exceptions are thrown, confirming that all services and their dependencies are correctly registered.

    ```csharp
    // tests/BlockLife.Core.Tests/Infrastructure/DependencyInjection/DependencyResolutionTests.cs
    using System;
    using System.Linq;
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class DependencyResolutionTests
    {
        [Fact]
        public void All_Registered_Services_Following_Patterns_Should_Be_Resolvable()
        {
            // Arrange
            // Architect's Note: We are testing the REAL GameStrapper's configuration.
            // We provide a dummy LogSettings, but the service registration logic is the one from production.
            var serviceProvider = GameStrapper.Initialize(new LogSettings());

            var coreAssembly = typeof(GameStrapper).Assembly;

            // #############################################################################
            // ARCHITECT'S NOTE: CRITICAL ARCHITECTURAL VALIDATION
            // This reflection query is a living document of our DI registration patterns.
            // If you introduce a new service base class, interface, or registration
            // convention (e.g., services ending with "Service"), you MUST update this
            // query. Failure to do so will create a silent gap in test coverage.
            // #############################################################################
            var servicesToTest = coreAssembly.GetTypes().Where(t =>
                !t.IsAbstract && !t.IsInterface &&
                (
                    // Pattern 1: All Presenters inherit from PresenterBase<T>
                    t.BaseType is { IsGenericType: true, GetGenericTypeDefinition: var baseTypeDef } && baseTypeDef == typeof(PresenterBase<>),
                    
                    // Pattern 2: All Command/Query/Notification Handlers
                    t.GetInterfaces().Any(i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                    )),

                    // Pattern 3: Key Singleton services registered by interface
                    typeof(IPresenterFactory).IsAssignableFrom(t)
                    // Add other key interfaces here, e.g., typeof(IPlayerStateService).IsAssignableFrom(t)
                )
            ).ToList();
            
            Assert.True(servicesToTest.Any(), "The reflection query did not find any services to test. Check the query logic and architectural patterns.");

            // Act & Assert
            foreach (var serviceType in servicesToTest)
            {
                try
                {
                    serviceProvider.GetRequiredService(serviceType);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to resolve service '{serviceType.Name}'. Check its DI registration in GameStrapper. Error: {ex.Message}");
                }
            }
        }
    }
    ```

2.  **Lifetime Test (`DependencyLifetimeTests.cs`):**
    *   **Objective:** Verify that services are resolved with the correct lifetimes (Singleton, Transient).
    *   **Implementation:**
        *   **Singleton Test:** The test will resolve a registered singleton service (e.g., `ILogger`, `IPresenterFactory`) twice from the same service provider. It will then assert that the two resolved instances are the *same object* (`Assert.Same(instance1, instance2)`).
        *   **Transient Test:** The test will resolve a registered transient service (e.g., a `Presenter` or a `LoggingBehavior`) twice. It will then assert that the two resolved instances are *different objects* (`Assert.NotSame(instance1, instance2)`).

    ```csharp
    // tests/BlockLife.Core.Tests/Infrastructure/DependencyInjection/DependencyLifetimeTests.cs
    using System;
    using System.Linq; // Added for FirstOrDefault
    using System.Reflection; // Added for reflection
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Xunit;
    using Moq; // Added for mocking

    public class DependencyLifetimeTests
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyLifetimeTests()
        {
            // Arrange: Create the real service provider once for all tests in this class.
            _serviceProvider = GameStrapper.Initialize(new LogSettings());
        }

        [Fact]
        public void Key_Singleton_Services_Should_Always_Return_Same_Instance()
        {
            // Architect's Note: We test the lifetimes of our ACTUAL services, not just dummies.
            // This prevents regressions if someone accidentally changes a registration in GameStrapper.
            // Act
            var logger1 = _serviceProvider.GetRequiredService<ILogger>();
            var logger2 = _serviceProvider.GetRequiredService<ILogger>();

            var factory1 = _serviceProvider.GetRequiredService<IPresenterFactory>();
            var factory2 = _serviceProvider.GetRequiredService<IPresenterFactory>();
            
            // Assert
            Assert.Same(logger1, logger2);
            Assert.Same(factory1, factory2);
        }

        [Fact]
        public void Presenters_Should_Always_Return_New_Instance()
        {
            // Arrange
            var presenterFactory = _serviceProvider.GetRequiredService<IPresenterFactory>();
            
            // ARCHITECT'S NOTE: Test with a REAL presenter, not a dummy.
            // This makes the test more valuable by ensuring our actual complex presenters
            // can be instantiated correctly with a transient lifetime.
            var presenterTypeToTest = typeof(GameStrapper).Assembly.GetTypes().FirstOrDefault(t =>
                !t.IsAbstract && !t.IsInterface &&
                t.BaseType is { IsGenericType: true, GetGenericTypeDefinition: var baseTypeDef } && 
                baseTypeDef == typeof(PresenterBase<>)
            );

            Assert.NotNull(presenterTypeToTest); // Fail fast if no presenters are found

            // We need the generic arguments to call the generic Create method.
            // The PresenterBase<TView> means the first generic argument is the view interface.
            var viewInterfaceType = presenterTypeToTest.BaseType.GetGenericArguments()[0];
            var createMethod = typeof(IPresenterFactory).GetMethod(nameof(IPresenterFactory.Create))
                .MakeGenericMethod(presenterTypeToTest, viewInterfaceType);

            // We need a mock/dummy view that implements the required interface.
            // For simplicity, we'll use a dynamic mock from Moq.
            var mockView = new Mock(viewInterfaceType).Object;

            // Act
            var presenter1 = createMethod.Invoke(presenterFactory, new[] { mockView });
            var presenter2 = createMethod.Invoke(presenterFactory, new[] { mockView });

            // Assert
            Assert.NotNull(presenter1);
            Assert.NotNull(presenter2);
            Assert.NotSame(presenter1, presenter2);
        }

        // Dummy classes for the transient test (kept for context, though not directly used in the improved test)
        public interface IDummyView { }
        public class DummyView : IDummyView { }
        public class DummyPresenter : PresenterBase<IDummyView>
        {
            public DummyPresenter(IDummyView view) : base(view) { }
        }
    }
    ```

3.  **Configuration Validation Test (`ConfigurationValidationTests.cs`):**
    *   **Objective:** Confirm that the `BuildServiceProvider` options correctly identify invalid configurations.
    *   **Implementation:**
        *   **Invalid Scope Test:** A test will create a temporary `ServiceCollection`, register a transient service (`TransientService`), and then register a singleton service (`SingletonService`) that depends on `TransientService`.
        *   It will then call `BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true })`.
        *   The test will assert that this call throws an `InvalidOperationException`, proving that our validation option correctly catches the invalid dependency scope (a singleton cannot depend on a transient).

    ```csharp
    // tests/BlockLife.Core.Tests/Infrastructure/DependencyInjection/ConfigurationValidationTests.cs
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    // Dummy services for validation testing
    public class DependentTransientService { }
    public class DependentSingletonService
    {
        public DependentSingletonService(DependentTransientService transientService) { }
    }

    public class ConfigurationValidationTests
    {
        [Fact]
        public void BuildServiceProvider_Should_Throw_On_Invalid_Scope_When_Validation_Enabled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<DependentTransientService>(); // Transient service
            services.AddSingleton<DependentSingletonService>();  // Singleton depending on transient

            // Act & Assert
            var options = new ServiceProviderOptions { ValidateScopes = true };
            var exception = Record.Exception(() => services.BuildServiceProvider(options));

            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Contains("Cannot consume scoped service", exception.Message); // Or similar message indicating scope issue
        }

        [Fact]
        public void BuildServiceProvider_Should_Throw_On_Unresolvable_Service_When_Validation_Enabled()
        {
            // Arrange
            var services = new ServiceCollection();
            // Register a service that depends on an unregistered service
            services.AddSingleton<DependentSingletonService>(); 

            // Act & Assert
            var options = new ServiceProviderOptions { ValidateOnBuild = true };
            var exception = Record.Exception(() => services.BuildServiceProvider(options));

            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Contains("Unable to resolve service for type", exception.Message);
        }
    }
    ```