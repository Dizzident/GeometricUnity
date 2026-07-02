using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// M3 acceptance tests for the Einsteinian Shiab candidate family
/// (<see cref="EinsteinianShiabOperator"/>, <see cref="Lambda2Algebra"/>,
/// <see cref="EinsteinianShiabBatteries"/>). Covers: the standalone Lambda^2
/// invariant-element algebra; the control-anchor exactness (id0/none reduces to
/// identity-Shiab); numerical distinctness of members on a 4D mesh (the 2D blocker
/// lifts); registry carrier-match; the richness certificate (control fails, sd2
/// passes); Weyl annihilation; analytic-vs-finite-difference Linearize for the
/// linear modes; Hessian self-adjointness for all three eps modes; gauge covariance;
/// and an informational omega-coupled degree probe.
/// </summary>
public class EinsteinianShiab4DTests
{
    // ===== Infrastructure =====

    private static SimplicialMesh Mesh4D() => SimplicialMeshGenerator.CreateUniform4D(1);

    private static BranchManifest Manifest() => new()
    {
        BranchId = "einsteinian-shiab-test",
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
            ProjectionBinding = new GeometryBinding
            { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
            ObservationBinding = new GeometryBinding
            { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    private static ConnectionField RandomOmega(SimplicialMesh mesh, LieAlgebra algebra, int seed, double scale = 0.1)
    {
        var rng = new Random(seed);
        var conn = new ConnectionField(mesh, algebra);
        for (int i = 0; i < conn.Coefficients.Length; i++)
            conn.Coefficients[i] = scale * (rng.NextDouble() - 0.5);
        return conn;
    }

    private static EinsteinianShiabFamilyMember ControlAnchor() => new()
    {
        Phi1 = InvariantElementSpec.Id0,
        Phi2 = InvariantElementSpec.None,
        EpsilonMode = "trivial",
    };

    private static EinsteinianShiabFamilyMember ScalarControl() => new()
    {
        Phi1 = InvariantElementSpec.Id0,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5,
        EpsilonMode = "trivial",
    };

    private static EinsteinianShiabFamilyMember Sd2Member(string epsilonMode = "trivial") => new()
    {
        Phi1 = InvariantElementSpec.Sd2,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5,
        EpsilonMode = epsilonMode,
    };

    private static EinsteinianShiabFamilyMember Asd2Member() => new()
    {
        Phi1 = InvariantElementSpec.Asd2,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5,
        EpsilonMode = "trivial",
    };

    // ===== Lambda^2 algebra (standalone, mesh-independent) =====

    [Fact]
    public void HodgeStar_SquaresToIdentity()
    {
        var star = Lambda2Algebra.HodgeStar();
        var sq = Lambda2Algebra.Multiply(star, star);
        var id = Lambda2Algebra.Identity(Lambda2Algebra.Dim);
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                Assert.Equal(id[i, j], sq[i, j], 12);
    }

    [Fact]
    public void Projectors_AreComplementaryAndIdempotent()
    {
        var pPlus = Lambda2Algebra.SelfDualProjector();
        var pMinus = Lambda2Algebra.AntiSelfDualProjector();
        var id = Lambda2Algebra.Identity(6);

        // P+ + P- = I
        var sum = Lambda2Algebra.ScaleAdd(pPlus, 1.0, pMinus, 1.0);
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                Assert.Equal(id[i, j], sum[i, j], 12);

        // P+^2 = P+ (idempotent)
        var pp = Lambda2Algebra.Multiply(pPlus, pPlus);
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                Assert.Equal(pPlus[i, j], pp[i, j], 12);

        // P+ P- = 0 (orthogonal)
        var cross = Lambda2Algebra.Multiply(pPlus, pMinus);
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                Assert.Equal(0.0, cross[i, j], 12);
    }

    [Fact]
    public void Projectors_HaveRankThree()
    {
        // trace of a projector = its rank
        var pPlus = Lambda2Algebra.SelfDualProjector();
        double trace = 0;
        for (int i = 0; i < 6; i++) trace += pPlus[i, i];
        Assert.Equal(3.0, trace, 12);
    }

    [Fact]
    public void ControlAnchorEndomorphism_IsExactlyIdentity()
    {
        var r = Lambda2Algebra.MemberEndomorphism(ControlAnchor());
        var id = Lambda2Algebra.Identity(6);
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
                Assert.Equal(id[i, j], r[i, j], 14);
    }

    [Fact]
    public void ScalarControlEndomorphism_IsScalarMultiple_ZeroDeviation()
    {
        // {id0,id0,c=0.5} -> R = (1-c) I = 0.5 I : proportional to identity.
        var r = Lambda2Algebra.MemberEndomorphism(ScalarControl());
        Assert.Equal(0.0, Lambda2Algebra.ScalarDeviation(r), 12);
    }

    [Fact]
    public void Sd2Endomorphism_IsNonScalar_PositiveDeviation()
    {
        var r = Lambda2Algebra.MemberEndomorphism(Sd2Member());
        Assert.True(Lambda2Algebra.ScalarDeviation(r) > 0.1,
            "sd2 member must be genuinely non-scalar on Lambda^2.");
    }

    [Fact]
    public void MatrixExp_OfZero_IsIdentity()
    {
        var zero = new double[3, 3];
        var e = Lambda2Algebra.MatrixExp(zero);
        var id = Lambda2Algebra.Identity(3);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.Equal(id[i, j], e[i, j], 12);
    }

    [Fact]
    public void Invert_RoundTrips()
    {
        var a = new double[,] { { 4, 1, 0 }, { 1, 3, 1 }, { 0, 1, 2 } };
        var inv = Lambda2Algebra.Invert(a);
        var prod = Lambda2Algebra.Multiply(a, inv);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.Equal(i == j ? 1.0 : 0.0, prod[i, j], 10);
    }

    // ===== Control anchor: exact reproduction of identity-Shiab =====

    [Fact]
    public void ControlAnchor_ReproducesIdentityShiab_Exactly()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new EinsteinianShiabOperator(mesh, algebra, ControlAnchor());
        var identity = new IdentityShiabCpu(mesh, algebra);

        var omega = RandomOmega(mesh, algebra, 1);
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();

        var sEin = op.Evaluate(f, omegaT, Manifest(), Geometry());
        var sId = identity.Evaluate(f, omegaT, Manifest(), Geometry());

        double maxDiff = 0;
        for (int i = 0; i < sId.Coefficients.Length; i++)
            maxDiff = System.Math.Max(maxDiff, System.Math.Abs(sEin.Coefficients[i] - sId.Coefficients[i]));

        Assert.True(maxDiff < 1e-12, $"Control anchor must reproduce identity-Shiab exactly; max|diff|={maxDiff:G6}.");
    }

    [Fact]
    public void ControlAnchor_OutputSignature_MatchesIdentityShiab()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new EinsteinianShiabOperator(mesh, algebra, ControlAnchor());
        var identity = new IdentityShiabCpu(mesh, algebra);

        var a = op.OutputSignature;
        var b = identity.OutputSignature;
        Assert.Equal(b.AmbientSpaceId, a.AmbientSpaceId);
        Assert.Equal(b.CarrierType, a.CarrierType);
        Assert.Equal(b.Degree, a.Degree);
        Assert.Equal(b.LieAlgebraBasisId, a.LieAlgebraBasisId);
        Assert.Equal(b.ComponentOrderId, a.ComponentOrderId);
        Assert.Equal(b.NumericPrecision, a.NumericPrecision);
        Assert.Equal(b.MemoryLayout, a.MemoryLayout);
    }

    // ===== Distinct members differ on a 4D mesh (the 2D blocker lifts) =====

    [Fact]
    public void DistinctMembers_ProduceDistinctOutputs_On4DMesh()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = RandomOmega(mesh, algebra, 7);
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
        var omegaT = omega.ToFieldTensor();

        var sd2 = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());
        var asd2 = new EinsteinianShiabOperator(mesh, algebra, Asd2Member());
        var control = new EinsteinianShiabOperator(mesh, algebra, ControlAnchor());

