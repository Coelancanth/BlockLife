# Comprehensive Development Workflow for BlockLife

## Executive Summary

This document establishes a rigorous, test-driven development workflow that integrates Vertical Slice Architecture (VSA), Clean Architecture principles, and functional programming patterns. The workflow is designed to maintain architectural integrity while delivering features incrementally with high confidence.

**Core Principles:**
- Test-Driven Development (TDD) at every layer
- Vertical Slice Architecture for feature isolation
- Functional programming with LanguageExt for error handling
- Strict architectural boundaries enforced by tests
- Continuous quality gates throughout development

## 1. Feature Development Lifecycle

### 1.1 Feature Discovery Phase

#### Step 1: User Story Definition
```markdown
# Feature: [Name]
As a [role]
I want [feature]
So that [benefit]

Acceptance Criteria:
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3
```

#### Step 2: Vertical Slice Analysis
```markdown
# Vertical Slice Components
- **Domain**: What entities/value objects?
- **Commands**: What state changes?
- **Queries**: What data reads?
- **Effects**: What side effects?
- **Presenter**: What coordination logic?
- **View**: What UI capabilities?
```

#### Step 3: Risk Assessment
```markdown
# Technical Risk Matrix
| Component | Risk Level | Mitigation Strategy |
|-----------|------------|-------------------|
| Domain Logic | Low/Med/High | Strategy |
| Integration | Low/Med/High | Strategy |
| Performance | Low/Med/High | Strategy |
```

### 1.2 TDD Implementation Phase

#### Step 1: Write Architecture Fitness Tests
```csharp
// tests/Architecture/Features/[Feature]ArchitectureTests.cs
[Fact]
public void Feature_Commands_Should_Be_Immutable()
{
    var commandTypes = Types.InAssembly(typeof(PlaceBlockCommand).Assembly)
        .That().HaveNameEndingWith("Command")
        .And().ResideInNamespace("Features.[Feature]");
        
    commandTypes.Should()
        .BeRecords()
        .And.HaveOnlyInitProperties();
}

[Fact]
public void Feature_Handlers_Should_Return_Fin()
{
    var handlerTypes = Types.InAssembly(typeof(PlaceBlockCommandHandler).Assembly)
        .That().HaveNameEndingWith("Handler");
        
    handlerTypes.Should()
        .HaveMethod("Handle")
        .WithReturnType(typeof(Fin<>));
}
```

#### Step 2: Write Domain Property Tests
```csharp
// tests/Properties/[Feature]PropertyTests.cs
[Property]
public Property Feature_Domain_Invariants_Hold()
{
    return Prop.ForAll(
        BlockLifeGenerators.ValidGridPosition(),
        BlockLifeGenerators.BlockType(),
        (position, type) =>
        {
            var block = new Block(Guid.NewGuid(), position, type, DateTime.UtcNow);
            
            // Invariants
            return block.Position.IsWithinBounds(GridSize) &&
                   block.Type.IsValid() &&
                   block.CreatedAt <= DateTime.UtcNow;
        }
    );
}
```

#### Step 3: Write Unit Tests for Commands
```csharp
// tests/Features/[Feature]/Commands/[Command]HandlerTests.cs
public class PlaceBlockCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var handler = new TestHandlerBuilder()
            .WithValidPosition()
            .WithEmptyGrid()
            .Build();
            
        var command = new PlaceBlockCommand(new Vector2Int(1, 1));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSucc.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(-1, 0, "INVALID_POSITION")]
    [InlineData(0, -1, "INVALID_POSITION")]
    [InlineData(100, 0, "INVALID_POSITION")]
    public async Task Handle_InvalidPosition_ReturnsExpectedError(
        int x, int y, string expectedError)
    {
        // Test boundary conditions
    }
}
```

