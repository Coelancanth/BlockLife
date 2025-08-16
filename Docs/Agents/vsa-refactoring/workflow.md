# VSA Refactoring Workflow

## Purpose
Define procedures for the VSA Refactoring agent to identify code duplication across vertical slices, extract shared code properly while preserving slice boundaries, and maintain clean VSA architecture.

---

## Core Workflow Actions

### 1. Analyze Code Duplication Across Slices

**Trigger**: "Check for duplication" or "Analyze VSA structure"

**Input Required**:
- Feature slices to analyze
- Threshold for duplication concern (default: 3+ slices)
- Areas of concern (if specific)

**Steps**:

1. **Scan All Feature Slices**
   ```
   Analyze structure:
   src/Features/
   ├── Block/Move/
   ├── Block/Place/
   ├── Block/Remove/
   ├── Inventory/Add/
   └── Inventory/Remove/
   
   Compare across slices:
   - Similar class structures
   - Repeated code patterns
   - Identical utility functions
   - Common validation logic
   - Shared constants/enums
   ```

2. **Identify Duplication Patterns**
   ```csharp
   // Pattern 1: Infrastructure Service Duplication
   // Found in: Block/Move, Block/Place, Block/Remove
   public class Handler
   {
       private Block? FindBlockAt(GridPosition pos)
       {
           return _blocks.FirstOrDefault(b => b.Position == pos);
       }
   }
   
   // Pattern 2: Value Object Duplication
   // Found in: Multiple slices
   public record Position(int X, int Y)
   {
       public static Position Zero => new(0, 0);
   }
   
   // Pattern 3: Validation Duplication  
   // Found in: Multiple handlers
   private bool IsValidPosition(GridPosition pos)
   {
       return pos.X >= 0 && pos.Y >= 0 && pos.X < GridWidth;
   }
   ```

3. **Categorize Duplication by Type**
   ```
   Infrastructure (Extract):
   - Database/repository logic
   - External service calls
   - Framework integrations
   - Logging patterns
   
   Domain Primitives (Extract):
   - Value objects (Position, BlockId)
   - Domain errors
   - Mathematical utilities
   - Business constants
   
   Technical Patterns (Extract):
   - Base classes for handlers
   - Common interfaces
   - DI registration patterns
   - Notification patterns
   
   Business Logic (Keep Duplicated):
   - Feature-specific workflows
   - Feature validation rules
   - Feature-specific DTOs
   - UI components
   ```

4. **Assess Extraction Benefit**
   ```
   For each duplication:
   - How many slices affected? (3+ = consider extraction)
   - Is it identical or similar?
   - Will it diverge in future?
   - Does extraction break slice independence?
   - What's the coupling cost?
   ```

**Output Format**:
```
📊 VSA Duplication Analysis

INFRASTRUCTURE DUPLICATION (Extract):
1. BlockQueryService logic - 4 slices affected
2. GridValidation - 3 slices affected
3. CommandLogging pattern - 5 slices affected

DOMAIN DUPLICATION (Extract):
1. GridPosition record - 6 slices affected  
2. ValidationError types - 4 slices affected

BUSINESS DUPLICATION (Keep):
1. Feature-specific DTOs - Expected per slice
2. UI components - Slice independence important

RECOMMENDATION: Extract 5 patterns, keep 2 duplicated
```

---

### 2. Extract Shared Infrastructure

**Trigger**: "Extract infrastructure from slices"

**Input Required**:
- Specific duplication to extract
- Target location for extraction
- Affected slices list

**Steps**:

1. **Design Extraction Structure**
   ```
   Plan extraction target:
   
   src/Infrastructure/Services/
   ├── BlockQueryService.cs      # From: Block slices
   ├── GridValidationService.cs  # From: Multiple slices  
   └── CommandAuditService.cs    # From: All command handlers
   
   src/Infrastructure/Interfaces/
   ├── IBlockQueryService.cs
   └── IGridValidationService.cs
   ```

2. **Extract Service Implementation**
   ```csharp
   // Before: In each slice handler
   public class MoveBlockHandler
   {
       private Block? FindBlockAt(GridPosition pos)
       {
           return _context.Blocks.FirstOrDefault(b => b.Position == pos);
       }
   }
   
   // After: Extracted service
   public interface IBlockQueryService
   {
       Task<Block?> GetBlockAt(GridPosition position);
       Task<IReadOnlyList<Block>> GetBlocksInArea(GridArea area);
   }
   
   public class BlockQueryService : IBlockQueryService
   {
       private readonly IBlockRepository _repository;
       
       public async Task<Block?> GetBlockAt(GridPosition position)
       {
           return await _repository.FindByPosition(position);
       }
   }
   ```

