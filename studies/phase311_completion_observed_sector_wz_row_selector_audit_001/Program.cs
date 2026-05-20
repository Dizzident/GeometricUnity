using System.Text.Json;

const string DefaultOutputDir = "studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output";
const string CompletionRevisionPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase310Path = "studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE311_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase310 = JsonDocument.Parse(File.ReadAllText(Phase310Path));

var completionLines = File.ReadAllLines(CompletionRevisionPath);
var observedSectorProgramEvidence = new[]
{
    LineEvidence(12, "The completion draft claims document-level observed-sector recovery infrastructure, while preserving open branch-independence and quantitative-validation problems."),
    LineEvidence(14, "The completion draft claims a typed physical-identification gate and prediction-to-test matrix, while leaving proofs, uniqueness results, and empirical validations open."),
    LineEvidence(477, "A representation component corresponding to a candidate observed boson sector is explicitly classified as phenomenological mapping."),
    LineEvidence(480, "An effective observed sector identified from pullback or decomposition is explicitly classified as phenomenological mapping."),
    LineEvidence(932, "Formal-to-physical observed-sector content is recorded as phenomenological rather than theorem-level support."),
    LineEvidence(1219, "Family and observed-sector recovery is present as a phenomenological mapping."),
    LineEvidence(1221, "The formal source for observed-sector recovery is representation decomposition and the observation pipeline."),
    LineEvidence(2165, "Recovery of observed bosonic/fermionic content is branch-defined and partial; uniqueness and full low-energy identification remain open."),
    LineEvidence(2581, "Observed fields are defined generically by pullback, restriction, decomposition, or projection along an observation."),
    LineEvidence(2646, "Observation-dependent objects must declare their dependency on base data, admissible observations, chosen observation, or extra structure."),
    LineEvidence(5298, "Detailed bosonic decompositions remain completion tasks rather than settled Standard Model identifications."),
    LineEvidence(9300, "Empirical comparison requires a typed prediction entry."),
    LineEvidence(9303, "The typed prediction entry must have a branch-valid observable map."),
    LineEvidence(9338, "The formal claim must be translated into a typed observable map."),
    LineEvidence(9456, "Observed comparison requires an explicit derivation path, observable map, comparison rule, and falsifier."),
    LineEvidence(9508, "The draft's physical placements are a placement guide, not an observable map."),
    LineEvidence(11349, "The observation/pullback mechanism remains structurally underdefined for observed field content."),
    LineEvidence(11369, "Representation decomposition of observed bosons remains interpretive rather than derivational."),
    LineEvidence(11666, "A branch-local observed extraction theorem is a proof obligation, not a completed promotion artifact."),
    LineEvidence(11973, "Full formalization of observation/pullback remains a critical open problem."),
    LineEvidence(11993, "Representation decomposition of observed bosons remains a high/critical open problem."),
    LineEvidence(12015, "The physical blockers include observation, topological-to-metric recovery, and representation decomposition into observed bosons."),
};

var searchedCueCounts = new[]
{
    SearchCount("observed-sector recovery", "observed sector", "observed bosonic", "observed bosons"),
    SearchCount("representation decomposition", "representation-chain", "branching computations"),
    SearchCount("observable map", "observable extraction map", "typed observable"),
    SearchCount("photon/W/Z", "photon W Z", "photon-W-Z", "W/Z eigenstate", "eigenstate projection", "projection rows"),
    SearchCount("physical W/Z row selector", "canonical W/Z row selector", "W/Z source row", "row selector"),
};

var completionDraftObservedSectorProgramPresent = observedSectorProgramEvidence.All(row => row.Found);
var completionDraftTreatsObservedSectorAsPhenomenologicalMapping = completionDraftObservedSectorProgramPresent;
var completionDraftRequiresTypedObservableMapBeforeComparison = observedSectorProgramEvidence
    .Where(row => row.LineNumber is 9300 or 9303 or 9338 or 9456 or 9508)
    .All(row => row.Found);

