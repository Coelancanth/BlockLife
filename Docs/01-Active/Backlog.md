# BlockLife Development Backlog

**Last Updated**: 2025-08-26 19:21
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 015 (Last: BR_014 - 2025-08-26 21:40)
- **Next TD**: 082 (Last: TD_081 - 2025-08-26 20:20)  
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


---

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


---


---


---



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_081: Add Comprehensive Merge System Test Coverage
**Status**: Proposed
**Owner**: Test Specialist
**Size**: M (4-6h)
**Priority**: Important
**Created**: 2025-08-26 20:20
**Complexity Score**: 3/10 (straightforward testing work)

**What**: Add missing test coverage for merge pattern execution system
**Why**: Core game mechanic has only 5 basic tests, missing critical path validation

**Current State**:
- MergePatternExecutorBasicTests: 5 tests (config and error cases only)
- Missing: Actual merge execution tests
- Missing: Tier scaling validation
- Missing: Integration tests

**Proposed Solution**:
- Add 20+ tests to MergePatternExecutorTests
- Test 3 T1 → 1 T2 transformation
- Test tier scaling (3x, 9x, 27x multipliers)
- Test mixed tier validation (should fail)
- Integration tests for full merge flow
- Property-based tests for invariants

**Simpler Alternative**: Just add 5-10 happy path tests (Score: 1/10)
- Would cover basic functionality but miss edge cases

**Pattern Match**: Follow existing test patterns in MatchPatternExecutorTests











## ✅ Completed This Sprint
*Items completed in current development cycle - will be archived monthly*




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