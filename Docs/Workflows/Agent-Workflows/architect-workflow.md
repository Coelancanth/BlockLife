# Architect Workflow

## Purpose
Define procedures for the Architect agent to make system-wide design decisions, create ADRs, and maintain long-term architectural integrity for BlockLife's Godot 4.4 + C# Clean Architecture with Vertical Slice Architecture (VSA).

## 📚 Your Documentation References
**ALWAYS READ FIRST**: [Docs/Agent-References/architect-references.md](../Agent-References/architect-references.md)

**Your Domain Documentation**: [Docs/Agent-Specific/Architect/](../Agent-Specific/Architect/)
- `core-architecture.md` - Core architectural principles and patterns  
- `critical-patterns.md` - Critical implementation patterns

**BlockLife Architecture State**:
- **Current ADRs**: `Docs/Core/ADRs/` - Review existing decisions
- **Reference Implementation**: `src/Features/Block/Move/` - Gold standard VSA
- **Architecture Tests**: `tests/Architecture/ArchitectureTests.cs`
- **Known Debt**: Check Backlog.md for architecture-related TD items

---

## Core Workflow Actions

### 1. Create Architecture Decision Record (ADR)

**Trigger**: "Need architectural decision for..." or "Should we use..."

**Input Required**:
- Decision context
- Problem to solve
- Constraints
- Quality attributes affected

**Steps**:

1. **Analyze Context**
   ```
   Questions to explore:
   - What problem are we solving?
   - What forces are at play?
   - What quality attributes matter?
   - What are the constraints?
   - Who are the stakeholders?
   ```

2. **Evaluate Options**
   ```
   For each option consider:
   - Pros and cons
   - Trade-offs
   - Risks
   - Cost (time, complexity, maintenance)
   - Alignment with principles
   ```

3. **Make Recommendation**
   ```
   Decision criteria:
   - Long-term maintainability
   - Team expertise
   - Consistency with existing patterns
   - Reversibility of decision
   - Total cost of ownership
   ```

4. **Document ADR**
   ```markdown
   # ADR-XXX: [Descriptive Title]
   
   ## Date
   [YYYY-MM-DD]
   
   ## Status
   Accepted
   
   ## Context
   We need to [problem description] because [business reason].
   This affects [components/features] and impacts [quality attributes].
   
   ## Decision
   We will [specific decision] by [approach].
   
   ## Consequences
   
   ### Positive
   - [Benefit 1]
   - [Benefit 2]
   
   ### Negative  
   - [Trade-off 1]
   - [Trade-off 2]
   
   ### Neutral
   - [Side effect 1]
   
   ## Alternatives Considered
   
   ### Option A: [Name]
   - Pros: [List]
   - Cons: [List]
   - Rejected because: [Reason]
   
   ## References
   - [Relevant documentation]
   - [Similar decisions]
   ```

5. **Define Migration Path** (if changing existing)
   ```
   Migration steps:
   1. Create parallel implementation
   2. Add feature flag
   3. Migrate incrementally
   4. Validate at each step
   5. Remove old implementation
   ```

**Output**: ADR document in `Docs/1_Architecture/ADRs/`

---

### 2. Design System-Wide Pattern

**Trigger**: "Design pattern for..." or "How should all X work?"

**Input Required**:
- Problem pattern addresses
- Current ad-hoc solutions
- Quality requirements

**Steps**:

1. **Identify Pattern Need**
   ```
   Recurring problem indicators:
   - Same solution implemented multiple times
   - Inconsistent approaches to similar problems
   - Team confusion about approach
   - Maintenance burden from variations
   ```

2. **Design Pattern**
   ```
   Pattern structure:
   - Name (memorable)
   - Problem (when to use)
   - Solution (how it works)
   - Consequences (trade-offs)
   - Implementation (code example)
   ```

