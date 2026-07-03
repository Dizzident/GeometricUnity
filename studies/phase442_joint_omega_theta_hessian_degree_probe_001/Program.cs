using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase442: joint (omega, theta) Hessian-degree probe on the draft-canonical
// Einsteinian Shiab (4D base, spinor-realized reduced Lambda^2 slice,
// epsilon-conjugation). The ratified §3.9 first physics study.
//
// THE QUESTION (design §3.9 / physics-decisions §0, §5, §6e). Phases 436/441
// proved a no-go: on the toy family the residual Upsilon = S(F) - T is exactly
// degree-2 in omega, so the objective S_B = (1/2)||Upsilon||^2 is degree-4, the
// exact Hessian along a background t*u is degree-2 in t, masses^2 grow exactly as
// t^2, the one-loop potential grows exactly as log(t), and NO log-saturation (no
// dynamical scale) can arise. Does the faithful 4D Einsteinian Shiab, with the
// INDEPENDENT-theta epsilon sector switched on, break that exact-quadraticity of
// the JOINT (omega, theta) Hessian?
//
// THE EPSILON REALIZATION IS PINNED (design §3.5, CO-SIGNED & RATIFIED dc7ddd36):
// theta is a genuine INDEPENDENT H-valued DOF on VERTICES (VertexCount x dimG),
// NOT a function of omega. The Hessian is over the JOINT (omega, theta). The
// per-face conjugator Ad_eps = exp(ad_theta) uses theta at the face's
// representative vertex; the slaved Wilson eps(omega) form is a labelled
// NON-GATING smoke-test only. epsilonRealization = "independent-theta-dof".
//
// THE MECHANISM (recorded, verified below, not assumed). S_h(F)_face =
// M(Ad_theta(F))_face, where M is the per-cell Lambda^2 contraction (acting on the
// spatial/face index) and Ad_theta acts on the ad/Lie index. For the identity-
// equivalent member (Phi1=id0, Phi2=none) R = identity on Lambda^2, so the per-cell
// face map M-I = W^T(R-I)Q = 0: there is NO face mixing, S_h_face = Ad_theta(F_face),
// and because the trace pairing is Ad-invariant ||Ad(F)|| = ||F||, so theta cancels
// from the objective (the theta-block is degenerate; degree stays 2). For a
// genuinely non-scalar R (sd2 = P_+, asd2 = P_-) the face map mixes faces carrying
// DIFFERENT per-face Ad rotations, theta survives into S_B, and the all-orders
// exp(ad_theta) non-polynomiality lifts the joint-Hessian degree above 2. So any
// degree-lift is attributable to the non-scalar Shiab, not to inserting the theta
// DOF -- exactly what the isolation battery certifies.
//
// FRAMING (physicist, mandatory, verbatim intent). A degree > 2 verdict is the
// NECESSARY condition for log-saturation, NOT sufficient, and is NOT a scale: no
// scale, pole, VEV, or GeV is produced. Extracting an actual scale requires the
// DEFERRED next study: the joint effective potential + variational eps*(omega)
// (solve dS/deps = 0, integrate epsilon out) + Coleman-Weinberg / gap-equation
// saturation analysis (the Phase435/438 machinery on the lifted Hessian). A
// degree-2 Einsteinian result is a legitimate frontier-sharpening outcome, not a
// failure. The phase PASSES on internal consistency (precursors + control arm +
// isolation battery + honesty sweep + cross-check) REGARDLESS of the treatment
// verdict; it reports what IS.
//
// Fail-closed: target-blind construction; reduced-spin4-slice; no scales/poles/GeV
// lineage; no Phase201/Phase256 contract field filled; nothing promoted.

const string DefaultOutputDir = "studies/phase442_joint_omega_theta_hessian_degree_probe_001/output";
const string Phase436SummaryPath = "studies/phase436_exact_hessian_saturation_no_go_probe_001/output/exact_hessian_saturation_no_go_probe_summary.json";
const string Phase441SummaryPath = "studies/phase441_toy_branch_family_universality_sweep_001/output/toy_branch_family_universality_sweep_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "joint-omega-theta-hessian-degree-probe";

// Probe geometry / finite-difference schedule (Phase436 method, joint DOF).
// CreateUniform4D(1) is the committed default; (2) runs opportunistically via the
// PHASE442_MESH_REFINEMENT override (design §3.9 "and (2) if runtime permits").
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE442_MESH_REFINEMENT"), out int mr) ? mr : 1;
const double RayT0 = 0.20;             // background ray base parameter
const double RayDt = 0.15;             // ray step; t in [t0, t0 + 5*dt]
const double ProbeDs = 0.05;           // second-derivative probe step for the Hessian
const int RaySeedCount = 4;            // random joint (u, v) directions per member
const double ThetaAmplitude = 1.0;     // |u_theta| knob for the main arms (honesty sweep varies it)
const int RngSeed = 20260702;

// Verdict thresholds. "Degree exceeds two" means the objective's 5th t-difference is
// RESOLVABLY above the machine-exact degree-4 floor the control reproduces -- i.e. it
// is defined RELATIVE to that same-mesh machine floor, not against an arbitrary fixed
// constant (a fixed *relative* tolerance is mesh-dependent because the relative
// 5th-difference dilutes as the objective sums over more faces, which spuriously moved
// the n=2 incident-average verdict below a fixed 1e-7 even though its lift was ~1e7x
// machine noise). The threshold is a large multiple of the control floor with an
// absolute backstop, giving ~3-4 orders of clearance on both sides.
const double NoiseMultipleForDegree = 1e4;       // deg>2 iff D5rel > this * control machine floor ...
const double AbsoluteDegreeBackstop = 1e-11;     // ... but never below this absolute backstop
const double MachineDegreeFloor = 1e-8;          // control must be BELOW this (degree-2/degree-4 exact)
const double CrossCheckTolerance = 1e-10;        // study-side Ad machinery vs operator EvaluateWithTheta
const double RichnessFloor = 1e-9;               // Lambda^2 off-scalar Frobenius floor
const int IsolationSubspaceDim = 8;              // theta-block sampling dimension

