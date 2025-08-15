# HF_004: Replace Static Events with Weak Event Pattern

**Status**: Ready
**Severity**: CRITICAL
**Priority**: P1 - High
**Incident Time**: 2025-08-14 (Discovered via stress test)
**Reported By**: Architecture Stress Test AST-001
**Assigned To**: TBD

## 🚨 CRITICAL ISSUE

### Impact
- **Users affected**: All users in long sessions
- **Functionality broken**: Memory consumption grows unbounded
- **Data loss risk**: No - Performance degradation
- **Workaround available**: No - Architectural pattern issue

## Problem Description
Static event pattern in notification bridges causes memory leaks. Presenters subscribe but if Dispose() isn't called (scene transitions, errors), subscriptions persist forever causing:
- Memory leaks growing over time
- Ghost presenters receiving events
- Performance degradation
- Potential crashes from memory exhaustion

## Immediate Symptoms
- [ ] Memory usage grows continuously
- [ ] Performance degrades over time
- [ ] Old presenters still process events
- [ ] Duplicate event handling

## Root Cause (If Known)
- Component: All NotificationBridge classes (e.g., `BlockPlacementNotificationBridge.cs`)
- Cause: Strong references via static events prevent garbage collection
- Introduced: ADR-006 static event bridge pattern

## Emergency Fix

### Quick Fix (Temporary)
Not applicable - requires architectural change

### Proper Fix (Permanent)
Implement weak event pattern:
```csharp
public static class BlockPlacementNotificationBridge
{
    private static readonly WeakEvent<BlockPlacedNotification> _blockPlacedEvent = new();
    
    public static void Subscribe(Action<BlockPlacedNotification> handler)
        => _blockPlacedEvent.Subscribe(handler);
    
    public static void Unsubscribe(Action<BlockPlacedNotification> handler)
        => _blockPlacedEvent.Unsubscribe(handler);
    
    public static void RaiseBlockPlaced(BlockPlacedNotification notification)
        => _blockPlacedEvent.Raise(notification);
}
```

## Rollback Plan
If fix fails:
1. Keep existing pattern temporarily
2. Add aggressive disposal checks
3. Implement presenter lifecycle monitoring
4. Add memory usage alerts

## Testing

### Smoke Test (Immediate)
- [ ] Events still propagate correctly
- [ ] Presenters can subscribe/unsubscribe
- [ ] Basic scene transitions work

### Full Test (Post-fix)
- [ ] Memory profiling shows no leaks
- [ ] 1000+ scene transitions stable
- [ ] Disposed presenters don't receive events
- [ ] Performance remains constant

## Communication

### Stakeholder Updates
- [ ] Team notified of memory leak risk
- [ ] Performance team involved
- [ ] Long-term pattern change communicated
- [ ] Migration plan created

### Status Updates
- 2025-08-14: Memory leak pattern identified
- 2025-08-15: Fixed via PR #13
- [Pending]: Performance validation
- [Pending]: Full deployment

## Verification
- [ ] No memory growth over time
- [ ] Weak references allow GC
- [ ] Events still function correctly
- [ ] Performance acceptable

## Post-Incident

### Lessons Learned
- Why it happened: Static events create strong references
- Why not caught earlier: No memory profiling in tests
- Prevention measures: Weak event pattern as standard

### Follow-up Actions
- [ ] Update all notification bridges
- [ ] Add memory leak detection tests
- [ ] Document weak event pattern
- [ ] Update architecture guidelines
- [ ] Create migration script for existing bridges

## Definition of Resolution
- [ ] All bridges use weak events
- [ ] Memory leaks eliminated
- [ ] Performance validated
- [ ] Pattern documented
- [ ] Tests prevent regression

## References
- Incident report: Architecture_Stress_Test_Critical_Findings.md
- Related issues: CRIT-003
- Fix PR: #13
- All NotificationBridge classes in codebase