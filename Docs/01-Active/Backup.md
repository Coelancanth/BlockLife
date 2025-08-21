
### VS_003A: Match-3 with Attributes (Phase 1) [Score: 95/100]
**Status**: Approved
**Owner**: Tech Lead → Dev Engineer
**Size**: M (6.5 hours - updated estimate)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: None

**What**: Match 3+ adjacent same-type blocks to clear them and earn attributes
**Why**: Proves core resource economy loop before adding complexity

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED for implementation with Pattern Recognition Architecture**

**Architecture Decision**: 
- Implement extensible Pattern Recognition Framework instead of simple match detection
- Patterns are descriptions (what COULD happen) separate from execution (what SHOULD happen)
- Enables future tier-ups, transmutations, and chains without refactoring
- See [ADR-001](../03-Reference/ADR/ADR-001-pattern-recognition-framework.md) for detailed architectural rationale

**Technical Approach**:
1. **Pattern Framework** (1.5h): Core abstractions - IPattern, IPatternRecognizer, IPatternResolver, IPatternExecutor
2. **Match Implementation** (1.5h): MatchPatternRecognizer with flood-fill, MatchPatternExecutor for clearing blocks
3. **Player State** (1h): Domain model for resources/attributes, PlayerStateService for tracking
4. **CQRS Integration** (1h): ProcessPatternsCommand triggered after actions, pattern processing pipeline
5. **Presentation** (0.5h): Simple text UI for attributes display

**Key Implementation Files**:
- `src/Core/Features/Block/Patterns/` - Pattern recognition framework
- `src/Core/Domain/Player/` - Player state domain model  
- `src/Core/Features/Block/Patterns/Recognizers/MatchPatternRecognizer.cs` - Flood-fill implementation
- `src/Core/Features/Block/Patterns/Commands/ProcessPatternsCommand.cs` - CQRS integration

**Testing Requirements**:
- 5 tests for MatchPatternRecognizer (horizontal, vertical, L-shape, T-shape, no-match)
- 3 tests for reward calculations (resource mapping, size bonuses, all block types)
- 3 tests for pattern resolution (priority handling, conflict resolution)
- 2 tests for player state updates

**Updated Estimate**: 6.5 hours (added 1h for pattern framework)

**Rationale for Architecture**:
- Flood-fill is encapsulated in ONE recognizer, not spread through codebase
- New pattern types (tier-up, transmute) plug in without touching existing code
- Testable in isolation - mock recognizers for complex scenarios
- Chains work automatically through recursive pattern detection
- Follows Open/Closed Principle - extensible but stable

**Developer Implementation Guide**:

**Pattern Framework Contracts**:
```csharp
// Core pattern interface - all patterns implement this
public interface IPattern
{
    PatternType Type { get; }              // Match, TierUp, Transmute
    Seq<Vector2Int> Positions { get; }     // Blocks involved
    int Priority { get; }                  // Match=10, TierUp=20, Transmute=30
    IPatternOutcome CalculateOutcome();    // What happens if executed
}

// Recognizer finds patterns at a position
public interface IPatternRecognizer
{
    PatternType SupportedType { get; }
    bool IsEnabled { get; }  // Can be toggled by unlocks
    Seq<IPattern> Recognize(IGridStateService grid, Vector2Int trigger, PatternContext ctx);
}

// Executor applies pattern to game state
public interface IPatternExecutor
{
    Task<Fin<ExecutionResult>> Execute(IPattern pattern, ExecutionContext context);
}
```

**Integration Flow (Step-by-Step)**:
1. Player completes action (move/place) → `BlockMovedNotification`
2. `ActionCompletedNotificationHandler` catches notification
3. Handler sends `ProcessPatternsCommand(triggerPosition)`
4. `ProcessPatternsCommandHandler` orchestrates:
   - PatternEngine.FindPatterns() → calls all recognizers
   - PatternResolver.Resolve() → picks highest priority
   - PatternExecutor.Execute() → applies changes
   - Check for chains → recursive call if new patterns
