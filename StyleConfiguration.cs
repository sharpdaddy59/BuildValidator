using System.Text.Json.Serialization;

namespace BuildValidator;

/// <summary>
/// Configuration for style validation rules, allowing teams to customize which rules are enforced
/// and their severity levels.
/// </summary>
public class StyleConfiguration
{
    /// <summary>
    /// Gets the default configuration with commonly recommended settings.
    /// </summary>
    public static StyleConfiguration Default => new StyleConfiguration();

    /// <summary>
    /// Gets or sets whether documentation rules are enabled.
    /// </summary>
    public bool EnableDocumentationRules { get; set; } = true;

    /// <summary>
    /// Gets or sets whether encapsulation rules are enabled.
    /// </summary>
    public bool EnableEncapsulationRules { get; set; } = true;

    /// <summary>
    /// Gets or sets whether accessibility rules are enabled.
    /// </summary>
    public bool EnableAccessibilityRules { get; set; } = true;

    /// <summary>
    /// Gets or sets whether code organization rules are enabled.
    /// </summary>
    public bool EnableOrganizationRules { get; set; } = true;

    /// <summary>
    /// Gets or sets the set of rule IDs that are explicitly disabled.
    /// Rules in this set will not be executed even if their category is enabled.
    /// </summary>
    public HashSet<string> DisabledRules { get; set; } = new();

    /// <summary>
    /// Gets or sets the set of rule IDs that are explicitly enabled.
    /// If this set is not empty, only rules in this set will be executed.
    /// </summary>
    public HashSet<string> EnabledRules { get; set; } = new();

    /// <summary>
    /// Gets or sets severity overrides for specific rules.
    /// Allows teams to make warnings into errors or reduce severity of certain rules.
    /// </summary>
    public Dictionary<string, StyleSeverity> SeverityOverrides { get; set; } = new();

    /// <summary>
    /// Gets or sets rule-specific parameters for customizing rule behavior.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> RuleParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum severity level to report.
    /// Rules with severity below this level will be filtered out.
    /// </summary>
    public StyleSeverity MinimumSeverity { get; set; } = StyleSeverity.Info;

    /// <summary>
    /// Gets or sets whether to treat warnings as errors.
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets file patterns to exclude from style analysis.
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = new()
    {
        "**/bin/**",
        "**/obj/**",
        "**/Properties/**",
        "**/*.Designer.cs",
        "**/*.g.cs",
        "**/*.generated.cs"
    };

    /// <summary>
    /// Gets or sets whether generated code should be analyzed.
    /// </summary>
    public bool AnalyzeGeneratedCode { get; set; } = false;

    /// <summary>
    /// Checks whether a rule is enabled based on the configuration.
    /// </summary>
    /// <param name="ruleId">The rule ID to check.</param>
    /// <param name="category">The rule category.</param>
    /// <returns>True if the rule should be executed.</returns>
    public bool IsRuleEnabled(string ruleId, string category)
    {
        // If explicit enabled rules are specified, only those rules are enabled
        if (EnabledRules.Any() && !EnabledRules.Contains(ruleId))
            return false;

        // Check if rule is explicitly disabled
        if (DisabledRules.Contains(ruleId))
            return false;

        // Check category-level settings
        return category switch
        {
            "Documentation" => EnableDocumentationRules,
            "Encapsulation" => EnableEncapsulationRules,
            "Accessibility" => EnableAccessibilityRules,
            "Code Organization" => EnableOrganizationRules,
            _ => true // Unknown categories are enabled by default
        };
    }

    /// <summary>
    /// Gets the effective severity for a rule, considering overrides.
    /// </summary>
    /// <param name="ruleId">The rule ID.</param>
    /// <param name="defaultSeverity">The default severity of the rule.</param>
    /// <returns>The effective severity to use.</returns>
    public StyleSeverity GetEffectiveSeverity(string ruleId, StyleSeverity defaultSeverity)
    {
        if (SeverityOverrides.TryGetValue(ruleId, out var overrideSeverity))
            return overrideSeverity;

        if (TreatWarningsAsErrors && defaultSeverity == StyleSeverity.Warning)
            return StyleSeverity.Error;

        return defaultSeverity;
    }

    /// <summary>
    /// Checks whether an issue should be reported based on minimum severity.
    /// </summary>
    /// <param name="severity">The issue severity.</param>
    /// <returns>True if the issue should be reported.</returns>
    public bool ShouldReportIssue(StyleSeverity severity)
    {
        return severity >= MinimumSeverity;
    }

