using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace BuildValidator.Tests;

public class OutputFormatterTests
{
    private static BuildResult[] SampleResults() => new[]
    {
        new BuildResult
        {
            ProjectPath = @"C:\repo\Good.csproj",
            ProjectName = "Good",
            Status = BuildStatus.Success,
            Duration = TimeSpan.FromSeconds(1.2),
            Diagnostics = new List<BuildDiagnostic>()
        },
        new BuildResult
        {
            ProjectPath = @"C:\repo\Bad.csproj",
            ProjectName = "Bad",
            Status = BuildStatus.Failed,
            Duration = TimeSpan.FromSeconds(0.5),
            Diagnostics = new List<BuildDiagnostic>
            {
                new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Id = "CS0246",
                    Message = "Type 'Foo' not found",
                    FilePath = @"C:\repo\Bad.cs",
                    LineNumber = 5,
                    ColumnNumber = 2
                }
            }
        }
    };

    private static async Task<string> WriteAndRead(string format)
    {
        using var dir = new TempDir();
        var outFile = Path.Combine(dir.Path, $"results.{format}");
        var options = new CommandLineOptions { OutputFormat = format, OutputFile = outFile, Verbosity = "minimal" };

        await OutputFormatters.WriteResultsAsync(SampleResults(), options);

        Assert.True(File.Exists(outFile), $"expected output file at {outFile}");
        return await File.ReadAllTextAsync(outFile);
    }

    [Fact]
    public async Task Csv_HasHeaderAndRows()
    {
        var csv = await WriteAndRead("csv");

        Assert.Contains("Type,ProjectName,ProjectPath,Status", csv);
        Assert.Contains("Good", csv);
        Assert.Contains("Bad", csv);
        Assert.Contains("CS0246", csv);
        Assert.Contains("Failed", csv);
    }

    [Fact]
    public async Task Sarif_IsValidJson_WithExpectedStructure()
    {
        var sarif = await WriteAndRead("sarif");

        using var doc = JsonDocument.Parse(sarif); // throws if invalid JSON
        var root = doc.RootElement;

        Assert.Equal("2.1.0", root.GetProperty("version").GetString());
        // GitHub code scanning requires the reserved "$schema" key (not "schema").
        Assert.True(root.TryGetProperty("$schema", out _), "SARIF must use the $schema property");
        Assert.False(root.TryGetProperty("schema", out _), "SARIF must not use a plain 'schema' property");
        var runs = root.GetProperty("runs");
        Assert.True(runs.GetArrayLength() >= 1);
        var driver = runs[0].GetProperty("tool").GetProperty("driver");
        Assert.Equal("BuildValidator", driver.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Json_IsParseable_AndMentionsProjects()
    {
        var json = await WriteAndRead("json");

        using var doc = JsonDocument.Parse(json); // throws if invalid JSON
        Assert.Contains("Good", json);
        Assert.Contains("Bad", json);
    }
}
