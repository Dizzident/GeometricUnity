using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase444: mode-volume-scaled saturation probe. THE QUESTION (standing spec): does the
// Phase443 "no log-saturation" verdict change with MODE VOLUME -- i.e. does running the same
// joint effective-potential analysis on a LARGER mesh (the periodic 4-torus, 81 vertices vs
// Phase443's 16) flip any member's classification or the octave-slope structure?
//
// OUTCOME (leader-ratified): this is a RECORDED-FINDINGS study, NOT a physics run. The mandated
// engineering unlock -- block-diagonalizing the joint Hessian by lattice momentum on the torus --
// was PROTOTYPED FIRST and found NOT VIABLE on this platform, and the SLQ fallback is ~2 orders of
// magnitude over budget. modeVolumeChangesVerdict = "undetermined-tooling-blocked". The phase
// PASSES on internal consistency (precursors + the honest, reproduced diagnostic findings +
// fail-closed framing); it reports what IS and names the future-work unlocks. No physics verdict.
//
// EVIDENCE CHAIN (all recomputed here on the n=3 torus so the JSON carries real measured numbers):
//   F1  lowest-index representative-vertex rule is NOT translation-covariant (fraction of
//       (face, translation) pairs violating v(f+R)=v(f)+R).
//   F2  the operator builds face bivectors from RAW coordinate differences -> seam-crossing faces
//       are inflated (fraction of large-bivector faces; raw orbit-norm spread), and the
//       minimal-image convention (the mesh's own documented consumer contract) restores exact
//       orbit-invariance of the bivector norms.
//   F3  DEFINITIVE, convention-agnostic: even incident-average + minimal-image does NOT make the
//       operator translation-covariant. S_B(T_R x) != S_B(x) under the correctly SIGNED edge
//       permutation for all four rule/bivector conventions, AND the curvature ||F||^2 alone
//       (pure Gu.Geometry, upstream of the Shiab) is already non-covariant -- the root cause is
//       global-index orientation conventions that do not commute with lattice translation.
//   F4  the SLQ fallback cost: measured joint-objective, theta-gradient, and J_omega-column costs
//       -> extrapolated Hessian-vector-product and per-composite-point SLQ cost (over budget).
//
// FRAMING: the mode-volume heuristic (at one loop with a quartic tree action, more modes only
// rescale the subleading ~N log t term and cannot overturn the t^4-dominated runaway) is recorded
// as a LABELED, NON-VERDICT expectation; the computed verdict would supersede it either way.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage; no Phase201/Phase256
// contract field filled; nothing promoted.

const string DefaultOutputDir = "studies/phase444_mode_volume_scaled_saturation_probe_001/output";
const string Phase443SummaryPath = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string Phase442SummaryPath = "studies/phase442_joint_omega_theta_hessian_degree_probe_001/output/joint_omega_theta_hessian_degree_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "mode-volume-scaled-saturation-probe";

int TorusN = int.TryParse(Environment.GetEnvironmentVariable("PHASE444_TORUS_N"), out int tn) ? tn : 3;
const int RngSeed = 20260703;

