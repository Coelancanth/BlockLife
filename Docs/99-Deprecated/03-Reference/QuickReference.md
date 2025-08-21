# 🤖 Agent Quick Reference

## 🧠 Memory Bank System (NEW)
**Persistent context between sessions**: `.claude/memory-bank/`
- **activeContext.md** - Current work focus (check first!)
- **patterns.md** - Established patterns to follow
- **decisions.md** - Why we made certain choices

## ✅ Verification Protocol
**Trust but Verify** - 10-second checks after subagent work:
- `./scripts/verify-subagent.ps1 -Type backlog`
- See [SubagentVerification.md](SubagentVerification.md) for patterns

## 📊 Best Practices (from Community)
**Quick Wins to adopt**:
- Memory Bank (✅ implemented)
- Chain of Draft mode for conciseness
- Parallel task processing with TodoWrite
- See [ClaudeCodeBestPractices.md](ClaudeCodeBestPractices.md) for full list

## 🎯 Reference Implementation (GOLD STANDARD)
**Copy this for ALL new work**: `src/Features/Block/Move/`
- File structure, naming, patterns
- C# and Godot integration
- Test organization and assertions

## 📐 Architecture Decision Records (ADRs)
**[ADR Directory](ADR/)** - Documented architectural decisions that guide implementation

### When to Check ADRs
- **Before implementing** features mentioned in ADRs
- **When creating** new architectural patterns
- **During code review** to verify compliance
- **When proposing** significant technical changes

### Current ADRs
- **[ADR-001](ADR/ADR-001-pattern-recognition-framework.md)**: Pattern Recognition Framework
  - Affects: VS_003A-D match/tier-up implementation
  - Key: IPattern, IPatternRecognizer interfaces
  - Benefit: Extensible without refactoring

### ADR Compliance in Code
```csharp
// Reference ADRs in implementation comments
// Following ADR-001: Pattern Recognition Framework
public class MatchPatternRecognizer : IPatternRecognizer
{
    // Implementation per ADR-001 specification
}
```

## 🎯 Persona Work Routing Matrix

**Quick Decision Guide**: When work comes in, use this table to route to the right persona.

