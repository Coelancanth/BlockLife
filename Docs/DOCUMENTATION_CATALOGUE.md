# 📚 BlockLife Documentation Catalogue

## 🗺️ Quick Navigation Guide for AI Agents

This catalogue provides a comprehensive index of all documentation, helping AI agents quickly locate necessary information for implementing features, fixing bugs, and maintaining architectural consistency.

## 🎯 Primary References (Start Here)

### Essential Documents
1. **CLAUDE.md** - Root-level agent instructions and project overview
2. **[Git_Workflow_Guide.md](Shared/Workflows/Git-And-CI/Git_Workflow_Guide.md)** - 🚨 **MANDATORY** Git branch workflow (READ FIRST)
3. **[Comprehensive_Development_Workflow.md](Shared/Workflows/Development/Comprehensive_Development_Workflow.md)** - MANDATORY TDD+VSA workflow to follow
4. **[Quick_Reference_Development_Checklist.md](Shared/Workflows/Development/Quick_Reference_Development_Checklist.md)** - Daily task checklist
5. **[Architecture_Guide.md](Shared/Core/Architecture/Architecture_Guide.md)** - Core architectural principles

### Reference Implementation
- **Move Block Feature** (`src/Features/Block/Move/`) - GOLD STANDARD implementation
- **Tests**: `tests/BlockLife.Core.Tests/Features/Block/Move/`
- **Documentation**: [002_Move_Block_Feature_Implementation_Plan.md](Shared/Implementation/Reference-Plans/002_Move_Block_Feature_Implementation_Plan.md)

## 🤖 Agent Ecosystem Documentation

### Agent-Specific Documentation
- **[Agent-Specific/Architect/](Agent-Specific/Architect/)** - Architecture principles and patterns owned by Architect agent
- **[Agent-Specific/DevOps/](Agent-Specific/DevOps/)** - Build commands and automation owned by DevOps Engineer agent
- **[Agent-Specific/Git/](Agent-Specific/Git/)** - Git workflows and PR requirements owned by Git Expert agent
- **[Agent-Specific/QA/](Agent-Specific/QA/)** - Testing strategies owned by QA Engineer agent
- **[Agent-Specific/VSA/](Agent-Specific/VSA/)** - VSA organization patterns owned by VSA Refactoring agent

### Agent Reference Files
- **[Agent-References/architect-references.md](Agent-References/architect-references.md)** - Documentation guide for Architect agent
- **[Agent-References/debugger-expert-references.md](Agent-References/debugger-expert-references.md)** - Documentation guide for Debugger Expert agent
- **[Agent-References/dev-engineer-references.md](Agent-References/dev-engineer-references.md)** - Documentation guide for Dev Engineer agent
- **[Agent-References/devops-references.md](Agent-References/devops-references.md)** - Documentation guide for DevOps Engineer agent
- **[Agent-References/git-references.md](Agent-References/git-references.md)** - Documentation guide for Git Expert agent
- **[Agent-References/product-owner-references.md](Agent-References/product-owner-references.md)** - Documentation guide for Product Owner agent
- **[Agent-References/qa-references.md](Agent-References/qa-references.md)** - Documentation guide for QA Engineer agent
- **[Agent-References/tech-lead-references.md](Agent-References/tech-lead-references.md)** - Documentation guide for Tech Lead agent
- **[Agent-References/test-designer-references.md](Agent-References/test-designer-references.md)** - Documentation guide for Test Designer agent
- **[Agent-References/vsa-refactoring-references.md](Agent-References/vsa-refactoring-references.md)** - Documentation guide for VSA Refactoring agent

### Agent Workflows
- **[Workflows/](Workflows/)** - All agent workflow files and orchestration guides
- **[Workflows/Orchestration-System/AGENT_ORCHESTRATION_GUIDE.md](Workflows/Orchestration-System/AGENT_ORCHESTRATION_GUIDE.md)** - Master orchestration documentation

