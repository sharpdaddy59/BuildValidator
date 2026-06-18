# BuildValidator

A comprehensive C# .NET console application that validates C# projects using a **hybrid approach**: MSBuild compilation validation + advanced Roslyn code analysis.

## Current Session Status (2026-06-18)

**Tooling upgrade: VS 2026 / .NET 10** ✅
- ✅ Retargeted to `net10.0`; upgraded Roslyn to 5.3.0 and `Microsoft.Build.Locator` to 1.11.2
- ✅ MSBuild assemblies referenced with `ExcludeAssets="runtime"`/`PrivateAssets="all"` (Build.Locator 1.11 MSBL001 requirement)
- ✅ **`.slnx` solution support**: discovery now finds `.slnx` (the .NET 10 `dotnet new sln` default); `MSBuildWorkspace.OpenSolutionAsync` parses it natively (verified — no custom parser needed)
- ✅ When both `.sln` and `.slnx` exist for the same solution, the `.slnx` is preferred (avoids double-building)

## Prior Session Status (2025-08-19)

**Phase 1 COMPLETE**: MSBuildWorkspace compilation engine is working perfectly ✅
- ✅ MSBuild.Locator initialization resolves "language not supported" errors
- ✅ Successfully compiles projects and reports diagnostics with Roslyn
- ✅ Command line parsing, error reporting, and colored output working
- ✅ Comprehensive documentation updated for hybrid approach

**Phase 2 COMPLETE**: Advanced Roslyn Analysis Integration ✅
- ✅ Created RoslynAnalyzer component using patterns from CSharpCodeReviewerAgent
- ✅ Implemented code quality metrics (cyclomatic complexity, maintainability index, nesting depth)
- ✅ Added semantic analysis (unused imports, potential null references detection)
- ✅ Integrated analysis modes with command line options (--analysis, --metrics-only, --include-metrics)
- ✅ Enhanced BuildResultFormatter with rich analysis output and colored formatting
- ✅ Added custom thresholds for complexity and maintainability analysis

**Phase 3 PARTIALLY COMPLETE**: Advanced Features ✅
- ✅ **Complete configurable rule system** (Step 18) with JSON configuration discovery
- ✅ **Advanced null reference detection** with SEM012-SEM015 rules for comprehensive null safety
- ✅ **Granular severity override system** allowing per-rule configuration and filtering
- ✅ **Enterprise-grade output formats** (CSV, SARIF) for integration with CI/CD pipelines
- ✅ **Comprehensive style validation** with DOC, ENC, ACC, ORG rule categories
- ✅ **Solution file support** for multi-project analysis

**Test suite added** ✅ (xUnit, `tests/BuildValidator.Tests`)
- ✅ Pure-logic units (discovery + .slnx dedup, CLI parsing, metrics incl. the MI regression)
- ✅ Analyzer rule coverage (ENC001/DOC001 and rule-id integrity)
- ✅ Output formatter shape (CSV/SARIF/JSON)
- ✅ End-to-end black-box builds against fixture projects (passing, failing, .slnx)
- Enablers: `InternalsVisibleTo`; `CommandLineParser` throws `CommandLineException` instead of `Environment.Exit`

**CI + metric accuracy** ✅
- ✅ GitHub Actions workflow (`.github/workflows/ci.yml`): build+test gate, plus informational SARIF upload to code scanning
- ✅ Cyclomatic complexity now counts &&/||/??, do-while, and each case label / switch-expression arm (McCabe/VS-aligned)
- ✅ SARIF emits repo-root-relative forward-slash URIs (inline PR annotations) and the reserved `$schema` key
- ✅ Solution mode reports one result per project (`<solution> / <project>`) instead of collapsing into one

**NEXT SESSION GOAL**: Phase 3 core items complete. Candidates: within-solution parallel compilation (each project is now an independent unit, so this is unblocked)

**Working Example Tests**: 
- Basic compilation: `dotnet run -- /mnt/c/dev/BuildValidator --verbosity detailed` ✅
- Full analysis: `dotnet run -- /mnt/c/dev/BuildValidator --analysis --verbosity detailed` ✅
- Metrics only: `dotnet run -- /mnt/c/dev/BuildValidator --metrics-only` ✅
- Custom thresholds: `dotnet run -- /mnt/c/dev/BuildValidator --analysis --complexity-threshold 15 --maintainability-threshold 50` ✅

## Purpose

**Primary**: Automatically discover and validate all C# projects in a directory tree for compilation errors.

