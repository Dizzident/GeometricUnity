using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// The M3 acceptance batteries, provided as reusable numerical primitives so tests,
/// qa-4d, and the first physics study (the 4D control-vs-Einsteinian Hessian-degree
/// probe) all compute them the same way. The §3.7 / physics-decisions §4.5 five:
/// <list type="number">
///   <item>Richness certificate — <see cref="RichnessDeviation"/>: the off-scalar
///         Frobenius norm of the member's 6x6 Lambda^2 endomorphism R. A genuinely
///         rich member exceeds the floor; the scalar control fails it (returns ~0).</item>
///   <item>Weyl annihilation — <see cref="WeylAnnihilationResidual"/>: feed a genuine
///         constant 2-form living in the subspace R annihilates, assert S_h -> 0.</item>
///   <item>Carrier-signature identity — <see cref="CarrierMatches"/>.</item>
///   <item>Linearization / Hessian symmetry — <see cref="HessianSymmetryResidual"/>.</item>
///   <item>Gauge covariance under the eps-dressing — <see cref="GaugeCovarianceResidual"/>.</item>
/// </list>
/// Plus two NON-GATING kappa-scan batteries that move with the slaved-Wilson SMOKE-TEST
/// (physics memo §6e resolved 2026-07-02: the pinned omega-coupled treatment is independent-theta
/// with a joint (omega,theta) Hessian; the Wilson eps(omega) realization is a labeled non-gating
/// smoke-test, NOT the pinned physics). These validate the smoke-test wiring only:
/// <list type="number">
///   <item>kappa=0 degree-2 control — <see cref="HessianThirdTDifference"/> at kappa=0
///         must vanish (reproduces the Phase436 degree-2 Hessian exactly).</item>
///   <item>Monotone degree lift — <see cref="KappaScanThirdTDifference"/>: the third
///         t-difference of the Hessian must grow monotonically with kappa, separating a
///         genuine degree-lift from a numerical artifact.</item>
/// </list>
/// The pinned independent-theta arm (theta a genuine independent field, joint (omega,theta)
/// Hessian) is implemented only when the physicist-RATIFIED design §3.5 lands (possibly M3b),
/// with THREE gating controls:
///   (a) LinearizeTheta matches finite difference;
///   (b) theta=0 reproduces the Phase436 degree-2 Hessian;
///   (c) ISOLATION battery — with the IDENTITY Shiab, theta is absent from Upsilon, so the
///       theta-block of the joint Hessian must be EXACTLY degenerate. This proves any degree-lift
///       comes from the Shiab's epsilon-dependence, not from merely inserting the theta DOF.
///
/// Framing (physicist): the third t-difference reports the Hessian degree structure of
/// THIS operator as-is. A degree-2 result for the Einsteinian family is a legitimate
/// frontier-sharpening outcome, not a failure — these batteries measure the mechanism,
/// they do not presuppose a scale, and nothing here is promoted.
/// </summary>
public static class EinsteinianShiabBatteries
{
    // ---- Battery 1: richness certificate ----

    /// <summary>Off-scalar deviation of the member's Lambda^2 endomorphism (0 iff scalar·I).</summary>
    public static double RichnessDeviation(EinsteinianShiabOperator op)
    {
        ArgumentNullException.ThrowIfNull(op);
        return Lambda2Algebra.ScalarDeviation(op.Lambda2Endomorphism);
    }

    /// <summary>A member is "genuinely rich" (not a scalar multiple of F on Lambda^2) iff
    /// its endomorphism deviation exceeds <paramref name="floor"/>.</summary>
    public static bool IsRich(EinsteinianShiabOperator op, double floor = 1e-9) =>
        RichnessDeviation(op) > floor;

    // ---- Battery 2: Weyl annihilation ----

