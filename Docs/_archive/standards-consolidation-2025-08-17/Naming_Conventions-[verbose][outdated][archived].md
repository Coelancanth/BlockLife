# Naming Conventions Guide

## Overview
This document establishes consistent naming conventions for the BlockLife project, ensuring clarity, maintainability, and adherence to Clean Architecture principles.

## General Principles

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

// ❌ Incorrect
public const int DEFAULT_GRID_SIZE = 10;
public enum BlockType
{
    empty,
    solid,
    liquid
}
```

### Formatting Standards
- Use **4 spaces** instead of tabs for indentation
- Use **Allman Style** bracing (opening brace on new line)
- Line length limit: **100 characters**
- Use **UTF-8** encoding without byte order mark
- Use **LF** line endings

```csharp
// ✅ Correct Allman Style
if (x > 0)
{
    DoSomething();
}

// ❌ Incorrect
if (x > 0) {
    DoSomething();
}
```

### Testing Conventions
- **Test Classes**: `{ClassUnderTest}Tests`
- **Test Methods**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- **Use FluentAssertions**: All assertions must use FluentAssertions library
- **Exception Testing**: Use `Action` delegate with `Should().Throw<TException>()`

```csharp
// ✅ Correct FluentAssertions
result.Should().Be(5, "because the calculation should yield 5");
handlerResult.Should().BeSuccess("because the command was valid");

// ✅ Correct Exception Testing
Action act = () => new MyService(nullLogger);
act.Should().Throw<ArgumentNullException>().WithParameterName("logger");

// ❌ Incorrect
Assert.Equal(5, result);
Assert.True(handlerResult.IsSucc);
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

### Queries
- **Pattern**: `{Action}{Entity}Query` or `Get{Entity}Query`
- **Examples**: `GetBlockAtPositionQuery`, `GetGridStateQuery`

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

### Repositories
- **Interface Pattern**: `I{Entity}Repository`
- **Implementation Pattern**: `{Technology}{Entity}Repository`
- **Examples**: `IBlockRepository` / `InMemoryBlockRepository`

### Domain Entities
- **Pattern**: Simple noun representing the business concept
- **Examples**: `Block`, `Grid`, `Player`

### Value Objects
- **Pattern**: Descriptive noun, often compound
- **Examples**: `Vector2Int`, `BlockType`, `GridPosition`

## File and Folder Organization

### Project Structure
```
src/
├── Core/
│   ├── Application/
│   │   ├── Commands/
│   │   ├── Behaviors/
│   │   └── Simulation/
│   ├── Domain/
│   │   ├── {Entity}/
│   │   └── Common/
│   ├── Infrastructure/
│   │   ├── Configuration/
│   │   ├── Logging/
│   │   └── Services/
│   └── Presentation/
├── Features/
│   └── {Domain}/
│       └── {Action}/
│           ├── {Action}{Entity}Command.cs
│           ├── {Action}{Entity}CommandHandler.cs
│           ├── {Entity}{Action}Notification.cs
│           ├── I{Entity}{Purpose}View.cs
│           └── {Entity}{Purpose}Presenter.cs
└── Infrastructure/
    └── {Entity}/
        └── {Technology}{Entity}Repository.cs
```

### Feature Organization (Vertical Slice Architecture)
Each feature should be organized as a complete vertical slice:

```
Features/Block/Move/
├── MoveBlockCommand.cs
├── MoveBlockCommandHandler.cs
├── BlockMovedNotification.cs
├── IBlockAnimationView.cs
├── BlockAnimationPresenter.cs
└── Rules/
    ├── IMoveValidationRule.cs
    └── MoveValidationRule.cs
```

## Godot-Specific Naming

### Scene Files
- **Pattern**: `{entity}_{purpose}.tscn` (lowercase with underscores)
- **Examples**: `grid_view.tscn`, `block_animation.tscn`, `main_menu.tscn`

### Godot Scripts (View Implementations)
- **Pattern**: `{Entity}{Purpose}View.cs` (implementing the interface)
- **Examples**: `GridView.cs`, `BlockAnimationView.cs`

### Scene Organization
```
godot_project/
├── features/
│   └── {domain}/
│       └── {action}/
│           ├── {entity}_{purpose}.tscn
│           └── {Entity}{Purpose}View.cs
├── scenes/
│   ├── Main/
│   └── UI/
└── infrastructure/
    ├── logging/
    └── debug/
```

## Testing Naming Conventions

### Test Classes
- **Pattern**: `{ClassUnderTest}Tests`
- **Examples**: `MoveBlockCommandHandlerTests`, `GridPresenterTests`

### Test Methods
- **Pattern**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- **Examples**: 
  - `Handle_WithValidPosition_ShouldReturnSuccess`
  - `Handle_WithInvalidPosition_ShouldReturnError`
  - `Initialize_WhenViewIsNull_ShouldThrowArgumentNullException`

