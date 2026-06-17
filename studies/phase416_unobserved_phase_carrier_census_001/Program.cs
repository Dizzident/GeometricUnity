using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase416: unobserved-phase carrier census.
//
// The restart prompt's next branch after Phase415 is the draft's unobserved or
// "dark" phase content. This phase reads the primary-source carrier statements
// as a representation census, not a boson-prediction source. The key question
// is narrow and fail-closed:
//
//   Which unobserved/dark carriers are actually pinned by source text, and does
//   any source-pinned carrier already define a linear bosonic spin-zero
//   SM-doublet carrier or observed photon/W/Z/H projection row?
//
// The answer below is intentionally conservative. The draft does pin dark
// fermionic sectors in the F/Q/Z decomposition, including the vector-spinor
// 144 remainder, but it does not define a bosonic spin-zero map, action, VEV
// selection, observed-field projection, weak-angle lineage, pole prescription,
// or GeV/unit normalization. That makes Q/Z useful next mathematical carriers,
// not promotion evidence.

const string DefaultOutputDir = "studies/phase416_unobserved_phase_carrier_census_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase407SummaryPath = "studies/phase407_chimeric_adjoint_sm_content_probe_001/output/chimeric_adjoint_sm_content_probe_summary.json";
const string Phase408SummaryPath = "studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/output/vertical_spin_zero_extraction_obstruction_probe_summary.json";
const string Phase409SummaryPath = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output/invariant_pairing_menu_spin_zero_extraction_probe_summary.json";
const string Phase412SummaryPath = "studies/phase412_quartic_sm_doublet_intersection_analysis_001/output/quartic_sm_doublet_intersection_analysis_summary.json";
const string Phase414SummaryPath = "studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001/output/general_shiab_epsilon_operator_ansatz_probe_summary.json";
const string Phase415SummaryPath = "studies/phase415_fermionic_cohomology_square_root_ansatz_probe_001/output/fermionic_cohomology_square_root_ansatz_probe_summary.json";
const string ApplicationSubjectKind = "unobserved-phase-carrier-census";

