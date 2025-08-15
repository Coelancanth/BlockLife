# Feature Development Guide

## Overview
This guide provides comprehensive instructions for implementing new features in the BlockLife codebase using Clean Architecture with MVP pattern, CQRS, and functional programming principles.

## How to Add a New Feature (Step-by-Step)

### Example: "Frost Block Freezes Neighbors"

1.  **Slice**: Create a new folder `src/Features/Block/Freeze/`. All C# logic for this feature lives here.
2.  **Data**: If needed, create/update a `Resource` script in `src/Data/Definitions/`.
3.  **Command**: In the feature slice, create `FreezeNeighborsCommand.cs` (implementing `ICommand`) and its `FreezeNeighborsHandler.cs`.
4.  **Logic**: Implement the freezing logic inside the handler. Enqueue a `BlocksFrozenEffect`.
5.  **Effect**: Define `BlocksFrozenEffect` in a shared location like `src/Core/Simulation/Effects/`.
6.  **Notification**: The `SimulationManager` publishes a `BlocksFrozenNotification` when it processes the effect.
7.  **View Contract**: Define the **minimal** `IBlockFreezeView.cs` interface in the feature slice. If the view has multiple distinct presentation concerns (e.g., animations, VFX, UI elements), define sub-interfaces (e.g., `IBlockFreezeAnimationView`, `IBlockFreezeVFXView`) and make `IBlockFreezeView` a **composite interface** that exposes these sub-interfaces as properties. These interfaces should describe *capabilities* (e.g., `PlayFreezeAnimation()`), not concrete Godot node types.
8.  **Presenter**: The `BlockFreezePresenter.cs` (in the slice) handles the notification and **coordinates actions by delegating to specialized Godot Controller Nodes through the composite View interface** (following the Humble Presenter Principle and Composite View Pattern). It can create `Tween`s, connect signals, and call methods on other specialized Godot components.
9.  **View**: The `BlockFreezeView.cs` (in the `godot_project` folder) implements the composite `IBlockFreezeView` interface. It **must inherit from its specific Godot Node type (e.g., `Node2D`), implement `IPresenterContainer<BlockFreezePresenter>`, and use `SceneRoot.Instance?.CreatePresenterFor(this)`** to automatically create and manage its Presenter. Its methods will typically be simple `GetNode<T>()` calls or direct forwarding of raw input events to the Presenter, and it will expose its child Controller Nodes via the properties defined in the composite interface.

## From Imperative to Functional Handler Implementation

This example demonstrates the power of using `LanguageExt.Core` to create clean, declarative, and robust command handlers, and how to refactor reusable business validation logic into dedicated services.

**Scenario**: `MoveBlockCommand`, which needs to validate that the target position is valid and the block is not locked before moving it.

### Old Imperative Style (The Anti-Pattern)

```csharp
// This style leads to nested if-statements and poor readability.
public class MoveBlockCommandHandler_Old : IRequestHandler<MoveBlockCommand, Fin<Unit>>
{
    // ... dependencies
    public Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
    {
        if (!_gridService.IsValidPosition(request.ToPosition))
        {
            return Task.FromResult(Fail<Unit>(Error.New("Invalid target position.")));
        }
        var block = _blockRepository.GetById(request.BlockId);
        if (block == null)
        {
            return Task.FromResult(Fail<Unit>(Error.New("Block not found.")));
        }
        if (block.IsLocked)
        {
            return Task.FromResult(Fail<Unit>(Error.New("Block is locked.")));
        }
        
        // ... update state ...
        return Task.FromResult(Succ(unit));
    }
}
```

### Refactoring Business Validation into Reusable Rule Services

First, define a generic interface for validation rules:
```csharp
// src/Core/Rules/IValidationRule.cs
using LanguageExt;

/// <summary>
/// A generic interface for a business rule that performs validation.
/// </summary>
/// <typeparam name="TContext">The type of data needed to evaluate the rule.</typeparam>
public interface IValidationRule<in TContext>
{
    /// <summary>
    /// Checks the rule against the provided context.
    /// Returns Unit on success, or an Error on failure.
    /// </summary>
    Fin<Unit> Check(TContext context);
}
```

