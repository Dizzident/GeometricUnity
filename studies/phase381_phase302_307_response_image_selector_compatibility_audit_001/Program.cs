using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/output";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit.json";
const string Phase307SummaryPath = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase308Path = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json";
const string Phase309Path = "studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json";
const string Phase310Path = "studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase380Path = "studies/phase380_response_image_wz_contract_application_audit_001/output/response_image_wz_contract_application_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;

var outputDir = Environment.GetEnvironmentVariable("PHASE381_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase307Summary = JsonDocument.Parse(File.ReadAllText(Phase307SummaryPath));
using var phase308 = JsonDocument.Parse(File.ReadAllText(Phase308Path));
using var phase309 = JsonDocument.Parse(File.ReadAllText(Phase309Path));
using var phase310 = JsonDocument.Parse(File.ReadAllText(Phase310Path));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase380 = JsonDocument.Parse(File.ReadAllText(Phase380Path));

var p302BestCandidate = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");
var bestSelection = phase307Summary.RootElement.GetProperty("bestP302ScaledStableCommonSelectionLaw");
var selectedAssessment = bestSelection.GetProperty("selectedAssessment");
var selectedWRow = selectedAssessment.GetProperty("wRow");
var selectedZRow = selectedAssessment.GetProperty("zRow");

var chargedAxis0CandidateIds = JsonStringArray(phase307Summary.RootElement, "chargedAxis0CandidateIds");
var chargedAxis1CandidateIds = JsonStringArray(phase307Summary.RootElement, "chargedAxis1CandidateIds");
var neutralAxisCandidateIds = JsonStringArray(phase307Summary.RootElement, "neutralAxisCandidateIds");
var chargedAxis0Set = chargedAxis0CandidateIds.ToHashSet(StringComparer.Ordinal);
var chargedAxis1Set = chargedAxis1CandidateIds.ToHashSet(StringComparer.Ordinal);
var neutralAxisSet = neutralAxisCandidateIds.ToHashSet(StringComparer.Ordinal);

var carrier = phase379.RootElement.GetProperty("carrier");
int gaugeDimension = JsonInt(carrier, "gaugeDimension") ?? 3;
int suppressedGaugeAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis") ?? -1;
var phase379DominantGaugeAxes = Enumerable.Range(0, gaugeDimension)
    .Where(axis => axis != suppressedGaugeAxis)
    .ToArray();
var phase307CanonicalChargedAxes = new[] { 0, 1 };

var wDefinition = FindSourceDefinition(RequiredString(selectedWRow, "definitionId"));
var zDefinition = FindSourceDefinition(RequiredString(selectedZRow, "definitionId"));
var selectedWAxisUsage = AxisUsageForRow("w-boson", selectedWRow, wDefinition);
var selectedZAxisUsage = AxisUsageForRow("z-boson", selectedZRow, zDefinition);
var selectedGaugeAxes = selectedWAxisUsage.GaugeAxes.Concat(selectedZAxisUsage.GaugeAxes).Distinct().Order().ToArray();

bool phase307Passed = JsonBool(phase307Summary.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true;
bool phase307TargetBlind = JsonBool(phase307Summary.RootElement, "targetObservablesUsedForConstruction") is false
    && JsonBool(phase307Summary.RootElement, "targetValuesUsedOnlyForPostCandidateEvaluation") is true;
bool bestSelectionUsesP302Scale = JsonBool(bestSelection, "usesP302ScaleForSelection") is true;
bool bestSelectionUsesTargets = JsonBool(bestSelection, "usesTargetsForSelection") is true;
bool bestSelectionP302ScaledPassed = JsonBool(bestSelection, "selectedP302ScaledStableCommonPassed") is true;
bool bestSelectionRawPassed = JsonBool(bestSelection, "selectedRawStableCommonPassed") is true;
bool phase307CanFillContract = JsonBool(phase307Summary.RootElement, "canFillPhase201WzContract") is true;

bool phase379Passed = JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true;
bool phase379TargetBlind = JsonBool(phase379.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is false;
bool phase379StableTwoAxis = JsonBool(phase379.RootElement, "stableTwoGaugeAxisDominance") is true;
bool phase379CanonicalSelectorPresent = JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector") is true;
bool phase379DirectWzLawPresent = JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;
bool phase379StrictTransportPassed = JsonBool(phase379.RootElement, "strictBackgroundImageTransportPassed") is true;
bool phase379RoutePromotesWz = JsonBool(phase379.RootElement, "routePromotesWzMasses") is true;

bool selectedWUsesSuppressedGaugeAxis = selectedWAxisUsage.UsesSuppressedGaugeAxis;
bool selectedZUsesSuppressedGaugeAxis = selectedZAxisUsage.UsesSuppressedGaugeAxis;
bool selectedAnyRowUsesSuppressedGaugeAxis = selectedWUsesSuppressedGaugeAxis || selectedZUsesSuppressedGaugeAxis;
bool selectedWUsesCanonicalChargedAxes = SetEquals(selectedWAxisUsage.GaugeAxes, phase307CanonicalChargedAxes);
bool selectedZUsesDominantNeutralAxis = selectedZAxisUsage.GaugeAxes.SequenceEqual(new[] { 2 })
    && phase379DominantGaugeAxes.Contains(2);
bool phase379DominantAxesMatchPhase307ChargedPair = SetEquals(phase379DominantGaugeAxes, phase307CanonicalChargedAxes);
bool responseImageSelectorSidecarCompatible = phase379CanonicalSelectorPresent
    && !selectedAnyRowUsesSuppressedGaugeAxis
    && phase379DominantAxesMatchPhase307ChargedPair;
bool responseImageSidecarConflictPresent = selectedWUsesSuppressedGaugeAxis
    && selectedWUsesCanonicalChargedAxes
    && selectedZUsesDominantNeutralAxis
    && !phase379DominantAxesMatchPhase307ChargedPair
    && !phase379CanonicalSelectorPresent;

bool phase302ScalePromotable = JsonBool(phase302.RootElement, "sourceRowsPromotable") is true
    || JsonBool(phase302.RootElement, "canFillPhase201WzContract") is true;
bool p302ScaleLeadRawCommonOnly = JsonBool(p302BestCandidate, "rawAndCommonGatesPassed") is true
    && JsonBool(p302BestCandidate, "stableRawCommonGatesPassed") is false
    && JsonBool(p302BestCandidate, "commonScaleApplicationTheoremPresent") is false
    && JsonBool(p302BestCandidate, "particleLawApplicationTheoremPresent") is false
    && JsonBool(p302BestCandidate, "promotionEligible") is false;
bool phase308ScaleTransferAllowed = JsonBool(phase308.RootElement, "scaleTransferAllowed") is true
    || JsonBool(phase308.RootElement, "canFillPhase201WzContract") is true;
bool phase309ScalePromotable = JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable") is true
    || JsonBool(phase309.RootElement, "canFillPhase201WzContract") is true;
bool phase310CanPromotePhase302Lead = JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead") is true
    || JsonBool(phase310.RootElement, "canFillPhase201WzContract") is true;

bool phase380Passed = JsonBool(phase380.RootElement, "responseImageWzContractApplicationAuditPassed") is true;
bool phase380PreservesContractBlock = phase380Passed
    && JsonBool(phase380.RootElement, "sourceContractApplicationAllowed") is false
    && JsonInt(phase380.RootElement, "acceptedContractFieldCount") == 0
    && JsonInt(phase380.RootElement, "blockedContractFieldCount") == ExpectedWzMissingFieldCount
    && JsonInt(phase380.RootElement, "phase213WzMissingFieldCount") == ExpectedWzMissingFieldCount
    && JsonBool(phase380.RootElement, "canFillPhase201WzContract") is false;

bool targetBlindConstruction = phase307TargetBlind && phase379TargetBlind;
bool physicalTargetsConsultedForConstruction = false;
var targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase381-phase302-307-response-image-selector-compatibility-audit-v1",
    $"phase302Path={Phase302Path}",
    $"phase307Path={Phase307Path}",
    $"phase307SummaryPath={Phase307SummaryPath}",
    $"phase379Hash={JsonString(phase379.RootElement, "targetBlindConstructionHash")}",
    $"phase379Path={Phase379Path}",
    $"bestSelectionLaw={JsonString(bestSelection, "lawId")}",
    $"selectedWDefinition={RequiredString(selectedWRow, "definitionId")}",
    $"selectedZDefinition={RequiredString(selectedZRow, "definitionId")}",
    "physicalTargetsConsultedForConstruction=false"));

bool sourceContractApplicationAllowed = false;
bool canFillPhase201WzContract = false;
bool canFillPhase201HiggsContract = false;
bool routePromotesWzMasses = false;
bool routePromotesHiggsMass = false;
bool routeCompletesBosonPredictions = false;
bool phase201TemplateMutated = false;
int fieldsAppliedToPhase201TemplateCount = 0;
int acceptedContractFieldCount = 0;
int blockedContractFieldCount = ExpectedWzMissingFieldCount;
int phase213WzMissingFieldCount = JsonInt(phase380.RootElement, "phase213WzMissingFieldCount") ?? ExpectedWzMissingFieldCount;

var checks = new[]
{
    Check(
        "phase307-selected-p302-scaled-near-pass-materialized",
        phase307Passed
            && phase307TargetBlind
            && JsonString(bestSelection, "lawId") == "p302-scaled-max-min-magnitude"
            && bestSelectionUsesP302Scale
            && !bestSelectionUsesTargets
            && bestSelectionP302ScaledPassed
            && !bestSelectionRawPassed
            && !phase307CanFillContract,
        $"phase307Passed={phase307Passed}; phase307TargetBlind={phase307TargetBlind}; lawId={JsonString(bestSelection, "lawId")}; usesP302Scale={bestSelectionUsesP302Scale}; usesTargets={bestSelectionUsesTargets}; p302ScaledPassed={bestSelectionP302ScaledPassed}; rawPassed={bestSelectionRawPassed}; phase307CanFill={phase307CanFillContract}"),
    Check(
        "selected-row-gauge-axis-usage-materialized",
        selectedWUsesCanonicalChargedAxes
            && selectedZUsesDominantNeutralAxis
            && selectedGaugeAxes.SequenceEqual(new[] { 0, 1, 2 }),
        $"wAxes={string.Join(",", selectedWAxisUsage.GaugeAxes)}; zAxes={string.Join(",", selectedZAxisUsage.GaugeAxes)}; selectedGaugeAxes={string.Join(",", selectedGaugeAxes)}; wDefinition={selectedWAxisUsage.DefinitionId}; zDefinition={selectedZAxisUsage.DefinitionId}"),
    Check(
        "phase379-response-image-sidecar-present-nonpromotional",
        phase379Passed
            && phase379TargetBlind
            && phase379StableTwoAxis
            && suppressedGaugeAxis == 1
            && JsonDouble(phase379.RootElement, "maxSuppressedGaugeAxisProjectorFraction") is <= 0.005
            && JsonDouble(phase379.RootElement, "minDominantGaugePairProjectorFraction") is >= 0.995
            && !phase379CanonicalSelectorPresent
            && !phase379DirectWzLawPresent
            && !phase379StrictTransportPassed
            && !phase379RoutePromotesWz,
        $"phase379Passed={phase379Passed}; targetBlind={phase379TargetBlind}; stableTwoAxis={phase379StableTwoAxis}; suppressedAxis={suppressedGaugeAxis}; dominantAxes={string.Join(",", phase379DominantGaugeAxes)}; maxSuppressedFraction={JsonDouble(phase379.RootElement, "maxSuppressedGaugeAxisProjectorFraction")}; minDominantPairFraction={JsonDouble(phase379.RootElement, "minDominantGaugePairProjectorFraction")}; canonicalSelector={phase379CanonicalSelectorPresent}; directWzLaw={phase379DirectWzLawPresent}; strictTransport={phase379StrictTransportPassed}; promotesWz={phase379RoutePromotesWz}"),
    Check(
        "response-image-sidecar-conflict-with-selected-charged-ladder",
        responseImageSidecarConflictPresent && !responseImageSelectorSidecarCompatible,
        $"selectedWUsesSuppressedGaugeAxis={selectedWUsesSuppressedGaugeAxis}; selectedZUsesSuppressedGaugeAxis={selectedZUsesSuppressedGaugeAxis}; selectedWUsesCanonicalChargedAxes={selectedWUsesCanonicalChargedAxes}; selectedZUsesDominantNeutralAxis={selectedZUsesDominantNeutralAxis}; phase379DominantAxesMatchPhase307ChargedPair={phase379DominantAxesMatchPhase307ChargedPair}; responseImageSelectorSidecarCompatible={responseImageSelectorSidecarCompatible}"),
    Check(
        "phase302-scale-theorem-blockers-preserved",
        p302ScaleLeadRawCommonOnly
            && !phase302ScalePromotable
            && !phase308ScaleTransferAllowed
            && !phase309ScalePromotable
            && !phase310CanPromotePhase302Lead,
        $"p302ScaleLeadRawCommonOnly={p302ScaleLeadRawCommonOnly}; commonScaleId={JsonString(p302BestCandidate, "commonScaleId")}; particleLawId={JsonString(p302BestCandidate, "particleLawId")}; phase302ScalePromotable={phase302ScalePromotable}; phase308ScaleTransferAllowed={phase308ScaleTransferAllowed}; phase309ScalePromotable={phase309ScalePromotable}; phase310CanPromotePhase302Lead={phase310CanPromotePhase302Lead}"),
    Check(
        "phase380-contract-blocker-preserved",
        phase380PreservesContractBlock,
        $"phase380Passed={phase380Passed}; sourceContractApplicationAllowed={JsonBool(phase380.RootElement, "sourceContractApplicationAllowed")}; acceptedFields={JsonInt(phase380.RootElement, "acceptedContractFieldCount")}; blockedFields={JsonInt(phase380.RootElement, "blockedContractFieldCount")}; phase213WzMissingFieldCount={JsonInt(phase380.RootElement, "phase213WzMissingFieldCount")}; canFillPhase201WzContract={JsonBool(phase380.RootElement, "canFillPhase201WzContract")}"),
    Check(
        "no-prediction-or-contract-promotion",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; canFillPhase201WzContract={canFillPhase201WzContract}; canFillPhase201HiggsContract={canFillPhase201HiggsContract}; routePromotesWzMasses={routePromotesWzMasses}; routePromotesHiggsMass={routePromotesHiggsMass}; routeCompletesBosonPredictions={routeCompletesBosonPredictions}; phase201TemplateMutated={phase201TemplateMutated}; fieldsApplied={fieldsAppliedToPhase201TemplateCount}; acceptedContractFieldCount={acceptedContractFieldCount}; blockedContractFieldCount={blockedContractFieldCount}"),
};

bool phase302307ResponseImageSelectorCompatibilityAuditPassed = checks.All(check => check.Passed);
string terminalStatus = phase302307ResponseImageSelectorCompatibilityAuditPassed
    ? "phase302-307-response-image-selector-compatibility-audit-sidecar-conflict-nonpromotional"
    : "phase302-307-response-image-selector-compatibility-audit-review-required";
string decision = phase302307ResponseImageSelectorCompatibilityAuditPassed
    ? "The strongest Phase307 predeclared near-pass selector remains non-promotional and is not supported by the Phase379 response-image sidecar. The selected W row uses the Phase307 canonical charged-ladder axes 0 and 1, but Phase379 suppresses axis 1 and has dominant axes 0 and 2. The selected Z row aligns with dominant neutral axis 2, so the sidecar sharpens the blocker: a theorem must either derive the charged-ladder axis use despite suppression or supply an observed electroweak projection map explaining why the response-image carrier axes are not the physical W/Z axes."
    : "Review the Phase302/307 response-image selector compatibility audit before using the Phase379 sidecar in W/Z source-law work.";

var result = new
{
    phaseId = "phase381-phase302-307-response-image-selector-compatibility-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    phase302307ResponseImageSelectorCompatibilityAuditPassed,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind = "phase307-selected-p302-scaled-wz-row-selector-with-phase379-response-image-sidecar",
    phase307SelectedNearPass = new
    {
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase307Passed,
        phase307TargetBlind,
        lawId = JsonString(bestSelection, "lawId"),
        usesP302ScaleForSelection = bestSelectionUsesP302Scale,
        usesTargetsForSelection = bestSelectionUsesTargets,
        selectedRawStableCommonPassed = bestSelectionRawPassed,
        selectedP302ScaledStableCommonPassed = bestSelectionP302ScaledPassed,
        canFillPhase201WzContract = phase307CanFillContract,
        selectedAssessmentId = JsonString(selectedAssessment, "assessmentId"),
        selectedP302ScaledCommonMeanRelativeSpread = JsonDouble(selectedAssessment, "p302ScaledCommonMeanRelativeSpread"),
        selectedMinP302ScaledRowRawToTargetRatio = JsonDouble(selectedAssessment, "minP302ScaledRowRawToTargetRatio"),
    },
    phase302ScaleLead = new
    {
        phase302Path = Phase302Path,
        candidateId = JsonString(p302BestCandidate, "candidateId"),
        commonScaleId = JsonString(p302BestCandidate, "commonScaleId"),
        particleLawId = JsonString(p302BestCandidate, "particleLawId"),
        commonScaleValue = JsonDouble(p302BestCandidate, "commonScaleValue"),
        wParticleMultiplier = JsonDouble(p302BestCandidate, "wParticleMultiplier"),
        zParticleMultiplier = JsonDouble(p302BestCandidate, "zParticleMultiplier"),
        wTotalScale = JsonDouble(p302BestCandidate, "wTotalScale"),
        zTotalScale = JsonDouble(p302BestCandidate, "zTotalScale"),
        rawAndCommonGatesPassed = JsonBool(p302BestCandidate, "rawAndCommonGatesPassed"),
        stableRawCommonGatesPassed = JsonBool(p302BestCandidate, "stableRawCommonGatesPassed"),
        commonScaleApplicationTheoremPresent = JsonBool(p302BestCandidate, "commonScaleApplicationTheoremPresent"),
        particleLawApplicationTheoremPresent = JsonBool(p302BestCandidate, "particleLawApplicationTheoremPresent"),
        promotionEligible = JsonBool(p302BestCandidate, "promotionEligible"),
        p302ScaleLeadRawCommonOnly,
        theoremClaimed = JsonBool(phase302.RootElement, "theoremClaimed"),
        sourceRowsPromotable = JsonBool(phase302.RootElement, "sourceRowsPromotable"),
        canFillPhase201WzContract = JsonBool(phase302.RootElement, "canFillPhase201WzContract"),
    },
    selectedRowAxisUsage = new
    {
        canonicalPhase307ChargedAxes = phase307CanonicalChargedAxes,
        selectedGaugeAxes,
        selectedWAxisUsage,
        selectedZAxisUsage,
        selectedWUsesSuppressedGaugeAxis,
        selectedZUsesSuppressedGaugeAxis,
        selectedAnyRowUsesSuppressedGaugeAxis,
        selectedWUsesCanonicalChargedAxes,
        selectedZUsesDominantNeutralAxis,
    },
    responseImageSidecar = new
    {
        phase379Path = Phase379Path,
        phase379Passed,
        phase379TargetBlind,
        suppressedGaugeAxis,
        dominantGaugeAxes = phase379DominantGaugeAxes,
        maxSuppressedGaugeAxisProjectorFraction = JsonDouble(phase379.RootElement, "maxSuppressedGaugeAxisProjectorFraction"),
        minDominantGaugePairProjectorFraction = JsonDouble(phase379.RootElement, "minDominantGaugePairProjectorFraction"),
        strictBackgroundImageTransportPassed = phase379StrictTransportPassed,
        looseBackgroundImageTransportPassed = JsonBool(phase379.RootElement, "looseBackgroundImageTransportPassed"),
        routeProvidesCanonicalGaugeAxisSelector = phase379CanonicalSelectorPresent,
        routeProvidesDirectTargetIndependentWzBridgeSourceLaw = phase379DirectWzLawPresent,
        phase379DominantAxesMatchPhase307ChargedPair,
        responseImageSelectorSidecarCompatible,
        responseImageSidecarConflictPresent,
    },
    inheritedBlockers = new
    {
        phase302Path = Phase302Path,
        phase302ScalePromotable,
        phase302CanFillPhase201WzContract = JsonBool(phase302.RootElement, "canFillPhase201WzContract"),
        phase308Path = Phase308Path,
        phase308ScaleTransferAllowed,
        phase308CanFillPhase201WzContract = JsonBool(phase308.RootElement, "canFillPhase201WzContract"),
        phase309Path = Phase309Path,
        phase309ScalePromotable,
        phase309CanFillPhase201WzContract = JsonBool(phase309.RootElement, "canFillPhase201WzContract"),
        phase310Path = Phase310Path,
        phase310CanPromotePhase302Lead,
        phase310CanFillPhase201WzContract = JsonBool(phase310.RootElement, "canFillPhase201WzContract"),
        phase380Path = Phase380Path,
        phase380PreservesContractBlock,
        phase380AcceptedContractFieldCount = JsonInt(phase380.RootElement, "acceptedContractFieldCount"),
        phase380BlockedContractFieldCount = JsonInt(phase380.RootElement, "blockedContractFieldCount"),
        phase380Phase213WzMissingFieldCount = JsonInt(phase380.RootElement, "phase213WzMissingFieldCount"),
    },
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    blockedContractFieldCount,
    phase213WzMissingFieldCount,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    checks,
    sourceEvidence = new
    {
        phase302Path = Phase302Path,
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase308Path = Phase308Path,
        phase309Path = Phase309Path,
        phase310Path = Phase310Path,
        phase379Path = Phase379Path,
        phase380Path = Phase380Path,
    },
    nextRequiredArtifact = new[]
    {
        "A theorem deriving the Phase307 charged-ladder axes and Phase302 scale before target comparison, including why the Phase379-suppressed axis is physically required.",
        "A GU-native observed electroweak projection map that separates diagnostic carrier axes from physical photon/W/Z axes.",
        "Separate W and Z source rows with raw-amplitude gates, common-bridge gates, derivation ids, stability sidecars, and GeV normalization.",
    },
    decision,
};

File.WriteAllText(
    Path.Combine(outputDir, "phase302_307_response_image_selector_compatibility_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase302_307_response_image_selector_compatibility_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase302307ResponseImageSelectorCompatibilityAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        phase307SelectedNearPassLawId = result.phase307SelectedNearPass.lawId,
        phase307SelectedNearPassUsesP302Scale = result.phase307SelectedNearPass.usesP302ScaleForSelection,
        phase307SelectedNearPassUsesTargets = result.phase307SelectedNearPass.usesTargetsForSelection,
        phase307SelectedRawStableCommonPassed = result.phase307SelectedNearPass.selectedRawStableCommonPassed,
        phase307SelectedP302ScaledStableCommonPassed = result.phase307SelectedNearPass.selectedP302ScaledStableCommonPassed,
        phase307SelectedAssessmentId = result.phase307SelectedNearPass.selectedAssessmentId,
        phase302CommonScaleId = result.phase302ScaleLead.commonScaleId,
        phase302ParticleLawId = result.phase302ScaleLead.particleLawId,
        phase302CommonScaleValue = result.phase302ScaleLead.commonScaleValue,
        phase302WTotalScale = result.phase302ScaleLead.wTotalScale,
        phase302ZTotalScale = result.phase302ScaleLead.zTotalScale,
        phase302RawAndCommonGatesPassed = result.phase302ScaleLead.rawAndCommonGatesPassed,
        phase302StableRawCommonGatesPassed = result.phase302ScaleLead.stableRawCommonGatesPassed,
        phase302CommonScaleApplicationTheoremPresent = result.phase302ScaleLead.commonScaleApplicationTheoremPresent,
        phase302ParticleLawApplicationTheoremPresent = result.phase302ScaleLead.particleLawApplicationTheoremPresent,
        phase302PromotionEligible = result.phase302ScaleLead.promotionEligible,
        p302ScaleLeadRawCommonOnly,
        selectedWDefinitionId = selectedWAxisUsage.DefinitionId,
        selectedZDefinitionId = selectedZAxisUsage.DefinitionId,
        selectedWGaugeAxes = selectedWAxisUsage.GaugeAxes,
        selectedZGaugeAxes = selectedZAxisUsage.GaugeAxes,
        selectedWUsesSuppressedGaugeAxis,
        selectedZUsesSuppressedGaugeAxis,
        selectedAnyRowUsesSuppressedGaugeAxis,
        selectedWUsesCanonicalChargedAxes,
        selectedZUsesDominantNeutralAxis,
        phase379SuppressedGaugeAxis = suppressedGaugeAxis,
        phase379DominantGaugeAxes,
        phase379DominantAxesMatchPhase307ChargedPair,
        responseImageSelectorSidecarCompatible,
        responseImageSidecarConflictPresent,
        phase302ScalePromotable,
        phase308ScaleTransferAllowed,
        phase309ScalePromotable,
        phase310CanPromotePhase302Lead,
        phase380PreservesContractBlock,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.acceptedContractFieldCount,
        result.blockedContractFieldCount,
        result.phase213WzMissingFieldCount,
        result.phase201FieldsDefensiblyFilled,
        result.checks,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase302307ResponseImageSelectorCompatibilityAuditPassed={phase302307ResponseImageSelectorCompatibilityAuditPassed}");
Console.WriteLine($"phase307SelectedNearPassLawId={JsonString(bestSelection, "lawId")}");
Console.WriteLine($"selectedWGaugeAxes={string.Join(",", selectedWAxisUsage.GaugeAxes)}");
Console.WriteLine($"selectedZGaugeAxes={string.Join(",", selectedZAxisUsage.GaugeAxes)}");
Console.WriteLine($"phase379DominantGaugeAxes={string.Join(",", phase379DominantGaugeAxes)}");
Console.WriteLine($"phase379SuppressedGaugeAxis={suppressedGaugeAxis}");
Console.WriteLine($"selectedWUsesSuppressedGaugeAxis={selectedWUsesSuppressedGaugeAxis}");
Console.WriteLine($"responseImageSidecarConflictPresent={responseImageSidecarConflictPresent}");
Console.WriteLine($"responseImageSelectorSidecarCompatible={responseImageSelectorSidecarCompatible}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

JsonElement FindSourceDefinition(string definitionId)
{
    foreach (var definition in phase307.RootElement.GetProperty("sourceDefinitions").EnumerateArray())
    {
        if (RequiredString(definition, "definitionId") == definitionId)
            return definition;
    }

    throw new InvalidDataException($"Phase307 source definition not found: {definitionId}");
}

AxisUsage AxisUsageForRow(string particleId, JsonElement row, JsonElement definition)
{
    var candidateIds = JsonStringArray(definition, particleId == "w-boson" ? "wCandidateIds" : "zCandidateIds");
    var axes = new List<int>();
    if (candidateIds.Any(chargedAxis0Set.Contains))
        axes.Add(0);
    if (candidateIds.Any(chargedAxis1Set.Contains))
        axes.Add(1);
    if (candidateIds.Any(neutralAxisSet.Contains))
        axes.Add(2);

    var gaugeAxes = axes.Distinct().Order().ToArray();
    return new AxisUsage(
        particleId,
        RequiredString(row, "rowId"),
        RequiredString(row, "definitionId"),
        gaugeAxes,
        candidateIds,
        gaugeAxes.Contains(suppressedGaugeAxis),
        gaugeAxes.Any(phase379DominantGaugeAxes.Contains),
        SetEquals(gaugeAxes, phase307CanonicalChargedAxes));
}

static bool SetEquals(IReadOnlyList<int> left, IReadOnlyList<int> right) =>
    left.Order().SequenceEqual(right.Order());

static Check Check(string checkId, bool passed, string details) => new(checkId, passed, details);

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray()
        : Array.Empty<string>();

static string HashText(string text)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record Check(string CheckId, bool Passed, string Details);
sealed record AxisUsage(
    string ParticleId,
    string RowId,
    string DefinitionId,
    IReadOnlyList<int> GaugeAxes,
    IReadOnlyList<string> CandidateIds,
    bool UsesSuppressedGaugeAxis,
    bool UsesDominantGaugeAxis,
    bool UsesCanonicalChargedAxes);
