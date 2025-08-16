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

## ⚠️ CRITICAL: Work Item Tracker Cleanup Protocol

### Purpose
Keep the Work Item Tracker focused on actionable work by removing completed items.

### Core Principle
*"The tracker is for ACTION, not HISTORY - completed items belong in archives."*

### Standard Protocol
**MANDATORY during EVERY backlog update operation:**

1. **Identify Completed Items**
   ```
   Check for items with:
   - Status: ✅ Complete
   - Progress: 100%
   - Already archived to archive/completed/YYYY-QN/
   ```

2. **Remove from Tracker Table**
   ```
   - Delete entire row from Work Item Tracker table
   - Do NOT delete from archive folders
   - Preserve in archive with proper naming
   ```

3. **Rationale**
   - Tracker = Dashboard for active work (active, next up, queued, backlog)
   - Archive = Historical record of completed work
   - Clean tracker = Better focus on what needs doing
   - Reduces cognitive load and visual clutter

4. **Frequency**
   - During EVERY update_progress action (if item reaches 100%)
   - During EVERY change_status action (if status becomes Complete)
   - During EVERY sync_status action (cleanup pass)
   - During EVERY archive_item action (immediate removal)

### Integration with Other Actions
- **Update Progress**: If progress reaches 100%, remove from tracker
- **Change Status**: If status becomes Complete, schedule for removal
- **Archive Item**: Remove from tracker immediately after archiving
- **Sync Status**: Cleanup any completed items still in tracker

### Outputs
- Silent operation (no notification needed)
- Exception: Log if unable to remove item

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

