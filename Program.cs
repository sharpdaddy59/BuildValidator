using BuildValidator;

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
