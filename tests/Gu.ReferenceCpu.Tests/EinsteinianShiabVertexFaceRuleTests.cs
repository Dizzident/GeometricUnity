using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// Tests for the additive <see cref="VertexFaceRule"/> option (lowest-index default vs
/// incident-average) and the minimal-image lattice-period convention on the Einsteinian Shiab
/// operator (Phase444 authorized platform change). These assert exactly what HOLDS:
/// <list type="number">
///   <item>Open-mesh byte-identity: the lowest-index default is unchanged across the member menu,
///         and the two rules agree at theta = 0 (both reduce to the pure M-contraction).</item>
///   <item>The incident-average rule's analytic LinearizeTheta matches finite difference (the
///         chain-rule weight 1/|incident| is folded in correctly).</item>
///   <item>Minimal-image reduction makes the periodic-torus face bivectors orbit-invariant
///         (exact 0), whereas the raw differences are not; and the operator's latticePeriod flag
///         actually changes the periodic-mesh contraction.</item>
///   <item>RECORDED LIMITATION: even incident-average + minimal-image does NOT make the operator
///         translation-covariant on the torus (the global-index orientation conventions upstream in
///         curvature/mesh assembly do not commute with lattice translation). This documents the
///         measured residual as a known limitation; it does not assert the residual vanishes.</item>
/// </list>
/// </summary>
public class EinsteinianShiabVertexFaceRuleTests
{
    private static BranchManifest Manifest() => new()
    {
        BranchId = "vertex-face-rule-test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "draft-2021",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "einsteinian-shiab",
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

    private static GeometryContext Geometry()
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
            ProjectionBinding = new GeometryBinding { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
            ObservationBinding = new GeometryBinding { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    // The Einsteinian member menu (independent-theta), each in a lowest-index and incident-average form.
    private static IEnumerable<(string tag, InvariantElementSpec phi1, double c)> Menu() => new[]
    {
        ("sd2-id0/c0.5", InvariantElementSpec.Sd2, 0.5),
        ("sd2-id0/c1", InvariantElementSpec.Sd2, 1.0),
        ("asd2-id0/c0.5", InvariantElementSpec.Asd2, 0.5),
        ("id0-id0/c0.5", InvariantElementSpec.Id0, 0.5),
    };

    private static EinsteinianShiabFamilyMember Member(InvariantElementSpec phi1, double c, VertexFaceRule rule) => new()
    {
        Phi1 = phi1,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = c,
        EpsilonMode = "independent-theta",
        VertexFaceRule = rule,
    };

    private static double[] RandomVector(int n, int seed, double scale = 1.0)
    {
        var rng = new Random(seed);
        var a = new double[n];
        for (int i = 0; i < n; i++) a[i] = scale * (rng.NextDouble() - 0.5);
        return a;
    }

    private static double MaxDiff(double[] a, double[] b)
    {
        double max = 0;
        for (int i = 0; i < a.Length; i++) max = System.Math.Max(max, System.Math.Abs(a[i] - b[i]));
        return max;
    }

    // ===== (1) open-mesh byte-identity =====

    [Fact]
    public void VertexFaceRule_DefaultsToLowestIndex()
    {
        var m = new EinsteinianShiabFamilyMember { Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0, EpsilonMode = "independent-theta" };
        Assert.Equal(VertexFaceRule.LowestIndex, m.VertexFaceRule);
        // The default BranchId carries no rule suffix (all prior BranchIds byte-identical).
        Assert.DoesNotContain("/avg", m.BranchId);
        Assert.EndsWith("/independent-theta", m.BranchId);
        // Incident-average appends the "/avg" suffix.
        var mAvg = Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage);
        Assert.EndsWith("/avg", mAvg.BranchId);
    }

    [Fact]
    public void OpenMesh_DefaultMember_ByteIdenticalToExplicitLowestIndex()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int nTheta = mesh.VertexCount * algebra.Dimension;
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 11, 0.2));
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();
        var theta = RandomVector(nTheta, 12, 0.15);

