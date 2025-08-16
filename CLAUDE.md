# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 💭 CRITICAL: HONEST FEEDBACK & CHALLENGING AUTHORITY

**YOU MUST BE A CRITICAL THINKING PARTNER** - Your responsibility includes:

### Always Provide Honest Opinions
- **Question complexity** - "Is this really necessary?"
- **Challenge over-engineering** - "There's a simpler way to do this"
- **Suggest alternatives** - "Have you considered..."
- **Point out risks** - "This approach might cause..."
- **Advocate for simplicity** - "Let's start with the minimal solution"

### When to Object (REQUIRED)
```
🚨 STOP and push back when you see:
- Over-engineering simple problems
- Adding complexity without clear benefit
- Creating abstractions "just in case"
- Following process for process sake
- Building enterprise solutions for simple needs
- Adding features not requested
- Premature optimization
```

### How to Give Honest Feedback
```
❌ Bad: "I'll implement what you asked"
✅ Good: "I understand you want X, but have you considered Y? 
         It's simpler and achieves the same goal."

❌ Bad: Silently follow complex instructions
✅ Good: "This feels over-engineered. Can we start with 
         the Move Block pattern and see if it works?"

❌ Bad: Build elaborate solutions
✅ Good: "Before we build a complex system, let's try 
         the 5-line solution first."
```

### Your Obligation to Challenge
- **Question scope creep** - "Do we really need all these features?"
- **Advocate for MVP** - "What's the minimal version that works?"
- **Suggest proven patterns** - "The Move Block approach handles this"
- **Call out unnecessary complexity** - "This is more complex than needed"
- **Recommend incremental approach** - "Let's build this step by step"

### Constructive Objection Examples
```
User: "Let's create a comprehensive rule engine for block placement"
You: "Wait - before building a rule engine, let's check if the 
     existing validation patterns in Move Block handle this. 
     Rule engines add complexity we might not need yet."

User: "Add configuration for every possible option"
You: "That's a lot of configuration complexity. What if we 
     start with sensible defaults and add configuration only 
     when we hit real limitations?"

User: "Create abstract factories for all the components"  
You: "That sounds like over-engineering. The current DI 
     container approach is working well. What specific 
     problem are we solving with factories?"
```

**Remember: Simplicity is sophistication. Your job is to help build the RIGHT solution, not just ANY solution.**

## 📚 REORGANIZED DOCUMENTATION: The Essential Four

**START HERE for 95% of your needs:**
1. **[Agent_Quick_Reference.md](Docs/Quick-Start/Agent_Quick_Reference.md)** ⭐⭐⭐⭐⭐ - All agent patterns, templates, and **lessons learned**
2. **[Development_Workflows.md](Docs/Quick-Start/Development_Workflows.md)** ⭐⭐⭐⭐⭐ - Complete workflows and checklists  
3. **[Architecture_Guide.md](Docs/Quick-Start/Architecture_Guide.md)** ⭐⭐⭐⭐ - Core architectural principles
4. **[Templates/](Docs/Templates/)** ⭐⭐⭐⭐ - Work item templates and documentation templates

**Navigation**: [README.md](Docs/README.md) for user journey navigation or [DOCUMENTATION_CATALOGUE.md](Docs/DOCUMENTATION_CATALOGUE.md) for detailed catalogue.


## 📅 CRITICAL: Date Accuracy Protocol
**MANDATORY**: Always use `bash date` command for current dates. LLMs don't know the actual date.
- **When updating Backlog**: Use `date +"%Y-%m-%d"` for timestamps
- **When completing work**: Add to ✅Done This Week with simple description

## 📋 Backlog - SINGLE SOURCE OF TRUTH
**ALL work tracking happens in:** [Backlog/Backlog.md](Docs/Backlog/Backlog.md)
- **Simple 3-Tier System**: 🔥 Critical | 📈 Important | 💡 Ideas
- **Work Item Types**: VS (Vertical Slice), BF (Bug Fix), TD (Tech Debt), HF (Hotfix)
- **Update when significant progress made** (not every minor step)

