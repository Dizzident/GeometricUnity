using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase382_response_image_observed_projection_requirement_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase311Path = "studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output/completion_observed_sector_wz_row_selector_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase320Path = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output/standard_electroweak_ladder_normalization_boundary_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase381Path = "studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/output/phase302_307_response_image_selector_compatibility_audit_summary.json";
const int ExpectedObservedFieldRequiredCount = 20;
const int ExpectedWzMissingFieldCount = 15;

var outputDir = Environment.GetEnvironmentVariable("PHASE382_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase311 = JsonDocument.Parse(File.ReadAllText(Phase311Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase320 = JsonDocument.Parse(File.ReadAllText(Phase320Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase381 = JsonDocument.Parse(File.ReadAllText(Phase381Path));

var observedProjectionFieldIds = new[]
{
    "observedFieldExtractionTheoremId",
    "electroweakGaugeEmbeddingId",
    "quadraticElectroweakMassOperatorId",
    "photonEigenstateProjectionId",
    "wBosonSourceRowId",
    "zBosonSourceRowId",
    "targetBlindConstructionHash",
};
var observedProjectionFieldResults = observedProjectionFieldIds
    .Select(fieldId => Phase295Field(fieldId))
    .ToArray();

bool phase381ConflictMaterialized =
    JsonBool(phase381.RootElement, "phase302307ResponseImageSelectorCompatibilityAuditPassed") is true
    && JsonBool(phase381.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase381.RootElement, "physicalTargetsConsultedForConstruction") is false
    && JsonString(phase381.RootElement, "phase307SelectedNearPassLawId") == "p302-scaled-max-min-magnitude"
    && JsonBool(phase381.RootElement, "selectedWUsesSuppressedGaugeAxis") is true
    && JsonBool(phase381.RootElement, "selectedZUsesSuppressedGaugeAxis") is false
    && JsonBool(phase381.RootElement, "selectedAnyRowUsesSuppressedGaugeAxis") is true
    && JsonBool(phase381.RootElement, "selectedWUsesCanonicalChargedAxes") is true
    && JsonBool(phase381.RootElement, "selectedZUsesDominantNeutralAxis") is true
    && JsonInt(phase381.RootElement, "phase379SuppressedGaugeAxis") == 1
    && JsonIntArray(phase381.RootElement, "phase379DominantGaugeAxes").SequenceEqual(new[] { 0, 2 })
    && JsonBool(phase381.RootElement, "responseImageSelectorSidecarCompatible") is false
    && JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent") is true
    && JsonBool(phase381.RootElement, "canFillPhase201WzContract") is false;

bool phase256ContractUnfilled =
    JsonInt(phase256.RootElement, "requiredFieldCount") == ExpectedObservedFieldRequiredCount
    && JsonInt(phase256.RootElement, "filledRequiredFieldCount") == 0
    && JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is false;

bool phase295NoIntakeReadyObservedProjection =
    JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true
    && JsonInt(phase295.RootElement, "contractFieldCount") == ExpectedObservedFieldRequiredCount
    && JsonInt(phase295.RootElement, "fieldsWithIntakeReadyCandidateCount") == 0
    && JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") == 0
    && JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is false
    && observedProjectionFieldResults.All(field => field.IntakeReadyCandidateCount == 0 && !field.Filled);

bool phase311ObservedMapAbsent =
    JsonBool(phase311.RootElement, "completionObservedSectorWzRowSelectorAuditPassed") is true
    && JsonBool(phase311.RootElement, "completionDraftProvidesCanonicalWzRowSelector") is false
    && JsonBool(phase311.RootElement, "completionDraftProvidesPhotonWzEigenstateProjectionRows") is false
    && JsonBool(phase311.RootElement, "completionDraftProvidesPhysicalWzObservableMap") is false
    && JsonBool(phase311.RootElement, "completionDraftProvidesBranchStableObservedWzRows") is false
    && JsonBool(phase311.RootElement, "completionDraftCanPromotePhase307Selector") is false
    && JsonBool(phase311.RootElement, "phase307RowsHaveObservedSectorMapId") is false
    && JsonBool(phase311.RootElement, "phase307RowsHaveElectroweakGaugeEmbeddingId") is false
    && JsonBool(phase311.RootElement, "phase307RowsHaveQuadraticElectroweakMassOperatorId") is false
    && JsonBool(phase311.RootElement, "phase307RowsHavePhotonMasslessGate") is false
    && JsonBool(phase311.RootElement, "phase307WRowsHavePhysicalEigenstateProjectionId") is false
    && JsonBool(phase311.RootElement, "phase307ZRowsHavePhysicalEigenstateProjectionId") is false
    && JsonBool(phase311.RootElement, "phase307ObservedProjectionBranchStable") is false
    && JsonBool(phase311.RootElement, "phase307ObservedProjectionTargetBlindHashPresent") is false
    && JsonBool(phase311.RootElement, "canFillPhase201WzContract") is false;

bool phase313OfficialProjectionMapAbsent =
    JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true
    && JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true
    && JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true
    && JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesElectromagneticUnbrokenGenerator") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesPhotonMasslessProjectionRow") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is false
    && JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is false
    && JsonBool(phase313.RootElement, "officialDraftProjectionMapCompletesObservedFieldExtraction") is false
    && JsonBool(phase313.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase313.RootElement, "canFillPhase201WzContract") is false;

