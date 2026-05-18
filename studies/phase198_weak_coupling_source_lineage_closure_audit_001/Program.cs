using System.Text.Json;

const string DefaultOutputDir = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output";
const string Phase68Path = "studies/phase68_normalized_weak_coupling_candidate_promotion_001/normalized_weak_coupling_candidate_promotion.json";
const string Phase77Path = "studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/raw_matrix_element_evidence_gate.json";
const string Phase114Path = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/wz_route_replayed_matrix_element_evidence.json";
const string Phase115Path = "studies/phase115_wz_route_fermion_quality_replay_001/output/wz_route_fermion_quality_replay.json";
const string Phase116Path = "studies/phase116_wz_absolute_projection_rerun_001/output/wz_absolute_projection_rerun.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE198_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase68 = JsonDocument.Parse(File.ReadAllText(Phase68Path));
using var phase77 = JsonDocument.Parse(File.ReadAllText(Phase77Path));
using var phase114 = JsonDocument.Parse(File.ReadAllText(Phase114Path));
using var phase115 = JsonDocument.Parse(File.ReadAllText(Phase115Path));
using var phase116 = JsonDocument.Parse(File.ReadAllText(Phase116Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));

var fixtureCandidate = phase68.RootElement.GetProperty("candidate");
var fixtureWeakCoupling = JsonDouble(fixtureCandidate, "couplingValue");
var fixtureProvenanceId = JsonString(fixtureCandidate, "provenanceId");
var fixtureRejectedByEvidenceGate = string.Equals(
    JsonString(phase77.RootElement, "terminalStatus"),
    "raw-weak-coupling-matrix-element-evidence-blocked",
    StringComparison.Ordinal);
var fixtureRawMagnitude = phase77.RootElement.TryGetProperty("evidence", out var phase77Evidence)
    ? JsonDouble(phase77Evidence, "rawMatrixElementMagnitude")
    : null;
var fixtureSourceKind = phase77.RootElement.TryGetProperty("evidence", out phase77Evidence)
    ? JsonString(phase77Evidence, "sourceKind")
    : null;

var replayRecords = phase114.RootElement.GetProperty("records")
    .EnumerateArray()
    .Select(record => new
    {
        particleId = JsonString(record, "particleId"),
        rawMatrixElementMagnitude = JsonDouble(record, "rawMatrixElementMagnitude"),
        normalizedWeakCoupling = JsonDouble(record, "normalizedWeakCoupling"),
        rawEvidenceValidated = JsonBool(record, "rawEvidenceValidated"),
    })
    .ToArray();

var replayReady = JsonBool(phase115.RootElement, "repairAccepted") is true;
var replayComparisonPassed = JsonBool(phase116.RootElement, "comparisonPassed") is true;
var replaySharedWeakCoupling = phase116.RootElement.TryGetProperty("sharedWeakCouplingInput", out var sharedWeakCoupling)
    ? JsonDouble(sharedWeakCoupling, "normalizedWeakCoupling")
    : null;
var replayWeakCouplingToTargetRatio = phase116.RootElement.TryGetProperty("sharedWeakCouplingInput", out sharedWeakCoupling)
    ? JsonDouble(sharedWeakCoupling, "weakCouplingToTargetRatio")
    : null;
var repairedBridgeRelativeSpread = phase116.RootElement.TryGetProperty("zRouteConsistencyDiagnostic", out var zRoute)
    ? JsonDouble(zRoute, "repairedBridgeRelativeSpread")
    : null;
var repairedBridgeCommonScalePassed = phase116.RootElement.TryGetProperty("zRouteConsistencyDiagnostic", out zRoute)
    ? JsonBool(zRoute, "commonBridgePassed")
    : null;

var p197WeakCouplingClosurePassed = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var targetImpliedWeakCoupling = JsonDouble(phase197.RootElement, "targetImpliedWeakCoupling");