## 📂 Shared Documentation Structure

### 📋 Product Backlog ([Backlog/](Backlog/)) - **SINGLE SOURCE OF TRUTH**
- [Backlog.md](Backlog/Backlog.md) - **ACTIVE** work tracking, session planning, solo dev workflow ⭐⭐⭐⭐⭐
- All work items (VS, HF, TD, BF) tracked here
- This is the **ONLY** place for work tracking

### 🏗️ Architecture Documentation ([Shared/Core/Architecture/](Shared/Core/Architecture/))
- [Architecture_Guide.md](Shared/Core/Architecture/Architecture_Guide.md) - Core principles, Clean Architecture, MVP pattern ⭐⭐⭐
- [Standard_Patterns.md](Shared/Core/Architecture/Standard_Patterns.md) - **VALIDATED** architectural patterns ⭐⭐⭐
- [Architecture_FAQ.md](Shared/Core/Architecture/Architecture_FAQ.md) - Frequently asked questions ⭐⭐
- [Integration_Testing_Guide.md](Shared/Core/Architecture/Integration_Testing_Guide.md) - GdUnit4 testing architecture
- [Test_Guide.md](Shared/Core/Architecture/Test_Guide.md) - Four-pillar testing strategy
- [Property_Based_Testing_Guide.md](Shared/Core/Architecture/Property_Based_Testing_Guide.md) - FsCheck patterns

### 📝 Implementation Planning (Embedded in VS Items)
- **Current Approach**: Implementation planning embedded within VS items via `Docs/Backlog/templates/VS_Template.md`
- **Tech Lead Section**: Each VS item contains "🏗️ Implementation Plan" section owned by Tech Lead
- **Archived Reference**: [Shared/Implementation/Reference-Plans/](Shared/Implementation/Reference-Plans/) - Historical implementation plans for reference
  - [000_Vertical_Slice_Architecture_Plan.md](Shared/Implementation/Reference-Plans/000_Vertical_Slice_Architecture_Plan.md) - VSA patterns (archived)
  - [001_F1_Block_Placement_Implementation_Plan.md](Shared/Implementation/Reference-Plans/001_F1_Block_Placement_Implementation_Plan.md) - ✅ **PRODUCTION READY** (Reference)
  - [002_Move_Block_Feature_Implementation_Plan.md](Shared/Implementation/Reference-Plans/002_Move_Block_Feature_Implementation_Plan.md) - ✅ **COMPLETED** (Reference)
- **Live Planning**: Check `Docs/Backlog/items/VS_*.md` files for current embedded implementation plans

### 🧠 Living Wisdom System ([Living-Wisdom/](Living-Wisdom/)) - **STRATEGIC KNOWLEDGE BASE** ⭐⭐⭐⭐⭐
- [index.md](Living-Wisdom/index.md) - **MASTER INDEX** to all living documents
- **Playbooks**: [LWP_001_Stress_Testing_Playbook.md](Living-Wisdom/Playbooks/LWP_001_Stress_Testing_Playbook.md), [LWP_002_Integration_Testing_Patterns.md](Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md), [LWP_004_Production_Readiness_Checklist.md](Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)
- **Troubleshooting**: [LWT_001_Notification_Pipeline_Debugging.md](Living-Wisdom/Troubleshooting/LWT_001_Notification_Pipeline_Debugging.md)

