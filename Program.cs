using BuildValidator;
using Microsoft.Build.Locator;

// Initialize MSBuild before using any Roslyn workspace APIs
if (!MSBuildLocator.IsRegistered)
{
    MSBuildLocator.RegisterDefaults();
}

try
{
    var options = CommandLineParser.Parse(args);
    return await BuildValidatorApp.RunAsync(options);
}
catch (CommandLineException ex)
{
    // The parser already wrote any user-facing message (error to stderr, or
    // help text to stdout); just honor the requested exit code.
    return ex.ExitCode;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
