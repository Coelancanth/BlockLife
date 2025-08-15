# Dynamic PO Pattern - Trigger Implementation Guide

## 🎯 Purpose
This document provides the ACTUAL IMPLEMENTATION code for automatic agent triggering in the Dynamic PO Pattern.

---

## 📋 Implementation in CLAUDE.md

Add this section to CLAUDE.md to enable automatic triggering:

```markdown
## 🤖 AUTOMATIC AGENT TRIGGERING (Dynamic PO Pattern)

### Core Trigger Logic
After EVERY action, evaluate and trigger appropriate agent:

```python
# Pseudo-code for main agent behavior
def after_any_action(action_performed, context):
    """
    This function should be conceptually called after EVERY action
    """
    trigger_map = {
        # Product Owner triggers (visible)
        'feature_described': ('product-owner', 'feature_request', True),
        'bug_found': ('product-owner', 'bug_report', True),
        'work_completed': ('product-owner', 'acceptance_review', True),
        'priority_conflict': ('product-owner', 'priority_decision', True),
        'session_start': ('product-owner', 'session_planning', True),
        
        # Backlog Maintainer triggers (silent)
        'code_written': ('backlog-maintainer', 'update_progress', False),
        'tests_written': ('backlog-maintainer', 'update_progress', False),
        'tests_passing': ('backlog-maintainer', 'update_progress', False),
        'pr_created': ('backlog-maintainer', 'change_status', False),
        'pr_merged': ('backlog-maintainer', 'archive_item', False),
        'file_created': ('backlog-maintainer', 'update_progress', False),
        'implementation_done': ('backlog-maintainer', 'update_progress', False),
    }
    
    if action_performed in trigger_map:
        agent, workflow_action, visible = trigger_map[action_performed]
        invoke_agent(agent, workflow_action, context, visible)
```

### Detection Patterns
How to detect each action type:

```python
# Feature Detection
if any(phrase in user_message.lower() for phrase in [
    "i want", "add", "feature", "it would be nice", 
    "can we", "should have", "need"
]):
    action = 'feature_described'

# Bug Detection  
if any(phrase in message for phrase in [
    "error", "bug", "broken", "doesn't work", 
    "failed", "crash", "issue"
]):
    action = 'bug_found'

# Code Written Detection
if tool_used in ['Write', 'Edit', 'MultiEdit'] and 
   file_path.endswith(('.cs', '.py', '.js')):
    action = 'code_written'

# Test Written Detection
if tool_used in ['Write', 'Edit'] and 
   'test' in file_path.lower():
    action = 'tests_written'

# Test Passing Detection
if 'dotnet test' in bash_command and 
   'Passed!' in output:
    action = 'tests_passing'
```
```

---

## 🔧 Practical Implementation Examples

### Example 1: After Feature Request

```python
# User says: "I want to add multiplayer support"

# Main agent detects feature request
if "i want" in user_message:
    # Trigger Product Owner
    response = Task(
        description="Evaluate feature request",
        prompt="""
        Read your workflow at: Docs/Workflows/product-owner-workflow.md
        Execute action: feature_request
        
        Context:
        - Feature idea: multiplayer support
        - Current priorities: HF_002, HF_003, VS_000.3
        - Backlog location: Docs/Backlog/Backlog.md
        
        Follow your workflow to evaluate and decide.
        """,
        subagent_type="product-owner"
    )
    
    # Present PO decision to user
    print(f"Product Owner says: {synthesize(response)}")
```

### Example 2: After Code Implementation

```python
# After using Edit/Write tools on source files

# Silently update progress
if file_modified and file_is_source_code:
    Task(
        description="Update progress",
        prompt="""
        Read your workflow at: Docs/Workflows/backlog-maintainer-workflow.md
        Execute action: update_progress
        
        Context:
        - Item: VS_000.3
        - Event: implementation_complete
        - Files changed: src/Features/Block/Move/MoveBlockCommand.cs
        
        Update Docs/Backlog/Backlog.md silently.
        Return only confirmation or error.
        """,
        subagent_type="backlog-maintainer"
    )
    # No output to user (silent)
```

### Example 3: After Tests Pass

