---
description: "Display current active persona for status line integration"
allowed_tools: ["bash"]
---

```bash
# Read current persona state, default to 'none' if not set
cat .claude/current-persona 2>/dev/null || echo "none"
```