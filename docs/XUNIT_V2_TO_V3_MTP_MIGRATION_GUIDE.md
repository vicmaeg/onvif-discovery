# Migration Guide: XUnit v2 with VSTest to XUnit v3 with Microsoft Testing Platform (MTP) v2

## Table of Contents

- [Overview](#overview)
- [Why Migrate?](#why-migrate)
- [Advantages of XUnit v3](#advantages-of-xunit-v3)
- [Advantages of Microsoft Testing Platform](#advantages-of-microsoft-testing-platform)
- [Migration Steps](#migration-steps)
  - [1. Project File Changes](#1-project-file-changes)
  - [2. Code Changes](#2-code-changes)
  - [3. CI/CD Pipeline Changes](#3-cicd-pipeline-changes)
- [Command-Line Reference](#command-line-reference)
- [Troubleshooting](#troubleshooting)

---

## Overview

This guide provides comprehensive instructions for migrating from XUnit v2 with VSTest to XUnit v3 with Microsoft Testing Platform (MTP) v2. The migration involves three main areas:

1. **Package and project configuration updates**
2. **Code adaptations for breaking changes**
3. **CI/CD pipeline modifications** (specifically Azure DevOps with SonarQube/SonarCloud integration)

---

## Why Migrate?

### End of VSTest Era

Microsoft Testing Platform (MTP) represents the future of .NET testing. VSTest, the underlying driver for `dotnet test` since Visual Studio 2010, is being gradually deprecated in favor of MTP. Microsoft has made it clear that MTP will be the standard going forward.

### XUnit v3 as the Standard

XUnit v2 is in maintenance mode while v3 receives active development and new features. Migrating to v3 ensures access to:

- Latest bug fixes and performance improvements
- New assertion methods and test features
- Better alignment with modern .NET capabilities
- Native integration with emerging tools and platforms

---

## Advantages of XUnit v3

### Standalone Executables

Unlike v2 where test projects were libraries requiring external runners, v3 projects are standalone executables:

```bash
# Build and run directly
dotnet build
./bin/Debug/net8.0/YourTests.exe

# Or build and run in one step
dotnet run
```

This eliminates the Application Domain complexity and dependency resolution issues that plagued v2.

### Improved Async Support

XUnit v3 embraces modern async patterns:

- **ValueTask support**: Better performance for frequently synchronous operations
- **CancellationToken integration**: Tests can properly respect cancellation
- **IAsyncLifetime improvements**: Now inherits from `IAsyncDisposable` following .NET best practices
- **`async void` tests rejected**: Caught at runtime to prevent common async mistakes

### Performance Enhancements

- Faster test discovery and execution
- Reduced memory footprint
- Better parallelization control
- Streamlined startup process

---

## Advantages of Microsoft Testing Platform

### Modern Architecture

MTP is designed from the ground up as a modern testing engine:

- **Unified execution model**: Same runner for command line, IDE, and CI/CD
- **Process isolation**: Better reliability and resource cleanup
- **Streamlined communication**: Reduced overhead between runner and tests

### Superior Performance

- Lower startup overhead compared to VSTest
- More efficient test discovery
- Better memory management
- Faster test execution, especially for large test suites

### Extensibility System

MTP introduces a first-class extension system:

- **Code coverage**: Native support via `Microsoft.Testing.Extensions.CodeCoverage`
- **Reporting**: Multiple output formats (TRX, JUnit, NUnit, CTRF, HTML)
- **Custom extensions**: Write your own testing platform extensions

### Consistent Experience

- Same command-line interface across all test frameworks (xUnit, MSTest, TUnit)
- Unified configuration via `testconfig.json`
- Consistent output formatting
- Better integration with modern IDEs

### No Runner Dependencies

Test projects can run independently without:
- `xunit.runner.visualstudio` (though still recommended for backward compatibility)
- `Microsoft.NET.Test.Sdk` (for pure MTP scenarios)
- External console runners

---

## Migration Steps

### 1. Project File Changes

#### Package Reference Updates

Update your `.csproj` file with the following changes:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- Convert from Library to Exe -->
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Update from xunit to xunit.v3 -->
    <!-- Use xunit.v3.mtp-v2 for explicit MTP v2 support -->
    <PackageReference Include="xunit.v3.mtp-v2" Version="3.*" />

    <!-- Update runner to v3.x -->
    <PackageReference Include="xunit.runner.visualstudio" Version="3.*">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

    <!-- Remove coverlet.msbuild -->
    <!-- Add Microsoft code coverage extension -->
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="18.*" />

    <!-- Other packages remain unchanged -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.*" />
  </ItemGroup>

</Project>
```

#### Package Mappings

| v2 Package | v3 Package | Action |
|------------|------------|--------|
| `xunit` | `xunit.v3` or `xunit.v3.mtp-v2` | Update reference |
| `xunit.abstractions` | - | Remove (no longer needed) |
| `xunit.runner.visualstudio` | `xunit.runner.visualstudio` | Update to v3.x |
| `coverlet.msbuild` | `Microsoft.Testing.Extensions.CodeCoverage` | Replace |

#### Global Configuration (global.json)

Create a `global.json` file in the repository root to enable MTP:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

This is the recommended approach for .NET SDK 10+ and works automatically with xUnit v3 - no additional MSBuild properties are required when using this method.

#### Alternative: MSBuild Properties (SDK 8/9)

For .NET SDK versions 8 or 9, you must use MSBuild properties instead of `global.json`:

```xml
<PropertyGroup>
  <!-- Enable Microsoft Testing Platform for dotnet test -->
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
  <!-- Optional: Use MTP command-line experience instead of xUnit native -->
  <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
</PropertyGroup>
```

| SDK Version | Configuration Method | Required Properties |
|-------------|---------------------|---------------------|
| .NET 10+ | `global.json` | None - MTP auto-detected |
| .NET 8/9 | MSBuild properties | `TestingPlatformDotnetTestSupport` (required), `UseMicrosoftTestingPlatformRunner` (optional) |

### 2. Code Changes

Code-level changes required for XUnit v3 are documented in the official xUnit.net documentation. Refer to these resources for comprehensive guidance on adapting your test code:

- **[Migrating from xUnit.net v2 to v3](https://xunit.net/docs/getting-started/v3/migration)** - Complete migration guide covering all breaking changes, including:
  - Namespace changes (removal of `Xunit.Abstractions`)
  - `IAsyncLifetime` changes (now inherits from `IAsyncDisposable`)
  - CancellationToken integration via `TestContext.Current`
  - Theory data changes (`IEnumerable<object[]>` to `IEnumerable<ITheoryDataRow>`)
  - Removal of `async void` test support
  - Package and namespace mappings

- **[What's New in xUnit.net v3](https://xunit.net/docs/getting-started/v3/whats-new)** - New features and quality-of-life improvements, including:
  - New assertions and overloads
  - Enhanced test configuration options
  - Performance improvements
  - New extensibility points

### 3. CI/CD Pipeline Changes

#### Azure DevOps with SonarCloud/SonarQube

**Complete Azure Pipelines Configuration:**

```yaml
trigger:
  branches:
    include:
      - main

pr:
  branches:
    include:
      - main

pool:
  vmImage: ubuntu-latest

variables:
  buildConfiguration: "Release"

steps:
  # Step 1: Prepare SonarCloud Analysis
  # Update from v1 to v4 for modern scanner support
  - task: SonarCloudPrepare@4
    inputs:
      SonarCloud: "sonarcloud"
      organization: "your-organization"
      # Change from MSBuild to dotnet scanner mode
      scannerMode: "dotnet"
      projectKey: "your-project-key"
      projectVersion:
      extraProperties: |
        sonar.exclusions=**/bin/**/*,**/obj/**/*
        sonar.language=cs
        # Update coverage path to use VS Coverage XML format
        sonar.cs.vscoveragexml.reportsPaths=$(Agent.TempDirectory)/coverage.xml

  # Step 2: Install .NET SDK
  - task: UseDotNet@2
    displayName: 'Install .NET Core SDK'
    inputs:
      version: '8.x'

  # Step 3: Restore Dependencies
  - task: DotNetCoreCLI@2
    displayName: Restore
    inputs:
      command: restore
      projects: '**/*.csproj'

  # Step 4: Build Project
  - task: DotNetCoreCLI@2
    displayName: Build
    inputs:
      command: build
      projects: '**/*.csproj'
      arguments: '--configuration $(buildConfiguration) --no-restore'

  # Step 5: Run Tests with Coverage
  # Change from standard test command to custom with MTP flags
  - task: DotNetCoreCLI@2
    displayName: Test
    inputs:
      command: custom
      custom: test
      arguments: >-
        --configuration $(buildConfiguration)
        --results-directory $(Agent.TempDirectory)
        --coverage
        --coverage-output-format xml
        --coverage-output coverage.xml
        --report-xunit-trx
        --report-xunit-trx-filename test-results.trx

  # Step 6: Publish Test Results
  # New step to publish TRX test results
  - task: PublishTestResults@2
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '$(Agent.TempDirectory)/*.trx'
      mergeTestResults: true

  # Step 7: Run SonarCloud Analysis
  # Update from v1 to v4
  - task: SonarCloudAnalyze@4

  # Step 8: Publish SonarCloud Quality Gate
  # Update from v1 to v4
  - task: SonarCloudPublish@4
    inputs:
      pollingTimeoutSec: "300"
```

#### Key Pipeline Changes Explained

**1. SonarCloud Task Version Updates**

Update all SonarCloud tasks from v1 to v4:
- `SonarCloudPrepare@1` → `SonarCloudPrepare@4`
- `SonarCloudAnalyze@1` → `SonarCloudAnalyze@4`
- `SonarCloudPublish@1` → `SonarCloudPublish@4`

**2. Scanner Mode Change**

Change from MSBuild to dotnet scanner:
```yaml
# Old
scannerMode: "MSBuild"

# New
scannerMode: "dotnet"
```

**3. Coverage Format Migration**

Replace Coverlet/OpenCover with MTP coverage:

```yaml
# Old (Coverlet with OpenCover)
arguments: "--configuration $(buildConfiguration) /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=../cov"
extraProperties: |
  sonar.cs.opencover.reportsPaths=$(Build.SourcesDirectory)/cov.opencover.xml

# New (MTP with XML coverage)
arguments: >-
  --configuration $(buildConfiguration)
  --results-directory $(Agent.TempDirectory)
  --coverage
  --coverage-output-format xml
  --coverage-output coverage.xml
extraProperties: |
  sonar.cs.vscoveragexml.reportsPaths=$(Agent.TempDirectory)/coverage.xml
```

**4. Test Command Changes**

Change from standard test to custom command:

```yaml
# Old
- task: DotNetCoreCLI@2
  inputs:
    command: test
    arguments: "--configuration $(buildConfiguration)"

# New
- task: DotNetCoreCLI@2
  inputs:
    command: custom
    custom: test
    arguments: >-
      --configuration $(buildConfiguration)
      --results-directory $(Agent.TempDirectory)
      --coverage
      --coverage-output-format xml
      --coverage-output coverage.xml
      --report-xunit-trx
      --report-xunit-trx-filename test-results.trx
```

**5. TRX Report Publishing**

Add explicit test result publishing:

```yaml
- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Agent.TempDirectory)/*.trx'
    mergeTestResults: true
```

---

## Command-Line Reference

### Running Tests Locally

```bash
# Build and run with MTP runner
dotnet run --project YourTests

# Run with coverage
dotnet run --project YourTests -- --coverage --coverage-output-format xml

# Run specific test class
dotnet run --project YourTests -- --filter-class YourNamespace.YourTestClass

# Run with TRX report output
dotnet run --project YourTests -- --report-xunit-trx --report-xunit-trx-filename results.trx
```

### Using dotnet test

```bash
# Standard run (requires global.json for SDK 10+ or TestingPlatformDotnetTestSupport for SDK 8/9)
dotnet test

# With coverage
dotnet test -- --coverage --coverage-output-format xml

# With specific filter
dotnet test -- --filter-class YourNamespace.YourTestClass
```

### Common MTP Flags

| Flag | Description |
|------|-------------|
| `--coverage` | Enable code coverage collection |
| `--coverage-output-format <format>` | Format: `xml`, `cobertura`, or `coverage` |
| `--coverage-output <filename>` | Coverage output filename |
| `--report-xunit-trx` | Generate TRX report |
| `--report-xunit-trx-filename <file>` | TRX report filename |
| `--report-junit` | Generate JUnit XML report |
| `--report-xunit-html` | Generate HTML report |
| `--filter-class <name>` | Run tests in specific class |
| `--filter-method <name>` | Run specific test method |
| `--fail-skips on` | Treat skipped tests as failed |
| `--parallel <option>` | Control parallelization |

---

## Troubleshooting

### Issue: Tests Not Running with MTP

**Symptom**: `dotnet test` still uses VSTest

**Solution**: Check your configuration based on SDK version:

**For .NET SDK 10+:** Ensure `global.json` exists in repository root:
```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

**For .NET SDK 8/9:** Add MSBuild property to `.csproj`:
```xml
<PropertyGroup>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>
```

### Issue: Coverage Report Not Generated

**Symptom**: No coverage file in output

**Solution**: Verify the package reference:
```xml
<PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="18.*" />
```

Ensure the `--coverage` flag is passed and `--results-directory` is set.

### Issue: SonarCloud Not Picking Up Coverage

**Symptom**: SonarCloud shows 0% coverage

**Solution**:
1. Verify the coverage file exists in `$(Agent.TempDirectory)`
2. Check SonarCloud property matches the format: `sonar.cs.vscoveragexml.reportsPaths`
3. Check flag `--coverage-output-format` is set to `xml`
4. Ensure the coverage file is generated before SonarCloudAnalyze runs

**Note**: Sonarqube does not fully support cobertura format


### Issue: MTP v2 vs v1 Confusion

**Symptom**: Incompatibility with certain tools or features

**Solution**: Be explicit about MTP v2:
```xml
<!-- Instead of xunit.v3 -->
<PackageReference Include="xunit.v3.mtp-v2" Version="3.*" />
```

**Note**: xunit.v3 by default uses MTP v1

---

## References

- [XUnit v3 Migration Guide](https://xunit.net/docs/getting-started/v3/migration)
- [Microsoft Testing Platform Documentation](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [Code Coverage with MTP](https://xunit.net/docs/getting-started/v3/code-coverage-with-mtp)
- [Microsoft Testing Platform Extensions](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions)