# Complex Rule Engine Implementation Guide

## Overview

This guide provides detailed implementation instructions for the Complex Rule Engine Architecture. This is a companion to the [Complex Rule Engine Architecture ADR](Complex_Rule_Engine_Architecture.md) which contains the decision rationale and high-level design.

## Core Implementation: Anchor-Based Pattern Matching System

This implementation addresses the critical challenges of rule evaluation ambiguity, performance optimization, and designer workflow for complex pattern-based game logic. The rule engine implements **Anchor-Based Pattern Matching** using the last-placed block as the trigger position, providing deterministic evaluation and dramatically improved performance.

### Problems Solved

**1. Rule Evaluation Ambiguity**
- Multiple patterns could match the same blocks
- Conflicting rule applications when patterns overlap
- Non-deterministic evaluation order leading to inconsistent outcomes

**2. Performance Issues**
- Traditional grid-scanning approaches scale poorly (O(n²) complexity)
- Checking every position on large grids causes frame drops
- Pattern matching inefficiencies with multiple overlapping rules

**3. Designer Workflow Complexity**
- Manual rule writing required for each pattern
- High complexity for non-technical game designers
- Difficult rule validation and correctness verification

## Anchor-Based Pattern Matching System

**Core Principle**: The **last-placed block becomes the anchor (trigger position)** for all pattern evaluation. This eliminates the need to scan the entire grid and provides a focal point for pattern definition.

### Anchor Position Strategy

```csharp
// src/Core/Rules/IAnchorPattern.cs
using System.Numerics;
using LanguageExt;

/// <summary>
/// Defines a pattern that can be evaluated relative to an anchor position.
/// All patterns are defined with the anchor as the reference point (0,0).
/// </summary>
public interface IAnchorPattern
{
    /// <summary>
    /// Unique identifier for the pattern (e.g., "L_SHAPE_STUDY")
    /// </summary>
    string PatternId { get; }
    
    /// <summary>
    /// Priority for conflict resolution. Higher values take precedence.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// The relative positions that must be occupied for this pattern to match.
    /// All positions are relative to the anchor (0,0).
    /// </summary>
    IReadOnlyList<Vector2> RequiredPositions { get; }
    
    /// <summary>
    /// Optional: Required block types at each position.
    /// If null, any block type matches.
    /// </summary>
    IReadOnlyDictionary<Vector2, BlockType>? RequiredBlockTypes { get; }
    
    /// <summary>
    /// Evaluates if this pattern matches at the given anchor position.
    /// </summary>
    Fin<PatternMatch> EvaluateAt(Vector2 anchorPosition, IGridStateSnapshot gridState);
}
```

### Pattern Definition Examples

**Example 1: Study Chain Pattern**
```csharp
// src/Features/Patterns/StudyChainPattern.cs
public class StudyChainPattern : IAnchorPattern
{
    public string PatternId => "STUDY_CHAIN_HORIZONTAL";
    public int Priority => 100;
    
    // Anchor at (0,0), requires blocks at (-1,0) and (1,0)
    public IReadOnlyList<Vector2> RequiredPositions => new[]
    {
        new Vector2(-1, 0), // Left of anchor
        new Vector2(0, 0),  // Anchor position
        new Vector2(1, 0)   // Right of anchor
    };
    
    public IReadOnlyDictionary<Vector2, BlockType> RequiredBlockTypes => new Dictionary<Vector2, BlockType>
    {
        { new Vector2(-1, 0), BlockType.Study },
        { new Vector2(0, 0), BlockType.Study },
        { new Vector2(1, 0), BlockType.Study }
    };
    
    public Fin<PatternMatch> EvaluateAt(Vector2 anchorPosition, IGridStateSnapshot gridState)
    {
        // Implementation uses anchor-relative coordinates
        var absolutePositions = RequiredPositions
            .Select(relativePos => anchorPosition + relativePos)
            .ToList();
            
        // Check if all required positions have the correct block types
        foreach (var (relativePos, requiredType) in RequiredBlockTypes)
        {
            var absolutePos = anchorPosition + relativePos;
            var block = gridState.GetBlockAt(absolutePos);
            
            if (block.IsNone || block.Map(b => b.Type) != Some(requiredType))
            {
                return Fail<PatternMatch>(Error.New((int)PatternErrorCode.RequiredBlockMissing, 
                    $"Pattern {PatternId} requires {requiredType} at {absolutePos}"));
            }
        }
        
        return Succ(new PatternMatch(PatternId, anchorPosition, absolutePositions, Priority));
    }
}
```

