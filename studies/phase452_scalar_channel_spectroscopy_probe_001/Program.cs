using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Cx = System.Numerics.Complex;

// Phase452: 0++ SCALAR-CHANNEL MASS-GAP SPECTROSCOPY on the lattice-canonical
// torus -- the 2026-07-05 task-force WS2 workstream (BINDING: cosh-corrected
// effective masses ONLY; strictly gauge+translation-invariant operators only;
// theta-Haar; report null as null), inheriting the 2026-07-04 review board's
// four phase450 binding conditions. This is the first MEASUREMENT test of the
// review board's convex/gapped picture: the true effective potential is
// convex, so the physics lives in gauge-invariant composite POLES (the
// Elitzur/FMS reframe) -- here, the 0++ scalar channel of the pure S_B
// ensemble e^{-beta S_B} (S_B >= 0 real: no sign problem).
//
// ENSEMBLE. HMC on omega (the prototype pattern from
// studies/phase448_torus_mode_volume_saturation_probe_001/cep_hmc_prototype/,
// forces from the analytic ComputeJointGradient) interleaved with a
// Haar-invariant per-vertex rotation Metropolis on theta (theta enters S_B
// only through Ad = exp(ad_theta) in SO(3) per vertex -- review-board binding
// condition (ii): the R^n theta integral diverges; the group-composition
// proposal R'_v = exp(ad_delta_v) exp(ad_theta_v) is symmetric w.r.t. Haar, so
// plain Metropolis targets Haar x e^{-beta S_B}). Members: identity control
// (theta-independent, battery-verified) and sd2-id0/c0.5 (Einsteinian,
// independent-theta). beta = 1 production columns (recorded convention), one
// higher-beta physics control column, and one large-beta FREE-FIELD control
// column per member (theta frozen at 0, labeled SAMPLER-DEMO per binding
// condition (iv) -- the free-field control validates the sampler+pipeline,
// never a physics verdict).
//
// INTERPOLATORS (strictly gauge + translation invariant; both interpolate the
// same 0++ channel):
//   O1(tau) = sum over faces in time-slice tau of tr(F_f^2) (trace-pairing
//             InnerProduct of the CurvatureAssembler face coefficients);
//   O2(tau) = sum over faces in time-slice tau of Tr(Upsilon_f^dagger
//             Upsilon_f), Upsilon = S_B's own residual via the operator's
//             EvaluateWithTheta path (Evaluate for the identity member).
// SLICE CONVENTION (documented): lattice axis 0 is Euclidean time; a face
// belongs to the slice of its CANONICAL BASE VERTEX = the unique vertex from
// which the minimal-image displacements of the other two vertices are both in
// {0,1}^4 (the Kuhn-chain minimum; manifestly translation covariant).
//
// CORRELATORS. Vacuum-subtracted connected C(t) = <O(t)O(0)> - <O>^2 averaged
// over all time translations. COSH-CORRECTED effective mass ONLY: solve
// C(t)/C(t+1) = cosh(m(t - T/2))/cosh(m(t+1 - T/2)) (the naive log-ratio was
// PROVEN corrupted by the periodic image -- 2026-07-05 referee correction).
// PRE-REGISTERED WINDOW RULE: informative points are t <= T/2 - 1 (beyond
// that the cosh ratio is identically 1); window = {t : 1 <= t <= T/2 - 1} if
// nonempty, else {0} with the t=0 excited-state-contamination caveat RECORDED
// (T = n = 3 has exactly one informative point). Plateau chi^2/dof gate
// applies when the window has >= 2 points; a single-point window is recorded
// as such and the mass carries the caveat.
//
// FAIL-CLOSED GATES.
//  (1) FREE-FIELD CONTROL (load-bearing): the analytic Gaussian correlator is
//      built from the exact block spectrum of the omega Hessian at omega = 0
//      (phase448's block-circulant momentum machinery: 45 orbit-representative
//      Hv columns -> n^4 Hermitian 45x45 momentum blocks -> eigenpairs), via
//      C_free(t) = 2 n^3 sum_{F,F',dx} Tr(K M K M^T) with
//      M = Cov(dx)[F,F'] = (1/(beta V)) sum_k e^{2 pi i k.dx/n}
//      G(k) H(k)^+ G(k)^dagger (G = the interpolator's exact linearization
//      kernel, K = the Lie Gram). The sampler's large-beta column must
//      reproduce the analytic cosh m_eff within statistical error (3 sigma,
//      O1 gating; O2 recorded). The analytic formula itself is validated by
//      an EXACT direct-Gaussian-sampling battery through the identical
//      measurement pipeline. Free columns sample e^{-beta S_B} restricted to
//      the orthogonal complement of ker H(0) (the exact flat/pure-gauge cone
//      tangent -- the Gaussian theory does not exist on ker H and the
//      analytic prediction excludes it; RECORDED CONVENTION, free columns
//      only; physics columns are unrestricted).
//  (2) <e^{-dH}> = 1 and equipartition <sum_i omega_i beta dS/domega_i> =
//      nDof per column (jackknifed).
//  (3) plateau quality over the pre-registered window.
//  (4) O1-vs-O2 cross-check: both interpolate 0++, so their masses must be
//      compatible (3 sigma); incompatibility fail-closes to
//      inconclusive-statistics.
//  (5) uniform-theta invariance assert: a UNIFORM (same at every site) gauge
//      rotation g -- omega_e -> Ad_g omega_e, theta_v -> Ad_g theta_v --
//      must leave S_B and every O1/O2 slice invariant (this is the exact
//      discrete global-gauge invariance of the interpolators).
// Gates (1), (2), (5) plus the exactness batteries gate the PHASE (blocked on
// failure); gates (3), (4) plus the tau_int-based N_eff >= 100 gate map to
// the per-member verdict "inconclusive-statistics" (fail-closed: no mass
// claim escapes a failed statistics gate).
//
// VERDICTS per production member/channel: a*m_0++ with jackknife errors;
// "scalar-channel-gapped-measured" (m >= 3 sigma), "scalar-channel-gapless"
// (m <= 2 sigma with sigma <= 0.3), else "inconclusive-statistics".
// LABEL CAVEAT (binding): this is the scalar glueball-like gap of THIS
// action; NEVER m_H -- W/Z/H labels attach only in a Higgs phase.
//
// MANDATORY FRAMING. Workbench-relative structure data ONLY (su(2) toy
// algebra on the reduced Spin(4) slice, lattice units); NO GeV/pole/VEV
// promotion either way. Fail-closed: target-blind; reduced-spin4-slice; no
// Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase452_scalar_channel_spectroscopy_probe_001/output";
const string Phase448SummaryPath = "studies/phase448_torus_mode_volume_saturation_probe_001/output/torus_mode_volume_saturation_probe_summary.json";
const string Phase449SummaryPath = "studies/phase449_variational_gaussian_effective_potential_probe_001/output/variational_gaussian_effective_potential_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ReviewBoardSourcePath = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md";
const string RestartPromptSourcePath = "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md";
const string ApplicationSubjectKind = "scalar-channel-spectroscopy-probe";
const string TerminalPrefix = "scalar-channel-spectroscopy-probe-";
const string Phase456ModeArg = "--phase456-production";
const string Phase456SmokeArg = "--phase456-smoke";
const string Phase456PackPath = "studies/phase456_consolidated_n4_launch_001/preregistration/a4_symmetry_irrep_projectors.json";
const string Phase456PackDir = "studies/phase456_consolidated_n4_launch_001/preregistration";
const string Phase456PinnedPackSha256 = "40fd3c3488f94d18f50961e85d0bb3a3eabd1a31a071b61149875b8cf3d437aa";
const string Phase456OutputDir = "studies/phase456_consolidated_n4_launch_001/production/phase452_n4";

bool phase456ProductionMode = args.Length == 1 && args[0] == Phase456ModeArg;
bool phase456SmokeMode = args.Length == 1 && args[0] == Phase456SmokeArg;
bool phase456Mode = phase456ProductionMode || phase456SmokeMode;
if (args.Length != 0 && !phase456Mode)
    throw new InvalidOperationException($"unknown argument(s): {string.Join(' ', args)}");
if (phase456Mode)
{
    string[] forbiddenOverrides = Environment.GetEnvironmentVariables().Keys.Cast<object>()
        .Select(x => x.ToString() ?? string.Empty)
        .Where(x => x.StartsWith("PHASE", StringComparison.Ordinal))
        .OrderBy(x => x, StringComparer.Ordinal)
        .ToArray();
    if (forbiddenOverrides.Length != 0)
        throw new InvalidOperationException("Phase456 production must be env-clean; forbidden overrides: " + string.Join(", ", forbiddenOverrides));
}

// --- Configuration (production defaults; env overrides). --------------------
int[] torusSizes = phase456Mode
    ? new[] { 4 }
    : (Environment.GetEnvironmentVariable("PHASE452_TORI") ?? "3").Split(',').Select(int.Parse).ToArray();
int trajProd = phase456ProductionMode ? 16000 : phase456SmokeMode ? 8 : int.TryParse(Environment.GetEnvironmentVariable("PHASE452_TRAJ"), out int tp) ? tp : 16000;
int trajCtrl = phase456ProductionMode ? 10000 : phase456SmokeMode ? 8 : int.TryParse(Environment.GetEnvironmentVariable("PHASE452_CTRL_TRAJ"), out int tc) ? tc : 10000;
int warmTraj = phase456ProductionMode ? 2000 : phase456SmokeMode ? 2 : int.TryParse(Environment.GetEnvironmentVariable("PHASE452_WARM"), out int tw) ? tw : 2000;
int nLeap = phase456Mode ? 12 : int.TryParse(Environment.GetEnvironmentVariable("PHASE452_NLEAP"), out int nl) ? nl : 12;
int gaussSimSamples = phase456ProductionMode ? 4000 : phase456SmokeMode ? 8 : int.TryParse(Environment.GetEnvironmentVariable("PHASE452_GAUSS_SIM"), out int gs) ? gs : 4000;
double betaProd = phase456Mode ? 1.0 : double.TryParse(Environment.GetEnvironmentVariable("PHASE452_BETA"), out double bp) ? bp : 1.0;
double betaMid = phase456Mode ? 4.0 : double.TryParse(Environment.GetEnvironmentVariable("PHASE452_BETA_MID"), out double bm) ? bm : 4.0;
double betaFree = phase456Mode ? 400.0 : double.TryParse(Environment.GetEnvironmentVariable("PHASE452_BETA_FREE"), out double bf) ? bf : 400.0;
var outputDir = phase456ProductionMode ? Phase456OutputDir : phase456SmokeMode ? "/tmp/geometricunity_phase456_smoke" : Environment.GetEnvironmentVariable("PHASE452_OUTPUT_DIR") ?? DefaultOutputDir;

int[] phase456A2Kernel = Array.Empty<int>();
int phase456A2Denominator = 0;
string? phase456ComputedPackSha256 = null;
if (phase456Mode)
{
    string[] pinnedFiles = { "a4_symmetry_irrep_projectors.json", "a5_gaussian_domination_stage_a.json", "pack_manifest.json" };
    var hashBytes = new List<byte>();
    foreach (string file in pinnedFiles) hashBytes.AddRange(File.ReadAllBytes(Path.Combine(Phase456PackDir, file)));
    phase456ComputedPackSha256 = Convert.ToHexString(SHA256.HashData(hashBytes.ToArray())).ToLowerInvariant();
    if (phase456ComputedPackSha256 != Phase456PinnedPackSha256)
        throw new InvalidOperationException($"Phase456 pack hash mismatch: expected {Phase456PinnedPackSha256}, got {phase456ComputedPackSha256}");
    using var pack = JsonDocument.Parse(File.ReadAllText(Phase456PackPath));
    var channel = pack.RootElement.GetProperty("projectorKernels").GetProperty("nonIdentityChannel");
    phase456A2Kernel = channel.GetProperty("kernelRowNumerators").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    phase456A2Denominator = channel.GetProperty("kernelDenominator").GetInt32();
    if (phase456A2Kernel.Length != 50 || phase456A2Denominator != 6)
        throw new InvalidOperationException("Phase456 committed A2 projector does not have the expected exact 50-entry /6 form");
}

const int RngSeed = 20260705;
const int InvariantRayRngSeed = 20260703;
const int TimeAxis = 0;                    // documented convention: lattice axis 0 = Euclidean time
const int ThetaMovesPerTraj = 2;           // collective Haar-Metropolis moves per trajectory
const int ThetaSweepEvery = 50;            // single-site Haar sweep cadence (ergodicity aid)
const double ThetaSiteSigma = 0.5;         // single-site proposal angle scale
const int JackknifeBlocks = 50;
const double HvStep = 1e-5;                // Hessian-vector FD step (analytic-gradient central difference)
const double LinStep = 1e-3;               // interpolator-linearization central-difference step (exact: maps are quadratic)
const double ZeroModeRelTol = 1e-8;        // verbatim IR convention (phase448)
const double ZeroModeAbsFloor = 1e-10;
const double ObjectiveConsistencyGate = 1e-10;
const double RotationInvarianceGate = 1e-8;    // gate (5)
const double BlockResidualGate = 1e-9;         // eigensolver residual per block (relative)
const double PlaneWaveHvGate = 1e-5;           // block-vs-direct Hv on a random plane wave (FD noise)
const double ExpLogGate = 1e-10;               // rotation-coordinate round-trip batteries
const double GaussSimSigmaGate = 5.0;          // exact-Gaussian pipeline battery (sigma)
const double FreeFieldSigmaGate = 3.0;         // gate (1): |m_meas - m_analytic| <= 3 sigma
const double CrossCheckSigmaGate = 3.0;        // gate (4)
const double DHSigmaGate = 3.0;                // gate (2a): <e^-dH> within 3 sigma of 1 (or abs)
const double DHAbsGate = 0.01;
const double VirialSigmaGate = 3.0;            // gate (2b): virial within 3 sigma of nDof (or rel)
const double VirialRelGate = 0.02;
const double NeffMin = 100.0;                  // tau_int-based effective-sample gate
const double PlateauChi2Max = 3.0;             // chi^2/dof when window has >= 2 points
const double GappedSignificance = 3.0;         // pre-registered verdict rule
const double GaplessSigmaCeiling = 0.3;        // pre-registered verdict rule

Directory.CreateDirectory(outputDir);

// --- Precursors (phase448 + phase449). ---------------------------------------
using var phase448 = JsonDocument.Parse(File.ReadAllText(Phase448SummaryPath));
bool phase448PrecursorPassed = JsonBool(phase448.RootElement, "torusModeVolumeSaturationProbePassed") is true;
using var phase449 = JsonDocument.Parse(File.ReadAllText(Phase449SummaryPath));
bool phase449PrecursorPassed = JsonBool(phase449.RootElement, "variationalGaussianEffectivePotentialProbePassed") is true;
bool precursorsPassed = phase448PrecursorPassed && phase449PrecursorPassed;

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();

// Lie Gram K[l,l'] (trace pairing) and basis bracket matrices.
var lieGram = new double[dimG, dimG];
var basis = new double[dimG][];
for (int l = 0; l < dimG; l++) { basis[l] = new double[dimG]; basis[l][l] = 1.0; }
for (int l = 0; l < dimG; l++)
    for (int m = 0; m < dimG; m++)
        lieGram[l, m] = algebra.InnerProduct(basis[l], basis[m]);

var stopwatch = Stopwatch.StartNew();
var torusRecords = new List<TorusRecord>();
var consoleLock = new object();

