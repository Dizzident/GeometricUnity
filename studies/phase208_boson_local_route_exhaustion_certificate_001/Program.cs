using System.Text.Json;

const string DefaultOutputDir = "studies/phase208_boson_local_route_exhaustion_certificate_001/output";
const string Phase192Path = "studies/phase192_boson_scientific_defensibility_ledger_001/output/boson_scientific_defensibility_ledger_summary.json";
const string Phase193Path = "studies/phase193_boson_prediction_completion_audit_001/output/boson_prediction_completion_audit_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase206Path = "studies/phase206_direct_bridge_normalization_closure_001/output/direct_bridge_normalization_closure_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE208_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase192 = JsonDocument.Parse(File.ReadAllText(Phase192Path));
using var phase193 = JsonDocument.Parse(File.ReadAllText(Phase193Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase206 = JsonDocument.Parse(File.ReadAllText(Phase206Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));

var allKnownBosonValuesDefensible = JsonBool(phase192.RootElement, "allKnownBosonValuesDefensible") is true;
var allSuccessCriteriaMet = JsonBool(phase193.RootElement, "allSuccessCriteriaMet") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var jsonIntakeReady = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? 0;
var textIntakeReady = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? 0;
var directBridgeNormalizationPromotable = JsonBool(phase206.RootElement, "canPromoteDirectBridgeNormalization") is true;
var higgsQuarticSelfCouplingPromotable = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;

var routeFamilies = new[]
{
    new RouteFamily(
        "gate-promoted-known-boson-ledger",
        allKnownBosonValuesDefensible ? "open-promotable" : "exhausted-current-artifacts",
        $"allKnownBosonValuesDefensible={allKnownBosonValuesDefensible}; defensibleValueCount={JsonInt(phase192.RootElement, "defensibleValueCount")}; failedAttemptCount={JsonInt(phase192.RootElement, "failedAttemptCount")}; blockedCount={JsonInt(phase192.RootElement, "blockedCount")}",
        Phase192Path),
    new RouteFamily(
        "completion-audit-routes",
        allSuccessCriteriaMet ? "open-promotable" : "exhausted-current-artifacts",
        $"allSuccessCriteriaMet={allSuccessCriteriaMet}; unresolvedItemCount={phase193.RootElement.GetProperty("unresolvedItems").GetArrayLength()}",
        Phase193Path),
    new RouteFamily(
        "phase201-source-lineage-intakes",
        allRequiredLineagesPromotable ? "open-promotable" : "awaiting-new-artifacts",
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; terminalStatus={JsonString(phase201.RootElement, "terminalStatus")}",
        Phase201Path),
    new RouteFamily(
        "json-artifact-source-lineage-scan",
        jsonIntakeReady > 0 ? "open-review-required" : "exhausted-current-artifacts",
        $"scannedJsonFileCount={JsonInt(phase204.RootElement, "scannedJsonFileCount")}; candidateCount={JsonInt(phase204.RootElement, "candidateCount")}; intakeReadyCandidateCount={jsonIntakeReady}",
        Phase204Path),
    new RouteFamily(
        "text-source-lineage-evidence-scan",
        textIntakeReady > 0 ? "open-review-required" : "exhausted-current-artifacts",
        $"scannedTextFileCount={JsonInt(phase205.RootElement, "scannedTextFileCount")}; intakeReadyFindingCount={textIntakeReady}",
        Phase205Path),
    new RouteFamily(
        "wz-direct-bridge-normalization-route",
        directBridgeNormalizationPromotable ? "open-promotable" : "exhausted-current-artifacts",
        $"canPromoteDirectBridgeNormalization={directBridgeNormalizationPromotable}; decision={JsonString(phase206.RootElement, "decision")}",
        Phase206Path),
    new RouteFamily(
        "higgs-quartic-self-coupling-source-route",
        higgsQuarticSelfCouplingPromotable ? "open-promotable" : "exhausted-current-artifacts",
        $"canPromoteHiggsQuarticSelfCouplingSource={higgsQuarticSelfCouplingPromotable}; candidateFindingCount={JsonInt(phase207.RootElement, "candidateFindingCount")}; intakeReadyFindingCount={JsonInt(phase207.RootElement, "intakeReadyFindingCount")}",
        Phase207Path),
};

var anyCurrentLocalRouteActionable = routeFamilies.Any(route => route.Status is "open-promotable" or "open-review-required");
var sourceLineageContractsStillNeeded = !allRequiredLineagesPromotable;
var localRouteExhaustionCertified = !allKnownBosonValuesDefensible
    && !allSuccessCriteriaMet
    && !anyCurrentLocalRouteActionable
    && sourceLineageContractsStillNeeded;

var terminalStatus = localRouteExhaustionCertified
    ? "boson-local-route-exhaustion-certified-new-source-required"
    : anyCurrentLocalRouteActionable
        ? "boson-local-route-exhaustion-open-route-present"
        : "boson-local-route-exhaustion-indeterminate";

var result = new
{
    phaseId = "phase208-boson-local-route-exhaustion-certificate",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    localRouteExhaustionCertified,
    anyCurrentLocalRouteActionable,
    allKnownBosonValuesDefensible,
    allSuccessCriteriaMet,
    sourceLineageContractsStillNeeded,
    routeFamilies,
    conclusion = localRouteExhaustionCertified
        ? "Current local artifacts have no actionable route to promote the remaining W/Z absolute or Higgs values. New source-lineage artifacts are required; the current defensible boson values remain partial."
        : anyCurrentLocalRouteActionable
            ? "At least one local route may still be actionable and requires review before declaring local route exhaustion."
            : "Route exhaustion could not be certified from the current inputs.",
    nextRequiredArtifacts = new[]
    {
        "W/Z: derivation-backed direct bridge/source theorem with separate W and Z rows and raw/common-scale/target-comparison gates.",
        "Higgs: solved scalar-sector source/operator with target-independent identity, massive profile, potential/self-coupling or excitation relation, and stability sidecars.",
    },
    sourceEvidence = new
    {
        phase192Path = Phase192Path,
        phase193Path = Phase193Path,
        phase201Path = Phase201Path,
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase206Path = Phase206Path,
        phase207Path = Phase207Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_local_route_exhaustion_certificate.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_local_route_exhaustion_certificate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.localRouteExhaustionCertified,
        result.anyCurrentLocalRouteActionable,
        result.allKnownBosonValuesDefensible,
        result.allSuccessCriteriaMet,
        result.sourceLineageContractsStillNeeded,
        result.routeFamilies,
        result.conclusion,
        result.nextRequiredArtifacts,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"localRouteExhaustionCertified={localRouteExhaustionCertified}");
Console.WriteLine($"anyCurrentLocalRouteActionable={anyCurrentLocalRouteActionable}");
Console.WriteLine($"routeFamilyCount={routeFamilies.Length}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record RouteFamily(string RouteFamilyId, string Status, string Evidence, string EvidencePath);
