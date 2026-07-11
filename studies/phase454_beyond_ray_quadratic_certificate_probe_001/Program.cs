using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase454: BEYOND-INVARIANT-RAY quadratic-certificate probe, Arms A/B (the
// sampling-free arms of Team B's Wave-1 rank-2 item in the committed
// three-team elimination program, docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md
// section 1 item 7). Arm C (structure factors on released sampling columns)
// is pre-registered inside phase453 and is NOT in scope here.
//
// THE B2 QUESTION AFTER PHASE453'S STAGE-0. The 443-453 line probed the scale
// question ON translation-invariant rays. Could a lower configuration hide
// OFF those rays? Arms A/B answer with exact certificates at quadratic and
// cubic order around a FINITE menu of audited backgrounds:
//
//   ARM A - positive-semidefiniteness on the gauge complement per momentum
//   sector. At the trivial background (omega=0, theta=0) and at the audited
//   invariant-ray backgrounds used by phases 448/450/453 (n=3 lattice-
//   canonical torus, theta* stationarity), the joint (omega,theta) Hessian
//   is block-circulant; its 48x48 Hermitian momentum blocks come from the 48
//   orbit-representative Hessian-vector products (the phase448 machinery,
//   copied verbatim). Per momentum sector the GAUGE KERNEL ker L = ker d is
//   RECOMPUTED PER BACKGROUND - d here is the background-covariant curvature
//   linearization D_{omega0} delta = d delta + [omega0 ^ delta], obtained
//   EXACTLY as a central difference of the exactly-quadratic discrete
//   curvature map (any step is exact; roundoff only), never reused from the
//   trivial background. Its kernel is projected out exactly (orthonormal
//   complement restriction, plain Euclidean coefficient inner product - the
//   recorded phase450 convention) and the complement block is eigensolved.
//   Any NEGATIVE complement direction is a NAMED beyond-ray instability
//   candidate (sector k, background, member, eigenvalue, direction profile);
//   all-nonnegative across all sectors and backgrounds is the quadratic
//   certificate that no beyond-ray descent direction exists at these
//   backgrounds. The projected-out kernel sub-block spectrum is ALSO
//   recorded (diagnostic; nothing is hidden by the projection).
//
//   ARM B - the directional-cubic a3(v) battery. On invariant rays a3 = 0
//   identically (the 2026-07-04 reviewed Z2 parity); OFF-ray that argument
//   fails. For a pre-registered deterministic menu of non-invariant
//   directions v on the lowest nonzero momentum shells (all 15 canonical
//   0/1 momenta of (Z_3)^4, cosine and sine waves, 45 single-type/Lie
//   coefficient sets plus the 3 audited ray coefficient sets), the exact
//   S_B(t v) = a2 t^2 + a3 t^3 + a4 t^4 (S_B is EXACTLY quartic in omega at
//   theta=0) is solved from evaluations at t = +-h, +-2h (h = 0.1, the
//   phase447-proven quartic-exact stencil scale), with an independent
//   +-0.15 odd-part cross-check, the 5-point third-derivative stencil, and
//   a t = 3h quartic-exactness gate. A direction can carry a degenerate
//   second S_B = 0 configuration on its ray iff a3^2 >= 4 a2 a4 (S_B >= 0
//   makes a strictly negative beyond-ray minimum impossible; degeneracy is
//   the only first-order-transition-style precondition); a3^2 < 4 a2 a4
//   everywhere on the menu is the cubic-order exclusion certificate.
//
// BATTERIES (fail-closed): S_B translation covariance; curvature face-space
// covariance (the oSign/fSign orientation discipline extended to faces);
// objective-path consistency; analytic-vs-FD gradient; H-block
// reconstruction vs direct Hv (the phase448 2.9e-11 discipline); D-block
// reconstruction vs direct curvature differences; theta* equivariance;
// identity theta-independence; gauge-kernel dimension asserts (252 = 12 at
// k=0 plus 3 per nonzero k at the n=3 trivial background - verified, and
// recomputed dimensions recorded per background); kernel spectral-gap
// cleanliness; eigenvector residuals; complement-partition trace
// consistency; PHASE448 LINEAGE (the full-block positive/zero/negative mode
// counts at the shared grid points must equal the committed phase448 values
// exactly); Arm B cross-checks, S(0) = 0, theta-flatness at the origin,
// positivity (a4 ~ 0 forces a3 ~ 0), and on-ray parity a3 = 0 controls.
// Seed-free determinism: the ONLY System.Random uses are the fixed-seed
// verbatim reproductions of the committed phase448/phase450 ray-coefficient
// conventions; every new menu object is closed-form deterministic.
//
// PRE-REGISTERED VERDICT TAXONOMY: beyond-ray-excluded-at-certified-
// backgrounds / beyond-ray-instability-candidate-found / inconclusive-
// gates-failed. SCOPE HONESTY: certificates at a FINITE background menu,
// n=3, quadratic+cubic order - this NARROWS B2 and feeds the closure
// theorem-of-record (program item 22); it does not close B2 alone. The
// phase PASSES on internal consistency REGARDLESS of which physics verdict
// the computation produces.
//
// MANDATORY FRAMING: workbench-relative structure data ONLY (su(2) toy
// algebra on the reduced Spin(4) slice, lattice units); the 445-452
// conventions carry physicistReviewPending (Wave-0 item 0.3 open, explicit);
// NO GeV/pole promotion; nothing fills a Phase201/Phase256 contract field.

const string DefaultOutputDir = "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output";
const string Phase448SummaryPath = "studies/phase448_torus_mode_volume_saturation_probe_001/output/torus_mode_volume_saturation_probe_summary.json";
const string Phase450SummaryPath = "studies/phase450_constraint_effective_potential_hmc_probe_001/output/constraint_effective_potential_hmc_probe_summary.json";
const string Phase453SummaryPath = "studies/phase453_wham_parity_error_model_repair_001/output/wham_parity_error_model_repair_summary.json";
const string ProgramDocPath = "docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md";
const string ApplicationSubjectKind = "beyond-ray-quadratic-certificate-probe";

const int TorusN = 3;
const int Phase448RngSeed = 20260704;    // phase448 ray convention (verbatim)
const int Phase450RayRngSeed = 20260703; // phase450 ray convention (verbatim)
const int GridN448 = 16;                 // phase448 tGrid (verbatim walk for theta* lineage)
const double TMax448 = 3.0;

const double HvStep = 1e-5;               // phase448 Hessian-vector convention (verbatim)
const double ZeroModeRelTol = 1e-8;       // phase448 IR convention (verbatim)
const double ZeroModeAbsFloor = 1e-10;
const double CovarianceGate = 1e-10;
const double FaceCovarianceGate = 1e-9;
const double ObjectiveConsistencyGate = 1e-10;
const double GradFdGate = 1e-6;
const double HBlockReconstructGate = 1e-4;   // FD-noise-limited (phase448: 2.9e-11 measured)
const double DBlockReconstructGate = 1e-9;   // exact-linear map, roundoff only
const double ThetaTolRel = 1e-9;             // phase448 theta* conventions (verbatim)
const double ThetaResidualBatteryRel = 1e-8;
const double ThetaAbsGradFloor = 1e-10;
const double EquivarianceGate = 1e-8;
const double KernelRelTol = 1e-10;           // mu <= tol * muMax counts as kernel of D^dag D
const double KernelGapMin = 1e4;             // min kept mu / max dropped mu
const double KernelEigResidualGate = 1e-8;
const double TracePartitionGate = 1e-8;
const double ArmBStep = 0.1;                 // phase447-proven quartic-exact stencil scale
const double ArmBCrossStep = 0.15;           // independent odd-part cross-check abscissa
const double ArmBQuarticExactGate = 1e-9;    // relative, at the held-out t = 3h point
const double ArmBCrossRel = 1e-9;            // cross-estimator agreement (relative to max(a2,a4,1e-6))
const double ArmBA3ZeroRel = 1e-10;          // |a3| <= rel * max(a2,a4) counts as zero

int expectedKernelTrivialTotal = 252;        // 3*(V - 1 + b1) = 3*(81 - 1 + 4) at n=3
int expectedKernelTrivialK0 = 12;            // harmonic sector (b1=4 x su(2))
int expectedKernelTrivialNonzeroK = 3;       // exact sector (3 per nonzero momentum)

var outputDir = Environment.GetEnvironmentVariable("PHASE454_OUTPUT_DIR") ?? DefaultOutputDir;
int maxDop = int.TryParse(Environment.GetEnvironmentVariable("PHASE454_MAXDOP"), out int md) ? md : 4;
Directory.CreateDirectory(outputDir);
var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDop };

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase448 = JsonDocument.Parse(File.ReadAllText(Phase448SummaryPath));
bool phase448PrecursorPassed =
    JsonBool(phase448.RootElement, "torusModeVolumeSaturationProbePassed") is true &&
    JsonString(phase448.RootElement, "modeVolumeVerdict") == "no-saturation-persists-across-mode-volumes";

using var phase450 = JsonDocument.Parse(File.ReadAllText(Phase450SummaryPath));
bool phase450PrecursorPassed = JsonBool(phase450.RootElement, "constraintEffectivePotentialHmcProbePassed") is true;

using var phase453 = JsonDocument.Parse(File.ReadAllText(Phase453SummaryPath));
bool phase453PrecursorPassed = JsonBool(phase453.RootElement, "whamParityErrorModelRepairPassed") is true;

bool precursorsPassed = phase448PrecursorPassed && phase450PrecursorPassed && phase453PrecursorPassed;

