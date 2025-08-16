# Product Owner Trigger Quick Reference Card

## 🚀 Instant Trigger Guide

### ALWAYS Trigger PO When:

#### 🎯 Feature Requests
```
"I want..." | "Can we add..." | "Feature request..." | "It would be nice if..."
→ ACTION: Evaluate and create VS item
```

#### 🐛 Bug Reports  
```
"Bug" | "Error" | "Broken" | "Doesn't work" | "Crashes"
→ ACTION: Create BF/HF item and prioritize
```

#### ✅ Work Complete
```
"Tests pass" | "Implementation done" | "Ready for review"
→ ACTION: Acceptance criteria validation
```

#### ❓ Priority Questions
```
"What's next?" | "Which first?" | "Should I do X or Y?"
→ ACTION: Strategic priority guidance
```

#### 🚫 Blockers
```
"Blocked by..." | "Can't continue..." | "Waiting on..."
→ ACTION: Unblock or reprioritize
```

---

## 🤔 Consider Triggering When:

#### Implicit Requests
```
"The system should..." | "Players need to..." | "It doesn't do X yet"
→ CHECK: Is this observation or request?
```

#### Scope Questions
```
"While I'm at it..." | "Should we also..." | "Easy to add..."
→ CHECK: Scope creep prevention needed?
```

#### Clarifications
```
"Not clear on..." | "How exactly..." | "AC doesn't specify..."
→ CHECK: Requirements ambiguity?
```

---

## ❌ DON'T Trigger PO For:

- Technical implementation questions → Tech Lead
- Code pattern decisions → Architect  
- Debugging assistance → Debugger Expert
- How-to questions → Documentation
- Status queries → Just answer

---

## 📊 Priority Order

If multiple triggers detected, process in this order:

1. 🔴 **Critical Bugs** (data loss, crashes)
2. 🟠 **Blockers** (work stopped)
3. 🟡 **Feature Requests** (explicit)
4. 🟢 **Work Completion** (acceptance)
5. 🔵 **Planning/Priority** (guidance)
6. ⚪ **Clarifications** (requirements)

---

## 🎯 Trigger Template

```python
print("🤖 PO TRIGGER: [Reason]")
print("   → Context: [What's being evaluated]")
Task(
    description="Product Owner [action]",
    prompt="""
    Read workflow: Docs/Workflows/product-owner-workflow.md
    Execute: [specific action]
    Context: [relevant context]
    """,
    subagent_type="product-owner"
)
```

---

## 📈 Health Check

Good session has:
- 3-5 PO triggers minimum
- Mix of request/complete/priority
- Timely trigger response

Warning signs:
- No triggers for 30+ min
- Only explicit triggers
- Delayed trigger response

---

**Full Documentation**: [PO_TRIGGER_POINTS.md](PO_TRIGGER_POINTS.md)