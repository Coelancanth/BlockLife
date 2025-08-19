# BlockLife Development Backlog

**Last Updated**: 2025-08-20

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




## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### TD_026: Fix Agent Path Specifications - Prevent Future Data Loss
**Status**: Approved
**Owner**: Dev Engineer
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: User during BR_011 root cause analysis
**Markers**: [AGENT-RELIABILITY] [PATH-SPECIFICATION] [DATA-INTEGRITY]

**Tech Lead Decision** (2025-08-20):
✅ **APPROVED** - Complexity: 2/10
- Real operational issue causing data loss
- Simple fix: hardcode correct paths in agent files
- 2-hour implementation prevents future debugging
- No over-engineering needed

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

### TD_027: Enforce Persona Subagent Protocol - Suggest Only, Never Auto-Execute
**Status**: Approved
**Owner**: Dev Engineer
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-20
**Proposed By**: Tech Lead (based on BR_011 violation)
**Markers**: [WORKFLOW] [PROTOCOL] [PERSONA-SYSTEM]

**What**: Update all persona files to enforce "suggest-only" protocol for subagent usage
**Why**: Prevent automation misuse and maintain user control over all agent operations

**Tech Lead Decision** (2025-08-20):
✅ **APPROVED** - Complexity: 2/10
- Addresses real process violation (BR_011)
- Simple documentation updates across persona files
- Prevents future trust erosion and automation overreach
- Clear implementation path

**Problem Analysis**:
- BR_011 showed Tech Lead violated Transparent Delegation Protocol
- Current personas have buried delegation rules (line 270+ in files)
- Risk of other personas making similar violations
- User loses control when personas auto-execute subagents

**Technical Approach**:
1. **Add protocol header** to all persona files (6 personas):
   ```markdown
   ## 🚨 SUBAGENT PROTOCOL - CRITICAL
   **PERSONAS MUST SUGGEST, NEVER AUTO-EXECUTE**
   - ❌ NEVER invoke Task tool directly for subagents
   - ✅ ALWAYS propose specific commands to user first
   - ✅ Wait for explicit user approval before any delegation
   - Example: "I suggest updating backlog via: [command preview]. Approve? (yes/no)"
   ```

2. **Update existing delegation sections** in each persona file
3. **Test with protocol compliance check** - try to trigger violations
4. **Update CLAUDE.md** with persona protocol reminder

**Files to Update** (6 total):
- `Docs/04-Personas/product-owner.md`
- `Docs/04-Personas/tech-lead.md`  
- `Docs/04-Personas/dev-engineer.md`
- `Docs/04-Personas/test-specialist.md`
- `Docs/04-Personas/debugger-expert.md`
- `Docs/04-Personas/devops-engineer.md`

**Done When**:
- All 6 persona files have prominent protocol headers
- Existing delegation sections updated for consistency
- CLAUDE.md updated with protocol reminder
- Test: Personas suggest rather than auto-execute subagents
- Protocol violations impossible to miss

**Related Issues**:
- BR_011: Tech Lead protocol violation (root cause)
- BR_007: Previous backlog-assistant automation issues

### TD_028: Remove "Recently Completed" Section from Backlog Management
**Status**: Approved  
**Owner**: Dev Engineer
**Size**: S (1-2 hours)
**Priority**: Important
**Created**: 2025-08-20
**Proposed By**: Tech Lead (workflow improvement)
**Markers**: [WORKFLOW] [BACKLOG-SIMPLIFICATION]

**What**: Update backlog-assistant protocol to skip "Recently Completed" section entirely
**Why**: Recently Completed adds no value and creates maintenance overhead

**Tech Lead Decision** (2025-08-20):
✅ **APPROVED** - Complexity: 1/10
- Eliminates unnecessary workflow step
- Reduces cognitive load in backlog management
- Archive.md provides complete completion history
- Simple documentation update

**Current Problem**:
- Recently Completed section requires manual maintenance
- Adds extra step in archival process (BR_010 related)
- No functional value - Archive.md is single source of truth
- Creates confusion about what belongs where

**Technical Approach**:
1. **Update backlog-assistant.md** protocol:
   ```markdown
   ## ARCHIVAL PROTOCOL (UPDATED)
   ❌ OLD: Move to Recently Completed first, then Archive later
   ✅ NEW: Move completed items directly to Archive.md
   
   - Skip "Recently Completed" section entirely
   - Archive.md is the ONLY destination for completed work
   - Preserve all metadata when archiving
   ```

2. **Update Backlog.md template** - remove Recently Completed section
3. **Archive existing items** in Recently Completed (if any)
4. **Test archival workflow** - verify direct-to-archive works

**Done When**:
- Backlog-assistant protocol updated to skip Recently Completed
- Recently Completed section removed from Backlog.md
- Any existing items in Recently Completed archived properly
- Archival workflow simplified and tested

**Related Issues**:
- BR_010: Incomplete archival operations (complexity reduction)
- TD_026: Agent path specifications (consistency improvement)

### TD_029: Update Context7 Documentation for Newtonsoft.Json Migration
**Status**: Approved
**Owner**: Dev Engineer  
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-20
**Proposed By**: Tech Lead (based on BR_009 findings)
**Markers**: [CONTEXT7] [SERIALIZATION] [DOCUMENTATION]

