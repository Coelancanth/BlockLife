# VSA Refactoring Agent - Documentation References
n## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation


## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/VSA/`

- `organization-patterns.md` - VSA file organization and slice independence patterns
- `refactoring-guidelines.md` - When to extract vs. keep duplicated
- `slice-boundaries.md` - Maintaining proper VSA boundaries

## Shared Documentation You Should Know

### 🧠 **Living Wisdom System** (For VSA Context)
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all living documents
- **[LWP_002_Integration_Testing_Patterns.md](../Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md)** - VSA testing patterns and slice isolation
- All Living Wisdom documents follow VSA organization - study their structure

### VSA Foundation
- `Docs/Shared/Architecture/Reference-Implementations/000_Vertical_Slice_Architecture_Plan.md` - Core VSA principles and patterns (archived reference)
- `Docs/Backlog/templates/VS_Template.md` - Current VSA template with embedded planning
- `src/Features/Block/Move/` - Gold standard VSA implementation to reference
- `tests/BlockLife.Core.Tests/Features/Block/Move/` - Proper slice testing patterns

### Architecture for VSA Compliance
- `Docs/Shared/Architecture/Architecture_Guide.md` - Clean Architecture boundaries
- `Docs/Shared/Architecture/Standard_Patterns.md` - Established patterns that cross slices
- `Docs/Shared/Architecture/Test_Guide.md` - Four-pillar testing in VSA context

### Post-Mortems for Refactoring Insights
- `Docs/Shared/Post-Mortems/Architecture_Stress_Testing_Lessons_Learned.md` - Lessons about shared state
- `Docs/Shared/Post-Mortems/Critical_Architecture_Fixes_Post_Mortem.md` - Architectural refactoring examples

## VSA Refactoring Decision Matrix

### Extract to Infrastructure When:
- ✅ Database/repository logic duplicated across slices
- ✅ External service integrations repeated
- ✅ Cross-cutting utilities (logging, validation)
- ✅ Framework abstractions

### Extract to Domain When:
- ✅ Value objects (GridPosition, BlockId) duplicated
- ✅ Domain errors and exceptions
- ✅ Mathematical utilities and business constants
- ✅ Validation rules that are truly domain-wide

### Keep in Slices When:
- ❌ Feature-specific business logic
- ❌ Feature-specific DTOs and commands
- ❌ UI components and view logic
- ❌ Feature-specific validation rules
- ❌ Slice-specific tests

## Duplication Assessment Checklist

Before extracting code:
- [ ] Is this identical in 3+ slices?
- [ ] Is this infrastructure/technical concern?
- [ ] Will this likely remain identical?
- [ ] Does extraction preserve slice independence?
- [ ] What's the coupling cost vs. duplication cost?

## Common Extraction Patterns

### Pattern 1: Service Extraction
```csharp
// From: Multiple handlers with same logic
// To: Shared service with interface
```

### Pattern 2: Value Object Consolidation
```csharp
// From: Similar records in each slice
// To: Single domain value object
```

### Pattern 3: Base Class Creation
```csharp
// From: Repeated handler patterns
// To: Abstract base class
```

## Validation After Extraction

Post-refactoring checklist:
- [ ] Dependencies flow inward only (Slices → Infrastructure → Domain)
- [ ] No slice-to-slice dependencies created
- [ ] Each slice remains independently testable
- [ ] All architecture tests still pass
- [ ] No business logic leaked to infrastructure

## Integration Points
- **Architect**: Validate architectural compliance of extractions
- **QA Engineer**: Ensure testing strategy supports refactored structure
- **Dev Engineer**: Implementation of extracted components