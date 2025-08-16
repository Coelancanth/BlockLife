# BlockLife Development Backlog

**Last Updated**: 2025-08-17

## 🔥 Critical (Do First)
*Blockers preventing other work, production bugs, dependencies for other features*

- **TD_005**: **URGENT** Debugger-expert agent workflow improvement (critical - agent failed twice with false resolutions)
- **VS_004**: View layer debugging system with GDUnit4 integration (specialized debugging tools for UI/animation performance)
- Fix critical bugs in Grid State
- Complete Move Block feature (Phase 1) dependencies

## 📈 Important (Do Next)  
*Core features for current milestone, technical debt affecting velocity*

- **TD_005**: Debugger-expert agent workflow improvement (analyze evidence before implementing solutions)
- **VS_001**: Block drag-and-drop interaction system (replace click-to-move)
- **TD_003**: Block movement range validation and constraints  
- **TD_004**: Block swap functionality for occupied positions
- Implement Block Rotation system
- Begin Save/Load system design
- Animation queuing system implementation
- Performance optimization for grid operations

## 💡 Ideas (Do Later)
*Nice-to-have features, experimental concepts, future considerations*

- Multiplayer support exploration
- Custom block shapes system
- Level editor prototype
- Audio system integration (waiting for Godot 4.4)

## ✅ Done This Week
- ~~Move Block feature (Phase 1) core implementation~~
- ~~VSA refactoring cleanup~~
- ~~Agent ecosystem documentation review~~
- ~~MAJOR: Documentation consolidation - 76 files → Essential Three system~~
- ~~Workflow transformation - intelligent orchestration with complexity-based delegation~~
- ~~CLAUDE.md update - added critical thinking mandate and over-engineering safeguards~~
- **TD_002**: Block interaction reflection removal (eliminated initialization bottleneck)
- **BF_001**: ✅ RESOLVED - First-click lag fixed via async/await pre-warming (271ms JIT compilation moved to startup)

## 🚧 Currently Blocked
- Audio system implementation (external dependency: Godot 4.4)

---

## 📋 Quick Reference

**Priority Decision Framework:**
1. **Blocking other work?** → 🔥 Critical
2. **Current milestone?** → 📈 Important  
3. **Everything else** → 💡 Ideas

**Work Item Types:**
- **VS_xxx**: Vertical Slice (complete feature)
- **BF_xxx**: Bug Fix
- **TD_xxx**: Technical Debt
- **HF_xxx**: Hotfix

---
*Single Source of Truth for all BlockLife development work. Simple, maintainable, actually used.*