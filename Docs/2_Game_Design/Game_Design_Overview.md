## **Game Overview**
This is a tile-based life simulation game that combines merge mechanics with resource management and life progression through different life stages.

---

## **Core Game Systems**

### **Block/Tile System** ✅ *IMPLEMENTED*
- **Grid**: ✅ *IMPLEMENTED*
	- 10x10 game board with configurable grid system
	- Grid positioning system with drag & drop mechanics
	- Block swap and movement validation with range checking
	- World/grid coordinate conversion system
- **Block Types** ✅ *IMPLEMENTED*
	- Basic, Premium, Rare, Empty block types with different values and movement ranges
	- Empty blocks that serve as placeholders
	- Life stage-based auto spawn system with different block distributions ✅ *IMPLEMENTED*
	- They can be
		- Un/mergeable ✅ *IMPLEMENTED*
		- Un/moveable ✅ *IMPLEMENTED*
		- Auto-generate resources ✅ *IMPLEMENTED*
		- Affect other blocks in the range *(PLANNED)*
- **Block Properties**: ✅ *IMPLEMENTED*
	- type: A block can only have one type ✅ *IMPLEMENTED*
    - tag: but can have multiple tags *(PLANNED)*
	- level: could be upgraded ✅ *IMPLEMENTED*
	- value: player will get corresponding resources after merging these blocks ✅ *IMPLEMENTED*
	- range: how far blocks can be moved ✅ *IMPLEMENTED*

### **Turn System** ✅ *IMPLEMENTED*
- **Turn-Based Mechanics**: Each successful block movement advances one turn ✅ *IMPLEMENTED*
- **Turn Counter**: Tracks current game turn for progression systems ✅ *IMPLEMENTED*
- **Turn Events**: Systems that trigger at the start/end of each turn ✅ *IMPLEMENTED*
- **Turn Validation**: Ensures only valid actions can trigger turn advancement ✅ *IMPLEMENTED*
- **Turn History**: Tracks recent turn events for debugging and gameplay features ✅ *IMPLEMENTED*

### **Event System** ✅ *PARTIALLY IMPLEMENTED*
- **Event Chain**: Service-to-service communication via events ✅ *IMPLEMENTED*
- **Random Event** (Like Crisis events) *(PLANNED)*

### **Debug Manager System** ✅ *IMPLEMENTED*
- **Categorized Logging**: Debug messages organized by category (Blocks, Grid, Turn, Resources, Detection, etc.) ✅ *IMPLEMENTED*
- **Log Level Control**: Trace, Info, Warning, Error, Critical levels ✅ *IMPLEMENTED*
- **Runtime Configuration**: Enable/disable categories and levels at runtime ✅ *IMPLEMENTED*
- **Rich Text Support**: Colored console output with formatting ✅ *IMPLEMENTED*
- **UI Integration**: Debug panel for in-game debugging ✅ *IMPLEMENTED*

### **Rule System** ✅ *IMPLEMENTED*
- **Detection Rules**: ✅ *IMPLEMENTED*
	- AdjacencyMerge rule for same type and level blocks ✅ *IMPLEMENTED*
	- Rule priority system ✅ *IMPLEMENTED*
	- Dynamic rule activation/deactivation ✅ *IMPLEMENTED*
	- Rule evaluation service with configurable parameters ✅ *IMPLEMENTED*
- **Auto Spawn Rules**: ✅ *IMPLEMENTED*
	- Life stage-based spawn modifiers ✅ *IMPLEMENTED*
	- Type and level distribution weights ✅ *IMPLEMENTED*
	- Turn-based spawn frequency control ✅ *IMPLEMENTED*
	- Configurable min/max spawns per turn ✅ *IMPLEMENTED*
- **Matching Rules**: ✅ *PARTIALLY IMPLEMENTED*
	- Pattern detection system ✅ *IMPLEMENTED*
	- Custom pattern matching *(PLANNED)*
	- During **Childhood** Stage, will be affected by **Potential Pool** *(PLANNED)*

### **Latent Trait System** *(PLANNED)*
- We can call it the "Latent Trait" system. At the start of a new life, the game generates a hidden set of affinities for the character, influenced by their parents or a choice the player makes. For a first playthrough, it could be a random selection. For subsequent "generations," the achievements of the previous life directly shape the pool for their child. Player is aware of the existence of latent trait, but don't know what it is unless it's already have been in the codex.

