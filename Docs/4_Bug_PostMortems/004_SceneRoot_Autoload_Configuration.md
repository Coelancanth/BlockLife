# Bug Post-Mortem: SceneRoot Autoload Configuration Error

## Bug ID: BPM-004
**Date**: 2025-08-13  
**Severity**: Critical (Application fails to start)  
**Component**: SceneRoot / Autoload System  
**Fixed In**: feature/move_block_vertical_slice

## Summary
The application failed to start due to multiple SceneRoot instantiation and missing LogSettingsController configuration. SceneRoot was incorrectly configured both as an autoload pointing to a script file AND attached to the main scene, causing duplicate instantiation.

## Timeline
- **Discovery**: Application crashed with "A second SceneRoot was instantiated" error
- **Root Cause Identified**: Dual instantiation from autoload and main scene
- **Secondary Issue**: LogSettingsControllerPath not configured
- **Fix Applied**: Created proper SceneRoot.tscn for autoload, removed from main scene

## Error Messages
```
ERROR: FATAL: A second SceneRoot was instantiated. Destroying self to prevent multiple DI containers.
ERROR: FATAL: LogSettingsControllerPath is not set in the editor. Application cannot start.
ERROR: Cannot create presenter BlockManagementPresenter: SceneRoot or PresenterFactory is null
```

## Root Cause Analysis

### The Problem Chain
1. **Incorrect Autoload Configuration**: 
   - `project.godot` had: `SceneRoot="*res://godot_project/scenes/Main/SceneRoot.cs"`
   - Autoloading a script file directly doesn't include node configuration

2. **Duplicate Instance in Main Scene**:
   - `main.tscn` also had SceneRoot script attached to its root node
   - This created a second instance when the main scene loaded

3. **Missing Node Configuration**:
   - LogSettingsController couldn't be configured via script autoload
   - Node paths and exports weren't properly set

### Initialization Order Issue
The incorrect setup caused:
1. First SceneRoot instance created by autoload (incomplete)
2. Second SceneRoot instance attempted from main scene
3. Singleton check prevented second instance
4. Views couldn't access a properly initialized SceneRoot

## The Fix

### Solution Implementation
1. **Created SceneRoot.tscn**: A proper scene file with configured nodes
2. **Updated Autoload**: Changed to load the scene file instead of script
3. **Cleaned Main Scene**: Removed SceneRoot from main.tscn

### Code Changes

#### Before (Incorrect)
**project.godot:**
```ini
[autoload]
SceneRoot="*res://godot_project/scenes/Main/SceneRoot.cs"  # ❌ Script file
```

**main.tscn:**
```
[node name="main" type="Node"]
script = ExtResource("SceneRoot.cs")  # ❌ Duplicate instance
_logSettingsControllerPath = NodePath("LogSettingsController")

[node name="LogSettingsController" type="Node" parent="."]
```

#### After (Correct)
**project.godot:**
```ini
[autoload]
SceneRoot="*res://godot_project/scenes/Main/SceneRoot.tscn"  # ✅ Scene file
```

**SceneRoot.tscn (new file):**
```
[node name="SceneRoot" type="Node"]
script = ExtResource("SceneRoot.cs")
_logSettingsControllerPath = NodePath("LogSettingsController")  # ✅ Configured

[node name="LogSettingsController" type="Node" parent="."]
script = ExtResource("LogSettingsController.cs")
# All properties properly configured
```

**main.tscn:**
```
[node name="main" type="Node"]  # ✅ No SceneRoot script

[node name="Grid" parent="." instance=ExtResource("grid.tscn")]
```

## Lessons Learned

### 1. Autoload Best Practices
- **Rule**: Always use scene files (.tscn) for autoloads that need configuration
- **Reason**: Script files alone can't include node hierarchy or export configurations
- **Exception**: Only use script autoloads for pure utility classes without node dependencies

### 2. Singleton Pattern in Godot
- **Discovery**: Godot's autoload system already provides singleton behavior
- **Implication**: Don't manually attach singleton scripts to scenes
- **Best Practice**: Use autoload exclusively for singletons

### 3. Scene Composition
- **Principle**: Autoload scenes should be self-contained with all dependencies
- **Pattern**: Include all required child nodes in the autoload scene
- **Benefit**: Ensures proper initialization before any other scenes load

## Architectural Guidelines Updated

### New Rules for Autoload Configuration
1. ✅ **DO** create dedicated .tscn files for autoload singletons
2. ✅ **DO** include all required child nodes in the autoload scene
3. ❌ **DON'T** autoload script files that need node configuration
4. ❌ **DON'T** manually instantiate autoload classes in other scenes

### SceneRoot Pattern
The correct SceneRoot pattern for this architecture:
```
project.godot
  └── Autoload: SceneRoot.tscn
       └── SceneRoot (Node)
            ├── Script: SceneRoot.cs
            ├── Export: _logSettingsControllerPath
            └── Child: LogSettingsController
                 └── Script: LogSettingsController.cs
                 └── All log level configurations
```

### View Access Pattern
Views should access SceneRoot using the autoload path:
```csharp
var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
```

## Testing Improvements

### Startup Validation
Added checks to detect configuration issues early:
1. SceneRoot singleton enforcement in `_EnterTree()`
2. Configuration validation in `_Ready()`
3. Clear error messages for missing dependencies

### Configuration Checklist
- [ ] SceneRoot.tscn exists with proper configuration
- [ ] project.godot autoload points to .tscn file
- [ ] No SceneRoot script in main scene
- [ ] LogSettingsController is child of SceneRoot
- [ ] All export paths are configured

## Impact on Existing Code

### Files Modified
1. `godot_project/scenes/Main/SceneRoot.tscn` - Created new scene file
2. `project.godot` - Updated autoload configuration
3. `godot_project/scenes/Main/main.tscn` - Removed SceneRoot

### Migration Guide
For projects with similar issues:
1. Create a dedicated scene file for your autoload singleton
2. Move all configuration to the scene file
3. Update project.godot to reference the .tscn file
4. Remove any manual instantiation from other scenes

## Prevention Measures

### 1. Documentation Updates
- Updated setup instructions in CLAUDE.md
- Added autoload configuration to architecture guide
- Included in development workflow checklist

### 2. Project Template
- Future project template should include properly configured SceneRoot.tscn
- Autoload configuration should be pre-set
- Comments warning against modification

### 3. Validation Script
Consider adding a validation script that checks:
- Autoload configuration consistency
- No duplicate singleton instantiations
- Required child nodes present

## Related Issues
- Builds on: BPM-001 (SceneRoot initialization order)
- Related to: BPM-003 (DI container presenter registration)
- Pattern used in: All views requiring presenter initialization

## Conclusion
This bug revealed a fundamental misunderstanding of Godot's autoload system. The fix establishes the correct pattern for singleton configuration in Godot projects using this architecture. The solution ensures SceneRoot is properly initialized with all its dependencies before any other scenes load, providing a stable foundation for the DI container and presenter system.