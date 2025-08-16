# 🚀 Development Workflows

**★ Insight ─────────────────────────────────────**
**Workflow Consolidation**: This single file replaces multiple workflow documents, providing essential checklists and patterns for rapid development without the overhead of consulting multiple guides.
**─────────────────────────────────────────────────**

## 🚨 MANDATORY: Git Workflow (Never Skip)

### Before Any Work
```bash
# Always start clean
git checkout main && git pull origin main

# Create feature branch (REQUIRED)
git checkout -b feat/your-feature-name
```

### Branch Types
- `feat/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation  
- `refactor/` - Code cleanup
- `test/` - Test additions
- `hotfix/` - Critical production fixes

### Commit & PR Workflow
```bash
# Commit changes
git add . && git commit -m "feat: descriptive message"
git push -u origin feat/your-feature-name

# Create PR
gh pr create --title "feat: feature name" --body "Description"
```

**🚫 FORBIDDEN**: Working directly on main branch.

## ⚡ TDD Development Cycle

### 1. Red Phase (test-designer agent)
```csharp
[Fact]
public async Task FeatureName_Scenario_ExpectedOutcome()
{
    // Arrange - Set up test data
    var handler = new FeatureCommandHandler();
    var command = new ValidCommand();
    
    // Act - Execute the operation
    var result = await handler.Handle(command);
    
    // Assert - Verify the outcome
    Assert.That(result.IsSuccess).IsTrue();
}
```

### 2. Green Phase (dev-engineer agent)
- Write minimal code to pass the test
- Copy patterns from `src/Features/Block/Move/`
- Follow Clean Architecture boundaries

### 3. Refactor Phase
- Improve code quality without breaking tests
- Extract shared logic if needed
- Keep it simple

## 📋 Feature Development Checklist

### Phase 1: Planning
- [ ] **Create feature branch** (mandatory)
- [ ] **Check reference implementation**: `src/Features/Block/Move/`
- [ ] **Define acceptance criteria** (3-5 measurable outcomes)
- [ ] **Create VS_xxx work item** in backlog

### Phase 2: Implementation
- [ ] **Write failing tests** (TDD Red phase)
- [ ] **Implement minimal solution** (TDD Green phase)
- [ ] **Add to DI container** (GameStrapper.cs)
- [ ] **Wire up notifications** (if needed)

### Phase 3: Quality Gates
- [ ] **All tests pass** (`dotnet test`)
- [ ] **Build succeeds** (`dotnet build`)
- [ ] **Integration test** (if UI changes)
- [ ] **Manual verification** (run the game)
- [ ] **Bug prevention check** (see checklist below)

### Phase 4: Completion
- [ ] **Create Pull Request**
- [ ] **Update backlog** (move to ✅ Done)
- [ ] **Clean up local branch** (after merge)

## 🐛 Bug Fix Workflow

### Immediate Response
1. **Reproduce the bug** - Follow exact steps
2. **Create BF_xxx work item** in backlog
3. **Write failing test** - Should reproduce the bug
4. **Fix the issue** - Minimal change needed
5. **Verify test passes** - Bug is fixed
6. **Add to regression suite** - Prevent recurrence

### Bug Prevention
```csharp
// Always test edge cases
[TestCase(null)]           // Null inputs
[TestCase("")]             // Empty strings
[TestCase(-1)]             // Invalid values
[TestCase(int.MaxValue)]   // Boundary values
```

## 🧪 Testing Strategy

### Unit Tests (Core Logic)
```
tests/BlockLife.Core.Tests/Features/[Feature]/
├── [Feature]CommandHandlerTests.cs    # Business logic
├── [Feature]ServiceTests.cs           # Domain services
└── [Feature]ValidationTests.cs        # Input validation
```

### Integration Tests (UI + Core)
```
tests/BlockLife.Integration.Tests/Features/[Feature]/
├── [Feature]IntegrationTests.cs       # End-to-end flows
└── [Feature]StressTests.cs            # Concurrent operations
```

### Testing Guidelines
- **Unit tests**: Fast, isolated, deterministic
- **Integration tests**: Real Godot scenes, full workflows
- **Stress tests**: 100+ concurrent operations
- **Regression tests**: Prevent known bugs

## 🔍 Quality Standards

### Code Quality
- **Follow GOLD STANDARD**: Copy patterns from Move Block feature
- **Clean Architecture**: No Godot in `src/` folder
- **Error handling**: Use `Fin<T>` for business logic
- **Async patterns**: Proper async/await usage

### Performance
- **Frame time**: Operations <16ms for 60fps
- **Memory**: Dispose resources properly
- **Concurrency**: Test with multiple threads
- **Godot integration**: Don't block main thread

### Documentation
- **Self-documenting code**: Clear names and structure
- **Minimal comments**: Only when business logic is complex
- **Update backlog**: Track progress and status
- **Living documentation**: Keep patterns up to date

## 🎯 Agent Orchestration (Enhanced)

### When to Use Agents
- **Complex tasks** (>5 minutes) → Use specialist agent
- **Simple tasks** (<5 minutes) → Do directly
- **Architecture decisions** → Always use architect
- **Bug diagnosis** → Always use debugger-expert

### Quick Agent Selection
- User requests → product-owner
- Technical planning → tech-lead
- Write tests → test-designer
- Implement code → dev-engineer  
- Quality checks → qa-engineer
- Architecture → architect
- Refactoring → vsa-refactoring
- Git issues → git-expert
- Scripts/automation → devops-engineer
- Complex bugs → debugger-expert

### Concurrent Agent Orchestration

#### When to Use Parallel Agents
- **Complex investigations** - Multiple experts analyzing same problem
- **Major feature development** - Requirements, architecture, testing simultaneously
- **Performance/bug analysis** - Root cause + architecture review + stress testing
- **Time-critical projects** - Compress timeline with parallel expert work

#### Parallel Orchestration Workflow
```
1. DECOMPOSE complex request into independent concerns
2. PACKAGE context for each specialist (same base info + domain-specific)
3. EXECUTE agents concurrently with clear deliverable expectations
4. SYNTHESIZE results, resolving conflicts with simplicity bias
5. UPDATE backlog with integrated action plan
```

#### Example: Performance Issue Investigation
```
Main Agent coordinates 3 parallel investigations:

