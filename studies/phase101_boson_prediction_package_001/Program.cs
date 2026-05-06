using System.Text.Json;

const string DefaultOutputDir = "studies/phase101_boson_prediction_package_001/output";
const string Phase100ReadinessPath = "studies/phase100_boson_prediction_readiness_001/output/boson_prediction_readiness.json";
const string Phase100InputPath = "studies/phase100_boson_prediction_readiness_001/output/boson_prediction_readiness_input.json";
const string Phase99EvidencePath = "studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_evidence.json";
const string Phase91BranchEvidencePath = "studies/phase91_branch_stability_evidence_promotion_001/output/projected_branch_stability_evidence.json";
const string Phase95SummaryPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_summary.json";
const string Phase102ClaimPromotionPath = "studies/phase102_internal_boson_claim_promotion_001/output/internal_boson_claim_promotion.json";
const string Phase103FalsifierPolicyPath = "studies/phase103_target_scoped_falsifier_policy_001/output/target_scoped_falsifier_policy.json";
const string Phase104MappingAttemptPath = "studies/phase104_candidate3_physical_mapping_attempt_001/output/candidate3_physical_mapping_attempt.json";
const string Phase105DerivationPrerequisitesPath = "studies/phase105_candidate3_physical_derivation_prerequisites_001/output/candidate3_physical_derivation_prerequisites.json";
const string Phase106IdentityDerivationPath = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase107NormalizationPath = "studies/phase107_candidate3_target_independent_normalization_001/output/candidate3_target_independent_normalization.json";
const string Phase108ComparisonClosurePath = "studies/phase108_candidate3_physical_comparison_closure_001/output/candidate3_physical_comparison_closure.json";
const string Phase109RouteSelectionPath = "studies/phase109_post_candidate3_route_selection_001/output/post_candidate3_route_selection.json";
const string Phase110WzAbsoluteRepairContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase111ReplayedMatrixElementAttemptPath = "studies/phase111_replayed_matrix_element_repair_attempt_001/output/replayed_matrix_element_repair_attempt.json";
const string Phase112ScalarRelationAttemptPath = "studies/phase112_scalar_sector_relation_revision_attempt_001/output/scalar_sector_relation_revision_attempt.json";
const string Phase113RepairAttemptGatePath = "studies/phase113_wz_absolute_repair_attempt_gate_001/output/wz_absolute_repair_attempt_gate.json";
const string Phase114WzRouteMatrixElementEvidencePath = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/wz_route_replayed_matrix_element_evidence.json";
const string Phase115WzRouteFermionQualityReplayPath = "studies/phase115_wz_route_fermion_quality_replay_001/output/wz_route_fermion_quality_replay.json";
const string Phase116WzAbsoluteProjectionRerunPath = "studies/phase116_wz_absolute_projection_rerun_001/output/wz_absolute_projection_rerun.json";
const string Phase117WzRepairedPairSweepPath = "studies/phase117_wz_repaired_pair_sweep_001/output/wz_repaired_pair_sweep.json";
const string Phase118WzMatrixElementNormalizationDiagnosticPath = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase119OperatorSourceScaleDerivationAuditPath = "studies/phase119_operator_source_scale_derivation_audit_001/output/operator_source_scale_derivation_audit.json";
const string Phase120AnalyticVariationMeasureConsistencyPath = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string Phase122CorrectedOperatorSelectionRuleSweepPath = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase123WzFermionSectorMetadataAuditPath = "studies/phase123_wz_fermion_sector_metadata_audit_001/output/wz_fermion_sector_metadata_audit.json";
const string Phase124Phase95SourceJoinMetadataMaterializationPath = "studies/phase124_phase95_source_join_metadata_materialization_001/output/phase95_source_join_metadata_materialization.json";
const string Phase125SourceJoinFamilyMetadataMaterializationPath = "studies/phase125_source_join_family_metadata_materialization_001/output/source_join_family_metadata_materialization.json";
const string Phase126FermionSectorIdentitySourceAuditPath = "studies/phase126_fermion_sector_identity_source_audit_001/output/fermion_sector_identity_source_audit.json";
const string Phase127FermionGaugeBasisContentObservablePath = "studies/phase127_fermion_gauge_basis_content_observable_001/output/fermion_gauge_basis_content_observable.json";
const string Phase128FermionSu2GeneratorSectorObservablePath = "studies/phase128_fermion_su2_generator_sector_observable_001/output/fermion_su2_generator_sector_observable.json";
const string Phase129CandidateClusterSectorIdentityAuditPath = "studies/phase129_candidate_cluster_sector_identity_audit_001/output/candidate_cluster_sector_identity_audit.json";
const string Phase130FermionSectorLabelTableGatePath = "studies/phase130_fermion_sector_label_table_gate_001/output/fermion_sector_label_table_gate.json";
const string Phase131SectorLabelCandidateCoverageRepairPath = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase132FermionSectorLabelDerivationSourceGatePath = "studies/phase132_fermion_sector_label_derivation_source_gate_001/output/fermion_sector_label_derivation_source_gate.json";
const string Phase133FermionIdentityFeatureExtractorPath = "studies/phase133_fermion_identity_feature_extractor_001/output/fermion_identity_feature_extractor.json";
const string Phase134FermionChiralityConjugationTransitionTablePath = "studies/phase134_fermion_chirality_conjugation_transition_table_001/output/fermion_chirality_conjugation_transition_table.json";
const string Phase135CorrectedWzSweepReadinessGatePath = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output/corrected_wz_sweep_readiness_gate.json";
const string Phase136NumericAliasSectorLabelTransferAuditPath = "studies/phase136_numeric_alias_sector_label_transfer_audit_001/output/numeric_alias_sector_label_transfer_audit.json";
const string Phase137BaseChiralityRouteAuditPath = "studies/phase137_base_chirality_route_audit_001/output/base_chirality_route_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE101_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var readiness = JsonDocument.Parse(File.ReadAllText(Phase100ReadinessPath));
using var readinessInput = JsonDocument.Parse(File.ReadAllText(Phase100InputPath));
using var phase99 = JsonDocument.Parse(File.ReadAllText(Phase99EvidencePath));
using var phase91 = JsonDocument.Parse(File.ReadAllText(Phase91BranchEvidencePath));
using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95SummaryPath));
using var phase115 = TryParseJson(Phase115WzRouteFermionQualityReplayPath);
using var phase116 = TryParseJson(Phase116WzAbsoluteProjectionRerunPath);
using var phase117 = TryParseJson(Phase117WzRepairedPairSweepPath);
using var phase118 = TryParseJson(Phase118WzMatrixElementNormalizationDiagnosticPath);
using var phase119 = TryParseJson(Phase119OperatorSourceScaleDerivationAuditPath);
using var phase120 = TryParseJson(Phase120AnalyticVariationMeasureConsistencyPath);
using var phase122 = TryParseJson(Phase122CorrectedOperatorSelectionRuleSweepPath);
using var phase123 = TryParseJson(Phase123WzFermionSectorMetadataAuditPath);
using var phase124 = TryParseJson(Phase124Phase95SourceJoinMetadataMaterializationPath);
using var phase125 = TryParseJson(Phase125SourceJoinFamilyMetadataMaterializationPath);
using var phase126 = TryParseJson(Phase126FermionSectorIdentitySourceAuditPath);
using var phase127 = TryParseJson(Phase127FermionGaugeBasisContentObservablePath);
using var phase128 = TryParseJson(Phase128FermionSu2GeneratorSectorObservablePath);
using var phase129 = TryParseJson(Phase129CandidateClusterSectorIdentityAuditPath);
using var phase130 = TryParseJson(Phase130FermionSectorLabelTableGatePath);
using var phase131 = TryParseJson(Phase131SectorLabelCandidateCoverageRepairPath);
using var phase132 = TryParseJson(Phase132FermionSectorLabelDerivationSourceGatePath);
using var phase133 = TryParseJson(Phase133FermionIdentityFeatureExtractorPath);
using var phase134 = TryParseJson(Phase134FermionChiralityConjugationTransitionTablePath);
using var phase135 = TryParseJson(Phase135CorrectedWzSweepReadinessGatePath);
using var phase136 = TryParseJson(Phase136NumericAliasSectorLabelTransferAuditPath);
using var phase137 = TryParseJson(Phase137BaseChiralityRouteAuditPath);

