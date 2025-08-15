# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 📚 IMPORTANT: Documentation Navigation
**FIRST STOP:** Always consult [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for a complete index of all documentation. This catalogue helps you quickly locate:
- Implementation plans and their status
- Bug post-mortems and lessons learned  
- Architecture FAQ and decisions
- Testing strategies and patterns
- Reference implementations

## 🚨 CRITICAL: AUTOMATIC AGENT TRIGGERING IS MANDATORY

### ⚠️ STOP! After ANY development action, you MUST trigger agents

**THE IRON RULE**: Edit/Write/Test/Move/Fix ANYTHING = Trigger backlog-maintainer IMMEDIATELY

**Quick Check - Did you just:**
- Edit a file? → TRIGGER backlog-maintainer NOW
- Run tests? → TRIGGER backlog-maintainer NOW  
- Fix a bug? → TRIGGER backlog-maintainer NOW
- Move/rename files? → TRIGGER backlog-maintainer NOW
- Update documentation? → TRIGGER backlog-maintainer NOW

**If you did work and haven't triggered an agent, STOP and trigger NOW.**

**MUST READ**: [AGENT_ORCHESTRATION_GUIDE.md](Docs/Workflows/Orchestration-System/AGENT_ORCHESTRATION_GUIDE.md) - Full trigger patterns
**VERIFY**: [DOUBLE_VERIFICATION_PROTOCOL.md](Docs/Workflows/Orchestration-System/DOUBLE_VERIFICATION_PROTOCOL.md) - Ensure triggers happen
**FEEDBACK**: [ORCHESTRATION_FEEDBACK_SYSTEM.md](Docs/Workflows/Orchestration-System/ORCHESTRATION_FEEDBACK_SYSTEM.md) - Report failures

## 📋 Backlog - SINGLE SOURCE OF TRUTH
**ALL work tracking happens in:** [Backlog/Backlog.md](Docs/Backlog/Backlog.md)
- **Dynamic Tracker**: Single file tracking all work in real-time
- **Work Item Types**: VS (Vertical Slice), BF (Bug Fix), TD (Tech Debt), HF (Hotfix)
- **Status**: This is the ONLY place for work tracking (0_Global_Tracker is DEPRECATED)
- **Naming Convention**: [Work_Item_Naming_Conventions.md](Docs/Shared/Core/Style-Standards/Work_Item_Naming_Conventions.md)
- **Maintained by**: Product Owner agent (automatically triggered after EVERY development action)

## 🤖 Complete Agent Ecosystem

BlockLife employs a comprehensive 11-agent ecosystem designed for solo developer + AI workflows:

### Core Workflow Agents
| Agent | Model | Purpose | Domain Docs |
|-------|-------|---------|-------------|
| `product-owner` | Opus | User stories, backlog prioritization, acceptance criteria | - |
| `backlog-maintainer` | Sonnet | Silent progress tracking, status updates | - |
| `tech-lead` | Opus | Implementation planning, technical decisions | [TechLead/](Docs/Agent-Specific/TechLead/) |

### TDD Workflow Agents  
| Agent | Model | Purpose | Domain Docs |
|-------|-------|---------|-------------|
| `test-designer` | Sonnet | TDD RED phase - create failing tests | - |
| `dev-engineer` | Sonnet | TDD GREEN phase - minimal implementation | - |
| `qa-engineer` | Sonnet | Integration tests, stress testing | [QA/](Docs/Agent-Specific/QA/) |

### Architecture & Maintenance Agents
| Agent | Model | Purpose | Domain Docs |
|-------|-------|---------|-------------|
| `architect` | Opus | System-wide design decisions, ADRs | [Architect/](Docs/Agent-Specific/Architect/) |
| `vsa-refactoring` | Opus | VSA maintenance, code duplication analysis | [VSA/](Docs/Agent-Specific/VSA/) |
| `debugger-expert` | Opus | Complex bug diagnosis, race conditions | - |

### DevOps & Operations Agents
| Agent | Model | Purpose | Domain Docs |
|-------|-------|---------|-------------|
| `git-expert` | Sonnet | Complex Git operations, merge conflicts | [Git/](Docs/Agent-Specific/Git/) |
| `devops-engineer` | Sonnet | CI/CD pipelines, Python automation | [DevOps/](Docs/Agent-Specific/DevOps/) |

### Agent-Driven Development

All agents are automatically triggered by Claude Code following the Automatic Orchestration Pattern. Each agent has:
- **Workflow File**: Detailed procedures in `Docs/Workflows/Agent-Workflows/[agent]-workflow.md`
- **Agent References**: Quick documentation guide in `Docs/Agent-References/[agent]-references.md`
- **Domain Documentation**: Agent-owned content in `Docs/Agent-Specific/[Agent]/`

**Key Principle**: Domain-specific knowledge lives with specialist agents, not in this central file.

## Project Overview

BlockLife is a C# Godot 4.4 game implementing a strict Clean Architecture with Model-View-Presenter (MVP) pattern. The project uses CQRS with functional programming principles (LanguageExt.Core) and maintains a pure C# core separated from Godot-specific presentation code. Development environment is Windows 10, use PowerShell.

**🎯 Reference Implementation**: `src/Features/Block/Move/` serves as the GOLD STANDARD for all feature development.

## ⚠️ CRITICAL: Git Workflow Requirements

**🚫 NEVER WORK DIRECTLY ON MAIN BRANCH - NO EXCEPTIONS**

**MANDATORY Git Workflow for ALL Changes:**
1. **Create feature branch FIRST**: `git checkout -b <type>/<description>`
2. **Make changes on branch**: Never on main
3. **Create Pull Request**: Always for review
4. **Wait for approval**: Before merging
5. **Follow guide**: [Git_Workflow_Guide.md](Docs/Shared/Workflows/Git-And-CI/Git_Workflow_Guide.md)

**Branch Types**: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `hotfix/`

*Detailed Git workflows are managed by the Git Expert agent.*

## 🚀 Quick Start for New Features

**Before implementing ANY feature:**
1. **🔥 CREATE BRANCH**: `git checkout -b feat/your-feature-name`
2. **Check the catalogue**: [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for navigation
3. Check if implementation plan exists: `Docs/Shared/Implementation/Reference-Plans/[FeatureName]_Implementation_Plan.md`
4. Read workflow guide: [Comprehensive_Development_Workflow.md](Docs/Shared/Workflows/Development/Comprehensive_Development_Workflow.md)
5. Use checklist: [Quick_Reference_Development_Checklist.md](Docs/Shared/Workflows/Development/Quick_Reference_Development_Checklist.md)
6. Reference Move Block implementation: `src/Features/Block/Move/` (Gold Standard)

*Detailed build commands and automation are managed by the DevOps Engineer agent.*

## Essential Documentation References

### 📚 **Primary Documents to Consult (In Order)**
1. **[Comprehensive_Development_Workflow.md](Docs/Shared/Guides/Comprehensive_Development_Workflow.md)** - Complete TDD+VSA process
2. **[Quick_Reference_Development_Checklist.md](Docs/Shared/Guides/Quick_Reference_Development_Checklist.md)** - Daily workflow checklists
3. **[Architecture_Guide.md](Docs/Shared/Architecture/Architecture_Guide.md)** - Core architectural principles
4. **`Docs/Shared/Implementation-Plans/`** - Feature-specific implementation plans

### 📋 **Implementation Plans by Feature**
- **Vertical Slice Architecture**: [000_Vertical_Slice_Architecture_Plan.md](Docs/Shared/Implementation-Plans/000_Vertical_Slice_Architecture_Plan.md) - Core VSA patterns
- **Block Placement (F1)**: [001_F1_Block_Placement_Implementation_Plan.md](Docs/Shared/Implementation-Plans/001_F1_Block_Placement_Implementation_Plan.md) - Foundation feature  
- **Move Block**: [002_Move_Block_Feature_Implementation_Plan.md](Docs/Shared/Implementation-Plans/002_Move_Block_Feature_Implementation_Plan.md) ✅ **Phase 1 COMPLETED** (Reference Implementation)
- **Animation System**: [003_Animation_System_Implementation_Plan.md](Docs/Shared/Implementation-Plans/003_Animation_System_Implementation_Plan.md) - Animation queuing and state
- **Dotnet Templates**: [005_Dotnet_New_Templates_Implementation_Plan.md](Docs/Shared/Implementation-Plans/005_Dotnet_New_Templates_Implementation_Plan.md) - Project templates

## 🚨 CRITICAL: AUTOMATIC AGENT TRIGGERING (Automatic Orchestration Pattern)

### ⚠️ STOP! YOU MUST TRIGGER AGENTS AUTOMATICALLY

**THE RULE**: After EVERY development action, you MUST trigger the appropriate agent. NO EXCEPTIONS.

### 🔴 IMMEDIATE TRIGGER CHECKLIST - DO THIS NOW

**After ANY of these actions, IMMEDIATELY trigger backlog-maintainer:**
- ✅ File edits (Edit/Write/MultiEdit tools used)
- ✅ Test runs (passing OR failing)  
- ✅ Documentation changes
- ✅ Code reorganization or refactoring
- ✅ Bug fixes applied
- ✅ Feature implementation progress
- ✅ File/folder reorganization
- ✅ Cross-reference updates

**If you just did ANY development work and haven't triggered an agent, YOU'RE DOING IT WRONG.**

### ⚠️ MANDATORY: READ ORCHESTRATION GUIDE
**FULL DETAILS**: [AGENT_ORCHESTRATION_GUIDE.md](Docs/Workflows/Orchestration-System/AGENT_ORCHESTRATION_GUIDE.md)

**Why Automatic Orchestration?** In solo dev + AI, work that isn't automatically tracked gets lost. The Backlog is the SINGLE SOURCE OF TRUTH - if it's not there, it didn't happen.

### Quick Reference Pattern
After EVERY development action:

**🔴 EARLY STAGE MODE**: Currently announcing all agent triggers for validation:
```
🤖 AGENT TRIGGER: [Reason detected]
   → Invoking [Agent] for [Action]
   → Context: [What's being processed]
```

### 🎯 CONCRETE EXAMPLES - "Should I Have Triggered?"

**YES, trigger backlog-maintainer after:**
- "I just reorganized the Docs/Shared folder" → TRIGGER NOW
- "I fixed that bug in GridStateService.cs" → TRIGGER NOW
- "I updated cross-references in documentation" → TRIGGER NOW
- "Tests are passing now" → TRIGGER NOW
- "I refactored that duplicate code" → TRIGGER NOW

**YES, trigger product-owner when:**
- User says "I want to add..." → TRIGGER NOW
- User reports a bug → TRIGGER NOW
- User asks "Can we implement..." → TRIGGER NOW

**YES, trigger tech-lead when:**
- Starting a new VS (Vertical Slice) → TRIGGER NOW
- Need implementation plan → TRIGGER NOW
- Architecture decision needed → TRIGGER NOW

### Agent Trigger Quick Reference

- **Product Owner**: Feature requests, bug reports, acceptance reviews (visible decisions)
- **Backlog Maintainer**: ALL code/doc changes, test results, progress (MUST trigger after EVERY edit)
- **Tech Lead**: VS items, technical decisions, implementation planning (visible planning)
- **Specialist Agents**: TDD phases, architecture decisions, debugging, Git operations, automation

**REMEMBER**: If you edited ANYTHING and didn't trigger backlog-maintainer, you failed the workflow.

### ❌ WORKFLOW FAILURES - DON'T DO THIS

**WRONG**: "I reorganized files, updated links, created README" → No agent triggered
**RIGHT**: Each action triggers backlog-maintainer for tracking

**WRONG**: "I'll update the backlog later after I finish everything"
**RIGHT**: Trigger immediately after EACH development action

**WRONG**: "This is minor work, doesn't need tracking"
**RIGHT**: ALL work gets tracked, no exceptions

**WRONG**: "I can handle cross-reference updates myself"
**RIGHT**: Delegate link maintenance to backlog-maintainer (it's their specialty)

### Development Workflow Integration

The agent ecosystem integrates seamlessly with the established development workflow:

1. **Git Workflow**: Always create feature branches - managed by Git Expert agent
2. **Documentation First**: Check implementation plans, comprehensive workflow, and quick reference guides
3. **TDD Cycle**: Architecture tests → RED → GREEN → REFACTOR with automatic agent triggering
4. **Quality Gates**: Full test suite validation with agent-assisted quality assurance
5. **Pull Requests**: Use established PR template - managed by Git Expert agent

The Automatic Orchestration Pattern ensures all development actions automatically update the Backlog as the Single Source of Truth.

## 🔍 Common Development Questions - Agent Delegation

### "How do I build and run tests?"
→ **DevOps Engineer** manages: [Agent-Specific/DevOps/build-commands.md](Docs/Agent-Specific/DevOps/build-commands.md)

### "What's the architecture and patterns I should follow?"
→ **Architect** manages: [Agent-Specific/Architect/core-architecture.md](Docs/Agent-Specific/Architect/core-architecture.md)

### "How do I write and run integration tests?"
→ **QA Engineer** manages: [Agent-Specific/QA/integration-testing.md](Docs/Agent-Specific/QA/integration-testing.md)

### "How do I handle Git workflows and PRs?"
→ **Git Expert** manages: [Agent-Specific/Git/workflow-requirements.md](Docs/Agent-Specific/Git/workflow-requirements.md)

### "How should I organize code and handle duplication?"
→ **VSA Refactoring** manages: [Agent-Specific/VSA/organization-patterns.md](Docs/Agent-Specific/VSA/organization-patterns.md)

### "I found a bug! What's the process?"
**🚨 MANDATORY Bug-to-Test Protocol (NO EXCEPTIONS):**
1. **Document**: Create bug report using [TEMPLATE_Bug_Report_And_Fix.md](Docs/Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md)
2. **Reproduce**: Verify bug exists and document exact reproduction steps
3. **Test First**: Write failing regression test that would have caught this bug
4. **Fix**: Implement minimal fix to make the test pass
5. **Validate**: Ensure all tests pass and bug is actually resolved
6. **Learn**: Document lessons learned and prevention strategies

**Key Principle**: **Every bug becomes a permanent test** - this ensures issues never reoccur and tests serve as living documentation.

---

## 📝 Notes on Agent Delegation

This streamlined CLAUDE.md focuses on:
- **Coordination**: Orchestration patterns and agent triggering
- **Navigation**: How to find the right documentation and agents
- **Standards**: Git workflow, bug protocols, and core principles
- **Delegation**: Clear references to agent-owned domain knowledge

**Domain-specific knowledge now lives with specialist agents** to reduce cognitive load and improve maintainability.