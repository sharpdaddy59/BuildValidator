using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;

namespace BuildValidator;

public static class BuildValidatorApp
{
    public static async Task<int> RunAsync(CommandLineOptions options)
    {
        Console.WriteLine($"Building C# Projects in: {options.Directory}");
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
            
            // For now, just show what we found
            Console.WriteLine($"Found {projectFiles.Count} project(s)/solution(s):");
            foreach (var file in projectFiles)
            {
                Console.WriteLine($"  {Path.GetFileName(file)}");
            }
            
            // TODO: Implement actual building
            Console.WriteLine();
            Console.WriteLine("Build validation functionality will be implemented next.");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
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