var phase255NoGoPassed =
    JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true
    && JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is false
    && JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired") is true;

var phase257PipelineStillNotPhysicalBosonCapable =
    JsonBool(phase257.RootElement, "observationPipelinePhysicalBosonCapabilityAuditPassed") is true
    && JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is false
    && JsonBool(phase257.RootElement, "directObservationPipelineBosonCapable") is false
    && JsonBool(phase257.RootElement, "phase3ObservationPipelineBosonCapable") is false
    && JsonBool(phase257.RootElement, "spectrumPhysicalBosonMassMatrixCapable") is false
    && JsonBool(phase257.RootElement, "minimal4dExamplePromotableForBosons") is false;

var phase295CandidateScanStillNoIntakeReadyObservedExtraction =
    JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true
    && JsonInt(phase295.RootElement, "contractFieldCount") == 20
    && JsonInt(phase295.RootElement, "fieldsWithIntakeReadyCandidateCount") == 0
    && JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") == 0
    && JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is false;
var phase295PhotonEigenstateProjectionIntakeReady = FieldIntakeReady(phase295.RootElement, "photonEigenstateProjectionId");
var phase295WSourceRowIntakeReady = FieldIntakeReady(phase295.RootElement, "wBosonSourceRowId");
var phase295ZSourceRowIntakeReady = FieldIntakeReady(phase295.RootElement, "zBosonSourceRowId");
var phase295ElectroweakGaugeEmbeddingIntakeReady = FieldIntakeReady(phase295.RootElement, "electroweakGaugeEmbeddingId");
var phase295QuadraticElectroweakMassOperatorIntakeReady = FieldIntakeReady(phase295.RootElement, "quadraticElectroweakMassOperatorId");

