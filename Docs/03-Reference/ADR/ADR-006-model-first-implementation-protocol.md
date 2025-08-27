# ADR-006: Model-First Implementation Protocol

## Status
Accepted - 2025-08-27

## Context
Our codebase has experienced integration complexity and debugging challenges when features are built UI-first or with mixed concerns. The Move Block pattern has proven that building from pure domain logic outward results in more maintainable, testable code with fewer bugs.

Current pain points:
- Godot integration bugs mask domain logic errors
- Complex mocking required for testing
- Refactoring is risky when UI and logic are intertwined
- TD items often stem from premature integration decisions
- Debugging requires full Godot runtime

The team needs a standardized approach that enforces Clean Architecture principles and ensures consistent quality across all feature implementations.

## Decision
We will adopt a **Model-First Implementation Protocol** where all features are built in strict phases, starting with pure C# domain models and expanding outward through architectural layers. No phase begins until the previous phase has all tests passing.

### Implementation Phases

#### Phase 1: Pure Domain Model (Zero Dependencies)
- Define domain entities and value objects
- Implement business rules as pure functions
- Write comprehensive unit tests
- **Gate**: 100% unit tests passing, >80% coverage

#### Phase 2: Application Layer (Commands/Handlers)
- Create CQRS commands and queries
- Implement handlers with Fin<T> error handling
- Write handler unit tests
- **Gate**: All handler tests passing

#### Phase 3: Infrastructure Layer (State/Services)
- Implement state services and repositories
- Add integration tests
- Verify data flow in isolation
- **Gate**: Integration tests passing

#### Phase 4: Presentation Layer (Godot/UI)
- Create presenter contracts
- Implement MVP pattern
- Wire Godot nodes and signals
- **Gate**: Manual testing in editor

### Phase Transition Rules
1. **Hard Gate**: Cannot proceed to next phase until current phase tests are GREEN
2. **No Shortcuts**: Even "simple" features follow all phases
3. **Documentation**: Each phase completion documented in commit
4. **Review**: Tech Lead validates phase completion

## Consequences

### Positive
- **Early Bug Detection**: Logic errors caught in milliseconds, not minutes
- **Clear Dependencies**: Each layer only knows about layers below
- **Parallel Development**: Multiple devs can work on different phases
- **Refactoring Safety**: Change domain without touching UI
- **Test Speed**: Domain tests run in <1 second
- **Learning Path**: Junior devs learn architecture through practice

### Negative
- **Initial Slower Delivery**: First implementation takes longer
- **More Files**: Separation requires more classes/interfaces
- **Mental Model Shift**: Requires thinking in layers
- **Upfront Design**: Must think through domain before coding

### Neutral
- **Documentation Needs**: Each phase needs clear acceptance criteria
- **Tooling Adjustments**: CI/CD must validate phase gates
- **Persona Workflow Changes**: All personas need updated protocols

## Alternatives Considered

### Alternative 1: UI-First Development
Build Godot scenes first, add logic after
- **Pros**: Visual progress quickly, designer-friendly
- **Cons**: Logic bugs hidden, hard to test, refactoring nightmare
- **Reason not chosen**: Led to our current technical debt

### Alternative 2: Mixed Development
Build features organically without phases
- **Pros**: Flexible, developer choice
- **Cons**: Inconsistent quality, unpredictable bugs
- **Reason not chosen**: Lack of consistency causes integration issues

### Alternative 3: Full Vertical Slices
Build complete slices top-to-bottom simultaneously
- **Pros**: Ship faster, see full feature
- **Cons**: Complex debugging, slow tests, coupling
- **Reason not chosen**: Our current pain points come from this approach

## Implementation Notes

### Example: Block Merge Feature

