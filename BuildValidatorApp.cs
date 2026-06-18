using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;

namespace BuildValidator;

public static class BuildValidatorApp
{
    public static async Task<int> RunAsync(CommandLineOptions options)
    {
        var modeText = options.MetricsOnly ? "Analyzing" : 
                      (options.EnableAnalysis || options.IncludeMetrics) ? "Building & Analyzing" : "Building";
        
        Console.WriteLine($"{modeText} C# Projects in: {options.Directory}");
        Console.WriteLine("==========================================");
        
        try
        {
            // Discover all project and solution files
            var discoveredFiles = DiscoverProjects(options.Directory);
            
            if (discoveredFiles.Count == 0)
            {
                Console.WriteLine("No C# project or solution files found.");
                return 1;
            }

            // Solution-first approach: prefer solution files over individual projects
            var solutionFiles = discoveredFiles.Where(f => f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                                                         f.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)).ToList();
            solutionFiles = DeduplicateSolutionFiles(solutionFiles);
            var projectFiles = discoveredFiles.Where(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                                         f.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase)).ToList();

            var buildEngine = new BuildEngine(options);
            List<BuildResult> results;
            var buildTimer = System.Diagnostics.Stopwatch.StartNew();

            if (solutionFiles.Any())
            {
                // Solution mode: compile entire solution(s)
                if (options.Verbosity != "minimal")
                {
                    Console.WriteLine($"Found {solutionFiles.Count} solution(s) to build:");
                    foreach (var solution in solutionFiles)
                    {
                        Console.WriteLine($"  {Path.GetFileName(solution)}");
                    }
                    Console.WriteLine();
                }

                results = new List<BuildResult>();
                foreach (var solutionPath in solutionFiles)
                {
                    var result = await buildEngine.CompileSolutionAsync(solutionPath);
                    results.Add(result);
                }
            }
            else if (projectFiles.Any())
            {
                // Fallback mode: compile individual projects
                if (options.Verbosity != "minimal")
                {
                    Console.WriteLine($"Found {projectFiles.Count} project(s) to build:");
                    foreach (var project in projectFiles)
                    {
                        Console.WriteLine($"  {Path.GetFileName(project)}");
                    }
                    Console.WriteLine();
                }

                results = await buildEngine.CompileProjectsAsync(projectFiles);
            }
            else
            {
                Console.WriteLine("No valid C# projects or solutions found.");
                return 1;
            }

            buildTimer.Stop();

            // Display or export results
            await OutputFormatters.WriteResultsAsync(results, options, buildTimer.Elapsed);

            // Return exit code based on results
            var hasFailures = results.Any(r => r.Status == BuildStatus.Failed);
            return hasFailures ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (options.Verbosity == "detailed")
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }
    
    internal static List<string> DiscoverProjects(string directory)
    {
        var projectFiles = new List<string>();

        // Look for solution files first (both legacy .sln and the newer XML-based .slnx)
        var solutionFiles = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
        projectFiles.AddRange(solutionFiles);

        var slnxFiles = Directory.GetFiles(directory, "*.slnx", SearchOption.AllDirectories);
        projectFiles.AddRange(slnxFiles);

        // Then look for individual project files
        var csharpProjects = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
        projectFiles.AddRange(csharpProjects);
        
        var vbProjects = Directory.GetFiles(directory, "*.vbproj", SearchOption.AllDirectories);
        projectFiles.AddRange(vbProjects);
        
        // Sort for consistent ordering
        projectFiles.Sort();

        return projectFiles;
    }

    // When both .sln and .slnx exist for the same solution (same directory + base name),
    // prefer the .slnx. This mirrors MSBuild's own precedence and avoids building the
    // same solution twice. .NET 10's `dotnet new sln` emits .slnx by default.
    internal static List<string> DeduplicateSolutionFiles(List<string> solutionFiles)
    {
        var slnxBaseNames = new HashSet<string>(
            solutionFiles
                .Where(f => f.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
                .Select(GetSolutionKey),
            StringComparer.OrdinalIgnoreCase);

        return solutionFiles
            .Where(f => f.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase) ||
                        !slnxBaseNames.Contains(GetSolutionKey(f)))
            .ToList();
    }

    // Directory + base name, used to match a .sln against its .slnx counterpart.
    private static string GetSolutionKey(string solutionPath)
    {
        var dir = Path.GetDirectoryName(solutionPath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(solutionPath);
        return Path.Combine(dir, name);
    }
}