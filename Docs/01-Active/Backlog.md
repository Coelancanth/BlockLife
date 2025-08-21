# BlockLife Development Backlog

**Last Updated**: 2025-08-21

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 013 (Last: BR_012 - 2025-08-21)
- **Next TD**: 050 (Last: TD_049 - 2025-08-21)  
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

### TD_047: Improve Persona Backlog Decision Protocol [Score: 20/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [PROCESS] [DOCUMENTATION] [PERSONA-PROTOCOL]
**Created**: 2025-08-21

**What**: Define clear criteria for when personas should add items to the backlog vs. suggesting user do it
**Why**: Current persona protocol causes confusion about when to auto-add vs. suggest backlog updates; need clear decision framework
**How**:
- Analyze current persona documentation for backlog protocols
- Define clear criteria: "Auto-add when X, suggest when Y"
- Create decision tree or flowchart for personas
- Update all 6 persona documents with consistent protocol
- Add examples of each scenario (auto-add vs. suggest)
- Test protocol with each persona type to verify clarity
**Done When**:
- Clear decision criteria documented in each persona file
- Consistent protocol across all 6 personas
- Decision examples provided for common scenarios
- Protocol tested and validated with representative cases
**Depends On**: None

**Problem Context**: DevOps Engineer and other personas inconsistently handle backlog updates - sometimes auto-adding, sometimes suggesting. Need clear protocol to eliminate confusion and ensure consistent behavior.

### TD_048: Migrate FsCheck Property-Based Tests to 3.x API [Score: 30/100]
**Status**: Proposed
**Owner**: Test Specialist
**Size**: M (4-8h)
**Priority**: Important
**Markers**: [TESTING] [MIGRATION] [FSCHECK] [PROPERTY-BASED]
**Created**: 2025-08-21

**What**: Update FsCheck property-based tests from 2.16.6 to 3.3.0 API
**Why**: FsCheck 3.x has breaking API changes; tests currently disabled and moved to `FsCheck_Migration_TD047/`
**How**:
- Research FsCheck 3.x API changes (use Context7 if available)
- Update `Prop.ForAll` usage to new API patterns
- Fix `Gen<T>`, `Arb`, and `Arbitrary<T>` usage
- Resolve Property attribute conflicts with xUnit
- Update custom generators in `BlockLifeGenerators.cs`
- Update property tests in `SimplePropertyTests.cs`
- Re-enable FsCheck.Xunit package reference
- Move tests back from `FsCheck_Migration_TD047/` to proper location
- Ensure all 7 property-based tests pass
**Done When**:
- All FsCheck tests compile with 3.x API
- All property-based tests passing (7 tests)
- FsCheck.Xunit package re-enabled in project
- No references to old 2.x API patterns
- Property tests moved back to proper test directory
**Depends On**: None

**Problem Context**: Package updates completed but FsCheck 3.x has extensive breaking changes requiring dedicated migration effort. Tests temporarily disabled to unblock other infrastructure updates.

### TD_049: Add Git Branch Context Tracking to Memory Bank [Score: 40/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [INFRASTRUCTURE] [MEMORY-BANK] [GIT] [PERSONA-CONTEXT]
**Created**: 2025-08-21

**What**: Enhance activeContext.md to track git branch state and uncommitted changes for each persona
**Why**: Multi-clone architecture requires branch context preservation when switching personas; current Memory Bank loses critical git state
**How**:
- Update activeContext.md template to include git status section
- Add current branch, working directory status, recent commits for active persona
- Include summary of other persona branch states (last known positions)
- Create automation script to capture git state on persona switch
- Update Memory Bank documentation with git context requirements
- Test context preservation across persona switches
- Ensure seamless work resumption with full git context
**Done When**:
- activeContext.md includes comprehensive git status tracking
- Persona switches preserve and restore git context accurately
- All 6 personas can resume work on correct branches
- Documentation updated with git context management
- Automation tested and validated across persona switches
**Depends On**: None

**Problem Context**: CRITICAL infrastructure gap - personas working on different branches lose context when switching. DevOps Engineer on `feat/td-042-consolidate-archives`, but Memory Bank doesn't track this. Multi-clone isolation is meaningless without branch context preservation.

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


### TD_046: Complete Git Workflow Documentation [Score: 20/100]
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Important
**Markers**: [DOCUMENTATION] [WORKFLOW] [DEVELOPER-EXPERIENCE]
**Created**: 2025-08-21

**What**: Add missing git workflow protocols to HANDBOOK.md
**Why**: Current workflow docs have gaps causing confusion about branch management and commit practices
**How**:
- **Remote branch cleanup**: When and how to clean (`git remote prune`, `git branch -d`)
- **Branch switching triggers**: When to create new branch vs continue existing
- **Small step commit protocol**: Guidelines for atomic commits and WIP handling
- **Post-merge cleanup**: Standard steps after PR merge
- Add to HANDBOOK.md Git Workflow section
- Include examples and decision trees
**Done When**:
- Clear protocol for remote branch cleanup
- Decision tree for when to create new branches
- Guidelines for commit granularity (atomic vs WIP)
- Post-merge cleanup checklist
- Examples covering common scenarios
- No confusion about basic git workflow decisions
**Depends On**: None

**Gap Analysis**:
- ❌ No mention of `git remote prune origin`
- ❌ No guidance on when to start fresh branch
- ❌ No atomic commit guidelines
- ❌ No post-merge cleanup protocol

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