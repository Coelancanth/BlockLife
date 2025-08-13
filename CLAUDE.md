# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 📚 IMPORTANT: Documentation Navigation
**FIRST STOP:** Always consult `Docs/DOCUMENTATION_CATALOGUE.md` for a complete index of all documentation. This catalogue helps you quickly locate:
- Implementation plans and their status
- Bug post-mortems and lessons learned  
- Architecture FAQ and decisions
- Testing strategies and patterns
- Reference implementations

## Project Overview

BlockLife is a C# Godot 4.4 game implementing a strict Clean Architecture with Model-View-Presenter (MVP) pattern. The project uses CQRS with functional programming principles (LanguageExt.Core) and maintains a pure C# core separated from Godot-specific presentation code. Development 
environment is windows 10, use powershell.

## 🚀 Quick Start for New Features

**Before implementing ANY feature:**
1. **Check the catalogue**: `Docs/DOCUMENTATION_CATALOGUE.md` for navigation
2. Check if implementation plan exists: `Docs/3_Implementation_Plan/[FeatureName]_Implementation_Plan.md`
3. Read workflow guide: `Docs/5_Guide/Comprehensive_Development_Workflow.md`
4. Use checklist: `Docs/5_Guide/Quick_Reference_Development_Checklist.md`
5. Reference Move Block implementation: `src/Features/Block/Move/` (Gold Standard)

## Build and Development Commands

### Building the Project
```bash
dotnet build                    # Build the entire solution
dotnet build --configuration Release  # Release build
```

### Running Tests (TDD Workflow)
```bash
# WORKFLOW STEP 1: Architecture fitness tests (run FIRST)
dotnet test --filter "FullyQualifiedName~Architecture"

# WORKFLOW STEP 2: TDD Red-Green-Refactor cycle
dotnet test tests/BlockLife.Core.Tests.csproj --filter "Category=Unit"

# WORKFLOW STEP 3: Property-based mathematical proofs  
dotnet test --filter "FullyQualifiedName~PropertyTests"

# WORKFLOW STEP 4: All core tests together
dotnet test tests/BlockLife.Core.Tests.csproj

# WORKFLOW STEP 5: Integration tests (GdUnit4 - requires Godot)
addons/gdUnit4/runtest.cmd --godot_bin "path/to/godot.exe"

# Or set environment variable and run
set GODOT_BIN=C:\path\to\godot.exe
addons\gdUnit4\runtest.cmd

# QUALITY GATE: Full validation pipeline
dotnet build && dotnet test tests/BlockLife.Core.Tests.csproj && echo "✅ Ready for commit"
```

### Godot Project
- Main project file: `project.godot`
- Main scene: Located in `godot_project/scenes/Main/main.tscn`
- The game uses Godot 4.4 with C# support enabled

### Autoload Configuration (CRITICAL)
- **SceneRoot**: Configured as autoload singleton at `/root/SceneRoot`
- **Location**: `godot_project/scenes/Main/SceneRoot.tscn` (NOT the .cs file!)
- **Contains**: SceneRoot node with LogSettingsController as child
- **Access Pattern**: Views use `GetNode<SceneRoot>("/root/SceneRoot")`
- **WARNING**: Never attach SceneRoot script to other scenes (causes duplicate instantiation)

## Architecture Overview

This project implements a rigorous 3-layer architecture with compiler-enforced boundaries:

### Project Structure
- **`BlockLife.Core.csproj`** (`src/` folder): Pure C# business logic, absolutely NO Godot dependencies
- **`BlockLife.csproj`** (root): Godot-aware presentation layer, references Core project  
- **`BlockLife.Core.Tests.csproj`** (`tests/` folder): Unit tests for core logic

### Key Architectural Principles

1. **Clean Architecture Enforcement**: Model layer (in `src/`) MUST NOT use `using Godot;`
2. **CQRS Pattern**: All state changes via Commands/Handlers, all reads via Queries
3. **Functional Programming**: Uses `LanguageExt.Fin<T>` and `Option<T>` for error handling and null safety
4. **MVP with Humble Presenters**: Presenters coordinate between pure Model and Godot Views
5. **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection throughout

### Critical Patterns

**Command/Query Flow:**
```
User Input → Presenter → Command → Handler → Effect → SimulationManager → Notification → Presenter → View Update
```

**Presenter Lifecycle:** 
All Views implement `IPresenterContainer<T>` and use `SceneRoot.Instance.CreatePresenterFor(this)` for automatic presenter creation/disposal.

**View Interfaces:** 
Presenters depend on interfaces (e.g., `IGridView`) that expose capabilities, not concrete Godot nodes.

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

## Development Guidelines

