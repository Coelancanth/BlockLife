# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 📚 IMPORTANT: Documentation Navigation
**FIRST STOP:** Always consult [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for a complete index of all documentation. This catalogue helps you quickly locate:
- Implementation plans and their status
- Bug post-mortems and lessons learned  
- Architecture FAQ and decisions
- Testing strategies and patterns
- Reference implementations

## 📋 Backlog - SINGLE SOURCE OF TRUTH
**ALL work tracking happens in:** [Backlog/Backlog.md](Docs/Backlog/Backlog.md)
- **Dynamic Tracker**: Single file tracking all work in real-time
- **Work Item Types**: VS (Vertical Slice), BF (Bug Fix), TD (Tech Debt), HF (Hotfix)
- **Status**: This is the ONLY place for work tracking (0_Global_Tracker is DEPRECATED)
- **Naming Convention**: [Work_Item_Naming_Conventions.md](Docs/6_Guides/Work_Item_Naming_Conventions.md)
- **Maintained by**: Agile Product Owner agent (automatically triggered after EVERY development action)

## 🔄 Agent Workflow System

### **Hybrid Workflow Architecture**

Each agent has a dedicated workflow file that defines its exact procedures. Main agent orchestrates by telling agents to read their workflows.

#### Workflow Files Location
All workflows are documented in `Docs/Workflows/`:
- `product-owner-workflow.md` - Strategic product decisions
- `backlog-maintainer-workflow.md` - Silent tracking updates
- Additional workflows added as new agents are created

### **Dynamic Backlog Pattern - CRITICAL**

**The Backlog Maintainer agent MUST be triggered automatically after EVERY development action to maintain the Backlog as the Single Source of Truth.**

#### Trigger Points for PO Updates:

1. **Feature Implementation Progress**
   ```
   Main Agent completes work → Trigger PO:
   "Phase X of VS_XXX completed, update Backlog.md progress to Y%"
   ```

2. **Bug Discovery**
   ```
   Bug found during testing → Trigger PO:
   "Create BF item for [bug description], link to affected feature"
   ```

3. **Code Review Findings**
   ```
   Review identifies issues → Trigger PO:
   "Review found N issues in VS_XXX, create TD items if needed"
   ```

4. **Test Results**
   ```
   Tests fail/pass → Trigger PO:
   "Update VS_XXX status based on test results: [details]"
   ```

5. **Architecture/Stress Test Results**
   ```
   Critical issues found → Trigger PO:
   "Stress test found critical issues, create HF items: [list]"
   ```

#### Agent Invocation Pattern:

```python
# Standard pattern for invoking any agent with workflow
def trigger_agent(agent_type, action, context):
    Task(
        description=f"{action} action",
        prompt=f"""
        Read your workflow at: Docs/Workflows/{agent_type}-workflow.md
        
        Execute action: {action}
        Context: {context}
        
        Follow the workflow steps exactly as documented.
        Return the outputs specified in the workflow.
        """,
        subagent_type=agent_type
    )

# Example: Feature request
trigger_agent(
    "product-owner",
    "feature_request",
    {"idea": "Add multiplayer", "current_priorities": ["HF_002", "HF_003"]}
)

# Example: Silent progress update  
trigger_agent(
    "backlog-maintainer",
    "update_progress", 
    {"item_id": "VS_000", "event": "tests_passing"}
)
```


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

#### 🚀 **NEW: Automated Test Monitoring (Recommended)**
```bash
# BEST PRACTICE: Use automated test watcher during development
.\test-watch.bat                    # Runs every 10s, auto-stops after 30min inactivity

# Or with custom settings:
python scripts/test_monitor.py --continuous --interval 5 --timeout 60

# Single test run with file output (for Claude Code collaboration)
python scripts/test_monitor.py      # Creates test-summary.md and test-results.json
```

**Benefits of Automated Monitoring:**
- ✅ No manual copy-pasting of test results for Claude Code
- ✅ Auto-stops after inactivity (prevents zombie processes)
- ✅ Structured output in both markdown and JSON formats
- ✅ Tracks file changes and shows time until auto-stop
- ✅ Perfect for TDD workflow with continuous feedback

#### Manual Test Commands
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

### 🔄 **Test Monitor - Automated Test Runner with File Output**
```bash
# Single run - generates test-summary.md and test-results.json
python scripts/test_monitor.py

# Continuous monitoring with auto-timeout
python scripts/test_monitor.py --continuous --interval 10 --timeout 30

# Quick development watcher (recommended)
.\test-watch.bat  # Pre-configured with sensible defaults
```

**Key Features:**
- Monitors `src/`, `tests/`, and `godot_project/` for changes
- Auto-stops after specified minutes of inactivity
- Generates structured output for Claude Code to read
- Shows countdown timer until auto-stop
- Perfect for TDD workflow

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
python scripts/setup_git_hooks.py

# This automatically creates git hooks that:
# - Prevent working on main branch
# - Enforce code formatting standards
# - Run architecture fitness tests
# - Validate unit tests before commits/pushes
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

### 🚨 CRITICAL: GdUnit4 Integration Test Pattern
**WARNING**: GdUnit4 integration tests are extremely sensitive to architectural patterns!

## ✅ MANDATORY PATTERN: SimpleSceneTest Architecture

**ALL integration tests MUST follow this exact pattern - no exceptions:**

```csharp
[TestSuite]
public partial class YourIntegrationTest : Node  // MUST inherit from Node directly
{
    private Node? _testScene;
    private IServiceProvider? _serviceProvider;
    private SceneTree? _sceneTree;

    [Before]
    public async Task Setup()
    {
        // 1. Get SceneTree from test node itself
        _sceneTree = GetTree();
        _sceneTree.Should().NotBeNull("scene tree must be available");
        
        // 2. Get SceneRoot autoload - THE REAL ONE, not a test copy
        var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
        if (sceneRoot == null)
        {
            GD.PrintErr("SceneRoot not found - test must be run from Godot editor");
            return;
        }
        
        // 3. Access the REAL service provider via reflection
        var field = typeof(SceneRoot).GetField("_serviceProvider",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _serviceProvider = field?.GetValue(sceneRoot) as IServiceProvider;
        
        // 4. Create test scene as child of this node
        _testScene = await CreateTestScene();
        
        // 5. Access your controllers normally
        _gridController = _testScene!.GetNode<GridInteractionController>("GridView/GridInteractionController");
    }
    
    [TestCase]
    public async Task YourTest()
    {
        if (_serviceProvider == null) 
        {
            GD.Print("Skipping test - not in proper Godot context");
            return;
        }
        // Your test logic using the REAL service provider
        var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
        // Tests now use the same services as production!
    }
}
```

## 🚫 FORBIDDEN PATTERN: Custom Test Base Classes

**NEVER use this pattern - it creates parallel service containers:**

```csharp
// ❌ FORBIDDEN - Creates parallel service containers!
public partial class BadIntegrationTest : GodotIntegrationTestBase
{
    // This pattern creates its own DI container separate from SceneRoot
    // Commands go to one container, notifications from another
    // Results in impossible-to-debug failures with "phantom blocks"
}
```

## 🔑 Critical Architecture Principle

**Single Service Container Rule**: Integration tests MUST use the exact same service instances as production SceneRoot. Any test infrastructure creating parallel containers will cause:
- Commands succeed but don't affect visible state
- Notifications fire but don't match actual state  
- "Blocks carryover between tests" symptoms
- Complete state isolation leading to phantom failures

## 📚 Reference Files

**Working Examples**:
- ✅ `SimpleSceneTest.cs` - Gold standard implementation
- ✅ `BlockPlacementIntegrationTest.cs` - Refactored to use correct pattern

**Investigation Documentation**:  
- 📚 `Docs/4_Post_Mortems/Integration_Test_Architecture_Deep_Dive.md` - Complete debugging journey and architectural lessons

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
- **Vertical Slice Architecture**: [000_Vertical_Slice_Architecture_Plan.md](_Vertical_Slice_Architecture_Plan.md) - Core VSA patterns
- **Block Placement (F1)**: [001_F1_Block_Placement_Implementation_Plan.md](_F1_Block_Placement_Implementation_Plan.md) - Foundation feature
- **Move Block**: [002_Move_Block_Feature_Implementation_Plan.md](_Move_Block_Feature_Implementation_Plan.md) ✅ **Phase 1 COMPLETED** (Reference Implementation)
- **Animation System**: [3_Animation_System_Implementation_Plan.md](Docs/3_Implementation_Plans/3_Animation_System_Implementation_Plan.md) - Animation queuing and state
- **Dotnet Templates**: [005_Dotnet_New_Templates_Implementation_Plan.md](_Dotnet_New_Templates_Implementation_Plan.md) - Project templates

### 🎯 **Reference Implementation: Move Block Feature**
The Move Block feature (Phase 1 completed) serves as the **GOLD STANDARD** for implementation:
- **Location**: `src/Features/Block/Move/` and `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Pattern**: TDD Red-Green-Refactor with architecture tests first
- **Components**: MoveBlockCommand, MoveBlockCommandHandler, BlockMovedNotification
- **Testing**: 5 comprehensive unit tests following exact TDD workflow

## Agent Workflow Reference

All agent-specific workflows are documented in `Docs/Workflows/`:

| Agent | Workflow File | Purpose |
|-------|--------------|---------|
| `product-owner` | [product-owner-workflow.md](Docs/Workflows/product-owner-workflow.md) | User stories, prioritization, acceptance |
| `backlog-maintainer` | [backlog-maintainer-workflow.md](Docs/Workflows/backlog-maintainer-workflow.md) | Silent progress tracking, status updates |
| *(future agents)* | *workflow files added as created* | *specific purposes* |

When triggering any agent, use the standard pattern shown above to have them read their workflow file.

## 🤖 AUTOMATIC AGENT TRIGGERING (Dynamic PO Pattern)

### Core Principle
After EVERY development action, automatically trigger the appropriate agent to maintain the Backlog as the Single Source of Truth.

### Action Detection and Triggering

**Feature/Bug Detection in User Messages:**
```python
# Detect feature requests
if any(phrase in user_message.lower() for phrase in ["i want", "add", "feature", "it would be nice", "can we", "should have", "need"]):
    trigger_product_owner("feature_request", context)

# Detect bug reports
if any(phrase in message.lower() for phrase in ["error", "bug", "broken", "doesn't work", "failed", "crash", "issue"]):
    trigger_product_owner("bug_report", context)
```

**Development Action Detection:**
```python
# After code changes
if tool_used in ['Write', 'Edit', 'MultiEdit'] and file_path.endswith(('.cs', '.py', '.js')):
    trigger_backlog_maintainer("update_progress", {"event": "code_written", "item": current_item})

# After tests written
if tool_used in ['Write', 'Edit'] and 'test' in file_path.lower():
    trigger_backlog_maintainer("update_progress", {"event": "tests_written", "item": current_item})

# After tests pass
if 'dotnet test' in bash_command and 'Passed!' in output:
    trigger_backlog_maintainer("update_progress", {"event": "tests_passing", "item": current_item})
```

### Trigger Mapping

| User Action | Agent | Action | Visibility |
|------------|-------|--------|------------|
| Describes feature | agile-product-owner | feature_request | Visible |
| Reports bug | agile-product-owner | bug_report | Visible |
| Completes work | agile-product-owner | acceptance_review | Visible |
| Writes code | backlog-maintainer | update_progress | Silent |
| Writes tests | backlog-maintainer | update_progress | Silent |
| Tests pass | backlog-maintainer | update_progress | Silent |
| Creates PR | backlog-maintainer | change_status | Silent |

### Standard Invocation Pattern

```python
def trigger_product_owner(action, context):
    """Visible Product Owner decisions"""
    response = Task(
        description=f"Product Owner {action}",
        prompt=f"""
        Read your workflow at: Docs/Workflows/product-owner-workflow.md
        Execute action: {action}
        
        Context:
        - Current priorities: {get_top_priorities()}
        - Backlog: Docs/Backlog/Backlog.md
        - Request: {context}
        
        Follow your workflow and provide decision.
        """,
        subagent_type="agile-product-owner"
    )
    # Present to user
    show_po_decision(response)

def trigger_backlog_maintainer(action, context):
    """Silent backlog updates"""
    Task(
        description=f"Update backlog",
        prompt=f"""
        Read your workflow at: Docs/Workflows/backlog-maintainer-workflow.md
        Execute action: {action}
        
        Context: {context}
        
        Update Docs/Backlog/Backlog.md silently.
        Return only confirmation or errors.
        """,
        subagent_type="backlog-maintainer"
    )
    # No output to user (silent)
```

### For General Development
**Claude Code MUST:**

**🔄 DYNAMIC PO PATTERN (MANDATORY):**
After ANY development action, automatically trigger the appropriate agent:
- **Product Owner (Visible)**: Feature requests, bug reports, acceptance reviews, priority decisions
- **Backlog Maintainer (Silent)**: Code changes, test results, progress updates, status changes

This maintains the Backlog as the Single Source of Truth automatically without manual synchronization.

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
5. **Trigger PO agent** to update Backlog after each action
6. **Follow TDD** Red-Green-Refactor cycle religiously
7. **Validate** against 4-pillar testing strategy
8. **Create Pull Request** for all changes - NO direct commits to main

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