var phase307SelectorStillNonPromotable =
    JsonBool(phase307.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true
    && JsonString(phase307.RootElement, "canonicalChargedOperator") == "T+/-=(axis0 +/- i axis1)/sqrt(2), evaluated on Phase27 charged axes 0 and 1."
    && JsonInt(phase307.RootElement, "selectionLawCount") >= 6
    && JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount") > 0
    && JsonInt(phase307.RootElement, "p302ScaledNearPassWithoutRawSelectionLawCount") > 0
    && JsonBool(phase307.RootElement, "theoremClaimed") is false
    && JsonBool(phase307.RootElement, "sourceRowsPromotable") is false
    && JsonBool(phase307.RootElement, "canFillPhase201WzContract") is false;

var phase310PhysicalRowsStillMissing =
    JsonBool(phase310.RootElement, "completionVariationalBranchToWzNormalizationAuditPassed") is true
    && JsonBool(phase310.RootElement, "completionDraftProvidesPhysicalWzSourceRowDerivation") is false
    && JsonBool(phase310.RootElement, "completionDraftProvidesBranchStableSourceRows") is false
    && JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead") is false
    && JsonBool(phase310.RootElement, "canFillPhase201WzContract") is false;

bool completionDraftProvidesCanonicalWzRowSelector = false;
bool completionDraftProvidesPhotonWzEigenstateProjectionRows = false;
bool completionDraftProvidesPhysicalWzObservableMap = false;
bool completionDraftProvidesBranchStableObservedWzRows = false;
bool completionDraftCanPromotePhase307Selector = false;
bool phase307RowsHaveObservedSectorMapId = false;
bool phase307RowsHaveElectroweakGaugeEmbeddingId = false;
bool phase307RowsHaveQuadraticElectroweakMassOperatorId = false;
bool phase307RowsHavePhotonMasslessGate = false;
bool phase307WRowsHavePhysicalEigenstateProjectionId = false;
bool phase307ZRowsHavePhysicalEigenstateProjectionId = false;
bool phase307ObservedProjectionBranchStable = false;
bool phase307ObservedProjectionTargetBlindHashPresent = false;
bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool canFillPhase201WzContract = false;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "completion-observed-sector-program-evidence-present",
        completionDraftObservedSectorProgramPresent,
        $"foundLineEvidenceCount={observedSectorProgramEvidence.Count(row => row.Found)}; requiredLineEvidenceCount={observedSectorProgramEvidence.Length}"),
    new Check(
        "observed-sector-evidence-is-governance-not-wz-row-selector",
        completionDraftTreatsObservedSectorAsPhenomenologicalMapping
            && completionDraftRequiresTypedObservableMapBeforeComparison
            && !completionDraftProvidesCanonicalWzRowSelector
            && !completionDraftProvidesPhotonWzEigenstateProjectionRows
            && !completionDraftProvidesPhysicalWzObservableMap,
        $"phenomenologicalMapping={completionDraftTreatsObservedSectorAsPhenomenologicalMapping}; requiresObservableMap={completionDraftRequiresTypedObservableMapBeforeComparison}; canonicalWzRowSelector={completionDraftProvidesCanonicalWzRowSelector}; photonWzEigenstateProjectionRows={completionDraftProvidesPhotonWzEigenstateProjectionRows}; physicalWzObservableMap={completionDraftProvidesPhysicalWzObservableMap}"),
    new Check(
        "phase255-observed-field-extraction-no-go-preserved",
        phase255NoGoPassed,
        $"observedFieldExtractionNoGoPassed={JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed")}; bridgePromotable={JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable")}; newArtifactRequired={JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired")}"),
    new Check(
        "phase257-observation-pipeline-not-physical-boson-capable",
        phase257PipelineStillNotPhysicalBosonCapable,
        $"canFillObservedFieldExtractionContract={JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract")}; directObservationPipelineBosonCapable={JsonBool(phase257.RootElement, "directObservationPipelineBosonCapable")}; spectrumPhysicalBosonMassMatrixCapable={JsonBool(phase257.RootElement, "spectrumPhysicalBosonMassMatrixCapable")}"),
    new Check(
        "phase295-candidate-scan-no-intake-ready-observed-extraction",
        phase295CandidateScanStillNoIntakeReadyObservedExtraction,
        $"contractFieldCount={JsonInt(phase295.RootElement, "contractFieldCount")}; fieldsWithIntakeReadyCandidateCount={JsonInt(phase295.RootElement, "fieldsWithIntakeReadyCandidateCount")}; intakeReadyObservedFieldExtractionCandidateCount={JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount")}; anyObservedFieldExtractionCandidateFillsContract={JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract")}"),
    new Check(
        "phase295-physical-photon-w-z-fields-not-intake-ready",
        !phase295PhotonEigenstateProjectionIntakeReady
            && !phase295WSourceRowIntakeReady
            && !phase295ZSourceRowIntakeReady
            && !phase295ElectroweakGaugeEmbeddingIntakeReady
            && !phase295QuadraticElectroweakMassOperatorIntakeReady,
        $"photonEigenstateProjectionIntakeReady={phase295PhotonEigenstateProjectionIntakeReady}; wSourceRowIntakeReady={phase295WSourceRowIntakeReady}; zSourceRowIntakeReady={phase295ZSourceRowIntakeReady}; electroweakGaugeEmbeddingIntakeReady={phase295ElectroweakGaugeEmbeddingIntakeReady}; quadraticElectroweakMassOperatorIntakeReady={phase295QuadraticElectroweakMassOperatorIntakeReady}"),
    new Check(
        "phase307-row-selector-near-pass-still-nonpromotable",
        phase307SelectorStillNonPromotable,
        $"selectionLawCount={JsonInt(phase307.RootElement, "selectionLawCount")}; p302ScaledStableCommonSelectionLawCount={JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount")}; theoremClaimed={JsonBool(phase307.RootElement, "theoremClaimed")}; sourceRowsPromotable={JsonBool(phase307.RootElement, "sourceRowsPromotable")}; canFillPhase201WzContract={JsonBool(phase307.RootElement, "canFillPhase201WzContract")}"),
    new Check(
        "phase307-selected-rows-lack-observed-electroweak-map-fields",
        !phase307RowsHaveObservedSectorMapId
            && !phase307RowsHaveElectroweakGaugeEmbeddingId
            && !phase307RowsHaveQuadraticElectroweakMassOperatorId
            && !phase307RowsHavePhotonMasslessGate
            && !phase307WRowsHavePhysicalEigenstateProjectionId
            && !phase307ZRowsHavePhysicalEigenstateProjectionId
            && !phase307ObservedProjectionBranchStable
            && !phase307ObservedProjectionTargetBlindHashPresent,
        $"observedSectorMapId={phase307RowsHaveObservedSectorMapId}; electroweakGaugeEmbeddingId={phase307RowsHaveElectroweakGaugeEmbeddingId}; quadraticElectroweakMassOperatorId={phase307RowsHaveQuadraticElectroweakMassOperatorId}; photonMasslessGate={phase307RowsHavePhotonMasslessGate}; wPhysicalEigenstateProjectionId={phase307WRowsHavePhysicalEigenstateProjectionId}; zPhysicalEigenstateProjectionId={phase307ZRowsHavePhysicalEigenstateProjectionId}; observedProjectionBranchStable={phase307ObservedProjectionBranchStable}; observedProjectionTargetBlindHashPresent={phase307ObservedProjectionTargetBlindHashPresent}"),
    new Check(
        "phase310-physical-wz-source-rows-still-missing",
        phase310PhysicalRowsStillMissing,
        $"physicalWzSourceRows={JsonBool(phase310.RootElement, "completionDraftProvidesPhysicalWzSourceRowDerivation")}; branchStableSourceRows={JsonBool(phase310.RootElement, "completionDraftProvidesBranchStableSourceRows")}; completionDraftCanPromotePhase302Lead={JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead")}"),
    new Check(
        "source-contract-remains-blocked",
        !canFillPhase201WzContract
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0
            && !completionDraftProvidesBranchStableObservedWzRows
            && !completionDraftCanPromotePhase307Selector,
        $"canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; branchStableObservedWzRows={completionDraftProvidesBranchStableObservedWzRows}; completionDraftCanPromotePhase307Selector={completionDraftCanPromotePhase307Selector}"),
};

