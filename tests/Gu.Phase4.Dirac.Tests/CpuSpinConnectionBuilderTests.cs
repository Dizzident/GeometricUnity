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

    private static FermionFieldLayout MakeSu3Layout(SpinorRepresentationSpec spec) =>
        FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-su3", spec, gaugeDimension: 8, TestProvenance(),
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

    // -------------------------------------------------------
    // GAP-7: su(3) gauge support tests
    // -------------------------------------------------------

    [Fact]
    public void Build_Su3Gauge_ProducesNonzeroGaugeCoupling()
    {
        // GAP-7: CpuSpinConnectionBuilder with dimG=8 (su(3)) must produce a non-zero
        // GaugeCouplingCoefficients array when the bosonic background is nonzero.
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu3Layout(spec);
        var bg = MakeBackgroundRecord("bg-su3", "m1");
        int dimG = 8;
        int edgeCount = mesh.EdgeCount; // 3
        var bosonicState = new double[edgeCount * dimG];
        // Set edge 0, a=0 component (T_1 direction) to 1.0
        // f_{012} = f^2_{01} = 1.0 → G[1,2] and G[2,1] should be nonzero
        bosonicState[0 * dimG + 0] = 1.0;

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        // At least one element must be nonzero
        Assert.Contains(bundle.GaugeCouplingCoefficients, v => v != 0.0);
    }

    [Fact]
    public void Build_Su3Gauge_CorrectShape()
    {
        // Shape must be edgeCount * dimG * dimG * 2
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu3Layout(spec);
        var bg = MakeBackgroundRecord("bg-su3-shape", "m1");
        int dimG = 8;
        var bosonicState = new double[mesh.EdgeCount * dimG];
        var builder = new CpuSpinConnectionBuilder();

        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.Equal(mesh.EdgeCount * dimG * dimG * 2, bundle.GaugeCouplingCoefficients.Length);
        Assert.Equal("adjoint", bundle.GaugeRepresentationId);
        Assert.Equal(8, bundle.GaugeDimension);
    }

    [Fact]
    public void Build_Su3Gauge_ZeroOmega_ZeroGaugeCoupling()
    {
        // With zero bosonic state, gauge coupling must be zero
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu3Layout(spec);
        var bg = MakeBackgroundRecord("bg-su3-zero", "m1");
        int dimG = 8;
        var bosonicState = new double[mesh.EdgeCount * dimG]; // all zeros

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        Assert.All(bundle.GaugeCouplingCoefficients, v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void Build_Su3Gauge_Antisymmetry_f012()
    {
        // f_{012} = 1 → for omega_e^0 = 1, G_e[1,2] = f[0,1,2] = 1, G_e[2,1] = f[0,2,1] = -1
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = MakeSu3Layout(spec);
        var bg = MakeBackgroundRecord("bg-su3-antisym", "m1");
        int dimG = 8;
        var bosonicState = new double[mesh.EdgeCount * dimG];
        bosonicState[0 * dimG + 0] = 1.0; // edge 0, a=0

        var builder = new CpuSpinConnectionBuilder();
        var bundle = builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        // Edge 0: G[1,2] = f[0,1,2] = f^2_{01} = 1.0 (from LieAlgebraFactory su(3) f_{123}=1)
        int gcBase = 0 * dimG * dimG * 2; // edge 0
        double G12_re = bundle.GaugeCouplingCoefficients[gcBase + (1 * dimG + 2) * 2];
        double G21_re = bundle.GaugeCouplingCoefficients[gcBase + (2 * dimG + 1) * 2];

        Assert.Equal(1.0, G12_re, 8);
        Assert.Equal(-1.0, G21_re, 8);
        // Antisymmetry
        Assert.Equal(-G12_re, G21_re, 8);
    }

    [Fact]
    public void Build_UnsupportedDimG_ThrowsNotSupportedException()
    {
        // GAP-7: dimG values other than 1, 3, 8 must throw NotSupportedException
        // (was: silent zero fallback)
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        // Build a layout with an unsupported gauge dimension (e.g., 2)
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-unsupported", spec, gaugeDimension: 2, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var bg = MakeBackgroundRecord("bg-bad", "m1");
        var bosonicState = new double[mesh.EdgeCount * 2]; // dimG=2

        var builder = new CpuSpinConnectionBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance()));
    }

    [Fact]
    public void Build_Su3Gauge_CasimirIdentity()
    {
        // Physicist sanity check: for su(3) in the adjoint rep,
        // sum_{a,c} f_{abc} * f_{adc} = 3 * delta_{bd}  (quadratic Casimir C_2(adj) = 3).
        // We verify this directly from the structure constants, independent of the builder.
        // This confirms the Gell-Mann normalization used by LieAlgebraFactory.CreateSu3().
        var su3 = Gu.Math.LieAlgebraFactory.CreateSu3();
        int dimG = su3.Dimension; // 8
        var f = new double[dimG, dimG, dimG];
        for (int a = 0; a < dimG; a++)
            for (int b = 0; b < dimG; b++)
                for (int c = 0; c < dimG; c++)
                    f[a, b, c] = su3.GetStructureConstant(a, b, c);

        // Check sum_{a,c} f_{abc} * f_{adc} = 3 * delta_{bd} for all b, d
        const double casimir = 3.0;
        const double tol = 1e-10;
        for (int b = 0; b < dimG; b++)
        {
            for (int d = 0; d < dimG; d++)
            {
                double sum = 0.0;
                for (int a = 0; a < dimG; a++)
                    for (int c = 0; c < dimG; c++)
                        sum += f[a, b, c] * f[a, d, c];
                double expected = (b == d) ? casimir : 0.0;
                Assert.True(System.Math.Abs(sum - expected) < tol,
                    $"Casimir check failed at b={b} d={d}: got {sum}, expected {expected}");
            }
        }
    }

    [Fact]
    public void Build_Su3Gauge_JacobiIdentity()
    {
        // Stronger consistency check: the Jacobi identity must hold for all structure constants.
        // sum_d (f_{ade}*f_{bcd} + f_{bde}*f_{cad} + f_{cde}*f_{abd}) = 0  for all a,b,c,e
        // This verifies total antisymmetry is correctly set across all permutations.
        var su3 = Gu.Math.LieAlgebraFactory.CreateSu3();
        int dimG = su3.Dimension; // 8
        var f = new double[dimG, dimG, dimG];
        for (int a = 0; a < dimG; a++)
            for (int b = 0; b < dimG; b++)
                for (int c = 0; c < dimG; c++)
                    f[a, b, c] = su3.GetStructureConstant(a, b, c);

        const double tol = 1e-10;
        for (int a = 0; a < dimG; a++)
        for (int b = 0; b < dimG; b++)
        for (int c = 0; c < dimG; c++)
        for (int e = 0; e < dimG; e++)
        {
            double sum = 0.0;
            for (int d = 0; d < dimG; d++)
                sum += f[a, d, e] * f[b, c, d]
                     + f[b, d, e] * f[c, a, d]
                     + f[c, d, e] * f[a, b, d];
            Assert.True(System.Math.Abs(sum) < tol,
                $"Jacobi identity failed at a={a} b={b} c={c} e={e}: got {sum}");
        }
    }
}
