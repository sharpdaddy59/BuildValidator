namespace BuildValidator.Tests;

public class DiscoveryTests
{
    [Fact]
    public void DiscoverProjects_FindsCsprojSlnAndSlnx_Recursively()
    {
        using var dir = new TempDir();
        dir.Touch("App.sln");
        dir.Touch("App.slnx");
        dir.Touch("src/Lib/Lib.csproj");
        dir.Touch("src/Legacy/Old.vbproj");
        dir.Touch("notes.txt");

        var found = BuildValidatorApp.DiscoverProjects(dir.Path);

        Assert.Contains(found, f => f.EndsWith("App.sln"));
        Assert.Contains(found, f => f.EndsWith("App.slnx"));
        Assert.Contains(found, f => f.EndsWith("Lib.csproj"));
        Assert.Contains(found, f => f.EndsWith("Old.vbproj"));
        Assert.DoesNotContain(found, f => f.EndsWith("notes.txt"));
    }

    [Fact]
    public void DeduplicateSolutionFiles_PrefersSlnx_WhenBothExistForSameSolution()
    {
        var input = new List<string>
        {
            @"C:\repo\App.sln",
            @"C:\repo\App.slnx",
        };

        var result = BuildValidatorApp.DeduplicateSolutionFiles(input);

        Assert.Single(result);
        Assert.EndsWith("App.slnx", result[0]);
    }

    [Fact]
    public void DeduplicateSolutionFiles_KeepsSlnWithoutSlnxCounterpart()
    {
        var input = new List<string>
        {
            @"C:\repo\App.sln",
            @"C:\repo\App.slnx",
            @"C:\repo\Other.sln",
        };

        var result = BuildValidatorApp.DeduplicateSolutionFiles(input);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.EndsWith("App.slnx"));
        Assert.Contains(result, f => f.EndsWith("Other.sln"));
        Assert.DoesNotContain(result, f => f.EndsWith("App.sln"));
    }

    [Fact]
    public void DeduplicateSolutionFiles_TreatsSameNameInDifferentDirsSeparately()
    {
        var input = new List<string>
        {
            @"C:\repo\a\App.sln",
            @"C:\repo\b\App.slnx",
        };

        var result = BuildValidatorApp.DeduplicateSolutionFiles(input);

        // Different directories => not the same solution; both kept.
        Assert.Equal(2, result.Count);
    }
}