var outputDir = Environment.GetEnvironmentVariable("PHASE416_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var phase407 = JsonDocument.Parse(File.ReadAllText(Phase407SummaryPath));
using var phase408 = JsonDocument.Parse(File.ReadAllText(Phase408SummaryPath));
using var phase409 = JsonDocument.Parse(File.ReadAllText(Phase409SummaryPath));
using var phase412 = JsonDocument.Parse(File.ReadAllText(Phase412SummaryPath));
using var phase414 = JsonDocument.Parse(File.ReadAllText(Phase414SummaryPath));
using var phase415 = JsonDocument.Parse(File.ReadAllText(Phase415SummaryPath));

bool phase404PrecursorPassed =
    JsonBool(phase404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true &&
    JsonBool(phase404.RootElement, "spinor16IsSixteenDimensional") is true &&
    JsonBool(phase404.RootElement, "familyPatternDerived") is true &&
    JsonBool(phase404.RootElement, "scalarDoubletMustComeFromNonAdjointConnectionComponents") is true;

bool phase407PrecursorPassed =
    JsonBool(phase407.RootElement, "chimericAdjointSmContentProbePassed") is true &&
    JsonBool(phase407.RootElement, "higgsPatternDoubletExistsInChimericAdjoint") is true &&
    JsonBool(phase407.RootElement, "higgsPatternCarriesSpacetimeVectorIndex") is true &&
    JsonBool(phase407.RootElement, "spinZeroExtractionPerformed") is false;

bool phase408PrecursorPassed =
    JsonBool(phase408.RootElement, "verticalSpinZeroExtractionObstructionProbePassed") is true &&
    JsonBool(phase408.RootElement, "spinZeroSlotCannotCarryFullDoublet") is true &&
    JsonBool(phase408.RootElement, "centralizerIsTrivial") is true;

bool phase409PrecursorPassed =
    JsonBool(phase409.RootElement, "invariantPairingMenuSpinZeroExtractionProbePassed") is true &&
    JsonBool(phase409.RootElement, "linearSpinZeroContentIsZero") is true &&
    JsonBool(phase409.RootElement, "bilinearSpinZeroDoubletAbsent") is true &&
    JsonBool(phase409.RootElement, "obstructionMenuCompleteThroughBilinearOrder") is true;

bool phase412PrecursorPassed =
    JsonBool(phase412.RootElement, "quarticSmDoubletIntersectionAnalysisPassed") is true &&
    JsonBool(phase412.RootElement, "quarticWeldedScalarSmDoubletAbsentAllChannels") is true;

bool phase414PrecursorPassed =
    JsonBool(phase414.RootElement, "generalShiabEpsilonOperatorAnsatzProbePassed") is true &&
    JsonBool(phase414.RootElement, "unobservedPhaseStillRequiredForFurtherProgress") is true &&
    JsonBool(phase414.RootElement, "anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet") is false;

bool phase415PrecursorPassed =
    JsonBool(phase415.RootElement, "fermionicCohomologySquareRootAnsatzProbePassed") is true &&
    JsonBool(phase415.RootElement, "unobservedPhaseStillRequiresCarrier") is true &&
    JsonInt(phase415.RootElement, "candidateComplexesWithWeldedScalarSmDoubletCount") == 0;

var sourceEvidence = new[]
{
    new SourceEvidence(
        "GU-DRAFT-2021 section 9.3, eqs. 9.16-9.20",
        "chi contains observed fermions plus LookingGlass, dark spinorial, Rarita-Schwinger, and other matter; omega subfields carry Higgs-like soft masses, Yukawa, CKM, and gauge potentials; no observed boson rows are defined."),
    new SourceEvidence(
        "GU-DRAFT-2021 section 11.2, eq. 11.6",
        "zeta decomposes into F, Q, and Z pieces with dimensions F=64, Q=192, Z=576 per displayed side; Q/Z sectors above the dashed line are described as currently dark."),
    new SourceEvidence(
        "GU-DRAFT-2021 section 12.9, eq. 12.20",
        "the opposite Weyl half is described as dark decoupled Looking Glass matter."),
    new SourceEvidence(
        "TOE-GU-ICEBERG transcript 02:51-02:53",
        "secondary explainer says there is a remainder not experimentally observed and asks about unobserved decoupled sectors; this is not primary promotion evidence."),
};

const int observedFDimensionPerZetaSide = 64;
const int qRaritaSchwingerDimensionPerZetaSide = 192; // (6 x 16+) + (6 x 16-)
const int zVectorSpinorDimensionPerZetaSide = 576;    // (2 x 144+) + (2 x 144-)
const int zetaSideDimension = 832;
bool draftZetaDecompositionDimensionCheck =
    observedFDimensionPerZetaSide + qRaritaSchwingerDimensionPerZetaSide + zVectorSpinorDimensionPerZetaSide == zetaSideDimension;

var carrierSlots = new[]
{
    new CarrierSlot(
        "observed-f-sector-64",
        "F_{1/2}: (2 x 16+) + (2 x 16-) per zeta side",
        "GU-DRAFT-2021 eq. 11.6",
        "observed-fermion-sector-control",
        true,
        false,
        true,
        observedFDimensionPerZetaSide,
        true,
        false,
        false,
        false,
        "phase411/phase412",
        "closed-control",
        "This is the already-probed observed spinor carrier family. Phases411/412 close the bilinear and quartic spinor-composite route on the 16; it is not an unobserved bosonic carrier."),
    new CarrierSlot(
        "q-dark-rarita-schwinger-spin-3-2-sector",
        "Q_{3/2}: (6 x 16+) + (6 x 16-) per zeta side",
        "GU-DRAFT-2021 eq. 11.6",
        "source-pinned-dark-fermionic-vector-spin-sector",
        true,
        true,
        true,
        qRaritaSchwingerDimensionPerZetaSide,
        true,
        false,
        false,
        true,
        "none-current",
        "open-composite-or-map-required",
        "The draft pins a computable dark Rarita-Schwinger-like carrier, but it is fermionic/spin-3/2 at the linear level. A bosonic spin-zero SM doublet would require an even composite or a source-defined bosonic projection map not supplied here."),
    new CarrierSlot(
        "z-dark-vector-spinor-144-sector",
        "Z_{1/2}: (2 x 144+) + (2 x 144-) per zeta side, with 144 the vector-spinor remainder in 10 x 16 = 16 + 144",
        "GU-DRAFT-2021 eqs. 11.3, 11.6, 12.22",
        "source-pinned-dark-vector-spinor-remainder",
        true,
        true,
        true,
        zVectorSpinorDimensionPerZetaSide,
        true,
        false,
        false,
        true,
        "none-current",
        "open-144-decomposition-and-composite-action-required",
        "This is the most concrete unobserved-phase carrier left by the source. Its representation class is pinned, but the repo has not yet materialized the SM/welded decomposition of the 144 or a bosonic action/projection turning it into a scalar VEV carrier."),
    new CarrierSlot(
        "dark-decoupled-looking-glass-weyl-half",
        "opposite Weyl-half 64 in the chirality toy decomposition",
        "GU-DRAFT-2021 eq. 12.20",
        "source-pinned-dark-decoupled-mirror-fermion-sector",
        true,
        true,
        true,
        64,
        true,
        false,
        false,
        true,
        "phase411/phase412-by-mirror",
        "closed-linear-open-interaction",
        "The source pins a dark mirror fermion sector, but it is another spinorial sector. Mirror copying the 16 does not create a linear bosonic spin-zero doublet; any bosonic effect needs a new interaction/composite map."),
    new CarrierSlot(
        "chimeric-frame-cross-internal-4x10",
        "frame-cross internal block in the chimeric adjoint: 4 x 10",
        "Phase407 source-backed branching from GU connection geometry",
        "beyond-frame-cross-connection-sector",
        true,
        false,
        true,
        40,
        false,
        false,
        false,
        false,
        "phase407/phase408/phase409/phase412",
        "closed-on-probed-spin-zero-extraction",
        "Phase407 found SM-Higgs-pattern doublets here, but they carry a spacetime-vector index. Phases408/409/412 close the naive, bilinear, epsilon, spinor-bilinear, and quartic spin-zero extraction routes on probed carriers."),
    new CarrierSlot(
        "generic-unobserved-decoupled-sector-language",
        "unobserved/decoupled/dark sectors described qualitatively outside a carrier equation",
        "TOE-GU-ICEBERG transcript open-question language",
        "secondary-qualitative-unpinned-sector",
        false,
        true,
        false,
        null,
        false,
        false,
        false,
        true,
        "none",
        "source-insufficient",
        "The secondary transcript is useful search context, but without a primary carrier/action it cannot define a computable boson source row."),
};

int carrierSlotCount = carrierSlots.Length;
int sourcePinnedCarrierCount = carrierSlots.Count(c => c.SourcePinnedRepresentation);
int sourcePinnedUnobservedCarrierCount = carrierSlots.Count(c => c.SourcePinnedRepresentation && c.SourceClassifiesAsDarkOrUnobserved);
int computableUnobservedCarrierCount = carrierSlots.Count(c => c.SourcePinnedRepresentation && c.SourceClassifiesAsDarkOrUnobserved && c.ComputableRepresentation);
int linearBosonicSpinZeroCandidateCount = carrierSlots.Count(c => c.LinearBosonicSpinZeroCandidate);
int sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount = carrierSlots.Count(c =>
    c.SourcePinnedRepresentation && c.SourceClassifiesAsDarkOrUnobserved && c.LinearBosonicSpinZeroCandidate);
int sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount = carrierSlots.Count(c =>
    c.SourcePinnedRepresentation && c.SourceClassifiesAsDarkOrUnobserved && c.WeldedScalarSmDoubletAtLinearLevel);
int sourcePinnedUnobservedCarriersRequiringCompositeOrBosonicMapCount = carrierSlots.Count(c =>
    c.SourcePinnedRepresentation && c.SourceClassifiesAsDarkOrUnobserved && c.RequiresCompositeOrBosonicMap);
int currentSourceOpenCarrierCount = carrierSlots.Count(c => c.Status.StartsWith("open", StringComparison.Ordinal));
int sourceInsufficientCarrierCount = carrierSlots.Count(c => !c.SourcePinnedRepresentation);

bool sourcePinnedUnobservedCarriersMaterialized =
    sourcePinnedUnobservedCarrierCount == 3 &&
    computableUnobservedCarrierCount == 3 &&
    draftZetaDecompositionDimensionCheck;
bool unobservedCarrierCensusCompleteForCurrentSources =
    sourcePinnedUnobservedCarriersMaterialized &&
    sourceInsufficientCarrierCount == 1;
bool noLinearBosonicSpinZeroCarrierInPinnedUnobservedSectors =
    sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount == 0 &&
    sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount == 0;

const bool sourceDefinesUnobservedBosonicVevCarrier = false;
const bool sourceDefinesUnobservedCarrierAction = false;
const bool sourceDefinesUnobservedSpinZeroExtractionMap = false;
const bool sourceDefinesUnobservedObservedBosonProjectionRows = false;
const bool sourceDefinesUnobservedWeakAngleScalePoleOrGevLineage = false;
bool unobservedPhaseStillRequiresBosonicMap =
    !sourceDefinesUnobservedBosonicVevCarrier ||
    !sourceDefinesUnobservedCarrierAction ||
    !sourceDefinesUnobservedSpinZeroExtractionMap ||
    !sourceDefinesUnobservedObservedBosonProjectionRows;
bool vectorSpinor144RemainsConcreteNextCarrier =
    carrierSlots.Any(c => c.Id == "z-dark-vector-spinor-144-sector" && c.Status.StartsWith("open", StringComparison.Ordinal));

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "\n",
    carrierSlots.Select(c =>
        $"{c.Id}|{c.SourceRepresentation}|{c.SourceLocation}|{c.SourceClass}|{c.SourcePinnedRepresentation}|{c.SourceClassifiesAsDarkOrUnobserved}|{c.ComputableRepresentation}|{c.Dimension}|{c.LinearFermionicCarrier}|{c.LinearBosonicSpinZeroCandidate}|{c.WeldedScalarSmDoubletAtLinearLevel}|{c.RequiresCompositeOrBosonicMap}|{c.Status}"))))).ToLowerInvariant();

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

