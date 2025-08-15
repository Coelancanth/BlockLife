# Shared Documentation Organization

**Last Updated**: 2025-08-15  
**Reorganization**: Complete - New logical structure implemented  

## 📁 Folder Structure

### 🏛️ Core/ - Foundational Documentation
Documents that define the fundamental principles and rarely change.

**📋 ADRs/** - Architectural Decision Records
- `ADR_001_Grid_Scanning_Approach_SUPERSEDED.md`
- `ADR_002_Anchor_Based_Implementation_Guide.md`
- `ADR_006_Fin_Task_Consistency.md`
- `ADR_007_Enhanced_Functional_Validation_Pattern.md`
- `ADR_008_Anchor_Based_Rule_Engine_Architecture.md`

**🏗️ Architecture/** - Core Architectural Principles
- `Architecture_Guide.md` - Master architectural guidance
- `Architecture_FAQ.md` - Common architectural questions
- `Standard_Patterns.md` - Proven architectural patterns

**📐 Style-Standards/** - Code Standards and Conventions
- `Style_Guide.md` - Code style requirements
- `Naming_Conventions.md` - General naming standards
- `Work_Item_Naming_Conventions.md` - Specific work item naming

### ⚙️ Workflows/ - Process Documentation
Documents that define how we work and collaborate.

**🤖 Agent-Patterns/** - AI Agent Workflow Patterns
- `AI_Workflow_overview.md` - AI workflow overview
- `Agent-Architecture-and-Workflow-Patterns.md` - Agent collaboration patterns
- `Agent-Ecosystem-Design.md` - Complete agent ecosystem design
- `Product-Owner-vs-Backlog-Maintainer-Design.md` - Specific agent responsibilities

**👨‍💻 Development/** - Development Process Guides
- `Comprehensive_Development_Workflow.md` - Complete TDD+VSA process
- `Feature_Development_Guide.md` - Feature development approach
- `Quick_Reference_Development_Checklist.md` - Daily workflow checklist
- `Documentation_Update_Workflow.md` - Documentation maintenance

**🌐 Git-And-CI/** - Version Control and CI/CD
- `Git_Workflow_Guide.md` - Git branching and workflow requirements
- `Pull_Request_Guide.md` - PR creation and review process

**🧪 Testing/** - Testing Strategies and Approaches
- `Test_Guide.md` - General testing guidance
- `Integration_Testing_Guide.md` - Integration testing approach
- `Test_Automation_Guide.md` - Test automation strategies
- `Property_Based_Testing_Guide.md` - Property-based testing
- `Mutation_Testing_Strategy.md` - Mutation testing approach

### 🔧 Implementation/ - Implementation Guidance
Specific guidance for implementing features and working with the codebase.

**📋 Reference-Plans/** - Step-by-Step Implementation Plans
- `00_Vertical_Slice_Architecture_Plan.md` - Core VSA implementation
- `01_F1_Block_Placement_Implementation_Plan.md` - F1 feature plan
- `02_Move_Block_Feature_Implementation_Plan.md` - Move block plan

**👨‍💻 Developer-Guides/** - Day-to-Day Development Help
- `Developer_Tooling_Guide.md` - Development tools and setup
- `Debugging_Notification_Pipeline.md` - Specific debugging procedures

**🔌 Integration-Guides/** - Technology Integration Help
- `GdUnit4_Integration_Testing_Guide.md` - GdUnit4 setup and usage

### 📝 Templates/ - Template Files
Centralized location for all template files.

**🐛 Bug-Report-Templates/**
- `TEMPLATE_Bug_Report_And_Fix.md` - Active bug report template
- `TEMPLATE_Bug_Post_Mortem.md` - Historical post-mortem template

**🏗️ Framework-Templates/**
- `Living_Document_Post_Mortem_Framework.md` - Living document framework

**⚙️ Implementation-Templates/**
- (Reserved for future implementation templates)

## 🗺️ Navigation Guide

### "I want to..."

**🔍 Understand the architecture**
→ Start with `Core/Architecture/Architecture_Guide.md`
→ Check `Core/Architecture/Architecture_FAQ.md` for specific questions

**🚀 Implement a new feature**
→ Follow `Workflows/Development/Comprehensive_Development_Workflow.md`
→ Use `Implementation/Reference-Plans/` for step-by-step guidance
→ Reference `Workflows/Development/Quick_Reference_Development_Checklist.md`

**🧪 Set up testing**
→ Start with `Workflows/Testing/Test_Guide.md`
→ For integration tests: `Implementation/Integration-Guides/GdUnit4_Integration_Testing_Guide.md`
→ For specific testing strategies: `Workflows/Testing/` folder

**🤖 Understand agent workflows**
→ Read `Workflows/Agent-Patterns/Agent-Ecosystem-Design.md`
→ Check `Workflows/Agent-Patterns/` for specific patterns

**📝 Report a bug**
→ Use templates in `Templates/Bug-Report-Templates/`
→ Follow the Bug-to-Test protocol

**🔄 Work with Git/CI**
→ `Workflows/Git-And-CI/Git_Workflow_Guide.md` - MANDATORY reading
→ `Workflows/Git-And-CI/Pull_Request_Guide.md` for PR process

**🎨 Check code standards**
→ `Core/Style-Standards/Style_Guide.md` for coding standards
→ `Core/Style-Standards/Naming_Conventions.md` for naming

## 🔄 Migration Notes

This reorganization was completed on 2025-08-15 to improve documentation discoverability and logical grouping. 

### Key Changes:
- **Separated concerns**: Architecture, Workflows, Implementation, and Templates
- **Centralized templates**: All templates now in one location
- **Logical grouping**: Related documents grouped by purpose, not type
- **Agent-specific content**: Dedicated section for agent workflow patterns
- **Clear navigation**: Purpose-based folder names instead of generic names

### Cross-References
Some documents may still reference old paths. These will be updated as encountered or in a follow-up task.

## 🎯 Benefits of New Structure

1. **Easier Discovery**: Find documents by purpose, not by type
2. **Logical Grouping**: Related documents are co-located
3. **Clearer Separation**: Core principles vs. daily workflows vs. implementation help
4. **Better Maintenance**: Template files centralized for easier updates
5. **Agent Clarity**: Dedicated space for agent-specific documentation

---

**Quick Start**: New to the project? Begin with `Core/Architecture/Architecture_Guide.md` and `Workflows/Development/Comprehensive_Development_Workflow.md`