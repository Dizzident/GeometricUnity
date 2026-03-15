using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Phase5.Environments;

namespace Gu.Phase5.Environments.Tests;

/// <summary>
/// WP-10: Tests that imported environments require and preserve external provenance fields.
/// </summary>
public class EnvironmentImporterProvenanceTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-03-14T00:00:00Z"),
        CodeRevision = "wp10-test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static string WriteGuJsonMesh(string path)
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(rows: 1, cols: 1);
        string json = GuJsonDefaults.Serialize(mesh);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Import_ImportedTier_WithFullProvenance_PreservesDatasetProvenance()
    {
        string tmpPath = Path.GetTempFileName();
        try
        {
            WriteGuJsonMesh(tmpPath);

            var spec = new EnvironmentImportSpec
            {
                EnvironmentId = "env-wp10-ok",
                SchemaVersion = "1.0",
                SourcePath = tmpPath,
                SourceFormat = "gu-json",
                GeometryTier = "imported",
                DatasetId = "dataset-external-001",
                SourceHash = "sha256-aabbccdd",
                ConversionVersion = "converter-v1.2",
                Provenance = TestProvenance(),
            };

            var record = EnvironmentImporter.Import(spec);

            Assert.Equal("dataset-external-001", record.DatasetId);
            Assert.Equal("sha256-aabbccdd", record.SourceHash);
            Assert.Equal("converter-v1.2", record.ConversionVersion);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public void Import_ImportedTier_SourceHashRoundTrips()
    {
        string tmpPath = Path.GetTempFileName();
        try
        {
            WriteGuJsonMesh(tmpPath);

            var spec = new EnvironmentImportSpec
            {
                EnvironmentId = "env-wp10-hash",
                SchemaVersion = "1.0",
                SourcePath = tmpPath,
                SourceFormat = "gu-json",
                GeometryTier = "imported",
                DatasetId = "dataset-hash-test",
                SourceHash = "sha256-deadbeef12345678",
                ConversionVersion = "v2.0",
                Provenance = TestProvenance(),
            };

            var record = EnvironmentImporter.Import(spec);

            // Source hash round-trips through the record unchanged
            Assert.Equal("sha256-deadbeef12345678", record.SourceHash);
            Assert.Equal(spec.SourceHash, record.SourceHash);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public void Import_ImportedTier_MissingProvenanceFields_FailsValidation()
    {
        string tmpPath = Path.GetTempFileName();
        try
        {
            WriteGuJsonMesh(tmpPath);

            // Missing DatasetId — should throw
            var spec = new EnvironmentImportSpec
            {
                EnvironmentId = "env-wp10-missing",
                SchemaVersion = "1.0",
                SourcePath = tmpPath,
                SourceFormat = "gu-json",
                GeometryTier = "imported",
                // DatasetId, SourceHash, ConversionVersion all missing
                Provenance = TestProvenance(),
            };

            var ex = Assert.Throws<InvalidOperationException>(() => EnvironmentImporter.Import(spec));
            Assert.Contains("DatasetId", ex.Message);
            Assert.Contains("imported", ex.Message);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }

    [Fact]
    public void Import_NonImportedTier_MissingProvenanceFields_Succeeds()
    {
        string tmpPath = Path.GetTempFileName();
        try
        {
            WriteGuJsonMesh(tmpPath);

            // For non-imported tier, provenance fields are optional
            var spec = new EnvironmentImportSpec
            {
                EnvironmentId = "env-wp10-structured",
                SchemaVersion = "1.0",
                SourcePath = tmpPath,
                SourceFormat = "gu-json",
                GeometryTier = "structured",
                // No DatasetId/SourceHash/ConversionVersion
                Provenance = TestProvenance(),
            };

            var record = EnvironmentImporter.Import(spec);

            Assert.Equal("env-wp10-structured", record.EnvironmentId);
            Assert.Null(record.DatasetId);
            Assert.Null(record.SourceHash);
            Assert.Null(record.ConversionVersion);
        }
        finally
        {
            File.Delete(tmpPath);
        }
    }
}
