# BuildValidator

A C# .NET console application that validates C# projects can build successfully using MSBuild APIs.

## Purpose

Automatically discover and build all C# projects in a directory tree to verify they compile without errors.

## Features

- **Project Discovery**: Recursively scans for `.csproj`, `.vbproj`, and `.sln` files
- **MSBuild Integration**: Uses Microsoft.Build APIs for accurate compilation
- **Parallel Builds**: Builds multiple projects concurrently for performance
- **Detailed Reporting**: Comprehensive error reporting with file paths and line numbers
- **Flexible Output**: Console output with colored status indicators

## Architecture

### Core Components

1. **ProjectDiscovery**: Finds all project files in target directory
2. **BuildEngine**: Manages MSBuild compilation using Microsoft.Build APIs
3. **ReportGenerator**: Formats and displays build results
4. **CommandLineParser**: Handles command line arguments

### Key Dependencies

- `Microsoft.Build` - MSBuild APIs for project compilation
- `Microsoft.CodeAnalysis.Workspaces.MSBuild` - Workspace integration
- `System.CommandLine` - Command line argument parsing

## Usage

```bash
BuildValidator.exe <directory-path> [options]
```

### Options

- `--parallel <count>` - Number of parallel builds (default: CPU count)
- `--config <configuration>` - Build configuration (Debug/Release, default: Debug)
- `--verbosity <level>` - Output verbosity (minimal/normal/detailed, default: normal)
- `--warnings` - Include warnings in output (default: errors only)

## Output Format

```
Building C# Projects in: /path/to/projects
==========================================
[1/5] MyApp.Api ...................... ✓ (2.3s)
[2/5] MyApp.Core ..................... ✓ (1.1s) 
[3/5] MyApp.Tests .................... ✗ (0.8s)
  Error CS0246: Type 'InvalidClass' not found (Tests.cs:15)
[4/5] MyApp.Web ...................... ✓ (3.2s)
[5/5] MyApp.CLI ...................... ✓ (0.9s)

Results: 4 succeeded, 1 failed (8.3s total)
```

## Error Categories

- **Compilation Errors**: Syntax, type, and semantic errors
- **Missing Dependencies**: NuGet packages or project references
- **Target Framework Issues**: Incompatible framework versions
- **MSBuild Configuration**: Project file or build configuration problems

## Implementation Plan

1. ✅ Create project structure
2. ⏳ Set up MSBuild dependencies
3. ⏳ Implement project file discovery
4. ⏳ Create MSBuild compilation engine
5. ⏳ Add command line argument parsing
6. ⏳ Implement error reporting and formatting
7. ⏳ Add parallel build support
8. ⏳ Create comprehensive testing

## Technical Notes

- Uses `MSBuildWorkspace` for loading projects
- Leverages `Microsoft.Build` APIs for accurate compilation
- Supports both individual projects and solution files
- Handles NuGet package resolution automatically
- Reports compilation diagnostics with source locations