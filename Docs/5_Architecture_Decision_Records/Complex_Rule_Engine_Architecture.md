# Complex Rule Engine Architecture for BlockLife

## Executive Summary

After reassessing the complex rule requirements for BlockLife, I acknowledge that the initial lightweight recommendation was insufficient. The requirements for shape-based matching (T-5, T-4 patterns), adjacency rules, and chain reactions demand a more sophisticated architecture. However, **NRule is still not the optimal choice** for this project due to architectural constraints and specific game requirements.

This document presents a **Custom Pattern-Matching Rule Engine** that maintains Clean Architecture principles while providing the flexibility and performance needed for complex rule evaluation.

## 1. Problem Analysis

### 1.1 Complexity Assessment

The updated requirements reveal several challenging aspects:

**Pattern Matching Complexity:**
- Variable-size shape patterns (T-5, T-4, custom orientations)
- Multiple pattern definitions per block type
- Potentially recursive pattern evaluation

**Spatial Relationship Complexity:**
- Non-trivial adjacency definitions beyond immediate neighbors
- Multi-hop spatial relationships
- Context-sensitive adjacency rules

**Chain Reaction Complexity:**
- New blocks triggering multiple cascade patterns
- Different trigger conditions per block type
- Potential for infinite loops requiring cycle detection

### 1.2 Why Not NRule?

While NRule's Rete algorithm excels at complex rule evaluation, it introduces significant architectural friction:

1. **Clean Architecture Violation**: NRule would require business rules to be expressed in a DSL or attribute-based format, breaking the pure C# principle
2. **Testing Complexity**: Rule definitions become harder to unit test in isolation
3. **Debugging Difficulty**: The Rete network obscures execution flow, making debugging complex
4. **Overkill for Game Logic**: NRule is designed for enterprise business rules, not real-time game pattern matching
5. **Functional Programming Clash**: NRule's imperative style conflicts with the LanguageExt functional approach

## 2. Proposed Architecture: Custom Pattern-Matching Rule Engine

### 2.1 Core Design Principles

1. **Maintain Clean Architecture**: All rule logic remains in pure C# within the core project
2. **Composable Pattern System**: Individual patterns can be combined into complex rule sets
3. **Functional Evaluation Pipeline**: Uses LanguageExt for robust error handling and composition
4. **Performance-Optimized**: Spatial indexing and lazy evaluation for real-time performance
5. **Testable and Debuggable**: Each component can be tested in isolation

### 2.2 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Rule Engine Core                         │
├─────────────────────────────────────────────────────────────┤
│  IRuleEngine<TContext>                                      │
│  │                                                         │
│  ├── IPatternMatcher<TPattern, TContext>                   │
│  │   ├── ShapePatternMatcher                              │
│  │   ├── AdjacencyPatternMatcher                          │
│  │   └── ChainReactionMatcher                             │
│  │                                                         │
│  ├── ISpatialIndex<TEntity>                               │
│  │   └── GridSpatialIndex                                 │
│  │                                                         │
│  └── IRuleEvaluationStrategy                              │
│      ├── EagerEvaluationStrategy                          │
│      └── LazyEvaluationStrategy                           │
└─────────────────────────────────────────────────────────────┘
```

## 3. Implementation Details

### 3.1 Core Interfaces

```csharp
// src/Core/Rules/IRuleEngine.cs
using LanguageExt;
using System.Collections.Generic;

/// <summary>
/// Core rule engine interface for evaluating complex game rules
/// against a given context.
/// </summary>
/// <typeparam name="TContext">The context type (e.g., GridState)</typeparam>
public interface IRuleEngine<in TContext>
{
    /// <summary>
    /// Evaluates all registered rules against the context and returns
    /// a collection of successful matches with their resulting effects.
    /// </summary>
    Fin<IReadOnlyCollection<IRuleMatchResult>> EvaluateRules(TContext context);
    
    /// <summary>
    /// Registers a new rule matcher with the engine.
    /// </summary>
    IRuleEngine<TContext> RegisterMatcher<TMatcher>(TMatcher matcher) 
        where TMatcher : IPatternMatcher<TContext>;
}

// src/Core/Rules/IPatternMatcher.cs
/// <summary>
/// Interface for pattern matchers that can identify specific patterns
/// in a given context and produce rule match results.
/// </summary>
public interface IPatternMatcher<in TContext>
{
    /// <summary>
    /// Attempts to find all instances of this pattern in the context.
    /// </summary>
    Fin<IReadOnlyCollection<IRuleMatchResult>> FindMatches(TContext context);
    
    /// <summary>
    /// The priority of this matcher (higher values evaluated first).
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Unique identifier for this matcher type.
    /// </summary>
    string MatcherId { get; }
}

// src/Core/Rules/IRuleMatchResult.cs
/// <summary>
/// Represents a successful pattern match and the effects it should produce.
/// </summary>
public interface IRuleMatchResult
{
    /// <summary>
    /// The matched entities (blocks) involved in this pattern.
    /// </summary>
    IReadOnlyCollection<Guid> MatchedBlockIds { get; }
    
    /// <summary>
    /// The effects that should be applied as a result of this match.
    /// </summary>
    IReadOnlyCollection<IEffect> Effects { get; }
    
    /// <summary>
    /// The pattern type that produced this match.
    /// </summary>
    string PatternType { get; }
    
    /// <summary>
    /// Optional metadata about the match (e.g., pattern orientation, score).
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}
```

### 3.2 Shape Pattern Matching System

```csharp
// src/Features/Block/Rules/Patterns/ShapePattern.cs
using System.Numerics;
using LanguageExt;

/// <summary>
/// Represents a shape pattern that can be matched against the grid.
/// Immutable value type for optimal performance.
/// </summary>
public readonly record struct ShapePattern(
    string Name,
    IReadOnlyCollection<Vector2> RelativePositions,
    IReadOnlyCollection<string> AllowedBlockTypes,
    int Priority = 0)
{
    /// <summary>
    /// Creates all possible rotations of this pattern.
    /// </summary>
    public IEnumerable<ShapePattern> GetRotations()
    {
        var rotations = new List<ShapePattern> { this };
        
        for (int rotation = 1; rotation < 4; rotation++)
        {
            var rotatedPositions = RelativePositions
                .Select(pos => RotatePoint(pos, rotation * 90))
                .ToHashSet();
                
            rotations.Add(this with 
            { 
                Name = $"{Name}_Rot{rotation * 90}",
                RelativePositions = rotatedPositions 
            });
        }
        
        return rotations;
    }
    
    private static Vector2 RotatePoint(Vector2 point, int degrees)
    {
        var radians = degrees * Math.PI / 180.0;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);
        
        return new Vector2(
            point.X * cos - point.Y * sin,
            point.X * sin + point.Y * cos
        );
    }
}

// src/Features/Block/Rules/Patterns/ShapePatternMatcher.cs
/// <summary>
/// Matcher for shape-based patterns (T-5, T-4, etc.).
/// Optimized for real-time grid evaluation.
/// </summary>
public class ShapePatternMatcher : IPatternMatcher<GridStateDto>
{
    private readonly IReadOnlyCollection<ShapePattern> _patterns;
    private readonly ISpatialIndex<BlockDto> _spatialIndex;
    
    public string MatcherId => "ShapePattern";
    public int Priority => 100;
    
    public ShapePatternMatcher(
        IReadOnlyCollection<ShapePattern> patterns,
        ISpatialIndex<BlockDto> spatialIndex)
    {
        _patterns = patterns;
        _spatialIndex = spatialIndex;
    }
    
