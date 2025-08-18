# Thin Slice Protocol - No Phases Allowed

## Core Principle: Every VS Item Must Stand Alone

**RULE**: If you need "phases," your slice is too thick. Period.

## ❌ What We're ELIMINATING

```markdown
### VS_001: Complex Feature with Phases  ❌ WRONG
Phase 1: Basic implementation
Phase 2: Add animations  
Phase 3: Add feedback

Problem: This is 3 features pretending to be 1
```

## ✅ What We're DOING Instead

### Option 1: Sequential VS Items (Recommended)

```markdown
### VS_001: Basic Block Swap (Day 1)
**Status**: Done
**What**: Swap two blocks positions
**Value**: Players can reorganize

### VS_002: Animate Block Swap (Day 2)  
**Status**: In Progress
**Depends On**: VS_001
**What**: Add smooth animations to swap
**Value**: Better user experience

### VS_003: Swap Feedback System (Day 3)
**Status**: Proposed
**Depends On**: VS_002
**What**: Add sounds and particles
**Value**: Satisfying interaction
```

**Benefits**:
- Each delivers value independently
- Can ship after any VS completes
- Clear dependencies
- No archiving confusion

### Option 2: Parent Epic with Child Items (Complex Features)

When you truly have a large feature that needs coordination:

```markdown
## EPIC_001: Complete Swap System
**Status**: Tracking Only (never "Done" - just tracks children)
**Children**: VS_001, VS_002, VS_003
**Progress**: 1/3 Complete

### VS_001: Basic Swap [EPIC_001]
**Status**: Done ✅
**Parent**: EPIC_001

### VS_002: Swap Animation [EPIC_001]
**Status**: In Progress
**Parent**: EPIC_001

### VS_003: Swap Polish [EPIC_001]
**Status**: Proposed
**Parent**: EPIC_001
```

**Rules for Epics**:
- Epic status is ALWAYS "Tracking Only"
- Epic shows "X/Y Complete" based on children
- Epic moves to archive only when ALL children done
- Children can be worked independently

## 🚫 Banned Patterns

1. **No "Phase X" in VS items** - Create separate VS items
2. **No VS items >3 days** - Break them down
3. **No "Part 1 of N"** - Each part is its own VS
4. **No nested phases** - Flat structure only

## ✅ Enforcement Rules

### When Tech Lead Reviews VS Items:

```python
if "phase" in vs_item.description.lower():
    return "REJECTED: Break into separate VS items"

if vs_item.estimated_days > 3:
    return "REJECTED: Slice too thick, break it down"

if not vs_item.delivers_value_alone:
    return "REJECTED: Must deliver value independently"
```

### Backlog Structure for Related Items:

```markdown
## 📈 Important

### VS_004: User Login Screen
**Status**: Done
**Size**: M (6 hours)

### VS_005: Password Reset Flow
**Status**: In Progress
**Depends On**: VS_004  ← Clear dependency
**Size**: M (4 hours)

### VS_006: Remember Me Feature
**Status**: Proposed
**Depends On**: VS_004  ← Can skip VS_005
**Size**: S (2 hours)
```

## 🎯 Decision Tree

```
Need multiple steps for a feature?
├─ Can each step deliver value alone?
│  ├─ YES → Create separate VS items with dependencies
│  └─ NO → Your slice is too thick, redesign it
│
└─ Is this a large initiative (>1 week)?
   ├─ YES → Create an EPIC with child VS items
   └─ NO → Break into 2-3 day VS items
```

## 📋 Migration Guide for Existing Phases

If you have existing multi-phase items:

1. **Extract each phase** to its own VS item
2. **Add dependencies** between them
3. **Archive the original** multi-phase item
4. **Update status** on individual items

Example:
```bash
# Before:
VS_001 with Phase 1 (Done), Phase 2 (Done), Phase 3 (In Progress)

# After:  
VS_001a: Phase 1 content [Status: Done] ✅
VS_001b: Phase 2 content [Status: Done] ✅
VS_001c: Phase 3 content [Status: In Progress]
```

## 🏆 Benefits of This Approach

1. **No archiving confusion** - Items are either done or not
2. **True agility** - Can ship value after each item
3. **Clear dependencies** - Explicit rather than implicit
4. **Simpler backlog** - No phase tracking needed
5. **Enforces good architecture** - Thin slices by design

---

*Remember: If you need phases, you're doing it wrong. Break it down.*