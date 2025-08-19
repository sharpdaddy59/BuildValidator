using Microsoft.CodeAnalysis;

namespace BuildValidator;

public static class BuildResultFormatter
{
    public static void DisplayResults(IEnumerable<BuildResult> results, CommandLineOptions options)
    {
        var resultsList = results.ToList();
        var totalDuration = TimeSpan.FromMilliseconds(resultsList.Sum(r => r.Duration.TotalMilliseconds));
        var successCount = resultsList.Count(r => r.Status == BuildStatus.Success);
        var failureCount = resultsList.Count(r => r.Status == BuildStatus.Failed);

        // Display individual results
        for (int i = 0; i < resultsList.Count; i++)
        {
            var result = resultsList[i];
            DisplayProjectResult(result, i + 1, resultsList.Count, options);
        }

        // Display summary
        Console.WriteLine();
        var summaryColor = failureCount > 0 ? ConsoleColor.Red : ConsoleColor.Green;
        Console.ForegroundColor = summaryColor;
        Console.WriteLine($"Results: {successCount} succeeded, {failureCount} failed ({totalDuration.TotalSeconds:F1}s total)");
        Console.ResetColor();
    }

    private static void DisplayProjectResult(BuildResult result, int index, int total, CommandLineOptions options)
    {
        // Format: [1/5] MyApp.Api ...................... ✓ (2.3s)
        var projectDisplay = $"[{index}/{total}] {result.ProjectName}";
        var statusSymbol = result.Status == BuildStatus.Success ? "✓" : "✗";
        var duration = $"({result.Duration.TotalSeconds:F1}s)";
        
        // Calculate padding for alignment
        var maxWidth = 50; // Adjust as needed
        var paddingLength = Math.Max(1, maxWidth - projectDisplay.Length);
        var padding = new string('.', paddingLength);

        // Color based on status
        Console.ForegroundColor = result.Status == BuildStatus.Success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{projectDisplay} {padding} {statusSymbol} {duration}");
        Console.ResetColor();

        // Show error message if present
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Error: {result.ErrorMessage}");
            Console.ResetColor();
        }

        // Show diagnostics
        DisplayDiagnostics(result.Diagnostics, options);

