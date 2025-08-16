# Product Owner Trigger Points Documentation

## Purpose
This document provides comprehensive documentation of ALL Product Owner trigger points, patterns, and priority rules to ensure the Backlog remains the Single Source of Truth through automatic orchestration.

---

## 🎯 Core Principle
The Product Owner agent is triggered automatically at strategic decision points to:
1. **Evaluate** new requirements and changes
2. **Prioritize** work based on value and urgency
3. **Validate** completed work against acceptance criteria
4. **Maintain** backlog as the Single Source of Truth

---

## 📋 Complete Trigger Point Catalog

### 1. Feature Request Triggers

#### Direct Request Pattern
**Detection**: User explicitly asks for new functionality
```
Patterns:
- "I want to add..."
- "Can we implement..."
- "Feature request:..."
- "It would be nice if..."
- "The game should..."
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Feature request detected")
print("   → Context: Evaluating new feature idea")
Task(
    description="Evaluate feature request",
    prompt="""
    Execute: Feature Request evaluation
    Request: {user_request}
    Current priorities: {top_3_items}
    Perform value analysis and create VS if approved
    """,
    subagent_type="product-owner"
)
```

#### Implicit Request Pattern
**Detection**: User describes desired behavior without explicit request
```
Patterns:
- "When I click X, it should Y"
- "Players need to be able to..."
- "The system needs..."
- "It doesn't do X yet"
```

**Priority**: MEDIUM - Requires validation that it's a request

---

### 2. Bug Report Triggers

#### Explicit Bug Pattern
**Detection**: Clear bug or error report
```
Patterns:
- "There's a bug..."
- "Error when..."
- "It's broken..."
- "Crashes when..."
- "Exception thrown..."
- "Doesn't work..."
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Bug reported")
print("   → Context: Creating bug fix item")
Task(
    description="Create bug fix item",
    prompt="""
    Execute: Bug Report processing
    Issue: {bug_description}
    Severity assessment and BF item creation
    Determine if HF (critical) or BF (normal)
    """,
    subagent_type="product-owner"
)
```

#### Behavioral Issue Pattern
**Detection**: Unexpected behavior that may be bug or misunderstanding
```
Patterns:
- "Why does it..."
- "Shouldn't this..."
- "I expected..."
- "It's doing X instead of Y"
```

**Priority**: HIGH - May be critical issue

---

### 3. Work Completion Triggers

#### Tests Pass Pattern
**Detection**: Test execution completes successfully
```
Patterns:
- Bash command: "dotnet test" returns "Passed!"
- "All tests pass"
- "Tests are green"
- "Implementation complete"
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Ready for acceptance review")
print("   → Context: Validating against acceptance criteria")
Task(
    description="Acceptance review",
    prompt="""
    Execute: Acceptance Review
    Work item: {vs_id}
    Test results: {test_summary}
    Validate all acceptance criteria met
    """,
    subagent_type="product-owner"
)
```

#### Phase Complete Pattern
**Detection**: Development phase completion
```
Patterns:
- "Phase X complete"
- "Finished implementing..."
- "Done with..."
- Edit/Write to implementation files followed by success
```

**Priority**: MEDIUM - May need acceptance review

---

### 4. Planning & Priority Triggers

#### Priority Question Pattern
**Detection**: Questions about work order or importance
```
Patterns:
- "What should I work on?"
- "Which is more important?"
- "Should I do X or Y first?"
- "What's the priority?"
- "What's next?"
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Priority decision needed")
print("   → Context: Strategic guidance requested")
Task(
    description="Priority guidance",
    prompt="""
    Execute: Priority decision
    Options: {choices}
    Current backlog state: {backlog_summary}
    Provide clear priority order with reasoning
    """,
    subagent_type="product-owner"
)
```

#### Session Planning Pattern
**Detection**: Beginning of work session or planning needs
```
Patterns:
- Session start (first interaction)
- "Plan for today"
- "Weekly goals"
- "Sprint planning"
- After long gap (>4 hours)
```

