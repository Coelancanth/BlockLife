# Orchestration Feedback System

## Purpose
Track and improve Claude Code's orchestration performance by capturing when triggers are missed, wrong agents are used, or workflows aren't followed.

---

## 🐛 Bug Report Templates

### Missed Trigger Report
```markdown
## MISSED TRIGGER: [Date/Time]
**What I said**: "[exact user message]"
**Expected trigger**: [Agent] for [Action]
**What happened**: No agent triggered / Wrong response
**Impact**: [What didn't get updated/tracked]
**Pattern to add**: [Detection pattern that would catch this]
```

### Wrong Agent Report
```markdown
## WRONG AGENT: [Date/Time]
**Scenario**: [What was happening]
**Agent triggered**: [Which agent]
**Should have been**: [Correct agent]
**Reason**: [Why it was wrong]
**Fix needed**: [How to correct detection]
```

### Failed Workflow Report
```markdown
## WORKFLOW FAILURE: [Date/Time]
**Agent**: [Which agent]
**Expected behavior**: [What should happen per workflow]
**Actual behavior**: [What actually happened]
**Workflow step missed**: [Specific step]
**Correction needed**: [How to fix]
```

---

## 📊 Feedback Categories

### 1. Trigger Detection Issues
- **False Negative**: Should have triggered but didn't
- **False Positive**: Triggered unnecessarily
- **Wrong Pattern Match**: Detected wrong intent
- **Ambiguous Language**: Unclear what user wanted

### 2. Agent Selection Issues
- **Wrong Agent**: Used A when should use B
- **Missing Agent**: No suitable agent for task
- **Capability Mismatch**: Agent can't do what's needed
- **Context Insufficient**: Not enough info provided

### 3. Workflow Execution Issues
- **Skipped Steps**: Didn't follow workflow completely
- **Wrong Order**: Steps done out of sequence
- **Missing Output**: Didn't create expected artifacts
- **Silent vs Visible**: Wrong visibility level

### 4. Announcement Issues
- **No Announcement**: Forgot to announce trigger
- **Unclear Announcement**: Didn't explain what/why
- **Wrong Format**: Didn't follow announcement pattern
- **Missing Context**: Announcement lacked detail

---

## 🔄 Feedback Commands

Use these commands to report issues quickly:

### Quick Reports
- **"Missed trigger!"** - I should have triggered an agent but didn't
- **"Wrong agent!"** - I used the wrong agent for the task
- **"No announcement!"** - I triggered without announcing
- **"Workflow broken!"** - Agent didn't follow its workflow

### Detailed Reports
- **"Bug: [description]"** - Detailed issue report
- **"Feedback: [observation]"** - General improvement suggestion
- **"Pattern failed: [example]"** - Detection pattern didn't work
- **"Add trigger: [pattern]"** - Suggest new trigger pattern

---

## 📈 Performance Metrics

Track orchestration quality over time:

### Daily Metrics
```markdown
## Orchestration Performance [Date]
- Successful triggers: X/Y (%)
- Correct agent selection: X/Y (%)
- Workflow compliance: X/Y (%)
- Announcement compliance: X/Y (%)

### Issues Found
- Missed triggers: [count]
- Wrong agents: [count]
- Workflow failures: [count]
- No announcements: [count]

### Patterns to Add/Fix
1. [Pattern that needs adding]
2. [Pattern that needs fixing]
```

---

## 🎯 Common Issues & Solutions

### Issue: "I said 'add feature' but no PO triggered"
**Detection Gap**: Variation of feature request language
**Solution**: Add pattern variation to detection
**Update**: Add "add feature" to feature detection patterns

### Issue: "Edited code but no progress update"
**Detection Gap**: Tool usage not tracked
**Solution**: Track Edit/Write tool usage better
**Update**: Check file extensions after edits

