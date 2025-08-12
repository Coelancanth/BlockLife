# Implementation Plan: `dotnet new` Architectural Templates

This document outlines the detailed plan to implement Section 6.2 of the Architecture Guide. The primary goal is to create a suite of `dotnet new` templates that automate the generation of boilerplate code for new features, ensuring consistency and reducing development friction. By making the "correct way" the "easiest way," we can significantly improve developer productivity and adherence to the established architecture.

## Phase 1: Create the Core "Feature Slice" Template

This is the most critical template, responsible for scaffolding an entire vertical feature slice as described in the architecture guide.

### 1.1. Template Definition

-   **Template Name:** `BlockLife Feature Slice`
-   **Short Name:** `blocklife-feature`
-   **Invocation:** `dotnet new blocklife-feature -FN {FeatureName} -SN {SubFeatureName} [-T {GodotNodeType}]`
    -   Example: `dotnet new blocklife-feature -FN Block -SN Freeze -T Node2D`

### 1.2. Template Parameters

-   **(Required)** `FeatureName` (`-FN`): The name of the main feature (e.g., "Block"). This will be used for the top-level feature folder and the feature part of the namespace.
-   **(Required)** `SubFeatureName` (`-SN`): The name of the sub-feature or specific slice (e.g., "Freeze"). This will be used for the sub-folder, class names, and interfaces.
-   **(Optional)** `GodotNodeType` (`-T`): The base Godot Node type for the `View` script.
    -   **Default:** `Node2D`
    -   This allows for flexibility when creating UI features (`Control`) or 3D features (`Node3D`).

### 1.3. File Structure and Content

The template will be contained in a folder named `templates/blocklife-feature`. It will have the following structure, which mirrors the project's architecture, using placeholders that will be replaced by the template engine:

```
templates/blocklife-feature/
├── .template.config/
│   └── template.json
├── src/Features/FeatureNamespacePlaceholder/SubFeatureNamePlaceholder/
│   ├── ISubFeatureNamePlaceholderView.cs
│   ├── SubFeatureNamePlaceholderPresenter.cs
│   ├── SubFeatureNamePlaceholderCommand.cs
│   ├── SubFeatureNamePlaceholderHandler.cs
│   └── Tests/
│       └── SubFeatureNamePlaceholderPresenterTests.cs
└── godot_project/features/feature-name-kebabcase/sub-feature-name-kebabcase/
    └── SubFeatureNamePlaceholderView.cs
```

### 1.4. `template.json` Configuration

The `template.json` file will define the template's identity, parameters, and source file mappings. Careful design of the `sources` configuration is crucial to ensure files are placed in the correct relative paths and adhere to naming conventions (e.g., `kebab-case` for Godot paths, `PascalCase` for C# names). The `casing` generator will be heavily utilized for this.

```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "BlockLife Team",
  "classifications": [ "BlockLife", "Game", "Godot" ],
  "identity": "BlockLife.Feature.CSharp",
  "name": "BlockLife Feature Slice",
  "shortName": "blocklife-feature",
  "sourceName": "SubFeatureNamePlaceholder", // This is the placeholder for the SubFeatureName in file names and content
  "symbols": {
    "FeatureName": {
      "type": "parameter",
      "datatype": "text",
      "description": "The name of the main feature (e.g., 'Block').",
      "replaces": "FeatureNamespacePlaceholder" // Placeholder for the Feature part of the namespace
    },
    "SubFeatureName": {
      "type": "parameter",
      "datatype": "text",
      "description": "The name of the sub-feature or specific slice (e.g., 'Freeze').",
      "replaces": "SubFeatureNamePlaceholder" // This will replace the sourceName placeholder
    },
    "GodotNodeType": {
      "type": "parameter",
      "datatype": "text",
      "defaultValue": "Node2D",
      "replaces": "NodeType"
    },
    // Generated symbols for casing transformations (e.g., for Godot kebab-case paths)
    "feature-name-kebabcase": {
      "type": "generated",
      "generator": "casing",
      "parameters": { "source": "FeatureName", "to": "kebab" }
    },
    "sub-feature-name-kebabcase": {
      "type": "generated",
      "generator": "casing",
      "parameters": { "source": "SubFeatureName", "to": "kebab" }
    }
  },
  "sources": [
    {
      "rename": {
        // Example renames. Actual implementation will require careful path and file renaming
        // using the placeholders and generated casing symbols.
        "ISubFeatureNamePlaceholderView.cs": "I{SubFeatureName}View.cs",
        // ... other renames
      }
    }
  ]
}
```
*(Note: The JSON above is a conceptual representation. The final implementation will require careful handling of path and file renames using the defined symbols and generated casing parameters.)*

### 1.5. Template File Content (Examples)

**`ISubFeatureNamePlaceholderView.cs`:**
```csharp
namespace Company.FeatureNamespacePlaceholder;

public interface ISubFeatureNamePlaceholderView
{
    // Methods to be implemented by the view
}
```

