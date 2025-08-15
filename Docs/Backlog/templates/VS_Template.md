# VS_XXX: [Feature Name]

**Status**: Ready  
**Priority**: TBD  
**Size**: [S/M/L] ([1-5] days)  
**Domain**: [Block/Inventory/Grid/UI/etc.]  
**Progress**: 0%

---

## 📋 User Story *(Product Owner)*
As a [player/developer]  
I want [specific functionality]  
So that [value/benefit]

### Business Context
- **Value Proposition**: [Why is this important?]
- **User Impact**: [Who benefits and how?]
- **Strategic Alignment**: [How does this fit product vision?]

---

## 🏗️ Implementation Plan *(Tech Lead)*

### Technical Approach
- **Architecture Pattern**: [VSA slice, follows Move Block pattern]
- **Complexity Assessment**: [Simple/Medium/Complex - reasoning]
- **Integration Points**: [What systems does this touch?]

### Implementation Phases
```
Phase 1: Core Domain Logic (TDD RED → GREEN)
├── Commands & validation rules
├── Handlers & business logic
├── Unit tests (architecture + unit)
└── Domain entities/DTOs

Phase 2: Notification & Integration
├── Notifications & bridges
├── Service interfaces
├── DI registration
└── Integration points

Phase 3: Presentation Layer
├── Presenter implementation
├── View interfaces
├── Godot scene integration
└── Integration tests

Phase 4: Validation & Polish
├── Stress testing
├── Error handling validation
├── Documentation
└── Performance verification
```

### Technical Decisions
- **Data Flow**: [Command → Handler → State → Notification → Presenter → View]
- **Error Handling**: [Fin<T> pattern with specific error codes]
- **State Management**: [Which services handle state?]
- **Testing Strategy**: [What tests at each level?]

### Risk Assessment
- **Technical Risks**: [What could go wrong technically?]
- **Dependencies**: [What must be done first?]
- **Complexity Factors**: [What makes this challenging?]
- **Mitigation**: [How to reduce risk?]

---

## 🎯 Vertical Slice Components *(Reference)*

### Commands
- `[CommandName] { Properties }`
- Validation rules:
  - [Rule 1]
  - [Rule 2]

### Handlers  
- `[HandlerName]` → Returns `Fin<Result>`
- Business logic:
  - [Step 1]
  - [Step 2]
- Error cases:
  - [Error case 1]  
  - [Error case 2]

### Queries (if needed)
- `[QueryName]` → Returns `[DataType]`

### Notifications
- `[NotificationName] { Properties }`
- Published when: [Condition]
- Subscribers: [List of presenters]

### Domain/DTOs
- `[EntityName]`: [Description]
- Properties:
  - [Property 1]
  - [Property 2]

### Presenters
- `[PresenterName]`
- Subscribes to: [Notifications]
- Updates view: [View interface methods]

### Views
- Interface: `I[ViewName]`
- Methods:
  - `Method1(params)`
  - `Method2(params)`
- Godot implementation: `godot_project/features/[domain]/[feature]/`

---

## ✅ Acceptance Criteria *(Product Owner)*
- [ ] Given [context], When [action], Then [outcome]
- [ ] Given [context], When [action], Then [outcome]  
- [ ] Given [context], When [action], Then [outcome]

### Success Metrics
- **Functional**: [How do we know it works?]
- **Performance**: [Any performance requirements?]
- **User Experience**: [What makes this successful from user perspective?]

---

## 🧪 Test Strategy *(QA Engineer + Test Designer)*

### Architecture Tests *(Already Exist)*
- [ ] Commands are immutable records
- [ ] Handlers return Fin<T>
- [ ] No Godot dependencies in Core

### Unit Tests *(Test Designer → Dev Engineer)*
- [ ] Command validation tests
- [ ] Handler success path tests  
- [ ] Handler error path tests
- [ ] Query tests (if applicable)

### Property Tests *(Test Designer)*
- [ ] [Invariant 1]: [Description]
- [ ] [Invariant 2]: [Description]

### Integration Tests *(QA Engineer)*  
- [ ] Full vertical slice flow
- [ ] UI interaction to state change
- [ ] Notification pipeline verification
- [ ] Stress testing with concurrent operations

---

## 🔄 Dependencies & Integration
- **Depends on**: [List other VS/features that must be completed first]
- **Blocks**: [List dependent VS/features waiting for this]
- **Service Dependencies**: [What services does this need?]
- **Integration Points**: [Where does this connect to existing systems?]

---

## 📚 Implementation References
- **Gold Standard**: `src/Features/Block/Move/` (ALWAYS reference this)
- **Similar Pattern**: [Other feature if applicable]
- **Architecture Patterns**: [Reference to specific architectural patterns]
- **Special Considerations**: [Any unique aspects or gotchas]

---

## ✅ Definition of Done *(Quality Gates)*
- [ ] **Architecture**: All architecture fitness tests pass
- [ ] **Unit Tests**: Comprehensive test coverage with TDD approach
- [ ] **Integration**: End-to-end testing with GdUnit4  
- [ ] **Performance**: Stress testing with 100+ concurrent operations
- [ ] **Code Quality**: Follows BlockLife patterns, no compiler warnings
- [ ] **Documentation**: Agent-specific docs updated if new patterns
- [ ] **Review**: PR approved following established template
- [ ] **Deployment**: Successfully integrated without breaking existing features

---

## 📋 Progress Tracking *(Auto-Updated by Backlog Maintainer)*

### Progress Increments
- **Architecture tests written**: +10%
- **Unit tests written (RED)**: +15%  
- **Implementation (GREEN)**: +40%
- **Tests passing**: +15%
- **Integration tests**: +15%
- **Documentation**: +5%

### Milestone Markers
- [ ] **Phase 1 Complete**: Core domain logic implemented
- [ ] **Phase 2 Complete**: Notification integration working
- [ ] **Phase 3 Complete**: UI integration functional  
- [ ] **Phase 4 Complete**: Validation and polish finished

---

## 🔗 References
- **ADR**: [Link to architectural decision record if exists]
- **Bug Report**: [Link if this addresses a specific bug]  
- **Design Context**: [Link to design documentation if exists]
- **Reference Implementation**: [Link to similar completed feature]

---

*This VS item follows the embedded implementation planning approach where Tech Lead adds detailed technical planning directly within the work item rather than separate implementation plan documents.*