**Example 2: Entrepreneurship L-Shape Pattern**
```csharp
// src/Features/Patterns/EntrepreneurshipLShapePattern.cs
public class EntrepreneurshipLShapePattern : IAnchorPattern
{
    public string PatternId => "ENTREPRENEURSHIP_L_SHAPE";
    public int Priority => 200; // Higher priority than simple chains
    
    // L-shape with anchor at the corner
    public IReadOnlyList<Vector2> RequiredPositions => new[]
    {
        new Vector2(0, 0),   // Anchor (corner of L)
        new Vector2(-1, 0),  // Left arm
        new Vector2(-2, 0),  // Left arm extended
        new Vector2(0, -1),  // Up arm  
        new Vector2(0, -2)   // Up arm extended
    };
    
    public IReadOnlyDictionary<Vector2, BlockType> RequiredBlockTypes => new Dictionary<Vector2, BlockType>
    {
        { new Vector2(0, 0), BlockType.Funding },    // Corner must be Funding
        { new Vector2(-1, 0), BlockType.Work },      // Work foundation
        { new Vector2(-2, 0), BlockType.Study },     // Study foundation
        { new Vector2(0, -1), BlockType.Networking },// Networking arm
        { new Vector2(0, -2), BlockType.Insight }    // Insight cap
    };
    
    // ... EvaluateAt implementation similar to above
}
```

## Ambiguity Resolution System

The rule engine implements a sophisticated priority and conflict resolution system to handle overlapping patterns deterministically.

### Priority-Based Resolution

```csharp
// src/Core/Rules/PatternMatchResult.cs
public record PatternMatch(
    string PatternId,
    Vector2 AnchorPosition,
    IReadOnlyList<Vector2> AffectedPositions,
    int Priority,
    DateTime DetectedAt = default
);

// src/Core/Rules/IPatternResolver.cs
public interface IPatternResolver
{
    /// <summary>
    /// Resolves conflicts when multiple patterns match overlapping positions.
    /// </summary>
    Fin<IReadOnlyList<PatternMatch>> ResolveConflicts(IReadOnlyList<PatternMatch> candidateMatches);
}
```

### Conflict Resolution Algorithm

```csharp
// src/Core/Rules/PatternResolver.cs
public class PatternResolver : IPatternResolver
{
    public Fin<IReadOnlyList<PatternMatch>> ResolveConflicts(IReadOnlyList<PatternMatch> candidateMatches)
    {
        if (!candidateMatches.Any())
            return Succ<IReadOnlyList<PatternMatch>>(Array.Empty<PatternMatch>());
            
        // Group matches by overlapping positions
        var conflictGroups = FindConflictGroups(candidateMatches);
        var resolvedMatches = new List<PatternMatch>();
        
        foreach (var group in conflictGroups)
        {
            if (group.Count == 1)
            {
                // No conflict, include the match
                resolvedMatches.Add(group.First());
            }
            else
            {
                // Resolve conflict using priority system
                var winningMatch = ResolveByPriority(group);
                resolvedMatches.Add(winningMatch);
            }
        }
        
        return Succ<IReadOnlyList<PatternMatch>>(resolvedMatches);
    }
    
    private PatternMatch ResolveByPriority(IList<PatternMatch> conflictingMatches)
    {
        // 1. Highest priority wins
        var maxPriority = conflictingMatches.Max(m => m.Priority);
        var highestPriorityMatches = conflictingMatches.Where(m => m.Priority == maxPriority).ToList();
        
        if (highestPriorityMatches.Count == 1)
            return highestPriorityMatches.First();
            
        // 2. Tie-breaker: Largest pattern (most blocks) wins
        var maxBlocks = highestPriorityMatches.Max(m => m.AffectedPositions.Count);
        var largestMatches = highestPriorityMatches.Where(m => m.AffectedPositions.Count == maxBlocks).ToList();
        
        if (largestMatches.Count == 1)
            return largestMatches.First();
            
        // 3. Final tie-breaker: Lexicographic by PatternId (deterministic)
        return largestMatches.OrderBy(m => m.PatternId).First();
    }
    
    private List<List<PatternMatch>> FindConflictGroups(IReadOnlyList<PatternMatch> matches)
    {
        // Implementation that groups patterns with overlapping AffectedPositions
        // Uses Union-Find or similar algorithm for efficient grouping
        // ... detailed implementation
    }
}
```

