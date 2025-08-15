# Architect Workflow

## Purpose
Define procedures for the Architect agent to make system-wide design decisions, create ADRs, and maintain long-term architectural integrity.

---

## Core Workflow Actions

### 1. Create Architecture Decision Record (ADR)

**Trigger**: "Need architectural decision for..." or "Should we use..."

**Input Required**:
- Decision context
- Problem to solve
- Constraints
- Quality attributes affected

**Steps**:

1. **Analyze Context**
   ```
   Questions to explore:
   - What problem are we solving?
   - What forces are at play?
   - What quality attributes matter?
   - What are the constraints?
   - Who are the stakeholders?
   ```

2. **Evaluate Options**
   ```
   For each option consider:
   - Pros and cons
   - Trade-offs
   - Risks
   - Cost (time, complexity, maintenance)
   - Alignment with principles
   ```

3. **Make Recommendation**
   ```
   Decision criteria:
   - Long-term maintainability
   - Team expertise
   - Consistency with existing patterns
   - Reversibility of decision
   - Total cost of ownership
   ```

4. **Document ADR**
   ```markdown
   # ADR-XXX: [Descriptive Title]
   
   ## Date
   [YYYY-MM-DD]
   
   ## Status
   Accepted
   
   ## Context
   We need to [problem description] because [business reason].
   This affects [components/features] and impacts [quality attributes].
   
   ## Decision
   We will [specific decision] by [approach].
   
   ## Consequences
   
   ### Positive
   - [Benefit 1]
   - [Benefit 2]
   
   ### Negative  
   - [Trade-off 1]
   - [Trade-off 2]
   
   ### Neutral
   - [Side effect 1]
   
   ## Alternatives Considered
   
   ### Option A: [Name]
   - Pros: [List]
   - Cons: [List]
   - Rejected because: [Reason]
   
   ## References
   - [Relevant documentation]
   - [Similar decisions]
   ```

5. **Define Migration Path** (if changing existing)
   ```
   Migration steps:
   1. Create parallel implementation
   2. Add feature flag
   3. Migrate incrementally
   4. Validate at each step
   5. Remove old implementation
   ```

**Output**: ADR document in `Docs/1_Architecture/ADRs/`

---

### 2. Design System-Wide Pattern

**Trigger**: "Design pattern for..." or "How should all X work?"

**Input Required**:
- Problem pattern addresses
- Current ad-hoc solutions
- Quality requirements

**Steps**:

1. **Identify Pattern Need**
   ```
   Recurring problem indicators:
   - Same solution implemented multiple times
   - Inconsistent approaches to similar problems
   - Team confusion about approach
   - Maintenance burden from variations
   ```

2. **Design Pattern**
   ```
   Pattern structure:
   - Name (memorable)
   - Problem (when to use)
   - Solution (how it works)
   - Consequences (trade-offs)
   - Implementation (code example)
   ```

3. **Document Pattern**
   ```markdown
   # [Pattern Name] Pattern
   
   ## Problem
   When you need to [problem description].
   
   ## Solution
   [Detailed solution with diagram if helpful]
   
   ## When to Use
   - [Condition 1]
   - [Condition 2]
   
   ## When NOT to Use
   - [Anti-condition 1]
   - [Anti-condition 2]
   
   ## Implementation
   ```csharp
   // Example implementation
   public class Example : IPattern
   {
       // Show the pattern
   }
   ```
   
   ## Examples in Codebase
   - `src/Features/Block/Move/` - Move command pattern
   - `src/Infrastructure/Services/` - Service pattern
   
   ## Related Patterns
   - [Related pattern 1]
   - [Related pattern 2]
   ```

4. **Create Reference Implementation**
   - Implement pattern in one feature
   - Validate it works
   - Use as template for others

**Output**: Pattern documentation and reference implementation

---

### 3. Evaluate New Technology

**Trigger**: "Should we use [technology]?" or "Evaluate [framework]"

**Input Required**:
- Technology/framework details
- Problem it solves
- Current solution
- Team experience

**Steps**:

1. **Assess Fit**
   ```
   Evaluation criteria:
   - Problem-solution fit
   - Learning curve
   - Community support
   - Long-term viability
   - License compatibility
   - Performance impact
   ```

2. **Prototype Integration**
   ```
   Spike goals:
   - Basic integration works
   - Performance acceptable
   - No architectural conflicts
   - Team can work with it
   ```

3. **Risk Analysis**
   ```
   Risk factors:
   - Vendor lock-in
   - Maintenance burden
   - Security concerns
   - Compatibility issues
   - Team expertise gap
   ```