foreach (int n in torusSizes)
{
    int T = n; // time extent
    var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
    int nOmega = mesh.EdgeCount * dimG;
    int nTheta = mesh.VertexCount * dimG;
    int volume = mesh.VertexCount;

    // --- Orbit machinery (verbatim phase448 conventions). --------------------
    var coords = new int[mesh.VertexCount][];
    double spacing = double.MaxValue;
    for (int v = 0; v < mesh.VertexCount; v++)
    {
        var p = mesh.GetVertexCoordinates(v);
        for (int d = 0; d < 4; d++) if (p[d] > 1e-12) spacing = System.Math.Min(spacing, p[d]);
        coords[v] = new int[4];
    }
    for (int v = 0; v < mesh.VertexCount; v++)
    {
        var p = mesh.GetVertexCoordinates(v);
        for (int d = 0; d < 4; d++) coords[v][d] = (int)System.Math.Round(p[d] / spacing);
    }
    var vertexAt = new Dictionary<(int, int, int, int), int>();
    for (int v = 0; v < mesh.VertexCount; v++)
        vertexAt[(coords[v][0], coords[v][1], coords[v][2], coords[v][3])] = v;

    int TypeOf(int[] disp) => ((disp[0] << 3) | (disp[1] << 2) | (disp[2] << 1) | disp[3]) - 1;
    var edgeBase = new int[mesh.EdgeCount];
    var edgeType = new int[mesh.EdgeCount];
    bool orbitMapOk = true;
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int a = mesh.Edges[e][0], b = mesh.Edges[e][1];
        int[]? disp = MinImage01(coords[a], coords[b], n);
        if (disp != null) { edgeBase[e] = a; edgeType[e] = TypeOf(disp); }
        else
        {
            disp = MinImage01(coords[b], coords[a], n);
            if (disp != null) { edgeBase[e] = b; edgeType[e] = TypeOf(disp); }
            else orbitMapOk = false;
        }
    }
    var edgeAt = new int[mesh.VertexCount, 15];
    for (int e = 0; e < mesh.EdgeCount; e++) edgeAt[edgeBase[e], edgeType[e]] = e;
    var oSign = new double[mesh.EdgeCount];
    for (int e = 0; e < mesh.EdgeCount; e++)
        oSign[e] = mesh.Edges[e][0] == edgeBase[e] ? 1.0 : -1.0;

    // --- Face orbit machinery: canonical base vertex + face type + slice. ----
    // Canonical base = the unique face vertex from which the min-image
    // displacements of both other vertices lie in {0,1}^4 (Kuhn-chain minimum).
    var faceBase = new int[mesh.FaceCount];
    var faceTypeId = new int[mesh.FaceCount];
    var faceSlice = new int[mesh.FaceCount];
    var faceTypeKeyToId = new Dictionary<(int, int), int>();
    bool faceMapOk = true;
    int faceBaseEqualsFaces0Count = 0;
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        var verts = mesh.Faces[f];
        int baseV = -1; int[]? dA = null, dB = null;
        for (int i = 0; i < verts.Length; i++)
        {
            int cand = verts[i];
            int[]? d1 = null, d2 = null;
            bool ok = true;
            int slot = 0;
            for (int j = 0; j < verts.Length; j++)
            {
                if (j == i) continue;
                var d = MinImage01(coords[cand], coords[verts[j]], n);
                if (d == null) { ok = false; break; }
                if (slot++ == 0) d1 = d; else d2 = d;
            }
            if (ok && d1 != null && d2 != null)
            {
                if (baseV >= 0) { faceMapOk = false; }
                baseV = cand; dA = d1; dB = d2;
            }
        }
        if (baseV < 0 || dA == null || dB == null) { faceMapOk = false; continue; }
        // Order the two displacements by nesting (Kuhn chain: one dominates).
        int sumA = dA.Sum(), sumB = dB.Sum();
        var (dLo, dHi) = sumA <= sumB ? (dA, dB) : (dB, dA);
        var key = (TypeOf(dLo), TypeOf(dHi));
        if (!faceTypeKeyToId.TryGetValue(key, out int tid))
        {
            tid = faceTypeKeyToId.Count;
            faceTypeKeyToId[key] = tid;
        }
        faceBase[f] = baseV;
        faceTypeId[f] = tid;
        faceSlice[f] = coords[baseV][TimeAxis];
        if (mesh.Faces[f][0] == baseV) faceBaseEqualsFaces0Count++;
    }
    int faceTypeCount = faceTypeKeyToId.Count;
    int slabFaces = mesh.FaceCount / T;
    // Uniform-orbit asserts.
    var typeCounts = new int[faceTypeCount];
    var sliceCounts = new int[T];
    for (int f = 0; f < mesh.FaceCount; f++) { typeCounts[faceTypeId[f]]++; sliceCounts[faceSlice[f]]++; }
    bool faceOrbitsUniform = typeCounts.All(c => c == volume) && sliceCounts.All(c => c == slabFaces);
    var faceAt = new int[mesh.VertexCount, faceTypeCount];
    for (int f = 0; f < mesh.FaceCount; f++) faceAt[faceBase[f], faceTypeId[f]] = f;

    int fdim = faceTypeCount * dimG; // face-value block dimension (types x lie)
    int blockDim = 15 * dimG;        // omega momentum-block dimension

    // Phase456 reuses the phase450/448 invariant-ray convention byte-for-byte.
    // The projection removes the global su(2) orbit before Phi is measured.
    var uInv = new double[nOmega];
    double phase456ProjectorOverlapMax = 0.0;
    if (phase456Mode)
    {
        if (faceTypeCount != phase456A2Kernel.Length)
            throw new InvalidOperationException($"Phase456 face-type count {faceTypeCount} does not match the committed projector length {phase456A2Kernel.Length}");
        var rayCoeffRng = new Random(InvariantRayRngSeed);
        var typeCoeffs = new double[16 * dimG];
        for (int i = 0; i < typeCoeffs.Length; i++) typeCoeffs[i] = rayCoeffRng.NextDouble() - 0.5;
        for (int e = 0; e < mesh.EdgeCount; e++)
            for (int l = 0; l < dimG; l++)
                uInv[e * dimG + l] = oSign[e] * typeCoeffs[edgeType[e] * dimG + l];
        double un = Norm(uInv);
        for (int i = 0; i < nOmega; i++) uInv[i] /= un;

        var zOrtho = new List<double[]>();
        for (int l = 0; l < dimG; l++)
        {
            var z = new double[nOmega];
            var el = new double[dimG]; el[l] = 1.0;
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                var ue = new double[dimG];
                for (int c = 0; c < dimG; c++) ue[c] = uInv[e * dimG + c];
                var br = algebra.Bracket(el, ue);
                for (int c = 0; c < dimG; c++) z[e * dimG + c] = br[c];
            }
            foreach (var q in zOrtho)
            {
                double d = Dot(z, q);
                for (int i = 0; i < nOmega; i++) z[i] -= d * q[i];
            }
            double zn = Norm(z);
            if (zn > 1e-12)
            {
                for (int i = 0; i < nOmega; i++) z[i] /= zn;
                zOrtho.Add(z);
            }
        }
        foreach (var q in zOrtho)
        {
            double d = Dot(uInv, q);
            for (int i = 0; i < nOmega; i++) uInv[i] -= d * q[i];
        }
        un = Norm(uInv);
        for (int i = 0; i < nOmega; i++) uInv[i] /= un;
        phase456ProjectorOverlapMax = zOrtho.Count == 0 ? 0.0 : zOrtho.Max(q => System.Math.Abs(Dot(uInv, q)));
        if (phase456ProjectorOverlapMax > 1e-12)
            throw new InvalidOperationException($"Phase456 invariant-ray projection battery failed: {phase456ProjectorOverlapMax:E3}");
    }

    int KeyOfK(int[] k) => ((k[0] * n + k[1]) * n + k[2]) * n + k[3];
    var kList = new List<int[]>();
    for (int a0 = 0; a0 < n; a0++) for (int a1 = 0; a1 < n; a1++)
    for (int a2 = 0; a2 < n; a2++) for (int a3 = 0; a3 < n; a3++)
        kList.Add(new[] { a0, a1, a2, a3 });
    int nK = kList.Count;

    // --- Members. -------------------------------------------------------------
    var controlSpec = new EinsteinianShiabFamilyMember
    {
        Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = "trivial",
    };
    var sd2Spec = new EinsteinianShiabFamilyMember
    {
        Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
        EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
    };
    EinsteinianShiabOperator MakeOp(bool isControl) =>
        new(mesh, algebra, isControl ? controlSpec : sd2Spec, latticePeriod: n);
    var memberNames = new[] { "identity", "sd2-id0/c0.5" };
    var memberIsControl = new[] { true, false };

    // Shared helpers over this torus. ------------------------------------------
    double[] UpsilonOf(EinsteinianShiabOperator op, bool isControl, double[] omega, double[] theta,
        out double[] fCoeffs)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        var curv = CurvatureAssembler.Assemble(conn);
        fCoeffs = curv.Coefficients;
        var f = curv.ToFieldTensor();
        var ups = isControl
            ? op.Evaluate(f, conn.ToFieldTensor(), manifest, geometry)
            : op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry);
        return ups.Coefficients;
    }

    void SliceSums(double[] faceCoeffs, double[] o1Out, double[] upsCoeffs, double[] o2Out)
    {
        Array.Clear(o1Out); Array.Clear(o2Out);
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            int s = faceSlice[f];
            double a1 = 0, a2 = 0;
            for (int l = 0; l < dimG; l++)
                for (int m = 0; m < dimG; m++)
                {
                    a1 += lieGram[l, m] * faceCoeffs[f * dimG + l] * faceCoeffs[f * dimG + m];
                    a2 += lieGram[l, m] * upsCoeffs[f * dimG + l] * upsCoeffs[f * dimG + m];
                }
            o1Out[s] += a1; o2Out[s] += a2;
        }
    }

    Phase456Measurement MeasurePhase456(double[] faceCoeffs, double[] upsCoeffs, double[] omega)
    {
        var a2O1 = new double[T];
        var a2O2 = new double[T];
        var kO1Re = Enumerable.Range(0, 3).Select(_ => new double[T]).ToArray();
        var kO1Im = Enumerable.Range(0, 3).Select(_ => new double[T]).ToArray();
        var siteO1 = new double[volume];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            double d1 = 0.0, d2 = 0.0;
            for (int l = 0; l < dimG; l++)
                for (int m = 0; m < dimG; m++)
                {
                    d1 += lieGram[l, m] * faceCoeffs[f * dimG + l] * faceCoeffs[f * dimG + m];
                    d2 += lieGram[l, m] * upsCoeffs[f * dimG + l] * upsCoeffs[f * dimG + m];
                }
            int s = faceSlice[f];
            siteO1[faceBase[f]] += d1;
            double w = (double)phase456A2Kernel[faceTypeId[f]] / phase456A2Denominator;
            a2O1[s] += w * d1;
            a2O2[s] += w * d2;
            for (int axisIndex = 0; axisIndex < 3; axisIndex++)
            {
                int q = coords[faceBase[f]][axisIndex + 1] & 3;
                double re = q switch { 0 => 1.0, 2 => -1.0, _ => 0.0 };
                double im = q switch { 1 => -1.0, 3 => 1.0, _ => 0.0 };
                kO1Re[axisIndex][s] += re * d1;
                kO1Im[axisIndex][s] += im * d1;
            }
        }
        return new Phase456Measurement(a2O1, a2O2, kO1Re, kO1Im, siteO1, Dot(uInv, omega));
    }

    // --- Startup batteries (per member) + analytic free-field machinery. -----
    var memberAnalytics = new AnalyticData[2];
    var memberBatteries = new MemberBattery[2];

    for (int mi = 0; mi < 2; mi++)
    {
        bool isControl = memberIsControl[mi];
        var op = MakeOp(isControl);
        var mass = new CpuMassMatrix(mesh, algebra);
        var rng = new Random(RngSeed + 101 * mi + 7 * n);

        // (a) Objective consistency: gradient path vs 0.5 * sum O2 slices.
        var xw = RandomVector(rng, nOmega, 0.3);
        var xt = isControl ? new double[nTheta] : RandomVector(rng, nTheta, 0.5);
        var g0 = op.ComputeJointGradient(xw, xt, mass);
        var ups0 = UpsilonOf(op, isControl, xw, xt, out var fc0);
        var o1a = new double[T]; var o2a = new double[T];
        SliceSums(fc0, o1a, ups0, o2a);
        double sDirect = 0.5 * o2a.Sum();
        double objConsistency = RelDiff(g0.Objective, sDirect);

        // (b) Gate (5): uniform-theta (global gauge rotation) invariance.
        var cRot = new double[] { 0.37, -0.81, 0.55 };
        var rotM = Rodrigues(cRot);
        var xwR = RotateField(xw, rotM, dimG);
        var xtR = RotateField(xt, rotM, dimG);
        var upsR = UpsilonOf(op, isControl, xwR, xtR, out var fcR);
        var o1b = new double[T]; var o2b = new double[T];
        SliceSums(fcR, o1b, upsR, o2b);
        var gR = op.ComputeJointGradient(xwR, xtR, mass);
        double rotResid = RelDiff(g0.Objective, gR.Objective);
        for (int s = 0; s < T; s++)
        {
            rotResid = System.Math.Max(rotResid, RelDiff(o1a[s], o1b[s]));
            rotResid = System.Math.Max(rotResid, RelDiff(o2a[s], o2b[s]));
        }

        // (c) Identity theta-independence (control member only).
        double identityThetaGradNorm = 0.0, identityThetaSDiff = 0.0;
        if (isControl)
        {
            var xtRand = RandomVector(rng, nTheta, 0.7);
            var gT = op.ComputeJointGradient(xw, xtRand, mass);
            identityThetaGradNorm = Norm(gT.GradTheta);
            identityThetaSDiff = RelDiff(gT.Objective, g0.Objective);
        }

        // (d) Rotation-coordinate batteries: Rodrigues == exp(ad_theta) series;
        //     Log(Exp) round trip; S_B branch equivalence at |theta| > pi.
        double expLogResid = 0.0;
        {
            var rr = new Random(RngSeed + 999);
            for (int rep = 0; rep < 8; rep++)
            {
                var th = new double[] { 2.4 * (rr.NextDouble() - 0.5) * 2, 2.4 * (rr.NextDouble() - 0.5) * 2, 2.4 * (rr.NextDouble() - 0.5) * 2 };
                var rod = Rodrigues(th);
                var ser = MatrixExpSeries(CrossMatrix(th));
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        expLogResid = System.Math.Max(expLogResid, System.Math.Abs(rod[i, j] - ser[i, j]));
                var back = LogRotation(rod);
                var rod2 = Rodrigues(back);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        expLogResid = System.Math.Max(expLogResid, System.Math.Abs(rod[i, j] - rod2[i, j]));
            }
            if (!isControl)
            {
                // Branch equivalence through the operator: theta vs Log(Exp(theta)).
                var thBig = RandomVector(rng, nTheta, 2.5);
                var thWrapped = new double[nTheta];
                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    var th3 = new[] { thBig[v * 3], thBig[v * 3 + 1], thBig[v * 3 + 2] };
                    var w = LogRotation(Rodrigues(th3));
                    thWrapped[v * 3] = w[0]; thWrapped[v * 3 + 1] = w[1]; thWrapped[v * 3 + 2] = w[2];
                }
                var gA = op.ComputeJointGradient(xw, thBig, mass);
                var gB = op.ComputeJointGradient(xw, thWrapped, mass);
                expLogResid = System.Math.Max(expLogResid, RelDiff(gA.Objective, gB.Objective));
            }
        }

        // (e) Momentum blocks of the omega Hessian at (0, 0). -------------------
        var zeroOmega = new double[nOmega];
        var zeroTheta = new double[nTheta];
        var cols = new double[blockDim][];
        double hvThetaMax = 0.0;
        for (int ty = 0; ty < 15; ty++)
            for (int l = 0; l < dimG; l++)
            {
                var vO = new double[nOmega]; var vT = new double[nTheta];
                int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
                vO[e0 * dimG + l] = oSign[e0];
                var (hvO, hvT) = op.JointHessianVectorProduct(zeroOmega, zeroTheta, vO, vT, mass, HvStep);
                hvThetaMax = System.Math.Max(hvThetaMax, Norm(hvT));
                var col = new double[nOmega];
                for (int e = 0; e < mesh.EdgeCount; e++)
                    for (int ll = 0; ll < dimG; ll++)
                        col[e * dimG + ll] = oSign[e] * hvO[e * dimG + ll];
                cols[ty * dimG + l] = col;
            }

        double KernH(int[] w, int tyR, int lR, int colIdx)
        {
            int v = vertexAt[(w[0], w[1], w[2], w[3])];
            return cols[colIdx][edgeAt[v, tyR] * dimG + lR];
        }

        var blockEvals = new double[nK][];
        var blockEvecsRe = new double[nK][,];
        var blockEvecsIm = new double[nK][,];
        double blockResidualMax = 0.0;
        double maxAbsEig = 0.0;
        for (int ki = 0; ki < nK; ki++)
        {
            var k = kList[ki];
            var h = new Cx[blockDim, blockDim];
            foreach (var w in kList)
            {
                double phase = -2.0 * System.Math.PI * (k[0] * w[0] + k[1] * w[1] + k[2] * w[2] + k[3] * w[3]) / n;
                var ph = new Cx(System.Math.Cos(phase), System.Math.Sin(phase));
                for (int col = 0; col < blockDim; col++)
                    for (int tyR = 0; tyR < 15; tyR++)
                        for (int lR = 0; lR < dimG; lR++)
                            h[tyR * dimG + lR, col] += ph * KernH(w, tyR, lR, col);
            }
            // Hermitize (FD noise).
            for (int i = 0; i < blockDim; i++)
                for (int j = i; j < blockDim; j++)
                {
                    var s = 0.5 * (h[i, j] + Cx.Conjugate(h[j, i]));
                    h[i, j] = s; h[j, i] = Cx.Conjugate(s);
                }
            var (evals, vecRe, vecIm, resid) = HermitianEigen(h);
            blockEvals[ki] = evals;
            blockEvecsRe[ki] = vecRe;
            blockEvecsIm[ki] = vecIm;
            blockResidualMax = System.Math.Max(blockResidualMax, resid);
            foreach (double ev in evals) maxAbsEig = System.Math.Max(maxAbsEig, System.Math.Abs(ev));
        }
        double zeroTol = System.Math.Max(ZeroModeAbsFloor, ZeroModeRelTol * maxAbsEig);
        int nZero = 0, nNeg = 0, nPos = 0;
        double minEig = double.MaxValue;
        foreach (var evs in blockEvals)
            foreach (double ev in evs)
            {
                minEig = System.Math.Min(minEig, ev);
                if (ev > zeroTol) nPos++;
                else if (ev < -zeroTol) nNeg++;
                else nZero++;
            }

        // (f) Plane-wave Hv battery: H (real space, FD) on Re(e^{ikx} u) vs blocks.
        double planeWaveResid;
        {
            var rk = kList[1 + rng.Next(nK - 1)];
            int rki = KeyOfK(rk);
            var uRe = new double[blockDim]; var uIm = new double[blockDim];
            for (int i = 0; i < blockDim; i++) { uRe[i] = rng.NextDouble() - 0.5; uIm[i] = rng.NextDouble() - 0.5; }
            var vO = new double[nOmega];
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                var x = coords[edgeBase[e]];
                double ph = 2.0 * System.Math.PI * (rk[0] * x[0] + rk[1] * x[1] + rk[2] * x[2] + rk[3] * x[3]) / n;
                double cph = System.Math.Cos(ph), sph = System.Math.Sin(ph);
                for (int l = 0; l < dimG; l++)
                {
                    int a = edgeType[e] * dimG + l;
                    vO[e * dimG + l] = oSign[e] * (cph * uRe[a] - sph * uIm[a]);
                }
            }
            var (hvO, _) = op.JointHessianVectorProduct(zeroOmega, zeroTheta, vO, new double[nTheta], mass, HvStep);
            // Expected: Re(e^{ikx} H(k) u).
            var hk = BlockTimesVector(blockEvals[rki], blockEvecsRe[rki], blockEvecsIm[rki], uRe, uIm);
            double num = 0, den = 0;
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                var x = coords[edgeBase[e]];
                double ph = 2.0 * System.Math.PI * (rk[0] * x[0] + rk[1] * x[1] + rk[2] * x[2] + rk[3] * x[3]) / n;
                double cph = System.Math.Cos(ph), sph = System.Math.Sin(ph);
                for (int l = 0; l < dimG; l++)
                {
                    int a = edgeType[e] * dimG + l;
                    double expect = cph * hk.Re[a] - sph * hk.Im[a];
                    double got = oSign[e] * hvO[e * dimG + l];
                    num += (got - expect) * (got - expect);
                    den += expect * expect;
                }
            }
            planeWaveResid = System.Math.Sqrt(num) / System.Math.Max(1e-30, System.Math.Sqrt(den));
        }

        // (g) Interpolator linearization kernels G_F, G_Ups (central difference:
        //     exact -- both maps are linear+quadratic in omega at theta=0).
        var gfCols = new double[blockDim][]; // face-value columns for F map
        var guCols = new double[blockDim][]; // face-value columns for Upsilon map
        for (int ty = 0; ty < 15; ty++)
            for (int l = 0; l < dimG; l++)
            {
                var vO = new double[nOmega];
                int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
                vO[e0 * dimG + l] = oSign[e0];
                var wp = Scale(vO, LinStep);
                var wm = Scale(vO, -LinStep);
                var upsP = UpsilonOf(op, isControl, wp, zeroTheta, out var fP);
                var upsM = UpsilonOf(op, isControl, wm, zeroTheta, out var fM);
                var gf = new double[mesh.FaceCount * dimG];
                var gu = new double[mesh.FaceCount * dimG];
                for (int i = 0; i < gf.Length; i++)
                {
                    gf[i] = (fP[i] - fM[i]) / (2 * LinStep);
                    gu[i] = (upsP[i] - upsM[i]) / (2 * LinStep);
                }
                gfCols[ty * dimG + l] = gf;
                guCols[ty * dimG + l] = gu;
            }

        // Momentum kernels G_X(k): fdim x blockDim complex.
        Cx[][,] GxOfK(double[][] gxCols)
        {
            var result = new Cx[nK][,];
            for (int ki = 0; ki < nK; ki++)
            {
                var k = kList[ki];
                var gk = new Cx[fdim, blockDim];
                foreach (var w in kList)
                {
                    double phase = -2.0 * System.Math.PI * (k[0] * w[0] + k[1] * w[1] + k[2] * w[2] + k[3] * w[3]) / n;
                    var ph = new Cx(System.Math.Cos(phase), System.Math.Sin(phase));
                    int vtx = vertexAt[(w[0], w[1], w[2], w[3])];
                    for (int ft = 0; ft < faceTypeCount; ft++)
                    {
                        int fIdx = faceAt[vtx, ft];
                        for (int lr = 0; lr < dimG; lr++)
                        {
                            int row = ft * dimG + lr;
                            for (int col = 0; col < blockDim; col++)
                            {
                                double cv = gxCols[col][fIdx * dimG + lr];
                                if (cv != 0.0) gk[row, col] += ph * cv;
                            }
                        }
                    }
                }
                result[ki] = gk;
            }
            return result;
        }
        var gFk = GxOfK(gfCols);
        var gUk = GxOfK(guCols);

        // (h) Covariance tables Cov_X(dx) at beta = 1 and analytic correlators.
        double covImagMaxShared = 0.0;
        double[][,] CovOf(Cx[][,] gk)
        {
            // B(k) = G(k) P(k) G(k)^dagger with P = pseudo-inverse over positive modes.
            var covRe = new double[nK][,]; // indexed by displacement key
            var accRe = new double[nK][,];
            var accIm = new double[nK][,];
            for (int di = 0; di < nK; di++) { accRe[di] = new double[fdim, fdim]; accIm[di] = new double[fdim, fdim]; }
            for (int ki = 0; ki < nK; ki++)
            {
                var evals = blockEvals[ki];
                var vRe = blockEvecsRe[ki]; var vIm = blockEvecsIm[ki];
                var g = gk[ki];
                // W = G V  (fdim x blockDim), then B = sum_a (1/lambda_a) W_a W_a^dagger over positive modes.
                var bRe = new double[fdim, fdim]; var bIm = new double[fdim, fdim];
                var waRe = new double[fdim]; var waIm = new double[fdim];
                for (int a = 0; a < blockDim; a++)
                {
                    if (evals[a] <= zeroTol) continue;
                    for (int r = 0; r < fdim; r++)
                    {
                        double sr = 0, si = 0;
                        for (int c = 0; c < blockDim; c++)
                        {
                            double gr = g[r, c].Real, gi = g[r, c].Imaginary;
                            sr += gr * vRe[c, a] - gi * vIm[c, a];
                            si += gr * vIm[c, a] + gi * vRe[c, a];
                        }
                        waRe[r] = sr; waIm[r] = si;
                    }
                    double inv = 1.0 / evals[a];
                    for (int r = 0; r < fdim; r++)
                        for (int c2 = 0; c2 < fdim; c2++)
                        {
                            bRe[r, c2] += inv * (waRe[r] * waRe[c2] + waIm[r] * waIm[c2]);
                            bIm[r, c2] += inv * (waIm[r] * waRe[c2] - waRe[r] * waIm[c2]);
                        }
                }
                // Accumulate DFT: Cov(dx) += e^{+2 pi i k.dx / n} B(k) / V.
                var k = kList[ki];
                for (int di = 0; di < nK; di++)
                {
                    var dx = kList[di];
                    double phase = 2.0 * System.Math.PI * (k[0] * dx[0] + k[1] * dx[1] + k[2] * dx[2] + k[3] * dx[3]) / n;
                    double cph = System.Math.Cos(phase) / volume, sph = System.Math.Sin(phase) / volume;
                    var ar = accRe[di]; var ai = accIm[di];
                    for (int r = 0; r < fdim; r++)
                        for (int c2 = 0; c2 < fdim; c2++)
                        {
                            ar[r, c2] += cph * bRe[r, c2] - sph * bIm[r, c2];
                            ai[r, c2] += sph * bRe[r, c2] + cph * bIm[r, c2];
                        }
                }
            }
            double imagMax = 0.0;
            for (int di = 0; di < nK; di++)
            {
                covRe[di] = accRe[di];
                for (int r = 0; r < fdim; r++)
                    for (int c2 = 0; c2 < fdim; c2++)
                        imagMax = System.Math.Max(imagMax, System.Math.Abs(accIm[di][r, c2]));
            }
            covImagMaxShared = System.Math.Max(covImagMaxShared, imagMax);
            return covRe;
        }
        var covO1 = CovOf(gFk);
        var covO2 = CovOf(gUk);
        double covImagMax = covImagMaxShared;

        // Analytic connected correlators at beta = 1 (scale by 1/beta^2 per column)
        // and analytic slice means at beta = 1 (scale by 1/beta).
        double[] AnalyticC(double[][,] cov)
        {
            var c = new double[T];
            for (int t = 0; t < T; t++)
            {
                double sum = 0.0;
                foreach (var dx in kList)
                {
                    if (dx[TimeAxis] != t) continue;
                    var m = cov[KeyOfK(dx)];
                    for (int ftA = 0; ftA < faceTypeCount; ftA++)
                        for (int ftB = 0; ftB < faceTypeCount; ftB++)
                        {
                            // Tr(K M K M^T) over the 3x3 lie block (K = lieGram).
                            for (int l1 = 0; l1 < dimG; l1++)
                                for (int l2 = 0; l2 < dimG; l2++)
                                    for (int l3 = 0; l3 < dimG; l3++)
                                        for (int l4 = 0; l4 < dimG; l4++)
                                            sum += lieGram[l1, l2] * m[ftA * dimG + l2, ftB * dimG + l3]
                                                 * lieGram[l3, l4] * m[ftA * dimG + l1, ftB * dimG + l4];
                        }
                }
                c[t] = 2.0 * (volume / T) * sum; // n^3 spatial translations
            }
            return c;
        }
        double AnalyticMean(double[][,] cov)
        {
            var m0 = cov[KeyOfK(new[] { 0, 0, 0, 0 })];
            double sum = 0.0;
            for (int ft = 0; ft < faceTypeCount; ft++)
                for (int l1 = 0; l1 < dimG; l1++)
                    for (int l2 = 0; l2 < dimG; l2++)
                        sum += lieGram[l1, l2] * m0[ft * dimG + l1, ft * dimG + l2];
            return (volume / T) * sum;
        }
        var c1An = AnalyticC(covO1);
        var c2An = AnalyticC(covO2);
        double mean1An = AnalyticMean(covO1);
        double mean2An = AnalyticMean(covO2);

        // (i) Zero-mode real-space basis (stored gauge) for the free columns.
        var zBasis = new List<double[]>();
        {
            var seen = new HashSet<int>();
            for (int ki = 0; ki < nK; ki++)
            {
                var k = kList[ki];
                var kNeg = new[] { Mod(-k[0], n), Mod(-k[1], n), Mod(-k[2], n), Mod(-k[3], n) };
                int keyNeg = KeyOfK(kNeg);
                if (seen.Contains(ki)) continue;
                seen.Add(ki); seen.Add(keyNeg);
                bool selfConj = ki == keyNeg;
                var evals = blockEvals[ki];
                var vRe = blockEvecsRe[ki]; var vIm = blockEvecsIm[ki];
                for (int a = 0; a < blockDim; a++)
                {
                    if (System.Math.Abs(evals[a]) > zeroTol) continue;
                    if (selfConj)
                    {
                        var z = new double[nOmega];
                        for (int e = 0; e < mesh.EdgeCount; e++)
                        {
                            var x = coords[edgeBase[e]];
                            double ph = 2.0 * System.Math.PI * (k[0] * x[0] + k[1] * x[1] + k[2] * x[2] + k[3] * x[3]) / n;
                            double cph = System.Math.Cos(ph), sph = System.Math.Sin(ph);
                            for (int l = 0; l < dimG; l++)
                            {
                                int idx = edgeType[e] * dimG + l;
                                z[e * dimG + l] = oSign[e] * (cph * vRe[idx, a] - sph * vIm[idx, a]) / System.Math.Sqrt(volume);
                            }
                        }
                        zBasis.Add(z);
                    }
                    else
                    {
                        var z1 = new double[nOmega]; var z2 = new double[nOmega];
                        double s2 = System.Math.Sqrt(2.0 / volume);
                        for (int e = 0; e < mesh.EdgeCount; e++)
                        {
                            var x = coords[edgeBase[e]];
                            double ph = 2.0 * System.Math.PI * (k[0] * x[0] + k[1] * x[1] + k[2] * x[2] + k[3] * x[3]) / n;
                            double cph = System.Math.Cos(ph), sph = System.Math.Sin(ph);
                            for (int l = 0; l < dimG; l++)
                            {
                                int idx = edgeType[e] * dimG + l;
                                double re = vRe[idx, a], im = vIm[idx, a];
                                z1[e * dimG + l] = oSign[e] * s2 * (cph * re - sph * im);
                                z2[e * dimG + l] = oSign[e] * s2 * (sph * re + cph * im);
                            }
                        }
                        zBasis.Add(z1); zBasis.Add(z2);
                    }
                }
            }
        }
        // Orthonormality + H z ~ 0 spot checks.
        double zGramResid = 0.0, zHzResid = 0.0;
        {
            var rr = new Random(RngSeed + 4242);
            for (int rep = 0; rep < 12 && zBasis.Count > 0; rep++)
            {
                int i = rr.Next(zBasis.Count), j = rr.Next(zBasis.Count);
                double dot = Dot(zBasis[i], zBasis[j]);
                zGramResid = System.Math.Max(zGramResid, System.Math.Abs(dot - (i == j ? 1.0 : 0.0)));
            }
            for (int rep = 0; rep < 3 && zBasis.Count > 0; rep++)
            {
                var z = zBasis[rr.Next(zBasis.Count)];
                var (hvO, _) = op.JointHessianVectorProduct(zeroOmega, zeroTheta, z, new double[nTheta], mass, HvStep);
                zHzResid = System.Math.Max(zHzResid, Norm(hvO) / System.Math.Max(1e-30, maxAbsEig));
            }
        }

        memberAnalytics[mi] = new AnalyticData
        {
            BlockEvals = blockEvals,
            BlockEvecsRe = blockEvecsRe,
            BlockEvecsIm = blockEvecsIm,
            ZeroTol = zeroTol,
            MaxAbsEig = maxAbsEig,
            NPos = nPos,
            NZero = nZero,
            NNeg = nNeg,
            MinEig = minEig,
            CovO1 = covO1,
            CovO2 = covO2,
            C1An = c1An,
            C2An = c2An,
            Mean1An = mean1An,
            Mean2An = mean2An,
            ZBasis = zBasis,
        };
        memberBatteries[mi] = new MemberBattery(
            objConsistency, rotResid, identityThetaGradNorm, identityThetaSDiff, expLogResid,
            hvThetaMax, blockResidualMax, planeWaveResid, covImagMax, zGramResid, zHzResid);

        lock (consoleLock)
            Console.WriteLine($"[n={n}] {memberNames[mi]}: blocks done. modes(p/z/n)={nPos}/{nZero}/{nNeg} minEig={minEig:E2} " +
                $"objCons={objConsistency:E2} rotInv={rotResid:E2} expLog={expLogResid:E2} planeWave={planeWaveResid:E2} blockResid={blockResidualMax:E2}");
    }

    // --- Exact-Gaussian direct-sampling pipeline battery (per member). --------
    var gaussSim = new GaussSimResult[2];
    for (int mi = 0; mi < 2; mi++)
    {
        var an = memberAnalytics[mi];
        bool isControl = memberIsControl[mi];
        var op = MakeOp(isControl);
        var rng = new Random(RngSeed + 5001 + mi);
        // Half-set of momenta with real-ified self-conjugate eigenvectors.
        var halfPairs = new List<(int Ki, bool SelfConj)>();
        {
            var seen = new HashSet<int>();
            for (int ki = 0; ki < nK; ki++)
            {
                if (seen.Contains(ki)) continue;
                var k = kList[ki];
                int keyNeg = KeyOfK(new[] { Mod(-k[0], n), Mod(-k[1], n), Mod(-k[2], n), Mod(-k[3], n) });
                seen.Add(ki); seen.Add(keyNeg);
                halfPairs.Add((ki, ki == keyNeg));
            }
        }
        double selfConjImagMax = 0.0;
        var o1Series = new double[gaussSimSamples][];
        var o2Series = new double[gaussSimSamples][];
        var gaussA2O1Series = phase456Mode ? new double[gaussSimSamples][] : null;
        var gaussA2O2Series = phase456Mode ? new double[gaussSimSamples][] : null;
        var gaussKO1ReSeries = phase456Mode ? Enumerable.Range(0, 3).Select(_ => new double[gaussSimSamples][]).ToArray() : null;
        var gaussKO1ImSeries = phase456Mode ? Enumerable.Range(0, 3).Select(_ => new double[gaussSimSamples][]).ToArray() : null;
        var gaussPhiSeries = phase456Mode ? new double[gaussSimSamples] : null;
        var phiRe = new double[blockDim]; var phiIm = new double[blockDim];
        var lat = new double[volume, blockDim]; // synthesized lattice field per sample
        var omegaBuf = new double[nOmega];
        for (int s = 0; s < gaussSimSamples; s++)
        {
            Array.Clear(lat);
            foreach (var (ki, selfConj) in halfPairs)
            {
                var k = kList[ki];
                var evals = an.BlockEvals[ki];
                var vRe = an.BlockEvecsRe[ki]; var vIm = an.BlockEvecsIm[ki];
                Array.Clear(phiRe); Array.Clear(phiIm);
                for (int a = 0; a < blockDim; a++)
                {
                    if (evals[a] <= an.ZeroTol) continue;
                    double sd = 1.0 / System.Math.Sqrt(evals[a]); // beta = 1
                    if (selfConj)
                    {
                        // Real-ify the eigenvector by phase rotation.
                        double bestAbs = -1; int best = 0;
                        for (int i = 0; i < blockDim; i++)
                        {
                            double m2 = vRe[i, a] * vRe[i, a] + vIm[i, a] * vIm[i, a];
                            if (m2 > bestAbs) { bestAbs = m2; best = i; }
                        }
                        double mag = System.Math.Sqrt(System.Math.Max(bestAbs, 1e-300));
                        double pr = vRe[best, a] / mag, pi = -vIm[best, a] / mag;
                        double xi = sd * Gauss(rng);
                        for (int i = 0; i < blockDim; i++)
                        {
                            double re = pr * vRe[i, a] - pi * vIm[i, a];
                            double im = pr * vIm[i, a] + pi * vRe[i, a];
                            selfConjImagMax = System.Math.Max(selfConjImagMax, System.Math.Abs(im));
                            phiRe[i] += xi * re;
                        }
                    }
                    else
                    {
                        double zr = sd * Gauss(rng) / System.Math.Sqrt(2.0);
                        double zi = sd * Gauss(rng) / System.Math.Sqrt(2.0);
                        for (int i = 0; i < blockDim; i++)
                        {
                            phiRe[i] += zr * vRe[i, a] - zi * vIm[i, a];
                            phiIm[i] += zr * vIm[i, a] + zi * vRe[i, a];
                        }
                    }
                }
                double normFac = (selfConj ? 1.0 : 2.0) / System.Math.Sqrt(volume);
                for (int v = 0; v < volume; v++)
                {
                    var x = coords[v];
                    double ph = 2.0 * System.Math.PI * (k[0] * x[0] + k[1] * x[1] + k[2] * x[2] + k[3] * x[3]) / n;
                    double cph = System.Math.Cos(ph), sph = System.Math.Sin(ph);
                    for (int i = 0; i < blockDim; i++)
                        lat[v, i] += normFac * (cph * phiRe[i] - sph * phiIm[i]);
                }
            }
            for (int e = 0; e < mesh.EdgeCount; e++)
                for (int l = 0; l < dimG; l++)
                    omegaBuf[e * dimG + l] = oSign[e] * lat[edgeBase[e], edgeType[e] * dimG + l];
            // Full measurement pipeline on the exact Gaussian sample --
            // QUADRATIC truncation: measure the linearized F and Upsilon so the
            // comparison to the analytic Gaussian formula is exact (the exact
            // maps add the anharmonic pieces the analytic formula omits; those
            // are the large-beta suppressed terms in the sampler gate).
            var upsLin = LinearizedFaceMap(op, isControl, omegaBuf);
            var o1 = new double[T]; var o2 = new double[T];
            SliceSums(upsLin.FLin, o1, upsLin.ULin, o2);
            o1Series[s] = o1; o2Series[s] = o2;
            if (phase456Mode)
            {
                var m456 = MeasurePhase456(upsLin.FLin, upsLin.ULin, omegaBuf);
                gaussA2O1Series![s] = m456.A2O1;
                gaussA2O2Series![s] = m456.A2O2;
                for (int axis = 0; axis < 3; axis++)
                {
                    gaussKO1ReSeries![axis][s] = m456.KO1Re[axis];
                    gaussKO1ImSeries![axis][s] = m456.KO1Im[axis];
                }
                gaussPhiSeries![s] = m456.Phi;
            }
        }
        var c1Sim = ConnectedCorrelator(o1Series, T, JackknifeBlocks);
        var c2Sim = ConnectedCorrelator(o2Series, T, JackknifeBlocks);
        double worstZ1 = 0, worstZ2 = 0;
        for (int t = 0; t < T; t++)
        {
            worstZ1 = System.Math.Max(worstZ1, System.Math.Abs(c1Sim.C[t] - an.C1An[t]) / System.Math.Max(1e-30, c1Sim.Sigma[t]));
            worstZ2 = System.Math.Max(worstZ2, System.Math.Abs(c2Sim.C[t] - an.C2An[t]) / System.Math.Max(1e-30, c2Sim.Sigma[t]));
        }
        Phase456GaussianControl? phase456GaussianControl = null;
        if (phase456Mode)
        {
            var c12Sim = CrossConnectedCorrelator(o1Series, o2Series, T, JackknifeBlocks);
            var gevpSim = GevpFromCorrelators(c1Sim, c2Sim, c12Sim, T, JackknifeBlocks);
            var a2c1Sim = ConnectedCorrelator(gaussA2O1Series!, T, JackknifeBlocks);
            var a2c2Sim = ConnectedCorrelator(gaussA2O2Series!, T, JackknifeBlocks);
            var kc1Sim = ComplexConnectedCorrelator(gaussKO1ReSeries!, gaussKO1ImSeries!, T, JackknifeBlocks);
            phase456GaussianControl = new Phase456GaussianControl(
                gaussSimSamples, gevpSim,
                a2c1Sim, a2c2Sim, CoshEffectiveMasses(a2c1Sim, T, JackknifeBlocks), CoshEffectiveMasses(a2c2Sim, T, JackknifeBlocks),
                kc1Sim, CoshEffectiveMasses(kc1Sim, T, JackknifeBlocks),
                BinderStatistics(gaussPhiSeries!, volume, JackknifeBlocks));
        }
        gaussSim[mi] = new GaussSimResult(worstZ1, worstZ2, selfConjImagMax, c1Sim.C, c2Sim.C, phase456GaussianControl);
        lock (consoleLock)
            Console.WriteLine($"[n={n}] {memberNames[mi]}: gaussian-sim battery worstZ(O1)={worstZ1:F2} worstZ(O2)={worstZ2:F2}");
    }

    // Linearized face maps used by the Gaussian battery.
    (double[] FLin, double[] ULin) LinearizedFaceMap(EinsteinianShiabOperator op, bool isControl, double[] omega)
    {
        var zt = new double[nTheta];
        var wp = Scale(omega, LinStep);
        var wm = Scale(omega, -LinStep);
        var upsP = UpsilonOf(op, isControl, wp, zt, out var fP);
        var upsM = UpsilonOf(op, isControl, wm, zt, out var fM);
        var fl = new double[mesh.FaceCount * dimG];
        var ul = new double[mesh.FaceCount * dimG];
        for (int i = 0; i < fl.Length; i++)
        {
            fl[i] = (fP[i] - fM[i]) / (2 * LinStep);
            ul[i] = (upsP[i] - upsM[i]) / (2 * LinStep);
        }
        return (fl, ul);
    }

    // --- Column specs. ---------------------------------------------------------
    var specs = new List<ColumnSpec>
    {
        new(0, "identity", true, betaProd, "production", trajProd, warmTraj, false, false, RngSeed + 11),
        new(1, "identity", true, betaFree, "free-field-control", trajCtrl, warmTraj, true, false, RngSeed + 12),
        new(2, "sd2-id0/c0.5", false, betaProd, "production", trajProd, warmTraj, false, true, RngSeed + 13),
        new(3, "sd2-id0/c0.5", false, betaMid, "higher-beta-control", trajCtrl, warmTraj, false, true, RngSeed + 14),
        new(4, "sd2-id0/c0.5", false, betaFree, "free-field-control", trajCtrl, warmTraj, true, false, RngSeed + 15),
    };

    var columnResults = new ConcurrentDictionary<int, ColumnResult>();

    void RunColumn(ColumnSpec spec)
    {
        int mi = spec.IsControlMember ? 0 : 1;
        var an = memberAnalytics[mi];
        var op = MakeOp(spec.IsControlMember);
        var mass = new CpuMassMatrix(mesh, algebra);
        var rng = new Random(spec.Seed);
        double GaussL() => Gauss(rng);

        var zList = spec.ProjectZeroModes ? an.ZBasis : new List<double[]>();
        void Project(double[] x)
        {
            foreach (var z in zList)
            {
                double d = Dot(z, x);
                for (int i = 0; i < nOmega; i++) x[i] -= d * z[i];
            }
        }
        int nDof = nOmega - (spec.ProjectZeroModes ? zList.Count : 0);

        var omega = new double[nOmega];
        for (int i = 0; i < nOmega; i++) omega[i] = 0.05 * GaussL() / System.Math.Sqrt(spec.Beta);
        if (spec.ProjectZeroModes) Project(omega);
        var theta = new double[nTheta];

        double eps = 0.08 / System.Math.Sqrt(spec.Beta);
        double sigTheta = 0.08;

        var grad = new double[nOmega];
        void Force(double[] w, double[] th, double[] force, out double s)
        {
            var g = op.ComputeJointGradient(w, th, mass);
            s = g.Objective;
            Array.Copy(g.GradOmega, grad, nOmega);
            for (int i = 0; i < nOmega; i++) force[i] = -spec.Beta * g.GradOmega[i];
            if (spec.ProjectZeroModes) Project(force);
        }

        var p = new double[nOmega];
        var force = new double[nOmega];
        var w0 = new double[nOmega];
        long nAcc = 0, nTraj = 0;
        double lastDH = 0.0;

        bool Trajectory()
        {
            Array.Copy(omega, w0, nOmega);
            for (int i = 0; i < nOmega; i++) p[i] = GaussL();
            if (spec.ProjectZeroModes) Project(p);
            Force(omega, theta, force, out double s0);
            double h0 = spec.Beta * s0;
            for (int i = 0; i < nOmega; i++) h0 += 0.5 * p[i] * p[i];
            for (int l = 0; l < nLeap; l++)
            {
                for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
                for (int i = 0; i < nOmega; i++) omega[i] += eps * p[i];
                Force(omega, theta, force, out _);
                for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
            }
            Force(omega, theta, force, out double s1);
            double h1 = spec.Beta * s1;
            for (int i = 0; i < nOmega; i++) h1 += 0.5 * p[i] * p[i];
            lastDH = h1 - h0;
            nTraj++;
            if (lastDH < 0 || rng.NextDouble() < System.Math.Exp(-lastDH))
            {
                nAcc++;
                return true;
            }
            Array.Copy(w0, omega, nOmega);
            Force(omega, theta, force, out _);
            return false;
        }

        // theta machinery (Haar-invariant rotation Metropolis).
        FieldTensor? cachedFT = null;
        FieldTensor? cachedConnT = null;
        bool curvCacheFresh = false;
        void RefreshCurvatureCacheIfNeeded()
        {
            if (curvCacheFresh) return;
            var conn = new ConnectionField(mesh, algebra, omega);
            var curv = CurvatureAssembler.Assemble(conn);
            cachedFT = curv.ToFieldTensor();
            cachedConnT = conn.ToFieldTensor();
            curvCacheFresh = true;
        }
        double SOfTheta(double[] th)
        {
            var ups = op.EvaluateWithTheta(cachedFT!, cachedConnT!, th, manifest, geometry).Coefficients;
            double s = 0.0;
            for (int f = 0; f < mesh.FaceCount; f++)
                for (int l = 0; l < dimG; l++)
                    for (int m2 = 0; m2 < dimG; m2++)
                        s += lieGram[l, m2] * ups[f * dimG + l] * ups[f * dimG + m2];
            return 0.5 * s; // uniform unit face weights (CpuMassMatrix default)
        }
        long thAcc = 0, thTot = 0, thSiteAcc = 0, thSiteTot = 0;
        void ThetaCollectiveMove()
        {
            RefreshCurvatureCacheIfNeeded();
            double sCur = SOfTheta(theta);
            var prop = new double[nTheta];
            for (int v = 0; v < mesh.VertexCount; v++)
            {
                var dlt = new[] { sigTheta * GaussL(), sigTheta * GaussL(), sigTheta * GaussL() };
                var rNew = MatMul3(Rodrigues(dlt), Rodrigues(new[] { theta[v * 3], theta[v * 3 + 1], theta[v * 3 + 2] }));
                var w = LogRotation(rNew);
                prop[v * 3] = w[0]; prop[v * 3 + 1] = w[1]; prop[v * 3 + 2] = w[2];
            }
            double sNew = SOfTheta(prop);
            thTot++;
            double dS = spec.Beta * (sNew - sCur);
            if (dS < 0 || rng.NextDouble() < System.Math.Exp(-dS))
            {
                Array.Copy(prop, theta, nTheta);
                thAcc++;
            }
        }
        void ThetaSiteSweep()
        {
            RefreshCurvatureCacheIfNeeded();
            double sCur = SOfTheta(theta);
            for (int v = 0; v < mesh.VertexCount; v++)
            {
                var dlt = new[] { ThetaSiteSigma * GaussL(), ThetaSiteSigma * GaussL(), ThetaSiteSigma * GaussL() };
                var rNew = MatMul3(Rodrigues(dlt), Rodrigues(new[] { theta[v * 3], theta[v * 3 + 1], theta[v * 3 + 2] }));
                var w = LogRotation(rNew);
                var old = new[] { theta[v * 3], theta[v * 3 + 1], theta[v * 3 + 2] };
                theta[v * 3] = w[0]; theta[v * 3 + 1] = w[1]; theta[v * 3 + 2] = w[2];
                double sNew = SOfTheta(theta);
                thSiteTot++;
                double dS = spec.Beta * (sNew - sCur);
                if (dS < 0 || rng.NextDouble() < System.Math.Exp(-dS)) { thSiteAcc++; sCur = sNew; }
                else { theta[v * 3] = old[0]; theta[v * 3 + 1] = old[1]; theta[v * 3 + 2] = old[2]; }
            }
        }

        // Warmup with eps / sigma tuning.
        long accWindow = 0, trajWindow = 0, thAccW = 0, thTotW = 0;
        for (int it = 0; it < spec.NWarm; it++)
        {
            curvCacheFresh = false;
            bool acc = Trajectory();
            if (acc) accWindow++;
            trajWindow++;
            if (spec.SampleTheta)
            {
                curvCacheFresh = false;
                long a0 = thAcc, t0 = thTot;
                for (int r = 0; r < ThetaMovesPerTraj; r++) ThetaCollectiveMove();
                thAccW += thAcc - a0; thTotW += thTot - t0;
                if ((it + 1) % ThetaSweepEvery == 0) ThetaSiteSweep();
            }
            if ((it + 1) % 50 == 0)
            {
                double a = (double)accWindow / trajWindow;
                if (a > 0.9) eps *= 1.15; else if (a < 0.6) eps *= 0.8;
                accWindow = 0; trajWindow = 0;
                if (spec.SampleTheta && thTotW > 0)
                {
                    double ta = (double)thAccW / thTotW;
                    if (ta > 0.5) sigTheta *= 1.2; else if (ta < 0.25) sigTheta *= 0.8;
                    thAccW = 0; thTotW = 0;
                }
            }
        }

        // Sampling.
        nAcc = 0; nTraj = 0; thAcc = 0; thTot = 0; thSiteAcc = 0; thSiteTot = 0;
        int nSamp = spec.NTraj;
        var o1Series = new double[nSamp][];
        var o2Series = new double[nSamp][];
        var sSeries = new double[nSamp];
        var expDHSeries = new double[nSamp];
        var virialSeries = new double[nSamp];
        var o1Bar = new double[nSamp];
        var o2Bar = new double[nSamp];
        var phase456A2O1Series = phase456Mode ? new double[nSamp][] : null;
        var phase456A2O2Series = phase456Mode ? new double[nSamp][] : null;
        var phase456KO1ReSeries = phase456Mode ? Enumerable.Range(0, 3).Select(_ => new double[nSamp][]).ToArray() : null;
        var phase456KO1ImSeries = phase456Mode ? Enumerable.Range(0, 3).Select(_ => new double[nSamp][]).ToArray() : null;
        var phase456PhiSeries = phase456Mode ? new double[nSamp] : null;
        int phase456SpatialBlockCount = phase456Mode ? System.Math.Min(JackknifeBlocks, System.Math.Max(2, nSamp / 4)) : 0;
        const int Phase456SpatialMomentumCount = 64;
        var phase456SpatialMeanRe = phase456Mode ? new double[phase456SpatialBlockCount, Phase456SpatialMomentumCount] : null;
        var phase456SpatialMeanIm = phase456Mode ? new double[phase456SpatialBlockCount, Phase456SpatialMomentumCount] : null;
        var phase456SpatialCorr = phase456Mode ? new double[phase456SpatialBlockCount, Phase456SpatialMomentumCount, T] : null;
        var phase456SpatialCounts = phase456Mode ? new long[phase456SpatialBlockCount] : null;

        void AccumulatePhase456PerSiteCorrelators(int sampleIndex, double[] siteO1)
        {
            int block = (int)((long)sampleIndex * phase456SpatialBlockCount / nSamp);
            phase456SpatialCounts![block]++;
            var re = new double[T]; var im = new double[T];
            for (int kx = 0; kx < 4; kx++) for (int ky = 0; ky < 4; ky++) for (int kz = 0; kz < 4; kz++)
            {
                int ki = (kx * 4 + ky) * 4 + kz;
                Array.Clear(re); Array.Clear(im);
                for (int v = 0; v < volume; v++)
                {
                    int q = (kx * coords[v][1] + ky * coords[v][2] + kz * coords[v][3]) & 3;
                    double pr = q switch { 0 => 1.0, 2 => -1.0, _ => 0.0 };
                    double pi = q switch { 1 => -1.0, 3 => 1.0, _ => 0.0 };
                    int tau = coords[v][TimeAxis];
                    re[tau] += pr * siteO1[v]; im[tau] += pi * siteO1[v];
                }
                phase456SpatialMeanRe![block, ki] += re.Sum();
                phase456SpatialMeanIm![block, ki] += im.Sum();
                for (int dt = 0; dt < T; dt++) for (int tau = 0; tau < T; tau++)
                    phase456SpatialCorr![block, ki, dt] += re[tau] * re[(tau + dt) % T] + im[tau] * im[(tau + dt) % T];
            }
        }
        var swCol = Stopwatch.StartNew();
        for (int it = 0; it < nSamp; it++)
        {
            curvCacheFresh = false;
            Trajectory();
            expDHSeries[it] = System.Math.Exp(-lastDH);
            // Virial at the (equilibrium) post-trajectory point, pre-theta-move:
            // grad currently holds the analytic omega gradient at the current state.
            double vir = 0.0;
            if (spec.ProjectZeroModes)
            {
                var gp2 = (double[])grad.Clone();
                Project(gp2);
                for (int i = 0; i < nOmega; i++) vir += omega[i] * spec.Beta * gp2[i];
            }
            else
                for (int i = 0; i < nOmega; i++) vir += omega[i] * spec.Beta * grad[i];
            virialSeries[it] = vir;
            if (spec.SampleTheta)
            {
                curvCacheFresh = false;
                for (int r = 0; r < ThetaMovesPerTraj; r++) ThetaCollectiveMove();
                if ((it + 1) % ThetaSweepEvery == 0) ThetaSiteSweep();
            }
            // Measurement (exact interpolators, current (omega, theta)).
            var ups = UpsilonOf(op, spec.IsControlMember, omega, theta, out var fc);
            var o1 = new double[T]; var o2 = new double[T];
            SliceSums(fc, o1, ups, o2);
            o1Series[it] = o1; o2Series[it] = o2;
            o1Bar[it] = o1.Sum(); o2Bar[it] = o2.Sum();
            sSeries[it] = 0.5 * o2Bar[it]; // = S_B (uniform face weights; battery-verified)
            if (phase456Mode)
            {
                var m456 = MeasurePhase456(fc, ups, omega);
                phase456A2O1Series![it] = m456.A2O1;
                phase456A2O2Series![it] = m456.A2O2;
                for (int axis = 0; axis < 3; axis++)
                {
                    phase456KO1ReSeries![axis][it] = m456.KO1Re[axis];
                    phase456KO1ImSeries![axis][it] = m456.KO1Im[axis];
                }
                phase456PhiSeries![it] = m456.Phi;
                AccumulatePhase456PerSiteCorrelators(it, m456.SiteO1);
            }
        }
        swCol.Stop();
        double msPerTraj = swCol.Elapsed.TotalMilliseconds / nSamp;

        // Statistics.
        var c1 = ConnectedCorrelator(o1Series, T, JackknifeBlocks);
        var c2 = ConnectedCorrelator(o2Series, T, JackknifeBlocks);
        var meff1 = CoshEffectiveMasses(c1, T, JackknifeBlocks);
        var meff2 = CoshEffectiveMasses(c2, T, JackknifeBlocks);
        var (expDHMean, expDHSigma) = JackknifeMean(expDHSeries, JackknifeBlocks);
        var (virMean, virSigma) = JackknifeMean(virialSeries, JackknifeBlocks);
        double tauO1 = TauInt(o1Bar), tauO2 = TauInt(o2Bar), tauS = TauInt(sSeries);
        double tauMax = System.Math.Max(tauO1, System.Math.Max(tauO2, tauS));
        double nEff = nSamp / (2.0 * System.Math.Max(0.5, tauMax));
        var (o1MeanBar, _) = JackknifeMean(o1Bar, JackknifeBlocks);
        var (o2MeanBar, _) = JackknifeMean(o2Bar, JackknifeBlocks);
        var (sMean, _) = JackknifeMean(sSeries, JackknifeBlocks);

        // Gates (2): dH + equipartition.
        bool dhGate = System.Math.Abs(expDHMean - 1.0) <= System.Math.Max(DHSigmaGate * expDHSigma, DHAbsGate);
        bool virGate = System.Math.Abs(virMean - nDof) <= System.Math.Max(VirialSigmaGate * virSigma, VirialRelGate * nDof);
        bool nEffGate = nEff >= NeffMin;

        Phase456ColumnData? phase456Data = null;
        if (phase456Mode)
        {
            var c12 = CrossConnectedCorrelator(o1Series, o2Series, T, JackknifeBlocks);
            var gevp = GevpFromCorrelators(c1, c2, c12, T, JackknifeBlocks);
            var a2c1 = ConnectedCorrelator(phase456A2O1Series!, T, JackknifeBlocks);
            var a2c2 = ConnectedCorrelator(phase456A2O2Series!, T, JackknifeBlocks);
            var kc1 = ComplexConnectedCorrelator(phase456KO1ReSeries!, phase456KO1ImSeries!, T, JackknifeBlocks);
            var binder = BinderStatistics(phase456PhiSeries!, volume, JackknifeBlocks);
            double tauA2 = System.Math.Max(
                TauInt(phase456A2O1Series!.Select(x => x.Sum()).ToArray()),
                TauInt(phase456A2O2Series!.Select(x => x.Sum()).ToArray()));
            double tauK = 0.5;
            for (int axis = 0; axis < 3; axis++)
            {
                tauK = System.Math.Max(tauK, TauInt(phase456KO1ReSeries![axis].Select(x => x.Sum()).ToArray()));
                tauK = System.Math.Max(tauK, TauInt(phase456KO1ImSeries![axis].Select(x => x.Sum()).ToArray()));
            }
            double nEffA2 = nSamp / (2.0 * System.Math.Max(0.5, tauA2));
            double nEffKMin = nSamp / (2.0 * System.Math.Max(0.5, tauK));
            var allSpatialMomenta = SpatialMomentumCorrelators(
                phase456SpatialMeanRe!, phase456SpatialMeanIm!, phase456SpatialCorr!, phase456SpatialCounts!, T);
            double spatialKMinReconstructionResidual = 0.0;
            int[] unitSpatialIndices = { 16, 4, 1 };
            for (int dt = 0; dt < T; dt++)
            {
                double reconstructed = unitSpatialIndices.Average(index => allSpatialMomenta[index].Correlator.C[dt]);
                spatialKMinReconstructionResidual = System.Math.Max(spatialKMinReconstructionResidual,
                    RelDiff(reconstructed, kc1.C[dt]));
                for (int b = 0; b < kc1.Jackknife.Length; b++)
                {
                    double reconstructedJk = unitSpatialIndices.Average(index => allSpatialMomenta[index].Correlator.Jackknife[b][dt]);
                    spatialKMinReconstructionResidual = System.Math.Max(spatialKMinReconstructionResidual,
                        RelDiff(reconstructedJk, kc1.Jackknife[b][dt]));
                }
            }
            phase456Data = new Phase456ColumnData(
                gevp,
                a2c1, a2c2, CoshEffectiveMasses(a2c1, T, JackknifeBlocks), CoshEffectiveMasses(a2c2, T, JackknifeBlocks),
                kc1, CoshEffectiveMasses(kc1, T, JackknifeBlocks), allSpatialMomenta, binder,
                tauA2, nEffA2, tauK, nEffKMin,
                phase456A2Kernel, phase456A2Denominator,
                phase456ProjectorOverlapMax,
                allSpatialMomenta.Length == Phase456SpatialMomentumCount && spatialKMinReconstructionResidual <= 1e-10,
                true,
                spatialKMinReconstructionResidual,
                "all 4^3 spatial-momentum correlators at every Euclidean-time separation are stored with aligned jackknifes; this complete discrete transform is invertible to the per-base-site spatial correlator, while the exact face-type projector is applied before any slice-only loss");
        }

        columnResults[spec.Index] = new ColumnResult
        {
            Spec = spec,
            NDof = nDof,
            NZeroProjected = spec.ProjectZeroModes ? zList.Count : 0,
            Eps = eps,
            SigmaTheta = sigTheta,
            Acceptance = (double)nAcc / System.Math.Max(1, nTraj),
            ThetaCollAcceptance = thTot > 0 ? (double)thAcc / thTot : (double?)null,
            ThetaSiteAcceptance = thSiteTot > 0 ? (double)thSiteAcc / thSiteTot : (double?)null,
            ExpDHMean = expDHMean, ExpDHSigma = expDHSigma, DHGate = dhGate,
            VirialMean = virMean, VirialSigma = virSigma, VirialGate = virGate,
            TauO1 = tauO1, TauO2 = tauO2, TauS = tauS, NEff = nEff, NEffGate = nEffGate,
            SMean = sMean, O1Mean = o1MeanBar / T, O2Mean = o2MeanBar / T,
            C1 = c1, C2 = c2, Meff1 = meff1, Meff2 = meff2,
            Phase456 = phase456Data,
            MsPerTrajectory = msPerTraj,
        };
        lock (consoleLock)
            Console.WriteLine($"[n={n}] column {spec.Member} beta={spec.Beta} ({spec.Kind}): acc={(double)nAcc / System.Math.Max(1, nTraj):F3} " +
                $"eps={eps:E2} <e^-dH>={expDHMean:F4}({expDHSigma:E1}) vir={virMean:F1}/{nDof} tau(O1,O2,S)=({tauO1:F1},{tauO2:F1},{tauS:F1}) " +
                $"Neff={nEff:F0} ms/traj={msPerTraj:F1}");
    }

    // Touch mesh lazy state before parallel section (all init-only arrays; safe).
    _ = mesh.FaceBoundaryEdges.Length; _ = mesh.FaceBoundaryOrientations.Length;

    Parallel.ForEach(specs, new ParallelOptions { MaxDegreeOfParallelism = specs.Count }, RunColumn);

    // --- Window rule, free-field gates, cross-checks, verdicts. ---------------
    int[] informative = Enumerable.Range(0, T).Where(t => (T / 2.0 - t - 1) >= -1e-9 && t >= 0).ToArray();
    int[] window = informative.Where(t => t >= 1).ToArray();
    bool windowIncludesT0 = window.Length == 0;
    if (windowIncludesT0) window = informative.Take(1).ToArray();

    (double? M, double? Sigma, double? Chi2Dof, bool Gate) WindowMass(MeffResult meff)
    {
        var pts = window.Select(t => (T: t, P: meff.Points.FirstOrDefault(p => p.T == t))).ToArray();
        if (pts.Any(x => x.P is null || x.P.M is null || x.P.Sigma is null or <= 0))
            return (null, null, null, false);
        if (pts.Length == 1) return (pts[0].P!.M, pts[0].P!.Sigma, null, true);
        double sw = 0, swm = 0;
        foreach (var x in pts) { double wgt = 1.0 / (x.P!.Sigma!.Value * x.P.Sigma.Value); sw += wgt; swm += wgt * x.P.M!.Value; }
        double mFit = swm / sw;
        double chi2 = 0;
        foreach (var x in pts) { double d = (x.P!.M!.Value - mFit) / x.P.Sigma!.Value; chi2 += d * d; }
        double chi2Dof = chi2 / (pts.Length - 1);
        return (mFit, System.Math.Sqrt(1.0 / sw), chi2Dof, chi2Dof <= PlateauChi2Max);
    }

    // Analytic effective masses from the analytic correlators (per member).
    var analyticMeff = new (double? MO1, double? MO2)[2];
    for (int mi = 0; mi < 2; mi++)
    {
        var an = memberAnalytics[mi];
        double? m1 = null, m2 = null;
        foreach (int t in window)
        {
            m1 ??= SolveCoshMass(an.C1An[t] / an.C1An[Mod(t + 1, T)], t, T);
            m2 ??= SolveCoshMass(an.C2An[t] / an.C2An[Mod(t + 1, T)], t, T);
        }
        analyticMeff[mi] = (m1, m2);
    }

    var freeGates = new List<FreeFieldGate>();
    foreach (var cr in columnResults.Values.Where(c => c.Spec.Kind == "free-field-control").OrderBy(c => c.Spec.Index))
    {
        int mi = cr.Spec.IsControlMember ? 0 : 1;
        var (mw1, sw1, _, _) = WindowMass(cr.Meff1);
        var (mw2, sw2, _, _) = WindowMass(cr.Meff2);
        double? mAn1 = analyticMeff[mi].MO1, mAn2 = analyticMeff[mi].MO2;
        double? z1 = mw1 is double a && sw1 is double b && b > 0 && mAn1 is double c ? System.Math.Abs(a - c) / b : null;
        double? z2 = mw2 is double a2 && sw2 is double b2 && b2 > 0 && mAn2 is double c2v ? System.Math.Abs(a2 - c2v) / b2 : null;
        bool gateO1 = z1 is double zz && zz <= FreeFieldSigmaGate;
        freeGates.Add(new FreeFieldGate(cr.Spec.Member, cr.Spec.Beta, mAn1, mAn2, mw1, sw1, mw2, sw2, z1, z2, gateO1));
    }
    bool freeFieldGatePassed = freeGates.Count == 2 && freeGates.All(g => g.GateO1);

    // Per-production-member verdicts.
    var memberVerdicts = new List<MemberVerdict>();
    foreach (var cr in columnResults.Values.Where(c => c.Spec.Kind == "production").OrderBy(c => c.Spec.Index))
    {
        var (m1, s1, chi1, plat1) = WindowMass(cr.Meff1);
        var (m2, s2, chi2v, plat2) = WindowMass(cr.Meff2);
        bool statsOk = cr.NEffGate && plat1 && plat2 && m1 is not null && m2 is not null;
        double? crossZ = m1 is double a && m2 is double b && s1 is double sa && s2 is double sb
            ? System.Math.Abs(a - b) / System.Math.Sqrt(sa * sa + sb * sb) : null;
        bool crossOk = crossZ is double cz && cz <= CrossCheckSigmaGate;
        string verdict; string reason;
        if (!statsOk)
        {
            verdict = "inconclusive-statistics";
            reason = !cr.NEffGate ? "tau_int-based N_eff below gate" : "effective-mass point missing or plateau gate failed in the pre-registered window";
        }
        else if (!crossOk)
        {
            verdict = "inconclusive-statistics";
            reason = $"interpolator-incompatible: |m(O1)-m(O2)| = {crossZ:F2} sigma > {CrossCheckSigmaGate} (both interpolate 0++; incompatibility fail-closes)";
        }
        else
        {
            // Combined mass: inverse-variance weighted over the two interpolators.
            // CORRELATION-CONSERVATIVE error: both interpolators are measured on the
            // SAME ensemble (for the identity member O2 = O1 exactly), so the
            // independent-channel 1/sqrt(w1+w2) would understate sigma; use the
            // rho = 1 upper bound sigma = s1 s2 (s1 + s2) / (s1^2 + s2^2), which
            // reduces to s for two identical channels.
            double w1 = 1.0 / (s1!.Value * s1.Value), w2 = 1.0 / (s2!.Value * s2.Value);
            double mComb = (w1 * m1!.Value + w2 * m2!.Value) / (w1 + w2);
            double sComb = s1.Value * s2.Value * (s1.Value + s2.Value) / (s1.Value * s1.Value + s2.Value * s2.Value);
            if (mComb >= GappedSignificance * sComb)
            { verdict = "scalar-channel-gapped-measured"; reason = $"a*m_0++ = {mComb:F4} +- {sComb:F4} ({mComb / sComb:F1} sigma from zero)"; }
            else if (mComb <= 2.0 * sComb && sComb <= GaplessSigmaCeiling)
            { verdict = "scalar-channel-gapless"; reason = $"a*m_0++ = {mComb:F4} +- {sComb:F4} consistent with zero at resolution {sComb:F4}"; }
            else
            { verdict = "inconclusive-statistics"; reason = $"a*m_0++ = {mComb:F4} +- {sComb:F4}: neither gapped ({GappedSignificance} sigma) nor gapless (2 sigma with sigma <= {GaplessSigmaCeiling}) rule fires"; }
        }
        memberVerdicts.Add(new MemberVerdict(cr.Spec.Member, cr.Spec.IsControlMember, cr.Spec.Beta,
            m1, s1, chi1, m2, s2, chi2v, crossZ, crossOk, statsOk, verdict, reason));
        lock (consoleLock)
            Console.WriteLine($"[n={n}] VERDICT {cr.Spec.Member}: {verdict} -- {reason}");
    }

    torusRecords.Add(new TorusRecord(n, volume, mesh.EdgeCount, mesh.FaceCount, nOmega, nTheta,
        orbitMapOk && faceMapOk, faceOrbitsUniform, faceTypeCount, faceBaseEqualsFaces0Count == mesh.FaceCount,
        informative, window, windowIncludesT0,
        memberBatteries, memberAnalytics, gaussSim, analyticMeff,
        columnResults.Values.OrderBy(c => c.Spec.Index).ToList(), freeGates, freeFieldGatePassed, memberVerdicts));
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Gate roll-up + verdicts.
// ---------------------------------------------------------------------------