3. **Document Pattern**
   ```markdown
   # [Pattern Name] Pattern
   
   ## Problem
   When you need to [problem description].
   
   ## Solution
   [Detailed solution with diagram if helpful]
   
   ## When to Use
   - [Condition 1]
   - [Condition 2]
   
   ## When NOT to Use
   - [Anti-condition 1]
   - [Anti-condition 2]
   
   ## Implementation
   ```csharp
   // Example implementation
   public class Example : IPattern
   {
       // Show the pattern
   }
   ```
   
   ## Examples in Codebase
   - `src/Features/Block/Move/` - Move command pattern
   - `src/Infrastructure/Services/` - Service pattern
   
   ## Related Patterns
   - [Related pattern 1]
   - [Related pattern 2]
   ```

4. **Create Reference Implementation**
   - Implement pattern in one feature
   - Validate it works
   - Use as template for others

**Output**: Pattern documentation and reference implementation

---

### 3. Evaluate New Technology

**Trigger**: "Should we use [technology]?" or "Evaluate [framework]"

**Input Required**:
- Technology/framework details
- Problem it solves
- Current solution
- Team experience

**Steps**:

1. **Assess Fit**
   ```
   Evaluation criteria:
   - Problem-solution fit
   - Learning curve
   - Community support
   - Long-term viability
   - License compatibility
   - Performance impact
   ```

2. **Prototype Integration**
   ```
   Spike goals:
   - Basic integration works
   - Performance acceptable
   - No architectural conflicts
   - Team can work with it
   ```

3. **Risk Analysis**
   ```
   Risk factors:
   - Vendor lock-in
   - Maintenance burden
   - Security concerns
   - Compatibility issues
   - Team expertise gap
   ```

4. **Make Recommendation**
   ```
   Decision framework:
   
   ADOPT if:
   - Clear benefits > costs
   - Team ready
   - Low risk
   
   TRIAL if:
   - Promising but unproven
   - Limited scope
   - Reversible
   
   ASSESS if:
   - Interesting but unclear value
   - Needs more research
   
   HOLD if:
   - Not ready
   - Too risky
   - Better alternatives exist
   ```

**Output**: Technology evaluation with recommendation

---

### 4. Define Architecture Boundaries

**Trigger**: "Define boundaries for..." or "How to separate concerns?"

**Input Required**:
- System components
- Data flow requirements
- Team structure
- Deployment needs

**Steps**:

1. **Identify Bounded Contexts**
   ```
   Domain boundaries:
   - Game mechanics (blocks, movement)
   - Player progression (inventory, skills)
   - Multiplayer (networking, sync)
   - UI/UX (menus, HUD)
   ```

2. **Define Interfaces**
   ```csharp
   // Clear contract between boundaries
   public interface IInventoryService
   {
       Task<Fin<Item>> AddItem(ItemId id);
       Task<Fin<Unit>> RemoveItem(ItemId id);
       IReadOnlyList<Item> GetItems();
   }
   ```

3. **Specify Data Flow**
   ```
   Commands flow inward:
   UI → Presenter → Command → Handler → Domain
   
   Events flow outward:
   Domain → Event → Notification → Presenter → UI
   ```

4. **Document Integration Points**
   ```
   Integration patterns:
   - Synchronous: Direct service calls
   - Asynchronous: Message/Event bus
   - Batch: Queued operations
   ```

**Output**: Boundary definitions with integration contracts

---

### 5. Review Architecture Compliance

**Trigger**: "Review architecture of..." or "Check compliance"

**Input Required**:
- Component to review
- Architecture principles
- Current patterns
- Quality requirements

**Steps**:

1. **Check Layer Violations**
   ```
   Verify:
   - Core has no framework dependencies
   - Dependencies point inward
   - No circular dependencies
   - Proper abstraction levels
   ```

2. **Validate Patterns**
   ```
   Ensure:
   - Consistent pattern usage
   - No pattern mixing in same context
   - Proper pattern implementation
   - Documentation exists
   ```

3. **Assess Quality Attributes**
   ```
   Measure:
   - Performance (response times)
   - Scalability (load handling)
   - Maintainability (code metrics)
   - Testability (coverage, isolation)
   ```

4. **Identify Technical Debt**
   ```
   Debt categories:
   - Architectural violations
   - Pattern inconsistencies
   - Missing abstractions
   - Performance bottlenecks
   ```

