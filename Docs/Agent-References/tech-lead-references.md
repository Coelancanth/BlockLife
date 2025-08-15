# Tech Lead Agent - Documentation References

## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/TechLead/`

- `feature-development.md` - Feature development planning and TDD+VSA workflow
- `implementation-planning.md` - Breaking down VS items into technical phases
- `technical-decisions.md` - Technical decision-making frameworks

## Shared Documentation You Should Know

### Implementation Planning
- `Docs/Shared/Implementation-Plans/` - All feature implementation plans
- Focus on: 000_VSA, 001_F1_Block_Placement, 002_Move_Block (reference implementations)

### Architecture for Planning Context
- `Docs/Shared/Architecture/Architecture_Guide.md` - Core architectural principles for planning
- `Docs/Shared/Architecture/Standard_Patterns.md` - Established patterns to follow
- `src/Features/Block/Move/` - Gold standard implementation to reference

### Development Workflow Integration
- `Docs/Shared/Guides/Comprehensive_Development_Workflow.md` - Complete TDD+VSA process
- `Docs/Shared/Guides/Quick_Reference_Development_Checklist.md` - Daily workflow steps
- `Docs/Shared/Guides/Git_Workflow_Guide.md` - Branch and PR workflow

### Post-Mortems for Risk Assessment
- `Docs/Shared/Post-Mortems/Architecture_Stress_Testing_Lessons_Learned.md` - Critical production lessons
- `Docs/Shared/Post-Mortems/F1_Architecture_Stress_Test_Report.md` - Comprehensive stress test insights
- All bug reports for understanding common failure patterns

## Planning Framework

When creating implementation plans:
1. **Architecture First**: Review existing patterns and constraints
2. **Phase Breakdown**: TDD cycle phases (RED → GREEN → REFACTOR)
3. **Risk Assessment**: Check post-mortems for similar features
4. **Dependency Analysis**: Identify integration points and dependencies
5. **Testing Strategy**: Plan for 4-pillar testing approach

## Technical Decision Criteria

- **Consistency**: Does this align with existing patterns?
- **Maintainability**: Can the team maintain this long-term?
- **Testability**: Can we validate this architecturally?
- **Performance**: Any performance implications?
- **Risk**: What could go wrong and how do we mitigate?

## VS Item Management

### VS Item Structure
```
VS_XXX_FeatureName.md
├── Business Context
├── Technical Approach  
├── Implementation Phases
├── Testing Strategy
├── Risk Assessment
└── Acceptance Criteria
```

### Phase Planning Pattern
- **Phase 1**: Core domain logic + architecture tests
- **Phase 2**: Presenter integration + unit tests  
- **Phase 3**: View implementation + integration tests
- **Phase 4**: Performance validation + stress tests

## Integration Points
- **Product Owner**: User story refinement and acceptance criteria
- **Architect**: Technical decision validation and pattern compliance
- **QA Engineer**: Testing strategy and validation approach
- **Dev Engineer**: Implementation guidance and code review