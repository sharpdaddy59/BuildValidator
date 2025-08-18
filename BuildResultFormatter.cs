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
}