5. Publish `PatternProcessedNotification` → UI updates

**Flood-Fill Implementation Details**:
```csharp
private Seq<Vector2Int> FloodFill(IGridStateService grid, Vector2Int start, BlockType targetType)
{
    var visited = new HashSet<Vector2Int>();
    var connected = new List<Vector2Int>();
    var stack = new Stack<Vector2Int>();
    
    stack.Push(start);
    
    while (stack.Count > 0 && connected.Count < 100) // Safety limit
    {
        var pos = stack.Pop();
        if (visited.Contains(pos)) continue;
        
        var block = grid.GetBlockAt(pos);
        if (block.IsNone || block.Value.Type != targetType) continue;
        
        visited.Add(pos);
        connected.Add(pos);
        
        // Add orthogonal neighbors (not diagonal)
        foreach (var neighbor in GetOrthogonalNeighbors(pos))
        {
            if (grid.IsValidPosition(neighbor) && !visited.Contains(neighbor))
                stack.Push(neighbor);
        }
    }
    
    return connected.Count >= 3 ? Seq(connected) : Seq<Vector2Int>.Empty;
}
```

**DI Container Registration** (in `BlockLifeServiceRegistration.cs`):
```csharp
// Player state (singleton - one player)
services.AddSingleton<IPlayerStateService, PlayerStateService>();

// Pattern recognition system (scoped per request)
services.AddScoped<IPatternEngine, PatternEngine>();
services.AddScoped<IPatternResolver, PriorityResolver>();

// Register ALL recognizers (they'll be injected as IEnumerable)
services.AddScoped<IPatternRecognizer, MatchPatternRecognizer>();
// Future: services.AddScoped<IPatternRecognizer, TierUpRecognizer>();

// Register executors by type
services.AddScoped<IPatternExecutor, MatchPatternExecutor>();
```

**First Test to Write** (TDD approach):
```csharp
[Fact]
public void Recognize_ThreeHorizontal_ReturnsMatchPattern()
{
    // Arrange
    var grid = new TestGridBuilder()
        .WithBlock(0, 0, BlockType.Work)
        .WithBlock(1, 0, BlockType.Work)
        .WithBlock(2, 0, BlockType.Work)
        .Build();
    
    var recognizer = new MatchPatternRecognizer();
    var context = new PatternContext(null, null, null); // Can be null for basic tests
    
    // Act
    var patterns = recognizer.Recognize(grid, new Vector2Int(1, 0), context);
    
    // Assert
    patterns.Should().ContainSingle();
    var pattern = patterns.First();
    pattern.Type.Should().Be(PatternType.Match);
    pattern.Positions.Should().BeEquivalentTo(new[] {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(2, 0)
    });
}
```

**Critical Edge Cases to Handle**:
- **Empty positions**: Skip during flood-fill (GetBlockAt returns None)
- **Grid boundaries**: Always check `IsValidPosition()` before access
- **Overlapping patterns**: Resolver picks highest priority (TierUp > Match)
- **Rapid actions**: Commands queued in MediatR, processed sequentially
- **Pattern at edge**: Works normally (flood-fill handles boundaries)
- **No patterns found**: Return empty Seq, no error
- **Circular dependencies**: Visited set prevents infinite loops

**Common Pitfalls to Avoid**:
❌ **DON'T** mutate grid during recognition phase
❌ **DON'T** execute patterns inside recognizers
❌ **DON'T** couple recognizers to specific executors
❌ **DON'T** use mutable collections in patterns
❌ **DON'T** throw exceptions - use Fin<T> for errors

✅ **DO** keep patterns immutable (use records)
✅ **DO** test recognizers in complete isolation
✅ **DO** use LanguageExt.Seq for collections
✅ **DO** validate all grid positions before access
✅ **DO** log pattern detection for debugging