var sourceLineages = new[]
{
    new SourceLineage(
        "phase65-fixture-promoted-through-phase68",
        "superseded-not-promotable-for-physical-wz",
        fixtureWeakCoupling,
        fixtureRawMagnitude,
        "Phase68 accepted the Phase65 scalar fixture under the older weak-coupling input audit, but Phase77 blocks that raw value because it is a scalar study input rather than replayed analytic matrix-element evidence.",
        new[]
        {
            $"phase68ProvenanceId={fixtureProvenanceId}",
            $"phase77SourceKind={fixtureSourceKind}",
            $"phase77EvidenceGateBlocked={fixtureRejectedByEvidenceGate}",
        }),
    new SourceLineage(
        "wz-route-replayed-analytic-matrix-element",
        replayReady && replayComparisonPassed && repairedBridgeCommonScalePassed is true
            ? "promotable"
            : "executed-but-failed",
        replaySharedWeakCoupling,
        replayRecords.FirstOrDefault(row => row.particleId == "w-boson")?.rawMatrixElementMagnitude,
        "The replayed W/Z-route matrix element path is the admissible evidence class, but the accepted replayed projection fails physical target comparison and does not define a common W/Z bridge scale.",
        new[]
        {
            $"phase115RepairAccepted={replayReady}",
            $"phase116ComparisonPassed={replayComparisonPassed}",
            $"phase116WeakCouplingToTargetRatio={replayWeakCouplingToTargetRatio}",
            $"phase116RepairedBridgeRelativeSpread={repairedBridgeRelativeSpread}",
            $"phase116CommonBridgePassed={repairedBridgeCommonScalePassed}",
        }),
    new SourceLineage(
        "phase75-target-implied-weak-coupling",
        "diagnostic-only",
        targetImpliedWeakCoupling,
        null,
        "The target-implied weak coupling is numerically sufficient by construction, but P197 preserves it as diagnostic-only because it is inferred from W/Z physical targets.",
        new[]
        {
            $"phase197ClosurePassed={p197WeakCouplingClosurePassed}",
        }),
};

var canPromoteAnyWeakCouplingSourceForWzAbsolute = sourceLineages.Any(row => row.Status == "promotable");
var terminalStatus = canPromoteAnyWeakCouplingSourceForWzAbsolute
    ? "weak-coupling-source-lineage-closure-promotable"
    : "weak-coupling-source-lineage-closure-no-promotable-source";

var result = new
{
    phaseId = "phase198-weak-coupling-source-lineage-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteAnyWeakCouplingSourceForWzAbsolute,
    sourceLineages,
    replayRecords,
    decision = canPromoteAnyWeakCouplingSourceForWzAbsolute
        ? "A weak-coupling source lineage is promotable for W/Z absolute masses."
        : "Do not promote W/Z absolute masses from the current weak-coupling lineages. The older Phase68 value is superseded by the Phase77 evidence gate, the admissible replay lineage fails target comparison/common-scale gates, and the numerically successful value is target-implied only.",
    nextRequiredArtifact = "A target-independent raw matrix-element or scalar-sector amplitude source that is replayed analytic evidence, clears the W/Z common-bridge gate, and passes absolute W/Z target comparison without using target-implied coupling.",
    sourceEvidence = new
    {
        phase68Path = Phase68Path,
        phase77Path = Phase77Path,
        phase114Path = Phase114Path,
        phase115Path = Phase115Path,
        phase116Path = Phase116Path,
        phase197Path = Phase197Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "weak_coupling_source_lineage_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "weak_coupling_source_lineage_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteAnyWeakCouplingSourceForWzAbsolute,
        result.sourceLineages,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteAnyWeakCouplingSourceForWzAbsolute={canPromoteAnyWeakCouplingSourceForWzAbsolute}");
Console.WriteLine($"fixtureWeakCoupling={fixtureWeakCoupling}");
Console.WriteLine($"targetImpliedWeakCoupling={targetImpliedWeakCoupling}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record SourceLineage(
    string LineageId,
    string Status,
    double? NormalizedWeakCoupling,
    double? RawMatrixElementMagnitude,
    string Finding,
    IReadOnlyList<string> Evidence);
