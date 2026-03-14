using System.Text.Json;
using Gu.Core;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Tests for FalsifierRecord and FalsifierSummary serialization (M50).
/// </summary>
public sealed class FalsifierRecordTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    [Fact]
    public void FalsifierRecord_RoundTrips_Json()
    {
        var record = new FalsifierRecord
        {
            FalsifierId = "falsifier-0001",
            FalsifierType = FalsifierTypes.BranchFragility,
            Severity = FalsifierSeverity.High,
            TargetId = "q-residual",
            BranchId = "branch-v1",
            EnvironmentId = null,
            TriggerValue = 0.75,
            Threshold = 0.5,
            Description = "Quantity q-residual is fragile across branch family.",
            Evidence = "FragilityRecord targetQuantityId=q-residual",
            Active = true,
            Provenance = MakeProvenance(),
        };

        var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
        var deserialized = JsonSerializer.Deserialize<FalsifierRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("falsifier-0001", deserialized.FalsifierId);
        Assert.Equal(FalsifierTypes.BranchFragility, deserialized.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, deserialized.Severity);
        Assert.Equal("q-residual", deserialized.TargetId);
        Assert.Equal(0.75, deserialized.TriggerValue);
        Assert.True(deserialized.Active);
    }

    [Fact]
    public void FalsifierRecord_OptionalFields_NullWhenAbsent()
    {
        var record = new FalsifierRecord
        {
            FalsifierId = "falsifier-0002",
            FalsifierType = FalsifierTypes.NonConvergence,
            Severity = FalsifierSeverity.High,
            TargetId = "q-lambda",
            BranchId = "branch-v1",
            Description = "Non-convergent.",
            Evidence = "ConvergenceFailureRecord",
            Active = true,
            Provenance = MakeProvenance(),
        };

        Assert.Null(record.EnvironmentId);
        Assert.Null(record.TriggerValue);
        Assert.Null(record.Threshold);
    }

    [Fact]
    public void FalsifierSummary_RoundTrips_Json()
    {
        var summary = new FalsifierSummary
        {
            StudyId = "study-test",
            Falsifiers = new[]
            {
                new FalsifierRecord
                {
                    FalsifierId = "f-001",
                    FalsifierType = FalsifierTypes.NonConvergence,
                    Severity = FalsifierSeverity.High,
                    TargetId = "q",
                    BranchId = "b1",
                    Description = "Non-convergent.",
                    Evidence = "failure",
                    Active = true,
                    Provenance = MakeProvenance(),
                },
            },
            ActiveFatalCount = 0,
            ActiveHighCount = 1,
            TotalActiveCount = 1,
            Provenance = MakeProvenance(),
        };

        var json = summary.ToJson();
        var deserialized = FalsifierSummary.FromJson(json);

        Assert.NotNull(deserialized);
        Assert.Equal("study-test", deserialized.StudyId);
        Assert.Single(deserialized.Falsifiers);
        Assert.Equal(1, deserialized.ActiveHighCount);
        Assert.Equal(1, deserialized.TotalActiveCount);
    }

    [Fact]
    public void FalsifierSummary_EmptyFalsifiers_CountsAreZero()
    {
        var summary = new FalsifierSummary
        {
            StudyId = "study-clean",
            Falsifiers = Array.Empty<FalsifierRecord>(),
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            Provenance = MakeProvenance(),
        };

        Assert.Equal(0, summary.ActiveFatalCount);
        Assert.Equal(0, summary.TotalActiveCount);
        Assert.Empty(summary.Falsifiers);
    }
}
