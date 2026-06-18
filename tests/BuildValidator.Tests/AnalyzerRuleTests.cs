namespace BuildValidator.Tests;

public class AnalyzerRuleTests
{
    private static async Task<CodeAnalysisResult> Analyze(string source) =>
        await new RoslynAnalyzer().AnalyzeCodeAsync(source, "Test.cs");

    private static IEnumerable<string> AllStyleRuleIds(StyleAnalysis style) =>
        style.DocumentationIssues
            .Concat(style.EncapsulationIssues)
            .Concat(style.AccessibilityIssues)
            .Concat(style.OrganizationIssues)
            .Select(i => i.RuleId!)
            .Where(id => id != null);

    [Fact]
    public async Task PublicField_TriggersEncapsulationRule_ENC001()
    {
        const string src = @"
public class Account {
    public int Balance;
}";
        var result = await Analyze(src);
        Assert.Contains("ENC001", AllStyleRuleIds(result.StyleAnalysis));
    }

    [Fact]
    public async Task UndocumentedPublicClass_TriggersDocumentationRule_DOC001()
    {
        const string src = @"
public class Widget {
    public void DoThing() { }
}";
        var result = await Analyze(src);
        Assert.Contains("DOC001", AllStyleRuleIds(result.StyleAnalysis));
    }

    [Fact]
    public async Task CleanDocumentedCode_DoesNotTriggerDocOrEncapsulationRules()
    {
        const string src = @"
/// <summary>A point.</summary>
public class Point {
    private int _x;
    /// <summary>Gets X.</summary>
    public int X => _x;
}";
        var result = await Analyze(src);
        var ids = AllStyleRuleIds(result.StyleAnalysis).ToList();
        Assert.DoesNotContain("ENC001", ids);
        Assert.DoesNotContain("DOC001", ids);
    }

    [Fact]
    public async Task SemanticIssues_HaveRuleIds_WhenPresent()
    {
        // Element access without a null guard should surface a SEM rule.
        const string src = @"
public class C {
    public int First(int[] data) {
        return data[0];
    }
}";
        var result = await Analyze(src);
        // Not asserting a specific count (depends on heuristics), but any
        // semantic issue raised must carry a non-empty rule id.
        Assert.All(result.SemanticIssues, issue => Assert.False(string.IsNullOrWhiteSpace(issue.RuleId)));
    }
}
