# BuildValidator User Guide

A comprehensive guide to using BuildValidator for C# project validation and code quality analysis.

## Table of Contents

- [Quick Start](#quick-start)
- [Project Discovery: Solutions vs Individual Projects](#project-discovery-solutions-vs-individual-projects)
- [Understanding Code Quality Metrics](#understanding-code-quality-metrics)
- [Analysis Modes](#analysis-modes)
- [Output Formats](#output-formats)
- [Real-World Scenarios](#real-world-scenarios)
- [Command Reference](#command-reference)
- [Troubleshooting](#troubleshooting)

## Quick Start

### Installation

BuildValidator is a .NET 8 console application. To get started:

1. Clone or download BuildValidator
2. Build the project: `dotnet build`
3. Run your first analysis: `dotnet run -- ./your-project-directory`

### Your First Analysis

```bash
# Basic compilation check (works with both solutions and individual projects)
dotnet run -- ./src

# Full analysis with code quality metrics
dotnet run -- ./src --analysis

# For enterprise codebases with solutions
dotnet run -- ./MyEnterpriseSolution --analysis --verbosity detailed

# Export results to Excel-friendly CSV
dotnet run -- ./src --analysis --output results.csv

# Test specific projects when no solution available
dotnet run -- ./single-project --metrics-only
```

### Expected Output

#### Solution Mode (Preferred)
When BuildValidator finds `.sln` files, it uses solution mode for better dependency resolution:

```
Building & Analyzing C# Projects in: ./src
==========================================
Found 1 solution(s) to build:
  MyApp.sln

[1/1] MyApp ................................ ✓ (2.4s)
  📊 Code Analysis Results:
    UserService.cs: Complexity: 8, Maintainability: 72, Methods: 5
    DataRepository.cs: Complexity: 12, Maintainability: 45, Methods: 8
      ⚠️  High complexity (>10)
    ⚡ Performance: 5 issues (🔴 0 high, 🟡 2 medium, 🟢 3 low)

Results: 1 succeeded, 0 failed (2.4s total)
Code Quality: 1 needs improvement
```

#### Individual Project Mode (Fallback)
When no `.sln` files are found, BuildValidator falls back to individual project mode:

```
Building & Analyzing C# Projects in: ./src
==========================================
Found 3 project(s) to build:
  MyApp.Core.csproj
  MyApp.Api.csproj  
  MyApp.Tests.csproj

[1/3] MyApp.Core ........................... ✓ (1.8s)
[2/3] MyApp.Api ............................ ✓ (2.1s)  
[3/3] MyApp.Tests .......................... ✓ (0.9s)

Results: 3 succeeded, 0 failed (4.8s total)
```

## Project Discovery: Solutions vs Individual Projects

BuildValidator uses a **solution-first discovery approach** for optimal analysis results:

### Discovery Priority

1. **Solution Mode** (Preferred): When `.sln` files are found
   - Uses `MSBuildWorkspace.OpenSolutionAsync()` for complete dependency resolution
   - Analyzes all projects in the solution as a unified workspace
   - Provides better cross-project reference analysis
   - Handles complex project dependencies automatically

2. **Individual Project Mode** (Fallback): When no solutions are found
   - Processes each `.csproj`/`.vbproj` file independently  
   - Works perfectly for single projects or simple codebases
   - Maintains backward compatibility with existing workflows

### When Each Mode is Used

**Solution Mode Activated When**:
- Directory contains any `.sln` file(s)
- Better for enterprise codebases with multiple related projects
- Provides comprehensive dependency analysis

**Individual Project Mode Used When**:
- No `.sln` files found in directory tree
- Perfect for single-project repositories
- Simple libraries or standalone applications

### Benefits of Solution Mode

- **Enhanced Analysis**: Full project context for better code quality metrics
- **Dependency Awareness**: Understands inter-project references and dependencies
- **Enterprise-Ready**: Handles complex solutions with multiple configurations
- **Unified Reporting**: Single analysis result covering entire solution

## Understanding Code Quality Metrics

### Cyclomatic Complexity

**What it measures**: The number of independent paths through your code. Higher complexity means more difficult testing and maintenance.

**Complexity Levels**:
- **1-10**: Simple, easy to understand and test
- **11-20**: Moderate complexity, consider refactoring
- **21+**: Complex, high risk, should be refactored

**Example**:
```csharp
// Complexity: 1 (simple, linear flow)
public string GetUserName(int userId)
{
    var user = GetUser(userId);
    return user.Name;
}

// Complexity: 4 (4 decision points: if, switch with 2 cases)
public string GetUserStatus(User user)
{
    if (user == null) return "Unknown";
    
    switch (user.Status)
    {
        case UserStatus.Active: return "Active";
        case UserStatus.Inactive: return "Inactive";
        default: return "Unknown";
    }
}
```

**Recommended Thresholds**:
- **New code**: Keep complexity ≤ 10
- **Legacy refactoring**: Flag methods > 15
- **Critical systems**: Keep complexity ≤ 7

### Maintainability Index

**What it measures**: A composite score (0-100) combining complexity, code size, and other factors. Higher scores indicate more maintainable code.

**Maintainability Levels**:
- **80-100**: Excellent maintainability
- **50-79**: Good maintainability  
- **20-49**: Moderate maintainability, consider improvements
- **0-19**: Low maintainability, refactoring recommended

**Factors that affect maintainability**:
- High cyclomatic complexity (lowers score)
- Long methods and large files (lowers score)
- Good documentation and clear structure (raises score)

**Recommended Thresholds**:
- **New projects**: Maintain index > 60
- **Existing codebases**: Flag files < 40
- **Legacy systems**: Target improvement to > 30

### Setting Appropriate Thresholds

Choose thresholds based on your project context:

```bash
# Strict thresholds for new, critical code
dotnet run -- ./src --analysis --complexity-threshold 7 --maintainability-threshold 60

# Moderate thresholds for existing projects
dotnet run -- ./src --analysis --complexity-threshold 12 --maintainability-threshold 40

# Lenient thresholds for legacy code assessment
dotnet run -- ./src --analysis --complexity-threshold 20 --maintainability-threshold 20
```

## Analysis Modes

### 1. Compilation Mode (Default)

**When to use**: Daily development, CI/CD pipelines, quick build verification

```bash
dotnet run -- ./src
```

**What it does**:
- Validates all projects compile successfully
- Reports compilation errors and warnings
- Fast execution (no code analysis overhead)
- Perfect for automated builds

**Output focus**: Build status, compilation diagnostics

### 2. Full Analysis Mode

**When to use**: Code reviews, quality assessments, before releases

```bash
dotnet run -- ./src --analysis
```

**What it does**:
- Full compilation validation
- Code quality metrics (complexity, maintainability)
- Semantic analysis (unused imports, potential issues)
- Comprehensive reporting

**Output focus**: Build status + detailed code quality insights

### 3. Metrics-Only Mode

**When to use**: Legacy code assessment, architecture reviews, performance analysis

```bash
dotnet run -- ./src --metrics-only
```

**What it does**:
- Skips MSBuild compilation (faster for large codebases)
- Focuses purely on code quality analysis
- Useful when build issues are known/acceptable
- Great for architectural assessment

**Output focus**: Pure code quality metrics and analysis

### 4. Include Metrics Mode

**When to use**: Regular development with light quality monitoring

```bash
dotnet run -- ./src --include-metrics
```

**What it does**:
- Standard compilation validation
- Basic code metrics without deep analysis
- Balanced performance and insight
- Good for development workflow integration

## Output Formats

### Console Output (Default)

**Best for**: Daily development, interactive use, immediate feedback

```bash
dotnet run -- ./src --analysis
```

**Advantages**:
- Real-time feedback with colored output
- Hierarchical presentation
- Interactive and immediate

### CSV Format

**Best for**: Excel analysis, reporting, trend tracking, business intelligence

```bash
dotnet run -- ./src --analysis --format csv --output results.csv
```

**Use cases**:
- **Executive reporting**: Import into Excel for dashboards
- **Trend analysis**: Track code quality over time
- **Team metrics**: Compare projects and teams
- **Compliance reporting**: Document code quality standards

**Excel workflow**:
1. Open CSV in Excel
2. Create pivot tables for summary views
3. Build charts for trend visualization
4. Share formatted reports with stakeholders

### SARIF Format

**Best for**: Enterprise Microsoft tooling, security tools, CI/CD integration

```bash
dotnet run -- ./src --analysis --format sarif --output results.sarif
```

**Integration points**:
- **Visual Studio**: Import SARIF files for in-IDE issue viewing
- **Azure DevOps**: Pipeline integration for quality gates
- **GitHub**: Advanced Security features
- **Enterprise security tools**: StandardSARIF 2.1.0 compliance

**Enterprise workflow**:
1. Generate SARIF in CI/CD pipeline
2. Import into Azure DevOps for tracking
3. Set quality gates based on issue counts
4. Integrate with security scanning tools

### JSON Format

**Best for**: API integration, custom tooling, automation

```bash
dotnet run -- ./src --analysis --format json --output results.json
```

**Use cases**:
- Custom dashboards and reporting tools
- Integration with other development tools
- Automated processing and analysis
- Data exchange between systems

### Markdown Format

**Best for**: Documentation, GitHub workflows, team communication

```bash
dotnet run -- ./src --analysis --format markdown --output results.md
```

**Use cases**:
- Code review documentation
- GitHub issue comments
- Team wikis and documentation
- Formatted reports for sharing

## Real-World Scenarios

### Scenario 1: New Project Quality Gates

**Situation**: Establishing quality standards for a new project

```bash
# Set strict thresholds for new code
dotnet run -- ./src --analysis --complexity-threshold 8 --maintainability-threshold 70 --output quality-gate.csv
```

**Workflow**:
1. Run analysis on every pull request
2. Reject PRs that exceed thresholds
3. Use CSV output for trend tracking
4. Adjust thresholds based on team capability

**Success metrics**:
- No methods with complexity > 8
- All files maintain maintainability index > 70
- Zero compilation errors/warnings

### Scenario 2: Legacy Codebase Assessment

**Situation**: Understanding technical debt in an existing system

```bash
# Assess current state with lenient thresholds
dotnet run -- ./legacy-app --metrics-only --complexity-threshold 25 --maintainability-threshold 20 --output legacy-assessment.csv
```

**Analysis approach**:
1. Start with metrics-only (faster for large codebases)
2. Use lenient thresholds to identify worst areas
3. Export to CSV for Excel-based analysis
4. Prioritize refactoring based on business impact

**Deliverables**:
- Technical debt assessment report
- Refactoring priority matrix
- Quality improvement roadmap

### Scenario 3: Enterprise Solution Analysis

**Situation**: Analyzing large enterprise codebases with multiple projects and dependencies

```bash
# Comprehensive solution analysis
dotnet run -- ./EnterpriseSolution --analysis --verbosity detailed --output enterprise-report.json

# Quick solution health check
dotnet run -- ./EnterpriseSolution --include-metrics

# Focus on performance issues in large solutions
dotnet run -- ./EnterpriseSolution --analysis --complexity-threshold 15 --output performance-audit.csv
```

**Benefits of Solution Mode**:
1. **Cross-Project Analysis**: Understands dependencies between projects
2. **Unified Reporting**: Single report covering entire solution architecture  
3. **Better Context**: More accurate code quality metrics with full project context
4. **Enterprise Scale**: Handles complex solutions with dozens of projects

**Typical Results**:
- Single solution analysis instead of multiple individual project results
- Better dependency resolution for inter-project references
- More accurate unused reference detection across projects
- Comprehensive architecture overview in output formats

### Scenario 4: Solution vs Project Mode Comparison

**Situation**: Understanding when to use solution mode vs individual project mode

**Enterprise Solution Example**:
```bash
# Solution mode - analyzes entire architecture
dotnet run -- ./MyEnterpriseSolution --analysis --verbosity detailed
# Output: Found 1 solution(s) to build: MyEnterpriseSolution.sln
# Analyzes: All 15 projects with full dependency context
```

**Individual Projects Example**:
```bash
# Individual mode - processes projects separately  
dotnet run -- ./src/standalone-projects --analysis --verbosity detailed
# Output: Found 3 project(s) to build: Core.csproj, Utils.csproj, Tests.csproj
# Analyzes: Each project independently
```

**When to Use Each**:
- **Solution Mode**: Multi-project codebases, enterprise applications, shared libraries
- **Individual Mode**: Single projects, microservices, independent libraries

### Scenario 5: CI/CD Pipeline Integration

**Situation**: Automated quality checking in build pipelines

```yaml
# Azure DevOps Pipeline Example
- task: DotNetCoreCLI@2
  displayName: 'Build Quality Analysis'
  inputs:
    command: 'run'
    arguments: '--project BuildValidator -- $(Build.SourcesDirectory) --analysis --format sarif --output $(Agent.TempDirectory)/analysis-results.sarif'

- task: PublishTestResults@2
  displayName: 'Publish Analysis Results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Agent.TempDirectory)/analysis-results.sarif'
```

**Pipeline workflow**:
1. Run BuildValidator on every build
2. Generate SARIF output for tooling integration
3. Fail build if critical thresholds exceeded
4. Publish results to Azure DevOps

### Scenario 4: Code Review Enhancement

**Situation**: Enhancing code review process with objective metrics

```bash
# Detailed analysis for code review
dotnet run -- ./feature-branch --analysis --verbosity detailed --format markdown --output code-review.md
```

**Review workflow**:
1. Run analysis on feature branches
2. Generate Markdown report for review comments
3. Focus discussion on high-complexity areas
4. Track quality improvements over time

### Scenario 5: Team Quality Monitoring

**Situation**: Regular monitoring of code quality across teams

```bash
# Weekly quality report
dotnet run -- ./src --analysis --format csv --output "weekly-quality-$(date +%Y-%m-%d).csv"
```

**Monitoring approach**:
1. Weekly automated quality scans
2. CSV export for Excel dashboard
3. Track trends and improvements
4. Identify teams/projects needing support

## Command Reference

### Core Options

```bash
BuildValidator <directory> [options]
```

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--parallel` | `-p` | Number of parallel builds | CPU count |
| `--config` | `-c` | Build configuration (Debug/Release) | Debug |
| `--verbosity` | `-v` | Output verbosity (minimal/normal/detailed) | normal |
| `--warnings` | `-w` | Include warnings in output | false |

### Analysis Options

| Option | Description | Default |
|--------|-------------|---------|
| `--analysis` | Enable full analysis mode | false |
| `--metrics-only` | Skip compilation, analyze code quality only | false |
| `--include-metrics` | Add metrics to standard compilation mode | false |
| `--complexity-threshold <n>` | Flag methods with complexity > n | 10 |
| `--maintainability-threshold <n>` | Flag files with maintainability < n | 20 |

### Output Options

| Option | Description | Default |
|--------|-------------|---------|
| `--format <format>` | Output format: console, csv, sarif, json, md | console |
| `--output <file>` | Save to file (auto-detects format from extension) | none |

### Examples

```bash
# Basic compilation check
BuildValidator ./src

# Full analysis with custom thresholds
BuildValidator ./src --analysis --complexity-threshold 15 --maintainability-threshold 50

# Export to Excel-ready CSV
BuildValidator ./src --analysis --output results.csv

# Generate SARIF for Visual Studio
BuildValidator ./src --analysis --format sarif --output results.sarif

# Legacy code assessment
BuildValidator ./legacy --metrics-only --complexity-threshold 30

# CI/CD pipeline integration
BuildValidator ./src --analysis --verbosity minimal --output pipeline-results.json
```

## Troubleshooting

### Common Issues

#### "No C# project or solution files found"

**Cause**: BuildValidator couldn't find `.csproj`, `.vbproj`, or `.sln` files in the specified directory.

**Solution**:
```bash
# Verify you're in the right directory
ls *.csproj *.vbproj *.sln

# Check subdirectories recursively
find . -name "*.csproj" -o -name "*.vbproj" -o -name "*.sln"

# Try parent directory if projects are in subdirectories
BuildValidator ../
```

**Understanding Discovery**:
- BuildValidator searches recursively for both solutions and projects
- Solution files (`.sln`) take priority over individual projects
- Individual projects are processed when no solution is found

#### "No valid C# projects or solutions found"

**Cause**: BuildValidator found files but couldn't process them (corrupted files, wrong format).

**Solution**:
```bash
# Verify solution file integrity
dotnet sln list

# Test individual project files
dotnet build ./path/to/project.csproj

# Check for file permissions or corruption
file *.sln *.csproj
```

#### "MSBuild errors during compilation"

**Cause**: Projects have dependency or configuration issues.

**Solutions**:
```bash
# Try building with .NET CLI first
dotnet build

# Check for missing NuGet packages
dotnet restore

# Use metrics-only mode to skip compilation
BuildValidator ./src --metrics-only
```

#### "High complexity/low maintainability everywhere"

**Cause**: Thresholds may be too strict for your codebase.

**Solution**:
```bash
# Start with lenient thresholds and gradually tighten
BuildValidator ./src --analysis --complexity-threshold 20 --maintainability-threshold 10

# Focus on worst cases first
BuildValidator ./src --analysis --complexity-threshold 50
```

#### "Analysis taking too long"

**Cause**: Large codebase with full analysis enabled.

**Solutions**:
```bash
# Use metrics-only for faster analysis
BuildValidator ./src --metrics-only

# Reduce verbosity
BuildValidator ./src --analysis --verbosity minimal

# Analyze specific projects instead of entire solution
BuildValidator ./src/MyApp.Core --analysis
```

### Performance Tips

1. **Use appropriate modes**:
   - Compilation-only for CI/CD
   - Metrics-only for large legacy codebases
   - Full analysis for focused code reviews

2. **Leverage parallel processing**:
   ```bash
   # Maximize CPU usage for large solutions
   BuildValidator ./src --parallel 8
   ```

3. **Export for offline analysis**:
   ```bash
   # Generate once, analyze multiple times in Excel
   BuildValidator ./src --analysis --output analysis.csv
   ```

### Getting Help

- **Command-line help**: `BuildValidator --help`
- **Verbose output**: Use `--verbosity detailed` for more information
- **Test with small projects**: Validate BuildValidator works with a simple project first

## Best Practices

### For Development Teams

1. **Establish team standards early** with agreed-upon thresholds
2. **Integrate into code review process** with Markdown reports
3. **Use CSV exports for retrospectives** and quality discussions
4. **Set up automated quality monitoring** with regular reports

### For Enterprise Environments

1. **Use SARIF format** for tool integration
2. **Establish quality gates** in CI/CD pipelines
3. **Create executive dashboards** from CSV exports
4. **Track quality trends over time** for continuous improvement

### For Legacy Projects

1. **Start with assessment** using metrics-only mode
2. **Set realistic initial thresholds** and improve gradually
3. **Focus on high-impact areas** first (critical business logic)
4. **Use trends to measure improvement** rather than absolute scores

---

## Need More Help?

BuildValidator is designed to be intuitive and helpful. If you encounter issues or need additional features:

1. Check the command-line help: `BuildValidator --help`
2. Try different verbosity levels for more information
3. Start with simple scenarios and build complexity gradually
4. Use the appropriate output format for your workflow

Happy coding! 🚀