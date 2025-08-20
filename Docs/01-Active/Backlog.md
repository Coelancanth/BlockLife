# BlockLife Development Backlog

**Last Updated**: 2025-08-20

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 012 (Last: BR_011 - 2025-08-20)
- **Next TD**: 035 (Last: TD_034 - 2025-08-20)  
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

### TD_029: Add Main Directory Protection for Persona Worktree System
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [PRODUCTIVITY] [SAFETY]
**Created**: 2025-08-20

**What**: Create warning system to prevent accidental work in main directory when personas should be used
**Why**: Users may accidentally open Claude in main directory, losing isolation benefits and risking conflicts
**How**: 
- Create claude.ps1 wrapper script with warning
- Add .claude-warning marker file
- Update README with clear guidance
- Optional git hook for commit warnings
**Done When**: 
- Warning appears when opening Claude in main
- Clear redirection to blocklife command
- Documentation updated
- Accidental main work reduced to near-zero
**Depends On**: TD_023 (Complete)

**Problem Context**: With worktree system complete, nothing prevents accidentally working in main directory which loses isolation benefits and can create conflicts
**Proposed Solution**: Gentle guardrails that remind users to use workspaces without being obstructive

**Tech Lead Decision** (2025-08-20):
- **Complexity Score**: 2/10
- **Decision**: Approved as simple, non-intrusive safety guardrail
- **Rationale**: PowerShell wrapper pattern matches existing tooling, allows bypass for legitimate work, minimal implementation risk
- **Technical Guidance**: Use .claude-warning marker approach that can be checked by wrapper script for clean detection

### TD_030: Simplify Persona Backlog Update Suggestions
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

### TD_031: Add Verification Step for Subagent Work Completion
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


## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_033: Create PowerShell Custom Tool Prototype for Architecture Validation
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

### TD_034: Enforce Up-to-Date Branch Requirement via GitHub Protection
**Status**: Approved
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [GIT] [PROCESS] [AUTOMATION]
**Created**: 2025-08-20

**What**: Configure GitHub branch protection to require branches be up-to-date with main before merging
**Why**: PR #49 conflict was caused by not fetching/pulling main first - automation can prevent this
**How**: 
- Enable "Require branches to be up to date before merging" in GitHub settings
- Review GitWorkflow.md for additional protection opportunities
- Consider "Require linear history" to enforce rebase workflow
- Add "Dismiss stale reviews when new commits are pushed"
- Document the protection rules in GitWorkflow.md
**Done When**: 
- Branch protection enforces up-to-date requirement
- PR conflicts from outdated branches become impossible
- GitWorkflow.md updated with protection details
- Team can't accidentally create conflicts from stale branches
**Depends On**: None

**Problem Context**: PR #49 had conflicts because branch was created without fetching latest main. While GitWorkflow.md documents the Sacred Sequence, human error still occurs.
**Solution**: GitHub's built-in protection can enforce this automatically - no PR can be merged unless it's current with main.
**Additional Benefits**: Forces good git hygiene, prevents "worked on my machine" issues, ensures CI runs against latest code

### TD_032: Fix Persona Documentation Routing and Ownership Gaps
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