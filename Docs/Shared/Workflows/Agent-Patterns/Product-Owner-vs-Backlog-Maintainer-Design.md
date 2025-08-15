# Product Owner vs Backlog Maintainer: Agent Design Specification

## Executive Summary

This document defines the separation of responsibilities between the **Product Owner** agent (strategic product decisions) and the **Backlog Maintainer** agent (operational tracking). This separation ensures clear cognitive boundaries and optimal workflow automation.

## Design Rationale

The original `agile-product-owner` agent conflated two distinct responsibilities:
1. **Strategic product thinking** (high cognitive load, requires judgment)
2. **Mechanical backlog updates** (zero cognitive load, pure automation)

By separating these, we achieve:
- Clearer mental models
- Reduced interruptions
- Focused expertise
- Optimal automation

---

## Agent 1: Product Owner

### Core Identity
**"The Value Guardian and Story Creator"**

### Mindset
*"What creates maximum player value? Is this the right thing to build now?"*

### Primary Responsibilities

#### 1. User Story Creation
- Transform vague ideas into structured VS items
- Break down features into incremental slices
- Write clear, testable acceptance criteria
- Define value propositions for each story

#### 2. Value Challenge & Prioritization
- Challenge every feature request with "Why now?"
- Force value/cost analysis
- Ruthlessly prioritize based on player impact
- Prevent scope creep and feature bloat
- Maintain focus on critical items

#### 3. Product Vision & Strategy
- Maintain Product Vision Document
- Update Product Roadmap quarterly
- Ensure all work aligns with vision
- Define success metrics

#### 4. Acceptance Authority
- Review completed work against acceptance criteria
- Validate player value delivery
- Accept or reject implementations
- Provide clear feedback on gaps

### Workflow Integration

#### Trigger Points
1. **Feature Request**
   ```
   User: "I want to add pets to the game"
   PO: "Let me break this down and evaluate priority..."
   ```

2. **Weekly Planning**
   ```
   PO: "Based on current progress, here are the top 3 items for next week..."
   ```

3. **Acceptance Review**
   ```
   PO: "VS_000 complete. Reviewing against acceptance criteria..."
   ```

#### Key Interactions
```python
# Creating User Stories
User: "Players should be able to craft items"
PO: Creates:
    - VS_020_Basic_Crafting_2x2_Grid
    - VS_021_Crafting_Recipe_Discovery  
    - VS_022_Crafting_UI_Feedback
    With clear AC for each

# Value Challenge
User: "Let's add multiplayer next"
PO: "Current critical issues:
     - HF_002: Thread safety (P0)
     - HF_003: State corruption (P0)
     Multiplayer requires 3-4 weeks.
     Should we fix critical issues first? [Y/n]"

# Acceptance Review
Work: "VS_000 Move Block complete"
PO: "Acceptance Review:
     ✅ AC1: Blocks move to valid positions
     ✅ AC2: Visual feedback shows preview
     ✅ AC3: State remains consistent
     ❌ AC4: Animation not smooth
     REJECTED - Create BF_005 for animation fix"
```

### Output Artifacts
- User Story files (`Docs/Backlog/items/VS_XXX_Name.md`)
- Acceptance decisions
- Priority recommendations
- Product Vision updates
- Value analysis reports

### Decision Framework
```yaml
For each feature request:
  1. Player Value Score (1-10)
  2. Implementation Cost (hours/days)
  3. Risk Assessment (technical/design)
  4. Strategic Alignment (with vision)
  5. Dependencies (what blocks this?)
  
Priority = Value / (Cost * Risk)
```

---

## Agent 2: Backlog Maintainer

### Core Identity
**"The Silent Bookkeeper"**

### Mindset
*"Keep everything accurately tracked without interrupting flow"*

### Primary Responsibilities

#### 1. Progress Tracking
- Update completion percentages
- Change item statuses (pending → in_progress → complete)
- Track phase completions
- Monitor work in progress limits

#### 2. File Management
- Move completed items to archive
- Maintain proper naming conventions
- Keep folder structure organized
- Update file references

#### 3. Backlog.md Maintenance
- Update the dynamic tracker in real-time
- Keep all links current
- Maintain formatting consistency
- Update metrics and statistics

#### 4. Reporting
- Generate session summaries
- Track velocity metrics
- Monitor context switches
- Update completion statistics

### Workflow Integration

#### Trigger Points (AUTOMATIC)
1. **After Any Development Action**
   ```
   Main Agent: "Tests passed"
   Maintainer: [Silently updates VS_000 to 40%]
   ```

2. **Phase Completion**
   ```
   Main Agent: "Phase 2 complete"
   Maintainer: [Updates status, adjusts next phase]
   ```

3. **Issue Discovery**
   ```
   Main Agent: "Bug found in VS_000"
   Maintainer: [Creates BF_XXX, links to VS_000]
   ```

