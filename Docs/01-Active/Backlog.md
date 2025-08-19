# BlockLife Development Backlog

**Last Updated**: 2025-08-19

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

### BR_009: SaveService Tests Fail on Linux After Newtonsoft.Json Migration
**Status**: Proposed
**Owner**: Test Specialist → Debugger Expert  
**Size**: M (4-6 hours)
**Priority**: Critical
**Created**: 2025-08-19
**Found By**: CI pipeline after TD_024 fix
**Markers**: [PLATFORM-SPECIFIC] [LINUX] [SERIALIZATION] [CI-BLOCKER]

**What**: 4 SaveService tests fail on Linux but pass on Windows after Newtonsoft.Json migration
**Why**: Platform-specific serialization issues break Linux deployments and block CI

**Failed Tests on Linux CI**:
1. `SaveAsync_AlwaysSavesInCurrentVersion` - Load after save fails (IsSucc = False)
2. `LoadAsync_UpdatesLoadCount` - Load fails (IsSucc = False)
3. `LoadAsync_WithOldVersion_ShouldMigrate` - Import fails (IsSucc = False)  
4. `ImportSave_WithValidJson_ShouldDeserialize` - Import fails (IsSucc = False)

**Evidence from CI**:
- PR #45 Build & Test job failed
- All failures show "Expected IsSucc to be True, but found False"
- Tests pass on Windows (local dev) but fail on Ubuntu (CI)
- Issue discovered only after removing `|| true` from CI (TD_024)

**Potential Causes**:
1. Newtonsoft.Json platform-specific behavior with `required` properties
2. File path handling differences (Windows backslash vs Linux forward slash)
3. Line ending differences (CRLF vs LF) in JSON serialization
4. Culture/locale differences affecting date/number formatting
5. Case sensitivity in file system operations

**Investigation Steps**:
1. Add detailed logging to SaveService to capture exact error
2. Check if System.Text.Json works on Linux (might need to revert TD_020)
3. Test Newtonsoft.Json settings for cross-platform compatibility
4. Verify file permissions and path separators
5. Check JSON output differences between platforms

**Done When**:
- All 4 SaveService tests pass on Linux CI
- Root cause documented
- Cross-platform compatibility ensured
- No platform-specific test exclusions needed

**Impact if Not Fixed**:
- CI remains red, blocking all PRs
- Linux deployments completely broken
- Save/Load functionality fails on Linux servers
- Cannot ship to Linux users



## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### TD_026: Fix Agent Path Specifications - Prevent Future Data Loss
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: User during BR_011 root cause analysis
**Markers**: [AGENT-RELIABILITY] [PATH-SPECIFICATION] [DATA-INTEGRITY]

**What**: Backlog-assistant and other agents lack proper path specifications for Archive.md and Post-Mortems
**Why**: Incorrect paths cause data loss, failed operations, and inconsistent file organization

**Root Cause Analysis**:
- Backlog-assistant doesn't know Archive.md is at `Docs/Workflow/Archive.md`
- Post-mortems should go to `Docs/06-PostMortems/Active/` not arbitrary locations
- No centralized path configuration for agents
- Agents may create files in wrong locations or fail to find existing ones

**Technical Approach**:
1. **Update backlog-assistant.md** with correct Archive.md path
2. **Update all persona files** with post-mortem path specification
3. **Create path reference** in agent instructions:
   ```
   CRITICAL PATHS:
   - Archive: Docs/Workflow/Archive.md
   - Post-Mortems: Docs/06-PostMortems/Active/
   - Backlog: Docs/01-Active/Backlog.md
   - Workflow: Docs/01-Active/Workflow.md
   ```
4. **Add validation** - agents should verify paths exist before operations

**Impact if Not Fixed**:
- Future archive operations may fail or lose data
- Post-mortems created in wrong locations
- Inconsistent file organization
- Agent operations unreliable

**Done When**:
- All agent personas have correct path specifications
- Backlog-assistant reliably finds and updates Archive.md
- Post-mortems consistently created in correct directory
- Path validation added to agent operations
- Test: Run archive operation successfully with correct paths

**Related Issues**:
- BR_011: Archive data loss (caused by this issue)
- BR_010: Backlog-assistant incomplete operations (same root cause)

### BR_012: Backlog.md Becoming Too Long - Workflow Refinement Needed
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (workflow design)
**Size**: M (4-6 hours analysis + implementation)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: Dev Engineer during backlog management
**Markers**: [WORKFLOW-EFFICIENCY] [PROCESS-IMPROVEMENT] [SCALABILITY]

**What**: Backlog.md is becoming excessively long, reducing readability and quick decision-making ability
**Why**: Effective backlog management requires quick scanning and prioritization, not reading novellas

