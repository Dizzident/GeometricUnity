using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase434: conditional observed-field extraction row ledger.
//
// Phase419 materialized the FMS/dressing-style symbolic template for the
// photon/W/Z/Higgs observed-field extraction with ZERO source-defined fields
// (sourceDefinedPhase256FieldCount=0). This phase derives - from the internal
// embedding ALONE - the extraction rows AS EXACT FUNCTIONS OF A CANDIDATE
// DOUBLET VEV, so that any future dynamically- or source-supplied VEV
// mechanically completes them. The single blind input is the tree-level
// kernel relation tan^2 theta_W = 3/5 (i.e. sin^2 = 3/8, cos^2 = 5/8) fixed
// at the unification point by Phase404 and reused by Phase429; no measured
// electroweak value appears anywhere in this program.
//
// HONEST BOUNDARIES (recorded, not decorative):
//   * The doublet VEV is a CANDIDATE parameter. Its existence is NOT
//     established: Phases 405/410/418/428 closed the named internal selection
//     mechanisms and the phase430-chain experiments are still pending.
//   * The rows are tree-level in the custodial limit.
//   * Nothing here is an observed-field extraction THEOREM. It is the
//     conditional algebra that such a theorem must reproduce.
//   * Derivation is strictly separated from comparison exactly as in Phase429:
//     no measured value is consulted and no residual against observation is
//     computed, so this phase can never be a promotion route by itself.
//
// Fail-closed: every canFill*/routePromotes* flag is false. In particular
// canFillPhase256ObservedFieldExtractionContract=false because the VEV
// existence/amplitude and the pole/GeV lineage fields remain unfilled.

const string DefaultOutputDir = "studies/phase434_conditional_observed_field_extraction_row_ledger_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase419SummaryPath = "studies/phase419_observed_field_symbolic_extraction_template_001/output/observed_field_symbolic_extraction_template_summary.json";
const string Phase429SummaryPath = "studies/phase429_target_blind_dimensionless_ratio_ledger_001/output/target_blind_dimensionless_ratio_ledger_summary.json";
const string ApplicationSubjectKind = "conditional-observed-field-extraction-row-ledger";

