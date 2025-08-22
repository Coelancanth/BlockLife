# BlockLife Developer Handbook

**Last Updated**: 2025-08-21  
**Purpose**: Single source of truth for daily development - everything you need in one place

## 📍 Navigation

- **Need a term definition?** → [GLOSSARY.md](Glossary.md)
- **Major architecture decision?** → [ADR Directory](ADR/)
- **Everything else?** → It's in this handbook

---

## 🏗️ Core Architecture

### Clean Architecture + MVP Pattern
- **Core Layer** (`src/`): Pure C# business logic, no Godot dependencies
- **Application Layer**: Commands, Queries, Handlers (MediatR)
- **Godot Integration** (`godot_project/`): Views, Presenters, Scene files
- **Separation**: Core can be tested without Godot

### Reference Implementation
**The Gold Standard**: `src/Features/Block/Move/`
- Copy this structure for ALL new features
- Demonstrates proper separation of concerns
- Shows testing patterns and coverage
- Includes MVP integration with Godot

### Grid Coordinate System (CRITICAL)
```
Y↑
3 □ □ □ □
2 □ □ □ □  
1 □ □ □ □
0 □ □ □ □
  0 1 2 3 → X

- Origin: (0,0) at bottom-left
- X increases rightward, Y increases upward
- Access: grid[x,y] consistently
- Godot aligned: Y+ is up
```

**Common Mistakes**:
- ❌ Using top-left origin (screen coordinates)
- ❌ Mixing row-major vs column-major access
- ✅ Always validate coordinates before array access

### Namespace Rules
- **NEVER** name a class the same as its containing namespace
- Causes resolution issues and confusing errors

---

## 💻 Development Workflow

### Branch Naming Convention (CRITICAL)
- **Feature**: `feat/VS_003-description`
- **Bug Fix**: `fix/BR_012-description`
- **Tech Debt**: `tech/TD_042-description`
- **Use underscores**: `VS_003` not `vs-003` (matches Backlog.md)

### Memory Bank Protocol (ADR-004 v3.0)
- **Location**: `.claude/memory-bank/active/[persona].md` and `session-log.md` (local-only, in .gitignore)
- **When to update**: Pre-push hook reminds you
- **What to record**: High-value context only (decisions, patterns, blockers, all git status)
- **No buffers**: Valuable patterns → Docs/03-Reference/ immediately or delete

### Persona System (ADR-004: Single-Repo)
- **Architecture**: Single repository with context management (supersedes multi-clone ADR-002)
- **Switching**: `/clear` → `embody [persona]` → auto-pulls latest
- **Script**: `./scripts/persona/embody.ps1 [persona]` - sets git identity and syncs
- **Context**: Preserved in `.claude/memory-bank/sessions/[persona].md`
- **Workflow**: Sequential solo dev - commit before switching personas

### Build & Test Commands

```bash
# Windows
./scripts/core/build.ps1 test    # Build + tests (commit-safe)
./scripts/core/build.ps1 test-only  # Tests only (dev iteration)

# Linux/Mac
./scripts/core/build.sh test     # Build + tests (commit-safe)
```

**CRITICAL**: Use `test` before committing (catches Godot compilation issues)

---

## 🎯 Persona Routing

### Quick Decision Matrix

| Work Type | Goes To | Key Responsibility | Don't Send To |
|-----------|---------|-------------------|---------------|
| New Features | Product Owner | Creates VS items | Tech Lead (until defined) |
| Technical Planning | Tech Lead | Breaks down, approves TD | Dev Engineer (direct) |
| Implementation | Dev Engineer | Builds features | Test Specialist (too early) |
| Testing | Test Specialist | Creates tests, finds bugs | Dev Engineer (for strategy) |
| Complex Bugs (>30min) | Debugger Expert | Deep investigation | Dev Engineer (timebox exceeded) |
| CI/CD & Tools | DevOps Engineer | Automation, infrastructure | Dev Engineer (for scripts) |

### Detailed Work Routing Rules

#### 🎯 By Work Category

