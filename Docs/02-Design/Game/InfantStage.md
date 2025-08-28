# Infant Stage Design Document

*The opening experience of BlockLife - where players learn through nurturing*

---

## 🎯 Design Philosophy

The Infant Stage serves as both tutorial and emotional hook. Players learn core mechanics not through instruction, but through the universal experience of caring for a baby. Every mechanic teaches through metaphor, every interaction builds emotional connection.

**Core Principle**: Simple rules + proximity effects = emergent gameplay

---

## 🎮 Core Mechanics

### Grid Configuration
- **Size**: 5x5 fixed grid (smaller = more intimate)
- **Parent Block**: Fixed at center position (2,2)
- **Playable Spaces**: 24 tiles around parent
- **No Expansion**: Keeps focus tight and manageable

### Block Types

#### 1. Need Block 😢 (Red)
- **Created**: Baby "cry" action or stress spawning
- **Match-3**: Creates "Tantrum" - Parent immediately helps
- **Proximity**: Near Care block → Both glow → Transform to Happy
- **Alone**: Pulses sadly, increases stress
- **Tooltip**: "I need care! Place care blocks nearby or match 3 to call parent"

#### 2. Care Block 🤗 (Blue)
- **Created**: Parent spawns based on baby state
- **Match-3**: Generates Comfort resource
- **Proximity**: Near Need → Healing glow → Resolves Need
- **Multiple**: Creates "comfort zone" affecting nearby tiles
- **Tooltip**: "Caring hands! Match 3 for comfort or place near needs"

#### 3. Love Block 💕 (Pink)
- **Created**: Parent spawns when baby is happy
- **Match-3**: Joy burst - massive comfort + happiness
- **Proximity**: Radiates happiness to adjacent blocks
- **Special**: Can substitute for Care in proximity effects
- **Tooltip**: "Pure love! Match 3 for joy or use to help any need"

#### 4. Toy Block 🧸 (Yellow)
- **Created**: Random spawns for variety
- **Match-3**: Fun points (teaching basic matching)
- **Special**: ONLY block type that can merge (tutorial for later)
- **Merge**: 3 Toys → Super Toy (if unlocked after 30 turns)
- **Tooltip**: "Fun toy! Match 3 to play, or merge for bigger toys later"

#### 5. Sleep Block 😴 (Purple)
- **Created**: Every 10 turns ("naptime")
- **Match-3**: Rest bonus - clears all stress
- **Special**: Auto-matches after 5 turns (teaches patience)
- **Visual**: Slowly closing eyes animation over 5 turns
- **Tooltip**: "Getting sleepy... Match 3 for rest or wait for auto-sleep"

---

## 🧠 Proximity Effect System

### Core Rules
1. **Proximity = 1 tile distance** (orthogonal only, not diagonal)
2. **Effects take 2-3 turns** to resolve (teaches patience)
3. **Visual feedback starts immediately** (glowing, particles)

### Effect Matrix

| Block A | Block B | Turn 1 | Turn 2 | Turn 3 | Result |
|---------|---------|--------|--------|--------|--------|
| Need | Care | Both glow | Transform animation | Complete | Both become Happy, +Comfort |
| Need | Need | Red aura | Stress field | Parent reacts | Parent spawns 2 Care |
| Care | Care | Blue aura | Comfort zone | Affects nearby | Adjacent Needs resolve faster |
| Need | Love | Pink glow | Hearts animation | Complete | Need becomes Happy instantly |
| Sleep | Any | Z particles | Drowsy aura | - | Makes adjacent blocks "drowsy" |

### Visual Language
- **Glowing**: Something will happen (proximity effect active)
- **Pulsing**: Needs attention (stressed/alone)
- **Particles**: Good things happening (comfort/love)
- **Aura**: Area effect active (influences nearby)

---

## 🤖 Parent Block AI

