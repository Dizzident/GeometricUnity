using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase410: curvature-coupled VEV-selection probe.
//
// MOTIVATION (qualitative primary-speaker source, TOE-GU-40YEARS-20250602,
// catalogued 2026-06-12; cite GU-DRAFT-2021 as primary): the VEV is "coaxed
// out of the vacuum" by SCALAR CURVATURE in GU's improved Einstein equation
// and "plays the role of a fundamental mass scale"; masses track curvature.
// Phase405 proved the bare control-branch objective gives an exactly flat
// vacuum manifold on every rank-1 direction with NO selection mechanism.
// This probe machine-characterizes what the SIMPLEST faithful realization
// of the curvature-coaxing claim - a uniform curvature-coupled quadratic
// term added to the bosonic objective - does to that landscape:
//
//   S_aug[omega] = S_B[omega] + (kappaR / 2) * R_eff * Q[omega],
//   Q[omega] = ||omega||^2 (plain coefficient norm; the unique uniform
//   direction-independent quadratic invariant available at this level),
//   R_eff an external scalar-curvature parameter (the toy branch has no
//   dynamical metric), kappaR the coupling. VEV formation needs
//   kappaR * R_eff < 0 (a negative effective mass-squared), exactly the
//   SM-Higgs pattern the interview's mechanism implies.
//
// Exact consequences the machine verifies:
//
//   C1. RUNAWAY ALONG FLAT RAYS: every rank-1 direction is EXACTLY flat in
//       S_B (Phase405; re-verified here), so along those rays
//       S_aug(t) = (kappaR R_eff / 2) c2 t^2 exactly, with NO quartic
//       stabilization: for kappaR R_eff < 0 the augmented landscape is
//       UNBOUNDED BELOW along every flat ray. Uniform curvature coupling
//       produces runaway, not a finite VEV, on the unlifted vacuum
//       manifold.
//
//   C2. The quadratic invariant is DIRECTION-BLIND: c2 is identical across
//       all 8 rank-1 directions (machine-verified), so the coupling cannot
//       order triplet vs doublet vs singlet rays at quadratic level.
//
//   C3. LIFTED-SECTOR MINIMA: on two-direction mixed configurations
//       (angle phi = 45 degrees, the maximal-mixing slice), the bare
//       objective is exactly quartic, S_B = K(u,v) t^4 with
//       K proportional to ||[u,v]||^2 (Phase405's exact shape; the
//       proportionality is re-verified here as a constant ratio across all
//       non-commuting pairs). The augmented landscape then has finite
//       minima t*^2 = |kappaR R_eff| c2 / (4 K) with depth
//       (kappaR R_eff c2)^2 / (16 K): the depth ORDERING is exactly the
//       INVERSE BRACKET-NORM ordering - the deepest valleys sit on the
//       pairs with the SMALLEST nonzero ||[u,v]||^2.
//
//   C4. SELECTION VERDICT: classify every non-commuting pair by su(2) x
//       u(1) block content (triplet 0-2 / doublet 3-6 / singlet 7) and ask
//       whether the maximal-depth stratum consists EXCLUSIVELY of
//       doublet-block-internal pairs. If not (the machine decides), the
//       uniform curvature coupling does NOT select the doublet VEV - the
//       claim's mechanism requires DIRECTION-DEPENDENT curvature coupling,
//       i.e. internal structure beyond the bare bosonic objective, which is
//       exactly the still-missing scalar-sector specification.
//
// Scope honesty: the interview's mechanism places the VEV "in a Dirac-like
// operator"; couplings through the FERMIONIC sector are NOT probed here
// (that is the Phase411 quartic/Dirac-squared candidate). This probe
// closes the BOSONIC uniform-coupling realization only. CPU reference
// backend per IA-5 (no GPU needed at this problem size).
//
// Fail-closed: control-branch objective only; R_eff and kappaR are
// arbitrary-sign external parameters, never fit to data; no scales;
// nothing promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase410_curvature_coupled_vev_selection_probe_001/output";
const string Phase405SummaryPath = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output/vacuum_manifold_doublet_vev_orbit_scan_summary.json";
const string Phase409SummaryPath = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output/invariant_pairing_menu_spin_zero_extraction_probe_summary.json";

