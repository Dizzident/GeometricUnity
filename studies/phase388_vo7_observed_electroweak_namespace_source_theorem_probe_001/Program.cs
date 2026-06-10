using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase370Path = "studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/output/completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_summary.json";
const string Phase372Path = "studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json";
const string Phase378Path = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output/full_connection_carrier_shell_response_gram_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase381Path = "studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/output/phase302_307_response_image_selector_compatibility_audit_summary.json";
const string Phase382Path = "studies/phase382_response_image_observed_projection_requirement_audit_001/output/response_image_observed_projection_requirement_audit_summary.json";
const string Phase383Path = "studies/phase383_phase307_suppressed_axis_counterfactual_selector_audit_001/output/phase307_suppressed_axis_counterfactual_selector_audit_summary.json";
const string Phase384Path = "studies/phase384_phase307_basis_energy_response_image_proxy_audit_001/output/phase307_basis_energy_response_image_proxy_audit_summary.json";
const string Phase385Path = "studies/phase385_observed_electroweak_namespace_map_intake_audit_001/output/observed_electroweak_namespace_map_intake_audit_summary.json";
const string Phase387Path = "studies/phase387_current_cox_first_principles_i_full_text_contract_audit_001/output/current_cox_first_principles_i_full_text_contract_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;
const int ExpectedHiggsMissingFieldCount = 14;