### Position
- **Fixed**: Center tile (2,2) - never moves
- **Visual**: Friendly parent face that changes expression
- **Range**: Can spawn to any adjacent tile (8 positions)

### Emotional States

#### 😊 Happy State (Baby doing well)
```
Triggers: Recent chain, high comfort, Love matches
Actions: 
- Spawn rhythm: Every 4 turns
- Spawn type: 70% Care, 30% Love
- Special: Chance for bonus Toy blocks
Expression: Smiling, occasional hearts
```

#### 😟 Worried State (Baby struggling)
```
Triggers: 3+ Need blocks, low comfort, failed matches
Actions:
- Spawn rhythm: Every 2 turns (helping more)
- Spawn type: 90% Care, 10% Love
- Special: Can clear oldest Need if grid >20 blocks
Expression: Concerned, reaching out animation
```

#### 😌 Calm State (Default)
```
Triggers: Balanced play, steady progress
Actions:
- Spawn rhythm: Every 3 turns
- Spawn type: 80% Care, 20% Love
Expression: Peaceful, occasional blink
```

### Spawn Logic
```
Every N turns (based on state):
  Count Need blocks
  Check recent matches
  Evaluate baby mood
  
  If stress > threshold:
    Spawn 2 Care blocks
    Show "helping" animation
  Elif happy:
    Spawn 1 Love block
    Show hearts animation
  Else:
    Spawn 1 Care block
    Normal animation
```

---

## 💬 Tooltip System

### Context-Sensitive Tooltips

#### First-Time Tooltips (Tutorial)
- **On first Need**: "Your baby needs care! Try placing Care blocks nearby"
- **On first match**: "Great! Matching 3 blocks gives comfort"
- **On first proximity**: "See the glow? Blocks affect neighbors!"
- **On first chain**: "Wow! Chain reactions make baby extra happy"
- **On first stress**: "Too many needs! Parent will help"

#### Hover Tooltips (Always Available)
- **Need Block**: Current stress level, turns until crisis
- **Care Block**: Comfort value, nearby needs it can help
- **Parent Block**: Current mood, next spawn timing
- **Empty Tile**: What could spawn here
- **Score Area**: What comfort does, progress to growing up

#### Smart Hints (Adaptive)
- If no matches for 5 turns: "Try moving blocks to create matches"
- If many Needs: "Parent helps more when baby is stressed"
- If near chain setup: "One more move could trigger a chain!"
- If Sleep blocks present: "Sleep blocks auto-match in X turns"

### Tooltip Visual Design
```
[Soft rounded rectangle]
[Icon] Title (bold)
Description text (1-2 lines)
[Hint if applicable] (italic)
```

Colors:
- Need tooltips: Soft red background
- Care tooltips: Soft blue background
- Love tooltips: Soft pink background
- Hint tooltips: Soft yellow background

---

## 📈 Learning Progression

### Phase 1: Basic Matching (Turns 1-10)
- Only Need, Care blocks spawn
- Parent helps frequently
- Tooltips very active
- Goal: Learn same-type matching

### Phase 2: Proximity Discovery (Turns 11-20)
- Proximity effects become visible
- Love blocks introduced
- Parent helps less frequently
- Goal: Discover blocks affect neighbors

### Phase 3: Strategic Play (Turns 21-30)
- Toy blocks appear
- Sleep blocks introduced
- Chains become possible
- Goal: Learn planning ahead

### Phase 4: Merge Introduction (Turns 31-40)
- Toy merge unlocked
- More complex board states
- Parent in calm mode mostly
- Goal: Prepare for childhood stage

### Phase 5: Graduation (Turns 41-50)
- All mechanics active
- Preparing for transition
- "Growing up" messages appear
- Goal: Reach 50 comfort to advance

---

## 🎯 Success Metrics

### Immediate Feedback (Every Action)
- Particle effects on matches
- Sound effects for all interactions
- Parent expression changes
- Comfort number pops up (+5, +10, etc.)