Then, implement specific rules as separate services. These should be stateless and injectable.
```csharp
// src/Features/Block/Rules/BlockIsNotLockedRule.cs
using LanguageExt;
using static LanguageExt.Prelude;

/// <summary>
/// Business rule: Checks if a block is locked.
/// </summary>
public class BlockIsNotLockedRule : IValidationRule<Block>
{
    public Fin<Unit> Check(Block block) =>
        !block.IsLocked
            ? unit // from static LanguageExt.Prelude
            // Using a structured error is better than a simple string.
            // It allows for pattern matching on error codes, etc.
            : Fail<Unit>(Error.New((int)ErrorCode.BlockIsLocked, $"Block {block.Id} is locked."));
}

// Register this rule in your DI container (e.g., in GameStrapper):
// services.AddTransient<BlockIsNotLockedRule>();
```

### New Functional Style with LanguageExt (The Preferred Pattern)

```csharp
// This style creates a declarative pipeline that clearly describes business rules.
// Error handling is managed by the Fin monad itself.
public class MoveBlockCommandHandler : IRequestHandler<MoveBlockCommand, Fin<Unit>>
{
    private readonly IGridService _gridService;
    private readonly IBlockRepository _blockRepository;
    private readonly ISimulationManager _simulationManager;
    private readonly BlockIsNotLockedRule _blockIsNotLockedRule; // Injected rule service

    public MoveBlockCommandHandler(
        IGridService gridService,
        IBlockRepository blockRepository,
        ISimulationManager simulationManager,
        BlockIsNotLockedRule blockIsNotLockedRule) // Inject the rule
    {
        _gridService = gridService;
        _blockRepository = blockRepository;
        _simulationManager = simulationManager;
        _blockIsNotLockedRule = blockIsNotLockedRule;
    }
    
    public Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
    {
        // This LINQ query syntax creates a declarative pipeline.
        // Each 'from' clause is a validation step. If any step fails,
        // the pipeline short-circuits and returns the failure.
        var result =
            from _ in ValidatePosition(request.ToPosition)
            from block in GetBlock(request.BlockId)
            // Use the injected rule service here.
            from __ in _blockIsNotLockedRule.Check(block) 
            select UpdateBlockPosition(block, request.ToPosition);

        return Task.FromResult(result);
    }

    private Fin<Unit> ValidatePosition(Vector2 toPosition) =>
        _gridService.IsValidPosition(toPosition)
            ? Succ(unit) // from static LanguageExt.Prelude
            // Using a structured error is better than a simple string.
            // It allows for pattern matching on error codes, etc.
            : Fail<Unit>(Error.New((int)ErrorCode.InvalidPosition, "Invalid target position."));

    private Fin<Block> GetBlock(Guid blockId) =>
        _blockRepository.FindById(blockId) // Assumes FindById returns Option<Block>
            .ToFin(Error.New((int)ErrorCode.BlockNotFound, $"Block with ID {blockId} not found."));
            
    private Unit UpdateBlockPosition(Block block, Vector2 newPosition)
    {
        // The actual mutation happens here, only if all previous steps succeeded.
        block.Position = newPosition;
        _blockRepository.Save(block);
        _simulationManager.Enqueue(new BlockMovedEffect(block.Id, newPosition));
        return unit;
    }
}

/*
* Architect's Note:
* Please be aware that while IRequestHandler's interface returns a Task, our architecture dictates that
* the core logic of all Command Handlers MUST be synchronous. Task.FromResult is used here to wrap
* a synchronous computation's result (which is now a `Fin<T>`) into an already completed Task, satisfying the asynchronous interface.
* It is strictly forbidden to directly 'await' any I/O-intensive or long-running asynchronous operations
* inside a CommandHandler. Such operations must follow the pattern defined in the Architecture Guide, where they are
* initiated by the Presenter (or a dedicated service) and then dispatch a new Command upon completion.
*
*   **CRITICAL PERFORMANCE NOTE**: The functional style using `Fin<T>` and LINQ expressions is exceptionally powerful for expressing complex business logic and validation pipelines. However, it comes at the cost of small heap allocations for each monadic object and closure created. This style is **highly recommended for event-driven, transactional command handling** (e.g., responding to a button click). It is **strictly forbidden for high-frequency logic inside a performance-critical loop like `_Process` or `_PhysicsProcess`**, as it will lead to significant GC pressure and frame rate stutter. Always choose the right tool for the job.
*/
```

## Advanced Pattern: The Rule Engine

