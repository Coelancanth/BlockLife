# Test Designer Workflow

## Core Principle

**Fast TDD RED phase** - write failing test in <3 minutes, suggest edge cases, hand off to dev-engineer.

## Action 1: Write Failing Test

### When to Use
User says: "I want to test..." or "Create test for..."

### Process
1. **Copy existing pattern** from `tests/Features/Block/Move/`
2. **Adapt to requirement** - change names, inputs, assertions
3. **Make it fail** - test should compile but fail assertion
4. **Suggest 2-3 edge cases**

### Standard Template
```csharp
[Test]
public async Task Method_Scenario_ExpectedOutcome()
{
    // Arrange
    var command = new Command(...);
    
    // Act
    var result = await _handler.Handle(command);
    
    // Assert
    result.IsSucc.Should().BeTrue();
}
```

### Common Edge Cases
- Null/empty inputs
- Boundary values (min/max)
- Invalid combinations  
- Already exists/not found

## Action 2: Quick Edge Case Tests

### When to Use
After main test, if user wants edge cases

### Process
1. **Pick 2-3 from common list**
2. **Copy main test pattern**
3. **Change input to edge case**
4. **Update assertion for expected failure**

### Edge Case Template
```csharp
[Test]
public async Task Method_EdgeCaseScenario_ReturnsError()
{
    // Arrange - invalid input
    var command = new Command(invalidValue);
    
    // Act
    var result = await _handler.Handle(command);
    
    // Assert
    result.IsFail.Should().BeTrue();
    result.Error.Should().Contain("expected error");
}
```

## Quality Gates

- **Test written in <3 minutes**
- **Follows existing pattern exactly**
- **Fails for right reason** (not compile error)
- **Clear assertion message**
- **One behavior per test**

## Common Patterns

### Command Handler Test
```csharp
[TestFixture]
public class FeatureCommandHandlerTests
{
    private FeatureCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _handler = new FeatureCommandHandler(...);
    }
    
    [Test]
    public async Task Handle_ValidCommand_Succeeds() { }
    
    [Test] 
    public async Task Handle_InvalidInput_ReturnsError() { }
}
```

### Assertion Patterns
```csharp
// Success
result.IsSucc.Should().BeTrue();

// Failure with message
result.IsFail.Should().BeTrue();
result.Error.Should().Contain("expected error");

// State verification
entity.Property.Should().Be(expectedValue);
```

## Success Metrics

- **90% of tests follow existing patterns**
- **Tests written quickly** without analysis paralysis
- **Clear failure messages** when tests fail
- **Minimal setup complexity**