bool orbitMapsOk = torusRecords.All(t => t.OrbitMapOk && t.FaceOrbitsUniform);
double objConsistencyWorst = torusRecords.Max(t => t.Batteries.Max(b => b.ObjConsistency));
bool objectiveConsistencyBattery = objConsistencyWorst <= ObjectiveConsistencyGate;
double rotInvarianceWorst = torusRecords.Max(t => t.Batteries.Max(b => b.RotationInvariance));
bool uniformThetaInvarianceGate = rotInvarianceWorst <= RotationInvarianceGate;   // gate (5)
double identityThetaGradWorst = torusRecords.Max(t => t.Batteries[0].IdentityThetaGradNorm);
bool identityThetaIndependent = identityThetaGradWorst <= 1e-10;
double expLogWorst = torusRecords.Max(t => t.Batteries.Max(b => b.ExpLogResid));
bool expLogBattery = expLogWorst <= ExpLogGate;
double blockResidWorst = torusRecords.Max(t => t.Batteries.Max(b => b.BlockResidual));
bool blockResidualBattery = blockResidWorst <= BlockResidualGate;
double planeWaveWorst = torusRecords.Max(t => t.Batteries.Max(b => b.PlaneWaveResid));
bool planeWaveBattery = planeWaveWorst <= PlaneWaveHvGate;
double gaussSimWorstZ = torusRecords.Max(t => t.GaussSim.Max(g => System.Math.Max(g.WorstZO1, g.WorstZO2)));
bool gaussSimBattery = gaussSimWorstZ <= GaussSimSigmaGate;
bool negativeModeAbsent = torusRecords.All(t => t.Analytics.All(a => a.NNeg == 0));
bool samplerGates = torusRecords.All(t => t.Columns.All(c => c.DHGate && c.VirialGate));   // gate (2)
bool freeFieldGate = torusRecords.All(t => t.FreeFieldGatePassed);                          // gate (1)
bool nEffGateAll = torusRecords.All(t => t.Columns.All(c => c.NEffGate));

