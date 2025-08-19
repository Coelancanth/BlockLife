# BlockLife Development Backlog

**Last Updated**: 2025-08-19

## 📖 How to Use This Backlog

### 🧠 Owner-Based Ultra-Think Protocol

**Each item has a single Owner persona responsible for decisions and progress.**

#### When You Embody a Persona:
1. **Filter** for items where `Owner: [Your Persona]`
2. **Ultra-Think** if `Status: Proposed` and you're the owner (5-15 min deep analysis)
3. **Quick Scan** for other statuses you own (<2 min updates)
4. **Update** the backlog before ending your session
5. **Reassign** owner when handing off to next persona

#### Ultra-Think Triggers:
- **Automatic**: When you own a "Proposed" item
- **Markers**: [ARCHITECTURE], [ROOT-CAUSE], [SAFETY-CRITICAL], [COMPLEX]
- **Output**: Document decision rationale directly in the item

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

### VS_003A: Match-3 with Attributes (Phase 1) [Score: 95/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: S (4-6 hours)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: None

**What**: Match 3+ adjacent same-type blocks to clear them and earn attributes
**Why**: Proves core resource economy loop before adding complexity

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


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### VS_003: Triple-Match System (Original - Archived) [Score: 85/100]
**Status**: Archived - Replaced by VS_003A-D phased approach
**Owner**: N/A (Archived)
**Size**: L (12-16 hours - split into 2 phases)
**Priority**: Ideas
**Created**: 2025-08-19
**Archived**: 2025-08-19
**Depends On**: None (separate from drag/move)

**What**: Original design mixing match and tier-up mechanics without clear separation
**Why**: Creates strategic gameplay with clear, simple rules that players instantly understand

**Note**: Replaced by VS_003A-D phased approach that properly separates:
- VS_003A: Match for resources/attributes
- VS_003B: Tier system introduction
- VS_003C: Unlockable tier-up mechanics
- VS_003D: Cross-type transmutation

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



### TD_009: Refine Persona Command Implementation for Production [Score: 40/100]
**Status**: Approved ✓
**Owner**: DevOps Engineer
**Size**: M (4-6 hours)  
**Priority**: Ideas
**Found By**: DevOps Engineer during persona system testing
**Created**: 2025-08-18

**Tech Lead Decision** (2025-08-18):
✅ APPROVED - Valid improvements but not urgent
- Do after critical items (TD_003, TD_004)
- Focus on error handling and silent mode

**What**: Improve persona command system robustness and user experience
**Why**: Current implementation works but needs refinement for reliable production automation

**Current Issues**:
- Local vs global config precedence unclear and inconsistent
- No error handling for corrupted state files
- Manual testing required to verify which config is active
- Command output verbose for status line usage

**Approach**:
- Add config detection and validation to persona commands
- Implement graceful error handling for missing/corrupted state files
- Create diagnostic commands to show active configuration
- Add silent mode for status line integration (no console output)
- Document config precedence rules clearly

**Technical Improvements**:
- Add `--quiet` flag for status line usage
- Validate .claude/current-persona file format
- Add config source detection (global vs local)
- Implement fallback behavior for missing configs

**Done When**:
- Commands work reliably regardless of config setup
- Clear error messages for configuration issues
- Silent mode works properly with ccstatusline
- Documentation explains config precedence clearly
- No false negatives in persona detection

## 🚧 Currently Blocked
*None*

## ✅ Recently Completed

*No recent completions. Older completed items have been moved to Archive for reference.*

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