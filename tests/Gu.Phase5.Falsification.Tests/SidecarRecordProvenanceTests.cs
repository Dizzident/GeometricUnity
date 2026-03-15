using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Task #25: Verifies that ProvenanceMeta round-trips correctly through JSON
/// serialization for EnvironmentVarianceRecord, RepresentationContentRecord,
/// and CouplingConsistencyRecord.
/// </summary>
public sealed class SidecarRecordProvenanceTests
{
    private static ProvenanceMeta MakeProvenance(string backend) => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 3, 14, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = backend,
    };

    [Fact]
    public void EnvironmentVarianceRecord_Provenance_RoundTrips()
    {
        var record = new EnvironmentVarianceRecord
        {
            RecordId = "evr-001",
            QuantityId = "bosonic-eigenvalue-ratio-1",
            EnvironmentTierId = "toy",
            RelativeStdDev = 0.12,
            Flagged = false,
            Notes = null,
            Provenance = MakeProvenance("cpu-reference"),
        };

        var json = GuJsonDefaults.Serialize(record);
        var rt = GuJsonDefaults.Deserialize<EnvironmentVarianceRecord>(json);

        Assert.NotNull(rt);
        Assert.NotNull(rt.Provenance);
        Assert.Equal("cpu-reference", rt.Provenance.Backend);
        Assert.Equal("abc123", rt.Provenance.CodeRevision);
        Assert.Equal("test-branch", rt.Provenance.Branch.BranchId);
        Assert.Equal(0.12, rt.RelativeStdDev);
    }

    [Fact]
    public void EnvironmentVarianceRecord_NullProvenance_SerializesWithoutField()
    {
        var record = new EnvironmentVarianceRecord
        {
            RecordId = "evr-002",
            QuantityId = "bosonic-eigenvalue-ratio-1",
            EnvironmentTierId = "toy",
            RelativeStdDev = 0.05,
        };

        var json = GuJsonDefaults.Serialize(record);

        Assert.DoesNotContain("provenance", json, StringComparison.OrdinalIgnoreCase);

        var rt = GuJsonDefaults.Deserialize<EnvironmentVarianceRecord>(json);
        Assert.NotNull(rt);
        Assert.Null(rt.Provenance);
    }

    [Fact]
    public void RepresentationContentRecord_Provenance_RoundTrips()
    {
        var record = new RepresentationContentRecord
        {
            RecordId = "rcr-001",
            CandidateId = "candidate-0001",
            ExpectedModeCount = 3,
            ObservedModeCount = 3,
            MissingRequiredCount = 0,
            StructuralMismatchScore = 0.05,
            Consistent = true,
            InconsistencyDescription = null,
            Provenance = MakeProvenance("cpu-reference"),
        };

        var json = GuJsonDefaults.Serialize(record);
        var rt = GuJsonDefaults.Deserialize<RepresentationContentRecord>(json);

        Assert.NotNull(rt);
        Assert.NotNull(rt.Provenance);
        Assert.Equal("cpu-reference", rt.Provenance.Backend);
        Assert.Equal("abc123", rt.Provenance.CodeRevision);
        Assert.Equal(3, rt.ExpectedModeCount);
        Assert.True(rt.Consistent);
    }

    [Fact]
    public void RepresentationContentRecord_NullProvenance_SerializesWithoutField()
    {
        var record = new RepresentationContentRecord
        {
            RecordId = "rcr-002",
            CandidateId = "candidate-0002",
            ExpectedModeCount = 2,
            ObservedModeCount = 2,
            Consistent = true,
        };

        var json = GuJsonDefaults.Serialize(record);

        Assert.DoesNotContain("provenance", json, StringComparison.OrdinalIgnoreCase);

        var rt = GuJsonDefaults.Deserialize<RepresentationContentRecord>(json);
        Assert.NotNull(rt);
        Assert.Null(rt.Provenance);
    }

    [Fact]
    public void CouplingConsistencyRecord_Provenance_RoundTrips()
    {
        var record = new CouplingConsistencyRecord
        {
            RecordId = "ccr-001",
            CandidateId = "candidate-0001",
            CouplingType = "gauge",
            RelativeSpread = 0.08,
            Consistent = true,
            Notes = null,
            Provenance = MakeProvenance("cpu-reference"),
        };

        var json = GuJsonDefaults.Serialize(record);
        var rt = GuJsonDefaults.Deserialize<CouplingConsistencyRecord>(json);

        Assert.NotNull(rt);
        Assert.NotNull(rt.Provenance);
        Assert.Equal("cpu-reference", rt.Provenance.Backend);
        Assert.Equal("abc123", rt.Provenance.CodeRevision);
        Assert.Equal(0.08, rt.RelativeSpread);
        Assert.True(rt.Consistent);
    }

    [Fact]
    public void CouplingConsistencyRecord_NullProvenance_SerializesWithoutField()
    {
        var record = new CouplingConsistencyRecord
        {
            RecordId = "ccr-002",
            CandidateId = "candidate-0002",
            CouplingType = "yukawa",
            RelativeSpread = 0.02,
            Consistent = true,
        };

        var json = GuJsonDefaults.Serialize(record);

        Assert.DoesNotContain("provenance", json, StringComparison.OrdinalIgnoreCase);

        var rt = GuJsonDefaults.Deserialize<CouplingConsistencyRecord>(json);
        Assert.NotNull(rt);
        Assert.Null(rt.Provenance);
    }

    [Fact]
    public void AllThreeRecords_BackendField_PreservedInJson()
    {
        // Confirms the JSON contains the literal "backend" key when set.
        var provenance = MakeProvenance("cpu-reference");

        var envRecord = new EnvironmentVarianceRecord
        {
            RecordId = "e", QuantityId = "q", EnvironmentTierId = "t", RelativeStdDev = 0.1,
            Provenance = provenance,
        };
        var reprRecord = new RepresentationContentRecord
        {
            RecordId = "r", CandidateId = "c", ExpectedModeCount = 1, ObservedModeCount = 1,
            Consistent = true, Provenance = provenance,
        };
        var coupRecord = new CouplingConsistencyRecord
        {
            RecordId = "cc", CandidateId = "c", CouplingType = "gauge",
            RelativeSpread = 0.01, Consistent = true, Provenance = provenance,
        };

        foreach (var json in new[]
        {
            GuJsonDefaults.Serialize(envRecord),
            GuJsonDefaults.Serialize(reprRecord),
            GuJsonDefaults.Serialize(coupRecord),
        })
        {
            Assert.Contains("\"backend\"", json);
            Assert.Contains("cpu-reference", json);
        }
    }
}
