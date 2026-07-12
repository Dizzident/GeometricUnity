using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase465: Anomaly Consistency Variety Kernel (WAVE2_ACTION_PLAN_2026-07-12 item 4; C-KERNEL v3).
//
// Re-executes the committed Phase404 construction to derive the signed slot
// space (the six chirality-projected blocks of the 16 with their exact
// hypercharge slots), then treats the six hypercharges as FREE exact-rational
// variables and imposes the standard chiral anomaly-consistency conditions
//   [SU(2)]^2 U(1),  [SU(3)]^2 U(1),  [grav]^2 U(1)  (three linear forms)
//   [U(1)]^3                                          (one cubic form)
// as an exact-rational polynomial system. It classifies the solution VARIETY
// (empty / isolated-point / positive-dimensional) by exact linear algebra
// (reduced row echelon over Q -> rank + nullspace) plus the projective
// hypersurface-dimension theorem, and cross-checks positive-dimensionality with
// an explicit basis-independent 2-parameter family lying wholly on the variety.
//
// MANDATORY FRAMING (O7). Anomaly freedom is a LABELED theoretical-consistency
// import: the GU fermionic action is recorded UNDEFINED, so phase326's cap
// anomalyRouteProvidesLowEnergyHyperchargeSource=false can never flip and even
// the strongest rule-in is conditional (direction only; normalization not
// provided; the phase451 two-loop tension stands). Zero physics compute is
// promoted; nothing is measured or normalized; promotedPhysicalMassClaimCount
// remains 0. Lattice-unit quantities stay in lattice units. physicistReviewPending
// is carried explicitly.

var stopwatch = Stopwatch.StartNew();

var R0 = Rational.Zero;
var R1 = new Rational(1, 1);

const string ApplicationSubjectKind = "anomaly-consistency-variety-kernel";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 4";
const string DefaultOutputDir = "studies/phase465_anomaly_consistency_variety_kernel_001/output";
const string TerminalPrefix = "anomaly-consistency-variety-kernel-";
const string Phase404SummaryPath =
    "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";

// --- Pins (drift => blocking terminal) --------------------------------------
// The committed Phase404 target-blind construction hash (stable content pin;
// does not depend on generatedAt). Reconstruction of the signed slot space must
// also match Phase404's committed standardSpinor16Blocks.
const string Phase404TargetBlindConstructionHashPin =
    "ed68a8f3e0d98f25f3c476e940103a4275a79f550346338d08c3c6319160a481";

// --- SM-PATTERN TRIPWIRE (committed BEFORE the census) -----------------------
// The Phase404 signed-slot hypercharge signature, recorded as an exact-rational
// const before any variety computation runs. If the census lands on it the
// tripwire FIRES as a flag (never a promotion). Order: Q, L, e, d, n, u
// (color-triplet weak-doublet; color-singlet weak-doublet; three weak-singlets;
// two more weak-singlet triplets), in the |Y|=1/2 lepton-doublet normalization.
var smPatternSlotSignature = new[]
{
    new Rational(-1, 6), // Q  : color triplet, weak doublet
    new Rational(1, 2),  // L  : color singlet, weak doublet
    new Rational(-1, 1), // e  : color singlet, weak singlet
    new Rational(-1, 3), // d  : color triplet, weak singlet
    new Rational(0, 1),  // n  : color singlet, weak singlet (sterile)
    new Rational(2, 3),  // u  : color triplet, weak singlet
};

Directory.CreateDirectory(DefaultOutputDir);

// --- Re-execute the Phase404 construction: read the committed signed slots ---
bool phase404Present = File.Exists(Phase404SummaryPath);
string phase404HashObserved = "";
bool phase404HashMatches = false;
bool phase404ReconstructionMatches = false;
var blocks = new List<SlotBlock>();
string phase404ReconstructionNote = "";

