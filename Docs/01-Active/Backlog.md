# BlockLife Development Backlog

**Last Updated**: 2025-08-25 16:26
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 014 (Last: BR_013 - 2025-08-22)
- **Next TD**: 081 (Last: TD_080 - 2025-08-25 19:31)  
- **Next VS**: 004 (Last: VS_003B - 2025-08-25 17:51)

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

### TD_080: CRITICAL - Fix Data Loss Bug in embody.ps1 Squash Merge Handler
**Status**: In Progress
**Owner**: DevOps Engineer
**Size**: S (1-2h)
**Priority**: 🔥 Critical
**Created**: 2025-08-25 19:31

**What**: Fix critical bug where embody.ps1 deletes unpushed local commits after squash merges
**Why**: Current implementation causes DATA LOSS - lost 4 commits today including VS_003B work

**Root Cause**:
- embody.ps1 detects squash merges and runs `git reset --hard origin/main`
- This DELETES all local commits without checking if they were pushed
- User continues working after PR merge, makes new commits
- Next embody run detects the squash, resets, and loses all new work

**How** (Technical Fix - ALREADY IMPLEMENTED):
- ✅ Check for unpushed commits BEFORE resetting: `git log origin/$branch..HEAD`
- ✅ If local commits exist, preserve them via temp branch and cherry-pick
- ✅ Only reset if no local commits exist
- ✅ Applied fix to both squash-reset paths (lines 70-107 and 127-163)

**Done When**:
- [x] Bug fixed in embody.ps1 (both occurrences)
- [x] Test with simulated squash merge scenario
- [x] Commit and push the fix
- [x] Create post-mortem at Docs/06-PostMortems/Inbox/
- [x] Update Memory Bank with incident details
- [ ] Consider additional safeguards (backup branch, confirmation prompt)

**Impact**: Prevents catastrophic data loss for all personas using embody.ps1

## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### VS_003B: Merge System - Progressive Tier Progression
**Status**: Proposed
**Owner**: Product Owner → Tech Lead
**Size**: M (4-8h)
**Priority**: Important
**Created**: 2025-08-25 17:51

**What**: Implement merge system where 3+ adjacent same-type blocks combine into higher tier when unlocked
**Why**: Core progression mechanic that replaces matching for specific tiers, creating strategic depth

**How** (Technical Approach):
- **Create BlockLibrary Resource** as single source of truth for block configurations
  - BlockTypeResource: type_name, base_value, color, icon, abbreviation (W/S/H)
  - Merge unlock costs per type and tier (Work-T2: 100 Money, etc.)
  - All block data configurable in Inspector via block_library.tres
- Extend pattern recognition to detect merge opportunities
- Create MergeUnlockService that reads costs from BlockLibrary
- Implement MergeCommand that replaces MatchCommand when merge is unlocked
- Update PlayerState to track merge unlocks (e.g., Work-merge-to-T2: true)
- Modify pattern handler to check merge unlock status before match/merge decision
- **Update BlockView to display tier/type labels** using BlockLibrary abbreviations
- Add debug HUD showing merge unlock states (F9 toggle)

**Done When**:
- [ ] **BlockLibrary Resource created** with all 9 block types configured
- [ ] Block types load from BlockLibrary (not hardcoded)
- [ ] 3 adjacent Work-T1 blocks merge to 1 Work-T2 when merge-to-T2 unlocked
- [ ] Same Work-T1 blocks match and clear when merge-to-T2 NOT unlocked
- [ ] Merge unlocks purchasable with attributes (costs from BlockLibrary)
- [ ] Each block type has independent merge unlock progression
- [ ] Visual feedback shows merge animation vs match animation
- [ ] **Block visuals display tier and type** from BlockLibrary abbreviations
- [ ] Debug overlay shows current merge unlock status for quick testing
- [ ] Designer can modify block values in Inspector without code changes
- [ ] 50+ tests validating merge vs match decision logic

**Depends On**: VS_003A (Complete ✅)

**BlockLibrary Structure**:
```
res://game/block_library.tres (main library)
  ├─ block_types[] array containing:
  │   ├─ work_type.tres (BlockTypeResource)
  │   ├─ study_type.tres (BlockTypeResource)
  │   └─ health_type.tres (etc...)
  └─ merge_unlock_costs{} dictionary:
      ├─ "Work": {"T2": 100, "T3": 300, "T4": 900}
      └─ "Study": {"T2": 200, "T3": 600, "T4": 1800}
```


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*











## ✅ Completed This Sprint
*Items completed in current development cycle - will be archived monthly*

*All items moved to Completed_Backlog.md for permanent archival*


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