### Comprehensive Development Workflow
**MANDATORY**: Follow the comprehensive TDD+VSA workflow documented in `Docs/5_Guide/Comprehensive_Development_Workflow.md` and use the `Quick_Reference_Development_Checklist.md` for daily tasks.

### Adding New Features (TDD + Vertical Slice Approach)
**MUST follow this exact sequence:**

1. **Architecture Tests First** - Write architecture fitness tests to define constraints
2. **TDD Red Phase** - Write failing unit tests for Commands and Handlers
3. **Property Tests** - Define mathematical invariants using FsCheck
4. **TDD Green Phase** - Implement minimal code to pass tests:
   - Create feature slice in `src/Features/[Domain]/[Feature]/`
   - Define Commands, Handlers, and View Interfaces in Core project
5. **TDD Refactor Phase** - Optimize while keeping tests green
6. **Integration Tests** - Implement View and Controller Nodes in `godot_project/`
7. **Quality Gates** - Run full test suite and architecture validation
8. **Documentation** - Update relevant docs and implementation plans

### Error Handling
- Use `Fin<T>` for operations that can fail
- Use `Option<T>` instead of nullable references  
- Structured errors with `Error.New(code, message)`

### Dependency Injection
- All services registered in GameStrapper
- Views get Presenters via PresenterFactory pattern
- NO static service locators in application code

### Key Dependencies
- **LanguageExt.Core**: Functional programming primitives
- **MediatR**: Command/Query handling
- **Serilog**: Logging framework
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **FsCheck.Xunit**: Property-based testing framework
- **GdUnit4**: Godot testing framework

## Important Constraints

- Model layer (`src/`) MUST be Godot-agnostic for testability and portability
- All state changes MUST go through Command Handlers 
- Presenters MUST NOT hold authoritative game state
- Use constructor injection for all dependencies
- Follow the established folder structure and naming conventions

## Test Strategy and Enforcement

The codebase uses a **Four-Pillar Testing Strategy** with automated architectural enforcement:

### Current Test Statistics
- **60 total tests** providing **1,935 validations** (Updated after Move Block Phase 1)
- **35 unit tests**: Example-based validation (+5 MoveBlockCommandHandlerTests)
- **9 property tests**: 900 mathematical proofs (100 cases each)
- **16 architecture tests**: Automated constraint enforcement

### Architecture Fitness Functions
Automated tests prevent architectural drift by enforcing:
- ✅ Clean Architecture boundaries (no Godot in Core)
- ✅ Command immutability (DTOs with init setters)
- ✅ Handler error handling (Fin<T> return types)
- ✅ Domain entity immutability (records preferred)
- ✅ Presenter state restrictions (no mutable fields)
- ✅ Service interface patterns (I-prefixed interfaces)
- ✅ No static service locators (constructor injection only)

### Property-Based Testing
Mathematical validation of architectural invariants:
- Grid position boundary constraints
- Block type classification consistency
- Mathematical properties (distance, adjacency)
- Generator correctness for test data

The architecture prioritizes long-term maintainability and testability over rapid prototyping convenience.

## Essential Documentation References

### 📚 **Primary Documents to Consult (In Order)**
1. **`Docs/5_Guide/Comprehensive_Development_Workflow.md`** - Complete TDD+VSA process
2. **`Docs/5_Guide/Quick_Reference_Development_Checklist.md`** - Daily workflow checklists
3. **`Docs/1_Architecture/Architecture_Guide.md`** - Core architectural principles
4. **`Docs/3_Implementation_Plan/`** - Feature-specific implementation plans

### 📋 **Implementation Plans by Feature**
- **Vertical Slice Architecture**: `0_Vertical_Slice_Architecture_Plan.md` - Core VSA patterns
- **Block Placement (F1)**: `1_F1_Block_Placement_Implementation_Plan.md` - Foundation feature
- **Move Block**: `2_Move_Block_Feature_Implementation_Plan.md` ✅ **Phase 1 COMPLETED** (Reference Implementation)
- **Animation System**: `3_Animation_System_Implementation_Plan.md` - Animation queuing and state
- **Dotnet Templates**: `5_Dotnet_New_Templates_Implementation_Plan.md` - Project templates

