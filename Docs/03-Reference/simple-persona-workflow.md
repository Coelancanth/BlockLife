# Simple Persona Workflow (ADR-004)

> The simplest effective workflow for solo dev with personas

## Core Philosophy

**One script, one purpose:**
- `embody.ps1`: Handles EVERYTHING needed to start as a persona
- No redundant "switch" script needed
- Claude manages context updates during work

## The Complete Workflow

### 🌅 Starting Fresh

```bash
# Check where you left off
git status
git log --oneline -3

# Embody your starting persona
./scripts/persona/embody.ps1 tech-lead
```

```bash
# In Claude
/clear
embody tech-lead
```

### 💼 Working as Persona

**Claude continuously:**
- Updates `.claude/memory-bank/active/tech-lead.md` as decisions are made
- Knows its identity throughout session
- Commits every 20-30 minutes (through user)

**Example work session:**
```markdown
Claude: "As Tech Lead, I'm approving TD_014 with complexity 3"
*Updates active/tech-lead.md automatically*

User: "commit this decision"
Claude: "git add -A && git commit -m 'decision: approve TD_014 complexity 3'"
```

### 🔄 Switching Personas (The Simple Way)

**User decides to switch:**
```
User: "I'm switching to dev-engineer"
```

**Claude responds:**
```markdown
Claude: "Let me prepare for the switch to Dev Engineer...

📝 Updating Tech Lead context...
✅ Updated: .claude/memory-bank/active/tech-lead.md

📜 Adding session log entry...
✅ Logged: Handoff from Tech Lead to Dev Engineer

Ready to switch! Please:
1. Run: ./scripts/persona/embody.ps1 dev-engineer
2. Then: /clear
3. Tell me: embody dev-engineer"
```

**User executes:**
```bash
# Terminal
./scripts/persona/embody.ps1 dev-engineer
# Shows any uncommitted work, pulls latest, sets identity

# Claude
/clear
embody dev-engineer
# Fresh context with new persona
```

## Why This is Better

### No switch-persona.ps1 Needed

**What we thought we needed:**
- Script to update context ❌ Claude does this
- Script to log session ❌ Claude does this  
- Script to check commits ❌ embody.ps1 does this
- Script to call embody ❌ User calls directly

**What we actually need:**
- embody.ps1 to set everything up ✅
- Claude to maintain context ✅
- User to commit frequently ✅

### The Beauty of Simplicity

1. **One script per action**: Embody = embody.ps1
2. **Context updates are natural**: Claude updates as it works
3. **No forced workflows**: Commit when ready, not when switching
4. **Clear mental model**: Script sets up, Claude maintains, user controls

## Quick Reference Card

### Daily Commands

```bash
# Start work
./scripts/persona/embody.ps1 tech-lead

# Commit progress
git add -A && git commit -m "type: description"
git push  # After 2-3 commits

# Switch personas
./scripts/persona/embody.ps1 dev-engineer
# Then /clear and embody in Claude
```

### Claude Commands

```markdown
# Start session
embody tech-lead

# During work (Claude does automatically)
*Updates active/tech-lead.md with decisions*

# Before switching
"I'm switching to dev-engineer"
*Claude prepares handoff*
```

## The 3-Step Switch

1. **Tell Claude**: "I'm switching to [persona]"
2. **Run embody**: `./scripts/persona/embody.ps1 [persona]`  
3. **Reset Claude**: `/clear` then `embody [persona]`

That's it. No complex scripts, no forced workflows, just simple and effective.

## Success Metrics

✅ **You're doing it right when:**
- Switches take < 30 seconds
- Context is always current
- No merge conflicts
- Session log tells clear story
- You forget about the scripts and just work

⚠️ **Warning signs:**
- Creating more scripts to "help"
- Complex multi-step processes
- Forgetting which persona you are
- Lost context between sessions

## Remember

> "Perfection is achieved not when there is nothing more to add, but when there is nothing left to take away." - Antoine de Saint-Exupéry

The switch-persona.ps1 script was something to take away. Now we have the simple, elegant workflow that was always there.