3. **Update Slice Handlers**
   ```csharp
   // Updated handler using extracted service
   public class MoveBlockHandler : IRequestHandler<MoveBlockCommand, Fin<Unit>>
   {
       private readonly IBlockQueryService _blockQuery;
       private readonly IGridValidationService _gridValidation;
       
       public MoveBlockHandler(
           IBlockQueryService blockQuery,
           IGridValidationService gridValidation)
       {
           _blockQuery = blockQuery;
           _gridValidation = gridValidation;
       }
       
       public async Task<Fin<Unit>> Handle(MoveBlockCommand cmd)
       {
           // Use injected services instead of duplicated logic
           var block = await _blockQuery.GetBlockAt(cmd.FromPosition);
           var isValid = await _gridValidation.IsValidMove(cmd.FromPosition, cmd.ToPosition);
           
           // Feature-specific logic remains in slice
           if (!isValid)
               return Error.New("Invalid move");
               
           // etc.
       }
   }
   ```

4. **Update DI Registration**
   ```csharp
   // In GameStrapper or DI configuration
   services.AddSingleton<IBlockQueryService, BlockQueryService>();
   services.AddSingleton<IGridValidationService, GridValidationService>();
   ```

5. **Remove Duplicated Code**
   ```
   Delete from each affected slice:
   - Duplicated methods
   - Inline implementations
   - Local utility functions
   
   Verify all references updated to use injected services
   ```

**Output**: Clean slices using shared infrastructure services

---

### 3. Extract Domain Primitives

**Trigger**: "Extract domain objects" or "Consolidate value objects"

**Input Required**:
- Duplicated domain concepts
- Target domain layer location

**Steps**:

1. **Identify Domain Primitives**
   ```csharp
   // Found duplicated across slices:
   
   // In Block/Move slice:
   public record GridPosition(int X, int Y);
   
   // In Block/Place slice:  
   public record Position(int X, int Y);
   
   // In Inventory slice:
   public record Coordinates(int X, int Y);
   ```

2. **Design Canonical Version**
   ```csharp
   // src/Domain/ValueObjects/GridPosition.cs
   public record GridPosition(int X, int Y)
   {
       public static GridPosition Zero => new(0, 0);
       public static GridPosition Invalid => new(-1, -1);
       
       public GridPosition Move(int deltaX, int deltaY) =>
           new(X + deltaX, Y + deltaY);
           
       public double DistanceTo(GridPosition other) =>
           Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
           
       public bool IsAdjacent(GridPosition other) =>
           Math.Abs(X - other.X) + Math.Abs(Y - other.Y) == 1;
   }
   ```

3. **Update All Slices**
   ```csharp
   // Replace in all feature slices:
   
   // Before:
   public record MoveBlockCommand(BlockId Id, Position To);
   
   // After:  
   using BlockLife.Domain.ValueObjects;
   public record MoveBlockCommand(BlockId Id, GridPosition To);
   ```

4. **Extract Domain Errors**
   ```csharp
   // src/Domain/Errors/DomainErrors.cs
   public static class DomainErrors
   {
       public static Error InvalidPosition(GridPosition pos) =>
           Error.New("INVALID_POSITION", $"Position {pos} is invalid");
           
       public static Error BlockNotFound(BlockId id) =>
           Error.New("BLOCK_NOT_FOUND", $"Block {id} not found");
   }
   ```

**Output**: Consolidated domain layer with shared primitives

---

### 4. Create Pattern Base Classes

**Trigger**: "Extract common patterns" or "Create base classes"

**Input Required**:
- Repeated patterns across slices
- Common functionality to abstract

**Steps**:

1. **Identify Common Patterns**
   ```csharp
   // Pattern: Command Handler Boilerplate
   // Repeated in every handler:
   public class SomeHandler
   {
       private readonly ILogger _logger;
       
       public async Task<Fin<Unit>> Handle(Command cmd)
       {
           _logger.Information("Handling {Command}", cmd.GetType());
           
           try
           {
               // Actual logic
               var result = await DoWork(cmd);
               
               _logger.Information("Completed {Command}", cmd.GetType());
               return result;
           }
           catch (Exception ex)
           {
               _logger.Error(ex, "Failed {Command}", cmd.GetType());
               return Error.New("HANDLER_ERROR", ex.Message);
           }
       }
   }
   ```

2. **Design Base Class**
   ```csharp
   // src/Infrastructure/Handlers/CommandHandlerBase.cs
   public abstract class CommandHandlerBase<TCommand> 
       : IRequestHandler<TCommand, Fin<Unit>>
       where TCommand : IRequest<Fin<Unit>>
   {
       protected readonly ILogger Logger;
       
       protected CommandHandlerBase(ILogger logger)
       {
           Logger = logger;
       }
       
       public async Task<Fin<Unit>> Handle(TCommand command, CancellationToken ct)
       {
           var commandType = typeof(TCommand).Name;
           Logger.Information("Handling {Command}", commandType);
           
           try
           {
               var result = await HandleCore(command, ct);
               
               Logger.Information("Completed {Command}", commandType);
               return result;
           }
           catch (Exception ex)
           {
               Logger.Error(ex, "Failed {Command}", commandType);
               return Error.New("HANDLER_ERROR", ex.Message);
           }
       }
       
       protected abstract Task<Fin<Unit>> HandleCore(TCommand command, CancellationToken ct);
   }
   ```

