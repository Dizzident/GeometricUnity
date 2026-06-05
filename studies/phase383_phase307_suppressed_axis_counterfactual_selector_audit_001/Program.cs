using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase383_phase307_suppressed_axis_counterfactual_selector_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit.json";
const string Phase307SummaryPath = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase381Path = "studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/output/phase302_307_response_image_selector_compatibility_audit_summary.json";
const string Phase382Path = "studies/phase382_response_image_observed_projection_requirement_audit_001/output/response_image_observed_projection_requirement_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;

var outputDir = Environment.GetEnvironmentVariable("PHASE383_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase307Summary = JsonDocument.Parse(File.ReadAllText(Phase307SummaryPath));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase381 = JsonDocument.Parse(File.ReadAllText(Phase381Path));
using var phase382 = JsonDocument.Parse(File.ReadAllText(Phase382Path));

var chargedAxis0CandidateIds = JsonStringArray(phase307.RootElement, "chargedAxis0CandidateIds");
var chargedAxis1CandidateIds = JsonStringArray(phase307.RootElement, "chargedAxis1CandidateIds");
var neutralAxisCandidateIds = JsonStringArray(phase307.RootElement, "neutralAxisCandidateIds");
var chargedAxis0Set = chargedAxis0CandidateIds.ToHashSet(StringComparer.Ordinal);
var chargedAxis1Set = chargedAxis1CandidateIds.ToHashSet(StringComparer.Ordinal);
var neutralAxisSet = neutralAxisCandidateIds.ToHashSet(StringComparer.Ordinal);

int suppressedGaugeAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis") ?? -1;
var carrier = phase379.RootElement.GetProperty("carrier");
int gaugeDimension = JsonInt(carrier, "gaugeDimension") ?? 3;
var phase379DominantGaugeAxes = Enumerable.Range(0, gaugeDimension)
    .Where(axis => axis != suppressedGaugeAxis)
    .ToArray();
var canonicalChargedAxes = new[] { 0, 1 };

var sourceDefinitions = phase307.RootElement.GetProperty("sourceDefinitions")
    .EnumerateArray()
    .Select(definition => new SourceDefinitionAxisAudit(
        RequiredString(definition, "definitionId"),
        JsonString(definition, "kind"),
        JsonStringArray(definition, "wCandidateIds"),
        JsonStringArray(definition, "zCandidateIds"),
        AxisUsage(JsonStringArray(definition, "wCandidateIds")),
        AxisUsage(JsonStringArray(definition, "zCandidateIds")),
        JsonString(definition, "description")))
    .OrderBy(definition => definition.DefinitionId, StringComparer.Ordinal)
    .ToArray();
var definitionById = sourceDefinitions.ToDictionary(definition => definition.DefinitionId, StringComparer.Ordinal);
var wDefinitionsUsingSuppressedAxis = sourceDefinitions.Where(definition => definition.WGaugeAxes.Contains(suppressedGaugeAxis)).ToArray();
var wDefinitionsAvoidingSuppressedAxis = sourceDefinitions.Where(definition => !definition.WGaugeAxes.Contains(suppressedGaugeAxis)).ToArray();
var wDefinitionsUsingCanonicalChargedAxes = sourceDefinitions.Where(definition => SetEquals(definition.WGaugeAxes, canonicalChargedAxes)).ToArray();
var zDefinitionsUsingSuppressedAxis = sourceDefinitions.Where(definition => definition.ZGaugeAxes.Contains(suppressedGaugeAxis)).ToArray();

var selectionLawAudits = phase307.RootElement.GetProperty("targetIndependentSelectionLawAssessments")
    .EnumerateArray()
    .Select(selectionLaw =>
    {
        var selectedAssessment = selectionLaw.GetProperty("selectedAssessment");
        var wRow = selectedAssessment.GetProperty("wRow");
        var zRow = selectedAssessment.GetProperty("zRow");
        var wDefinitionId = RequiredString(wRow, "definitionId");
        var zDefinitionId = RequiredString(zRow, "definitionId");
        var wDefinition = definitionById[wDefinitionId];
        var zDefinition = definitionById[zDefinitionId];
        bool selectedWUsesSuppressedGaugeAxis = wDefinition.WGaugeAxes.Contains(suppressedGaugeAxis);
        bool selectedZUsesSuppressedGaugeAxis = zDefinition.ZGaugeAxes.Contains(suppressedGaugeAxis);
        bool selectedAvoidsSuppressedGaugeAxis = !selectedWUsesSuppressedGaugeAxis && !selectedZUsesSuppressedGaugeAxis;
        bool selectedSidecarCompatible = selectedAvoidsSuppressedGaugeAxis
            && SetEquals(wDefinition.WGaugeAxes, phase379DominantGaugeAxes)
            && zDefinition.ZGaugeAxes.All(phase379DominantGaugeAxes.Contains);
        return new SelectionLawAxisAudit(
            RequiredString(selectionLaw, "lawId"),
            JsonString(selectionLaw, "description"),
            JsonBool(selectionLaw, "usesP302ScaleForSelection") is true,
            JsonBool(selectionLaw, "usesTargetsForSelection") is true,
            JsonString(selectionLaw, "selectionPopulation"),
            JsonBool(selectionLaw, "selectedRawStableCommonPassed") is true,
            JsonBool(selectionLaw, "selectedP302ScaledStableCommonPassed") is true,
            JsonBool(selectionLaw, "canFillPhase201WzContract") is true,
            RequiredString(selectedAssessment, "assessmentId"),
            wDefinitionId,
            zDefinitionId,
            wDefinition.WGaugeAxes,
            zDefinition.ZGaugeAxes,
            selectedWUsesSuppressedGaugeAxis,
            selectedZUsesSuppressedGaugeAxis,
            selectedAvoidsSuppressedGaugeAxis,
            selectedSidecarCompatible,
            JsonDouble(selectedAssessment, "p302ScaledCommonMeanRelativeSpread"),
            JsonDouble(selectedAssessment, "minP302ScaledRowRawToTargetRatio"));
    })
    .ToArray();

var selectorsAvoidingSuppressedAxis = selectionLawAudits.Where(selection => selection.SelectedAvoidsSuppressedGaugeAxis).ToArray();
var selectorsWithWNotUsingSuppressedAxis = selectionLawAudits.Where(selection => !selection.SelectedWUsesSuppressedGaugeAxis).ToArray();
var selectorsP302ScaledStableCommonAvoidingSuppressedAxis = selectionLawAudits
    .Where(selection => selection.SelectedP302ScaledStableCommonPassed && selection.SelectedAvoidsSuppressedGaugeAxis)
    .ToArray();
var selectorsRawStableCommonAvoidingSuppressedAxis = selectionLawAudits
    .Where(selection => selection.SelectedRawStableCommonPassed && selection.SelectedAvoidsSuppressedGaugeAxis)
    .ToArray();
var sidecarCompatibleSelectionLaws = selectionLawAudits.Where(selection => selection.SelectedSidecarCompatible).ToArray();
var p302ScaledStableCommonSelectionLaw = selectionLawAudits
    .SingleOrDefault(selection => selection.SelectedP302ScaledStableCommonPassed);

bool phase307Passed = JsonBool(phase307Summary.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true;
bool phase307TargetBlind = JsonBool(phase307Summary.RootElement, "targetObservablesUsedForConstruction") is false
    && JsonBool(phase307Summary.RootElement, "targetValuesUsedOnlyForPostCandidateEvaluation") is true;
bool phase307PredeclaredSelectorSpaceMaterialized = phase307Passed
    && phase307TargetBlind
    && JsonInt(phase307Summary.RootElement, "selectionLawCount") == selectionLawAudits.Length
    && selectionLawAudits.Length == 8
    && sourceDefinitions.Length == JsonInt(phase307Summary.RootElement, "definitionCount");

bool phase379SidecarConflictContextMaterialized =
    JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true
    && JsonBool(phase379.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is false
    && JsonBool(phase379.RootElement, "stableTwoGaugeAxisDominance") is true
    && suppressedGaugeAxis == 1
    && phase379DominantGaugeAxes.SequenceEqual(new[] { 0, 2 })
    && JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap") is false
    && JsonBool(phase379.RootElement, "canFillPhase201WzContract") is false;

bool phase381ConflictMaterialized =
    JsonBool(phase381.RootElement, "phase302307ResponseImageSelectorCompatibilityAuditPassed") is true
    && JsonString(phase381.RootElement, "phase307SelectedNearPassLawId") == "p302-scaled-max-min-magnitude"
    && JsonBool(phase381.RootElement, "selectedWUsesSuppressedGaugeAxis") is true
    && JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent") is true
    && JsonBool(phase381.RootElement, "canFillPhase201WzContract") is false;

bool phase382ProjectionBlockerMaterialized =
    JsonBool(phase382.RootElement, "responseImageObservedProjectionRequirementAuditPassed") is true
    && JsonBool(phase382.RootElement, "observedCarrierAxisNamespaceSeparationMapPresent") is false
    && JsonBool(phase382.RootElement, "phase307SelectedNearPassRehabilitatedByObservedProjectionMap") is false
    && JsonBool(phase382.RootElement, "responseImageConflictRemainsActive") is true
    && JsonBool(phase382.RootElement, "canFillPhase201WzContract") is false;

bool everyPhase307WDefinitionUsesSuppressedGaugeAxis = sourceDefinitions.Length > 0
    && wDefinitionsUsingSuppressedAxis.Length == sourceDefinitions.Length
    && wDefinitionsAvoidingSuppressedAxis.Length == 0;
bool everyPhase307WDefinitionUsesCanonicalChargedAxes = sourceDefinitions.Length > 0
    && wDefinitionsUsingCanonicalChargedAxes.Length == sourceDefinitions.Length;
bool everyPredeclaredSelectorUsesSuppressedAxis = selectionLawAudits.Length > 0
    && selectionLawAudits.All(selection => selection.SelectedWUsesSuppressedGaugeAxis);
bool noPredeclaredSelectorAvoidsSuppressedAxis = selectorsAvoidingSuppressedAxis.Length == 0
    && selectorsWithWNotUsingSuppressedAxis.Length == 0;
bool noP302ScaledStableCommonSelectorAvoidsSuppressedAxis = selectorsP302ScaledStableCommonAvoidingSuppressedAxis.Length == 0
    && p302ScaledStableCommonSelectionLaw is not null
    && p302ScaledStableCommonSelectionLaw.SelectedWUsesSuppressedGaugeAxis;
bool noRawStableCommonSelectorAvoidsSuppressedAxis = selectorsRawStableCommonAvoidingSuppressedAxis.Length == 0;
bool noSidecarCompatiblePredeclaredSelector = sidecarCompatibleSelectionLaws.Length == 0;

bool sourceContractApplicationAllowed = false;
bool canFillPhase201WzContract = false;
bool canFillPhase201HiggsContract = false;
bool canFillPhase256ObservedFieldExtractionContract = false;
bool routePromotesWzMasses = false;
bool routePromotesHiggsMass = false;
bool routeCompletesBosonPredictions = false;
bool phase201TemplateMutated = false;
int fieldsAppliedToPhase201TemplateCount = 0;
int acceptedContractFieldCount = 0;
int blockedContractFieldCount = ExpectedWzMissingFieldCount;
int phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? ExpectedWzMissingFieldCount;

var checks = new[]
{
    Check(
        "phase307-predeclared-selector-space-materialized",
        phase307PredeclaredSelectorSpaceMaterialized,
        $"phase307Passed={phase307Passed}; phase307TargetBlind={phase307TargetBlind}; definitionCount={sourceDefinitions.Length}; selectionLawCount={selectionLawAudits.Length}; summaryDefinitionCount={JsonInt(phase307Summary.RootElement, "definitionCount")}; summarySelectionLawCount={JsonInt(phase307Summary.RootElement, "selectionLawCount")}"),
    Check(
        "phase379-381-382-sidecar-context-preserved",
        phase379SidecarConflictContextMaterialized && phase381ConflictMaterialized && phase382ProjectionBlockerMaterialized,
        $"phase379Context={phase379SidecarConflictContextMaterialized}; suppressedAxis={suppressedGaugeAxis}; dominantAxes={string.Join(",", phase379DominantGaugeAxes)}; phase381Conflict={phase381ConflictMaterialized}; phase382ProjectionBlocker={phase382ProjectionBlockerMaterialized}"),
    Check(
        "all-phase307-w-definitions-use-suppressed-canonical-axis",
        everyPhase307WDefinitionUsesSuppressedGaugeAxis && everyPhase307WDefinitionUsesCanonicalChargedAxes,
        $"definitionCount={sourceDefinitions.Length}; wDefinitionsUsingSuppressedAxis={wDefinitionsUsingSuppressedAxis.Length}; wDefinitionsAvoidingSuppressedAxis={wDefinitionsAvoidingSuppressedAxis.Length}; wDefinitionsUsingCanonicalChargedAxes={wDefinitionsUsingCanonicalChargedAxes.Length}; zDefinitionsUsingSuppressedAxis={zDefinitionsUsingSuppressedAxis.Length}"),
    Check(
        "all-phase307-predeclared-selectors-use-suppressed-axis",
        everyPredeclaredSelectorUsesSuppressedAxis && noPredeclaredSelectorAvoidsSuppressedAxis,
        $"selectionLawCount={selectionLawAudits.Length}; selectorsWithWNotUsingSuppressedAxis={selectorsWithWNotUsingSuppressedAxis.Length}; selectorsAvoidingSuppressedAxis={selectorsAvoidingSuppressedAxis.Length}; selectedLaws={string.Join(";", selectionLawAudits.Select(selection => $"{selection.LawId}:{string.Join(",", selection.SelectedWGaugeAxes)}"))}"),
    Check(
        "no-phase307-passing-selector-avoids-suppressed-axis",
        noP302ScaledStableCommonSelectorAvoidsSuppressedAxis
            && noRawStableCommonSelectorAvoidsSuppressedAxis
            && noSidecarCompatiblePredeclaredSelector,
        $"p302ScaledStableCommonSelectionLaw={p302ScaledStableCommonSelectionLaw?.LawId}; p302ScaledStableCommonSelectionWUsesSuppressedAxis={p302ScaledStableCommonSelectionLaw?.SelectedWUsesSuppressedGaugeAxis}; selectorsP302ScaledStableCommonAvoidingSuppressedAxis={selectorsP302ScaledStableCommonAvoidingSuppressedAxis.Length}; selectorsRawStableCommonAvoidingSuppressedAxis={selectorsRawStableCommonAvoidingSuppressedAxis.Length}; sidecarCompatibleSelectionLaws={sidecarCompatibleSelectionLaws.Length}"),
    Check(
        "no-contract-or-prediction-promotion",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount
            && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; canFillPhase201WzContract={canFillPhase201WzContract}; canFillPhase201HiggsContract={canFillPhase201HiggsContract}; canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}; routePromotesWzMasses={routePromotesWzMasses}; routePromotesHiggsMass={routePromotesHiggsMass}; routeCompletesBosonPredictions={routeCompletesBosonPredictions}; phase201TemplateMutated={phase201TemplateMutated}; acceptedContractFieldCount={acceptedContractFieldCount}; blockedContractFieldCount={blockedContractFieldCount}; phase213WzMissingFieldCount={phase213WzMissingFieldCount}"),
};

bool phase307SuppressedAxisCounterfactualSelectorAuditPassed = checks.All(check => check.Passed);
string terminalStatus = phase307SuppressedAxisCounterfactualSelectorAuditPassed
    ? "phase307-suppressed-axis-counterfactual-selector-audit-no-alternate-predeclared-selector"
    : "phase307-suppressed-axis-counterfactual-selector-audit-review-required";
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase383-phase307-suppressed-axis-counterfactual-selector-audit-v1",
    $"phase307Path={Phase307Path}",
    $"phase307SummaryPath={Phase307SummaryPath}",
    $"phase379Path={Phase379Path}",
    $"phase381Path={Phase381Path}",
    $"phase382Path={Phase382Path}",
    $"suppressedGaugeAxis={suppressedGaugeAxis}",
    $"phase381Hash={JsonString(phase381.RootElement, "targetBlindConstructionHash")}",
    $"phase382Hash={JsonString(phase382.RootElement, "targetBlindConstructionHash")}",
    "physicalTargetsConsultedForConstruction=false"));
string decision = phase307SuppressedAxisCounterfactualSelectorAuditPassed
    ? "No current Phase307 predeclared W/Z selector can avoid the Phase379-suppressed axis. All persisted Phase307 W source definitions use the canonical charged axes 0 and 1, so every predeclared selector's W row uses suppressed carrier axis 1. The Phase381 conflict therefore cannot be repaired inside the existing Phase307 selector space; progress requires a new source-definition theorem, an observed electroweak projection/namespace map, or a derivation that makes the suppressed axis physically required."
    : "Review the Phase307 suppressed-axis counterfactual selector audit before claiming the Phase379 sidecar conflict exhausts the existing selector space.";

var result = new
{
    phaseId = "phase383-phase307-suppressed-axis-counterfactual-selector-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    phase307SuppressedAxisCounterfactualSelectorAuditPassed,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    targetBlindConstructionHash,
    applicationSubjectKind = "phase307-predeclared-selector-space-under-phase379-suppressed-axis-counterfactual",
    phase307SelectorSpace = new
    {
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase307PredeclaredSelectorSpaceMaterialized,
        definitionCount = sourceDefinitions.Length,
        selectionLawCount = selectionLawAudits.Length,
        p302ScaledStableCommonSelectionLawCount = JsonInt(phase307Summary.RootElement, "p302ScaledStableCommonSelectionLawCount"),
        rawStableCommonSelectionLawCount = JsonInt(phase307Summary.RootElement, "rawStableCommonSelectionLawCount"),
        canFillPhase201WzContract = JsonBool(phase307Summary.RootElement, "canFillPhase201WzContract"),
    },
    sidecarContext = new
    {
        phase379Path = Phase379Path,
        phase381Path = Phase381Path,
        phase382Path = Phase382Path,
        suppressedGaugeAxis,
        dominantGaugeAxes = phase379DominantGaugeAxes,
        phase379SidecarConflictContextMaterialized,
        phase381ConflictMaterialized,
        phase382ProjectionBlockerMaterialized,
    },
    sourceDefinitionAxisAudit = new
    {
        sourceDefinitions,
        everyPhase307WDefinitionUsesSuppressedGaugeAxis,
        everyPhase307WDefinitionUsesCanonicalChargedAxes,
        wDefinitionsUsingSuppressedAxisCount = wDefinitionsUsingSuppressedAxis.Length,
        wDefinitionsAvoidingSuppressedAxisCount = wDefinitionsAvoidingSuppressedAxis.Length,
        wDefinitionsUsingCanonicalChargedAxesCount = wDefinitionsUsingCanonicalChargedAxes.Length,
        zDefinitionsUsingSuppressedAxisCount = zDefinitionsUsingSuppressedAxis.Length,
    },
    selectionLawAxisAudit = new
    {
        selectionLawAudits,
        everyPredeclaredSelectorUsesSuppressedAxis,
        noPredeclaredSelectorAvoidsSuppressedAxis,
        noP302ScaledStableCommonSelectorAvoidsSuppressedAxis,
        noRawStableCommonSelectorAvoidsSuppressedAxis,
        noSidecarCompatiblePredeclaredSelector,
        selectorsAvoidingSuppressedAxisCount = selectorsAvoidingSuppressedAxis.Length,
        selectorsWithWNotUsingSuppressedAxisCount = selectorsWithWNotUsingSuppressedAxis.Length,
        selectorsP302ScaledStableCommonAvoidingSuppressedAxisCount = selectorsP302ScaledStableCommonAvoidingSuppressedAxis.Length,
        selectorsRawStableCommonAvoidingSuppressedAxisCount = selectorsRawStableCommonAvoidingSuppressedAxis.Length,
        sidecarCompatibleSelectionLawCount = sidecarCompatibleSelectionLaws.Length,
        p302ScaledStableCommonSelectionLaw,
    },
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
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
        phase213Path = Phase213Path,
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase379Path = Phase379Path,
        phase381Path = Phase381Path,
        phase382Path = Phase382Path,
    },
    nextRequiredArtifact = new[]
    {
        "A new theorem-backed W source-definition family that does not require the Phase379-suppressed carrier axis, if that is physically intended.",
        "A GU-native observed electroweak projection/namespace map separating diagnostic carrier axes from physical photon/W/Z axes.",
        "A derivation explaining why the Phase307 canonical charged-ladder W row must use the Phase379-suppressed axis despite the response-image diagnostic.",
        "Separate W and Z Phase201 source rows with source-lineage ids, raw/common/target gates, stability sidecars, derivation ids, and GeV normalization.",
    },
    decision,
};

