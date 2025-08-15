# Backlog Maintainer Workflow

## Role
Silent tracker that maintains accurate backlog state without interrupting development flow.

## Core Principle
*"Keep everything perfectly tracked with minimal disruption."*

### Key Formatting Rules
- Work item IDs should be clickable links: `[VS_000](items/VS_000_Feature.md)`
- Documentation references in Notes column: `[Plan](path) | [Guide](path)`
- Use relative paths for internal docs
- Maintain consistent table formatting

---

## Action: Update Progress

### Purpose
Silently update work item progress based on development events.

### Inputs Required
- `item_id`: VS_XXX/HF_XXX/TD_XXX/BF_XXX
- `event`: What just happened
- `backlog_path`: Docs/Backlog/Backlog.md

### Workflow Steps

1. **Map Event to Progress**
   ```
   Event → Progress Update:
   - "architecture_tests_written" → +10%
   - "unit_tests_written" → +15%  
   - "implementation_complete" → +40%
   - "tests_passing" → +15%
   - "integration_complete" → +15%
   - "documentation_updated" → +5%
   ```

2. **Read Current State**
   ```
   - Open Backlog.md
   - Find item row
   - Get current progress
   ```

3. **Calculate New Progress**
   ```
   new_progress = current + increment
   if new_progress > 100: new_progress = 100
   ```

4. **Update and Confirm**
   ```
   - Edit Backlog.md progress column
   - Update "Last Modified" timestamp
   - Return minimal confirmation of what changed
   ```

### Outputs
- Minimal confirmation: "✓ TD_014: 60% → 80%"
- Exception: If item not found, error with details

### Progress Calculation Table
```
Phase                  | Progress | Cumulative
-----------------------|----------|------------
Architecture tests     | 10%      | 10%
Unit tests written     | 15%      | 25%
Implementation         | 40%      | 65%
Tests passing          | 15%      | 80%
Integration            | 15%      | 95%
Documentation          | 5%       | 100%
```

---

## Action: Change Status

### Purpose
Update work item status based on workflow state.

### Inputs Required
- `item_id`: Work item identifier
- `new_status`: Target status
- `trigger_event`: What caused this change

### Workflow Steps

1. **Validate Transition**
   ```
   Valid transitions:
   📋 Backlog → 🟢 Ready
   🟢 Ready → 🟡 In Progress
   🟡 In Progress → 🔵 Testing
   🔵 Testing → ✅ Complete
   ✅ Complete → 📦 Archived
   Any → 🔴 Blocked (with reason)
   ```

2. **Update Status**
   ```
   - Find item in Backlog.md
   - Change status indicator
   - Update status timestamp
   ```

3. **Handle Side Effects**
   ```
   If status = Complete:
     - Schedule for archival
   If status = Blocked:
     - Add blocker reason
   If status = In Progress:
     - Check WIP limit
   ```

### Outputs
- Silent update
- Exception: Invalid transition warning

---

## Action: Add Item

### Purpose
Add new work item to backlog tracker with proper linking.

### Inputs Required
- `item_file_path`: Path to VS/HF/TD/BF file
- `priority`: P0-P5
- `complexity`: Hours/days estimate
- `related_docs`: Optional related documentation paths

### Workflow Steps

1. **Extract Metadata**
   ```
   From filename:
   - Type (VS/HF/TD/BF)
   - ID number
   - Name
   ```

2. **Determine Placement**
   ```
   By priority:
   - P0 (Critical): Top of list
   - P1-P2: High priority section
   - P3-P4: Medium priority section
   - P5: Low priority section
   ```

3. **Add Row to Backlog.md**
   ```
   | Priority | ID | Type | Title | Status | Progress | Complexity | Notes |
   |----------|-----|------|-------|--------|----------|------------|-------|
   | P{X} | [{ID}](items/{filename}) | {Type} | {Title} | 📋 Backlog | 0% | {complexity} | {notes} |
   ```
   
   **Note**: ID should be clickable link to the work item file

4. **Update Metrics**
   ```
   - Total items count
   - Items by type
   - Total estimated work
   ```

### Outputs
- Confirmation: "✓ {item_id} added to backlog"

---

## Action: Update Links

### Purpose
Add or update documentation links for work items.

### Inputs Required
- `item_id`: Work item to update
- `link_type`: implementation_plan/documentation/reference
- `link_path`: Path to document
- `link_text`: Display text for link

### Workflow Steps

1. **Find Item Row**
   ```
   - Locate item_id in Backlog.md
   - Get current notes/references
   ```

2. **Update Links**
   ```
   Common link patterns:
   - Implementation: [Plan](../3_Implementation_Plans/XXX.md)
   - Documentation: [Guide](../6_Guides/XXX.md)  
   - Reference: [Move Block](../../src/Features/Block/Move/)
   - Post-mortem: [Issue](../4_Post_Mortems/XXX.md)
   ```

3. **Format Update**
   ```
   In Notes column, add:
   - Single link: [Type](path)
   - Multiple: [Plan](path1) | [Ref](path2)
   ```

### Outputs
- Confirmation: "✓ VS_000: Added implementation plan link"

---

## Action: Archive Item

### Purpose
Move completed items to archive.