### **Detection System** ✅ *IMPLEMENTED*
Before any merge can occur, the system must first detect potential merge groups. Detection is triggered whenever a cell's content changes through any of the following methods:

1. **Skill Usage**: When a skill modifies cell content *(PLANNED)*
2. **User Interaction**: When player drags and swaps blocks ✅ *IMPLEMENTED*
3. **Auto Spawn**: When blocks are automatically spawned ✅ *IMPLEMENTED*
4. **Chain Effects**: When blocks are spawned or modified by cascading effects *(PLANNED)*

**Detection Process**: ✅ *IMPLEMENTED*
- **Signal Emission**: Grid changes trigger detection scans ✅ *IMPLEMENTED*
- **Group Detection**: System scans for groups that meet current matching rules ✅ *IMPLEMENTED*
- **Result Storage**: Detection results stored with metadata and timestamps ✅ *IMPLEMENTED*
- **Rule Evaluation**: Configurable detection rules (AdjacencyMerge implemented) ✅ *IMPLEMENTED*
- **Event System**: Detection events fired for UI updates and game logic ✅ *IMPLEMENTED*

### **Merge System** ✅ *IMPLEMENTED*
- **Detection-First Approach**: All merges must be detected before execution ✅ *IMPLEMENTED*
- **Adjacency Rule**: Blocks of same type and level detected when 2+ are adjacent ✅ *IMPLEMENTED*

**Merge Service** ✅ *IMPLEMENTED*:
- **Strategic Planning**: Determine optimal merge sequences from detected opportunities ✅ *IMPLEMENTED*
- **Resource Calculation**: Calculate rewards and costs for merge operations ✅ *IMPLEMENTED*
- **Merge Validation**: Ensure merge operations are valid and beneficial ✅ *IMPLEMENTED*
- **Chain Planning**: Plan cascading merge sequences for maximum efficiency ✅ *IMPLEMENTED*

**Execution Service** ✅ *IMPLEMENTED*:
- **Grid Manipulation**: Remove source blocks and place result blocks ✅ *IMPLEMENTED*
- **Transaction System**: Atomic merge operations with rollback capability ✅ *IMPLEMENTED*
- **Animation System**: Smooth visual transitions for merge operations ✅ *IMPLEMENTED*
- **Effect Triggering**: Handle merge-related particle effects and feedback ✅ *IMPLEMENTED*
- **State Consistency**: Maintain grid integrity during merge operations ✅ *IMPLEMENTED*
- **Cascade Detection**: Automatic detection of chain reactions ✅ *IMPLEMENTED*

**Advanced Features** ✅ *PARTIALLY IMPLEMENTED*:
- **Smart Merging**: Some techs allow merging (by modifying detection rules) ✅ *IMPLEMENTED*
- **Skill-Enhanced Merging**: Special abilities can create instant merge opportunities *(PLANNED)*

### **Resource Management** ✅ *IMPLEMENTED*
Three core resources with turn-based consumption:

- **Vitality**: ✅ *IMPLEMENTED*
  - Consumed each turn (Cost can be changed)
  - Reaches 0 = Game Over
  - Regenerated by certain blocks/actions

- **Money**: ✅ *IMPLEMENTED*
  - Used for skills or special abilities
  - Generated by certain blocks (like working)

- **Personal Attribute**: ✅ *IMPLEMENTED*
  - Used for condition check
  - Generated by certain blocks (by studying etc.)

- **Resource Events**: ✅ *IMPLEMENTED*
  - Resource change notifications
  - Game over event handling
  - Resource validation systems

### **Time Progression** 🎯 *FOUNDATION IMPLEMENTED*
- **Life Stage Progression**: 
	- Childhood (0-12)
	- Adolescence (12-18)
	- Young Adulthood (18-30)
	- Prime Adulthood (30-60)
	- Late Adulthood (60-)
- **Key Event** *(PLANNED)*
- **Ambition Selection**: Choose from 3 goals when requirement is met *(PLANNED)*
- **Turn-Based**: Each action advances time, i.e. when player successfully moves a block, it counts as 1 turn ✅ *IMPLEMENTED*
- **Dynamic Time Scale**: *(PLANNED)*
  - Early game: 
  - Late game: 