**Current Problems**:
- **Excessive detail in backlog items** - Investigation steps, technical plans, root causes taking 20-50 lines each
- **Poor scanning experience** - Cannot quickly identify priorities without scrolling through walls of text
- **Mixed concerns** - Planning details mixed with status tracking
- **Intimidating for stakeholders** - Too much technical detail for quick reviews
- **Maintenance overhead** - Complex items harder to update and reorganize

**Evidence**:
- Current backlog.md is 1000+ lines
- BR items have 30-50 lines each with detailed investigation plans
- Critical section harder to scan due to verbose descriptions
- Recently Completed section accumulating without clear retention policy

**Proposed Workflow Refinements**:

**1. Lean Backlog Items (5-10 lines max)**:
```markdown
### BR_XXX: Short Descriptive Title
**Status**: Proposed | **Owner**: Persona | **Priority**: Level
**What**: One sentence problem description
**Why**: One sentence business impact
**Investigation**: Link to separate investigation doc if needed
**Done When**: 3 bullet points maximum
```

**2. Separate Investigation Documents**:
- Complex items get dedicated files in `Docs/02-Investigations/`
- Backlog references investigation doc, not inline details
- Keeps backlog scannable while preserving detail when needed

**3. Retention Policies**:
- Recently Completed: Max 3 items (auto-archive older ones)
- Important section: Max 10 items (force prioritization)
- Ideas section: Max 15 items (regular pruning)

**4. Status-Only Updates**:
- Backlog updates focus on status changes, not detailed progress
- Technical details go in investigation docs or commit messages
- Keep backlog as decision-making tool, not documentation repository

**5. Template Enforcement**:
- Standardized item templates prevent scope creep
- Clear separation between "what needs doing" vs "how to do it"
- Force conciseness through template constraints

**Benefits**:
- **Faster scanning** - See all priorities at a glance
- **Better decision-making** - Focus on what matters without noise
- **Easier maintenance** - Simple status updates vs complex rewrites
- **Stakeholder friendly** - Non-technical people can understand priorities
- **Scalable** - Process works with 50+ items, not just 10

**Implementation Plan**:
1. **Create item templates** with strict length limits
2. **Migrate verbose items** to investigation docs  
3. **Implement retention policies** with automated archival
4. **Update persona instructions** for lean backlog maintenance
5. **Create backlog health metrics** (item count, average length)

**Done When**:
- Backlog.md under 500 lines total
- Individual items 5-10 lines maximum
- Quick scanning possible for all sections
- Investigation details moved to separate documents
- Retention policies implemented and enforced

### BR_010: Backlog-Assistant Incomplete Archival Operations
**Status**: Proposed
**Owner**: Test Specialist → Debugger Expert
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: Dev Engineer during archival process
**Markers**: [WORKFLOW] [AGENT-RELIABILITY] [BACKLOG-MANAGEMENT]
**Related**: TD_026 (path specifications root cause)

**What**: Backlog-assistant fails to consistently archive all completed items from Recently Completed section
**Why**: Incomplete archival operations leave stale items in active backlog, reducing clarity and organization

**Problem Evidence**:
- Asked to "move completed items" to Archive.md
- Only moved TD_024, left 4 other completed items (BR_007, TD_015, TD_016, BR_006)
- No clear completion verification or status reporting
- Inconsistent interpretation of "all completed items" vs specific items

**Current Archival Failures**:
1. **BR_007**: Completed 2025-08-19, still in Recently Completed
2. **TD_015**: Completed 2025-08-19, still in Recently Completed  
3. **TD_016**: Completed 2025-08-19, still in Recently Completed
4. **BR_006**: Completed 2025-08-19, still in Recently Completed

**Root Causes**:
1. **Task specification ambiguity** - "completed items" vs specific item names
2. **Missing completion verification** - agent doesn't report what was/wasn't moved
3. **No archival criteria defined** - unclear when items should move
4. **Agent scope limitations** - follows literal instructions, misses broader intent

**Impact**:
- Recently Completed section cluttered with old items
- Reduces visibility of truly recent work
- Manual cleanup required after every archival request
- Undermines trust in backlog-assistant reliability

**Investigation Steps**:
1. Review backlog-assistant.md for archival instruction clarity
2. Test various archival command phrasings
3. Check if agent can identify "completed" status programmatically
4. Verify multi-file coordination capabilities (Backlog.md + Archive.md)

**Proposed Solutions**:
1. **Clear archival protocol** in agent instructions
2. **Completion verification** checklist in agent responses  
3. **Status reporting** - list what was moved and what remains
4. **Retention policy** - Recently Completed keeps max 3 items

**Done When**:
- Backlog-assistant consistently moves ALL completed items when requested
- Agent reports completion status clearly
- Manual cleanup no longer required after archival operations
- Archival protocol documented and tested

