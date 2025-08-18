namespace BuildValidator;

public record CommandLineOptions
{
    public string Directory { get; init; } = string.Empty;
    public int ParallelCount { get; init; } = Environment.ProcessorCount;
    public string Configuration { get; init; } = "Debug";
    public string Verbosity { get; init; } = "normal";
    public bool IncludeWarnings { get; init; } = false;
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
            IncludeWarnings = false
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
        Console.WriteLine("BuildValidator - Validates C# projects can build successfully using MSBuild APIs");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  BuildValidator <directory> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  directory           The directory path to search for C# projects");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --parallel, -p      Number of parallel builds (default: processor count)");
        Console.WriteLine("  --config, -c        Build configuration: Debug or Release (default: Debug)");
        Console.WriteLine("  --verbosity, -v     Output verbosity: minimal, normal, or detailed (default: normal)");
        Console.WriteLine("  --warnings, -w      Include warnings in output (default: false)");
        Console.WriteLine("  --help, -h          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  BuildValidator ./src");
        Console.WriteLine("  BuildValidator ./src --config Release --parallel 4");
        Console.WriteLine("  BuildValidator ./src --verbosity detailed --warnings");
    }
}