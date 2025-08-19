# BuildValidator Results

**Analysis Date**: 2025-08-19 10:51:46
**Total Projects**: 1
**Successful**: 1
**Failed**: 0

## BuildValidator

- **Status**: ✅ Success
- **Duration**: 3.4s
- **Path**: `/mnt/c/dev/BuildValidator/BuildValidator.sln`

### Code Quality Analysis

#### BuildEngine.cs

- **Complexity**: 29
- **Maintainability**: 0
- **Methods**: 6
- **Classes**: 1
- **Lines of Code**: 267

**Code Issues:**
- Line 49: Potential null reference: Path
- Line 60: Potential null reference: MSBuildWorkspace
- Line 66: Potential null reference: workspace
- Line 67: Potential null reference: project
- Line 84: Potential null reference: compilation
- Line 85: Potential null reference: d
- Line 85: Potential null reference: DiagnosticSeverity
- Line 128: Potential null reference: DiagnosticSeverity
- Line 132: Potential null reference: DiagnosticSeverity
- Line 136: Potential null reference: DiagnosticSeverity
- Line 140: Potential null reference: location
- Line 147: Potential null reference: location
- Line 147: Potential null reference: location
- Line 148: Potential null reference: location
- Line 148: Potential null reference: lineSpan.StartLinePosition
- Line 148: Potential null reference: lineSpan
- Line 149: Potential null reference: location
- Line 149: Potential null reference: lineSpan.StartLinePosition
- Line 149: Potential null reference: lineSpan
- Line 159: Potential null reference: Path
- Line 164: Potential null reference: MSBuildWorkspace
- Line 170: Potential null reference: workspace
- Line 176: Potential null reference: solution
- Line 186: Potential null reference: compilation
- Line 188: Potential null reference: d
- Line 188: Potential null reference: DiagnosticSeverity
- Line 247: Potential null reference: document.FilePath
- Line 252: Potential null reference: sourceText
- Line 262: Potential null reference: Console
- Line 279: Potential null reference: Path
- Line 282: Potential null reference: Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("bin") && !f.Contains("obj"))
- Line 282: Potential null reference: Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
- Line 282: Potential null reference: Directory
- Line 282: Potential null reference: SearchOption
- Line 283: Potential null reference: f
- Line 283: Potential null reference: f
- Line 297: Potential null reference: Console
- Line 297: Potential null reference: Path

**Performance Analysis:**
- Total Issues: 12
- High Severity: 0
- Medium Severity: 1
- Low Severity: 11

**Performance Issues by Category:**
- **Memory Allocation** (1 issues):
  - 🟡 Line 188: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
- **Async Performance** (11 issues):
  - 🟢 Line 54: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 66: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 67: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 92: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 170: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks

#### BuildResultFormatter.cs

- **Complexity**: 26
- **Maintainability**: 0
- **Methods**: 10
- **Classes**: 1
- **Lines of Code**: 215

**Code Issues:**
- Line 10: Potential null reference: TimeSpan
- Line 10: Potential null reference: resultsList
- Line 10: Potential null reference: r.Duration
- Line 10: Potential null reference: r
- Line 11: Potential null reference: resultsList
- Line 11: Potential null reference: r
- Line 11: Potential null reference: BuildStatus
- Line 12: Potential null reference: resultsList
- Line 12: Potential null reference: r
- Line 12: Potential null reference: BuildStatus
- Line 15: Potential null reference: resultsList
- Line 18: Potential null reference: resultsList
- Line 22: Potential null reference: Console
- Line 23: Potential null reference: ConsoleColor
- Line 23: Potential null reference: ConsoleColor
- Line 24: Potential null reference: Console
- Line 25: Potential null reference: Console
- Line 25: Potential null reference: totalDuration
- Line 26: Potential null reference: Console
- Line 33: Potential null reference: BuildStatus
- Line 34: Potential null reference: result.Duration
- Line 38: Potential null reference: Math
- Line 42: Potential null reference: Console
- Line 42: Potential null reference: BuildStatus
- Line 42: Potential null reference: ConsoleColor
- Line 42: Potential null reference: ConsoleColor
- Line 43: Potential null reference: Console
- Line 44: Potential null reference: Console
- Line 49: Potential null reference: Console
- Line 49: Potential null reference: ConsoleColor
- Line 50: Potential null reference: Console
- Line 51: Potential null reference: Console
- Line 58: Potential null reference: result.AnalysisResults
- Line 62: Potential null reference: Console
- Line 70: Potential null reference: diagnostics.OrderBy(d => d.Severity).ThenBy(d => d.FilePath)
- Line 70: Potential null reference: diagnostics.OrderBy(d => d.Severity)
- Line 70: Potential null reference: d
- Line 70: Potential null reference: d
- Line 70: Potential null reference: d
- Line 74: Potential null reference: DiagnosticSeverity
- Line 75: Potential null reference: DiagnosticSeverity
- Line 76: Potential null reference: DiagnosticSeverity
- Line 77: Potential null reference: DiagnosticSeverity
- Line 83: Potential null reference: DiagnosticSeverity
- Line 83: Potential null reference: ConsoleColor
- Line 84: Potential null reference: DiagnosticSeverity
- Line 84: Potential null reference: ConsoleColor
- Line 85: Potential null reference: DiagnosticSeverity
- Line 85: Potential null reference: ConsoleColor
- Line 86: Potential null reference: ConsoleColor
- Line 92: Potential null reference: Path
- Line 96: Potential null reference: Console
- Line 97: Potential null reference: Console
- Line 98: Potential null reference: Console
- Line 117: Potential null reference: Path
- Line 118: Potential null reference: Console
- Line 118: Potential null reference: ConsoleColor
- Line 119: Potential null reference: Console
- Line 120: Potential null reference: Console
- Line 125: Potential null reference: Console
- Line 125: Potential null reference: ConsoleColor
- Line 126: Potential null reference: Console
- Line 127: Potential null reference: Console
- Line 133: Potential null reference: Console
- Line 133: Potential null reference: ConsoleColor
- Line 134: Potential null reference: Console
- Line 135: Potential null reference: Console
- Line 141: Potential null reference: Console
- Line 141: Potential null reference: ConsoleColor
- Line 142: Potential null reference: Console
- Line 143: Potential null reference: Console
- Line 150: Potential null reference: analysis.SemanticAnalysis.UnusedUsings
- Line 150: Potential null reference: analysis.SemanticAnalysis
- Line 152: Potential null reference: Console
- Line 152: Potential null reference: ConsoleColor
- Line 153: Potential null reference: Console
- Line 154: Potential null reference: analysis.SemanticAnalysis
- Line 156: Potential null reference: Console
- Line 158: Potential null reference: Console
- Line 162: Potential null reference: analysis.SemanticAnalysis.PotentialNullReferences
- Line 162: Potential null reference: analysis.SemanticAnalysis
- Line 164: Potential null reference: Console
- Line 164: Potential null reference: ConsoleColor
- Line 165: Potential null reference: Console
- Line 166: Potential null reference: analysis.SemanticAnalysis
- Line 168: Potential null reference: Console
- Line 170: Potential null reference: Console
- Line 179: Potential null reference: Console
- Line 179: Potential null reference: ConsoleColor
- Line 180: Potential null reference: Console
- Line 180: Potential null reference: analysis.SyntaxAnalysis
- Line 180: Potential null reference: analysis.SyntaxAnalysis
- Line 180: Potential null reference: analysis.SyntaxAnalysis
- Line 181: Potential null reference: Console
- Line 181: Potential null reference: analysis.SyntaxAnalysis.ClassNames
- Line 181: Potential null reference: analysis.SyntaxAnalysis
- Line 181: Potential null reference: analysis.SyntaxAnalysis.PropertyNames
- Line 181: Potential null reference: analysis.SyntaxAnalysis
- Line 182: Potential null reference: Console
- Line 188: Potential null reference: performance.LinqPerformanceIssues
            .Concat(performance.AllocationIssues)
            .Concat(performance.AsyncPerformanceIssues)
            .Concat(performance.StringPerformanceIssues)
