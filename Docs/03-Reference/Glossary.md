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

**Block Type Rewards** (What each type generates when matched)
Mapping of block types to their rewards:
- **Work** → Money (resource)
- **Study** → Knowledge (attribute)
- **Health** → Health (attribute)
- **Relationship** → Social Capital (resource)
- **Creativity** → Creativity (attribute)
- **Fun** → Happiness (attribute)
- **Exercise** → Fitness (attribute)
- **Special Blocks** (future):
  - **Career** → Money + Knowledge
  - **Partnership** → Social Capital + Happiness
  - **Passion** → Creativity + Happiness

**Tier**  
The tier of a block (1-9).
- **Tier 1**: Base tier blocks
- **Increases**: Through merging
- **Not called**: Level, Rank, Grade
- **Code**: `BlockTier`, starts at 1

**Type**  
The category of a block (Work, Study, Exercise, etc).
- **Count**: 9 different types
- **Behavior**: Only same-type blocks merge
- **Code**: `BlockType` enum

## Match & Transform Mechanics

**Match**  
The clearing of 3+ adjacent same-type blocks to earn resources or attributes (default behavior).
- **Default Behavior**: Applies to any tier that doesn't have merge-to-next-tier unlocked
- **Trigger**: Completion of player action when 3+ adjacent
- **Result**: Blocks disappear, rewards granted based on match size and block type
- **Reward Types**:
  - **Resources** (external): Work → Money, Relationship → Social Capital
  - **Attributes** (internal): Study → Knowledge, Health → Health, Creativity → Creativity
- **Match Size Bonuses**:
  - **Match-3**: Base rewards (×1.0)
  - **Match-4**: Good bonus (×1.5)
  - **Match-5**: Great bonus (×2.0)
  - **Match-6+**: Excellent bonus (×3.0)
- **Formula**: `BaseValue × BlockCount × TierMultiplier × MatchSizeBonus`
- **Replaced By**: Merge behavior when merge-to-next-tier is unlocked for that specific tier
- **Code**: `MatchCommand`, `IMatchDetector`, `MatchResult`, `MatchSizeBonus`

**Merge**  
The combination of 3+ adjacent same-type blocks into ONE block of higher tier (replaces match when unlocked).
- **Unlock System**: Each merge tier must be unlocked separately (merge-to-T2, merge-to-T3, etc.)
- **Behavior Change**: When merge-to-tier-(N+1) is unlocked, tier-N blocks merge instead of matching
- **Example**: 
  - Merge-to-T2 unlocked: 3× Work-T1 → 1× Work-T2 (no longer clears)
  - Merge-to-T3 NOT unlocked: 3× Work-T2 → clears and gives rewards (still matches)
  - Merge-to-T3 unlocked: 3× Work-T2 → 1× Work-T3 (no longer clears)
