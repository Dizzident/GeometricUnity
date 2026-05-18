using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase157_scientific_defensibility_boundary_001/output";
const string Phase126Path = "studies/phase126_fermion_sector_identity_source_audit_001/output/fermion_sector_identity_source_audit.json";
const string Phase127Path = "studies/phase127_fermion_gauge_basis_content_observable_001/output/fermion_gauge_basis_content_observable.json";
const string Phase128Path = "studies/phase128_fermion_su2_generator_sector_observable_001/output/fermion_su2_generator_sector_observable.json";
const string Phase145Path = "studies/phase145_fermion_sector_intake_unlock_fixture_001/output/fermion_sector_intake_unlock_fixture.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";
const string Phase156Path = "studies/phase156_boson_generation_execution_package_001/output/boson_generation_execution_package.json";
const string Phase160Path = "studies/phase160_phase_sensitive_transition_rule_materialization_001/output/phase_sensitive_transition_rule_materialization.json";
const string Phase164Path = "studies/phase164_source_level_wz_bridge_candidate_census_001/output/source_level_wz_bridge_candidate_census.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt.json";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase168Path = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output/source_shape_scalar_relation_closure_audit.json";
const string Phase169Path = "studies/phase169_source_shape_law_stability_experiment_001/output/source_shape_law_stability_experiment.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string Phase171Path = "studies/phase171_branch_stable_bridge_pair_census_001/output/branch_stable_bridge_pair_census.json";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE157_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase126 = JsonDocument.Parse(File.ReadAllText(Phase126Path));
using var phase127 = JsonDocument.Parse(File.ReadAllText(Phase127Path));
using var phase128 = JsonDocument.Parse(File.ReadAllText(Phase128Path));
using var phase145 = JsonDocument.Parse(File.ReadAllText(Phase145Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));
using var phase156 = JsonDocument.Parse(File.ReadAllText(Phase156Path));
using var phase160 = File.Exists(Phase160Path) ? JsonDocument.Parse(File.ReadAllText(Phase160Path)) : null;
using var phase164 = File.Exists(Phase164Path) ? JsonDocument.Parse(File.ReadAllText(Phase164Path)) : null;
using var phase166 = File.Exists(Phase166Path) ? JsonDocument.Parse(File.ReadAllText(Phase166Path)) : null;
using var phase167 = File.Exists(Phase167Path) ? JsonDocument.Parse(File.ReadAllText(Phase167Path)) : null;
using var phase168 = File.Exists(Phase168Path) ? JsonDocument.Parse(File.ReadAllText(Phase168Path)) : null;
using var phase169 = File.Exists(Phase169Path) ? JsonDocument.Parse(File.ReadAllText(Phase169Path)) : null;
using var phase170 = File.Exists(Phase170Path) ? JsonDocument.Parse(File.ReadAllText(Phase170Path)) : null;
using var phase171 = File.Exists(Phase171Path) ? JsonDocument.Parse(File.ReadAllText(Phase171Path)) : null;
using var phase172 = File.Exists(Phase172Path) ? JsonDocument.Parse(File.ReadAllText(Phase172Path)) : null;
using var phase173 = File.Exists(Phase173Path) ? JsonDocument.Parse(File.ReadAllText(Phase173Path)) : null;

