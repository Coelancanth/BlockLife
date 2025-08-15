# Agent Guides

This directory contains usage guides, workflows, and best practices for each Claude Code agent in the BlockLife project.

## Available Agent Guides

### 📋 [Agile Product Owner](./agile-product-owner/workflow.md)
**Purpose**: Break down features into user stories with vertical slices
- Feature decomposition into VSA slices
- User story creation with BDD acceptance criteria
- Backlog prioritization
- Test-driven development planning

### 🏗️ Implementation Planner
**Purpose**: Create detailed implementation plans following BlockLife architecture
- *Guide coming soon*

### 👀 Code Reviewer
**Purpose**: Review code against BlockLife standards and patterns
- *Guide coming soon*

### 🎯 Tech Lead Advisor
**Purpose**: Provide architectural guidance and create ADRs
- *Guide coming soon*

## Quick Start

1. **Choose the right agent** based on your task
2. **Read the workflow guide** in the agent's folder
3. **Use example prompts** from the templates
4. **Follow BlockLife patterns** (reference Move Block implementation)

## Agent Selection Guide

| Task | Recommended Agent |
|------|------------------|
| Breaking down a complex feature | Agile Product Owner |
| Creating implementation plan | Implementation Planner |
| Reviewing code changes | Code Reviewer |
| Architecture decisions | Tech Lead Advisor |
| Searching codebase | General Purpose |

## Best Practices

1. **Always create feature branches first** before using any agent
2. **Reference existing patterns** (Move Block is the gold standard)
3. **Include test requirements** in all planning
4. **Follow TDD workflow**: Architecture tests → Red → Green → Refactor
5. **Respect Clean Architecture** boundaries

## Contributing

When adding a new agent guide:
1. Create a folder under `agent-guides/[agent-name]/`
2. Include at minimum: `workflow.md`
3. Add examples from actual usage
4. Update this README with the new guide