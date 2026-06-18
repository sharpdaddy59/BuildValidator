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
        
        md.AppendLine("# BuildValidator Results");
        md.AppendLine();
        md.AppendLine($"**Analysis Date**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine($"**Total Projects**: {resultsList.Count}");
        md.AppendLine($"**Successful**: {resultsList.Count(r => r.Status == BuildStatus.Success)}");
        md.AppendLine($"**Failed**: {resultsList.Count(r => r.Status == BuildStatus.Failed)}");
        md.AppendLine();

        foreach (var result in resultsList)
        {
            md.AppendLine($"## {result.ProjectName}");
            md.AppendLine();
            md.AppendLine($"- **Status**: {(result.Status == BuildStatus.Success ? "✅ Success" : "❌ Failed")}");
            md.AppendLine($"- **Duration**: {result.Duration.TotalSeconds:F1}s");
            md.AppendLine($"- **Path**: `{result.ProjectPath}`");

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                md.AppendLine($"- **Error**: {result.ErrorMessage}");
            }

            if (result.Diagnostics.Any())
            {
                md.AppendLine();
                md.AppendLine("### Diagnostics");
                md.AppendLine();
                foreach (var diagnostic in result.Diagnostics)
                {
                    md.AppendLine($"- **{diagnostic.Severity}** {diagnostic.Id}: {diagnostic.Message}");
                    if (!string.IsNullOrEmpty(diagnostic.FilePath))
                    {
                        md.AppendLine($"  - File: `{diagnostic.FilePath}:{diagnostic.LineNumber}`");
                    }
                }
            }

            if (result.AnalysisResults != null && result.AnalysisResults.Any())
            {
                md.AppendLine();
                md.AppendLine("### Code Quality Analysis");
                md.AppendLine();

                foreach (var analysis in result.AnalysisResults)
                {
                    var fileName = Path.GetFileName(analysis.FilePath);
                    
                    // Make file name more prominent with larger heading and separator
                    md.AppendLine("---");
                    md.AppendLine();
                    md.AppendLine($"## 📄 **{fileName}**");
                    md.AppendLine();
                    md.AppendLine("**📊 Code Metrics:**");
                    md.AppendLine($"- **Complexity**: {analysis.CodeMetrics.CyclomaticComplexity}");
                    md.AppendLine($"- **Maintainability**: {analysis.CodeMetrics.MaintainabilityIndex:F0}");
                    md.AppendLine($"- **Methods**: {analysis.CodeMetrics.MethodCount}");
                    md.AppendLine($"- **Classes**: {analysis.CodeMetrics.ClassCount}");
                    md.AppendLine($"- **Lines of Code**: {analysis.SyntaxAnalysis.CodeLines}");

                    // Code Issues
                    if (analysis.SemanticAnalysis.UnusedUsings.Any() || analysis.SemanticAnalysis.PotentialNullReferences.Any())
                    {
                        md.AppendLine();
                        md.AppendLine("**🚨 Code Issues:**");
                        
                        foreach (var issue in analysis.SemanticAnalysis.UnusedUsings)
                        {
                            md.AppendLine($"- Line {issue.Line}: {issue.Message}");
                        }
                        
                        foreach (var issue in analysis.SemanticAnalysis.PotentialNullReferences)
                        {
                            md.AppendLine($"- Line {issue.Line}: {issue.Message}");
                        }
                    }

                    // Performance Analysis
                    var allPerformanceIssues = analysis.PerformanceAnalysis.LinqPerformanceIssues
                        .Concat(analysis.PerformanceAnalysis.AllocationIssues)
                        .Concat(analysis.PerformanceAnalysis.AsyncPerformanceIssues)
                        .Concat(analysis.PerformanceAnalysis.StringPerformanceIssues)
                        .ToArray();

                    if (allPerformanceIssues.Any())
                    {
                        md.AppendLine();
                        md.AppendLine("**⚡ Performance Analysis:**");
                        md.AppendLine($"- Total Issues: {analysis.PerformanceAnalysis.Metrics.TotalPerformanceIssues}");
                        md.AppendLine($"- High Severity: {analysis.PerformanceAnalysis.Metrics.HighSeverityIssues}");
                        md.AppendLine($"- Medium Severity: {analysis.PerformanceAnalysis.Metrics.MediumSeverityIssues}");
                        md.AppendLine($"- Low Severity: {analysis.PerformanceAnalysis.Metrics.LowSeverityIssues}");
                        
                        md.AppendLine();
                        md.AppendLine("**📋 Performance Issues by Category:**");
                        
                        var groupedIssues = allPerformanceIssues.GroupBy(i => i.Category);
                        foreach (var group in groupedIssues)
                        {
                            md.AppendLine($"- **{group.Key}** ({group.Count()} issues):");
                            foreach (var issue in group.OrderByDescending(i => i.Severity).Take(5)) // Top 5 issues per category
                            {
                                var severityIcon = issue.Severity switch
                                {
                                    PerformanceSeverity.High => "🔴",
                                    PerformanceSeverity.Medium => "🟡",
                                    PerformanceSeverity.Low => "🟢",
                                    _ => "⚪"
                                };
                                md.AppendLine($"  - {severityIcon} Line {issue.Line}: {issue.Message}");
                                if (!string.IsNullOrEmpty(issue.Recommendation))
                                {
                                    md.AppendLine($"    - 💡 {issue.Recommendation}");
                                }
                            }
                        }
                    }

                    md.AppendLine();
                }
            }

            md.AppendLine();
        }

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