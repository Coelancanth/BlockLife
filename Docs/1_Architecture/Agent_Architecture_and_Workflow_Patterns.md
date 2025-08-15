# Agent Architecture and Workflow Patterns

## Executive Summary

This document clarifies the architecture and operational patterns of Claude Code's agent system, including the relationship between the main orchestrator agent and specialized subagents, workflow patterns, and best practices for extending the agent ecosystem.

## Table of Contents

1. [Core Architecture](#core-architecture)
2. [Agent Types and Hierarchy](#agent-types-and-hierarchy)
3. [Communication Patterns](#communication-patterns)
4. [Workflow Orchestration](#workflow-orchestration)
5. [Creating New Agents](#creating-new-agents)
6. [Configuration and Control](#configuration-and-control)
7. [Best Practices](#best-practices)
8. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
9. [Examples and Case Studies](#examples-and-case-studies)

## Core Architecture

### Automatic Agent Selection via Description Matching

**CRITICAL:** Claude Code automatically selects subagents based on the `description` field matching the current task context. Write descriptions that are:
- **Specific** about the task type
- **Action-oriented** with clear trigger words
- **Contextual** to match user language patterns

### Single Orchestrator Pattern

Claude Code implements a **Single Orchestrator Pattern** where one main agent maintains context and coordinates all specialized work:

```
┌─────────────────────────────────────────┐
│          Claude Code (Main)             │
│  - Maintains conversation context       │
│  - Orchestrates subagent calls          │
│  - Synthesizes results                  │
│  - Manages todo lists                   │
│  - Handles user interaction             │
└────────────┬────────────────────────────┘
             │
    ┌────────┴────────┬────────┬────────┬────────┐
    ▼                 ▼        ▼        ▼        ▼
┌─────────┐    ┌─────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│ Agent A │    │ Agent B │ │Agent C │ │Agent D │ │Agent E │
│(stories)│    │(planner)│ │(review)│ │ (docs) │ │(custom)│
└─────────┘    └─────────┘ └────────┘ └────────┘ └────────┘
```

### Key Architectural Principles

1. **Context Preservation**: Only the main agent maintains conversation history
2. **Stateless Specialists**: Subagents are stateless and complete tasks autonomously
3. **Single Response**: Each agent invocation returns exactly one response
4. **No Direct Chaining**: Agents cannot call other agents directly
5. **Orchestrated Coordination**: Main agent decides sequencing and parallelization

## Agent Types and Hierarchy

### Main Orchestrator Agent (Claude Code)

**Responsibilities:**
- User interaction and dialogue management
- Context and state management
- Subagent selection and invocation
- Result synthesis and presentation
- Todo list management
- Tool usage coordination

**Characteristics:**
- Stateful (maintains conversation history)
- Has access to all tools
- Follows CLAUDE.md instructions
- Can invoke multiple subagents

### Specialized Subagents

**Current Agent Types with Trigger Descriptions:**

| Agent Name | Description (Triggers Selection & Proactivity) | Purpose | Model |
|------------|-----------------------------------------------|---------|-------|
| `general-purpose` | "Complex research and multi-step tasks" | Open-ended exploration | - |
| `agile-product-owner` | "Use PROACTIVELY when user describes features or bugs are found. Break down features into user stories, create acceptance criteria, prioritize backlog items, translate business requirements into technical implementation plans" | Feature planning & backlog maintenance | opus |
| `code-reviewer` | "MUST BE USED after significant code implementation. Review for architecture violations, validate patterns, check test coverage" | Code quality assurance | - |
| `statusline-setup` | "Configure status line settings" | UI configuration | - |

**Key Insight:** The `description` field is pattern-matched against user requests. When users say "break down this feature" or "create user stories", the agile-product-owner agent is automatically selected.

**Characteristics:**
- **Clean slate** (no CLAUDE.md, no conversation history)
- **Tool access** (Read, Write, Edit, etc.)
- **Must be told what to read** (explicit file paths)
- **Autonomous execution** (complete task independently)
- **Single response** (return summary to main)

## Communication Patterns

### Critical: File Path Orchestration

**Subagents start with clean slate - the main agent MUST provide explicit file paths:**

```
Main Agent (knows from CLAUDE.md):
  "Read the backlog at Docs/Product_Backlog/Backlog.md
   Check existing items in Docs/Product_Backlog/backlog/
   Use template from Docs/Product_Backlog/templates/VS_Template.md
   Create new item at Docs/Product_Backlog/backlog/VS_XXX_Feature.md"
       ↓
Subagent (executes with tools):
  1. Read(Docs/Product_Backlog/Backlog.md)
  2. Glob(Docs/Product_Backlog/backlog/*.md)
  3. Read(Docs/Product_Backlog/templates/VS_Template.md)
  4. Write(Docs/Product_Backlog/backlog/VS_XXX_Feature.md)
  5. Return summary to main
```

**Key Insight:** Subagents don't know WHERE things are - they only know HOW to process them.

### Sequential Pattern

The most common pattern where agents are invoked one after another:

```
User Request
    ↓
Main Agent (analyzes request)
    ↓
Main → Subagent A (Task tool with context)
    ↓
Subagent A → Main (returns result)
    ↓
Main (processes result, determines next step)
    ↓
Main → Subagent B (Task tool with A's output as context)
    ↓
Subagent B → Main (returns result)
    ↓
Main → User (synthesized response)
```

**Example Flow with Explicit Paths:**
```python
# Pseudo-code representation showing file path orchestration
def handle_feature_request(user_request):
    # Step 1: Create user stories with explicit paths
    stories = Task(
        description="Create user stories",
        prompt=f"""Create user stories for: {user_request}
        
        Instructions:
        1. Read current backlog: Docs/Product_Backlog/Backlog.md
        2. Check existing stories: Docs/Product_Backlog/backlog/
        3. Use template: Docs/Product_Backlog/templates/VS_Template.md
        4. Create new story: Docs/Product_Backlog/backlog/VS_XXX_Feature.md
        5. Update README with new item
        """,
        subagent_type="agile-product-owner"
    )
    
    # Step 2: Create implementation plan
    plan = Task(
        description="Create technical plan",
        prompt=f"""Plan implementation for stories created at:
        Docs/Product_Backlog/backlog/VS_XXX_Feature.md
        
        Instructions:
        1. Read the story file
        2. Check architecture: Docs/1_Architecture/Architecture_Guide.md
        3. Reference: src/Features/Block/Move/ for patterns
        4. Create plan: Docs/3_Implementation_Plans/Feature_Plan.md
        """,
        subagent_type="implementation-planner"
    )
    
    return synthesize(stories, plan)
```

### Parallel Pattern

For independent tasks that can run simultaneously:

```
User Request
    ↓
Main Agent (identifies parallel opportunities)
    ├─→ Subagent A ─┐
    ├─→ Subagent B ─┼─→ All return to Main
    └─→ Subagent C ─┘
           ↓
    Main (synthesizes all results)
           ↓
        User Response
```

**Use Cases:**
- Gathering information from multiple sources
- Running different types of analysis
- Creating multiple independent artifacts

### Context Passing

Context flows through the main agent, not directly between subagents:

```
✅ CORRECT:
Main → Agent A (with initial context)
     ← Result A
Main → Agent B (with initial context + Result A)
     ← Result B
Main → Agent C (with all previous context)

❌ INCORRECT:
Main → Agent A → Agent B → Agent C
(Agents cannot directly communicate)
```

## Workflow Orchestration

### Defining Workflows in CLAUDE.md

CLAUDE.md must contain **file paths and locations** that the main agent uses to orchestrate subagents:

```markdown
## Multi-Agent Workflow Orchestration

### Feature Request → Agile Product Owner
When user requests a feature, invoke agile-product-owner with:
- Read current backlog: Docs/Product_Backlog/Backlog.md
- Check existing items: Docs/Product_Backlog/backlog/
- Templates location: Docs/Product_Backlog/templates/
- Create new items in: Docs/Product_Backlog/backlog/
- Naming pattern: VS_XXX_[Domain]_[Feature].md

### Bug Report → Agile Product Owner
When bugs are found, invoke agile-product-owner with:
- Read bug report: [specific path provided]
- Template: Docs/Product_Backlog/templates/BF_Template.md
- Create items in: Docs/Product_Backlog/backlog/
- For critical bugs: Move directly to active/

### Implementation Planning
When planning needed, invoke implementation-planner with:
- Read story: [specific VS_XXX file path]
- Architecture guide: Docs/1_Architecture/Architecture_Guide.md
- Reference patterns: src/Features/Block/Move/
- Create plan: Docs/3_Implementation_Plans/XXX_Feature_Plan.md

### Code Review
When review needed, invoke code-reviewer with:
- Read implementation: [specific file paths]
- Architecture tests: tests/Architecture/ArchitectureFitnessTests.cs
- Standards: Docs/1_Architecture/Standard_Patterns.md
```

### Workflow Decision Tree

```
User Request
    ↓
Is it complex/multi-faceted?
    ├─ No → Handle directly
    └─ Yes → Determine agent needs
              ↓
         What type of task?
         ├─ Feature Planning → product-owner-story-writer
         ├─ Technical Design → implementation-planner
         ├─ Code Review → code-review-expert
         ├─ Documentation → docs-updater
         └─ Research → general-purpose
```

### Todo Management Across Workflows

The main agent maintains a single todo list that spans all agent activities:

```markdown
## TodoWrite Integration

1. Main agent creates initial todos
2. After each subagent returns:
   - Mark relevant todos as completed
   - Add new todos based on subagent output
3. Track progress across entire workflow
4. Ensure all todos completed before finishing
```

## Creating New Agents

### When to Create a New Agent

Create a new specialized agent when you have:

1. **Domain Specialization**
   ```yaml
   Examples:
   - database-migration-specialist
   - security-vulnerability-scanner
   - performance-optimizer
   - api-contract-designer
   ```

2. **Complex Workflows**
   ```yaml
   Examples:
   - legacy-code-refactorer
   - test-suite-generator
   - dependency-updater
   - ci-cd-pipeline-builder
   ```

3. **Specific Output Formats**
   ```yaml
   Examples:
   - diagram-generator
   - api-documentation-writer
   - changelog-creator
   - release-notes-compiler
   ```

### Agent Definition Structure

Create new agents in `.claude/agents/[agent-name].md`:

```markdown
---
name: your-agent-name
description: [CRITICAL - This triggers automatic selection! Be specific and action-oriented]
model: sonnet  # or other available model
color: blue    # visual identifier
---

You are an expert in [domain]. Your role is to [specific purpose].

## Core Capabilities (HOW, not WHERE)
- Process and analyze [type of content]
- Create [type of output]
- Apply [domain] best practices

## Expected Instructions
You will receive explicit paths and instructions:
1. Files to read (paths will be provided)
2. Templates to use (locations will be given)
3. Where to create output (destination specified)

## Process (Generic, not path-specific)
1. Read files as instructed
2. Apply your expertise
3. Create/modify as directed
4. Return summary of actions

## Domain Knowledge
[Specific expertise without hardcoded paths]

## Quality Criteria
- [Domain-specific quality measures]
- [Output standards]
- [Success metrics]
```

**Key Principle:** Agent definitions should be **portable** - they work regardless of project structure because the main agent provides all paths.

### Writing Effective Description Fields

The `description` field controls **both selection and proactivity**. Use special phrases to encourage proactive use:

#### Proactivity Control Phrases
```yaml
# Highly Proactive (agent used automatically)
description: "Use PROACTIVELY when code is written. Review code for architecture violations..."

# Mandatory Use (agent MUST be invoked)
description: "MUST BE USED after feature implementation. Validate patterns, check coverage..."

# Conditional Proactive (used based on context)
description: "Use proactively for complex features. Break down into user stories..."
```

#### ✅ GOOD Descriptions (Specific, Action-Oriented, with Proactivity)
```yaml
description: "Use PROACTIVELY when user describes features. Break down into user stories, create acceptance criteria, prioritize backlog items"
# Proactive trigger + specific actions

description: "MUST BE USED after significant code changes. Review for architecture violations, validate patterns, check test coverage"
# Mandatory trigger + review actions

description: "Use proactively when bugs are found. Create BF items, prioritize fixes, update backlog"
# Conditional trigger + bug handling
```

#### ❌ BAD Descriptions (Vague & Passive)
```yaml
description: "Helps with product management tasks"
# Too vague - won't trigger reliably

description: "Code analysis"
# Too brief - misses trigger opportunities

description: "An agent that does various things related to testing"
# Not action-oriented - won't match user language
```

#### Description Writing Formula
```
[Proactivity phrase] + [trigger condition] + [action verbs] + [specific tasks]

Examples:
- "Use PROACTIVELY when features are described. Break down into user stories with acceptance criteria"
- "MUST BE USED after database changes. Migrate schemas safely between versions"
- "Use proactively for React code. Optimize components for performance"
```

#### Proactivity Levels

| Level | Phrase | Behavior |
|-------|--------|----------|
| **Mandatory** | "MUST BE USED" | Agent always invoked when condition met |
| **Highly Proactive** | "Use PROACTIVELY" | Agent invoked automatically without user asking |
| **Conditionally Proactive** | "Use proactively when/for" | Agent invoked based on context |
| **On Request** | (no proactivity phrase) | Agent only used when explicitly requested |

### Agent Capability Matrix

Define clear boundaries for each agent:

| Capability | Main | Story Writer | Planner | Reviewer | Custom |
|-----------|------|--------------|---------|----------|---------|
| File I/O | ✅ | ❌ | ✅ | ✅ | Varies |
| Web Access | ✅ | ❌ | ✅ | ❌ | Varies |
| Code Execution | ✅ | ❌ | ❌ | ❌ | Varies |
| Todo Management | ✅ | ❌ | ❌ | ❌ | ❌ |
| User Interaction | ✅ | ❌ | ❌ | ❌ | ❌ |

## Configuration and Control

### CLAUDE.md Configuration

Control agent behavior through CLAUDE.md:

```markdown
## Agent Invocation Rules

### Automatic Agent Selection
Claude Code automatically selects agents when task matches description.
To override or ensure specific agent use:

**Force specific agent:**
"Use the agile-product-owner agent to..."

**Trigger automatic selection with keywords:**
- "break down this feature" → agile-product-owner
- "review this code" → code-reviewer
- "stress test this" → architecture-stress-tester

### Proactive Agent Usage
**Main agent should proactively use agents when:**
- Complex feature request → agile-product-owner
- Code implementation complete → code-reviewer
- Bug report received → agile-product-owner (create BF item)
- Performance concerns → architecture-stress-tester

### Agent Sequencing
**Preferred sequences:**
1. Feature: stories → planning → implementation → review → docs
2. Bug: report → BF creation → test → fix → verification
3. Refactor: TD creation → plan → implementation → validation
```

### Environment-Specific Configuration

Different environments can have different agent configurations:

```markdown
## Environment: Development
- Use all available agents
- Verbose output from agents
- Include debug information

## Environment: Production
- Limited agent set
- Concise output only
- No debug information

## Environment: CI/CD
- Automated agent selection
- Structured output for parsing
- Fail-fast on errors
```

## Best Practices

### 1. Agent Selection

**DO:**
- Choose the most specialized agent for the task
- Use agents for complex, multi-step operations
- Provide clear, detailed context to agents

**DON'T:**
- Use agents for simple, single-step tasks
- Chain more than 3-4 agents in sequence
- Assume agents remember previous invocations

### 2. Context Management

**Effective Context Passing:**
```python
# Good: Structured context with clear sections
context = f"""
Previous Results:
{previous_agent_output}

Current Requirements:
{user_requirements}

Constraints:
{project_constraints}

Expected Output:
{output_specification}
"""

# Bad: Dumping everything without structure
context = previous_output + user_input + some_other_data
```

### 3. Error Handling

**Agent Failure Handling:**
```markdown
If agent returns unexpected output:
1. Log the issue for debugging
2. Attempt alternative approach
3. Inform user of limitation
4. Suggest manual intervention if needed
```

### 4. Performance Optimization

**Parallel vs Sequential:**
```markdown
## Parallel Execution (when possible):
- Independent analysis tasks
- Multiple document generation
- Separate validation checks

## Sequential Execution (required):
- Dependent transformations
- Progressive refinement
- Staged workflows
```

## Anti-Patterns to Avoid

### 1. Multiple Main Agents

**❌ WRONG: Multiple orchestrators**
```
User ─┬→ Main Agent A ─→ Subagents
      └→ Main Agent B ─→ Subagents
```

**Problems:**
- Context fragmentation
- State synchronization issues
- User confusion
- Duplicate work

**✅ CORRECT: Single orchestrator**
```
User ─→ Main Agent ─┬→ Subagent A
                    ├→ Subagent B
                    └→ Subagent C
```

### 2. Direct Agent Chaining

**❌ WRONG: Agents calling agents**
```
Main → Agent A → Agent B → Agent C
```

**Problems:**
- Loss of orchestration control
- Cannot adjust based on intermediate results
- Difficult to debug
- No opportunity for user intervention

**✅ CORRECT: Orchestrated sequence**
```
Main → Agent A → Main → Agent B → Main → Agent C → Main
```

### 3. Stateful Subagents

**❌ WRONG: Expecting memory**
```python
# First call
Task("Remember this: X", agent_type="helper")

# Second call
Task("Use what I told you before", agent_type="helper")
# Agent has no memory of X
```

**✅ CORRECT: Explicit context**
```python
# First call
result = Task("Process this: X", agent_type="helper")

# Second call
Task(f"Given {result}, do Y", agent_type="helper")
```

### 4. Over-Specialization

**❌ WRONG: Too many narrow agents**
```
- agent-for-adding-buttons
- agent-for-adding-forms
- agent-for-adding-tables
- agent-for-adding-modals
```

**✅ CORRECT: Balanced specialization**
```
- ui-component-designer (handles all UI components)
```

## Examples and Case Studies

### Case Study 1: Feature Development with File Path Orchestration

**Scenario:** User requests "Add inventory system to the game"

**Workflow with Explicit Paths:**
```
1. Main agent recognizes complex feature request

2. Invokes agile-product-owner:
   Task: "Create user stories for inventory system
   Instructions:
   - Read backlog: Docs/Product_Backlog/Backlog.md
   - Check existing: Docs/Product_Backlog/backlog/
   - Use template: Docs/Product_Backlog/templates/VS_Template.md
   - Create: Docs/Product_Backlog/backlog/VS_XXX_Inventory_System.md"
   
   Output: Created VS_XXX_Inventory_System.md

3. Main assigns priority, moves to ready/
   - Renames: VS_XXX → VS_015
   - Moves: backlog/ → ready/

4. Invokes implementation-planner:
   Task: "Create implementation plan
   Instructions:
   - Read story: Docs/Product_Backlog/ready/VS_015_Inventory_System.md
   - Read architecture: Docs/1_Architecture/Architecture_Guide.md
   - Reference: src/Features/Block/Move/
   - Create: Docs/3_Implementation_Plans/015_Inventory_Implementation.md"
   
   Output: Technical plan created

5. Main implements following plan
   - Uses TodoWrite for tracking
   - Creates files in src/Features/Inventory/

6. Invokes code-reviewer:
   Task: "Review implementation
   Instructions:
   - Read code: src/Features/Inventory/
   - Check tests: tests/BlockLife.Core.Tests/Features/Inventory/
   - Validate against: tests/Architecture/ArchitectureFitnessTests.cs"
   
   Output: Review feedback

Result: Complete, tracked, maintainable feature
```

### Case Study 2: Debugging Complex Issue

**Scenario:** "The game freezes when placing blocks rapidly"

**Workflow:**
```
1. Main agent identifies performance/debugging task
2. Invokes general-purpose agent for investigation
   - Input: "Investigate freeze when rapidly placing blocks"
   - Output: Identified race condition in notification system

3. Main creates fix based on investigation
4. Invokes architecture-stress-tester
   - Input: Fixed code + stress test parameters
   - Output: Performance metrics and validation

5. Main confirms fix resolves issue
6. Creates post-mortem documentation

Result: Issue resolved with proper validation
```

### Case Study 3: Parallel Information Gathering

**Scenario:** "Analyze our codebase for security, performance, and architecture issues"

**Workflow:**
```
1. Main agent identifies three independent analyses
2. Invokes three agents in parallel:
   - security-scanner: Checks for vulnerabilities
   - performance-analyzer: Identifies bottlenecks
   - architecture-validator: Verifies patterns

3. All three return simultaneously to main
4. Main synthesizes findings into comprehensive report
5. Creates prioritized action items

Benefit: 3x faster than sequential execution
```

## Monitoring and Metrics

### Agent Performance Metrics

Track these metrics for agent optimization:

```markdown
## Metrics to Monitor

### Invocation Metrics
- Frequency of each agent use
- Success/failure rates
- Average execution time
- Context size provided

### Quality Metrics
- User satisfaction with agent output
- Rework required after agent completion
- Accuracy of agent recommendations

### Efficiency Metrics
- Time saved vs manual approach
- Reduction in user queries
- Completion rate of agent suggestions
```

### Continuous Improvement

```markdown
## Agent Evolution Process

1. **Monitor Usage Patterns**
   - Which agents are used most?
   - What tasks are handled manually?

2. **Identify Gaps**
   - Repeated manual tasks → New agent opportunity
   - Failed agent attempts → Agent improvement needed

3. **Iterate on Agents**
   - Refine prompts based on failures
   - Add new capabilities as needed
   - Retire unused agents

4. **Document Learnings**
   - Update this document
   - Share patterns that work
   - Document anti-patterns discovered
```

## Summary: The Agent Architecture

### Four Critical Success Factors

1. **Description-Driven Selection & Proactivity**
   - The `description` field triggers automatic agent selection
   - Proactivity phrases ("MUST BE USED", "Use PROACTIVELY") control when agents are invoked
   - Must be specific, action-oriented, and match user language
   - Poor descriptions = agents won't be used

2. **Clean Slate with Explicit Paths**
   - Subagents start with no context (no CLAUDE.md)
   - Main agent must provide all file paths explicitly
   - Separation: Agents know HOW, Main knows WHERE

3. **Single Orchestrator Pattern**
   - Main agent maintains all context and state
   - Subagents execute independently and return
   - No direct agent-to-agent communication

4. **Proactive vs Reactive Invocation**
   - Proactive: Agent used without user explicitly asking
   - Reactive: Agent used only when requested
   - Controlled by description field phrases

### The Complete Flow

```
User: "Break down the inventory feature"
         ↓
Main Agent: Matches "break down" to agile-product-owner description
         ↓
Main Agent: "Read backlog at Docs/Product_Backlog/Backlog.md,
            Create story at Docs/Product_Backlog/backlog/VS_XXX.md"
         ↓
Subagent: Executes with tools, returns summary
         ↓
Main Agent: Continues with context
```

By following these patterns, you create a robust, maintainable agent ecosystem where:
- Agents are automatically selected based on task context
- File paths are centralized in CLAUDE.md for easy maintenance  
- Each agent has clear, focused expertise
- The system scales without complexity explosion

## References

- [CLAUDE.md](../../CLAUDE.md) - Project-specific agent instructions
- [Agent Configuration](.claude/agents/) - Individual agent definitions
- [Comprehensive Development Workflow](../6_Guides/Comprehensive_Development_Workflow.md) - How agents fit into development