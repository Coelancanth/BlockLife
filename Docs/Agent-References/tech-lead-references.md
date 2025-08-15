# Tech Lead Agent - Documentation References

## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation

## Your Domain-Specific Documentation

### 🧠 **Your Living Wisdom Documents** (YOU OWN THESE)
- **[LWP_004_Production_Readiness_Checklist.md](../Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)** - ⭐ **YOUR RESPONSIBILITY** ⭐
  - Comprehensive production readiness validation procedures
  - Update this with new production lessons and critical checkpoints
  - MANDATORY gate before any production deployment

### Implementation Planning Workspace
**Note**: With embedded implementation planning, Tech Leads now work directly within VS items rather than maintaining separate planning documents.
- **Your workspace**: `Docs/Backlog/items/VS_*.md` - Add "🏗️ Implementation Plan" sections to VS items
- **Your template**: `Docs/Backlog/templates/VS_Template.md` - Standard embedded planning structure

## Shared Documentation You Should Know

### Implementation Planning (Embedded Approach)
- `Docs/Backlog/templates/VS_Template.md` - Embedded implementation planning template
- `Docs/Shared/Architecture/Reference-Implementations/` - Archived reference implementations
- Focus on: Move Block feature in VS items as the gold standard pattern

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

When adding implementation planning to VS items:
1. **Architecture First**: Review existing patterns and constraints
2. **Phase Breakdown**: TDD cycle phases (RED → GREEN → REFACTOR) 
3. **Risk Assessment**: Check post-mortems for similar features
4. **Dependency Analysis**: Identify integration points and dependencies
5. **Testing Strategy**: Plan for 4-pillar testing approach
6. **Embed in VS Item**: Add detailed planning directly in the VS item's "🏗️ Implementation Plan" section

## Technical Decision Criteria

- **Consistency**: Does this align with existing patterns?
- **Maintainability**: Can the team maintain this long-term?
- **Testability**: Can we validate this architecturally?
- **Performance**: Any performance implications?
- **Risk**: What could go wrong and how do we mitigate?

## VS Item Management (Embedded Planning)

### VS Item Structure with Embedded Planning
```
VS_XXX_FeatureName.md
├── 📋 User Story (Product Owner)
├── 🏗️ Implementation Plan (Tech Lead) ← YOUR DOMAIN
│   ├── Technical Approach
│   ├── Implementation Phases  
│   ├── Technical Decisions
│   └── Risk Assessment
├── 🎯 Vertical Slice Components
├── ✅ Acceptance Criteria
├── 🧪 Test Strategy
└── ✅ Definition of Done
```

### Your Implementation Planning Responsibilities
- **Technical Approach**: Architecture patterns and integration points
- **Phase Breakdown**: Detailed implementation phases with TDD workflow
- **Technical Decisions**: Data flow, error handling, state management patterns  
- **Risk Assessment**: Technical risks, dependencies, complexity factors
- **Mitigation Strategies**: How to reduce identified risks

### Standard Phase Planning Pattern
- **Phase 1**: Core domain logic (TDD RED → GREEN)
- **Phase 2**: Notification & integration layer
- **Phase 3**: Presentation layer implementation
- **Phase 4**: Validation, stress testing, and polish

## Integration Points
- **Product Owner**: User story refinement and acceptance criteria
- **Architect**: Technical decision validation and pattern compliance
- **QA Engineer**: Testing strategy and validation approach
- **Dev Engineer**: Implementation guidance and code review