var outputDir = Environment.GetEnvironmentVariable("PHASE444_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var stopwatch = Stopwatch.StartNew();

// ---------------------------------------------------------------------------
// Precursors: Phase443 (no-saturation on 16 vertices) + Phase442 (degree-lift).
// ---------------------------------------------------------------------------

using var phase443 = JsonDocument.Parse(File.ReadAllText(Phase443SummaryPath));
using var phase442 = JsonDocument.Parse(File.ReadAllText(Phase442SummaryPath));
bool phase443PrecursorPassed = JsonBool(phase443.RootElement, "jointEffectivePotentialSaturationProbePassed") is true;
bool phase443NoSaturation = JsonBool(phase443.RootElement, "einsteinianLogSaturationObservedAny") is false;
bool phase442PrecursorPassed =
    JsonBool(phase442.RootElement, "jointOmegaThetaHessianDegreeProbePassed") is true &&
    JsonBool(phase442.RootElement, "einsteinianJointHessianDegreeExceedsTwo") is true &&
    JsonBool(phase442.RootElement, "isolationBatteryPassed") is true;
bool precursorsPassed = phase443PrecursorPassed && phase442PrecursorPassed;

// ---------------------------------------------------------------------------
// Torus workbench.
// ---------------------------------------------------------------------------

var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(TorusN);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;
int nJoint = nOmega + nTheta;
var mass = new CpuMassMatrix(mesh, algebra);
var manifest = BuildManifest();
var geometry = BuildGeometry();

// M1b golden counts for the n=3 torus (81/1215/4050/4860/1944).
var goldens = new { vertices = 81, edges = 1215, faces = 4050, volumes = 4860, cells = 1944 };
bool meshGoldensMatch = TorusN != 3 ||
    (mesh.VertexCount == goldens.vertices && mesh.EdgeCount == goldens.edges &&
     mesh.FaceCount == goldens.faces && mesh.VolumeCount == goldens.volumes && mesh.CellCount == goldens.cells);

// Compare against Phase443's 16-vertex workbench.
int phase443Vertices = phase443.RootElement.GetProperty("probeConfiguration").GetProperty("vertexCount").GetInt32();
int phase443JointDof = phase443.RootElement.GetProperty("probeConfiguration").GetProperty("jointDof").GetInt32();
double modeVolumeRatio = nJoint / (double)phase443JointDof;

// ---------------------------------------------------------------------------
// Translation action on the (Z_n)^4 torus (vertex, edge, face maps).
// ---------------------------------------------------------------------------

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
    return coordToVertex[(((c[0] + R[0]) % TorusN + TorusN) % TorusN, ((c[1] + R[1]) % TorusN + TorusN) % TorusN,
        ((c[2] + R[2]) % TorusN + TorusN) % TorusN, ((c[3] + R[3]) % TorusN + TorusN) % TorusN)];
}
var edgeLookup = new Dictionary<(int, int), int>();
for (int e = 0; e < mesh.EdgeCount; e++) { var ee = mesh.Edges[e]; edgeLookup[(System.Math.Min(ee[0], ee[1]), System.Math.Max(ee[0], ee[1]))] = e; }
var faceLookup = new Dictionary<(int, int, int), int>();
for (int f = 0; f < mesh.FaceCount; f++) { var s = (int[])mesh.Faces[f].Clone(); Array.Sort(s); faceLookup[(s[0], s[1], s[2])] = f; }
int TranslateFace(int f, int[] R)
{
    var vv = mesh.Faces[f];
    var s = new[] { Translate(vv[0], R), Translate(vv[1], R), Translate(vv[2], R) };
    Array.Sort(s);
    return faceLookup[(s[0], s[1], s[2])];
}
var allTranslations = new List<int[]>();
for (int a = 0; a < TorusN; a++) for (int b = 0; b < TorusN; b++) for (int c = 0; c < TorusN; c++) for (int d = 0; d < TorusN; d++)
    allTranslations.Add(new[] { a, b, c, d });
var sampleTranslations = new[] { new[] { 1, 0, 0, 0 }, new[] { 0, 1, 0, 0 }, new[] { 0, 0, 1, 0 }, new[] { 0, 0, 0, 1 }, new[] { 1, 1, 0, 0 }, new[] { 2, 1, 2, 0 } };

// ---------------------------------------------------------------------------
// F1: lowest-index representative-vertex rule is NOT translation-covariant.
// ---------------------------------------------------------------------------

long repChecks = 0, repViolations = 0;
foreach (var R in allTranslations)
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int g = TranslateFace(f, R);
        repChecks++;
        if (Translate(mesh.Faces[f][0], R) != mesh.Faces[g][0]) repViolations++;
    }
double lowestIndexNonCovarianceFraction = repViolations / (double)repChecks;

// ---------------------------------------------------------------------------
// F2: raw bivectors are inflated at the seam; minimal-image restores orbit-invariance.
// ---------------------------------------------------------------------------

double[] Bivector(int f, bool minimalImage)
{
    var verts = mesh.Faces[f];
    var pa = mesh.GetVertexCoordinates(verts[0]);
    var pb = mesh.GetVertexCoordinates(verts[1]);
    var pc = mesh.GetVertexCoordinates(verts[2]);
    var u = new double[4]; var v = new double[4];
    for (int d = 0; d < 4; d++)
    {
        u[d] = pb[d] - pa[d]; v[d] = pc[d] - pa[d];
        if (minimalImage) { u[d] -= TorusN * System.Math.Round(u[d] / TorusN); v[d] -= TorusN * System.Math.Round(v[d] / TorusN); }
    }
    return Lambda2Algebra.Wedge(u, v);
}
double Norm(double[] x) { double s = 0; foreach (double t in x) s += t * t; return System.Math.Sqrt(s); }

var rawNorms = new double[mesh.FaceCount];
var minNorms = new double[mesh.FaceCount];
double minRawNorm = double.MaxValue, maxRawNorm = 0, minMinNorm = double.MaxValue, maxMinNorm = 0;
for (int f = 0; f < mesh.FaceCount; f++)
{
    rawNorms[f] = Norm(Bivector(f, false)); minNorms[f] = Norm(Bivector(f, true));
    minRawNorm = System.Math.Min(minRawNorm, rawNorms[f]); maxRawNorm = System.Math.Max(maxRawNorm, rawNorms[f]);
    minMinNorm = System.Math.Min(minMinNorm, minNorms[f]); maxMinNorm = System.Math.Max(maxMinNorm, minNorms[f]);
}
int seamFaces = rawNorms.Count(x => x > 1.5 * minRawNorm + 1e-9);
double seamFraction = seamFaces / (double)mesh.FaceCount;
double rawBivectorOrbitDiff = 0, minImageBivectorOrbitDiff = 0;
foreach (var R in sampleTranslations)
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int g = TranslateFace(f, R);
        rawBivectorOrbitDiff = System.Math.Max(rawBivectorOrbitDiff, System.Math.Abs(rawNorms[f] - rawNorms[g]));
        minImageBivectorOrbitDiff = System.Math.Max(minImageBivectorOrbitDiff, System.Math.Abs(minNorms[f] - minNorms[g]));
    }

// ---------------------------------------------------------------------------
// F3: DEFINITIVE convention-agnostic covariance test S_B(T_R x) == S_B(x)
// under the SIGNED edge permutation (connection 1-form negates on edge reversal)
// + unsigned vertex permutation. Tested for all four rule/bivector conventions,
// plus the curvature ||F||^2 alone (upstream of the Shiab).
// ---------------------------------------------------------------------------

(double[] omega, double[] theta) SignedPermute(double[] omega, double[] theta, int[] R)
{
    var no = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        var ee = mesh.Edges[e];
        int a = Translate(ee[0], R), b = Translate(ee[1], R);
        int e2 = edgeLookup[(System.Math.Min(a, b), System.Math.Max(a, b))];
        int sign = a < b ? 1 : -1;
        for (int c = 0; c < dimG; c++) no[e2 * dimG + c] = sign * omega[e * dimG + c];
    }
    var nt = new double[nTheta];
    for (int v = 0; v < mesh.VertexCount; v++)
        for (int c = 0; c < dimG; c++) nt[Translate(v, R) * dimG + c] = theta[v * dimG + c];
    return (no, nt);
}

FieldTensor FaceTensor(double[] coeffs) => new()
{
    Label = "coeffs",
    Signature = new TensorSignature
    {
        AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
        LieAlgebraBasisId = algebra.BasisOrderId, ComponentOrderId = "face-major",
        NumericPrecision = "float64", MemoryLayout = "dense-row-major",
    },
    Coefficients = coeffs, Shape = new[] { mesh.FaceCount, dimG },
};

double SB(EinsteinianShiabOperator op, double[] omega, double[] theta)
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    var s = FaceTensor(op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry).Coefficients);
    return 0.5 * mass.InnerProduct(s, s);
}

EinsteinianShiabFamilyMember Member(VertexFaceRule rule) => new()
{
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta", VertexFaceRule = rule,
};

double CovarianceResidual(VertexFaceRule rule, bool minimalImage)
{
    var op = new EinsteinianShiabOperator(mesh, algebra, Member(rule), null, 1.0, minimalImage ? TorusN : 0);
    var rng = new Random(RngSeed);
    double maxRel = 0;
    for (int trial = 0; trial < 4; trial++)
    {
        var omega = new double[nOmega]; for (int i = 0; i < nOmega; i++) omega[i] = 0.4 * (rng.NextDouble() - 0.5);
        var theta = new double[nTheta]; for (int i = 0; i < nTheta; i++) theta[i] = 0.2 * (rng.NextDouble() - 0.5);
        double s0 = SB(op, omega, theta);
        foreach (var R in sampleTranslations)
        {
            var (po, pt) = SignedPermute(omega, theta, R);
            maxRel = System.Math.Max(maxRel, System.Math.Abs(SB(op, po, pt) - s0) / (System.Math.Abs(s0) + 1e-300));
        }
    }
    return maxRel;
}

double covLowestRaw = CovarianceResidual(VertexFaceRule.LowestIndex, false);
double covLowestMin = CovarianceResidual(VertexFaceRule.LowestIndex, true);
double covAvgRaw = CovarianceResidual(VertexFaceRule.IncidentAverage, false);
double covAvgMin = CovarianceResidual(VertexFaceRule.IncidentAverage, true);

// Curvature ||F||^2 alone (no Shiab): isolates the upstream Gu.Geometry non-covariance.
double CurvNorm2(double[] omega)
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = FaceTensor(CurvatureAssembler.Assemble(conn).ToFieldTensor().Coefficients);
    return mass.InnerProduct(f, f);
}
double covCurvatureOnly;
{
    var rng = new Random(RngSeed + 5);
    double maxRel = 0;
    for (int trial = 0; trial < 4; trial++)
    {
        var omega = new double[nOmega]; for (int i = 0; i < nOmega; i++) omega[i] = 0.4 * (rng.NextDouble() - 0.5);
        double c0 = CurvNorm2(omega);
        foreach (var R in sampleTranslations)
        {
            var (po, _) = SignedPermute(omega, new double[nTheta], R);
            maxRel = System.Math.Max(maxRel, System.Math.Abs(CurvNorm2(po) - c0) / (System.Math.Abs(c0) + 1e-300));
        }
    }
    covCurvatureOnly = maxRel;
}
double bestCovarianceResidual = System.Math.Min(System.Math.Min(covLowestRaw, covLowestMin), System.Math.Min(covAvgRaw, covAvgMin));
const double BlockDiagBar = 1e-8; // the block-vs-dense sampled-entry bar from the standing spec
bool blockDiagonalizationViable = bestCovarianceResidual < BlockDiagBar;

