using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase414: general Shiab/epsilon operator ansatz probe.
//
// Restart prompt directive: construct the broadest low-order invariant menu
// using wedge, Hodge star, contraction, commutator, i-weighted anticommutator,
// Clifford volume, and epsilon conjugation; then test whether any such operator
// can map currently computable GU field content into a welded spin-0 SM doublet.
//
// This phase is deliberately a closure certificate, not a new numerical fit.
// The low-order operator alphabet consists of functorial invariant operations:
// they can only move a candidate through representation modules already
// counted by Phases408-413 unless a new carrier or an explicit new first-order
// operator is supplied. Phase414 therefore enumerates the operator families and
// binds every closed family to its exact upstream no-go certificate. Anything
// outside that envelope remains named as new-source work, not hidden inside a
// near-pass.
//
// Fail-closed: no targets, no dynamics, no scale, no pole extraction, no
// Phase201/Phase256 mutation.

const string DefaultOutputDir = "studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001/output";
const string Phase408SummaryPath = "studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/output/vertical_spin_zero_extraction_obstruction_probe_summary.json";
const string Phase409SummaryPath = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output/invariant_pairing_menu_spin_zero_extraction_probe_summary.json";
const string Phase411SummaryPath = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output/quartic_dirac_squared_spinor_composite_probe_summary.json";
const string Phase412SummaryPath = "studies/phase412_quartic_sm_doublet_intersection_analysis_001/output/quartic_sm_doublet_intersection_analysis_summary.json";
const string Phase413SummaryPath = "studies/phase413_noncompact_real_form_transfer_probe_001/output/noncompact_real_form_transfer_probe_summary.json";
const string ApplicationSubjectKind = "general-shiab-epsilon-operator-ansatz-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE414_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase408 = JsonDocument.Parse(File.ReadAllText(Phase408SummaryPath));
using var phase409 = JsonDocument.Parse(File.ReadAllText(Phase409SummaryPath));
using var phase411 = JsonDocument.Parse(File.ReadAllText(Phase411SummaryPath));
using var phase412 = JsonDocument.Parse(File.ReadAllText(Phase412SummaryPath));
using var phase413 = JsonDocument.Parse(File.ReadAllText(Phase413SummaryPath));

bool phase408PrecursorPassed =
    JsonBool(phase408.RootElement, "verticalSpinZeroExtractionObstructionProbePassed") is true &&
    JsonBool(phase408.RootElement, "spinZeroSlotCannotCarryFullDoublet") is true &&
    JsonBool(phase408.RootElement, "weldEntanglesSpinAndIsospin") is true &&
    JsonBool(phase408.RootElement, "centralizerIsTrivial") is true;

bool phase409PairingMenuComplete = false;
if (phase409.RootElement.TryGetProperty("pairingMenu", out var pairingMenu) &&
    pairingMenu.TryGetProperty("fiber4x4", out var fiber44) &&
    pairingMenu.TryGetProperty("fiber10x10", out var fiber1010) &&
    pairingMenu.TryGetProperty("fiber4x10", out var fiber410))
{
    phase409PairingMenuComplete =
        JsonInt(fiber44, "dimension") == 1 &&
        JsonInt(fiber44, "parityOdd") == 0 &&
        JsonInt(fiber1010, "dimension") == 2 &&
        JsonInt(fiber1010, "parityOdd") == 0 &&
        JsonInt(fiber410, "dimension") == 0;
}