bool phase320StandardBoundaryOnly =
    JsonBool(phase320.RootElement, "standardElectroweakNormalizationBoundaryAuditPassed") is true
    && JsonBool(phase320.RootElement, "standardWChargedLadderDefinitionAvailable") is true
    && JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing") is true
    && JsonBool(phase320.RootElement, "standardPhotonZRotationRequiresWeakMixingAngle") is true
    && JsonBool(phase320.RootElement, "standardElectroweakAlgebraPromotesDecoupledSelector") is false
    && JsonBool(phase320.RootElement, "standardElectroweakBoundaryPromotesWzMasses") is false
    && JsonBool(phase320.RootElement, "standardElectroweakBoundaryCompletesBosonPredictions") is false
    && JsonBool(phase320.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase320.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;

bool phase321NeutralMixingRouteUnpromotable =
    JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesSourceDerivedHyperchargeCoupling") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesPhotonMasslessProjection") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesZPhysicalProjectionRow") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteProvidesObservedEmbedding") is false
    && JsonBool(phase321.RootElement, "neutralMixingRoutePromotesWzMasses") is false
    && JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is false
    && JsonBool(phase321.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase321.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;

bool phase379ProvidesNoNamespaceMap =
    JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true
    && JsonBool(phase379.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is false
    && JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector") is false
    && JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap") is false
    && JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is false
    && JsonBool(phase379.RootElement, "routeProvidesSeparateWzSourceRows") is false
    && JsonBool(phase379.RootElement, "routeProvidesGeVUnitNormalization") is false
    && JsonBool(phase379.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase379.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;

bool phase201WzContractStillBlocked =
    JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is false
    && JsonInt(phase213.RootElement, "wzMissingFieldCount") == ExpectedWzMissingFieldCount
    && JsonInt(phase213.RootElement, "higgsMissingFieldCount") == 14;

bool observedCarrierAxisNamespaceSeparationMapPresent = false;
bool observedCarrierAxisNamespaceSeparationMapTargetIndependent = false;
bool observedCarrierAxisNamespaceSeparationMapSourceLineageEligible = false;
bool observedCarrierAxisNamespaceSeparationMapCanFillPhase256 = false;
bool observedCarrierAxisNamespaceSeparationMapCanFillPhase201Wz = false;
bool phase379CarrierAxesCanBeSeparatedFromPhysicalWzAxes = false;
bool phase307SelectedNearPassRehabilitatedByObservedProjectionMap = false;
bool responseImageConflictRemainsActive = true;
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
        "phase381-sidecar-conflict-materialized",
        phase381ConflictMaterialized,
        $"phase381Passed={JsonBool(phase381.RootElement, "phase302307ResponseImageSelectorCompatibilityAuditPassed")}; selectedLaw={JsonString(phase381.RootElement, "phase307SelectedNearPassLawId")}; wUsesSuppressedAxis={JsonBool(phase381.RootElement, "selectedWUsesSuppressedGaugeAxis")}; zUsesSuppressedAxis={JsonBool(phase381.RootElement, "selectedZUsesSuppressedGaugeAxis")}; suppressedAxis={JsonInt(phase381.RootElement, "phase379SuppressedGaugeAxis")}; dominantAxes={string.Join(",", JsonIntArray(phase381.RootElement, "phase379DominantGaugeAxes"))}; sidecarCompatible={JsonBool(phase381.RootElement, "responseImageSelectorSidecarCompatible")}; sidecarConflict={JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent")}"),
    Check(
        "observed-field-contract-and-scanner-remain-unfilled",
        phase256ContractUnfilled && phase295NoIntakeReadyObservedProjection,
        $"phase256RequiredFields={JsonInt(phase256.RootElement, "requiredFieldCount")}; phase256FilledRequiredFields={JsonInt(phase256.RootElement, "filledRequiredFieldCount")}; phase256Promotable={JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable")}; phase295IntakeReadyCount={JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount")}; fields={string.Join(";", observedProjectionFieldResults.Select(field => $"{field.FieldId}:{field.IntakeReadyCandidateCount}/{field.Filled}"))}"),
    Check(
        "completion-and-official-draft-do-not-supply-observed-projection-map",
        phase311ObservedMapAbsent && phase313OfficialProjectionMapAbsent,
        $"phase311PhysicalMap={JsonBool(phase311.RootElement, "completionDraftProvidesPhysicalWzObservableMap")}; phase311WProjection={JsonBool(phase311.RootElement, "phase307WRowsHavePhysicalEigenstateProjectionId")}; phase311ZProjection={JsonBool(phase311.RootElement, "phase307ZRowsHavePhysicalEigenstateProjectionId")}; phase313WRows={JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows")}; phase313ZRow={JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection")}; phase313ObservedEmbedding={JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding")}"),
    Check(
        "standard-and-neutral-mixing-boundaries-do-not-promote-gu-map",
        phase320StandardBoundaryOnly && phase321NeutralMixingRouteUnpromotable,
        $"phase320StandardWChargedLadderDefinitionAvailable={JsonBool(phase320.RootElement, "standardWChargedLadderDefinitionAvailable")}; phase320ZRequiresMixing={JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing")}; phase320PromotesSelector={JsonBool(phase320.RootElement, "standardElectroweakAlgebraPromotesDecoupledSelector")}; phase321ObservedEmbedding={JsonBool(phase321.RootElement, "neutralMixingRouteProvidesObservedEmbedding")}; phase321ZProjection={JsonBool(phase321.RootElement, "neutralMixingRouteProvidesZPhysicalProjectionRow")}"),
    Check(
        "phase379-response-image-does-not-provide-namespace-separation-map",
        phase379ProvidesNoNamespaceMap,
        $"routeProvidesCanonicalGaugeAxisSelector={JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector")}; routeProvidesObservedElectroweakFieldMap={JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap")}; routeProvidesDirectWzLaw={JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw")}; routeProvidesSeparateWzRows={JsonBool(phase379.RootElement, "routeProvidesSeparateWzSourceRows")}; canFillPhase256={JsonBool(phase379.RootElement, "canFillPhase256ObservedFieldExtractionContract")}; canFillPhase201Wz={JsonBool(phase379.RootElement, "canFillPhase201WzContract")}"),
    Check(
        "no-rehabilitation-or-contract-promotion",
        !observedCarrierAxisNamespaceSeparationMapPresent
            && !observedCarrierAxisNamespaceSeparationMapTargetIndependent
            && !observedCarrierAxisNamespaceSeparationMapSourceLineageEligible
            && !observedCarrierAxisNamespaceSeparationMapCanFillPhase256
            && !observedCarrierAxisNamespaceSeparationMapCanFillPhase201Wz
            && !phase379CarrierAxesCanBeSeparatedFromPhysicalWzAxes
            && !phase307SelectedNearPassRehabilitatedByObservedProjectionMap
            && responseImageConflictRemainsActive
            && phase201WzContractStillBlocked
            && !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount,
        $"namespaceMapPresent={observedCarrierAxisNamespaceSeparationMapPresent}; targetIndependent={observedCarrierAxisNamespaceSeparationMapTargetIndependent}; sourceLineageEligible={observedCarrierAxisNamespaceSeparationMapSourceLineageEligible}; separatesAxes={phase379CarrierAxesCanBeSeparatedFromPhysicalWzAxes}; rehabilitatesPhase307={phase307SelectedNearPassRehabilitatedByObservedProjectionMap}; responseImageConflictRemainsActive={responseImageConflictRemainsActive}; phase201Blocked={phase201WzContractStillBlocked}; acceptedFields={acceptedContractFieldCount}; blockedFields={blockedContractFieldCount}"),
};