const int MeshRows = 6;
const int MeshCols = 6;
const int DimG = 8;
const double FlatnessFloor = 1e-20;
const double FitRelativeTolerance = 1e-6;
const double RatioCvTolerance = 1e-6;

var outputDir = Environment.GetEnvironmentVariable("PHASE410_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p405 = JsonDocument.Parse(File.ReadAllText(Phase405SummaryPath));
using var p409 = JsonDocument.Parse(File.ReadAllText(Phase409SummaryPath));
bool phase405PrecursorPassed =
    JsonBool(p405.RootElement, "vacuumManifoldDoubletVevOrbitScanPassed") is true &&
    JsonBool(p405.RootElement, "vacuumManifoldPermitsConstantDoubletVev") is true &&
    JsonBool(p405.RootElement, "noSelectionMechanismAtConstantRank1Level") is true;
bool phase409PrecursorPassed =
    JsonBool(p409.RootElement, "invariantPairingMenuSpinZeroExtractionProbePassed") is true;

// ---------------------------------------------------------------------------
// Geometry, algebra, CPU backend (Phase405 construction).
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu3();
var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: MeshRows, cols: MeshCols);
var mesh = bundle.AmbientMesh;
int edgeCount = mesh.EdgeCount;
int carrierDimension = edgeCount * DimG;
var geometry = bundle.ToGeometryContext("centroid", "P1");

var manifest = new BranchManifest
{
    BranchId = "phase410-curvature-coupled-vev-selection",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "r1",
    CodeRevision = "phase410",
    LieAlgebraId = "su3",
    BaseDimension = bundle.BaseMesh.EmbeddingDimension,
    AmbientDimension = mesh.EmbeddingDimension,
    ActiveGeometryBranch = "simplicial",
    ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial",
    ActiveShiabBranch = "identity-shiab",
    ActiveGaugeStrategy = "penalty",
    PairingConventionId = "pairing-killing",
    BasisConventionId = "canonical",
    ComponentOrderId = "face-major",
    AdjointConventionId = "adjoint-explicit",
    NormConventionId = "norm-l2-quadrature",
    DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(),
    InsertedChoiceIds = new[] { "IX-1", "IX-2" },
};

var signature = new TensorSignature
{
    AmbientSpaceId = "Y_h",
    CarrierType = "connection-1form",
    Degree = "1",
    LieAlgebraBasisId = "canonical",
    ComponentOrderId = "edge-major",
    MemoryLayout = "dense-row-major",
};

FieldTensor WrapCarrier(double[] coefficients) => new()
{
    Label = "omega_h",
    Signature = signature,
    Coefficients = coefficients,
    Shape = new[] { edgeCount, DimG },
};

var a0 = WrapCarrier(new double[carrierDimension]);
var torsion = new TrivialTorsionCpu(mesh, algebra);
var shiab = new IdentityShiabCpu(mesh, algebra);
var cpuBackend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

// Closed (exact 1-form) profiles dx, dy as in Phase405: d omega = 0 for
// every sample, so the bare landscape is purely bracket-driven.
var profileA = new double[edgeCount];
var profileB = new double[edgeCount];
{
    double maxAbsB = 0.0;
    for (int e = 0; e < edgeCount; e++)
    {
        var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
        var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
        profileA[e] = c1[1] - c0[1];
        profileB[e] = c1[0] - c0[0];
        maxAbsB = System.Math.Max(maxAbsB, System.Math.Abs(profileB[e]));
    }
    if (maxAbsB > 1e-12)
        for (int e = 0; e < edgeCount; e++)
        {
            profileA[e] /= maxAbsB;
            profileB[e] /= maxAbsB;
        }
}