bool batteriesAllPassed =
    precursorsPassed &&
    orbitMapsOk &&
    objectiveConsistencyBattery &&
    uniformThetaInvarianceGate &&
    identityThetaIndependent &&
    expLogBattery &&
    blockResidualBattery &&
    planeWaveBattery &&
    gaussSimBattery &&
    negativeModeAbsent &&
    samplerGates &&
    nEffGateAll &&
    freeFieldGate;

// Headline: the Einsteinian production member's verdict (identity recorded as control).
var allVerdicts = torusRecords.SelectMany(t => t.Verdicts.Select(v => (t.N, v))).ToList();
var einVerdicts = allVerdicts.Where(x => !x.v.IsControlMember).Select(x => x.v.Verdict).ToList();
var idVerdicts = allVerdicts.Where(x => x.v.IsControlMember).Select(x => x.v.Verdict).ToList();
string scalarChannelVerdict =
    !batteriesAllPassed ? "blocked"
    : einVerdicts.All(v => v == "scalar-channel-gapped-measured") ? "scalar-channel-gapped-measured"
    : einVerdicts.All(v => v == "scalar-channel-gapless") ? "scalar-channel-gapless"
    : einVerdicts.Any(v => v is "scalar-channel-gapped-measured" or "scalar-channel-gapless") ? "mixed-verdicts-recorded"
    : "inconclusive-statistics";

