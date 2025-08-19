# BlockLife Game Design Vision

*A block-based life simulation game that combines merge mechanics with resource management and life progression through different life stages.*

---

## 🎯 Core Vision

BlockLife simulates the journey of human life through strategic block management. Players navigate different life stages, each with unique mechanics, challenges, and emotional themes. The game emphasizes meaningful choices, resource balance, and the bittersweet nature of time's passage.

---

## 🎲 Core Gameplay Mechanics (Designer Confirmed)

### Merge System - Triple Town Model
**Fundamental Rule**: 3 adjacent same-type blocks merge into higher tier
- **Merge Trigger**: When 3+ same-type blocks are adjacent (orthogonal, not diagonal)
- **Result Placement**: Merged block appears at the last-moved/placed block position
- **Tier Progression**: Tier 1 + Tier 1 + Tier 1 → Tier 2 (and so on)
- **Same-Type Only** (Phase 1): Work blocks only merge with Work, Study with Study, etc.

### Auto-Spawn System
**Turn-Based Pressure**: Each turn spawns new blocks automatically
- **Spawn Timing**: After each player action (move/merge)
- **Spawn Rules**: Random type, empty position
- **Difficulty Scaling**: Spawn rate/quantity increases over time
- **Strategic Element**: Players must manage space while pursuing merges

### Chain Reaction System (Core Mechanic)
**The Magic Moment**: Merges that trigger additional merges automatically
- **Chain Detection**: After any merge completes, check if result triggers new merges
- **Chain Multiplier**: Each chain step increases score (1x → 2x → 4x → 8x)
- **Visual Celebration**: Each chain step gets bigger effects/sounds
- **Strategic Depth**: Players set up elaborate chain reactions for massive scores

**Example Chain**:
1. Merge three Tier-1 Work blocks → Tier-2 Work appears
2. Tier-2 Work completes a group with two other Tier-2s → Auto-merges to Tier-3
3. Tier-3 triggers another adjacent group → Chain continues
4. Score: 10 (base) × 1 × 2 × 4 = 80 points instead of just 10!

### Placement Strategy
- **Grid Management**: Balance between keeping space for moves and setting up merges
- **Chain Planning**: Deliberately position blocks to trigger chain reactions
- **Risk/Reward**: Complex setups risk grid filling but offer huge rewards
- **Skill Expression**: Difference between beginners (random merges) and experts (5+ chains)

### Future Complexity Layers (Post-MVP)
- **Cross-Type Merging**: 2 Work + 1 Study → Career Opportunity
- **Merge Effects**: Special blocks spawn additional blocks
- **Life Stage Modifiers**: Different merge rules per life stage
- **Chain Bonus Blocks**: Special blocks created only through long chains

---

## 🎮 Advanced Systems (Post-MVP)

### Block Tag System
- **Multiple Tags**: Blocks can have multiple tags affecting interactions
- **Tag-Based Effects**: Blocks affect others within range based on tags
- **Dynamic Tag Changes**: Tags evolve based on life events and player choices

### Advanced Effect System
Effects triggered by specific conditions:
- **Trigger Conditions**: Chains, patterns, life events
- **Effect Types**:
  - **Teleport**: Move blocks instantly via "Opportunity" blocks
  - **Transform**: Change block types
  - **Freeze**: Lock blocks temporarily
  - **Split**: Divide high-tier blocks for strategic positioning
  - **Area Effects**: Affect multiple blocks in range
  - **Inertia**: High-tier blocks gain automatic merging behavior

### Random Event System
- **Crisis Events**: Unexpected challenges requiring adaptation
- **Opportunity Events**: Rare chances for major gains
- **Life Events**: Marriage, childbirth, job changes
- **Fortune Cycles**: Periodic luck changes affecting gameplay

### Skills System
Unlockable abilities through progression:
- **Spawn**: Create specific blocks on empty positions
- **Remove**: Clear unwanted blocks
- **Save**: Checkpoint system
- **Enhance**: Increase block movement range
- **Upgrade**: Tier up blocks directly

### Skill-Enhanced Interactions
- **Instant Merge**: Special abilities creating immediate merge opportunities
- **Pattern Recognition**: Advanced merging beyond adjacency
- **Custom Patterns**: Player-defined merge rules

---

## 🌟 Life Stage Progression

### Infant Stage (0-3 years)
- **Theme**: Passive observation, dependency, learning
- **Grid**: Small 4x4 or 5x5
- **Mechanics**:
  - No energy cost or game over
  - "Cry" action creates "Need" blocks
  - "Parents' Love" auto-generates satisfaction
  - Learn basic merge concepts passively

