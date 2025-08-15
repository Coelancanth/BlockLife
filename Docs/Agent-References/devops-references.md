# DevOps Engineer Agent - Documentation References

## 🗺️ Quick Navigation
**START HERE**: [DOCUMENTATION_CATALOGUE.md](../DOCUMENTATION_CATALOGUE.md) - Complete index of all BlockLife documentation

## Your Domain-Specific Documentation
Location: `Docs/Agent-Specific/DevOps/`

- `build-commands.md` - All build and development commands
- `automation-guide.md` - Python automation scripts and patterns
- `ci-cd-patterns.md` - GitHub Actions and pipeline configurations

## Shared Documentation You Should Know

### 🧠 **Living Wisdom System** (For Automation Context)
- **[Living-Wisdom Index](../Living-Wisdom/index.md)** - Master index to all living documents
- **[LWP_001_Stress_Testing_Playbook.md](../Living-Wisdom/Playbooks/LWP_001_Stress_Testing_Playbook.md)** - Automation opportunities for stress testing
- **[LWP_004_Production_Readiness_Checklist.md](../Living-Wisdom/Playbooks/LWP_004_Production_Readiness_Checklist.md)** - Deployment validation automation needs

### Current Automation
- `scripts/` folder - All existing Python automation scripts
- `scripts/test_monitor.py` - Test automation reference
- `scripts/setup_git_hooks.py` - Git workflow automation

### Build Configuration
- `BlockLife.sln` - Main solution file
- `project.godot` - Godot project configuration
- `tests/BlockLife.Core.Tests.csproj` - Test project structure

### Development Workflow Integration
- `Docs/Shared/Guides/Git_Workflow_Guide.md` - Git workflow requirements
- `Docs/Shared/Guides/Comprehensive_Development_Workflow.md` - Full development process
- `Docs/Shared/Guides/Test_Automation_Guide.md` - Testing automation patterns

### Post-Mortems for Automation Insights
- Focus on process improvement opportunities from bug reports
- Look for repetitive manual tasks that could be automated

## Quick Automation Assessment

When evaluating automation opportunities:
1. **Frequency**: How often is this done? (>1x per day = automate)
2. **Error-prone**: Are there manual mistakes happening?
3. **Cognitive load**: Does this interrupt development flow?
4. **Time savings**: Is the automation effort worth it?
5. **Reliability**: Will automation be more reliable than manual?

## Technology Stack
- **Python**: Primary automation language
- **PowerShell**: Windows environment commands
- **GitHub Actions**: CI/CD pipeline
- **dotnet CLI**: Build and test commands
- **Godot CLI**: Export and project management

## Common Automation Patterns
- File watching with `watchdog`
- Parallel execution with `asyncio`
- Configuration management with `yaml`
- CLI interfaces with `click`
- Rich console output with `rich`

## Integration Points
- **Git Expert**: Git workflow automation
- **QA Engineer**: Test automation and reporting
- **Tech Lead**: Build pipeline integration