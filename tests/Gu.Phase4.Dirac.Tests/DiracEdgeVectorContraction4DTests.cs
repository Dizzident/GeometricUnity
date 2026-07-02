using System.Numerics;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// M2 acceptance for the design §2.4 Dirac refinement: the opt-in
/// <see cref="GammaEdgeScheme.EdgeVectorContraction"/> edge→gamma scheme
/// (Gamma_hat(e) = ê·Γ) on the Cl(4,0) 4D mesh. Structural tests 2.4(a–d):
///   (a) Gamma_hat for a unit axis-μ edge equals γ^μ (reduces to DominantAxis);
///   (b) {Gamma_hat, Gamma_hat} = 2 I for a unit edge vector;
///   (c) EdgeVectorContraction Dirac on CreateUniform4D(1) is Hermitian;
///   (d) DominantAxis remains the default and is unchanged (existing suite green).
/// </summary>
public sealed class DiracEdgeVectorContraction4DTests
{
    private const double Tol = 1e-13;

    private static ProvenanceMeta Prov() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 7, 2, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "m2-dirac-refine",
        Branch = new BranchRef { BranchId = "4d-platform", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SpinorRepresentationSpec Dim4Spec()
    {
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        return new SpinorRepresentationSpec
        {
            SpinorSpecId = "spinor-cl40-v1",
            SpacetimeDimension = 4,
            CliffordSignature = sig,
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = "dirac-tensor-product-v1",
                Signature = sig,
                Representation = "standard",
                SpinorDimension = 4,
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
            SpinorComponents = 4,
            ChiralitySplit = 2,
            Provenance = Prov(),
        };
    }

    private static BackgroundRecord Background()
    {
        var metrics = new BackgroundMetrics
        {
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            ObjectiveValue = 0.1,
            GaugeViolation = 0.0,
            SolverIterations = 10,
            SolverConverged = true,
            TerminationReason = "residual-converged",
            GaussNewtonValid = false,
        };
        return new BackgroundRecord
        {
            BackgroundId = "bg-4d",
            EnvironmentId = "test-env",
            BranchManifestId = "manifest-1",
            GeometryFingerprint = "fp-4d",
            StateArtifactRef = "state-ref",
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = metrics,
            ReplayTierAchieved = "R1",
            Provenance = Prov(),
        };
    }

    private static (GammaOperatorBundle gammas, SpinConnectionBundle conn, FermionFieldLayout layout, SimplicialMesh mesh)
        Setup4D()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var spec = Dim4Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout("layout-4d", spec, gaugeDimension: 1, Prov());
        var gammas = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var omega = new double[mesh.EdgeCount]; // flat / zero connection
        var conn = new CpuSpinConnectionBuilder().Build(Background(), omega, spec, layout, mesh, Prov());
        return (gammas, conn, layout, mesh);
    }

    // ---------------------------------------------------------------
    // (a) Gamma_hat for a unit axis-μ edge equals γ^μ, for both schemes.
    // ---------------------------------------------------------------

    [Fact]
    public void EdgeGamma_UnitAxisEdge_EqualsGammaMu_BothSchemes()
    {
        var (gammas, _, _, mesh) = Setup4D();
        var contraction = new CpuDiracOperatorAssembler(GammaEdgeScheme.EdgeVectorContraction);
        var dominant = new CpuDiracOperatorAssembler(GammaEdgeScheme.DominantAxis);

        for (int mu = 0; mu < 4; mu++)
        {
            int edge = FindUnitAxisEdge(mesh, mu);
            Assert.True(edge >= 0, $"no unit axis-{mu} edge in CreateUniform4D(1)");

            var gContract = contraction.EdgeGamma(mesh, gammas, edge)!;
            var gDominant = dominant.EdgeGamma(mesh, gammas, edge)!;

            Assert.True(FrobeniusDiff(gContract, gammas.GammaMatrices[mu]) < Tol,
                $"EdgeVectorContraction Gamma_hat(axis-{mu}) != gamma[{mu}]");
            Assert.True(FrobeniusDiff(gDominant, gammas.GammaMatrices[mu]) < Tol,
                $"DominantAxis Gamma_hat(axis-{mu}) != gamma[{mu}]");
        }
    }

    // ---------------------------------------------------------------
    // (b) {Gamma_hat, Gamma_hat} = 2 I for every (unit) edge vector.
    // ---------------------------------------------------------------

