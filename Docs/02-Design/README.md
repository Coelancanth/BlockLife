# Game Design Documentation

This folder contains the creative vision for BlockLife.

## 📚 Document Structure

### Single Source of Truth
- **[Vision.md](Vision.md)** ⭐⭐⭐⭐⭐ - THE complete game design document
  - Core mechanics (Triple Town merging, auto-spawn, chains)
  - Future systems (life stages, personality, narratives)
  - Design philosophy and player experience goals

### Archived Ideas
- **[/07-Archive/Future-Ideas/](../07-Archive/Future-Ideas/)** - Premature detailed specs
  - Moved here to prevent confusion
  - Will be resurrected only when needed
  - Not promises, just possibilities


## 🎮 For AI Personas

**IMPORTANT**: The user is the Game Designer. These documents represent their creative vision.

### How to Use These Docs

1. **Vision.md Only** - This is THE design reference (ignore archived ideas)
2. **Reference Only** - Never modify without Game Designer approval
3. **Ask When Unclear** - If design intent is ambiguous, ask the user
4. **Build What's in Backlog** - Not everything in Vision is approved for building

### Current Implementation Priority

1. **VS_003** - Triple-match merging (same-type only)
2. **VS_004** - Auto-spawn system
3. **VS_005** - Chain reactions
4. Everything else in Vision.md is FUTURE possibility

## 🚀 Design Philosophy

BlockLife is about simulating the human experience through game mechanics:
- Every mechanic should tell a story
- Time should feel meaningful
- Choices should have weight
- No perfect path exists

## 📝 Important Notes

- **Vision.md is the ONLY active design document**
- **Backlog.md defines what's actually being built**
- **Archived ideas are NOT commitments**
- Features in Vision.md are possibilities, not promises
- User (Game Designer) maintains sole creative authority

## 🎯 Current Game Focus

**Core Loop Being Built (VS_003-005):**
```
Move Block → Check for 3+ Match → Merge → Check for Chains → Score → Spawn New Block
```

Everything else (life stages, personality, etc.) comes AFTER we validate this is fun.

---

*Build the fun first. Add depth later. Document only what exists.*