    public Fin<IReadOnlyCollection<IRuleMatchResult>> FindMatches(GridStateDto context)
    {
        try
        {
            var matches = new List<IRuleMatchResult>();
            
            // Get all possible anchor points from spatial index
            var anchorPositions = _spatialIndex.GetAllPositions();
            
            foreach (var anchorPos in anchorPositions)
            {
                foreach (var pattern in _patterns)
                {
                    foreach (var rotatedPattern in pattern.GetRotations())
                    {
                        var matchResult = TryMatchPattern(rotatedPattern, anchorPos, context);
                        if (matchResult.IsSome)
                        {
                            matches.Add(matchResult.Value);
                        }
                    }
                }
            }
            
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(matches);
        }
        catch (Exception ex)
        {
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Fail(
                Error.New(1001, $"Shape pattern matching failed: {ex.Message}", ex));
        }
    }
    
    private Option<IRuleMatchResult> TryMatchPattern(
        ShapePattern pattern, 
        Vector2 anchorPosition, 
        GridStateDto context)
    {
        var matchedBlocks = new List<Guid>();
        
        foreach (var relativePos in pattern.RelativePositions)
        {
            var worldPos = anchorPosition + relativePos;
            var blockAtPosition = _spatialIndex.GetBlockAt(worldPos);
            
            if (blockAtPosition.IsNone) 
                return Option<IRuleMatchResult>.None;
                
            var block = blockAtPosition.Value;
            if (!pattern.AllowedBlockTypes.Contains(block.BlockType))
                return Option<IRuleMatchResult>.None;
                
            matchedBlocks.Add(block.Id);
        }
        
        // Create effects based on pattern type
        var effects = CreateEffectsForPattern(pattern, matchedBlocks, anchorPosition);
        
        return new ShapeMatchResult(
            matchedBlocks,
            effects,
            pattern.Name,
            new Dictionary<string, object>
            {
                ["AnchorPosition"] = anchorPosition,
                ["PatternSize"] = pattern.RelativePositions.Count
            }
        );
    }
    
    private IReadOnlyCollection<IEffect> CreateEffectsForPattern(
        ShapePattern pattern,
        IReadOnlyCollection<Guid> matchedBlocks,
        Vector2 anchorPosition)
    {
        return pattern.Name switch
        {
            "T5_Pattern" => new IEffect[] 
            { 
                new BlockPatternMatchedEffect(matchedBlocks, "T5_Clear", 500),
                new SpawnNewBlockEffect(anchorPosition, "CareerOpportunity")
            },
            "T4_Pattern" => new IEffect[]
            {
                new BlockPatternMatchedEffect(matchedBlocks, "T4_Clear", 200)
            },
            _ => Array.Empty<IEffect>()
        };
    }
}
```

### 3.3 Adjacency Rule System

```csharp
// src/Features/Block/Rules/Adjacency/AdjacencyRule.cs
/// <summary>
/// Defines complex adjacency relationships beyond simple grid neighbors.
/// </summary>
public readonly record struct AdjacencyRule(
    string Name,
    IReadOnlyCollection<string> TriggerBlockTypes,
    IReadOnlyCollection<string> TargetBlockTypes,
    IAdjacencyCalculator AdjacencyCalculator,
    IReadOnlyCollection<IEffect> Effects)
{
    /// <summary>
    /// Checks if two blocks satisfy this adjacency rule.
    /// </summary>
    public bool IsAdjacent(BlockDto triggerBlock, BlockDto targetBlock, GridStateDto context)
    {
        if (!TriggerBlockTypes.Contains(triggerBlock.BlockType)) return false;
        if (!TargetBlockTypes.Contains(targetBlock.BlockType)) return false;
        
        return AdjacencyCalculator.AreAdjacent(triggerBlock, targetBlock, context);
    }
}

// src/Features/Block/Rules/Adjacency/IAdjacencyCalculator.cs
/// <summary>
/// Strategy interface for different adjacency calculation methods.
/// </summary>
public interface IAdjacencyCalculator
{
    /// <summary>
    /// Determines if two blocks are considered adjacent according to this calculator's rules.
    /// </summary>
    bool AreAdjacent(BlockDto block1, BlockDto block2, GridStateDto context);
    
    /// <summary>
    /// Gets all blocks adjacent to the given block according to this calculator.
    /// </summary>
    IEnumerable<BlockDto> GetAdjacentBlocks(BlockDto block, GridStateDto context);
}

// src/Features/Block/Rules/Adjacency/RadiusAdjacencyCalculator.cs
/// <summary>
/// Adjacency calculator based on distance radius.
/// </summary>
public class RadiusAdjacencyCalculator : IAdjacencyCalculator
{
    private readonly float _maxDistance;
    private readonly bool _requiresLineOfSight;
    
    public RadiusAdjacencyCalculator(float maxDistance, bool requiresLineOfSight = false)
    {
        _maxDistance = maxDistance;
        _requiresLineOfSight = requiresLineOfSight;
    }
    
    public bool AreAdjacent(BlockDto block1, BlockDto block2, GridStateDto context)
    {
        var distance = Vector2.Distance(block1.Position, block2.Position);
        if (distance > _maxDistance) return false;
        
        if (_requiresLineOfSight)
        {
            return HasLineOfSight(block1.Position, block2.Position, context);
        }
        
        return true;
    }
    
    public IEnumerable<BlockDto> GetAdjacentBlocks(BlockDto block, GridStateDto context)
    {
        return context.Blocks.Where(other => 
            other.Id != block.Id && AreAdjacent(block, other, context));
    }
    
    private bool HasLineOfSight(Vector2 from, Vector2 to, GridStateDto context)
    {
        // Implementation of line-of-sight calculation
        // Could use Bresenham's line algorithm or ray-casting
        // For now, simplified implementation
        return true; // Placeholder
    }
}

// src/Features/Block/Rules/Adjacency/AdjacencyPatternMatcher.cs
/// <summary>
/// Matcher for adjacency-based rules (Work + Study blocks triggering Career Opportunity).
/// </summary>
public class AdjacencyPatternMatcher : IPatternMatcher<GridStateDto>
{
    private readonly IReadOnlyCollection<AdjacencyRule> _adjacencyRules;
    private readonly ISpatialIndex<BlockDto> _spatialIndex;
    
    public string MatcherId => "AdjacencyPattern";
    public int Priority => 90;
    
    public AdjacencyPatternMatcher(
        IReadOnlyCollection<AdjacencyRule> adjacencyRules,
        ISpatialIndex<BlockDto> spatialIndex)
    {
        _adjacencyRules = adjacencyRules;
        _spatialIndex = spatialIndex;
    }
    
    public Fin<IReadOnlyCollection<IRuleMatchResult>> FindMatches(GridStateDto context)
    {
        try
        {
            var matches = new List<IRuleMatchResult>();
            
            foreach (var rule in _adjacencyRules)
            {
                var ruleMatches = FindMatchesForRule(rule, context);
                matches.AddRange(ruleMatches);
            }
            
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(matches);
        }
        catch (Exception ex)
        {
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Fail(
                Error.New(1002, $"Adjacency pattern matching failed: {ex.Message}", ex));
        }
    }
    
