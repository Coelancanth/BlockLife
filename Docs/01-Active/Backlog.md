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

### VS_003: Triple-Match Merging (Same-Type Only) [Score: 85/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (for breakdown)
**Size**: M (8-10 hours - new mechanic, not simple extension)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: None (separate from drag/move)

**What**: Implement Triple Town-style merging - 3 adjacent same-type blocks merge into higher level
**Why**: Creates strategic gameplay with clear, simple rules that players instantly understand

**Core Mechanic** (Triple Town Model):
- **3+ adjacent blocks** of same type (orthogonal only, not diagonal)
- **Merge triggers** when player moves a block to complete a group of 3+
- **Result appears** at the last-moved block position
- **Level progression**: Three Level-1 blocks → One Level-2 block

**Implementation Approach**:
- Create IMergeDetector interface for future extensibility
- Implement AdjacentSameTypeMergeDetector (uses flood-fill internally)
- Detect merges after any move/place action via detector
- Remove all but last-moved block from detected group
- Transform last-moved block to next level
- Add score based on merge (Level 1 merge = 10pts, Level 2 = 30pts, etc.)

**Done When**:
- Moving a block to form 3+ adjacent same-type triggers merge
- Merged block appears at last-moved position
- Other blocks in merge group disappear with visual effect
- Score increases and displays (simple text for now)
- Works for all 9 block types (same-type only)
- 5+ unit tests covering merge detection and execution
- 3+ unit tests for scoring calculation

**NOT in Scope** (VS_004+ territory):
- Auto-spawn system (that's next)
- Cross-type merging (2 Work + 1 Study combinations)
- Chain reactions/cascading merges
- Merge animations beyond simple disappear
- Special effects or particles
- Combo multipliers

**Product Owner Ultra-Think Notes**:
- This is MORE complex than I initially thought - it's a new core mechanic
- Triple Town model proven fun for 10+ years
- Same-type only keeps it learnable (9 rules vs 100+ combinations)
- Must feel responsive - merge detection after EVERY action
- **Architecture Decision**: IMergeDetector interface prevents lock-in to flood-fill
- Future rule engine can swap detector implementation without touching other code
- This is strategic abstraction - minimal cost now, huge flexibility later

### VS_004: Auto-Spawn System [Score: 75/100]
**Status**: Proposed
**Owner**: Product Owner → Tech Lead (for breakdown)
**Size**: S (4-6 hours - straightforward mechanic)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003 (need merging to manage spawned blocks)

**What**: Automatically spawn new blocks after player actions to create natural game pressure
**Why**: Transforms sandbox into challenging game with failure state and strategic decisions

**Core Mechanic**:
- **Spawn Trigger**: After EVERY player action (move, place, merge)
- **Spawn Count**: 1 block per turn (adjustable for difficulty later)
- **Spawn Position**: Random empty tile
- **Spawn Type**: Random from available block types (weighted distribution)
- **Game Over**: When spawn fails due to no empty tiles

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
- Spawns only on empty tiles
- Visual indication of newly spawned block
- Game over triggers when grid is full
- Game over screen with final score
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
**Size**: M (6-8 hours - builds on VS_003 foundation)
**Priority**: Important
**Created**: 2025-08-19
**Depends On**: VS_003 (need basic merging first)

**What**: Add cascading merges that trigger automatically, with exponential scoring multipliers
**Why**: Creates the addictive "YES!" moments that separate good puzzle games from great ones

**Core Mechanic**:
- **Cascade Trigger**: After any merge completes, check if result can trigger new merge
- **Recursive Detection**: Each cascade can trigger another cascade
- **Multiplier System**: Base × 1 → ×2 → ×4 → ×8 → ×16 (exponential)
- **Chain Counter**: Display "Chain ×2!", "Chain ×3!" etc.
- **Celebration**: Bigger effects for longer chains

**Implementation Approach**:
- After merge completes, run merge detection on result position
- If new merge detected, execute it with increased multiplier
- Continue recursively until no more merges possible
- Track chain depth for scoring and display
- Add brief delay between cascades for visual clarity

**Scoring Formula**:
```
Chain 1: 10 points × 1 = 10
Chain 2: 10 points × 2 = 20
Chain 3: 10 points × 4 = 40
Chain 4: 10 points × 8 = 80
Total for 4-chain: 150 points!
```

**Done When**:
- Merges automatically trigger follow-up merges
- Score multiplier increases exponentially per chain
- Chain counter displays during cascades
- Visual delay between cascade steps (player can follow what happened)
- Different sound effects for each chain level
- 5+ unit tests for cascade detection
- 3+ tests for multiplier calculation
- 2+ tests for recursive cascade limits

**NOT in Scope**:
- Special chain-only blocks
- Chain preview/planning UI
- Undo for chains
- Chain-specific animations (use simple for now)
- Maximum chain bonuses/achievements
- Chain-triggered special events

**Critical Design Decisions**:
- **Delay Between Cascades**: 0.3-0.5 seconds (fast enough to feel smooth, slow enough to see)
- **Max Chain Depth**: Unlimited (let players find crazy combos)
- **Multiplier Cap**: No cap initially (see how high players can go)

**Product Owner Notes**:
- This is THE feature that makes match-3 games addictive
- Must feel satisfying - sound/visual feedback crucial
- Exponential scoring rewards elaborate setups
- Creates skill gap between new and experienced players
- Watch for degenerate strategies (infinite chains)



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### VS_002: Translate Creative Brainstorms into Feature Specifications [Score: 30/100]
**Status**: ✅ Completed
**Owner**: Product Owner
**Size**: L (2-3 days)
**Priority**: Ideas
**Created**: 2025-08-19
**Completed**: 2025-08-18
**Depends On**: None

**What**: Translate Chinese brainstorming content into actionable VS items and feature specifications
**Why**: Valuable creative vision needs to be transformed into implementable features

**COMPLETED WORK**:
- ✅ Reviewed all brainstormA/B/C.md files and extracted key concepts
- ✅ Successfully consolidated all creative content into Vision.md
- ✅ Added new systems: Character Origins & Talents, Regional Gameplay, Memory Palace
- ✅ Integrated Inner Monologue System, Narrative Anchor Choices, Time Budget System
- ✅ Added Game Modes (Authentic/Destiny/Legacy) and Generational Legacy System
- ✅ Removed redundant brainstorm files after consolidation
- ✅ Updated documentation references for consistency

**Key Features Integrated**:
- ✅ Emotional time flow (Dynamic Time Scale)
- ✅ Relationship blocks with visual bonds (Bond Links)
- ✅ Personality development through actions (MBTI System)
- ✅ Life stage-specific block types (All Life Stages)
- ✅ Legacy and memory systems (Memory Palace, Generational Legacy)

**Product Owner Note**: All valuable creative vision from Chinese brainstorms has been successfully integrated into the consolidated Vision.md. The game design is now comprehensive and ready for implementation prioritization.

### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 45/100]
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