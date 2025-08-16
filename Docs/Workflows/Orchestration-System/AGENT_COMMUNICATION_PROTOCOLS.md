# Agent Communication Protocols

## Purpose
This document defines HOW agents communicate with each other and coordinate work in the BlockLife agent ecosystem.

---

## 🎯 Core Communication Principles

### 1. Hub-and-Spoke Model
- **Claude Code** is the central orchestrator (hub)
- All agents communicate through Claude Code (spokes)
- No direct agent-to-agent communication
- Ensures single point of control and visibility

### 2. Context Passing Protocol
- Each agent receives complete context from Claude Code
- Context includes: current state, previous actions, expected outcomes
- Agents return structured responses for synthesis
- Claude Code maintains context continuity

### 3. Async Operation Model
- Agents operate independently when triggered
- No blocking dependencies between agents
- Results are synthesized asynchronously
- Work continues even if one agent is slow

---

## 📡 Communication Patterns

### Pattern 1: Sequential Hand-off
**Use When**: One agent's output feeds directly into another's input

```
User Request → Claude Code
    ↓
Claude Code → Product Owner (evaluate)
    ↓
PO Response → Claude Code
    ↓
Claude Code → Tech Lead (plan)
    ↓
TL Response → Claude Code
    ↓
Claude Code → User (synthesized result)
```

**Example**: Feature request → PO creates VS → TL adds implementation plan

### Pattern 2: Parallel Consultation
**Use When**: Multiple perspectives needed simultaneously

```
User Request → Claude Code
    ↓
Claude Code → [Agent A] [Agent B] [Agent C] (parallel)
    ↓
All Responses → Claude Code
    ↓
Claude Code → User (synthesized result)
```

**Example**: Complex bug → Debugger + Architect + QA analyze simultaneously

### Pattern 3: Triggered Cascade
**Use When**: One action automatically triggers follow-up actions

```
Development Action → Claude Code
    ↓
Claude Code → Backlog Maintainer (silent update)
    ↓
Status Change → Claude Code
    ↓
Claude Code → Product Owner (if milestone reached)
    ↓
PO Decision → Claude Code
    ↓
Claude Code → Next Action
```

**Example**: Tests pass → Backlog updates → PO reviews acceptance → Next phase

---

## 🔄 Integration Points Between Agents

### Product Owner ↔ Tech Lead
**Integration**: VS item handoff
- PO creates VS with acceptance criteria
- TL adds technical implementation plan
- Both update same VS file sequentially
- Backlog Maintainer tracks progress

### Tech Lead ↔ Test Designer
**Integration**: TDD cycle initiation
- TL defines technical approach
- Test Designer creates tests from TL's plan
- Context includes architecture decisions
- Tests align with technical phases

### Test Designer ↔ Dev Engineer
**Integration**: RED-GREEN cycle
- Test Designer creates failing test
- Dev Engineer receives test context
- Implementation follows test requirements
- Both reference same test file

### Dev Engineer ↔ QA Engineer
**Integration**: Quality validation
- Dev Engineer completes implementation
- QA Engineer receives code location
- Integration tests verify full stack
- Performance benchmarks validate quality

### Debugger Expert ↔ QA Engineer
**Integration**: Bug reproduction
- Debugger diagnoses root cause
- QA creates regression test
- Both document in bug report
- Test prevents recurrence

### Architect ↔ VSA Refactoring
**Integration**: Pattern extraction
- Architect defines system patterns
- VSA Refactoring implements extraction
- Both maintain architectural integrity
- Patterns documented in ADRs

### Git Expert ↔ DevOps Engineer
**Integration**: Release automation
- Git Expert manages branching
- DevOps automates release pipeline
- Both coordinate on CI/CD
- Releases follow Git flow

---

## 📋 Standard Message Formats

### Request Format (Claude → Agent)
```yaml
agent: [agent-name]
action: [specific-action]
context:
  current_state: [where we are]
  previous_actions: [what happened before]
  files_involved: [relevant paths]
  constraints: [boundaries/limitations]
expected_output: [what to produce]
workflow_step: [which workflow step to execute]
```

### Response Format (Agent → Claude)
```yaml
agent: [agent-name]
action_completed: [what was done]
results:
  files_modified: [paths]
  decisions_made: [list]
  recommendations: [next steps]
  blockers: [if any]
confidence: [high/medium/low]
time_estimate: [if applicable]
follow_up_needed: [yes/no + details]
```

---

## 🔀 Context Propagation Rules