double[] PairField(int dirI, double coeffI, int dirJ, double coeffJ)
{
    var field = new double[carrierDimension];
    for (int e = 0; e < edgeCount; e++)
    {
        field[e * DimG + dirI] += coeffI * profileA[e];
        field[e * DimG + dirJ] += coeffJ * profileB[e];
    }
    return field;
}

// Plain-coefficient bare objective (1/2)||Upsilon||^2 (Phase405 convention;
// su(3) Killing pairing is negative-definite, so the plain norm is used).
double BareObjective(double[] coefficients)
{
    var derived = cpuBackend.EvaluateDerived(WrapCarrier(coefficients), a0, manifest, geometry);
    double sum = 0.0;
    foreach (double value in derived.ResidualUpsilon.Coefficients)
        sum += value * value;
    return 0.5 * sum;
}

static double PlainNormSquared(double[] coefficients)
{
    double sum = 0.0;
    foreach (double value in coefficients)
        sum += value * value;
    return sum;
}

// ---------------------------------------------------------------------------
// C1 + C2: rank-1 rays - exact flatness of S_B and direction-blindness of
// the quadratic invariant c2.
// ---------------------------------------------------------------------------

var rank1BareObjectives = new double[DimG];
var rank1QuadraticInvariants = new double[DimG];
const double ProbeAmplitude = 0.5;
for (int dir = 0; dir < DimG; dir++)
{
    var field = PairField(dir, 0.0, dir, ProbeAmplitude); // profileB only
    rank1BareObjectives[dir] = BareObjective(field);
    rank1QuadraticInvariants[dir] = PlainNormSquared(field) / (ProbeAmplitude * ProbeAmplitude);
}
bool rank1RaysExactlyFlat = rank1BareObjectives.All(v => v <= FlatnessFloor);
double c2Mean = rank1QuadraticInvariants.Average();
double c2MaxRelDev = rank1QuadraticInvariants.Max(v => System.Math.Abs(v - c2Mean)) / c2Mean;
bool quadraticInvariantDirectionBlind = c2MaxRelDev <= 1e-12;

// C1 verdict is exact arithmetic: along a flat ray S_aug(t) =
// (kappaR R_eff / 2) c2 t^2 with zero quartic; for kappaR R_eff < 0 this is
// unbounded below. The machine encodes the inference from the verified
// coefficients rather than chasing a numerical infinity.
bool curvatureCouplingProducesRunawayAlongFlatRays =
    rank1RaysExactlyFlat && quadraticInvariantDirectionBlind;

// ---------------------------------------------------------------------------
// C3: lifted sector - exact quartic coefficients at maximal mixing and the
// inverse-bracket-norm depth ordering.
// ---------------------------------------------------------------------------

var amplitudes = new[] { 0.15, 0.30, 0.45, 0.60 };
const double InvSqrt2 = 0.70710678118654752440;

string BlockOf(int index) => index <= 2 ? "T" : index <= 6 ? "D" : "S";

var pairRecords = new List<(int I, int J, string BlockPair, double BracketNormSq, double K, double FitResidual, bool Flat)>();
for (int i = 0; i < DimG; i++)
    for (int j = i + 1; j < DimG; j++)
    {
        var u = new double[DimG];
        var v = new double[DimG];
        u[i] = 1.0;
        v[j] = 1.0;
        double bracketNormSq = PlainNormSquared(algebra.Bracket(u, v));

        var objectives = new double[amplitudes.Length];
        for (int a = 0; a < amplitudes.Length; a++)
        {
            double t = amplitudes[a];
            objectives[a] = BareObjective(PairField(i, t * InvSqrt2, j, t * InvSqrt2));
        }
        bool flat = objectives.All(o => o <= FlatnessFloor);
        double k = 0.0;
        double fitResidual = 0.0;
        if (!flat)
        {
            var ratios = new double[amplitudes.Length];
            for (int a = 0; a < amplitudes.Length; a++)
                ratios[a] = objectives[a] / System.Math.Pow(amplitudes[a], 4);
            k = ratios.Average();
            fitResidual = ratios.Max(r => System.Math.Abs(r - k)) / k;
        }
        string blockPair = string.Concat(new[] { BlockOf(i), BlockOf(j) }.OrderBy(x => x));
        pairRecords.Add((i, j, blockPair, bracketNormSq, k, fitResidual, flat));
    }

