# Naming Standards

## Overview

This document establishes consistent naming conventions for the BlockLife project, ensuring clarity, maintainability, and adherence to Clean Architecture principles. All naming follows the principle: **descriptive, consistent, and simple**.

## Core Principles

### 1. **Descriptive and Intentional**
- Names should clearly express purpose and intent
- Avoid abbreviations unless universally understood
- Use full words rather than acronyms (except established ones like `UI`, `ID`)

### 2. **Consistency Across Layers**
- Similar concepts use similar naming patterns across different layers
- Interface/Implementation pairs follow consistent patterns

### 3. **Domain-Driven Language**
- Use terminology from the business domain
- Maintain ubiquitous language throughout the codebase

## C# Language Conventions

### Classes, Interfaces, Methods, Properties, Events
- **PascalCase** for all public members
- **Interface Prefix**: All interfaces start with `I` (e.g., `IGridView`, `IBlockRepository`)

```csharp
// ✅ Correct
public class BlockPlacementService
public interface IBlockRepository
public void ProcessCommand()
public string PlayerName { get; set; }
public event EventHandler<BlockPlacedEventArgs> BlockPlaced;

// ❌ Incorrect
public class blockPlacementService
public interface BlockRepository
public void processCommand()
public string playerName { get; set; }
```

### Fields and Variables
- **Private fields**: `_camelCase` with underscore prefix
- **Local variables**: `camelCase`
- **Method parameters**: `camelCase`

```csharp
// ✅ Correct
private readonly ILogger _logger;
private Vector2Int _currentPosition;

public void MoveBlock(Vector2Int targetPosition)
{
    var validationResult = ValidateMove(targetPosition);
    // ...
}

// ❌ Incorrect
private readonly ILogger Logger;
private Vector2Int CurrentPosition;

public void MoveBlock(Vector2Int TargetPosition)
{
    var ValidationResult = ValidateMove(TargetPosition);
    // ...
}
```

### Constants and Enums
- **PascalCase** for both constants and enum values

```csharp
// ✅ Correct
public const int DefaultGridSize = 10;
public enum BlockType
{
    Empty,
    Solid,
    Liquid
}
```

### Testing Conventions
- **Test Classes**: `{ClassUnderTest}Tests`
- **Test Methods**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- **Use FluentAssertions**: All assertions must use FluentAssertions library

```csharp
// ✅ Correct FluentAssertions
result.Should().Be(5, "because the calculation should yield 5");
handlerResult.Should().BeSuccess("because the command was valid");

// ✅ Correct Exception Testing
Action act = () => new MyService(nullLogger);
act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
```

## Architecture-Specific Naming

### Commands (CQRS Pattern)
- **Pattern**: `{Action}{Entity}Command`
- **Examples**: `PlaceBlockCommand`, `MoveBlockCommand`, `RemoveBlockCommand`
- **Location**: `src/Features/{Domain}/{Action}/`

### Command Handlers
- **Pattern**: `{Action}{Entity}CommandHandler`
- **Examples**: `PlaceBlockCommandHandler`, `MoveBlockCommandHandler`
- **Location**: Same directory as corresponding command

### Notifications/Events
- **Pattern**: `{Entity}{Action}Notification` (past tense)
- **Examples**: `BlockPlacedNotification`, `BlockMovedNotification`, `BlockRemovedNotification`
- **Location**: `src/Features/{Domain}/{Action}/`

### View Interfaces
- **Pattern**: `I{Entity}{Purpose}View`
- **Examples**: `IGridView`, `IBlockAnimationView`, `IBlockVisualizationView`
- **Location**: `src/Features/{Domain}/{Action}/` (with the feature that uses them)

### Presenters
- **Pattern**: `{Entity}{Purpose}Presenter`
- **Examples**: `GridPresenter`, `BlockManagementPresenter`
- **Location**: `src/Features/{Domain}/{Action}/`

### Services
- **Interface Pattern**: `I{Entity}{Purpose}Service`
- **Implementation Pattern**: `{Entity}{Purpose}Service`
- **Examples**: `IGridStateService` / `GridStateService`

## File and Folder Organization

### Project Structure (Current)
```
src/Features/{Domain}/{Action}/
├── {Action}{Entity}Command.cs
├── {Action}{Entity}CommandHandler.cs
├── {Entity}{Action}Notification.cs
├── I{Entity}{Purpose}View.cs
└── {Entity}{Purpose}Presenter.cs

tests/BlockLife.Core.Tests/Features/{Feature}/
├── {Feature}CommandHandlerTests.cs
├── {Feature}ServiceTests.cs
└── {Feature}ValidationTests.cs
```

### Reference Implementation
**GOLD STANDARD**: `src/Features/Block/Move/` - Copy this structure for all new work
```
Features/Block/Move/
├── MoveBlockCommand.cs
├── MoveBlockCommandHandler.cs
├── BlockMovedNotification.cs
├── IBlockAnimationView.cs
└── BlockAnimationPresenter.cs
```

## Godot-Specific Naming