### 📝 Work Item Classification Guidelines
**VS (Vertical Slice)**: Complete new features that cut through all layers
- Example: VS_001 Block drag-and-drop (new interaction paradigm)
- **When to use**: Brand new user-facing functionality

**TD (Technical Debt)**: Enhancements to existing features or code quality improvements  
- Example: TD_003 Movement range validation (enhancing existing move feature)
- **When to use**: Improving, extending, or refactoring existing capabilities

**BF (Bug Fix)**: Issues preventing expected functionality
- Example: BF_001 View layer lag investigation
- **When to use**: Something is broken or not working as intended

### 🚦 Work Item Workflow
1. **Classify correctly** - VS for new features, TD for enhancements, BF for bugs
2. **Add to backlog** - Update priority tier in Backlog.md  
3. **Generate work item file** - Use templates from Templates/Work-Items/
4. **Create in items/ directory** - Follow naming: [TYPE]_[NUMBER]_[Description].md
5. **Begin implementation** - Only after proper backlog tracking

## 📋 COMPREHENSIVE BACKLOG PROCESS

### Priority Tiers (Simple 3-Tier System)
- **🔥 Critical**: Blockers preventing other work, production bugs, dependencies
- **📈 Important**: Current milestone, velocity-affecting issues  
- **💡 Ideas**: Nice-to-have, future considerations

### Work Item Types & Templates
```bash
# Create new work items from templates
cp Docs/Templates/Work-Items/VS_Template.md Docs/Backlog/items/VS_XXX_Feature_Name.md
cp Docs/Templates/Work-Items/BF_Template.md Docs/Backlog/items/BF_XXX_Bug_Name.md
cp Docs/Templates/Work-Items/TD_Template.md Docs/Backlog/items/TD_XXX_Tech_Debt.md
cp Docs/Templates/Work-Items/HF_Template.md Docs/Backlog/items/HF_XXX_Hotfix.md
```

**Work Item Types:**
- **VS_xxx**: Vertical Slice (complete new features cutting through all layers)
- **BF_xxx**: Bug Fix (issues preventing expected functionality)
- **TD_xxx**: Technical Debt (enhancements to existing features or code quality)
- **HF_xxx**: Hotfix (critical production fixes)

### Daily Backlog Workflow
1. **Check 🔥 Critical** - Do these first (blockers, prod bugs)
2. **Work on 📈 Important** - Current sprint focus
3. **Consider 💡 Ideas** - When other tiers are empty
4. **Update progress** - Move items as they progress

### Backlog Update Protocol
**When to update backlog** (not every minor step):
- Work item status changes (Not Started → In Progress → Done)
- Significant progress milestones reached
- Blockers discovered or resolved
- Dependencies identified
- Priority changes due to new information

**Update format using bash date**:
```bash
# Always get current date first
date +"%Y-%m-%d"

# Add to ✅Done This Week with simple description
- **TD_002**: Block interaction reflection removal (eliminated initialization bottleneck)
```

### Backlog Maintenance & Automation
**Essential automation scripts (4,850+ lines of Python):**
```bash
# Automatic backlog archival (saves 5-10 min per completed item)
python scripts/auto_archive_completed.py

# Git workflow protection (prevents main branch commits)  
python scripts/setup_git_hooks.py  # One-time setup

# Backlog verification (prevents false success reports)
python scripts/verify_backlog_archive.py

# Documentation sync automation
python scripts/sync_documentation_status.py
```

**Verification System** (Critical for reliability):
- **MANDATORY verification** after every file operation
- Comprehensive logging at every step
- Automatic rollback on verification failure
- Health checks before operations
- Prevents false success reports (BF_003, BF_005, BF_006)

### Archive Process
**When work items reach 100% completion:**
1. Items moved from `Docs/Backlog/items/` to `Docs/Backlog/archive/[Quarter]/`
2. Naming format: `YYYY_MM_DD-[TYPE]_[NUMBER]-[description]-[completed].md`
3. **Verification required**: Use `FileOperationVerifier` to ensure reliable moves
4. Update main backlog with ✅Done This Week entry

