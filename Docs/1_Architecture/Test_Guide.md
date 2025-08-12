# **Project Test Guide (v2.0, Aligned with Architecture Guide v2.9)**

## 1. Core Philosophy

This guide is the **mandatory companion enforcement strategy** for the **Project Architecture Guide**. Our tests do not merely verify features; their core mission is to **defend the architecture**. Every test written must be traceable to the architectural principle it upholds. A test that does not validate an architectural rule is a test that misses the point and provides a false sense of security.

This testing strategy is the primary mechanism to prevent "architecture rot" and ensure the long-term health, maintainability, and integrity of the codebase. **Violation of this guide is equivalent to a violation of the architecture.**

**The Pragmatic View**: This rigorous approach will increase initial development time and may lead to more "fragile" tests (i.e., tests that fail when legitimate changes are made). We **consciously accept this trade-off**. This "fragility" is, in fact, "sensitivity"—it forces a micro-architectural review with every modification, ensuring absolute precision and strong defense of business logic and architectural rules. For rapid prototyping, a relaxed approach to "precise effect assertion" might be acceptable, but strict adherence is mandatory once formal development begins.

## 2.1 The Test Pyramid (Our Specific Practice)

Our test pyramid is not a generic theory; it is a practical map of what to test, where, and why, specifically for our architecture.

```mermaid
graph TD
    subgraph Test Suite & Scope
        direction LR
        A(E2E Tests);
        B(Integration Tests);
        C(Unit Tests);
    end
    subgraph Coverage & Purpose
        direction BT
        D[Few, Extremely Slow<br><b>Verifies:</b> Critical user flows];
        E[More, Medium Speed<br><b>Verifies:</b> Architecture's "wiring" (DI, Lifecycle), <br><b>Presenter coordination logic</b>, <br>View-to-Controller delegation];
        F[Many, Extremely Fast<br><b>Verifies:</b> Business logic, Data transformation, <br>Model state transitions];
    end
    
    A -- validates --> D;
    B -- validates --> E;
    C -- validates --> F;
```

-   **Unit Tests (The Bedrock):** Our foundation. They run in-memory, detached from the Godot engine, and are extremely fast. They are responsible for validating the **four core pillars** of our application logic.
-   **Integration Tests (The Presentation Layer Guardian):** Our connection and coordination logic verifiers. They run within the Godot environment and are medium speed. Their **core responsibility** has been expanded to:
    1.  Verify the architecture's "wiring" correctness (DI container, lifecycle).
    2.  **Verify the Presenter's coordination logic**: Ensure the Presenter correctly calls its dependent Controller Nodes and Godot APIs when receiving notifications or events.
    3.  Verify that the View and Controller Nodes themselves behave as expected.
-   **End-to-End (E2E) Tests (The Safety Net):** Our final guarantee. Used sparingly, only to simulate user actions and validate critical, must-not-fail gameplay loops from start to finish.

## 2.2 The Builder Pattern
```csharp
// In Tests/Builders/GridStateBuilder.cs
public class GridStateBuilder
{
    private readonly GridState _gridState = new();

    public GridStateBuilder WithBlock(Guid id, int level, Vector2Int position)
    {
        var block = new BlockEntity(id, level);
        _gridState.AddBlock(block, position);
        return this;
    }

    public GridStateBuilder WithBlock(int level, Vector2Int position)
    {
        return WithBlock(Guid.NewGuid(), level, position);
    }

    public GridState Build()
    {
        return _gridState;
    }
}

// Usage in a test:
var gridState = new GridStateBuilder()
    .WithBlock(level: 2, at: new Vector2Int(1, 1))
    .WithBlock(level: 4, at: new Vector2Int(1, 2))
    .Build();
```

## 3. Unit Tests: The Three Pillars

**Architectural Validation:** This entire unit test suite is the ultimate proof of **Architecture Rule #3 (`NO using Godot; IN HANDLERS OR SERVICES`)**. The ability of these tests to compile and run without the Godot engine is the highest form of validation for our core logic's decoupling.

### **Pillar 1: Testing Authoritative Write Model (The Foundation of State)**

-   **Goal:** Verify the absolute correctness of the model's internal state transition logic.
-   **Architectural Validation:** Rule #1 (Single Authoritative Write Model).
-   **Testing Contract:**
    > Tests for the authoritative write model (e.g., `GridState`) **must** verify that after an operation is performed, its internal state is **exactly** as expected. It does not concern itself with `Effect`s or `Result`s; its sole focus is the **final correctness of the state itself**. This ensures the "ground truth" of our application is always sound.