**Extended**: Provide advanced code quality analysis, metrics, and style validation using Roslyn syntax trees for deeper insights beyond basic compilation.

## Features

### Core Validation (MSBuild-based)
- **Project Discovery**: Recursively scans for `.csproj`, `.vbproj`, `.sln`, and `.slnx` files
- **MSBuild Integration**: Uses MSBuildWorkspace for accurate project compilation
- **Parallel Builds**: Builds multiple projects concurrently for performance
- **Detailed Reporting**: Comprehensive error reporting with file paths and line numbers
- **Flexible Output**: Console output with colored status indicators

### Advanced Analysis (Roslyn-based)
- **Code Quality Metrics**: Cyclomatic complexity, maintainability index, nesting depth
- **Syntax Analysis**: Class/method/property analysis, code structure metrics
- **Semantic Analysis**: Advanced null safety (SEM010-SEM015), unused imports detection, type safety analysis
- **Style Validation**: Naming conventions, code organization patterns
- **Performance Analysis**: Potential optimization opportunities
- **Security Analysis**: Basic security pattern detection (future)

## Architecture

### Hybrid Approach Design

BuildValidator uses a **dual-engine architecture** inspired by patterns from CSharpCodeReviewerAgent:

1. **MSBuildWorkspace Engine**: For reliable project compilation and dependency resolution
2. **Direct Roslyn Engine**: For advanced syntax/semantic analysis without full project loading

### Core Components

1. **ProjectDiscovery**: Finds all project files in target directory
2. **BuildEngine**: Manages MSBuildWorkspace compilation for full project validation
3. **RoslynAnalyzer**: Direct syntax tree analysis for code quality metrics (inspired by CSharpCodeReviewerAgent)
4. **BuildResultFormatter**: Enhanced reporting with compilation + analysis results
5. **CommandLineParser**: Handles command line arguments with analysis mode options

### Analysis Modes

- **Compilation Mode** (default): Fast MSBuildWorkspace validation for build errors
- **Full Analysis Mode** (`--analysis`): Compilation + advanced Roslyn code quality analysis
- **Metrics Only Mode** (`--metrics-only`): Skip compilation, analyze code quality only

### Key Dependencies

- `Microsoft.CodeAnalysis.Workspaces.MSBuild` - MSBuild workspace integration
- `Microsoft.CodeAnalysis.CSharp.Workspaces` - C# language support
- `Microsoft.CodeAnalysis.CSharp` - Direct syntax analysis
- `Microsoft.Build.Locator` - MSBuild discovery and initialization

## Usage

```bash
BuildValidator.exe <directory-path> [options]
```

### Core Options

- `--parallel <count>` - Number of parallel builds (default: CPU count)
- `--config <configuration>` - Build configuration (Debug/Release, default: Debug)
- `--verbosity <level>` - Output verbosity (minimal/normal/detailed, default: normal)
- `--warnings` - Include warnings in output (default: errors only)

### Analysis Options

- `--analysis` - Enable full analysis mode (compilation + code quality analysis)
- `--metrics-only` - Skip compilation, perform code quality analysis only
- `--complexity-threshold <n>` - Flag methods with complexity > n (default: 10)
- `--maintainability-threshold <n>` - Flag files with maintainability index < n (default: 20)
- `--include-metrics` - Include code metrics in standard compilation mode

### Output Modes

- `--format <format>` - Output format: console (default), json, markdown
- `--output <file>` - Save results to file (format auto-detected from extension)

## Output Format

### Compilation Mode (Default)
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

### Full Analysis Mode (`--analysis`)
```
Analyzing C# Projects in: /path/to/projects
===========================================
[1/3] MyApp.Core ..................... ✓ (1.8s)
  📊 Complexity: 15, Maintainability: 67, Methods: 23
  ⚠️  High complexity in ProcessData() method (complexity: 12)
  
[2/3] MyApp.Tests .................... ✗ (1.2s)
  Error CS0246: Type 'InvalidClass' not found (Tests.cs:15)
  📊 Complexity: 8, Maintainability: 45, Methods: 12
  ⚠️  Low maintainability index (< 50)
  
[3/3] MyApp.Utils .................... ✓ (0.6s)
  📊 Complexity: 4, Maintainability: 89, Methods: 8
  ✨ Excellent code quality

Results: 2 succeeded, 1 failed (3.6s total)
Code Quality: 2 excellent, 1 needs improvement
```

## Issue Categories