bool responseImageObservedProjectionRequirementAuditPassed = checks.All(check => check.Passed);
string terminalStatus = responseImageObservedProjectionRequirementAuditPassed
    ? "response-image-observed-projection-requirement-audit-no-namespace-separation-map"
    : "response-image-observed-projection-requirement-audit-review-required";
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase382-response-image-observed-projection-requirement-audit-v1",
    $"phase256Path={Phase256Path}",
    $"phase295Path={Phase295Path}",
    $"phase311Path={Phase311Path}",
    $"phase313Path={Phase313Path}",
    $"phase320Path={Phase320Path}",
    $"phase321Path={Phase321Path}",
    $"phase379Path={Phase379Path}",
    $"phase381Path={Phase381Path}",
    $"phase381Hash={JsonString(phase381.RootElement, "targetBlindConstructionHash")}",
    "physicalTargetsConsultedForConstruction=false"));
string decision = responseImageObservedProjectionRequirementAuditPassed
    ? "No current artifact provides the observed electroweak projection map needed to treat Phase379 carrier axes as a separate namespace from physical W/Z axes. The Phase381 conflict therefore remains active: the selected W row uses the Phase379-suppressed axis, while the existing standard and neutral-mixing audits only state external dependency boundaries and cannot rehabilitate the selector or fill Phase201."
    : "Review the observed-projection requirement audit before using Phase379 carrier-axis data to reinterpret physical W/Z selector axes.";

