using System.Numerics;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// Tests for M36: CpuDiracOperatorAssembler and DiracOperatorValidator.
/// Minimum 12 tests required (see ARCH_P4.md §7.4).
/// </summary>
public class CpuDiracOperatorAssemblerTests
{
    // -------------------------------------------------------
    // Test fixtures
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m36",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

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

    private static BackgroundRecord MakeBackground(string id) => new()
    {
        BackgroundId = id,
        EnvironmentId = "test-env",
        BranchManifestId = "manifest-1",
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

    /// <summary>
    /// Assemble a minimal Dirac operator for a two-triangle mesh, trivial gauge (dimG=1),
    /// dim2 spinors. Returns (bundle, connection, layout, assembler).
    /// </summary>
    private static (DiracOperatorBundle, SpinConnectionBundle,
                    FermionFieldLayout, IDiracOperatorAssembler)
        AssembleTwoTriangle(double[]? omega = null)
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance());
        var bg = MakeBackground("bg-1");

        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(
            spec.CliffordSignature, spec.GammaConvention, TestProvenance());

        omega ??= new double[mesh.EdgeCount]; // zero by default

        var connBuilder = new CpuSpinConnectionBuilder();
        var conn = connBuilder.Build(bg, omega, spec, layout, mesh, TestProvenance());

