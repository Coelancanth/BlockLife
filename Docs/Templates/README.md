# Templates Guide

## Overview
Unified templates for consistent work item tracking and documentation in BlockLife.

## Work Item Templates

**Location**: `Work-Items/`

### Primary Work Items
- **[VS_Template.md](Work-Items/VS_Template.md)** - Vertical Slice (new features)
- **[BF_Template.md](Work-Items/BF_Template.md)** - Bug Fix (defect repair)  
- **[TD_Template.md](Work-Items/TD_Template.md)** - Tech Debt (refactoring/cleanup)
- **[HF_Template.md](Work-Items/HF_Template.md)** - Hotfix (critical issues)

### Usage Pattern
1. **Copy appropriate template** to `Docs/Backlog/items/`
2. **Rename with proper ID**: `VS_XXX_Feature_Name.md` (XXX = unprioritized)
3. **Fill out all sections** completely
4. **When prioritized**: Rename XXX to sequential number (001, 002, etc.)

## Documentation Templates

**Location**: `Documentation/`

- **[Bug_Post_Mortem_Template.md](Documentation/Bug_Post_Mortem_Template.md)** - Post-incident analysis

### Usage
- **Copy template** for post-mortem analysis
- **Focus on architectural lessons** and prevention
- **Link to related work items** (BF_XXX, TD_XXX)

## Template Philosophy

**Simple, actionable, actually used.**

- ✅ **Practical sections** that agents and developers actually fill out
- ✅ **Clear ownership** - Who does what when
- ✅ **Progress tracking** - Current status and blockers
- ✅ **Reference links** - Connection to architecture and patterns
- ❌ **No enterprise complexity** - Just what's needed to get work done

## Agent Integration

**Templates designed for our agent ecosystem:**
- **Product Owner**: Creates and prioritizes work items
- **Tech Lead**: Adds implementation plans and risk assessment
- **Dev Engineer**: Updates progress and technical notes
- **Test Designer**: Defines testing approach and acceptance criteria
- **QA Engineer**: Validates completion and quality

## Quick Reference

**Creating new work:**
```bash
# Copy template
cp Docs/Templates/Work-Items/VS_Template.md Docs/Backlog/items/VS_XXX_My_Feature.md

# Edit content, then when prioritized:
mv VS_XXX_My_Feature.md VS_001_My_Feature.md
```

**Post-incident analysis:**
```bash
# Copy template
cp Docs/Templates/Documentation/Bug_Post_Mortem_Template.md Docs/post-mortems/Bug_Analysis_YYYY_MM_DD.md
```

---
*Simple templates for effective work management and learning.*