### Scene Files
- **Pattern**: `{entity}_{purpose}.tscn` (lowercase with underscores)
- **Examples**: `grid_view.tscn`, `block_animation.tscn`, `main_menu.tscn`

### Godot Scripts (View Implementations)
- **Pattern**: `{Entity}{Purpose}View.cs` (implementing the interface)
- **Examples**: `GridView.cs`, `BlockAnimationView.cs`

## Work Item Naming (Simplified)

### Work Item Types

| Prefix | Type | Description | Example |
|--------|------|-------------|---------|
| **VS_** | Vertical Slice | Complete feature through all layers | `VS_XXX: Block Rotation` |
| **BF_** | Bug Fix | Defect repair | `BF_XXX: Grid Boundary Fix` |
| **TD_** | Tech Debt | Refactoring or cleanup | `TD_XXX: Notification Refactor` |
| **HF_** | Hotfix | Critical production issue | `HF_XXX: Critical State Fix` |

### Work Item Naming Pattern
```
[Type]_XXX: [Description]
```

**Components:**
1. **Type**: 2-3 letter prefix from the table above
2. **XXX**: Placeholder for simple backlog items (no complex numbering needed)
3. **Description**: Brief, descriptive name (2-5 words)

**Examples:**
```
VS_XXX: Block Rotation System
BF_XXX: Grid Boundary Validation
TD_XXX: Command Pipeline Refactor
HF_XXX: Critical Memory Leak
```

**File Names** (when creating work item files):
```
VS_XXX_Block_Rotation.md
BF_XXX_Grid_Boundary_Fix.md
TD_XXX_Command_Refactor.md
```

## Documentation Organization (Current Structure)

### Simplified Folder Structure
```
Docs/
├── Quick-Start/              # Essential Four - daily essentials
│   ├── Agent_Quick_Reference.md
│   ├── Development_Workflows.md
│   ├── Architecture_Guide.md
│   └── [Templates link]
├── Workflows/               # Process documentation
├── Architecture/            # Technical design and standards  
├── Templates/              # All templates in one place
├── Agents/                 # Agent-specific workflows
├── Backlog/               # Project tracking (simple 3-tier)
├── Testing/               # Testing guidance and debugging
└── _archive/              # Historical documents
```

### Document Naming Conventions
- **Pattern**: `{Purpose}_{Context}.md` (PascalCase with underscores)
- **Examples**: 
  - `Architecture_Guide.md`
  - `Development_Workflows.md`
  - `Agent_Quick_Reference.md`

## Simple Archive Patterns (Solo Development Scale)

For completed work items that need to be archived:

### Basic Archive Pattern
```
Docs/Backlog/archive/YYYY-MM-DD/
├── VS_XXX_Feature_Name.md
├── BF_XXX_Bug_Description.md
└── TD_XXX_Refactor_Description.md
```

### Archive Naming
- **Date folder**: `YYYY-MM-DD` (completion date)
- **File name**: Keep original work item name
- **Simple and searchable**: Use standard file system search, no complex tagging needed

### Archive Organization
```bash
# Find all completed features
find Docs/Backlog/archive -name "VS_*.md"

# Find items completed on specific date
ls Docs/Backlog/archive/2025-08-17/

# Find all bug fixes
find Docs/Backlog/archive -name "BF_*.md"
```

## Anti-Patterns to Avoid

### ❌ Incorrect Naming
```csharp
// Vague or abbreviated names
public class Mgr
public void Proc(string str)
public int cnt;

// Inconsistent casing
public interface blockRepository
public class GridView_Controller

// Type prefixes (except interfaces)
public string strPlayerName;
public int intScore;
```

### ❌ Organizational Anti-Patterns
- Mixing different features in the same folder
- Commands and handlers in different locations
- Complex naming schemes that require documentation to understand
- Over-engineering file organization for solo development scale

## Validation and Enforcement

These naming conventions are enforced through:
- **Code review** - Check against this document
- **Reference implementation** - Copy from `src/Features/Block/Move/`
- **IDE configuration** - EditorConfig settings
- **Agent guidance** - Agents follow these patterns

## Migration from Old Patterns

When updating existing code:
1. **Start with new features** - apply conventions strictly
2. **Refactor incrementally** - one vertical slice at a time
3. **Update tests first** - ensure they reflect new naming
4. **Use IDE refactoring tools** - for safe renaming

## Quick Reference

### Essential Patterns
- **C# Classes**: `PascalCase`
- **Private fields**: `_camelCase`
- **Interfaces**: `IPascalCase`
- **Commands**: `{Action}{Entity}Command`
- **Handlers**: `{Action}{Entity}CommandHandler`
- **Work Items**: `{Type}_XXX: {Description}`
- **Reference**: Copy `src/Features/Block/Move/`

### Key Locations
- **Gold Standard**: `src/Features/Block/Move/`
- **Templates**: `Docs/Templates/Work-Items/`
- **Architecture Guide**: `Docs/Quick-Start/Architecture_Guide.md`
- **Development Workflow**: `Docs/Quick-Start/Development_Workflows.md`

---

*This consolidated guide replaces multiple naming documents, providing a single source of truth optimized for solo development flow while maintaining professional code quality standards.*