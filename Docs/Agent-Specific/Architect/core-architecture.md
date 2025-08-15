# Core Architecture Overview

*This content was delegated from CLAUDE.md to the Architect agent.*

## Architecture Overview

This project implements a rigorous 3-layer architecture with compiler-enforced boundaries and **production-ready patterns validated through stress testing**:

### Project Structure
- **`BlockLife.Core.csproj`** (`src/` folder): Pure C# business logic, absolutely NO Godot dependencies
- **`BlockLife.csproj`** (root): Godot-aware presentation layer, references Core project  
- **`BlockLife.Core.Tests.csproj`** (`tests/` folder): Unit tests for core logic

### Key Architectural Principles

1. **Clean Architecture Enforcement**: Model layer (in `src/`) MUST NOT use `using Godot;`
2. **CQRS Pattern**: All state changes via Commands/Handlers, all reads via Queries
3. **Functional Programming**: Uses `LanguageExt.Fin<T>` and `Option<T>` for error handling and null safety
4. **MVP with Humble Presenters**: Presenters coordinate between pure Model and Godot Views
5. **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection throughout
6. **🔥 CRITICAL: Single Source of Truth**: One entity = One implementation = One DI registration (learned from F1 stress test)

### Critical Patterns

**Command/Query Flow:**
```
User Input → Presenter → Command → Handler → Effect → SimulationManager → Notification → Presenter → View Update
```

**Presenter Lifecycle:** 
All Views implement `IPresenterContainer<T>` and use `SceneRoot.Instance.CreatePresenterFor(this)` for automatic presenter creation/disposal.

**View Interfaces:** 
Presenters depend on interfaces (e.g., `IGridView`) that expose capabilities, not concrete Godot nodes.

**🔥 CRITICAL State Management Pattern:**
```csharp
// ✅ CORRECT - Single service implementing multiple interfaces
services.AddSingleton<GridStateService>();
services.AddSingleton<IGridStateService>(provider => provider.GetRequiredService<GridStateService>());
services.AddSingleton<IBlockRepository>(provider => provider.GetRequiredService<GridStateService>());

// ❌ WRONG - Dual state sources = race conditions
services.AddSingleton<IGridStateService, GridStateService>();
services.AddSingleton<IBlockRepository, InMemoryBlockRepository>();
```

**🔥 CRITICAL Notification Pattern:**
Presenters MUST subscribe to domain notifications in `Initialize()` and unsubscribe in `Dispose()`:
```csharp
public override void Initialize()
{
    BlockPlacementNotificationBridge.BlockPlacedEvent += OnBlockPlacedNotification;
}

public override void Dispose()
{
    BlockPlacementNotificationBridge.BlockPlacedEvent -= OnBlockPlacedNotification;
    base.Dispose();
}
```

## File Organization

### Core Logic (`src/` - BlockLife.Core project)
- `Core/`: Foundation classes (GameStrapper, Infrastructure)
- `Features/`: Feature slices organized by domain (e.g., `Features/Block/Move/`)
- Each feature contains: Commands, Handlers, DTOs, View Interfaces, Presenters

### Godot Integration (`godot_project/` and root)
- `godot_project/features/`: Godot scene files and View implementations  
- `godot_project/infrastructure/`: Logging and debug systems
- `godot_project/scenes/`: Main scenes including SceneRoot
- Root level: Godot-specific C# scripts that implement View interfaces

### Testing
- `tests/`: Comprehensive test suite with four pillars:
  - **Unit Tests**: Example-based validation of business logic
  - **Property Tests**: Mathematical proofs using FsCheck.Xunit
  - **Architecture Tests**: Automated architectural constraint enforcement
  - **Integration Tests**: GdUnit4 for Godot-specific testing

## 🔑 Critical Architecture Principle

**Single Service Container Rule**: Integration tests MUST use the exact same service instances as production SceneRoot. Any test infrastructure creating parallel containers will cause:
- Commands succeed but don't affect visible state
- Notifications fire but don't match actual state  
- "Blocks carryover between tests" symptoms
- Complete state isolation leading to phantom failures

## Important Constraints

- Model layer (`src/`) MUST be Godot-agnostic for testability and portability
- All state changes MUST go through Command Handlers 
- Presenters MUST NOT hold authoritative game state
- Use constructor injection for all dependencies
- Follow the established folder structure and naming conventions

### Error Handling
- Use `Fin<T>` for operations that can fail
- Use `Option<T>` instead of nullable references  
- Structured errors with `Error.New(code, message)`

### Unified Logging System
- **Core Layer**: Uses `ILogger` from DI container
- **View Layer**: Access via `GetNode<SceneRoot>("/root/SceneRoot").Logger?.ForContext("SourceContext", "UI")`
- **Structured Logging**: Use proper parameters `logger.Information("Message with {Param}", value)`
- **Categories**: "Commands", "Queries", "UI", "Core", "DI"
- **NEVER use GD.Print()**: Always use structured logger for consistency
- **LogSettings**: Simple boolean flags (`VerboseCommands`, `VerboseQueries`) instead of complex arrays

### Dependency Injection
- All services registered in GameStrapper
- Views get Presenters via PresenterFactory pattern
- Logger exposed via SceneRoot.Logger for View access
- NO static service locators in application code

### Key Dependencies
- **LanguageExt.Core**: Functional programming primitives
- **MediatR**: Command/Query handling
- **Serilog**: Logging framework
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **FsCheck.Xunit**: Property-based testing framework
- **GdUnit4**: Godot testing framework

### Autoload Configuration (CRITICAL)
- **SceneRoot**: Configured as autoload singleton at `/root/SceneRoot`
- **Location**: `godot_project/scenes/Main/SceneRoot.tscn` (NOT the .cs file!)
- **Contains**: SceneRoot node with unified logging system
- **Access Patterns**: 
  - Presenters: `GetNode<SceneRoot>("/root/SceneRoot").PresenterFactory`
  - Logging: `GetNode<SceneRoot>("/root/SceneRoot").Logger?.ForContext("SourceContext", "UI")`
- **WARNING**: Never attach SceneRoot script to other scenes (causes duplicate instantiation)

The architecture prioritizes long-term maintainability and testability over rapid prototyping convenience.