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
        materializedModePath = JsonString(phase99.RootElement, "materializedModePath"),
        replayProbePath = JsonString(phase99.RootElement, "replayProbePath"),
    },
    nextPhasePrerequisites = phase119 is not null
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
