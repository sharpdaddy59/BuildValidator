using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BuildValidator;

/// <summary>
/// Provides configurable semantic analysis of C# code using Roslyn semantic models.
/// </summary>
public static class SemanticAnalyzer
{
    /// <summary>
    /// Analyzes semantic issues in the given syntax tree based on configuration.
    /// </summary>
    public static SemanticIssue[] AnalyzeSemantics(
        SyntaxNode root, 
        SemanticModel semanticModel, 
        string? filePath = null,
        StyleConfiguration? config = null)
    {
        config ??= StyleConfiguration.Default;
        var issues = new List<SemanticIssue>();

        // Check if file should be analyzed
        if (!string.IsNullOrEmpty(filePath) && !config.ShouldAnalyzeFile(filePath))
        {
            return Array.Empty<SemanticIssue>();
        }

        // Only analyze if semantic rules are enabled
        if (!config.EnableSemanticRules)
        {
            return Array.Empty<SemanticIssue>();
        }

        // Unused imports detection
        if (config.EnableUnusedImportDetection)
        {
            issues.AddRange(AnalyzeUnusedImports(root, semanticModel, filePath, config));
        }

        // Null reference detection
        if (config.EnableNullReferenceDetection)
        {
            issues.AddRange(AnalyzeNullReferences(root, semanticModel, filePath, config));
        }

        // Type analysis
        if (config.EnableTypeAnalysis)
        {
            issues.AddRange(AnalyzeTypeUsage(root, semanticModel, filePath, config));
        }

        // Code flow analysis
        if (config.EnableCodeFlowAnalysis)
        {
            issues.AddRange(AnalyzeCodeFlow(root, semanticModel, filePath, config));
        }

        // Apply rule-level filtering and severity overrides
        return FilterAndAdjustSemanticIssues(issues, config).ToArray();
    }

