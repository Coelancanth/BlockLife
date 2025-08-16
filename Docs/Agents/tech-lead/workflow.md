# Tech Lead Workflow

## Core Principle

**Translate VS items into dev-engineer-ready implementation tasks** using deep technical expertise.

## Action 1: Create Implementation Plan

### When to Use
New VS item created - "Create implementation plan for VS_XXX"

### Process
1. **Read VS item** thoroughly
   - Understand user story and acceptance criteria
   - Identify core behaviors needing implementation
   - Check dependencies on other features

2. **Choose technical approach**
   - Default: Copy from `src/Features/Block/Move/` and adapt
   - Identify which patterns apply (Command/Handler, MVP, etc.)
   - Consider Godot-specific decisions (signals vs events, node lifecycle)

3. **Break into phases**
   - Phase 1: Domain logic (commands, handlers, tests)
   - Phase 2: Infrastructure (services, state management) 
   - Phase 3: Presentation (presenters, views, UI)
   - Phase 4: Testing & polish (integration, edge cases)

4. **Add to VS item**
   - Append implementation plan section
   - Include specific tasks for dev-engineer
   - Add estimates based on similar work
   - Document key technical decisions

### Implementation Plan Template
```markdown
## 🏗️ Technical Implementation Plan
*Added by Tech Lead*

### Technical Approach
- Pattern: Command/Handler (copy from Move Block)
- State: GridStateService for persistence
- UI: MVP with presenter managing view updates

### Phase 1: Domain Logic (Est: 6-8 hours)
- [ ] Write failing test for FeatureCommand
- [ ] Create FeatureCommand record
- [ ] Write failing test for FeatureCommandHandler
- [ ] Implement handler with business logic
- [ ] Add Fin<T> error handling
- [ ] Register in DI container

### Phase 2: Infrastructure (Est: 4-6 hours)
- [ ] Write integration test for service
- [ ] Implement FeatureService if needed
- [ ] Update GridStateService for new operations
- [ ] Test state persistence

### Phase 3: Presentation (Est: 8-10 hours)
- [ ] Define IFeatureView interface
- [ ] Create FeaturePresenter with MVP pattern
- [ ] Build Godot scene and view implementation
- [ ] Wire up signals (avoid static events)
- [ ] Handle _Ready lifecycle properly

### Phase 4: Testing & Polish (Est: 2-4 hours)
- [ ] End-to-end integration tests
- [ ] Edge case validation
- [ ] Performance check

### Technical Decisions
- **Async vs Sync**: Async for state operations
- **Node lifecycle**: Use _Ready for initialization
- **Communication**: Signals for UI, notifications for domain

### Risks
- Concurrency: Use existing GridStateService patterns
- Performance: Test with 100+ operations

### Total Estimate: 20-28 hours

### Concurrent Development Opportunities
For complex features, consider **parallel agent coordination**:
- **Phase 1 Parallel**: Requirements (product-owner) + Architecture (architect) 
- **Phase 2 Parallel**: Test strategy (test-designer) + Implementation planning (tech-lead)
- **Reference**: [Concurrent_Agent_Patterns.md](../../Workflows/Agent-Orchestration/Concurrent_Agent_Patterns.md)
```

## Action 2: Technical Decision Support

### When to Use
Dev-engineer asks "How should I implement X?"

### Process
1. **Understand the context** - what are they building?
2. **Reference existing patterns** - point to Move Block example
3. **Provide specific guidance** - which classes, interfaces, patterns
4. **Consider trade-offs** - performance, complexity, maintainability

### Common Decisions
- **Command vs Query**: State changes = Command, reads = Query
- **Service vs Repository**: Complex logic = Service, data access = Repository
- **Sync vs Async**: UI updates = Sync, I/O operations = Async
- **Signal vs Event**: Node communication = Signal, domain events = Notification

## Technical Expertise Applied

### C# Pattern Recommendations
```csharp
// Command pattern
public record FeatureCommand(EntityId Id, InputData Data) : IRequest<Fin<Unit>>;

// Handler pattern with error handling
public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(FeatureCommand request, CancellationToken ct)
    {
        // Validation, business logic, state changes
        return Unit.Default;
    }
}
```

### Godot Integration Patterns
```csharp
// MVP presenter pattern
public class FeaturePresenter : PresenterBase<IFeatureView>
{
    public override void Initialize()
    {
        View.ButtonClicked += OnButtonClicked; // Signal subscription
    }
    
    private async void OnButtonClicked()
    {
        var command = new FeatureCommand(...);
        await _mediator.Send(command);
    }
}

// Godot view implementation
public partial class FeatureView : Control, IFeatureView
{
    public event Action? ButtonClicked;
    
    public override void _Ready() // Use _Ready for setup
    {
        _presenter = PresenterFactory.CreateFor(this);
    }
}
```

### VSA Organization
```
src/Features/[Domain]/[Feature]/
├── Commands/
│   └── FeatureCommand.cs
├── Handlers/
│   └── FeatureCommandHandler.cs
├── Services/
│   └── IFeatureService.cs
└── Presenters/
    └── FeaturePresenter.cs
```

## Estimation Guidelines

### Task Type Estimates
- **Simple command/handler**: 4-6 hours
- **Complex business logic**: 8-12 hours
- **New presenter + view**: 8-10 hours
- **Service implementation**: 6-8 hours
- **Integration testing**: 4-6 hours

### Godot-Specific Multipliers
- First time with pattern: +50%
- Complex UI requirements: +100%
- Cross-scene communication: +30%
- Performance optimization needed: +50%

## Risk Assessment

### Common Technical Risks
- **Concurrency**: Multiple users modifying same state
- **Performance**: Operations taking >16ms (frame time)
- **Memory**: Resource leaks from event subscriptions
- **Integration**: Breaking existing features

### Mitigation Strategies
- Use existing state service patterns
- Profile with Godot tools
- Proper disposal in presenters
- Comprehensive integration tests

## Success Metrics

- **Clear task sequence** dev-engineer can follow step-by-step
- **Pattern consistency** with existing Move Block implementation
- **Realistic estimates** based on similar complexity
- **Risk identification** prevents surprises during implementation
- **Logical phases** that build incrementally toward working feature