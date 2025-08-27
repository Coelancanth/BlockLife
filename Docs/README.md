# BlockLife Documentation

Welcome to the BlockLife documentation. This guide helps you navigate our organized documentation structure.

## 📂 Documentation Structure

### [01-Active/](01-Active/) - Daily Working Documents
- **[Backlog.md](01-Active/Backlog.md)** - Current work items and priorities (TD focus)
- **[Backup.md](01-Active/Backup.md)** - VS content storage (prevents backlog bloat)
- **[Workflow.md](01-Active/Workflow.md)** - Development workflow and processes
- **[README.md](01-Active/README.md)** - Navigation guide for active documents

### [02-Design/](02-Design/) - Game Design & Vision
- **[Vision.md](02-Design/Vision.md)** - Complete consolidated game design vision
- **[README.md](02-Design/README.md)** - Design directory navigation

### [03-Reference/](03-Reference/) - Current Technical Guidelines
- **[HANDBOOK.md](03-Reference/HANDBOOK.md)** ⭐⭐⭐⭐⭐ - Daily companion (architecture, workflow, testing, patterns)
- **[Glossary.md](03-Reference/Glossary.md)** ⭐⭐⭐⭐⭐ - Authoritative vocabulary for all code
- **[CurrentImplementationStatus.md](01-Active/CurrentImplementationStatus.md)** - Implementation status vs. vision (SSOT)
- **[ADR/](03-Reference/ADR/)** - Architecture Decision Records
- **[README.md](03-Reference/README.md)** - Reference directory navigation

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
- **[Future-Ideas/](07-Archive/Future-Ideas/)** - Future feature concepts
- **[Historical/](07-Archive/Historical/)** - Deprecated documentation

### [99-Deprecated/](99-Deprecated/) - Legacy Documentation
- **[03-Reference/](99-Deprecated/03-Reference/)** - Old reference documentation (Architecture.md, Testing.md, GitWorkflow.md, etc.)
- **[README.md](99-Deprecated/README.md)** - Deprecation information

## 🚀 Quick Start

### For New Contributors
1. Check `.claude/memory-bank/active/[persona].md` - Current persona context
2. Read [HANDBOOK.md](03-Reference/HANDBOOK.md) - Essential daily companion
3. Check [Backlog.md](01-Active/Backlog.md) - Current TD items and priorities
4. Review [Workflow.md](01-Active/Workflow.md) - Development processes

### For AI Personas  
1. Check `.claude/memory-bank/active/[persona].md` and `session-log.md` first
2. Your definition is in [04-Personas/](04-Personas/)
3. Check [Backlog.md](01-Active/Backlog.md) for TD items you own
4. Reference [Glossary.md](03-Reference/Glossary.md) before naming anything
5. **Legacy docs**: Architecture, Testing, GitWorkflow in [99-Deprecated/03-Reference/](99-Deprecated/03-Reference/)
6. Use [HANDBOOK.md](03-Reference/HANDBOOK.md) as primary technical reference

### For Game Design
1. [Vision.md](02-Design/Vision.md) - Primary design document  
2. User is the Game Designer - this is reference only

## 📋 Document Maintenance

- **Backlog**: Updated continuously during development
- **Post-Mortems**: Created after significant issues
- **Archive**: Updated when work completes
- **Design**: Only updated by the user (Game Designer)

## 🔍 Finding Information

### By Purpose
- **What to work on?** → [01-Active/Backlog.md](01-Active/Backlog.md)
- **How to work?** → [01-Active/Workflow.md](01-Active/Workflow.md) + [03-Reference/HANDBOOK.md](03-Reference/HANDBOOK.md)
- **Game mechanics?** → [02-Design/Vision.md](02-Design/Vision.md)
- **Technical guidance?** → [03-Reference/HANDBOOK.md](03-Reference/HANDBOOK.md)
- **Past issues?** → [06-PostMortems/](06-PostMortems/)
- **Legacy docs?** → [99-Deprecated/03-Reference/](99-Deprecated/03-Reference/)

### By Role
- **Product Owner** → Design docs, then create VS items
- **Tech Lead** → HANDBOOK.md for architecture decisions  
- **Dev Engineer** → HANDBOOK.md for patterns and standards
- **Test Specialist** → Templates and post-mortem analysis

## 📝 Key Principles

1. **Numbered folders** indicate priority/frequency of use
2. **Each folder** has a single, clear purpose
3. **Active docs** are kept minimal and current
4. **Archive** preserves history without cluttering

---

*Navigate with purpose. Build with clarity. Learn from history.*