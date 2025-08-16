# Agent Orchestration Guide

## Purpose
This document defines WHEN and HOW Claude Code triggers specialized agents, verifies their outputs, and updates the simple 3-tier priority backlog to maintain effective orchestration workflow.

**Integration**: This guide works with [CLAUDE.md](../../../CLAUDE.md) and the simple [Backlog.md](../../Backlog/Backlog.md) priority system.

---

## 🎯 Core Principle
Claude Code (main agent) is the orchestrator who:
1. **Detects** trigger conditions
2. **Triggers** appropriate agents
3. **Verifies** agent outputs
4. **Updates** simple backlog priority sections (🔥📈💡)
5. **Synthesizes** responses

## 📅 Date Accuracy Protocol
**MANDATORY**: Always use `bash date` command for current dates. LLMs don't know the actual date.
- **When updating Backlog**: Use `date +"%Y-%m-%d"` to update "Last Updated" field
- **When completing work**: Add to ✅Done This Week with simple description

---

## 🔄 Agent Trigger Map

### Product Owner Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Feature Request** | "I want", "add", "feature", "can we" | Evaluate and add to appropriate priority tier |
| **Bug Report** | "error", "bug", "broken", "doesn't work" | Add to 🔥Critical or 📈Important based on severity |
| **Work Complete** | Tests pass + implementation done | Move to ✅Done This Week |
| **Priority Question** | "what should", "which first", "priority" | Reorganize 🔥📈💡 sections |

### Tech Lead Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Implementation Planning** | "plan implementation", "how to build" | Create implementation strategy |
| **Technical Decision** | "how should", "which pattern", "architecture" | Provide technical guidance |
| **Estimation Request** | "how long", "estimate", "complexity" | Provide time estimates |
| **Dependency Question** | "depends on", "requires", "blocked by" | Sequence work items |

### Test Designer Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Test Needed** | "I want to test", "create test for", "write test" | Create failing test (RED phase) |
| **Edge Cases** | "what edge cases", "boundary tests" | Suggest additional test scenarios |
| **Test Structure** | "set up tests", "test suite for" | Design test class organization |

### Dev Engineer Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Test Written (RED)** | "make this pass", "implement this test", "failing test" | Write minimal code for GREEN |
| **Handler Needed** | "implement handler", "create handler" | Implement command/query handler |
| **Service Needed** | "implement service", "create service" | Implement service interface |

### QA Engineer Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Implementation Complete** | "tests pass", "implementation done", "ready for QA" | Write integration test suite |
| **Stress Test Needed** | "stress test", "load test", "concurrent" | Test with 100+ operations |
| **Bug Found** | "bug", "issue", "broken", "fails" | Create regression test |

### Debugger Expert Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Stuck on Bug** | "debug", "stuck", "not working", "help with bug" | Systematic root cause analysis |
| **Race Condition** | "intermittent", "sometimes works", "concurrent" | Find threading issues |
| **State Issue** | "phantom", "corruption", "inconsistent" | Diagnose state problems |

### Git Expert Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Merge Conflict** | "merge conflict", "can't merge", "conflict help" | Strategic conflict resolution |
| **History Cleanup** | "clean commits", "squash", "rebase" | Interactive rebase and cleanup |
| **Release Needed** | "create release", "tag version" | Tag and release creation |

### DevOps Engineer Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Automation Need** | "automate this", "create script", "reduce manual work" | Python script development |
| **CI/CD Setup** | "setup CI", "automate builds", "pipeline" | GitHub Actions pipeline |
| **Environment** | "setup environment", "configure dev" | Dev environment automation |

### Architect Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Major Decision** | "should we use", "architecture for", "design system" | Create ADR with analysis |
| **Pattern Needed** | "pattern for", "how should all", "standard way" | Define system-wide pattern |
| **Tech Evaluation** | "evaluate", "consider using", "new framework" | Technology evaluation |

### VSA Refactoring Triggers
| Trigger Condition | Detection Pattern | Action |
|-------------------|-------------------|--------|
| **Duplication Found** | "duplicate code", "same code in", "repeated across" | Identify extraction candidates |
| **Extract Request** | "extract shared", "pull out common", "create service" | Extract to proper layer |
| **3+ Slice Rule** | Code appears in 3+ feature slices | Suggest extraction |

---

## 🔍 Mandatory Post-Agent Verification Workflow

### Critical Step: Verify and Update After EVERY Agent

After **every single agent interaction**, Claude Code MUST:

1. **Verify Agent Output**
   - Did the agent actually complete what it reported?
   - Are files created/moved/modified as claimed?
   - Are status changes reflected in the actual system?

2. **Update Backlog Directly**
   - Update simple status in Backlog.md (move items between 🔥Critical, 📈Important, 💡Ideas)
   - Add completed items to ✅Done This Week section
   - Note any blockers in 🚧Currently Blocked section

3. **Document Issues**
   - If agent reported success but didn't complete work, create bug report
   - Note any unexpected findings or complications

---

## 📊 Simple Status Tracking

### Backlog Updates (No Percentages)
| Event | Action | Notes |
|-------|--------|-------|
| Work started | Move to 🔥Critical if blocking, otherwise keep in 📈Important | Active work gets priority |
| Work completed | Move to ✅Done This Week | Simple completion tracking |
| Blocker found | Add to 🚧Currently Blocked | Visibility for dependencies |
| New idea | Add to 💡Ideas | Future consideration |
| Priority change | Move between 🔥📈💡 sections | Based on current needs |

**Approach**: Simple, visual priority management in single file
**Updated by**: Claude Code directly updates Backlog.md sections

---

## 🎮 Manual Override Commands

If automatic triggering misses something, you can request:

- **"Trigger PO for [action]"** - Manually invoke Product Owner
- **"Update backlog status"** - Move items between priority sections
- **"Check backlog status"** - Verify backlog reflects current work state
- **"Show PO decision"** - Get strategic input
- **"Verify agent output"** - Check if agent actually completed reported work

---

## 🚨 Trigger Validation Checklist

During workflow validation, check each trigger:

- [ ] Was the trigger condition correctly detected?
- [ ] Did the right agent get invoked?
- [ ] Was the context sufficient?
- [ ] Did the agent follow its workflow?
- [ ] Was the result properly integrated?
- [ ] Did Claude Code verify the agent output?
- [ ] Was the backlog updated with verified results?
- [ ] Were any blockers or issues documented?

---

## 📈 Workflow Maturity

### Current Stage: Pragmatic Orchestration
- Triggers happen when value is clear
- Balance automation with direct action
- Focus on complex tasks requiring specialization
- Verify all agent outputs

### Success Metrics
- Agent triggered for appropriate complexity
- Verification catches agent failures
- Backlog stays synchronized with reality
- User receives unified, synthesized responses

---

## 📚 Related Documentation

### Core Orchestration Documents
- **[CLAUDE.md](../../../CLAUDE.md)** - Main orchestration instructions and agent delegation rules
- **[Individual Agent Workflows](../../Agents/)** - Specific agent procedures and workflows

### Agent Workflows
All agent-specific workflows are in: `../../Agents/`
- Each agent has a dedicated workflow file
- Workflows define exact procedures
- Integration points are documented

### Implementation Status
- **Current Version**: Aligned with 10-agent ecosystem
- **Integration**: Works with CLAUDE.md orchestration workflow
- **Last Updated**: 2025-08-16 - Created to match actual system structure