#### Step 4: Implement Domain Logic (Red-Green-Refactor)
```csharp
// src/Features/[Feature]/Commands/PlaceBlockCommand.cs
public sealed record PlaceBlockCommand(
    Vector2Int Position,
    BlockType Type = BlockType.Basic
) : ICommand;

// src/Features/[Feature]/Commands/PlaceBlockCommandHandler.cs
public class PlaceBlockCommandHandler : IRequestHandler<PlaceBlockCommand, Fin<Unit>>
{
    // TDD: Write minimal code to pass tests
    public async Task<Fin<Unit>> Handle(
        PlaceBlockCommand request, 
        CancellationToken cancellationToken)
    {
        return await (
            from validPosition in ValidatePosition(request.Position)
            from emptyPosition in ValidateEmpty(request.Position)
            from block in CreateBlock(request)
            from placed in PlaceBlock(block)
            from effect in QueueEffect(block)
            select Unit.Default
        ).AsTask();
    }
}
```

#### Step 5: Write Integration Tests
```csharp
// tests/Integration/[Feature]IntegrationTests.cs
[Fact]
public async Task Feature_EndToEnd_WorksCorrectly()
{
    // Arrange
    var services = new ServiceCollection();
    GameStrapper.Initialize(services);
    var provider = services.BuildServiceProvider();
    
    var mediator = provider.GetRequiredService<IMediator>();
    var gridState = provider.GetRequiredService<IGridStateService>();
    
    // Act
    var command = new PlaceBlockCommand(new Vector2Int(5, 5));
    var result = await mediator.Send(command);
    
    // Assert
    result.IsSucc.Should().BeTrue();
    gridState.GetBlockAt(new Vector2Int(5, 5)).IsSome.Should().BeTrue();
}
```

### 1.3 Presenter Development Phase

#### Step 1: Define View Interface
```csharp
// src/Features/[Feature]/Views/I[Feature]View.cs
public interface IGridView
{
    // Capabilities, not implementation details
    Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type);
    Task HideBlockAsync(Guid blockId);
    IObservable<Vector2Int> CellClicked { get; }
}
```

#### Step 2: Write Presenter Tests
```csharp
// tests/Features/[Feature]/Presenters/[Feature]PresenterTests.cs
[Fact]
public async Task Presenter_UserClick_SendsCommand()
{
    // Arrange
    var mockView = new Mock<IGridView>();
    var mockMediator = new Mock<IMediator>();
    var clickSubject = new Subject<Vector2Int>();
    
    mockView.Setup(v => v.CellClicked).Returns(clickSubject);
    mockMediator.Setup(m => m.Send(It.IsAny<PlaceBlockCommand>(), default))
        .ReturnsAsync(FinSucc(Unit.Default));
    
    var presenter = new GridPresenter(mockView.Object, mockMediator.Object);
    await presenter.InitializeAsync();
    
    // Act
    clickSubject.OnNext(new Vector2Int(3, 3));
    
    // Assert
    mockMediator.Verify(m => 
        m.Send(It.Is<PlaceBlockCommand>(cmd => 
            cmd.Position == new Vector2Int(3, 3)), 
            default), 
        Times.Once);
}
```

#### Step 3: Implement Presenter
```csharp
// src/Features/[Feature]/Presenters/GridPresenter.cs
public class GridPresenter : PresenterBase<IGridView>,
    INotificationHandler<BlockPlacedNotification>
{
    private readonly IMediator _mediator;
    private readonly CompositeDisposable _subscriptions = new();
    
    public override async Task InitializeAsync()
    {
        View.CellClicked
            .Subscribe(OnCellClicked)
            .DisposeWith(_subscriptions);
    }
    
    private async void OnCellClicked(Vector2Int position)
    {
        var command = new PlaceBlockCommand(position);
        var result = await _mediator.Send(command);
        
        result.Match(
            Succ: _ => { /* Success handled via notification */ },
            Fail: error => HandleError(error)
        );
    }
}
```

### 1.4 Godot View Implementation Phase

#### Step 1: Create View Stub for Testing
```csharp
// godot_project/features/[feature]/[Feature]View.cs
public partial class GridView : Control, IGridView
{
    // Minimal implementation for integration testing
    private readonly Subject<Vector2Int> _cellClicked = new();
    
    public IObservable<Vector2Int> CellClicked => _cellClicked;
    
    public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
    {
        GD.Print($"Showing block {blockId} at {position}");
        return Task.CompletedTask;
    }
}
```