**Resource/Attribute Mapping** (for reference):
```csharp
public static class BlockTypeRewards
{
    public static (RewardType type, int amount) GetReward(BlockType blockType) => blockType switch
    {
        BlockType.Work => (RewardType.Resource(ResourceType.Money), 10),
        BlockType.Study => (RewardType.Attribute(AttributeType.Knowledge), 10),
        BlockType.Health => (RewardType.Attribute(AttributeType.Health), 10),
        BlockType.Relationship => (RewardType.Resource(ResourceType.SocialCapital), 10),
        BlockType.Fun => (RewardType.Attribute(AttributeType.Happiness), 10),
        // ... etc for all 9 types
    };
}
```

**Performance Considerations**:
- Flood-fill limited to 100 blocks (prevent runaway)
- Pattern detection runs synchronously (fast enough)
- Cache patterns for same board state (future optimization)
- Use ValueTask for hot paths (if profiling shows need)

**Core Mechanic**:
- Match 3+ adjacent same-type blocks (orthogonal only)
- Matched blocks disappear (no transformation)
- Each block type grants specific resources or attributes:
  - Work → Money +10 per block (resource)
  - Study → Knowledge +10 per block (attribute)
  - Health → Health +10 per block (attribute)
  - Relationship → Social Capital +10 per block (resource)
  - Fun → Happiness +10 per block (attribute)
  - Sleep → Energy +10 per block (attribute)
  - Food → Nutrition +10 per block (attribute)
  - Exercise → Fitness +10 per block (attribute)
  - Meditation → Mindfulness +10 per block (attribute)
- Display current attributes (text UI for now)

**Match Size Bonuses**:
- Match-3: Base rewards (×1.0)
- Match-4: ×1.5 bonus multiplier
- Match-5: ×2.0 bonus multiplier
- Match-6+: ×3.0 bonus multiplier

**Done When**:
- Matching 3+ blocks clears them from grid
- Attributes increase based on block types matched
- Current attributes display on screen
- Works for all 9 block types
- 5+ unit tests for match detection
- 3+ tests for attribute calculation

**NOT in Scope**:
- Transformation to higher tiers
- Spending attributes
- Unlocks or progression
- Chain reactions

### VS_003B: Tier System Introduction [Score: 80/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: S (4-6 hours)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003A

**What**: Add tier indicators and tier-based reward scaling
**Why**: Introduces progression concept without transformation mechanics

**Core Mechanic**:
- Blocks show tier indicator (T1, T2, T3)
- Higher tier blocks give more resources/attributes when matched:

**Tier Bonuses** (multiplicative):
- Tier 1: ×1.0 (base)
- Tier 2: ×3.0 tier bonus
- Tier 3: ×10.0 tier bonus
- Manual tier-2/3 spawn for testing (debug command)

**Done When**:
- Blocks display tier visually
- Matching higher tiers gives proportional resources/attributes
- Debug commands to spawn specific tiers
- 3+ tests for tier-based calculations

### VS_003C: Unlockable Tier-Up System [Score: 75/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: M (8-10 hours)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003B

**What**: Spend resources to unlock tier-up abilities
**Why**: Creates strategic resource management and player agency

**Core Mechanic**:
- Unlock shop UI (simple buttons)
- Spend resources to unlock tier-up abilities:
  - "Unlock Work Tier-Up" - 100 Money (resource)
  - "Unlock Study Tier-Up" - 100 Knowledge (attribute)
- When unlocked: 3 same-type blocks can tier-up to 1 higher tier block
- Player chooses: match for resources/attributes OR tier-up for progression

**Done When**:
- Unlock shop displays available unlocks
- Can spend resources to unlock abilities
- Unlocked tier-ups work alongside matching
- Visual indicator for unlocked abilities
- 5+ tests for unlock system

### VS_004: Auto-Spawn System [Score: 75/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (for breakdown)
**Size**: S (4-6 hours - straightforward mechanic)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003A (need matching to manage spawned blocks)

