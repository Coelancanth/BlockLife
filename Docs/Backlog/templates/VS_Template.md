# VS_XXX: [Feature Name]

**Status**: Backlog
**Priority**: TBD
**Size**: [S/M/L] ([1-5] days)
**Sprint**: TBD
**Domain**: [Block/Inventory/Grid/UI/etc.]

## User Story
As a [player/developer]
I want [specific functionality]
So that [value/benefit]

## Vertical Slice Components

### Commands
- `[CommandName] { Properties }`
- Validation rules:
  - [Rule 1]
  - [Rule 2]

### Handlers
- `[HandlerName]` → Returns `Fin<Result>`
- Business logic:
  - [Step 1]
  - [Step 2]
- Error cases:
  - [Error case 1]
  - [Error case 2]

### Queries (if needed)
- `[QueryName]` → Returns `[DataType]`

### Notifications
- `[NotificationName] { Properties }`
- Published when: [Condition]
- Subscribers: [List of presenters]

### Domain/DTOs
- `[EntityName]`: [Description]
- Properties:
  - [Property 1]
  - [Property 2]

### Presenters
- `[PresenterName]`
- Subscribes to: [Notifications]
- Updates view: [View interface methods]

### Views
- Interface: `I[ViewName]`
- Methods:
  - `Method1(params)`
  - `Method2(params)`
- Godot implementation: `godot_project/features/[domain]/[feature]/`

## Acceptance Criteria
- [ ] Given [context], When [action], Then [outcome]
- [ ] Given [context], When [action], Then [outcome]
- [ ] Given [context], When [action], Then [outcome]

## Test Requirements

### Architecture Tests
- [ ] Commands are immutable records
- [ ] Handlers return Fin<T>
- [ ] No Godot dependencies in Core

### Unit Tests
- [ ] Command validation tests
- [ ] Handler success path tests
- [ ] Handler error path tests
- [ ] Query tests (if applicable)

### Property Tests
- [ ] [Invariant 1]: [Description]
- [ ] [Invariant 2]: [Description]

### Integration Tests
- [ ] Full vertical slice flow
- [ ] UI interaction to state change
- [ ] Notification pipeline verification

## Dependencies
- Depends on: [List other VS/features]
- Blocks: [List dependent VS/features]

## Implementation Notes
- Reference pattern: `src/Features/Block/Move/`
- Similar to: [Other feature if applicable]
- Special considerations: [Any unique aspects]

## Definition of Done
- [ ] All tests passing
- [ ] Code follows BlockLife patterns
- [ ] Documentation updated
- [ ] PR approved and merged
- [ ] Deployed to test environment

## References
- Related ADR: [Link if exists]
- Bug report: [Link if this addresses a bug]
- Design doc: [Link if exists]