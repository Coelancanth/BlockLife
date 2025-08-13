# 📚 BlockLife Documentation Catalogue

## 🗺️ Quick Navigation Guide for AI Agents

This catalogue provides a comprehensive index of all documentation, helping AI agents quickly locate necessary information for implementing features, fixing bugs, and maintaining architectural consistency.

## 🎯 Primary References (Start Here)

### Essential Documents
1. **CLAUDE.md** - Root-level agent instructions and project overview
2. **Docs/5_Guide/Comprehensive_Development_Workflow.md** - MANDATORY workflow to follow
3. **Docs/5_Guide/Quick_Reference_Development_Checklist.md** - Daily task checklist
4. **Docs/1_Architecture/Architecture_Guide.md** - Core architectural principles

### Reference Implementation
- **Move Block Feature** (`src/Features/Block/Move/`) - GOLD STANDARD implementation
- **Tests**: `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Documentation**: `Docs/3_Implementation_Plan/2_Move_Block_Feature_Implementation_Plan.md`

## 📂 Documentation Structure

### 1️⃣ Architecture Documentation (`Docs/1_Architecture/`)
- `Architecture_Guide.md` - Core principles, Clean Architecture, MVP pattern
- `Architecture_FAQ.md` - Frequently asked questions about architecture
- Additional architecture decisions and patterns

### 2️⃣ Design Documents (`Docs/2_Design/`)
- Game design specifications
- Feature requirements
- User stories

### 3️⃣ Implementation Plans (`Docs/3_Implementation_Plan/`)
- `0_Vertical_Slice_Architecture_Plan.md` - VSA patterns and approach
- `1_F1_Block_Placement_Implementation_Plan.md` - ✅ COMPLETED foundation feature
- `2_Move_Block_Feature_Implementation_Plan.md` - 🔄 IN PROGRESS reference implementation
- `3_Animation_System_Implementation_Plan.md` - Animation architecture
- `4_Move_Block_Feature_Implementation_Plan.md` - Detailed move mechanics
- `5_Dotnet_New_Templates_Implementation_Plan.md` - Project templates

### 📝 TODO Lists (`Docs/4_TODO/`)
- `3_Comprehensive_TODO_List.md` - Master task tracking
- Individual feature TODOs

### 5️⃣ Guides & Workflows (`Docs/5_Guide/`)
- `Comprehensive_Development_Workflow.md` - **MUST FOLLOW** TDD+VSA process
- `Quick_Reference_Development_Checklist.md` - Step-by-step checklists

### 4️⃣ Bug Post-Mortems (`Docs/4_Bug_PostMortems/`)
- `001_SceneRoot_Initialization_Order.md` - SceneRoot singleton lifecycle issues
- `002_Architecture_Test_Failures.md` - Test framework and architecture compliance
- `003_DI_Container_Presenter_Registration.md` - Presenter notification handler constraints
- `004_SceneRoot_Autoload_Configuration.md` - Autoload configuration and duplicate instantiation
- `Bug_Post_Mortem_F1_Implementation.md` - Lessons from F1 implementation
- Pattern discoveries and anti-patterns

## 🔍 Where to Find What

### When implementing a new feature:
1. Check `Docs/3_Implementation_Plan/` for existing plan
2. Read `Docs/5_Guide/Comprehensive_Development_Workflow.md`
3. Use `Docs/5_Guide/Quick_Reference_Development_Checklist.md`
4. Reference `src/Features/Block/Move/` as pattern example

### When fixing a bug:
1. Check `Docs/4_Architecture_Decision_Records/` for similar issues
2. Review `Docs/1_Architecture/Architecture_FAQ.md` for known problems
3. Follow debugging strategies in workflow guide

### When writing tests:
1. Check `tests/Architecture/ArchitectureFitnessTests.cs` for constraints
2. Reference `tests/BlockLife.Core.Tests/Features/Block/Move/` for patterns
3. Follow 4-pillar testing strategy in CLAUDE.md

### When updating documentation:
1. Maintain this catalogue when adding new docs
2. Update implementation plan status after completing phases
3. Add post-mortems for significant bugs or discoveries

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

See `Docs/4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md` for:
- LanguageExt.Fin<T> type ambiguity solutions
- Presenter DI registration patterns
- Error message format standards
- Validation rule patterns

## 🔗 External Resources

- **Godot 4.4 Documentation**: https://docs.godotengine.org/
- **LanguageExt Documentation**: https://github.com/louthy/language-ext
- **MediatR Documentation**: https://github.com/jbogard/MediatR
- **Clean Architecture**: Uncle Bob's Clean Architecture principles