debugger-expert: "Root cause analysis of slow block placement"
├─ Focus: Notification pipeline timing and bottlenecks
└─ Deliverable: Specific performance bottleneck identification

architect: "Review GridStateService architecture for scalability"  
├─ Focus: Concurrent access patterns and state synchronization
└─ Deliverable: Architecture optimization recommendations

qa-engineer: "Design stress tests for concurrent operations"
├─ Focus: 100+ simultaneous block operations baseline
└─ Deliverable: Performance test suite revealing characteristics

→ Main Agent synthesizes findings into unified optimization plan
```

### After Agent Work
1. **Verify output** - Did they complete the task?
2. **Check for conflicts** - Do expert recommendations contradict?
3. **Apply critical thinking** - Challenge over-engineering or complexity
4. **Synthesize results** - Create unified, actionable plan
5. **Update backlog** - Record progress/results
6. **Continue workflow** - Move to next step

## 📊 Backlog Management (Simple 3-Tier)

### Priority Tiers
- **🔥 Critical**: Blockers, production bugs, dependencies
- **📈 Important**: Current milestone, velocity-affecting issues
- **💡 Ideas**: Nice-to-have, future considerations

### Work Item Types
- **VS_xxx**: Vertical Slice (complete feature)
- **BF_xxx**: Bug Fix
- **TD_xxx**: Technical Debt
- **HF_xxx**: Hotfix

### Daily Workflow
1. **Check 🔥 Critical** - Do these first
2. **Work on 📈 Important** - Current sprint focus
3. **Consider 💡 Ideas** - When other tiers are empty
4. **Update progress** - Move items as they progress

## 🛡️ Bug Prevention Checklist

### Before Committing (Critical Checks)
- [ ] **Property stability**: Do properties return stable values on multiple accesses?
- [ ] **Validation enforcement**: Are business rules validated before state changes?
- [ ] **DI registration**: Do all registered services have resolvable dependencies?
- [ ] **Notification uniqueness**: Is each notification published exactly once?
- [ ] **Error consistency**: Are error messages and codes consistent across handlers?

### Common Patterns to Avoid
```csharp
// ❌ Unstable property values
public Guid Id => Guid.NewGuid(); // New value each time!

