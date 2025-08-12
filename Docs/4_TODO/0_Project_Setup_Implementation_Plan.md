# Project Setup Implementation Plan (v1.0)

This document outlines the steps to restructure the project to align with the architecture defined in `Docs/1_Architecture/Architecture_Guide.md`. The goal is to establish a multi-project solution that enforces separation of concerns at the compiler level, as mandated by the guide.

This plan is based on **Section 8: Folder and Project Structure Guidance** of the architecture guide.

## 1. Folder Structure Realignment

The first step is to create the physical directories that will house the separated C# projects.

**Action:**
1.  Create a new directory named `src` at the project root (`C:\Users\Coel\Documents\Godot\blocklife\src`). This folder will contain all pure, Godot-agnostic C# core logic.
2.  Create a new directory named `tests` at the project root (`C:\Users\Coel\Documents\Godot\blocklife\tests`). This folder will contain all unit tests for the core logic.

## 2. Create Core Logic Project (`BlockLife.Core.csproj`)

This project will be the heart of the application's business logic, completely decoupled from Godot.

**Action:**
1.  Create a new C# class library project file at `src/BlockLife.Core.csproj`.
2.  The `.csproj` file should target .NET 8.0 and contain the following basic structure:
    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
      </PropertyGroup>
    </Project>
    ```
3.  This project **must not** reference any Godot assemblies. It will contain all pure business logic, services, handlers, DTOs, and view interfaces.

## 3. Create Core Tests Project (`BlockLife.Core.Tests.csproj`)

This project will ensure the core logic is robust and correct, following the guidelines in the `Test_Guide.md`.

**Action:**
1.  Create a new C# xUnit test project file at `tests/BlockLife.Core.Tests.csproj`.
2.  The `.csproj` file should target .NET 8.0 and include references to testing libraries (xUnit, Moq, Shouldly, etc.).
3.  It must contain a project reference to the core logic project:
    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
      </PropertyGroup>
      <ItemGroup>
        <ProjectReference Include="..\src\BlockLife.Core.csproj" />
      </ItemGroup>
      <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
        <PackageReference Include="xunit" Version="2.6.2" />
        <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4">
          <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
          <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <!-- Add other testing packages like Moq and Shouldly here -->
      </ItemGroup>
    </Project>
    ```

## 4. Update Solution File (`BlockLife.sln`)

The solution file must be updated to be aware of the new projects, creating the multi-project structure.

**Action:**
1.  Use the `dotnet sln` command or an IDE to add the two new projects to the existing `BlockLife.sln`.
    ```bash
    dotnet sln BlockLife.sln add src/BlockLife.Core.csproj
    dotnet sln BlockLife.sln add tests/BlockLife.Core.Tests.csproj
    ```
2.  The solution will now manage three projects: `BlockLife` (the Godot project), `BlockLife.Core`, and `BlockLife.Core.Tests`.

## 5. Update Godot Project (`BlockLife.csproj`)

Finally, the main Godot project must be configured to consume the pure core logic.

**Action:**
1.  Modify the existing `BlockLife.csproj` file.
2.  Add a project reference to `BlockLife.Core.csproj`. This allows Godot-aware code (Views, Presenters, Controller Nodes) to access the core logic and interfaces. The reference should look like this:
    ```xml
    <ItemGroup>
      <ProjectReference Include="src\BlockLife.Core.csproj" />
    </ItemGroup>
    ```

## 6. Install NuGet Packages

To support the architecture, specific NuGet packages must be installed into the correct projects.

**Action:**
Use the `dotnet add package` command to install the following dependencies.

**1. For `BlockLife.Core.csproj` (Pure Logic):**
These packages provide the core building blocks for our architecture.
```bash
dotnet add src/BlockLife.Core.csproj package MediatR --version 12.2.0
dotnet add src/BlockLife.Core.csproj package LanguageExt.Core --version 4.4.8
dotnet add src/BlockLife.Core.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
dotnet add src/BlockLife.Core.csproj package Serilog --version 3.1.1
```

**2. For `BlockLife.Core.Tests.csproj` (Unit Tests):**
These packages provide the tools for robust unit testing as defined in the `Test_Guide.md`.
```bash
dotnet add tests/BlockLife.Core.Tests.csproj package Moq --version 4.20.70
dotnet add tests/BlockLife.Core.Tests.csproj package FluentAssertions --version 6.12.0
```
*(Note: `xunit`, `xunit.runner.visualstudio`, and `Microsoft.NET.Test.Sdk` are already included in the template from Step 3).*

**3. For `BlockLife.csproj` (Godot Project):**
These packages provide the concrete implementations needed to run the application.
```bash
dotnet add BlockLife.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add BlockLife.csproj package Serilog.Sinks.Console --version 5.0.1
```

## Final Structure Overview

After completing these steps, the project structure will be fully aligned with the architectural guide, providing compiler-enforced separation of concerns.

```
/BlockLife/
├── BlockLife.sln
│
├── src/
│   └── BlockLife.Core.csproj
│
├── tests/
│   └── BlockLife.Core.Tests.csproj
│
├── BlockLife.csproj  <-- The Godot project, now references BlockLife.Core
└── ... (all other Godot assets and project files)
```