    /// <summary>
    /// Analyzes unused using statements.
    /// </summary>
    private static IEnumerable<SemanticIssue> AnalyzeUnusedImports(
        SyntaxNode root, 
        SemanticModel semanticModel, 
        string? filePath,
        StyleConfiguration config)
    {
        var issues = new List<SemanticIssue>();
        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

        foreach (var usingDirective in usings)
        {
            var namespaceName = usingDirective.Name?.ToString();
            if (!string.IsNullOrEmpty(namespaceName))
            {
                // Simple heuristic: check if namespace is used in the source text
                var sourceText = root.ToString();
                var namespaceShortName = namespaceName.Split('.').Last();
                
                // Skip if this appears to be unused (basic detection)
                if (!sourceText.Contains(namespaceShortName) || 
                    sourceText.Count(namespaceShortName.Contains) <= 1)
                {
                    var lineSpan = usingDirective.GetLocation().GetLineSpan();
                    issues.Add(new SemanticIssue
                    {
                        Category = "Unused Imports",
                        Message = $"Unused using directive: {namespaceName}",
                        Recommendation = "Remove unused using statement to improve compilation performance",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Info,
                        RuleId = "SEM001",
                        CodeSnippet = usingDirective.ToString().Trim(),
                        FilePath = filePath
                    });
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Analyzes potential null reference issues.
    /// </summary>
    private static IEnumerable<SemanticIssue> AnalyzeNullReferences(
        SyntaxNode root, 
        SemanticModel semanticModel, 
        string? filePath,
        StyleConfiguration config)
    {
        var issues = new List<SemanticIssue>();

        // Analyze member access expressions for potential null references
        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
            if (typeInfo.Type != null)
            {
                var typeName = typeInfo.Type.ToDisplayString();
                
                // Check for nullable types or potential null references
                if (typeName.EndsWith("?") || typeName.Contains("null"))
                {
                    var lineSpan = memberAccess.GetLocation().GetLineSpan();
                    issues.Add(new SemanticIssue
                    {
                        Category = "Null References",
                        Message = $"Potential null reference: {memberAccess.Expression}",
                        Recommendation = "Add null check before accessing member or use null-conditional operator (?.)",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "SEM010",
                        CodeSnippet = memberAccess.ToString(),
                        FilePath = filePath
                    });
                }
            }
        }

        // Analyze method invocations for null reference potential
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
                if (typeInfo.Type != null)
                {
                    var typeName = typeInfo.Type.ToDisplayString();
                    
                    if (typeName.EndsWith("?") || typeName.Contains("null"))
                    {
                        var lineSpan = invocation.GetLocation().GetLineSpan();
                        issues.Add(new SemanticIssue
                        {
                            Category = "Null References",
                            Message = $"Potential null reference in method call: {memberAccess.Expression}",
                            Recommendation = "Add null check before method invocation",
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            Severity = StyleSeverity.Warning,
                            RuleId = "SEM011",
                            CodeSnippet = invocation.ToString(),
                            FilePath = filePath
                        });
                    }
                }
            }
        }

        // SEM012: Array/collection access without null checks
        foreach (var elementAccess in root.DescendantNodes().OfType<ElementAccessExpressionSyntax>())
        {
            var typeInfo = semanticModel.GetTypeInfo(elementAccess.Expression);
            if (typeInfo.Type != null)
            {
                var typeName = typeInfo.Type.ToDisplayString();
                if (typeName.EndsWith("?") || typeName.Contains("null") || 
                    typeName.Contains("[]") || typeName.Contains("List") || typeName.Contains("Array"))
                {
                    var lineSpan = elementAccess.GetLocation().GetLineSpan();
                    issues.Add(new SemanticIssue
                    {
                        Category = "Null References",
                        Message = $"Potential null reference on array/collection access: {elementAccess.Expression}",
                        Recommendation = "Add null check before array/collection access or use null-conditional operator (?[])",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Severity = StyleSeverity.Warning,
                        RuleId = "SEM012",
                        CodeSnippet = elementAccess.ToString(),
                        FilePath = filePath
                    });
                }
            }
        }

        // SEM013: Nullable types used without null checks
        foreach (var variable in root.DescendantNodes().OfType<VariableDeclarationSyntax>())
        {
            if (variable.Type is NullableTypeSyntax nullableType)
            {
                var variableName = variable.Variables.FirstOrDefault()?.Identifier.ValueText;
                if (!string.IsNullOrEmpty(variableName))
                {
                    // Look for usage without null checks in the same scope
                    var parentMethod = variable.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                    if (parentMethod != null)
                    {
                        var usages = parentMethod.DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Where(id => id.Identifier.ValueText == variableName);

                        foreach (var usage in usages)
                        {
                            // Check if usage is in a null-conditional context
                            var parent = usage.Parent;
                            bool hasNullCheck = false;
                            
                            // Simple heuristic: check if it's used with ?. or within an if statement checking for null
                            while (parent != null)
                            {
                                if (parent is ConditionalAccessExpressionSyntax ||
                                    (parent is IfStatementSyntax ifStmt && 
                                     ifStmt.Condition.ToString().Contains($"{variableName} != null")))
                                {
                                    hasNullCheck = true;
                                    break;
                                }
                                parent = parent.Parent;
                            }

                            if (!hasNullCheck && usage.Parent is MemberAccessExpressionSyntax)
                            {
                                var lineSpan = usage.GetLocation().GetLineSpan();
                                issues.Add(new SemanticIssue
                                {
                                    Category = "Null References",
                                    Message = $"Nullable variable '{variableName}' used without null check",
                                    Recommendation = $"Add null check: if ({variableName} != null) or use null-conditional operator: {variableName}?.",
                                    Line = lineSpan.StartLinePosition.Line + 1,
                                    Column = lineSpan.StartLinePosition.Character + 1,
                                    Severity = StyleSeverity.Warning,
                                    RuleId = "SEM013",
                                    CodeSnippet = usage.Parent.ToString(),
                                    FilePath = filePath
                                });
                            }
                        }
                    }
                }
            }
        }

        // SEM014: Non-nullable reference types assigned null
        foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                var leftTypeInfo = semanticModel.GetTypeInfo(assignment.Left);
                if (leftTypeInfo.Type != null)
                {
                    var typeName = leftTypeInfo.Type.ToDisplayString();
                    
                    // Check if it's a reference type that shouldn't be nullable
                    if (leftTypeInfo.Type.CanBeReferencedByName && 
                        leftTypeInfo.Type.IsReferenceType && 
                        !typeName.EndsWith("?") &&
                        leftTypeInfo.Type.NullableAnnotation != NullableAnnotation.Annotated)
                    {
                        var lineSpan = assignment.GetLocation().GetLineSpan();
                        issues.Add(new SemanticIssue
                        {
                            Category = "Null References",
                            Message = $"Non-nullable reference type '{typeName}' assigned null",
                            Recommendation = "Use nullable reference type (string?) or assign a non-null value",
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            Severity = StyleSeverity.Warning,
                            RuleId = "SEM014",
                            CodeSnippet = assignment.ToString(),
                            FilePath = filePath
                        });
                    }
                }
            }
        }

