# Agile Product Owner Agent Workflow Guide

## Overview
The Agile Product Owner agent is the **single source of truth maintainer** for the Product Backlog. Beyond breaking down features into vertical slices, this agent proactively monitors all project activities and ensures every work item is properly tracked, prioritized, and managed through its lifecycle.

## Primary Workflow: Backlog Maintenance

### Continuous Monitoring
The PO agent proactively monitors:
- **Other agent outputs** → Converts to work items
- **Sprint progress** → Updates item status
- **Completed work** → Archives and documents

### Work Item Creation
When detecting new work needs:
1. Create appropriate item type (VS/BF/TD/HF)
2. Place in `Docs/Product_Backlog/backlog/` with XXX suffix
3. Link to source document (bug report, ADR, etc.)
4. Update README.md dashboard

### Prioritization Workflow
1. Review items in `backlog/` folder
2. Assign priority number (001, 002, etc.)
3. Move to `ready/` folder
4. Update sprint planning in README.md

## Secondary Workflow: Feature Decomposition

### Step 1: Feature Analysis
When you have a new feature idea, the agent will:
1. Analyze the feature's scope and complexity
2. Identify architectural components needed
3. Determine dependencies on existing systems
4. Break down into thin vertical slices

### Step 2: Vertical Slice Creation
Each slice will be saved as a separate file:
- **Naming**: `VS_XXX_Domain_Feature.md` (initially)
- **Location**: `Docs/Product_Backlog/backlog/`
- **Format**: Complete vertical slice with all layers
- **Size**: 1-2 day implementation chunks

### Step 3: Implementation Sequence
The agent provides:
- Prioritized list of slices
- Dependencies between slices
- Recommended implementation order
- Risk assessment for each slice

## Usage Patterns

### Pattern A: New Feature Decomposition
```
You: "I want to add a block rotation feature"

Agent will provide:
- Story 1: Basic 90° Y-axis rotation
  - Command: RotateBlockCommand
  - Handler: RotateBlockCommandHandler
  - Tests: Rotation validation, grid boundaries
  
- Story 2: Multi-axis rotation
  - Enhanced command with axis parameter
  - Property tests for rotation matrices
  
- Story 3: Visual rotation preview
  - Ghost block presenter
  - Integration tests for UI feedback
```

### Pattern B: Backlog Prioritization
```
You: "Prioritize: inventory, crafting, multiplayer, animations"

Agent will analyze:
- Core gameplay dependencies
- Architectural complexity
- Testing pattern establishment
- Player value delivery
```

### Pattern C: Story Refinement
```
You: "Refine the crafting system into implementable slices"

Agent will detail:
- Exact commands and handlers needed
- Notification events for state changes
- Presenter subscriptions required
- Complete test scenarios for TDD
```

## Work Item Types and Naming

### Naming Convention
All work items follow this pattern:
```
[Type]_[Number]_[Domain]_[Description].md

Types:
- VS_XXX = Vertical Slice (new feature)
- BF_XXX = Bug Fix (defect repair)
- TD_XXX = Tech Debt (refactoring)
- HF_XXX = Hotfix (critical issue)

Examples:
- VS_001_Block_Rotation.md
- BF_002_Grid_Boundary_Validation.md
- TD_003_Notification_Pipeline_Refactor.md
- HF_001_Critical_State_Corruption.md
```

### File Structure
```
Docs/Product_Backlog/
├── README.md              # Sprint dashboard (maintained by PO)
├── active/               # In development
│   └── VS_001_Block_Rotation.md
├── ready/                # Prioritized for next sprint
│   ├── VS_002_Inventory_Basic.md
│   └── BF_001_State_Fix.md
├── backlog/              # Unprioritized (XXX suffix)
│   ├── VS_XXX_Multiplayer.md
│   └── TD_XXX_Performance.md
└── completed/            # Archive
    └── 2024-Q1/
        └── VS_000_Block_Move.md
```

## BlockLife-Specific Patterns

### Vertical Slice Components
Every VS story must include:
1. **Godot View** (`godot_project/features/`)
2. **Presenter** (`src/Features/[Domain]/Presenters/`)
3. **Commands** (`src/Features/[Domain]/Commands/`)
4. **Handlers** (`src/Features/[Domain]/Handlers/`)
5. **DTOs** (`src/Features/[Domain]/DTOs/`)
6. **Notifications** (Event bridges)
7. **Tests** (All 4 pillars)

### Command Flow Pattern
```
User Input → Presenter → Command → Handler → Effect → Notification → Presenter → View Update
```

### Test-Driven Development
1. **Architecture Tests**: Define constraints
2. **Red Phase**: Write failing unit tests
3. **Green Phase**: Minimal implementation
4. **Refactor Phase**: Optimize code
5. **Property Tests**: Mathematical invariants
6. **Integration Tests**: Full slice validation

## Example Prompts

### Feature Breakdown
```
"Break down a save/load game system into vertical slices following BlockLife architecture"
```

### User Story Creation
```
"Create user stories for block physics with gravity, including test scenarios"
```

### Technical Alignment
```
"How would a multiplayer sync system fit into our CQRS architecture?"
```

### Prioritization
```
"Prioritize these features for MVP: block placement, movement, destruction, crafting, inventory"
```

## Quality Checklist

Before implementing any story from the agent, verify:
- [ ] Story delivers complete functionality (not just UI or backend)
- [ ] All 7 architectural layers are addressed
- [ ] Commands are immutable records with init setters
- [ ] Handlers return `Fin<T>` for error handling
- [ ] No Godot dependencies in Core project
- [ ] Notification pipeline is complete
- [ ] References Move Block pattern where applicable
- [ ] TDD workflow is clearly defined
- [ ] Acceptance criteria are testable

## Integration with Development

1. **Get stories from agent** → 
2. **Create feature branch** (`git checkout -b feat/feature-name`) →
3. **Write architecture tests** →
4. **Follow TDD Red-Green-Refactor** →
5. **Implement vertical slice** →
6. **Run full test suite** →
7. **Create PR with complete slice**

## Common Pitfalls to Avoid

❌ **Don't**: Create UI-only stories
✅ **Do**: Include all layers in each story

❌ **Don't**: Skip test requirements
✅ **Do**: Define tests upfront for TDD

❌ **Don't**: Mix concerns across boundaries
✅ **Do**: Keep Core pure, Godot in presentation only

❌ **Don't**: Create large, multi-day stories
✅ **Do**: Break into 1-2 session slices

## References

- **Gold Standard**: Move Block implementation at `src/Features/Block/Move/`
- **Architecture Guide**: `Docs/1_Architecture/Architecture_Guide.md`
- **Development Workflow**: `Docs/6_Guides/Comprehensive_Development_Workflow.md`
- **Implementation Plans**: `Docs/3_Implementation_Plans/`