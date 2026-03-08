using Gu.Core;
using Gu.Math;
using Gu.Validation;

namespace Gu.Validation.Tests;

public class ValidationEngineTests
{
    [Fact]
    public void ValidateAlgebraicIdentities_Su2_AllPass()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAlgebraicIdentities(branch, algebra);

        Assert.True(bundle.AllPassed, FormatFailures(bundle));
        Assert.Equal(3, bundle.Records.Count);
        Assert.Contains(bundle.Records, r => r.RuleId == "jacobi-identity" && r.Passed);
        Assert.Contains(bundle.Records, r => r.RuleId == "antisymmetry" && r.Passed);
        Assert.Contains(bundle.Records, r => r.RuleId == "metric-symmetry" && r.Passed);
    }

    [Fact]
    public void ValidateAlgebraicIdentities_BrokenAlgebra_AntisymmetryFails()
    {
        var algebra = TestHelpers.CreateBrokenAlgebra();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAlgebraicIdentities(branch, algebra);

        Assert.False(bundle.AllPassed);
        var antisymRecord = bundle.Records.First(r => r.RuleId == "antisymmetry");
        Assert.False(antisymRecord.Passed);
        Assert.NotNull(antisymRecord.MeasuredValue);
        Assert.True(antisymRecord.MeasuredValue > 0);
    }

    [Fact]
    public void ValidateAlgebraicIdentities_BrokenAlgebra_MetricSymmetryFails()
    {
        var algebra = TestHelpers.CreateBrokenAlgebra();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAlgebraicIdentities(branch, algebra);

        var metricRecord = bundle.Records.First(r => r.RuleId == "metric-symmetry");
        Assert.False(metricRecord.Passed);
        Assert.NotNull(metricRecord.MeasuredValue);
        Assert.True(metricRecord.MeasuredValue > 0);
    }

    [Fact]
    public void ValidateAlgebraicIdentities_ToleranceIsRespected()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var branch = TestHelpers.CreateTestBranchRef();

        // SU(2) violations should be exactly 0, so any positive tolerance should pass
        var strictEngine = new ValidationEngine(tolerance: 0.0);
        var bundle = strictEngine.ValidateAlgebraicIdentities(branch, algebra);

        // Even with zero tolerance, SU(2) should pass since violations are exactly 0
        Assert.True(bundle.AllPassed, FormatFailures(bundle));
    }

    [Fact]
    public void ValidateSignatures_ValidDerived_AllPass()
    {
        var derived = TestHelpers.CreateValidDerivedState();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateSignatures(branch, derived);

        Assert.True(bundle.AllPassed, FormatFailures(bundle));
        Assert.Contains(bundle.Records, r => r.RuleId == "carrier-compatibility" && r.Passed);
        Assert.Contains(bundle.Records, r => r.RuleId == "shape-coefficient-match" && r.Passed);
    }

    [Fact]
    public void ValidateSignatures_MismatchedCarrier_Fails()
    {
        var derived = TestHelpers.CreateMismatchedCarrierDerivedState();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateSignatures(branch, derived);

        Assert.False(bundle.AllPassed);
        var carrierRecord = bundle.Records.First(r => r.RuleId == "carrier-compatibility");
        Assert.False(carrierRecord.Passed);
    }

    [Fact]
    public void ValidateSignatures_ShapeMismatch_Fails()
    {
        var derived = TestHelpers.CreateShapeMismatchDerivedState();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateSignatures(branch, derived);

        Assert.False(bundle.AllPassed);
        var shapeRecord = bundle.Records.First(r => r.RuleId == "shape-coefficient-match");
        Assert.False(shapeRecord.Passed);
    }

    [Fact]
    public void ValidateManifest_ValidManifest_Passes()
    {
        var manifest = TestHelpers.CreateValidManifest();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateManifest(branch, manifest);

        Assert.True(bundle.AllPassed, FormatFailures(bundle));
        Assert.Contains(bundle.Records, r => r.RuleId == "manifest-complete" && r.Passed);
    }

    [Fact]
    public void ValidateManifest_IncompleteManifest_Fails()
    {
        var manifest = TestHelpers.CreateIncompleteManifest();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateManifest(branch, manifest);

        Assert.False(bundle.AllPassed);
        var manifestRecord = bundle.Records.First(r => r.RuleId == "manifest-complete");
        Assert.False(manifestRecord.Passed);
    }

    [Fact]
    public void ValidateAll_ValidInputs_AllPass()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var derived = TestHelpers.CreateValidDerivedState();
        var manifest = TestHelpers.CreateValidManifest();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAll(branch, algebra, derived, manifest);

        Assert.True(bundle.AllPassed, FormatFailures(bundle));
        // Should have: jacobi-identity, antisymmetry, metric-symmetry,
        //              carrier-compatibility, shape-coefficient-match, manifest-complete
        Assert.Equal(6, bundle.Records.Count);
    }

    [Fact]
    public void ValidateAll_BrokenAlgebra_SomeFail()
    {
        var algebra = TestHelpers.CreateBrokenAlgebra();
        var derived = TestHelpers.CreateValidDerivedState();
        var manifest = TestHelpers.CreateValidManifest();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAll(branch, algebra, derived, manifest);

        Assert.False(bundle.AllPassed);
        // At least antisymmetry and metric-symmetry should fail
        Assert.True(bundle.Records.Count(r => !r.Passed) >= 2);
    }

    [Fact]
    public void ValidateWithRegistry_CustomRules_Work()
    {
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var registry = new ValidationRuleRegistry();
        registry.Add(new Gu.Validation.Rules.PredicateRule(
            "custom-check", "custom", () => true, "always passes"));

        var bundle = engine.ValidateWithRegistry(branch, registry);

        Assert.True(bundle.AllPassed);
        Assert.Single(bundle.Records);
        Assert.Equal("custom-check", bundle.Records[0].RuleId);
    }

    [Fact]
    public void ValidationRecords_HaveCorrectCategories()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var derived = TestHelpers.CreateValidDerivedState();
        var manifest = TestHelpers.CreateValidManifest();
        var branch = TestHelpers.CreateTestBranchRef();
        var engine = new ValidationEngine();

        var bundle = engine.ValidateAll(branch, algebra, derived, manifest);

        var algebraicRecords = bundle.Records.Where(r => r.Category == "algebraic-identity").ToList();
        Assert.Equal(3, algebraicRecords.Count);

        var signatureRecords = bundle.Records.Where(r => r.Category == "signature").ToList();
        Assert.Equal(3, signatureRecords.Count);
    }

    [Fact]
    public void ValidationEngine_DefaultTolerance_Is1eNeg12()
    {
        Assert.Equal(1e-12, ValidationEngine.DefaultTolerance);
    }

    private static string FormatFailures(ValidationBundle bundle)
    {
        var failures = bundle.Records.Where(r => !r.Passed).Select(r => $"[{r.RuleId}] {r.Detail}");
        return "Failed checks: " + string.Join("; ", failures);
    }
}