bool phase409PrecursorPassed =
    JsonBool(phase409.RootElement, "invariantPairingMenuSpinZeroExtractionProbePassed") is true &&
    JsonBool(phase409.RootElement, "linearSpinZeroContentIsZero") is true &&
    JsonInt(phase409.RootElement, "linearSingletDimension") == 0 &&
    JsonInt(phase409.RootElement, "bilinearSpinZeroDimension") == 7 &&
    JsonBool(phase409.RootElement, "bilinearSpinZeroDoubletAbsent") is true &&
    JsonBool(phase409.RootElement, "obstructionMenuCompleteThroughBilinearOrder") is true &&
    JsonBool(phase409.RootElement, "oddOrderSpinZeroForbidden") is true &&
    JsonInt(phase409.RootElement, "parityOddInvariantCount") == 1 &&
    JsonInt(phase409.RootElement, "doubletPatternStateCount") == 0 &&
    phase409PairingMenuComplete;

bool phase411PrecursorPassed =
    JsonBool(phase411.RootElement, "quarticDiracSquaredSpinorCompositeProbePassed") is true &&
    JsonBool(phase411.RootElement, "leftRightBilinearChannelHasNoWeldedScalar") is true &&
    JsonBool(phase411.RootElement, "spinorBilinearSpinZeroDoubletAbsent") is true &&
    JsonInt(phase411.RootElement, "majoranaSpinZeroSmDoubletCount") == 0;

bool phase412PrecursorPassed =
    JsonBool(phase412.RootElement, "quarticSmDoubletIntersectionAnalysisPassed") is true &&
    JsonBool(phase412.RootElement, "quarticWeldedScalarSmDoubletAbsentAllChannels") is true &&
    JsonInt(phase412.RootElement, "unionIntersectionRealDimension") == 0;

bool phase413PrecursorPassed =
    JsonBool(phase413.RootElement, "noncompactRealFormTransferProbePassed") is true &&
    JsonBool(phase413.RootElement, "complexifiedWeldsCoincide") is true &&
    JsonBool(phase413.RootElement, "realFormTransferEstablished") is true;

var requiredOperatorAlphabet = new[]
{
    "wedge",
    "hodge-star",
    "contraction",
    "commutator",
    "i-weighted-anticommutator",
    "clifford-volume",
    "epsilon-conjugation",
};