    [Fact]
    public void EdgeGamma_SatisfiesCliffordRelation_ForUnitEdgeVector()
    {
        var (gammas, _, _, mesh) = Setup4D();
        var contraction = new CpuDiracOperatorAssembler(GammaEdgeScheme.EdgeVectorContraction);
        int s = gammas.SpinorDimension;

        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var g = contraction.EdgeGamma(mesh, gammas, e)!;
            // {G,G} = 2 G^2 ; for the unit contraction ê·Γ this equals 2 |ê|^2 I = 2 I.
            var anti = MatAdd(MatMul(g, g), MatMul(g, g));
            Assert.True(FrobeniusDiffFromScaledIdentity(anti, 2.0, s) < Tol,
                $"edge {e}: {{Gamma_hat, Gamma_hat}} != 2 I");
        }
    }

    // ---------------------------------------------------------------
    // (c) EdgeVectorContraction Dirac on CreateUniform4D(1) is Hermitian.
    // ---------------------------------------------------------------

    [Fact]
    public void EdgeVectorContraction_Dirac_IsHermitian_On4DMesh()
    {
        var (gammas, conn, layout, mesh) = Setup4D();
        var assembler = new CpuDiracOperatorAssembler(GammaEdgeScheme.EdgeVectorContraction);
        var bundle = assembler.Assemble(conn, gammas, layout, mesh, Prov());

        Assert.True(bundle.HasExplicitMatrix); // totalDof = 16*4 = 64 <= 4096
        double herm = DiracOperatorValidator.CheckHermiticity(bundle);
        Assert.True(herm <= 1e-10, $"EdgeVectorContraction Dirac not Hermitian: residual {herm:E3}");
        Assert.True(bundle.IsHermitian);
    }

    // ---------------------------------------------------------------
    // (d) DominantAxis is the default and unchanged; the new scheme genuinely
    //     differs on the diagonal-heavy 4D mesh (else the refinement is a no-op).
    // ---------------------------------------------------------------

    [Fact]
    public void DefaultScheme_IsDominantAxis()
    {
        Assert.Equal(GammaEdgeScheme.DominantAxis, new CpuDiracOperatorAssembler().Scheme);
    }

    [Fact]
    public void AssemblerEdgeGamma_DelegatesToSharedHelper_SingleSourceOfTruth()
    {
        // The assembler's EdgeVectorContraction edge-gamma MUST be exactly the
        // shared EdgeGammaContraction.UnitContract (no second implementation).
        var (gammas, _, _, mesh) = Setup4D();
        var assembler = new CpuDiracOperatorAssembler(GammaEdgeScheme.EdgeVectorContraction);

        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var viaAssembler = assembler.EdgeGamma(mesh, gammas, e)!;
            int[] ep = mesh.Edges[e];
            var viaHelper = EdgeGammaContraction.UnitContract(
                mesh.GetVertexCoordinates(ep[0]),
                mesh.GetVertexCoordinates(ep[1]),
                gammas, out _)!;
            Assert.True(FrobeniusDiff(viaAssembler, viaHelper) < 1e-15,
                $"edge {e}: assembler edge-gamma diverges from shared helper");
        }
    }

    [Fact]
    public void EdgeVectorContraction_DiffersFromDominantAxis_OnDiagonalMesh()
    {
        var (gammas, conn, layout, mesh) = Setup4D();
        var dominant = new CpuDiracOperatorAssembler(GammaEdgeScheme.DominantAxis)
            .Assemble(conn, gammas, layout, mesh, Prov());
        var contraction = new CpuDiracOperatorAssembler(GammaEdgeScheme.EdgeVectorContraction)
            .Assemble(conn, gammas, layout, mesh, Prov());

        double maxDiff = dominant.ExplicitMatrix!
            .Zip(contraction.ExplicitMatrix!, (a, b) => System.Math.Abs(a - b))
            .Max();
        Assert.True(maxDiff > 1e-6,
            "EdgeVectorContraction should change diagonal-edge contributions vs DominantAxis on the Freudenthal mesh.");
    }

    // ---------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------

    private static int FindUnitAxisEdge(SimplicialMesh mesh, int axis)
    {
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int[] ep = mesh.Edges[e];
            var c0 = mesh.GetVertexCoordinates(ep[0]);
            var c1 = mesh.GetVertexCoordinates(ep[1]);
            bool ok = true;
            for (int mu = 0; mu < mesh.EmbeddingDimension; mu++)
            {
                double dx = c1[mu] - c0[mu];
                double want = mu == axis ? 1.0 : 0.0;
                if (System.Math.Abs(dx - want) > 1e-12) { ok = false; break; }
            }
            if (ok) return e;
        }
        return -1;
    }

    private static Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex acc = Complex.Zero;
                for (int k = 0; k < n; k++) acc += a[i, k] * b[k, j];
                r[i, j] = acc;
            }
        return r;
    }

    private static Complex[,] MatAdd(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] + b[i, j];
        return r;
    }

    private static double FrobeniusDiff(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        double sum = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex d = a[i, j] - b[i, j];
                sum += d.Real * d.Real + d.Imaginary * d.Imaginary;
            }
        return System.Math.Sqrt(sum);
    }

    private static double FrobeniusDiffFromScaledIdentity(Complex[,] m, double scale, int s)
    {
        double sum = 0;
        for (int i = 0; i < s; i++)
            for (int j = 0; j < s; j++)
            {
                Complex expected = i == j ? new Complex(scale, 0) : Complex.Zero;
                Complex d = m[i, j] - expected;
                sum += d.Real * d.Real + d.Imaginary * d.Imaginary;
            }
        return System.Math.Sqrt(sum);
    }
}
