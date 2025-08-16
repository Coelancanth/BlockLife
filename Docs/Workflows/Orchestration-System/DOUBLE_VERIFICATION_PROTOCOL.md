# Double Verification Protocol for Agent Triggering

## Purpose
Ensure 100% compliance with automatic agent triggering during development workflows and verify agents actually complete their reported actions.

## 1. Self-Check Protocol (DURING Work)

### Immediate Trigger Checklist
- [ ] Did I edit a file? → Trigger backlog-maintainer
- [ ] Did I run tests? → Trigger backlog-maintainer
- [ ] Did I fix a bug? → Trigger backlog-maintainer
- [ ] Did I reorganize files? → Trigger backlog-maintainer
- [ ] Did I update documentation? → Trigger backlog-maintainer
- [ ] Did I create new work items? → Trigger backlog-maintainer
- [ ] Did I complete work items? → Trigger backlog-maintainer for archival

### Verification Metrics
- **Trigger Completeness**: 100% of development actions
- **Trigger Timing**: Immediately after action
- **Trigger Type**: Match action to appropriate agent
- **Action Verification**: Confirm agent actually performed reported actions
- **False Success Detection**: Catch agents reporting success without completing work

## 2. Session Review Protocol (END of Work)

### Post-Session Verification Checklist
- [ ] Review all actions taken during session
- [ ] Confirm each action triggered appropriate agent
- [ ] Check Backlog.md for complete tracking
- [ ] Validate no missed triggers

## 3. User Verification Points

### Mandatory User Checks
1. Confirm all changes reflected in Backlog.md
2. Verify agent-triggered annotations present
3. Check for no missing work items

## 4. Red Flags (IMMEDIATE Correction)

### Critical Trigger Failures
- No backlog-maintainer trigger after edits
- Missed agent delegation
- Incomplete work item tracking
- Undocumented changes
- **Agent reports success but work not done** (e.g., BF_003 archive failure)
- **Agent uses wrong paths or configurations**
- **Silent failures with no error reporting**

### Emergency Protocol
1. STOP current work
2. Manually document ALL missed triggers
3. Update Backlog.md comprehensively
4. Trigger all missed agents sequentially

## 5. Quick Reference Trigger Table

| Action Type | Primary Trigger Agent | Secondary Verification |
|------------|----------------------|------------------------|
| File Edit | backlog-maintainer | product-owner |
| Test Run | backlog-maintainer | test-designer |
| Bug Fix | backlog-maintainer | debugger-expert |
| Feature Progress | backlog-maintainer | tech-lead |
| Code Reorganization | backlog-maintainer | vsa-refactoring |
| Architecture Change | tech-lead | architect |

## 6. Verification Metrics Tracking

### Daily Trigger Health Dashboard
- Total Actions Taken: __
- Actions Tracked: __
- Trigger Compliance Rate: __%
- Missed Triggers: __
- False Success Reports: __
- Verification Failures: __
- Correction Time: __ minutes

### Agent Reliability Metrics
- Agent Success Rate: __%
- False Positive Rate: __%
- Silent Failure Count: __
- Path/Config Errors: __

## 7. Continuous Improvement

### Quarterly Review
- Analyze trigger compliance metrics
- Identify systemic trigger gaps
- Refine orchestration patterns

## Enforcement
- ZERO tolerance for untracked work
- Mandatory real-time tracking
- Immediate correction of any missed triggers

## 8. Agent Output Verification Protocol

### Mandatory Verification Steps
1. **File Operations**: Always verify file moves/creates/deletes actually happened
2. **Status Updates**: Check that status changes are reflected in tracking files
3. **Archive Operations**: Confirm files exist in destination and are removed from source
4. **Backlog Updates**: Verify Backlog.md accurately reflects reported changes

### Verification Commands
```powershell
# Verify file exists
Test-Path "path/to/file"

# Check archive contents
Get-ChildItem "Docs/Backlog/archive/completed/2025-Q1/"

# Verify file moved (not copied)
-not (Test-Path "old/path") -and (Test-Path "new/path")
```

### Agent Trust Levels
- **High Trust**: Agents with consistent success history
- **Verify Always**: Agents with history of false reports
- **Critical Verify**: Operations affecting multiple files or critical data

## 9. Incident Response Protocol

### When Agent Fails Verification
1. **Document the failure** as a bug (BF_XXX)
2. **Identify root cause** (path issues, logic errors, missing checks)
3. **Fix the agent workflow** immediately
4. **Retry the operation** with verification
5. **Update trust level** for that agent

### Learning from Failures
- Every agent failure becomes a test case
- Add verification step to prevent recurrence
- Update this protocol with new failure patterns
- Share learnings in bug post-mortem

**Last Updated**: 2025-08-16
**Version**: 2.0
**Major Update**: Added agent output verification and incident response protocols based on BF_003 learnings