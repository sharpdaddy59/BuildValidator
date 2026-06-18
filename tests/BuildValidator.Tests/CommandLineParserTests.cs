namespace BuildValidator.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_ValidArgs_PopulatesOptions()
    {
        using var dir = new TempDir();
        var options = CommandLineParser.Parse(new[]
        {
            dir.Path, "--config", "Release", "--parallel", "4",
            "--verbosity", "detailed", "--analysis", "--warnings"
        });

        Assert.Equal(dir.Path, options.Directory);
        Assert.Equal("Release", options.Configuration);
        Assert.Equal(4, options.ParallelCount);
        Assert.Equal("detailed", options.Verbosity);
        Assert.True(options.EnableAnalysis);
        Assert.True(options.IncludeWarnings);
    }

    [Fact]
    public void Parse_Defaults_AreApplied()
    {
        using var dir = new TempDir();
        var options = CommandLineParser.Parse(new[] { dir.Path });

        Assert.Equal("Debug", options.Configuration);
        Assert.Equal("normal", options.Verbosity);
        Assert.Equal("console", options.OutputFormat);
        Assert.Equal(10, options.ComplexityThreshold);
        Assert.Equal(20, options.MaintainabilityThreshold);
        Assert.False(options.EnableAnalysis);
    }

    [Theory]
    [InlineData("results.csv", "csv")]
    [InlineData("results.sarif", "sarif")]
    [InlineData("results.json", "json")]
    [InlineData("report.md", "markdown")]
    public void Parse_AutoDetectsFormat_FromOutputExtension(string file, string expectedFormat)
    {
        using var dir = new TempDir();
        var output = Path.Combine(dir.Path, file);
        var options = CommandLineParser.Parse(new[] { dir.Path, "--output", output, "--verbosity", "minimal" });

        Assert.Equal(expectedFormat, options.OutputFormat);
        Assert.Equal(output, options.OutputFile);
    }

    [Fact]
    public void Parse_ExplicitFormat_IsNotOverriddenByExtension()
    {
        using var dir = new TempDir();
        var output = Path.Combine(dir.Path, "results.csv");
        var options = CommandLineParser.Parse(new[] { dir.Path, "--format", "json", "--output", output, "--verbosity", "minimal" });

        Assert.Equal("json", options.OutputFormat);
    }

    [Fact]
    public void Parse_NonexistentDirectory_ThrowsWithExitCode1()
    {
        var ex = Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(new[] { @"C:\definitely\does\not\exist\xyz123" }));
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void Parse_UnknownArgument_ThrowsWithExitCode1()
    {
        using var dir = new TempDir();
        var ex = Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(new[] { dir.Path, "--bogus" }));
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void Parse_InvalidConfig_ThrowsWithExitCode1()
    {
        using var dir = new TempDir();
        var ex = Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(new[] { dir.Path, "--config", "Banana" }));
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public void Parse_Help_ThrowsWithExitCode0()
    {
        var ex = Assert.Throws<CommandLineException>(() => CommandLineParser.Parse(new[] { "--help" }));
        Assert.Equal(0, ex.ExitCode);
    }

    [Fact]
    public void Parse_NoArgs_ThrowsWithExitCode1()
    {
        var ex = Assert.Throws<CommandLineException>(() => CommandLineParser.Parse(Array.Empty<string>()));
        Assert.Equal(1, ex.ExitCode);
    }
}