- Line 188: Potential null reference: performance.LinqPerformanceIssues
            .Concat(performance.AllocationIssues)
            .Concat(performance.AsyncPerformanceIssues)
- Line 188: Potential null reference: performance.LinqPerformanceIssues
            .Concat(performance.AllocationIssues)
- Line 188: Potential null reference: performance.LinqPerformanceIssues
- Line 194: Potential null reference: allIssues
- Line 197: Potential null reference: performance.Metrics
- Line 199: Potential null reference: Console
- Line 199: Potential null reference: performance.Metrics
- Line 200: Potential null reference: Console
- Line 200: Potential null reference: performance.Metrics
- Line 201: Potential null reference: performance.Metrics
- Line 202: Potential null reference: performance.Metrics
- Line 203: Potential null reference: performance.Metrics
- Line 204: Potential null reference: Console
- Line 221: Potential null reference: Console
- Line 221: Potential null reference: ConsoleColor
- Line 222: Potential null reference: Console
- Line 223: Potential null reference: Console
- Line 225: Potential null reference: i
- Line 229: Potential null reference: PerformanceSeverity
- Line 230: Potential null reference: PerformanceSeverity
- Line 231: Potential null reference: PerformanceSeverity
- Line 235: Potential null reference: Console
- Line 236: Potential null reference: Console
- Line 240: Potential null reference: Console
- Line 240: Potential null reference: ConsoleColor
- Line 241: Potential null reference: Console
- Line 244: Potential null reference: Console
- Line 252: Potential null reference: ConsoleColor
- Line 253: Potential null reference: ConsoleColor
- Line 254: Potential null reference: ConsoleColor
- Line 262: Potential null reference: PerformanceSeverity
- Line 262: Potential null reference: ConsoleColor
- Line 263: Potential null reference: PerformanceSeverity
- Line 263: Potential null reference: ConsoleColor
- Line 264: Potential null reference: PerformanceSeverity
- Line 264: Potential null reference: ConsoleColor
- Line 265: Potential null reference: ConsoleColor

**Performance Analysis:**
- Total Issues: 9
- High Severity: 0
- Medium Severity: 9
- Low Severity: 0

**Performance Issues by Category:**
- **LINQ Performance** (5 issues):
  - 🟡 Line 10: Multiple enumeration detected on 'resultsList' with Sum()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 11: Multiple enumeration detected on 'resultsList' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 12: Multiple enumeration detected on 'resultsList' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 219: Multiple enumeration detected on 'issues' with Any()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 225: Multiple enumeration detected on 'issues' with OrderByDescending()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
- **Memory Allocation** (4 issues):
  - 🟡 Line 70: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 70: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 70: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 225: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda

#### BuildValidatorApp.cs

- **Complexity**: 14
- **Maintainability**: 9
- **Methods**: 2
- **Classes**: 1
- **Lines of Code**: 87

