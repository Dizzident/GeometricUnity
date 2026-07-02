using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase433: blind beta-coefficient running ledger (the running extension of
// the Phase429 target-blind dimensionless-ratio ledger).
//
// Phase429 recorded the scale-FREE surface fixed blind by the internal
// embedding chain: tan^2 theta_W = 3/5 and sin^2 theta_W = 3/8 at the
// unification point. One-loop beta coefficients are the next scale-free
// datum: they are PURE GROUP THEORY of the derived matter content - no
// measured value, no dimensionful scale, and no comparison to observation
// enters anywhere. This phase computes the one-loop b_i as EXACT RATIONALS
// from the Phase404-derived one-family 16 content, then materializes the
// one-parameter running family sin^2 theta_W(x) as an exact rational
// function of a single dimensionless running variable. DERIVATION IS
// STRICTLY SEPARATED FROM COMPARISON, exactly as in Phase429: no measured
// electroweak value appears, no residual against observation is computed,
// and no Phase201/Phase256 field is filled.
//
// Convention (stated once, used consistently): the renormalization-group
// equation is  d g_i / d ln mu = - b_i g_i^3 / (16 pi^2), so a POSITIVE b_i
// means ASYMPTOTIC FREEDOM (the coupling weakens toward the ultraviolet).
// Integrating d(1/g_i^2)/d ln mu = + b_i/(8 pi^2) from the unification scale
// M down to mu gives
//     1/g_i^2(mu) = 1/g_GUT^2 - (b_i/(8 pi^2)) ln(M/mu),
// i.e. for an asymptotically free group (b_i > 0) the coupling GROWS toward
// the infrared. (A "+" sign on the running term corresponds to the opposite
// beta-function sign convention; we fix the asymptotic-freedom-positive
// convention throughout and derive the running relation from it.)
//
// One-loop coefficient formula (Weyl fermions, complex scalars):
//     b = (11/3) C_2(G) - (2/3) sum_{Weyl fermions} T(R)
//                       - (1/3) sum_{complex scalars} T(R).

const string DefaultOutputDir = "studies/phase433_blind_beta_coefficient_running_ledger_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase429SummaryPath = "studies/phase429_target_blind_dimensionless_ratio_ledger_001/output/target_blind_dimensionless_ratio_ledger_summary.json";
const string ApplicationSubjectKind = "blind-beta-coefficient-running-ledger";

var outputDir = Environment.GetEnvironmentVariable("PHASE433_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var phase429 = JsonDocument.Parse(File.ReadAllText(Phase429SummaryPath));

bool phase404PrecursorPassed =
    JsonBool(phase404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true &&
    JsonBool(phase404.RootElement, "familyPatternDerived") is true;
bool phase429PrecursorPassed =
    JsonBool(phase429.RootElement, "targetBlindDimensionlessRatioLedgerPassed") is true &&
    JsonBool(phase429.RootElement, "derivationComparisonSeparationMaintained") is true;

// ---------------------------------------------------------------------------
// The derived one-family matter content (Phase404 pattern). Every hypercharge
// below is the Phase404-derived charge bookkeeping, NOT an imported value.
// nu^c is a total singlet (all charges/quantum-numbers trivial) and drops out
// of every group's index sum.
// ---------------------------------------------------------------------------
var family = new[]
{
    //          name  colorDim  su2Dim   Y (rational)
    new WeylMultiplet("Q",   3, 2, new Rational(1, 6)),
    new WeylMultiplet("u^c", 3, 1, new Rational(-2, 3)),
    new WeylMultiplet("d^c", 3, 1, new Rational(1, 3)),
    new WeylMultiplet("L",   1, 2, new Rational(-1, 2)),
    new WeylMultiplet("e^c", 1, 1, new Rational(1, 1)),
    new WeylMultiplet("nu^c",1, 1, new Rational(0, 1)),
};

// Dynkin index of the fundamental is 1/2 for SU(N); a singlet contributes 0.
Rational THalf = new(1, 2);

// SU(3)_c fermion index sum per family: only color (anti)triplets contribute,
// each weighted by its SU(2) multiplicity. Q -> 2 * 1/2, u^c -> 1/2, d^c -> 1/2.
Rational sumTColorPerFamily = Rational.Zero;
foreach (var m in family)
    if (m.ColorDim == 3)
        sumTColorPerFamily += THalf * m.Su2Dim; // weighted by su(2) multiplicity

// SU(2)_L fermion index sum per family: only doublets contribute, each
// weighted by its color multiplicity. Q -> 3 * 1/2, L -> 1 * 1/2.
Rational sumTWeakPerFamily = Rational.Zero;
foreach (var m in family)
    if (m.Su2Dim == 2)
        sumTWeakPerFamily += THalf * m.ColorDim; // weighted by color multiplicity

// U(1)_Y: sum of Y^2 over all Weyl states (full color x weak multiplicity).
Rational sumYSquaredPerFamily = Rational.Zero;
foreach (var m in family)
    sumYSquaredPerFamily += m.Y * m.Y * (m.ColorDim * m.Su2Dim);

// GUT normalization g_1^2 = (5/3) g_Y^2  =>  the properly normalized U(1)
// index is T_1 = (3/5) * sum Y^2.
Rational gutNorm = new(3, 5);
Rational sumTHyperPerFamily = gutNorm * sumYSquaredPerFamily;

// Family-universal fermion contribution to b per group: (2/3) * (index sum).
Rational twoThirds = new(2, 3);
Rational fermionContribColor = twoThirds * sumTColorPerFamily;
Rational fermionContribWeak = twoThirds * sumTWeakPerFamily;
Rational fermionContribHyper = twoThirds * sumTHyperPerFamily;

// Gauge (pure Yang-Mills) contribution: (11/3) C_2(G).
Rational elevenThirds = new(11, 3);
Rational gaugeColor = elevenThirds * new Rational(3, 1); // C2(SU(3)) = 3
Rational gaugeWeak = elevenThirds * new Rational(2, 1);  // C2(SU(2)) = 2
Rational gaugeHyper = Rational.Zero;                     // C2(U(1)) = 0

// Conditional Higgs (one complex doublet (1,2,1/2)) scalar contribution:
// -(1/3) T(R). Color singlet -> 0 for b_3. SU(2) doublet -> T = 1/2. U(1):
// sum Y^2 over the two doublet states = 2 * (1/2)^2 = 1/2, GUT-normalized.
Rational oneThird = new(1, 3);
Rational higgsDeltaColor = Rational.Zero;
Rational higgsDeltaWeak = -(oneThird * THalf);                       // -1/6
Rational higgsSumYSquared = new Rational(1, 2) * new Rational(1, 2) * 2; // 1/2
Rational higgsDeltaHyper = -(oneThird * gutNorm * higgsSumYSquared);    // -1/10

// ---------------------------------------------------------------------------
// Build the ledger rows: (n_f in {1, 3}) x (Higgs conditional off/on).
// n_f = 1 is the derived single family; n_f = 3 is the OBSERVED family count,
// an imported structural parameter that this chain does NOT derive.
// ---------------------------------------------------------------------------
int[] familyCounts = { 1, 3 };
var rows = new List<BetaRow>();
foreach (int nf in familyCounts)
{
    // b_i without the Higgs, n_f families.
    Rational b3 = gaugeColor - fermionContribColor * nf;
    Rational b2 = gaugeWeak - fermionContribWeak * nf;
    Rational b1 = gaugeHyper - fermionContribHyper * nf;

    rows.Add(MakeRow(nf, higgsIncluded: false, b3, b2, b1));

    // With the conditional Higgs doublet.
    rows.Add(MakeRow(nf, higgsIncluded: true,
        b3 + higgsDeltaColor, b2 + higgsDeltaWeak, b1 + higgsDeltaHyper));
}

// ---------------------------------------------------------------------------
// Verification batteries. Every expected value is checked as an EXACT rational
// equality (no floating-point tolerance).
// ---------------------------------------------------------------------------
bool gaugeCasimirsCorrect =
    gaugeColor == new Rational(11, 1) &&      // (11/3)*3 = 11
    gaugeWeak == new Rational(22, 3) &&       // (11/3)*2 = 22/3
    gaugeHyper == Rational.Zero;

bool perFamilyIndexSumsUniform =
    sumTColorPerFamily == new Rational(2, 1) &&
    sumTWeakPerFamily == new Rational(2, 1) &&
    sumTHyperPerFamily == new Rational(2, 1); // all three index sums equal 2 per family

bool sumYSquaredIsTenThirds = sumYSquaredPerFamily == new Rational(10, 3);

bool fermionContribsAreFourThirds =
    fermionContribColor == new Rational(4, 3) &&
    fermionContribWeak == new Rational(4, 3) &&
    fermionContribHyper == new Rational(4, 3);

bool higgsDeltasCorrect =
    higgsDeltaColor == Rational.Zero &&
    higgsDeltaWeak == new Rational(-1, 6) &&
    higgsDeltaHyper == new Rational(-1, 10);

// Row-level expected exact values.
BetaRow Row(int nf, bool h) => rows.Single(r => r.FamilyCount == nf && r.HiggsIncluded == h);
bool expectedRowValues =
    Row(1, false).B3 == new Rational(29, 3) && Row(1, false).B2 == new Rational(6, 1) && Row(1, false).B1 == new Rational(-4, 3) &&
    Row(1, true).B3 == new Rational(29, 3) && Row(1, true).B2 == new Rational(35, 6) && Row(1, true).B1 == new Rational(-43, 30) &&
    Row(3, false).B3 == new Rational(7, 1) && Row(3, false).B2 == new Rational(10, 3) && Row(3, false).B1 == new Rational(-4, 1) &&
    // n_f = 3 with Higgs reproduces the standard SM one-loop set in the AF-positive convention:
    Row(3, true).B3 == new Rational(7, 1) && Row(3, true).B2 == new Rational(19, 6) && Row(3, true).B1 == new Rational(-41, 10);

// The general formula check: b3 = 11 - (4/3) n_f, b2 = 22/3 - (4/3) n_f,
// b1 = -(4/3) n_f (without Higgs), for every tabulated n_f.
bool generalFormulaHolds = familyCounts.All(nf =>
{
    var r = Row(nf, false);
    return r.B3 == new Rational(11, 1) - new Rational(4, 3) * nf
        && r.B2 == new Rational(22, 3) - new Rational(4, 3) * nf
        && r.B1 == -(new Rational(4, 3) * nf);
});

// The b_1 - b_2 combination that drives the weak-angle running is
// family-INDEPENDENT (each family contributes equally to both), fixed only by
// the Higgs conditional: -22/3 without Higgs, -109/15 with Higgs.
bool b1MinusB2FamilyIndependent =
    Row(1, false).B1 - Row(1, false).B2 == new Rational(-22, 3) &&
    Row(3, false).B1 - Row(3, false).B2 == new Rational(-22, 3) &&
    Row(1, true).B1 - Row(1, true).B2 == new Rational(-109, 15) &&
    Row(3, true).B1 - Row(3, true).B2 == new Rational(-109, 15);

// Running-ledger batteries: sin^2(0) = 3/8 on every row, and the exact linear
// slope 15 (b_1 - b_2) / 64 (n_f-independent, Higgs-dependent).
bool runningSinSquaredAtUnificationIsThreeEighths =
    rows.All(r => r.SinSquaredConstNum == new Rational(3, 1) && r.SinSquaredDenConst == new Rational(8, 1)
                  && (r.SinSquaredConstNum / r.SinSquaredDenConst) == new Rational(3, 8));
bool runningSlopesCorrect =
    Row(1, false).SinSquaredLinearSlope == new Rational(-55, 32) &&
    Row(3, false).SinSquaredLinearSlope == new Rational(-55, 32) &&
    Row(1, true).SinSquaredLinearSlope == new Rational(-109, 64) &&
    Row(3, true).SinSquaredLinearSlope == new Rational(-109, 64);

bool exactRationalArithmeticVerified =
    gaugeCasimirsCorrect &&
    perFamilyIndexSumsUniform &&
    sumYSquaredIsTenThirds &&
    fermionContribsAreFourThirds &&
    higgsDeltasCorrect &&
    expectedRowValues &&
    generalFormulaHolds &&
    b1MinusB2FamilyIndependent &&
    runningSinSquaredAtUnificationIsThreeEighths &&
    runningSlopesCorrect;

// ---------------------------------------------------------------------------
// Derivation/comparison separation (identical discipline to Phase429).
// ---------------------------------------------------------------------------
const bool measuredElectroweakValuesConsulted = false;
const bool comparisonAgainstObservationPerformed = false;
const bool derivationComparisonSeparationMaintained = true;
const bool sourceDefinesRunningOrThresholds = false;
const bool sourceDefinesUnificationScale = false;
const bool sourceDefinesSymmetryBreakingSector = false;

// Family multiplicity provenance: 1 is derived, 3 is imported structure.
const int derivedFamilyCount = 1;
const int importedObservedFamilyCount = 3;
const bool familyCountIsDerived = false;
const bool familyCountIsImportedStructuralParameter = true;

// Higgs conditional provenance: recorded separately, never asserted.
const bool higgsBreakingSectorEstablished = false;
const bool higgsContributionRecordedAsConditional = true;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false; // group-theory running, not scale lineage
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

// missing comparison lineage shared by every row that would face observation
var comparisonLineageFields = new[]
{
    "the-unification-scale-M-itself (no dimensionful anchor exists in any source)",
    "two-loop-and-higher-running-corrections",
    "threshold-corrections-and-matching-at-intermediate-and-electroweak-scales",
    "renormalization-scheme-definition-for-the-compared-coupling",
    "the-actual-matter-content-between-scales (this ledger assumes exactly the derived family, nothing more)",
    "input-coupling-value-g_GUT-at-the-unification-point (a scale-fixing datum, deliberately not supplied)",
};

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "exact-rational one-loop b_i from the Phase404-derived 16 content; AF-positive convention; running ledger sin^2(x); derivation strictly separated from comparison; no measured values")))).ToLowerInvariant();

