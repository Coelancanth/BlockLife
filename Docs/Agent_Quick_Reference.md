# 🤖 Agent Quick Reference

**★ Insight ─────────────────────────────────────**
**Consolidated Knowledge**: This single file replaces 8 separate agent reference files, reducing navigation overhead while maintaining all essential patterns and templates for rapid agent deployment.
**─────────────────────────────────────────────────**

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

## 📚 Essential Documentation Links

- **Move Block Reference**: `src/Features/Block/Move/` (Copy this!)
- **Architecture Guide**: `Docs/Shared/Core/Architecture/Architecture_Guide.md`
- **Development Workflow**: `Docs/Shared/Workflows/Development/Essential_Development_Workflow.md`
- **Git Workflow**: `Docs/Shared/Workflows/Git-And-CI/Git_Workflow_Guide.md`
- **Backlog**: `Docs/Backlog/Backlog.md` (Single source of truth)

---
*This reference replaces 8 individual agent reference files, providing all essential patterns and templates in one location for faster navigation and reduced cognitive overhead.*