### Test Fixtures and Builders
- **Builder Pattern**: `{Entity}Builder`
- **Examples**: `BlockBuilder`, `GridStateServiceBuilder`

## Work Item Naming

### Work Item Types

| Prefix | Type | Description | Example |
|--------|------|-------------|---------|
| **VS_** | Vertical Slice | New feature or user story | `VS_001_Block_Rotation.md` |
| **BF_** | Bug Fix | Defect repair | `BF_002_Grid_Boundary_Fix.md` |
| **TD_** | Tech Debt | Refactoring or cleanup | `TD_003_Notification_Refactor.md` |
| **HF_** | Hotfix | Critical production issue | `HF_001_Critical_State_Fix.md` |

### Work Item Naming Pattern
```
[Type]_[Number]_[Domain]_[Description].md
```

**Components:**
1. **Type**: 2-3 letter prefix from the table above
2. **Number**: 
   - `XXX` for unprioritized items in backlog
   - `001-999` for prioritized items
3. **Domain**: System or feature area (e.g., Block, Inventory, Grid)
4. **Description**: Brief, descriptive name (2-4 words)

**Examples:**
```
VS_001_Block_Rotation.md           # Prioritized vertical slice
VS_XXX_Multiplayer_Init.md         # Unprioritized backlog item
BF_002_Grid_Boundary_Validation.md # Bug fix
TD_003_Command_Pipeline_Refactor.md # Tech debt
HF_001_Critical_Memory_Leak.md     # Hotfix
```

## Documentation Naming

### Folder Structure and Naming
All documentation follows a numbered category system for consistent organization:

```
Docs/
├── 0_Global_Tracker/              # Project-wide tracking and status
├── 1_Architecture/                # Architecture principles, patterns, guides
├── 2_Design/                      # Game design and business logic
├── 3_Implementation_Plans/        # Feature implementation plans
├── 4_Post_Mortems/               # Bug reports and lessons learned
├── 5_Architecture_Decision_Records/ # ADRs and technical decisions
├── 6_Guides/                     # How-to guides and workflows
├── 7_Overview/                   # High-level project overviews
├── DOCUMENTATION_CATALOGUE.md    # Master index of all documents
└── README.md                     # Project entry point
```

### Markdown File Naming Conventions

#### **General Pattern**: `{Purpose}_{Context}.md`
- Use **PascalCase** with **underscores** as separators
- Be descriptive but concise
- Include context when multiple similar documents exist

#### **Category-Specific Patterns**

**1. Architecture Documents (`1_Architecture/`)**
- **Pattern**: `{Topic}_{Type}.md`
- **Examples**: 
  - `Architecture_Guide.md`
  - `Naming_Conventions.md`
  - `Standard_Patterns.md`
  - `Property_Based_Testing_Guide.md`

**2. Design Documents (`2_Design/`)**
- **Pattern**: `{System}_{Purpose}.md`
- **Examples**:
  - `Game_Design_Overview.md`
  - `Core_Mechanics/Life_Stages.md`
  - `Core_Mechanics/Personality_System.md`

**3. Implementation Plans (`3_Implementation_Plans/`)**
- **Pattern**: `{FeatureName}_Implementation_Plan.md`
- **Numbering**: Use numeric prefixes for phases/priority
- **Examples**:
  - `1_Block_Placement_Implementation_Plan.md`
  - `02_Move_Block_Feature_Implementation_Plan.md`
  - `3_Animation_System_Implementation_Plan.md`

**4. Post Mortems (`4_Post_Mortems/`)**
- **Pattern**: `{System}_{Issue}_Report.md` or `{Context}_Lessons_Learned.md`
- **Examples**:
  - `F1_Architecture_Stress_Test_Report.md`
  - `Architecture_Stress_Testing_Lessons_Learned.md`
  - `Move_Block_Architecture_Stress_Test_Report.md`

**5. Architecture Decision Records (`5_Architecture_Decision_Records/`)**
- **Pattern**: `ADR_{Number}_{Decision_Title}.md`
- **Numbering**: Use sequential 3-digit numbers (001, 002, 003...)
- **Examples**:
  - `ADR_001_Clean_Architecture_Enforcement.md`
  - `ADR_002_CQRS_Implementation_Pattern.md`
  - `ADR_003_Functional_Error_Handling.md`

**6. Guides (`6_Guides/`)**
- **Pattern**: `{Purpose}_{Type}.md`
- **Examples**:
  - `Essential_Development_Workflow.md`
  - `Quick_Reference_Development_Checklist.md`
  - `Debugging_Notification_Pipeline.md`
  - `Pull_Request_Guide.md`

**7. Overview Documents (`7_Overview/`)**
- **Pattern**: `{Scope}_{Purpose}.md`
- **Examples**:
  - `BlockLife_Comprehensive_Overview.md`
  - `Current_Implementation_Overview.md`

### Special Document Types