var rejectedShortcuts = new[]
{
    Shortcut(
        "transfer-boson-charge-sector-labels-to-fermions",
        "rejected",
        "Phase126 records that Phase27 charge sectors label boson source families only and are not transferable to fermion sector identity."),
    Shortcut(
        "promote-mixed-gauge-basis-fractions",
        "rejected",
        "Phase127 materializes gauge-basis fractions, but both quality modes are mixed and fail dominance/gap gates."),
    Shortcut(
        "promote-mixed-t3-generator-sector",
        "rejected",
        "Phase128 materializes SU(2) generator moments, but both quality modes are mixed in the T3 eigenbasis and near-zero polarized."),
    Shortcut(
        "use-trivial-chirality-or-missing-conjugation",
        "rejected",
        "Phase126 records trivial chirality and no conjugation-pair evidence for the repaired families."),
    Shortcut(
        "use-synthetic-intake-fixture",
        "rejected",
        "Phase145 validates only synthetic unlock fixtures; P146 finds no non-synthetic promotable candidate."),
    Shortcut(
        "fit-sector-or-bridge-labels-from-wz-target-residuals",
        "rejected",
        "P140/P156 require target-independent evidence and explicitly forbid inferring labels from W/Z target residuals."),
    Shortcut(
        "promote-nearest-existing-phase12-variation-as-wz-bridge",
        "rejected",
        "P164 scans existing Phase12 variation matrices and finds that none clears the source-level W/Z raw-amplitude gate."),
    Shortcut(
        "promote-best-dimensional-source-normalization",
        "rejected",
        "P166 applies the best P118 dimensional normalization candidate, but it fails W/Z common-scale and target-comparison gates."),
    Shortcut(
        "promote-source-shape-normalization-without-derivation",
        "rejected",
        "P167 finds source-shape corrections that improve common-scale spread, but the best diagnostic candidate is not derivation-backed and still fails target comparison."),
    Shortcut(
        "promote-target-implied-scalar-relation-factor",
        "rejected",
        "P168 shows the remaining scalar factor is target-implied diagnostic evidence and cannot be used as a prediction."),
    Shortcut(
        "promote-unstable-source-shape-law",
        "rejected",
        "P169 shows the P167 L1 source-shape law is not stable across sibling Phase12 source backgrounds."),
    Shortcut(
        "promote-stable-source-shape-law-without-prediction-validation",
        "rejected",
        "P170 shows the P169-stable source-shape laws do not pass the W/Z prediction gates."),
    Shortcut(
        "promote-local-branch-stable-pair-as-wz-bridge",
        "rejected",
        "P171 exhausts the promoted Phase91 branch-stable mode-index pair family and finds no raw-amplitude-clearing W/Z bridge source."),
    Shortcut(
        "promote-local-variation-subspace-as-wz-bridge",
        "rejected",
        "P172 exhausts the full current Phase12 variation subspace norm and finds no raw-amplitude-clearing W/Z bridge source."),
    Shortcut(
        "promote-generic-zero-boson-modes-as-photon-or-gluon",
        "rejected",
        "P173 finds a numerical massless-gauge diagnostic, but no U(1) photon identity, color-octet gluon identity, target/benchmark contracts, or branch-stable protected subspace evidence."),
};

bool anyShortcutAllowed = rejectedShortcuts.Any(shortcut => shortcut.Status != "rejected");
bool phase160TransitionRulePromotable = phase160 is not null && JsonBool(phase160.RootElement, "transitionRulePromotable") is true;
bool phase164NoExistingBridgeSource = phase164 is not null
    && string.Equals(JsonString(phase164.RootElement, "terminalStatus"), "source-level-wz-bridge-candidate-census-no-existing-source-clears-gate", StringComparison.Ordinal);
bool localEvidencePresent = JsonBool(phase146.RootElement, "currentEvidencePresent") is true || phase160TransitionRulePromotable;
bool localEvidenceExhaustedByBridgeCensus = phase164NoExistingBridgeSource
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-no-existing-wz-bridge-source", StringComparison.Ordinal);
bool phase166SourceNormalizationFailed = phase166 is not null
    && string.Equals(JsonString(phase166.RootElement, "terminalStatus"), "source-normalized-wz-prediction-attempt-failed-not-promoted", StringComparison.Ordinal);
bool localEvidenceExhaustedByNormalizationAttempt = phase166SourceNormalizationFailed
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-source-normalized-wz-attempt-failed", StringComparison.Ordinal);
bool phase167SourceShapeNotPromoted = phase167 is not null
    && string.Equals(JsonString(phase167.RootElement, "terminalStatus"), "source-shape-normalized-wz-attempt-common-scale-only-not-promoted", StringComparison.Ordinal);
bool localEvidenceExhaustedBySourceShapeAttempt = phase167SourceShapeNotPromoted
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-source-shape-wz-attempt-not-promoted", StringComparison.Ordinal);
bool phase168ScalarRelationBlocked = phase168 is not null
    && string.Equals(JsonString(phase168.RootElement, "terminalStatus"), "source-shape-scalar-relation-closure-blocked-no-independent-revision", StringComparison.Ordinal);