var result = new
{
    phaseId = "phase382-response-image-observed-projection-requirement-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    responseImageObservedProjectionRequirementAuditPassed,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    targetBlindConstructionHash,
    applicationSubjectKind = "phase381-sidecar-conflict-rehabilitation-requirement",
    phase381Conflict = new
    {
        phase381Path = Phase381Path,
        phase381ConflictMaterialized,
        selectedLaw = JsonString(phase381.RootElement, "phase307SelectedNearPassLawId"),
        selectedWGaugeAxes = JsonIntArray(phase381.RootElement, "selectedWGaugeAxes"),
        selectedZGaugeAxes = JsonIntArray(phase381.RootElement, "selectedZGaugeAxes"),
        selectedWUsesSuppressedGaugeAxis = JsonBool(phase381.RootElement, "selectedWUsesSuppressedGaugeAxis"),
        selectedZUsesSuppressedGaugeAxis = JsonBool(phase381.RootElement, "selectedZUsesSuppressedGaugeAxis"),
        phase379SuppressedGaugeAxis = JsonInt(phase381.RootElement, "phase379SuppressedGaugeAxis"),
        phase379DominantGaugeAxes = JsonIntArray(phase381.RootElement, "phase379DominantGaugeAxes"),
        responseImageSelectorSidecarCompatible = JsonBool(phase381.RootElement, "responseImageSelectorSidecarCompatible"),
        responseImageSidecarConflictPresent = JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent"),
    },
    observedProjectionEvidence = new
    {
        phase256Path = Phase256Path,
        phase256ContractUnfilled,
        observedFieldRequiredCount = JsonInt(phase256.RootElement, "requiredFieldCount"),
        observedFieldFilledRequiredCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount"),
        observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable"),
        phase295Path = Phase295Path,
        phase295NoIntakeReadyObservedProjection,
        phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount"),
        phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract"),
        observedProjectionFieldResults,
    },
    priorMapAudits = new
    {
        phase311Path = Phase311Path,
        phase311ObservedMapAbsent,
        completionDraftProvidesPhysicalWzObservableMap = JsonBool(phase311.RootElement, "completionDraftProvidesPhysicalWzObservableMap"),
        phase307WRowsHavePhysicalEigenstateProjectionId = JsonBool(phase311.RootElement, "phase307WRowsHavePhysicalEigenstateProjectionId"),
        phase307ZRowsHavePhysicalEigenstateProjectionId = JsonBool(phase311.RootElement, "phase307ZRowsHavePhysicalEigenstateProjectionId"),
        phase313Path = Phase313Path,
        phase313OfficialProjectionMapAbsent,
        officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows"),
        officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection"),
        officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding"),
        phase320Path = Phase320Path,
        phase320StandardBoundaryOnly,
        standardWChargedLadderDefinitionAvailable = JsonBool(phase320.RootElement, "standardWChargedLadderDefinitionAvailable"),
        standardZRequiresNeutralSu2U1Mixing = JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing"),
        standardElectroweakAlgebraPromotesDecoupledSelector = JsonBool(phase320.RootElement, "standardElectroweakAlgebraPromotesDecoupledSelector"),
        phase321Path = Phase321Path,
        phase321NeutralMixingRouteUnpromotable,
        neutralMixingRouteProvidesPhotonMasslessProjection = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesPhotonMasslessProjection"),
        neutralMixingRouteProvidesZPhysicalProjectionRow = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesZPhysicalProjectionRow"),
        neutralMixingRouteProvidesObservedEmbedding = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesObservedEmbedding"),
    },
    responseImageMapBoundary = new
    {
        phase379Path = Phase379Path,
        phase379ProvidesNoNamespaceMap,
        stableSuppressedGaugeAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis"),
        routeProvidesCanonicalGaugeAxisSelector = JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector"),
        routeProvidesObservedElectroweakFieldMap = JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap"),
        routeProvidesDirectTargetIndependentWzBridgeSourceLaw = JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw"),
        routeProvidesSeparateWzSourceRows = JsonBool(phase379.RootElement, "routeProvidesSeparateWzSourceRows"),
        routeProvidesGeVUnitNormalization = JsonBool(phase379.RootElement, "routeProvidesGeVUnitNormalization"),
    },
    namespaceSeparationAssessment = new
    {
        observedCarrierAxisNamespaceSeparationMapPresent,
        observedCarrierAxisNamespaceSeparationMapTargetIndependent,
        observedCarrierAxisNamespaceSeparationMapSourceLineageEligible,
        observedCarrierAxisNamespaceSeparationMapCanFillPhase256,
        observedCarrierAxisNamespaceSeparationMapCanFillPhase201Wz,
        phase379CarrierAxesCanBeSeparatedFromPhysicalWzAxes,
        phase307SelectedNearPassRehabilitatedByObservedProjectionMap,
        responseImageConflictRemainsActive,
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
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase295Path = Phase295Path,
        phase311Path = Phase311Path,
        phase313Path = Phase313Path,
        phase320Path = Phase320Path,
        phase321Path = Phase321Path,
        phase379Path = Phase379Path,
        phase381Path = Phase381Path,
    },
    nextRequiredArtifact = new[]
    {
        "A GU-native observed electroweak projection map that maps diagnostic carrier axes into physical photon/W/Z axes before target comparison.",
        "A source-lineage-eligible namespace-separation theorem explaining why Phase379 carrier-axis suppression should not apply to the Phase307 charged-ladder W row.",
        "Branch-stable photon, W, and Z projection/source rows with target-blind construction hashes, raw/common gates, and GeV normalization.",
    },
    decision,
};