    /// <summary>
    /// Gets a parameter value for a specific rule.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="ruleId">The rule ID.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="defaultValue">The default value if parameter is not found.</param>
    /// <returns>The parameter value or default value.</returns>
    public T GetRuleParameter<T>(string ruleId, string parameterName, T defaultValue)
    {
        if (RuleParameters.TryGetValue(ruleId, out var ruleParams) &&
            ruleParams.TryGetValue(parameterName, out var value))
        {
            try
            {
                if (value is T directValue)
                    return directValue;

                // Try to convert from JSON element or other types
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // If conversion fails, return default value
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Checks whether a file should be analyzed based on exclude patterns.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file should be analyzed.</returns>
    public bool ShouldAnalyzeFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        // Check if file matches any exclude pattern
        foreach (var pattern in ExcludePatterns)
        {
            if (MatchesPattern(filePath, pattern))
                return false;
        }

        // Check for generated code patterns if disabled
        if (!AnalyzeGeneratedCode)
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName.Contains(".Designer.") ||
                fileName.Contains(".g.") ||
                fileName.Contains(".generated.") ||
                fileName.EndsWith(".AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesPattern(string filePath, string pattern)
    {
        // Simple glob pattern matching
        var normalizedPath = filePath.Replace('\\', '/');
        var normalizedPattern = pattern.Replace('\\', '/');

        // Handle ** wildcard (matches any number of directories)
        if (normalizedPattern.Contains("**/"))
        {
            var parts = normalizedPattern.Split("**/", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return true;

            if (parts.Length == 1)
            {
                // Pattern like "**/filename" or "dirname/**"
                var part = parts[0];
                if (normalizedPattern.StartsWith("**/"))
                    return normalizedPath.EndsWith(part.TrimStart('/'));
                else
                    return normalizedPath.StartsWith(part.TrimEnd('/'));
            }

            // Pattern like "start/**/end"
            return normalizedPath.StartsWith(parts[0].TrimEnd('/')) &&
                   normalizedPath.EndsWith(parts[1].TrimStart('/'));
        }

        // Handle simple * wildcard
        if (normalizedPattern.Contains('*'))
        {
            var regexPattern = normalizedPattern.Replace("*", ".*");
            return System.Text.RegularExpressions.Regex.IsMatch(normalizedPath, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        // Exact match
        return normalizedPath.Equals(normalizedPattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a configuration for enterprise teams with strict rules.
    /// </summary>
    public static StyleConfiguration CreateEnterpriseConfiguration()
    {
        return new StyleConfiguration
        {
            EnableDocumentationRules = true,
            EnableEncapsulationRules = true,
            EnableAccessibilityRules = true,
            EnableOrganizationRules = true,
            TreatWarningsAsErrors = true,
            MinimumSeverity = StyleSeverity.Info,
            SeverityOverrides = new Dictionary<string, StyleSeverity>
            {
                ["DOC001"] = StyleSeverity.Error, // Public classes must have documentation
                ["DOC002"] = StyleSeverity.Error, // Public methods must have documentation
                ["ENC001"] = StyleSeverity.Error, // Public fields are not allowed
                ["USG001"] = StyleSeverity.Warning, // Using statement order
                ["ORG003"] = StyleSeverity.Warning  // Member accessibility order
            }
        };
    }

    /// <summary>
    /// Creates a configuration for teams that prefer minimal style checking.
    /// </summary>
    public static StyleConfiguration CreateRelaxedConfiguration()
    {
        return new StyleConfiguration
        {
            EnableDocumentationRules = false,
            EnableEncapsulationRules = true,
            EnableAccessibilityRules = false,
            EnableOrganizationRules = false,
            MinimumSeverity = StyleSeverity.Warning,
            DisabledRules = new HashSet<string>
            {
                "USG001", // Don't enforce using statement order
                "USG002", // Don't require blank lines between using groups
                "ORG001", "ORG002", "ORG003" // Don't enforce member ordering
            }
        };
    }

    /// <summary>
    /// Creates a configuration focused on documentation and API design.
    /// </summary>
    public static StyleConfiguration CreateDocumentationFocusedConfiguration()
    {
        return new StyleConfiguration
        {
            EnableDocumentationRules = true,
            EnableEncapsulationRules = true,
            EnableAccessibilityRules = true,
            EnableOrganizationRules = false,
            SeverityOverrides = new Dictionary<string, StyleSeverity>
            {
                ["DOC001"] = StyleSeverity.Error,
                ["DOC002"] = StyleSeverity.Error,
                ["DOC003"] = StyleSeverity.Warning,
                ["DOC004"] = StyleSeverity.Warning,
                ["DOC005"] = StyleSeverity.Error
            }
        };
    }
}