bool localEvidenceExhaustedByScalarRelationAudit = phase168ScalarRelationBlocked
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-scalar-relation-revision-required", StringComparison.Ordinal);
bool phase169ShapeLawStabilityFailed = phase169 is not null
    && string.Equals(JsonString(phase169.RootElement, "terminalStatus"), "source-shape-law-stability-failed-for-p167-law", StringComparison.Ordinal);
bool localEvidenceExhaustedByShapeLawStability = phase169ShapeLawStabilityFailed
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-source-shape-law-not-stable", StringComparison.Ordinal);
bool phase170StableShapePredictionFailed = phase170 is not null
    && string.Equals(JsonString(phase170.RootElement, "terminalStatus"), "stable-source-shape-prediction-failed-not-promoted", StringComparison.Ordinal);
bool localEvidenceExhaustedByStableShapePrediction = phase170StableShapePredictionFailed
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-stable-source-shape-prediction-failed", StringComparison.Ordinal);
bool phase171BranchStableBridgePairCensusExhausted = phase171 is not null
    && string.Equals(JsonString(phase171.RootElement, "terminalStatus"), "branch-stable-bridge-pair-census-no-branch-stable-source-clears-gate", StringComparison.Ordinal);
bool localEvidenceExhaustedByBranchStableBridgePairCensus = phase171BranchStableBridgePairCensusExhausted
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-branch-stable-bridge-pair-census-exhausted", StringComparison.Ordinal);
bool phase172VariationSubspaceBridgeNormCensusExhausted = phase172 is not null
    && string.Equals(JsonString(phase172.RootElement, "terminalStatus"), "variation-subspace-bridge-norm-census-no-subspace-source-clears-gate", StringComparison.Ordinal);
bool localEvidenceExhaustedByVariationSubspaceBridgeNormCensus = phase172VariationSubspaceBridgeNormCensusExhausted
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-variation-subspace-bridge-norm-census-exhausted", StringComparison.Ordinal);
bool phase173NextPredictionRouteNoAttempt = phase173 is not null
    && string.Equals(JsonString(phase173.RootElement, "terminalStatus"), "next-prediction-route-no-defensible-attempt", StringComparison.Ordinal);
bool localEvidenceExhaustedByNextPredictionRouteSelection = phase173NextPredictionRouteNoAttempt
    && string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-next-prediction-route-no-defensible-attempt", StringComparison.Ordinal);
bool generationComplete = string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-complete", StringComparison.Ordinal);
string terminalStatus = generationComplete
    ? "scientific-defensibility-boundary-not-needed"
    : !anyShortcutAllowed && (!localEvidencePresent || localEvidenceExhaustedByBridgeCensus || localEvidenceExhaustedByNormalizationAttempt || localEvidenceExhaustedBySourceShapeAttempt || localEvidenceExhaustedByScalarRelationAudit || localEvidenceExhaustedByShapeLawStability || localEvidenceExhaustedByStableShapePrediction || localEvidenceExhaustedByBranchStableBridgePairCensus || localEvidenceExhaustedByVariationSubspaceBridgeNormCensus || localEvidenceExhaustedByNextPredictionRouteSelection)
        ? "scientific-defensibility-boundary-reached"
        : "scientific-defensibility-boundary-review-required";