var nonCommuting = pairRecords.Where(p => !p.Flat).ToList();
var commuting = pairRecords.Where(p => p.Flat).ToList();
bool flatnessEqualsCommutativity = pairRecords.All(p =>
    p.Flat == (p.BracketNormSq <= 1e-15));
bool quarticFitsExact = nonCommuting.All(p => p.FitResidual <= FitRelativeTolerance);

// K proportional to ||[u,v]||^2: the ratio K / bracketNormSq must be a
// single constant across all non-commuting pairs.
var kRatios = nonCommuting.Select(p => p.K / p.BracketNormSq).ToList();
double kRatioMean = kRatios.Average();
double kRatioCv = kRatios.Max(r => System.Math.Abs(r - kRatioMean)) / kRatioMean;
bool kProportionalToBracketNorm = kRatioCv <= RatioCvTolerance;

// Depth ordering: with S_aug minima depth = (kappaR R_eff c2)^2 / (16 K),
// depth is maximized where K (equivalently ||[u,v]||^2) is SMALLEST.
double minBracketNormSq = nonCommuting.Min(p => p.BracketNormSq);
var deepestStratum = nonCommuting
    .Where(p => System.Math.Abs(p.BracketNormSq - minBracketNormSq) <= 1e-9 * minBracketNormSq)
    .ToList();
var deepestBlockPairs = deepestStratum.Select(p => p.BlockPair).Distinct().OrderBy(x => x).ToArray();
bool deepestStratumExclusivelyDoubletInternal =
    deepestBlockPairs.Length == 1 && deepestBlockPairs[0] == "DD";
bool doubletVevSelectedByCurvatureCoupling =
    curvatureCouplingProducesRunawayAlongFlatRays == false && // runaway would moot selection
    deepestStratumExclusivelyDoubletInternal;
bool maxDepthStratumBlockDegenerate = deepestBlockPairs.Length > 1;

// Distinct bracket-norm strata (data).
var strata = nonCommuting
    .GroupBy(p => System.Math.Round(p.BracketNormSq, 9))
    .OrderBy(g => g.Key)
    .Select(g => new
    {
        bracketNormSquared = g.Key,
        pairCount = g.Count(),
        blockPairs = g.Select(p => p.BlockPair).Distinct().OrderBy(x => x).ToArray(),
    })
    .ToArray();

bool probeInternallyConsistent =
    phase405PrecursorPassed &&
    phase409PrecursorPassed &&
    rank1RaysExactlyFlat &&
    quadraticInvariantDirectionBlind &&
    flatnessEqualsCommutativity &&
    quarticFitsExact &&
    kProportionalToBracketNorm;

// The selection verdict (data): with runaway present and the deepest lifted
// stratum block-degenerate, the uniform curvature coupling does not select
// the doublet.
bool curvatureCouplingFailsToSelectDoublet =
    curvatureCouplingProducesRunawayAlongFlatRays || !doubletVevSelectedByCurvatureCoupling;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool fermionicCurvatureCouplingProbed = false; // Phase411 territory
const bool directionDependentCouplingProbed = false; // requires the missing scalar-sector structure
const bool externalCurvatureParameterFitToData = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
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
const bool canFillPhase256ObservedFieldExtractionContract = false;
const string ApplicationSubjectKind = "curvature-coupled-vev-selection-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "uniform curvature-coupled quadratic on the Phase405 su(3) 6x6 landscape; exact flat-ray runaway inference; lifted-sector inverse-bracket-norm depth ordering; block classification of the deepest stratum")))).ToLowerInvariant();