3. **Update Slice Handlers**
   ```csharp
   // Updated handler inheriting common functionality
   public class MoveBlockCommandHandler : CommandHandlerBase<MoveBlockCommand>
   {
       private readonly IGridStateService _gridState;
       
       public MoveBlockCommandHandler(
           IGridStateService gridState,
           ILogger<MoveBlockCommandHandler> logger) : base(logger)
       {
           _gridState = gridState;
       }
       
       protected override async Task<Fin<Unit>> HandleCore(
           MoveBlockCommand command, CancellationToken ct)
       {
           // Only feature-specific logic remains
           return await _gridState.MoveBlock(command.BlockId, command.NewPosition);
       }
   }
   ```

**Output**: Base classes eliminating boilerplate across slices

---

### 5. Validate Slice Boundaries

**Trigger**: After any extraction to ensure VSA integrity

**Steps**:

1. **Check Dependency Direction**
   ```
   Verify dependencies flow inward:
   
   ✅ Slices → Infrastructure (OK)
   ✅ Infrastructure → Domain (OK)  
   ❌ Slices → Slices (NOT ALLOWED)
   ❌ Infrastructure → Slices (NOT ALLOWED)
   ```

2. **Verify Slice Independence**
   ```
   Each slice should be:
   ✅ Independently testable
   ✅ Independently deployable (conceptually)
   ✅ Modifiable without affecting other slices
   ❌ Not coupled to other slice internals
   ```

3. **Check for Proper Abstractions**
   ```
   Shared code should be:
   ✅ Infrastructure/technical concerns
   ✅ Domain primitives/value objects
   ✅ Cross-cutting patterns
   ❌ Not business logic
   ❌ Not slice-specific workflows
   ```

**Output**: VSA integrity validation report

---

## Refactoring Decision Framework

### Extract vs Keep Duplicated
```
Extract when:
✅ Infrastructure/technical concern
✅ Domain primitive (value object)
✅ Cross-cutting pattern
✅ 3+ identical implementations
✅ No business logic involved

Keep duplicated when:
❌ Business logic specific to feature
❌ Likely to diverge in future
❌ Slice-specific behavior
❌ UI/presentation concerns
❌ Only 1-2 occurrences
```

### Abstraction Level Decision
```
Domain Layer:
- Value objects (GridPosition, BlockId)
- Domain entities
- Business errors
- Domain rules

Infrastructure Layer:
- Repository implementations
- External service integrations
- Technical utilities
- Framework abstractions

Application Layer:
- Base handler classes
- Cross-cutting behaviors
- Shared interfaces
```

---

## Common Extraction Patterns

### Pattern 1: Query Service Extraction
```csharp
// Before: Duplicated query logic
public class Handler1
{
    private Block? GetBlock(BlockId id) { /* logic */ }
}

// After: Extracted query service
public interface IBlockQueryService
{
    Task<Block?> GetById(BlockId id);
}
```

### Pattern 2: Validation Service Extraction
```csharp
// Before: Repeated validation
private bool IsValidMove(GridPosition from, GridPosition to) { /* logic */ }

// After: Extracted validation service
public interface IGridValidationService
{
    Task<bool> IsValidMove(GridPosition from, GridPosition to);
}
```

### Pattern 3: Base Class Extraction
```csharp
// Before: Repeated handler structure
public class Handler
{
    // Common logging, error handling, etc.
}

// After: Base class
public abstract class CommandHandlerBase<T>
{
    // Common functionality
    protected abstract Task<Fin<Unit>> HandleCore(T command);
}
```

---

## Quality Checklist

After each refactoring:
- [ ] Slice boundaries maintained?
- [ ] No slice-to-slice dependencies?
- [ ] Dependencies flow inward only?
- [ ] Each slice remains independently testable?
- [ ] Shared code is truly cross-cutting?
- [ ] Business logic remains in slices?
- [ ] All tests still pass?
- [ ] DI registrations updated?

---

## Response Templates

### When analyzing duplication:
"📊 VSA Duplication Analysis Complete

FINDINGS:
- Infrastructure duplication: 4 patterns (EXTRACT)
- Domain primitives: 2 objects (EXTRACT)
- Business logic: 3 patterns (KEEP DUPLICATED)

HIGH PRIORITY EXTRACTIONS:
1. BlockQueryService - 5 slices affected
2. GridPosition - 6 slices affected

EXTRACTION PLAN: [detailed steps]"

### When extraction complete:
"✅ VSA Refactoring Complete

EXTRACTED:
- Service: IBlockQueryService → Infrastructure/
- Value Object: GridPosition → Domain/
- Base Class: CommandHandlerBase → Infrastructure/

SLICES UPDATED: 4 handlers refactored
VSA INTEGRITY: ✅ All boundaries preserved
TESTS: ✅ All passing

CODE REDUCTION: 200+ lines eliminated"

### When validation fails:
"⚠️ VSA Boundary Violation Detected

ISSUE: Slice dependency detected
- From: Features/Block/Move/
- To: Features/Inventory/Add/

RESOLUTION: Extract shared concern to Infrastructure
IMPACT: Maintains slice independence"