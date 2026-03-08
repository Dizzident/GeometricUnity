using Gu.Artifacts;
using Gu.Core;
using Gu.Validation;
using Gu.Validation.Rules;

namespace Gu.Artifacts.Tests;

public class ValidationRunnerTests
{
    [Fact]
    public void RunWithNoRules_ReturnsEmptyPassingBundle()
    {
        var runner = new ValidationRunner();
        var bundle = runner.Run(TestHelpers.CreateTestBranchRef());

        Assert.True(bundle.AllPassed);
        Assert.Empty(bundle.Records);
    }

    [Fact]
    public void RunWithPassingRules_ReturnsPassingBundle()
    {
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("check-1", "structural", () => true, "always true"))
            .AddRule(new PredicateRule("check-2", "structural", () => true, "also true"));

        var bundle = runner.Run(TestHelpers.CreateTestBranchRef());

        Assert.True(bundle.AllPassed);
        Assert.Equal(2, bundle.Records.Count);
        Assert.All(bundle.Records, r => Assert.True(r.Passed));
    }

    [Fact]
    public void RunWithFailingRule_ReturnsFailedBundle()
    {
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("check-1", "structural", () => true, "passes"))
            .AddRule(new PredicateRule("check-2", "structural", () => false, "fails"));

        var bundle = runner.Run(TestHelpers.CreateTestBranchRef());

        Assert.False(bundle.AllPassed);
        Assert.Equal(2, bundle.Records.Count);
        Assert.Contains(bundle.Records, r => r.Passed);
        Assert.Contains(bundle.Records, r => !r.Passed);
    }

    [Fact]
    public void RunOrThrow_ThrowsOnFailure()
    {
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("bad-rule", "structural", () => false, "always fails"));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            runner.RunOrThrow(TestHelpers.CreateTestBranchRef()));

        Assert.Contains("bad-rule", ex.Message);
    }

    [Fact]
    public void RunOrThrow_SucceedsWhenAllPass()
    {
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("good-rule", "structural", () => true, "always passes"));

        var bundle = runner.RunOrThrow(TestHelpers.CreateTestBranchRef());
        Assert.True(bundle.AllPassed);
    }

    [Fact]
    public void ToleranceRule_PassesWithinTolerance()
    {
        var rule = new ToleranceRule("tol-check", "convergence", () => 1.0001, 1.0, 0.001);
        var record = rule.Execute();

        Assert.True(record.Passed);
        Assert.Equal(1.0001, record.MeasuredValue);
    }

    [Fact]
    public void ToleranceRule_FailsOutsideTolerance()
    {
        var rule = new ToleranceRule("tol-check", "convergence", () => 2.0, 1.0, 0.001);
        var record = rule.Execute();

        Assert.False(record.Passed);
        Assert.Contains("exceeds tolerance", record.Detail);
    }

    [Fact]
    public void ThresholdRule_PassesBelowThreshold()
    {
        var rule = new ThresholdRule("thresh-check", "convergence", () => 0.001, 0.01);
        var record = rule.Execute();

        Assert.True(record.Passed);
    }

    [Fact]
    public void ThresholdRule_FailsAboveThreshold()
    {
        var rule = new ThresholdRule("thresh-check", "convergence", () => 0.1, 0.01);
        var record = rule.Execute();

        Assert.False(record.Passed);
        Assert.Contains("exceeds threshold", record.Detail);
    }

    [Fact]
    public void ParityRule_PassesForIdenticalValues()
    {
        var values = new double[] { 1.0, 2.0, 3.0 };
        var rule = new ParityRule("parity-check", "parity",
            () => values, "A", () => values, "B", 1e-12);
        var record = rule.Execute();

        Assert.True(record.Passed);
        Assert.Equal(0.0, record.MeasuredValue);
    }

    [Fact]
    public void ParityRule_FailsForDifferentValues()
    {
        var valuesA = new double[] { 1.0, 2.0, 3.0 };
        var valuesB = new double[] { 1.0, 2.0, 4.0 };
        var rule = new ParityRule("parity-check", "parity",
            () => valuesA, "CPU", () => valuesB, "GPU", 1e-12);
        var record = rule.Execute();

        Assert.False(record.Passed);
        Assert.Contains("Parity failure", record.Detail);
        Assert.Equal(1.0, record.MeasuredValue);
    }

    [Fact]
    public void ParityRule_FailsForLengthMismatch()
    {
        var valuesA = new double[] { 1.0, 2.0 };
        var valuesB = new double[] { 1.0, 2.0, 3.0 };
        var rule = new ParityRule("parity-check", "parity",
            () => valuesA, "A", () => valuesB, "B", 1e-12);
        var record = rule.Execute();

        Assert.False(record.Passed);
        Assert.Contains("Length mismatch", record.Detail);
    }

    [Fact]
    public void AddRules_AcceptsMultipleRules()
    {
        var rules = new IValidationRule[]
        {
            new PredicateRule("r1", "test", () => true, "rule 1"),
            new PredicateRule("r2", "test", () => true, "rule 2"),
            new PredicateRule("r3", "test", () => true, "rule 3"),
        };

        var runner = new ValidationRunner().AddRules(rules);
        var bundle = runner.Run(TestHelpers.CreateTestBranchRef());

        Assert.Equal(3, bundle.Records.Count);
        Assert.True(bundle.AllPassed);
    }

    [Fact]
    public void ValidationRecords_HaveTimestamps()
    {
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("ts-check", "structural", () => true, "has timestamp"));

        var bundle = runner.Run(TestHelpers.CreateTestBranchRef());
        var record = bundle.Records[0];

        Assert.True(record.Timestamp > DateTimeOffset.MinValue);
    }

    [Fact]
    public void ValidationBundle_CorrectBranchRef()
    {
        var branch = TestHelpers.CreateTestBranchRef();
        var runner = new ValidationRunner()
            .AddRule(new PredicateRule("br-check", "structural", () => true, "check"));

        var bundle = runner.Run(branch);

        Assert.Equal("test-branch", bundle.Branch.BranchId);
        Assert.Equal("1.0.0", bundle.Branch.SchemaVersion);
    }
}