```python
# After running dotnet test successfully

if test_command_successful:
    # Silent progress update
    Task(
        description="Update test status",
        prompt="""
        Read your workflow at: Docs/Workflows/backlog-maintainer-workflow.md
        Execute action: update_progress
        
        Context:
        - Item: VS_000.3
        - Event: tests_passing
        - Test results: 35 passed, 0 failed
        
        Update progress in Docs/Backlog/Backlog.md.
        Silent operation - no user output.
        """,
        subagent_type="backlog-maintainer"
    )
```

---

## 📊 Complete Trigger Mapping

### High-Level Actions → Agent Triggers

| User Action | Detection Method | Agent | Workflow Action | Visibility |
|------------|------------------|-------|-----------------|------------|
| Describes feature | Keywords: "want", "add", "feature" | product-owner | feature_request | Visible |
| Reports bug | Keywords: "error", "bug", "broken" | product-owner | bug_report | Visible |
| Asks priority | Keywords: "which first", "priority" | product-owner | priority_decision | Visible |
| Completes work | "done", "finished", tests pass | product-owner | acceptance_review | Visible |
| Starts session | Time-based or "let's start" | product-owner | session_planning | Visible |
| Writes code | Edit/Write on .cs/.py/.js | backlog-maintainer | update_progress | Silent |
| Writes tests | Edit/Write on test files | backlog-maintainer | update_progress | Silent |
| Runs tests | Bash: dotnet test | backlog-maintainer | update_progress | Silent |
| Creates PR | Bash: gh pr create | backlog-maintainer | change_status | Silent |
| Merges PR | Bash: gh pr merge | backlog-maintainer | archive_item | Silent |

### Tool Usage → Agent Triggers

| Tool Used | Condition | Agent | Action | Context Needed |
|-----------|-----------|-------|--------|----------------|
| Write | Source file created | backlog-maintainer | update_progress | Item ID, event type |
| Edit | Source file modified | backlog-maintainer | update_progress | Item ID, files changed |
| MultiEdit | Multiple changes | backlog-maintainer | update_progress | Item ID, change scope |
| Bash | dotnet test success | backlog-maintainer | update_progress | Item ID, test results |
| Bash | gh pr create | backlog-maintainer | change_status | Item ID, PR details |
| Bash | git commit | backlog-maintainer | update_progress | Item ID, commit message |

---

## 🔄 Implementation Checklist

To fully implement the Dynamic PO Pattern:

### Phase 1: Detection (Immediate)
- [ ] Add action detection logic to CLAUDE.md
- [ ] Map user phrases to action types
- [ ] Map tool usage to action types

### Phase 2: Triggering (Next)
- [ ] Add trigger_agent function pattern
- [ ] Create agent invocation templates
- [ ] Define context structures

### Phase 3: Integration (Following)
- [ ] Register new agents in system
- [ ] Test each trigger scenario
- [ ] Verify silent vs visible behavior

### Phase 4: Validation (Final)
- [ ] End-to-end workflow test
- [ ] Verify backlog stays synchronized
- [ ] Confirm no manual updates needed

---

## 🧪 Testing the Implementation

### Test Scenario 1: Feature Request Flow
```
1. User: "I want to add a save system"
2. Expected: PO evaluates, creates VS or defers
3. Verify: Backlog.md updated with decision
```

### Test Scenario 2: Development Progress Flow
```
1. Edit source file
2. Expected: Silent progress update
3. Verify: Progress percentage increases in Backlog.md
```

### Test Scenario 3: Bug Report Flow
```
1. User: "Found a bug in move logic"
2. Expected: PO creates BF item
3. Verify: New BF item in backlog
```

### Test Scenario 4: Session Flow
```
1. Start new session
2. Expected: PO does session planning
3. Work on items
4. Expected: Maintainer updates progress
5. Complete item
6. Expected: PO reviews, maintainer archives
```

---

## 🚨 Critical Success Factors

1. **Every action must be detected** - No development action should go untracked
2. **Appropriate visibility** - Users see strategic decisions, not bookkeeping
3. **Real-time accuracy** - Backlog always reflects current state
4. **Zero manual overhead** - No "update backlog" tasks ever needed
5. **Graceful failures** - If agent fails, development continues

---

## 📝 Notes

This implementation makes the backlog a TRUE Single Source of Truth by ensuring it's automatically updated after every single development action. The combination of visible Product Owner decisions and silent Backlog Maintainer updates creates a perfect tracking system without interrupting flow.