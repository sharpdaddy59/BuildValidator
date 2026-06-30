using Microsoft.CodeAnalysis;
using System.Text;

namespace BuildValidator;

public static class OutputFormatters
{
    public static async Task WriteResultsAsync(IEnumerable<BuildResult> results, CommandLineOptions options, TimeSpan? wallClock = null)
    {
        switch (options.OutputFormat.ToLowerInvariant())
        {
            case "csv":
                await WriteCsvAsync(results, options);
                break;
            case "sarif":
                await WriteSarifAsync(results, options);
                break;
            case "json":
                await WriteJsonAsync(results, options);
                break;
            case "markdown":
            case "md":
                await WriteMarkdownAsync(results, options);
                break;
            default:
                BuildResultFormatter.DisplayResults(results, options, wallClock);
                break;
        }
    }

    private static async Task WriteCsvAsync(IEnumerable<BuildResult> results, CommandLineOptions options)
    {
        var csv = new StringBuilder();
        
        // CSV Headers for compilation results
        csv.AppendLine("Type,ProjectName,ProjectPath,Status,Duration(s),ErrorMessage,DiagnosticSeverity,DiagnosticId,DiagnosticMessage,DiagnosticFile,DiagnosticLine");
        
        foreach (var result in results)
        {
            // Main project result row
            csv.AppendLine($"Project,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(result.ProjectPath)}\",{result.Status},{result.Duration.TotalSeconds:F1},\"{EscapeCsv(result.ErrorMessage ?? "")}\",,,,,");
            
            // Diagnostic rows
            foreach (var diagnostic in result.Diagnostics)
            {
                csv.AppendLine($"Diagnostic,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(result.ProjectPath)}\",{result.Status},{result.Duration.TotalSeconds:F1},,{diagnostic.Severity},\"{EscapeCsv(diagnostic.Id)}\",\"{EscapeCsv(diagnostic.Message)}\",\"{EscapeCsv(diagnostic.FilePath ?? "")}\",{diagnostic.LineNumber}");
            }
        }

        // Add analysis results if available
        if (results.Any(r => r.AnalysisResults != null && r.AnalysisResults.Any()))
        {
            csv.AppendLine();
            csv.AppendLine("Type,ProjectName,FileName,CyclomaticComplexity,MaintainabilityIndex,MethodCount,ClassCount,PropertyCount,NestingDepth,CodeLines,CommentLines,BlankLines,TotalLines");
            
            foreach (var result in results.Where(r => r.AnalysisResults != null))
            {
                foreach (var analysis in result.AnalysisResults!)
                {
                    var fileName = Path.GetFileName(analysis.FilePath);
                    var metrics = analysis.CodeMetrics;
                    var syntax = analysis.SyntaxAnalysis;
                    
                    csv.AppendLine($"CodeMetrics,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(fileName)}\",{metrics.CyclomaticComplexity},{metrics.MaintainabilityIndex:F1},{metrics.MethodCount},{metrics.ClassCount},{metrics.PropertyCount},{metrics.NestingDepth},{syntax.CodeLines},{syntax.CommentLines},{syntax.BlankLines},{syntax.TotalLines}");
                }
            }

            // Add code issues if available
            csv.AppendLine();
            csv.AppendLine("Type,ProjectName,FileName,IssueType,Severity,Line,Column,Message,Recommendation");
            
            foreach (var result in results.Where(r => r.AnalysisResults != null))
            {
                foreach (var analysis in result.AnalysisResults!)
                {
                    var fileName = Path.GetFileName(analysis.FilePath);
                    
                    // Unused usings
                    foreach (var issue in analysis.SemanticAnalysis.UnusedUsings)
                    {
                        csv.AppendLine($"CodeIssue,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(fileName)}\",UnusedUsing,Low,{issue.Line},{issue.Column},\"{EscapeCsv(issue.Message)}\",");
                    }
                    
                    // Potential null references
                    foreach (var issue in analysis.SemanticAnalysis.PotentialNullReferences)
                    {
                        csv.AppendLine($"CodeIssue,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(fileName)}\",PotentialNullReference,Medium,{issue.Line},{issue.Column},\"{EscapeCsv(issue.Message)}\",");
                    }

                    // Performance issues
                    var allPerformanceIssues = analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
                        .Concat(analysis.PerformanceAnalysis.StringPerformanceIssues);

                    foreach (var issue in allPerformanceIssues)
                    {
                        csv.AppendLine($"PerformanceIssue,\"{EscapeCsv(result.ProjectName)}\",\"{EscapeCsv(fileName)}\",\"{EscapeCsv(issue.Category)}\",{issue.Severity},{issue.Line},{issue.Column},\"{EscapeCsv(issue.Message)}\",\"{EscapeCsv(issue.Recommendation)}\"");
                    }
                }
            }
        }

        await WriteToFileAsync(csv.ToString(), options, "csv");
    }

