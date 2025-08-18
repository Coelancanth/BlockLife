# BlockLife Development Backlog

**Last Updated**: 2025-08-19

## 📖 How to Use This Backlog

### 🧠 Owner-Based Ultra-Think Protocol

**Each item has a single Owner persona responsible for decisions and progress.**

#### When You Embody a Persona:
1. **Filter** for items where `Owner: [Your Persona]`
2. **Ultra-Think** if `Status: Proposed` and you're the owner (5-15 min deep analysis)
3. **Quick Scan** for other statuses you own (<2 min updates)
4. **Update** the backlog before ending your session
5. **Reassign** owner when handing off to next persona

#### Ultra-Think Triggers:
- **Automatic**: When you own a "Proposed" item
- **Markers**: [ARCHITECTURE], [ROOT-CAUSE], [SAFETY-CRITICAL], [COMPLEX]
- **Output**: Document decision rationale directly in the item

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

### BR_004: AI Violated Critical Git Workflow - No Fetch/Rebase Before Push [Score: 90/100]
**Status**: Solution Implemented ✅
**Owner**: Completed
**Size**: XS (workflow reinforcement)
**Priority**: Critical
**Created**: 2025-08-19
**Category**: Workflow/Process/AI-Behavior

**Bug Description**: 
AI assistant (Dev Engineer) pushed feature branch without following documented CRITICAL workflow steps: failed to fetch, update main, and rebase before pushing.

**Tech Lead Solution** (2025-08-18):
✅ **APPROVED - Two-Layer Defense System**

**Layer 1: Git Hook Enforcement**
- Created pre-push hook that blocks outdated branches
- Prevents direct pushes to main
- Clear error messages with fix instructions
- Installation scripts for all platforms

**Layer 2: Documentation & AI Training**
- Created GitWorkflow.md as single source of truth
- Updated CLAUDE.md to reference it with mandatory behavior rules
- AI must output each git command for transparency
- Sacred Sequence must be followed without exception

**Status**: Solution Implemented ✅



### BR_001: Multi-Phase Items Incorrectly Archived Before Completion [Score: 85/100]
**Status**: Solution Implemented ✅
**Owner**: Completed
**Size**: S (2-3 hours to update documentation and process)
**Priority**: Important (prevents work item loss)
**Found By**: Test Specialist
**Created**: 2025-08-18
**Markers**: [PROCESS-BUG] [WORKFLOW]

**What**: Multi-phase items (like VS_001 with Phase 1/2/3) get archived when individual phases complete
**Why**: Nearly lost VS_001 Phase 3 when Phase 2 was marked "Done" - causes work items to disappear

**Tech Lead Solution** (2025-08-18):
✅ **ROOT CAUSE: Violating Thin Slice Principle**

The bug is a symptom of a deeper problem - we're creating multi-phase VS items that violate our core principle: every vertical slice must be independently shippable in ≤3 days.

**The Elegant Solution: Eliminate Phases Entirely**

**New Rules (Enforced):**
1. **NO PHASES in VS items** - Each VS must complete in ≤3 days
2. **Break large features** into sequential VS items with dependencies
3. **Use EPICs** for coordinating related VS items (rare cases)

**Status**: Solution Implemented ✅
**Files Created**: 
- `Docs/Workflow/ThinSliceProtocol.md`
- Updated `Templates/VerticalSlice_Template.md`




## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 65/100]
**Status**: Proposed
**Owner**: Tech Lead (needs approval)
**Size**: S (2-3 hours)
**Priority**: Ideas (quality improvement)
**Created**: 2025-08-19
**Proposed By**: Test Specialist
**Markers**: [QUALITY] [TESTING]

**What**: Implement FSCheck property tests for swap operation invariants
**Why**: Catch edge cases that example-based tests might miss, ensure mathematical properties hold

**Approach**:
1. Create `tests/Features/Block/Properties/SwapProperties.cs`
2. Implement three core property tests
3. Add custom generators for valid swap scenarios
4. Integrate with existing test suite

**Proposed Properties**:
```csharp
// 1. Swap preserves total block count
[Property]
public Property SwapOperation_PreservesBlockCount()

// 2. Swap validation is symmetric
[Property]
public Property SwapDistance_IsSymmetric()
// If A can swap with B, then B can swap with A

// 3. Double swap returns to original state
[Property]
public Property DoubleSwap_ReturnsToOriginal()
// Swapping twice = identity operation
```

**Additional Properties to Consider**:
- No blocks occupy same position after swap
- Swap respects grid boundaries
- Failed swaps leave grid unchanged
- Concurrent swaps don't corrupt state

**Done When**:
- Property tests integrated into test suite
- All properties pass with 1000+ generated cases
- Edge cases discovered are documented
- CI pipeline includes property test execution

**Test Specialist Note**: These properties would have caught the notification bug earlier through systematic testing. FSCheck's shrinking would provide minimal failing cases for easier debugging.

### TD_001: Extract Input System to Separate Feature Module [Score: 45/100]
**Status**: Approved ✓
**Owner**: Dev Engineer
**Size**: M (4-6 hours)
**Priority**: Ideas (after VS_001)
**Tech Lead Decision** (2025-08-18): Valid architectural improvement. Implement after VS_001 to understand full requirements.

**Approach**:
- Create `src/Features/Input/` module
- Extract common input handling patterns
- Centralize input state management
- Follow existing feature module structure


### TD_009: Refine Persona Command Implementation for Production [Score: 40/100]
**Status**: Approved ✓
**Owner**: DevOps Engineer
**Size**: M (4-6 hours)  
**Priority**: Ideas
**Found By**: DevOps Engineer during persona system testing
**Created**: 2025-08-18

**Tech Lead Decision** (2025-08-18):
✅ APPROVED - Valid improvements but not urgent
- Do after critical items (TD_003, TD_004)
- Focus on error handling and silent mode

**What**: Improve persona command system robustness and user experience
**Why**: Current implementation works but needs refinement for reliable production automation

**Current Issues**:
- Local vs global config precedence unclear and inconsistent
- No error handling for corrupted state files
- Manual testing required to verify which config is active
- Command output verbose for status line usage

**Approach**:
- Add config detection and validation to persona commands
- Implement graceful error handling for missing/corrupted state files
- Create diagnostic commands to show active configuration
- Add silent mode for status line integration (no console output)
- Document config precedence rules clearly

**Technical Improvements**:
- Add `--quiet` flag for status line usage
- Validate .claude/current-persona file format
- Add config source detection (global vs local)
- Implement fallback behavior for missing configs

**Done When**:
- Commands work reliably regardless of config setup
- Clear error messages for configuration issues
- Silent mode works properly with ccstatusline
- Documentation explains config precedence clearly
- No false negatives in persona detection

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