**Priority**: HIGH - Sets session direction

---

### 5. Value Challenge Triggers

#### Scope Creep Pattern
**Detection**: Feature additions during implementation
```
Patterns:
- "While I'm at it, should I also..."
- "It would be easy to add..."
- "Should we include..."
- "What about also doing..."
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Scope validation needed")
print("   → Context: Evaluating scope addition")
Task(
    description="Scope challenge",
    prompt="""
    Execute: Value Challenge
    Proposed addition: {scope_item}
    Current work: {active_item}
    Apply YAGNI principle and assess value
    """,
    subagent_type="product-owner"
)
```

#### Over-Engineering Pattern
**Detection**: Complex solutions to simple problems
```
Patterns:
- "Framework for..."
- "Generalized solution..."
- "Future-proof..."
- "Extensible system for..."
```

**Priority**: HIGH - Prevent complexity

---

### 6. Blocker & Risk Triggers

#### Blocked Work Pattern
**Detection**: Work cannot proceed
```
Patterns:
- "Blocked by..."
- "Can't continue until..."
- "Waiting on..."
- "Depends on..."
- "Need X before Y"
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Blocker reported")
print("   → Context: Assessing impact and alternatives")
Task(
    description="Blocker assessment",
    prompt="""
    Execute: Blocker analysis
    Blocked item: {item}
    Blocker: {dependency}
    Find alternatives or reprioritize
    """,
    subagent_type="product-owner"
)
```

#### Risk Identification Pattern
**Detection**: Potential issues identified
```
Patterns:
- "This might break..."
- "Risk of..."
- "Could cause..."
- "Potential issue..."
- "Concerned about..."
```

**Priority**: HIGH - Prevent future problems

---

### 7. Clarification Triggers

#### Requirement Ambiguity Pattern
**Detection**: Unclear requirements
```
Patterns:
- "What exactly should..."
- "Not clear on..."
- "How should this work?"
- "User story doesn't specify..."
- "Acceptance criteria unclear"
```

**Trigger Action**:
```python
print("🤖 PO TRIGGER: Requirement clarification needed")
print("   → Context: Resolving ambiguity")
Task(
    description="Requirement clarification",
    prompt="""
    Execute: Clarify requirements
    Ambiguity: {unclear_point}
    Work item: {item_id}
    Provide specific guidance
    """,
    subagent_type="product-owner"
)
```

---

## 🎯 Priority Rules for Triggers

### Immediate Triggers (MUST trigger within same response)
1. **Critical Bugs** - Data loss, crashes, security issues
2. **Explicit Feature Requests** - Direct user asks
3. **Work Completion** - Tests pass, ready for review
4. **Blockers** - Work cannot proceed

### Deferred Triggers (Can batch at end of response)
1. **Planning Requests** - Session goals, priorities
2. **Value Challenges** - Scope creep, over-engineering
3. **Minor Clarifications** - Non-blocking questions

### Optional Triggers (Trigger if uncertain)
1. **Implicit Requests** - May be observation or request
2. **Performance Concerns** - May or may not need action
3. **Technical Debt** - May be acceptable temporarily

---

## 📊 Edge Cases and Exceptions

### When NOT to Trigger

#### Information Only
```
Don't trigger for:
- "How does X work?" (explanation, not change)
- "What is the current status?" (query only)
- Code reading without changes
- Documentation reading
```

#### Technical Decisions
```
Don't trigger for:
- "Which pattern should I use?" (Tech Lead domain)
- "How to implement?" (Dev Engineer domain)
- "Best practice for?" (Architect domain)
```

### Special Cases

#### Multiple Triggers in One Message
**Rule**: Process in priority order
```
Example: "There's a bug [1] and I want to add feature X [2]"
Order:
1. Bug report trigger (higher priority)
2. Feature request trigger
```

