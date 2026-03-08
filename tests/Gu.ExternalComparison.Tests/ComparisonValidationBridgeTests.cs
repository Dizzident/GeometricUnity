using Gu.Core;
using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class ComparisonValidationBridgeTests
{
    [Fact]
    public void ToValidationRecord_PassingComparison_ProducesPassedRecord()
    {
        var record = new ComparisonRecord
        {
            ComparisonId = "test-1",
            TemplateId = "tpl-bianchi",
            ObservableId = "bianchi-residual",
            ReferenceSourceId = "structural",
            ReferenceVersion = "v1",
            Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            ComparisonRule = "structural_match",
            ComparisonScope = "NumericalImplementation",
            Outcome = ComparisonOutcome.Pass,
            Metrics = new Dictionary<string, double> { ["maxDeviation"] = 1e-10 },
            Message = "OK",
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc",
                Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };

        var validationRecord = ComparisonValidationBridge.ToValidationRecord(record);

        Assert.True(validationRecord.Passed);
        Assert.Equal("comparison:tpl-bianchi", validationRecord.RuleId);
        Assert.Equal("external-comparison", validationRecord.Category);
        Assert.Equal(1e-10, validationRecord.MeasuredValue);
    }

    [Fact]
    public void ToValidationRecord_FailedComparison_ProducesFailedRecord()
    {
        var record = new ComparisonRecord
        {
            ComparisonId = "test-2",
            TemplateId = "tpl-fail",
            ObservableId = "curvature",
            ReferenceSourceId = "benchmark-su2",
            ReferenceVersion = "v1",
            Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            ComparisonRule = "relative_error",
            ComparisonScope = "NumericalImplementation",
            Outcome = ComparisonOutcome.Fail,
            Metrics = new Dictionary<string, double> { ["maxRelativeError"] = 0.5 },
            Message = "Too large",
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc",
                Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };

        var validationRecord = ComparisonValidationBridge.ToValidationRecord(record);

        Assert.False(validationRecord.Passed);
        Assert.Equal(0.5, validationRecord.MeasuredValue);
    }

    [Fact]
    public void ToValidationRecords_MultipleRecords_ConvertsAll()
    {
        var records = new List<ComparisonRecord>();
        for (int i = 0; i < 3; i++)
        {
            records.Add(new ComparisonRecord
            {
                ComparisonId = $"test-{i}",
                TemplateId = $"tpl-{i}",
                ObservableId = "obs",
                ReferenceSourceId = "ref",
                ReferenceVersion = "v1",
                Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
                ComparisonRule = "structural_match",
                ComparisonScope = "NumericalImplementation",
                Outcome = ComparisonOutcome.Pass,
                Metrics = new Dictionary<string, double>(),
                Message = "OK",
                PullbackOperatorId = "sigma_h_star",
                ObservationBranchId = "sigma-pullback",
                Provenance = new ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = "abc",
                    Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
                },
                ExecutedAt = DateTimeOffset.UtcNow,
            });
        }

        var validationRecords = ComparisonValidationBridge.ToValidationRecords(records);

        Assert.Equal(3, validationRecords.Count);
    }
}
