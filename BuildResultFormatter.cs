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

        // Potential null references
        if (analysis.SemanticAnalysis.PotentialNullReferences.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ⚠️  Potential null references:");
            foreach (var nullRef in analysis.SemanticAnalysis.PotentialNullReferences)
            {
                Console.WriteLine($"      - Line {nullRef.Line}: {nullRef.Message}");
            }
            Console.ResetColor();
        }

        // Syntax analysis details
        if (options.Verbosity == "detailed")
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    📝 Lines: {analysis.SyntaxAnalysis.CodeLines} code, {analysis.SyntaxAnalysis.CommentLines} comments, {analysis.SyntaxAnalysis.BlankLines} blank");
            Console.WriteLine($"    🏗️  Structure: {analysis.SyntaxAnalysis.ClassNames.Length} classes, {analysis.SyntaxAnalysis.PropertyNames.Length} properties");
            Console.ResetColor();
        }
    }
}