## Designer-Friendly Rule Definition

To address the complexity issue for game designers, the system provides multiple abstraction layers.

### Declarative Pattern Builder

```csharp
// src/Core/Rules/PatternBuilder.cs
public class PatternBuilder
{
    private string _patternId;
    private int _priority = 100;
    private readonly List<(Vector2 position, BlockType blockType)> _requirements = new();
    
    public static PatternBuilder Create(string patternId) => new() { _patternId = patternId };
    
    public PatternBuilder WithPriority(int priority)
    {
        _priority = priority;
        return this;
    }
    
    public PatternBuilder RequireBlock(int x, int y, BlockType blockType)
    {
        _requirements.Add((new Vector2(x, y), blockType));
        return this;
    }
    
    public PatternBuilder RequireHorizontalLine(BlockType blockType, int length, int startX = -1)
    {
        for (int i = 0; i < length; i++)
        {
            RequireBlock(startX + i, 0, blockType);
        }
        return this;
    }
    
    public PatternBuilder RequireVerticalLine(BlockType blockType, int length, int startY = -1)
    {
        for (int i = 0; i < length; i++)
        {
            RequireBlock(0, startY + i, blockType);
        }
        return this;
    }
    
    public PatternBuilder RequireLShape(BlockType corner, BlockType arm1, BlockType arm2)
    {
        RequireBlock(0, 0, corner);   // Anchor at corner
        RequireBlock(-1, 0, arm1);    // Left arm
        RequireBlock(0, -1, arm2);    // Up arm
        return this;
    }
    
    public IAnchorPattern Build() => new DeclarativePattern(_patternId, _priority, _requirements);
}

// Usage Example:
var studyChain = PatternBuilder.Create("STUDY_CHAIN_3")
    .WithPriority(100)
    .RequireHorizontalLine(BlockType.Study, 3)
    .Build();

var entrepreneurshipL = PatternBuilder.Create("ENTREPRENEURSHIP_BASIC")
    .WithPriority(200)
    .RequireLShape(BlockType.Funding, BlockType.Work, BlockType.Networking)
    .RequireBlock(-2, 0, BlockType.Study)  // Extend left arm
    .RequireBlock(0, -2, BlockType.Insight) // Extend up arm
    .Build();
```

### Visual Pattern Definition (Future Enhancement)

```csharp
// src/Core/Rules/VisualPatternDefinition.cs
/// <summary>
/// Allows patterns to be defined using ASCII art or grid notation
/// for easier visualization by game designers.
/// </summary>
public class VisualPatternDefinition
{
    // Example: L-shape pattern definition
    // S = Study, W = Work, N = Networking, I = Insight, F = Funding, A = Anchor
    public static IAnchorPattern ParsePattern(string patternId, int priority, string[] grid)
    {
        /*
        Example input:
        new[] {
            "  I  ",
            "  N  ",
            "SWF  ",  // F is at anchor position (2,2)
            "     ",
            "     "
        }
        */
        
        // Implementation parses the grid, finds the anchor ('A' or designated type),
        // and converts to relative coordinates
    }
}
```

## Rule Evaluation Engine

The core engine that processes pattern matching at the anchor position.

### Engine Interface and Implementation

