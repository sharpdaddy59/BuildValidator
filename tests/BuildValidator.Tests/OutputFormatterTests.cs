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
    public async Task Sarif_UsesRepoRelativeForwardSlashUris()
    {
        // Build platform-appropriate absolute paths so the assertion holds on
        // both Windows and Linux CI.
        var baseDir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo"));
        var filePath = Path.Combine(baseDir, "src", "Broken.cs");

        var results = new[]
        {
            new BuildResult
            {
                ProjectPath = Path.Combine(baseDir, "src", "Broken.csproj"),
                ProjectName = "Broken",
                Status = BuildStatus.Failed,
                Duration = TimeSpan.FromSeconds(0.3),
                Diagnostics = new List<BuildDiagnostic>
                {
                    new BuildDiagnostic
                    {
                        Severity = DiagnosticSeverity.Error,
                        Id = "CS0246",
                        Message = "boom",
                        FilePath = filePath,
                        LineNumber = 7,
                        ColumnNumber = 1
                    }
                }
            }
        };

        using var dir = new TempDir();
        var outFile = Path.Combine(dir.Path, "results.sarif");
        var options = new CommandLineOptions
        {
            Directory = baseDir,
            OutputFormat = "sarif",
            OutputFile = outFile,
            Verbosity = "minimal"
        };

        await OutputFormatters.WriteResultsAsync(results, options);

        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(outFile));
        var uri = doc.RootElement
            .GetProperty("runs")[0]
            .GetProperty("results")[0]
            .GetProperty("locations")[0]
            .GetProperty("physicalLocation")
            .GetProperty("artifactLocation")
            .GetProperty("uri")
            .GetString();

        Assert.Equal("src/Broken.cs", uri);
    }

    [Fact]
    public async Task Json_IsParseable_AndMentionsProjects()
    {
        var json = await WriteAndRead("json");

        using var doc = JsonDocument.Parse(json); // throws if invalid JSON
        Assert.Contains("Good", json);
        Assert.Contains("Bad", json);
    }

    [Fact]
    public async Task Markdown_HasTableStructure_WithProjectResults()
    {
        var md = await WriteAndRead("md");

        // Should have a table header
        Assert.Contains("| # | Project | Status | Duration | Errors |", md);
        // Should have the separator row
        Assert.Contains("|---|---------|--------|----------|--------|", md);
        // Should list both projects
        Assert.Contains("| 1 | Good | ✅ Passed |", md);
        Assert.Contains("| 2 | Bad | ❌ Failed |", md);
        // Should include the error diagnostic
        Assert.Contains("CS0246", md);
        Assert.Contains("Bad.cs:5", md);
        // Should have a footer
        Assert.Contains("_Report generated by BuildValidator", md);
    }

    [Fact]
    public async Task Markdown_HasSummaryLine_WithCounts()
    {
        var md = await WriteAndRead("md");

        // Summary header line with counts
        Assert.Contains("**Total Projects:** 2", md);
        Assert.Contains("**✅ Passed:** 1", md);
        Assert.Contains("**❌ Failed:** 1", md);
    }

    [Fact]
    public async Task Markdown_HasDiagnosticsTable_WhenDiagnosticsExist()
    {
        var md = await WriteAndRead("md");

        Assert.Contains("### Diagnostics", md);
        Assert.Contains("| Project | Severity | Code | Message | Location |", md);
    }

    [Fact]
    public async Task Markdown_CanAcceptMdAlias()
    {
        // The "md" alias and "markdown" format name should both route to the same output
        var md = await WriteAndRead("markdown");

        Assert.Contains("| # | Project | Status | Duration | Errors |", md);
        Assert.Contains("Good", md);
    }
}