        // SEM015: Methods that can return null but are not marked nullable
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.ReturnType != null && method.Body != null)
            {
                var returnTypeInfo = semanticModel.GetTypeInfo(method.ReturnType);
                if (returnTypeInfo.Type != null && returnTypeInfo.Type.IsReferenceType)
                {
                    var typeName = returnTypeInfo.Type.ToDisplayString();
                    
                    // Check if method can return null but return type is not nullable
                    if (!typeName.EndsWith("?") && 
                        returnTypeInfo.Type.NullableAnnotation != NullableAnnotation.Annotated)
                    {
                        // Look for return null statements
                        var nullReturns = method.Body.DescendantNodes()
                            .OfType<ReturnStatementSyntax>()
                            .Where(r => r.Expression?.IsKind(SyntaxKind.NullLiteralExpression) == true);

                        if (nullReturns.Any())
                        {
                            var lineSpan = method.Identifier.GetLocation().GetLineSpan();
                            issues.Add(new SemanticIssue
                            {
                                Category = "Null References",
                                Message = $"Method '{method.Identifier.ValueText}' can return null but return type is not nullable",
                                Recommendation = $"Change return type to '{typeName}?' or ensure method never returns null",
                                Line = lineSpan.StartLinePosition.Line + 1,
                                Column = lineSpan.StartLinePosition.Character + 1,
                                Severity = StyleSeverity.Warning,
                                RuleId = "SEM015",
                                CodeSnippet = method.Identifier.ToString(),
                                FilePath = filePath
                            });
                        }
                    }
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Analyzes type usage patterns.
    /// </summary>
    private static IEnumerable<SemanticIssue> AnalyzeTypeUsage(
        SyntaxNode root, 
        SemanticModel semanticModel, 
        string? filePath,
        StyleConfiguration config)
    {
        var issues = new List<SemanticIssue>();

        // Analyze for unnecessary type casts
        foreach (var castExpression in root.DescendantNodes().OfType<CastExpressionSyntax>())
        {
            var expressionType = semanticModel.GetTypeInfo(castExpression.Expression).Type;
            var castType = semanticModel.GetTypeInfo(castExpression.Type).Type;

            if (expressionType != null && castType != null && 
                SymbolEqualityComparer.Default.Equals(expressionType, castType))
            {
                var lineSpan = castExpression.GetLocation().GetLineSpan();
                issues.Add(new SemanticIssue
                {
                    Category = "Type Analysis",
                    Message = $"Unnecessary cast to {castType.Name}",
                    Recommendation = "Remove unnecessary type cast",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Severity = StyleSeverity.Info,
                    RuleId = "SEM030",
                    CodeSnippet = castExpression.ToString(),
                    FilePath = filePath
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Analyzes code flow patterns.
    /// </summary>
    private static IEnumerable<SemanticIssue> AnalyzeCodeFlow(
        SyntaxNode root, 
        SemanticModel semanticModel, 
        string? filePath,
        StyleConfiguration config)
    {
        var issues = new List<SemanticIssue>();

        // Analyze for unreachable code after return statements
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.Body != null)
            {
                var statements = method.Body.Statements;
                for (int i = 0; i < statements.Count - 1; i++)
                {
                    if (statements[i] is ReturnStatementSyntax)
                    {
                        var nextStatement = statements[i + 1];
                        var lineSpan = nextStatement.GetLocation().GetLineSpan();
                        
                        issues.Add(new SemanticIssue
                        {
                            Category = "Code Flow",
                            Message = "Unreachable code after return statement",
                            Recommendation = "Remove unreachable code or restructure control flow",
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            Severity = StyleSeverity.Warning,
                            RuleId = "SEM020",
                            CodeSnippet = nextStatement.ToString().Trim(),
                            FilePath = filePath
                        });
                    }
                }
            }
        }

        return issues;
    }

    /// <summary>
    /// Filters semantic issues based on configuration rules and adjusts their severity.
    /// </summary>
    private static IEnumerable<SemanticIssue> FilterAndAdjustSemanticIssues(
        IEnumerable<SemanticIssue> issues, 
        StyleConfiguration config)
    {
        return issues
            .Where(issue => config.IsRuleEnabled(issue.RuleId, issue.Category))
            .Select(issue => 
            {
                var effectiveSeverity = config.GetEffectiveSeverity(issue.RuleId, issue.Severity);
                
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
}