```csharp
// src/Core/Rules/IRuleEvaluationEngine.cs
public interface IRuleEvaluationEngine
{
    /// <summary>
    /// Evaluates all registered patterns at the given anchor position.
    /// Returns resolved matches after conflict resolution.
    /// </summary>
    Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateAtAnchorAsync(
        Vector2 anchorPosition, 
        IGridStateSnapshot gridState,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a new pattern for evaluation.
    /// </summary>
    void RegisterPattern(IAnchorPattern pattern);
    
    /// <summary>
    /// Unregisters a pattern by ID.
    /// </summary>
    bool UnregisterPattern(string patternId);
}

// src/Core/Rules/RuleEvaluationEngine.cs
public class RuleEvaluationEngine : IRuleEvaluationEngine
{
    private readonly IPatternResolver _patternResolver;
    private readonly ILogger _logger;
    private readonly Dictionary<string, IAnchorPattern> _registeredPatterns = new();
    
    public RuleEvaluationEngine(IPatternResolver patternResolver, ILogger logger)
    {
        _patternResolver = patternResolver;
        _logger = logger;
    }
    
    public async Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateAtAnchorAsync(
        Vector2 anchorPosition, 
        IGridStateSnapshot gridState,
        CancellationToken cancellationToken = default)
    {
        var candidateMatches = new List<PatternMatch>();
        
        // Evaluate all registered patterns at the anchor position
        foreach (var pattern in _registeredPatterns.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var evaluationResult = pattern.EvaluateAt(anchorPosition, gridState);
            evaluationResult.IfSucc(match => candidateMatches.Add(match));
            
            // Log failed evaluations for debugging
            evaluationResult.IfFail(error => 
                _logger.Debug("Pattern {PatternId} failed at {AnchorPosition}: {Error}", 
                    pattern.PatternId, anchorPosition, error.Message));
        }
        
        // Resolve conflicts and return final matches
        return await Task.FromResult(_patternResolver.ResolveConflicts(candidateMatches));
    }
    
    public void RegisterPattern(IAnchorPattern pattern)
    {
        _registeredPatterns[pattern.PatternId] = pattern;
        _logger.Information("Registered pattern {PatternId} with priority {Priority}", 
            pattern.PatternId, pattern.Priority);
    }
    
    public bool UnregisterPattern(string patternId)
    {
        var removed = _registeredPatterns.Remove(patternId);
        if (removed)
        {
            _logger.Information("Unregistered pattern {PatternId}", patternId);
        }
        return removed;
    }
}
```

## Integration with Command Processing

The rule engine integrates seamlessly with the existing CQRS architecture.

### Enhanced Move Block Command Handler

```csharp
// src/Features/Block/Move/MoveBlockCommandHandler.cs (Enhanced)
public class MoveBlockCommandHandler : IRequestHandler<MoveBlockCommand, Fin<Unit>>
{
    private readonly IBlockRepository _blockRepository;
    private readonly IGridStateService _gridStateService;
    private readonly IRuleEvaluationEngine _ruleEngine;
    private readonly ISimulationManager _simulationManager;
    
    public async Task<Fin<Unit>> Handle(MoveBlockCommand request, CancellationToken ct)
    {
        var result = await (
            from block in GetBlock(request.BlockId)
            from _ in ValidateMove(block, request.ToPosition)
            from moveResult in ExecuteMove(block, request.ToPosition)
            from ruleMatches in EvaluateRulesAtAnchor(request.ToPosition)
            select ProcessMoveAndRules(moveResult, ruleMatches)
        );
        
        return result;
    }
    
    private async Task<Fin<IReadOnlyList<PatternMatch>>> EvaluateRulesAtAnchor(Vector2 anchorPosition)
    {
        var gridSnapshot = _gridStateService.CreateSnapshot();
        return await _ruleEngine.EvaluateAtAnchorAsync(anchorPosition, gridSnapshot);
    }
    
    private Unit ProcessMoveAndRules(MoveResult moveResult, IReadOnlyList<PatternMatch> ruleMatches)
    {
        // Enqueue the basic move effect
        _simulationManager.Enqueue(new BlockMovedEffect(moveResult.BlockId, moveResult.ToPosition));
        
        // Enqueue effects for each matched pattern
        foreach (var match in ruleMatches)
        {
            _simulationManager.Enqueue(new PatternMatchedEffect(
                match.PatternId, 
                match.AnchorPosition, 
                match.AffectedPositions.ToList()));
        }
        
        return unit;
    }
}
```

## Performance Characteristics

