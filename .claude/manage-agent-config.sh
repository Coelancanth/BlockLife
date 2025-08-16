#!/bin/bash

# Agent Detail Configuration Manager
# Usage: ./manage-agent-config.sh [get|set|status] [value]

CONFIG_FILE=".claude/agent-details-config.json"
CONFIG_DIR=".claude"

# Ensure config directory exists
mkdir -p "$CONFIG_DIR"

# Initialize config file if it doesn't exist
if [ ! -f "$CONFIG_FILE" ]; then
    echo '{"agent_details_enabled": false, "last_updated": "'$(date -u +"%Y-%m-%dT%H:%M:%SZ")'"}' > "$CONFIG_FILE"
fi

case "$1" in
    "get")
        if [ -f "$CONFIG_FILE" ]; then
            cat "$CONFIG_FILE" | grep -o '"agent_details_enabled": *[^,}]*' | sed 's/"agent_details_enabled": *//' | tr -d '"'
        else
            echo "false"
        fi
        ;;
    "set")
        VALUE="$2"
        if [ "$VALUE" = "on" ] || [ "$VALUE" = "true" ]; then
            ENABLED="true"
        else
            ENABLED="false"
        fi
        echo '{"agent_details_enabled": '$ENABLED', "last_updated": "'$(date -u +"%Y-%m-%dT%H:%M:%SZ")'"}' > "$CONFIG_FILE"
        echo "Agent details set to: $ENABLED"
        ;;
    "status")
        if [ -f "$CONFIG_FILE" ]; then
            echo "=== Agent Detail Settings ==="
            echo "Configuration file: $CONFIG_FILE"
            echo "Current setting: $(cat "$CONFIG_FILE" | grep -o '"agent_details_enabled": *[^,}]*' | sed 's/"agent_details_enabled": *//' | tr -d '"')"
            echo "Last updated: $(cat "$CONFIG_FILE" | grep -o '"last_updated": *"[^"]*"' | sed 's/"last_updated": *//' | tr -d '"')"
            echo "File exists: Yes"
            echo "File size: $(wc -c < "$CONFIG_FILE") bytes"
        else
            echo "=== Agent Detail Settings ==="
            echo "Configuration file: $CONFIG_FILE"
            echo "Current setting: false (default)"
            echo "Last updated: Never"
            echo "File exists: No"
        fi
        ;;
    *)
        echo "Usage: $0 [get|set|status] [value]"
        echo "  get      - Get current setting"
        echo "  set on   - Enable agent details"
        echo "  set off  - Disable agent details"
        echo "  status   - Show detailed status"
        exit 1
        ;;
esac