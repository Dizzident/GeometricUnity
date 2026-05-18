using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase156_boson_generation_execution_package_001/output";
const string GeneratorScriptPath = "scripts/generate_validated_boson_predictions.sh";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase155Path = "studies/phase155_fermion_sector_transition_evidence_derivation_001/output/fermion_sector_transition_evidence_derivation.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase140TemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json";
const string Phase162Path = "studies/phase162_phase_sensitive_wz_bridge_projection_attempt_001/output/phase_sensitive_wz_bridge_projection_attempt.json";
const string Phase163Path = "studies/phase163_source_to_repaired_bridge_transfer_audit_001/output/source_to_repaired_bridge_transfer_audit.json";
const string Phase164Path = "studies/phase164_source_level_wz_bridge_candidate_census_001/output/source_level_wz_bridge_candidate_census.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt.json";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase168Path = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output/source_shape_scalar_relation_closure_audit.json";
const string Phase169Path = "studies/phase169_source_shape_law_stability_experiment_001/output/source_shape_law_stability_experiment.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string Phase171Path = "studies/phase171_branch_stable_bridge_pair_census_001/output/branch_stable_bridge_pair_census.json";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE156_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase155 = JsonDocument.Parse(File.ReadAllText(Phase155Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase140Template = JsonDocument.Parse(File.ReadAllText(Phase140TemplatePath));
using var phase162 = File.Exists(Phase162Path) ? JsonDocument.Parse(File.ReadAllText(Phase162Path)) : null;
using var phase163 = File.Exists(Phase163Path) ? JsonDocument.Parse(File.ReadAllText(Phase163Path)) : null;
using var phase164 = File.Exists(Phase164Path) ? JsonDocument.Parse(File.ReadAllText(Phase164Path)) : null;
using var phase166 = File.Exists(Phase166Path) ? JsonDocument.Parse(File.ReadAllText(Phase166Path)) : null;
using var phase167 = File.Exists(Phase167Path) ? JsonDocument.Parse(File.ReadAllText(Phase167Path)) : null;
using var phase168 = File.Exists(Phase168Path) ? JsonDocument.Parse(File.ReadAllText(Phase168Path)) : null;
using var phase169 = File.Exists(Phase169Path) ? JsonDocument.Parse(File.ReadAllText(Phase169Path)) : null;
using var phase170 = File.Exists(Phase170Path) ? JsonDocument.Parse(File.ReadAllText(Phase170Path)) : null;
using var phase171 = File.Exists(Phase171Path) ? JsonDocument.Parse(File.ReadAllText(Phase171Path)) : null;
using var phase172 = File.Exists(Phase172Path) ? JsonDocument.Parse(File.ReadAllText(Phase172Path)) : null;
using var phase173 = File.Exists(Phase173Path) ? JsonDocument.Parse(File.ReadAllText(Phase173Path)) : null;

int validatedPredictionCount = JsonInt(phase151.RootElement, "validatedPredictionCount") ?? 0;
int failedAttemptCount = JsonInt(phase151.RootElement, "failedAttemptCount") ?? 0;
int blockedRowCount = JsonInt(phase151.RootElement, "blockedRowCount") ?? 0;
bool allRowsValidated = JsonBool(phase151.RootElement, "allRowsValidated") is true;
bool bridgeProjectionBlocked = phase162 is not null
    && string.Equals(JsonString(phase162.RootElement, "terminalStatus"), "phase-sensitive-wz-bridge-projection-absolute-scale-blocked", StringComparison.Ordinal);
bool sourceBridgeBlocked = phase163 is not null
    && string.Equals(JsonString(phase163.RootElement, "terminalStatus"), "source-to-repaired-bridge-transfer-source-already-absolute-scale-blocked", StringComparison.Ordinal);
bool noExistingSourceBridge = phase164 is not null
    && string.Equals(JsonString(phase164.RootElement, "terminalStatus"), "source-level-wz-bridge-candidate-census-no-existing-source-clears-gate", StringComparison.Ordinal);