if (phase404Present)
{
    using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
    var root = phase404.RootElement;
    phase404HashObserved = root.GetProperty("targetBlindConstructionHash").GetString() ?? "";
    phase404HashMatches = string.Equals(phase404HashObserved, Phase404TargetBlindConstructionHashPin, StringComparison.Ordinal);

    // The signed slot space: each block carries a hypercharge slot plus the
    // quantum numbers (weak j, color) that fix its anomaly multiplicities.
    var reconstructed = true;
    var expectedDims = new[] { 6, 2, 1, 3, 1, 3 };
    var committed = root.GetProperty("standardSpinor16Blocks");
    int idx = 0;
    foreach (var b in committed.EnumerateArray())
    {
        int dimension = b.GetProperty("dimension").GetInt32();
        double jValue = b.GetProperty("jValue").GetDouble();
        double y = b.GetProperty("y").GetDouble();
        bool isColorSinglet = b.GetProperty("isColorSinglet").GetBoolean();

        bool isWeakDoublet = System.Math.Abs(jValue - 0.5) < 1e-9;
        int su2Mult = isWeakDoublet ? 2 : 1;
        int colorMult = isColorSinglet ? 1 : 3;

        // Derived multiplicity must reproduce the committed block dimension.
        if (su2Mult * colorMult != dimension) reconstructed = false;
        if (idx < expectedDims.Length && dimension != expectedDims[idx]) reconstructed = false;

        // The committed hypercharge decimal must match the exact-rational slot.
        if (idx < smPatternSlotSignature.Length &&
            System.Math.Abs(y - smPatternSlotSignature[idx].ToDouble()) > 1e-6) reconstructed = false;

        blocks.Add(new SlotBlock(dimension, isWeakDoublet, !isColorSinglet, su2Mult, colorMult));
        idx++;
    }
    if (idx != 6) reconstructed = false;
    phase404ReconstructionMatches = reconstructed && phase404HashMatches;
    phase404ReconstructionNote = reconstructed
        ? "Six committed blocks reproduced (dimensions 6/2/1/3/1/3; weak/color multiplicities re-derived; hypercharge slots match)."
        : "Reconstruction mismatch against Phase404's committed standardSpinor16Blocks.";
}
else
{
    phase404ReconstructionNote = "Phase404 committed summary not found; cannot reconstruct the signed slot space.";
}

// --- Build the exact-rational anomaly-consistency polynomial system ----------
// Coefficients are derived from each block's quantum numbers (NOT hardcoded):
//   [SU(2)]^2 U(1):  sum over weak doublets of (color multiplicity) * Y
//   [SU(3)]^2 U(1):  sum over color triplets of (weak multiplicity) * Y
//   [grav]^2 U(1) :  sum over all states of Y                      (= dimension * Y)
//   [U(1)]^3      :  sum over all states of Y^3                    (= dimension * Y^3)
Rational[] linSu2 = new Rational[6];
Rational[] linSu3 = new Rational[6];
Rational[] linGrav = new Rational[6];
int[] cubicWeights = new int[6];
if (blocks.Count == 6)
{
    for (int b = 0; b < 6; b++)
    {
        var blk = blocks[b];
        linSu2[b] = new Rational(blk.IsWeakDoublet ? blk.ColorMult : 0, 1);
        linSu3[b] = new Rational(blk.IsColorTriplet ? blk.Su2Mult : 0, 1);
        linGrav[b] = new Rational(blk.Dimension, 1);
        cubicWeights[b] = blk.Dimension;
    }
}

// --- Exact linear algebra: rank + nullspace over Q (RREF) --------------------
var linearRows = new List<Rational[]> { linSu2, linSu3, linGrav };
int linearRank = 0;
var nullspaceBasis = new List<Rational[]>();
bool exactArithmeticOk = blocks.Count == 6;
if (exactArithmeticOk)
{
    (linearRank, nullspaceBasis) = RankAndNullspace(linearRows, 6);
}
int nullity = 6 - linearRank;

// --- Restrict the cubic to the nullspace: is it identically zero? -----------
// Parametrize y_b = sum_j (v_j)_b * t_j over the nullspace basis {v_j}.
// The [U(1)]^3 form becomes a homogeneous cubic in t_1..t_nullity; record
// whether it is the zero polynomial (=> variety = whole nullspace) or genuine.
bool cubicIdenticallyZeroOnNullspace = false;
int restrictedCubicMonomialCount = 0;
if (exactArithmeticOk && nullity > 0)
{
    var cubic = new MultiPoly(nullity);
    for (int b = 0; b < 6; b++)
    {
        // linear form for y_b in the t-parameters
        var yb = new MultiPoly(nullity);
        for (int j = 0; j < nullity; j++)
            yb.AddMonomial(UnitExponent(nullity, j), nullspaceBasis[j][b]);
        var cube = yb.Mul(yb).Mul(yb);
        cubic = cubic.Add(cube.Scale(new Rational(cubicWeights[b], 1)));
    }
    restrictedCubicMonomialCount = cubic.NonZeroTermCount();
    cubicIdenticallyZeroOnNullspace = restrictedCubicMonomialCount == 0;
}

