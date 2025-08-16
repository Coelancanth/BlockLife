# Workflows Documentation

**Last Updated**: 2025-08-15  
**Status**: Organized into logical subdirectories  

## 📁 Folder Structure

### 🤖 Agent-Workflows/
Individual workflow definitions for each of the 11 specialized agents in the BlockLife ecosystem.

**Core Workflow Agents:**
- `product-owner-workflow.md` - User story creation, backlog prioritization, work item management
- ~~`backlog-maintainer-workflow.md`~~ - **DEPRECATED** - Moved to `archived/` folder  
- `tech-lead-workflow.md` - Implementation planning, technical decisions

**TDD Workflow Agents:**
- `test-designer-workflow.md` - TDD RED phase, test creation
- `dev-engineer-workflow.md` - TDD GREEN phase, minimal implementation
- `qa-engineer-workflow.md` - Integration testing, stress testing

**Specialist Agents:**
- `architect-workflow.md` - System design, ADRs, architectural patterns
- `vsa-refactoring-workflow.md` - VSA maintenance, code duplication
- `debugger-expert-workflow.md` - Complex bug diagnosis
- `git-expert-workflow.md` - Git operations, merge conflicts
- `devops-engineer-workflow.md` - CI/CD, automation scripts

### 🎯 Orchestration-System/
Core orchestration system that manages how and when agents are triggered.

**Critical Files:**
- `AGENT_ORCHESTRATION_GUIDE.md` - **MANDATORY READING** - Trigger patterns and detection logic
- `AGENT_COMMUNICATION_PROTOCOLS.md` - How agents communicate and coordinate
- `AGENT_INTEGRATION_PATTERNS.md` - Proven patterns for multi-agent workflows
- `AGENT_INTERACTION_DIAGRAMS.md` - Visual representations of agent workflows
- `DOUBLE_VERIFICATION_PROTOCOL.md` - Validation mechanisms for agent triggers
- `ORCHESTRATION_FEEDBACK_SYSTEM.md` - Report and track workflow failures
- `workflow-testing-protocol.md` - Testing procedures for workflow validation
- `TD_014_COMPLETION_SUMMARY.md` - Agent Architecture Pattern Update (100% complete)

### 📊 Automatic-Orchestration-Pattern/
Documentation of the Automatic Orchestration Pattern that ensures all development work is tracked.

- `automatic-orchestration-pattern-final-report.md` - Complete pattern documentation
- `automatic-orchestration-pattern-test-results.md` - Testing and validation results
- `automatic-orchestration-trigger-implementation.md` - Technical implementation details

## 🚨 CRITICAL: Start Here

### For Understanding Agent Triggering:
1. **READ FIRST**: `Orchestration-System/AGENT_ORCHESTRATION_GUIDE.md`
2. **UNDERSTAND**: Why automatic triggering is mandatory (no exceptions!)
3. **LEARN**: Which agents to trigger and when

### For Working with Specific Agents:
1. Navigate to `Agent-Workflows/[agent-name]-workflow.md`
2. Understand the agent's responsibilities and trigger conditions
3. Follow the workflow steps exactly

### For Reporting Issues:
1. Use `Orchestration-System/ORCHESTRATION_FEEDBACK_SYSTEM.md`
2. Report missed triggers, wrong agents, or workflow failures
3. Help improve the system through feedback

## 📋 Quick Reference: Agent Triggering

### 🔴 IMMEDIATE TRIGGERS REQUIRED

**After ANY code/doc changes:**
→ Update backlog progress directly

**When user requests features:**
→ Trigger `product-owner` NOW

**When starting new VS work:**
→ Trigger `tech-lead` NOW

**When writing tests:**
→ Trigger `test-designer` NOW

**When implementing to pass tests:**
→ Trigger `dev-engineer` NOW

## 🎯 Key Principles

### 1. **Automatic Triggering is Mandatory**
- Every development action MUST trigger the appropriate agent
- No exceptions, no "I'll do it later"
- The Backlog is the Single Source of Truth

### 2. **Agents Have Specialties**
- Don't do work that belongs to a specialist agent
- Example: Feature evaluation → product-owner (not manual decision)
- Example: Git conflicts → git-expert (not general debugging)

### 3. **Workflows Are Prescriptive**
- Follow the exact steps in each workflow
- Don't skip steps or change order
- If a workflow doesn't work, report it via feedback system

## 📊 Workflow Maturity

### Current Phase: Learning
- High feedback rate expected
- Workflow failures will happen
- Active refinement ongoing
- User guidance needed

### Target: Mature Operation
- Automatic trigger compliance >90%
- Correct agent selection >95%
- Minimal workflow failures
- Autonomous operation

## 🔄 Common Workflow Patterns

### Feature Development Flow
1. User requests feature → `product-owner` creates story
2. Story becomes VS → `tech-lead` creates plan
3. Tests needed → `test-designer` writes failing tests
4. Implementation → `dev-engineer` makes tests pass
5. Every edit → Claude Code updates progress directly

### Bug Fix Flow
1. User reports bug → `product-owner` creates BF item
2. Complex debugging → `debugger-expert` diagnoses
3. Fix implemented → `dev-engineer` applies fix
4. Tests added → `qa-engineer` ensures no regression
5. Every step → Claude Code updates progress directly

### Documentation Update Flow
1. Docs need update → Direct work (no special agent)
2. Files reorganized → Update backlog progress
3. Links need fixing → Fix directly and note in backlog
4. Cross-references → Update directly

## ⚠️ Common Mistakes to Avoid

### ❌ DON'T:
- Skip agent triggers "because it's minor work"
- Handle specialist work yourself (e.g., link updates)
- Batch triggers "for later"
- Work without creating branches (git-expert domain)
- Debug complex issues alone (debugger-expert exists)

### ✅ DO:
- Trigger immediately after EVERY action
- Delegate to specialist agents
- Follow workflows exactly
- Report workflow failures
- Use feedback system actively

## 📈 Success Metrics

**Tracking in each session:**
- Trigger accuracy rate
- Correct agent selection
- Workflow compliance
- Announcement consistency

**Target performance:**
- >90% automatic triggers
- >95% correct agent selection
- 100% critical step compliance

## 🔗 Integration with Other Systems

### With CLAUDE.md
- CLAUDE.md references this structure
- Critical triggers defined here
- Orchestration rules enforced

### With Backlog
- All work tracked via agent triggers
- Backlog.md is Single Source of Truth
- Progress updated automatically

### With Living Wisdom
- Workflow lessons become playbooks
- Failures tracked in incident reports
- Patterns evolve through experience

---

**Remember**: The workflow system is how solo developer + AI scales effectively. Every trigger missed is work lost. Every workflow followed is progress tracked. The system only works when we work the system!