### Rule 1: Preserve Critical Context
- Work item ID always propagates
- Session goals remain visible
- Error states carry forward
- Test results persist

### Rule 2: Filter Noise
- Remove intermediate outputs
- Summarize verbose logs
- Extract key decisions only
- Focus on actionable items

### Rule 3: Maintain Traceability
- Link all changes to work items
- Reference previous decisions
- Document rationale
- Create audit trail

---

## 🚦 Coordination Protocols

### Protocol 1: Work Item State Sync
```
1. Any agent modifies work item
2. Claude Code detects change
3. Backlog Maintainer updates tracker
4. Status reflects in Backlog.md
5. Next agent sees current state
```

### Protocol 2: Test Result Propagation
```
1. Test execution completes
2. Results analyzed by Claude Code
3. Backlog Maintainer updates progress
4. QA Engineer triggered if passing
5. Debugger triggered if failing
```

### Protocol 3: Decision Escalation
```
1. Agent encounters decision point
2. Returns "needs_decision" flag
3. Claude Code triggers appropriate decider
4. Decision propagates to requesting agent
5. Work continues with decision context
```

---

## 🎭 Agent Role Boundaries

### Clear Separations
| Agent | Owns | Does NOT Own |
|-------|------|--------------|
| Product Owner | Business value, prioritization | Technical implementation |
| Tech Lead | Technical approach, estimation | Business requirements |
| Architect | System patterns, boundaries | Feature-specific code |
| Test Designer | Test structure, assertions | Implementation code |
| Dev Engineer | Implementation code | Test design |
| QA Engineer | Integration tests, stress tests | Unit tests |
| Debugger Expert | Root cause analysis | Preventive architecture |
| VSA Refactoring | Code extraction, patterns | Business logic changes |
| Git Expert | Version control, branching | Code content |
| DevOps Engineer | Automation, CI/CD | Application code |
| Backlog Maintainer | Status tracking, progress | Decision making |

### Collaboration Points
- Agents collaborate at boundaries
- Hand-offs are explicit and documented
- Context includes boundary information
- No agent modifies another's domain

---

## 📊 Communication Metrics

### Tracking Success
- **Response Time**: How quickly agents respond
- **Context Quality**: Completeness of passed context
- **Hand-off Success**: Clean transfers between agents
- **Error Rate**: Failed communications
- **Retry Count**: Re-triggers needed

### Quality Indicators
- ✅ Clean hand-offs with full context
- ✅ No lost information between agents
- ✅ Clear action completions
- ✅ Traceable decision paths
- ❌ Context loss requiring re-work
- ❌ Ambiguous hand-offs
- ❌ Conflicting agent outputs

---

## 🔍 Troubleshooting Communication Issues

### Issue: Context Lost Between Agents
**Solution**: 
- Verify Claude Code synthesis preserves key data
- Check message format completeness
- Add explicit context fields

### Issue: Conflicting Agent Outputs
**Solution**:
- Review role boundaries
- Clarify ownership in workflows
- Add conflict resolution protocol

### Issue: Slow Agent Response
**Solution**:
- Implement timeout handling
- Add async progress indicators
- Consider parallel alternatives

### Issue: Unclear Hand-offs
**Solution**:
- Standardize message formats
- Add validation checks
- Document expected inputs/outputs

---

## 🚀 Future Enhancements

### Phase 1 (Current)
- Manual orchestration with announcements
- Explicit context passing
- Visible communication paths

### Phase 2 (Next)
- Semi-automatic orchestration
- Context inference from patterns
- Reduced announcement verbosity

### Phase 3 (Future)
- Fully automatic orchestration
- Implicit context propagation
- Silent operation mode
- Self-healing communication

---

## 📝 Communication Best Practices

1. **Always Include Work Item Context**
   - Reference VS/BF/TD/HF ID
   - Link to source files
   - Maintain traceability

2. **Make Hand-offs Explicit**
   - State what was completed
   - Define what's needed next
   - Identify the next agent

3. **Preserve Decision Rationale**
   - Document why, not just what
   - Link to requirements
   - Reference patterns used

4. **Handle Errors Gracefully**
   - Provide fallback paths
   - Document failure modes
   - Enable recovery options

5. **Optimize for Clarity**
   - Use standard terminology
   - Avoid agent-specific jargon
   - Focus on outcomes

---

**Last Updated**: 2025-08-16
**Maintained By**: Tech Lead Agent
**Next Review**: After Phase 2 implementation