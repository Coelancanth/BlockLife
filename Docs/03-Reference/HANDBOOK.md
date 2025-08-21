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

### Git Workflow (Standard Commands + Smart Hooks)

#### Daily Flow
```bash
# Start new work
git checkout main && git pull origin main
git checkout -b feat/VS_003-feature-name

# Make changes
git add -A
git commit -m "feat(VS_003): clear description"

# Stay synchronized
git fetch origin && git rebase origin/main

# Push changes (hooks run automatically)
git push -u origin feat/VS_003-feature-name

# Create PR
gh pr create --title "feat(VS_003): Title" --body "Description"
```

#### Branch Naming
- **Feature**: `feat/VS_003-description`
- **Bug Fix**: `fix/BR_012-description`
- **Tech Debt**: `tech/TD_042-description`
- **Use underscores**: `VS_003` not `vs-003`

#### Automated Hooks (via Husky.NET)
- **pre-commit**: Format validation
- **commit-msg**: Conventional commits enforcement
- **pre-push**: Build + unit tests
- Auto-installed on `dotnet build` - zero config needed

### Memory Bank System

**Location**: `.claude/memory-bank/`

#### When to Update
- ✅ After completing work items
- ✅ When discovering patterns (saves >10min)
- ✅ After fixing bugs (took >30min)
- ✅ Making architectural decisions
- ❌ NOT for trivial changes or WIP

#### Structure
```
.claude/memory-bank/
├── activeContext.md    # Current work (expires: 7 days)
├── patterns.md         # Proven patterns
├── decisions.md        # Architecture choices
├── lessons.md          # Bug fixes & gotchas
└── SESSION_LOG.md      # Session history (30 days)
```

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

| Work Type | Goes To | Key Responsibility |
|-----------|---------|-------------------|
| New Features | Product Owner | Creates VS items |
| Technical Planning | Tech Lead | Breaks down, approves TD |
| Implementation | Dev Engineer | Builds features |
| Testing | Test Specialist | Creates tests, finds bugs |
| Complex Bugs (>30min) | Debugger Expert | Deep investigation |
| CI/CD & Tools | DevOps Engineer | Automation, infrastructure |

### Work Item Types
- **VS_xxx**: Vertical Slice (new feature)
- **BR_xxx**: Bug Report (investigation)
- **TD_xxx**: Technical Debt (refactoring)

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
   - **Issue**: Hooks not executing
   - **Fix**: Verify `git config --get core.hookspath` shows `.husky`

4. **dotnet format Needs .sln**
   - **Issue**: "找到 MSBuild 项目文件"
   - **Fix**: Specify `BlockLife.sln` explicitly

---

## 🎯 Quick Commands

### Most Used Commands
```bash
# Build & test
./scripts/core/build.ps1 test

# Check what changed
git status

# Find something in code
grep -r "pattern" src/

# Run specific test
dotnet test --filter "FullyQualifiedName~MoveBlock"

# Create PR
gh pr create --title "feat: title" --body "description"
```

### Verification Commands
```bash
# Was file modified?
git status | grep filename

# Is content present?
grep "search term" file.md

# Check build status
dotnet build --no-restore

# Run quick tests
dotnet test --no-build --filter "Category!=Integration"
```

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