# Dynamic PO Pattern - Quick Onboarding Guide

## 🚀 Quick Start for New Conversations

This file helps Claude Code quickly understand the Dynamic Product Owner Pattern and agent workflow system implemented in this project.

---

## 📋 What is the Dynamic PO Pattern?

A workflow system where specialized agents automatically maintain the Backlog as the Single Source of Truth after EVERY development action. No manual tracking needed!

### The Two Key Agents

1. **`product-owner`** - Strategic decision maker
   - Location: `.claude/agents/product-owner.md`
   - Workflow: `Docs/Workflows/product-owner-workflow.md`
   - Purpose: Evaluates features, creates user stories, prioritizes work
   - Trigger: Feature requests, bug reports, completion reviews

2. **`backlog-maintainer`** - Silent bookkeeper
   - Location: `.claude/agents/backlog-maintainer.md`
   - Workflow: `Docs/Workflows/backlog-maintainer-workflow.md`
   - Purpose: Updates progress, changes statuses, manages links
   - Trigger: After code changes, tests, PR operations

---

## 🤖 How to Trigger Agents (CRITICAL)

### You (Claude Code) MUST announce triggers in early stage:

```
🤖 AGENT TRIGGER: [Reason detected]
   → Invoking [Agent] for [Action]
   → Context: [What's being processed]
```

### Product Owner Triggers (Visible)

| User Says | You Do |
|-----------|--------|
| "I want to add..." | Trigger PO for `feature_request` |
| "Bug/Error found" | Trigger PO for `bug_report` |
| "Work complete" | Trigger PO for `acceptance_review` |
| "What's priority?" | Trigger PO for `priority_decision` |

**Example Invocation:**
```python
Task(
    description="Evaluate feature request",
    prompt="""
    Read your workflow at: Docs/Workflows/product-owner-workflow.md
    Execute action: feature_request
    
    Context:
    - Feature idea: [what user wants]
    - Current priorities: [top 3 from Backlog]
    - Backlog: Docs/Backlog/Backlog.md
    
    Follow your workflow exactly.
    """,
    subagent_type="product-owner"
)
```

### Backlog Maintainer Triggers (Minimal Output)

| After You | Do This |
|-----------|---------|
| Edit/Write code | Trigger maintainer `update_progress` (+40%) |
| Write tests | Trigger maintainer `update_progress` (+15%) |
| Tests pass | Trigger maintainer `update_progress` (+15%) |
| Create PR | Trigger maintainer `change_status` |

**Example Invocation:**
```python
Task(
    description="Update progress",
    prompt="""
    Read your workflow at: Docs/Workflows/backlog-maintainer-workflow.md
    Execute action: update_progress
    
    Context:
    - Item: VS_000
    - Event: implementation_complete
    - Files changed: [list]
    
    Update Docs/Backlog/Backlog.md with minimal confirmation.
    """,
    subagent_type="backlog-maintainer"
)
```

---

## 📁 Key Files Structure

```
Docs/
├── Backlog/
│   ├── Backlog.md           # ⭐ SINGLE SOURCE OF TRUTH
│   ├── items/               # Work item files (VS/BF/TD/HF)
│   └── archive/             # Completed items
├── Workflows/
│   ├── product-owner-workflow.md
│   ├── backlog-maintainer-workflow.md
│   ├── AGENT_ORCHESTRATION_GUIDE.md    # ⚠️ MUST READ
│   └── ORCHESTRATION_FEEDBACK_SYSTEM.md
└── 1_Architecture/
    └── Agent_Architecture_and_Workflow_Patterns.md

.claude/
└── agents/
    ├── product-owner.md
    └── backlog-maintainer.md
```

---

## ⚠️ CRITICAL RULES

### 1. ALWAYS Read These First
- **CLAUDE.md** - Has links to orchestration guide
- **AGENT_ORCHESTRATION_GUIDE.md** - Complete trigger patterns
- **Workflow files** - Before triggering any agent

### 2. Announcement Mode (Early Stage)
We're in transparency mode - ALWAYS announce when triggering agents:
- Shows user what's happening
- Helps validate patterns
- Builds trust in the system

### 3. Feedback Commands
User can report issues with:
- **"Missed trigger!"** - You should have triggered but didn't
- **"Wrong agent!"** - Wrong agent used
- **"No announcement!"** - Forgot to announce
- **"Bug: [details]"** - Detailed issue

---

## 🎯 Common Scenarios

### Scenario 1: User Requests Feature
```
User: "I want to add multiplayer support"
You: 🤖 PO TRIGGER: Feature request detected
     → Invoking Product Owner for evaluation
     → Context: Multiplayer feature request
     
[Trigger PO agent]
[Show PO's decision]
```

### Scenario 2: You Write Code
```
[After using Edit/Write on .cs file]
You: 🤖 MAINTAINER: Updating progress (code written)
     → Item: VS_000
     → Silent update: +40% progress
     
[Trigger maintainer]
[Show confirmation: "✓ VS_000: 20% → 60%"]
```

### Scenario 3: Bug Found
```
User: "There's a bug in the move logic"
You: 🤖 PO TRIGGER: Bug report detected
     → Invoking Product Owner for BF creation
     → Context: Move logic bug
     
[Trigger PO to create BF item]
```

---

## 📊 Progress Calculation

Backlog Maintainer uses these increments:
- Architecture tests: +10%
- Unit tests written: +15%
- Implementation: +40%
- Tests passing: +15%
- Integration tests: +15%
- Documentation: +5%

---

## 🐛 Quick Debugging

### Agent Not Working?
1. Check if registered: User runs `/agents` command
2. Verify workflow file exists
3. Ensure you're using correct agent name
4. Check context is complete

### Backlog Not Updating?
1. Verify Backlog.md path is correct
2. Check item ID exists
3. Ensure maintainer is triggered
4. Look for error messages

---

## 💡 Key Insights

1. **You are the orchestrator** - Agents don't trigger themselves
2. **Workflows are the truth** - Agents MUST read their workflow files
3. **Backlog.md is sacred** - It's the Single Source of Truth
4. **Transparency builds trust** - Always announce in early stage
5. **Feedback improves system** - Report issues immediately

---

## 🔄 Workflow Status

### What's Working
- ✅ Product Owner makes strategic decisions
- ✅ Backlog Maintainer tracks progress
- ✅ Both agents follow workflows
- ✅ Backlog stays synchronized

### What to Watch
- ⚠️ Remember to announce all triggers
- ⚠️ Ensure context is complete
- ⚠️ Check for feedback commands
- ⚠️ Keep Backlog.md as source of truth

---

## 📝 Session Handoff

When starting a new conversation, Claude Code should:

1. **Read this file first** for quick context
2. **Check AGENT_ORCHESTRATION_GUIDE.md** for details
3. **Review recent Backlog.md** changes
4. **Ask user**: "Should I continue with announcement mode for agent triggers?"
5. **Confirm agents**: Ask user to run `/agents` to verify registration

---

## 🎉 Success Metrics

The Dynamic PO Pattern is working when:
- No manual "update backlog" tasks needed
- All work items tracked automatically
- Progress updates happen silently
- Strategic decisions are documented
- User trusts the system

---

**Last Updated**: 2025-08-15
**PR**: #14 - Dynamic PO Pattern Implementation
**Status**: Production Ready

---

## Quick Test

To verify everything works, try:
```
User: "I want to add a save system"
```

Expected: You trigger PO, announce it, show decision, update backlog.

---

*This onboarding file ensures continuity across conversations. The Dynamic PO Pattern transforms manual tracking into automatic synchronization through intelligent agent orchestration.*