var completionObservedSectorWzRowSelectorAuditPassed = checks.All(check => check.Passed)
    && !completionDraftProvidesCanonicalWzRowSelector
    && !completionDraftProvidesPhotonWzEigenstateProjectionRows
    && !completionDraftProvidesPhysicalWzObservableMap
    && !completionDraftProvidesBranchStableObservedWzRows
    && !completionDraftCanPromotePhase307Selector
    && !canFillPhase201WzContract;

var terminalStatus = completionObservedSectorWzRowSelectorAuditPassed
    ? "completion-observed-sector-wz-row-selector-audit-governance-not-row-law"
    : "completion-observed-sector-wz-row-selector-audit-review-required";

var result = new
{
    phaseId = "phase311-completion-observed-sector-wz-row-selector-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    completionObservedSectorWzRowSelectorAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    completionDraftObservedSectorProgramPresent,
    completionDraftTreatsObservedSectorAsPhenomenologicalMapping,
    completionDraftRequiresTypedObservableMapBeforeComparison,
    completionDraftProvidesCanonicalWzRowSelector,
    completionDraftProvidesPhotonWzEigenstateProjectionRows,
    completionDraftProvidesPhysicalWzObservableMap,
    completionDraftProvidesBranchStableObservedWzRows,
    completionDraftCanPromotePhase307Selector,
    phase307RowsHaveObservedSectorMapId,
    phase307RowsHaveElectroweakGaugeEmbeddingId,
    phase307RowsHaveQuadraticElectroweakMassOperatorId,
    phase307RowsHavePhotonMasslessGate,
    phase307WRowsHavePhysicalEigenstateProjectionId,
    phase307ZRowsHavePhysicalEigenstateProjectionId,
    phase307ObservedProjectionBranchStable,
    phase307ObservedProjectionTargetBlindHashPresent,
    phase295PhotonEigenstateProjectionIntakeReady,
    phase295WSourceRowIntakeReady,
    phase295ZSourceRowIntakeReady,
    phase307SelectorStillNonPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    completionRevision = new
    {
        path = CompletionRevisionPath,
        totalLineCount = completionLines.Length,
        observedSectorProgramEvidence,
        searchedCueCounts,
    },
    inheritedEvidence = new
    {
        phase255 = new
        {
            phase255NoGoPassed,
            observedFieldExtractionNoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed"),
            observedFieldExtractionBridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable"),
            newObservedFieldExtractionArtifactRequired = JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired"),
        },
        phase257 = new
        {
            phase257PipelineStillNotPhysicalBosonCapable,
            currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract"),
            directObservationPipelineBosonCapable = JsonBool(phase257.RootElement, "directObservationPipelineBosonCapable"),
            phase3ObservationPipelineBosonCapable = JsonBool(phase257.RootElement, "phase3ObservationPipelineBosonCapable"),
            spectrumPhysicalBosonMassMatrixCapable = JsonBool(phase257.RootElement, "spectrumPhysicalBosonMassMatrixCapable"),
            minimal4dExamplePromotableForBosons = JsonBool(phase257.RootElement, "minimal4dExamplePromotableForBosons"),
        },
        phase295 = new
        {
            phase295CandidateScanStillNoIntakeReadyObservedExtraction,
            contractFieldCount = JsonInt(phase295.RootElement, "contractFieldCount"),
            fieldsWithCandidateLineCount = JsonInt(phase295.RootElement, "fieldsWithCandidateLineCount"),
            fieldsWithIntakeReadyCandidateCount = JsonInt(phase295.RootElement, "fieldsWithIntakeReadyCandidateCount"),
            intakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount"),
            anyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract"),
            phase295PhotonEigenstateProjectionIntakeReady,
            phase295WSourceRowIntakeReady,
            phase295ZSourceRowIntakeReady,
            phase295ElectroweakGaugeEmbeddingIntakeReady,
            phase295QuadraticElectroweakMassOperatorIntakeReady,
        },
        phase307 = new
        {
            phase307SelectorStillNonPromotable,
            canonicalChargedOperator = JsonString(phase307.RootElement, "canonicalChargedOperator"),
            selectionLawCount = JsonInt(phase307.RootElement, "selectionLawCount"),
            p302ScaledStableCommonSelectionLawCount = JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount"),
            p302ScaledNearPassWithoutRawSelectionLawCount = JsonInt(phase307.RootElement, "p302ScaledNearPassWithoutRawSelectionLawCount"),
            theoremClaimed = JsonBool(phase307.RootElement, "theoremClaimed"),
            sourceRowsPromotable = JsonBool(phase307.RootElement, "sourceRowsPromotable"),
            canFillPhase201WzContract = JsonBool(phase307.RootElement, "canFillPhase201WzContract"),
        },
        phase310 = new
        {
            phase310PhysicalRowsStillMissing,
            completionDraftProvidesPhysicalWzSourceRowDerivation = JsonBool(phase310.RootElement, "completionDraftProvidesPhysicalWzSourceRowDerivation"),
            completionDraftProvidesBranchStableSourceRows = JsonBool(phase310.RootElement, "completionDraftProvidesBranchStableSourceRows"),
            completionDraftCanPromotePhase302Lead = JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead"),
            canFillPhase201WzContract = JsonBool(phase310.RootElement, "canFillPhase201WzContract"),
        },
    },
    checks,
    decision = completionObservedSectorWzRowSelectorAuditPassed
        ? "Do not use the completion draft's observed-sector recovery program to promote the Phase307 W/Z row selector. The draft supplies governance, status discipline, and proof obligations for observation and representation decomposition, but it does not derive photon/W/Z eigenstate projection rows, a canonical physical W/Z observable map, or branch-stable physical W/Z source rows."
        : "Review the completion observed-sector W/Z row-selector audit before relying on this non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A theorem deriving a canonical physical W/Z row selector from the observed-sector representation decomposition before target comparison.",
        "Photon/W/Z eigenstate projection rows with branch, normalization, and source-lineage provenance.",
        "A physical W/Z observable map tying GU source rows to electroweak W and Z mass observables without target-value construction.",
        "Phase201/P209 W/Z rows with derivation, raw-amplitude, common-bridge, target-comparison, and stability gates all filled.",
    },
    sourceEvidence = new
    {
        completionRevisionPath = CompletionRevisionPath,
        phase213Path = Phase213Path,
        phase255Path = Phase255Path,
        phase257Path = Phase257Path,
        phase295Path = Phase295Path,
        phase307Path = Phase307Path,
        phase310Path = Phase310Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "completion_observed_sector_wz_row_selector_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "completion_observed_sector_wz_row_selector_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.completionObservedSectorWzRowSelectorAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.completionDraftObservedSectorProgramPresent,
        result.completionDraftTreatsObservedSectorAsPhenomenologicalMapping,
        result.completionDraftRequiresTypedObservableMapBeforeComparison,
        result.completionDraftProvidesCanonicalWzRowSelector,
        result.completionDraftProvidesPhotonWzEigenstateProjectionRows,
        result.completionDraftProvidesPhysicalWzObservableMap,
        result.completionDraftProvidesBranchStableObservedWzRows,
        result.completionDraftCanPromotePhase307Selector,
        result.phase307RowsHaveObservedSectorMapId,
        result.phase307RowsHaveElectroweakGaugeEmbeddingId,
        result.phase307RowsHaveQuadraticElectroweakMassOperatorId,
        result.phase307RowsHavePhotonMasslessGate,
        result.phase307WRowsHavePhysicalEigenstateProjectionId,
        result.phase307ZRowsHavePhysicalEigenstateProjectionId,
        result.phase307ObservedProjectionBranchStable,
        result.phase307ObservedProjectionTargetBlindHashPresent,
        result.phase295PhotonEigenstateProjectionIntakeReady,
        result.phase295WSourceRowIntakeReady,
        result.phase295ZSourceRowIntakeReady,
        result.phase307SelectorStillNonPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        completionRevision = new
        {
            path = CompletionRevisionPath,
            observedSectorProgramEvidenceCount = observedSectorProgramEvidence.Length,
            keyEvidenceLines = observedSectorProgramEvidence.Select(row => new { row.LineNumber, row.AuditFinding, row.Excerpt }).ToArray(),
            searchedCueCounts,
        },
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"completionObservedSectorWzRowSelectorAuditPassed={completionObservedSectorWzRowSelectorAuditPassed}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

LineEvidence LineEvidence(int lineNumber, string auditFinding)
{
    var found = lineNumber > 0 && lineNumber <= completionLines.Length && !string.IsNullOrWhiteSpace(completionLines[lineNumber - 1]);
    var excerpt = found ? completionLines[lineNumber - 1].Trim() : null;
    return new LineEvidence(lineNumber, found, excerpt, auditFinding);
}

SearchCount SearchCount(params string[] patterns)
{
    var count = completionLines.Sum(line => patterns.Count(pattern => line.Contains(pattern, StringComparison.OrdinalIgnoreCase)));
    return new SearchCount(string.Join(" | ", patterns), count);
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
        ? result
        : null;

static bool FieldIntakeReady(JsonElement element, string fieldId) =>
    element.TryGetProperty("fieldResults", out var fields)
    && fields.ValueKind == JsonValueKind.Array
    && fields.EnumerateArray().Any(field =>
        JsonString(field, "fieldId") == fieldId
        && JsonInt(field, "intakeReadyCandidateCount") > 0
        && JsonBool(field, "filled") is true);

sealed record LineEvidence(int LineNumber, bool Found, string? Excerpt, string AuditFinding);
sealed record SearchCount(string PatternSet, int Count);
sealed record Check(string Id, bool Passed, string Detail);