**What**: Automatically spawn new blocks at TURN START before player acts (Tetris-style)
**Why**: Creates strategic planning - player must account for new block before moving

**Core Mechanic** (UPDATED - Turn-Based):
- **Spawn Trigger**: At TURN START (before player can act)
- **Turn Flow**: Spawn → Player sees board → Player acts → Matches resolve → Turn ends
- **Spawn Count**: 1 block per turn (adjustable for difficulty later)
- **Spawn Position**: Random empty position
- **Spawn Type**: Random from available block types (weighted distribution)
- **Game Over**: When spawn fails due to no empty positions at turn start

**Implementation Approach**:
- Hook into existing command completion (after move/place/merge)
- Find all empty positions on grid
- If empty positions exist: spawn random block type at random position
- If no empty positions: trigger game over state
- Visual feedback for spawned block (appear animation/effect)

**Spawn Distribution** (initial):
- All 9 block types equally weighted (11.1% each)
- Future: Weight based on life stage or difficulty

**Done When**:
- Block spawns after every player action
- Spawns only on empty positions
- Visual indication of newly spawned block
- Game over triggers when grid is full
- Game over screen with final resources and attributes
- 5+ unit tests for spawn logic
- 2+ tests for game over detection

**NOT in Scope**:
- Difficulty progression (spawn rate increase)
- Weighted spawn probabilities
- Special spawn patterns or rules
- Power-ups to clear spawned blocks
- Spawn preview/prediction
- Multiple spawns per turn
- Life-stage specific spawn rules

**Product Owner Notes**:
- Start with simplest version - one block per turn
- This creates the core gameplay loop: Act → Spawn → React
- Game over condition finally makes score meaningful
- Must feel fair - random but not cruel

### VS_005: Chain Reaction System [Score: 90/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (for breakdown)
**Size**: M (6-8 hours - builds on VS_003A foundation)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003A (need basic matching first)

**What**: Add cascading matches that trigger automatically, with exponential resource/attribute bonuses
**Why**: Creates the addictive "YES!" moments that separate good puzzle games from great ones

**Core Mechanic**:
- **Chain Trigger**: After any match completes, check if result can trigger new match
- **Recursive Detection**: Each chain can trigger another chain
- **Chain Bonus**: Base × 1 → ×2 → ×4 → ×8 → ×16 (exponential)
- **Chain Counter**: Display "Chain ×2!", "Chain ×3!" etc.
- **Celebration**: Bigger effects for longer chains

**Implementation Approach**:
- After match completes, run match detection on result position
- If new match detected, execute it with increased bonus
- Continue recursively until no more matches possible
- Track chain depth for scoring and display
- Add brief delay between chains for visual clarity

**Resource/Attribute Formula**:
```
Chain 1: 10 resources/attributes × 1 = 10
Chain 2: 10 resources/attributes × 2 = 20
Chain 3: 10 resources/attributes × 4 = 40
Chain 4: 10 resources/attributes × 8 = 80
Total for 4-chain: 150 resources/attributes!

Final Formula: (BaseValue × BlockCount × TierBonus × MatchSizeBonus × ChainBonus) + Rewards
```

**Done When**:
- Matches automatically trigger follow-up matches
- Resource/attribute bonus increases exponentially per chain
- Chain counter displays during chains
- Visual delay between chain steps (player can follow what happened)
- Different sound effects for each chain level
- 5+ unit tests for chain detection
- 3+ tests for bonus calculation
- 2+ tests for recursive chain limits

**NOT in Scope**:
- Special chain-only blocks
- Chain preview/planning UI
- Undo for chains
- Chain-specific animations (use simple for now)
- Maximum chain bonuses/achievements
- Chain-triggered special events

**Critical Design Decisions**:
- **Delay Between Chains**: 0.3-0.5 seconds (fast enough to feel smooth, slow enough to see)
- **Max Chain Depth**: Unlimited (let players find crazy combos)
- **Bonus Cap**: No cap initially (see how high players can go)