**`SubFeatureNamePlaceholderPresenter.cs`:**
```csharp
namespace Company.FeatureNamespacePlaceholder;

public class SubFeatureNamePlaceholderPresenter : PresenterBase<ISubFeatureNamePlaceholderView>
{
    public SubFeatureNamePlaceholderPresenter(ISubFeatureNamePlaceholderView view) : base(view) { }

    // TODO: Add methods to handle view events and update view state.
}
```

**`SubFeatureNamePlaceholderHandler.cs`:**
```csharp
using System.Threading;
using System.Threading.Tasks;
using MediatR; // Assuming MediatR for IRequestHandler and Unit

namespace Company.FeatureNamespacePlaceholder;

public class SubFeatureNamePlaceholderHandler : IRequestHandler<SubFeatureNamePlaceholderCommand, Result<Unit>>
{
    public SubFeatureNamePlaceholderHandler(/* Inject dependencies here */)
    {
    }

    public Task<Result<Unit>> Handle(SubFeatureNamePlaceholderCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implement your business logic here.
        // Example: return Result.Success(Unit.Value); or Result.Failure("Error message");
        return Task.FromResult(Result.Success(Unit.Value));
    }
}
```

**`godot_project/.../SubFeatureNamePlaceholderView.cs`:**
```csharp
using Godot;
using System;
using Company.FeatureNamespacePlaceholder; // Namespace from src
using System.Linq; // Example using directive

public partial class SubFeatureNamePlaceholderView : NodeType, ISubFeatureNamePlaceholderView, IPresenterContainer<SubFeatureNamePlaceholderPresenter>
{
    public SubFeatureNamePlaceholderPresenter Presenter { get; set; }
    private IDisposable _lifecycleManager;

    public override void _Ready()
    {
        _lifecycleManager = SceneRoot.Instance?.CreatePresenterFor(this);
    }

    public override void _ExitTree()
    {
        _lifecycleManager?.Dispose();
    }
}
```

## Phase 2: Create Granular Component Templates

To support adding parts to an *existing* feature, we will create smaller, more focused templates.

-   **`blocklife-command`**: Creates a `...Command.cs` and `...Handler.cs` pair in an existing feature folder.
    -   `dotnet new blocklife-command -n {CommandName} -f {Feature.SubFeature}`
-   **`blocklife-query`**: Creates a `...Query.cs` and `...Handler.cs` pair.
    -   `dotnet new blocklife-query -n {QueryName} -f {Feature.SubFeature}`
-   **`blocklife-rule`**: Creates a `...Rule.cs` and its test file.
    -   `dotnet new blocklife-rule -n {RuleName} -f {Feature.SubFeature}`

Each of these will have its own template folder and a simpler `template.json` configuration.

## Phase 3: Packaging and Distribution

To make the templates easily accessible to the entire team, we will package them into a single NuGet package.

1.  **Create a Solution Folder:** A new solution folder, `build/templates/`, will be created to house all template projects.
2.  **Create a `csproj` for the Package:** A single C# project file will be configured to act as the manifest for the NuGet package. This project won't contain any compilable code; its purpose is to define package metadata and include the template files.
    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <PackageType>Template</PackageType>
        <PackageId>BlockLife.Templates</PackageId>
        <Title>Architectural Templates for BlockLife</Title>
        <Version>1.0.0</Version>
        <!-- other metadata -->
      </PropertyGroup>
      <ItemGroup>
        <Content Include="templates\**\*" Exclude="**\bin\**\*;**\obj\**\*" />
        <Compile Remove="**\*" />
      </ItemGroup>
    </Project>
    ```
3.  **Packing:** Running `dotnet pack` on this project will generate `BlockLife.Templates.1.0.0.nupkg`.
4.  **Distribution:**
    -   **Local Install:** For testing, developers can run `dotnet new --install /path/to/BlockLife.Templates.1.0.0.nupkg`.
    -   **Team-wide:** The `.nupkg` will be pushed to a private NuGet feed (e.g., GitHub Packages, Azure Artifacts). Team members will configure this feed once and can then install the templates with `dotnet new --install BlockLife.Templates`.

## Phase 4: Documentation and Training

A tool is only effective if people know how to use it.

1.  **Create `Tooling_Guide.md`:** A new document will be added to `Docs/1_Architecture/` (or a new `Docs/6_Tooling/` folder).
2.  **Content:** This guide will detail:
    -   How to install the template package from the NuGet feed.
    -   How to **update** the template package to a newer version (`dotnet new --update-apply`).
    -   A complete reference for each available template (`blocklife-feature`, `blocklife-command`, etc.).
    -   Usage examples for each template, explaining all parameters.
3.  **Onboarding:** The tooling guide will become a mandatory part of the onboarding process for new developers.
4.  **Ownership and Maintenance:** To ensure long-term sustainability and responsiveness to architectural evolution, a designated owner or a small group (e.g., the "Architecture Core Group") will be responsible for maintaining these templates, responding to change requests, and publishing new versions.