File.WriteAllText(
    Path.Combine(outputDir, "phase307_suppressed_axis_counterfactual_selector_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase307_suppressed_axis_counterfactual_selector_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase307SuppressedAxisCounterfactualSelectorAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        phase307PredeclaredSelectorSpaceMaterialized,
        sourceDefinitionCount = sourceDefinitions.Length,
        selectionLawCount = selectionLawAudits.Length,
        phase379SuppressedGaugeAxis = suppressedGaugeAxis,
        phase379DominantGaugeAxes = phase379DominantGaugeAxes,
        phase379SidecarConflictContextMaterialized,
        phase381ConflictMaterialized,
        phase382ProjectionBlockerMaterialized,
        everyPhase307WDefinitionUsesSuppressedGaugeAxis,
        everyPhase307WDefinitionUsesCanonicalChargedAxes,
        wDefinitionsUsingSuppressedAxisCount = wDefinitionsUsingSuppressedAxis.Length,
        wDefinitionsAvoidingSuppressedAxisCount = wDefinitionsAvoidingSuppressedAxis.Length,
        zDefinitionsUsingSuppressedAxisCount = zDefinitionsUsingSuppressedAxis.Length,
        everyPredeclaredSelectorUsesSuppressedAxis,
        noPredeclaredSelectorAvoidsSuppressedAxis,
        noP302ScaledStableCommonSelectorAvoidsSuppressedAxis,
        noRawStableCommonSelectorAvoidsSuppressedAxis,
        noSidecarCompatiblePredeclaredSelector,
        selectorsAvoidingSuppressedAxisCount = selectorsAvoidingSuppressedAxis.Length,
        selectorsWithWNotUsingSuppressedAxisCount = selectorsWithWNotUsingSuppressedAxis.Length,
        selectorsP302ScaledStableCommonAvoidingSuppressedAxisCount = selectorsP302ScaledStableCommonAvoidingSuppressedAxis.Length,
        selectorsRawStableCommonAvoidingSuppressedAxisCount = selectorsRawStableCommonAvoidingSuppressedAxis.Length,
        sidecarCompatibleSelectionLawCount = sidecarCompatibleSelectionLaws.Length,
        p302ScaledStableCommonSelectionLawId = p302ScaledStableCommonSelectionLaw?.LawId,
        p302ScaledStableCommonSelectionLawWUsesSuppressedAxis = p302ScaledStableCommonSelectionLaw?.SelectedWUsesSuppressedGaugeAxis,
        p302ScaledStableCommonSelectionLawAssessmentId = p302ScaledStableCommonSelectionLaw?.SelectedAssessmentId,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
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
Console.WriteLine($"phase307SuppressedAxisCounterfactualSelectorAuditPassed={phase307SuppressedAxisCounterfactualSelectorAuditPassed}");
Console.WriteLine($"sourceDefinitionCount={sourceDefinitions.Length}");
Console.WriteLine($"selectionLawCount={selectionLawAudits.Length}");
Console.WriteLine($"phase379SuppressedGaugeAxis={suppressedGaugeAxis}");
Console.WriteLine($"phase379DominantGaugeAxes={string.Join(",", phase379DominantGaugeAxes)}");
Console.WriteLine($"wDefinitionsAvoidingSuppressedAxisCount={wDefinitionsAvoidingSuppressedAxis.Length}");
Console.WriteLine($"selectorsAvoidingSuppressedAxisCount={selectorsAvoidingSuppressedAxis.Length}");
Console.WriteLine($"selectorsP302ScaledStableCommonAvoidingSuppressedAxisCount={selectorsP302ScaledStableCommonAvoidingSuppressedAxis.Length}");
Console.WriteLine($"sidecarCompatibleSelectionLawCount={sidecarCompatibleSelectionLaws.Length}");
Console.WriteLine($"p302ScaledStableCommonSelectionLawId={p302ScaledStableCommonSelectionLaw?.LawId}");
Console.WriteLine($"p302ScaledStableCommonSelectionLawWUsesSuppressedAxis={p302ScaledStableCommonSelectionLaw?.SelectedWUsesSuppressedGaugeAxis}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

int[] AxisUsage(IReadOnlyList<string> candidateIds)
{
    var axes = new List<int>();
    if (candidateIds.Any(chargedAxis0Set.Contains))
        axes.Add(0);
    if (candidateIds.Any(chargedAxis1Set.Contains))
        axes.Add(1);
    if (candidateIds.Any(neutralAxisSet.Contains))
        axes.Add(2);
    return axes.Distinct().Order().ToArray();
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
sealed record SourceDefinitionAxisAudit(
    string DefinitionId,
    string? Kind,
    IReadOnlyList<string> WCandidateIds,
    IReadOnlyList<string> ZCandidateIds,
    IReadOnlyList<int> WGaugeAxes,
    IReadOnlyList<int> ZGaugeAxes,
    string? Description);
sealed record SelectionLawAxisAudit(
    string LawId,
    string? Description,
    bool UsesP302ScaleForSelection,
    bool UsesTargetsForSelection,
    string? SelectionPopulation,
    bool SelectedRawStableCommonPassed,
    bool SelectedP302ScaledStableCommonPassed,
    bool CanFillPhase201WzContract,
    string SelectedAssessmentId,
    string SelectedWDefinitionId,
    string SelectedZDefinitionId,
    IReadOnlyList<int> SelectedWGaugeAxes,
    IReadOnlyList<int> SelectedZGaugeAxes,
    bool SelectedWUsesSuppressedGaugeAxis,
    bool SelectedZUsesSuppressedGaugeAxis,
    bool SelectedAvoidsSuppressedGaugeAxis,
    bool SelectedSidecarCompatible,
    double? SelectedP302ScaledCommonMeanRelativeSpread,
    double? SelectedMinP302ScaledRowRawToTargetRatio);