// Gating controls that DO hold for the incident-average rule (recorded positives).
double avgLinearizeThetaFdResidual;
double openMeshThetaZeroByteIdentity;
{
    var opAvg = new EinsteinianShiabOperator(mesh, algebra, Member(VertexFaceRule.IncidentAverage), null, 1.0, TorusN);
    var rng = new Random(RngSeed + 9);
    var omega = new double[nOmega]; for (int i = 0; i < nOmega; i++) omega[i] = 0.2 * (rng.NextDouble() - 0.5);
    var theta = new double[nTheta]; for (int i = 0; i < nTheta; i++) theta[i] = 0.15 * (rng.NextDouble() - 0.5);
    var dth = new double[nTheta]; for (int i = 0; i < nTheta; i++) dth[i] = rng.NextDouble() - 0.5;
    avgLinearizeThetaFdResidual = EinsteinianShiabBatteries.LinearizeThetaFdResidual(opAvg, mesh, algebra, omega, theta, dth, manifest, geometry);

    // Open-mesh byte-identity: avg vs lowest-index at theta=0 (both = M(F)).
    var openMesh = SimplicialMeshGenerator.CreateUniform4D(1);
    int nThetaOpen = openMesh.VertexCount * dimG;
    var omO = new double[openMesh.EdgeCount * dimG]; for (int i = 0; i < omO.Length; i++) omO[i] = 0.3 * (rng.NextDouble() - 0.5);
    var connO = new ConnectionField(openMesh, algebra, omO);
    var fO = CurvatureAssembler.Assemble(connO).ToFieldTensor();
    var opLowO = new EinsteinianShiabOperator(openMesh, algebra, Member(VertexFaceRule.LowestIndex));
    var opAvgO = new EinsteinianShiabOperator(openMesh, algebra, Member(VertexFaceRule.IncidentAverage));
    var th0 = new double[nThetaOpen];
    var sLowO = opLowO.EvaluateWithTheta(fO, connO.ToFieldTensor(), th0, manifest, geometry).Coefficients;
    var sAvgO = opAvgO.EvaluateWithTheta(fO, connO.ToFieldTensor(), th0, manifest, geometry).Coefficients;
    double d = 0; for (int i = 0; i < sLowO.Length; i++) d = System.Math.Max(d, System.Math.Abs(sLowO[i] - sAvgO[i]));
    openMeshThetaZeroByteIdentity = d;
}

// ---------------------------------------------------------------------------
// F4: SLQ-fallback cost micro-benchmark and extrapolation.
// ---------------------------------------------------------------------------

double evalMs, thetaGradientSeconds, jOmegaColumnMs;
{
    var op = new EinsteinianShiabOperator(mesh, algebra, Member(VertexFaceRule.IncidentAverage), null, 1.0, TorusN);
    var rng = new Random(RngSeed + 11);
    var omega = new double[nOmega]; for (int i = 0; i < nOmega; i++) omega[i] = 0.3 * (rng.NextDouble() - 0.5);
    var theta = new double[nTheta]; for (int i = 0; i < nTheta; i++) theta[i] = 0.1 * (rng.NextDouble() - 0.5);
    var conn = new ConnectionField(mesh, algebra, omega);
    var fTensor = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    var connTensor = conn.ToFieldTensor();

    var sw = Stopwatch.StartNew();
    int nEval = 30; double acc = 0;
    for (int i = 0; i < nEval; i++)
    {
        var s = FaceTensor(op.EvaluateWithTheta(fTensor, connTensor, theta, manifest, geometry).Coefficients);
        acc += mass.InnerProduct(s, s);
    }
    sw.Stop(); evalMs = sw.Elapsed.TotalMilliseconds / nEval;

    sw.Restart();
    var sH = FaceTensor(op.EvaluateWithTheta(fTensor, connTensor, theta, manifest, geometry).Coefficients);
    for (int k = 0; k < nTheta; k++)
    {
        var e = new double[nTheta]; e[k] = 1.0;
        var jk = FaceTensor(op.LinearizeTheta(fTensor, connTensor, theta, e, manifest, geometry).Coefficients);
        acc += mass.InnerProduct(sH, jk);
    }
    sw.Stop(); thetaGradientSeconds = sw.Elapsed.TotalSeconds;

    sw.Restart();
    int nCol = 20;
    for (int k = 0; k < nCol; k++)
    {
        var e = new double[nOmega]; e[k] = 1.0;
        var dF = CovariantExterior(omega, e);
        var jk = op.EvaluateWithTheta(FaceTensor(dF), connTensor, theta, manifest, geometry).Coefficients;
        acc += jk[0];
    }
    sw.Stop(); jOmegaColumnMs = sw.Elapsed.TotalMilliseconds / nCol;
    _ = acc;
}
double fullJointGradientSeconds = jOmegaColumnMs * nOmega / 1000.0 + thetaGradientSeconds;
double slqHessianVectorSeconds = 2.0 * fullJointGradientSeconds;
int slqProbesPerPoint = 120; // ~15 Lanczos steps x ~8 probes (standing-spec class)
double slqPerCompositePointHours = slqHessianVectorSeconds * slqProbesPerPoint / 3600.0;

