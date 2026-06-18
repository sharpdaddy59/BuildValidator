using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BuildValidator.Tests;

/// <summary>
/// Restores the fixture projects once so BuildValidator can load/compile them.
/// </summary>
public sealed class FixturesRestoreFixture
{
    public string FixturesDir { get; }

    public FixturesRestoreFixture()
    {
        FixturesDir = LocateFixturesDir();
        foreach (var csproj in Directory.GetFiles(FixturesDir, "*.csproj", SearchOption.AllDirectories))
        {
            Run("dotnet", "restore", csproj);
        }
    }

    private static string LocateFixturesDir([CallerFilePath] string? thisFile = null)
        => Path.Combine(Path.GetDirectoryName(thisFile)!, "Fixtures");

    private static void Run(string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        p.StandardOutput.ReadToEnd();
        p.StandardError.ReadToEnd();
        p.WaitForExit();
    }
}

public class E2EBuildTests : IClassFixture<FixturesRestoreFixture>
{
    private readonly FixturesRestoreFixture _fixtures;

    public E2EBuildTests(FixturesRestoreFixture fixtures) => _fixtures = fixtures;

    private async Task<(int ExitCode, string Output)> RunValidator(string relativeTarget, params string[] extraArgs)
    {
        var dll = Path.Combine(AppContext.BaseDirectory, "BuildValidator.dll");
        Assert.True(File.Exists(dll), $"BuildValidator.dll not found at {dll}");

        var target = Path.Combine(_fixtures.FixturesDir, relativeTarget);
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.ArgumentList.Add("exec");
        psi.ArgumentList.Add(dll);
        psi.ArgumentList.Add(target);
        foreach (var a in extraArgs) psi.ArgumentList.Add(a);

        using var p = Process.Start(psi)!;
        var stdoutTask = p.StandardOutput.ReadToEndAsync();
        var stderrTask = p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        var output = (await stdoutTask) + (await stderrTask);
        return (p.ExitCode, output);
    }

    [Fact]
    public async Task PassingProject_ReturnsZero_AndReportsSuccess()
    {
        var (exit, output) = await RunValidator("PassingProject", "--verbosity", "normal");

        Assert.Equal(0, exit);
        Assert.Contains("succeeded", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FailingProject_ReturnsNonZero_AndReportsTheCompileError()
    {
        var (exit, output) = await RunValidator("FailingProject", "--verbosity", "normal");

        Assert.NotEqual(0, exit);
        Assert.Contains("CS0246", output);
    }

    [Fact]
    public async Task SlnxSolution_IsDiscoveredAndBuilt()
    {
        var (exit, output) = await RunValidator("SlnxSolution", "--verbosity", "normal");

        Assert.Equal(0, exit);
        Assert.Contains("App.slnx", output);
        Assert.Contains("succeeded", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SolutionMode_ReportsEachProjectSeparately()
    {
        var (exit, output) = await RunValidator("MixedSolution", "--verbosity", "normal");

        // The solution has one passing and one failing project, so the overall
        // run fails but each project is reported under its own name.
        Assert.NotEqual(0, exit);
        Assert.Contains("Mixed / Good", output);
        Assert.Contains("Mixed / Bad", output);
        Assert.Contains("CS0246", output);
        // Exactly one passed and one failed.
        Assert.Contains("1 succeeded, 1 failed", output);
    }
}