### Childhood (4-12 years)
- **Theme**: Active exploration, curiosity, rules
- **Grid**: Expands to 7x7 or 8x8
- **Mechanics**:
  - Small energy cost introduced
  - "Curiosity" as primary resource
  - First "Dream Seed" block appears
  - "Rules" obstacles to navigate

### Adolescence (12-18 years)
- **Theme**: Self-discovery, social complexity, choices
- **Grid**: Full 10x10
- **Mechanics**:
  - "Mindset" resource becomes critical
  - Academic specialization (Math, Arts, etc.)
  - Social dynamics (Friendship, Crush, Peer Pressure)
  - "Masks" for fitting in vs. authenticity
  - College Entrance Exam mini-game

### Young Adulthood (18-25 years)
- **Theme**: Freedom, exploration, financial pressure
- **Mechanics**:
  - Career foundation building
  - Relationship investments
  - Student loans and first apartments
  - Risk-taking opportunities

### Prime Adulthood (25-60 years)
- **Theme**: Balance, responsibility, achievement
- **Mechanics**:
  - Family management systems
  - Career advancement paths
  - Mortgage and investment blocks
  - Child-rearing challenges

### Late Adulthood (60+ years)
- **Theme**: Legacy, wisdom, reflection
- **Mechanics**:
  - Energy limits increase dramatically
  - "Legacy Project" multi-stage blocks
  - Memory and wisdom blocks
  - Mentoring next generation

---

## 🧠 Personality System (MBTI-Inspired)

### Core Philosophy
Personality develops through actions, not initial selection. Player behavior shapes traits, which unlock unique abilities.

### Four Personality Axes

#### Energy Source (I/E)
- **Introversion**: Gain from solitude, hobbies, reading
- **Extroversion**: Gain from social interaction, parties
- **Effect**: Determines resource regeneration methods

#### Information Processing (S/N)
- **Sensing**: Focus on concrete, immediate actions
- **Intuition**: Complex planning, abstract thinking
- **Effect**: S-types get steady bonuses, N-types get chain rewards

#### Decision Making (T/F)
- **Thinking**: Maximize efficiency, prioritize logic
- **Feeling**: Prioritize relationships, emotional harmony
- **Effect**: T-types grow attributes faster, F-types have better resilience

#### Lifestyle (J/P)
- **Judging**: Organization, planning, structure
- **Perceiving**: Spontaneity, adaptation, flexibility
- **Effect**: J-types benefit from order, P-types from chaos

### Implementation
- Actions nudge personality axes
- Stage-end personality reports
- Trait-based ability unlocks
- Personality solidifies with age
- "Midlife Crisis" allows dramatic shifts

---

## 🎭 Character Origins & Talents

### Background System
Starting conditions that shape early life:
- **Scholarly Family**: Enhanced knowledge blocks, academic expectations
- **Wealthy Family**: Financial advantages, social pressure, larger grid
- **Humble Beginnings**: Resource constraints, resilience bonuses, motivation
- **Each background**: Provides unique starting blocks and opportunities

### Talent System
Rule-changing abilities unlocked through personality development:
- **Logical Genius**: Merge non-adjacent blocks in straight lines
- **Social Butterfly**: Social blocks require only 2 instead of 3 to merge
- **Artistic Intuition**: 10% chance for merges to tier up twice
- **Meticulous Planner**: Storage ability to save blocks for strategic timing
- **Born Optimist**: Convert negative blocks to positive ones
- **Boundless Energy**: First merge each turn costs no energy

---

## 🎲 Luck System

### Fortune Cycles
- **Visual Indicator**: Fortune Wheel showing current state
- **Cycle States**:
  - Great Fortune: Blessed merges, positive spawns
  - Good Fortune: Minor bonuses
  - Neutral: Standard probabilities
  - Misfortune: Negative spawns, action fumbles
- **Duration**: 20-30 turns per cycle
- **Player Influence**: Charm blocks can affect duration

### Luck Manifestations
- **Lucky Star Blocks**: Temporary positive aura
- **Ominous Cloud Blocks**: Cursed areas
- **Critical Success/Failure**: 1% chance on any action
- **Ritual Actions**: Hidden sequences for luck boosts
- **Lucky Charm Blocks**: Permanent passive luck bonuses

---

## 💭 Special Block Narratives

### Relationship Blocks
- **Rival Block**: Unmovable competition creating pressure
- **Bond Links**: Visual connections sharing benefits/harm
- **Pet Block**: Absorbs small negative emotions

### Health & Psychology Blocks
- **Burnout Block**: From overwork, locks tiles
- **Inner Demon Block**: Duplicates negatives, requires sustained positivity
- **Comfort Zone Block**: From stagnation, reduces variety