// Committed phase448 n=3 mode counts (the lineage battery comparands).
var committed448 = new Dictionary<(string Member, int Seed, double T), (int Pos, int Zero, int Neg)>();
foreach (var vol in phase448.RootElement.GetProperty("volumes").EnumerateArray())
{
    if (vol.GetProperty("torusN").GetInt32() != TorusN) continue;
    foreach (var mem in vol.GetProperty("members").EnumerateArray())
    {
        string mName = mem.GetProperty("member").GetString()!;
        foreach (var sd in mem.GetProperty("seeds").EnumerateArray())
        {
            int seed = sd.GetProperty("seed").GetInt32();
            foreach (var pt in sd.GetProperty("points").EnumerateArray())
            {
                committed448[(mName, seed, pt.GetProperty("t").GetDouble())] = (
                    pt.GetProperty("positiveModes").GetInt32(),
                    pt.GetProperty("zeroModes").GetInt32(),
                    pt.GetProperty("negativeModes").GetInt32());
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Geometry, orbit machinery (phase448 verbatim), NEW face-orbit machinery.
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();
int n = TorusN;

var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;
int nJoint = nOmega + nTheta;
int volume = mesh.VertexCount;

var stopwatch = Stopwatch.StartNew();

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

// --- Faces: canonical monotone-chain orbit coordinates + orientation gauge.
// A Kuhn 2-face is a chain (base, base+d1, base+d1+d2) with d1, d2 nonzero
// 0/1 displacements whose sum is still 0/1. fSign converts the STORED
// (index-ordered) face orientation to the canonical chain orientation - the
// face-space analogue of the oSign lattice-gauge lesson.
var faceBase = new int[mesh.FaceCount];
var faceTypeOf = new int[mesh.FaceCount];
var fSign = new double[mesh.FaceCount];
var faceTypeIds = new Dictionary<int, int>();
bool faceMapOk = true;
int[][] perms = { new[] { 0, 1, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 2, 1, 0 } };
double[] permSign = { 1.0, 1.0, 1.0, -1.0, -1.0, -1.0 };
for (int f = 0; f < mesh.FaceCount; f++)
{
    var verts = mesh.Faces[f];
    bool found = false;
    for (int pi = 0; pi < perms.Length && !found; pi++)
    {
        int a = verts[perms[pi][0]], b = verts[perms[pi][1]], c = verts[perms[pi][2]];
        var d1 = MinImage01(coords[a], coords[b], n);
        if (d1 == null) continue;
        var d2 = MinImage01(coords[b], coords[c], n);
        if (d2 == null) continue;
        var dc = new int[4];
        bool ok = true;
        for (int i = 0; i < 4; i++) { dc[i] = d1[i] + d2[i]; if (dc[i] > 1) ok = false; }
        if (!ok) continue;
        var dcCheck = MinImage01(coords[a], coords[c], n);
        if (dcCheck == null) continue;
        for (int i = 0; i < 4; i++) if (dcCheck[i] != dc[i]) ok = false;
        if (!ok) continue;
        int key = TypeOf(d1) * 15 + TypeOf(d2);
        if (!faceTypeIds.TryGetValue(key, out int tid)) { tid = faceTypeIds.Count; faceTypeIds[key] = tid; }
        faceBase[f] = a;
        faceTypeOf[f] = tid;
        fSign[f] = permSign[pi];
        found = true;
    }
    if (!found) faceMapOk = false;
}
int nFaceTypes = faceTypeIds.Count;
bool faceTypeCountConsistent = nFaceTypes * volume == mesh.FaceCount;
var faceAt = new int[mesh.VertexCount, System.Math.Max(nFaceTypes, 1)];
{
    for (int v = 0; v < mesh.VertexCount; v++) for (int t = 0; t < nFaceTypes; t++) faceAt[v, t] = -1;
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        if (faceAt[faceBase[f], faceTypeOf[f]] >= 0) faceMapOk = false;
        faceAt[faceBase[f], faceTypeOf[f]] = f;
    }
    for (int v = 0; v < mesh.VertexCount && faceMapOk; v++)
        for (int t = 0; t < nFaceTypes; t++)
            if (faceAt[v, t] < 0) faceMapOk = false;
}
int nFaceDof = mesh.FaceCount * dimG;
int nFaceRows = nFaceTypes * dimG;

// Translation of a joint (omega,theta) vector by lattice vector a (phase448 verbatim).
double[] Translate(double[] x, int[] a)
{
    var y = new double[nJoint];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        var c0 = coords[edgeBase[e]];
        int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
        int te = edgeAt[tv, edgeType[e]];
        double sgn = oSign[e] * oSign[te];
        for (int l = 0; l < dimG; l++) y[te * dimG + l] = sgn * x[e * dimG + l];
    }
    for (int v = 0; v < mesh.VertexCount; v++)
    {
        var c0 = coords[v];
        int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
        for (int l = 0; l < dimG; l++) y[nOmega + tv * dimG + l] = x[nOmega + v * dimG + l];
    }
    return y;
}

// Translation of a face-space vector (canonical fSign gauge at the boundary).
double[] TranslateFace(double[] x, int[] a)
{
    var y = new double[nFaceDof];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        var c0 = coords[faceBase[f]];
        int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
        int tf = faceAt[tv, faceTypeOf[f]];
        double sgn = fSign[f] * fSign[tf];
        for (int l = 0; l < dimG; l++) y[tf * dimG + l] = sgn * x[f * dimG + l];
    }
    return y;
}

// Exact discrete curvature coefficients (face-major).
double[] Curv(double[] omega) =>
    CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;

// ---------------------------------------------------------------------------
// Members (phase448 verbatim family specs).
// ---------------------------------------------------------------------------

var controlSpec = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = "trivial",
};
var memberSpecs = new List<(string Name, EinsteinianShiabFamilyMember Spec, bool IsControl)>
{
    ("identity", controlSpec, true),
};
foreach (var (phi1, tag) in new[] { (InvariantElementSpec.Sd2, "sd2-id0"), (InvariantElementSpec.Asd2, "asd2-id0") })
{
    memberSpecs.Add(($"{tag}/c0.5", new EinsteinianShiabFamilyMember
    {
        Phi1 = phi1, Phi2 = InvariantElementSpec.Id0, EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
    }, false));
}
EinsteinianShiabOperator MakeOp(EinsteinianShiabFamilyMember spec) =>
    new(mesh, algebra, spec, latticePeriod: n);
var sharedOps = memberSpecs.Select(m => MakeOp(m.Spec)).ToArray();
var sharedMass = new CpuMassMatrix(mesh, algebra);

double SBOf(EinsteinianShiabOperator op, CpuMassMatrix mass, double[] omega, double[] theta) =>
    op.ComputeJointGradient(omega, theta, mass).Objective;

// Direct-path S_B (Evaluate / EvaluateWithTheta route; certified against the
// gradient-path objective by the consistency battery below).
double SBDirect(EinsteinianShiabOperator op, CpuMassMatrix mass, bool isControl, double[] omega, double[] theta)
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    var sTensor = FaceTensor(mesh, algebra, (isControl
        ? op.Evaluate(f, conn.ToFieldTensor(), manifest, geometry)
        : op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry)).Coefficients);
    return 0.5 * mass.InnerProduct(sTensor, sTensor);
}

// ---------------------------------------------------------------------------
// Deterministic (RNG-free) test vectors and fixed shifts for the batteries.
// ---------------------------------------------------------------------------

double[] DetVector(int len, int salt)
{
    var v = new double[len];
    for (int i = 0; i < len; i++)
    {
        double x = System.Math.Sin((i + 1) * 12.9898 + salt * 78.233) * 43758.5453;
        v[i] = x - System.Math.Floor(x) - 0.5;
    }
    double nn = Norm(v);
    for (int i = 0; i < len; i++) v[i] /= nn;
    return v;
}
int[][] fixedShifts = { new[] { 1, 0, 0, 0 }, new[] { 0, 1, 2, 0 }, new[] { 2, 2, 1, 1 } };

// ---------------------------------------------------------------------------
// Core batteries: S_B covariance, objective consistency, gradient FD,
// identity theta-independence, curvature face-space covariance.
// ---------------------------------------------------------------------------

double covarianceMax = 0.0, objConsistencyMax = 0.0, gradFdMax = 0.0;
for (int mi = 0; mi < memberSpecs.Count; mi++)
{
    var (name, spec, isControl) = memberSpecs[mi];
    var op = sharedOps[mi];
    var xw = DetVector(nOmega, 11 + mi);
    var xt = DetVector(nTheta, 41 + mi);
    double s0 = SBOf(op, sharedMass, xw, xt);
    objConsistencyMax = System.Math.Max(objConsistencyMax,
        RelDiff(s0, SBDirect(op, sharedMass, isControl, xw, xt)));
    var xj = new double[nJoint];
    Array.Copy(xw, 0, xj, 0, nOmega); Array.Copy(xt, 0, xj, nOmega, nTheta);
    foreach (var a in fixedShifts)
    {
        var y = Translate(xj, a);
        double sT = SBOf(op, sharedMass, y.Take(nOmega).ToArray(), y.Skip(nOmega).ToArray());
        covarianceMax = System.Math.Max(covarianceMax, RelDiff(s0, sT));
    }
    var g = op.ComputeJointGradient(xw, xt, sharedMass);
    foreach (int i in new[] { 7, nOmega / 2, nOmega - 3, nOmega + 5, nJoint - 2 })
    {
        double h = 1e-6;
        var xp = (double[])xj.Clone(); xp[i] += h;
        var xm = (double[])xj.Clone(); xm[i] -= h;
        double fd = (SBOf(op, sharedMass, xp.Take(nOmega).ToArray(), xp.Skip(nOmega).ToArray())
                   - SBOf(op, sharedMass, xm.Take(nOmega).ToArray(), xm.Skip(nOmega).ToArray())) / (2 * h);
        double an = i < nOmega ? g.GradOmega[i] : g.GradTheta[i - nOmega];
        gradFdMax = System.Math.Max(gradFdMax,
            System.Math.Abs(fd - an) / System.Math.Max(1.0, System.Math.Abs(fd)));
    }
}
double identityThetaGradNorm;
{
    var g = sharedOps[0].ComputeJointGradient(DetVector(nOmega, 5), DetVector(nTheta, 6), sharedMass);
    identityThetaGradNorm = Norm(g.GradTheta);
}

double faceCovarianceMax = 0.0;
{
    var xw = DetVector(nOmega, 77);
    var f0 = Curv(xw);
    var xj = new double[nJoint];
    Array.Copy(xw, 0, xj, 0, nOmega);
    foreach (var a in fixedShifts)
    {
        var y = Translate(xj, a).Take(nOmega).ToArray();
        var f1 = Curv(y);
        var f0T = TranslateFace(f0, a);
        double num = 0, den = 0;
        for (int i = 0; i < nFaceDof; i++)
        {
            num += (f1[i] - f0T[i]) * (f1[i] - f0T[i]);
            den += f1[i] * f1[i];
        }
        faceCovarianceMax = System.Math.Max(faceCovarianceMax,
            System.Math.Sqrt(num) / System.Math.Max(1e-30, System.Math.Sqrt(den)));
    }
}

// ---------------------------------------------------------------------------
// Audited ray sets (fixed-seed verbatim reproductions of committed conventions).
// ---------------------------------------------------------------------------

double[] BuildTypeCoeffs(int seed)
{
    var rng = new Random(seed);
    var c = new double[16 * dimG];
    for (int i = 0; i < c.Length; i++) c[i] = rng.NextDouble() - 0.5;
    return c;
}
double[] RayFromTypeCoeffs(double[] c)
{
    var u = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
            u[e * dimG + l] = oSign[e] * c[edgeType[e] * dimG + l];
    double un = Norm(u);
    for (int i = 0; i < nOmega; i++) u[i] /= un;
    return u;
}

var rayCoeff448S0 = BuildTypeCoeffs(Phase448RngSeed + 31 * 0);
var rayCoeff448S1 = BuildTypeCoeffs(Phase448RngSeed + 31 * 1);
var rayCoeff450 = BuildTypeCoeffs(Phase450RayRngSeed + 31 * 0);
var ray448S0 = RayFromTypeCoeffs(rayCoeff448S0);
var ray448S1 = RayFromTypeCoeffs(rayCoeff448S1);

// phase450 verbatim: unit-normalize, then Gram-Schmidt-project the 3 global
// su(2) zero-mode directions z_l = [e_l, u] (machine-zero overlaps, applied anyway).
double projectorOverlapMaxAfter;
double[] ray450;
{
    var u = RayFromTypeCoeffs(rayCoeff450);
    var zOrtho = new List<double[]>();
    for (int l = 0; l < dimG; l++)
    {
        var z = new double[nOmega];
        var basis = new double[dimG]; basis[l] = 1.0;
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var ue = new double[dimG];
            for (int c = 0; c < dimG; c++) ue[c] = u[e * dimG + c];
            var br = algebra.Bracket(basis, ue);
            for (int c = 0; c < dimG; c++) z[e * dimG + c] = br[c];
        }
        double zn = Norm(z);
        if (zn > 0) for (int i = 0; i < nOmega; i++) z[i] /= zn;
        var w = (double[])z.Clone();
        foreach (var q in zOrtho) { double d = Dot(w, q); for (int i = 0; i < nOmega; i++) w[i] -= d * q[i]; }
        double wn = Norm(w);
        if (wn > 1e-12) { for (int i = 0; i < nOmega; i++) w[i] /= wn; zOrtho.Add(w); }
    }
    foreach (var q in zOrtho) { double d = Dot(u, q); for (int i = 0; i < nOmega; i++) u[i] -= d * q[i]; }
    double un = Norm(u);
    for (int i = 0; i < nOmega; i++) u[i] /= un;
    projectorOverlapMaxAfter = zOrtho.Count > 0 ? zOrtho.Max(q => System.Math.Abs(Dot(u, q))) : 0.0;
    ray450 = u;
}