bool blindBetaCoefficientRunningLedgerPassed =
    phase404PrecursorPassed &&
    phase429PrecursorPassed &&
    exactRationalArithmeticVerified &&
    higgsContributionRecordedAsConditional &&
    familyCountIsImportedStructuralParameter &&
    !familyCountIsDerived &&
    !measuredElectroweakValuesConsulted &&
    !comparisonAgainstObservationPerformed &&
    derivationComparisonSeparationMaintained &&
    !sourceDefinesRunningOrThresholds &&
    !sourceDefinesUnificationScale &&
    !sourceDefinesSymmetryBreakingSector &&
    !higgsBreakingSectorEstablished &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
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

string terminalStatus = blindBetaCoefficientRunningLedgerPassed
    ? "blind-beta-coefficient-running-ledger-materialized-comparison-lineage-still-missing"
    : "blind-beta-coefficient-running-ledger-blocked";

string decision = blindBetaCoefficientRunningLedgerPassed
    ? "The one-loop beta coefficients of the derived matter content are now an explicit fail-closed ledger of EXACT rationals, computed as pure group theory with no measured value and no comparison anywhere. Per derived family every gauge group receives the identical fermion index sum 2 (color, weak, and GUT-normalized hypercharge alike), giving the family-universal contribution 4/3; the conditional Higgs doublet adds -1/6 to b_2 and -1/10 to b_1 and is recorded separately and marked conditional (no breaking sector is established). The n_f = 3 (imported, not derived) with-Higgs row reproduces the standard SM set (b_3, b_2, b_1) = (7, 19/6, -41/10) in the asymptotic-freedom-positive convention. The running family sin^2 theta_W(x) = 3(1 - b_2 x)/(8 - (5 b_1 + 3 b_2) x) is fixed at 3/8 at the unification point with exact linear slope 15(b_1 - b_2)/64, which is family-INDEPENDENT (-55/32 without Higgs, -109/64 with). Every row still lacks the full comparison lineage - the unification scale, higher-order and threshold corrections, the scheme, the actual inter-scale matter content, and an input coupling; no measured value was consulted, no comparison was performed, and no Phase201/Phase256 field is filled."
    : "Do not use the beta-coefficient running ledger until the precursor and separation batteries pass.";