// ---------------------------------------------------------------------------
// Framing + fail-closed block (verbatim keys).
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool physicistReviewPending = true;
const bool coshCorrectedEffectiveMassesOnly = true;
const bool thetaHaarMeasureUsed = true;
const bool theta0SliceIsSamplerDemoOnly = true;
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase256Contract = false;
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; lattice-canonical tori n in {{{string.Join(",", torusSizes)}}}; time axis 0; face slice = canonical Kuhn-chain base vertex",
    "0++ spectroscopy of e^{-beta S_B}: HMC on omega (analytic ComputeJointGradient forces) + Haar-invariant per-vertex rotation Metropolis on theta; interpolators O1 = time-slice tr(F^2), O2 = time-slice Tr(Upsilon^dagger Upsilon) via the operator's EvaluateWithTheta path; vacuum-subtracted connected correlators, time-translation averaged; COSH-CORRECTED effective masses only; pre-registered window rule; jackknife errors; analytic Gaussian free-field gate from block-circulant momentum spectra; no target values")))).ToLowerInvariant();

bool scalarChannelSpectroscopyProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    coshCorrectedEffectiveMassesOnly &&
    thetaHaarMeasureUsed &&
    theta0SliceIsSamplerDemoOnly &&
    physicistReviewPending &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized &&
    !internalGaugeContentRealized &&
    !weldRealized &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = scalarChannelSpectroscopyProbePassed
    ? scalarChannelVerdict switch
    {
        "scalar-channel-gapped-measured" => TerminalPrefix + "passed-scalar-channel-gapped-measured-workbench-relative-no-gev",
        "scalar-channel-gapless" => TerminalPrefix + "passed-scalar-channel-gapless-recorded",
        "mixed-verdicts-recorded" => TerminalPrefix + "passed-mixed-verdicts-recorded",
        _ => TerminalPrefix + "passed-inconclusive-statistics-recorded",
    }
    : TerminalPrefix + "blocked";

const string PureGaugeRatioNote =
    "a*m_0++ is the scalar glueball-like gap of THIS pure-S_B lattice action, in lattice units of the " +
    "reduced spin-4 slice workbench. It is a pure-gauge-sector composite-pole scale: ratios to any " +
    "electroweak-labeled quantity are undefined here, and per the binding label caveat the W/Z/H labels " +
    "attach only in a Higgs phase -- this number is NEVER m_H.";

