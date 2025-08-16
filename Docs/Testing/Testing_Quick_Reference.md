# Testing Quick Reference

## Core Testing Approach

**Goal**: Tests defend architecture and verify business logic  
**Strategy**: TDD with FluentAssertions  
**Integration**: GdUnit4 for Godot components  

## Test Types & When to Use

### Unit Tests (Most Common)
- **What**: Pure business logic, commands, handlers
- **Where**: `tests/BlockLife.Core.Tests/`
- **Speed**: Fast (< 10ms)
- **Pattern**: TDD Red-Green-Refactor

```csharp
[Fact]
public void Handle_WithValidPosition_ShouldPlaceBlock()
{
    // Arrange
    var command = new PlaceBlockCommand { Position = new(0, 0), BlockType = BlockType.Basic };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().BeSuccess();
}
```

### Integration Tests (When Needed)
- **What**: Presenter coordination, Godot scene interaction
- **Where**: `tests/BlockLife.Integration.Tests/`
- **Speed**: Medium (< 100ms)
- **Pattern**: GdUnit4 SimpleSceneTest (see GdUnit4 guide)

### Architecture Tests (Always Required)
- **What**: Ensure patterns are followed
- **When**: Every feature implementation

```csharp
[Fact]
public void Commands_ShouldBeImmutableRecords()
{
    var commandTypes = GetAllCommandTypes();
    foreach (var type in commandTypes)
    {
        type.Should().BeRecord("commands must be immutable");
    }
}
```

## Essential Patterns

### 1. FluentAssertions (Required)
```csharp
// ✅ Good
result.Should().BeSuccess("because command was valid");
blocks.Should().HaveCount(1, "because one block was placed");

// ❌ Bad
Assert.True(result.IsSuccess);
Assert.Equal(1, blocks.Count);
```

### 2. Test Builders (For Complex Setup)
```csharp
public class BlockBuilder
{
    public Block WithPosition(Vector2Int position) { /* ... */ }
    public Block Build() { /* ... */ }
}

// Usage
var block = new BlockBuilder().WithPosition(new(0, 0)).Build();
```

### 3. Error Testing
```csharp
[Fact]
public void Handle_WithInvalidPosition_ShouldReturnError()
{
    // Arrange
    var command = new PlaceBlockCommand { Position = new(-1, -1) };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().BeFail("because position is invalid");
    result.IfFail(error => error.Message.Should().Contain("position"));
}
```

## File Organization

```
tests/
├── BlockLife.Core.Tests/          # Unit tests
│   └── Features/
│       └── Block/
│           ├── PlaceBlockCommandHandlerTests.cs
│           └── Rules/
│               └── PositionValidRuleTests.cs
└── BlockLife.Integration.Tests/   # Integration tests
    └── Features/
        └── Block/
            └── BlockPlacementIntegrationTests.cs
```

## Quick Commands

```bash
# Run all tests
dotnet test

# Run specific feature tests  
dotnet test --filter "PlaceBlock"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Common Test Categories

- **Happy Path**: Valid inputs, expected success
- **Validation**: Invalid inputs, expected errors  
- **Edge Cases**: Boundary conditions, null values
- **Integration**: Multi-component interactions

## Reference Implementations

**Copy these test patterns:**
- `PlaceBlockCommandHandlerTests.cs` - Command handler testing
- `PositionValidRuleTests.cs` - Validation rule testing
- Architecture fitness tests - Pattern compliance

## When Writing Tests

1. **Start with failing test** (Red)
2. **Write minimal code** to pass (Green)  
3. **Refactor** while keeping green
4. **Use descriptive test names**: `Method_Scenario_ExpectedResult`
5. **One assertion per concept** (multiple Should() calls OK)

---

For **integration testing specifics**, see [GdUnit4_Integration_Testing_Guide.md](GdUnit4_Integration_Testing_Guide.md)