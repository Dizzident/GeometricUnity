using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Environments;

namespace Gu.Phase5.Environments.Tests;

/// <summary>
/// Round-trip serialization tests for all M48 record types.
/// </summary>
public class EnvironmentTypeSerializationTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-03-14T00:00:00Z"),
        CodeRevision = "test-rev-serial",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    [Fact]
    public void EnvironmentRecord_RoundTrips()
    {
        var report = new EnvironmentAdmissibilityReport
        {
            Level = "admissible",
            Checks = [new AdmissibilityCheck
            {
                CheckId = "mesh-valid",
                Description = "All face volumes positive",
                Passed = true,
            }],
            Passed = true,
        };

        var record = new EnvironmentRecord
        {
            EnvironmentId = "env-serial-001",
            GeometryTier = "structured",
            GeometryFingerprint = "abc123",
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = 3,
            FaceCount = 1,
            Admissibility = report,
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("env-serial-001", deserialized.EnvironmentId);
        Assert.Equal("structured", deserialized.GeometryTier);
        Assert.Equal(2, deserialized.BaseDimension);
        Assert.Equal(2, deserialized.AmbientDimension);
        Assert.Equal(3, deserialized.EdgeCount);
        Assert.Equal(1, deserialized.FaceCount);
        Assert.True(deserialized.Admissibility.Passed);
    }

    [Fact]
    public void EnvironmentCampaignSpec_RoundTrips()
    {
        var spec = new EnvironmentCampaignSpec
        {
            CampaignId = "campaign-001",
            SchemaVersion = "1.0",
            EnvironmentIds = ["env-001", "env-002"],
            BranchManifestId = "manifest-001",
            TargetQuantities = ["eigenvalue-1", "eigenvalue-2"],
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentCampaignSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("campaign-001", deserialized.CampaignId);
        Assert.Equal(2, deserialized.EnvironmentIds.Count);
        Assert.Equal(2, deserialized.TargetQuantities.Count);
    }

    [Fact]
    public void StructuredEnvironmentSpec_RoundTrips()
    {
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-structured-001",
            SchemaVersion = "1.0",
            GeneratorId = "refined-toy-2d",
            Parameters = new Dictionary<string, double> { ["refinement"] = 3 },
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<StructuredEnvironmentSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("env-structured-001", deserialized.EnvironmentId);
        Assert.Equal("refined-toy-2d", deserialized.GeneratorId);
        Assert.Equal(3.0, deserialized.Parameters["refinement"]);
    }

    [Fact]
    public void EnvironmentImportSpec_RoundTrips()
    {
        var spec = new EnvironmentImportSpec
        {
            EnvironmentId = "env-imported-001",
            SchemaVersion = "1.0",
            SourcePath = "/data/mesh.json",
            SourceFormat = "gu-json",
            GeometryTier = "imported",
            Description = "Test import",
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentImportSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("env-imported-001", deserialized.EnvironmentId);
        Assert.Equal("gu-json", deserialized.SourceFormat);
        Assert.Equal("imported", deserialized.GeometryTier);
    }

    // ─── WP-10: imported environment provenance ───

    [Fact]
    public void ImportedEnvironmentRecord_PreservesDatasetProvenance()
    {
        var report = new EnvironmentAdmissibilityReport
        {
            Level = "admissible",
            Checks = [new AdmissibilityCheck { CheckId = "mesh-valid", Description = "ok", Passed = true }],
            Passed = true,
        };
        var record = new EnvironmentRecord
        {
            EnvironmentId = "env-imported-wp10",
            GeometryTier = "imported",
            GeometryFingerprint = "fp-abc",
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = 3,
            FaceCount = 1,
            Admissibility = report,
            DatasetId = "dataset-external-001",
            SourceHash = "sha256-aabbccdd",
            ConversionVersion = "converter-v1.2",
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("dataset-external-001", deserialized.DatasetId);
        Assert.Equal("sha256-aabbccdd", deserialized.SourceHash);
        Assert.Equal("converter-v1.2", deserialized.ConversionVersion);
    }

    [Fact]
    public void EnvironmentImportSpec_SourceHash_RoundTrips()
    {
        var spec = new EnvironmentImportSpec
        {
            EnvironmentId = "env-imported-hash",
            SchemaVersion = "1.0",
            SourcePath = "/data/mesh.json",
            SourceFormat = "gu-json",
            GeometryTier = "imported",
            DatasetId = "dataset-hash-test",
            SourceHash = "sha256-deadbeef",
            ConversionVersion = "v2.0",
            Provenance = TestProvenance(),
        };

        string json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentImportSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("sha256-deadbeef", deserialized.SourceHash);
        Assert.Equal("dataset-hash-test", deserialized.DatasetId);
        Assert.Equal("v2.0", deserialized.ConversionVersion);
    }

    [Fact]
    public void AdmissibilityCheck_WithOptionalFields_RoundTrips()
    {
        var check = new AdmissibilityCheck
        {
            CheckId = "mesh-valid",
            Description = "Positive volumes",
            Passed = true,
            Value = 0.5,
            Threshold = 1e-14,
        };

        string json = GuJsonDefaults.Serialize(check);
        var deserialized = GuJsonDefaults.Deserialize<AdmissibilityCheck>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("mesh-valid", deserialized.CheckId);
        Assert.True(deserialized.Passed);
        Assert.Equal(0.5, deserialized.Value);
        Assert.Equal(1e-14, deserialized.Threshold);
    }
}
