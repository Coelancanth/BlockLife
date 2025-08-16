# TD_015: PO Trigger Points Documentation

## 📋 Overview
**Type**: Tech Debt (Documentation)
**Status**: 20% Complete (Structure Defined)
**Priority**: P1 - High
**Size**: S (2 hours)
**Started**: 2025-08-15

## 📝 Description
Document all trigger points where the Product Owner or Backlog Maintainer agents should be automatically invoked during development workflow.

## 🎯 Why This Matters
- **Problem**: Unclear when agents should be triggered
- **Impact**: Manual invocation, missed updates, inconsistent tracking
- **Solution**: Comprehensive trigger point documentation

## ✅ Completed (20%)
- [x] Basic trigger categories identified
- [x] Silent vs visible update distinction made
- [x] Initial mapping created

## 🔄 Remaining Work (80%)
- [ ] **Complete trigger mapping**
  - [ ] List ALL development actions
  - [ ] Map each to appropriate agent
  - [ ] Define visibility level
  - [ ] Specify context needed
  
- [ ] **Create trigger documentation**
  - [ ] Write comprehensive guide
  - [ ] Add to CLAUDE.md
  - [ ] Create quick reference
  
- [ ] **Implement trigger code**
  - [ ] Add to CLAUDE.md patterns
  - [ ] Test each trigger point
  - [ ] Verify agent invocation

## 📊 Trigger Point Mapping

### Product Owner Triggers (Visible)
| Action | Trigger | Context Needed | Output |
|--------|---------|----------------|--------|
| User describes feature | "I want to add X" | Feature description, current priorities | User story or deferral |
| Bug discovered | Error/issue found | Bug details, severity | BF item creation |
| Acceptance review | Work completed | Work item, test results | Accept/reject decision |
| Priority conflict | Multiple urgent items | Conflicting items | Priority decision |
| Session planning | Session start/end | Current backlog state | Session goals |

### Backlog Maintainer Triggers (Silent)
| Action | Trigger | Context Needed | Output |
|--------|---------|----------------|--------|
| Code written | File saved/created | Files changed, feature | Progress update |
| Tests written | Test file created | Test coverage, type | Progress update |
| Tests passing | Test run success | Test results | Status update |
| PR created | Git PR command | PR details, changes | Item status change |
| PR merged | Merge complete | Merged items | Archive completed |
| Session end | Time-based | Session work | Progress summary |

### Special Triggers
| Action | Trigger | Agent | Notes |
|--------|---------|-------|-------|
| Critical error | Exception/crash | product-owner | Create HF immediately |
| Performance issue | Slow operation | architecture-stress-tester | Then PO for prioritization |
| Architecture violation | Test failure | code-reviewer | Then PO if needs TD item |

## 🚧 Implementation Status
- Basic structure defined
- Need to code actual triggers
- Integration with CLAUDE.md pending

## 📚 References
- [CLAUDE.md](../../../CLAUDE.md)
- [Product Owner Workflow](../../Workflows/product-owner-workflow.md)
- [Backlog Maintainer Workflow](../../Workflows/backlog-maintainer-workflow.md)

## ✅ Acceptance Criteria
- [ ] All trigger points documented
- [ ] Clear mapping table created
- [ ] Implementation code provided
- [ ] Testing protocol defined
- [ ] Quick reference available

## 📝 Notes
This documentation is critical for making the Automatic Orchestration Pattern actually work. Without clear triggers, the pattern remains theoretical rather than practical.