#### **Templates and Examples**
- **Templates**: Prefix with `TEMPLATE_`
- **Examples**: Prefix with `EXAMPLE_`
- **Examples**:
  - `TEMPLATE_Bug_Post_Mortem.md`
  - `EXAMPLE_Architecture_Decision_Record.md`

#### **Index and Catalogue Documents**
- **Master Catalogue**: `DOCUMENTATION_CATALOGUE.md` (at root level)
- **Category Indices**: `{Category}_INDEX.md` (if needed within categories)

#### **Status and Tracking Documents**
- **Global Status**: `Implementation_Status_Tracker.md`
- **Action Items**: `Master_Action_Items.md`
- **Meeting Notes**: `{Date}_{Purpose}_Notes.md` (YYYY-MM-DD format)

### File Content Structure

#### **Standard Document Headers**
All documents should start with:
```markdown
# Document Title

## Overview
Brief description of the document's purpose and scope.

## Status
- **Created**: YYYY-MM-DD
- **Last Updated**: YYYY-MM-DD  
- **Status**: [Draft|In Progress|Complete|Deprecated]
- **Related Documents**: Links to related docs
```

#### **Implementation Plans Structure**
```markdown
# {Feature Name} Implementation Plan

## Status
- **Phase**: [Planning|In Progress|Testing|Complete]
- **Priority**: [Critical|High|Medium|Low]
- **Estimated Effort**: {time estimate}

## Overview
## Architecture Impact
## Implementation Steps
## Testing Strategy
## Acceptance Criteria
## Related Documents
```

#### **Post Mortem Structure**
```markdown
# {Issue/Event} Post Mortem

## Summary
## Timeline
## Root Cause Analysis
## Impact Assessment
## Lessons Learned
## Action Items
## Prevention Measures
```

#### **ADR Structure**
```markdown
# ADR {Number}: {Decision Title}

## Status
[Proposed|Accepted|Deprecated|Superseded]

## Context
## Decision
## Consequences
## Alternatives Considered
## Related ADRs
```

### Documentation Anti-Patterns

#### ❌ **Incorrect Document Naming**
```
// Mixed separators and casing
Architecture-guide.md
implementation_plan_moveblock.md
Bug-Post-Mortem_001.md

// Inconsistent numbering
ADR-006_Something.md
ADR_007_SomethingElse.md
1_F1_Block_Placement.md (mixing prefixes)

// Vague or abbreviated names
arch_doc.md
impl_plan.md
bug_fix.md
```

#### ❌ **Organizational Anti-Patterns**
- Documents in wrong categories
- Missing status information
- No cross-references between related documents
- Outdated documentation not marked as deprecated
- Templates mixed with actual documents

### Migration Guidelines for Existing Documents

**Current Inconsistencies to Fix:**
1. **Folder duplication**: Merge `4_Post_Mortems/` into `4_Post_Mortems/`
2. **ADR numbering**: Standardize to `ADR_{###}_{Title}.md` format
3. **Missing prefixes**: Add appropriate prefixes to implementation plans
4. **Template identification**: Clearly mark template vs example documents

**Migration Steps:**
1. **Audit current structure** - catalog all existing documents
2. **Create migration mapping** - old name → new name
3. **Update cross-references** - fix all internal links
4. **Batch rename operations** - use systematic approach
5. **Update catalogue** - reflect new structure in master index
6. **Validate links** - ensure no broken references remain

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
- Notifications scattered across multiple directories
- Views not co-located with their presenters

## Migration Guidelines

When refactoring existing code to follow these conventions:

1. **Start with new features** - apply conventions strictly
2. **Refactor incrementally** - one vertical slice at a time
3. **Update tests first** - ensure they reflect new naming
4. **Use IDE refactoring tools** - for safe renaming
5. **Update documentation** - keep it in sync with code changes

## Validation

These naming conventions are enforced through:
- **Architecture Fitness Tests** in `tests/Architecture/ArchitectureFitnessTests.cs`
- **Code review checklists**
- **IDE configuration** (EditorConfig, ReSharper settings)
- **Static analysis tools**

## Examples of Correct Implementation

### Complete Feature Example: Move Block
```
src/Features/Block/Move/
├── MoveBlockCommand.cs              // Command definition
├── MoveBlockCommandHandler.cs       // Command handler
├── BlockMovedNotification.cs        // Domain event
├── IBlockAnimationView.cs          // View interface
├── BlockAnimationPresenter.cs      // Presenter
└── Rules/
    ├── IMoveValidationRule.cs      // Rule interface
    └── MoveValidationRule.cs       // Rule implementation

godot_project/features/block/move/
├── block_animation.tscn            // Scene file
└── BlockAnimationView.cs          // View implementation

tests/BlockLife.Core.Tests/Features/Block/Move/
├── MoveBlockCommandHandlerTests.cs
├── BlockAnimationPresenterTests.cs
└── MoveValidationRuleTests.cs
```

This naming system ensures consistency, clarity, and maintainability across the entire BlockLife codebase while supporting Clean Architecture principles and domain-driven design.