        foreach (var (tag, phi1, c) in Menu())
        {
            // Default member (no VertexFaceRule set) must equal explicit LowestIndex, exactly.
            var mDefault = new EinsteinianShiabFamilyMember { Phi1 = phi1, Phi2 = InvariantElementSpec.Id0, EinsteinCoefficient = c, EpsilonMode = "independent-theta" };
            var opDefault = new EinsteinianShiabOperator(mesh, algebra, mDefault);
            var opLowest = new EinsteinianShiabOperator(mesh, algebra, Member(phi1, c, VertexFaceRule.LowestIndex));

            var sDefault = opDefault.EvaluateWithTheta(f, omegaT, theta, Manifest(), Geometry()).Coefficients;
            var sLowest = opLowest.EvaluateWithTheta(f, omegaT, theta, Manifest(), Geometry()).Coefficients;
            Assert.True(MaxDiff(sDefault, sLowest) == 0.0, $"{tag}: default member must be byte-identical to explicit LowestIndex.");
        }
    }

    [Fact]
    public void OpenMesh_ThetaZero_AverageEqualsLowestIndexExactly()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int nTheta = mesh.VertexCount * algebra.Dimension;
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 21, 0.2));
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();
        var zeroTheta = new double[nTheta];

        foreach (var (tag, phi1, c) in Menu())
        {
            var opLow = new EinsteinianShiabOperator(mesh, algebra, Member(phi1, c, VertexFaceRule.LowestIndex));
            var opAvg = new EinsteinianShiabOperator(mesh, algebra, Member(phi1, c, VertexFaceRule.IncidentAverage));
            var sLow = opLow.EvaluateWithTheta(f, omegaT, zeroTheta, Manifest(), Geometry()).Coefficients;
            var sAvg = opAvg.EvaluateWithTheta(f, omegaT, zeroTheta, Manifest(), Geometry()).Coefficients;
            Assert.True(MaxDiff(sLow, sAvg) < 1e-14, $"{tag}: theta=0 must give identical S_h for both vertex-face rules (both = M(F)).");
        }
    }

    // ===== (2) incident-average analytic LinearizeTheta vs FD =====

    [Fact]
    public void IncidentAverage_LinearizeTheta_MatchesFiniteDifference()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var op = new EinsteinianShiabOperator(mesh, algebra, Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage));

        int nTheta = mesh.VertexCount * algebra.Dimension;
        var omega = RandomVector(mesh.EdgeCount * algebra.Dimension, 31, 0.2);
        var theta = RandomVector(nTheta, 32, 0.15);
        var dTheta = RandomVector(nTheta, 33, 1.0);

        double residual = EinsteinianShiabBatteries.LinearizeThetaFdResidual(
            op, mesh, algebra, omega, theta, dTheta, Manifest(), Geometry());
        Assert.True(residual < 1e-5,
            $"Incident-average analytic LinearizeTheta must match finite difference; max|diff|={residual:G6}.");
    }

    [Fact]
    public void IncidentAverage_CouplesTheta_DiffersFromLowestIndexAtNonzeroTheta()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int nTheta = mesh.VertexCount * algebra.Dimension;
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 41, 0.2));
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();
        var theta = RandomVector(nTheta, 42, 0.3);

        var opLow = new EinsteinianShiabOperator(mesh, algebra, Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.LowestIndex));
        var opAvg = new EinsteinianShiabOperator(mesh, algebra, Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage));
        var sLow = opLow.EvaluateWithTheta(f, omegaT, theta, Manifest(), Geometry()).Coefficients;
        var sAvg = opAvg.EvaluateWithTheta(f, omegaT, theta, Manifest(), Geometry()).Coefficients;
        Assert.True(MaxDiff(sLow, sAvg) > 1e-6, "At nonzero theta the two rules must differ (the rule is wired, not a no-op).");
    }

    // ===== (3) minimal-image bivector orbit-invariance + latticePeriod is active =====

    [Fact]
    public void MinimalImage_FaceBivectorNorms_OrbitInvariantOnTorus_RawAreNot()
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n);

        double[] Biv(int fc, bool minImage)
        {
            var verts = mesh.Faces[fc];
            var pa = mesh.GetVertexCoordinates(verts[0]);
            var pb = mesh.GetVertexCoordinates(verts[1]);
            var pc = mesh.GetVertexCoordinates(verts[2]);
            var u = new double[4]; var v = new double[4];
            for (int d = 0; d < 4; d++)
            {
                u[d] = pb[d] - pa[d]; v[d] = pc[d] - pa[d];
                if (minImage) { u[d] -= n * System.Math.Round(u[d] / n); v[d] -= n * System.Math.Round(v[d] / n); }
            }
            return Lambda2Algebra.Wedge(u, v);
        }
        double Norm(double[] x) { double s = 0; foreach (var t in x) s += t * t; return System.Math.Sqrt(s); }

        var (translate, translateFace) = TorusMaps(mesh, n);
        var Rs = new[] { new[] { 1, 0, 0, 0 }, new[] { 0, 1, 0, 0 }, new[] { 1, 1, 0, 0 } };

        double maxRawDiff = 0, maxMinDiff = 0;
        for (int fc = 0; fc < mesh.FaceCount; fc++)
            foreach (var R in Rs)
            {
                int g = translateFace(fc, R);
                maxRawDiff = System.Math.Max(maxRawDiff, System.Math.Abs(Norm(Biv(fc, false)) - Norm(Biv(g, false))));
                maxMinDiff = System.Math.Max(maxMinDiff, System.Math.Abs(Norm(Biv(fc, true)) - Norm(Biv(g, true))));
            }
        Assert.True(maxRawDiff > 1.0, $"Raw torus bivectors must NOT be orbit-invariant (seam inflation); max diff={maxRawDiff:G6}.");
        Assert.True(maxMinDiff < 1e-12, $"Minimal-image torus bivectors must be orbit-invariant exactly; max diff={maxMinDiff:G6}.");
    }

    [Fact]
    public void LatticePeriod_ChangesPeriodicMeshContraction()
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var member = Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage);
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 51, 0.2));
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();
        var zeroTheta = new double[mesh.VertexCount * algebra.Dimension];

        var opRaw = new EinsteinianShiabOperator(mesh, algebra, member);                 // latticePeriod = 0
        var opMin = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: n);
        var sRaw = opRaw.EvaluateWithTheta(f, omegaT, zeroTheta, Manifest(), Geometry()).Coefficients;
        var sMin = opMin.EvaluateWithTheta(f, omegaT, zeroTheta, Manifest(), Geometry()).Coefficients;
        Assert.True(MaxDiff(sRaw, sMin) > 1e-6, "latticePeriod>0 must change the periodic-mesh contraction (minimal-image is active).");
    }

    [Fact]
    public void PerAxisLatticePeriods_IsotropicVectorExactlyMatchesScalarOperator()
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var member = Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage);
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 510, 0.2));
        var curvature = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var theta = new double[mesh.VertexCount * algebra.Dimension];

        var scalar = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: n);
        var vector = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriods: [n, n, n, n]);
        double[] scalarResult = scalar.EvaluateWithTheta(curvature, omega.ToFieldTensor(), theta, Manifest(), Geometry()).Coefficients;
        double[] vectorResult = vector.EvaluateWithTheta(curvature, omega.ToFieldTensor(), theta, Manifest(), Geometry()).Coefficients;

        Assert.Equal(scalarResult, vectorResult);
    }

    [Fact]
    public void PerAxisLatticePeriods_ChangeAnisotropicSeamContraction()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, 3, 3, 4, latticeCanonical: true);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var member = Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage);
        var omega = new ConnectionField(mesh, algebra, RandomVector(mesh.EdgeCount * algebra.Dimension, 511, 0.2));
        var curvature = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var theta = new double[mesh.VertexCount * algebra.Dimension];

        var raw = new EinsteinianShiabOperator(mesh, algebra, member);
        var minimal = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriods: [3, 3, 3, 4]);
        double[] rawResult = raw.EvaluateWithTheta(curvature, omega.ToFieldTensor(), theta, Manifest(), Geometry()).Coefficients;
        double[] minimalResult = minimal.EvaluateWithTheta(curvature, omega.ToFieldTensor(), theta, Manifest(), Geometry()).Coefficients;

        Assert.True(MaxDiff(rawResult, minimalResult) > 1e-6, "Per-axis minimal-image reduction must affect an anisotropic seam contraction.");
    }

    // ===== (4) RECORDED LIMITATION: not translation-covariant on the torus =====

    [Fact]
    public void PeriodicMesh_NotTranslationCovariant_RecordedLimitation()
    {
        // Even incident-average + minimal-image does NOT make the operator translation-covariant on
        // the torus: the curvature/mesh orientation conventions (global-index sort) do not commute
        // with lattice translation. This test DOCUMENTS the measured residual as a known limitation
        // (it asserts non-covariance is present and bounded, NOT that it vanishes).
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n);
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        int nOmega = mesh.EdgeCount * dimG;
        int nTheta = mesh.VertexCount * dimG;
        var op = new EinsteinianShiabOperator(mesh, algebra, Member(InvariantElementSpec.Sd2, 0.5, VertexFaceRule.IncidentAverage), latticePeriod: n);
        var mass = new CpuMassMatrix(mesh, algebra);

        double SB(double[] omega, double[] theta)
        {
            var conn = new ConnectionField(mesh, algebra, omega);
            var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
            var s = op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, Manifest(), Geometry());
            return 0.5 * mass.InnerProduct(s, s);
        }

        var (translate, _) = TorusMaps(mesh, n);
        var edgeLookup = new Dictionary<(int, int), int>();
        for (int e = 0; e < mesh.EdgeCount; e++) { var ee = mesh.Edges[e]; edgeLookup[(System.Math.Min(ee[0], ee[1]), System.Math.Max(ee[0], ee[1]))] = e; }

        (double[] o, double[] t) SignedPermute(double[] omega, double[] theta, int[] R)
        {
            var no = new double[nOmega];
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                var ee = mesh.Edges[e];
                int a = translate(ee[0], R), b = translate(ee[1], R);
                int e2 = edgeLookup[(System.Math.Min(a, b), System.Math.Max(a, b))];
                int sign = a < b ? 1 : -1;
                for (int c = 0; c < dimG; c++) no[e2 * dimG + c] = sign * omega[e * dimG + c];
            }
            var nt = new double[nTheta];
            for (int v = 0; v < mesh.VertexCount; v++)
                for (int c = 0; c < dimG; c++) nt[translate(v, R) * dimG + c] = theta[v * dimG + c];
            return (no, nt);
        }

        var omega0 = RandomVector(nOmega, 71, 0.4);
        var theta0 = RandomVector(nTheta, 72, 0.2);
        double s0 = SB(omega0, theta0);
        double maxRel = 0;
        foreach (var R in new[] { new[] { 1, 0, 0, 0 }, new[] { 0, 1, 0, 0 }, new[] { 1, 1, 0, 0 }, new[] { 2, 1, 2, 0 } })
        {
            var (po, pt) = SignedPermute(omega0, theta0, R);
            maxRel = System.Math.Max(maxRel, System.Math.Abs(SB(po, pt) - s0) / (System.Math.Abs(s0) + 1e-300));
        }

        // Recorded limitation: the residual is present (well above numerical noise) and bounded.
        Assert.True(maxRel > 1e-4,
            $"Recorded limitation: the operator is NOT translation-covariant on the torus; residual {maxRel:G6} should be present (> 1e-4).");
        Assert.True(maxRel < 1e-1,
            $"Recorded limitation: the non-covariance residual is bounded (measured ~2.5e-3); got {maxRel:G6}.");
    }

    // ===== helpers =====

    // Builds vertex- and face-translation maps on the (Z_n)^4 torus.
    private static (Func<int, int[], int> translate, Func<int, int[], int> translateFace) TorusMaps(SimplicialMesh mesh, int n)
    {
        int[] Coord(int v)
        {
            var c = mesh.GetVertexCoordinates(v);
            return new[] { (int)System.Math.Round(c[0]), (int)System.Math.Round(c[1]), (int)System.Math.Round(c[2]), (int)System.Math.Round(c[3]) };
        }
        var coordToVertex = new Dictionary<(int, int, int, int), int>();
        for (int v = 0; v < mesh.VertexCount; v++) { var c = Coord(v); coordToVertex[(c[0], c[1], c[2], c[3])] = v; }
        int Translate(int v, int[] R)
        {
            var c = Coord(v);
            return coordToVertex[(((c[0] + R[0]) % n + n) % n, ((c[1] + R[1]) % n + n) % n, ((c[2] + R[2]) % n + n) % n, ((c[3] + R[3]) % n + n) % n)];
        }
        var faceLookup = new Dictionary<(int, int, int), int>();
        for (int fc = 0; fc < mesh.FaceCount; fc++) { var s = (int[])mesh.Faces[fc].Clone(); Array.Sort(s); faceLookup[(s[0], s[1], s[2])] = fc; }
        int TranslateFace(int fc, int[] R)
        {
            var vv = mesh.Faces[fc];
            var s = new[] { Translate(vv[0], R), Translate(vv[1], R), Translate(vv[2], R) };
            Array.Sort(s);
            return faceLookup[(s[0], s[1], s[2])];
        }
        return (Translate, TranslateFace);
    }
}
