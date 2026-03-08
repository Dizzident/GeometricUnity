using Gu.Core;
using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class ComparisonEngineTests
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
        InsertedAssumptionIds = new[] { "IA-1", "IA-2", "IA-3", "IA-4", "IA-5", "IA-6" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2", "IX-3", "IX-4", "IX-5" },
    };

    private static ObservableSnapshot CreateVerifiedSnapshot(
        string observableId = "residual",
        OutputType outputType = OutputType.Quantitative,
        double[]? values = null) => new()
    {
        ObservableId = observableId,
        OutputType = outputType,
        Values = values ?? new double[] { 1e-8, 2e-8, 3e-8 },
        Provenance = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            IsVerified = true,
            PipelineTimestamp = DateTimeOffset.UtcNow,
        },
    };

    private static ObservedState CreateObservedState(params ObservableSnapshot[] snapshots)
    {
        var observables = new Dictionary<string, ObservableSnapshot>();
        foreach (var s in snapshots)
            observables[s.ObservableId] = s;

        return new ObservedState
        {
            ObservationBranchId = "sigma-pullback",
            Observables = observables,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            },
        };
    }

    private static ComparisonTemplate CreateTemplate(
        string observableId = "residual",
        string adapterType = "structural_fact",
        string comparisonRule = "structural_match",
        OutputType minimumOutputType = OutputType.Quantitative) => new()
    {
        TemplateId = $"test-{Guid.NewGuid():N}",
        AdapterType = adapterType,
        ObservableId = observableId,
        ReferenceSourceId = "structural",
        ComparisonRule = comparisonRule,
        ComparisonScope = "NumericalImplementation",
        Tolerance = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-4,
        },
        FalsifierCondition = "F-HARD-01",
        MinimumOutputType = minimumOutputType,
    };

    [Fact]
    public void Engine_WithMatchingAdapter_ProducesResult()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });
        var observed = CreateObservedState(CreateVerifiedSnapshot());
        var templates = new[] { CreateTemplate() };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Pass, records[0].Outcome);
    }

    [Fact]
    public void Engine_MissingObservable_ReturnsInvalid()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });
        var observed = CreateObservedState(CreateVerifiedSnapshot("curvature"));
        var templates = new[] { CreateTemplate("nonexistent") };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Invalid, records[0].Outcome);
        Assert.Contains("not found", records[0].Message);
    }

    [Fact]
    public void Engine_UnverifiedProvenance_ReturnsInvalid()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });

        var unverifiedSnapshot = new ObservableSnapshot
        {
            ObservableId = "residual",
            OutputType = OutputType.Quantitative,
            Values = new double[] { 1e-8 },
            Provenance = new ObservationProvenance
            {
                PullbackOperatorId = "sigma_h_star",
                ObservationBranchId = "sigma-pullback",
                IsVerified = false, // NOT verified
                PipelineTimestamp = DateTimeOffset.UtcNow,
            },
        };

        var observed = CreateObservedState(unverifiedSnapshot);
        var templates = new[] { CreateTemplate() };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Invalid, records[0].Outcome);
        Assert.Contains("sigma_h*", records[0].Message);
    }

    [Fact]
    public void Engine_NullProvenance_ReturnsInvalid()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });

        var noProvenanceSnapshot = new ObservableSnapshot
        {
            ObservableId = "residual",
            OutputType = OutputType.Quantitative,
            Values = new double[] { 1e-8 },
            Provenance = null, // no provenance
        };

        var observed = CreateObservedState(noProvenanceSnapshot);
        var templates = new[] { CreateTemplate() };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Invalid, records[0].Outcome);
    }

    [Fact]
    public void Engine_OutputTypeMismatch_ReturnsInvalid()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });

        // Observable is ExactStructural, but template requires Quantitative
        var structuralSnapshot = CreateVerifiedSnapshot("residual", OutputType.ExactStructural);
        var observed = CreateObservedState(structuralSnapshot);
        var templates = new[] { CreateTemplate(minimumOutputType: OutputType.Quantitative) };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Invalid, records[0].Outcome);
        Assert.Contains("Output type mismatch", records[0].Message);
    }

    [Fact]
    public void Engine_NoMatchingAdapter_ReturnsInvalid()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });
        var observed = CreateObservedState(CreateVerifiedSnapshot());
        var templates = new[] { CreateTemplate(adapterType: "unknown_adapter") };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Invalid, records[0].Outcome);
        Assert.Contains("No adapter found", records[0].Message);
    }

    [Fact]
    public void Engine_MultipleTemplates_ProcessesAll()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });
        var observed = CreateObservedState(
            CreateVerifiedSnapshot("residual"),
            CreateVerifiedSnapshot("curvature"));
        var templates = new[]
        {
            CreateTemplate("residual"),
            CreateTemplate("curvature"),
        };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(ComparisonOutcome.Pass, r.Outcome));
    }

    [Fact]
    public void Engine_FailedComparisonsArePreserved()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });

        // Large values that will fail the structural match
        var badSnapshot = CreateVerifiedSnapshot("residual", values: new double[] { 1.0, 2.0, 3.0 });
        var observed = CreateObservedState(badSnapshot);
        var templates = new[] { CreateTemplate() };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        Assert.Single(records);
        Assert.Equal(ComparisonOutcome.Fail, records[0].Outcome);
        // Failures are first-class results, not filtered
        Assert.NotEmpty(records[0].Message);
    }

    [Fact]
    public void Engine_RecordContainsProvenanceChain()
    {
        var engine = new ComparisonEngine(new IComparisonAdapter[] { new StructuralFactAdapter() });
        var observed = CreateObservedState(CreateVerifiedSnapshot());
        var templates = new[] { CreateTemplate() };
        var branch = CreateTestManifest();

        var records = engine.RunComparisons(observed, templates, branch);

        var record = records[0];
        Assert.Equal("sigma_h_star", record.PullbackOperatorId);
        Assert.Equal("sigma-pullback", record.ObservationBranchId);
        Assert.Equal("test-branch", record.Branch.BranchId);
        Assert.NotNull(record.Provenance);
    }
}
