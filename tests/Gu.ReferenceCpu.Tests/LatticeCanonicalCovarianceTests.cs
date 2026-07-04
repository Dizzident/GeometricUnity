using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// Acceptance tests for the lattice-canonical orientation convention (UNLOCK
/// project (i)) at the platform-action level, using the real Gu.ReferenceCpu stack:
/// ConnectionField → CurvatureAssembler → IdentityShiabCpu → CpuMassMatrix.
/// Phase444 (F3) measured these quantities failing translation covariance on the
/// default torus (pure ||F||^2 at 2.5e-4); with latticeCanonical = true the same
/// signed-permutation test must hold to floating-point roundoff, because every
/// downstream consumer reads the builder's precomputed orientation arrays.
/// </summary>
public class LatticeCanonicalCovarianceTests
{
    private const int TorusN = 3;
    private const double RoundoffBar = 1e-12;

    /// <summary>Acceptance 3a: pure curvature action ||F||^2_M via CurvatureAssembler.</summary>
    [Fact]
    public void CanonicalTorus_CurvatureNormSquared_IsTranslationCovariantToRoundoff()
    {
        double residual = CovarianceResidual(latticeCanonical: true, ControlAction: false);
        Assert.True(residual < RoundoffBar, $"||F||^2 covariance residual {residual:E3} exceeds {RoundoffBar:E0}.");
    }

    /// <summary>Acceptance 3b: identity-Shiab control action S_B = (1/2)⟨S, M S⟩.</summary>
    [Fact]
    public void CanonicalTorus_IdentityShiabControlAction_IsTranslationCovariantToRoundoff()
    {
        double residual = CovarianceResidual(latticeCanonical: true, ControlAction: true);
        Assert.True(residual < RoundoffBar, $"Control-action covariance residual {residual:E3} exceeds {RoundoffBar:E0}.");
    }

    /// <summary>
    /// Documented limitation kept measurable: the default global-index convention
    /// fails the identical test (phase444 F3 root cause), so the roundoff bars above
    /// have teeth.
    /// </summary>
    [Fact]
    public void DefaultTorus_CurvatureNormSquared_IsNotTranslationCovariant()
    {
        double residual = CovarianceResidual(latticeCanonical: false, ControlAction: false);
        Assert.True(residual > 1e-6, $"Expected the default convention to violate covariance, got {residual:E3}.");
    }

    // =====================================================================
    // Machinery (phase444 methodology, real platform classes)
    // =====================================================================

    private static double CovarianceResidual(bool latticeCanonical, bool ControlAction)
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(TorusN, latticeCanonical);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var mass = new CpuMassMatrix(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = TestGeometry();
        var translations = BuildTranslations(mesh);

        double Action(double[] omegaCoeffs)
        {
            var omega = new ConnectionField(mesh, algebra, omegaCoeffs);
            var curvature = CurvatureAssembler.Assemble(omega).ToFieldTensor();
            if (!ControlAction)
                return mass.InnerProduct(curvature, curvature);

            var s = shiab.Evaluate(curvature, omega.ToFieldTensor(), manifest, geometry);
            return mass.EvaluateObjective(s);
        }

        var rng = new Random(20260703);
        double worst = 0;
        for (int trial = 0; trial < 2; trial++)
        {
            var omega = new double[mesh.EdgeCount * dimG];
            for (int i = 0; i < omega.Length; i++)
                omega[i] = 0.4 * (rng.NextDouble() - 0.5);

            double reference = Action(omega);
            foreach (var map in translations)
            {
                double translated = Action(SignedPermute(mesh, omega, dimG, map));
                worst = System.Math.Max(
                    worst, System.Math.Abs(translated - reference) / System.Math.Abs(reference));
            }
        }

        return worst;
    }

    /// <summary>
    /// Vertex index maps for a sample of lattice translations on the (Z_n)^4 torus.
    /// </summary>
    private static List<int[]> BuildTranslations(SimplicialMesh mesh)
    {
        var coordToVertex = new Dictionary<(int, int, int, int), int>();
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            var c = mesh.GetVertexCoordinates(v);
            coordToVertex[(
                (int)System.Math.Round(c[0]), (int)System.Math.Round(c[1]),
                (int)System.Math.Round(c[2]), (int)System.Math.Round(c[3]))] = v;
        }

        int Wrap(int x) => ((x % TorusN) + TorusN) % TorusN;

        int[][] samples =
        [
            [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1],
            [1, 1, 0, 0], [2, 1, 2, 0],
        ];

        var maps = new List<int[]>();
        foreach (var r in samples)
        {
            var map = new int[mesh.VertexCount];
            for (int v = 0; v < mesh.VertexCount; v++)
            {
                var c = mesh.GetVertexCoordinates(v);
                map[v] = coordToVertex[(
                    Wrap((int)System.Math.Round(c[0]) + r[0]), Wrap((int)System.Math.Round(c[1]) + r[1]),
                    Wrap((int)System.Math.Round(c[2]) + r[2]), Wrap((int)System.Math.Round(c[3]) + r[3]))];
            }

            maps.Add(map);
        }

        return maps;
    }

    /// <summary>
    /// The signed edge permutation of phase444: transport omega along the vertex map;
    /// a connection 1-form negates when its stored (min, max) edge direction reverses.
    /// </summary>
    private static double[] SignedPermute(SimplicialMesh mesh, double[] omega, int dimG, int[] vertexMap)
    {
        var edgeLookup = new Dictionary<(int, int), int>(mesh.EdgeCount);
        for (int e = 0; e < mesh.EdgeCount; e++)
            edgeLookup[(mesh.Edges[e][0], mesh.Edges[e][1])] = e;

        var result = new double[omega.Length];
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int a = vertexMap[mesh.Edges[e][0]];
            int b = vertexMap[mesh.Edges[e][1]];
            int target = edgeLookup[(System.Math.Min(a, b), System.Math.Max(a, b))];
            int sign = a < b ? +1 : -1;
            for (int c = 0; c < dimG; c++)
                result[target * dimG + c] = sign * omega[e * dimG + c];
        }

        return result;
    }

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "lattice-canonical-covariance-test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "draft-2021",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 4,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-2" },
    };

    private static GeometryContext TestGeometry()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 4 };
        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
            },
            Patches = Array.Empty<PatchInfo>(),
        };
    }
}