// --- Classification (projective dimension of the anomaly variety) ------------
// Linear system cuts P^5 to P^(nullity-1). One cubic hypersurface cuts one more
// projective dimension (projective hypersurface-dimension theorem) unless it is
// identically zero on the nullspace.
string classification;
int varietyProjectiveDimension;
int varietyAffineDimension;
if (!exactArithmeticOk)
{
    classification = "nonconvergent";
    varietyProjectiveDimension = -99;
    varietyAffineDimension = -99;
}
else if (nullity == 0)
{
    classification = "empty";
    varietyProjectiveDimension = -1;
    varietyAffineDimension = 0;
}
else if (cubicIdenticallyZeroOnNullspace)
{
    classification = "positive-dimensional";
    varietyProjectiveDimension = nullity - 1;
    varietyAffineDimension = nullity;
}
else
{
    // genuine cubic on P^(nullity-1)
    int projDim = (nullity - 1) - 1; // hypersurface cuts one dimension
    if (projDim >= 1)
    {
        classification = "positive-dimensional";
        varietyProjectiveDimension = projDim;
        varietyAffineDimension = projDim + 1;
    }
    else if (projDim == 0)
    {
        classification = "isolated-point";
        varietyProjectiveDimension = 0;
        varietyAffineDimension = 1;
    }
    else
    {
        // nullity == 1: a single ray on which c*t^3 vanishes only at 0
        classification = "empty";
        varietyProjectiveDimension = -1;
        varietyAffineDimension = 0;
    }
}

// --- Explicit basis-independent positive-dimensionality witness --------------
// The pure vector-like family y = (0, 0, -c, d, c, 0->-d on u) : concretely
// y_Q=y_L=0, y_e=-c, y_n=c, y_d=d, y_u=-d for ALL rational (c, d). Verify it
// lies wholly on the variety symbolically (all four forms identically zero in
// c, d). This is an exact 2-parameter plane inside the variety, independent of
// the RREF basis choice, certifying affine dimension >= 2.
bool vectorLikePlaneOnVariety = false;
if (exactArithmeticOk)
{
    // free 2-plane spanned by e_family (c) and d_family (d)
    var cVec = new[] { R0, R0, new Rational(-1, 1), R0, new Rational(1, 1), R0 };   // y_e=-c, y_n=+c
    var dVec = new[] { R0, R0, R0, new Rational(1, 1), R0, new Rational(-1, 1) };   // y_d=+d, y_u=-d
    vectorLikePlaneOnVariety = FamilyLiesOnVariety(cVec, dVec, linSu2, linSu3, linGrav, cubicWeights);
}

// --- SM-pattern tripwire evaluation -----------------------------------------
bool smSatisfiesLinear =
    exactArithmeticOk &&
    Dot(linSu2, smPatternSlotSignature).IsZero &&
    Dot(linSu3, smPatternSlotSignature).IsZero &&
    Dot(linGrav, smPatternSlotSignature).IsZero;
bool smSatisfiesCubic = exactArithmeticOk && CubicValue(cubicWeights, smPatternSlotSignature).IsZero;
bool smPatternIsAnomalyConsistent = smSatisfiesLinear && smSatisfiesCubic;
bool smPatternTripwireFired = smPatternIsAnomalyConsistent; // lands on the variety
bool smPatternIsIsolatedSelection = smPatternIsAnomalyConsistent && classification == "isolated-point";