- **Progressive Unlocking**: Must unlock each merge tier sequentially (can't skip tiers)
- **Purpose**: Tier progression and grid compression
- **Code**: `MergeCommand`, `IMergeDetector`, `MergeResult`, `MergeUnlockService`

**Transmute** (Cross-Type Transform)
The combination of specific blocks into a DIFFERENT type block (advanced unlock).
- **Availability**: Expensive attribute unlock (late game)
- **Input**: 2× Work + 1× Study blocks
- **Output**: 1× Career block (different type entirely!)
- **Purpose**: Create special blocks through strategic combinations
- **Code**: `TransmuteCommand`, `ITransmuteDetector`, `TransmuteResult`

**Transform** (umbrella term)
General term covering both merge and transmute operations.
- **Merge Transform**: Same type, higher tier (Work-T1 → Work-T2)
- **Transmute Transform**: Different type (Work + Study → Career)
- **Usage**: Use specific terms (merge/transmute) when precision matters

**Terminology Evolution Note**  
The term "merge" has evolved through our design process:
- **V1 (Deprecated)**: "Merge" conflated matching, tier-progression, and transmutation
- **V2 (Brief)**: Replaced with "tier-up" to distinguish from matching
- **V3 (Current)**: "Merge" reinstated for tier-progression since it REPLACES match behavior
- **Clear distinction**: Match = clear for resources, Merge = combine to higher tier
- **Why the change**: With replacement mechanics, merge and match are mutually exclusive

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

**Match Patterns** (Named Formations)
Specific arrangements that trigger matches with bonuses:
- **Line-3**: Basic 3 in a row/column (×1.0)
- **Line-4**: 4 in a row/column (×1.5) 
- **Line-5**: 5 in a row/column (×2.0)
- **L-Shape**: 3×3 corner formation (×2.0)
- **T-Shape**: 3 vertical + 3 horizontal (×2.5)
- **Plus**: 5 blocks in + pattern (×2.5)
- **Cluster**: 6+ connected blocks (×3.0)
- **Code**: `MatchPattern`, `PatternDetector`, `PatternBonus`

**Chain**  
A sequence of matches/tier-ups triggered by a single action.
- **Chain Depth**: Number of sequential operations (starts at 1)
- **Chain Bonus**: Doubles each step (×2, ×4, ×8, ×16...)
- **Stacks with Match Size**: `MatchSizeBonus × ChainBonus`
- **Example**: Match-5 (×2.0) triggering Chain-2 (×2) = ×4.0 total!
- **Chain Rewards** (in addition to bonuses):
  - First Chain-3 in game: +100 Knowledge "Chain Master"
  - Chain-5+: +200 Money "Impossible Chain"
  - Perfect Chain (clears grid): +1000 Money "Perfect Clear"
- **Code**: `ChainDepth`, `ChainBonus`, `ChainRewardTrigger`

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

## Bonuses & Rewards

**Base Value**
The fundamental resource/attribute amount before any modifiers.
- **Per Block Type**: Each type has specific base values
- **Example**: Work block = 10 Money base
- **Code**: `BlockTypeConfig.BaseValue`

**Bonus** (Multiplicative Modifier)
A percentage modifier that scales base values.
- **Types of Bonuses**:
  - **Match Size Bonus**: ×1.5 for Match-4, ×2.0 for Match-5
  - **Tier Bonus**: ×3 for Tier-2, ×10 for Tier-3
  - **Chain Bonus**: ×2^(chain-1) for chain reactions
  - **Pattern Bonus**: Special shapes (T-shape ×2.5)
- **Stacking**: Bonuses multiply together
- **Example**: Match-5 (×2.0) of Tier-2 (×3.0) = ×6.0 total
- **Code**: `BonusCalculator`, `IBonusModifier`

**Reward** (Discrete Grant)
Additional resources/attributes granted for achievements.
- **Types of Rewards**:
  - **First-Time Rewards**: First Match-5 ever = +50 Knowledge
  - **Milestone Rewards**: 10th chain reaction = +100 Money
  - **Perfect Clear Reward**: Clear entire grid = +500 Money
  - **Challenge Rewards**: Complete specific patterns
- **Not multiplicative**: Added after bonus calculations
- **One-time or repeatable**: Depends on reward type
- **Code**: `RewardService`, `IReward`, `RewardTrigger`

**Total Calculation Formula**
```
Total = (BaseValue × BlockCount × AllBonuses) + Rewards
      = (10 × 5 × 6.0) + 50
      = 300 + 50 = 350
```

## Resources & Progression

**Resources**  
External assets that can be gained, spent, or lost.
- **Types**: Money, Social Capital, Opportunities
- **Characteristics**: 
  - Can be spent on unlocks
  - Can be lost through events
  - Represent external/material wealth
- **Examples**: 
  - Money: Financial assets
  - Social Capital: Network influence
  - Opportunities: Career/life chances
- **Code**: `ResourceService`, `IResource`, `ResourceType`

**Attributes**  
Internal qualities that define the player character.
- **Types**: Knowledge, Health, Creativity, Wisdom, Fitness
- **Characteristics**:
  - Generally don't decrease (you don't "unlearn")
  - Represent personal growth
  - Can unlock abilities through thresholds
- **Examples**:
  - Knowledge: Education and experience
  - Health: Physical wellbeing
  - Creativity: Artistic capability
- **Persistence**: Both resources and attributes carry across turns
- **Code**: `AttributeService`, `IAttribute`, `AttributeType`

**Score** (secondary to attributes)  
Points earned through gameplay actions (for leaderboards/achievements).
- **Sources**: Matches, chains, special actions
- **Formula**: `BlocksMatched × Tier × 10 × ChainMultiplier`
- **Note**: Attributes are the primary progression currency, not score
- **Code**: `ScoreService`, `ScoreCalculator`

**Unlock**  
A permanent ability purchased with attributes.
- **Types**: Transform abilities, special moves, new block types
- **Cost**: Varies by unlock type and power
- **Persistence**: Once unlocked, available for rest of game
- **Examples**: "Work Transform" (100 Money), "Career Combo" (500 Money + 300 Knowledge)
- **Code**: `UnlockService`, `IUnlock`, `UnlockType`

---

## Visual Quick Reference

### The Three Core Operations

