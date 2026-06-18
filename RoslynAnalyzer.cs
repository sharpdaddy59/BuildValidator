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
        var semanticAnalysis = AnalyzeSemantics(root, semanticModel, styleConfig);
        var codeMetrics = CalculateMetrics(root, semanticModel);
        var compilationIssues = GetCompilationIssues(compilation);
        var performanceAnalysis = PerformanceAnalyzer.AnalyzePerformance(root, semanticModel);
        var styleAnalysis = StyleValidationAnalyzer.AnalyzeStyle(root, semanticModel, filePath, styleConfig);
        var semanticIssues = SemanticAnalyzer.AnalyzeSemantics(root, semanticModel, filePath, styleConfig);

        return new CodeAnalysisResult
        {
            FilePath = filePath,
            SourceCode = sourceCode,
            SyntaxAnalysis = syntaxAnalysis,
            SemanticAnalysis = semanticAnalysis,
            CodeMetrics = codeMetrics,
            CompilationIssues = compilationIssues,
            PerformanceAnalysis = performanceAnalysis,
            StyleAnalysis = styleAnalysis,
            SemanticIssues = semanticIssues
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

    private static SemanticAnalysis AnalyzeSemantics(SyntaxNode root, SemanticModel semanticModel, StyleConfiguration? styleConfig = null)
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

        var unusedUsings = styleConfig?.EnableUnusedImportDetection == true 
            ? FindUnusedUsings(root, semanticModel) 
            : Array.Empty<CodeIssue>();
            
        var potentialNullRefs = styleConfig?.EnableNullReferenceDetection == true 
            ? FindPotentialNullReferences(root, semanticModel) 
            : Array.Empty<CodeIssue>();

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
        var linesOfCode = CountLinesOfCode(root);
        var halsteadVolume = CalculateHalsteadVolume(root);
        var maintainabilityIndex = CalculateMaintainabilityIndex(complexity, linesOfCode, halsteadVolume);

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

    // Visual Studio-aligned Maintainability Index.
    //   raw = 171 - 5.2*ln(HalsteadVolume) - 0.23*CyclomaticComplexity - 16.2*ln(LinesOfCode)
    //   MI  = max(0, raw * 100 / 171)   -> normalized to the familiar 0-100 scale
    // Inputs must be real lines of code and a real Halstead volume (see helpers below).
    private static double CalculateMaintainabilityIndex(int complexity, int linesOfCode, double halsteadVolume)
    {
        var cyclomaticComplexity = Math.Max(complexity, 1);
        var loc = Math.Max(linesOfCode, 1);

        // Drop the volume term when there are no measurable operators/operands
        // (ln(0) is undefined); an empty file should not produce a misleading score.
        var volumeTerm = halsteadVolume > 0 ? 5.2 * Math.Log(halsteadVolume) : 0;

        var raw = 171 - volumeTerm - 0.23 * cyclomaticComplexity - 16.2 * Math.Log(loc);
        var normalized = raw * 100.0 / 171.0;

        return Math.Max(0, Math.Min(100, normalized));
    }

    // Physical lines of code: non-blank source lines. A reasonable, deterministic
    // proxy for the LOC term (the previous code mistakenly passed character count).
    private static int CountLinesOfCode(SyntaxNode root)
    {
        var text = root.SyntaxTree.GetText();
        return text.Lines.Count(line => !string.IsNullOrWhiteSpace(line.ToString()));
    }

    // Halstead volume V = (N1 + N2) * log2(n1 + n2), where operators/operands are
    // counted from syntax tokens. Operands are identifiers and literals (including
    // true/false/null); everything else (keywords, punctuation, operators) is an
    // operator. Trivia (comments/whitespace) are not tokens, so they're excluded.
    private static double CalculateHalsteadVolume(SyntaxNode root)
    {
        var operators = new Dictionary<string, int>();
        var operands = new Dictionary<string, int>();

        foreach (var token in root.DescendantTokens())
        {
            var kind = token.Kind();
            if (kind == SyntaxKind.EndOfFileToken || kind == SyntaxKind.BadToken)
                continue;

            if (IsOperand(kind))
            {
                var key = token.ValueText;
                operands[key] = operands.GetValueOrDefault(key) + 1;
            }
            else if (!string.IsNullOrEmpty(token.Text))
            {
                operators[token.Text] = operators.GetValueOrDefault(token.Text) + 1;
            }
        }

        int vocabulary = operators.Count + operands.Count;          // n1 + n2
        long length = operators.Values.Sum() + operands.Values.Sum(); // N1 + N2

        return vocabulary <= 1 ? 0 : length * Math.Log2(vocabulary);
    }

    private static bool IsOperand(SyntaxKind kind) => kind switch
    {
        SyntaxKind.IdentifierToken => true,
        SyntaxKind.NumericLiteralToken => true,
        SyntaxKind.StringLiteralToken => true,
        SyntaxKind.CharacterLiteralToken => true,
        SyntaxKind.InterpolatedStringTextToken => true,
        SyntaxKind.TrueKeyword => true,
        SyntaxKind.FalseKeyword => true,
        SyntaxKind.NullKeyword => true,
        _ => false
    };

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