### Growth & Legacy Blocks
- **Forgotten Dream Block**: Neglected potential, can be reactivated
- **Eureka Moment**: Breakthrough from complex merges
- **Legacy Project**: Multi-stage elder year investment

---

## 🔄 Advanced Gameplay Systems

### Latent Trait System
- Hidden affinities generated at life start
- Influenced by parents or player choice
- Affects available opportunities
- Discovered through gameplay
- Shapes next generation's potential

### Dynamic Time Scale
- **Happy moments**: Time passes faster (1 action = 2 turns)
- **Difficult periods**: Time slows (1 action = 0.5 turns)
- Emotional weight affects temporal perception

### Mindset Resource (Fourth Core Resource)
- High mindset: Better rewards, fewer negatives
- Low mindset: Darkened vision, increased mistakes
- Sources: Happiness, hobbies, family
- Drains: Bills, work pressure, stress

### Generational Legacy System
- **Inheritance Mechanics**: Previous life achievements become next generation advantages
- **Legacy Burdens**: Past regrets create new challenges and expectations
- **Family Traits**: Successful traits pass to descendants with variations
- **Bloodline Effects**: Multi-generational story arcs spanning families
- **New Game+**: Enhanced replay with meaningful continuity

### Inner Monologue System
- **Real-time Commentary**: Personality-driven thoughts reflect current state
- **MBTI-based Voice**: Different internal dialogue for each personality type
- **Contextual Responses**: Thoughts change based on actions and situations
- **Emotional Resonance**: Players experience character's mental state directly

### Narrative Anchor Choices
- **Value-based Decisions**: Pause gameplay for moral/ethical choices
- **Character Defining**: Choices that shape personality and unlock abilities
- **Long-term Consequences**: Decisions create lasting effects on gameplay
- **Principle Testing**: Moments that challenge player's established values

### Regional Gameplay
- **Life Areas**: Home, work, school as separate board regions
- **Area-specific Rules**: Different merging costs and effects per region
- **Spillover Effects**: Stress/happiness bleeds between regions
- **Commute Costs**: Moving blocks between areas requires time/energy
- **Environmental Buffs**: Each region provides unique advantages/challenges

### Memory Palace
- **Secondary Board**: Separate space for storing precious memories
- **Memory Crystallization**: Peak experiences preserved as permanent blocks
- **Recall Resource**: Revisit memories for mindset restoration in hard times
- **Fading Mechanism**: Old memories require investment to maintain vividness

### Time Budget System
- **Limited Time Tokens**: Explicit allocation of time resources in adulthood
- **Opportunity Cost**: Clear trade-offs between competing life demands
- **Priority Management**: Strategic decisions about time investment
- **Work-Life Integration**: Balancing professional and personal time allocation

### Life Sharing System
- End-game life summary generation
- Shareable life stories
- Achievement tracking
- Cross-generational influence tracking

---

## 🎮 Game Modes

### Authentic Mode
- **Random Genetics**: Hidden traits and backgrounds assigned randomly
- **Pure Simulation**: Emphasizes life's unpredictability and adaptation
- **Discovery-based**: Players learn their character through gameplay
- **Realism Focus**: Models true-to-life uncertainty and chance

### Destiny Mode
- **Character Creation**: Players influence starting background through choices
- **Guided Experience**: Personality tests determine initial traits
- **Accessible Entry**: Reduced randomness for strategic planning
- **Customization**: Balance between choice and surprise

### Legacy Mode
- **Generational Focus**: Mandatory inheritance from previous lives
- **Family Saga**: Multi-generation storytelling emphasis
- **Continuity Themes**: Explores how past shapes present
- **Dynasty Building**: Long-term family development across playthroughs

---

## 🏗️ Technical Considerations

### Architecture Alignment
- **Feature Modules**: Each life stage as separate feature
- **Command Pattern**: All player actions as commands
- **Event-Driven**: Life events trigger through event system
- **Service Layer**: Personality, Luck, and Trait services

### State Persistence
- Save/load system for long play sessions
- Cloud saves for life sharing
- Achievement integration
- Multiple difficulty modes

### Animation & Feedback
- Smooth block movements
- Life stage transitions
- Emotional visual effects
- Audio tied to life events

---

## 🎯 Design Principles

1. **Every mechanic tells a story** - Game systems reflect real life experiences
2. **Meaningful choices** - No optimal path, only different life journeys
3. **Emotional resonance** - Players should feel the weight of time
4. **Emergent narratives** - Stories arise from player actions
5. **Accessible depth** - Easy to play, difficult to master

---

*This document represents the future vision of BlockLife. Features marked as planned are aspirational goals guiding development direction.*