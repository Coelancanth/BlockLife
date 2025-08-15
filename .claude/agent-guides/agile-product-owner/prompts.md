# Agile Product Owner Agent - Prompt Templates

## Feature Decomposition Prompts

### Basic Feature Breakdown
```
"Break down [feature name] into vertical slices following BlockLife's architecture"
```

### Detailed Feature Analysis
```
"I want to implement [feature description]. Create user stories with:
- Complete vertical slices
- Test requirements for TDD
- Reference to similar patterns
- Priority and dependencies"
```

### Complex System Breakdown
```
"Decompose [system name] into implementable stories that:
- Respect Clean Architecture boundaries
- Include all 7 architectural layers
- Can be completed in 1-2 sessions each
- Build incrementally on each other"
```

## User Story Creation Prompts

### Single Story with Tests
```
"Create a user story for [specific feature] including:
- BDD acceptance criteria
- Commands and handlers needed
- Test scenarios for TDD Red phase
- Notification events"
```

### Story Refinement
```
"Refine this rough idea into a proper user story: [rough description]
Include technical components and test requirements"
```

### Bug Fix Story
```
"Create a user story to fix: [bug description]
Include regression test requirements and root cause analysis"
```

## Prioritization Prompts

### Feature List Prioritization
```
"Prioritize these features for development:
- [Feature 1]
- [Feature 2]
- [Feature 3]
Explain rationale based on gameplay value and dependencies"
```

### MVP Definition
```
"What features are MUST HAVE for BlockLife MVP?
Prioritize this list: [features]
Consider core gameplay loop and architectural risk"
```

### Risk-Based Prioritization
```
"Analyze these features for implementation risk:
[feature list]
Recommend order to minimize architectural problems"
```

## Architecture Alignment Prompts

### Pattern Fit Analysis
```
"How would [feature/system] fit into our CQRS architecture?
Show command flow and component breakdown"
```

### Boundary Check
```
"Verify [feature] respects Clean Architecture boundaries
Identify what goes in Core vs Presentation"
```

### Integration Planning
```
"How should [new system] integrate with existing systems?
Consider notification pipeline and DI setup"
```

## Test Planning Prompts

### TDD Scenario Creation
```
"Define TDD test scenarios for [feature]:
- Architecture fitness tests
- Unit test cases (Red phase)
- Property test invariants
- Integration test requirements"
```

### Property Test Definition
```
"What mathematical properties should [feature] maintain?
Define property tests using FsCheck patterns"
```

### Regression Test Planning
```
"Create regression test requirements for [bug fix]
that would have caught this issue"
```

## Technical Specification Prompts

### Command Design
```
"Design commands for [feature]:
- Command structure (immutable record)
- Validation requirements
- Handler return type (Fin<T>)
- Error cases"
```

### Notification Design
```
"Define notification events for [feature]:
- When events fire
- Event data structure
- Presenter subscriptions
- Bridge pattern needed"
```

### Service Design
```
"Design service interfaces for [system]:
- Interface definitions
- DI registration approach
- Single source of truth pattern"
```

## Sprint Planning Prompts

### Sprint Goal Setting
```
"Given 1 week sprint, which stories from [backlog] should we complete?
Consider dependencies and team velocity"
```

### Daily Task Breakdown
```
"Break down [story] into daily development tasks:
- Day 1: Architecture tests and TDD Red
- Day 2: Implementation and Green
- Day 3: Refactor and integration"
```

## Review and Validation Prompts

### Story Completeness Check
```
"Review this user story for completeness:
[story text]
Check against BlockLife patterns and architecture"
```

### Acceptance Criteria Validation
```
"Are these acceptance criteria testable and complete?
[criteria]
Suggest improvements if needed"
```

### Implementation Plan Review
```
"Review this implementation approach for [feature]:
[approach]
Identify architectural risks or pattern violations"
```

## Quick Templates

### Feature → Stories
```
"[Feature] → User stories with tests"
```

### Prioritize List
```
"Priority order: [item1, item2, item3, ...]"
```

### Architecture Check
```
"[Feature] + CQRS/Clean Architecture = ?"
```

### Test Requirements
```
"TDD tests for [feature]"
```

### Bug → Story
```
"Bug: [description] → User story + regression test"
```

## Advanced Prompts

### Cross-Cutting Concerns
```
"How should [logging/auth/caching] work across vertical slices?
Maintain Clean Architecture while avoiding duplication"
```

### Performance Considerations
```
"Analyze [feature] for performance implications:
- State management approach
- Query optimization needs
- Caching opportunities"
```

### Migration Planning
```
"Plan migration from [old system] to [new system]:
- Incremental stories
- Backward compatibility
- Data migration approach"
```

## Tips for Effective Prompts

1. **Be specific** about BlockLife context
2. **Reference existing patterns** (Move Block)
3. **Ask for test requirements** explicitly
4. **Request rationale** for recommendations
5. **Include constraints** (time, dependencies)
6. **Specify output format** needed