string decision = scalarChannelSpectroscopyProbePassed
    ? $"The 0++ scalar-channel spectroscopy probe is decided on internal consistency. On the lattice-canonical torus/tori n in {{{string.Join(",", torusSizes)}}} the pure S_B ensemble e^{{-beta S_B}} was sampled by omega-HMC (analytic joint-gradient forces, prototype pattern) interleaved with a Haar-invariant per-vertex rotation Metropolis on theta (review-board binding condition (ii)); the strictly gauge+translation-invariant interpolators O1 = time-slice tr(F^2) and O2 = time-slice Tr(Upsilon^dagger Upsilon) (the action's own residual) were measured every trajectory; vacuum-subtracted connected correlators were time-translation averaged and COSH-CORRECTED effective masses extracted (never the log-ratio) with jackknife errors and a pre-registered window rule. " +
      $"GATES: free-field control (sampler large-beta columns vs the analytic Gaussian correlator built from the exact block-circulant momentum spectrum, validated by an exact direct-Gaussian pipeline battery at worst {gaussSimWorstZ:F2} sigma) = {freeFieldGate}; <e^-dH>/equipartition per column = {samplerGates}; uniform-theta (global gauge rotation) invariance worst {rotInvarianceWorst:E2} <= {RotationInvarianceGate:E0}; objective consistency {objConsistencyWorst:E2}; block eigensolver residual {blockResidWorst:E2}; plane-wave Hv {planeWaveWorst:E2}; exp/log rotation-coordinate batteries {expLogWorst:E2}. " +
      $"VERDICT ({scalarChannelVerdict}): per-member channel verdicts {string.Join("; ", allVerdicts.Select(x => $"n={x.N} {x.v.Member}: {x.v.Verdict}"))}. " +
      $"{PureGaugeRatioNote} MANDATORY FRAMING: workbench-relative structure data ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units, beta = {betaProd} recorded); NO GeV/pole/VEV promotion; theta=0 free-field columns are sampler demos, never physics verdicts. Everything is target-blind; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the channel verdicts until the precursor, free-field, sampler, invariance, and exactness gates pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

object MeffJson(MeffResult m) => m.Points.Select(p => new
{
    t = p.T,
    ratio = FiniteOrNull(p.Ratio),
    m = p.M is double mv ? FiniteOrNull(mv) : (double?)null,
    sigma = p.Sigma is double sv ? FiniteOrNull(sv) : (double?)null,
}).ToArray();

object? Phase456Json(Phase456ColumnData? p) => p is null ? null : new
{
    identityIrrep2x2Gevp = new
    {
        dominantEigenvalue = p.IdentityIrrep2x2Gevp.DominantEigenvalue.Select(FiniteOrNull).ToArray(),
        jackknifeDominantEigenvalue = p.IdentityIrrep2x2Gevp.JackknifeDominantEigenvalue
            .Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(),
        effectiveMasses = MeffJson(p.IdentityIrrep2x2Gevp.EffectiveMasses),
        p.IdentityIrrep2x2Gevp.C0PositiveDefinite,
        p.IdentityIrrep2x2Gevp.T0,
    },
    a2 = new
    {
        o1 = new { c = p.A2O1.C.Select(FiniteOrNull).ToArray(), sigma = p.A2O1.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.A2O1.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.A2MeffO1) },
        o2 = new { c = p.A2O2.C.Select(FiniteOrNull).ToArray(), sigma = p.A2O2.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.A2O2.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.A2MeffO2) },
        p.A2KernelNumerators,
        p.A2KernelDenominator,
    },
    kMin = new
    {
        spatialAxisAverageO1 = new { c = p.KMinO1.C.Select(FiniteOrNull).ToArray(), sigma = p.KMinO1.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.KMinO1.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.KMinMeffO1) },
        momentum = "one exact fourth-root-of-unity unit, averaged over spatial axes 1,2,3",
    },
    perSiteSpatialCorrelators = p.AllSpatialMomenta.Select(k => new
    {
        k = new[] { k.Kx, k.Ky, k.Kz },
        c = k.Correlator.C.Select(FiniteOrNull).ToArray(),
        sigma = k.Correlator.Sigma.Select(FiniteOrNull).ToArray(),
        jackknife = k.Correlator.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(),
    }).ToArray(),
    binder = new
    {
        binderCumulant = FiniteOrNull(p.Binder.BinderCumulant),
        binderSigma = FiniteOrNull(p.Binder.BinderSigma),
        binderJackknife = p.Binder.BinderJackknife.Select(FiniteOrNull).ToArray(),
        susceptibility = FiniteOrNull(p.Binder.Susceptibility),
        susceptibilitySigma = FiniteOrNull(p.Binder.SusceptibilitySigma),
        susceptibilityJackknife = p.Binder.SusceptibilityJackknife.Select(FiniteOrNull).ToArray(),
        tauPhi = FiniteOrNull(p.Binder.TauPhi),
        nEffPhi = FiniteOrNull(p.Binder.NEffPhi),
    },
    rowEffectiveSampleSizes = new
    {
        tauA2 = FiniteOrNull(p.TauA2),
        nEffA2 = FiniteOrNull(p.NEffA2),
        tauKMin = FiniteOrNull(p.TauKMin),
        nEffKMin = FiniteOrNull(p.NEffKMin),
    },
    invariantRayProjectorOverlapMax = FiniteOrNull(p.InvariantRayProjectorOverlapMax),
    p.PerSiteCorrelatorStorage,
    p.PerFaceTypeResolutionRetained,
    spatialKMinReconstructionResidual = FiniteOrNull(p.SpatialKMinReconstructionResidual),
    p.StorageMeaning,
};

object? Phase456GaussianJson(Phase456GaussianControl? p) => p is null ? null : new
{
    p.IndependentSamples,
    identityIrrep2x2Gevp = new
    {
        dominantEigenvalue = p.IdentityIrrep2x2Gevp.DominantEigenvalue.Select(FiniteOrNull).ToArray(),
        jackknifeDominantEigenvalue = p.IdentityIrrep2x2Gevp.JackknifeDominantEigenvalue
            .Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(),
        effectiveMasses = MeffJson(p.IdentityIrrep2x2Gevp.EffectiveMasses),
        p.IdentityIrrep2x2Gevp.C0PositiveDefinite,
        p.IdentityIrrep2x2Gevp.T0,
    },
    a2 = new
    {
        o1 = new { c = p.A2O1.C.Select(FiniteOrNull).ToArray(), sigma = p.A2O1.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.A2O1.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.A2MeffO1) },
        o2 = new { c = p.A2O2.C.Select(FiniteOrNull).ToArray(), sigma = p.A2O2.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.A2O2.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.A2MeffO2) },
    },
    kMin = new
    {
        spatialAxisAverageO1 = new { c = p.KMinO1.C.Select(FiniteOrNull).ToArray(), sigma = p.KMinO1.Sigma.Select(FiniteOrNull).ToArray(), jackknife = p.KMinO1.Jackknife.Select(row => row.Select(FiniteOrNull).ToArray()).ToArray(), meff = MeffJson(p.KMinMeffO1) },
    },
    binder = new
    {
        binderCumulant = FiniteOrNull(p.Binder.BinderCumulant),
        binderSigma = FiniteOrNull(p.Binder.BinderSigma),
        binderJackknife = p.Binder.BinderJackknife.Select(FiniteOrNull).ToArray(),
        susceptibility = FiniteOrNull(p.Binder.Susceptibility),
        susceptibilitySigma = FiniteOrNull(p.Binder.SusceptibilitySigma),
        susceptibilityJackknife = p.Binder.SusceptibilityJackknife.Select(FiniteOrNull).ToArray(),
        tauPhi = FiniteOrNull(p.Binder.TauPhi),
        nEffPhi = FiniteOrNull(p.Binder.NEffPhi),
    },
};

var result = new
{
    phaseId = "phase452-scalar-channel-spectroscopy-probe",
    generatedAt = DateTimeOffset.UtcNow,
    phase456Run = phase456Mode ? new
    {
        mode = phase456ProductionMode ? "production" : "smoke-nonproduction",
        committedDefaults = phase456ProductionMode,
        environmentOverridesRefused = true,
        pinnedPackSha256 = Phase456PinnedPackSha256,
        computedPackSha256 = phase456ComputedPackSha256,
        perSiteCorrelatorStorage = true,
        perFaceTypeResolutionRetained = true,
    } : null,
    terminalStatus,
    scalarChannelSpectroscopyProbePassed,

    phase448PrecursorPassed,
    phase449PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    probeConfiguration = new
    {
        lieAlgebraId = "su2-trace-pairing",
        torusSizes,
        latticeCanonical = true,
        timeAxis = TimeAxis,
        sliceConvention = "lattice axis 0 is Euclidean time; a face belongs to the slice of its CANONICAL BASE VERTEX = the unique face vertex from which the minimal-image displacements of the other two vertices both lie in {0,1}^4 (the Kuhn-chain minimum; manifestly translation covariant)",
        interpolators = new
        {
            o1 = "O1(tau) = sum over faces in slice tau of tr(F_f^2) (trace-pairing InnerProduct of the CurvatureAssembler face coefficients); gauge invariant (F transforms in the adjoint), theta-blind",
            o2 = "O2(tau) = sum over faces in slice tau of Tr(Upsilon_f^dagger Upsilon_f) with Upsilon = the S_B residual via the operator's EvaluateWithTheta path (Evaluate for the identity member); with the uniform unit face weights sum_tau O2(tau) = 2 S_B exactly (battery-verified)",
        },
        ensemble = new
        {
            action = "pure S_B = (1/2)<Upsilon, M Upsilon> (exactly quartic in omega, transcendental in theta; S_B >= 0: no sign problem)",
            betaProduction = betaProd,
            betaHigherControl = betaMid,
            betaFreeField = betaFree,
            omegaSampler = "HMC, leapfrog nLeap steps, analytic ComputeJointGradient forces, warmup step-size auto-tune (the cep_hmc_prototype pattern at torus scale)",
            thetaSampler = "Haar-invariant rotation Metropolis: per-vertex group-composition proposals R'_v = exp(ad_delta_v) exp(ad_theta_v) (delta isotropic Gaussian, symmetric w.r.t. Haar), applied as collective all-vertex moves each trajectory plus a periodic single-site sweep; targets Haar x e^{-beta S_B} (binding condition (ii): theta lives in SO(3) per vertex; the R^n theta integral diverges)",
            identityThetaNote = "the identity member is exactly theta-independent (battery: GradTheta = 0), so its theta-Haar factor cancels in every observable and theta is not sampled for it",
            freeColumnConvention = "free-field control columns run at theta = 0 (SAMPLER-DEMO label per binding condition (iv)) and sample e^{-beta S_B} restricted to the orthogonal complement of ker H(0) -- the exact flat/pure-gauge cone tangent, on which the Gaussian theory does not exist and which the analytic prediction excludes (RECORDED CONVENTION; physics columns are unrestricted)",
            trajProduction = trajProd,
            trajControl = trajCtrl,
            warmup = warmTraj,
            nLeap,
            thetaMovesPerTraj = ThetaMovesPerTraj,
            thetaSweepEvery = ThetaSweepEvery,
        },
        correlators = "C(t) = <O(t)O(0)> - <O>^2 averaged over all time translations (vacuum-subtracted connected); jackknife over " + JackknifeBlocks + " blocks",
        coshConvention = "COSH-CORRECTED ONLY (the naive log-ratio was proven corrupted by the periodic image): solve C(t)/C(t+1) = cosh(m(t - T/2))/cosh(m(t + 1 - T/2)) by bisection; informative points t <= T/2 - 1",
        windowRule = "PRE-REGISTERED: window = { t : 1 <= t <= T/2 - 1 } if nonempty, else {0} with the excited-state-contamination caveat recorded (T = 3 has exactly one informative point); plateau chi^2/dof <= " + PlateauChi2Max + " gates windows with >= 2 points",
        freeFieldGate = "analytic Gaussian correlator from the exact block spectrum: Cov(dx) = (1/(beta V)) sum_k e^{2 pi i k.dx/n} G(k) H(k)^+ G(k)^dagger with H(k) the 45x45 momentum blocks (45 orbit-representative Hv columns, phase448 machinery) and G(k) the interpolator's exact linearization kernel; C_free(t) = 2 n^3 sum_{F,F',dx_s} Tr(K M K M^T); sampler large-beta cosh m_eff must match within " + FreeFieldSigmaGate + " sigma (O1 gating, O2 recorded); the formula itself is validated by an exact direct-Gaussian-sampling battery through the identical measurement pipeline (quadratic-truncated interpolators; gate " + GaussSimSigmaGate + " sigma)",
        verdictRules = $"pre-registered: gapped-measured if m >= {GappedSignificance} sigma; gapless if m <= 2 sigma and sigma <= {GaplessSigmaCeiling}; else inconclusive-statistics; O1-vs-O2 incompatibility (> {CrossCheckSigmaGate} sigma) fail-closes to inconclusive-statistics; combined mass = inverse-variance weighted mean with the CORRELATION-CONSERVATIVE rho=1 error s1 s2 (s1+s2)/(s1^2+s2^2) (both interpolators live on the same ensemble; for the identity member O2 = O1 exactly)",
        gates = new
        {
            objectiveConsistencyGate = ObjectiveConsistencyGate,
            rotationInvarianceGate = RotationInvarianceGate,
            blockResidualGate = BlockResidualGate,
            planeWaveHvGate = PlaneWaveHvGate,
            expLogGate = ExpLogGate,
            gaussSimSigmaGate = GaussSimSigmaGate,
            freeFieldSigmaGate = FreeFieldSigmaGate,
            crossCheckSigmaGate = CrossCheckSigmaGate,
            dhSigmaGate = DHSigmaGate,
            dhAbsGate = DHAbsGate,
            virialSigmaGate = VirialSigmaGate,
            virialRelGate = VirialRelGate,
            nEffMin = NeffMin,
            zeroModeRelTol = ZeroModeRelTol,
            zeroModeAbsFloor = ZeroModeAbsFloor,
            hvStep = HvStep,
            linStep = LinStep,
        },
        rngSeed = RngSeed,
        gaussSimSamples,
    },

    // Headline.
    scalarChannelVerdict,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    coshCorrectedEffectiveMassesOnly,
    thetaHaarMeasureUsed,
    theta0SliceIsSamplerDemoOnly,
    physicistReviewPending,
    pureGaugeRatioNote = PureGaugeRatioNote,

    batteries = new
    {
        batteriesAllPassed,
        orbitMapsOk,
        objectiveConsistencyBattery,
        objConsistencyWorst,
        uniformThetaInvarianceGate,
        rotInvarianceWorst,
        identityThetaIndependent,
        identityThetaGradWorst,
        expLogBattery,
        expLogWorst,
        blockResidualBattery,
        blockResidWorst,
        planeWaveBattery,
        planeWaveWorst,
        gaussSimBattery,
        gaussSimWorstZ,
        negativeModeAbsent,
        samplerGates,
        freeFieldGate,
        nEffGateAll,
    },

    tori = torusRecords.Select(tr => new
    {
        torusN = tr.N,
        vertexCount = tr.Volume,
        edgeCount = tr.Edges,
        faceCount = tr.FaceCountValue,
        nOmega = tr.NOmega,
        nTheta = tr.NTheta,
        faceTypeCount = tr.FaceTypeCount,
        faceBaseMatchesStoredOrder = tr.FaceBaseMatchesStored,
        informativePoints = tr.Informative,
        window = tr.Window,
        windowIncludesT0 = tr.WindowIncludesT0,
        memberBatteries = tr.Batteries.Select((b, i) => new
        {
            member = i == 0 ? "identity" : "sd2-id0/c0.5",
            objectiveConsistency = b.ObjConsistency,
            uniformThetaInvariance = b.RotationInvariance,
            identityThetaGradNorm = b.IdentityThetaGradNorm,
            identityThetaSDiff = b.IdentityThetaSDiff,
            expLogResid = b.ExpLogResid,
            hvThetaLeakAtOrigin = b.HvThetaMax,
            blockResidual = b.BlockResidual,
            planeWaveHvResid = b.PlaneWaveResid,
            covImagMax = b.CovImagMax,
            zeroBasisGramResid = b.ZGramResid,
            zeroBasisHzResid = b.ZHzResid,
        }).ToArray(),
        spectra = tr.Analytics.Select((a, i) => new
        {
            member = i == 0 ? "identity" : "sd2-id0/c0.5",
            positiveModes = a.NPos,
            zeroModes = a.NZero,
            negativeModes = a.NNeg,
            minEigenvalue = a.MinEig,
            maxAbsEigenvalue = a.MaxAbsEig,
            zeroTol = a.ZeroTol,
            analyticC1 = a.C1An,
            analyticC2 = a.C2An,
            analyticMeanO1PerSlice = a.Mean1An,
            analyticMeanO2PerSlice = a.Mean2An,
            analyticMeffO1 = FiniteOrNull(tr.AnalyticMeff[i].MO1 ?? double.NaN),
            analyticMeffO2 = FiniteOrNull(tr.AnalyticMeff[i].MO2 ?? double.NaN),
        }).ToArray(),
        gaussianSimBattery = tr.GaussSim.Select((g, i) => new
        {
            member = i == 0 ? "identity" : "sd2-id0/c0.5",
            worstZO1 = g.WorstZO1,
            worstZO2 = g.WorstZO2,
            selfConjImagMax = g.SelfConjImagMax,
            simC1 = g.SimC1,
            simC2 = g.SimC2,
            phase456ExactGaussianControl = Phase456GaussianJson(g.Phase456ExactControl),
        }).ToArray(),
        columns = tr.Columns.Select(c => new
        {
            member = c.Spec.Member,
            beta = c.Spec.Beta,
            kind = c.Spec.Kind,
            samplerDemoLabel = c.Spec.Kind == "free-field-control",
            trajectories = c.Spec.NTraj,
            warmup = c.Spec.NWarm,
            nDof = c.NDof,
            zeroModesProjected = c.NZeroProjected,
            eps = c.Eps,
            sigmaTheta = c.Spec.SampleTheta ? (double?)c.SigmaTheta : null,
            acceptance = c.Acceptance,
            thetaCollectiveAcceptance = c.ThetaCollAcceptance,
            thetaSiteAcceptance = c.ThetaSiteAcceptance,
            expDH = c.ExpDHMean,
            expDHSigma = c.ExpDHSigma,
            dhGate = c.DHGate,
            virial = c.VirialMean,
            virialSigma = c.VirialSigma,
            virialTarget = c.NDof,
            virialGate = c.VirialGate,
            tauIntO1 = c.TauO1,
            tauIntO2 = c.TauO2,
            tauIntS = c.TauS,
            nEff = c.NEff,
            nEffGate = c.NEffGate,
            meanS = c.SMean,
            meanO1PerSlice = c.O1Mean,
            meanO2PerSlice = c.O2Mean,
            c1 = c.C1.C,
            c1Sigma = c.C1.Sigma,
            c2 = c.C2.C,
            c2Sigma = c.C2.Sigma,
            meffO1 = MeffJson(c.Meff1),
            meffO2 = MeffJson(c.Meff2),
            phase456 = Phase456Json(c.Phase456),
            msPerTrajectory = c.MsPerTrajectory,
        }).ToArray(),
        freeFieldGates = tr.FreeGates.Select(g => new
        {
            member = g.Member,
            beta = g.Beta,
            analyticMeffO1 = FiniteOrNull(g.MAn1 ?? double.NaN),
            analyticMeffO2 = FiniteOrNull(g.MAn2 ?? double.NaN),
            measuredMeffO1 = FiniteOrNull(g.M1 ?? double.NaN),
            measuredMeffO1Sigma = FiniteOrNull(g.S1 ?? double.NaN),
            measuredMeffO2 = FiniteOrNull(g.M2 ?? double.NaN),
            measuredMeffO2Sigma = FiniteOrNull(g.S2 ?? double.NaN),
            zO1 = FiniteOrNull(g.Z1 ?? double.NaN),
            zO2Recorded = FiniteOrNull(g.Z2 ?? double.NaN),
            gateO1 = g.GateO1,
        }).ToArray(),
        verdicts = tr.Verdicts.Select(v => new
        {
            member = v.Member,
            isControl = v.IsControlMember,
            beta = v.Beta,
            mO1 = FiniteOrNull(v.M1 ?? double.NaN),
            mO1Sigma = FiniteOrNull(v.S1 ?? double.NaN),
            plateauChi2DofO1 = FiniteOrNull(v.Chi1 ?? double.NaN),
            mO2 = FiniteOrNull(v.M2 ?? double.NaN),
            mO2Sigma = FiniteOrNull(v.S2 ?? double.NaN),
            plateauChi2DofO2 = FiniteOrNull(v.Chi2 ?? double.NaN),
            crossCheckSigma = FiniteOrNull(v.CrossZ ?? double.NaN),
            crossCheckCompatible = v.CrossOk,
            statisticsGatesPassed = v.StatsOk,
            verdict = v.Verdict,
            reason = v.Reason,
        }).ToArray(),
    }).ToArray(),

    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
        draft14dObserverseNotRealized = true,
    },

    runtimeSeconds,

    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256Contract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256Contract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },

    sourceEvidence = new
    {
        phase448SummaryPath = Phase448SummaryPath,
        phase449SummaryPath = Phase449SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
        reviewBoardSourcePath = ReviewBoardSourcePath,
        restartPromptSourcePath = RestartPromptSourcePath,
        hmcPrototypePath = "studies/phase448_torus_mode_volume_saturation_probe_001/cep_hmc_prototype/",
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "LABEL CAVEAT (binding): any a*m_0++ reported here is the scalar glueball-like gap of THIS pure-S_B action in lattice units; it is NEVER m_H, and no W/Z/H label attaches to anything in this phase -- those labels attach only in a Higgs phase.",
        "This phase measurement-tests the 2026-07-04 review board's convex/gapped picture (physical content = gauge-invariant composite poles per the Elitzur/FMS reframe); a gapped verdict supports the picture at workbench level, a gapless or inconclusive verdict is recorded as exactly that (null reported as null).",
        "All masses are workbench-relative structure data of the reduced spin-4 slice (su(2) toy algebra, lattice-canonical torus, lattice units, beta = 1 convention recorded): NOT physical masses, NOT a scale in GeV; no VEV/pole/GeV lineage exists on this route.",
        "The theta = 0 free-field control columns are SAMPLER DEMOS (binding condition (iv)): they validate the sampler and measurement pipeline against the exactly solvable Gaussian theory and carry no physics verdict; the ker-H(0) restriction there is a recorded control convention, not a physics choice.",
        "The T = 3 time extent admits exactly one informative cosh point; the pre-registered window rule fires on it with the excited-state-contamination caveat recorded -- single-window masses are upper-bound-flavored estimates of the 0++ gap, and n = 4 (env-enabled) adds the second point.",
        "No Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content; nothing is promoted either way.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "scalar_channel_spectroscopy_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "scalar_channel_spectroscopy_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scalarChannelSpectroscopyProbePassed={scalarChannelSpectroscopyProbePassed}");