```csharp
// PHASE 1: Pure Domain Model
public record BlockMergeResult(Block Result, int Score);

public static class BlockMerger 
{
    public static Fin<BlockMergeResult> TryMerge(Block a, Block b)
        => (a.Tier == b.Tier) 
            ? FinSucc(new BlockMergeResult(
                new Block(a.Tier + 1, BlockType.Merged),
                CalculateScore(a.Tier)))
            : FinFail<BlockMergeResult>(Error.New("Blocks must be same tier"));
    
    private static int CalculateScore(int tier) => tier * 100;
}

// PHASE 1 TEST
[Test]
public void Merge_SameTier_Succeeds()
{
    var a = new Block(1, BlockType.Basic);
    var b = new Block(1, BlockType.Basic);
    
    var result = BlockMerger.TryMerge(a, b);
    
    result.Match(
        Succ: r => {
            Assert.AreEqual(2, r.Result.Tier);
            Assert.AreEqual(100, r.Score);
        },
        Fail: e => Assert.Fail("Should succeed")
    );
}

// PHASE 2: Application Layer (only after Phase 1 complete)
public record MergeBlockCommand(BlockId A, BlockId B) : IRequest<Fin<Unit>>;

public class MergeBlockHandler : IRequestHandler<MergeBlockCommand, Fin<Unit>>
{
    private readonly IBlockRepository _repo;
    
    public async Task<Fin<Unit>> Handle(MergeBlockCommand cmd, CancellationToken ct)
    {
        var blockA = await _repo.Get(cmd.A);
        var blockB = await _repo.Get(cmd.B);
        
        return from a in blockA
               from b in blockB
               from merged in BlockMerger.TryMerge(a, b)
               from _ in _repo.Replace(cmd.A, merged.Result)
               from __ in _repo.Remove(cmd.B)
               select unit;
    }
}

// PHASE 3: Infrastructure (only after Phase 2 complete)
public class BlockStateService : IBlockRepository
{
    private readonly Dictionary<BlockId, Block> _blocks = new();
    
    public Task<Fin<Block>> Get(BlockId id) 
        => Task.FromResult(
            _blocks.TryGetValue(id, out var block)
                ? FinSucc(block)
                : FinFail<Block>(Error.New("Block not found")));
    
    // ... implementation
}

// PHASE 4: Presentation (only after Phase 3 complete)
public partial class BlockMergePresenter : Node, IBlockMergeView
{
    private readonly IMediator _mediator;
    
    public void OnBlocksCollided(BlockNode a, BlockNode b)
    {
        _mediator.Send(new MergeBlockCommand(a.Id, b.Id))
            .Match(
                Succ: _ => ShowMergeEffect(),
                Fail: e => ShowError(e));
    }
}
```

### Testing Requirements Per Phase

| Phase | Test Type | Speed | Dependencies | Run With |
|-------|-----------|-------|--------------|----------|
| 1. Domain | Unit | <100ms | None | `dotnet test --filter Category=Unit` |
| 2. Application | Unit | <500ms | Mocked repos | `dotnet test --filter Category=Handlers` |
| 3. Infrastructure | Integration | <2s | Real services | `dotnet test --filter Category=Integration` |
| 4. Presentation | Manual/E2E | Variable | Full Godot | Manual testing in editor |

### Using Existing Test Infrastructure
We leverage our existing test categories rather than creating new scripts:
- **Phase 1-2 Quick Validation**: `./scripts/test/quick.ps1` (1.3s)
- **Phase 3 Integration**: `./scripts/test/full.ps1` (3-5s)
- **Full Validation**: `./scripts/core/build.ps1 test` (complete suite)

No new phase-specific scripts needed - discipline and code review enforce the protocol.

### Commit Message Convention

```bash
feat(merge): implement domain model [Phase 1/4]
feat(merge): add command handlers [Phase 2/4]  
feat(merge): integrate state service [Phase 3/4]
feat(merge): complete UI presentation [Phase 4/4]
```

## References
- [Move Block Reference Implementation](../../../src/Features/Block/Move/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [HANDBOOK.md](../HANDBOOK.md) - Architecture patterns
- [TEST_CATEGORIES_GUIDE.md](../TEST_CATEGORIES_GUIDE.md) - Testing approach