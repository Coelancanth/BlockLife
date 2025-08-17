# Context7 Usage Examples for BlockLife

## ✅ Available Libraries (Verified)

All our critical libraries are already in Context7:
- **LanguageExt**: `/louthy/language-ext` (Trust Score: 9.4)
- **MediatR**: `/jbogard/mediatr` (Trust Score: 10.0) 
- **Godot**: `/godotengine/godot` (Trust Score: 9.9)

No library submission needed - start querying immediately!

## Quick Start
Context7 provides instant documentation access through MCP tools. Use these examples to prevent the bugs we've encountered in post-mortems.

## Common Scenarios & Solutions

### 1. Before Creating Error Handlers (LanguageExt) ✅ VERIFIED AVAILABLE

**Problem from Post-Mortem**: "Error.Message returns just the error code, not the full message"

**Context7 Query**:
```bash
# Library already available! Direct query:
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error class Message property"
```

**✅ CONFIRMED**: Context7 documentation validates our finding - Error properties don't include message text!

**What You'll Learn**:
- `Error.Message` returns the code only
- Use `Error.ToString()` for full details
- Consider `Error.New(code, message)` pattern
- How to extract error details in tests

### 2. Before Implementing MediatR Handlers ✅ VERIFIED AVAILABLE

**Problem from Post-Mortem**: "DragPresenter auto-discovered as INotificationHandler, causing DI failures"

**Context7 Query**:
```bash
# Library already available! Direct query:
mcp__context7__get-library-docs "/jbogard/mediatr" --topic "IRequestHandler registration auto-discovery"
```

**What You'll Learn**:
- MediatR auto-discovers ALL IRequestHandler implementations
- Presenters should NOT implement INotificationHandler
- How to exclude types from auto-discovery
- Manual registration patterns

### 3. Before Overriding Base Class Methods

**Problem from Post-Mortem**: "Used OnViewAttached/OnViewDetached which don't exist"

**Context7 Query**:
```bash
# For our MVP framework
mcp__context7__get-library-docs "/blocklife/mvp" --topic "PresenterBase lifecycle methods"

# Or check our own docs
mcp__context7__get-library-docs "/blocklife" --topic "PresenterBase"
```

**What You'll Learn**:
- Actual methods are `Initialize()` and `Dispose()`
- Lifecycle order and timing
- What to put in each method

### 4. Before Using Godot Node Methods ✅ VERIFIED AVAILABLE

**Problem Prevention**: Verify Godot C# API before using

**Context7 Query**:
```bash
# Library already available! Direct query:
mcp__context7__get-library-docs "/godotengine/godot" --topic "Node2D C# methods"
```

**What You'll Learn**:
- Available virtual methods to override
- Signal patterns in C#
- Resource loading approaches
- Input handling methods

### 5. Before Writing Tests

**Problem from Post-Mortem**: "Used non-existent BlockFactory"

**Context7 Query**:
```bash
# Query our test utilities
mcp__context7__get-library-docs "/blocklife" --topic "test utilities BlockBuilder"
```

**What You'll Learn**:
- Use `BlockBuilder` not `BlockFactory`
- Available test helper methods
- Mock setup patterns
- Service registration for tests

### 6. Understanding Fin<T> Patterns

**Common Confusion**: How to handle Fin<Unit> in command handlers

**Context7 Query**:
```bash
# Get LanguageExt Fin documentation
mcp__context7__resolve-library-id "LanguageExt"
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Fin monad Unit"
```

**What You'll Learn**:
- Fin<T> success/failure patterns
- Why we use Fin<Unit> for commands
- How to chain Fin operations
- Test assertions for Fin results

### 7. Dependency Injection Patterns

**Problem Prevention**: Understand service lifetimes

**Context7 Query**:
```bash
# For Microsoft DI
mcp__context7__resolve-library-id "Microsoft.Extensions.DependencyInjection"
mcp__context7__get-library-docs "/dotnet/runtime" --topic "ServiceLifetime Singleton Scoped"
```

**What You'll Learn**:
- When to use Singleton vs Scoped
- How MediatR integrates with DI
- Service registration patterns
- Why DragStateService is Singleton

## Quick Reference Queries

### Most Common Queries for BlockLife
```bash
# LanguageExt Error handling
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error Fin Option Match"

# MediatR patterns  
mcp__context7__get-library-docs "/jbogard/MediatR" --topic "IRequestHandler INotificationHandler"

# Godot C# specifics
mcp__context7__get-library-docs "/godotengine/godot" --topic "GodotObject Node2D C#"

# xUnit test patterns
mcp__context7__get-library-docs "/xunit/xunit" --topic "Theory Fact Assert"
```

## When NOT to Use Context7

Context7 is powerful but not always necessary:
- ✅ USE for unfamiliar APIs
- ✅ USE when overriding methods
- ✅ USE after confusing errors
- ❌ SKIP for basic C# syntax
- ❌ SKIP for our well-documented patterns (check Move Block first)
- ❌ SKIP if you've used the API successfully before

## Integration with Workflow

### Before Starting Any Task
1. Check `src/Features/Block/Move/` for patterns
2. If using new framework features → Query Context7
3. If overriding base methods → Query Context7
4. If test fails mysteriously → Query Context7

### During Debugging
- Error message mentions unknown method → Context7
- DI registration fails → Context7 for MediatR/DI docs
- Type mismatch in functional code → Context7 for LanguageExt

### Before Code Review
- Verify your API usage matches documentation
- Check if there are better patterns available
- Confirm you're not using deprecated methods

## Pro Tips

1. **Batch Your Queries**: If implementing a feature, query all needed docs upfront
2. **Save Common Patterns**: Add frequently used patterns to QuickReference.md
3. **Trust But Verify**: Even with Context7, test your assumptions
4. **Document Gotchas**: When Context7 reveals a surprise, document it

## Remember

> "Five minutes with Context7 saves an hour of debugging" - Every developer who's fought with LanguageExt.Error.Message

The goal isn't to query everything, but to query strategically when:
- Using unfamiliar territory
- After encountering errors
- Before major implementations
- When something "should work" but doesn't

---
*Context7 integration configured in `/context7.json` - our rules and patterns are indexed for AI assistance*