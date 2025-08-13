# Mutation Testing Strategy

## Tool Selection: Stryker.NET

### Why Stryker.NET
- Native C# support with excellent xUnit integration
- Supports .NET 8 and LanguageExt functional constructs
- Can target specific assemblies (perfect for Core-only testing)
- HTML reports show exactly which mutations survived

## Implementation Plan

### Phase 1: Core Business Logic Only
Target high-value areas first:

```bash
# Install Stryker
dotnet tool install -g dotnet-stryker

# Run on Core project only (not Godot layer)
cd tests
dotnet stryker --project-file "../src/BlockLife.Core.csproj"
```

### Configuration (.stryker-config.json)
```json
{
  "stryker-config": {
    "project": "BlockLife.Core.csproj",
    "test-projects": ["BlockLife.Core.Tests.csproj"],
    "mutation-level": "Standard",
    "threshold-high": 80,
    "threshold-low": 60,
    "threshold-break": 50,
    "mutate": [
      "src/Features/**/*.cs",
      "!src/Features/**/*DTO.cs",
      "!src/Features/**/*Notification.cs"
    ],
    "reporters": ["html", "console", "dashboard"]
  }
}
```

### High-Priority Mutation Targets

1. **Command Handlers** - Validation logic
   ```csharp
   // Stryker will mutate these conditions
   if (command.Position.X < 0 || command.Position.Y < 0)  // Changes to >= 
       return FinFail<Unit>(Error.New("INVALID_POS"));
   ```

2. **Business Rules** - Domain constraints
   ```csharp
   // Mutations will test boundary conditions
   public bool CanPlaceBlock(Position pos) =>
       pos.X >= 0 && pos.X < GridWidth &&  // Boundary mutations
       pos.Y >= 0 && pos.Y < GridHeight;
   ```

3. **Error Handling** - Fin<T> chains
   ```csharp
   return ValidatePosition(pos)
       .Bind(ValidateNoOverlap)  // Will remove Bind to test coverage
       .Map(ApplyPlacement);
   ```

### Mutation Types to Enable

Essential for your architecture:
- **Conditional boundaries**: `<` → `<=`, `>` → `>=`
- **Boolean substitutions**: `&&` → `||`
- **Linq mutations**: `Any()` → `All()`, `Where()` removal
- **Return value mutations**: `FinSucc` → `FinFail`

### Interpreting Results

#### Good Mutation Score Targets
- **Handlers**: 85%+ (critical business logic)
- **Validators**: 90%+ (your defensive line)
- **DTOs/Notifications**: Skip (no logic to mutate)
- **Presenters**: 70%+ (coordination logic)

#### Red Flags in Reports
1. **Survived mutations in validation** → Missing negative test cases
2. **Survived boundary mutations** → Off-by-one errors waiting to happen
3. **Survived Bind/Map removals** → Error paths untested

### Integration with CI/CD

```yaml
# .github/workflows/mutation-tests.yml
name: Mutation Testing
on:
  pull_request:
    paths:
      - 'src/Features/**/*.cs'
      - 'tests/**/*.cs'

jobs:
  stryker:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - name: Run Stryker
        run: |
          dotnet tool install -g dotnet-stryker
          dotnet stryker --threshold-break 75
      - name: Upload Report
        uses: actions/upload-artifact@v3
        with:
          name: stryker-report
          path: StrykerOutput/**/reports/
```

## Mutation Testing Patterns for Clean Architecture

### Pattern 1: Handler Validation Coverage
```csharp
[Fact]
public void MoveBlockHandler_Rejects_Negative_Position()
{
    // This test will kill mutations that change < to <=
    var command = new MoveBlockCommand { Position = new(-1, 0) };
    var result = await handler.Handle(command);
    
    result.IsFail.Should().BeTrue();
    result.FailValue.Code.Should().Be("INVALID_POS");
}

[Fact]
public void MoveBlockHandler_Accepts_Zero_Position()
{
    // This kills the mutation of < to <=
    var command = new MoveBlockCommand { Position = new(0, 0) };
    var result = await handler.Handle(command);
    
    result.IsSucc.Should().BeTrue();
}
```

### Pattern 2: Fin<T> Chain Coverage
```csharp
[Fact]
public void Handler_Stops_On_First_Validation_Failure()
{
    // Ensures Bind chain short-circuits properly
    var command = new InvalidCommand();
    
    var result = await handler.Handle(command);
    
    // Verify ONLY first validation error returned
    result.FailValue.Code.Should().Be("FIRST_VALIDATION");
    validatorTwo.Verify(v => v.Validate(It.IsAny<>()), Times.Never);
}
```

### Pattern 3: Business Rule Boundaries
```csharp
[Theory]
[InlineData(0, 0, true)]      // Lower boundary
[InlineData(9, 9, true)]      // Upper boundary  
[InlineData(10, 10, false)]   // Just outside
[InlineData(-1, -1, false)]   // Negative
public void GridBounds_Check_Handles_Boundaries(int x, int y, bool expected)
{
    var result = grid.IsValidPosition(new Position(x, y));
    result.Should().Be(expected);
}
```

## Anti-Patterns to Avoid

1. **Don't mutate DTOs** - No logic to test
2. **Don't mutate generated code** - Waste of cycles
3. **Don't aim for 100%** - Diminishing returns after 85%
4. **Don't mutate logging statements** - Not business critical

## Rollout Strategy

### Week 1: Baseline
- Install Stryker
- Run on MoveBlockHandler only
- Establish baseline score (likely 60-70%)

### Week 2: Improve Coverage
- Add tests to kill survived mutations
- Focus on boundary conditions
- Target 85% score for this handler

### Week 3: Expand Scope
- Add more handlers to mutation testing
- Create mutation test policy for new code
- Integrate into PR checks

### Month 2: Full Integration
- All handlers under mutation testing
- CI/CD gate at 75% minimum
- Team trained on interpreting reports

## Expected ROI

Based on your recent bugs:
- **Fin<T> ambiguity**: Would be caught by return type mutations
- **Validation gaps**: Directly caught by conditional mutations
- **Error format issues**: String mutations would reveal untested error paths

Estimated bug prevention: 30-40% reduction in logic errors reaching production.