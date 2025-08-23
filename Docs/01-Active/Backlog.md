# BlockLife Development Backlog

**Last Updated**: 2025-08-23 21:26
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 014 (Last: BR_013 - 2025-08-22)
- **Next TD**: 077 (Last: TD_076 - 2025-08-23)  
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

*None*

## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*

### TD_069: Critical Namespace Analyzer (Simplified)
**Status**: Approved ✅
**Owner**: Dev Engineer
**Size**: S (2h)
**Priority**: Important
**Created**: 2025-08-22

**What**: Roslyn analyzer for assembly boundary safety ONLY
**Why**: Prevent MediatR discovery failures (like TD_068) without pedantic rules
**How**:
- Check: src/ files use BlockLife.Core.* namespace (not BlockLife.*)
- Check: test/ files use BlockLife.Core.Tests.* namespace
- NOT checking: Folder structure matching or Commands/Queries subfolder requirements
**Done When**:
- TD_068 scenario impossible to repeat
- No false positives on legacy code patterns
- Zero pedantic folder-matching rules

**Test Specialist Note**: Simplified after realizing strict folder-namespace matching was causing more problems than it solved. Focus only on what actually breaks (assembly boundaries for MediatR).

**Tech Lead Decision** (2025-08-23):
- ✅ APPROVED - Complexity 3/10
- Focuses on real problem (MediatR discovery failures)
- Simple Roslyn analyzer pattern, 2-hour implementation
- No pedantic rules, just assembly boundary safety

### TD_070: DI Registration Validator (Test-Based)
**Status**: Approved with Modification ⚠️
**Owner**: Test Specialist
**Size**: S (1h)
**Priority**: Important
**Created**: 2025-08-22

**What**: Test-based validation for critical service registrations
**Why**: Missing DI registrations cause cascade test failures that mask root cause
**How**:
- Create GameStrapperValidationTests.cs that runs first
- Verify all critical interfaces have implementations registered
- Check MediatR handler discovery works
- Run as first test in Architecture category (per TD_071)
**Done When**:
- Test fails fast if critical services not registered
- Clear error messages identify missing registrations
- No more "13 tests fail from 1 missing service"

**Tech Lead Decision** (2025-08-23):
- ⚠️ APPROVED WITH MODIFICATION - Reduced complexity from 5/10 to 2/10
- Use GameStrapperValidationTests instead of source generator
- Simpler to implement and debug (just a test)
- Runs in Architecture test category for fast feedback
- Can evolve to source generator later if needed

### TD_071: Test Categories for Faster Feedback
**Status**: Proposed
**Owner**: Test Specialist  
**Size**: S (2h)
**Priority**: Important
**Created**: 2025-08-22

**What**: Categorize tests for staged execution (Architecture/Integration/Stress)
**Why**: Get faster feedback on architectural issues before running slow tests
**How**:
- Add [Trait("Category", "Architecture")] to fast validation tests
- Configure pre-commit hook to run only Architecture category
- CI pipeline runs categories in stages
**Done When**:
- Architecture tests complete in <5 seconds
- Pre-commit catches namespace/DI issues immediately
- CI fails fast on architectural violations

### TD_076: Auto-Fix Session Log on Every Embody  
**Status**: Approved ✅
**Owner**: DevOps Engineer
**Size**: S (1h)
**Priority**: Important
**Created**: 2025-08-23
**Complexity Score**: 2/10
**Pattern Match**: Scripts already exist (fix-session-log-order.ps1)
**Simpler Alternative**: Manual cleanup monthly (0h but never happens)

**Problem**: Session log becomes unreadable chaos - entries out of order, duplicates, scattered dates

**Solution**:
- Add to embody.ps1: Auto-run fix-session-log-order.ps1
- Silent operation unless errors found
- Fix before showing session history to user
- NO new scripts needed - reuse existing

**Why Not Simpler**: Manual cleanup never happens. This is a 1-line addition to embody.ps1

**Done When**:
- Session log automatically sorted on every embody
- No manual cleanup ever needed
- Historical order preserved and readable

**Tech Lead Decision** (2025-08-23):
- ✅ APPROVED - Complexity 2/10
- Classic automation win - 1 hour saves 100 hours
- Reuses existing scripts, trivial integration
- Should have been automated from day 1



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*



### TD_067: Refine Active Context Protocol - Preserve Multi-Phase Learnings
**Status**: Proposed
**Owner**: DevOps Engineer
**Size**: S (<4h)
**Priority**: Ideas
**Created**: 2025-08-22
**Complexity Score**: 3/10
**Pattern Match**: Follows documentation improvement patterns from existing workflow docs
**Simpler Alternative**: Manual reminder in persona docs (2-hour version)

**Problem**: Active context gets completely rewritten between phases, losing valuable learnings from previous phases. Phase 1 & 2 learnings from VS_003A were nearly discarded when updating for Phase 3.

**Solution**: 
- Create "Cumulative Learnings" section that preserves insights across all phases
- Implement "Phase History" tracking to maintain context of completed work
- Add protocol for merging new learnings with existing knowledge
- Update persona docs with guidance on preserving vs refreshing context

**Why Not Simpler**: Multi-phase projects (like VS_003A with 5 phases) accumulate significant technical insights that are lost with current approach. A systematic protocol ensures knowledge retention across phase boundaries.

**Files to Update**:
- `.claude/memory-bank/active/[persona].md` templates
- `Docs/04-Personas/` persona documentation
- Memory bank protocols documentation

### TD_074: Root Cause Memory Bank Protocol
**Status**: Proposed
**Owner**: Tech Lead
**Size**: S (2h)
**Priority**: Ideas
**Created**: 2025-08-23
**Complexity Score**: 3/10
**Pattern Match**: Similar to existing memory bank protocols
**Simpler Alternative**: Just add template to persona docs (1h)

**Problem**: ActiveContext captures surface fixes ("fixed DI registration") instead of root causes ("stateless services must be Singleton in MediatR lifecycle")

**Solution**:
- Add simple 3-line template to activeContext updates
- Template: Surface/Root/Pattern structure
- Update embody.ps1 to remind about root cause analysis
- NO complex protocols or additional documents

**Why Not Simpler**: The 1-hour template-only version might work, but automated reminders ensure consistency

**Done When**:
- activeContext entries show WHY bugs happened
- Pattern solutions documented to prevent recurrence
- No added complexity to workflow

### TD_075: Context-Aware Reference Display with MANDATORY Context7 Prompts
**Status**: Proposed (ELEVATED PRIORITY)
**Owner**: DevOps Engineer
**Size**: M (4h)
**Priority**: Ideas → Important (considering elevation)
**Created**: 2025-08-23
**Complexity Score**: 4/10
**Pattern Match**: Embody script already shows quick refs
**Simpler Alternative**: Do nothing, rely on personas to remember (0h)

**Problem**: Personas don't use Context7 for LanguageExt, causing API mistakes and wasted hours

**Solution**: 
- Enhance embody.ps1 to show task-specific references
- **CRITICAL**: Detect LanguageExt work → MANDATE Context7 query
- When VS work detected → Display Move Block pattern location
- When error handling detected → Show "Query Context7: Fin Error bind patterns"
- Add pre-implementation checklist that includes Context7 queries
- Smart detection based on file patterns and backlog content

**Why Not Simpler**: LanguageExt API mistakes are our #1 time waster

**Done When**:
- Embody script FORCES Context7 consideration for LanguageExt work
- Pre-implementation checklist shown for complex patterns
- Measurable reduction in LanguageExt API errors
- No new documentation created

**Tech Lead Note**: This should probably be Important priority - LanguageExt mistakes waste HOURS







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