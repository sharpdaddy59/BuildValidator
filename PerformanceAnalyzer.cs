using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BuildValidator;

public class PerformanceAnalyzer
{
    public static PerformanceAnalysis AnalyzePerformance(SyntaxNode root, SemanticModel semanticModel)
    {
        var linqIssues = AnalyzeLinqPerformance(root, semanticModel).ToArray();
        var allocationIssues = AnalyzeAllocationPatterns(root, semanticModel).ToArray();
        var asyncIssues = AnalyzeAsyncPatterns(root, semanticModel).ToArray();
        var stringIssues = AnalyzeStringPerformance(root, semanticModel).ToArray();

        var metrics = CalculatePerformanceMetrics(root, linqIssues, allocationIssues, asyncIssues, stringIssues);

        return new PerformanceAnalysis
        {
            LinqPerformanceIssues = linqIssues,
            AllocationIssues = allocationIssues,
            AsyncPerformanceIssues = asyncIssues,
            StringPerformanceIssues = stringIssues,
            Metrics = metrics
        };
    }

    private static IEnumerable<PerformanceIssue> AnalyzeLinqPerformance(SyntaxNode root, SemanticModel semanticModel)
    {
        var issues = new List<PerformanceIssue>();

        // Pattern 1: Multiple enumeration of IEnumerable
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var memberName = memberAccess.Name.Identifier.ValueText;
            
            // Detect Count() followed by Any(), First(), etc. on same variable
            if (IsLinqMethod(memberName))
            {
                var expressionText = memberAccess.Expression.ToString();
                
                // Look for other LINQ operations on the same expression in the same method
                var containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (containingMethod != null)
                {
                    var otherLinqCalls = containingMethod.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv != invocation)
                        .OfType<InvocationExpressionSyntax>()
                        .Where(inv => inv.Expression is MemberAccessExpressionSyntax ma &&
                                     ma.Expression.ToString() == expressionText &&
                                     IsLinqMethod(ma.Name.Identifier.ValueText))
                        .ToArray();

                    if (otherLinqCalls.Any())
                    {
                        var lineSpan = invocation.GetLocation().GetLineSpan();
                        issues.Add(new PerformanceIssue
                        {
                            Message = $"Multiple enumeration detected on '{expressionText}' with {memberName}()",
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            Category = "LINQ Performance",
                            Severity = PerformanceSeverity.Medium,
                            Recommendation = "Consider calling .ToList() or .ToArray() once and reusing the result",
                            CodeSnippet = invocation.ToString()
                        });
                    }
                }
            }