#### **Example: `GridStateTests`**

```csharp
// In Tests/Models/GridStateTests.cs
public class GridStateTests
{
    [Fact]
    public void AddBlock_WhenPositionIsEmpty_AddsBlockAndReturnsTrue()
    {
        // Arrange
        var gridState = new GridState(width: 5, height: 5);
        var block = new BlockEntity(Guid.NewGuid(), 1);
        var position = new Vector2Int(2, 2);

        // Act
        bool result = gridState.AddBlock(block, position);

        // Assert
        result.Should().BeTrue();
        gridState.GetBlockAt(position).Should().Be(block);
        gridState.TotalBlocks.Should().Be(1);
    }

    [Fact]
    public void AddBlock_WhenPositionIsOccupied_DoesNotAddBlockAndReturnsFalse()
    {
        // Arrange
        var gridState = new GridStateBuilder()
            .WithBlock(level: 1, at: new Vector2Int(2, 2))
            .Build();
        var newBlock = new BlockEntity(Guid.NewGuid(), 2);

        // Act
        bool result = gridState.AddBlock(newBlock, new Vector2Int(2, 2));

        // Assert
        result.Should().BeFalse();
        gridState.GetBlockAt(new Vector2Int(2, 2)).Level.Should().Be(1); // Still the old block
        gridState.TotalBlocks.Should().Be(1);
    }
}
```

### **Pillar 2: Testing Command Handlers (The Core of Write Operations)**

-   **Goal:** Verify that a `CommandHandler`, given a specific state, makes the correct **decision**, and produces the **precise, complete, and correctly ordered set of side-effects (`IEffect`)**.
-   **Architectural Validation:** Rule #1 (Single Write Point), Rule #7 (`Result` Return), Rule #8 (Triggering Effects).
-   **Testing Contract:**
    > A `CommandHandler`'s unit test **must and only** assert two things:
    > 1.  **The final `Result`**: Was the operation successful (`IsSuccess`) or failed (`IsFailed`)?
    > 2.  **The produced `IEffect` collection**: Use `BeEquivalentTo` to assert the collection of produced effects, ensuring their **type, content, and quantity** are **exactly** as expected.
    > **Strictly forbidden** is asserting the final state of the model within a handler's test. That is the sole responsibility of `ModelTests` (Pillar 1). This prevents "placebo tests" and ensures that a failing test points with laser precision to the handler's decision-making logic.

#### **The `EffectCollector` Utility**
To facilitate clean and robust effect assertion, we use a simple utility class that captures all produced effects for easy inspection.

```csharp
// In Tests/Utils/EffectCollector.cs
public class EffectCollector : IEffectCollector
{
    private readonly List<IEffect> _effects = new();
    public void Add(IEffect effect) => _effects.Add(effect);

    // Add a getter to allow flexible assertions
    public IReadOnlyList<IEffect> GetEffects() => _effects.AsReadOnly();

    public void ShouldHaveProducedNothing() => _effects.Should().BeEmpty();
}
```

#### **Example: `MergeBlocksCommandHandlerTests`**

