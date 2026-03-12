using System.Numerics;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// Tests for M35: CpuSpinConnectionBuilder.
/// Minimum 8 tests required (see ARCH_P4.md §7.3).
/// </summary>
public class CpuSpinConnectionBuilderTests
{
    // -------------------------------------------------------
    // Test fixtures
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m35",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    /// <summary>Single triangle mesh in 2D (3 vertices, 3 edges, 1 cell).</summary>
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    /// <summary>Two-triangle mesh (4 vertices, 5 edges, 2 cells).</summary>
    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static SpinorRepresentationSpec Dim2Spec() => new()
    {
        SpinorSpecId = "spinor-dim2-v1",
        SpacetimeDimension = 2,
        CliffordSignature = new CliffordSignature { Positive = 2, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 2, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 2,
            HasChirality = true,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-standard-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "-1",
            HasChirality = true,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 2,
        ChiralitySplit = 1,
        Provenance = TestProvenance(),
    };

    private static BackgroundRecord MakeBackgroundRecord(string id, string manifestId) => new()
    {
        BackgroundId = id,
        EnvironmentId = "test-env",
        BranchManifestId = manifestId,
        GeometryFingerprint = "test-fp",
        StateArtifactRef = "test-state-ref",
        ResidualNorm = 0.001,
        StationarityNorm = 0.001,
        AdmissibilityLevel = AdmissibilityLevel.B1,
        Metrics = new BackgroundMetrics
        {
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            ObjectiveValue = 0.1,
            GaugeViolation = 0.0,
            SolverIterations = 10,
            SolverConverged = true,
            TerminationReason = "residual-converged",
            GaussNewtonValid = false,
        },
        ReplayTierAchieved = "R1",
        Provenance = TestProvenance(),
    };

    private static FermionFieldLayout MakeTrivialLayout(SpinorRepresentationSpec spec) =>
        FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });

    private static FermionFieldLayout MakeSu2Layout(SpinorRepresentationSpec spec) =>
        FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-su2", spec, gaugeDimension: 3, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------

    [Fact]
    public void Build_Trivial_ReturnsBundle()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "manifest-1");
        var bosonicState = new double[mesh.EdgeCount * 1]; // trivial gauge: dimG=1
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.NotNull(bundle);
        Assert.Equal("bg-1", bundle.BackgroundId);
        Assert.Equal(1, bundle.GaugeDimension);
        Assert.Equal(2, bundle.SpinorDimension);
        Assert.True(bundle.FlatLeviCivitaAssumption);
    }

    [Fact]
    public void Build_SpinorSpecIdPreserved()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var bosonicState = new double[mesh.EdgeCount];
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.Equal("spinor-dim2-v1", bundle.SpinorSpecId);
    }

    [Fact]
    public void Build_EdgeCount_MatchesMesh()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var bosonicState = new double[mesh.EdgeCount];
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.Equal(mesh.EdgeCount, bundle.EdgeCount);
    }

    [Fact]
    public void Build_ZeroBosonicState_ZeroGaugeCoupling()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var bosonicState = new double[mesh.EdgeCount]; // all zeros
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.All(bundle.GaugeCouplingCoefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void Build_FlatLC_LCCoefficientsAllZero()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var bosonicState = new double[mesh.EdgeCount];
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.All(bundle.LeviCivitaCoefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void Build_TwoDistinctBackgrounds_ProduceDistinctConnectionIds()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg1 = MakeBackgroundRecord("bg-A", "m1");
        var bg2 = MakeBackgroundRecord("bg-B", "m1");
        var omega = new double[mesh.EdgeCount];
        var builder = new CpuSpinConnectionBuilder();

        var c1 = builder.Build(bg1, omega, spec, layout, mesh, TestProvenance());
        var c2 = builder.Build(bg2, omega, spec, layout, mesh, TestProvenance());

        Assert.NotEqual(c1.ConnectionId, c2.ConnectionId);
        Assert.Equal("bg-A", c1.BackgroundId);
        Assert.Equal("bg-B", c2.BackgroundId);
    }

    [Fact]
    public void Build_NonZeroOmega_Trivial_GaugeCouplingMatchesOmega()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        int edgeCount = mesh.EdgeCount;
        var bosonicState = new double[] { 0.5, 1.2, -0.3 }; // one omega per edge (trivial: dimG=1)
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        // For trivial rep: G_e = [omega_e^0, 0] (real, imaginary=0)
        // Storage: edgeCount * 1 * 1 * 2 = edgeCount * 2
        Assert.Equal(edgeCount * 2, bundle.GaugeCouplingCoefficients.Length);
        for (int e = 0; e < edgeCount; e++)
        {
            Assert.Equal(bosonicState[e], bundle.GaugeCouplingCoefficients[e * 2],     8); // Re
            Assert.Equal(0.0,             bundle.GaugeCouplingCoefficients[e * 2 + 1], 8); // Im
        }
    }

    [Fact]
    public void Build_Su2Gauge_GaugeCouplingHasCorrectShape()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu2Layout(spec);
        var bg = MakeBackgroundRecord("bg-su2", "m1");
        int edgeCount = mesh.EdgeCount;
        int dimG = 3;
        var bosonicState = new double[edgeCount * dimG];
        // Nonzero: edge 0, a=0 component = 1.0
        bosonicState[0 * dimG + 0] = 1.0;

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        // Shape: edgeCount * dimG * dimG * 2
        Assert.Equal(edgeCount * dimG * dimG * 2, bundle.GaugeCouplingCoefficients.Length);
        Assert.Equal("adjoint", bundle.GaugeRepresentationId);
    }

    [Fact]
    public void Build_Su2Gauge_ZeroOmega_ZeroGaugeCoupling()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu2Layout(spec);
        var bg = MakeBackgroundRecord("bg-su2", "m1");
        int edgeCount = mesh.EdgeCount;
        int dimG = 3;
        var bosonicState = new double[edgeCount * dimG]; // all zeros

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.All(bundle.GaugeCouplingCoefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void Build_Su2Gauge_AntisymmetryOfStructureConstants()
    {
        // f_{abc} = epsilon_{abc} so f[0,1,2] = 1, f[0,2,1] = -1
        // G_e[b,c] = sum_a omega_e^a * f[a,b,c]
        // For omega_e^0 = 1, others zero: G_e[b,c] = f[0,b,c] = epsilon_{0bc}
        // epsilon_{012} = 1, epsilon_{021} = -1, all others zero
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu2Layout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        int dimG = 3;
        var omega = new double[mesh.EdgeCount * dimG];
        omega[0] = 1.0; // edge 0, a=0 component = 1.0

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, omega, spec, layout, mesh, TestProvenance());

        // Edge 0, G[1,2] should be f[0,1,2] = 1
        int e = 0;
        int gcBase = e * dimG * dimG * 2;
        double G12_re = bundle.GaugeCouplingCoefficients[gcBase + (1 * dimG + 2) * 2];
        double G21_re = bundle.GaugeCouplingCoefficients[gcBase + (2 * dimG + 1) * 2];

        Assert.Equal(1.0, G12_re, 8);
        Assert.Equal(-1.0, G21_re, 8);
        // G is antisymmetric
        Assert.Equal(-G12_re, G21_re, 8);
    }

    [Fact]
    public void Build_ProvenancePreserved()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var bosonicState = new double[mesh.EdgeCount];
        var prov = TestProvenance();
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, prov);

        Assert.Equal("test-m35", bundle.Provenance.CodeRevision);
    }

    [Fact]
    public void Build_Reproducible_SameMeshSameState()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var omega = new double[] { 0.3, -0.7, 1.1 };
        var builder = new CpuSpinConnectionBuilder();

        var b1 = builder.Build(bg, omega, spec, layout, mesh, TestProvenance());
        var b2 = builder.Build(bg, omega, spec, layout, mesh, TestProvenance());

        Assert.Equal(b1.GaugeCouplingCoefficients, b2.GaugeCouplingCoefficients);
        Assert.Equal(b1.LeviCivitaCoefficients, b2.LeviCivitaCoefficients);
    }

    [Fact]
    public void Build_TwoTriangleMesh_BuildsCorrectly()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-2tri", "m1");
        var omega = new double[mesh.EdgeCount]; // 5 edges for 2-triangle mesh
        for (int i = 0; i < omega.Length; i++) omega[i] = i * 0.1;
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, omega, spec, layout, mesh, TestProvenance());

        Assert.Equal(mesh.EdgeCount, bundle.EdgeCount);
        Assert.Equal("bg-2tri", bundle.BackgroundId);
    }

    [Fact]
    public void Build_WrongBosonicStateLength_Throws()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeTrivialLayout(spec);
        var bg = MakeBackgroundRecord("bg-1", "m1");
        var badOmega = new double[100]; // wrong length
        var builder = new CpuSpinConnectionBuilder();

        Assert.Throws<ArgumentException>(() =>
            builder.Build(bg, badOmega, spec, layout, mesh, TestProvenance()));
    }
}
