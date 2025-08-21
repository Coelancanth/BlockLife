# BlockLife Development Backlog

**Last Updated**: 2025-08-22

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 063 (Last: TD_062 - 2025-08-22)  
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


### TD_061: Automated Link Integrity Checking [Score: 20/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Ideas
**Markers**: [TOOLING] [DOCUMENTATION] [QUALITY]
**Created**: 2025-08-22

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

**Example Output**:
```
Checking 127 markdown files...
Found 4 broken links:

1. CLAUDE.md:129
   Current: [GitWorkflow.md](Docs/03-Reference/GitWorkflow.md)
   Found in: Docs/99-Deprecated/03-Reference/GitWorkflow.md
   ⚠️  File has been DEPRECATED
   Suggestion: REMOVE this reference (deprecated content)
   Alternative: Link to [GitWorkflow.md](Docs/03-Reference/GitWorkflow.md) instead
   Action? (remove/update/skip)

2. HANDBOOK.md:45  
   Current: [Architecture.md](../03-Reference/Architecture.md)
   Found in: Docs/99-Deprecated/03-Reference/Architecture.md
   ⚠️  File has been DEPRECATED
   Note: HANDBOOK.md now contains this content directly
   Suggestion: REMOVE this link (content integrated into HANDBOOK)
   Action? (remove/skip)

3. tech-lead.md:314
   Current: [Patterns.md](../03-Reference/Patterns.md)
   Found in: Docs/02-Design/Patterns.md (moved, not deprecated)
   ✅ File still active, just relocated
   Suggestion: UPDATE path to Docs/02-Design/Patterns.md
   Action? (update/skip)

4. README.md:78
   Current: [OldScript.ps1](scripts/OldScript.ps1)
   Status: FILE DELETED (not found anywhere)
   Suggestion: REMOVE this reference (script no longer exists)
   Action? (remove/skip)
```

**Tech Lead Note** (2025-08-22):
- Created after rejecting Foam as over-engineered solution
- Directly addresses the broken links problem without adding complexity
- Compatible with AI persona workflow (CLI-based)
- Maintenance discipline tool, not new linking system
- Enhanced with deprecation intelligence - knows when to remove vs update
- Context-aware suggestions based on file destination (99-Deprecated/ = remove)

**Implementation Strategy**:
- Phase 1: Basic detection and path updates
- Phase 2: Deprecation pattern recognition
- Phase 3: Alternative suggestion engine (find non-deprecated versions)





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