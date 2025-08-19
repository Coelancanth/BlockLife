# BlockLife Glossary

*The authoritative vocabulary for code, documentation, and team discussion.*

**Rule**: If you're unsure what to call something, check here first. All code must use these exact terms.

## Core Game Loop

**Turn**  
A complete game cycle from spawn appearance to player action resolution.
- **Starts**: When new block spawns on grid
- **Ends**: After all merges and chains complete
- **Code**: `TurnManager`, `TurnStartNotification`, `TurnEndNotification`

**Action**  
A player-initiated change to the game board.
- **Types**: Move, Place, Rotate (future)
- **Triggers**: Merge detection after completion
- **Code**: `IAction`, `MoveCommand`, `PlaceCommand`

**Spawn**  
The automatic appearance of a NEW block at turn start (VS_004).
- **When**: Beginning of each turn
- **Where**: Random empty position
- **Type**: Random from available types
- **Not**: Merge results (those are transforms)
- **Code**: `SpawnService`, `TurnSpawnCommand`

**Transform**  
The conversion of merged blocks into a higher tier block (VS_003).
- **When**: After successful merge
- **Where**: Trigger position (where player acted)
- **Result**: Same type, tier + 1
- **Not called**: Spawn, create, generate
- **Code**: `TransformToHigherTier()`, `MergeResult.TransformedBlock`

## Block Concepts

**Block**  
A single game piece with a type and tier.
- **Properties**: Type (Work/Study/etc), Tier (1-9)
- **Not called**: Piece, Token, Tile
- **Code**: `Block`, `BlockType`, `BlockTier`

**Tier**  
The level/rank of a block (1-9).
- **Tier 1**: Base level blocks
- **Increases**: Through merging
- **Not called**: Level, Rank, Grade
- **Code**: `BlockTier`, starts at 1

**Type**  
The category of a block (Work, Study, Exercise, etc).
- **Count**: 9 different types
- **Behavior**: Only same-type blocks merge
- **Code**: `BlockType` enum

## Merge Mechanics

**Merge**  
The combination of 3+ adjacent same-type blocks into a higher tier.
- **Trigger**: Completion of player action
- **Result**: Single block of Tier+1 at trigger position
- **Code**: `MergeCommand`, `IMergeDetector`, `MergeResult`

**Adjacent**  
Blocks that share an edge (not diagonal).
- **Directions**: Up, Down, Left, Right only
- **Not**: Diagonal connections
- **Code**: `GetAdjacentPositions()`, orthogonal only

**Result Position**  
The location where a transformed block appears after merge.
- **Purpose**: Determines placement of higher-tier block
- **Rule**: Last-acted position (for player moves) or last-placed position (for spawns)
- **Not called**: Trigger position (ambiguous with what triggered merge)
- **Code**: `MergeResult.ResultPosition`, `TransformPosition`

**Trigger** (verb only)  
What causes an event to occur.
- **Usage**: "Move triggers merge", "Spawn triggers merge"
- **Not**: A position or location
- **Code**: Used in method names like `TriggerMergeDetection()`

**Chain**  
A sequence of merges triggered by a single action.
- **Chain Depth**: Number of sequential merges (starts at 1)
- **Multiplier**: Score bonus that doubles each chain (2^depth)
- **Code**: `ChainDepth`, `ChainMultiplier`

## Grid & Position

**Grid**  
The game board containing blocks.
- **Initial Size**: 10x10 (can change based on conditions)
- **Coordinates**: (0,0) top-left to (width-1, height-1) bottom-right
- **Dynamic**: Size may expand/contract during gameplay
- **Code**: `BlockGrid`, `IBlockGridService`, `GridSize`

**Position**  
A specific x,y coordinate on the grid.
- **Range**: x:[0 to grid.width-1], y:[0 to grid.height-1]
- **Not called**: Cell, Slot, Tile, Square
- **Code**: `BlockPosition`, `GridPosition`

**Empty Position**  
A grid position without a block.
- **Used for**: Spawning, moving blocks
- **Detection**: `GridService.GetEmptyPositions()`
- **Code**: Position where `GetBlockAt()` returns None

## Game States

**Game Over**  
The end state when the game cannot continue.
- **Possible Triggers**: Various conditions (design in flux)
  - No empty positions for spawn
  - Player-specific failure conditions
  - Other game-specific rules
- **Note**: Exact conditions to be refined during development
- **Code**: `GameOverDetector`, `GameState.GameOver`

**Valid Move**  
An action that can be legally performed.
- **Move**: Target position is within range and different
- **Place**: Target position is empty
- **Code**: `CanExecute()` in command validators

## Scoring

**Score**  
Points earned through gameplay actions.
- **Sources**: Merges, chains, special actions
- **Formula**: `BlocksRemoved × Tier × 10 × ChainMultiplier`
- **Code**: `ScoreService`, `ScoreCalculator`

---

## Usage Examples

✅ **Correct**: "When a turn starts, spawn one block"
❌ **Wrong**: "At the beginning of a round, place a tile"

✅ **Correct**: "Merge three adjacent same-type blocks"
❌ **Wrong**: "Combine three touching similar pieces"

✅ **Correct**: "The block appears at the trigger position"
❌ **Wrong**: "The token spawns where clicked"

---

## Living Document Protocol

**This glossary evolves with the game design!**

### When to Update Terms
- **Design changes**: Update immediately when design evolves
- **New discoveries**: Add terms as features are implemented
- **Clarifications**: Refine definitions when ambiguity is found
- **Deprecation**: Mark old terms as deprecated, don't delete immediately

### How to Update
1. **During development**: Update terms as you discover design details
2. **Mark uncertainty**: Use "Note: design in flux" for evolving concepts
3. **Version big changes**: Comment with date when major definitions change
4. **Keep history**: Don't delete, mark as deprecated with replacement

### Example Evolution
```
// Version 1 (Initial):
Grid: Fixed 10x10 board

// Version 2 (After design clarification):
Grid: Dynamic board, starts 10x10, can expand

// Version 3 (After implementation):
Grid: Dynamic board with min 5x5, max 20x20
```

Remember: **The glossary serves the game, not vice versa.** Update it freely as understanding deepens.