Console.WriteLine($"precursors: p448={phase448PrecursorPassed} p449={phase449PrecursorPassed}");
Console.WriteLine($"BATTERIES: all={batteriesAllPassed} objCons={objectiveConsistencyBattery}({objConsistencyWorst:E2}) uniformTheta={uniformThetaInvarianceGate}({rotInvarianceWorst:E2}) " +
    $"expLog={expLogBattery}({expLogWorst:E2}) blockResid={blockResidualBattery}({blockResidWorst:E2}) planeWave={planeWaveBattery}({planeWaveWorst:E2}) gaussSim={gaussSimBattery}({gaussSimWorstZ:F2}sig) " +
    $"negFree={negativeModeAbsent} sampler={samplerGates} freeField={freeFieldGate} nEffAll={nEffGateAll}");
foreach (var tr in torusRecords)
{
    foreach (var g in tr.FreeGates)
        Console.WriteLine($"  n={tr.N} FREE-FIELD {g.Member} beta={g.Beta}: analytic m(O1)={g.MAn1:F4} measured {g.M1:F4}+-{g.S1:F4} z={g.Z1:F2} gate={g.GateO1}; O2 analytic {g.MAn2:F4} measured {g.M2:F4}+-{g.S2:F4} (recorded)");
    foreach (var v in tr.Verdicts)
        Console.WriteLine($"  n={tr.N} {v.Member,-14} m(O1)={v.M1:F4}+-{v.S1:F4} m(O2)={v.M2:F4}+-{v.S2:F4} cross={v.CrossZ:F2}sig verdict={v.Verdict}");
}
Console.WriteLine($"VERDICT: {scalarChannelVerdict}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Statistics helpers.
// ---------------------------------------------------------------------------

static CorrelatorResult ConnectedCorrelator(double[][] series, int T, int nBlocks)
{
    int nS = series.Length;
    nBlocks = System.Math.Min(nBlocks, System.Math.Max(2, nS / 4));
    // Per-block partial sums of sum_tau O(tau) and sum_tau O(tau) O(tau+t).
    var bMean = new double[nBlocks];
    var bCorr = new double[nBlocks, T];
    var bCount = new long[nBlocks];
    for (int s = 0; s < nS; s++)
    {
        int b = (int)((long)s * nBlocks / nS);
        var o = series[s];
        double m = 0;
        for (int tau = 0; tau < T; tau++) m += o[tau];
        bMean[b] += m;
        bCount[b]++;
        for (int t = 0; t < T; t++)
        {
            double c = 0;
            for (int tau = 0; tau < T; tau++) c += o[tau] * o[(tau + t) % T];
            bCorr[b, t] += c;
        }
    }
    double[] CFrom(int skip)
    {
        double sm = 0; long cnt = 0;
        var sc = new double[T];
        for (int b = 0; b < nBlocks; b++)
        {
            if (b == skip) continue;
            sm += bMean[b]; cnt += bCount[b];
            for (int t = 0; t < T; t++) sc[t] += bCorr[b, t];
        }
        double mean = sm / (cnt * T);
        var c = new double[T];
        for (int t = 0; t < T; t++) c[t] = sc[t] / (cnt * T) - mean * mean;
        return c;
    }
    var full = CFrom(-1);
    var jk = new double[nBlocks][];
    for (int b = 0; b < nBlocks; b++) jk[b] = CFrom(b);
    var sigma = new double[T];
    for (int t = 0; t < T; t++)
    {
        double mean = 0;
        for (int b = 0; b < nBlocks; b++) mean += jk[b][t];
        mean /= nBlocks;
        double var2 = 0;
        for (int b = 0; b < nBlocks; b++) var2 += (jk[b][t] - mean) * (jk[b][t] - mean);
        sigma[t] = System.Math.Sqrt(var2 * (nBlocks - 1.0) / nBlocks);
    }
    return new CorrelatorResult(full, sigma, jk);
}

static CorrelatorResult CrossConnectedCorrelator(double[][] aSeries, double[][] bSeries, int T, int nBlocks)
{
    int nS = aSeries.Length;
    nBlocks = System.Math.Min(nBlocks, System.Math.Max(2, nS / 4));
    var bMeanA = new double[nBlocks]; var bMeanB = new double[nBlocks];
    var bCorr = new double[nBlocks, T]; var bCount = new long[nBlocks];
    for (int s = 0; s < nS; s++)
    {
        int b = (int)((long)s * nBlocks / nS);
        bCount[b]++;
        bMeanA[b] += aSeries[s].Sum(); bMeanB[b] += bSeries[s].Sum();
        for (int t = 0; t < T; t++)
            for (int tau = 0; tau < T; tau++)
                bCorr[b, t] += 0.5 * (aSeries[s][tau] * bSeries[s][(tau + t) % T] + bSeries[s][tau] * aSeries[s][(tau + t) % T]);
    }
    double[] Calc(int skip)
    {
        double ma = 0, mb = 0; long cnt = 0; var sc = new double[T];
        for (int b = 0; b < nBlocks; b++) if (b != skip)
        {
            ma += bMeanA[b]; mb += bMeanB[b]; cnt += bCount[b];
            for (int t = 0; t < T; t++) sc[t] += bCorr[b, t];
        }
        ma /= cnt * T; mb /= cnt * T;
        for (int t = 0; t < T; t++) sc[t] = sc[t] / (cnt * T) - ma * mb;
        return sc;
    }
    var full = Calc(-1); var jk = Enumerable.Range(0, nBlocks).Select(Calc).ToArray();
    return new CorrelatorResult(full, JackknifeSigma(jk, T), jk);
}

static CorrelatorResult ComplexConnectedCorrelator(double[][][] reSeries, double[][][] imSeries, int T, int nBlocks)
{
    int axes = reSeries.Length, nS = reSeries[0].Length;
    nBlocks = System.Math.Min(nBlocks, System.Math.Max(2, nS / 4));
    var bRe = new double[nBlocks, axes]; var bIm = new double[nBlocks, axes];
    var bCorr = new double[nBlocks, T]; var bCount = new long[nBlocks];
    for (int s = 0; s < nS; s++)
    {
        int b = (int)((long)s * nBlocks / nS); bCount[b]++;
        for (int axis = 0; axis < axes; axis++)
        {
            bRe[b, axis] += reSeries[axis][s].Sum(); bIm[b, axis] += imSeries[axis][s].Sum();
            for (int t = 0; t < T; t++) for (int tau = 0; tau < T; tau++)
                bCorr[b, t] += reSeries[axis][s][tau] * reSeries[axis][s][(tau + t) % T]
                    + imSeries[axis][s][tau] * imSeries[axis][s][(tau + t) % T];
        }
    }
    double[] Calc(int skip)
    {
        long cnt = 0; var sr = new double[axes]; var si = new double[axes]; var c = new double[T];
        for (int b = 0; b < nBlocks; b++) if (b != skip)
        {
            cnt += bCount[b];
            for (int axis = 0; axis < axes; axis++) { sr[axis] += bRe[b, axis]; si[axis] += bIm[b, axis]; }
            for (int t = 0; t < T; t++) c[t] += bCorr[b, t];
        }
        double vacuum = 0.0;
        for (int axis = 0; axis < axes; axis++)
        {
            double mr = sr[axis] / (cnt * T), mi = si[axis] / (cnt * T);
            vacuum += mr * mr + mi * mi;
        }
        vacuum /= axes;
        for (int t = 0; t < T; t++) c[t] = c[t] / (cnt * T * axes) - vacuum;
        return c;
    }
    var full = Calc(-1); var jk = Enumerable.Range(0, nBlocks).Select(Calc).ToArray();
    return new CorrelatorResult(full, JackknifeSigma(jk, T), jk);
}

static SpatialMomentumCorrelator[] SpatialMomentumCorrelators(
    double[,] meanRe, double[,] meanIm, double[,,] corr, long[] counts, int T)
{
    int blocks = counts.Length, momenta = meanRe.GetLength(1);
    var result = new SpatialMomentumCorrelator[momenta];
    for (int ki = 0; ki < momenta; ki++)
    {
        double[] Calc(int skip)
        {
            double sr = 0, si = 0; long count = 0; var c = new double[T];
            for (int b = 0; b < blocks; b++) if (b != skip)
            {
                sr += meanRe[b, ki]; si += meanIm[b, ki]; count += counts[b];
                for (int dt = 0; dt < T; dt++) c[dt] += corr[b, ki, dt];
            }
            double mr = sr / (count * T), mi = si / (count * T);
            double vacuum = mr * mr + mi * mi;
            for (int dt = 0; dt < T; dt++) c[dt] = c[dt] / (count * T) - vacuum;
            return c;
        }
        var full = Calc(-1); var jk = Enumerable.Range(0, blocks).Select(Calc).ToArray();
        var cr = new CorrelatorResult(full, JackknifeSigma(jk, T), jk);
        int kx = ki / 16, ky = (ki / 4) % 4, kz = ki % 4;
        result[ki] = new SpatialMomentumCorrelator(kx, ky, kz, cr);
    }
    return result;
}

static GevpResult GevpFromCorrelators(CorrelatorResult c11, CorrelatorResult c22, CorrelatorResult c12, int T, int nBlocks)
{
    double[] Solve(double[] a, double[] d, double[] b, out bool pd)
    {
        double det0 = a[0] * d[0] - b[0] * b[0];
        pd = a[0] > 0 && d[0] > 0 && det0 > 1e-24;
        var lambda = Enumerable.Repeat(double.NaN, T).ToArray();
        if (!pd) return lambda;
        for (int t = 0; t < T; t++)
        {
            double aa = (d[0] * a[t] - b[0] * b[t]) / det0;
            double ab = (d[0] * b[t] - b[0] * d[t]) / det0;
            double ba = (-b[0] * a[t] + a[0] * b[t]) / det0;
            double dd = (-b[0] * b[t] + a[0] * d[t]) / det0;
            double tr = aa + dd, det = aa * dd - ab * ba;
            double disc = tr * tr - 4 * det;
            if (disc >= -1e-12) lambda[t] = 0.5 * (tr + System.Math.Sqrt(System.Math.Max(0.0, disc)));
        }
        return lambda;
    }
    var full = Solve(c11.C, c22.C, c12.C, out bool pdFull);
    var jk = new double[c11.Jackknife.Length][]; bool jkPd = true;
    for (int block = 0; block < jk.Length; block++)
    {
        jk[block] = Solve(c11.Jackknife[block], c22.Jackknife[block], c12.Jackknife[block], out bool pd);
        jkPd &= pd;
    }
    var corr = new CorrelatorResult(full, JackknifeSigma(jk, T), jk);
    return new GevpResult(full, jk, CoshEffectiveMasses(corr, T, nBlocks), pdFull && jkPd, 0);
}

static BinderResult BinderStatistics(double[] series, int volume, int nBlocks)
{
    int nS = series.Length;
    nBlocks = System.Math.Min(nBlocks, System.Math.Max(2, nS / 4));
    (double U, double Chi) Calc(int skip)
    {
        var kept = Enumerable.Range(0, nS).Where(i => (int)((long)i * nBlocks / nS) != skip).Select(i => series[i]).ToArray();
        double mean = kept.Average(); double m2 = kept.Average(x => (x - mean) * (x - mean));
        double m4 = kept.Average(x => System.Math.Pow(x - mean, 4));
        return (m2 > 0 ? 1.0 - m4 / (3.0 * m2 * m2) : double.NaN, volume * m2);
    }
    var full = Calc(-1); var ju = new double[nBlocks][]; var jc = new double[nBlocks][];
    for (int b = 0; b < nBlocks; b++) { var x = Calc(b); ju[b] = new[] { x.U }; jc[b] = new[] { x.Chi }; }
    double tau = TauInt(series); double neff = nS / (2.0 * System.Math.Max(0.5, tau));
    return new BinderResult(full.U, JackknifeSigma(ju, 1)[0], ju.Select(x => x[0]).ToArray(),
        full.Chi, JackknifeSigma(jc, 1)[0], jc.Select(x => x[0]).ToArray(), tau, neff);
}

static double[] JackknifeSigma(double[][] jk, int length)
{
    var sigma = new double[length];
    if (jk.Length < 2) return sigma;
    for (int i = 0; i < length; i++)
    {
        var finite = jk.Select(x => x[i]).Where(double.IsFinite).ToArray();
        if (finite.Length != jk.Length) { sigma[i] = double.NaN; continue; }
        double mean = finite.Average();
        sigma[i] = System.Math.Sqrt(finite.Sum(x => (x - mean) * (x - mean)) * (finite.Length - 1.0) / finite.Length);
    }
    return sigma;
}

static MeffResult CoshEffectiveMasses(CorrelatorResult corr, int T, int nBlocks)
{
    nBlocks = corr.Jackknife.Length;
    var points = new List<MeffPoint>();
    for (int t = 0; t + 1 <= T - 1; t++)
    {
        if (T / 2.0 - t - 1 < -1e-9) break; // uninformative beyond T/2 - 1
        double ratio = corr.C[t] / corr.C[(t + 1) % T];
        double? m = SolveCoshMass(ratio, t, T);
        double? sig = null;
        if (m is not null)
        {
            var jkM = new List<double>();
            bool jkOk = true;
            for (int b = 0; b < nBlocks; b++)
            {
                double rb = corr.Jackknife[b][t] / corr.Jackknife[b][(t + 1) % T];
                double? mb = SolveCoshMass(rb, t, T);
                if (mb is null) { jkOk = false; break; }
                jkM.Add(mb.Value);
            }
            if (jkOk)
            {
                double mean = jkM.Average();
                double var2 = jkM.Sum(x => (x - mean) * (x - mean));
                sig = System.Math.Sqrt(var2 * (nBlocks - 1.0) / nBlocks);
            }
        }
        points.Add(new MeffPoint(t, ratio, m, sig));
    }
    return new MeffResult(points.ToArray());
}

static double? SolveCoshMass(double ratio, int t, int T)
{
    // C(t)/C(t+1) = cosh(m (T/2 - t)) / cosh(m (T/2 - t - 1)), increasing in m for t <= T/2 - 1.
    double a = T / 2.0 - t, b = T / 2.0 - t - 1;
    if (b < -1e-12 || !double.IsFinite(ratio) || ratio <= 1.0) return null;
    double F(double m) => System.Math.Cosh(m * a) / System.Math.Cosh(m * b) - ratio;
    double lo = 0.0, hi = 1.0;
    while (F(hi) < 0 && hi < 60) hi *= 2;
    if (F(hi) < 0) return null;
    for (int it = 0; it < 200; it++)
    {
        double mid = 0.5 * (lo + hi);
        if (F(mid) < 0) lo = mid; else hi = mid;
    }
    return 0.5 * (lo + hi);
}

static (double Mean, double Sigma) JackknifeMean(double[] series, int nBlocks)
{
    int nS = series.Length;
    nBlocks = System.Math.Min(nBlocks, System.Math.Max(2, nS / 4));
    var bSum = new double[nBlocks];
    var bCnt = new long[nBlocks];
    for (int s = 0; s < nS; s++)
    {
        int b = (int)((long)s * nBlocks / nS);
        bSum[b] += series[s]; bCnt[b]++;
    }
    double total = bSum.Sum(); long cnt = bCnt.Sum();
    var jk = new double[nBlocks];
    for (int b = 0; b < nBlocks; b++) jk[b] = (total - bSum[b]) / (cnt - bCnt[b]);
    double mean = total / cnt;
    double jkMean = jk.Average();
    double var2 = jk.Sum(x => (x - jkMean) * (x - jkMean)) * (nBlocks - 1.0) / nBlocks;
    return (mean, System.Math.Sqrt(var2));
}

static double TauInt(double[] x)
{
    int n = x.Length;
    double mean = x.Average();
    double c0 = 0;
    for (int i = 0; i < n; i++) { double d = x[i] - mean; c0 += d * d; }
    c0 /= n;
    if (c0 <= 0) return 0.5;
    double tau = 0.5;
    int wMax = System.Math.Min(2000, n / 4);
    for (int lag = 1; lag < wMax; lag++)
    {
        double c = 0;
        for (int i = 0; i < n - lag; i++) c += (x[i] - mean) * (x[i + lag] - mean);
        c /= n - lag;
        double rho = c / c0;
        if (rho < 0.02 && lag > 8) break;
        tau += rho;
    }
    return tau;
}

// ---------------------------------------------------------------------------
// Linear algebra + rotation helpers.
// ---------------------------------------------------------------------------

static (double[] Evals, double[,] VecRe, double[,] VecIm, double Residual) HermitianEigen(Cx[,] input)
{
    int n = input.GetLength(0);
    var a = (Cx[,])input.Clone();
    var v = new Cx[n, n];
    for (int i = 0; i < n; i++) v[i, i] = Cx.One;
    double scale = 0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) scale = System.Math.Max(scale, a[i, j].Magnitude);
    scale = System.Math.Max(scale, 1e-30);
    for (int sweep = 0; sweep < 100; sweep++)
    {
        double off = 0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++) off += a[p, q].Magnitude * a[p, q].Magnitude;
        if (System.Math.Sqrt(off) < 1e-14 * scale) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double m = a[p, q].Magnitude;
                if (m < 1e-30 * scale) continue;
                var phase = a[p, q] / m; // e^{i phi}
                double app = a[p, p].Real, aqq = a[q, q].Real;
                double tau = (aqq - app) / (2.0 * m);
                double t = tau >= 0 ? tau + System.Math.Sqrt(tau * tau + 1) : tau - System.Math.Sqrt(tau * tau + 1);
                // pick the root with |t| <= 1 (the other root of t^2 - 2 tau t - 1 = 0)
                if (System.Math.Abs(t) > 1.0) t = -1.0 / t;
                double c = 1.0 / System.Math.Sqrt(1 + t * t);
                double s = t * c;
                var sPh = s * phase;             // s e^{i phi}
                var sPhC = s * Cx.Conjugate(phase);
                // Column ops (A <- A U): col p <- c colp + s e^{-i phi} colq ; col q <- -s e^{i phi} colp + c colq.
                for (int k = 0; k < n; k++)
                {
                    var akp = a[k, p]; var akq = a[k, q];
                    a[k, p] = c * akp + sPhC * akq;
                    a[k, q] = -sPh * akp + c * akq;
                    var vkp = v[k, p]; var vkq = v[k, q];
                    v[k, p] = c * vkp + sPhC * vkq;
                    v[k, q] = -sPh * vkp + c * vkq;
                }
                // Row ops (A <- U^dagger A): row p <- c rowp + s e^{i phi} rowq ; row q <- -s e^{-i phi} rowp + c rowq.
                for (int k = 0; k < n; k++)
                {
                    var apk = a[p, k]; var aqk = a[q, k];
                    a[p, k] = c * apk + sPh * aqk;
                    a[q, k] = -sPhC * apk + c * aqk;
                }
            }
    }
    var evals = new double[n];
    for (int i = 0; i < n; i++) evals[i] = a[i, i].Real;
    var vecRe = new double[n, n]; var vecIm = new double[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) { vecRe[i, j] = v[i, j].Real; vecIm[i, j] = v[i, j].Imaginary; }
    // Residual: max |A v - lambda v| relative to scale (spot columns).
    double resid = 0;
    for (int j = 0; j < n; j++)
    {
        for (int i = 0; i < n; i++)
        {
            Cx acc = Cx.Zero;
            for (int k = 0; k < n; k++) acc += input[i, k] * v[k, j];
            acc -= evals[j] * v[i, j];
            resid = System.Math.Max(resid, acc.Magnitude / scale);
        }
    }
    return (evals, vecRe, vecIm, resid);
}

