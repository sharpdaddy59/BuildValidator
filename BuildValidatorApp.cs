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
            var projectFiles = DiscoverProjects(options.Directory);
            
            if (projectFiles.Count == 0)
            {
                Console.WriteLine("No C# project or solution files found.");
                return 1;
            }

            // Filter to only project files for now (skip .sln files until we implement solution support)
            var projects = projectFiles.Where(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || 
                                                  f.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
                                       .ToList();

            if (projects.Count == 0)
            {
                Console.WriteLine("No individual project files found. Solution file support coming soon.");
                return 1;
            }

            if (options.Verbosity != "minimal")
            {
                Console.WriteLine($"Found {projects.Count} project(s) to build:");
                foreach (var project in projects)
                {
                    Console.WriteLine($"  {Path.GetFileName(project)}");
                }
                Console.WriteLine();
            }

            // Create build engine and compile projects
            var buildEngine = new BuildEngine(options);
            var results = await buildEngine.CompileProjectsAsync(projects);

            // Display or export results
            await OutputFormatters.WriteResultsAsync(results, options);

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
    
    private static List<string> DiscoverProjects(string directory)
    {
        var projectFiles = new List<string>();
        
        // Look for solution files first
        var solutionFiles = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
        projectFiles.AddRange(solutionFiles);
        
        // Then look for individual project files
        var csharpProjects = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
        projectFiles.AddRange(csharpProjects);
        
        var vbProjects = Directory.GetFiles(directory, "*.vbproj", SearchOption.AllDirectories);
        projectFiles.AddRange(vbProjects);
        
        // Sort for consistent ordering
        projectFiles.Sort();
        
        return projectFiles;
    }
}