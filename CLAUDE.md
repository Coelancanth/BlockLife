# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 📚 IMPORTANT: Documentation Navigation
**FIRST STOP:** Always consult [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for a complete index of all documentation. This catalogue helps you quickly locate:
- Implementation plans and their status
- Bug post-mortems and lessons learned  
- Architecture FAQ and decisions
- Testing strategies and patterns
- Reference implementations

## Project Overview

BlockLife is a C# Godot 4.4 game implementing a strict Clean Architecture with Model-View-Presenter (MVP) pattern. The project uses CQRS with functional programming principles (LanguageExt.Core) and maintains a pure C# core separated from Godot-specific presentation code. Development 
environment is windows 10, use powershell.

## ⚠️ CRITICAL: Git Workflow Requirements

**🚫 NEVER WORK DIRECTLY ON MAIN BRANCH - NO EXCEPTIONS**

**MANDATORY Git Workflow for ALL Changes:**
1. **Create feature branch FIRST**: `git checkout -b <type>/<description>`
2. **Make changes on branch**: Never on main
3. **Create Pull Request**: Always for review
4. **Wait for approval**: Before merging
5. **Follow guide**: [Git_Workflow_Guide.md](Docs/6_Guides/Git_Workflow_Guide.md)

**Branch Types**: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `hotfix/`

## 🚀 Quick Start for New Features

**Before implementing ANY feature:**
1. **🔥 CREATE BRANCH**: `git checkout -b feat/your-feature-name`
2. **Check the catalogue**: [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for navigation
3. Check if implementation plan exists: `Docs/3_Implementation_Plans/[FeatureName]_Implementation_Plan.md`
4. Read workflow guide: [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
5. Use checklist: [Quick_Reference_Development_Checklist.md](Docs/6_Guides/Quick_Reference_Development_Checklist.md)
6. Reference Move Block implementation: `src/Features/Block/Move/` (Gold Standard)

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

# AUTOMATION: Enhanced quality pipeline with cognitive load reduction
dotnet build && dotnet test tests/BlockLife.Core.Tests.csproj && python scripts/collect_test_metrics.py --update-docs && echo "✅ Ready for commit"
```

## 🤖 Automation Scripts (Cognitive Load Reduction)

BlockLife includes Python automation scripts to reduce manual maintenance and cognitive load:

### 🧪 Test Metrics Automation
```bash
# Automatically update documentation with test statistics
python scripts/collect_test_metrics.py --update-docs

# Integrate with quality gates (recommended)
dotnet test tests/BlockLife.Core.Tests.csproj && python scripts/collect_test_metrics.py --update-docs
```

### 🚨 Git Workflow Enforcement  
```bash
# Setup automatic Git workflow enforcement (HIGHLY RECOMMENDED)
python scripts/enforce_git_workflow.py --setup-hooks

# This prevents working on main branch and validates branch naming
```

### 🔄 Documentation Synchronization
```bash
# Keep all documentation tracking files synchronized
python scripts/sync_documentation_status.py

# Check for broken documentation links
python scripts/sync_documentation_status.py --check-links
```

**Benefits:**
- ✅ Eliminates manual test counting and documentation updates
- ✅ Prevents Git workflow violations automatically
- ✅ Maintains documentation consistency without manual effort
- ✅ Reduces cognitive load for development tasks

**See [scripts/README.md](scripts/README.md) for complete automation guide.**

### Godot Project
- Main project file: `project.godot`
- Main scene: Located in `godot_project/scenes/Main/main.tscn`
- The game uses Godot 4.4 with C# support enabled

### Autoload Configuration (CRITICAL)
- **SceneRoot**: Configured as autoload singleton at `/root/SceneRoot`
- **Location**: `godot_project/scenes/Main/SceneRoot.tscn` (NOT the .cs file!)
- **Contains**: SceneRoot node with unified logging system
- **Access Patterns**: 
  - Presenters: `GetNode<SceneRoot>("/root/SceneRoot").PresenterFactory`
  - Logging: `GetNode<SceneRoot>("/root/SceneRoot").Logger?.ForContext("SourceContext", "UI")`
- **WARNING**: Never attach SceneRoot script to other scenes (causes duplicate instantiation)

## Architecture Overview

This project implements a rigorous 3-layer architecture with compiler-enforced boundaries and **production-ready patterns validated through stress testing**:

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
6. **🔥 CRITICAL: Single Source of Truth**: One entity = One implementation = One DI registration (learned from F1 stress test)

### Critical Patterns

**Command/Query Flow:**
```
User Input → Presenter → Command → Handler → Effect → SimulationManager → Notification → Presenter → View Update
```

**Presenter Lifecycle:** 
All Views implement `IPresenterContainer<T>` and use `SceneRoot.Instance.CreatePresenterFor(this)` for automatic presenter creation/disposal.

**View Interfaces:** 
Presenters depend on interfaces (e.g., `IGridView`) that expose capabilities, not concrete Godot nodes.

**🔥 CRITICAL State Management Pattern:**
```csharp
// ✅ CORRECT - Single service implementing multiple interfaces
services.AddSingleton<GridStateService>();
services.AddSingleton<IGridStateService>(provider => provider.GetRequiredService<GridStateService>());
services.AddSingleton<IBlockRepository>(provider => provider.GetRequiredService<GridStateService>());

// ❌ WRONG - Dual state sources = race conditions
services.AddSingleton<IGridStateService, GridStateService>();
services.AddSingleton<IBlockRepository, InMemoryBlockRepository>();
```

**🔥 CRITICAL Notification Pattern:**
Presenters MUST subscribe to domain notifications in `Initialize()` and unsubscribe in `Dispose()`:
```csharp
public override void Initialize()
{
    BlockPlacementNotificationBridge.BlockPlacedEvent += OnBlockPlacedNotification;
}

public override void Dispose()
{
    BlockPlacementNotificationBridge.BlockPlacedEvent -= OnBlockPlacedNotification;
    base.Dispose();
}
```

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

### 🔥 CRITICAL: Production-Ready Development Workflow
**MANDATORY**: Follow the comprehensive TDD+VSA workflow documented in [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md) and use the [Quick_Reference_Development_Checklist.md](Docs/6_Guides/Quick_Reference_Development_Checklist.md) for daily tasks.

**⚠️ LESSONS LEARNED**: Architecture patterns alone don't guarantee production readiness. See [Architecture_Stress_Testing_Lessons_Learned.md](Docs/4_Post_Mortems/Architecture_Stress_Testing_Lessons_Learned.md) for critical insights from F1 stress testing experience.

### 🛡️ **Pre-Commit Checklist** (Born from F1 Stress Test Experience)
Before marking ANY feature complete:
- [ ] **State Management**: Is there exactly one source of truth per entity?
- [ ] **Notifications**: Are all published events actually subscribed to?
- [ ] **Concurrency**: Can this code handle 100+ concurrent operations safely?
- [ ] **Memory**: Are all subscriptions properly disposed?
- [ ] **Performance**: Are there any N+1 query patterns?
- [ ] **Async Safety**: No `.Wait()` or `.Result` on async operations in validation rules?

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

### Unified Logging System (NEW)
- **Core Layer**: Uses `ILogger` from DI container
- **View Layer**: Access via `GetNode<SceneRoot>("/root/SceneRoot").Logger?.ForContext("SourceContext", "UI")`
- **Structured Logging**: Use proper parameters `logger.Information("Message with {Param}", value)`
- **Categories**: "Commands", "Queries", "UI", "Core", "DI"
- **NEVER use GD.Print()**: Always use structured logger for consistency
- **LogSettings**: Simple boolean flags (`VerboseCommands`, `VerboseQueries`) instead of complex arrays

### Dependency Injection
- All services registered in GameStrapper
- Views get Presenters via PresenterFactory pattern
- Logger exposed via SceneRoot.Logger for View access
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
1. **[Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)** - Complete TDD+VSA process
2. **[Quick_Reference_Development_Checklist.md](Docs/6_Guides/Quick_Reference_Development_Checklist.md)** - Daily workflow checklists
3. **[Architecture_Guide.md](Docs/1_Architecture/Architecture_Guide.md)** - Core architectural principles
4. **`Docs/3_Implementation_Plans/`** - Feature-specific implementation plans

### 📋 **Implementation Plans by Feature**
- **Vertical Slice Architecture**: [000_Vertical_Slice_Architecture_Plan.md](_Vertical_Slice_Architecture_Plan.md) - Core VSA patterns
- **Block Placement (F1)**: [001_F1_Block_Placement_Implementation_Plan.md](_F1_Block_Placement_Implementation_Plan.md) - Foundation feature
- **Move Block**: [002_Move_Block_Feature_Implementation_Plan.md](_Move_Block_Feature_Implementation_Plan.md) ✅ **Phase 1 COMPLETED** (Reference Implementation)
- **Animation System**: [3_Animation_System_Implementation_Plan.md](Docs/3_Implementation_Plans/3_Animation_System_Implementation_Plan.md) - Animation queuing and state
- **Dotnet Templates**: [005_Dotnet_New_Templates_Implementation_Plan.md](_Dotnet_New_Templates_Implementation_Plan.md) - Project templates

### 🎯 **Reference Implementation: Move Block Feature**
The Move Block feature (Phase 1 completed) serves as the **GOLD STANDARD** for implementation:
- **Location**: `src/Features/Block/Move/` and `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Pattern**: TDD Red-Green-Refactor with architecture tests first
- **Components**: MoveBlockCommand, MoveBlockCommandHandler, BlockMovedNotification
- **Testing**: 5 comprehensive unit tests following exact TDD workflow

## Agent-Specific Workflow Instructions

### For Implementation Planning (implementation-planner agent)
**MUST consult these documents:**
1. **First**: [000_Vertical_Slice_Architecture_Plan.md](_Vertical_Slice_Architecture_Plan.md)
2. **Then**: [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
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
1. **Workflow**: [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
2. **Architecture**: [Architecture_Guide.md](Docs/1_Architecture/Architecture_Guide.md)
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
1. **Workflow Docs**: [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
2. **Checklist**: [Quick_Reference_Development_Checklist.md](Docs/6_Guides/Quick_Reference_Development_Checklist.md)
3. **Implementation Plans**: Update status in `Docs/3_Implementation_Plans/`
4. **Architecture Docs**: Keep `Docs/1_Architecture/` current

**Update requirements:**
- Mark completed phases in implementation plans
- Update test statistics when new tests added
- Maintain consistency with Move Block reference implementation
- Document any new patterns discovered

### For General Development
**Claude Code MUST:**

**🚨 CRITICAL FIRST STEP - GIT WORKFLOW:**
0. **NEVER work on main branch** - Always create feature branch first:
   ```bash
   git checkout -b <type>/<description>
   ```
   - **Types**: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `hotfix/`
   - **Must read**: [Git_Workflow_Guide.md](Docs/6_Guides/Git_Workflow_Guide.md)

**DEVELOPMENT WORKFLOW:**
1. **Check documentation** in this order:
   - Relevant implementation plan in [3_Implementation_Plans/](Docs/3_Implementation_Plans/)
   - [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
   - [Quick_Reference_Development_Checklist.md](Docs/6_Guides/Quick_Reference_Development_Checklist.md)
2. **Reference Move Block** implementation as the pattern to follow
3. **Run tests** in this sequence:
   - Architecture tests first: `dotnet test --filter "FullyQualifiedName~Architecture"`
   - Unit tests: `dotnet test --filter "Category=Unit"`
   - All tests: `dotnet test tests/BlockLife.Core.Tests.csproj`
4. **Use TodoWrite** tool to track workflow compliance
5. **Follow TDD** Red-Green-Refactor cycle religiously
6. **Validate** against 4-pillar testing strategy
7. **Create Pull Request** for all changes - NO direct commits to main

## 🔍 Common Agent Queries - Quick Answers

### "Why isn't my view updating after a command succeeds?"
**This is a common notification pipeline issue!** Follow the systematic debugging guide:
1. Check [Debugging_Notification_Pipeline.md](Docs/6_Guides/Debugging_Notification_Pipeline.md) for step-by-step diagnosis
2. Verify command handler publishes via `await _mediator.Publish(notification)`
3. Ensure notification handler bridges to presenter via static events
4. Confirm presenter subscribes to events in `Initialize()` method

### "How should I implement a new feature?"
1. Check [3_Implementation_Plans/](Docs/3_Implementation_Plans/) for existing plan
2. Follow [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md)
3. Copy pattern from `src/Features/Block/Move/` (reference implementation)
4. Start with architecture tests, then TDD Red-Green-Refactor

### "What's the correct project structure?"
- **Core Logic**: `src/Features/[Domain]/[Feature]/` (e.g., `src/Features/Block/Move/`)
- **Tests**: `tests/BlockLife.Core.Tests/Features/[Domain]/[Feature]/`
- **Godot Views**: `godot_project/features/[domain]/[feature]/`
- **Documentation**: `Docs/3_Implementation_Plans/[Feature]_Implementation_Plan.md`

### "How do I write tests?"
1. **Architecture tests first**: Already exist in `tests/Architecture/ArchitectureFitnessTests.cs`
2. **Unit tests (TDD)**: Write failing test → Implement → Pass → Refactor
3. **Property tests**: Use FsCheck for mathematical invariants
4. **Integration tests**: Use GdUnit4 for Godot-specific testing
5. **🚨 CRITICAL: Bug-to-Test Protocol**: Every bug MUST become a regression test - see [Comprehensive_Development_Workflow.md](Docs/6_Guides/Comprehensive_Development_Workflow.md) Section 9.1

### "What patterns should I follow?"
- **Commands**: Immutable records with init setters (see `MoveBlockCommand.cs`)
- **Handlers**: Return `Fin<T>` for error handling (see `MoveBlockCommandHandler.cs`)
- **Notifications**: Immutable records for events (see `BlockMovedNotification.cs`)
- **Services**: Always use interfaces (e.g., `IGridStateService`)
- **Notification Bridges**: Use Static Event Bridge Pattern (see [Standard_Patterns.md](Docs/1_Architecture/Standard_Patterns.md))

### "How do I validate my implementation?"
```bash
# Run in this order:
dotnet test --filter "FullyQualifiedName~Architecture"  # Architecture compliance
dotnet test --filter "Category=Unit"                    # Unit tests
dotnet test tests/BlockLife.Core.Tests.csproj          # All tests
```

### "I found a bug! What's the process?"
**🚨 MANDATORY Bug-to-Test Protocol (NO EXCEPTIONS):**
1. **Document**: Create bug report using [TEMPLATE_Bug_Report_And_Fix.md](Docs/4_Post_Mortems/TEMPLATE_Bug_Report_And_Fix.md)
2. **Reproduce**: Verify bug exists and document exact reproduction steps
3. **Test First**: Write failing regression test that would have caught this bug
4. **Fix**: Implement minimal fix to make the test pass
5. **Validate**: Ensure all tests pass and bug is actually resolved
6. **Learn**: Document lessons learned and prevention strategies

**Key Principle**: **Every bug becomes a permanent test** - this ensures issues never reoccur and tests serve as living documentation.

**Reference Example**: See [BlockId_Stability_Bug_Report.md](Docs/4_Post_Mortems/BlockId_Stability_Bug_Report.md) for complete example.

### "How do I create a proper Pull Request?"
**CRITICAL**: Always use the repository's PR template located at `.github/pull_request_template.md`

**Required Sections (will fail CI if missing):**
- ✅ **## Changes** - Features Added, Bugs Fixed, Refactoring
- ✅ **## Testing** - TDD workflow, test coverage, testing instructions  
- ✅ **## Checklist** - Documentation, Architecture, Code Quality, Final Checks

**Example gh pr create command:**
```bash
gh pr create --title "feat: descriptive feature title" --body "$(cat <<'EOF'
## 📋 PR Description

### What does this PR do?
[Clear description of changes]

### Which issue does this PR address?
[Link to issue or implementation plan]

## Changes

### 🎯 Features Added
- [List new features]

### 🐛 Bugs Fixed  
- [List bugs fixed]

### 🔧 Refactoring
- [Describe refactoring]

## Testing

### 🧪 TDD Workflow Followed
- [x] Architecture fitness tests pass
- [x] Tests written BEFORE implementation (RED phase)
- [x] Implementation done to pass tests (GREEN phase) 
- [x] Code refactored while keeping tests green (REFACTOR phase)

### 📊 Test Coverage
- **Tests added:** [Number]
- **Test categories:**
  - [x] Unit tests
  - [x] Architecture tests

### 🔬 Testing Instructions
1. Run `dotnet test --filter "FullyQualifiedName~Architecture"`
2. Run `dotnet test tests/BlockLife.Core.Tests.csproj`

## Checklist

### 📚 Documentation
- [x] CLAUDE.md updated (if adding new patterns)
- [x] Implementation plan updated (if applicable)
- [x] Code comments added where necessary
- [x] No TODO comments left in code

### 🏗️ Architecture  
- [x] Follows Clean Architecture principles
- [x] No Godot dependencies in Core project
- [x] Commands are immutable records
- [x] Handlers return `Fin<T>` for error handling
- [x] Follows vertical slice architecture

### 🎨 Code Quality
- [x] Code follows existing patterns
- [x] No compiler warnings
- [x] Meaningful variable/method names
- [x] SOLID principles followed

### ✅ Final Checks
- [x] All tests pass locally
- [x] Branch is up to date with target branch
- [x] PR has meaningful title
- [x] Ready for review

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```