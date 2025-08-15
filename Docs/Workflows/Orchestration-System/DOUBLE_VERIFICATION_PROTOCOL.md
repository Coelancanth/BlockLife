# Double Verification Protocol for Agent Triggering

## Purpose
Ensure 100% compliance with automatic agent triggering during development workflows.

## 1. Self-Check Protocol (DURING Work)

### Immediate Trigger Checklist
- [ ] Did I edit a file? → Trigger backlog-maintainer
- [ ] Did I run tests? → Trigger backlog-maintainer
- [ ] Did I fix a bug? → Trigger backlog-maintainer
- [ ] Did I reorganize files? → Trigger backlog-maintainer
- [ ] Did I update documentation? → Trigger backlog-maintainer

### Verification Metrics
- **Trigger Completeness**: 100% of development actions
- **Trigger Timing**: Immediately after action
- **Trigger Type**: Match action to appropriate agent

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
- Correction Time: __ minutes

## 7. Continuous Improvement

### Quarterly Review
- Analyze trigger compliance metrics
- Identify systemic trigger gaps
- Refine orchestration patterns

## Enforcement
- ZERO tolerance for untracked work
- Mandatory real-time tracking
- Immediate correction of any missed triggers

**Last Updated**: 2025-08-15
**Version**: 1.0