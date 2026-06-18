# BuildValidator

[![CI](https://github.com/sharpdaddy59/BuildValidator/actions/workflows/ci.yml/badge.svg)](https://github.com/sharpdaddy59/BuildValidator/actions/workflows/ci.yml)

A C# / .NET 10 console application that validates C# projects using a **hybrid approach**: MSBuild compilation validation combined with advanced Roslyn code analysis.

It recursively discovers every C# project in a directory tree, compiles it to surface real build errors, and (optionally) layers on code-quality metrics, semantic null-safety analysis, and configurable style validation.

## Features

### Core validation (MSBuild-based)
- **Project discovery** — recursively scans for `.csproj`, `.vbproj`, `.sln`, and `.slnx` files
- **MSBuild integration** — uses `MSBuildWorkspace` for accurate compilation and dependency resolution
- **Solution-first builds** — prefers solution files (`.sln`/`.slnx`) when present, falls back to individual projects; when both formats exist for one solution, the `.slnx` wins
- **Detailed reporting** — errors and warnings with file paths and line numbers
- **Multiple output formats** — console, JSON, Markdown, CSV, and SARIF (for CI/CD integration)

### Advanced analysis (Roslyn-based)
- **Code quality metrics** — cyclomatic complexity, maintainability index, nesting depth, method/class counts
- **Semantic analysis** — advanced null-safety detection (SEM010–SEM015), unused imports, type-safety checks
- **Style validation** — documentation (DOC), encapsulation (ENC), accessibility (ACC), and organization (ORG) rules
- **Performance analysis** — LINQ inefficiencies, unnecessary allocations, async-pattern checks
- **Configurable rules** — JSON configuration with per-rule severity overrides, rule disabling, and file exclusions

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- The .NET SDK / MSBuild toolchain installed (BuildValidator locates MSBuild automatically via `Microsoft.Build.Locator`)

## Build

```bash
git clone https://github.com/sharpdaddy59/BuildValidator.git
cd BuildValidator
dotnet build -c Release
```

## Usage

```bash
BuildValidator <directory> [options]
```

Or run directly from source:

```bash
dotnet run -- <directory> [options]
```

### Core options

| Option | Description |
| --- | --- |
| `--parallel`, `-p <n>` | Number of parallel builds (default: processor count) |
| `--config`, `-c <cfg>` | Build configuration: `Debug` or `Release` (default: `Debug`) |
| `--verbosity`, `-v <level>` | Output verbosity: `minimal`, `normal`, or `detailed` (default: `normal`) |
| `--warnings`, `-w` | Include warnings in output (default: errors only) |

### Analysis options

| Option | Description |
| --- | --- |
| `--analysis`, `-a` | Full analysis mode (compilation + code-quality analysis) |
| `--metrics-only` | Skip compilation, perform code-quality analysis only |
| `--include-metrics` | Include code metrics in standard compilation mode |
| `--complexity-threshold <n>` | Flag methods with complexity > `n` (default: 10) |
| `--maintainability-threshold <n>` | Flag files with maintainability index < `n` (default: 20) |

### Output options

| Option | Description |
| --- | --- |
| `--format <format>` | Output format: `console`, `json`, `markdown`/`md`, `csv`, `sarif` (default: `console`) |
| `--output <file>` | Save results to file (format auto-detected from extension) |
| `--help`, `-h` | Show help |

### Examples

```bash
# Basic compilation validation
BuildValidator ./src

# Release build with 4 parallel workers
BuildValidator ./src --config Release --parallel 4

# Full analysis with detailed output
BuildValidator ./src --analysis --verbosity detailed

# Code-quality metrics only, stricter complexity threshold
BuildValidator ./src --metrics-only --complexity-threshold 15

# Export SARIF for CI/CD integration
BuildValidator ./src --analysis --format sarif --output results.sarif
```

## Configuration

BuildValidator discovers a `buildvalidator.json` file to control which rules run and at what severity. Example:

```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": true,
  "enableSemanticRules": true,
  "enableNullReferenceDetection": true,
  "enableUnusedImportDetection": true,
  "analyzeGeneratedCode": false,
  "minimumSeverity": "Warning",
  "treatWarningsAsErrors": false,
  "disabledRules": ["USG002"],
  "severityOverrides": {
    "DOC001": "Warning",
    "DOC002": "Warning",
    "ENC001": "Warning"
  },
  "semanticSeverityOverrides": {
    "SEM011": "Info",
    "SEM013": "Info"
  },
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/Properties/**",
    "**/*.Designer.cs",
    "**/*.g.cs",
    "**/*.generated.cs"
  ]
}
```

See the [User Guide](USER-GUIDE.md) for full configuration details and the [Architecture](ARCHITECTURE.md) document for design notes.

## Exit codes

| Code | Meaning |
| --- | --- |
| `0` | All projects compiled successfully |
| `1` | One or more projects failed, or an error occurred (e.g. no projects found, invalid arguments) |

## Project structure

| File | Responsibility |
| --- | --- |
| `Program.cs` | Entry point; MSBuild initialization |
| `BuildValidatorApp.cs` | Orchestration: discovery, build, output |
| `BuildEngine.cs` | MSBuildWorkspace compilation engine |
| `RoslynAnalyzer.cs` | Syntax-tree code-quality metrics |
| `SemanticAnalyzer.cs` | Semantic / null-safety analysis |
| `StyleValidationAnalyzer.cs` | Style rule validation (DOC/ENC/ACC/ORG) |
| `PerformanceAnalyzer.cs` | Performance pattern analysis |
| `OutputFormatters.cs` | Console, JSON, Markdown, CSV, SARIF output |
| `CommandLineParser.cs` | Argument parsing |
| `StyleConfiguration.cs` / `StyleConfigurationLoader.cs` | Configuration model and discovery |

## Continuous Integration

The [`CI` workflow](.github/workflows/ci.yml) runs on every push and pull request to `main`:

- **Build & Test** — restores, builds in Release, and runs the full xUnit suite. This gates merges.
- **Code Analysis (SARIF)** — runs BuildValidator on the repo and uploads the SARIF to GitHub code scanning, so findings appear in the **Security** tab. This job is informational and never fails the build.

## License

See [LICENSE](LICENSE).
