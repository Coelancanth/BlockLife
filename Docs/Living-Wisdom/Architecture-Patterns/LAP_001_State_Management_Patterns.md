# State Management Patterns

**Document ID**: LAP_001  
**Version**: 1.0  
**Last Updated**: 2025-08-15  
**Owner**: Architect Agent  
**Status**: Active  
**Evolution History**: 
- v1.0: Extracted from F1 Architecture Stress Test Report (2025-08-15)

## Purpose

This document captures proven state management patterns for BlockLife's Clean Architecture implementation. Born from the F1 Block Placement stress test that revealed critical dual-state management vulnerabilities and their solutions.

## 🚨 CRITICAL PATTERN: Single Source of Truth

### The Anti-Pattern (NEVER DO THIS)

```csharp
// ❌ DUAL STATE DISASTER - Creates race conditions
services.AddSingleton<IGridStateService, GridStateService>();
services.AddSingleton<IBlockRepository, InMemoryBlockRepository>();

// Results in TWO separate data stores:
// - GridStateService: Uses ConcurrentDictionary (thread-safe)
// - InMemoryBlockRepository: Uses Dictionary (NOT thread-safe)
```

**Why This Fails:**
- **Race Conditions**: Plain Dictionary throws InvalidOperationException under concurrent access
- **State Drift**: Two stores managing same data get out of sync
- **Phantom Data**: Blocks exist in one store but not the other
- **Silent Corruption**: Data loss without error indication

### The Correct Pattern (ALWAYS DO THIS)

```csharp
// ✅ SINGLE SOURCE OF TRUTH - Thread-safe and consistent
services.AddSingleton<GridStateService>();
services.AddSingleton<IGridStateService>(provider => provider.GetRequiredService<GridStateService>());
services.AddSingleton<IBlockRepository>(provider => provider.GetRequiredService<GridStateService>());
```

**Why This Works:**
- **Single Instance**: One implementation serving multiple interface contracts
- **Thread Safety**: ConcurrentDictionary handles concurrent operations safely
- **State Consistency**: Impossible for interfaces to drift out of sync
- **Performance**: Single lookup, no data synchronization overhead

## State Management Implementation Patterns

### Pattern 1: Consolidated Service Implementation

```csharp
public class GridStateService : IGridStateService, IBlockRepository
{
    private readonly ConcurrentDictionary<Guid, Block> _blocksById;
    private readonly ConcurrentDictionary<GridPosition, Guid> _blocksByPosition;
    
    // IGridStateService methods
    public Option<Block> GetBlockAt(GridPosition position) => 
        _blocksByPosition.TryGetValue(position, out var blockId) 
            ? _blocksById.TryGetValue(blockId, out var block) ? block : Option<Block>.None
            : Option<Block>.None;
    
    // IBlockRepository methods  
    public async Task<Fin<Unit>> AddBlockAsync(Block block)
    {
        _blocksById.TryAdd(block.Id, block);
        _blocksByPosition.TryAdd(block.Position, block.Id);
        return Unit.Default;
    }
}
```

**Key Principles:**
- **Single Data Store**: One ConcurrentDictionary per data type
- **Interface Separation**: Multiple interfaces, single implementation
- **Thread-Safe Operations**: Use TryAdd, TryRemove, TryUpdate patterns
- **Atomic Updates**: Ensure related data stays synchronized

### Pattern 2: Thread-Safe Collection Choices

```csharp
// ✅ CORRECT - Thread-safe collections
private readonly ConcurrentDictionary<TKey, TValue> _data = new();
private readonly ConcurrentQueue<TItem> _queue = new();
private readonly ConcurrentBag<TItem> _bag = new();

// ❌ WRONG - Not thread-safe
private readonly Dictionary<TKey, TValue> _data = new();
private readonly Queue<TItem> _queue = new();
private readonly List<TItem> _list = new();
```