bool sourceNormalizedAttemptFailed = phase166 is not null
    && string.Equals(JsonString(phase166.RootElement, "terminalStatus"), "source-normalized-wz-prediction-attempt-failed-not-promoted", StringComparison.Ordinal);
bool sourceShapeAttemptNotPromoted = phase167 is not null
    && string.Equals(JsonString(phase167.RootElement, "terminalStatus"), "source-shape-normalized-wz-attempt-common-scale-only-not-promoted", StringComparison.Ordinal);
bool scalarRelationClosureBlocked = phase168 is not null
    && string.Equals(JsonString(phase168.RootElement, "terminalStatus"), "source-shape-scalar-relation-closure-blocked-no-independent-revision", StringComparison.Ordinal);
bool sourceShapeLawStabilityFailed = phase169 is not null
    && string.Equals(JsonString(phase169.RootElement, "terminalStatus"), "source-shape-law-stability-failed-for-p167-law", StringComparison.Ordinal);
bool stableShapePredictionFailed = phase170 is not null
    && string.Equals(JsonString(phase170.RootElement, "terminalStatus"), "stable-source-shape-prediction-failed-not-promoted", StringComparison.Ordinal);
bool branchStableBridgePairCensusExhausted = phase171 is not null
    && string.Equals(JsonString(phase171.RootElement, "terminalStatus"), "branch-stable-bridge-pair-census-no-branch-stable-source-clears-gate", StringComparison.Ordinal);
bool variationSubspaceBridgeNormCensusExhausted = phase172 is not null
    && string.Equals(JsonString(phase172.RootElement, "terminalStatus"), "variation-subspace-bridge-norm-census-no-subspace-source-clears-gate", StringComparison.Ordinal);
bool nextPredictionRouteNoAttempt = phase173 is not null
    && string.Equals(JsonString(phase173.RootElement, "terminalStatus"), "next-prediction-route-no-defensible-attempt", StringComparison.Ordinal);
string terminalStatus = allRowsValidated
    ? "boson-generation-package-complete"
    : nextPredictionRouteNoAttempt
        ? "boson-generation-package-blocked-next-prediction-route-no-defensible-attempt"
    : variationSubspaceBridgeNormCensusExhausted
        ? "boson-generation-package-blocked-variation-subspace-bridge-norm-census-exhausted"
    : branchStableBridgePairCensusExhausted
        ? "boson-generation-package-blocked-branch-stable-bridge-pair-census-exhausted"
    : stableShapePredictionFailed
        ? "boson-generation-package-blocked-stable-source-shape-prediction-failed"
    : sourceShapeLawStabilityFailed
        ? "boson-generation-package-blocked-source-shape-law-not-stable"
    : scalarRelationClosureBlocked
        ? "boson-generation-package-blocked-scalar-relation-revision-required"
    : sourceShapeAttemptNotPromoted
        ? "boson-generation-package-blocked-source-shape-wz-attempt-not-promoted"
    : sourceNormalizedAttemptFailed
        ? "boson-generation-package-blocked-source-normalized-wz-attempt-failed"
    : noExistingSourceBridge
        ? "boson-generation-package-blocked-no-existing-wz-bridge-source"
    : sourceBridgeBlocked
        ? "boson-generation-package-blocked-wz-source-absolute-scale-required"
    : bridgeProjectionBlocked
        ? "boson-generation-package-blocked-wz-bridge-source-normalization-required"
    : "boson-generation-package-blocked-external-sector-evidence-required";

