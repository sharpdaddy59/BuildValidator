using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BuildValidator;

public enum StyleSeverity
{
    Info,
    Warning,
    Error
}

public record StyleIssue
{
    public required string Category { get; init; }
    public required string Message { get; init; }
    public required string Recommendation { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required StyleSeverity Severity { get; init; }
    public string? CodeSnippet { get; init; }
    public string? RuleId { get; init; }
}

public record SemanticIssue
{
    public required string Category { get; init; }
    public required string Message { get; init; }
    public required string Recommendation { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required StyleSeverity Severity { get; init; }
    public string? CodeSnippet { get; init; }
    public required string RuleId { get; init; }
    public string? FilePath { get; init; }
}

public record StyleAnalysis
{
    public required StyleIssue[] AccessibilityIssues { get; init; }
    public required StyleIssue[] DocumentationIssues { get; init; }
    public required StyleIssue[] EncapsulationIssues { get; init; }
    public required StyleIssue[] OrganizationIssues { get; init; }
    public required StyleMetrics Metrics { get; init; }
}

public record StyleMetrics
{
    public required int TotalStyleIssues { get; init; }
    public required int AccessibilityViolations { get; init; }
    public required int DocumentationViolations { get; init; }
    public required int EncapsulationViolations { get; init; }
    public required int OrganizationViolations { get; init; }
    public required int PublicMembersWithoutDocs { get; init; }
    public required int PublicFieldsCount { get; init; }
    public required int PrivateFieldsExposedCount { get; init; }
}

public class StyleValidationAnalyzer
{
    public static StyleAnalysis AnalyzeStyle(SyntaxNode root, SemanticModel? semanticModel = null, string? filePath = null, StyleConfiguration? config = null)
    {
        config ??= StyleConfiguration.Default;

        // Check if file should be analyzed
        if (!string.IsNullOrEmpty(filePath) && !config.ShouldAnalyzeFile(filePath))
        {
            return new StyleAnalysis
            {
                DocumentationIssues = Array.Empty<StyleIssue>(),
                EncapsulationIssues = Array.Empty<StyleIssue>(),
                AccessibilityIssues = Array.Empty<StyleIssue>(),
                OrganizationIssues = Array.Empty<StyleIssue>(),
                Metrics = new StyleMetrics
                {
                    TotalStyleIssues = 0,
                    AccessibilityViolations = 0,
                    DocumentationViolations = 0,
                    EncapsulationViolations = 0,
                    OrganizationViolations = 0,
                    PublicMembersWithoutDocs = 0,
                    PublicFieldsCount = 0,
                    PrivateFieldsExposedCount = 0
                }
            };
        }

        var documentationIssues = config.EnableDocumentationRules ? AnalyzeDocumentation(root, config).ToArray() : Array.Empty<StyleIssue>();
        var encapsulationIssues = config.EnableEncapsulationRules ? AnalyzeEncapsulation(root, config).ToArray() : Array.Empty<StyleIssue>();
        var accessibilityIssues = config.EnableAccessibilityRules ? AnalyzeAccessibility(root, config).ToArray() : Array.Empty<StyleIssue>();
        var organizationIssues = config.EnableOrganizationRules ? AnalyzeCodeOrganization(root, filePath, config).ToArray() : Array.Empty<StyleIssue>();

        // Apply rule-level filtering and severity overrides
        documentationIssues = FilterAndAdjustIssues(documentationIssues, config).ToArray();
        encapsulationIssues = FilterAndAdjustIssues(encapsulationIssues, config).ToArray();
        accessibilityIssues = FilterAndAdjustIssues(accessibilityIssues, config).ToArray();
        organizationIssues = FilterAndAdjustIssues(organizationIssues, config).ToArray();

        var metrics = CalculateStyleMetrics(documentationIssues, encapsulationIssues, accessibilityIssues, organizationIssues, root);

        return new StyleAnalysis
        {
            DocumentationIssues = documentationIssues,
            EncapsulationIssues = encapsulationIssues,
            AccessibilityIssues = accessibilityIssues,
            OrganizationIssues = organizationIssues,
            Metrics = metrics
        };
    }