| Work Type | Goes To | Red Flags (Don't Accept) |
|-----------|---------|--------------------------|
| **New Feature Definition** | Product Owner | If technical implementation details included |
| **Feature Breakdown** | Tech Lead | If requirements/acceptance criteria missing |
| **Code Implementation** | Dev Engineer | If tests not written or architecture unclear |
| **Test Creation** | Test Specialist | If acceptance criteria undefined |
| **Bug Investigation (>30min)** | Debugger Expert | If simple fix or reproduction steps missing |
| **CI/CD & Automation** | DevOps Engineer | If business logic changes required |

### 📋 Handoff Decision Tree

```
Work Request
     ↓
┌─────────────────────────┐
│ Is this a new feature   │──Yes──→ Product Owner
│ or requirement?         │        (VS creation)
└─────────┬───────────────┘
         No
          ↓
┌─────────────────────────┐
│ Is this architectural   │──Yes──→ Tech Lead
│ or design work?         │        (patterns, breakdown)
└─────────┬───────────────┘
         No
          ↓
┌─────────────────────────┐
│ Is this implementation  │──Yes──→ Dev Engineer
│ with clear tests?       │        (coding, integration)
└─────────┬───────────────┘
         No
          ↓
┌─────────────────────────┐
│ Is this test design     │──Yes──→ Test Specialist
│ or quality validation?  │        (testing, validation)
└─────────┬───────────────┘
         No
          ↓
┌─────────────────────────┐
│ Is this complex bug     │──Yes──→ Debugger Expert
│ investigation?          │        (>30min debugging)
└─────────┬───────────────┘
         No
          ↓
┌─────────────────────────┐
│ Is this automation      │──Yes──→ DevOps Engineer
│ or infrastructure?      │        (CI/CD, tooling)
└─────────────────────────┘
```

### 🚫 Common Routing Mistakes

❌ **Sending code to Product Owner** - They define WHAT, not HOW  
❌ **Asking Dev Engineer for architecture** - They implement patterns, don't create them  
❌ **Giving complex bugs to Dev Engineer** - They fix simple issues, Debugger handles complex  
❌ **Asking Test Specialist for requirements** - They validate requirements, don't create them  
❌ **Sending infrastructure work to Dev Engineer** - DevOps handles automation and tooling  

### ✅ Perfect Handoffs

✅ **Product Owner → Tech Lead**: "Here's a VS with clear acceptance criteria"  
✅ **Tech Lead → Dev Engineer**: "Here's the breakdown with patterns and sequence"  
✅ **Dev Engineer → Test Specialist**: "Implementation complete, ready for validation"  
✅ **Test Specialist → Debugger Expert**: "Complex failure, reproduction steps attached"  
✅ **Any Persona → DevOps Engineer**: "Manual process needs automation"  

### 🔥 Critical Lessons Learned

#### Workflow Optimization: Delegate Mechanical Tasks
- **ALWAYS delegate backlog mechanics to backlog-assistant** - Focus on decisions
- **Symptom**: Spending 30% of time on formatting/moving items instead of core work
- **Fix**: Use `/task backlog-assistant` for all mechanical updates
- **Example**: `Tech Lead decides → delegates updates → saves 15 min per session`
- *Lesson from*: Tech Lead workflow optimization (2025-08-18)

#### State Management Anti-Patterns
- **NEVER create dual state sources** - Use single service like GridStateService
- **Symptom**: Tests pass individually but fail in integration
- **Fix**: Consolidate to one source of truth
- *Lesson from*: Block Placement architecture stress test (2025-08-13)

#### Namespace Design Critical Rule
- **NEVER name a class the same as its containing namespace** - Causes compilation nightmares
- **Symptom**: 20+ files fail with "Block is a namespace but used like a type"
- **Fix**: Use plural for namespace (Blocks), singular for class (Block)
- **Example**: `namespace Domain.Blocks { class Block {} }` not `namespace Domain.Block { class Block {} }`
- *Lesson from*: TD_015/TD_016 implementation disaster (2025-01-19)

#### Serialization Design Constraints
- **AVOID `required` properties in domain models that need serialization** - Breaks JSON deserialization
- **Symptom**: Tests fail on round-trip serialization despite working runtime
- **Fix**: Use init-only properties or create DTOs for serialization boundaries
- **Example**: SaveData tests failing due to Block's `required` properties
- *Lesson from*: Save System implementation (2025-01-19)

#### Service Container Isolation
- **NEVER create parallel service containers** in tests
- **Symptom**: Commands succeed but notifications come from different instances
- **Fix**: Use production SceneRoot service provider in integration tests
- *Lesson from*: Integration test architecture investigation (2025-08-14)

#### Build Error Prevention Patterns
- **ALWAYS verify namespace locations** - Use Grep/Glob before assuming
- **Symptom**: "Type or namespace could not be found"
- **Fix**: `Grep "class ClassName"` to find actual location
- *Lesson from*: BlockInputManager refactoring (2025-08-17)

#### Type Alias Best Practices
- **USE type aliases for ambiguous types** in mixed frameworks
- **Symptom**: "Ambiguous reference between X and Y"
- **Fix**: `using LangError = LanguageExt.Common.Error;`
- *Lesson from*: Godot.Error vs LanguageExt.Error conflict (2025-08-17)

#### Lambda Type Consistency
- **ENSURE all Match branches return same type**
- **Symptom**: "Cannot convert lambda expression"
- **Fix**: All branches must return Unit.Default or same type
- *Lesson from*: Option<T>.Match pattern usage (2025-08-17)

## 📚 Context7 Documentation Integration

### What is Context7?
MCP-powered documentation service that provides instant access to library docs while coding. **Prevents assumption-based bugs by verifying APIs before implementation.**

### Critical Libraries to Query

#### LanguageExt Documentation
```bash
# Before using Error handling:
mcp__context7__resolve-library-id "LanguageExt"
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error"

# Common gotchas resolved:
- Error.Message returns code only (use ToString() for full)
- Fin<T> patterns for command handlers
- Option<T>.Match branch consistency
```

#### MediatR Documentation  
```bash
# Before implementing handlers:
mcp__context7__resolve-library-id "MediatR"
mcp__context7__get-library-docs "/jbogard/MediatR" --topic "handler registration"

# Common gotchas resolved:
- Auto-discovery includes ALL IRequestHandler implementations
- Presenters should NOT implement INotificationHandler
- Pipeline behavior registration
```

#### Godot C# API
```bash
# Before using Godot classes:
mcp__context7__resolve-library-id "Godot"
mcp__context7__get-library-docs "/godotengine/godot" --topic "C# Node"

# Common gotchas resolved:
- Node lifecycle methods
- Signal connection patterns
- Resource loading in C#
```

### When to Use Context7

**MANDATORY Context7 checks:**
- ❗ Before overriding base class methods
- ❗ Before using unfamiliar framework APIs
- ❗ When error messages reference unknown types
- ❗ Before implementing interface methods

**Query Pattern:**
```bash
# 1. Resolve library ID first
mcp__context7__resolve-library-id "library-name"

# 2. Get specific documentation
mcp__context7__get-library-docs "/org/project" --topic "specific-api"
```

### CI/CD Testing Patterns
- **NEVER test wall-clock time in CI** - You're testing infrastructure, not code
- **Symptom**: Tests pass in PR, fail on main after merge
- **Fix**: Skip timing tests in CI with `[Theory(Skip = "...")]`
- *Lesson from*: CI timing test failures (2025-08-18)

### Context7 Rules (from context7.json)
Our project-specific rules are indexed for AI assistance:
- Always use src/Features/Block/Move/ as reference
- Use Fin<Unit> for error handling
- Presenters need manual DI registration
- Test with BlockBuilder not BlockFactory
- Replace features properly: Build → Find → Disable → Test

## 💻 Dev Engineer Agent

### Command Handler Template
```csharp
public class FeatureCommandHandler : IRequestHandler<FeatureCommand, Fin<Unit>>
{
    private readonly IRequiredService _service;
    private readonly IMediator _mediator;
    
    public async Task<Fin<Unit>> Handle(FeatureCommand request, CancellationToken ct)
    {
        // 1. Validation
        if (request.IsInvalid)
            return Error.New("INVALID_REQUEST", "Details");
        
        // 2. Business logic
        var result = await _service.Execute(request.Data);
        if (result.IsFail) return result.Error;
        
        // 3. Notification
        await _mediator.Publish(new FeatureCompletedNotification(result.Value));
        return Unit.Default;
    }
}
```

### VSA File Structure
```
src/Features/[Domain]/[Feature]/
├── Commands/           # User intentions
├── Handlers/          # Business logic
├── Notifications/     # State change events
├── Services/          # Domain operations
└── ViewInterfaces/    # UI contracts
```

### Architecture Rules
- **No Godot in src/**: Keep domain pure
- **Fin<T> for errors**: No exceptions in business logic
- **Async/await properly**: Don't block main thread
- **Single responsibility**: One concern per class

### 🔥 Critical Implementation Lessons

#### Notification Pipeline Pitfalls
- **ALWAYS use _mediator.Publish()** for notifications (not effect queues)
- **NEVER assume notifications are processed** without explicit handling
- **Symptom**: Commands succeed but UI doesn't update
- *Lesson from*: Block Placement display bug (BPM-005)

#### Command/Handler Stability
- **AVOID property getters that change values** (BlockId => Guid.NewGuid())
- **USE lazy initialization** for stable IDs
- **Symptom**: Non-deterministic behavior, test failures
- *Lesson from*: BlockId stability investigation (2025-08-15)

#### Error Handling Patterns
- **ALWAYS return specific error messages** with context
- **USE functional composition** with BindAsync for error chains
- **AVOID throwing exceptions** in business logic - use Fin<T>

## 🧪 Test Specialist Agent (Merged Role)

### Unified Testing Expert
Combines Test Designer + QA Engineer responsibilities: TDD unit tests, integration validation, and stress testing.

### Unit Test Pattern (TDD RED)
```csharp
[Test]
public void FeatureName_Scenario_ExpectedOutcome()
{
    // Arrange
    var input = CreateValidInput();
    
    // Act  
    var result = ExecuteOperation(input);
    
    // Assert
    Assert.That(result.IsSuccess).IsTrue();
    Assert.That(result.Value).IsEqualTo(expected);
}
```

### Edge Cases to Always Test
```csharp
[TestCase(null)]           // Null inputs
[TestCase("")]             // Empty strings  
[TestCase(-1)]             // Negative values
[TestCase(int.MaxValue)]   // Boundary values
```

### Test Organization
```
tests/BlockLife.Core.Tests/Features/[Feature]/
├── [Feature]CommandHandlerTests.cs
├── [Feature]ServiceTests.cs  
└── [Feature]ValidationTests.cs
```

## 🔍 QA Engineer Agent

### Integration Test Template
```csharp
[TestSuite]
public partial class FeatureIntegrationTest : Node
{
    [TestCase]
    public async Task CompleteFeatureFlow_ValidInput_MeetsAcceptance()
    {
        // Setup test scene
        var scene = await LoadTestScene();
        
        // Execute user workflow  
        await SimulateUserActions();
        
        // Validate end result
        AssertAcceptanceCriteriaMet();
    }
}
```

### Stress Test Template
```csharp
[TestCase]
public async Task StressTest_ConcurrentOperations_NoCorruption()
{
    var barrier = new Barrier(100);
    var tasks = Enumerable.Range(0, 100)
        .Select(_ => Task.Run(async () =>
        {
            barrier.SignalAndWait();
            await ExecuteOperation();
        }))
        .ToArray();
    
    await Task.WhenAll(tasks);
    AssertSystemIntegrity();
}
```

### Bug Report (BR) Protocol
1. **Test Specialist creates BR** - documents symptoms and reproduction
2. **Debugger Expert investigates** - owns BR through lifecycle
3. **Status flow**: Reported → Investigating → Fix Proposed → Fix Applied → Verified
4. **User approval required** before applying fixes
5. **Post-mortem created** for significant bugs after verification

### BR Status Meanings
- **Reported**: Bug symptoms documented
- **Investigating**: Actively debugging
- **Fix Proposed**: Suspected root cause, awaiting approval
- **Fix Applied**: Fix implemented, testing in progress
- **Verified**: Fix confirmed working

### TD (Technical Debt) Protocol
1. **Anyone proposes TD** - documents problem, solution, benefit
2. **Tech Lead reviews** - approves real debt, rejects non-issues
3. **Status flow**: Proposed → Approved/Rejected → In Progress → Done
4. **Dev Engineer implements** approved TD when convenient
5. **Small, focused TD** - one issue at a time, not "refactor everything"

### 🔥 Critical Testing Lessons

#### Integration Test Architecture
- **MUST use SimpleSceneTest pattern** for all GdUnit4 tests
- **NEVER create separate service containers** for tests
- **USE production SceneRoot services** in integration tests
- **Symptom**: Phantom state, block carryover between tests
- *Lesson from*: Parallel service container investigation (2025-08-14)

#### Stress Testing Essentials
- **ALWAYS test concurrent operations** for state corruption
- **USE Thread.Sleep() carefully** - can hide race conditions
- **VERIFY system integrity** after stress tests
- **Pattern**: 100 concurrent operations as baseline
- *Lesson from*: Architecture stress testing (2025-08-13)

#### Notification Pipeline Testing
- **VERIFY end-to-end flows** not just individual components
- **TEST presenter event subscription** explicitly
- **ENSURE UI updates reflect state changes**
- *Lesson from*: Block Placement display bug verification

## 📋 Tech Lead Agent  

### TD Review Responsibility
**Gatekeeper for Technical Debt**:
- Reviews all proposed TD items
- Approves real technical debt
- Rejects non-issues, duplicates, preferences
- Sets priority for approved TD

### TD Review Criteria
```markdown
✅ Approve if:
- Slows development velocity
- Creates maintenance burden
- Causes performance issues
- Violates established patterns

❌ Reject if:
- Personal preference
- Working as intended
- Duplicate of existing TD
- Not actually debt
```

### Implementation Planning Template
```markdown
## 🎯 Feature: [Name]
**Acceptance Criteria**: [List 3-5 measurable criteria]

## 🏗️ Implementation Plan
### Phase 1: Core Logic
- [ ] Create command/handler structure
- [ ] Implement domain services
- [ ] Add validation rules

### Phase 2: Integration  
- [ ] Create presenter/view contracts
- [ ] Wire up notification pipeline
- [ ] Add to DI container

### Phase 3: Testing
- [ ] Unit tests for handlers/services
- [ ] Integration tests for workflows
- [ ] Edge case coverage
```

### Risk Assessment
- **Technical Risks**: Dependencies, performance, complexity
- **Integration Risks**: Breaking existing features
- **Timeline Risks**: Unknown unknowns, scope creep

### 🔥 Critical Planning Lessons

#### Integration Test Planning
- **PLAN integration tests early** - don't add them as afterthought
- **ENSURE SimpleSceneTest pattern** for all Godot integration
- **VERIFY notification pipeline design** before implementation starts
- *Lesson from*: Parallel service container architecture issues

#### State Management Architecture
- **DESIGN single source of truth** from the beginning
- **AVOID splitting state across services** even if it seems logical
- **CONSIDER consolidation** when multiple services manage same domain
- *Lesson from*: GridStateService + BlockRepository duplication

## 📦 Product Owner Agent

### Vertical Slice Template
```markdown
## 🎯 VS_[ID]: [Feature Name]

**Player Outcome**: [What the player experiences when this slice ships]

**Slice Scope**:
- **UI Layer**: [Interface changes]
- **Command Layer**: [Player actions handled]  
- **Logic Layer**: [Business rules implemented]
- **Data Layer**: [State changes]

### Acceptance Criteria
- [ ] [End-to-end behavior through all layers]
- [ ] [Layer integration validation]
- [ ] [Performance requirement]
```

### Backlog Prioritization
- **🔥 Critical**: Blockers, production bugs, dependencies
- **📈 Important**: Current milestone features, velocity-affecting tech debt  
- **💡 Ideas**: Nice-to-have features, experimental concepts

## 🔧 DevOps Engineer Agent

### Essential Build Commands
```bash
# Build and test
dotnet build BlockLife.sln
dotnet test

# Godot operations
dotnet run --project godot_project

# Git workflow
git checkout -b feat/feature-name
git add . && git commit -m "feat: description"
```

### Automation Patterns
- **Python scripts**: For repetitive tasks, file generation
- **PowerShell**: For Windows-specific operations  
- **Bash/Git hooks**: For workflow automation

## 🔀 Git Expert Agent

### Branch Workflow
```bash
# Feature development
git checkout -b feat/feature-name
git add . && git commit -m "feat: implement feature"
git push -u origin feat/feature-name

# Create PR
gh pr create --title "feat: feature name" --body "Description"
```

### Conflict Resolution
1. **Identify conflict type**: Content vs. file structure
2. **Choose resolution strategy**: Merge, rebase, or cherry-pick
3. **Test after resolution**: Build and run tests
4. **Document complex resolutions**: For future reference

## 🤖 DevOps Engineer Agent

### Automation & Scripting
Use this agent for **workflow automation** and **friction reduction**:

#### Python Script Automation (4,850+ lines available)
```bash
# Backlog management automation
python scripts/auto_archive_completed.py  # Saves 5-10 min per completed item

# Git workflow protection
python scripts/setup_git_hooks.py  # One-time setup prevents main commits

# Documentation automation  
python scripts/sync_documentation_status.py  # Saves 30-60 min monthly

# Test metrics automation
python scripts/collect_test_metrics.py --update-docs
```

#### When to Use DevOps Agent
- **Process takes >5 minutes manually** → Automate it
- **Repetitive workflow steps** → Create scripts
- **CI/CD pipeline improvements** → Build automation
- **Manual process errors** → Add verification scripts

#### Automation Patterns to Follow
- **Verification-first**: All file operations must be verified
- **Dry-run capable**: Always include `--dry-run` option
- **Comprehensive logging**: UTF-8 support, structured output
- **Error recovery**: Automatic rollback on failure

**Note**: Automation scripts capabilities documented inline above.

## 🔄 VSA Refactoring Agent

### Code Duplication Detection
- **Cross-slice duplication**: Extract to shared utilities
- **Within-slice duplication**: Refactor internal structure
- **Pattern duplication**: Create template or base class

### Refactoring Safety
1. **Tests pass before**: Ensure current behavior is captured
2. **Small incremental changes**: One refactoring at a time
3. **Tests pass after**: Verify behavior preserved
4. **Review impact**: Check all usage sites

## 🐛 Debugger Expert Agent

### When to Use
- **Complex bug investigation** (unclear root cause)
- **Race conditions and timing issues**
- **Memory leaks and performance problems**
- **Cross-component debugging**
- **BR items assigned for investigation**

### BR Investigation Workflow
1. **Receive BR item** (Status: Reported)
2. **Update to Investigating** and start debugging
3. **Document findings** in Investigation Log
4. **Form hypothesis** with confidence level
5. **Update to Fix Proposed** and request user approval
6. **Implement fix** after approval
7. **Update to Fix Applied** while testing
8. **Update to Verified** when confirmed

### Quick Investigation Steps
1. **Reproduce reliably** - Create minimal case
2. **Isolate component** - Binary search through changes
3. **Form hypothesis** - What could cause this?
4. **Test focused fix** - Minimal change to address root cause

### 🔥 Critical Debugging Lessons

#### Notification Pipeline Debugging
- **CHECK presenter event subscription** first when UI doesn't update
- **VERIFY _mediator.Publish() calls** in command handlers
- **TRACE notification bridge static events** for subscription issues
- **Pattern**: Commands succeed but views don't update = subscription problem
- *Lesson from*: Block Placement display bug (BPM-005)

#### State Corruption Diagnosis
- **LOOK for dual state sources** when tests behave inconsistently
- **CHECK service registration** for singleton vs. transient issues
- **VERIFY effect processing** vs. direct notification publishing
- **Symptom**: Different state in different parts of system
- *Lesson from*: GridStateService consolidation investigation

#### Race Condition Detection
- **USE concurrent stress tests** to expose race conditions
- **EXAMINE thread-unsafe operations** in state management
- **VERIFY proper async/await usage** throughout call chains
- *Lesson from*: Architecture stress testing findings

**For detailed bug patterns and debugging procedures**: → **See persona knowledge bases and post-mortems**



## 📚 Essential Documentation Links

- **Move Block Reference**: `src/Features/Block/Move/` (Copy this!)
- **Architecture Guide**: `Docs/Core/Architecture/Architecture_Guide.md`
- **Development Workflow**: `Docs/01-Active/Workflow.md`
- **Git Workflow**: `Docs/03-Reference/GitWorkflow.md`
- **Backlog**: `Docs/01-Active/Backlog.md` (Single source of truth)

---
*This reference replaces 8 individual agent reference files, providing all essential patterns and templates in one location for faster navigation and reduced cognitive overhead.*