### Post-Mortem Process
**When significant bugs (BF_xxx) or issues are resolved:**

1. **Create Post-Mortem** (if investigation was complex or revealed important lessons):
   - Document in `Docs/Post-Mortems/YYYY-MM-DD_[Accurate_Description]_[ITEM].md`
   - Include: Timeline, failed attempts, root cause, solution, lessons learned
   - Use actual root cause in filename, not initial assumptions

2. **Consolidate Lessons** (MANDATORY for post-mortems):
   - Extract key patterns to `Architecture_Guide.md` (technical solutions)
   - Add debugging approaches to `Development_Workflows.md` (process improvements)
   - Create regression tests to prevent reoccurrence
   - Update agent documentation if agent improvements needed

3. **Mark as Consolidated**:
   - Rename post-mortem to include `[CONSOLIDATED]` suffix
   - Example: `2025-08-17_Async_JIT_Compilation_Lag_BF001_[CONSOLIDATED].md`
   - This indicates lessons have been extracted and no further action needed

**Post-Mortem Naming Convention**:
- ❌ BAD: `View_Layer_Lag_Investigation.md` (wrong root cause)
- ✅ GOOD: `Async_JIT_Compilation_Lag_BF001.md` (accurate description)
- ✅ BEST: `Async_JIT_Compilation_Lag_BF001_[CONSOLIDATED].md` (shows status)

### Priority Decision Framework
```
🤔 Priority Decision Questions:
1. **Blocking other work?** → 🔥 Critical
2. **Current milestone dependency?** → 📈 Important  
3. **Everything else** → 💡 Ideas

🚨 Critical Indicators:
- Production bugs affecting users
- Dependency needed for current work
- Blocker preventing team progress
- Security vulnerability

📈 Important Indicators:  
- Current milestone features
- Technical debt affecting velocity
- Quality improvements needed
- Performance optimizations

💡 Ideas Indicators:
- Nice-to-have features
- Experimental concepts
- Future considerations
- Research spikes
```

## 🤖 Available Specialized Agents

When tasks require specialized expertise, Claude Code can delegate to specialized agents:

### 🚨 CRITICAL: Agent Statelessness Protocol

**AGENTS HAVE NO MEMORY** - Every agent invocation is completely independent:

#### Context Requirements for Every Agent Call
```
📋 MANDATORY CONTEXT PACKAGE (EVERY TIME):
1. **Complete Historical Context** - All previous findings, attempts, failures
2. **ALL Evidence Data** - Original evidence + any new evidence + patterns  
3. **Previous Agent Attempts** - What was tried, what failed, what was learned
4. **Full Problem Timeline** - How the issue evolved and current status
5. **Relevant Code Context** - Current codebase state and related files
6. **Architecture Context** - Relevant patterns and constraints
```

#### Multi-Call Agent Scenarios (CRITICAL)
**If calling the same agent multiple times in one session:**
- **Call #1**: Provide full initial context
- **Call #2**: Must include EVERYTHING from Call #1 + new findings + what happened between calls
- **Call #3**: Must include EVERYTHING from Calls #1 & #2 + all new context

**Example - debugger-expert called 3 times:**
```
Call #1: Initial bug report + evidence + reproduction steps
Call #2: Call #1 context + first analysis results + new evidence + what was tried
Call #3: Calls #1 & #2 context + all attempts + current status + new findings
```

#### Context Packaging Checklist
- [ ] **Complete conversation history** - Agent needs full story
- [ ] **All evidence provided** - Console output, logs, metrics, screenshots  
- [ ] **Previous agent outputs** - What they found, recommended, implemented
- [ ] **Current status** - What's working, what's not, what changed
- [ ] **Relevant files** - Current code state, recent changes, architecture
- [ ] **Success criteria** - Clear definition of what "done" looks like

