# BlockLife Development Backlog

**Last Updated**: 2025-08-21

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 056 (Last: TD_055 - 2025-08-21)  
- **Next VS**: 004 (Last: VS_003D - 2025-08-19)

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



## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### TD_055: Define Branch and Commit Decision Protocols [Score: 35/100]
**Status**: Proposed
**Owner**: Tech Lead
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [PROCESS] [DOCUMENTATION] [WORKFLOW]
**Created**: 2025-08-21

**What**: Create clear decision protocols for branching and commit strategies
**Why**: "New work = new branch" and "atomic commits" are oversimplified; real scenarios need nuanced decisions
**How**:
- Define "new work" vs "continuing work" with concrete examples
- Create decision tree for branch creation scenarios
- Define "atomic commit" for BlockLife context
- Address work continuity (multi-day tasks)
- Handle context switching protocols
- Document in persona protocols or HANDBOOK.md
**Done When**:
- Clear definition of when to create new branches
- Atomic commit guidelines with examples
- Decision tree for common scenarios
- Personas have actionable branch/commit guidance
- No ambiguity about work continuity
**Depends On**: None

**Problem Context**: Current guidance "new work = new branch" is undefined. Is fixing a typo "new work"? Continuing yesterday's TD? Found bug while working on VS? These decisions aren't obvious and need clear protocols. Similarly, pre-push hook can't enforce atomic commits retroactively - need pre-commit guidance.

**Key Questions to Answer**:
- When is work "new" enough for a new branch?
- How to handle multi-day work items?
- What makes a commit "atomic" in BlockLife?
- Should quick fixes get branches?
- How to handle discovered issues while working?

**Tech Lead Note**: This complexity was hidden by claiming things were "obvious" - they're actually undefined, which is worse than complex.




### TD_032: Fix Persona Documentation Routing and Ownership Gaps [Score: 40/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [DOCUMENTATION] [PRODUCTIVITY]
**Created**: 2025-08-20

**What**: Add routing tables and "what NOT to accept" sections to all persona documents
**Why**: Current docs cause work misrouting - personas don't know when to hand off to others
**How**: 
- Add "Work I Don't Accept" section to each persona doc
- Create master routing table in HANDBOOK.md
- Expand DevOps ownership to include all dev tooling/scripts
- Clarify Test Specialist vs Debugger Expert handoff criteria
- Add cross-references between related personas
**Done When**: 
- Each persona has clear rejection criteria
- Master routing table exists and is referenced
- No ambiguity about who owns what type of work
- DevOps owns all developer experience improvements
**Depends On**: None

**Tech Lead Decision** (2025-08-20):
- Complexity Score: 4/10
- Decision: Auto-approved as documentation improvement
- Rationale: Addresses systematic routing confusion discovered during TD_029/030 review
- Impact: Prevents future work misrouting and clarifies ownership
- Pattern: Similar to tech-lead.md improvements just completed

**Tech Lead Update** (2025-08-21):
- Moved from Ideas to Important - routing confusion actively affecting productivity
- Should be prioritized to prevent ongoing misrouting issues

## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*





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