# BlockLife Development Backlog

**Last Updated**: 2025-08-21

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 043 (Last: TD_042 - 2025-08-21)  
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

### TD_042: Consolidate Duplicate Archive Files [Score: 15/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Critical
**Markers**: [DATA-INTEGRITY] [INFRASTRUCTURE] [RISK]
**Created**: 2025-08-21

**What**: Merge two duplicate archive files into single authoritative source
**Why**: Two archives (Archive.md and Completed_Backlog.md) create confusion, risk data loss, and increase maintenance burden
**How**:
- Merge all content from Docs/01-Active/Archive.md into Docs/07-Archive/Completed_Backlog.md
- Preserve append-only safeguards and recovery protocols from Archive.md
- Delete Archive.md after successful merge
- Update all persona documentation to reference correct path
- Update backlog-assistant with correct archive path
- Add redirect note in 01-Active pointing to 07-Archive
- Verify no references to old Archive.md remain
**Done When**:
- Single authoritative archive at Docs/07-Archive/Completed_Backlog.md
- All 489 lines from Archive.md preserved in Completed_Backlog.md
- No duplicate archive files exist
- All personas use correct archive path
- Git history shows clean consolidation
- Safeguards section preserved
**Depends On**: None

**Problem Context**: 
- Archive.md (489 lines, complete) in 01-Active folder
- Completed_Backlog.md (265 lines, partial) in 07-Archive folder
- Different items in each, creating confusion about which is authoritative
- Risk of updates going to wrong file and data loss




## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### TD_041: Verify and Document Persona Embodiment Flow [Score: 25/100]
**Status**: Proposed
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

### TD_038: Create Architectural Consistency Validation System [Score: 35/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [ARCHITECTURE] [QUALITY] [TOOLING]
**Created**: 2025-08-20

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


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_033: Create PowerShell Custom Tool Prototype for Architecture Validation [Score: 60/100]
**Status**: Proposed
**Owner**: Tech Lead
**Size**: L (1-3 days)
**Priority**: Ideas
**Markers**: [ARCHITECTURE] [TOOLING] [PROTOTYPE]
**Created**: 2025-08-20

**What**: Create custom PowerShell tool for Claude Code to validate architecture and patterns
**Why**: Current validation requires multiple grep/read operations; structured validation would catch issues early
**Note**: Revisit after TD_038 and TD_040 implementation to learn from patterns
**How**: 
- Implement after TD_029/030 to compare approaches
- Create validate-architecture.ps1 with structured output
- Check Clean Architecture boundaries
- Validate Glossary term usage in code
- Compare against Move Block reference pattern
- Return JSON with violations and suggestions
**Done When**: 
- Tool validates architecture rules programmatically
- Returns structured data Claude can interpret
- Demonstrates clear value over manual checking
- Decision made on wider custom tool adoption
**Depends On**: TD_029, TD_030 (learn from implementation)

**Problem Context**: We manually check architecture compliance through multiple file reads and greps. A custom tool could provide instant, structured feedback.
**Prototype Goal**: Evaluate if custom PowerShell tools reduce cognitive load and improve quality
**Success Metrics**: Time saved, bugs prevented, developer experience improvement


### TD_032: Fix Persona Documentation Routing and Ownership Gaps [Score: 40/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Ideas
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