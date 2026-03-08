using Gu.Core;
using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class StructuralFactAdapterTests
{
    private static BranchManifest CreateTestManifest() => new()
    {
        BranchId = "test-branch",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static ObservableSnapshot CreateSnapshot(double[] values) => new()
    {
        ObservableId = "bianchi-residual",
        OutputType = OutputType.Quantitative,
        Values = values,
        Provenance = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            IsVerified = true,
            PipelineTimestamp = DateTimeOffset.UtcNow,
        },
    };

    private static ComparisonTemplate CreateStructuralTemplate(
        string rule = "structural_match",
        double tolerance = 1e-4) => new()
    {
        TemplateId = "test-structural",
        AdapterType = "structural_fact",
        ObservableId = "bianchi-residual",
        ReferenceSourceId = "structural",
        ComparisonRule = rule,
        ComparisonScope = "NumericalImplementation",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = tolerance,
        },
        FalsifierCondition = "F-HARD-01",
        MinimumOutputType = OutputType.Quantitative,
    };

    [Fact]
    public void CanHandle_MatchingType_ReturnsTrue()
    {
        var adapter = new StructuralFactAdapter();
        Assert.True(adapter.CanHandle(CreateStructuralTemplate()));
    }

    [Fact]
    public void CanHandle_WrongType_ReturnsFalse()
    {
        var adapter = new StructuralFactAdapter();
        var template = new ComparisonTemplate
        {
            TemplateId = "test",
            AdapterType = "curated_table",
            ObservableId = "x",
            ReferenceSourceId = "y",
            ComparisonRule = "relative_error",
            ComparisonScope = "NumericalImplementation",
            Tolerance = new TolerancePolicy { BaseToleranceType = "RelativeDeviation", BaseValue = 0.01 },
            FalsifierCondition = "F-HARD-01",
            MinimumOutputType = OutputType.Quantitative,
        };
        Assert.False(adapter.CanHandle(template));
    }

    [Fact]
    public void StructuralMatch_SmallValues_Passes()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1e-8, -2e-9, 5e-10, 0.0 });
        var template = CreateStructuralTemplate("structural_match", 1e-4);

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
        Assert.True(record.Metrics["maxDeviation"] < 1e-4);
    }

    [Fact]
    public void StructuralMatch_LargeValues_Fails()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 0.5, -0.3, 0.1 });
        var template = CreateStructuralTemplate("structural_match", 1e-4);

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Fail, record.Outcome);
        Assert.True(record.Metrics["maxDeviation"] > 1e-4);
    }

    [Fact]
    public void NormBound_WithinTolerance_Passes()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1e-6, 1e-6, 1e-6 });
        var template = CreateStructuralTemplate("norm_bound", 1e-3);

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
    }

    [Fact]
    public void IntegerCheck_IntegerValues_Passes()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1.0, 2.0, -3.0, 0.0 });
        var template = CreateStructuralTemplate("integer_check", 1e-4);

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
        Assert.True(record.Metrics["maxFractionalPart"] < 1e-10);
    }

    [Fact]
    public void IntegerCheck_NonIntegerValues_Fails()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1.5 });
        var template = CreateStructuralTemplate("integer_check", 1e-4);

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Fail, record.Outcome);
    }

    [Fact]
    public void CountMatch_CorrectCount_Passes()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 3.0 });
        var template = new ComparisonTemplate
        {
            TemplateId = "count-test",
            AdapterType = "structural_fact",
            ObservableId = "bianchi-residual",
            ReferenceSourceId = "structural",
            ComparisonRule = "count_match",
            ComparisonScope = "NumericalImplementation",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "FactorBound",
                BaseValue = 3.0, // expected count
            },
            FalsifierCondition = "F-HARD-03",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
    }

    [Fact]
    public void CountMatch_WrongCount_Fails()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 5.0 });
        var template = new ComparisonTemplate
        {
            TemplateId = "count-test",
            AdapterType = "structural_fact",
            ObservableId = "bianchi-residual",
            ReferenceSourceId = "structural",
            ComparisonRule = "count_match",
            ComparisonScope = "NumericalImplementation",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "FactorBound",
                BaseValue = 3.0, // expected 3, got 5
            },
            FalsifierCondition = "F-HARD-03",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Fail, record.Outcome);
    }

    [Fact]
    public void UnknownRule_ReturnsInvalid()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1.0 });
        var template = CreateStructuralTemplate("unknown_rule");

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal(ComparisonOutcome.Invalid, record.Outcome);
        Assert.Contains("Unknown comparison rule", record.Message);
    }

    [Fact]
    public void Compare_PopulatesProvenanceFields()
    {
        var adapter = new StructuralFactAdapter();
        var snapshot = CreateSnapshot(new double[] { 1e-10 });
        var template = CreateStructuralTemplate();

        var record = adapter.Compare(snapshot, template, CreateTestManifest());

        Assert.Equal("sigma_h_star", record.PullbackOperatorId);
        Assert.Equal("sigma-pullback", record.ObservationBranchId);
        Assert.Equal("test-branch", record.Branch.BranchId);
    }
}
