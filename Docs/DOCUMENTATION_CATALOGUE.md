# 📚 BlockLife Documentation Catalogue

## 🗺️ Quick Navigation Guide for AI Agents

This catalogue provides a comprehensive index of all documentation, helping AI agents quickly locate necessary information for implementing features, fixing bugs, and maintaining architectural consistency.

## 🎯 Primary References (Start Here)

### Essential Documents
1. **CLAUDE.md** - Root-level agent instructions and project overview
2. **[Git_Workflow_Guide.md](6_Guides/Git_Workflow_Guide.md)** - 🚨 **MANDATORY** Git branch workflow (READ FIRST)
3. **[Comprehensive_Development_Workflow.md](6_Guides/Comprehensive_Development_Workflow.md)** - MANDATORY TDD+VSA workflow to follow
4. **[Quick_Reference_Development_Checklist.md](6_Guides/Quick_Reference_Development_Checklist.md)** - Daily task checklist
5. **[Architecture_Guide.md](1_Architecture/Architecture_Guide.md)** - Core architectural principles

### Reference Implementation
- **Move Block Feature** (`src/Features/Block/Move/`) - GOLD STANDARD implementation
- **Tests**: `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Documentation**: [02_Move_Block_Feature_Implementation_Plan.md](3_Implementation_Plans/02_Move_Block_Feature_Implementation_Plan.md)

## 📂 Documentation Structure

### 0️⃣ Global Tracking ([0_Global_Tracker/](0_Global_Tracker/))
- [Master_Action_Items.md](0_Global_Tracker/Master_Action_Items.md) - **CENTRALIZED** tracker for all post-mortem action items and prevention measures ⭐⭐⭐
- [Implementation_Status_Tracker.md](0_Global_Tracker/Implementation_Status_Tracker.md) - **NEW**: Central tracking of all implementation plan statuses and progress ⭐⭐⭐

### 1️⃣ Architecture Documentation ([1_Architecture/](1_Architecture/))
- [Architecture_Guide.md](1_Architecture/Architecture_Guide.md) - Core principles, Clean Architecture, MVP pattern ⭐⭐⭐
- [Standard_Patterns.md](1_Architecture/Standard_Patterns.md) - **VALIDATED** architectural patterns (notification bridges, MVP, etc.) ⭐⭐⭐
- [Architecture_FAQ.md](1_Architecture/Architecture_FAQ.md) - Frequently asked questions about architecture ⭐⭐
- Additional architecture decisions and patterns

### 2️⃣ Design Documents ([2_Design/](2_Design/))
- Game design specifications
- Feature requirements
- User stories

### 3️⃣ Implementation Plans ([3_Implementation_Plans/](3_Implementation_Plans/))
- [00_Vertical_Slice_Architecture_Plan.md](3_Implementation_Plans/00_Vertical_Slice_Architecture_Plan.md) - VSA patterns and approach
- [01_F1_Block_Placement_Implementation_Plan.md](3_Implementation_Plans/01_F1_Block_Placement_Implementation_Plan.md) - ✅ **PRODUCTION READY** (Gold Standard - All critical fixes applied)
- [02_Move_Block_Feature_Implementation_Plan.md](3_Implementation_Plans/02_Move_Block_Feature_Implementation_Plan.md) - 🔄 **IN PROGRESS** (70% - Phase 1-2 Complete)
- [03_Animation_System_Implementation_Plan.md](3_Implementation_Plans/03_Animation_System_Implementation_Plan.md) - ❌ **NOT STARTED** (Animation architecture)
- [05_Dotnet_New_Templates_Implementation_Plan.md](3_Implementation_Plans/05_Dotnet_New_Templates_Implementation_Plan.md) - ❌ **NOT STARTED** (Project templates)
- [06_Advanced_Logger_And_GameStrapper_Implementation_Plan.md](3_Implementation_Plans/06_Advanced_Logger_And_GameStrapper_Implementation_Plan.md) - ❌ **NOT STARTED** (Infrastructure)
- [07_Dynamic_Logging_UI_Implementation_Plan.md](3_Implementation_Plans/07_Dynamic_Logging_UI_Implementation_Plan.md) - ❌ **NOT STARTED** (Debug tooling)
- [08_Automated_Debug_Console_Implementation_Plan.md](3_Implementation_Plans/08_Automated_Debug_Console_Implementation_Plan.md) - ❌ **NOT STARTED** (Debug tooling)

📊 **[Implementation Status Tracker](0_Global_Tracker/Implementation_Status_Tracker.md)** - **NEW**: Central tracking of all implementation plan statuses and progress

### 4️⃣ Post-Mortems & Architecture Reviews ([4_Post_Mortems/](4_Post_Mortems/) & [4_Bug_PostMortems/](4_Bug_PostMortems/))

#### 🔴 **Architecture Stress Test Reports**
- [F1_Architecture_Stress_Test_Report.md](4_Post_Mortems/F1_Architecture_Stress_Test_Report.md) - ✅ **RESOLVED** - Critical architecture vulnerabilities identified and fixed ⭐⭐⭐
- [Architecture_Stress_Testing_Lessons_Learned.md](4_Post_Mortems/Architecture_Stress_Testing_Lessons_Learned.md) - ✅ **REFERENCE** - Key learnings from F1 stress test experience ⭐⭐⭐
- [Move_Block_Architecture_Stress_Test_Report.md](4_Post_Mortems/Move_Block_Architecture_Stress_Test_Report.md) - 📝 **DRAFT** - F2 Move Block feature architecture review

#### 🐛 **Bug Post-Mortems**
- [TEMPLATE_Bug_Post_Mortem.md](4_Post_Mortems/TEMPLATE_Bug_Post_Mortem.md) - Standard template for post-mortems
- [EXAMPLE_Bug_Post_Mortem.md](4_Post_Mortems/EXAMPLE_Bug_Post_Mortem.md) - Common patterns and examples
- [DI_Container_Presenter_Registration_Bug_Report.md](4_Post_Mortems/DI_Container_Presenter_Registration_Bug_Report.md) - Presenter notification handler constraints
- [SceneRoot_Autoload_Configuration_Bug_Report.md](4_Post_Mortems/SceneRoot_Autoload_Configuration_Bug_Report.md) - Autoload configuration and duplicate instantiation
- [Block_Placement_Display_Bug_Report.md](4_Post_Mortems/Block_Placement_Display_Bug_Report.md) - ✅ Notification pipeline consistency lessons
- [F1_Block_Placement_Implementation_Issues_Report.md](4_Post_Mortems/F1_Block_Placement_Implementation_Issues_Report.md) - Lessons from F1 implementation
- Pattern discoveries and anti-patterns

### 5️⃣ Architecture Decision Records ([5_Architecture_Decision_Records/](5_Architecture_Decision_Records/))
- [ADR_006_Fin_Task_Consistency.md](5_Architecture_Decision_Records/ADR_006_Fin_Task_Consistency.md) - **ACTIVE DECISION**: Fin<T> vs Task<T> consistency
- [ADR_007_Enhanced_Functional_Validation_Pattern.md](5_Architecture_Decision_Records/ADR_007_Enhanced_Functional_Validation_Pattern.md) - Enhanced functional validation patterns instead of FluentValidation ⭐⭐⭐
- [ADR_008_Anchor_Based_Rule_Engine_Architecture.md](5_Architecture_Decision_Records/ADR_008_Anchor_Based_Rule_Engine_Architecture.md) - **NEW**: ✅ **APPROVED** Anchor-based rule engine (150x performance improvement) ⭐⭐⭐
- [ADR_001_Grid_Scanning_Approach_SUPERSEDED.md](5_Architecture_Decision_Records/ADR_001_Grid_Scanning_Approach_SUPERSEDED.md) - ❌ **SUPERSEDED** Grid-scanning approach (performance inadequate)
- [ADR_002_Anchor_Based_Implementation_Guide.md](5_Architecture_Decision_Records/ADR_002_Anchor_Based_Implementation_Guide.md) - **APPROVED APPROACH**: Detailed anchor-based implementation guide ⭐⭐⭐
- [Developer_Tooling_Guide.md](5_Architecture_Decision_Records/Developer_Tooling_Guide.md) - Development tooling decisions

### 6️⃣ Guides & Workflows ([6_Guides/](6_Guides/))
- [Git_Workflow_Guide.md](6_Guides/Git_Workflow_Guide.md) - 🚨 **MANDATORY** Git branch workflow - NEVER work on main ⭐⭐⭐
- [Comprehensive_Development_Workflow.md](6_Guides/Comprehensive_Development_Workflow.md) - **MUST FOLLOW** TDD+VSA process
- [Quick_Reference_Development_Checklist.md](6_Guides/Quick_Reference_Development_Checklist.md) - Step-by-step checklists
- [Pull_Request_Guide.md](6_Guides/Pull_Request_Guide.md) - **CRITICAL**: PR template compliance to prevent CI failures ⭐⭐⭐
- [Feature_Development_Guide.md](6_Guides/Feature_Development_Guide.md) - **NEW**: Comprehensive feature implementation guide with examples
- [Debugging_Notification_Pipeline.md](6_Guides/Debugging_Notification_Pipeline.md) - Systematic debugging for broken view updates
- [Developer_Tooling_Guide.md](6_Guides/Developer_Tooling_Guide.md) - Development environment setup and tooling

### 7️⃣ Overview Documents ([7_Overview/](7_Overview/))
- [BlockLife_Comprehensive_Overview.md](7_Overview/BlockLife_Comprehensive_Overview.md) - Complete project overview
- [Current_Implementation_Overview.md](7_Overview/Current_Implementation_Overview.md) - Current state documentation

## 🔍 Where to Find What

### When implementing a new feature:
1. **🚨 FIRST**: Create feature branch following [Git_Workflow_Guide.md](6_Guides/Git_Workflow_Guide.md)
2. Check [3_Implementation_Plans/](3_Implementation_Plans/) for existing plan
3. Read [Feature_Development_Guide.md](6_Guides/Feature_Development_Guide.md) for comprehensive implementation patterns
4. Read [Comprehensive_Development_Workflow.md](6_Guides/Comprehensive_Development_Workflow.md) for TDD+VSA process
5. Use [Quick_Reference_Development_Checklist.md](6_Guides/Quick_Reference_Development_Checklist.md)
6. Reference `src/Features/Block/Move/` as pattern example
7. **🚨 LAST**: Create PR following [Pull_Request_Guide.md](6_Guides/Pull_Request_Guide.md)

### When fixing a bug:
1. Check [Master_Action_Items.md](0_Global_Tracker/Master_Action_Items.md) for related prevention measures
2. Check [4_Post_Mortems/](4_Post_Mortems/) for similar issues (especially [Block_Placement_Display_Bug_Report.md](4_Post_Mortems/Block_Placement_Display_Bug_Report.md) for notification pipeline issues)
3. Review [Architecture_FAQ.md](1_Architecture/Architecture_FAQ.md) for known problems and patterns
4. Follow debugging strategies in workflow guide

### When writing tests:
1. Check `tests/Architecture/ArchitectureFitnessTests.cs` for constraints
2. Reference `tests/BlockLife.Core.Tests/Features/Block/Move/` for patterns
3. Follow 4-pillar testing strategy in CLAUDE.md

### When updating documentation:
1. Maintain this catalogue when adding new docs
2. Update [Master_Action_Items.md](0_Global_Tracker/Master_Action_Items.md) when implementing prevention measures
3. Update implementation plan status after completing phases
4. Add post-mortems for significant bugs or discoveries

## 🏗️ Project Structure Quick Reference

```
blocklife/
├── src/                          # Core business logic (NO Godot)
│   ├── Core/                     # Foundation (GameStrapper, Infrastructure)
│   └── Features/                 # Feature slices by domain
│       └── Block/               
│           ├── Commands/         # CQRS commands
│           ├── Effects/          # Effects and notifications
│           ├── Presenters/       # MVP presenters
│           ├── Rules/            # Business rules
│           └── Move/            # Move-specific interfaces
├── tests/                        # Comprehensive test suite
│   └── BlockLife.Core.Tests/
│       ├── Architecture/         # Architecture fitness tests
│       └── Features/            # Feature tests matching src structure
├── godot_project/               # Godot-specific implementation
│   ├── features/                # Godot scenes and views
│   ├── infrastructure/          # Logging and debug systems
│   └── scenes/                  # Main scenes
└── Docs/                        # All documentation
```

## 🚀 Quick Commands

### Build & Test
```bash
dotnet build BlockLife.sln
dotnet test tests/BlockLife.Core.Tests.csproj
dotnet test --filter "FullyQualifiedName~Architecture"  # Architecture tests only
dotnet test --filter "Category=Unit"                    # Unit tests only
```

### Godot Testing
```bash
set GODOT_BIN=C:\path\to\godot.exe
addons\gdUnit4\runtest.cmd
```

## 📊 Current Statistics
- **Total Tests**: 65 (as of 2025-08-13)
- **Architecture Tests**: 16
- **Unit Tests**: 40
- **Property Tests**: 9 (900 validations)
- **Test Coverage Target**: >90% for core logic

## 🔄 Maintenance Notes

**Last Updated**: 2025-08-13
**Maintained By**: Development Team
**Update Frequency**: After each feature completion or major documentation change

### When to Update This Catalogue:
- New documentation added
- Documentation moved or renamed
- New patterns or practices established
- Significant architectural changes
- New tools or workflows introduced

## 📝 Agent Instructions

When using this catalogue:
1. Always check for the most recent version
2. Follow links to get detailed information
3. Update this catalogue when you create new documentation
4. Report inconsistencies or missing information
5. Use the reference implementation (Move Block) as your guide

## 🐛 Known Issues & Workarounds

### Notification Pipeline Issues
See [Block_Placement_Display_Bug_Report.md](4_Post_Mortems/Block_Placement_Display_Bug_Report.md) for:
- Notification pipeline debugging steps (Command → Handler → MediatR → Static Bridge → Presenter)
- Common anti-patterns (empty handlers, effect queues without processing)
- Standard notification flow patterns and troubleshooting

### General Implementation Issues  
See [F1_Block_Placement_Implementation_Issues_Report.md](4_Post_Mortems/F1_Block_Placement_Implementation_Issues_Report.md) for:
- LanguageExt.Fin<T> type ambiguity solutions
- Presenter DI registration patterns
- Error message format standards

### Architecture Decision Records
See [ADR_006_Fin_Task_Consistency.md](5_Architecture_Decision_Records/ADR_006_Fin_Task_Consistency.md) for:
- **ACTIVE DECISION**: Fin<T> vs Task<T> consistency in async operations
- Functional vs imperative error handling patterns
- Migration strategy from mixed approach to pure functional architecture
- Validation rule patterns

## 🔗 External Resources

- **Godot 4.4 Documentation**: https://docs.godotengine.org/
- **LanguageExt Documentation**: https://github.com/louthy/language-ext
- **MediatR Documentation**: https://github.com/jbogard/MediatR
- **Clean Architecture**: Uncle Bob's Clean Architecture principles