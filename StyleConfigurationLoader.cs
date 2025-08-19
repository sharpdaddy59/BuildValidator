using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildValidator;

/// <summary>
/// Handles loading and saving style configuration files.
/// </summary>
public static class StyleConfigurationLoader
{
    private const string DefaultConfigFileName = ".buildvalidator.json";
    private const string LegacyConfigFileName = "buildvalidator.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Loads configuration from the default location or returns default configuration.
    /// </summary>
    /// <param name="directoryPath">The directory to search for configuration files.</param>
    /// <returns>The loaded configuration or default configuration if no file found.</returns>
    public static StyleConfiguration LoadConfiguration(string? directoryPath = null)
    {
        directoryPath ??= Environment.CurrentDirectory;

        // Try to find configuration file in current directory and parent directories
        var configPath = FindConfigurationFile(directoryPath);
        
        if (configPath != null)
        {
            return LoadConfigurationFromFile(configPath);
        }

        return StyleConfiguration.Default;
    }

    /// <summary>
    /// Loads configuration from a specific file path.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>The loaded configuration.</returns>
    public static StyleConfiguration LoadConfigurationFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<StyleConfiguration>(json, JsonOptions);
            
            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize configuration from: {filePath}");
            }

            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in configuration file '{filePath}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading configuration from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves configuration to a file.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    /// <param name="filePath">The path to save the configuration to.</param>
    public static void SaveConfiguration(StyleConfiguration config, string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error saving configuration to '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a default configuration file in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory to create the configuration file in.</param>
    /// <param name="configurationType">The type of configuration to create.</param>
    /// <returns>The path to the created configuration file.</returns>
    public static string CreateDefaultConfiguration(string directoryPath, ConfigurationType configurationType = ConfigurationType.Default)
    {
        var configPath = Path.Combine(directoryPath, DefaultConfigFileName);
        
        var config = configurationType switch
        {
            ConfigurationType.Enterprise => StyleConfiguration.CreateEnterpriseConfiguration(),
            ConfigurationType.Relaxed => StyleConfiguration.CreateRelaxedConfiguration(),
            ConfigurationType.DocumentationFocused => StyleConfiguration.CreateDocumentationFocusedConfiguration(),
            _ => StyleConfiguration.Default
        };

        SaveConfiguration(config, configPath);
        return configPath;
    }

    /// <summary>
    /// Searches for a configuration file starting from the specified directory and moving up the directory tree.
    /// </summary>
    /// <param name="startDirectory">The directory to start searching from.</param>
    /// <returns>The path to the configuration file, or null if not found.</returns>
    private static string? FindConfigurationFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            // Check for .buildvalidator.json (preferred)
            var configPath = Path.Combine(directory.FullName, DefaultConfigFileName);
            if (File.Exists(configPath))
                return configPath;

            // Check for legacy buildvalidator.json
            var legacyConfigPath = Path.Combine(directory.FullName, LegacyConfigFileName);
            if (File.Exists(legacyConfigPath))
                return legacyConfigPath;

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Validates a configuration and returns any validation errors.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <returns>A list of validation error messages.</returns>
    public static List<string> ValidateConfiguration(StyleConfiguration config)
    {
        var errors = new List<string>();

        // Check for conflicting settings
        if (config.EnabledRules.Any() && config.DisabledRules.Any())
        {
            var conflicts = config.EnabledRules.Intersect(config.DisabledRules).ToList();
            if (conflicts.Any())
            {
                errors.Add($"Rules cannot be both enabled and disabled: {string.Join(", ", conflicts)}");
            }
        }

        // Validate severity overrides
        var validSeverities = Enum.GetValues<StyleSeverity>().ToHashSet();
        foreach (var (ruleId, severity) in config.SeverityOverrides)
        {
            if (!validSeverities.Contains(severity))
            {
                errors.Add($"Invalid severity '{severity}' for rule '{ruleId}'");
            }
        }

        // Validate rule parameters
        foreach (var (ruleId, parameters) in config.RuleParameters)
        {
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                errors.Add("Rule ID cannot be empty in rule parameters");
            }
        }

        // Validate exclude patterns
        foreach (var pattern in config.ExcludePatterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                errors.Add("Exclude pattern cannot be empty");
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets information about a configuration file.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>Configuration file information.</returns>
    public static ConfigurationInfo GetConfigurationInfo(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var fileInfo = new FileInfo(filePath);
        var config = LoadConfigurationFromFile(filePath);
        var validationErrors = ValidateConfiguration(config);

        return new ConfigurationInfo
        {
            FilePath = filePath,
            LastModified = fileInfo.LastWriteTime,
            FileSize = fileInfo.Length,
            IsValid = !validationErrors.Any(),
            ValidationErrors = validationErrors,
            EnabledRuleCount = GetEnabledRuleCount(config),
            Configuration = config
        };
    }

    /// <summary>
    /// Estimates the number of enabled rules based on configuration.
    /// </summary>
    private static int GetEnabledRuleCount(StyleConfiguration config)
    {
        // This is an estimate based on known rule categories
        var totalRules = 0;

        if (config.EnableDocumentationRules) totalRules += 5; // DOC001-DOC005
        if (config.EnableEncapsulationRules) totalRules += 2; // ENC001-ENC002  
        if (config.EnableAccessibilityRules) totalRules += 2; // ACC001-ACC002
        if (config.EnableOrganizationRules) totalRules += 7; // USG001-USG003, FIL001-FIL002, ORG001-ORG003

        // Subtract disabled rules
        totalRules -= config.DisabledRules.Count;

        // If explicit enabled rules are specified, use that count instead
        if (config.EnabledRules.Any())
        {
            totalRules = config.EnabledRules.Count;
        }

        return Math.Max(0, totalRules);
    }
}

/// <summary>
/// Types of predefined configurations.
/// </summary>
public enum ConfigurationType
{
    Default,
    Enterprise,
    Relaxed,
    DocumentationFocused
}

/// <summary>
/// Information about a configuration file.
/// </summary>
public record ConfigurationInfo
{
    public required string FilePath { get; init; }
    public required DateTime LastModified { get; init; }
    public required long FileSize { get; init; }
    public required bool IsValid { get; init; }
    public required List<string> ValidationErrors { get; init; }
    public required int EnabledRuleCount { get; init; }
    public required StyleConfiguration Configuration { get; init; }
}