    private static async Task WriteSarifAsync(IEnumerable<BuildResult> results, CommandLineOptions options)
    {
        // Paths are emitted relative to the analysis root so GitHub code scanning
        // can map results to files in the repository for inline annotations.
        var baseDir = string.IsNullOrEmpty(options.Directory)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(options.Directory);

        // SARIF 2.1.0 format implementation
        var sarif = new
        {
            version = "2.1.0",
            runs = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "BuildValidator",
                            version = "1.0.0",
                            informationUri = "https://github.com/buildvalidator",
                            rules = new[]
                            {
                                new
                                {
                                    id = "BV0001",
                                    name = "CompilationError",
                                    shortDescription = new { text = "Compilation Error" },
                                    fullDescription = new { text = "The code contains compilation errors that prevent successful build." },
                                    messageStrings = new
                                    {
                                        @default = new { text = "{0}" }
                                    }
                                },
                                new
                                {
                                    id = "BV0002", 
                                    name = "UnusedUsing",
                                    shortDescription = new { text = "Unused Using Statement" },
                                    fullDescription = new { text = "The file contains unnecessary using statements that can be removed." },
                                    messageStrings = new
                                    {
                                        @default = new { text = "{0}" }
                                    }
                                },
                                new
                                {
                                    id = "BV0003",
                                    name = "PotentialNullReference", 
                                    shortDescription = new { text = "Potential Null Reference" },
                                    fullDescription = new { text = "The code may contain potential null reference exceptions." },
                                    messageStrings = new
                                    {
                                        @default = new { text = "{0}" }
                                    }
                                },
                                new
                                {
                                    id = "BV0004",
                                    name = "PerformanceIssue",
                                    shortDescription = new { text = "Performance Issue" },
                                    fullDescription = new { text = "The code contains patterns that may impact performance." },
                                    messageStrings = new
                                    {
                                        @default = new { text = "{0}" }
                                    }
                                }
                            }
                        }
                    },
                    results = CreateSarifResults(results, baseDir).ToArray()
                }
            }
        };

        var serializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        // SARIF uses the reserved "$schema" key, which a C# anonymous type can't
        // express. Serialize the body, then prepend $schema so the document
        // validates (GitHub's code scanning rejects a plain "schema" property).
        var root = System.Text.Json.JsonSerializer.SerializeToNode(sarif, serializerOptions)!.AsObject();
        var document = new System.Text.Json.Nodes.JsonObject
        {
            ["$schema"] = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json"
        };
        foreach (var property in root)
        {
            document[property.Key] = property.Value?.DeepClone();
        }

        var json = document.ToJsonString(serializerOptions);

        await WriteToFileAsync(json, options, "sarif");
    }

    private static IEnumerable<object> CreateSarifResults(IEnumerable<BuildResult> results, string baseDir)
    {
        foreach (var result in results)
        {
            // Compilation diagnostics
            foreach (var diagnostic in result.Diagnostics)
            {
                yield return new
                {
                    ruleId = "BV0001",
                    level = diagnostic.Severity.ToString().ToLower(),
                    message = new { text = diagnostic.Message },
                    locations = new[]
                    {
                        new
                        {
                            physicalLocation = new
                            {
                                artifactLocation = new
                                {
                                    uri = ToRelativeUri(diagnostic.FilePath ?? result.ProjectPath, baseDir)
                                },
                                region = new
                                {
                                    startLine = diagnostic.LineNumber,
                                    startColumn = 1
                                }
                            }
                        }
                    }
                };
            }

            // Analysis results
            if (result.AnalysisResults != null)
            {
                foreach (var analysis in result.AnalysisResults)
                {
                    // Unused usings
                    foreach (var issue in analysis.SemanticAnalysis.UnusedUsings)
                    {
                        yield return new
                        {
                            ruleId = "BV0002",
                            level = "warning",
                            message = new { text = issue.Message },
                            locations = new[]
                            {
                                new
                                {
                                    physicalLocation = new
                                    {
                                        artifactLocation = new { uri = ToRelativeUri(analysis.FilePath, baseDir) },
                                        region = new
                                        {
                                            startLine = issue.Line,
                                            startColumn = issue.Column
                                        }
                                    }
                                }
                            }
                        };
                    }

                    // Potential null references
                    foreach (var issue in analysis.SemanticAnalysis.PotentialNullReferences)
                    {
                        yield return new
                        {
                            ruleId = "BV0003",
                            level = "warning",
                            message = new { text = issue.Message },
                            locations = new[]
                            {
                                new
                                {
                                    physicalLocation = new
                                    {
                                        artifactLocation = new { uri = ToRelativeUri(analysis.FilePath, baseDir) },
                                        region = new
                                        {
                                            startLine = issue.Line,
                                            startColumn = issue.Column
                                        }
                                    }
                                }
                            }
                        };
                    }

                    // Performance issues
                    var allPerformanceIssues = analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
                        .Concat(analysis.PerformanceAnalysis.StringPerformanceIssues);

                    foreach (var issue in allPerformanceIssues)
                    {
                        var level = issue.Severity switch
                        {
                            PerformanceSeverity.High => "error",
                            PerformanceSeverity.Medium => "warning",
                            PerformanceSeverity.Low => "note",
                            _ => "note"
                        };

                        yield return new
                        {
                            ruleId = "BV0004",
                            level = level,
                            message = new { text = $"{issue.Message}. {issue.Recommendation}" },
                            locations = new[]
                            {
                                new
                                {
                                    physicalLocation = new
                                    {
                                        artifactLocation = new { uri = ToRelativeUri(analysis.FilePath, baseDir) },
                                        region = new
                                        {
                                            startLine = issue.Line,
                                            startColumn = issue.Column
                                        }
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }
    }

    private static async Task WriteJsonAsync(IEnumerable<BuildResult> results, CommandLineOptions options)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        
        await WriteToFileAsync(json, options, "json");
    }

    private static async Task WriteMarkdownAsync(IEnumerable<BuildResult> results, CommandLineOptions options)
    {
        var md = new StringBuilder();
        var resultsList = results.ToList();
        var successCount = resultsList.Count(r => r.Status == BuildStatus.Success);
        var failureCount = resultsList.Count(r => r.Status == BuildStatus.Failed);
        var totalDuration = TimeSpan.FromMilliseconds(resultsList.Sum(r => r.Duration.TotalMilliseconds));
        
        // Header
        md.AppendLine("# BuildValidator Results");
        md.AppendLine();
        md.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss} | **Total Projects:** {resultsList.Count} | **✅ Passed:** {successCount} | **❌ Failed:** {failureCount} | **Duration:** {totalDuration.TotalSeconds:F1}s");
        md.AppendLine();

        // Project summary table
        md.AppendLine("## Project Results");
        md.AppendLine();
        md.AppendLine("| # | Project | Status | Duration | Errors |");
        md.AppendLine("|---|---------|--------|----------|--------|");

        int index = 1;
        foreach (var result in resultsList)
        {
            var status = result.Status == BuildStatus.Success ? "✅ Passed" : "❌ Failed";
            var errorSummary = string.Empty;

            if (result.Status == BuildStatus.Failed)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorSummary = result.ErrorMessage;
                else if (result.Diagnostics.Any())
                {
                    var firstError = result.Diagnostics.FirstOrDefault(d => d.Severity == DiagnosticSeverity.Error);
                    if (firstError != null)
                        errorSummary = $"{firstError.Id}: {firstError.Message}";
                }
            }
            
            // Escape pipe characters in cell content
            errorSummary = errorSummary.Replace("|", "\\|");
            
            md.AppendLine($"| {index} | {result.ProjectName} | {status} | {result.Duration.TotalSeconds:F1}s | {errorSummary} |");
            index++;
        }

        md.AppendLine();

        // Diagnostics table across all projects (if any)
        var allDiagnostics = resultsList
            .SelectMany(r => r.Diagnostics.Select(d => new { r.ProjectName, d }))
            .ToList();
        
        if (allDiagnostics.Any())
        {
            md.AppendLine("### Diagnostics");
            md.AppendLine();
            md.AppendLine("| Project | Severity | Code | Message | Location |");
            md.AppendLine("|---------|----------|------|---------|----------|");

            foreach (var item in allDiagnostics)
            {
                var location = string.Empty;
                if (!string.IsNullOrEmpty(item.d.FilePath))
                {
                    var fileName = Path.GetFileName(item.d.FilePath);
                    location = item.d.LineNumber > 0 ? $"{fileName}:{item.d.LineNumber}" : fileName;
                }
                var message = item.d.Message.Replace("|", "\\|");
                md.AppendLine($"| {item.ProjectName} | {item.d.Severity} | {item.d.Id} | {message} | {location} |");
            }
            md.AppendLine();
        }

        // Analysis results across all projects
        var projectsWithAnalysis = resultsList.Where(r => r.AnalysisResults != null && r.AnalysisResults.Any()).ToList();
        if (projectsWithAnalysis.Any())
        {
            md.AppendLine("---");
            md.AppendLine();
            md.AppendLine("## Code Quality Analysis");
            md.AppendLine();

            // Code metrics table
            md.AppendLine("### Code Metrics");
            md.AppendLine();
            md.AppendLine("| Project | File | Complexity | Maintainability | Methods | Classes | LOC |");
            md.AppendLine("|---------|------|-----------|----------------|---------|---------|-----|");

            foreach (var result in projectsWithAnalysis)
            {
                foreach (var analysis in result.AnalysisResults!)
                {
                    var fileName = Path.GetFileName(analysis.FilePath);
                    md.AppendLine($"| {result.ProjectName} | {fileName} | {analysis.CodeMetrics.CyclomaticComplexity} | {analysis.CodeMetrics.MaintainabilityIndex:F0} | {analysis.CodeMetrics.MethodCount} | {analysis.CodeMetrics.ClassCount} | {analysis.SyntaxAnalysis.CodeLines} |");
                }
            }
            md.AppendLine();

            // Code issues table (unused usings, null refs)
            var allCodeIssues = projectsWithAnalysis
                .SelectMany(r => r.AnalysisResults!.SelectMany(a =>
                    a.SemanticAnalysis.UnusedUsings.Select(u => new { r.ProjectName, File = Path.GetFileName(a.FilePath), Type = "Unused Using", u.Line, Severity = "Low", u.Message })
                    .Concat(a.SemanticAnalysis.PotentialNullReferences.Select(n => new { r.ProjectName, File = Path.GetFileName(a.FilePath), Type = "Null Reference", n.Line, Severity = "Medium", n.Message }))))
                .ToList();

            if (allCodeIssues.Any())
            {
                md.AppendLine("### Code Issues");
                md.AppendLine();
                md.AppendLine("| Project | File | Type | Line | Severity | Message |");
                md.AppendLine("|---------|------|------|------|----------|---------|");

                foreach (var issue in allCodeIssues)
                {
                    var msg = issue.Message.Replace("|", "\\|");
                    md.AppendLine($"| {issue.ProjectName} | {issue.File} | {issue.Type} | {issue.Line} | {issue.Severity} | {msg} |");
                }
                md.AppendLine();
            }

            // Performance issues table
            var allPerfIssues = projectsWithAnalysis
                .SelectMany(r => r.AnalysisResults!.SelectMany(a =>
                {
                    var fileName = Path.GetFileName(a.FilePath);
                    return a.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(a.PerformanceAnalysis.AllocationIssues)
                        .Concat(a.PerformanceAnalysis.AsyncPerformanceIssues)
                        .Concat(a.PerformanceAnalysis.StringPerformanceIssues)
                        .Select(p => new { r.ProjectName, File = fileName, p.Category, p.Severity, p.Line, p.Message, p.Recommendation });
                }))
                .ToList();

            if (allPerfIssues.Any())
            {
                md.AppendLine("### Performance Issues");
                md.AppendLine();
                md.AppendLine("| Project | File | Category | Severity | Line | Message |");
                md.AppendLine("|---------|------|----------|----------|------|---------|");

                foreach (var issue in allPerfIssues)
                {
                    var sev = issue.Severity switch
                    {
                        PerformanceSeverity.High => "🔴 High",
                        PerformanceSeverity.Medium => "🟡 Medium",
                        PerformanceSeverity.Low => "🟢 Low",
                        _ => "⚪ Info"
                    };
                    var msg = issue.Message.Replace("|", "\\|");
                    md.AppendLine($"| {issue.ProjectName} | {issue.File} | {issue.Category} | {sev} | {issue.Line} | {msg} |");
                }
                md.AppendLine();
            }
        }

        // Footer
        md.AppendLine("---");
        md.AppendLine();
        md.AppendLine($"_Report generated by BuildValidator on {DateTime.Now:yyyy-MM-dd HH:mm:ss}_");

        await WriteToFileAsync(md.ToString(), options, "md");
    }

    // Converts an absolute file path to a forward-slash URI relative to baseDir,
    // which GitHub code scanning resolves against the repository root. Falls back
    // to the path as-is when a relative path can't be formed (e.g. different drive).
    private static string ToRelativeUri(string? path, string baseDir)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        try
        {
            var relative = Path.GetRelativePath(baseDir, Path.GetFullPath(path));
            return relative.Replace('\\', '/');
        }
        catch
        {
            return path.Replace('\\', '/');
        }
    }

    private static async Task WriteToFileAsync(string content, CommandLineOptions options, string defaultExtension)
    {
        string filePath;
        
        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            filePath = options.OutputFile;
        }
        else
        {
            // Generate default filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            filePath = $"BuildValidator-Results-{timestamp}.{defaultExtension}";
        }

        await File.WriteAllTextAsync(filePath, content);
        Console.WriteLine($"Results written to: {filePath}");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Escape double quotes by doubling them
        value = value.Replace("\"", "\"\"");
        
        // If the value contains commas, quotes, or newlines, it needs to be quoted
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value}\"";
        }

        return value;
    }
}