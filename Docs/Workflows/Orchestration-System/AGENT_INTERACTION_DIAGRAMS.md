# Agent Interaction Diagrams

## Purpose
Visual representations of how agents interact in the BlockLife ecosystem for common workflows.

---

## 🎯 Agent Ecosystem Overview

```
                            ┌─────────────────┐
                            │  Claude Code    │
                            │  (Orchestrator) │
                            └────────┬────────┘
                                     │
                 ┌───────────────────┼───────────────────┐
                 │                   │                   │
         ┌───────▼──────┐   ┌───────▼──────┐   ┌───────▼──────┐
         │   Business   │   │  Technical   │   │ Operational  │
         │    Layer     │   │    Layer     │   │    Layer     │
         └───────┬──────┘   └───────┬──────┘   └───────┬──────┘
                 │                   │                   │
      ┌──────────┼──────────┐       │         ┌─────────┼─────────┐
      │          │          │       │         │         │         │
┌─────▼───┐ ┌───▼────┐ ┌───▼───┐   │   ┌─────▼───┐ ┌───▼───┐ ┌───▼────┐
│Product  │ │Backlog │ │  QA   │   │   │  Git    │ │DevOps │ │Debugger│
│ Owner   │ │Maintain│ │Engineer│  │   │ Expert  │ │Engineer│ │Expert  │
└─────────┘ └────────┘ └────────┘   │   └─────────┘ └────────┘ └────────┘
                                     │
                        ┌────────────┼────────────┐
                        │            │            │
                  ┌─────▼───┐ ┌─────▼────┐ ┌────▼─────┐
                  │Tech Lead│ │Architect │ │   VSA    │
                  │         │ │          │ │Refactoring│
                  └────┬────┘ └──────────┘ └──────────┘
                       │
              ┌────────┼────────┐
              │                 │
        ┌─────▼────┐     ┌─────▼────┐
        │  Test    │     │   Dev    │
        │ Designer │     │ Engineer │
        └──────────┘     └──────────┘
```

---

## 📋 Standard Development Flow

```
User Request
    │
    ▼
┌─────────────────────────────────────────────────────┐
│                  Claude Code                        │
│  1. Analyzes request                                │
│  2. Determines workflow needed                      │
│  3. Triggers appropriate agents                     │
└─────────────────────────────────────────────────────┘
    │
    ├──► Product Owner (if feature/bug)
    │        │
    │        ▼
    │    Create VS/BF item
    │        │
    │        ▼
    ├──► Tech Lead (if VS ready)
    │        │
    │        ▼
    │    Add technical plan
    │        │
    │        ▼
    ├──► Test Designer (TDD Red)
    │        │
    │        ▼
    │    Create failing tests
    │        │
    │        ▼
    ├──► Dev Engineer (TDD Green)
    │        │
    │        ▼
    │    Implement to pass tests
    │        │
    │        ▼
    ├──► QA Engineer (Validation)
    │        │
    │        ▼
    │    Integration & stress tests
    │        │
    │        ▼
    └──► Backlog Maintainer (Throughout)
             │
             ▼
         Update progress
```

---

## 🔄 TDD Workflow Interactions

```
┌─────────────────────────────────────────────────────────────────┐
│                         TDD CYCLE                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Requirements ──► Test Designer ──► RED Tests                  │
│                         │                                      │
│                         ▼                                      │
│                   Failing Tests                                │
│                         │                                      │
│                         ▼                                      │
│              Dev Engineer ──► GREEN Implementation             │
│                         │                                      │
│                         ▼                                      │
│                   Passing Tests                                │
│                         │                                      │
│                         ▼                                      │
│               QA Engineer ──► REFACTOR & Validate              │
│                         │                                      │
│                         ▼                                      │
│                  Quality Assured                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

Backlog Maintainer tracks: 0% ──► 25% ──► 75% ──► 100%
```

---

## 🐛 Bug Resolution Flow

```
Bug Reported
    │
    ▼
Claude Code
    │
    ├─────────────────┐
    ▼                 ▼
Product Owner    Debugger Expert
    │                 │
    │                 ▼
    │            Root Cause
    │            Analysis
    │                 │
    ▼                 ▼
Create BF        Test Designer
Item                  │
    │                 ▼
    │            Regression
    │              Test
    │                 │
    └────────►  Dev Engineer
                      │
                      ▼
                 Fix Implementation
                      │
                      ▼
                 QA Engineer
                      │
                      ▼
                 Validation
                      │
                      ▼
              Backlog Maintainer
                      │
                      ▼
                Archive BF Item
```

---

## 🏗️ Architecture Decision Flow

```
Architectural Question/Need
            │
            ▼
      Claude Code
            │
    ┌───────┼───────┐
    ▼               ▼
Architect      Tech Lead
    │               │
    │               ▼
    │         Assess Impact
    │               │
    ▼               │
Design          Provide
Pattern         Context
    │               │
    └───────┬───────┘
            ▼
    Create/Update ADR
            │
            ▼
    VSA Refactoring
    (if extraction needed)
            │
            ▼
    Implementation
    (Test Designer → Dev Engineer)
```