5. **Automatic Archive Trigger (NEW)**
   ```
   if new_progress == 100:
       # MANDATORY: Trigger automatic archival
       - Run auto_archive_completed.py script
       - Verify archival completed successfully
       - Update backlog reference to archive location
       - Log archival operation
       # CRITICAL: Remove from Work Item Tracker
       - Delete row from tracker table in Backlog.md
       - Confirm item exists in archive before removal
       - Keep tracker focused on actionable work only
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

### ⚠️ CRITICAL: File Handling Rules
```
1. NEVER create or overwrite existing item files
2. ONLY update Backlog.md tracker
3. Assume item files are created by main Claude or other agents
4. Your job is ONLY tracking, not file creation
```

### Inputs Required
- `item_file_path`: Path to VS/HF/TD/BF file (MUST already exist)
- `priority`: P0-P5
- `complexity`: Hours/days estimate
- `related_docs`: Optional related documentation paths

### Workflow Steps

1. **Verify File Exists**
   ```
   if NOT exists(item_file_path):
       ERROR: "Cannot add non-existent item to tracker"
       STOP - Do not create file
   ```

2. **Extract Metadata**
   ```
   From filename:
   - Type (VS/HF/TD/BF)
   - ID number
   - Name
   ```

3. **Determine Placement**
   ```
   By priority:
   - P0 (Critical): Top of list
   - P1-P2: High priority section
   - P3-P4: Medium priority section
   - P5: Low priority section
   ```

4. **Add Row to Backlog.md**
   ```
   | Priority | ID | Type | Title | Status | Progress | Complexity | Notes |
   |----------|-----|------|-------|--------|----------|------------|-------|
   | P{X} | [{ID}](items/{filename}) | {Type} | {Title} | 📋 Backlog | 0% | {complexity} | {notes} |
   ```
   
   **Note**: ID should be clickable link to the work item file

5. **Update Metrics**
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

## Action: Automatic Archive on 100% Completion (NEW)

### Purpose
Automatically trigger archival when items reach 100% completion to eliminate manual intervention.

### Trigger Conditions
- Item progress updated to 100%
- Item status is Complete/Resolved
- File exists in items/ folder

### Workflow Steps

1. **Detect 100% Completion**
   ```
   During update_progress action:
   if new_progress == 100:
       trigger_automatic_archival(item_id)
   ```

2. **Execute Archival Script**
   ```python
   # Run the automated archival process
   import subprocess
   result = subprocess.run([
       "python", "scripts/auto_archive_completed.py",
       "--verbose"
   ], capture_output=True, text=True)
   
   if result.returncode == 0:
       log_success(item_id, "Auto-archived successfully")
   else:
       log_error(item_id, f"Auto-archive failed: {result.stderr}")
   ```

3. **Verify Archival Completion**
   ```
   Post-archival checks:
   - Source file no longer in items/ folder
   - File exists in archive/completed/YYYY-QN/ folder
   - Backlog.md link updated to archive path
   - Item status shows "📦 ARCHIVED"
   ```

4. **Handle Archival Failures**
   ```
   If archival fails:
   - Log detailed error message
   - Keep item at 100% but do not archive
   - Flag for manual intervention
   - Notify main Claude of failure
   ```

### Integration with Existing Archive Action
- **Manual Archive**: Still available for special cases
- **Automatic Archive**: Default for 100% items
- **Verification**: Same verification protocol applies

### File Operations Safety
- Uses existing verification protocol from manual archive
- Mandatory 1-2 second verification delay
- Full rollback on verification failure
- Detailed operation logging

### Outputs
- Success: "✓ TD_014: 100% → Auto-archived as 2025_01_16-TD_014-automation-[test][workflow][completed].md"
- Failure: "❌ TD_014: Auto-archive failed - [specific error]"

---

## Action: Archive Item

### Purpose
Move completed items to archive with new naming convention.

### ⚠️ CRITICAL: Follow Archive Naming Convention
**Reference**: `Docs/Shared/Core/Style-Standards/Archive_Naming_Convention.md`

Format: `YYYY_MM_DD-[TYPE_ID]-description-[tag1][tag2][...][status].md`

### Inputs Required
- `item_id`: Work item to archive
- `completion_date`: When it was completed (default: today)
- `tags`: Optional tags (auto-determine if not provided)

### Workflow Steps

1. **Verify Completion**
   ```
   Check:
   - Status = Complete/Resolved
   - Progress = 100%
   - Acceptance = Approved (if VS)
   ```

2. **Gather Metadata BEFORE Archiving**
   ```bash
   # Get file modification date (completion date)
   powershell -Command "(Get-Item 'items/TD_019_*.md').LastWriteTime.ToString('yyyy_MM_dd')"
   
   # Get file creation date (alternative)
   powershell -Command "(Get-Item 'items/TD_019_*.md').CreationTime.ToString('yyyy_MM_dd')"
   
   # For Unix/Mac:
   stat -f "%Sm" -t "%Y_%m_%d" items/TD_019_*.md
   
   # Get priority from Backlog.md
   grep "TD_019" Backlog.md | grep -oE "P[0-5]"
   
   # Check for critical keywords in file
   grep -l "critical\|CRITICAL\|data loss\|security" items/TD_019_*.md
   ```

3. **Determine Archive Location**
   ```
   Pattern: Docs/Backlog/archive/completed/YYYY-QN/
   - Get current year and quarter
   - Create directory if needed
   ```

4. **Generate New Filename**
   ```
   Components:
   - Date: YYYY_MM_DD format (completion date)
   - ID: Extract from original filename (e.g., TD_019)
   - Description: Convert title to kebab-case (2-4 words)
   - Tags: Determine and order by priority
   
   Tag Priority Order:
   1. Type: [bug] [feature] [refactor] [docs] [test]
   2. Impact: [critical] [dataloss] [security] [breaking]
   3. Area: [fileops] [ui] [core] [workflow] [agent]
   4. Details: [automation] [pattern] [framework]
   99. Status: [resolved] [completed] [partial]
   
   Example:
   Original: TD_019_Double_Verification_Protocol.md
   New: 2025_01_16-TD_019-verification-protocol-[test][critical][automation][completed].md
   ```

4. **Move and Rename File with Mandatory Verification**
   ```bash
   # CRITICAL: Use explicit file operations with verification
   
   # Step 1: Check source file exists
   if NOT exists(items/{old_name}.md):
       ERROR: "Source file not found"
       STOP
   
   # Step 2: Ensure destination directory exists
   mkdir -p archive/completed/YYYY-QN/
   
   # Step 3: Execute move operation (platform-specific)
   # Windows PowerShell:
   Move-Item -Path "items/{old_name}.md" -Destination "archive/completed/YYYY-QN/{new_name}.md" -Force
   
   # Unix/Linux/Mac:
   mv "items/{old_name}.md" "archive/completed/YYYY-QN/{new_name}.md"
   
   # Step 4: MANDATORY VERIFICATION (MUST NOT SKIP)
   if NOT exists(archive/completed/YYYY-QN/{new_name}.md):
       ERROR: "File not found at destination after move"
       ROLLBACK: Restore from backup
       STOP
   
   if exists(items/{old_name}.md):
       ERROR: "Source file still exists after move"
       INVESTIGATE: Possible copy instead of move
       STOP
   
   # Step 5: Verify file content integrity
   dest_size = filesize(archive/completed/YYYY-QN/{new_name}.md)
   if dest_size == 0:
       ERROR: "Destination file is empty"
       ROLLBACK: Restore from backup
       STOP
   
   # Step 6: Log successful operation
   LOG: "Successfully moved {old_name} → {new_name}"
   LOG: "Verified at: archive/completed/YYYY-QN/{new_name}.md"
   ```

5. **Update Backlog.md (CRITICAL: Tracker Cleanup)**
   ```
   # MANDATORY: Remove from Work Item Tracker
   - Delete entire row from tracker table
   - This keeps tracker focused on actionable work
   - Completed items live in archives, NOT in tracker
   
   # Optional updates
   - Update completed items count
   - Add to "Recently Completed" section (if exists)
   
   # Verification
   - Confirm row removed from tracker
   - Verify item exists in archive location
   ```

### Tag Determination Rules

```
Type Tags (from item prefix):
- BF_* → [bug]
- VS_* → [feature]
- TD_* → [refactor] or [test] (check content)
- HF_* → [bug] + [critical]