double[] tGrid448 = new double[GridN448];
for (int i = 0; i < GridN448; i++) tGrid448[i] = TMax448 * (i + 1) / GridN448;
int[] selected448Indices = { 0, 4, 8, 15 };
double[] tMenu450 = { 0.5, 1.25, 2.375 }; // spans the phase450/453 window range incl. the antisymmetry-max bin

// ---------------------------------------------------------------------------
// Momentum-sector scaffolding (phase448 pair enumeration, verbatim order).
// ---------------------------------------------------------------------------

int nTypes = 16;
int blockDim = nTypes * dimG; // 48
var wsAll = new List<int[]>();
for (int w0 = 0; w0 < n; w0++) for (int w1 = 0; w1 < n; w1++)
for (int w2 = 0; w2 < n; w2++) for (int w3 = 0; w3 < n; w3++)
    wsAll.Add(new[] { w0, w1, w2, w3 });
int KeyOf(int[] k) => ((k[0] * n + k[1]) * n + k[2]) * n + k[3];
var pairList = new List<(int[] K, bool SelfConj)>();
{
    var seenK = new HashSet<int>();
    foreach (var k in wsAll)
    {
        int key = KeyOf(k);
        if (seenK.Contains(key)) continue;
        var kNeg = new[] { Mod(-k[0], n), Mod(-k[1], n), Mod(-k[2], n), Mod(-k[3], n) };
        seenK.Add(key); seenK.Add(KeyOf(kNeg));
        pairList.Add((k, key == KeyOf(kNeg)));
    }
}

// ---------------------------------------------------------------------------
// ARM A machinery.
// ---------------------------------------------------------------------------

// 45 orbit-representative curvature-linearization columns at background omega0
// (canonical face gauge). D_{omega0} is exactly linear-in-delta of an exactly
// quadratic map, so one central difference is EXACT (roundoff only).
double[][] BuildDColumns(double[] omega0)
{
    var cols = new double[15 * dimG][];
    double h = 0.5;
    for (int ty = 0; ty < 15; ty++)
        for (int l = 0; l < dimG; l++)
        {
            var vO = new double[nOmega];
            int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
            vO[e0 * dimG + l] = oSign[e0];
            var wp = new double[nOmega];
            var wm = new double[nOmega];
            for (int i = 0; i < nOmega; i++) { wp[i] = omega0[i] + h * vO[i]; wm[i] = omega0[i] - h * vO[i]; }
            var fp = Curv(wp);
            var fm = Curv(wm);
            var col = new double[nFaceDof];
            for (int f = 0; f < mesh.FaceCount; f++)
                for (int ll = 0; ll < dimG; ll++)
                    col[f * dimG + ll] = fSign[f] * (fp[f * dimG + ll] - fm[f * dimG + ll]) / (2.0 * h);
            cols[ty * dimG + l] = col;
        }
    return cols;
}

// D-block reconstruction battery: D*x from translated canonical columns vs a
// direct exact central difference, at a given background.
double DBlockReconstruct(double[] omega0, double[][] colsD, int salt)
{
    var x = DetVector(nOmega, salt);
    double h = 0.5;
    var wp = new double[nOmega];
    var wm = new double[nOmega];
    for (int i = 0; i < nOmega; i++) { wp[i] = omega0[i] + h * x[i]; wm[i] = omega0[i] - h * x[i]; }
    var fp = Curv(wp);
    var fm = Curv(wm);
    var direct = new double[nFaceDof];
    for (int f = 0; f < mesh.FaceCount; f++)
        for (int l = 0; l < dimG; l++)
            direct[f * dimG + l] = fSign[f] * (fp[f * dimG + l] - fm[f * dimG + l]) / (2.0 * h);
    var recon = new double[nFaceDof];
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
        {
            double weight = oSign[e] * x[e * dimG + l];
            if (weight == 0.0) continue;
            var site = coords[edgeBase[e]];
            var col = colsD[edgeType[e] * dimG + l];
            for (int f = 0; f < mesh.FaceCount; f++)
            {
                var c0 = coords[faceBase[f]];
                int sv = vertexAt[(Mod(c0[0] - site[0], n), Mod(c0[1] - site[1], n), Mod(c0[2] - site[2], n), Mod(c0[3] - site[3], n))];
                int sf = faceAt[sv, faceTypeOf[f]];
                for (int ll = 0; ll < dimG; ll++)
                    recon[f * dimG + ll] += weight * col[sf * dimG + ll];
            }
        }
    double num = 0, den = 0;
    for (int i = 0; i < nFaceDof; i++)
    {
        num += (recon[i] - direct[i]) * (recon[i] - direct[i]);
        den += direct[i] * direct[i];
    }
    return System.Math.Sqrt(num) / System.Math.Max(1e-30, System.Math.Sqrt(den));
}

// Sector kernels of D at a background: per pair, the complex kernel basis
// (padded to the 48-block layout / its 96 real embedding), its dimension,
// the spectral-gap statistics, and the cached orthonormal complement basis.
List<SectorKernel> BuildSectorKernels(double[] omega0)
{
    var colsD = BuildDColumns(omega0);
    double KernD(int[] w, int ftype, int lR, int colIdx)
        => colsD[colIdx][faceAt[vertexAt[(w[0], w[1], w[2], w[3])], ftype] * dimG + lR];

    var kernels = new SectorKernel[pairList.Count];
    Parallel.For(0, pairList.Count, parallelOptions, pi =>
    {
        var (k, selfConj) = pairList[pi];
        int nCols = 15 * dimG; // 45 omega columns
        var dre = new double[nFaceRows, nCols];
        var dim = new double[nFaceRows, nCols];
        foreach (var w in wsAll)
        {
            double phase = -2.0 * System.Math.PI * (k[0] * w[0] + k[1] * w[1] + k[2] * w[2] + k[3] * w[3]) / n;
            double cph = System.Math.Cos(phase), sph = System.Math.Sin(phase);
            for (int col = 0; col < nCols; col++)
                for (int ftype = 0; ftype < nFaceTypes; ftype++)
                    for (int lR = 0; lR < dimG; lR++)
                    {
                        double cv = KernD(w, ftype, lR, col);
                        int row = ftype * dimG + lR;
                        dre[row, col] += cph * cv;
                        dim[row, col] += sph * cv;
                    }
        }
        // M = D^dag D (45x45 complex Hermitian).
        var mre = new double[nCols, nCols];
        var mim = new double[nCols, nCols];
        for (int i = 0; i < nCols; i++)
            for (int j = 0; j < nCols; j++)
            {
                double sre = 0, sim = 0;
                for (int r = 0; r < nFaceRows; r++)
                {
                    sre += dre[r, i] * dre[r, j] + dim[r, i] * dim[r, j];
                    sim += dre[r, i] * dim[r, j] - dim[r, i] * dre[r, j];
                }
                mre[i, j] = sre; mim[i, j] = sim;
            }

        double[,] mSolve;
        int solveDim;
        if (selfConj)
        {
            mSolve = mre; solveDim = nCols;
        }
        else
        {
            solveDim = 2 * nCols;
            mSolve = new double[solveDim, solveDim];
            for (int i = 0; i < nCols; i++)
                for (int j = 0; j < nCols; j++)
                {
                    mSolve[i, j] = mre[i, j];
                    mSolve[i + nCols, j + nCols] = mre[i, j];
                    mSolve[i, j + nCols] = -mim[i, j];
                    mSolve[i + nCols, j] = mim[i, j];
                }
        }
        var (mu, vec) = JacobiEigenSystem(mSolve);
        double muMax = mu.Max(System.Math.Abs);
        double kerTol = KernelRelTol * System.Math.Max(muMax, 1e-30);
        var kerIdx = new List<int>();
        double maxDropped = 0.0, minKept = double.MaxValue;
        for (int i = 0; i < solveDim; i++)
        {
            if (mu[i] <= kerTol) { kerIdx.Add(i); maxDropped = System.Math.Max(maxDropped, System.Math.Abs(mu[i])); }
            else minKept = System.Math.Min(minKept, mu[i]);
        }
        bool gapOk = kerIdx.Count == solveDim
            || maxDropped <= 1e-14 * System.Math.Max(muMax, 1e-30)
            || minKept / System.Math.Max(maxDropped, 1e-300) >= KernelGapMin;
        // Eigenvector residual battery on the kernel vectors.
        double eigResidual = 0.0;
        foreach (int i in kerIdx)
        {
            double rr = 0.0;
            for (int r = 0; r < solveDim; r++)
            {
                double s = 0.0;
                for (int c = 0; c < solveDim; c++) s += mSolve[r, c] * vec[c][i];
                s -= mu[i] * vec[r][i];
                rr += s * s;
            }
            eigResidual = System.Math.Max(eigResidual,
                System.Math.Sqrt(rr) / System.Math.Max(muMax, 1e-30));
        }
        bool evenOk = selfConj || kerIdx.Count % 2 == 0;
        int dimC = selfConj ? kerIdx.Count : kerIdx.Count / 2;

        // Pad kernel vectors into the H-block layout (45 omega -> 48 with 3
        // theta zeros; embedded 90 -> 96).
        int hDim = selfConj ? blockDim : 2 * blockDim;
        var padded = new List<double[]>();
        foreach (int i in kerIdx)
        {
            var pv = new double[hDim];
            for (int j = 0; j < solveDim; j++)
            {
                int target = selfConj
                    ? j
                    : (j < nCols ? j : blockDim + (j - nCols));
                pv[target] = vec[j][i];
            }
            padded.Add(pv);
        }
        var q = ComplementBasis(hDim, padded);
        kernels[pi] = new SectorKernel(k, selfConj, dimC, kerIdx.Count, gapOk, evenOk,
            maxDropped, minKept == double.MaxValue ? double.NaN : minKept, eigResidual,
            padded, q.Basis, q.Ok);
    });
    return kernels.ToList();
}