### Issue: "PO created story but didn't update tracker"
**Workflow Gap**: Missing backlog update step
**Solution**: Chain maintainer after PO creates items
**Update**: Add automatic maintainer trigger after PO

### Issue: "No announcement when agent triggered"
**Compliance Gap**: Forgot transparency mode
**Solution**: Add announcement to trigger function
**Update**: Make announcement mandatory in early stage

---

## 📝 Feedback Log

Keep a running log of issues and improvements:

### 2025-08-15 Session
- ✅ **Fixed**: Added mandatory orchestration guide reading
- ✅ **Fixed**: Created announcement patterns
- 🔄 **Pending**: Test in real development
- 📝 **Note**: User concerned about follow-through

### Future Sessions
- [ ] **Issue**: [Description]
- [ ] **Fix**: [What was done]
- [ ] **Result**: [Did it work?]

---

## 🔧 Continuous Improvement Process

### 1. Capture Issue
- User reports via feedback command
- Log the specific failure
- Identify pattern gap

### 2. Analyze Root Cause
- Why did it fail?
- What pattern was missing?
- What assumption was wrong?

### 3. Update Documentation
- Add new pattern to orchestration guide
- Update CLAUDE.md if critical
- Document in feedback log

### 4. Test Fix
- Try the scenario again
- Verify fix works
- Check for side effects

### 5. Monitor
- Track if issue recurs
- Measure improvement
- Iterate as needed

---

## 🚨 Critical Feedback

If orchestration completely breaks:

### Emergency Commands
- **"STOP AGENTS"** - Disable all agent triggers
- **"MANUAL MODE"** - Revert to manual updates
- **"DEBUG ORCHESTRATION"** - Show all detection logic
- **"RESET WORKFLOW"** - Clear agent state

### Escalation Path
1. Try quick feedback command
2. If not fixed, detailed bug report
3. If critical, use emergency commands
4. Document for post-mortem

---

## 📊 Success Metrics

### Target Performance (After Training)
- **Trigger Accuracy**: >90%
- **Agent Selection**: >95%
- **Workflow Compliance**: >90%
- **Announcement Rate**: 100% (in early stage)

### Acceptable Performance (Learning Phase)
- **Trigger Accuracy**: >70%
- **Agent Selection**: >80%
- **Workflow Compliance**: >70%
- **Announcement Rate**: >90%

### Red Flags (Need Immediate Fix)
- **Trigger Accuracy**: <50%
- **Agent Selection**: <70%
- **Workflow Compliance**: <50%
- **Announcement Rate**: <80%

---

## 🎓 Learning From Feedback

Each feedback item should result in:
1. **Immediate Fix**: Correct the specific issue
2. **Pattern Update**: Improve detection/workflow
3. **Documentation**: Update guides
4. **Test Case**: Prevent regression
5. **Lesson Learned**: Share insight

---

## 💡 Feedback Best Practices

### For Users
- Report immediately when issue occurs
- Provide exact wording/context
- Suggest what should have happened
- Use quick commands for speed

### For Claude Code
- Acknowledge feedback immediately
- Log the issue properly
- Implement fix if possible
- Test the fix
- Confirm resolution

---

## 📈 Maturity Model

### Phase 1: Learning (CURRENT)
- High feedback rate expected
- Many missed triggers
- Frequent corrections needed
- Heavy user guidance

### Phase 2: Improving
- Feedback rate decreasing
- Most triggers caught
- Occasional corrections
- Light user guidance

### Phase 3: Mature
- Minimal feedback needed
- Reliable triggers
- Rare corrections
- Autonomous operation

---

## 🔗 Integration Points

This feedback system integrates with:
- **AGENT_ORCHESTRATION_GUIDE.md** - Update patterns
- **CLAUDE.md** - Critical updates
- **Backlog** - Track improvement items
- **Workflows** - Refine procedures

---

**Remember**: Every piece of feedback makes the system better. Don't hesitate to report issues - that's how we improve!