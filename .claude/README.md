# Claude Code Custom Commands

This directory contains custom slash commands for the BlockLife project's agent orchestration system.

## Available Commands

### `/agent-details [on|off]`
Toggle full agent prompt display on or off.

**Usage:**
- `/agent-details on` - Enable full agent prompt display
- `/agent-details off` - Disable full agent prompt display 
- `/agent-details` - Show current setting

**Examples:**
```
/agent-details on    # Shows: Agent details set to: true
/agent-details off   # Shows: Agent details set to: false
/agent-details       # Shows current status
```

### `/agent-status`
Show current agent detail settings and system status.

**Usage:**
- `/agent-status` - Display comprehensive status information

**Example output:**
```
=== Agent Detail Settings ===
Configuration file: .claude/agent-details-config.json
Current setting: false
Last updated: 2025-08-16T08:14:48Z
File exists: Yes
File size: 73 bytes
```

## Implementation Details

### Files Created:
- `.claude/commands/agent-details.md` - Agent details toggle command
- `.claude/commands/agent-status.md` - Agent status display command
- `.claude/manage-agent-config.sh` - Configuration management script
- `.claude/get-agent-detail-setting.sh` - Simple getter for orchestration system
- `.claude/agent-details-config.json` - Persistent configuration storage

### Configuration Format:
```json
{
  "agent_details_enabled": false,
  "last_updated": "2025-08-16T08:14:48Z"
}
```

### Integration with Orchestration System:
The orchestration system can check the current setting using:
```bash
.claude/get-agent-detail-setting.sh
```

This returns either "true" or "false" for use in conditional logic.

## Technical Notes

- Commands use bash scripts for cross-platform compatibility
- Settings persist between Claude Code sessions
- Configuration file is automatically created on first use
- Scripts handle error cases gracefully (missing files, invalid input)
- Follows Claude Code custom command format with proper frontmatter

## Workflow Integration

These commands integrate with the Automatic Orchestration Pattern by allowing users to control the verbosity of agent outputs:

- **Details ON**: Full agent prompts and context displayed
- **Details OFF**: Brief summaries only, reducing cognitive load

This supports the principle of "cognitive load reduction" while maintaining transparency when full details are needed for debugging or learning purposes.