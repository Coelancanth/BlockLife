# Agent Orchestration Guide

## Purpose
This document defines WHEN and HOW Claude Code triggers specialized agents to maintain the Dynamic PO Pattern.

---

## 🎯 Core Principle
Claude Code (main agent) is the orchestrator who:
1. **Detects** trigger conditions
2. **Announces** agent invocations (during early stage)
3. **Triggers** appropriate agents
4. **Synthesizes** responses

---

## 📢 Transparency Mode (Early Stage)
During workflow development, Claude Code will announce all agent triggers:

```
🤖 AGENT TRIGGER: Detected feature request
   → Invoking Product Owner for evaluation
   → Context: [what's being evaluated]
```

This helps validate the pattern is working correctly.

---

## 🔄 Agent Trigger Map

### Product Owner Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Feature Request** | "I want", "add", "feature", "can we" | "🤖 PO TRIGGER: Feature request detected" | Evaluate and create user story |
| **Bug Report** | "error", "bug", "broken", "doesn't work" | "🤖 PO TRIGGER: Bug reported" | Create BF item and prioritize |
| **Work Complete** | Tests pass + implementation done | "🤖 PO TRIGGER: Ready for acceptance" | Review acceptance criteria |
| **Priority Question** | "what should", "which first", "priority" | "🤖 PO TRIGGER: Priority decision needed" | Provide strategic guidance |
| **Session Start** | Beginning work session | "🤖 PO TRIGGER: Session planning" | Set session goals |

### Backlog Maintainer Triggers (Silent but Announced)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Code Written** | After Edit/Write on .cs/.py/.js | "🤖 MAINTAINER: Updating progress (code)" | +40% progress |
| **Tests Written** | After Edit/Write on test files | "🤖 MAINTAINER: Updating progress (tests)" | +15% progress |
| **Tests Pass** | "dotnet test" with "Passed!" | "🤖 MAINTAINER: Updating progress (tests pass)" | +15% progress |
| **PR Created** | "gh pr create" command | "🤖 MAINTAINER: Changing status (PR)" | Status → In Review |
| **PR Merged** | "gh pr merge" command | "🤖 MAINTAINER: Archiving completed item" | Move to archive |

---

## 🎮 Manual Override Commands

If automatic triggering misses something, you can request:

- **"Trigger PO for [action]"** - Manually invoke Product Owner
- **"Update backlog progress"** - Manually trigger maintainer
- **"Check backlog status"** - Force synchronization
- **"Show PO decision"** - Get strategic input

## 🐛 Feedback Commands

Report orchestration issues immediately:

- **"Missed trigger!"** - Should have triggered but didn't
- **"Wrong agent!"** - Used wrong agent for task
- **"No announcement!"** - Forgot to announce
- **"Workflow broken!"** - Didn't follow workflow
- **"Bug: [details]"** - Detailed issue report

See [ORCHESTRATION_FEEDBACK_SYSTEM.md](ORCHESTRATION_FEEDBACK_SYSTEM.md) for complete feedback guide.

---

## 📋 Standard Invocation Patterns

### Visible Product Owner Pattern
```python
# Claude Code announces and triggers
print("🤖 PO TRIGGER: [Reason]")
print("   → Context: [What's being evaluated]")

response = Task(
    description="Product Owner evaluation",
    prompt=f"""
    Read workflow: Docs/Workflows/product-owner-workflow.md
    Execute: {action}
    Context: {context}
    """,
    subagent_type="agile-product-owner"
)

print("📊 PO DECISION:")
print(synthesize(response))
```

### Silent Maintainer Pattern
```python
# Claude Code announces but output is minimal
print("🤖 MAINTAINER: [Silent update type]")

Task(
    description="Update backlog",
    prompt=f"""
    Read workflow: Docs/Workflows/backlog-maintainer-workflow.md
    Execute: {action}
    Context: {context}
    Silent operation - return only confirmation
    """,
    subagent_type="backlog-maintainer"
)

# No output shown unless error
```

---

## 🔍 Trigger Detection Logic

### Phase 1: Message Analysis
```python
# Runs on every user message
analyze_for_triggers(user_message):
    - Check for feature language
    - Check for bug language  
    - Check for priority questions
    - Check for completion signals
```

### Phase 2: Tool Usage Analysis
```python
# Runs after every tool use
analyze_tool_usage(tool, params, result):
    - If Edit/Write → check file type
    - If Bash → check command type
    - If tests run → check results
```

### Phase 3: Context Analysis
```python
# Runs periodically
analyze_context():
    - Check session duration
    - Check WIP items
    - Check blocked items
```

---

## 📊 Progress Calculation Rules

### Backlog Maintainer Progress Increments
| Event | Progress | Notes |
|-------|----------|-------|
| Architecture tests written | +10% | First step in TDD |
| Unit tests written | +15% | Red phase complete |
| Implementation complete | +40% | Green phase complete |
| Tests passing | +15% | Validation complete |
| Integration tests | +15% | End-to-end verified |
| Documentation | +5% | Final step |

**Total**: 100% across full TDD cycle

---

## 🚨 Trigger Validation Checklist

During early stage, validate each trigger:

- [ ] Was the trigger condition correctly detected?
- [ ] Was the announcement clear and timely?
- [ ] Did the right agent get invoked?
- [ ] Was the context sufficient?
- [ ] Did the agent follow its workflow?
- [ ] Was the result properly integrated?

---

## 📈 Maturity Stages

### Stage 1: Manual with Announcements (CURRENT)
- Every trigger is announced
- Manual validation required
- Learning patterns

### Stage 2: Automatic with Confirmations
- Triggers happen automatically
- Brief confirmations shown
- Occasional validation

### Stage 3: Fully Automatic (FUTURE)
- Silent operation
- Only errors shown
- Complete automation

---

## 🔧 Troubleshooting

### Agent Not Triggering
1. Check if pattern matches detection rules
2. Verify agent is registered (`/agents` command)
3. Check context is complete
4. Try manual trigger command

### Wrong Agent Triggered
1. Review detection patterns
2. Check for ambiguous language
3. Use explicit trigger commands

### Agent Fails
1. Check workflow file exists
2. Verify context provided
3. Check file paths are correct
4. Review agent capabilities

---

## 📝 Notes

- This is a living document that will evolve as we refine triggers
- Announcement mode helps us validate and improve patterns
- Goal is to reach Stage 3 (fully automatic) once patterns are proven
- Feedback on missed triggers helps improve detection