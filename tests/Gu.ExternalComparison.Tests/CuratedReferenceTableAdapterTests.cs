using Gu.Core;
using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class CuratedReferenceTableAdapterTests
{
    private sealed class TestReferenceSource : IReferenceDataSource
    {
        private readonly Dictionary<string, ReferenceDataEntry> _data = new();

        public void Add(string sourceId, string observableId, double[] values, string version = "v1")
        {
            _data[$"{sourceId}:{observableId}"] = new ReferenceDataEntry
            {
                SourceId = sourceId,
                Version = version,
                ObservableId = observableId,
                Values = values,
            };
        }

        public ReferenceDataEntry? GetReference(string referenceSourceId, string observableId)
        {
            _data.TryGetValue($"{referenceSourceId}:{observableId}", out var entry);
            return entry;
        }
    }

    private static BranchManifest CreateManifest() => new()
    {
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "abc",
        ActiveGeometryBranch = "g",
        ActiveObservationBranch = "o",
        ActiveTorsionBranch = "t",
        ActiveShiabBranch = "s",
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
        ObservableId = "mass_ratio",
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

    [Fact]
    public void RelativeError_WithinTolerance_Passes()
    {
        var source = new TestReferenceSource();
        source.Add("PDG-2024", "mass_ratio", new double[] { 1.0, 2.0, 3.0 });

        var adapter = new CuratedReferenceTableAdapter(source);
        var snapshot = CreateSnapshot(new double[] { 1.001, 2.002, 3.003 });
        var template = new ComparisonTemplate
        {
            TemplateId = "mass-ratio-check",
            AdapterType = "curated_table",
            ObservableId = "mass_ratio",
            ReferenceSourceId = "PDG-2024",
            ComparisonRule = "relative_error",
            ComparisonScope = "AuxiliaryEffectiveModel",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "RelativeDeviation",
                BaseValue = 0.01,
            },
            FalsifierCondition = "F-SOFT-04",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
        Assert.True(record.Metrics["maxRelativeError"] < 0.01);
    }

    [Fact]
    public void RelativeError_ExceedsTolerance_Fails()
    {
        var source = new TestReferenceSource();
        source.Add("PDG-2024", "mass_ratio", new double[] { 1.0, 2.0, 3.0 });

        var adapter = new CuratedReferenceTableAdapter(source);
        var snapshot = CreateSnapshot(new double[] { 1.5, 2.5, 3.5 }); // 50% off
        var template = new ComparisonTemplate
        {
            TemplateId = "mass-ratio-check",
            AdapterType = "curated_table",
            ObservableId = "mass_ratio",
            ReferenceSourceId = "PDG-2024",
            ComparisonRule = "relative_error",
            ComparisonScope = "AuxiliaryEffectiveModel",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "RelativeDeviation",
                BaseValue = 0.01,
            },
            FalsifierCondition = "F-SOFT-04",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateManifest());

        Assert.Equal(ComparisonOutcome.Fail, record.Outcome);
    }

    [Fact]
    public void OrderOfMagnitude_WithinRange_Passes()
    {
        var source = new TestReferenceSource();
        source.Add("literature", "coupling", new double[] { 0.1 });

        var adapter = new CuratedReferenceTableAdapter(source);
        var snapshot = CreateSnapshot(new double[] { 0.08 }); // within 1 order
        var template = new ComparisonTemplate
        {
            TemplateId = "coupling-check",
            AdapterType = "curated_table",
            ObservableId = "coupling",
            ReferenceSourceId = "literature",
            ComparisonRule = "order_of_magnitude",
            ComparisonScope = "AuxiliaryEffectiveModel",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "OrderEstimate",
                BaseValue = 1.0, // within 1 order of magnitude
            },
            FalsifierCondition = "F-SOFT-04",
            MinimumOutputType = OutputType.SemiQuantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateManifest());

        Assert.Equal(ComparisonOutcome.Pass, record.Outcome);
    }

    [Fact]
    public void MissingReference_ReturnsInvalid()
    {
        var source = new TestReferenceSource();
        var adapter = new CuratedReferenceTableAdapter(source);
        var snapshot = CreateSnapshot(new double[] { 1.0 });
        var template = new ComparisonTemplate
        {
            TemplateId = "missing-ref",
            AdapterType = "curated_table",
            ObservableId = "mass_ratio",
            ReferenceSourceId = "nonexistent-source",
            ComparisonRule = "relative_error",
            ComparisonScope = "AuxiliaryEffectiveModel",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "RelativeDeviation",
                BaseValue = 0.01,
            },
            FalsifierCondition = "F-SOFT-04",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateManifest());

        Assert.Equal(ComparisonOutcome.Invalid, record.Outcome);
        Assert.Contains("not found", record.Message);
    }

    [Fact]
    public void ReferenceVersion_AppearsInRecord()
    {
        var source = new TestReferenceSource();
        source.Add("PDG-2024", "mass_ratio", new double[] { 1.0 }, version: "2024.1");

        var adapter = new CuratedReferenceTableAdapter(source);
        var snapshot = CreateSnapshot(new double[] { 1.0 });
        var template = new ComparisonTemplate
        {
            TemplateId = "version-check",
            AdapterType = "curated_table",
            ObservableId = "mass_ratio",
            ReferenceSourceId = "PDG-2024",
            ComparisonRule = "relative_error",
            ComparisonScope = "AuxiliaryEffectiveModel",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "RelativeDeviation",
                BaseValue = 0.01,
            },
            FalsifierCondition = "F-SOFT-04",
            MinimumOutputType = OutputType.Quantitative,
        };

        var record = adapter.Compare(snapshot, template, CreateManifest());

        Assert.Equal("2024.1", record.ReferenceVersion);
    }
}