#### Conflicting Signals
**Rule**: Err on side of triggering
```
Example: "This might be a bug or maybe intended behavior"
Action: Trigger PO for clarification
```

#### Batch Operations
**Rule**: Single trigger with consolidated context
```
Example: Multiple test files edited
Action: One trigger with all changes summarized
```

---

## 🔄 Integration with Other Agents

### PO Triggers Other Agents

| After PO Action | Triggers | Purpose |
|-----------------|----------|---------|
| Creates VS item | Tech Lead | Create implementation plan |
| Approves feature | Backlog Maintainer | Update backlog status |
| Rejects feature | None | Decision recorded only |
| Creates BF/HF | Debugger Expert | If complex issue |
| Accepts work | Backlog Maintainer | Archive completed item |

### Other Agents Trigger PO

| Agent | Triggers PO When | Context Provided |
|-------|------------------|------------------|
| Tech Lead | Needs requirement clarification | Ambiguous acceptance criteria |
| QA Engineer | Finds acceptance criteria gap | Missing test scenarios |
| Debugger Expert | Discovers design flaw | Bug is actually missing feature |
| Dev Engineer | Scope creep during implementation | Additional functionality question |

---

## 📝 Quick Reference Guide

### Most Common Triggers (90% of cases)

1. **"I want..."** → Feature Request
2. **"Bug/Error/Broken"** → Bug Report  
3. **"Tests pass"** → Acceptance Review
4. **"What's next?"** → Priority Guidance
5. **"Should I..."** → Value Challenge

### Trigger Confidence Rules

**HIGH Confidence** (Always trigger):
- Explicit keywords present
- User directly asks
- Tests complete
- Blocked work

**MEDIUM Confidence** (Usually trigger):
- Implicit patterns
- Ambiguous language
- Planning needs

**LOW Confidence** (Consider context):
- Technical questions
- Status queries
- Information requests

---

## 🚨 Trigger Validation Checklist

Before triggering, verify:
- [ ] Is this a Product Owner responsibility?
- [ ] Is user asking for decision/action?
- [ ] Would this change the backlog?
- [ ] Is value assessment needed?
- [ ] Are priorities affected?

If ANY are "yes" → TRIGGER

---

## 📊 Metrics and Monitoring

### Expected Trigger Frequency
- **Per Session**: 3-5 triggers minimum
- **Per Feature**: 2-3 triggers (request, completion, acceptance)
- **Per Bug**: 1-2 triggers (report, verification)

### Trigger Health Indicators
- **Good**: Regular triggers throughout session
- **Warning**: No triggers for 30+ minutes of active work
- **Bad**: Only triggering on explicit requests

---

## 🔧 Troubleshooting

### Common Issues

#### "PO should have been triggered but wasn't"
1. Check if pattern is in this document
2. Add pattern if missing
3. Report via feedback system

#### "PO triggered unnecessarily"
1. Review trigger pattern
2. Check if technical-only decision
3. Refine pattern matching

#### "Wrong context provided to PO"
1. Ensure complete context gathering
2. Include all relevant files/states
3. Provide clear action needed

---

## 📚 Related Documentation

- [AGENT_ORCHESTRATION_GUIDE.md](AGENT_ORCHESTRATION_GUIDE.md) - Overall orchestration patterns
- [product-owner-workflow.md](../Agent-Workflows/product-owner-workflow.md) - PO workflow details
- [DOUBLE_VERIFICATION_PROTOCOL.md](DOUBLE_VERIFICATION_PROTOCOL.md) - Trigger verification
- [ORCHESTRATION_FEEDBACK_SYSTEM.md](ORCHESTRATION_FEEDBACK_SYSTEM.md) - Report issues

---

## Version History

- **v1.0** (2025-08-16): Complete trigger point documentation for TD_015
- Created comprehensive catalog of all PO trigger patterns
- Defined priority rules and edge cases
- Integrated with orchestration system
- Added quick reference guide and metrics