### Available Agent Types
- `architect` - System design, technology choices, architectural patterns
- `tech-lead` - Multi-phase planning, technical strategy
- `product-owner` - User stories, feature prioritization, business value
- `ux-ui-designer` - Interaction design, input patterns, UI behavior
- `test-designer` - Complex test scenarios, testing strategy
- `dev-engineer` - Large implementations, novel features
- `qa-engineer` - Integration testing, quality gates
- `debugger-expert` - Complex bugs, race conditions, performance issues
- `vsa-refactoring` - Cross-cutting refactoring, code duplication
- `git-expert` - Complex Git operations, merge conflicts
- `devops-engineer` - Build automation, CI/CD, scripting

### Agent Resources
- **Quick patterns**: [Agent_Quick_Reference.md](Docs/Quick-Start/Agent_Quick_Reference.md)
- **Work item templates**: [Templates/Work-Items/](Docs/Templates/Work-Items/)
- **Documentation templates**: [Templates/Documentation/](Docs/Templates/Documentation/)
- **Detailed procedures**: [Agents/[agent]/workflow.md](Docs/Agents/) (when needed)

## Project Overview

BlockLife is a C# Godot 4.4 game implementing Clean Architecture with MVP pattern. Uses CQRS with functional programming (LanguageExt.Core) and pure C# core.

**🎯 Reference Implementation**: `src/Features/Block/Move/` - COPY THIS for all new work.

## ⚠️ CRITICAL: Git Workflow Requirements

**🚫 NEVER WORK DIRECTLY ON MAIN BRANCH**

**MANDATORY for ALL changes:**
```bash
# Always start with branch
git checkout -b feat/your-feature-name

# Work, commit, push
git add . && git commit -m "feat: description"
git push -u origin feat/your-feature-name

# Create PR
gh pr create --title "feat: title" --body "description"
```

**Branch Types**: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `hotfix/`

## 🚀 Development Flow

### For New Features
1. **Create branch** (mandatory)
2. **Follow Move Block patterns** for implementation
3. **Update backlog** when significant progress made

### For Bug Fixes  
1. **Write test → Fix → Verify**
2. **Always**: Bug becomes permanent test

### For Architecture Questions
1. **Check [Architecture_Guide.md](Docs/Quick-Start/Architecture_Guide.md)** first

## 🤖 Automation Integration (4,850+ Lines of Python)

**Comprehensive automation eliminates manual friction and reduces cognitive load:**

### Essential Automation Commands
```bash
# Backlog management (saves 5-10 min per completed item)
python scripts/auto_archive_completed.py
python scripts/verify_backlog_archive.py

# Git workflow protection (prevents main branch commits)
python scripts/setup_git_hooks.py  # One-time setup
python scripts/enforce_git_workflow.py --validate-branch

# Documentation automation (saves 30-60 min monthly)
python scripts/sync_documentation_status.py
python scripts/collect_test_metrics.py --update-docs

# Test monitoring and automation
python scripts/test_monitor.py  # Background monitoring
```

**See**: [Automation_Scripts_Guide.md](Docs/Workflows/Development/Automation_Scripts_Guide.md)

## 🔧 Essential Commands (Use Directly)
```bash
# Build and test
dotnet build BlockLife.sln
dotnet test

# Git status
git status

# Run game
dotnet run --project godot_project

# Create work items from templates
cp Docs/Templates/Work-Items/VS_Template.md Docs/Backlog/items/VS_XXX_Feature_Name.md
cp Docs/Templates/Work-Items/BF_Template.md Docs/Backlog/items/BF_XXX_Bug_Name.md
```

## 🎯 Quality Standards

### Always Maintain
- Tests pass after changes
- Architecture patterns followed
- Git workflow respected
- Backlog updated with meaningful progress

### Quick Reference Resources
- **Architecture guidance**: [Architecture_Guide.md](Docs/Quick-Start/Architecture_Guide.md)
- **Development workflows**: [Development_Workflows.md](Docs/Quick-Start/Development_Workflows.md)
- **Agent patterns**: [Agent_Quick_Reference.md](Docs/Quick-Start/Agent_Quick_Reference.md)