# Backlog Backup

> Items on probation - will be deleted after 10 days total unless rescued
> See BACKLOG_AGING_PROTOCOL.md for rules
> **Last Cleaned**: 2025-08-22
> **Aging Reset**: 2025-08-22 (all items start fresh from today)

## 📊 Aging Statistics
- Items in backup: 10 (VS: 4, TD: 6)
- Oldest item: All reset to 2025-08-22
- Next move to deletion: 2025-09-01 (10 days from reset)

## ⏰ Active Backup Items (Reset to Day 0 on 2025-08-22)

*These items have 10 days total lifetime from reset date before automatic deletion*

### VS_003B: Tier System Introduction [Score: 80/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: S (4-6 hours)
**Priority**: Important
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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

### VS_003D: Cross-Type Transmutation System [Score: 60/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: M (6-8 hours)
**Priority**: Ideas (future)
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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

### VS_004: Auto-Spawn System [Score: 75/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (for breakdown)
**Size**: S (4-6 hours - straightforward mechanic)
**Priority**: Important
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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

---

## 🔧 Technical Debt Items

### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 40/100]
**Status**: Approved - Immediate Part Ready
**Owner**: Test Specialist  
**Size**: XS (immediate) + M (future property suite)
**Priority**: Ideas (not critical path)
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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

### TD_017: Create Centralized Turn Manager
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer  
**Size**: S (2-3 hours)
**Priority**: Important
**Created**: 2025-08-19
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
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
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)

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

### TD_038: Create Architectural Consistency Validation System [Score: 35/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Important
**Created**: 2025-08-20
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
**Markers**: [ARCHITECTURE] [QUALITY] [TOOLING]

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

### TD_061: Automated Link Integrity Checking [Score: 20/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Ideas
**Created**: 2025-08-22
**Reset**: 2025-08-22 (Aging protocol started)
**Will Delete**: 2025-09-01 (unless rescued)
**Markers**: [TOOLING] [DOCUMENTATION] [QUALITY]

**What**: Create intelligent link checking script with context-aware fix/remove suggestions
**Why**: Frequent doc moves create broken links; deprecated docs need different handling than simple moves
**How**:
- Parse all .md files for markdown links using regex
- Verify each linked file exists at specified path
- Smart suggestions based on destination:
  - If moved to 99-Deprecated/ → Suggest removal or replacement
  - If moved elsewhere → Suggest path update
  - If deleted → Suggest removal with warning
- Check for non-deprecated alternatives when suggesting removal
- Optional auto-fix mode with user confirmation
- Integrate as pre-push warning (non-blocking)

**Done When**:
- Script detects all broken markdown links
- Provides context-aware suggestions (fix/remove/replace)
- Handles deprecation patterns intelligently
- Integrated into workflow as pre-push warning
- Zero false positives on valid links
- Documentation updated with usage instructions

**Depends On**: None

**Problem Context**: Recent doc reorganizations (moving files to 99-Deprecated/) broke multiple links in CLAUDE.md and other docs. Manual link maintenance is error-prone. Need automated detection and correction suggestions.

**Tech Lead Note** (2025-08-22):
- Created after rejecting Foam as over-engineered solution
- Directly addresses the broken links problem without adding complexity
- Compatible with AI persona workflow (CLI-based)
- Maintenance discipline tool, not new linking system
- Enhanced with deprecation intelligence - knows when to remove vs update
- Context-aware suggestions based on file destination (99-Deprecated/ = remove)

---

## 🚫 Won't Fix / Deprecated

*Items explicitly decided against - kept for reference*

(None currently)

## 📜 Deletion Log

*Items that aged out - kept for 30 days for reference*

### 2025-08-22
- Removed VS_003A (duplicate - active copy exists in Backlog.md)

---

## Rescue Instructions

To rescue an item back to active Backlog:
1. Add **Rescued** timestamp with justification
2. Update priority to Important or Critical
3. Assign clear owner
4. Move entire item back to Backlog.md
5. Delete from this file

Example:
```markdown
**Rescued**: 2025-08-23 - Critical for next milestone
**New Owner**: Dev Engineer
**Commitment**: Will start tomorrow
```