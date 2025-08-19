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