# Test Designer Agent - Documentation References

## Your Role in TDD RED Phase

You are responsible for the TDD RED phase - translating requirements into clear, failing tests that guide implementation. You work closely with Dev Engineer who implements code to make your tests pass.

## Shared Documentation You Should Know

### Testing Strategy Foundation
- `Docs/Shared/Architecture/Test_Guide.md` - Four-pillar testing strategy
- `Docs/Shared/Architecture/Property_Based_Testing_Guide.md` - FsCheck property test patterns
- `tests/Architecture/ArchitectureFitnessTests.cs` - Architecture constraint examples

### TDD Workflow Integration
- `Docs/Shared/Guides/Comprehensive_Development_Workflow.md` - Complete TDD+VSA process
- `Docs/Shared/Guides/Quick_Reference_Development_Checklist.md` - TDD workflow steps

### Reference Test Implementations
- `tests/BlockLife.Core.Tests/Features/Block/Move/` - Gold standard test structure
- `tests/BlockLife.Core.Tests/` - Unit test patterns and organization
- Property test examples throughout test suite

### Post-Mortems for Test Design Insights
- `Docs/Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md` - Bug-to-test protocol
- All bug reports show how tests should have caught issues

## Test Design Principles

### 1. Test Structure (Arrange-Act-Assert)
```csharp
[Test]
public void Should_ReturnError_When_InvalidInput()
{
    // Arrange - Set up test data and dependencies
    var command = new MoveBlockCommand(...);
    var handler = new MoveBlockCommandHandler(...);
    
    // Act - Execute the operation
    var result = await handler.Handle(command);
    
    // Assert - Verify expected outcome
    result.IsFail.Should().BeTrue();
    result.Error.Should().Contain("expected error message");
}
```

### 2. Test Categories
- **Unit Tests**: Fast, isolated, example-based validation
- **Property Tests**: Mathematical invariants using FsCheck
- **Architecture Tests**: Constraint enforcement (already exist)
- **Integration Tests**: End-to-end validation (QA Engineer handles these)

### 3. Test Naming Convention
- **Method**: `Should_[ExpectedOutcome]_When_[Condition]`
- **Variables**: Descriptive names that show intent
- **Classes**: `[FeatureName]Tests` or `[FeatureName]PropertyTests`

## Common Test Patterns

### Command Handler Testing
```csharp
public class MoveBlockCommandHandlerTests
{
    // Success scenarios
    [Test] public void Should_MoveBlock_When_ValidCommand() 
    
    // Error scenarios  
    [Test] public void Should_ReturnError_When_BlockNotFound()
    [Test] public void Should_ReturnError_When_InvalidPosition()
    
    // Edge cases
    [Test] public void Should_ReturnError_When_MoveToSamePosition()
}
```

### Property-Based Testing
```csharp
[Property]
public void GridPosition_ShouldAlwaysBeValid(NonNegativeInt x, NonNegativeInt y)
{
    var position = new GridPosition(x.Get, y.Get);
    
    position.X.Should().BeGreaterOrEqualTo(0);
    position.Y.Should().BeGreaterOrEqualTo(0);
}
```

### Error Condition Testing
```csharp
[Test]
public void Should_ReturnSpecificError_When_BusinessRuleViolated()
{
    // Focus on specific error codes and messages
    result.Error.Code.Should().Be("INVALID_MOVE");
    result.Error.Message.Should().Contain("cannot move to occupied position");
}
```

## Test Design Workflow

### 1. Understand Requirement
- What is the user trying to achieve?
- What are the happy path scenarios?
- What can go wrong?
- What are the edge cases?

### 2. Design Test Structure
- Start with happy path test
- Add error condition tests
- Consider edge cases and boundary conditions
- Think about property-based tests for mathematical operations

### 3. Write Failing Tests
- Tests should fail for the right reason (not compile errors)
- Clear assertion messages
- Test one thing at a time
- Make the test as simple as possible

### 4. Edge Case Discovery
Common edge cases to consider:
- Null/empty inputs
- Boundary values (min/max)
- Invalid combinations
- Concurrent access scenarios
- Resource exhaustion

## Integration with Other Agents

### With Dev Engineer
- Provide clear, failing tests that guide implementation
- Tests should specify exact expected behavior
- Focus on "what" not "how"

### With QA Engineer  
- Unit tests complement integration tests
- Share edge case discoveries
- Ensure comprehensive coverage between unit and integration tests

### With Product Owner
- Translate acceptance criteria into executable tests
- Clarify ambiguous requirements through test scenarios
- Validate business rule understanding