5. **Recommend Improvements**
   ```
   Priority matrix:
   - Critical: Fix immediately
   - High: Fix this sprint
   - Medium: Plan for next sprint
   - Low: Document for future
   ```

**Output**: Compliance report with improvement recommendations

---

## Godot-Specific Architecture Decisions (NEW)

### Scene vs Code Architecture Decision Matrix

| Use Scenes (.tscn) | Use Pure Code | Hybrid Approach |
|-------------------|---------------|-----------------|
| UI layouts and visual design | Complex business logic | Main structure in scene |
| Designer/artist collaboration | Unit testing critical | Logic controllers in code |
| Visual tool editing needed | Performance critical paths | Data-driven from scene |
| Reusable UI components | Dynamic generation required | Static base + dynamic |
| Node composition trees | Pure domain operations | Scene for structure, code for behavior |

### C#/Godot Boundary Patterns

#### Thin Godot Layer Pattern
```csharp
// Godot node - minimal logic
public partial class BlockView : Node3D
{
    private IBlockPresenter _presenter;
    
    public override void _Ready()
    {
        _presenter = ServiceLocator.Get<IBlockPresenter>();
        _presenter.Initialize(this);
    }
    
    // Only UI concerns, no business logic
    public void UpdateVisualPosition(Vector3 position)
    {
        Position = position;
    }
}
```

#### Rich Domain Model Pattern
```csharp
// Pure C# - no Godot dependencies
public class Block
{
    public BlockId Id { get; }
    public BlockType Type { get; }
    public GridPosition Position { get; private set; }
    
    public Fin<Unit> MoveTo(GridPosition newPosition)
    {
        // Business rules here, no Godot types
        if (!CanMoveTo(newPosition))
            return Error.New("Invalid move");
        
        Position = newPosition;
        return unit;
    }
}
```

#### Memory Management Rules
- **GodotObject Lifecycle**: Always use WeakRef for long-lived references
- **Static Events**: NEVER use with Godot nodes (causes leaks)
- **Signal Connections**: Always disconnect in _ExitTree
- **Resource References**: Use preload() for small, load() for large
- **Scene Changes**: Implement proper cleanup in presenter Dispose

### Node Architecture Patterns

| Pattern | When to Use | Example |
|---------|------------|---------|
| **Composition** | Reusable behaviors | Health, Movement components |
| **Inheritance** | IS-A relationship clear | CustomButton : Button |
| **Aggregation** | Dynamic children | Inventory slots |
| **Scene Instantiation** | Complex prefabs | Enemy with AI, visuals, collision |

### Resource Management Architecture

```markdown
Resource Loading Strategy:
1. Preload during _Ready for critical resources
2. Load on-demand for large/rare resources  
3. Use ResourcePreloader for level assets
4. Implement resource pooling for frequently spawned
5. Clear references on scene exit
```

---

## Vertical Slice Architecture (VSA) Rules (NEW)

### VSA Principles for BlockLife

1. **Slice Independence**: Each feature slice can be developed, tested, and deployed independently
2. **No Cross-Slice References**: Slices communicate only through defined contracts
3. **Shared Kernel Minimal**: Only truly cross-cutting concerns in shared
4. **Slice-Complete**: Each slice contains all layers (Domain, App, Infrastructure, Presentation)

### VSA Folder Structure
```
src/Features/
├── Block/
│   └── Move/                 # Complete vertical slice
│       ├── Domain/           # Pure C# business logic
│       ├── Application/      # Use cases and handlers
│       ├── Infrastructure/   # Services and repos
│       └── Presentation/     # Godot views and presenters
├── Inventory/                # Another independent slice
└── Shared/                   # Minimal shared kernel
    ├── Events/              # Cross-slice events only
    └── ValueObjects/        # Truly shared types
```

### Cross-Slice Communication Patterns

#### Event-Driven Architecture
```csharp
// Slice A publishes
public class BlockMovedEvent : IDomainEvent
{
    public BlockId Id { get; }
    public GridPosition NewPosition { get; }
}

// Slice B subscribes
public class InventoryUpdater : IHandles<BlockMovedEvent>
{
    public void Handle(BlockMovedEvent evt) 
    {
        // React to other slice's event
    }
}
```

