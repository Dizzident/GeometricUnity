using System.Diagnostics;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Xunit.Abstractions;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// Gating tests for the reverse-mode adjoint / joint-gradient path (unlock project ii).
/// Three families of checks, following the FD-vs-analytic pattern of
/// <see cref="EinsteinianShiabBatteries.LinearizeThetaFdResidual"/>:
/// <list type="number">
///   <item>Adjoint dot-product identity &lt;J v, w&gt; = &lt;v, J^T w&gt; for each of the three
///         true transposes (contraction, curvature linearization, theta linearization)
///         against its FD-verified forward map, random v/w, several family members.</item>
///   <item>The analytic O(mesh) joint gradient of S_B(omega, theta) against a full
///         central-FD gradient of <see cref="EinsteinianShiabBatteries.JointObjective"/>,
///         plus exact agreement with the legacy column-loop
///         <see cref="CpuLocalJacobian.ComputeGradient"/> at theta = 0.</item>
///   <item>The Hessian-vector product (central FD of the analytic gradient) against
///         dense-FD joint Hessian columns from pure objective second differences.</item>
/// </list>
/// Plus a TIMING measurement (informational, not a hard gate) of per-gradient and
/// per-Hv wall time versus the legacy column-by-column transpose on the same mesh.
/// </summary>
public class EinsteinianShiabAdjointTests
{
    private readonly ITestOutputHelper _output;

    public EinsteinianShiabAdjointTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ===== Infrastructure (matches EinsteinianShiab4DTests conventions) =====

    private static SimplicialMesh Mesh4D() => SimplicialMeshGenerator.CreateUniform4D(1);

    private static BranchManifest Manifest() => new()
    {
        BranchId = "einsteinian-shiab-adjoint-test",
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

    private static EinsteinianShiabFamilyMember ControlAnchor() => new()
    {
        Phi1 = InvariantElementSpec.Id0,
        Phi2 = InvariantElementSpec.None,
        EpsilonMode = "independent-theta",
    };

    private static EinsteinianShiabFamilyMember Sd2Member(
        VertexFaceRule rule = VertexFaceRule.LowestIndex) => new()
    {
        Phi1 = InvariantElementSpec.Sd2,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5,
        EpsilonMode = "independent-theta",
        VertexFaceRule = rule,
    };

    private static EinsteinianShiabFamilyMember Asd2Member(
        VertexFaceRule rule = VertexFaceRule.LowestIndex) => new()
    {
        Phi1 = InvariantElementSpec.Asd2,
        Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5,
        EpsilonMode = "independent-theta",
        VertexFaceRule = rule,
    };

    private static EinsteinianShiabFamilyMember[] MemberMenu() => new[]
    {
        ControlAnchor(),
        Sd2Member(),
        Asd2Member(),
        Sd2Member(VertexFaceRule.IncidentAverage),
    };

    private static double[] RandomVector(int n, int seed, double scale = 1.0)
    {
        var rng = new Random(seed);
        var a = new double[n];
        for (int i = 0; i < n; i++) a[i] = scale * (rng.NextDouble() - 0.5);
        return a;
    }

    private static double Dot(double[] a, double[] b)
    {
        double s = 0;
        for (int i = 0; i < a.Length; i++) s += a[i] * b[i];
        return s;
    }

    private static double MaxAbs(double[] a)
    {
        double m = 0;
        foreach (double x in a) m = System.Math.Max(m, System.Math.Abs(x));
        return m;
    }

    // ===== 1a. Contraction adjoint: <C_theta v, w> = <v, C_theta^T w> =====

    [Fact]
    public void ContractionAdjoint_DotProductIdentity_AcrossMembers()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int nFace = mesh.FaceCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;

        int seed = 100;
        foreach (var member in MemberMenu())
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, member);
            var theta = RandomVector(nTheta, seed++, 0.2);