#### Step 2: Write GdUnit4 Integration Tests
```csharp
// tests/Integration/Godot/[Feature]GodotTests.cs
[TestSuite]
public class GridViewIntegrationTests
{
    [Test]
    public async Task GridView_ClickCell_PlacesBlock()
    {
        // Arrange
        var scene = GD.Load<PackedScene>("res://godot_project/features/grid/Grid.tscn");
        var grid = scene.Instantiate<GridView>();
        
        // Act
        grid._Ready();
        await grid.SimulateClick(new Vector2Int(5, 5));
        
        // Assert
        Assert.That(grid.GetBlockCount()).IsEqual(1);
    }
}
```

#### Step 3: Full View Implementation
```csharp
// godot_project/features/[feature]/[Feature]View.cs
public partial class GridView : Control, IGridView, IPresenterContainer<GridPresenter>
{
    [Export] public PackedScene BlockScene { get; set; }
    
    private GridPresenter _presenter;
    private readonly Dictionary<Guid, Node2D> _blocks = new();
    
    public override void _Ready()
    {
        _presenter = SceneRoot.Instance.CreatePresenterFor(this);
    }
    
    public async Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
    {
        var block = BlockScene.Instantiate<Node2D>();
        block.Position = GridToWorld(position);
        AddChild(block);
        _blocks[blockId] = block;
        
        // Animate appearance
        var tween = CreateTween();
        tween.TweenProperty(block, "scale", Vector2.One, 0.3f)
            .From(Vector2.Zero)
            .SetEase(Tween.EaseType.Out);
        await ToSignal(tween, "finished");
    }
}
```

## 2. Quality Gates & Code Review Process

### 2.1 Pre-Commit Quality Gates

#### Automated Checks (Git Hooks)
```bash
# .git/hooks/pre-commit
#!/bin/bash

# 1. Run architecture tests
dotnet test --filter "Category=Architecture" --no-build

# 2. Run unit tests for modified files
dotnet test --filter "FullyQualifiedName~$(git diff --name-only | grep -E '\.cs$')"

# 3. Check code formatting
dotnet format --verify-no-changes

# 4. Run static analysis
dotnet build /p:TreatWarningsAsErrors=true
```

#### Manual Checklist
```markdown
## Pre-Commit Checklist
- [ ] All tests pass (unit, property, architecture)
- [ ] No Godot references in Core project
- [ ] Commands are immutable records
- [ ] Handlers return Fin<T>
- [ ] Error handling uses LanguageExt patterns
- [ ] Logging added for debugging
- [ ] Documentation updated
```

### 2.2 Pull Request Process

#### PR Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature (vertical slice)
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Property tests added/updated
- [ ] Architecture tests pass
- [ ] Integration tests pass
- [ ] GdUnit4 tests pass (if UI changes)

## Checklist
- [ ] Follows Clean Architecture principles
- [ ] No static service locators
- [ ] Constructor injection used
- [ ] Presenter has no mutable state
- [ ] View interface exposes capabilities only
```

#### Code Review Focus Areas
1. **Architecture Compliance**
   - Clean Architecture boundaries respected
   - CQRS pattern properly implemented
   - MVP pattern correctly applied

2. **Functional Programming**
   - Proper use of Fin<T> and Option<T>
   - Immutability maintained
   - Pure functions where possible

3. **Testing Coverage**
   - All four pillars covered
   - Edge cases tested
   - Property tests for invariants

## 3. Build & Deployment Pipeline

### 3.1 Local Development Build
```powershell
# PowerShell build script
function Build-BlockLife {
    param(
        [Parameter()]
        [ValidateSet("Debug", "Release")]
        [string]$Configuration = "Debug",
        
        [switch]$RunTests,
        [switch]$OpenGodot
    )
    
    Write-Host "Building BlockLife ($Configuration)..." -ForegroundColor Cyan
    
    # Clean
    dotnet clean
    
    # Build
    dotnet build --configuration $Configuration
    
    if ($RunTests) {
        Write-Host "Running tests..." -ForegroundColor Green
        
        # Run tests in order of importance
        dotnet test --filter "Category=Architecture" --no-build
        dotnet test --filter "Category=Unit" --no-build
        dotnet test --filter "Category=Property" --no-build
        dotnet test --filter "Category=Integration" --no-build
    }
    
    if ($OpenGodot) {
        Write-Host "Opening in Godot..." -ForegroundColor Yellow
        & $env:GODOT_BIN project.godot
    }
}
```

### 3.2 CI/CD Pipeline
```yaml
# .github/workflows/ci.yml
name: CI Pipeline

