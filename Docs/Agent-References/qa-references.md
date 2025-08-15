# QA Engineer Agent - Documentation References

## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation

## Your Domain-Specific Documentation

### 🧠 **Your Living Wisdom Documents** (YOU OWN THESE)
- **[LWP_001_Stress_Testing_Playbook.md](../Living-Wisdom/Playbooks/LWP_001_Stress_Testing_Playbook.md)** - ⭐ **YOUR RESPONSIBILITY** ⭐
  - Comprehensive stress testing procedures born from F1 experience
  - Update this with new stress testing insights and patterns
  - Critical for preventing production failures

### Agent-Specific Reference Documents  
Location: `Docs/Agent-Specific/QA/`
- `integration-testing.md` - GdUnit4 patterns and integration test architecture
- `testing-strategy.md` - Four-pillar testing strategy implementation

## Shared Documentation You Should Know

### Testing Guides
- `Docs/Shared/Architecture/Test_Guide.md` - Complete testing strategy
- `Docs/Shared/Architecture/Property_Based_Testing_Guide.md` - FsCheck patterns
- `Docs/Shared/Guides/GdUnit4_Integration_Testing_Guide.md` - Godot-specific testing

### Critical Testing Architecture
- `tests/Architecture/ArchitectureFitnessTests.cs` - Architecture enforcement tests
- `tests/BlockLife.Core.Tests/` - Unit test examples and patterns
- SimpleSceneTest pattern for integration tests

### Living Wisdom System References
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all living documents
- **[LWP_002_Integration_Testing_Patterns.md](../Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md)** - Architect-owned integration patterns (collaborate with Architect)
- **[LWP_004_Production_Readiness_Checklist.md](../Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)** - Tech Lead-owned readiness validation

### Historical Incident Reports
- `Docs/Incident-Reports/Resolved/` - Archived bug reports for reference
- Original stress test insights now integrated into your LWP_001 playbook

### Bug Investigation Patterns
- `Docs/Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md` - Bug-to-test protocol
- All bug reports show regression test patterns

## Four-Pillar Testing Strategy

1. **Unit Tests**: Example-based validation of business logic
2. **Property Tests**: Mathematical proofs using FsCheck.Xunit  
3. **Architecture Tests**: Automated constraint enforcement
4. **Integration Tests**: GdUnit4 for Godot-specific testing

## Critical Testing Patterns

### Integration Test Architecture (MANDATORY)
```csharp
[TestSuite]
public partial class YourIntegrationTest : Node
{
    // MUST use SimpleSceneTest pattern
    // Access real SceneRoot service container
    // No parallel service containers allowed
}
```

### Stress Testing Requirements
- Test with 100+ concurrent operations
- Verify thread safety of all shared state
- Validate memory leak prevention
- Test notification pipeline under load

## Quality Gates Checklist

Before marking implementation complete:
- [ ] Architecture fitness tests pass
- [ ] Unit tests provide comprehensive coverage
- [ ] Property tests validate mathematical invariants  
- [ ] Integration tests verify end-to-end scenarios
- [ ] Stress tests confirm production readiness
- [ ] All critical patterns enforced by tests

## Testing Technology Stack
- **XUnit**: Unit testing framework
- **FsCheck.Xunit**: Property-based testing
- **GdUnit4**: Godot integration testing
- **FluentAssertions**: Test readability
- **Moq**: Mocking framework (when needed)

## Integration Points
- **Dev Engineer**: Test-driven development collaboration
- **Debugger Expert**: Complex bug investigation
- **Architect**: Architecture fitness test design