| Category | Primary Owner | Secondary Owner | Never Route To |
|----------|--------------|-----------------|----------------|
| **Requirements & Features** |
| User stories | Product Owner | - | Tech Lead (before definition) |
| Acceptance criteria | Product Owner | Test Specialist | Dev Engineer |
| Priority decisions | Product Owner | - | Any technical persona |
| **Architecture & Design** |
| Pattern selection | Tech Lead | - | Dev Engineer |
| ADR creation | Tech Lead | - | Test Specialist |
| System design | Tech Lead | - | DevOps Engineer |
| **Implementation** |
| Feature coding | Dev Engineer | - | Test Specialist |
| Bug fixes (<30min) | Dev Engineer | - | Debugger Expert |
| Refactoring | Dev Engineer | Tech Lead (approval) | Test Specialist |
| **Quality & Testing** |
| Test strategy | Test Specialist | - | Dev Engineer |
| Unit test creation | Test Specialist | Dev Engineer (TDD green) | Product Owner |
| Bug reports | Test Specialist | Debugger Expert (complex) | Product Owner |
| **Infrastructure** |
| Build scripts | DevOps Engineer | - | Dev Engineer |
| CI/CD pipeline | DevOps Engineer | - | Test Specialist |
| Automation tools | DevOps Engineer | - | Tech Lead |
| **Investigation** |
| Complex bugs | Debugger Expert | - | Dev Engineer (after 30min) |
| Performance issues | Debugger Expert | DevOps Engineer | Test Specialist |
| State corruption | Debugger Expert | - | Product Owner |

#### 🔄 Handoff Triggers

| From | To | When |
|------|-----|------|
| Product Owner | Tech Lead | VS item fully defined with acceptance criteria |
| Tech Lead | Dev Engineer | Technical approach documented and approved |
| Test Specialist | Dev Engineer | Failing tests written (TDD red phase) |
| Dev Engineer | Test Specialist | Implementation complete, ready for validation |
| Dev Engineer | Debugger Expert | 30min debugging timebox exceeded |
| Debugger Expert | Dev Engineer | Root cause identified with clear fix |
| DevOps Engineer | Dev Engineer | Automation requires business logic changes |
| Any Persona | Product Owner | Requirements unclear or need priority decision |

#### ⚠️ Common Routing Mistakes

| Wrong Route | Right Route | Why |
|-------------|------------|-----|
| PO → Dev for requirements | PO → Tech Lead → Dev | Tech Lead needs to plan approach |
| Dev → PO for bugs | Dev → Test Specialist | Test validates, PO accepts features |
| Test → Dev for test strategy | Test owns strategy | Dev implements, doesn't design tests |
| Dev → DevOps for simple scripts | Dev implements | DevOps for infrastructure only |
| Tech Lead → Debugger for design | Tech Lead owns | Debugger investigates, not designs |

#### 🎲 When in Doubt

1. **Feature Work**: Product Owner → Tech Lead → Dev Engineer → Test Specialist
2. **Bug Work**: Test Specialist → Debugger Expert (if complex) → Dev Engineer
3. **Infrastructure**: DevOps Engineer (unless business logic involved)
4. **30-Minute Rule**: Any debugging > 30min → Debugger Expert
5. **Approval Needed**: All TD items → Tech Lead for approval

### Work Item Types
- **VS_xxx**: Vertical Slice (new feature) - Owner: Product Owner → Tech Lead → Dev
- **BR_xxx**: Bug Report (defect) - Owner: Test Specialist → Debugger/Dev
- **TD_xxx**: Technical Debt (improvement) - Owner: Tech Lead (approval) → Dev

---

## 🧪 Testing Patterns

### Test Types & Usage

1. **Unit Tests** (Most common)
   - Pure business logic
   - Location: `tests/BlockLife.Core.Tests/`
   - Speed: <10ms

2. **Integration Tests**
   - End-to-end flows
   - Location: `tests/GdUnit4/Features/`
   - Speed: <100ms

3. **Architecture Tests** (Required)
   - Enforce patterns
   - Defend decisions

### LanguageExt Testing Patterns

#### Testing Fin<T> Results
```csharp
// ✅ Test success
result.IsSucc.Should().BeTrue();
result.IfSucc(value => value.Id.Should().Be(expectedId));

// ✅ Test failure
result.IsFail.Should().BeTrue();
result.IfFail(error => error.Code.Should().Be("VALIDATION_ERROR"));

// ✅ Pattern matching
result.Match(
    Succ: value => value.State.Should().Be(ExpectedState.Active),
    Fail: error => Assert.Fail($"Expected success: {error.Message}")
);
```

#### Testing Option<T>
```csharp
option.IsSome.Should().BeTrue();
option.IfSome(entity => {
    entity.Name.Should().Be("Expected");
    entity.IsActive.Should().BeTrue();
});
```

**Key Rule**: Everything returns `Fin<T>` - no exceptions thrown

---

## 📐 Implementation Patterns

### Git Hook Auto-Installation
**Pattern**: Configure Husky in .csproj for zero-config setup  
**Example**: `BlockLife.Core.csproj:22-25`  
**Rationale**: Hooks auto-install on `dotnet tool restore` across all clones  

### Branch Naming Convention  
**Pattern**: Use underscores for work items (VS_003 not vs-003)  
**Example**: `feat/VS_003-save-system`  
**Rationale**: Matches Backlog.md format exactly  

