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

### DevOps Engineer Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Automation Need** | "automate this", "create script", "reduce manual work" | "🤖 DEVOPS: Creating automation" | Python script development |
| **CI/CD Setup** | "setup CI", "automate builds", "pipeline" | "🤖 DEVOPS: Configuring CI/CD" | GitHub Actions pipeline |
| **Deployment** | "deploy", "release", "package build" | "🤖 DEVOPS: Setting up deployment" | Release automation |
| **Environment** | "setup environment", "configure dev" | "🤖 DEVOPS: Environment setup" | Dev environment automation |
| **Monitoring** | "add monitoring", "track metrics" | "🤖 DEVOPS: Adding observability" | Metrics and logging |

### Git Expert Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Merge Conflict** | "merge conflict", "can't merge", "conflict help" | "🔧 GIT EXPERT: Resolving conflicts" | Strategic conflict resolution |
| **History Cleanup** | "clean commits", "squash", "rebase" | "🔧 GIT EXPERT: Organizing history" | Interactive rebase and cleanup |
| **Release Needed** | "create release", "tag version" | "🔧 GIT EXPERT: Managing release" | Tag and release creation |
| **Lost Work** | "lost commits", "accidentally deleted" | "🔧 GIT EXPERT: Recovering work" | Reflog recovery operations |
| **Repo Issues** | "git slow", "repository large" | "🔧 GIT EXPERT: Optimizing repository" | Clean and optimize |

### Architect Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Major Decision** | "should we use", "architecture for", "design system" | "📐 ARCHITECT: Evaluating decision" | Create ADR with analysis |
| **Pattern Needed** | "pattern for", "how should all", "standard way" | "📐 ARCHITECT: Designing pattern" | Define system-wide pattern |
| **Tech Evaluation** | "evaluate", "consider using", "new framework" | "📐 ARCHITECT: Assessing technology" | Technology evaluation |
| **Boundaries** | "separate concerns", "define boundaries" | "📐 ARCHITECT: Defining boundaries" | Design bounded contexts |
| **Review** | "review architecture", "check compliance" | "📐 ARCHITECT: Reviewing compliance" | Architecture audit |

### Debugger Expert Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Stuck on Bug** | "debug", "stuck", "not working", "help with bug" | "🔍 DEBUGGER: Diagnosing issue" | Systematic root cause analysis |
| **Race Condition** | "intermittent", "sometimes works", "concurrent" | "🔍 DEBUGGER: Investigating race condition" | Find threading issues |
| **State Issue** | "phantom", "corruption", "inconsistent" | "🔍 DEBUGGER: Checking state sync" | Diagnose state problems |
| **Memory Issue** | "memory leak", "slowing down", "growing" | "🔍 DEBUGGER: Analyzing memory" | Find leaks and retention |
| **Pipeline Issue** | "not updating", "events not working" | "🔍 DEBUGGER: Tracing notification flow" | Debug event pipeline |

### Test Designer Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Test Needed** | "I want to test", "create test for", "write test" | "📝 TEST DESIGNER: Creating failing test" | Translate requirement to test |
| **Edge Cases** | "what edge cases", "boundary tests" | "📝 TEST DESIGNER: Identifying edge cases" | Suggest additional test scenarios |
| **Test Structure** | "set up tests", "test suite for" | "📝 TEST DESIGNER: Creating test structure" | Design test class organization |
| **Assertion Help** | "what to assert", "how to verify" | "📝 TEST DESIGNER: Designing assertions" | Create meaningful assertions |
| **Mock Needed** | "mock this", "fake service" | "📝 TEST DESIGNER: Creating test doubles" | Design mocks and fakes |

### Dev Engineer Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Test Written (RED)** | "make this pass", "implement this test", "failing test" | "💚 DEV: Implementing to pass test" | Write minimal code for GREEN |
| **Handler Needed** | "implement handler", "create handler" | "💚 DEV: Creating handler" | Implement command/query handler |
| **Service Needed** | "implement service", "create service" | "💚 DEV: Creating service" | Implement service interface |
| **Presenter Needed** | "implement presenter", "create presenter" | "💚 DEV: Creating presenter" | Implement MVP pattern |
| **DI Setup** | "wire up", "register service", "configure DI" | "💚 DEV: Configuring dependencies" | Setup dependency injection |

