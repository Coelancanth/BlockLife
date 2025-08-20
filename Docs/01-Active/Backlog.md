# BlockLife Development Backlog

**Last Updated**: 2025-08-20

## 🔢 Next Item Numbers by Type
**CRITICAL**: Before creating new items, check and update the appropriate counter.

- **Next BR**: 012 (Last: BR_011 - 2025-08-20)
- **Next TD**: 029 (Last: TD_028 - 2025-08-20)  
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

### TD_023: Implement Persona Worktree System - Automated Isolation Workspaces ✅ APPROVED (MODIFIED)
**Status**: Approved - Phased Implementation
**Owner**: DevOps Engineer
**Size**: S (2 hours Phase 1) + M (4 hours total if expanded)
**Priority**: Important (moved up - actual pain point)
**Created**: 2025-08-19
**Reviewed**: 2025-08-19
**Markers**: [PRODUCTIVITY] [SOLO-DEV]

**What**: Automated persona workspace isolation using git worktrees
**Why**: Solo dev with frequent persona switching experiencing real friction with standard git flow

**Tech Lead Decision** (2025-08-19):
✅ **APPROVED WITH MODIFICATIONS**

**Context That Changed Decision**:
- Solo developer, not team (different dynamics)
- Frequent persona switching confirmed
- Standard git flow already tried and insufficient
- Actual productivity impact measured

**Phased Implementation**:

**Phase 1** (2 hours - START HERE):
```powershell
# Simple switch script
./scripts/persona/switch-persona.ps1 dev-engineer
# Creates worktree if needed, switches context
```
- Just Dev Engineer + Tech Lead personas
- Basic PowerShell script
- Manual invocation
- Measure if it helps

**Phase 2** (If Phase 1 proves valuable):
- Add remaining personas
- State management
- Cleanup utilities

**Phase 3** (If heavily used):
- Slash command integration
- Cross-platform support
- Full automation

**Success Metrics**:
- Reduces context switch time by >50%
- Eliminates merge conflicts from persona switches
- Actually gets used daily

**Done When (Phase 1)**:
- Basic switch script working
- Two personas supported
- Documentation created
- Friction measurably reduced

**Problem Solved**:
- Multiple persona sessions conflict on same branch (file locks, merge conflicts)
- Manual branch switching between personas is error-prone
- Context switching overhead reduces persona effectiveness
- Coordination complexity grows exponentially with concurrent personas

**Solution - Persona Worktree System**:
- Single command `/embody dev-engineer` automatically creates worktree + switches directory + activates persona
- Each persona gets isolated workspace using native git worktree functionality
- Zero maintenance overhead (git handles all complexity)
- Perfect isolation - no shared files, branches, or state

**Key Benefits**:
- **Complete Isolation**: No more file conflicts between persona sessions
- **Zero Context Switch**: Instant persona activation with full environment
- **Native Git**: Uses proven git worktree functionality (stable, zero bugs)
- **Single Command**: `/embody persona-name` handles everything automatically
- **9 Hours Total**: vs weeks for complex coordination systems

**Implementation Components**:
1. **Custom Slash Command** (2h): `/embody` command with persona parameter
2. **Worktree Management** (3h): Create, switch, cleanup worktree operations
3. **Cross-Platform Scripts** (2h): Bash + PowerShell automation scripts
4. **Management Utilities** (1h): Status, cleanup, workspace listing commands
5. **Documentation** (1h): Usage guide and troubleshooting

**Technical Approach**:
```bash
# Single command does everything:
/embody dev-engineer
  → git worktree add ../blocklife-dev-engineer
  → cd ../blocklife-dev-engineer
  → activate dev-engineer persona
  → ready for isolated development
```

**Reference**: Comprehensive design document at `Docs/02-Design/PersonaWorktreeSystem.md`

**Done When**:
- `/embody persona-name` creates isolated workspace automatically
- Cross-platform support (Windows/Linux/Mac)
- Workspace management commands (status, cleanup, list)
- Complete persona isolation achieved
- Documentation and troubleshooting guide complete
- 5+ tests for worktree operations

**Depends On**: None

## 📈 Important (Do Next)
*Core features for current milestone, technical debt affecting velocity*




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