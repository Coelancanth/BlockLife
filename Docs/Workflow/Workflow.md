# Backlog Workflow

## Work Item Types & Ownership

| Type | Description | Creator | Owner | Example |
|------|-------------|---------|-------|---------|
| **VS** | Vertical Slice (Feature) | Product Owner | Dev Engineer | VS_001: Drag-and-drop blocks |
| **BR** | Bug Report | Test Specialist | Debugger Expert | BR_007: Blocks disappear on click |
| **TD** | Technical Debt | Anyone (propose) → Tech Lead (approve) | Dev Engineer | TD_003: Refactor grid service |

## The Flow

```
Product Owner → Tech Lead → Dev Engineer → Test Specialist → DevOps
     (VS)       (Validate)     (BUILD)      (BR/TD)       (CI/CD)
                    ↓                           ↓
                (TD approve)            Debugger Expert (OWNS BR)
```

## VS (Vertical Slice) Flow
```
Product Owner creates VS (Status: Proposed)
    ↓
Tech Lead reviews (Status: Under Review)
    ↓
[Validates: thin, independent, shippable]
    ↓                    ↓
Approved               Needs Refinement
    ↓                    ↓
Ready for Dev      (back to Product Owner)
    ↓
Dev Engineer implements (Status: In Progress)
    ↓
[Runs ./scripts/build.ps1 test locally]
    ↓
Creates PR → CI/CD runs automatically
    ↓
Test Specialist validates (Status: Testing)
    ↓
[Checks functionality AND code quality]
    ↓                    ↓
Passes                Quality Issues
    ↓                    ↓
CI passes & merged   Proposes TD item
    ↓                    ↓
(Status: Done)      (Continues testing)
```

## TD (Tech Debt) Flow
```
Anyone proposes TD (Status: Proposed)
(Including Test Specialist during quality validation)
    ↓
Tech Lead reviews
    ↓
Approved → Dev Engineer implements
    ↓                  ↓
Rejected (with reason)   Done
```

## BR (Bug) Flow
```
Test Specialist creates BR (Status: Reported)
    ↓
Debugger Expert investigates (Status: Investigating)
    ↓
Debugger proposes fix (Status: Fix Proposed)
    ↓
User approves → Dev Engineer implements (Status: Fix Applied)
    ↓
Test Specialist verifies (Status: Verified)
```

## Status Updates

### Who Updates What
- **Product Owner**: Priority changes (🔥/📈/💡)
- **Tech Lead**: TD approval/rejection, adds estimates
- **Dev Engineer**: In Progress → Done
- **Test Specialist**: Testing status, creates BR for bugs, proposes TD for quality issues
- **Debugger Expert**: BR investigation status
- **DevOps**: Build/Deploy status
- **Anyone**: Can propose TD items (Test Specialist commonly does during testing)

### Status Progression

**VS Items:**
```
Proposed → Under Review → Ready for Dev → In Progress → Testing → Done
    ↓           ↓
    ↓    Needs Refinement
    ↓           ↓
    └───────────┘ (back to Product Owner)
```

**TD Items:**
```
Proposed → Approved → In Progress → Done
    ↓
Rejected
```

**BR Items:**
```
Reported → Investigating → Fix Proposed → Fix Applied → Verified
                ↓
        (Can loop back if fix fails)
```

## Priority Tiers

- **🔥 Critical**: Blockers, crashes, data loss, dependencies
- **📈 Important**: Current milestone, active work
- **💡 Ideas**: Future considerations

**Note**: Critical bugs are just BR items with 🔥 priority - no special "hotfix" type needed.

## Quick Rules

1. **One owner per item** - No shared responsibility
2. **Update on state change** - Not every minor step
3. **BR for bugs** - Not BF (Bug Fix)
4. **TD for quality issues** - Test Specialist proposes during testing (Tech Lead approves)
5. **User approves fixes** - Debugger can't autonomously fix
6. **Single source**: `Docs/Workflow/Backlog.md`
7. **Quality gates**: Test Specialist blocks if untestable, proposes TD if messy
8. **CI/CD gates**: All tests must pass locally (`./scripts/build.ps1 test`) before commit
9. **PR requirements**: CI must pass on GitHub before merge

## 🔧 Build Error Troubleshooting

### Common Build Errors & Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| **"Type or namespace not found"** | Wrong namespace assumption | Run `Grep "class ClassName"` to find actual location |
| **"Ambiguous reference between X and Y"** | Multiple types with same name | Add type alias: `using LangError = LanguageExt.Common.Error;` |
| **"Cannot convert lambda expression"** | Match branches return different types | Ensure all branches return same type (e.g., `Unit.Default`) |
| **"Error is an ambiguous reference"** | Godot.Error vs LanguageExt.Error | Use fully qualified: `LanguageExt.Common.Error.New()` |

### Prevention Checklist
Before building after refactoring:
- [ ] Verified namespaces with `Grep "class ClassName"`
- [ ] Added type aliases for any ambiguous types
- [ ] Checked all Match branches return consistent types
- [ ] Ran `./scripts/build.ps1 build` locally first
- [ ] Fixed any namespace conflicts with type aliases

### Quick Commands
```bash
# Find where a class is actually defined
Grep "class PlaceBlockCommand" src/

# Find all usages of a type
Grep "PlaceBlockCommand" --type cs

# Build locally before committing
./scripts/build.ps1 build

# Run tests to verify
./scripts/build.ps1 test
```

## Templates

- `Docs/Workflow/Templates/VerticalSlice_Template.md` - Features
- `Docs/Workflow/Templates/BugReport_Template.md` - Bugs  
- `Docs/Workflow/Templates/TechnicalDebt_Template.md` - Tech Debt

---
*This workflow ensures clear ownership and smooth handoffs through the development lifecycle.*