            // Pattern 2: Count() > 0 instead of Any()
            if (memberName == "Count")
            {
                var parent = invocation.Parent;
                if (parent is BinaryExpressionSyntax binary && 
                    (binary.OperatorToken.IsKind(SyntaxKind.GreaterThanToken) ||
                     binary.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken)))
                {
                    var lineSpan = invocation.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "Using Count() > 0 instead of Any()",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "LINQ Performance", 
                        Severity = PerformanceSeverity.Medium,
                        Recommendation = "Replace Count() > 0 with Any() for better performance",
                        CodeSnippet = binary.ToString()
                    });
                }
            }

            // Pattern 3: Where().Count() instead of Count(predicate)
            if (memberName == "Count" && 
                memberAccess.Expression is InvocationExpressionSyntax whereInvocation &&
                whereInvocation.Expression is MemberAccessExpressionSyntax whereMember &&
                whereMember.Name.Identifier.ValueText == "Where")
            {
                var lineSpan = invocation.GetLocation().GetLineSpan();
                issues.Add(new PerformanceIssue
                {
                    Message = "Using Where().Count() instead of Count(predicate)",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Category = "LINQ Performance",
                    Severity = PerformanceSeverity.Low,
                    Recommendation = "Combine Where() and Count() into Count(predicate)",
                    CodeSnippet = invocation.ToString()
                });
            }
        }

        return issues;
    }

    private static IEnumerable<PerformanceIssue> AnalyzeAllocationPatterns(SyntaxNode root, SemanticModel semanticModel)
    {
        var issues = new List<PerformanceIssue>();

        // Pattern 1: Boxing in foreach loops
        foreach (var foreachStatement in root.DescendantNodes().OfType<ForEachStatementSyntax>())
        {
            var typeInfo = semanticModel.GetTypeInfo(foreachStatement.Expression);
            if (typeInfo.Type != null)
            {
                var typeName = typeInfo.Type.ToDisplayString();
                if (typeName.Contains("System.Collections.ArrayList") || 
                    typeName.Contains("System.Collections.Hashtable"))
                {
                    var lineSpan = foreachStatement.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "Boxing/unboxing in non-generic collection iteration",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "Memory Allocation",
                        Severity = PerformanceSeverity.High,
                        Recommendation = "Use generic collections like List<T> or Dictionary<T,U>",
                        CodeSnippet = foreachStatement.ToString()
                    });
                }
            }
        }

        // Pattern 2: Unnecessary array allocations in params
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.ArgumentList.Arguments.Count > 0)
            {
                var lastArg = invocation.ArgumentList.Arguments.Last();
                if (lastArg.Expression is ArrayCreationExpressionSyntax arrayCreation &&
                    arrayCreation.Initializer != null &&
                    arrayCreation.Initializer.Expressions.Count <= 3)
                {
                    var lineSpan = invocation.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "Unnecessary array allocation for params parameter",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "Memory Allocation",
                        Severity = PerformanceSeverity.Low,
                        Recommendation = "Pass individual arguments instead of array for params",
                        CodeSnippet = invocation.ToString()
                    });
                }
            }
        }

        // Pattern 3: Closure allocations in loops
        foreach (var forStatement in root.DescendantNodes().OfType<ForStatementSyntax>().Concat<StatementSyntax>(
                     root.DescendantNodes().OfType<ForEachStatementSyntax>()).Concat(
                     root.DescendantNodes().OfType<WhileStatementSyntax>()))
        {
            var lambdas = forStatement.DescendantNodes().OfType<LambdaExpressionSyntax>();
            foreach (var lambda in lambdas)
            {
                var lineSpan = lambda.GetLocation().GetLineSpan();
                issues.Add(new PerformanceIssue
                {
                    Message = "Lambda expression in loop may cause closure allocation",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Category = "Memory Allocation",
                    Severity = PerformanceSeverity.Medium,
                    Recommendation = "Consider moving lambda outside loop or using static lambda",
                    CodeSnippet = lambda.ToString()
                });
            }
        }

        return issues;
    }

    private static IEnumerable<PerformanceIssue> AnalyzeAsyncPatterns(SyntaxNode root, SemanticModel semanticModel)
    {
        var issues = new List<PerformanceIssue>();

        // Pattern 1: Async void methods (except event handlers)
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)) &&
                method.ReturnType.ToString() == "void")
            {
                // Check if it's an event handler pattern
                var isEventHandler = method.ParameterList.Parameters.Count == 2 &&
                                   method.ParameterList.Parameters[0].Type?.ToString().Contains("sender") == true;

                if (!isEventHandler)
                {
                    var lineSpan = method.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "Async void method detected (should return Task)",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "Async Performance",
                        Severity = PerformanceSeverity.High,
                        Recommendation = "Change return type to Task for proper error handling and awaiting",
                        CodeSnippet = method.Identifier.ToString()
                    });
                }
            }
        }

        // Pattern 2: .Result or .Wait() usage
        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            var memberName = memberAccess.Name.Identifier.ValueText;
            if (memberName == "Result" || memberName == "Wait")
            {
                var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
                if (typeInfo.Type != null && 
                    (typeInfo.Type.ToDisplayString().Contains("Task") || 
                     typeInfo.Type.AllInterfaces.Any(i => i.ToDisplayString().Contains("Task"))))
                {
                    var lineSpan = memberAccess.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = $"Blocking call .{memberName} on Task (potential deadlock)",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "Async Performance",
                        Severity = PerformanceSeverity.High,
                        Recommendation = "Use await instead of blocking calls to prevent deadlocks",
                        CodeSnippet = memberAccess.ToString()
                    });
                }
            }
        }

        // Pattern 3: ConfigureAwait(false) missing in library code
        foreach (var awaitExpression in root.DescendantNodes().OfType<AwaitExpressionSyntax>())
        {
            var expression = awaitExpression.Expression;
            var hasConfigureAwait = false;

            if (expression is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.ValueText == "ConfigureAwait")
            {
                hasConfigureAwait = true;
            }

            if (!hasConfigureAwait)
            {
                // Check if we're in a library context (not Main method, not event handler)
                var containingMethod = awaitExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (containingMethod != null && 
                    containingMethod.Identifier.ValueText != "Main" &&
                    !IsEventHandler(containingMethod))
                {
                    var lineSpan = awaitExpression.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "Missing ConfigureAwait(false) in library code",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "Async Performance",
                        Severity = PerformanceSeverity.Low,
                        Recommendation = "Add .ConfigureAwait(false) in library code to avoid deadlocks",
                        CodeSnippet = awaitExpression.ToString()
                    });
                }
            }
        }

        return issues;
    }

    private static IEnumerable<PerformanceIssue> AnalyzeStringPerformance(SyntaxNode root, SemanticModel semanticModel)
    {
        var issues = new List<PerformanceIssue>();

        // Pattern 1: String concatenation in loops
        foreach (var loopStatement in root.DescendantNodes().OfType<ForStatementSyntax>().Concat<StatementSyntax>(
                     root.DescendantNodes().OfType<ForEachStatementSyntax>()).Concat(
                     root.DescendantNodes().OfType<WhileStatementSyntax>()))
        {
            var concatenations = loopStatement.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(a => a.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken) ||
                           (a.IsKind(SyntaxKind.SimpleAssignmentExpression) && 
                            a.Right is BinaryExpressionSyntax binary && 
                            binary.OperatorToken.IsKind(SyntaxKind.PlusToken)));

            foreach (var concat in concatenations)
            {
                var typeInfo = semanticModel.GetTypeInfo(concat.Left);
                if (typeInfo.Type != null && typeInfo.Type.SpecialType == SpecialType.System_String)
                {
                    var lineSpan = concat.GetLocation().GetLineSpan();
                    issues.Add(new PerformanceIssue
                    {
                        Message = "String concatenation in loop",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Category = "String Performance",
                        Severity = PerformanceSeverity.Medium,
                        Recommendation = "Use StringBuilder for multiple string concatenations",
                        CodeSnippet = concat.ToString()
                    });
                }
            }
        }

        // Pattern 2: String.Format vs interpolation
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression.ToString() == "string" &&
                memberAccess.Name.Identifier.ValueText == "Format")
            {
                var lineSpan = invocation.GetLocation().GetLineSpan();
                issues.Add(new PerformanceIssue
                {
                    Message = "Using string.Format instead of string interpolation",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Category = "String Performance", 
                    Severity = PerformanceSeverity.Low,
                    Recommendation = "Consider using string interpolation ($\"\") for better readability and performance",
                    CodeSnippet = invocation.ToString()
                });
            }
        }

        return issues;
    }

    private static PerformanceMetrics CalculatePerformanceMetrics(
        SyntaxNode root, 
        PerformanceIssue[] linqIssues,
        PerformanceIssue[] allocationIssues, 
        PerformanceIssue[] asyncIssues,
        PerformanceIssue[] stringIssues)
    {
        var allIssues = linqIssues.Concat(allocationIssues).Concat(asyncIssues).Concat(stringIssues).ToArray();

        return new PerformanceMetrics
        {
            TotalPerformanceIssues = allIssues.Length,
            HighSeverityIssues = allIssues.Count(i => i.Severity == PerformanceSeverity.High),
            MediumSeverityIssues = allIssues.Count(i => i.Severity == PerformanceSeverity.Medium),
            LowSeverityIssues = allIssues.Count(i => i.Severity == PerformanceSeverity.Low),
            LinqUsageCount = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Count(inv => inv.Expression is MemberAccessExpressionSyntax ma && IsLinqMethod(ma.Name.Identifier.ValueText)),
            AsyncMethodCount = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Count(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AsyncKeyword))),
            StringConcatenationCount = root.DescendantNodes().OfType<BinaryExpressionSyntax>()
                .Count(b => b.OperatorToken.IsKind(SyntaxKind.PlusToken))
        };
    }

    private static bool IsLinqMethod(string methodName)
    {
        var linqMethods = new HashSet<string>
        {
            "Where", "Select", "SelectMany", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
            "GroupBy", "Join", "GroupJoin", "Take", "Skip", "TakeWhile", "SkipWhile",
            "First", "FirstOrDefault", "Last", "LastOrDefault", "Single", "SingleOrDefault",
            "Any", "All", "Count", "Sum", "Min", "Max", "Average", "Aggregate",
            "ToArray", "ToList", "ToDictionary", "ToLookup", "AsEnumerable", "AsQueryable"
        };
        
        return linqMethods.Contains(methodName);
    }

    private static bool IsEventHandler(MethodDeclarationSyntax method)
    {
        return method.ParameterList.Parameters.Count == 2 &&
               method.ParameterList.Parameters[0].Type?.ToString().Contains("sender") == true &&
               method.ParameterList.Parameters[1].Type?.ToString().Contains("EventArgs") == true;
    }
}