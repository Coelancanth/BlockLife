# QA Engineer Agent - Documentation References

## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/QA/`

- `integration-testing.md` - GdUnit4 patterns and integration test architecture
- `testing-strategy.md` - Four-pillar testing strategy implementation
- `stress-testing.md` - Concurrent operation testing patterns

## Shared Documentation You Should Know

### Testing Guides
- `Docs/Shared/Architecture/Test_Guide.md` - Complete testing strategy
- `Docs/Shared/Architecture/Property_Based_Testing_Guide.md` - FsCheck patterns
- `Docs/Shared/Guides/GdUnit4_Integration_Testing_Guide.md` - Godot-specific testing

### Critical Testing Architecture
- `tests/Architecture/ArchitectureFitnessTests.cs` - Architecture enforcement tests
- `tests/BlockLife.Core.Tests/` - Unit test examples and patterns
- SimpleSceneTest pattern for integration tests

### Post-Mortems for Testing Insights
- `Docs/Shared/Post-Mortems/Architecture_Stress_Testing_Lessons_Learned.md` - F1 stress test critical lessons
- `Docs/Shared/Post-Mortems/Integration_Test_Architecture_Deep_Dive.md` - GdUnit4 architecture patterns
- `Docs/Shared/Post-Mortems/F1_Architecture_Stress_Test_Report.md` - Comprehensive stress test results

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