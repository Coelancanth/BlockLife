# Agent Verification Checklist

## Quick Verification Commands

### After Backlog-Maintainer Triggers

#### Archive Operations
```powershell
# Verify files moved to archive
Get-ChildItem "C:\Users\Coel\Documents\Godot\blocklife\Docs\Backlog\archive\completed\2025-Q1\"

# Verify files removed from items
Get-ChildItem "C:\Users\Coel\Documents\Godot\blocklife\Docs\Backlog\items\" | Where-Object {$_.Name -like "*TD_012*"}

# Should return nothing if properly archived
```

#### Status Updates
```powershell
# Check Backlog.md for status change
Select-String -Path "C:\Users\Coel\Documents\Godot\blocklife\Docs\Backlog\Backlog.md" -Pattern "VS_000.*100%"
```

#### New Item Creation
```powershell
# Verify new item file exists
Test-Path "C:\Users\Coel\Documents\Godot\blocklife\Docs\Backlog\items\BF_003_*.md"
```

### After Tech-Lead Triggers

#### Implementation Plan Creation
```powershell
# Verify plan exists
Test-Path "C:\Users\Coel\Documents\Godot\blocklife\Docs\Shared\Implementation\*.md"
```

### After Git-Expert Triggers

#### Branch Operations
```bash
# Verify current branch
git branch --show-current

# Verify commits
git log --oneline -5
```

## Verification Decision Tree

```
Did agent report success?
├─ YES → VERIFY
│   ├─ File operation? → Check source AND destination
│   ├─ Status update? → Check Backlog.md
│   ├─ Archive operation? → Check both folders
│   └─ Creation? → Check file exists
└─ NO → Check error
    ├─ Path issue? → Fix paths in workflow
    ├─ Permission? → Check access rights
    └─ Logic error? → Fix agent workflow
```

## Common Failure Patterns

### 1. Path Mismatches (Most Common)
- **Symptom**: Agent reports success, files not moved
- **Check**: Compare workflow paths with actual structure
- **Example**: BF_003 - Missing `/completed/` in archive path

### 2. Silent Failures
- **Symptom**: No error reported, work not done
- **Check**: Manually verify expected outcome
- **Fix**: Add verification steps to workflow

### 3. Partial Completion
- **Symptom**: Some items processed, others skipped
- **Check**: Count expected vs actual changes
- **Fix**: Add batch verification

## Verification Automation Script

Create `scripts/verify_agent_output.py`:

```python
import os
import json
from pathlib import Path
from datetime import datetime

class AgentVerifier:
    def __init__(self):
        self.project_root = Path("C:/Users/Coel/Documents/Godot/blocklife")
        self.backlog_path = self.project_root / "Docs/Backlog"
        
    def verify_archive(self, item_ids):
        """Verify items were properly archived"""
        results = {}
        for item_id in item_ids:
            in_items = list(self.backlog_path.glob(f"items/{item_id}_*.md"))
            in_archive = list(self.backlog_path.glob(f"archive/completed/*/{item_id}_*.md"))
            
            results[item_id] = {
                "archived": len(in_archive) > 0 and len(in_items) == 0,
                "in_items": len(in_items),
                "in_archive": len(in_archive)
            }
        return results
    
    def verify_status(self, item_id, expected_status):
        """Verify item status in Backlog.md"""
        backlog_file = self.backlog_path / "Backlog.md"
        with open(backlog_file, 'r') as f:
            content = f.read()
            # Check for status marker
            return f"{item_id}.*{expected_status}" in content
    
    def report(self, verification_type, results):
        """Generate verification report"""
        print(f"\n📊 Verification Report: {verification_type}")
        print(f"Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print("-" * 40)
        
        for key, value in results.items():
            status = "✅" if value.get("archived", value) else "❌"
            print(f"{status} {key}: {value}")
        
        return all(v.get("archived", v) for v in results.values())

# Usage
if __name__ == "__main__":
    verifier = AgentVerifier()
    
    # Verify recent archive operation
    items = ["TD_012", "TD_013", "TD_016", "TD_017", "TD_018"]
    results = verifier.verify_archive(items)
    verifier.report("Archive Operation", results)
```

## Incident Response Template

When verification fails:

```markdown
## Verification Failure Report

**Date**: [DATE]
**Agent**: [AGENT_NAME]
**Operation**: [WHAT_WAS_ATTEMPTED]

### Expected Outcome
- [What should have happened]

### Actual Outcome
- [What actually happened]

### Verification Method
```powershell
[Commands used to verify]
```

### Root Cause
- [Path issue / Logic error / Missing check]

### Fix Applied
- [What was changed]

### Prevention
- [How to prevent recurrence]
```

## Trust Levels by Agent

Based on incident history:

| Agent | Trust Level | Verification Required | Known Issues |
|-------|------------|----------------------|--------------|
| backlog-maintainer | MEDIUM | Archive operations | BF_003: Path mismatch |
| tech-lead | HIGH | Spot check only | None |
| product-owner | HIGH | Spot check only | None |
| git-expert | MEDIUM | Branch operations | Check current branch |
| test-designer | HIGH | Test file creation | None |
| dev-engineer | HIGH | Implementation | None |
| qa-engineer | HIGH | Test results | None |
| debugger-expert | HIGH | Fix validation | None |
| architect | HIGH | ADR creation | None |
| vsa-refactoring | MEDIUM | File moves | Verify refactoring |
| devops-engineer | MEDIUM | Script execution | Check output |

## Quick Reference Card

```
🔴 ALWAYS VERIFY:
- Archive operations
- File moves/deletes
- Status updates to 'Complete'
- Branch changes

🟡 SPOT CHECK:
- Progress updates
- Documentation updates
- New item creation

🟢 TRUST BUT LOG:
- Read operations
- Search operations
- Report generation
```

---

**Last Updated**: 2025-08-16
**Version**: 1.0
**Created After**: BF_003 Incident