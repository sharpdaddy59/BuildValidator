# BuildValidator User Guide

A comprehensive guide to using BuildValidator for C# project validation and code quality analysis.

## Table of Contents

- [Quick Start](#quick-start)
- [Project Discovery: Solutions vs Individual Projects](#project-discovery-solutions-vs-individual-projects)
- [Understanding Code Quality Metrics](#understanding-code-quality-metrics)
- [Analysis Modes](#analysis-modes)
- [Output Formats](#output-formats)
- [Configurable Rules](#configurable-rules)
- [Real-World Scenarios](#real-world-scenarios)
- [Command Reference](#command-reference)
- [Troubleshooting](#troubleshooting)

## Quick Start

### Installation

BuildValidator is a .NET 10 console application. To get started:

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
When BuildValidator finds solution files (`.sln` or `.slnx`), it uses solution mode for better dependency resolution. If both a `.sln` and a `.slnx` exist for the same solution, the `.slnx` is used:

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
When no solution files (`.sln`/`.slnx`) are found, BuildValidator falls back to individual project mode:

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

1. **Solution Mode** (Preferred): When solution files (`.sln` or `.slnx`) are found
   - Uses `MSBuildWorkspace.OpenSolutionAsync()` for complete dependency resolution
   - Supports both the legacy `.sln` format and the newer XML-based `.slnx` format (the .NET 10 default)
   - When both formats exist for the same solution, the `.slnx` takes precedence
   - Analyzes all projects in the solution as a unified workspace
   - Provides better cross-project reference analysis
   - Handles complex project dependencies automatically

2. **Individual Project Mode** (Fallback): When no solutions are found
   - Processes each `.csproj`/`.vbproj` file independently  
   - Works perfectly for single projects or simple codebases
   - Maintains backward compatibility with existing workflows

### When Each Mode is Used

**Solution Mode Activated When**:
- Directory contains any `.sln` or `.slnx` file(s)
- Better for enterprise codebases with multiple related projects
- Provides comprehensive dependency analysis

**Individual Project Mode Used When**:
- No `.sln`/`.slnx` files found in directory tree
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

## Configurable Rules

BuildValidator allows teams to customize style validation rules through configuration files, enabling everything from strict enterprise standards to relaxed development environments.

### Configuration File Setup

Create a `buildvalidator.json` file in your project root to customize rule behavior:

```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": true,
  "disabledRules": [
    "USG002",
    "ORG003"
  ],
  "severityOverrides": {
    "DOC001": "Error",
    "DOC002": "Error", 
    "ENC001": "Error"
  },
  "minimumSeverity": "Info",
  "treatWarningsAsErrors": false,
  "excludePatterns": [
    "**/bin/**",
    "**/obj/**",
    "**/Properties/**",
    "**/*.Designer.cs",
    "**/*.g.cs",
    "**/*.generated.cs"
  ],
  "analyzeGeneratedCode": false
}
```

### Configuration Discovery

BuildValidator automatically discovers configuration files by searching up the directory tree:

1. **Primary**: `buildvalidator.json` (preferred)
2. **Legacy**: `.buildvalidator.json` (backwards compatibility)

**Discovery behavior**:
- Starts from project directory
- Walks up parent directories until configuration found
- Uses default settings if no configuration file found
- Project-specific configs override parent directory configs

### Rule Categories

#### Documentation Rules (DOC001-DOC005)

Controls XML documentation requirements for public APIs:

```json
{
  "enableDocumentationRules": true,
  "severityOverrides": {
    "DOC001": "Error",    // Public classes must have documentation
    "DOC002": "Error",    // Public methods must have documentation  
    "DOC003": "Warning",  // Method parameters should be documented
    "DOC004": "Warning",  // Return values should be documented
    "DOC005": "Error"     // Public properties must have documentation
  }
}
```

**Rules explained**:
- **DOC001**: Public classes lacking XML documentation
- **DOC002**: Public methods lacking XML documentation  
- **DOC003**: Method parameters not documented with `<param>` tags
- **DOC004**: Return values not documented with `<returns>` tags
- **DOC005**: Public properties lacking XML documentation

**Example violations**:
```csharp
// DOC001 violation - missing class documentation
public class UserService 
{
    // DOC002 violation - missing method documentation
    public User GetUser(int id) { ... }
    
    // DOC005 violation - missing property documentation  
    public string ApiKey { get; set; }
}
```

#### Encapsulation Rules (ENC001-ENC002)

Enforces proper encapsulation and data hiding:

```json
{
  "enableEncapsulationRules": true,
  "severityOverrides": {
    "ENC001": "Error",    // No public fields allowed
    "ENC002": "Warning"   // Protected fields indicate design issues
  }
}
```

**Rules explained**:
- **ENC001**: Public fields violate encapsulation (use properties instead)
- **ENC002**: Protected fields may indicate inheritance design issues

**Example violations**:
```csharp
public class Account
{
    public decimal Balance;        // ENC001 - should be property
    protected string AccountId;   // ENC002 - consider private + protected property
}
```

#### Accessibility Rules (ACC001-ACC002)

Validates constructor and class accessibility patterns:

```json
{
  "enableAccessibilityRules": true,
  "disabledRules": ["ACC002"]  // Allow utility classes without static modifier
}
```

**Rules explained**:
- **ACC001**: Public classes should have explicit constructors when needed
- **ACC002**: Utility classes with private constructors should be static

#### Organization Rules (USG001-USG003, FIL001-FIL002, ORG001-ORG003)

Enforces code organization and file structure standards:

```json
{
  "enableOrganizationRules": true,
  "disabledRules": [
    "USG002",  // Skip blank line requirements between using groups
    "FIL002"   // Allow multiple classes per file
  ]
}
```

**Using Statement Rules (USG001-USG003)**:
- **USG001**: Using statements should be ordered (System → Microsoft → Third-party → Project)
- **USG002**: Blank lines should separate using statement groups
- **USG003**: No duplicate using statements

**File Organization Rules (FIL001-FIL002)**:
- **FIL001**: File name should match primary public class name
- **FIL002**: Avoid multiple unrelated public classes in single file

**Member Organization Rules (ORG001-ORG003)**:
- **ORG001**: Fields should come before methods
- **ORG002**: Constructors should come before other methods
- **ORG003**: Public members should come before private members

#### Semantic Analysis Rules (SEM001-SEM030)

Controls deep code analysis using compiler semantic information:

```json
{
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": true,
  "enableTypeAnalysis": true,
  "enableCodeFlowAnalysis": true,
  "semanticSeverityOverrides": {
    "SEM001": "Warning",  // Unused using statements
    "SEM010": "Warning",  // Null reference on method calls
    "SEM011": "Warning",  // Null reference on property access
    "SEM012": "Warning",  // Array/collection null access
    "SEM013": "Warning",  // Nullable types without null checks
    "SEM014": "Warning",  // Non-nullable types assigned null
    "SEM015": "Warning",  // Methods returning null without nullable annotation
    "SEM020": "Info",     // Unreachable code after return
    "SEM030": "Info"      // Unnecessary type casts
  }
}
```

**Unused Imports Rules (SEM001-SEM003)**:
- **SEM001**: Unused using statements that can be removed

**Null Safety Rules (SEM010-SEM015)**:
- **SEM010**: Potential null reference exceptions on method calls
- **SEM011**: Potential null reference exceptions on property/field access
- **SEM012**: Potential null reference on array/collection access - detects `array[index]` and `list[item]` operations on potentially null collections
- **SEM013**: Nullable types used without null checks - ensures nullable variables (`int?`, `string?`) are checked before member access
- **SEM014**: Non-nullable reference types assigned null - catches direct null assignments to non-nullable reference types
- **SEM015**: Methods that can return null but are not marked nullable - identifies methods with `return null;` but non-nullable return types

**Code Flow Rules (SEM020-SEM025)**:
- **SEM020**: Unreachable code after return/throw statements
- **SEM021**: *[Future]* Dead code (unused variables/methods)
- **SEM022**: *[Future]* Infinite loops detected
- **SEM023**: *[Future]* Missing break statements in switch cases
- **SEM024**: *[Future]* Unused method parameters
- **SEM025**: *[Future]* Variables assigned but never read

**Type Safety Rules (SEM030-SEM035)**:
- **SEM030**: Unnecessary type casts that can be removed
- **SEM031**: *[Future]* Boxing/unboxing performance issues
- **SEM032**: *[Future]* Unsafe type conversions
- **SEM033**: *[Future]* Generic type constraint violations
- **SEM034**: *[Future]* Interface segregation violations
- **SEM035**: *[Future]* Covariance/contravariance issues

**Example violations**:
```csharp
using System.Collections.Generic;  // SEM001 if not used
using System.Linq;                 // SEM001 if not used

public class UserService 
{
    public User GetUser(int id) 
    {
        var service = GetService();
        return service.FindUser(id);    // SEM010 if service could be null
    }
    
    public void ProcessUser(User user)
    {
        var name = user.Name;           // SEM011 if user could be null
        return;
        Console.WriteLine("Done");      // SEM020 - unreachable code
    }
    
    public string Convert(object value)
    {
        return (string)value;           // SEM030 if value is already string
    }
}
```

### Configuration Presets

#### Enterprise Configuration

Strict rules for enterprise development:

```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": true,
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": true,
  "enableTypeAnalysis": true,
  "enableCodeFlowAnalysis": true,
  "treatWarningsAsErrors": true,
  "minimumSeverity": "Info",
  "severityOverrides": {
    "DOC001": "Error",
    "DOC002": "Error",
    "ENC001": "Error",
    "USG001": "Warning",
    "ORG003": "Warning"
  },
  "semanticSeverityOverrides": {
    "SEM001": "Error",    // Unused imports fail build
    "SEM010": "Error",    // Null references fail build
    "SEM011": "Error",    // Property null access fails build
    "SEM012": "Error",    // Array/collection null access fails build
    "SEM013": "Error",    // Nullable types must be checked
    "SEM014": "Error",    // Null assignments not allowed
    "SEM015": "Error",    // Return null requires nullable annotation
    "SEM020": "Warning",  // Dead code as warning
    "SEM030": "Warning"   // Type safety as warning
  }
}
```

**Use case**: Mission-critical applications, public APIs, large teams

#### Relaxed Configuration

Minimal rules for rapid development:

```json
{
  "enableDocumentationRules": false,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": false,
  "enableOrganizationRules": false,
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": false,
  "enableTypeAnalysis": false,
  "enableCodeFlowAnalysis": false,
  "minimumSeverity": "Warning",
  "disabledRules": [
    "USG001", "USG002", "ORG001", "ORG002", "ORG003"
  ],
  "semanticSeverityOverrides": {
    "SEM001": "Warning"   // Only unused imports as warnings
  }
}
```

**Use case**: Prototyping, internal tools, small teams

#### Documentation-Focused Configuration

Emphasis on API documentation quality:

```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": false,
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": true,
  "enableTypeAnalysis": false,
  "enableCodeFlowAnalysis": false,
  "severityOverrides": {
    "DOC001": "Error",
    "DOC002": "Error",
    "DOC003": "Warning",
    "DOC004": "Warning",
    "DOC005": "Error"
  },
  "semanticSeverityOverrides": {
    "SEM001": "Warning",  // Clean imports for better docs
    "SEM010": "Warning",  // Null safety for API reliability
    "SEM011": "Warning"   // Property safety for API reliability
  }
}
```

**Use case**: Libraries, SDKs, public-facing APIs

### Advanced Configuration Options

#### Rule Filtering

**Explicit rule selection**:
```json
{
  "enabledRules": ["DOC001", "DOC002", "ENC001"],  // Only these style rules
  "enableSemanticRules": true,
  "enableNullReferenceDetection": false,  // Disable entire semantic category
  "disabledRules": []  // Ignored when enabledRules is specified
}
```

**Rule exclusion**:
```json
{
  "enableDocumentationRules": true,
  "enableSemanticRules": true,
  "disabledRules": ["DOC003", "DOC004", "SEM010", "SEM011"]  // Disable specific rules
}
```

**Semantic-specific filtering**:
```json
{
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,   // Enable unused imports
  "enableNullReferenceDetection": false, // Disable null reference detection
  "enableTypeAnalysis": false,           // Disable type analysis
  "enableCodeFlowAnalysis": true,        // Enable code flow analysis
  "disabledRules": ["SEM020"]            // But disable specific flow rule
}
```

#### Severity Management

**Custom severity levels**:
```json
{
  "severityOverrides": {
    "DOC001": "Error",      // Make documentation violations fail builds
    "USG001": "Info",       // Reduce using order to informational
    "ENC001": "Warning"     // Public fields as warnings not errors
  },
  "semanticSeverityOverrides": {
    "SEM001": "Error",      // Unused imports fail builds
    "SEM010": "Info",       // Null references as guidance only
    "SEM011": "Warning",    // Property access as warnings
    "SEM012": "Info",       // Array access as guidance
    "SEM013": "Info",       // Nullable checks as guidance
    "SEM014": "Warning",    // Null assignments as warnings
    "SEM015": "Warning",    // Return null as warnings
    "SEM020": "Error",      // Dead code fails builds
    "SEM030": "Info"        // Type casts as informational
  },
  "minimumSeverity": "Warning",  // Filter out Info-level issues
  "treatWarningsAsErrors": true  // Fail builds on any warnings
}
```

**Semantic vs Style severity separation**:
```json
{
  "treatWarningsAsErrors": false,
  "severityOverrides": {
    "DOC001": "Warning"     // Style rules as warnings
  },
  "semanticSeverityOverrides": {
    "SEM010": "Error",      // But semantic issues as errors
    "SEM011": "Error"
  }
}
```

#### Null Reference Filtering

**Complete null reference suppression**:
```json
{
  "enableNullReferenceDetection": false  // Disables all null reference analysis
}
```

**Granular null reference control** (recommended):
```json
{
  "enableNullReferenceDetection": true,
  "semanticSeverityOverrides": {
    "SEM010": "Info",  // Member access null refs as guidance
    "SEM011": "Info",  // Method call null refs as guidance  
    "SEM012": "Info",  // Array/collection access as guidance
    "SEM013": "Info",  // Nullable variable usage as guidance
    "SEM014": "Info",  // Null assignments as guidance
    "SEM015": "Info"   // Return null methods as guidance
  },
  "minimumSeverity": "Warning"  // Filters out Info-level issues
}
```

**Selective null reference filtering**:
```json
{
  "semanticSeverityOverrides": {
    "SEM010": "Warning",  // Keep member access warnings
    "SEM011": "Warning",  // Keep method call warnings
    "SEM012": "Info",     // Filter array access (common false positives)
    "SEM013": "Info",     // Filter nullable checks (legacy code)
    "SEM014": "Error",    // Strict on null assignments
    "SEM015": "Error"     // Strict on return null methods
  }
}
```

**Use cases**:
- **Legacy codebases**: Set all to `Info` with `minimumSeverity: "Warning"`
- **New projects**: Keep all as `Warning` or `Error` for strict null safety
- **Mixed environments**: Use selective filtering per rule type

#### File Exclusions

**Pattern-based exclusions**:
```json
{
  "excludePatterns": [
    "**/bin/**",           // Build outputs
    "**/obj/**",           // Compiler artifacts
    "**/Properties/**",    // Auto-generated properties
    "**/*.Designer.cs",    // WinForms/WPF designers
    "**/*.g.cs",          // Generated files
    "**/Migrations/**",    // Entity Framework migrations
    "**/wwwroot/**"        // Web static files
  ],
  "analyzeGeneratedCode": false  // Skip all generated code patterns
}
```

### Configuration Validation

BuildValidator validates configuration files and reports errors:

```bash
# Configuration errors are reported during analysis
dotnet run -- ./src --analysis --verbosity detailed

# Example error output:
# Warning: Invalid configuration in buildvalidator.json:
# - Rules cannot be both enabled and disabled: DOC001
# - Invalid severity 'Critical' for rule 'DOC002' 
```

**Common validation errors**:
- Conflicting enabled/disabled rules
- Invalid severity values
- Empty rule IDs or parameters
- Malformed file exclusion patterns

### Team Workflow Integration

#### Development Workflow

```bash
# Local development with team standards
dotnet run -- ./src --analysis

# Customized for code review
dotnet run -- ./feature-branch --analysis --format markdown --output review.md
```

#### CI/CD Integration

```yaml
# Azure DevOps with custom rules
- script: dotnet run --project BuildValidator -- $(Build.SourcesDirectory) --analysis --format sarif --output quality-results.sarif
  displayName: 'Code Quality Analysis'
  
# Fail build if configuration sets treatWarningsAsErrors: true
- script: exit 1
  condition: and(failed(), contains(variables['Agent.JobStatus'], 'Failed'))
  displayName: 'Quality Gate Failed'
```

#### Team Standards Evolution

**Gradual improvement approach**:

1. **Assessment phase**: Start with relaxed configuration
```json
{ "minimumSeverity": "Error", "enableOrganizationRules": false }
```

2. **Improvement phase**: Gradually increase standards
```json
{ "minimumSeverity": "Warning", "enableOrganizationRules": true }
```

3. **Maintenance phase**: Strict enforcement for new code
```json
{ "minimumSeverity": "Info", "treatWarningsAsErrors": true }
```

### Configuration Best Practices

1. **Start conservative**: Begin with relaxed rules and tighten gradually
2. **Team agreement**: Ensure all team members agree on rule choices
3. **Project context**: Adjust rules based on project type (library vs application)
4. **Legacy accommodation**: Use separate configs for legacy vs new code
5. **Regular review**: Periodically review and update rules as team matures

### Troubleshooting Configuration

#### Configuration Not Loading

```bash
# Check if configuration file is found
dotnet run -- ./src --analysis --verbosity detailed
# Look for: "Loading configuration from: /path/to/buildvalidator.json"
```

#### Rules Not Working as Expected

```bash
# Verify rule IDs and syntax
dotnet run -- ./src --analysis --verbosity detailed
# Check for configuration validation warnings
```

#### Performance Issues with Many Rules

```json
{
  "minimumSeverity": "Warning",  // Filter out low-priority issues
  "excludePatterns": [
    "**/Generated/**",          // Skip generated code
    "**/ThirdParty/**"         // Skip vendor code
  ]
}
```

## Real-World Scenarios

### Scenario 1: New Project Quality Gates

**Situation**: Establishing quality standards for a new project

**Configuration setup**:
```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": true,
  "enableSemanticRules": true,
  "enableNullReferenceDetection": true,
  "enableUnusedImportDetection": true,
  "treatWarningsAsErrors": true,
  "minimumSeverity": "Info",
  "severityOverrides": {
    "DOC001": "Error",
    "DOC002": "Error",
    "ENC001": "Error"
  },
  "semanticSeverityOverrides": {
    "SEM001": "Error",    // Unused imports fail builds
    "SEM010": "Error",    // Null references fail builds
    "SEM011": "Error"     // Property access safety required
  }
}
```

**Usage**:
```bash
# Set strict thresholds for new code
dotnet run -- ./src --analysis --complexity-threshold 8 --maintainability-threshold 70 --output quality-gate.csv
```

**Workflow**:
1. Create team-agreed buildvalidator.json configuration
2. Run analysis on every pull request
3. Reject PRs that exceed thresholds or violate style rules
4. Use CSV output for trend tracking
5. Adjust thresholds and rules based on team capability

**Success metrics**:
- No methods with complexity > 8
- All files maintain maintainability index > 70
- Zero compilation errors/warnings
- All public APIs properly documented
- No encapsulation violations
- No unused imports or potential null references
- Clean semantic analysis across all code

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

### Scenario 6: Customizing Rules for Different Project Types

**Situation**: Managing multiple projects with different quality requirements

**Library Project Configuration** (`buildvalidator.json`):
```json
{
  "enableDocumentationRules": true,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": true,
  "enableOrganizationRules": false,
  "treatWarningsAsErrors": true,
  "severityOverrides": {
    "DOC001": "Error",
    "DOC002": "Error",
    "DOC005": "Error",
    "ENC001": "Error"
  },
  "minimumSeverity": "Warning"
}
```

**Internal Tool Configuration** (`buildvalidator.json`):
```json
{
  "enableDocumentationRules": false,
  "enableEncapsulationRules": true,
  "enableAccessibilityRules": false,
  "enableOrganizationRules": false,
  "minimumSeverity": "Warning",
  "disabledRules": ["USG001", "USG002"],
  "excludePatterns": [
    "**/bin/**", "**/obj/**", "**/Temp/**"
  ]
}
```

**Prototype Project Configuration** (`buildvalidator.json`):
```json
{
  "enableDocumentationRules": false,
  "enableEncapsulationRules": false,
  "enableAccessibilityRules": false,
  "enableOrganizationRules": false,
  "minimumSeverity": "Error",
  "enabledRules": []
}
```

**Usage workflow**:
```bash
# Library - strict documentation and encapsulation
cd ./MyLibrary && dotnet run --project ../BuildValidator -- . --analysis

# Internal tool - focus on functionality over documentation
cd ./InternalTool && dotnet run --project ../BuildValidator -- . --analysis

# Prototype - only catch critical errors
cd ./Prototype && dotnet run --project ../BuildValidator -- . --analysis
```

**Benefits**:
- **Context-appropriate standards**: Each project type gets suitable rules
- **Reduced noise**: Developers see relevant issues for their context
- **Gradual improvement**: Can upgrade prototype → tool → library over time
- **Team productivity**: Rules support rather than hinder development goals

**Team workflow**:
1. Establish project type categories and corresponding rule sets
2. Create template configurations for each category
3. Place appropriate configuration in each project root
4. Document standards for each project type
5. Review and evolve rules based on project maturity

### Scenario 7: Configuring Semantic Analysis for Different Code Areas

**Situation**: Managing semantic analysis rules across legacy and new code areas

**Legacy Code Area** (`/src/Legacy/buildvalidator.json`):
```json
{
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": false,  // Too noisy in legacy code
  "enableTypeAnalysis": false,
  "enableCodeFlowAnalysis": false,
  "semanticSeverityOverrides": {
    "SEM001": "Info"      // Unused imports as guidance only
  },
  "minimumSeverity": "Warning"
}
```

**New API Code** (`/src/Api/buildvalidator.json`):
```json
{
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": true,
  "enableTypeAnalysis": true,
  "enableCodeFlowAnalysis": true,
  "treatWarningsAsErrors": true,
  "semanticSeverityOverrides": {
    "SEM001": "Error",    // Clean imports required
    "SEM010": "Error",    // Null safety critical for APIs
    "SEM011": "Error",    // Property safety critical
    "SEM020": "Warning",  // Dead code as warnings
    "SEM030": "Warning"   // Type efficiency important
  }
}
```

**Internal Tools** (`/src/Tools/buildvalidator.json`):
```json
{
  "enableSemanticRules": true,
  "enableUnusedImportDetection": true,
  "enableNullReferenceDetection": true,
  "enableTypeAnalysis": false,       // Skip for rapid development
  "enableCodeFlowAnalysis": false,
  "semanticSeverityOverrides": {
    "SEM001": "Warning",  // Clean imports encouraged
    "SEM010": "Info",     // Null refs as guidance
    "SEM011": "Info"      // Property access as guidance
  },
  "minimumSeverity": "Warning"
}
```

**Usage workflow**:
```bash
# Legacy assessment - minimal semantic noise
cd ./src/Legacy && dotnet run --project ../../BuildValidator -- . --analysis

# API validation - strict semantic requirements  
cd ./src/Api && dotnet run --project ../../BuildValidator -- . --analysis

# Tools development - balanced semantic guidance
cd ./src/Tools && dotnet run --project ../../BuildValidator -- . --analysis
```

**Benefits**:
- **Context-appropriate semantic analysis**: Each area gets suitable semantic rules
- **Gradual improvement**: Legacy code can adopt semantic rules incrementally  
- **Performance optimization**: Skip expensive analysis where not needed
- **Developer productivity**: Semantic rules support rather than hinder development

**Migration strategy**:
1. Start with legacy config for all areas to assess current state
2. Gradually enable semantic rules in new development areas
3. Increase semantic rule strictness as code quality improves
4. Eventually standardize on enterprise-level semantic analysis

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

**Cause**: BuildValidator couldn't find `.csproj`, `.vbproj`, `.sln`, or `.slnx` files in the specified directory.

**Solution**:
```bash
# Verify you're in the right directory
ls *.csproj *.vbproj *.sln *.slnx

# Check subdirectories recursively
find . -name "*.csproj" -o -name "*.vbproj" -o -name "*.sln" -o -name "*.slnx"

# Try parent directory if projects are in subdirectories
BuildValidator ../
```

**Understanding Discovery**:
- BuildValidator searches recursively for both solutions and projects
- Solution files (`.sln`/`.slnx`) take priority over individual projects
- When both a `.sln` and `.slnx` exist for the same solution, the `.slnx` is preferred
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