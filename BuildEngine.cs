using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using System.Diagnostics;

namespace BuildValidator;


public enum BuildStatus
{
    Success,
    Failed,
    Skipped
}

public record BuildResult
{
    public string ProjectPath { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public BuildStatus Status { get; init; }
    public TimeSpan Duration { get; init; }
    public List<BuildDiagnostic> Diagnostics { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public List<CodeAnalysisResult>? AnalysisResults { get; init; }
}

public record BuildDiagnostic
{
    public DiagnosticSeverity Severity { get; init; }
    public string Id { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? FilePath { get; init; }
    public int LineNumber { get; init; }
    public int ColumnNumber { get; init; }
}

public class BuildEngine
{
    private readonly CommandLineOptions _options;

    public BuildEngine(CommandLineOptions options)
    {
        _options = options;
    }

    public async Task<BuildResult> CompileProjectAsync(string projectPath)
    {
        var stopwatch = Stopwatch.StartNew();
        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        
        // Handle metrics-only mode - skip MSBuild compilation
        if (_options.MetricsOnly)
        {
            return await AnalyzeProjectMetricsOnly(projectPath, projectName, stopwatch);
        }
        
        try
        {
            // Use MSBuildWorkspace for full Roslyn analysis
            using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
            {
                ["Configuration"] = _options.Configuration
            });

            // Load the project
            var project = await workspace.OpenProjectAsync(projectPath);
            var compilation = await project.GetCompilationAsync();

            stopwatch.Stop();

            if (compilation == null)
            {
                return new BuildResult
                {
                    ProjectPath = projectPath,
                    ProjectName = projectName,
                    Status = BuildStatus.Failed,
                    Duration = stopwatch.Elapsed,
                    ErrorMessage = "Failed to create compilation"
                };
            }

            // Check for compilation errors
            var diagnostics = ConvertDiagnostics(compilation.GetDiagnostics());
            var hasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            var status = hasErrors ? BuildStatus.Failed : BuildStatus.Success;

            // Perform code analysis if requested
            List<CodeAnalysisResult>? analysisResults = null;
            if (_options.EnableAnalysis || _options.IncludeMetrics || _options.MetricsOnly)
            {
                analysisResults = await PerformCodeAnalysis(project);
            }

            return new BuildResult
            {
                ProjectPath = projectPath,
                ProjectName = projectName,
                Status = status,
                Duration = stopwatch.Elapsed,
                Diagnostics = diagnostics,
                AnalysisResults = analysisResults
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                ProjectPath = projectPath,
                ProjectName = projectName,
                Status = BuildStatus.Failed,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }



    private List<BuildDiagnostic> ConvertDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        var result = new List<BuildDiagnostic>();

        foreach (var diagnostic in diagnostics)
        {
            // Skip hidden diagnostics unless verbose
            if (diagnostic.Severity == DiagnosticSeverity.Hidden && _options.Verbosity != "detailed")
                continue;

            // Skip informational diagnostics unless verbose
            if (diagnostic.Severity == DiagnosticSeverity.Info && _options.Verbosity == "minimal")
                continue;

            // Skip warnings unless requested
            if (diagnostic.Severity == DiagnosticSeverity.Warning && !_options.IncludeWarnings)
                continue;

            var location = diagnostic.Location;
            var lineSpan = location.GetLineSpan();

            result.Add(new BuildDiagnostic
            {
                Severity = diagnostic.Severity,
                Id = diagnostic.Id,
                Message = diagnostic.GetMessage(),
                FilePath = location.IsInSource ? location.SourceTree?.FilePath : null,
                LineNumber = location.IsInSource ? lineSpan.StartLinePosition.Line + 1 : 0,
                ColumnNumber = location.IsInSource ? lineSpan.StartLinePosition.Character + 1 : 0
            });
        }

        return result;
    }

    public async Task<List<BuildResult>> CompileSolutionAsync(string solutionPath)
    {
        var solutionName = Path.GetFileNameWithoutExtension(solutionPath);

        try
        {
            // Use MSBuildWorkspace to load the entire solution
            using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
            {
                ["Configuration"] = _options.Configuration
            });

            // Load the solution
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            var results = new List<BuildResult>();

            // One BuildResult per project so per-project pass/fail, diagnostics,
            // and timing are preserved (rather than collapsed into the solution).
            foreach (var project in solution.Projects)
            {
                var stopwatch = Stopwatch.StartNew();
                var projectName = $"{solutionName} / {project.Name}";

                var compilation = await project.GetCompilationAsync();
                if (compilation == null)
                {
                    stopwatch.Stop();
                    results.Add(new BuildResult
                    {
                        ProjectPath = project.FilePath ?? solutionPath,
                        ProjectName = projectName,
                        Status = BuildStatus.Failed,
                        Duration = stopwatch.Elapsed,
                        ErrorMessage = "Failed to create compilation"
                    });
                    continue;
                }

                var diagnostics = ConvertDiagnostics(compilation.GetDiagnostics());
                var hasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

                List<CodeAnalysisResult>? analysisResults = null;
                if (_options.EnableAnalysis || _options.IncludeMetrics || _options.MetricsOnly)
                {
                    analysisResults = await PerformCodeAnalysis(project);
                }

                stopwatch.Stop();

                results.Add(new BuildResult
                {
                    ProjectPath = project.FilePath ?? solutionPath,
                    ProjectName = projectName,
                    Status = hasErrors ? BuildStatus.Failed : BuildStatus.Success,
                    Duration = stopwatch.Elapsed,
                    Diagnostics = diagnostics,
                    AnalysisResults = analysisResults
                });
            }

            // An empty solution still yields one result so the run reports something.
            if (results.Count == 0)
            {
                results.Add(new BuildResult
                {
                    ProjectPath = solutionPath,
                    ProjectName = solutionName,
                    Status = BuildStatus.Success,
                    Duration = TimeSpan.Zero
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            // Solution-level failure (e.g. the solution couldn't be opened).
            return new List<BuildResult>
            {
                new BuildResult
                {
                    ProjectPath = solutionPath,
                    ProjectName = solutionName,
                    Status = BuildStatus.Failed,
                    Duration = TimeSpan.Zero,
                    ErrorMessage = ex.Message
                }
            };
        }
    }

    public async Task<List<BuildResult>> CompileProjectsAsync(IEnumerable<string> projectPaths)
    {
        var paths = projectPaths.ToList();

        // Build projects concurrently, throttled to the configured parallelism.
        // Each CompileProjectAsync creates its own MSBuildWorkspace, so the builds
        // are independent. Results are written by input index, so ordering is
        // preserved regardless of which build finishes first.
        using var throttle = new SemaphoreSlim(Math.Max(1, _options.ParallelCount));
        var results = new BuildResult[paths.Count];

        var tasks = paths.Select(async (path, index) =>
        {
            await throttle.WaitAsync();
            try
            {
                results[index] = await CompileProjectAsync(path);
            }
            finally
            {
                throttle.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        return results.ToList();
    }

    private async Task<List<CodeAnalysisResult>> PerformCodeAnalysis(Microsoft.CodeAnalysis.Project project)
    {
        var analyzer = new RoslynAnalyzer();
        var analysisResults = new List<CodeAnalysisResult>();
        
        // Load style configuration from project directory
        var projectDir = Path.GetDirectoryName(project.FilePath);
        var styleConfig = StyleConfigurationLoader.LoadConfiguration(projectDir);

        foreach (var document in project.Documents)
        {
            if (document.FilePath != null && document.FilePath.EndsWith(".cs"))
            {
                try
                {
                    var sourceText = await document.GetTextAsync();
                    var sourceCode = sourceText.ToString();
                    
                    var analysis = await analyzer.AnalyzeCodeAsync(sourceCode, document.FilePath, styleConfig);
                    analysisResults.Add(analysis);
                }
                catch (Exception ex)
                {
                    // Log analysis failure but continue with other files
                    if (_options.Verbosity == "detailed")
                    {
                        Console.WriteLine($"  Warning: Failed to analyze {document.Name}: {ex.Message}");
                    }
                }
            }
        }

        return analysisResults;
    }

    private async Task<BuildResult> AnalyzeProjectMetricsOnly(string projectPath, string projectName, Stopwatch stopwatch)
    {
        try
        {
            var analyzer = new RoslynAnalyzer();
            var analysisResults = new List<CodeAnalysisResult>();
            
            // Find all .cs files in the project directory
            var projectDir = Path.GetDirectoryName(projectPath);
            if (projectDir != null)
            {
                // Load style configuration from project directory
                var styleConfig = StyleConfigurationLoader.LoadConfiguration(projectDir);
                
                var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("bin") && !f.Contains("obj")) // Skip build output
                    .ToList();

                foreach (var csFile in csFiles)
                {
                    try
                    {
                        var analysis = await analyzer.AnalyzeFileAsync(csFile, styleConfig);
                        analysisResults.Add(analysis);
                    }
                    catch (Exception ex)
                    {
                        if (_options.Verbosity == "detailed")
                        {
                            Console.WriteLine($"  Warning: Failed to analyze {Path.GetFileName(csFile)}: {ex.Message}");
                        }
                    }
                }
            }

            stopwatch.Stop();
            
            return new BuildResult
            {
                ProjectPath = projectPath,
                ProjectName = projectName,
                Status = BuildStatus.Success,
                Duration = stopwatch.Elapsed,
                Diagnostics = new List<BuildDiagnostic>(),
                AnalysisResults = analysisResults
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new BuildResult
            {
                ProjectPath = projectPath,
                ProjectName = projectName,
                Status = BuildStatus.Failed,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message,
                AnalysisResults = new List<CodeAnalysisResult>()
            };
        }
    }
}