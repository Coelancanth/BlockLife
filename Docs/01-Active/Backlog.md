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

### BR_001: Dev Engineer Must Run Build Before Committing
**Status**: New
**Owner**: Dev Engineer
**Size**: XS (5 minutes)
**Priority**: Important
**Created**: 2025-08-19
**Found By**: Dev Engineer during TD_001 implementation

**What**: Dev Engineer forgot to run full build before attempting to commit, only ran tests
**Why**: Tests passing doesn't guarantee Godot compilation succeeds - could break the game

**Root Cause**:
- Workflow habit focused on unit tests only
- Build script has both `test` and `build` commands, easy to forget one
- No reminder in workflow to run full build

**Impact**:
- Could commit code that breaks Godot compilation
- Other developers would pull broken code
- CI would catch it but wastes time

**Reproduction**:
1. Make changes to Godot-specific code (godot_project/)
2. Run only `./scripts/build.ps1 test`
3. Tests pass but Godot compilation might fail
4. Attempt to commit without running build

**Fix**:
- Update Dev Engineer workflow to ALWAYS run full build before commit
- Consider adding pre-commit hook to enforce build
- Add to CLAUDE.md as mandatory step

**Done When**:
- Workflow documentation updated
- Dev Engineer checklist includes mandatory build step
- Consider: pre-commit hook that runs build



## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

### VS_002: Translate Creative Brainstorms into Feature Specifications
**Status**: Proposed
**Owner**: Product Owner
**Size**: L (2-3 days)
**Priority**: Ideas
**Created**: 2025-08-19
**Depends On**: None

**What**: Translate Chinese brainstorming content into actionable VS items and feature specifications
**Why**: Valuable creative vision needs to be transformed into implementable features

**Scope**:
- Review all brainstormA/B/C.md files (Chinese content)
- Extract key gameplay concepts and emotional themes
- Merge them into gamevision
- Preserve creative intent while ensuring technical feasibility
- Prioritize based on player value and implementation complexity

**Approach**:
1. **Extract Core Concepts**: Identify the most impactful ideas from brainstorms
2. **Validate Against Vision**: Ensure alignment with Game_Design_Vision.md
5. **Prioritize**: Rank by player impact and technical complexity

**Examples to Extract**:
- Emotional time flow (happy = fast, sad = slow)
- Relationship blocks with visual bonds
- Personality development through actions
- Life stage-specific block types
- Legacy and memory systems

**Done When**:
- All brainstorm files reviewed and key concepts extracted
- Priority and dependencies established
- Creative intent preserved in specifications

**Product Owner Note**: This is meta-work that enables future development. The Chinese brainstorms contain rich creative vision that needs systematic extraction and formalization.

### TD_015: Fix All Internal Documentation Links After Reorganization
**Status**: Done ✅
**Owner**: Tech Lead
**Size**: S (2-3 hours)
**Priority**: Important (documentation broken until fixed)
**Completed**: 2025-08-18
**Created**: 2025-08-19
**Proposed By**: Tech Lead
**Markers**: [DOCUMENTATION] [TECHNICAL-DEBT]

**What**: Scan all markdown files and fix broken internal links after docs reorganization
**Why**: Major restructure moved all files - many internal links are now broken, making docs hard to navigate

**Impact of Not Fixing**:
- Broken documentation navigation
- AI personas referencing wrong paths
- User frustration finding information
- Loss of documentation trustworthiness

**Scope**:
1. **Scan all .md files** for internal links
2. **Identify broken links** pointing to old structure
3. **Update to new paths** following numbered folder structure
4. **Verify external links** still valid
5. **Test navigation paths** in key documents

**Technical Approach**:
```bash
# Find all internal links in markdown files
grep -r "\[.*\](.*/.*\.md)" Docs/ --include="*.md"

# Common replacements needed:
Docs/Workflow/ → Docs/01-Active/
Docs/Reference/ → Docs/03-Reference/
Docs/Game-Design/ → Docs/02-Design/
Docs/Workflow/Templates/ → Docs/05-Templates/
Docs/Workflow/Personas/ → Docs/04-Personas/
```

**Files Likely Affected**:
- All persona files (references to templates, workflow)
- Workflow.md (links to various docs)
- Post-mortems (links to architecture docs)
- Templates (cross-references)
- Any file with navigation sections