var outputDir = Environment.GetEnvironmentVariable("PHASE388_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase370 = JsonDocument.Parse(File.ReadAllText(Phase370Path));
using var phase372 = JsonDocument.Parse(File.ReadAllText(Phase372Path));
using var phase378 = JsonDocument.Parse(File.ReadAllText(Phase378Path));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase381 = JsonDocument.Parse(File.ReadAllText(Phase381Path));
using var phase382 = JsonDocument.Parse(File.ReadAllText(Phase382Path));
using var phase383 = JsonDocument.Parse(File.ReadAllText(Phase383Path));
using var phase384 = JsonDocument.Parse(File.ReadAllText(Phase384Path));
using var phase385 = JsonDocument.Parse(File.ReadAllText(Phase385Path));
using var phase387 = JsonDocument.Parse(File.ReadAllText(Phase387Path));

var phase201ContractMaterialized = JsonBool(phase201.RootElement, "intakeContractMaterialized") is true;
var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var phase213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var phase256ContractMaterialized = JsonBool(phase256.RootElement, "contractMaterialized") is true
    && JsonBool(phase256.RootElement, "observedFieldExtractionIntakeContractPassed") is true;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ObservedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var phase307Passed = JsonBool(phase307.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true;
var phase307TargetBlind = JsonBool(phase307.RootElement, "targetObservablesUsedForConstruction") is false;
var phase307NearPassPresent = JsonBool(phase307.RootElement, "numericalP302ScaledDecoupledNearPassPresent") is true;
var phase307SelectionLawCount = JsonInt(phase307.RootElement, "selectionLawCount") ?? -1;
var phase307P302ScaledStableCommonSelectionLawCount = JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount") ?? -1;
var phase307CanFillWz = JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true;

var phase370Passed = JsonBool(phase370.RootElement, "completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed") is true;
var phase370RecordsVo7 = JsonBool(phase370.RootElement, "completionRecordsVo7CoupledMixedLinearizationObligation") is true;
var phase370MixedBlocksPresent = JsonBool(phase370.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") is true;
var phase370ObservedRowsPresent = JsonBool(phase370.RootElement, "routeProvidesObservedPhotonWzHiggsProjectionRows") is true;
var phase370HiggsSourcePresent = JsonBool(phase370.RootElement, "routeProvidesHiggsScalarSourceOperator") is true;

var phase372Passed = JsonBool(phase372.RootElement, "discreteFermionicBilinearReciprocalMixedBlockAuditPassed") is true;
var phase372Vo7BuildingBlock = JsonBool(phase372.RootElement, "reciprocalDiscreteBilinearSourceBlockCandidateIsVo7BuildingBlock") is true;
var phase372CompletesVo7 = JsonBool(phase372.RootElement, "reciprocalDiscreteBilinearSourceBlockCandidateCompletesVo7") is true;
var phase372PhysicalMassPsiBranch = JsonBool(phase372.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") is true;
var phase372MeshVolumeBranchCompatible = JsonBool(phase372.RootElement, "meshVolumeWeightBranchCompatible") is true;

var phase378Passed = JsonBool(phase378.RootElement, "fullConnectionCarrierShellResponseGramAuditPassed") is true;
var phase378Rank = JsonInt(phase378.RootElement, "observedStableFullCarrierResponseRank") ?? -1;
var phase378PhysicalHessian = JsonBool(phase378.RootElement, "routeProvidesPhysicalEffectiveActionHessian") is true;
var phase378DirectWzLaw = JsonBool(phase378.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;

var phase379Passed = JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true;
var phase379StableTwoAxis = JsonBool(phase379.RootElement, "stableTwoGaugeAxisDominance") is true;
var phase379SuppressedAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis") ?? -1;
var phase379ObservedMap = JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap") is true;
var phase379CanonicalSelector = JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector") is true;
var phase379DirectWzLaw = JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;

var phase381Passed = JsonBool(phase381.RootElement, "phase302307ResponseImageSelectorCompatibilityAuditPassed") is true;
var phase381SelectedWUsesSuppressed = JsonBool(phase381.RootElement, "selectedWUsesSuppressedGaugeAxis") is true;
var phase381SidecarConflict = JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent") is true;
var phase381SidecarCompatible = JsonBool(phase381.RootElement, "responseImageSelectorSidecarCompatible") is true;
var phase381CanFillWz = JsonBool(phase381.RootElement, "canFillPhase201WzContract") is true;

var phase382Passed = JsonBool(phase382.RootElement, "responseImageObservedProjectionRequirementAuditPassed") is true;
var phase382NamespaceMapPresent = JsonBool(phase382.RootElement, "observedCarrierAxisNamespaceSeparationMapPresent") is true;
var phase382Rehabilitated = JsonBool(phase382.RootElement, "phase307SelectedNearPassRehabilitatedByObservedProjectionMap") is true;
var phase382CanFillWz = JsonBool(phase382.RootElement, "canFillPhase201WzContract") is true;

var phase383Passed = JsonBool(phase383.RootElement, "phase307SuppressedAxisCounterfactualSelectorAuditPassed") is true;
var phase383EveryWUsesSuppressed = JsonBool(phase383.RootElement, "everyPhase307WDefinitionUsesSuppressedGaugeAxis") is true;
var phase383NoSelectorAvoidsSuppressed = JsonBool(phase383.RootElement, "noPredeclaredSelectorAvoidsSuppressedAxis") is true;
var phase383SidecarCompatibleCount = JsonInt(phase383.RootElement, "sidecarCompatibleSelectionLawCount") ?? -1;
var phase383CanFillWz = JsonBool(phase383.RootElement, "canFillPhase201WzContract") is true;

var phase384Passed = JsonBool(phase384.RootElement, "phase307BasisEnergyResponseImageProxyAuditPassed") is true;
var phase384NoLowSuppressedProxy = JsonBool(phase384.RootElement, "noWDefinitionHasLowSuppressedBasisEnergyProxy") is true;
var phase384MinWSuppressedBasisEnergyFraction = JsonDouble(phase384.RootElement, "minWSuppressedBasisEnergyFraction") ?? double.NaN;
var phase384SelectedSuppressedBasisEnergyFraction = JsonDouble(phase384.RootElement, "selectedP302ScaledStableCommonWSuppressedBasisEnergyFraction") ?? double.NaN;
var phase384CanFillWz = JsonBool(phase384.RootElement, "canFillPhase201WzContract") is true;

var phase385Passed = JsonBool(phase385.RootElement, "observedElectroweakNamespaceMapIntakeAuditPassed") is true;
var phase385CandidateCount = JsonInt(phase385.RootElement, "candidateCount") ?? -1;
var phase385IntakeReadyCandidateCount = JsonInt(phase385.RootElement, "intakeReadyCandidateCount") ?? -1;
var phase385NoNamespaceCandidate = JsonBool(phase385.RootElement, "noCandidateProvidesGuNativeObservedElectroweakNamespaceMap") is true;
var phase385NoPhase256Candidate = JsonBool(phase385.RootElement, "noCandidateCanFillPhase256ObservedFieldExtractionContract") is true;
var phase385NoWzCandidate = JsonBool(phase385.RootElement, "noCandidateCanFillPhase201WzContract") is true;
var phase385NoHiggsCandidate = JsonBool(phase385.RootElement, "noCandidateCanFillPhase201HiggsContract") is true;

var phase387Passed = JsonBool(phase387.RootElement, "currentCoxFirstPrinciplesIFullTextContractAuditPassed") is true;
var phase387FullTextBosonContractEvidenceFound = JsonBool(phase387.RootElement, "fullTextBosonContractEvidenceFound") is true;

var targetBlindConstruction = true;
var physicalTargetsConsultedForConstruction = false;
var applicationSubjectKind = "vo7-shell-response-observed-electroweak-namespace-source-theorem-probe";

var theoremRequirements = new[]
{
    new Requirement("physical-m-psi-compatible-branch", phase372PhysicalMassPsiBranch && phase372MeshVolumeBranchCompatible, "A physical M_psi-compatible Dirac branch and matching modes are required before shell-response pairings can be physical."),
    new Requirement("completed-vo7-mixed-linearization", phase372CompletesVo7 && phase370MixedBlocksPresent, "The reciprocal block is a VO-7 building block; completed mixed-linearization blocks and gauge identities are still absent."),
    new Requirement("physical-effective-action-hessian", phase378PhysicalHessian, "The full carrier Gram is a study-defined Hilbert-Schmidt pullback, not a physical effective-action Hessian."),
    new Requirement("carrier-axis-to-observed-photon-w-z-h-map", phase379ObservedMap || phase382NamespaceMapPresent, "No current artifact maps carrier axes to observed photon/W/Z/H fields."),
    new Requirement("canonical-gauge-axis-or-observed-namespace-selector", phase379CanonicalSelector || phase382NamespaceMapPresent, "The response-image axis diagnostic is not a canonical electroweak selector."),
    new Requirement("w-row-source-theorem-explains-suppressed-axis", false, "The selected W row uses the Phase379-suppressed carrier axis and no current theorem explains why that is physically required."),
    new Requirement("phase307-selector-escape-from-suppressed-axis", !phase383EveryWUsesSuppressed && !phase383NoSelectorAvoidsSuppressed && phase383SidecarCompatibleCount > 0, "Every current Phase307 W definition uses the suppressed axis; no predeclared selector avoids it."),
    new Requirement("basis-energy-proxy-escape", !phase384NoLowSuppressedProxy && phase384MinWSuppressedBasisEnergyFraction <= 0.1, "Phase27 basis-energy metadata does not supply a low-suppressed-axis escape."),
    new Requirement("separate-w-z-source-rows", phase307CanFillWz || phase381CanFillWz || phase382CanFillWz || phase383CanFillWz || phase384CanFillWz, "No current W/Z source-row candidate can fill Phase201."),
    new Requirement("higgs-scalar-source-row", phase370HiggsSourcePresent, "The v29/VO-7 lane still lacks a solved Higgs scalar-source operator or row."),
    new Requirement("weak-angle-or-coupling-lineage", false, "No target-independent weak-angle or coupling lineage is supplied by the VO-7 shell-response chain."),
    new Requirement("vev-or-source-scale-lineage", false, "No target-independent electroweak VEV or source-scale lineage is supplied by the VO-7 shell-response chain."),
    new Requirement("pole-extraction-and-gev-normalization", false, "No observed-pole extraction or GeV unit lineage is supplied by the VO-7 shell-response chain."),
};

var missingTheoremRequirementCount = theoremRequirements.Count(requirement => !requirement.Present);
var candidateTheoremPresent = theoremRequirements.All(requirement => requirement.Present);
var vo7ObservedNamespaceSourceTheoremProbePassed = phase370Passed
    && phase370RecordsVo7
    && phase372Passed
    && phase372Vo7BuildingBlock
    && phase378Passed
    && phase378Rank == 3
    && phase379Passed
    && phase379StableTwoAxis
    && phase379SuppressedAxis == 1
    && phase381Passed
    && phase381SelectedWUsesSuppressed
    && phase381SidecarConflict
    && !phase381SidecarCompatible
    && phase382Passed
    && !phase382NamespaceMapPresent
    && !phase382Rehabilitated
    && phase383Passed
    && phase383EveryWUsesSuppressed
    && phase383NoSelectorAvoidsSuppressed
    && phase384Passed
    && phase384NoLowSuppressedProxy
    && phase385Passed
    && phase385NoNamespaceCandidate
    && phase387Passed
    && !phase387FullTextBosonContractEvidenceFound
    && missingTheoremRequirementCount > 0
    && !candidateTheoremPresent;

var sourceContractApplicationAllowed = false;
var canFillPhase201WzContract = false;
var canFillPhase201HiggsContract = false;
var canFillPhase256ObservedFieldExtractionContract = false;
var routePromotesWzMasses = false;
var routePromotesHiggsMass = false;
var routeCompletesBosonPredictions = false;
var phase201TemplateMutated = false;
var fieldsAppliedToPhase201TemplateCount = 0;
var acceptedContractFieldCount = 0;
var blockedContractFieldCount = phase213WzMissingFieldCount;
var phase201FieldsDefensiblyFilled = Array.Empty<string>();

var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    applicationSubjectKind,
    phase307SelectionLawCount,
    phase307P302ScaledStableCommonSelectionLawCount,
    phase378Rank,
    phase379SuppressedAxis,
    phase381SelectedWUsesSuppressed,
    phase383EveryWUsesSuppressed,
    phase384MinWSuppressedBasisEnergyFraction,
    theoremRequirements = theoremRequirements.Select(requirement => new { requirement.RequirementId, requirement.Present }),
}, options));

var checks = new[]
{
    new Check(
        "vo7-local-chain-materialized-but-incomplete",
        phase370Passed
            && phase370RecordsVo7
            && phase372Passed
            && phase372Vo7BuildingBlock
            && !phase372CompletesVo7
            && !phase372PhysicalMassPsiBranch
            && !phase370MixedBlocksPresent,
        $"phase370Passed={phase370Passed}; recordsVo7={phase370RecordsVo7}; phase372Passed={phase372Passed}; vo7BuildingBlock={phase372Vo7BuildingBlock}; completesVo7={phase372CompletesVo7}; physicalMassPsiBranch={phase372PhysicalMassPsiBranch}"),
    new Check(
        "response-image-diagnostic-materialized-not-observed-map",
        phase378Passed
            && phase378Rank == 3
            && phase379Passed
            && phase379StableTwoAxis
            && phase379SuppressedAxis == 1
            && !phase378PhysicalHessian
            && !phase379ObservedMap
            && !phase379DirectWzLaw,
        $"phase378Rank={phase378Rank}; phase379SuppressedAxis={phase379SuppressedAxis}; physicalHessian={phase378PhysicalHessian}; observedMap={phase379ObservedMap}; directWzLaw={phase379DirectWzLaw}"),
    new Check(
        "phase307-near-pass-sidecar-conflict-preserved",
        phase307Passed
            && phase307TargetBlind
            && phase307NearPassPresent
            && phase307P302ScaledStableCommonSelectionLawCount == 1
            && !phase307CanFillWz
            && phase381Passed
            && phase381SelectedWUsesSuppressed
            && phase381SidecarConflict
            && !phase381SidecarCompatible,
        $"nearPass={phase307NearPassPresent}; scaledStableCommonSelectionLawCount={phase307P302ScaledStableCommonSelectionLawCount}; canFillP307={phase307CanFillWz}; selectedWUsesSuppressed={phase381SelectedWUsesSuppressed}; sidecarCompatible={phase381SidecarCompatible}"),
    new Check(
        "no-current-selector-or-proxy-escapes-suppressed-axis",
        phase383Passed
            && phase383EveryWUsesSuppressed
            && phase383NoSelectorAvoidsSuppressed
            && phase383SidecarCompatibleCount == 0
            && phase384Passed
            && phase384NoLowSuppressedProxy
            && !phase384CanFillWz,
        $"everyWUsesSuppressed={phase383EveryWUsesSuppressed}; noSelectorAvoids={phase383NoSelectorAvoidsSuppressed}; sidecarCompatibleCount={phase383SidecarCompatibleCount}; minWSuppressedBasisEnergyFraction={phase384MinWSuppressedBasisEnergyFraction}; selectedSuppressedBasisEnergyFraction={phase384SelectedSuppressedBasisEnergyFraction}"),
    new Check(
        "observed-namespace-and-source-contracts-remain-unfilled",
        phase201ContractMaterialized
            && !phase201AllRequiredLineagesPromotable
            && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount
            && phase213HiggsMissingFieldCount == ExpectedHiggsMissingFieldCount
            && phase256ContractMaterialized
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ObservedFieldExtractionContractPromotable
            && phase385Passed
            && phase385CandidateCount == 9
            && phase385IntakeReadyCandidateCount == 0
            && phase385NoNamespaceCandidate
            && phase385NoPhase256Candidate
            && phase385NoWzCandidate
            && phase385NoHiggsCandidate,
        $"wzMissing={phase213WzMissingFieldCount}; higgsMissing={phase213HiggsMissingFieldCount}; phase256Filled={phase256FilledRequiredFieldCount}; phase385CandidateCount={phase385CandidateCount}; intakeReady={phase385IntakeReadyCandidateCount}"),
    new Check(
        "candidate-theorem-not-present-and-no-contract-promotion",
        !candidateTheoremPresent
            && missingTheoremRequirementCount > 0
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
            && phase201FieldsDefensiblyFilled.Length == 0,
        $"candidateTheoremPresent={candidateTheoremPresent}; missingTheoremRequirementCount={missingTheoremRequirementCount}; acceptedContractFieldCount={acceptedContractFieldCount}; phase201TemplateMutated={phase201TemplateMutated}"),
};

var terminalStatus = vo7ObservedNamespaceSourceTheoremProbePassed && checks.All(check => check.Passed)
    ? "vo7-observed-electroweak-namespace-source-theorem-probe-failed-closed-new-theorem-required"
    : "vo7-observed-electroweak-namespace-source-theorem-probe-review-required";

var result = new
{
    phaseId = "phase388-vo7-observed-electroweak-namespace-source-theorem-probe",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    vo7ObservedNamespaceSourceTheoremProbePassed = vo7ObservedNamespaceSourceTheoremProbePassed && checks.All(check => check.Passed),
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    applicationSubjectKind,
    candidateTheoremPresent,
    missingTheoremRequirementCount,
    theoremRequirements,
    localChainEvidence = new
    {
        phase370 = new
        {
            path = Phase370Path,
            phase370Passed,
            phase370RecordsVo7,
            phase370MixedBlocksPresent,
            phase370ObservedRowsPresent,
            phase370HiggsSourcePresent,
        },
        phase372 = new
        {
            path = Phase372Path,
            phase372Passed,
            phase372Vo7BuildingBlock,
            phase372CompletesVo7,
            phase372PhysicalMassPsiBranch,
            phase372MeshVolumeBranchCompatible,
        },
        phase378 = new
        {
            path = Phase378Path,
            phase378Passed,
            phase378Rank,
            phase378PhysicalHessian,
            phase378DirectWzLaw,
        },
        phase379 = new
        {
            path = Phase379Path,
            phase379Passed,
            phase379StableTwoAxis,
            phase379SuppressedAxis,
            phase379ObservedMap,
            phase379CanonicalSelector,
            phase379DirectWzLaw,
        },
        phase307 = new
        {
            path = Phase307Path,
            phase307Passed,
            phase307TargetBlind,
            phase307NearPassPresent,
            phase307SelectionLawCount,
            phase307P302ScaledStableCommonSelectionLawCount,
            phase307CanFillWz,
        },
        phase381 = new
        {
            path = Phase381Path,
            phase381Passed,
            phase381SelectedWUsesSuppressed,
            phase381SidecarConflict,
            phase381SidecarCompatible,
            phase381CanFillWz,
        },
        phase382 = new
        {
            path = Phase382Path,
            phase382Passed,
            phase382NamespaceMapPresent,
            phase382Rehabilitated,
            phase382CanFillWz,
        },
        phase383 = new
        {
            path = Phase383Path,
            phase383Passed,
            phase383EveryWUsesSuppressed,
            phase383NoSelectorAvoidsSuppressed,
            phase383SidecarCompatibleCount,
            phase383CanFillWz,
        },
        phase384 = new
        {
            path = Phase384Path,
            phase384Passed,
            phase384NoLowSuppressedProxy,
            phase384MinWSuppressedBasisEnergyFraction,
            phase384SelectedSuppressedBasisEnergyFraction,
            phase384CanFillWz,
        },
        phase387 = new
        {
            path = Phase387Path,
            phase387Passed,
            phase387FullTextBosonContractEvidenceFound,
        },
    },
    currentContractEvidence = new
    {
        phase201Path = Phase201Path,
        phase201ContractMaterialized,
        phase201AllRequiredLineagesPromotable,
        phase213Path = Phase213Path,
        phase213WzMissingFieldCount,
        phase213HiggsMissingFieldCount,
        phase256Path = Phase256Path,
        phase256ContractMaterialized,
        phase256RequiredFieldCount,
        phase256FilledRequiredFieldCount,
        phase256ObservedFieldExtractionContractPromotable,
        phase385Path = Phase385Path,
        phase385Passed,
        phase385CandidateCount,
        phase385IntakeReadyCandidateCount,
        phase385NoNamespaceCandidate,
        phase385NoPhase256Candidate,
        phase385NoWzCandidate,
        phase385NoHiggsCandidate,
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
    phase201FieldsDefensiblyFilled,
    checks,
    decision = "Do not promote the VO-7 shell-response branch. It remains the strongest repo-local diagnostic lead, but the current artifacts do not contain a theorem-level observed electroweak namespace map or source law. A future promotion attempt must either derive why the W row physically uses the Phase379-suppressed carrier axis or supply an observed photon/W/Z/H namespace map proving the carrier axes are not physical W/Z axes, plus the missing source, scale, pole, and unit lineages.",
    nextRequiredArtifact = new[]
    {
        "A physical M_psi-compatible fermionic branch with completed VO-7 mixed-linearization blocks and gauge identities.",
        "A physical effective-action Hessian or equivalent source operator, not only a study-defined shell-response Gram.",
        "A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem.",
        "A theorem explaining the W row's use of the Phase379-suppressed axis, or a namespace map proving carrier axes are not physical W/Z axes.",
        "Separate W/Z source rows, a Higgs scalar-source row, weak-angle/coupling lineage, VEV/source scale, pole extraction, and GeV normalization.",
    },
};

File.WriteAllText(Path.Combine(outputDir, "vo7_observed_electroweak_namespace_source_theorem_probe.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "vo7_observed_electroweak_namespace_source_theorem_probe_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.vo7ObservedNamespaceSourceTheoremProbePassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        result.candidateTheoremPresent,
        result.missingTheoremRequirementCount,
        result.theoremRequirements,
        result.localChainEvidence,
        result.currentContractEvidence,
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
        result.phase201FieldsDefensiblyFilled,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vo7ObservedNamespaceSourceTheoremProbePassed={result.vo7ObservedNamespaceSourceTheoremProbePassed}");
Console.WriteLine($"candidateTheoremPresent={candidateTheoremPresent}");
Console.WriteLine($"missingTheoremRequirementCount={missingTheoremRequirementCount}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string Sha256Hex(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record Requirement(string RequirementId, bool Present, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
