# Work Item Naming Conventions

## Overview
This document defines the naming conventions for all work items in the BlockLife project. These conventions ensure consistency, traceability, and clear communication across the team.

## Work Item Types

### Primary Work Items (Product Backlog)

| Prefix | Type | Description | Example |
|--------|------|-------------|---------|
| **VS_** | Vertical Slice | New feature or user story | `VS_001_Block_Rotation.md` |
| **BF_** | Bug Fix | Defect repair | `BF_002_Grid_Boundary_Fix.md` |
| **TD_** | Tech Debt | Refactoring or cleanup | `TD_003_Notification_Refactor.md` |
| **HF_** | Hotfix | Critical production issue | `HF_001_Critical_State_Fix.md` |

### Documentation Types

| Prefix | Type | Location | Example |
|--------|------|----------|---------|
| **ADR_** | Architecture Decision Record | `Docs/5_Architecture_Decision_Records/` | `ADR_009_Backlog_Unification.md` |
| **FB_** | Feature Breakdown | `Docs/Product_Backlog/` | `FB_001_Multiplayer_Epic.md` |

### Deprecated Prefixes

| Prefix | Type | Status | Migration |
|--------|------|--------|-----------|
| **US_** | User Story | Deprecated | Use **VS_** instead |

## Naming Pattern

### Standard Format
```
[Type]_[Number]_[Domain]_[Description].md
```

### Components

1. **Type**: 2-3 letter prefix from the table above
2. **Number**: 
   - `XXX` for unprioritized items in backlog
   - `001-999` for prioritized items
3. **Domain**: System or feature area (e.g., Block, Inventory, Grid)
4. **Description**: Brief, descriptive name (2-4 words)

### Examples
```
VS_001_Block_Rotation.md           # Prioritized vertical slice
VS_XXX_Multiplayer_Init.md         # Unprioritized backlog item
BF_002_Grid_Boundary_Validation.md # Bug fix
TD_003_Command_Pipeline_Refactor.md # Tech debt
HF_001_Critical_Memory_Leak.md     # Hotfix
```

## File Organization

### Product Backlog Structure
```
Docs/Product_Backlog/
├── README.md                    # Sprint dashboard
├── active/                      # Currently in development
│   ├── VS_001_Block_Rotation.md
│   └── BF_001_State_Fix.md
├── ready/                       # Prioritized for next sprint
│   ├── VS_002_Inventory_Basic.md
│   └── TD_001_Logging_Cleanup.md
├── backlog/                     # Unprioritized items (XXX)
│   ├── VS_XXX_Crafting_System.md
│   └── TD_XXX_Performance_Opt.md
└── completed/                   # Archived work
    └── 2024-Q1/
        └── VS_000_Block_Move.md
```

## Lifecycle Rules

### Creation
1. New items created with `XXX` suffix in `backlog/`
2. Created by Product Owner agent or designated role

### Prioritization
1. When prioritized, assign sequential number (001, 002, etc.)
2. Move from `backlog/` to `ready/`
3. Update README.md with priority rationale

### Sprint Start
1. Move items from `ready/` to `active/`
2. Keep original number throughout lifecycle

### Completion
1. Move from `active/` to `completed/[YYYY-QN]/`
2. Archive quarterly to prevent folder bloat

## Work Item Ownership

| Item Type | Created By | Prioritized By | Implemented By |
|-----------|------------|----------------|----------------|
| VS (Vertical Slice) | Product Owner | Product Owner | Development Team |
| BF (Bug Fix) | Tester/Developer | Product Owner | Development Team |
| TD (Tech Debt) | Tech Lead/Developer | Tech Lead + PO | Development Team |
| HF (Hotfix) | Anyone | Immediate | On-call Developer |

## Integration with Other Systems

### From Other Agents
- **Implementation Planner** creates plans → PO creates VS_XXX
- **Architecture Stress Tester** finds bugs → PO creates BF_XXX
- **Tech Lead Advisor** identifies debt → PO creates TD_XXX
- **Code Reviewer** finds issues → PO creates BF_XXX or TD_XXX

### To Development
- Work items in `active/` → Implementation begins
- Each item links to:
  - Source documents (bug reports, ADRs)
  - Implementation plans
  - Pull requests
  - Test results

## Anti-Patterns to Avoid

### ❌ DON'T
- Create duplicate items for same work
- Use inconsistent naming patterns
- Skip the XXX → numbered progression
- Keep items in active/ after completion
- Create items without clear acceptance criteria

### ✅ DO
- Maintain single source of truth in Product_Backlog/
- Follow naming conventions strictly
- Archive completed work quarterly
- Link related documents
- Keep README.md current

## Quick Reference

### Creating a New Feature
```bash
1. Create: VS_XXX_Feature_Name.md in backlog/
2. Prioritize: Rename to VS_001_Feature_Name.md
3. Move: backlog/ → ready/ → active/ → completed/
```

### Reporting a Bug
```bash
1. Create: BF_XXX_Bug_Description.md in backlog/
2. Assess severity
3. If critical: Rename to HF_001 and fast-track
4. Otherwise: Normal prioritization flow
```

### Technical Debt
```bash
1. Create: TD_XXX_Refactor_Area.md in backlog/
2. Discuss in tech review
3. Prioritize based on risk/impact
4. Schedule in sprint capacity
```

## Maintenance

This document is maintained by the Product Owner role and should be updated when:
- New work item types are introduced
- Naming patterns change
- Organization structure evolves
- Lessons learned suggest improvements

Last Updated: 2024-01-15