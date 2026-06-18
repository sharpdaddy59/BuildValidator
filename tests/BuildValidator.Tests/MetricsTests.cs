namespace BuildValidator.Tests;

public class MetricsTests
{
    private static async Task<CodeMetrics> MetricsFor(string source)
    {
        var result = await new RoslynAnalyzer().AnalyzeCodeAsync(source, "Test.cs");
        return result.CodeMetrics;
    }

    [Fact]
    public async Task CyclomaticComplexity_CountsBranchesPlusOne()
    {
        // Base 1 + two ifs + one for = 4 (note: &&/||/case are not counted by design)
        const string src = @"
class C {
    void M(int x) {
        if (x > 0) { }
        if (x > 1) { }
        for (int i = 0; i < x; i++) { }
    }
}";
        var metrics = await MetricsFor(src);
        Assert.Equal(4, metrics.CyclomaticComplexity);
    }

    [Fact]
    public async Task MaintainabilityIndex_IsNotZero_ForNormalFile()
    {
        // Regression guard: the old implementation passed character count as
        // lines-of-code, flooring any non-trivial file to 0.
        const string src = @"
namespace Demo;
public class Calculator {
    public int Add(int a, int b) => a + b;
    public int Sub(int a, int b) => a - b;
    public int Mul(int a, int b) => a * b;
}";
        var metrics = await MetricsFor(src);
        Assert.True(metrics.MaintainabilityIndex > 0,
            $"Expected MI > 0 but got {metrics.MaintainabilityIndex}");
    }

    [Fact]
    public async Task MaintainabilityIndex_StaysWithin0To100()
    {
        const string src = "class C { void M() { } }";
        var metrics = await MetricsFor(src);
        Assert.InRange(metrics.MaintainabilityIndex, 0, 100);
    }

    [Fact]
    public async Task MaintainabilityIndex_SimpleScoresHigherThanComplex()
    {
        const string simple = @"
class Simple {
    public int Value => 42;
}";
        const string complex = @"
class Complex {
    public int Compute(int x) {
        int total = 0;
        for (int i = 0; i < x; i++) {
            if (i % 2 == 0) { total += i; }
            else if (i % 3 == 0) { total -= i; }
            else { total *= 2; }
            while (total > 100) { total /= 2; }
            switch (i) {
                case 1: total++; break;
                case 2: total--; break;
                default: break;
            }
        }
        return total;
    }
}";
        var simpleMetrics = await MetricsFor(simple);
        var complexMetrics = await MetricsFor(complex);

        Assert.True(simpleMetrics.MaintainabilityIndex > complexMetrics.MaintainabilityIndex,
            $"simple={simpleMetrics.MaintainabilityIndex} complex={complexMetrics.MaintainabilityIndex}");
    }

    [Fact]
    public async Task MethodAndClassCounts_AreAccurate()
    {
        const string src = @"
class A {
    void M1() { }
    void M2() { }
}
class B {
    void M3() { }
}";
        var metrics = await MetricsFor(src);
        Assert.Equal(2, metrics.ClassCount);
        Assert.Equal(3, metrics.MethodCount);
    }
}
