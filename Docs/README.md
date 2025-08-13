# BlockLife Documentation Index

Welcome to the BlockLife project documentation. This documentation follows a structured approach to help developers understand, implement, and maintain the codebase effectively.

## 📋 Quick Navigation

### 🎯 For New Developers
1. **Start Here**: [Current Implementation Overview](0_Overview/Current_Implementation_Overview.md) - What's working right now
2. **Architecture**: [Architecture Guide](1_Architecture/Architecture_Guide.md) - Core patterns and principles
3. **Implementation**: [F1 Block Placement](3_Implementation_Plan/1_F1_Block_Placement_Implementation_Plan.md) - Reference implementation

### 🚀 For Active Development
1. **Next Feature**: [Move Block Implementation](3_Implementation_Plan/2_Move_Block_Feature_Implementation_Plan.md)
2. **Testing**: [Test Guide](1_Architecture/Test_Guide.md) - Testing strategies and patterns
3. **Troubleshooting**: [Bug Post-Mortem](4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md) - Lessons learned

## 📁 Documentation Structure

### [0_Overview/](0_Overview/) - Project Status & Summaries
- **[Current Implementation Overview](0_Overview/Current_Implementation_Overview.md)** - What's implemented and how to use it
- **[BlockLife Comprehensive Overview](0_Overview/BlockLife_Comprehensive_Overview.md)** - Complete project vision

### [1_Architecture/](1_Architecture/) - Technical Architecture & Guidelines
- **[Architecture Guide](1_Architecture/Architecture_Guide.md)** 📘 - **MUST READ** - Core architectural principles and patterns
- **[Architecture FAQ](1_Architecture/Architecture_FAQ.md)** - Common questions and answers
- **[Style Guide](1_Architecture/Style_Guide.md)** - Code style and naming conventions
- **[Test Guide](1_Architecture/Test_Guide.md)** - Testing strategies and best practices
- **[Mutation Testing Strategy](1_Architecture/Mutation_Testing_Strategy.md)** - Advanced testing techniques
- **[Property-Based Testing Guide](1_Architecture/Property_Based_Testing_Guide.md)** - Property-based testing patterns

### [2_Game_Design/](2_Game_Design/) - Game Mechanics & Design
- **[Game Design Overview](2_Game_Design/Game_Design_Overview.md)** - Core game mechanics and vision
- **[Brainstorming Archive](2_Game_Design/Brainstorming_Archive.md)** - Historical design discussions
- **[Core_Mechanics/](2_Game_Design/Core_Mechanics/)** - Detailed mechanic specifications
  - [Life Stages](2_Game_Design/Core_Mechanics/01_Life_Stages.md)
  - [Personality System](2_Game_Design/Core_Mechanics/02_Personality_System.md)
  - [Luck System](2_Game_Design/Core_Mechanics/03_Luck_System.md)
  - [Block Narratives](2_Game_Design/Core_Mechanics/04_Block_Narratives.md)

### [3_Implementation_Plan/](3_Implementation_Plan/) - Implementation Roadmaps
- **[0_Vertical Slice Architecture Plan](3_Implementation_Plan/0_Vertical_Slice_Architecture_Plan.md)** - Overall implementation strategy
- **[1_F1 Block Placement](3_Implementation_Plan/1_F1_Block_Placement_Implementation_Plan.md)** ✅ **COMPLETED** - Reference implementation
- **[2_Move Block Feature](3_Implementation_Plan/2_Move_Block_Feature_Implementation_Plan.md)** 🚧 **NEXT** - Drag & drop functionality
- **[3_Animation System](3_Implementation_Plan/3_Animation_System_Implementation_Plan.md)** - Animation infrastructure
- **[5_Dotnet Templates](3_Implementation_Plan/5_Dotnet_New_Templates_Implementation_Plan.md)** - Developer tooling

### [4_Architecture_Decision_Records/](4_Architecture_Decision_Records/) - Decisions & Lessons Learned
- **[Bug Post-Mortem F1](4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md)** 🔍 **IMPORTANT** - Lessons from F1 implementation
- **[Bug Post-Mortem Template](4_Architecture_Decision_Records/Bug_Post_Mortem_Template.md)** - Template for future post-mortems
- **[Complex Rule Engine Architecture](4_Architecture_Decision_Records/Complex_Rule_Engine_Architecture.md)** - Rule system design decisions
- **[Developer Tooling Guide](4_Architecture_Decision_Records/Developer_Tooling_Guide.md)** - Development environment setup