File.WriteAllText(
    Path.Combine(outputDir, "response_image_observed_projection_requirement_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "response_image_observed_projection_requirement_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.responseImageObservedProjectionRequirementAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        phase381ConflictMaterialized,
        phase381SelectedLaw = result.phase381Conflict.selectedLaw,
        phase381SelectedWGaugeAxes = result.phase381Conflict.selectedWGaugeAxes,
        phase381SelectedZGaugeAxes = result.phase381Conflict.selectedZGaugeAxes,
        phase381SelectedWUsesSuppressedGaugeAxis = result.phase381Conflict.selectedWUsesSuppressedGaugeAxis,
        phase381SelectedZUsesSuppressedGaugeAxis = result.phase381Conflict.selectedZUsesSuppressedGaugeAxis,
        phase379SuppressedGaugeAxis = result.phase381Conflict.phase379SuppressedGaugeAxis,
        phase379DominantGaugeAxes = result.phase381Conflict.phase379DominantGaugeAxes,
        phase381ResponseImageSelectorSidecarCompatible = result.phase381Conflict.responseImageSelectorSidecarCompatible,
        phase381ResponseImageSidecarConflictPresent = result.phase381Conflict.responseImageSidecarConflictPresent,
        phase256ContractUnfilled,
        phase295NoIntakeReadyObservedProjection,
        phase311ObservedMapAbsent,
        phase313OfficialProjectionMapAbsent,
        phase320StandardBoundaryOnly,
        phase321NeutralMixingRouteUnpromotable,
        phase379ProvidesNoNamespaceMap,
        result.namespaceSeparationAssessment.observedCarrierAxisNamespaceSeparationMapPresent,
        result.namespaceSeparationAssessment.observedCarrierAxisNamespaceSeparationMapTargetIndependent,
        result.namespaceSeparationAssessment.observedCarrierAxisNamespaceSeparationMapSourceLineageEligible,
        result.namespaceSeparationAssessment.observedCarrierAxisNamespaceSeparationMapCanFillPhase256,
        result.namespaceSeparationAssessment.observedCarrierAxisNamespaceSeparationMapCanFillPhase201Wz,
        result.namespaceSeparationAssessment.phase379CarrierAxesCanBeSeparatedFromPhysicalWzAxes,
        result.namespaceSeparationAssessment.phase307SelectedNearPassRehabilitatedByObservedProjectionMap,
        result.namespaceSeparationAssessment.responseImageConflictRemainsActive,
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
Console.WriteLine($"responseImageObservedProjectionRequirementAuditPassed={responseImageObservedProjectionRequirementAuditPassed}");
Console.WriteLine($"phase381ConflictMaterialized={phase381ConflictMaterialized}");
Console.WriteLine($"phase295NoIntakeReadyObservedProjection={phase295NoIntakeReadyObservedProjection}");
Console.WriteLine($"phase311ObservedMapAbsent={phase311ObservedMapAbsent}");
Console.WriteLine($"phase313OfficialProjectionMapAbsent={phase313OfficialProjectionMapAbsent}");
Console.WriteLine($"phase379ProvidesNoNamespaceMap={phase379ProvidesNoNamespaceMap}");
Console.WriteLine($"observedCarrierAxisNamespaceSeparationMapPresent={observedCarrierAxisNamespaceSeparationMapPresent}");
Console.WriteLine($"phase307SelectedNearPassRehabilitatedByObservedProjectionMap={phase307SelectedNearPassRehabilitatedByObservedProjectionMap}");
Console.WriteLine($"responseImageConflictRemainsActive={responseImageConflictRemainsActive}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

FieldResult Phase295Field(string fieldId)
{
    if (!phase295.RootElement.TryGetProperty("fieldResults", out var fieldResults)
        || fieldResults.ValueKind != JsonValueKind.Array)
    {
        return new FieldResult(fieldId, 0, -1, false);
    }

    foreach (var field in fieldResults.EnumerateArray())
    {
        if (JsonString(field, "fieldId") == fieldId)
        {
            return new FieldResult(
                fieldId,
                JsonInt(field, "candidateLineCount") ?? 0,
                JsonInt(field, "intakeReadyCandidateCount") ?? 0,
                JsonBool(field, "filled") is true);
        }
    }

    return new FieldResult(fieldId, 0, -1, false);
}

static Check Check(string checkId, bool passed, string details) => new(checkId, passed, details);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static int[] JsonIntArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out _))
            .Select(item => item.GetInt32())
            .ToArray()
        : Array.Empty<int>();

static string HashText(string text)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record Check(string CheckId, bool Passed, string Details);
sealed record FieldResult(string FieldId, int CandidateLineCount, int IntakeReadyCandidateCount, bool Filled);