// ❌ Missing validation
await _service.ChangeState(input); // No validation!

// ❌ Presenter as MediatR handler
public class MyPresenter : INotificationHandler<Event> // DI conflict!

// ❌ Multiple notification sources
// Both handler AND manager publishing same notification

// ❌ Direct state access
_someStaticValue = newValue; // No validation or tracking!
```

### Bug-to-Test Protocol
1. **Write failing test** that reproduces the bug
2. **Verify test fails** (confirms bug reproduction)  
3. **Fix the code** with minimal change
4. **Verify test passes** (confirms fix works)
5. **Add to regression suite** (prevents reoccurrence)

## 📚 Performance Gotchas & Lessons Learned

### Critical Performance Issues (From Post-Mortems)

#### Async/Await JIT Compilation (BF_001)
**Issue**: First async operation can take 200-300ms due to Mono/.NET JIT compilation
**Symptom**: First block move took 271ms, subsequent moves <1ms
**Root Cause**: `Option<T>.Match` with async lambdas triggers state machine compilation
**Solution**: Pre-warm async patterns during startup:
```csharp
// In _Ready() method
private async Task PreWarmAsyncPatterns()
{
    var testOption = Some(new Vector2Int(0, 0));
    await testOption.Match(
        Some: async pos => await Task.CompletedTask,
        None: async () => await Task.CompletedTask
    );
}
PreWarmAsyncPatterns().Wait(500); // Move 271ms cost to startup
```

### Debugging Complex Performance Issues

When facing mysterious performance lags:
1. **Add granular instrumentation first** - Don't guess, measure
2. **Check for first-time initialization costs** - JIT, lazy loading, etc.
3. **Profile at micro-operation level** - Even simple comparisons can hide costs
4. **Trust user reports over assumptions** - If they say it's slow, it is
5. **Use debugger-expert with complete context** - Include all evidence and history

## 🤖 Automation Integration

### Essential Automation Scripts
BlockLife includes comprehensive automation (4,850+ lines of Python) to eliminate manual friction:

```bash
# Automatic backlog archival (saves 5-10 min per completed item)
python scripts/auto_archive_completed.py

# Git workflow protection (prevents main branch commits)
python scripts/setup_git_hooks.py  # One-time setup

# Test metrics automation (keeps docs current)
python scripts/collect_test_metrics.py --update-docs

# Documentation sync (saves 30-60 min monthly)
python scripts/sync_documentation_status.py
```

### Automation-Enhanced Development Flow
```bash
# 1. Protected branch creation
git checkout -b feat/new-feature  # Pre-commit hooks prevent main commits

# 2. TDD with monitoring
python scripts/test_monitor.py &  # Background test monitoring
# Write tests, implement features...

# 3. Automatic completion handling
# When backlog items reach 100%, auto-archive triggers

# 4. Quality gates
python scripts/verify_backlog_archive.py  # Verify operations
```

**See**: [Automation_Scripts_Guide.md](../Workflows/Development/Automation_Scripts_Guide.md) for complete automation capabilities.

## 🔧 Essential Commands

### Build & Test
```bash
# Build the solution
dotnet build BlockLife.sln

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/BlockLife.Core.Tests
```

### Git Operations
```bash
# Check status
git status

# Commit workflow
git add . && git commit -m "feat: description"

# Push and create PR
git push -u origin branch-name
gh pr create --title "Title" --body "Description"
```

### Godot Integration
```bash
# Run the game (from solution root)
dotnet run --project godot_project
```

## 🚦 Definition of Done

### Feature Complete When
- [ ] All acceptance criteria met
- [ ] Unit tests pass (100% for new code)
- [ ] Integration tests pass
- [ ] Build succeeds without warnings
- [ ] Manual testing confirms functionality
- [ ] Code follows established patterns
- [ ] PR approved and merged
- [ ] Backlog updated

### Bug Fixed When
- [ ] Reproduction test created and failing
- [ ] Fix implemented with minimal change
- [ ] Reproduction test now passes
- [ ] Regression test added to permanent suite
- [ ] Root cause documented
- [ ] Related edge cases tested

---

*This workflow consolidates multiple process documents into actionable checklists, reducing decision fatigue while maintaining quality standards.*