bool unobservedPhaseCarrierCensusPassed =
    phase404PrecursorPassed &&
    phase407PrecursorPassed &&
    phase408PrecursorPassed &&
    phase409PrecursorPassed &&
    phase412PrecursorPassed &&
    phase414PrecursorPassed &&
    phase415PrecursorPassed &&
    unobservedCarrierCensusCompleteForCurrentSources &&
    noLinearBosonicSpinZeroCarrierInPinnedUnobservedSectors &&
    unobservedPhaseStillRequiresBosonicMap &&
    vectorSpinor144RemainsConcreteNextCarrier &&
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

string terminalStatus = unobservedPhaseCarrierCensusPassed
    ? "unobserved-phase-carriers-censused-no-linear-bosonic-sm-doublet-map"
    : "unobserved-phase-carrier-census-review-required";

string decision = unobservedPhaseCarrierCensusPassed
    ? "The source-pinned unobserved/dark carriers have now been materialized as a fail-closed census. The primary draft pins three computable unobserved fermionic carriers: Q_{3/2} (192 states per zeta side), Z_{1/2} with the vector-spinor 144 remainder (576 states per zeta side), and the dark decoupled Weyl-half/mirror sector (64 states). None is a linear bosonic spin-zero SM-doublet carrier, and no source supplies a bosonic map/action/VEV/projection that would turn them into one. The most concrete next mathematical carrier is the Z/vector-spinor 144 decomposition and any source-defined even-composite or bosonic projection built from it. No Phase201 or Phase256 field is filled."
    : "Do not use the unobserved carrier census until all precursor and fail-closed accounting checks pass.";

