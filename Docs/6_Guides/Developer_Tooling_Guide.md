# Developer Tooling Guide (v1.0)

This guide provides instructions on how to use the custom `dotnet new` templates created for this project. These tools are a **mandatory** part of our development workflow, as defined in the Architecture Guide. Their purpose is to accelerate development, reduce boilerplate, and ensure all new code strictly adheres to the established architectural patterns.

---

## 1. Installation

Before you can use the templates, you must install them into your local `dotnet` SDK. You only need to do this once.

From the root directory of the project, run the following commands in your terminal:

```bash
dotnet new install ./templates/new-rule
dotnet new install ./templates/new-controller
dotnet new install ./templates/new-feature
```

If the templates are ever updated, you can re-run these commands to install the latest versions.

To verify the installation, you can run `dotnet new list` and search for the template short names: `bl-rule`, `bl-controller`, and `bl-feature`.

---

## 2. The Templates

### 2.1 New Validation Rule (`bl-rule`)

This is the simplest template. It scaffolds a new validation rule service and its corresponding unit test. Rules are used within Command Handlers to keep validation logic clean and reusable.

**Usage:**

```bash
dotnet new bl-rule --name <RuleName> --output <OutputPath>
```

-   `--name`: The name of the rule (e.g., `BlockIsNotLocked`). Do not include "Rule" in the name.
-   `--output`: The directory where the files will be created.

**Example:**

Let's create a rule to check if a block is not locked.

```bash
# From the project root directory
dotnet new bl-rule --name BlockIsNotLocked --output src/Core/Features/Block/Rules
dotnet new bl-rule --name BlockIsNotLocked --output tests/BlockLife.Core.Tests/Features/Block/Rules
```

**Resulting Files:**

This will generate two files:

1.  `src/Core/Features/Block/Rules/BlockIsNotLockedRule.cs`: The rule implementation.
2.  `tests/BlockLife.Core.Tests/Features/Block/Rules/BlockIsNotLockedRuleTests.cs`: The unit test for the rule.

You will then need to:
1.  Open the generated rule file and implement its `Check` method with the correct logic and context type.
2.  Open the generated test file and write meaningful unit tests.
3.  Register the new rule in the DI container in `GameStrapper.cs`.

---

### 2.2 New Controller (`bl-controller`)

This template scaffolds a new **Controller Node** and its corresponding **View Sub-Interface**. This is used to enforce the "Humble Presenter" and "Composite View" patterns. The controller is a specialized Godot `Node` that handles a specific piece of presentation logic (like animation or VFX), and its interface is what the Presenter uses to delegate tasks to it.

**Usage:**

```bash
dotnet new bl-controller --name <ControllerName> --feature <FeaturePath> --output <OutputPath>
```

-   `--name`: The name of the controller (e.g., `PlayerAnimation`). Do not include "Controller" in the name.
-   `--feature`: The feature path within the `Core` project (e.g., `Player/Skills`).
-   `--output`: The directory where the files will be created.

**Example:**

Let's create an animation controller for the player.

```bash
# Create the interface in the Core project
dotnet new bl-controller --name PlayerAnimation --feature Player/Skills --output src/Core/Features/Player/Skills

# Create the implementation in the Godot project
dotnet new bl-controller --name PlayerAnimation --feature Player/Skills --output godot_project/features/player/skills
```

**Resulting Files:**

1.  `src/Core/Features/Player/Skills/IPlayerAnimationView.cs`: The sub-interface defining the controller's capabilities.
2.  `godot_project/features/player/skills/PlayerAnimationController.cs`: The Godot script implementing the interface.

You will then need to:
1.  Define the required methods in the `IPlayerAnimationView` interface.
2.  Implement those methods in the `PlayerAnimationController.cs` script.
3.  Add the `PlayerAnimationController` as a child node in your main view's scene tree.
4.  Expose the controller through your main composite view interface (e.g., `IPlayerView`) so the Presenter can access it.

---

### 2.3 New Feature (`bl-feature`)

This is the most comprehensive template. It scaffolds the entire vertical slice for a new feature, creating all the necessary files across the `src`, `godot_project`, and `tests` directories.

**Usage:**

```bash
dotnet new bl-feature --name <FeatureName> --output .
```

-   `--name`: The name of the feature in PascalCase (e.g., `PlayerHealth`).
-   `--output`: The root directory of the project. Use `.` to generate files in their correct locations.

**Example:**

Let's create a new feature for managing player health.

```bash
# Run from the project root directory
dotnet new bl-feature --name PlayerHealth --output .
```

**Resulting Files:**

This command creates a complete, architecturally-compliant folder structure and file set for the "PlayerHealth" feature:

-   `src/Core/Features/PlayerHealth/`
    -   `PlayerHealthCommand.cs`
    -   `PlayerHealthCommandHandler.cs`
    -   `IPlayerHealthView.cs`
    -   `PlayerHealthPresenter.cs`
-   `godot_project/features/PlayerHealth/`
    -   `PlayerHealthView.cs`
-   `tests/BlockLife.Core.Tests/Features/PlayerHealth/`
    -   `PlayerHealthCommandHandlerTests.cs`

You will then need to go through each generated file and fill in the `// TODO:` sections with the specific logic for your feature. This template lays the foundation, but you provide the implementation details.
