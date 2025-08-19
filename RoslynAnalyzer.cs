using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BuildValidator;

public class RoslynAnalyzer
{
    public async Task<CodeAnalysisResult> AnalyzeCodeAsync(string sourceCode, string filePath = "temp.cs", StyleConfiguration? styleConfig = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, path: filePath);
        var compilation = CreateCompilation(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = await syntaxTree.GetRootAsync();

        var syntaxAnalysis = AnalyzeSyntax(root, sourceCode);
        var semanticAnalysis = AnalyzeSemantics(root, semanticModel);
        var codeMetrics = CalculateMetrics(root, semanticModel);
        var compilationIssues = GetCompilationIssues(compilation);
        var performanceAnalysis = PerformanceAnalyzer.AnalyzePerformance(root, semanticModel);
        var styleAnalysis = StyleValidationAnalyzer.AnalyzeStyle(root, semanticModel, filePath, styleConfig);

        return new CodeAnalysisResult
        {
            FilePath = filePath,
            SourceCode = sourceCode,
            SyntaxAnalysis = syntaxAnalysis,
            SemanticAnalysis = semanticAnalysis,
            CodeMetrics = codeMetrics,
            CompilationIssues = compilationIssues,
            PerformanceAnalysis = performanceAnalysis,
            StyleAnalysis = styleAnalysis
        };
    }

    public async Task<CodeAnalysisResult> AnalyzeFileAsync(string filePath, StyleConfiguration? styleConfig = null)
    {
        var sourceCode = await File.ReadAllTextAsync(filePath);
        return await AnalyzeCodeAsync(sourceCode, filePath, styleConfig);
    }

