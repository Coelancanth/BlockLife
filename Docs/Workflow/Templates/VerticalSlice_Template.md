# VS_XXX: [Feature Name]

**Type**: Vertical Slice  
**Priority**: [🔥 Critical | 📈 Important | 💡 Ideas]  
**Size**: [Thin (1 day) | Medium (2 days) | Max (3 days)]  
**Status**: [Proposed | Under Review | Needs Refinement | Ready for Dev | In Progress | Testing | Done]  
**Domain**: [Block/Inventory/Grid/UI/etc.]

---

## 📋 Vertical Slice Definition *(Product Owner)*

**Player Outcome**: [What the player experiences when this complete slice ships]

**Slice Scope**: This vertical slice includes:
- **UI Layer**: [What changes in the user interface]
- **Command Layer**: [What player actions/inputs are handled]
- **Logic Layer**: [What business rules/game mechanics are implemented]
- **Data Layer**: [What state/persistence changes occur]

**Value Proposition**: [Why is this complete slice important now?]

**Slice Boundaries**: 
- **Included**: [What's part of THIS slice]
- **Excluded**: [What's intentionally left for future slices]

**Success Criteria**: [How do we know the complete slice works end-to-end?]

---

## 🏗️ Technical Implementation *(Tech Lead)*

**Architecture Pattern**: Follow `src/Features/Block/Move/` as gold standard

**Vertical Slice Components**:
- **Commands**: [Specific commands for this slice]
- **Handlers**: [Business logic handlers for this slice] 
- **Services**: [Domain services needed for this slice]
- **Presenters**: [MVP presenters for UI updates]
- **Views**: [Godot scenes/nodes for this slice]
- **Events**: [Domain events this slice publishes/consumes]

**Integration Points**: [How this slice connects to existing features]

**Technical Risks**: [Challenges specific to implementing this complete slice]

---

## ✅ Acceptance Criteria *(Product Owner)*

**End-to-End Behavior**:
- [ ] Given [initial state], When [player action in UI], Then [complete flow through all layers]
- [ ] Given [context], When [command issued], Then [state updated and UI reflects change]
- [ ] [Additional complete slice behaviors]

**Layer Validation**:
- [ ] UI responds correctly to player input
- [ ] Commands are validated and processed
- [ ] Business logic executes correctly
- [ ] State persists appropriately
- [ ] Integration with existing features works

---

## 🧪 Testing Approach *(Test Specialist)*

**Vertical Slice Tests**: 
- [ ] **End-to-End**: Complete flow from UI action to state change
- [ ] **Command Tests**: Validation and command processing
- [ ] **Handler Tests**: Business logic success/error paths
- [ ] **Service Tests**: Domain service behavior
- [ ] **Integration Tests**: This slice working with existing features

**Layer-Specific Tests**:
- [ ] **UI Layer**: User interactions trigger correct commands
- [ ] **Logic Layer**: Business rules enforced correctly
- [ ] **Data Layer**: State changes persist correctly
- [ ] **Cross-Layer**: Events flow correctly between layers

---

## 🔄 Dependencies
- **Depends on**: [Other work items that must be done first]
- **Blocks**: [Work items waiting for this to complete]

---

## 📝 Implementation Progress & Notes

**Current Status**: [Brief update on current state]

**Agent Updates**:
- [Date] - [Agent]: [Brief note about progress/findings/blockers]
- [Date] - [Agent]: [Additional updates]

**Blockers**: [Current issues preventing progress]

**Next Steps**: [What needs to happen next]

---

## 📚 References
- **Gold Standard**: `src/Features/Block/Move/` 
- **Related Work**: [Links to related VS items, bugs, or documentation]

---

*Vertical Slice Architecture: Complete, shippable increments through all layers. Each slice delivers working functionality from UI to data.*