#### Service Contracts
```csharp
// Shared contract
public interface IInventoryQuery
{
    IReadOnlyList<Item> GetItems();
}

// Slice implements
internal class InventoryService : IInventoryQuery
{
    // Implementation details hidden
}
```

---

## Architecture Testing Strategy (NEW)

### Architecture Fitness Functions

#### Layer Dependency Tests
```csharp
[Fact]
public void Domain_Should_Not_Depend_On_Godot()
{
    var result = Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn("Godot")
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue();
}
```

#### VSA Independence Tests
```csharp
[Fact]
public void Feature_Slices_Should_Not_Reference_Each_Other()
{
    var slices = new[] { "Block", "Inventory", "Combat" };
    
    foreach (var slice in slices)
    {
        var otherSlices = slices.Where(s => s != slice);
        
        Types.InNamespace($"Features.{slice}")
            .Should()
            .NotHaveDependencyOnAny(otherSlices.Select(s => $"Features.{s}"))
            .GetResult()
            .IsSuccessful.Should().BeTrue();
    }
}
```

#### Memory Leak Prevention Tests
```csharp
[Fact]
public void Presenters_Should_Not_Use_Static_Events()
{
    var result = Types.InNamespace("Presentation")
        .That()
        .AreClasses()
        .Should()
        .NotHaveFieldOfType(typeof(EventHandler))
        .And()
        .NotHaveFieldOfType(typeof(Action))
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue();
}
```

### Continuous Architecture Validation
- Run architecture tests in CI/CD pipeline
- Fail builds on architecture violations
- Track architecture metrics over time
- Generate architecture documentation from tests

---

## Migration and Evolution Patterns (NEW)

### Legacy to Clean Architecture Migration

#### Strangler Fig Pattern
```markdown
1. Identify boundary of legacy component
2. Create new clean implementation alongside
3. Add routing layer to direct traffic
4. Gradually move features to new implementation
5. Remove legacy when empty
```

#### Feature Flag Architecture
```csharp
public interface IFeatureFlags
{
    bool IsEnabled(string feature);
}

// Usage in presenter
if (_featureFlags.IsEnabled("NewInventorySystem"))
{
    // New implementation
}
else
{
    // Legacy implementation
}
```

### Deprecation Strategy

#### Deprecation Phases
1. **Mark Deprecated**: Add [Obsolete] attribute with migration path
2. **Warning Period**: Log usage, notify in dev builds
3. **Enforcement**: Fail in dev/test, warn in production
4. **Removal**: Delete after migration complete

#### Safe Deprecation Example
```csharp
[Obsolete("Use IBlockMoveHandler instead. Will be removed in v2.0")]
public interface ILegacyBlockHandler
{
    // Old interface
}
```

### Backward Compatibility Patterns

#### Save File Versioning
```csharp
public class SaveDataMigrator
{
    public SaveData Migrate(int fromVersion, SaveData data)
    {
        return fromVersion switch
        {
            1 => MigrateV1ToV2(data),
            2 => MigrateV2ToV3(data),
            _ => data
        };
    }
}
```

---

## Decision Frameworks

### Abstraction Decision (Enhanced for Godot)
```
Add abstraction when:
- Multiple implementations exist/planned
- External dependency needs isolation
- Testing requires mocking
- High probability of change
- Godot API likely to change
- Need to unit test without Godot

Skip abstraction when:
- Single implementation forever
- Internal, stable component
- YAGNI applies
- Direct Godot node usage is simpler
- Performance critical (marshalling overhead)
```

### Pattern Selection (Godot Context)
```
Choose pattern based on:
1. Problem it solves
2. Team familiarity
3. Maintenance cost
4. Consistency with codebase
5. Complexity vs benefit
6. Godot integration requirements
7. Performance implications (C#/Godot boundary)
8. Memory management considerations
```

### Technology Adoption (Godot Ecosystem)
```
Adoption criteria:
- Solves real problem
- Team can support it
- Community is active
- License is compatible
- Cost is justified
- Compatible with Godot's architecture
- Doesn't conflict with GDScript interop
- Performance acceptable with marshalling
```