// H-block assembly at (member, omega, theta): the phase448 verbatim column
// construction, then per-pair block build + full spectrum (verbatim path),
// plus the complement / kernel-restricted spectra against a sector kernel set.
BackgroundMemberResult ComputeMemberPoint(
    int memberIndex, double[] omega, double[] theta, List<SectorKernel> kernels,
    string backgroundId, double tValue)
{
    var spec = memberSpecs[memberIndex].Spec;
    var cols = new double[blockDim][];
    Parallel.For(0, blockDim, parallelOptions,
        () => (Op: MakeOp(spec), Mass: new CpuMassMatrix(mesh, algebra)),
        (ci, _, local) =>
        {
            int ty = ci / dimG, l = ci % dimG;
            var vO = new double[nOmega];
            var vT = new double[nTheta];
            if (ty < 15)
            {
                int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
                vO[e0 * dimG + l] = oSign[e0];
            }
            else vT[vertexAt[(0, 0, 0, 0)] * dimG + l] = 1.0;
            var (hvO, hvT) = local.Op.JointHessianVectorProduct(omega, theta, vO, vT, local.Mass, HvStep);
            var col = new double[nJoint];
            for (int e = 0; e < mesh.EdgeCount; e++)
                for (int ll = 0; ll < dimG; ll++)
                    col[e * dimG + ll] = oSign[e] * hvO[e * dimG + ll];
            Array.Copy(hvT, 0, col, nOmega, nTheta);
            cols[ci] = col;
            return local;
        },
        _ => { });

    double Kern(int[] w, int tyR, int lR, int colIdx)
    {
        int v = vertexAt[(w[0], w[1], w[2], w[3])];
        return tyR < 15
            ? cols[colIdx][edgeAt[v, tyR] * dimG + lR]
            : cols[colIdx][nOmega + v * dimG + lR];
    }

    var perPair = new SectorSpectra[pairList.Count];
    Parallel.For(0, pairList.Count, parallelOptions, pi =>
    {
        var (k, selfConj) = pairList[pi];
        var re = new double[blockDim, blockDim];
        var im = new double[blockDim, blockDim];
        foreach (var w in wsAll)
        {
            double phase = -2.0 * System.Math.PI * (k[0] * w[0] + k[1] * w[1] + k[2] * w[2] + k[3] * w[3]) / n;
            double cph = System.Math.Cos(phase), sph = System.Math.Sin(phase);
            for (int col = 0; col < blockDim; col++)
                for (int tyR = 0; tyR < nTypes; tyR++)
                    for (int lR = 0; lR < dimG; lR++)
                    {
                        double cv = Kern(w, tyR, lR, col);
                        int row = tyR * dimG + lR;
                        re[row, col] += cph * cv;
                        im[row, col] += sph * cv;
                    }
        }
        for (int i = 0; i < blockDim; i++)
            for (int j = 0; j < blockDim; j++)
            {
                double reS = 0.5 * (re[i, j] + re[j, i]);
                double imS = 0.5 * (im[i, j] - im[j, i]);
                re[i, j] = reS; re[j, i] = reS;
                im[i, j] = imS; im[j, i] = -imS;
            }

        double[,] hFull;
        if (selfConj)
        {
            hFull = re;
        }
        else
        {
            int d2 = 2 * blockDim;
            hFull = new double[d2, d2];
            for (int i = 0; i < blockDim; i++)
                for (int j = 0; j < blockDim; j++)
                {
                    hFull[i, j] = re[i, j];
                    hFull[i + blockDim, j + blockDim] = re[i, j];
                    hFull[i, j + blockDim] = -im[i, j];
                    hFull[i + blockDim, j] = im[i, j];
                }
        }
        var fullEigs = JacobiEigenvalues(hFull); // phase448-verbatim eigen path

        var kern = kernels[pi];
        // Complement restriction Hc = Q^T H Q.
        var q = kern.Complement;
        int nc = q.Count;
        var hq = new double[nc][];
        int hDim = selfConj ? blockDim : 2 * blockDim;
        for (int j = 0; j < nc; j++)
        {
            var col = new double[hDim];
            for (int r = 0; r < hDim; r++)
            {
                double s = 0.0;
                for (int c = 0; c < hDim; c++) s += hFull[r, c] * q[j][c];
                col[r] = s;
            }
            hq[j] = col;
        }
        var hc = new double[nc, nc];
        for (int i = 0; i < nc; i++)
            for (int j = 0; j < nc; j++)
            {
                double s = 0.0;
                for (int r = 0; r < hDim; r++) s += q[i][r] * hq[j][r];
                hc[i, j] = s;
            }
        var (compEigs, compVecs) = JacobiEigenSystem(hc);

        // Kernel-restricted diagnostic Hk = K^T H K.
        var kb = kern.PaddedKernel;
        int nk = kb.Count;
        var hkEigs = Array.Empty<double>();
        if (nk > 0)
        {
            var hk = new double[nk, nk];
            for (int i = 0; i < nk; i++)
                for (int j = 0; j < nk; j++)
                {
                    double s = 0.0;
                    for (int r = 0; r < hDim; r++)
                    {
                        double hr = 0.0;
                        for (int c = 0; c < hDim; c++) hr += hFull[r, c] * kb[j][c];
                        s += kb[i][r] * hr;
                    }
                    hk[i, j] = s;
                }
            (hkEigs, _) = JacobiEigenSystem(hk);
        }

        // Partition-trace battery: tr(H) = tr(Q^T H Q) + tr(K^T H K).
        double trFull = 0.0;
        for (int i = 0; i < hDim; i++) trFull += hFull[i, i];
        double trPart = 0.0;
        for (int i = 0; i < nc; i++) trPart += hc[i, i];
        foreach (double ev in hkEigs) trPart += ev;
        double traceResidual = System.Math.Abs(trFull - trPart) /
            System.Math.Max(1.0, System.Math.Abs(trFull));

        perPair[pi] = new SectorSpectra(k, selfConj, fullEigs, compEigs, compVecs, hkEigs, traceResidual, q);
    });

    // Aggregate with the phase448 verbatim IR/zero-mode convention.
    var allEigs = new List<double>(nJoint);
    foreach (var sp in perPair) allEigs.AddRange(sp.FullEigs);
    double maxAbs = 0.0;
    foreach (double e in allEigs) maxAbs = System.Math.Max(maxAbs, System.Math.Abs(e));
    double zeroTol = System.Math.Max(ZeroModeAbsFloor, ZeroModeRelTol * maxAbs);
    int pos = 0, neg = 0, zero = 0;
    foreach (double e in allEigs)
    {
        if (e > zeroTol) pos++;
        else if (e < -zeroTol) neg++;
        else zero++;
    }

    int cPos = 0, cNeg = 0, cZero = 0, kPos = 0, kNeg = 0, kZero = 0;
    double minComplementEig = double.MaxValue, minKernelEig = double.MaxValue;
    double tracePartitionWorst = 0.0;
    var candidates = new List<NegativeCandidate>();
    foreach (var sp in perPair)
    {
        tracePartitionWorst = System.Math.Max(tracePartitionWorst, sp.TraceResidual);
        for (int i = 0; i < sp.CompEigs.Length; i++)
        {
            double e = sp.CompEigs[i];
            minComplementEig = System.Math.Min(minComplementEig, e);
            if (e > zeroTol) cPos++;
            else if (e < -zeroTol)
            {
                cNeg++;
                // Name the candidate: sector, eigenvalue, direction profile.
                int hDim = sp.SelfConj ? blockDim : 2 * blockDim;
                var x = new double[hDim];
                for (int r = 0; r < hDim; r++)
                {
                    double s = 0.0;
                    for (int j = 0; j < sp.Complement.Count; j++) s += sp.Complement[j][r] * sp.CompVecs[j][i];
                    x[r] = s;
                }
                double thetaW = 0.0, totW = 0.0;
                var typeW = new double[nTypes];
                for (int ty = 0; ty < nTypes; ty++)
                    for (int l = 0; l < dimG; l++)
                    {
                        int idx = ty * dimG + l;
                        double w2 = x[idx] * x[idx];
                        if (!sp.SelfConj) w2 += x[idx + blockDim] * x[idx + blockDim];
                        typeW[ty] += w2;
                        totW += w2;
                        if (ty == 15) thetaW += w2;
                    }
                var top = typeW.Select((w, ty) => (ty, w)).OrderByDescending(p => p.w).Take(3)
                    .Select(p => new { edgeOrThetaType = p.ty, weight = p.w / System.Math.Max(totW, 1e-30) })
                    .ToArray();
                candidates.Add(new NegativeCandidate(
                    backgroundId, memberSpecs[memberIndex].Name, tValue,
                    $"{sp.K[0]}{sp.K[1]}{sp.K[2]}{sp.K[3]}", e,
                    thetaW / System.Math.Max(totW, 1e-30),
                    top.Select(t => (t.edgeOrThetaType, t.weight)).ToArray()));
            }
            else cZero++;
        }
        foreach (double e in sp.KernEigs)
        {
            minKernelEig = System.Math.Min(minKernelEig, e);
            if (e > zeroTol) kPos++;
            else if (e < -zeroTol) kNeg++;
            else kZero++;
        }
    }
    return new BackgroundMemberResult(
        memberSpecs[memberIndex].Name, backgroundId, tValue,
        pos, zero, neg, maxAbs, zeroTol,
        cPos, cZero, cNeg, minComplementEig == double.MaxValue ? double.NaN : minComplementEig,
        kPos, kZero, kNeg, minKernelEig == double.MaxValue ? double.NaN : minKernelEig,
        tracePartitionWorst, candidates);
}

// theta* projected Newton in the invariant sector (phase448 verbatim), one
// step of the warm-started grid walk; returns updated invariant theta and
// the residual/equivariance battery values at this point.
(double[] ThetaInv, double ThetaResidRel, double EquivResid, bool ThetaOk) ThetaStar(
    EinsteinianShiabOperator op, CpuMassMatrix mass, double[] omega, double[] thetaInvWarm)
{
    double[] ProjGrad(double[] cInv)
    {
        var theta = ExpandTheta(cInv, mesh.VertexCount, dimG);
        var g = op.ComputeJointGradient(omega, theta, mass);
        var p = new double[dimG];
        for (int v = 0; v < mesh.VertexCount; v++)
            for (int l = 0; l < dimG; l++) p[l] += g.GradTheta[v * dimG + l];
        return p;
    }
    var c = (double[])thetaInvWarm.Clone();
    var gStart = op.ComputeJointGradient(omega, ExpandTheta(c, mesh.VertexCount, dimG), mass);
    double equivScale = System.Math.Max(Norm(gStart.GradTheta), 1e-12);
    var g0 = new double[dimG];
    for (int v = 0; v < mesh.VertexCount; v++)
        for (int l = 0; l < dimG; l++) g0[l] += gStart.GradTheta[v * dimG + l];
    double scale = System.Math.Max(Norm(g0), 1e-12);
    for (int it = 0; it < 200; it++)
    {
        var g = ProjGrad(c);
        if (Norm(g) <= System.Math.Max(ThetaTolRel * scale, 1e-13)) break;
        var J = new double[dimG, dimG];
        for (int j = 0; j < dimG; j++)
        {
            var cp = (double[])c.Clone(); cp[j] += 1e-6;
            var gp = ProjGrad(cp);
            for (int i = 0; i < dimG; i++) J[i, j] = (gp[i] - g[i]) / 1e-6;
        }
        var step = Solve3(J, g);
        if (step == null) break;
        for (int i = 0; i < dimG; i++) c[i] -= step[i];
    }
    var thetaFull = ExpandTheta(c, mesh.VertexCount, dimG);
    var gFin = op.ComputeJointGradient(omega, thetaFull, mass);
    var pFin = new double[dimG];
    for (int v = 0; v < mesh.VertexCount; v++)
        for (int l = 0; l < dimG; l++) pFin[l] += gFin.GradTheta[v * dimG + l];
    double thetaResidRel = Norm(pFin) / scale;
    double equivResid = Norm(gFin.GradTheta) / equivScale;
    bool relOk = thetaResidRel <= ThetaResidualBatteryRel && equivResid <= EquivarianceGate;
    bool absOk = Norm(gFin.GradTheta) <= ThetaAbsGradFloor;
    return (c, thetaResidRel, equivResid, relOk || absOk);
}

// ---------------------------------------------------------------------------
// ARM A execution over the audited background menu.
// ---------------------------------------------------------------------------

var backgroundRecords = new List<BackgroundRecord>();
var allCandidates = new List<NegativeCandidate>();
double hBlockReconstructMax = 0.0, dBlockReconstructMax = 0.0;
double thetaResidWorst = 0.0, equivarianceWorst = 0.0;
bool allThetaOk = true;
bool lineage448AllMatch = true;
int lineage448Comparisons = 0;
var lineageRows = new List<object>();