### Compilation Issues
- **Compilation Errors**: Syntax, type, and semantic errors
- **Missing Dependencies**: NuGet packages or project references
- **Target Framework Issues**: Incompatible framework versions
- **MSBuild Configuration**: Project file or build configuration problems

### Code Quality Issues (Analysis Mode)
- **High Complexity**: Methods exceeding cyclomatic complexity thresholds
- **Low Maintainability**: Files with poor maintainability index scores
- **Code Smells**: Unused imports, potential null references, long parameter lists
- **Style Violations**: Naming convention violations, accessibility issues
- **Performance Concerns**: Inefficient patterns, unnecessary allocations

## Implementation Plan

### Phase 1: Core Compilation Validation ✅
1. ✅ Create project structure with .NET 8
2. ✅ Set up MSBuild dependencies (MSBuildWorkspace + Locator)
3. ✅ Implement project file discovery (.csproj, .vbproj, .sln)
4. ✅ Create MSBuildWorkspace compilation engine with proper initialization
5. ✅ Add command line argument parsing (custom parser)
6. ✅ Implement error reporting and formatting with colored output
7. ✅ Update documentation for hybrid approach (CLAUDE.md + ARCHITECTURE.md)
8. ✅ Add parallel build support
9. ⏳ Add solution file support (.sln)

### Phase 2: Advanced Analysis Integration (Inspired by CSharpCodeReviewerAgent) ✅ **COMPLETE**
10. ✅ **Create RoslynAnalyzer component** - Copied patterns from CSharpCodeReviewerAgent with BuildValidator-specific enhancements
11. ✅ **Implement code quality metrics** (cyclomatic complexity, maintainability index, nesting depth, method/class counts)
12. ✅ **Add semantic analysis** (unused imports, potential null references, type information, symbol analysis)
13. ✅ **Integrate analysis modes** with command line options (--analysis, --metrics-only, --include-metrics, thresholds)
14. ✅ **Enhance BuildResultFormatter** for rich analysis output with colored formatting and detailed metrics
15. ⏳ **Add JSON/Markdown output formats** (moved to Phase 3)

### Phase 3: Advanced Features ✅ **MOSTLY COMPLETE**
15. ✅ **Add JSON/Markdown output formats** with --format and --output options (CSV, SARIF, JSON, Markdown)
16. ✅ **Implement style validation rules** (DOC001-005, ENC001-002, ACC001-002, ORG001-003)
17. ✅ **Add performance analysis patterns** (LINQ inefficiencies, memory allocations, async patterns)
18. ✅ **Create configurable rule system** (JSON config, severity overrides, rule filtering, file exclusions)
19. ✅ **Add parallel build support** (SemaphoreSlim-throttled, wall-clock timing) for better performance
20. ✅ **Add solution file support** (.sln and .slnx files with project dependencies)
21. ✅ **Add comprehensive testing** (xUnit: units, analyzer rules, formatters, e2e fixture builds)

### Phase 4: Advanced Semantic Analysis ✅ **COMPLETE**
22. ✅ **Advanced null reference detection** (SEM012-SEM015: array access, nullable types, assignments, return values)
23. ✅ **Granular semantic configuration** (per-rule severity overrides, minimum severity filtering)
24. ✅ **Legacy system integration** (unified dual-analysis approach with configurable filtering)
25. ✅ **Comprehensive documentation** (User Guide with real-world configuration examples)

## Technical Notes

### Current Implementation (Phase 1)
- **MSBuildWorkspace**: Successfully loads and compiles C# projects with full dependency resolution
- **MSBuild.Locator**: Proper MSBuild initialization resolves "language not supported" issues
- **Dependency Chain**: MSBuild.Locator → MSBuildWorkspace → Project compilation → Roslyn diagnostics
- **Proven Approach**: Based on successful patterns from CSharpCodeReviewerAgent project

### Hybrid Architecture Benefits
- **MSBuildWorkspace Engine**: Handles complex project dependencies, NuGet packages, multi-file projects
- **Direct Roslyn Engine**: Lightweight analysis for code quality without full project loading
- **Best of Both Worlds**: Reliable compilation + advanced analysis capabilities
- **Scalable Design**: Can add analysis features incrementally without affecting core compilation

### Technical Decisions
- **MSBuildWorkspace over Direct MSBuild**: Better SDK integration and dependency resolution
- **Custom CommandLineParser over System.CommandLine**: Avoid beta API compatibility issues
- **Hybrid over Single Approach**: Combine reliability of MSBuild with flexibility of direct Roslyn
- **Inspired by CSharpCodeReviewerAgent**: Proven patterns for advanced Roslyn analysis