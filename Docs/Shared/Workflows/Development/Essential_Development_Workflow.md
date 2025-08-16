# Essential Development Workflow for BlockLife

## Overview

Simple, practical workflow for solo developer + AI agent collaboration. Focuses on essential practices without enterprise overhead.

**Core Principles:**
- Mandatory Git branching (never work on main)
- Test-Driven Development for confidence
- Simple quality gates
- Bug prevention through tests

---

## 1. 🚨 MANDATORY: Git Workflow

**BEFORE ANY WORK - CREATE BRANCH:**

```bash
# Always start clean
git checkout main
git pull origin main

# Create feature branch (REQUIRED)
git checkout -b feat/your-feature-name
```

**Branch Types:**
- `feat/` - New features
- `fix/` - Bug fixes  
- `docs/` - Documentation
- `refactor/` - Code cleanup

**FORBIDDEN:** Working directly on main branch.

---

## 2. 🔄 Basic TDD Cycle

### Red-Green-Refactor Pattern

```csharp
// 1. RED: Write failing test
[Fact]
public async Task PlaceBlock_ValidPosition_Success()
{
    // Arrange
    var handler = new PlaceBlockCommandHandler();
    var command = new PlaceBlockCommand(new Vector2Int(1, 1));
    
    // Act
    var result = await handler.Handle(command);
    
    // Assert
    result.IsSucc.Should().BeTrue();
}

// 2. GREEN: Write minimal code to pass
public async Task<Fin<Unit>> Handle(PlaceBlockCommand request)
{
    return Unit.Default; // Simplest implementation
}

// 3. REFACTOR: Improve without breaking tests
```

### Essential Test Types
1. **Unit Tests** - Core logic validation
2. **Integration Tests** - Full feature flows
3. **Architecture Tests** - Boundary enforcement (optional)

---

## 3. 🏗️ Simple Architecture Rules

### Core Boundaries
- **Domain** - Pure C# business logic
- **Infrastructure** - Godot views and external services
- **Application** - Command handlers and coordination

### Basic Patterns
```csharp
// Commands are immutable records
public record PlaceBlockCommand(Vector2Int Position) : IRequest<Fin<Unit>>;

// Handlers return Fin<T> for error handling
public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request, CancellationToken ct)
    {
        // Implementation
        return Unit.Default;
    }
}

// Views implement interfaces for testing
public interface IGridView
{
    Task ShowBlockAsync(Vector2Int position);
    IObservable<Vector2Int> CellClicked { get; }
}
```

---

## 4. 🐛 Bug-to-Test Protocol

**MANDATORY**: Every bug becomes a regression test.

### Bug Response Process
1. **Document** - What broke and why
2. **Test** - Write failing test that reproduces bug
3. **Fix** - Make test pass with minimal change
4. **Verify** - Run all tests to ensure no new breaks

### Example Bug Response
```csharp
/// <summary>
/// REGRESSION TEST: Prevents BlockId regeneration bug
/// Bug Date: 2025-08-14
/// Issue: BlockId was generating new GUID on each access
/// Fix: Use cached GUID in command
/// </summary>
[Fact]
public void PlaceBlockCommand_BlockId_StaysStable()
{
    var command = new PlaceBlockCommand(Vector2Int.Zero);
    var firstId = command.BlockId;
    var secondId = command.BlockId;
    
    firstId.Should().Be(secondId);
}
```

---

## 5. 📋 Simple Quality Gates

### Before Committing
- [ ] Tests pass locally
- [ ] No obvious breaking changes
- [ ] Code compiles without warnings

### Before PR
- [ ] All tests pass
- [ ] Feature works as expected
- [ ] Simple manual testing completed

### Build Commands
```bash
# Quick test run
dotnet test

# Full build
dotnet build

# Run specific tests
dotnet test --filter "NameContains=PlaceBlock"
```

---

## 6. 🔧 Agent Integration

### Use Agents For:
- **Complex debugging** → debugger-expert
- **Architecture decisions** → architect  
- **Test design** → test-designer
- **Implementation** → dev-engineer
- **Quality assurance** → qa-engineer

### Simple Agent Workflow
1. Start work on feature branch
2. Use appropriate agents for complex tasks
3. Verify agent outputs actually work
4. Update simple backlog status (🔥📈💡)
5. Create PR when feature complete

---

## 7. 📝 Minimal Documentation

### Required Documentation
- Update backlog status after work
- Add comments for non-obvious code
- Document any architectural decisions
- Include bug context in regression tests

### Skip Unless Needed
- Elaborate API documentation
- Complex architecture diagrams
- Detailed process documentation
- Enterprise-style templates

---

## 8. 🚀 Pull Request Process

### Create PR
```bash
# Push feature branch
git push -u origin feat/your-feature-name

# Create PR with simple template
gh pr create --title "feat: brief description" --body "
## What Changed
- Brief bullet points of changes

## Testing
- [ ] Tests pass
- [ ] Feature works manually

## Notes
- Any important context
"
```

### PR Requirements
- Descriptive title
- All tests passing
- Basic manual verification
- No direct pushes to main

---

## 9. 🎯 Success Criteria

### Code Quality
- Tests cover main functionality
- No obvious bugs in manual testing
- Code is readable and maintainable

### Development Speed
- Features complete in reasonable time
- Not blocked by process overhead
- Agent orchestration reduces cognitive load

### Reliability
- Bugs caught before reaching main
- Regression tests prevent recurring issues
- Architecture stays reasonably clean

---

## 10. 🚨 Emergency Procedures

### Critical Bug
1. Create `hotfix/issue-description` branch
2. Write failing test reproducing bug
3. Fix to make test pass
4. Fast-track PR with clear description

### Performance Issue
1. Profile to identify bottleneck
2. Apply tactical fix
3. Add performance test if needed
4. Plan strategic improvement

### Architecture Violation
1. Fix the immediate issue
2. Add test to prevent recurrence
3. Update guidelines if needed

---

## Conclusion

This workflow prioritizes **effectiveness over ceremony**. The goal is shipping quality features quickly while maintaining basic safeguards against common issues.

**Key Reminders:**
- Always work on feature branches
- Write tests for confidence
- Use agents for complex tasks
- Keep documentation simple and useful
- Fix bugs with regression tests

**When in doubt**: Choose the simpler approach that gets you shipping quality code faster.

---

*This replaces the comprehensive enterprise workflow with practical essentials for solo development.*