// H-block reconstruction battery (phase448 discipline) at a given point.
double HBlockReconstruct(int memberIndex, double[] omega, double[] theta, int salt)
{
    var spec = memberSpecs[memberIndex].Spec;
    var op = sharedOps[memberIndex];
    var cols = new double[blockDim][];
    for (int ci = 0; ci < blockDim; ci++)
    {
        int ty = ci / dimG, l = ci % dimG;
        var vO = new double[nOmega];
        var vT = new double[nTheta];
        if (ty < 15)
        {
            int e0 = edgeAt[vertexAt[(0, 0, 0, 0)], ty];
            vO[e0 * dimG + l] = oSign[e0];
        }
        else vT[vertexAt[(0, 0, 0, 0)] * dimG + l] = 1.0;
        var (hvO, hvT) = op.JointHessianVectorProduct(omega, theta, vO, vT, sharedMass, HvStep);
        var col = new double[nJoint];
        for (int e = 0; e < mesh.EdgeCount; e++)
            for (int ll = 0; ll < dimG; ll++)
                col[e * dimG + ll] = oSign[e] * hvO[e * dimG + ll];
        Array.Copy(hvT, 0, col, nOmega, nTheta);
        cols[ci] = col;
    }
    void AccumulateTranslatedColumn(double[] target, double[] col, int[] site, double weight)
    {
        if (weight == 0.0) return;
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var c0 = coords[edgeBase[e]];
            int sv = vertexAt[(Mod(c0[0] - site[0], n), Mod(c0[1] - site[1], n), Mod(c0[2] - site[2], n), Mod(c0[3] - site[3], n))];
            int se = edgeAt[sv, edgeType[e]];
            for (int l = 0; l < dimG; l++)
                target[e * dimG + l] += weight * col[se * dimG + l];
        }
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            var c0 = coords[v];
            int sv = vertexAt[(Mod(c0[0] - site[0], n), Mod(c0[1] - site[1], n), Mod(c0[2] - site[2], n), Mod(c0[3] - site[3], n))];
            for (int l = 0; l < dimG; l++)
                target[nOmega + v * dimG + l] += weight * col[nOmega + sv * dimG + l];
        }
    }
    var vFull = DetVector(nJoint, salt);
    var hvRecon = new double[nJoint];
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
            AccumulateTranslatedColumn(hvRecon, cols[edgeType[e] * dimG + l],
                coords[edgeBase[e]], oSign[e] * vFull[e * dimG + l]);
    for (int v = 0; v < mesh.VertexCount; v++)
        for (int l = 0; l < dimG; l++)
            AccumulateTranslatedColumn(hvRecon, cols[15 * dimG + l],
                coords[v], vFull[nOmega + v * dimG + l]);
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
            hvRecon[e * dimG + l] *= oSign[e];
    var (dO, dT) = op.JointHessianVectorProduct(omega, theta,
        vFull.Take(nOmega).ToArray(), vFull.Skip(nOmega).ToArray(), sharedMass, HvStep);
    var hvDirect = new double[nJoint];
    Array.Copy(dO, 0, hvDirect, 0, nOmega); Array.Copy(dT, 0, hvDirect, nOmega, nTheta);
    double num = 0, den = 0;
    for (int i = 0; i < nJoint; i++)
    {
        num += (hvRecon[i] - hvDirect[i]) * (hvRecon[i] - hvDirect[i]);
        den += hvDirect[i] * hvDirect[i];
    }
    return System.Math.Sqrt(num) / System.Math.Max(1e-30, System.Math.Sqrt(den));
}

// --- Background 1: trivial (omega=0, theta=0). ----------------------------
bool kernelTrivialTotalOk, kernelTrivialPatternOk;
bool kernelGapCleanAll = true, kernelEvenAll = true, complementConstructionOk = true;
double kernelEigResidualWorst = 0.0;
int kernelTrivialTotalComplexDim;
{
    var omega0 = new double[nOmega];
    var theta0 = new double[nTheta];
    var kernels = BuildSectorKernels(omega0);
    kernelTrivialTotalComplexDim = kernels.Sum(kk => kk.SelfConj ? kk.DimC : 2 * kk.DimC);
    kernelTrivialTotalOk = kernelTrivialTotalComplexDim == expectedKernelTrivialTotal;
    kernelTrivialPatternOk = kernels.All(kk =>
        kk.SelfConj ? kk.DimC == expectedKernelTrivialK0 : kk.DimC == expectedKernelTrivialNonzeroK);
    foreach (var kk in kernels)
    {
        kernelGapCleanAll &= kk.GapOk;
        kernelEvenAll &= kk.EvenOk;
        complementConstructionOk &= kk.ComplementOk;
        kernelEigResidualWorst = System.Math.Max(kernelEigResidualWorst, kk.EigResidual);
    }
    dBlockReconstructMax = System.Math.Max(dBlockReconstructMax, DBlockReconstruct(omega0, BuildDColumns(omega0), 301));

    var memberRows = new List<BackgroundMemberResult>();
    for (int mi = 0; mi < memberSpecs.Count; mi++)
    {
        var r = ComputeMemberPoint(mi, omega0, theta0, kernels, "trivial", 0.0);
        memberRows.Add(r);
        allCandidates.AddRange(r.Candidates);
        if (mi == 1) hBlockReconstructMax = System.Math.Max(hBlockReconstructMax, HBlockReconstruct(mi, omega0, theta0, 401));
    }
    backgroundRecords.Add(new BackgroundRecord("trivial", "none", 0.0,
        kernelTrivialTotalComplexDim,
        kernels.First(kk => kk.SelfConj).DimC,
        kernels.Where(kk => !kk.SelfConj).Select(kk => kk.DimC).DefaultIfEmpty(0).Min(),
        kernels.Where(kk => !kk.SelfConj).Select(kk => kk.DimC).DefaultIfEmpty(0).Max(),
        memberRows));
}

// --- Backgrounds 2: the phase448 rays (theta* grid walk, lineage battery). --
var raySets = new (string Id, double[] Ray, double[] TGridWalk, int[] SelectedIdx, int? LineageSeed)[]
{
    ("p448-seed0", ray448S0, tGrid448, selected448Indices, 0),
    ("p448-seed1", ray448S1, tGrid448, selected448Indices, 1),
    ("p450-ray", ray450, tMenu450, Enumerable.Range(0, tMenu450.Length).ToArray(), null),
};

foreach (var rs in raySets)
{
    // Sector kernels per selected t (member-independent).
    var kernelsByT = new Dictionary<int, List<SectorKernel>>();
    foreach (int si in rs.SelectedIdx)
    {
        var omega0 = Scale(rs.Ray, rs.TGridWalk[si]);
        var kernels = BuildSectorKernels(omega0);
        kernelsByT[si] = kernels;
        foreach (var kk in kernels)
        {
            kernelGapCleanAll &= kk.GapOk;
            kernelEvenAll &= kk.EvenOk;
            complementConstructionOk &= kk.ComplementOk;
            kernelEigResidualWorst = System.Math.Max(kernelEigResidualWorst, kk.EigResidual);
        }
    }
    dBlockReconstructMax = System.Math.Max(dBlockReconstructMax,
        DBlockReconstruct(Scale(rs.Ray, rs.TGridWalk[rs.SelectedIdx[^1]]),
            BuildDColumns(Scale(rs.Ray, rs.TGridWalk[rs.SelectedIdx[^1]])), 302));

    var rowsByT = rs.SelectedIdx.ToDictionary(si => si, _ => new List<BackgroundMemberResult>());
    for (int mi = 0; mi < memberSpecs.Count; mi++)
    {
        var (name, spec, isControl) = memberSpecs[mi];
        var op = sharedOps[mi];
        var thetaInv = new double[dimG];
        for (int gi = 0; gi < rs.TGridWalk.Length; gi++)
        {
            var omega = Scale(rs.Ray, rs.TGridWalk[gi]);
            double thetaResidRel = 0.0, equivResid = 0.0;
            bool thetaOk = true;
            if (!isControl)
            {
                (thetaInv, thetaResidRel, equivResid, thetaOk) = ThetaStar(op, sharedMass, omega, thetaInv);
                thetaResidWorst = System.Math.Max(thetaResidWorst, thetaResidRel);
                equivarianceWorst = System.Math.Max(equivarianceWorst, equivResid);
                allThetaOk &= thetaOk;
            }
            if (!rs.SelectedIdx.Contains(gi)) continue;
            var theta = isControl ? new double[nTheta] : ExpandTheta(thetaInv, mesh.VertexCount, dimG);
            string bgId = $"{rs.Id}/t{rs.TGridWalk[gi]:F4}";
            var r = ComputeMemberPoint(mi, omega, theta, kernelsByT[gi], bgId, rs.TGridWalk[gi]);
            r = r with
            {
                ThetaResidRel = thetaResidRel,
                EquivResid = equivResid,
                ThetaStarInv = isControl ? new double[dimG] : (double[])thetaInv.Clone(),
            };
            rowsByT[gi].Add(r);
            allCandidates.AddRange(r.Candidates);

            if (rs.LineageSeed is int seed)
            {
                lineage448Comparisons++;
                bool match = committed448.TryGetValue((name, seed, rs.TGridWalk[gi]), out var exp)
                    && exp.Pos == r.Pos && exp.Zero == r.Zero && exp.Neg == r.Neg;
                lineage448AllMatch &= match;
                lineageRows.Add(new
                {
                    member = name,
                    seed,
                    t = rs.TGridWalk[gi],
                    expected = committed448.TryGetValue((name, seed, rs.TGridWalk[gi]), out var e2)
                        ? new[] { e2.Pos, e2.Zero, e2.Neg } : null,
                    computed = new[] { r.Pos, r.Zero, r.Neg },
                    match,
                });
            }
            if (mi == 1 && gi == rs.SelectedIdx[0])
                hBlockReconstructMax = System.Math.Max(hBlockReconstructMax, HBlockReconstruct(mi, omega, theta, 402));
        }
    }
    foreach (int si in rs.SelectedIdx)
    {
        var kernels = kernelsByT[si];
        backgroundRecords.Add(new BackgroundRecord(
            $"{rs.Id}/t{rs.TGridWalk[si]:F4}", rs.Id, rs.TGridWalk[si],
            kernels.Sum(kk => kk.SelfConj ? kk.DimC : 2 * kk.DimC),
            kernels.First(kk => kk.SelfConj).DimC,
            kernels.Where(kk => !kk.SelfConj).Select(kk => kk.DimC).DefaultIfEmpty(0).Min(),
            kernels.Where(kk => !kk.SelfConj).Select(kk => kk.DimC).DefaultIfEmpty(0).Max(),
            rowsByT[si]));
    }
}

double tracePartitionWorstAll = backgroundRecords
    .SelectMany(b => b.Members).Max(m => m.TracePartitionWorst);

// ---------------------------------------------------------------------------
// ARM B: the directional-cubic battery about the origin.
// ---------------------------------------------------------------------------

// Pre-registered momentum menu: all 15 canonical 0/1 momenta of (Z_3)^4,
// labeled by shell = number of nonzero components.
var momentumMenu = new List<(int[] K, int Shell)>();
for (int mask = 1; mask < 16; mask++)
{
    var k = new[] { (mask >> 3) & 1, (mask >> 2) & 1, (mask >> 1) & 1, mask & 1 };
    momentumMenu.Add((k, k.Sum()));
}
momentumMenu = momentumMenu.OrderBy(m => m.Shell).ThenBy(m => KeyOf(m.K)).ToList();

// Pre-registered coefficient sets: the 3 audited ray coefficient sets plus
// the 45 deterministic single-(type,Lie) probes.
var coeffSets = new List<(string Id, double[] C)>
{
    ("p448-seed0", rayCoeff448S0),
    ("p448-seed1", rayCoeff448S1),
    ("p450-ray", rayCoeff450),
};
for (int ty = 0; ty < 15; ty++)
    for (int l = 0; l < dimG; l++)
    {
        var c = new double[16 * dimG];
        c[ty * dimG + l] = 1.0;
        coeffSets.Add(($"type{ty:D2}-l{l}", c));
    }

