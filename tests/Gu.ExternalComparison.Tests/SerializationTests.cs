using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class SerializationTests
{
    [Fact]
    public void TolerancePolicy_RoundTrips()
    {
        var original = new TolerancePolicy
        {
            BaseToleranceType = "RelativeDeviation",
            BaseValue = 1e-6,
            BranchSensitivity = "High",
            MeshDependence = "Converging",
            Notes = "Test note",
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<TolerancePolicy>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.BaseToleranceType, deserialized.BaseToleranceType);
        Assert.Equal(original.BaseValue, deserialized.BaseValue);
        Assert.Equal(original.BranchSensitivity, deserialized.BranchSensitivity);
        Assert.Equal(original.MeshDependence, deserialized.MeshDependence);
        Assert.Equal(original.Notes, deserialized.Notes);
    }

    [Fact]
    public void FalsifierCheck_RoundTrips()
    {
        var original = FalsifierRegistry.BianchiViolation;

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<FalsifierCheck>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Category, deserialized.Category);
        Assert.Equal(original.Severity, deserialized.Severity);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.BranchDependence, deserialized.BranchDependence);
        Assert.Equal(original.Tolerance.BaseValue, deserialized.Tolerance.BaseValue);
    }

    [Fact]
    public void ComparisonTemplate_RoundTrips()
    {
        var original = new ComparisonTemplate
        {
            TemplateId = "tpl-bianchi-01",
            AdapterType = "structural_fact",
            ObservableId = "bianchi-residual",
            ReferenceSourceId = "structural",
            ComparisonRule = "structural_match",
            ComparisonScope = "NumericalImplementation",
            Tolerance = new TolerancePolicy
            {
                BaseToleranceType = "RelativeDeviation",
                BaseValue = 1e-6,
            },
            FalsifierCondition = "F-HARD-01",
            MinimumOutputType = OutputType.Quantitative,
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ComparisonTemplate>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.TemplateId, deserialized.TemplateId);
        Assert.Equal(original.AdapterType, deserialized.AdapterType);
        Assert.Equal(original.ObservableId, deserialized.ObservableId);
        Assert.Equal(original.ReferenceSourceId, deserialized.ReferenceSourceId);
        Assert.Equal(original.ComparisonRule, deserialized.ComparisonRule);
        Assert.Equal(original.ComparisonScope, deserialized.ComparisonScope);
        Assert.Equal(original.FalsifierCondition, deserialized.FalsifierCondition);
        Assert.Equal(original.MinimumOutputType, deserialized.MinimumOutputType);
        Assert.Equal(original.Tolerance.BaseValue, deserialized.Tolerance.BaseValue);
    }

    [Fact]
    public void ComparisonRecord_RoundTrips()
    {
        var original = new ComparisonRecord
        {
            ComparisonId = "cmp-001",
            TemplateId = "tpl-bianchi-01",
            ObservableId = "bianchi-residual",
            ReferenceSourceId = "structural",
            ReferenceVersion = "v1.0",
            Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            ComparisonRule = "structural_match",
            ComparisonScope = "NumericalImplementation",
            Outcome = ComparisonOutcome.Pass,
            Metrics = new Dictionary<string, double>
            {
                ["maxDeviation"] = 1e-10,
                ["l2Norm"] = 2e-10,
            },
            Message = "Bianchi identity satisfied",
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            },
            ExecutedAt = new DateTimeOffset(2026, 1, 15, 12, 0, 1, TimeSpan.Zero),
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ComparisonRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ComparisonId, deserialized.ComparisonId);
        Assert.Equal(original.TemplateId, deserialized.TemplateId);
        Assert.Equal(original.Outcome, deserialized.Outcome);
        Assert.Equal(original.PullbackOperatorId, deserialized.PullbackOperatorId);
        Assert.Equal(original.ObservationBranchId, deserialized.ObservationBranchId);
        Assert.Equal(original.Message, deserialized.Message);
        Assert.Equal(original.Branch.BranchId, deserialized.Branch.BranchId);
        Assert.Equal(1e-10, deserialized.Metrics["maxDeviation"]);
        Assert.Equal(2e-10, deserialized.Metrics["l2Norm"]);
    }

    [Fact]
    public void ComparisonOutcome_SerializesAsString()
    {
        var record = new ComparisonRecord
        {
            ComparisonId = "x",
            TemplateId = "t",
            ObservableId = "o",
            ReferenceSourceId = "r",
            ReferenceVersion = "v",
            Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            ComparisonRule = "rule",
            ComparisonScope = "scope",
            Outcome = ComparisonOutcome.Fail,
            Metrics = new Dictionary<string, double>(),
            Message = "msg",
            PullbackOperatorId = "p",
            ObservationBranchId = "ob",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "c",
                Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(record);
        Assert.Contains("\"Fail\"", json);
    }

    [Fact]
    public void OutputType_EnumOrdering_SupportsGuard()
    {
        // Verify that ExactStructural < SemiQuantitative < Quantitative
        // This is critical for the MinimumOutputType guard
        Assert.True(OutputType.ExactStructural < OutputType.SemiQuantitative);
        Assert.True(OutputType.SemiQuantitative < OutputType.Quantitative);
    }
}