**Code Issues:**
- Line 13: Potential null reference: Console
- Line 14: Potential null reference: Console
- Line 23: Potential null reference: Console
- Line 28: Potential null reference: discoveredFiles.Where(f => f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
- Line 28: Potential null reference: f
- Line 28: Potential null reference: StringComparison
- Line 29: Potential null reference: discoveredFiles.Where(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || 
                                                         f.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
- Line 29: Potential null reference: f
- Line 29: Potential null reference: StringComparison
- Line 30: Potential null reference: f
- Line 30: Potential null reference: StringComparison
- Line 35: Potential null reference: solutionFiles
- Line 40: Potential null reference: Console
- Line 40: Potential null reference: solutionFiles
- Line 43: Potential null reference: Console
- Line 43: Potential null reference: Path
- Line 45: Potential null reference: Console
- Line 55: Potential null reference: projectFiles
- Line 60: Potential null reference: Console
- Line 60: Potential null reference: projectFiles
- Line 63: Potential null reference: Console
- Line 63: Potential null reference: Path
- Line 65: Potential null reference: Console
- Line 72: Potential null reference: Console
- Line 77: Potential null reference: OutputFormatters
- Line 80: Potential null reference: r
- Line 80: Potential null reference: BuildStatus
- Line 85: Potential null reference: Console.Error
- Line 85: Potential null reference: Console
- Line 88: Potential null reference: Console.Error
- Line 88: Potential null reference: Console
- Line 99: Potential null reference: Directory
- Line 99: Potential null reference: SearchOption
- Line 103: Potential null reference: Directory
- Line 103: Potential null reference: SearchOption
- Line 106: Potential null reference: Directory
- Line 106: Potential null reference: SearchOption

**Performance Analysis:**
- Total Issues: 5
- High Severity: 0
- Medium Severity: 2
- Low Severity: 3

**Performance Issues by Category:**
- **LINQ Performance** (2 issues):
  - 🟡 Line 28: Multiple enumeration detected on 'discoveredFiles' with Where()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 29: Multiple enumeration detected on 'discoveredFiles' with Where()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
- **Async Performance** (3 issues):
  - 🟢 Line 51: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 68: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 77: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks

#### CodeAnalysisResult.cs

- **Complexity**: 1
- **Maintainability**: 13
- **Methods**: 0
- **Classes**: 0
- **Lines of Code**: 107

#### CommandLineParser.cs

- **Complexity**: 31
- **Maintainability**: 0
- **Methods**: 2
- **Classes**: 1
- **Lines of Code**: 264

**Code Issues:**
- Line 6: Potential null reference: Environment
- Line 26: Potential null reference: Environment
- Line 33: Potential null reference: Environment
- Line 39: Potential null reference: Environment
- Line 66: Potential null reference: Console.Error
- Line 66: Potential null reference: Console
- Line 67: Potential null reference: Environment
- Line 75: Potential null reference: StringComparison
- Line 76: Potential null reference: StringComparison
- Line 83: Potential null reference: Console.Error
- Line 83: Potential null reference: Console
- Line 84: Potential null reference: Environment
- Line 89: Potential null reference: Console.Error
- Line 89: Potential null reference: Console
- Line 90: Potential null reference: Environment
- Line 105: Potential null reference: Console.Error
- Line 105: Potential null reference: Console
- Line 106: Potential null reference: Environment
- Line 111: Potential null reference: Console.Error
- Line 111: Potential null reference: Console
- Line 112: Potential null reference: Environment
- Line 140: Potential null reference: Console.Error
- Line 140: Potential null reference: Console
- Line 141: Potential null reference: Environment
- Line 153: Potential null reference: Console.Error
- Line 153: Potential null reference: Console
- Line 154: Potential null reference: Environment
- Line 169: Potential null reference: Console.Error
- Line 169: Potential null reference: Console
- Line 170: Potential null reference: Environment
- Line 175: Potential null reference: Console.Error
- Line 175: Potential null reference: Console
- Line 176: Potential null reference: Environment
- Line 188: Potential null reference: Console.Error
- Line 188: Potential null reference: Console
- Line 189: Potential null reference: Environment
- Line 195: Potential null reference: Environment
- Line 199: Potential null reference: Console.Error
- Line 199: Potential null reference: Console
- Line 200: Potential null reference: Environment
- Line 205: Potential null reference: Directory
- Line 207: Potential null reference: Console.Error
- Line 207: Potential null reference: Console
- Line 208: Potential null reference: Environment
- Line 214: Potential null reference: Console.Error
- Line 214: Potential null reference: Console
- Line 215: Potential null reference: Environment
- Line 221: Potential null reference: Path.GetExtension(options.OutputFile)
- Line 221: Potential null reference: Path
- Line 237: Potential null reference: Console
- Line 247: Potential null reference: Console
- Line 248: Potential null reference: Console
- Line 249: Potential null reference: Console
- Line 250: Potential null reference: Console
- Line 251: Potential null reference: Console
- Line 252: Potential null reference: Console
- Line 253: Potential null reference: Console
- Line 254: Potential null reference: Console
- Line 255: Potential null reference: Console
- Line 256: Potential null reference: Console
- Line 257: Potential null reference: Console
- Line 258: Potential null reference: Console
- Line 259: Potential null reference: Console
- Line 260: Potential null reference: Console
- Line 261: Potential null reference: Console
- Line 262: Potential null reference: Console
- Line 263: Potential null reference: Console
- Line 264: Potential null reference: Console
- Line 265: Potential null reference: Console
- Line 266: Potential null reference: Console
- Line 267: Potential null reference: Console
- Line 268: Potential null reference: Console
- Line 269: Potential null reference: Console
- Line 270: Potential null reference: Console
- Line 271: Potential null reference: Console
- Line 272: Potential null reference: Console
- Line 273: Potential null reference: Console
- Line 274: Potential null reference: Console
- Line 275: Potential null reference: Console
- Line 276: Potential null reference: Console
- Line 277: Potential null reference: Console
- Line 278: Potential null reference: Console
- Line 279: Potential null reference: Console

#### OutputFormatters.cs

- **Complexity**: 37
- **Maintainability**: 0
- **Methods**: 8
- **Classes**: 1
- **Lines of Code**: 422

**Code Issues:**
- Line 9: Potential null reference: options.OutputFormat
- Line 25: Potential null reference: BuildResultFormatter
- Line 40: Potential null reference: result.Duration
- Line 45: Potential null reference: result.Duration
- Line 50: Potential null reference: r
- Line 50: Potential null reference: r.AnalysisResults
- Line 50: Potential null reference: r
- Line 55: Potential null reference: r
- Line 59: Potential null reference: Path
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: metrics
- Line 63: Potential null reference: syntax
- Line 63: Potential null reference: syntax
- Line 63: Potential null reference: syntax
- Line 63: Potential null reference: syntax
- Line 71: Potential null reference: r
- Line 75: Potential null reference: Path
- Line 78: Potential null reference: analysis.SemanticAnalysis
- Line 84: Potential null reference: analysis.SemanticAnalysis
- Line 90: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
- Line 90: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
- Line 90: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
- Line 90: Potential null reference: analysis.PerformanceAnalysis
- Line 91: Potential null reference: analysis.PerformanceAnalysis
- Line 92: Potential null reference: analysis.PerformanceAnalysis
- Line 93: Potential null reference: analysis.PerformanceAnalysis
- Line 178: Potential null reference: System.Text.Json.JsonSerializer
- Line 181: Potential null reference: System.Text.Json.JsonNamingPolicy
- Line 197: Potential null reference: diagnostic.Severity.ToString()
- Line 197: Potential null reference: diagnostic.Severity
- Line 226: Potential null reference: analysis.SemanticAnalysis
- Line 252: Potential null reference: analysis.SemanticAnalysis
- Line 278: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
- Line 278: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
- Line 278: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
- Line 278: Potential null reference: analysis.PerformanceAnalysis
- Line 279: Potential null reference: analysis.PerformanceAnalysis
- Line 280: Potential null reference: analysis.PerformanceAnalysis
- Line 281: Potential null reference: analysis.PerformanceAnalysis
- Line 287: Potential null reference: PerformanceSeverity
- Line 288: Potential null reference: PerformanceSeverity
- Line 289: Potential null reference: PerformanceSeverity
- Line 322: Potential null reference: System.Text.Json.JsonSerializer
- Line 325: Potential null reference: System.Text.Json.JsonNamingPolicy
- Line 338: Potential null reference: DateTime
- Line 339: Potential null reference: resultsList
- Line 340: Potential null reference: resultsList
- Line 340: Potential null reference: r
- Line 340: Potential null reference: BuildStatus
- Line 341: Potential null reference: resultsList
- Line 341: Potential null reference: r
- Line 341: Potential null reference: BuildStatus
- Line 348: Potential null reference: BuildStatus
- Line 349: Potential null reference: result.Duration
- Line 357: Potential null reference: result.Diagnostics
- Line 372: Potential null reference: result.AnalysisResults
- Line 380: Potential null reference: Path
- Line 383: Potential null reference: analysis.CodeMetrics
- Line 384: Potential null reference: analysis.CodeMetrics
- Line 385: Potential null reference: analysis.CodeMetrics
- Line 386: Potential null reference: analysis.CodeMetrics
- Line 387: Potential null reference: analysis.SyntaxAnalysis
- Line 390: Potential null reference: analysis.SemanticAnalysis.UnusedUsings
- Line 390: Potential null reference: analysis.SemanticAnalysis
- Line 390: Potential null reference: analysis.SemanticAnalysis.PotentialNullReferences
- Line 390: Potential null reference: analysis.SemanticAnalysis
- Line 395: Potential null reference: analysis.SemanticAnalysis
- Line 400: Potential null reference: analysis.SemanticAnalysis
- Line 407: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
                        .Concat(analysis.PerformanceAnalysis.StringPerformanceIssues)
- Line 407: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
- Line 407: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
- Line 407: Potential null reference: analysis.PerformanceAnalysis.LinqPerformanceIssues
- Line 407: Potential null reference: analysis.PerformanceAnalysis
- Line 408: Potential null reference: analysis.PerformanceAnalysis
- Line 409: Potential null reference: analysis.PerformanceAnalysis
- Line 410: Potential null reference: analysis.PerformanceAnalysis
- Line 413: Potential null reference: allPerformanceIssues
- Line 417: Potential null reference: analysis.PerformanceAnalysis.Metrics
- Line 417: Potential null reference: analysis.PerformanceAnalysis
- Line 418: Potential null reference: analysis.PerformanceAnalysis.Metrics
- Line 418: Potential null reference: analysis.PerformanceAnalysis
- Line 419: Potential null reference: analysis.PerformanceAnalysis.Metrics
- Line 419: Potential null reference: analysis.PerformanceAnalysis
- Line 420: Potential null reference: analysis.PerformanceAnalysis.Metrics
- Line 420: Potential null reference: analysis.PerformanceAnalysis
- Line 425: Potential null reference: allPerformanceIssues
- Line 425: Potential null reference: i
- Line 429: Potential null reference: group.OrderByDescending(i => i.Severity)
- Line 429: Potential null reference: i
- Line 433: Potential null reference: PerformanceSeverity
- Line 434: Potential null reference: PerformanceSeverity
- Line 435: Potential null reference: PerformanceSeverity
- Line 468: Potential null reference: DateTime.Now
- Line 468: Potential null reference: DateTime
- Line 472: Potential null reference: File
- Line 473: Potential null reference: Console

**Performance Analysis:**
- Total Issues: 26
- High Severity: 0
- Medium Severity: 17
- Low Severity: 9

**Performance Issues by Category:**
- **LINQ Performance** (9 issues):
  - 🟡 Line 50: Multiple enumeration detected on 'results' with Any()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 55: Multiple enumeration detected on 'results' with Where()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 71: Multiple enumeration detected on 'results' with Where()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 340: Multiple enumeration detected on 'resultsList' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 341: Multiple enumeration detected on 'resultsList' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
- **Memory Allocation** (8 issues):
  - 🟡 Line 55: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 71: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 425: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 429: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 425: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
- **Async Performance** (9 issues):
  - 🟢 Line 12: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 15: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 18: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 22: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 103: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks

#### PerformanceAnalyzer.cs

- **Complexity**: 42
- **Maintainability**: 0
- **Methods**: 8
- **Classes**: 1
- **Lines of Code**: 480

**Code Issues:**
- Line 33: Potential null reference: root.DescendantNodes()
- Line 38: Potential null reference: memberAccess.Name.Identifier
- Line 38: Potential null reference: memberAccess.Name
- Line 43: Potential null reference: memberAccess.Expression
- Line 49: Potential null reference: containingMethod.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv != invocation)
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv.Expression is MemberAccessExpressionSyntax ma &&
                                     ma.Expression.ToString() == expressionText &&
                                     IsLinqMethod(ma.Name.Identifier.ValueText))
