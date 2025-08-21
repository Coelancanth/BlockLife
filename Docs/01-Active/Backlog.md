# BlockLife Development Backlog

**Last Updated**: 2025-08-22

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 065 (Last: TD_064 - 2025-08-22)  
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

### TD_041: Verify and Document Persona Embodiment Flow [Score: 20/100]
**Status**: Approved ✅
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [DOCUMENTATION] [PROCESS] [VERIFICATION]
**Created**: 2025-08-21

**What**: Verify what actually happens when embodying personas and document the complete flow
**Why**: Gap between documented behavior and actual behavior creates confusion; need to ensure personas follow intended workflow
**How**:
- Test embodying each persona and document actual behavior
- Verify which docs are automatically read (Memory Bank, Backlog, etc.)
- Check if personas follow the documented workflow steps
- Identify gaps between intended and actual behavior
- Update persona docs with accurate flow description
- Create checklist of what SHOULD happen vs what DOES happen
**Done When**:
- Complete flow documented for each persona
- Gaps between intended and actual behavior identified
- Persona docs updated with accurate automation descriptions
- Verification checklist created for future testing
- Memory Bank integration verified in practice
**Depends On**: None

**Problem Context**: We've documented elaborate workflows but haven't verified personas actually follow them. Need empirical testing to ensure documentation matches reality.

**Tech Lead Decision** (2025-08-22):
- Complexity Score: 20/100 (simpler than estimated)
- Decision: APPROVED - Critical verification work
- Rationale: Must verify documentation matches reality before building more persona features
- Impact: Will reveal gaps between aspirational and actual persona automation
- Guidance: Use empirical testing protocol - for each persona, document EXACTLY what happens vs. what's documented
- Expected finding: Most "automated" behaviors likely need explicit prompting







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