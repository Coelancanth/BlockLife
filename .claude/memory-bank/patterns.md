# Established Patterns - BlockLife

**Last Updated**: 2025-08-21

## 🔧 Recent Patterns (TD_039 Implementation)

### Git Hook Auto-Installation
**Pattern**: Configure Husky in .csproj for zero-config setup
**Example**: `BlockLife.Core.csproj:22-25`
**Rationale**: Hooks auto-install on `dotnet tool restore` across all clones
**Added**: 2025-08-21

### Branch Naming Convention
**Pattern**: Use underscores for work items (VS_003 not vs-003)
**Example**: `feat/VS_003-save-system`
**Rationale**: Matches Backlog.md format exactly
**Added**: 2025-08-21

### CI Branch Freshness Check
**Pattern**: Fail PRs that are >20 commits behind main
**Example**: `.github/workflows/ci.yml:49-99`
**Rationale**: Prevents surprise conflicts during merge
**Added**: 2025-08-21

### Memory Bank Conflict Resolution 
**Pattern**: Manual 3-way merge resolution for Memory Bank files
**Example**: PR #57 rebase with activeContext.md, decisions.md, patterns.md conflicts
**Rationale**: Memory Bank files updated on every branch create guaranteed conflicts
**Process**: Read all versions → Write unified version → Continue rebase
**Added**: 2025-08-21

## 🎯 Core Patterns

### Move Block Pattern (Gold Standard)
**Location**: `src/Features/Block/Move/`
**Use Case**: Reference implementation for all new features
**Key Elements**:
- Command/Handler separation
- Fin<T> error handling
- MVP presenter pattern
- Comprehensive test coverage
**Added**: 2025-08-15

### Clean Architecture Layers
```
UI (Godot) → Presenters → Commands → Handlers → Services → Data
```
**Never Skip Layers**: Each layer has a specific responsibility

### Error Handling Pattern
```csharp
public Fin<MoveResult> Execute(MoveCommand command) =>
    ValidateCommand(command)
        .Bind(ValidateBusinessRules)
        .Map(ExecuteMove)
        .MapFail(LogError);
```

### Async Error Handling
**Pattern**: Use Fin<T> for all async operations
**Example**: `MoveBlockService.cs` async methods
**Rationale**: Consistent error propagation with LanguageExt
**Added**: 2025-08-10

### Test Structure Pattern
```
Arrange → Act → Assert
Given → When → Then
```

## 📝 Documentation Patterns

### Persona Workflow
1. Read backlog
2. Filter by owner
3. Execute work
4. Suggest updates (bullet points)
5. User decides on execution

### Subagent Verification
**Trust but Verify**: 10-second checks after subagent work
- Check file modifications
- Verify content presence
- Validate status changes

### Backlog Update Pattern
```markdown
**Suggested backlog updates:**
- Create TD_XXX: Description
- Update VS_XXX status to 'Complete'
- Move BR_XXX to Critical section
```

## 🔧 Development Patterns

### Git Workflow
```bash
git checkout main
git pull origin main
git checkout -b feat/description
# work
git add -A
git commit -m "type: description"
git push -u origin feat/description
```

### Build & Test Pattern
```powershell
./scripts/core/build.ps1 test  # Always before commit
```

### Naming Patterns
- **Commands**: VerbNounCommand (MoveBlockCommand)
- **Handlers**: VerbNounHandler (MoveBlockHandler)
- **Services**: NounService (BlockService)
- **Tests**: ClassNameTests (MoveBlockHandlerTests)

## 🚫 Anti-Patterns to Avoid

### ❌ Direct Godot Access from Domain
```csharp
// WRONG
public class BlockService {
    private Node2D _sprite; // Never reference Godot in domain
}
```

### ❌ Skipping Error Handling
```csharp
// WRONG
var result = service.Execute();
return result.Value; // Assumes success

// RIGHT
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

## 🎨 Code Style Patterns

### No Comments Unless Asked
```csharp
// WRONG (unless requested)
// This method moves the block
public void MoveBlock() { }

// RIGHT
public void MoveBlock() { }
```

### Glossary Enforcement
- Always use terms from Glossary.md
- "Match" not "Merge"
- "Tier" not "Level"
- "Turn" not "Round"

---
*These patterns are proven and should be followed consistently*