For commands that require numerous, complex validations, injecting many individual `IValidationRule` services can bloat the handler. In such cases, you can introduce a `RuleEngine` to encapsulate the validation pipeline.

### 1. Define the Engine Interface

```csharp
// src/Core/Rules/IRuleEngine.cs
using LanguageExt;
using System.Collections.Generic;

public interface IRuleEngine<in TContext>
{
    /// <summary>
    /// Executes a series of validation rules against a context.
    /// </summary>
    /// <param name="context">The data to validate.</param>
    /// <param name="rules">The collection of rules to apply.</param>
    /// <returns>A Fin that is Succ on success, or the first Fail encountered.</returns>
    Fin<Unit> Validate(TContext context, IEnumerable<IValidationRule<TContext>> rules);
}
```

### 2. Implement the Engine

```csharp
// src/Core/Rules/RuleEngine.cs
using LanguageExt;
using static LanguageExt.Prelude;
using System.Collections.Generic;
using System.Linq;

public class RuleEngine<TContext> : IRuleEngine<TContext>
{
    public Fin<Unit> Validate(TContext context, IEnumerable<IValidationRule<TContext>> rules) =>
        // Use Aggregate to chain the checks. The pipeline stops on the first Fail.
        rules.Aggregate(
            Succ(unit), 
            (currentResult, rule) => currentResult.Bind(_ => rule.Check(context))
        );
}
```

### 3. Refactor the Handler to Use the Engine

```csharp
// The handler is now much cleaner and doesn't need to know about every single rule.
public class MoveBlockCommandHandler_WithEngine : IRequestHandler<MoveBlockCommand, Fin<Unit>>
{
    private readonly IRuleEngine<Block> _blockRuleEngine;
    private readonly IValidationRule<Vector2> _positionRule; // Can still inject simple rules
    private readonly IBlockRepository _blockRepository;
    // ... other dependencies

    private readonly List<IValidationRule<Block>> _blockValidationRules;

    public MoveBlockCommandHandler_WithEngine(
        IRuleEngine<Block> blockRuleEngine,
        BlockIsNotLockedRule blockIsNotLockedRule, // Inject concrete rules
        BlockHasEnoughEnergyRule blockHasEnoughEnergyRule, // Another rule
        /* ... */)
    {
        _blockRuleEngine = blockRuleEngine;
        // Collect all relevant rules into a list. This can be configured via DI.
        _blockValidationRules = new List<IValidationRule<Block>>
        {
            blockIsNotLockedRule,
            blockHasEnoughEnergyRule
        };
        // ...
    }

    public Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
    {
        var result =
            from _ in _positionRule.Check(request.ToPosition)
            from block in GetBlock(request.BlockId)
            // A single, clean call to the rule engine.
            from __ in _blockRuleEngine.Validate(block, _blockValidationRules)
            select UpdateBlockPosition(block, request.ToPosition);

        return Task.FromResult(result);
    }
    // ... helper methods remain the same
}
```

This pattern is not mandatory for simple cases but is the **recommended approach** for features with complex validation logic. It keeps handlers clean and promotes a highly cohesive, composable, and testable rule system.

## Practical Tools and Strategies

This architecture, while robust, introduces boilerplate and a steeper learning curve. To mitigate this "friction cost" and ensure long-term adherence, we must invest in tooling:

