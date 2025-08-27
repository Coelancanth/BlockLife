# BlockLife Developer Handbook

**Last Updated**: 2025-08-23  
**Purpose**: Single source of truth for daily development - everything you need in one place

## 🔍 Quick Reference Protocol - Find What You Need FAST

### When You Need Help With...

**🐛 Debugging an Issue:**
1. **Namespace/DI errors** → Jump to [Common Bug Patterns](#-common-bug-patterns)
2. **Test failures** → Check [Testing.md](Testing.md) troubleshooting
3. **Build errors** → See [Critical Gotchas](#critical-gotchas) (20 common issues)

**💻 Writing Code:**
1. **New feature** → Copy from `src/Features/Block/Move/` reference
2. **Test patterns** → [Testing.md](Testing.md) has all patterns
3. **Architecture questions** → [Core Architecture](#-core-architecture)

**🔧 Common Tasks:**
1. **Run tests** → [Quick Commands](#-quick-commands)
2. **Switch personas** → [Persona System](#persona-system-adr-004-single-repo)
3. **Create PR** → [Branch Naming](#branch-naming-convention-critical)

**❓ "How Do I...?" Questions:**
- **Use LanguageExt?** → [Testing Patterns](#languageext-testing-patterns)
- **Mock with Moq?** → [Testing.md](Testing.md#-mocking-with-moq)
- **Handle errors?** → Everything returns `Fin<T>` (no exceptions)
- **Route work?** → [Persona Routing](#-persona-routing)

**🚨 Emergency Fixes:**
- **30+ test failures** → Check DI registration in GameStrapper.cs
- **Handler not found** → Verify namespace is `BlockLife.Core.*`
- **First-time setup** → Run `dotnet tool restore` then `./scripts/core/build.ps1 test`

## 📍 Navigation

- **Need a term definition?** → [GLOSSARY.md](Glossary.md)
- **Major architecture decision?** → [ADR Directory](ADR/)
- **Testing guide?** → [Testing.md](Testing.md)
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

#### Core Build Scripts
```bash
# Windows
./scripts/core/build.ps1 test    # Build + tests (commit-safe)
./scripts/core/build.ps1 test-only  # Tests only (dev iteration)

# Linux/Mac
./scripts/core/build.sh test     # Build + tests (commit-safe)
```

#### Test Execution Scripts (TD_071)
```bash
# Quick Tests (Architecture only, ~1.3s)
./scripts/test/quick.ps1         # Run before committing

# Full Test Suite (Staged execution, ~3-5s)
./scripts/test/full.ps1          # Complete validation
./scripts/test/full.ps1 -SkipSlow  # Skip Performance/Stress tests

# Future: Incremental Testing (TD_077)
./scripts/test/incremental.ps1   # Only test changed code (~2s)
```

#### When to Use Each
- **Developing**: `quick.ps1` for rapid feedback
- **Before commit**: `build.ps1 test` for full validation
- **Before PR**: `full.ps1` for complete test coverage
- **CI/CD**: Automatic staged execution

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

#### Testing with LanguageExt Collections
```csharp
// ✅ Correct Seq initialization
new[] { item }.ToSeq()
new IPattern[] { item }.ToSeq()

// ✅ Correct Map construction
Map((key, value))  // NOT Map<K,V>((key, value))
```

#### Property-Based Testing with FsCheck 3.x
```csharp
// Generators return Gen<T> directly
private static Gen<BlockType> GenBlockType() =>
    Gen.Elements(BlockType.Work, BlockType.Study, ...);

// Use .ToArbitrary() for Prop.ForAll
Prop.ForAll(
    GenConnectedPattern().ToArbitrary(),
    (positions) => { /* test invariant */ }
).QuickCheckThrowOnFailure();
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

### Phase-Based Implementation for Large Features
**Pattern**: Break features into discrete phases to prevent context exhaustion  
**Example**: VS_003A implemented in 5 phases across separate sessions  
**Benefits**:  
- Prevents 10+ hour continuous sessions
- Each phase independently testable
- Clear progress tracking
- Reduced cognitive load

### MVP Pattern for UI Integration
**Pattern**: Humble views with business logic in presenters  
```csharp
// View: Only display logic
public partial class MatchRewardsView : Control, IMatchRewardsView {
    public void DisplayRewards(RewardData data) => _label.Text = data.Text;
}

// Presenter: Business logic and coordination
public class MatchRewardsPresenter {
    public void ProcessMatch(MatchPattern pattern) {
        var rewards = CalculateRewards(pattern);
        _view.DisplayRewards(rewards);
    }
}
```

### Performance Optimization Pattern
**Pattern**: Measure first, optimize specific bottlenecks  
**Example**: CanRecognizeAt() pre-validation achieved 4.20x speedup  
```csharp
// Pre-check before expensive operation
if (!CanRecognizeAt(position)) return None;
// Now do expensive flood-fill
return PerformFloodFill(position);
```  

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

### ❌ Assuming Constructor Parameters (Modern C# Pattern)
```csharp
// ❌ WRONG - Old pattern assumption
public class PlayerState {
    public PlayerState(string name, int level) { }
}

// ✅ CORRECT - Modern required properties
public class PlayerState {
    public required string Name { get; init; }
    public required int Level { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

### ❌ Wrong Test Framework
```csharp
// ❌ WRONG - Assuming NSubstitute
var mock = Substitute.For<IService>();

// ✅ CORRECT - Check project first
// Run: grep -r "using Moq" tests/
var mock = new Mock<IService>();
mock.Setup(x => x.Method()).Returns(value);
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

### Namespace Mismatch Breaking MediatR Discovery (Critical)
```csharp
// ❌ WRONG - Silent failure, handler won't be discovered
namespace BlockLife.Features.Player
public class MyCommandHandler : IRequestHandler<MyCommand, Fin<Result>>

// ✅ CORRECT - Will be auto-discovered by MediatR
namespace BlockLife.Core.Features.Player
public class MyCommandHandler : IRequestHandler<MyCommand, Fin<Result>>
```
**Impact**: Handlers outside `BlockLife.Core.*` namespace are invisible to MediatR
**Prevention**: Always verify namespace matches assembly scanning configuration

### DI Cascade Failures
**Pattern**: Single missing registration causes multiple test failures
```csharp
// Missing this one line...
services.AddScoped<IPlayerStateService, PlayerStateService>();

// ...causes 30+ test failures across:
// - DI validation tests
// - Integration tests
// - Stress tests (constructor failures)
```
**Debugging**: When multiple tests fail, check DI registrations first

### Notification Layer Completeness (Critical)
**Pattern**: Model changes MUST notify view or nothing happens visually
```csharp
// ❌ WRONG - Updates model but view never knows
gridService.RemoveBlock(position);

// ✅ CORRECT - Model update + view notification
gridService.RemoveBlock(position);
_mediator.Publish(new BlockRemovedNotification(blockId, position, type, DateTime.UtcNow));
```
**Impact**: Without notifications, UI becomes disconnected from game state
**Prevention**: Every model change needs corresponding notification

### All Entry Points Must Trigger Patterns
**Pattern**: Game mechanics need handlers for ALL triggering events
```csharp
// ❌ WRONG - Only handles moves, not placements
public class ProcessPatternsAfterMoveHandler : INotificationHandler<BlockMovedNotification>

// ✅ CORRECT - Need both handlers
public class ProcessPatternsAfterMoveHandler : INotificationHandler<BlockMovedNotification>
public class ProcessPatternsAfterPlacementHandler : INotificationHandler<BlockPlacedNotification>
```
**Example**: Match detection failed because only BlockMovedNotification triggered patterns
**Prevention**: List all events that should trigger mechanics, ensure handlers exist

### Conditional Logic Coverage
**Pattern**: New features must work in ALL code paths
```csharp
// ❌ WRONG - Tier effects only in fallback path
if (BlockScene != null) {
    blockNode = BlockScene.Instantiate();  // Missing tier setup!
} else {
    // Tier effects only here
}

// ✅ CORRECT - Apply features in all branches
if (BlockScene != null) {
    blockNode = BlockScene.Instantiate();
    ApplyTierEffects(blockNode, tier);  // Apply everywhere
}
```
**Impact**: Features work inconsistently based on execution path

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

7. **Namespace Must Match for MediatR Discovery**
   - **Issue**: Handlers in `BlockLife.Features.*` silently ignored
   - **Fix**: MUST use `BlockLife.Core.Features.*` namespace
   - **Impact**: Single namespace error = 30+ test failures

8. **DI Registration Cascade Failures**
   - **Issue**: Missing IPlayerStateService causes widespread failures
   - **Fix**: Always register services in GameStrapper.cs

### Architectural Best Practices (From Memory Bank Extraction 2025-08-27)

#### 🔴 The Simplicity Principle (MOST CRITICAL)
**Before writing ANY code, ask**: "Can I add one condition to existing code?"
- **Red flag**: Solution > 100 lines for a "simple" feature
- **Example**: Merge pattern needed 5 lines, not 369 lines of new recognizer
- **Enforcement**: Estimate LOC before coding, >100 = mandatory design review

#### 🔴 Data Flow Completeness
**When adding fields to domain objects**: Trace through ENTIRE pipeline
- **Critical path**: Domain → Effect → Notification → Presenter → View
- **Common failure**: Adding field to domain but forgetting effect/notification layer
- **Example**: BlockPlacedEffect missing Tier field broke entire visualization
- **Prevention**: Create data flow checklist for new fields

#### 🔴 Reuse Before Create
**Exhaust reuse opportunities** before new abstractions:
- Check existing patterns, services, components FIRST
- Example: MatchPattern worked perfectly for merge, didn't need TierUpPattern
- Metric: 257 lines of focused code vs 500+ with new abstractions

#### 🟡 Service Lifetime Based on State
Choose DI lifetime based on statefulness, not convention:
- **Stateless services** → Singleton (e.g., PatternRecognizer, Executors)
- **Request state services** → Scoped (e.g., Handlers with context)
- **Transient state services** → Transient (rarely needed)
- **Common mistake**: Registering stateless services as Scoped causes resolution errors

#### 🟡 Defensive Programming with Fin<T>
Add comprehensive defensive checks using functional patterns:
- **Example**: MergePatternExecutor needs null checks, bounds validation
- **Solution**: Use Fin<T> error handling for graceful degradation
- **Impact**: Prevents runtime exceptions in production

#### 🟢 Ultra-Careful Development Approach
"Slow is smooth, smooth is fast" methodology:
- Ultra-think each step, validate before proceeding
- Example: VS_003B-2 completed in single session with E2E success
- Contrast: Previous sessions had more trial-and-error cycles

### Critical Process Patterns (From Memory Bank Extraction 2025-08-27)

#### 📋 Pre-Coding Checklist (MANDATORY)
Before writing ANY code:
- [ ] **Check Glossary.md** for correct terminology
- [ ] **Question arbitrary requirements** (e.g., "exactly 3" - why not 3+?)
- [ ] **Look for existing patterns** to reuse
- [ ] **Estimate lines of code** (>100 = review first)
- [ ] **Query Context7** for unfamiliar APIs (especially LanguageExt)

#### 🔍 Verification Protocol
Don't trust issue descriptions without verification:
- **Example**: TD_084 claimed 40% false violations
- **Prevention**: Always check actual code before accepting problem statements
- **Tool**: Use grep/search to verify claims quickly

#### 📝 Test Design Philosophy
Test effectiveness = (Coverage × Maintainability) ÷ Complexity
- **Failed approach**: 21-test suite, 45+ min fixing compilation
- **Successful approach**: 7 focused tests covering critical functionality
- **Principle**: Start simple, expand incrementally
- **Test defaults**: MUST match production defaults (e.g., PlayerState.MaxUnlockedTier)

#### 🎯 Strategic Deferral Pattern
Document what you're NOT fixing to prevent scope creep:
- Example: VS_003B-2 fixed 2 of 3 limitations, deferred tier detection
- Benefit: Delivers value faster, prevents over-engineering
- Communication: Be explicit about deferred items in commit messages
   - **Debug**: When multiple tests fail, check DI first

9. **Modern C# Required Properties**
   - **Issue**: Assuming constructor parameters
   - **Fix**: Use `required` with `init` properties
   - **Example**: `public required string Name { get; init; }`

10. **Test Framework Detection**
    - **Issue**: Using wrong mocking library
    - **Fix**: Run `grep -r "using Moq" tests/` first
    - **Note**: Project uses Moq, NOT NSubstitute

11. **LanguageExt Collection Initialization**
    - **Issue**: Wrong Seq/Map syntax
    - **Fix**: `new[] { item }.ToSeq()` and `Map((key, value))`
    - **Never**: `Seq<T>(item)` or `Map<K,V>((key, value))`

12. **Phase-Based Implementation**
    - **Issue**: Context exhaustion on large features
    - **Fix**: Break into discrete phases (VS_003A = 5 phases)
    - **Benefit**: Each phase independently testable

13. **Time Estimation Reality**
    - **Issue**: Features taking longer than expected
    - **Fix**: Multiply estimates by 1.5x
    - **Example**: VS_003A estimated 6.5h, actual 10h

14. **False Simplicity Trap**
    - **Issue**: Removing docs doesn't remove complexity
    - **Fix**: Define behaviors explicitly
    - **Remember**: Undefined ≠ Simple

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