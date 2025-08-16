# VSA Organization Patterns

*This content will be managed by the VSA Refactoring agent.*

## File Organization

### Core Logic (`src/` - BlockLife.Core project)
- `Core/`: Foundation classes (GameStrapper, Infrastructure)
- `Features/`: Feature slices organized by domain (e.g., `Features/Block/Move/`)
- Each feature contains: Commands, Handlers, DTOs, View Interfaces, Presenters

### Godot Integration (`godot_project/` and root)
- `godot_project/features/`: Godot scene files and View implementations  
- `godot_project/infrastructure/`: Logging and debug systems
- `godot_project/scenes/`: Main scenes including SceneRoot
- Root level: Godot-specific C# scripts that implement View interfaces

### Testing
- `tests/`: Comprehensive test suite with four pillars:
  - **Unit Tests**: Example-based validation of business logic
  - **Property Tests**: Mathematical proofs using FsCheck.Xunit
  - **Architecture Tests**: Automated architectural constraint enforcement
  - **Integration Tests**: GdUnit4 for Godot-specific testing

## VSA Slice Independence Principles

### What to Keep in Slices
- Feature-specific business logic
- Feature-specific DTOs and validation rules
- Slice-specific tests
- UI components for specific features
- Feature workflows and orchestration

### What to Extract to Shared Infrastructure
- Database repositories and external service integrations
- Cross-cutting utilities and framework abstractions
- Domain primitives (GridPosition, BlockId, etc.)
- Technical utilities and mathematical functions
- Base classes for common patterns

## Reference Implementation

**🎯 Gold Standard**: `src/Features/Block/Move/` 
- Complete vertical slice implementation
- Proper separation of concerns
- Clean dependency flow
- Comprehensive test coverage

For detailed refactoring guidance, see [Docs/Workflows/vsa-refactoring-workflow.md](../../Workflows/vsa-refactoring-workflow.md).