### BR_008: Investigate Flaky SimulationManagerThreadSafetyTests.ConcurrentOperations Test
**Status**: Proposed
**Owner**: Test Specialist → Debugger Expert
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Markers**: [TEST-FLAKINESS]

**What**: SimulationManagerThreadSafetyTests.ConcurrentOperations_ShouldMaintainPerformance fails intermittently
**Why**: Flaky tests reduce confidence in CI/CD pipeline and mask real issues

**Symptoms**:
- Test failed during TD_020A implementation but seems unrelated to SaveService changes
- Stress test involving concurrent operations
- Performance-based assertion that might be timing-sensitive
- Test output: "[xUnit.net 00:00:00.33] BlockLife.Core.Tests.StressTests.SimulationManagerThreadSafetyTests.ConcurrentOperations_ShouldMaintainPerformance [FAIL]"

**Initial Investigation Needed**:
1. Check if test has history of flakiness
2. Review performance thresholds - might be too tight
3. Consider machine-dependent factors (CPU load, etc.)
4. May need retry logic or loosened constraints

**Done When**:
- Root cause identified
- Test either fixed or marked with appropriate attributes
- No false positives in CI
- Documentation of why test was flaky

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


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

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

### TD_025: Add Verification Protocol for Agent Task Completion
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Markers**: [QUALITY-CONTROL] [AGENT-VERIFICATION]

**What**: Implement verification checklist for agent-completed tasks, especially backlog-assistant
**Why**: Agents may partially complete tasks without clear indication of what was/wasn't done

**Problem Observed**:
- Backlog-assistant was asked to move TD_020 and clean up duplicates
- It moved TD_020 to Archive.md correctly
- It did NOT add TD_020 to Recently Completed section
- Manual intervention was needed to complete the task
- No clear report of what was/wasn't completed

**Proposed Solution**:
1. **Agent Completion Checklist**:
   - Pre-task: Document expected outcomes
   - During: Track each sub-task completion
   - Post-task: Verify all outcomes achieved
   - Report: Clear summary of completed/incomplete items

2. **Backlog-Assistant Specific Checks**:
   - Items removed from original location?
   - Items added to target location?
   - Duplicates removed?
   - Formatting preserved?
   - Both Backlog.md AND Archive.md updated?
   - Chronological order maintained?

3. **Implementation Ideas**:
   - Add verification prompts to agent personas
   - Create standard verification templates
   - Document common failure patterns
   - Add "verify completion" step to workflows

**Done When**:
- Verification protocol documented in agent personas
- Checklist templates created for common operations
- Backlog-assistant specifically updated with multi-file awareness
- Test case: Successfully moves item between Backlog.md and Archive.md completely

### TD_023: Implement Persona Worktree System - Automated Isolation Workspaces ✅ APPROVED (MODIFIED)
**Status**: Approved - Phased Implementation
**Owner**: Dev Engineer
**Size**: S (2 hours Phase 1) + M (4 hours total if expanded)
**Priority**: Important (moved up - actual pain point)
**Created**: 2025-08-19
**Reviewed**: 2025-08-19
**Markers**: [PRODUCTIVITY] [SOLO-DEV]

**What**: Automated persona workspace isolation using git worktrees
**Why**: Solo dev with frequent persona switching experiencing real friction with standard git flow

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED WITH MODIFICATIONS**

**Context That Changed Decision**:
- Solo developer, not team (different dynamics)
- Frequent persona switching confirmed
- Standard git flow already tried and insufficient
- Actual productivity impact measured

**Phased Implementation**:

**Phase 1** (2 hours - START HERE):
```powershell
# Simple switch script
./scripts/persona/switch-persona.ps1 dev-engineer
# Creates worktree if needed, switches context
```
- Just Dev Engineer + Tech Lead personas
- Basic PowerShell script
- Manual invocation
- Measure if it helps

**Phase 2** (If Phase 1 proves valuable):
- Add remaining personas
- State management
- Cleanup utilities

**Phase 3** (If heavily used):
- Slash command integration
- Cross-platform support
- Full automation

**Success Metrics**:
- Reduces context switch time by >50%
- Eliminates merge conflicts from persona switches
- Actually gets used daily

**Done When (Phase 1)**:
- Basic switch script working
- Two personas supported
- Documentation created
- Friction measurably reduced

**Problem Solved**:
- Multiple persona sessions conflict on same branch (file locks, merge conflicts)
- Manual branch switching between personas is error-prone
- Context switching overhead reduces persona effectiveness
- Coordination complexity grows exponentially with concurrent personas

**Solution - Persona Worktree System**:
- Single command `/embody dev-engineer` automatically creates worktree + switches directory + activates persona
- Each persona gets isolated workspace using native git worktree functionality
- Zero maintenance overhead (git handles all complexity)
- Perfect isolation - no shared files, branches, or state