```csharp
// In a CommandHandler test file
public class MergeBlocksCommandHandlerTests
{
    private readonly EffectCollector _effectCollector;
    private readonly GridState _gridState;
    private readonly MergeBlocksCommandHandler _handler;

    public MergeBlocksCommandHandlerTests()
    {
        _effectCollector = new EffectCollector();
        // Use a builder for clean test setup
        _gridState = new GridStateBuilder()
            .WithBlock(id: TestGuids.BlockA, level: 1, at: new Vector2Int(0, 0))
            .WithBlock(id: TestGuids.BlockB, level: 1, at: new Vector2Int(0, 1))
            .Build();
            
        _handler = new MergeBlocksCommandHandler(_gridState, _effectCollector);
    }

    [Fact]
    public void Handle_WhenBlocksCanMerge_ReturnsSuccessAndProducesPreciseEffects()
    {
        // Arrange
        var command = new MergeBlocksCommand(TestGuids.BlockA, TestGuids.BlockB);
        var expectedEffects = new IEffect[]
        {
            new BlockRemovedEffect(TestGuids.BlockB),
            new BlockTransformedEffect(TestGuids.BlockA, newLevel: 2)
        };

        // Act
        var result = _handler.Handle(command, CancellationToken.None).Result;

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // ARCHITECT'S NOTE: Use BeEquivalentTo for PRECISE matching.
        // This is a strict contract. It ensures no unexpected side effects are produced.
        // It checks type, content, and count. Order is not checked by default, which is robust.
        _effectCollector.GetEffects().Should().BeEquivalentTo(expectedEffects);
    }

    [Fact]
    public void Handle_WhenBlocksCannotMerge_ReturnsFailureAndProducesNoEffects()
    {
        // Arrange
        // Create a non-mergeable scenario
        var unmergeableState = new GridStateBuilder()
            .WithBlock(id: TestGuids.BlockA, level: 1, at: new Vector2Int(0, 0))
            .WithBlock(id: TestGuids.BlockC, level: 2, at: new Vector2Int(0, 1))
            .Build();
        var localHandler = new MergeBlocksCommandHandler(unmergeableState, _effectCollector);
        var command = new MergeBlocksCommand(TestGuids.BlockA, TestGuids.BlockC);

        // Act
        var result = localHandler.Handle(command, CancellationToken.None).Result;

        // Assert
        result.IsFailed.Should().BeTrue();
        // Assert the specific error if possible
        result.Error.Should().Be(Error.New("Blocks cannot merge."));

        // It is CRITICAL to assert that no side effects were produced on failure.
        _effectCollector.ShouldHaveProducedNothing();
    }
}
```

### **Pillar 3: Testing Query Handlers (The Core of Read Operations)**

-   **Goal:** Verify that data is safely and accurately read from the model and transformed into the correct Data Transfer Object (DTO).
-   **Architectural Validation:** Rule #13 (Formalize Queries), Rule #11 (DTO Data Contracts).
-   **Testing Contract:**
    > `QueryHandler` tests **must** verify that the returned `Result<DTO>` is successful, and that the contained DTO data **precisely matches** the current state of the source model. Use `BeEquivalentTo` for DTO comparison to ensure all properties are correctly mapped.

#### **Example: `GetGridStateQueryHandlerTests`**

```csharp
// In a QueryHandler test file
public class GetGridStateQueryHandlerTests
{
    [Fact]
    public void Handle_WhenQueried_ReturnsCorrectSnapshotFromState()
    {
        // Arrange
        var gridState = new GridStateBuilder()
            .WithBlock(level: 2, at: new Vector2Int(1, 1))
            .WithBlock(level: 4, at: new Vector2Int(3, 3))
            .Build();
        
        var query = new GetGridStateQuery();
        var handler = new GetGridStateQueryHandler(gridState);

        // Act
        var result = handler.Handle(query, CancellationToken.None).Result;

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Success(); // Extract DTO from Result
        
        var expectedDto = new GridSnapshotDto(
            Width: gridState.Width,
            Height: gridState.Height,
            Blocks: new[]
            {
                new BlockDto(Level: 2, Position: new Vector2Int(1, 1)),
                new BlockDto(Level: 4, Position: new Vector2Int(3, 3))
            }
        );
        
        dto.Should().BeEquivalentTo(expectedDto);
    }
}
```

## 4. Integration Tests: Verifying "Wiring" and "Presentation Logic"