    private IEnumerable<IRuleMatchResult> FindMatchesForRule(
        AdjacencyRule rule, 
        GridStateDto context)
    {
        var triggerBlocks = context.Blocks
            .Where(b => rule.TriggerBlockTypes.Contains(b.BlockType));
            
        foreach (var triggerBlock in triggerBlocks)
        {
            var adjacentBlocks = rule.AdjacencyCalculator
                .GetAdjacentBlocks(triggerBlock, context)
                .Where(b => rule.TargetBlockTypes.Contains(b.BlockType));
                
            foreach (var targetBlock in adjacentBlocks)
            {
                yield return new AdjacencyMatchResult(
                    new[] { triggerBlock.Id, targetBlock.Id },
                    rule.Effects,
                    rule.Name,
                    new Dictionary<string, object>
                    {
                        ["TriggerBlock"] = triggerBlock.Id,
                        ["TargetBlock"] = targetBlock.Id,
                        ["Distance"] = Vector2.Distance(triggerBlock.Position, targetBlock.Position)
                    }
                );
            }
        }
    }
}
```

### 3.4 Chain Reaction System

```csharp
// src/Features/Block/Rules/ChainReaction/ChainReactionMatcher.cs
/// <summary>
/// Matcher for handling cascading chain reactions.
/// Includes cycle detection to prevent infinite loops.
/// </summary>
public class ChainReactionMatcher : IPatternMatcher<GridStateDto>
{
    private readonly IReadOnlyCollection<IPatternMatcher<GridStateDto>> _baseMatchers;
    private readonly int _maxChainDepth;
    
    public string MatcherId => "ChainReaction";
    public int Priority => 80;
    
    public ChainReactionMatcher(
        IReadOnlyCollection<IPatternMatcher<GridStateDto>> baseMatchers,
        int maxChainDepth = 10)
    {
        _baseMatchers = baseMatchers;
        _maxChainDepth = maxChainDepth;
    }
    
    public Fin<IReadOnlyCollection<IRuleMatchResult>> FindMatches(GridStateDto context)
    {
        try
        {
            var allMatches = new List<IRuleMatchResult>();
            var currentContext = context;
            var processedStates = new HashSet<string>();
            var chainDepth = 0;
            
            while (chainDepth < _maxChainDepth)
            {
                // Create a state signature for cycle detection
                var stateSignature = CreateStateSignature(currentContext);
                if (processedStates.Contains(stateSignature))
                {
                    // Cycle detected, break the chain
                    break;
                }
                processedStates.Add(stateSignature);
                
                // Find all matches in current state
                var currentMatches = new List<IRuleMatchResult>();
                foreach (var matcher in _baseMatchers)
                {
                    var matchResult = matcher.FindMatches(currentContext);
                    if (matchResult.IsSucc)
                    {
                        currentMatches.AddRange(matchResult.Value);
                    }
                }
                
                if (!currentMatches.Any())
                {
                    // No more matches found, chain ends naturally
                    break;
                }
                
                allMatches.AddRange(currentMatches);
                
                // Simulate the effects to create new context for next iteration
                currentContext = ApplyEffectsToContext(currentContext, currentMatches);
                chainDepth++;
            }
            
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(allMatches);
        }
        catch (Exception ex)
        {
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Fail(
                Error.New(1003, $"Chain reaction evaluation failed: {ex.Message}", ex));
        }
    }
    
    private string CreateStateSignature(GridStateDto context)
    {
        // Create a deterministic signature of the current grid state
        // for cycle detection
        var blockSignatures = context.Blocks
            .OrderBy(b => b.Id)
            .Select(b => $"{b.Id}:{b.Position.X},{b.Position.Y}:{b.BlockType}")
            .ToArray();
            
        return string.Join("|", blockSignatures);
    }
    
    private GridStateDto ApplyEffectsToContext(
        GridStateDto context, 
        IReadOnlyCollection<IRuleMatchResult> matches)
    {
        // This is a simulation method - applies effects to create new state
        // for chain reaction evaluation without affecting the actual game state
        var newBlocks = context.Blocks.ToList();
        
        foreach (var match in matches)
        {
            foreach (var effect in match.Effects)
            {
                switch (effect)
                {
                    case BlockPatternMatchedEffect clearEffect:
                        // Remove matched blocks
                        newBlocks.RemoveAll(b => clearEffect.BlockIds.Contains(b.Id));
                        break;
                        
                    case SpawnNewBlockEffect spawnEffect:
                        // Add new block
                        newBlocks.Add(new BlockDto(
                            Guid.NewGuid(),
                            spawnEffect.Position,
                            spawnEffect.BlockType,
                            false // Not locked
                        ));
                        break;
                }
            }
        }
        
        return context with { Blocks = newBlocks };
    }
}
```

### 3.5 Spatial Indexing for Performance

```csharp
// src/Core/Rules/Spatial/ISpatialIndex.cs
/// <summary>
/// Spatial indexing interface for efficient spatial queries.
/// </summary>
public interface ISpatialIndex<TEntity>
{
    /// <summary>
    /// Gets the entity at the specified position, if any.
    /// </summary>
    Option<TEntity> GetEntityAt(Vector2 position);
    
    /// <summary>
    /// Gets all entities within the specified radius of the position.
    /// </summary>
    IEnumerable<TEntity> GetEntitiesInRadius(Vector2 position, float radius);
    
    /// <summary>
    /// Gets all positions that contain entities.
    /// </summary>
    IEnumerable<Vector2> GetAllPositions();
    
    /// <summary>
    /// Updates the index with new entity data.
    /// </summary>
    void UpdateIndex(IReadOnlyCollection<TEntity> entities);
}

// src/Features/Block/Rules/Spatial/GridSpatialIndex.cs
/// <summary>
/// Grid-based spatial index optimized for block-based games.
/// Uses a dictionary for O(1) position lookups.
/// </summary>
public class GridSpatialIndex : ISpatialIndex<BlockDto>
{
    private readonly Dictionary<Vector2, BlockDto> _positionToBlock = new();
    private readonly float _tolerance = 0.001f;
    
    public Option<BlockDto> GetEntityAt(Vector2 position)
    {
        // Snap to grid for consistent lookups
        var gridPosition = SnapToGrid(position);
        return _positionToBlock.TryGetValue(gridPosition, out var block) 
            ? Option<BlockDto>.Some(block)
            : Option<BlockDto>.None;
    }
    
    public IEnumerable<BlockDto> GetEntitiesInRadius(Vector2 position, float radius)
    {
        var radiusSquared = radius * radius;
        return _positionToBlock.Values
            .Where(block => Vector2.DistanceSquared(block.Position, position) <= radiusSquared);
    }
    
    public IEnumerable<Vector2> GetAllPositions() => _positionToBlock.Keys;
    
    public void UpdateIndex(IReadOnlyCollection<BlockDto> entities)
    {
        _positionToBlock.Clear();
        foreach (var entity in entities)
        {
            var gridPosition = SnapToGrid(entity.Position);
            _positionToBlock[gridPosition] = entity;
        }
    }
    
    // Alias for block-specific usage
    public Option<BlockDto> GetBlockAt(Vector2 position) => GetEntityAt(position);
    
    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            MathF.Round(position.X / _tolerance) * _tolerance,
            MathF.Round(position.Y / _tolerance) * _tolerance
        );
    }
}
```

### 3.6 Rule Engine Implementation

```csharp
// src/Core/Rules/RuleEngine.cs
/// <summary>
/// Main rule engine implementation that coordinates all pattern matchers
/// and provides a unified evaluation interface.
/// </summary>
public class RuleEngine : IRuleEngine<GridStateDto>
{
    private readonly List<IPatternMatcher<GridStateDto>> _matchers = new();
    private readonly ISpatialIndex<BlockDto> _spatialIndex;
    private readonly ILogger _logger;
    
    public RuleEngine(ISpatialIndex<BlockDto> spatialIndex, ILogger logger)
    {
        _spatialIndex = spatialIndex;
        _logger = logger;
    }
    
    public IRuleEngine<GridStateDto> RegisterMatcher<TMatcher>(TMatcher matcher) 
        where TMatcher : IPatternMatcher<GridStateDto>
    {
        _matchers.Add(matcher);
        // Sort by priority (highest first)
        _matchers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return this;
    }
    