var result = new
{
    phaseId = "phase157-scientific-defensibility-boundary",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canDefensiblyProgressFromCurrentLocalArtifacts = terminalStatus != "scientific-defensibility-boundary-reached",
    finalKnownBlocker = JsonString(phase156.RootElement, "finalKnownBlocker"),
    rejectedShortcuts,
    evidenceSummary = new
    {
        phase126Status = JsonString(phase126.RootElement, "terminalStatus"),
        phase127Status = JsonString(phase127.RootElement, "terminalStatus"),
        phase128Status = JsonString(phase128.RootElement, "terminalStatus"),
        phase145Status = JsonString(phase145.RootElement, "terminalStatus"),
        phase146Status = JsonString(phase146.RootElement, "terminalStatus"),
        phase156Status = JsonString(phase156.RootElement, "terminalStatus"),
        phase160Status = phase160 is null ? null : JsonString(phase160.RootElement, "terminalStatus"),
        phase164Status = phase164 is null ? null : JsonString(phase164.RootElement, "terminalStatus"),
        phase166Status = phase166 is null ? null : JsonString(phase166.RootElement, "terminalStatus"),
        phase167Status = phase167 is null ? null : JsonString(phase167.RootElement, "terminalStatus"),
        phase168Status = phase168 is null ? null : JsonString(phase168.RootElement, "terminalStatus"),
        phase169Status = phase169 is null ? null : JsonString(phase169.RootElement, "terminalStatus"),
        phase170Status = phase170 is null ? null : JsonString(phase170.RootElement, "terminalStatus"),
        phase171Status = phase171 is null ? null : JsonString(phase171.RootElement, "terminalStatus"),
        phase172Status = phase172 is null ? null : JsonString(phase172.RootElement, "terminalStatus"),
        phase173Status = phase173 is null ? null : JsonString(phase173.RootElement, "terminalStatus"),
        phase146PromotableCandidateCount = JsonInt(phase146.RootElement, "promotableCandidateCount"),
        phase160TransitionRulePromotable,
        phase164NoExistingBridgeSource,
        localEvidenceExhaustedByBridgeCensus,
        phase166SourceNormalizationFailed,
        localEvidenceExhaustedByNormalizationAttempt,
        phase167SourceShapeNotPromoted,
        localEvidenceExhaustedBySourceShapeAttempt,
        phase168ScalarRelationBlocked,
        localEvidenceExhaustedByScalarRelationAudit,
        phase169ShapeLawStabilityFailed,
        localEvidenceExhaustedByShapeLawStability,
        phase170StableShapePredictionFailed,
        localEvidenceExhaustedByStableShapePrediction,
        phase171BranchStableBridgePairCensusExhausted,
        localEvidenceExhaustedByBranchStableBridgePairCensus,
        phase172VariationSubspaceBridgeNormCensusExhausted,
        localEvidenceExhaustedByVariationSubspaceBridgeNormCensus,
        phase173NextPredictionRouteNoAttempt,
        localEvidenceExhaustedByNextPredictionRouteSelection,
    },
    requiredExternalEvidence = new
    {
        artifactKind = "target-independent fermion-sector label table, nontrivial transition rule, or W/Z bridge-revision artifact",
        intakeContractPath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json",
        intakeTemplatePath = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json",
        rerunCommand = "./scripts/generate_validated_boson_predictions.sh",
    },
    sourceEvidence = new
    {
        phase126Path = Phase126Path,
        phase127Path = Phase127Path,
        phase128Path = Phase128Path,
        phase145Path = Phase145Path,
        phase146Path = Phase146Path,
        phase156Path = Phase156Path,
        phase160Path = File.Exists(Phase160Path) ? Phase160Path : null,
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
File.WriteAllText(Path.Combine(outputDir, "scientific_defensibility_boundary.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "scientific_defensibility_boundary_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canDefensiblyProgressFromCurrentLocalArtifacts,
        result.finalKnownBlocker,
        result.rejectedShortcuts,
        result.evidenceSummary,
        result.requiredExternalEvidence,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "scientific_defensibility_boundary.md"),
    BuildMarkdown(terminalStatus, result.finalKnownBlocker, rejectedShortcuts));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canDefensiblyProgressFromCurrentLocalArtifacts={result.canDefensiblyProgressFromCurrentLocalArtifacts}");
Console.WriteLine($"localEvidencePresent={localEvidencePresent}");

static ShortcutRecord Shortcut(string id, string status, string reason) => new(id, status, reason);

static string BuildMarkdown(string terminalStatus, string? finalKnownBlocker, IReadOnlyList<ShortcutRecord> rejectedShortcuts)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Scientific Defensibility Boundary");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    if (!string.IsNullOrWhiteSpace(finalKnownBlocker))
    {
        builder.AppendLine();
        builder.AppendLine($"Final known blocker: {finalKnownBlocker}");
    }

    builder.AppendLine();
    builder.AppendLine("## Rejected Shortcuts");
    foreach (var shortcut in rejectedShortcuts)
        builder.AppendLine($"- `{shortcut.ShortcutId}`: {shortcut.Reason}");
    return builder.ToString();
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record ShortcutRecord(string ShortcutId, string Status, string Reason);
