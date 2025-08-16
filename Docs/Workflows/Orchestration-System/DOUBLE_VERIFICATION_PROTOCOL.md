# Double Verification Protocol for Agent Triggering

## Purpose
Ensure 100% compliance with the current orchestration workflow: trigger appropriate agents, verify their outputs, and update the backlog directly with verified results. Critical for catching false agent success reports.

## 1. Real-Time Workflow Protocol (DURING Work)

### Immediate Action Checklist
- [ ] Did I edit a file? → Update backlog directly with progress
- [ ] Did I run tests? → Update backlog with test results
- [ ] Did I fix a bug? → Update backlog with fix status
- [ ] Did I reorganize files? → Update backlog with changes
- [ ] Did I update documentation? → Update backlog with doc changes
- [ ] Did I trigger an agent? → Verify output then update backlog
- [ ] Did I complete work items? → Trigger product-owner for archival decision

### Verification Metrics
- **Action Coverage**: 100% of development actions tracked in backlog
- **Update Timing**: Immediately after action or agent interaction
- **Agent Verification**: Confirm agents actually performed reported actions
- **Backlog Accuracy**: Verify backlog reflects actual system state
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

### Critical Workflow Failures
- No backlog update after actions or agent interactions
- Missed agent delegation for specialist tasks
- Incomplete work item tracking
- Undocumented changes or progress
- **Agent reports success but work not done** (e.g., BF_003 archive failure)
- **Agent uses wrong paths or configurations**
- **Silent failures with no error reporting**
- **Backlog state doesn't match actual system state**

### Emergency Protocol
1. STOP current work
2. Manually document ALL missed actions and agent interactions
3. Update Backlog.md comprehensively with current state
4. Verify all agent outputs claimed to be complete
5. Trigger any missed agent delegations sequentially

## 5. Quick Reference Action Table

| Action Type | Agent to Trigger | Claude Code Follow-up |
|------------|------------------|----------------------|
| User Request | product-owner | Update backlog with work items created |
| Technical Planning | tech-lead | Update backlog with implementation plan |
| Test Creation | test-designer | Update backlog with test progress |
| Code Implementation | dev-engineer | Update backlog with implementation progress |
| Bug Analysis | debugger-expert | Update backlog with findings and fixes |
| Code Duplication | vsa-refactoring | Update backlog with refactoring results |
| Git Operations | git-expert | Update backlog with commit/PR status |
| Build/Automation | devops-engineer | Update backlog with automation results |
| Quality Assurance | qa-engineer | Update backlog with test results |
| Architecture Decisions | architect | Update backlog with decision outcomes |

## 6. Integration with Current Orchestration Workflow

### CLAUDE.md Workflow Integration
This verification protocol integrates with CLAUDE.md's 7-step orchestration workflow:

1. **Listen** - Understand user needs
2. **Map** - Identify specialist agents needed
3. **Delegate** - Trigger appropriate agents
4. **Verify** - ⭐ **THIS PROTOCOL** - Confirm agent completed work
5. **Update** - Record progress/results in backlog directly
6. **Coordinate** - Manage multi-agent workflows
7. **Synthesize** - Present unified results to user

### Verification Happens at Steps 4-5
- **Step 4 (Verify)**: Use this protocol to confirm agents did what they claimed
- **Step 5 (Update)**: Claude Code updates backlog directly with verified results
- **Critical**: Never skip verification - agents can report false success

## 7. Verification Metrics Tracking

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

## 8. Continuous Improvement

### Quarterly Review
- Analyze trigger compliance metrics
- Identify systemic trigger gaps
- Refine orchestration patterns

## Enforcement
- ZERO tolerance for untracked work
- Mandatory real-time tracking
- Immediate correction of any missed triggers

## 9. Agent Output Verification Protocol

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

## 10. Incident Response Protocol

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
**Version**: 3.0
**Major Update**: Removed deprecated backlog-maintainer references, aligned with 10-agent ecosystem, integrated with CLAUDE.md orchestration workflow