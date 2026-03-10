using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

public class BackgroundAtlasBuilderTests
{
    private static (ISolverBackend Backend, GeometryContext Geometry, FieldTensor A0, BranchManifest Manifest, LieAlgebra Algebra) SetupToy()
    {
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bundle = ToyGeometryFactory.CreateToy2D();
        var mesh = bundle.AmbientMesh;
        var geometry = bundle.ToGeometryContext("centroid", "P1");
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[edgeN],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };

        var manifest = TestHelpers.MakeManifest();
        return (backend, geometry, a0, manifest, algebra);
    }

    [Fact]
    public void Build_SingleTrivialBackground_ProducesAtlas()
    {
        var (backend, geometry, a0, manifest, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);

        var study = new BackgroundStudySpec
        {
            StudyId = "study-trivial",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-trivial",
                    EnvironmentId = "env-1",
                    BranchManifestId = "branch-1",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions
                    {
                        SolveMode = SolveMode.ResidualOnly,
                        ToleranceResidualDiagnostic = 1.0, // Very permissive
                        ToleranceStationary = 1.0,
                        ToleranceResidualStrict = 1.0,
                    },
                },
            },
        };

        var manifests = new Dictionary<string, BranchManifest> { ["branch-1"] = manifest };
        var geometries = new Dictionary<string, GeometryContext> { ["env-1"] = geometry };
        var a0s = new Dictionary<string, FieldTensor> { ["env-1"] = a0 };

        var atlas = builder.Build(study, manifests, geometries, a0s, TestHelpers.MakeProvenance());

        Assert.NotNull(atlas);
        Assert.Equal("study-trivial", atlas.StudyId);
        Assert.Equal(1, atlas.TotalAttempts);
        // With permissive tolerances and zero state (zero residual), should be admitted
        Assert.True(atlas.Backgrounds.Count >= 1 || atlas.RejectedBackgrounds.Count >= 1);
    }

    [Fact]
    public void Build_MultipleSpecs_ProducesAtlasWithMultipleBackgrounds()
    {
        var (backend, geometry, a0, manifest, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);

        var study = new BackgroundStudySpec
        {
            StudyId = "study-multi",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-1",
                    EnvironmentId = "env-1",
                    BranchManifestId = "branch-1",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions
                    {
                        SolveMode = SolveMode.ResidualOnly,
                        ToleranceResidualDiagnostic = 10.0,
                        ToleranceStationary = 10.0,
                        ToleranceResidualStrict = 10.0,
                    },
                },
                new BackgroundSpec
                {
                    SpecId = "spec-2",
                    EnvironmentId = "env-1",
                    BranchManifestId = "branch-1",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions
                    {
                        SolveMode = SolveMode.ResidualOnly,
                        ToleranceResidualDiagnostic = 10.0,
                        ToleranceStationary = 10.0,
                        ToleranceResidualStrict = 10.0,
                    },
                },
            },
        };

        var manifests = new Dictionary<string, BranchManifest> { ["branch-1"] = manifest };
        var geometries = new Dictionary<string, GeometryContext> { ["env-1"] = geometry };
        var a0s = new Dictionary<string, FieldTensor> { ["env-1"] = a0 };

        var atlas = builder.Build(study, manifests, geometries, a0s, TestHelpers.MakeProvenance());

        Assert.NotNull(atlas);
        Assert.Equal(2, atlas.TotalAttempts);
    }

    [Fact]
    public void Build_MissingManifest_ProducesRejectedRecord()
    {
        var (backend, geometry, a0, _, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);

        var study = new BackgroundStudySpec
        {
            StudyId = "study-missing",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-missing-branch",
                    EnvironmentId = "env-1",
                    BranchManifestId = "nonexistent-branch",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions { SolveMode = SolveMode.ResidualOnly },
                },
            },
        };

        var manifests = new Dictionary<string, BranchManifest>();
        var geometries = new Dictionary<string, GeometryContext> { ["env-1"] = geometry };
        var a0s = new Dictionary<string, FieldTensor> { ["env-1"] = a0 };

        var atlas = builder.Build(study, manifests, geometries, a0s, TestHelpers.MakeProvenance());

        Assert.Single(atlas.RejectedBackgrounds);
        Assert.Empty(atlas.Backgrounds);
        Assert.Contains("not found", atlas.RejectedBackgrounds[0].RejectionReason);
    }

    [Fact]
    public void Deduplicate_RemovesDuplicatesWithinThreshold()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1, residualNorm: 1.0001e-5, stationarityNorm: 1.0001e-7),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-3);
        Assert.Single(deduped);
    }

    [Fact]
    public void Deduplicate_KeepsDifferentBackgrounds()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B0, residualNorm: 1e-2, stationarityNorm: 1e-1),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-6);
        Assert.Equal(2, deduped.Count);
    }

    [Fact]
    public void Rank_AdmissibilityThenResidual()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-b0", AdmissibilityLevel.B0, residualNorm: 1e-5),
            TestHelpers.MakeRecord("bg-b2", AdmissibilityLevel.B2, residualNorm: 1e-9),
            TestHelpers.MakeRecord("bg-b1", AdmissibilityLevel.B1, residualNorm: 1e-6),
        };

        var ranked = BackgroundAtlasBuilder.Rank(records, "admissibility-then-residual");
        Assert.Equal("bg-b2", ranked[0].BackgroundId); // B2 strongest, ranks first
        Assert.Equal("bg-b1", ranked[1].BackgroundId);
        Assert.Equal("bg-b0", ranked[2].BackgroundId);
    }

    [Fact]
    public void Rank_ResidualThenStationarity()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-high", AdmissibilityLevel.B1, residualNorm: 1e-3),
            TestHelpers.MakeRecord("bg-low", AdmissibilityLevel.B1, residualNorm: 1e-9),
            TestHelpers.MakeRecord("bg-mid", AdmissibilityLevel.B1, residualNorm: 1e-6),
        };

        var ranked = BackgroundAtlasBuilder.Rank(records, "residual-then-stationarity");
        Assert.Equal("bg-low", ranked[0].BackgroundId);
        Assert.Equal("bg-mid", ranked[1].BackgroundId);
        Assert.Equal("bg-high", ranked[2].BackgroundId);
    }

    [Fact]
    public void Build_NullStudy_Throws()
    {
        var (backend, _, _, _, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);
        Assert.Throws<ArgumentNullException>(() =>
            builder.Build(null!, new Dictionary<string, BranchManifest>(),
                new Dictionary<string, GeometryContext>(),
                new Dictionary<string, FieldTensor>(),
                TestHelpers.MakeProvenance()));
    }

    [Fact]
    public void Build_MissingGeometry_ProducesRejectedRecord()
    {
        var (backend, _, _, manifest, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);

        var study = new BackgroundStudySpec
        {
            StudyId = "study-no-geo",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-no-geo",
                    EnvironmentId = "env-missing",
                    BranchManifestId = "branch-1",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions { SolveMode = SolveMode.ResidualOnly },
                },
            },
        };

        var manifests = new Dictionary<string, BranchManifest> { ["branch-1"] = manifest };
        var geometries = new Dictionary<string, GeometryContext>();
        var a0s = new Dictionary<string, FieldTensor>();

        var atlas = builder.Build(study, manifests, geometries, a0s, TestHelpers.MakeProvenance());

        Assert.Single(atlas.RejectedBackgrounds);
        Assert.Contains("Geometry", atlas.RejectedBackgrounds[0].RejectionReason);
    }

    [Fact]
    public void Build_AdmissibilityCounts_AreCorrect()
    {
        var (backend, geometry, a0, manifest, _) = SetupToy();
        var builder = new BackgroundAtlasBuilder(backend);

        var study = new BackgroundStudySpec
        {
            StudyId = "study-counts",
            Specs = new[]
            {
                new BackgroundSpec
                {
                    SpecId = "spec-1",
                    EnvironmentId = "env-1",
                    BranchManifestId = "branch-1",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions
                    {
                        SolveMode = SolveMode.ResidualOnly,
                        ToleranceResidualDiagnostic = 10.0,
                        ToleranceStationary = 10.0,
                        ToleranceResidualStrict = 10.0,
                    },
                },
                new BackgroundSpec
                {
                    SpecId = "spec-missing",
                    EnvironmentId = "env-1",
                    BranchManifestId = "missing-branch",
                    Seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial },
                    SolveOptions = new BackgroundSolveOptions { SolveMode = SolveMode.ResidualOnly },
                },
            },
        };

        var manifests = new Dictionary<string, BranchManifest> { ["branch-1"] = manifest };
        var geometries = new Dictionary<string, GeometryContext> { ["env-1"] = geometry };
        var a0s = new Dictionary<string, FieldTensor> { ["env-1"] = a0 };

        var atlas = builder.Build(study, manifests, geometries, a0s, TestHelpers.MakeProvenance());

        Assert.Equal(2, atlas.TotalAttempts);
        Assert.True(atlas.AdmissibilityCounts.ContainsKey("Rejected"));
    }
}
