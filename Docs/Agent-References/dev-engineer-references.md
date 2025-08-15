# Dev Engineer Agent - Documentation References
n## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation


## Your Role in TDD GREEN Phase

You are responsible for the TDD GREEN phase - writing minimal code to make failing tests pass. You work closely with Test Designer who creates the failing tests that guide your implementation.

## Shared Documentation You Should Know

### 🧠 **Living Wisdom References** (For Implementation Guidance)
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all implementation guidance
- **[LWP_004_Production_Readiness_Checklist.md](../Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)** - **READ BEFORE CODING** - Critical patterns you must follow
- **[LWP_002_Integration_Testing_Patterns.md](../Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md)** - Testing patterns that will validate your code

### Architecture Patterns for Implementation
- `Docs/Shared/Architecture/Architecture_Guide.md` - Clean Architecture boundaries you must follow
- `Docs/Shared/Architecture/Standard_Patterns.md` - Established patterns to implement
- `src/Features/Block/Move/` - Gold standard implementation to copy patterns from

### Implementation Guidance
- `Docs/Shared/Guides/Comprehensive_Development_Workflow.md` - TDD GREEN phase details
- `Docs/Shared/Guides/Quick_Reference_Development_Checklist.md` - Implementation workflow

### VSA Structure Understanding  
- `Docs/Shared/Architecture/Reference-Implementations/000_Vertical_Slice_Architecture_Plan.md` - VSA patterns (archived reference)
- `Docs/Backlog/items/VS_*.md` - Current VS items with embedded Tech Lead implementation planning
- `Docs/Agent-Specific/VSA/organization-patterns.md` - Where to put your code

### Post-Mortems for Implementation Lessons
- `Docs/Shared/Post-Mortems/Architecture_Stress_Testing_Lessons_Learned.md` - Critical production patterns
- `Docs/Shared/Post-Mortems/Critical_Architecture_Fixes_Post_Mortem.md` - Patterns that work

## Core Implementation Principles

### 1. TDD GREEN Phase Mindset
- **Minimal**: Write the least code to make tests pass
- **Simple**: Avoid over-engineering or premature optimization  
- **Focused**: Only implement what the failing test requires
- **Refactoring comes later**: Don't optimize during GREEN phase

### 2. Clean Architecture Compliance
```csharp
// ✅ CORRECT - Core project structure
namespace BlockLife.Core.Features.Block.Move
{
    // NO using Godot; statements allowed
    public record MoveBlockCommand(BlockId BlockId, GridPosition ToPosition);
    
    public class MoveBlockCommandHandler : IRequestHandler<MoveBlockCommand, Fin<Unit>>
    {
        // Use dependency injection
        // Return Fin<T> for error handling
        // Follow established patterns
    }
}
```

### 3. Dependency Patterns
```csharp
// Constructor injection pattern
public class MoveBlockCommandHandler
{
    private readonly IGridStateService _gridState;
    private readonly IMediator _mediator;
    
    public MoveBlockCommandHandler(
        IGridStateService gridState,
        IMediator mediator)
    {
        _gridState = gridState;
        _mediator = mediator;
    }
}
```

## Implementation Patterns

### Command Handler Pattern
```csharp
public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(FeatureCommand command, CancellationToken ct)
    {
        // 1. Validate input
        if (command.IsInvalid())
            return Error.New("VALIDATION_ERROR", "Description");
            
        // 2. Execute business logic
        var result = await _service.DoSomething(command.Data);
        if (result.IsFail)
            return result.Error;
            
        // 3. Publish notification
        await _mediator.Publish(new FeatureCompletedNotification(...));
        
        return Unit.Value;
    }
}
```

### Error Handling Pattern
```csharp
// Always use Fin<T> for operations that can fail
public async Task<Fin<Unit>> SomeOperation()
{
    try
    {
        // Business logic
        return Unit.Value;
    }
    catch (SpecificException ex)
    {
        return Error.New("SPECIFIC_ERROR", ex.Message);
    }
}
```

### Service Interface Pattern
```csharp
// Always program against interfaces
public interface IFeatureService
{
    Task<Fin<SomeResult>> DoSomething(SomeInput input);
}

public class FeatureService : IFeatureService
{
    public async Task<Fin<SomeResult>> DoSomething(SomeInput input)
    {
        // Implementation
    }
}
```

## VSA File Organization

### Where to Put Your Code
```
src/Features/[Domain]/[Feature]/
├── Commands/
│   └── FeatureCommand.cs
├── Handlers/
│   └── FeatureCommandHandler.cs
├── Notifications/
│   └── FeatureCompletedNotification.cs
├── Services/
│   └── IFeatureService.cs
└── ViewInterfaces/
    └── IFeatureView.cs
```

### What Goes in Each File
- **Commands**: Immutable records with validation
- **Handlers**: Business logic implementation
- **Notifications**: Events published after operations
- **Services**: Complex business logic extraction
- **ViewInterfaces**: Presenter dependencies

## Common Implementation Tasks

### 1. Implementing Command Handlers
- Follow the handler pattern exactly
- Use dependency injection
- Return `Fin<Unit>` or `Fin<T>`
- Publish notifications on success

### 2. Creating Services
- Always create interface first
- Register in DI container
- Follow single responsibility principle
- Return `Fin<T>` for error handling

### 3. Implementing Presenters
- Inherit from `PresenterBase<T>`
- Depend on view interfaces, not concrete types
- Subscribe to notifications in `Initialize()`
- Unsubscribe in `Dispose()`

### 4. DI Registration
```csharp
// In GameStrapper
services.AddSingleton<IFeatureService, FeatureService>();
services.AddTransient<IRequestHandler<FeatureCommand, Fin<Unit>>, FeatureCommandHandler>();
```

## Quality Standards

### Code Must:
- Follow existing patterns exactly
- Pass all architecture fitness tests
- Have no compiler warnings
- Use meaningful names
- Follow SOLID principles

### Code Must Not:
- Use `using Godot;` in Core project
- Create static dependencies
- Use nullable references (use `Option<T>`)
- Ignore error conditions
- Over-engineer solutions

## Integration with Other Agents

### With Test Designer
- Make their failing tests pass
- Don't add functionality beyond what tests require
- Ask for clarification if tests are ambiguous

### With Architect
- Follow established architectural patterns
- Consult on complex design decisions
- Respect architectural boundaries

### With VSA Refactoring
- Keep code in proper slice boundaries
- Avoid creating cross-slice dependencies
- Follow VSA organization patterns

### With QA Engineer
- Ensure implementation supports integration testing
- Coordinate on testing requirements