**Key Benefits**:
- **Complete Isolation**: No more file conflicts between persona sessions
- **Zero Context Switch**: Instant persona activation with full environment
- **Native Git**: Uses proven git worktree functionality (stable, zero bugs)
- **Single Command**: `/embody persona-name` handles everything automatically
- **9 Hours Total**: vs weeks for complex coordination systems

**Implementation Components**:
1. **Custom Slash Command** (2h): `/embody` command with persona parameter
2. **Worktree Management** (3h): Create, switch, cleanup worktree operations
3. **Cross-Platform Scripts** (2h): Bash + PowerShell automation scripts
4. **Management Utilities** (1h): Status, cleanup, workspace listing commands
5. **Documentation** (1h): Usage guide and troubleshooting

**Technical Approach**:
```bash
# Single command does everything:
/embody dev-engineer
  → git worktree add ../blocklife-dev-engineer
  → cd ../blocklife-dev-engineer
  → activate dev-engineer persona
  → ready for isolated development
```

**Reference**: Comprehensive design document at `Docs/02-Design/PersonaWorktreeSystem.md`

**Done When**:
- `/embody persona-name` creates isolated workspace automatically
- Cross-platform support (Windows/Linux/Mac)
- Workspace management commands (status, cleanup, list)
- Complete persona isolation achieved
- Documentation and troubleshooting guide complete
- 5+ tests for worktree operations

**Depends On**: None

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


## 🚧 Currently Blocked
*None*

## ✅ Recently Completed

### BR_011: Critical Archive Data Loss ✅ COMPLETED
**Completed**: 2025-08-19
**Owner**: Debugger Expert
**Effort**: M (4 hours actual)
**Outcome**: 100% data recovery achieved, archive reconstructed with safeguards
**Impact**: Restored organizational memory, prevented future data loss
**Post-Mortem**: `Docs/06-PostMortems/Active/2025-08-19-BR011-Archive-Data-Loss.md`
**Follow-up**: TD_026 addresses root cause (agent path specifications)

### BR_007: Backlog-Assistant Automation Misuse ✅ COMPLETED
**Completed**: 2025-08-19
**Owner**: Completed
**Effort**: S (2 hours)
**Original Issue**: Personas were automatically calling backlog-assistant instead of user explicitly invoking it, bypassing review process
**Solution**: Fixed inconsistent backlog-assistant invocation patterns across ALL persona documentation
**Impact**: Review process integrity restored, user control over backlog changes maintained, Tech Lead review gates properly enforced

**Implementation Details**:
- Fixed inconsistent backlog-assistant invocation patterns across ALL persona documentation
- Updated 5 persona files to use "Suggest-Don't-Execute" pattern instead of auto-invocation
- Preserved Strategic Prioritizer exception for meta-analysis functions
- Updated Workflow.md and CLAUDE.md with corrected protocol
- Root cause eliminated: Documentation drift between personas resolved

**Impact Assessment**:
- ✅ Review process integrity restored
- ✅ User control over backlog changes maintained  
- ✅ Efficiency benefits preserved through suggestion pattern
- ✅ Clear documentation prevents regression
- ✅ Tech Lead review gates properly enforced

**Files Updated**:
- Docs/04-Personas/tech-lead.md
- Docs/04-Personas/test-specialist.md  
- Docs/04-Personas/debugger-expert.md
- Docs/04-Personas/devops-engineer.md
- Docs/04-Personas/product-owner.md
- Docs/01-Active/Workflow.md
- CLAUDE.md

### TD_015: Add Save System Versioning ✅ COMPLETED
**Completed**: 2025-08-19
**Owner**: Dev Engineer
**Effort**: XS (30 minutes)
**Implementation**: Added version field to SaveData with migration framework for future compatibility
**Impact**: Save system now protected against format changes, prevents player data loss during updates
**Key Components**: Version field, migration pattern, test coverage for v0→v1 transitions

### TD_016: Document Grid Coordinate System ✅ COMPLETED
**Completed**: 2025-08-19
**Owner**: Dev Engineer
**Effort**: XS (15 minutes)
**Implementation**: Documented coordinate convention and added validation helpers
**Impact**: Eliminates coordinate confusion bugs, standardizes grid access patterns
**Key Components**: Architecture.md documentation, GridAssert helper class, consistent (0,0) bottom-left convention

### BR_006: Parallel Incompatible Features Prevention System ✅ RESOLVED
**Completed**: 2025-08-19
**Owner**: DevOps Engineer
**Effort**: M (4-6 hours)
**Solution**: Comprehensive automated prevention system with branch protection, VS locking, and workflow enforcement
**Impact**: Eliminates parallel incompatible development, prevents wasted effort, ensures design consistency
**Key Components**: GitHub branch protection + Design Guard Action + PR templates + Git hooks + updated GitWorkflow.md

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