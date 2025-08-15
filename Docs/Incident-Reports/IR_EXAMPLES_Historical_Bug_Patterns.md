# Historical Bug Pattern Examples

**Document ID**: IR_EXAMPLES_Historical_Bug_Patterns  
**Type**: Reference Documentation  
**Status**: Archived Examples  
**Source**: Legacy `EXAMPLE_Bug_Post_Mortem.md`  

## Purpose

This document preserves examples of common bug patterns and their analyses from the original post-mortem format. These examples demonstrate recurring architectural issues and their prevention strategies.

## Historical Examples

### Example 1: Type Ambiguity Issues
**Pattern**: LanguageExt.Fin<T> conflicted with System.Runtime.Intrinsics.Arm.AdvSimd.Arm64  
**Root Cause**: Implicit global using statements created hidden coupling  
**Prevention**: Always use fully qualified names for functional types in public APIs  

### Example 2: DI Registration Circular Dependencies
**Pattern**: Presenters couldn't be resolved due to circular dependency on Views  
**Root Cause**: Presenter depending on concrete View instead of interface  
**Prevention**: Presenters MUST depend only on IView interfaces, use factory pattern  

### Example 3: Inconsistent Error Creation
**Pattern**: Mixed Error.New(code, message) with Error.New(message) causing test failures  
**Root Cause**: Error creation logic scattered across handlers  
**Prevention**: Centralize error creation in domain-specific error factories  

### Example 4: Notification Pipeline Broken
**Pattern**: Commands executed successfully but UI didn't update  
**Root Cause**: Notifications weren't being published from handlers  
**Prevention**: Always publish notifications after successful command execution  

### Example 5: Architecture Boundary Violations
**Pattern**: Core project accidentally imported Godot namespace  
**Root Cause**: Inner layer depending on outer layer  
**Prevention**: Add architecture tests to prevent boundary violations  

## Modern Incident Format

These historical examples have been incorporated into the Living Wisdom framework:

- **LWP_001_Stress_Testing_Playbook.md**: Prevention strategies and testing patterns
- **LWP_004_Production_Readiness_Checklist.md**: Pre-production validation patterns
- **Individual Incident Reports**: Specific bug resolutions with modern format

## References

- **Original Source**: `EXAMPLE_Bug_Post_Mortem.md` (archived)
- **Modern Framework**: Living Wisdom Playbooks in `Docs/Living-Wisdom/`
- **Current Template**: `TEMPLATE_Bug_Report_And_Fix.md` (active)

---

**Note**: This document preserves historical examples for reference while the project has migrated to the Living Wisdom framework for ongoing knowledge management.