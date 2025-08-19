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

### BR_008: Investigate Flaky SimulationManagerThreadSafetyTests.ConcurrentOperations Test [Score: 70/100]
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