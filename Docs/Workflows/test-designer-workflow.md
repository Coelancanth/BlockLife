# Test Designer Workflow

## Purpose
Define procedures for the Test Designer agent to translate Commander's requirements into failing unit tests during the TDD RED phase.

---

## Core Workflow Actions

### 1. Translate Requirement to Test

**Trigger**: "I want to test..." or "Create a test for..."

**Input Required**:
- Feature requirement or user story
- Expected behavior description
- Context about the system
- Any constraints or rules

**Steps**:

1. **Clarify Requirement**
   ```
   Commander: "I want the inventory to hold items"
   
   Test Designer asks:
   - What's the maximum capacity?
   - What happens when full?
   - Can items stack?
   - Are there item types?
   ```

2. **Propose Test Structure**
   ```csharp
   // Suggest test class organization
   public class InventoryTests
   {
       // Happy path tests
       // Boundary tests  
       // Error cases
       // Edge cases
   }
   ```

3. **Design Core Test**
   ```csharp
   [Test]
   public async Task AddItem_ToEmptyInventory_ItemAdded()
   {
       // Arrange
       var inventory = new Inventory(capacity: 10);
       var item = new Item("Sword");
       
       // Act
       var result = await inventory.AddItem(item);
       
       // Assert
       result.IsSucc.Should().BeTrue();
       inventory.Count.Should().Be(1);
       inventory.Contains(item).Should().BeTrue();
   }
   ```

4. **Suggest Edge Cases**
   ```csharp
   // Edge cases to consider:
   - Adding to full inventory
   - Adding null item
   - Adding duplicate items
   - Removing from empty inventory
   - Concurrent additions
   ```

5. **Create Failing Test**
   - Test must fail (no implementation yet)
   - Failure message should be clear
   - Test should compile but fail assertions

**Output Format**:
```
📝 Test Created: AddItem_ToEmptyInventory_ItemAdded

Test validates: Items can be added to inventory
Expected to fail because: Inventory class doesn't exist yet

Suggested additional tests:
1. AddItem_ToFullInventory_ReturnsError
2. AddItem_NullItem_ThrowsException
3. AddItem_DuplicateItem_StacksCorrectly

Ready for implementation (GREEN phase).
```

---

### 2. Create Test Suite Structure

**Trigger**: "Set up tests for [feature]"

**Steps**:

1. **Define Test Class**
   ```csharp
   [TestFixture]
   public class [Feature]Tests
   {
       private [Feature]Handler _handler;
       private IMediator _mediator;
       private I[Service] _mockService;
       
       [SetUp]
       public void Setup()
       {
           // Test initialization
       }
   }
   ```

2. **Organize Test Categories**
   ```csharp
   #region Happy Path Tests
   #endregion
   
   #region Validation Tests
   #endregion
   
   #region Error Handling Tests
   #endregion
   
   #region Edge Cases
   #endregion
   ```

3. **Create Test Data Builders**
   ```csharp
   public class TestDataBuilder
   {
       public static Command ValidCommand() => new Command(...);
       public static Command InvalidCommand() => new Command(...);
   }
   ```

**Output**: Complete test class structure ready for tests

---

### 3. Design Assertions

**Trigger**: "What should I assert?" or "How do I verify..."

**Steps**:

1. **Identify What to Verify**
   ```
   State changes: Object properties updated?
   Side effects: Events published? Services called?
   Return values: Correct result returned?
   Error handling: Proper errors for bad input?
   ```

2. **Write Clear Assertions**
   ```csharp
   // State verification
   entity.Status.Should().Be(ExpectedStatus);
   
   // Behavior verification
   _mockService.Verify(x => x.Method(It.IsAny<Type>()), Times.Once);
   
   // Collection assertions
   collection.Should().HaveCount(3);
   collection.Should().Contain(item);
   
   // Exception assertions
   act.Should().Throw<InvalidOperationException>()
      .WithMessage("Inventory full");
   ```

3. **Multiple Assertions Pattern**
   ```csharp
   // Group related assertions
   result.Should().NotBeNull();
   result.IsSucc.Should().BeTrue();
   result.Value.Should().BeEquivalentTo(expected);
   ```

**Output**: Clear assertions that verify behavior

---

### 4. Create Mocks and Fakes

**Trigger**: "Mock this dependency" or "Create test double"

**Steps**:

1. **Identify Dependencies**
   ```csharp
   // What needs mocking?
   - External services
   - Database repositories  
   - Message publishers
   - Time providers
   ```

