# TD_005: Debugger-Expert Agent Workflow Improvement

**Type**: Technical Debt  
**Priority**: 📈 Important  
**Category**: Agent Workflow/Process  
**Status**: Not Started  
**Impact**: High

---

## 🔧 The Problem
**What's Wrong**: Current debugger-expert agent implements speculative solutions without analyzing actual evidence first. In BF_001, agent created performance "fixes" based on assumptions rather than investigating real data provided by user.

**Where**: `.claude/agents/debugger-expert.md` agent workflow and prompting

**Why It Matters**: 
- **False positive fixes** - Implementing solutions that don't address real problems
- **Wasted development effort** - Time spent on incorrect solutions
- **User frustration** - Issues marked "resolved" when they're not actually fixed
- **Diagnostic capability erosion** - Agents should debug, not guess

**Cost of Delay**: Continued poor debugging practices leading to ineffective issue resolution and loss of user confidence in agent capabilities.

---

## 💡 The Solution
**Approach**: Enhance debugger-expert agent workflow to require evidence analysis before solution implementation

**Key Changes**:
- `.claude/agents/debugger-expert.md` - Update workflow to mandate evidence collection and analysis
- Add requirement for data gathering before proposing solutions  
- Include validation steps to confirm diagnosis before implementation
- Create debugging methodology checklist for systematic investigation

**Benefits**: 
- **Evidence-based debugging** - Solutions based on real data, not assumptions
- **Higher success rate** - Fixes address actual root causes
- **Systematic approach** - Consistent debugging methodology across all issues
- **User confidence** - Reliable problem resolution process

---

## 🧪 Testing & Migration
**Migration Strategy**: Incremental - Update agent instructions and validate with current BF_001 case

**Testing Requirements**:
- [ ] Validate agent requests evidence before proposing solutions
- [ ] Test with BF_001 case - agent should analyze console output first
- [ ] Verify agent follows systematic debugging methodology
- [ ] Confirm no speculative implementations without data

**Rollback Plan**: Revert to previous agent instructions if new workflow proves too restrictive

---

## ✅ Acceptance Criteria
- [ ] Debugger-expert agent always requests available evidence before starting
- [ ] Agent analyzes provided data (console output, logs, metrics) systematically  
- [ ] Agent identifies specific root causes with evidence citations
- [ ] Agent proposes targeted solutions based on findings, not assumptions
- [ ] Agent validates diagnosis before implementing any code changes
- [ ] Agent documents investigation methodology and findings

---

## 🔄 Dependencies & Impact
**Depends On**: Current BF_001 case provides perfect validation scenario

**Blocks**: 
- Effective resolution of BF_001 and future debugging tasks
- User confidence in agent debugging capabilities

**Side Effects**: 
- May slow initial response time (good trade-off for accuracy)
- Requires users to provide available evidence/data
- Sets higher standard for debugging quality

---

## 📝 Progress & Notes

**Current Status**: Issue identified through BF_001 false resolution - agent implemented animation timing changes without analyzing user's actual console output

**Agent Updates**:
- 2025-08-17 - User: Identified that debugger-expert made assumptions instead of analyzing evidence
- 2025-08-17 - Main Agent: Need to fix agent workflow to require evidence analysis first

**Blockers**: None - clear improvement needed

**Next Steps**: 
1. Review current debugger-expert agent instructions
2. Design evidence-first debugging workflow
3. Update agent prompts and methodology
4. Validate with BF_001 re-investigation using actual console output

---

## 📚 References
- **Problem Case**: BF_001 - debugger-expert marked as resolved without investigating user's console output
- **Best Practice**: Evidence-based debugging methodology
- **Agent Documentation**: `.claude/agents/debugger-expert.md`

---

## 🎯 Specific Workflow Improvements Needed

**Current Problematic Pattern**:
1. Receive problem description
2. Make assumptions about likely causes  
3. Implement speculative solutions
4. Mark as "resolved"

**Required Evidence-First Pattern**:
1. Receive problem description
2. **REQUEST available evidence** (logs, console output, metrics, reproduction steps)
3. **ANALYZE provided data** systematically  
4. **IDENTIFY specific root cause** with evidence citations
5. **PROPOSE targeted solution** based on findings
6. **VALIDATE diagnosis** before implementation
7. Implement fix with verification approach

**Evidence Analysis Requirements**:
- Parse and interpret console logs/output
- Identify timing patterns and performance bottlenecks
- Look for error patterns or anomalies  
- Cross-reference user reports with data evidence
- Document findings with specific data citations

---

*Focus on improving debugging quality and development velocity through systematic evidence-based investigation.*