    private static CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
    {
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location)
        };

        return CSharpCompilation.Create(
            assemblyName: "TempAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static SyntaxAnalysis AnalyzeSyntax(SyntaxNode root, string sourceCode)
    {
        var lines = sourceCode.Split('\n');
        var totalLines = lines.Length;
        var blankLines = lines.Count(line => string.IsNullOrWhiteSpace(line));
        var commentLines = CountCommentLines(root);
        var codeLines = totalLines - blankLines - commentLines;

        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Select(c => c.Identifier.ValueText).ToArray();

        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Select(m => m.Identifier.ValueText).ToArray();

        var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.ValueText).ToArray();

        var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
            .Select(n => n.Name.ToString()).ToArray();

        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? "").ToArray();

        return new SyntaxAnalysis
        {
            TotalLines = totalLines,
            CodeLines = codeLines,
            CommentLines = commentLines,
            BlankLines = blankLines,
            ClassNames = classes,
            MethodNames = methods,
            PropertyNames = properties,
            NamespaceNames = namespaces,
            UsingDirectives = usings
        };
    }

    private static int CountCommentLines(SyntaxNode root)
    {
        var commentCount = 0;
        foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
        {
            commentCount += token.LeadingTrivia.Count(t => 
                t.IsKind(SyntaxKind.SingleLineCommentTrivia) || 
                t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
        }
        return commentCount;
    }

    private static SemanticAnalysis AnalyzeSemantics(SyntaxNode root, SemanticModel semanticModel)
    {
        var typeInfos = new List<TypeInfo>();
        var symbolInfos = new List<SymbolInfo>();

        foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var symbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (symbol != null)
            {
                typeInfos.Add(new TypeInfo
                {
                    Name = symbol.Name,
                    FullName = symbol.ToDisplayString(),
                    Kind = symbol.TypeKind.ToString(),
                    IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
                    IsStatic = symbol.IsStatic,
                    IsAbstract = symbol.IsAbstract
                });
            }
        }

        foreach (var methodDecl in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var symbol = semanticModel.GetDeclaredSymbol(methodDecl);
            if (symbol != null)
            {
                symbolInfos.Add(new SymbolInfo
                {
                    Name = symbol.Name,
                    Kind = "Method",
                    Type = symbol.ReturnType.ToDisplayString(),
                    IsUsed = true
                });
            }
        }

        var unusedUsings = FindUnusedUsings(root, semanticModel);
        var potentialNullRefs = FindPotentialNullReferences(root, semanticModel);

        return new SemanticAnalysis
        {
            TypeInfos = typeInfos.ToArray(),
            SymbolInfos = symbolInfos.ToArray(),
            UnusedUsings = unusedUsings,
            PotentialNullReferences = potentialNullRefs
        };
    }

    private static CodeIssue[] FindUnusedUsings(SyntaxNode root, SemanticModel semanticModel)
    {
        var unusedUsings = new List<CodeIssue>();
        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

        foreach (var usingDirective in usings)
        {
            var namespaceName = usingDirective.Name?.ToString();
            if (!string.IsNullOrEmpty(namespaceName))
            {
                var sourceText = root.ToString();
                var namespaceShortName = namespaceName.Split('.').Last();
                
                if (!sourceText.Contains(namespaceShortName) || 
                    sourceText.Count(namespaceShortName.Contains) <= 1)
                {
                    var lineSpan = usingDirective.GetLocation().GetLineSpan();
                    unusedUsings.Add(new CodeIssue
                    {
                        Message = $"Unused using directive: {namespaceName}",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        FilePath = lineSpan.Path
                    });
                }
            }
        }

        return unusedUsings.ToArray();
    }

    private static CodeIssue[] FindPotentialNullReferences(SyntaxNode root, SemanticModel semanticModel)
    {
        var potentialNulls = new List<CodeIssue>();

        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
            if (typeInfo.Type != null)
            {
                var typeName = typeInfo.Type.ToDisplayString();
                if (typeName.EndsWith("?") || typeName.Contains("null"))
                {
                    var lineSpan = memberAccess.GetLocation().GetLineSpan();
                    potentialNulls.Add(new CodeIssue
                    {
                        Message = $"Potential null reference: {memberAccess.Expression}",
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        FilePath = lineSpan.Path
                    });
                }
            }
        }

        return potentialNulls.ToArray();
    }

    private static CodeMetrics CalculateMetrics(SyntaxNode root, SemanticModel semanticModel)
    {
        var complexity = CalculateCyclomaticComplexity(root);
        var nestingDepth = CalculateMaxNestingDepth(root);
        var methodCount = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Count();
        var classCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
        var propertyCount = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count();
        var maintainabilityIndex = CalculateMaintainabilityIndex(complexity, root.ToString().Length, methodCount);

        return new CodeMetrics
        {
            CyclomaticComplexity = complexity,
            NestingDepth = nestingDepth,
            MethodCount = methodCount,
            ClassCount = classCount,
            PropertyCount = propertyCount,
            MaintainabilityIndex = maintainabilityIndex
        };
    }

    private static int CalculateCyclomaticComplexity(SyntaxNode root)
    {
        var complexity = 1;

        var complexityNodes = root.DescendantNodes().Where(node =>
            node.IsKind(SyntaxKind.IfStatement) ||
            node.IsKind(SyntaxKind.WhileStatement) ||
            node.IsKind(SyntaxKind.ForStatement) ||
            node.IsKind(SyntaxKind.ForEachStatement) ||
            node.IsKind(SyntaxKind.SwitchStatement) ||
            node.IsKind(SyntaxKind.CatchClause) ||
            node.IsKind(SyntaxKind.ConditionalExpression));

        return complexity + complexityNodes.Count();
    }

    private static int CalculateMaxNestingDepth(SyntaxNode root)
    {
        return CalculateNestingDepth(root, 0);
    }

    private static int CalculateNestingDepth(SyntaxNode node, int currentDepth)
    {
        var maxDepth = currentDepth;

        foreach (var child in node.ChildNodes())
        {
            var childDepth = currentDepth;
            
            if (child.IsKind(SyntaxKind.IfStatement) ||
                child.IsKind(SyntaxKind.WhileStatement) ||
                child.IsKind(SyntaxKind.ForStatement) ||
                child.IsKind(SyntaxKind.ForEachStatement) ||
                child.IsKind(SyntaxKind.SwitchStatement) ||
                child.IsKind(SyntaxKind.TryStatement))
            {
                childDepth++;
            }

            var depth = CalculateNestingDepth(child, childDepth);
            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    private static double CalculateMaintainabilityIndex(int complexity, int linesOfCode, int methodCount)
    {
        var halsteadVolume = Math.Log(linesOfCode + 1) * 10;
        var cyclomaticComplexity = Math.Max(complexity, 1);
        
        var maintainabilityIndex = 171 - 5.2 * Math.Log(halsteadVolume) - 0.23 * cyclomaticComplexity - 16.2 * Math.Log(Math.Max(linesOfCode, 1));
        
        return Math.Max(0, Math.Min(100, maintainabilityIndex));
    }

    private static CompilationIssue[] GetCompilationIssues(Compilation compilation)
    {
        var diagnostics = compilation.GetDiagnostics();
        var issues = new List<CompilationIssue>();

        foreach (var diagnostic in diagnostics)
        {
            var location = diagnostic.Location.GetLineSpan();
            
            issues.Add(new CompilationIssue
            {
                Id = diagnostic.Id,
                Title = diagnostic.Descriptor.Title.ToString(),
                Message = diagnostic.GetMessage(),
                Severity = diagnostic.Severity,
                Line = location.StartLinePosition.Line + 1,
                Column = location.StartLinePosition.Character + 1,
                Category = diagnostic.Descriptor.Category
            });
        }

        return issues.ToArray();
    }
}