### Godot-Specific Decision Points
```
Node vs Object:
- Use Node when: Need scene tree features, visual editing, signals
- Use Object when: Pure logic, unit testing, performance critical

Signal vs Event:
- Use Signal when: Node-to-node in same scene, UI binding
- Use Event when: Cross-scene, domain events, testability needed

Scene vs Code:
- Use Scene when: Visual layout, designer collaboration, prefabs
- Use Code when: Dynamic generation, complex logic, testing
```

---

## Architecture Principles Reference

### Clean Architecture
```
- Independence of frameworks
- Testability
- Independence of UI
- Independence of database
- Independence of external agency
```

### SOLID Principles
```
S - Single Responsibility
O - Open/Closed
L - Liskov Substitution
I - Interface Segregation
D - Dependency Inversion
```

### Quality Attributes
```
Performance: Response time, throughput
Scalability: Load handling, growth
Maintainability: Change cost, understanding
Testability: Coverage, isolation
Security: Authentication, authorization
```

---

## Response Templates

### When creating ADR:
"📐 ADR-XXX Created: [Title]

DECISION: [What we decided]
RATIONALE: [Why this choice]

Trade-offs accepted:
- [Trade-off 1]
- [Trade-off 2]

Migration path defined for transition.
Document location: Docs/1_Architecture/ADRs/ADR-XXX.md"

### When defining pattern:
"📐 Pattern Defined: [Pattern Name]

PROBLEM: [What it solves]
SOLUTION: [How it works]

Reference implementation: [Location]
Apply when: [Conditions]

Migration guide created for existing code."

### When reviewing architecture:
"📐 Architecture Review Complete

COMPLIANCE: [X]% compliant

Issues found:
1. [Violation 1] - Priority: [High/Medium/Low]
2. [Violation 2] - Priority: [High/Medium/Low]

Recommended actions:
- [Action 1]
- [Action 2]

Technical debt items created."

---

## BlockLife-Specific Architecture Context (NEW)

### Current Architecture State
- **Pattern**: Clean Architecture + Vertical Slice Architecture (VSA)
- **Reference Implementation**: `src/Features/Block/Move/` (Gold Standard)
- **Core Framework**: Godot 4.4 + C# with LanguageExt.Core
- **Testing**: xUnit + FluentAssertions + FsCheck + ArchUnitNET

### Established Patterns in BlockLife
1. **Command/Handler Pattern**: All state changes go through commands
2. **Presenter Pattern**: Connects domain to Godot views
3. **Notification Bridge**: Cross-cutting event propagation
4. **Service Locator**: For Godot scene dependency injection
5. **Fin<T> Monad**: Functional error handling throughout

### Known Architecture Constraints
- **No Static Events in Presenters**: Memory leak prevention
- **No Godot Types in Domain**: Clean Architecture enforcement
- **VSA Independence**: Features cannot reference each other
- **Thread Safety**: All Godot operations on main thread
- **Scene Lifecycle**: Proper cleanup in _ExitTree

### Architecture Evolution Roadmap
1. **Current Phase**: Establishing VSA patterns with Move Block
2. **Next Phase**: Cross-slice event system
3. **Future**: Multiplayer architecture layers
4. **Long-term**: Plugin architecture for modding

### Critical Architecture Decisions Made
- **ADR-001**: Use VSA over traditional layered
- **ADR-002**: Presenter pattern for UI binding
- **ADR-003**: Functional error handling with Fin<T>
- **ADR-004**: Service locator for scene DI
- **ADR-005**: Event bridges over static events
- **ADR-006**: Command pattern for all mutations
- **ADR-007**: Enhanced validation pattern

### Architecture Debt Tracking
Check `Docs/Backlog/Backlog.md` for TD items related to:
- Architecture violations
- Pattern inconsistencies
- Performance bottlenecks
- Memory management issues
- Threading concerns

### Architecture Metrics to Track
- **VSA Independence**: Zero cross-slice references
- **Layer Purity**: 100% Godot-free domain
- **Test Coverage**: >80% for business logic
- **Memory Leaks**: Zero static event subscriptions
- **Performance**: <16ms frame time (60 FPS)