// ---------------------------------------------------------------------------
// Verdict, framing, fail-closed, recorded boundary.
// ---------------------------------------------------------------------------

// The evidence chain is internally consistent iff all findings hold as measured.
bool f1RepresentativeVertexNonCovariant = lowestIndexNonCovarianceFraction > 0.5;
bool f2RawBivectorsInflated = seamFraction > 0.5 && rawBivectorOrbitDiff > 1.0;
bool f2MinimalImageRestoresBivectorOrbitInvariance = minImageBivectorOrbitDiff < 1e-12;
bool f3AllConventionsNonCovariant = covLowestRaw > BlockDiagBar && covLowestMin > BlockDiagBar && covAvgRaw > BlockDiagBar && covAvgMin > BlockDiagBar;
bool f3CurvatureAloneNonCovariant = covCurvatureOnly > BlockDiagBar;
bool f4SlqOverBudget = slqPerCompositePointHours > 1.0;
bool avgGatingControlsHold = avgLinearizeThetaFdResidual < 1e-6 && openMeshThetaZeroByteIdentity == 0.0;

bool findingsInternallyConsistent =
    meshGoldensMatch &&
    f1RepresentativeVertexNonCovariant &&
    f2RawBivectorsInflated &&
    f2MinimalImageRestoresBivectorOrbitInvariance &&
    f3AllConventionsNonCovariant &&
    f3CurvatureAloneNonCovariant &&
    f4SlqOverBudget &&
    avgGatingControlsHold &&
    !blockDiagonalizationViable;

const string ModeVolumeChangesVerdict = "undetermined-tooling-blocked";

// Mode-volume heuristic (LABELED, NON-VERDICT expectation).
const string modeVolumeHeuristic =
    "At one loop with a quartic tree action S_B ~ t^4, additional modes rescale only the subleading " +
    "~N log t one-loop term and cannot overturn the t^4-dominated runaway asymptotics (t^4 >> N log t " +
    "as t -> infinity for any fixed mode count N). This LABELED expectation suggests mode volume alone " +
    "would NOT flip Phase443's no-saturation verdict -- consistent with the Phase437 lesson that a new " +
    "MECHANISM, not just mode volume, is what is missing. It is NOT a computed verdict; the block-path " +
    "or SLQ computation (future work) would supersede it either way.";

// Two NAMED future-work unlocks.
var namedUnlocks = new[]
{
    new
    {
        id = "lattice-canonical-geometry-conventions",
        kind = "user-decision-major-platform-investment",
        scope = "MeshTopologyBuilder orientation/sort conventions + CurvatureAssembler + the Einsteinian Shiab per-cell assembly (Gu.Geometry): replace global-index-sorted orientation/bivector/cell-face conventions with lattice-canonical ones so the whole stack commutes with lattice translation on a periodic mesh.",
        evidenceThatDemandsIt = "F3: even incident-average + minimal-image leaves the operator non-covariant; the curvature ||F||^2 alone is already non-covariant (global-index conventions do not commute with lattice translation).",
        expectedOutcome = "exact block-diagonalization of the joint Hessian by lattice momentum on tori (81 blocks of 48x48 for su(2) at n=3), giving exact eigenvalues that match Phase443's Jacobi convention -- enabling the mode-volume physics run cheaply.",
        started = false,
    },
    new
    {
        id = "adjoint-joint-gradient-platform-path",
        kind = "engineering-enabler-for-slq",
        scope = "expose an analytic joint (omega,theta) gradient / contraction-adjoint on EinsteinianShiabOperator so a Hessian-vector product costs O(mesh) instead of O(nDOF x mesh).",
        evidenceThatDemandsIt = "F4: through public forward-only methods a faithful Hessian-vector product costs ~" + $"{slqHessianVectorSeconds:F0}" + " s, so a single composite point's SLQ tr-log is ~" + $"{slqPerCompositePointHours:F1}" + " h -- ~2 orders of magnitude over the 60-min budget.",
        expectedOutcome = "SLQ tr-log of the joint Hessian becomes feasible (variance-bounded estimates, NOT exact eigenvalue counts) on the torus without the geometry rewrite.",
        started = false,
    },
};