**Done When**:
- All internal links verified and working
- No 404s when following documentation links
- Key navigation paths tested
- Link consistency report generated
- CI could verify links (future enhancement)

**Tech Lead Note**: This is immediate technical debt from the reorganization. Should be done before any new documentation is created to avoid compounding the problem.

### TD_014: Add Property-Based Tests for Swap Mechanic [Score: 45/100]
**Status**: Approved (Deferred)
**Owner**: Dev Engineer (after MVP)
**Size**: XS (immediate) + M (future property suite)
**Priority**: Ideas (not critical path)
**Created**: 2025-08-19
**Proposed By**: Test Specialist
**Markers**: [QUALITY] [TESTING]

**What**: Implement FSCheck property tests for swap operation invariants
**Why**: Catch edge cases that example-based tests might miss, ensure mathematical properties hold

**Tech Lead Decision** (2025-08-18):
✅ **APPROVED with modifications - Defer to after MVP**

**Analysis**:
- Current swap has only 2 example-based tests
- Property tests would catch edge cases we haven't thought of
- FSCheck is mature and well-suited for game logic invariants
- Swap operation has clear mathematical properties to verify

**However**: 
- We have only 2 swap tests currently - not enough surface area yet
- Property tests shine when you have complex state spaces
- Current swap is relatively simple (range check + position swap)

**Modified Approach**:
1. **Immediate** (5 min): Add 2-3 more example-based tests for critical cases:
   - Swap with boundary blocks
   - Failed swap attempts (out of range)
   - Swap with same block (should fail gracefully)

2. **After MVP** (when swap gets complex):
   - Implement full property-based test suite
   - Add generators for game states
   - Test invariants across all block operations

**Rationale**:
- Property tests are valuable but premature optimization now
- With only 2 tests, we need basic coverage first
- When swap mechanics get complex (power-ups, constraints), revisit

**Proposed Properties** (Future Implementation):
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

**Done When**:
- **Immediate**: 2-3 additional example tests added and passing
- **Future**: Property tests integrated with 1000+ generated cases
- Edge cases discovered are documented
- CI pipeline includes property test execution

### TD_001: Extract Input System to Separate Feature Module [Score: 45/100]
**Status**: Done ✅
**Owner**: Dev Engineer
**Size**: S (1 hour actual) - REVISED DOWN from M
**Completed**: 2025-08-19
**Priority**: Ideas (after VS_001)
**Markers**: [ARCHITECTURE]

**Tech Lead Ultra-Think Decision** (2025-08-19): 
⚠️ **ARCHITECTURE CHALLENGE: Original design is over-engineered**

After deep analysis of existing code, the proposed two-layer architecture adds unnecessary complexity. Current system already has clean separation via MediatR.

**Problem Analysis**:
- Current: `Godot Input → BlockInputManager → MediatR Command` (2 layers, works well)
- Proposed: `Godot → Adapter → Domain Input → Router → Commands` (4 layers, over-engineered)
- **Real Issue**: Input handlers are scattered across 4 classes with some duplication

**Revised Simpler Solution**:
```
godot_project/features/block/input/
├── BlockInputManager.cs        # Keep as central router (existing)
├── InputMappings.cs            # Extract key bindings to config (new)
├── InputStateManager.cs        # Track modifiers/modes (refactor from SelectionManager)
└── UnifiedInputHandler.cs      # Consolidate 4 handlers into 1 (refactor)
```

**Why This is Better**:
- **No new architectural layers** - MediatR already provides decoupling
- **Solves actual problem** - Consolidates scattered handlers
- **1-2 hours vs 4-6 hours** - Much simpler implementation
- **Easier to debug** - Less indirection
- **Follows YAGNI** - Don't add abstraction until needed

**Implementation Approach**:
1. Consolidate BlockPlacementHandler, BlockInspectionHandler, BlockMovementHandler into UnifiedInputHandler
2. Extract key mappings to configuration class
3. Keep sending commands via existing MediatR pattern
4. Add tests for the consolidated handler

**Tech Lead Rationale**:
The best architecture isn't the most "pure" - it's the simplest that solves the real problem. Current MediatR pattern already provides testability and decoupling. Adding more layers just makes debugging harder without clear benefit.

**Done When**:
- Input handlers consolidated into single class
- Key mappings extracted to configuration
- Existing tests still pass
- No new architectural layers added


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