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

public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            Environment.Exit(1);
        }

        // Check for help first
        if (args[0] == "--help" || args[0] == "-h")
        {
            ShowHelp();
            Environment.Exit(0);
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
                    Console.Error.WriteLine("Error: --parallel requires a numeric value");
                    Environment.Exit(1);
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
                        Console.Error.WriteLine("Error: Configuration must be either 'Debug' or 'Release'");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.Error.WriteLine("Error: --config requires a value");
                    Environment.Exit(1);
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
                        Console.Error.WriteLine("Error: Verbosity must be one of: minimal, normal, detailed");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.Error.WriteLine("Error: --verbosity requires a value");
                    Environment.Exit(1);
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
                    Console.Error.WriteLine("Error: --complexity-threshold requires a numeric value");
                    Environment.Exit(1);
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
                    Console.Error.WriteLine("Error: --maintainability-threshold requires a numeric value");
                    Environment.Exit(1);
                }
            }
            else if (arg == "--format")
            {
                if (i + 1 < args.Length)
                {
                    string format = args[i + 1];
                    if (new[] { "console", "json", "markdown" }.Contains(format.ToLowerInvariant()))
                    {
                        options = options with { OutputFormat = format };
                        i++;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Format must be one of: console, json, markdown");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.Error.WriteLine("Error: --format requires a value");
                    Environment.Exit(1);
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
                    Console.Error.WriteLine("Error: --output requires a file path");
                    Environment.Exit(1);
                }
            }
            else if (arg == "--help" || arg == "-h")
            {
                ShowHelp();
                Environment.Exit(0);
            }
            else
            {
                Console.Error.WriteLine($"Error: Unknown argument '{arg}'");
                Environment.Exit(1);
            }
        }

        // Validate directory exists
        if (!Directory.Exists(options.Directory))
        {
            Console.Error.WriteLine($"Error: Directory '{options.Directory}' does not exist");
            Environment.Exit(1);
        }

        // Validate parallel count
        if (options.ParallelCount <= 0)
        {
            Console.Error.WriteLine("Error: Parallel count must be greater than 0");
            Environment.Exit(1);
        }

        return options;
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
        Console.WriteLine("  --format <format>   Output format: console, json, markdown (default: console)");
        Console.WriteLine("  --output <file>     Save results to file (format auto-detected from extension)");
        Console.WriteLine("  --help, -h          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  BuildValidator ./src");
        Console.WriteLine("  BuildValidator ./src --config Release --parallel 4");
        Console.WriteLine("  BuildValidator ./src --analysis --verbosity detailed");
        Console.WriteLine("  BuildValidator ./src --metrics-only --complexity-threshold 15");
        Console.WriteLine("  BuildValidator ./src --analysis --format json --output results.json");
    }
}