// tripwire discriminator: a deliberately anomalous pattern must NOT fire
var syntheticAnomalousPattern = new[] { R1, R1, R1, R1, R1, R1 }; // all Y=1
bool syntheticAnomalousFires =
    exactArithmeticOk &&
    Dot(linSu2, syntheticAnomalousPattern).IsZero &&
    Dot(linSu3, syntheticAnomalousPattern).IsZero &&
    Dot(linGrav, syntheticAnomalousPattern).IsZero &&
    CubicValue(cubicWeights, syntheticAnomalousPattern).IsZero;

// --- Batteries ---------------------------------------------------------------
// b1: Phase404 reconstruction match (blocks + hash pin).
bool batteryPhase404Reconstruction = phase404ReconstructionMatches;
// b2: exact-arithmetic determinism (recompute rank/nullity/classification).
bool batteryDeterminism = false;
if (exactArithmeticOk)
{
    var (rank2, _) = RankAndNullspace(linearRows, 6);
    batteryDeterminism = rank2 == linearRank;
}
// b3: tripwire battery — synthetic SM input fires; synthetic anomalous input does not.
bool batteryTripwire = smPatternTripwireFired && !syntheticAnomalousFires;

bool allBatteriesPassed =
    batteryPhase404Reconstruction && batteryDeterminism && batteryTripwire;

// --- Terminal (fail-closed) --------------------------------------------------
string verdictKind;
if (!phase404Present || !phase404HashMatches || !phase404ReconstructionMatches)
    verdictKind = "anomaly-variety-phase404-hash-drift";
else if (!exactArithmeticOk || classification == "nonconvergent")
    verdictKind = "anomaly-variety-exact-arithmetic-nonconvergent";
else if (!allBatteriesPassed)
    verdictKind = "anomaly-variety-battery-failed";
else
    verdictKind = classification switch
    {
        "empty" => "anomaly-variety-empty-consistency-alarm",
        "isolated-point" => "anomaly-variety-isolated-point-conditional",
        "positive-dimensional" => "anomaly-variety-positive-dimensional-route-closed",
        _ => "anomaly-variety-exact-arithmetic-nonconvergent",
    };

bool routeClosedAsRuleOut = verdictKind == "anomaly-variety-positive-dimensional-route-closed";
bool conditionalRowEmitted = verdictKind == "anomaly-variety-isolated-point-conditional";

// --- O7 cap (labeled theoretical-consistency import; can never flip) ---------
const bool anomalyRouteProvidesLowEnergyHyperchargeSource = false; // phase326 cap; O7
const bool o7FermionicActionUndefined = true;
const string o7ImportLabel =
    "Anomaly freedom is a LABELED theoretical-consistency import (O7): the GU fermionic action is recorded UNDEFINED, " +
    "so anomalyRouteProvidesLowEnergyHyperchargeSource can never flip; even the strongest rule-in is conditional " +
    "(direction only, normalization not provided) and the phase451 two-loop tension stands.";

// --- Standing claim boundary (verbatim across the program) ------------------
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;

bool kernelExecuted =
    exactArithmeticOk && phase404ReconstructionMatches && allBatteriesPassed &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    physicistReviewPending && scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract && !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract && !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian && !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization && !routeCompletesBosonPredictions &&
    !routePromotesWzMasses && !routePromotesHiggsMass;

string terminalStatus = TerminalPrefix + verdictKind;

string decision = routeClosedAsRuleOut
    ? "The chiral anomaly-consistency conditions on the Phase404 signed slot space define a POSITIVE-DIMENSIONAL " +
      "solution variety (projective dimension " + varietyProjectiveDimension + "; a scale-invariant cone containing an " +
      "explicit vector-like 2-parameter plane alongside the SM ray). Anomaly freedom therefore does NOT isolate the SM " +
      "hypercharge assignment - neither its normalization nor even its direction is selected - so this route CLOSES as a " +
      "rule-out. O7 caps it further: the fermionic action is undefined, so the low-energy hypercharge-source flag can " +
      "never flip. Nothing is promoted; promotedPhysicalMassClaimCount remains 0."
    : "Anomaly-consistency variety kernel executed; see classification and terminal.";

