# Bug Post-Mortem: Namespace Mismatch Breaking DI Resolution

## Bug ID: PM_003-2025-08-23-Namespace-DI-Resolution

### Summary
VS_003A Phase 4 CQRS handlers used incorrect namespace (`BlockLife.Features` instead of `BlockLife.Core.Features`), causing MediatR auto-discovery to miss them and breaking 13 DI resolution tests.

### Timeline
- **Introduced**: 2025-08-22 23:30 - VS_003A Phase 4 CQRS implementation
- **Discovered**: 2025-08-22 23:41 - DI validation tests failed in CI pipeline
- **Documented**: 2025-08-23 01:30 - Root cause identified by Test Specialist
- **Fixed**: [Pending - TD_068 in backlog]

### Root Cause Analysis

#### Immediate Cause
Six new handler/command/query classes were created with namespace `BlockLife.Features.Player.*` instead of `BlockLife.Core.Features.Player.*`, placing them outside the assembly that MediatR scans for auto-registration.

**Affected Files**:
- `src/Features/Player/Commands/ApplyMatchRewardsCommand.cs`
- `src/Features/Player/Commands/ApplyMatchRewardsCommandHandler.cs`
- `src/Features/Player/Commands/CreatePlayerCommand.cs`
- `src/Features/Player/Commands/CreatePlayerCommandHandler.cs`
- `src/Features/Player/Queries/GetCurrentPlayerQuery.cs`
- `src/Features/Player/Queries/GetCurrentPlayerQueryHandler.cs`

Additionally, `IPlayerStateService` was not registered in the DI container despite being a dependency of all new handlers.

#### Architectural Cause
Which architectural principle was violated:
- [x] **Explicit Dependencies** - MediatR silently failed to discover handlers with no warning
- [x] **Dependency Inversion** - Missing interface registration for IPlayerStateService
- [ ] Single Responsibility (class doing too much)
- [ ] Interface Segregation (fat interface)
- [ ] Command/Query Separation (state change outside handler)

#### Process Cause
How did this slip through:
- [x] **Missing test coverage** - No immediate DI validation after creating handlers
- [x] **IDE default behavior** - Auto-generated namespace from folder structure
- [x] **Documentation gap** - Reference pattern not explicitly checked
- [x] **Silent failure mode** - MediatR doesn't warn about handlers it can't find
- [ ] Didn't query Context7 for API documentation
- [x] **Assumed behavior** - Expected handlers to be discovered regardless of namespace

### Impact Analysis
- **Scope**: Infrastructure layer (DI container), affecting all layers that depend on these handlers
- **Blast Radius**: 
  - 13 DI resolution tests failing
  - VS_003A Phase 4 CQRS handlers completely non-functional
  - Pipeline blocked until fixed
- **Technical Debt**: None - simple fix with no architectural changes needed

### Fix Details
```csharp
// Before (problematic code) - ApplyMatchRewardsCommandHandler.cs
namespace BlockLife.Features.Player.Commands  // WRONG - outside Core assembly
{
    public class ApplyMatchRewardsCommandHandler : IRequestHandler<ApplyMatchRewardsCommand, Fin<PlayerState>>
    {
        // Handler implementation
    }
}

// After (fixed code)
namespace BlockLife.Core.Features.Player.Commands  // CORRECT - inside Core assembly
{
    public class ApplyMatchRewardsCommandHandler : IRequestHandler<ApplyMatchRewardsCommand, Fin<PlayerState>>
    {
        // Handler implementation
    }
}

// GameStrapper.cs - RegisterCoreServices method
// Before (missing registration)
private static void RegisterCoreServices(IServiceCollection services)
{
    // ... other services
    // IPlayerStateService not registered!
}

// After (with registration)
private static void RegisterCoreServices(IServiceCollection services)
{
    // ... other services
    
    // --- Player State Service (VS_003A) ---
    services.AddSingleton<IPlayerStateService, PlayerStateService>();
}
```

### Prevention Measures

#### Immediate Actions
- [x] **Add specific test case** - Run DI tests immediately after adding new handlers
- [x] **Update developer checklist** - "Verify namespace starts with BlockLife.Core"
- [x] **Document in HANDBOOK.md** - Add namespace requirements to CQRS section
- [ ] **Add analyzer rule** - Consider Roslyn analyzer for namespace validation
- [x] **Quick validation command** - Add to build.ps1: `test-di` target

#### Systemic Changes
- [x] **Handler creation protocol** - Always copy from Move Block reference, including namespace
- [x] **PR template update** - Add "DI tests pass" checkbox
- [x] **Build script enhancement** - Fail fast on DI resolution errors
- [ ] **Consider explicit registration** - Evaluate if manual registration is clearer than auto-discovery
- [x] **Namespace validation script** - PowerShell script to verify all handlers in correct namespace

### Lessons Learned

**Key Takeaway**: MediatR's assembly scanning is powerful but silent about what it misses. When using auto-discovery patterns in Clean Architecture, namespace alignment with assembly boundaries is critical.

**For the Team**:
1. **Trust but Verify** - Auto-discovery is convenient but needs validation tests
2. **IDE Defaults Can Mislead** - Folder structure doesn't always match namespace requirements  
3. **Reference Patterns are Complete** - Move Block includes correct namespaces for a reason
4. **Fail Fast** - DI resolution tests should run early and often in development
5. **Silent Failures are Dangerous** - Prefer explicit failures over silent skips

### Action Items
1. **Immediate** (Before Fix):
   - [ ] Run `./scripts/core/build.ps1 test` to confirm only 13 DI tests failing
   - [ ] Document namespace requirement in handler file headers

2. **With Fix** (TD_068):
   - [ ] Fix all 6 file namespaces to include "Core"
   - [ ] Add IPlayerStateService registration to GameStrapper
   - [ ] Verify all 320 tests pass

3. **Post-Fix**:
   - [ ] Add regression test for handler discovery
   - [ ] Update HANDBOOK.md with namespace guidance
   - [ ] Create namespace validation script

### Namespace Validation Script (Proposed)
```powershell
# Check-HandlerNamespaces.ps1
$handlers = Get-ChildItem -Path "src/Features" -Filter "*Handler.cs" -Recurse
$invalid = @()

foreach ($handler in $handlers) {
    $content = Get-Content $handler.FullName -Raw
    if ($content -notmatch 'namespace\s+BlockLife\.Core\.Features') {
        $invalid += $handler.Name
    }
}

if ($invalid.Count -gt 0) {
    Write-Error "Handlers with invalid namespaces: $($invalid -join ', ')"
    exit 1
}
```

---

**Related:**
- **Backlog Item**: TD_068 - Fix DI Registration for VS_003A CQRS Handlers
- **Feature**: VS_003A - Match-3 with Attributes Phase 4
- **Reference Pattern**: src/Features/Block/Move/ (correct namespace example)
- **Similar Issues**: None yet (first namespace-related DI failure)