*   **IDE/CLI Templates (MANDATORY)**: Provide the team with simple scripts or IDE templates (e.g., Rider's File Templates, `dotnet new` templates). A developer **MUST** be able to execute a single command, such as `create-feature Block.Freeze`, and automatically generate all necessary boilerplate files in the correct `src/` and `godot_project/` locations (e.g., `Command.cs`, `Handler.cs`, `IView.cs`, `Presenter.cs`, `View.cs`, `Tests.cs`). This is not a suggestion but a **critical investment** to reduce the "tooling tax" and make the "correct way" the "easiest way."
*   **Rationale**: By making the "correct way" the "easiest way," we significantly reduce the temptation for developers to bypass architectural rules for convenience. This solidifies best practices into the development workflow.
*   **Onboarding & Training**: Supplement this guide with hands-on workshops and pair programming sessions for new team members. Focus on the "why" behind each rule, not just the "how."

## Example: The Humble Presenter Principle in Action ("Meteor Strike")

This example demonstrates the application of the **Humble Presenter Principle**, now enforced by the **Composite View Pattern**, to a complex feature.

**The Goal**: Implement a "Meteor Strike" skill involving casting animations, UI, sound, VFX, and finally, dealing damage.

### Scene Tree Structure

The `PlayerView` node contains specialized Controller Nodes as children. The Presenter itself is a C# object, not a node.

```
- PlayerView (Node2D, PlayerView.cs)
  - AnimationController (Node, AnimationController.cs)
    - AnimationPlayer
  - VFXController (Node, VFXController.cs)
  - SFXController (Node, SFXController.cs)
    - AudioStreamPlayer
  - UIController (Node, UIController.cs)
    - CastBar (ProgressBar)
  - PlayerCharacter (CharacterBody2D)
```

### View Sub-Interfaces (The Capabilities)

These interfaces describe the specific presentation capabilities provided by each Controller Node. They reside in `src/Features/Player/Skills/`.

```csharp
// src/Features/Player/Skills/IAnimationView.cs
using System.Threading.Tasks;

public interface IAnimationView
{
    Task PlayAnimationAsync(string animationName);
}

// src/Features/Player/Skills/IVFXView.cs
using Godot; // Allowed for interfaces that expose Godot-specific types like Vector2
public interface IVFXView
{
    void ShowTargetingReticule(Vector2 position);
    void HideTargetingReticule();
    void SpawnMeteorVFX(Vector2 position);
}

// src/Features/Player/Skills/ISFXView.cs
public interface ISFXView
{
    void PlayCastingSound();
    void PlayImpactSound();
}

// src/Features/Player/Skills/IUIView.cs
using System.Threading.Tasks;

public interface IUIView
{
    Task ShowCastBar(float duration);
}
```

### Composite View Interface (The Contract for the Presenter)

This is the **ONLY** interface the Presenter will depend on. It combines all sub-capabilities. It resides in `src/Features/Player/Skills/`.

```csharp
// src/Features/Player/Skills/IPlayerSkillView.cs

// This is the ONLY interface the Presenter will know about.
// It exposes capabilities as properties, not concrete types or Get() methods.
public interface IPlayerSkillView
{
    IAnimationView Animation { get; }
    IVFXView VFX { get; }
    ISFXView SFX { get; }
    IUIView UI { get; }
}
```

### Controller Node Implementations

These are Godot scripts that implement the specific sub-interfaces. They reside in `godot_project/features/player/skills/`.

```csharp
// godot_project/features/player/skills/AnimationController.cs
using Godot;
using System.Threading.Tasks;

// This node's script now implements the specific sub-interface.
public partial class AnimationController : Node, IAnimationView
{
    [Export]
    private AnimationPlayer _animationPlayer;

    public async Task PlayAnimationAsync(string animationName)
    {
        _animationPlayer.Play(animationName);
        await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }
}

// godot_project/features/player/skills/VFXController.cs
using Godot;
public partial class VFXController : Node, IVFXView
{
    [Export] private Node2D _targetingReticule;
    [Export] private PackedScene _meteorVFXScene;

    public void ShowTargetingReticule(Vector2 position)
    {
        _targetingReticule.GlobalPosition = position;
        _targetingReticule.Visible = true;
    }
    public void HideTargetingReticule() => _targetingReticule.Visible = false;
    public void SpawnMeteorVFX(Vector2 position)
    {
        var vfxInstance = _meteorVFXScene.Instantiate<Node2D>();
        GetTree().CurrentScene.AddChild(vfxInstance);
        vfxInstance.GlobalPosition = position;
        // Add logic to queue_free after animation/duration
    }
}

// godot_project/features/player/skills/SFXController.cs
using Godot;
public partial class SFXController : Node, ISFXView
{
    [Export] private AudioStreamPlayer _castingSoundPlayer;
    [Export] private AudioStreamPlayer _impactSoundPlayer;

    public void PlayCastingSound() => _castingSoundPlayer.Play();
    public void PlayImpactSound() => _impactSoundPlayer.Play();
}

// godot_project/features/player/skills/UIController.cs
using Godot;
using System.Threading.Tasks;
public partial class UIController : Node, IUIView
{
    [Export] private ProgressBar _castBar;

    public async Task ShowCastBar(float duration)
    {
        _castBar.Visible = true;
        _castBar.Value = 0;
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_castBar, "value", 100, duration);
        await ToSignal(tween, Tween.SignalName.Finished);
        _castBar.Visible = false;
    }
}
```

### Main View (The Composition Root)

The `PlayerSkillView` is responsible for composing its child Controller Nodes and exposing them via the composite interface. It resides in `godot_project/features/player/skills/`.

```csharp
// godot_project/features/player/skills/PlayerSkillView.cs
using Godot;
using System; // For IDisposable

// Implements the composite interface and the required container interface.
public partial class PlayerSkillView : Node2D, IPlayerSkillView, IPresenterContainer<PlayerSkillPresenter>
{
    // These are wired up in the Godot Editor via the [Export] attribute.
    // This View's job is to compose its children.
    [Export]
    private AnimationController _animationController;
    [Export]
    private VFXController _vfxController;
    [Export]
    private SFXController _sfxController;
    [Export]
    private UIController _uiController;
    
    // --- IPlayerSkillView Implementation ---
    // The implementation is trivial: just return the exported nodes.
    // This cleanly separates the Presenter from the scene tree structure.
    public IAnimationView Animation => _animationController;
    public IVFXView VFX => _vfxController;
    public ISFXView SFX => _sfxController;
    public IUIView UI => _uiController;

    // --- Presenter Lifecycle Management ---
    public PlayerSkillPresenter Presenter { get; set; }
    private IDisposable _lifecycleManager;

    public override void _Ready()
    {
        _lifecycleManager = SceneRoot.Instance?.CreatePresenterFor(this);
        if (_lifecycleManager == null) return;
    }

    public override void _ExitTree()
    {
        _lifecycleManager?.Dispose();
    }
}
```

### Humble Presenter (The Implementation)

The `PlayerSkillPresenter` now interacts with the View through the clean, composite interface. It resides in `src/Features/Player/Skills/`.

```csharp
// src/Features/Player/Skills/PlayerSkillPresenter.cs
using System.Threading.Tasks;
using MediatR;
using Godot; // Allowed for Presenters (e.g., for Vector2)

// The Presenter is now truly humble. It doesn't know about "Controllers" or their Get() methods.
// It only knows about capabilities exposed by the IPlayerSkillView interface.
public class PlayerSkillPresenter : PresenterBase<IPlayerSkillView>
{
    private readonly IMediator _mediator;

    public PlayerSkillPresenter(IMediator mediator, IPlayerSkillView view) : base(view)
    {
        _mediator = mediator;
    }

    // No need for Initialize() to grab controllers anymore.
    // The View contract guarantees they are always available via properties.

    public async Task ActivateMeteorStrike(Vector2 targetPosition)
    {
        // Coordination Principle - The code reads like a high-level script.
        // Delegation Principle - Every step is delegated to a specialized controller
        //           via the composite View interface.
        View.SFX.PlayCastingSound();
        View.VFX.ShowTargetingReticule(targetPosition);
        
        // Using `async/await` for presentation flow control is allowed.
        await View.Animation.PlayAnimationAsync("Cast_Meteor");
        await View.UI.ShowCastBar(1.5f);

        View.VFX.HideTargetingReticule();
        View.VFX.SpawnMeteorVFX(targetPosition);
        View.SFX.PlayImpactSound();

        // State-Impotence Principle - To change authoritative state (damage), it sends a command.
        var command = new DealAreaDamageCommand(targetPosition, 50);
        await _mediator.Send(command);
    }
}
```

This implementation is clean, maintainable, and strictly adheres to our architecture. The Presenter is "humble" because it knows its job is simply to tell others what to do, without knowing *how* those others are structured internally. The complexity is neatly encapsulated in specialized, testable (via integration tests), and reusable Controller Nodes, exposed through a clean composite interface. This is the standard all our Presenters must meet.

---

## Related Documents

- [Architecture Guide](../1_Architecture/Architecture_Guide.md) - Core principles and rules
- [Standard Patterns](../1_Architecture/Standard_Patterns.md) - Validated architectural patterns
- [Comprehensive Development Workflow](Comprehensive_Development_Workflow.md) - Complete TDD+VSA process
- [Quick Reference Development Checklist](Quick_Reference_Development_Checklist.md) - Daily workflow steps
- [Move Block Feature](../../src/Features/Block/Move/) - Gold standard reference implementation