**Collection Guidelines:**
- **Key-Value Data**: Always use ConcurrentDictionary
- **Queue Operations**: ConcurrentQueue for FIFO operations
- **Unordered Collection**: ConcurrentBag for producer/consumer scenarios
- **Ordered Lists**: Use immutable collections with replacement patterns

### Pattern 3: State Consistency Validation

```csharp
public class StateConsistencyValidator
{
    public Fin<Unit> ValidateConsistency()
    {
        // Verify position index matches block positions
        foreach (var (position, blockId) in _blocksByPosition)
        {
            if (!_blocksById.TryGetValue(blockId, out var block))
                return Error.New("INCONSISTENT_STATE", $"Position {position} references missing block {blockId}");
                
            if (block.Position != position)
                return Error.New("POSITION_MISMATCH", $"Block {blockId} position mismatch: index={position}, block={block.Position}");
        }
        
        return Unit.Default;
    }
}
```

## DI Container Configuration Patterns

### Pattern 1: Interface Delegation

```csharp
// Register the concrete implementation
services.AddSingleton<ConcreteService>();

// Delegate interfaces to the same instance
services.AddSingleton<IInterface1>(provider => provider.GetRequiredService<ConcreteService>());
services.AddSingleton<IInterface2>(provider => provider.GetRequiredService<ConcreteService>());
services.AddSingleton<IInterface3>(provider => provider.GetRequiredService<ConcreteService>());
```

### Pattern 2: Registration Validation

```csharp
// Validate single instance registration
public static void ValidateRegistrations(IServiceProvider provider)
{
    var gridService = provider.GetRequiredService<IGridStateService>();
    var blockRepo = provider.GetRequiredService<IBlockRepository>();
    
    if (!ReferenceEquals(gridService, blockRepo))
        throw new InvalidOperationException("IGridStateService and IBlockRepository must be same instance");
}
```

## State Update Patterns

### Pattern 1: Atomic State Updates

```csharp
public async Task<Fin<Unit>> MoveBlockAsync(Guid blockId, GridPosition newPosition)
{
    // Get current state
    if (!_blocksById.TryGetValue(blockId, out var block))
        return Error.New("BLOCK_NOT_FOUND", $"Block {blockId} not found");
    
    var oldPosition = block.Position;
    var updatedBlock = block with { Position = newPosition };
    
    // Atomic update: position index first, then block
    _blocksByPosition.TryRemove(oldPosition, out _);
    _blocksByPosition.TryAdd(newPosition, blockId);
    _blocksById.TryUpdate(blockId, updatedBlock, block);
    
    return Unit.Default;
}
```

### Pattern 2: Rollback Safety

```csharp
public async Task<Fin<Unit>> UpdateWithRollback(Guid blockId, Func<Block, Block> updateFunc)
{
    if (!_blocksById.TryGetValue(blockId, out var originalBlock))
        return Error.New("BLOCK_NOT_FOUND", $"Block {blockId} not found");
    
    try
    {
        var updatedBlock = updateFunc(originalBlock);
        
        // Try to update
        if (!_blocksById.TryUpdate(blockId, updatedBlock, originalBlock))
        {
            // Rollback not needed - update failed due to concurrent modification
            return Error.New("CONCURRENT_MODIFICATION", "Block was modified by another operation");
        }
        
        return Unit.Default;
    }
    catch (Exception ex)
    {
        // Update function failed, state remains unchanged
        return Error.New("UPDATE_FAILED", ex.Message);
    }
}
```

## Performance Patterns

### Pattern 1: Efficient Lookups

```csharp
// ✅ CORRECT - Single dictionary lookup
public Option<Block> GetBlockAt(GridPosition position)
{
    return _blocksByPosition.TryGetValue(position, out var blockId) && 
           _blocksById.TryGetValue(blockId, out var block) 
           ? block 
           : Option<Block>.None;
}

// ❌ WRONG - Multiple enumerations
public Option<Block> GetBlockAt(GridPosition position)
{
    var block = _blocksById.Values.FirstOrDefault(b => b.Position == position);
    return block != null ? block : Option<Block>.None;
}
```