var outputDir = Environment.GetEnvironmentVariable("PHASE442_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: Phase436 exact-Hessian no-go + Phase441 toy-family universality.
// ---------------------------------------------------------------------------

using var phase436 = JsonDocument.Parse(File.ReadAllText(Phase436SummaryPath));
using var phase441 = JsonDocument.Parse(File.ReadAllText(Phase441SummaryPath));

bool phase436PrecursorPassed =
    JsonBool(phase436.RootElement, "exactHessianSaturationNoGoProbePassed") is true &&
    JsonBool(phase436.RootElement, "scaleGapPinnedBeyondControlBranch") is true;
bool phase441PrecursorPassed =
    JsonBool(phase441.RootElement, "toyBranchFamilyUniversalitySweepPassed") is true;
bool precursorsPassed = phase436PrecursorPassed && phase441PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery: 4D mesh, su(2) trace pairing (positive-definite; project memory),
// trivial torsion (Upsilon = S_h), mass matrix, manifest + geometry.
// ---------------------------------------------------------------------------

var mesh = SimplicialMeshGenerator.CreateUniform4D(MeshRefinement);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;

var trivialTorsion = new TrivialTorsionCpu(mesh, algebra);
var mass = new CpuMassMatrix(mesh, algebra);
var manifest = BuildManifest();
var geometry = BuildGeometry();

// ---------------------------------------------------------------------------
// Family members.
// ---------------------------------------------------------------------------

// Control anchor: identity-equivalent {Phi1=id0, Phi2=none, trivial}. R = identity
// on Lambda^2 => S_h = F exactly (proven in EinsteinianShiab4DTests). theta-blind.
var controlMember = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Id0,
    Phi2 = InvariantElementSpec.None,
    EpsilonMode = "trivial",
};
var controlOp = new EinsteinianShiabOperator(mesh, algebra, controlMember);

// Treatment members: {sd2-id0, asd2-id0, sd2-none} x c in {0, 0.5, 1}, independent-theta.
var treatmentMembers = new List<(string Name, EinsteinianShiabFamilyMember Member)>();
foreach (var (phi1, phi2, tag) in new[]
         {
             (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0"),
             (InvariantElementSpec.Asd2, InvariantElementSpec.Id0, "asd2-id0"),
             (InvariantElementSpec.Sd2, InvariantElementSpec.None, "sd2-none"),
         })
foreach (double c in new[] { 0.0, 0.5, 1.0 })
{
    var member = new EinsteinianShiabFamilyMember
    {
        Phi1 = phi1,
        Phi2 = phi2,
        EinsteinCoefficient = c,
        EpsilonMode = "independent-theta",
    };
    treatmentMembers.Add(($"{tag}/c{c:0.###}", member));
}

// ---------------------------------------------------------------------------
// shiabEval builders: (omega, theta) -> S_h coefficients.
// ---------------------------------------------------------------------------

// Control / isolation eval: theta-blind identity-equivalent (uses the interface
// Evaluate = theta=0 path; theta argument ignored). theta is literally absent from
// Upsilon, so any theta finite-difference is bit-exact zero.
Func<double[], double[], double[]> ControlEval() => (omega, _) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    return controlOp.Evaluate(f, conn.ToFieldTensor(), manifest, geometry).Coefficients;
};

// Pinned vertex->face rule: the SHIPPED instrument. Ad at the face's lowest-index
// incident vertex, via EinsteinianShiabOperator.EvaluateWithTheta.
Func<double[], double[], double[]> PinnedEval(EinsteinianShiabOperator op) =>
    EinsteinianShiabBatteries.EinsteinianThetaEval(op, mesh, algebra, manifest, geometry);

// Incident-average vertex->face rule (mandated robustness variant, design §3.5).
// Implemented study-side: pre-rotate F per face by exp(ad_thetaFaceAvg), then the
// operator's theta=0 Evaluate applies the pure M contraction. Because Ad acts on
// the ad-index face-by-face BEFORE the linear M mixing, this is exactly
// M(Ad_avg(F)) -- identical machinery to EvaluateWithTheta but with the averaged
// per-face theta (verified for the lowest-index rule by the cross-check battery).
Func<double[], double[], double[]> AverageEval(EinsteinianShiabOperator op) => (omega, theta) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var fCoeffs = CurvatureAssembler.Assemble(conn).ToFieldTensor().Coefficients;
    var fRot = PreRotateFacewise(fCoeffs, theta, average: true);
    return op.Evaluate(FaceTensor(mesh, algebra, fRot), conn.ToFieldTensor(), manifest, geometry).Coefficients;
};

// ---------------------------------------------------------------------------
// Joint objective + degree probes (Phase436 finite-difference machinery on the
// enlarged (omega, theta) DOF vector).
// ---------------------------------------------------------------------------

double Jobj(Func<double[], double[], double[]> eval, double[] omega, double[] theta) =>
    EinsteinianShiabBatteries.JointObjective(
        eval, trivialTorsion, mass, mesh, algebra, omega, theta, manifest, geometry);

var stopwatch = Stopwatch.StartNew();

