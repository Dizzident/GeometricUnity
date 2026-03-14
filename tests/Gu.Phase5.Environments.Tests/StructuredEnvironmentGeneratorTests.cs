using Gu.Core;
using Gu.Phase5.Environments;

namespace Gu.Phase5.Environments.Tests;

public class StructuredEnvironmentGeneratorTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-03-14T00:00:00Z"),
        CodeRevision = "test-rev",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    [Fact]
    public void RefinedToy2D_ProducesAdmissibleRecord()
    {
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-refined-2d-001",
            SchemaVersion = "1.0",
            GeneratorId = "refined-toy-2d",
            Parameters = new Dictionary<string, double> { ["refinement"] = 3 },
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        var record = StructuredEnvironmentGenerator.Generate(spec);

        Assert.Equal("env-refined-2d-001", record.EnvironmentId);
        Assert.Equal("structured", record.GeometryTier);
        Assert.Equal(2, record.BaseDimension);
        Assert.Equal(2, record.AmbientDimension);
        Assert.True(record.EdgeCount > 0);
        Assert.True(record.FaceCount > 0);
        Assert.True(record.Admissibility.Passed);
        Assert.NotEmpty(record.GeometryFingerprint);
    }

    [Fact]
    public void Structured3D_ProducesAdmissibleRecord()
    {
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-structured-3d-001",
            SchemaVersion = "1.0",
            GeneratorId = "structured-3d",
            Parameters = new Dictionary<string, double> { ["n"] = 2 },
            BaseDimension = 3,
            AmbientDimension = 3,
            Provenance = TestProvenance(),
        };

        var record = StructuredEnvironmentGenerator.Generate(spec);

        Assert.Equal("env-structured-3d-001", record.EnvironmentId);
        Assert.Equal(3, record.BaseDimension);
        Assert.Equal(3, record.AmbientDimension);
        Assert.True(record.EdgeCount > 0);
        Assert.True(record.FaceCount > 0);
        Assert.NotEmpty(record.GeometryFingerprint);
    }

    [Fact]
    public void FlatTorus2D_ProducesAdmissibleRecord()
    {
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-flat-torus-001",
            SchemaVersion = "1.0",
            GeneratorId = "flat-torus-2d",
            Parameters = new Dictionary<string, double> { ["size"] = 3 },
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        var record = StructuredEnvironmentGenerator.Generate(spec);

        Assert.Equal("env-flat-torus-001", record.EnvironmentId);
        Assert.Equal("structured", record.GeometryTier);
        Assert.True(record.Admissibility.Passed);
    }

    [Fact]
    public void UnknownGenerator_ThrowsNotSupported()
    {
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-unknown",
            SchemaVersion = "1.0",
            GeneratorId = "nonexistent-generator",
            Parameters = new Dictionary<string, double>(),
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        Assert.Throws<NotSupportedException>(() => StructuredEnvironmentGenerator.Generate(spec));
    }

    [Fact]
    public void RefinedToy2D_DefaultRefinement_ProducesRecord()
    {
        // No "refinement" parameter -- uses default of 2
        var spec = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-default-refinement",
            SchemaVersion = "1.0",
            GeneratorId = "refined-toy-2d",
            Parameters = new Dictionary<string, double>(),
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        var record = StructuredEnvironmentGenerator.Generate(spec);

        Assert.True(record.FaceCount > 0);
    }

    [Fact]
    public void DifferentRefinementLevels_ProduceDifferentFingerprints()
    {
        var spec1 = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-r2",
            SchemaVersion = "1.0",
            GeneratorId = "refined-toy-2d",
            Parameters = new Dictionary<string, double> { ["refinement"] = 2 },
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        var spec2 = new StructuredEnvironmentSpec
        {
            EnvironmentId = "env-r4",
            SchemaVersion = "1.0",
            GeneratorId = "refined-toy-2d",
            Parameters = new Dictionary<string, double> { ["refinement"] = 4 },
            BaseDimension = 2,
            AmbientDimension = 2,
            Provenance = TestProvenance(),
        };

        var rec1 = StructuredEnvironmentGenerator.Generate(spec1);
        var rec2 = StructuredEnvironmentGenerator.Generate(spec2);

        // Larger refinement produces more faces
        Assert.True(rec2.FaceCount > rec1.FaceCount);
        // Different meshes get different fingerprints
        Assert.NotEqual(rec1.GeometryFingerprint, rec2.GeometryFingerprint);
    }
}