    private static IEnumerable<StyleIssue> AnalyzeDocumentation(SyntaxNode root, StyleConfiguration config)
    {
        var issues = new List<StyleIssue>();

        // Check public classes for XML documentation
        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (HasPublicModifier(classDeclaration.Modifiers))
            {
                if (!HasXmlDocumentation(classDeclaration))
                {
                    var lineSpan = classDeclaration.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Documentation",
                        Message = $"Public class '{classDeclaration.Identifier.ValueText}' lacks XML documentation",
                        Recommendation = "Add /// <summary> XML documentation to describe the class purpose",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "DOC001",
                        CodeSnippet = classDeclaration.Identifier.ValueText
                    });
                }
            }
        }

        // Check public methods for XML documentation
        foreach (var methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (HasPublicModifier(methodDeclaration.Modifiers))
            {
                if (!HasXmlDocumentation(methodDeclaration))
                {
                    var lineSpan = methodDeclaration.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Documentation",
                        Message = $"Public method '{methodDeclaration.Identifier.ValueText}' lacks XML documentation",
                        Recommendation = "Add /// <summary> XML documentation and document parameters/return values",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "DOC002",
                        CodeSnippet = methodDeclaration.Identifier.ValueText
                    });
                }
                else
                {
                    // Check if parameters are documented
                    var hasParams = methodDeclaration.ParameterList.Parameters.Any();
                    var hasReturn = !methodDeclaration.ReturnType.ToString().Equals("void", StringComparison.OrdinalIgnoreCase);
                    
                    if (hasParams || hasReturn)
                    {
                        var docComment = GetDocumentationComment(methodDeclaration);
                        if (docComment != null)
                        {
                            if (hasParams && !ContainsParamTags(docComment))
                            {
                                var lineSpan = methodDeclaration.GetLocation().GetLineSpan();
                                issues.Add(new StyleIssue
                                {
                                    Category = "Documentation",
                                    Message = $"Method '{methodDeclaration.Identifier.ValueText}' parameters are not documented",
                                    Recommendation = "Add <param> tags for each parameter in XML documentation",
                                    Line = lineSpan.StartLinePosition.Line + 1,
                                    Column = lineSpan.StartLinePosition.Character + 1,
                                    Severity = StyleSeverity.Info,
                                    RuleId = "DOC003",
                                    CodeSnippet = methodDeclaration.Identifier.ValueText
                                });
                            }

                            if (hasReturn && !ContainsReturnTag(docComment))
                            {
                                var lineSpan = methodDeclaration.GetLocation().GetLineSpan();
                                issues.Add(new StyleIssue
                                {
                                    Category = "Documentation",
                                    Message = $"Method '{methodDeclaration.Identifier.ValueText}' return value is not documented",
                                    Recommendation = "Add <returns> tag to describe the return value",
                                    Line = lineSpan.StartLinePosition.Line + 1,
                                    Column = lineSpan.StartLinePosition.Character + 1,
                                    Severity = StyleSeverity.Info,
                                    RuleId = "DOC004",
                                    CodeSnippet = methodDeclaration.Identifier.ValueText
                                });
                            }
                        }
                    }
                }
            }
        }

        // Check public properties for XML documentation
        foreach (var propertyDeclaration in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (HasPublicModifier(propertyDeclaration.Modifiers))
            {
                if (!HasXmlDocumentation(propertyDeclaration))
                {
                    var lineSpan = propertyDeclaration.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Documentation",
                        Message = $"Public property '{propertyDeclaration.Identifier.ValueText}' lacks XML documentation",
                        Recommendation = "Add /// <summary> XML documentation to describe the property",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "DOC005",
                        CodeSnippet = propertyDeclaration.Identifier.ValueText
                    });
                }
            }
        }

        return issues;
    }

    private static IEnumerable<StyleIssue> AnalyzeEncapsulation(SyntaxNode root, StyleConfiguration config)
    {
        var issues = new List<StyleIssue>();

        // Check for public fields (should be properties instead)
        foreach (var fieldDeclaration in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (HasPublicModifier(fieldDeclaration.Modifiers))
            {
                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    var lineSpan = variable.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Encapsulation",
                        Message = $"Public field '{variable.Identifier.ValueText}' violates encapsulation",
                        Recommendation = "Convert public field to property with appropriate getters/setters",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "ENC001",
                        CodeSnippet = variable.Identifier.ValueText
                    });
                }
            }
        }

        // Check for protected fields in classes (should generally be private)
        foreach (var fieldDeclaration in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (HasProtectedModifier(fieldDeclaration.Modifiers))
            {
                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    var lineSpan = variable.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Encapsulation",
                        Message = $"Protected field '{variable.Identifier.ValueText}' may indicate design issue",
                        Recommendation = "Consider using private field with protected property, or review inheritance design",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ENC002",
                        CodeSnippet = variable.Identifier.ValueText
                    });
                }
            }
        }

        return issues;
    }

    private static IEnumerable<StyleIssue> AnalyzeAccessibility(SyntaxNode root, StyleConfiguration config)
    {
        var issues = new List<StyleIssue>();

        // Check constructor accessibility patterns
        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var constructors = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            
            // If class is public but has no constructors, check if it should have explicit constructor
            if (HasPublicModifier(classDeclaration.Modifiers) && !constructors.Any())
            {
                // Check if class has instance members (suggesting it might need a constructor)
                var hasInstanceMembers = classDeclaration.DescendantNodes()
                    .OfType<MemberDeclarationSyntax>()
                    .Any(m => !HasStaticModifier(m.Modifiers) && 
                              !(m is ConstructorDeclarationSyntax));

                if (hasInstanceMembers)
                {
                    var lineSpan = classDeclaration.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Accessibility",
                        Message = $"Public class '{classDeclaration.Identifier.ValueText}' relies on implicit constructor",
                        Recommendation = "Consider adding explicit constructor with appropriate access modifier",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ACC001",
                        CodeSnippet = classDeclaration.Identifier.ValueText
                    });
                }
            }

            // Check for private constructors in non-static classes (possible utility class)
            if (constructors.Any(c => HasPrivateModifier(c.Modifiers)))
            {
                var hasOnlyStaticMembers = classDeclaration.DescendantNodes()
                    .OfType<MemberDeclarationSyntax>()
                    .Where(m => !(m is ConstructorDeclarationSyntax))
                    .All(m => HasStaticModifier(m.Modifiers));

                if (hasOnlyStaticMembers && !HasStaticModifier(classDeclaration.Modifiers))
                {
                    var lineSpan = classDeclaration.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Accessibility",
                        Message = $"Class '{classDeclaration.Identifier.ValueText}' appears to be utility class but is not static",
                        Recommendation = "Consider making class static if it only contains static members",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ACC002",
                        CodeSnippet = classDeclaration.Identifier.ValueText
                    });
                }
            }
        }

        return issues;
    }

    private static IEnumerable<StyleIssue> AnalyzeCodeOrganization(SyntaxNode root, string? filePath, StyleConfiguration config)
    {
        var issues = new List<StyleIssue>();

        // Using statement organization rules (USG001-USG004)
        issues.AddRange(AnalyzeUsingStatements(root));

        // File organization rules (FIL001-FIL003) - only if we have file path
        if (!string.IsNullOrEmpty(filePath))
        {
            issues.AddRange(AnalyzeFileOrganization(root, filePath));
        }

        // Member organization rules (ORG001-ORG003)
        issues.AddRange(AnalyzeMemberOrganization(root));

        return issues;
    }

    /// <summary>
    /// Filters issues based on configuration rules and adjusts their severity.
    /// </summary>
    private static IEnumerable<StyleIssue> FilterAndAdjustIssues(IEnumerable<StyleIssue> issues, StyleConfiguration config)
    {
        return issues
            .Where(issue => issue.RuleId != null && config.IsRuleEnabled(issue.RuleId, issue.Category))
            .Select(issue => 
            {
                var effectiveSeverity = config.GetEffectiveSeverity(issue.RuleId!, issue.Severity);
                
                // Check if issue meets minimum severity threshold
                if (!config.ShouldReportIssue(effectiveSeverity))
                    return null;
                
                // Return issue with adjusted severity if needed
                return effectiveSeverity != issue.Severity 
                    ? issue with { Severity = effectiveSeverity }
                    : issue;
            })
            .Where(issue => issue != null)!;
    }

    private static IEnumerable<StyleIssue> AnalyzeUsingStatements(SyntaxNode root)
    {
        var issues = new List<StyleIssue>();
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

        if (!usingDirectives.Any()) return issues;

        // USG001: Check using statement ordering (System -> Microsoft -> Third-party -> Project)
        var systemUsings = new List<UsingDirectiveSyntax>();
        var microsoftUsings = new List<UsingDirectiveSyntax>();
        var thirdPartyUsings = new List<UsingDirectiveSyntax>();
        var projectUsings = new List<UsingDirectiveSyntax>();

        foreach (var usingDir in usingDirectives)
        {
            var nameText = usingDir.Name?.ToString() ?? "";
            if (nameText.StartsWith("System"))
                systemUsings.Add(usingDir);
            else if (nameText.StartsWith("Microsoft"))
                microsoftUsings.Add(usingDir);
            else if (char.IsUpper(nameText[0]) && !nameText.Contains('.') || 
                     nameText.Split('.').FirstOrDefault()?.All(char.IsUpper) == true)
                projectUsings.Add(usingDir);
            else
                thirdPartyUsings.Add(usingDir);
        }

        // Check if usings are properly ordered
        var expectedOrder = systemUsings.Concat(microsoftUsings).Concat(thirdPartyUsings).Concat(projectUsings).ToList();
        for (int i = 0; i < usingDirectives.Count; i++)
        {
            if (i < expectedOrder.Count && usingDirectives[i] != expectedOrder[i])
            {
                var lineSpan = usingDirectives[i].GetLocation().GetLineSpan();
                issues.Add(new StyleIssue
                {
                    Category = "Code Organization",
                    Message = "Using statements not properly ordered (System → Microsoft → Third-party → Project)",
                    Recommendation = "Organize using statements: System usings first, then Microsoft, then third-party, then project usings",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Severity = StyleSeverity.Info,
                    RuleId = "USG001",
                    CodeSnippet = usingDirectives[i].ToString().Trim()
                });
                break; // Report once per file
            }
        }

        // USG002: Check for proper grouping (blank lines between groups)
        var allGroups = new[] { systemUsings, microsoftUsings, thirdPartyUsings, projectUsings }
            .Where(g => g.Any()).ToList();

        if (allGroups.Count > 1)
        {
            for (int i = 0; i < allGroups.Count - 1; i++)
            {
                var currentGroup = allGroups[i];
                var nextGroup = allGroups[i + 1];
                
                var lastInCurrent = currentGroup.Last();
                var firstInNext = nextGroup.First();
                
                // Check if there's a blank line between groups (simplified check)
                var currentLine = lastInCurrent.GetLocation().GetLineSpan().EndLinePosition.Line;
                var nextLine = firstInNext.GetLocation().GetLineSpan().StartLinePosition.Line;
                
                if (nextLine - currentLine == 1) // No blank line
                {
                    var lineSpan = firstInNext.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = "Missing blank line between using statement groups",
                        Recommendation = "Add blank lines between different using statement groups for better readability",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "USG002",
                        CodeSnippet = firstInNext.ToString().Trim()
                    });
                }
            }
        }

        // USG003: Check for duplicate using statements
        var usingNames = new HashSet<string>();
        foreach (var usingDir in usingDirectives)
        {
            var nameText = usingDir.Name?.ToString();
            if (!string.IsNullOrEmpty(nameText))
            {
                if (usingNames.Contains(nameText))
                {
                    var lineSpan = usingDir.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = $"Duplicate using statement: {nameText}",
                        Recommendation = "Remove duplicate using statement",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "USG003",
                        CodeSnippet = usingDir.ToString().Trim()
                    });
                }
                else
                {
                    usingNames.Add(nameText);
                }
            }
        }

        return issues;
    }

    private static IEnumerable<StyleIssue> AnalyzeFileOrganization(SyntaxNode root, string filePath)
    {
        var issues = new List<StyleIssue>();
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        // FIL001: File name should match primary public class name
        var publicClasses = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(c => HasPublicModifier(c.Modifiers)).ToList();

        if (publicClasses.Any())
        {
            var primaryClass = publicClasses.First();
            var className = primaryClass.Identifier.ValueText;

            if (!fileName.Equals(className, StringComparison.OrdinalIgnoreCase))
            {
                var lineSpan = primaryClass.GetLocation().GetLineSpan();
                issues.Add(new StyleIssue
                {
                    Category = "Code Organization",
                    Message = $"File name '{fileName}.cs' doesn't match class name '{className}'",
                    Recommendation = $"Rename file to '{className}.cs' or rename class to match file name",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Severity = StyleSeverity.Info,
                    RuleId = "FIL001",
                    CodeSnippet = className
                });
            }
        }

        // FIL002: Multiple unrelated public classes in single file
        if (publicClasses.Count > 1)
        {
            // Consider classes related if they share a common prefix or one is nested
            var unrelatedClasses = publicClasses.Where(c => 
                !publicClasses.Any(other => other != c && 
                    (c.Identifier.ValueText.StartsWith(other.Identifier.ValueText) ||
                     other.Identifier.ValueText.StartsWith(c.Identifier.ValueText)))).ToList();

            if (unrelatedClasses.Count > 1)
            {
                foreach (var cls in unrelatedClasses.Skip(1)) // Skip first one
                {
                    var lineSpan = cls.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = $"Multiple unrelated public classes in single file: '{cls.Identifier.ValueText}'",
                        Recommendation = "Consider moving this class to its own file for better organization",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "FIL002",
                        CodeSnippet = cls.Identifier.ValueText
                    });
                }
            }
        }

        return issues;
    }

    private static IEnumerable<StyleIssue> AnalyzeMemberOrganization(SyntaxNode root)
    {
        var issues = new List<StyleIssue>();

        // ORG001-ORG003: Check member ordering within classes
        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var members = classDeclaration.Members.ToList();
            if (members.Count <= 1) continue;

            var previousMemberType = MemberType.Field;
            var previousWasPublic = false;

            foreach (var member in members)
            {
                var memberType = GetMemberType(member);
                var isPublic = HasPublicModifier(member.Modifiers);

                // ORG001: Fields should come before methods
                if (previousMemberType == MemberType.Method && memberType == MemberType.Field)
                {
                    var lineSpan = member.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = "Fields should be declared before methods",
                        Recommendation = "Move field declarations to the top of the class",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ORG001",
                        CodeSnippet = GetMemberName(member)
                    });
                }

                // ORG002: Constructors should come before other methods
                if (previousMemberType == MemberType.Method && memberType == MemberType.Constructor)
                {
                    var lineSpan = member.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = "Constructors should be declared before other methods",
                        Recommendation = "Move constructor declarations before other methods",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ORG002",
                        CodeSnippet = GetMemberName(member)
                    });
                }

                // ORG003: Public members should come before private members (within same type)
                if (memberType == previousMemberType && !isPublic && previousWasPublic)
                {
                    var lineSpan = member.GetLocation().GetLineSpan();
                    issues.Add(new StyleIssue
                    {
                        Category = "Code Organization",
                        Message = $"Public {memberType.ToString().ToLower()}s should come before private {memberType.ToString().ToLower()}s",
                        Recommendation = $"Group public {memberType.ToString().ToLower()}s together before private ones",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "ORG003",
                        CodeSnippet = GetMemberName(member)
                    });
                }

                previousMemberType = memberType;
                previousWasPublic = isPublic;
            }
        }

        return issues;
    }

    private enum MemberType
    {
        Field,
        Constructor,
        Property,
        Method,
        Event,
        Other
    }

    private static MemberType GetMemberType(MemberDeclarationSyntax member)
    {
        return member switch
        {
            FieldDeclarationSyntax => MemberType.Field,
            ConstructorDeclarationSyntax => MemberType.Constructor,
            PropertyDeclarationSyntax => MemberType.Property,
            MethodDeclarationSyntax => MemberType.Method,
            EventDeclarationSyntax => MemberType.Event,
            _ => MemberType.Other
        };
    }

    private static string GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? "field",
            ConstructorDeclarationSyntax ctor => ctor.Identifier.ValueText,
            PropertyDeclarationSyntax prop => prop.Identifier.ValueText,
            MethodDeclarationSyntax method => method.Identifier.ValueText,
            EventDeclarationSyntax evt => evt.Identifier.ValueText,
            _ => "member"
        };
    }

    private static StyleMetrics CalculateStyleMetrics(
        StyleIssue[] documentationIssues,
        StyleIssue[] encapsulationIssues,
        StyleIssue[] accessibilityIssues,
        StyleIssue[] organizationIssues,
        SyntaxNode root)
    {
        var totalIssues = documentationIssues.Length + encapsulationIssues.Length + accessibilityIssues.Length + organizationIssues.Length;
        
        var publicFields = root.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Where(f => HasPublicModifier(f.Modifiers))
            .SelectMany(f => f.Declaration.Variables)
            .Count();

        var publicMembersWithoutDocs = documentationIssues
            .Count(i => i.RuleId?.StartsWith("DOC") == true);

        return new StyleMetrics
        {
            TotalStyleIssues = totalIssues,
            AccessibilityViolations = accessibilityIssues.Length,
            DocumentationViolations = documentationIssues.Length,
            EncapsulationViolations = encapsulationIssues.Length,
            OrganizationViolations = organizationIssues.Length,
            PublicMembersWithoutDocs = publicMembersWithoutDocs,
            PublicFieldsCount = publicFields,
            PrivateFieldsExposedCount = encapsulationIssues.Count(i => i.RuleId == "ENC001")
        };
    }

    // Helper methods for modifier checking
    private static bool HasPublicModifier(SyntaxTokenList modifiers) =>
        modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

    private static bool HasPrivateModifier(SyntaxTokenList modifiers) =>
        modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

    private static bool HasProtectedModifier(SyntaxTokenList modifiers) =>
        modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));

    private static bool HasStaticModifier(SyntaxTokenList modifiers) =>
        modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

    // Helper methods for documentation checking
    private static bool HasXmlDocumentation(SyntaxNode node)
    {
        var documentationComment = node.GetLeadingTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
                                
        return !documentationComment.IsKind(SyntaxKind.None);
    }

    private static DocumentationCommentTriviaSyntax? GetDocumentationComment(SyntaxNode node)
    {
        var docTrivia = node.GetLeadingTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

        return docTrivia.GetStructure() as DocumentationCommentTriviaSyntax;
    }

    private static bool ContainsParamTags(DocumentationCommentTriviaSyntax docComment)
    {
        return docComment.DescendantNodes()
            .OfType<XmlElementSyntax>()
            .Any(e => e.StartTag.Name.LocalName.ValueText.Equals("param", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsReturnTag(DocumentationCommentTriviaSyntax docComment)
    {
        return docComment.DescendantNodes()
            .OfType<XmlElementSyntax>()
            .Any(e => e.StartTag.Name.LocalName.ValueText.Equals("returns", StringComparison.OrdinalIgnoreCase));
    }
}