### Progress Tracking
- **Comfort Score**: Main metric (0-50 for stage completion)
- **Happiness Level**: Affects spawn rates
- **Stress Level**: Triggers parent intervention
- **Chain Record**: Best chain achieved
- **Turn Count**: How long in infant stage

### No Failure State
- Cannot lose - only learn
- Stuck states trigger parent help
- Too much stress → Parent clears board edges
- Always forward progress

---

## 🎨 Visual & Audio Design

### Visual Hierarchy
1. **Parent Block**: Largest, most detailed, animated face
2. **Active Blocks**: Bright colors, clear icons
3. **Proximity Effects**: Glowing auras, particle connections
4. **Background**: Soft, nursery-themed, non-distracting

### Animation Priority
1. **Proximity effects**: Slow, clear, educational
2. **Matches**: Celebratory but not overwhelming
3. **Parent reactions**: Emotionally expressive
4. **Block emotions**: Subtle personality animations

### Audio Design
- **Need blocks**: Soft crying (not annoying)
- **Care blocks**: Gentle whoosh
- **Matches**: Musical chimes (C major pentatonic)
- **Love blocks**: Harp glissando
- **Parent helps**: Warm "there there" sound
- **Background**: Soft lullaby (optional)

---

## 🔄 Transition to Childhood

### Graduation Conditions
- Accumulated 50+ comfort
- Experienced all block types
- Completed at least one 3+ chain
- Player demonstrates understanding

### Transition Sequence
1. "Baby is growing!" message
2. Parent block celebrates
3. Grid expands animation (5x5 → 7x7)
4. Blocks transform to childhood versions
5. New mechanics unlock message

### What Carries Forward
- Basic match-3 understanding
- Merge concept (from Toys)
- Proximity effect awareness
- Emotional investment in character

---

## 📝 Technical Considerations

### Implementation Priority
1. **Core match-3 system** (exists, needs infant blocks)
2. **Parent block spawning** (new, central feature)
3. **Proximity effects** (new, defines infant stage)
4. **Tooltips system** (critical for learning)
5. **Visual polish** (can iterate)

### Reusable Systems
- Match detection (existing)
- Block spawning (modify existing)
- Grid management (use existing)
- Effect system (new, but reusable for all stages)
- Tooltip system (new, needed throughout game)

### Performance Considerations
- 5x5 grid = max 25 blocks (very lightweight)
- Proximity checks = max 24 × 4 directions (trivial)
- Parent AI = simple state machine
- No complex calculations needed

---

## 🎮 Player Experience Goals

### Emotional Journey
1. **Curiosity**: "What do these blocks do?"
2. **Discovery**: "Oh, they affect each other!"
3. **Mastery**: "I can set up chains!"
4. **Attachment**: "I'm taking care of my baby"
5. **Pride**: "My baby is growing up!"

### Skills Learned (Hidden Tutorial)
- Basic match-3 mechanics ✓
- Proximity effects awareness ✓
- Timing and patience ✓
- Resource management (comfort) ✓
- Merge concept (through Toys) ✓
- Emotional investment ✓

### Metrics for Success
- 90% of players complete infant stage
- Average time to understand: <2 minutes
- No tutorial skips needed
- High emotional engagement scores
- Smooth transition to childhood

---

## 🚀 Future Considerations

### Potential Expansions
- Photo album: Save cute moments
- Parent customization: Different parent types
- Twin mode: Manage two babies
- Seasonal themes: Holiday decorations

### Difficulty Variants
- **Easy**: Parent helps more, slower pacing
- **Normal**: As designed above
- **Challenge**: Less parent help, faster needs

### Accessibility
- Colorblind modes (shapes + colors)
- Reduced motion options
- Audio cues for all visual effects
- One-handed play support

---

*This design creates an emotionally engaging tutorial that doesn't feel like learning. Players nurture their way to understanding, building both mechanical skill and emotional investment from the very first moment.*