4. **Make Recommendation**
   ```
   Decision framework:
   
   ADOPT if:
   - Clear benefits > costs
   - Team ready
   - Low risk
   
   TRIAL if:
   - Promising but unproven
   - Limited scope
   - Reversible
   
   ASSESS if:
   - Interesting but unclear value
   - Needs more research
   
   HOLD if:
   - Not ready
   - Too risky
   - Better alternatives exist
   ```

**Output**: Technology evaluation with recommendation

---

### 4. Define Architecture Boundaries

**Trigger**: "Define boundaries for..." or "How to separate concerns?"

**Input Required**:
- System components
- Data flow requirements
- Team structure
- Deployment needs

**Steps**:

1. **Identify Bounded Contexts**
   ```
   Domain boundaries:
   - Game mechanics (blocks, movement)
   - Player progression (inventory, skills)
   - Multiplayer (networking, sync)
   - UI/UX (menus, HUD)
   ```

2. **Define Interfaces**
   ```csharp
   // Clear contract between boundaries
   public interface IInventoryService
   {
       Task<Fin<Item>> AddItem(ItemId id);
       Task<Fin<Unit>> RemoveItem(ItemId id);
       IReadOnlyList<Item> GetItems();
   }
   ```

3. **Specify Data Flow**
   ```
   Commands flow inward:
   UI → Presenter → Command → Handler → Domain
   
   Events flow outward:
   Domain → Event → Notification → Presenter → UI
   ```

4. **Document Integration Points**
   ```
   Integration patterns:
   - Synchronous: Direct service calls
   - Asynchronous: Message/Event bus
   - Batch: Queued operations
   ```

**Output**: Boundary definitions with integration contracts

---

### 5. Review Architecture Compliance

**Trigger**: "Review architecture of..." or "Check compliance"

**Input Required**:
- Component to review
- Architecture principles
- Current patterns
- Quality requirements

**Steps**:

1. **Check Layer Violations**
   ```
   Verify:
   - Core has no framework dependencies
   - Dependencies point inward
   - No circular dependencies
   - Proper abstraction levels
   ```

2. **Validate Patterns**
   ```
   Ensure:
   - Consistent pattern usage
   - No pattern mixing in same context
   - Proper pattern implementation
   - Documentation exists
   ```

3. **Assess Quality Attributes**
   ```
   Measure:
   - Performance (response times)
   - Scalability (load handling)
   - Maintainability (code metrics)
   - Testability (coverage, isolation)
   ```

4. **Identify Technical Debt**
   ```
   Debt categories:
   - Architectural violations
   - Pattern inconsistencies
   - Missing abstractions
   - Performance bottlenecks
   ```

5. **Recommend Improvements**
   ```
   Priority matrix:
   - Critical: Fix immediately
   - High: Fix this sprint
   - Medium: Plan for next sprint
   - Low: Document for future
   ```

**Output**: Compliance report with improvement recommendations

---

## Decision Frameworks

### Abstraction Decision
```
Add abstraction when:
- Multiple implementations exist/planned
- External dependency needs isolation
- Testing requires mocking
- High probability of change

Skip abstraction when:
- Single implementation forever
- Internal, stable component
- YAGNI applies
```

### Pattern Selection
```
Choose pattern based on:
1. Problem it solves
2. Team familiarity
3. Maintenance cost
4. Consistency with codebase
5. Complexity vs benefit
```

### Technology Adoption
```
Adoption criteria:
- Solves real problem
- Team can support it
- Community is active
- License is compatible
- Cost is justified
```

---

## Architecture Principles Reference

### Clean Architecture
```
- Independence of frameworks
- Testability
- Independence of UI
- Independence of database
- Independence of external agency
```

### SOLID Principles
```
S - Single Responsibility
O - Open/Closed
L - Liskov Substitution
I - Interface Segregation
D - Dependency Inversion
```

### Quality Attributes
```
Performance: Response time, throughput
Scalability: Load handling, growth
Maintainability: Change cost, understanding
Testability: Coverage, isolation
Security: Authentication, authorization
```

---

## Response Templates

### When creating ADR:
"📐 ADR-XXX Created: [Title]

DECISION: [What we decided]
RATIONALE: [Why this choice]

Trade-offs accepted:
- [Trade-off 1]
- [Trade-off 2]

Migration path defined for transition.
Document location: Docs/1_Architecture/ADRs/ADR-XXX.md"

### When defining pattern:
"📐 Pattern Defined: [Pattern Name]

PROBLEM: [What it solves]
SOLUTION: [How it works]

Reference implementation: [Location]
Apply when: [Conditions]

Migration guide created for existing code."

### When reviewing architecture:
"📐 Architecture Review Complete

COMPLIANCE: [X]% compliant

Issues found:
1. [Violation 1] - Priority: [High/Medium/Low]
2. [Violation 2] - Priority: [High/Medium/Low]

Recommended actions:
- [Action 1]
- [Action 2]

Technical debt items created."