double[]? BuildWaveDirection(double[] cType, int[] k, bool sinPhase)
{
    var v = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        var x = coords[edgeBase[e]];
        double ph = 2.0 * System.Math.PI * (k[0] * x[0] + k[1] * x[1] + k[2] * x[2] + k[3] * x[3]) / n;
        double w = sinPhase ? System.Math.Sin(ph) : System.Math.Cos(ph);
        for (int l = 0; l < dimG; l++)
            v[e * dimG + l] = oSign[e] * cType[edgeType[e] * dimG + l] * w;
    }
    double nn = Norm(v);
    if (nn < 1e-12) return null;
    for (int i = 0; i < nOmega; i++) v[i] /= nn;
    return v;
}

var armBDirections = new List<(string CoeffId, int[] K, int Shell, bool Sin, double[] V)>();
foreach (var (cid, c) in coeffSets)
    foreach (var (k, shell) in momentumMenu)
        foreach (bool sinPhase in new[] { false, true })
        {
            var v = BuildWaveDirection(c, k, sinPhase);
            if (v != null) armBDirections.Add((cid, k, shell, sinPhase, v));
        }

// Menu hash (determinism record; the menu is closed-form, no RNG).
string armBMenuHash;
{
    var sb = new StringBuilder();
    foreach (var d in armBDirections)
    {
        sb.Append(d.CoeffId).Append('|').Append(string.Join(",", d.K)).Append('|').Append(d.Sin ? "sin" : "cos").Append('|');
        sb.Append(d.V.Length).Append('|').Append(d.V[0].ToString("R")).Append('|').Append(d.V[^1].ToString("R")).Append(';');
    }
    armBMenuHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
}

// S(0) and theta-flatness batteries.
double s0Worst = 0.0, thetaFlatWorst = 0.0;
for (int mi = 0; mi < memberSpecs.Count; mi++)
{
    var (_, _, isControl) = memberSpecs[mi];
    s0Worst = System.Math.Max(s0Worst,
        System.Math.Abs(SBDirect(sharedOps[mi], sharedMass, isControl, new double[nOmega], new double[nTheta])));
    if (!isControl)
        thetaFlatWorst = System.Math.Max(thetaFlatWorst,
            System.Math.Abs(SBDirect(sharedOps[mi], sharedMass, false, new double[nOmega], DetVector(nTheta, 91))));
}

var armBRows = new ArmBRow[armBDirections.Count * memberSpecs.Count];
for (int mi = 0; mi < memberSpecs.Count; mi++)
{
    var spec = memberSpecs[mi].Spec;
    bool isControl = memberSpecs[mi].IsControl;
    string mName = memberSpecs[mi].Name;
    int baseIdx = mi * armBDirections.Count;
    Parallel.For(0, armBDirections.Count, parallelOptions,
        () => (Op: MakeOp(spec), Mass: new CpuMassMatrix(mesh, algebra)),
        (di, _, local) =>
        {
            var (cid, k, shell, sinPhase, v) = armBDirections[di];
            var theta0 = new double[nTheta];
            double S(double t) => SBDirect(local.Op, local.Mass, isControl, Scale(v, t), theta0);
            double h = ArmBStep;
            double s1 = S(h), s1m = S(-h), s2 = S(2 * h), s2m = S(-2 * h), s3 = S(3 * h);
            double hc = ArmBCrossStep;
            double sc = S(hc), scm = S(-hc);
            double a3 = (s1 - s1m) / (2 * h * h * h);
            double a3b = (s2 - s2m) / (16 * h * h * h);
            double a3c = (sc - scm) / (2 * hc * hc * hc);
            double a3Stencil = (s2 - 2 * s1 + 2 * s1m - s2m) / (12 * h * h * h);
            double eh = 0.5 * (s1 + s1m), e2h = 0.5 * (s2 + s2m);
            double a4 = (e2h - 4 * eh) / (12 * h * h * h * h);
            double a2 = (16 * eh - e2h) / (12 * h * h);
            double sPred3h = a2 * 9 * h * h + a3 * 27 * h * h * h + a4 * 81 * h * h * h * h;
            double quarticResid = System.Math.Abs(s3 - sPred3h) / System.Math.Max(System.Math.Abs(s3), 1e-14);
            double crossScale = System.Math.Max(System.Math.Max(a2, a4), 1e-6);
            double crossWorst = System.Math.Max(
                System.Math.Max(System.Math.Abs(a3 - a3b), System.Math.Abs(a3 - a3c)),
                System.Math.Abs(a3 - a3Stencil)) / crossScale;
            double a3ZeroScale = System.Math.Max(System.Math.Max(a2, a4), 1e-30);
            bool a3IsZero = System.Math.Abs(a3) <= ArmBA3ZeroRel * a3ZeroScale;
            double disc = a3 * a3 - 4.0 * a2 * a4;
            double degRatio = a2 > 0 && a4 > 0 ? a3 * a3 / (4.0 * a2 * a4) : double.NaN;
            armBRows[baseIdx + di] = new ArmBRow(mName, cid, $"{k[0]}{k[1]}{k[2]}{k[3]}", shell, sinPhase,
                a2, a3, a4, degRatio, disc, a3IsZero, crossWorst, quarticResid);
            return local;
        },
        _ => { });
}

// On-ray parity controls: a3 = 0 on the three audited INVARIANT rays.
var onRayControls = new List<object>();
double onRayA3Worst = 0.0;
{
    var invariantRays = new (string Id, double[] U)[]
    {
        ("p448-seed0", ray448S0), ("p448-seed1", ray448S1), ("p450-ray", ray450),
    };
    for (int mi = 0; mi < memberSpecs.Count; mi++)
    {
        bool isControl = memberSpecs[mi].IsControl;
        foreach (var (rid, u) in invariantRays)
        {
            var theta0 = new double[nTheta];
            double S(double t) => SBDirect(sharedOps[mi], sharedMass, isControl, Scale(u, t), theta0);
            double h = ArmBStep;
            double s1 = S(h), s1m = S(-h);
            double eh = 0.5 * (s1 + s1m);
            double a3 = (s1 - s1m) / (2 * h * h * h);
            double relA3 = System.Math.Abs(a3) / System.Math.Max(eh / (h * h), 1e-30);
            onRayA3Worst = System.Math.Max(onRayA3Worst, relA3);
            onRayControls.Add(new { member = memberSpecs[mi].Name, ray = rid, a3, a3OverA2Scale = relA3 });
        }
    }
}

// Arm B aggregation and gates.
double armBCrossWorst = armBRows.Max(r => r.CrossWorst);
double armBQuarticResidWorst = armBRows.Max(r => r.QuarticResid);
bool armBCrossOk = armBCrossWorst <= ArmBCrossRel;
bool armBQuarticOk = armBQuarticResidWorst <= ArmBQuarticExactGate;
double armBA2Min = armBRows.Min(r => r.A2);
double armBA4Min = armBRows.Min(r => r.A4);
bool armBPositivityOk = armBRows.All(r =>
    r.A2 >= -1e-10 * System.Math.Max(System.Math.Abs(r.A4), 1.0) &&
    (r.A4 > 1e-12 * System.Math.Max(r.A2, 1.0) || r.A3IsZero));
bool onRayParityOk = onRayA3Worst <= 1e-8;
bool s0Ok = s0Worst <= 1e-20;
bool thetaFlatOk = thetaFlatWorst <= 1e-20;

int armBNonzeroA3Count = armBRows.Count(r => !r.A3IsZero);
int armBDegenerateCapableCount = armBRows.Count(r => !r.A3IsZero && !double.IsNaN(r.DegRatio) && r.DegRatio >= 1.0);
double armBMaxDegRatio = armBRows.Where(r => !double.IsNaN(r.DegRatio)).Select(r => r.DegRatio).DefaultIfEmpty(double.NaN).Max();
double armBMaxA3OverA2 = armBRows.Where(r => r.A2 > 1e-12).Select(r => System.Math.Abs(r.A3) / r.A2).DefaultIfEmpty(double.NaN).Max();
bool armBNoDegenerateCubicOnMenu = armBDegenerateCapableCount == 0;

var armBShellSummaries = armBRows
    .GroupBy(r => (r.Member, r.Shell))
    .OrderBy(g => g.Key.Member).ThenBy(g => g.Key.Shell)
    .Select(g => new
    {
        member = g.Key.Member,
        shell = g.Key.Shell,
        directionCount = g.Count(),
        nonzeroA3Count = g.Count(r => !r.A3IsZero),
        maxAbsA3 = g.Max(r => System.Math.Abs(r.A3)),
        maxA3OverA2 = Fin(g.Where(r => r.A2 > 1e-12).Select(r => System.Math.Abs(r.A3) / r.A2).DefaultIfEmpty(double.NaN).Max()),
        maxDegeneracyRatio = Fin(g.Where(r => !double.IsNaN(r.DegRatio)).Select(r => r.DegRatio).DefaultIfEmpty(double.NaN).Max()),
        minA2 = g.Min(r => r.A2),
        minA4 = g.Min(r => r.A4),
    })
    .ToArray();

// ---------------------------------------------------------------------------
// Gates roll-up + pre-registered verdict.
// ---------------------------------------------------------------------------

bool covarianceBattery = covarianceMax <= CovarianceGate;
bool faceCovarianceBattery = faceCovarianceMax <= FaceCovarianceGate;
bool objectiveConsistencyBattery = objConsistencyMax <= ObjectiveConsistencyGate;
bool gradientBattery = gradFdMax <= GradFdGate;
bool identityThetaIndependent = identityThetaGradNorm <= 1e-10;
bool hBlockReconstructionBattery = hBlockReconstructMax <= HBlockReconstructGate;
bool dBlockReconstructionBattery = dBlockReconstructMax <= DBlockReconstructGate;
bool kernelEigResidualOk = kernelEigResidualWorst <= KernelEigResidualGate;
bool tracePartitionOk = tracePartitionWorstAll <= TracePartitionGate;
bool lineage448Ok = lineage448AllMatch && lineage448Comparisons == 24;

bool batteriesAllPassed =
    precursorsPassed &&
    orbitMapOk && faceMapOk && faceTypeCountConsistent &&
    covarianceBattery && faceCovarianceBattery &&
    objectiveConsistencyBattery && gradientBattery && identityThetaIndependent &&
    hBlockReconstructionBattery && dBlockReconstructionBattery &&
    allThetaOk &&
    kernelTrivialTotalOk && kernelTrivialPatternOk &&
    kernelGapCleanAll && kernelEvenAll && complementConstructionOk && kernelEigResidualOk &&
    tracePartitionOk &&
    lineage448Ok &&
    armBCrossOk && armBQuarticOk && armBPositivityOk && onRayParityOk && s0Ok && thetaFlatOk;

int armANegativeComplementCount = allCandidates.Count;
bool armAAllNonnegative = armANegativeComplementCount == 0;
double armAMinComplementEig = backgroundRecords.SelectMany(b => b.Members).Min(m => m.CMinEig);
int armAKernelNegativeCount = backgroundRecords.SelectMany(b => b.Members).Sum(m => m.KNeg);

string verdictKind = !batteriesAllPassed
    ? "inconclusive-gates-failed"
    : armAAllNonnegative && armBNoDegenerateCubicOnMenu
        ? "beyond-ray-excluded-at-certified-backgrounds"
        : "beyond-ray-instability-candidate-found";

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool certificateConventionsAreWorkbenchConventions = true;
const bool physicistReviewPending = true;
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
    $"su2 trace-pairing; lattice-canonical torus n={TorusN}",
    "Arm A: joint (omega,theta) Hessian 48x48 momentum blocks from 48 orbit-representative Hessian-vector products (phase448 machinery verbatim); gauge kernel ker d recomputed per background as the exact background-covariant curvature linearization kernel per momentum sector; orthonormal complement restriction (plain Euclidean coefficient inner product); phase448 IR zero-mode convention; kernel-restricted spectra recorded",
    "Arm B: exact quartic ray solve at t=+-0.1,+-0.2 with +-0.15 and 5-point-stencil cross-checks and a t=0.3 held-out exactness gate over the pre-registered deterministic menu (15 canonical 0/1 momenta x cos/sin x {45 single-type probes + 3 audited ray coefficient sets}); degeneracy classifier a3^2 vs 4 a2 a4",
    "backgrounds: trivial + p448 rays seeds 0/1 at t in {0.1875,0.9375,1.6875,3.0} + p450 ray at t in {0.5,1.25,2.375}; theta* projected-Newton warm-started grid walk (phase448 verbatim); no target values",
    armBMenuHash)))).ToLowerInvariant();

bool beyondRayQuadraticCertificateProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    certificateConventionsAreWorkbenchConventions &&
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

string terminalStatus = beyondRayQuadraticCertificateProbePassed
    ? $"beyond-ray-quadratic-certificate-probe-passed-{verdictKind}"
    : "beyond-ray-quadratic-certificate-probe-blocked";

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

string decision = beyondRayQuadraticCertificateProbePassed
    ? $"The beyond-invariant-ray quadratic/cubic certificate probe (Arms A/B of the Team B rank-2 item) is decided on internal consistency. ARM A: at the trivial background and {backgroundRecords.Count - 1} audited invariant-ray backgrounds (n={TorusN} lattice-canonical torus, {pairList.Count} momentum sector pairs), the joint (omega,theta) Hessian's momentum blocks were eigensolved on the exact orthogonal complement of the per-background gauge kernel ker d (recomputed per background; trivial-background dimension {kernelTrivialTotalComplexDim} = 12 at k=0 + 3 per nonzero momentum, verified). Complement negative directions found: {armANegativeComplementCount} (min complement eigenvalue {armAMinComplementEig:E3}); projected-kernel-restricted negatives (diagnostic, recorded, not certified): {armAKernelNegativeCount}. " +
      $"ARM B: over {armBRows.Length} exact directional-cubic solves ({armBDirections.Count} pre-registered non-invariant menu directions x {memberSpecs.Count} members), nonzero cubics appear on {armBNonzeroA3Count} rows with max |a3|/a2 = {armBMaxA3OverA2:E2} and max degeneracy ratio a3^2/(4 a2 a4) = {armBMaxDegRatio:E2} - {(armBNoDegenerateCubicOnMenu ? "NO menu direction satisfies the degenerate-second-zero precondition a3^2 >= 4 a2 a4 (S_B >= 0 excludes any strictly lower configuration; degeneracy was the only opening)" : $"{armBDegenerateCapableCount} directions satisfy the degeneracy precondition and are recorded as named candidates")}. " +
      $"VERDICT: {verdictKind}. BATTERIES: covariance {covarianceMax:E2}, face covariance {faceCovarianceMax:E2}, H-block reconstruction {hBlockReconstructMax:E2}, D-block reconstruction {dBlockReconstructMax:E2}, theta* residual {thetaResidWorst:E2} / equivariance {equivarianceWorst:E2}, phase448 lineage {lineage448Comparisons}/24 exact-count matches, Arm B cross-check {armBCrossWorst:E2} and held-out quartic-exactness {armBQuarticResidWorst:E2}, on-ray parity |a3| control {onRayA3Worst:E2}. " +
      "SCOPE HONESTY: these are certificates at a FINITE background menu, one volume (n=3), and quadratic+cubic order; they NARROW B2 and feed the closure theorem-of-record (program item 22); they do not close B2 alone. MANDATORY FRAMING: workbench-relative structure data only (su(2) toy algebra, reduced Spin(4) slice, lattice units); the 445-452 conventions carry physicistReviewPending; NO GeV/pole promotion; nothing fills a Phase201/Phase256 contract field."
    : "Do not use the certificate verdicts until the precursor, covariance, reconstruction, kernel-dimension, lineage, and Arm B exactness batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase454-beyond-ray-quadratic-certificate-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    beyondRayQuadraticCertificateProbePassed,

    phase448PrecursorPassed,
    phase450PrecursorPassed,
    phase453PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    verdictKind,
    verdictTaxonomy = new[]
    {
        "beyond-ray-excluded-at-certified-backgrounds",
        "beyond-ray-instability-candidate-found",
        "inconclusive-gates-failed",
    },
    scopeStatement = "Certificates at a FINITE background menu (trivial + audited invariant-ray points), one volume (n=3), quadratic+cubic order. This narrows B2 and feeds the B-closure theorem-of-record (program item 22); it does NOT close B2 alone. Arm C (structure factors) is pre-registered in phase453 and out of scope here.",

    probeConfiguration = new
    {
        lieAlgebraId = "su2-trace-pairing",
        torusN = TorusN,
        latticeCanonical = true,
        jointDof = nJoint,
        momentumSectorPairs = pairList.Count,
        hvStep = HvStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        kernelRelTol = KernelRelTol,
        kernelGapMin = KernelGapMin,
        armBStep = ArmBStep,
        armBCrossStep = ArmBCrossStep,
        maxDegreeOfParallelism = maxDop,
        gaugeKernelConvention = "ker L = ker d recomputed PER BACKGROUND: d is the background-covariant curvature linearization D_{omega0} delta = d delta + [omega0 ^ delta], obtained exactly (central difference of the exactly-quadratic discrete curvature map); per momentum sector the kernel of D(k)^dag D(k) is extracted with a fail-closed spectral-gap rule and projected out by orthonormal complement restriction in the plain Euclidean coefficient inner product (recorded phase450 convention); the trivial-background kernel is never reused",
        backgroundMenu = "trivial (omega=0, theta=0) + phase448 invariant rays seeds 0/1 at t in {0.1875, 0.9375, 1.6875, 3.0} (the committed phase448 grid points; exact-count lineage battery) + the phase450/453 ray at t in {0.5, 1.25, 2.375} (spanning the committed CEP window range incl. the antisymmetry-max bin); theta* projected-Newton warm-started grid walk per member (phase448 verbatim); identity control at theta = 0",
        armBMenu = "15 canonical 0/1 momenta of (Z_3)^4 (shells 1-4) x {cos, sin} x {45 single-(edge-type,Lie) probes + 3 audited ray coefficient sets}, unit-normalized, all closed-form deterministic",
        armBMenuHash,
        rngUsage = "the ONLY System.Random uses are the fixed-seed verbatim reproductions of the committed phase448 (Random(20260704 + 31*seed)) and phase450 (Random(20260703)) ray-coefficient conventions; all new menu objects and battery vectors are closed-form deterministic (no fresh randomness anywhere)",
        spectrumMethod = "phase448 verbatim: 48 orbit-representative Hessian-vector products, DFT into Hermitian 48x48 momentum blocks, {k,-k} pairs via the real symmetric embedding",
        irConvention = "phase448 verbatim: zeroTol = max(absFloor, relTol*maxAbsEig) from the full block spectrum per point",
    },

    // ---- ARM A. ----
    armA = new
    {
        allNonnegativeOnGaugeComplement = armAAllNonnegative,
        negativeComplementDirectionCount = armANegativeComplementCount,
        minComplementEigenvalue = Fin(armAMinComplementEig),
        kernelRestrictedNegativeCount = armAKernelNegativeCount,
        kernelTrivialTotalComplexDim,
        kernelTrivialTotalOk,
        kernelTrivialPatternOk,
        expectedKernelTrivialTotal,
        expectedKernelTrivialK0,
        expectedKernelTrivialNonzeroK,
        backgroundCount = backgroundRecords.Count,
        namedCandidates = allCandidates.Select(c => new
        {
            background = c.BackgroundId,
            member = c.Member,
            t = c.T,
            sectorK = c.SectorK,
            eigenvalue = c.Eigenvalue,
            thetaWeightFraction = c.ThetaWeight,
            topTypeWeights = c.TopTypes.Select(tt => new { type = tt.Type, weight = tt.Weight }).ToArray(),
        }).ToArray(),
        backgrounds = backgroundRecords.Select(b => new
        {
            id = b.Id,
            raySet = b.RaySet,
            t = b.T,
            kernelTotalComplexDim = b.KernelTotal,
            kernelK0Dim = b.KernelK0,
            kernelNonzeroKDimMin = b.KernelMinNonzero,
            kernelNonzeroKDimMax = b.KernelMaxNonzero,
            members = b.Members.Select(m => new
            {
                member = m.Member,
                fullSpectrum = new { positive = m.Pos, zero = m.Zero, negative = m.Neg, maxAbsEig = m.MaxAbs, zeroTol = m.ZeroTol },
                gaugeComplement = new { positive = m.CPos, zero = m.CZero, negative = m.CNeg, minEigenvalue = Fin(m.CMinEig) },
                kernelRestricted = new { positive = m.KPos, zero = m.KZero, negative = m.KNeg, minEigenvalue = Fin(m.KMinEig) },
                thetaStarInvariant = m.ThetaStarInv,
                thetaResidualRel = m.ThetaResidRel,
                equivarianceResidual = m.EquivResid,
            }).ToArray(),
        }).ToArray(),
        lineage448 = new
        {
            comparisons = lineage448Comparisons,
            allMatch = lineage448AllMatch,
            rows = lineageRows,
        },
    },

    // ---- ARM B. ----
    armB = new
    {
        noDegenerateCubicOnMenu = armBNoDegenerateCubicOnMenu,
        degenerateCapableDirectionCount = armBDegenerateCapableCount,
        nonzeroA3RowCount = armBNonzeroA3Count,
        totalRowCount = armBRows.Length,
        directionCount = armBDirections.Count,
        maxDegeneracyRatio = Fin(armBMaxDegRatio),
        maxA3OverA2 = Fin(armBMaxA3OverA2),
        minA2 = armBA2Min,
        minA4 = armBA4Min,
        degeneracyClassifier = "a3^2 >= 4 a2 a4 is the precondition for a second S_B = 0 configuration on the ray; S_B >= 0 (exact) excludes any strictly LOWER beyond-ray configuration, so ray-degeneracy is the only first-order-transition-style opening at this order",
        shellSummaries = armBShellSummaries,
        onRayParityControls = onRayControls,
        degenerateCapableRows = armBRows.Where(r => !r.A3IsZero && !double.IsNaN(r.DegRatio) && r.DegRatio >= 1.0)
            .Select(r => new { r.Member, coeffSet = r.CoeffId, r.SectorK, r.Shell, phase = r.Sin ? "sin" : "cos", r.A2, r.A3, r.A4, degeneracyRatio = Fin(r.DegRatio) })
            .ToArray(),
        topA3Rows = armBRows.OrderByDescending(r => double.IsNaN(r.DegRatio) ? 0.0 : r.DegRatio).Take(10)
            .Select(r => new { r.Member, coeffSet = r.CoeffId, r.SectorK, r.Shell, phase = r.Sin ? "sin" : "cos", r.A2, r.A3, r.A4, degeneracyRatio = Fin(r.DegRatio) })
            .ToArray(),
    },

    batteries = new
    {
        batteriesAllPassed,
        orbitMapOk,
        faceMapOk,
        faceTypeCountConsistent,
        faceTypeCount = nFaceTypes,
        covarianceBattery,
        covarianceMax,
        faceCovarianceBattery,
        faceCovarianceMax,
        objectiveConsistencyBattery,
        objConsistencyMax,
        gradientBattery,
        gradFdMax,
        identityThetaIndependent,
        identityThetaGradNorm,
        hBlockReconstructionBattery,
        hBlockReconstructMax,
        dBlockReconstructionBattery,
        dBlockReconstructMax,
        thetaGate = allThetaOk,
        thetaResidWorst,
        equivarianceWorst,
        kernelTrivialTotalOk,
        kernelTrivialPatternOk,
        kernelGapCleanAll,
        kernelEvenAll,
        complementConstructionOk,
        kernelEigResidualOk,
        kernelEigResidualWorst,
        tracePartitionOk,
        tracePartitionWorst = tracePartitionWorstAll,
        lineage448Ok,
        armBCrossOk,
        armBCrossWorst,
        armBQuarticOk,
        armBQuarticResidWorst,
        armBPositivityOk,
        onRayParityOk,
        onRayA3Worst,
        s0Ok,
        s0Worst,
        thetaFlatOk,
        thetaFlatWorst,
        projectorOverlapMaxAfter,
    },

    // Headline booleans.
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    certificateConventionsAreWorkbenchConventions,
    physicistReviewPending,

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
        phase450SummaryPath = Phase450SummaryPath,
        phase453SummaryPath = Phase453SummaryPath,
        programDocPath = ProgramDocPath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "Every eigenvalue, kernel dimension, and cubic coefficient here is candidate-only structure data of the reduced spin-4 slice on the n=3 lattice-canonical torus in lattice units - NOT a physical mass, spectrum, or scale in GeV.",
        "The certificates hold at a FINITE background menu, one volume, and quadratic+cubic order; they narrow B2 and feed the closure theorem-of-record; they do not close B2 alone.",
        "A named instability candidate, where found, is a workbench-relative structure observation pending physicist review of the 445-452 conventions; an exclusion verdict sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

