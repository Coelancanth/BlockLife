# BlockLife Documentation

Welcome to the BlockLife documentation. This guide helps you navigate our organized documentation structure.

## 📂 Documentation Structure

### [01-Active/](01-Active/) - Daily Working Documents
- **[Backlog.md](01-Active/Backlog.md)** - Current work items and priorities
- **[Workflow.md](01-Active/Workflow.md)** - Development workflow and processes

### [02-Design/](02-Design/) - Game Design & Vision
- **[Vision.md](02-Design/Vision.md)** - Complete consolidated game design vision
- **[Mechanics/](02-Design/Mechanics/)** - Detailed game mechanics

### [03-Reference/](03-Reference/) - Technical Guidelines
- **[Architecture.md](03-Reference/Architecture.md)** - Clean Architecture principles
- **[Patterns.md](03-Reference/Patterns.md)** - Code patterns and examples
- **[Standards.md](03-Reference/Standards.md)** - Naming and coding standards
- **[Testing.md](03-Reference/Testing.md)** - Testing strategies
- **[GitWorkflow.md](03-Reference/GitWorkflow.md)** - Git procedures (MANDATORY)
- **[ThinSliceProtocol.md](03-Reference/ThinSliceProtocol.md)** - VS sizing rules
- **[Context7/](03-Reference/Context7/)** - Library documentation guides
- **[SubagentVerification.md](03-Reference/SubagentVerification.md)** ⭐ - Trust but Verify protocol
- **[ClaudeCodeBestPractices.md](03-Reference/ClaudeCodeBestPractices.md)** ⭐ - Community patterns to adopt

### [04-Personas/](04-Personas/) - AI Persona Definitions
- Individual persona files for each role
- **[strategic-prioritizer/](04-Personas/strategic-prioritizer/)** - Prioritizer with analysis docs

### [05-Templates/](05-Templates/) - Document Templates
- Vertical Slice, Bug Report, Technical Debt, Post-Mortem templates

### [06-PostMortems/](06-PostMortems/) - Learning from Issues
- **[Active/](06-PostMortems/Active/)** - Recent post-mortems
- **[Archive/](06-PostMortems/Archive/)** - Historical post-mortems

### [07-Archive/](07-Archive/) - Completed Work
- **[Completed_Backlog.md](07-Archive/Completed_Backlog.md)** - Finished work items
- **[Historical/](07-Archive/Historical/)** - Deprecated documentation

## 🚀 Quick Start

### For New Contributors
1. **NEW**: Check `.claude/memory-bank/activeContext.md` - Current session state
2. Read [Workflow.md](01-Active/Workflow.md) - Understand the development process
3. Check [Backlog.md](01-Active/Backlog.md) - See current priorities
4. Review [Architecture.md](03-Reference/Architecture.md) - Understand the codebase

### For AI Personas
1. **NEW**: Check `.claude/memory-bank/` for context first
2. Your definition is in [04-Personas/](04-Personas/)
3. Check [Backlog.md](01-Active/Backlog.md) for your assigned items
4. Reference [02-Design/Vision.md](02-Design/Vision.md) for game design
5. Follow [GitWorkflow.md](03-Reference/GitWorkflow.md) religiously
6. **NEW**: Verify subagent work with [SubagentVerification.md](03-Reference/SubagentVerification.md)

### For Game Design
1. [Vision.md](02-Design/Vision.md) - Primary design document
2. [Mechanics/](02-Design/Mechanics/) - Detailed specifications
3. User is the Game Designer - these are reference only

## 📋 Document Maintenance

- **Backlog**: Updated continuously during development
- **Post-Mortems**: Created after significant issues
- **Archive**: Updated when work completes
- **Design**: Only updated by the user (Game Designer)

## 🔍 Finding Information

### By Purpose
- **What to work on?** → [01-Active/Backlog.md](01-Active/Backlog.md)
- **How to work?** → [01-Active/Workflow.md](01-Active/Workflow.md)
- **Game mechanics?** → [02-Design/](02-Design/)
- **Code patterns?** → [03-Reference/](03-Reference/)
- **Past issues?** → [06-PostMortems/](06-PostMortems/)

### By Role
- **Product Owner** → Design docs, then create VS items
- **Tech Lead** → Reference docs for architecture decisions
- **Dev Engineer** → Patterns and standards for implementation
- **Test Specialist** → Testing strategies and bug templates

## 📝 Key Principles

1. **Numbered folders** indicate priority/frequency of use
2. **Each folder** has a single, clear purpose
3. **Active docs** are kept minimal and current
4. **Archive** preserves history without cluttering

---

*Navigate with purpose. Build with clarity. Learn from history.*