var closedFamilies = new[]
{
    new OperatorFamily(
        "linear-hodge-clifford-volume",
        1,
        ["hodge-star", "clifford-volume"],
        "frame-cross V = 4 x 10",
        "phase409",
        "Linear functorial form operators are welded-spin intertwiners; Phase409 found zero linear spin-zero content in V.",
        true,
        false,
        false),
    new OperatorFamily(
        "linear-epsilon-conjugation",
        1,
        ["epsilon-conjugation", "hodge-star"],
        "frame-cross V = 4 x 10",
        "phase408/phase409",
        "The naive trace slot is one-dimensional and too small, while the full linear frame-cross singlet count is zero.",
        true,
        false,
        false),
    new OperatorFamily(
        "linear-contraction-trace",
        1,
        ["contraction"],
        "vertical Sym^2(4) trace slot",
        "phase408",
        "The only vertical spin-zero slot is the trace direction; it cannot carry a four-real-dimensional SM doublet.",
        true,
        false,
        false),
    new OperatorFamily(
        "mixed-4x10-fiber-pairing",
        2,
        ["contraction", "wedge"],
        "fiber 4 x 10",
        "phase409",
        "The complete invariant fiber-pairing menu has dimension zero on 4 x 10, so there is no mixed fiber pairing for Shiab-style extraction on the probed carrier.",
        true,
        false,
        false),
    new OperatorFamily(
        "metric-even-bilinear-pairings",
        2,
        ["wedge", "hodge-star", "contraction"],
        "V tensor V, parity-even sector",
        "phase409",
        "The complete bilinear spin-zero sector has six parity-even directions after all metric/Hodge/contraction pairings, and its SM-stable subspace contains no doublet.",
        true,
        false,
        false),
    new OperatorFamily(
        "epsilon-built-bilinear-pairing",
        2,
        ["wedge", "epsilon-conjugation"],
        "V tensor V, parity-odd sector",
        "phase409",
        "The epsilon-built parity-odd bilinear sector is exactly one-dimensional and contains no SM-doublet state.",
        true,
        false,
        false),
    new OperatorFamily(
        "commutator-shiab-curvature",
        2,
        ["wedge", "commutator"],
        "Upsilon-like bracket [omega wedge omega]",
        "phase409/phase412",
        "The commutator bracket is a bilinear invariant on the same welded tensor envelope; Phase409 closes bilinear doublets and Phase412 closes quartic composites needed by bracket-squared variants.",
        true,
        false,
        false),
    new OperatorFamily(
        "i-weighted-anticommutator-hermitian",
        2,
        ["i-weighted-anticommutator"],
        "complexified V tensor V",
        "phase409/phase413",
        "Multiplication by i and anticommutator symmetrization change real/parity bookkeeping but not the complexified invariant module; Phase409 found no doublet and Phase413 transfers the no-go across real forms.",
        true,
        false,
        false),
    new OperatorFamily(
        "hodge-wedge-star-f2",
        2,
        ["wedge", "hodge-star"],
        "two-form-valued bilinear residuals",
        "phase409",
        "Hodge-star on the form index is an equivariant post-composition of the bilinear residual; it cannot create an SM doublet absent from the bilinear spin-zero module.",
        true,
        false,
        false),
    new OperatorFamily(
        "quartic-dirac-square-and-bracket-square",
        4,
        ["wedge", "commutator", "clifford-volume"],
        "spinor bilinear/quartic and bracket-squared composites",
        "phase411/phase412",
        "The Dirac-squared bilinear route has no welded scalar in the Yukawa channel, and the ambient quartic intersection with the SM-doublet is zero in every channel.",
        true,
        false,
        false),
    new OperatorFamily(
        "noncompact-shiab-epsilon-real-form",
        4,
        ["epsilon-conjugation", "clifford-volume", "hodge-star"],
        "Lorentzian/noncompact finite-dimensional welded carriers",
        "phase413",
        "The Lorentzian weld complexifies to the compact weld; all complex-linear no-go counts transfer to the noncompact finite-dimensional form.",
        true,
        false,
        false),
};

var openFamilies = new[]
{
    new OperatorFamily(
        "unobserved-phase-carrier",
        null,
        ["epsilon-conjugation", "wedge", "commutator"],
        "draft unobserved-phase fields not pinned to a computable representation",
        "none",
        "Not closed by this operator menu because no explicit carrier/action exists locally or in the reviewed public mirror.",
        false,
        null,
        true),
    new OperatorFamily(
        "fermionic-cohomology-square-root-delta-omega",
        null,
        ["square-root-delta-omega", "commutator", "i-weighted-anticommutator"],
        "Superphysics part-09b cohomology/square-root clue",
        "none",
        "Not closed here because a first-order complex, differential, and cohomology target must be specified before representation counting is meaningful.",
        false,
        null,
        true),
};

var operatorMenu = closedFamilies.Concat(openFamilies).ToArray();
bool requestedOperatorAlphabetCovered = requiredOperatorAlphabet.All(required =>
    operatorMenu.Any(f => f.Operations.Contains(required, StringComparer.Ordinal)));
bool closedFamiliesAllDischarged = closedFamilies.All(f =>
    f.ClosedByExistingProof && f.ProducesWeldedScalarSmDoublet is false && !f.RequiresNewSourceSpecification);
bool lowOrderProbedCarrierFamiliesClosed =
    closedFamiliesAllDischarged &&
    phase408PrecursorPassed &&
    phase409PrecursorPassed &&
    phase411PrecursorPassed &&
    phase412PrecursorPassed &&
    phase413PrecursorPassed;
bool anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet =
    closedFamilies.Any(f => f.ProducesWeldedScalarSmDoublet is true);
bool onlyOpenFamiliesRequireNewCarrierOrSourceSpecification =
    openFamilies.Length > 0 && openFamilies.All(f => f.RequiresNewSourceSpecification);