### QA Engineer Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Implementation Complete** | "tests pass", "implementation done", "ready for QA" | "🧪 QA: Creating integration tests" | Write integration test suite |
| **Stress Test Needed** | "stress test", "load test", "concurrent" | "🧪 QA: Running stress tests" | Test with 100+ operations |
| **Bug Found** | "bug", "issue", "broken", "fails" | "🧪 QA: Creating regression test" | Prevent bug recurrence |
| **Edge Cases** | "edge case", "boundary", "corner case" | "🧪 QA: Finding edge cases" | Discover boundary conditions |
| **Performance Check** | "benchmark", "slow", "performance" | "🧪 QA: Benchmarking performance" | Measure and profile |

### Product Owner Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **Feature Request** | "I want", "add", "feature", "can we" | "🤖 PO TRIGGER: Feature request detected" | Evaluate and create user story |
| **Bug Report** | "error", "bug", "broken", "doesn't work" | "🤖 PO TRIGGER: Bug reported" | Create BF item and prioritize |
| **Work Complete** | Tests pass + implementation done | "🤖 PO TRIGGER: Ready for acceptance" | Review acceptance criteria |
| **Priority Question** | "what should", "which first", "priority" | "🤖 PO TRIGGER: Priority decision needed" | Provide strategic guidance |
| **Session Start** | Beginning work session | "🤖 PO TRIGGER: Session planning" | Set session goals |

### Tech Lead Triggers (Visible)

| Trigger Condition | Detection Pattern | Announcement | Action |
|-------------------|-------------------|--------------|--------|
| **VS Item Ready** | VS moved to ready/ or "plan implementation" | "🤖 TECH LEAD: Creating implementation plan" | Break down into technical phases |
| **Technical Decision** | "how should", "which pattern", "architecture" | "🤖 TECH LEAD: Technical decision needed" | Provide technical guidance |
| **Estimation Request** | "how long", "estimate", "complexity" | "🤖 TECH LEAD: Estimating complexity" | Provide time estimates |
| **Dependency Question** | "depends on", "requires", "blocked by" | "🤖 TECH LEAD: Analyzing dependencies" | Sequence work items |
| **Technical Risk** | "might break", "could fail", "risky" | "🤖 TECH LEAD: Assessing technical risk" | Risk assessment and mitigation |

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

### DevOps Engineer Pattern
```python
# Automation and CI/CD setup
print("🤖 DEVOPS: Creating automation")
print("   → Task: [What to automate]")

response = Task(
    description="DevOps automation",
    prompt=f"""
    Read workflow: Docs/Workflows/devops-engineer-workflow.md
    Execute: {automation_type}
    
    Task to automate: {task_description}
    Current process: {manual_steps}
    Frequency: {how_often}
    
    Create Python script to automate.
    Include error handling and logging.
    Document time savings and usage.
    Use existing scripts in scripts/ as reference.
    """,
    subagent_type="devops-engineer"
)

print("🤖 AUTOMATION CREATED:")
print(synthesize(response))
```

### Git Expert Pattern
```python
# Complex Git operations
print("🔧 GIT EXPERT: Handling Git operation")
print("   → Issue: [Git problem]")

response = Task(
    description="Git operation assistance",
    prompt=f"""
    Read workflow: Docs/Workflows/git-expert-workflow.md
    Execute: {git_operation}
    
    Current situation: {problem_description}
    Branch state: {branch_info}
    Desired outcome: {goal}
    
    Provide safe resolution strategy.
    Create backup before dangerous operations.
    Use --force-with-lease, never plain --force.
    """,
    subagent_type="git-expert"
)

print("🔧 GIT SOLUTION:")
print(synthesize(response))
```