    public Fin<IReadOnlyCollection<IRuleMatchResult>> EvaluateRules(GridStateDto context)
    {
        try
        {
            // Update spatial index for current context
            _spatialIndex.UpdateIndex(context.Blocks);
            
            var allMatches = new List<IRuleMatchResult>();
            var usedBlocks = new HashSet<Guid>();
            
            // Evaluate matchers in priority order
            foreach (var matcher in _matchers)
            {
                _logger.Debug("Evaluating matcher: {MatcherId}", matcher.MatcherId);
                
                var matchResult = matcher.FindMatches(context);
                if (matchResult.IsFail)
                {
                    _logger.Warning("Matcher {MatcherId} failed: {Error}", 
                        matcher.MatcherId, matchResult.Error);
                    continue;
                }
                
                // Filter out matches that conflict with higher-priority matches
                var validMatches = matchResult.Value
                    .Where(match => !match.MatchedBlockIds.Any(id => usedBlocks.Contains(id)))
                    .ToList();
                
                foreach (var match in validMatches)
                {
                    allMatches.Add(match);
                    foreach (var blockId in match.MatchedBlockIds)
                    {
                        usedBlocks.Add(blockId);
                    }
                }
                
                _logger.Debug("Matcher {MatcherId} found {MatchCount} valid matches", 
                    matcher.MatcherId, validMatches.Count);
            }
            
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(allMatches);
        }
        catch (Exception ex)
        {
            return Fin<IReadOnlyCollection<IRuleMatchResult>>.Fail(
                Error.New(1000, $"Rule engine evaluation failed: {ex.Message}", ex));
        }
    }
}
```

## 4. Integration with CQRS Architecture

### 4.1 Command Handler Integration

```csharp
// src/Features/Block/Rules/EvaluateRulesCommand.cs
/// <summary>
/// Command to evaluate rules against the current grid state.
/// Triggered after state-changing operations.
/// </summary>
public record EvaluateRulesCommand(Guid? TriggerBlockId = null) : ICommand;

// src/Features/Block/Rules/EvaluateRulesCommandHandler.cs
/// <summary>
/// Handler that evaluates rules and produces effects for any matches found.
/// </summary>
public class EvaluateRulesCommandHandler : IRequestHandler<EvaluateRulesCommand, Fin<Unit>>
{
    private readonly IRuleEngine<GridStateDto> _ruleEngine;
    private readonly ISimulationManager _simulationManager;
    private readonly IGridStateService _gridStateService;
    private readonly ILogger _logger;
    
    public EvaluateRulesCommandHandler(
        IRuleEngine<GridStateDto> ruleEngine,
        ISimulationManager simulationManager,
        IGridStateService gridStateService,
        ILogger logger)
    {
        _ruleEngine = ruleEngine;
        _simulationManager = simulationManager;
        _gridStateService = gridStateService;
        _logger = logger;
    }
    
    public Task<Fin<Unit>> Handle(EvaluateRulesCommand request, CancellationToken cancellationToken)
    {
        var result = 
            from gridState in GetCurrentGridState()
            from ruleMatches in _ruleEngine.EvaluateRules(gridState)
            select ProcessRuleMatches(ruleMatches, request.TriggerBlockId);
            
        return Task.FromResult(result);
    }
    
    private Fin<GridStateDto> GetCurrentGridState()
    {
        try
        {
            var gridState = _gridStateService.GetCurrentSnapshot();
            return Fin<GridStateDto>.Succ(gridState);
        }
        catch (Exception ex)
        {
            return Fin<GridStateDto>.Fail(
                Error.New(2001, $"Failed to get grid state: {ex.Message}", ex));
        }
    }
    
    private Unit ProcessRuleMatches(
        IReadOnlyCollection<IRuleMatchResult> matches, 
        Guid? triggerBlockId)
    {
        _logger.Information("Processing {MatchCount} rule matches", matches.Count);
        
        foreach (var match in matches)
        {
            _logger.Debug("Processing match for pattern {PatternType} with {BlockCount} blocks",
                match.PatternType, match.MatchedBlockIds.Count);
                
            foreach (var effect in match.Effects)
            {
                _simulationManager.Enqueue(effect);
            }
        }
        
        return unit;
    }
}
```

### 4.2 Automatic Rule Evaluation

```csharp
// src/Features/Block/Move/MoveBlockCommandHandler.cs (Updated)
/// <summary>
/// Updated move block handler that triggers rule evaluation after movement.
/// </summary>
public class MoveBlockCommandHandler : IRequestHandler<MoveBlockCommand, Fin<Unit>>
{
    private readonly IBlockRepository _blockRepository;
    private readonly IGridStateService _gridStateService;
    private readonly ISimulationManager _simulationManager;
    private readonly IMediator _mediator; // For triggering rule evaluation
    private readonly BlockIsNotLockedRule _blockIsNotLockedRule;
    
    // ... constructor and dependencies
    
    public Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
    {
        var result =
            from _ in ValidatePosition(request.ToPosition)
            from block in GetBlock(request.BlockId)
            from __ in _blockIsNotLockedRule.Check(block)
            select UpdateBlockPositionAndEvaluateRules(block, request.ToPosition);

        return Task.FromResult(result);
    }
    
    private Unit UpdateBlockPositionAndEvaluateRules(Block block, Vector2 newPosition)
    {
        // Update the block position
        block.Position = newPosition;
        _blockRepository.Save(block);
        _simulationManager.Enqueue(new BlockMovedEffect(block.Id, newPosition));
        
        // Trigger rule evaluation with the moved block as context
        var evaluateRulesCommand = new EvaluateRulesCommand(block.Id);
        _mediator.Send(evaluateRulesCommand); // Fire and forget
        
        return unit;
    }
    
    // ... other methods remain the same
}
```

## 5. Performance Considerations

### 5.1 Optimization Strategies

1. **Spatial Indexing**: Grid-based dictionary lookups provide O(1) access
2. **Priority-Based Evaluation**: Higher priority patterns evaluated first, conflicts resolved early
3. **Lazy Evaluation**: Pattern rotations generated only when needed
4. **Block Conflict Resolution**: Once a block is matched by a high-priority pattern, it's excluded from lower-priority patterns
5. **Chain Reaction Limits**: Maximum depth and cycle detection prevent infinite loops

### 5.2 Performance Metrics

Expected performance for a 50x50 grid with 500 blocks:
- Simple pattern matching: ~1-2ms
- Complex adjacency rules: ~5-10ms
- Chain reaction evaluation: ~10-20ms (depending on chain depth)

### 5.3 Hot-Path Considerations

For performance-critical scenarios (many blocks, frequent rule evaluation):
1. **Incremental Evaluation**: Only evaluate rules for blocks that changed
2. **Async Processing**: Use background threads for complex rule evaluation
3. **Caching**: Cache pattern match results for unchanged grid regions

## 6. Testing Strategy

### 6.1 Unit Testing

```csharp
// tests/Features/Block/Rules/ShapePatternMatcherTests.cs
[Fact]
public void ShapePatternMatcher_FindsT5Pattern_Successfully()
{
    // Arrange
    var t5Pattern = new ShapePattern(
        "T5_Pattern",
        new[] 
        { 
            new Vector2(0, 0),  // Center
            new Vector2(-1, 0), // Left
            new Vector2(1, 0),  // Right
            new Vector2(0, -1), // Top
            new Vector2(0, 1)   // Bottom
        },
        new[] { "Work", "Study" }
    );
    
    var spatialIndex = new GridSpatialIndex();
    var matcher = new ShapePatternMatcher(new[] { t5Pattern }, spatialIndex);
    
    var gridState = CreateGridStateWithT5Pattern();
    
    // Act
    var result = matcher.FindMatches(gridState);
    
    // Assert
    result.Should().BeSuccessful();
    result.Value.Should().HaveCount(1);
    result.Value.First().PatternType.Should().Be("T5_Pattern");
    result.Value.First().MatchedBlockIds.Should().HaveCount(5);
}

