# BlockLife Development Backlog

**Last Updated**: 2025-08-23 21:26
**Last Aging Check**: 2025-08-22
> 📚 See BACKLOG_AGING_PROTOCOL.md for 3-10 day aging rules

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 014 (Last: BR_013 - 2025-08-22)
- **Next TD**: 080 (Last: TD_079 - 2025-08-24 02:21)  
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
**Status**: Done ✅
**Owner**: DevOps Engineer
**Size**: S (2h)
**Priority**: Important
**Created**: 2025-08-22
**Completed**: 2025-08-24

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

### TD_078: Timestamp Accuracy Protocol for Memory Bank & Session Logs
**Status**: Approved ✅
**Owner**: DevOps Engineer
**Size**: S (1h)
**Priority**: Important
**Complexity**: 1/10
**Created**: 2025-08-24 01:59
**Approved**: 2025-08-24 02:21

**What**: Enforce getting current date/time before updating timestamped documents
**Why**: Inaccurate timestamps in Memory Bank and session logs cause confusion
**How**:
- Update embody.ps1 to capture timestamp at start of execution
- Add validation to ensure timestamps are current (not stale)
- Update CLAUDE.md to remind about running `date` command first
- Auto-timestamp injection for session log entries (specific implementation)
**Done When**:
- Memory Bank updates always have accurate timestamps
- Session log entries reflect actual work time
- No more "future dated" or incorrect timestamps
- Process is automatic/zero-friction

**Tech Lead Decision** (2025-08-24 02:21):
- ✅ APPROVED - Complexity 1/10, trivial fix for real problem
- Solves actual confusion about when work was done
- Simple PowerShell timestamp capture, no new dependencies
- "Consider auto-timestamp" changed to specific requirement
- Classic "obvious fix" that should just be done

**DevOps Engineer Notes**:
- Common issue: Updating Memory Bank with old timestamps
- Solution: Capture `Get-Date` at script start, use throughout
- Could add timestamp validation (reject if >5 min old)
- Make it impossible to get wrong, not just documented

### TD_077: Incremental Test Runner for 95% Faster Feedback
**Status**: Rejected ❌
**Owner**: DevOps Engineer
**Size**: M (4-6h)
**Priority**: Important
**Complexity**: 7/10 (too high)
**Created**: 2025-08-24
**Rejected**: 2025-08-24 02:21

**What**: Git-based incremental test runner for local AND CI/CD environments
**Why**: Current tests take 39s locally, CI runs all tests on every PR (wasteful)

**Tech Lead Decision** (2025-08-24 02:21):
- ❌ REJECTED - Complexity 7/10, over-engineered solution
- We already solved local feedback with TD_071 (1.3s quick tests)
- File mapping is fragile (breaks with refactoring)
- Cache invalidation adds debugging complexity
- Risk of missing affected tests outweighs 30s CI savings
- **SEE TD_079 for simpler alternative (2/10 complexity)**

**Why Rejected in Detail**:
1. Local feedback already fast: quick.ps1 runs in 1.3s
2. File → test mapping breaks easily with renames
3. Cache bugs are notoriously hard to debug
4. False negatives (missing tests) are worse than 30s wait
5. Maintenance burden not worth the complexity

**Original Proposal** (preserved for reference):
- Detect changed files via git diff (local) or GitHub API (CI)
- Map source files to test files using conventions
- Run only affected test classes/categories
- Cache test results by file hash (with GitHub Actions cache)
- Progressive execution: unit → integration → architecture
- CI: Use GitHub's changed-files action for PR-specific testing

### TD_079: Simple CI Staged Test Execution
**Status**: Approved ✅
**Owner**: DevOps Engineer  
**Size**: S (30min)
**Priority**: Important
**Complexity**: 2/10
**Created**: 2025-08-24 02:21
**Approved**: 2025-08-24 02:21 (Tech Lead auto-approved)

**What**: Add fail-fast architecture tests to CI before full suite
**Why**: Get 1.3s feedback in CI for architecture violations before running 39s full suite
**How**:
- Add "Quick Tests" step that runs `./scripts/test/quick.ps1`
- Only run full suite if quick tests pass
- No complex mapping, caching, or file detection needed
- Reuse existing test categorization from TD_071
**Done When**:
- CI fails in 1.3s for architecture violations
- Full test suite only runs after quick tests pass
- Zero maintenance burden (uses existing scripts)
- PR workflow shows clear "Quick Tests" and "Full Tests" steps

**Tech Lead Decision** (2025-08-24 02:21):
- ✅ AUTO-APPROVED - Complexity 2/10, obvious improvement
- Simpler alternative to rejected TD_077
- Reuses TD_071 work (test categories)
- No fragile file mapping or caching
- Gives fast CI feedback without complexity

**Implementation** (for .github/workflows/ci.yml):
```yaml
- name: Quick Architecture Tests (fail fast)
  run: ./scripts/test/quick.ps1  # 1.3s
  
- name: Full Test Suite
  if: success()
  run: ./scripts/test/full.ps1    # 39s
```

## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*











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