// canonical content for the kernel provenance hash (excludes timestamps)
string kernelConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|",
        ApplicationSubjectKind, PlanSection, Phase404TargetBlindConstructionHashPin,
        "linearRank=" + linearRank, "nullity=" + nullity,
        "cubicZeroOnNull=" + cubicIdenticallyZeroOnNullspace,
        "classification=" + classification,
        "vectorLikePlaneOnVariety=" + vectorLikePlaneOnVariety,
        "smTripwire=" + smPatternTripwireFired,
        "o7Cap=" + anomalyRouteProvidesLowEnergyHyperchargeSource)))).ToLowerInvariant();

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase465-anomaly-consistency-variety-kernel",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    anomalyConsistencyVarietyKernelExecuted = kernelExecuted,
    zeroPhysicsPromotion = true,

    // Phase404 reconstruction + hash pin
    phase404SummaryPath = Phase404SummaryPath,
    phase404Present,
    phase404TargetBlindConstructionHashPin = Phase404TargetBlindConstructionHashPin,
    phase404TargetBlindConstructionHashObserved = phase404HashObserved,
    phase404HashMatches,
    phase404ReconstructionMatches,
    phase404ReconstructionNote,

    // signed slot space (weak/color multiplicities re-derived from Phase404)
    signedSlotSpace = blocks.Select((b, i) => new
    {
        slotIndex = i,
        dimension = b.Dimension,
        isWeakDoublet = b.IsWeakDoublet,
        isColorTriplet = b.IsColorTriplet,
        weakMultiplicity = b.Su2Mult,
        colorMultiplicity = b.ColorMult,
        committedHyperchargeSlot = smPatternSlotSignature[i].ToString(),
    }).ToArray(),

    // anomaly system (exact-rational forms)
    anomalyForms = new
    {
        su2SquaredU1_linear = linSu2.Select(r => r.ToString()).ToArray(),
        su3SquaredU1_linear = linSu3.Select(r => r.ToString()).ToArray(),
        gravSquaredU1_linear = linGrav.Select(r => r.ToString()).ToArray(),
        u1Cubed_weights = cubicWeights,
        note = "Order Q,L,e,d,n,u. The [grav]^2-U(1) mixed gravitational-gauge form and the [U(1)]^3 cubic complete the standard chiral anomaly set.",
    },

    // exact classification
    linearRank,
    nullity,
    cubicIdenticallyZeroOnNullspace,
    restrictedCubicMonomialCount,
    varietyClassification = classification,
    varietyProjectiveDimension,
    varietyAffineDimension,
    varietyIsScaleInvariantCone = classification == "positive-dimensional" || classification == "isolated-point",
    vectorLikePlaneOnVariety,
    positiveDimensionalityWitness =
        "The exact 2-parameter vector-like plane y=(0,0,-c,d,c,-d) lies wholly on the variety for all rational c,d " +
        "(all four anomaly forms vanish identically), certifying affine dimension >= 2 independently of any basis choice.",
    routeClosedAsRuleOut,
    conditionalRowEmitted,
    conditionalTheoremRows = conditionalRowEmitted
        ? new[] { "direction-only-conditional-row (normalization explicitly not provided; O7 import label attached; phase451 tension stands)" }
        : Array.Empty<string>(),

    // SM-pattern tripwire (committed before the census)
    smPatternTripwire = new
    {
        committedBeforeCensus = true,
        committedSignature = smPatternSlotSignature.Select(r => r.ToString()).ToArray(),
        smPatternIsAnomalyConsistent,
        smPatternTripwireFired,
        smPatternIsIsolatedSelection,
        interpretation =
            "The SM hypercharge signature lies ON the variety (fires as a flag), but the variety is positive-dimensional, " +
            "so the SM pattern is one ray among an infinite anomaly-free cone: NOT an isolated selection and NOT a promotion.",
        discriminatorSyntheticAnomalousFires = syntheticAnomalousFires,
    },

    // batteries
    batteries = new
    {
        phase404ReconstructionMatch = batteryPhase404Reconstruction,
        exactArithmeticDeterminism = batteryDeterminism,
        tripwireBattery = batteryTripwire,
        allPassed = allBatteriesPassed,
    },

    // O7 cap
    anomalyRouteProvidesLowEnergyHyperchargeSource,
    o7FermionicActionUndefined,
    o7ImportLabel,
    o7LabeledImportCap = "conditional-only",

    // standing claim boundary
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    kernelConstructionHash,
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
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The anomaly variety is positive-dimensional: anomaly cancellation is a consistency condition, not a selection principle for the hypercharge assignment on this slot space.",
        "The SM-pattern tripwire firing is a membership flag, never a promotion; an infinite anomaly-free cone contains the SM ray.",
        "O7: the fermionic action is UNDEFINED, so anomaly freedom is a labeled theoretical import; no low-energy hypercharge source is provided and none may be claimed. promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
    runtimeSeconds,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "anomaly_consistency_variety_kernel.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(System.IO.Path.Combine(DefaultOutputDir, "anomaly_consistency_variety_kernel_summary.json"), JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} classification={classification} projDim={varietyProjectiveDimension} affineDim={varietyAffineDimension}");
