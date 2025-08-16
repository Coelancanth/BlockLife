# 🤖 Agent Quick Reference

## 🎯 Reference Implementation (GOLD STANDARD)
**Copy this for ALL new work**: `src/Features/Block/Move/`
- File structure, naming, patterns
- C# and Godot integration
- Test organization and assertions

## 🏗️ Architect Agent

### Core Architecture Rules (Non-Negotiable)
1. **No Godot in `src/`** - Domain stays pure
2. **Commands for state changes** - No direct mutations  
3. **Single source of truth** - One service per responsibility
4. **MVP pattern** - Presenters handle UI coordination

### Strategic Decision Areas
- **Rule Engines**: >5 interconnected conditions → consider rule engine
- **Technology Choices**: Must integrate with existing DI/CQRS
- **Performance**: System-level changes when affecting multiple features

### Quick Decision Template
```
Problem: [specific architectural issue]
Current: [how it's handled now] 
Proposed: [new pattern]
Impact: [what changes in codebase]
```

### 🔥 Critical Lessons Learned

#### State Management Anti-Patterns
- **NEVER create dual state sources** - Use single service like GridStateService
- **Symptom**: Tests pass individually but fail in integration
- **Fix**: Consolidate to one source of truth
- *Lesson from*: Block Placement architecture stress test (2025-08-13)

#### Service Container Isolation
- **NEVER create parallel service containers** in tests
- **Symptom**: Commands succeed but notifications come from different instances
- **Fix**: Use production SceneRoot service provider in integration tests
- *Lesson from*: Integration test architecture investigation (2025-08-14)

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

## 🧪 Test Designer Agent

### Test Creation Pattern
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

### Bug Regression Protocol
1. **Reproduce exactly** - follow reported steps
2. **Create failing test** - would have caught the bug  
3. **Verify test fails** - confirms bug reproduction
4. **Apply fix and verify test passes**
5. **Add to permanent regression suite**

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

### User Story Template
```markdown
## 🎯 VS_[ID]: [Feature Name]

**As a** [user type]
**I want** [capability]  
**So that** [business value]

### Acceptance Criteria
- [ ] [Specific, measurable outcome]
- [ ] [Edge case handling]
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

**See**: [Automation_Scripts_Guide.md](../Workflows/Development/Automation_Scripts_Guide.md) for complete capabilities.

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

**For detailed bug patterns and debugging procedures**: → **[Technical_Patterns.md](Technical_Patterns.md)**

## 🚀 Quick Agent Selection Guide

**Before doing work, ask: Is there a specialist for this?**

- User requests/bugs → **product-owner**
- Technical planning → **tech-lead**  
- Tests needed → **test-designer**
- Code implementation → **dev-engineer**
- Quality assurance → **qa-engineer**
- Architecture decisions → **architect**
- Code duplication → **vsa-refactoring**
- Bug diagnosis → **debugger-expert**
- Git operations → **git-expert**
- Automation/scripts → **devops-engineer**

## 🔄 Concurrent Agent Orchestration

### When to Use Parallel Agents
- **Complex investigations** - Multiple experts analyzing same problem from different angles
- **Feature development** - Requirements, architecture, and testing designed simultaneously  
- **Performance issues** - Debugging, architecture review, and stress testing in parallel
- **Bug analysis** - Root cause investigation, test design, and architecture review concurrently

### Parallel Investigation Pattern
```
Main Agent coordinates 3 simultaneous investigations:

Agent 1 (debugger-expert):
└─ "Systematic root cause analysis of [specific issue]"

Agent 2 (architect):  
└─ "Review architecture patterns for [domain concern]"

Agent 3 (qa-engineer):
└─ "Design stress tests to validate [system behavior]"

Main Agent synthesizes findings → Unified action plan
```

### Multi-Phase Concurrent Development
```
Phase 1: Concurrent Planning
├─ product-owner → User stories and acceptance criteria
└─ architect → Technical design and integration approach

Phase 2: Concurrent Implementation Prep  
├─ test-designer → Test strategy (uses Phase 1 results)
└─ tech-lead → Implementation plan (uses Phase 1 results)

Phase 3: Execution
└─ dev-engineer → Implementation (uses all previous results)
```

### Main Agent Orchestration Responsibilities
1. **Task Decomposition** - Break complex requests into independent parallel concerns
2. **Context Packaging** - Ensure each agent has complete information for their domain
3. **Conflict Resolution** - Synthesize potentially contradictory expert recommendations
4. **Quality Integration** - Weave multiple expert outputs into coherent action plan
5. **Critical Evaluation** - Apply over-engineering safeguards to expert recommendations

### Synthesis Decision Framework
```
When expert recommendations conflict:
1. Assess complexity vs. benefit
2. Consider project constraints (solo dev, existing patterns)
3. Apply simplicity bias ("simpler approach wins")
4. Update all affected plans with resolved approach
5. Document decision rationale
```

## 📚 Essential Documentation Links

- **Move Block Reference**: `src/Features/Block/Move/` (Copy this!)
- **Architecture Guide**: `Docs/Core/Architecture/Architecture_Guide.md`
- **Development Workflow**: `Docs/Workflows/Development/Essential_Development_Workflow.md`
- **Git Workflow**: `Docs/Workflows/Git/Git_Workflow_Guide.md`
- **Backlog**: `Docs/Backlog/Backlog.md` (Single source of truth)

---
*This reference replaces 8 individual agent reference files, providing all essential patterns and templates in one location for faster navigation and reduced cognitive overhead.*