**Product Owner Notes**:
- This is THE feature that makes match-3 games addictive
- Must feel satisfying - sound/visual feedback crucial
- Exponential bonuses reward elaborate setups
- Creates skill gap between new and experienced players
- Watch for degenerate strategies (infinite chains)

### TD_017: Create Centralized Turn Manager
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer  
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Markers**: [ARCHITECTURE] [ROOT-CAUSE]

**What**: Centralize turn/phase management in single service
**Why**: Scattered turn logic causes race conditions, makes replay/undo impossible

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED** - Implement before VS_004 (Auto-Spawn)

**Technical Approach**:
```csharp
public enum TurnPhase 
{
    SpawnPhase,      // New blocks appear
    PlayerPhase,     // Player can act
    ResolutionPhase, // Patterns process
    EndPhase         // Cleanup, check game over
}

public interface ITurnManager 
{
    int CurrentTurn { get; }
    TurnPhase CurrentPhase { get; }
    bool IsPlayerActionAllowed { get; }
    
    Task<Fin<Unit>> StartNewTurn();
    Task<Fin<Unit>> AdvancePhase();
    Task<Fin<Unit>> ExecutePlayerAction(IRequest command);
}

public class TurnManager : ITurnManager 
{
    private readonly IMediator _mediator;
    
    public async Task<Fin<Unit>> StartNewTurn() 
    {
        CurrentTurn++;
        CurrentPhase = TurnPhase.SpawnPhase;
        
        // Publish notification for spawn system
        await _mediator.Publish(new TurnStartedNotification(CurrentTurn));
        
        return await AdvancePhase();
    }
    
    public async Task<Fin<Unit>> ExecutePlayerAction(IRequest command) 
    {
        if (CurrentPhase != TurnPhase.PlayerPhase)
            return Fin<Unit>.Fail(Error.New("Actions only allowed during player phase"));
            
        var result = await _mediator.Send(command);
        if (result.IsSucc)
            await AdvancePhase();
            
        return result;
    }
}
```

**Done When**:
- ITurnManager interface defined
- TurnManager implementation with phases
- All player actions go through TurnManager
- Turn flow: Spawn → Player → Resolve → End
- 5+ unit tests for phase transitions

**Depends On**: None - Do before VS_004

### TD_018: Add Block Unique IDs
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer
**Size**: S (1-2 hours)  
**Priority**: Important
**Created**: 2025-08-19
**Markers**: [ARCHITECTURE]

**What**: Add unique identifier to Block entity
**Why**: Position-based identity breaks with animations, history tracking, future multiplayer

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED** - Implement before complex features need block tracking

**Technical Approach**:
```csharp
public record Block 
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public BlockType Type { get; init; }
    public int Tier { get; init; } = 1;
    public Vector2Int Position { get; init; }
    
    // Position changes, ID stays same
    public Block MoveTo(Vector2Int newPos) => this with { Position = newPos };
}

// Commands reference blocks by ID, not position
public record MoveBlockCommand(
    Guid BlockId,  // Not Vector2Int position
    Vector2Int TargetPosition
) : IRequest<Fin<Unit>>;
```

**Done When**:
- Block has Id field (Guid or int)
- Commands use BlockId not position
- Grid service has GetBlockById method
- Tests verify ID persistence through moves

**Depends On**: None

### VS_003D: Cross-Type Transmutation System [Score: 60/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: M (6-8 hours)
**Priority**: Ideas (future)
**Created**: 2025-08-19
**Depends On**: VS_003C

**What**: Unlock special cross-type transmutations
**Why**: Adds strategic depth through type conversion

**Core Mechanic**:
- Expensive unlocks for transmutation recipes:
  - Work + Work + Study → Career (500 Money + 300 Knowledge)
  - Health + Health + Fun → Wellness (300 Health + 200 Happiness)
- Different from tier-up: Changes block TYPE not TIER
- Creates special blocks with unique properties