-   **Goal:** In a running Godot scene, verify the correct connection of architectural components, and **verify the core logic of the Presenter as a coordinator**.
-   **Architectural Validation:** Rule #10 (DI), Rule #12 (Lifecycle), Rule #16 (Humble Presenter), and all `GetNode` logic within `View` and `Controller` implementations.
-   **Testing Contract:**
    > Integration tests **strictly forbid** testing any pure business logic (that is the responsibility of unit tests). Their assertions **must focus on** the following high-value objectives:
    > 1.  **DI & Lifecycle**: Can the PresenterFactory successfully create the Presenter? Is the Presenter correctly created and disposed with the View's lifecycle?
    > 2.  **Presenter Coordination Logic (Core)**:
    >     *   **Model -> View Direction**: When simulating a `Notification` (by directly calling the Presenter's `Handle` method), does the Presenter correctly call methods on its `View` or `Controller Node` dependencies? (e.g., was `AnimationPlayer.Play` called?)
    >     *   **View -> Model Direction**: When simulating a user input (e.g., `Button.EmitSignal("pressed")`), does the Presenter send the correct `Command` or `Query` to the `IMediator`?
    > 3.  **Scene Tree Dependencies**: Can `GetNode` successfully find expected nodes within the actual test scene?

#### **Example: `PlayerSkillPresenterIntegrationTests`**

This new example demonstrates how to test a "Humble Presenter".

```csharp
// In Tests/Integration/Player/Skills/PlayerSkillPresenterIntegrationTests.cs
// This test MUST be run with a test runner like GdUnit that can load Godot scenes.
[TestSuite]
public class PlayerSkillPresenterIntegrationTests
{
    private Mock<IMediator> _mockMediator;
    private PlayerSkillPresenter _presenter;
    private PlayerSkillView _view;
    private AnimationController _animController; // The real controller node
    private SceneRunner _sceneRunner;

    [BeforeEach]
    public async Task Setup()
    {
        // Arrange
        _mockMediator = new Mock<IMediator>();

        // 1. Load the actual scene that contains the View and its Controller nodes.
        _sceneRunner = new GdUnit4.Executions.SceneRunner("res://tests/integration/Player/Skills/PlayerSkillTestScene.tscn");
        _view = _sceneRunner.FindChild<PlayerSkillView>();
        
        // For testing, we need a way to inject the mock mediator.
        // The PresenterFactory should be adapted to allow overriding dependencies for tests.
        // Or, the view can have a test-only method to set up the presenter.
        // For simplicity, let's assume a test-only setup method on the view.
        _presenter = _view.SetupPresenterForTest(_mockMediator.Object);
        
        // Get the real controller node for verification
        _animController = _view.GetAnimationController();
    }

    [Test]
    public async Task ActivateMeteorStrike_CoordinatesTheFullSequenceCorrectly()
    {
        // Arrange
        var targetPosition = new Vector2(100, 100);
        
        // We can spy on the AnimationPlayer to see if it was called.
        var animPlayer = _animController.GetNode<AnimationPlayer>("AnimationPlayer");
        var spy = new GdUnit4.Api.Spies.MethodSpy(animPlayer, AnimationPlayer.MethodName.Play);

        // Act
        // We directly call the public method on the presenter, just like the game would.
        await _presenter.ActivateMeteorStrike(targetPosition);

        // Assert
        // 1. Verify presentation logic delegation: Was the animation played?
        spy.ShouldHaveBeenCalledWith("Cast_Meteor");

        // 2. Verify command sending: Was the correct command sent at the end?
        _mockMediator.Verify(m => m.Send(
            It.Is<DealAreaDamageCommand>(cmd => cmd.TargetPosition == targetPosition && cmd.Damage == 50),
            It.IsAny<CancellationToken>()
        ), Times.Once());
        
        // ... other assertions for VFX, SFX controllers ...
    }
}
```

## 5. Quality Gates (Code Review Checklist)

During code review, every test file must be scrutinized against this guide and the overarching architecture.

1.  **Component Responsibility**: Is this test for a `Model`, `CommandHandler`, `QueryHandler`, or `Presenter`? Do its assertions **strictly adhere** to that component's "Testing Contract"?
2.  **Handler Test Contract**: Does the test use `BeEquivalentTo` to assert the **precise `IEffect` collection**? Does it test the failure path and assert **zero effects** (`ShouldHaveProducedNothing`)?
3.  **Presenter Test Contract**:
    *   **Is the test an INTEGRATION TEST running in a Godot scene?** (e.g., using GdUnit).
    *   For model notifications, does it verify the **correct methods were called on real `Controller Nodes`** (e.g., `AnimationPlayer.Play`)?
    *   For view events, does it verify the **correct `Command` or `Query` was sent to a mocked `IMediator`**?
4.  **Architectural Boundary**:
    *   Can the `Model`, `Handler`, `Service`, and `Query` tests compile and run **without the Godot engine**? If not, it violates **Architecture Rule #3**.
    *   Does the `Presenter` test correctly run **within the Godot engine**?
5.  **Mutation Test Mindset**: If I deliberately break the production code this test covers (e.g., change a condition, remove an effect), **will this test fail?** If the answer is no, the test's assertions are too weak and provide a false sense of security. It must be strengthened.
6.  **Test Structure (AAA)**: Does the test clearly follow the `Arrange-Act-Assert` structure? Is each section single-responsibility?
7.  **(New) Humble Presenter Scrutiny**: Does the Presenter under test appear to be doing too much? As a heuristic, if a Presenter's method exceeds 20 lines or contains complex control flow, does it appropriately delegate that complexity to a `Controller Node` or a pure C# `Service`? The test should reflect this delegation.