### CI Branch Freshness Check
**Pattern**: Fail PRs that are >20 commits behind main  
**Example**: `.github/workflows/ci.yml:49-99`  
**Rationale**: Prevents surprise conflicts during merge  

---

## 🚫 Anti-Patterns to Avoid

### ❌ Direct Godot Access from Domain
```csharp
// WRONG - Never reference Godot in domain
public class BlockService {
    private Node2D _sprite; 
}
```

### ❌ Skipping Error Handling
```csharp
// WRONG - Assumes success
var result = service.Execute();
return result.Value;

// RIGHT - Handle both paths  
return service.Execute()
    .Match(
        success => success,
        failure => HandleError(failure)
    );
```

### ❌ Creating Files Without Need
- Never create documentation proactively
- Always prefer editing existing files
- Only create files when explicitly required

### ❌ Comments Unless Asked
```csharp
// WRONG (unless requested)
// This method moves the block
public void MoveBlock() { }

// RIGHT - Clean code without comments
public void MoveBlock() { }
```

---

## 🐛 Common Bug Patterns

### GUID Stability Issues
```csharp
// ❌ DANGEROUS - New GUID each access
public Guid BlockId => RequestedId ?? Guid.NewGuid();

// ✅ SAFE - Cached stable value
private readonly Lazy<Guid> _blockId = new(() => Guid.NewGuid());
public Guid BlockId => RequestedId ?? _blockId.Value;
```

### DI Registration Issues
```csharp
// ❌ Presenter as notification handler
public class MyPresenter : INotificationHandler<Event>

// ✅ Separate handler + factory
public class MyPresenter  // No MediatR interfaces
```

### Missing Validation
```csharp
// ❌ Direct state change
await _gridService.RemoveBlock(id);

// ✅ Validate first
var validation = await _validator.ValidateRemoval(id);
if (validation.IsFail) return validation.Error;
await _gridService.RemoveBlock(id);
```

---

## 📏 C# Naming Standards

### Classes, Interfaces, Methods
- **PascalCase** for all public members
- Interfaces start with `I`

```csharp
public class BlockPlacementService
public interface IBlockRepository
public void ProcessCommand()
public string PlayerName { get; set; }
```

### Fields and Variables
- **Private fields**: `_camelCase` with underscore
- **Local variables**: `camelCase`
- **Parameters**: `camelCase`

```csharp
private readonly ILogger _logger;
var validationResult = ValidateMove(targetPosition);
```

---

## ✅ Subagent Verification

### Trust but Verify (10-Second Rule)

When subagents complete work, perform quick checks:

```bash
# File modified?
git status | grep Backlog.md

# Content added?
grep "TD_041" Backlog.md

# Status changed?
grep "Status: Completed"
```

**Common False Completions**:
- Partial updates (some changes missed)
- Wrong location (added to wrong section)
- Format issues (broken template)

**Goal**: Quick confidence check, not full audit

---

## 📚 Lessons Learned

### Critical Gotchas

1. **GdUnit4 Tests Not Found**
   - **Fix**: Add `[TestSuite]` attribute to test classes
   - **Time wasted**: 45 minutes

2. **Godot Compilation vs C# Tests**
   - **Issue**: Tests pass but game won't compile
   - **Fix**: Use `build.ps1 test` not `test-only`

3. **Husky Hook Path**
   - **Issue**: Hooks not executing after installation
   - **Fix**: `dotnet husky install` sets core.hookspath automatically
   - **Debug**: `git config --get core.hookspath` should return `.husky`

4. **CI Status Job Dependencies**
   - **Issue**: Status job fails with missing dependency
   - **Fix**: Include all jobs in needs array: `[build-and-test, code-quality, branch-freshness]`
   - **Location**: `.github/workflows/ci.yml:278`

5. **Documentation Sprawl**
   - **Issue**: Same info in 4+ files, hard to find anything
   - **Solution**: Consolidated into HANDBOOK.md (4,675 → 800 lines)
   - **Lesson**: Enforce single source of truth ruthlessly

6. **dotnet format Needs .sln**
   - **Issue**: "找到 MSBuild 项目文件"
   - **Fix**: Specify `BlockLife.sln` explicitly

---

## 🎯 Quick Commands

### Testing Commands
```bash
# Run specific test suite
dotnet test --filter "FullyQualifiedName~MoveBlock"

# Run unit tests only (skip integration)
dotnet test --no-build --filter "Category!=Integration"

# Quick build check
dotnet build --no-restore
```

---

## 🎯 Technology Decisions