- Line 49: Potential null reference: containingMethod.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv != invocation)
                        .OfType<InvocationExpressionSyntax>()
- Line 49: Potential null reference: containingMethod.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv != invocation)
- Line 49: Potential null reference: containingMethod.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
- Line 49: Potential null reference: containingMethod.DescendantNodes()
- Line 49: Potential null reference: containingMethod
- Line 53: Potential null reference: inv
- Line 54: Potential null reference: ma.Expression
- Line 55: Potential null reference: ma.Name.Identifier
- Line 55: Potential null reference: ma.Name
- Line 58: Potential null reference: otherLinqCalls
- Line 60: Potential null reference: invocation.GetLocation()
- Line 64: Potential null reference: lineSpan.StartLinePosition
- Line 64: Potential null reference: lineSpan
- Line 65: Potential null reference: lineSpan.StartLinePosition
- Line 65: Potential null reference: lineSpan
- Line 67: Potential null reference: PerformanceSeverity
- Line 80: Potential null reference: binary.OperatorToken
- Line 80: Potential null reference: SyntaxKind
- Line 81: Potential null reference: binary.OperatorToken
- Line 81: Potential null reference: SyntaxKind
- Line 83: Potential null reference: invocation.GetLocation()
- Line 87: Potential null reference: lineSpan.StartLinePosition
- Line 87: Potential null reference: lineSpan
- Line 88: Potential null reference: lineSpan.StartLinePosition
- Line 88: Potential null reference: lineSpan
- Line 90: Potential null reference: PerformanceSeverity
- Line 101: Potential null reference: whereMember.Name.Identifier
- Line 101: Potential null reference: whereMember.Name
- Line 103: Potential null reference: invocation.GetLocation()
- Line 107: Potential null reference: lineSpan.StartLinePosition
- Line 107: Potential null reference: lineSpan
- Line 108: Potential null reference: lineSpan.StartLinePosition
- Line 108: Potential null reference: lineSpan
- Line 110: Potential null reference: PerformanceSeverity
- Line 120: Potential null reference: toListMember.Name.Identifier
- Line 120: Potential null reference: toListMember.Name
- Line 122: Potential null reference: invocation.GetLocation()
- Line 126: Potential null reference: lineSpan.StartLinePosition
- Line 126: Potential null reference: lineSpan
- Line 127: Potential null reference: lineSpan.StartLinePosition
- Line 127: Potential null reference: lineSpan
- Line 129: Potential null reference: PerformanceSeverity
- Line 139: Potential null reference: whereFirstMember.Name.Identifier
- Line 139: Potential null reference: whereFirstMember.Name
- Line 141: Potential null reference: invocation.GetLocation()
- Line 145: Potential null reference: lineSpan.StartLinePosition
- Line 145: Potential null reference: lineSpan
- Line 146: Potential null reference: lineSpan.StartLinePosition
- Line 146: Potential null reference: lineSpan
- Line 148: Potential null reference: PerformanceSeverity
- Line 158: Potential null reference: whereDefaultMember.Name.Identifier
- Line 158: Potential null reference: whereDefaultMember.Name
- Line 160: Potential null reference: invocation.GetLocation()
- Line 164: Potential null reference: lineSpan.StartLinePosition
- Line 164: Potential null reference: lineSpan
- Line 165: Potential null reference: lineSpan.StartLinePosition
- Line 165: Potential null reference: lineSpan
- Line 167: Potential null reference: PerformanceSeverity
- Line 177: Potential null reference: toListAnyMember.Name.Identifier
- Line 177: Potential null reference: toListAnyMember.Name
- Line 179: Potential null reference: invocation.GetLocation()
- Line 183: Potential null reference: lineSpan.StartLinePosition
- Line 183: Potential null reference: lineSpan
- Line 184: Potential null reference: lineSpan.StartLinePosition
- Line 184: Potential null reference: lineSpan
- Line 186: Potential null reference: PerformanceSeverity
- Line 196: Potential null reference: whereAnyMember.Name.Identifier
- Line 196: Potential null reference: whereAnyMember.Name
- Line 198: Potential null reference: invocation.GetLocation()
- Line 202: Potential null reference: lineSpan.StartLinePosition
- Line 202: Potential null reference: lineSpan
- Line 203: Potential null reference: lineSpan.StartLinePosition
- Line 203: Potential null reference: lineSpan
- Line 205: Potential null reference: PerformanceSeverity
- Line 215: Potential null reference: selectMember.Name.Identifier
- Line 215: Potential null reference: selectMember.Name
- Line 220: Potential null reference: parentMember.Name.Identifier
- Line 220: Potential null reference: parentMember.Name
- Line 223: Potential null reference: invocation.GetLocation()
- Line 227: Potential null reference: lineSpan.StartLinePosition
- Line 227: Potential null reference: lineSpan
- Line 228: Potential null reference: lineSpan.StartLinePosition
- Line 228: Potential null reference: lineSpan
- Line 230: Potential null reference: PerformanceSeverity
- Line 240: Potential null reference: invocation.ArgumentList
- Line 241: Potential null reference: arguments
- Line 243: Potential null reference: arguments[0]
- Line 247: Potential null reference: identifier.Identifier
- Line 247: Potential null reference: lambda.Parameter.Identifier
- Line 247: Potential null reference: lambda.Parameter
- Line 249: Potential null reference: invocation.GetLocation()
- Line 253: Potential null reference: lineSpan.StartLinePosition
- Line 253: Potential null reference: lineSpan
- Line 254: Potential null reference: lineSpan.StartLinePosition
- Line 254: Potential null reference: lineSpan
- Line 256: Potential null reference: PerformanceSeverity
- Line 273: Potential null reference: root.DescendantNodes()
- Line 276: Potential null reference: typeInfo
- Line 278: Potential null reference: typeInfo.Type
- Line 278: Potential null reference: typeInfo
- Line 279: Potential null reference: typeName
- Line 280: Potential null reference: typeName
- Line 282: Potential null reference: foreachStatement.GetLocation()
- Line 286: Potential null reference: lineSpan.StartLinePosition
- Line 286: Potential null reference: lineSpan
- Line 287: Potential null reference: lineSpan.StartLinePosition
- Line 287: Potential null reference: lineSpan
- Line 289: Potential null reference: PerformanceSeverity
- Line 298: Potential null reference: root.DescendantNodes()
- Line 300: Potential null reference: invocation.ArgumentList.Arguments
- Line 300: Potential null reference: invocation.ArgumentList
- Line 302: Potential null reference: invocation.ArgumentList.Arguments
- Line 302: Potential null reference: invocation.ArgumentList
- Line 303: Potential null reference: lastArg
- Line 305: Potential null reference: arrayCreation.Initializer.Expressions
- Line 305: Potential null reference: arrayCreation.Initializer
- Line 307: Potential null reference: invocation.GetLocation()
- Line 311: Potential null reference: lineSpan.StartLinePosition
- Line 311: Potential null reference: lineSpan
- Line 312: Potential null reference: lineSpan.StartLinePosition
- Line 312: Potential null reference: lineSpan
- Line 314: Potential null reference: PerformanceSeverity
- Line 323: Potential null reference: root.DescendantNodes().OfType<ForStatementSyntax>().Concat<StatementSyntax>(
                     root.DescendantNodes().OfType<ForEachStatementSyntax>())