// Fifth t-difference of S_B along the joint ray (omega=t*uOmega, theta=t*uTheta).
// S_B degree-4 in t  <=>  D5 = 0. Degree > 4  <=>  D5 != 0. Relative to the sample
// magnitude. Also returns the third t-difference of the Hessian probe h(t)=v^T H v.
(double D5ObjRel, double D3HessRel, double D5ObjAbs, double D3HessAbs) DegreeProbe(
    Func<double[], double[], double[]> eval, Ray ray)
{
    // Objective fifth difference: samples S_B at t0 + k*dt, k = 0..5.
    var s = new double[6];
    for (int k = 0; k < 6; k++)
    {
        double t = RayT0 + k * RayDt;
        s[k] = Jobj(eval, Scale(ray.UOmega, t), Scale(ray.UTheta, t));
    }
    double d5 = s[5] - 5 * s[4] + 10 * s[3] - 10 * s[2] + 5 * s[1] - s[0]; // annihilates deg <= 4
    double objRef = 0.0; foreach (double v in s) objRef += System.Math.Abs(v);

    // Hessian probe: h(t) = [S_B(bg + ds*v) - 2 S_B(bg) + S_B(bg - ds*v)] / ds^2,
    // with bg = t*(uOmega, uTheta) and v = (vOmega, vTheta) a fixed joint direction.
    // Degree-2 Hessian in t  <=>  third t-difference of h vanishes.
    double H(double t)
    {
        var bo = Scale(ray.UOmega, t);
        var bt = Scale(ray.UTheta, t);
        double c = Jobj(eval, bo, bt);
        double p = Jobj(eval, Axpy(bo, ray.VOmega, ProbeDs), Axpy(bt, ray.VTheta, ProbeDs));
        double m = Jobj(eval, Axpy(bo, ray.VOmega, -ProbeDs), Axpy(bt, ray.VTheta, -ProbeDs));
        return (p - 2 * c + m) / (ProbeDs * ProbeDs);
    }
    double h0 = H(RayT0), h1 = H(RayT0 + RayDt), h2 = H(RayT0 + 2 * RayDt), h3 = H(RayT0 + 3 * RayDt);
    double d3 = h3 - 3 * h2 + 3 * h1 - h0; // annihilates deg <= 2 in t
    double hessRef = System.Math.Abs(h0) + System.Math.Abs(h1) + System.Math.Abs(h2) + System.Math.Abs(h3);

    return (System.Math.Abs(d5) / (objRef + 1e-300),
            System.Math.Abs(d3) / (hessRef + 1e-300),
            System.Math.Abs(d5),
            System.Math.Abs(d3));
}

// A member's degree measurement: max over ray seeds, at a given theta amplitude.
// The exceedsTwo flag uses the caller's noise-relative threshold (0 => flag unused,
// as for the control measurement whose degree-2 verdict is taken from the machine
// floor directly).
DegreeMeasurement MeasureDegree(Func<double[], double[], double[]> eval, double thetaAmplitude, int seedBase, double threshold)
{
    double maxD5Rel = 0, maxD3Rel = 0, maxD5Abs = 0, maxD3Abs = 0;
    for (int seed = 0; seed < RaySeedCount; seed++)
    {
        var ray = RandomRay(RngSeed + seedBase * 1009 + seed, thetaAmplitude);
        var (d5r, d3r, d5a, d3a) = DegreeProbe(eval, ray);
        maxD5Rel = System.Math.Max(maxD5Rel, d5r);
        maxD3Rel = System.Math.Max(maxD3Rel, d3r);
        maxD5Abs = System.Math.Max(maxD5Abs, d5a);
        maxD3Abs = System.Math.Max(maxD3Abs, d3a);
    }
    bool exceedsTwo = maxD5Rel > threshold;
    return new DegreeMeasurement(maxD5Rel, maxD3Rel, maxD5Abs, maxD3Abs, exceedsTwo);
}

// ---------------------------------------------------------------------------
// ARM A (CONTROL): identity-equivalent {id0, none, trivial}. MUST reproduce the
// Phase436/441 no-go on the 4D mesh: degree-4-exact objective (5th t-difference at
// machine level), degree-2 Hessian. It must also FAIL the richness certificate.
// ---------------------------------------------------------------------------

var controlEval = ControlEval();
var controlDegree = MeasureDegree(controlEval, ThetaAmplitude, seedBase: 0, threshold: double.PositiveInfinity);
bool controlRichnessFails = !EinsteinianShiabBatteries.IsRich(controlOp, RichnessFloor);
bool controlArmReproducesPhase436DegreeTwo =
    controlDegree.MaxD5ObjRel < MachineDegreeFloor &&
    controlDegree.MaxD3HessRel < MachineDegreeFloor;

// Noise-relative degree threshold, calibrated on THIS mesh's machine-exact control.
double degreeThreshold = System.Math.Max(AbsoluteDegreeBackstop, NoiseMultipleForDegree * controlDegree.MaxD5ObjRel);

// ---------------------------------------------------------------------------
// ARM B (ISOLATION): with the identity-equivalent Shiab, theta is absent from
// Upsilon, so the theta-block of the joint Hessian is EXACTLY (bit-exact) zero.
// This is the check the slaved-Wilson form structurally cannot run. The CONTRAST
// (a genuine sd2 member couples theta => nonzero block) proves any lift in arm C
// is Shiab-caused, not a free-DOF artifact.
// ---------------------------------------------------------------------------

var omegaForIsolation = RandomVector(nOmega, RngSeed + 501, 0.2);
var thetaZero = new double[nTheta];
double isolationThetaBlock = EinsteinianShiabBatteries.ThetaBlockFrobenius(
    controlEval, mesh, algebra, omegaForIsolation, thetaZero, manifest, geometry, IsolationSubspaceDim);
bool isolationThetaBlockExactlyDegenerate = isolationThetaBlock == 0.0;

var sd2IsolationOp = new EinsteinianShiabOperator(mesh, algebra, new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
});
double einsteinianThetaBlock = EinsteinianShiabBatteries.ThetaBlockFrobenius(
    PinnedEval(sd2IsolationOp), mesh, algebra, omegaForIsolation, thetaZero, manifest, geometry, IsolationSubspaceDim);
