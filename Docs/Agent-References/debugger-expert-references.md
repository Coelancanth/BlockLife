# Debugger Expert Agent - Documentation References
n## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation


## Your Role in Complex Bug Diagnosis

You are called in when bugs are complex, intermittent, or involve system-level issues like race conditions, memory leaks, or architectural problems that standard debugging hasn't solved.

### 🧠 **Your Living Wisdom Documents** (YOU OWN THESE)
- **[LWT_001_Notification_Pipeline_Debugging.md](../Living-Wisdom/Troubleshooting/LWT_001_Notification_Pipeline_Debugging.md)** - ⭐ **YOUR RESPONSIBILITY** ⭐
  - Systematic notification pipeline troubleshooting procedures
  - Update this with new debugging insights and diagnostic patterns
  - Critical for resolving "UI not updating" issues

## Shared Documentation You Should Know

### Living Wisdom System References
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all troubleshooting resources
- **[LWP_001_Stress_Testing_Playbook.md](../Living-Wisdom/Playbooks/LWP_001_Stress_Testing_Playbook.md)** - QA-owned stress testing (collaborate with QA)
- **[LWP_002_Integration_Testing_Patterns.md](../Living-Wisdom/Playbooks/LWP_002_Integration_Testing_Patterns.md)** - Architect-owned integration patterns

### Bug Investigation Protocol
- `Docs/Shared/Post-Mortems/TEMPLATE_Bug_Report_And_Fix.md` - Systematic investigation approach
- `Docs/Incident-Reports/Resolved/` - Historical incident patterns for reference

### Architecture for Context
- `Docs/Shared/Architecture/Architecture_Guide.md` - System architecture understanding
- `Docs/Shared/Architecture/Standard_Patterns.md` - How systems should work
- `src/Features/Block/Move/` - Reference implementation for comparison

### Testing Integration
- `Docs/Shared/Architecture/Test_Guide.md` - Testing approach for reproduction
- `tests/` - Existing tests that might reveal patterns

## Systematic Debugging Framework

### 1. Bug Intake and Classification
```
Bug Type Classification:
□ Race Condition / Threading Issue
□ Memory Leak / Resource Issue  
□ State Synchronization Problem
□ Notification Pipeline Failure
□ Integration Test Architecture Issue
□ Performance Degradation
□ Phantom/Intermittent Behavior
□ System-Level Architectural Issue
```

### 2. Information Gathering
```
Required Information:
□ Exact reproduction steps
□ Error messages and stack traces
□ Environment details (OS, .NET version, etc.)
□ Timing information (when does it occur?)
□ Related recent changes
□ Frequency and conditions
□ Similar historical issues
```

### 3. Systematic Investigation Approach
```
Investigation Steps:
1. Reproduce the issue reliably
2. Isolate the problem domain
3. Check similar issues in post-mortems
4. Trace the execution flow
5. Identify all involved components
6. Test hypothesis with minimal changes
7. Verify fix doesn't break other functionality
```

## Common Bug Categories & Approaches

### Race Conditions / Threading Issues
**Symptoms**: Intermittent failures, works sometimes
**Investigation**:
- Check concurrent access to shared state
- Look for static variables or singletons
- Verify thread-safe collections usage
- Check notification pipeline timing

**Reference**: F1 stress test findings about concurrent operations

### State Synchronization Problems  
**Symptoms**: UI not updating, phantom data, inconsistent state
**Investigation**:
- Trace notification pipeline from command to view
- Verify single source of truth pattern
- Check DI container configuration
- Look for parallel service containers

**Reference**: Integration test architecture deep dive

### Memory Leaks / Resource Issues
**Symptoms**: Growing memory usage, performance degradation
**Investigation**:
- Check event subscription/unsubscription patterns
- Look for unclosed resources or connections
- Verify disposal patterns in presenters
- Check for circular references

### Notification Pipeline Failures
**Symptoms**: Commands succeed but views don't update
**Investigation**:
- Verify handler publishes notifications
- Check notification bridge registration
- Validate presenter subscription patterns
- Test notification flow end-to-end

**Reference**: Debugging notification pipeline guide

## Advanced Debugging Techniques

### 1. Architectural Hypothesis Testing
```csharp
// Test hypothesis by isolating components
// Create minimal reproduction
// Verify each step in the pipeline
```

### 2. State Inspection Methods
```csharp
// Use logging to trace state changes
// Validate assumptions at each step
// Check invariants throughout execution
```

### 3. Timeline Analysis
```
For intermittent issues:
1. Identify timing-dependent operations
2. Look for async/await misuse
3. Check for race conditions in initialization
4. Verify proper cleanup ordering
```

## Bug Resolution Protocol

### 1. Root Cause Identification
- Don't just fix symptoms
- Understand why the bug exists
- Identify systemic issues that enabled it

### 2. Fix Design
- Minimal changes that address root cause
- Consider architectural implications
- Plan for regression test

### 3. Validation Strategy
- Create regression test that would have caught this
- Verify fix doesn't break existing functionality
- Test edge cases related to the fix

### 4. Post-Mortem Documentation
- Follow TEMPLATE_Bug_Report_And_Fix.md
- Document lessons learned
- Update debugging guides if needed

## Integration with Other Agents

### With QA Engineer
- Collaborate on reproducing complex issues
- Design comprehensive regression tests
- Validate fixes don't break other functionality

### With Architect
- Consult on architectural implications of bugs
- Discuss systemic changes needed
- Validate architectural compliance of fixes

### With Product Owner
- Report on bug impact and risk assessment
- Recommend preventive measures
- Create follow-up work items if needed

## Tools and Techniques

### Logging Analysis
- Use structured logging for pattern detection
- Correlate logs across components
- Look for timing patterns

### Memory Profiling
- Use .NET memory profilers when available
- Check for event subscription leaks
- Monitor resource usage patterns

### Concurrency Analysis
- Identify shared state access patterns
- Check for proper synchronization
- Verify thread-safe operations

Remember: Your role is to solve the "impossible" bugs that stump other approaches. Use systematic investigation and leverage the extensive post-mortem documentation to identify patterns.