- Line 323: Potential null reference: root.DescendantNodes().OfType<ForStatementSyntax>()
- Line 323: Potential null reference: root.DescendantNodes()
- Line 324: Potential null reference: root.DescendantNodes()
- Line 325: Potential null reference: root.DescendantNodes()
- Line 327: Potential null reference: forStatement.DescendantNodes()
- Line 330: Potential null reference: lambda.GetLocation()
- Line 334: Potential null reference: lineSpan.StartLinePosition
- Line 334: Potential null reference: lineSpan
- Line 335: Potential null reference: lineSpan.StartLinePosition
- Line 335: Potential null reference: lineSpan
- Line 337: Potential null reference: PerformanceSeverity
- Line 352: Potential null reference: root.DescendantNodes()
- Line 354: Potential null reference: method.Modifiers
- Line 354: Potential null reference: m
- Line 354: Potential null reference: SyntaxKind
- Line 355: Potential null reference: method.ReturnType
- Line 358: Potential null reference: method.ParameterList.Parameters
- Line 358: Potential null reference: method.ParameterList
- Line 359: Potential null reference: method.ParameterList.Parameters[0]
- Line 359: Potential null reference: method.ParameterList
- Line 359: Potential null reference: .ToString()
- Line 363: Potential null reference: method.GetLocation()
- Line 367: Potential null reference: lineSpan.StartLinePosition
- Line 367: Potential null reference: lineSpan
- Line 368: Potential null reference: lineSpan.StartLinePosition
- Line 368: Potential null reference: lineSpan
- Line 370: Potential null reference: PerformanceSeverity
- Line 372: Potential null reference: method.Identifier
- Line 379: Potential null reference: root.DescendantNodes()
- Line 381: Potential null reference: memberAccess.Name.Identifier
- Line 381: Potential null reference: memberAccess.Name
- Line 385: Potential null reference: typeInfo
- Line 386: Potential null reference: typeInfo.Type.ToDisplayString()
- Line 386: Potential null reference: typeInfo.Type
- Line 386: Potential null reference: typeInfo
- Line 387: Potential null reference: typeInfo.Type.AllInterfaces
- Line 387: Potential null reference: typeInfo.Type
- Line 387: Potential null reference: typeInfo
- Line 387: Potential null reference: i.ToDisplayString()
- Line 387: Potential null reference: i
- Line 389: Potential null reference: memberAccess.GetLocation()
- Line 393: Potential null reference: lineSpan.StartLinePosition
- Line 393: Potential null reference: lineSpan
- Line 394: Potential null reference: lineSpan.StartLinePosition
- Line 394: Potential null reference: lineSpan
- Line 396: Potential null reference: PerformanceSeverity
- Line 405: Potential null reference: root.DescendantNodes()
- Line 412: Potential null reference: memberAccess.Name.Identifier
- Line 412: Potential null reference: memberAccess.Name
- Line 422: Potential null reference: containingMethod.Identifier
- Line 422: Potential null reference: containingMethod
- Line 425: Potential null reference: awaitExpression.GetLocation()
- Line 429: Potential null reference: lineSpan.StartLinePosition
- Line 429: Potential null reference: lineSpan
- Line 430: Potential null reference: lineSpan.StartLinePosition
- Line 430: Potential null reference: lineSpan
- Line 432: Potential null reference: PerformanceSeverity
- Line 448: Potential null reference: root.DescendantNodes().OfType<ForStatementSyntax>().Concat<StatementSyntax>(
                     root.DescendantNodes().OfType<ForEachStatementSyntax>())
