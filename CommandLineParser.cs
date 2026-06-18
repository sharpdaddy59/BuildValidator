namespace BuildValidator;

public record CommandLineOptions
{
    public string Directory { get; init; } = string.Empty;
    public int ParallelCount { get; init; } = Environment.ProcessorCount;
    public string Configuration { get; init; } = "Debug";
    public string Verbosity { get; init; } = "normal";
    public bool IncludeWarnings { get; init; } = false;
    public bool EnableAnalysis { get; init; } = false;
    public bool MetricsOnly { get; init; } = false;
    public bool IncludeMetrics { get; init; } = false;
    public int ComplexityThreshold { get; init; } = 10;
    public int MaintainabilityThreshold { get; init; } = 20;
    public string OutputFormat { get; init; } = "console";
    public string? OutputFile { get; init; } = null;
}

// Thrown by the parser instead of calling Environment.Exit, so the parser is
// testable and Program.cs owns process termination. An empty Message means the
// caller already wrote output (e.g. help text) and only the exit code matters.
public class CommandLineException : Exception
{
    public int ExitCode { get; }

    public CommandLineException(string? message, int exitCode)
        : base(message ?? string.Empty)
    {
        ExitCode = exitCode;
    }
}

public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            throw new CommandLineException(null, 1);
        }

        // Check for help first
        if (args[0] == "--help" || args[0] == "-h")
        {
            ShowHelp();
            throw new CommandLineException(null, 0);
        }

        var options = new CommandLineOptions
        {
            Directory = args[0], // First argument is always the directory
            ParallelCount = Environment.ProcessorCount,
            Configuration = "Debug",
            Verbosity = "normal",
            IncludeWarnings = false,
            EnableAnalysis = false,
            MetricsOnly = false,
            IncludeMetrics = false,
            ComplexityThreshold = 10,
            MaintainabilityThreshold = 20,
            OutputFormat = "console",
            OutputFile = null
        };

        // Parse remaining arguments
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == "--parallel" || arg == "-p")
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int parallel))
                {
                    options = options with { ParallelCount = parallel };
                    i++; // Skip the value
                }
                else
                {
                    throw Fail("Error: --parallel requires a numeric value");
                }
            }
            else if (arg == "--config" || arg == "-c")
            {
                if (i + 1 < args.Length)
                {
                    string config = args[i + 1];
                    if (config.Equals("Debug", StringComparison.OrdinalIgnoreCase) ||
                        config.Equals("Release", StringComparison.OrdinalIgnoreCase))
                    {
                        options = options with { Configuration = config };
                        i++; // Skip the value
                    }
                    else
                    {
                        throw Fail("Error: Configuration must be either 'Debug' or 'Release'");
                    }
                }
                else
                {
                    throw Fail("Error: --config requires a value");
                }
            }
            else if (arg == "--verbosity" || arg == "-v")
            {
                if (i + 1 < args.Length)
                {
                    string verbosity = args[i + 1];
                    if (new[] { "minimal", "normal", "detailed" }.Contains(verbosity.ToLowerInvariant()))
                    {
                        options = options with { Verbosity = verbosity };
                        i++; // Skip the value
                    }
                    else
                    {
                        throw Fail("Error: Verbosity must be one of: minimal, normal, detailed");
                    }
                }
                else
                {
                    throw Fail("Error: --verbosity requires a value");
                }
            }
            else if (arg == "--warnings" || arg == "-w")
            {
                options = options with { IncludeWarnings = true };
            }
            else if (arg == "--analysis" || arg == "-a")
            {
                options = options with { EnableAnalysis = true };
            }
            else if (arg == "--metrics-only")
            {
                options = options with { MetricsOnly = true };
            }
            else if (arg == "--include-metrics")
            {
                options = options with { IncludeMetrics = true };
            }
            else if (arg == "--complexity-threshold")
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int threshold))
                {
                    options = options with { ComplexityThreshold = threshold };
                    i++;
                }
                else
                {
                    throw Fail("Error: --complexity-threshold requires a numeric value");
                }
            }
            else if (arg == "--maintainability-threshold")
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int threshold))
                {
                    options = options with { MaintainabilityThreshold = threshold };
                    i++;
                }
                else
                {
                    throw Fail("Error: --maintainability-threshold requires a numeric value");
                }
            }
            else if (arg == "--format")
            {
                if (i + 1 < args.Length)
                {
                    string format = args[i + 1];
                    if (new[] { "console", "json", "markdown", "md", "csv", "sarif" }.Contains(format.ToLowerInvariant()))
                    {
                        options = options with { OutputFormat = format };
                        i++;
                    }
                    else
                    {
                        throw Fail("Error: Format must be one of: console, json, markdown, md, csv, sarif");
                    }
                }
                else
                {
                    throw Fail("Error: --format requires a value");
                }
            }
            else if (arg == "--output")
            {
                if (i + 1 < args.Length)
                {
                    options = options with { OutputFile = args[i + 1] };
                    i++;
                }
                else
                {
                    throw Fail("Error: --output requires a file path");
                }
            }
            else if (arg == "--help" || arg == "-h")
            {
                ShowHelp();
                throw new CommandLineException(null, 0);
            }
            else
            {
                throw Fail($"Error: Unknown argument '{arg}'");
            }
        }

        // Validate directory exists
        if (!Directory.Exists(options.Directory))
        {
            throw Fail($"Error: Directory '{options.Directory}' does not exist");
        }

        // Validate parallel count
        if (options.ParallelCount <= 0)
        {
            throw Fail("Error: Parallel count must be greater than 0");
        }

        // Auto-detect format from file extension if output file is specified
        if (!string.IsNullOrEmpty(options.OutputFile) && options.OutputFormat == "console")
        {
            var extension = Path.GetExtension(options.OutputFile).ToLowerInvariant();
            var detectedFormat = extension switch
            {
                ".csv" => "csv",
                ".sarif" => "sarif",
                ".json" => "json",
                ".md" => "markdown",
                ".markdown" => "markdown",
                _ => "console"
            };

            if (detectedFormat != "console")
            {
                options = options with { OutputFormat = detectedFormat };
                if (options.Verbosity != "minimal")
                {
                    Console.WriteLine($"Auto-detected format: {detectedFormat} from file extension");
                }
            }
        }

        return options;
    }

    // Writes the error message to stderr and signals exit code 1.
    private static CommandLineException Fail(string message)
    {
        Console.Error.WriteLine(message);
        return new CommandLineException(message, 1);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("BuildValidator - Validates C# projects using MSBuild compilation + advanced Roslyn analysis");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  BuildValidator <directory> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  directory           The directory path to search for C# projects");
        Console.WriteLine();
        Console.WriteLine("Core Options:");
        Console.WriteLine("  --parallel, -p      Number of parallel builds (default: processor count)");
        Console.WriteLine("  --config, -c        Build configuration: Debug or Release (default: Debug)");
        Console.WriteLine("  --verbosity, -v     Output verbosity: minimal, normal, or detailed (default: normal)");
        Console.WriteLine("  --warnings, -w      Include warnings in output (default: false)");
        Console.WriteLine();
        Console.WriteLine("Analysis Options:");
        Console.WriteLine("  --analysis, -a      Enable full analysis mode (compilation + code quality analysis)");
        Console.WriteLine("  --metrics-only      Skip compilation, perform code quality analysis only");
        Console.WriteLine("  --include-metrics   Include code metrics in standard compilation mode");
        Console.WriteLine("  --complexity-threshold <n>    Flag methods with complexity > n (default: 10)");
        Console.WriteLine("  --maintainability-threshold <n>  Flag files with maintainability index < n (default: 20)");
        Console.WriteLine();
        Console.WriteLine("Output Options:");
        Console.WriteLine("  --format <format>   Output format: console, csv, sarif, json, markdown/md (default: console)");
        Console.WriteLine("  --output <file>     Save results to file (format auto-detected from extension)");
        Console.WriteLine("  --help, -h          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  BuildValidator ./src");
        Console.WriteLine("  BuildValidator ./src --config Release --parallel 4");
        Console.WriteLine("  BuildValidator ./src --analysis --verbosity detailed");
        Console.WriteLine("  BuildValidator ./src --metrics-only --complexity-threshold 15");
        Console.WriteLine("  BuildValidator ./src --analysis --format csv --output results.csv");
        Console.WriteLine("  BuildValidator ./src --analysis --format sarif --output results.sarif");
    }
}