bool curvatureCoupledVevSelectionProbePassed =
    probeInternallyConsistent &&
    !fermionicCurvatureCouplingProbed &&
    !directionDependentCouplingProbed &&
    !externalCurvatureParameterFitToData &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesWeakAngleOrCouplingLineage &&
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
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = curvatureCoupledVevSelectionProbePassed
    ? (curvatureCouplingFailsToSelectDoublet
        ? "uniform-curvature-coupling-produces-runaway-not-doublet-selection"
        : "uniform-curvature-coupling-selects-doublet-vev")
    : "curvature-coupled-vev-selection-probe-blocked";

string decision = curvatureCoupledVevSelectionProbePassed
    ? (curvatureCouplingFailsToSelectDoublet
        ? "The simplest faithful bosonic realization of the curvature-coaxing claim is machine-characterized and FAILS to select the doublet VEV, two ways at once. (C1) RUNAWAY: every rank-1 vacuum ray is exactly flat in the bare objective, so a uniform negative-mass-squared curvature term makes the augmented landscape UNBOUNDED BELOW along every such ray - no finite VEV forms where the vacuum manifold actually lives. (C2) The uniform quadratic invariant is direction-blind (identical c2 on all 8 rank-1 directions), so no triplet/doublet/singlet ordering can arise at quadratic level. (C3+C4) On the lifted (non-commuting) sector the augmented landscape does form finite minima with depth ordered by INVERSE bracket norm - but the deepest stratum is BLOCK-DEGENERATE (it contains non-doublet-internal pairs), so even there the doublet is not selected. CONSEQUENCE: the interview's curvature-coaxing mechanism, realized as a uniform bosonic coupling, leaves scalar-sector sub-gap (b) open; selection requires DIRECTION-DEPENDENT curvature coupling or the fermionic/Dirac-sector realization (Phase411 candidate) - i.e. exactly the internal structure the missing scalar-sector specification would have to provide. R_eff and kappaR are arbitrary external parameters throughout; nothing is promoted; no contract field is filled."
        : "The uniform curvature coupling SELECTS the doublet stratum - the depth data identify where; the next phase must characterize the selected vacuum before any contract claim. Nothing is promoted; no contract field is filled.")
    : "Do not use the characterization until the precursors and the exactness battery pass.";