### 🎯 **Reference Implementation: Move Block Feature**
The Move Block feature (Phase 1 completed) serves as the **GOLD STANDARD** for implementation:
- **Location**: `src/Features/Block/Move/` and `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Pattern**: TDD Red-Green-Refactor with architecture tests first
- **Components**: MoveBlockCommand, MoveBlockCommandHandler, BlockMovedNotification
- **Testing**: 5 comprehensive unit tests following exact TDD workflow

## Agent-Specific Workflow Instructions

### For Implementation Planning (implementation-planner agent)
**MUST consult these documents:**
1. **First**: `Docs/3_Implementation_Plan/0_Vertical_Slice_Architecture_Plan.md`
2. **Then**: `Docs/5_Guide/Comprehensive_Development_Workflow.md`
3. **Reference**: Move Block implementation in `src/Features/Block/Move/`
4. **Follow**: TDD+VSA workflow exactly as demonstrated

**Required planning elements:**
- Architecture fitness tests definition
- Vertical slice breakdown
- TDD cycle for each component (Red-Green-Refactor)
- Property test requirements for invariants
- Integration test boundaries
- Quality gates and acceptance criteria

### For Code Review (code-review-expert agent)
**MUST validate against:**
1. **Workflow**: `Docs/5_Guide/Comprehensive_Development_Workflow.md`
2. **Architecture**: `Docs/1_Architecture/Architecture_Guide.md`
3. **Reference**: Move Block implementation as gold standard
4. **Tests**: `tests/Architecture/ArchitectureFitnessTests.cs`

**Validation checklist:**
✅ Architecture fitness tests written first
✅ Commands are immutable records with init setters
✅ Handlers return Fin<T> for error handling  
✅ No Godot dependencies in Core project
✅ Presenters follow MVP pattern without mutable state
✅ Property tests cover mathematical invariants
✅ Integration tests verify complete slices
✅ Documentation updated per workflow

### For Documentation Updates (docs-updater agent)
**MUST maintain these documents:**
1. **Workflow Docs**: `Docs/5_Guide/Comprehensive_Development_Workflow.md`
2. **Checklist**: `Docs/5_Guide/Quick_Reference_Development_Checklist.md`
3. **Implementation Plans**: Update status in `Docs/3_Implementation_Plan/`
4. **Architecture Docs**: Keep `Docs/1_Architecture/` current

**Update requirements:**
- Mark completed phases in implementation plans
- Update test statistics when new tests added
- Maintain consistency with Move Block reference implementation
- Document any new patterns discovered

### For General Development
**Claude Code MUST:**
1. **Check documentation** in this order:
   - Relevant implementation plan in `Docs/3_Implementation_Plan/`
   - `Docs/5_Guide/Comprehensive_Development_Workflow.md`
   - `Docs/5_Guide/Quick_Reference_Development_Checklist.md`
2. **Reference Move Block** implementation as the pattern to follow
3. **Run tests** in this sequence:
   - Architecture tests first: `dotnet test --filter "FullyQualifiedName~Architecture"`
   - Unit tests: `dotnet test --filter "Category=Unit"`
   - All tests: `dotnet test tests/BlockLife.Core.Tests.csproj`
4. **Use TodoWrite** tool to track workflow compliance
5. **Follow TDD** Red-Green-Refactor cycle religiously
6. **Validate** against 4-pillar testing strategy

## 🔍 Common Agent Queries - Quick Answers

### "How should I implement a new feature?"
1. Check `Docs/3_Implementation_Plan/` for existing plan
2. Follow `Docs/5_Guide/Comprehensive_Development_Workflow.md`
3. Copy pattern from `src/Features/Block/Move/` (reference implementation)
4. Start with architecture tests, then TDD Red-Green-Refactor

### "What's the correct project structure?"
- **Core Logic**: `src/Features/[Domain]/[Feature]/` (e.g., `src/Features/Block/Move/`)
- **Tests**: `tests/BlockLife.Core.Tests/Features/[Domain]/[Feature]/`
- **Godot Views**: `godot_project/features/[domain]/[feature]/`
- **Documentation**: `Docs/3_Implementation_Plan/[Feature]_Implementation_Plan.md`

### "How do I write tests?"
1. **Architecture tests first**: Already exist in `tests/Architecture/ArchitectureFitnessTests.cs`
2. **Unit tests (TDD)**: Write failing test → Implement → Pass → Refactor
3. **Property tests**: Use FsCheck for mathematical invariants
4. **Integration tests**: Use GdUnit4 for Godot-specific testing

### "What patterns should I follow?"
- **Commands**: Immutable records with init setters (see `MoveBlockCommand.cs`)
- **Handlers**: Return `Fin<T>` for error handling (see `MoveBlockCommandHandler.cs`)
- **Notifications**: Immutable records for events (see `BlockMovedNotification.cs`)
- **Services**: Always use interfaces (e.g., `IGridStateService`)

### "How do I validate my implementation?"
```bash
# Run in this order:
dotnet test --filter "FullyQualifiedName~Architecture"  # Architecture compliance
dotnet test --filter "Category=Unit"                    # Unit tests
dotnet test tests/BlockLife.Core.Tests.csproj          # All tests
```