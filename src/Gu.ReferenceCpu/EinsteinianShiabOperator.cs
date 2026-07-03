using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// The Einsteinian Shiab candidate family (design §3, physics-decisions §4). A
/// degree-preserving Omega^2 -> Omega^2 map that realizes the eq. 9.3 two-term
/// Einsteinian contraction on the 6-dimensional Lambda^2(T*X^4) of a Euclidean
/// (Cl(4,0)) 4D base. Its <see cref="OutputSignature"/> is byte-identical to
/// <see cref="IdentityShiabCpu"/> so it drops into the existing residual pipeline
/// (Upsilon = S - T) with no consumer change.
///
/// Per-cell contraction (design §3.3, reduced to the projector language confirmed
/// in physics-decisions §6e):
/// <list type="number">
///   <item>For each pentachoron, assemble the 6x(#faces) matrix W whose columns
///         are the face bivectors B_face = (p_b - p_a) ∧ (p_c - p_a) in Lambda^2(R^4).</item>
///   <item>The per-cell face map is M = I + W^T (R - I) Q, with Q = (W W^T)^{-1} W
///         the least-squares lift faces -> Lambda^2 and R the member's Lambda^2
///         endomorphism (<see cref="Lambda2Algebra.MemberEndomorphism"/>). This
///         applies R to the genuine 2-form (Lambda^2) content of the cell's faces and
///         is the identity on the orthogonal complement (the non-representable face
///         DOF), so R = identity (Phi1=id0, Phi2=none) reproduces identity-Shiab EXACTLY.</item>
///   <item>The gauge dressing Ad_eps = exp(ad_theta) acts on the ad (Lie-algebra)
///         index: S_h(F) = M( Ad_eps(F) ). See <see cref="EpsilonMode"/>.</item>
///   <item>Cell contributions are scattered to global faces and averaged over the
///         cells sharing each face.</item>
/// </list>
///
/// Recorded boundary (mandatory study language, design §3.2 point 4 / §6e):
/// shiabOutputDegree = 2; the draft operator is genuinely degree-raising
/// (Omega^2 -> Omega^{d-1} with Phi_1 a 1-form) but the degree-raising Hodge/wedge
/// legs are linear in F and carry no omega-dependence, so they are irrelevant to
/// the Hessian-degree question and are dropped in this first realization
/// (draftDegreeReductionRecorded = true). This slice captures the Ricci/Weyl
/// algebra, not the literal form degree.
///
/// Bracket-type note: only "commutator" is numerically realized in this real-valued
/// Euclidean reduced slice. "i-anticommutator" carries an i^{(1±1)/2} prefactor
/// (physics-decisions §4.3) that needs the complex extension; it is accepted as a
/// recorded parameter (it changes <see cref="BranchId"/>) but does not alter the
/// real-valued numerics here — a recorded, named gap.
/// </summary>
public sealed class EinsteinianShiabOperator : IShiabBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly EinsteinianShiabFamilyMember _member;
    private readonly int _dimG;
    private readonly int _faceCount;
    private readonly double _kappa;

    // Per-cell precomputed data (mesh- and member-fixed, omega-independent).
    private readonly int[][] _cellFaces;          // global face indices per cell
    private readonly double[][,] _cellFaceMap;    // M - I = W^T (R-I) Q, per cell (nFaces x nFaces)
    private readonly double[] _faceInvCount;      // 1 / (#cells sharing a face)

    // Epsilon dressing.
    private readonly double[][,]? _adPerCell;     // frozen-background Ad_eps per cell (null for trivial)

    /// <summary>
    /// Build an Einsteinian Shiab operator for one family member.
    /// </summary>
    /// <param name="mesh">4D base mesh (EmbeddingDimension &gt;= 4, populated CellFaces).</param>
    /// <param name="algebra">Gauge Lie algebra (ad-bundle).</param>
    /// <param name="member">The family-member spec.</param>
    /// <param name="backgroundEps">
    /// Frozen-background theta coefficients (only for EpsilonMode="frozen-background"):
    /// either length dim(g) (a single global theta) or CellCount*dim(g) (per-cell theta).
    /// The conjugator is Ad = exp(ad_theta). Ignored for other modes.</param>
    /// <param name="omegaCouplingKappa">
    /// The omega-coupling strength kappa for EpsilonMode="slaved-wilson-smoketest": the per-cell
    /// theta is theta_cell = kappa * sum_{e in cell} omega_e. kappa = 0 recovers eps = 1.
    /// Discrete eps map per design §3.5 / physics §6e — physicist sign-off pending.</param>
    public EinsteinianShiabOperator(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        EinsteinianShiabFamilyMember member,
        double[]? backgroundEps = null,
        double omegaCouplingKappa = 1.0)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _member = member ?? throw new ArgumentNullException(nameof(member));
        _dimG = algebra.Dimension;
        _faceCount = mesh.FaceCount;
        _kappa = omegaCouplingKappa;

        if (mesh.EmbeddingDimension < 4)
            throw new ArgumentException(
                $"Einsteinian Shiab requires a 4D base (EmbeddingDimension >= 4); got {mesh.EmbeddingDimension}. " +
                "The Lambda^2(T*X^4) contraction is undefined below dim 4 — this is the documented 2D blocker.",
                nameof(mesh));

        if (member.EpsilonMode is not ("trivial" or "frozen-background" or "slaved-wilson-smoketest" or "independent-theta"))
            throw new ArgumentException(
                $"Unknown EpsilonMode '{member.EpsilonMode}' " +
                "(expected trivial|frozen-background|slaved-wilson-smoketest|independent-theta).",
                nameof(member));

        var r = Lambda2Algebra.MemberEndomorphism(member);
        var rMinusI = Lambda2Algebra.ScaleAdd(r, 1.0, Lambda2Algebra.Identity(Lambda2Algebra.Dim), -1.0);

        (_cellFaces, _cellFaceMap, _faceInvCount) = PrecomputeCellMaps(mesh, rMinusI);

        _adPerCell = member.EpsilonMode == "frozen-background"
            ? BuildFrozenAd(backgroundEps)
            : null;
    }

    /// <summary>The family member this operator realizes.</summary>
    public EinsteinianShiabFamilyMember Member => _member;

    /// <summary>The 6x6 Lambda^2 endomorphism R this member realizes (for the richness certificate).</summary>
    public double[,] Lambda2Endomorphism => Lambda2Algebra.MemberEndomorphism(_member);

    /// <summary>The epsilon mode (trivial | frozen-background | slaved-wilson-smoketest).</summary>
    public string EpsilonMode => _member.EpsilonMode;

    public string BranchId => _member.BranchId;

    public string OutputCarrierType => "curvature-2form";

    /// <summary>Byte-identical to <see cref="IdentityShiabCpu.OutputSignature"/> (carrier match REQUIRED).</summary>
    public TensorSignature OutputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = OutputCarrierType,
        Degree = "2",
        LieAlgebraBasisId = _algebra.BasisOrderId,
        ComponentOrderId = "face-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    // ------------------------------------------------------------------
    // Evaluate
    // ------------------------------------------------------------------

    public FieldTensor Evaluate(
        FieldTensor curvatureF,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(curvatureF);
        ArgumentNullException.ThrowIfNull(omega);

        var ad = ResolveAd(omega.Coefficients);
        var result = ApplyContraction(curvatureF.Coefficients, ad);

        return new FieldTensor
        {
            Label = "S_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _faceCount, _dimG },
        };
    }

    // ------------------------------------------------------------------
    // Linearize (dispatched by EpsilonMode, design §3.5)
    // ------------------------------------------------------------------

    public FieldTensor Linearize(
        FieldTensor curvatureF,
        FieldTensor omega,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(deltaOmega);

        return _member.EpsilonMode == "slaved-wilson-smoketest"
            ? LinearizeFiniteDifference(omega, deltaOmega, manifest, geometry)
            : LinearizeAnalytic(omega, deltaOmega);
    }

    /// <summary>
    /// Analytic exact Jacobian for the LINEAR modes (trivial, frozen-background, and
    /// independent-theta at its theta=0 background): dS/domega(delta) = M( Ad_eps( dF/domega(delta) ) ),
    /// where Ad_eps is the constant conjugator and dF/domega(delta) = d(delta) + [omega, delta] is
    /// the identity-Shiab curvature linearization. Degree-2 in omega (reproduces the no-go).
    /// </summary>
    private FieldTensor LinearizeAnalytic(FieldTensor omega, FieldTensor deltaOmega)
    {
        // Ad is constant in omega for these modes (trivial/independent-theta -> I, frozen -> _adPerCell).
        var ad = PerCellSelector(_adPerCell); // null => identity
        var dF = CovariantExteriorDerivative(omega.Coefficients, deltaOmega.Coefficients);
        var result = ApplyContraction(dF, ad);

        return new FieldTensor
        {
            Label = "dS_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _faceCount, _dimG },
        };
    }

    /// <summary>
    /// Central finite-difference Jacobian for the NONLINEAR slaved-wilson-smoketest mode
    /// (design §3.5, Phase436 JacobianColumn style): dS/domega(delta) ≈
    /// (S(omega + h·delta) - S(omega - h·delta)) / (2h). This captures both the
    /// F-dependence and the eps(omega)-dependence, which is the extra term that lifts
    /// the Hessian degree above 2.
    /// </summary>
    private FieldTensor LinearizeFiniteDifference(
        FieldTensor omega, FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry)
    {
        const double h = 1e-6;
        var plus = EvaluateAtConnection(omega.Coefficients, deltaOmega.Coefficients, +h, manifest, geometry);
        var minus = EvaluateAtConnection(omega.Coefficients, deltaOmega.Coefficients, -h, manifest, geometry);

        var result = new double[_faceCount * _dimG];
        for (int i = 0; i < result.Length; i++)
            result[i] = (plus[i] - minus[i]) / (2.0 * h);

        return new FieldTensor
        {
            Label = "dS_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _faceCount, _dimG },
        };
    }

    /// <summary>
    /// Evaluate S_h at omega + step·delta, recomputing the curvature F from that
    /// connection (so the finite difference sees the full omega-dependence).
    /// </summary>
    private double[] EvaluateAtConnection(
        double[] omega, double[] delta, double step, BranchManifest manifest, GeometryContext geometry)
    {
        var perturbed = new double[omega.Length];
        for (int i = 0; i < omega.Length; i++) perturbed[i] = omega[i] + step * delta[i];

        var conn = new ConnectionField(_mesh, _algebra, perturbed);
        var curvature = CurvatureAssembler.Assemble(conn);

        var ad = ResolveAd(perturbed);
        return ApplyContraction(curvature.Coefficients, ad);
    }

    // ------------------------------------------------------------------
    // Core contraction
    // ------------------------------------------------------------------

    /// <summary>
    /// Apply the per-cell Lambda^2 contraction with a given ad-conjugation. The
    /// <paramref name="adSelector"/> returns the dim(g) x dim(g) Ad matrix for a given
    /// (cell, global-face) pair, or null there for the identity; a null selector means
    /// Ad = identity everywhere. Per-cell dressing (frozen / slaved-wilson-smoketest smoke-test)
    /// ignores the face argument; per-face dressing (independent-theta) ignores the cell
    /// argument. Linear in the input coefficients for fixed Ad.
    /// </summary>
    private double[] ApplyContraction(double[] fCoeffs, Func<int, int, double[,]?>? adSelector)
    {
        var acc = new double[_faceCount * _dimG];

        for (int c = 0; c < _cellFaces.Length; c++)
        {
            var faces = _cellFaces[c];
            int nF = faces.Length;
            var faceMap = _cellFaceMap[c]; // = M - I

            // Gather local face block, applying Ad on the ad-index.
            var fa = new double[nF][];
            for (int j = 0; j < nF; j++)
            {
                int gf = faces[j];
                var row = new double[_dimG];
                var ad = adSelector?.Invoke(c, gf);
                if (ad == null)
                {
                    for (int a = 0; a < _dimG; a++)
                        row[a] = fCoeffs[gf * _dimG + a];
                }
                else
                {
                    for (int a = 0; a < _dimG; a++)
                    {
                        double s = 0;
                        for (int b = 0; b < _dimG; b++)
                            s += ad[a, b] * fCoeffs[gf * _dimG + b];
                        row[a] = s;
                    }
                }
                fa[j] = row;
            }

            // out = (I + faceMap) fa  ->  out[j] = fa[j] + sum_k faceMap[j,k] fa[k].
            for (int j = 0; j < nF; j++)
            {
                int gf = faces[j];
                for (int a = 0; a < _dimG; a++)
                {
                    double s = fa[j][a];
                    for (int k = 0; k < nF; k++)
                        s += faceMap[j, k] * fa[k][a];
                    acc[gf * _dimG + a] += s;
                }
            }
        }

        // Average over the cells sharing each face.
        for (int f = 0; f < _faceCount; f++)
        {
            double inv = _faceInvCount[f];
            for (int a = 0; a < _dimG; a++)
                acc[f * _dimG + a] *= inv;
        }
        return acc;
    }

    /// <summary>
    /// Resolve the ad-conjugation selector (cell, global-face) -> Ad matrix for a given
    /// connection, per epsilon mode. "trivial" and "independent-theta" (whose Evaluate uses
    /// the theta=0 background) return null (Ad = identity); "frozen-background" and the
    /// "slaved-wilson-smoketest" smoke-test return per-cell dressing.
    /// </summary>
    private Func<int, int, double[,]?>? ResolveAd(double[] omega)
    {
        switch (_member.EpsilonMode)
        {
            case "frozen-background":
            {
                var ad = _adPerCell!;
                return (cell, _) => ad[cell];
            }
            case "slaved-wilson-smoketest":
            {
                var ad = BuildOmegaCoupledAd(omega);
                return (cell, _) => ad[cell];
            }
            default: // "trivial", "independent-theta" (theta=0 background)
                return null;
        }
    }

    /// <summary>Wrap per-cell Ad matrices as a (cell, face) selector; null stays null.</summary>
    private static Func<int, int, double[,]?>? PerCellSelector(double[][,]? adPerCell)
        => adPerCell == null ? null : (cell, _) => adPerCell[cell];

    /// <summary>
    /// Build the slaved-wilson-smoketest conjugator Ad_cell = exp(ad_theta) with the Wilson-line
    /// accumulation theta_cell = kappa * sum_{e in cell} omega_e (discrete eps map per
    /// design §3.5 / physics §6e). theta = 0 (kappa = 0 or omega = 0) gives Ad = I, so
    /// the operator reduces to the linear R(F) and reproduces Phase436 degree-2.
    ///
    /// SLAVED-WILSON SMOKE-TEST — NOT the pinned physics treatment (physics memo §6e,
    /// resolved 2026-07-02). The physicist LOCKED independent-theta: theta is a genuine
    /// independent H-valued field varied in a joint (omega, theta) Hessian, NOT a function of
    /// omega. This Wilson eps(omega) = exp(kappa * sum_edges omega_e) map is retained as an
    /// OPTIONAL, LABELED, NON-GATING smoke-test only. The pinned independent-theta arm (separate
    /// path, implemented only when the physicist-RATIFIED design §3.5 lands, possibly M3b) has
    /// three gating controls: (a) LinearizeTheta matches FD; (b) theta=0 reproduces Phase436
    /// degree-2; (c) ISOLATION — with the identity Shiab the theta-block of the joint Hessian is
    /// exactly degenerate (theta absent from Upsilon), proving the degree-lift is from the Shiab's
    /// epsilon-dependence, not the inserted DOF.
    /// This method is the SINGLE point embodying the slaved map. Trivial and frozen-background
    /// modes are unaffected and remain the gating linear modes.
    /// </summary>
    private double[][,] BuildOmegaCoupledAd(double[] omega)
    {
        int cellCount = _cellFaces.Length;
        var ad = new double[cellCount][,];
        for (int c = 0; c < cellCount; c++)
        {
            var theta = new double[_dimG];
            foreach (int e in _mesh.CellEdges[c])
                for (int a = 0; a < _dimG; a++)
                    theta[a] += _kappa * omega[e * _dimG + a];

            ad[c] = AdExp(theta);
        }
        return ad;
    }

    /// <summary>The ad-representation matrix (ad_x)^c_b = sum_a f^c_{ab} x^a.</summary>
    private double[,] AdMatrix(double[] x)
    {
        var m = new double[_dimG, _dimG];
        for (int cc = 0; cc < _dimG; cc++)
            for (int b = 0; b < _dimG; b++)
            {
                double s = 0;
                for (int a = 0; a < _dimG; a++)
                    s += _algebra.GetStructureConstant(a, b, cc) * x[a];
                m[cc, b] = s;
            }
        return m;
    }

    /// <summary>Ad = exp(ad_theta).</summary>
    private double[,] AdExp(double[] theta) => Lambda2Algebra.MatrixExp(AdMatrix(theta));

    // ------------------------------------------------------------------
    // Mode (2): independent-theta / joint (omega, theta) Hessian plumbing.
    //
    // theta is a genuine INDEPENDENT H-valued fluctuation field on vertices (length
    // VertexCount * dim(g)), NOT a function of omega (physics memo §6e, ratified spec
    // pending). eps_v = exp(theta_v); the ad-conjugator Ad = exp(ad_theta) acts on the
    // ad-index and is evaluated per face at the face's representative vertex = the
    // lowest-index incident vertex (mesh.Faces[f][0], already canonically sorted).
    // S_h(F; theta)_face = M( Ad_{theta at face}(F_face) ), M the per-cell Lambda^2
    // contraction. theta = 0 => Ad = I => S_h = M(F) (identical to the trivial mode), so
    // the theta=0 slice reproduces the Phase436 degree-2 structure. The joint (omega,theta)
    // Hessian is assembled by the harness (EinsteinianShiabBatteries) via the same Phase436
    // finite-difference machinery on the enlarged vector.
    // ------------------------------------------------------------------

    /// <summary>
    /// Evaluate S_h at an explicit independent theta field (mode 2). <paramref name="theta"/>
    /// has length VertexCount * dim(g); theta = 0 reproduces the linear (trivial) evaluation.
    /// </summary>
    public FieldTensor EvaluateWithTheta(
        FieldTensor curvatureF,
        FieldTensor omega,
        double[] theta,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(curvatureF);
        ArgumentNullException.ThrowIfNull(theta);
        var adPerFace = BuildPerFaceAd(theta);
        var result = ApplyContraction(curvatureF.Coefficients, (_, gf) => adPerFace[gf]);
        return new FieldTensor
        {
            Label = "S_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _faceCount, _dimG },
        };
    }

    /// <summary>
    /// Analytic Jacobian in the theta direction (mode 2): dS/dtheta(deltaTheta). Since
    /// S_h = M(Ad(theta)·F) with M linear and F fixed w.r.t. theta,
    /// dS/dtheta = M( dAd/dtheta(deltaTheta)·F ), where per face dAd is the Fréchet derivative
    /// of exp(ad_theta_v) in direction ad_{deltaTheta_v} (v = the face's representative vertex).
    /// The §4.5 finite-difference battery validates it. GATING control (i) for mode 2.
    /// </summary>
    public FieldTensor LinearizeTheta(
        FieldTensor curvatureF,
        FieldTensor omega,
        double[] theta,
        double[] deltaTheta,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(curvatureF);
        ArgumentNullException.ThrowIfNull(theta);
        ArgumentNullException.ThrowIfNull(deltaTheta);

        // gField[f] = dAd_f(deltaTheta) · F_face, then apply the pure M-contraction (no Ad).
        var g = new double[_faceCount * _dimG];
        for (int f = 0; f < _faceCount; f++)
        {
            int v = _mesh.Faces[f][0]; // representative vertex = lowest index
            var thetaV = ExtractVertexBlock(theta, v);
            var dThetaV = ExtractVertexBlock(deltaTheta, v);
            var dAd = Lambda2Algebra.MatrixExpFrechet(AdMatrix(thetaV), AdMatrix(dThetaV));
            for (int a = 0; a < _dimG; a++)
            {
                double s = 0;
                for (int b = 0; b < _dimG; b++)
                    s += dAd[a, b] * curvatureF.Coefficients[f * _dimG + b];
                g[f * _dimG + a] = s;
            }
        }

        var result = ApplyContraction(g, null);
        return new FieldTensor
        {
            Label = "dS_h_dtheta",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _faceCount, _dimG },
        };
    }

    /// <summary>Per-face Ad = exp(ad_theta) at each face's representative (lowest-index) vertex.</summary>
    private double[][,] BuildPerFaceAd(double[] theta)
    {
        int expectedLen = _mesh.VertexCount * _dimG;
        if (theta.Length != expectedLen)
            throw new ArgumentException(
                $"theta length {theta.Length} must be VertexCount*dim(g) = {expectedLen}.", nameof(theta));

        var ad = new double[_faceCount][,];
        var cache = new Dictionary<int, double[,]>();
        for (int f = 0; f < _faceCount; f++)
        {
            int v = _mesh.Faces[f][0];
            if (!cache.TryGetValue(v, out var m))
            {
                m = AdExp(ExtractVertexBlock(theta, v));
                cache[v] = m;
            }
            ad[f] = m;
        }
        return ad;
    }

    private double[] ExtractVertexBlock(double[] field, int vertex)
    {
        var block = new double[_dimG];
        Array.Copy(field, vertex * _dimG, block, 0, _dimG);
        return block;
    }

    /// <summary>
    /// dF/domega(delta) = d(delta) + [omega, delta] on each face — the identity-Shiab
    /// curvature linearization (same as <see cref="IdentityShiabCpu"/>).
    /// </summary>
    private double[] CovariantExteriorDerivative(double[] omega, double[] delta)
    {
        var result = new double[_faceCount * _dimG];
        for (int fi = 0; fi < _faceCount; fi++)
        {
            var boundaryEdges = _mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = _mesh.FaceBoundaryOrientations[fi];

            var dDelta = new double[_dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < _dimG; a++)
                    dDelta[a] += sign * delta[edgeIdx * _dimG + a];
            }

            var bracketTerm = new double[_dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    var omegaI = new double[_dimG];
                    var omegaJ = new double[_dimG];
                    var deltaI = new double[_dimG];
                    var deltaJ = new double[_dimG];
                    for (int a = 0; a < _dimG; a++)
                    {
                        omegaI[a] = si * omega[ei * _dimG + a];
                        omegaJ[a] = sj * omega[ej * _dimG + a];
                        deltaI[a] = si * delta[ei * _dimG + a];
                        deltaJ[a] = sj * delta[ej * _dimG + a];
                    }

                    var b1 = _algebra.Bracket(omegaI, deltaJ);
                    var b2 = _algebra.Bracket(deltaI, omegaJ);
                    for (int a = 0; a < _dimG; a++)
                        bracketTerm[a] += 0.5 * (b1[a] + b2[a]);
                }

            for (int a = 0; a < _dimG; a++)
                result[fi * _dimG + a] = dDelta[a] + bracketTerm[a];
        }
        return result;
    }

    // ------------------------------------------------------------------
    // Precomputation
    // ------------------------------------------------------------------

    private (int[][] cellFaces, double[][,] cellFaceMap, double[] faceInvCount) PrecomputeCellMaps(
        SimplicialMesh mesh, double[,] rMinusI)
    {
        int cellCount = mesh.CellCount;
        var cellFaces = new int[cellCount][];
        var cellFaceMap = new double[cellCount][,];
        var faceCount = mesh.FaceCount;
        var faceCount2 = new int[faceCount];

        for (int c = 0; c < cellCount; c++)
        {
            var faces = mesh.CellFaces[c];
            cellFaces[c] = faces;
            int nF = faces.Length;

            // W : Dim x nF, columns are face bivectors in Lambda^2(R^4).
            var w = new double[Lambda2Algebra.Dim, nF];
            for (int j = 0; j < nF; j++)
            {
                var verts = mesh.Faces[faces[j]];
                var pa = mesh.GetVertexCoordinates(verts[0]);
                var pb = mesh.GetVertexCoordinates(verts[1]);
                var pc = mesh.GetVertexCoordinates(verts[2]);
                var u = new double[4];
                var v = new double[4];
                for (int d = 0; d < 4; d++)
                {
                    u[d] = pb[d] - pa[d];
                    v[d] = pc[d] - pa[d];
                }
                var biv = Lambda2Algebra.Wedge(u, v);
                for (int k = 0; k < Lambda2Algebra.Dim; k++) w[k, j] = biv[k];

                faceCount2[faces[j]]++;
            }

            // Q = (W W^T)^{-1} W  (Dim x nF); faceMap = W^T (R - I) Q  (nF x nF).
            var wt = Lambda2Algebra.Transpose(w);                 // nF x Dim
            var wwt = Lambda2Algebra.Multiply(w, wt);             // Dim x Dim
            var wwtInv = Lambda2Algebra.Invert(wwt);              // Dim x Dim
            var q = Lambda2Algebra.Multiply(wwtInv, w);           // Dim x nF
            var rmiQ = Lambda2Algebra.Multiply(rMinusI, q);       // Dim x nF
            var faceMap = Lambda2Algebra.Multiply(wt, rmiQ);      // nF x nF
            cellFaceMap[c] = faceMap;
        }

        var invCount = new double[faceCount];
        for (int f = 0; f < faceCount; f++)
            invCount[f] = faceCount2[f] > 0 ? 1.0 / faceCount2[f] : 0.0;

        return (cellFaces, cellFaceMap, invCount);
    }

    private double[][,] BuildFrozenAd(double[]? backgroundEps)
    {
        int cellCount = _cellFaces.Length;
        var ad = new double[cellCount][,];

        if (backgroundEps == null)
        {
            // No background supplied -> eps = 1 everywhere (degenerate frozen mode).
            var identity = Lambda2Algebra.Identity(_dimG);
            for (int c = 0; c < cellCount; c++) ad[c] = identity;
            return ad;
        }

        if (backgroundEps.Length == _dimG)
        {
            var global = AdExp(backgroundEps);
            for (int c = 0; c < cellCount; c++) ad[c] = global;
            return ad;
        }

        if (backgroundEps.Length == cellCount * _dimG)
        {
            for (int c = 0; c < cellCount; c++)
            {
                var theta = new double[_dimG];
                Array.Copy(backgroundEps, c * _dimG, theta, 0, _dimG);
                ad[c] = AdExp(theta);
            }
            return ad;
        }

        throw new ArgumentException(
            $"backgroundEps length {backgroundEps.Length} must be dim(g)={_dimG} (global) " +
            $"or CellCount*dim(g)={cellCount * _dimG} (per-cell).",
            nameof(backgroundEps));
    }
}