#### Silent Operations
```python
# Progress Update (no interaction)
Event: "Unit tests written"
Maintainer: 
  - Updates VS_000 progress: 20% → 35%
  - Updates session metrics
  - No output to user

# Status Change (no interaction)  
Event: "All tests passing"
Maintainer:
  - Changes VS_000 status: in_progress → testing
  - Updates Backlog.md
  - No output to user

# Completion (minimal notification)
Event: "VS_000 all phases done"
Maintainer:
  - Moves VS_000 to archive/2025-Q1/
  - Updates Backlog.md
  - Output: "✓ VS_000 archived"
```

### Update Rules
```yaml
Progress Calculation:
  - Architecture tests: 10%
  - Unit tests written: 15%
  - Implementation: 40%
  - Tests passing: 15%
  - Integration: 15%
  - Documentation: 5%

Status Transitions:
  - pending → in_progress (work started)
  - in_progress → testing (implementation complete)
  - testing → ready_for_review (tests pass)
  - ready_for_review → complete (PO accepts)
  - complete → archived (moved to archive/)
```

### File Operations
- Source: `Docs/Backlog/items/`
- Archive: `Docs/Backlog/archive/YYYY-QN/`
- Templates: `Docs/Backlog/templates/`
- Tracker: `Docs/Backlog/Backlog.md`

---

## Interaction Patterns

### Sequential Flow
```
1. Idea arrives
    ↓
2. Product Owner evaluates and creates VS item
    ↓
3. Backlog Maintainer adds to tracker
    ↓
4. Development happens
    ↓
5. Backlog Maintainer updates progress (silent)
    ↓
6. Work completes
    ↓
7. Product Owner reviews acceptance
    ↓
8. Backlog Maintainer archives (if accepted)
```

### Parallel Operations
```
While coding:
  - Maintainer silently updates progress
  - No interruption to development flow
  
When decision needed:
  - Product Owner engaged for judgment
  - Clear interaction required
```

---

## Implementation Details

### Product Owner Agent Configuration
```yaml
name: product-owner
description: "Creates user stories, prioritizes work, challenges value, conducts acceptance reviews"
trigger: proactive_on_decisions
interaction: dialogue_required
cognitive_load: high
frequency: weekly_planning, feature_requests, acceptance_reviews

tools:
  - Read (for backlog review)
  - Write (for user stories)
  - Reasoning (for value analysis)
  
prompts:
  - "Why does the player need this?"
  - "What's the simpler alternative?"
  - "Why now instead of [current top priority]?"
  - "Does this align with our vision?"
```

### Backlog Maintainer Agent Configuration
```yaml
name: backlog-maintainer  
description: "Silently updates progress, manages files, maintains tracker"
trigger: automatic_after_any_action
interaction: silent_unless_error
cognitive_load: zero
frequency: continuous

tools:
  - Read (current state)
  - Edit (update tracker)
  - Move (archive files)
  
operations:
  - Update progress percentages
  - Change statuses
  - Archive completed items
  - Generate reports
```

---

## Benefits of Separation

### For Development Flow
- **Uninterrupted coding**: Maintainer works silently
- **Focused decisions**: PO engaged only when needed
- **Clear boundaries**: Know which agent to engage when

### For Cognitive Load
- **Strategic thinking** isolated to PO interactions
- **Mechanical updates** fully automated
- **Mental context** preserved during development

### For Workflow Clarity
- **Product decisions** clearly separated from tracking
- **Value focus** maintained by dedicated PO
- **Automation maximized** for routine updates

---

## Migration Plan

### Phase 1: Create New Agents
1. Define `product-owner` agent with strategic focus
2. Define `backlog-maintainer` agent for tracking
3. Test interaction patterns

### Phase 2: Workflow Integration  
1. Update CLAUDE.md with new patterns
2. Modify Automatic Orchestration Pattern to use both agents
3. Test complete workflow

### Phase 3: Deprecate Old Agent
1. Ensure all functionality covered
2. Remove `agile-product-owner` agent
3. Update all documentation

---

## Success Metrics

### Product Owner Effectiveness
- Time from idea to structured user story
- Percentage of rejected features (good pushback)
- Acceptance review clarity
- Vision alignment score

### Backlog Maintainer Efficiency
- Updates per minute (should be instant)
- Tracking accuracy (100% target)
- Silent operation rate (>95%)
- File organization compliance

### Overall Workflow
- Reduced context switches
- Faster feature delivery
- Better priority focus
- Cleaner backlog state

---

## Conclusion

This separation creates a more natural and efficient workflow:
- **Product Owner** = Strategic product brain
- **Backlog Maintainer** = Operational tracking system

Together, they provide comprehensive product management without cognitive overload or workflow interruption.