### 🔍 Incident Reports ([Incident-Reports/](Incident-Reports/))
- [TEMPLATE_Bug_Report_And_Fix.md](Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md) - Bug-to-test protocol template ⭐⭐⭐
- **Active/**: Current incidents under investigation
- **Resolved/**: Completed incident reports (archived by date)
- **Archive/**: Historical incidents (organized by quarter)

### 🏛️ Architecture Decision Records ([Shared/Core/ADRs/](Shared/Core/ADRs/))
- [ADR_007_Enhanced_Functional_Validation_Pattern.md](Shared/Core/ADRs/ADR_007_Enhanced_Functional_Validation_Pattern.md) - Functional validation patterns
- [ADR_008_Anchor_Based_Rule_Engine_Architecture.md](Shared/Core/ADRs/ADR_008_Anchor_Based_Rule_Engine_Architecture.md) - Rule engine architecture

### 📖 Development Guides ([Shared/Guides/](Shared/Guides/))
- [Comprehensive_Development_Workflow.md](Shared/Workflows/Development/Comprehensive_Development_Workflow.md) - **MANDATORY** TDD+VSA workflow ⭐⭐⭐⭐⭐
- [Quick_Reference_Development_Checklist.md](Shared/Workflows/Development/Quick_Reference_Development_Checklist.md) - Daily checklist ⭐⭐⭐
- [Git_Workflow_Guide.md](Shared/Workflows/Git-And-CI/Git_Workflow_Guide.md) - **MANDATORY** Git workflow ⭐⭐⭐⭐
- [GdUnit4_Integration_Testing_Guide.md](Shared/Guides/GdUnit4_Integration_Testing_Guide.md) - Godot integration testing
- [Debugging_Notification_Pipeline.md](Shared/Guides/Debugging_Notification_Pipeline.md) - Notification debugging guide
- [Work_Item_Naming_Conventions.md](Shared/Core/Style-Standards/Work_Item_Naming_Conventions.md) - Backlog item naming

## 🎯 Quick Reference by Role

### For New Features
1. Check [Backlog.md](Backlog/Backlog.md) for current priorities
2. Read [Comprehensive_Development_Workflow.md](Shared/Workflows/Development/Comprehensive_Development_Workflow.md)
3. Create feature branch using [Git_Workflow_Guide.md](Shared/Workflows/Git-And-CI/Git_Workflow_Guide.md)
4. Use embedded planning: Tech Lead adds "🏗️ Implementation Plan" section to VS item
5. Follow Move Block reference implementation pattern
6. Use [Quick_Reference_Development_Checklist.md](Shared/Workflows/Development/Quick_Reference_Development_Checklist.md)

### For Bug Fixes
1. Use [TEMPLATE_Bug_Report_And_Fix.md](Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md)
2. Check [Debugging_Notification_Pipeline.md](Shared/Guides/Debugging_Notification_Pipeline.md) for common issues
3. Review relevant post-mortems for similar issues
4. Follow bug-to-test protocol (mandatory)

### For Architecture Questions
1. Start with [Architecture_Guide.md](Shared/Core/Architecture/Architecture_Guide.md)
2. Check [Architecture_FAQ.md](Shared/Core/Architecture/Architecture_FAQ.md)
3. Review [Standard_Patterns.md](Shared/Core/Architecture/Standard_Patterns.md)
4. Consult [Agent-Specific/Architect/](Agent-Specific/Architect/) for detailed patterns

### For Testing Questions
1. Review [Test_Guide.md](Shared/Core/Architecture/Test_Guide.md) for four-pillar strategy
2. Check [Integration_Testing_Guide.md](Shared/Core/Architecture/Integration_Testing_Guide.md) for GdUnit4
3. Use [Agent-Specific/QA/](Agent-Specific/QA/) for testing strategies

## 📊 Documentation Quality Ratings

- ⭐⭐⭐⭐⭐ **CRITICAL** - Must read, production-validated
- ⭐⭐⭐⭐ **HIGH** - Important for compliance and quality
- ⭐⭐⭐ **MEDIUM** - Useful reference, well-maintained
- ⭐⭐ **LOW** - Helpful but not essential
- ⭐ **REFERENCE** - Occasional reference only

## 🔄 Recent Changes

- **MAJOR**: Reorganized documentation structure with agent-specific ownership
- **NEW**: Agent reference files for quick documentation discovery
- **UPDATED**: All paths updated to new Shared/ structure
- **DEPRECATED**: Old numbered folder structure (1_, 2_, etc.) replaced with semantic names