---

## 🔀 Parallel Development Flow

```
Multiple Features Ready
            │
            ▼
      Claude Code
            │
    ┌───────┼───────┐
    │       │       │
    ▼       ▼       ▼
Feature A  Feature B  Feature C
    │       │       │
[Parallel TDD Cycles]
    │       │       │
    ▼       ▼       ▼
Branch A   Branch B   Branch C
    │       │       │
    └───────┼───────┘
            ▼
      Git Expert
    (Merge Coordination)
            │
            ▼
      QA Engineer
    (Integration Testing)
            │
            ▼
    DevOps Engineer
    (CI/CD Pipeline)
```

---

## 📊 Release Preparation Flow

```
Release Milestone Approaching
            │
            ▼
      Product Owner
            │
            ▼
    Review Acceptance
            │
    ┌───────┼───────┐
    ▼               ▼
QA Engineer    Git Expert
    │               │
    │               ▼
    │         Create Release
    │           Branch
    │               │
    ▼               │
Final Testing       │
    │               │
    └───────┬───────┘
            ▼
    DevOps Engineer
            │
    ├──► Build Pipeline
    ├──► Run Tests
    ├──► Package
    └──► Deploy
            │
            ▼
    Product Owner
    (Release Notes)
```

---

## 🔧 Continuous Maintenance Flow

```
                 ┌─────────────────┐
                 │  Claude Code    │
                 │  (Monitoring)   │
                 └────────┬────────┘
                          │
         ┌────────────────┼────────────────┐
         │                │                │
         ▼                ▼                ▼
    Code Changes    Test Results    Documentation
         │                │                │
         ▼                ▼                ▼
 Backlog Maintainer  QA Engineer    Product Owner
         │                │                │
         │                ▼                │
         │          Quality Gates          │
         │                │                │
         └────────────────┼────────────────┘
                          │
                          ▼
                   Update Backlog.md
                   (Single Source of Truth)
```

---

## 🎯 Agent Communication Patterns

### Hub-and-Spoke Pattern
```
        Agent A
           ↑
           │
    ┌──────▼──────┐
    │ Claude Code │
    └──────┬──────┘
           │
           ▼
        Agent B

Never: Agent A ←→ Agent B
```

### Sequential Hand-off Pattern
```
Claude → Agent 1 → Result 1
           ↓
         Claude → Agent 2 → Result 2
                     ↓
                  Claude → Agent 3 → Result 3
                              ↓
                           Claude → User
```

### Parallel Consultation Pattern
```
           Claude Code
          ╱     │     ╲
         ▼      ▼      ▼
     Agent A  Agent B  Agent C
         │      │      │
         ▼      ▼      ▼
      Result  Result  Result
         ╲      │      ╱
          ▼     ▼     ▼
         Synthesized Output
```

---

## 📈 Progress Tracking Flow

```
Any Development Action
         │
         ▼
   Claude Code
         │
         ├──► Execute Action
         │
         └──► Trigger Backlog Maintainer
                     │
                     ▼
              Analyze Changes
                     │
         ┌───────────┼───────────┐
         │           │           │
         ▼           ▼           ▼
    Code Changes  Test Results  Docs
         │           │           │
         ▼           ▼           ▼
      +40%         +15%        +5%
         │           │           │
         └───────────┼───────────┘
                     │
                     ▼
              Update Backlog.md
                     │
                     ▼
              Progress: XX%
```

---

## 🚨 Error Recovery Flow

```
Error/Failure Detected
         │
         ▼
   Claude Code
         │
    ┌────┼────┐
    │    │    │
    ▼    ▼    ▼
Type?  Severity?  Impact?
    │    │    │
    └────┼────┘
         │
         ▼
   Route to Agent
         │
    ┌────┼──────────┐
    ▼    ▼          ▼
Debugger  Git    Architect
Expert   Expert  (if structural)
    │     │          │
    ▼     ▼          ▼
Diagnose  Recover  Redesign
    │     │          │
    └─────┼──────────┘
          │
          ▼
    Implement Fix
    (TDD Cycle)
```

---

## 📝 Key Interaction Principles

1. **Claude Code Always Orchestrates**
   - No direct agent-to-agent communication
   - All coordination through central hub
   - Maintains context and continuity

2. **Backlog Maintainer Runs Continuously**
   - Triggered after every development action
   - Silent operation (announced in early stage)
   - Maintains Single Source of Truth

3. **Agents Have Clear Boundaries**
   - Each agent owns specific domain
   - Hand-offs are explicit
   - No overlapping responsibilities

4. **Progress Is Always Visible**
   - Every action updates progress
   - Backlog.md reflects real-time state
   - Transparency throughout workflow

5. **Failures Trigger Recovery**
   - Errors route to appropriate expert
   - Recovery paths are defined
   - Learning captured in documentation

---

**Last Updated**: 2025-08-16
**Maintained By**: Tech Lead Agent
**Next Review**: After workflow stabilization