static double? Fin(double x) => double.IsFinite(x) ? x : null;

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "beyond_ray_quadratic_certificate_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "beyond_ray_quadratic_certificate_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"beyondRayQuadraticCertificateProbePassed={beyondRayQuadraticCertificateProbePassed}");
Console.WriteLine($"precursors: p448={phase448PrecursorPassed} p450={phase450PrecursorPassed} p453={phase453PrecursorPassed}");
Console.WriteLine($"VERDICT: {verdictKind}");
Console.WriteLine($"ARM A: negComplement={armANegativeComplementCount} minComplementEig={armAMinComplementEig:E3} kernelRestrictedNeg={armAKernelNegativeCount} kernelTrivialDim={kernelTrivialTotalComplexDim} (expect {expectedKernelTrivialTotal}; pattern ok={kernelTrivialPatternOk})");
foreach (var b in backgroundRecords)
    foreach (var m in b.Members)
        Console.WriteLine($"  bg={b.Id,-22} {m.Member,-14} full(p/z/n)={m.Pos}/{m.Zero}/{m.Neg} complement(p/z/n)={m.CPos}/{m.CZero}/{m.CNeg} minC={m.CMinEig:E2} kernel(p/z/n)={m.KPos}/{m.KZero}/{m.KNeg} kerDim={b.KernelTotal}");
Console.WriteLine($"LINEAGE448: {lineage448Comparisons} comparisons, allMatch={lineage448AllMatch}");
Console.WriteLine($"ARM B: rows={armBRows.Length} nonzeroA3={armBNonzeroA3Count} degenerateCapable={armBDegenerateCapableCount} maxDegRatio={armBMaxDegRatio:E2} maxA3overA2={armBMaxA3OverA2:E2} crossWorst={armBCrossWorst:E2} quarticResidWorst={armBQuarticResidWorst:E2} onRayA3Worst={onRayA3Worst:E2}");
foreach (var s in armBShellSummaries)
    Console.WriteLine($"  {s.member,-14} shell={s.shell} dirs={s.directionCount} nonzeroA3={s.nonzeroA3Count} maxA3overA2={s.maxA3OverA2:E2} maxDegRatio={s.maxDegeneracyRatio:E2}");
Console.WriteLine($"BATTERIES: all={batteriesAllPassed} cov={covarianceMax:E2} faceCov={faceCovarianceMax:E2} obj={objConsistencyMax:E2} grad={gradFdMax:E2} hRecon={hBlockReconstructMax:E2} dRecon={dBlockReconstructMax:E2} theta={allThetaOk}({thetaResidWorst:E2}/{equivarianceWorst:E2}) kerGap={kernelGapCleanAll} trace={tracePartitionWorstAll:E2} s0={s0Worst:E2} thetaFlat={thetaFlatWorst:E2}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
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

static double[] ExpandTheta(double[] cInv, int vertexCount, int dimG)
{
    var theta = new double[vertexCount * dimG];
    for (int v = 0; v < vertexCount; v++)
        for (int l = 0; l < dimG; l++) theta[v * dimG + l] = cInv[l];
    return theta;
}

static double[]? Solve3(double[,] a, double[] b)
{
    int n = b.Length;
    var m = (double[,])a.Clone();
    var x = (double[])b.Clone();
    for (int col = 0; col < n; col++)
    {
        int piv = col;
        for (int r = col + 1; r < n; r++)
            if (System.Math.Abs(m[r, col]) > System.Math.Abs(m[piv, col])) piv = r;
        if (System.Math.Abs(m[piv, col]) < 1e-14) return null;
        if (piv != col)
        {
            for (int c2 = 0; c2 < n; c2++) (m[col, c2], m[piv, c2]) = (m[piv, c2], m[col, c2]);
            (x[col], x[piv]) = (x[piv], x[col]);
        }
        for (int r = col + 1; r < n; r++)
        {
            double f = m[r, col] / m[col, col];
            for (int c2 = col; c2 < n; c2++) m[r, c2] -= f * m[col, c2];
            x[r] -= f * x[col];
        }
    }
    for (int r = n - 1; r >= 0; r--)
    {
        for (int c2 = r + 1; c2 < n; c2++) x[r] -= m[r, c2] * x[c2];
        x[r] /= m[r, r];
    }
    return x;
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

static FieldTensor FaceTensor(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs) => new()
{
    Label = "coeffs",
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

// Eigenvalue-only Jacobi (phase448 verbatim - the lineage battery depends on
// this exact arithmetic path).
static double[] JacobiEigenvalues(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    for (int sweep = 0; sweep < 100; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-11) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (System.Math.Abs(apq) < 1e-15) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double tt = System.Math.Sign(tau == 0 ? 1.0 : tau) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                double cc = 1.0 / System.Math.Sqrt(1.0 + tt * tt);
                double ss = tt * cc;
                for (int k = 0; k < n; k++)
                {
                    if (k == p || k == q) continue;
                    double akp = a[k, p], akq = a[k, q];
                    a[k, p] = a[p, k] = cc * akp - ss * akq;
                    a[k, q] = a[q, k] = ss * akp + cc * akq;
                }
                a[p, p] = cc * cc * app - 2.0 * ss * cc * apq + ss * ss * aqq;
                a[q, q] = ss * ss * app + 2.0 * ss * cc * apq + cc * cc * aqq;
                a[p, q] = a[q, p] = 0.0;
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return values;
}

// Jacobi with eigenvector accumulation (scale-relative stop). Returns
// (values, vectors) with vectors[r][i] = component r of eigenvector i.
static (double[] Values, double[][] Vectors) JacobiEigenSystem(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vv = new double[n, n];
    for (int i = 0; i < n; i++) vv[i, i] = 1.0;
    double scale = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++) scale = System.Math.Max(scale, System.Math.Abs(a[i, j]));
    double stop = System.Math.Max(1e-300, 1e-14 * System.Math.Max(scale, 1e-30)) * n;
    for (int sweep = 0; sweep < 200; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < stop) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (System.Math.Abs(apq) < 1e-300) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double tt = System.Math.Sign(tau == 0 ? 1.0 : tau) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                double cc = 1.0 / System.Math.Sqrt(1.0 + tt * tt);
                double ss = tt * cc;
                for (int k = 0; k < n; k++)
                {
                    if (k != p && k != q)
                    {
                        double akp = a[k, p], akq = a[k, q];
                        a[k, p] = a[p, k] = cc * akp - ss * akq;
                        a[k, q] = a[q, k] = ss * akp + cc * akq;
                    }
                    double vkp = vv[k, p], vkq = vv[k, q];
                    vv[k, p] = cc * vkp - ss * vkq;
                    vv[k, q] = ss * vkp + cc * vkq;
                }
                a[p, p] = cc * cc * app - 2.0 * ss * cc * apq + ss * ss * aqq;
                a[q, q] = ss * ss * app + 2.0 * ss * cc * apq + cc * cc * aqq;
                a[p, q] = a[q, p] = 0.0;
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    var vectors = new double[n][];
    for (int r = 0; r < n; r++)
    {
        vectors[r] = new double[n];
        for (int i = 0; i < n; i++) vectors[r][i] = vv[r, i];
    }
    return (values, vectors);
}

// Orthonormal complement of a set of orthonormal vectors inside R^dim
// (double Gram-Schmidt; fail-closed count/orthonormality checks).
static (List<double[]> Basis, bool Ok) ComplementBasis(int dim, List<double[]> kernel)
{
    var basis = new List<double[]>();
    for (int i = 0; i < dim && basis.Count < dim - kernel.Count; i++)
    {
        var w = new double[dim];
        w[i] = 1.0;
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (var kv in kernel)
            {
                double d = Dot(w, kv);
                for (int r = 0; r < dim; r++) w[r] -= d * kv[r];
            }
            foreach (var q in basis)
            {
                double d = Dot(w, q);
                for (int r = 0; r < dim; r++) w[r] -= d * q[r];
            }
        }
        double nn = Norm(w);
        if (nn > 1e-6)
        {
            for (int r = 0; r < dim; r++) w[r] /= nn;
            basis.Add(w);
        }
    }
    bool ok = basis.Count == dim - kernel.Count;
    if (ok)
    {
        double resid = 0.0;
        for (int i = 0; i < basis.Count && ok; i++)
        {
            foreach (var kv in kernel) resid = System.Math.Max(resid, System.Math.Abs(Dot(basis[i], kv)));
            for (int j = i + 1; j < basis.Count; j++)
                resid = System.Math.Max(resid, System.Math.Abs(Dot(basis[i], basis[j])));
            resid = System.Math.Max(resid, System.Math.Abs(Norm(basis[i]) - 1.0));
        }
        ok = resid <= 1e-10;
    }
    return (basis, ok);
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase454-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase454",
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

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

public sealed record SectorKernel(
    int[] K, bool SelfConj, int DimC, int PaddedCount, bool GapOk, bool EvenOk,
    double MaxDroppedMu, double MinKeptMu, double EigResidual,
    List<double[]> PaddedKernel, List<double[]> Complement, bool ComplementOk);

public sealed record SectorSpectra(
    int[] K, bool SelfConj, double[] FullEigs, double[] CompEigs, double[][] CompVecs,
    double[] KernEigs, double TraceResidual, List<double[]> Complement);

public sealed record NegativeCandidate(
    string BackgroundId, string Member, double T, string SectorK, double Eigenvalue,
    double ThetaWeight, (int Type, double Weight)[] TopTypes);

public sealed record BackgroundMemberResult(
    string Member, string BackgroundId, double T,
    int Pos, int Zero, int Neg, double MaxAbs, double ZeroTol,
    int CPos, int CZero, int CNeg, double CMinEig,
    int KPos, int KZero, int KNeg, double KMinEig,
    double TracePartitionWorst, List<NegativeCandidate> Candidates)
{
    public double ThetaResidRel { get; init; }
    public double EquivResid { get; init; }
    public double[] ThetaStarInv { get; init; } = new double[3];
}

public sealed record BackgroundRecord(
    string Id, string RaySet, double T,
    int KernelTotal, int KernelK0, int KernelMinNonzero, int KernelMaxNonzero,
    List<BackgroundMemberResult> Members);

public sealed record ArmBRow(
    string Member, string CoeffId, string SectorK, int Shell, bool Sin,
    double A2, double A3, double A4, double DegRatio, double Disc,
    bool A3IsZero, double CrossWorst, double QuarticResid);
