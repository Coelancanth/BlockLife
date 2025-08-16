# Architect References

## Core Architecture Context

### Current System
- **Clean Architecture** with pure C# core
- **CQRS** with MediatR commands/queries
- **MVP pattern** connecting domain to Godot views
- **Vertical Slice Architecture** per feature
- **Event-driven notifications** for decoupling

### Reference Implementation
**Gold Standard**: `src/Features/Block/Move/`
- Command/Handler pattern
- Notification pipeline
- Presenter coordination
- Domain purity (no Godot)

## Architecture Rules (Non-Negotiable)

1. **No Godot in `src/`** - Domain stays pure
2. **Commands for state changes** - No direct mutations
3. **Single source of truth** - One service per responsibility
4. **MVP pattern** - Presenters handle UI coordination

## Strategic Decision Areas

### Rule Engines
- **When needed**: Complex game logic with many conditions
- **Evaluation criteria**: Performance, maintainability, team knowledge
- **Integration**: Through command handlers, maintain CQRS

### Technology Choices
- **Framework additions**: Must integrate with existing DI/CQRS
- **Performance libraries**: Profile first, optimize bottlenecks
- **External integrations**: Keep behind interfaces, maintain testability

### System-Wide Patterns
- **Cross-cutting concerns**: Logging, caching, validation
- **Performance patterns**: When optimization affects architecture
- **Scalability approaches**: Data access, command processing

## Common Strategic Scenarios

### Game Logic Complexity
- **Simple rules**: Handle in command handlers
- **Complex rules**: Consider rule engine
- **Decision point**: >5 interconnected conditions

### Performance Requirements
- **Feature-level**: Tactical optimization
- **System-level**: Architectural pattern change
- **Decision point**: Affects multiple features

### External Integration
- **Data sources**: Repository pattern
- **Services**: Interface abstraction
- **APIs**: Keep domain isolated

## Anti-Patterns to Prevent

### Over-Architecture
- ❌ Creating abstractions "just in case"
- ❌ Multiple implementations without need
- ❌ Complex frameworks for simple problems

### Under-Architecture  
- ❌ Tight coupling between layers
- ❌ Direct Godot dependencies in domain
- ❌ Shared mutable state

## Decision Templates

### Technology Evaluation
```
Options: [A, B, C]
Criteria: Performance, Learning curve, Maintenance
Recommendation: [Choice] because [reasoning]
Migration: [approach if needed]
```

### Pattern Introduction
```
Problem: [specific architectural issue]
Current: [how it's handled now]
Proposed: [new pattern]
Impact: [what changes in codebase]
```

## Quality Attributes

### Maintainability
- Clear separation of concerns
- Consistent patterns across features
- Minimal cognitive load

### Testability
- Pure functions where possible
- Interface abstractions
- Dependency injection

### Performance
- Profile before optimizing
- Consider caching strategically
- Avoid premature abstraction

## Reference Documents

- `Docs/Agents/architect/core-architecture.md` - Essential patterns
- `src/Features/Block/Move/` - Reference implementation
- `Docs/Architecture/ADRs/` - Historical decisions

## Quick References

### Layer Dependencies (Must Point Inward)
```
Godot Views → Presenters → Commands → Domain ← Infrastructure
```

### Command Flow
```
User Input → Presenter → Command → Handler → Notification → View Update
```

### DI Registration Pattern
```csharp
// ✅ Single source of truth
services.AddSingleton<GridStateService>();
services.AddSingleton<IGridStateService>(provider => 
    provider.GetRequiredService<GridStateService>());
```