    /// <summary>
    /// Build a genuine, spatially-constant ad-valued 2-form from a Lambda^2 coefficient
    /// vector <paramref name="xiLambda2"/> (length 6), placed in a single ad-component
    /// <paramref name="adComponent"/>. Face f gets coefficient &lt;xi, B_face&gt;, the pairing
    /// of xi with the face bivector — i.e. the exact face-integral of the constant 2-form.
    /// This lands in the per-cell Lambda^2 image, so it is reconstructed exactly by the
    /// operator's lift and is the correct probe for Weyl annihilation.
    /// </summary>
    public static double[] ConstantTwoForm(
        SimplicialMesh mesh, LieAlgebra algebra, double[] xiLambda2, int adComponent)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(algebra);
        if (xiLambda2.Length != Lambda2Algebra.Dim)
            throw new ArgumentException($"xiLambda2 must have length {Lambda2Algebra.Dim}.", nameof(xiLambda2));

        int dimG = algebra.Dimension;
        var coeffs = new double[mesh.FaceCount * dimG];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            var verts = mesh.Faces[f];
            var pa = mesh.GetVertexCoordinates(verts[0]);
            var pb = mesh.GetVertexCoordinates(verts[1]);
            var pc = mesh.GetVertexCoordinates(verts[2]);
            var u = new double[4];
            var v = new double[4];
            for (int d = 0; d < 4; d++) { u[d] = pb[d] - pa[d]; v[d] = pc[d] - pa[d]; }
            var biv = Lambda2Algebra.Wedge(u, v);

