# Context7 Usage Examples for BlockLife

## ✅ Available Libraries in Context7 Service (Verified)

**Context7-Indexed Libraries** (Use with MCP tools):
- **LanguageExt**: `/louthy/language-ext` (Trust Score: 9.4)
- **MediatR**: `/jbogard/mediatr` (Trust Score: 10.0) 
- **Godot**: `/godotengine/godot` (Trust Score: 9.9)
- **🆕 Claude Code**: `/websites/docs_anthropic_com-en-docs-claude-code` (Trust Score: 7.5, 440 snippets)
- **🆕 ccstatusline**: `/sirmalloc/ccstatusline` (Trust Score: 7, 28 snippets)

**✅ Complete Coverage**: All our automation tools are now in Context7!

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

## 🚀 Claude Code & ccstatusline Integration Examples

### 8. Custom Slash Commands (NEW!) ✅ NOW IN CONTEXT7

**Problem Prevention**: Understanding Claude Code slash command frontmatter and configuration

**Context7 Query** (Claude Code now indexed in Context7):
```bash
# Query Claude Code custom command documentation via Context7
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "custom commands frontmatter allowed_tools bash integration"
```

**What You'll Learn**:
- Frontmatter metadata format (`---description:`, `allowed_tools:`)
- Command parameter handling
- Bash tool integration patterns
- Project vs global command scope

**Real Example From Our System**:
```markdown
---
description: "Display current active persona for status line integration"
allowed_tools: ["bash"]
---

# Read current persona state, default to 'none' if not set
cat .claude/current-persona 2>/dev/null || echo "none"
```

### 9. ccstatusline Configuration ✅ NOW IN CONTEXT7

**Problem Prevention**: Understanding ccstatusline config syntax and custom commands

**Context7 Query** (ccstatusline now indexed in Context7):
```bash
# Query ccstatusline configuration documentation via Context7
mcp__context7__get-library-docs "/sirmalloc/ccstatusline" --topic "configuration statusLines custom commands local vs global"
```

**What You'll Learn**:
- Local vs global configuration precedence
- Custom command integration syntax
- Status line item types (git-branch, model, custom)
- Color and styling options

**Real Example From Our System**:
```json
{
  "statusLines": [
    {
      "items": [
        {
          "type": "custom",
          "command": "/persona/current",
          "label": "Persona",
          "icon": "👤"
        }
      ]
    }
  ]
}
```

### 10. MCP Tool Integration ✅ NOW IN CONTEXT7

**Problem Prevention**: Understanding Claude Code MCP (Model Context Protocol) tools

**Context7 Query** (Claude Code MCP docs now indexed in Context7):
```bash
# Query MCP integration documentation via Context7
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "MCP server integration tool discovery Context7"
```

**What You'll Learn**:
- How MCP tools appear as slash commands
- Tool discovery and registration
- Context7 MCP integration patterns
- Custom tool development

## Updated Quick Reference Queries

### DevOps & Automation Queries ✅ ALL NOW IN CONTEXT7
```bash
# Claude Code settings and configuration
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "settings local configuration statusLine permissions"

# ccstatusline custom commands  
mcp__context7__get-library-docs "/sirmalloc/ccstatusline" --topic "custom commands configuration examples project global"

# Slash command syntax
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "slash commands frontmatter allowed_tools"
```

### Core Framework Queries (Unchanged)
```bash
# LanguageExt Error handling
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error Fin Option Match"

# MediatR patterns  
mcp__context7__get-library-docs "/jbogard/MediatR" --topic "IRequestHandler INotificationHandler"

# Godot C# specifics
mcp__context7__get-library-docs "/godotengine/godot" --topic "GodotObject Node2D C#"
```

## Updated Integration Workflow

### Before Configuring Automation Tools
1. Check existing patterns in `.claude/commands/` and `.claude/settings.local.json`
2. If creating custom commands → Query Claude Code slash command docs
3. If configuring status line → Query ccstatusline documentation
4. If using MCP tools → Query MCP integration documentation

### DevOps Automation Scenarios ✅ ALL NOW IN CONTEXT7
```bash
# Before creating persona commands
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "bash tools integration custom commands state management"

# Before configuring ccstatusline
mcp__context7__get-library-docs "/sirmalloc/ccstatusline" --topic "project global configuration precedence troubleshooting detection"

# Before debugging status line issues
mcp__context7__get-library-docs "/websites/docs_anthropic_com-en-docs-claude-code" --topic "statusLine command execution working directory troubleshooting"
```

## Documentation Coverage Update

**🎉 COMPLETE CONTEXT7 COVERAGE ACHIEVED!**

**Context7 MCP service now covers our entire stack**:
- ✅ **Core Libraries**: LanguageExt, MediatR, Godot, xUnit
- ✅ **🆕 Automation Tools**: Claude Code (440 snippets), ccstatusline (28 snippets)

**Single-Tool Strategy**:
- **Use Context7 MCP tools for everything**: All our libraries and tools are indexed!
- **No more WebFetch needed**: Complete documentation through one unified interface

**🚫 Zero assumption bugs across our complete automation stack!**

---
*Context7 integration configured in `/context7.json` - our rules, patterns, and automation tools are indexed for AI assistance*