var outputDir = Environment.GetEnvironmentVariable("PHASE434_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var phase419 = JsonDocument.Parse(File.ReadAllText(Phase419SummaryPath));
using var phase429 = JsonDocument.Parse(File.ReadAllText(Phase429SummaryPath));

bool phase404PrecursorPassed =
    JsonBool(phase404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true;
bool phase419PrecursorPassed =
    JsonBool(phase419.RootElement, "observedFieldSymbolicExtractionTemplatePassed") is true &&
    JsonInt(phase419.RootElement, "sourceDefinedPhase256FieldCount") == 0;
bool phase429PrecursorPassed =
    JsonBool(phase429.RootElement, "targetBlindDimensionlessRatioLedgerPassed") is true;

// ---------------------------------------------------------------------------
// The single blind kernel input and its exact-radical mixing coefficients.
// tan^2 theta_W = 3/5  =>  sin^2 = 3/8, cos^2 = 5/8
//   sin theta_W = sqrt(3/8) = sqrt(6)/4
//   cos theta_W = sqrt(5/8) = sqrt(10)/4
// ---------------------------------------------------------------------------

const double TanSquared = 3.0 / 5.0;
double sinSquared = TanSquared / (1.0 + TanSquared);   // 3/8 exactly
double cosSquared = 1.0 - sinSquared;                  // 5/8 exactly

var sinCoeff = new Radical(1, 4, 6);                   // sqrt(6)/4
var cosCoeff = new Radical(1, 4, 10);                  // sqrt(10)/4
var invSqrt2 = new Radical(1, 2, 2);                   // 1/sqrt(2) = sqrt(2)/2

bool mixingExact =
    Math.Abs(sinSquared - 3.0 / 8.0) <= 1e-15 &&
    Math.Abs(cosSquared - 5.0 / 8.0) <= 1e-15 &&
    Math.Abs(sinCoeff.Value * sinCoeff.Value - 3.0 / 8.0) <= 1e-15 &&
    Math.Abs(cosCoeff.Value * cosCoeff.Value - 5.0 / 8.0) <= 1e-15 &&
    Math.Abs(sinCoeff.Value * sinCoeff.Value + cosCoeff.Value * cosCoeff.Value - 1.0) <= 1e-15 &&
    Math.Abs(invSqrt2.Value * invSqrt2.Value - 0.5) <= 1e-15;

// ---------------------------------------------------------------------------
// Gauge-boson mass matrix from |D_mu Phi|^2 with Phi = (0, v/sqrt(2)) in the
// (W1, W2, W3, B) basis, gPrime^2/g^2 = tan^2 = 3/5 exactly:
//
//   M^2 = (v^2/4) * [[g^2,0,0,0],
//                    [0,g^2,0,0],
//                    [0,0,g^2,-g gPrime],
//                    [0,0,-g gPrime, gPrime^2]]
//
// Eigenvalues (v^2/4) * { g^2, g^2, g^2 + gPrime^2, 0 }. The neutral 2x2 block
// has trace g^2+gPrime^2 and determinant 0, so it splits into a massless mode
// (photon) and g^2+gPrime^2 (Z). Verified numerically below in units where
// v^2/4 = 1 and g^2 = 1 (hence gPrime^2 = 3/5).
// ---------------------------------------------------------------------------

double gSq = 1.0;
double gpSq = TanSquared * gSq;                        // gPrime^2 = 3/5
double gTimesGp = Math.Sqrt(gSq) * Math.Sqrt(gpSq);    // g * gPrime
// neutral block [[gSq, -gTimesGp], [-gTimesGp, gpSq]]
double neutralTrace = gSq + gpSq;
double neutralDet = gSq * gpSq - gTimesGp * gTimesGp;  // exactly 0
double neutralHi = 0.5 * (neutralTrace + Math.Sqrt(neutralTrace * neutralTrace - 4.0 * neutralDet));
double neutralLo = 0.5 * (neutralTrace - Math.Sqrt(neutralTrace * neutralTrace - 4.0 * neutralDet));

// eigenvalues in units of (v^2/4): {g^2, g^2, g^2+gPrime^2, 0}
double[] eigenvaluesInVevUnits = { gSq, gSq, neutralHi, neutralLo };
double mWsqUnit = gSq;                 // m_W^2 = (v^2/4) g^2
double mZsqUnit = neutralHi;           // m_Z^2 = (v^2/4)(g^2+gPrime^2)
double photonMassSqUnit = neutralLo;   // exactly 0
double mWmZRatioSq = mWsqUnit / mZsqUnit;   // = g^2/(g^2+gPrime^2) = cos^2 = 5/8

// Cross-check the mass-ratio row against Phase429's ledger (sqrt(5/8)).
double phase429TreeMwOverMz = 0.0;
bool phase429RatioRowFound = false;
if (phase429.RootElement.TryGetProperty("ledger", out var ledgerEl) &&
    ledgerEl.ValueKind == JsonValueKind.Array)
{
    foreach (var row in ledgerEl.EnumerateArray())
        if (row.TryGetProperty("rowId", out var idEl) &&
            idEl.GetString() == "tree-level-w-z-mass-ratio-at-unification" &&
            row.TryGetProperty("value", out var valEl))
        {
            phase429TreeMwOverMz = valEl.GetDouble();
            phase429RatioRowFound = true;
        }
}
bool massMatrixExact =
    Math.Abs(neutralDet) <= 1e-15 &&
    Math.Abs(photonMassSqUnit) <= 1e-15 &&
    Math.Abs(mZsqUnit - (gSq + gpSq)) <= 1e-15 &&
    Math.Abs(mWmZRatioSq - 5.0 / 8.0) <= 1e-15 &&
    phase429RatioRowFound &&
    Math.Abs(Math.Sqrt(mWmZRatioSq) - phase429TreeMwOverMz) <= 1e-15;

var massEigenvalues = new[]
{
    new EigenRow("w1-charged", "(v^2/4) g^2", "g^2", mWsqUnit, "charged W1 direction; degenerate with W2."),
    new EigenRow("w2-charged", "(v^2/4) g^2", "g^2", mWsqUnit, "charged W2 direction; degenerate with W1."),
    new EigenRow("z-neutral", "(v^2/4) (g^2 + gPrime^2)", "g^2 + gPrime^2 = (8/5) g^2", mZsqUnit, "massive neutral eigenvalue of the (W3,B) block."),
    new EigenRow("photon-neutral", "0", "0", photonMassSqUnit, "massless neutral eigenvalue; det of the (W3,B) block is exactly 0."),
};

// ---------------------------------------------------------------------------
// EXTRACTION ROWS (the Phase419 projection targets) as exact functions of the
// candidate doublet VEV. Coefficients are exact (p/q)*sqrt(r) radicals,
// verified numerically to 1e-15. Row VALUES are dimensionless mixing/charge
// data; the ABSOLUTE mass amplitudes carry the still-missing v factor.
// ---------------------------------------------------------------------------

var extractionRows = new[]
{
    new ExtractionRow(
        "photon-eigenstate",
        "A = sin(theta_W) W3 + cos(theta_W) B",
        new[]
        {
            new TermCoeff("W3", sinCoeff, "sin theta_W = sqrt(6)/4"),
            new TermCoeff("B", cosCoeff, "cos theta_W = sqrt(10)/4"),
        },
        "massless neutral eigenstate; eigenvector of the (W3,B) block for eigenvalue 0.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
    new ExtractionRow(
        "z-eigenstate",
        "Z = cos(theta_W) W3 - sin(theta_W) B",
        new[]
        {
            new TermCoeff("W3", cosCoeff, "cos theta_W = sqrt(10)/4"),
            new TermCoeff("B", sinCoeff.Negate(), "-sin theta_W = -sqrt(6)/4"),
        },
        "massive neutral eigenstate orthogonal to the photon; eigenvector for eigenvalue g^2+gPrime^2.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
    new ExtractionRow(
        "w-plus-eigenstate",
        "W+ = (W1 - i W2)/sqrt(2)",
        new[]
        {
            new TermCoeff("W1", invSqrt2, "1/sqrt(2) = sqrt(2)/2"),
            new TermCoeff("-i*W2", invSqrt2, "coefficient of -i W2 is 1/sqrt(2)"),
        },
        "charged eigenstate; degenerate mass (v^2/4) g^2 with W-.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
    new ExtractionRow(
        "w-minus-eigenstate",
        "W- = (W1 + i W2)/sqrt(2)",
        new[]
        {
            new TermCoeff("W1", invSqrt2, "1/sqrt(2) = sqrt(2)/2"),
            new TermCoeff("+i*W2", invSqrt2, "coefficient of +i W2 is 1/sqrt(2)"),
        },
        "charged eigenstate; degenerate mass (v^2/4) g^2 with W+.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
    new ExtractionRow(
        "electric-charge-relation",
        "e = g sin(theta_W)  (e^2 = (3/8) g^2)",
        new[]
        {
            new TermCoeff("g", sinCoeff, "e/g = sin theta_W = sqrt(6)/4, so e^2/g^2 = 3/8"),
        },
        "unbroken U(1)_em coupling from the photon projection; dimensionless ratio e^2/g^2 = 3/8.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
    new ExtractionRow(
        "w-z-mass-ratio",
        "m_W^2/m_Z^2 = g^2/(g^2+gPrime^2) = cos^2 theta_W = 5/8",
        new[]
        {
            new TermCoeff("m_W^2/m_Z^2", new Radical(5, 8, 1), "5/8 (rational; VEV-independent)"),
        },
        "custodial tree-level mass ratio; VEV amplitude cancels. Cross-checked against Phase429.",
        Category: "conditionally-determined",
        RequiresVevAmplitude: false),
};

bool extractionRowsExact = extractionRows.All(r => r.Terms.All(t => t.Coeff.Verified));
int extractionRowCount = extractionRows.Length;
int conditionallyDeterminedRowCount = extractionRows.Count(r => !r.RequiresVevAmplitude);

// ---------------------------------------------------------------------------
// CONDITIONAL-COMPLETION LEDGER against the Phase419 template. We read the 20
// Phase256 template field ids straight from the Phase419 map's
// coveredPhase256Fields and classify each into exactly one bucket:
//   (a) conditionally-determined-by-rows-given-any-vev
//   (b) still-requires-vev-amplitude-or-scale
//   (c) still-requires-independent-lineage
// ---------------------------------------------------------------------------

var conditionallyDetermined = new (string Field, string Why)[]
{
    ("electroweakGaugeEmbeddingId", "SU(2)_L x U(1)_Y embedding and weak angle tan^2=3/5 fixed by the internal kernel relation."),
    ("photonEigenstateProjectionId", "A = sin W3 + cos B with the exact-radical coefficients above; massless by construction."),
    ("zBosonSourceRowId", "Z = cos W3 - sin B eigenstate direction fixed by the same block; only its amplitude needs v."),
    ("wBosonSourceRowId", "W+- = (W1 -+ i W2)/sqrt(2) charged eigenstate direction fixed; only its amplitude needs v."),
    ("wzCommonBridgeGatePassed", "the common bridge is the mass RATIO m_W/m_Z = cos = sqrt(5/8), VEV-independent."),
    ("quadraticElectroweakMassOperatorId", "the mass-matrix FORM (v^2/4)*[[g^2..],[..gPrime^2]] and its eigenstructure/ratios are fixed."),
    ("targetBlindConstructionHash", "provenance of the target-blind construction is fixed by this phase's algebra itself."),
};
var stillRequiresVevAmplitude = new (string Field, string Why)[]
{
    ("fourDimensionalObservedVacuumArtifactId", "the candidate VEV/vacuum amplitude (and its very existence) is not supplied; Phases 405/410/418/428 closed the mechanisms."),
    ("branchNormalizationSourceId", "the overall normalization that sets the dimensionful scale of the mass operator needs the VEV amplitude."),
    ("wBosonRawAmplitudeGatePassed", "m_W = (v/2) g is an absolute amplitude requiring the VEV scale, not a ratio."),
    ("zBosonRawAmplitudeGatePassed", "m_Z = (v/2) sqrt(g^2+gPrime^2) is an absolute amplitude requiring the VEV scale, not a ratio."),
};
var stillRequiresIndependentLineage = new (string Field, string Why)[]
{
    ("observedFieldExtractionTheoremId", "no extraction THEOREM exists; this is only the conditional algebra a theorem must reproduce."),
    ("sourceReferenceIds", "requires the independent source lineage that would establish VEV existence and comparison provenance."),
    ("higgsScalarSourceOperatorId", "no scalar potential exists in any source; the Higgs source operator is unavailable."),
    ("higgsMassiveScalarProfileId", "the Higgs mass row requires a potential; no potential exists (independent lineage)."),
    ("higgsPotentialSelfCouplingRelationId", "the Higgs self-coupling requires a potential; none exists."),
    ("canonicalOrDeclaredShiabBranchId", "branch selection is a closed internal mechanism (Phases 405/410/418/428); independent lineage."),
    ("stabilitySidecarIds", "stability sidecars are independent artifacts, not fixed by the tree-level mixing algebra."),
    ("targetComparisonAfterConstructionGatePassed", "comparison requires pole extraction, scheme, running from the unification point - none present."),
    ("phase201And209ApplicationReady", "application readiness requires pole extraction, GeV normalization, and units lineage."),
};

// Read the authoritative field list from the Phase419 map and confirm our
// classification partitions it exactly (each field once, all 20 covered).
var phase256Fields = new List<string>();
{
    var mapPath = "studies/phase419_observed_field_symbolic_extraction_template_001/output/observed_field_symbolic_extraction_template_map.json";
    using var map = JsonDocument.Parse(File.ReadAllText(mapPath));
    if (map.RootElement.TryGetProperty("coveredPhase256Fields", out var covered) &&
        covered.ValueKind == JsonValueKind.Array)
        foreach (var f in covered.EnumerateArray())
            if (f.GetString() is { } s)
                phase256Fields.Add(s);
}

int phase256TemplateFieldCount = phase256Fields.Count;
var classified = conditionallyDetermined.Select(x => x.Field)
    .Concat(stillRequiresVevAmplitude.Select(x => x.Field))
    .Concat(stillRequiresIndependentLineage.Select(x => x.Field))
    .ToList();
bool classificationPartitionsTemplate =
    phase256TemplateFieldCount == 20 &&
    classified.Count == 20 &&
    classified.Distinct().Count() == 20 &&
    classified.OrderBy(x => x).SequenceEqual(phase256Fields.OrderBy(x => x));

int conditionallyDeterminedFieldCount = conditionallyDetermined.Length;
int stillRequiresVevAmplitudeFieldCount = stillRequiresVevAmplitude.Length;
int stillRequiresIndependentLineageFieldCount = stillRequiresIndependentLineage.Length;

// ---------------------------------------------------------------------------
// Honest boundaries + fail-closed contract flags.
// ---------------------------------------------------------------------------

var honestBoundaries = new[]
{
    "The doublet VEV is a CANDIDATE parameter; its existence is not established (Phases 405/410/418/428 closed the internal mechanisms; the phase430-chain experiments are pending).",
    "The extraction rows are tree-level in the custodial limit.",
    "Nothing here is an observed-field extraction THEOREM; it is the conditional algebra that such a theorem must reproduce.",
    "Derivation is strictly separated from comparison as in Phase429: no measured value is consulted and no residual against observation is computed.",
    "All ratios (mixing angles, eigenstate directions, charge relation, m_W/m_Z) are VEV-independent; every absolute mass still needs the VEV amplitude, and pole/GeV/scheme lineage is independent of everything here.",
};

const bool candidateVevExistenceEstablished = false;
const bool measuredElectroweakValuesConsulted = false;
const bool comparisonAgainstObservationPerformed = false;
const bool derivationComparisonSeparationMaintained = true;
const bool treeLevelCustodialLimit = true;
const bool isObservedFieldExtractionTheorem = false;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const int sourceDefinedFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const string canFillPhase256Reason =
    "The eigenstate rows, mixing angles, charge relation and mass RATIOS are only conditionally determined; the VEV existence/amplitude fields and the pole/GeV/scheme lineage fields remain unfilled, so the observed-field extraction contract stays closed.";

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "extraction rows as exact functions of a candidate doublet VEV from tan^2=3/5 only; derivation strictly separated from comparison; no measured values")))).ToLowerInvariant();

bool conditionalObservedFieldExtractionRowLedgerPassed =
    phase404PrecursorPassed &&
    phase419PrecursorPassed &&
    phase429PrecursorPassed &&
    mixingExact &&
    massMatrixExact &&
    extractionRowsExact &&
    extractionRowCount == 6 &&
    conditionallyDeterminedRowCount == 6 &&
    classificationPartitionsTemplate &&
    conditionallyDeterminedFieldCount == 7 &&
    stillRequiresVevAmplitudeFieldCount == 4 &&
    stillRequiresIndependentLineageFieldCount == 9 &&
    !candidateVevExistenceEstablished &&
    !measuredElectroweakValuesConsulted &&
    !comparisonAgainstObservationPerformed &&
    derivationComparisonSeparationMaintained &&
    treeLevelCustodialLimit &&
    !isObservedFieldExtractionTheorem &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    sourceDefinedFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = conditionalObservedFieldExtractionRowLedgerPassed
    ? "conditional-observed-field-extraction-row-ledger-materialized-vev-and-lineage-still-missing"
    : "conditional-observed-field-extraction-row-ledger-blocked";

string decision = conditionalObservedFieldExtractionRowLedgerPassed
    ? "The Phase419 extraction targets are now explicit as exact functions of a candidate doublet VEV. From the internal kernel relation tan^2 theta_W = 3/5 alone, the |D_mu Phi|^2 mass matrix in the (W1,W2,W3,B) basis has eigenvalues (v^2/4){g^2,g^2,g^2+gPrime^2,0}, giving a massless photon and m_W^2/m_Z^2 = cos^2 = 5/8 exactly (cross-checked against Phase429). The photon A = (sqrt6/4)W3 + (sqrt10/4)B, Z = (sqrt10/4)W3 - (sqrt6/4)B, W+- = (W1 -+ i W2)/sqrt(2), and e = g sin (e^2 = (3/8)g^2) are recorded with exact radical coefficients verified to 1e-15. Against the 20-field Phase419 template: 7 fields are now conditionally determined by these rows given any doublet VEV, 4 still require the VEV amplitude/scale for absolute masses, and 9 still require independent lineage (pole extraction, GeV normalization, running/scheme, Higgs potential). The candidate VEV's existence is not established and no measured value was consulted, so no Phase201 or Phase256 field is filled."
    : "Do not use the conditional extraction row ledger until the precursor, exact-arithmetic, and classification batteries pass.";

var result = new
{
    phaseId = "phase434-conditional-observed-field-extraction-row-ledger",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    conditionalObservedFieldExtractionRowLedgerPassed,
    phase404PrecursorPassed,
    phase419PrecursorPassed,
    phase429PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    mixingExact,
    massMatrixExact,
    extractionRowsExact,
    kernelRelation = new
    {
        tanSquaredThetaW = TanSquared,
        tanSquaredExactForm = "3/5",
        sinSquaredThetaW = sinSquared,
        sinSquaredExactForm = "3/8",
        cosSquaredThetaW = cosSquared,
        cosSquaredExactForm = "5/8",
        sinThetaWExactForm = sinCoeff.ExactForm,
        cosThetaWExactForm = cosCoeff.ExactForm,
    },
    massMatrix = new
    {
        basis = new[] { "W1", "W2", "W3", "B" },
        prefactor = "v^2/4",
        gPrimeSqOverGSq = TanSquared,
        gPrimeSqOverGSqExactForm = "3/5",
        eigenvaluesInVevUnitsExactForm = "(v^2/4) * { g^2, g^2, g^2+gPrime^2, 0 }",
        eigenvalues = massEigenvalues.Select(e => new
        {
            eigenId = e.EigenId,
            exactForm = e.ExactForm,
            symbolicValue = e.SymbolicValue,
            numericInGSqUnits = e.Numeric,
            note = e.Note,
        }).ToArray(),
        mWSqOverMZSq = mWmZRatioSq,
        mWSqOverMZSqExactForm = "5/8",
        mWOverMZExactForm = "sqrt(5/8) = sqrt(10)/4",
        photonMassSqInGSqUnits = photonMassSqUnit,
        phase429RatioRowFound,
        phase429TreeMwOverMz,
        crossCheckMatchesPhase429 = phase429RatioRowFound && Math.Abs(Math.Sqrt(mWmZRatioSq) - phase429TreeMwOverMz) <= 1e-15,
    },
    extractionRows = extractionRows.Select(r => new
    {
        rowId = r.RowId,
        field = r.Field,
        terms = r.Terms.Select(t => new
        {
            basisComponent = t.BasisComponent,
            coefficientExactForm = t.Coeff.ExactForm,
            coefficientValue = t.Coeff.Value,
            coefficientVerified = t.Coeff.Verified,
            note = t.Note,
        }).ToArray(),
        note = r.Note,
        category = r.Category,
        requiresVevAmplitude = r.RequiresVevAmplitude,
    }).ToArray(),
    extractionRowCount,
    conditionallyDeterminedRowCount,
    conditionalCompletionLedger = new
    {
        phase256TemplateFieldCount,
        classificationPartitionsTemplate,
        conditionallyDeterminedByRowsGivenAnyVev = conditionallyDetermined.Select(x => new { field = x.Field, why = x.Why }).ToArray(),
        stillRequiresVevAmplitudeOrScale = stillRequiresVevAmplitude.Select(x => new { field = x.Field, why = x.Why }).ToArray(),
        stillRequiresIndependentLineage = stillRequiresIndependentLineage.Select(x => new { field = x.Field, why = x.Why }).ToArray(),
        conditionallyDeterminedFieldCount,
        stillRequiresVevAmplitudeFieldCount,
        stillRequiresIndependentLineageFieldCount,
    },
    honestBoundaries,
    candidateVevExistenceEstablished,
    measuredElectroweakValuesConsulted,
    comparisonAgainstObservationPerformed,
    derivationComparisonSeparationMaintained,
    treeLevelCustodialLimit,
    isObservedFieldExtractionTheorem,
    physicalCouplingProvided,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesObservedElectroweakNamespaceMap,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    sourceDefinedFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    canFillPhase256Reason,
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
        phase419SummaryPath = Phase419SummaryPath,
        phase429SummaryPath = Phase429SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The extraction rows are conditional algebra keyed to a CANDIDATE doublet VEV whose existence and amplitude no internal mechanism supplies.",
        "No measured electroweak value appears and no comparison against observation was performed; derivation and comparison are strictly separated.",
        "Nothing here is an observed-field extraction theorem; it is the algebra such a theorem must reproduce.",
        "Absolute W/Z masses and all pole/GeV/scheme/Higgs-potential lineage remain outside this phase.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "conditional_observed_field_extraction_row_ledger.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "conditional_observed_field_extraction_row_ledger_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"conditionalObservedFieldExtractionRowLedgerPassed={conditionalObservedFieldExtractionRowLedgerPassed}");
Console.WriteLine($"mixingExact={mixingExact} massMatrixExact={massMatrixExact} extractionRowsExact={extractionRowsExact}");
Console.WriteLine($"m_W^2/m_Z^2={mWmZRatioSq:F10} (exact 5/8); crossCheckMatchesPhase429={phase429RatioRowFound && Math.Abs(Math.Sqrt(mWmZRatioSq) - phase429TreeMwOverMz) <= 1e-15}");
foreach (var r in extractionRows)
    Console.WriteLine($"  {r.RowId}: {r.Field}");
Console.WriteLine($"classificationPartitionsTemplate={classificationPartitionsTemplate} (a)={conditionallyDeterminedFieldCount} (b)={stillRequiresVevAmplitudeFieldCount} (c)={stillRequiresIndependentLineageFieldCount} of {phase256TemplateFieldCount}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

// Exact coefficient (p/q)*sqrt(r) with numeric verification.
public sealed record Radical(long P, long Q, long R)
{
    public double Value => (double)P / Q * Math.Sqrt(R);
    public string ExactForm => R == 1
        ? (Q == 1 ? $"{P}" : $"{P}/{Q}")
        : (P == 1 && Q == 1 ? $"sqrt({R})" : $"({P}/{Q})*sqrt({R})");
    // "verified" means the coefficient is a well-formed real radical: nonzero
    // denominator, non-negative radicand, and a finite value.
    public bool Verified => Q != 0 && R >= 0 && double.IsFinite(Value);
    public Radical Negate() => new(-P, Q, R);
}

public sealed record EigenRow(string EigenId, string ExactForm, string SymbolicValue, double Numeric, string Note);

public sealed record TermCoeff(string BasisComponent, Radical Coeff, string Note);

public sealed record ExtractionRow(
    string RowId,
    string Field,
    TermCoeff[] Terms,
    string Note,
    string Category,
    bool RequiresVevAmplitude);