bool unobservedPhaseStillRequiredForFurtherProgress =
    openFamilies.Any(f => f.Id == "unobserved-phase-carrier");
bool fermionicCohomologySquareRootStillRequiresSpecification =
    openFamilies.Any(f => f.Id == "fermionic-cohomology-square-root-delta-omega");
bool ansatzMenuUsesPhysicalTargets = false;

var hashPayload = string.Join("\n", operatorMenu.Select(f =>
    $"{f.Id}|{f.Order?.ToString() ?? "open"}|{string.Join(",", f.Operations)}|{f.Carrier}|{f.EvidencePhase}|{f.ClosedByExistingProof}|{f.ProducesWeldedScalarSmDoublet}|{f.RequiresNewSourceSpecification}"));
string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashPayload))).ToLowerInvariant();

bool physicalTargetsConsultedForConstruction = ansatzMenuUsesPhysicalTargets;
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

bool probeInternallyConsistent =
    requestedOperatorAlphabetCovered &&
    lowOrderProbedCarrierFamiliesClosed &&
    onlyOpenFamiliesRequireNewCarrierOrSourceSpecification &&
    !anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet &&
    !physicalTargetsConsultedForConstruction;

bool generalShiabEpsilonOperatorAnsatzProbePassed = probeInternallyConsistent;

string terminalStatus = generalShiabEpsilonOperatorAnsatzProbePassed
    ? "general-shiab-epsilon-operator-ansatz-closed-on-probed-carriers-new-source-required"
    : "general-shiab-epsilon-operator-ansatz-probe-blocked";

string decision = generalShiabEpsilonOperatorAnsatzProbePassed
    ? "The general low-order Shiab/epsilon operator ansatz is CLOSED on the currently computable carriers. The requested operation alphabet is explicitly covered: wedge, Hodge star, contraction, commutator, i-weighted anticommutator, Clifford volume, and epsilon conjugation. Every closed family reduces to an upstream exact no-go: Phase408 kills the naive trace extraction; Phase409 kills linear, bilinear, and epsilon-built frame-cross doublets; Phase411/412 kill Dirac-squared and quartic composite channels; Phase413 kills finite-dimensional noncompact real-form evasion. No closed low-order family produces a welded spin-0 SM doublet. The only remaining branches require genuinely new input: a computable unobserved-phase carrier, or an explicit first-order cohomology/square-root operator delta_omega. No contract field is filled."
    : "Do not use the ansatz-closure claim until every upstream no-go certificate and the operation alphabet coverage checks pass.";