- Line 448: Potential null reference: root.DescendantNodes().OfType<ForStatementSyntax>()
- Line 448: Potential null reference: root.DescendantNodes()
- Line 449: Potential null reference: root.DescendantNodes()
- Line 450: Potential null reference: root.DescendantNodes()
- Line 452: Potential null reference: loopStatement.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
- Line 452: Potential null reference: loopStatement.DescendantNodes()
- Line 454: Potential null reference: a.OperatorToken
- Line 454: Potential null reference: a
- Line 454: Potential null reference: SyntaxKind
- Line 455: Potential null reference: a
- Line 455: Potential null reference: SyntaxKind
- Line 456: Potential null reference: a
- Line 457: Potential null reference: binary.OperatorToken
- Line 457: Potential null reference: SyntaxKind
- Line 462: Potential null reference: typeInfo
- Line 462: Potential null reference: typeInfo.Type
- Line 462: Potential null reference: typeInfo
- Line 462: Potential null reference: SpecialType
- Line 464: Potential null reference: concat.GetLocation()
- Line 468: Potential null reference: lineSpan.StartLinePosition
- Line 468: Potential null reference: lineSpan
- Line 469: Potential null reference: lineSpan.StartLinePosition
- Line 469: Potential null reference: lineSpan
- Line 471: Potential null reference: PerformanceSeverity
- Line 480: Potential null reference: root.DescendantNodes()
- Line 483: Potential null reference: memberAccess.Expression
- Line 484: Potential null reference: memberAccess.Name.Identifier
- Line 484: Potential null reference: memberAccess.Name
- Line 486: Potential null reference: invocation.GetLocation()
- Line 490: Potential null reference: lineSpan.StartLinePosition
- Line 490: Potential null reference: lineSpan
- Line 491: Potential null reference: lineSpan.StartLinePosition
- Line 491: Potential null reference: lineSpan
- Line 493: Potential null reference: PerformanceSeverity
- Line 510: Potential null reference: linqIssues.Concat(allocationIssues).Concat(asyncIssues).Concat(stringIssues)
- Line 510: Potential null reference: linqIssues.Concat(allocationIssues).Concat(asyncIssues)
- Line 510: Potential null reference: linqIssues.Concat(allocationIssues)
- Line 514: Potential null reference: allIssues
- Line 515: Potential null reference: allIssues
- Line 515: Potential null reference: i
- Line 515: Potential null reference: PerformanceSeverity
- Line 516: Potential null reference: allIssues
- Line 516: Potential null reference: i
- Line 516: Potential null reference: PerformanceSeverity
- Line 517: Potential null reference: allIssues
- Line 517: Potential null reference: i
- Line 517: Potential null reference: PerformanceSeverity
- Line 518: Potential null reference: root.DescendantNodes().OfType<InvocationExpressionSyntax>()
- Line 518: Potential null reference: root.DescendantNodes()
- Line 519: Potential null reference: inv
- Line 519: Potential null reference: ma.Name.Identifier
- Line 519: Potential null reference: ma.Name
- Line 520: Potential null reference: root.DescendantNodes().OfType<MethodDeclarationSyntax>()
- Line 520: Potential null reference: root.DescendantNodes()
- Line 521: Potential null reference: m.Modifiers
- Line 521: Potential null reference: m
- Line 521: Potential null reference: mod
- Line 521: Potential null reference: SyntaxKind
- Line 522: Potential null reference: root.DescendantNodes().OfType<BinaryExpressionSyntax>()
- Line 522: Potential null reference: root.DescendantNodes()
- Line 523: Potential null reference: b.OperatorToken
- Line 523: Potential null reference: b
- Line 523: Potential null reference: SyntaxKind
- Line 543: Potential null reference: method.ParameterList.Parameters
- Line 543: Potential null reference: method.ParameterList
- Line 544: Potential null reference: method.ParameterList.Parameters[0]
- Line 544: Potential null reference: method.ParameterList
- Line 544: Potential null reference: .ToString()
- Line 545: Potential null reference: method.ParameterList.Parameters[1]
- Line 545: Potential null reference: method.ParameterList
- Line 545: Potential null reference: .ToString()

