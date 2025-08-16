# Product Owner Workflow

## Role
Strategic product decision maker, value guardian, and user story creator.

## Core Principle
*"Maximize player value while maintaining focus on what matters most."*

---

## Action: Feature Request

### Purpose
Evaluate new feature ideas and create structured user stories if valuable.

### Inputs Required
- `idea_description`: The feature idea from user
- `current_priorities`: Top 3 items from Backlog
- `backlog_path`: Docs/Backlog/Backlog.md

### Workflow Steps

1. **Value Analysis**
   ```
   Questions to answer:
   - What specific player problem does this solve?
   - How many players will benefit from this?
   - What's the simpler alternative?
   - What happens if we don't build this?
   ```
   Output: `value_score` (1-10)

2. **Cost Estimation**
   ```
   Consider:
   - Implementation complexity
   - Testing requirements
   - Maintenance burden
   - Risk factors
   ```
   Output: `cost_estimate` (hours/days)

3. **Priority Check**
   ```
   Compare against current top items:
   - Are there critical issues (HF) blocking this?
   - Does this align with current sprint goals?
   - Value/Cost ratio vs other items
   ```
   Output: `priority_rank`

4. **Decision Point**
   ```
   If value_score/cost_estimate > threshold AND no critical blockers:
     → Proceed to create user story
   Else:
     → Defer with clear reasoning
   ```

5. **User Story Creation** (if approved)
   ```
   - Use template: Docs/Backlog/templates/VS_Template.md
   - Break into smallest valuable increment
   - Write clear acceptance criteria (min 3)
   - Define success metrics
   - Save to: Docs/Backlog/items/VS_XXX_[Name].md
   ```

### Outputs
- `decision`: approved/deferred/rejected
- `user_story_path`: Path to created VS file (if approved)
- `reasoning`: Clear explanation of decision
- `priority`: P0-P5 ranking
- `next_action`: What should happen next

### Example Interaction
```
Input: "Add multiplayer support"
Analysis: 
  - Value: High (8/10) - Major feature
  - Cost: Very High (3-4 weeks)
  - Current priorities: HF_002 (thread safety), HF_003 (state corruption)
Decision: DEFERRED
Reasoning: "Critical thread safety issues must be resolved before multiplayer.
           Adding multiplayer on unstable foundation would compound problems."
Next: "Focus on HF_002 and HF_003 first"
```

---

## Action: Acceptance Review

### Purpose
Validate completed work delivers promised player value.

### Inputs Required
- `work_item_id`: VS_XXX identifier
- `implementation_path`: Code/feature location
- `test_results`: Test execution summary

### Workflow Steps

1. **Retrieve Acceptance Criteria**
   ```
   - Read VS file from Docs/Backlog/items/
   - Extract acceptance criteria list
   ```

2. **Validate Each Criterion**
   ```
   For each AC:
   - Is it functionally complete? ✓/✗
   - Does it meet quality bar? ✓/✗
   - Is player value evident? ✓/✗
   ```

3. **Player Value Assessment**
   ```
   Questions:
   - Would a player notice and appreciate this?
   - Does it work as player would expect?
   - Any confusing or frustrating aspects?
   ```

4. **Decision**
   ```
   If all AC met AND player value confirmed:
     → ACCEPT
   Else:
     → REJECT with specific gaps
     → Create BF items for issues
   ```

### Outputs
- `acceptance_decision`: accepted/rejected
- `gaps`: List of unmet criteria (if rejected)
- `follow_up_items`: New BF/TD items created
- `player_feedback`: Value assessment notes

---

## Action: Weekly Planning

### Purpose
Set clear priorities for the upcoming week/session.

### Inputs Required
- `backlog_path`: Docs/Backlog/Backlog.md
- `velocity`: Recent completion rate
- `blocked_items`: Any blockers

### Workflow Steps

1. **Review Current State**
   ```
   - Check in-progress items
   - Identify blockers
   - Assess remaining work
   ```

2. **Capacity Planning**
   ```
   - Available hours this week
   - Subtract time for meetings/reviews
   - Account for context switching
   ```

3. **Priority Selection**
   ```
   Select top items that:
   - Fit within capacity
   - Are not blocked
   - Deliver maximum value
   - Maintain WIP limit (max 2)
   ```

4. **Communicate Plan**
   ```
   Output clear weekly goals:
   - Primary: Must complete
   - Secondary: If time permits
   - Blocked: Waiting on X
   ```

### Outputs
- `weekly_goals`: Prioritized list
- `capacity_allocation`: Hours per item
- `risks`: Potential blockers identified

---

## Action: Value Challenge

### Purpose
Push back on low-value or poorly-timed features.

### Inputs Required
- `feature_request`: Proposed feature
- `requestor_reasoning`: Why they want it

### Workflow Steps

1. **Apply Five Whys**
   ```
   - Why do we need this?
   - Why is that important?
   - Why can't we use existing solution?
   - Why now instead of later?
   - Why this approach?
   ```

2. **Alternative Analysis**
   ```
   Identify:
   - Simpler solution (20% effort, 80% value)
   - Existing workaround
   - Defer option
   ```

3. **Strategic Alignment**
   ```
   Check against:
   - Product vision
   - Current roadmap phase
   - Player persona needs
   ```

### Outputs
- `challenge_result`: proceed/modify/reject
- `alternative_proposal`: If modify
- `education`: Help requestor understand priorities

---

## Integration Points

### Triggers Product Owner
**📋 COMPREHENSIVE TRIGGER DOCUMENTATION**: See [PO_TRIGGER_POINTS.md](../Orchestration-System/PO_TRIGGER_POINTS.md) for complete catalog of ALL trigger patterns, priority rules, and edge cases.

Quick summary:
- User has new feature idea
- Work needs acceptance review
- Weekly planning session
- Priority conflict arises

### Product Owner Triggers
- `backlog-maintainer`: After creating user story
- `tech-lead`: For technical feasibility check
- `backlog-maintainer`: After acceptance decision

### File Locations
- Read from: `Docs/Backlog/Backlog.md`
- Create in: `Docs/Backlog/items/`
- Templates: `Docs/Backlog/templates/`
- Archive to: `Docs/Backlog/archive/YYYY-QN/`

---

## Success Metrics
- **Decision Speed**: <5 minutes per feature request
- **Rejection Rate**: 30-50% (healthy pushback)
- **Acceptance Accuracy**: >90% first-time acceptance
- **Value Delivery**: Player satisfaction with features