# Implementation Plan: Solution and Project Structure

This document provides the definitive, concrete implementation of the solution and project structure as mandated by the **Project Architecture Guide (v2.9)**. It synthesizes the requirements from all architectural and implementation-plan documents into a single, actionable blueprint. Adherence to this structure is mandatory to ensure compiler-enforced architectural boundaries.

## 1. Top-Level Directory Structure

The root `BlockLife/` directory will be organized as follows, separating the solution file, the pure C# source, the Godot-specific project, and the tests.

```
/BlockLife/
├── BlockLife.sln
├── .gitignore
├── README.md
│
├── src/
│   ├── BlockLife.Core.csproj
│   └── (All pure C# logic folders...)
│
├── godot_project/
│   ├── BlockLife.Godot.csproj
│   ├── project.godot
│   └── (All Godot scenes, assets, and Node scripts...)
│
├── tests/
│   ├── BlockLife.Core.Tests.csproj
│   └── (All C# unit test folders...)
│
└── docs/
    └── (All project documentation...)
```

## 2. Solution File (`BlockLife.sln`)

This solution file binds the three core projects together, enabling developers to work on the entire codebase from a single IDE instance.

```solution
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BlockLife.Core", "src\BlockLife.Core.csproj", "{1EADF7A5-7A36-4B59-84DA-3A1D54A3D4E0}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BlockLife.Godot", "godot_project\BlockLife.Godot.csproj", "{2A5AE2A4-1B9C-4E7A-9C3C-9E0D7F6B8F1A}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BlockLife.Core.Tests", "tests\BlockLife.Core.Tests.csproj", "{3B6C2B7C-2D1E-4F8A-B8A9-C0E9E6F2A1B3}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{1EADF7A5-7A36-4B59-84DA-3A1D54A3D4E0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{1EADF7A5-7A36-4B59-84DA-3A1D54A3D4E0}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{1EADF7A5-7A36-4B59-84DA-3A1D54A3D4E0}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{1EADF7A5-7A36-4B59-84DA-3A1D54A3D4E0}.Release|Any CPU.Build.0 = Release|Any CPU
		{2A5AE2A4-1B9C-4E7A-9C3C-9E0D7F6B8F1A}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{2A5AE2A4-1B9C-4E7A-9C3C-9E0D7F6B8F1A}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{2A5AE2A4-1B9C-4E7A-9C3C-9E0D7F6B8F1A}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{2A5AE2A4-1B9C-4E7A-9C3C-9E0D7F6B8F1A}.Release|Any CPU.Build.0 = Release|Any CPU
		{3B6C2B7C-2D1E-4F8A-B8A9-C0E9E6F2A1B3}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{3B6C2B7C-2D1E-4F8A-B8A9-C0E9E6F2A1B3}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{3B6C2B7C-2D1E-4F8A-B8A9-C0E9E6F2A1B3}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{3B6C2B7C-2D1E-4F8A-B8A9-C0E9E6F2A1B3}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
```

## 3. Core Logic Project (`src/BlockLife.Core.csproj`)

This project contains the pure, Godot-agnostic C# logic. It **must not** reference any Godot libraries.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>BlockLife.Core</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="LanguageExt.Core" Version="4.4.8" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>

</Project>
```

## 4. Godot Project (`godot_project/BlockLife.Godot.csproj`)

This is the main C# project that Godot builds. It contains all Godot-specific logic (`View` implementations, `Controller Nodes`) and is the only place where `using Godot;` is permitted outside of Presenters.

```xml
<Project Sdk="Godot.NET.Sdk/4.2.2">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>BlockLife.Godot</RootNamespace>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
    <!-- This reference is CRITICAL. It allows Godot code to access pure logic. -->
    <ProjectReference Include="..\src\BlockLife.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Mapperly is included here, at the boundary, as per its implementation plan. -->
    <PackageReference Include="Riok.Mapperly" Version="3.5.1" />
  </ItemGroup>

</Project>
```

## 5. Test Project (`tests/BlockLife.Core.Tests.csproj`)

This project contains all unit tests for the `BlockLife.Core` project. These tests run in a standard .NET environment, completely independent of the Godot engine.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>BlockLife.Core.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- This reference allows tests to access the logic they are testing. -->
    <ProjectReference Include="..\src\BlockLife.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="xunit" Version="2.8.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

## 6. Detailed Folder Structure with Examples

This tree illustrates where the components from the various implementation plans would reside within this structure.

```
/BlockLife/
├── BlockLife.sln
│
├── src/
│   ├── BlockLife.Core.csproj
│   └── Core/
│       ├── Application/
│       │   ├── Async/
│       │   │   └── IAsyncOperation.cs
│       │   ├── Commands/
│       │   │   ├── ICommand.cs
│       │   │   └── IQuery.cs
│       │   ├── Rules/
│       │   │   └── IValidationRule.cs
│       │   └── Simulation/
│       │       ├── Effects/
│       │       │   └── BlockMovedEffect.cs
│       │       ├── Notifications/
│       │       │   └── BlockMovedNotification.cs
│       │       ├── IEffect.cs
│       │       └── ISimulationManager.cs
│       │
│       ├── Domain/
│       │   ├── Block/
│       │   │   ├── Block.cs
│       │   │   └── BlockErrorCodes.cs
│       │   └── Common/
│       │       ├── Dto.cs
│       │       └── Entity.cs
│       │
│       ├── Infrastructure/
│       │   └── (Persistence, repositories, etc.)
│       │
│       ├── Presentation/
│       │   ├── IPresenterContainer.cs
│       │   ├── IPresenterFactory.cs
│       │   ├── PresenterBase.cs
│       │   ├── PresenterFactory.cs
│       │   └── PresenterLifecycleManager.cs
│       │
│       └── System/
│           ├── ISystemStateService.cs
│           └── SystemStateService.cs
│
│   └── Features/
│       ├── Authentication/
│       │   ├── AuthOrchestrator.cs
│       │   ├── IAuthOrchestrator.cs
│       │   ├── LoginPresenter.cs
│       │   └── ProcessLoginResultCommand.cs
│       │
│       └── Block/
│           └── Move/
│               ├── MoveBlockCommand.cs
│               └── MoveBlockCommandHandler.cs
│
├── godot_project/
│   ├── BlockLife.Godot.csproj
│   ├── project.godot
│   ├── icon.svg
│   │
│   ├── Mappers/
│   │   └── NumericsMapper.cs
│   │
│   ├── scenes/
│   │   ├── Main/
│   │   │   ├── SceneRoot.cs
│   │   │   └── SceneRoot.tscn
│   │   └── Grid/
│   │       ├── GridView.cs
│   │       └── GridView.tscn
│   │
│   └── features/  (Mirrors `src/Features` for Godot-specific parts)
│       └── block/
│           └── move/
│               ├── BlockMoveView.cs
│               └── BlockMoveView.tscn
│
└── tests/
    ├── BlockLife.Core.Tests.csproj
    └── Core/
        ├── Builders/
        │   └── GridStateBuilder.cs
        ├── Utils/
        │   └── EffectCollector.cs
        └── Features/
            └── Block/
                └── Move/
                    └── MoveBlockCommandHandlerTests.cs
```
