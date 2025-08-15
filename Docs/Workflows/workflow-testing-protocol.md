# Workflow Testing Protocol - Making the Black Box Transparent

## The Problem
We have no visibility into:
- What the main agent actually sends to subagents
- Whether subagents read the workflow files
- If the workflow steps are being followed
- Where failures occur in the chain

## Testing Framework

### 1. Test with Explicit Logging

Instead of normal invocation, use verbose test mode:

```python
# What we WANT to happen (but can't see):
Task("product-owner", "feature_request", context)

# What we'll TEST with (full visibility):
print("=== WORKFLOW TEST START ===")
print(f"1. Invoking: product-owner")
print(f"2. Action: feature_request")
print(f"3. Context being passed: {context}")

result = Task(
    subagent_type="agile-product-owner",  # Using existing agent
    prompt=f"""
    === WORKFLOW TEST MODE ===
    
    1. First, confirm you're reading this workflow:
       Docs/Workflows/product-owner-workflow.md
       
    2. Report which steps you're following:
       - [ ] Step 1: Value Analysis
       - [ ] Step 2: Cost Estimation
       - [ ] Step 3: Priority Check
       - [ ] Step 4: Decision Point
       - [ ] Step 5: User Story Creation
    
    3. Execute action: feature_request
    
    4. Log your decision process:
       - What value score?
       - What cost estimate?
       - What priority decision?
    
    5. Return both:
       - Your normal output
       - A "debug log" of what you did
    """
)

print(f"4. Result received: {result}")
print("=== WORKFLOW TEST END ===")
```

### 2. Verification Checklist

For each test, verify:

```markdown
## Test: Feature Request to Product Owner

### Pre-Test
- [ ] Workflow file exists at correct path
- [ ] Agent definition exists
- [ ] Context data prepared

### During Test
- [ ] Agent acknowledges reading workflow
- [ ] Agent reports following steps
- [ ] Agent makes expected decisions
- [ ] Agent returns expected outputs

### Post-Test  
- [ ] Backlog.md was updated (if expected)
- [ ] Files created in correct location
- [ ] Next agent triggered (if applicable)
```

### 3. Test Scenarios

#### Scenario A: Simple Feature Request
```python
# TEST INPUT
context = {
    "idea": "Add pets",
    "current_priorities": ["HF_002", "HF_003"]
}

# EXPECTED BEHAVIOR
1. PO reads workflow file
2. PO evaluates value (should be low due to critical HFs)
3. PO defers feature
4. Returns reasoning

# VERIFICATION
- Check: Did PO mention the critical issues?
- Check: Was decision to defer?
- Check: Was reasoning clear?
```

#### Scenario B: Progress Update
```python
# TEST INPUT
context = {
    "item_id": "VS_000",
    "event": "tests_passing"
}

# EXPECTED BEHAVIOR
1. Maintainer reads workflow
2. Finds "+15%" for tests_passing
3. Updates Backlog.md silently
4. Returns minimal confirmation

# VERIFICATION
- Check: Was operation silent?
- Check: Did progress increase by 15%?
- Check: Was update atomic?
```

### 4. Debug Mode Implementation

Add debug flag to see internal process:

```python
def trigger_agent_debug(agent_type, action, context, debug=True):
    if debug:
        print(f"\n{'='*50}")
        print(f"DEBUG: Triggering {agent_type}")
        print(f"Action: {action}")
        print(f"Context: {json.dumps(context, indent=2)}")
        print(f"{'='*50}\n")
    
    prompt = f"""
    {"=== DEBUG MODE ===" if debug else ""}
    
    Read your workflow at: Docs/Workflows/{agent_type}-workflow.md
    
    {f"Report each step you take:" if debug else ""}
    
    Action: {action}
    Context: {context}
    
    {f"Include a debug section showing your process" if debug else ""}
    """
    
    result = Task(agent_type, prompt)
    
    if debug:
        print(f"\n{'='*50}")
        print(f"DEBUG: Response from {agent_type}")
        print(result)
        print(f"{'='*50}\n")
    
    return result
```

### 5. Feedback Loop Test

Test the complete chain:

```
1. Feature Request
   → Invoke PO with debug
   → Verify: Story created?
   
2. Trigger Maintainer
   → Invoke Maintainer with debug  
   → Verify: Backlog updated?
   
3. Check Results
   → Read Backlog.md
   → Confirm: All changes correct?
```

### 6. Black Box Visibility Report

After each test, create a report:

```markdown
## Workflow Test Report

### Test: Feature Request for Pet System
Date: 2024-XX-XX

### What We Sent
```
Agent: product-owner
Action: feature_request
Context: {
  "idea": "Add pet system",
  "priorities": ["HF_002", "HF_003"]
}
```

### What Agent Did
1. ✅ Read workflow file
2. ✅ Evaluated value (scored 7/10)
3. ✅ Checked priorities
4. ✅ Made decision (deferred)
5. ✅ Returned reasoning

### What We Got Back
```
Decision: DEFERRED
Reasoning: "Critical thread safety issues must be resolved first"
Next Action: "Focus on HF_002"
```

### Verification
- [x] Workflow followed correctly
- [x] Decision makes sense
- [x] Output format correct
- [ ] Next agent triggered (not implemented)

### Issues Found
- Agent didn't explicitly confirm reading workflow
- No automatic trigger to maintainer
- Silent operations are TOO silent (no feedback)
```

## Making It Visible

### Option 1: Test Harness Script
Create `test_workflows.py`:

```python
#!/usr/bin/env python3
"""Test the agent workflow system with full visibility"""

def test_feature_request():
    """Test PO evaluating a feature"""
    print("TEST: Feature Request to Product Owner")
    
    # Show what we're sending
    print("INPUT:", {"idea": "pets", "priorities": ["HF_002"]})
    
    # Invoke with debug
    result = invoke_po_debug("feature_request", {...})
    
    # Show what we got
    print("OUTPUT:", result)
    
    # Verify
    assert "deferred" in result.lower()
    print("✅ Test passed")

def test_progress_update():
    """Test Maintainer updating progress"""
    print("TEST: Progress Update")
    # ... similar pattern
```

### Option 2: Interactive Testing

I can run tests for you right now with full visibility:

```python
# I'll show you:
1. Exactly what prompt I'm sending
2. What the agent responds
3. Whether it followed the workflow
4. What actually changed in files
```

Would you like me to:
1. Run a visible test right now?
2. Create the test harness script?
3. Build a feedback dashboard?
```