bool internalReady = JsonBool(readiness.RootElement, "internalBosonReplayPredictionReady") ?? false;
bool externalReady = JsonBool(readiness.RootElement, "externalPhysicalComparisonReady") ?? false;
string terminalStatus = externalReady
    ? "external-physical-boson-prediction-package-built"
    : internalReady
        ? "internal-boson-prediction-package-built-physical-comparison-blocked"
        : "boson-prediction-package-blocked";

var package = new
{
    phaseId = "phase101-boson-prediction-package",
    terminalStatus,
    createdAt = DateTimeOffset.UtcNow,
    predictionLevel = JsonString(readiness.RootElement, "predictionLevel"),
    candidateId = JsonString(readiness.RootElement, "candidateId"),
    selectedBosonModeId = JsonString(readiness.RootElement, "selectedBosonModeId"),
    sourceBackedReplayReady = JsonBool(readiness.RootElement, "sourceBackedReplayReady"),
    internalBosonReplayPredictionReady = internalReady,
    externalPhysicalComparisonReady = externalReady,
    canCompareToExternalBosonValues = externalReady,
    internalPrediction = new
    {
        observableId = "phase99-candidate-3-replayed-coupling-proxy-magnitude",
        observableType = "internal-coupling-proxy",
        value = JsonDouble(readiness.RootElement, "couplingMagnitude"),
        unitFamily = "internal-native",
        externalTargetsUsed = false,
        selectedBosonModeVectorLength = JsonInt(
            phase99.RootElement.GetProperty("fullConnectionLift"),
            "modeVectorLength"),
        branchStabilityScore = JsonDouble(phase91.RootElement, "branchStabilityScore"),
        branchCouplingMean = JsonDouble(phase91.RootElement, "branchCouplingMean"),
        branchCouplingAbsoluteSpread = JsonDouble(phase91.RootElement, "branchCouplingAbsoluteSpread"),
        branchCouplingRelativeSpread = JsonDouble(phase91.RootElement, "branchCouplingRelativeSpread"),
        refinementStabilityScore = JsonDouble(phase95.RootElement, "refinementStabilityScore"),
    },
    physicalComparison = externalReady
        ? new
        {
            status = "ready",
            note = "external physical comparison gates passed",
        }
        : new
        {
            status = "blocked",
            note = "candidate remains an internal replay prediction until Phase100 physical gates pass",
        },
    blockers = JsonStringArray(readiness.RootElement, "blockers"),
    requiredFixes = JsonStringArray(readiness.RootElement, "requiredFixes"),
    validationGates = readiness.RootElement.GetProperty("validationGates").Clone(),
    residualLimitations = JsonStringArray(readiness.RootElement, "residualLimitations"),
    sourceEvidence = new
    {
        readinessPath = Phase100ReadinessPath,
        readinessInputPath = Phase100InputPath,
        selectorFullLiftEvidencePath = Phase99EvidencePath,
        branchStabilityEvidencePath = Phase91BranchEvidencePath,
        refinementEvidencePath = Phase95SummaryPath,
        internalClaimPromotionPath = File.Exists(Phase102ClaimPromotionPath) ? Phase102ClaimPromotionPath : null,
        targetScopedFalsifierPolicyPath = File.Exists(Phase103FalsifierPolicyPath) ? Phase103FalsifierPolicyPath : null,
        candidate3PhysicalMappingAttemptPath = File.Exists(Phase104MappingAttemptPath) ? Phase104MappingAttemptPath : null,
        candidate3PhysicalDerivationPrerequisitesPath = File.Exists(Phase105DerivationPrerequisitesPath) ? Phase105DerivationPrerequisitesPath : null,
        candidate3ObservableIdentityDerivationPath = File.Exists(Phase106IdentityDerivationPath) ? Phase106IdentityDerivationPath : null,
        candidate3TargetIndependentNormalizationPath = File.Exists(Phase107NormalizationPath) ? Phase107NormalizationPath : null,
        candidate3PhysicalComparisonClosurePath = File.Exists(Phase108ComparisonClosurePath) ? Phase108ComparisonClosurePath : null,
        postCandidate3RouteSelectionPath = File.Exists(Phase109RouteSelectionPath) ? Phase109RouteSelectionPath : null,
        wzAbsoluteRepairExecutionContractPath = File.Exists(Phase110WzAbsoluteRepairContractPath) ? Phase110WzAbsoluteRepairContractPath : null,
        replayedMatrixElementRepairAttemptPath = File.Exists(Phase111ReplayedMatrixElementAttemptPath) ? Phase111ReplayedMatrixElementAttemptPath : null,
        scalarSectorRelationRevisionAttemptPath = File.Exists(Phase112ScalarRelationAttemptPath) ? Phase112ScalarRelationAttemptPath : null,
        wzAbsoluteRepairAttemptGatePath = File.Exists(Phase113RepairAttemptGatePath) ? Phase113RepairAttemptGatePath : null,
        wzRouteReplayedMatrixElementEvidencePath = File.Exists(Phase114WzRouteMatrixElementEvidencePath) ? Phase114WzRouteMatrixElementEvidencePath : null,
        wzRouteFermionQualityReplayPath = File.Exists(Phase115WzRouteFermionQualityReplayPath) ? Phase115WzRouteFermionQualityReplayPath : null,
        wzAbsoluteProjectionRerunPath = File.Exists(Phase116WzAbsoluteProjectionRerunPath) ? Phase116WzAbsoluteProjectionRerunPath : null,
        wzRepairedPairSweepPath = File.Exists(Phase117WzRepairedPairSweepPath) ? Phase117WzRepairedPairSweepPath : null,
        wzMatrixElementNormalizationDiagnosticPath = File.Exists(Phase118WzMatrixElementNormalizationDiagnosticPath) ? Phase118WzMatrixElementNormalizationDiagnosticPath : null,
        operatorSourceScaleDerivationAuditPath = File.Exists(Phase119OperatorSourceScaleDerivationAuditPath) ? Phase119OperatorSourceScaleDerivationAuditPath : null,
        analyticVariationMeasureConsistencyPath = File.Exists(Phase120AnalyticVariationMeasureConsistencyPath) ? Phase120AnalyticVariationMeasureConsistencyPath : null,
        correctedOperatorSelectionRuleSweepPath = File.Exists(Phase122CorrectedOperatorSelectionRuleSweepPath) ? Phase122CorrectedOperatorSelectionRuleSweepPath : null,
        wzFermionSectorMetadataAuditPath = File.Exists(Phase123WzFermionSectorMetadataAuditPath) ? Phase123WzFermionSectorMetadataAuditPath : null,
        phase95SourceJoinMetadataMaterializationPath = File.Exists(Phase124Phase95SourceJoinMetadataMaterializationPath) ? Phase124Phase95SourceJoinMetadataMaterializationPath : null,
        sourceJoinFamilyMetadataMaterializationPath = File.Exists(Phase125SourceJoinFamilyMetadataMaterializationPath) ? Phase125SourceJoinFamilyMetadataMaterializationPath : null,
        fermionSectorIdentitySourceAuditPath = File.Exists(Phase126FermionSectorIdentitySourceAuditPath) ? Phase126FermionSectorIdentitySourceAuditPath : null,
        fermionGaugeBasisContentObservablePath = File.Exists(Phase127FermionGaugeBasisContentObservablePath) ? Phase127FermionGaugeBasisContentObservablePath : null,
        fermionSu2GeneratorSectorObservablePath = File.Exists(Phase128FermionSu2GeneratorSectorObservablePath) ? Phase128FermionSu2GeneratorSectorObservablePath : null,
        candidateClusterSectorIdentityAuditPath = File.Exists(Phase129CandidateClusterSectorIdentityAuditPath) ? Phase129CandidateClusterSectorIdentityAuditPath : null,
        fermionSectorLabelTableGatePath = File.Exists(Phase130FermionSectorLabelTableGatePath) ? Phase130FermionSectorLabelTableGatePath : null,
        sectorLabelCandidateCoverageRepairPath = File.Exists(Phase131SectorLabelCandidateCoverageRepairPath) ? Phase131SectorLabelCandidateCoverageRepairPath : null,
        fermionSectorLabelDerivationSourceGatePath = File.Exists(Phase132FermionSectorLabelDerivationSourceGatePath) ? Phase132FermionSectorLabelDerivationSourceGatePath : null,
        fermionIdentityFeatureExtractorPath = File.Exists(Phase133FermionIdentityFeatureExtractorPath) ? Phase133FermionIdentityFeatureExtractorPath : null,
        fermionChiralityConjugationTransitionTablePath = File.Exists(Phase134FermionChiralityConjugationTransitionTablePath) ? Phase134FermionChiralityConjugationTransitionTablePath : null,
        correctedWzSweepReadinessGatePath = File.Exists(Phase135CorrectedWzSweepReadinessGatePath) ? Phase135CorrectedWzSweepReadinessGatePath : null,
        numericAliasSectorLabelTransferAuditPath = File.Exists(Phase136NumericAliasSectorLabelTransferAuditPath) ? Phase136NumericAliasSectorLabelTransferAuditPath : null,
        baseChiralityRouteAuditPath = File.Exists(Phase137BaseChiralityRouteAuditPath) ? Phase137BaseChiralityRouteAuditPath : null,
        materializedModePath = JsonString(phase99.RootElement, "materializedModePath"),
        replayProbePath = JsonString(phase99.RootElement, "replayProbePath"),
    },
    nextPhasePrerequisites = phase137 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase137.RootElement, "terminalStatus"),
                "fermion-base-chirality-route-sector-labels-ready",
                StringComparison.Ordinal)
                ? "fermion-base-chirality-route-sector-labels-ready"
                : "fermion-base-chirality-route-diagnostic-only",
            prerequisitesPath = Phase137BaseChiralityRouteAuditPath,
            nextWork = JsonBool(phase137.RootElement, "baseChiralityRoutePromotable") is true
                ? "apply base-chirality-derived labels and rerun sector-label gates"
                : "combine base-chirality diagnostics with an independent fermion charge/weak-sector derivation or conjugation transition rule before assigning labels",
        }
        : phase136 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase136.RootElement, "terminalStatus"),
                "fermion-sector-numeric-alias-transfer-ready",
                StringComparison.Ordinal)
                ? "fermion-sector-numeric-alias-transfer-ready"
                : "fermion-sector-numeric-alias-transfer-rejected",
            prerequisitesPath = Phase136NumericAliasSectorLabelTransferAuditPath,
            nextWork = JsonBool(phase136.RootElement, "aliasTransferPromotable") is true
                ? "apply the audited sector-label transfer and rerun sector-label gates"
                : "derive fermion-specific target-blind sector labels or a nontrivial transition observable; do not transfer Phase46 vector-boson charge sectors by numeric suffix alias alone",
        }
        : phase135 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase135.RootElement, "terminalStatus"),
                "corrected-wz-sweep-rerun-ready",
                StringComparison.Ordinal)
                ? "corrected-wz-sweep-rerun-ready"
                : "corrected-wz-sweep-rerun-sector-labels-blocked",
            prerequisitesPath = Phase135CorrectedWzSweepReadinessGatePath,
            nextWork = JsonBool(phase135.RootElement, "rerunReady") is true
                ? "rerun the corrected W/Z transition sweep under the promoted fermion sector-label and transition prerequisites"
                : "complete fermion sector-label or nontrivial transition-table derivation before rerunning the corrected W/Z transition sweep",
        }
        : phase134 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase134.RootElement, "terminalStatus"),
                "fermion-chirality-conjugation-transition-table-ready",
                StringComparison.Ordinal)
                ? "fermion-chirality-conjugation-transition-table-ready"
                : "fermion-chirality-conjugation-transition-table-blocked",
            prerequisitesPath = Phase134FermionChiralityConjugationTransitionTablePath,
            nextWork = JsonBool(phase134.RootElement, "transitionTablePromotable") is true
                ? "use the transition table to assign fermion weak-sector labels and rerun sector-label gates"
                : "derive a nontrivial chirality/conjugation transition observable or prove another promotable fermion sector observable before rerunning the corrected W/Z transition sweep",
        }
        : phase133 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase133.RootElement, "terminalStatus"),
                "fermion-identity-feature-extractor-labels-ready",
                StringComparison.Ordinal)
                ? "fermion-identity-feature-extractor-labels-ready"
                : "fermion-identity-feature-extractor-labels-blocked",
            prerequisitesPath = Phase133FermionIdentityFeatureExtractorPath,
            nextWork = JsonBool(phase133.RootElement, "featureExtractorPromotable") is true
                ? "apply extracted fermion identity labels to the sector-label table and rerun the corrected W/Z transition sweep"
                : "derive a promotable fermion sector observable or nontrivial chirality/conjugation transition table for the P131 rows before assigning charge/weak-sector labels",
        }
        : phase132 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase132.RootElement, "terminalStatus"),
                "fermion-sector-label-derivation-source-ready",
                StringComparison.Ordinal)
                ? "fermion-sector-label-derivation-source-ready"
                : "fermion-sector-label-derivation-source-blocked",
            prerequisitesPath = Phase132FermionSectorLabelDerivationSourceGatePath,
            nextWork = JsonBool(phase132.RootElement, "derivationSourcePromotable") is true
                ? "apply the matching fermion sector-label derivation records to the coverage-repaired label table and rerun the corrected W/Z transition sweep"
                : "implement a target-blind fermion identity feature extractor keyed to the P131 coverage-repaired rows before applying charge/weak-sector labels",
        }
        : phase131 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase131.RootElement, "terminalStatus"),
                "sector-label-candidate-coverage-repaired-label-table-ready",
                StringComparison.Ordinal)
                ? "sector-label-candidate-coverage-repaired-label-table-ready"
                : "sector-label-candidate-coverage-repaired-labels-blocked",
            prerequisitesPath = Phase131SectorLabelCandidateCoverageRepairPath,
            nextWork = JsonBool(phase131.RootElement, "sectorLabelTablePromotable") is true
                ? "rerun the corrected-operator W/Z transition sweep under the coverage-repaired fermion sector-label table"
                : "derive explicit target-blind chargeSector and weak-sector/quantum-number labels for the coverage-repaired sector-label table before rerunning the corrected W/Z transition sweep",
        }
        : phase130 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase130.RootElement, "terminalStatus"),
                "fermion-sector-label-table-ready",
                StringComparison.Ordinal)
                ? "fermion-sector-label-table-ready"
                : "fermion-sector-label-table-incomplete-blocked",
            prerequisitesPath = Phase130FermionSectorLabelTableGatePath,
            nextWork = JsonBool(phase130.RootElement, "sectorLabelTablePromotable") is true
                ? "rerun the corrected-operator W/Z transition sweep under the materialized fermion sector-label table"
                : "repair candidate coverage and populate explicit target-blind charge/weak-sector labels for every sector-label table row before rerunning the corrected W/Z transition sweep",
        }
        : phase129 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase129.RootElement, "terminalStatus"),
                "candidate-cluster-sector-identity-source-ready",
                StringComparison.Ordinal)
                ? "candidate-cluster-sector-identity-source-ready"
                : "candidate-cluster-sector-identity-blocked",
            prerequisitesPath = Phase129CandidateClusterSectorIdentityAuditPath,
            nextWork = JsonBool(phase129.RootElement, "sectorIdentityPromotable") is true
                ? "materialize candidate/family sector labels and rerun the corrected-operator W/Z transition sweep"
                : "derive explicit target-blind fermion charge/weak-sector labels for the matched candidate or its member families before rerunning the corrected W/Z transition sweep",
        }
        : phase128 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase128.RootElement, "terminalStatus"),
                "fermion-su2-generator-sector-observable-wz-rule-ready",
                StringComparison.Ordinal)
                ? "fermion-su2-generator-sector-wz-rule-ready"
                : "fermion-su2-generator-sector-mixed-sector-blocked",
            prerequisitesPath = Phase128FermionSu2GeneratorSectorObservablePath,
            nextWork = JsonBool(phase128.RootElement, "wzTransitionRulePromotable") is true
                ? "rerun the corrected-operator W/Z transition sweep under the promoted SU(2) generator-sector transition rule"
                : "derive a stable target-blind fermion sector identity source beyond SU(2) basis and generator moments before rerunning the corrected W/Z transition sweep",
        }
        : phase127 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase127.RootElement, "terminalStatus"),
                "fermion-gauge-basis-content-observable-sector-ready",
                StringComparison.Ordinal)
                ? "fermion-gauge-basis-sector-ready"
                : "fermion-gauge-basis-content-mixed-sector-blocked",
            prerequisitesPath = Phase127FermionGaugeBasisContentObservablePath,
            nextWork = JsonBool(phase127.RootElement, "sectorRulePromotable") is true
                ? "rerun the corrected-operator W/Z transition sweep under the promoted fermion sector rule"
                : "implement a nontrivial fermion sector identity observable beyond gauge-basis energy fractions before rerunning the corrected W/Z transition sweep",
        }
        : phase126 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase126.RootElement, "terminalStatus"),
                "fermion-sector-identity-source-ready",
                StringComparison.Ordinal)
                ? "fermion-sector-identity-source-ready"
                : "fermion-sector-identity-source-blocked",
            prerequisitesPath = Phase126FermionSectorIdentitySourceAuditPath,
            nextWork = JsonBool(phase126.RootElement, "fermionSectorIdentityPromotable") is true
                ? "materialize the fermion sector table and rerun the corrected-operator W/Z transition sweep under the derived physical transition rule"
                : "implement a target-blind fermion sector identity observable or table that emits chargeSector and weak-sector/quantum-number labels for repaired families",
        }
        : phase125 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase125.RootElement, "terminalStatus"),
                "source-family-metadata-materialized-sector-labels-blocked",
                StringComparison.Ordinal)
                ? "source-family-metadata-ready-sector-labels-blocked"
                : "source-family-metadata-materialization-blocked",
            prerequisitesPath = Phase125SourceJoinFamilyMetadataMaterializationPath,
            nextWork = JsonBool(phase125.RootElement, "familyMetadataMaterialized") is true
                ? "derive or materialize target-blind fermion chargeSector and weak-sector/quantum-number labels, then rerun the corrected-operator transition sweep under a W/Z transition rule"
                : "repair family metadata materialization for all quality source-joined Phase95 modes before deriving fermion sector labels",
        }
        : phase124 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase124.RootElement, "terminalStatus"),
                "phase95-source-join-metadata-materialized-sector-labels-blocked",
                StringComparison.Ordinal)
                ? "source-join-metadata-ready-sector-labels-blocked"
                : "source-join-metadata-materialization-blocked",
            prerequisitesPath = Phase124Phase95SourceJoinMetadataMaterializationPath,
            nextWork = JsonBool(phase124.RootElement, "sourceJoinMetadataMaterialized") is true
                ? "derive or materialize target-blind fermion charge/family/weak-sector labels on the source-join-enriched repaired modes, then rerun the corrected-operator transition sweep under a W/Z transition rule"
                : "repair source-mode join materialization for all quality Phase95 modes before deriving fermion sector labels",
        }
        : phase123 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase123.RootElement, "terminalStatus"),
                "wz-fermion-sector-rule-prerequisites-ready",
                StringComparison.Ordinal)
                ? "wz-fermion-sector-rule-ready"
                : "wz-fermion-sector-rule-prerequisites-blocked",
            prerequisitesPath = Phase123WzFermionSectorMetadataAuditPath,
            nextWork = JsonBool(phase123.RootElement, "fermionSectorRulePromotable") is true
                ? "materialize the derived W/Z fermion transition rule and rerun the corrected-operator transition sweep under that rule"
                : "materialize target-blind fermion charge/family/weak-sector metadata and source-mode join keys on repaired exact modes, then rerun the corrected-operator transition sweep under a derived W/Z transition rule",
        }
        : phase122 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase122.RootElement, "terminalStatus"),
                "corrected-operator-selection-rule-sweep-found-projection-candidate",
                StringComparison.Ordinal)
                ? "corrected-transition-projection-rerun-ready"
                : "corrected-transition-selection-sweep-exhausted",
            prerequisitesPath = Phase122CorrectedOperatorSelectionRuleSweepPath,
            nextWork = "derive the physical W/Z fermion sector transition rule or revise the W/Z bridge construction; corrected operator and Phase95 transition sweep do not produce a projection candidate",
        }
        : phase120 is not null && JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true && phase116 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase116.RootElement, "terminalStatus"),
                "wz-absolute-projection-rerun-target-comparison-passed",
                StringComparison.Ordinal)
                ? "absolute-projection-rerun-passed"
                : "absolute-projection-rerun-failed-after-analytic-variation-repair",
            prerequisitesPath = Phase116WzAbsoluteProjectionRerunPath,
            nextWork = "investigate the now-consistent analytic matrix element cancellation, fermion transition selection rule, or W/Z bridge construction; scalar normalization alone is no longer the blocker",
        }
        : phase120 is not null
        ? new
        {
            status = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true
                ? "analytic-variation-measure-ready"
                : "analytic-variation-measure-consistency-blocked",
            prerequisitesPath = Phase120AnalyticVariationMeasureConsistencyPath,
            nextWork = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true
                ? "rerun Phase115 and W/Z absolute projection with the repaired analytic variation operator and Phase120 target-independent amplitude measure"
                : "repair the analytic replay variation operator or source-mode interpretation until it reproduces Phase12 finite-difference variation matrices with a common target-independent scale",
        }
        : phase119 is not null
        ? new
        {
            status = JsonBool(phase119.RootElement, "promotableAmplitudeScaleFound") is true
                ? "operator-source-scale-ready"
                : "operator-source-scale-derivation-blocked",
            prerequisitesPath = Phase119OperatorSourceScaleDerivationAuditPath,
            nextWork = JsonBool(phase119.RootElement, "promotableAmplitudeScaleFound") is true
                ? "rerun W/Z absolute projection with the target-independent operator/source amplitude scale"
                : "derive and materialize the analytic connection-mode-to-Dirac-variation amplitude measure before rerunning W/Z absolute projection",
        }
        : phase118 is not null
        ? new
        {
            status = "operator-source-scale-blocked",
            prerequisitesPath = Phase118WzMatrixElementNormalizationDiagnosticPath,
            nextWork = "derive or audit the target-independent connection-mode-to-Dirac-variation operator/source scale before replaying W/Z absolute projection",
        }
        : phase117 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase117.RootElement, "terminalStatus"),
                "wz-repaired-pair-sweep-found-projection-candidate",
                StringComparison.Ordinal)
                ? "repaired-pair-rerun-ready"
                : "pair-sweep-exhausted",
            prerequisitesPath = Phase117WzRepairedPairSweepPath,
            nextWork = "investigate analytic matrix-element normalization, boson vector/source normalization, or missing operator-scale factors upstream of the repaired fermion pair choice",
        }
        : phase116 is not null
        ? new
        {
            status = string.Equals(
                JsonString(phase116.RootElement, "terminalStatus"),
                "wz-absolute-projection-rerun-target-comparison-passed",
                StringComparison.Ordinal)
                ? "absolute-projection-rerun-passed"
                : "absolute-projection-rerun-failed",
            prerequisitesPath = Phase116WzAbsoluteProjectionRerunPath,
            nextWork = "repair the W/Z-route matrix-element amplitude and common shared W/Z bridge consistency before another absolute projection rerun",
        }
        : phase115 is not null && JsonBool(phase115.RootElement, "repairAccepted") is true
        ? new
        {
            status = "absolute-projection-rerun-ready",
            prerequisitesPath = Phase115WzRouteFermionQualityReplayPath,
            nextWork = "rerun the absolute W/Z projection with the repaired W/Z-route replay evidence and compare against the Phase110 target-independent repair contract",
        }
        : File.Exists(Phase114WzRouteMatrixElementEvidencePath)
        ? new
        {
            status = "fermion-quality-blocked",
            prerequisitesPath = Phase114WzRouteMatrixElementEvidencePath,
            nextWork = "repair W/Z-route fermion inputs with gauge-reduced exact modes and promoted branch/refinement stability, then rebuild raw matrix-element evidence",
        }
        : File.Exists(Phase113RepairAttemptGatePath)
        ? new
        {
            status = "repair-evidence-blocked",
            prerequisitesPath = Phase113RepairAttemptGatePath,
            nextWork = "produce W/Z-route-compatible replayed matrix-element evidence or target-independent scalar-sector revision evidence before rerunning absolute projection",
        }
        : File.Exists(Phase110WzAbsoluteRepairContractPath)
        ? new
        {
            status = "handoff-ready",
            prerequisitesPath = Phase110WzAbsoluteRepairContractPath,
            nextWork = "execute W/Z absolute-mass repair using the Phase110 target-independent repair contract",
        }
        : File.Exists(Phase108ComparisonClosurePath)
        ? new
        {
            status = "closed",
            prerequisitesPath = Phase105DerivationPrerequisitesPath,
            nextWork = "current candidate-3 path is closed as internal-only unless new observable-identity evidence is introduced",
        }
        : File.Exists(Phase105DerivationPrerequisitesPath)
        ? new
        {
            status = "complete",
            prerequisitesPath = Phase105DerivationPrerequisitesPath,
            nextWork = "derive candidate-3 observable identity and target-independent normalization before physical comparison",
        }
        : new
        {
            status = "missing",
            prerequisitesPath = Phase105DerivationPrerequisitesPath,
            nextWork = "run Phase105 to prepare the candidate-3 physical derivation inputs",
        },
    doesNotClaim = new[]
    {
        "external W/Z or Standard Model boson mass prediction while externalPhysicalComparisonReady is false",
        "candidate-specific physical mapping or calibration for candidate-3",
        "full materialization of secondary and tertiary selector-cell axes",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
var packagePath = Path.Combine(outputDir, "boson_prediction_package.json");
File.WriteAllText(packagePath, JsonSerializer.Serialize(package, options));

var summary = new
{
    phaseId = "phase101-boson-prediction-package",
    terminalStatus,
    predictionLevel = JsonString(readiness.RootElement, "predictionLevel"),
    couplingMagnitude = JsonDouble(readiness.RootElement, "couplingMagnitude"),
    externalPhysicalComparisonReady = externalReady,
    packagePath,
};
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_package_summary.json"),
    JsonSerializer.Serialize(summary, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"packagePath={packagePath}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static JsonDocument? TryParseJson(string path) =>
    File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path)) : null;

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToList();
}