[Fact]
public void AdjacencyPatternMatcher_DetectsWorkStudyAdjacency_CreatesCareerOpportunity()
{
    // Arrange
    var adjacencyRule = new AdjacencyRule(
        "WorkStudyAdjacency",
        new[] { "Work" },
        new[] { "Study" },
        new RadiusAdjacencyCalculator(2.0f),
        new[] { new SpawnNewBlockEffect(Vector2.Zero, "CareerOpportunity") }
    );
    
    var spatialIndex = new GridSpatialIndex();
    var matcher = new AdjacencyPatternMatcher(new[] { adjacencyRule }, spatialIndex);
    
    var gridState = CreateGridStateWithAdjacentWorkStudyBlocks();
    
    // Act
    var result = matcher.FindMatches(gridState);
    
    // Assert
    result.Should().BeSuccessful();
    result.Value.Should().HaveCount(1);
    result.Value.First().Effects.Should().ContainSingle()
        .Which.Should().BeOfType<SpawnNewBlockEffect>()
        .Which.BlockType.Should().Be("CareerOpportunity");
}
```

### 6.2 Integration Testing

```csharp
// tests/Features/Block/Rules/RuleEngineIntegrationTests.cs
[Fact]
public async Task RuleEngine_EvaluatesComplexChainReaction_ProducesCorrectEffects()
{
    // Arrange
    var ruleEngine = CreateConfiguredRuleEngine();
    var gridState = CreateGridStateForChainReaction();
    
    // Act
    var result = ruleEngine.EvaluateRules(gridState);
    
    // Assert
    result.Should().BeSuccessful();
    result.Value.Should().HaveCountGreaterThan(1); // Chain reaction occurred
    
    // Verify specific effects
    var allEffects = result.Value.SelectMany(m => m.Effects).ToList();
    allEffects.Should().Contain(e => e is BlockPatternMatchedEffect);
    allEffects.Should().Contain(e => e is SpawnNewBlockEffect);
}
```

## 7. Rule Definition and Configuration

### 7.1 Rule Configuration System

```csharp
// src/Features/Block/Rules/Configuration/RuleConfiguration.cs
/// <summary>
/// Configuration system for defining rules in a structured, maintainable way.
/// </summary>
public class RuleConfiguration
{
    public IReadOnlyCollection<ShapePatternDefinition> ShapePatterns { get; init; } = Array.Empty<ShapePatternDefinition>();
    public IReadOnlyCollection<AdjacencyRuleDefinition> AdjacencyRules { get; init; } = Array.Empty<AdjacencyRuleDefinition>();
    public int MaxChainDepth { get; init; } = 10;
}

// src/Features/Block/Rules/Configuration/ShapePatternDefinition.cs
public record ShapePatternDefinition(
    string Name,
    Vector2[] RelativePositions,
    string[] AllowedBlockTypes,
    EffectDefinition[] Effects,
    int Priority = 0);

// src/Features/Block/Rules/Configuration/AdjacencyRuleDefinition.cs
public record AdjacencyRuleDefinition(
    string Name,
    string[] TriggerBlockTypes,
    string[] TargetBlockTypes,
    float MaxDistance,
    bool RequiresLineOfSight,
    EffectDefinition[] Effects);

// src/Features/Block/Rules/Configuration/EffectDefinition.cs
public record EffectDefinition(
    string EffectType,
    Dictionary<string, object> Parameters);
```

### 7.2 Rule Factory

```csharp
// src/Features/Block/Rules/Configuration/RuleEngineFactory.cs
/// <summary>
/// Factory for creating configured rule engines from rule definitions.
/// </summary>
public class RuleEngineFactory
{
    private readonly ISpatialIndex<BlockDto> _spatialIndex;
    private readonly ILogger _logger;
    
    public RuleEngineFactory(ISpatialIndex<BlockDto> spatialIndex, ILogger logger)
    {
        _spatialIndex = spatialIndex;
        _logger = logger;
    }
    
    public IRuleEngine<GridStateDto> CreateRuleEngine(RuleConfiguration config)
    {
        var ruleEngine = new RuleEngine(_spatialIndex, _logger);
        
        // Register shape pattern matchers
        if (config.ShapePatterns.Any())
        {
            var shapePatterns = config.ShapePatterns
                .Select(CreateShapePattern)
                .ToList();
            var shapePatternMatcher = new ShapePatternMatcher(shapePatterns, _spatialIndex);
            ruleEngine.RegisterMatcher(shapePatternMatcher);
        }
        
        // Register adjacency matchers
        if (config.AdjacencyRules.Any())
        {
            var adjacencyRules = config.AdjacencyRules
                .Select(CreateAdjacencyRule)
                .ToList();
            var adjacencyMatcher = new AdjacencyPatternMatcher(adjacencyRules, _spatialIndex);
            ruleEngine.RegisterMatcher(adjacencyMatcher);
        }
        
        // Register chain reaction matcher if there are base matchers
        var baseMatchers = new List<IPatternMatcher<GridStateDto>>();
        if (config.ShapePatterns.Any())
        {
            baseMatchers.Add(new ShapePatternMatcher(
                config.ShapePatterns.Select(CreateShapePattern).ToList(),
                _spatialIndex));
        }
        if (config.AdjacencyRules.Any())
        {
            baseMatchers.Add(new AdjacencyPatternMatcher(
                config.AdjacencyRules.Select(CreateAdjacencyRule).ToList(),
                _spatialIndex));
        }
        
        if (baseMatchers.Any())
        {
            var chainReactionMatcher = new ChainReactionMatcher(baseMatchers, config.MaxChainDepth);
            ruleEngine.RegisterMatcher(chainReactionMatcher);
        }
        
        return ruleEngine;
    }
    
    private ShapePattern CreateShapePattern(ShapePatternDefinition definition)
    {
        return new ShapePattern(
            definition.Name,
            definition.RelativePositions,
            definition.AllowedBlockTypes,
            definition.Priority
        );
    }
    
    private AdjacencyRule CreateAdjacencyRule(AdjacencyRuleDefinition definition)
    {
        var calculator = new RadiusAdjacencyCalculator(
            definition.MaxDistance,
            definition.RequiresLineOfSight
        );
        
        var effects = definition.Effects.Select(CreateEffect).ToList();
        
        return new AdjacencyRule(
            definition.Name,
            definition.TriggerBlockTypes,
            definition.TargetBlockTypes,
            calculator,
            effects
        );
    }
    
