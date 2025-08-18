# BlockLife Game Design Vision

*A tile-based life simulation game that combines merge mechanics with resource management and life progression through different life stages.*

---

## 🎯 Core Vision

BlockLife simulates the journey of human life through strategic tile management. Players navigate different life stages, each with unique mechanics, challenges, and emotional themes. The game emphasizes meaningful choices, resource balance, and the bittersweet nature of time's passage.

---

## 🎮 Planned Core Systems

### Block Tag System
- **Multiple Tags**: Blocks can have multiple tags affecting interactions
- **Tag-Based Effects**: Blocks affect others within range based on tags
- **Dynamic Tag Changes**: Tags evolve based on life events and player choices

### Advanced Effect System
Effects triggered by specific conditions:
- **Trigger Conditions**: Combos, patterns, life events
- **Effect Types**:
  - **Teleport**: Move blocks instantly
  - **Transform**: Change block types
  - **Freeze**: Lock blocks temporarily
  - **Split**: Divide high-level blocks
  - **Area Effects**: Affect multiple blocks in range

### Random Event System
- **Crisis Events**: Unexpected challenges requiring adaptation
- **Opportunity Events**: Rare chances for major gains
- **Life Events**: Marriage, childbirth, job changes
- **Fortune Cycles**: Periodic luck changes affecting gameplay

### Skills System
Unlockable abilities through progression:
- **Spawn**: Create specific blocks on empty tiles
- **Remove**: Clear unwanted blocks
- **Save**: Checkpoint system
- **Enhance**: Increase block movement range
- **Upgrade**: Level up blocks directly

### Skill-Enhanced Interactions
- **Instant Merge**: Special abilities creating immediate merge opportunities
- **Pattern Recognition**: Advanced matching beyond adjacency
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
- **Effect**: S-types get steady bonuses, N-types get combo rewards

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

### Life Sharing System
- End-game life summary generation
- Shareable life stories
- Achievement tracking
- Legacy influence on next playthrough

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