### Build System: MSBuild + PowerShell
**Why**: Cross-platform support, familiar to C# developers  
**Alternative Considered**: Make, CMake  
**Decision Factor**: Windows-first development environment  

### Test Framework: XUnit + GdUnit4
**Why**: XUnit for domain, GdUnit4 for Godot integration  
**Alternative Considered**: NUnit, MSTest  
**Decision Factor**: Better async support in XUnit  

### DI Container: Microsoft.Extensions.DependencyInjection
**Why**: Standard, well-documented, integrated with .NET  
**Alternative Considered**: Autofac, Unity  
**Decision Factor**: Simplicity and standards compliance  

### State Management: Service-based with DI
**Why**: Simple, testable, no hidden dependencies  
**Alternative Considered**: Redux-style, Event sourcing  
**Decision Factor**: Complexity not justified for current scope  

### Git Strategy: Feature Branches + Squash Merge
**Why**: Clean main history, detailed feature history  
**Alternative**: GitFlow, GitHub Flow  
**Decision Factor**: Simplicity with safety  

### Git Hooks: Husky.NET
**Why**: Zero-config across clones, automatic installation  
**Alternative Considered**: pre-commit (Python), manual scripts  
**Decision Factor**: Native to .NET ecosystem  

### Critical Warning Enforcement (TD_057)
**Defense in Depth**: Multi-layer protection against protocol violations

**Enforcement Levels**:
1. **Client-Side Blocking** (`.husky/pre-push`): 
   - Direct main push → `exit 1` (BLOCKS push)
   - Clear workflow guidance with emergency override
   - Override: `git push --no-verify` (emergencies only)

2. **Server-Side Protection** (GitHub Branch Protection):
   - Required: Enable "Restrict pushes to matching branches" 
   - Required: Set "Enforce admins" to true
   - Effect: Makes direct main push technically impossible

3. **Quality Gates**: All PRs must pass CI before merge

**Override Protocol**: 
- Emergency hotfixes only: Use `--no-verify` + immediate PR
- Document override reason in commit message
- Never override for convenience

### Branch Alignment Intelligence (TD_058)
**Semantic Workflow Validation**: Educational guidance at commit time

**Validation Layers**:
1. **Work Item Alignment** (`.husky/pre-commit`):
   - Detects branch context vs commit content mismatches
   - Example: TD work on VS branch → Educational warning
   - Performance: <0.5s execution time

2. **Work Type Consistency**:
   - Validates branch type (feat/, tech/, fix/) vs commit type
   - Exception handling: docs and test commits accepted everywhere
   - Clear guidance for type misalignments

3. **Main Branch Detection** (Backup):
   - Warns about upcoming pre-push block
   - Provides feature branch creation guidance

**Educational Approach**: 
- Non-blocking warnings with actionable advice
- Clear explanations of expected alignment
- Integration with existing atomic commit guidance

**Success Metrics**: Reduction in mixed-concern PRs, improved git history clarity

---

## ❌ Rejected Approaches

### Microservices Architecture
**Rejected Because**: Over-engineering for a game  
**Complexity**: 9/10  
**Benefit**: 2/10 for current scope  

### Event Sourcing
**Rejected Because**: Unnecessary complexity  
**Complexity**: 8/10  
**Benefit**: 1/10 for current needs  

### Custom Build System
**Rejected Because**: MSBuild works fine  
**Complexity**: 7/10  
**Benefit**: 1/10 marginal gains  

### Worktree-based Development (Sacred Sequence)
**Rejected Because**: Conflicts and complexity  
**Replaced With**: Multi-clone architecture  
**Result**: Much cleaner workflow  

### Complex Memory Bank Sync (TD_051)
**Rejected Because**: Over-engineered 250-line solution  
**Replaced With**: Local-only Memory Bank (TD_053/054)  
**Result**: Radical simplification  

---

## 📖 External References

- **Architecture Decisions**: [ADR Directory](ADR/)
- **Term Definitions**: [GLOSSARY.md](Glossary.md)
- **Context7 Library Access**: Use `mcp__context7__get-library-docs` for LanguageExt, MediatR
- **Move Block Reference**: `src/Features/Block/Move/` (copy this pattern)

---

## 🔄 Document History

This handbook consolidates and replaces:
- QuickReference.md (668 lines)
- Architecture.md (380 lines)
- Patterns.md (310 lines)
- Standards.md (318 lines)
- Testing.md (298 lines)
- GitWorkflow.md (353 lines)
- MemoryBankProtocol.md (284 lines)
- SubagentVerification.md (113 lines)
- ClaudeCodeBestPractices.md (266 lines)

**Reduction**: 3,590 lines → ~400 lines (89% reduction, 100% value retained)