var result = new
{
    phaseId = "phase156-boson-generation-execution-package",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    generatorScriptPath = GeneratorScriptPath,
    currentPredictionState = new
    {
        phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
        knownBosonRowCount = JsonInt(phase151.RootElement, "knownBosonRowCount"),
        validatedPredictionCount,
        failedAttemptCount,
        blockedRowCount,
        allRowsValidated,
        validatedPredictions = phase151.RootElement.GetProperty("validatedPredictions").Clone(),
        failedAttempts = phase151.RootElement.GetProperty("failedAttempts").Clone(),
        blockedRows = phase151.RootElement.GetProperty("blockedRows").Clone(),
    },
    finalKnownBlocker = nextPredictionRouteNoAttempt
        ? JsonString(phase173!.RootElement, "predictionAttemptMadeReason")
        : variationSubspaceBridgeNormCensusExhausted
        ? JsonString(phase172!.RootElement, "diagnosis")
        : branchStableBridgePairCensusExhausted
        ? JsonString(phase171!.RootElement, "diagnosis")
        : stableShapePredictionFailed
        ? JsonString(phase170!.RootElement, "diagnosis")
        : sourceShapeLawStabilityFailed
        ? JsonString(phase169!.RootElement, "diagnosis")
        : scalarRelationClosureBlocked
        ? JsonString(phase168!.RootElement, "diagnosis")
        : sourceShapeAttemptNotPromoted
        ? JsonString(phase167!.RootElement, "diagnosis")
        : sourceNormalizedAttemptFailed
        ? JsonString(phase166!.RootElement, "diagnosis")
        : noExistingSourceBridge
        ? JsonString(phase164!.RootElement, "diagnosis")
        : sourceBridgeBlocked
        ? JsonString(phase163!.RootElement, "diagnosis")
        : bridgeProjectionBlocked
        ? JsonString(phase162!.RootElement, "diagnosis")
        : JsonString(phase155.RootElement, "finalKnownBlocker"),
    fermionSectorEvidenceStatus = new
    {
        phase155Status = JsonString(phase155.RootElement, "terminalStatus"),
        anyPromotableEvidence = JsonBool(phase155.RootElement, "anyPromotableEvidence"),
        projectionCandidateFound = JsonBool(phase155.RootElement, "projectionCandidateFound"),
        evidenceAssessments = phase155.RootElement.GetProperty("evidenceAssessments").Clone(),
    },
    requiredIntake = new
    {
        contractPath = Phase140Path,
        templatePath = Phase140TemplatePath,
        acceptableArtifactKinds = phase140.RootElement.GetProperty("targetContract").GetProperty("acceptableArtifactKinds").Clone(),
        rejectedShortcuts = phase140.RootElement.GetProperty("targetContract").GetProperty("rejectedShortcuts").Clone(),
        targetRows = phase140Template.RootElement.GetProperty("rows").Clone(),
        transitionRuleTemplate = phase140Template.RootElement.GetProperty("transitionRule").Clone(),
    },
    phaseSensitiveBridgeStatus = phase162 is null
        ? null
        : new
        {
            phase162Status = JsonString(phase162.RootElement, "terminalStatus"),
            projectionCandidateReady = JsonBool(phase162.RootElement, "projectionCandidateReady"),
            nextWork = JsonString(phase162.RootElement, "nextWork"),
        },
    sourceToRepairedBridgeTransferStatus = phase163 is null
        ? null
        : new
        {
            phase163Status = JsonString(phase163.RootElement, "terminalStatus"),
            sourceBestRawToTargetRatio = JsonDouble(phase163.RootElement.GetProperty("transfer"), "sourceBestRawToTargetRatio"),
            repairedBestRawToTargetRatio = JsonDouble(phase163.RootElement.GetProperty("transfer"), "repairedBestRawToTargetRatio"),
            sourceAlreadyBlocked = JsonBool(phase163.RootElement.GetProperty("transfer"), "sourceAlreadyBlocked"),
            repairSuppressesAmplitude = JsonBool(phase163.RootElement.GetProperty("transfer"), "repairSuppressesAmplitude"),
            nextWork = JsonString(phase163.RootElement, "nextWork"),
        },
    sourceLevelBridgeCandidateCensusStatus = phase164 is null
        ? null
        : new
        {
            phase164Status = JsonString(phase164.RootElement, "terminalStatus"),
            candidateCount = JsonInt(phase164.RootElement, "candidateCount"),
            promotableCandidateCount = JsonInt(phase164.RootElement, "promotableCandidateCount"),
            bestCandidate = phase164.RootElement.GetProperty("bestCandidate").Clone(),
            nextWork = JsonString(phase164.RootElement, "nextWork"),
        },
    sourceNormalizedWzPredictionAttemptStatus = phase166 is null
        ? null
        : new
        {
            phase166Status = JsonString(phase166.RootElement, "terminalStatus"),
            normalizationCandidate = phase166.RootElement.GetProperty("normalizationCandidate").Clone(),
            gates = phase166.RootElement.GetProperty("gates").Clone(),
            predictionAttempts = phase166.RootElement.GetProperty("predictionAttempts").Clone(),
            nextWork = JsonString(phase166.RootElement, "nextWork"),
        },
    sourceShapeNormalizedWzAttemptStatus = phase167 is null
        ? null
        : new
        {
            phase167Status = JsonString(phase167.RootElement, "terminalStatus"),
            bestAttempt = phase167.RootElement.GetProperty("bestAttempt").Clone(),
            attemptCount = JsonInt(phase167.RootElement, "attemptCount"),
            promotableAttemptCount = JsonInt(phase167.RootElement, "promotableAttemptCount"),
            commonScalePassingAttemptCount = JsonInt(phase167.RootElement, "commonScalePassingAttemptCount"),
            nextWork = JsonString(phase167.RootElement, "nextWork"),
        },
    sourceShapeScalarRelationClosureStatus = phase168 is null
        ? null
        : new
        {
            phase168Status = JsonString(phase168.RootElement, "terminalStatus"),
            p167BestCandidateId = JsonString(phase168.RootElement, "p167BestCandidateId"),
            diagnosticCommonFactor = JsonDouble(phase168.RootElement, "diagnosticCommonFactor"),
            bestAttempt = phase168.RootElement.GetProperty("bestAttempt").Clone(),
            nextWork = JsonString(phase168.RootElement, "nextWork"),
        },
    sourceShapeLawStabilityStatus = phase169 is null
        ? null
        : new
        {
            phase169Status = JsonString(phase169.RootElement, "terminalStatus"),
            p167BestLawId = JsonString(phase169.RootElement, "p167BestLawId"),
            p167BestLawStable = JsonBool(phase169.RootElement, "p167BestLawStable"),
            bestStableLaw = phase169.RootElement.TryGetProperty("bestStableLaw", out var bestStableLaw) ? bestStableLaw.Clone() : default,
            nextWork = JsonString(phase169.RootElement, "nextWork"),
        },
    stableSourceShapePredictionStatus = phase170 is null
        ? null
        : new
        {
            phase170Status = JsonString(phase170.RootElement, "terminalStatus"),
            stableLawIds = phase170.RootElement.GetProperty("stableLawIds").Clone(),
            bestAttempt = phase170.RootElement.TryGetProperty("bestAttempt", out var bestStableAttempt) ? bestStableAttempt.Clone() : default,
            nextWork = JsonString(phase170.RootElement, "nextWork"),
        },
    branchStableBridgePairCensusStatus = phase171 is null
        ? null
        : new
        {
            phase171Status = JsonString(phase171.RootElement, "terminalStatus"),
            assessmentCount = JsonInt(phase171.RootElement, "assessmentCount"),
            rawGateClearingAssessmentCount = JsonInt(phase171.RootElement, "rawGateClearingAssessmentCount"),
            stableRawGateClearingAssessmentCount = JsonInt(phase171.RootElement, "stableRawGateClearingAssessmentCount"),
            bestAssessment = phase171.RootElement.TryGetProperty("bestAssessment", out var bestBridgePair) ? bestBridgePair.Clone() : default,
            nextWork = JsonString(phase171.RootElement, "nextWork"),
        },
    variationSubspaceBridgeNormCensusStatus = phase172 is null
        ? null
        : new
        {
            phase172Status = JsonString(phase172.RootElement, "terminalStatus"),
            assessmentCount = JsonInt(phase172.RootElement, "assessmentCount"),
            rawGatePassingAssessmentCount = JsonInt(phase172.RootElement, "rawGatePassingAssessmentCount"),
            stableRawGatePassingAssessmentCount = JsonInt(phase172.RootElement, "stableRawGatePassingAssessmentCount"),
            bestAssessment = phase172.RootElement.TryGetProperty("bestAssessment", out var bestSubspace) ? bestSubspace.Clone() : default,
            nextWork = JsonString(phase172.RootElement, "nextWork"),
        },
    nextPredictionRouteSelectionStatus = phase173 is null
        ? null
        : new
        {
            phase173Status = JsonString(phase173.RootElement, "terminalStatus"),
            bestNextRoute = phase173.RootElement.TryGetProperty("bestNextRoute", out var bestNextRoute) ? bestNextRoute.Clone() : default,
            diagnosticMasslessGaugeObservation = phase173.RootElement.TryGetProperty("diagnosticMasslessGaugeObservation", out var masslessDiagnostic) ? masslessDiagnostic.Clone() : default,
            predictionAttemptMade = JsonBool(phase173.RootElement, "predictionAttemptMade"),
            predictionAttemptMadeReason = JsonString(phase173.RootElement, "predictionAttemptMadeReason"),
            nextWork = JsonString(phase173.RootElement, "nextWork"),
        },
    executionSteps = allRowsValidated
        ? new[] { "review Phase151 validated predictions for promotion" }
        : nextPredictionRouteNoAttempt
            ? new[]
            {
                "materialize U(1) photon and color-octet gluon identity/benchmark contracts, or prove a branch-stable protected massless subspace; P173 shows a massless diagnostic exists but cannot be promoted",
                "do not promote photon/gluon masslessness from generic zero boson eigenvalues without identity and target/benchmark contracts",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : variationSubspaceBridgeNormCensusExhausted
            ? new[]
            {
                "derive a W/Z bridge source outside the current Phase12 variation subspace, or provide a derivation-backed analytic source-shape law; P172 exhausts the full current variation subspace norm",
                "do not promote any local Phase12 variation-subspace source as the absolute W/Z bridge",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : branchStableBridgePairCensusExhausted
            ? new[]
            {
                "derive a new W/Z bridge source outside the current Phase91 promoted pair family, or provide analytic source-shape law evidence; P171 exhausts the current branch-stable pair census",
                "do not promote any local Phase91/Phase12 pair as the absolute W/Z bridge source",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : stableShapePredictionFailed
            ? new[]
            {
                "derive a new W/Z bridge source or an analytic source-shape law that is both stable and target-validating; P170 shows stable laws do not pass prediction gates",
                "do not promote stable source-shape diagnostic attempts",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : sourceShapeLawStabilityFailed
            ? new[]
            {
                "derive a different source-shape law or a new W/Z bridge source; P169 shows the P167 L1 diagnostic law is not stable across sibling Phase12 source backgrounds",
                "do not promote the P167 diagnostic law",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : scalarRelationClosureBlocked
            ? new[]
            {
                "derive an independent scalar-sector relation revision or analytic source-shape normalization law; P168 shows the remaining scalar factor is target-implied diagnostic evidence and still fails validation",
                "do not promote target-implied scalar factors as predictions",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : sourceShapeAttemptNotPromoted
            ? new[]
            {
                "derive an analytic source-shape normalization law; P167 shows L1 shape correction repairs common-scale spread diagnostically but is not derivation-backed and fails target comparison",
                "do not promote the P167 diagnostic W/Z mass attempts",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : sourceNormalizedAttemptFailed
            ? new[]
            {
                "derive a source-level amplitude normalization with W/Z common-scale spread at or below 5 percent, or derive a new W/Z bridge source",
                "do not promote the P166 diagnostic W/Z mass attempts; they fail common-scale and target-comparison gates",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : noExistingSourceBridge
            ? new[]
            {
                "derive a new target-independent W/Z bridge source or source-level amplitude normalization; the existing Phase12 variation matrices do not supply the absolute W/Z bridge",
                "do not choose a source by nearest W/Z target residual or fit an amplitude scale from W/Z target values",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : sourceBridgeBlocked
            ? new[]
            {
                "derive a different target-independent W/Z bridge source or a source-level absolute normalization before using candidate-8 for absolute W/Z masses",
                "do not repair this by fitting the bridge from W/Z target residuals",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : bridgeProjectionBlocked
            ? new[]
            {
                "derive a target-independent W/Z bridge source or normalization connecting the accepted phase-sensitive candidate-8 transition to the Phase69 W/Z mass-generation relation",
                "do not fit the bridge from W/Z target residuals",
                "rerun ./scripts/generate_validated_boson_predictions.sh",
                "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
            }
        : new[]
        {
            "derive or supply a real target-independent fermion-sector label table, nontrivial transition rule, or W/Z bridge-revision artifact",
            "place it through the P140 intake contract without using W/Z target residuals",
            "run ./scripts/generate_validated_boson_predictions.sh",
            "promote only rows that Phase151 reports with promotionAllowed=true and passed=true",
        },
    sourceEvidence = new
    {
        phase151Path = Phase151Path,
        phase155Path = Phase155Path,
        phase140Path = Phase140Path,
        phase140TemplatePath = Phase140TemplatePath,
        phase162Path = File.Exists(Phase162Path) ? Phase162Path : null,
        phase163Path = File.Exists(Phase163Path) ? Phase163Path : null,
        phase164Path = File.Exists(Phase164Path) ? Phase164Path : null,
        phase166Path = File.Exists(Phase166Path) ? Phase166Path : null,
        phase167Path = File.Exists(Phase167Path) ? Phase167Path : null,
        phase168Path = File.Exists(Phase168Path) ? Phase168Path : null,
        phase169Path = File.Exists(Phase169Path) ? Phase169Path : null,
        phase170Path = File.Exists(Phase170Path) ? Phase170Path : null,
        phase171Path = File.Exists(Phase171Path) ? Phase171Path : null,
        phase172Path = File.Exists(Phase172Path) ? Phase172Path : null,
        phase173Path = File.Exists(Phase173Path) ? Phase173Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_generation_execution_package.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_generation_execution_package_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.generatorScriptPath,
        result.currentPredictionState,
        result.finalKnownBlocker,
        result.phaseSensitiveBridgeStatus,
        result.sourceToRepairedBridgeTransferStatus,
        result.sourceLevelBridgeCandidateCensusStatus,
        result.sourceNormalizedWzPredictionAttemptStatus,
        result.sourceShapeNormalizedWzAttemptStatus,
        result.sourceShapeScalarRelationClosureStatus,
        result.sourceShapeLawStabilityStatus,
        result.stableSourceShapePredictionStatus,
        result.branchStableBridgePairCensusStatus,
        result.variationSubspaceBridgeNormCensusStatus,
        result.nextPredictionRouteSelectionStatus,
        result.requiredIntake,
        result.executionSteps,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_generation_execution_package.md"),
    BuildMarkdown(
        terminalStatus,
        validatedPredictionCount,
        failedAttemptCount,
        blockedRowCount,
        result.finalKnownBlocker,
        result.executionSteps));

Console.WriteLine(terminalStatus);
Console.WriteLine($"validatedPredictionCount={validatedPredictionCount}");
Console.WriteLine($"failedAttemptCount={failedAttemptCount}");
Console.WriteLine($"blockedRowCount={blockedRowCount}");
Console.WriteLine($"generatorScriptPath={GeneratorScriptPath}");

static string BuildMarkdown(
    string terminalStatus,
    int validatedPredictionCount,
    int failedAttemptCount,
    int blockedRowCount,
    string? finalKnownBlocker,
    IReadOnlyList<string> executionSteps)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Boson Generation Execution Package");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine($"Validated predictions: {validatedPredictionCount}");
    builder.AppendLine($"Failed non-promotable attempts: {failedAttemptCount}");
    builder.AppendLine($"Blocked rows: {blockedRowCount}");
    if (!string.IsNullOrWhiteSpace(finalKnownBlocker))
    {
        builder.AppendLine();
        builder.AppendLine($"Final known blocker: {finalKnownBlocker}");
    }

    builder.AppendLine();
    builder.AppendLine("## Execution Steps");
    foreach (var step in executionSteps)
        builder.AppendLine($"- {step}");

    return builder.ToString();
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