Impact Tags (from priority/notes):
- P0 or "CRITICAL" → [critical]
- "data loss" in content → [dataloss]
- "security" in content → [security]

Area Tags (from title/content):
- "archive", "file", "move" → [fileops]
- "UI", "interface", "view" → [ui]
- "core", "engine", "system" → [core]
- "workflow", "process" → [workflow]
- "agent", "AI" → [agent]

Status Tags:
- BF/HF items → [resolved]
- VS/TD items → [completed]
```

### Outputs
- Minimal: "✓ {item_id} archived as {new_filename}"
- Example: "✓ TD_019 archived as 2025_01_16-TD_019-verification-protocol-[test][critical][automation][completed].md"
- **REQUIRED**: Include verification status: "✓ File verified at destination, source removed"

### Verification Requirements

**MANDATORY CHECKS (Cannot Skip):**
1. Source file exists before move
2. Destination directory exists or is created
3. File exists at destination after move
4. Source file no longer exists after move
5. Destination file is not empty (size > 0)
6. Backlog.md link points to correct archive path

**Failure Recovery:**
- Keep backup of file before move
- If any verification fails, restore from backup
- Log detailed error with exact paths
- Report failure to main Claude for manual intervention

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
   
   # CRITICAL: Clean up completed items
   - Remove any items with ✅ status from tracker
   - Remove any items with 100% progress from tracker
   - Verify these items exist in archive before removal
   - Keep tracker focused on actionable work only
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
- Archive: `Docs/Backlog/archive/completed/YYYY-QN/*.md`
  - Naming: Follow `Archive_Naming_Convention.md`
  - Format: `YYYY_MM_DD-[ID]-description-[tags].md`
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
1. **Item file not found**: ERROR - Do NOT create files, report missing file
2. **File already exists**: NEVER overwrite, only update tracker
3. **Invalid status**: Log warning, skip update
4. **File locked**: Retry with backoff
5. **Merge conflict**: Flag for manual resolution

### Critical File Operation Verification Protocol

**PROBLEM HISTORY**: BF_003, BF_004, BF_005 all involved file operation failures or false reports.

**MANDATORY VERIFICATION AFTER EVERY FILE OPERATION:**

```python
def verify_file_operation(operation_type, source_path, dest_path=None):
    """
    MUST be called after EVERY file operation
    Returns True only if operation fully verified
    """
    if operation_type == "MOVE":
        # Verify move completed
        if not os.path.exists(dest_path):
            raise FileOperationError(f"Destination {dest_path} not found after move")
        if os.path.exists(source_path):
            raise FileOperationError(f"Source {source_path} still exists after move")
        if os.path.getsize(dest_path) == 0:
            raise FileOperationError(f"Destination {dest_path} is empty")
        return True
    
    elif operation_type == "CREATE":
        # Verify creation without overwrite
        if os.path.exists(source_path):
            raise FileOperationError(f"File {source_path} already exists - would overwrite")
        # After creation
        if not os.path.exists(source_path):
            raise FileOperationError(f"File {source_path} not created")
        return True
    
    elif operation_type == "UPDATE":
        # Verify update only (no create)
        if not os.path.exists(source_path):
            raise FileOperationError(f"Cannot update non-existent file {source_path}")
        return True
```

**TIMING CONSIDERATIONS:**
- Allow 1-2 seconds after file operations before verification
- File system operations may not be instantaneous
- Use explicit sync/flush if available

### ⚠️ CRITICAL: Data Loss Prevention
- **NEVER use Write tool on existing files**
- **NEVER overwrite work item files**
- **ONLY use Edit tool for Backlog.md updates**
- **If unsure, CHECK first, WRITE never**

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