### Architect Pattern
```python
# System-wide architectural decisions
print("📐 ARCHITECT: Evaluating architecture")
print("   → Context: [Decision needed]")

response = Task(
    description="Architectural decision",
    prompt=f"""
    Read workflow: Docs/Workflows/architect-workflow.md
    Execute: {decision_type}
    
    Context: {problem_context}
    Constraints: {constraints}
    Quality attributes: {quality_requirements}
    
    Current architecture: Docs/1_Architecture/
    Reference: src/Features/Block/Move/
    
    Create ADR with decision rationale.
    Consider long-term implications.
    Document trade-offs clearly.
    """,
    subagent_type="architect"
)

print("📐 ARCHITECTURAL DECISION:")
print(synthesize(response))
```

### Debugger Expert Pattern
```python
# Complex bug diagnosis
print("🔍 DEBUGGER EXPERT: Investigating issue")
print("   → Symptoms: [What's happening]")

response = Task(
    description="Debug complex issue",
    prompt=f"""
    Read workflow: Docs/Workflows/debugger-expert-workflow.md
    Execute: Systematic debugging
    
    Symptoms: {bug_description}
    Error messages: {errors}
    Reproduction: {steps}
    Already tried: {attempts}
    
    Diagnose root cause systematically.
    Reference past issues like F1 stress test.
    Suggest concrete fix with regression test.
    """,
    subagent_type="debugger-expert"
)

print("🔍 DIAGNOSIS:")
print(synthesize(response))
```

### Test Designer Pattern
```python
# TDD RED phase test creation
print("📝 TEST DESIGNER: Creating failing test")
print("   → Context: [Requirement to test]")

response = Task(
    description="Create failing unit test",
    prompt=f"""
    Read workflow: Docs/Workflows/test-designer-workflow.md
    Execute: Create failing test (RED phase)
    
    Requirement: {user_requirement}
    Feature context: {feature_description}
    Reference tests: tests/BlockLife.Core.Tests/
    
    Translate requirement into clear failing test.
    Use Arrange-Act-Assert pattern.
    Suggest edge cases to consider.
    """,
    subagent_type="test-designer"
)

print("📝 TEST DESIGN:")
print(synthesize(response))
```

### Dev Engineer Pattern
```python
# TDD GREEN phase implementation
print("💚 DEV ENGINEER: Implementing to pass test")
print("   → Context: [Test to make pass]")

response = Task(
    description="Implement code to pass test",
    prompt=f"""
    Read workflow: Docs/Workflows/dev-engineer-workflow.md
    Execute: Make test pass (GREEN phase)
    
    Failing test: {test_code}
    Test location: {test_path}
    Reference patterns: src/Features/Block/Move/
    
    Write MINIMAL code to make test pass.
    Follow existing patterns exactly.
    No over-engineering or extra features.
    """,
    subagent_type="dev-engineer"
)

print("💚 DEV RESULTS:")
print(synthesize(response))
```

### QA Engineer Pattern
```python
# QA validation and stress testing
print("🧪 QA ENGINEER: [Test action needed]")
print("   → Context: [What's being tested]")

response = Task(
    description="Quality assurance testing",
    prompt=f"""
    Read workflow: Docs/Workflows/qa-engineer-workflow.md
    Execute: {action}
    
    Feature to test: {feature_path}
    Implementation: {code_location}
    Acceptance criteria: {criteria}
    
    Focus on:
    - Integration tests (NOT unit tests)
    - Stress testing with concurrent operations
    - Edge case discovery
    - Performance benchmarking
    """,
    subagent_type="qa-engineer"
)

print("🧪 QA RESULTS:")
print(synthesize(response))
```

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
    subagent_type="product-owner"
)

print("📊 PO DECISION:")
print(synthesize(response))
```

### Tech Lead Pattern
```python
# Claude Code announces and triggers
print("🤖 TECH LEAD: [Technical action needed]")
print("   → Context: [What's being planned]")

response = Task(
    description="Technical planning",
    prompt=f"""
    Read workflow: Docs/Workflows/tech-lead-workflow.md
    Execute: {action}
    VS Item: {vs_path}
    Architecture: Docs/1_Architecture/Architecture_Guide.md
    Reference: src/Features/Block/Move/
    """,
    subagent_type="tech-lead"
)

print("📐 TECH LEAD OUTPUT:")
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