bool einsteinianThetaBlockNonzero = einsteinianThetaBlock > 1e-6;
bool isolationBatteryPassed = isolationThetaBlockExactlyDegenerate && einsteinianThetaBlockNonzero;

// ---------------------------------------------------------------------------
// Cross-check: study-side per-face Ad (lowest-index rule) + operator theta=0
// Evaluate must reproduce the operator's own EvaluateWithTheta bit-for-bit (to FD-
// free machine precision). Validates the AverageEval machinery against the shipped
// instrument before the robustness verdict rests on it.
// ---------------------------------------------------------------------------

double crossCheckMaxDiff;
{
    var omegaX = RandomVector(nOmega, RngSeed + 601, 0.2);
    var thetaX = RandomVector(nTheta, RngSeed + 602, 0.15);
    var connX = new ConnectionField(mesh, algebra, omegaX);
    var fX = CurvatureAssembler.Assemble(connX).ToFieldTensor();
    var viaOperator = sd2IsolationOp.EvaluateWithTheta(fX, connX.ToFieldTensor(), thetaX, manifest, geometry).Coefficients;
    var fRotLowest = PreRotateFacewise(fX.Coefficients, thetaX, average: false);
    var viaStudy = sd2IsolationOp.Evaluate(FaceTensor(mesh, algebra, fRotLowest), connX.ToFieldTensor(), manifest, geometry).Coefficients;
    crossCheckMaxDiff = 0.0;
    for (int i = 0; i < viaOperator.Length; i++)
        crossCheckMaxDiff = System.Math.Max(crossCheckMaxDiff, System.Math.Abs(viaOperator[i] - viaStudy[i]));
}
bool crossCheckPinnedMatchesOperator = crossCheckMaxDiff < CrossCheckTolerance;

// ---------------------------------------------------------------------------
// ARM C (TREATMENT) + ARM E (ROBUSTNESS): each treatment member, degree verdict
// under BOTH vertex->face rules (pinned lowest-index / incident-average).
// ---------------------------------------------------------------------------

bool carrierMatchAllPassed = true;
var memberRecords = new List<MemberRecord>();
for (int mi = 0; mi < treatmentMembers.Count; mi++)
{
    var (name, member) = treatmentMembers[mi];
    var op = new EinsteinianShiabOperator(mesh, algebra, member);

    bool carrierOk;
    try { BranchOperatorRegistry.ValidateCarrierMatch(trivialTorsion, op); carrierOk = true; }
    catch { carrierOk = false; }
    carrierMatchAllPassed &= carrierOk;

    double richness = EinsteinianShiabBatteries.RichnessDeviation(op);
    bool isRich = richness > RichnessFloor;

    var pinned = MeasureDegree(PinnedEval(op), ThetaAmplitude, seedBase: 10 + mi, degreeThreshold);
    var average = MeasureDegree(AverageEval(op), ThetaAmplitude, seedBase: 100 + mi, degreeThreshold);

    memberRecords.Add(new MemberRecord(
        mi, name, member.Phi1.InvariantElement, member.Phi2.InvariantElement,
        member.EinsteinCoefficient, richness, isRich, carrierOk, pinned, average));
}

// Overall treatment verdict: over the genuinely-rich Einsteinian members (a scalar
// or zero R is not a treatment), does the joint Hessian exceed degree 2? Honest
// either way; does NOT gate the phase.
var richMembers = memberRecords.Where(m => m.IsRich).ToList();
bool einsteinianJointHessianDegreeExceedsTwoAll =
    richMembers.Count > 0 && richMembers.All(m => m.Pinned.ExceedsTwo);
bool einsteinianJointHessianDegreeExceedsTwoAny = richMembers.Any(m => m.Pinned.ExceedsTwo);

// Robustness: does the degree verdict AGREE under both vertex->face rules for every
// member? A material disagreement is a FINDING to flag (physicist mandate), not a
// gate failure.
bool vertexFaceRuleRobust = memberRecords.All(m => m.Pinned.ExceedsTwo == m.Average.ExceedsTwo);
var vertexFaceRuleDisagreements = memberRecords
    .Where(m => m.Pinned.ExceedsTwo != m.Average.ExceedsTwo)
    .Select(m => m.Name).ToArray();

// The incident-average rule smooths theta over a face's three incident vertices, so
// it ATTENUATES the lift magnitude relative to the pinned (single-vertex) rule. This
// is recorded transparently (a rule-dependent MAGNITUDE, not a verdict flip): the
// factor is reported so the difference between rules is flagged, not papered over.
double meanRichAttenuation = 0.0;
if (richMembers.Count > 0)
    meanRichAttenuation = richMembers
        .Select(m => m.Average.MaxD5ObjRel > 0 ? m.Pinned.MaxD5ObjRel / m.Average.MaxD5ObjRel : 0.0)
        .Average();

// ---------------------------------------------------------------------------
// ARM D (HONESTY SWEEP, design battery #8): the third t-difference of the Hessian
// must -> 0 as |u_theta| -> 0 and grow with theta amplitude (artifact separation).
// One rich Einsteinian member (sd2-id0, c=0.5), pinned rule.
// ---------------------------------------------------------------------------