**Performance Analysis:**
- Total Issues: 8
- High Severity: 0
- Medium Severity: 8
- Low Severity: 0

**Performance Issues by Category:**
- **LINQ Performance** (3 issues):
  - 🟡 Line 515: Multiple enumeration detected on 'allIssues' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 516: Multiple enumeration detected on 'allIssues' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 517: Multiple enumeration detected on 'allIssues' with Count()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
- **Memory Allocation** (5 issues):
  - 🟡 Line 51: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 53: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 354: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 387: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
  - 🟡 Line 454: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda

#### Program.cs

- **Complexity**: 3
- **Maintainability**: 52
- **Methods**: 0
- **Classes**: 0
- **Lines of Code**: 16

**Code Issues:**
- Line 5: Potential null reference: MSBuildLocator
- Line 7: Potential null reference: MSBuildLocator
- Line 12: Potential null reference: CommandLineParser
- Line 13: Potential null reference: BuildValidatorApp
- Line 17: Potential null reference: Console.Error
- Line 17: Potential null reference: Console

#### RoslynAnalyzer.cs

- **Complexity**: 15
- **Maintainability**: 0
- **Methods**: 14
- **Classes**: 1
- **Lines of Code**: 269

**Code Issues:**
- Line 11: Potential null reference: CSharpSyntaxTree
- Line 14: Potential null reference: syntaxTree
- Line 20: Potential null reference: PerformanceAnalyzer
- Line 36: Potential null reference: File
- Line 44: Potential null reference: MetadataReference
- Line 45: Potential null reference: MetadataReference
- Line 46: Potential null reference: MetadataReference
- Line 47: Potential null reference: MetadataReference
- Line 50: Potential null reference: CSharpCompilation
- Line 54: Potential null reference: OutputKind
- Line 65: Potential null reference: root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText)
- Line 65: Potential null reference: root.DescendantNodes().OfType<ClassDeclarationSyntax>()
- Line 65: Potential null reference: root.DescendantNodes()
- Line 66: Potential null reference: c.Identifier
- Line 66: Potential null reference: c
- Line 68: Potential null reference: root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Select(m => m.Identifier.ValueText)
- Line 68: Potential null reference: root.DescendantNodes().OfType<MethodDeclarationSyntax>()
- Line 68: Potential null reference: root.DescendantNodes()
- Line 69: Potential null reference: m.Identifier
- Line 69: Potential null reference: m
- Line 71: Potential null reference: root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText)
- Line 71: Potential null reference: root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
- Line 71: Potential null reference: root.DescendantNodes()
- Line 72: Potential null reference: p.Identifier
- Line 72: Potential null reference: p
- Line 74: Potential null reference: root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
            .Select(n => n.Name.ToString())
