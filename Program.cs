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
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
