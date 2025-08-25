# BlockLife Development Backlog

**Last Updated**: 2025-08-25 16:26
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 014 (Last: BR_013 - 2025-08-22)
- **Next TD**: 081 (Last: TD_080 - 2025-08-25 19:31)  
- **Next VS**: 005 (Last: VS_003B-4 - 2025-08-25 18:50)

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

*None currently*

## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### VS_003B: Merge System - Progressive Tier Progression
**Status**: ~~Proposed~~ **Rejected - Too Large**
**Owner**: ~~Product Owner → Tech Lead~~
**Size**: ~~M (4-8h)~~ **XL (16h+ as specified)**
**Priority**: Important
**Created**: 2025-08-25 17:51

**Tech Lead Decision** (2025-08-25 18:50):
- **REJECTED**: This slice is 3-4x larger than our thin slice limit (3 days max)
- **Actual scope**: Resource system + pattern changes + services + UI + 50+ tests = 16+ hours
- **Action**: Split into 4 thinner slices (VS_003B-1 through VS_003B-4)
- **Key insight**: Should reuse existing MatchPattern, not create new pattern type
- **Approach**: Merge is just Match with different executor when unlocked

---

---

### VS_003B-1: Merge Pattern Recognition with 3+ Blocks
**Status**: In Progress  
**Owner**: Dev Engineer
**Size**: S (4h)
**Priority**: Important
**Created**: 2025-08-25 18:50
**Updated**: 2025-08-25 20:30

**What**: Recognize match patterns that should merge when merge-to-next-tier is unlocked
**Why**: Foundation for merge system - merge replaces match behavior when unlocked

**Current State**:
- ❌ **WRONG**: Created TierUpPatternRecognizer for exactly 3 blocks (over-engineered)
- ❌ **WRONG**: Using "TierUp" terminology instead of "Merge" from Glossary
- ✅ Tests written for correct behavior (merge with 3+ blocks)
- ⚠️ **MISSING**: Result position tracking for where merged block appears

**Correct Approach** (per post-mortem):
- Use existing MatchPattern (already finds 3+ blocks)
- Add IMergeUnlockService to check if merge-to-tier-N is unlocked
- PatternExecutionResolver chooses MergeExecutor vs MatchExecutor
- **CRITICAL**: Track result position (last-acted position) for merge placement

**Done When**:
- [ ] Match patterns of 3+ blocks can be recognized for merge
- [ ] Resolver correctly picks executor based on unlock status
- [ ] Result position tracked for merge placement
- [ ] All "TierUp" references renamed to "Merge"
- [ ] Unnecessary TierUpPatternRecognizer deleted

---

### VS_003B-2: Merge Execution with Result Position
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer  
**Size**: S (4h)
**Priority**: Important
**Created**: 2025-08-25 18:50

**What**: Execute merge patterns when unlocked, converting 3+ blocks to 1 higher-tier block at result position
**Why**: Core merge mechanic, builds on detection from VS_003B-1

**How** (Technical Approach):
- Create MergePatternExecutor implementing IPatternExecutor
- Add tier field to Block domain entity (default Tier = 1)
- Track and use result position (last-acted position) for merged block placement
- Create MergeCommand/Handler (like MoveBlockCommand pattern)
- Add simple unlock check to PlayerState (hardcoded for now)
- Pattern resolver picks Merge over Match when unlocked
- Higher tier blocks get multiplied values (T2 = base * 3, T3 = base * 9)

**Done When**:
- [ ] 3 Work-T1 blocks convert to 1 Work-T2 when unlocked
- [ ] Same blocks still match when NOT unlocked
- [ ] Block tier persists in domain model
- [ ] Rewards scale with tier (T2 = 3x base value)
- [ ] 20+ tests for execution logic

**Depends On**: VS_003B-1

---

### VS_003B-3: Unlock Purchase System
**Status**: Proposed  
**Owner**: Tech Lead → Dev Engineer
**Size**: S (3h)
**Priority**: Important
**Created**: 2025-08-25 18:50

**What**: Allow players to purchase merge unlocks using attributes (Money, etc.)
**Why**: Progression system, gives purpose to resource accumulation

**How** (Technical Approach):
- Create PurchaseMergeUnlockCommand/Handler
- Store unlock costs in constants (for now, resources later)
- Update PlayerState to track unlocks persistently
- Add validation for sufficient resources
- Deduct cost on successful purchase
- Simple API endpoint for UI to query unlock status/costs

**Done When**:
- [ ] Can purchase "Work-merge-to-T2" for 100 Money
- [ ] Purchase fails if insufficient resources
- [ ] Unlock state persists across sessions
- [ ] Can query which unlocks are available/purchased
- [ ] 10+ tests for purchase logic

**Depends On**: VS_003B-2

---

### VS_003B-4: Visual Feedback & Debug Tools  
**Status**: Proposed
**Owner**: Tech Lead → Dev Engineer
**Size**: S (3h)
**Priority**: Important
**Created**: 2025-08-25 18:50

**What**: Add visual tier indicators and debug overlay for merge system
**Why**: Players need to see tiers, developers need to debug unlock states

**How** (Technical Approach):
- Update BlockView to show tier number (T1, T2, T3)
- Different visual scale/glow for higher tiers
- Merge animation (blocks combining) vs match animation (blocks disappearing)
- F9 debug overlay showing all unlock states
- Use existing BlockType.GetColorRGB() for base colors

**Done When**:
- [ ] Blocks visually show their tier (T1, T2, etc.)
- [ ] Higher tiers look visually distinct (size/glow)
- [ ] Merge animation differs from match animation
- [ ] F9 shows unlock states for debugging
- [ ] Visual feedback tested on all 9 block types

**Depends On**: VS_003B-2 (needs tiers to display)


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*











## ✅ Completed This Sprint
*Items completed in current development cycle - will be archived monthly*


### TD_080: CRITICAL - Fix Data Loss Bug in embody.ps1 Squash Merge Handler
**Status**: ~~In Progress~~ **Completed**
**Owner**: DevOps Engineer
**Size**: S (1-2h)
**Priority**: 🔥 Critical
**Created**: 2025-08-25 19:31
**Completed**: 2025-08-25 18:50

**What**: Fixed critical bug where embody.ps1 deleted unpushed local commits after squash merges
**Why**: Prevented DATA LOSS - was losing commits after PR merges

**Resolution**:
- ✅ Fixed both squash-reset code paths to check for unpushed commits first
- ✅ Preserves local work via temp branch if needed
- ✅ Tested and deployed fix
- ✅ Post-mortem created
- ✅ Memory Bank updated

**Impact**: All personas now safe from data loss when using embody.ps1


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