**Done When**:
- Unlock shop displays transmutation recipes
- Can spend resources to unlock transmutation abilities
- Transmutation works alongside matching and tier-up
- Visual indication of transmuted block types
- 5+ tests for transmutation system

### TD_019: Create Resource Manager Service
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer
**Size**: S (2 hours)
**Priority**: Ideas
**Created**: 2025-08-19

**What**: Centralize all resource loading through single service
**Why**: Scattered GD.Load calls make asset reorganization and bundling painful

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED for later** - Not critical path, do after MVP

**Technical Approach**:
```csharp
public interface IResourceManager 
{
    Texture2D GetBlockTexture(BlockType type, int tier);
    AudioStream GetSoundEffect(SoundType type);
    PackedScene GetPrefab(string prefabName);
    T LoadResource<T>(string path) where T : Resource;
}

public class ResourceManager : IResourceManager 
{
    private readonly Dictionary<string, Resource> _cache = new();
    
    public Texture2D GetBlockTexture(BlockType type, int tier) 
    {
        var key = $"block_{type}_t{tier}";
        if (!_cache.ContainsKey(key)) 
        {
            var path = $"res://assets/blocks/{type.ToString().ToLower()}_tier{tier}.png";
            _cache[key] = GD.Load<Texture2D>(path);
        }
        return (Texture2D)_cache[key];
    }
}
```

**Done When**:
- IResourceManager interface exists
- All GD.Load calls go through manager
- Resources are cached appropriately
- Easy to swap resource paths/bundles

**Depends On**: None - Do after MVP





### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 40/100]
**Status**: Approved - Immediate Part Ready
**Owner**: Test Specialist  
**Size**: XS (immediate) + M (future property suite)
**Priority**: Ideas (not critical path)
**Created**: 2025-08-19
**Proposed By**: Test Specialist
**Markers**: [QUALITY] [TESTING]

**What**: Implement FSCheck property tests for swap operation invariants
**Why**: Catch edge cases that example-based tests might miss, ensure mathematical properties hold

**Tech Lead Decision** (2025-08-18):
✅ **APPROVED with modifications - Defer to after MVP**

**Analysis**:
- Current swap has only 2 example-based tests
- Property tests would catch edge cases we haven't thought of
- FSCheck is mature and well-suited for game logic invariants
- Swap operation has clear mathematical properties to verify

**However**: 
- We have only 2 swap tests currently - not enough surface area yet
- Property tests shine when you have complex state spaces
- Current swap is relatively simple (range check + position swap)

**Modified Approach**:
1. **Immediate** (5 min): Add 2-3 more example-based tests for critical cases:
   - Swap with boundary blocks (edge of grid)
   - Failed swap attempts (out of range) 
   - Swap with same block (should fail gracefully)
   
**Test Specialist Assignment** (2025-08-18):
✅ **READY FOR IMPLEMENTATION** - Tech Lead analysis complete, Test Specialist to implement
- Current swap tests: 2 existing (CompleteDrag_ToOccupiedPosition_WithinRange_ShouldSwapBlocks, CompleteDrag_SwapAtMaxRange_ShouldSucceed)
- Missing coverage: boundary cases, same-block swaps, edge validation
- Test file location: `tests/BlockLife.Core.Tests/Features/Block/Drag/DragCommandTests.cs`
- Follow existing test patterns using BlockBuilder and FluentAssertions

2. **After MVP** (when swap gets complex):
   - Implement full property-based test suite
   - Add generators for game states
   - Test invariants across all block operations

**Rationale**:
- Property tests are valuable but premature optimization now
- With only 2 tests, we need basic coverage first
- When swap mechanics get complex (power-ups, constraints), revisit

**Proposed Properties** (Future Implementation):
```csharp
// 1. Swap preserves total block count
[Property]
public Property SwapOperation_PreservesBlockCount()

// 2. Swap validation is symmetric
[Property]
public Property SwapDistance_IsSymmetric()
// If A can swap with B, then B can swap with A

// 3. Double swap returns to original state
[Property]
public Property DoubleSwap_ReturnsToOriginal()
// Swapping twice = identity operation
```