var result = new
{
    phaseId = "phase433-blind-beta-coefficient-running-ledger",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    blindBetaCoefficientRunningLedgerPassed,
    phase404PrecursorPassed,
    phase429PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    exactRationalArithmeticVerified,
    convention = new
    {
        rgEquation = "d g_i / d ln mu = - b_i g_i^3 / (16 pi^2)",
        signMeaning = "positive b_i means asymptotic freedom (coupling weakens toward the ultraviolet)",
        runningRelation = "1/g_i^2(mu) = 1/g_GUT^2 - (b_i/(8 pi^2)) ln(M/mu)",
        coefficientFormula = "b = (11/3) C_2(G) - (2/3) sum_Weyl T(R) - (1/3) sum_complex-scalar T(R)",
        gutNormalization = "g_1^2 = (5/3) g_Y^2  =>  U(1) index = (3/5) sum Y^2",
    },
    derivedFamilyContent = family.Select(m => new
    {
        name = m.Name,
        colorDim = m.ColorDim,
        su2Dim = m.Su2Dim,
        hyperchargeY = m.Y.ToString(),
        isTotalSinglet = m.ColorDim == 1 && m.Su2Dim == 1 && m.Y.IsZero,
    }).ToArray(),
    perFamilyIndexSums = new
    {
        su3ColorIndexSum = sumTColorPerFamily.ToString(),
        su2WeakIndexSum = sumTWeakPerFamily.ToString(),
        u1HyperIndexSumGutNormalized = sumTHyperPerFamily.ToString(),
        sumYSquared = sumYSquaredPerFamily.ToString(),
        fermionContributionColor = fermionContribColor.ToString(),
        fermionContributionWeak = fermionContribWeak.ToString(),
        fermionContributionHyper = fermionContribHyper.ToString(),
    },
    gaugeContributions = new
    {
        color = gaugeColor.ToString(),
        weak = gaugeWeak.ToString(),
        hyper = gaugeHyper.ToString(),
    },
    conditionalHiggsContribution = new
    {
        recordedAsConditional = higgsContributionRecordedAsConditional,
        breakingSectorEstablished = higgsBreakingSectorEstablished,
        deltaB3 = higgsDeltaColor.ToString(),
        deltaB2 = higgsDeltaWeak.ToString(),
        deltaB1 = higgsDeltaHyper.ToString(),
        note = "one complex doublet (1,2,1/2); contributes only if a symmetry-breaking sector exists (Phases 405/410/418/428 closed internal mechanisms; phase430-chain experiments pending).",
    },
    betaLedger = rows.Select(r => new
    {
        familyCount = r.FamilyCount,
        familyCountProvenance = r.FamilyCount == derivedFamilyCount ? "derived-single-family" : "imported-observed-structural-parameter",
        higgsIncluded = r.HiggsIncluded,
        b3 = r.B3.ToString(),
        b2 = r.B2.ToString(),
        b1 = r.B1.ToString(),
        b3Decimal = r.B3.ToDouble(),
        b2Decimal = r.B2.ToDouble(),
        b1Decimal = r.B1.ToDouble(),
        runningSinSquared = new
        {
            closedForm = "sin^2(x) = (numConst + numLinear*x) / (denConst + denLinear*x)",
            runningVariable = "x = (g_GUT^2/(16 pi^2)) ln(M^2/mu^2) = (g_GUT^2/(8 pi^2)) ln(M/mu)",
            numConst = r.SinSquaredConstNum.ToString(),
            numLinear = r.SinSquaredNumLinear.ToString(),
            denConst = r.SinSquaredDenConst.ToString(),
            denLinear = r.SinSquaredDenLinear.ToString(),
            valueAtUnification = (r.SinSquaredConstNum / r.SinSquaredDenConst).ToString(),
            linearSlope = r.SinSquaredLinearSlope.ToString(),
            linearForm = $"sin^2(x) = 3/8 + ({r.SinSquaredLinearSlope}) x + O(x^2)",
        },
    }).ToArray(),
    betaLedgerRowCount = rows.Count,
    familyCountProvenanceNote = new
    {
        derivedFamilyCount,
        importedObservedFamilyCount,
        familyCountIsDerived,
        familyCountIsImportedStructuralParameter,
    },
    b1MinusB2FamilyIndependent,
    comparisonLineageFields,
    comparisonLineageFieldCount = comparisonLineageFields.Length,
    sourceDefinedComparisonLineageFieldCount = 0,
    measuredElectroweakValuesConsulted,
    comparisonAgainstObservationPerformed,
    derivationComparisonSeparationMaintained,
    sourceDefinesRunningOrThresholds,
    sourceDefinesUnificationScale,
    sourceDefinesSymmetryBreakingSector,
    higgsBreakingSectorEstablished,
    physicalCouplingProvided,
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
    sourceEvidence = new
    {
        phase404SummaryPath = Phase404SummaryPath,
        phase429SummaryPath = Phase429SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The b_i are pure group theory of the derived matter content; they are scale-free and carry no dimensionful information.",
        "No measured electroweak value appears in this phase and no comparison was performed; derivation and comparison are strictly separated by construction.",
        "Family multiplicity is NOT derived: n_f = 1 is the derived single family; n_f = 3 is an imported observed structural parameter, marked as such on every row.",
        "The Higgs row is conditional on a symmetry-breaking sector that no internal mechanism supplies (Phases 405/410/418/428 closed) and no source defines; it is recorded separately and never asserted.",
        "The running family sin^2(x) is a symbolic one-parameter surface in a dimensionless variable; it is NOT evaluated at any physical scale because no unification scale or input coupling exists in any source.",
        "Any future comparison phase must import the listed lineage fields explicitly and pass the existing promotion gates.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "blind_beta_coefficient_running_ledger.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "blind_beta_coefficient_running_ledger_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"blindBetaCoefficientRunningLedgerPassed={blindBetaCoefficientRunningLedgerPassed}");
Console.WriteLine($"exactRationalArithmeticVerified={exactRationalArithmeticVerified} betaLedgerRowCount={rows.Count}");
Console.WriteLine($"per-family index sums: SU(3)={sumTColorPerFamily} SU(2)={sumTWeakPerFamily} U(1)_GUT={sumTHyperPerFamily} (sumY^2={sumYSquaredPerFamily})");
Console.WriteLine($"fermion contribution per family per group = {fermionContribColor} (all groups)");
Console.WriteLine($"Higgs deltas: dB3={higgsDeltaColor} dB2={higgsDeltaWeak} dB1={higgsDeltaHyper}");
foreach (var r in rows)
    Console.WriteLine($"  n_f={r.FamilyCount} Higgs={r.HiggsIncluded}: b3={r.B3} b2={r.B2} b1={r.B1} | sin^2 slope={r.SinSquaredLinearSlope}");
Console.WriteLine($"comparisonLineageFieldCount={comparisonLineageFields.Length} sourceDefined=0");
Console.WriteLine($"measuredElectroweakValuesConsulted={measuredElectroweakValuesConsulted} comparisonAgainstObservationPerformed={comparisonAgainstObservationPerformed}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// Build one ledger row: given the without/with-Higgs b_i, also derive the
// exact rational running surface sin^2(x) = 3(1 - b_2 x)/(8 - (5 b_1 + 3 b_2) x).
static BetaRow MakeRow(int nf, bool higgsIncluded, Rational b3, Rational b2, Rational b1)
{
    // sin^2(x) = (3/5)(1 - b_2 x) / [(1 - b_1 x) + (3/5)(1 - b_2 x)]
    //          = 3(1 - b_2 x) / [8 - (5 b_1 + 3 b_2) x].
    Rational numConst = new(3, 1);
    Rational numLinear = new Rational(-3, 1) * b2;
    Rational denConst = new(8, 1);
    Rational denLinear = -(new Rational(5, 1) * b1 + new Rational(3, 1) * b2);
    // d/dx of numerator/denominator at x = 0 gives slope 15 (b_1 - b_2) / 64.
    Rational slope = new Rational(15, 1) * (b1 - b2) / new Rational(64, 1);
    return new BetaRow(nf, higgsIncluded, b3, b2, b1, numConst, numLinear, denConst, denLinear, slope);
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record WeylMultiplet(string Name, int ColorDim, int Su2Dim, Rational Y);

public sealed record BetaRow(
    int FamilyCount,
    bool HiggsIncluded,
    Rational B3,
    Rational B2,
    Rational B1,
    Rational SinSquaredConstNum,
    Rational SinSquaredNumLinear,
    Rational SinSquaredDenConst,
    Rational SinSquaredDenLinear,
    Rational SinSquaredLinearSlope);

// Exact rational arithmetic over BigInteger; always stored in lowest terms with
// a positive denominator.
public readonly struct Rational : IEquatable<Rational>
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
    public static Rational operator *(Rational a, int b) => new(a.Num * b, a.Den);
    public static Rational operator /(Rational a, Rational b) => new(a.Num * b.Den, a.Den * b.Num);

    public static bool operator ==(Rational a, Rational b) => a.Num == b.Num && a.Den == b.Den;
    public static bool operator !=(Rational a, Rational b) => !(a == b);

    public bool Equals(Rational other) => this == other;
    public override bool Equals(object? obj) => obj is Rational r && this == r;
    public override int GetHashCode() => HashCode.Combine(Num, Den);
    public double ToDouble() => (double)Num / (double)Den;
    public override string ToString() => Den == BigInteger.One ? Num.ToString() : $"{Num}/{Den}";
}