// Recorded boundary (six verbatim keys) + the two Phase444 recorded flags.
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase256Contract = false;
const bool minimalImageBivectorsOnPeriodicMeshes = true;
const bool physicistReviewPending = true;

// Standard fail-closed block.
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
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; CreateUniform4DPeriodic({TorusN}); V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nJoint={nJoint}",
    "recorded-findings: block-diag non-viable (global-index orientation conventions non-covariant), SLQ over budget; modeVolumeChangesVerdict=undetermined-tooling-blocked; no target values")))).ToLowerInvariant();

bool phase444Passed =
    precursorsPassed &&
    findingsInternallyConsistent &&
    ModeVolumeChangesVerdict == "undetermined-tooling-blocked" &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized && !internalGaugeContentRealized && !weldRealized &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction && !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian && !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization && !routeCompletesBosonPredictions &&
    !routePromotesWzMasses && !routePromotesHiggsMass && !sourceContractApplicationAllowed &&
    !phase201TemplateMutated && fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract && !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = phase444Passed
    ? "mode-volume-scaled-saturation-probe-passed-tooling-blocked-block-diagonalization-non-viable-slq-over-budget-verdict-undetermined"
    : "mode-volume-scaled-saturation-probe-blocked";

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

string decision = phase444Passed
    ? $"Phase444 is decided on internal consistency as a RECORDED-FINDINGS study; it carries NO physics verdict. " +
      $"The mode-volume question (does the Phase443 no-saturation verdict change on a larger mesh?) is left " +
      $"modeVolumeChangesVerdict=\"undetermined-tooling-blocked\" because the mandated engineering unlock -- " +
      $"block-diagonalizing the joint Hessian by lattice momentum on the periodic {mesh.VertexCount}-vertex torus " +
      $"(mode-volume ratio {modeVolumeRatio:F1}x over Phase443's {phase443Vertices} vertices) -- is NOT viable on " +
      $"this platform, and the SLQ fallback is ~{slqPerCompositePointHours:F1} h per composite point (~2 orders over budget). " +
      $"EVIDENCE: (F1) the lowest-index representative-vertex rule violates translation covariance on {lowestIndexNonCovarianceFraction:P1} of " +
      $"(face, translation) pairs; (F2) {seamFraction:P1} of faces are seam-inflated with raw bivectors (orbit-norm spread {rawBivectorOrbitDiff:F1}), " +
      $"and the minimal-image convention restores exact bivector orbit-invariance ({minImageBivectorOrbitDiff:E1}); (F3) DEFINITIVE -- even " +
      $"incident-average + minimal-image leaves S_B non-covariant under the signed permutation (best residual {bestCovarianceResidual:E2} >> {BlockDiagBar:E0} bar), " +
      $"and the curvature ||F||^2 alone is already non-covariant ({covCurvatureOnly:E2}); the root cause is global-index orientation conventions " +
      $"that do not commute with lattice translation; (F4) a faithful Hessian-vector product costs ~{slqHessianVectorSeconds:F0} s. " +
      $"The authorized platform change (incident-average VertexFaceRule + minimal-image bivectors) is verified-correct and open-mesh byte-identical " +
      $"(theta=0 low-vs-avg diff {openMeshThetaZeroByteIdentity:E1}; avg LinearizeTheta-vs-FD {avgLinearizeThetaFdResidual:E2}) -- necessary-but-insufficient. " +
      $"Two named unlocks are recorded as future work. MANDATORY FRAMING: the mode-volume heuristic is a LABELED, NON-VERDICT expectation. " +
      $"Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use this study: precursors or the recorded-findings internal-consistency checks did not pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase444-mode-volume-scaled-saturation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    phase444Passed,
    applicationSubjectKind = ApplicationSubjectKind,
    studyKind = "recorded-findings (engineering/architecture); no physics verdict",

    modeVolumeChangesVerdict = ModeVolumeChangesVerdict,
    blockDiagonalizationViable,

    precursors = new
    {
        phase443PrecursorPassed,
        phase443NoSaturation,
        phase442PrecursorPassed,
        precursorsPassed,
        phase443Vertices,
        phase443JointDof,
    },

    torusWorkbench = new
    {
        torusN = TorusN,
        lieAlgebraId = "su2-trace-pairing",
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        faceCount = mesh.FaceCount,
        volumeCount = mesh.VolumeCount,
        cellCount = mesh.CellCount,
        nOmega, nTheta, jointDof = nJoint,
        meshGoldensMatch,
        goldens,
        modeVolumeRatioOverPhase443 = modeVolumeRatio,
    },

    findings = new
    {
        f1_lowestIndexRepresentativeVertex = new
        {
            note = "Lowest-index rule v(f)=Faces[f][0] is NOT translation-covariant: the row-major index wrap reorders which incident vertex is smallest.",
            checks = repChecks,
            violations = repViolations,
            nonCovarianceFraction = lowestIndexNonCovarianceFraction,
            covariant = false,
        },
        f2_bivectorMinimalImage = new
        {
            note = "Operator builds face bivectors from RAW coordinate differences; seam-crossing faces are inflated (up to period-1). Minimal-image reduction restores exact orbit-invariance of the bivector norms.",
            rawNormMin = minRawNorm, rawNormMax = maxRawNorm, rawNormRatio = maxRawNorm / minRawNorm,
            seamFaces, seamFraction,
            rawBivectorOrbitDiff,
            minimalImageNormMin = minMinNorm, minimalImageNormMax = maxMinNorm, minimalImageNormRatio = maxMinNorm / minMinNorm,
            minimalImageBivectorOrbitDiff = minImageBivectorOrbitDiff,
            minimalImageRestoresOrbitInvariance = f2MinimalImageRestoresBivectorOrbitInvariance,
        },
        f3_definitiveCovariance = new
        {
            note = "Convention-agnostic: S_B(T_R x) vs S_B(x) under SIGNED edge permutation + unsigned vertex permutation; max relative deviation over sampled translations & random x. All four conventions fail the block-diagonalization bar; curvature ||F||^2 alone (upstream, Gu.Geometry) is already non-covariant.",
            blockDiagonalizationBar = BlockDiagBar,
            covarianceResidual = new
            {
                lowestIndex_rawBivectors = covLowestRaw,
                lowestIndex_minimalImage = covLowestMin,
                incidentAverage_rawBivectors = covAvgRaw,
                incidentAverage_minimalImage = covAvgMin,
                curvatureOnly_noShiab = covCurvatureOnly,
            },
            bestResidual = bestCovarianceResidual,
            allConventionsNonCovariant = f3AllConventionsNonCovariant,
            curvatureAloneNonCovariant = f3CurvatureAloneNonCovariant,
            rootCause = "global-index-sorted orientation conventions (mesh face-boundary orientation signs, bivector vertex-sort parity, cell-face ordering) do not commute with lattice translation on a periodic mesh.",
            supersededEarlierMeasurementNote = "Earlier theta-Hessian residuals (1.6e-2 lowest-index / 8.7e-3 averaged) used a background that ignored edge-orientation signs, so H(x) and H(T_R x) were Hessians at different physical points -- a confound. This signed-S_B test is the clean, convention-agnostic measurement and supersedes those; the qualitative conclusion is unchanged.",
        },
        f4_slqCost = new
        {
            note = "Platform exposes only O(full-mesh) forward evaluations (no adjoint/joint-gradient), so every gradient costs O(nDOF x mesh). Measured on the torus, extrapolated to SLQ.",
            jointObjectiveEvalMs = evalMs,
            fullThetaGradientSeconds = thetaGradientSeconds,
            oneJOmegaColumnMs = jOmegaColumnMs,
            fullJointGradientSeconds,
            slqHessianVectorSeconds,
            slqProbesPerPointAssumed = slqProbesPerPoint,
            slqPerCompositePointHours,
            overBudget = f4SlqOverBudget,
        },
    },

    authorizedPlatformChange = new
    {
        note = "Leader-authorized (option A'): additive VertexFaceRule.IncidentAverage on the member + minimal-image latticePeriod ctor flag on the operator. Verified-correct and necessary-but-insufficient for the block path.",
        vertexFaceRulePrimary = "incident-average(translation-covariant vertex->face attachment)",
        minimalImageBivectorsOnPeriodicMeshes,
        physicistReviewPending,
        openMeshThetaZeroByteIdentity,
        openMeshByteIdentical = openMeshThetaZeroByteIdentity == 0.0,
        avgLinearizeThetaFdResidual,
        avgGatingControlsHold,
        pinnedRuleNonCovarianceRecordedLimitation = lowestIndexNonCovarianceFraction,
    },

    namedFutureWorkUnlocks = namedUnlocks,

    modeVolumeHeuristic = new
    {
        label = "LABELED NON-VERDICT expectation (superseded by any computed verdict)",
        statement = modeVolumeHeuristic,
    },

    internalConsistency = new
    {
        findingsInternallyConsistent,
        f1RepresentativeVertexNonCovariant,
        f2RawBivectorsInflated,
        f2MinimalImageRestoresBivectorOrbitInvariance,
        f3AllConventionsNonCovariant,
        f3CurvatureAloneNonCovariant,
        f4SlqOverBudget,
        avgGatingControlsHold,
        blockDiagonalizationViable,
    },

    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        minimalImageBivectorsOnPeriodicMeshes,
        physicistReviewPending,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
        draft14dObserverseNotRealized = true,
    },

    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,

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

    explicitCandidateOnlyNonclaims = new[]
    {
        "This is a recorded-findings (engineering/architecture) study; it carries NO physics verdict on saturation or mode volume.",
        "modeVolumeChangesVerdict is undetermined because the platform cannot block-diagonalize the periodic-mesh joint Hessian and the dense/SLQ alternatives are over budget.",
        "The mode-volume heuristic is a LABELED expectation, not a computed result; a future block-path or SLQ run would supersede it.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    sourceEvidence = new
    {
        phase443SummaryPath = Phase443SummaryPath,
        phase442SummaryPath = Phase442SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    runtimeSeconds,
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "mode_volume_scaled_saturation_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "mode_volume_scaled_saturation_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase444Passed={phase444Passed}");
Console.WriteLine($"precursors: p443={phase443PrecursorPassed} (noSat={phase443NoSaturation}) p442={phase442PrecursorPassed}");
Console.WriteLine($"torus: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} Vol={mesh.VolumeCount} C={mesh.CellCount} goldensMatch={meshGoldensMatch}; nJoint={nJoint} (modeVolume {modeVolumeRatio:F1}x over p443)");
Console.WriteLine($"F1 lowest-index non-covariance: {lowestIndexNonCovarianceFraction:P1} ({repViolations}/{repChecks})");
Console.WriteLine($"F2 seam faces {seamFraction:P1}; raw orbit-diff {rawBivectorOrbitDiff:F2}; minimal-image orbit-diff {minImageBivectorOrbitDiff:E2}");
Console.WriteLine($"F3 covariance residual: low/raw={covLowestRaw:E2} low/min={covLowestMin:E2} avg/raw={covAvgRaw:E2} avg/min={covAvgMin:E2} curvatureOnly={covCurvatureOnly:E2} (bar {BlockDiagBar:E0}) -> blockViable={blockDiagonalizationViable}");
Console.WriteLine($"F4 SLQ: eval={evalMs:F2}ms thetaGrad={thetaGradientSeconds:F1}s Jcol={jOmegaColumnMs:F2}ms -> Hv~{slqHessianVectorSeconds:F0}s per-point~{slqPerCompositePointHours:F1}h");
Console.WriteLine($"authorized change: openMeshByteIdentity={openMeshThetaZeroByteIdentity:E1} avgLinTheta-vs-FD={avgLinearizeThetaFdResidual:E2}");
Console.WriteLine($"modeVolumeChangesVerdict={ModeVolumeChangesVerdict}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");
Console.WriteLine($"Passed={phase444Passed}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

double[] CovariantExterior(double[] om, double[] delta)
{
    var result = new double[mesh.FaceCount * dimG];
    for (int fi = 0; fi < mesh.FaceCount; fi++)
    {
        var be = mesh.FaceBoundaryEdges[fi];
        var bo = mesh.FaceBoundaryOrientations[fi];
        var dD = new double[dimG];
        for (int i = 0; i < be.Length; i++)
            for (int a = 0; a < dimG; a++) dD[a] += bo[i] * delta[be[i] * dimG + a];
        var br = new double[dimG];
        for (int i = 0; i < be.Length; i++)
            for (int j = i + 1; j < be.Length; j++)
            {
                int ei = be[i], ej = be[j]; int si = bo[i], sj = bo[j];
                var oi = new double[dimG]; var oj = new double[dimG]; var di = new double[dimG]; var dj = new double[dimG];
                for (int a = 0; a < dimG; a++)
                { oi[a] = si * om[ei * dimG + a]; oj[a] = sj * om[ej * dimG + a]; di[a] = si * delta[ei * dimG + a]; dj[a] = sj * delta[ej * dimG + a]; }
                var b1 = algebra.Bracket(oi, dj); var b2 = algebra.Bracket(di, oj);
                for (int a = 0; a < dimG; a++) br[a] += 0.5 * (b1[a] + b2[a]);
            }
        for (int a = 0; a < dimG; a++) result[fi * dimG + a] = dD[a] + br[a];
    }
    return result;
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase444-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase444",
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
        ProjectionBinding = new GeometryBinding { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
        ObservationBinding = new GeometryBinding { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
        Patches = Array.Empty<PatchInfo>(),
    };
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;