on: [push, pull_request]

jobs:
  architecture-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test --filter "Category=Architecture"
      
  unit-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test --filter "Category=Unit"
      
  property-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test --filter "Category=Property"
      
  integration-tests:
    runs-on: windows-latest
    needs: [architecture-tests, unit-tests]
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test --filter "Category=Integration"
```

### 3.3 Release Process
```powershell
# Release checklist script
function New-Release {
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )
    
    # 1. Run all tests
    dotnet test
    
    # 2. Build release
    dotnet build --configuration Release
    
    # 3. Run mutation tests
    dotnet stryker
    
    # 4. Generate documentation
    docfx build
    
    # 5. Create git tag
    git tag -a "v$Version" -m "Release $Version"
    
    # 6. Push tag
    git push origin "v$Version"
}
```

## 4. Feature-Specific Workflows

### 4.1 Core Logic Features (Pure C#)

#### Workflow Steps:
1. **Discovery**
   - Define domain entities
   - Identify commands/queries
   - Map effects and notifications

2. **TDD Implementation**
   - Write property tests for invariants
   - Write unit tests for handlers
   - Implement minimal code to pass
   - Refactor for clarity

3. **Integration**
   - Wire up in GameStrapper
   - Add to SimulationManager
   - Test full command flow

#### Example: Block Movement Feature
```csharp
// 1. Property test
[Property]
public Property Movement_Maintains_Grid_Bounds()
{
    return Prop.ForAll(
        BlockLifeGenerators.Block(),
        BlockLifeGenerators.ValidGridPosition(),
        (block, newPosition) =>
        {
            var moved = block.MoveTo(newPosition);
            return moved.Position.IsWithinBounds(GridSize);
        }
    );
}

// 2. Unit test
[Fact]
public async Task MoveBlock_ValidMove_Success()
{
    // Red: Test fails
    // Green: Implement handler
    // Refactor: Optimize
}

// 3. Integration test
[Fact]
public async Task MoveBlock_FullFlow_UpdatesGridState()
{
    // Test entire command pipeline
}
```

### 4.2 UI-Heavy Features

#### Workflow Steps:
1. **UI Prototype**
   - Create minimal Godot scene
   - Define view interface
   - Stub presenter logic

2. **Presenter TDD**
   - Mock view interface
   - Test presenter logic
   - Handle notifications

3. **View Implementation**
   - Implement interface methods
   - Add animations
   - Polish UX

#### Example: Particle Effects Feature
```csharp
// 1. Interface definition
public interface IParticleEffectView
{
    Task PlayEffectAsync(string effectName, Vector2 position);
    Task StopEffectAsync(string effectName);
}

// 2. Presenter test
[Fact]
public async Task BlockPlaced_PlaysParticleEffect()
{
    var mockView = new Mock<IParticleEffectView>();
    // Test presenter triggers effect
}

// 3. Godot implementation
public partial class ParticleEffectView : Node2D, IParticleEffectView
{
    [Export] public Dictionary<string, PackedScene> Effects { get; set; }
    
