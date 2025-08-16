---
allowed-tools: Bash(.claude/manage-agent-config.sh:*)
argument-hint: [on|off]
description: Toggle full agent prompt display on or off
---

You are implementing an agent detail toggle feature for the BlockLife project's orchestration system.

Arguments: $ARGUMENTS

If the user provided an argument, use the configuration script to set the agent details mode:

- For "on": Execute `!.claude/manage-agent-config.sh set on`
- For "off": Execute `!.claude/manage-agent-config.sh set off`
- For no argument or "status": Execute `!.claude/manage-agent-config.sh status`

The configuration script handles all the logic for storing and retrieving settings.

Provide clear feedback about what was changed and the current state.