        var assembler = new CpuDiracOperatorAssembler();
        var bundle = assembler.Assemble(conn, gammas, layout, mesh, TestProvenance());
        return (bundle, conn, layout, assembler);
    }

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------

    [Fact]
    public void Assemble_ReturnsBundle_ForToySystem()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        Assert.NotNull(bundle);
    }

    [Fact]
    public void Assemble_MatrixShape_IsSquare()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        Assert.Equal(2, bundle.MatrixShape.Length);
        Assert.Equal(bundle.MatrixShape[0], bundle.MatrixShape[1]);
    }

    [Fact]
    public void Assemble_MatrixShape_MatchesNodeCountTimesDofsPerCell()
    {
        var mesh = TwoTriangles();
        var (bundle, _, _, _) = AssembleTwoTriangle();
        // vertexCount=4, spinorDim=2, dimG=1 => dofsPerCell=2 => totalDof=8
        Assert.Equal(mesh.VertexCount * bundle.DofsPerCell, bundle.TotalDof);
        Assert.Equal(bundle.TotalDof, bundle.MatrixShape[0]);
    }

    [Fact]
    public void Assemble_DofsPerCell_IsSpinorTimesGauge()
    {
        var spec = Dim2Spec(); // spinorDim=2
        var (bundle, _, _, _) = AssembleTwoTriangle();
        // dimG=1 (trivial), spinorDim=2 => dofsPerCell = 2
        Assert.Equal(spec.SpinorComponents * 1, bundle.DofsPerCell);
    }

    [Fact]
    public void Assemble_ExplicitMatrix_HasCorrectStorageLength()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        Assert.True(bundle.HasExplicitMatrix);
        Assert.NotNull(bundle.ExplicitMatrix);
        int expected = bundle.TotalDof * bundle.TotalDof * 2;
        Assert.Equal(expected, bundle.ExplicitMatrix!.Length);
    }

    [Fact]
    public void Assemble_ZeroOmega_IsHermitian()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        // Zero omega => zero gauge coupling. LC = 0 (flat). So D = 0 (trivially Hermitian).
        // The assembled matrix should be Hermitian.
        Assert.True(bundle.IsHermitian);
        Assert.Equal(0.0, bundle.HermiticityResidual, 12);
    }

    [Fact]
    public void Assemble_NonZeroOmega_IsHermitian()
    {
        // Nontrivial omega should still give Hermitian D (Riemannian + symmetrized assembly)
        var mesh = TwoTriangles();
        var omega = Enumerable.Range(0, mesh.EdgeCount).Select(i => 0.3 * (i + 1)).ToArray();
        var (bundle, _, _, _) = AssembleTwoTriangle(omega);

        Assert.True(bundle.IsHermitian,
            $"Hermiticity residual = {bundle.HermiticityResidual:E4}");
    }

    [Fact]
    public void Apply_ZeroOmega_ReturnsDerivativeOperator()
    {
        var (bundle, _, _, assembler) = AssembleTwoTriangle();
        int n = bundle.TotalDof;
        var psi = new double[n * 2];
        for (int i = 0; i < psi.Length; i++) psi[i] = i * 0.1;

        var result = assembler.Apply(bundle, psi);

        // Apply must return a vector of the same length as psi
        Assert.Equal(psi.Length, result.Length);
        // With zero omega the kinetic finite-difference term is still present;
        // verify that Apply matches explicit-matrix multiplication.
        var D = bundle.ExplicitMatrix!;
        var expected = new double[n * 2];
        for (int i = 0; i < n; i++)
        {
            double re = 0, im = 0;
            for (int j = 0; j < n; j++)
            {
                double dRe = D[(i * n + j) * 2], dIm = D[(i * n + j) * 2 + 1];
                re += dRe * psi[j * 2] - dIm * psi[j * 2 + 1];
                im += dRe * psi[j * 2 + 1] + dIm * psi[j * 2];
            }
            expected[i * 2] = re;
            expected[i * 2 + 1] = im;
        }
        for (int i = 0; i < result.Length; i++)
            Assert.Equal(expected[i], result[i], 10);
    }

    [Fact]
    public void Apply_MatchesExplicitMatrix()
    {
        var mesh = TwoTriangles();
        var omega = new double[] { 0.1, 0.2, 0.0, -0.1, 0.3 }; // 5 edges
        var (bundle, _, _, assembler) = AssembleTwoTriangle(omega);
        int n = bundle.TotalDof;

        // Test vector
        var rng = new Random(77);
        var psi = new double[n * 2];
        for (int i = 0; i < psi.Length; i++) psi[i] = rng.NextDouble() * 2.0 - 1.0;

        var result = assembler.Apply(bundle, psi);

        // Manually compute D * psi using explicit matrix
        var D = bundle.ExplicitMatrix!;
        var expected = new double[n * 2];
        for (int i = 0; i < n; i++)
        {
            double re = 0, im = 0;
            for (int j = 0; j < n; j++)
            {
                double dRe = D[(i * n + j) * 2], dIm = D[(i * n + j) * 2 + 1];
                re += dRe * psi[j * 2] - dIm * psi[j * 2 + 1];
                im += dRe * psi[j * 2 + 1] + dIm * psi[j * 2];
            }
            expected[i * 2] = re;
            expected[i * 2 + 1] = im;
        }

        for (int i = 0; i < result.Length; i++)
            Assert.Equal(expected[i], result[i], 10);
    }

    [Fact]
    public void Apply_WrongPsiLength_Throws()
    {
        var (bundle, _, _, assembler) = AssembleTwoTriangle();
        var psi = new double[100]; // wrong length
        Assert.Throws<ArgumentException>(() => assembler.Apply(bundle, psi));
    }

    [Fact]
    public void Assemble_MassBranchTermNotIncluded_ByDefault()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        Assert.False(bundle.MassBranchTermIncluded);
    }

    [Fact]
    public void Assemble_GaugeReductionNotApplied_ByDefault()
    {
        var (bundle, _, _, _) = AssembleTwoTriangle();
        Assert.False(bundle.GaugeReductionApplied);
    }

    [Fact]
    public void Assemble_DistinctBackgrounds_DistinctOperatorIds()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance());
        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var connBuilder = new CpuSpinConnectionBuilder();
        var assembler = new CpuDiracOperatorAssembler();
        var omega = new double[mesh.EdgeCount];

        var bg1 = MakeBackground("bg-A");
        var bg2 = MakeBackground("bg-B");
        var c1 = connBuilder.Build(bg1, omega, spec, layout, mesh, TestProvenance());
        var c2 = connBuilder.Build(bg2, omega, spec, layout, mesh, TestProvenance());
        var d1 = assembler.Assemble(c1, gammas, layout, mesh, TestProvenance());
        var d2 = assembler.Assemble(c2, gammas, layout, mesh, TestProvenance());

        Assert.NotEqual(d1.OperatorId, d2.OperatorId);
    }
}