            // With the theta dressing AND with the trivial (theta = null) dressing.
            foreach (var th in new[] { theta, null })
            {
                var v = RandomVector(nFace, seed++);
                var w = RandomVector(nFace, seed++);

                var jv = op.ApplyContractionWithTheta(v, th);
                var jtw = op.ApplyContractionWithThetaTranspose(w, th);

                double lhs = Dot(jv, w);
                double rhs = Dot(v, jtw);
                double rel = System.Math.Abs(lhs - rhs) /
                    (System.Math.Abs(lhs) + System.Math.Abs(rhs) + 1e-300);
                Assert.True(rel < 1e-10,
                    $"Contraction adjoint identity failed for {member.BranchId} " +
                    $"(theta {(th == null ? "null" : "random")}): <Jv,w>={lhs:G16}, <v,J^Tw>={rhs:G16}, rel={rel:G6}.");
            }
        }
    }

    // ===== 1b. Curvature adjoint: <dF(v), w> = <v, dF^T(w)> =====

    [Fact]
    public void CurvatureAdjoint_DotProductIdentity_AcrossMembers()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int nEdge = mesh.EdgeCount * algebra.Dimension;
        int nFace = mesh.FaceCount * algebra.Dimension;

        int seed = 200;
        foreach (var member in MemberMenu())
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, member);
            var omega = RandomVector(nEdge, seed++, 0.3);
            var v = RandomVector(nEdge, seed++);
            var w = RandomVector(nFace, seed++);

            var jv = op.LinearizeCurvature(omega, v);
            var jtw = op.LinearizeCurvatureTranspose(omega, w);

            double lhs = Dot(jv, w);
            double rhs = Dot(v, jtw);
            double rel = System.Math.Abs(lhs - rhs) /
                (System.Math.Abs(lhs) + System.Math.Abs(rhs) + 1e-300);
            Assert.True(rel < 1e-10,
                $"Curvature adjoint identity failed for {member.BranchId}: " +
                $"<Jv,w>={lhs:G16}, <v,J^Tw>={rhs:G16}, rel={rel:G6}.");
        }
    }

    // ===== 1c. Theta adjoint: <LinearizeTheta(dtheta), w> = <dtheta, LinearizeThetaTranspose(w)> =====

    [Fact]
    public void ThetaAdjoint_DotProductIdentity_AcrossMembers()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = Manifest();
        var geometry = Geometry();
        int nEdge = mesh.EdgeCount * algebra.Dimension;
        int nFace = mesh.FaceCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;

        int seed = 300;
        foreach (var member in MemberMenu())
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, member);

            var omega = new ConnectionField(mesh, algebra, RandomVector(nEdge, seed++, 0.25));
            var f = CurvatureAssembler.Assemble(omega).ToFieldTensor();
            var omegaT = omega.ToFieldTensor();

            var theta = RandomVector(nTheta, seed++, 0.2);
            var dTheta = RandomVector(nTheta, seed++);
            var w = RandomVector(nFace, seed++);

            var jv = op.LinearizeTheta(f, omegaT, theta, dTheta, manifest, geometry).Coefficients;
            var jtw = op.LinearizeThetaTranspose(f.Coefficients, theta, w);

            double lhs = Dot(jv, w);
            double rhs = Dot(dTheta, jtw);
            double rel = System.Math.Abs(lhs - rhs) /
                (System.Math.Abs(lhs) + System.Math.Abs(rhs) + 1e-300);
            Assert.True(rel < 1e-10,
                $"Theta adjoint identity failed for {member.BranchId}: " +
                $"<Jv,w>={lhs:G16}, <v,J^Tw>={rhs:G16}, rel={rel:G6}.");
        }
    }

    // ===== 2a. Analytic joint gradient vs full central-FD gradient of S_B =====

    [Fact]
    public void JointGradient_MatchesCentralFdGradient()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = Manifest();
        var geometry = Geometry();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        int nOmega = mesh.EdgeCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;

        int seed = 400;
        foreach (var member in new[] { Sd2Member(), Asd2Member(), Sd2Member(VertexFaceRule.IncidentAverage) })
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, member);
            var shiabEval = EinsteinianShiabBatteries.EinsteinianThetaEval(op, mesh, algebra, manifest, geometry);
            var omega = RandomVector(nOmega, seed++, 0.2);
            var theta = RandomVector(nTheta, seed++, 0.15);

            var (objective, gradOmega, gradTheta) = op.ComputeJointGradient(omega, theta, mass);

            double Obj(double[] w, double[] th) => EinsteinianShiabBatteries.JointObjective(
                shiabEval, torsion, mass, mesh, algebra, w, th, manifest, geometry);

            // Objective consistency with the battery-computed joint objective.
            double objRef = Obj(omega, theta);
            Assert.True(System.Math.Abs(objective - objRef) < 1e-12 * (1 + System.Math.Abs(objRef)),
                $"Objective mismatch for {member.BranchId}: analytic={objective:G16}, battery={objRef:G16}.");

            // Full-coordinate central-FD gradient.
            const double h = 1e-5;
            double scaleO = MaxAbs(gradOmega) + 1e-300;
            for (int i = 0; i < nOmega; i++)
            {
                var p = (double[])omega.Clone();
                var m = (double[])omega.Clone();
                p[i] += h;
                m[i] -= h;
                double fd = (Obj(p, theta) - Obj(m, theta)) / (2 * h);
                double rel = System.Math.Abs(gradOmega[i] - fd) / scaleO;
                Assert.True(rel < 1e-6,
                    $"grad_omega[{i}] mismatch for {member.BranchId}: analytic={gradOmega[i]:G12}, fd={fd:G12}, rel={rel:G6}.");
            }

            double scaleT = MaxAbs(gradTheta) + 1e-300;
            for (int i = 0; i < nTheta; i++)
            {
                var p = (double[])theta.Clone();
                var m = (double[])theta.Clone();
                p[i] += h;
                m[i] -= h;
                double fd = (Obj(omega, p) - Obj(omega, m)) / (2 * h);
                double rel = System.Math.Abs(gradTheta[i] - fd) / scaleT;
                Assert.True(rel < 1e-6,
                    $"grad_theta[{i}] mismatch for {member.BranchId}: analytic={gradTheta[i]:G12}, fd={fd:G12}, rel={rel:G6}.");
            }
        }
    }

    // ===== 2b. At theta=0 the omega-gradient equals the legacy column-loop gradient =====

    [Fact]
    public void JointGradient_ThetaZero_MatchesLegacyColumnLoopGradient()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = Manifest();
        var geometry = Geometry();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        int nOmega = mesh.EdgeCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;

        // Trivial mode so the legacy Linearize is the exact analytic Jacobian; at theta=0 the
        // mode-2 dressing is Ad = I, so both paths compute the same gradient.
        var member = new EinsteinianShiabFamilyMember
        {
            Phi1 = InvariantElementSpec.Sd2,
            Phi2 = InvariantElementSpec.Id0,
            EinsteinCoefficient = 0.5,
            EpsilonMode = "trivial",
        };
        var op = new EinsteinianShiabOperator(mesh, algebra, member);

        var omega = RandomVector(nOmega, 500, 0.2);
        var conn = new ConnectionField(mesh, algebra, omega);
        var omegaT = conn.ToFieldTensor();
        var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var s = op.Evaluate(f, omegaT, manifest, geometry);
        var t = torsion.Evaluate(omegaT, a0, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(s, t);

        var legacy = new CpuLocalJacobian(op, torsion, omegaT, a0, f, manifest, geometry, mesh, algebra);
        var legacyGrad = legacy.ComputeGradient(upsilon, mass).Coefficients;

        var opTheta = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());
        var (_, gradOmega, gradTheta) = opTheta.ComputeJointGradient(omega, new double[nTheta], mass);

        double scale = MaxAbs(legacyGrad) + 1e-300;
        for (int i = 0; i < nOmega; i++)
        {
            double rel = System.Math.Abs(gradOmega[i] - legacyGrad[i]) / scale;
            Assert.True(rel < 1e-10,
                $"theta=0 omega-gradient must match the legacy column-loop gradient at [{i}]: " +
                $"new={gradOmega[i]:G12}, legacy={legacyGrad[i]:G12}, rel={rel:G6}.");
        }

        // Sanity: the theta-gradient exists and is finite (nonzero in general: dAd(dtheta) pairs
        // the curvature with M Upsilon even at theta=0).
        Assert.Equal(nTheta, gradTheta.Length);
        Assert.True(gradTheta.All(double.IsFinite), "theta-gradient must be finite.");
    }

    // ===== 3. Hessian-vector product vs dense-FD joint Hessian columns =====

    [Fact]
    public void HessianVectorProduct_MatchesDenseFdHessianColumns()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = Manifest();
        var geometry = Geometry();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        int nOmega = mesh.EdgeCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;
        int nJoint = nOmega + nTheta;

        var op = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());
        var shiabEval = EinsteinianShiabBatteries.EinsteinianThetaEval(op, mesh, algebra, manifest, geometry);
        var omega = RandomVector(nOmega, 600, 0.2);
        var theta = RandomVector(nTheta, 601, 0.15);

        double Obj(double[] x)
        {
            var w = new double[nOmega];
            var th = new double[nTheta];
            Array.Copy(x, 0, w, 0, nOmega);
            Array.Copy(x, nOmega, th, 0, nTheta);
            return EinsteinianShiabBatteries.JointObjective(
                shiabEval, torsion, mass, mesh, algebra, w, th, manifest, geometry);
        }

        var x0 = new double[nJoint];
        Array.Copy(omega, 0, x0, 0, nOmega);
        Array.Copy(theta, 0, x0, nOmega, nTheta);

        // One omega column and one theta column of the dense-FD joint Hessian.
        const double h = 1e-3;
        foreach (int k in new[] { 3, nOmega + 5 })
        {
            // Analytic-gradient Hv with v = e_k.
            var vOmega = new double[nOmega];
            var vTheta = new double[nTheta];
            if (k < nOmega) vOmega[k] = 1.0;
            else vTheta[k - nOmega] = 1.0;
            var (hvOmega, hvTheta) = op.JointHessianVectorProduct(omega, theta, vOmega, vTheta, mass);
            var hv = new double[nJoint];
            Array.Copy(hvOmega, 0, hv, 0, nOmega);
            Array.Copy(hvTheta, 0, hv, nOmega, nTheta);

            // Dense-FD column k via pure-objective mixed second differences.
            var column = new double[nJoint];
            for (int j = 0; j < nJoint; j++)
            {
                var xpp = (double[])x0.Clone();
                var xpm = (double[])x0.Clone();
                var xmp = (double[])x0.Clone();
                var xmm = (double[])x0.Clone();
                xpp[j] += h; xpp[k] += h;
                xpm[j] += h; xpm[k] -= h;
                xmp[j] -= h; xmp[k] += h;
                xmm[j] -= h; xmm[k] -= h;
                column[j] = (Obj(xpp) - Obj(xpm) - Obj(xmp) + Obj(xmm)) / (4.0 * h * h);
            }

            double scale = MaxAbs(column) + 1e-300;
            for (int j = 0; j < nJoint; j++)
            {
                double rel = System.Math.Abs(hv[j] - column[j]) / scale;
                Assert.True(rel < 1e-5,
                    $"Hv mismatch at H[{j},{k}]: analytic-FD Hv={hv[j]:G12}, dense-FD={column[j]:G12}, rel={rel:G6}.");
            }
        }
    }

    // ===== 4. Timing measurement (informational — NOT a hard gate) =====

    [Fact]
    public void Timing_JointGradientAndHv_AreMeasuredAndReported()
    {
        var mesh = Mesh4D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = Manifest();
        var geometry = Geometry();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        int nOmega = mesh.EdgeCount * algebra.Dimension;
        int nTheta = mesh.VertexCount * algebra.Dimension;

        var op = new EinsteinianShiabOperator(mesh, algebra, Sd2Member());
        var omega = RandomVector(nOmega, 700, 0.2);
        var theta = RandomVector(nTheta, 701, 0.15);
        var vOmega = RandomVector(nOmega, 702);
        var vTheta = RandomVector(nTheta, 703);

        // Warm-up (JIT).
        op.ComputeJointGradient(omega, theta, mass);
        op.JointHessianVectorProduct(omega, theta, vOmega, vTheta, mass);

        const int gradReps = 20;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < gradReps; i++)
            op.ComputeJointGradient(omega, theta, mass);
        sw.Stop();
        double gradMs = sw.Elapsed.TotalMilliseconds / gradReps;

        const int hvReps = 10;
        sw.Restart();
        for (int i = 0; i < hvReps; i++)
            op.JointHessianVectorProduct(omega, theta, vOmega, vTheta, mass);
        sw.Stop();
        double hvMs = sw.Elapsed.TotalMilliseconds / hvReps;

        // Legacy column-by-column J^T (M Upsilon) on the same mesh for context (the path that
        // measured ~60 s/Hv on the study mesh).
        var trivialMember = new EinsteinianShiabFamilyMember
        {
            Phi1 = InvariantElementSpec.Sd2,
            Phi2 = InvariantElementSpec.Id0,
            EinsteinCoefficient = 0.5,
            EpsilonMode = "trivial",
        };
        var legacyOp = new EinsteinianShiabOperator(mesh, algebra, trivialMember);
        var conn = new ConnectionField(mesh, algebra, omega);
        var omegaT = conn.ToFieldTensor();
        var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var s = legacyOp.Evaluate(f, omegaT, manifest, geometry);
        var t = torsion.Evaluate(omegaT, a0, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(s, t);
        var legacy = new CpuLocalJacobian(legacyOp, torsion, omegaT, a0, f, manifest, geometry, mesh, algebra);

        legacy.ComputeGradient(upsilon, mass); // warm-up
        sw.Restart();
        legacy.ComputeGradient(upsilon, mass);
        sw.Stop();
        double legacyMs = sw.Elapsed.TotalMilliseconds;

        _output.WriteLine($"Mesh CreateUniform4D(1): edges={mesh.EdgeCount}, faces={mesh.FaceCount}, " +
            $"vertices={mesh.VertexCount}, cells={mesh.CellCount}; nOmega={nOmega}, nTheta={nTheta}.");
        _output.WriteLine($"Analytic joint gradient (reverse pass): {gradMs:F3} ms/eval ({gradReps} reps).");
        _output.WriteLine($"Hessian-vector product (2 gradient evals): {hvMs:F3} ms/eval ({hvReps} reps).");
        _output.WriteLine($"Legacy column-loop gradient (CpuLocalJacobian.ComputeGradient, omega-block only, " +
            $"{nOmega} columns): {legacyMs:F3} ms/eval on this mesh (~60 s/Hv measured on the study mesh).");

        Assert.True(gradMs > 0 && hvMs > 0, "Timing must be measurable.");
    }
}
