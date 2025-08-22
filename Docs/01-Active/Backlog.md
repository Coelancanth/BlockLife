# BlockLife Development Backlog

**Last Updated**: 2025-08-22
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 014 (Last: BR_013 - 2025-08-22)
- **Next TD**: 066 (Last: TD_065 - 2025-08-22)  
- **Next VS**: 004 (Last: VS_003D - 2025-08-19)

**Protocol**: Check your type's counter → Use that number → Increment the counter → Update timestamp

## 📖 How to Use This Backlog

### 🧠 Owner-Based Protocol

**Each item has a single Owner persona responsible for decisions and progress.**

#### When You Embody a Persona:
1. **Filter** for items where `Owner: [Your Persona]`
3. **Quick Scan** for other statuses you own (<2 min updates)
4. **Update** the backlog before ending your session
5. **Reassign** owner when handing off to next persona


### Default Ownership Rules
| Item Type | Status | Default Owner | Next Owner |
|-----------|--------|---------------|------------|
| **VS** | Proposed | Product Owner | → Tech Lead (breakdown) |
| **VS** | Approved | Tech Lead | → Dev Engineer (implement) |
| **BR** | New | Test Specialist | → Debugger Expert (complex) |
| **TD** | Proposed | Tech Lead | → Dev Engineer (approved) |

### Pragmatic Documentation Approach
- **Quick items (<1 day)**: 5-10 lines inline below
- **Medium items (1-3 days)**: 15-30 lines inline (like VS_001-003 below)
- **Complex items (>3 days)**: Create separate doc and link here

**Rule**: Start inline. Only extract to separate doc if it grows beyond 30 lines or needs diagrams.

### Adding New Items
```markdown
### [Type]_[Number]: Short Name
**Status**: Proposed | Approved | In Progress | Done
**Owner**: [Persona Name]  ← Single responsible persona
**Size**: S (<4h) | M (4-8h) | L (1-3 days) | XL (>3 days)
**Priority**: Critical | Important | Ideas
**Markers**: [ARCHITECTURE] [SAFETY-CRITICAL] etc. (if applicable)

**What**: One-line description
**Why**: Value in one sentence  
**How**: 3-5 technical approach bullets (if known)
**Done When**: 3-5 acceptance criteria
**Depends On**: Item numbers or None

**[Owner] Decision** (date):  ← Added after ultra-think
- Decision rationale
- Risks considered
- Next steps
```

## 🔥 Critical (Do First)
*Blockers preventing other work, production bugs, dependencies for other features*

### BR_013: CI Workflow Fails on Main Branch After Merge
**Status**: Done
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Critical
**Markers**: [CI/CD] [WORKFLOW] [BRANCH-PROTECTION]
**Created**: 2025-08-22

**What**: CI workflow falsely fails on main branch pushes due to incorrect job dependencies
**Why**: Post-merge CI failures create false alarms and mask real issues
**Root Cause**: `build-and-test` job depends on `branch-freshness`, but `branch-freshness` only runs on PRs (skipped on main), causing `build-and-test` to be skipped, which triggers failure in `ci-status`
**Impact**: Makes it appear that merges are bypassing failing CI when protection is actually working correctly

**How**: 
- Remove `branch-freshness` from `build-and-test` dependencies
- Add conditional logic to only run when `quick-check` succeeds
- Enhanced logging in `ci-status` job for better debugging
- Maintain proper branch protection (only "Build & Test" required for PRs)

**Done When**:
- CI workflow runs successfully on main branch pushes
- `build-and-test` job runs independently of `branch-freshness` 
- Branch protection rules remain intact
- Post-merge CI failures eliminated

**DevOps Engineer Decision** (2025-08-22):
- **FIXED**: Updated `.github/workflows/ci.yml` with corrected dependencies
- **Tested**: Local build passes, workflow logic verified
- **Root cause**: Workflow design flaw, not branch protection issue
- **Impact**: Eliminates false CI failure alarms after legitimate merges



## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*


### TD_048: Migrate FsCheck Property-Based Tests to 3.x API [Score: 30/100]
**Status**: Approved
**Owner**: Test Specialist
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [TESTING] [MIGRATION] [FSCHECK] [PROPERTY-BASED]
**Created**: 2025-08-22
**Reset**: 2025-08-22 (Aging protocol started today)

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



### VS_003A: Match-3 with Attributes (Phase 1) [Score: 95/100]
**Status**: Approved
**Owner**: Tech Lead → Dev Engineer
**Size**: M (6.5 hours - updated estimate)
**Priority**: Important
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging clock starts fresh from today)
**Last Updated**: 2025-08-22
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



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_065: Automate Memory Bank Rotation
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Ideas
**Created**: 2025-08-22

**What**: Create automated rotation scripts for Memory Bank files per protocol
**Why**: Manual rotation is error-prone and likely to be forgotten
**How**:
- PowerShell script for monthly session-log rotation
- PowerShell script for quarterly active context archival
- Git hook or scheduled task to trigger rotations
- Auto-create archive directory structure
- Keep last N archives, delete older ones
**Done When**:
- Scripts rotate files according to MEMORY_BANK_PROTOCOL.md
- Old archives are cleaned up automatically
- Can be triggered manually or via schedule
**Depends On**: None

**Tech Lead Notes** (2025-08-22):
- Complexity Score: 2/10 - straightforward scripting
- Pattern match: Similar to existing embody.ps1 file management
- Low priority - manual rotation works fine for now







## 🚧 Currently Blocked
*None*


---

## 📋 Quick Reference

**Priority Decision Framework:**
1. **Blocking other work?** → 🔥 Critical
2. **Current milestone?** → 📈 Important  
3. **Everything else** → 💡 Ideas

**Work Item Types:**
- **VS_xxx**: Vertical Slice (new feature) - Product Owner creates
- **BR_xxx**: Bug Report (investigation) - Test Specialist creates, Debugger owns
- **TD_xxx**: Technical Debt (refactoring) - Anyone proposes → Tech Lead approves

*Notes:*
- *Critical bugs are BR items with 🔥 priority*
- *TD items need Tech Lead approval to move from "Proposed" to actionable*

---
*Single Source of Truth for all BlockLife development work. Simple, maintainable, actually used.*