    public async Task PlayEffectAsync(string effectName, Vector2 position)
    {
        var effect = Effects[effectName].Instantiate<CPUParticles2D>();
        // Implementation
    }
}
```

### 4.3 Complex Business Rules

#### Workflow Steps:
1. **Rule Specification**
   - Define rule interface
   - Create test scenarios
   - Document edge cases

2. **Property-Based Testing**
   - Generate test cases
   - Verify invariants
   - Test combinations

3. **Rule Implementation**
   - Implement validation
   - Chain rules functionally
   - Cache results if needed

#### Example: Chain Reaction Rules
```csharp
// 1. Rule interface
public interface IChainReactionRule
{
    Fin<IEnumerable<Block>> GetChainTargets(Block source);
}

// 2. Property test
[Property]
public Property ChainReaction_Never_Infinite()
{
    return Prop.ForAll(
        BlockLifeGenerators.GridWithBlocks(),
        grid =>
        {
            var chains = rule.GetChainTargets(grid.First());
            return chains.Count() < grid.Count();
        }
    );
}
```

## 5. Documentation & Knowledge Management

### 5.1 Documentation Structure
```
Docs/
├── 0_Overview/           # High-level architecture
├── 1_Architecture/       # Technical decisions
├── 2_Game_Design/        # Game mechanics
├── 3_Implementation/     # Feature plans
├── 4_Architecture_Decision_Records/  # ADRs
└── 5_Guide/             # Developer guides
```

### 5.2 Documentation Requirements

#### For Each Feature:
1. **Implementation Plan**
   - User story
   - Technical design
   - Risk assessment
   - Success criteria

2. **API Documentation**
   ```csharp
   /// <summary>
   /// Places a block at the specified grid position.
   /// </summary>
   /// <param name="position">Grid position for the block</param>
   /// <param name="type">Type of block to place</param>
   /// <returns>Success or error with reason</returns>
   public Fin<Unit> PlaceBlock(Vector2Int position, BlockType type)
   ```

3. **Architecture Decision Record**
   ```markdown
   # ADR-001: Use LanguageExt for Error Handling
   
   ## Status
   Accepted
   
   ## Context
   Need robust error handling without exceptions
   
   ## Decision
   Use LanguageExt's Fin<T> monad
   
   ## Consequences
   - Explicit error handling
   - Functional composition
   - Learning curve for team
   ```

### 5.3 Knowledge Sharing

#### Code Reviews as Teaching
```csharp
// REVIEW NOTE: This violates Clean Architecture
// The presenter should not know about Godot types
public class BadPresenter : PresenterBase<IView>
{
    // ❌ BAD: Godot reference in presenter
    public void OnNodeReady(Node node) { }
    
    // ✅ GOOD: Use abstraction
    public void OnViewReady(IView view) { }
}
```

#### Pair Programming Sessions
- Feature kickoff pairing
- Complex problem solving
- Knowledge transfer

## 6. Performance & Optimization Workflow

### 6.1 Performance Benchmarking
```csharp
[Benchmark]
public class GridPerformanceBenchmarks
{
    [Params(10, 50, 100)]
    public int GridSize { get; set; }
    
    [Benchmark]
    public async Task PlaceBlocks()
    {
        var grid = new GridStateService(GridSize);
        for (int i = 0; i < GridSize * GridSize; i++)
        {
            await grid.PlaceBlock(GenerateRandomBlock());
        }
    }
}
```

### 6.2 Optimization Process
1. **Measure First**
   - Profile with dotTrace
   - Identify bottlenecks
   - Set performance targets

2. **Optimize Carefully**
   - Maintain architecture
   - Add caching strategically
   - Use spatial indexing

3. **Verify Improvements**
   - Re-run benchmarks
   - Test at scale
   - Monitor in production

## 7. Debugging & Troubleshooting

### 7.1 Structured Logging
```csharp
public class PlaceBlockCommandHandler
{
    public async Task<Fin<Unit>> Handle(PlaceBlockCommand request)
    {
        using var activity = Activity.StartActivity("PlaceBlock");
        activity?.SetTag("position", request.Position);
        activity?.SetTag("type", request.Type);
        
        _logger.LogDebug("Handling {Command} at {Position}", 
            nameof(PlaceBlockCommand), request.Position);
            
        return await ExecuteWithLogging(request);
    }
}
```

### 7.2 Debug Tools
```csharp
#if DEBUG
// Debug command for testing
public sealed record DebugFillGridCommand(int Density) : ICommand;