**MATCH** (Default Behavior - Replaced by Merge When Next Tier Unlocked)
```
Before merge-to-T2 unlock:
[Work-T1] [Work-T1] [Work-T1] → *poof* → +30 Money (matches/clears)

After merge-to-T2 unlock:
[Work-T1] [Work-T1] [Work-T1] → [Work-T2] (merges, doesn't clear)

But T2 still matches (until merge-to-T3 unlocked):
[Work-T2] [Work-T2] [Work-T2] → *poof* → +90 Money (matches/clears)

Match Size Still Matters (for tiers that match):
Match-3: Base rewards (×1.0)
Match-4: Good bonus (×1.5)
Match-5: Great bonus (×2.0)
```

**MERGE** (Progressive Unlock System - Replaces Match for Each Tier)
```
Merge-to-T2 Unlock: [Work-T1] [Work-T1] [Work-T1] → [Work-T2]
Merge-to-T3 Unlock: [Work-T2] [Work-T2] [Work-T2] → [Work-T3]
Merge-to-T4 Unlock: [Work-T3] [Work-T3] [Work-T3] → [Work-T4]

Each merge tier must be unlocked separately!
```

**TRANSMUTE** (Advanced Unlock - Specific Recipes)
```
[Work-T1] [Work-T1] [Study-T1] → [Career-T1]
Different input types → New type output (fixed recipes)
```

### Complete Calculation Example

**Scenario**: Player matches 5 Work-T2 blocks, triggering a chain-2, and it's their first Match-5

```
Step 1: Base Calculation
- Base Value: 10 Money per Work block
- Block Count: 5 blocks
- Base Total: 10 × 5 = 50 Money

Step 2: Apply Bonuses (multiplicative)
- Tier Bonus: ×3 (Tier-2 blocks)
- Match Size Bonus: ×2.0 (Match-5)
- Chain Bonus: ×2 (Chain-2)
- Total Bonuses: 3 × 2.0 × 2 = ×12
- After Bonuses: 50 × 12 = 600 Money

Step 3: Add Rewards (additive)
- First Match-5 Reward: +50 Knowledge
- Chain-2 Milestone: (none for Chain-2)

Final Result:
- Money Gained: 600
- Knowledge Gained: 50
- Achievement Unlocked: "First Match-5!"
```

### Bonus vs Reward Quick Reference
| Type | Effect | Example | Stacks? |
|------|--------|---------|---------|
| **Bonus** | Multiplies base | Match-5 = ×2.0 | Yes (multiply) |
| **Reward** | Adds fixed amount | First Chain = +100 | Yes (addition) |

---

## Usage Examples

✅ **Correct**: "Match three Work-T1 blocks to earn 30 Money (before merge unlock)"
❌ **Wrong**: "Merge three Work blocks for points" (when you mean match/clear)

✅ **Correct**: "After unlocking merge-to-T2, Work-T1 blocks merge instead of matching"
❌ **Wrong**: "Choose between match or merge for Work blocks"

✅ **Correct**: "Work-T2 blocks still match until merge-to-T3 is unlocked"
❌ **Wrong**: "All Work blocks merge once any upgrade is unlocked"

✅ **Correct**: "Unlock merge-to-T2 for Work blocks with 100 Money"
❌ **Wrong**: "Buy the universal merge ability"

✅ **Correct**: "Each merge tier must be unlocked separately"
❌ **Wrong**: "One unlock enables all merges"

✅ **Correct**: "Transmute Work + Study blocks into Career block"
❌ **Wrong**: "Merge different block types together" (merge is same-type only)

✅ **Correct**: "When a turn starts, spawn one block"
❌ **Wrong**: "At the beginning of a round, place a tile"

✅ **Correct**: "The tier-up result appears at the result position"
❌ **Wrong**: "The merged token spawns where clicked"

---

## Design Philosophy: Resources vs Attributes

**Why This Distinction Matters**

The separation between **resources** (external) and **attributes** (internal) reflects real life:
- **Resources** can be gained and lost (money, connections, opportunities)
- **Attributes** represent personal growth (knowledge, skills, health)

**Gameplay Implications**:
1. **Resources** are volatile - events can drain money, relationships can break
2. **Attributes** are permanent growth - you don't "unlearn" knowledge
3. **Strategic depth** - Balance immediate resource needs vs long-term attribute building
4. **Life simulation** - Mirrors real decisions (work for money vs study for knowledge)

**Future Mechanics** (enabled by this distinction):
- Economic crashes affect resources, not attributes
- Aging might reduce physical attributes but not mental ones
- Career changes leverage attributes to generate new resources
- Retirement planning: Convert resources to sustainable income

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