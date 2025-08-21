# Troubleshooting & Known Issues

## 🔧 Common Issues and Solutions

### Issue: Tests pass but Godot won't compile
**Symptom**: Unit tests green, but game won't run
**Root Cause**: Tests only validate C# domain, not Godot integration
**Solution**: Always run `./scripts/core/build.ps1 test` before committing
**Prevention**: Pre-commit hook now enforces build + test

### Issue: Glossary violations in code
**Symptom**: Using "merge" instead of "match", "level" instead of "tier"
**Root Cause**: Natural language habits override project vocabulary
**Solution**: Search and replace with correct terms from Glossary.md
**Prevention**: Regular glossary validation checks

### Issue: Subagent reports completion but nothing changed
**Symptom**: backlog-assistant says "updated" but git shows no changes
**Root Cause**: Various subagent failures or partial completions
**Solution**: Use verification scripts: `./scripts/verify-subagent.ps1`
**Prevention**: Always perform 10-second verification after subagent work

### Issue: Context lost between sessions
**Symptom**: Have to re-explain project state each time
**Root Cause**: No persistent memory between sessions
**Solution**: Check `.claude/memory-bank/activeContext.md` first
**Prevention**: Update Memory Bank files after significant work

### Issue: Persona handoffs confusion
**Symptom**: Work routed to wrong persona, unclear ownership
**Root Cause**: Overlapping responsibilities not well defined
**Solution**: Check routing matrix in QuickReference.md
**Prevention**: Clear "Work I Don't Accept" sections in persona docs

## 🐛 Known Bugs

### BR_001: Race condition in notification pipeline
**Status**: Fixed, regression test added
**Lesson**: Always unsubscribe events in Dispose()

### BR_002: Memory leak from static event handlers
**Status**: Fixed, pattern documented
**Lesson**: Use weak references for static events

## 📝 Lessons Learned

### From TD Implementation
- Simple solutions often better than complex ones
- Move Block pattern works for 90% of features
- Don't create abstraction until needed twice

### From Persona System
- Clear boundaries prevent confusion
- Handoff protocols essential
- Single owner per item reduces conflicts

### From Testing
- TDD for domain logic saves time
- Integration tests catch Godot issues
- Stress tests (F1-F10) reveal race conditions

## 🔍 Debugging Tips

### For Notification Issues
1. Check command publishes notification
2. Verify handler bridges to presenter
3. Confirm presenter subscribes in Initialize()
4. Validate presenter disposes properly
5. Test notification reaches view

### For State Issues
1. Identify all state sources
2. Check for dual sources of truth
3. Verify single registration in DI
4. Test state consistency under load
5. Add state validation checks

### For Performance Issues
1. Profile with dotnet-trace
2. Check for unnecessary allocations
3. Look for synchronous I/O
4. Verify efficient collection usage
5. Consider caching expensive operations

---
*Living document - update when new issues discovered or resolved*