var honestyMember = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
};
var honestyEval = PinnedEval(new EinsteinianShiabOperator(mesh, algebra, honestyMember));
var honestyAmplitudes = new[] { 0.0, 0.25, 0.5, 1.0 };
var honestyD3Hess = new double[honestyAmplitudes.Length];
var honestyD5Obj = new double[honestyAmplitudes.Length];
for (int i = 0; i < honestyAmplitudes.Length; i++)
{
    // Fixed omega/theta directions; only the theta amplitude varies (single seed for
    // a clean amplitude axis).
    var ray = RandomRay(RngSeed + 900, honestyAmplitudes[i]);
    var (d5r, d3r, _, _) = DegreeProbe(honestyEval, ray);
    honestyD5Obj[i] = d5r;
    honestyD3Hess[i] = d3r;
}
bool honestyZeroAmplitudeVanishes = honestyD3Hess[0] < MachineDegreeFloor;
bool honestyMonotoneInAmplitude = true;
for (int i = 1; i < honestyD3Hess.Length; i++)
    honestyMonotoneInAmplitude &= honestyD3Hess[i] >= honestyD3Hess[i - 1] - 1e-12;
bool honestyGrows = honestyD3Hess[^1] > honestyD3Hess[0];
bool honestySweepPassed = honestyZeroAmplitudeVanishes && honestyMonotoneInAmplitude && honestyGrows;

// ---------------------------------------------------------------------------
// ARM F (SMOKE ONLY, NON-GATING): one slaved-wilson-smoketest row, clearly labelled.
// ---------------------------------------------------------------------------

var slavedMember = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "slaved-wilson-smoketest",
};
var slavedOp = new EinsteinianShiabOperator(mesh, algebra, slavedMember, null, omegaCouplingKappa: 1.0);
double slavedSmokeThirdTDiff = System.Math.Abs(EinsteinianShiabBatteries.HessianThirdTDifference(
    slavedOp, mesh, algebra, RandomVector(nOmega, RngSeed + 55), RandomVector(nOmega, RngSeed + 77),
    manifest, geometry));

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Batteries + fail-closed boundary.
// ---------------------------------------------------------------------------

bool batteriesAllPassed =
    precursorsPassed &&
    controlArmReproducesPhase436DegreeTwo &&
    controlRichnessFails &&
    isolationThetaBlockExactlyDegenerate &&
    isolationBatteryPassed &&
    crossCheckPinnedMatchesOperator &&
    honestySweepPassed &&
    carrierMatchAllPassed;

// Recorded-boundary keys (design §3.9 fail-closed wall; physicist memo §3/§6b/§6c/§6e).
const string epsilonRealization = "independent-theta-dof";
const bool hessianOverJointOmegaTheta = true;
const bool slavedWilsonKeptAsSmokeTestOnly = true;
const int shiabOutputDegree = 2;
const bool draftOperatorIsDegreeRaising = true;
const bool reducedRealizationCapturesRicciWeylAlgebraNotFormDegree = true;
const bool draftDegreeReductionRecorded = true;
const string draftAlignmentStatus = "surrogate";
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const string baseSignature = "Cl(4,0)-euclidean-slice";
const bool lorentzianBaseNotYetRealized = true;
const bool geometricFiberTrivial = true;
const bool spinorUsedAsShiabCarrierNotFiber = true;
const bool draft14dObserverseNotRealized = true;
const bool noKaluzaKleinTowerModeled = true;
const bool naiveDiracDoublersPresent = true;

const bool degreeLiftIsNecessaryNotSufficientForScale = true;
const bool noScaleProduced = true;
const bool noGevPromotion = true;

// Standard fail-closed block (identical policy to Phases 436/440/441).
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
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; CreateUniform4D({MeshRefinement}); V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta}",
    "joint (omega,theta) DOF; S_B=(1/2)||S_h(omega,theta)-T||^2; T=trivial; Ad_eps=exp(ad_theta) VERTEX-based; degree via 5th-obj/3rd-Hess t-difference; no target values")))).ToLowerInvariant();

bool jointOmegaThetaHessianDegreeProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    degreeLiftIsNecessaryNotSufficientForScale &&
    noScaleProduced &&
    noGevPromotion &&
    hessianOverJointOmegaTheta &&
    slavedWilsonKeptAsSmokeTestOnly &&
    epsilonRealization == "independent-theta-dof" &&
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

string treatmentVerdictPhrase = einsteinianJointHessianDegreeExceedsTwoAll
    ? "every genuinely-rich Einsteinian member lifts the joint (omega,theta) Hessian above degree 2"
    : einsteinianJointHessianDegreeExceedsTwoAny
        ? "some (not all) rich Einsteinian members lift the joint Hessian above degree 2 (recorded per member)"
        : "the rich Einsteinian members leave the joint Hessian at degree 2 -- the no-go theorem extends to the faithful 4D operator (a legitimate frontier-sharpening outcome)";
string robustnessPhrase = vertexFaceRuleRobust
    ? $"the degree verdict AGREES under both vertex->face rules (lowest-index and incident-average); the incident-average rule attenuates the lift MAGNITUDE by ~{meanRichAttenuation:F1}x (it smooths theta over the face's three vertices) but the verdict is robust"
    : $"the degree verdict DIFFERS between vertex->face rules for [{string.Join(", ", vertexFaceRuleDisagreements)}] -- FLAGGED as a finding (physicist mandate), not papered over (incident-average attenuation ~{meanRichAttenuation:F1}x)";

string terminalStatus = jointOmegaThetaHessianDegreeProbePassed
    ? (einsteinianJointHessianDegreeExceedsTwoAny
        ? "joint-omega-theta-hessian-degree-probe-passed-einsteinian-lift-above-degree-two-necessary-not-sufficient-no-scale"
        : "joint-omega-theta-hessian-degree-probe-passed-einsteinian-remains-degree-two-no-go-extends-to-4d-operator")
    : "joint-omega-theta-hessian-degree-probe-blocked";