- Line 74: Potential null reference: root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
- Line 74: Potential null reference: root.DescendantNodes()
- Line 75: Potential null reference: n.Name
- Line 75: Potential null reference: n
- Line 77: Potential null reference: root.DescendantNodes().OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? "")
- Line 77: Potential null reference: root.DescendantNodes().OfType<UsingDirectiveSyntax>()
- Line 77: Potential null reference: root.DescendantNodes()
- Line 78: Potential null reference: u
- Line 99: Potential null reference: token.LeadingTrivia
- Line 100: Potential null reference: t
- Line 100: Potential null reference: SyntaxKind
- Line 101: Potential null reference: t
- Line 101: Potential null reference: SyntaxKind
- Line 102: Potential null reference: t
- Line 102: Potential null reference: SyntaxKind
- Line 103: Potential null reference: t
- Line 103: Potential null reference: SyntaxKind
- Line 113: Potential null reference: root.DescendantNodes()
- Line 120: Potential null reference: symbol
- Line 121: Potential null reference: symbol
- Line 122: Potential null reference: symbol.TypeKind
- Line 122: Potential null reference: symbol
- Line 123: Potential null reference: symbol
- Line 123: Potential null reference: Accessibility
- Line 124: Potential null reference: symbol
- Line 125: Potential null reference: symbol
- Line 130: Potential null reference: root.DescendantNodes()
- Line 137: Potential null reference: symbol
- Line 139: Potential null reference: symbol.ReturnType
- Line 139: Potential null reference: symbol
- Line 160: Potential null reference: root.DescendantNodes()
- Line 168: Potential null reference: namespaceName.Split('.')
- Line 168: Potential null reference: namespaceName
- Line 170: Potential null reference: sourceText
- Line 171: Potential null reference: sourceText
- Line 171: Potential null reference: namespaceShortName
- Line 173: Potential null reference: usingDirective.GetLocation()
- Line 177: Potential null reference: lineSpan.StartLinePosition
- Line 177: Potential null reference: lineSpan
- Line 178: Potential null reference: lineSpan.StartLinePosition
- Line 178: Potential null reference: lineSpan
- Line 179: Potential null reference: lineSpan
- Line 192: Potential null reference: root.DescendantNodes()
- Line 195: Potential null reference: typeInfo
- Line 197: Potential null reference: typeInfo.Type
- Line 197: Potential null reference: typeInfo
- Line 198: Potential null reference: typeName
- Line 198: Potential null reference: typeName
- Line 200: Potential null reference: memberAccess.GetLocation()
- Line 204: Potential null reference: lineSpan.StartLinePosition
- Line 204: Potential null reference: lineSpan
- Line 205: Potential null reference: lineSpan.StartLinePosition
- Line 205: Potential null reference: lineSpan
- Line 206: Potential null reference: lineSpan
- Line 219: Potential null reference: root.DescendantNodes().OfType<MethodDeclarationSyntax>()
- Line 219: Potential null reference: root.DescendantNodes()
- Line 220: Potential null reference: root.DescendantNodes().OfType<ClassDeclarationSyntax>()
- Line 220: Potential null reference: root.DescendantNodes()
- Line 221: Potential null reference: root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
- Line 221: Potential null reference: root.DescendantNodes()
- Line 222: Potential null reference: root.ToString()
- Line 239: Potential null reference: root.DescendantNodes()
- Line 240: Potential null reference: node
- Line 240: Potential null reference: SyntaxKind
- Line 241: Potential null reference: node
- Line 241: Potential null reference: SyntaxKind
- Line 242: Potential null reference: node
- Line 242: Potential null reference: SyntaxKind
- Line 243: Potential null reference: node
- Line 243: Potential null reference: SyntaxKind
- Line 244: Potential null reference: node
- Line 244: Potential null reference: SyntaxKind
- Line 245: Potential null reference: node
- Line 245: Potential null reference: SyntaxKind
- Line 246: Potential null reference: node
- Line 246: Potential null reference: SyntaxKind
- Line 248: Potential null reference: complexityNodes
- Line 264: Potential null reference: SyntaxKind
- Line 265: Potential null reference: SyntaxKind
- Line 266: Potential null reference: SyntaxKind
- Line 267: Potential null reference: SyntaxKind
- Line 268: Potential null reference: SyntaxKind
- Line 269: Potential null reference: SyntaxKind
- Line 275: Potential null reference: Math
- Line 283: Potential null reference: Math
- Line 284: Potential null reference: Math
- Line 286: Potential null reference: Math
- Line 286: Potential null reference: Math
- Line 286: Potential null reference: Math
- Line 288: Potential null reference: Math
- Line 288: Potential null reference: Math
- Line 298: Potential null reference: diagnostic.Location
- Line 303: Potential null reference: diagnostic.Descriptor.Title
- Line 303: Potential null reference: diagnostic.Descriptor
- Line 306: Potential null reference: location.StartLinePosition
- Line 306: Potential null reference: location
- Line 307: Potential null reference: location.StartLinePosition
- Line 307: Potential null reference: location
- Line 308: Potential null reference: diagnostic.Descriptor

**Performance Analysis:**
- Total Issues: 8
- High Severity: 0
- Medium Severity: 5
- Low Severity: 3

**Performance Issues by Category:**
- **LINQ Performance** (4 issues):
  - 🟡 Line 284: Multiple enumeration detected on 'Math' with Max()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 286: Multiple enumeration detected on 'Math' with Max()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 288: Multiple enumeration detected on 'Math' with Max()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
  - 🟡 Line 288: Multiple enumeration detected on 'Math' with Min()
    - 💡 Consider calling .ToList() or .ToArray() once and reusing the result
- **Memory Allocation** (1 issues):
  - 🟡 Line 99: Lambda expression in loop may cause closure allocation
    - 💡 Consider moving lambda outside loop or using static lambda
- **Async Performance** (3 issues):
  - 🟢 Line 14: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 36: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks
  - 🟢 Line 37: Missing ConfigureAwait(false) in library code
    - 💡 Add .ConfigureAwait(false) in library code to avoid deadlocks

#### BuildValidator.GlobalUsings.g.cs

- **Complexity**: 1
- **Maintainability**: 59
- **Methods**: 0
- **Classes**: 0
- **Lines of Code**: 7

#### .NETCoreApp,Version=v8.0.AssemblyAttributes.cs

- **Complexity**: 1
- **Maintainability**: 67
- **Methods**: 0
- **Classes**: 0
- **Lines of Code**: 3

#### BuildValidator.AssemblyInfo.cs

- **Complexity**: 1
- **Maintainability**: 45
- **Methods**: 0
- **Classes**: 0
- **Lines of Code**: 9