2. **Create Mocks** (using Moq)
   ```csharp
   var mockService = new Mock<IService>();
   mockService.Setup(x => x.GetData(It.IsAny<int>()))
              .ReturnsAsync(Fin<Data>.Succ(testData));
   ```

3. **Create Fakes** (for complex scenarios)
   ```csharp
   public class FakeRepository : IRepository
   {
       private readonly List<Entity> _data = new();
       
       public Task<Fin<Entity>> GetById(Id id)
       {
           var entity = _data.FirstOrDefault(x => x.Id == id);
           return Task.FromResult(
               entity != null 
                   ? Fin<Entity>.Succ(entity)
                   : Fin<Entity>.Fail("Not found"));
       }
   }
   ```

**Output**: Test doubles ready for use

---

### 5. Design Parameterized Tests

**Trigger**: "Test multiple scenarios" or "Test with different inputs"

**Steps**:

1. **Identify Parameters**
   ```csharp
   [TestCase(0, false)]  // Minimum
   [TestCase(1, true)]   // Valid
   [TestCase(100, true)] // Maximum
   [TestCase(101, false)] // Over maximum
   public async Task ValidateRange_VariousInputs_ReturnsExpected(
       int value, bool expectedValid)
   {
       var result = await Validate(value);
       result.IsValid.Should().Be(expectedValid);
   }
   ```

2. **Create Test Data Source**
   ```csharp
   public static IEnumerable<TestCaseData> InvalidPositions()
   {
       yield return new TestCaseData(-1, 0).SetName("Negative X");
       yield return new TestCaseData(0, -1).SetName("Negative Y");
       yield return new TestCaseData(999, 0).SetName("X Out of Bounds");
   }
   
   [TestCaseSource(nameof(InvalidPositions))]
   public async Task MoveBlock_InvalidPosition_Fails(int x, int y)
   ```

**Output**: Parameterized tests covering multiple scenarios

---

## Test Patterns Reference

### Standard Test Structure
```csharp
[Test]
public async Task MethodName_Scenario_ExpectedOutcome()
{
    // Arrange - 20% of test
    var input = CreateTestInput();
    
    // Act - 10% of test  
    var result = await SystemUnderTest.Method(input);
    
    // Assert - 70% of test
    result.Should().MatchExpectation();
}
```

### Common Assertion Patterns
```csharp
// Success/Failure
result.IsSucc.Should().BeTrue();
result.FailureOption.Should().BeNone();

// Equality
actual.Should().Be(expected);
actual.Should().BeEquivalentTo(expected);

// Collections
collection.Should().HaveCount(3);
collection.Should().Contain(x => x.Id == id);
collection.Should().BeInAscendingOrder(x => x.Name);

// Exceptions
act.Should().Throw<T>();
act.Should().NotThrow();
```

---

## Quality Checklist

Before test is complete:
- [ ] Test has clear, descriptive name?
- [ ] Tests ONE behavior only?
- [ ] Will fail without implementation?
- [ ] Failure message is helpful?
- [ ] No dependency on other tests?
- [ ] Edge cases considered?
- [ ] Mocks properly configured?

---

## Common Pitfalls to Avoid

1. **Testing Implementation**: Test behavior, not how it's done
2. **Multiple Behaviors**: One test should verify one thing
3. **Unclear Names**: Test name should explain what and why
4. **Missing Edge Cases**: Always consider boundaries
5. **Over-Mocking**: Only mock external dependencies

---

## Integration Points

### With Commander (You)
- Clarify requirements
- Get approval on test names
- Suggest edge cases
- Explain test intent

### With Dev Engineer
- Hand off failing tests
- Tests define the contract
- Clear about expectations

### With QA Engineer  
- Unit tests focus on logic
- Integration tests focus on flow
- Clear separation of concerns

---

## Response Templates

### When test created:
"📝 Failing test created: [TestName]

Tests: [Behavior being validated]
Currently fails with: [Error message]

Test location: tests/BlockLife.Core.Tests/[Path]

Additional edge cases to consider:
1. [Edge case 1]
2. [Edge case 2]

Ready for GREEN phase implementation."

### When clarification needed:
"❓ Need clarification on requirement:

You said: '[Requirement]'

Questions:
1. [Specific question]
2. [Another question]

This affects how I structure the test."

### When suggesting test cases:
"💡 Suggested test cases for [Feature]:

Core tests (must have):
1. [Happy path test]
2. [Basic validation test]

Edge cases (should have):
1. [Boundary test]
2. [Error test]

Advanced (nice to have):
1. [Concurrency test]
2. [Performance test]

Which should I create first?"