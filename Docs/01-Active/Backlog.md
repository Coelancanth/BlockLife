# BlockLife Development Backlog

**Last Updated**: 2025-08-21

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 039 (Last: TD_038 - 2025-08-20)  
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


### TD_037: Update All Personas for Multi-Clone Architecture [Score: 70/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: M (4-8h)
**Priority**: Critical
**Markers**: [INFRASTRUCTURE] [PERSONA-SYSTEM]
**Created**: 2025-08-20

**What**: Update all persona documentation and scripts to work with multi-clone structure
**Why**: Personas still reference worktree paths and Sacred Sequence commands that no longer exist
**How**:
- Update each persona's .md file to remove worktree references
- Update paths to use blocklife-[persona-name] directories
- Remove all Sacred Sequence command references
- Update any scripts that assume worktree structure
- Add persona-specific git identity information
- Update CLAUDE.md files in each clone after creation

**Done When**:
- All 6 persona docs updated with correct clone paths
- No references to worktrees or Sacred Sequence remain
- Each persona knows its git identity (e.g., dev-eng@blocklife)
- Personas can navigate to their correct directories
- All scripts work with new structure

**Depends On**: TD_035 (Complete)

**Problem Context**: We've migrated to multi-clone architecture but persona documentation and scripts still reference the old worktree structure. This will cause immediate failures when personas try to use old paths or commands.

**Implementation Notes**: This is essentially the "cleanup" phase of the migration. Each persona needs to know about its new home directory and identity.


## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*


### TD_030: Simplify Persona Backlog Update Suggestions [Score: 50/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [UX] [PRODUCTIVITY]
**Created**: 2025-08-20

**What**: Personas should provide concise task summaries instead of detailed backlog-assistant commands
**Why**: Current pattern shows full command syntax which is verbose and intimidating for users, reduces readability
**How**: 
- Update all persona documentation to suggest summaries in bullet points
- User can request command generation if needed
- Focus on WHAT changed not HOW to update
**Done When**: 
- All personas provide clean summaries like "Mark VS_004 complete, Create TD_031 for refactoring"
- Commands only generated on explicit request
- Documentation updated
**Depends On**: None

**Problem**: Personas currently suggest long, detailed backlog-assistant commands that are hard to read and review
**Better Approach**: Simple bullet summaries that user can easily understand and approve

**Tech Lead Decision** (2025-08-20):
- **Complexity Score**: 1/10
- **Decision**: Approved as pure documentation improvement
- **Rationale**: Reduces cognitive load, improves readability, no technical changes required, aligns with simplicity principle
- **Implementation Note**: Update all persona docs to use bullet summaries instead of showing full backlog-assistant command syntax

### TD_031: Add Verification Step for Subagent Work Completion [Score: 45/100]
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [PROCESS] [QUALITY]
**Created**: 2025-08-20

**What**: Create verification mechanism to confirm subagent tasks are actually completed
**Why**: Currently operating on trust without verification - subagents report completion but we don't verify
**How**: 
- Add post-subagent verification step in workflows
- Create simple verification scripts/patterns
- Document verification protocol for each subagent type
- Consider adding automated checks where possible
**Done When**: 
- Verification patterns documented for all subagents
- Scripts/tools created for common verifications
- Process integrated into persona workflows
- False completion reports detectible
**Depends On**: None

**Tech Lead Decision** (2025-08-20):
- Complexity Score: 3/10
- Decision: Auto-approved as process improvement
- Rationale: Addresses real trust-but-don't-verify gap in our automation
- Owner: DevOps (workflow tooling and process automation)

### TD_038: Create Architectural Consistency Validation System [Score: 65/100]
**Status**: Proposed
**Owner**: Tech Lead
**Size**: L (1-3 days)
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

**Tech Lead Consideration**: Should this be a PowerShell tool, Python script, or Claude Code subagent? Each has trade-offs for maintenance and usage.


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_033: Create PowerShell Custom Tool Prototype for Architecture Validation [Score: 25/100]
**Status**: Proposed
**Owner**: Tech Lead
**Size**: L (1-3 days)
**Priority**: Ideas
**Markers**: [ARCHITECTURE] [TOOLING] [PROTOTYPE]
**Created**: 2025-08-20

**What**: Create custom PowerShell tool for Claude Code to validate architecture and patterns
**Why**: Current validation requires multiple grep/read operations; structured validation would catch issues early
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
- Create master routing table in QuickReference.md
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