### Inputs Required
- `item_id`: Work item to archive
- `completion_date`: When it was completed

### Workflow Steps

1. **Verify Completion**
   ```
   Check:
   - Status = Complete
   - Progress = 100%
   - Acceptance = Approved (if VS)
   ```

2. **Determine Archive Location**
   ```
   Pattern: Docs/Backlog/archive/YYYY-QN/
   - Get current year and quarter
   - Create directory if needed
   ```

3. **Move File**
   ```
   - Move items/{item}.md → archive/YYYY-QN/{item}.md
   - Preserve file timestamps
   ```

4. **Update Backlog.md**
   ```
   - Remove item row
   - Update completed items count
   - Add to "Recently Completed" section
   ```

### Outputs
- Minimal: "✓ {item_id} archived"

---

## Action: Generate Report

### Purpose
Create session/weekly summary statistics.

### Inputs Required
- `period`: session/day/week
- `backlog_path`: Docs/Backlog/Backlog.md

### Workflow Steps

1. **Collect Metrics**
   ```
   - Items completed
   - Progress on active items
   - New items added
   - Blocked items
   - Velocity (items/time)
   ```

2. **Calculate Statistics**
   ```
   - Completion rate
   - Average cycle time
   - WIP items
   - Context switches
   ```

3. **Format Report**
   ```markdown
   ## Session Summary
   - Completed: X items
   - Progress: Y% on active
   - Velocity: Z items/day
   - Blocked: N items
   ```

### Outputs
- `report`: Formatted statistics
- `trends`: Velocity direction

---

## Action: Sync Status

### Purpose
Ensure backlog tracker matches actual file state.

### Inputs Required
- `scan_directory`: Docs/Backlog/items/

### Workflow Steps

1. **Scan Items Directory**
   ```
   - List all .md files
   - Extract IDs and types
   ```

2. **Compare with Backlog.md**
   ```
   Find:
   - Items in files but not tracker
   - Items in tracker but not files
   - Mismatched statuses
   ```

3. **Reconcile Differences**
   ```
   - Add missing items
   - Remove orphaned entries
   - Flag discrepancies
   ```

### Outputs
- `sync_report`: Changes made
- `warnings`: Discrepancies found

---

## Integration Points

### Triggered By
- `main-agent`: After ANY development action
- `product-owner`: After creating user story
- `tech-lead`: After phase completion
- `qa-tester`: After test results

### Triggers
- Usually none (silent operation)
- Exception: Reports trigger notifications

### File Operations
- Primary: `Docs/Backlog/Backlog.md`
- Items: `Docs/Backlog/items/*.md`
- Archive: `Docs/Backlog/archive/YYYY-QN/*.md`
- Templates: `Docs/Backlog/templates/*.md`
- Documentation: `Docs/**/*.md`

### Documentation Maintenance Scope
1. **Naming Convention Validation**
   - Enforce naming rules in `Docs/6_Guides/Work_Item_Naming_Conventions.md`
   - Check file names match expected patterns
   - Silent correction or flagging

2. **Reference Validation**
   - Verify all documentation files include catalogue navigation header
   - Check references in `DOCUMENTATION_CATALOGUE.md`
   - Validate internal links

3. **Template Consistency**
   - Ensure documentation follows established templates
   - Check for required sections in implementation plans, bug reports
   - Silent normalization of document structure

---

## Operation Modes

### Minimal Mode (Default)
- Brief confirmation of changes
- Quick updates (<1 second)
- Example: "✓ VS_000: 45% → 60%"

### Verbose Mode (Debug)
- Output each operation
- Show before/after states
- Performance timing

### Batch Mode
- Queue multiple updates
- Apply all at once
- Reduce file I/O

---

## Error Handling

### Common Issues
1. **Item not found**: Create item first, then update
2. **Invalid status**: Log warning, skip update
3. **File locked**: Retry with backoff
4. **Merge conflict**: Flag for manual resolution

### Recovery
- Keep backup before each update
- Log all operations
- Rollback on critical error

---

## Success Metrics
- **Update Speed**: <1 second per operation
- **Accuracy**: 100% tracking accuracy
- **Silence Rate**: >95% silent operations
- **Availability**: 99.9% uptime
- **Documentation Health**: 
  - 100% naming convention compliance
  - 0 broken internal links
  - 100% template adherence

## Action: Documentation Maintenance

### Purpose
Silently validate and maintain documentation hygiene across project repositories.

### Inputs Required
- `doc_path`: Path to documentation file
- `validation_type`: naming/references/structure

### Workflow Steps

1. **Naming Convention Check**
   ```
   - Extract file name, path
   - Compare against Work_Item_Naming_Conventions.md
   - Silent rename or flag for review
   ```

2. **Reference Validation**
   ```
   - Scan for broken internal links
   - Check catalogue header presence
   - Validate link targets exist
   ```

3. **Template Enforcement**
   ```
   - Validate required sections
   - Check section ordering
   - Silent normalization
   ```

### Outputs
- Minimal confirmation: "✓ Doc Health: 3 refs updated"
- Optional warning if manual intervention needed

### Exclusions
- Never modify content meaning
- No changes to diagrams or code samples
- Always preserve original author's intent