var result = new
{
    phaseId = "phase416-unobserved-phase-carrier-census",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    unobservedPhaseCarrierCensusPassed,
    phase404PrecursorPassed,
    phase407PrecursorPassed,
    phase408PrecursorPassed,
    phase409PrecursorPassed,
    phase412PrecursorPassed,
    phase414PrecursorPassed,
    phase415PrecursorPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind = ApplicationSubjectKind,
    observedFDimensionPerZetaSide,
    qRaritaSchwingerDimensionPerZetaSide,
    zVectorSpinorDimensionPerZetaSide,
    zetaSideDimension,
    draftZetaDecompositionDimensionCheck,
    carrierSlotCount,
    sourcePinnedCarrierCount,
    sourcePinnedUnobservedCarrierCount,
    computableUnobservedCarrierCount,
    linearBosonicSpinZeroCandidateCount,
    sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount,
    sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount,
    sourcePinnedUnobservedCarriersRequiringCompositeOrBosonicMapCount,
    currentSourceOpenCarrierCount,
    sourceInsufficientCarrierCount,
    sourcePinnedUnobservedCarriersMaterialized,
    unobservedCarrierCensusCompleteForCurrentSources,
    noLinearBosonicSpinZeroCarrierInPinnedUnobservedSectors,
    sourceDefinesUnobservedBosonicVevCarrier,
    sourceDefinesUnobservedCarrierAction,
    sourceDefinesUnobservedSpinZeroExtractionMap,
    sourceDefinesUnobservedObservedBosonProjectionRows,
    sourceDefinesUnobservedWeakAngleScalePoleOrGevLineage,
    unobservedPhaseStillRequiresBosonicMap,
    vectorSpinor144RemainsConcreteNextCarrier,
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
    sourceEvidence,
    carrierSlots = carrierSlots.Select(c => c.ToOutput()).ToArray(),
    explicitCandidateOnlyNonclaims = new[]
    {
        "Q and Z are source-pinned dark fermionic carriers, not linear bosonic VEV/scalar carriers.",
        "The vector-spinor 144 is a concrete next representation target, but the current phase does not claim its even-composite or bosonic projection content.",
        "The chimeric frame-cross 4x10 positive doublet remains covered by Phases407-412: it carries a spacetime-vector index and no probed spin-zero extraction succeeds.",
        "No observed photon/W/Z/H projection rows, weak-angle lineage, VEV scale, pole extraction, or GeV normalization are supplied.",
        "No Phase201 or Phase256 fill.",
    },
    sourcePaths = new
    {
        guDraftText = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
        superphysicsMirrorNote = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md",
        toeIcebergGapAnalysis = "docs/Reference/ExperimentReferences/TOE-GU-ICEBERG-20250423-GAP-ANALYSIS.md",
        phase404SummaryPath = Phase404SummaryPath,
        phase407SummaryPath = Phase407SummaryPath,
        phase408SummaryPath = Phase408SummaryPath,
        phase409SummaryPath = Phase409SummaryPath,
        phase412SummaryPath = Phase412SummaryPath,
        phase414SummaryPath = Phase414SummaryPath,
        phase415SummaryPath = Phase415SummaryPath,
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "unobserved_phase_carrier_census.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "unobserved_phase_carrier_census_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"unobservedPhaseCarrierCensusPassed={unobservedPhaseCarrierCensusPassed}");
Console.WriteLine($"sourcePinnedUnobservedCarrierCount={sourcePinnedUnobservedCarrierCount}");
Console.WriteLine($"computableUnobservedCarrierCount={computableUnobservedCarrierCount}");
Console.WriteLine($"draftZetaDecompositionDimensionCheck={draftZetaDecompositionDimensionCheck} ({observedFDimensionPerZetaSide}+{qRaritaSchwingerDimensionPerZetaSide}+{zVectorSpinorDimensionPerZetaSide}={zetaSideDimension})");
Console.WriteLine($"sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount={sourcePinnedUnobservedLinearBosonicSpinZeroCandidateCount}");
Console.WriteLine($"sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount={sourcePinnedUnobservedWeldedScalarSmDoubletCandidateCount}");
Console.WriteLine($"unobservedPhaseStillRequiresBosonicMap={unobservedPhaseStillRequiresBosonicMap}");
Console.WriteLine($"vectorSpinor144RemainsConcreteNextCarrier={vectorSpinor144RemainsConcreteNextCarrier}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
        return null;
    return property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
        return null;
    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value)
        ? value
        : null;
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record SourceEvidence(string Location, string Finding);

public sealed record CarrierSlot(
    string Id,
    string SourceRepresentation,
    string SourceLocation,
    string SourceClass,
    bool SourcePinnedRepresentation,
    bool SourceClassifiesAsDarkOrUnobserved,
    bool ComputableRepresentation,
    int? Dimension,
    bool LinearFermionicCarrier,
    bool LinearBosonicSpinZeroCandidate,
    bool WeldedScalarSmDoubletAtLinearLevel,
    bool RequiresCompositeOrBosonicMap,
    string CoveredOrRequires,
    string Status,
    string DecisionReason)
{
    public object ToOutput() => new
    {
        id = Id,
        sourceRepresentation = SourceRepresentation,
        sourceLocation = SourceLocation,
        sourceClass = SourceClass,
        sourcePinnedRepresentation = SourcePinnedRepresentation,
        sourceClassifiesAsDarkOrUnobserved = SourceClassifiesAsDarkOrUnobserved,
        computableRepresentation = ComputableRepresentation,
        dimension = Dimension,
        linearFermionicCarrier = LinearFermionicCarrier,
        linearBosonicSpinZeroCandidate = LinearBosonicSpinZeroCandidate,
        weldedScalarSmDoubletAtLinearLevel = WeldedScalarSmDoubletAtLinearLevel,
        requiresCompositeOrBosonicMap = RequiresCompositeOrBosonicMap,
        coveredOrRequires = CoveredOrRequires,
        status = Status,
        decisionReason = DecisionReason,
    };
}