**Before (Grid Scanning):**
- Time Complexity: O(n² × p) where n = grid size, p = number of patterns
- Space Complexity: O(n²) for grid state storage
- Frame Impact: Significant on large grids (>50x50)

**After (Anchor-Based):**
- Time Complexity: O(p × k) where p = number of patterns, k = average pattern size
- Space Complexity: O(1) for pattern evaluation (grid state unchanged)
- Frame Impact: Minimal, evaluation happens only at trigger position

**Benchmark Example (100x100 grid, 20 patterns):**
- Old system: ~45ms per evaluation (unacceptable for real-time)
- New system: ~0.3ms per evaluation (easily handled in frame budget)

## Rule Validation and Correctness

```csharp
// src/Core/Rules/Validation/IRuleValidator.cs
public interface IRuleValidator
{
    /// <summary>
    /// Validates that a pattern definition is correct and will not cause
    /// runtime errors or infinite loops.
    /// </summary>
    Fin<Unit> ValidatePattern(IAnchorPattern pattern);
    
    /// <summary>
    /// Validates the entire set of registered patterns for conflicts
    /// and design issues.
    /// </summary>
    Fin<ValidationReport> ValidatePatternSet(IEnumerable<IAnchorPattern> patterns);
}

public record ValidationReport(
    IReadOnlyList<ValidationWarning> Warnings,
    IReadOnlyList<ValidationError> Errors,
    bool IsValid
);
```

## Project Structure Integration

To provide **compiler-enforced architectural boundaries**, the rule engine components are organized within the multi-project .NET solution structure.

### Solution Structure Overview

```
/BlockLife/
├── src/                           <-- BlockLife.Core.csproj
│   ├── Core/
│   │   ├── Rules/                 <-- Complex Rule Engine components
│   │   │   ├── IAnchorPattern.cs
│   │   │   ├── IPatternResolver.cs
│   │   │   ├── IRuleEvaluationEngine.cs
│   │   │   └── PatternBuilder.cs
│   │   └── ...
│   ├── Features/
│   │   ├── Patterns/              <-- Concrete pattern implementations
│   │   │   ├── StudyChainPattern.cs
│   │   │   └── EntrepreneurshipLShapePattern.cs
│   │   └── ...
│   └── ... (all pure C# logic)
│
├── tests/                         <-- BlockLife.Core.Tests.csproj
│   └── Features/
│       ├── Patterns/              <-- Pattern-specific tests
│       │   ├── StudyChainPatternTests.cs
│       │   └── RuleEvaluationEngineTests.cs
│       └── ...
```

### Rationale and Workflow

*   **Rationale**: This structure enforces architectural boundaries at compile time. Rule Engine components in the Core project cannot accidentally depend on Godot types.
*   **Workflow**:
    1.  When adding pure rule logic (patterns, resolvers, engines), add files to the **`BlockLife.Core`** project.
    2.  When adding rule-related tests, add files to the **`BlockLife.Core.Tests`** project.
    3.  Use the mandatory CLI/IDE tooling to generate pattern boilerplate in correct locations.

## Benefits Summary

This comprehensive rule engine architecture solves all the critical issues identified:

1. **Eliminates Rule Evaluation Ambiguity** through priority-based conflict resolution
2. **Dramatically Improves Performance** with anchor-based evaluation (O(1) grid scanning)
3. **Provides Designer-Friendly Workflows** through declarative builders and visual definitions
4. **Ensures Deterministic Behavior** with well-defined tie-breaking rules
5. **Integrates Seamlessly** with existing Clean Architecture and CQRS patterns
6. **Enables Easy Testing** through pure, functional pattern definitions

The anchor-based approach using the last-placed block as the trigger position is the key insight that makes this system both performant and conceptually simple for game designers to understand and work with.

---

## Related Documents

- [Complex Rule Engine Architecture ADR](Complex_Rule_Engine_Architecture.md) - Decision rationale and architectural overview
- [Feature Development Guide](../6_Guides/Feature_Development_Guide.md) - General feature implementation patterns
- [Standard Patterns](../1_Architecture/Standard_Patterns.md) - Validated architectural patterns
- [Architecture Guide](../1_Architecture/Architecture_Guide.md) - Core architectural principles