using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

/// <summary>
/// G-003 tests: environment tier separation (toy/structured/imported) in
/// BackgroundRecord, BackgroundAtlas, and BackgroundStudySpec.
/// </summary>
public class EnvironmentTierTests
{
    private static BackgroundMetrics MakeMetrics(double residual = 1e-6, double stationary = 1e-8) =>
        new BackgroundMetrics
        {
            ResidualNorm = residual,
            StationarityNorm = stationary,
            ObjectiveValue = residual * residual / 2,
            GaugeViolation = 0,
            SolverIterations = 10,
            SolverConverged = true,
            TerminationReason = "converged",
            GaussNewtonValid = false,
        };

    [Fact]
    public void BackgroundRecord_EnvironmentTier_RoundTrips()
    {
        var record = new BackgroundRecord
        {
            BackgroundId = "bg-tier-test",
            EnvironmentId = "env-structured",
            BranchManifestId = "branch-1",
            GeometryFingerprint = "structured-X_h-Y_h-centroid-P1",
            StateArtifactRef = "state-tier-test",
            ResidualNorm = 1e-6,
            StationarityNorm = 1e-8,
            AdmissibilityLevel = AdmissibilityLevel.B0,
            Metrics = MakeMetrics(),
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
            EnvironmentTier = "structured",
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("structured", deserialized.EnvironmentTier);
    }

    [Fact]
    public void BackgroundRecord_NullEnvironmentTier_IsNull()
    {
        var record = TestHelpers.MakeRecord("bg-no-tier", AdmissibilityLevel.B0);

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized.EnvironmentTier);
    }

    [Theory]
    [InlineData("toy")]
    [InlineData("structured")]
    [InlineData("imported")]
    public void BackgroundRecord_AllTiers_RoundTrip(string tier)
    {
        var record = new BackgroundRecord
        {
            BackgroundId = $"bg-{tier}",
            EnvironmentId = "env-1",
            BranchManifestId = "branch-1",
            GeometryFingerprint = "fp",
            StateArtifactRef = "state",
            ResidualNorm = 1e-6,
            StationarityNorm = 1e-8,
            AdmissibilityLevel = AdmissibilityLevel.B0,
            Metrics = MakeMetrics(),
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
            EnvironmentTier = tier,
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(tier, deserialized.EnvironmentTier);
    }

    [Fact]
    public void BackgroundAtlas_EnvironmentTier_RoundTrips()
    {
        var atlas = new BackgroundAtlas
        {
            AtlasId = "atlas-structured",
            StudyId = "study-structured",
            Backgrounds = new[]
            {
                TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B0),
            },
            RejectedBackgrounds = Array.Empty<BackgroundRecord>(),
            RankingCriteria = "admissibility-then-residual",
            TotalAttempts = 1,
            Provenance = TestHelpers.MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int> { ["B0"] = 1 },
            EnvironmentTier = "structured",
        };

        var json = BackgroundAtlasSerializer.SerializeAtlas(atlas);
        var deserialized = BackgroundAtlasSerializer.DeserializeAtlas(json);

        Assert.NotNull(deserialized);
        Assert.Equal("structured", deserialized.EnvironmentTier);
    }

    [Fact]
    public void BackgroundStudySpec_EnvironmentTier_RoundTrips()
    {
        var study = new BackgroundStudySpec
        {
            StudyId = "study-with-tier",
            Specs = Array.Empty<BackgroundSpec>(),
            EnvironmentTier = "structured",
        };

        var json = GuJsonDefaults.Serialize(study);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundStudySpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("structured", deserialized.EnvironmentTier);
    }

    [Fact]
    public void BackgroundAtlasBuilder_PropagatesEnvironmentTierToAtlas()
    {
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bundle = ToyGeometryFactory.CreateToy2D();
        var mesh = bundle.AmbientMesh;
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var geometry = bundle.ToGeometryContext("centroid", "P1");
        var manifest = TestHelpers.MakeManifest();
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major", MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[edgeN],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
        var provenance = TestHelpers.MakeProvenance();

        var study = new BackgroundStudySpec
        {
            StudyId = "study-tier-propagation",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-tier",
                    EnvironmentId = "env-default",
                    BranchManifestId = manifest.BranchId,
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions
                    {
                        SolveMode = SolveMode.ResidualOnly,
                        MaxIterations = 1,
                        ToleranceResidualDiagnostic = 1.0,
                        ToleranceStationary = 1.0,
                        ToleranceResidualStrict = 1.0,
                    },
                },
            },
            EnvironmentTier = "structured",
        };

        var manifests = new Dictionary<string, BranchManifest> { [manifest.BranchId] = manifest };
        var geometries = new Dictionary<string, GeometryContext> { ["env-default"] = geometry };
        var a0s = new Dictionary<string, FieldTensor> { ["env-default"] = a0 };

        var builder = new BackgroundAtlasBuilder(backend);
        var atlas = builder.Build(study, manifests, geometries, a0s, provenance);

        Assert.Equal("structured", atlas.EnvironmentTier);
        // All background records should carry the tier
        foreach (var bg in atlas.Backgrounds.Concat(atlas.RejectedBackgrounds))
        {
            Assert.Equal("structured", bg.EnvironmentTier);
        }
    }

    [Fact]
    public void ToyGeometryFactory_CreateStructured2D_ProducesLargerMeshThanToy()
    {
        var toy = ToyGeometryFactory.CreateToy2D();
        // 4x4 structured grid = 32 triangles
        var structured = ToyGeometryFactory.CreateStructured2D(4, 4);

        Assert.True(structured.AmbientMesh.CellCount > toy.AmbientMesh.CellCount,
            "Structured 4x4 mesh should have more cells than toy2D");
        Assert.Same(structured.BaseMesh, structured.AmbientMesh); // trivial fiber: base == ambient
        Assert.True(structured.ValidateSection());
    }

    [Fact]
    public void ToyGeometryFactory_CreateStructured2D_ScalesBySize()
    {
        var s2x2 = ToyGeometryFactory.CreateStructured2D(2, 2);
        var s4x4 = ToyGeometryFactory.CreateStructured2D(4, 4);
        var s8x8 = ToyGeometryFactory.CreateStructured2D(8, 8);

        // 2x2 = 8 cells, 4x4 = 32 cells, 8x8 = 128 cells
        Assert.Equal(8, s2x2.AmbientMesh.CellCount);
        Assert.Equal(32, s4x4.AmbientMesh.CellCount);
        Assert.Equal(128, s8x8.AmbientMesh.CellCount);
    }

    [Fact]
    public void ToyGeometryFactory_CreateStructuredFiberBundle2D_PreservesFiveDimensionalAmbientBranch()
    {
        var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(4, 4);

        Assert.Equal(2, bundle.BaseMesh.EmbeddingDimension);
        Assert.Equal(5, bundle.AmbientMesh.EmbeddingDimension);
        Assert.True(bundle.AmbientMesh.CellCount > bundle.BaseMesh.CellCount);
        Assert.True(bundle.ValidateSection());
    }
}