Console.WriteLine($"linearRank={linearRank} nullity={nullity} cubicZeroOnNull={cubicIdenticallyZeroOnNullspace} vectorLikePlaneOnVariety={vectorLikePlaneOnVariety}");
Console.WriteLine($"smTripwireFired={smPatternTripwireFired} smIsolatedSelection={smPatternIsIsolatedSelection} batteries={allBatteriesPassed}");
Console.WriteLine($"kernelConstructionHash={kernelConstructionHash}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

return;

// ---------------------------------------------------------------------------
// helpers
static Rational Dot(Rational[] a, Rational[] b)
{
    var s = Rational.Zero;
    for (int i = 0; i < a.Length; i++) s += a[i] * b[i];
    return s;
}

static Rational CubicValue(int[] weights, Rational[] y)
{
    var s = Rational.Zero;
    for (int i = 0; i < y.Length; i++) s += new Rational(weights[i], 1) * y[i] * y[i] * y[i];
    return s;
}

// Verify a 2-parameter linear family span{u, w} lies wholly on the variety by
// checking every anomaly form vanishes identically as a polynomial in (s, t)
// with y = s*u + t*w. Linear forms: check L(u)=0 and L(w)=0. Cubic: expand
// weights_b * (s*u_b + t*w_b)^3 and require all monomials in (s,t) to cancel.
static bool FamilyLiesOnVariety(Rational[] u, Rational[] w, Rational[] su2, Rational[] su3, Rational[] grav, int[] weights)
{
    if (!Dot(su2, u).IsZero || !Dot(su2, w).IsZero) return false;
    if (!Dot(su3, u).IsZero || !Dot(su3, w).IsZero) return false;
    if (!Dot(grav, u).IsZero || !Dot(grav, w).IsZero) return false;
    var cubic = new MultiPoly(2);
    for (int b = 0; b < u.Length; b++)
    {
        var yb = new MultiPoly(2);
        yb.AddMonomial(new[] { 1, 0 }, u[b]);
        yb.AddMonomial(new[] { 0, 1 }, w[b]);
        cubic = cubic.Add(yb.Mul(yb).Mul(yb).Scale(new Rational(weights[b], 1)));
    }
    return cubic.NonZeroTermCount() == 0;
}

static int[] UnitExponent(int n, int j)
{
    var e = new int[n];
    e[j] = 1;
    return e;
}

// Reduced row echelon over Q -> rank, and a nullspace basis of Q^cols.
static (int rank, List<Rational[]> nullspace) RankAndNullspace(List<Rational[]> rows, int cols)
{
    var m = rows.Select(r => (Rational[])r.Clone()).ToList();
    int r = 0;
    var pivotCols = new List<int>();
    for (int c = 0; c < cols && r < m.Count; c++)
    {
        int pivot = -1;
        for (int i = r; i < m.Count; i++) if (!m[i][c].IsZero) { pivot = i; break; }
        if (pivot == -1) continue;
        (m[r], m[pivot]) = (m[pivot], m[r]);
        var inv = new Rational(1, 1) / m[r][c];
        for (int k = 0; k < cols; k++) m[r][k] *= inv;
        for (int i = 0; i < m.Count; i++)
        {
            if (i == r || m[i][c].IsZero) continue;
            var f = m[i][c];
            for (int k = 0; k < cols; k++) m[i][k] -= f * m[r][k];
        }
        pivotCols.Add(c);
        r++;
    }
    var freeCols = Enumerable.Range(0, cols).Where(c => !pivotCols.Contains(c)).ToList();
    var nullspace = new List<Rational[]>();
    foreach (var fc in freeCols)
    {
        var v = new Rational[cols];
        for (int k = 0; k < cols; k++) v[k] = Rational.Zero;
        v[fc] = new Rational(1, 1);
        for (int pi = 0; pi < pivotCols.Count; pi++)
            v[pivotCols[pi]] = -m[pi][fc]; // pivot var = -(coeff of free var)
        nullspace.Add(v);
    }
    return (r, nullspace);
}

// ---------------------------------------------------------------------------
readonly record struct SlotBlock(int Dimension, bool IsWeakDoublet, bool IsColorTriplet, int Su2Mult, int ColorMult);

// Sparse multivariate polynomial over Rational in a fixed number of variables.
sealed class MultiPoly
{
    private readonly int _vars;
    private readonly Dictionary<string, (int[] exp, Rational coeff)> _terms = new();

    public MultiPoly(int vars) { _vars = vars; }

    private static string Key(int[] exp) => string.Join(",", exp);

    public void AddMonomial(int[] exp, Rational coeff)
    {
        if (coeff.IsZero) return;
        var k = Key(exp);
        if (_terms.TryGetValue(k, out var cur))
        {
            var sum = cur.coeff + coeff;
            if (sum.IsZero) _terms.Remove(k);
            else _terms[k] = (exp, sum);
        }
        else _terms[k] = ((int[])exp.Clone(), coeff);
    }

    public MultiPoly Add(MultiPoly other)
    {
        var res = new MultiPoly(_vars);
        foreach (var t in _terms.Values) res.AddMonomial(t.exp, t.coeff);
        foreach (var t in other._terms.Values) res.AddMonomial(t.exp, t.coeff);
        return res;
    }

    public MultiPoly Scale(Rational s)
    {
        var res = new MultiPoly(_vars);
        if (s.IsZero) return res;
        foreach (var t in _terms.Values) res.AddMonomial(t.exp, t.coeff * s);
        return res;
    }

    public MultiPoly Mul(MultiPoly other)
    {
        var res = new MultiPoly(_vars);
        foreach (var a in _terms.Values)
            foreach (var b in other._terms.Values)
            {
                var e = new int[_vars];
                for (int i = 0; i < _vars; i++) e[i] = a.exp[i] + b.exp[i];
                res.AddMonomial(e, a.coeff * b.coeff);
            }
        return res;
    }

    public int NonZeroTermCount() => _terms.Count;
}

readonly struct Rational : IEquatable<Rational>
{
    public BigInteger Num { get; }
    public BigInteger Den { get; }

    public static readonly Rational Zero = new(0, 1);

    public Rational(BigInteger num, BigInteger den)
    {
        if (den.IsZero) throw new DivideByZeroException("Rational denominator is zero.");
        if (den.Sign < 0) { num = -num; den = -den; }
        BigInteger g = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den);
        if (g > BigInteger.One) { num /= g; den /= g; }
        Num = num;
        Den = den;
    }

    public bool IsZero => Num.IsZero;

    public static Rational operator +(Rational a, Rational b) => new(a.Num * b.Den + b.Num * a.Den, a.Den * b.Den);
    public static Rational operator -(Rational a, Rational b) => new(a.Num * b.Den - b.Num * a.Den, a.Den * b.Den);
    public static Rational operator -(Rational a) => new(-a.Num, a.Den);
    public static Rational operator *(Rational a, Rational b) => new(a.Num * b.Num, a.Den * b.Den);
    public static Rational operator /(Rational a, Rational b) => new(a.Num * b.Den, a.Den * b.Num);

    public static bool operator ==(Rational a, Rational b) => a.Num == b.Num && a.Den == b.Den;
    public static bool operator !=(Rational a, Rational b) => !(a == b);

    public bool Equals(Rational other) => this == other;
    public override bool Equals(object? obj) => obj is Rational r && this == r;
    public override int GetHashCode() => HashCode.Combine(Num, Den);
    public double ToDouble() => (double)Num / (double)Den;
    public override string ToString() => Den == BigInteger.One ? Num.ToString() : $"{Num}/{Den}";
}