    private IEffect CreateEffect(EffectDefinition definition)
    {
        return definition.EffectType switch
        {
            "SpawnNewBlock" => new SpawnNewBlockEffect(
                (Vector2)definition.Parameters["Position"],
                (string)definition.Parameters["BlockType"]
            ),
            "BlockPatternMatched" => new BlockPatternMatchedEffect(
                (IReadOnlyCollection<Guid>)definition.Parameters["BlockIds"],
                (string)definition.Parameters["PatternType"],
                (int)definition.Parameters["Score"]
            ),
            _ => throw new ArgumentException($"Unknown effect type: {definition.EffectType}")
        };
    }
}
```

## 8. Concrete Control Flow Examples with Game Elements

### 8.1 Game Element Definitions

For the BlockLife entrepreneurship simulation, we have these core game elements:

- **Study**: Represents investing time in learning and research (builds knowledge)
- **Work**: Represents performing routine tasks to earn money (builds experience and capital)
- **Networking**: Represents building connections with people (builds social capital)
- **Insight**: A rare, valuable block generated through strategic combinations (represents breakthrough ideas)
- **Funding**: Represents accumulating sufficient capital for ventures

**Goal**: Strategically combine these life elements to achieve "Successful Entrepreneurship"

### 8.2 Specific Rule Definitions

```csharp
// src/Features/Block/Rules/Configuration/EntrepreneurshipRuleConfiguration.cs
public static class EntrepreneurshipRuleConfiguration
{
    public static RuleConfiguration CreateConfiguration()
    {
        return new RuleConfiguration
        {
            ShapePatterns = new[]
            {
                // T-5 Pattern: Study surrounded by Work blocks creates Innovation potential
                new ShapePatternDefinition(
                    "Innovation_T5",
                    new Vector2[]
                    {
                        new(0, 0),   // Center: Study
                        new(-1, 0),  // Left: Work
                        new(1, 0),   // Right: Work
                        new(0, -1),  // Top: Work
                        new(0, 1)    // Bottom: Work
                    },
                    new[] { "Study", "Work" },
                    new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 0), // Center position
                            ["BlockType"] = "Insight"
                        }),
                        new EffectDefinition("BlockPatternMatched", new Dictionary<string, object>
                        {
                            ["PatternType"] = "Innovation_T5",
                            ["Score"] = 1000
                        })
                    },
                    Priority: 100
                ),
                
                // T-4 Pattern: Study + 3 Work blocks creates smaller insight
                new ShapePatternDefinition(
                    "Learning_T4",
                    new Vector2[]
                    {
                        new(0, 0),   // Center: Study
                        new(-1, 0),  // Left: Work
                        new(1, 0),   // Right: Work
                        new(0, -1)   // Top: Work
                    },
                    new[] { "Study", "Work" },
                    new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 1), // Below center
                            ["BlockType"] = "Insight"
                        }),
                        new EffectDefinition("BlockPatternMatched", new Dictionary<string, object>
                        {
                            ["PatternType"] = "Learning_T4",
                            ["Score"] = 500
                        })
                    },
                    Priority: 90
                ),
                
                // Entrepreneurship Line: Insight + Funding + Networking in a line
                new ShapePatternDefinition(
                    "Entrepreneurship_Line",
                    new Vector2[]
                    {
                        new(0, 0),   // Insight
                        new(1, 0),   // Funding
                        new(2, 0)    // Networking
                    },
                    new[] { "Insight", "Funding", "Networking" },
                    new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(1, 0), // Replace center
                            ["BlockType"] = "SuccessfulEntrepreneurship"
                        }),
                        new EffectDefinition("BlockPatternMatched", new Dictionary<string, object>
                        {
                            ["PatternType"] = "Entrepreneurship_Line",
                            ["Score"] = 5000
                        })
                    },
                    Priority: 150
                )
            },
            
            AdjacencyRules = new[]
            {
                // Work + Study proximity generates career opportunities
                new AdjacencyRuleDefinition(
                    "CareerDevelopment",
                    TriggerBlockTypes: new[] { "Work" },
                    TargetBlockTypes: new[] { "Study" },
                    MaxDistance: 2.0f,
                    RequiresLineOfSight: false,
                    Effects: new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 0), // Will be calculated based on trigger
                            ["BlockType"] = "CareerOpportunity"
                        })
                    }
                ),
                
                // Networking + Work creates business connections
                new AdjacencyRuleDefinition(
                    "BusinessConnection",
                    TriggerBlockTypes: new[] { "Networking" },
                    TargetBlockTypes: new[] { "Work" },
                    MaxDistance: 1.5f,
                    RequiresLineOfSight: true,
                    Effects: new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 0), // Calculated
                            ["BlockType"] = "BusinessConnection"
                        })
                    }
                ),
                
                // Study + Networking creates knowledge sharing
                new AdjacencyRuleDefinition(
                    "KnowledgeSharing",
                    TriggerBlockTypes: new[] { "Study" },
                    TargetBlockTypes: new[] { "Networking" },
                    MaxDistance: 2.0f,
                    RequiresLineOfSight: false,
                    Effects: new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 0),
                            ["BlockType"] = "KnowledgeNetwork"
                        })
                    }
                ),
                
                // Multiple Work blocks near each other create Funding
                new AdjacencyRuleDefinition(
                    "AccumulateWealth",
                    TriggerBlockTypes: new[] { "Work" },
                    TargetBlockTypes: new[] { "Work" },
                    MaxDistance: 1.0f,
                    RequiresLineOfSight: false,
                    Effects: new[]
                    {
                        new EffectDefinition("SpawnNewBlock", new Dictionary<string, object>
                        {
                            ["Position"] = new Vector2(0, 0),
                            ["BlockType"] = "Funding"
                        })
                    }
                )
            },
            
            MaxChainDepth = 5 // Allow for complex chain reactions
        };
    }
}
```

### 8.3 Step-by-Step Control Flow Examples

#### 8.3.1 Example 1: Study + Work → Insight Generation

**Initial Grid State:**
```
[W] [W] [W]
[W] [S] [ ]  (W=Work, S=Study)
[ ] [ ] [ ]
```

**Control Flow:**

1. **User Places Final Work Block**
   ```csharp
   // User drops Work block at position (0, -1), completing T-5 pattern
   var moveCommand = new MoveBlockCommand(workBlockId, new Vector2(0, -1));
   await _mediator.Send(moveCommand);
   ```

2. **MoveBlockCommandHandler Triggers Rule Evaluation**
   ```csharp
   private Unit UpdateBlockPositionAndEvaluateRules(Block block, Vector2 newPosition)
   {
       // Update position first
       block.Position = newPosition;
       _blockRepository.Save(block);
       
       // Trigger rule evaluation immediately
       var evaluateRulesCommand = new EvaluateRulesCommand(block.Id);
       _mediator.Send(evaluateRulesCommand);
       
       return unit;
   }
   ```

3. **EvaluateRulesCommandHandler Processes Grid**
   ```csharp
   public Task<Fin<Unit>> Handle(EvaluateRulesCommand request, CancellationToken cancellationToken)
   {
       var result = 
           from gridState in GetCurrentGridState()           // Get current blocks
           from ruleMatches in _ruleEngine.EvaluateRules(gridState)  // Evaluate patterns
           select ProcessRuleMatches(ruleMatches, request.TriggerBlockId); // Apply effects
           
       return Task.FromResult(result);
   }
   ```

4. **RuleEngine Evaluates Patterns by Priority**
   ```csharp
   public Fin<IReadOnlyCollection<IRuleMatchResult>> EvaluateRules(GridStateDto context)
   {
       // Update spatial index
       _spatialIndex.UpdateIndex(context.Blocks);
       
       var allMatches = new List<IRuleMatchResult>();
       var usedBlocks = new HashSet<Guid>();
       
       // Evaluate ShapePatternMatcher (Priority: 100) first
       foreach (var matcher in _matchers.OrderByDescending(m => m.Priority))
       {
           var matchResult = matcher.FindMatches(context);
           // Filter conflicts with higher-priority matches
           var validMatches = matchResult.Value
               .Where(match => !match.MatchedBlockIds.Any(id => usedBlocks.Contains(id)));
           
           allMatches.AddRange(validMatches);
           // Mark blocks as used
           foreach (var match in validMatches)
               foreach (var blockId in match.MatchedBlockIds)
                   usedBlocks.Add(blockId);
       }
       
       return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(allMatches);
   }
   ```

5. **ShapePatternMatcher Detects T-5 Pattern**
   ```csharp
   private Option<IRuleMatchResult> TryMatchPattern(
       ShapePattern pattern, 
       Vector2 anchorPosition, 
       GridStateDto context)
   {
       var matchedBlocks = new List<Guid>();
       
       // Check each position in the T-5 pattern
       foreach (var relativePos in pattern.RelativePositions)
       {
           var worldPos = anchorPosition + relativePos;
           var blockAtPosition = _spatialIndex.GetBlockAt(worldPos);
           
           if (blockAtPosition.IsNone) return None;
           
           var block = blockAtPosition.Value;
           
           // For Innovation_T5: center must be Study, others must be Work
           if (relativePos == Vector2.Zero && block.BlockType != "Study") return None;
           if (relativePos != Vector2.Zero && block.BlockType != "Work") return None;
           
           matchedBlocks.Add(block.Id);
       }
       
       // Pattern matched! Create effects
       var effects = new IEffect[]
       {
           new BlockPatternMatchedEffect(matchedBlocks, "Innovation_T5", 1000),
           new SpawnNewBlockEffect(anchorPosition, "Insight") // Replace Study with Insight
       };
       
       return new ShapeMatchResult(matchedBlocks, effects, "Innovation_T5", metadata);
   }
   ```

6. **Effects Applied to Simulation**
   ```csharp
   private Unit ProcessRuleMatches(IReadOnlyCollection<IRuleMatchResult> matches, Guid? triggerBlockId)
   {
       foreach (var match in matches)
       {
           foreach (var effect in match.Effects)
           {
               _simulationManager.Enqueue(effect); // Queued for application
           }
       }
       
       return unit;
   }
   ```

7. **Final Grid State:**
   ```
   [W] [W] [W]
   [W] [I] [ ]  (I=Insight replaces Study)
   [ ] [ ] [ ]
   ```

#### 8.3.2 Example 2: Chain Reaction - Work Accumulation → Funding → Entrepreneurship

**Initial Grid State:**
```
[W] [W] [I] [N] [ ]  (W=Work, I=Insight, N=Networking)
[W] [W] [ ] [ ] [ ]
```

**Control Flow:**

1. **First Rule Evaluation - Adjacency Matcher Detects Work Clusters**
   ```csharp
   // AdjacencyPatternMatcher processes "AccumulateWealth" rule
   private IEnumerable<IRuleMatchResult> FindMatchesForRule(AdjacencyRule rule, GridStateDto context)
   {
       var triggerBlocks = context.Blocks.Where(b => b.BlockType == "Work");
       
       foreach (var triggerBlock in triggerBlocks)
       {
           var adjacentWorkBlocks = rule.AdjacencyCalculator
               .GetAdjacentBlocks(triggerBlock, context)
               .Where(b => b.BlockType == "Work");
               
           foreach (var targetBlock in adjacentWorkBlocks)
           {
               // Found Work-Work adjacency - spawn Funding
               yield return new AdjacencyMatchResult(
                   new[] { triggerBlock.Id, targetBlock.Id },
                   new[] { new SpawnNewBlockEffect(GetMidpoint(triggerBlock, targetBlock), "Funding") },
                   "AccumulateWealth",
                   metadata
               );
           }
       }
   }
   ```

2. **ChainReactionMatcher Detects Additional Patterns**
   ```csharp
   public Fin<IReadOnlyCollection<IRuleMatchResult>> FindMatches(GridStateDto context)
   {
       var allMatches = new List<IRuleMatchResult>();
       var currentContext = context;
       var chainDepth = 0;
       
       while (chainDepth < _maxChainDepth)
       {
           var currentMatches = new List<IRuleMatchResult>();
           
           // Evaluate all base matchers against current state
           foreach (var matcher in _baseMatchers)
           {
               var matchResult = matcher.FindMatches(currentContext);
               if (matchResult.IsSucc)
                   currentMatches.AddRange(matchResult.Value);
           }
           
           if (!currentMatches.Any()) break; // No more matches
           
           allMatches.AddRange(currentMatches);
           
           // Simulate effects to create new context for next iteration
           currentContext = ApplyEffectsToContext(currentContext, currentMatches);
           chainDepth++;
       }
       
       return Fin<IReadOnlyCollection<IRuleMatchResult>>.Succ(allMatches);
   }
   ```

3. **State After First Chain Step:**
   ```
   [W] [W] [I] [N] [ ]
   [W] [F] [ ] [ ] [ ]  (F=Funding spawned between Work blocks)
   ```

4. **Second Chain Step - ShapePatternMatcher Detects Entrepreneurship Pattern**
   ```csharp
   // Now we have: Insight at (2,0), Funding at (1,1), Networking at (3,0)
   // Check for "Entrepreneurship_Line" pattern with rotations
   
   foreach (var rotatedPattern in pattern.GetRotations())
   {
       // Try pattern: Insight-Funding-Networking in various orientations
       if (TryMatchEntrepreneurshipPattern(rotatedPattern, anchorPos, currentContext))
       {
           return new ShapeMatchResult(
               matchedBlocks: new[] { insightId, fundingId, networkingId },
               effects: new[] 
               { 
                   new SpawnNewBlockEffect(centerPos, "SuccessfulEntrepreneurship"),
                   new BlockPatternMatchedEffect(matchedBlocks, "Entrepreneurship_Line", 5000)
               },
               "Entrepreneurship_Line",
               metadata
           );
       }
   }
   ```

5. **Final State After Chain Reaction:**
   ```
   [W] [W] [I] [N] [ ]
   [W] [E] [ ] [ ] [ ]  (E=SuccessfulEntrepreneurship)
   ```

### 8.4 Pattern Matching Examples with Rotations

#### 8.4.1 T-5 Pattern Rotation Handling

```csharp
public class T5PatternExample
{
    public static void DemonstrateRotations()
    {
        var baseT5Pattern = new ShapePattern(
            "Innovation_T5",
            new Vector2[]
            {
                new(0, 0),   // Center: Study
                new(-1, 0),  // Left: Work
                new(1, 0),   // Right: Work
                new(0, -1),  // Top: Work
                new(0, 1)    // Bottom: Work
            },
            new[] { "Study", "Work" }
        );
        
        // Generate all 4 rotations automatically
        var allRotations = baseT5Pattern.GetRotations();
        
        // Rotation 0° (original):
        // [W]
        // [W][S][W]
        // [W]
        
        // Rotation 90°:
        // [W][W][W]
        // [S]
        // [W]
        
        // Rotation 180°:
        // [W]
        // [W][S][W]
        // [W]
        
        // Rotation 270°:
        // [W]
        // [S]
        // [W][W][W]
    }
}
```

#### 8.4.2 Complex Adjacency Calculations

```csharp
public class AdjacencyCalculationExample
{
    public void DemonstrateBusinessConnectionRule()
    {
        // Rule: Networking + Work within 1.5 units creates BusinessConnection
        var rule = new AdjacencyRule(
            "BusinessConnection",
            TriggerBlockTypes: new[] { "Networking" },
            TargetBlockTypes: new[] { "Work" },
            AdjacencyCalculator: new RadiusAdjacencyCalculator(1.5f, requiresLineOfSight: true),
            Effects: new[] { new SpawnNewBlockEffect(Vector2.Zero, "BusinessConnection") }
        );
        
        // Grid scenario:
        // [N] [ ] [W]  - Distance = 2.0 (too far)
        // [ ] [W] [ ]  - Distance = 1.41 (within range, line of sight clear)
        // [ ] [ ] [ ]
        
        var networkingBlock = new BlockDto(Guid.NewGuid(), new Vector2(0, 0), "Networking", false);
        var workBlock1 = new BlockDto(Guid.NewGuid(), new Vector2(2, 0), "Work", false);     // Too far
        var workBlock2 = new BlockDto(Guid.NewGuid(), new Vector2(1, 1), "Work", false);     // Valid
        
        var context = new GridStateDto(new[] { networkingBlock, workBlock1, workBlock2 });
        
        // AdjacencyPatternMatcher will only match workBlock2
        var validMatches = rule.AdjacencyCalculator.GetAdjacentBlocks(networkingBlock, context)
            .Where(b => rule.TargetBlockTypes.Contains(b.BlockType));
            
        // Result: Only workBlock2 matches, BusinessConnection spawned at midpoint
    }
}
```

### 8.5 Timing and Sequencing Examples

#### 8.5.1 Priority-Based Pattern Resolution

```csharp
public class PriorityResolutionExample
{
    public void DemonstrateConflictResolution()
    {
        // Scenario: One grid position could match multiple patterns
        
        // Grid State:
        // [W] [W] [W]
        // [W] [S] [W]  - Center Study could be part of T-5 OR T-4
        // [W] [ ] [ ]
        
        var patterns = new[]
        {
            new ShapePattern("Innovation_T5", t5Positions, allowedTypes, Priority: 100),
            new ShapePattern("Learning_T4", t4Positions, allowedTypes, Priority: 90)
        };
        
        // RuleEngine evaluation:
        var usedBlocks = new HashSet<Guid>();
        
        // 1. Innovation_T5 evaluated first (higher priority)
        var t5Match = FindT5Pattern(); // Matches! Uses all 5 blocks
        usedBlocks.UnionWith(t5Match.MatchedBlockIds);
        
        // 2. Learning_T4 evaluated second
        var t4Match = FindT4Pattern(); // Would match, but blocks already used
        var validT4 = t4Match.MatchedBlockIds.Any(id => usedBlocks.Contains(id)) 
            ? null  // Filtered out due to conflict
            : t4Match;
        
        // Result: Only T-5 pattern triggers, preventing double-processing
    }
}
```

#### 8.5.2 Chain Reaction Cycle Detection

```csharp
public class CycleDetectionExample
{
    public void DemonstrateCycleBreaking()
    {
        // Scenario: Chain reaction could theoretically loop forever
        
        var initialState = CreateGridWithPotentialCycle();
        var processedStates = new HashSet<string>();
        var chainDepth = 0;
        var currentContext = initialState;
        
        while (chainDepth < maxChainDepth)
        {
            // Create unique signature of current state
            var stateSignature = CreateStateSignature(currentContext);
            
            if (processedStates.Contains(stateSignature))
            {
                // Cycle detected! Same state seen before
                _logger.Warning("Chain reaction cycle detected at depth {Depth}, breaking chain", chainDepth);
                break;
            }
            
            processedStates.Add(stateSignature);
            
            // Find matches and apply effects
            var matches = FindAllMatches(currentContext);
            if (!matches.Any()) break; // Natural termination
            
            // Simulate effects for next iteration
            currentContext = ApplyEffectsToContext(currentContext, matches);
            chainDepth++;
        }
        
        // Result: Chain terminates safely, no infinite loops
    }
    
