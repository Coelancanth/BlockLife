# Context7 Library Coverage Matrix for BlockLife

**Updated**: 2025-08-18 by Tech Lead
**Token Cost**: ~3,500 tokens per query - Use strategically!

## 📊 Complete Library Inventory

### 🎯 Tier 1: Critical Libraries (Mandatory Context7)
*High bug risk - assumption errors cost 2+ hours debugging*

| Library | Context7 ID | Trust Score | Snippets | Risk Level | Token ROI |
|---------|-------------|-------------|----------|------------|-----------|
| **LanguageExt** | `/louthy/language-ext` | 9.4 | Unknown | CRITICAL | HIGH ⭐⭐⭐ |
| **MediatR** | `/jbogard/mediatr` | 10.0 | Unknown | CRITICAL | HIGH ⭐⭐⭐ |
| ~~**Godot**~~ | ~~`/godotengine/godot`~~ | ~~9.9~~ | ❌ **NOT C# API** | LOW | ❌ NO VALUE |

**Usage Pattern**: ALWAYS query before unfamiliar usage, overriding methods, or complex patterns.

⚠️ **CRITICAL NOTE**: Godot Context7 docs are NOT the C# API we use - they're engine internals and build scripts.

### 🧪 Tier 2: Testing Libraries (Strategic Context7)
*Medium bug risk - useful for advanced scenarios*

| Library | Context7 ID | Trust Score | Snippets | Risk Level | Token ROI |
|---------|-------------|-------------|----------|------------|-----------|
| **FluentAssertions** | `/fluentassertions/fluentassertions` | 8.6 | 1,554 | MEDIUM | MEDIUM ⭐⭐ |
| **xUnit Core** | `/xunit/xunit` | 8.8 | 5 | LOW | LOW ⭐ |
| **xUnit Docs** | `/websites/api_xunit_net-v3-2.0.3-xunit.html` | 7.5 | 1,524 | MEDIUM | MEDIUM ⭐⭐ |
| **Moq** | `/devlooped/moq` | 9.5 | 54 | MEDIUM | MEDIUM ⭐⭐ |
| **GdUnit4 Docs** | `/websites/mikeschulze_github_io-gdunit4` | 7.5 | 397 | HIGH | HIGH ⭐⭐⭐ |
| **GdUnit4 .NET** | `/mikeschulze/gdunit4net` | 8.0 | 49 | LOW | LOW ⭐ |
| **FsCheck** | `/fscheck/fscheck` | 5.2 | 143 | MEDIUM | ⚠️ PENDING ⭐ |

**Usage Pattern**: Query for advanced testing scenarios, complex assertions, custom extensions.

### ⚡ Tier 3: Supporting Libraries (Selective Context7)
*Lower bug risk - query only for complex usage*

| Library | Context7 ID | Trust Score | Snippets | Risk Level | Token ROI |
|---------|-------------|-------------|----------|------------|-----------|
| **System.Reactive** | `/dotnet/reactive` | 8.3 | 518 | MEDIUM | MEDIUM ⭐⭐ |
| **Serilog** | `/serilog/serilog` | 7.2 | 8 | LOW | LOW ⭐ |

**Usage Pattern**: Query for advanced reactive patterns, complex logging scenarios.

### 🔧 Tier 4: Infrastructure (DevOps Context7)
*Automation and tooling support*

| Library | Context7 ID | Trust Score | Snippets | Risk Level | Token ROI |
|---------|-------------|-------------|----------|------------|-----------|
| **Claude Code** | `/websites/docs_anthropic_com-en-docs-claude-code` | 7.5 | 440 | LOW | LOW ⭐ |
| **ccstatusline** | `/sirmalloc/ccstatusline` | 7.0 | 28 | LOW | LOW ⭐ |

**Usage Pattern**: Query for advanced automation scenarios, troubleshooting.

### ⚠️ Partially Available in Context7
*Listed in search but may have access issues*