**Done When**:
- **Immediate**: 2-3 additional example tests added and passing
- **Future**: Property tests integrated with 1000+ generated cases
- Edge cases discovered are documented
- CI pipeline includes property test execution


### TD_048: Migrate FsCheck Property-Based Tests to 3.x API [Score: 30/100]
**Status**: Approved
**Owner**: Test Specialist
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [TESTING] [MIGRATION] [FSCHECK] [PROPERTY-BASED]
**Created**: 2025-08-21

**What**: Update FsCheck property-based tests from 2.16.6 to 3.3.0 API
**Why**: FsCheck 3.x has breaking API changes; tests currently disabled and moved to `FsCheck_Migration_TD047/`
**How**:
- Research FsCheck 3.x API changes (use Context7 if available)
- Update `Prop.ForAll` usage to new API patterns
- Fix `Gen<T>`, `Arb`, and `Arbitrary<T>` usage
- Resolve Property attribute conflicts with xUnit
- Update custom generators in `BlockLifeGenerators.cs`
- Update property tests in `SimplePropertyTests.cs`
- Re-enable FsCheck.Xunit package reference
- Move tests back from `FsCheck_Migration_TD047/` to proper location
- Ensure all 7 property-based tests pass
**Done When**:
- All FsCheck tests compile with 3.x API
- All property-based tests passing (7 tests)
- FsCheck.Xunit package re-enabled in project
- No references to old 2.x API patterns
- Property tests moved back to proper test directory
**Depends On**: None

**Problem Context**: Package updates completed but FsCheck 3.x has extensive breaking changes requiring dedicated migration effort. Tests temporarily disabled to unblock other infrastructure updates.

**Tech Lead Decision** (2025-08-21):
- Actual Complexity: 35/100 (slightly underestimated due to API redesign)
- Decision: APPROVED - Legitimate technical debt from package updates
- FsCheck 3.x is fundamental API redesign, not just version bump
- Property testing provides valuable edge case coverage for game logic
- Migration necessary to re-enable 7 disabled tests

### TD_038: Create Architectural Consistency Validation System [Score: 35/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [ARCHITECTURE] [QUALITY] [TOOLING]
**Created**: 2025-08-20

**What**: Design and implement comprehensive consistency checker for post-migration validation
**Why**: Major architecture shift from worktrees to multi-clone needs systematic validation to ensure nothing was missed
**How**:
- Create consistency-checker subagent or slash command
- Validate all references updated (no worktree mentions remain)
- Check persona documentation consistency across all 6 personas
- Verify git workflow references are standard (no Sacred Sequence)
- Validate Clean Architecture boundaries still enforced
- Check Glossary term usage consistency
- Ensure all paths reference correct clone directories
- Verify Context7 integration points still valid

**Done When**:
- Consistency validation tool/subagent created
- Can run full codebase scan in <30 seconds
- Produces structured report of inconsistencies
- Zero false positives on clean codebase
- Catches all migration-related issues
- Integrated into CI/CD pipeline as optional check
- Documentation includes usage examples

**Depends On**: TD_037 (personas must be updated first)

**Problem Context**: We've made a fundamental architectural change (worktrees → clones) and updated many files. We need systematic validation that everything is consistent. Manual checking is error-prone and doesn't scale.

**Reference**: https://github.com/centminmod/my-claude-code-setup demonstrates excellent patterns for custom commands and validation tools we can adapt.

**Tech Lead Decision** (2025-08-21):
- Complexity Score: 35/100 (pattern copying from reference repos, not creating from scratch)
- Decision: Approved with focused scope
- Rationale: Proven patterns exist in centminmod/my-claude-code-setup we can directly adapt
- Implementation: Follow memory-bank-synchronizer pattern for validation
- Key: Not building from scratch - adapting existing successful implementations


