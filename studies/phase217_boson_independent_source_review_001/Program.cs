using System.Text.Json;

const string DefaultOutputDir = "studies/phase217_boson_independent_source_review_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase208Path = "studies/phase208_boson_local_route_exhaustion_certificate_001/output/boson_local_route_exhaustion_certificate_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase216Path = "studies/phase216_boson_nonclaim_firewall_001/output/boson_nonclaim_firewall_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE217_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase208 = JsonDocument.Parse(File.ReadAllText(Phase208Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase216 = JsonDocument.Parse(File.ReadAllText(Phase216Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));

var p190Law = RequiredObject(phase190.RootElement, "branchLocalLaw");
var p191Gates = RequiredObject(phase191.RootElement, "gates");
var p213Scans = RequiredObject(phase213.RootElement, "scans");

var p190CandidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
var p190TargetObservablesUsed = JsonBool(phase190.RootElement, "targetObservablesUsed") is true;
var p190StableCandidateCount = JsonInt(RequiredObject(phase190.RootElement, "siblingStability"), "stableCandidateCount") ?? 0;
var targetIndependentWzCandidateEvaluated = p190CandidateLawConstructed && !p190TargetObservablesUsed;
var stableTargetIndependentWzCandidateExists =
    targetIndependentWzCandidateEvaluated
    && p190StableCandidateCount > 0;
var wzSourceLineagePromotable =
    JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true
    && JsonNestedBool(phase210.RootElement, "wzApplication", "readyForApplication") is true;
var higgsSourceLineagePromotable =
    JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true
    && JsonNestedBool(phase210.RootElement, "higgsApplication", "readyForApplication") is true;
var repoSourceScanFoundIntakeReadyEvidence =
    (JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? 0) > 0
    || (JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? 0) > 0
    || JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var localRoutesExhausted =
    JsonBool(phase208.RootElement, "localRouteExhaustionCertified") is true
    && JsonBool(phase208.RootElement, "anyCurrentLocalRouteActionable") is false;
var promotionRerunAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var blockerMatrixReady = JsonBool(phase213.RootElement, "blockerMatrixReady") is true;
var nonclaimFirewallReady = JsonBool(phase216.RootElement, "nonclaimFirewallReady") is true;
var draftProvidesCompletionSource =
    JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw") is true
    || JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is true;
var officialPublicSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;

var noFixableImplementationRoute =
    targetIndependentWzCandidateEvaluated
    && !wzSourceLineagePromotable
    && !higgsSourceLineagePromotable
    && !repoSourceScanFoundIntakeReadyEvidence
    && !promotionRerunAllowed
    && !draftProvidesCompletionSource
    && officialPublicSourceAuditMaterialized
    && !officialDraftProvidesCompletionSource
    && localRoutesExhausted
    && blockerMatrixReady
    && nonclaimFirewallReady;

var terminalStatus = noFixableImplementationRoute
    ? "boson-independent-source-review-no-fixable-implementation-route"
    : "boson-independent-source-review-action-required";

var result = new
{
    phaseId = "phase217-boson-independent-source-review",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    independentReviewMaterialized = true,
    noFixableImplementationRoute,
    promotionRerunAllowed,
    repoSourceScanFoundIntakeReadyEvidence,
    localRoutesExhausted,
    draftProvidesCompletionSource,
    officialPublicSourceAuditMaterialized,
    officialDraftProvidesCompletionSource,
    wzReview = new
    {
        targetIndependentCandidateEvaluated = targetIndependentWzCandidateEvaluated,
        stableTargetIndependentCandidateExists = stableTargetIndependentWzCandidateExists,
        stableCandidateCount = p190StableCandidateCount,
        sourceLineagePromotable = wzSourceLineagePromotable,
        lawId = JsonString(p190Law, "lawId"),
        formula = JsonString(p190Law, "formula"),
        theoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed"),
        rawGatePassed = JsonBool(p191Gates, "rawGatePassed"),
        bestRawToTargetRatio = JsonDouble(p191Gates, "bestRawToTargetRatio"),
        wZParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent"),
        targetComparisonRunnable = JsonBool(p191Gates, "targetComparisonRunnable"),
        decision = JsonString(phase191.RootElement, "decision"),
        nextRequiredArtifact = JsonString(phase191.RootElement, "nextRequiredArtifact"),
    },
    higgsReview = new
    {
        sourceLineagePromotable = higgsSourceLineagePromotable,
        canPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage"),
        decision = JsonString(phase199.RootElement, "decision"),
        nextRequiredArtifact = JsonString(phase199.RootElement, "nextRequiredArtifact"),
    },
    evidenceReadiness = new
    {
        jsonIntakeReadyCount = JsonInt(p213Scans, "jsonIntakeReadyCount"),
        textIntakeReadyCount = JsonInt(p213Scans, "textIntakeReadyCount"),
        rerunPromotionAllowed = promotionRerunAllowed,
        blockerMatrixReady,
        wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount"),
        higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount"),
        nonclaimFirewallReady,
    },
    conclusion = noFixableImplementationRoute
        ? "The independent source review did not find a remaining code-level fix. W/Z has a target-independent direct-bridge candidate route but it does not supply a promotable source lineage; after branch-local replay it may fail sibling stability in addition to theorem promotion, raw-gate closure, and particle-specific rows. Higgs lacks a solved scalar-source lineage. The checked public GU draft passages also do not supply the missing source lineages. New Phase201/P209 source evidence is required before promotion can rerun."
        : "At least one upstream review signal changed; inspect this phase before relying on the prior no-fixable-route conclusion.",
    nextRequiredWork = new[]
    {
        "W/Z: provide a derivation-backed direct bridge-source theorem with separate W and Z source rows, raw-amplitude gates, common bridge gates, stability sidecars, and post-construction target comparison.",
        "Higgs: provide a solved target-independent scalar source/operator with identity envelope, massive profile, potential or excitation relation, stability sidecars, and a prediction row.",
        "Do not fill Phase201 templates with target-implied, external electroweak, CODATA/Fermi, observed Higgs, or diagnostic shortcut evidence.",
    },
    sourceEvidence = new
    {
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase194Path = Phase194Path,
        phase199Path = Phase199Path,
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase208Path = Phase208Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
        phase216Path = Phase216Path,
        phase218Path = Phase218Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_independent_source_review.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_independent_source_review_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.independentReviewMaterialized,
        result.noFixableImplementationRoute,
        result.promotionRerunAllowed,
        result.repoSourceScanFoundIntakeReadyEvidence,
        result.localRoutesExhausted,
        result.draftProvidesCompletionSource,
        result.officialPublicSourceAuditMaterialized,
        result.officialDraftProvidesCompletionSource,
        result.evidenceReadiness,
        result.conclusion,
        result.nextRequiredWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"noFixableImplementationRoute={noFixableImplementationRoute}");
Console.WriteLine($"promotionRerunAllowed={promotionRerunAllowed}");
Console.WriteLine($"repoSourceScanFoundIntakeReadyEvidence={repoSourceScanFoundIntakeReadyEvidence}");

static JsonElement RequiredObject(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Object
        ? property
        : throw new InvalidOperationException($"Missing object property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static bool? JsonNestedBool(JsonElement element, string objectName, string propertyName) =>
    element.TryGetProperty(objectName, out var obj) && obj.ValueKind == JsonValueKind.Object
        ? JsonBool(obj, propertyName)
        : null;
