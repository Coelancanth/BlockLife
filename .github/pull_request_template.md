## 📋 PR Description

### What does this PR do?
<!-- Provide a clear and concise description of the changes -->

### Which issue does this PR address?
<!-- Link to the issue: Closes #XXX -->

## Changes

### 🎯 Features Added
<!-- List new features or functionality -->
- 

### 🐛 Bugs Fixed
<!-- List any bugs fixed -->
- 

### 🔧 Refactoring
<!-- Describe any refactoring done -->
- 

## Testing

### 🧪 TDD Workflow Followed
- [ ] Architecture fitness tests pass
- [ ] Tests written BEFORE implementation (RED phase)
- [ ] Implementation done to pass tests (GREEN phase)
- [ ] Code refactored while keeping tests green (REFACTOR phase)

### 📊 Test Coverage
- **Tests added:** <!-- Number of new tests -->
- **Test categories:** 
  - [ ] Unit tests
  - [ ] Property-based tests
  - [ ] Integration tests
  - [ ] Architecture tests

### 🔬 Testing Instructions
<!-- How should reviewers test these changes? -->
1. Run `dotnet test --filter "FullyQualifiedName~Architecture"`
2. Run `dotnet test tests/BlockLife.Core.Tests.csproj`
3. 

## Checklist

### 📚 Documentation
- [ ] CLAUDE.md updated (if adding new patterns)
- [ ] Implementation plan updated (if applicable)
- [ ] Code comments added where necessary
- [ ] No TODO comments left in code

### 🏗️ Architecture
- [ ] Follows Clean Architecture principles
- [ ] No Godot dependencies in Core project
- [ ] Commands are immutable records
- [ ] Handlers return `Fin<T>` for error handling
- [ ] Follows vertical slice architecture

### 🎨 Code Quality
- [ ] Code follows existing patterns (reference: `src/Features/Block/Move/`)
- [ ] No compiler warnings
- [ ] Meaningful variable/method names
- [ ] SOLID principles followed

### ✅ Final Checks
- [ ] All tests pass locally
- [ ] Branch is up to date with target branch
- [ ] PR has meaningful title
- [ ] Ready for review

## 📸 Screenshots/Logs
<!-- If applicable, add screenshots or test output -->

## 📝 Additional Notes
<!-- Any additional context for reviewers -->

---
**Reference Implementation:** `src/Features/Block/Move/` (Gold Standard)  
**Workflow Guide:** [Development_Workflows.md](../Docs/Quick-Start/Development_Workflows.md)  
**Agent Reference:** [Agent_Quick_Reference.md](../Docs/Quick-Start/Agent_Quick_Reference.md)