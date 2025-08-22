# The Elegant Persona Workflow (ADR-004 Final)

> The simplest possible workflow that leverages Claude Code's full automation capabilities

## Core Insight

**Claude Code can run scripts automatically!** This changes everything. The user just says "embody X" and the entire environment is configured automatically.

## The Three-Word Workflow

### For Users

```
embody [persona]     # Everything automated
/clear               # Reset context  
embody [next]        # Everything automated
```

That's the ENTIRE workflow from the user's perspective.

### What Actually Happens (Invisible Automation)

When you say `embody tech-lead`, Claude Code automatically:

1. **Runs embody.ps1 tech-lead**
   - Pulls latest from main
   - Checks for uncommitted work
   - Sets git identity
   - Shows repository state

2. **Loads Context Files**
   - `.claude/memory-bank/active/tech-lead.md`
   - Recent session log entries
   - Backlog items assigned to Tech Lead

3. **Executes Persona Commands**
   - Whatever the embodiment command specifies
   - Example: `git pull origin main --ff-only && git status`

4. **Presents Ready State**
   - Shows current work
   - Lists assigned items
   - Awaits direction

## The Natural Work Flow

### Morning Start
```
User: embody product-owner
Claude: [Runs everything, presents state]
User: "Define VS_004 for grid expansion"
Claude: [Works, updates context continuously]
```

### Midday Switch
```
User: "I'm switching to tech-lead"
Claude: "Saving Product Owner context... Ready to switch!"
User: /clear
User: embody tech-lead
Claude: [Runs everything, presents state]
User: "Review and break down VS_004"
Claude: [Works with full context from Product Owner]
```

### Afternoon Implementation
```
User: "Switching to dev-engineer"
Claude: "Saving Tech Lead decisions... Ready to switch!"
User: /clear
User: embody dev-engineer
Claude: [Runs everything, ready to implement]
```

## Why This is Elegant

### From User Perspective
- **Three words** to switch personas
- **No manual scripts** to run
- **No commands** to remember
- **Natural language** interaction

### From Claude's Perspective
- **Full automation** via Bash tool
- **Context preserved** automatically
- **Identity maintained** throughout session
- **Handoffs tracked** in session log

### From System Perspective
- **Git always synced** (embody.ps1 enforces)
- **Commits encouraged** (not forced)
- **Conflicts prevented** (frequent commits)
- **History clean** (squash merges on PR)

## The Complete Protocol

### 1. Embodiment Protocol

Each persona document should specify:

```markdown
## 🚀 Embodiment Command
When embodied, run: `git pull origin main --ff-only && git status`

## 🆔 Identity Management
When embodied as [Persona], I:
- Remember I am [Persona] throughout session
- Update `.claude/memory-bank/active/[persona].md` continuously
- Log important decisions to session-log.md
```

### 2. Context Management

```
.claude/memory-bank/
├── active/           # Per-persona working memory
│   ├── tech-lead.md
│   ├── dev-engineer.md
│   └── ...
└── session-log.md    # Chronological history

Backlog.md            # Shared truth (no duplication)
```

### 3. Switching Protocol

When user says "I'm switching to X":

```markdown
Claude automatically:
1. Updates current persona's active context
2. Adds entry to session-log.md
3. Says: "Ready to switch! Use /clear then embody [persona]"
```

## Success Metrics

✅ **Perfect workflow when:**
- User only types: `embody [persona]`
- Everything else is automated
- Context always preserved
- No manual script execution
- No merge conflicts

⚠️ **Something's wrong if:**
- User has to run scripts manually
- Context gets lost
- Merge conflicts happen
- Process feels heavy

## The Philosophy

> "Perfection is achieved not when there is nothing more to add, but when there is nothing left to take away." - Antoine de Saint-Exupéry

We've reached that perfection:
- **User interface**: Three words
- **Automation**: Complete
- **Complexity**: Hidden
- **Experience**: Natural

## Quick Reference

### Daily Commands (User)
```bash
embody tech-lead          # Start as Tech Lead
embody dev-engineer       # Switch to Dev Engineer  
embody test-specialist    # Switch to Test Specialist
```

### What Claude Does (Automatic)
```bash
Bash: ./scripts/persona/embody.ps1 [persona]
Read: .claude/memory-bank/active/[persona].md
Grep: "Owner: [Persona]" in Backlog.md
Bash: [embodiment command from persona doc]
```

### Context Files (Automatic Updates)
```
active/[persona].md    # Updated continuously during work
session-log.md         # Updated on switches and milestones
Backlog.md            # Updated as decisions are made
```

## The Beautiful Truth

The entire complex system of:
- Git identity management
- Context preservation  
- Conflict prevention
- Work tracking
- Handoff management

Is reduced to:

**`embody [persona]`**

That's the power of good automation. Complex systems, simple interfaces.