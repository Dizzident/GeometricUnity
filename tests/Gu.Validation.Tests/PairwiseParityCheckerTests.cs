using Gu.Core;
using Gu.Validation;

namespace Gu.Validation.Tests;

public class PairwiseParityCheckerTests
{
    [Fact]
    public void CompareFields_IdenticalFields_Passes()
    {
        var checker = new PairwiseParityChecker();
        var field = TestHelpers.CreateTestField("omega", new[] { 3, 4 },
            new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 });

        var record = checker.CompareFields(field, field, "omega");

        Assert.True(record.Passed);
        Assert.Equal(0.0, record.MeasuredValue);
        Assert.Equal("parity-omega", record.RuleId);
        Assert.Equal(ValidationRuleRegistry.Parity, record.Category);
    }

    [Fact]
    public void CompareFields_WithinTolerance_Passes()
    {
        var checker = new PairwiseParityChecker(tolerance: 1e-6);
        var fieldA = TestHelpers.CreateTestField("omega", new[] { 3 },
            new double[] { 1.0, 2.0, 3.0 });
        var fieldB = TestHelpers.CreateTestField("omega", new[] { 3 },
            new double[] { 1.0 + 1e-7, 2.0 - 1e-8, 3.0 + 1e-9 });

        var record = checker.CompareFields(fieldA, fieldB, "omega");

        Assert.True(record.Passed);
        Assert.True(record.MeasuredValue!.Value < 1e-6);
    }

    [Fact]
    public void CompareFields_ExceedsTolerance_Fails()
    {
        var checker = new PairwiseParityChecker(tolerance: 1e-10);
        var fieldA = TestHelpers.CreateTestField("omega", new[] { 3 },
            new double[] { 1.0, 2.0, 3.0 });
        var fieldB = TestHelpers.CreateTestField("omega", new[] { 3 },
            new double[] { 1.0, 2.0, 3.001 });

        var record = checker.CompareFields(fieldA, fieldB, "omega");

        Assert.False(record.Passed);
        Assert.True(record.MeasuredValue!.Value > 1e-10);
    }

    [Fact]
    public void CompareFields_DifferentLengths_Fails()
    {
        var checker = new PairwiseParityChecker();
        var fieldA = TestHelpers.CreateTestField("omega", new[] { 3 },
            new double[] { 1.0, 2.0, 3.0 });
        var fieldB = TestHelpers.CreateTestField("omega", new[] { 4 },
            new double[] { 1.0, 2.0, 3.0, 4.0 });

        var record = checker.CompareFields(fieldA, fieldB, "omega");

        Assert.False(record.Passed);
        Assert.Contains("length mismatch", record.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompareDerivedStates_IdenticalStates_Passes()
    {
        var checker = new PairwiseParityChecker();
        var state = TestHelpers.CreateValidDerivedState();
        var branch = TestHelpers.CreateTestBranchRef();

        var bundle = checker.CompareDerivedStates(state, state, branch);

        Assert.True(bundle.AllPassed);
        Assert.Equal(4, bundle.Records.Count);
    }

    [Fact]
    public void CompareDerivedStates_DifferentStates_Fails()
    {
        var checker = new PairwiseParityChecker(tolerance: 1e-10);
        var stateA = TestHelpers.CreateValidDerivedState();
        var stateB = new DerivedState
        {
            CurvatureF = TestHelpers.CreateTestField("F_h", new[] { 3, 4 },
                coefficients: Enumerable.Repeat(1.0, 12).ToArray(), carrierType: "curvature-2form"),
            TorsionT = TestHelpers.CreateTestField("T_h", new[] { 3, 4 }),
            ShiabS = TestHelpers.CreateTestField("S_h", new[] { 3, 4 }),
            ResidualUpsilon = TestHelpers.CreateTestField("Upsilon_h", new[] { 3, 4 }),
        };
        var branch = TestHelpers.CreateTestBranchRef();

        var bundle = checker.CompareDerivedStates(stateA, stateB, branch);

        Assert.False(bundle.AllPassed);
        var curvatureRecord = bundle.Records.First(r => r.RuleId == "parity-curvatureF");
        Assert.False(curvatureRecord.Passed);
    }

    [Fact]
    public void Compare_ArtifactBundles_MatchingValidation_Passes()
    {
        var checker = new PairwiseParityChecker();
        var branch = TestHelpers.CreateTestBranchRef();
        var bundleA = TestHelpers.CreateTestArtifactBundle();
        var bundleB = TestHelpers.CreateTestArtifactBundle();

        var result = checker.Compare(bundleA, bundleB, branch);

        Assert.True(result.AllPassed);
    }

    [Fact]
    public void Compare_ArtifactBundles_DifferentValidationOutcome_Fails()
    {
        var checker = new PairwiseParityChecker();
        var branch = TestHelpers.CreateTestBranchRef();
        var bundleA = TestHelpers.CreateTestArtifactBundle(
            validation: TestHelpers.CreateTestValidationBundle(allPassed: true));
        var bundleB = TestHelpers.CreateTestArtifactBundle(
            validation: TestHelpers.CreateTestValidationBundle(allPassed: false));

        var result = checker.Compare(bundleA, bundleB, branch);

        Assert.False(result.AllPassed);
        Assert.Contains(result.Records, r => r.RuleId == "parity-validation-outcome" && !r.Passed);
    }

    [Fact]
    public void Compare_ArtifactBundles_WithObservedState_Passes()
    {
        var checker = new PairwiseParityChecker();
        var branch = TestHelpers.CreateTestBranchRef();
        var observed = TestHelpers.CreateTestObservedState(new[] { 1.0, 2.0, 3.0 });
        var bundleA = TestHelpers.CreateTestArtifactBundle(observed: observed);
        var bundleB = TestHelpers.CreateTestArtifactBundle(observed: observed);

        var result = checker.Compare(bundleA, bundleB, branch);

        Assert.True(result.AllPassed);
    }

    [Fact]
    public void Compare_ArtifactBundles_DifferentObservedValues_Fails()
    {
        var checker = new PairwiseParityChecker(tolerance: 1e-10);
        var branch = TestHelpers.CreateTestBranchRef();
        var observedA = TestHelpers.CreateTestObservedState(new[] { 1.0, 2.0, 3.0 });
        var observedB = TestHelpers.CreateTestObservedState(new[] { 1.0, 2.0, 4.0 });
        var bundleA = TestHelpers.CreateTestArtifactBundle(observed: observedA);
        var bundleB = TestHelpers.CreateTestArtifactBundle(observed: observedB);

        var result = checker.Compare(bundleA, bundleB, branch);

        Assert.False(result.AllPassed);
        Assert.Contains(result.Records, r =>
            r.RuleId == "parity-observable-energy" && !r.Passed);
    }

    [Fact]
    public void PairwiseParityChecker_DefaultTolerance_Is1eNeg10()
    {
        Assert.Equal(1e-10, PairwiseParityChecker.DefaultTolerance);
    }
}