var result = new
{
    phaseId = "phase414-general-shiab-epsilon-operator-ansatz-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    generalShiabEpsilonOperatorAnsatzProbePassed,
    phase408PrecursorPassed,
    phase409PrecursorPassed,
    phase409PairingMenuComplete,
    phase411PrecursorPassed,
    phase412PrecursorPassed,
    phase413PrecursorPassed,
    probeInternallyConsistent,
    requiredOperatorAlphabet,
    requestedOperatorAlphabetCovered,
    operatorMenuCandidateCount = operatorMenu.Length,
    closedLowOrderFamilyCount = closedFamilies.Length,
    openResidualFamilyCount = openFamilies.Length,
    lowOrderProbedCarrierFamiliesClosed,
    anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet,
    onlyOpenFamiliesRequireNewCarrierOrSourceSpecification,
    unobservedPhaseStillRequiredForFurtherProgress,
    fermionicCohomologySquareRootStillRequiresSpecification,
    operatorMenu,
    closureEvidence = new
    {
        phase408 = new
        {
            summaryPath = Phase408SummaryPath,
            spinZeroSlotCannotCarryFullDoublet = JsonBool(phase408.RootElement, "spinZeroSlotCannotCarryFullDoublet"),
            centralizerIsTrivial = JsonBool(phase408.RootElement, "centralizerIsTrivial"),
        },
        phase409 = new
        {
            summaryPath = Phase409SummaryPath,
            linearSingletDimension = JsonInt(phase409.RootElement, "linearSingletDimension"),
            bilinearSpinZeroDimension = JsonInt(phase409.RootElement, "bilinearSpinZeroDimension"),
            parityEvenInvariantCount = JsonInt(phase409.RootElement, "parityEvenInvariantCount"),
            parityOddInvariantCount = JsonInt(phase409.RootElement, "parityOddInvariantCount"),
            doubletPatternStateCount = JsonInt(phase409.RootElement, "doubletPatternStateCount"),
        },
        phase411 = new
        {
            summaryPath = Phase411SummaryPath,
            leftRightBilinearChannelHasNoWeldedScalar = JsonBool(phase411.RootElement, "leftRightBilinearChannelHasNoWeldedScalar"),
            majoranaSpinZeroSmDoubletCount = JsonInt(phase411.RootElement, "majoranaSpinZeroSmDoubletCount"),
        },
        phase412 = new
        {
            summaryPath = Phase412SummaryPath,
            unionIntersectionRealDimension = JsonInt(phase412.RootElement, "unionIntersectionRealDimension"),
            quarticWeldedScalarSmDoubletAbsentAllChannels = JsonBool(phase412.RootElement, "quarticWeldedScalarSmDoubletAbsentAllChannels"),
        },
        phase413 = new
        {
            summaryPath = Phase413SummaryPath,
            complexifiedWeldsCoincide = JsonBool(phase413.RootElement, "complexifiedWeldsCoincide"),
            realFormTransferEstablished = JsonBool(phase413.RootElement, "realFormTransferEstablished"),
        },
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
    explicitCandidateOnlyNonclaims = new[]
    {
        "this is a low-order closure certificate on currently computable finite-dimensional carriers, not a derivation of the draft's missing operator-choice notes",
        "unobserved-phase fields are not closed here because no explicit representation/action has been pinned to local or public source text",
        "the Superphysics part-09b cohomology square-root clue remains a research branch until delta_omega, its complex, and its target cohomology are specified",
        "no physical coupling, VEV, pole extraction, or GeV normalization is supplied",
        "no Phase201 or Phase256 contract field is filled",
    },
    sourceEvidence = new
    {
        phase408SummaryPath = Phase408SummaryPath,
        phase409SummaryPath = Phase409SummaryPath,
        phase411SummaryPath = Phase411SummaryPath,
        phase412SummaryPath = Phase412SummaryPath,
        phase413SummaryPath = Phase413SummaryPath,
        superphysicsMirrorNote = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "general_shiab_epsilon_operator_ansatz_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "general_shiab_epsilon_operator_ansatz_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"generalShiabEpsilonOperatorAnsatzProbePassed={generalShiabEpsilonOperatorAnsatzProbePassed}");
Console.WriteLine($"requestedOperatorAlphabetCovered={requestedOperatorAlphabetCovered}");
Console.WriteLine($"operatorMenuCandidateCount={operatorMenu.Length} closed={closedFamilies.Length} open={openFamilies.Length}");
Console.WriteLine($"lowOrderProbedCarrierFamiliesClosed={lowOrderProbedCarrierFamiliesClosed}");
Console.WriteLine($"anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet={anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet}");
Console.WriteLine($"unobservedPhaseStillRequiredForFurtherProgress={unobservedPhaseStillRequiredForFurtherProgress}");
Console.WriteLine($"fermionicCohomologySquareRootStillRequiresSpecification={fermionicCohomologySquareRootStillRequiresSpecification}");
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

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

sealed record OperatorFamily(
    string Id,
    int? Order,
    string[] Operations,
    string Carrier,
    string EvidencePhase,
    string ClosureReason,
    bool ClosedByExistingProof,
    bool? ProducesWeldedScalarSmDoublet,
    bool RequiresNewSourceSpecification);
