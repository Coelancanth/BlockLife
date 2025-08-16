#!/bin/bash

# Simple script to get the current agent detail setting
# Returns "true" or "false"
# Used by the orchestration system to determine display mode

CONFIG_FILE=".claude/agent-details-config.json"

if [ -f "$CONFIG_FILE" ]; then
    cat "$CONFIG_FILE" | grep -o '"agent_details_enabled": *[^,}]*' | sed 's/"agent_details_enabled": *//' | tr -d '"'
else
    echo "false"
fi