### Pattern 2: Batch Operations

```csharp
public async Task<Fin<Unit>> AddBlocksAsync(IEnumerable<Block> blocks)
{
    // Validate all blocks first
    var blockList = blocks.ToList();
    foreach (var block in blockList)
    {
        if (_blocksById.ContainsKey(block.Id))
            return Error.New("DUPLICATE_BLOCK", $"Block {block.Id} already exists");
        if (_blocksByPosition.ContainsKey(block.Position))
            return Error.New("POSITION_OCCUPIED", $"Position {block.Position} already occupied");
    }
    
    // Add all blocks atomically
    foreach (var block in blockList)
    {
        _blocksById.TryAdd(block.Id, block);
        _blocksByPosition.TryAdd(block.Position, block.Id);
    }
    
    return Unit.Default;
}
```

## Testing Patterns for State Management

### Pattern 1: Concurrent Operation Testing

```csharp
[Fact]
public async Task ConcurrentOperations_MaintainStateConsistency()
{
    var tasks = Enumerable.Range(0, 100)
        .Select(i => Task.Run(async () =>
        {
            var block = new Block(Guid.NewGuid(), new GridPosition(i % 10, i / 10), BlockType.Stone);
            await _stateService.AddBlockAsync(block);
        }))
        .ToArray();
    
    await Task.WhenAll(tasks);
    
    // Verify consistency
    var allBlocks = await _stateService.GetAllBlocksAsync();
    allBlocks.Should().HaveCount(100);
    
    // Verify no position conflicts
    var positions = allBlocks.Select(b => b.Position).ToHashSet();
    positions.Should().HaveCount(100, "No two blocks should occupy the same position");
}
```

### Pattern 2: State Consistency Validation

```csharp
[Fact]
public async Task StateOperations_MaintainIndexConsistency()
{
    // Arrange
    var blocks = GenerateTestBlocks(50);
    
    // Act
    foreach (var block in blocks)
        await _stateService.AddBlockAsync(block);
    
    // Assert - Verify position index matches block positions
    foreach (var block in blocks)
    {
        var foundBlock = _stateService.GetBlockAt(block.Position);
        foundBlock.Should().NotBeNull();
        foundBlock.Value.Id.Should().Be(block.Id);
    }
}
```

## Failure Mode Prevention

### Common Failures and Prevention

1. **Dictionary Modification During Enumeration**
   - **Prevention**: Use ConcurrentDictionary or create snapshots
   - **Detection**: InvalidOperationException during iteration

2. **Lost Updates in Concurrent Scenarios**
   - **Prevention**: Use TryUpdate with original value comparison
   - **Detection**: Stress testing with concurrent operations

3. **Memory Leaks from Static References**
   - **Prevention**: Proper disposal patterns and weak references
   - **Detection**: Memory profiling during long-running tests

4. **State Drift Between Multiple Stores**
   - **Prevention**: Single source of truth pattern
   - **Detection**: Consistency validation in tests

## Evolution Notes

This pattern library evolved from the F1 Block Placement stress test that revealed catastrophic dual-state management vulnerabilities. The "Single Source of Truth" principle became the foundation for all subsequent state management decisions.

**Key Lesson**: Thread safety is not optional in concurrent systems, and state management complexity grows exponentially with the number of data stores.

## References

- **F1_Architecture_Stress_Test_Report.md**: Original issue discovery (archived)
- **LWP_001_Stress_Testing_Playbook.md**: Testing procedures that validate these patterns
- **LWP_004_Production_Readiness_Checklist.md**: Production validation including state management
- **Critical_Architecture_Fixes_Post_Mortem.md**: Specific fixes applied (archived)