            double pairing = 0;
            for (int k = 0; k < Lambda2Algebra.Dim; k++) pairing += xiLambda2[k] * biv[k];
            coeffs[f * dimG + adComponent] = pairing;
        }
        return coeffs;
    }

    /// <summary>
    /// Weyl-annihilation residual: feed the constant 2-form built from
    /// <paramref name="xiLambda2"/> (which should lie in the subspace the member's R
    /// annihilates, e.g. an anti-self-dual bivector for a self-dual member) and return
    /// max|S_h|. Near zero means the member annihilates that sector.
    /// </summary>
    public static double WeylAnnihilationResidual(
        EinsteinianShiabOperator op,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] xiLambda2,
        BranchManifest manifest,
        GeometryContext geometry,
        int adComponent = 0)
    {
        ArgumentNullException.ThrowIfNull(op);
        var fCoeffs = ConstantTwoForm(mesh, algebra, xiLambda2, adComponent);
        var f = FaceTensor(mesh, algebra, fCoeffs, "F_weyl");
        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var s = op.Evaluate(f, omega, manifest, geometry);
        return MaxAbs(s.Coefficients);
    }

    /// <summary>An anti-self-dual Lambda^2 direction e01 - e23 (in Lambda^2_-).</summary>
    public static double[] AntiSelfDualSample() => new double[] { 1, 0, 0, 0, 0, -1 };

    /// <summary>A self-dual Lambda^2 direction e01 + e23 (in Lambda^2_+).</summary>
    public static double[] SelfDualSample() => new double[] { 1, 0, 0, 0, 0, 1 };

    // ---- Battery 3: carrier-signature identity ----

    /// <summary>True iff the torsion and Shiab output signatures are strictly identical.</summary>
    public static bool CarrierMatches(ITorsionBranchOperator torsion, IShiabBranchOperator shiab)
    {
        try
        {
            BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // ---- Battery 4: Hessian self-adjointness ----

    /// <summary>
    /// Relative asymmetry of the Hessian of S_B = (1/2)||Upsilon||^2_M at
    /// <paramref name="omega"/>, sampled on a random <paramref name="subspaceDim"/>-dimensional
    /// subspace of edge DOFs via second-order central finite differences. Returns
    /// max|H_ij - H_ji| / (||H||_F + eps). Small confirms self-adjointness (IX.32.2.1).
    /// Uses TrivialTorsionCpu so Upsilon = S_h, and the positive-definite trace pairing.
    /// </summary>
    public static double HessianSymmetryResidual(
        EinsteinianShiabOperator op,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        BranchManifest manifest,
        GeometryContext geometry,
        int subspaceDim = 6,
        double h = 1e-4,
        int seed = 20260702)
    {
        ArgumentNullException.ThrowIfNull(op);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var mass = new CpuMassMatrix(mesh, algebra);
        int n = omega.Length;
        int k = System.Math.Min(subspaceDim, n);

        // Random orthonormal-ish directions (unit-normalized, independence is enough for FD).
        var rng = new Random(seed);
        var dirs = new double[k][];
        for (int i = 0; i < k; i++)
        {
            var d = new double[n];
            double norm = 0;
            for (int j = 0; j < n; j++) { d[j] = rng.NextDouble() - 0.5; norm += d[j] * d[j]; }
            norm = System.Math.Sqrt(norm);
            for (int j = 0; j < n; j++) d[j] /= norm;
            dirs[i] = d;
        }

        double Obj(double[] w) => Objective(op, torsion, mass, mesh, algebra, w, manifest, geometry);

        var hess = new double[k, k];
        for (int i = 0; i < k; i++)
            for (int j = 0; j < k; j++)
            {
                double fpp = Obj(Combine(omega, dirs[i], +h, dirs[j], +h));
                double fpm = Obj(Combine(omega, dirs[i], +h, dirs[j], -h));
                double fmp = Obj(Combine(omega, dirs[i], -h, dirs[j], +h));
                double fmm = Obj(Combine(omega, dirs[i], -h, dirs[j], -h));
                hess[i, j] = (fpp - fpm - fmp + fmm) / (4.0 * h * h);
            }

        double maxAsym = 0, frob = 0;
        for (int i = 0; i < k; i++)
            for (int j = 0; j < k; j++)
            {
                frob += hess[i, j] * hess[i, j];
                double asym = System.Math.Abs(hess[i, j] - hess[j, i]);
                if (asym > maxAsym) maxAsym = asym;
            }
        frob = System.Math.Sqrt(frob);
        return maxAsym / (frob + 1e-300);
    }

    /// <summary>Objective S_B = (1/2) Upsilon^T M Upsilon with Upsilon = S_h - T.</summary>
    public static double Objective(
        EinsteinianShiabOperator op,
        ITorsionBranchOperator torsion,
        CpuMassMatrix mass,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        var omegaT = conn.ToFieldTensor();
        var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var s = op.Evaluate(f, omegaT, manifest, geometry);
        var t = torsion.Evaluate(omegaT, a0, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(s, t);
        return 0.5 * mass.InnerProduct(upsilon, upsilon);
    }

    // ---- omega-coupled degree batteries (physicist conditional sign-off) ----

    /// <summary>
    /// The third finite t-difference of the Hessian of S_B along the ray omega = t·u,
    /// probed as h(t) = v^T H(t·u) v (a second directional derivative in a fixed direction
    /// <paramref name="v"/> at the background t·u), returned as
    ///   D3 = h(t0+3dt) - 3 h(t0+2dt) + 3 h(t0+dt) - h(t0).
    /// This mirrors Phase436's exact-Hessian degree probe: for an S_h that is linear in
    /// omega the Hessian H(t·u) is degree-2 in t, so D3 vanishes. A nonzero D3 signals a
    /// Hessian of degree &gt; 2 (the eps(omega)-conjugation degree-lift). Uses
    /// TrivialTorsionCpu (Upsilon = S_h) and the positive-definite trace pairing.
    /// Reports what the operator IS; it does not presuppose either verdict.
    /// </summary>
    public static double HessianThirdTDifference(
        EinsteinianShiabOperator op,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] u,
        double[] v,
        BranchManifest manifest,
        GeometryContext geometry,
        double t0 = 0.5,
        double dt = 0.3,
        double ds = 0.05)
    {
        ArgumentNullException.ThrowIfNull(op);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var mass = new CpuMassMatrix(mesh, algebra);

        // h(t) = [S_B(t·u + ds·v) - 2 S_B(t·u) + S_B(t·u - ds·v)] / ds^2  ≈ v^T H(t·u) v.
        double H(double t)
        {
            var baseW = Scale(u, t);
            double c = Objective(op, torsion, mass, mesh, algebra, baseW, manifest, geometry);
            double p = Objective(op, torsion, mass, mesh, algebra, Axpy(baseW, v, +ds), manifest, geometry);
            double m = Objective(op, torsion, mass, mesh, algebra, Axpy(baseW, v, -ds), manifest, geometry);
            return (p - 2 * c + m) / (ds * ds);
        }

        return H(t0 + 3 * dt) - 3 * H(t0 + 2 * dt) + 3 * H(t0 + dt) - H(t0);
    }

    /// <summary>
    /// Scan the omega-coupling kappa and return |third t-difference of the Hessian| for
    /// each kappa (one operator built per kappa from <paramref name="baseMember"/>, which
    /// must have EpsilonMode="omega-coupled"). kappa=0 should give ~0 (degree-2 control);
    /// the sequence should grow monotonically, separating a genuine degree-lift from a
    /// numerical artifact (physicist conditional sign-off battery).
    /// </summary>
    public static double[] KappaScanThirdTDifference(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        EinsteinianShiabFamilyMember baseMember,
        double[] kappas,
        double[] u,
        double[] v,
        BranchManifest manifest,
        GeometryContext geometry,
        double t0 = 0.5,
        double dt = 0.3,
        double ds = 0.05)
    {
        var result = new double[kappas.Length];
        for (int i = 0; i < kappas.Length; i++)
        {
            var op = new EinsteinianShiabOperator(mesh, algebra, baseMember, null, kappas[i]);
            result[i] = System.Math.Abs(
                HessianThirdTDifference(op, mesh, algebra, u, v, manifest, geometry, t0, dt, ds));
        }
        return result;
    }

    private static double[] Scale(double[] a, double s)
    {
        var r = new double[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
        return r;
    }

    private static double[] Axpy(double[] baseW, double[] d, double s)
    {
        var r = new double[baseW.Length];
        for (int i = 0; i < r.Length; i++) r[i] = baseW[i] + s * d[i];
        return r;
    }

    // ---- mode (2) independent-theta GATING batteries (joint (omega,theta) Hessian) ----
    //
    // theta is a genuine independent H-valued fluctuation field (length VertexCount*dim(g)).
    // The joint Hessian of S_B over the enlarged vector (omega, theta) is probed with the same
    // Phase436 finite-difference machinery. shiabEval : (omega, theta) -> S_h coefficients lets
    // the harness treat any Shiab uniformly (the identity Shiab simply ignores theta).

    /// <summary>Joint objective S_B(omega, theta) = (1/2)||S_h(omega,theta) - T(omega)||^2_M.</summary>
    public static double JointObjective(
        Func<double[], double[], double[]> shiabEval,
        ITorsionBranchOperator torsion,
        CpuMassMatrix mass,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        double[] theta,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        var omegaT = conn.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var sCoeffs = shiabEval(omega, theta);
        var s = FaceTensor(mesh, algebra, sCoeffs, "S_h");
        var t = torsion.Evaluate(omegaT, a0, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(s, t);
        return 0.5 * mass.InnerProduct(upsilon, upsilon);
    }

    /// <summary>
    /// A shiabEval that runs the Einsteinian operator at an explicit theta (mode 2): recomputes
    /// the curvature from omega and calls <see cref="EinsteinianShiabOperator.EvaluateWithTheta"/>.
    /// </summary>
    public static Func<double[], double[], double[]> EinsteinianThetaEval(
        EinsteinianShiabOperator op, SimplicialMesh mesh, LieAlgebra algebra,
        BranchManifest manifest, GeometryContext geometry)
        => (omega, theta) =>
        {
            var conn = new ConnectionField(mesh, algebra, omega);
            var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
            return op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry).Coefficients;
        };

    /// <summary>
    /// A shiabEval for the IDENTITY Shiab (S = F): theta-blind, used for the isolation battery.
    /// </summary>
    public static Func<double[], double[], double[]> IdentityThetaEval(
        SimplicialMesh mesh, LieAlgebra algebra, BranchManifest manifest, GeometryContext geometry)
    {
        var identity = new IdentityShiabCpu(mesh, algebra);
        return (omega, theta) =>
        {
            var conn = new ConnectionField(mesh, algebra, omega);
            var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
            return identity.Evaluate(f, conn.ToFieldTensor(), manifest, geometry).Coefficients;
        };
    }

    /// <summary>
    /// Frobenius norm of the theta-block of the joint (omega,theta) Hessian of S_B at
    /// (<paramref name="omega"/>, <paramref name="theta"/>), sampled on a random
    /// <paramref name="subspaceDim"/>-dimensional subspace of theta DOFs via second-order
    /// central finite differences (omega held fixed). ISOLATION battery (control iii): with the
    /// identity Shiab this is ~0 (theta absent from Upsilon => the theta-block is exactly
    /// degenerate), proving any degree-lift is Shiab-caused, not a free-DOF artifact.
    /// </summary>
    public static double ThetaBlockFrobenius(
        Func<double[], double[], double[]> shiabEval,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        double[] theta,
        BranchManifest manifest,
        GeometryContext geometry,
        int subspaceDim = 6,
        double h = 1e-4,
        int seed = 20260702)
    {
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var mass = new CpuMassMatrix(mesh, algebra);
        int nTheta = theta.Length;
        int k = System.Math.Min(subspaceDim, nTheta);

        var rng = new Random(seed);
        var dirs = new double[k][];
        for (int i = 0; i < k; i++)
        {
            var d = new double[nTheta];
            double norm = 0;
            for (int j = 0; j < nTheta; j++) { d[j] = rng.NextDouble() - 0.5; norm += d[j] * d[j]; }
            norm = System.Math.Sqrt(norm);
            for (int j = 0; j < nTheta; j++) d[j] /= norm;
            dirs[i] = d;
        }

        double Obj(double[] th) => JointObjective(shiabEval, torsion, mass, mesh, algebra, omega, th, manifest, geometry);

        double frob = 0;
        for (int i = 0; i < k; i++)
            for (int j = 0; j < k; j++)
            {
                double fpp = Obj(Axpy2(theta, dirs[i], +h, dirs[j], +h));
                double fpm = Obj(Axpy2(theta, dirs[i], +h, dirs[j], -h));
                double fmp = Obj(Axpy2(theta, dirs[i], -h, dirs[j], +h));
                double fmm = Obj(Axpy2(theta, dirs[i], -h, dirs[j], -h));
                double hij = (fpp - fpm - fmp + fmm) / (4.0 * h * h);
                frob += hij * hij;
            }
        return System.Math.Sqrt(frob);
    }

    /// <summary>
    /// Residual between the analytic <see cref="EinsteinianShiabOperator.LinearizeTheta"/> and a
    /// central finite difference of <see cref="EinsteinianShiabOperator.EvaluateWithTheta"/> in the
    /// theta direction. GATING control (i) for mode 2: analytic LinearizeTheta must match FD.
    /// </summary>
    public static double LinearizeThetaFdResidual(
        EinsteinianShiabOperator op,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        double[] theta,
        double[] deltaTheta,
        BranchManifest manifest,
        GeometryContext geometry,
        double h = 1e-7)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        var omegaT = conn.ToFieldTensor();
        var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();

        var analytic = op.LinearizeTheta(f, omegaT, theta, deltaTheta, manifest, geometry).Coefficients;

        var plus = new double[theta.Length];
        var minus = new double[theta.Length];
        for (int i = 0; i < theta.Length; i++)
        {
            plus[i] = theta[i] + h * deltaTheta[i];
            minus[i] = theta[i] - h * deltaTheta[i];
        }
        var sP = op.EvaluateWithTheta(f, omegaT, plus, manifest, geometry).Coefficients;
        var sM = op.EvaluateWithTheta(f, omegaT, minus, manifest, geometry).Coefficients;

        double max = 0;
        for (int i = 0; i < analytic.Length; i++)
        {
            double fd = (sP[i] - sM[i]) / (2 * h);
            double diff = System.Math.Abs(fd - analytic[i]);
            if (diff > max) max = diff;
        }
        return max;
    }

    private static double[] Axpy2(double[] baseV, double[] d1, double s1, double[] d2, double s2)
    {
        var r = new double[baseV.Length];
        for (int i = 0; i < r.Length; i++) r[i] = baseV[i] + s1 * d1[i] + s2 * d2[i];
        return r;
    }

    // ---- Battery 5: gauge covariance ----

    /// <summary>
    /// First-order gauge covariance residual: max|S_h(Ad_g F) - Ad_g S_h(F)| for a global
    /// gauge parameter <paramref name="gaugeParam"/>, with Ad_g = exp(ad_{gaugeParam}) applied
    /// to the ad-index of every face. For the trivial/frozen (constant-eps) modes the
    /// Lambda^2 contraction commutes with the ad-index action, so this is exact (~0).
    /// </summary>
    public static double GaugeCovarianceResidual(
        EinsteinianShiabOperator op,
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] curvatureCoeffs,
        double[] gaugeParam,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(op);
        int dimG = algebra.Dimension;
        var adG = AdExp(algebra, gaugeParam);

        // Ad_g applied to each face of F.
        var fRotated = ApplyAdPerFace(curvatureCoeffs, adG, mesh.FaceCount, dimG);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var lhs = op.Evaluate(FaceTensor(mesh, algebra, fRotated, "AdF"), omega, manifest, geometry);
        var sBase = op.Evaluate(FaceTensor(mesh, algebra, curvatureCoeffs, "F"), omega, manifest, geometry);
        var rhs = ApplyAdPerFace(sBase.Coefficients, adG, mesh.FaceCount, dimG);

        double max = 0;
        for (int i = 0; i < lhs.Coefficients.Length; i++)
        {
            double diff = System.Math.Abs(lhs.Coefficients[i] - rhs[i]);
            if (diff > max) max = diff;
        }
        return max;
    }

    // ---- shared helpers ----

    /// <summary>Maximum absolute coefficient.</summary>
    public static double MaxAbs(double[] a)
    {
        double max = 0;
        foreach (double v in a)
        {
            double av = System.Math.Abs(v);
            if (av > max) max = av;
        }
        return max;
    }

    private static double[] ApplyAdPerFace(double[] coeffs, double[,] ad, int faceCount, int dimG)
    {
        var result = new double[coeffs.Length];
        for (int f = 0; f < faceCount; f++)
            for (int a = 0; a < dimG; a++)
            {
                double s = 0;
                for (int b = 0; b < dimG; b++) s += ad[a, b] * coeffs[f * dimG + b];
                result[f * dimG + a] = s;
            }
        return result;
    }

    private static double[,] AdExp(LieAlgebra algebra, double[] theta)
    {
        int dimG = algebra.Dimension;
        var adMat = new double[dimG, dimG];
        for (int c = 0; c < dimG; c++)
            for (int b = 0; b < dimG; b++)
            {
                double s = 0;
                for (int a = 0; a < dimG; a++) s += algebra.GetStructureConstant(a, b, c) * theta[a];
                adMat[c, b] = s;
            }
        return Lambda2Algebra.MatrixExp(adMat);
    }

    private static double[] Combine(double[] baseW, double[] d1, double s1, double[] d2, double s2)
    {
        var w = new double[baseW.Length];
        for (int i = 0; i < w.Length; i++) w[i] = baseW[i] + s1 * d1[i] + s2 * d2[i];
        return w;
    }

    private static FieldTensor FaceTensor(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs, string label) =>
        new()
        {
            Label = label,
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