    private string CreateStateSignature(GridStateDto context)
    {
        // Create deterministic hash of grid state
        return string.Join("|", context.Blocks
            .OrderBy(b => b.Id)
            .Select(b => $"{b.Id}:{b.Position.X},{b.Position.Y}:{b.BlockType}"));
    }
}
```

### 8.6 Real-World Scenario: Building to Entrepreneurship

Here's a complete example showing how a player might progress from basic blocks to entrepreneurship:

```csharp
public class EntrepreneurshipScenarioExample
{
    public async Task DemonstrateFullGameplayFlow()
    {
        // Phase 1: Player places initial Work and Study blocks
        var workBlocks = await PlaceWorkBlocks();  // Builds experience
        var studyBlocks = await PlaceStudyBlocks(); // Builds knowledge
        
        // Phase 2: Adjacency rules trigger automatically
        // Work + Study proximity → CareerOpportunity
        // Multiple Work blocks → Funding accumulation
        
        // Phase 3: Player arranges T-5 pattern
        var t5Arrangement = await ArrangeT5Pattern(studyBlocks[0], workBlocks);
        
        // Automatic rule evaluation:
        // 1. ShapePatternMatcher detects Innovation_T5
        // 2. Study + 4 Work → Insight spawned
        // 3. ChainReactionMatcher evaluates new state
        
        // Phase 4: Player places Networking block
        var networkingBlock = await PlaceNetworkingBlock();
        
        // Phase 5: Final pattern completion
        // Insight + Funding + Networking → SuccessfulEntrepreneurship
        
        var finalState = await _gridStateService.GetCurrentSnapshot();
        var entrepreneurshipBlocks = finalState.Blocks
            .Where(b => b.BlockType == "SuccessfulEntrepreneurship");
            
        // Success! Player has achieved the game's primary objective
        Assert.True(entrepreneurshipBlocks.Any());
    }
    
