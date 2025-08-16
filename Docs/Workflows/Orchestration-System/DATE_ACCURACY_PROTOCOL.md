# Date Accuracy Protocol

**Created**: 2025-08-16  
**Purpose**: Ensure all agents use accurate system dates instead of LLM knowledge cutoff dates  
**Critical**: LLMs don't know the current date - always use bash

## 🚨 CRITICAL: Always Use Bash for Current Date

### The Problem
LLMs (including Claude) have a knowledge cutoff and cannot inherently know the current date. They may incorrectly guess dates based on context or patterns, leading to:
- Wrong dates in documentation
- Incorrect archive folder selection (wrong quarter/year)
- Misleading timestamps in reports
- Version control confusion

### The Solution: bash `date` Command

**MANDATORY**: When you need the current date for ANY purpose, use:

```bash
# Get current date and time
date

# Get date in specific format for filenames (YYYY_MM_DD)
date +"%Y_%m_%d"

# Get date for documentation (Month DD, YYYY)
date +"%B %d, %Y"

# Get quarter for archiving (Q1-Q4)
echo "Q$((($(date +%-m)-1)/3+1))"

# Get year-quarter combination (YYYY-QN)
echo "$(date +%Y)-Q$((($(date +%-m)-1)/3+1))"
```

## 📋 Required Date Check Scenarios

### 1. Creating New Documents
```python
# WRONG - Using LLM's guess
filename = "2025_01_16_Report.md"  # LLM might guess wrong month

# RIGHT - Using bash
import subprocess
date_str = subprocess.run(["date", "+%Y_%m_%d"], capture_output=True, text=True).stdout.strip()
filename = f"{date_str}_Report.md"
```

### 2. Updating Backlog/Documentation
```markdown
# WRONG
**Last Updated**: 2025-01-16  # LLM's incorrect guess

# RIGHT - Check with bash first
$ date +"%Y-%m-%d"
**Last Updated**: 2025-08-16  # Actual system date
```

### 3. Archive Folder Selection
```bash
# WRONG - Guessing quarter
archive_path = "archive/2025-Q1/"  # Could be wrong quarter

# RIGHT - Calculate from system date
QUARTER="$(date +%Y)-Q$((($(date +%-m)-1)/3+1))"
archive_path = "archive/$QUARTER/"
```

### 4. Work Item Creation
```yaml
# WRONG
Created: 2025-01-16  # LLM's guess

# RIGHT
Created: $(date +"%Y-%m-%d")  # System date
```

## 🤖 Agent-Specific Implementation

### Main Orchestrator (Claude Code)
- Run `date` at session start to establish current date
- Use for all document creation
- Verify dates before committing

### Backlog Maintainer
- Check date when archiving items
- Use correct quarter for archive folders
- Timestamp all operations with system time

### Product Owner
- Date stamp all new work items
- Use system date for planning cycles
- Track velocity with accurate dates

### All Agents
- Never assume current date from context
- Always verify with bash when date matters
- Log operations with system timestamps

## 🔧 Implementation Examples

### PowerShell (Windows)
```powershell
# Get current date
Get-Date

# Format for filenames
Get-Date -Format "yyyy_MM_dd"

# Get quarter
[int]((Get-Date).Month - 1) / 3 + 1
```

### Python Script Integration
```python
import subprocess
from datetime import datetime

def get_system_date():
    """Always get actual system date, not LLM's guess"""
    result = subprocess.run(['date'], capture_output=True, text=True)
    return result.stdout.strip()

def get_formatted_date(format_string="%Y-%m-%d"):
    """Get system date in specific format"""
    cmd = f"date +'{format_string}'"
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    return result.stdout.strip()
```

## ⚠️ Common Mistakes to Avoid

1. **Don't trust LLM date knowledge**
   - LLMs may say "today is [date]" but they're guessing
   - Always verify with system

2. **Don't hardcode dates**
   - Even if user mentions a date, verify if it's "today"
   - Use relative date calculations when possible

3. **Don't assume date from context**
   - File modification times might be old
   - Git commits might be from different days
   - Always check current system date

## 📝 Verification Checklist

Before any date-sensitive operation:
- [ ] Did I check system date with bash?
- [ ] Am I using the correct format for this context?
- [ ] Will this date be accurate tomorrow?
- [ ] Have I avoided hardcoding dates?
- [ ] Is the quarter calculation correct?

## 🔗 References
- [Anthropic Bash Tool Documentation](https://docs.anthropic.com/en/docs/agents-and-tools/tool-use/bash-tool)
- [Backlog Archive Naming Convention](../../Shared/Core/Style-Standards/Archive_Naming_Convention.md)
- Environment shows date but LLM knowledge is separate

## Critical Reminder

**The LLM (Claude) has a knowledge cutoff and cannot know the current date without checking the system.**

Always use:
```bash
date  # This is the truth
```

Never trust:
- LLM's assumption about current date
- Context clues about what "today" is
- Previous messages about dates

**System date via bash is the ONLY reliable source.**