static (double[] Re, double[] Im) BlockTimesVector(double[] evals, double[,] vRe, double[,] vIm, double[] uRe, double[] uIm)
{
    int n = evals.Length;
    var re = new double[n]; var im = new double[n];
    // H u = V Lambda V^dagger u.
    var cRe = new double[n]; var cIm = new double[n];
    for (int a = 0; a < n; a++)
    {
        double sr = 0, si = 0;
        for (int i = 0; i < n; i++)
        {
            // conj(V[i,a]) * u[i]
            sr += vRe[i, a] * uRe[i] + vIm[i, a] * uIm[i];
            si += vRe[i, a] * uIm[i] - vIm[i, a] * uRe[i];
        }
        cRe[a] = evals[a] * sr; cIm[a] = evals[a] * si;
    }
    for (int i = 0; i < n; i++)
    {
        double sr = 0, si = 0;
        for (int a = 0; a < n; a++)
        {
            sr += vRe[i, a] * cRe[a] - vIm[i, a] * cIm[a];
            si += vRe[i, a] * cIm[a] + vIm[i, a] * cRe[a];
        }
        re[i] = sr; im[i] = si;
    }
    return (re, im);
}

static double[,] CrossMatrix(double[] w) => new double[3, 3]
{
    { 0, -w[2], w[1] },
    { w[2], 0, -w[0] },
    { -w[1], w[0], 0 },
};

static double[,] Rodrigues(double[] w)
{
    double a = System.Math.Sqrt(w[0] * w[0] + w[1] * w[1] + w[2] * w[2]);
    var k = CrossMatrix(w);
    double s, c2;
    if (a < 1e-8) { s = 1.0 - a * a / 6.0; c2 = 0.5 - a * a / 24.0; }
    else { s = System.Math.Sin(a) / a; c2 = (1.0 - System.Math.Cos(a)) / (a * a); }
    var r = new double[3, 3];
    for (int i = 0; i < 3; i++) r[i, i] = 1.0;
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
            double k2 = 0;
            for (int m = 0; m < 3; m++) k2 += k[i, m] * k[m, j];
            r[i, j] += s * k[i, j] + c2 * k2;
        }
    return r;
}

static double[] LogRotation(double[,] r)
{
    // Robust axis-angle via quaternion extraction; |w| <= pi.
    double tr = r[0, 0] + r[1, 1] + r[2, 2];
    double q0, q1, q2, q3;
    if (tr > 0)
    {
        double s = System.Math.Sqrt(tr + 1.0) * 2;
        q0 = 0.25 * s;
        q1 = (r[2, 1] - r[1, 2]) / s;
        q2 = (r[0, 2] - r[2, 0]) / s;
        q3 = (r[1, 0] - r[0, 1]) / s;
    }
    else if (r[0, 0] > r[1, 1] && r[0, 0] > r[2, 2])
    {
        double s = System.Math.Sqrt(1.0 + r[0, 0] - r[1, 1] - r[2, 2]) * 2;
        q0 = (r[2, 1] - r[1, 2]) / s;
        q1 = 0.25 * s;
        q2 = (r[0, 1] + r[1, 0]) / s;
        q3 = (r[0, 2] + r[2, 0]) / s;
    }
    else if (r[1, 1] > r[2, 2])
    {
        double s = System.Math.Sqrt(1.0 + r[1, 1] - r[0, 0] - r[2, 2]) * 2;
        q0 = (r[0, 2] - r[2, 0]) / s;
        q1 = (r[0, 1] + r[1, 0]) / s;
        q2 = 0.25 * s;
        q3 = (r[1, 2] + r[2, 1]) / s;
    }
    else
    {
        double s = System.Math.Sqrt(1.0 + r[2, 2] - r[0, 0] - r[1, 1]) * 2;
        q0 = (r[1, 0] - r[0, 1]) / s;
        q1 = (r[0, 2] + r[2, 0]) / s;
        q2 = (r[1, 2] + r[2, 1]) / s;
        q3 = 0.25 * s;
    }
    if (q0 < 0) { q0 = -q0; q1 = -q1; q2 = -q2; q3 = -q3; }
    double vn = System.Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3);
    double angle = 2.0 * System.Math.Atan2(vn, q0);
    if (vn < 1e-12) return new[] { 2.0 * q1, 2.0 * q2, 2.0 * q3 };
    double f = angle / vn;
    return new[] { f * q1, f * q2, f * q3 };
}

static double[,] MatrixExpSeries(double[,] a)
{
    var r = new double[3, 3];
    for (int i = 0; i < 3; i++) r[i, i] = 1.0;
    var term = new double[3, 3];
    for (int i = 0; i < 3; i++) term[i, i] = 1.0;
    for (int k2 = 1; k2 <= 40; k2++)
    {
        var next = new double[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                double s = 0;
                for (int m = 0; m < 3; m++) s += term[i, m] * a[m, j];
                next[i, j] = s / k2;
            }
        term = next;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++) r[i, j] += term[i, j];
    }
    return r;
}

static double[,] MatMul3(double[,] a, double[,] b)
{
    var r = new double[3, 3];
    for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
            double s = 0;
            for (int m = 0; m < 3; m++) s += a[i, m] * b[m, j];
            r[i, j] = s;
        }
    return r;
}

static double[] RotateField(double[] field, double[,] rot, int dimG)
{
    var outF = new double[field.Length];
    int blocks = field.Length / dimG;
    for (int b = 0; b < blocks; b++)
        for (int i = 0; i < dimG; i++)
        {
            double s = 0;
            for (int j = 0; j < dimG; j++) s += rot[i, j] * field[b * dimG + j];
            outF[b * dimG + i] = s;
        }
    return outF;
}

// ---------------------------------------------------------------------------
// Small helpers.
// ---------------------------------------------------------------------------

static int Mod(int a, int n) => ((a % n) + n) % n;

static int[]? MinImage01(int[] from, int[] to, int n)
{
    var d = new int[4];
    for (int i = 0; i < 4; i++)
    {
        int raw = Mod(to[i] - from[i], n);
        if (raw == 0) d[i] = 0;
        else if (raw == 1) d[i] = 1;
        else return null;
    }
    return d[0] + d[1] + d[2] + d[3] == 0 ? null : d;
}

static double Gauss(Random rng)
{
    double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}

static double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
}

static double Dot(double[] a, double[] b)
{
    double s = 0; for (int i = 0; i < a.Length; i++) s += a[i] * b[i]; return s;
}

static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));

static double[] Scale(double[] a, double s)
{
    var r = new double[a.Length];
    for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
    return r;
}

static double[] RandomVector(Random rng, int len, double scale)
{
    var v = new double[len];
    for (int i = 0; i < len; i++) v[i] = scale * (rng.NextDouble() - 0.5);
    return v;
}

static double? FiniteOrNull(double x) => double.IsFinite(x) ? x : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

BranchManifest BuildManifest() => new()
{
    BranchId = "phase452-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase452",
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

GeometryContext BuildGeometry()
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

// ---------------------------------------------------------------------------
// Records.
// ---------------------------------------------------------------------------

public sealed record ColumnSpec(int Index, string Member, bool IsControlMember, double Beta, string Kind,
    int NTraj, int NWarm, bool ProjectZeroModes, bool SampleTheta, int Seed);

public sealed record CorrelatorResult(double[] C, double[] Sigma, double[][] Jackknife);

public sealed record MeffPoint(int T, double Ratio, double? M, double? Sigma);

public sealed record MeffResult(MeffPoint[] Points);

public sealed record Phase456Measurement(double[] A2O1, double[] A2O2, double[][] KO1Re, double[][] KO1Im,
    double[] SiteO1, double Phi);

public sealed record SpatialMomentumCorrelator(int Kx, int Ky, int Kz, CorrelatorResult Correlator);

public sealed record GevpResult(double[] DominantEigenvalue, double[][] JackknifeDominantEigenvalue,
    MeffResult EffectiveMasses, bool C0PositiveDefinite, int T0);

public sealed record BinderResult(double BinderCumulant, double BinderSigma, double[] BinderJackknife,
    double Susceptibility, double SusceptibilitySigma, double[] SusceptibilityJackknife,
    double TauPhi, double NEffPhi);

public sealed record Phase456GaussianControl(int IndependentSamples,
    GevpResult IdentityIrrep2x2Gevp,
    CorrelatorResult A2O1, CorrelatorResult A2O2, MeffResult A2MeffO1, MeffResult A2MeffO2,
    CorrelatorResult KMinO1, MeffResult KMinMeffO1, BinderResult Binder);

public sealed record Phase456ColumnData(
    GevpResult IdentityIrrep2x2Gevp,
    CorrelatorResult A2O1, CorrelatorResult A2O2, MeffResult A2MeffO1, MeffResult A2MeffO2,
    CorrelatorResult KMinO1, MeffResult KMinMeffO1, SpatialMomentumCorrelator[] AllSpatialMomenta,
    BinderResult Binder,
    double TauA2, double NEffA2, double TauKMin, double NEffKMin,
    int[] A2KernelNumerators, int A2KernelDenominator,
    double InvariantRayProjectorOverlapMax,
    bool PerSiteCorrelatorStorage, bool PerFaceTypeResolutionRetained,
    double SpatialKMinReconstructionResidual, string StorageMeaning);

public sealed class ColumnResult
{
    public required ColumnSpec Spec { get; init; }
    public required int NDof { get; init; }
    public required int NZeroProjected { get; init; }
    public required double Eps { get; init; }
    public required double SigmaTheta { get; init; }
    public required double Acceptance { get; init; }
    public double? ThetaCollAcceptance { get; init; }
    public double? ThetaSiteAcceptance { get; init; }
    public required double ExpDHMean { get; init; }
    public required double ExpDHSigma { get; init; }
    public required bool DHGate { get; init; }
    public required double VirialMean { get; init; }
    public required double VirialSigma { get; init; }
    public required bool VirialGate { get; init; }
    public required double TauO1 { get; init; }
    public required double TauO2 { get; init; }
    public required double TauS { get; init; }
    public required double NEff { get; init; }
    public required bool NEffGate { get; init; }
    public required double SMean { get; init; }
    public required double O1Mean { get; init; }
    public required double O2Mean { get; init; }
    public required CorrelatorResult C1 { get; init; }
    public required CorrelatorResult C2 { get; init; }
    public required MeffResult Meff1 { get; init; }
    public required MeffResult Meff2 { get; init; }
    public Phase456ColumnData? Phase456 { get; init; }
    public required double MsPerTrajectory { get; init; }
}

public sealed class AnalyticData
{
    public required double[][] BlockEvals { get; init; }
    public required double[][,] BlockEvecsRe { get; init; }
    public required double[][,] BlockEvecsIm { get; init; }
    public required double ZeroTol { get; init; }
    public required double MaxAbsEig { get; init; }
    public required int NPos { get; init; }
    public required int NZero { get; init; }
    public required int NNeg { get; init; }
    public required double MinEig { get; init; }
    public required double[][,] CovO1 { get; init; }
    public required double[][,] CovO2 { get; init; }
    public required double[] C1An { get; init; }
    public required double[] C2An { get; init; }
    public required double Mean1An { get; init; }
    public required double Mean2An { get; init; }
    public required List<double[]> ZBasis { get; init; }
}

public sealed record MemberBattery(
    double ObjConsistency, double RotationInvariance, double IdentityThetaGradNorm,
    double IdentityThetaSDiff, double ExpLogResid, double HvThetaMax, double BlockResidual,
    double PlaneWaveResid, double CovImagMax, double ZGramResid, double ZHzResid);

public sealed record GaussSimResult(double WorstZO1, double WorstZO2, double SelfConjImagMax,
    double[] SimC1, double[] SimC2, Phase456GaussianControl? Phase456ExactControl);

public sealed record FreeFieldGate(string Member, double Beta, double? MAn1, double? MAn2,
    double? M1, double? S1, double? M2, double? S2, double? Z1, double? Z2, bool GateO1);

public sealed record MemberVerdict(string Member, bool IsControlMember, double Beta,
    double? M1, double? S1, double? Chi1, double? M2, double? S2, double? Chi2,
    double? CrossZ, bool CrossOk, bool StatsOk, string Verdict, string Reason);

public sealed record TorusRecord(int N, int Volume, int Edges, int FaceCountValue, int NOmega, int NTheta,
    bool OrbitMapOk, bool FaceOrbitsUniform, int FaceTypeCount, bool FaceBaseMatchesStored,
    int[] Informative, int[] Window, bool WindowIncludesT0,
    MemberBattery[] Batteries, AnalyticData[] Analytics, GaussSimResult[] GaussSim,
    (double? MO1, double? MO2)[] AnalyticMeff,
    List<ColumnResult> Columns, List<FreeFieldGate> FreeGates, bool FreeFieldGatePassed,
    List<MemberVerdict> Verdicts);
