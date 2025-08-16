# Debugger Expert Workflow

## Core Actions

### 1. Diagnose Complex Bug

**When**: Stuck on bug >30 minutes, intermittent failures, race conditions

**Process**:
1. **Gather Evidence**
   - Exact reproduction steps
   - Stack traces and error messages  
   - When it occurs (timing, conditions)
   - What changed recently

2. **Form Hypothesis**
   - What's the simplest explanation?
   - Check common patterns:
     - Notification pipeline: Command → Handler → Bridge → Presenter → View
     - Race condition: Shared state, missing locks, async timing
     - Memory leak: Event subscriptions, disposal issues
     - State sync: Multiple sources of truth

3. **Test Systematically**
   - Binary search to isolate problem
   - Minimal reproduction case
   - Verify hypothesis with targeted tests

4. **Find Root Cause**
   - Not just "Node is null" but "Presenter disposed during scene change"
   - Not just "Signal not received" but "Signal emitted before node ready"

**Output Format**:
```
🔍 ROOT CAUSE ANALYSIS

SYMPTOMS: [What was observed]
ROOT CAUSE: [The actual problem]
EVIDENCE: [How we know this is the cause]

REPRODUCTION:
1. [Step to reproduce]
2. [Step to reproduce]

FIX:
[Specific code changes needed]

REGRESSION TEST:
[Test that would have caught this]
```

### 2. Create Regression Test

**When**: After bug is fixed

**Process**:
1. **Capture Bug Conditions**
   ```csharp
   [Test]
   public async Task Regression_BugDescription()
   {
       // Arrange - Setup exact conditions that caused bug
       var conditions = CreateBugConditions();
       
       // Act - Perform operation that failed
       var result = await OperationThatFailed();
       
       // Assert - Verify bug doesn't recur
       result.Should().NotHaveOldBugBehavior();
   }
   ```

2. **Verify Test Works**
   - Test fails without fix
   - Test passes with fix

## Common Investigation Patterns

**Notification Issues**: Check each step in pipeline
**Thread Safety**: Look for Godot operations off main thread
**Memory Leaks**: Check event subscriptions and disposal
**State Problems**: Find all sources of truth
**Godot Lifecycle**: Verify node validity and timing

## Quality Standards

- Find root cause, not symptoms
- Provide clear reproduction steps  
- Include concrete fix
- Create regression test
- Document lessons learned