        var sSd2 = sd2.Evaluate(f, omegaT, Manifest(), Geometry());
        var sAsd2 = asd2.Evaluate(f, omegaT, Manifest(), Geometry());
        var sControl = control.Evaluate(f, omegaT, Manifest(), Geometry());

        double sdVsAsd = MaxDiff(sSd2.Coefficients, sAsd2.Coefficients);
        double sdVsControl = MaxDiff(sSd2.Coefficients, sControl.Coefficients);

        Assert.True(sdVsAsd > 1e-6, $"sd2 and asd2 must differ on a 4D mesh; max|diff|={sdVsAsd:G6}.");
        Assert.True(sdVsControl > 1e-6, $"sd2 must differ from identity control; max|diff|={sdVsControl:G6}.");
    }

    // ===== Registry carrier match =====

    [Fact]
    public void CarrierMatch_PassesForMenuOfMembers()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var menu = new[]
        {
            ControlAnchor(), ScalarControl(), Sd2Member(), Asd2Member(),
            new EinsteinianShiabFamilyMember
            { Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Vol4, EpsilonMode = "trivial" },
            Sd2Member("frozen-background"), Sd2Member("omega-coupled"),
        };

        foreach (var member in menu)
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, member);
            // Must not throw.
            BranchOperatorRegistry.ValidateCarrierMatch(torsion, op);
            Assert.True(EinsteinianShiabBatteries.CarrierMatches(torsion, op), member.BranchId);
        }
    }

    // ===== Richness certificate: control FAILS, sd2 PASSES =====

    [Fact]
    public void RichnessCertificate_ScalarControlFails_Sd2Passes()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();

        var control = new EinsteinianShiabOperator(mesh, algebra, ScalarControl());
        var sd2 = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());

        Assert.False(EinsteinianShiabBatteries.IsRich(control),
            "The scalar control (id0,id0) must FAIL the richness certificate (expected control outcome).");
        Assert.True(EinsteinianShiabBatteries.IsRich(sd2),
            "The sd2 member must PASS the richness certificate.");
    }

    // ===== Weyl annihilation =====

    [Fact]
    public void WeylAnnihilation_Sd2AnnihilatesAntiSelfDual()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var sd2 = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());

        double residual = EinsteinianShiabBatteries.WeylAnnihilationResidual(
            sd2, mesh, algebra, EinsteinianShiabBatteries.AntiSelfDualSample(), Manifest(), Geometry());

        Assert.True(residual < 1e-10,
            $"Self-dual member must annihilate an anti-self-dual 2-form; residual={residual:G6}.");
    }

    [Fact]
    public void WeylAnnihilation_Asd2AnnihilatesSelfDual()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var asd2 = new EinsteinianShiabOperator(mesh, algebra, Asd2Member());

        double residual = EinsteinianShiabBatteries.WeylAnnihilationResidual(
            asd2, mesh, algebra, EinsteinianShiabBatteries.SelfDualSample(), Manifest(), Geometry());

        Assert.True(residual < 1e-10,
            $"Anti-self-dual member must annihilate a self-dual 2-form; residual={residual:G6}.");
    }

    [Fact]
    public void WeylAnnihilation_Sd2DoesNotAnnihilateSelfDual()
    {
        // Sanity: the sd2 member must NOT kill the self-dual sector (else it is trivial).
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var sd2 = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());

        double residual = EinsteinianShiabBatteries.WeylAnnihilationResidual(
            sd2, mesh, algebra, EinsteinianShiabBatteries.SelfDualSample(), Manifest(), Geometry());

        Assert.True(residual > 1e-6, "sd2 member should retain the self-dual sector.");
    }

    // ===== Linearize: analytic matches FD for the linear modes =====

    [Fact]
    public void Linearize_Trivial_MatchesFiniteDifference()
    {
        AssertLinearizeMatchesFd(Sd2Member("trivial"));
    }

    [Fact]
    public void Linearize_FrozenBackground_MatchesFiniteDifference()
    {
        AssertLinearizeMatchesFd(Sd2Member("frozen-background"), backgroundEps: new[] { 0.3, 0.1, 0.2 });
    }

    private void AssertLinearizeMatchesFd(EinsteinianShiabFamilyMember member, double[]? backgroundEps = null)
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new EinsteinianShiabOperator(mesh, algebra, member, backgroundEps);
        var manifest = Manifest();
        var geometry = Geometry();

        var omega = RandomOmega(mesh, algebra, 11);
        var delta = RandomOmega(mesh, algebra, 12);
        var omegaT = omega.ToFieldTensor();
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();

        var dS = op.Linearize(f, omegaT, delta.ToFieldTensor(), manifest, geometry);

        double eps = 1e-7;
        var plus = new double[omega.Coefficients.Length];
        var minus = new double[omega.Coefficients.Length];
        for (int i = 0; i < plus.Length; i++)
        {
            plus[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];
            minus[i] = omega.Coefficients[i] - eps * delta.Coefficients[i];
        }
        var connP = new ConnectionField(mesh, algebra, plus);
        var connM = new ConnectionField(mesh, algebra, minus);
        var sP = op.Evaluate(CurvatureAssembler.Assemble(connP).ToFieldTensor(), connP.ToFieldTensor(), manifest, geometry);
        var sM = op.Evaluate(CurvatureAssembler.Assemble(connM).ToFieldTensor(), connM.ToFieldTensor(), manifest, geometry);

        for (int i = 0; i < dS.Coefficients.Length; i++)
        {
            double fd = (sP.Coefficients[i] - sM.Coefficients[i]) / (2 * eps);
            Assert.Equal(fd, dS.Coefficients[i], 5);
        }
    }

    // ===== Hessian self-adjointness for all three eps modes =====

    [Fact]
    public void HessianSymmetry_Trivial_IsSelfAdjoint()
    {
        AssertHessianSymmetric(Sd2Member("trivial"));
    }

    [Fact]
    public void HessianSymmetry_FrozenBackground_IsSelfAdjoint()
    {
        AssertHessianSymmetric(Sd2Member("frozen-background"), backgroundEps: new[] { 0.2, 0.1, 0.15 });
    }

    [Fact]
    [Trait("Gating", "false")] // slaved-Wilson smoke-test mode; not the pinned physics treatment
    public void HessianSymmetry_OmegaCoupled_IsSelfAdjoint()
    {
        AssertHessianSymmetric(Sd2Member("omega-coupled"));
    }

    private void AssertHessianSymmetric(EinsteinianShiabFamilyMember member, double[]? backgroundEps = null)
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var op = new EinsteinianShiabOperator(mesh, algebra, member, backgroundEps, omegaCouplingKappa: 0.5);

        var omega = RandomOmega(mesh, algebra, 21, scale: 0.15).Coefficients;
        double residual = EinsteinianShiabBatteries.HessianSymmetryResidual(
            op, mesh, algebra, omega, Manifest(), Geometry(), subspaceDim: 6);

        Assert.True(residual < 1e-2,
            $"Hessian must be self-adjoint for {member.EpsilonMode}; relative asymmetry={residual:G6}.");
    }

    // ===== Gauge covariance =====

    [Fact]
    public void GaugeCovariance_Trivial_IsExact()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());

        var omega = RandomOmega(mesh, algebra, 31);
        var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();

        double residual = EinsteinianShiabBatteries.GaugeCovarianceResidual(
            op, mesh, algebra, f.Coefficients, new[] { 0.4, 0.2, 0.1 }, Manifest(), Geometry());

        Assert.True(residual < 1e-10,
            $"Trivial-eps Shiab must be exactly gauge covariant; residual={residual:G6}.");
    }

    // ===== omega-coupled = SLAVED-WILSON SMOKE-TEST (NON-GATING) =====
    // RESOLVED (2026-07-02): the physicist LOCKED independent-theta (a genuine independent field
    // with a joint (omega,theta) Hessian) as the pinned omega-coupled physics treatment (§6e).
    // The slaved Wilson eps(omega)=exp(kappa*sum omega_e) realization SURVIVES as an OPTIONAL,
    // LABELED, NON-GATING smoke-test — NOT the pinned treatment. These tests carry
    // [Trait("Gating","false")] so QA excludes them from the M3 acceptance gate. The true
    // independent-theta arm (with its gating controls: theta=0 reproduces Phase436 degree-2, and
    // LinearizeTheta matches FD) is implemented only when the co-signed §3.5 lands (possibly M3b).

    [Fact]
    [Trait("Gating", "false")]
    public void OmegaCoupled_AtZeroConnection_EqualsTrivial()
    {
        // theta = kappa * sum omega_e = 0 at omega=0 => Ad = I => same as trivial.
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var coupled = new EinsteinianShiabOperator(mesh, algebra, Sd2Member("omega-coupled"));
        var trivial = new EinsteinianShiabOperator(mesh, algebra, Sd2Member("trivial"));

        // Feed a nonzero curvature but ZERO omega (so theta=0).
        var f = EinsteinianShiabBatteries.ConstantTwoForm(mesh, algebra, EinsteinianShiabBatteries.SelfDualSample(), 0);
        var fT = FaceTensor(mesh, algebra, f);
        var zeroOmega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var sC = coupled.Evaluate(fT, zeroOmega, Manifest(), Geometry());
        var sT = trivial.Evaluate(fT, zeroOmega, Manifest(), Geometry());
        Assert.True(MaxDiff(sC.Coefficients, sT.Coefficients) < 1e-12,
            "omega-coupled at omega=0 must equal trivial (theta=0 => Ad=I).");
    }

    [Fact]
    [Trait("Gating", "false")]
    public void OmegaCoupled_IsNonlinearInOmega()
    {
        // The eps(omega) conjugation makes Evaluate genuinely nonlinear in omega:
        // S(F, 2*omega) != 2*S(F, omega) - S(F, 0) (an affine map would satisfy equality).
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new EinsteinianShiabOperator(mesh, algebra, Sd2Member("omega-coupled"), omegaCouplingKappa: 1.0);

        var f = EinsteinianShiabBatteries.ConstantTwoForm(mesh, algebra, EinsteinianShiabBatteries.SelfDualSample(), 0);
        var fT = FaceTensor(mesh, algebra, f);

        var omega = RandomOmega(mesh, algebra, 41, scale: 0.4);
        var twoOmega = new double[omega.Coefficients.Length];
        for (int i = 0; i < twoOmega.Length; i++) twoOmega[i] = 2 * omega.Coefficients[i];

        var s0 = op.Evaluate(fT, ConnectionField.Zero(mesh, algebra).ToFieldTensor(), Manifest(), Geometry());
        var s1 = op.Evaluate(fT, omega.ToFieldTensor(), Manifest(), Geometry());
        var s2 = op.Evaluate(fT, new ConnectionField(mesh, algebra, twoOmega).ToFieldTensor(), Manifest(), Geometry());

        // affine deviation: ||S(2w) - (2 S(w) - S(0))||
        double dev = 0;
        for (int i = 0; i < s0.Coefficients.Length; i++)
            dev = System.Math.Max(dev,
                System.Math.Abs(s2.Coefficients[i] - (2 * s1.Coefficients[i] - s0.Coefficients[i])));

        Assert.True(dev > 1e-6,
            $"omega-coupled Evaluate must be nonlinear in omega (affine deviation={dev:G6}).");
    }

    // ===== kappa-scan degree batteries — SMOKE-TEST (NON-GATING) =====
    // These batteries move WITH the slaved-Wilson smoke-test (see the section note above): they
    // are non-gating and carry [Trait("Gating","false")]. They validate that the smoke-test mode
    // is wired correctly (kappa=0 => degree-2; growth with kappa), not the pinned physics.

    [Fact]
    [Trait("Gating", "false")]
    public void OmegaCoupled_KappaZero_ReproducesDegreeTwoHessian()
    {
        // Battery (i): kappa=0 must reproduce the Phase436 degree-2 Hessian exactly
        // (vanishing third t-difference), because eps = exp(0) = 1 => S_h linear in omega.
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var member = Sd2Member("omega-coupled");
        var op = new EinsteinianShiabOperator(mesh, algebra, member, null, omegaCouplingKappa: 0.0);

        int n = mesh.EdgeCount * algebra.Dimension;
        var u = RandomVector(n, 55);
        var v = RandomVector(n, 77);

        double d3 = EinsteinianShiabBatteries.HessianThirdTDifference(
            op, mesh, algebra, u, v, Manifest(), Geometry());

        Assert.True(System.Math.Abs(d3) < 1e-6,
            $"kappa=0 must reproduce a degree-2 Hessian (vanishing third t-difference); |D3|={d3:G6}.");
    }

    [Fact]
    [Trait("Gating", "false")]
    public void OmegaCoupled_ThirdTDifference_GrowsMonotonicallyWithKappa()
    {
        // Battery (ii): the third t-difference must grow monotonically with kappa in the
        // perturbative window, separating a genuine degree-lift from a numerical artifact.
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var member = Sd2Member("omega-coupled");

        int n = mesh.EdgeCount * algebra.Dimension;
        var u = RandomVector(n, 55);
        var v = RandomVector(n, 77);

        var kappas = new[] { 0.0, 0.25, 0.5, 1.0 };
        var d3 = EinsteinianShiabBatteries.KappaScanThirdTDifference(
            mesh, algebra, member, kappas, u, v, Manifest(), Geometry());

        Assert.True(d3[0] < 1e-6, $"kappa=0 control must vanish; |D3|={d3[0]:G6}.");
        for (int i = 1; i < d3.Length; i++)
            Assert.True(d3[i] > d3[i - 1],
                $"third t-difference must grow with kappa: |D3|[{i}]={d3[i]:G6} !> |D3|[{i - 1}]={d3[i - 1]:G6}.");
    }

    // ===== helpers =====

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
        for (int i = 0; i < a.Length; i++)
            max = System.Math.Max(max, System.Math.Abs(a[i] - b[i]));
        return max;
    }

    private static FieldTensor FaceTensor(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs) =>
        new()
        {
            Label = "F",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = algebra.BasisOrderId,
                ComponentOrderId = "face-major",
                NumericPrecision = "float64",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { mesh.FaceCount, algebra.Dimension },
        };
}