| Library | Context7 Status | Alternative Documentation | Usage Frequency |
|---------|-----------------|---------------------------|------------------|
| **FsCheck** | ⚠️ Listed but 404 errors | [Official FsCheck docs](https://fscheck.github.io/FsCheck/) | MEDIUM |

### ❌ Missing from Context7
*Use official documentation instead*

| Library | Alternative Documentation | Usage Frequency |
|---------|---------------------------|-----------------|
| **Godot C# API** | [Official C# Tutorials](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) | CRITICAL |
| **Microsoft.Extensions.DependencyInjection** | .NET Runtime docs | HIGH |
| **FluentAssertions.LanguageExt** | Package README | LOW |

## 💰 Token Economics Analysis

### High ROI Queries (Worth 3,500 tokens)
```bash
# LanguageExt complex patterns - saves 2+ hours
mcp__context7__get-library-docs "/louthy/language-ext" --topic "Error Fin bind chain ToEff"

# MediatR DI integration issues - saves 1+ hours  
mcp__context7__get-library-docs "/jbogard/mediatr" --topic "IRequestHandler registration ServiceCollection"

# Godot C# API - NOT AVAILABLE in Context7 ❌
# Use: Official Godot C# API docs instead
```

### Medium ROI Queries (Selective use)
```bash
# FluentAssertions custom extensions
mcp__context7__get-library-docs "/fluentassertions/fluentassertions" --topic "Should custom extensions collections"

# System.Reactive complex patterns
mcp__context7__get-library-docs "/dotnet/reactive" --topic "Observable Subscribe Dispose memory leaks"

# GdUnit4 comprehensive testing documentation (HIGH VALUE)
mcp__context7__get-library-docs "/websites/mikeschulze_github_io-gdunit4" --topic "C# TestSuite assertions scene runner"
```

### Low ROI Queries (Avoid unless critical)
```bash
# Basic xUnit usage - use quick reference instead
# Basic Serilog configuration - use official quickstart
# Simple DI registration - use .NET docs
```

## 🎯 Usage Decision Matrix

| Scenario | Context7? | Alternative | Rationale |
|----------|-----------|-------------|-----------|
| First LanguageExt Error usage | ✅ YES | None | High bug risk |
| Basic IServiceCollection.AddScoped | ❌ NO | .NET docs | Simple, well-documented |
| MediatR pipeline behaviors | ✅ YES | None | Complex, error-prone |
| Simple [Fact] test method | ❌ NO | Team patterns | Established convention |
| GdUnit4 advanced testing | ✅ YES | None | Complex scene testing, signals |
| Basic GdUnit4 assertions | ❌ MAYBE | Team patterns | Simple tests don't need docs |
| Godot _Ready override | ✅ YES | None | Lifecycle timing critical |
| Basic Serilog.Log.Information | ❌ NO | Serilog quickstart | Straightforward usage |
| FluentAssertions Should().BeEquivalentTo | ✅ MAYBE | Team patterns | Complex comparison rules |

## 📋 Team Guidelines

### Before Using Context7, Ask:
1. **Is this a complex/unfamiliar API?** → Use Context7
2. **Am I overriding virtual/abstract methods?** → Use Context7  
3. **Have I used this pattern successfully before?** → Skip Context7
4. **Does Move Block have this pattern?** → Copy Move Block
5. **Is this basic C# syntax?** → Skip Context7

### Cost Control Measures:
- **Batch queries**: Combine related topics
- **Document discoveries**: Add to QuickReference.md
- **Share knowledge**: Prevent duplicate queries
- **Use targeted topics**: Specific vs broad queries

## 🔄 Maintenance Protocol

**Monthly Review**:
- Update trust scores and snippet counts
- Add new libraries discovered in dependencies
- Remove deprecated or unused libraries
- Adjust ROI ratings based on team experience

**When Adding New Dependencies**:
1. Check Context7 availability with `resolve-library-id`
2. Add to appropriate tier based on complexity/risk
3. Update this matrix
4. Add examples to Context7Examples.md

---
*This matrix provides the strategic framework for cost-effective Context7 usage across our complete technology stack.*