string namedNextStudy =
    "joint effective-potential + variational eps*(omega) (solve dS/deps=0, integrate epsilon out) + " +
    "Coleman-Weinberg / gap-equation saturation analysis (Phase435/438 machinery on the lifted joint Hessian). " +
    "Degree > 2 is the NECESSARY condition for log-saturation, NOT sufficient, and NOT a scale.";

string decision = jointOmegaThetaHessianDegreeProbePassed
    ? $"The joint (omega, theta) Hessian-degree probe on the draft-canonical Einsteinian Shiab is decided on internal consistency. " +
      $"On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}), with theta a genuine INDEPENDENT H-valued vertex DOF and the Hessian over the JOINT (omega, theta): " +
      $"the CONTROL arm (identity-equivalent {{id0, none, trivial}}) reproduces the Phase436/441 no-go exactly on the 4D mesh -- the objective is degree-4-exact (max relative 5th t-difference {controlDegree.MaxD5ObjRel:E2} < {MachineDegreeFloor:E0}) and the Hessian degree-2 (3rd t-difference {controlDegree.MaxD3HessRel:E2}) -- and it FAILS the richness certificate (the expected control outcome). " +
      $"The ISOLATION battery is bit-exact: with the identity-equivalent Shiab theta is absent from Upsilon so the theta-block of the joint Hessian is EXACTLY zero (||block|| = {isolationThetaBlock:E1}), while a genuine sd2 member couples theta (||block|| = {einsteinianThetaBlock:E3} > 0) -- so {treatmentVerdictPhrase}, and that lift (where present) is Shiab-caused, NOT a free-DOF artifact of inserting theta. " +
      $"The HONESTY sweep confirms the third t-difference vanishes at theta amplitude 0 ({honestyD3Hess[0]:E2}) and grows with amplitude (to {honestyD3Hess[^1]:E2}), separating a genuine lift from a numerical artifact. {char.ToUpper(robustnessPhrase[0]) + robustnessPhrase[1..]}. " +
      $"The study-side per-face Ad machinery reproduces the shipped EvaluateWithTheta to {crossCheckMaxDiff:E1}. " +
      $"MANDATORY FRAMING: a degree > 2 verdict is the NECESSARY condition for log-saturation, NOT sufficient, and is NOT a scale -- no scale, pole, VEV, or GeV is produced. The named NEXT study is: {namedNextStudy} " +
      $"Everything is target-blind, reduced-spin4-slice structure; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the degree verdicts until the precursors, control-arm reproduction, isolation battery, honesty sweep, cross-check, and carrier-match batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase442-joint-omega-theta-hessian-degree-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    jointOmegaThetaHessianDegreeProbePassed,

    // Precursors.
    phase436PrecursorPassed,
    phase441PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    // Probe configuration.
    probeConfiguration = new
    {
        meshRefinement = MeshRefinement,
        lieAlgebraId = "su2-trace-pairing",
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        faceCount = mesh.FaceCount,
        cellCount = mesh.CellCount,
        nOmega,
        nTheta,
        jointDof = nOmega + nTheta,
        rayT0 = RayT0,
        rayDt = RayDt,
        probeDs = ProbeDs,
        raySeedCount = RaySeedCount,
        thetaAmplitude = ThetaAmplitude,
        degreeThreshold,
        degreeThresholdBasis = "noise-relative: max(absoluteBackstop, noiseMultiple * control machine floor)",
        noiseMultipleForDegree = NoiseMultipleForDegree,
        absoluteDegreeBackstop = AbsoluteDegreeBackstop,
        controlMachineFloorD5ObjRel = controlDegree.MaxD5ObjRel,
        machineDegreeFloor = MachineDegreeFloor,
        degreeMeasurement = "objective 5th t-difference (deg-4 exact => 0) and Hessian 3rd t-difference (deg-2 => 0) along joint rays t*(u_omega, u_theta)",
        residualIdentity = "Upsilon = S_h(omega, theta) - T; T = TrivialTorsion = 0; S_h = M(Ad_theta(F)); Ad_theta = exp(ad_theta) per face at the representative vertex",
    },

    // Headline record keys.
    epsilonRealization,
    hessianOverJointOmegaTheta,
    slavedWilsonKeptAsSmokeTestOnly,
    controlArmReproducesPhase436DegreeTwo,
    isolationThetaBlockExactlyDegenerate,
    isolationBatteryPassed,
    einsteinianJointHessianDegreeExceedsTwo = einsteinianJointHessianDegreeExceedsTwoAll,
    einsteinianJointHessianDegreeExceedsTwoAny,
    honestySweepPassed,
    vertexFaceRuleRobust,
    vertexFaceRuleDisagreements,
    degreeLiftIsNecessaryNotSufficientForScale,
    noScaleProduced,
    noGevPromotion,

    // Recorded boundary (the six VERBATIM + the §3.9 wall set).
    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        shiabOutputDegree,
        draftOperatorIsDegreeRaising,
        reducedRealizationCapturesRicciWeylAlgebraNotFormDegree,
        draftDegreeReductionRecorded,
        draftAlignmentStatus,
        baseSignature,
        lorentzianBaseNotYetRealized,
        geometricFiberTrivial,
        spinorUsedAsShiabCarrierNotFiber,
        draft14dObserverseNotRealized,
        noKaluzaKleinTowerModeled,
        naiveDiracDoublersPresent,
    },

    // Batteries.
    batteries = new
    {
        batteriesAllPassed,
        controlArmReproducesPhase436DegreeTwo,
        controlRichnessFails,
        controlMaxD5ObjRelative = controlDegree.MaxD5ObjRel,
        controlMaxD3HessRelative = controlDegree.MaxD3HessRel,
        isolationThetaBlockExactlyDegenerate,
        isolationThetaBlockFrobenius = isolationThetaBlock,
        einsteinianThetaBlockFrobenius = einsteinianThetaBlock,
        einsteinianThetaBlockNonzero,
        isolationBatteryPassed,
        crossCheckPinnedMatchesOperator,
        crossCheckMaxDiff,
        honestySweepPassed,
        carrierMatchAllPassed,
        richnessFloor = RichnessFloor,
        isolationSubspaceDim = IsolationSubspaceDim,
    },

    // ARM A/B/E/D per-member and per-sweep detail.
    treatmentTable = memberRecords.Select(m => new
    {
        index = m.Index,
        member = m.Name,
        phi1 = m.Phi1,
        phi2 = m.Phi2,
        einsteinCoefficient = m.EinsteinCoefficient,
        richnessDeviation = m.Richness,
        isRich = m.IsRich,
        carrierMatchPassed = m.CarrierOk,
        pinnedRule = new
        {
            maxD5ObjRelative = m.Pinned.MaxD5ObjRel,
            maxD3HessRelative = m.Pinned.MaxD3HessRel,
            maxD5ObjAbsolute = m.Pinned.MaxD5ObjAbs,
            maxD3HessAbsolute = m.Pinned.MaxD3HessAbs,
            degreeExceedsTwo = m.Pinned.ExceedsTwo,
        },
        incidentAverageRule = new
        {
            maxD5ObjRelative = m.Average.MaxD5ObjRel,
            maxD3HessRelative = m.Average.MaxD3HessRel,
            maxD5ObjAbsolute = m.Average.MaxD5ObjAbs,
            maxD3HessAbsolute = m.Average.MaxD3HessAbs,
            degreeExceedsTwo = m.Average.ExceedsTwo,
        },
        vertexFaceRuleVerdictsAgree = m.Pinned.ExceedsTwo == m.Average.ExceedsTwo,
    }).ToArray(),

    einsteinianJointHessianDegreeExceedsTwoAll,
    richMemberCount = richMembers.Count,
    incidentAverageMeanAttenuationOnRichMembers = meanRichAttenuation,

    honestySweep = new
    {
        amplitudes = honestyAmplitudes,
        thirdTDifferenceHessianRelative = honestyD3Hess,
        fifthTDifferenceObjectiveRelative = honestyD5Obj,
        zeroAmplitudeVanishes = honestyZeroAmplitudeVanishes,
        monotoneInAmplitude = honestyMonotoneInAmplitude,
        growsWithAmplitude = honestyGrows,
        honestySweepPassed,
        note = "sd2-id0/c0.5, independent-theta, pinned rule; the third t-difference of the Hessian must vanish at theta amplitude 0 and grow with amplitude (design battery #8).",
    },

    slavedWilsonSmokeTest = new
    {
        gating = false,
        epsilonMode = "slaved-wilson-smoketest",
        member = "sd2-id0/c0.5",
        kappa = 1.0,
        hessianThirdTDifferenceAbs = slavedSmokeThirdTDiff,
        note = "NON-GATING smoke-test only (design §3.5). eps(omega)=exp(kappa*sum omega_e) routes the exp(ad_theta) non-polynomiality through an inserted omega-functional; it cannot run the isolation battery, so it is NOT the study instrument. slavedWilsonKeptAsSmokeTestOnly=true.",
    },

    // Named next study + framing.
    framing = new
    {
        degreeLiftIsNecessaryNotSufficientForScale,
        noScaleProduced,
        namedNextStudy,
        treatmentVerdict = treatmentVerdictPhrase,
        robustness = robustnessPhrase,
    },

    runtimeSeconds,

    // Standard fail-closed block.
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
        phase436SummaryPath = Phase436SummaryPath,
        phase441SummaryPath = Phase441SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "The joint (omega,theta) Hessian degrees, block norms, and t-differences are candidate-only structure data of the reduced spin-4 slice, not physical mass spectra.",
        "A degree > 2 verdict is the NECESSARY condition for one-loop log-saturation, NOT sufficient, and NOT a scale; no scale, pole, VEV, or GeV is produced.",
        "The isolation battery certifies any lift is caused by the non-scalar Einsteinian contraction, not by inserting the theta DOF (identity Shiab => bit-exact-zero theta-block).",
        "Extracting an actual scale requires the deferred variational eps*(omega) + effective-potential saturation analysis (the named next study); nothing here is promoted.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "joint_omega_theta_hessian_degree_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "joint_omega_theta_hessian_degree_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"jointOmegaThetaHessianDegreeProbePassed={jointOmegaThetaHessianDegreeProbePassed}");