public class DebugCommandHandler : IRequestHandler<DebugFillGridCommand, Fin<Unit>>
{
    public async Task<Fin<Unit>> Handle(DebugFillGridCommand request)
    {
        // Fill grid with random blocks for testing
    }
}
#endif
```

### 7.3 Error Diagnosis
```csharp
public static class ErrorDiagnostics
{
    public static string GetDetailedError(Error error)
    {
        return error.Code switch
        {
            "POSITION_OCCUPIED" => $"Position {error.Data["position"]} already has block {error.Data["blockId"]}",
            "INVALID_POSITION" => $"Position {error.Data["position"]} is outside grid bounds",
            _ => error.Message
        };
    }
}
```

## 8. Continuous Improvement

### 8.1 Retrospectives After Each Feature
```markdown
## Feature Retrospective: [Feature Name]

### What Went Well
- TDD caught edge cases early
- Property tests found invariant violations
- Clean Architecture made testing easy

### What Could Improve
- Presenter tests were verbose
- Animation timing issues
- Performance regression in grid lookup

### Action Items
- [ ] Create presenter test helpers
- [ ] Add animation test utilities
- [ ] Implement spatial indexing
```

### 8.2 Architecture Fitness Evolution
```csharp
// Add new architecture tests as patterns emerge
[Fact]
public void NewPattern_Should_FollowConvention()
{
    // As you discover new patterns, codify them
    var types = Types.InAssembly(CoreAssembly)
        .That().ImplementInterface<INewPattern>();
        
    types.Should().FollowNewConvention();
}
```

### 8.3 Tool & Process Improvements
- Regular tooling updates
- Build script enhancements
- Test framework upgrades
- Documentation automation

## 9. Emergency Procedures

### 9.1 Production Bug Response
```powershell
# Emergency hotfix workflow
function New-Hotfix {
    param([string]$IssueNumber)
    
    # 1. Create hotfix branch
    git checkout -b "hotfix/issue-$IssueNumber"
    
    # 2. Write failing test
    # 3. Fix issue
    # 4. Run all tests
    dotnet test
    
    # 5. Create PR with expedited review
}
```

### 9.2 Performance Crisis
1. Enable detailed logging
2. Profile with dotTrace
3. Identify bottleneck
4. Apply tactical fix
5. Plan strategic solution

### 9.3 Architecture Violation
1. Add architecture test to prevent recurrence
2. Refactor violation
3. Update documentation
4. Team education session

## 10. Success Metrics

### 10.1 Code Quality Metrics
- **Test Coverage**: >90% for Core
- **Mutation Score**: >80%
- **Architecture Test Pass Rate**: 100%
- **Build Time**: <30 seconds
- **PR Review Time**: <4 hours

### 10.2 Development Velocity
- **Feature Completion Rate**: 1-2 features/sprint
- **Bug Discovery Rate**: <5 per feature
- **Technical Debt Ratio**: <10%
- **Documentation Coverage**: 100% public APIs

### 10.3 Team Health
- **Knowledge Sharing Sessions**: Weekly
- **Pair Programming**: 20% of time
- **Code Review Participation**: 100%
- **Retrospective Action Completion**: >80%

## Conclusion

This workflow provides a comprehensive framework for developing BlockLife with confidence and quality. The combination of TDD, VSA, and Clean Architecture ensures that each feature is:

1. **Testable**: Through strict architectural boundaries
2. **Maintainable**: Through functional programming and immutability
3. **Extensible**: Through vertical slices and CQRS
4. **Reliable**: Through comprehensive testing strategies
5. **Performant**: Through continuous measurement and optimization

Remember: The architecture serves the game, not the other way around. When in doubt, prioritize player experience while maintaining architectural integrity.

---

**Document Version**: 1.0  
**Created**: 2025-08-13  
**Primary Audience**: BlockLife Development Team  
**Review Schedule**: After each major feature completion