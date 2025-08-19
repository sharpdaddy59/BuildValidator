using Microsoft.CodeAnalysis;

namespace BuildValidator;

public record CodeAnalysisResult
{
    public required string FilePath { get; init; }
    public required string SourceCode { get; init; }
    public required SyntaxAnalysis SyntaxAnalysis { get; init; }
    public required SemanticAnalysis SemanticAnalysis { get; init; }
    public required CodeMetrics CodeMetrics { get; init; }
    public required CompilationIssue[] CompilationIssues { get; init; }
}

public record SyntaxAnalysis
{
    public required int TotalLines { get; init; }
    public required int CodeLines { get; init; }
    public required int CommentLines { get; init; }
    public required int BlankLines { get; init; }
    public required string[] ClassNames { get; init; }
    public required string[] MethodNames { get; init; }
    public required string[] PropertyNames { get; init; }
    public required string[] NamespaceNames { get; init; }
    public required string[] UsingDirectives { get; init; }
}

public record SemanticAnalysis
{
    public required TypeInfo[] TypeInfos { get; init; }
    public required SymbolInfo[] SymbolInfos { get; init; }
    public required CodeIssue[] UnusedUsings { get; init; }
    public required CodeIssue[] PotentialNullReferences { get; init; }
}

public record TypeInfo
{
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string Kind { get; init; }
    public required bool IsPublic { get; init; }
    public required bool IsStatic { get; init; }
    public required bool IsAbstract { get; init; }
}

public record SymbolInfo
{
    public required string Name { get; init; }
    public required string Kind { get; init; }
    public required string Type { get; init; }
    public required bool IsUsed { get; init; }
}

public record CodeMetrics
{
    public required int CyclomaticComplexity { get; init; }
    public required int NestingDepth { get; init; }
    public required int MethodCount { get; init; }
    public required int ClassCount { get; init; }
    public required int PropertyCount { get; init; }
    public required double MaintainabilityIndex { get; init; }
}

public record CompilationIssue
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required string Category { get; init; }
}

public record CodeIssue
{
    public required string Message { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required string? FilePath { get; init; }
}