        // Show analysis results if available
        if (result.AnalysisResults != null && result.AnalysisResults.Any())
        {
            if (options.Verbosity != "minimal")
            {
                Console.WriteLine("  📊 Code Analysis Results:");
            }
            DisplayAnalysisResults(result.AnalysisResults, options);
        }
    }

    private static void DisplayDiagnostics(IEnumerable<BuildDiagnostic> diagnostics, CommandLineOptions options)
    {
        foreach (var diagnostic in diagnostics.OrderBy(d => d.Severity).ThenBy(d => d.FilePath).ThenBy(d => d.LineNumber))
        {
            var severity = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => "Error",
                DiagnosticSeverity.Warning => "Warning",
                DiagnosticSeverity.Info => "Info",
                DiagnosticSeverity.Hidden => "Hidden",
                _ => "Unknown"
            };

            var color = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => ConsoleColor.Red,
                DiagnosticSeverity.Warning => ConsoleColor.Yellow,
                DiagnosticSeverity.Info => ConsoleColor.Cyan,
                _ => ConsoleColor.Gray
            };

            var location = "";
            if (!string.IsNullOrEmpty(diagnostic.FilePath) && diagnostic.LineNumber > 0)
            {
                var fileName = Path.GetFileName(diagnostic.FilePath);
                location = $" ({fileName}:{diagnostic.LineNumber})";
            }

            Console.ForegroundColor = color;
            Console.WriteLine($"  {severity} {diagnostic.Id}: {diagnostic.Message}{location}");
            Console.ResetColor();
        }
    }

    private static void DisplayAnalysisResults(IEnumerable<CodeAnalysisResult> analysisResults, CommandLineOptions options)
    {
        foreach (var analysis in analysisResults)
        {
            DisplayCodeMetrics(analysis.CodeMetrics, analysis.FilePath, options);
            
            if (options.Verbosity == "detailed")
            {
                DisplayDetailedAnalysis(analysis, options);
            }
        }
    }

    private static void DisplayCodeMetrics(CodeMetrics metrics, string filePath, CommandLineOptions options)
    {
        var fileName = Path.GetFileName(filePath);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    {fileName}: Complexity: {metrics.CyclomaticComplexity}, Maintainability: {metrics.MaintainabilityIndex:F0}, Methods: {metrics.MethodCount}");
        Console.ResetColor();

        // Flag high complexity
        if (metrics.CyclomaticComplexity > options.ComplexityThreshold)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"      ⚠️  High complexity (>{options.ComplexityThreshold})");
            Console.ResetColor();
        }

        // Flag low maintainability
        if (metrics.MaintainabilityIndex < options.MaintainabilityThreshold)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"      ⚠️  Low maintainability index (<{options.MaintainabilityThreshold})");
            Console.ResetColor();
        }

        // Excellent code quality
        if (metrics.MaintainabilityIndex >= 80 && metrics.CyclomaticComplexity <= options.ComplexityThreshold)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("      ✨ Excellent code quality");
            Console.ResetColor();
        }
    }

    private static void DisplayDetailedAnalysis(CodeAnalysisResult analysis, CommandLineOptions options)
    {
        // Unused usings
        if (analysis.SemanticAnalysis.UnusedUsings.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ⚠️  Unused using statements:");
            foreach (var unused in analysis.SemanticAnalysis.UnusedUsings)
            {
                Console.WriteLine($"      - Line {unused.Line}: {unused.Message}");
            }
            Console.ResetColor();
        }

        // Note: Legacy null reference display removed - now handled by configurable semantic analysis

        // Performance analysis
        DisplayPerformanceAnalysis(analysis.PerformanceAnalysis, options);

        // Style analysis
        DisplayStyleAnalysis(analysis.StyleAnalysis, options);

        // Semantic analysis
        DisplaySemanticAnalysis(analysis.SemanticIssues, options);

        // Syntax analysis details
        if (options.Verbosity == "detailed")
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    📝 Lines: {analysis.SyntaxAnalysis.CodeLines} code, {analysis.SyntaxAnalysis.CommentLines} comments, {analysis.SyntaxAnalysis.BlankLines} blank");
            Console.WriteLine($"    🏗️  Structure: {analysis.SyntaxAnalysis.ClassNames.Length} classes, {analysis.SyntaxAnalysis.PropertyNames.Length} properties");
            Console.ResetColor();
        }
    }

    private static void DisplayPerformanceAnalysis(PerformanceAnalysis performance, CommandLineOptions options)
    {
        var allIssues = performance.LinqPerformanceIssues
            .Concat(performance.AllocationIssues)
            .Concat(performance.AsyncPerformanceIssues)
            .Concat(performance.StringPerformanceIssues)
            .ToArray();

        if (!allIssues.Any()) return;

        // Summary for normal verbosity
        if (performance.Metrics.TotalPerformanceIssues > 0)
        {
            Console.ForegroundColor = GetPerformanceColor(performance.Metrics.HighSeverityIssues);
            Console.WriteLine($"    ⚡ Performance: {performance.Metrics.TotalPerformanceIssues} issues " +
                            $"(🔴 {performance.Metrics.HighSeverityIssues} high, " +
                            $"🟡 {performance.Metrics.MediumSeverityIssues} medium, " +
                            $"🟢 {performance.Metrics.LowSeverityIssues} low)");
            Console.ResetColor();
        }

        // Detailed analysis for verbose modes
        if (options.Verbosity == "detailed")
        {
            DisplayPerformanceIssuesByCategory("LINQ Performance", performance.LinqPerformanceIssues);
            DisplayPerformanceIssuesByCategory("Memory Allocation", performance.AllocationIssues);
            DisplayPerformanceIssuesByCategory("Async Performance", performance.AsyncPerformanceIssues);
            DisplayPerformanceIssuesByCategory("String Performance", performance.StringPerformanceIssues);
        }
    }

    private static void DisplayPerformanceIssuesByCategory(string category, PerformanceIssue[] issues)
    {
        if (!issues.Any()) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    ⚡ {category} Issues:");
        Console.ResetColor();

        foreach (var issue in issues.OrderByDescending(i => i.Severity))
        {
            var severityIcon = issue.Severity switch
            {
                PerformanceSeverity.High => "🔴",
                PerformanceSeverity.Medium => "🟡",
                PerformanceSeverity.Low => "🟢",
                _ => "⚪"
            };

            Console.ForegroundColor = GetSeverityColor(issue.Severity);
            Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
            
            if (issue.Recommendation != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"         💡 {issue.Recommendation}");
            }
            
            Console.ResetColor();
        }
    }

    private static ConsoleColor GetPerformanceColor(int highSeverityCount)
    {
        return highSeverityCount switch
        {
            0 => ConsoleColor.Green,
            <= 2 => ConsoleColor.Yellow,
            _ => ConsoleColor.Red
        };
    }

    private static ConsoleColor GetSeverityColor(PerformanceSeverity severity)
    {
        return severity switch
        {
            PerformanceSeverity.High => ConsoleColor.Red,
            PerformanceSeverity.Medium => ConsoleColor.Yellow,
            PerformanceSeverity.Low => ConsoleColor.Green,
            _ => ConsoleColor.Gray
        };
    }

    private static void DisplayStyleAnalysis(StyleAnalysis style, CommandLineOptions options)
    {
        var allIssues = style.DocumentationIssues
            .Concat(style.EncapsulationIssues)
            .Concat(style.AccessibilityIssues)
            .Concat(style.OrganizationIssues)
            .ToArray();

        if (!allIssues.Any()) return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    🎨 Style: {allIssues.Length} issues ({GetStyleSummary(style.Metrics)})");
        Console.ResetColor();

        if (options.Verbosity == "detailed")
        {
            // Documentation Issues
            if (style.DocumentationIssues.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    🎨 Documentation Issues:");
                Console.ResetColor();
                
                foreach (var issue in style.DocumentationIssues.OrderByDescending(i => i.Severity).Take(5))
                {
                    var severityIcon = GetStyleSeverityIcon(issue.Severity);
                    Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
                    if (options.Verbosity == "detailed" && !string.IsNullOrEmpty(issue.Recommendation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"         💡 {issue.Recommendation}");
                        Console.ResetColor();
                    }
                }
            }

            // Encapsulation Issues
            if (style.EncapsulationIssues.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    🎨 Encapsulation Issues:");
                Console.ResetColor();
                
                foreach (var issue in style.EncapsulationIssues.OrderByDescending(i => i.Severity).Take(5))
                {
                    var severityIcon = GetStyleSeverityIcon(issue.Severity);
                    Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
                    if (options.Verbosity == "detailed" && !string.IsNullOrEmpty(issue.Recommendation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"         💡 {issue.Recommendation}");
                        Console.ResetColor();
                    }
                }
            }

            // Accessibility Issues
            if (style.AccessibilityIssues.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    🎨 Accessibility Issues:");
                Console.ResetColor();
                
                foreach (var issue in style.AccessibilityIssues.OrderByDescending(i => i.Severity).Take(5))
                {
                    var severityIcon = GetStyleSeverityIcon(issue.Severity);
                    Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
                    if (options.Verbosity == "detailed" && !string.IsNullOrEmpty(issue.Recommendation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"         💡 {issue.Recommendation}");
                        Console.ResetColor();
                    }
                }
            }

            // Organization Issues
            if (style.OrganizationIssues.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    🎨 Organization Issues:");
                Console.ResetColor();
                
                foreach (var issue in style.OrganizationIssues.OrderByDescending(i => i.Severity).Take(5))
                {
                    var severityIcon = GetStyleSeverityIcon(issue.Severity);
                    Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
                    if (options.Verbosity == "detailed" && !string.IsNullOrEmpty(issue.Recommendation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"         💡 {issue.Recommendation}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }

    private static string GetStyleSummary(StyleMetrics metrics)
    {
        var parts = new List<string>();
        
        if (metrics.DocumentationViolations > 0)
            parts.Add($"📝 {metrics.DocumentationViolations} docs");
        if (metrics.EncapsulationViolations > 0)
            parts.Add($"🔒 {metrics.EncapsulationViolations} encapsulation");
        if (metrics.AccessibilityViolations > 0)
            parts.Add($"♿ {metrics.AccessibilityViolations} accessibility");
        if (metrics.OrganizationViolations > 0)
            parts.Add($"📋 {metrics.OrganizationViolations} organization");
            
        return parts.Any() ? string.Join(", ", parts) : "✨ clean code";
    }

    private static void DisplaySemanticAnalysis(SemanticIssue[] semanticIssues, CommandLineOptions options)
    {
        if (!semanticIssues.Any()) return;

        var totalIssues = semanticIssues.Length;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"    🔍 Semantic: {totalIssues} issues ({GetSemanticSummary(semanticIssues)})");
        Console.ResetColor();

        // Group issues by category
        var issuesByCategory = semanticIssues.GroupBy(i => i.Category).ToList();

        foreach (var categoryGroup in issuesByCategory)
        {
            var categoryIssues = categoryGroup.ToArray();
            if (!categoryIssues.Any()) continue;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"    🔍 {categoryGroup.Key} Issues:");
            Console.ResetColor();

            foreach (var issue in categoryIssues.OrderByDescending(i => i.Severity).Take(options.Verbosity == "detailed" ? 10 : 3))
            {
                var severityIcon = GetStyleSeverityIcon(issue.Severity);
                Console.WriteLine($"      {severityIcon} Line {issue.Line}: {issue.Message}");
                Console.WriteLine($"         💡 {issue.Recommendation}");
            }

            if (categoryIssues.Length > 3 && options.Verbosity != "detailed")
            {
                Console.WriteLine($"      ... and {categoryIssues.Length - 3} more {categoryGroup.Key.ToLower()} issues");
            }
        }
    }

    private static string GetSemanticSummary(SemanticIssue[] issues)
    {
        var parts = new List<string>();
        
        var unusedImports = issues.Count(i => i.Category == "Unused Imports");
        var nullRefs = issues.Count(i => i.Category == "Null References");
        var typeIssues = issues.Count(i => i.Category == "Type Analysis");
        var codeFlow = issues.Count(i => i.Category == "Code Flow");
        
        if (unusedImports > 0)
            parts.Add($"📦 {unusedImports} imports");
        if (nullRefs > 0)
            parts.Add($"⚠️ {nullRefs} null refs");
        if (typeIssues > 0)
            parts.Add($"🔧 {typeIssues} types");
        if (codeFlow > 0)
            parts.Add($"🌊 {codeFlow} flow");
            
        return parts.Any() ? string.Join(", ", parts) : "✨ clean semantics";
    }

    private static string GetStyleSeverityIcon(StyleSeverity severity)
    {
        return severity switch
        {
            StyleSeverity.Error => "🔴",
            StyleSeverity.Warning => "🟡",
            StyleSeverity.Info => "🔵",
            _ => "⚪"
        };
    }
}