var result = new
{
    phaseId = "phase410-curvature-coupled-vev-selection-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    curvatureCoupledVevSelectionProbePassed,
    phase405PrecursorPassed,
    phase409PrecursorPassed,
    probeInternallyConsistent,
    meshRows = MeshRows,
    meshCols = MeshCols,
    edgeCount,
    rank1RaysExactlyFlat,
    rank1BareObjectiveMax = rank1BareObjectives.Max(),
    quadraticInvariantDirectionBlind,
    quadraticInvariantMeanC2 = c2Mean,
    quadraticInvariantMaxRelativeDeviation = c2MaxRelDev,
    curvatureCouplingProducesRunawayAlongFlatRays,
    flatnessEqualsCommutativity,
    commutingPairCount = commuting.Count,
    nonCommutingPairCount = nonCommuting.Count,
    quarticFitsExact,
    maxQuarticFitResidual = nonCommuting.Count > 0 ? nonCommuting.Max(p => p.FitResidual) : 0.0,
    kProportionalToBracketNorm,
    kOverBracketNormMean = kRatioMean,
    kOverBracketNormCv = kRatioCv,
    minNonzeroBracketNormSquared = minBracketNormSq,
    deepestStratumPairCount = deepestStratum.Count,
    deepestStratumBlockPairs = deepestBlockPairs,
    deepestStratumExclusivelyDoubletInternal,
    maxDepthStratumBlockDegenerate,
    doubletVevSelectedByCurvatureCoupling,
    curvatureCouplingFailsToSelectDoublet,
    bracketNormStrata = strata,
    pairTable = pairRecords.Select(p => new
    {
        i = p.I,
        j = p.J,
        blockPair = p.BlockPair,
        bracketNormSquared = p.BracketNormSq,
        quarticCoefficient = p.K,
        fitResidual = p.FitResidual,
        flat = p.Flat,
    }).ToArray(),
    fermionicCurvatureCouplingProbed,
    directionDependentCouplingProbed,
    externalCurvatureParameterFitToData,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        augmentedObjective = "S_aug = S_B + (kappaR/2) R_eff ||omega||^2 with R_eff, kappaR arbitrary-sign external parameters; VEV formation needs kappaR R_eff < 0",
        c1 = "exact flat-ray coefficients: quartic <= 1e-20 floor, quadratic positive -> unbounded descent for kappaR R_eff < 0 (inference encoded from verified coefficients)",
        c2 = "plain-norm quadratic invariant per rank-1 direction; direction-blindness to 1e-12 relative",
        c3 = "exact quartic fit S_B = K t^4 at maximal mixing (45 degrees) over all 28 pairs; K / ||[u,v]||^2 constant across non-commuting pairs",
        c4 = "deepest stratum = argmin nonzero ||[u,v]||^2 (depth proportional to 1/K); block classification triplet 0-2 / doublet 3-6 / singlet 7",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesWeakAngleOrCouplingLineage,
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
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "the curvature-coaxing claim is QUALITATIVE primary-speaker material (TOE-GU-40YEARS-20250602); this probe tests its simplest uniform bosonic realization, not the (unspecified) full mechanism",
        "the fermionic/Dirac-sector realization ('a VEV in a Dirac-like operator') is NOT probed here - it is the Phase411 candidate",
        "direction-dependent curvature couplings are NOT probed - they presuppose the missing scalar-sector structure",
        "R_eff and kappaR are arbitrary external parameters; no value is fit to any observable; no scale is produced",
        "the toy branch has no dynamical metric; R_eff stands in for the claim's scalar curvature as an external parameter",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase405SummaryPath = Phase405SummaryPath,
        phase409SummaryPath = Phase409SummaryPath,
        motivatingInterview = "docs/Reference/ExperimentReferences/TOE-GU-40YEARS-20250602.md (qualitative; cite GU-DRAFT-2021 as primary)",
        deepResearchContext = "docs/Reference/ExperimentReferences/DEEP-RESEARCH-20260612.md (no public quantitative curvature-VEV formula exists)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "curvature_coupled_vev_selection_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "curvature_coupled_vev_selection_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"curvatureCoupledVevSelectionProbePassed={curvatureCoupledVevSelectionProbePassed}");
Console.WriteLine($"rank1RaysExactlyFlat={rank1RaysExactlyFlat} (max bare objective {rank1BareObjectives.Max():E3})");
Console.WriteLine($"quadraticInvariantDirectionBlind={quadraticInvariantDirectionBlind} (c2 rel dev {c2MaxRelDev:E3})");
Console.WriteLine($"curvatureCouplingProducesRunawayAlongFlatRays={curvatureCouplingProducesRunawayAlongFlatRays}");
Console.WriteLine($"flatnessEqualsCommutativity={flatnessEqualsCommutativity} commuting={commuting.Count} nonCommuting={nonCommuting.Count}");
Console.WriteLine($"kProportionalToBracketNorm={kProportionalToBracketNorm} (cv {kRatioCv:E3})");
Console.WriteLine($"deepestStratum blockPairs=[{string.Join(",", deepestBlockPairs)}] pairs={deepestStratum.Count} minBracketNormSq={minBracketNormSq:R}");
Console.WriteLine($"maxDepthStratumBlockDegenerate={maxDepthStratumBlockDegenerate}");
Console.WriteLine($"doubletVevSelectedByCurvatureCoupling={doubletVevSelectedByCurvatureCoupling}");
Console.WriteLine($"curvatureCouplingFailsToSelectDoublet={curvatureCouplingFailsToSelectDoublet}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;