### [5_Archive/](5_Archive/) - Historical Documents
- Deprecated or superseded documentation

## 🎯 Implementation Status

### ✅ Completed Features
- **F1 Block Placement Vertical Slice** (2025-08-13)
  - Core domain models (Block, BlockType, Vector2Int)
  - GridStateService for state management
  - PlaceBlockCommand/Handler and RemoveBlockCommand/Handler
  - Validation rules and complete test suite (30 tests passing)
  - GridPresenter and IGridView interface

### 🚧 In Progress
- Documentation reorganization and updates

### 📋 Next Priorities
1. **F2 Move Block Feature** - Drag & drop functionality
2. **Animation System Refinement** - Based on F1 learnings
3. **Developer Tooling** - Templates and CLI tools

## 🏗️ Architecture Quick Reference

### Core Principles
1. **Clean Architecture**: Pure C# core (`src/`) with NO Godot dependencies
2. **CQRS Pattern**: Commands for writes, Queries for reads
3. **MVP Pattern**: Presenters coordinate between Model and View
4. **Functional Programming**: Use `Fin<T>` and `Option<T>` for safety
5. **Dependency Injection**: Constructor injection throughout

### Key Files to Understand
1. **[Architecture Guide](1_Architecture/Architecture_Guide.md)** - Core patterns (MUST READ)
2. **[F1 Implementation](3_Implementation_Plan/1_F1_Block_Placement_Implementation_Plan.md)** - Reference example
3. **[Bug Post-Mortem](4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md)** - What NOT to do

### Project Structure
```
BlockLife/
├── src/                    # Pure C# Core (Model Layer)
├── godot_project/          # Godot Presentation Layer  
├── tests/                  # Unit Tests
├── Docs/                   # This documentation
└── *.csproj, *.sln        # Build configuration
```

## 🚀 Getting Started

### For New Team Members
1. Read [Current Implementation Overview](0_Overview/Current_Implementation_Overview.md)
2. Study [Architecture Guide](1_Architecture/Architecture_Guide.md)
3. Examine [F1 Implementation](3_Implementation_Plan/1_F1_Block_Placement_Implementation_Plan.md) as reference
4. Review [Bug Post-Mortem](4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md) to avoid common mistakes

### For Implementation Work
1. Check current implementation status in [Overview](0_Overview/Current_Implementation_Overview.md)
2. Follow patterns established in F1 implementation
3. Review relevant implementation plan before starting
4. Write tests first, following established patterns

### For Architecture Questions
1. Check [Architecture FAQ](1_Architecture/Architecture_FAQ.md)
2. Review [Architecture Guide](1_Architecture/Architecture_Guide.md)
3. Examine existing implementations for patterns
4. Document decisions in Architecture Decision Records

## 📝 Documentation Guidelines

### When to Update Documentation
- ✅ After completing any implementation
- ✅ When discovering bugs or issues  
- ✅ When making architectural decisions
- ✅ When adding new patterns or conventions

### How to Update Documentation
1. Keep implementation status current
2. Add post-mortems for significant bugs
3. Update architecture guides with lessons learned
4. Maintain cross-references between documents

## 🔍 Finding Information

| I need to... | Look in... |
|---------------|------------|
| Understand what's currently working | [Current Implementation Overview](0_Overview/Current_Implementation_Overview.md) |
| Learn the architecture patterns | [Architecture Guide](1_Architecture/Architecture_Guide.md) |
| See how to implement a feature | [F1 Implementation Plan](3_Implementation_Plan/1_F1_Block_Placement_Implementation_Plan.md) |
| Understand test strategies | [Test Guide](1_Architecture/Test_Guide.md) |
| Avoid known issues | [Bug Post-Mortem](4_Architecture_Decision_Records/Bug_Post_Mortem_F1_Implementation.md) |
| Plan next implementation | [Move Block Plan](3_Implementation_Plan/2_Move_Block_Feature_Implementation_Plan.md) |
| Understand game mechanics | [Game Design Overview](2_Game_Design/Game_Design_Overview.md) |

---

**Last Updated**: 2025-08-13  
**Documentation Version**: 2.0  
**Project Status**: F1 Block Placement Complete, F2 Move Block In Planning