- **Age Transitions**: Automatic when turn thresholds (usually time) are met *(PLANNED)*
- **Era Effects**: Each new stage brings: *(PLANNED)*
  - Map expansion
  - New block types
  - Resource changes
  - Difficulty scaling


### **Skills System** *(PLANNED)*
Different skills unlocked through progress:

- **Spawn** certain block on empty tile(s)
- **Remove** certain block(s) 
- **Save** game
- **Increase** block(s) movement range
- **Upgrade** block(s)

### **Effect System** *(PLANNED)*
Effect can be triggered via certain condition (combos)

Every effect is defined by 

- Trigger conditions
- Source block
- Affected range (a helper method, shape + range + center position)
- Filter for certain type of block
- Execution result
	- **Teleport**
	- **Transform**
	- **Remove**
	- **Spawn**
	- **Freeze**
	- **Split**
---

## 🎮 **Core Gameplay Loop** 

**Current Implementation Status:**
1. **Place/Move Blocks**: Drag blocks within movement range ✅ *IMPLEMENTED*
2. **Turn Advancement**: Successfully moving a block advances one turn ✅ *IMPLEMENTED*
3. **Resource Management**: Turn-based resource consumption and tracking ✅ *IMPLEMENTED*
4. **Detection System**: Identify merge opportunities and block interactions ✅ *IMPLEMENTED*
5. **Auto Spawn**: Automatic block generation based on life stage and turn ✅ *IMPLEMENTED*
6. **Merge Planning**: Strategic merge sequence planning from detected opportunities ✅ *IMPLEMENTED*
7. **Merge Execution**: Physical execution of planned merge operations ✅ *IMPLEMENTED*
8. **Merge Strategy**: Create adjacent groups of 2+ matchable blocks for optimal merging ✅ *IMPLEMENTED*
9. **Debug System**: Comprehensive debug manager with categorized logging ✅ *IMPLEMENTED*

**Planned Implementation:**
10. **Use Skills**: Spend resources on powerful abilities *(PLANNED)*
11. **Deploy Heroes**: Place special block for strategic bonuses *(PLANNED)*
12. **Research Tech**: Accumulate attribute to meet some conditions *(PLANNED)*
13. **Survive Ages**: Manage vitality consumption while progressing *(PLANNED)*
14. **Save/Load System**: Game state persistence *(PLANNED)*

---

## 🎯 **Strategic Elements**

### **Movement System** ✅ *IMPLEMENTED*
- **Range Limitation**: Blocks can only move within their range value
- **Drag & Drop**: Intuitive tile swapping mechanics with validation
- **Shadow Indicators**: Visual feedback for valid move ranges *(PLANNED)*

### **Turn Economy** ✅ *IMPLEMENTED*
- **Action Cost**: Each block movement consumes exactly one turn
- **Resource Planning**: Plan movements to optimize resource generation/consumption
- **Time Management**: Balance immediate needs vs long-term progression

---

## 🔧 **Technical Implementation Notes**

### **Current Architecture** ✅ *IMPLEMENTED*
- **MVP Pattern**: Model-View-Presenter with dependency injection ✅ *IMPLEMENTED*
- **Service Layer**: Comprehensive service architecture with interface abstractions ✅ *IMPLEMENTED*
  - BlockService, GridService, TurnService, ResourceService ✅ *IMPLEMENTED*
  - DetectionService, MergeService, MergeExecutionService ✅ *IMPLEMENTED*
  - AutoSpawnService, RuleEvaluationService, DebugManager ✅ *IMPLEMENTED*
- **Bootstrap System**: GameStrapper autoload with service registration ✅ *IMPLEMENTED*
- **Event-Driven Architecture**: Service communication via events ✅ *IMPLEMENTED*
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection ✅ *IMPLEMENTED*

### **State Management** *(PLANNED)*
- Persistent save/load system
- Steam integration for achievements
- Multiple difficulty modes and language support

### **Animation System** *(PLANNED)*
- Smooth block movement with `SmoothDamp`
- Appear/disappear animations for new blocks
- Visual feedback for player actions

### **Audio Integration** *(PLANNED)*
- Age-specific background music
- Sound effects for different block types and actions
- Dynamic audio mixing

### **Life Sharing** *(PLANNED)*
- When game ended, will based on the whole game progress generate a life, it can be shared.
---