**What**: Update Context7 documentation to reflect our switch from System.Text.Json to Newtonsoft.Json
**Why**: Prevent future serialization bugs from incorrect Context7 advice based on wrong JSON library

**Tech Lead Decision** (2025-08-20):
✅ **APPROVED** - Complexity: 2/10
- Prevents real serialization bugs
- Context7 is critical for preventing assumption errors
- Simple documentation updates with clear examples
- Addresses gap revealed by BR_009 migration

**Problem Analysis**:
- **BR_009**: We migrated SaveService to Newtonsoft.Json due to `required` property issues
- **Current Risk**: Context7 docs may still reference System.Text.Json patterns
- **Bug Prevention**: Future Context7 queries need correct serialization examples
- **Cost of Wrong Advice**: 2+ hours debugging serialization issues

**Technical Approach**:
1. **Update Context7Examples.md**:
   - Add Newtonsoft.Json section with common patterns
   - Update serialization examples to use Newtonsoft patterns
   - Add JsonConstructor attribute examples (from BR_009 fix)
   - Document `required` property handling

2. **Update Context7LibraryMatrix.md**:
   - Add Newtonsoft.Json to Tier 2 libraries if available in Context7
   - Note System.Text.Json is NOT used in BlockLife
   - Update serialization risk assessment

3. **Add Migration Notes**:
   ```markdown
   ## ⚠️ SERIALIZATION LIBRARY CHANGE
   **BlockLife uses Newtonsoft.Json, NOT System.Text.Json**
   
   Migration Completed: 2025-08-19 (BR_009)
   Reason: C# 11 `required` property compatibility on Linux
   
   When querying Context7 for serialization advice:
   - Request Newtonsoft.Json patterns specifically
   - Ignore System.Text.Json recommendations
   - Focus on JsonConstructor and JsonProperty attributes
   ```

**Files to Update**:
- `Docs/03-Reference/Context7/Context7Examples.md`
- `Docs/03-Reference/Context7/Context7LibraryMatrix.md`
- Add examples from BR_009 fix (JsonConstructor pattern)

**Done When**:
- Context7 docs reflect Newtonsoft.Json usage
- Examples show JsonConstructor patterns from BR_009
- Clear warning about NOT using System.Text.Json
- Future Context7 queries will return correct advice
- No risk of reverting to wrong serialization library

**Related Issues**:
- BR_009: Linux CI failure due to Newtonsoft migration (source of change)
- Future serialization work will query Context7 correctly

### BR_010: Backlog-Assistant Incomplete Archival Operations
**Status**: Proposed
**Owner**: Debugger Expert
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: Dev Engineer during archival process
**Markers**: [WORKFLOW] [AGENT-RELIABILITY] [BACKLOG-MANAGEMENT]
**Related**: TD_026 (path specifications root cause)

**Tech Lead Assignment** (2025-08-20): Assigned to Debugger Expert for agent behavior investigation

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

### BR_011: Tech Lead Violated Transparent Delegation Protocol
**Status**: Proposed
**Owner**: Test Specialist → Debugger Expert
**Size**: S (1-2 hours)
**Priority**: Important
**Created**: 2025-08-20
**Found By**: User during Tech Lead backlog update
**Markers**: [WORKFLOW] [PROTOCOL-VIOLATION] [AGENT-MISUSE]

**What**: Tech Lead persona attempted to invoke backlog-assistant without following Transparent Delegation Protocol
**Why**: Protocol violations erode user trust and create automation misuse risks

**Problem Evidence**:
- Tech Lead was updating backlog with technical decisions
- Attempted direct `Task` invocation with backlog-assistant
- DID NOT show command preview to user first
- DID NOT request explicit approval before execution
- Violated lines 270-328 of tech-lead.md protocol

**Root Cause Analysis**:
1. **Missing safeguard**: No automatic check for delegation protocol compliance
2. **Persona confusion**: Tech Lead acted like it had autonomous execution rights
3. **Protocol buried**: Delegation protocol is deep in the persona file (line 270+)

**Impact**:
- User loses control over what changes are made
- Risk of incorrect or incomplete updates
- Undermines trust in persona system
- Sets bad precedent for other personas

**Correct Behavior (per protocol)**:
```markdown
BEFORE delegation:
1. Summarize intent
2. Show exact command preview
3. List expected outcomes
4. Request approval: "Do you approve this delegation?"
5. Wait for explicit "yes" before executing
```

**Done When**:
- Protocol violation documented
- Fix implemented to prevent recurrence
- All personas updated with clear delegation requirements
- Test coverage for protocol compliance

**Related Issues**:
- BR_007: Previous automation misuse with backlog-assistant
- TD_026: Agent specification improvements (could include protocol enforcement)

### BR_008: Investigate Flaky SimulationManagerThreadSafetyTests.ConcurrentOperations Test
**Status**: Proposed
**Owner**: Debugger Expert
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Markers**: [TEST-FLAKINESS]

**Tech Lead Assignment** (2025-08-20): Assigned to Debugger Expert for timing analysis

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
**Owner**: Dev Engineer
**Size**: M (6.5 hours - updated estimate)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: None

**What**: Match 3+ adjacent same-type blocks to clear them and earn attributes
**Why**: Proves core resource economy loop before adding complexity

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED for implementation with Pattern Recognition Architecture**

**Implementation Guide Verified** (2025-08-20): Dev-ready with complete contracts, tests, and edge cases documented

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