Console.WriteLine($"precursors: p436={phase436PrecursorPassed} p441={phase441PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta}");
Console.WriteLine($"CONTROL: reproducesPhase436Deg2={controlArmReproducesPhase436DegreeTwo} (D5rel={controlDegree.MaxD5ObjRel:E2} D3rel={controlDegree.MaxD3HessRel:E2}) richnessFails={controlRichnessFails}");
Console.WriteLine($"ISOLATION: thetaBlockExactlyDegenerate={isolationThetaBlockExactlyDegenerate} (||block||={isolationThetaBlock:E1}) einsteinBlock={einsteinianThetaBlock:E3} passed={isolationBatteryPassed}");
Console.WriteLine($"CROSS-CHECK: pinnedMatchesOperator={crossCheckPinnedMatchesOperator} maxDiff={crossCheckMaxDiff:E1}");
Console.WriteLine($"HONESTY: passed={honestySweepPassed} d3Hess[amp]={string.Join(", ", honestyD3Hess.Select(x => x.ToString("E2")))}");
Console.WriteLine($"vertexFaceRuleRobust={vertexFaceRuleRobust} (incident-avg attenuation ~{meanRichAttenuation:F1}x) einsteinianDegreeExceedsTwo(all/any)={einsteinianJointHessianDegreeExceedsTwoAll}/{einsteinianJointHessianDegreeExceedsTwoAny}");
Console.WriteLine($"degreeThreshold={degreeThreshold:E2} (noise-relative; control floor={controlDegree.MaxD5ObjRel:E2})");
Console.WriteLine($"batteriesAllPassed={batteriesAllPassed} runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine("--- treatment table (pinned rule) ---");
foreach (var m in memberRecords)
    Console.WriteLine($"  {m.Name,-16} rich={m.IsRich,-5} D5rel={m.Pinned.MaxD5ObjRel:E2} D3rel={m.Pinned.MaxD3HessRel:E2} deg>2={m.Pinned.ExceedsTwo} | avg deg>2={m.Average.ExceedsTwo}");
Console.WriteLine($"slaved-wilson SMOKE (non-gating): |D3|={slavedSmokeThirdTDiff:E2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Local helpers.
// ---------------------------------------------------------------------------

BranchManifest BuildManifest() => new()
{
    BranchId = "phase442-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase442",
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

// Per-face pre-rotation of the curvature by Ad = exp(ad_thetaFace), applied on the
// ad-index. thetaFace is the theta at the face's lowest-index vertex (average=false)
// or the mean over the face's incident vertices (average=true).
double[] PreRotateFacewise(double[] fCoeffs, double[] theta, bool average)
{
    var result = new double[fCoeffs.Length];
    var cache = new Dictionary<string, double[,]>();
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        var verts = mesh.Faces[f];
        double[] thetaFace;
        string key;
        if (average)
        {
            thetaFace = new double[dimG];
            foreach (int v in verts)
                for (int a = 0; a < dimG; a++)
                    thetaFace[a] += theta[v * dimG + a];
            for (int a = 0; a < dimG; a++) thetaFace[a] /= verts.Length;
            key = "avg:" + string.Join(",", verts);
        }
        else
        {
            int v0 = verts[0];
            thetaFace = new double[dimG];
            Array.Copy(theta, v0 * dimG, thetaFace, 0, dimG);
            key = "low:" + v0;
        }

        if (!cache.TryGetValue(key, out var ad))
        {
            ad = AdExpMatrix(thetaFace);
            cache[key] = ad;
        }
        for (int a = 0; a < dimG; a++)
        {
            double s = 0;
            for (int b = 0; b < dimG; b++) s += ad[a, b] * fCoeffs[f * dimG + b];
            result[f * dimG + a] = s;
        }
    }
    return result;
}

// Ad = exp(ad_x); (ad_x)^c_b = sum_a f^c_{ab} x^a. Matches the operator's AdMatrix/AdExp.
double[,] AdExpMatrix(double[] x)
{
    var m = new double[dimG, dimG];
    for (int c = 0; c < dimG; c++)
        for (int b = 0; b < dimG; b++)
        {
            double s = 0;
            for (int a = 0; a < dimG; a++) s += algebra.GetStructureConstant(a, b, c) * x[a];
            m[c, b] = s;
        }
    return Lambda2Algebra.MatrixExp(m);
}

// The theta amplitude scales the ENTIRE theta sector -- both the background
// direction u_theta AND the second-derivative probe direction v_theta -- so that
// amplitude 0 switches the epsilon sector fully off (pure-omega ray + pure-omega
// probe) and both the objective 5th and Hessian 3rd t-differences collapse to the
// Phase436 degree-2/degree-4 result. This is what makes the honesty sweep a clean
// artifact separation: the lift vanishes exactly when theta is off and grows with it.
Ray RandomRay(int seed, double thetaAmplitude)
{
    var rng = new Random(seed);
    var uOmega = UnitRandom(rng, nOmega);
    var uThetaUnit = UnitRandom(rng, nTheta);
    var vOmega = UnitRandom(rng, nOmega);
    var vThetaUnit = UnitRandom(rng, nTheta);
    var uTheta = new double[nTheta];
    var vTheta = new double[nTheta];
    for (int i = 0; i < nTheta; i++) uTheta[i] = thetaAmplitude * uThetaUnit[i];
    for (int i = 0; i < nTheta; i++) vTheta[i] = thetaAmplitude * vThetaUnit[i];
    return new Ray(uOmega, uTheta, vOmega, vTheta);
}

static double[] UnitRandom(Random rng, int n)
{
    var v = new double[n];
    double norm = 0;
    for (int i = 0; i < n; i++) { v[i] = rng.NextDouble() - 0.5; norm += v[i] * v[i]; }
    norm = System.Math.Sqrt(norm);
    for (int i = 0; i < n; i++) v[i] /= norm;
    return v;
}

static double[] RandomVector(int n, int seed, double scale = 1.0)
{
    var rng = new Random(seed);
    var v = new double[n];
    for (int i = 0; i < n; i++) v[i] = scale * (rng.NextDouble() - 0.5);
    return v;
}

static double[] Scale(double[] a, double s)
{
    var r = new double[a.Length];
    for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
    return r;
}

static double[] Axpy(double[] baseV, double[] dir, double s)
{
    var r = new double[baseV.Length];
    for (int i = 0; i < r.Length; i++) r[i] = baseV[i] + s * dir[i];
    return r;
}

static FieldTensor FaceTensor(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs) => new()
{
    Label = "F_rot",
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

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed record Ray(double[] UOmega, double[] UTheta, double[] VOmega, double[] VTheta);

public sealed record DegreeMeasurement(
    double MaxD5ObjRel, double MaxD3HessRel, double MaxD5ObjAbs, double MaxD3HessAbs, bool ExceedsTwo);

public sealed record MemberRecord(
    int Index, string Name, string Phi1, string Phi2, double EinsteinCoefficient,
    double Richness, bool IsRich, bool CarrierOk, DegreeMeasurement Pinned, DegreeMeasurement Average);