    private async Task<Block[]> ArrangeT5Pattern(Block centerStudy, Block[] workBlocks)
    {
        // Move work blocks into T-5 formation around study block
        var center = centerStudy.Position;
        
        await _mediator.Send(new MoveBlockCommand(workBlocks[0].Id, center + new Vector2(-1, 0))); // Left
        await _mediator.Send(new MoveBlockCommand(workBlocks[1].Id, center + new Vector2(1, 0)));  // Right
        await _mediator.Send(new MoveBlockCommand(workBlocks[2].Id, center + new Vector2(0, -1))); // Top
        await _mediator.Send(new MoveBlockCommand(workBlocks[3].Id, center + new Vector2(0, 1)));  // Bottom
        
        // Each move triggers rule evaluation, but T-5 only completes on final move
        // Final move triggers: Innovation_T5 pattern → Insight creation
        
        return workBlocks;
    }
}
```

This demonstrates the complete flow from user interaction through pattern detection to chain reactions, showing how the architecture supports complex, emergent gameplay through simple rule definitions.

## 9. Conclusion and Recommendations

### 8.1 Why This Architecture is Superior to NRule

1. **Architectural Integrity**: Maintains Clean Architecture and functional programming principles
2. **Performance**: Optimized for real-time game scenarios with spatial indexing
3. **Testability**: Each component can be unit tested in isolation
4. **Flexibility**: Easy to add new pattern types and rule configurations
5. **Debuggability**: Clear execution flow with detailed logging
6. **Type Safety**: Full compile-time checking with C# type system

### 8.2 Implementation Roadmap

**Phase 1: Core Infrastructure (Week 1-2)**
- Implement core interfaces and spatial indexing
- Create basic rule engine and pattern matcher framework
- Set up comprehensive unit testing

**Phase 2: Shape Pattern System (Week 3)**
- Implement ShapePatternMatcher with T-5 and T-4 patterns
- Add pattern rotation and validation logic
- Create integration tests for shape matching

**Phase 3: Adjacency System (Week 4)**
- Implement AdjacencyPatternMatcher with radius-based calculations
- Add Work + Study → Career Opportunity rule
- Optimize spatial queries for performance

**Phase 4: Chain Reactions (Week 5)**
- Implement ChainReactionMatcher with cycle detection
- Add comprehensive testing for complex scenarios
- Performance optimization and profiling

**Phase 5: Integration and Polish (Week 6)**
- Integrate with existing CQRS command handlers
- Add rule configuration system
- Documentation and team training

### 8.3 Long-term Benefits

This architecture provides a solid foundation that can handle:
- Complex multi-step patterns
- Dynamic rule addition at runtime
- Performance optimization through spatial indexing
- Easy debugging and maintenance
- Comprehensive testing at all levels

The investment in this custom rule engine will pay dividends as the game's complexity grows, providing a maintainable and performant solution that aligns perfectly with the existing Clean Architecture principles.